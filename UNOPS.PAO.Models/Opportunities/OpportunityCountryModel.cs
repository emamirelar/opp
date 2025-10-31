namespace UNOPS.PAO.Models;

public class OpportunityCountryModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode { get; set; }
    public string? Continent { get; set; }
    public string? Region { get; set; }
    public string? SpecificAreas { get; set; }
    public string? ContextWarning { get; set; }
    public decimal? RiskScore { get; set; }
}

