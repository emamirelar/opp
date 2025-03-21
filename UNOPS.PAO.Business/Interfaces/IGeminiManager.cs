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
    Task<string> fetchResultFromGemini(AiPromptModel promptData, string relatedJsonData);
    Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type);
    Task<string> GetDataBasedOnScreenMapping(string type, int recordId, AiScreenMapping[] mapping);
    IEnumerable<AiChatSession> GetSessionData(Guid sessionId, int userId);
    IEnumerable<AiChatSession> GetUserSessions(int userId);
    Guid CreateNewSession(int userId);
    bool EndSession(Guid sessionId);
    Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId, string type);
    Task<string> EntityDetectionThroughGemini(IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request);
    Task<string> FetchDetailedResponseFromGemini(IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string promptType);
    JObject GetDetailsFromGeminiResponse(string modelResponse);
    bool UpdateChatHistoryTable(Guid sessionId, string originalMessage, string userMessage, string modelResponse, string entity, string intent, string promptType);
    void UpdateCurrentSessionIfInactive(int userId, Guid sessionId);
    Task<string> ProcessImage(IFormFile file);
    Task<string> ProcessAudio(IFormFile file);
    Task<string> ExtractDataFromFile(IFormFile file);
}