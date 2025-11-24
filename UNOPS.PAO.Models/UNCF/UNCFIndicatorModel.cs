namespace UNOPS.PAO.Models.UNCF;

/// <summary>
/// Model for UN Cooperation Framework (UNCF) Indicator
/// </summary>
public class UNCFIndicatorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UNCFIndicatorExternalId { get; set; }
    public string? UNCFOutcomeExternalId { get; set; }
    public int? VersionNo { get; set; }
    public string? Country { get; set; }
}

