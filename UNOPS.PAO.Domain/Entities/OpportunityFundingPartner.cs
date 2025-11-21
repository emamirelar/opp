using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityFundingPartner
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }
    
    public int CurrencyId { get; set; }
    public virtual Currency? Currency { get; set; }
    
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Percentage { get; set; }
    
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? FeePercentage { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FeeAmount { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FeeAmountUSD { get; set; }
    
    public bool IsAmountBasedFee { get; set; }
    
    [MaxLength(255)]
    public string? PartnershipAgreementReference { get; set; }
    
    [MaxLength(50)]
    public string? CommitmentStatus { get; set; }
    
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    
    /// <summary>
    /// Preferred currency from partner record (for suggestion)
    /// </summary>
    [MaxLength(10)]
    public string? PartnerPreferredCurrency { get; set; }
    
    /// <summary>
    /// Amount in USD (converted)
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? AmountUSD { get; set; }
    
    /// <summary>
    /// Exchange rate used for conversion
    /// </summary>
    [Column(TypeName = "decimal(18, 8)")]
    public decimal? ExchangeRate { get; set; }
    
    /// <summary>
    /// Date of exchange rate
    /// </summary>
    public DateTime? ExchangeRateDate { get; set; }
    
    /// <summary>
    /// Exchange rate ID used (FK to ExchangeRate table)
    /// </summary>
    public int? ExchangeRateId { get; set; }
    
    // Navigation property
    public virtual ExchangeRate? ExchangeRateRecord { get; set; }
    
    /// <summary>
    /// Whether this partner's contribution is part of pooled funding
    /// </summary>
    public bool IsPooledContribution { get; set; }
}

