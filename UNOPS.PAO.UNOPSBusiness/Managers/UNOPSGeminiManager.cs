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

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCredential _credentials;
    private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly UNOPSAppDbContext _context;
    private readonly string _connectionString;

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        _mapper = mapper;
        _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _credentials = GetCredentials();
        _context = context;
        _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
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
    public async Task<string> fetchResultFromGemini(AiPromptModel promptData, string relatedJsonData) {
        string promptTemplate = promptData.Prompt;
        string finalPrompt = promptTemplate.Replace("{jsonData}", relatedJsonData);
        string geminiResponse = await callGemini(finalPrompt, promptData);
        return geminiResponse;
    }

    // Call Gemini API
    public async Task<string> callGemini(string prompt, AiPromptModel promptData)
    {
        string accessToken = await GetAccessTokenAsync();
        var requestBody = await GetRequestBody(prompt, promptData);
        string url = await GetURL(promptData);
        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        string response = await CallGeminiApiAsync(url, jsonRequest, accessToken);
        return response;
    }

    public async Task<dynamic> GetRequestBody(string prompt, AiPromptModel promptData)
    {
        dynamic contentConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.ContentConfig);
        dynamic generationConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.GenerationConfig);

        contentConfig.parts[0].text = prompt;

        var requestBody = new
        {
            contents = new[] { contentConfig },
            generationConfig = generationConfig
        };

        return requestBody;
    }

    public async Task<string> GetURL(AiPromptModel promptData)
    {
        return $"https://{promptData.Location}-aiplatform.googleapis.com/v1/projects/{promptData.Project}/locations/{promptData.Location}/publishers/google/models/{promptData.Model}:generateContent";
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

        var dbProperties = typeof(UNOPSAppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var selectColumns = BuildSelectColumns(mappings);
        var joinClauses = BuildJoinClauses(mappings);
        string baseTableName = mappings[0].Name;
        string baseTable = $"{_connectionString}.\"{baseTableName}\"";
        string columnWithQuotes = "\"Id\"";
        string baseTableKeyCheck = $"{baseTable}.{columnWithQuotes}";

        string sqlQuery = $@"
            SELECT ROW_TO_JSON(t)
            FROM (
                SELECT {string.Join(", ", selectColumns)}
                FROM {baseTable}
                {string.Join(" ", joinClauses)}
                WHERE {baseTableKeyCheck} = @RecordId
            ) t;";

        var result = await ExecuteSqlQuery(sqlQuery, recordId);
        return JsonConvert.SerializeObject(result[0]?["row_to_json"], Formatting.Indented);
    }

    private List<string> BuildSelectColumns(AiScreenMapping[] mappings)
    {
        var selectColumns = new List<string>();
        var aggregates = new List<string>();
        foreach (var mapping in mappings)
        {
            var tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.TableName, StringComparison.OrdinalIgnoreCase));
            var foreignKeys = tableRecord?.GetForeignKeys()?.ToList();
            var primaryKey = tableRecord?.FindPrimaryKey();

            string tableWithSchema = $"{_connectionString}.\"{mapping.TableName}\"";
            var tableProperties = tableRecord?.GetProperties();
            var tablePropetiesAsList = tableProperties?.ToList();
            string aggregation = $"JSON_AGG(DISTINCT JSONB_BUILD_OBJECT(";

            if (!selectColumns.Any(col => col.Contains(tableWithSchema)) && (tableRecord?.ClrType != null && tablePropetiesAsList.Count != 2 && foreignKeys.Count != 2))
            {
                foreach (var property in tableProperties)
                {
                    var commaSeparation = string.Empty;
                    if (tableProperties.First().Name != property.Name)
                    {
                        commaSeparation = ",";
                    }
                    aggregation = string.Concat(aggregation, $"{commaSeparation} '{property.Name}', ", $"{tableWithSchema}.\"{property.Name}\"");
                }
                if (!string.IsNullOrEmpty(aggregation))
                {
                    aggregation = string.Concat(aggregation, $")) AS {mapping.TableName}");
                    selectColumns.Add(aggregation);
                }
            }


            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                aggregation = $"JSON_AGG(DISTINCT JSONB_BUILD_OBJECT(";
                string relatedTableWithSchema = $"{_connectionString}.\"{mapping.RelatedEntity}\"";
                tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.RelatedEntity, StringComparison.OrdinalIgnoreCase));
                tableProperties = tableRecord?.GetProperties();
                tablePropetiesAsList = tableProperties?.ToList();
                foreignKeys = tableRecord?.GetForeignKeys()?.ToList();
                primaryKey = tableRecord?.FindPrimaryKey();

                if (!selectColumns.Any(col => col.Contains(relatedTableWithSchema)) && (tableRecord?.ClrType != null && tablePropetiesAsList.Count != 2 && foreignKeys.Count != 2))
                {
                    foreach (var property in tableProperties)
                    {
                        var commaSeparation = string.Empty;
                        if (tableProperties.First().Name != property.Name)
                        {
                            commaSeparation = ",";
                        }
                        aggregation = string.Concat(aggregation, $"{commaSeparation} '{property.Name}', ", $"{relatedTableWithSchema}.\"{property.Name}\"");
                    }
                    if (!string.IsNullOrEmpty(aggregation))
                    {
                        aggregation = string.Concat(aggregation, $")) AS {mapping.RelatedEntity}");
                        selectColumns.Add(aggregation);
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