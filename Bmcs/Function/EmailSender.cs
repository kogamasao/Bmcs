using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Bmcs.Function
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var mailServer = emailSettings["MailServer"];
                var mailPort = int.Parse(emailSettings["MailPort"]);
                var senderName = emailSettings["SenderName"];
                var senderEmail = emailSettings["SenderEmail"];
                var password = emailSettings["Password"];

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(senderName, senderEmail));
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message,
                    TextBody = message // Fallback for plain text
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // For demo purposes, accept all SSL certificates (in production, use proper validation)
                    // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(mailServer, mailPort, MailKit.Security.SecureSocketOptions.StartTls);

                    // Note: only needed if the SMTP server requires authentication
                    if (!string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(senderEmail, password);
                    }

                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);
                }
                
                _logger.LogInformation($"Email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw;
            }
        }
    }
}
