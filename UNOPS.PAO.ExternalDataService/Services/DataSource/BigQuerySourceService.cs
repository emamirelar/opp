using Google.Cloud.BigQuery.V2;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;

namespace UNOPS.PAO.ExternalDataService.Services.DataSource;

public class BigQuerySourceService : IDataSourceService
{
    private readonly ILogger<BigQuerySourceService> _logger;
    private readonly IConfiguration _configuration;

    public BigQuerySourceService(ILogger<BigQuerySourceService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ExternalDataRecord>> ExtractDataAsync(
        SourceConfiguration sourceConfig, 
        DateTime? lastSyncDate = null)
    {
        var client = await CreateBigQueryClientAsync(sourceConfig.Connection);
        
        var query = ProcessQueryParameters(sourceConfig.Query, lastSyncDate);
        _logger.LogInformation("Executing BigQuery: {Query}", query);
        
        var queryJob = await client.CreateQueryJobAsync(query, parameters: null);
        var result = await queryJob.GetQueryResultsAsync();

        var records = new List<ExternalDataRecord>();
        
        await foreach (var row in result.GetRowsAsync())
        {
            var record = new ExternalDataRecord
            {
                PrimaryKey = row[sourceConfig.PrimaryKeyField]?.ToString() ?? string.Empty,
                Data = new Dictionary<string, object?>(),
                SourceSystem = "BigQuery"
            };

            foreach (var column in result.Schema.Fields)
            {
                var columnType = ParseBigQueryType(column.Type?.ToString());
                var value = ConvertBigQueryValue(row[column.Name], columnType);
                record.Data[column.Name] = value;
            }

            // Set last modified if incremental field is specified
            if (!string.IsNullOrEmpty(sourceConfig.IncrementalField) && 
                record.Data.ContainsKey(sourceConfig.IncrementalField))
            {
                if (DateTime.TryParse(record.Data[sourceConfig.IncrementalField]?.ToString(), out var lastModified))
                {
                    record.LastModified = lastModified;
                }
            }

            records.Add(record);
        }

        _logger.LogInformation("Extracted {Count} records from BigQuery", records.Count);
        return records;
    }

    public async Task<bool> TestConnectionAsync(SourceConfiguration sourceConfig)
    {
        try
        {
            var client = await CreateBigQueryClientAsync(sourceConfig.Connection);
            
            // Simple test query
            var testQuery = "SELECT 1 as test_column LIMIT 1";
            var queryJob = await client.CreateQueryJobAsync(testQuery, parameters: null);
            var result = await queryJob.GetQueryResultsAsync();
            
            await foreach (var row in result.GetRowsAsync())
            {
                return true; // If we can read at least one row, connection is good
            }
            
            return true;
        }
        catch (Exception ex)
        {
            var projectId = GetResolvedProjectId(sourceConfig.Connection);
            _logger.LogError(ex, "BigQuery connection test failed for project {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<DataSchema> GetSourceSchemaAsync(SourceConfiguration sourceConfig)
    {
        try
        {
            var client = await CreateBigQueryClientAsync(sourceConfig.Connection);
            
            // Execute the query with LIMIT 0 to get schema without data
            var schemaQuery = $"SELECT * FROM ({sourceConfig.Query}) LIMIT 0";
            var queryJob = await client.CreateQueryJobAsync(schemaQuery, parameters: null);
            var result = await queryJob.GetQueryResultsAsync();

            var schema = new DataSchema();
            
            foreach (var field in result.Schema.Fields)
            {
                schema.Fields[field.Name] = new FieldSchema
                {
                    Name = field.Name,
                    DataType = MapBigQueryTypeToStandard(ParseBigQueryType(field.Type?.ToString())),
                    IsRequired = field.Mode?.ToString() == "REQUIRED",
                    IsNullable = field.Mode?.ToString() == "NULLABLE"
                };
            }

            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get source schema from BigQuery");
            throw;
        }
    }

    private Task<BigQueryClient> CreateBigQueryClientAsync(BigQueryConnection connection)
    {
        try
        {
            // Resolve project ID: use connection config first, then fall back to appsettings
            var projectId = GetResolvedProjectId(connection);

            if (connection.UseEnvironmentAuth)
            {
                _logger.LogDebug("Using environment authentication for BigQuery project {ProjectId}", projectId);
                return Task.FromResult(BigQueryClient.Create(projectId));
            }
            else if (!string.IsNullOrEmpty(connection.CredentialsFile))
            {
                if (!File.Exists(connection.CredentialsFile))
                {
                    throw new FileNotFoundException($"BigQuery credentials file not found: {connection.CredentialsFile}");
                }

                _logger.LogDebug("Using credentials file for BigQuery: {CredentialsFile}", connection.CredentialsFile);
                var credential = GoogleCredential.FromFile(connection.CredentialsFile);
                return Task.FromResult(BigQueryClient.Create(projectId, credential));
            }
            else
            {
                throw new InvalidOperationException("No valid authentication method configured for BigQuery");
            }
        }
        catch (Exception ex)
        {
            var projectId = GetResolvedProjectId(connection);
            _logger.LogError(ex, "Failed to create BigQuery client for project {ProjectId}", projectId);
            throw;
        }
    }

    private string GetResolvedProjectId(BigQueryConnection connection)
    {
        // Priority: 1. Connection config, 2. Appsettings fallback
        if (!string.IsNullOrEmpty(connection.ProjectId))
        {
            _logger.LogDebug("Using project ID from sync configuration: {ProjectId}", connection.ProjectId);
            return connection.ProjectId;
        }

        var fallbackProjectId = _configuration["BigQuery:ProjectId"];
        if (!string.IsNullOrEmpty(fallbackProjectId))
        {
            _logger.LogDebug("Using fallback project ID from appsettings: {ProjectId}", fallbackProjectId);
            return fallbackProjectId;
        }

        throw new InvalidOperationException(
            "BigQuery project ID not specified in sync configuration and no fallback project ID found in appsettings.BigQuery.ProjectId");
    }

    private string ProcessQueryParameters(string query, DateTime? lastSyncDate)
    {
        var processedQuery = query;
        
        if (lastSyncDate.HasValue && query.Contains("@last_sync_date"))
        {
            var parameterValue = lastSyncDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            processedQuery = processedQuery.Replace("@last_sync_date", $"'{parameterValue}'");
            _logger.LogDebug("Replaced @last_sync_date parameter with: {ParameterValue}", parameterValue);
        }
        else if (query.Contains("@last_sync_date"))
        {
            // If no last sync date available, use a very old date to get all records
            var fallbackDate = new DateTime(1900, 1, 1).ToString("yyyy-MM-dd HH:mm:ss");
            processedQuery = processedQuery.Replace("@last_sync_date", $"'{fallbackDate}'");
            _logger.LogDebug("No last sync date available, using fallback date: {FallbackDate}", fallbackDate);
        }

        return processedQuery;
    }

    private object? ConvertBigQueryValue(object? value, BigQueryDbType type)
    {
        if (value == null) return null;

        try
        {
            return type switch
            {
                BigQueryDbType.String => value.ToString(),
                BigQueryDbType.Int64 => Convert.ToInt64(value),
                BigQueryDbType.Float64 => Convert.ToDouble(value),
                BigQueryDbType.Bool => Convert.ToBoolean(value),
                BigQueryDbType.DateTime => Convert.ToDateTime(value),
                BigQueryDbType.Date => DateOnly.FromDateTime(Convert.ToDateTime(value)),
                BigQueryDbType.Time => TimeOnly.FromTimeSpan(TimeSpan.Parse(value.ToString()!)),
                BigQueryDbType.Timestamp => Convert.ToDateTime(value),
                BigQueryDbType.Numeric => Convert.ToDecimal(value),
                BigQueryDbType.Bytes => value, // Keep as-is for binary data
                BigQueryDbType.Geography => value.ToString(), // Convert to string for now
                BigQueryDbType.Json => value.ToString(), // Keep JSON as string
                _ => value.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert BigQuery value {Value} of type {Type}, returning as string", 
                value, type);
            return value.ToString();
        }
    }

    private BigQueryDbType ParseBigQueryType(string? typeString)
    {
        return (typeString?.ToUpper()) switch
        {
            "STRING" => BigQueryDbType.String,
            "INT64" or "INTEGER" => BigQueryDbType.Int64,
            "FLOAT64" or "FLOAT" => BigQueryDbType.Float64,
            "BOOL" or "BOOLEAN" => BigQueryDbType.Bool,
            "DATETIME" => BigQueryDbType.DateTime,
            "DATE" => BigQueryDbType.Date,
            "TIME" => BigQueryDbType.Time,
            "TIMESTAMP" => BigQueryDbType.Timestamp,
            "NUMERIC" or "DECIMAL" => BigQueryDbType.Numeric,
            "BYTES" => BigQueryDbType.Bytes,
            "GEOGRAPHY" => BigQueryDbType.Geography,
            "JSON" => BigQueryDbType.Json,
            _ => BigQueryDbType.String
        };
    }

    private string MapBigQueryTypeToStandard(BigQueryDbType type)
    {
        return type switch
        {
            BigQueryDbType.String => "varchar",
            BigQueryDbType.Int64 => "bigint",
            BigQueryDbType.Float64 => "double precision",
            BigQueryDbType.Bool => "boolean",
            BigQueryDbType.DateTime => "timestamp",
            BigQueryDbType.Date => "date",
            BigQueryDbType.Time => "time",
            BigQueryDbType.Timestamp => "timestamp",
            BigQueryDbType.Numeric => "decimal",
            BigQueryDbType.Bytes => "bytea",
            BigQueryDbType.Geography => "text",
            BigQueryDbType.Json => "jsonb",
            _ => "text"
        };
    }
}
