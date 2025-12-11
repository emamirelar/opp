namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the Overview section of an opportunity
/// Includes name, description, and initiative budget fields
/// </summary>
public class OverviewSectionRequest
{
    /// <summary>
    /// Opportunity name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Opportunity description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Proposed budget for the initiative in USD
    /// </summary>
    public decimal? InitiativeBudgetUSD { get; set; }
}


