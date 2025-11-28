using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Country master data entity
/// Data synced from External Data Service - Read Only
/// </summary>
public class Country : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// Country Name (e.g., "Kenya", "Afghanistan")
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "KE", "AF")
    /// </summary>
    [MaxLength(5)]
    public required string Iso2Code { get; set; }
    
    /// <summary>
    /// ISO 3166-1 alpha-3 country code (e.g., "KEN", "AFG") - Optional
    /// </summary>
    [MaxLength(3)]
    public string? Iso3Code { get; set; }
    
    /// <summary>
    /// Region Description (e.g., "Eastern and Southern Africa")
    /// </summary>
    [MaxLength(255)]
    public string? RegionDescription { get; set; }
    
    /// <summary>
    /// Continent Description (e.g., "Africa", "Asia", "Europe")
    /// </summary>
    [MaxLength(255)]
    public string? ContinentDescription { get; set; }
    
    // Computed properties for list/search operations
    [NotMapped]
    public int PartnerCount { get; set; } // Will be populated by service
    
    [NotMapped]
    public int LiaisonOfficeCount { get; set; } // Will be populated by service
    
    [NotMapped]
    public bool HasActiveUNCF { get; set; } // Will be populated by service - indicates if country has active UNCF metadata
}
