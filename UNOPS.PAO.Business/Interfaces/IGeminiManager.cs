using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGeminiManager
{
    AiPrompt MapModelToEntity(GeminiProcessDataRequest req);
    Task<IEnumerable<AiPrompt>> GetPromptData(string type);
    Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData);
    Task<SessionWithChats> GetSessionDataWithChats(Guid sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetSessionData(Guid sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetUserSessions(int userId);
    Task<string> ExtractDataFromFile(IFormFile file);
    string FindFileType(IFormFile file);
    Task<string> UploadFileToGCS(IFormFile file);
    Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req);
    Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req);
    Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req);
    Task<bool> UpdateSessionStar(Guid sessionId, bool starred);
    Task<bool> UpdateSessionArchive(Guid sessionId, bool archived);
    Task<bool> UpdateSessionTitle(Guid sessionId, string title);
    Task<dynamic> GenerateEmbeddings(string? entityName);

    Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId);

    Task<string> BulkInsertRecordsAsync(BulkUploadRequest request);
}