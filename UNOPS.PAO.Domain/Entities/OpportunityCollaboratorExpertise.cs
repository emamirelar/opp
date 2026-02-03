using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Junction table for the many-to-many relationship between OpportunityCollaborator and CollaboratorExpertise.
/// A collaborator can have multiple expertises, and an expertise can be assigned to multiple collaborators.
/// </summary>
public class OpportunityCollaboratorExpertise : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }

    public new string? Name { get; set; }

    /// <summary>
    /// Foreign key to the Opportunity (denormalized for easier querying)
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }

    /// <summary>
    /// Foreign key to the OpportunityCollaborator
    /// </summary>
    public int OpportunityCollaboratorId { get; set; }
    public virtual OpportunityCollaborator? OpportunityCollaborator { get; set; }

    /// <summary>
    /// Foreign key to the CollaboratorExpertise lookup
    /// </summary>
    public int CollaboratorExpertiseId { get; set; }
    public virtual CollaboratorExpertise? CollaboratorExpertise { get; set; }
}
