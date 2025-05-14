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
[Authorize(AuthenticationSchemes = "IAP")]
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
        var sessionData = manager.GetSessionDataWithChats(req.sessionId, currentUserId).ToList();
        return Ok(sessionData);
    }

    [HttpPost(APIDictionary.AiAssistantCreateSession)]
    public async Task<ActionResult> CreateSession() {
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
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) 
    {
        if (req == null || req?.sessionId == Guid.Empty)
        {
            return BadRequest("Invalid request.");
        }

        if (req.File != null)
        {
            var fileType = manager.FindFileType(req.File);
            if (string.IsNullOrEmpty(fileType)) {
                return StatusCode(500, new { message = "File type not compatible" });
            }
        }

        var response = await manager.ProcessChatWithGemini(req, currentUserId);
        
        return Ok(response);
    }

    [HttpPost(APIDictionary.GeminiFileScan)]
    public async Task<ActionResult> ScanFile([FromForm] GeminiFileRequest req) {
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

            string response = await manager.ScanFileForGeminiProcessing(req);

            if (string.IsNullOrEmpty(response))
            {
                return NotFound(new { message = $"Prompt configuration for the screen is not found." });
            }

            return Ok(response.Trim());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the file.", details = ex.Message });
        }
    }

    [HttpPost(APIDictionary.GeminiProcessDataSummary)]
    // Internal call: Process Data Related Summary
    public async Task<ActionResult> ProcessDataRelatedSummaryDetails([FromBody] GeminiProcessDataRequest req)
    {
        try {
            if (req == null || req?.Id == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var response = await manager.ProcessDataRelatedSummaryDetails(req);
            
            if (string.IsNullOrEmpty(response))
            {
                return NotFound(new { message = $"Prompt configuration for the screen is not found." });
            }

            return Ok(response.Trim());

        } catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost(APIDictionary.AiAssistantAccessibility)]
    public async Task<ActionResult> UpdateAiAssistantAccessibility([FromBody] GeminiAccessibilityRequest req)
    {
        try {
            if (req == null || req?.SessionId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid request" });
            }
            var success = await manager.UpdateAiAssistantAccessibility(req);
            return Ok(new { success = success });

        } catch(Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet(APIDictionary.GenerateEmbeddings)]
    public async Task<ActionResult> GenerateAndStoreEmbeddings(string? entityName)
    {
        try
        {
            var result = await manager.GenerateEmbeddings(entityName);

            return Ok(new { message = $"Embeddings generated and published'." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while generating embeddings.", details = ex.Message });
        }
    }

    [HttpPost(APIDictionary.BulkUpload)]
    public async Task<ActionResult> BulkUpload([FromBody] BulkUploadRequest req) {
        try {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                return BadRequest("Invalid request.");
            }

            // Call the updated BulkInsertRecordsAsync method
            string response = await manager.BulkInsertRecordsAsync(req);

            return Ok(new { message = response });
        } 
        catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost(APIDictionary.AnalyseFile)]
    public async Task<ActionResult> AnalyseFile([FromBody]AnalyseFileRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            var response = await manager.ExtractDataAfterAnalysis(request, currentUserId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the file.", details = ex.Message });
        }
    }
}
