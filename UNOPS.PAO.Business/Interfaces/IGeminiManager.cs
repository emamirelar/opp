using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGeminiManager
{
    AiPrompt MapModelToEntity(GeminiProcessRequest req);
    IEnumerable<AiPromptModel> GetPromptData(string type);
    Task<string> fetchResultFromGemini(string promptTemplate, string relatedJsonData);
    Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type);
    Task<string> GetDataBasedOnScreenMapping(int recordId, AiScreenMapping[] mapping);
}