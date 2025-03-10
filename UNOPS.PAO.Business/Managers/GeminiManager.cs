namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class GeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
    private readonly DataRepository<AiPrompt> _promptRepository;

    public GeminiManager(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
        _promptRepository = new DataRepository<AiPrompt>(context);
    }

    public IEnumerable<AiPromptModel> GetPromptData(string type)
    {
        return _promptRepository
            .GetAll()
            .Where(x => x.Type == type)
            .Select(x => _mapper.Map<AiPrompt, AiPromptModel>(x));
    }

    public async Task<string> fetchResultFromGemini(string promptTemplate, string relatedJsonData)
    {
        // Implement the logic to fetch result from Gemini
        throw new NotImplementedException();
    }

    Task<IEnumerable<AiScreenMapping>> IGeminiManager.GetScreenMappingsByType(string type)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetDataBasedOnScreenMapping(string type, int recordId, AiScreenMapping[] mapping)
    {
        throw new NotImplementedException();
    }

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessRequest req)
    {
        var entity = _mapper.Map<AiPrompt>(req);
        return entity;
    }
}