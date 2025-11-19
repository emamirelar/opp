namespace UNOPS.PAO.Models.Interactions;

/// <summary>
/// Lightweight interaction summary model for listing and selection
/// Used in dropdowns, multi-select components, and opportunity creation
/// </summary>
public class InteractionSummaryModel
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public string? Location { get; set; }
    public int ContactCount { get; set; }
    public int UserCount { get; set; }
}

