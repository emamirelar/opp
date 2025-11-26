using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.DataAccess.Context;
using AutoMapper;
using UNOPS.PAO.Business.Repositories.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Text.Json;
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
using UNOPS.PAO.UNOPSBusiness.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Globalization;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Reflection.Metadata.Ecma335;
using Google.Cloud.AIPlatform.V1;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Models;
using Z.EntityFramework.Plus;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Opportunities;

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
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IUserProfileCacheService _userProfileCacheService;
    private readonly IScreenContextCacheService _screenContextCacheService;
    private readonly IGeoTimeCacheService _geoTimeCacheService;
    private readonly IMemoryCache _memoryCache;
    private readonly HttpClient _httpClient;
    private IManagerWrapper _managerWrapper;
    
    // Session configuration caching
    private readonly string _sessionConfigCacheKey = "session_configuration";
    private readonly TimeSpan _sessionConfigCacheExpiration = TimeSpan.FromHours(1);

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, ILogger<UNOPSGeminiManager> logger, IUserManagementManager userManagementManager, IUserInfoService userInfoService, UserManager<PAOIdentityUser> userManager, RoleManager<PAOIdentityRole> roleManager, IUserPreferenceService userPreferenceService, IUserProfileCacheService userProfileCacheService, IScreenContextCacheService screenContextCacheService, IGeoTimeCacheService geoTimeCacheService, IAiPromptCacheService aiPromptCacheService, IMemoryCache memoryCache, HttpClient httpClient)
    {
        _mapper = mapper;
        _context = context;
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _logger = logger;
        _userManagementManager = userManagementManager;
        _userInfoService = userInfoService;
        _userManager = userManager;
        _roleManager = roleManager;
        _userPreferenceService = userPreferenceService;
        _userProfileCacheService = userProfileCacheService;
        _screenContextCacheService = screenContextCacheService;
        _geoTimeCacheService = geoTimeCacheService;
        _memoryCache = memoryCache;
        _httpClient = httpClient;
        
        // Initialize CloudRunHelper internally
        var cloudRunHelperLogger = new LoggerFactory().CreateLogger<CloudRunHelper>();
        var credentials = GetCredentials();
        _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, credentials);
        
        _credentials = GetCredentials()
                        .CreateScoped("https://www.googleapis.com/auth/spreadsheets.readonly");
        _textExtractionService = new TextExtractionService();
        _gcsService = new GoogleCloudStorageService(configuration);

        _ttsService = new GoogleTextToSpeechService();
        _aiService = new AiContextualService(configuration, _context, _credentials, aiPromptCacheService);
    }

    public void SetManagerWrapper(IManagerWrapper managerWrapper)
    {
        _managerWrapper = managerWrapper;
    }

    // Map AiPromptModel to AiPrompt entity
    private AiPrompt MapModelToEntity(AiPromptModel model)
    {
        var entity = _mapper.Map(model, new AiPrompt
        {
            Type = model.Type ?? "default",
            DataRetrievalMethod = model.DataRetrievalMethod ?? "default",
            GenerationConfig = model.GenerationConfig ?? "{}",
            ContentConfig = model.ContentConfig ?? "{}",
            Project = model.Project ?? "default",
            Location = model.Location ?? "default",
            Model = model.Model ?? "default"
        });
        return entity;
    }

    // Get prompt data by type
    public async Task<IEnumerable<AiPrompt>> GetPromptData(string type)
    {
        return await _aiService.GetPromptData(type);
    }

    // Updated FetchResultFromGemini to use CallGeminiApi
    public async Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData, string entityId = null)
    {
        return await _aiService.FetchResultFromGemini((AiPrompt)promptData, relatedJsonData, entityId, bypassCache: false);
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
        {
            _logger.LogError("UNOPSGeminiManager: AISettings configuration is missing");
            throw new Exception("AISettings configuration is missing.");
        }
    
        var secretName = _configuration.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        
        var basicProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
        var credential = GoogleCredential.FromJson(secretValue);
        
        _logger.LogInformation("UNOPSGeminiManager: Successfully retrieved Google credentials for project: {ProjectId}", 
            credentialParams.ProjectId);
            
        return credential;
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

    public async Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req, ClaimsPrincipal user = null)
    {
        string relatedMessage = "";

        AiPrompt promptModel = MapModelToEntity(req);

        // Call the GetPromptData method and get the first prompt
        AiPrompt promptData = (await GetPromptData(promptModel.Type)).FirstOrDefault();

        if (promptData == null)
        {
            return "";
        }

        // Check if DataRetrievalMethod is available (new approach with backward compatibility)
        var dataRetrievalMethod = !string.IsNullOrEmpty(promptData.DataRetrievalMethod) 
            ? promptData.DataRetrievalMethod 
            : null;
            
        if (!string.IsNullOrEmpty(dataRetrievalMethod))
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
                
                // Create PartnerTreeService instance if needed
                PartnerTreeService partnerTreeService = null;
                if (parameterTypes.Contains(typeof(PartnerTreeService)))
                {
                    var partnerTreeRepository = new DataRepository<UNOPSDomain.Entities.UNOPSPartnerTree>(_context);
                    var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);
                }
                
                foreach (var paramType in parameterTypes)
                {
                    if (paramType == typeof(IMapper))
                        args.Add(_mapper);
                    else if (paramType == typeof(UNOPSAppDbContext))
                        args.Add(_context);
                    else if (paramType == typeof(IConfiguration))
                        args.Add(_configuration);
                    else if (paramType == typeof(PartnerTreeService))
                        args.Add(partnerTreeService);
                    else if (paramType == typeof(IPermissionService))
                        args.Add(null); // IPermissionService is optional and can be null
                    else if (paramType == typeof(UserManager<PAOIdentityUser>))
                        args.Add(_userManager);
                    else if (paramType == typeof(IHttpContextAccessor))
                        args.Add(null); // IHttpContextAccessor not available in this context
                    else
                        args.Add(null); // Pass null for other dependencies we don't have
                }
                
                // Create instance of the manager
                var managerInstance = Activator.CreateInstance(managerType, args.ToArray());
                
                if (managerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create instance of {managerType.Name}");
                }
                
                // Verify that _context is set in the manager instance
                var contextField = managerType.BaseType?.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
                if (contextField != null)
                {
                    var contextValue = contextField.GetValue(managerInstance);
                    if (contextValue == null)
                    {
                        _logger.LogError("_context is null in manager instance {ManagerType}", managerType.Name);
                        throw new InvalidOperationException($"_context is null in {managerType.Name}");
                    }
                }
                
                // Check if it's a BaseUNOPSManager that has CallFunctionByNameAsync
                var callFunctionMethod = managerType.GetMethod("CallFunctionByNameAsync");
                if (callFunctionMethod != null)
                {
                    // Use the BaseUNOPSManager's CallFunctionByNameAsync method which handles parameter matching
                    // Pass the user parameter so that methods can access user context
                    var task = (Task<object>)callFunctionMethod.Invoke(managerInstance, new object[] { dataRetrievalMethod, req.Id, user });
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
                _logger.LogError(ex, "Error calling function {DataRetrievalMethod}: {ErrorMessage}", dataRetrievalMethod, ex.Message);
                return $"Error retrieving data: {ex.Message}";
            }
        }

        // Fetch result from Gemini with caching support
        // Pass entity ID for caching if available
        var entityIdForCache = req.Id > 0 ? req.Id.ToString() : null;
        
        // Pass document storage path if available (for document transcription)
        string geminiResponse;
        if (!string.IsNullOrEmpty(req.DocumentStoragePath) && req.DocumentStoragePath.StartsWith("gs://"))
        {
            // Document transcription: pass gs:// URI and MIME type
            geminiResponse = await _aiService.FetchResultFromGeminiWithDocument(
                promptData, 
                relatedMessage, 
                req.DocumentStoragePath, 
                req.DocumentMimeType ?? "application/pdf",
                entityIdForCache
            );
        }
        else
        {
            // Regular processing without document
            geminiResponse = await FetchResultFromGemini(promptData, relatedMessage, entityIdForCache);
        }
        
        // Process dependent dropdowns for opportunity document transcription
        if (promptData.Type == "opportunity_document_transcribe")
        {
            try
            {
                // Parse the Gemini response to extract the JSON content
                var parsedResponse = _aiService.GetDetailsFromGeminiResponse(geminiResponse);
                
                // Check if there are dependents to process
                var dependents = parsedResponse["dependents"]?.ToString();
                if (!string.IsNullOrEmpty(dependents))
                {
                    // Process dependents to convert names to IDs
                    var processedResponse = await _aiService.GetDependentDropdownValues(dependents, parsedResponse, promptData);
                    
                    // Return the processed response as JSON string
                    return JsonConvert.SerializeObject(processedResponse);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with unprocessed response
                _logger.LogWarning(ex, "Error processing dependent dropdowns for opportunity transcription. Returning unprocessed response.");
            }
        }
        
        return geminiResponse;
    }

    public async Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req)
    {
        string extractedText = await ExtractDataFromFile(req.File);
        string type = req?.Type;

        if (!string.IsNullOrEmpty(type)) {
            var promptData = (await _aiService.GetPromptData(type)).FirstOrDefault();
            if (promptData == null)
            {
                return "";
            }
            // Send to Gemini with the extracted text
            var geminiResponse = await _aiService.FetchResultFromGemini(promptData, extractedText, entityId: null, bypassCache: false);
            var parsedResponse = _aiService.GetDetailsFromGeminiResponse(geminiResponse);

            // Handle the nested structure - check if there's a data array
            dynamic processedResponse;
            
            if (parsedResponse["data"] != null && parsedResponse["data"] is JArray dataArray && dataArray.Count > 0)
            {
                // Process each item in the data array
                var processedDataArray = new JArray();
                
                foreach (var dataItem in dataArray)
                {
                    var dependents = dataItem["dependents"]?.ToString();
                    if (!string.IsNullOrEmpty(dependents))
                    {
                        // Process dependents for this specific data item
                        var processedDataItem = await _aiService.GetDependentDropdownValues(dependents, dataItem, promptData);
                        processedDataArray.Add(JToken.FromObject(processedDataItem));
                    }
                    else
                    {
                        // No dependents to process, add as-is
                        processedDataArray.Add(dataItem);
                    }
                }
                
                // Reconstruct the response with processed data
                processedResponse = new JObject
                {
                    ["Message"] = parsedResponse["Message"],
                    ["Category"] = parsedResponse["Category"],
                    ["ResponseType"] = parsedResponse["ResponseType"],
                    ["data"] = processedDataArray
                };
            }
            else
            {
                // Fallback to original logic for flat structure
                var dependents = parsedResponse["dependents"]?.ToString();
                processedResponse = await _aiService.GetDependentDropdownValues(dependents, parsedResponse, promptData);
            }

            // Return the processed response as JSON string
            return Newtonsoft.Json.JsonConvert.SerializeObject(processedResponse);
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

    public async Task<string> GetSessionDataWithChats(string sessionId, int userId) 
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"/session-with-chats?app_name={appName}&user_id={userId}&session_id={sessionId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                // Return the raw JSON content as-is without any deserialization or transformation
                return jsonContent;
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


    public async Task<IEnumerable<AiChatSession>> GetUserSessions(int userId) 
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"/get-user-sessions?app_name={appName}&user_id={userId}";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("🔍 Raw JSON response from Python service: {JsonContent}", jsonContent);
                
                var settings = new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                };
                
                var externalSessions = JsonConvert.DeserializeObject<IEnumerable<AiChatSession>>(jsonContent, settings);
                
                if (externalSessions == null || !externalSessions.Any())
                {
                    return new List<AiChatSession>();
                }
                
                // All session data now comes from ADK session state via Python service
                // No need to query database - use external session data directly
                var sessions = externalSessions.Select(extSession => new AiChatSession
                {
                    Id = extSession.Id,
                    UserId = extSession.UserId,
                    Status = extSession.Status,
                    LastUpdated = extSession.LastUpdated,
                    Title = extSession.Title,
                    Starred = extSession.Starred,
                    Archived = extSession.Archived,
                    AiGenerateTitle = extSession.AiGenerateTitle
                }).ToList();
                
                return sessions.OrderByDescending(s => s.LastUpdated);
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
        // Session accessibility settings are now managed in ADK session state
        // This functionality should be implemented via Python service if needed
        // For now, return true as the session state is managed elsewhere
        return true;
    }

    public async Task<bool> UpdateSessionStar(string sessionId, bool starred)
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"/update-session-metadata";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var requestBody = new
            {
                sessionId = sessionId,
                starred = starred
            };
            
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new HttpRequestException($"Failed to update session star status. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating session star status: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateSessionArchive(string sessionId, bool archived)
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"/update-session-metadata";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var requestBody = new
            {
                sessionId = sessionId,
                archived = archived
            };
            
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new HttpRequestException($"Failed to update session archive status. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating session archive status: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateSessionTitle(string sessionId, string title)
    {
        try
        {
            var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
            }

            // Get app_name from session configuration
            var sessionConfig = await GetSessionConfigurationAsync();
            var appName = sessionConfig.AppName;
            
            var apiUrl = $"/update-session-title";
            
            using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var requestBody = new
            {
                sessionId = sessionId,
                title = title
            };
            
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new HttpRequestException($"Failed to update session title. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating session title: {ex.Message}", ex);
        }
    }

    public async Task UpdateSessionTitleAndFlag(string sessionId, string title)
    {
        // This method is now handled by UpdateSessionTitle which calls the Python service
        await UpdateSessionTitle(sessionId, title);
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

            var fileData = await _aiService.ReadFileData(req.FileId, req.SheetName);
            if (string.IsNullOrEmpty(fileData))
            {
                throw new Exception("No data found in the Google Sheet. Please ensure the sheet contains data.");
            }

            var fileDataArray = JArray.Parse(fileData);

        // Determine entity name for batch size optimization
        string entityName = GetEntityNameFromPromptType(req.Type);
        
        // For Partners: Always use batch size 5, but check total rows for async vs sync
        // For other entities: Use existing logic (batch size 25, async if > 20 rows)
        bool isPartnerEntity = entityName.Equals("Partners", StringComparison.OrdinalIgnoreCase);
        int totalRows = fileDataArray.Count - 1; // Excluding header row
        
        // Check if we should process asynchronously (changed threshold to 20)
        bool shouldProcessAsync = isPartnerEntity ? (totalRows > 20) : (fileDataArray.Count > 20);
        
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
        
        // Special handling for User Role Import (ASP.NET Core Identity User-Role assignments)
        if (type.Equals("user_role_import", StringComparison.OrdinalIgnoreCase))
        {
            return await BulkInsertUserRolesAsync(request);
        }

        // Use specific manager methods instead of generic entity mapping
        if (type.Equals("interaction", StringComparison.OrdinalIgnoreCase))
        {
            return await BulkInsertInteractionsAsync(request);
        }
        
        if (type.Equals("partner", StringComparison.OrdinalIgnoreCase))
        {
            return await BulkInsertPartnersAsync(request);
        }
        
        if (type.Equals("contact", StringComparison.OrdinalIgnoreCase))
        {
            return await BulkInsertContactsAsync(request);
        }

        // Fallback to generic method for other types
        return await BulkInsertGenericRecordsAsync(request);
    }

    private async Task<string> BulkInsertGenericRecordsAsync(BulkUploadRequest request)
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

    private async Task<string> BulkInsertInteractionsAsync(BulkUploadRequest request)
    {
        var successList = new List<object>();
        var errorMessages = new List<string>();
        var isSuccess = true;

        try
        {
            foreach (var record in request.Records)
            {
                try
                {
                    // Convert JsonElement to JObject for property access
                    JObject recordObj;
                    if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                        recordObj = JObject.FromObject(dictionary);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
                    }

                    // Convert JObject to InteractionRequest
                    var interactionRequest = recordObj.ToObject<UpdateInteractionRequest>();
                    
                    if (interactionRequest == null)
                    {
                        errorMessages.Add("Failed to convert record to InteractionRequest");
                        isSuccess = false;
                        continue;
                    }

                    // Check if this is an update (has ID) or create (no ID or ID = 0)
                    if (interactionRequest.Id > 0)
                    {
                        // Update existing interaction
                        var updateRequest = new UpdateInteractionRequest
                        {
                            Id = interactionRequest.Id,
                            Type = interactionRequest.Type,
                            Date = interactionRequest.Date,
                            Subject = interactionRequest.Subject,
                            Description = interactionRequest.Description,
                            Location = interactionRequest.Location,
                            ContactIds = interactionRequest.ContactIds,
                            PartnerIds = interactionRequest.PartnerIds,
                            UserIds = interactionRequest.UserIds,
                            EmailAddresses = interactionRequest.EmailAddresses,
                            OrganizationHierarchyIds = interactionRequest.OrganizationHierarchyIds
                        };

                        var updatedResult = await _managerWrapper.InteractionManager.UpdateInteractionAsync(0, updateRequest);
                        if (updatedResult != null)
                        {
                            successList.Add(new { Id = updatedResult.Id, Action = "Updated", Subject = updatedResult.Subject });
                        }
                        else
                        {
                            errorMessages.Add($"Failed to update interaction with ID {interactionRequest.Id}");
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        // Create new interaction
                        interactionRequest.Id = 0; // Ensure ID is 0 for new records
                        var createdResult = await _managerWrapper.InteractionManager.CreateInteractionAsync(interactionRequest);
                        if (createdResult != null)
                        {
                            successList.Add(new { Id = createdResult.Id, Action = "Created", Subject = createdResult.Subject });
                        }
                        else
                        {
                            errorMessages.Add("Failed to create interaction");
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Error processing interaction record: {ex.Message}");
                    isSuccess = false;
                }
            }

            var result = new
            {
                IsSuccess = isSuccess,
                SuccessCount = successList.Count,
                ErrorCount = errorMessages.Count,   
                Errors = errorMessages,
                SuccessRecords = successList,
                Message = isSuccess ? 
                    $"Successfully processed {successList.Count} interactions" :
                    $"Processed {successList.Count} interactions with {errorMessages.Count} errors"
            };

            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk interaction operation");
            
            var errorResult = new
            {
                IsSuccess = false,
                SuccessCount = 0,
                ErrorCount = 1,
                Errors = new[] { $"Bulk interaction operation failed: {ex.Message}" },
                SuccessRecords = new object[0],
                Message = $"Bulk interaction operation failed: {ex.Message}"
            };

            return JsonConvert.SerializeObject(errorResult);
        }
    }

    private async Task<string> BulkInsertPartnersAsync(BulkUploadRequest request)
    {
        var successList = new List<object>();
        var errorMessages = new List<string>();
        var isSuccess = true;

        try
        {
            foreach (var record in request.Records)
            {
                try
                {
                    // Convert JsonElement to JObject for property access
                    JObject recordObj;
                    if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                        recordObj = JObject.FromObject(dictionary);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
                    }

                    // Convert JObject to PartnerRequest
                    var partnerRequest = recordObj.ToObject<UpdatePartnerRequest>();
                    
                    if (partnerRequest == null)
                    {
                        errorMessages.Add("Failed to convert record to PartnerRequest");
                        isSuccess = false;
                        continue;
                    }

                    // Check if this is an update (has ID) or create (no ID or ID = 0)
                    if (partnerRequest.Id > 0)
                    {
                        // Update existing partner
                        var updateRequest = new UpdatePartnerRequest
                        {
                            Id = partnerRequest.Id,
                            Name = partnerRequest.Name,
                            PartnerShortDescription = partnerRequest.PartnerShortDescription,
                            PartnerLongDescription = partnerRequest.PartnerLongDescription,
                            Status = partnerRequest.Status,
                            PartnerGroupId = partnerRequest.PartnerGroupId,
                            UNAndStateEntity = partnerRequest.UNAndStateEntity,
                            CanCreateNewOpportunities = partnerRequest.CanCreateNewOpportunities,
                            PooledFund = partnerRequest.PooledFund,
                            OrganizationHierarchyIds = partnerRequest.OrganizationHierarchyIds
                        };

                        var updatedPartnerResult = await _managerWrapper.PartnerManager.UpdatePartnerAsync(0, updateRequest);
                        if (updatedPartnerResult != null)
                        {
                            successList.Add(new { Id = updatedPartnerResult.Id, Action = "Updated", Name = updatedPartnerResult.Name });
                        }
                        else
                        {
                            errorMessages.Add($"Failed to update partner with ID {partnerRequest.Id}");
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        // Create new partner
                        partnerRequest.Id = 0; // Ensure ID is 0 for new records
                        var createdResult = await _managerWrapper.PartnerManager.CreatePartnerAsync(partnerRequest);
                        if (createdResult != null)
                        {
                            successList.Add(new { Id = createdResult.Id, Action = "Created", Name = createdResult.Name });
                        }
                        else
                        {
                            errorMessages.Add("Failed to create partner");
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Error processing partner record: {ex.Message}");
                    isSuccess = false;
                }
            }

            var result = new
            {
                IsSuccess = isSuccess,
                SuccessCount = successList.Count,
                ErrorCount = errorMessages.Count,
                Errors = errorMessages,
                SuccessRecords = successList,
                Message = isSuccess ? 
                    $"Successfully processed {successList.Count} partners" :
                    $"Processed {successList.Count} partners with {errorMessages.Count} errors"
            };

            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk partner operation");
            
            var errorResult = new
            {
                IsSuccess = false,
                SuccessCount = 0,
                ErrorCount = 1,
                Errors = new[] { $"Bulk partner operation failed: {ex.Message}" },
                SuccessRecords = new object[0],
                Message = $"Bulk partner operation failed: {ex.Message}"
            };

            return JsonConvert.SerializeObject(errorResult);
        }
    }

    private async Task<string> BulkInsertContactsAsync(BulkUploadRequest request)
    {
        var successList = new List<object>();
        var errorDetails = new List<object>(); // Changed from List<string> to include record IDs
        var isSuccess = true;

        try
        {
            foreach (var record in request.Records)
            {
                string recordId = null; // Track the record ID for error reporting
                try
                {
                    // Convert JsonElement to JObject for property access
                    JObject recordObj;
                    if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                        recordObj = JObject.FromObject(dictionary);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
                    }

                    // Extract _importRowId for error tracking
                    recordId = recordObj["_importRowId"]?.ToString();

                    // Convert JObject to ContactRequest
                    var contactRequest = recordObj.ToObject<UpdateContactRequest>();
                    
                    if (contactRequest == null)
                    {
                        errorDetails.Add(new {
                            recordId = recordId,
                            _importRowId = recordId,
                            message = "Failed to convert record data - Invalid data format or missing required fields",
                            error = "Data Conversion Failed",
                            details = "Record structure does not match expected contact format"
                        });
                        isSuccess = false;
                        continue;
                    }

                    // Check if this is an update (has ID) or create (no ID or ID = 0)
                    if (contactRequest.Id > 0)
                    {
                        // First, check if the contact exists before attempting update
                        var existingContact = await _managerWrapper.ContactManager.GetContactAsync(contactRequest.Id);
                        
                        if (existingContact == null)
                        {
                            errorDetails.Add(new {
                                recordId = recordId,
                                _importRowId = recordId,
                                message = $"Contact with ID {contactRequest.Id} does not exist in the system",
                                error = "Record Not Found",
                                details = $"Cannot update non-existent contact. The contact with ID {contactRequest.Id} was not found in the database. Consider removing the ID to create a new contact instead."
                            });
                            isSuccess = false;
                        }
                        else
                        {
                            // Update existing contact
                            var updateRequest = new UpdateContactRequest
                            {
                                Id = contactRequest.Id,
                                Salutation = contactRequest.Salutation,
                                FirstName = contactRequest.FirstName,
                                MiddleName = contactRequest.MiddleName,
                                LastName = contactRequest.LastName,
                                Suffix = contactRequest.Suffix,
                                Title = contactRequest.Title,
                                Department = contactRequest.Department,
                                Description = contactRequest.Description,
                                Email = contactRequest.Email,
                                Phone = contactRequest.Phone,
                                Mobile = contactRequest.Mobile,
                                Assistant = contactRequest.Assistant,
                                AssistantPhone = contactRequest.AssistantPhone,
                                AssistantEmail = contactRequest.AssistantEmail,
                                MailingStreet = contactRequest.MailingStreet,
                                MailingStreet2 = contactRequest.MailingStreet2,
                                MailingCity = contactRequest.MailingCity,
                                MailingStateProvince = contactRequest.MailingStateProvince,
                                MailingPostalCode = contactRequest.MailingPostalCode,
                                MailingCountry = contactRequest.MailingCountry,
                                PartnerId = contactRequest.PartnerId
                            };

                            var updatedResult = await _managerWrapper.ContactManager.UpdateContactAsync(0, updateRequest);
                            if (updatedResult != null)
                            {
                                successList.Add(new { Id = updatedResult.Id, Action = "Updated", Name = $"{updatedResult.FirstName} {updatedResult.LastName}", Email = updatedResult.Email });
                            }
                            else
                            {
                                errorDetails.Add(new {
                                    recordId = recordId,
                                    _importRowId = recordId,
                                    message = $"Failed to update contact with ID {contactRequest.Id} - Update operation failed",
                                    error = "Update Failed", 
                                    details = "Update operation completed but returned null - possible business rule validation failure"
                                });
                                isSuccess = false;
                            }
                        }
                    }
                    else
                    {
                        // Create new contact
                        contactRequest.Id = 0; // Ensure ID is 0 for new records
                        var createdResult = await _managerWrapper.ContactManager.CreateContactAsync(contactRequest);
                        if (createdResult != null)
                        {
                            successList.Add(new { Id = createdResult.Id, Action = "Created", Name = $"{createdResult.FirstName} {createdResult.LastName}", Email = createdResult.Email });
                        }
                        else
                        {
                            errorDetails.Add(new {
                                recordId = recordId,
                                _importRowId = recordId,
                                message = "Failed to create contact - Please check all required fields are provided and valid",
                                error = "Creation Failed",
                                details = "Contact creation returned null - validation or business rule failure"
                            });
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Extract detailed error information
                    string errorMessage = ExtractDetailedErrorMessage(ex);
                    string specificError = ExtractSpecificErrorType(ex);
                    
                    errorDetails.Add(new {
                        recordId = recordId,
                        _importRowId = recordId,
                        message = $"Error processing contact: {errorMessage}",
                        error = specificError,
                        details = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace?.Split('\n').Take(3).ToArray(), // First 3 lines for debugging
                        exceptionType = ex.GetType().Name
                    });
                    isSuccess = false;
                }
            }

            var result = new
            {
                IsSuccess = isSuccess,
                SuccessCount = successList.Count,
                ErrorCount = errorDetails.Count,
                ErrorDetails = errorDetails, // New structured error details with record IDs
                Errors = errorDetails.Select(e => ((dynamic)e).message).ToList(), // Backward compatibility
                SuccessRecords = successList,
                Message = isSuccess ? 
                    $"Successfully processed {successList.Count} contacts" :
                    $"Processed {successList.Count} contacts with {errorDetails.Count} errors"
            };

            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk contact operation");
            
            var errorResult = new
            {
                IsSuccess = false,
                SuccessCount = 0,
                ErrorCount = 1,
                Errors = new[] { $"Bulk contact operation failed: {ex.Message}" },
                SuccessRecords = new object[0],
                Message = $"Bulk contact operation failed: {ex.Message}"
            };

            return JsonConvert.SerializeObject(errorResult);
        }
    }

    /// <summary>
    /// Extract detailed error message from exception, handling common database and validation errors
    /// </summary>
    private string ExtractDetailedErrorMessage(Exception ex)
    {
        // Handle DbUpdateException (Entity Framework errors)
        if (ex is DbUpdateException dbEx)
        {
            if (dbEx.InnerException != null)
            {
                var innerMessage = dbEx.InnerException.Message;
                
                // Handle common SQL Server errors with user-friendly messages
                if (innerMessage.Contains("UNIQUE KEY constraint") || innerMessage.Contains("duplicate key"))
                {
                    if (innerMessage.Contains("Email"))
                        return "Email address already exists in the system";
                    if (innerMessage.Contains("Phone"))
                        return "Phone number already exists in the system";
                    return "Duplicate entry detected - this record already exists";
                }
                
                if (innerMessage.Contains("FOREIGN KEY constraint"))
                    return "Referenced data not found - please check related fields";
                
                if (innerMessage.Contains("CHECK constraint"))
                    return "Data validation failed - invalid value provided";
                
                if (innerMessage.Contains("NOT NULL constraint"))
                    return "Required field is missing";
                
                return $"Database error: {innerMessage}";
            }
            return "Database update failed";
        }
        
        // Handle validation exceptions
        if (ex is ArgumentException || ex is ArgumentNullException)
        {
            return $"Validation error: {ex.Message}";
        }
        
        // Handle other specific exceptions
        if (ex is InvalidOperationException)
        {
            return $"Operation error: {ex.Message}";
        }
        
        if (ex is UnauthorizedAccessException)
        {
            return "Access denied - insufficient permissions";
        }
        
        // Default to the main exception message
        return ex.Message ?? "Unknown error occurred";
    }

    /// <summary>
    /// Extract specific error type for categorization
    /// </summary>
    private string ExtractSpecificErrorType(Exception ex)
    {
        if (ex is DbUpdateException dbEx)
        {
            if (dbEx.InnerException?.Message.Contains("UNIQUE KEY") == true)
                return "Duplicate Entry";
            if (dbEx.InnerException?.Message.Contains("FOREIGN KEY") == true)
                return "Reference Error";
            if (dbEx.InnerException?.Message.Contains("CHECK constraint") == true)
                return "Validation Error";
            if (dbEx.InnerException?.Message.Contains("NOT NULL") == true)
                return "Required Field Missing";
            return "Database Error";
        }
        
        if (ex is ArgumentException || ex is ArgumentNullException)
            return "Validation Error";
        
        if (ex is InvalidOperationException)
            return "Operation Error";
        
        if (ex is UnauthorizedAccessException)
            return "Permission Error";
        
        return ex.GetType().Name;
    }

    public async Task<string> ChatWithGemini(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null)
    {
        _logger.LogDebug("ChatWithGemini: Method called with sessionId: {SessionId}, hasFiles: {HasFiles}", 
            req.sessionId, req.Files?.Any() ?? false);
            
        var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
        if (string.IsNullOrEmpty(serviceUrl))
        {
            _logger.LogError("ChatWithGemini: AgenticAi:ServiceURL configuration is missing");
            throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
        }

        // Get app_name from session configuration
        var sessionConfig = await GetSessionConfigurationAsync();
        var appName = sessionConfig.AppName;
        
        // Extract user ID from claims
        var currentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Get user email from currentUserId using UserManagementManager
        // TODO: may be fix the interface...
        // var currentUser = await ((UNOPSUserManagementManager)_userManagementManager).GetBasicEntityAsync(currentUserId) as UserManagementModel;
        // TODO: In DEV mode, somehow the currentUserId is set to 90, but the email in the database is empty
        var currentUserEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(currentUserEmail) || string.IsNullOrEmpty(currentUserId))
        {
          _logger.LogError("ChatWithGemini: Missing user information - UserId: {UserId}, UserEmail: {UserEmail}", 
              currentUserId, currentUserEmail);
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
            _logger.LogDebug("ChatWithGemini: Request has {FileCount} files, using multipart form data", req.Files.Count());
            
            // Use multipart form data for requests with files
            var multipartContent = new MultipartFormDataContent();
            
            // Add form fields
            multipartContent.Add(new StringContent(appName), "app_name");
            multipartContent.Add(new StringContent(currentUserId.ToString()), "user_id");
            multipartContent.Add(new StringContent(currentUserEmail), "user_email");
            multipartContent.Add(new StringContent(req.sessionId?.ToString() ?? ""), "session_id");
            multipartContent.Add(new StringContent(req.Message ?? ""), "message");
            multipartContent.Add(new StringContent(req.Streaming.ToString().ToLower()), "streaming");
            multipartContent.Add(new StringContent(enhancedState ?? ""), "state");
            
            // Add files
            foreach (var file in req.Files)
            {
                if (file != null && file.Length > 0)
                {
                    _logger.LogDebug("ChatWithGemini: Adding file - Name: {FileName}, Size: {FileSize} bytes", 
                        file.FileName, file.Length);
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    multipartContent.Add(streamContent, "files", file.FileName);
                }
            }
            
            httpContent = multipartContent;
        }
        else
        {
            // Use JSON for requests without files
            var aiChatRequest = new AiChatRequest
            {
                AppName = appName,
                UserId = currentUserId.ToString(),
                UserEmail = currentUserEmail,
                SessionId = req.sessionId?.ToString() ?? "",
                Message = req.Message ?? "",
                Streaming = req.Streaming,
                State = enhancedState
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(aiChatRequest);
            httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        HttpClient httpClient;
        
        // For local development, use unauthenticated HttpClient
        if (serviceUrl.StartsWith("http://localhost") || serviceUrl.StartsWith("http://127.0.0.1"))
        {
            _logger.LogDebug("ChatWithGemini: Using local development HttpClient");
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(serviceUrl);
        }
        else
        {
            // For production/Cloud Run, use authenticated HttpClient
            httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
        }

        using (httpClient)
        {
            var response = await httpClient.PostAsync(apiUrl, httpContent);
            
            _logger.LogInformation("ChatWithGemini: Response Status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("ChatWithGemini: AI service call failed - Status: {StatusCode}, Reason: {ReasonPhrase}, Content: {ErrorContent}", 
                    response.StatusCode, response.ReasonPhrase, errorContent);
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
                catch (Exception ex)
                {
                    _logger.LogWarning("ChatWithGemini: Failed to extract sessionId from response: {Error}", ex.Message);
                }
            }

            // Session management is now handled entirely by the Python service
            // No need to create or update AiChatSession entries

            return responseContent;
        }
    }

    public async IAsyncEnumerable<string> ChatWithGeminiStreaming(GeminiAssistantRequest req, ClaimsPrincipal user, IHeaderDictionary headers = null)
    {
        _logger.LogDebug("ChatWithGeminiStreaming: Method called with sessionId: {SessionId}, hasFiles: {HasFiles}", 
            req.sessionId, req.Files?.Any() ?? false);
            
        var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
            
        if (string.IsNullOrEmpty(serviceUrl))
        {
            _logger.LogError("ChatWithGeminiStreaming: AgenticAi:ServiceURL configuration is missing");
            throw new InvalidOperationException("AgenticAi:ServiceURL configuration is missing.");
        }

        // Get app_name from session configuration
        var sessionConfig = await GetSessionConfigurationAsync();
        var appName = sessionConfig.AppName;
        
        // Extract user ID from claims
        var currentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Get user email from currentUserId using UserManagementManager
        var currentUserEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(currentUserEmail) || string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogError("ChatWithGeminiStreaming: Missing user information - UserId: {UserId}, UserEmail: {UserEmail}", 
                currentUserId, currentUserEmail);
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
            
            // Add form fields - ENABLE STREAMING
            multipartContent.Add(new StringContent(appName), "app_name");
            multipartContent.Add(new StringContent(currentUserId.ToString()), "user_id");
            multipartContent.Add(new StringContent(currentUserEmail), "user_email");
            multipartContent.Add(new StringContent(req.sessionId?.ToString() ?? ""), "session_id");
            multipartContent.Add(new StringContent(req.Message ?? ""), "message");
            multipartContent.Add(new StringContent("true"), "streaming"); // Enable streaming
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
                Streaming = true, // Enable streaming
                State = enhancedState
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(aiChatRequest);
            httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        HttpClient httpClient;
        
        // For local development, use unauthenticated HttpClient
        if (serviceUrl.StartsWith("http://localhost") || serviceUrl.StartsWith("http://127.0.0.1"))
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(serviceUrl);
        }
        else
        {
            // For production/Cloud Run, use authenticated HttpClient
            httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
        }

        using (httpClient)
        {
            
            // Set headers for streaming (preserve existing auth headers)
            if (!httpClient.DefaultRequestHeaders.Contains("Accept"))
                httpClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");
            if (!httpClient.DefaultRequestHeaders.Contains("Cache-Control"))
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            if (!httpClient.DefaultRequestHeaders.Contains("Pragma"))
                httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
            if (!httpClient.DefaultRequestHeaders.Contains("Connection"))
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            
            // Configure for streaming - disable buffering and set reasonable timeout
            httpClient.Timeout = TimeSpan.FromMinutes(30); // Long timeout for streaming but not infinite
            
            // Use SendAsync for more control over streaming with ResponseHeadersRead to start reading immediately
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = httpContent
            };
            
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("ChatWithGeminiStreaming: AI service call failed - Status: {StatusCode}, Reason: {ReasonPhrase}, Content: {ErrorContent}", 
                    response.StatusCode, response.ReasonPhrase, errorContent);
                throw new InvalidOperationException($"AI service call failed. Status: {response.StatusCode}");
            }

            // Read the streaming response with immediate forwarding
            using var stream = await response.Content.ReadAsStreamAsync();
            
            string? sessionId = req.sessionId;
            var buffer = new byte[64]; // Small buffer for responsive streaming while avoiding excessive system calls
            var stringBuilder = new StringBuilder();
            
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // End of stream
                    break;
                }
                
                // Convert bytes to string and add to buffer
                var chunk = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                stringBuilder.Append(chunk);
                
                // Process complete lines immediately
                var content = stringBuilder.ToString();
                var lines = content.Split('\n');
                
                // Keep the last incomplete line in the buffer
                if (lines.Length > 1)
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(lines[lines.Length - 1]);
                    
                    // Process all complete lines
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        var line = lines[i].Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Process the JSON line for session management and notifications (non-blocking)
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await ProcessStreamingEventData(line, int.Parse(currentUserId), sessionId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning("ChatWithGeminiStreaming: Error processing line: {Error}", ex.Message);
                                }
                            });
                            
                            // Yield immediately - no waiting
                            yield return line;
                        }
                    }
                }
                else if (content.Length > 0)
                {
                    // If we have partial content but no complete lines, check if it looks like a complete JSON object
                    var trimmedContent = content.Trim();
                    if (trimmedContent.StartsWith("{") && trimmedContent.EndsWith("}"))
                    {
                        // Try to parse as JSON to see if it's complete
                        bool isValidJson = false;
                        try
                        {
                            var testParse = Newtonsoft.Json.Linq.JObject.Parse(trimmedContent);
                            isValidJson = true;
                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            // Not complete JSON yet, continue reading
                            isValidJson = false;
                        }
                        
                        if (isValidJson)
                        {
                            // Process for session management (non-blocking)
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await ProcessStreamingEventData(trimmedContent, int.Parse(currentUserId), sessionId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning("ChatWithGeminiStreaming: Error processing JSON chunk: {Error}", ex.Message);
                                }
                            });
                            
                            yield return trimmedContent;
                            stringBuilder.Clear(); // Clear the buffer since we yielded this content
                        }
                    }
                }
            }
            
            // Process any remaining content in the buffer
            var remainingContent = stringBuilder.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(remainingContent))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessStreamingEventData(remainingContent, int.Parse(currentUserId), sessionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("ChatWithGeminiStreaming: Error processing final content: {Error}", ex.Message);
                    }
                });
                
                yield return remainingContent;
            }
        }
    }

    private async Task<string?> ProcessStreamingEventData(string jsonLine, int userId, string? sessionId)
    {
        try
        {
            // Try to parse the JSON line directly (no SSE format)
            if (jsonLine.Contains("session_id") && string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var eventObj = JObject.Parse(jsonLine);
                    var extractedSessionId = eventObj["session_id"]?.ToString();
                    if (!string.IsNullOrEmpty(extractedSessionId))
                    {
                        sessionId = extractedSessionId;
                        
                        // Session management is now handled entirely by the Python service
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("ProcessStreamingEventData: Failed to parse JSON: {Error}", ex.Message);
                }
            }
            
            // Process data modifications for notifications
            await ProcessDataModificationsForNotifications(jsonLine, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("ProcessStreamingEventData: Error processing event data: {Error}", ex.Message);
            // Don't rethrow - processing failures shouldn't break the stream
        }
        
        return sessionId;
    }

    // Session management is now handled entirely by the Python service
    // No need for CreateOrUpdateSession method



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

        /// <summary>
        /// Handles bulk user-role assignments for ASP.NET Core Identity
        /// </summary>
        private async Task<string> BulkInsertUserRolesAsync(BulkUploadRequest request)
        {
            try
            {
                var successList = new List<object>();
                var errorMessages = new List<string>();
                var isSuccess = true;

                foreach (var record in request.Records)
                {
                    try
                    {
                        // Handle JsonElement properly - convert to JObject for easier access
                        JObject userRoleData;
                        if (record is JsonElement jsonElement)
                        {
                            var jsonString = jsonElement.GetRawText();
                            userRoleData = JObject.Parse(jsonString);
                        }
                        else
                        {
                            // Fallback for other types
                            var recordJson = JsonConvert.SerializeObject(record);
                            userRoleData = JObject.Parse(recordJson);
                        }
                        
                        // Extract resolved userId and roleIds (should already be resolved at this point)
                        var userIdValue = userRoleData["userId"]?.ToString();
                        var roleIdsArray = userRoleData["roleIds"]?.ToObject<List<string>>();
                        
                        if (string.IsNullOrEmpty(userIdValue))
                        {
                            errorMessages.Add("No user ID found in record");
                            isSuccess = false;
                            continue;
                        }
                        
                        if (roleIdsArray == null || !roleIdsArray.Any())
                        {
                            errorMessages.Add("No role IDs found in record");
                            isSuccess = false;
                            continue;
                        }

                        // Parse userId (should be a resolved integer)
                        if (!int.TryParse(userIdValue, out int userId))
                        {
                            errorMessages.Add($"Invalid user ID format: {userIdValue}");
                            isSuccess = false;
                            continue;
                        }

                        // Get the user object for AddToRolesAsync
                        var user = await _userManager.FindByIdAsync(userId.ToString());
                        if (user == null)
                        {
                            errorMessages.Add($"Could not find user with ID: {userId}");
                            isSuccess = false;
                            continue;
                        }

                        // Convert role IDs to role names and check for existing roles
                        var roleNames = new List<string>();
                        foreach (var roleId in roleIdsArray)
                        {
                            var role = await _roleManager.FindByIdAsync(roleId);
                            if (role != null)
                            {
                                roleNames.Add(role.Name);
                            }
                            else
                            {
                                errorMessages.Add($"Could not find role with ID: {roleId}");
                                isSuccess = false;
                            }
                        }

                        if (!roleNames.Any())
                        {
                            errorMessages.Add($"No valid roles found for user: {userId}");
                            isSuccess = false;
                            continue;
                        }

                        // Get current user roles to avoid duplicates
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        
                        // Filter out roles the user already has
                        var rolesToAdd = roleNames.Where(roleName => !currentRoles.Contains(roleName)).ToList();
                        
                        if (rolesToAdd.Any())
                        {
                            // Only add roles that the user doesn't already have
                            var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                            if (!addRolesResult.Succeeded)
                            {
                                var errors = string.Join(", ", addRolesResult.Errors.Select(e => e.Description));
                                errorMessages.Add($"Failed to assign roles to user {userId}: {errors}");
                                isSuccess = false;
                                continue;
                            }
                        }
                        
                        // Determine which roles were skipped (already existed)
                        var skippedRoles = roleNames.Where(roleName => currentRoles.Contains(roleName)).ToList();
                        
                        successList.Add(new 
                        { 
                            userId = userId,
                            rolesAdded = rolesToAdd,
                            rolesSkipped = skippedRoles,
                            allRequestedRoles = roleNames,
                            action = rolesToAdd.Any() ? (skippedRoles.Any() ? "partially_assigned" : "assigned") : "already_assigned"
                        });
                    }
                    catch (Exception recordEx)
                    {
                        errorMessages.Add($"Error processing user-role record: {recordEx.Message}");
                        isSuccess = false;
                    }
                }

                var result = new
                {
                    IsSuccess = isSuccess,
                    SuccessCount = successList.Count,
                    ErrorCount = errorMessages.Count,
                    Errors = errorMessages.ToArray(),
                    SuccessRecords = successList.ToArray(),
                    Message = isSuccess ? 
                        $"Successfully processed {successList.Count} user-role assignments" :
                        $"Processed {successList.Count} user-role assignments with {errorMessages.Count} errors"
                };

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during bulk user-role operation");
                
                var errorResult = new
                {
                    IsSuccess = false,
                    SuccessCount = 0,
                    ErrorCount = 1,
                    Errors = new[] { $"Bulk user-role operation failed: {ex.Message}" },
                    SuccessRecords = new object[0],
                    Message = $"Bulk user-role operation failed: {ex.Message}"
                };

                return JsonConvert.SerializeObject(errorResult);
            }
        }

        #region Session Configuration Methods

        /// <summary>
        /// Gets the session configuration including app_name and other session-related settings.
        /// This method fetches configuration from the Python service and caches it.
        /// </summary>
        /// <returns>Session configuration with app_name and other settings</returns>
        public async Task<SessionConfiguration> GetSessionConfigurationAsync()
        {
            // Try to get from cache first
            if (_memoryCache.TryGetValue(_sessionConfigCacheKey, out SessionConfiguration cachedConfig))
            {
                _logger.LogDebug("📋 Retrieved session configuration from cache");
                return cachedConfig;
            }

            // If not in cache, fetch from Python service
            try
            {
                var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
                if (string.IsNullOrEmpty(serviceUrl))
                {
                    _logger.LogError("❌ AgenticAi:ServiceURL is not configured");
                    throw new InvalidOperationException("AgenticAi:ServiceURL is not configured");
                }

                var configUrl = $"{serviceUrl.TrimEnd('/')}/api/ai-assistant/configuration";
                _logger.LogInformation("🔍 Fetching session configuration from: {ConfigUrl}", configUrl);

                HttpResponseMessage response;
                string jsonContent;
                
                // For local development, use unauthenticated HttpClient
                if (serviceUrl.StartsWith("http://localhost") || serviceUrl.StartsWith("http://127.0.0.1"))
                {
                    _logger.LogDebug("GetSessionConfiguration: Using local development HttpClient");
                    response = await _httpClient.GetAsync(configUrl);
                    response.EnsureSuccessStatusCode();
                    jsonContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // For production/Cloud Run, use authenticated HttpClient
                    _logger.LogDebug("GetSessionConfiguration: Creating authenticated HttpClient for Cloud Run");
                    using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);
                    response = await httpClient.GetAsync(configUrl);
                    response.EnsureSuccessStatusCode();
                    jsonContent = await response.Content.ReadAsStringAsync();
                }
                _logger.LogInformation("📋 Raw JSON response from Python service: {JsonContent}", jsonContent);
                
                var config = System.Text.Json.JsonSerializer.Deserialize<SessionConfiguration>(jsonContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config == null)
                {
                    throw new InvalidOperationException("Failed to deserialize session configuration");
                }

                _logger.LogInformation("📋 Deserialized configuration - AppName: '{AppName}', ApplicationName: '{ApplicationName}'", 
                    config.AppName, config.ApplicationName);

                // Cache the configuration
                _memoryCache.Set(_sessionConfigCacheKey, config, _sessionConfigCacheExpiration);
                _logger.LogInformation("✅ Session configuration cached successfully: {AppName}", config.AppName);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to fetch session configuration from Python service");
                throw new InvalidOperationException("AI service is unavailable - cannot fetch session configuration", ex);
            }
        }

        /// <summary>
        /// Clears the cached session configuration, forcing a fresh fetch on next request.
        /// </summary>
        public void ClearSessionConfigurationCache()
        {
            _memoryCache.Remove(_sessionConfigCacheKey);
            _logger.LogInformation("🗑️ Session configuration cache cleared");
        }

        #endregion

        #region Similar Projects

        /// <summary>
        /// Finds similar projects for an opportunity using AI-powered keyword extraction and vector store search
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to find similar projects for</param>
        /// <param name="maxResults">Maximum number of similar projects to return (default: 10)</param>
        /// <param name="user">Current user context</param>
        /// <returns>Response containing similar projects and extracted keywords</returns>
        public async Task<UNOPS.PAO.Models.SimilarProjectsResponse> GetSimilarProjectsAsync(int opportunityId, int maxResults = 6, ClaimsPrincipal user = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation($"🔍 [SIMILAR-PROJECTS] Starting similar projects search for opportunity {opportunityId}");
                
                // Step 1: Get opportunity data through manager wrapper
                if (_managerWrapper == null)
                {
                    throw new InvalidOperationException("Manager wrapper not initialized");
                }
                
                var opportunityManager = _managerWrapper.OpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("Opportunity manager not available");
                }
                
                // Cast to UNOPSOpportunityManager to access BaseUNOPSManager methods
                if (!(opportunityManager is UNOPSOpportunityManager uNOPSOpportunityManager))
                {
                    throw new InvalidOperationException("Opportunity manager must be UNOPSOpportunityManager type");
                }
                
                // Get complete opportunity context via DataRetrievalMethod
                _logger.LogInformation($"📊 [SIMILAR-PROJECTS] Fetching opportunity context for ID {opportunityId}");
                var opportunityContext = await uNOPSOpportunityManager.CallFunctionByNameAsync("GetOpportunityDetailsForAIAsync", opportunityId, user);
                
                if (opportunityContext == null)
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }
                
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityContext);
                _logger.LogInformation($"✅ [SIMILAR-PROJECTS] Opportunity context retrieved. Length: {opportunityContextJson.Length} characters");
                
                // Step 2: Extract keywords using Gemini AI
                _logger.LogInformation($"🤖 [SIMILAR-PROJECTS] Extracting semantic search keywords using Gemini AI");
                var promptData = await _aiService.GetPromptData("opportunity_extract_keywords");
                var extractKeywordsPrompt = promptData.FirstOrDefault();
                
                if (extractKeywordsPrompt == null)
                {
                    throw new InvalidOperationException("Keyword extraction prompt 'opportunity_extract_keywords' not found in database");
                }
                
                var keywords = await _aiService.ExtractKeywordsForSemanticSearchAsync(opportunityContextJson, extractKeywordsPrompt);
                _logger.LogInformation($"✅ [SIMILAR-PROJECTS] Extracted {keywords.Count} keywords: {string.Join(", ", keywords.Take(5))}...");
                
                // Combine keywords into a single search query
                var searchQuery = string.Join(" ", keywords);
                
                // Step 3: Search vector store for similar projects
                _logger.LogInformation($"🔎 [SIMILAR-PROJECTS] Searching vector store with query: \"{searchQuery.Substring(0, Math.Min(100, searchQuery.Length))}...\"");
                
                var vectorStoreRequest = new UNOPS.PAO.Models.AI.VectorStoreSearchRequest
                {
                    Query = searchQuery,
                    MaxResults = maxResults,
                    EntityTypeId = "PROJECT",  // Search for projects
                    EntityId = "",
                    ApplicationId = "",
                    DatasourceId = "",
                    DatasourceConnector = "GOOGLE_BIGQUERY",  // Filter by BigQuery datasource
                    PrimaryRelatedToEntityTypeId = "",
                    PrimaryRelatedToEntityId = "",
                    Filters = new Dictionary<string, string>(),
                    Debug = false
                };
                
                // Use AiRetrieverManager to search
                var aiRetrieverManager = _managerWrapper.AiRetrieverManager;
                if (aiRetrieverManager == null)
                {
                    throw new InvalidOperationException("AI Retriever manager not available");
                }
                
                var userEmail = user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var vectorStoreResponse = await aiRetrieverManager.SearchVectorStoreAsync(vectorStoreRequest, userEmail);
                
                _logger.LogInformation($"✅ [SIMILAR-PROJECTS] Vector store search returned {vectorStoreResponse.Documents?.Count ?? 0} results");
                
                // Step 4: Map vector store documents to similar project models
                var similarProjects = new List<UNOPS.PAO.Models.SimilarProjectModel>();
                
                if (vectorStoreResponse.Documents != null && vectorStoreResponse.Documents.Any())
                {
                    foreach (var doc in vectorStoreResponse.Documents)
                    {
                        var projectId = doc.EntityId ?? doc.DocumentId;
                        var similarProject = new UNOPS.PAO.Models.SimilarProjectModel
                        {
                            ProjectId = projectId,
                            Description = ExtractFromMetadata(doc.Metadata, "Project_Description"),
                            RelevanceScore = doc.Score * 100, // Convert to 0-100 scale
                            StartDate = ExtractFromMetadata(doc.Metadata, "Implementation_Start_Date"),
                            EndDate = ExtractFromMetadata(doc.Metadata, "Implementation_End_Date"),
                            Partners = ExtractFromMetadata(doc.Metadata, "Partners"),
                            Countries = ExtractFromMetadata(doc.Metadata, "Project_Country_List"),
                            ProjectManagerName = ExtractFromMetadata(doc.Metadata, "Project_Manager_Name"),
                            ProjectManagerEmail = ExtractFromMetadata(doc.Metadata, "Project_Manager_Email_Address"),
                            ProjectUrl = $"https://projects.unops.org/#b0/{projectId}/dashboard/overview"
                        };
                        
                        similarProjects.Add(similarProject);
                    }
                }
                
                // Step 5: Refine results with Gemini to add relevance explanations
                if (similarProjects.Any())
                {
                    _logger.LogInformation($"🤖 [SIMILAR-PROJECTS] Refining {similarProjects.Count} projects with AI-generated relevance explanations");
                    
                    try
                    {
                        var refinePromptData = await _aiService.GetPromptData("opportunity_refine_projects");
                        var refinePrompt = refinePromptData.FirstOrDefault();
                        
                        if (refinePrompt != null)
                        {
                            // Prepare the data for the refine prompt
                            var opportunityData = opportunityContext as Dictionary<string, object>;
                            var placeholders = new Dictionary<string, string>
                            {
                                { "opportunityName", opportunityData?.GetValueOrDefault("name")?.ToString() ?? "" },
                                { "opportunityDescription", opportunityData?.GetValueOrDefault("description")?.ToString() ?? "" },
                                { "proposedInitiativeTypeName", opportunityData?.GetValueOrDefault("proposedInitiativeTypeName")?.ToString() ?? "" },
                                { "countries", opportunityData?.GetValueOrDefault("countries")?.ToString() ?? "" },
                                { "sdGs", opportunityData?.GetValueOrDefault("sdGs")?.ToString() ?? "" },
                                { "deliverables", opportunityData?.GetValueOrDefault("deliverables")?.ToString() ?? "" },
                                { "projects", JsonConvert.SerializeObject(new { projects = similarProjects }) }
                            };
                            
                            // Process placeholders in the prompt
                            var refinedPrompt = _aiService.ProcessPlaceholders(refinePrompt.UserPrompt, JsonConvert.SerializeObject(placeholders));
                            
                            // Call Gemini to refine the projects
                            var refineResponse = await _aiService.FetchResultFromGemini(refinePrompt, refinedPrompt, opportunityId.ToString());
                            
                            if (!string.IsNullOrEmpty(refineResponse))
                            {
                                try
                                {
                                    // Extract JSON from Gemini response (handles both raw JSON and wrapped in API response)
                                    var extractedJson = ExtractJsonFromGeminiResponse(refineResponse);
                                    
                                    if (!string.IsNullOrEmpty(extractedJson))
                                    {
                                        var refinedData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(extractedJson);
                                        if (refinedData != null && refinedData.ContainsKey("projects"))
                                        {
                                            var refinedProjects = JsonConvert.DeserializeObject<List<UNOPS.PAO.Models.SimilarProjectModel>>(refinedData["projects"].ToString());
                                            if (refinedProjects != null)
                                            {
                                                similarProjects = refinedProjects;
                                                _logger.LogInformation($"✅ [SIMILAR-PROJECTS] Successfully added relevance explanations to {similarProjects.Count} projects");
                                            }
                                        }
                                    }
                                }
                                catch (Exception parseEx)
                                {
                                    _logger.LogWarning(parseEx, $"⚠️ [SIMILAR-PROJECTS] Failed to parse refined response: {parseEx.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception refineEx)
                    {
                        _logger.LogWarning(refineEx, $"⚠️ [SIMILAR-PROJECTS] Failed to refine projects with relevance explanations, returning original results: {refineEx.Message}");
                        // Continue with original results if refinement fails
                    }
                }
                
                var executionTime = DateTime.UtcNow - startTime;
                
                var response = new UNOPS.PAO.Models.SimilarProjectsResponse
                {
                    SimilarProjects = similarProjects,
                    ExtractedKeywords = keywords,
                    TotalFound = similarProjects.Count,
                    ExecutionTimeMs = (long)executionTime.TotalMilliseconds
                };
                
                _logger.LogInformation($"✅ [SIMILAR-PROJECTS] Search completed successfully in {executionTime.TotalMilliseconds}ms. Found {similarProjects.Count} similar projects");
                
                return response;
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, $"❌ [SIMILAR-PROJECTS] Error finding similar projects for opportunity {opportunityId}: {ex.Message}");
                
                // Return empty result on error
                return new UNOPS.PAO.Models.SimilarProjectsResponse
                {
                    SimilarProjects = new List<UNOPS.PAO.Models.SimilarProjectModel>(),
                    ExtractedKeywords = new List<string>(),
                    TotalFound = 0,
                    ExecutionTimeMs = (long)executionTime.TotalMilliseconds
                };
            }
        }
        
        /// <summary>
        /// Gets relevant people from corporate directory for an opportunity
        /// Step 1: Extract role keywords from opportunity context using specialized prompt
        /// Step 2: Search vector store for PERSON entity type
        /// Step 3: Map results to relevant person models
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to find relevant people for</param>
        /// <param name="maxResults">Maximum number of relevant people to return (default: 10)</param>
        /// <param name="user">Current user context</param>
        /// <returns>Response containing relevant people and extracted roles</returns>
        public async Task<UNOPS.PAO.Models.RelevantPeopleResponse> GetRelevantPeopleAsync(int opportunityId, int maxResults = 10, ClaimsPrincipal user = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation($"👥 [RELEVANT-PEOPLE] Starting relevant people search for opportunity {opportunityId}");
                
                // Step 1: Get opportunity data through manager wrapper
                if (_managerWrapper == null)
                {
                    throw new InvalidOperationException("Manager wrapper not initialized");
                }
                
                var opportunityManager = _managerWrapper.OpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("Opportunity manager not available");
                }
                
                // Cast to UNOPSOpportunityManager to access BaseUNOPSManager methods
                if (!(opportunityManager is UNOPSOpportunityManager uNOPSOpportunityManager))
                {
                    throw new InvalidOperationException("Opportunity manager must be UNOPSOpportunityManager type");
                }
                
                // Get complete opportunity context via DataRetrievalMethod
                _logger.LogInformation($"📊 [RELEVANT-PEOPLE] Fetching opportunity context for ID {opportunityId}");
                var opportunityContext = await uNOPSOpportunityManager.CallFunctionByNameAsync("GetOpportunityDetailsForAIAsync", opportunityId, user);
                
                if (opportunityContext == null)
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }
                
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityContext);
                _logger.LogInformation($"✅ [RELEVANT-PEOPLE] Opportunity context retrieved. Length: {opportunityContextJson.Length} characters");
                
                // Step 2: Extract role keywords using specialized Gemini AI prompt
                _logger.LogInformation($"🤖 [RELEVANT-PEOPLE] Extracting role keywords using specialized prompt 'opportunity_extract_people_keywords'");
                var promptData = await _aiService.GetPromptData("opportunity_extract_people_keywords");
                var extractRolesPrompt = promptData.FirstOrDefault();
                
                if (extractRolesPrompt == null)
                {
                    throw new InvalidOperationException("Role extraction prompt 'opportunity_extract_people_keywords' not found in database");
                }
                
                var roles = await _aiService.ExtractKeywordsForSemanticSearchAsync(opportunityContextJson, extractRolesPrompt);
                _logger.LogInformation($"✅ [RELEVANT-PEOPLE] Extracted {roles.Count} role keywords: {string.Join(", ", roles.Take(5))}...");
                
                // Combine roles into a single search query
                var searchQuery = string.Join(" ", roles);
                
                // Step 3: Search vector store for PERSON entity
                _logger.LogInformation($"🔎 [RELEVANT-PEOPLE] Searching vector store for PERSON entity with query: \"{searchQuery.Substring(0, Math.Min(100, searchQuery.Length))}...\"");
                
                var vectorStoreRequest = new UNOPS.PAO.Models.AI.VectorStoreSearchRequest
                {
                    Query = searchQuery,
                    MaxResults = maxResults,
                    EntityTypeId = "PERSON",  // Search for people
                    EntityId = "",
                    ApplicationId = "",
                    DatasourceId = "",
                    DatasourceConnector = "GOOGLE_BIGQUERY",  // Corporate directory doesn't need specific connector
                    PrimaryRelatedToEntityTypeId = "",
                    PrimaryRelatedToEntityId = "",
                    Filters = new Dictionary<string, string>(),
                    Debug = false
                };
                
                // Use AiRetrieverManager to search
                var aiRetrieverManager = _managerWrapper.AiRetrieverManager;
                if (aiRetrieverManager == null)
                {
                    throw new InvalidOperationException("AI Retriever manager not available");
                }
                
                var userEmail = user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var vectorStoreResponse = await aiRetrieverManager.SearchVectorStoreAsync(vectorStoreRequest, userEmail);
                
                _logger.LogInformation($"✅ [RELEVANT-PEOPLE] Vector store search returned {vectorStoreResponse.Documents?.Count ?? 0} results");
                
                // Step 4: Map vector store documents to relevant person models
                var relevantPeople = new List<UNOPS.PAO.Models.RelevantPersonModel>();
                
                if (vectorStoreResponse.Documents != null && vectorStoreResponse.Documents.Any())
                {
                    foreach (var doc in vectorStoreResponse.Documents)
                    {
                        var personId = doc.EntityId ?? doc.DocumentId;
                        
                        // Extract expertise from metadata if available (could be skills, areas of expertise, etc.)
                        var expertiseStr = ExtractFromMetadata(doc.Metadata, "Expertise") 
                                          ?? ExtractFromMetadata(doc.Metadata, "Skills") 
                                          ?? ExtractFromMetadata(doc.Metadata, "Areas_Of_Expertise");
                        var expertiseList = string.IsNullOrEmpty(expertiseStr) 
                            ? new List<string>() 
                            : expertiseStr.Split(',').Select(e => e.Trim()).ToList();
                        
                        var relevantPerson = new UNOPS.PAO.Models.RelevantPersonModel
                        {
                            PersonId = personId,
                            Name = ExtractFromMetadata(doc.Metadata, "Name") 
                                  ?? ExtractFromMetadata(doc.Metadata, "Full_Name") 
                                  ?? ExtractFromMetadata(doc.Metadata, "DisplayName"),
                            Title = ExtractFromMetadata(doc.Metadata, "Title") 
                                   ?? ExtractFromMetadata(doc.Metadata, "Job_Title") 
                                   ?? ExtractFromMetadata(doc.Metadata, "Position"),
                            Department = ExtractFromMetadata(doc.Metadata, "Department") 
                                       ?? ExtractFromMetadata(doc.Metadata, "Organizational_Unit") 
                                       ?? ExtractFromMetadata(doc.Metadata, "Unit"),
                            Email = ExtractFromMetadata(doc.Metadata, "Email") 
                                   ?? ExtractFromMetadata(doc.Metadata, "Email_Address"),
                            Location = ExtractFromMetadata(doc.Metadata, "Location") 
                                      ?? ExtractFromMetadata(doc.Metadata, "Duty_Station") 
                                      ?? ExtractFromMetadata(doc.Metadata, "Office"),
                            PhotoUrl = ExtractFromMetadata(doc.Metadata, "Photo") 
                                      ?? ExtractFromMetadata(doc.Metadata, "ProfilePicture") 
                                      ?? ExtractFromMetadata(doc.Metadata, "ProfilePhoto"),
                            Expertise = expertiseList.Any() ? expertiseList : null,
                            RelevanceScore = doc.Score * 100, // Convert to 0-100 scale
                            Metadata = doc.Metadata
                        };
                        
                        relevantPeople.Add(relevantPerson);
                    }
                }
                
                // Step 5: Refine results with Gemini to add relevance explanations
                if (relevantPeople.Any())
                {
                    _logger.LogInformation($"🤖 [RELEVANT-PEOPLE] Refining {relevantPeople.Count} people with AI-generated relevance explanations");
                    
                    try
                    {
                        var refinePromptData = await _aiService.GetPromptData("opportunity_refine_people");
                        var refinePrompt = refinePromptData.FirstOrDefault();
                        
                        if (refinePrompt != null)
                        {
                            // Prepare the data for the refine prompt
                            var opportunityData = opportunityContext as Dictionary<string, object>;
                            var placeholders = new Dictionary<string, string>
                            {
                                { "opportunityName", opportunityData?.GetValueOrDefault("name")?.ToString() ?? "" },
                                { "opportunityDescription", opportunityData?.GetValueOrDefault("description")?.ToString() ?? "" },
                                { "proposedInitiativeTypeName", opportunityData?.GetValueOrDefault("proposedInitiativeTypeName")?.ToString() ?? "" },
                                { "countries", opportunityData?.GetValueOrDefault("countries")?.ToString() ?? "" },
                                { "sdGs", opportunityData?.GetValueOrDefault("sdGs")?.ToString() ?? "" },
                                { "deliverables", opportunityData?.GetValueOrDefault("deliverables")?.ToString() ?? "" },
                                { "expertiseAreas", string.Join(", ", roles) },
                                { "people", JsonConvert.SerializeObject(new { people = relevantPeople }) }
                            };
                            
                            // Process placeholders in the prompt
                            var refinedPrompt = _aiService.ProcessPlaceholders(refinePrompt.UserPrompt, JsonConvert.SerializeObject(placeholders));
                            
                            // Call Gemini to refine the people
                            var refineResponse = await _aiService.FetchResultFromGemini(refinePrompt, refinedPrompt, opportunityId.ToString());
                            
                            if (!string.IsNullOrEmpty(refineResponse))
                            {
                                try
                                {
                                    // Extract JSON from Gemini response (handles both raw JSON and wrapped in API response)
                                    var extractedJson = ExtractJsonFromGeminiResponse(refineResponse);
                                    
                                    if (!string.IsNullOrEmpty(extractedJson))
                                    {
                                        var refinedData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(extractedJson);
                                        if (refinedData != null && refinedData.ContainsKey("people"))
                                        {
                                            var refinedPeople = JsonConvert.DeserializeObject<List<UNOPS.PAO.Models.RelevantPersonModel>>(refinedData["people"].ToString());
                                            if (refinedPeople != null)
                                            {
                                                relevantPeople = refinedPeople;
                                                _logger.LogInformation($"✅ [RELEVANT-PEOPLE] Successfully added relevance explanations to {relevantPeople.Count} people");
                                            }
                                        }
                                    }
                                }
                                catch (Exception parseEx)
                                {
                                    _logger.LogWarning(parseEx, $"⚠️ [RELEVANT-PEOPLE] Failed to parse refined response: {parseEx.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception refineEx)
                    {
                        _logger.LogWarning(refineEx, $"⚠️ [RELEVANT-PEOPLE] Failed to refine people with relevance explanations, returning original results: {refineEx.Message}");
                        // Continue with original results if refinement fails
                    }
                }
                
                var executionTime = DateTime.UtcNow - startTime;
                
                var response = new UNOPS.PAO.Models.RelevantPeopleResponse
                {
                    RelevantPeople = relevantPeople,
                    ExtractedRoles = roles,
                    TotalFound = relevantPeople.Count,
                    SearchTimestamp = DateTime.UtcNow
                };
                
                _logger.LogInformation($"✅ [RELEVANT-PEOPLE] Search completed successfully in {executionTime.TotalMilliseconds}ms. Found {relevantPeople.Count} relevant people");
                
                return response;
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, $"❌ [RELEVANT-PEOPLE] Error finding relevant people for opportunity {opportunityId}: {ex.Message}");
                
                // Return empty result on error
                return new UNOPS.PAO.Models.RelevantPeopleResponse
                {
                    RelevantPeople = new List<UNOPS.PAO.Models.RelevantPersonModel>(),
                    ExtractedRoles = new List<string>(),
                    TotalFound = 0,
                    SearchTimestamp = DateTime.UtcNow
                };
            }
        }
        
        /// <summary>
        /// Helper method to extract a value from metadata dictionary
        /// </summary>
        private string? ExtractFromMetadata(Dictionary<string, object>? metadata, string key)
        {
            if (metadata == null || !metadata.ContainsKey(key))
                return null;
                
            var value = metadata[key]?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        
        /// <summary>
        /// Gets AI-powered DST risk recommendations for an opportunity (3-step process)
        /// Step 1: Extract risk keywords from opportunity context
        /// Step 2: Search vector store for similar risks
        /// Step 3: Refine and rank top 5 risks with LLM
        /// </summary>
        public async Task<UNOPS.PAO.Models.DSTRecommendationsResponse> GetDSTRecommendationsAsync(int opportunityId, ClaimsPrincipal? user = null, int maxResults = 10)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation($"🎯 [DST-RECOMMENDATIONS] Starting DST recommendations for opportunity {opportunityId}");
            
            try
            {
                // Step 1: Get opportunity details for AI context
                _logger.LogInformation($"📋 [DST-RECOMMENDATIONS] Step 1: Fetching opportunity details for AI context");
                var opportunityManager = _managerWrapper.OpportunityManager as UNOPSOpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("Opportunity manager not available");
                }
                
                var opportunityDetails = await opportunityManager.CallFunctionByNameAsync("GetOpportunityDetailsForAIAsync", opportunityId, user);
                
                if (opportunityDetails == null)
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }
                
                var opportunityDetailsDict = opportunityDetails as Dictionary<string, object>;
                if (opportunityDetailsDict == null)
                {
                    throw new InvalidOperationException("Unable to convert opportunity details to dictionary");
                }
                
                // Step 2: Extract risk-related keywords using LLM
                _logger.LogInformation($"🔍 [DST-RECOMMENDATIONS] Step 2: Extracting risk keywords from opportunity context");
                var keywords = await ExtractRiskKeywordsAsync(opportunityDetailsDict, user);
                
                if (!keywords.Any())
                {
                    _logger.LogWarning($"⚠️ [DST-RECOMMENDATIONS] No keywords extracted, returning empty recommendations");
                    return new UNOPS.PAO.Models.DSTRecommendationsResponse
                    {
                        Recommendations = new List<UNOPS.PAO.Models.DSTRecommendation>(),
                        ExtractedKeywords = new List<string>(),
                        TotalFound = 0,
                        ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                    };
                }
                
                _logger.LogInformation($"✅ [DST-RECOMMENDATIONS] Extracted {keywords.Count} risk keywords: {string.Join(", ", keywords.Take(5))}...");
                
                // Combine keywords into a single search query
                var searchQuery = string.Join(" ", keywords);
                
                // Step 3: Search vector store for similar risks
                _logger.LogInformation($"🔎 [DST-RECOMMENDATIONS] Step 3: Searching vector store for similar risks with query: \"{searchQuery.Substring(0, Math.Min(100, searchQuery.Length))}...\"");
                
                var vectorStoreRequest = new UNOPS.PAO.Models.AI.VectorStoreSearchRequest
                {
                    Query = searchQuery,
                    MaxResults = maxResults,
                    EntityTypeId = "RISK",  // Search for risks
                    EntityId = "",
                    ApplicationId = "",
                    DatasourceId = "",
                    DatasourceConnector = "GOOGLE_BIGQUERY",  // Filter by BigQuery datasource
                    PrimaryRelatedToEntityTypeId = "",
                    PrimaryRelatedToEntityId = "",
                    Filters = new Dictionary<string, string>(),
                    Debug = false
                };
                
                // Use AiRetrieverManager to search
                var aiRetrieverManager = _managerWrapper.AiRetrieverManager;
                if (aiRetrieverManager == null)
                {
                    throw new InvalidOperationException("AI Retriever manager not available");
                }
                
                var userEmail = user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var vectorStoreResponse = await aiRetrieverManager.SearchVectorStoreAsync(vectorStoreRequest, userEmail);
                
                _logger.LogInformation($"✅ [DST-RECOMMENDATIONS] Vector store search returned {vectorStoreResponse.Documents?.Count ?? 0} risk results");
                
                // Step 4: Refine and rank risks using LLM
                _logger.LogInformation($"🤖 [DST-RECOMMENDATIONS] Step 4: Refining and ranking top risks with LLM");
                var refinedRecommendations = await RefineAndRankRisksAsync(opportunityDetailsDict, vectorStoreResponse, user);
                
                var executionTime = DateTime.UtcNow - startTime;
                
                var response = new UNOPS.PAO.Models.DSTRecommendationsResponse
                {
                    Recommendations = refinedRecommendations,
                    ExtractedKeywords = keywords,
                    TotalFound = refinedRecommendations.Count(),
                    ExecutionTimeMs = (long)executionTime.TotalMilliseconds
                };
                
                _logger.LogInformation($"✅ [DST-RECOMMENDATIONS] Completed successfully in {executionTime.TotalMilliseconds}ms. Returned {refinedRecommendations.Count()} recommendations");
                
                return response;
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, $"❌ [DST-RECOMMENDATIONS] Error getting DST recommendations for opportunity {opportunityId}: {ex.Message}");
                
                // Return empty result on error
                return new UNOPS.PAO.Models.DSTRecommendationsResponse
                {
                    Recommendations = new List<UNOPS.PAO.Models.DSTRecommendation>(),
                    ExtractedKeywords = new List<string>(),
                    TotalFound = 0,
                    ExecutionTimeMs = (long)executionTime.TotalMilliseconds
                };
            }
        }
        
        /// <summary>
        /// Extract risk-related keywords from opportunity context using LLM
        /// Uses the opportunity_extract_risk_keywords AI prompt
        /// </summary>
        private async Task<List<string>> ExtractRiskKeywordsAsync(Dictionary<string, object> opportunityDetails, ClaimsPrincipal? user)
        {
            try
            {
                _logger.LogInformation($"🔍 [EXTRACT-RISK-KEYWORDS] Calling LLM to extract risk keywords from opportunity context");
                
                // Get the keyword extraction prompt
                var promptData = await _aiService.GetPromptData("opportunity_extract_risk_keywords");
                var extractKeywordsPrompt = promptData.FirstOrDefault();
                
                if (extractKeywordsPrompt == null)
                {
                    throw new InvalidOperationException("Risk keyword extraction prompt 'opportunity_extract_risk_keywords' not found in database");
                }
                
                // Convert opportunity details to JSON string
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityDetails);
                
                // Call Gemini to extract keywords
                var keywords = await _aiService.ExtractKeywordsForSemanticSearchAsync(opportunityContextJson, extractKeywordsPrompt);
                _logger.LogInformation($"✅ [EXTRACT-RISK-KEYWORDS] Successfully extracted {keywords?.Count ?? 0} risk keywords");
                return keywords ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [EXTRACT-RISK-KEYWORDS] Error extracting risk keywords: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Refine and rank risks from vector store using LLM
        /// Uses the refine_opportunity_risks AI prompt to select top 5 most relevant risks
        /// </summary>
        private async Task<List<UNOPS.PAO.Models.DSTRecommendation>> RefineAndRankRisksAsync(
            Dictionary<string, object> opportunityDetails,
            UNOPS.PAO.Models.AI.VectorStoreSearchResponse vectorStoreResponse,
            ClaimsPrincipal? user)
        {
            try
            {
                _logger.LogInformation($"🤖 [REFINE-RISKS] Calling LLM to refine and rank {vectorStoreResponse.Documents?.Count ?? 0} risks");
                
                // Get the refine risks prompt
                var promptData = await _aiService.GetPromptData("refine_opportunity_risks");
                var refineRisksPrompt = promptData.FirstOrDefault();
                
                if (refineRisksPrompt == null)
                {
                    throw new InvalidOperationException("Risk refinement prompt 'refine_opportunity_risks' not found in database");
                }
                
                // Prepare vector store risks as JSON string for the prompt
                var vectorStoreRisks = JsonConvert.SerializeObject(vectorStoreResponse.Documents ?? new List<UNOPS.PAO.Models.AI.VectorStoreDocument>());
                
                // Create prompt data combining opportunity details and vector store risks
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityDetails);
                var promptDataJson = $"{{\"opportunityDetails\": {opportunityContextJson}, \"vectorStoreRisks\": {vectorStoreRisks}}}";
                
                // Call Gemini to refine and rank risks
                var refinedRisksJson = await _aiService.FetchResultFromGemini(refineRisksPrompt, promptDataJson, entityId: null, bypassCache: false);
                
                _logger.LogInformation($"📝 [REFINE-RISKS] Raw Gemini response: {refinedRisksJson}");
                
                // Parse the Gemini response to extract the text content
                var geminiResponse = JObject.Parse(refinedRisksJson);
                var textContent = geminiResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                
                if (string.IsNullOrEmpty(textContent))
                {
                    _logger.LogWarning($"⚠️ [REFINE-RISKS] No text content found in Gemini response");
                    return new List<UNOPS.PAO.Models.DSTRecommendation>();
                }
                
                _logger.LogInformation($"📝 [REFINE-RISKS] Extracted text content: {textContent}");
                
                // Try to extract JSON array from the response
                List<UNOPS.PAO.Models.DSTRecommendation>? refinedRisks = null;
                
                // Remove markdown code fences if present
                textContent = System.Text.RegularExpressions.Regex.Replace(textContent, @"```json\s*|\s*```", "");
                textContent = textContent.Trim();
                
                // Try to find the JSON array
                var arrayStart = textContent.IndexOf('[');
                var arrayEnd = textContent.LastIndexOf(']');
                
                if (arrayStart >= 0 && arrayEnd > arrayStart)
                {
                    var jsonArray = textContent.Substring(arrayStart, arrayEnd - arrayStart + 1);
                    _logger.LogInformation($"📝 [REFINE-RISKS] Extracted JSON array: {jsonArray}");
                    
                    try
                    {
                        refinedRisks = JsonConvert.DeserializeObject<List<UNOPS.PAO.Models.DSTRecommendation>>(jsonArray);
                    }
                    catch (Newtonsoft.Json.JsonException ex)
                    {
                        _logger.LogError(ex, $"❌ [REFINE-RISKS] Failed to deserialize JSON array: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning($"⚠️ [REFINE-RISKS] Could not find JSON array in response: {textContent}");
                }
                
                if (refinedRisks != null && refinedRisks.Any())
                {
                    _logger.LogInformation($"✅ [REFINE-RISKS] Successfully refined and ranked {refinedRisks.Count} risks");
                    
                    // Add relevance scores from vector store if available
                    for (int i = 0; i < refinedRisks.Count && i < (vectorStoreResponse.Documents?.Count ?? 0); i++)
                    {
                        refinedRisks[i].RelevanceScore = vectorStoreResponse.Documents![i].Score * 100;
                        refinedRisks[i].SourceRiskId = vectorStoreResponse.Documents[i].DocumentId;
                    }
                    
                    return refinedRisks;
                }
                
                _logger.LogWarning($"⚠️ [REFINE-RISKS] No refined risks returned from LLM");
                return new List<UNOPS.PAO.Models.DSTRecommendation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [REFINE-RISKS] Error refining and ranking risks: {ex.Message}");
                return new List<UNOPS.PAO.Models.DSTRecommendation>();
            }
        }
        
        /// <summary>
        /// Generates AI-powered insights and suggestions for an opportunity
        /// Analyzes opportunity data for completeness, quality, strategic alignment, and provides actionable recommendations
        /// Uses the opportunity_generate_insights AI prompt
        /// </summary>
        public async Task<UNOPS.PAO.Models.OpportunityInsightsResponse> GenerateOpportunityInsightsAsync(
            int opportunityId, 
            ClaimsPrincipal? user = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation($"🔍 [INSIGHTS] Starting AI insights generation for opportunity {opportunityId}");

                // Get comprehensive opportunity data
                var opportunityManager = _managerWrapper.OpportunityManager as UNOPSOpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("UNOPSOpportunityManager is required for insights generation");
                }

                var opportunityDetails = await opportunityManager.GetOpportunityDetailsForAIAsync(opportunityId);

                if (opportunityDetails == null || !opportunityDetails.Any())
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }

                _logger.LogInformation($"📊 [INSIGHTS] Retrieved opportunity details with {opportunityDetails.Count} fields");

                // Get the insights generation prompt
                var promptData = await _aiService.GetPromptData("opportunity_generate_insights");
                var insightsPrompt = promptData.FirstOrDefault();
                
                if (insightsPrompt == null)
                {
                    throw new InvalidOperationException("Insights generation prompt 'opportunity_generate_insights' not found in database");
                }

                // Call AI service to generate insights
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityDetails);
                var aiResponse = await _aiService.FetchResultFromGemini(insightsPrompt, opportunityContextJson, entityId: opportunityId.ToString(), bypassCache: false);

                _logger.LogInformation($"📝 [INSIGHTS] Received AI response: {aiResponse?.Substring(0, Math.Min(200, aiResponse?.Length ?? 0))}...");

                // Parse the Gemini response to extract the text content
                var geminiResponse = JObject.Parse(aiResponse);
                var textContent = geminiResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                
                if (string.IsNullOrEmpty(textContent))
                {
                    _logger.LogWarning($"⚠️ [INSIGHTS] No text content found in Gemini response");
                    stopwatch.Stop();
                    return new UNOPS.PAO.Models.OpportunityInsightsResponse
                    {
                        Insights = new List<UNOPS.PAO.Models.OpportunityInsight>(),
                        Suggestions = new List<UNOPS.PAO.Models.OpportunitySuggestion>(),
                        AnalysisConfidence = 0,
                        AnalysisTimestamp = DateTime.UtcNow,
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                    };
                }
                
                _logger.LogInformation($"📄 [INSIGHTS] Full text content from AI: {textContent}");
                
                // Parse the JSON response with robust handling
                JObject parsedResponse;
                try
                {
                    // Try direct parse first
                    parsedResponse = JObject.Parse(textContent);
                    _logger.LogInformation($"✅ [INSIGHTS] Successfully parsed JSON directly");
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    _logger.LogWarning($"⚠️ [INSIGHTS] Direct JSON parse failed, attempting to extract JSON from text: {ex.Message}");
                    
                    // Try to extract JSON from markdown or wrapped text
                    var jsonMatch = System.Text.RegularExpressions.Regex.Match(textContent, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);
                    if (jsonMatch.Success)
                    {
                        try
                        {
                            parsedResponse = JObject.Parse(jsonMatch.Value);
                            _logger.LogInformation($"✅ [INSIGHTS] Successfully extracted and parsed JSON from text");
                        }
                        catch
                        {
                            _logger.LogError($"❌ [INSIGHTS] Failed to parse extracted JSON. Raw text content: {textContent}");
                            stopwatch.Stop();
                            return new UNOPS.PAO.Models.OpportunityInsightsResponse
                            {
                                Insights = new List<UNOPS.PAO.Models.OpportunityInsight>(),
                                Suggestions = new List<UNOPS.PAO.Models.OpportunitySuggestion>(),
                                AnalysisConfidence = 0,
                                AnalysisTimestamp = DateTime.UtcNow,
                                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                            };
                        }
                    }
                    else
                    {
                        _logger.LogError($"❌ [INSIGHTS] No JSON object found in text content: {textContent}");
                        stopwatch.Stop();
                        return new UNOPS.PAO.Models.OpportunityInsightsResponse
                        {
                            Insights = new List<UNOPS.PAO.Models.OpportunityInsight>(),
                            Suggestions = new List<UNOPS.PAO.Models.OpportunitySuggestion>(),
                            AnalysisConfidence = 0,
                            AnalysisTimestamp = DateTime.UtcNow,
                            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                        };
                    }
                }
                
                _logger.LogInformation($"📋 [INSIGHTS] Parsed response keys: {string.Join(", ", parsedResponse.Properties().Select(p => p.Name))}");
                
                var insights = parsedResponse["insights"]?.ToObject<List<UNOPS.PAO.Models.OpportunityInsight>>() ?? new();
                var suggestions = parsedResponse["suggestions"]?.ToObject<List<UNOPS.PAO.Models.OpportunitySuggestion>>() ?? new();
                var confidence = parsedResponse["analysisConfidence"]?.Value<double>() ?? 0.85;
                var timestamp = parsedResponse["analysisTimestamp"]?.Value<DateTime>() ?? DateTime.UtcNow;
                
                _logger.LogInformation($"📊 [INSIGHTS] Deserialized {insights.Count} insights and {suggestions.Count} suggestions");

                stopwatch.Stop();

                _logger.LogInformation(
                    $"✅ [INSIGHTS] Generated {insights.Count()} insights and {suggestions.Count()} suggestions for opportunity {opportunityId} in {stopwatch.ElapsedMilliseconds}ms"
                );

                return new UNOPS.PAO.Models.OpportunityInsightsResponse
                {
                    Insights = insights,
                    Suggestions = suggestions,
                    AnalysisConfidence = confidence,
                    AnalysisTimestamp = timestamp,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [INSIGHTS] Error generating insights for opportunity {opportunityId}: {ex.Message}");
                stopwatch.Stop();
                
                return new UNOPS.PAO.Models.OpportunityInsightsResponse
                {
                    Insights = new List<UNOPS.PAO.Models.OpportunityInsight>(),
                    Suggestions = new List<UNOPS.PAO.Models.OpportunitySuggestion>(),
                    AnalysisConfidence = 0,
                    AnalysisTimestamp = DateTime.UtcNow,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        /// <summary>
        /// Generates AI-powered opportunity proposal from multiple sources (interactions, documents, existing opportunities)
        /// Supports flexible source selection: interactions, documents, or combination
        /// Fetches source data, sends to Gemini AI, and processes dependents
        /// </summary>
        /// <summary>
        /// Generates AI-powered opportunity proposal from multiple sources (interactions, documents, or both)
        /// Documents are passed directly to Gemini via GCS URIs in the parts array
        /// Frontend converts Office docs to PDF and uploads to GCS before calling this method
        /// </summary>
        public async Task<UNOPS.PAO.Models.Opportunities.OpportunityProposalResponse> GenerateOpportunityProposalAsync(
            UNOPS.PAO.Models.Opportunities.OpportunityProposalRequest request,
            ClaimsPrincipal? user = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation($"🔍 [OPPORTUNITY-PROPOSAL] Starting proposal generation: " +
                    $"Interactions={request.InteractionIds?.Count ?? 0}, " +
                    $"NewDocuments={request.NewDocumentStoragePaths?.Count ?? 0}, " +
                    $"ExistingDocuments={request.ExistingDocumentIds?.Count ?? 0}, " +
                    $"PartnerId={request.PartnerId}");

                // Step 1: Get partner information (if provided)
                string partnerName = "Unknown Partner";
                int? partnerId = request.PartnerId;
                
                if (request.PartnerId.HasValue && request.PartnerId.Value > 0)
                {
                    var partnerManager = _managerWrapper.PartnerManager as UNOPSPartnerManager;
                    if (partnerManager == null)
                    {
                        throw new InvalidOperationException("UNOPSPartnerManager is required");
                    }

                    var partner = await partnerManager.GetPartnerAsync(request.PartnerId.Value);
                    if (partner == null)
                    {
                        throw new KeyNotFoundException($"Partner with ID {request.PartnerId} not found");
                    }

                    partnerName = partner.Name ?? "Unknown Partner";
                    _logger.LogInformation($"📊 [OPPORTUNITY-PROPOSAL] Found partner: {partnerName}");
                }
                else if (request.InteractionIds != null && request.InteractionIds.Any())
                {
                    // Try to infer partner from first interaction if not provided
                    var interactionManager = _managerWrapper.InteractionManager as UNOPSInteractionManager;
                    if (interactionManager != null)
                    {
                        var firstInteraction = await interactionManager.GetInteractionDetailsForOpportunityCreationAsync(request.InteractionIds.First());
                        if (firstInteraction != null && firstInteraction.TryGetValue("partners", out var partnersObj))
                        {
                            var partnersList = partnersObj as List<dynamic>;
                            if (partnersList != null && partnersList.Any())
                            {
                                partnerId = partnersList.First().id;
                                partnerName = partnersList.First().name ?? "Unknown Partner";
                                _logger.LogInformation($"📊 [OPPORTUNITY-PROPOSAL] Inferred partner from interaction: {partnerName}");
                            }
                        }
                    }
                }

                // Step 2: Get interaction details if provided
                var interactionsList = new List<Dictionary<string, object>>();
                if (request.InteractionIds != null && request.InteractionIds.Any())
                {
                    var interactionManager = _managerWrapper.InteractionManager as UNOPSInteractionManager;
                    if (interactionManager == null)
                    {
                        throw new InvalidOperationException("UNOPSInteractionManager is required");
                    }

                    foreach (var interactionId in request.InteractionIds)
                    {
                        var interactionDetails = await interactionManager.GetInteractionDetailsForOpportunityCreationAsync(interactionId);
                        if (interactionDetails != null)
                        {
                            interactionsList.Add(interactionDetails);
                        }
                    }

                    _logger.LogInformation($"✅ [OPPORTUNITY-PROPOSAL] Retrieved {interactionsList.Count} interaction details");
                }

                // Step 3: Gather document GCS paths from two sources:
                // 1. New documents: Already uploaded to GCS by frontend, paths provided directly
                // 2. Existing documents: Query database by ID to get their GCS paths
                var documentParts = new List<(string storagePath, string mimeType, int? documentId)>();
                
                // 3a. Add newly uploaded documents
                if (request.NewDocumentStoragePaths != null && request.NewDocumentStoragePaths.Any())
                {
                    _logger.LogInformation($"📄 [OPPORTUNITY-PROPOSAL] Processing {request.NewDocumentStoragePaths.Count} newly uploaded documents");
                    
                    for (int i = 0; i < request.NewDocumentStoragePaths.Count; i++)
                    {
                        var storagePath = request.NewDocumentStoragePaths[i];
                        var mimeType = request.NewDocumentMimeTypes != null && i < request.NewDocumentMimeTypes.Count
                            ? request.NewDocumentMimeTypes[i]
                            : "application/pdf";
                            
                        if (!string.IsNullOrEmpty(storagePath) && storagePath.StartsWith("gs://"))
                        {
                            documentParts.Add((storagePath, mimeType, null));
                            _logger.LogInformation($"  ✓ New document: {storagePath} ({mimeType})");
                        }
                    }
                }
                
                // 3b. Add existing documents from database
                var existingDocumentIds = new List<int>();
                if (request.ExistingDocumentIds != null && request.ExistingDocumentIds.Any())
                {
                    _logger.LogInformation($"📄 [OPPORTUNITY-PROPOSAL] Retrieving {request.ExistingDocumentIds.Count} existing documents from database");
                    
                    var documentManager = _managerWrapper.DocumentManager as UNOPSDocumentManager;
                    if (documentManager == null)
                    {
                        throw new InvalidOperationException("UNOPSDocumentManager is required");
                    }

                    foreach (var documentId in request.ExistingDocumentIds)
                    {
                        try
                        {
                            var document = await _context.Documents.FindAsync(documentId);
                            if (document != null && !string.IsNullOrEmpty(document.StoragePath) && document.StoragePath.StartsWith("gs://"))
                            {
                                var mimeType = !string.IsNullOrEmpty(document.Type) 
                                    ? document.Type 
                                    : "application/pdf";
                                    
                                documentParts.Add((document.StoragePath, mimeType, documentId));
                                existingDocumentIds.Add(documentId);
                                _logger.LogInformation($"  ✓ Existing document {documentId}: {document.StoragePath} ({mimeType})");
                            }
                            else
                            {
                                _logger.LogWarning($"  ⚠️ Document {documentId} has no GCS storage path, skipping");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"  ⚠️ Error retrieving document {documentId}: {ex.Message}");
                        }
                    }
                }

                // Validate we have at least one source
                if (!interactionsList.Any() && !documentParts.Any())
                {
                    throw new InvalidOperationException("At least one source (interaction or document) is required for proposal generation");
                }

                _logger.LogInformation($"📊 [OPPORTUNITY-PROPOSAL] Total sources: {interactionsList.Count} interactions, {documentParts.Count} documents");

                // Step 4: Build prompt data
                var promptData = await _aiService.GetPromptData("opportunity_from_interactions");
                var opportunityPrompt = promptData.FirstOrDefault();
                
                if (opportunityPrompt == null)
                {
                    throw new InvalidOperationException("Prompt 'opportunity_from_interactions' not found in database");
                }

                // Determine partner role string
                string partnerRole = "";
                if (request.IsFundingPartner && request.IsClientPartner)
                {
                    partnerRole = "Both Funding and Client Partner";
                }
                else if (request.IsFundingPartner)
                {
                    partnerRole = "Funding Partner";
                }
                else if (request.IsClientPartner)
                {
                    partnerRole = "Client Partner";
                }

                // Step 5: Format source data for the prompt
                var interactionsJson = JsonConvert.SerializeObject(interactionsList, Formatting.Indented);
                
                // Build document metadata (not the full content, just references)
                var documentMetadata = documentParts.Select((doc, index) => new
                {
                    index = index + 1,
                    storagePath = doc.storagePath,
                    mimeType = doc.mimeType,
                    documentId = doc.documentId,
                    isNewUpload = !doc.documentId.HasValue
                }).ToList();
                var documentsJson = JsonConvert.SerializeObject(documentMetadata, Formatting.Indented);

                // Build the prompt context
                var promptContext = new Dictionary<string, object>
                {
                    { "opportunityName", request.OpportunityName },
                    { "opportunityDescription", request.OpportunityDescription },
                    { "partnerId", partnerId ?? 0 },
                    { "partnerName", partnerName },
                    { "partnerRole", partnerRole },
                    { "interactions", interactionsJson },
                    { "documents", documentsJson },
                    { "hasInteractions", interactionsList.Any() },
                    { "hasDocuments", documentParts.Any() },
                    { "sourceCount", interactionsList.Count + documentParts.Count }
                };

                var promptJson = JsonConvert.SerializeObject(promptContext);
                
                // Process placeholders in system instructions
                var systemInstructionsTemplate = opportunityPrompt.SystemInstructions ?? string.Empty;
                var fullyFormedSystemInstructions = _aiService.ProcessPlaceholders(systemInstructionsTemplate, promptJson);
                
                // Process placeholders in user prompt
                var userPromptTemplate = opportunityPrompt.UserPrompt ?? string.Empty;
                var fullyFormedUserPrompt = _aiService.ProcessPlaceholders(userPromptTemplate, promptJson);

                _logger.LogInformation($"📝 [OPPORTUNITY-PROPOSAL] Calling Gemini AI with {documentParts.Count} document(s) in parts array");

                // Step 6: Build parts array for Gemini API (text + document URIs)
                var parts = new List<object>
                {
                    new { text = fullyFormedUserPrompt }
                };

                // Add each document as a fileData part
                foreach (var doc in documentParts)
                {
                    parts.Add(new 
                    { 
                        fileData = new
                        {
                            fileUri = doc.storagePath,
                            mimeType = doc.mimeType
                        }
                    });
                }

                // Build user content with parts array
                var userContent = new
                {
                    role = "user",
                    parts = parts.ToArray()
                };
                
                // Call Gemini API directly with document parts
                var aiResponse = await _aiService.CallGeminiApi(userContent, opportunityPrompt, fullyFormedSystemInstructions);

                _logger.LogInformation($"📄 [OPPORTUNITY-PROPOSAL] Received AI response (length: {aiResponse?.Length ?? 0} chars)");

                // Step 7: Parse AI response
                var parsedResponse = _aiService.GetDetailsFromGeminiResponse(aiResponse);

                // Step 8: Process dependent dropdowns (convert text names to IDs)
                var dependents = parsedResponse["dependents"]?.ToString();
                if (!string.IsNullOrEmpty(dependents))
                {
                    _logger.LogInformation($"🔄 [OPPORTUNITY-PROPOSAL] Processing dependents: {dependents}");
                    parsedResponse = await _aiService.GetDependentDropdownValues(dependents, parsedResponse, opportunityPrompt);
                }
                else
                {
                    _logger.LogWarning($"⚠️ [OPPORTUNITY-PROPOSAL] No dependents found in AI response, collection fields may not be properly resolved");
                }

                // Step 9: Stringify collection fields to avoid serialization issues
                // The frontend will parse these JSON strings
                _logger.LogInformation($"🔄 [OPPORTUNITY-PROPOSAL] Stringifying collection fields for safe transport");
                
                var proposedData = new UNOPS.PAO.Models.Opportunities.ProposedOpportunityData
                {
                    Name = parsedResponse["name"]?.ToString() ?? "",
                    Description = parsedResponse["description"]?.ToString() ?? "",
                    PartnerReference = parsedResponse["partnerReference"]?.ToString(),
                    ResponsibleOrgUnitId = parsedResponse["responsibleOrgUnitId"]?.ToObject<int?>(),
                    ResponsibleOrgUnitName = parsedResponse["responsibleOrgUnitName"]?.ToString(),
                    ProposedInitiativeTypeId = parsedResponse["proposedInitiativeTypeId"]?.ToObject<int?>(),
                    ProposedInitiativeTypeName = parsedResponse["proposedInitiativeTypeName"]?.ToString(),
                    InitiativeBudgetUSD = parsedResponse["initiativeBudgetUSD"]?.ToObject<decimal?>(),
                    PartnershipAgreementReference = parsedResponse["partnershipAgreementReference"]?.ToString(),
                    TargetSigningDate = parsedResponse["targetSigningDate"]?.ToObject<DateTime?>(),
                    TargetDeliveryDate = parsedResponse["targetDeliveryDate"]?.ToObject<DateTime?>(),
                    StrategicAlignment = parsedResponse["strategicAlignment"]?.ToString(),
                    ResultsFocus = parsedResponse["resultsFocus"]?.ToString(),
                    IntendedImpactOutcomes = parsedResponse["intendedImpactOutcomes"]?.ToString(),
                    ExpectedBeneficiaries = parsedResponse["expectedBeneficiaries"]?.ToString(),
                    
                    // Stringify collection fields (these are arrays of objects after GetDependentDropdownValues)
                    FundingPartners = parsedResponse["fundingPartners"]?.ToString(),
                    ClientPartners = parsedResponse["clientPartners"]?.ToString(),
                    Stakeholders = parsedResponse["stakeholders"]?.ToString(),
                    Deliverables = parsedResponse["deliverables"]?.ToString(),
                    Countries = parsedResponse["countries"]?.ToString(),
                    SdGs = parsedResponse["sdGs"]?.ToString(),
                    
                    Dependents = parsedResponse["dependents"]?.ToObject<List<string>>() ?? new List<string>()
                };

                stopwatch.Stop();

                _logger.LogInformation($"✅ [OPPORTUNITY-PROPOSAL] Successfully generated opportunity proposal in {stopwatch.ElapsedMilliseconds}ms");
                _logger.LogInformation($"📊 [OPPORTUNITY-PROPOSAL] Collection fields stringified - FundingPartners: {proposedData.FundingPartners?.Length ?? 0} chars");

                // Step 10: Build response
                return new UNOPS.PAO.Models.Opportunities.OpportunityProposalResponse
                {
                    Opportunity = proposedData,
                    InteractionsAnalyzed = interactionsList.Count,
                    SourceInteractionIds = request.InteractionIds,
                    DocumentsAnalyzed = documentParts.Count,
                    SourceDocumentIds = existingDocumentIds.Any() ? existingDocumentIds : null,
                    PartnerId = partnerId,
                    PartnerName = partnerName,
                    IsFundingPartner = request.IsFundingPartner,
                    IsClientPartner = request.IsClientPartner
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [OPPORTUNITY-PROPOSAL] Error generating opportunity proposal: {ex.Message}");
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Extracts JSON content from Gemini API response
        /// Handles both raw JSON and responses wrapped in API structure with markdown code blocks
        /// </summary>
        /// <param name="geminiResponse">The raw response from Gemini API</param>
        /// <returns>Extracted JSON string, or empty string if extraction fails</returns>
        private string ExtractJsonFromGeminiResponse(string geminiResponse)
        {
            try
            {
                // First, try to parse as a Gemini API response structure
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(geminiResponse);
                
                // Check if it's wrapped in the standard Gemini API response format
                if (apiResponse?.candidates != null && apiResponse.candidates.Count > 0)
                {
                    var firstCandidate = apiResponse.candidates[0];
                    if (firstCandidate?.content?.parts != null && firstCandidate.content.parts.Count > 0)
                    {
                        var textContent = firstCandidate.content.parts[0]?.text?.ToString();
                        
                        if (!string.IsNullOrEmpty(textContent))
                        {
                            // Remove markdown code block wrapping if present (```json ... ```)
                            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                                textContent, 
                                @"```(?:json)?\s*\n?(.*?)\n?```", 
                                System.Text.RegularExpressions.RegexOptions.Singleline
                            );
                            
                            if (jsonMatch.Success)
                            {
                                return jsonMatch.Groups[1].Value.Trim();
                            }
                            
                            // If no markdown wrapping, return the text content directly
                            return textContent.Trim();
                        }
                    }
                }
                
                // If it's already valid JSON (not wrapped), return as is
                return geminiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ Failed to extract JSON from Gemini response: {ex.Message}");
                return geminiResponse; // Return original if extraction fails
            }
        }

        #endregion

        #region Opportunity Statement Generation

        /// <summary>
        /// Generates a comprehensive opportunity statement in markdown format following the UNOPS template
        /// Retrieves opportunity details and attached documents, sends to Gemini for analysis
        /// Caches the result and saves to the Opportunity entity
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to generate statement for</param>
        /// <param name="user">Current user context</param>
        /// <returns>Generated opportunity statement in markdown format</returns>
        public async Task<string> GenerateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal? user = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation($"📝 [OPPORTUNITY-STATEMENT] Starting statement generation for opportunity {opportunityId}");

                // Step 1: Get opportunity manager
                var opportunityManager = _managerWrapper.OpportunityManager as UNOPSOpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("UNOPSOpportunityManager is required for statement generation");
                }

                // Step 2: Get comprehensive opportunity data
                var opportunityDetails = await opportunityManager.GetOpportunityDetailsForAIAsync(opportunityId);

                if (opportunityDetails == null || !opportunityDetails.Any())
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }

                _logger.LogInformation($"📊 [OPPORTUNITY-STATEMENT] Retrieved opportunity details with {opportunityDetails.Count} fields");

                // Step 3: Get attached document GCS URIs through DocumentRelationships
                var documents = await _context.DocumentRelationships
                    .Where(dr => dr.EntityType == "Opportunity" && 
                                dr.EntityId == opportunityId)
                    .Include(dr => dr.Document)
                    .Where(dr => dr.Document != null &&
                                !string.IsNullOrEmpty(dr.Document.StoragePath) && 
                                dr.Document.StoragePath.StartsWith("gs://"))
                    .Select(dr => new 
                    { 
                        storagePath = dr.Document!.StoragePath, 
                        mimeType = dr.Document.Type ?? "application/pdf",
                        name = dr.Document.Name
                    })
                    .ToListAsync();

                var documentCount = documents.Count;
                _logger.LogInformation($"📄 [OPPORTUNITY-STATEMENT] Found {documentCount} attached documents");

                // Step 4: Get the statement generation prompt
                var promptData = await _aiService.GetPromptData("opportunity_generate_statement");
                var statementPrompt = promptData.FirstOrDefault();
                
                if (statementPrompt == null)
                {
                    throw new InvalidOperationException("Statement generation prompt 'opportunity_generate_statement' not found in database");
                }

                // Step 5: Prepare opportunity context and document metadata for the prompt
                var opportunityContextJson = JsonConvert.SerializeObject(opportunityDetails, Formatting.Indented);
                var documentsMetadata = documents.Select((doc, index) => new
                {
                    index = index + 1,
                    name = doc.name,
                    storagePath = doc.storagePath,
                    mimeType = doc.mimeType
                }).ToList();
                var documentsJson = JsonConvert.SerializeObject(documentsMetadata, Formatting.Indented);

                // Build prompt context
                var promptContext = new Dictionary<string, object>
                {
                    { "opportunityDetails", opportunityContextJson },
                    { "documents", documentsJson },
                    { "hasDocuments", documents.Any() },
                    { "documentCount", documentCount }
                };

                var promptJson = JsonConvert.SerializeObject(promptContext);
                
                // Process placeholders in system instructions
                var systemInstructionsTemplate = statementPrompt.SystemInstructions ?? string.Empty;
                var fullyFormedSystemInstructions = _aiService.ProcessPlaceholders(systemInstructionsTemplate, promptJson);
                
                // Process placeholders in user prompt
                var userPromptTemplate = statementPrompt.UserPrompt ?? string.Empty;
                var fullyFormedUserPrompt = _aiService.ProcessPlaceholders(userPromptTemplate, promptJson);

                _logger.LogInformation($"📝 [OPPORTUNITY-STATEMENT] Calling Gemini AI with {documentCount} document(s)");

                // Step 6: Build parts array for Gemini API (text + document URIs)
                var parts = new List<object>
                {
                    new { text = fullyFormedUserPrompt }
                };

                // Add each document as a fileData part
                foreach (var doc in documents)
                {
                    parts.Add(new 
                    { 
                        fileData = new
                        {
                            fileUri = doc.storagePath,
                            mimeType = doc.mimeType
                        }
                    });
                }

                // Build user content with parts array
                var userContent = new
                {
                    role = "user",
                    parts = parts.ToArray()
                };
                
                // Step 7: Call Gemini API with caching support (use opportunityId as cache key)
                var aiResponse = await _aiService.CallGeminiApi(userContent, statementPrompt, fullyFormedSystemInstructions);

                _logger.LogInformation($"📄 [OPPORTUNITY-STATEMENT] Received AI response (length: {aiResponse?.Length ?? 0} chars)");

                // Step 8: Extract markdown from Gemini response
                string statementMarkdown;
                try
                {
                    if (string.IsNullOrEmpty(aiResponse))
                    {
                        throw new InvalidOperationException("AI response is empty");
                    }
                    
                    // Log the first 500 characters of the response for debugging
                    _logger.LogDebug($"📋 [OPPORTUNITY-STATEMENT] Response preview: {aiResponse.Substring(0, Math.Min(500, aiResponse.Length))}");
                    
                    var geminiResponse = JObject.Parse(aiResponse);
                    
                    // Check for error in the response
                    var error = geminiResponse["error"];
                    if (error != null)
                    {
                        var errorMessage = error["message"]?.ToString() ?? "Unknown error";
                        var errorCode = error["code"]?.ToString() ?? "UNKNOWN";
                        _logger.LogError($"❌ [OPPORTUNITY-STATEMENT] Gemini API returned error - Code: {errorCode}, Message: {errorMessage}");
                        throw new InvalidOperationException($"Gemini API error: {errorMessage}");
                    }
                    
                    // Navigate through the JSON structure safely
                    var candidates = geminiResponse["candidates"];
                    if (candidates == null || !candidates.Any())
                    {
                        _logger.LogError($"❌ [OPPORTUNITY-STATEMENT] No candidates found in response. Response structure: {geminiResponse.ToString(Newtonsoft.Json.Formatting.None).Substring(0, Math.Min(200, geminiResponse.ToString().Length))}");
                        throw new InvalidOperationException("No candidates found in Gemini response");
                    }
                    
                    var firstCandidate = candidates[0];
                    var content = firstCandidate?["content"];
                    if (content == null)
                    {
                        _logger.LogError($"❌ [OPPORTUNITY-STATEMENT] No content found in first candidate. Candidate structure: {firstCandidate?.ToString(Newtonsoft.Json.Formatting.None)}");
                        throw new InvalidOperationException("No content found in Gemini response candidate");
                    }
                    
                    var responseParts = content["parts"];
                    if (responseParts == null || !responseParts.Any())
                    {
                        _logger.LogError($"❌ [OPPORTUNITY-STATEMENT] No parts found in content. Content structure: {content.ToString(Newtonsoft.Json.Formatting.None)}");
                        throw new InvalidOperationException("No parts found in Gemini response content");
                    }
                    
                    var textContent = responseParts[0]?["text"]?.ToString();
                    if (string.IsNullOrEmpty(textContent))
                    {
                        _logger.LogError($"❌ [OPPORTUNITY-STATEMENT] No text found in first part. Part structure: {responseParts[0]?.ToString(Newtonsoft.Json.Formatting.None)}");
                        throw new InvalidOperationException("No text content found in Gemini response");
                    }

                    _logger.LogInformation($"✅ [OPPORTUNITY-STATEMENT] Successfully extracted text content (length: {textContent.Length} chars)");

                    // Remove markdown code block wrapping if present (```markdown ... ```)
                    var markdownMatch = System.Text.RegularExpressions.Regex.Match(
                        textContent, 
                        @"```(?:markdown)?\s*\n?(.*?)\n?```", 
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );
                    
                    if (markdownMatch.Success)
                    {
                        statementMarkdown = markdownMatch.Groups[1].Value.Trim();
                        _logger.LogInformation($"📝 [OPPORTUNITY-STATEMENT] Extracted markdown from code block (length: {statementMarkdown.Length} chars)");
                    }
                    else
                    {
                        statementMarkdown = textContent.Trim();
                        _logger.LogInformation($"📝 [OPPORTUNITY-STATEMENT] Using raw text content (length: {statementMarkdown.Length} chars)");
                    }
                }
                catch (Newtonsoft.Json.JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, $"❌ [OPPORTUNITY-STATEMENT] Failed to parse JSON response. Response: {aiResponse?.Substring(0, Math.Min(1000, aiResponse?.Length ?? 0))}");
                    throw new InvalidOperationException("Failed to parse AI response as JSON", jsonEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ [OPPORTUNITY-STATEMENT] Failed to extract text from Gemini response: {ex.Message}");
                    throw new InvalidOperationException($"Failed to process AI response: {ex.Message}", ex);
                }

                // Step 9: Save the generated statement to the Opportunity entity
                var opportunity = await _context.Opportunities.FindAsync(opportunityId);
                if (opportunity != null)
                {
                    opportunity.OpportunityStatementMarkdown = statementMarkdown;
                    _context.Opportunities.Update(opportunity);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"💾 [OPPORTUNITY-STATEMENT] Saved statement to database for opportunity {opportunityId}");
                }

                stopwatch.Stop();

                _logger.LogInformation(
                    $"✅ [OPPORTUNITY-STATEMENT] Generated opportunity statement for opportunity {opportunityId} in {stopwatch.ElapsedMilliseconds}ms"
                );

                return statementMarkdown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [OPPORTUNITY-STATEMENT] Error generating opportunity statement for opportunity {opportunityId}: {ex.Message}");
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Validates whether the opportunity statement is aligned with the structured data in the opportunity record
        /// Uses Gemini AI to analyze the statement content against actual opportunity fields
        /// Returns whether the statement is aligned and specific misalignment items if not aligned
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to validate statement for</param>
        /// <param name="user">Current user context</param>
        /// <returns>Validation response with alignment status and misalignment items</returns>
        public async Task<UNOPS.PAO.Models.Opportunities.OpportunityStatementValidationResponse> ValidateOpportunityStatementAsync(int opportunityId, ClaimsPrincipal? user = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation($"🔍 [STATEMENT-VALIDATION] Starting statement validation for opportunity {opportunityId}");

                // Step 1: Get opportunity manager
                var opportunityManager = _managerWrapper.OpportunityManager as UNOPSOpportunityManager;
                if (opportunityManager == null)
                {
                    throw new InvalidOperationException("UNOPSOpportunityManager is required for statement validation");
                }

                // Step 2: Get comprehensive opportunity data using the same method as statement generation
                var opportunityDetails = await opportunityManager.GetOpportunityDetailsForAIAsync(opportunityId);
                
                if (opportunityDetails == null || !opportunityDetails.Any())
                {
                    throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
                }

                // Step 3: Get the opportunity statement markdown separately
                var opportunity = await _context.Opportunities
                    .Where(o => o.Id == opportunityId)
                    .Select(o => new { o.OpportunityStatementMarkdown })
                    .FirstOrDefaultAsync();

                if (opportunity == null || string.IsNullOrWhiteSpace(opportunity.OpportunityStatementMarkdown))
                {
                    throw new BusinessException("No opportunity statement exists to validate");
                }

                _logger.LogInformation($"📊 [STATEMENT-VALIDATION] Retrieved opportunity with statement (length: {opportunity.OpportunityStatementMarkdown.Length} chars)");

                // Step 4: Get the validation prompt
                var promptData = await _aiService.GetPromptData("opportunity_statement_validation");
                var validationPrompt = promptData.FirstOrDefault();

                if (validationPrompt == null)
                {
                    throw new InvalidOperationException("Validation prompt 'opportunity_statement_validation' not found in database");
                }

                // Step 5: Add the statement markdown to the opportunity details dictionary
                opportunityDetails["statementMarkdown"] = opportunity.OpportunityStatementMarkdown;

                var structuredDataJson = JsonConvert.SerializeObject(opportunityDetails, Formatting.Indented);
                _logger.LogInformation($"📝 [STATEMENT-VALIDATION] Prepared structured data (length: {structuredDataJson.Length} chars)");

                // Step 6: Process placeholders in system instructions and user prompt
                var systemInstructionsTemplate = validationPrompt.SystemInstructions ?? string.Empty;
                var fullyFormedSystemInstructions = _aiService.ProcessPlaceholders(systemInstructionsTemplate, structuredDataJson);
                
                var userPromptTemplate = validationPrompt.UserPrompt ?? string.Empty;
                var fullyFormedUserPrompt = _aiService.ProcessPlaceholders(userPromptTemplate, structuredDataJson);

                // Step 7: Call Gemini API for validation
                var userContent = new
                {
                    role = "user",
                    parts = new[] { new { text = fullyFormedUserPrompt } }
                };

                var aiResponse = await _aiService.CallGeminiApi(userContent, validationPrompt, fullyFormedSystemInstructions);
                _logger.LogInformation($"🤖 [STATEMENT-VALIDATION] Received AI response (length: {aiResponse?.Length ?? 0} chars)");

                // Step 8: Parse the AI response
                string validationResultJson;
                try
                {
                    if (string.IsNullOrEmpty(aiResponse))
                    {
                        throw new InvalidOperationException("AI response was null or empty");
                    }
                    
                    var geminiResponse = JObject.Parse(aiResponse);
                    
                    // Check for errors
                    var error = geminiResponse["error"];
                    if (error != null)
                    {
                        var errorMessage = error["message"]?.ToString() ?? "Unknown error";
                        var errorCode = error["code"]?.ToString() ?? "UNKNOWN";
                        _logger.LogError($"❌ [STATEMENT-VALIDATION] Gemini API returned error - Code: {errorCode}, Message: {errorMessage}");
                        throw new InvalidOperationException($"Gemini API error: {errorMessage}");
                    }
                    
                    // Extract text content from response
                    var candidates = geminiResponse["candidates"];
                    if (candidates == null || !candidates.Any())
                    {
                        throw new InvalidOperationException("No candidates found in Gemini response");
                    }
                    
                    var firstCandidate = candidates[0];
                    var content = firstCandidate?["content"];
                    if (content == null)
                    {
                        throw new InvalidOperationException("No content found in Gemini response candidate");
                    }
                    
                    var responseParts = content["parts"];
                    if (responseParts == null || !responseParts.Any())
                    {
                        throw new InvalidOperationException("No parts found in Gemini response content");
                    }
                    
                    var textContent = responseParts[0]?["text"]?.ToString();
                    if (string.IsNullOrEmpty(textContent))
                    {
                        throw new InvalidOperationException("No text content found in Gemini response");
                    }

                    validationResultJson = textContent.Trim();
                    _logger.LogInformation($"✅ [STATEMENT-VALIDATION] Extracted validation result (length: {validationResultJson.Length} chars)");

                    // Remove markdown JSON code block wrapping if present (```json ... ```)
                    var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                        validationResultJson, 
                        @"```(?:json)?\s*\n?(.*?)\n?```", 
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );
                    
                    if (jsonMatch.Success)
                    {
                        validationResultJson = jsonMatch.Groups[1].Value.Trim();
                        _logger.LogInformation($"📝 [STATEMENT-VALIDATION] Extracted JSON from code block");
                    }
                    
                    // Additional check: try to find JSON object boundaries if plain text was returned
                    if (!validationResultJson.StartsWith("{"))
                    {
                        _logger.LogWarning($"⚠️ [STATEMENT-VALIDATION] Response doesn't start with JSON. First 100 chars: {validationResultJson.Substring(0, Math.Min(100, validationResultJson.Length))}");
                        
                        // Try to find the first { and last } to extract JSON
                        var firstBrace = validationResultJson.IndexOf('{');
                        var lastBrace = validationResultJson.LastIndexOf('}');
                        
                        if (firstBrace >= 0 && lastBrace > firstBrace)
                        {
                            validationResultJson = validationResultJson.Substring(firstBrace, lastBrace - firstBrace + 1);
                            _logger.LogInformation($"📝 [STATEMENT-VALIDATION] Extracted JSON from text boundaries");
                        }
                        else
                        {
                            _logger.LogError($"❌ [STATEMENT-VALIDATION] Could not find valid JSON in response. Full response: {validationResultJson}");
                            throw new InvalidOperationException("AI response does not contain valid JSON");
                        }
                    }
                }
                catch (Newtonsoft.Json.JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, $"❌ [STATEMENT-VALIDATION] Failed to parse JSON response");
                    throw new InvalidOperationException("Failed to parse AI response as JSON", jsonEx);
                }

                // Step 9: Parse validation result into response model
                _logger.LogInformation($"🔍 [STATEMENT-VALIDATION] Attempting to deserialize JSON (length: {validationResultJson.Length})");
                
                OpportunityStatementValidationResponse? validationResult;
                try 
                {
                    validationResult = JsonConvert.DeserializeObject<OpportunityStatementValidationResponse>(validationResultJson);
                }
                catch (Newtonsoft.Json.JsonException deserializeEx)
                {
                    _logger.LogError(deserializeEx, $"❌ [STATEMENT-VALIDATION] Failed to deserialize validation result. JSON content: {validationResultJson}");
                    throw new InvalidOperationException($"Failed to deserialize AI response as OpportunityStatementValidationResponse. JSON: {validationResultJson}", deserializeEx);
                }
                
                if (validationResult == null)
                {
                    throw new InvalidOperationException("Failed to deserialize validation result");
                }

                validationResult.OpportunityId = opportunityId;

                stopwatch.Stop();

                _logger.LogInformation(
                    $"✅ [STATEMENT-VALIDATION] Validated opportunity statement for opportunity {opportunityId} in {stopwatch.ElapsedMilliseconds}ms. IsAligned: {validationResult.IsAligned}, Misalignments: {validationResult.MisalignmentItems?.Count ?? 0}"
                );

                return validationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [STATEMENT-VALIDATION] Error validating opportunity statement for opportunity {opportunityId}: {ex.Message}");
                stopwatch.Stop();
                throw;
            }
        }

        #endregion
    }


