namespace UNOPS.PAO.Business.Workflow.StageRequirements;

/// <summary>
/// Field type constants for stage requirements.
/// Used by OpportunityStageRequirementsProvider and PaoStageRequirement.
/// Note: PaoStageRequirement (extends UNOPS.Workflow.Models.StageRequirement) provides IsMet for Task 8.4.
/// </summary>
public static class FieldTypes
{
    public const string Text = "text";
    public const string Number = "number";
    public const string Date = "date";
    public const string Array = "array";
    public const string Select = "select";
}
