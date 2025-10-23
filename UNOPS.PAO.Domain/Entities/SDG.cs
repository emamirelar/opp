using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// UN Sustainable Development Goal entity
/// Data synced from External Data Service (ERP Database) - Read Only
/// All fields are strings for fallback and flexibility
/// </summary>
public class SDG : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// SDG Name (e.g., "No Poverty", "Zero Hunger")
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
    /// SDG ID - External system identifier
    /// </summary>
    [MaxLength(100)]
    public string? SDGId { get; set; }
    
    /// <summary>
    /// SDG Number (e.g., "1", "2", "3"... up to "17")
    /// Stored as string for fallback
    /// </summary>
    [MaxLength(50)]
    public string? SDGNumber { get; set; }
    
    /// <summary>
    /// SDG Description - Short description of the goal
    /// </summary>
    [MaxLength(1000)]
    public string? SDGDescription { get; set; }
    
    /// <summary>
    /// SDG Logo - URL or path to the SDG logo image
    /// </summary>
    [MaxLength(500)]
    public string? SDGLogo { get; set; }
    
    /// <summary>
    /// SDG Long Description - Detailed description of the goal
    /// </summary>
    public string? SDGLongDescription { get; set; }
}

