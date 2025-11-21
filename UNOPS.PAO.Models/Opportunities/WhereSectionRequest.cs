namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHERE section of an opportunity
/// Includes implementation countries
/// </summary>
public class WhereSectionRequest
{
    /// <summary>
    /// List of implementation countries
    /// </summary>
    public List<OpportunityCountryRequest>? Countries { get; set; }
}

