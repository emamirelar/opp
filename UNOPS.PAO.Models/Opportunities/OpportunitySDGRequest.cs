namespace UNOPS.PAO.Models;

public class OpportunitySDGRequest
{
    public int SDGId { get; set; }
    public bool IsPrimary { get; set; }
    public bool? SkipTargetsAndIndicators { get; set; }
    public string? Notes { get; set; }
    public List<OpportunitySDGTargetRequest>? Targets { get; set; }
}

