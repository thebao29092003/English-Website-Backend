using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using English.Website.Application.Services;
using English.Website.Api.Dtos.HomeDtos;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/home")]
    [ApiController]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly HomeService _homeService;

        public HomeController(HomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet("recordings")]
        public async Task<IActionResult> GetUserRecordings()
        {
            var result = await _homeService.GetUserRecordingsAsync();

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "home.recordings",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get user recordings successfully."
            });
        }
    }
}
