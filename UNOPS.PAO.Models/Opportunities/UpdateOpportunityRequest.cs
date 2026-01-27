namespace UNOPS.PAO.Models;

public class UpdateOpportunityRequest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? PartnerReference { get; set; }
    
    /// <summary>
    /// Current workflow stage. Values: "IDENTIFY &amp; PROFILE", "GO", "NO GO"
    /// </summary>
    public string? Stage { get; set; }
    
    public int? ResponsibleOrgUnitId { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }
    public List<OpportunityCountryRequest>? Countries { get; set; }
    public List<OpportunitySDGRequest>? SDGs { get; set; }
}

