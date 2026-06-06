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
      
    }
}
