using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using English.Website.Application.Services;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/audio")]
    [ApiController]
    [Authorize]
    public class AudioController : ControllerBase
    {
        private readonly AudioService _audioService;

        public AudioController(AudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet("recording")]
        public async Task<IActionResult> GetUserRecordings()
        {
            var result = await _audioService.GetUserRecordingsAsync();

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "audio.recordings",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get user recordings successfully."
            });
        }

        [HttpGet("audio-detail")]
        public async Task<IActionResult> GetAudioDetail([FromQuery] string recordingId)
        {
            var result = await _audioService.GetAudioDetailAsync(recordingId);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "audio.audio-detail",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get audio detail successfully."
            });
        }

        [HttpDelete("audio-detail")]
        public async Task<IActionResult> DeleteRecording([FromQuery] string recordingId)
        {
            await _audioService.SoftDeleteRecordingAsync(recordingId);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "audio.delete",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "Recording deleted successfully."
            });
        }
    }
}
