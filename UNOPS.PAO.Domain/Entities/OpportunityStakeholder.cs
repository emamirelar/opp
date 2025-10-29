using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityStakeholder
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int EntityRoleId { get; set; }
    public virtual EntityRole? EntityRole { get; set; }
    
    public bool IsInternal { get; set; } = true;
    
    [MaxLength(50)]
    public string? StakeholderType { get; set; } // "Internal" or "External"
    
    public int? UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    
    public int? ContactId { get; set; }
    public virtual Contact? Contact { get; set; }
    
    [MaxLength(500)]
    public string? Organization { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

