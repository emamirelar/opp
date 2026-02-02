using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to interactions that led to their creation
/// Many-to-many relationship between Opportunity and Interaction
/// </summary>
public class OpportunityInteraction : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int InteractionId { get; set; }
    public virtual Interaction? Interaction { get; set; }
}

