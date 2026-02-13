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
    Task<SimilarProjectsResponse> GetSimilarProjectsAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null, bool invalidateCache = false);
    Task<RelevantPeopleResponse> GetRelevantPeopleAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null, bool invalidateCache = false);
    /// <summary>
    /// Gets AI-powered DST risk recommendations for an opportunity
    /// Includes predefined high risks with oupQuestionId for oUP mapping
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="user">Current user claims</param>
    /// <param name="maxResults">Max vector store results</param>
    /// <param name="dismissedOupQuestionIds">OupQuestionIds that user has dismissed (from localStorage)</param>
    /// <param name="forceRefresh">If true, bypasses cache to get fresh recommendations</param>
    Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(int opportunityId, ClaimsPrincipal user = null, int maxResults = 10, List<int>? dismissedOupQuestionIds = null, bool forceRefresh = false);
    
    /// <summary>
    /// Generates AI-powered insights and suggestions for an opportunity
    /// </summary>
    /// <param name="forceRefresh">When true, bypasses cache to ensure fresh Gemini response (e.g. after section save)</param>
    Task<OpportunityInsightsResponse> GenerateOpportunityInsightsAsync(int opportunityId, ClaimsPrincipal user = null, bool forceRefresh = false);
    
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
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="user">Current user claims</param>
    /// <param name="saveToDatabase">Whether to save the generated statement to the database (default: true)</param>
    Task<string> GenerateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal user = null, bool saveToDatabase = true);
    
    /// <summary>
    /// Validates whether the existing opportunity statement is aligned with a freshly generated statement
    /// Compares the stored statement against a newly generated one based on current data
    /// </summary>
    Task<OpportunityStatementValidationResponse> ValidateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal user = null);
    
    /// <summary>
    /// Creates batch embeddings for a list of texts (delegates to AiContextualService)
    /// </summary>
    Task<List<string>> CreateBatchEmbeddingsAsync(List<string> texts);
    
    /// <summary>
    /// Generates keywords for a list of texts for hybrid search (delegates to AiContextualService)
    /// </summary>
    Task<Dictionary<string, string>> GenerateKeywordsAsync(List<string> texts);
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