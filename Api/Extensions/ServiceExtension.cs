
using CloudinaryDotNet;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Extend;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using English.Website.Domain.Cores.Exceptions;
using English.Website.Domain.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

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
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IAssemblyAIService, AssemblyAIService>();

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

            // register authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["AppSettings:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = configuration["AppSettings:Audience"],

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["AppSettings:SecretKey"]!)),

                        // THÊM DÒNG NÀY ĐỂ ĐỊNH NGHĨA LẠI KEY PHÂN QUYỀN TRONG JWT
                        // còn nếu không thêm bắt buộc phải dùng ClaimTypes.Role để phân quyền thì mới có thể dùng [Authorize(Roles = "Admin")]
                        RoleClaimType = "Role"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // Sự kiện OnTokenValidated chạy ngay sau khi token đã vượt qua các bước kiểm tra cơ bản ở trên
                        OnTokenValidated = async context =>
                        {
                            // Lấy các dịch vụ cần thiết từ DI Container của HTTP Context
                            var memoryCache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                            var dbContext = context.HttpContext.RequestServices.GetRequiredService<EnglishDBContext>();

                            // Lấy thông tin UserId và SecurityStamp được giải mã từ Token ra
                            var userIdClaim = context.Principal?.FindFirst("UserId")?.Value;
                            var tokenStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;

                            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(tokenStamp))
                            {
                                throw new BadRequestException("Invalid token");
                            }

                            string cacheKeyIsAcitve = $"user-active:{userIdClaim.ToString().ToLowerInvariant()}";
                            string cacheKeySecurityStamp = $"security-stamp:{userIdClaim.ToString().ToLowerInvariant()}";

                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(8)) // Hết hạn tuyệt đối sau 8 phút
                                .SetSlidingExpiration(TimeSpan.FromMinutes(3)); // Nếu user không hoạt động trong 3 phút thì xóa

                            if (!memoryCache.TryGetValue(cacheKeyIsAcitve, out bool isActive))
                            {
                                // 3. Nếu RAM chưa lưu (Cache Miss), ta mới truy vấn Database
                                isActive = await dbContext.User
                                    .Where(u => u.UserId.ToString() == userIdClaim)
                                    .Select(u => u.IsActive)
                                    .FirstOrDefaultAsync();

                                memoryCache.Set(cacheKeyIsAcitve, isActive, cacheEntryOptions);
                            }

                            if (!isActive)
                            {
                                throw new BadRequestException("Account is blocked. Plase contact admin via email");
                            }

                            // Vì mỗi request đều phải kiểm tra bước này, nếu request nào cũng gọi Database (DB) thì server sẽ rất chậm.
                            // Do đó, code sẽ ưu tiên kiểm tra trong RAM (MemoryCache) trước
                            if (!memoryCache.TryGetValue(cacheKeySecurityStamp, out string? validStamp))
                            {
                                // 2. CACHE MISS: Nếu RAM chưa lưu, truy vấn database để lấy Stamp mới nhất
                                var userId = Guid.Parse(userIdClaim);
                                var user = await dbContext.User
                                    .AsNoTracking() // Tối ưu truy vấn nhanh không cần tracking
                                    .FirstOrDefaultAsync(u => u.UserId == userId);

                                if (user == null)
                                {
                                    throw new BadRequestException("User not found");
                                }

                                validStamp = user.SecurityStamp;

                                memoryCache.Set(cacheKeySecurityStamp, validStamp, cacheEntryOptions);
                            }

                            // 4.SO SÁNH: Nếu Stamp trong Token lệch với Stamp hợp lệ->Chặn đứng ngay
                            if (tokenStamp != validStamp)
                            {
                                throw new BadRequestException("User logout or invalid refresh token");
                            }
                        }
                    };
                });

            var apiSecret = configuration["Cloud:Cloudinary"];
            var CLOUDINARY_URL = $"cloudinary://347691969814999:{apiSecret}@dshd9jst0";
            // Khởi tạo và đăng ký Singleton cho Cloudinary
            services.AddSingleton(new Cloudinary(CLOUDINARY_URL));

            // Đăng ký Service xử lý upload của bạn
            services.AddScoped<ICloudinaryService, CloudinaryService>();
        }
    }
}
