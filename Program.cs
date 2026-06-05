using English.Website.Api.Extensions;
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
                    context.Fail("Token không hợp lệ (Thiếu thông tin nhận diện).");
                    return;
                }

                string cacheKey = $"security-stamp:{userIdClaim}";

                // Vì mỗi request đều phải kiểm tra bước này, nếu request nào cũng gọi Database (DB) thì server sẽ rất chậm.
                // Do đó, code sẽ ưu tiên kiểm tra trong RAM (MemoryCache) trước
                if (!memoryCache.TryGetValue(cacheKey, out string? validStamp))
                {
                    // 2. CACHE MISS: Nếu RAM chưa lưu, truy vấn database để lấy Stamp mới nhất
                    var userId = Guid.Parse(userIdClaim);
                    var user = await dbContext.Users
                        .AsNoTracking() // Tối ưu truy vấn nhanh không cần tracking
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user == null)
                    {
                        context.Fail("Người dùng không tồn tại.");
                        return;
                    }

                    validStamp = user.SecurityStamp;

                    // 3. LƯU VÀO RAM TRONG 10 PHÚT: Để các request sau không phải gọi DB nữa
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                    memoryCache.Set(cacheKey, validStamp, cacheEntryOptions);
                }

                // 4.SO SÁNH: Nếu Stamp trong Token lệch với Stamp hợp lệ->Chặn đứng ngay
                if (tokenStamp != validStamp)
                {
                    context.Fail("Phiên đăng nhập đã bị vô hiệu hóa (Người dùng đã đăng xuất hoặc đổi mật khẩu).");
                }
            }
        };
    });

var app = builder.Build();

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
