namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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

    public async Task<string> fetchResultFromGemini(AiPromptModel promptData, string relatedJsonData)
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

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessDataRequest req)
    {
        var entity = _mapper.Map<AiPrompt>(req);
        return entity;
    }

    public IEnumerable<AiChatSession> GetSessionData(Guid sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<AiChatSession> GetUserSessions(int userId) {
        throw new NotImplementedException();
    }

    public Guid CreateNewSession(int userId) {
        throw new NotImplementedException();
    }

    public bool EndSession(Guid sessionId) {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId, string type)
    {
        throw new NotImplementedException();
    }

    public Task<string> EntityDetectionThroughGemini(IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request) {
        throw new NotImplementedException();
    }

    public Task<string> FetchDetailedResponseFromGemini(IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string promptType) {
        throw new NotImplementedException();
    }

    public JObject GetDetailsFromGeminiResponse(string modelResponse) {
        throw new NotImplementedException();
    }

    public bool UpdateChatHistoryTable(Guid sessionId, string originalMessage, string userMessage, string modelResponse, string entity, string intent, string promptType) {
        throw new NotImplementedException();
    }

    public void UpdateCurrentSessionIfInactive(int userId, Guid sessionId) {
        throw new NotImplementedException();
    }
}