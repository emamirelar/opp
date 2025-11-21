namespace UNOPS.PAO.Models;

public class OpportunitySDGModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int SDGDatabaseId { get; set; }  // The integer FK for database relations
    public string SDGId { get; set; } = string.Empty;  // The string identifier for display (e.g., "SDG-01")
    public string? SDGNumber { get; set; }
    public string? SDGName { get; set; }
    public string? SDGLogoUrl { get; set; }  // URL to the SDG logo image
    public bool IsPrimary { get; set; }
    public bool? SkipTargetsAndIndicators { get; set; }
    public string? Notes { get; set; }
    public List<OpportunitySDGTargetModel> Targets { get; set; } = new List<OpportunitySDGTargetModel>();
}

