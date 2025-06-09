using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class Entities : ModifiableDeletableEntity
{
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool CanManage { get; set; } = false;
} 