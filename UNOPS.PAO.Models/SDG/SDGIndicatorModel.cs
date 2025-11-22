namespace UNOPS.PAO.Models.SDG;

public class SDGIndicatorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SDGIndicatorId { get; set; } = string.Empty;
    public string SDGTargetId { get; set; } = string.Empty;
    public string? SDGIndicatorLongDescription { get; set; }
}

