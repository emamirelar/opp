using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to Sustainable Development Goals (SDGs)
/// Many-to-many relationship between Opportunity and SDG
/// </summary>
public class OpportunitySDG : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Linked SDG (Sustainable Development Goal)
    /// </summary>
    public int SDGId { get; set; }
    public virtual SDG? SDG { get; set; }
    
    /// <summary>
    /// Alignment type - Primary or Secondary
    /// Primary = main focus, Secondary = supporting goal
    /// </summary>
    [MaxLength(50)]
    public string? AlignmentType { get; set; }
    
    /// <summary>
    /// Notes on how the opportunity aligns with this SDG
    /// </summary>
    [MaxLength(2000)]
    public string? AlignmentNotes { get; set; }
    
    /// <summary>
    /// Expected contribution level (e.g., "High", "Medium", "Low")
    /// </summary>
    [MaxLength(50)]
    public string? ContributionLevel { get; set; }
}

