using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityDeliverable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int? OutputId { get; set; }
    
    [ForeignKey(nameof(OutputId))]
    public virtual Output? Output { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Quantity { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

