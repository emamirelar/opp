namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the Overview section of an opportunity
/// Includes name and description fields
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
}

