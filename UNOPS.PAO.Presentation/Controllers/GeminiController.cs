namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

[Route("/")]
[ApiController]
[Authorize]
public class GeminiController : ControllerBase
{
    private readonly IGeminiManager manager;
    private UserResolverService<int> userResolverService;
    private int currentUserId => userResolverService.GetCurrentUserId();

    public GeminiController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.GeminiManager;
        this.userResolverService = userResolverService;
    }

    [HttpPost(APIDictionary.AiAssistantGetUserSessions)]
    public async Task<ActionResult> GetUserSessions() {
        var sessionData = manager.GetUserSessions(currentUserId).ToList();
        return Ok(sessionData);
    }

    [HttpPost(APIDictionary.AiAssistantGetSession)]
    public async Task<ActionResult> GetSessionDetails([FromBody] GeminiSessionRequest req) {
        var sessionData = manager.GetSessionData(req.sessionId, currentUserId).ToList();
        return Ok(sessionData);
    }

    [HttpPost(APIDictionary.AiAssistantCreateSession)]
    public async Task<ActionResult> CreateSession() {
        // Check if an active session exists (where EndDate is null)
        var sessionId = manager.CreateNewSession(currentUserId);
        return Ok(new {sessionId = sessionId});
    }

    [HttpPost(APIDictionary.AiAssistantEndSession)]
    public async Task<ActionResult> EndSession([FromBody] GeminiSessionRequest req)
    {
        var success = manager.EndSession(req.sessionId);
        return Ok(new { success = success });
        
    }

    [HttpPost(APIDictionary.AiAssistantChat)]
    public async Task<ActionResult> ChatWithGemini([FromBody] GeminiAssistantRequest req) {

        if (req == null)
        {
            return BadRequest("Invalid request.");
        }

        // If any other session is active, mark it as inactive and activate this session (if required)
        manager.UpdateCurrentSessionIfInactive(currentUserId, req.sessionId);

        var chatHistory = await manager.GetChatHistory(req.sessionId);

        var formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.Message } }
        }).ToList();

        // Entity detection and intent classification to be done
        var entityDetectionResponse = await manager.EntityDetectionThroughGemini(formattedChatHistory, req.Message);
        var entityResponse = manager.GetDetailsFromGeminiResponse(entityDetectionResponse);

        var entity = entityResponse["Entity"].ToString();
        var intent = entityResponse["Intent"].ToString();
        var promptType = entityResponse["Type"].ToString();
        var modelMessage = entityResponse["Message"].ToString();
        var forward = entityResponse["Forward"].ToString();

        if (forward == "No") {
            manager.UpdateChatHistoryTable(req.sessionId, req.Message, modelMessage, entity, intent);
            return Ok(entityDetectionResponse);
        }
        
        var detailedResponse = await manager.FetchDetailedResponseFromGemini(formattedChatHistory, req.Message, promptType);
        var parsedDetailedResponse = manager.GetDetailsFromGeminiResponse(detailedResponse);

        manager.UpdateChatHistoryTable(req.sessionId, req.Message, modelMessage, entity, intent);
        
        return Ok(detailedResponse);
    }

    [HttpPost(APIDictionary.Gemini)]
    // Internal call: Create a Gemini
    public async Task<ActionResult> FetchResponseFromGemini([FromBody] GeminiProcessDataRequest req)
    {
        try {
           // bool isFromAiAssistant = req.AiAssistant;
            string relatedMessage = "";

            AiPrompt promptModel = manager.MapModelToEntity(req);

            // Call the GetPromptData method and get the first prompt
            var promptData = manager.GetPromptData(promptModel.Type).FirstOrDefault();

            if (promptData == null)
            {
                return NotFound(new { message = $"Prompt configuration for the screen '{req.Type}' is not found." });
            }

            // Query the AiScreenMapping table based on Type
            var screenMappings = (await manager.GetScreenMappingsByType(promptData.Type)).ToArray();
            relatedMessage = await manager.GetDataBasedOnScreenMapping(promptData.Type, req.Id, screenMappings);

            // Fetch result from Gemini
            return Ok(await manager.fetchResultFromGemini(promptData, relatedMessage));

        } catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }
}
