using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using System.Text.Json;

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
/// Also creates in-system notifications for the notification bell.
/// Uses DbContextFactory to create separate context instances for each operation,
/// avoiding DbContext concurrency issues with other async workflow operations.
/// </summary>
public class PaoWorkflowNotificationService : IWorkflowNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<PaoWorkflowNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly NotificationManager _notificationManager;
    private readonly string _baseUrl;

    /// <summary>
    /// Category identifier for workflow approval notifications.
    /// Used to identify and mark as done when decision is made.
    /// </summary>
    public const string WorkflowApprovalCategory = "workflow_approval";

    public PaoWorkflowNotificationService(
        IEmailSender emailSender,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<PaoWorkflowNotificationService> logger,
        IConfiguration configuration,
        NotificationManager notificationManager)
    {
        _emailSender = emailSender;
        _contextFactory = contextFactory;
        _logger = logger;
        _configuration = configuration;
        _notificationManager = notificationManager;
        _baseUrl = _configuration["AppBaseUrl"] ?? "https://pao.unops.org";
    }

    /// <summary>
    /// Notifies DoA Level 2 holders about a new Go Decision requiring their attention.
    /// Creates both email notifications and in-system notifications for the notification bell.
    /// Email includes CC recipients: Opportunity Manager, workflow initiator, Director/Manager.
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

            // Build CC recipient list (Opportunity Manager, initiator, Director/Manager)
            var ccRecipients = await BuildCCRecipientsAsync(notification);

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
                EmailReceivers = recipientEmails.ToArray(),
                CcReceivers = ccRecipients.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent approval request email for {EntityName} (ID: {EntityId}) to {RecipientCount} recipients with {CcCount} CC recipients",
                notification.EntityDisplayName, notification.EntityId, recipientEmails.Count, ccRecipients.Count);

            // Create in-system notifications for each approver
            await CreateInSystemNotificationsAsync(notification, orgUnitName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval request notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Creates in-system notifications for workflow approval requests.
    /// These appear in the notification bell and Actions Required card.
    /// </summary>
    private async Task CreateInSystemNotificationsAsync(WorkflowNotification notification, string orgUnitName)
    {
        try
        {
            if (!int.TryParse(notification.EntityId, out var entityId))
            {
                _logger.LogWarning("Invalid entity ID for in-system notification: {EntityId}", notification.EntityId);
                return;
            }

            var notificationMessage = $"Go Decision approval required for \"{notification.EntityDisplayName}\" ({orgUnitName})";

            // Create a notification record with entity reference for navigation
            var notificationData = new
            {
                entityName = notification.EntityName,
                entityId = entityId,
                entityDisplayName = notification.EntityDisplayName,
                orgUnitName = orgUnitName,
                requestedBy = notification.PerformedByUserName,
                requestedOn = notification.Timestamp.ToString("o"),
                pendingStage = "GO"
            };

            foreach (var userId in notification.RecipientUserIds)
            {
                await CreateWorkflowNotificationAsync(
                    userId,
                    notificationMessage,
                    notification.EntityName,
                    entityId,
                    notificationData);
            }

            _logger.LogInformation(
                "Created {Count} in-system notifications for workflow approval request on {EntityName} (ID: {EntityId})",
                notification.RecipientUserIds.Count, notification.EntityName, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create in-system notifications for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
            // Don't rethrow - email was sent successfully, in-system notification failure is non-critical
        }
    }

    /// <summary>
    /// Creates a single workflow approval notification.
    /// </summary>
    private async Task CreateWorkflowNotificationAsync(int userId, string message, string entityName, int entityId, object recordData)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Category = WorkflowApprovalCategory,
            ResponseType = "action_required",
            Entity = entityName,
            EntityId = entityId,
            RecordData = JsonSerializer.Serialize(recordData),
            IsRead = false,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await context.Notifications.AddAsync(notification);
        await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var opportunity = await context.Opportunities
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
            var orgUnitIds = await context.OrganizationUnitRelationships
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
            var stakeholderUserIds = await context.EntityUserRoles
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

    #region In-System Notification Management

    /// <summary>
    /// Marks workflow approval notifications as done when a decision is made.
    /// Called when an opportunity is approved, rejected, or recalled.
    /// </summary>
    /// <param name="entityName">The entity type (e.g., "Opportunity")</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="decisionMessage">Optional message describing the decision</param>
    public async Task MarkWorkflowNotificationsAsDoneAsync(string entityName, int entityId, string? decisionMessage = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Find all pending workflow_approval notifications for this entity
            var notifications = await context.Notifications
                .Where(n => n.Category == WorkflowApprovalCategory 
                         && n.Entity == entityName 
                         && n.EntityId == entityId
                         && n.Status == NotificationStatus.Pending)
                .ToListAsync();

            if (!notifications.Any())
            {
                _logger.LogDebug("No pending workflow notifications found for {EntityName} {EntityId}", entityName, entityId);
                return;
            }

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Done;
                notification.IsRead = true;
                
                if (!string.IsNullOrEmpty(decisionMessage))
                {
                    notification.Message = $"{notification.Message} - {decisionMessage}";
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Marked {Count} workflow notifications as done for {EntityName} (ID: {EntityId})",
                notifications.Count, entityName, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark workflow notifications as done for {EntityName} {EntityId}",
                entityName, entityId);
            // Don't rethrow - notification update failure is non-critical
        }
    }

    /// <summary>
    /// Marks workflow approval notifications as done with "Approved" status message.
    /// </summary>
    /// <param name="entityName">The entity type (e.g., "Opportunity")</param>
    /// <param name="entityId">The entity ID</param>
    public async Task MarkWorkflowNotificationsAsApprovedAsync(string entityName, int entityId)
    {
        await MarkWorkflowNotificationsAsDoneAsync(entityName, entityId, "Approved");
    }

    /// <summary>
    /// Marks workflow approval notifications as done with "Rejected" status message.
    /// </summary>
    /// <param name="entityName">The entity type (e.g., "Opportunity")</param>
    /// <param name="entityId">The entity ID</param>
    public async Task MarkWorkflowNotificationsAsRejectedAsync(string entityName, int entityId)
    {
        await MarkWorkflowNotificationsAsDoneAsync(entityName, entityId, "Set to NO GO");
    }

    /// <summary>
    /// Marks workflow approval notifications as done with "Recalled" status message.
    /// </summary>
    /// <param name="entityName">The entity type (e.g., "Opportunity")</param>
    /// <param name="entityId">The entity ID</param>
    public async Task MarkWorkflowNotificationsAsRecalledAsync(string entityName, int entityId)
    {
        await MarkWorkflowNotificationsAsDoneAsync(entityName, entityId, "Recalled");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets email addresses for the specified user IDs.
    /// </summary>
    private async Task<List<string>> GetRecipientEmailsAsync(List<int> userIds)
    {
        if (!userIds.Any())
            return new List<string>();

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.PAOUsers
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

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.PAOUsers
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

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        return opportunity?.ResponsibleOrgUnit?.Name ?? "Unknown";
    }

    #endregion

    #region CC Recipient Methods

    /// <summary>
    /// Builds the CC recipient list for workflow approval request emails.
    /// Includes: Opportunity Manager, workflow initiator (if different), Director/Manager of org unit.
    /// </summary>
    /// <param name="notification">The workflow notification containing entity and submitter info</param>
    /// <returns>List of email addresses for CC recipients (deduplicated)</returns>
    private async Task<List<string>> BuildCCRecipientsAsync(WorkflowNotification notification)
    {
        var ccRecipients = new List<string>();

        // Only add CC for Opportunity entities
        if (!notification.EntityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase))
            return ccRecipients;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // 1. Add Opportunity Manager email
            var omEmail = await GetOpportunityManagerEmailAsync(notification.EntityId);
            if (!string.IsNullOrEmpty(omEmail))
            {
                ccRecipients.Add(omEmail);
            }

            // 2. Add workflow initiator email (if different from OM)
            if (notification.PerformedByUserId > 0)
            {
                var initiatorEmail = await GetUserEmailAsync(notification.PerformedByUserId);
                if (!string.IsNullOrEmpty(initiatorEmail) && 
                    !ccRecipients.Contains(initiatorEmail, StringComparer.OrdinalIgnoreCase))
                {
                    ccRecipients.Add(initiatorEmail);
                }
            }

            // 3. Add Director/Manager of org unit
            if (int.TryParse(notification.EntityId, out var opportunityId))
            {
                var opportunity = await context.Opportunities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

                if (opportunity?.ResponsibleOrgUnitId != null)
                {
                    var directorEmail = await GetDirectorManagerEmailAsync(opportunity.ResponsibleOrgUnitId.Value);
                    if (!string.IsNullOrEmpty(directorEmail) && 
                        !ccRecipients.Contains(directorEmail, StringComparer.OrdinalIgnoreCase))
                    {
                        ccRecipients.Add(directorEmail);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error building CC recipients for entity {EntityName} {EntityId}, proceeding without CC",
                notification.EntityName, notification.EntityId);
        }

        return ccRecipients.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Gets the Opportunity Manager's email for the specified opportunity.
    /// Queries stakeholders with the "Opportunity_Manager_Opportunity" role.
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>The Opportunity Manager's email, or null if not found</returns>
    private async Task<string?> GetOpportunityManagerEmailAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var opportunityId))
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var omStakeholder = await context.OpportunityStakeholders
            .AsNoTracking()
            .Include(s => s.EntityRole)
            .Include(s => s.User)
            .Where(s => s.OpportunityId == opportunityId
                     && s.EntityRole != null
                     && s.EntityRole.Code == "Opportunity_Manager_Opportunity"
                     && s.User != null)
            .FirstOrDefaultAsync();

        return omStakeholder?.User?.Email;
    }

    /// <summary>
    /// Gets the email address for a single user by ID.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The user's email, or null if not found</returns>
    private async Task<string?> GetUserEmailAsync(int userId)
    {
        if (userId <= 0)
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var user = await context.PAOUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Email;
    }

    /// <summary>
    /// Gets the Director/Manager email for the specified org unit.
    /// Queries EntityUserRole for Director roles in priority order.
    /// </summary>
    /// <param name="orgUnitId">The organization unit ID</param>
    /// <returns>The Director/Manager's email, or null if not found</returns>
    private async Task<string?> GetDirectorManagerEmailAsync(int orgUnitId)
    {
        var directorRoleCodes = new[]
        {
            "OrgUnit_Director_OrganizationHierarchy",
            "OrgUnit_Deputy_Director_OrganizationHierarchy",
            "Regional_Director_OrganizationHierarchy",
            "Regional_Deputy_Director_OrganizationHierarchy",
            "MCO_Director_OrganizationHierarchy",
            "MCO_Deputy_Director_OrganizationHierarchy"
        };

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var directorRole = await context.EntityUserRoles
            .AsNoTracking()
            .Include(eur => eur.User)
            .Include(eur => eur.EntityRole)
            .Where(eur => eur.EntityType == "OrganizationHierarchy"
                       && eur.EntityId == orgUnitId
                       && !eur.IsDeleted
                       && eur.EntityRole != null
                       && directorRoleCodes.Contains(eur.EntityRole.Code))
            .FirstOrDefaultAsync();

        return directorRole?.User?.Email;
    }

    #endregion
}
