using UNOPS.Workflow.Models.Requirements;

namespace UNOPS.PAO.Business.Workflow.StageRequirements;

/// <summary>
/// PAO-specific stage requirement that extends Workflow Models with IsMet.
/// Used for Task 8.4: Requirements endpoint returns unmet requirements with IsMet = false for incomplete fields.
/// Frontend requirements-validation component displays unmet items based on this.
/// </summary>
public class PaoStageRequirement : StageRequirement
{
    /// <summary>
    /// Whether this requirement is met for the current entity.
    /// Set by validation when returning requirements to the frontend.
    /// </summary>
    public bool IsMet { get; set; }
}
