using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Generic entity to track all email notifications sent by the system
/// </summary>
public class EmailNotificationLog : BaseEntity
{
    public new int Id { get; set; }
    
    // Recipient information
    public int? RecipientUserId { get; set; }
    [MaxLength(255)]
    public string? RecipientEmail { get; set; }
    [MaxLength(255)]
    public string? RecipientName { get; set; }
    
    // Email details
    [MaxLength(500)]
    public string? EmailSubject { get; set; }
    [MaxLength(100)]
    public string NotificationType { get; set; } = string.Empty; // e.g., "DueDiligenceExpiry", "ContractReminder", etc.
    
    // Timing
    public DateTime SentAt { get; set; }
    public DateTime? ScheduledFor { get; set; } // For future scheduled notifications
    
    // Related entity information (flexible for any entity type)
    public int? RelatedEntityId { get; set; } // Could be PartnerId, ContractId, etc.
    [MaxLength(100)]
    public string? RelatedEntityType { get; set; } // "Partner", "Contract", "Project", etc.
    [MaxLength(255)]
    public string? RelatedEntityName { get; set; } // Partner name, contract title, etc.
    
    // Notification-specific data (stored as JSON for flexibility)
    public string? NotificationData { get; set; } // JSON string for notification-specific fields
    
    // Status tracking
    public bool IsSuccessful { get; set; } = true;
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual PAOUser? RecipientUser { get; set; }
}
