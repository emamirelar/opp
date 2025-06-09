namespace UNOPS.PAO.Models;

public class ConfigurationResponse
{
    public string? GoogleClientId { get; set; }
    public string? GoogleApiKey { get; set; }
    public string? Environment { get; set; }
    public string? ProjectId { get; set; }
    public string? Location { get; set; }
    public string? DefaultModel { get; set; }
}
