using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

#region Email Template Models

/// <summary>
/// Template model for workflow approval request emails.
/// </summary>
public record ApprovalRequestEmailModel
{
    public string ApproverName { get; init; } = string.Empty;
    public string ApproverRole { get; init; } = "DoA Level 2";
    public string OrgUnitName { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string RequestedByName { get; init; } = string.Empty;
    public string RequestedOn { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string EntityUrl { get; init; } = string.Empty;
}

/// <summary>
/// Template model for workflow completed (Go Decision approved) emails.
/// </summary>
public record WorkflowCompletedEmailModel
{
    public string RecipientName { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string OrgUnitName { get; init; } = string.Empty;
    public string ApprovedByName { get; init; } = string.Empty;
    public string ApprovedOn { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string EntityUrl { get; init; } = string.Empty;
}

/// <summary>
/// Template model for workflow rejected (NO GO) emails.
/// </summary>
public record WorkflowRejectedEmailModel
{
    public string RecipientName { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string OrgUnitName { get; init; } = string.Empty;
    public string RejectedByName { get; init; } = string.Empty;
    public string RejectedOn { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string EntityUrl { get; init; } = string.Empty;
}

/// <summary>
/// Template model for workflow recalled emails.
/// </summary>
public record WorkflowRecalledEmailModel
{
    public string RecipientName { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string OrgUnitName { get; init; } = string.Empty;
    public string RecalledByName { get; init; } = string.Empty;
    public string RecalledOn { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string EntityUrl { get; init; } = string.Empty;
}

#endregion

/// <summary>
/// PAO implementation of IWorkflowNotificationService.
/// Sends workflow-related email notifications using PAO's email infrastructure.
/// </summary>
public class PaoWorkflowNotificationService : IWorkflowNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;
    private readonly ILogger<PaoWorkflowNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public PaoWorkflowNotificationService(
        IEmailSender emailSender,
        AppDbContext context,
        ILogger<PaoWorkflowNotificationService> logger,
        IConfiguration configuration)
    {
        _emailSender = emailSender;
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _baseUrl = _configuration["AppBaseUrl"] ?? "https://pao.unops.org";
    }

    /// <summary>
    /// Notifies DoA Level 2 holders about a new Go Decision requiring their attention.
    /// </summary>
    public async Task NotifyNewApprovalRequestAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipientEmails.Any())
            {
                _logger.LogWarning("No recipients found for approval request notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(notification.RecipientUserIds);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var emailModel = new ApprovalRequestEmailModel
            {
                ApproverName = string.Join(", ", recipientNames),
                ApproverRole = "DoA Level 2",
                OrgUnitName = orgUnitName,
                EntityName = notification.EntityDisplayName,
                RequestedByName = notification.PerformedByUserName,
                RequestedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                EntityUrl = $"{_baseUrl}/opportunity/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "WorkflowApprovalRequest.html",
                Title = $"PAO: {notification.EntityDisplayName} - Action Required",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent approval request email for {EntityName} (ID: {EntityId}) to {RecipientCount} recipients",
                notification.EntityDisplayName, notification.EntityId, recipientEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval request notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies the submitter that the Go Decision has been approved.
    /// </summary>
    public async Task NotifyWorkflowCompletedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipientEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow completed notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(notification.RecipientUserIds);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var emailModel = new WorkflowCompletedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                ApprovedByName = notification.PerformedByUserName,
                ApprovedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                EntityUrl = $"{_baseUrl}/opportunity/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "WorkflowCompleted.html",
                Title = $"PAO: {notification.EntityDisplayName} - Go Decision Approved",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow completed email for {EntityName} (ID: {EntityId}) to {RecipientCount} recipients",
                notification.EntityDisplayName, notification.EntityId, recipientEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow completed notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies the submitter that the opportunity has been set to NO GO.
    /// </summary>
    public async Task NotifyWorkflowRejectedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipientEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow rejected notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(notification.RecipientUserIds);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var emailModel = new WorkflowRejectedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                RejectedByName = notification.PerformedByUserName,
                RejectedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                EntityUrl = $"{_baseUrl}/opportunity/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "WorkflowRejected.html",
                Title = $"PAO: {notification.EntityDisplayName} - Set to NO GO",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow rejected (NO GO) email for {EntityName} (ID: {EntityId}) to {RecipientCount} recipients",
                notification.EntityDisplayName, notification.EntityId, recipientEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow rejected notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies DoA Level 2 holders that the Go Decision submission has been recalled.
    /// </summary>
    public async Task NotifyWorkflowRecalledAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipientEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow recalled notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(notification.RecipientUserIds);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var emailModel = new WorkflowRecalledEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                RecalledByName = notification.PerformedByUserName,
                RecalledOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                EntityUrl = $"{_baseUrl}/opportunity/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "WorkflowRecalled.html",
                Title = $"PAO: {notification.EntityDisplayName} - Submission Recalled",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow recalled email for {EntityName} (ID: {EntityId}) to {RecipientCount} recipients",
                notification.EntityDisplayName, notification.EntityId, recipientEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow recalled notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies internal stakeholders from other org units about an approved Go Decision.
    /// Called when an opportunity moves to GO stage.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="approverName">Name of the person who approved</param>
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync(int opportunityId, string approverName)
    {
        try
        {
            var opportunity = await _context.Opportunities
                .AsNoTracking()
                .Include(o => o.ResponsibleOrgUnit)
                .Include(o => o.Countries)
                    .ThenInclude(oc => oc.Country)
                .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found for internal stakeholder notification", opportunityId);
                return;
            }

            // Get country IDs from the opportunity
            var countryIds = opportunity.Countries.Select(c => c.CountryId).ToList();
            if (!countryIds.Any())
            {
                _logger.LogInformation("No countries found for opportunity {OpportunityId}, skipping internal stakeholder notification", opportunityId);
                return;
            }

            // Get org units normally responsible for these countries (excluding opportunity's own org unit)
            var orgUnitIds = await _context.OrganizationUnitRelationships
                .AsNoTracking()
                .Where(r => countryIds.Contains(r.EntityId) && r.EntityType == "Country" && r.OrganizationHierarchyId != opportunity.ResponsibleOrgUnitId)
                .Select(r => r.OrganizationHierarchyId)
                .Distinct()
                .ToListAsync();

            if (!orgUnitIds.Any())
            {
                _logger.LogInformation("No other org units responsible for opportunity {OpportunityId} countries, skipping notification", opportunityId);
                return;
            }

            // Get internal stakeholders from those org units (users with relevant roles)
            // For now, we'll notify users who have DoA2 role on those org units
            var stakeholderUserIds = await _context.EntityUserRoles
                .AsNoTracking()
                .Include(e => e.EntityRole)
                .Where(e => e.EntityType == "OrganizationHierarchy" 
                         && orgUnitIds.Contains(e.EntityId)
                         && !e.IsDeleted)
                .Select(e => e.UserId)
                .Distinct()
                .ToListAsync();

            if (!stakeholderUserIds.Any())
            {
                _logger.LogInformation("No internal stakeholders found for other org units for opportunity {OpportunityId}", opportunityId);
                return;
            }

            var recipientEmails = await GetRecipientEmailsAsync(stakeholderUserIds);
            if (!recipientEmails.Any())
            {
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(stakeholderUserIds);

            var emailModel = new WorkflowCompletedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = opportunity.Name,
                OrgUnitName = opportunity.ResponsibleOrgUnit?.Name ?? "Unknown",
                ApprovedByName = approverName,
                ApprovedOn = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"),
                Comment = "This opportunity has been approved for development and may affect countries in your area of responsibility.",
                EntityUrl = $"{_baseUrl}/opportunity/{opportunityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "WorkflowCompleted.html",
                Title = $"PAO: {opportunity.Name} - Go Decision Approved (FYI)",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent internal stakeholder notification for opportunity {OpportunityId} to {RecipientCount} users from {OrgUnitCount} org units",
                opportunityId, recipientEmails.Count, orgUnitIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send internal stakeholder notification for opportunity {OpportunityId}", opportunityId);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Gets email addresses for the specified user IDs.
    /// </summary>
    private async Task<List<string>> GetRecipientEmailsAsync(List<int> userIds)
    {
        if (!userIds.Any())
            return new List<string>();

        return await _context.PAOUsers
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
            .Select(u => u.Email!)
            .ToListAsync();
    }

    /// <summary>
    /// Gets display names for the specified user IDs.
    /// </summary>
    private async Task<List<string>> GetRecipientNamesAsync(List<int> userIds)
    {
        if (!userIds.Any())
            return new List<string>();

        return await _context.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.UserProfile != null 
                ? $"{u.UserProfile.FirstName} {u.UserProfile.LastName}".Trim() 
                : u.Email ?? "User")
            .ToListAsync();
    }

    /// <summary>
    /// Gets the responsible org unit name for an opportunity.
    /// </summary>
    private async Task<string> GetOrgUnitNameForOpportunityAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var opportunityId))
            return "Unknown";

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        return opportunity?.ResponsibleOrgUnit?.Name ?? "Unknown";
    }

    #endregion
}
