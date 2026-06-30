using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace English.Website.Api.Controllers
{

    [Route("api/assembly")]
    [ApiController]
    public class AssemblyAIController : ControllerBase
    {
        private readonly string _webhookAuth;
        private readonly string _emailAdmin;
        private readonly IAssemblyAIService _assemblyAIService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEmailService _emailService;

        public AssemblyAIController(
            IConfiguration configuration,
            IAssemblyAIService assemblyAIService,
            IServiceScopeFactory serviceScopeFactory,
            IEmailService emailService
        )
        {
            _webhookAuth = configuration["AI:AssemblyAIKey"]!;
            _emailAdmin = configuration["AdminSettings:Email"]!;
            _assemblyAIService = assemblyAIService;
            _serviceScopeFactory = serviceScopeFactory;
            _emailService = emailService;
        }

        // Này là endpoint trả về cho AssemblyAI nên nó khác những endpoint kia
        // những webhook không được gọi _useUserContextService để lấy userId vì nó ko có token của user
        [HttpPost("webhook")]
        public async Task<IActionResult> AssemblyAIWebhook([FromBody] AssemblyAiWebhookDto webhookData)
        {
            if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var receivedSecret) ||
                receivedSecret != _webhookAuth)
            {
                return Unauthorized(new { message = "Unauthorized webhook request." });
            }

            if (webhookData.Status == "completed")
            {
                try
                {
                    _ = Task.Run(async () =>
                        {
                            // Tạo một Scope mới độc lập với vòng đời của HTTP Request
                            using var scope = _serviceScopeFactory.CreateScope();

                            // Lấy các Service cần thiết từ Scope mới này
                            // Các dịch vụ này sẽ không bị giải phóng khi Controller trả về kết quả
                            var assemblyAIService = scope.ServiceProvider.GetRequiredService<IAssemblyAIService>();

                            // Tiến hành tải dữ liệu, chấm điểm và gọi DeepSeek song song dưới nền
                            await assemblyAIService.GetDataAssemblyAI(webhookData.TranscriptId);
                        }
                    );
                }
                catch (Exception ex)
                {
                    string subject = "🚨 CẢNH BÁO LỖI HỆ THỐNG 500 - English Website";
                    // Ghi log lỗi của tác vụ nền để dễ dàng debug khi có sự cố
                    Console.WriteLine($"Error in background task for transcript {webhookData.TranscriptId}: {ex.Message}");
                    await _emailService.SendEmailAsync(_emailAdmin!, subject, $"Error in background task for transcript {webhookData.TranscriptId}: {ex.Message}");
                }
            }
            return StatusCode(200);
        }

    }
}
