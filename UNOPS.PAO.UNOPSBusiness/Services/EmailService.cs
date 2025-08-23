using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Google.Cloud.SecretManager.V1;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly bool _enableSsl;
    private readonly bool _useCredentials;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _emailSecretName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IEmailTemplateService emailTemplateService)
    {
        _configuration = configuration;
        _logger = logger;
        _emailTemplateService = emailTemplateService;
        
        var emailSettings = configuration.GetSection("EmailSettings");
        _smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        _enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
        _useCredentials = bool.Parse(emailSettings["UseCredentials"] ?? "true");
        _fromEmail = emailSettings["FromEmail"] ?? "notifications@unops.org";
        _fromName = emailSettings["FromName"] ?? "UNOPS PAO System";
        _emailSecretName = emailSettings["EmailSecretName"] ?? "EmailCredentials";
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null)
    {
        return await SendEmailAsync(new[] { toEmail }, subject, htmlBody, plainTextBody);
    }

    public async Task<bool> SendEmailAsync(IEnumerable<string> toEmails, string subject, string htmlBody, string? plainTextBody = null)
    {
        try
        {
            var credentials = await GetEmailCredentialsAsync();
            
            using var client = new SmtpClient(_smtpServer, _smtpPort);
            client.EnableSsl = _enableSsl;
            
            if (_useCredentials && credentials != null)
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(credentials.Username, credentials.Password);
            }
            
            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                IsBodyHtml = !string.IsNullOrEmpty(htmlBody)
            };

            // Add recipients
            foreach (var email in toEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    message.To.Add(email);
                }
            }

            // Set body content
            if (!string.IsNullOrEmpty(htmlBody))
            {
                message.Body = htmlBody;
                message.IsBodyHtml = true;
                
                // Add plain text alternative if provided
                if (!string.IsNullOrEmpty(plainTextBody))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, "text/plain");
                    var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                    message.AlternateViews.Add(plainView);
                    message.AlternateViews.Add(htmlView);
                }
            }
            else if (!string.IsNullOrEmpty(plainTextBody))
            {
                message.Body = plainTextBody;
                message.IsBodyHtml = false;
            }

            await client.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {string.Join(", ", toEmails)}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {string.Join(", ", toEmails)}");
            return false;
        }
    }

    /// <summary>
    /// Send due diligence expiry notification with optional partner ID for direct link
    /// </summary>
    public async Task<bool> SendDueDiligenceExpiryNotificationAsync(string userEmail, string userName, string partnerName, DateTime expiryDate, int? partnerId)
    {
        var templateData = new Dictionary<string, object>
        {
            ["UserName"] = userName,
            ["PartnerName"] = partnerName,
            ["ExpiryDate"] = expiryDate
        };
        
        if (partnerId.HasValue)
        {
            templateData["PartnerId"] = partnerId.Value;
        }

        var subject = _emailTemplateService.GetSubjectLine("DueDiligenceExpiry", templateData);
        var htmlBody = _emailTemplateService.GenerateDueDiligenceExpiryHtml(userName, partnerName, expiryDate, partnerId);
        var plainTextBody = _emailTemplateService.GenerateDueDiligenceExpiryPlainText(userName, partnerName, expiryDate, partnerId);

        return await SendEmailAsync(userEmail, userName, subject, htmlBody, plainTextBody);
    }



    private async Task<EmailCredentials?> GetEmailCredentialsAsync()
    {
        try
        {
            var projectId = _configuration["AppConfig:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                _logger.LogWarning("ProjectId not found in configuration");
                return null;
            }

            var secretClient = SecretManagerServiceClient.Create();
            var secretName = $"projects/{projectId}/secrets/{_emailSecretName}/versions/latest";
            
            var response = await secretClient.AccessSecretVersionAsync(secretName);
            var secretValue = response.Payload.Data.ToStringUtf8();
            
            // Expect JSON format: {"Username": "email@domain.com", "Password": "app-password"}
            var credentials = System.Text.Json.JsonSerializer.Deserialize<EmailCredentials>(secretValue);
            return credentials;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve email credentials from secret {_emailSecretName}");
            return null;
        }
    }

    private class EmailCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
