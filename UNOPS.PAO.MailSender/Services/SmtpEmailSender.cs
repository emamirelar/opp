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

            await client.ConnectAsync(
                _emailConfig.SmtpServer,
                _emailConfig.SmtpPort,
                SecureSocketOptions.StartTlsWhenAvailable
            );

            // Authenticate using Google Secret Manager credentials
            var credentials = await GetEmailCredentialsAsync();
            if (credentials != null && !string.IsNullOrEmpty(credentials.Username) && !string.IsNullOrEmpty(credentials.Password))
            {
                await client.AuthenticateAsync(credentials.Username, credentials.Password);
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

    private async Task<EmailCredentials?> GetEmailCredentialsAsync()
    {
        try
        {
            var projectId = _configuration.GetSection("AppConfig")["ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                _logger.LogError("ProjectId not configured for Google Secret Manager");
                return null;
            }

            var client = SecretManagerServiceClient.Create();
            var secretName = $"projects/{projectId}/secrets/EmailCredentials/versions/latest";
            
            var response = await client.AccessSecretVersionAsync(secretName);
            var secretValue = response.Payload.Data.ToStringUtf8();
            
            var credentials = JsonSerializer.Deserialize<EmailCredentials>(secretValue);
            return credentials;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve email credentials from secret manager");
            return null;
        }
    }

    private static string AddPlatformFooter(string emailBody, string platformUrl) =>
        $"{emailBody}<br><br><hr><small>You are receiving this email because you are registered on: " +
        $"<a href='{platformUrl}'>{platformUrl}</a>.</small>";

    private record EmailCredentials
    {
        public string? Username { get; init; }
        public string? Password { get; init; }
    }
}
