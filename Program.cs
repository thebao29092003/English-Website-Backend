using English.Website.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// register services + automapper
ServiceExtension.AddServices(services, configuration);


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
