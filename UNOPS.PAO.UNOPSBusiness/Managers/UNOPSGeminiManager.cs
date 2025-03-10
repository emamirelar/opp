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
    private readonly string _connectionString;

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
        _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
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

    // Get screen mappings by type and sort the response
    public async Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type)
    {
        return await _screenMappingRepository
            .GetAll()
            .Where(x => x.Type == type)
            .OrderBy(x => x.Order) // Sort by TableName or any other property
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
        // To add custom logic, uncomment the following code-block and edit it (example)
        /*
        if (type == 'contacts_summary') {
            // custom logic
            // ensure to return a string (json)
        }
        */
        if (mappings == null || mappings.Length == 0)
        {
            throw new InvalidOperationException("Screen mappings are missing.");
        }

        var dbProperties = typeof(AppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var selectColumns = BuildSelectColumns(mappings);
        var joinClauses = BuildJoinClauses(mappings);
        string baseTableName = mappings[0].Name;
        string baseTable = $"{_connectionString}.\"{baseTableName}\"";
        string columnWithQuotes = "\"Id\"";
        string baseTableKeyCheck = $"{baseTable}.{columnWithQuotes}";

        // Build SQL Query
        string sqlQuery = $@"
            SELECT {string.Join(", ", selectColumns)}
            FROM {baseTable}
            {string.Join(" ", joinClauses)}
            WHERE {baseTableKeyCheck} = @RecordId";

        var result = await ExecuteSqlQuery(sqlQuery, recordId);

        // Transform result into Nested JSON
        var structuredResponse = BuildNestedJson(result, mappings);

        return JsonConvert.SerializeObject(structuredResponse, Formatting.Indented);
    }

    private List<string> BuildSelectColumns(AiScreenMapping[] mappings)
    {
        var selectColumns = new List<string>();
        foreach (var mapping in mappings)
        {
            var tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.TableName, StringComparison.OrdinalIgnoreCase));
            if (tableRecord == null)
            {
                throw new InvalidOperationException($"Table '{mapping.TableName}' does not exist in the context.");
            }

            string tableWithSchema = $"{_connectionString}.\"{mapping.TableName}\"";
            var tableProperties = tableRecord.GetProperties();

            if (!selectColumns.Any(col => col.StartsWith(tableWithSchema)))
            {
                foreach (var property in tableProperties)
                {
                    var p = $"\"{property.Name}\" AS \"{mapping.TableName}_{property.Name}\"";
                    selectColumns.Add($"{tableWithSchema}.{p}");
                }
            }

            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                string relatedTableWithSchema = $"{_connectionString}.\"{mapping.RelatedEntity}\"";
                tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.RelatedEntity, StringComparison.OrdinalIgnoreCase));
                if (tableRecord == null)
                {
                    throw new InvalidOperationException($"Related entity '{mapping.RelatedEntity}' does not exist in the context.");
                }

                if (!selectColumns.Any(col => col.StartsWith(relatedTableWithSchema)))
                {
                    tableProperties = tableRecord.GetProperties();
                    foreach (var property in tableProperties)
                    {
                        var p = $"\"{property.Name}\" AS \"{mapping.RelatedEntity}_{property.Name}\"";
                        selectColumns.Add($"{relatedTableWithSchema}.{p}");
                    }
                }
            }
        }
        return selectColumns;
    }

    private List<string> BuildJoinClauses(AiScreenMapping[] mappings)
    {
        var joinClauses = new List<string>();
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                string tableWithSchema = $"{_connectionString}.\"{mapping.TableName}\"";
                string relatedTableWithSchema = $"{_connectionString}.\"{mapping.RelatedEntity}\"";
                joinClauses.Add($"LEFT JOIN {relatedTableWithSchema} ON {tableWithSchema}.\"{mapping.ComparisonKey}\" = {relatedTableWithSchema}.\"{mapping.RelatedEntityKey}\"");
            }
        }
        return joinClauses;
    }

    private async Task<List<Dictionary<string, object>>> ExecuteSqlQuery(string sqlQuery, int recordId)
    {
        var result = new List<Dictionary<string, object>>();
        using (var connection = _context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sqlQuery;
                command.CommandType = System.Data.CommandType.Text;
                var param = command.CreateParameter();
                param.ParameterName = "@RecordId";
                param.Value = recordId;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        result.Add(row);
                    }
                }
            }
        }
        return result;
    }

    private object BuildNestedJson(List<Dictionary<string, object>> result, AiScreenMapping[] mappings)
    {
        // Implement the logic to build nested JSON from the result and mappings
        // This is a placeholder implementation
        return result;
    }
}