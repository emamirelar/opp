using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityDeliverable : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Name/title of the deliverable
    /// </summary>
    public new required string Name { get; set; }
    
    /// <summary>
    /// Detailed description of what will be delivered
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// UNOPS Service Line / Output Subgroup / Output structure reference
    /// </summary>
    [MaxLength(500)]
    public string? ServiceLineReference { get; set; }
}

