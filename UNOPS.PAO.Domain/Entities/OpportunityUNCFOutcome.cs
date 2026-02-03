using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to UN Cooperation Framework (UNCF) Outcomes via country
/// UNCF Outcomes are country-specific, so they are linked through OpportunityCountry
/// Only the latest version outcomes should be displayed for selection
/// </summary>
public class OpportunityUNCFOutcome : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Reference to the country this UNCF Outcome belongs to
    /// UNCF Outcomes are country-specific
    /// </summary>
    public int OpportunityCountryId { get; set; }
    public virtual OpportunityCountry? OpportunityCountry { get; set; }
    
    /// <summary>
    /// Reference to the UNCF Outcome (latest version only)
    /// </summary>
    public int UNCFOutcomeId { get; set; }
    public virtual UNCFOutcome? UNCFOutcome { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public virtual ICollection<OpportunityUNCFIndicator> Indicators { get; set; } = new HashSet<OpportunityUNCFIndicator>();
}

