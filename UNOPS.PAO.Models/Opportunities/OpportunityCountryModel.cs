using UNOPS.PAO.Models.Locations;

namespace UNOPS.PAO.Models;

public class OpportunityCountryModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int CountryId { get; set; }
    
    /// <summary>
    /// Opportunity-specific fields for this country relationship
    /// </summary>
    public string? SpecificAreas { get; set; }
    public string? ContextWarning { get; set; }
    public decimal? RiskScore { get; set; }
    
    /// <summary>
    /// Full country details with artifacts (optional, for detailed views)
    /// </summary>
    public CountryModel? Country { get; set; }
}

