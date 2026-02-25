namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for applying AI-extracted changes to an opportunity
/// Accepts a flexible set of properties that can span across multiple sections
/// </summary>
public class ApplyOpportunityAiChangesRequest
{
    // WHAT Section Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public int? DeliveryModality { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }

    // WHY Section Properties
    public string? Challenges { get; set; }
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
    public bool? BeneficiariesToBeDetermined { get; set; }
    public List<int>? SdGs { get; set; }
    public List<OpportunityUNOPSMissionRequest>? UNOPSMissions { get; set; }

    // WHO Section Properties
    /// <summary>
    /// Funding partners with optional budget information (amount, currency)
    /// </summary>
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<int>? ClientPartners { get; set; }
    /// <summary>
    /// Internal stakeholders with userId and entityRoleId
    /// When applying AI changes, Opportunity Manager role is preserved if not included in this list
    /// </summary>
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    public string? MiscExternalStakeholders { get; set; }
    public string? ExternalStakeholderNotes { get; set; }

    // WHERE Section Properties
    public List<int>? Countries { get; set; }

    // WHEN Section Properties
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public DateTime? ImplementationStartDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public bool? IsTargetSigningDateFirm { get; set; }
    public string? SigningDateNotes { get; set; }

    // OTHER Properties
    public string? PartnerReference { get; set; }
    public string? Status { get; set; }
    
    /// <summary>
    /// Current workflow stage. Values: "IDENTIFY &amp; PROFILE", "GO", "NO GO"
    /// </summary>
    public string? Stage { get; set; }
    
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
}


