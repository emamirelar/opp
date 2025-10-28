namespace UNOPS.PAO.Business.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;

public interface IAiPromptManager
{
    
    Task<TestPromptResponse> TestPromptAsync(ClaimsPrincipal user, TestPromptRequest request);
    
    Task<PaginationResponse<AiPromptModel>> GetPromptsAsync(ClaimsPrincipal user, AiPromptFilterRequest request);
    
    Task<AiPromptModel?> GetPromptByIdAsync(ClaimsPrincipal user, int id);
    
    Task<AiPromptModel> CreatePromptAsync(ClaimsPrincipal user, AiPromptModel model);
    
    Task<AiPromptModel?> UpdatePromptAsync(ClaimsPrincipal user, int id, AiPromptModel model);
    
    Task<bool> DeletePromptAsync(ClaimsPrincipal user, int id);
    
    Task<IEnumerable<AiPromptModel>> GetPromptsByTypeAsync(ClaimsPrincipal user, string type);
    
    Task<IEnumerable<string>> GetPromptTypesAsync(ClaimsPrincipal user);
    
    Task<IEnumerable<string>> GetModelsAsync(ClaimsPrincipal user);
    
    Task<IEnumerable<string>> GetProjectsAsync(ClaimsPrincipal user);
    
    Task<IEnumerable<string>> GetLocationsAsync(ClaimsPrincipal user);
    
    Task<GeminiModelUpgradeResult> UpgradeToLatestGeminiModelAsync(ClaimsPrincipal user);
    
    Task<string> ExportAiPromptsAsSqlAsync(ClaimsPrincipal user);
} 