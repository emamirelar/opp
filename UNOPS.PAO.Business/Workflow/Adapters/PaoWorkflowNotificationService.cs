using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IWorkflowNotificationService.
/// Sends workflow-related email notifications using PAO's email infrastructure.
/// Note: Currently logs notifications - email templates can be added later.
/// </summary>
public class PaoWorkflowNotificationService : IWorkflowNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;
    private readonly ILogger<PaoWorkflowNotificationService> _logger;

    public PaoWorkflowNotificationService(
        IEmailSender emailSender,
        AppDbContext context,
        ILogger<PaoWorkflowNotificationService> logger)
    {
        _emailSender = emailSender;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Notifies users about a new workflow action requiring their attention.
    /// </summary>
    public async Task NotifyNewApprovalRequestAsync(WorkflowNotification notification)
    {
        try
        {
            var recipients = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipients.Any())
            {
                _logger.LogWarning("No recipients found for approval request notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            // TODO: Create email template for workflow approval requests
            // For now, log the notification details
            _logger.LogInformation(
                "Workflow approval request: Entity={EntityName} Id={EntityId} Action={Action} " +
                "FromStage={FromStage} ToStage={ToStage} PerformedBy={PerformedBy} Recipients={Recipients}",
                notification.EntityName, notification.EntityId, notification.Action,
                notification.FromStage, notification.ToStage, notification.PerformedByUserName,
                string.Join(", ", recipients));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval request notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies the submitter that their workflow action was completed.
    /// </summary>
    public async Task NotifyWorkflowCompletedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipients = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipients.Any())
            {
                _logger.LogWarning("No recipients found for workflow completed notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            // TODO: Create email template for workflow completed
            _logger.LogInformation(
                "Workflow completed: Entity={EntityName} Id={EntityId} NewStage={ToStage} " +
                "ApprovedBy={PerformedBy} Recipients={Recipients}",
                notification.EntityName, notification.EntityId, notification.ToStage,
                notification.PerformedByUserName, string.Join(", ", recipients));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow completed notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies the submitter that their workflow action was rejected.
    /// </summary>
    public async Task NotifyWorkflowRejectedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipients = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipients.Any())
            {
                _logger.LogWarning("No recipients found for workflow rejected notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            // TODO: Create email template for workflow rejected
            _logger.LogInformation(
                "Workflow rejected: Entity={EntityName} Id={EntityId} RequestedStage={ToStage} " +
                "RejectedBy={PerformedBy} Reason={Comment} Recipients={Recipients}",
                notification.EntityName, notification.EntityId, notification.ToStage,
                notification.PerformedByUserName, notification.Comment, string.Join(", ", recipients));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow rejected notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Notifies approvers that a workflow has been recalled.
    /// </summary>
    public async Task NotifyWorkflowRecalledAsync(WorkflowNotification notification)
    {
        try
        {
            var recipients = await GetRecipientEmailsAsync(notification.RecipientUserIds);
            if (!recipients.Any())
            {
                _logger.LogWarning("No recipients found for workflow recalled notification for entity {EntityName} {EntityId}",
                    notification.EntityName, notification.EntityId);
                return;
            }

            // TODO: Create email template for workflow recalled
            _logger.LogInformation(
                "Workflow recalled: Entity={EntityName} Id={EntityId} " +
                "RecalledBy={PerformedBy} Reason={Comment} Recipients={Recipients}",
                notification.EntityName, notification.EntityId,
                notification.PerformedByUserName, notification.Comment, string.Join(", ", recipients));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow recalled notification for entity {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    /// <summary>
    /// Gets email addresses for the specified user IDs.
    /// </summary>
    private async Task<List<string>> GetRecipientEmailsAsync(List<int> userIds)
    {
        if (!userIds.Any())
            return new List<string>();

        return await _context.PAOUsers
            .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
            .Select(u => u.Email!)
            .ToListAsync();
    }
}
