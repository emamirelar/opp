namespace UNOPS.PAO.Models;

public class OpportunitySDGIndicatorModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int OpportunitySDGTargetId { get; set; }
    public int SDGIndicatorDatabaseId { get; set; }  // The integer FK for database relations
    public string SDGIndicatorId { get; set; } = string.Empty;  // The string identifier for display (e.g., "1.1.1", "3.3.2")
    public string? SDGIndicatorLongDescription { get; set; }
    public string? Notes { get; set; }
}

