using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class Output : ModifiableDeletableEntity<int, int>
{
    [MaxLength(255)]
    public string? OutputGroup { get; set; }
    
    [MaxLength(255)]
    public string? OutputSubGroup { get; set; }
    
    [MaxLength(500)]
    public string? OutputName { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public int? UnitId { get; set; }
    
    [ForeignKey(nameof(UnitId))]
    public Unit? Unit { get; set; }
    
    public int? ProjectCategoryId { get; set; }
    
    [ForeignKey(nameof(ProjectCategoryId))]
    public ProjectCategory? ProjectCategory { get; set; }
    
    [MaxLength(255)]
    public string? OutputServiceLine { get; set; }
}

