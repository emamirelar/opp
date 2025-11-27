namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Response model for opportunity statement validation
/// Contains information about whether the statement is aligned with structured data
/// </summary>
public class OpportunityStatementValidationResponse
{
    /// <summary>
    /// Opportunity ID
    /// </summary>
    public int OpportunityId { get; set; }

    /// <summary>
    /// Whether the opportunity statement is aligned with the structured data
    /// </summary>
    public bool IsAligned { get; set; }

    /// <summary>
    /// List of misalignment items where the statement doesn't match structured data
    /// Empty if IsAligned is true
    /// </summary>
    public List<string> MisalignmentItems { get; set; } = new List<string>();

    /// <summary>
    /// Summary message about the validation result
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

