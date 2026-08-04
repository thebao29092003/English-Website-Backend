namespace English.Website.Application.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendTemplatedEmailAsync(string toEmail, string subject, string title, string content);
    }
}
