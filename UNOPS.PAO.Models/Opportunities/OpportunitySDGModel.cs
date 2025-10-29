namespace UNOPS.PAO.Models;

public class OpportunitySDGModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int SDGId { get; set; }
    public int SDGNumber { get; set; }
    public string? SDGName { get; set; }
    public bool IsPrimary { get; set; }
    public string? ContributionLevel { get; set; }
    public string? Notes { get; set; }
}

