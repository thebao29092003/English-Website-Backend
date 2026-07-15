using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using English.Website.Application.Services;
using English.Website.Api.Dtos.HomeDtos;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/audio-detail")]
    [ApiController]
    [Authorize]
    public class AudioDetailController : ControllerBase
    {
        private readonly AudioDetailService _audioDetailService;

        public AudioDetailController(AudioDetailService audioDetailService)
        {
            _audioDetailService = audioDetailService;
        }


        [HttpGet("")]
        public async Task<IActionResult> GetAudioDetail([FromQuery] string recordingId)
        {
            var result = await _audioDetailService.GetAudioDetailAsync(recordingId);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "home.audio-detail",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get audio detail successfully."
            });
        }
    }
}
