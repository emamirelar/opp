namespace UNOPS.PAO.Models;

public class OpportunityFundingPartnerRequest
{
    public int PartnerId { get; set; }
    public decimal? FundedAmount { get; set; }
    public int CurrencyId { get; set; }
    public decimal? FeePercentage { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? FeeAmountUSD { get; set; }
    public bool IsAmountBasedFee { get; set; }
}

