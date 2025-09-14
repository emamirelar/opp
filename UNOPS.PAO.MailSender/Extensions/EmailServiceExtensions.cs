using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.MailSender.Interfaces;

namespace UNOPS.PAO.MailSender;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailConfiguration>(
            configuration.GetSection(EmailConfiguration.SectionName)
        );

        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
