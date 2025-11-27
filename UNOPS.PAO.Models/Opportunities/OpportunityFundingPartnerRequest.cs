namespace UNOPS.PAO.Models;

public class OpportunityFundingPartnerRequest
{
    public int PartnerId { get; set; }
    public decimal? FundedAmount { get; set; }
    public decimal? Amount { get; set; }  // Alias for FundedAmount for consistency
    public decimal? Percentage { get; set; }
    public int? CurrencyId { get; set; }  // Optional - backend will use opportunity's currency if not provided
    public decimal? FeePercentage { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? FeeAmountUSD { get; set; }
    public bool IsAmountBasedFee { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    public int? DocumentId { get; set; }
    public bool IsPooledContribution { get; set; }
    public string? SelectedPartnerAgreementNumber { get; set; }
}

