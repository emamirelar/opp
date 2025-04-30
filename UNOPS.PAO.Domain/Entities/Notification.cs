using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string RecordData { get; set; } = string.Empty; // JSON string of the record data
    public bool IsRead { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 