namespace UNOPS.PAO.Models;

public class OpportunityModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? PartnerReference { get; set; }
    public string? Status { get; set; } // Mapped from EntityStatus enum
    public int? WorkflowStageId { get; set; }
    public string? WorkflowStageName { get; set; }
    public int? ResponsibleOrgUnitId { get; set; }
    public string? ResponsibleOrgUnitName { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public decimal? InitiativeBudgetUSD { get; set; }
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public string? ProposedInitiativeTypeName { get; set; }
    
    // Child collections
    public List<OpportunityFundingPartnerModel>? FundingPartners { get; set; }
    public List<OpportunityClientPartnerModel>? ClientPartners { get; set; }
    public List<OpportunityStakeholderModel>? Stakeholders { get; set; }
    public List<OpportunityDeliverableModel>? Deliverables { get; set; }
    public List<OpportunityCountryModel>? Countries { get; set; }
    public List<DocumentModel>? Documents { get; set; }
    
    // Audit fields
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

public class OpportunityFundingPartnerModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public decimal? FundedAmount { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? FeePercentage { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? FeeAmountUSD { get; set; }
    public bool IsAmountBasedFee { get; set; }
}

public class OpportunityClientPartnerModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
}

public class OpportunityStakeholderModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public required string StakeholderType { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
}

public class OpportunityDeliverableModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ServiceLineReference { get; set; }
}

public class OpportunityCountryModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? SpecificAreas { get; set; }
}

