namespace UNOPS.PAO.Models;

public class OpportunityCountryRequest
{
    public int CountryId { get; set; }
    public string? SpecificAreas { get; set; }
    
    /// <summary>
    /// Humanitarian, Peace & Security Framework alignment status for this country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? HumanitarianFrameworkAlignment { get; set; }
    
    /// <summary>
    /// Nationally Determined Contributions (NDC) alignment status for this country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NdcAlignment { get; set; }
    
    /// <summary>
    /// National Adaptation Plan (NAP) alignment status for this country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NapAlignment { get; set; }
    
    /// <summary>
    /// Organization Unit Strategy alignment status for this country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? OrgUnitStrategyAlignment { get; set; }
}

