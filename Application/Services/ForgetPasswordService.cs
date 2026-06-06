using English.Website.Api.Dtos.AuthDtos;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace English.Website.Application.Services
{
    public class ForgetPasswordService
    {
        private readonly EnglishDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        public ForgetPasswordService(EnglishDBContext context, IEmailService emailService, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        public async Task<(bool Success, string Message)> SendResetPasswordOtp(string email)
        {
            // 1. Kiểm tra xem Email này có tồn tại trong hệ thống chưa
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == email);
            if (user == null)
            {
                return (false, "Email not exist.");
            }

            // 2. Sinh mã OTP 6 số
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // 3. Lưu vào RAM trong vòng 5 phút
            string cacheKey = $"reset-otp:{email}";
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // 4. Gửi email khôi phục mật khẩu
            string subject = "Yêu cầu khôi phục mật khẩu - English Website";
            string body = $"<h3>Yêu cầu khôi phục mật khẩu tài khoản</h3>" +
                          $"<p>Mã OTP đặt lại mật khẩu của bạn là: <strong>{otp}</strong></p>" +
                          $"<p>Mã này có hiệu lực trong vòng 5 phút. Nếu không phải bạn yêu cầu, vui lòng đổi mật khẩu tài khoản ngay lập tức.</p>";

            await _emailService.SendEmailAsync(email, subject, body);

            return (true, "Mã khôi phục đã được gửi đến email của bạn.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordWithOtp(ResetPasswordRequestDto dto)
        {
            // 1. Kiểm tra OTP trong RAM
            string cacheKey = $"reset-otp:{dto.Email}";
            if (!_memoryCache.TryGetValue(cacheKey, out string? validOtp) || validOtp != dto.Otp)
            {
                return (false, "Invalid OTP or OTP has expired.");
            }

            // 2. Tìm người dùng
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Email);
            if (user == null)
            {
                return (false, "Email not exist.");
            }

            // 3. Đặt mật khẩu mới
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, dto.NewPassword);
            user.Password = hashedPassword;

            // 👇 ĐỔI SECURITY STAMP: Vô hiệu hóa ngay lập tức toàn bộ các phiên đăng nhập (Access Token) cũ của user này
            user.SecurityStamp = Guid.NewGuid().ToString();

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            // 4. Xóa cache OTP khỏi RAM
            _memoryCache.Remove(cacheKey);

            // Xóa luôn cache SecurityStamp của user này để bắt buộc nạp lại từ DB
            _memoryCache.Remove($"security-stamp:{user.UserId}");

            return (true, "Reset password successfully.");
        }
    }
}
