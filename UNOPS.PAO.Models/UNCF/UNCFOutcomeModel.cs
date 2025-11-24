namespace UNOPS.PAO.Models.UNCF;

/// <summary>
/// Model for UN Cooperation Framework (UNCF) Outcome
/// </summary>
public class UNCFOutcomeModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UNCFOutcomeExternalId { get; set; }
    public int? VersionNo { get; set; }
    public string? Country { get; set; }
}

