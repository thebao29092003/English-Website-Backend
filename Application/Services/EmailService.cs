using English.Website.Application.Services.IServices;

namespace English.Website.Application.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Implement email sending logic here
        }
    }
}
