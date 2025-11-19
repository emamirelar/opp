using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Links opportunities to interactions that led to their creation
/// Many-to-many relationship between Opportunity and Interaction
/// </summary>
public class OpportunityInteraction
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int InteractionId { get; set; }
    public virtual Interaction? Interaction { get; set; }
}

