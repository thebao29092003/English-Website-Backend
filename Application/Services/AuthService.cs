using English.Website.Api.Dtos.AuthDtos;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        public AuthService(EnglishDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> Register(UserDto userDto)
        {
            var isExistingUser = await _context.Users.AnyAsync(u => u.Username == userDto.Username);
            if (isExistingUser)
            {
                return false;
            }

            User userNew = new User();
            var hashedPassword = new PasswordHasher<User>().HashPassword(userNew, userDto.Password);

            userNew.Username = userDto.Username;
            userNew.Password = hashedPassword;

            _context.Users.Add(userNew);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(bool, TokenResponseDto?)> Login(UserDto userDto)
        {
            // đối với login không nên trả lỗi cụ thể dể tránh lộ thông tin về tài khoản, nên trả về lỗi chung chung như "Invalid username or password"
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (user == null)
            {
                return (false, null);
            }

            var passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, userDto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return (false, null);
            }

            var response = await CreateTokenResponse(user);
            return (true, response);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("Email", user.Username),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Role", user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:SecretKey")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<TokenResponseDto?> RefreshToken(RefreshTokenRequestDto requestDto)
        {
            var user = await ValidateRefreshToken(requestDto);
            if (user == null)
            {
                return null;
            }
            TokenResponseDto response = await CreateTokenResponse(user);
            return response;
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await SaveRefreshToken(user)
            };
        }

        private async Task<User?> ValidateRefreshToken(RefreshTokenRequestDto requestDto)
        {
            var user = await _context.Users.FindAsync(requestDto.UserId);
            if (user == null ||
                user.RefreshToken != requestDto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow
            )
            {
                return null;
            }
            return user;
        }

        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<string> SaveRefreshToken(User user)
        {
            var refreshToken = CreateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;
        }
    }
}
