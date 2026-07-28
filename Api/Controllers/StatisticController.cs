using System.Net;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/statistic")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("UserApiLimit")]
    public class StatisticController : ControllerBase
    {
        private readonly StatisticService _statisticService;

        public StatisticController(StatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("user-average-score")]
        public async Task<IActionResult> GetUserAverageScores()
        {
            var result = await _statisticService.GetUserAverageScores();

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "statistic.user-average-score",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get user average scores successfully."
            });
        }

        [HttpGet("daily-scores")]
        public async Task<IActionResult> GetDailyScores([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            var result = await _statisticService.GetDailyScoresAsync(fromDate, toDate);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "statistic.daily-scores",
                Status = (int)HttpStatusCode.OK,
                Value = result,
                Message = "Get daily scores successfully."
            });
        }
    }
}
