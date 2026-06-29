using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace English.Website.Api.Controllers
{

    [Route("api/assembly")]
    [ApiController]
    public class AssemblyAIController : ControllerBase
    {
        private readonly string _webhookAuth;
        private readonly IAssemblyAIService _assemblyAIService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AssemblyAIController(
            IConfiguration configuration,
            IAssemblyAIService assemblyAIService,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _webhookAuth = configuration["AI:AssemblyAIKey"]!;
            _assemblyAIService = assemblyAIService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Này là endpoint trả về cho AssemblyAI nên nó khác những endpoint kia
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
            return StatusCode(200);
        }

    }
}
