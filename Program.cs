using English.Website.Api.Extensions;
using English.Website.Api.Hubs;
using English.Website.Domain.DatabaseContext;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

// Đọc Docker Secrets từ thư mục /run/secrets (nếu ứng dụng chạy trong Docker với Docker Secrets)
configuration.AddKeyPerFile("/run/secrets", optional: true);

// register services + automapper
ServiceExtension.AddServices(services, configuration);

// Logs (Nhật ký sự kiện): Các dòng chữ ghi lại sự kiện 
builder.Host.UseSerilog();

var app = builder.Build();

// 👇 TỰ ĐỘNG KHỞI TẠO DATABASE & MIGRATION
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishDBContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi khởi tạo database: {ex.Message}");
    }
}

// 👇 KÍCH HOẠT MIDDLEWARE BẮT LỖI TOÀN CỤC (Phải đặt ở dòng đầu tiên của Pipeline)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseHangfireDashboard();

app.MapControllers();

app.MapHub<AudioProcessingHub>("/hubs/audio-processing");

app.MapHealthChecks("/health")
    .AllowAnonymous()
    .DisableRateLimiting();
app.Run();
