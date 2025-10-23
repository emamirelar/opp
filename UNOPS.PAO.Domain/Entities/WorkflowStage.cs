using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class WorkflowStage : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Entity type this workflow stage applies to (e.g., "Opportunity", "Project", "Portfolio")
    /// </summary>
    public required string EntityType { get; set; }
    
    /// <summary>
    /// Display name of the workflow stage
    /// </summary>
    public new required string Name { get; set; }
    
    /// <summary>
    /// Description of what this stage represents
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Order/sequence of this stage in the workflow
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Whether this stage allows parallel processing with other stages
    /// </summary>
    public bool AllowsParallelProcessing { get; set; }
    
    /// <summary>
    /// Whether this is a terminal/final stage
    /// </summary>
    public bool IsFinalStage { get; set; }
}

