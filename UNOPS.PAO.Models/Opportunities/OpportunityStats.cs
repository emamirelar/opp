namespace UNOPS.PAO.Models;

public class OpportunityStats
{
    public decimal TotalFundingUSD { get; set; }
    public decimal TotalFeeAmountUSD { get; set; }
    public int FundingPartnerCount { get; set; }
    public int ClientPartnerCount { get; set; }
    public int StakeholderCount { get; set; }
    public int InternalStakeholderCount { get; set; }
    public int ExternalStakeholderCount { get; set; }
    public int DeliverableCount { get; set; }
    public int CountryCount { get; set; }
    public int SDGCount { get; set; }
    public int? PrimarySDGId { get; set; }
}

