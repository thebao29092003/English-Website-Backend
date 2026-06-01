using Azure.Core;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using English.Website.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/video-game")]
    [ApiController]
    public class VideoGameController : ControllerBase
    {
        private readonly AudioService _audioService;
        public VideoGameController(AudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var videoGames = await _audioService.GetAll();
            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "videoGame.getAll",
                status = (int)HttpStatusCode.OK,
                value = videoGames,
                message = MessageConstants.GetDataMessage(true, "videoGame")
            });
        }

        //[HttpGet("getById")]
        //public IActionResult GetById([FromQuery] int id)
        //{
        //    var game = listGame.FirstOrDefault(g => g.Id == id);
        //    if (game == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(game);
        //}

        //[HttpPost]
        //public IActionResult AddVideoGame([FromBody] VideoGame newGame)
        //{
        //    if (newGame is null)
        //        return BadRequest();

        //    newGame.Id = listGame.Max(g => g.Id) + 1;
        //    listGame.Add(newGame);

        //    return Created();
        //}
    }
}
