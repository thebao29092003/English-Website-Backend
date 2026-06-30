using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/deepseek")]
    [ApiController]
    public class DeepSeekController : ControllerBase
    {
        private readonly IDeepSeekService _deepSeekService;
        public DeepSeekController(IDeepSeekService deepSeekService)
        {
            _deepSeekService = deepSeekService;
        }

        [HttpPost("chat")]
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
