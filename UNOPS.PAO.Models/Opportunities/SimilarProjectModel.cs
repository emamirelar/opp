namespace UNOPS.PAO.Models;

/// <summary>
/// Model representing a similar project found through AI-powered semantic search
/// </summary>
public class SimilarProjectModel
{
    /// <summary>
    /// Project ID from oneUNOPS (entityId)
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description from metadata
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Relevance score (0-100) based on similarity
    /// </summary>
    public double RelevanceScore { get; set; }
    
    /// <summary>
    /// Implementation start date
    /// </summary>
    public string? StartDate { get; set; }
    
    /// <summary>
    /// Implementation end date
    /// </summary>
    public string? EndDate { get; set; }
    
    /// <summary>
    /// Partners list (comma-separated)
    /// </summary>
    public string? Partners { get; set; }
    
    /// <summary>
    /// Project country list (comma-separated)
    /// </summary>
    public string? Countries { get; set; }
    
    /// <summary>
    /// Project manager name
    /// </summary>
    public string? ProjectManagerName { get; set; }
    
    /// <summary>
    /// Project manager email address
    /// </summary>
    public string? ProjectManagerEmail { get; set; }
    
    /// <summary>
    /// oneUNOPS project URL
    /// </summary>
    public string? ProjectUrl { get; set; }
    
    /// <summary>
    /// AI-generated explanation of why this project is relevant to the opportunity (one-line, max 120 chars)
    /// </summary>
    public string? RelevanceExplanation { get; set; }
}

/// <summary>
/// Request model for fetching similar projects
/// </summary>
public class SimilarProjectsRequest
{
    /// <summary>
    /// Opportunity ID to find similar projects for
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Maximum number of results to return (default: 10)
    /// </summary>
    public int MaxResults { get; set; } = 10;
}

/// <summary>
/// Response model for similar projects
/// </summary>
public class SimilarProjectsResponse
{
    /// <summary>
    /// List of similar projects found
    /// </summary>
    public List<SimilarProjectModel> SimilarProjects { get; set; } = new();
    
    /// <summary>
    /// Keywords extracted from the opportunity for search
    /// </summary>
    public List<string> ExtractedKeywords { get; set; } = new();
    
    /// <summary>
    /// Total number of similar projects found
    /// </summary>
    public int TotalFound { get; set; }
    
    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Model representing a similar opportunity found through semantic search
/// </summary>
public class SimilarOpportunityModel
{
    /// <summary>
    /// Opportunity ID
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Opportunity name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Opportunity description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Initiative budget in USD
    /// </summary>
    public decimal? Budget { get; set; }
    
    /// <summary>
    /// Duration in months (calculated from target delivery date)
    /// </summary>
    public int? DurationMonths { get; set; }
    
    /// <summary>
    /// Relevance score (0-100) based on similarity
    /// </summary>
    public double RelevanceScore { get; set; }
    
    /// <summary>
    /// Workflow stage name
    /// </summary>
    public string? WorkflowStage { get; set; }
}

/// <summary>
/// Response model for similar opportunities
/// </summary>
public class SimilarOpportunitiesResponse
{
    /// <summary>
    /// List of similar opportunities found
    /// </summary>
    public List<SimilarOpportunityModel> SimilarOpportunities { get; set; } = new();
    
    /// <summary>
    /// Total number of similar opportunities found
    /// </summary>
    public int TotalFound { get; set; }
    
    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

