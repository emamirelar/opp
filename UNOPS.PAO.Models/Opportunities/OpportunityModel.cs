using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models;

public class OpportunityModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PartnerReference { get; set; }
    public string? Status { get; set; }
    public int? WorkflowStageId { get; set; }
    public string? WorkflowStageName { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public string? ResponsibleOrgUnitName { get; set; }
    
    /// <summary>
    /// Full organization unit model for the responsible org unit (includes artifacts)
    /// </summary>
    public OrganizationHierarchyModel? ResponsibleOrgUnit { get; set; }
    
    public int? ProposedInitiativeTypeId { get; set; }
    public string? ProposedInitiativeTypeName { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    
    /// <summary>
    /// Implementation start date - defaults to TargetSigningDate if not specified
    /// </summary>
    public DateTime? ImplementationStartDate { get; set; }
    
    public DateTime? TargetDeliveryDate { get; set; }
    
    /// <summary>
    /// Indicates if the target signing date is a firm deadline defined by the partner
    /// </summary>
    public bool IsTargetSigningDateFirm { get; set; }
    
    /// <summary>
    /// Notes about the target signing date (e.g., partner deadline, submission closing date)
    /// </summary>
    public string? SigningDateNotes { get; set; }
    
    /// <summary>
    /// Partner submission deadline (if applicable)
    /// </summary>
    public DateTime? SubmissionDeadline { get; set; }
    
    public string? ResultsFocus { get; set; }
    /// <summary>
    /// Expected impact description (max 200 characters)
    /// </summary>
    public string? ExpectedImpact { get; set; }
    /// <summary>
    /// Expected outcomes description (max 200 characters)
    /// </summary>
    public string? ExpectedOutcomes { get; set; }
    public string? ExpectedBeneficiaries { get; set; }
    public int? EstimatedDirectBeneficiaries { get; set; }
    public int? EstimatedIndirectBeneficiaries { get; set; }
    public bool BeneficiariesToBeDetermined { get; set; }
    public string? Challenges { get; set; }
    
    /// <summary>
    /// AI-generated opportunity statement in markdown format
    /// </summary>
    public string? OpportunityStatementMarkdown { get; set; }
    
    /// <summary>
    /// AI-generated banner image for the opportunity (base64 encoded)
    /// </summary>
    public string? OpportunityBannerImage { get; set; }
    
    /// <summary>
    /// AI-generated thumbnail image for the opportunity (base64 encoded)
    /// </summary>
    public string? OpportunityThumbnail { get; set; }
    
    public bool IsPooledFunding { get; set; }
    
    /// <summary>
    /// Indicates that the user has acknowledged reviewing all organizational high risks
    /// </summary>
    public bool HighRisksAcknowledged { get; set; }
    
    /// <summary>
    /// Indicates how UNOPS will deliver the Products & Services (nullable - not set by default)
    /// </summary>
    public int? DeliveryModality { get; set; }
    
    public List<OpportunityFundingPartnerModel>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerModel>? ClientPartners { get; set; }
    public List<OpportunityStakeholderModel>? Stakeholders { get; set; }
    public List<OpportunityExternalStakeholderModel>? ExternalStakeholders { get; set; }
    public string? MiscExternalStakeholders { get; set; }
    public string? ExternalStakeholderNotes { get; set; }
    public List<OpportunityDeliverableModel>? Deliverables { get; set; }
    public List<OpportunityCountryModel>? Countries { get; set; }
    public List<OpportunitySDGModel>? SDGs { get; set; }
    public List<OpportunityUNCFOutcomeModel>? UNCFOutcomes { get; set; }
    public List<OpportunityUNOPSMissionModel>? UNOPSMissions { get; set; }
    
    /// <summary>
    /// SME (Subject Matter Expert) selections for the opportunity.
    /// Loaded from OpportunityStakeholder table where IsInternal = true and EntityRole.Type = "SME".
    /// </summary>
    public List<SMESelectionModel>? SMESelections { get; set; }
    
    public OpportunityStats? Stats { get; set; }
    
    /// <summary>
    /// Whether this is a new value range for the responsible org unit
    /// </summary>
    public bool? IsNewValueRangeForOrgUnit { get; set; }
    
    /// <summary>
    /// Historical max value for the org unit
    /// </summary>
    public decimal? OrgUnitHistoricalMaxValue { get; set; }
    
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
    
    /// <summary>
    /// The current user's role(s) for this opportunity (for dashboard display)
    /// </summary>
    public string? UserRole { get; set; }
    
    /// <summary>
    /// Permission information for the current user on this opportunity
    /// </summary>
    public EntityPermissionsModel? Permissions { get; set; }
    
    // ========== CONDITIONAL TAGS ==========
    /// <summary>
    /// Conditional tags based on opportunity's current state for frontend display
    /// </summary>
    public List<EntityTagModel>? Tags => CalculateConditionalTags();
    
    /// <summary>
    /// Calculate conditional tags based on opportunity's current state for frontend display
    /// </summary>
    public List<EntityTagModel> CalculateConditionalTags()
    {
        var tags = new List<EntityTagModel>();
        
        // Opportunity Status Tags (matches PrimeNG badge severities)
        if (!string.IsNullOrEmpty(Status))
        {
            var statusColor = Status switch
            {
                "Draft" => "bg-badge-secondary text-badge-secondary",      // Gray - matches p-badge severity="secondary"
                "Active" => "bg-badge-info text-badge-info",                // Blue - matches p-badge severity="info"
                "Closed" => "bg-badge-danger text-badge-danger",            // Red - matches p-badge severity="danger"
                "Archived" => "bg-yellow-100 text-yellow-800",              // Yellow - archived state
                _ => "bg-badge-secondary text-badge-secondary"
            };
            tags.Add(new EntityTagModel { Tag = Status, Color = statusColor });
        }
        
        // Workflow Stage Tag (if exists and status is not Closed/Archived)  
        if (!string.IsNullOrEmpty(WorkflowStageName) && !string.IsNullOrEmpty(Status) && Status != "Closed" && Status != "Archived")
        {
            // UNOPS warning color (amber/golden) for opportunity workflow stages - matches p-badge severity="warn"
            var workflowColor = "bg-badge-warn text-badge-warn";
            tags.Add(new EntityTagModel { Tag = WorkflowStageName, Color = workflowColor });
        }
        
        return tags;
    }
}

