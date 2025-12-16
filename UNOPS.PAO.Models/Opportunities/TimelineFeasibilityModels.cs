namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Response model for timeline feasibility analysis
/// Compares opportunity timeline against historical data from similar opportunities
/// </summary>
public class TimelineFeasibilityResponse
{
    /// <summary>
    /// ID of the opportunity being analyzed
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Number of days remaining until target signing date
    /// Negative if signing date has passed
    /// </summary>
    public int? DaysUntilSigning { get; set; }
    
    /// <summary>
    /// Overall feasibility status based on worst case of all benchmarks
    /// </summary>
    public FeasibilityStatus OverallStatus { get; set; }
    
    /// <summary>
    /// Whether there is sufficient historical data for meaningful analysis
    /// </summary>
    public bool HasSufficientData { get; set; }
    
    /// <summary>
    /// Human-readable summary of the feasibility assessment
    /// </summary>
    public string? Summary { get; set; }
    
    /// <summary>
    /// Minimum number of similar opportunities required for benchmark analysis
    /// </summary>
    public int MinimumSampleSize { get; set; }
    
    /// <summary>
    /// Benchmark analysis based on similar service lines
    /// </summary>
    public ServiceLineBenchmark? ServiceLineBenchmark { get; set; }
    
    /// <summary>
    /// Benchmark analysis based on similar countries
    /// </summary>
    public CountryBenchmark? CountryBenchmark { get; set; }
    
    /// <summary>
    /// List of warnings and suggestions for the timeline
    /// </summary>
    public List<FeasibilityWarning> Warnings { get; set; } = new();
    
    /// <summary>
    /// Reference list of similar completed opportunities for comparison
    /// </summary>
    public List<HistoricalOpportunityReference> SimilarCompletedOpportunities { get; set; } = new();
}

/// <summary>
/// Benchmark analysis based on opportunities with similar service lines
/// </summary>
public class ServiceLineBenchmark
{
    /// <summary>
    /// Service lines used for comparison
    /// </summary>
    public List<string> ServiceLines { get; set; } = new();
    
    /// <summary>
    /// Number of historical opportunities in the sample
    /// </summary>
    public int SampleSize { get; set; }
    
    /// <summary>
    /// Average number of days for development (creation to signing)
    /// </summary>
    public double AverageDevelopmentDays { get; set; }
    
    /// <summary>
    /// Minimum development days observed in sample
    /// </summary>
    public int MinDevelopmentDays { get; set; }
    
    /// <summary>
    /// Maximum development days observed in sample
    /// </summary>
    public int MaxDevelopmentDays { get; set; }
    
    /// <summary>
    /// Feasibility status for this benchmark
    /// </summary>
    public FeasibilityStatus Status { get; set; }
}

/// <summary>
/// Benchmark analysis based on opportunities in similar countries
/// </summary>
public class CountryBenchmark
{
    /// <summary>
    /// Countries used for comparison
    /// </summary>
    public List<string> Countries { get; set; } = new();
    
    /// <summary>
    /// Number of historical opportunities in the sample
    /// </summary>
    public int SampleSize { get; set; }
    
    /// <summary>
    /// Average number of days for development (creation to signing)
    /// </summary>
    public double AverageDevelopmentDays { get; set; }
    
    /// <summary>
    /// Minimum development days observed in sample
    /// </summary>
    public int MinDevelopmentDays { get; set; }
    
    /// <summary>
    /// Maximum development days observed in sample
    /// </summary>
    public int MaxDevelopmentDays { get; set; }
    
    /// <summary>
    /// Feasibility status for this benchmark
    /// </summary>
    public FeasibilityStatus Status { get; set; }
}

/// <summary>
/// Warning or suggestion about timeline feasibility
/// </summary>
public class FeasibilityWarning
{
    /// <summary>
    /// Severity level of the warning
    /// </summary>
    public WarningSeverity Severity { get; set; }
    
    /// <summary>
    /// Category of the warning
    /// </summary>
    public WarningCategory Category { get; set; }
    
    /// <summary>
    /// Warning message describing the issue
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Suggested action to address the warning
    /// </summary>
    public string? Suggestion { get; set; }
}

/// <summary>
/// Reference to a historical opportunity used for comparison
/// </summary>
public class HistoricalOpportunityReference
{
    /// <summary>
    /// Opportunity ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Opportunity name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Number of days it took to develop (creation to signing)
    /// </summary>
    public int DevelopmentDays { get; set; }
    
    /// <summary>
    /// Current workflow stage
    /// </summary>
    public string? WorkflowStage { get; set; }
    
    /// <summary>
    /// Service lines that match the current opportunity
    /// </summary>
    public List<string> MatchingServiceLines { get; set; } = new();
    
    /// <summary>
    /// Countries that match the current opportunity
    /// </summary>
    public List<string> MatchingCountries { get; set; } = new();
}

/// <summary>
/// Overall feasibility status of the timeline
/// </summary>
public enum FeasibilityStatus
{
    /// <summary>
    /// Insufficient data to determine feasibility
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Timeline is feasible with adequate buffer
    /// </summary>
    OnTrack = 1,
    
    /// <summary>
    /// Timeline is tight but achievable
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// Timeline is at high risk or unrealistic
    /// </summary>
    Critical = 3
}

/// <summary>
/// Severity level of a feasibility warning
/// </summary>
public enum WarningSeverity
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info = 0,
    
    /// <summary>
    /// Warning that requires attention
    /// </summary>
    Warning = 1,
    
    /// <summary>
    /// Critical issue that needs immediate action
    /// </summary>
    Critical = 2
}

/// <summary>
/// Category of feasibility warning
/// </summary>
public enum WarningCategory
{
    /// <summary>
    /// General timeline warning
    /// </summary>
    General = 0,
    
    /// <summary>
    /// Timeline-related warning
    /// </summary>
    Timeline = 1,
    
    /// <summary>
    /// Service line-specific warning
    /// </summary>
    ServiceLine = 2,
    
    /// <summary>
    /// Country-specific warning
    /// </summary>
    Country = 3
}



