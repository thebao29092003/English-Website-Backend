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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
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

        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // 👈 Bảo vệ khỏi XSS (React không đọc được)
                Secure = true,   // 👈 Chỉ gửi qua HTTPS (khi deploy thật)
                SameSite = SameSiteMode.Lax, // 👈 Chống tấn công CSRF
                Expires = DateTime.UtcNow.AddDays(7) // Khớp với hạn của RefreshToken
            };

            // Ghi cookie tên là "refreshToken" vào trình duyệt của client
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
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

            SetRefreshTokenInCookie(result.Item2!.RefreshToken);

            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.login",
                status = (int)HttpStatusCode.OK,
                value = result.Item2.AccessToken,
                message = MessageConstants.GetDataMessage(true, "user")
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {

            // 👇 Đọc trực tiếp từ Cookie mà trình duyệt tự động gửi kèm lên
            var refreshToken = Request.Cookies["refreshToken"];


            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.refreshToken",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = MessageConstants.GetFoundMessage(false, "refresh token")
                });
            }

            var result = await _authService.RefreshToken(refreshToken);

            if (result == null)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.refreshToken",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = "Invalid refresh token."
                });
            }

            SetRefreshTokenInCookie(result.RefreshToken);

            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.refreshToken",
                status = (int)HttpStatusCode.OK,
                value = result.AccessToken,
                message = MessageConstants.GetDataMessage(true, "refresh token")
            });
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("UserId")!.Value;
            var result = await _authService.Logout(userId);

            if (!result)
            {
                return BadRequest(new APIResponseBase
                {
                    isResponseResult = false,
                    success = false,
                    endPointCode = "auth.logout",
                    status = (int)HttpStatusCode.BadRequest,
                    value = null,
                    message = "Invalid user ID."
                });
            }

            return Ok(new APIResponseBase
            {
                isResponseResult = true,
                success = true,
                endPointCode = "auth.logout",
                status = (int)HttpStatusCode.OK,
                value = result,
                message = "Logout successful."
            });
        }

        [HttpGet]
        [Authorize]
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
