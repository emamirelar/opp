namespace UNOPS.PAO.Models.Values;

/// <summary>
/// Model for UN Sustainable Development Goal (SDG)
/// </summary>
public class SDGModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SDGId { get; set; }
    public string? SDGNumber { get; set; }
    public string? SDGDescription { get; set; }
    public string? SDGLogo { get; set; }
    public string? SDGLongDescription { get; set; }
    public string Status { get; set; } = string.Empty;
}

