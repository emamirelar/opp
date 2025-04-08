using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using Google.Api.Gax.ResourceNames;
using Value = Google.Protobuf.WellKnownTypes.Value;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Npgsql;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Globalization;
using UNOPS.PAO.Domain.Entities;
using System.Reflection;
using Newtonsoft.Json;
using UNOPS.PAO.Business.Repositories.Generic;
using System.Data;
using Humanizer;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class AiContextualService
{
    private readonly PredictionServiceClient _predictionClient;
    private readonly string _endpoint;
    private readonly IConfiguration _configuration;
    public readonly UNOPSAppDbContext _context;
    private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
    private readonly string _connectionString;

    private readonly GoogleCredential _credentials;

    public AiContextualService(IConfiguration configuration, UNOPSAppDbContext context, GoogleCredential credentials)
    {
        _configuration = configuration;
        _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
        var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
        var location = _configuration.GetValue<string>("AISettings:Location");
        var model = _configuration.GetValue<string>("AISettings:EmbeddingModelName");
        _endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";
        _predictionClient = PredictionServiceClient.Create(); // gRPC Client
        _context = context;
        _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
        _credentials = credentials;
    }

    public async Task<string> CreateEmbeddingForText(string text)
    {
        // Create the request instance
        // Create structured instance in JSON format
        var instance = new Value
        {
            StructValue = new Struct
            {
                Fields = { { "content", Value.ForString(text) } }
            }
        };
        var request = new PredictRequest
        {
            Endpoint = _endpoint,
            Instances = { instance }
        };

        // Call Vertex AI using gRPC
        PredictResponse response = await _predictionClient.PredictAsync(request);

        var structValue = response.Predictions[0].StructValue;

        // Navigate to the "values" inside "embeddings"
        var embeddingValues = structValue.Fields["embeddings"].StructValue.Fields["values"].ListValue.Values;

        // Convert to float array
        float[] embedding = embeddingValues.Select(v => (float)v.NumberValue).ToArray();

        string vectorString = "[" + string.Join(",", embedding.Select(f => f.ToString(CultureInfo.InvariantCulture))) + "]";

        return vectorString;
    }

    public async Task PersistEmbedding(string entityName, int entityId, string vectorString)
    {
        var sql = "CALL public.\"InsertEntityEmbedding\"(@entityName, @entityId, @embedding)";

        var parameters = new[] 
        {
            new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
            new NpgsqlParameter("@entityId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = entityId },
            new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorString }
        };

        // Execute the stored procedure using ExecuteSqlRaw
        await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        //return embedding;
    }

    public async Task GenerateEmbeddingAsync(string entityName, int entityId, string text)
    {
        var vectorString = await CreateEmbeddingForText(text);

        await PersistEmbedding(entityName, entityId, vectorString);
    }

    public async Task<string> RetrieveContent(string promptType, dynamic entityId)
    {
        
        var content = "No content found";

        if (entityId is not int)
            return content;

        // Query the AiScreenMapping table based on Type
        var screenMappings = (await GetScreenMappingsByType(promptType)).ToArray();
        content = await GetDataBasedOnScreenMapping(promptType, (int)entityId, screenMappings);

        return content;
    }

    public async Task<dynamic> RetrieveEntityId(string entityName, string vectorEmbedding)
    {
        var sql = "SELECT public.RetrieveSimilarityId(@entityName, @embedding)";

        entityName = entityName.Pluralize();

        var parameters = new[] 
        {
            new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
            new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorEmbedding }
        };

        // Execute the stored procedure using ExecuteSqlRaw
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);

        var result = await command.ExecuteScalarAsync();

        return result;
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

    // Build select columns for SQL query
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

    // Build join clauses for SQL query
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

    // Execute SQL query
    private async Task<List<Dictionary<string, object>>> ExecuteSqlQuery(string sqlQuery, int recordId)
    {
        var result = new List<Dictionary<string, object>>();
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
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
        return result;
    }

    // Build nested JSON from result and mappings
    private object BuildNestedJson(List<Dictionary<string, object>> result, AiScreenMapping[] mappings)
    {
        // Implement the logic to build nested JSON from the result and mappings
        // This is a placeholder implementation
        return result;
    }

    public async Task<string> ReadFileData(string fileId)
    {
        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = _credentials,
            ApplicationName = "GoogleSheetsReader",
        });

        // Read values
        var request = service.Spreadsheets.Values.Get(fileId, "Sheet1");
        ValueRange response = await request.ExecuteAsync();
        var data = string.Empty;

        if (response.Values != null && response.Values.Count > 0)
        {
            // Convert response.Values to a stringified array
            data = JsonConvert.SerializeObject(response.Values);
        }

        return data;
    }
   /* public async Task<List<object>> GetAllEntityDataAsync(string entityName)
    {
        var dbSetProperty = _context.GetType()
            .GetProperties()
            .FirstOrDefault(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                string.Equals(p.Name, entityName, StringComparison.OrdinalIgnoreCase));

        if (dbSetProperty == null)
            throw new Exception($"No DbSet found for entity name '{entityName}'");

        var dbSet = dbSetProperty.GetValue(_context);
        var toListAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethod("ToListAsync", new[] { typeof(IQueryable<>), typeof(CancellationToken) })
            ?.MakeGenericMethod(dbSetProperty.PropertyType.GenericTypeArguments[0]);

        if (toListAsyncMethod == null)
            throw new Exception("Couldn't find ToListAsync method.");

        var result = await (Task)toListAsyncMethod.Invoke(
            null,
            new object[] { dbSet, CancellationToken.None });

        return ((IEnumerable<object>)((dynamic)result)).ToList();
    }*/


}