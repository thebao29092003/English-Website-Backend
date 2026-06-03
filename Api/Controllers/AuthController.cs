using English.Website.Api.Dtos.AuthDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    // CẦN XEM LẠI KHI NÀO RESPONSE VALUE KHI NÀO RESPONE MESSGARE 
    // THỐNG NHẤT HIỆN LÊN CHO USER THẤY THÔNG BÁO SẼ LÀ MESSAGE
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var result = await _authService.Register(userDto);
            if (!result)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.register",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = MessageConstants.GetExistMessage(true, "user")
                });
            }

            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.register",
                status = (int)HttpStatusCode.Created,
                value = null,
                message = MessageConstants.GetInsertMessage(true, "user")
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            var result = await _authService.Login(userDto);

            if (!result.Item1)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.login",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = "Invalid username or password."
                });
            }

            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.login",
                status = (int)HttpStatusCode.OK,
                value = result.Item2,
                message = MessageConstants.GetDataMessage(true, "user")
            });
        }

        [Authorize]
        [HttpGet]
        public IActionResult TestAuth()
        {
            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.allApiGet",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "You are authorized to access this endpoint."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminRoute()
        {
            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.AdminRoute",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "You are an admin."
            });
        }
    }
}
