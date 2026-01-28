using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Lookup table for collaborator expertise types.
/// Represents the specific expertise/capacity in which a collaborator is related to an opportunity.
/// </summary>
public class CollaboratorExpertise : ModifiableDeletableEntity
{
    /// <summary>
    /// Unique code for the expertise type (e.g., "GEN_OPP_DEV", "FIN_MGMT")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }

    /// <summary>
    /// Description of the expertise type
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in dropdowns
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property for collaborators with this expertise
    /// </summary>
    public virtual ICollection<OpportunityCollaboratorExpertise> CollaboratorExpertises { get; set; } = new HashSet<OpportunityCollaboratorExpertise>();
}
