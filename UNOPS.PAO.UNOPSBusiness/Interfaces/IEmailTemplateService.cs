namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

/// <summary>
/// Service for generating email templates for various notification types
/// </summary>
public interface IEmailTemplateService
{    
    /// <summary>
    /// Generate HTML email content for due diligence expiry notifications with optional partner ID for direct link
    /// </summary>
    string GenerateDueDiligenceExpiryHtml(string userName, string partnerName, DateTime expiryDate, int? partnerId);
        
    /// <summary>
    /// Generate plain text email content for due diligence expiry notifications with optional partner ID for direct link
    /// </summary>
    string GenerateDueDiligenceExpiryPlainText(string userName, string partnerName, DateTime expiryDate, int? partnerId);
    
    /// <summary>
    /// Generate HTML email content for any notification type using a template
    /// </summary>
    string GenerateHtmlFromTemplate(string templateName, Dictionary<string, object> templateData);
    
    /// <summary>
    /// Generate plain text email content for any notification type using a template
    /// </summary>
    string GeneratePlainTextFromTemplate(string templateName, Dictionary<string, object> templateData);
    
    /// <summary>
    /// Get email subject line for a specific notification type
    /// </summary>
    string GetSubjectLine(string notificationType, Dictionary<string, object> templateData);
    
    /// <summary>
    /// Build URL to a specific entity page
    /// </summary>
    string BuildEntityUrl(string entityType, int entityId);
    
    /// <summary>
    /// Get the current host URL (useful for other services)
    /// </summary>
    string GetCurrentHostUrl();
}
