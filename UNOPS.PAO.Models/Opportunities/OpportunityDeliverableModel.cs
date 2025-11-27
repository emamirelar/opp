namespace UNOPS.PAO.Models;

/// <summary>
/// DTO for Opportunity Deliverables using the new UNOPS Products and Services List hierarchy.
/// </summary>
public class OpportunityDeliverableModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int? OutputId { get; set; }
    
    // Hierarchical Output fields from new Products and Services List
    public string? OutputName { get; set; }
    public string? Level0 { get; set; }
    public string? Level1 { get; set; }
    public string? DefinitionLevel1 { get; set; }
    public string? Level2 { get; set; }
    public string? DefinitionLevel2 { get; set; }
    public string? Level3 { get; set; }
    public string? DefinitionLevel3 { get; set; }
    public string? Level4 { get; set; }
    public string? DefinitionLevel4 { get; set; }
    public string? ServiceLine { get; set; }
    
    // Component flags from Output entity
    public bool? GrantSupportImplementingModality { get; set; }
    public bool? GrantSupportComponent { get; set; }
    public bool? ProcurementComponent { get; set; }
    public bool? ProcurementInstallationComponent { get; set; }
    public bool? InfrastructureComponent { get; set; }
    
    public decimal? Quantity { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Planned start date for this deliverable (Work Breakdown Structure timeline)
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }
    
    /// <summary>
    /// Planned end date for this deliverable (Work Breakdown Structure timeline)
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }
    
    /// <summary>
    /// Sequence order for display in the Work Breakdown Structure
    /// </summary>
    public int? SequenceOrder { get; set; }
}

