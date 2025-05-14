namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class NotificationController : ControllerBase
{
    private readonly NotificationManager notificationManager;
    private readonly UserResolverService<int> userResolverService;

    public NotificationController(NotificationManager notificationManager, UserResolverService<int> userResolverService)
    {
        this.notificationManager = notificationManager;
        this.userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.Notifications)]
    public async Task<ActionResult<List<NotificationModel>>> GetNotifications()
    {
        var userId = userResolverService.GetCurrentUserId();
        var notifications = await notificationManager.GetNotifications(userId);
        return Ok(notifications);
    }

    [HttpPut(APIDictionary.NotificationRead)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = userResolverService.GetCurrentUserId();
        await notificationManager.MarkAsRead(notificationId, userId);
        return Ok();
    }

    [HttpPut("api/notifications/{notificationId}/update")]
    public async Task<IActionResult> UpdateNotification(int notificationId, [FromBody] UpdateNotificationRequest request)
    {
        await notificationManager.UpdateNotification(notificationId, request.Message, request.Status);
        return Ok();
    }
} 