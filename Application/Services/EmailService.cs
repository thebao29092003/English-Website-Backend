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

            // Khi đặt bằng true, toàn bộ kết nối giữa ứng dụng .NET của bạn và SMTP Server sẽ được mã hóa bảo mật 
            client.EnableSsl = true;
            // Không dùng thông tin máy tính để gửi email 
            client.UseDefaultCredentials = false;
            // Đây chính là dòng cung cấp Tên đăng nhập (Username) và Mật khẩu (Password) để đăng nhập vào hòm thư gửi [5]
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
