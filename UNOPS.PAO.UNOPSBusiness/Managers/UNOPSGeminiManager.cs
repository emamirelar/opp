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

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCredential _credentials;
    private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly string _projectId;
    private readonly string _location;
    private readonly string _modelName;
    private readonly string _url;
    private readonly AppDbContext _context;

    public UNOPSGeminiManager(IMapper mapper, AppDbContext context, IConfiguration configuration)
    {
        _mapper = mapper;
        _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _credentials = GetCredentials();
        _context = context;
        _projectId = configuration.GetValue<string>("GoogleDriveSettings:ProjectId");
        _location = configuration.GetValue<string>("GoogleDriveSettings:Location");
        _modelName = configuration.GetValue<string>("GoogleDriveSettings:GeminiModelName");
        _url = $"https://{_location}-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/{_modelName}:generateContent";
    }

    // Map AiPrompt entity to AiPromptModel
    private static AiPromptModel MapEntityToAiPromptModel(AiPrompt entity, IMapper mapper)
    {
        var result = mapper.Map<AiPrompt, AiPromptModel>(entity);
        return result;
    }

    // Map AiPromptModel to AiPrompt entity
    private AiPrompt MapModelToEntity(AiPromptModel model)
    {
        var entity = _mapper.Map(model, new AiPrompt());
        return entity;
    }

    // Get prompt data by type
    public IEnumerable<AiPromptModel> GetPromptData(string type)
    {
        return _promptRepository
            .GetAll()
            .Where(x => x.Type == type)
            .Select(x => MapEntityToAiPromptModel(x, _mapper));
    }

    // Get screen mappings by type
    public async Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type)
    {
        return await _screenMappingRepository
            .GetAll()
            .Where(x => x.Type == type)
            .ToListAsync();
    }

    // Fetch result from Gemini
    public async Task<string> fetchResultFromGemini(string promptTemplate, string relatedJsonData) {
        string finalPrompt = promptTemplate.Replace("{jsonData}", relatedJsonData);
        string geminiResponse = await callGemini(finalPrompt);
        return geminiResponse;
    }

    // Call Gemini API
    public async Task<string> callGemini(string prompt)
    {
        string accessToken = await GetAccessTokenAsync();

        // Create the request
        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            }
        };

        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        string response = await CallGeminiApiAsync(_url, jsonRequest, accessToken);
        return response;
    }

    // Map GeminiProcessRequest to AiPrompt entity
    private AiPrompt MapModelToEntity(GeminiProcessRequest model)
    {
        var entity = _mapper.Map<AiPrompt>(model);
        return entity;
    }

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessRequest req)
    {
        return MapModelToEntity(req);
    }

    // Get access token for Gemini API
    static async Task<string> GetAccessTokenAsync()
    {
        GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    // Call Gemini API with the request
    static async Task<string> CallGeminiApiAsync(string url, string jsonRequest, string accessToken)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
    }

    // Get Google credentials from configuration
    private GoogleCredential GetCredentials()
    {
        var credentialParams = _configuration.GetSection("GoogleDriveSettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("GoogleDriveSettings configuration is missing.");

        var secretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveConnectionKeySecretId");
        var secretManagerProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId, secretName);
        var secretValue = secretManagerProvider.GetSecretVersion(secretName, "latest");

        return GoogleCredential.FromJson(secretValue);
    }

    // Get data based on screen mappings
    public async Task<string> GetDataBasedOnScreenMapping(string type, int recordId, AiScreenMapping[] mappings)
    {
        var dbProperties = typeof(AppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var resultData = new Dictionary<string, object>();
        string lastMainTableName = null;

        object mainTableRecord = null;

        foreach (var mapping in mappings)
        {
            if (lastMainTableName != mapping.TableName) {
                mainTableRecord = await GetMainTableRecord(recordId, mapping.TableName, dbProperties, "Id");
                resultData[mapping.TableName] = mainTableRecord;
                lastMainTableName = mapping.TableName;
            }
            // Fetch the main record only once per table
            if (mapping.ComparisonKey.Equals("Id", StringComparison.OrdinalIgnoreCase) && resultData[mapping.TableName] == null) // It is expected that ID is the primary key of a table
            {
                mainTableRecord = await GetMainTableRecord(recordId, mapping.TableName, dbProperties, mapping.ComparisonKey);
                resultData[mapping.TableName] = mainTableRecord;
            }

            // If there's a related entity, fetch it
            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                object relatedRecords = null;
                
                if (mapping.ComparisonKey.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    // If ComparisonKey is 'Id', use recordId directly
                    relatedRecords = await GetRelatedRecords(recordId, mapping, dbProperties);
                }
                else
                {
                    // Fetch the foreign key value dynamically
                    var foreignKeyValue = mainTableRecord?.GetType().GetProperty(mapping.ComparisonKey, BindingFlags.Public | BindingFlags.Instance)?.GetValue(mainTableRecord);

                    if (foreignKeyValue != null)
                    {
                        relatedRecords = await GetRelatedRecords(Convert.ToInt32(foreignKeyValue), mapping, dbProperties);
                    }
                }

                resultData[mapping.RelatedEntity] = relatedRecords;
            }
        }

        return JsonConvert.SerializeObject(resultData, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        });
    }


    // Get main table record by ID
    private async Task<object> GetMainTableRecord(int recordId, string tableName, PropertyInfo[] dbProperties, string comparisonKey)
    {
        var tableProperty = dbProperties.FirstOrDefault(p => p.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (tableProperty == null) throw new InvalidOperationException($"Table '{tableName}' not found in DbContext.");

        var entityType = tableProperty.PropertyType.GetGenericArguments().FirstOrDefault();
        if (entityType == null) throw new InvalidOperationException($"Could not determine entity type for '{tableName}'.");

        var dbSet = (IQueryable<object>)tableProperty.GetValue(_context);

        // Handle both direct (Id) and indirect (foreign key) lookups
        if (comparisonKey.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            return await _context.FindAsync(entityType, recordId);
        }
        else
        {
            var property = entityType.GetProperty(comparisonKey, BindingFlags.Public | BindingFlags.Instance);
            
            if (property == null) throw new InvalidOperationException($"Column '{comparisonKey}' not found in table '{tableName}'.");

            var parameter = Expression.Parameter(entityType, "x");
            var condition = Expression.Equal(Expression.Property(parameter, property), Expression.Constant(recordId));
            var lambda = Expression.Lambda(condition, parameter);

            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(entityType);

            var filteredQuery = whereMethod.Invoke(null, new object[] { dbSet, lambda });

            return await ((IQueryable<object>)filteredQuery).FirstOrDefaultAsync();
        }
    }


    // Get related records by foreign key
    private async Task<IEnumerable<object>> GetRelatedRecords(int relatedRecordId, AiScreenMapping mapping, PropertyInfo[] dbProperties)
    {
        var relatedTableProperty = dbProperties.FirstOrDefault(p => p.Name.Equals(mapping.RelatedEntity, StringComparison.OrdinalIgnoreCase));
        if (relatedTableProperty == null) throw new InvalidOperationException($"Table '{mapping.RelatedEntity}' not found in DbContext.");

        var relatedEntityType = relatedTableProperty.PropertyType.GetGenericArguments().FirstOrDefault();
        if (relatedEntityType == null) throw new InvalidOperationException($"Could not determine entity type for '{mapping.RelatedEntity}'.");

        var foreignKeyProperty = relatedEntityType.GetProperty(mapping.RelatedEntityKey, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (foreignKeyProperty == null) throw new InvalidOperationException($"Foreign key '{mapping.RelatedEntityKey}' not found in table '{mapping.RelatedEntity}'.");

        var parameter = Expression.Parameter(relatedEntityType, "x");
        var foreignKeyCondition = Expression.Equal(Expression.Property(parameter, foreignKeyProperty), Expression.Constant(relatedRecordId));
        var lambda = Expression.Lambda(foreignKeyCondition, parameter);

        var whereMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(relatedEntityType);

        var relatedDbSet = relatedTableProperty.GetValue(_context);
        if (relatedDbSet == null) throw new InvalidOperationException($"Could not get DbSet for '{mapping.RelatedEntity}'.");

        var queryable = whereMethod.Invoke(null, new object[] { relatedDbSet, lambda });
        return await ((IQueryable<object>)queryable).ToListAsync();
    }


    // Apply JSON conditions to the query
    /*private IQueryable<object> ApplyJsonConditions(IQueryable<object> query, string jsonCondition)
    {
        if (string.IsNullOrEmpty(jsonCondition)) return query;
        var conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonCondition);
        var parameter = Expression.Parameter(query.ElementType, "x");
        Expression finalExpression = null;

        foreach (var condition in conditions)
        {
            var property = Expression.Property(parameter, condition.Key);
            var value = Expression.Constant(condition.Value);
            var comparison = Expression.Equal(property, value);
            finalExpression = finalExpression == null ? comparison : Expression.AndAlso(finalExpression, comparison);
        }

        if (finalExpression != null)
        {
            var lambda = Expression.Lambda(finalExpression, parameter);
            query = (IQueryable<object>)typeof(Queryable).GetMethods().First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(query.ElementType)
                .Invoke(null, new object[] { query, lambda });
        }
        return query;
    }*/
}