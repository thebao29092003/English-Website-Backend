using English.Website.Api.Dtos.AIDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Nodes;
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
            var result = await _deepSeekService.AnalyzeSpeech(transcriptRequest);
            //var jsonParse = JsonNode.Parse(result);
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
