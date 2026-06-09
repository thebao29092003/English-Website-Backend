using Azure;
using English.Website.Api.Dtos.UserDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
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

        // Đọc trường 'sub' hoặc 'NameIdentifier' trong JWT làm UserId


        public async Task<UserContextDtos> GetUserDetail()
        {
            string userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserId")!;
            var userDetail = await _dbContext.User
                //.Include(u => u.Subscription)
                .Where(u => u.UserId.ToString() == userId)
                .Select(u => new UserContextDtos
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    Role = u.Role
                })
                .FirstOrDefaultAsync();
            if (userDetail == null) {
                throw new BadRequestException("UserDetail in UserContextService is null");
            }
            return userDetail;
        }
    }
}
