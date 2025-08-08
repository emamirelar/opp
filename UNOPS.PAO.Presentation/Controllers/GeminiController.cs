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
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Humanizer;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Attributes;

public class SearchResult
{
    public int EntityId { get; set; }
    public float Score { get; set; }
    public string SearchType { get; set; }
}

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class GeminiController : BaseController
{
    private readonly IGeminiManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _agenticAiServiceUrl;

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
        _agenticAiServiceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
    }

    #region AI Prompt Management Endpoints

    /// <summary>
    /// Retrieves all AI prompts with advanced filtering, pagination, search capabilities, and access control for prompt management.
    /// </summary>
    /// <param name="request">AI prompt filter request containing search and pagination parameters</param>
    /// <param name="request.pageIndex">Page number (1-based)</param>
    /// <param name="request.pageSize">Number of items per page</param>
    /// <param name="request.searchText">Text to search across prompt fields</param>
    /// <param name="request.orderBy">Field to order results by</param>
    /// <param name="request.ascending">Sort direction (true for ascending)</param>
    /// <param name="request.type">Filter by prompt type</param>
    /// <param name="request.model">Filter by AI model</param>
    /// <param name="request.project">Filter by project</param>
    /// <param name="request.location">Filter by location</param>
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
    /// <param name="model.name">Prompt name (required)</param>
    /// <param name="model.type">Prompt type/category (required)</param>
    /// <param name="model.content">Prompt content/template (required)</param>
    /// <param name="model.description">Description of prompt purpose</param>
    /// <param name="model.model">AI model to use (GPT-4, GPT-3.5, etc.)</param>
    /// <param name="model.project">Associated project</param>
    /// <param name="model.location">Location/region</param>
    /// <param name="model.temperature">Model temperature setting</param>
    /// <param name="model.maxTokens">Maximum tokens for response</param>
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
    /// <param name="model.name">Updated prompt name</param>
    /// <param name="model.type">Updated prompt type</param>
    /// <param name="model.content">Updated prompt content/template</param>
    /// <param name="model.description">Updated description</param>
    /// <param name="model.model">Updated AI model selection</param>
    /// <param name="model.temperature">Updated temperature setting</param>
    /// <param name="model.maxTokens">Updated token limit</param>
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
    /// <param name="request.promptId">ID of prompt to test</param>
    /// <param name="request.testData">Test input data for prompt validation</param>
    /// <param name="request.parameters">Additional parameters for testing</param>
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
            var sessionData = await _manager.GetSessionDataWithChats(req.sessionId, CurrentUserId);
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantChat)]
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            // Debug logging for file upload
            if (req.Files != null && req.Files.Any())
            {
                Console.WriteLine($"📎 [CONTROLLER] Received {req.Files.Count()} files:");
                foreach (var file in req.Files)
                {
                    Console.WriteLine($"   - {file.FileName} ({file.ContentType}, {file.Length} bytes)");
                }
            }
            else
            {
                Console.WriteLine("📎 [CONTROLLER] No files received in request");
            }
            
            return await _manager.ChatWithGemini(req, User, Request.Headers);
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





    [HttpGet(APIDictionary.AiAssistantGenerateTitle)]
    public async Task<ActionResult> GenerateTitle([FromQuery] string sessionId)
    {
        return await HandleOperationAsync(async () =>
        {
            var title = await _manager.GenerateTitle(sessionId, CurrentUserId);
            return new { title };
        });
    }

    public class GenerateTitleResponse
    {
        public string session_id { get; set; }
        public string formatted_conversation { get; set; }
        public string title { get; set; }
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
        var containsCall = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(entityIds), 
            containsMethod, 
            property);
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        // Apply the where condition with proper typing
        var filteredQuery = typedDbSet.Where(lambda);

        // Apply RBAC filtering with proper typing
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
}
