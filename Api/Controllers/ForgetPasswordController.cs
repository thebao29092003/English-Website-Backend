using English.Website.Api.Dtos.AuthDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/forget-password")]
    [ApiController]
    public class ForgetPasswordController : ControllerBase
    {
        private readonly AuthService _authService;
        public ForgetPasswordController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
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

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
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
    }
}
