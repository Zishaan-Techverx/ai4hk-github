using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Email configuration is missing. SmtpServer: {SmtpServer}, SmtpUsername: {SmtpUsername}, SmtpPassword: {HasPassword}", 
                        smtpServer ?? "null", smtpUsername ?? "null", !string.IsNullOrEmpty(smtpPassword));
                    return false;
                }

                if (string.IsNullOrEmpty(toEmail))
                {
                    _logger.LogError("Recipient email address is empty. Cannot send email.");
                    return false;
                }

                _logger.LogInformation("Attempting to send email to {ToEmail} via {SmtpServer}:{SmtpPort}", toEmail, smtpServer, smtpPort);

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.Timeout = 1000; // 30 seconds timeout
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (var message = new MailMessage())
                    {
                        // Use a valid sender email (SendGrid requires verified sender)
                        var senderEmail = !string.IsNullOrEmpty(fromEmail) ? fromEmail : "noreply@sendgrid.net";
                        var senderName = !string.IsNullOrEmpty(fromName) ? fromName : "TPA Sod Management System";
                        
                        message.From = new MailAddress(senderEmail, senderName);
                        message.To.Add(new MailAddress(toEmail, toName));
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = isHtml;

                        _logger.LogInformation("Sending email from {FromEmail} to {ToEmail} with subject: {Subject}", senderEmail, toEmail, subject);
                        
                        await client.SendMailAsync(message);
                        _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
                return false;
            }
        }
    }
}

