namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for creating an opportunity from an AI-generated proposal
/// Contains user-accepted fields with resolved IDs from the dependents
/// </summary>
public class CreateOpportunityFromInteractionsRequest
{
    /// <summary>
    /// Opportunity name (user-provided, required)
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Opportunity description (enhanced by AI, optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Partner ID associated with the interactions (optional - only provided when creating from partner context)
    /// </summary>
    public int? PartnerId { get; set; }
    
    /// <summary>
    /// Whether partner is a funding partner (only required if PartnerId is provided)
    /// </summary>
    public bool IsFundingPartner { get; set; }
    
    /// <summary>
    /// Whether partner is a client partner (only required if PartnerId is provided)
    /// </summary>
    public bool IsClientPartner { get; set; }
    
    /// <summary>
    /// Source interaction IDs that were analyzed (optional - may not be provided in all cases)
    /// </summary>
    public List<int>? SourceInteractionIds { get; set; }
    
    /// <summary>
    /// Newly uploaded documents to be persisted to database after opportunity creation
    /// </summary>
    public List<NewDocumentRequest>? Documents { get; set; }
    
    // WHAT Section Properties (AI-proposed, user-accepted)
    public string? PartnerReference { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }

    // WHY Section Properties (AI-proposed, user-accepted)
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
    public string? Challenges { get; set; }
    public List<int>? SdGs { get; set; }

    // WHO Section Properties (AI-proposed, user-accepted)
    // These are now proper structured objects from AI analysis
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }

    // WHERE Section Properties (AI-proposed, user-accepted)
    public List<int>? Countries { get; set; }

    // WHEN Section Properties (AI-proposed, user-accepted)
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
}

