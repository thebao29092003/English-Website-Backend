using English.Website.Application.Services.IServices;
using System.Net;
using System.Net.Mail;

namespace English.Website.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["Smtp:Server"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var senderEmail = _configuration["Smtp:SenderEmail"];
            var senderPassword = _configuration["Smtp:SenderPassword"];

            using var client = new SmtpClient(smtpServer, smtpPort);

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, "English Website AI"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

        }
    }
}
