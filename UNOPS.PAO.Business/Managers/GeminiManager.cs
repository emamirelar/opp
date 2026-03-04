namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using System.Security.Claims;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;

public class GeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly DataRepository<AiPrompt> _promptRepository;

    public GeminiManager(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _promptRepository = new DataRepository<AiPrompt>(context);
    }

    public async Task<IEnumerable<AiPrompt>> GetPromptData(string type)
    {
        return await Task.FromResult(_promptRepository
            .GetAll()
            .Where(x => x.Type == type));
    }

    public async Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData, string entityId = null)
    {
        // Implement the logic to fetch result from Gemini
        throw new NotImplementedException();
    }

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessDataRequest req)
    {
        var entity = _mapper.Map<AiPrompt>(req);
        return entity;
    }

    public Task<string> GetSessionDataWithChats(string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AiChatSession>> GetUserSessions(int userId) {
        throw new NotImplementedException();
    }

    public string CreateNewSession(int userId) {
        throw new NotImplementedException();
    }

    public bool EndSession(string sessionId) {
        throw new NotImplementedException();
    }

    public Task<dynamic> EntityDetectionThroughGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string fileUrl, string fileType) {
        throw new NotImplementedException();
    }

    public Task<dynamic> FetchDetailedResponseFromGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string promptType, string fileUrl, string fileType) {
        throw new NotImplementedException();
    }

    public JObject GetDetailsFromGeminiResponse(string modelResponse) {
        throw new NotImplementedException();
    }

    public Task<AiChatSession> UpdateCurrentSessionIfInactive(int userId, string sessionId) {
        throw new NotImplementedException();
    }

    public Task<string> ProcessImage(IFormFile file) {
        throw new NotImplementedException();
    }

    public Task<string> ProcessAudio(IFormFile file) {
        throw new NotImplementedException();
    }

    public Task<string> ExtractDataFromFile(IFormFile file) {
        throw new NotImplementedException();
    }

    public string FindFileType(IFormFile file) {
        throw new NotImplementedException();
    }

    public Task<string> UploadFileToGCS(IFormFile file) {
        throw new NotImplementedException();
    }

    public Task<dynamic> ProcessChatWithGemini(GeminiAssistantRequest req, int currentUserId)
    {
        throw new NotImplementedException();
    }

    public Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req, System.Security.Claims.ClaimsPrincipal user = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateSessionStar(string sessionId, bool starred)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateSessionArchive(string sessionId, bool archived)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateSessionTitle(string sessionId, string title)
    {
        throw new NotImplementedException();
    }

    public Task<dynamic> GenerateEmbeddings(string? entityName)
    {
        throw new NotImplementedException();
    }

    public Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<string> BulkInsertRecordsAsync(BulkUploadRequest request)
    {
        throw new NotImplementedException();
    }


    public async Task UpdateSessionTitleAndFlag(string sessionId, string title)
    {
        throw new NotImplementedException();
    }

    public async Task<string> ChatWithGemini(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<string> ChatWithGeminiStreaming(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null)
    {
        throw new NotImplementedException();
#pragma warning disable CS0162 // Unreachable code - required for IAsyncEnumerable
        yield break;
#pragma warning restore CS0162
    }



    public Task<SessionConfiguration> GetSessionConfigurationAsync()
    {
        throw new NotImplementedException();
    }

    public Task<SimilarProjectsResponse> GetSimilarProjectsAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null, bool invalidateCache = false)
    {
        throw new NotImplementedException();
    }
    
    public Task<RelevantPeopleResponse> GetRelevantPeopleAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null, bool invalidateCache = false)
    {
        throw new NotImplementedException();
    }
    
    public Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(int opportunityId, ClaimsPrincipal user = null, int maxResults = 10, List<int>? dismissedOupQuestionIds = null, bool forceRefresh = false)
    {
        throw new NotImplementedException();
    }
    
    public Task<OpportunityInsightsResponse> GenerateOpportunityInsightsAsync(int opportunityId, ClaimsPrincipal user = null, bool forceRefresh = false)
    {
        throw new NotImplementedException();
    }

    public Task<OpportunityProposalResponse> GenerateOpportunityProposalAsync(OpportunityProposalRequest request, ClaimsPrincipal user = null)
    {
        throw new NotImplementedException();
    }

    //Default stub implementations (overridden in UNOPSGeminiManager)
    public virtual Task<List<ExtractedDeliverableInfo>> ExtractDeliverablesWithFrameworkPriorityAsync(int opportunityId)
    {
        throw new NotImplementedException("This method should be implemented in UNOPSGeminiManager");
    }

    public virtual Task<FrameworkStatusResponse> GetFrameworkStatusAsync(int opportunityId)
    {
        throw new NotImplementedException("This method should be implemented in UNOPSGeminiManager");
    }
    
    public Task<string> GenerateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal user = null, bool saveToDatabase = true)
    {
        throw new NotImplementedException();
    }
    
    public Task<OpportunityStatementValidationResponse> ValidateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal user = null)
    {
        throw new NotImplementedException();
    }
    
    public Task<List<string>> CreateBatchEmbeddingsAsync(List<string> texts)
    {
        throw new NotImplementedException("This method is only implemented in UNOPSGeminiManager");
    }
    
    public Task<Dictionary<string, string>> GenerateKeywordsAsync(List<string> texts)
    {
        throw new NotImplementedException("This method is only implemented in UNOPSGeminiManager");
    }
}