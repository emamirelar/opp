using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.DataAccess;
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
    public string ApproverRoleShort { get; init; } = "DoA2";
    public string OrgUnitName { get; init; } = string.Empty;
    public string OrgUnitIdAndDescription { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string RequestedByName { get; init; } = string.Empty;
    public string RequestedOn { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string CommentSection { get; init; } = string.Empty;
    public string EntityUrl { get; init; } = string.Empty;
    public string EntityStatementUrl { get; init; } = string.Empty;
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
    public string CommentSection { get; init; } = string.Empty;
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
    public string CommentSection { get; init; } = string.Empty;
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
    public string CommentSection { get; init; } = string.Empty;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;
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
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PaoWorkflowNotificationService> logger,
        IConfiguration configuration,
        NotificationManager notificationManager)
    {
        _emailSender = emailSender;
        _contextFactory = contextFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
        _notificationManager = notificationManager;
        _baseUrl = _configuration["AppConfig:BaseUrl"]
            ?? "https://opportunityplus.dev.unops.org";
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
            var orgUnitIdAndDescription = await GetOrgUnitIdAndDescriptionForOpportunityAsync(notification.EntityId);
            var approverRoleShort = await GetApproverRoleShortForOpportunityAsync(notification.EntityId);

            // Build CC recipient list (Opportunity Manager, initiator, Director/Manager)
            var ccRecipients = await BuildCCRecipientsAsync(notification);

            var commentSection = !string.IsNullOrEmpty(notification.Comment)
                ? $"<div class=\"comment-box\"><strong>Submitter's Remarks:</strong><br>{System.Net.WebUtility.HtmlEncode(notification.Comment)}</div>"
                : string.Empty;

            var emailModel = new ApprovalRequestEmailModel
            {
                ApproverName = string.Join(", ", recipientNames),
                ApproverRole = approverRoleShort == "DoA2" ? "DoA Level 2" : "DoA Level 3",
                ApproverRoleShort = approverRoleShort,
                OrgUnitName = orgUnitName,
                OrgUnitIdAndDescription = orgUnitIdAndDescription,
                EntityName = notification.EntityDisplayName,
                RequestedByName = notification.PerformedByUserName,
                RequestedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                CommentSection = commentSection,
                EntityUrl = $"{_baseUrl}/partnerships/opportunities/{notification.EntityId}",
                EntityStatementUrl = $"{_baseUrl}/partnerships/opportunities/{notification.EntityId}/statement"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowApprovalRequest.html",
                Title = $"Opportunity+: {notification.EntityDisplayName} - Action Required",
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
    /// Per matrix: TO = OM, Workflow Initiator; CC = Region Director, Region Deputy Director, Hub Director,
    /// Hub Deputy Director, OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit).
    /// </summary>
    public async Task NotifyWorkflowCompletedAsync(WorkflowNotification notification)
    {
        try
        {
            List<string> toEmails;
            List<string> ccEmails;

            if (notification.EntityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(notification.EntityId, out var opportunityId))
            {
                // TO: OM, Workflow Initiator
                var toUserIds = await GetRejectionRecipientUserIdsForOpportunityAsync(notification.EntityId);
                toEmails = await GetRecipientEmailsAsync(toUserIds);

                // CC: All hierarchy directors + DoA2, DoA3 (Responsible OrgUnit)
                ccEmails = await BuildApprovalCompleteCCRecipientsAsync(opportunityId);
            }
            else
            {
                toEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
                ccEmails = new List<string>();
            }

            if (!toEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow completed notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var toUserIdsForNames = notification.EntityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase)
                ? await GetRejectionRecipientUserIdsForOpportunityAsync(notification.EntityId)
                : notification.RecipientUserIds;
            var recipientNames = await GetRecipientNamesAsync(toUserIdsForNames);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var commentSection = !string.IsNullOrEmpty(notification.Comment)
                ? $"<div class=\"comment-box\"><strong>Approver's Comment:</strong><br>{System.Net.WebUtility.HtmlEncode(notification.Comment)}</div>"
                : string.Empty;

            var emailModel = new WorkflowCompletedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                ApprovedByName = notification.PerformedByUserName,
                ApprovedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                CommentSection = commentSection,
                EntityUrl = $"{_baseUrl}/partnerships/opportunities/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowCompleted.html",
                Title = $"Opportunity+: {notification.EntityDisplayName} - Go Decision Approved",
                EmailReceivers = toEmails.ToArray(),
                CcReceivers = ccEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow completed email for {EntityName} (ID: {EntityId}) to {ToCount} recipients with {CcCount} CC",
                notification.EntityDisplayName, notification.EntityId, toEmails.Count, ccEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow completed notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies the submitter that the opportunity has been set to NO GO.
    /// Per matrix: TO = OM, Workflow Initiator; CC = OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit).
    /// </summary>
    public async Task NotifyWorkflowRejectedAsync(WorkflowNotification notification)
    {
        try
        {
            List<string> toEmails;
            List<string> ccEmails;

            if (string.Equals(notification.EntityName, "Opportunity", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(notification.EntityId, out var opportunityId))
            {
                // TO: OM, Workflow Initiator
                var toUserIds = await GetRejectionRecipientUserIdsForOpportunityAsync(notification.EntityId);
                toEmails = await GetRecipientEmailsAsync(toUserIds);

                // CC: OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit)
                ccEmails = await BuildRecalledOrRejectedCCRecipientsAsync(opportunityId);
            }
            else
            {
                toEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
                ccEmails = new List<string>();
            }

            if (!toEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow rejected notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var toUserIdsForNames = string.Equals(notification.EntityName, "Opportunity", StringComparison.OrdinalIgnoreCase)
                ? await GetRejectionRecipientUserIdsForOpportunityAsync(notification.EntityId)
                : notification.RecipientUserIds;
            var recipientNames = await GetRecipientNamesAsync(toUserIdsForNames);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var commentSection = !string.IsNullOrEmpty(notification.Comment)
                ? $"<div class=\"comment-box\"><strong>Reason:</strong><br>{System.Net.WebUtility.HtmlEncode(notification.Comment)}</div>"
                : string.Empty;

            var emailModel = new WorkflowRejectedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                RejectedByName = notification.PerformedByUserName,
                RejectedOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                CommentSection = commentSection,
                EntityUrl = $"{_baseUrl}/partnerships/opportunities/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowRejected.html",
                Title = $"Opportunity+: {notification.EntityDisplayName} - Set to NO GO",
                EmailReceivers = toEmails.ToArray(),
                CcReceivers = ccEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow rejected (NO GO) email for {EntityName} (ID: {EntityId}) to {ToCount} recipients with {CcCount} CC",
                notification.EntityDisplayName, notification.EntityId, toEmails.Count, ccEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow rejected notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies that the Go Decision submission has been recalled.
    /// Per matrix: TO = OM, Workflow Initiator; CC = OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit).
    /// </summary>
    public async Task NotifyWorkflowRecalledAsync(WorkflowNotification notification)
    {
        try
        {
            List<string> toEmails;
            List<string> ccEmails;

            if (string.Equals(notification.EntityName, "Opportunity", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(notification.EntityId, out var opportunityId))
            {
                // TO: OM, Workflow Initiator
                var toUserIds = await GetRecallAdditionalRecipientUserIdsForOpportunityAsync(notification.EntityId);
                toEmails = await GetRecipientEmailsAsync(toUserIds);

                // CC: OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit)
                ccEmails = await BuildRecalledOrRejectedCCRecipientsAsync(opportunityId);
            }
            else
            {
                toEmails = await GetRecipientEmailsAsync(notification.RecipientUserIds);
                ccEmails = new List<string>();
            }

            if (!toEmails.Any())
            {
                _logger.LogWarning("No recipients found for workflow recalled notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            var toUserIdsForNames = string.Equals(notification.EntityName, "Opportunity", StringComparison.OrdinalIgnoreCase)
                ? await GetRecallAdditionalRecipientUserIdsForOpportunityAsync(notification.EntityId)
                : notification.RecipientUserIds;
            var recipientNames = await GetRecipientNamesAsync(toUserIdsForNames);
            var orgUnitName = await GetOrgUnitNameForOpportunityAsync(notification.EntityId);

            var commentSection = !string.IsNullOrEmpty(notification.Comment)
                ? $"<div class=\"comment-box\"><strong>Justification:</strong><br>{System.Net.WebUtility.HtmlEncode(notification.Comment)}</div>"
                : string.Empty;

            var emailModel = new WorkflowRecalledEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = notification.EntityDisplayName,
                OrgUnitName = orgUnitName,
                RecalledByName = notification.PerformedByUserName,
                RecalledOn = notification.Timestamp.ToString("dd MMM yyyy HH:mm"),
                Comment = notification.Comment,
                CommentSection = commentSection,
                EntityUrl = $"{_baseUrl}/partnerships/opportunities/{notification.EntityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowRecalled.html",
                Title = $"Opportunity+: {notification.EntityDisplayName} - Submission Recalled",
                EmailReceivers = toEmails.ToArray(),
                CcReceivers = ccEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent workflow recalled email for {EntityName} (ID: {EntityId}) to {ToCount} recipients with {CcCount} CC",
                notification.EntityDisplayName, notification.EntityId, toEmails.Count, ccEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow recalled notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies internal stakeholders from Implementation Country OrgUnits about an approved Go Decision.
    /// Per matrix: TO = Region Director, Region Deputy Director, Hub Director, Hub Deputy Director,
    /// OrgUnit Director, OrgUnit Deputy Director (Implementation Country OrgUnit only, excludes DoA1-DoA4);
    /// CC = OM, Workflow Initiator.
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

            // Get Implementation Country OrgUnits (org units responsible for these countries, excluding opportunity's own org unit)
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

            // TO: Directors and Deputy Directors only (excludes DoA1-DoA4) from Implementation Country OrgUnits
            var toUserIds = await context.EntityUserRoles
                .AsNoTracking()
                .Include(e => e.EntityRole)
                .Where(e => e.EntityType == "OrganizationHierarchy"
                         && orgUnitIds.Contains(e.EntityId)
                         && !e.IsDeleted
                         && e.EntityRole != null
                         && ImplementationCountryDirectorRoleCodes.Contains(e.EntityRole.Code))
                .Select(e => e.UserId)
                .Distinct()
                .ToListAsync();

            if (!toUserIds.Any())
            {
                _logger.LogInformation("No director/deputy director stakeholders found for implementation country org units for opportunity {OpportunityId}", opportunityId);
                return;
            }

            // CC: OM, Workflow Initiator
            var ccEmails = new List<string>();
            var omEmail = await GetOpportunityManagerEmailAsync(opportunityId.ToString());
            if (!string.IsNullOrEmpty(omEmail))
                ccEmails.Add(omEmail);

            var initiatorUserId = await GetInitiatorUserIdForRejectedOpportunityAsync(opportunityId.ToString());
            if (initiatorUserId.HasValue)
            {
                var initiatorEmail = await GetUserEmailAsync(initiatorUserId.Value);
                if (!string.IsNullOrEmpty(initiatorEmail) && !ccEmails.Contains(initiatorEmail, StringComparer.OrdinalIgnoreCase))
                    ccEmails.Add(initiatorEmail);
            }

            var toEmails = await GetRecipientEmailsAsync(toUserIds);
            if (!toEmails.Any())
            {
                return;
            }

            var recipientNames = await GetRecipientNamesAsync(toUserIds);

            var commentSection = "<div class=\"comment-box\"><strong>Approver's Comment:</strong><br>This opportunity has been approved for development and may affect countries in your area of responsibility.</div>";

            var emailModel = new WorkflowCompletedEmailModel
            {
                RecipientName = string.Join(", ", recipientNames),
                EntityName = opportunity.Name,
                OrgUnitName = opportunity.ResponsibleOrgUnit?.Name ?? "Unknown",
                ApprovedByName = approverName,
                ApprovedOn = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"),
                Comment = "This opportunity has been approved for development and may affect countries in your area of responsibility.",
                CommentSection = commentSection,
                EntityUrl = $"{_baseUrl}/partnerships/opportunities/{opportunityId}"
            };

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowCompleted.html",
                Title = $"Opportunity+: {opportunity.Name} - Go Decision Approved (FYI)",
                EmailReceivers = toEmails.ToArray(),
                CcReceivers = ccEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, emailModel, _baseUrl);

            _logger.LogInformation(
                "Sent internal stakeholder notification for opportunity {OpportunityId} to {ToCount} TO and {CcCount} CC recipients from {OrgUnitCount} org units",
                opportunityId, toEmails.Count, ccEmails.Count, orgUnitIds.Count);
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

    /// <summary>
    /// Gets the responsible org unit description (name) for an opportunity.
    /// Returns only the org unit name (no ID) for display in email notifications.
    /// </summary>
    private async Task<string> GetOrgUnitIdAndDescriptionForOpportunityAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var opportunityId))
            return "Unknown";

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        var orgUnit = opportunity?.ResponsibleOrgUnit;
        if (orgUnit == null)
            return "Unknown";

        return orgUnit.Name ?? "Unknown";
    }

    /// <summary>
    /// Gets the approver role short form (DoA2 or DoA3) for an opportunity's ResponsibleOrgUnit.
    /// Uses DoA2 if holders exist; otherwise DoA3.
    /// </summary>
    private async Task<string> GetApproverRoleShortForOpportunityAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var opportunityId))
            return "DoA2";

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        if (opportunity?.ResponsibleOrgUnitId == null)
            return "DoA2";

        var orgUnitId = opportunity.ResponsibleOrgUnitId.Value;

        var hasDoA2 = await context.Set<EntityUserRole>()
            .AsNoTracking()
            .Include(e => e.EntityRole)
            .AnyAsync(e => !e.IsDeleted &&
                e.EntityType == "OrganizationHierarchy" &&
                e.EntityId == orgUnitId &&
                e.EntityRole != null &&
                e.EntityRole.Code == "DoA2_OrganizationHierarchy");

        return hasDoA2 ? "DoA2" : "DoA3";
    }

    #endregion

    #region CC Recipient Methods

    /// <summary>
    /// Builds the CC recipient list for workflow approval request emails.
    /// Per matrix: OM, Workflow Initiator (if different from OM), OrgUnit Director, OrgUnit Deputy Director (Responsible OrgUnit only).
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

            // 3. Add OrgUnit Director and OrgUnit Deputy Director only (not Region/Hub Directors)
            if (int.TryParse(notification.EntityId, out var opportunityId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var opportunity = await context.Opportunities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

                if (opportunity?.ResponsibleOrgUnitId != null)
                {
                    var directorEmails = await GetRoleHolderEmailsForOrgUnitAsync(
                        opportunity.ResponsibleOrgUnitId.Value, OrgUnitDirectorRoleCodes);
                    foreach (var email in directorEmails.Where(e =>
                        !string.IsNullOrEmpty(e) && !ccRecipients.Contains(e, StringComparer.OrdinalIgnoreCase)))
                    {
                        ccRecipients.Add(email);
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
    /// Builds CC recipients for Approval Complete (GO) notifications.
    /// Per matrix: Region Director, Region Deputy Director, Hub Director, Hub Deputy Director,
    /// OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit).
    /// </summary>
    private async Task<List<string>> BuildApprovalCompleteCCRecipientsAsync(int opportunityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        if (opportunity?.ResponsibleOrgUnitId == null)
            return new List<string>();

        var orgUnitId = opportunity.ResponsibleOrgUnitId.Value;
        var roleCodes = AllHierarchyDirectorRoleCodes
            .Concat(new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" })
            .ToArray();

        return await GetRoleHolderEmailsForOrgUnitAsync(orgUnitId, roleCodes);
    }

    /// <summary>
    /// Builds CC recipients for Recalled and Rejected notifications.
    /// Per matrix: OrgUnit Director, OrgUnit Deputy Director, DoA2, DoA3 (Responsible OrgUnit).
    /// </summary>
    private async Task<List<string>> BuildRecalledOrRejectedCCRecipientsAsync(int opportunityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        if (opportunity?.ResponsibleOrgUnitId == null)
            return new List<string>();

        var orgUnitId = opportunity.ResponsibleOrgUnitId.Value;
        var roleCodes = OrgUnitDirectorRoleCodes
            .Concat(new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" })
            .ToArray();

        return await GetRoleHolderEmailsForOrgUnitAsync(orgUnitId, roleCodes);
    }

    /// <summary>
    /// Gets additional recipient user IDs for Opportunity recall notifications.
    /// Returns Opportunity Manager and initiator (when different from OM).
    /// Per matrix: these are the TO recipients for Recalled.
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>List of user IDs: OM + initiator (if different from OM)</returns>
    private async Task<List<int>> GetRecallAdditionalRecipientUserIdsForOpportunityAsync(string entityId)
    {
        var recipientIds = new List<int>();

        var omUserId = await GetOpportunityManagerUserIdAsync(entityId);
        if (omUserId.HasValue)
        {
            recipientIds.Add(omUserId.Value);
        }

        var initiatorUserId = await GetInitiatorUserIdForRecalledOpportunityAsync(entityId);
        if (initiatorUserId.HasValue && initiatorUserId != omUserId)
        {
            recipientIds.Add(initiatorUserId.Value);
        }

        return recipientIds.Distinct().ToList();
    }

    /// <summary>
    /// Gets the initiator user ID for a recalled Opportunity (the user who submitted for Go Decision).
    /// Queries the pending Submit workflow log - at recall time the task is not yet closed.
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>The initiator's user ID, or null if not found</returns>
    private async Task<int?> GetInitiatorUserIdForRecalledOpportunityAsync(string entityId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

        var submitLog = await context.WorkflowLogs
            .AsNoTracking()
            .Where(w => !w.IsDeleted
                     && w.EntityName == "Opportunity"
                     && w.EntityId == entityId
                     && w.Action == "Submit"
                     && w.CompletedOn == null)
            .OrderByDescending(w => w.CreatedDate)
            .FirstOrDefaultAsync();

        return submitLog != null ? submitLog.UserId : null;
    }

    /// <summary>
    /// Gets the recipient user IDs for Opportunity rejection notifications.
    /// Returns only the Opportunity Manager and the initiator (when different from OM).
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>List of user IDs: OM + initiator (if different from OM)</returns>
    private async Task<List<int>> GetRejectionRecipientUserIdsForOpportunityAsync(string entityId)
    {
        var recipientIds = new List<int>();

        var omUserId = await GetOpportunityManagerUserIdAsync(entityId);
        if (omUserId.HasValue)
        {
            recipientIds.Add(omUserId.Value);
        }

        var initiatorUserId = await GetInitiatorUserIdForRejectedOpportunityAsync(entityId);
        if (initiatorUserId.HasValue && initiatorUserId != omUserId)
        {
            recipientIds.Add(initiatorUserId.Value);
        }

        return recipientIds.Distinct().ToList();
    }

    /// <summary>
    /// Gets the Opportunity Manager's user ID for the specified opportunity.
    /// Queries stakeholders with the "Opportunity_Manager_Opportunity" role.
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>The Opportunity Manager's user ID, or null if not found</returns>
    private async Task<int?> GetOpportunityManagerUserIdAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var opportunityId))
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();

        var omStakeholder = await context.OpportunityStakeholders
            .AsNoTracking()
            .Include(s => s.EntityRole)
            .Where(s => !s.IsDeleted
                     && s.OpportunityId == opportunityId
                     && s.EntityRole != null
                     && s.EntityRole.Code == "Opportunity_Manager_Opportunity")
            .FirstOrDefaultAsync();

        return omStakeholder?.UserId;
    }

    /// <summary>
    /// Gets the initiator user ID for a rejected Opportunity (the user who submitted for Go Decision).
    /// Queries the most recent Submit workflow log for this entity.
    /// </summary>
    /// <param name="entityId">The opportunity ID as a string</param>
    /// <returns>The initiator's user ID, or null if not found</returns>
    private async Task<int?> GetInitiatorUserIdForRejectedOpportunityAsync(string entityId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

        var submitLog = await context.WorkflowLogs
            .AsNoTracking()
            .Where(w => !w.IsDeleted
                     && w.EntityName == "Opportunity"
                     && w.EntityId == entityId
                     && w.Action == "Submit"
                     && w.CompletedOn != null)
            .OrderByDescending(w => w.CompletedOn)
            .FirstOrDefaultAsync();

        return submitLog != null ? submitLog.UserId : null;
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
    /// Gets email addresses for users with the specified role codes on an org unit.
    /// </summary>
    private async Task<List<string>> GetRoleHolderEmailsForOrgUnitAsync(int orgUnitId, params string[] roleCodes)
    {
        if (roleCodes.Length == 0)
            return new List<string>();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var emails = await context.EntityUserRoles
            .AsNoTracking()
            .Include(eur => eur.User)
            .Include(eur => eur.EntityRole)
            .Where(eur => eur.EntityType == "OrganizationHierarchy"
                       && eur.EntityId == orgUnitId
                       && !eur.IsDeleted
                       && eur.EntityRole != null
                       && roleCodes.Contains(eur.EntityRole.Code)
                       && eur.User != null
                       && !string.IsNullOrEmpty(eur.User.Email))
            .Select(eur => eur.User!.Email!)
            .Distinct()
            .ToListAsync();

        return emails;
    }

    /// <summary>
    /// Role codes for OrgUnit Director and OrgUnit Deputy Director only (Approval Request, Recalled, Rejected CC).
    /// </summary>
    private static readonly string[] OrgUnitDirectorRoleCodes =
    {
        "OrgUnit_Director_OrganizationHierarchy",
        "OrgUnit_Deputy_Director_OrganizationHierarchy"
    };

    /// <summary>
    /// Role codes for all hierarchy directors (Approval Complete CC).
    /// </summary>
    private static readonly string[] AllHierarchyDirectorRoleCodes =
    {
        "Regional_Director_OrganizationHierarchy",
        "Regional_Deputy_Director_OrganizationHierarchy",
        "MCO_Director_OrganizationHierarchy",
        "MCO_Deputy_Director_OrganizationHierarchy",
        "OrgUnit_Director_OrganizationHierarchy",
        "OrgUnit_Deputy_Director_OrganizationHierarchy"
    };

    /// <summary>
    /// Role codes for Implementation Country OrgUnit directors only (Internal Stakeholder FYI TO).
    /// Excludes DoA1-DoA4.
    /// </summary>
    private static readonly string[] ImplementationCountryDirectorRoleCodes =
    {
        "Regional_Director_OrganizationHierarchy",
        "Regional_Deputy_Director_OrganizationHierarchy",
        "MCO_Director_OrganizationHierarchy",
        "MCO_Deputy_Director_OrganizationHierarchy",
        "OrgUnit_Director_OrganizationHierarchy",
        "OrgUnit_Deputy_Director_OrganizationHierarchy"
    };

    #endregion
}
