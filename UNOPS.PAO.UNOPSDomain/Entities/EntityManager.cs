using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class EntityManager : ModifiableDeletableEntity
{
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EnableChangeLog { get; set; } = false;
    
    // Navigation property to entity fields
    public virtual ICollection<EntityFieldManager> EntityFields { get; set; } = new HashSet<EntityFieldManager>();
} 