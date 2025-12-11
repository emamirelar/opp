namespace UNOPS.PAO.Models.Shared;

/// <summary>
/// Request model for semantic search of Products & Services
/// </summary>
public class OutputSemanticSearchRequest
{
    /// <summary>
    /// The user's text/phrase to search for (in their own words)
    /// </summary>
    public required string SearchText { get; set; }
    
    /// <summary>
    /// Maximum number of results to return (default: 5)
    /// </summary>
    public int MaxResults { get; set; } = 5;
    
    /// <summary>
    /// Minimum similarity threshold (0.0 - 1.0, default: 0.3)
    /// </summary>
    public float MinSimilarity { get; set; } = 0.3f;
}

/// <summary>
/// Response model for semantic search of Products & Services
/// </summary>
public class OutputSemanticSearchResponse
{
    /// <summary>
    /// The original search text entered by the user
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
    
    /// <summary>
    /// List of matched outputs with similarity scores
    /// </summary>
    public List<OutputSemanticSearchMatch> Matches { get; set; } = new();
    
    /// <summary>
    /// Total number of matches found
    /// </summary>
    public int TotalMatches { get; set; }
}

/// <summary>
/// A single match result from semantic search
/// </summary>
public class OutputSemanticSearchMatch
{
    /// <summary>
    /// The matched Output
    /// </summary>
    public OutputModel Output { get; set; } = new();
    
    /// <summary>
    /// Combined similarity score (0.0 - 1.0)
    /// </summary>
    public float SimilarityScore { get; set; }
    
    /// <summary>
    /// The level at which the match was found (Level0, Level1, etc.)
    /// </summary>
    public string MatchedLevel { get; set; } = string.Empty;
    
    /// <summary>
    /// The hierarchy path that matched (e.g., "Infrastructure > Construction > Building")
    /// </summary>
    public string MatchedHierarchy { get; set; } = string.Empty;
    
    /// <summary>
    /// Semantic score component
    /// </summary>
    public float SemanticScore { get; set; }
    
    /// <summary>
    /// Keyword match score component  
    /// </summary>
    public float KeywordScore { get; set; }
    
    /// <summary>
    /// Text similarity score component
    /// </summary>
    public float TextSimilarityScore { get; set; }
}

