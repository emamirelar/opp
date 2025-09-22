using System.Text;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;
using UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;
using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;
using UNOPS.PAO.ExternalDataService.Models.Sync;
using UNOPS.PAO.ExternalDataService.Services.Database;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public class SyncProcessor : ISyncProcessor
{
    private readonly IApplicationDatabaseService _databaseService;
    private readonly IForeignKeyResolver _fkResolver;
    private readonly TransformationEngine _transformationEngine;
    private readonly ISyncLoggingService _syncLoggingService;
    private readonly ILogger<SyncProcessor> _logger;

    public SyncProcessor(
        IApplicationDatabaseService databaseService,
        IForeignKeyResolver fkResolver,
        TransformationEngine transformationEngine,
        ISyncLoggingService syncLoggingService,
        ILogger<SyncProcessor> logger)
    {
        _databaseService = databaseService;
        _fkResolver = fkResolver;
        _transformationEngine = transformationEngine;
        _syncLoggingService = syncLoggingService;
        _logger = logger;
    }

    public async Task<SyncBatchResult> ProcessBatchAsync(
        IEnumerable<ExternalDataRecord> records, 
        SyncConfiguration configuration, 
        string batchId)
    {
        var result = new SyncBatchResult();
        var transformedRecords = new List<ExternalDataRecord>();

        _logger.LogDebug("Processing batch {BatchId} with {RecordCount} records", batchId, records.Count());

        // Step 1: Resolve foreign keys BEFORE transformation (using original source data)
        var originalRecords = records.ToList();
        var fkStartTime = DateTime.UtcNow;
        await ResolveForeignKeysAsync(originalRecords, configuration.Destination.ForeignKeyMappings, result);
        result.ForeignKeyResolutionDuration = DateTime.UtcNow - fkStartTime;

        // Step 2: Transform records and handle validation errors (preserving resolved FK values)
        foreach (var record in originalRecords)
        {
            try
            {
                var transformedRecord = await TransformRecordAsync(record, configuration.Destination);
                transformedRecords.Add(transformedRecord);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to transform record with key {RecordKey}", record.PrimaryKey);
                result.Errors.Add(new SyncError
                {
                    Message = $"Record transformation failed: {ex.Message}",
                    Code = "TRANSFORM_ERROR",
                    Exception = ex,
                    RecordKey = record.PrimaryKey
                });
                result.RecordsFailed++;
            }
        }

        // Step 3: Perform database operations (with timing)
        if (transformedRecords.Any())
        {
            var dbStartTime = DateTime.UtcNow;
            var upsertResult = await UpsertRecordsAsync(transformedRecords, configuration.Destination, batchId);
            result.DatabaseOperationDuration = DateTime.UtcNow - dbStartTime;
            
            result.RecordsInserted += upsertResult.RecordsInserted;
            result.RecordsUpdated += upsertResult.RecordsUpdated;
            result.RecordsFailed += upsertResult.RecordsFailed;
            result.RecordsSkipped += upsertResult.RecordsSkipped;
            result.Errors.AddRange(upsertResult.Errors);
        }

        _logger.LogInformation("Batch {BatchId} processed: {Inserted} inserted, {Updated} updated, {Failed} failed",
            batchId, result.RecordsInserted, result.RecordsUpdated, result.RecordsFailed);

        return result;
    }

    public Task<ExternalDataRecord> TransformRecordAsync(
        ExternalDataRecord record, 
        DestinationConfiguration destinationConfig)
    {
        var transformedRecord = new ExternalDataRecord
        {
            PrimaryKey = record.PrimaryKey,
            Data = new Dictionary<string, object?>(),
            ResolvedForeignKeys = record.ResolvedForeignKeys,
            LastModified = record.LastModified,
            SourceSystem = record.SourceSystem
        };

        // Apply field mappings and transformations
        foreach (var fieldMapping in destinationConfig.FieldMappings)
        {
            var sourceValue = record.Data.GetValueOrDefault(fieldMapping.SourceField);
            
            // Apply transformations if specified
            if (fieldMapping.Transformations.Any())
            {
                sourceValue = _transformationEngine.ApplyTransformations(
                    sourceValue, 
                    fieldMapping.Transformations, 
                    fieldMapping.DestinationField);
            }

            // Apply default value if empty and default is specified
            if ((sourceValue == null || string.IsNullOrEmpty(sourceValue.ToString())) && 
                !string.IsNullOrEmpty(fieldMapping.DefaultValue))
            {
                sourceValue = fieldMapping.DefaultValue;
            }

            // Validate required fields
            if (fieldMapping.IsRequired && (sourceValue == null || string.IsNullOrEmpty(sourceValue.ToString())))
            {
                throw new InvalidOperationException($"Required field '{fieldMapping.DestinationField}' is null or empty");
            }

            // Type conversion and validation
            sourceValue = ConvertToDestinationType(sourceValue, fieldMapping.DataType, fieldMapping.DestinationField);

            transformedRecord.Data[fieldMapping.DestinationField] = sourceValue;
        }

        return Task.FromResult(transformedRecord);
    }

    public async Task<SyncBatchResult> UpsertRecordsAsync(
        IEnumerable<ExternalDataRecord> records, 
        DestinationConfiguration destinationConfig, 
        string batchId)
    {
        var result = new SyncBatchResult();
        var tableName = GetFullTableName(destinationConfig);
        var processedRecords = new List<(string PrimaryKey, string RecordAction, string? ErrorMessage)>();

        // Parse batchId to get the numeric batch log ID for foreign key reference
        var batchLogId = long.Parse(batchId);

        try
        {
            using var transaction = await _databaseService.BeginTransactionAsync();

            foreach (var record in records)
            {
                try
                {
                    var recordResult = await UpsertSingleRecordAsync(record, destinationConfig, batchId);
                    
                    if (recordResult.WasInserted)
                    {
                        result.RecordsInserted++;
                        processedRecords.Add((record.PrimaryKey, "inserted", null));
                    }
                    else
                    {
                        result.RecordsUpdated++;
                        processedRecords.Add((record.PrimaryKey, "updated", null));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upsert record with key {RecordKey}", record.PrimaryKey);
                    var errorMessage = $"Database upsert failed: {ex.Message}";
                    result.Errors.Add(new SyncError
                    {
                        Message = errorMessage,
                        Code = "DB_UPSERT_ERROR",
                        Exception = ex,
                        RecordKey = record.PrimaryKey
                    });
                    result.RecordsFailed++;
                    processedRecords.Add((record.PrimaryKey, "failed", errorMessage));
                }
            }

            // Log all processed records to the database
            if (processedRecords.Any())
            {
                await _syncLoggingService.LogProcessedRecordsAsync(batchLogId, processedRecords);
            }

            await transaction.CommitAsync();
            _logger.LogDebug("Successfully committed batch transaction for {RecordCount} records", records.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for batch {BatchId}", batchId);
            result.Errors.Add(new SyncError
            {
                Message = $"Batch transaction failed: {ex.Message}",
                Code = "DB_TRANSACTION_ERROR",
                Exception = ex
            });
            
            // Don't throw - return the result with the individual record failures preserved
            // The calling code will handle the transaction failure appropriately
            _logger.LogWarning("Transaction failed but returning partial results: {RecordsFailed} records failed, {RecordsInserted} inserted, {RecordsUpdated} updated",
                result.RecordsFailed, result.RecordsInserted, result.RecordsUpdated);
        }

        return result;
    }

    private async Task<(bool WasInserted, bool WasUpdated)> UpsertSingleRecordAsync(
        ExternalDataRecord record, 
        DestinationConfiguration destinationConfig, 
        string batchId)
    {
        var tableName = GetFullTableName(destinationConfig);
        var primaryKeyField = GetPrimaryKeyFieldForUpsert(destinationConfig);

        // Build the upsert query (PostgreSQL UPSERT)
        var columns = new List<string>();
        var values = new List<string>();
        var updateClauses = new List<string>();
        var parameters = new Dictionary<string, object?>();

        // Add mapped fields
        foreach (var fieldMapping in destinationConfig.FieldMappings)
        {
            if (record.Data.TryGetValue(fieldMapping.DestinationField, out var value))
            {
                var paramName = $"@{fieldMapping.DestinationField}";
                columns.Add($"\"{fieldMapping.DestinationField}\"");
                values.Add(paramName);
                parameters[paramName] = value;

                // Add to update clause (exclude unique fields from updates to avoid conflicts)
                if (!fieldMapping.IsUnique)
                {
                    updateClauses.Add($"\"{fieldMapping.DestinationField}\" = EXCLUDED.\"{fieldMapping.DestinationField}\"");
                }
            }
        }

        // Add resolved foreign keys
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            if (record.ResolvedForeignKeys.TryGetValue(fkMapping.DestinationField, out var fkValue))
            {
                var paramName = $"@{fkMapping.DestinationField}";
                columns.Add($"\"{fkMapping.DestinationField}\"");
                values.Add(paramName);
                parameters[paramName] = fkValue;
                updateClauses.Add($"\"{fkMapping.DestinationField}\" = EXCLUDED.\"{fkMapping.DestinationField}\"");
                
                _logger.LogDebug("Including FK in UPSERT for record {RecordKey}: {DestinationField} = {FKValue}", 
                    record.PrimaryKey, fkMapping.DestinationField, fkValue);
            }
            else
            {
                _logger.LogDebug("No resolved FK value found for record {RecordKey}: {DestinationField} (resolved FK count: {ResolvedFKCount})", 
                    record.PrimaryKey, fkMapping.DestinationField, record.ResolvedForeignKeys.Count);
            }
        }

        // Add audit fields
        var now = DateTime.UtcNow;
        var auditFields = new Dictionary<string, object?>
        {
            [destinationConfig.AuditFields.CreatedBy] = 0, // External system user
            [destinationConfig.AuditFields.CreatedDate] = now,
            [destinationConfig.AuditFields.LastModifiedBy] = 0,
            [destinationConfig.AuditFields.LastModifiedDate] = now,
            [destinationConfig.AuditFields.SyncDate] = now,
            [destinationConfig.AuditFields.SyncBatchId] = batchId,
            [destinationConfig.AuditFields.SourceSystem] = record.SourceSystem ?? "External",
            [destinationConfig.AuditFields.IsDeleted] = false
        };

        foreach (var auditField in auditFields)
        {
            var paramName = $"@{auditField.Key}";
            columns.Add($"\"{auditField.Key}\"");
            values.Add(paramName);
            parameters[paramName] = auditField.Value;

            // Update LastModifiedBy, LastModifiedDate, SyncDate, SyncBatchId, and IsDeleted on updates
            if (auditField.Key == destinationConfig.AuditFields.LastModifiedBy ||
                auditField.Key == destinationConfig.AuditFields.LastModifiedDate ||
                auditField.Key == destinationConfig.AuditFields.SyncDate ||
                auditField.Key == destinationConfig.AuditFields.SyncBatchId ||
                auditField.Key == destinationConfig.AuditFields.IsDeleted)
            {
                updateClauses.Add($"\"{auditField.Key}\" = EXCLUDED.\"{auditField.Key}\"");
            }
        }

        // Build the UPSERT query
        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO {tableName} ({string.Join(", ", columns)})");
        sql.AppendLine($"VALUES ({string.Join(", ", values)})");
        
        if (updateClauses.Any())
        {
            sql.AppendLine($"ON CONFLICT ({primaryKeyField}) DO UPDATE SET");
            sql.AppendLine(string.Join(",\n    ", updateClauses));
        }
        else
        {
            sql.AppendLine("ON CONFLICT DO NOTHING");
        }

        sql.AppendLine("RETURNING (xmax = 0) AS inserted;"); // PostgreSQL trick to detect insert vs update

        _logger.LogDebug("Executing upsert for record {RecordKey}", record.PrimaryKey);
        
        // Only log SQL details at Trace level to reduce log volume
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Upsert SQL for record {RecordKey}: {SQL}", record.PrimaryKey, sql.ToString());
        }

        // Execute the query
        var wasInserted = await ExecuteUpsertQuery(sql.ToString(), parameters);

        return (wasInserted, !wasInserted);
    }

    private async Task<bool> ExecuteUpsertQuery(string sql, Dictionary<string, object?> parameters)
    {
        using var connection = await _databaseService.GetConnectionAsync();
        
        // Log database connection details for debugging
        _logger.LogDebug("Executing UPSERT in database: {Database} on server: {Server}", 
            connection.Database, connection.DataSource);
            
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var param in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = param.Key;
            parameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToBoolean(result);
    }

    private async Task ResolveForeignKeysAsync(
        List<ExternalDataRecord> records, 
        List<ForeignKeyMapping> fkMappings,
        SyncBatchResult result)
    {
        _logger.LogInformation("Starting foreign key resolution for {FKMappingCount} mappings across {RecordCount} records", 
            fkMappings.Count, records.Count);

        // Log all FK mappings for diagnostics
        for (int i = 0; i < fkMappings.Count; i++)
        {
            var fk = fkMappings[i];
            _logger.LogInformation("FK Mapping {Index}: {SourceField} -> {LookupTable}.{LookupField} -> {DestinationField} (Required: {IsRequired})", 
                i + 1, fk.SourceField, fk.LookupTable, fk.LookupField, fk.DestinationField, fk.IsRequired);
        }

        foreach (var fkMapping in fkMappings)
        {
            try
            {
                _logger.LogInformation("*** PROCESSING FK mapping: {SourceField} -> {LookupTable}.{LookupField} -> {DestinationField} ***", 
                    fkMapping.SourceField, fkMapping.LookupTable, fkMapping.LookupField, fkMapping.DestinationField);

                // Get all source values for this FK mapping
                var recordsWithSourceField = records.Where(r => r.Data.ContainsKey(fkMapping.SourceField)).ToList();
                _logger.LogInformation("Records containing source field '{SourceField}': {RecordCount}/{TotalRecords}", 
                    fkMapping.SourceField, recordsWithSourceField.Count, records.Count);

                // Log sample source field values for diagnostics
                if (recordsWithSourceField.Any())
                {
                    var sampleValues = recordsWithSourceField.Take(3).Select(r => 
                        $"Record {r.PrimaryKey}: '{fkMapping.SourceField}' = '{r.Data[fkMapping.SourceField]}'").ToList();
                    _logger.LogInformation("Sample source field values: [{SampleValues}]", string.Join(", ", sampleValues));
                }

                var sourceValues = recordsWithSourceField
                    .Select(r => r.Data[fkMapping.SourceField]?.ToString())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Distinct()
                    .Cast<string>()
                    .ToList();

                _logger.LogInformation("Found {SourceValueCount} unique non-empty source values for FK mapping {SourceField}: [{SourceValues}]", 
                    sourceValues.Count, fkMapping.SourceField, string.Join(", ", sourceValues.Take(10)));

                if (!sourceValues.Any()) 
                {
                    _logger.LogWarning("No source values found for FK mapping {SourceField}, skipping FK resolution", fkMapping.SourceField);
                    continue;
                }

                // Resolve all FK values at once
                var resolvedKeys = await _fkResolver.ResolveForeignKeysAsync(fkMapping, sourceValues);

                _logger.LogDebug("FK Resolution completed for {SourceField}. Resolved {ResolvedCount}/{TotalCount} values: {ResolvedKeys}", 
                    fkMapping.SourceField, 
                    resolvedKeys.Count(kvp => kvp.Value.HasValue), 
                    resolvedKeys.Count,
                    string.Join(", ", resolvedKeys.Take(5).Select(kvp => $"{kvp.Key}={kvp.Value}")));

                // Update records with resolved FK values
                var recordsUpdated = 0;
                foreach (var record in records)
                {
                    if (record.Data.TryGetValue(fkMapping.SourceField, out var sourceValue) && 
                        sourceValue != null)
                    {
                        var sourceValueStr = sourceValue.ToString()!;
                        if (resolvedKeys.TryGetValue(sourceValueStr, out var resolvedId))
                        {
                            record.ResolvedForeignKeys[fkMapping.DestinationField] = resolvedId;
                            recordsUpdated++;
                            _logger.LogDebug("Applied FK value to record {RecordKey}: {SourceField}='{SourceValue}' -> {DestinationField}={ResolvedId}", 
                                record.PrimaryKey, fkMapping.SourceField, sourceValueStr, fkMapping.DestinationField, resolvedId);
                        }
                        else 
                        {
                            _logger.LogDebug("No FK resolution for record {RecordKey}: {SourceField}='{SourceValue}' (OnLookupFail: {OnLookupFail})", 
                                record.PrimaryKey, fkMapping.SourceField, sourceValueStr, fkMapping.OnLookupFail);

                            if (fkMapping.OnLookupFail == LookupFailAction.FailRecord)
                            {
                                result.Errors.Add(new SyncError
                                {
                                    Message = $"Foreign key lookup failed for {fkMapping.LookupTable}.{fkMapping.LookupField} = '{sourceValueStr}'",
                                    Code = "FK_LOOKUP_FAILED",
                                    FieldName = fkMapping.DestinationField,
                                    FieldValue = sourceValueStr,
                                    RecordKey = record.PrimaryKey
                                });
                            }
                        }
                        // For LogWarning and SetNull, the ForeignKeyResolver already handles the logging
                        // and we set null by default
                    }
                }

                _logger.LogInformation("*** COMPLETED FK mapping {SourceField} -> {DestinationField}: Updated {UpdatedCount}/{TotalRecords} records ***", 
                    fkMapping.SourceField, fkMapping.DestinationField, recordsUpdated, records.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** FAILED FK mapping {SourceField} -> {LookupTable}.{LookupField} ***",
                    fkMapping.SourceField, fkMapping.LookupTable, fkMapping.LookupField);
                
                result.Errors.Add(new SyncError
                {
                    Message = $"Foreign key resolution error: {ex.Message}",
                    Code = "FK_RESOLUTION_ERROR",
                    Exception = ex
                });
            }
        }
        
        _logger.LogInformation("*** FINISHED processing all {FKMappingCount} FK mappings ***", fkMappings.Count);
    }

    private object? ConvertToDestinationType(object? value, string dataType, string fieldName)
    {
        if (value == null) return null;

        var stringValue = value.ToString();
        if (string.IsNullOrEmpty(stringValue)) return null;

        try
        {
            var normalizedType = dataType.ToLower();
            
            return normalizedType switch
            {
                var t when t.Contains("varchar") || t.Contains("char") || t.Contains("text") => stringValue,
                var t when t.Contains("integer") || t == "int" => int.Parse(stringValue),
                var t when t.Contains("bigint") || t == "long" => long.Parse(stringValue),
                var t when t.Contains("smallint") || t == "short" => short.Parse(stringValue),
                var t when t.Contains("decimal") || t.Contains("numeric") => decimal.Parse(stringValue),
                var t when t.Contains("real") || t == "float" => float.Parse(stringValue),
                var t when t.Contains("double") => double.Parse(stringValue),
                var t when t.Contains("boolean") || t == "bool" => bool.Parse(stringValue),
                var t when t.Contains("date") && !t.Contains("time") => DateOnly.Parse(stringValue),
                var t when t.Contains("time") && !t.Contains("stamp") => TimeOnly.Parse(stringValue),
                var t when t.Contains("timestamp") || t.Contains("datetime") => DateTime.Parse(stringValue),
                var t when t.Contains("uuid") || t.Contains("guid") => Guid.Parse(stringValue),
                _ => stringValue
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert value '{Value}' to type '{DataType}' for field '{FieldName}', keeping as string",
                stringValue, dataType, fieldName);
            return stringValue; // Return as string if conversion fails
        }
    }

    private string GetFullTableName(DestinationConfiguration destinationConfig)
    {
        return string.IsNullOrEmpty(destinationConfig.Schema) 
            ? $"\"{destinationConfig.TableName}\"" 
            : $"\"{destinationConfig.Schema}\".\"{destinationConfig.TableName}\"";
    }

    private string GetPrimaryKeyFieldForUpsert(DestinationConfiguration destinationConfig)
    {
        // Find the first unique field to use as primary key for UPSERT
        var uniqueField = destinationConfig.FieldMappings.FirstOrDefault(f => f.IsUnique);
        if (uniqueField != null)
        {
            return $"\"{uniqueField.DestinationField}\"";
        }

        // If no unique field is specified, use the first field (assuming it's the external ID)
        var firstField = destinationConfig.FieldMappings.First();
        return $"\"{firstField.DestinationField}\"";
    }
}
