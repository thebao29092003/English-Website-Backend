using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Nodes;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/backend-python")]
    [ApiController]
    public class BackendPythonController : ControllerBase
    {
        private readonly string _webhookAuth;
        private readonly AISpeechToTextService _aiSpeechToTextService;
        public BackendPythonController(
            IConfiguration configuration,
            AISpeechToTextService aiSpeechToTextService
        )
        {
            _webhookAuth = configuration["BackendPython"]!;
            _aiSpeechToTextService = aiSpeechToTextService;
        }

        // Này là endpoint trả về cho AssemblyAI nên nó khác những endpoint kia
        [HttpPost("phonetic-webhook")]
        public async Task<IActionResult> PythonPhoneticWebhook([FromBody] PythonPhonemeWebhookDto webhookData)
        {
            if (!Request.Headers.TryGetValue("X-Python-Secret", out var receivedSecret) ||
                receivedSecret != _webhookAuth)
            {
                return Unauthorized(new { message = "Unauthorized webhook request." });
            }

            try
            {
                await _aiSpeechToTextService.Update(webhookData);

                Console.WriteLine($"[Webhook] Successfully saved phonemes for Recording: {webhookData.RecordingId}");

                // TÙY TRỌN ĐẨY SignalR (dùng khi có frontend)
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi lại nhưng vẫn nên trả về 200/500 tùy ý để báo cho AssemblyAI biết
                Console.WriteLine($"Error processing completed transcription: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
