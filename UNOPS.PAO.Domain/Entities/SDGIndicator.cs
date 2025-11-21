using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// SDG Indicator entity - represents specific indicators under each SDG Target
/// Data synced from External Data Service (ERP Database) - Read Only
/// All fields are strings for fallback and flexibility
/// </summary>
public class SDGIndicator : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// SDG Indicator Name (e.g., "1.1.1", "1.2.1")
    /// Maps to IBaseBusinessEntity.Name requirement
    /// Typically the indicator ID for display purposes
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
    /// SDG_Indicator_ID - External system identifier for the indicator
    /// Example: "1.1.1", "1.2.1", "1.2.2"
    /// </summary>
    [MaxLength(100)]
    public string? SDGIndicatorId { get; set; }
    
    /// <summary>
    /// SDG_Target_ID - Foreign key reference to parent SDG Target
    /// </summary>
    [MaxLength(100)]
    public string? SDGTargetId { get; set; }
    
    /// <summary>
    /// Indicator_Long_Description - Detailed description of the specific indicator
    /// Example: "Proportion of the population living below the international poverty line by sex, age, employment status and geographic location (urban/rural)"
    /// </summary>
    public string? SDGIndicatorLongDescription { get; set; }
}

