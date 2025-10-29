using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to Sustainable Development Goals (SDGs)
/// Many-to-many relationship between Opportunity and SDG
/// </summary>
public class OpportunitySDG
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int SDGId { get; set; }
    public virtual SDG? SDG { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    [MaxLength(50)]
    public string? ContributionLevel { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

