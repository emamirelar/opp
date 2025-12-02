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
    /// Indicates if the target signing date is a firm deadline defined by the partner
    /// </summary>
    public bool? IsTargetSigningDateFirm { get; set; }
    
    /// <summary>
    /// Notes about the target signing date (e.g., partner deadline, submission closing date)
    /// </summary>
    public string? SigningDateNotes { get; set; }
    
    /// <summary>
    /// Partner submission deadline (if applicable)
    /// </summary>
    public DateTime? SubmissionDeadline { get; set; }
    
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

