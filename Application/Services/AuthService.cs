using English.Website.Api.Dtos.AuthDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace English.Website.Application.Services
{

    public class AuthService
    {
        private readonly EnglishDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserContextService _useContext;

        public AuthService(
            EnglishDBContext context,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IUserContextService useContext
        )
        {
            _context = context;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _useContext = useContext;
        }

        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new BadRequestException("No HttpContext available");
            }
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // 👈 Bảo vệ khỏi XSS (React không đọc được)
                Secure = true,   // 👈 Chỉ gửi qua HTTPS (khi deploy thật)
                SameSite = SameSiteMode.Lax, // 👈 Chống tấn công CSRF
                Expires = DateTime.UtcNow.AddDays(7) // Khớp với hạn của RefreshToken
            };

            // Ghi cookie tên là "refreshToken" vào trình duyệt của client
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("Email", user.Username),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Role", user.Role),
                new Claim("SecurityStamp", user.SecurityStamp)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:SecretKey")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private async Task<User> ValidateRefreshToken(string refreshToken)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow
            )
            {
                throw new BadRequestException("Invalid refresh token");
            }
            return user;
        }

        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];

            // using dùng để giải phóng tài nguyên sau khi sử dụng xong, ở đây là đối tượng RandomNumberGenerator
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);

        }

        private async Task<string> SaveRefreshToken(User user)
        {
            var refreshToken = CreateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task SendRegisterOtp(string toEmail)
        {
            var isExistingUser = await _context.User.AnyAsync(u => u.Username == toEmail);
            if (isExistingUser)
            {
                throw new BadRequestException("Account already exists.");
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // 3. Lưu vào RAM trong vòng 5 phút
            string cacheKey = $"reg-otp:{toEmail}";
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(10));

            string subject = "Mã xác nhận đăng ký tài khoản - English Website";
            string body = $"<h3>Chào mừng bạn đến với English Website!</h3>" +
                     $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>" +
                     $"<p>Mã này có hiệu lực trong vòng 5 phút. Vui lòng tuyệt đối không chia sẻ mã này với bất kỳ ai.</p>";
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await SaveRefreshToken(user)
            };
        }

        public async Task Register(RegisterDto registerDto)
        {
            // 1. Kiểm tra OTP trong RAM
            string cacheKey = $"reg-otp:{registerDto.Username}";
            if (
                !_memoryCache.TryGetValue(cacheKey, out string? validOtp) ||
                validOtp != registerDto.Otp
                )
            {
                throw new BadRequestException("Otp invalid or expired");
            }

            // 2. Kiểm tra lại trùng lặp email đề phòng race condition
            var isExistingUser = await _context.User.AnyAsync(u => u.Username == registerDto.Username);
            if (isExistingUser)
            {
                throw new BadRequestException("Account already exists.");
            }

            User userNew = new User();
            var hashedPassword = new PasswordHasher<User>().HashPassword(userNew, registerDto.Password);

            userNew.Username = registerDto.Username;
            userNew.Password = hashedPassword;
            // 👇 TẠO STAMP MỚI KHI ĐĂNG KÝ
            userNew.SecurityStamp = Guid.NewGuid().ToString();

            // 👇 THIẾT LẬP THỜI GIAN ĐĂNG KÝ (Sử dụng giờ quốc tế UTC)
            userNew.CreatedAt = DateTime.UtcNow;
            userNew.LastLoginAt = null; // Tài khoản mới tinh chưa đăng nhập lần nào

            await _context.User.AddAsync(userNew);
            await _context.SaveChangesAsync();

            // 4. Xóa mã OTP khỏi RAM sau khi đăng ký thành công
            _memoryCache.Remove(cacheKey);
        }

        public async Task<TokenResponseDto> Login(UserDto userDto)
        {
            // đối với login không nên trả lỗi cụ thể dể tránh lộ thông tin về tài khoản, nên trả về lỗi chung chung như "Invalid username or password"
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == userDto.Username)
                ?? throw new BadRequestException("Invalid username or password");

            var passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, userDto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Invalid username or password");
            }

            if (!user.IsActive)
            {
                throw new BadRequestException("Account is blocked. Plase contact admin via email");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = await CreateTokenResponse(user);
            SetRefreshTokenInCookie(response.RefreshToken);

            return response;
        }

        public async Task<TokenResponseDto> RefreshToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HttpContext available");
            }

            var refreshToken = httpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Refresh token not found");
            }

            var user = await ValidateRefreshToken(refreshToken);

            if(!user.IsActive)
            {
                throw new BadRequestException("Account is blocked. Plase contact admin via email");
            }

            var response = await CreateTokenResponse(user);

            SetRefreshTokenInCookie(response.RefreshToken);

            return response;
        }

        public async Task Logout()
        {

            var userFromToken = await _useContext.GetUserDetail();
            var userId = userFromToken.UserId.ToString();

            // FindAsync bắt buộc trùng kiểu dữ liệu với khóa chính
            var user = await _context.User.FindAsync(Guid.Parse(userId))
                ?? throw new BadRequestException("User not found");

            // 1. Thay đổi SecurityStamp trong DB -> Toàn bộ Access Token cũ sẽ bị vô hiệu hóa
            user.SecurityStamp = Guid.NewGuid().ToString();

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            string cacheKey = $"security-stamp:{userId.ToString().ToLowerInvariant()}";
            _memoryCache.Remove(cacheKey);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserStatusAsync(string userId)
        {
            var user = await _context.User.FindAsync(Guid.Parse(userId));
            if (user == null) throw new BadRequestException("User not found");

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            // 👇 BƯỚC QUAN TRỌNG: XOÁ CACHE ĐỂ ĐỒNG BỘ TRẠNG THÁI KHÓA NGAY LẬP TỨC
            // Khi xóa key này, request tiếp theo của user đó gửi lên sẽ bị ép truy vấn DB và bị chặn lại.
            string cacheKey = $"user-active:{userId.ToString().ToLowerInvariant()}";
             _memoryCache.Remove(cacheKey);
        }
    }
}
