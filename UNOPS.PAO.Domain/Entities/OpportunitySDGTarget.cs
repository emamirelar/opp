using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to SDG Targets
/// Child relationship of OpportunitySDG
/// </summary>
public class OpportunitySDGTarget
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
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

