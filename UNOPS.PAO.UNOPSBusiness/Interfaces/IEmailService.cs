namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null);
    Task<bool> SendEmailAsync(IEnumerable<string> toEmails, string subject, string htmlBody, string? plainTextBody = null);
    Task<bool> SendDueDiligenceExpiryNotificationAsync(string userEmail, string userName, string partnerName, DateTime expiryDate, int? partnerId);
}
