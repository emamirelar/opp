using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents a collaborator on an opportunity - personnel who have permissions to edit all fields of the opportunity.
/// Part of the Opportunity Development Team.
/// </summary>
public class OpportunityCollaborator : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// The user ID of the collaborator. Must be an active internal user.
    /// </summary>
    public int UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    
    /// <summary>
    /// Date when the collaborator was added to the opportunity
    /// </summary>
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who added this collaborator
    /// </summary>
    public int? AddedBy { get; set; }
    public virtual PAOUser? AddedByUser { get; set; }

    /// <summary>
    /// Navigation property for the collaborator's expertises (many-to-many via junction table).
    /// Represents the specific expertise/capacity in which this collaborator is related to the opportunity.
    /// </summary>
    public virtual ICollection<OpportunityCollaboratorExpertise> Expertises { get; set; } = new HashSet<OpportunityCollaboratorExpertise>();
}
