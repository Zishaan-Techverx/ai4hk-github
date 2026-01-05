namespace TpaSodManagement.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, bool isHtml = true);
    }
}

