using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Notifications;

public class UpdateNotificationRequest
{
    public string Message { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Progress;
} 