using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class Unit : ModifiableDeletableEntity<int, int>
{
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

