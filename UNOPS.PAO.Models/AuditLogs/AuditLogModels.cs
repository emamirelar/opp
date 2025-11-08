namespace UNOPS.PAO.Models.AuditLogs;

/// <summary>
/// Request model for creating an audit log entry
/// </summary>
public class AuditLogCreateRequest
{
    public required string EntityType { get; set; }
    public required int EntityId { get; set; }
    public required string Action { get; set; }
    public required int UserId { get; set; }
    public string? JsonData { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Response model for audit log
/// </summary>
public class AuditLogModel
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public string? JsonData { get; set; }
    public string? Description { get; set; }
}

