using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityClientPartner
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }
    
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
}

