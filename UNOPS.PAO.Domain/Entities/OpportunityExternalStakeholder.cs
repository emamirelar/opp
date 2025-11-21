using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents external stakeholders (contacts) associated with an opportunity
/// These are interested parties who have an active interest or may need to receive updates
/// </summary>
public class OpportunityExternalStakeholder
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int ContactId { get; set; }
    public virtual Contact? Contact { get; set; }
}

