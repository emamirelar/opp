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
}

