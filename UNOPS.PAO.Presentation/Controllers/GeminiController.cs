namespace UNOPS.PAO.Presentation.Controllers;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Domain.Entities;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;

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
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) {

        if (req == null || req?.sessionId == Guid.Empty)
        {
            return BadRequest("Invalid request.");
        }

        if (string.IsNullOrEmpty(req?.Message)) {
            req.Message = "";
        }

        // If any other session is active, mark it as inactive and activate this session (if required)
        manager.UpdateCurrentSessionIfInactive(currentUserId, req.sessionId);

        string extractedText = "";
        string fileUrl = "";
        string fileType = "";

        if (req.File != null) {
            fileType = manager.FindFileType(req.File);
            if (string.IsNullOrEmpty(fileType)) {
                return StatusCode(500, new { message = "File type not compatible" });
            }
            extractedText = await manager.ExtractDataFromFile(req.File, fileType);
            fileUrl = await manager.UploadFileToGCS(req.File);
        }

        var chatHistory = await manager.GetChatHistory(req.sessionId, "entity_intent_detection");
        bool hasEntityHistory = false;

        req.Message = req.Message + "\\n " + extractedText + ".\\n"; 

        var formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.RawMessage } }
        }).ToList();

        // Entity detection and intent classification to be done
        var entityDetectionResponse = await manager.EntityDetectionThroughGemini(formattedChatHistory, req, fileUrl, fileType);
        var entityResponse = manager.GetDetailsFromGeminiResponse(entityDetectionResponse);
        var promptType = entityResponse["Type"]?.ToString();
        var forward = entityResponse["Forward"]?.ToString();
        var summary = entityResponse["Summary"]?.ToString();
        if (forward == string.Empty || forward == "No") {
            return Ok(entityDetectionResponse);
        }

        chatHistory = await manager.GetChatHistory(req.sessionId, promptType);

        formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.RawMessage } }
        }).ToList();

        req.Message = "Summary: " + summary;

        var detailedResponse = await manager.FetchDetailedResponseFromGemini(formattedChatHistory, req, promptType, fileUrl, fileType);
        var parsedDetailedResponse = manager.GetDetailsFromGeminiResponse(detailedResponse);
        
        return Ok(detailedResponse);
    }

    [HttpPost(APIDictionary.GeminiFileScan)]
    public async Task<ActionResult> ScanFile([FromForm] GeminiFileRequest req) {
        string errorMessage = "An error occurred while processing the file.";
        try
        {
            if (req?.File == null || req?.File.Length == 0)
            {
                return BadRequest(new { message = "No valid file detected." });
            }

            string fileType = manager.FindFileType(req.File);

            if (string.IsNullOrEmpty(fileType)) {
                return StatusCode(500, new { message = "File type not compatible" });
            }

            string extractedText = await manager.ExtractDataFromFile(req.File, fileType);
            string type = req?.Type;

            if (!string.IsNullOrEmpty(type)) {
                var promptData = manager.GetPromptData(type).FirstOrDefault();

                if (promptData == null)
                {
                    return NotFound(new { message = $"Prompt configuration for the screen '{type}' is not found." });
                }

                // Fetch result from Gemini
                return Ok(await manager.fetchResultFromGemini(promptData, extractedText));
            }

            return Ok(new { extractedText = extractedText.Trim() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = errorMessage, details = ex.Message });
        }
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
