namespace UNOPS.Workflow.Models.Requirements;

/// <summary>
/// Stub types for stage requirements until the Workflow submodule adds them.
/// These types are referenced by OpportunityStageRequirementsProvider.
/// TODO: Remove once UNOPS.Workflow.Models includes these types natively.
/// </summary>

public interface IStageRequirementsProvider
{
    IEnumerable<string> EntityNames { get; }
    List<StageRequirement> GetRequirementsForStageChange(string currentStage, string nextStage);
}

public class StageRequirement
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool OnlyServerSideEvaluation { get; set; }
    public bool IsMet { get; set; }
    public RequirementValidation? Validation { get; set; }
    public Dictionary<string, object>? CustomValidatorConfig { get; set; }
}

public class RequirementValidation
{
    public bool Required { get; set; }
    public double? GreaterThan { get; set; }
    public int? MinLength { get; set; }
    public ConditionalValidation? Conditional { get; set; }
}

public class ConditionalValidation
{
    public string Field { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public static class FieldTypes
{
    public const string Text = "text";
    public const string Number = "number";
    public const string Date = "date";
    public const string Array = "array";
    public const string Select = "select";
}
