namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

public class UNOPSAiPromptManager : BaseUNOPSManager, IAiPromptManager
{
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly IManagerWrapper _managerWrapper;

    public UNOPSAiPromptManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration, 
        UserManager<PAOIdentityUser> userManager,
        IManagerWrapper managerWrapper,
        IPermissionService permissionService)
        : base(mapper, context, configuration, userManager, "AiPrompt", permissionService)
    {
        _promptRepository = new DataRepository<AiPrompt>(context);
        _managerWrapper = managerWrapper;
    }

    /// <summary>
    /// Tests an AI prompt with provided test data using the new function-based pattern
    /// </summary>
    public async Task<TestPromptResponse> TestPromptAsync(ClaimsPrincipal user, TestPromptRequest request)
    {
        // RBAC interceptor handles security enforcement
        try
        {
            // Validate that either ID or TestData is provided
            if (!request.Id.HasValue && string.IsNullOrEmpty(request.TestData))
            {
                return new TestPromptResponse
                {
                    Success = false,
                    Error = "Either EntityId or TestData must be provided"
                };
            }

            object entityData = null;
            
            // If ID is provided, get entity data from database
            if (request.Id.HasValue)
            {
                // Look up the AI prompt configuration by type to get entity information
                var aiPrompt = await _promptRepository.GetAll()
                    .Where(p => p.Type == request.Type)
                    .FirstOrDefaultAsync();
                    
                if (aiPrompt == null)
                {
                    return new TestPromptResponse
                    {
                        Success = false,
                        Error = $"AI prompt configuration not found for type '{request.Type}'"
                    };
                }

                // Get entity data using the function name from the database
                entityData = await GetEntityDataAsync(aiPrompt.Name, aiPrompt.PromptFunction, request.Id.Value, user);
            }
            
            // If no ID provided, use the first available AI prompt for this type (testData mode)
            var promptConfig = await _promptRepository.GetAll()
                .Where(p => p.Type == request.Type)
                .FirstOrDefaultAsync();
                
            if (promptConfig == null)
            {
                return new TestPromptResponse
                {
                    Success = false,
                    Error = $"AI prompt configuration not found for type '{request.Type}'"
                };
            }
            
            // Create AiContextualService instance
            var aiContextualService = new AiContextualService(_configuration, _context, null);

            // Use values from database or request overrides
            var promptModel = new AiPrompt
            {
                Type = promptConfig.Type,
                Prompt = request.Prompt ?? promptConfig.Prompt,
                Model = request.Model ?? promptConfig.Model,
                Project = request.Project ?? promptConfig.Project,
                Location = request.Location ?? promptConfig.Location,
                GenerationConfig = JsonConvert.SerializeObject(new
                {
                    temperature = request.Temperature ?? ExtractTemperatureFromConfig(promptConfig.GenerationConfig),
                    top_p = request.TopP ?? ExtractTopPFromConfig(promptConfig.GenerationConfig),
                    max_output_tokens = request.MaxOutputTokens ?? ExtractMaxTokensFromConfig(promptConfig.GenerationConfig)
                }),
                ContentConfig = promptConfig.ContentConfig,
                ToolsConfig = request.GoogleSearch.HasValue 
                    ? (request.GoogleSearch.Value ? JsonConvert.SerializeObject(new { googleSearch = new { } }) : null)
                    : promptConfig.ToolsConfig,
                SafetySettings = request.SafetySettings ?? promptConfig.SafetySettings
            };

            // Prepare data for AI processing
            string dataForAI;
            
            if (request.Id.HasValue)
            {
                // Entity ID mode: Get real entity data and send as JSON
                // Serialize the entity data to JSON for AI processing with enum string conversion
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                dataForAI = JsonConvert.SerializeObject(entityData, settings);
            }
            else
            {
                // Test Data mode: Send test data directly
                dataForAI = request.TestData;
            }

            // Call the Gemini API with the data
            var result = await aiContextualService.FetchResultFromGemini(promptModel, dataForAI);

            return new TestPromptResponse
            {
                Success = true,
                Response = result
            };
        }
        catch (Exception ex)
        {
            return new TestPromptResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets entity data by calling the specified function on the appropriate manager
    /// </summary>
    private async Task<object> GetEntityDataAsync(string entityType, string functionName, int entityId, ClaimsPrincipal user)
    {
        // Get the appropriate manager
        var manager = GetManagerByEntityType(entityType);
        
        // Call the specific function on the manager with user context
        return await ((BaseUNOPSManager)manager).CallFunctionByNameAsync(functionName, entityId, user);
    }

    /// <summary>
    /// Gets the appropriate manager using minimal, safe reflection
    /// </summary>
    private BaseUNOPSManager GetManagerByEntityType(string entityType)
    {
        // Create field name pattern: entityType -> partnerManager, contactManager, etc.
        var fieldName = $"{entityType.ToLower()}Manager";
        var manager = GetUNOPSManagerByReflection(fieldName);

        if (manager == null)
            throw new ArgumentException($"Manager for entity type '{entityType}' does not implement BaseUNOPSManager");

        return manager;
    }

    /// <summary>
    /// Gets UNOPS manager instances using reflection to access private fields
    /// </summary>
    private BaseUNOPSManager GetUNOPSManagerByReflection(string fieldName)
    {
        // Get the private field from UNOPSManagerWrapper that contains the actual UNOPS manager instance
        var field = _managerWrapper.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var manager = field?.GetValue(_managerWrapper) as BaseUNOPSManager;
        
        return manager ?? throw new ArgumentException($"Manager not found or doesn't inherit from BaseUNOPSManager: {fieldName}");
    }

    /// <summary>
    /// Gets all AI prompts with pagination
    /// </summary>
    public async Task<PaginationResponse<AiPromptModel>> GetPromptsAsync(ClaimsPrincipal user, AiPromptFilterRequest request)
    {
        // RBAC interceptor handles security enforcement
        var query = _promptRepository.GetAll().AsQueryable();

        // Only show prompts that can be changed by admins
        query = query.Where(p => p.AdminCanChange == true);

        // Apply search if provided
        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(p => 
                p.Type.Contains(request.SearchText) ||
                p.Model.Contains(request.SearchText) ||
                p.Project.Contains(request.SearchText) ||
                p.Location.Contains(request.SearchText) ||
                (p.Prompt != null && p.Prompt.Contains(request.SearchText)));
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply ordering
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        // Apply pagination
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        var prompts = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();

        var mappedPrompts = prompts.Select(p => _mapper.Map<AiPromptModel>(p)).ToList();

        return new PaginationResponse<AiPromptModel>
        {
            Records = mappedPrompts,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Gets a specific AI prompt by ID
    /// </summary>
    public async Task<AiPromptModel?> GetPromptByIdAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var prompt = await _promptRepository.GetByIdAsync(id);
        return prompt != null ? _mapper.Map<AiPromptModel>(prompt) : null;
    }

    /// <summary>
    /// Creates a new AI prompt
    /// </summary>
    public async Task<AiPromptModel> CreatePromptAsync(ClaimsPrincipal user, AiPromptModel model)
    {
        // RBAC interceptor handles security enforcement
        var entity = _mapper.Map<AiPrompt>(model);
        entity.CreatedAt = DateTime.UtcNow;
        entity.Id = null; // Ensure new entity

        await _promptRepository.AddAsync(entity);

        return _mapper.Map<AiPromptModel>(entity);
    }

    /// <summary>
    /// Updates an existing AI prompt
    /// </summary>
    public async Task<AiPromptModel?> UpdatePromptAsync(ClaimsPrincipal user, int id, AiPromptModel model)
    {
        // RBAC interceptor handles security enforcement
        var existingPrompt = await _promptRepository.GetByIdAsync(id);
        if (existingPrompt == null)
        {
            return null;
        }

        // Map the model to the existing entity, but preserve the ID and CreatedAt
        var originalId = existingPrompt.Id;
        var originalCreatedAt = existingPrompt.CreatedAt;
        
        _mapper.Map(model, existingPrompt);
        
        existingPrompt.Id = originalId;
        existingPrompt.CreatedAt = originalCreatedAt;

        await _promptRepository.UpdateAsync(existingPrompt);

        return _mapper.Map<AiPromptModel>(existingPrompt);
    }

    /// <summary>
    /// Deletes an AI prompt
    /// </summary>
    public async Task<bool> DeletePromptAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var existingPrompt = await _promptRepository.GetByIdAsync(id);
        if (existingPrompt == null)
        {
            return false;
        }

        await _promptRepository.Delete(existingPrompt);
        return true;
    }

    /// <summary>
    /// Gets prompts by type
    /// </summary>
    public async Task<IEnumerable<AiPromptModel>> GetPromptsByTypeAsync(ClaimsPrincipal user, string type)
    {
        // RBAC interceptor handles security enforcement
        var prompts = await _promptRepository
            .GetAll()
            .Where(p => p.Type == type)
            .ToListAsync();

        return prompts.Select(p => _mapper.Map<AiPromptModel>(p));
    }

    /// <summary>
    /// Gets unique prompt types for dropdown/filter
    /// </summary>
    public async Task<IEnumerable<string>> GetPromptTypesAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        return await _promptRepository
            .GetAll()
            .Select(p => p.Type)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unique models for dropdown/filter
    /// </summary>
    public async Task<IEnumerable<string>> GetModelsAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        return await _promptRepository
            .GetAll()
            .Select(p => p.Model)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unique projects for dropdown/filter
    /// </summary>
    public async Task<IEnumerable<string>> GetProjectsAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        return await _promptRepository
            .GetAll()
            .Select(p => p.Project)
            .Distinct()
            .OrderBy(p => p)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unique locations for dropdown/filter
    /// </summary>
    public async Task<IEnumerable<string>> GetLocationsAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        return await _promptRepository
            .GetAll()
            .Select(p => p.Location)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }

    /// <summary>
    /// Gets basic entity data for AI prompts and generic operations
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        if (user != null)
        {
            return await GetPromptByIdAsync(user, entityId);
        }
        
        // Fallback for cases without user context
        var prompt = await _promptRepository.GetByIdAsync(entityId);
        return prompt != null ? _mapper.Map<AiPromptModel>(prompt) : null;
    }

    // Helper methods to extract values from JSON configuration
    private double ExtractTemperatureFromConfig(string generationConfig)
    {
        try
        {
            var config = JsonConvert.DeserializeObject<dynamic>(generationConfig);
            return config?.temperature ?? 0.7;
        }
        catch
        {
            return 0.7; // Default value
        }
    }

    private double ExtractTopPFromConfig(string generationConfig)
    {
        try
        {
            var config = JsonConvert.DeserializeObject<dynamic>(generationConfig);
            return config?.top_p ?? 0.9;
        }
        catch
        {
            return 0.9; // Default value
        }
    }

    private int ExtractMaxTokensFromConfig(string generationConfig)
    {
        try
        {
            var config = JsonConvert.DeserializeObject<dynamic>(generationConfig);
            return config?.max_output_tokens ?? 1000;
        }
        catch
        {
            return 1000; // Default value
        }
    }

    public async Task<GeminiModelUpgradeResult> UpgradeToLatestGeminiModelAsync(ClaimsPrincipal user)
    {
        try
        {
            // Use the latest available model value - hardcoded as the newest in system
            var latestModelValue = "gemini-2.5-flash";
            var latestModelDisplay = "Gemini 2.5 Flash";

            // Get all prompts using the repository's existing methods
            var allPrompts = _promptRepository.GetAll().ToList();
            var promptsToUpdate = allPrompts.Where(p => p.Model != latestModelValue).ToList();

            if (!promptsToUpdate.Any())
            {
                return new GeminiModelUpgradeResult
                {
                    Success = true,
                    UpdatedCount = 0,
                    Message = "All AI prompts are already using the latest available model configured in the system.",
                    LatestModel = latestModelDisplay,
                    AlreadyLatest = true
                };
            

            // Update all prompts to use the latest model
            foreach (var prompt in promptsToUpdate)
            {
                prompt.Model = latestModelValue;
                await _promptRepository.UpdateAsync(prompt);
            }

            return new GeminiModelUpgradeResult
            {
                Success = true,
                UpdatedCount = promptsToUpdate.Count(),
                Message = $"Successfully upgraded {promptsToUpdate.Count()} AI prompts to {latestModelDisplay}.",
                LatestModel = latestModelDisplay,
                AlreadyLatest = false
            };
        }
        catch (Exception ex)
        {
            return new GeminiModelUpgradeResult
            {
                Success = false,
                UpdatedCount = 0,
                Message = $"Error upgrading models: {ex.Message}",
                LatestModel = null,
                AlreadyLatest = false
            };
        }
    }
} 