namespace UNOPS.PAO.Models;

public class OpportunitySDGTargetRequest
{
    public int OpportunitySDGId { get; set; }
    public int SDGTargetDatabaseId { get; set; }
    public string? Notes { get; set; }
    public List<int>? SDGIndicatorDatabaseIds { get; set; }  // List of indicator IDs to associate
}

