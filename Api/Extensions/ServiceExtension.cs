
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Api.Extensions
{
    public static class ServiceExtension
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Đăng ký DbContext
            services.AddDbContext<EnglishDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("englistWebsite")));

            // 2. Đăng ký Database Services (Không dùng Interface)
            services.AddScoped<AuthService>();
            services.AddScoped<ForgetPasswordService>();
            services.AddScoped<IEmailService, EmailService>();

            // 3. Đăng ký AutoMapper
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            // 4. Đăng ký MEMORY CACHE (RAM) CỦA .NET
            services.AddMemoryCache();
        }
    }
}