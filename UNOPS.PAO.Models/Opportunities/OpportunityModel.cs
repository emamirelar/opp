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
    public int? ProposedInitiativeTypeId { get; set; }
    public string? ProposedInitiativeTypeName { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    
    public string? StrategicAlignment { get; set; }
    public string? ResultsFocus { get; set; }
    public string? IntendedImpactOutcomes { get; set; }
    public string? ExpectedBeneficiaries { get; set; }
    
    public List<OpportunityFundingPartnerModel>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerModel>? ClientPartners { get; set; }
    public List<OpportunityStakeholderModel>? Stakeholders { get; set; }
    public List<OpportunityDeliverableModel>? Deliverables { get; set; }
    public List<OpportunityCountryModel>? Countries { get; set; }
    public List<OpportunitySDGModel>? SDGs { get; set; }
    
    public OpportunityStats? Stats { get; set; }
    
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

