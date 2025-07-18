using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGeminiManager
{
    AiPrompt MapModelToEntity(GeminiProcessDataRequest req);
    Task<IEnumerable<AiPrompt>> GetPromptData(string type);
    Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData);
    Task<SessionWithChats> GetSessionDataWithChats(string sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetSessionData(string sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetUserSessions(int userId);
    Task<string> ExtractDataFromFile(IFormFile file);
    string FindFileType(IFormFile file);
    Task<string> UploadFileToGCS(IFormFile file);
    Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req);
    Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req);
    Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req);
    Task<bool> UpdateSessionStar(string sessionId, bool starred);
    Task<bool> UpdateSessionArchive(string sessionId, bool archived);
    Task<bool> UpdateSessionTitle(string sessionId, string title);
    Task<dynamic> GenerateEmbeddings(string? entityName);

    Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId);

    Task<string> BulkInsertRecordsAsync(BulkUploadRequest request);
    Task<bool> CanGenerateTitle(string sessionId);
    Task UpdateSessionTitleAndFlag(string sessionId, string title);
    Task<string> ChatWithGemini(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null);
    Task<string> GenerateTitle(string sessionId, int userId);
}