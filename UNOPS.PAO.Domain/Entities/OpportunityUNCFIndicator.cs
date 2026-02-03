using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to UNCF Indicators
/// Child relationship of OpportunityUNCFOutcome
/// </summary>
public class OpportunityUNCFIndicator : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Reference to the parent OpportunityUNCFOutcome
    /// </summary>
    public int OpportunityUNCFOutcomeId { get; set; }
    public virtual OpportunityUNCFOutcome? OpportunityUNCFOutcome { get; set; }
    
    /// <summary>
    /// Reference to the UNCF Indicator
    /// </summary>
    public int UNCFIndicatorId { get; set; }
    public virtual UNCFIndicator? UNCFIndicator { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

