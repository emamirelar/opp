namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    [HttpGet(APIDictionary.Notifications)]
    public async Task<ActionResult<List<NotificationModel>>> GetNotifications()
    {
        try
        {
            var notifications = await _notificationManager.GetNotifications(CurrentUserId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", CurrentUserId);
            return StatusCode(500, new { error = "An error occurred while retrieving notifications" });
        }
    }

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