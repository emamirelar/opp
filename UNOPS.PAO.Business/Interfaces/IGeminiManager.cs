using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGeminiManager
{
    AiPrompt MapModelToEntity(GeminiProcessDataRequest req);
    IEnumerable<AiPromptModel> GetPromptData(string type);
    Task<string> FetchResultFromGemini(AiPromptModel promptData, string relatedJsonData);
    Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type);
    Task<string> GetDataBasedOnScreenMapping(string type, int recordId, AiScreenMapping[] mapping);
    IEnumerable<AiChatSession> GetSessionDataWithChats(Guid sessionId, int userId);
    Task<IEnumerable<AiChatSession>> GetSessionData(Guid sessionId, int userId);
    IEnumerable<AiChatSession> GetUserSessions(int userId);
    Guid CreateNewSession(int userId);
    bool EndSession(Guid sessionId);
    Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId, string type);
    Task<dynamic> EntityDetectionThroughGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string fileUrl, string fileType);
    Task<dynamic> FetchDetailedResponseFromGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string promptType, string fileUrl, string fileType);
    JObject GetDetailsFromGeminiResponse(string modelResponse);
    Task<AiChatSession> UpdateCurrentSessionIfInactive(int userId, Guid sessionId);
    Task<string> ExtractDataFromFile(IFormFile file);
    string FindFileType(IFormFile file);
    Task<string> UploadFileToGCS(IFormFile file);
    Task<dynamic> ProcessChatWithGemini(GeminiAssistantRequest req, int currentUserId);
    Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req);
    Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req);
    Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req);
}