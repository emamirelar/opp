using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.DataAccess.Context;
using AutoMapper;
using UNOPS.PAO.Business.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Vision.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Google.Cloud.TextToSpeech.V1;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GeminiSessionService
{
    private readonly UNOPSAppDbContext _context;

    public GeminiSessionService(UNOPSAppDbContext context) 
    {
        _context = context;
    }

    public async Task<AiChatSession> UpdateCurrentSessionIfInactive(int userId, Guid sessionId) {
        EndDateActiveSessions(userId, sessionId);

        var currentSession = _context.AiChatSession
                                .FirstOrDefault(x => x.Id == sessionId && x.UserId == userId);

        if (currentSession != null) {
            currentSession.EndTime = null;
            currentSession.Status = "Active";
            _context.SaveChangesAsync();
        }

        return currentSession;
    }

    private void EndDateActiveSessions(int userId, Guid sessionId) {
        var activeSessions = _context.AiChatSession
                                .Where(x => x.UserId == userId && (sessionId != Guid.Empty && x.Id != sessionId) && x.EndTime == null)
                                .ToList();

        foreach (var session in activeSessions) {
            // End the active session by setting EndTime
            session.EndTime = DateTime.UtcNow.ToUniversalTime();
            session.Status = "Inactive";
        }

        _context.SaveChanges(); // Save changes before creating a new session
    }


// Create a new session for the user
    public Guid CreateNewSession(int userId) {
        EndDateActiveSessions(userId, Guid.Empty);
        // Create a new session
        var newSession = new AiChatSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTime = DateTime.UtcNow,
            Status = "Active",
            EndTime = null, // This is a new active session
            Chats = new List<AiChatHistory>() 
        };

        _context.AiChatSession.Add(newSession);
        _context.SaveChanges(); // Commit to the database

        return newSession.Id;
    }

    // End the session by session ID
    public bool EndSession(Guid sessionId) {
        var activeSession = _context.AiChatSession
                                .FirstOrDefault(x => x.Id == sessionId && x.EndTime == null);

        var success = false;
        if (activeSession != null) {
            // End the active session by setting EndTime
            activeSession.EndTime = DateTime.UtcNow;
            activeSession.Status = "Inactive";
            _context.SaveChanges(); // Save changes before creating a new session
            success = true;
        }

        return success;
    }

    // Get user sessions by user ID
    public IEnumerable<AiChatSession> GetUserSessions(int userId) {
        return (IEnumerable<AiChatSession>)_context.AiChatSession
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.EndTime ?? DateTime.MaxValue); // Place active sessions last
    }

    // Get session data by session ID and user ID
    public IEnumerable<AiChatSession> GetSessionDataWithChats(Guid sessionId, int userId) {
        return (IEnumerable<AiChatSession>)_context.AiChatSession
                .Include(x => x.Chats)
                .Where(x => x.Id == sessionId && x.UserId == userId)
                .Select(session => new AiChatSession
                {
                    Id = session.Id,
                    UserId = session.UserId,
                    // Other AiChatSession properties...

                    Chats = session.Chats
                        .Where(chat => chat.Type == "entity_intent_detection")
                        .ToList()
                });
    }

    public async Task<IEnumerable<AiChatSession>> GetSessionData(Guid sessionId, int userId) {
        return (IEnumerable<AiChatSession>)_context.AiChatSession
                .Where(x => x.Id == sessionId && x.UserId == userId);
    }

    // Get chat history by session ID
    public async Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId, string type) {
        return await _context.AiChatHistory
                    .Where(x => x.SessionId == sessionId && x.Type == type)
                    .OrderBy(x => x.TimeStamp)
                    .ToListAsync();
    }

    // Update chat history table
    public bool UpdateChatHistoryTable(Guid sessionId, string sender, string message, string rawMessage, string entity, string intent, string promptType, string fileUrl, string fileType) {
        var newChatHistory = new AiChatHistory{
            SessionId = sessionId,
            Sender = sender,
            Message = message ?? "",
            RawMessage = rawMessage ?? "",
            EntityType = entity,
            RequestType = intent,
            Type = promptType,
            MediaUrl = fileUrl,
            MediaType = fileType,
            TimeStamp = DateTime.Now.ToUniversalTime()
        };

        _context.AiChatHistory
                .Add(newChatHistory);

        _context.SaveChanges();

        return true;
    }

    public async Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req)
    {
        var session = await _context.AiChatSession
                                .FirstOrDefaultAsync(x => x.Id == req.SessionId);

        if (session != null)
        {
            session.TextToSpeech = req.TextToSpeech ?? false;
            await _context.SaveChangesAsync();
            return true; // Save changes to DB
        }

        return false; // No session found
    }
}