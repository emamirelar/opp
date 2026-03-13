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
using UNOPS.PAO.Models.Notifications;

public class NotificationManager : IApplicationService
{
    private readonly AppDbContext appDbContext;
    private readonly UserResolverService<int> userResolverService;

    public NotificationManager(AppDbContext appDbContext, UserResolverService<int> userResolverService)
    {
        this.appDbContext = appDbContext;
        this.userResolverService = userResolverService;
    }

    public async Task<List<NotificationModel>> GetNotifications(int userId, bool? unreadOnly = null)
    {
        var query = appDbContext.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationModel
        {
            Id = n.Id,
            Message = n.Message,
            Category = n.Category,
            ResponseType = n.ResponseType,
            Entity = n.Entity,
            EntityId = n.EntityId,
            Status = n.Status,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            Records = ParseRecordData(n.RecordData)
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

    /// <summary>
    /// Parse RecordData JSON string into List<object>, handling both array and object formats
    /// </summary>
    private static List<object> ParseRecordData(string recordData)
    {
        if (string.IsNullOrEmpty(recordData))
        {
            return new List<object>();
        }

        try
        {
            // Try to deserialize as array first (normal bulk import notifications)
            var asList = JsonSerializer.Deserialize<List<object>>(recordData);
            return asList ?? new List<object>();
        }
        catch (JsonException)
        {
            try
            {
                // If that fails, try to deserialize as single object (internal duplicates, errors, etc.)
                var asObject = JsonSerializer.Deserialize<object>(recordData);
                return asObject != null ? new List<object> { asObject } : new List<object>();
            }
            catch (JsonException)
            {
                // If all else fails, return the raw string as a single item
                return new List<object> { recordData };
            }
        }
    }
} 