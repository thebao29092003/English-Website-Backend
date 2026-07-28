using System.Net;
using English.Website.Api.Dtos.ContactDtos;
using English.Website.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using whOperation.API.APIPayload;

namespace English.Website.Api.Controllers
{
    [Route("api/contact")]
    [ApiController]
    [EnableRateLimiting("PublicApiLimit")]
    public class ContactController : ControllerBase
    {
        private readonly ContactService _contactService;

        public ContactController(ContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDto dto)
        {
            await _contactService.CreateContactAsync(dto);

            return Ok(new APIResponseBase
            {
                Success = true,
                EndPointCode = "contact.create",
                Status = (int)HttpStatusCode.OK,
                Value = null,
                Message = "Gửi thông tin liên hệ thành công!"
            });
        }
    }
}
