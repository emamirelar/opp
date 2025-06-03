namespace UNOPS.PAO.Business.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models;

public interface IAiPromptManager
{
    Task<TestPromptResponse> TestPromptAsync(TestPromptRequest request);
    Task<PaginationResponse<AiPromptModel>> GetPromptsAsync(AiPromptFilterRequest request);
    Task<AiPromptModel?> GetPromptByIdAsync(int id);
    Task<AiPromptModel> CreatePromptAsync(AiPromptModel model);
    Task<AiPromptModel?> UpdatePromptAsync(int id, AiPromptModel model);
    Task<bool> DeletePromptAsync(int id);
    Task<IEnumerable<AiPromptModel>> GetPromptsByTypeAsync(string type);
    Task<IEnumerable<string>> GetPromptTypesAsync();
    Task<IEnumerable<string>> GetModelsAsync();
    Task<IEnumerable<string>> GetProjectsAsync();
    Task<IEnumerable<string>> GetLocationsAsync();
} 