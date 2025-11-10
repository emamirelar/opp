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
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }

    // WHY Section Properties
    public string? StrategicAlignment { get; set; }
    public string? ResultsFocus { get; set; }
    public string? IntendedImpactOutcomes { get; set; }
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
    public int? WorkflowStageId { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
}


