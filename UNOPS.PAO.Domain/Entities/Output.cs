using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents UNOPS Products and Services List with hierarchical structure (Level 0-4).
/// Based on the official UNOPS Products and Services taxonomy.
/// </summary>
public class Output : ModifiableDeletableEntity<int, int>
{
    /// <summary>
    /// Level 0 - Top level category (e.g., "Project management-related services", "Infrastructure-related services")
    /// </summary>
    [MaxLength(500)]
    public string? Level0 { get; set; }
    
    /// <summary>
    /// Level 1 - Primary subcategory
    /// </summary>
    [MaxLength(500)]
    public string? Level1 { get; set; }
    
    /// <summary>
    /// Definition/description for Level 1
    /// </summary>
    [MaxLength(4000)]
    public string? DefinitionLevel1 { get; set; }
    
    /// <summary>
    /// Level 2 - Secondary subcategory
    /// </summary>
    [MaxLength(500)]
    public string? Level2 { get; set; }
    
    /// <summary>
    /// Definition/description for Level 2
    /// </summary>
    [MaxLength(4000)]
    public string? DefinitionLevel2 { get; set; }
    
    /// <summary>
    /// Level 3 - Tertiary subcategory
    /// </summary>
    [MaxLength(500)]
    public string? Level3 { get; set; }
    
    /// <summary>
    /// Definition/description for Level 3
    /// </summary>
    [MaxLength(4000)]
    public string? DefinitionLevel3 { get; set; }
    
    /// <summary>
    /// Level 4 - Most specific/granular level
    /// </summary>
    [MaxLength(500)]
    public string? Level4 { get; set; }
    
    /// <summary>
    /// Definition/description for Level 4
    /// </summary>
    [MaxLength(4000)]
    public string? DefinitionLevel4 { get; set; }
    
    /// <summary>
    /// UNOPS Service Line (e.g., "Project Management", "Infrastructure", "Procurement", "Financial Management", "Human Resources")
    /// </summary>
    [MaxLength(255)]
    public string? ServiceLine { get; set; }
    
    /// <summary>
    /// Grant Support (Implementing Modality) flag
    /// Indicates if this output is a Grant Support implementing modality (marked with * or GQ)
    /// </summary>
    public bool? GrantSupportImplementingModality { get; set; }
    
    /// <summary>
    /// Grant Support (Component) flag
    /// Indicates if this output includes a grant support component (Y flag in CSV)
    /// </summary>
    public bool? GrantSupportComponent { get; set; }
    
    /// <summary>
    /// Procurement (Component) flag
    /// Indicates if this output includes a procurement component (Y flag in CSV)
    /// When true and ServiceLine is NOT "Procurement", a procurement expert should be involved
    /// </summary>
    public bool? ProcurementComponent { get; set; }
    
    /// <summary>
    /// Procurement (Installation Component) flag
    /// Indicates if this output includes a procurement installation component (Y flag in CSV)
    /// </summary>
    public bool? ProcurementInstallationComponent { get; set; }
    
    /// <summary>
    /// Infrastructure (Component) flag
    /// Indicates if this output includes an infrastructure component (Y flag in CSV)
    /// </summary>
    public bool? InfrastructureComponent { get; set; }
}


