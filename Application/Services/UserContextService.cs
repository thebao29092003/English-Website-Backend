using Azure;
using English.Website.Api.Dtos.UserDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace English.Website.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EnglishDBContext _dbContext;

        public UserContextService(IHttpContextAccessor httpContextAccessor, EnglishDBContext dBContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dBContext;
        }

        public async Task<UserContextDtos> GetUserDetail()
        {
            /*
             Cách hoạt động: IHttpContextAccessor giúp bạn tiếp cận phiên làm việc HTTP hiện tại. Hệ thống sẽ đi vào User (đối tượng đại diện cho người dùng đã đăng nhập thành công qua JWT).
             Hàm FindFirstValue("UserId") tìm kiếm trong Token xem có claim (thông tin đi kèm) nào tên là "UserId" không và lấy ra giá trị dạng chuỗi (string) của nó.
             */
            string userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserId")!;

            // Nếu ID là kiểu Guid
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                throw new BadRequestException("Invalid UserId format");
            }

            var userDetail = await _dbContext.User
                //.Include(u => u.Subscription)
                .Where(u => u.UserId == userGuid)
                .Select(u => new UserContextDtos
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();
            if (userDetail == null) {
                throw new BadRequestException("UserDetail in UserContextService is null");
            }
            return userDetail;
        }
    }
}
