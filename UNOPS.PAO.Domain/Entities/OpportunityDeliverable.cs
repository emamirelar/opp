using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityDeliverable : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int? OutputId { get; set; }
    
    [ForeignKey(nameof(OutputId))]
    public virtual Output? Output { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Quantity { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    // Timeline and Work Breakdown Structure fields
    /// <summary>
    /// Sequence order for displaying deliverables in timeline/WBS
    /// </summary>
    public int? SequenceOrder { get; set; }
    
    /// <summary>
    /// Planned start date for this deliverable (milestone placeholder)
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }
    
    /// <summary>
    /// Planned end date for this deliverable (milestone placeholder)
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }
}

