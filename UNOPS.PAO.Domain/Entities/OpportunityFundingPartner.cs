using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityFundingPartner : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Funding partner from Partner Tree
    /// </summary>
    public int PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }
    
    /// <summary>
    /// Funded amount in partner's currency
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FundedAmount { get; set; }
    
    /// <summary>
    /// Currency of the funded amount (defaults to USD, required)
    /// </summary>
    public int CurrencyId { get; set; }
    public virtual Currency? Currency { get; set; }
    
    /// <summary>
    /// Fee percentage estimate for this partner
    /// </summary>
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? FeePercentage { get; set; }
    
    /// <summary>
    /// Fee amount (can be calculated or manually entered)
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FeeAmount { get; set; }
    
    /// <summary>
    /// Fee amount in USD
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FeeAmountUSD { get; set; }
    
    /// <summary>
    /// Whether fee is amount-based or percentage-based
    /// </summary>
    public bool IsAmountBasedFee { get; set; }
}

