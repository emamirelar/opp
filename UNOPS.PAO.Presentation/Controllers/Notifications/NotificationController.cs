namespace UNOPS.PAO.Presentation.Controllers.Notifications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Notifications;

[Route("/")]
[Authorize] // Basic authorization to ensure user is logged in, but no permission checks
public class NotificationController : ControllerBase
{
    private readonly NotificationManager _notificationManager;
    private readonly UserResolverService<int> _userResolverService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        NotificationManager notificationManager, 
        UserResolverService<int> userResolverService,
        ILogger<NotificationController> logger)
    {
        _notificationManager = notificationManager;
        _userResolverService = userResolverService;
        _logger = logger;
    }

    private int CurrentUserId => _userResolverService.GetCurrentUserId();

    /// <summary>
    /// Retrieves all notifications for the current user with optional filtering for unread notifications only.
    /// </summary>
    /// <param name="unreadOnly">Optional filter to show only unread notifications (null returns all)</param>
    /// <example_uses>
    /// Show me all my notifications
    /// Get only unread notifications
    /// List all system alerts and messages
    /// Show notification history
    /// Get latest notifications for user
    /// Check for new messages and alerts
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see notifications, alerts, messages, or wants to check for new system updates.</when_to_use>
    /// <returns>List of notifications with read/unread status and content</returns>
    [HttpGet(APIDictionary.Notifications)]
    public async Task<ActionResult<List<NotificationModel>>> GetNotifications([FromQuery] bool? unreadOnly = null)
    {
        try
        {
            var notifications = await _notificationManager.GetNotifications(CurrentUserId, unreadOnly);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", CurrentUserId);
            return StatusCode(500, new { error = "An error occurred while retrieving notifications" });
        }
    }

    /// <summary>
    /// Marks a specific notification as read for the current user to update notification status.
    /// </summary>
    /// <param name="notificationId">Notification ID to mark as read</param>
    /// <example_uses>
    /// Mark notification 123 as read
    /// Read notification about system update
    /// Clear unread status for alert 456
    /// Mark message as seen
    /// Update notification read status
    /// </example_uses>
    /// <when_to_use>Use this when the user opens, reads, or acknowledges a notification to update its status.</when_to_use>
    /// <returns>No content on successful status update</returns>
    [HttpPut(APIDictionary.NotificationRead)]
    public async Task<ActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            await _notificationManager.MarkAsRead(notificationId, CurrentUserId);
            return NoContent(); // 204 No Content for successful void operations
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", 
                notificationId, CurrentUserId);
            return StatusCode(500, new { error = "An error occurred while updating notification" });
        }
    }

    /// <summary>
    /// Updates a notification's content and status for administrative or system management purposes.
    /// </summary>
    /// <param name="notificationId">Notification ID to update</param>
    /// <param name="request">Update request containing new message and status</param>
    /// <param name="request.message">Updated notification message content</param>
    /// <param name="request.status">Updated notification status</param>
    /// <example_uses>
    /// Update notification 123's message content
    /// Change notification status to resolved
    /// Modify system alert text
    /// Update notification priority level
    /// Change notification message and status
    /// </example_uses>
    /// <when_to_use>Use this when administrators need to update notification content or status for system management.</when_to_use>
    /// <returns>No content on successful update</returns>
    [HttpPut("api/notifications/{notificationId}/update")]
    public async Task<ActionResult> UpdateNotification(int notificationId, [FromBody] UpdateNotificationRequest request)
    {
        try
        {
            await _notificationManager.UpdateNotification(notificationId, request.Message, request.Status);
            return NoContent(); // 204 No Content for successful void operations
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification {NotificationId}", notificationId);
            return StatusCode(500, new { error = "An error occurred while updating notification" });
        }
    }
} 