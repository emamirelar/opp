using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGeminiManager
{
    AiPrompt MapModelToEntity(GeminiProcessDataRequest req);
    Task<IEnumerable<AiPrompt>> GetPromptData(string type);
    Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData, string entityId = null);
    Task<string> GetSessionDataWithChats(string sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetUserSessions(int userId);
    Task<string> ExtractDataFromFile(IFormFile file);
    string FindFileType(IFormFile file);
    Task<string> UploadFileToGCS(IFormFile file);
    Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req);
    Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req, ClaimsPrincipal user = null);
    Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req);
    Task<bool> UpdateSessionStar(string sessionId, bool starred);
    Task<bool> UpdateSessionArchive(string sessionId, bool archived);
    Task<bool> UpdateSessionTitle(string sessionId, string title);
    Task<dynamic> GenerateEmbeddings(string? entityName);

    Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId);

    Task<string> BulkInsertRecordsAsync(BulkUploadRequest request);
    Task UpdateSessionTitleAndFlag(string sessionId, string title);
    Task<string> ChatWithGemini(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null);
    IAsyncEnumerable<string> ChatWithGeminiStreaming(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null);
    Task<SessionConfiguration> GetSessionConfigurationAsync();
    Task<SimilarProjectsResponse> GetSimilarProjectsAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null);
    Task<RelevantPeopleResponse> GetRelevantPeopleAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null);
    Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(int opportunityId, ClaimsPrincipal user = null, int maxResults = 10);
    
    /// <summary>
    /// Generates AI-powered insights and suggestions for an opportunity
    /// </summary>
    Task<OpportunityInsightsResponse> GenerateOpportunityInsightsAsync(int opportunityId, ClaimsPrincipal user = null);
    
    /// <summary>
    /// Generates AI-powered opportunity proposal from multiple sources (interactions, documents, etc.)
    /// </summary>
    Task<OpportunityProposalResponse> GenerateOpportunityProposalAsync(OpportunityProposalRequest request, ClaimsPrincipal user = null);
    
    /// <summary>
    /// Priority: Tagged framework docs first, then fallback to all other documents if needed.
    /// Returns temporary extraction data for user verification (not saved to database).
    /// </summary>
    Task<List<ExtractedDeliverableInfo>> ExtractDeliverablesWithFrameworkPriorityAsync(int opportunityId);
    
    /// <summary>
    /// </summary>
    Task<FrameworkStatusResponse> GetFrameworkStatusAsync(int opportunityId);
    
    /// <summary>
    /// Generates a comprehensive opportunity statement in markdown format following the UNOPS template
    /// </summary>
    Task<string> GenerateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal user = null);
}

/// <summary>
/// Represents session configuration data from the Python service.
/// </summary>
public class SessionConfiguration
{
    [System.Text.Json.Serialization.JsonPropertyName("app_name")]
    public string AppName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("application_name")]
    public string ApplicationName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("organization")]
    public string Organization { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("environment")]
    public string Environment { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}