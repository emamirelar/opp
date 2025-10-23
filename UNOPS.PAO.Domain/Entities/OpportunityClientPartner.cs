using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityClientPartner : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Client partner from Partner Tree
    /// </summary>
    public int PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }
}

