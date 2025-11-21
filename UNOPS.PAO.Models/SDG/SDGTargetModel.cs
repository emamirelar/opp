namespace UNOPS.PAO.Models.SDG;

public class SDGTargetModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SDGTargetId { get; set; } = string.Empty;
    public string SDGId { get; set; } = string.Empty;
    public string? TargetDescription { get; set; }
    public string? TargetType { get; set; }
}

