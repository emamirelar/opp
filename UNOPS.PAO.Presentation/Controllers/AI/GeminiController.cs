namespace UNOPS.PAO.Presentation.Controllers.AI;

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
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Humanizer;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.GoogleServices;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Presentation.Controllers.Shared;

public class SearchResult
{
    public int EntityId { get; set; }
    public float Score { get; set; }
    public string SearchType { get; set; } = string.Empty;
}

public class StreamingActionResult : ActionResult
{
    private readonly IGeminiManager _manager;
    private readonly GeminiAssistantRequest _request;
    private readonly ClaimsPrincipal _user;
    private readonly IHeaderDictionary _headers;

    public StreamingActionResult(IGeminiManager manager, GeminiAssistantRequest request, ClaimsPrincipal user, IHeaderDictionary headers)
    {
        _manager = manager;
        _request = request;
        _user = user;
        _headers = headers;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        
        // Set SSE headers (remove Transfer-Encoding to avoid conflicts)
        response.Headers["Content-Type"] = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        response.Headers["Pragma"] = "no-cache";
        response.Headers["Expires"] = "0";
        response.Headers["Connection"] = "keep-alive";
        response.Headers["X-Accel-Buffering"] = "no";
        response.Headers["X-Proxy-Buffering"] = "no";
        response.Headers["Access-Control-Allow-Origin"] = "*";
        response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
        response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        
        try
        {
            Console.WriteLine("🌊 [CONTROLLER] Starting streaming response");
            var chunkCount = 0;
            var startTime = DateTime.UtcNow;
            
            await foreach (var chunk in _manager.ChatWithGeminiStreaming(_request, _user, _headers))
            {
                chunkCount++;
                var elapsed = DateTime.UtcNow - startTime;
                Console.WriteLine($"🌊 [CONTROLLER] Chunk #{chunkCount} (after {elapsed.TotalSeconds:F2}s): {chunk.Substring(0, Math.Min(100, chunk.Length))}...");
                
                // Format as proper SSE event if the chunk doesn't already have SSE formatting
                string formattedChunk;
                if (chunk.StartsWith("data: ") || chunk.StartsWith("event: "))
                {
                    // Already SSE formatted
                    formattedChunk = chunk.EndsWith("\n\n") ? chunk : chunk + "\n\n";
                    Console.WriteLine($"🌊 [CONTROLLER] Chunk #{chunkCount} already SSE formatted");
                }
                else
                {
                    // Raw JSON - format as SSE
                    // Ensure the JSON is properly escaped for SSE
                    var escapedChunk = chunk.Replace("\n", "\\n").Replace("\r", "\\r");
                    formattedChunk = $"data: {escapedChunk}\n\n";
                    Console.WriteLine($"🌊 [CONTROLLER] Chunk #{chunkCount} formatted raw JSON as SSE");
                }
                
                await response.WriteAsync(formattedChunk);
                await response.Body.FlushAsync();
                
                // Force immediate transmission to client
                if (response.HttpContext.Response.HasStarted)
                {
                    // Additional flush to ensure immediate delivery
                    await response.HttpContext.Response.Body.FlushAsync();
                }
                
                Console.WriteLine($"🌊 [CONTROLLER] Chunk #{chunkCount} sent to client");
            }
            var totalElapsed = DateTime.UtcNow - startTime;
            Console.WriteLine($"🌊 [CONTROLLER] Streaming completed successfully - {chunkCount} chunks in {totalElapsed.TotalSeconds:F2}s");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [CONTROLLER] Streaming error: {ex.Message}");
            var errorEvent = $"data: {{\"error\": \"An error occurred during streaming: {ex.Message}\"}}\n\n";
            await response.WriteAsync(errorEvent);
            await response.Body.FlushAsync();
        }
    }
}

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class GeminiController : BaseController
{
    private readonly IGeminiManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string? _agenticAiServiceUrl;
    private readonly CloudRunHelper _cloudRunHelper;
    private readonly UNOPSAppDbContext _context;

    public GeminiController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<GeminiController> logger,
        UNOPSAppDbContext context,
        AiContextualService aiService,
        IPermissionService permissionService,
        HttpClient httpClient,
        IConfiguration configuration)
        : base(logger, authorizationService, userResolverService, permissionService, context, aiService)
    {
        _manager = manager.GeminiManager;
        _managerWrapper = manager;
        _httpClient = httpClient;
        _configuration = configuration;
        _context = context;
        _agenticAiServiceUrl = _configuration.GetValue<string?>("AgenticAi:ServiceURL");
        
        // Initialize CloudRunHelper
        var cloudRunHelperLogger = new LoggerFactory().CreateLogger<CloudRunHelper>();
        var credentials = GetCredentials();
        _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, credentials);
    }
    
    private GoogleCredential GetCredentials()
    {
        var credentialParams = _configuration.GetSection("AISettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
        {
            throw new Exception("AISettings configuration is missing.");
        }
    
        var secretName = _configuration.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        if (string.IsNullOrEmpty(secretName))
        {
            throw new Exception("AISettings:AIServiceAccountJSONSecretName is not configured.");
        }

        var basicProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
#pragma warning disable CS0618 // Type or member is obsolete - migration to CredentialFactory pending
        var credential = GoogleCredential.FromJson(secretValue);
#pragma warning restore CS0618
        
        return credential;
    }

    #region AI Prompt Management Endpoints

    /// <summary>
    /// Retrieves all AI prompts with advanced filtering, pagination, search capabilities, and access control for prompt management.
    /// </summary>
    /// <param name="request">AI prompt filter request containing search and pagination parameters</param>
    /// <example_uses>
    /// Show me all AI prompts
    /// List prompts for GPT-4 model
    /// Find prompts containing 'email' in the name
    /// Get prompts for specific project
    /// Show active prompts with pagination
    /// Search for customer service prompts
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search, list, filter, or browse AI prompts and prompt templates.</when_to_use>
    /// <returns>Paginated list of AI prompts with metadata</returns>
    [HttpPost(APIDictionary.AiPromptsList)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptsAsync([FromBody] AiPromptFilterRequest request)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptsAsync(User, request);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific AI prompt by ID with complete details including prompt content, parameters, and configuration.
    /// </summary>
    /// <param name="id">AI prompt ID</param>
    /// <example_uses>
    /// Show me details for AI prompt ID 123
    /// Get full information about prompt 456
    /// Display prompt record 789
    /// Get complete prompt configuration
    /// Show prompt with all parameters and content
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific AI prompt details by ID or when you need complete prompt information for editing or testing.</when_to_use>
    /// <returns>Complete AI prompt details with content and configuration</returns>
    [HttpGet(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptByIdAsync(int id)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptByIdAsync(User, id);
        if (result == null)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a new AI prompt with complete configuration including content, parameters, model settings, and metadata.
    /// </summary>
    /// <param name="model">AI prompt creation request with all configuration details</param>
    /// <example_uses>
    /// Create a new email generation prompt
    /// Add a customer service response template
    /// Set up a new data analysis prompt
    /// Create prompt for document summarization
    /// Add meeting notes generation template
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to create, add, or set up a new AI prompt template.</when_to_use>
    /// <returns>Created AI prompt with ID and metadata</returns>
    [HttpPost(APIDictionary.AiPrompts)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> CreatePromptAsync([FromBody] AiPromptModel model)
    {
        // RBAC interceptor handles permission checking
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _managerWrapper.AiPromptManager.CreatePromptAsync(User, model);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Updates an existing AI prompt's configuration including content, parameters, model settings, and metadata.
    /// </summary>
    /// <param name="id">AI prompt ID to update (required)</param>
    /// <param name="model">Updated AI prompt data</param>
    /// <example_uses>
    /// Update prompt 123's content with new template
    /// Change prompt 456's model from GPT-3.5 to GPT-4
    /// Modify prompt temperature settings
    /// Update prompt description and metadata
    /// Change prompt type classification
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or change AI prompt configuration or content.</when_to_use>
    /// <returns>Updated AI prompt data</returns>
    [HttpPut(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "update")]
    public async Task<ActionResult> UpdatePromptAsync(int id, [FromBody] AiPromptModel model)
    {
        // RBAC interceptor handles permission checking
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _managerWrapper.AiPromptManager.UpdatePromptAsync(User, id, model);
        if (result == null)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Exports all AI prompts as a SQL script file for seeding
    /// </summary>
    /// <returns>SQL script file content for seeding AI prompts</returns>
    /// <example_uses>
    /// Export AI prompts as SQL script
    /// Download SQL file for AI prompts
    /// Generate SQL version of prompts for seeding
    /// Export prompts as SQL with PROJECT_ID placeholder
    /// </example_uses>
    /// <when_to_use>Use this when you need to export AI prompts as SQL scripts for database seeding with configurable PROJECT_ID.</when_to_use>
    [HttpGet(APIDictionary.AiPrompts + "/export-sql")]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> ExportAiPromptsAsSqlAsync()
    {
        // RBAC interceptor handles permission checking
        var sqlScript = await _managerWrapper.AiPromptManager.ExportAiPromptsAsSqlAsync(User);
        
        var fileName = $"05_AiPrompts_{DateTime.UtcNow:yyyyMMddHHmmss}.sql";
        var contentType = "text/plain";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(sqlScript);
        
        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// Soft deletes an AI prompt from the system (marks as deleted rather than permanent removal).
    /// </summary>
    /// <param name="id">AI prompt ID to delete</param>
    /// <example_uses>
    /// Delete AI prompt ID 123
    /// Remove prompt 456 from the system
    /// Deactivate email template prompt
    /// Delete outdated prompt template
    /// Remove unused AI prompt
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to delete, remove, or eliminate an AI prompt.</when_to_use>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "delete")]
    public async Task<ActionResult> DeletePromptAsync(int id)
    {
        // RBAC interceptor handles permission checking
        var success = await _managerWrapper.AiPromptManager.DeletePromptAsync(User, id);
        if (!success)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Retrieves all AI prompts filtered by specific prompt type with access control.
    /// </summary>
    /// <param name="type">Prompt type to filter by (e.g., 'email', 'summary', 'analysis', 'translation')</param>
    /// <example_uses>
    /// Show all email generation prompts
    /// List prompts for document summarization
    /// Get data analysis prompt templates
    /// Find translation prompts
    /// Show customer service prompts
    /// Get meeting notes prompts
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to filter prompts by specific type, category, or functional purpose.</when_to_use>
    /// <returns>List of AI prompts matching the specified type</returns>
    [HttpGet(APIDictionary.AiPromptsByType + "/{type}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptsByTypeAsync(string type)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptsByTypeAsync(User, type);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all unique prompt types available in the system for dropdown menus and filtering operations.
    /// </summary>
    /// <example_uses>
    /// Get prompt types for dropdown menu
    /// What types of prompts are available?
    /// Show all prompt categories
    /// Get filter options for prompt types
    /// List available prompt classifications
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to see available prompt types for filtering or when building UI dropdown menus.</when_to_use>
    /// <returns>List of unique prompt types for filtering and selection</returns>
    [HttpGet(APIDictionary.AiPromptsTypes)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptTypesAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptTypesAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all unique AI models available in the system for dropdown menus and filtering operations.
    /// </summary>
    /// <example_uses>
    /// Get AI models for dropdown menu
    /// What AI models are available?
    /// Show all supported models
    /// Get filter options for AI models
    /// List GPT and other model options
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to see available AI models for filtering or when configuring prompt model settings.</when_to_use>
    /// <returns>List of unique AI models for filtering and selection</returns>
    [HttpGet(APIDictionary.AiPromptsModels)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetModelsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetModelsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all unique projects associated with AI prompts for dropdown menus and filtering operations.
    /// </summary>
    /// <example_uses>
    /// Get projects for dropdown menu
    /// What projects have AI prompts?
    /// Show all project assignments
    /// Get filter options for projects
    /// List projects using AI prompts
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to see available projects for filtering prompts or when assigning prompts to projects.</when_to_use>
    /// <returns>List of unique projects for filtering and selection</returns>
    [HttpGet(APIDictionary.AiPromptsProjects)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetProjectsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetProjectsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all unique locations/regions associated with AI prompts for dropdown menus and filtering operations.
    /// </summary>
    /// <example_uses>
    /// Get locations for dropdown menu
    /// What locations have AI prompts?
    /// Show all regional assignments
    /// Get filter options for locations
    /// List regions using AI prompts
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to see available locations for filtering prompts or when assigning prompts to regions.</when_to_use>
    /// <returns>List of unique locations for filtering and selection</returns>
    [HttpGet(APIDictionary.AiPromptsLocations)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetLocationsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetLocationsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Tests an AI prompt with provided test data to validate prompt effectiveness and output quality before deployment.
    /// </summary>
    /// <param name="request">Test request containing prompt data and test parameters</param>
    /// <example_uses>
    /// Test email prompt with sample data
    /// Validate prompt 123 with test input
    /// Check prompt performance before deployment
    /// Test prompt output quality
    /// Verify prompt generates expected results
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to test, validate, or preview AI prompt output before using it in production.</when_to_use>
    /// <returns>Test results including prompt output and performance metrics</returns>
    [HttpPost(APIDictionary.AiPromptsTest)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> TestPromptAsync([FromBody] TestPromptRequest request)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.TestPromptAsync(User, request);
        return Ok(result);
    }

    /// <summary>
    /// Upgrades all AI prompts to use the latest available Gemini model
    /// </summary>
    /// <returns>Upgrade result with updated count and status</returns>
    [HttpPost(APIDictionary.AiPromptsUpgradeModel)]
    [AccessControlled(EntityTypes.AiPromptManagement, "update")]
    public async Task<ActionResult> UpgradeGeminiModel()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.UpgradeToLatestGeminiModelAsync(User);
        return Ok(result);
    }

    #endregion


    #region Existing Gemini Endpoints

    [HttpPost(APIDictionary.AiAssistantGetUserSessions)]
    public async Task<ActionResult> GetUserSessions() 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionData = (await _manager.GetUserSessions(CurrentUserId)).ToList();
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantGetSession)]
    public async Task<ActionResult> GetSessionDetails([FromBody] GeminiSessionRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await _manager.GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"{serviceUrl.TrimEnd('/')}/session-with-chats?app_name={appName}&user_id={CurrentUserId}&session_id={req.sessionId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                // Return the raw JSON content as-is without any deserialization or transformation
                return Content(jsonContent, "application/json");
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch session with chats from external API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        });
    }

    [HttpPost(APIDictionary.AiAssistantChat)]
    [HttpHead(APIDictionary.AiAssistantChat)]
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) 
    {
        // Handle HEAD requests
        if (Request.Method == "HEAD")
        {
            return Ok();
        }

        // Check if streaming is requested
        var streaming = req.Streaming;
        streaming = true; // Force streaming mode for testing
        if (streaming)
        {
            Console.WriteLine("📡 [CONTROLLER] Streaming mode requested");
            
            // Return Server-Sent Events stream
            return new StreamingActionResult(_manager, req, User, Request.Headers);
        }
        else
        {
            Console.WriteLine("📄 [CONTROLLER] Regular (non-streaming) mode requested");
            
            // Handle regular non-streaming request
            return await HandleOperationAsync(async () => 
            {
                return await _manager.ChatWithGemini(req, User, Request.Headers);
            });
        }
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

            // Pass User context like other methods (e.g., ChatWithGemini)
            var response = await _manager.ProcessDataRelatedSummaryDetails(req, User);
            
            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for the screen is not found.");
            }

            return response.Trim();
        });
    }

    /// <summary>
    /// Transcribes opportunity document using Gemini AI to extract structured opportunity data.
    /// Receives documentId, retrieves document content and type, sends to Gemini for analysis.
    /// </summary>
    /// <param name="req">Request containing document ID</param>
    /// <returns>JSON string with extracted opportunity information matching OpportunityModel structure</returns>
    [HttpPost(APIDictionary.GeminiDocumentTranscribe)]
    public async Task<ActionResult> TranscribeOpportunityDocument([FromBody] GeminiProcessDataRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || req.Id <= 0)
            {
                throw new BusinessException("Invalid document ID");
            }

            // Get document from database to retrieve storage path
            var document = await _context.Documents.FindAsync(req.Id);
            if (document == null)
            {
                throw new BusinessException("Document not found");
            }

            // Set the type to trigger the correct AI prompt
            req.Type = "opportunity_document_transcribe";
            
            // Pass document storage path and MIME type for AI processing
            req.DocumentStoragePath = document.StoragePath; // gs:// URI
            req.DocumentMimeType = document.Type ?? "application/pdf";

            // Pass User context for authorization
            var response = await _manager.ProcessDataRelatedSummaryDetails(req, User);
            
            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Failed to transcribe document. AI analysis returned no results.");
            }
            
            // Mark document as AI transcribed
            document.AITranscribed = true;
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            return response.Trim();
        });
    }

    [HttpPost(APIDictionary.AiAssistantAccessibility)]
    public async Task<ActionResult> UpdateAiAssistantAccessibility([FromBody] GeminiAccessibilityRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            var success = await _manager.UpdateAiAssistantAccessibility(req);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateStar)]
    public async Task<ActionResult> UpdateSessionStar([FromBody] SessionStarRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionStar(req.SessionId, req.Starred);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateArchive)]
    public async Task<ActionResult> UpdateSessionArchive([FromBody] SessionArchiveRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionArchive(req.SessionId, req.Archived);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateTitle)]
    public async Task<ActionResult> UpdateSessionTitle([FromBody] SessionTitleRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId) || string.IsNullOrEmpty(req.Title))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionTitle(req.SessionId, req.Title.Trim());
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








    /// <summary>
    /// Helper method to build and execute the query with proper generic typing
    /// </summary>
    private async Task<IEnumerable<object>> BuildAndExecuteQuery<T>(object dbSet, List<int> entityIds, string singularizedEntityName) where T : class
    {
        var typedDbSet = (IQueryable<T>)dbSet;
        
        // Find the correct Id property to avoid ambiguity
        var idProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int))
            .ToList();
        
        PropertyInfo idPropertyInfo;
        if (idProperties.Count == 1)
        {
            idPropertyInfo = idProperties.First();
        }
        else if (idProperties.Count > 1)
        {
            // Prefer properties declared on the actual type over inherited ones
            idPropertyInfo = idProperties
                .OrderBy(p => p.DeclaringType == typeof(T) ? 0 : 1)
                .First();
        }
        else
        {
            throw new InvalidOperationException($"No 'Id' property found on entity type {typeof(T).Name}");
        }

        // Build the query with proper typing
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, idPropertyInfo);
        var containsMethod = typeof(List<int>).GetMethod(nameof(List<int>.Contains));
        if (containsMethod == null)
        {
            throw new InvalidOperationException("Contains method not found on List<int>");
        }
        var containsCall = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(entityIds), 
            containsMethod, 
            property);
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        // Apply the where condition with proper typing
        var filteredQuery = typedDbSet.Where(lambda);

        // Apply RBAC filtering with proper typing
        if (_permissionService == null)
        {
            throw new InvalidOperationException("Permission service is not available");
        }
        var rbacFilteredQueryResult = await _permissionService.ApplyAccessControlFiltersAsync<T>(
            filteredQuery, 
            User, 
            "read", 
            singularizedEntityName);

        // Handle both cases: IQueryable<T> or List<T>
        List<T> results;
        if (rbacFilteredQueryResult is IQueryable<T> queryable)
        {
            // If it's still a queryable, execute it
            results = await queryable.ToListAsync();
        }
        else if (rbacFilteredQueryResult is List<T> list)
        {
            // If it's already materialized as a list, use it directly
            results = list;
        }
        else
        {
            // Try to cast it as IEnumerable<T> and convert to list
            results = ((IEnumerable<T>)rbacFilteredQueryResult).ToList();
        }

        return results.Cast<object>();
    }

    #endregion

    #region Opportunity Statement Generation

    /// <summary>
    /// Generates an AI-powered opportunity statement in markdown format following the UNOPS template
    /// Retrieves opportunity data and attached documents, sends to Gemini for analysis
    /// Results are cached and saved to the Opportunity entity
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <example_uses>
    /// Generate opportunity statement for opportunity 123
    /// Create opportunity statement document
    /// Generate formal opportunity proposal statement
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to generate a formal opportunity statement document following the UNOPS template format.</when_to_use>
    /// <returns>Generated opportunity statement in markdown format</returns>
    [HttpPost(APIDictionary.OpportunityGenerateStatement)]
    public async Task<ActionResult> GenerateOpportunityStatement(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var statementMarkdown = await _manager.GenerateOpportunityStatementAsync(id, User);
            
            if (string.IsNullOrEmpty(statementMarkdown))
            {
                throw new BusinessException("Failed to generate opportunity statement. AI analysis returned no results.");
            }

            return new
            {
                opportunityId = id,
                statementMarkdown = statementMarkdown,
                message = "Opportunity statement generated successfully"
            };
        });
    }

    /// <summary>
    /// Validates whether the existing opportunity statement is up-to-date by comparing it against a freshly generated statement
    /// Generates a new statement based on current data (without saving) and compares it with the existing statement
    /// Returns whether the statements are aligned and specific differences if not aligned
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <example_uses>
    /// Validate opportunity statement alignment for opportunity 123
    /// Check if existing statement is outdated compared to current data
    /// Find differences between existing and freshly generated statements
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to verify that the opportunity statement is up-to-date with current opportunity data, or to see what would change if the statement were regenerated.</when_to_use>
    /// <returns>Validation response with alignment status, misalignment items, and the freshly generated statement for reference</returns>
    [HttpPost(APIDictionary.OpportunityValidateStatement)]
    public async Task<ActionResult> ValidateOpportunityStatement(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var validationResult = await _manager.ValidateOpportunityStatementAsync(id, User);
            
            if (validationResult == null)
            {
                throw new BusinessException("Failed to validate opportunity statement. AI analysis returned no results.");
            }

            return validationResult;
        });
    }

    #endregion
}
