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
    Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId);
    Task<string> EntityDetectionThroughGemini(IEnumerable<dynamic> formattedChatHistory, string message);
    Task<string> FetchDetailedResponseFromGemini(IEnumerable<dynamic> formattedChatHistory, string message, string promptType);
    JObject GetDetailsFromGeminiResponse(string modelResponse);
    bool UpdateChatHistoryTable(Guid sessionId, string userMessage, string modelResponse, string entity, string intent);
    void UpdateCurrentSessionIfInactive(int userId, Guid sessionId);
}