namespace UNOPS.PAO.MailSender.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync<T>(EmailMessage emailMessage, T templateModel, string? baseUrl = null);
}
