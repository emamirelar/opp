using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityCountry
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int CountryId { get; set; }
    public virtual Country? Country { get; set; }
    
    [MaxLength(1000)]
    public string? SpecificAreas { get; set; }

    [MaxLength(500)]
    public string? ContextWarning { get; set; }
    
    [Column(TypeName = "decimal(3, 1)")]
    public decimal? RiskScore { get; set; }
}

