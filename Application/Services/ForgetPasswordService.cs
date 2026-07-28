using English.Website.Api.Dtos.AuthDtos;
using English.Website.Api.Extensions.Helpers;
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
        private readonly IMemoryCache _memoryCache;

        public ForgetPasswordService(EnglishDBContext context, IEmailService emailService, IMemoryCache memoryCache)
        {
            _context = context;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }

        public async Task SendResetPasswordOtp(string email)
        {
            // 1. Kiểm tra xem Email này có tồn tại trong hệ thống chưa
            var user = 
                await _context.User.FirstOrDefaultAsync(u => u.Username == email) 
                ?? throw new BadRequestException("Email not exist.");

            if(!user.IsActive)
            {
                throw new BadRequestException("Account is blocked. Plase contact admin via email");
            }

            // 2. Sinh mã OTP 6 số
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // 3. Lưu vào RAM trong vòng 5 phút
            string cacheKey = $"reset-otp:{email}";
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // 4. Gửi email khôi phục mật khẩu
            string subject = "Yêu cầu khôi phục mật khẩu - Engsteps";
            string title = "Khôi Phục Mật Khẩu Tài Khoản";
            string content = $@"
                <p style=""color: #cbd5e1; font-size: 15px; margin-bottom: 20px;"">
                    Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>Engsteps</strong> của bạn. Dưới đây là mã OTP xác nhận:
                </p>
                <div style=""text-align: center; margin: 28px 0;"">
                    <span style=""display: inline-block; background: linear-gradient(135deg, #3b82f6 0%, #8b5cf6 50%, #d946ef 100%); color: #ffffff; font-size: 32px; font-weight: 800; letter-spacing: 6px; padding: 14px 28px; border-radius: 12px; box-shadow: 0 4px 20px rgba(139, 92, 246, 0.4);"">
                        {otp}
                    </span>
                </div>
                <p style=""color: #94a3b8; font-size: 13px; text-align: center; margin: 0;"">
                    Mã này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng tuyệt đối không chia sẻ mã này với bất kỳ ai.
                </p>";

            await _emailService.SendTemplatedEmailAsync(email, subject, title, content);
        }

        public async Task ResetPasswordWithOtp(ResetPasswordRequestDto dto)
        {
            // 1. Kiểm tra OTP trong RAM
            string cacheKey = $"reset-otp:{dto.Email}";
            if (!_memoryCache.TryGetValue(cacheKey, out string? validOtp) || validOtp != dto.Otp)
            {
                throw new BadRequestException("Invalid or expired OTP.");
            }

            // 2. Tìm người dùng
            var user = 
                await _context.User.FirstOrDefaultAsync(u => u.Username == dto.Email) 
                ?? throw new BadRequestException("User not exist.");
            
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
        }
    }
}
