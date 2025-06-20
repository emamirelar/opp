using UNOPS.PAO.RBAC.Attributes;

namespace UNOPS.PAO.Business.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using UNOPS.PAO.Models;

public interface IAiPromptManager
{
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<TestPromptResponse> TestPromptAsync(ClaimsPrincipal user, TestPromptRequest request);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<PaginationResponse<AiPromptModel>> GetPromptsAsync(ClaimsPrincipal user, AiPromptFilterRequest request);
    
    [RBAC("read", Entity = "AiPromptManagement", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task<AiPromptModel?> GetPromptByIdAsync(ClaimsPrincipal user, int id);
    
    [RBAC("create", Entity = "AiPromptManagement")]
    Task<AiPromptModel> CreatePromptAsync(ClaimsPrincipal user, AiPromptModel model);
    
    [RBAC("update", Entity = "AiPromptManagement", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task<AiPromptModel?> UpdatePromptAsync(ClaimsPrincipal user, int id, AiPromptModel model);
    
    [RBAC("delete", Entity = "AiPromptManagement", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task<bool> DeletePromptAsync(ClaimsPrincipal user, int id);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<IEnumerable<AiPromptModel>> GetPromptsByTypeAsync(ClaimsPrincipal user, string type);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<IEnumerable<string>> GetPromptTypesAsync(ClaimsPrincipal user);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<IEnumerable<string>> GetModelsAsync(ClaimsPrincipal user);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<IEnumerable<string>> GetProjectsAsync(ClaimsPrincipal user);
    
    [RBAC("read", Entity = "AiPromptManagement")]
    Task<IEnumerable<string>> GetLocationsAsync(ClaimsPrincipal user);
} 