using English.Website.Api.Dtos.AuthDtos;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/forget-password")]
    [ApiController]
    [EnableRateLimiting("PublicApiLimit")]
    public class ForgetPasswordController : ControllerBase
    {
        private readonly ForgetPasswordService _forgetPasswordService;
        public ForgetPasswordController(ForgetPasswordService forgetPasswordService)
        {
            _forgetPasswordService = forgetPasswordService;
        }

        [HttpGet("send-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromQuery] string email, [FromQuery] string? turnstileToken = null)
        {
            var remoteIp = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                           ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                           ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            await _forgetPasswordService.SendResetPasswordOtp(email, turnstileToken, remoteIp);
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.forget-password.send-otp",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "OTP sent successfully."
            });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _forgetPasswordService.ResetPasswordWithOtp(dto);
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.forget-password.reset",
                Status = (int)HttpStatusCode.Created,
                Value = null,
                Message = "Password reset successful."
            });
        }
    }
}
