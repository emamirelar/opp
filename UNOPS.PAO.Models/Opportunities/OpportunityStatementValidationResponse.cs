namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Response model for opportunity statement validation
/// Contains information about whether the existing statement is aligned with a freshly generated statement
/// </summary>
public class OpportunityStatementValidationResponse
{
    /// <summary>
    /// Opportunity ID
    /// </summary>
    public int OpportunityId { get; set; }

    /// <summary>
    /// Whether the existing opportunity statement is aligned with the freshly generated statement
    /// </summary>
    public bool IsAligned { get; set; }

    /// <summary>
    /// List of misalignment items where the existing statement differs from the freshly generated statement
    /// Empty if IsAligned is true
    /// </summary>
    public List<string> MisalignmentItems { get; set; } = new List<string>();

    /// <summary>
    /// Summary message about the validation result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The freshly generated opportunity statement for reference
    /// This is what the system would generate based on current data
    /// </summary>
    public string? FreshlyGeneratedStatement { get; set; }
}

