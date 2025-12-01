using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// UN Cooperation Framework (UNCF) Metadata entity
/// Data synced from External Data Service (ERP Database) - Read Only
/// Tracks metadata about UNCF files for specific countries including file URLs and version information
/// </summary>
public class UNCFMetadata : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// Name computed from Country and Version
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (C=Inactive, N=Active from source data)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// UNCF Metadata External ID - Maps to agrtid from source system
    /// </summary>
    public int? UNCFMetadataId { get; set; }
    
    /// <summary>
    /// Country code (ISO2 code)
    /// </summary>
    [MaxLength(10)]
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the UNCF file (typically Google Drive link)
    /// </summary>
    [MaxLength(1000)]
    public string? UNCFFileURL { get; set; }
    
    /// <summary>
    /// UN Cooperation Framework Version Number
    /// </summary>
    public int? UNCooperationFrameworkVersionNo { get; set; }
    
    /// <summary>
    /// UNCF Last Updated Date - When this metadata was last updated
    /// </summary>
    public DateTime? UNCFLastUpdatedDate { get; set; }
    
    /// <summary>
    /// UNCF File Name
    /// </summary>
    [MaxLength(500)]
    public string? UNCFFileName { get; set; }
}


