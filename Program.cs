using English.Website.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
