using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to SDG Targets
/// Child relationship of OpportunitySDG
/// </summary>
public class OpportunitySDGTarget : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int OpportunitySDGId { get; set; }
    public virtual OpportunitySDG? OpportunitySDG { get; set; }
    
    public int SDGTargetId { get; set; }
    public virtual SDGTarget? SDGTarget { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public virtual ICollection<OpportunitySDGIndicator> Indicators { get; set; } = new HashSet<OpportunitySDGIndicator>();
}

