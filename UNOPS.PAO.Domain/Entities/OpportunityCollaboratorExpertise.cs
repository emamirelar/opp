using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Junction table for the many-to-many relationship between OpportunityCollaborator and CollaboratorExpertise.
/// A collaborator can have multiple expertises, and an expertise can be assigned to multiple collaborators.
/// </summary>
public class OpportunityCollaboratorExpertise
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

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
