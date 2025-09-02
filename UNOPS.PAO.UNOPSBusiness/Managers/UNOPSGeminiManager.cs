using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.DataAccess.Context;
using AutoMapper;
using UNOPS.PAO.Business.Repositories.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Dynamic;
using Humanizer;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Vision.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Google.Cloud.TextToSpeech.V1;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Globalization;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Reflection.Metadata.Ecma335;
using Google.Cloud.AIPlatform.V1;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using Z.EntityFramework.Plus;
using System.Text.Json;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory; // Add this for IMemoryCache
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCredential _credentials;
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly UNOPSAppDbContext _context;
    private readonly GoogleTextToSpeechService _ttsService;
    private readonly TextExtractionService _textExtractionService;
    private readonly GoogleCloudStorageService _gcsService;

    private readonly AiContextualService _aiService;
    private readonly ILogger<UNOPSGeminiManager> _logger;
    private readonly CloudRunHelper _cloudRunHelper;
    private readonly IUserManagementManager _userManagementManager;
    private readonly IUserInfoService _userInfoService;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IUserProfileCacheService _userProfileCacheService;
    private readonly IScreenContextCacheService _screenContextCacheService;
    private readonly IGeoTimeCacheService _geoTimeCacheService;

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, ILogger<UNOPSGeminiManager> logger, IUserManagementManager userManagementManager, IUserInfoService userInfoService, UserManager<PAOIdentityUser> userManager, IUserPreferenceService userPreferenceService, IUserProfileCacheService userProfileCacheService, IScreenContextCacheService screenContextCacheService, IGeoTimeCacheService geoTimeCacheService)
    {
        _mapper = mapper;
        _context = context;
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _logger = logger;
        _userManagementManager = userManagementManager;
        _userInfoService = userInfoService;
        _userManager = userManager;
        _userPreferenceService = userPreferenceService;
        _userProfileCacheService = userProfileCacheService;
        _screenContextCacheService = screenContextCacheService;
        _geoTimeCacheService = geoTimeCacheService;
        
        // Initialize CloudRunHelper internally
        var cloudRunHelperLogger = new LoggerFactory().CreateLogger<CloudRunHelper>();
        _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, GetCredentials());
        
        _credentials = GetCredentials()
                        .CreateScoped("https://www.googleapis.com/auth/spreadsheets.readonly");
        _textExtractionService = new TextExtractionService();
        _gcsService = new GoogleCloudStorageService(configuration);

        _ttsService = new GoogleTextToSpeechService();
        _aiService = new AiContextualService(configuration, _context, _credentials);
    }

    // Map AiPromptModel to AiPrompt entity
    private AiPrompt MapModelToEntity(AiPromptModel model)
    {
        var entity = _mapper.Map(model, new AiPrompt());
        return entity;
    }

    // Get prompt data by type
    public async Task<IEnumerable<AiPrompt>> GetPromptData(string type)
    {
        return await _aiService.GetPromptData(type);
    }

    // Chat with Gemini
    private async Task<dynamic> ChatWithGemini(AiChatSession session, GeminiAssistantRequest req, string promptType, IEnumerable<dynamic> formattedChatHistory, string fileUrl, string fileType)
    {
        throw new NotImplementedException();
    }

    // Updated FetchResultFromGemini to use CallGeminiApi
    public async Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData)
    {
        return await _aiService.FetchResultFromGemini((AiPrompt)promptData, relatedJsonData);
    }

    // Updated callGemini to use CallGeminiApi
    public async Task<string> callGemini(string prompt, AiPrompt promptData)
    {
        var promptList = new
        {
            role = "user",
            parts = new[] { new { text = prompt } }
        };
        return await _aiService.CallGeminiApi(promptList, promptData);
    }

    // Map GeminiProcessDataRequest to AiPrompt entity
    private AiPrompt MapModelToEntity(GeminiProcessDataRequest model)
    {
        var entity = _mapper.Map<AiPrompt>(model);
        return entity;
    }

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessDataRequest req)
    {
        return MapModelToEntity(req);
    }

    // Get Google credentials from configuration
    private GoogleCredential GetCredentials()
    {
        var credentialParams = _configuration.GetSection("AISettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("AISettings configuration is missing.");
    
        var secretName = _configuration.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        
        var basicProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
        return GoogleCredential.FromJson(secretValue);
    }

    // Get user profile details - first check cache, then fallback to database
    private async Task<object?> GetUserProfileDetailsAsync(ClaimsPrincipal user)
    {
        try
        {
            // Try multiple ways to get the current user's email from claims
            var currentEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                              user.FindFirst("email")?.Value ?? 
                              user.Identity?.Name;
            
            if (string.IsNullOrEmpty(currentEmail))
            {
                return null;
            }

            // Extract email if it contains colon (for dev mode)
            currentEmail = currentEmail.Contains(':') ? currentEmail.Split(':').Last() : currentEmail;

            // Get user ID from claims for cache lookup
            var currentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Try to get from cache first using user ID, then fallback to email
            var cacheKey = !string.IsNullOrEmpty(currentUserId) ? currentUserId : currentEmail;
            var cachedProfile = await _userProfileCacheService.GetCachedUserProfileAsync(cacheKey);
            
            if (cachedProfile != null)
            {
                _logger.LogDebug("Using cached user profile for user: {UserId}/{Email}", currentUserId, currentEmail);
                return cachedProfile;
            }

            _logger.LogDebug("User profile not in cache, fetching from database for user: {UserId}/{Email}", currentUserId, currentEmail);

            // Cache miss - fetch from database (same logic as UserProfileController)
            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // If no roles in claims, try to get them from database using email
            if (!userRoles.Any())
            {
                try
                {
                    var aspNetUser = await _userManager.FindByEmailAsync(currentEmail);
                    if (aspNetUser != null)
                    {
                        userRoles = (await _userManager.GetRolesAsync(aspNetUser)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get user roles from database for email: {Email}", currentEmail);
                    userRoles = new List<string>();
                }
            }

            // Check if user is PARTNER_GLOB_ADMIN
            var isPartnerGlobalAdmin = userRoles.Contains("PARTNER_GLOB_ADMIN");

            // Get user info with organization settings
            var userInfoWithOrgSettings = await _userInfoService.GetUserInfoWithOrgSettingsAsync(currentEmail);
            
            if (userInfoWithOrgSettings == null)
            {
                return null;
            }

            // Get user preferences
            UserPreference? userPreferences = null;
            try
            {
                var aspNetUser = await _userManager.FindByEmailAsync(currentEmail);
                if (aspNetUser != null)
                {
                    userPreferences = await _userPreferenceService.GetUserPreferencesAsync(aspNetUser.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user preferences for email: {Email}", currentEmail);
                userPreferences = null;
            }

            // Create response object with additional properties including user preferences
            var response = new
            {
                userInfoWithOrgSettings,
                Roles = userRoles,
                IsPartnerGlobalAdmin = isPartnerGlobalAdmin,
                // PARTNER_GLOB_ADMIN always has self-management enabled regardless of org setting
                CanManageOffice = isPartnerGlobalAdmin || 
                                 (userInfoWithOrgSettings.GetType().GetProperty("IsSelfManagementEnabled")?.GetValue(userInfoWithOrgSettings) as bool? ?? false),
                UserPreferences = userPreferences
            };

            // Cache the response for future use
            await _userProfileCacheService.SetCachedUserProfileAsync(cacheKey, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile details");
            return null;
        }
    }

    // Enhance state with user profile information, screen context, and geo-time data
    private async Task<string> EnhanceStateWithUserProfile(string? originalState, object? userProfileDetails)
    {
        try
        {
            var stateObject = new Dictionary<string, object>();
            
            // Parse existing state if it exists
            if (!string.IsNullOrEmpty(originalState))
            {
                try
                {
                    var existingState = JsonConvert.DeserializeObject<Dictionary<string, object>>(originalState);
                    if (existingState != null)
                    {
                        stateObject = existingState;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse existing state, starting with empty state");
                }
            }
            
            // Add user profile details to state
            if (userProfileDetails != null)
            {
                stateObject["user_profile"] = userProfileDetails;
            }
            
            // Add screen context if available in state
            await AddScreenContextToState(stateObject);
            
            // Add geo-time data
            await AddGeoTimeToState(stateObject);
            
            return JsonConvert.SerializeObject(stateObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing state with context data");
            return originalState ?? "{}";
        }
    }

    private async Task AddScreenContextToState(Dictionary<string, object> stateObject)
    {
        try
        {
            // Extract screen URL and user focus context from existing state
            var screenUrl = stateObject.TryGetValue("screen_url", out var screenUrlObj) ? screenUrlObj?.ToString() : "";
            var userFocusContext = stateObject.TryGetValue("user_focus_context", out var userFocusObj) ? userFocusObj?.ToString() : "";
            
            if (!string.IsNullOrEmpty(screenUrl) || !string.IsNullOrEmpty(userFocusContext))
            {
                // Get current user ID for context
                var userId = stateObject.TryGetValue("user_id", out var userIdObj) ? userIdObj?.ToString() : "";
                
                var screenContext = await _screenContextCacheService.GetScreenContextAsync(screenUrl, userFocusContext, userId);
                if (screenContext != null)
                {
                    stateObject["screen_context"] = screenContext;
                    _logger.LogDebug("Added screen context to state for URL: {ScreenUrl}", screenUrl);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add screen context to state");
        }
    }

    private async Task AddGeoTimeToState(Dictionary<string, object> stateObject)
    {
        try
        {
            // Extract user IP if available from state
            var userIp = stateObject.TryGetValue("user_ip", out var userIpObj) ? userIpObj?.ToString() : null;
            
            var geoTimeData = await _geoTimeCacheService.GetGeoTimeDataAsync(userIp);
            if (geoTimeData != null)
            {
                stateObject["user_geo_stats"] = geoTimeData;
                _logger.LogDebug("Added geo-time data to state");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add geo-time data to state");
        }
    }

    public async Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req)
    {
        string relatedMessage = "";

        AiPrompt promptModel = MapModelToEntity(req);

        // Call the GetPromptData method and get the first prompt
        AiPrompt promptData = (await GetPromptData(promptModel.Type)).FirstOrDefault();

        if (promptData == null)
        {
            return "";
        }

        // Check if promptFunction is available (new approach)
        if (!string.IsNullOrEmpty(promptData.PromptFunction))
        {
            try
            {
                // Determine the correct manager based on entity type
                string managerTypeName = $"UNOPS.PAO.UNOPSBusiness.Managers.UNOPS{promptData.Name.TrimEnd('s')}Manager";
                System.Type managerType = System.Type.GetType(managerTypeName);
                
                if (managerType == null)
                {
                    throw new InvalidOperationException($"Manager type not found for entity: {promptData.Name}");
                }
                
                // Get constructor parameters that the manager needs
                var constructors = managerType.GetConstructors();
                var constructor = constructors.FirstOrDefault();
                
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No suitable constructor found for {managerType.Name}");
                }
                
                // Prepare constructor arguments (common ones that most managers need)
                var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                var args = new List<object>();
                
                foreach (var paramType in parameterTypes)
                {
                    if (paramType == typeof(IMapper))
                        args.Add(_mapper);
                    else if (paramType == typeof(UNOPSAppDbContext))
                        args.Add(_context);
                    else if (paramType == typeof(IConfiguration))
                        args.Add(_configuration);
                    else
                        args.Add(null); // Pass null for other dependencies we don't have
                }
                
                // Create instance of the manager
                var managerInstance = Activator.CreateInstance(managerType, args.ToArray());
                
                // Check if it's a BaseUNOPSManager that has CallFunctionByNameAsync
                var callFunctionMethod = managerType.GetMethod("CallFunctionByNameAsync");
                if (callFunctionMethod != null)
                {
                    // Use the BaseUNOPSManager's CallFunctionByNameAsync method which handles parameter matching
                    var task = (Task<object>)callFunctionMethod.Invoke(managerInstance, new object[] { promptData.PromptFunction, req.Id, null });
                    var entityData = await task;
                    
                    if (entityData != null)
                    {
                        // Serialize the entity data to JSON for AI processing with enum string conversion
                        var settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
                        };
                        relatedMessage = JsonConvert.SerializeObject(entityData, settings);
                    }
                    else
                    {
                        return "Entity not found or function returned null.";
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Manager {managerType.Name} does not inherit from BaseUNOPSManager or does not have CallFunctionByNameAsync method");
                }
            }
            catch (Exception ex)
            {
                // Log error and fallback to empty response
                _logger.LogError(ex, "Error calling function {PromptFunction}: {ErrorMessage}", promptData.PromptFunction, ex.Message);
                return $"Error retrieving data: {ex.Message}";
            }
        }

        // Fetch result from Gemini
        return await FetchResultFromGemini(promptData, relatedMessage);
    }

    public async Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req)
    {
        string extractedText = await ExtractDataFromFile(req.File);
        string type = req?.Type;

        if (!string.IsNullOrEmpty(type)) {
            // For partner_action type, use the enhanced AiContextualService approach
            if (type.Equals("partner_action", StringComparison.OrdinalIgnoreCase))
            {
                var promptData = (await _aiService.GetPromptData("partner_action")).FirstOrDefault();
                if (promptData == null)
                {
                    return "";
                }

                // Send to Gemini with the extracted text
                var geminiResponse = await _aiService.FetchResultFromGemini(promptData, extractedText);
                var parsedResponse = _aiService.GetDetailsFromGeminiResponse(geminiResponse);

                // Process dependents to convert text to IDs using enhanced logic
                var dependents = parsedResponse["dependents"]?.ToString();
                var processedResponse = await _aiService.GetDependentDropdownValues(dependents, parsedResponse, promptData);

                // Return the processed response as JSON string
                return Newtonsoft.Json.JsonConvert.SerializeObject(processedResponse);
            }
            else
            {
                // For other types, use the existing logic
                var promptData = (await GetPromptData(type)).FirstOrDefault();

                if (promptData == null)
                {
                    return "";
                }

                // Fetch result from Gemini
                return await FetchResultFromGemini(promptData, extractedText);
            }
        }

        return extractedText;
    }

    /// <summary>
    /// Maps prompt types to entity names for duplicate detection
    /// </summary>
    /// <param name="promptType">The prompt type (e.g., "bulk_contact_action")</param>
    /// <returns>The entity name for duplicate detection (e.g., "Contacts")</returns>
    private string GetEntityNameFromPromptType(string promptType)
    {
        if (string.IsNullOrEmpty(promptType))
            return "Contacts"; // Default fallback

        return promptType.ToLower() switch
        {
            "bulk_contact_action" or "contact_action" => "Contacts",
            "bulk_partner_action" or "partner_action" => "Partners", 
            "bulk_interaction_action" or "interaction_action" => "Interactions",
            _ => "Contacts" // Default fallback
        };
    }

    public async Task<SessionWithChats> GetSessionDataWithChats(string sessionId, int userId) 
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            var appName = _configuration.GetValue<string>("AgenticAi:AppName");
            
            if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException("AgenticAi configuration is missing or incomplete.");
            }
            
            var apiUrl = $"/session-with-chats?app_name={appName}&user_id={userId}&session_id={sessionId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var sessionWithChats = JsonConvert.DeserializeObject<SessionWithChats>(jsonContent);
                
                if (sessionWithChats?.Session != null)
                {
                    // Get the actual session data from database to get real title, starred, archived status
                    var dbSession = await _context.AiChatSession
                        .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);
                    
                    if (dbSession != null)
                    {
                        // Update session with database values
                        sessionWithChats.Session.Title = dbSession.Title ?? "New Chat";
                        sessionWithChats.Session.Starred = dbSession.Starred;
                        sessionWithChats.Session.Archived = dbSession.Archived;
                        sessionWithChats.Session.AiGenerateTitle = dbSession.AiGenerateTitle;
                        sessionWithChats.Session.LastUpdated = dbSession.LastUpdated;
                    }
                }
                
                return sessionWithChats ?? new SessionWithChats();
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch session with chats from external API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling external API for session with chats: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AiChatSession>> GetSessionData(string sessionId, int userId) 
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            var appName = _configuration.GetValue<string>("AgenticAi:AppName");
            
            if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException("AgenticAi configuration is missing or incomplete.");
            }
            
            var apiUrl = $"/session-data?app_name={appName}&user_id={userId}&session_id={sessionId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var sessionData = JsonConvert.DeserializeObject<IEnumerable<AiChatSession>>(jsonContent);
                
                return sessionData ?? new List<AiChatSession>();
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch session data from external API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling external API for session data: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AiChatSession>> GetUserSessions(int userId) 
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            var appName = _configuration.GetValue<string>("AgenticAi:AppName");
            
            if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException("AgenticAi configuration is missing or incomplete.");
            }
            
            var apiUrl = $"/user-sessions?app_name={appName}&user_id={userId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var externalSessions = JsonConvert.DeserializeObject<IEnumerable<AiChatSession>>(jsonContent);
                
                if (externalSessions == null || !externalSessions.Any())
                {
                    return new List<AiChatSession>();
                }
                
                // Get session IDs from external API response
                var sessionIds = externalSessions.Select(s => s.Id).ToList();
                
                // Query AiChatSession table to get additional details
                var dbSessions = await _context.AiChatSession
                    .Where(x => sessionIds.Contains(x.Id) && x.UserId == userId)
                    .ToListAsync();
                
                // Join external sessions with database sessions to combine data
                var joinedSessions = externalSessions.Select(extSession =>
                {
                    var dbSession = dbSessions.FirstOrDefault(db => db.Id == extSession.Id);
                    if (dbSession != null)
                    {
                        // Use database session data for fields like Title, Starred, Archived, etc.
                        // but keep external session data for chat-related fields
                        return new AiChatSession
                        {
                            Id = extSession.Id,
                            UserId = extSession.UserId,
                            Status = extSession.Status,
                            LastUpdated = dbSession.LastUpdated, // Use actual database timestamp
                            Title = dbSession.Title ?? "New Chat",
                            Starred = dbSession.Starred,
                            Archived = dbSession.Archived,
                            AiGenerateTitle = dbSession.AiGenerateTitle
                        };
                    }
                    else
                    {
                        // If no database record found, use external session data with defaults
                        return new AiChatSession
                        {
                            Id = extSession.Id,
                            UserId = extSession.UserId,
                            Status = extSession.Status,
                            LastUpdated = DateTime.UtcNow, // Use current time for new sessions
                            Title = "New Chat",
                            Starred = false,
                            Archived = false,
                            AiGenerateTitle = true
                        };
                    }
                }).ToList();
                
                return joinedSessions.OrderByDescending(s => s.LastUpdated);
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch sessions from external API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling external API for user sessions: {ex.Message}", ex);
        }
    }

    public async Task<string> ExtractDataFromFile(IFormFile file) {
        return await _textExtractionService.ExtractDataFromFile(file);
    }

    public string FindFileType(IFormFile file) 
    {
        return _textExtractionService.FindFileType(file);
    }

    // Overload for IFormFile
    public async Task<string> UploadFileToGCS(IFormFile file)
    {
        return await _gcsService.UploadFileToGCS(file);
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

    public async Task UpdateSessionTitleAndFlag(string sessionId, string title)
    {
        var session = await _context.AiChatSession.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session != null)
        {
            session.Title = title;
            session.AiGenerateTitle = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId)
    {
        try
        {
            var promptData = (await GetPromptData(req.Type)).FirstOrDefault();
            if (promptData == null)
            {
                throw new Exception($"No prompt configuration found for type: {req.Type}");
            }

            var fileData = await _aiService.ReadFileData(req.FileId);
            if (string.IsNullOrEmpty(fileData))
            {
                throw new Exception("No data found in the Google Sheet. Please ensure the sheet contains data.");
            }

            var fileDataArray = JArray.Parse(fileData);

        // Determine entity name for batch size optimization
        string entityName = GetEntityNameFromPromptType(req.Type);
        
        // For Partners: Always use batch size 5, but check total rows for async vs sync
        // For other entities: Use existing logic (batch size 25, async if > 100 rows)
        bool isPartnerEntity = entityName.Equals("Partners", StringComparison.OrdinalIgnoreCase);
        int totalRows = fileDataArray.Count - 1; // Excluding header row
        
        // Check if we should process asynchronously (changed threshold to 50)
        bool shouldProcessAsync = isPartnerEntity ? (totalRows > 50) : (fileDataArray.Count > 50);
        
        if (shouldProcessAsync)
        {
            var message = new MyPubSubMessage
            {
                MessageType = "BulkImport",
                EntityName = req.Type,
                PromptType = promptData.Type,
                BatchData = JsonConvert.SerializeObject(fileDataArray.ToObject<List<object>>()), // Convert to JSON string
                UserId = currentUserId,
                FileId = req.FileId // Include Google Sheet ID for identification
            };

            var pubSubPublisher = new PubSubPublisher(_configuration);

            await pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });

            return new
            {
                Message = "Bulk import processing started. You will be notified when complete.",
                Entity = req.Type,
                Intent = "Processing"
            };
        }
        else
        {
            var headerRow = fileDataArray[0];
            var finalResponse = new List<dynamic>();
            // Process synchronously
            var batch = new JArray
            {
                headerRow
            };
            for (int i = 1; i < fileDataArray.Count; i++)
            {
                batch.Add(fileDataArray[i]);
            }
            
            finalResponse = await _aiService.ProcessBulkImport(
                JsonConvert.SerializeObject(batch),
                promptData,
                currentUserId,
                entityName,
                false
            );

            // Check for internal duplicates within the uploaded file first
            if (finalResponse != null && finalResponse.Count > 0)
            {
                // Convert records to dynamic list for internal duplicate detection
                var recordsList = finalResponse.Select(r => (dynamic)r).ToList();
                
                // Check for duplicates within the file itself
                var internalDuplicateResult = await _aiService.DetectInternalDuplicatesAsync(entityName, recordsList, 0.8);
                
                // If internal duplicates are found, stop and ask user to fix the file
                if (internalDuplicateResult.HasInternalDuplicates)
                {
                    return new
                    {
                        message = !string.IsNullOrEmpty(req.FileId) 
                            ? $"Internal duplicates found in the uploaded file (Sheet ID: {req.FileId}). Please fix the duplicates before proceeding."
                            : "Internal duplicates found in the uploaded file. Please fix the duplicates before proceeding.",
                        entity = req.Type,
                        intent = "InternalDuplicatesFound",
                        fileId = req.FileId, // Include sheet ID for identification
                        internalDuplicates = new
                        {
                            totalGroups = internalDuplicateResult.TotalDuplicateGroups,
                            totalDuplicateRecords = internalDuplicateResult.TotalDuplicateRecords,
                            totalRecords = internalDuplicateResult.TotalRecords,
                            cleanRecords = internalDuplicateResult.CleanRecords,
                            duplicateGroups = internalDuplicateResult.DuplicateGroups.Select(group => new
                            {
                                masterRowNumber = group.MasterIndex + 2, // +2 because: +1 for 0-based index, +1 for header row
                                duplicateRowNumbers = group.DuplicateIndices.Select(idx => idx + 2).ToList(),
                                matchReasons = group.MatchReasons,
                                masterRecord = ExtractDisplayFields(group.MasterRecord, entityName),
                                duplicateRecords = group.DuplicateRecords.Select(rec => ExtractDisplayFields(rec, entityName)).ToList()
                            }).ToList()
                        }
                    };
                }
                
                // If no internal duplicates, proceed with database duplicate detection
                var recordsWithDuplicates = await _aiService.DetectDuplicatesAsync(entityName, recordsList, 0.65);
                
                // Update finalResponse with duplicate information
                finalResponse = recordsWithDuplicates.Select(r => (object)r).ToList();
            }

            return new
            {
                Message = "Processing completed successfully",
                Entity = req.Type,
                Intent = "Success",
                Records = JsonConvert.SerializeObject(finalResponse)
            };
        }
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            _logger.LogError(ex, "Error in ExtractDataAfterAnalysis for type: {Type}, fileId: {FileId}. Error: {ErrorMessage}", 
                req.Type, req.FileId, ex.Message);
            
            // Return a structured error response
            return new
            {
                Message = $"Error processing file: {ex.Message}",
                Entity = req.Type,
                Intent = "Error",
                Error = ex.Message
            };
        }
    }

    public async Task<dynamic> GenerateEmbeddings(string entityName)
    {
        var tableNames = _context.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name)
            .Where(name => name != "EntityEmbeddings" && name != "AiChatSession")
            .ToArray();

        if (entityName != null)
        {
            tableNames = new[] { entityName };
        }

        var result = new List<MyPubSubMessage>();
        var pubSubPublisher = new PubSubPublisher(_configuration);

        foreach (var tableName in tableNames)
        {
            var dbSetProperty = _context.GetType()
                .GetProperty(tableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (dbSetProperty == null)
            {
                _logger.LogWarning("DbSet for table '{TableName}' not found.", tableName);
                continue;
            }

            var dbSet = dbSetProperty.GetValue(_context) as IQueryable<object>;
            if (dbSet == null)
            {
                _logger.LogWarning("Unable to retrieve DbSet for table '{TableName}'.", tableName);
                continue;
            }

            // Dynamically include all navigation properties
            var navigationProperties = dbSetProperty.PropertyType
                .GenericTypeArguments[0]
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(IEnumerable<object>).IsAssignableFrom(p.PropertyType) || !p.PropertyType.IsValueType && p.PropertyType != typeof(string))
                .Select(p => p.Name);

            foreach (var navigationProperty in navigationProperties)
            {
                dbSet = dbSet.Include(navigationProperty);
            }

            var records = await dbSet.ToListAsync();
            foreach (var record in records)
            {
                var idProperties = record.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                    .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var entityId = idProperties
                    .Select(p => (int)p.GetValue(record))
                    .FirstOrDefault(value => value != 0); // Take the first non-zero Id

                if (entityId == 0)
                {
                    _logger.LogWarning("No valid Id found for record in table '{TableName}'.", tableName);
                    continue;
                }

                // Check if embedding already exists
                var exists = await _context.EntityEmbeddings
                    .AnyAsync(e => e.EntityName == tableName && e.EntityId == entityId);

                if (exists)
                {
                    _logger.LogInformation("Embedding already exists for Entity '{TableName}' with Id '{EntityId}'. Skipping...", tableName, entityId);
                    continue;
                }

                var content = JsonConvert.SerializeObject(record, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
                });

                result.Add(new MyPubSubMessage
                {
                    EntityName = tableName,
                    EntityId = entityId,
                    Content = content
                });

                if (result.Count == 30)
                {
                    // Publish the result array to Pub/Sub
                    await pubSubPublisher.PublishMessageAsync(result);
                    result.Clear(); // Clear the list after publishing
                }
            }

            // Publish any remaining messages
            if (result.Count > 0)
            {
                await pubSubPublisher.PublishMessageAsync(result);
            }
        }

        return null;
    }

    public async Task<string> BulkInsertRecordsAsync(BulkUploadRequest request)
    {
        var type = request.Type;
        var camelCaseType = char.ToUpper(type[0]) + type.Substring(1).ToLower();

        var assembly = typeof(UNOPSContact).Assembly;
        var modelType = assembly.GetType($"UNOPS.PAO.UNOPSDomain.Entities.UNOPS{camelCaseType}", throwOnError: false, ignoreCase: true);

        if (modelType == null)
            throw new InvalidOperationException($"Unsupported type: {type}");

        var recordsArray = request.Records.Select(record =>
        {
            if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                return JObject.FromObject(dictionary);
            }
            throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
        }).ToList();

        var convertedRecords = recordsArray.Select(r => r.ToObject(modelType)).Cast<object>().ToList();

        var tableName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.Type).Pluralize();
        var dbSetProperty = _context.GetType().GetProperty(tableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (dbSetProperty == null)
            throw new InvalidOperationException($"Table '{tableName}' not found in the context.");

        var dbSet = dbSetProperty.GetValue(_context) as dynamic;
        if (dbSet == null)
            throw new InvalidOperationException($"Unable to retrieve DbSet for table '{tableName}'.");

        var recordsToAdd = new List<object>();
        var recordsToUpdate = new List<object>();

        // Separate records into updates vs. inserts based on ID
        foreach (var record in convertedRecords)
        {
            var idProperty = record.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(record);
                
                // Fix: Set ID to null if it's 0 to prevent primary key constraint violations
                if (idValue != null && idValue is int id && id == 0)
                {
                    _logger.LogInformation("Setting ID from 0 to null for record to prevent primary key constraint violation");
                    idProperty.SetValue(record, null);
                    idValue = null;
                }
                
                if (idValue != null && idValue is int validId && validId > 0)
                {
                    // This is an existing record, so it should be updated
                    recordsToUpdate.Add(record);
                }
                else
                {
                    // No valid ID, so it's a new record
                    recordsToAdd.Add(record);
                }
            }
            else
            {
                // No ID property, so it's a new record
                recordsToAdd.Add(record);
            }
        }
        
        _logger.LogInformation("Processing bulk insert for {EntityType}. Total records: {TotalCount}, To insert: {InsertCount}, To update: {UpdateCount}", 
            tableName, convertedRecords.Count, recordsToAdd.Count, recordsToUpdate.Count);

        // Process updates
        foreach (var record in recordsToUpdate)
        {
            var idProperty = record.GetType()
                           .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                           
            var id = (int)idProperty.GetValue(record);
            
            // Find the entity by id
            var findMethod = dbSet.GetType().GetMethod("Find", new[] { typeof(object[]) });
            var existingEntity = findMethod?.Invoke(dbSet, new object[] { new object[] { id } });
            
            if (existingEntity != null)
            {
                // Update the entity properties
                foreach (var prop in record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.Name != "Id" && prop.CanWrite && !(prop.PropertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(prop.PropertyType.GetGenericTypeDefinition())))
                    {
                        try
                        {
                            var value = prop.GetValue(record);
                            prop.SetValue(existingEntity, value);
                        }
                        catch 
                        {
                            // Skip properties that cannot be set
                        }
                    }
                }
                
                var entryMethod = _context.GetType().GetMethod("Entry", new[] { typeof(object) });
                var entry = entryMethod?.Invoke(_context, new object[] { existingEntity });
                
                if (entry != null)
                {
                    var stateProperty = entry.GetType().GetProperty("State");
                    // Set to EntityState.Modified
                    stateProperty?.SetValue(entry, 2); // 2 is EntityState.Modified
                }
            }
        }

        // Add new records if any
        if (recordsToAdd.Count > 0)
        {
            var typedArray = Array.CreateInstance(modelType, recordsToAdd.Count);
            for (int i = 0; i < recordsToAdd.Count; i++)
            {
                var idProperty = recordsToAdd[i].GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                idProperty.SetValue(recordsToAdd[i], null);
                typedArray.SetValue(recordsToAdd[i], i);
            }

            // Add all at once using AddRange if available
            var addRangeMethod = ((IEnumerable<MethodInfo>)dbSet.GetType().GetMethods())
                                .FirstOrDefault(m => m.Name == "AddRange" && m.GetParameters().Length == 1);

            addRangeMethod?.Invoke(dbSet, new[] { typedArray });
        }

        var successList = new List<object>();
        var errorMessages = new List<string>();
        var isSuccess = true;

        try
        {
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Bulk insert completed successfully. Inserted: {InsertedCount}, Updated: {UpdatedCount}", 
                recordsToAdd.Count, recordsToUpdate.Count);

            // Collect all updated and added records for the response
            var processedRecords = new List<object>();
            processedRecords.AddRange(recordsToAdd);
            processedRecords.AddRange(recordsToUpdate);

            foreach (var record in processedRecords)
            {
                try
                {
                    var idValue = record.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int))
                                .Select(p => (int?)p.GetValue(record))
                                .FirstOrDefault(v => v.HasValue && v.Value != 0);

                    if (idValue != null)
                    {
                        successList.Add(new { Id = idValue, Entity = record });
                    }
                    else
                    {
                        successList.Add(new { Id = "Unknown", Entity = record });
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Success record parsed but ID fetch failed: {ex.Message}");
                }
            }
        }
        catch (DbUpdateException dbEx)
        {
            isSuccess = false;
            foreach (var entry in dbEx.Entries)
            {
                var entityJson = JsonConvert.SerializeObject(entry.Entity, new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
                });
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                errorMessages.Add($"Error saving entity {entry.Entity.GetType().Name}: {entityJson} - {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessages.Add($"Unexpected error during SaveChangesAsync: {ex.Message}");
            
            // Add inner exception details if available
            if (ex.InnerException != null)
            {
                errorMessages.Add($"Inner exception: {ex.InnerException.Message}");
            }
        }

        // If successful, publish messages to PubSub for entity processing
        if (isSuccess && successList.Count > 0)
        {
            try
            {
                // Use the AiContextualService to publish entity processing messages
                // Create a list of dynamic objects that have an Id property for the helper method
                var entities = successList.Select(s => {
                    dynamic entity = new JObject();
                    entity.Id = ((dynamic)s).Id;
                    return entity;
                }).ToList<dynamic>();
                
                await _aiService.PublishEntityProcessingMessages(tableName, entities);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the operation
                _logger.LogError(ex, "Error publishing entity processing messages to PubSub: {ErrorMessage}", ex.Message);
            }
        }

        var result = new
        {
            IsSuccess = isSuccess,
            SuccessCount = successList.Count,
            SuccessRecords = successList.Select(s => new { Id = ((dynamic)s).Id }),
            ErrorCount = errorMessages.Count,
            Errors = errorMessages,
            UpdatedCount = recordsToUpdate.Count,
            InsertedCount = recordsToAdd.Count
        };

        return JsonConvert.SerializeObject(result, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
        });
    }

    public async Task<string> ChatWithGemini(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null)
    {
        var appName = _configuration.GetValue<string>("AgenticAi:AppName");
        var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
        if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(appName))
        {
            throw new InvalidOperationException("AgenticAi configuration is missing or incomplete.");
        }
        
        // Extract user ID from claims
        var currentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Get user email from currentUserId using UserManagementManager
        // TODO: may be fix the interface...
        // var currentUser = await ((UNOPSUserManagementManager)_userManagementManager).GetBasicEntityAsync(currentUserId) as UserManagementModel;
        // TODO: In DEV mode, somehow the currentUserId is set to 90, but the email in the database is empty
        var currentUserEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(currentUserEmail) || string.IsNullOrEmpty(currentUserId))
        {
          throw new InvalidOperationException($"Unable to lookup both current user email {currentUserEmail} and current user id {currentUserId}");
        }
        currentUserEmail = currentUserEmail.Contains(':') ? currentUserEmail.Split(':').Last() : currentUserEmail;

        // Get user profile details to include in state
        var userProfileDetails = await GetUserProfileDetailsAsync(user);
        
        // Enhance the state with user profile information
        var enhancedState = await EnhanceStateWithUserProfile(req.State, userProfileDetails);

        var apiUrl = $"/chat";
        HttpContent httpContent;

        // Check if request has files
        if (req.Files != null && req.Files.Any())
        {
            // Use multipart form data for requests with files
            var multipartContent = new MultipartFormDataContent();
            
            // Add form fields
            multipartContent.Add(new StringContent(appName), "app_name");
            multipartContent.Add(new StringContent(currentUserId.ToString()), "user_id");
            multipartContent.Add(new StringContent(currentUserEmail), "user_email");
            multipartContent.Add(new StringContent(req.sessionId?.ToString() ?? ""), "session_id");
            multipartContent.Add(new StringContent(req.Message ?? ""), "message");
            multipartContent.Add(new StringContent("false"), "streaming");
            multipartContent.Add(new StringContent(enhancedState ?? ""), "state");
            
            // Add files
            foreach (var file in req.Files)
            {
                if (file != null && file.Length > 0)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    multipartContent.Add(streamContent, "files", file.FileName);
                }
            }
            
            httpContent = multipartContent;
            _logger.LogInformation($"Sending chat request with {req.Files.Count()} files to AI service");
        }
        else
        {
            // Use JSON for requests without files (backward compatibility)
            var aiChatRequest = new AiChatRequest
            {
                AppName = appName,
                UserId = currentUserId.ToString(),
                UserEmail = currentUserEmail,
                SessionId = req.sessionId?.ToString() ?? "",
                Message = req.Message ?? "",
                Streaming = false,
                State = enhancedState
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(aiChatRequest);
            httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            _logger.LogInformation("Sending chat request without files to AI service");
        }

        using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);

        var response = await httpClient.PostAsync(apiUrl, httpContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AI service call failed. Status: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        // Check for data_modifications in the response and create notifications
        await ProcessDataModificationsForNotifications(responseContent, int.Parse(currentUserId));

        // Extract sessionId from req or responseContent
        string sessionId = req.sessionId;
        if (string.IsNullOrEmpty(sessionId))
        {
            try
            {
                var responseObj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                sessionId = responseObj["session_id"]?.ToString();
            }
            catch { /* ignore parse errors, sessionId will remain null if not found */ }
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            var session = await _context.AiChatSession.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session != null)
            {
                session.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                var newSession = new AiChatSession
                {
                    Id = sessionId,
                    UserId = int.Parse(currentUserId),
                    Status = "Active",
                    Title = "New Chat",
                    LastUpdated = DateTime.UtcNow,
                    AiGenerateTitle = true,
                    Archived = false,
                    Starred = false
                };
                _context.AiChatSession.Add(newSession);
                await _context.SaveChangesAsync();
            }
        }

        return responseContent;
    }

    public async Task<string> GenerateTitle(string sessionId, int userId)
    {
        // If sessionId is null or empty, throw
        if (string.IsNullOrEmpty(sessionId))
            throw new ArgumentException("SessionId is required");

        var canGenerate = await CanGenerateTitle(sessionId);
        if (!canGenerate)
            throw new InvalidOperationException("Title generation is not allowed for this session.");

        var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
        var apiUrl = $"/generate-title?session_id={sessionId}&user_id={userId}";
        using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
        var response = await httpClient.GetAsync(apiUrl);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to generate title");

        var content = await response.Content.ReadAsStringAsync();
        var result = Newtonsoft.Json.Linq.JObject.Parse(content);
        string title = result["title"]?.ToString();
        await UpdateSessionTitleAndFlag(sessionId, title);
        return title;
    }

    public async Task<bool> CanGenerateTitle(string sessionId)
    {
        var session = await _context.AiChatSession.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
        return session != null && session.AiGenerateTitle;
    }

    public async Task<object> GenerateSuggestions(int userId)
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            var apiUrl = $"/generate-suggestions?user_id={userId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            var response = await httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to generate suggestions. Status: {response.StatusCode}");
                throw new InvalidOperationException($"Failed to generate suggestions. Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.Linq.JObject.Parse(content);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating suggestions: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Process AI response for data_modifications and create notifications
    /// </summary>
    /// <param name="responseContent">The AI response content</param>
    /// <param name="userId">The user ID who triggered the AI action</param>
    private async Task ProcessDataModificationsForNotifications(string responseContent, int userId)
    {
        try
        {
            // Parse the response content to look for data_modifications
            var responseObj = JObject.Parse(responseContent);
            
            // Look for data_modifications in events
            var events = responseObj["events"] as JArray;
            if (events != null)
            {
                foreach (var eventObj in events)
                {
                    await ProcessEventForDataModifications(eventObj, userId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error processing data modifications for notifications: {ex.Message}");
            // Don't rethrow - notification failures shouldn't break the chat response
        }
    }

    /// <summary>
    /// Process a single event for data_modifications
    /// </summary>
    /// <param name="eventObj">The event object to process</param>
    /// <param name="userId">The user ID</param>
    private async Task ProcessEventForDataModifications(JToken eventObj, int userId)
    {
        try
        {
            // Check if event has content
            var content = eventObj["content"];
            if (content != null)
            {
                // Look for data_modifications in the content
                await ExtractAndCreateNotifications(content, userId);
                
                // Also check content parts if they exist
                var parts = content["parts"] as JArray;
                if (parts != null)
                {
                    foreach (var part in parts)
                    {
                        var text = part["text"]?.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            await ExtractAndCreateNotificationsFromText(text, userId);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error processing event for data modifications: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract data_modifications from content and create notifications
    /// </summary>
    /// <param name="content">The content to process</param>
    /// <param name="userId">The user ID</param>
    private async Task ExtractAndCreateNotifications(JToken content, int userId)
    {
        try
        {
            // Convert content to string and try to parse as JSON
            var contentStr = content.ToString();
            
            // Try to parse the content as JSON to find data_modifications
            if (contentStr.Trim().StartsWith("{") || contentStr.Trim().StartsWith("["))
            {
                var contentData = JObject.Parse(contentStr);
                var dataModifications = contentData["data_modifications"] as JArray;
                
                if (dataModifications != null && dataModifications.Count > 0)
                {
                    await CreateNotificationsFromModifications(dataModifications, userId);
                }
            }
        }
        catch (JsonReaderException)
        {
            // Content is not valid JSON, skip
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error extracting notifications from content: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract data_modifications from text content and create notifications
    /// </summary>
    /// <param name="text">The text to process</param>
    /// <param name="userId">The user ID</param>
    private async Task ExtractAndCreateNotificationsFromText(string text, int userId)
    {
        try
        {
            // Try to parse the text as JSON to find data_modifications
            if (text.Trim().StartsWith("{") || text.Trim().StartsWith("["))
            {
                var textData = JObject.Parse(text);
                var dataModifications = textData["data_modifications"] as JArray;
                
                if (dataModifications != null && dataModifications.Count > 0)
                {
                    await CreateNotificationsFromModifications(dataModifications, userId);
                }
            }
        }
        catch (JsonReaderException)
        {
            // Text is not valid JSON, skip
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error extracting notifications from text: {ex.Message}");
        }
    }

    /// <summary>
    /// Create notification records from data_modifications array
    /// </summary>
    /// <param name="dataModifications">Array of data modification objects</param>
    /// <param name="userId">The user ID</param>
    private async Task CreateNotificationsFromModifications(JArray dataModifications, int userId)
    {
        try
        {
            foreach (var modification in dataModifications)
            {
                var modificationType = modification["type"]?.ToString() ?? "unknown";
                var message = modification["message"]?.ToString() ?? "Data modification performed";
                var entityType = modification["entity_type"]?.ToString() ?? "unknown";
                var entityIdRaw = modification["entity_id"]?.ToString();

                // Process entityId - handle cases where it might be "entity_<id>" format
                string cleanEntityId = entityIdRaw;
                if (!string.IsNullOrEmpty(entityIdRaw) && entityIdRaw.Contains('_'))
                {
                    var parts = entityIdRaw.Split('_');
                    if (parts.Length > 1)
                    {
                        cleanEntityId = parts[1]; // Take the ID part after the underscore
                    }
                }

                // Create category in format "ENTITYTYPE_ID"
                var category = $"{entityType?.ToLower() ?? "UNKNOWN"}_{cleanEntityId ?? "0"}";

                // Create notification record
                var notification = new UNOPS.PAO.Domain.Entities.Notification
                {
                    UserId = userId,
                    Message = message,
                    Category = category,
                    ResponseType = modificationType,
                    RecordData = "[]",
                    IsRead = false,
                    Status = UNOPS.PAO.Domain.Enums.NotificationStatus.Done,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                
                _logger.LogInformation($"Created notification for user {userId}: {modificationType} on {entityType} {cleanEntityId}");
            }

            // Save all notifications to database
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Successfully saved {dataModifications.Count} notifications for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating notifications from modifications: {ex.Message}");
            throw;
        }
    }

        /// <summary>
        /// Extracts display fields for showing duplicate information to the user
        /// </summary>
        private object ExtractDisplayFields(dynamic record, string entityName)
        {
            try
            {
                var obj = JObject.FromObject(record);
                
                return entityName.ToLower() switch
                {
                    "contact" or "contacts" => new
                    {
                        firstName = obj["firstName"]?.ToString(),
                        lastName = obj["lastName"]?.ToString(),
                        email = obj["email"]?.ToString(),
                        phone = obj["phone"]?.ToString(),
                        title = obj["title"]?.ToString()
                    },
                    "partner" or "partners" => new
                    {
                        name = obj["name"]?.ToString(),
                        partnerShortDescription = obj["partnerShortDescription"]?.ToString(),
                        erpDimValue = obj["erpDimValue"]?.ToString(),
                        status = obj["status"]?.ToString()
                    },
                    "interaction" or "interactions" => new
                    {
                        type = obj["type"]?.ToString(),
                        subject = obj["subject"]?.ToString(),
                        date = obj["date"]?.ToString(),
                        description = obj["description"]?.ToString()
                    },
                    _ => new
                    {
                        name = obj["name"]?.ToString(),
                        title = obj["title"]?.ToString(),
                        email = obj["email"]?.ToString()
                    }
                };
            }
            catch (Exception)
            {
                return new { error = "Unable to extract display fields" };
            }
        }
    }