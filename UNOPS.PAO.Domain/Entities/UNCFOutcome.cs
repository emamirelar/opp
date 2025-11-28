using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// UN Cooperation Framework (UNCF) Outcome entity
/// Data synced from External Data Service (ERP Database) - Read Only
/// Represents outcomes defined in UN Cooperation Frameworks for specific countries
/// </summary>
public class UNCFOutcome : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// UNCF Outcome Name (computed or descriptive name)
    /// Maps to IBaseBusinessEntity.Name requirement
    /// Note: Some outcome names can be very long (up to 716 chars in source data)
    /// </summary>
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// UN Cooperation Framework Version Number
    /// </summary>
    public int? UNCooperationFrameworkVersionNo { get; set; }
    
    /// <summary>
    /// Country code or identifier
    /// </summary>
    [MaxLength(255)]
    public string? Country { get; set; }
    
    /// <summary>
    /// UNCF Outcome ID - External system identifier
    /// </summary>
    [MaxLength(255)]
    public string? UNCFOutcomeId { get; set; }

    /// <summary>
    /// UNCF Outcome Last Updated Date - When this outcome was last updated
    /// </summary>
    public DateTime? UNCFOutcomeLastUpdatedDate { get; set; }
}

