namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

[Route("/")]
public class NotificationController : BaseController
{
    private readonly NotificationManager _notificationManager;

    public NotificationController(
        NotificationManager notificationManager, 
        UserResolverService<int> userResolverService,
        ILogger<NotificationController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _notificationManager = notificationManager;
    }

    [HttpGet(APIDictionary.Notifications)]
    public async Task<ActionResult<List<NotificationModel>>> GetNotifications()
    {
        return await HandleOperationAsync(async () =>
        {
            return await _notificationManager.GetNotifications(CurrentUserId);
        });
    }

    [HttpPut(APIDictionary.NotificationRead)]
    public async Task<ActionResult> MarkAsRead(int notificationId)
    {
        return await HandleOperationAsync(async () =>
        {
            await _notificationManager.MarkAsRead(notificationId, CurrentUserId);
        });
    }

    [HttpPut("api/notifications/{notificationId}/update")]
    public async Task<ActionResult> UpdateNotification(int notificationId, [FromBody] UpdateNotificationRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            await _notificationManager.UpdateNotification(notificationId, request.Message, request.Status);
        });
    }
} 