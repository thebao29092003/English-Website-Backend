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
        private readonly ForgetPasswordService _forgetPasswordService;
        public ForgetPasswordController(ForgetPasswordService forgetPasswordService)
        {
            _forgetPasswordService = forgetPasswordService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromQuery] string email)
        {
            var (success, message) = await _forgetPasswordService.SendResetPasswordOtp(email);
            if (!success)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.forget-password.send-otp",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = message
                });
            }

            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.forget-password.send-otp",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = message
            });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            var (success, message) = await _forgetPasswordService.ResetPasswordWithOtp(dto);
            if (!success)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.forget-password.reset",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = message
                });
            }

            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.forget-password.reset",
                status = (int)HttpStatusCode.Created,
                value = null,
                message = message
            });
        }
    }
}
