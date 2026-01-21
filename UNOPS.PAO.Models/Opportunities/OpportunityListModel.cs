using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models;

/// <summary>
/// Lightweight model for opportunity list views (search, list, dashboard).
/// Excludes heavy data like banner images, nested collections, and detailed relationships.
/// Use OpportunityModel for detail views that need complete data.
/// 
/// PERFORMANCE: This model is designed to minimize database load by:
/// - Excluding banner image (large base64 string)
/// - Excluding all collection data (FundingPartners, ClientPartners, Countries, SDGs, etc.)
/// - Only including fields needed for list display
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
    
    /// <summary>
    /// Current workflow stage. Values: "IDENTIFY &amp; PROFILE", "GO", "NO GO"
    /// </summary>
    public string? Stage { get; set; }
    
    /// <summary>
    /// Workflow status for approval tracking.
    /// </summary>
    public WorkflowStatus WorkflowStatus { get; set; }
    
    /// <summary>
    /// Indicates if the opportunity is in an approval workflow.
    /// </summary>
    public bool IsInWorkflow { get; set; }
    
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
        
        if (!string.IsNullOrEmpty(Stage) && !string.IsNullOrEmpty(Status) && Status != "Closed" && Status != "Archived")
        {
            var workflowColor = "bg-badge-warn text-badge-warn";
            tags.Add(new EntityTagModel { Tag = Stage, Color = workflowColor });
        }
        
        return tags;
    }
}

