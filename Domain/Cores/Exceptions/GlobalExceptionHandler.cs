using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Diagnostics;
using englishWebSite.API.APIPayload;

/*
IExceptionHandler bắt buộc phải là Singleton (vì nó chạy ở tầng Middleware ngoài cùng).
IEmailService của bạn đang được đăng ký là Scoped (services.AddScoped<IEmailService, EmailService>();).
Quy tắc của .NET: Một dịch vụ Singleton không bao giờ được phép "chứa" (inject) một dịch vụ Scoped, 
vì Scoped bị hủy sau mỗi Request, trong khi Singleton thì sống mãi. 
Điều này gây ra lỗi tranh chấp tài nguyên [7].
 
Cách khắc phục: Sử dụng IServiceScopeFactory (Cách chuẩn)
Trong GlobalExceptionHandler, chúng ta không inject thẳng IEmailService vào constructor nữa, 
mà inject IServiceScopeFactory để tự tay "mở" một phạm vi (scope) tạm thời khi cần gửi mail.
 */
namespace English.Website.Domain.Cores.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            // Lấy thông tin request để bổ sung vào log
            var request = httpContext.Request;
            var requestPath = $"{request.Method} {request.Path}{request.QueryString}";

            // 1. Ghi log lỗi vào hệ thống terminal khi mà chạy chương trình
            _logger.LogError
                (exception, 
                "An unhandled exception occurred while processing request: {RequestPath}. Message: {Message}",
                requestPath, exception.Message);

            int statusCode;
            string message;

            // cú pháp trong if kết hợp 2 bước 1 là type check xem có phải BadRequestException
            // nếu đúng thì ép kiểu exception sang badRequestException và gán vào một biến mới tên là badRequestException
            if (exception is BadRequestException badRequestException)
            {
                // LỖI 400: Lỗi do bạn chủ động throw từ Service (ví dụ: Trùng Email, Sai mã OTP...)
                statusCode = StatusCodes.Status400BadRequest;
                message = badRequestException.Message;
            }
            else
            {
                // LỖI 500: Lỗi hệ thống ngoài ý muốn (sập database, lỗi code, lỗi null pointer...)
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Important Error";

                // 👇 GỬI EMAIL CẢNH BÁO CHO BẠN (ADMIN) KHI SẬP NGUỒN 500
                try
                {
                    await SendErrorEmailToAdminAsync(exception, httpContext);
                }
                catch (Exception emailEx)
                {
                    // Tránh việc lỗi gửi mail làm sập tiếp luồng xử lý chính
                    _logger.LogError(emailEx, "Not email admin");
                }
            }

            // 3. ĐÓNG GÓI PHẢN HỒI JSON CHUẨN (APIResponseBase) TRẢ VỀ CHO FRONTEND
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var response = new APIResponseBase
            {
                Success = false,
                EndPointCode = "system.error",
                Status = statusCode,
                Value = null,
                Message = message
            };

            /* 
            Tại sao cần truyền vào CancellationToken? Khi người dùng hủy request,
            CancellationToken của request đó sẽ tự động chuyển sang trạng thái "Hủy" (Cancelled).
            Khi ta truyền cancellationToken vào hàm WriteAsJsonAsync,
            .NET sẽ kiểm tra và lập tức ngừng việc ghi dữ liệu ra mạng (vì client đóng kết nối rồi, có ghi ra cũng không ai nhận).
            Việc này giúp Server tiết kiệm tài nguyên CPU, RAM và băng thông ngay lập tức.
            */
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true; // Trả về true để báo hiệu .NET đã xử lý xong lỗi này [6]
        }

        private async Task SendErrorEmailToAdminAsync(Exception exception, HttpContext? context)
        {
            var adminEmail = _configuration["AdminSettings:Email"]; // Đọc email nhận của bạn từ cấu hình

            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();


            string subject = "🚨 CẢNH BÁO LỖI HỆ THỐNG 500 - Engsteps";
            string title = "Phát Hiện Lỗi Sập Nguồn Hệ Thống (HTTP 500)";

            string content = $@"
                <div style=""background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); border-radius: 12px; padding: 16px; margin-bottom: 20px;"">
                    <p style=""color: #fca5a5; font-size: 14px; margin: 0 0 8px 0;""><strong>Thời gian xảy ra (UTC):</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}</p>
                    <p style=""color: #fca5a5; font-size: 14px; margin: 0 0 8px 0;""><strong>API Endpoint bị lỗi:</strong> <span style=""background: rgba(255,255,255,0.1); padding: 3px 8px; border-radius: 6px; font-family: monospace;"">{context?.Request.Method} {context?.Request.Path}</span></p>
                    <p style=""color: #ef4444; font-size: 15px; font-weight: bold; margin: 0;""><strong>Nội dung lỗi:</strong> {exception.Message}</p>
                </div>
                <p style=""color: #e2e8f0; font-size: 14px; font-weight: 600; margin-bottom: 8px;"">Chi tiết Stack Trace (Dòng code bị lỗi):</p>
                <pre style=""background: #060412; color: #f87171; padding: 16px; border-radius: 10px; border-left: 4px solid #ef4444; font-family: Consolas, Monaco, monospace; font-size: 12px; line-height: 1.5; overflow-x: auto; white-space: pre-wrap;"">{exception.StackTrace}</pre>";

            await emailService.SendTemplatedEmailAsync(adminEmail!, subject, title, content);
        }
    }

}
