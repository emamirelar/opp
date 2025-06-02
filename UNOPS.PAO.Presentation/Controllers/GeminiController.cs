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
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class GeminiController : BaseController
{
    private readonly IGeminiManager _manager;

    public GeminiController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<GeminiController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.GeminiManager;
    }

    [HttpPost(APIDictionary.AiAssistantGetUserSessions)]
    public async Task<ActionResult> GetUserSessions() 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionData = _manager.GetUserSessions(CurrentUserId).ToList();
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantGetSession)]
    public async Task<ActionResult> GetSessionDetails([FromBody] GeminiSessionRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionData = _manager.GetSessionDataWithChats(req.sessionId, CurrentUserId).ToList();
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantCreateSession)]
    public async Task<ActionResult> CreateSession() 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionId = _manager.CreateNewSession(CurrentUserId);
            return new { sessionId = sessionId };
        });
    }

    [HttpPost(APIDictionary.AiAssistantEndSession)]
    public async Task<ActionResult> EndSession([FromBody] GeminiSessionRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            var success = _manager.EndSession(req.sessionId);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantChat)]
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || req?.sessionId == Guid.Empty)
            {
                throw new BusinessException("Invalid request.");
            }

            if (req.File != null)
            {
                var fileType = _manager.FindFileType(req.File);
                if (string.IsNullOrEmpty(fileType)) 
                {
                    throw new BusinessException("File type not compatible");
                }
            }

            return await _manager.ProcessChatWithGemini(req, CurrentUserId);
        });
    }

    [HttpPost(APIDictionary.GeminiFileScan)]
    public async Task<ActionResult> ScanFile([FromForm] GeminiFileRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req?.File == null || req?.File.Length == 0)
            {
                throw new BusinessException("No valid file detected.");
            }

            string fileType = _manager.FindFileType(req.File);

            if (string.IsNullOrEmpty(fileType)) 
            {
                throw new BusinessException("File type not compatible");
            }

            string response = await _manager.ScanFileForGeminiProcessing(req);

            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for the screen is not found.");
            }

            return response.Trim();
        });
    }

    [HttpPost(APIDictionary.GeminiProcessDataSummary)]
    // Internal call: Process Data Related Summary
    public async Task<ActionResult> ProcessDataRelatedSummaryDetails([FromBody] GeminiProcessDataRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || req?.Id == null)
            {
                throw new BusinessException("Invalid request");
            }

            var response = await _manager.ProcessDataRelatedSummaryDetails(req);
            
            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for the screen is not found.");
            }

            return response.Trim();
        });
    }

    [HttpPost(APIDictionary.AiAssistantAccessibility)]
    public async Task<ActionResult> UpdateAiAssistantAccessibility([FromBody] GeminiAccessibilityRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || req?.SessionId == Guid.Empty)
            {
                throw new BusinessException("Invalid request");
            }
            var success = await _manager.UpdateAiAssistantAccessibility(req);
            return new { success = success };
        });
    }

    [HttpGet(APIDictionary.GenerateEmbeddings)]
    public async Task<ActionResult> GenerateAndStoreEmbeddings(string? entityName)
    {
        return await HandleOperationAsync(async () => 
        {
            await _manager.GenerateEmbeddings(entityName);
            return new { message = $"Embeddings generated and published'." };
        });
    }

    [HttpPost(APIDictionary.BulkUpload)]
    public async Task<ActionResult> BulkUpload([FromBody] BulkUploadRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                throw new BusinessException("Invalid request.");
            }

            // Call the updated BulkInsertRecordsAsync method
            string response = await _manager.BulkInsertRecordsAsync(req);
            return new { message = response };
        });
    }

    [HttpPost(APIDictionary.AnalyseFile)]
    public async Task<ActionResult> AnalyseFile([FromBody]AnalyseFileRequest request)
    {
        return await HandleOperationAsync(async () => 
        {
            if (request == null)
            {
                throw new BusinessException("Invalid request.");
            }

            return await _manager.ExtractDataAfterAnalysis(request, CurrentUserId);
        });
    }
}
