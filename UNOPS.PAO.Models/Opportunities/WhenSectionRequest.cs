namespace UNOPS.PAO.Models.Opportunities;

public class WhenSectionRequest
{
    public DateTime? TargetSigningDate { get; set; }
    
    /// <summary>
    /// Implementation start date - defaults to TargetSigningDate if not specified
    /// </summary>
    public DateTime? ImplementationStartDate { get; set; }
    
    public DateTime? TargetDeliveryDate { get; set; }
    
    /// <summary>
    /// Deliverables with updated planned dates for the Work Breakdown Structure
    /// </summary>
    public List<DeliverableDateUpdate>? Deliverables { get; set; }
}

/// <summary>
/// DTO for updating deliverable planned dates in the WHEN section
/// </summary>
public class DeliverableDateUpdate
{
    public int Id { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
}

