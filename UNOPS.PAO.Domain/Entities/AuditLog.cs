using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class AuditLog : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Entity type being audited
    /// </summary>
    public required string EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity record
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Action performed (create, update, delete, source_update, etc.)
    /// </summary>
    public required string Action { get; set; }
    
    /// <summary>
    /// Timestamp of the action
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// JSON data containing change details or additional context
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? JsonData { get; set; }
    
    /// <summary>
    /// Human-readable description of the action
    /// </summary>
    public string? Description { get; set; }
}

