namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for applying AI-extracted changes to an opportunity
/// Accepts a flexible set of properties that can span across multiple sections
/// </summary>
public class ApplyAiChangesRequest
{
    // WHAT Section Properties
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }

    // WHY Section Properties
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
    public List<int>? SdGs { get; set; }

    // WHO Section Properties
    public List<int>? FundingPartners { get; set; }
    public List<int>? ClientPartners { get; set; }
    public List<int>? Stakeholders { get; set; }

    // WHERE Section Properties
    public List<int>? Countries { get; set; }

    // WHEN Section Properties
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }

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


