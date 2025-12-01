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
    /// Humanitarian, Peace & Security Framework alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? HumanitarianFrameworkAlignment { get; set; }
    
    /// <summary>
    /// Indicates if this country has an active Humanitarian, Peace & Security Framework
    /// Populated by the manager when loading opportunity data
    /// </summary>
    public bool HasHumanitarianFramework { get; set; }
    
    /// <summary>
    /// Nationally Determined Contributions (NDC) alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NdcAlignment { get; set; }
    
    /// <summary>
    /// Indicates if this country has active Nationally Determined Contributions (NDC)
    /// Populated by the manager when loading opportunity data
    /// </summary>
    public bool HasNdc { get; set; }
    
    /// <summary>
    /// National Adaptation Plan (NAP) alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? NapAlignment { get; set; }
    
    /// <summary>
    /// Indicates if this country has an active National Adaptation Plan (NAP)
    /// Populated by the manager when loading opportunity data
    /// </summary>
    public bool HasNap { get; set; }
    
    /// <summary>
    /// Organization Unit Strategy alignment status for this specific country
    /// Null = Not specified, True = Will align, False = Will not align at this time
    /// </summary>
    public bool? OrgUnitStrategyAlignment { get; set; }
    
    /// <summary>
    /// Indicates if there is an Organization Unit with a Strategy artifact (for this country)
    /// Determined by traversing up the org hierarchy from the country's local org unit
    /// </summary>
    public bool HasOrgUnitStrategy { get; set; }
    
    /// <summary>
    /// The ID of the most local OrganizationHierarchy that has a Strategy artifact
    /// Null if no strategy found in the hierarchy
    /// </summary>
    public int? OrgUnitWithStrategyId { get; set; }
    
    /// <summary>
    /// The name of the most local OrganizationHierarchy that has a Strategy artifact
    /// For display purposes
    /// </summary>
    public string? OrgUnitWithStrategyName { get; set; }
    
    /// <summary>
    /// The code of the most local OrganizationHierarchy that has a Strategy artifact
    /// For display purposes
    /// </summary>
    public string? OrgUnitWithStrategyCode { get; set; }
    
    /// <summary>
    /// The current (stored) most local OrganizationHierarchy ID that has a Strategy
    /// This is what was computed when the opportunity was last saved
    /// </summary>
    public int? CurrentOrgUnitWithStrategyId { get; set; }
    
    /// <summary>
    /// The name of the current (stored) OrganizationHierarchy with Strategy
    /// For display purposes when showing changes
    /// </summary>
    public string? CurrentOrgUnitWithStrategyName { get; set; }
    
    /// <summary>
    /// The code of the current (stored) OrganizationHierarchy with Strategy
    /// For display purposes when showing changes
    /// </summary>
    public string? CurrentOrgUnitWithStrategyCode { get; set; }
    
    /// <summary>
    /// Indicates if a more local Organization Unit Strategy is now available
    /// Computed by comparing CurrentOrgUnitWithStrategyId with OrgUnitWithStrategyId
    /// </summary>
    public bool HasMoreLocalStrategyAvailable { get; set; }
    
    /// <summary>
    /// Full country details with artifacts (optional, for detailed views)
    /// </summary>
    public CountryModel? Country { get; set; }
}

