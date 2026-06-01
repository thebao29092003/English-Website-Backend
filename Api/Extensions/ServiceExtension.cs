
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
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
            services.AddScoped<AudioService>();
            //services.AddScoped<RecordingService>();
            //services.AddScoped<ScoreService>();

            // 3. Đăng ký AutoMapper
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
        }
    }
}