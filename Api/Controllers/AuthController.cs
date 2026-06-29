using English.Website.Api.Dtos.AuthDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("register/send-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromQuery] string email)
        {
            await _authService.SendRegisterOtp(email);
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.register.send-otp",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "OTP sent successfully."
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            await _authService.Register(registerDto);
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.register",
                status = (int)HttpStatusCode.Created,
                value = null,
                message = "Registration successful."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            var result = await _authService.Login(userDto);

            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.login",
                status = (int)HttpStatusCode.OK,
                value = result.AccessToken,
                message = "Get user successfull"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshToken();
            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.refreshToken",
                status = (int)HttpStatusCode.OK,
                value = result.AccessToken,
                message = "Get refresh token successfull"
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();

            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.logout",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "Logout successful."
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.allApiGet",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "You are authorized to access this endpoint."
            });
        }

        [HttpPut("change-is-active")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateUserIsActive([FromQuery] string userId)
        {
            await _authService.UpdateUserStatusAsync(userId);
            return Ok(new APIResponseBase
            {
                isResponseResult = false,
                success = true,
                endPointCode = "auth.changeIsActive",
                status = (int)HttpStatusCode.OK,
                value = null,
                message = "Update status is active successfully"
            });
        }
    }
}

