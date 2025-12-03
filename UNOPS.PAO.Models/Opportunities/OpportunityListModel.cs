using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models;

/// <summary>
/// Lightweight model for opportunity list views (search, list, dashboard).
/// Excludes heavy data like images, nested collections, and detailed relationships.
/// Use OpportunityModel for detail views that need complete data.
/// </summary>
public class OpportunityListModel
{
    // ========== CORE IDENTITY ==========
    public int Id { get; set; }
    public required string Name { get; set; }
    
    /// <summary>
    /// Truncated description for list display (max 200 chars)
    /// </summary>
    public string? DescriptionPreview { get; set; }
    
    public string? PartnerReference { get; set; }
    
    // ========== STATUS & WORKFLOW ==========
    public string? Status { get; set; }
    public int? WorkflowStageId { get; set; }
    public string? WorkflowStageName { get; set; }
    
    // ========== ORGANIZATION ==========
    public int? ResponsibleOrgUnitId { get; set; }
    public string? ResponsibleOrgUnitName { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public string? ProposedInitiativeTypeName { get; set; }
    
    // ========== FINANCIALS ==========
    public decimal? InitiativeBudgetUSD { get; set; }
    
    // ========== KEY DATES ==========
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public bool IsTargetSigningDateFirm { get; set; }
    
    // ========== VISUAL (Thumbnail only - small icon) ==========
    /// <summary>
    /// Small thumbnail icon for list display (1:1 ratio, ~50KB)
    /// Banner image is excluded - only available on detail view
    /// </summary>
    public string? OpportunityThumbnail { get; set; }
    
    // ========== SUMMARY COUNTS (instead of full collections) ==========
    /// <summary>
    /// Number of funding partners (instead of full FundingPartners list)
    /// </summary>
    public int FundingPartnersCount { get; set; }
    
    /// <summary>
    /// Number of client partners (instead of full ClientPartners list)
    /// </summary>
    public int ClientPartnersCount { get; set; }
    
    /// <summary>
    /// Number of implementation countries (instead of full Countries list)
    /// </summary>
    public int CountriesCount { get; set; }
    
    /// <summary>
    /// Number of SDGs (instead of full SDGs list with nested targets/indicators)
    /// </summary>
    public int SDGsCount { get; set; }
    
    /// <summary>
    /// Number of deliverables (instead of full Deliverables list with Output details)
    /// </summary>
    public int DeliverablesCount { get; set; }
    
    /// <summary>
    /// Number of stakeholders (instead of full Stakeholders list)
    /// </summary>
    public int StakeholdersCount { get; set; }
    
    /// <summary>
    /// Primary country names for display (comma-separated, max 3)
    /// </summary>
    public string? PrimaryCountries { get; set; }
    
    /// <summary>
    /// Primary funding partner name for display
    /// </summary>
    public string? PrimaryFundingPartner { get; set; }
    
    // ========== AUDIT INFO ==========
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? CreatedByName { get; set; }
    
    // ========== DISPLAY TAGS ==========
    /// <summary>
    /// Conditional tags for frontend badge display
    /// </summary>
    public List<EntityTagModel>? Tags => CalculateConditionalTags();
    
    private List<EntityTagModel> CalculateConditionalTags()
    {
        var tags = new List<EntityTagModel>();
        
        if (!string.IsNullOrEmpty(Status))
        {
            var statusColor = Status switch
            {
                "Draft" => "bg-badge-secondary text-badge-secondary",
                "Active" => "bg-badge-info text-badge-info",
                "Closed" => "bg-badge-danger text-badge-danger",
                "Archived" => "bg-yellow-100 text-yellow-800",
                _ => "bg-badge-secondary text-badge-secondary"
            };
            tags.Add(new EntityTagModel { Tag = Status, Color = statusColor });
        }
        
        if (!string.IsNullOrEmpty(WorkflowStageName) && !string.IsNullOrEmpty(Status) && Status != "Closed" && Status != "Archived")
        {
            var workflowColor = "bg-badge-warn text-badge-warn";
            tags.Add(new EntityTagModel { Tag = WorkflowStageName, Color = workflowColor });
        }
        
        return tags;
    }
}

