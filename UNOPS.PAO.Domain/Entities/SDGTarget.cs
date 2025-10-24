using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// SDG Target entity - represents specific targets under each SDG
/// Data synced from External Data Service (ERP Database) - Read Only
/// All fields are strings for fallback and flexibility
/// </summary>
public class SDGTarget : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// SDG Target Name (computed or descriptive name)
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// SDG_Target_ID - External system identifier for the target
    /// </summary>
    [MaxLength(100)]
    public string? SDGTargetId { get; set; }
    
    /// <summary>
    /// SDG_ID - Foreign key reference to parent SDG
    /// </summary>
    [MaxLength(100)]
    public string? SDGId { get; set; }
    
    /// <summary>
    /// Target_Description - Description of the specific target
    /// </summary>
    [MaxLength(2000)]
    public string? TargetDescription { get; set; }
    
    /// <summary>
    /// Target_Type - Type or category of the target
    /// </summary>
    [MaxLength(100)]
    public string? TargetType { get; set; }
}

