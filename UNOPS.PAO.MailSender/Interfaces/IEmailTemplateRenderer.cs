namespace UNOPS.PAO.MailSender.Interfaces;

public interface IEmailTemplateRenderer
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}
