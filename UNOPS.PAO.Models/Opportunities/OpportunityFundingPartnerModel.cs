using UNOPS.PAO.Models.Documents;

namespace UNOPS.PAO.Models;

public class OpportunityFundingPartnerModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerLogoUrl { get; set; }
    public decimal? Amount { get; set; }
    public decimal? FundedAmount { get; set; }
    public decimal? Percentage { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public string? CommitmentStatus { get; set; }
    public bool IsAmountBasedFee { get; set; }
    public decimal? FeePercentage { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? FeeAmountUSD { get; set; }
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public List<DocumentDetailModel>? AssociatedDocuments { get; set; }
    
    /// <summary>
    /// Partner's current status (Draft/Active/Closed/Archived)
    /// </summary>
    public string? PartnerStatus { get; set; }
    
    /// <summary>
    /// Partner's approval status (Approved/NotApproved)
    /// </summary>
    public string? PartnerApprovalStatus { get; set; }
}

