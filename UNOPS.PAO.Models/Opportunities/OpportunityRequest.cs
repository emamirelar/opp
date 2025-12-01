namespace UNOPS.PAO.Models;

public class OpportunityRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? PartnerReference { get; set; }
    public int? WorkflowStageId { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    
    public string? ResultsFocus { get; set; }
    public string? IntendedImpactOutcomes { get; set; }
    public string? ExpectedBeneficiaries { get; set; }
    public string? Challenges { get; set; }
    
    public bool IsPooledFunding { get; set; }
    
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }
    public List<OpportunityCountryRequest>? Countries { get; set; }
    public List<OpportunitySDGRequest>? SDGs { get; set; }
}

