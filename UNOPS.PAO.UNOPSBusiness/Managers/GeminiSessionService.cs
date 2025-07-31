using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Text;
using System.Text.RegularExpressions;
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
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiSessionService(UNOPSAppDbContext context, HttpClient httpClient, IConfiguration configuration) 
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // Get user sessions by user ID
    public IEnumerable<AiChatSession> GetUserSessions(int userId) {
        return (IEnumerable<AiChatSession>)_context.AiChatSession
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.LastUpdated); // Place active sessions last
    }

    // Get session data by session ID and user ID with chat messages from AI service
    public async Task<SessionWithChats> GetSessionDataWithChats(string sessionId, int userId) {
        try 
        {
            // First, get the session details from the database
            var session = await _context.AiChatSession
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session == null)
            {
                return null; // Session not found
            }

            var result = new SessionWithChats 
            { 
                Session = session,
                ChatMessages = new List<ChatMessage>()
            };

            // Then, get the chat messages from the AI service
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            var appName = _configuration.GetValue<string>("AgenticAi:AppName");
            
            if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(appName))
            {
                // Return session without chat messages if AI service is not configured
                return result;
            }
            
            var apiUrl = $"{serviceUrl}/apps/{appName}/users/{userId}/sessions/{sessionId}";
            
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                // Return session without chat messages if AI service call fails
                return result;
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(jsonContent);
            
            var events = jsonObject["events"] as JArray;
            
            if (events != null)
            {
                var userMessages = new List<ChatMessage>();
                var modelMessagesByInvocation = new Dictionary<string, ChatMessage>();

                foreach (var eventItem in events)
                {
                    var content = eventItem["content"];
                    if (content == null) continue; // Skip if content is missing
                    var role = content?["role"]?.ToString();
                    var invocationId = eventItem["invocationId"]?.ToString();
                    // Parse timestamp as double (seconds since epoch, possibly with fraction)
                    var timestampValue = eventItem["timestamp"]?.ToObject<double?>();
                    DateTime timestamp = DateTime.MinValue;
                    if (timestampValue.HasValue)
                    {
                        var seconds = (long)Math.Floor(timestampValue.Value);
                        var fraction = timestampValue.Value - seconds;
                        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                        timestamp = dateTimeOffset.AddSeconds(fraction);
                    }
                    var parts = content["parts"] as JArray;
                    if (parts == null) continue;

                    // Collect all text and inline data for this message
                    var messageText = new StringBuilder();
                    var inlineDataList = new List<InlineData>();

                    foreach (var part in parts)
                    {
                        var text = part["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            if (messageText.Length > 0)
                                messageText.Append(" ");
                            messageText.Append(text);
                        }

                        var inlineData = part["inlineData"];
                        if (inlineData != null)
                        {
                            // Debug: Log the raw JSON structure
                            Console.WriteLine($"Raw inlineData JSON: {inlineData}");
                            
                            var dataToken = inlineData["data"];
                            var mimeType = inlineData["mimeType"]?.ToString();
                            
                            if (dataToken != null && !string.IsNullOrEmpty(mimeType))
                            {
                                // Get the raw string value, handling potential newlines/formatting
                                var data = dataToken.Type == Newtonsoft.Json.Linq.JTokenType.String 
                                    ? dataToken.Value<string>() 
                                    : dataToken.ToString();
                                
                                // Clean the base64 data: remove newlines, spaces, and other formatting
                                data = System.Text.RegularExpressions.Regex.Replace(data, @"[\r\n\s]", "");
                                
                                Console.WriteLine($"Original data length: {dataToken.ToString().Length}");
                                Console.WriteLine($"Cleaned data length: {data.Length}");
                                Console.WriteLine($"First 100 chars: {data.Substring(0, Math.Min(100, data.Length))}");
                                
                                if (!string.IsNullOrEmpty(data))
                                {
                                    inlineDataList.Add(new InlineData
                                    {
                                        Data = data,
                                        MimeType = mimeType
                                    });
                                }
                            }
                        }
                    }

                    // Only create a message if we have text or inline data
                    if (messageText.Length > 0 || inlineDataList.Any())
                    {
                        if (role == "user")
                        {
                            userMessages.Add(new ChatMessage
                            {
                                Role = role,
                                Text = messageText.ToString(),
                                Timestamp = timestamp,
                                InlineData = inlineDataList
                            });
                        }
                        else if (role == "model" && !string.IsNullOrEmpty(invocationId))
                        {
                            // Only keep the latest model message per invocation_id
                            if (!modelMessagesByInvocation.TryGetValue(invocationId, out var existing) || existing.Timestamp < timestamp)
                            {
                                modelMessagesByInvocation[invocationId] = new ChatMessage
                                {
                                    Role = role,
                                    Text = messageText.ToString(),
                                    Timestamp = timestamp,
                                    InlineData = inlineDataList
                                };
                            }
                        }
                    }
                }

                // Merge user and model messages, sort by timestamp
                var allMessages = userMessages
                    .Concat(modelMessagesByInvocation.Values)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                result.ChatMessages = allMessages;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // Log the exception and return session without chat messages if possible
            try 
            {
                var session = await _context.AiChatSession
                    .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);
                
                return session != null ? new SessionWithChats 
                { 
                    Session = session,
                    ChatMessages = new List<ChatMessage>()
                } : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public async Task<IEnumerable<AiChatSession>> GetSessionData(string sessionId, int userId) {
        return (IEnumerable<AiChatSession>)_context.AiChatSession
                .Where(x => x.Id == sessionId && x.UserId == userId);
    }

    public async Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req)
    {
        var session = await _context.AiChatSession
                                .FirstOrDefaultAsync(x => x.Id == req.SessionId);

        if (session != null)
        {
            // Note: TextToSpeech property not available in AiChatSession entity
            // This functionality may need to be implemented separately or added to the entity
            await _context.SaveChangesAsync();
            return true; // Save changes to DB
        }

        return false; // No session found
    }

    public async Task<bool> UpdateSessionStar(string sessionId, bool starred)
    {
        var session = await _context.AiChatSession
                                .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session != null)
        {
            session.Starred = starred;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateSessionArchive(string sessionId, bool archived)
    {
        var session = await _context.AiChatSession
                                .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session != null)
        {
            session.Archived = archived;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateSessionTitle(string sessionId, string title)
    {
        var session = await _context.AiChatSession
                                .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session != null)
        {
            session.Title = title;
            session.AiGenerateTitle = false;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}