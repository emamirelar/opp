using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Business;

public class PAOEmailSender
{
    private readonly IEmailSender _emailSender;

    public PAOEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendDueDiligenceExpiryNotificationAsync(
        string email, 
        string userName, 
        string partnerName, 
        DateTime expiryDate, 
        string partnerUrl,
        decimal monthsUntilExpiry,
        int daysRemaining)
    {
        var emailMessage = new EmailMessage
        {
            TemplateName = "UNOPS.PAO.Business.EmailTemplates.DueDiligenceExpiryNotification.html",
            Title = $"Due Diligence Expiry Warning - {partnerName}",
            EmailReceivers = [email]
        };
        
        await _emailSender.SendEmailAsync(
            emailMessage,
            new
            {
                UserName = userName,
                PartnerName = partnerName,
                ExpiryDate = expiryDate.ToString("MMMM dd, yyyy"),
                MonthsUntilExpiry = monthsUntilExpiry,
                DaysRemaining = daysRemaining,
                PartnerUrl = partnerUrl,
                CurrentYear = DateTime.UtcNow.Year
            }
        );
    }
}