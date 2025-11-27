using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Partners;

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
    /// <summary>
    /// Due Diligence approval status (NotRequired/Required/NotApproved/Approved)
    /// </summary>
    public string? DDApproval { get; set; }
    
    /// <summary>
    /// Due Diligence approval date
    /// </summary>
    public DateTime? DDApprovalDate { get; set; }
    
    /// <summary>
    /// Due Diligence expiry date
    /// </summary>
    public DateTime? DDExpiryDate { get; set; }
    
    /// <summary>
    /// Computed: DD status based on expiry date
    /// </summary>
    public string? DDStatus { get; set; }
    
    /// <summary>
    /// Computed: Whether DD expires before opportunity end
    /// </summary>
    public bool? DDExpiresBeforeOpportunityEnd { get; set; }
    
    /// <summary>
    /// Partner's preferred currency (for suggestion)
    /// </summary>
    public string? PartnerPreferredCurrency { get; set; }
    
    /// <summary>
    /// Amount in USD
    /// </summary>
    public decimal? AmountUSD { get; set; }
    
    /// <summary>
    /// Exchange rate used
    /// </summary>
    public decimal? ExchangeRate { get; set; }
    
    /// <summary>
    /// Exchange rate date
    /// </summary>
    public DateTime? ExchangeRateDate { get; set; }
    
    /// <summary>
    /// Display text for exchange rate (e.g., "1.11 on Nov 20, 2024")
    /// </summary>
    public string? ExchangeRateDisplay { get; set; }
    
    /// <summary>
    /// Whether this partner's contribution is part of pooled funding
    /// </summary>
    public bool IsPooledContribution { get; set; }
    
    /// <summary>
    /// Selected Partner Agreement Number
    /// </summary>
    public string? SelectedPartnerAgreementNumber { get; set; }
    
    /// <summary>
    /// Available partner agreements for this partner
    /// </summary>
    public List<PartnerAgreementInfo>? AvailableAgreements { get; set; }
}
