using English.Website.Api.Extensions;
using Hangfire;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// register services + automapper
ServiceExtension.AddServices(services, configuration);

// Logs (Nhật ký sự kiện): Các dòng chữ ghi lại sự kiện 
builder.Host.UseSerilog();

var app = builder.Build();

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

app.UseHangfireDashboard();

app.MapControllers();

app.Run();
