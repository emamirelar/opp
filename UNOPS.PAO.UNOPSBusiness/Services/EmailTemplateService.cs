using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Text.Json;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Service for generating email templates for various notification types
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _fallbackOppPlusSystemUrl;

    public EmailTemplateService(IConfiguration configuration, ILogger<EmailTemplateService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _fallbackOppPlusSystemUrl = configuration["EmailSettings:OppPlusSystemUrl"] ?? "https://test-opportunityplus.unops.org";
    }

    /// <summary>
    /// Gets the current host URL from the HTTP context, with fallback to configuration
    /// </summary>
    private string GetHostUrl()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request != null)
            {
                var request = httpContext.Request;
                var scheme = request.Scheme; // http or https
                var host = request.Host.Value; // domain.com:port
                var hostUrl = $"{scheme}://{host}";
                var normalizedHostUrl = hostUrl.TrimEnd('/'); // ✅ Add URL normalization like GmailAddonHelper
                
                _logger.LogDebug("Generated host URL from request context: {HostUrl}", normalizedHostUrl);
                return normalizedHostUrl;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get host URL from HTTP context, using fallback configuration");
        }

        var normalizedFallbackUrl = _fallbackOppPlusSystemUrl.TrimEnd('/'); // ✅ Normalize fallback URL too
        _logger.LogDebug("Using fallback host URL from configuration: {HostUrl}", normalizedFallbackUrl);
        return normalizedFallbackUrl;
    }

    /// <summary>
    /// Builds a URL to a specific page/route
    /// </summary>
    private string BuildUrl(string relativePath)
    {
        var hostUrl = GetHostUrl();
        var trimmedPath = relativePath.TrimStart('/');
        return $"{hostUrl}/{trimmedPath}";
    }

    /// <summary>
    /// Build URL to a specific entity page
    /// </summary>
    public string BuildEntityUrl(string entityType, int entityId)
    {
        var entityPath = entityType.ToLower() switch
        {
            "partner" => $"#/partnerships/partners/{entityId}",
            "contact" => $"#/partnerships/contacts/{entityId}",
            "interaction" => $"#/partnerships/interaction/{entityId}",
            "opportunity" => $"#/partnerships/opportunities/{entityId}",
            _ => $"#/partnerships/{entityType.ToLower()}s/{entityId}" // Generic fallback
        };
        
        return BuildUrl(entityPath);
    }
    
    /// <summary>
    /// Get the current host URL (useful for other services)
    /// </summary>
    public string GetCurrentHostUrl()
    {
        return GetHostUrl();
    }

    /// <summary>
    /// Generate HTML email content for due diligence expiry notifications with optional partner ID for direct link
    /// </summary>
    public string GenerateDueDiligenceExpiryHtml(string userName, string partnerName, DateTime expiryDate, int? partnerId)
    {
        var monthsUntilExpiry = Math.Round((expiryDate - DateTime.UtcNow).TotalDays / 30.44, 1);
        var daysRemaining = (int)(expiryDate - DateTime.UtcNow).TotalDays;
        
        // Build specific partner URL if partnerId is provided, otherwise general partners page
        var partnerUrl = partnerId.HasValue ? BuildEntityUrl("partner", partnerId.Value) : BuildUrl("#/partnerships/partners");
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .btn {{ display: inline-block; padding: 10px 20px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>UNOPS Opportunity Plus</h1>
            <h2>Due Diligence Expiry Warning</h2>
        </div>
        
        <div class='content'>
            <p>Dear {userName},</p>
            
            <div class='warning'>
                <strong>⚠️ Important Notice:</strong> The due diligence for partner <strong>{partnerName}</strong> is set to expire in approximately <strong>{monthsUntilExpiry} months</strong>.
            </div>
            
            <p><strong>Partner:</strong> {partnerName}</p>
            <p><strong>Expiry Date:</strong> {expiryDate:MMMM dd, yyyy}</p>
            <p><strong>Days Remaining:</strong> {daysRemaining} days</p>
            
            <p>As the Partner Focal Point for this organization, please take the necessary actions to:</p>
            <ul>
                <li>Review the current due diligence status</li>
                <li>Initiate renewal process if required</li>
                <li>Update partner information in the system</li>
                <li>Contact relevant stakeholders for document updates</li>
            </ul>
            
            <p>
                <a href='{partnerUrl}' class='btn'>View Partner Details</a>
            </p>
            
            <p>If you have any questions or need assistance, please contact the Opportunity Plus system administrators.</p>
            
            <p>Best regards,<br>
            UNOPS Opportunity Plus</p>
        </div>
        
        <div class='footer'>
            <p>This is an automated notification from UNOPS Opportunity Plus. Please do not reply to this email.</p>
            <p>© {DateTime.UtcNow.Year} United Nations Office for Project Services (UNOPS)</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Generate plain text email content for due diligence expiry notifications with optional partner ID for direct link
    /// </summary>
    public string GenerateDueDiligenceExpiryPlainText(string userName, string partnerName, DateTime expiryDate, int? partnerId)
    {
        var monthsUntilExpiry = Math.Round((expiryDate - DateTime.UtcNow).TotalDays / 30.44, 1);
        var daysRemaining = (int)(expiryDate - DateTime.UtcNow).TotalDays;
        
        // Build specific partner URL if partnerId is provided, otherwise general partners page
        var partnerUrl = partnerId.HasValue ? BuildEntityUrl("partner", partnerId.Value) : BuildUrl("#/partnerships/partners");
        
        return $@"UNOPS Opportunity Plus - Due Diligence Expiry Warning

Dear {userName},

IMPORTANT NOTICE: The due diligence for partner '{partnerName}' is set to expire in approximately {monthsUntilExpiry} months.

Partner: {partnerName}
Expiry Date: {expiryDate:MMMM dd, yyyy}
Days Remaining: {daysRemaining} days

As the Partner Focal Point for this organization, please take the necessary actions to:
- Review the current due diligence status
- Initiate renewal process if required
- Update partner information in the system
- Contact relevant stakeholders for document updates

Please access Opportunity Plus to manage this partner's due diligence: {partnerUrl}

If you have any questions or need assistance, please contact the Opportunity Plus system administrators.

Best regards,
UNOPS Opportunity Plus

---
This is an automated notification from UNOPS Opportunity Plus. Please do not reply to this email.
© {DateTime.UtcNow.Year} United Nations Office for Project Services (UNOPS)";
    }

    public string GenerateHtmlFromTemplate(string templateName, Dictionary<string, object> templateData)
    {
        return templateName.ToLower() switch
        {
            "duediligenceexpiry" => GenerateDueDiligenceExpiryFromData(templateData, isHtml: true),
            _ => GenerateGenericNotificationFromData(templateData, isHtml: true)
        };
    }

    public string GeneratePlainTextFromTemplate(string templateName, Dictionary<string, object> templateData)
    {
        return templateName.ToLower() switch
        {
            "duediligenceexpiry" => GenerateDueDiligenceExpiryFromData(templateData, isHtml: false),
            _ => GenerateGenericNotificationFromData(templateData, isHtml: false)
        };
    }

    public string GetSubjectLine(string notificationType, Dictionary<string, object> templateData)
    {
        return notificationType.ToLower() switch
        {
            "duediligenceexpiry" => $"Due Diligence Expiry Warning - {templateData.GetValueOrDefault("PartnerName", "Partner")}",
            _ => $"UNOPS Opportunity Plus Notification - {templateData.GetValueOrDefault("Subject", "Important Notice")}"
        };
    }

    private string GenerateDueDiligenceExpiryFromData(Dictionary<string, object> data, bool isHtml)
    {
        var userName = data.GetValueOrDefault("UserName", "User")?.ToString() ?? "User";
        var partnerName = data.GetValueOrDefault("PartnerName", "Partner")?.ToString() ?? "Partner";
        var expiryDate = data.GetValueOrDefault("ExpiryDate") is DateTime date ? date : DateTime.UtcNow.AddMonths(6);
        var partnerId = data.GetValueOrDefault("PartnerId") is int id ? (int?)id : null;

        return isHtml 
            ? GenerateDueDiligenceExpiryHtml(userName, partnerName, expiryDate, partnerId)
            : GenerateDueDiligenceExpiryPlainText(userName, partnerName, expiryDate, partnerId);
    }





    private string GenerateGenericNotificationFromData(Dictionary<string, object> data, bool isHtml)
    {
        var userName = data.GetValueOrDefault("UserName", "User")?.ToString() ?? "User";
        var title = data.GetValueOrDefault("Title", "UNOPS Opportunity Plus Notification")?.ToString() ?? "UNOPS Opportunity Plus Notification";
        var message = data.GetValueOrDefault("Message", "You have a new notification.")?.ToString() ?? "You have a new notification.";

        if (isHtml)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .btn {{ display: inline-block; padding: 10px 20px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>UNOPS Opportunity Plus</h1>
            <h2>{title}</h2>
        </div>
        
        <div class='content'>
            <p>Dear {userName},</p>
            
            <p>{message}</p>
            
            <p>
                <a href='{GetHostUrl()}' class='btn'>Access Opportunity Plus</a>
            </p>
            
            <p>Best regards,<br>
            UNOPS Opportunity Plus</p>
        </div>
        
        <div class='footer'>
            <p>This is an automated notification from UNOPS Opportunity Plus. Please do not reply to this email.</p>
            <p>© {DateTime.UtcNow.Year} United Nations Office for Project Services (UNOPS)</p>
        </div>
    </div>
</body>
</html>";
        }
        else
        {
            return $@"UNOPS Opportunity Plus - {title}

Dear {userName},

{message}

Please access Opportunity Plus: {GetHostUrl()}

Best regards,
UNOPS Opportunity Plus

---
This is an automated notification from UNOPS Opportunity Plus. Please do not reply to this email.
© {DateTime.UtcNow.Year} United Nations Office for Project Services (UNOPS)";
        }
    }
}
