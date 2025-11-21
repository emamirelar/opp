namespace UNOPS.PAO.Models;

public class OpportunitySDGTargetModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int OpportunitySDGId { get; set; }
    public int SDGTargetDatabaseId { get; set; }  // The integer FK for database relations
    public string SDGTargetId { get; set; } = string.Empty;  // The string identifier for display (e.g., "1.1", "3.3")
    public string? TargetDescription { get; set; }
    public string? TargetType { get; set; }
    public string? Notes { get; set; }
    public List<OpportunitySDGIndicatorModel> Indicators { get; set; } = new List<OpportunitySDGIndicatorModel>();
}

