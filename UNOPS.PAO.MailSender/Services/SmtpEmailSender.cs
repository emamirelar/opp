using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using Google.Cloud.SecretManager.V1;
using System.Text.Json;
using UNOPS.PAO.MailSender.Interfaces;

namespace UNOPS.PAO.MailSender;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailConfiguration _emailConfig;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(
        IOptions<EmailConfiguration> emailConfig,
        IEmailTemplateRenderer templateRenderer,
        ILogger<SmtpEmailSender> logger,
        IConfiguration configuration)
    {
        _emailConfig = emailConfig.Value;
        _templateRenderer = templateRenderer;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync<T>(EmailMessage emailModel, T templateModel, string? baseUrl = null)
    {
        if (!emailModel.EmailReceivers.Any())
            return;

        var emailBody = await _templateRenderer.RenderTemplateAsync(emailModel.TemplateName, templateModel);
        var mimeMessage = CreateMimeMessage(emailModel, emailBody, baseUrl);

        await SendSmtpEmailAsync(mimeMessage);
    }

    private MimeMessage CreateMimeMessage(EmailMessage emailMessage, string emailBody, string? baseUrl)
    {
        var message = new MimeMessage
        {
            Subject = emailMessage.Title,
            Body = new TextPart(TextFormat.Html)
            {
                Text = baseUrl != null ? AddPlatformFooter(emailBody, baseUrl) : emailBody
            }
        };

        message.From.Add(new MailboxAddress(
            _emailConfig.SmtpEmailDisplayName,
            _emailConfig.SmtpEmail
        ));

        message.To.AddRange(emailMessage.EmailReceivers.Select(r => new MailboxAddress("", r)));

        // Add CC recipients for workflow notifications
        if (emailMessage.CcReceivers.Any())
        {
            message.Cc.AddRange(emailMessage.CcReceivers.Select(r => new MailboxAddress("", r)));
        }

        AddAttachments(message, emailMessage.Attachments);

        return message;
    }

    private void AddAttachments(MimeMessage message, List<EmailAttachment>? attachments)
    {
        if (attachments == null) return;

        var multipart = new Multipart("mixed");
        multipart.Add(message.Body);

        foreach (var attachment in attachments)
        {
            var mimeEntity = new MimePart(attachment.ContentType)
            {
                Content = new MimeContent(attachment.FileStream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = attachment.FileName
            };
            multipart.Add(mimeEntity);
        }

        message.Body = multipart;
    }

    private async Task SendSmtpEmailAsync(MimeMessage message)
    {
        using var client = new SmtpClient();

        try
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

            if (!string.IsNullOrEmpty(_emailConfig.LocalDomain))
                client.LocalDomain = _emailConfig.LocalDomain;

            _logger.LogInformation("Before attempting connection to SMTP Server: {SmtpServer}, Port: {SmtpPort}", _emailConfig.SmtpServer,
                _emailConfig.SmtpPort);

            await client.ConnectAsync(
                _emailConfig.SmtpServer,
                _emailConfig.SmtpPort,
                SecureSocketOptions.StartTlsWhenAvailable
            );

            _logger.LogInformation("After the connection to SMTP Server: {SmtpServer}, Port: {SmtpPort}", _emailConfig.SmtpServer,
                _emailConfig.SmtpPort);

            if (!string.IsNullOrEmpty(_emailConfig.Username) && !string.IsNullOrEmpty(_emailConfig.Password))
            {
                // Notice: UNOPS SMTP does not support auth
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
            }
            
            await client.SendAsync(message);
            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", message.To));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private static string AddPlatformFooter(string emailBody, string platformUrl) =>
        $"{emailBody}<br><br><hr><small>You are receiving this email because you are registered on: " +
        $"<a href='{platformUrl}'>{platformUrl}</a>.</small>";

}
