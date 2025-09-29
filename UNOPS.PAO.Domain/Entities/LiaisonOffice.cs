using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class LiaisonOffice : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Code { get; set; }
    
    [Required]
    [MaxLength(250)]
    public required new string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Region { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public virtual ICollection<Partner> Partners { get; set; } = new HashSet<Partner>();
    
    // Computed properties for list/search operations
    [NotMapped]
    public int PartnerCount { get; set; } // Will be populated by service
}
