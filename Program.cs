using English.Website.Api.Extensions;
using English.Website.Api.Extensions.Helpers;
using English.Website.Domain.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// register services + automapper
ServiceExtension.AddServices(services, configuration);

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
                    throw new BadRequestException("Invalid stamp");
                }
            }
        };
    });

var app = builder.Build();

// 👇 KÍCH HOẠT MIDDLEWARE BẮT LỖI TOÀN CỤC (Phải đặt ở dòng đầu tiên của Pipeline)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
