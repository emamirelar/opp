using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to SDG Indicators
/// Child relationship of OpportunitySDGTarget
/// </summary>
public class OpportunitySDGIndicator : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int OpportunitySDGTargetId { get; set; }
    public virtual OpportunitySDGTarget? OpportunitySDGTarget { get; set; }
    
    public int SDGIndicatorId { get; set; }
    public virtual SDGIndicator? SDGIndicator { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

