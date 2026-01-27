namespace UNOPS.PAO.Models;

public class OpportunityRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? PartnerReference { get; set; }
    
    /// <summary>
    /// Initial workflow stage. Defaults to "IDENTIFY &amp; PROFILE" if not provided.
    /// Values: "IDENTIFY &amp; PROFILE", "GO", "NO GO"
    /// </summary>
    public string? Stage { get; set; }
    
    public int? ResponsibleOrgUnitId { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public int? DeliveryModality { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    
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
    public bool BeneficiariesToBeDetermined { get; set; }
    
    public bool IsPooledFunding { get; set; }
    public string? MiscExternalStakeholders { get; set; }
    public string? ExternalStakeholderNotes { get; set; }
    
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }
    public List<OpportunityCountryRequest>? Countries { get; set; }
    public List<OpportunitySDGRequest>? SDGs { get; set; }
}

