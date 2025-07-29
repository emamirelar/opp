using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
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
    private readonly GeminiSessionService _sessionService;
    private readonly AiContextualService _aiService;
    private readonly ILogger<UNOPSGeminiManager> _logger;
    private readonly CloudRunHelper _cloudRunHelper;
    private readonly IUserManagementManager _userManagementManager;

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, HttpClient httpClient, ILogger<UNOPSGeminiManager> logger, IUserManagementManager userManagementManager)
    {
        _mapper = mapper;
        _context = context;
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _logger = logger;
        _userManagementManager = userManagementManager;
        
        // Initialize CloudRunHelper internally
        var cloudRunHelperLogger = new LoggerFactory().CreateLogger<CloudRunHelper>();
        _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, GetCredentials());
        
        _credentials = GetCredentials()
                        .CreateScoped("https://www.googleapis.com/auth/spreadsheets.readonly");
        _textExtractionService = new TextExtractionService();
        _gcsService = new GoogleCloudStorageService(configuration);
        _sessionService = new GeminiSessionService(context, httpClient, configuration);
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
            var promptData = (await GetPromptData(type)).FirstOrDefault();

            if (promptData == null)
            {
                return "";
            }

            // Fetch result from Gemini
            return await FetchResultFromGemini(promptData, extractedText);
        }

        return extractedText;
    }

    public async Task<SessionWithChats> GetSessionDataWithChats(string sessionId, int userId) 
    {
        return await _sessionService.GetSessionDataWithChats(sessionId, userId);
    }

    public async Task<IEnumerable<AiChatSession>> GetSessionData(string sessionId, int userId) 
    {
        return await _sessionService.GetSessionData(sessionId, userId);
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
            
            var apiUrl = $"/apps/{appName}/users/{userId}/sessions";
            
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
                            LastUpdated = DateTime.UtcNow,
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
                            LastUpdated = DateTime.UtcNow,
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
        return await _sessionService.UpdateAiAssistantAccessibility(req);
    }

    public async Task<bool> UpdateSessionStar(string sessionId, bool starred)
    {
        return await _sessionService.UpdateSessionStar(sessionId, starred);
    }

    public async Task<bool> UpdateSessionArchive(string sessionId, bool archived)
    {
        return await _sessionService.UpdateSessionArchive(sessionId, archived);
    }

    public async Task<bool> UpdateSessionTitle(string sessionId, string title)
    {
        return await _sessionService.UpdateSessionTitle(sessionId, title);
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
        var promptData = (await GetPromptData(req.Type)).FirstOrDefault();
        if (promptData == null)
        {
            return null;
        }

        var fileData = await _aiService.ReadFileData(req.FileId);
        var fileDataArray = JArray.Parse(fileData);

        // Check if we should process asynchronously
        if (fileDataArray.Count > 100)
        {
            var message = new MyPubSubMessage
            {
                MessageType = "BulkImport",
                EntityName = req.Type,
                PromptType = promptData.Type,
                BatchData = JsonConvert.SerializeObject(fileDataArray.ToObject<List<object>>()), // Convert to JSON string
                UserId = currentUserId
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
                req.Type,
                false
            );

            return new
            {
                Message = "Processing completed successfully",
                Entity = req.Type,
                Intent = "Success",
                Records = JsonConvert.SerializeObject(finalResponse)
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
                if (idValue != null && idValue is int id && id > 0)
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
        var aiChatRequest = new AiChatRequest
        {
            AppName = appName,
            UserId = currentUserId.ToString(),
            UserEmail = currentUserEmail,
            SessionId = req.sessionId?.ToString() ?? "",
            Message = req.Message ?? "",
            Streaming = false,
            State = req.State
        };

        var apiUrl = $"/chat";
        var jsonContent = System.Text.Json.JsonSerializer.Serialize(aiChatRequest);

        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var httpClient = await _cloudRunHelper.CreateAuthenticatedHttpClientForUrl(serviceUrl);

        var response = await httpClient.PostAsync(apiUrl, httpContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AI service call failed. Status: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

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

    private static bool IsRestrictedHeader(string headerName)
    {
        // List of headers that should not be forwarded or are set automatically by HttpClient
        var restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Content-Length",
            "Content-Type",
            "Host",
            "Connection",
            "Transfer-Encoding",
            "Expect",
            "If-Modified-Since",
            "Range",
            "Referer",
            "User-Agent"
        };
        
        return restrictedHeaders.Contains(headerName);
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
}