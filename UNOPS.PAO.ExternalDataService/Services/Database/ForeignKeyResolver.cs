using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;
using UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;

namespace UNOPS.PAO.ExternalDataService.Services.Database;

public class ForeignKeyResolver : IForeignKeyResolver
{
    private readonly IApplicationDatabaseService _databaseService;
    private readonly ILogger<ForeignKeyResolver> _logger;
    private readonly SqlHelper _sqlHelper;
    private readonly Dictionary<string, Dictionary<string, int?>> _cache = new();
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
    private const string DefaultSchema = "public";

    public ForeignKeyResolver(
        IApplicationDatabaseService databaseService,
        ILogger<ForeignKeyResolver> logger,
        SqlHelper sqlHelper)
    {
        _databaseService = databaseService;
        _logger = logger;
        _sqlHelper = sqlHelper;
    }

    public async Task<Dictionary<string, int?>> ResolveForeignKeysAsync(
        ForeignKeyMapping mapping, 
        IEnumerable<string> sourceValues)
    {
        var result = new Dictionary<string, int?>();
        var distinctValues = sourceValues.Distinct().Where(v => !string.IsNullOrEmpty(v)).ToList();

        if (!distinctValues.Any())
            return result;

        _logger.LogDebug("Starting FK resolution for {LookupTable}.{LookupField} with source values of types: {ValueTypes}", 
            mapping.LookupTable, mapping.LookupField, string.Join(", ", distinctValues.Take(3).Select(v => $"'{v}' ({v.GetType().Name})")));

        try
        {
            // Check cache first
            var cacheKey = GetCacheKey(mapping);
            await _cacheSemaphore.WaitAsync();
            
            try
            {
                if (_cache.ContainsKey(cacheKey))
                {
                    var cachedResults = _cache[cacheKey];
                    var uncachedValues = distinctValues.Where(v => !cachedResults.ContainsKey(v)).ToList();
                    
                    // Return cached results for values we have
                    foreach (var value in distinctValues.Where(v => cachedResults.ContainsKey(v)))
                    {
                        result[value] = cachedResults[value];
                    }

                    // Only query for uncached values
                    distinctValues = uncachedValues;
                }
                else
                {
                    _cache[cacheKey] = new Dictionary<string, int?>();
                }
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            if (!distinctValues.Any())
                return result; // All values were cached

            // Build dynamic query to lookup FK values with schema qualification
            var qualifiedTableName = _sqlHelper.GetFullTableName(mapping.LookupTable, DefaultSchema);
            
            // Always cast the lookup field to text and compare with string parameters
            // This handles data type mismatches safely (integer field vs string source values)
            var sql = $@"
                SELECT ""{mapping.LookupField}"" as lookup_value, ""{mapping.LookupReturnField}"" as id 
                FROM {qualifiedTableName} 
                WHERE ""{mapping.LookupField}""::text = ANY(@values)";

            _logger.LogDebug("Executing FK lookup query with text casting: {Query} with {ValueCount} values: [{SampleValues}]", 
                sql, distinctValues.Count, string.Join(", ", distinctValues.Take(5)));

            var lookupResults = new List<ForeignKeyLookupResult>();
            
            try
            {
                using var connection = await _databaseService.GetConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                
                // Always use string array parameters with text casting
                var parameter = command.CreateParameter();
                parameter.ParameterName = "values";
                parameter.Value = distinctValues.ToArray();
                command.Parameters.Add(parameter);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var lookupValue = reader["lookup_value"];
                    var idValue = reader["id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["id"]);
                    
                    _logger.LogDebug("FK lookup result: lookup_value='{LookupValue}' (type: {LookupValueType}), id={Id}", 
                        lookupValue, lookupValue?.GetType().Name, idValue);
                    
                    lookupResults.Add(new ForeignKeyLookupResult
                    {
                        LookupValue = lookupValue,
                        Id = idValue
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error during FK lookup for {Schema}.{LookupTable}.{LookupField}", 
                    DefaultSchema, mapping.LookupTable, mapping.LookupField);
                throw;
            }

            // Map results back to source values
            var newCacheEntries = new Dictionary<string, int?>();
            
            foreach (var sourceValue in distinctValues)
            {
                var lookup = lookupResults.FirstOrDefault(r => 
                    string.Equals(r.LookupValue?.ToString(), sourceValue, StringComparison.OrdinalIgnoreCase));
                
                var resolvedId = lookup?.Id;
                result[sourceValue] = resolvedId;
                newCacheEntries[sourceValue] = resolvedId;

                if (lookup == null)
                {
                    _logger.LogWarning("Foreign key lookup failed for {Schema}.{LookupTable}.{LookupField} = '{SourceValue}'", 
                        DefaultSchema, mapping.LookupTable, mapping.LookupField, sourceValue);
                }
            }

            // Update cache
            await _cacheSemaphore.WaitAsync();
            try
            {
                foreach (var entry in newCacheEntries)
                {
                    _cache[cacheKey][entry.Key] = entry.Value;
                }
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            _logger.LogDebug("Resolved {ResolvedCount}/{TotalCount} foreign keys for {Schema}.{LookupTable}", 
                result.Count(r => r.Value.HasValue), distinctValues.Count, DefaultSchema, mapping.LookupTable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving foreign keys for {Schema}.{LookupTable}.{LookupField}", 
                DefaultSchema, mapping.LookupTable, mapping.LookupField);
                
            // Return null results for all values on error
            foreach (var sourceValue in distinctValues)
            {
                result[sourceValue] = null;
            }
        }

        return result;
    }

    public async Task<int?> ResolveSingleForeignKeyAsync(
        ForeignKeyMapping mapping, 
        string sourceValue)
    {
        if (string.IsNullOrEmpty(sourceValue))
            return null;

        var results = await ResolveForeignKeysAsync(mapping, new[] { sourceValue });
        return results.GetValueOrDefault(sourceValue);
    }

    public async Task<bool> ValidateForeignKeyMappingAsync(ForeignKeyMapping mapping)
    {
        try
        {
            // Check if the lookup table exists (with schema qualification)
            var tableExistsQuery = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_name = @tableName 
                AND table_schema = @schemaName";

            var tableExists = await _databaseService.ExecuteScalarAsync<long>(
                tableExistsQuery, 
                new { tableName = mapping.LookupTable, schemaName = DefaultSchema }
            );

            if (tableExists == 0)
            {
                _logger.LogError("Lookup table '{LookupTable}' does not exist in schema '{Schema}' for foreign key mapping: " +
                    "source_field='{SourceField}' -> destination_field='{DestinationField}'", 
                    mapping.LookupTable, DefaultSchema, mapping.SourceField, mapping.DestinationField);
                return false;
            }

            // Check if the lookup field exists
            var fieldExistsQuery = @"
                SELECT COUNT(*) 
                FROM information_schema.columns 
                WHERE table_name = @tableName 
                AND table_schema = @schemaName
                AND column_name = @columnName";

            var fieldExists = await _databaseService.ExecuteScalarAsync<long>(
                fieldExistsQuery,
                new { tableName = mapping.LookupTable, schemaName = DefaultSchema, columnName = mapping.LookupField }
            );

            if (fieldExists == 0)
            {
                _logger.LogError("Lookup field '{LookupField}' does not exist in table '{LookupTable}.{Schema}' for foreign key mapping: " +
                    "source_field='{SourceField}' -> destination_field='{DestinationField}'. Available columns can be checked using: SELECT column_name FROM information_schema.columns WHERE table_name='{LookupTable}' AND table_schema='{Schema}'", 
                    mapping.LookupField, mapping.LookupTable, DefaultSchema, mapping.SourceField, mapping.DestinationField, mapping.LookupTable, DefaultSchema);
                return false;
            }

            // Check if the return field exists
            var returnFieldExists = await _databaseService.ExecuteScalarAsync<long>(
                fieldExistsQuery,
                new { tableName = mapping.LookupTable, schemaName = DefaultSchema, columnName = mapping.LookupReturnField }
            );

            if (returnFieldExists == 0)
            {
                _logger.LogError("Return field '{LookupReturnField}' does not exist in table '{LookupTable}.{Schema}' for foreign key mapping: " +
                    "source_field='{SourceField}' -> destination_field='{DestinationField}'. Available columns can be checked using: SELECT column_name FROM information_schema.columns WHERE table_name='{LookupTable}' AND table_schema='{Schema}'", 
                    mapping.LookupReturnField, mapping.LookupTable, DefaultSchema, mapping.SourceField, mapping.DestinationField, mapping.LookupTable, DefaultSchema);
                return false;
            }

            _logger.LogDebug("Foreign key mapping validation passed for {Schema}.{LookupTable}.{LookupField} -> {LookupReturnField}", 
                DefaultSchema, mapping.LookupTable, mapping.LookupField, mapping.LookupReturnField);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating foreign key mapping for {Schema}.{LookupTable}.{LookupField}", 
                DefaultSchema, mapping.LookupTable, mapping.LookupField);
            return false;
        }
    }

    private string GetCacheKey(ForeignKeyMapping mapping)
    {
        return $"{mapping.LookupTable}.{mapping.LookupField}->{mapping.LookupReturnField}";
    }

    public void ClearCache()
    {
        _cacheSemaphore.Wait();
        try
        {
            _cache.Clear();
            _logger.LogInformation("Foreign key resolver cache cleared");
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public void ClearCache(ForeignKeyMapping mapping)
    {
        var cacheKey = GetCacheKey(mapping);
        _cacheSemaphore.Wait();
        try
        {
            if (_cache.ContainsKey(cacheKey))
            {
                _cache.Remove(cacheKey);
                _logger.LogDebug("Cleared cache for foreign key mapping: {CacheKey}", cacheKey);
            }
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cacheSemaphore?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
