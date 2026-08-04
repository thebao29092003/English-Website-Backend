using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

namespace English.Website.Api.Controllers
{

    [Route("api/assembly")]
    [ApiController]
    public class AssemblyAIController : ControllerBase
    {
        private readonly string _webhookAuth;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AssemblyAIController(
            IConfiguration configuration,
            IBackgroundJobClient backgroundJobClient
        )
        {
            _webhookAuth = configuration["WebHook:AssemblyAI:Token"]!;
            _backgroundJobClient = backgroundJobClient;
        }

        // Này là endpoint trả về cho AssemblyAI nên nó khác những endpoint kia
        // những webhook không được gọi _useUserContextService để lấy userId vì nó ko có token của user
        [HttpPost("webhook")]
        public IActionResult AssemblyAIWebhook([FromBody] AssemblyAiWebhookDto webhookData)
        {
            if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var receivedSecret) ||
                receivedSecret != _webhookAuth)
            {
                return StatusCode(403);
            }

            if (webhookData.Status == "completed")
            {
                _backgroundJobClient.Enqueue<IAssemblyAIService>(service =>
                    service.GetDataAssemblyAI(webhookData.TranscriptId));
            }
            return StatusCode(200);
        }

    }
}
