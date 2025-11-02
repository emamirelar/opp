using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Notifications;

public class NotificationModel
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public List<object> Records { get; set; } = new();
    public string? Entity { get; set; } // Entity type for navigation
    public int? EntityId { get; set; } // Entity ID for navigation
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
} 