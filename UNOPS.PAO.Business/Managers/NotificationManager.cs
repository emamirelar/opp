namespace UNOPS.PAO.Business.Managers;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Interfaces;
using UNOPS.PAO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Enums;

public class NotificationManager : IApplicationService
{
    private readonly AppDbContext appDbContext;
    private readonly UserResolverService<int> userResolverService;

    public NotificationManager(AppDbContext appDbContext, UserResolverService<int> userResolverService)
    {
        this.appDbContext = appDbContext;
        this.userResolverService = userResolverService;
    }

    public async Task<List<NotificationModel>> GetNotifications(int userId)
    {
        var notifications = await appDbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationModel
        {
            Id = n.Id,
            Message = n.Message,
            Category = n.Category,
            ResponseType = n.ResponseType,
            Records = JsonSerializer.Deserialize<List<object>>(n.RecordData) ?? new List<object>()
        }).ToList();
    }

    public async Task MarkAsRead(int notificationId, int userId)
    {
        var notification = await appDbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await appDbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateNotification(int notificationId, string message, NotificationStatus status)
    {
        var notification = await appDbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification != null)
        {
            notification.Message = message;
            notification.Status = status;
            await appDbContext.SaveChangesAsync();
        }
    }

    public async Task CreateNotification(int userId, string message, string category, string responseType, object record)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Category = category,
            ResponseType = responseType,
            RecordData = JsonSerializer.Serialize(new List<object> { record }),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await appDbContext.Notifications.AddAsync(notification);
        await appDbContext.SaveChangesAsync();
    }
} 