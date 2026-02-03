using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityClientPartner : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int PartnerId { get; set; }
    public virtual Partner? Partner { get; set; }
    
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    
    /// <summary>
    /// Selected Partner Agreement Number for this client relationship
    /// </summary>
    [MaxLength(50)]
    public string? SelectedPartnerAgreementNumber { get; set; }
}
