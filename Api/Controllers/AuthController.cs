using English.Website.Api.Dtos.AuthDtos;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Net;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("register/send-otp")]
        [EnableRateLimiting("PublicApiLimit")]
        public async Task<IActionResult> SendRegisterOtp([FromQuery] string email, [FromQuery] string? turnstileToken = null)
        {
            var remoteIp = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                           ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                           ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            await _authService.SendRegisterOtp(email, turnstileToken, remoteIp);
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.register.send-otp",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "OTP sent successfully."
            });
        }

        [HttpPost("register")]
        [EnableRateLimiting("PublicApiLimit")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            await _authService.Register(registerDto);
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.register",
                Status = (int)HttpStatusCode.Created,
                Value = null,
                Message = "Registration successful."
            });
        }

        [HttpPost("login")]
        [EnableRateLimiting("PublicApiLimit")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            var result = await _authService.Login(userDto);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.login",
                Status = (int)HttpStatusCode.OK,
                Value = result.AccessToken,
                Message = "Get user successfull"
            });
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("PublicApiLimit")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshToken();
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.refreshToken",
                Status = (int)HttpStatusCode.OK,
                Value = result.AccessToken,
                Message = "Get refresh token successfull"
            });
        }

        [HttpPost("logout")]
        [Authorize]
        [EnableRateLimiting("UserApiLimit")]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.logout",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "Logout successful."
            });
        }

        [HttpPut("change-is-active")]
        [Authorize(Roles = "ADMIN")]
        [EnableRateLimiting("UserApiLimit")]
        public async Task<IActionResult> UpdateUserIsActive([FromQuery] string userId)
        {
            await _authService.UpdateUserStatusAsync(userId);
            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "auth.changeIsActive",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "Update status is active successfully"
            });
        }
    }
}

