using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class Engagement : ModifiableDeletableEntity
{
    [MaxLength(500)]
    public string? BaseEngagement { get; set; }
    
    [MaxLength(1000)]
    public string? EngagementDescription { get; set; }
    
    public string? EngagementLongDescription { get; set; }
    
    [MaxLength(500)]
    public string? EngagementStageDescription { get; set; }
    
    public DateTime? EngagementImplementationStartDate { get; set; }
    
    public DateTime? EngagementImplementationEndDate { get; set; }
    
    public int? PartnerId { get; set; }
    
    [MaxLength(2000)]
    public string? ImplementationCountriesDescriptionConcatenated { get; set; }
    
    // Navigation property
    [ForeignKey("ErpDimValue")]
    public virtual Partner? Partner { get; set; }
}
