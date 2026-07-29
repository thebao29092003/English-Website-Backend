using English.Website.Api.Dtos.ContactDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;

namespace English.Website.Application.Services
{
    public class ContactService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly ITurnstileService _turnstileService;

        public ContactService(EnglishDBContext dbContext, ITurnstileService turnstileService)
        {
            _dbContext = dbContext;
            _turnstileService = turnstileService;
        }

        public async Task CreateContactAsync(CreateContactDto dto, string? remoteIp = null)
        {
            var isTurnstileValid = await _turnstileService.VerifyTokenAsync(dto.TurnstileToken, remoteIp);
            if (!isTurnstileValid)
            {
                throw new BadRequestException("Invalid or expired CAPTCHA.");
            }

            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                string.IsNullOrWhiteSpace(dto.Occupation) ||
                string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new BadRequestException("Missing or invalid contact information.");
            }

            var contact = new Contact
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Occupation = dto.Occupation.Trim(),
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _dbContext.Contact.AddAsync(contact);
            await _dbContext.SaveChangesAsync();
        }
    }
}
