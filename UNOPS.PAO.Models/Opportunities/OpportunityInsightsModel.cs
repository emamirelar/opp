namespace UNOPS.PAO.Models;

/// <summary>
/// Insight about an opportunity (observation, finding, note)
/// </summary>
public class OpportunityInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, warning, success
    public string Priority { get; set; } = "medium"; // high, medium, low
}

/// <summary>
/// Actionable suggestion for improving an opportunity
/// </summary>
public class OpportunitySuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ActionTarget { get; set; } // WHAT, WHERE, WHY, WHO, WHEN
}

/// <summary>
/// Response model for opportunity insights and suggestions
/// </summary>
public class OpportunityInsightsResponse
{
    public List<OpportunityInsight> Insights { get; set; } = new();
    public List<OpportunitySuggestion> Suggestions { get; set; } = new();
    public double AnalysisConfidence { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
    public long ExecutionTimeMs { get; set; }
}


