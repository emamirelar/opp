using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class ProposedInitiativeType : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Name of the initiative type (e.g., "Project", "Programme", "Portfolio")
    /// </summary>
    public new required string Name { get; set; }
    
    /// <summary>
    /// Description of what this initiative type represents
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Display order for UI
    /// </summary>
    public int Order { get; set; }
}

