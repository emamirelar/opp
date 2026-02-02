using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to Sustainable Development Goals (SDGs)
/// Many-to-many relationship between Opportunity and SDG
/// </summary>
public class OpportunitySDG : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int SDGId { get; set; }
    public virtual SDG? SDG { get; set; }
    
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Indicates if the user has opted out of providing SDG Targets and Indicators for this SDG
    /// When true, no targets or indicators will be selected/displayed
    /// </summary>
    public bool? SkipTargetsAndIndicators { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public virtual ICollection<OpportunitySDGTarget> Targets { get; set; } = new HashSet<OpportunitySDGTarget>();
}

