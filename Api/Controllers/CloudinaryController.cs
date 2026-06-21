using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Dtos.CloudinaryDtos;
using English.Website.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/cloudinary")]
    [ApiController]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        public CloudinaryController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadAudio([FromForm] UploadRequestDto requestDto)
        {
            var result = await _cloudinaryService.UploadFileAsync(requestDto);
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "cloudinary.upload",
                status = (int)HttpStatusCode.OK,
                value = result,
                message = "Upload cloudinary successfully"
            });
        }
    }
}
