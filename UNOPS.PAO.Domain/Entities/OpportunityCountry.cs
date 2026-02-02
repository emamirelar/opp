using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityCountry : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int CountryId { get; set; }
    public virtual Country? Country { get; set; }
    
    [MaxLength(1000)]
    public string? SpecificAreas { get; set; }

    [MaxLength(500)]
    public string? ContextWarning { get; set; }
    
    [Column(TypeName = "decimal(3, 1)")]
    public decimal? RiskScore { get; set; }
    
    /// <summary>
    /// Humanitarian, Peace & Security Framework alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? HumanitarianFrameworkAlignment { get; set; }
    
    /// <summary>
    /// Nationally Determined Contributions (NDC) alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NdcAlignment { get; set; }
    
    /// <summary>
    /// National Adaptation Plan (NAP) alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NapAlignment { get; set; }
    
    /// <summary>
    /// Organization Unit Strategy alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? OrgUnitStrategyAlignment { get; set; }
    
    /// <summary>
    /// The most local OrganizationHierarchy (for this country) that has a Strategy artifact
    /// Populated by traversing up the hierarchy from the country's org unit
    /// </summary>
    public int? OrgUnitWithStrategyId { get; set; }
    
    /// <summary>
    /// Navigation property to the OrganizationHierarchy that has the Strategy
    /// </summary>
    public virtual OrganizationHierarchy? OrgUnitWithStrategy { get; set; }
    
    /// <summary>
    /// UNCF Outcomes associated with this country for the opportunity
    /// </summary>
    public virtual ICollection<OpportunityUNCFOutcome> UNCFOutcomes { get; set; } = new HashSet<OpportunityUNCFOutcome>();
}

