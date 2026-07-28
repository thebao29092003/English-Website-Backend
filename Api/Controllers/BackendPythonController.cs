using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
            _webhookAuth = configuration["WebHook:BackendPython:Token"]!;
            _aiSpeechToTextService = aiSpeechToTextService;
        }

        // những webhook không được gọi _useUserContextService để lấy userId vì nó ko có token của user
        [HttpPost("phonetic-webhook")]
        public async Task<IActionResult> PythonPhoneticWebhook([FromBody] PythonPhonemeWebhookDto webhookData)
        {
            if (!Request.Headers.TryGetValue("X-Python-Secret", out var receivedSecret) ||
                receivedSecret != _webhookAuth)
            {
                return Unauthorized(new { message = "Unauthorized webhook request." });
            }

            await _aiSpeechToTextService.Update(webhookData);

            return StatusCode(200);
        }
    }
}
