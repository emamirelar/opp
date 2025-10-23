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
    
    // Child collections
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }
    public List<OpportunityCountryRequest>? Countries { get; set; }
}

public class OpportunityFundingPartnerRequest
{
    public int PartnerId { get; set; }
    public decimal? FundedAmount { get; set; }
    public int CurrencyId { get; set; } // Required, defaults to USD
    public decimal? FeePercentage { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? FeeAmountUSD { get; set; }
    public bool IsAmountBasedFee { get; set; }
}

public class OpportunityClientPartnerRequest
{
    public int PartnerId { get; set; }
}

public class OpportunityStakeholderRequest
{
    public required string StakeholderType { get; set; }
    public int? UserId { get; set; }
    public int? EntityRoleId { get; set; }
}

public class OpportunityDeliverableRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ServiceLineReference { get; set; }
}

public class OpportunityCountryRequest
{
    public int CountryId { get; set; }
    public string? SpecificAreas { get; set; }
}

