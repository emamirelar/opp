namespace UNOPS.PAO.Models;

/// <summary>
/// Represents a relevant person from corporate directory for an opportunity
/// </summary>
public class RelevantPersonModel
{
    /// <summary>
    /// Person ID from oneUNOPS (entityId)
    /// </summary>
    public string PersonId { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the person
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Job title/position
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Department or organizational unit
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Location/duty station
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Profile photo URL from Google Workspace
    /// </summary>
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Areas of expertise or skills
    /// </summary>
    public List<string>? Expertise { get; set; }
    
    /// <summary>
    /// Relevance score (0-100) based on similarity to opportunity needs
    /// </summary>
    public double RelevanceScore { get; set; }
    
    /// <summary>
    /// AI-generated explanation of why this person is relevant to the opportunity (one-line, max 120 chars)
    /// </summary>
    public string? RelevanceExplanation { get; set; }
    
    /// <summary>
    /// Additional metadata from the vector store
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request model for getting relevant people
/// </summary>
public class RelevantPeopleRequest
{
    /// <summary>
    /// Opportunity ID to find relevant people for
    /// </summary>
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// Maximum number of results to return (default: 10)
    /// </summary>
    public int MaxResults { get; set; } = 10;
}

/// <summary>
/// Response model for relevant people
/// </summary>
public class RelevantPeopleResponse
{
    /// <summary>
    /// List of relevant people found
    /// </summary>
    public List<RelevantPersonModel> RelevantPeople { get; set; } = new();
    
    /// <summary>
    /// Role keywords extracted from the opportunity for search
    /// </summary>
    public List<string> ExtractedRoles { get; set; } = new();
    
    /// <summary>
    /// Total number of relevant people found
    /// </summary>
    public int TotalFound { get; set; }
    
    /// <summary>
    /// Timestamp when the search was performed
    /// </summary>
    public DateTime SearchTimestamp { get; set; }
}

