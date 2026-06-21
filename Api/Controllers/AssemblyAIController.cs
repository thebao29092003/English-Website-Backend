using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
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
        private readonly IDeepSeekService _deepSeekService;
        public AssemblyAIController(IDeepSeekService deepSeekService)
        {
            _deepSeekService = deepSeekService;
        }

        [HttpPost("speech-to-text")]
        [Authorize]
        public async Task<IActionResult> RequestAI([FromBody] TranscriptRequestDto transcriptRequest)
        {
            var result = await _deepSeekService.CallDeepSeekApi(transcriptRequest);
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "deepseek.chat",
                status = (int)HttpStatusCode.OK,
                value = result,
                message = "DeepSeek response successfully"
            });
        }

    }
}
