using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class EntityRole : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Entity type this role applies to (e.g., "Opportunity", "Partner", "Interaction")
    /// </summary>
    public required string EntityType { get; set; }
    
    /// <summary>
    /// Role name (e.g., "Opportunity Manager", "Internal Stakeholder", "External Stakeholder")
    /// </summary>
    public new required string Name { get; set; }
    
    /// <summary>
    /// Description of the role's responsibilities
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this role is for internal (UNOPS) users
    /// </summary>
    public bool IsInternal { get; set; }
    
    /// <summary>
    /// Whether multiple people can be assigned to this role for the same entity
    /// </summary>
    public bool AllowsMultiple { get; set; }
    
    /// <summary>
    /// Optional type classification for the role
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// Optional subtype classification for the role
    /// </summary>
    public string? SubType { get; set; }
    
    /// <summary>
    /// Optional unique code identifier for the role (typically derived from Name with spaces replaced by underscores and dashes removed)
    /// </summary>
    public string? Code { get; set; }
}

