namespace UNOPS.PAO.Models;

public class OpportunitySDGRequest
{
    public int SDGId { get; set; }
    public bool IsPrimary { get; set; }
    public string? ContributionLevel { get; set; }
    public string? Notes { get; set; }
}

