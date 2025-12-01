using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents a UNOPS Strategic Mission
/// Predefined reference data entity - follows same pattern as SDG, UNCFOutcome
/// </summary>
public class UNOPSMission : IBaseBusinessEntity<int>
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Mission name (e.g., "Climate, Biodiversity, and Pollution")
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [Required]
    [MaxLength(500)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Entity status (reference data defaults to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    /// <summary>
    /// Soft delete flag (for reference data management)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Unique code for the mission (e.g., 'CLIMATE_BIODIVERSITY')
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Code { get; set; }

    /// <summary>
    /// Display order for UI presentation
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Full description of the mission
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., 'pi pi-globe')
    /// </summary>
    [MaxLength(50)]
    public string? IconClass { get; set; }

    /// <summary>
    /// Navigation property for opportunities aligned to this mission
    /// </summary>
    public virtual ICollection<OpportunityUNOPSMission> Opportunities { get; set; } = new List<OpportunityUNOPSMission>();
}

