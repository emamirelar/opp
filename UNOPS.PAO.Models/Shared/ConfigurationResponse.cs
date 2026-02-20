namespace UNOPS.PAO.Models.Shared;

public class ConfigurationResponse
{
    public string? GoogleClientId { get; set; }
    public string? GoogleApiKey { get; set; }
    public string? Environment { get; set; }
    public string? ProjectId { get; set; }
    public string? Location { get; set; }
    public string? DefaultModel { get; set; }

    /// <summary>
    /// Google Analytics measurement ID (e.g., G-2FSJTG2DKE). Used only when GoogleAnalyticsEnabled is true.
    /// </summary>
    public string? GoogleAnalyticsMeasurementId { get; set; }

    /// <summary>
    /// When true, the frontend loads Google Analytics. Typically enabled for Dev/Test/QA, disabled for Production.
    /// </summary>
    public bool GoogleAnalyticsEnabled { get; set; }
}
