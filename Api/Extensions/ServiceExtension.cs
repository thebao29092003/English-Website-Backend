
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using English.Website.Domain.Cores.Exceptions;
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
            services.AddScoped<IUserContextService, UserContextService>();

            // 3. Đăng ký AutoMapper
            // cfg => { } để viết riêng map thôi mình có file riêng rồi nên không cần
            services.AddAutoMapper(cfg => { },typeof(MappingProfiles));

            // 4. Đăng ký MEMORY CACHE (RAM) CỦA .NET
            services.AddMemoryCache();

            // 👇 ĐĂNG KÝ BỘ XỬ LÝ LỖI TOÀN CỤC CỦA .NET 8
            services.AddExceptionHandler<GlobalExceptionHandler>();

            /* 
            "Nếu lập trình viên có viết bộ xử lý lỗi riêng (GlobalExceptionHandler), 
            tôi sẽ gọi nó. Nhưng lỡ như bộ xử lý lỗi của họ bị lỗi tiếp,
            hoặc họ không xử lý lỗi này, thì tôi phải trả về lỗi cho Client dưới định dạng nào?"
            thì dòng phía dưới khai báo chuẩn cấu hình chuẩn RFC 7807 để trả lỗi
            */
            services.AddProblemDetails(); 

            // 👇 ĐĂNG KÝ CẦU NỐI ĐỂ SERVICE CÓ THỂ ĐỌC/GHI COOKIE
            services.AddHttpContextAccessor();

            services.AddHttpClient<IDeepSeekService, DeepSeekService>();

        }
    }
}
