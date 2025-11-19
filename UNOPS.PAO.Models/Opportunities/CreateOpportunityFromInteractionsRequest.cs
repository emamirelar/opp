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
    /// Opportunity description (enhanced by AI, required)
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Partner ID associated with the interactions
    /// </summary>
    public required int PartnerId { get; set; }
    
    /// <summary>
    /// Whether partner is a funding partner
    /// </summary>
    public required bool IsFundingPartner { get; set; }
    
    /// <summary>
    /// Whether partner is a client partner
    /// </summary>
    public required bool IsClientPartner { get; set; }
    
    /// <summary>
    /// Source interaction IDs that were analyzed
    /// </summary>
    public required List<int> SourceInteractionIds { get; set; }
    
    // WHAT Section Properties (AI-proposed, user-accepted)
    public string? PartnerReference { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }

    // WHY Section Properties (AI-proposed, user-accepted)
    public string? StrategicAlignment { get; set; }
    public string? ResultsFocus { get; set; }
    public string? IntendedImpactOutcomes { get; set; }
    public string? ExpectedBeneficiaries { get; set; }
    public List<int>? SdGs { get; set; }

    // WHO Section Properties (AI-proposed, user-accepted)
    public List<int>? FundingPartners { get; set; }
    public List<int>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }

    // WHERE Section Properties (AI-proposed, user-accepted)
    public List<int>? Countries { get; set; }

    // WHEN Section Properties (AI-proposed, user-accepted)
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
}

