using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Nodes;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/assembly")]
    [ApiController]
    public class AssemblyAIController : ControllerBase
    {
        private readonly string _webhookAuth; 
        public AssemblyAIController(
            IConfiguration configuration)
        {
            _webhookAuth = configuration["AI:AssemblyAIKey"]!;
        }

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
                    // Gọi Orchestrator để tải kết quả, tính điểm, gọi DeepSeek song song và lưu DB
                    //await _orchestrator.HandleCompletedTranscriptionAsync(webhookData.TranscriptId);
                    return StatusCode(200);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi lại nhưng vẫn nên trả về 200/500 tùy ý để báo cho AssemblyAI biết
                    Console.WriteLine($"Error processing completed transcription: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }
            return BadRequest(new { message = "Transcription failed on AssemblyAI server." });
        }

    }
}
