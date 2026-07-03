using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace English.Website.Api.Controllers
{

    [Route("api/assembly")]
    [ApiController]
    public class AssemblyAIController : ControllerBase
    {
        private readonly string _webhookAuth;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AssemblyAIController(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _webhookAuth = configuration["WebHook:AssemblyAI:Token"]!;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Này là endpoint trả về cho AssemblyAI nên nó khác những endpoint kia
        // những webhook không được gọi _useUserContextService để lấy userId vì nó ko có token của user
        // task chạy ngầm nên dùng globalExceptionHandler không thể bắt được lỗi nên phải dùng try catch
        [HttpPost("webhook")]
        public async Task<IActionResult> AssemblyAIWebhook([FromBody] AssemblyAiWebhookDto webhookData)
        {
            if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var receivedSecret) ||
                receivedSecret != _webhookAuth)
            {
                return StatusCode(500);
            }

            if (webhookData.Status == "completed")
            {
                // _: thể hiện rằng không cần lấy kết quả trả về của nó
                _ = Task.Run(async () =>
                    {
                        // Tạo một Scope mới độc lập với vòng đời của HTTP Request
                        using var scope = _serviceScopeFactory.CreateScope();

                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AssemblyAIController>>();

                        // Lấy các Service cần thiết từ Scope mới này
                        // Các dịch vụ này sẽ không bị giải phóng khi Controller trả về kết quả
                        var assemblyAIService = scope.ServiceProvider.GetRequiredService<IAssemblyAIService>();

                        try
                        {
                            // Tiến hành tải dữ liệu, chấm điểm và gọi DeepSeek song song dưới nền
                            await assemblyAIService.GetDataAssemblyAI(webhookData.TranscriptId);
                        }
                        catch (BadRequestException badEx)
                        {
                            // Đây là lỗi nghiệp vụ được dự báo trước -> Ghi log ở mức Warning (Cảnh báo)
                            logger.LogWarning("Invalid require in backgroud task: {Message}", badEx.Message);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex, "Background Task Error: Failed to process AssemblyAI webhook. TranscriptId: {TranscriptId}",
                                webhookData.TranscriptId
                            );
                        }

                    }
                );
            }
            return StatusCode(200);
        }

    }
}
