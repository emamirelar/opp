using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;
using UNOPS.PAO.ExternalDataService.Models.Sync;
using UNOPS.PAO.ExternalDataService.Services.DataSource;
using UNOPS.PAO.ExternalDataService.Services.Database;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public class SyncOrchestrator : ISyncOrchestrator
{
    private readonly IDataSourceService _dataSourceService;
    private readonly IDynamicTableService _tableService;
    private readonly IApplicationDatabaseService _databaseService;
    private readonly IForeignKeyResolver _fkResolver;
    private readonly ISyncProcessor _syncProcessor;
    private readonly ISyncLoggingService _syncLoggingService;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(
        IDataSourceService dataSourceService,
        IDynamicTableService tableService,
        IApplicationDatabaseService databaseService,
        IForeignKeyResolver fkResolver,
        ISyncProcessor syncProcessor,
        ISyncLoggingService syncLoggingService,
        ILogger<SyncOrchestrator> logger)
    {
        _dataSourceService = dataSourceService;
        _tableService = tableService;
        _databaseService = databaseService;
        _fkResolver = fkResolver;
        _syncProcessor = syncProcessor;
        _syncLoggingService = syncLoggingService;
        _logger = logger;
    }

    public async Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default)
    {
        // Step 0: Start logging
        var executionLog = await _syncLoggingService.StartSyncExecutionAsync(
            configuration, 
            "manual", // This can be parameterized
            Environment.GetEnvironmentVariable("K_SERVICE") ?? "local"
        );

        var syncResult = new SyncResult
        {
            ConfigurationName = configuration.Metadata.Name,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting sync for configuration {ConfigName} (Execution ID: {ExecutionId})", 
                configuration.Metadata.Name, executionLog.Id);

            // Step 1: Validate configuration
            if (!await ValidateConfigurationAsync(configuration))
            {
                throw new InvalidOperationException($"Configuration validation failed for '{configuration.Metadata.Name}'. Check the preceding error logs for specific validation failures (field mappings, foreign key mappings, table/field existence, etc.).");
            }

            // Step 2: Test source connection
            if (!await _dataSourceService.TestConnectionAsync(configuration.Source))
            {
                throw new InvalidOperationException($"Source connection test failed for {configuration.Metadata.Name}");
            }

            // Step 3: Prepare destination table
            await PrepareDestinationTableAsync(configuration.Destination);

            // Step 4: Extract data from source
            var lastSyncDate = await _syncLoggingService.GetLastSuccessfulSyncDateAsync(configuration.Metadata.Name);
            _logger.LogInformation("Last successful sync date: {LastSyncDate}", lastSyncDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");
            
            var sourceRecords = await _dataSourceService.ExtractDataAsync(configuration.Source, lastSyncDate);
            
            syncResult.TotalRecordsExtracted = sourceRecords.Count();
            _logger.LogInformation("Extracted {RecordCount} records from source", syncResult.TotalRecordsExtracted);

            if (!sourceRecords.Any())
            {
                _logger.LogInformation("No records to sync for configuration {ConfigName}", configuration.Metadata.Name);
                syncResult.Status = SyncStatus.Completed;
                syncResult.EndTime = DateTime.UtcNow;
                await _syncLoggingService.UpdateSyncExecutionAsync(executionLog.Id, syncResult);
                await _syncLoggingService.UpdateConfigurationHistoryAsync(configuration.Metadata.Name, syncResult);
                return syncResult;
            }

            // Step 5: Process records in batches
            var batches = sourceRecords.Chunk(configuration.Source.BatchSize);
            var batchNumber = 1;
            var totalBatches = (int)Math.Ceiling((double)sourceRecords.Count() / configuration.Source.BatchSize);
            
            _logger.LogInformation("Processing {TotalRecords} records in {BatchCount} batches of size {BatchSize}", 
                sourceRecords.Count(), totalBatches, configuration.Source.BatchSize);

            foreach (var batch in batches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Sync cancelled for configuration {ConfigName} at batch {BatchNumber}", 
                        configuration.Metadata.Name, batchNumber);
                    syncResult.Status = SyncStatus.Cancelled;
                    break;
                }

                await ProcessBatchAsync(batch, configuration, syncResult, executionLog.Id, batchNumber, totalBatches);
                batchNumber++;

                // Add small delay between batches to avoid overwhelming the database
                if (batchNumber <= totalBatches)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }

            // Step 6: Handle record deletions (mark orphaned records as deleted)
            if (syncResult.Status != SyncStatus.Cancelled)
            {
                _logger.LogInformation("Starting deletion phase for execution {ExecutionId}", executionLog.Id);
                var deletedCount = await HandleOrphanedRecordsAsync(configuration, executionLog.Id);
                syncResult.RecordsDeleted = deletedCount;
                
                _logger.LogInformation("Deletion phase completed: {DeletedCount} orphaned records marked as deleted", deletedCount);
                _logger.LogInformation("syncResult.RecordsDeleted is now set to: {RecordsDeleted}", syncResult.RecordsDeleted);
            }

            // Step 7: Determine final sync status
            if (syncResult.Status != SyncStatus.Cancelled)
            {
                syncResult.Status = syncResult.RecordsFailed > 0 
                    ? SyncStatus.CompletedWithErrors 
                    : SyncStatus.Completed;
            }

            syncResult.EndTime = DateTime.UtcNow;

            // Update execution log
            await _syncLoggingService.UpdateSyncExecutionAsync(executionLog.Id, syncResult);
            
            // Update configuration history
            await _syncLoggingService.UpdateConfigurationHistoryAsync(configuration.Metadata.Name, syncResult);

            _logger.LogInformation("Sync completed for {ConfigName}: {Inserted} inserted, {Updated} updated, {Deleted} deleted, {Failed} failed",
                configuration.Metadata.Name, syncResult.RecordsInserted, syncResult.RecordsUpdated, syncResult.RecordsDeleted, syncResult.RecordsFailed);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed for configuration {ConfigName}", configuration.Metadata.Name);
            syncResult.Status = SyncStatus.Failed;
            syncResult.EndTime = DateTime.UtcNow;
            
            var syncError = new SyncError 
            { 
                Message = ex.Message, 
                Exception = ex, 
                Code = "SYNC_FAILURE"
            };
            syncResult.Errors.Add(syncError);
            
            // Log the failure
            await _syncLoggingService.LogErrorAsync(executionLog.Id, null, syncError);
            await _syncLoggingService.UpdateSyncExecutionAsync(executionLog.Id, syncResult);
            await _syncLoggingService.UpdateConfigurationHistoryAsync(configuration.Metadata.Name, syncResult);
        }
        finally
        {
            if (syncResult.EndTime == DateTime.MinValue)
            {
                syncResult.EndTime = DateTime.UtcNow;
            }
        }

        return syncResult;
    }

    public async Task<bool> ValidateConfigurationAsync(SyncConfiguration configuration)
    {
        try
        {
            // Validate foreign key mappings
            foreach (var fkMapping in configuration.Destination.ForeignKeyMappings)
            {
                if (!await _fkResolver.ValidateForeignKeyMappingAsync(fkMapping))
                {
                    _logger.LogError("Foreign key mapping validation failed for configuration '{ConfigName}': " +
                        "source_field='{SourceField}', lookup_table='{LookupTable}', lookup_field='{LookupField}', " +
                        "destination_field='{DestinationField}', lookup_return_field='{LookupReturnField}'", 
                        configuration.Metadata.Name, fkMapping.SourceField, fkMapping.LookupTable, fkMapping.LookupField, 
                        fkMapping.DestinationField, fkMapping.LookupReturnField);
                    return false;
                }
            }

            // Validate field mappings have valid data types
            foreach (var fieldMapping in configuration.Destination.FieldMappings)
            {
                if (string.IsNullOrEmpty(fieldMapping.DataType))
                {
                    _logger.LogError("Field mapping validation failed for configuration '{ConfigName}': " +
                        "source_field='{SourceField}' -> destination_field='{DestinationField}' has no data_type specified", 
                        configuration.Metadata.Name, fieldMapping.SourceField, fieldMapping.DestinationField);
                    return false;
                }
            }

            _logger.LogDebug("Configuration validation passed for {ConfigName}", configuration.Metadata.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation error for {ConfigName}", configuration.Metadata.Name);
            return false;
        }
    }

    private async Task PrepareDestinationTableAsync(DestinationConfiguration destination)
    {
        var fullTableName = GetFullTableName(destination);
        
        try
        {
            var tableExists = await _tableService.TableExistsAsync(destination.TableName, destination.Schema);
            
            if (!tableExists)
            {
                _logger.LogInformation("Creating destination table {TableName}", fullTableName);
                await _tableService.CreateTableAsync(destination);
                
                // Double-check that table creation was successful
                var tableExistsAfterCreation = await _tableService.TableExistsAsync(destination.TableName, destination.Schema);
                if (!tableExistsAfterCreation)
                {
                    throw new InvalidOperationException($"Failed to create destination table {fullTableName}. Table does not exist after creation attempt.");
                }
                
                _logger.LogInformation("Destination table {TableName} created and verified successfully", fullTableName);
            }
            else
            {
                _logger.LogDebug("Updating destination table schema for {TableName}", fullTableName);
                await _tableService.UpdateTableSchemaAsync(destination);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare destination table {TableName}", fullTableName);
            throw;
        }
    }

    private async Task ProcessBatchAsync(
        ExternalDataRecord[] batch,
        SyncConfiguration configuration,
        SyncResult syncResult,
        long executionId,
        int batchNumber,
        int totalBatches)
    {
        var firstRecordKey = batch.FirstOrDefault()?.PrimaryKey;
        var lastRecordKey = batch.LastOrDefault()?.PrimaryKey;
        
        _logger.LogDebug("Processing batch {BatchNumber}/{TotalBatches} with {RecordCount} records", 
            batchNumber, totalBatches, batch.Length);

        // Start batch logging
        var batchLog = await _syncLoggingService.StartBatchAsync(
            executionId, 
            batchNumber, 
            batch.Length, 
            firstRecordKey, 
            lastRecordKey
        );

        try
        {
            var batchStartTime = DateTime.UtcNow;
            
            var batchResult = await _syncProcessor.ProcessBatchAsync(batch, configuration, batchLog.Id.ToString());
            
            var batchDuration = DateTime.UtcNow - batchStartTime;
            _logger.LogDebug("Batch {BatchNumber} completed in {Duration}ms with {TotalProcessed} records processed",
                batchNumber, (int)batchDuration.TotalMilliseconds, 
                batchResult.RecordsInserted + batchResult.RecordsUpdated + batchResult.RecordsFailed);
            
            syncResult.RecordsInserted += batchResult.RecordsInserted;
            syncResult.RecordsUpdated += batchResult.RecordsUpdated;
            syncResult.RecordsFailed += batchResult.RecordsFailed;
            syncResult.Errors.AddRange(batchResult.Errors);

            // Complete batch logging
            await _syncLoggingService.CompleteBatchAsync(batchLog.Id, batchResult);

            // Log any batch-specific errors
            foreach (var error in batchResult.Errors)
            {
                await _syncLoggingService.LogErrorAsync(
                    executionId, 
                    batchLog.Id, 
                    error, 
                    error.RecordKey, 
                    error.RecordData
                );
            }
        }
        catch (Exception batchEx)
        {
            _logger.LogError(batchEx, "Error processing batch {BatchNumber} for configuration {ConfigName}", 
                batchNumber, configuration.Metadata.Name);
                
            var batchError = new SyncError 
            { 
                Message = $"Batch processing failed: {batchEx.Message}", 
                Exception = batchEx,
                Code = "BATCH_ERROR"
            };
            
            syncResult.Errors.Add(batchError);
            
            // For catastrophic batch failures (before individual record processing),
            // count all records in the batch as failed
            syncResult.RecordsFailed += batch.Length;
            var recordsFailedInBatch = batch.Length;
            
            await _syncLoggingService.LogErrorAsync(executionId, batchLog.Id, batchError);
            
            // Mark batch as failed
            var failedBatchResult = new SyncBatchResult 
            { 
                RecordsFailed = recordsFailedInBatch,
                Errors = new List<SyncError> { batchError }
            };
            await _syncLoggingService.CompleteBatchAsync(batchLog.Id, failedBatchResult);

            // Don't re-throw the exception - let sync continue with other batches
            // Individual record failures are already properly counted and logged
            _logger.LogWarning("Batch {BatchNumber} failed but sync will continue with remaining batches. Individual record failures have been preserved.", batchNumber);
        }
    }

    private string GetFullTableName(DestinationConfiguration destination)
    {
        return string.IsNullOrEmpty(destination.Schema) 
            ? $"\"{destination.TableName}\"" 
            : $"\"{destination.Schema}\".\"{destination.TableName}\"";
    }

    public async Task<Dictionary<string, object?>> GetSyncStatusAsync(string configurationName)
    {
        try
        {
            var recentExecutions = await _syncLoggingService.GetRecentExecutionsAsync(configurationName, 10);
            var lastExecution = recentExecutions.FirstOrDefault();
            
            var status = new Dictionary<string, object?>
            {
                ["configurationName"] = configurationName,
                ["isRunning"] = lastExecution?.Status == SyncStatus.Running,
                ["lastExecutionTime"] = lastExecution?.StartTime as object,
                ["lastExecutionStatus"] = lastExecution?.Status.ToString() as object,
                ["lastExecutionDuration"] = lastExecution?.Duration?.TotalSeconds as object,
                 ["recentExecutions"] = recentExecutions.Select(e => new
                 {
                     id = e.Id,
                     startTime = e.StartTime,
                     endTime = e.EndTime,
                     status = e.Status.ToString(),
                     recordsInserted = e.RecordsInserted,
                     recordsUpdated = e.RecordsUpdated,
                     recordsDeleted = e.RecordsDeleted,
                     recordsFailed = e.RecordsFailed,
                     duration = e.Duration?.TotalSeconds
                 }).ToList()
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status for {ConfigurationName}", configurationName);
            throw;
        }
    }

    private async Task<int> HandleOrphanedRecordsAsync(SyncConfiguration configuration, long executionId)
    {
        var destination = configuration.Destination;
        // Use the same SourceSystem value that records are inserted with
        // This should match what BigQuerySourceService.cs sets: SourceSystem = "BigQuery"
        var sourceSystemValue = configuration.Source.Type switch
        {
            "bigquery" => "BigQuery",
            _ => "External"
        };
        
        // Find the destination field that corresponds to the source primary key
        var primaryKeyMapping = destination.FieldMappings.FirstOrDefault(fm => 
            fm.SourceField == configuration.Source.PrimaryKeyField);
            
        if (primaryKeyMapping == null)
        {
            _logger.LogWarning("No field mapping found for primary key field {PrimaryKeyField}, skipping deletion handling", 
                configuration.Source.PrimaryKeyField);
            return 0;
        }

        var tableName = GetFullTableName(destination);
        var primaryKeyField = primaryKeyMapping.DestinationField;
        var sourceSystemField = destination.AuditFields.SourceSystem;
        var isDeletedField = destination.AuditFields.IsDeleted;
        
        try
        {
            using var connection = await GetDatabaseConnectionAsync();
            
            // Get processed primary keys for this execution from the database
            var processedPrimaryKeys = await _syncLoggingService.GetProcessedPrimaryKeysForExecutionAsync(executionId);
            
            // Debug logging
            _logger.LogInformation("Deletion detection: Table={TableName}, SourceSystem={SourceSystem}, PrimaryKeyField={PrimaryKeyField}", 
                tableName, sourceSystemValue, primaryKeyField);
            _logger.LogInformation("Found {Count} processed keys for execution {ExecutionId}", 
                processedPrimaryKeys.Count, executionId);
            
            // Only log actual key values at Debug level to reduce log volume
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Processed keys from execution {ExecutionId}: [{Keys}]", 
                    executionId, string.Join(", ", processedPrimaryKeys.Take(10)) + (processedPrimaryKeys.Count > 10 ? "..." : ""));
            }

            if (!processedPrimaryKeys.Any())
            {
                _logger.LogWarning("No processed records found for execution {ExecutionId}, skipping deletion detection", executionId);
                return 0;
            }

            // First, check what records exist in destination for this source system
            var checkSql = $@"
                SELECT ""{primaryKeyField}"", ""{isDeletedField}"" 
                FROM {tableName} 
                WHERE ""{sourceSystemField}"" = @sourceSystem
                ORDER BY ""{primaryKeyField}""";
            
            _logger.LogDebug("Checking existing records for deletion cleanup");
            
            // Only log SQL details at Trace level to reduce log volume
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Deletion check SQL: {SQL}", checkSql);
            }
            
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = checkSql;
                checkCommand.Parameters.Add(checkCommand.CreateParameter());
                checkCommand.Parameters[0].ParameterName = "@sourceSystem";
                checkCommand.Parameters[0].Value = sourceSystemValue;
                
                using var reader = await checkCommand.ExecuteReaderAsync();
                var existingRecords = new List<(string Key, bool IsDeleted)>();
                while (await reader.ReadAsync())
                {
                    existingRecords.Add((reader.GetString(0), reader.GetBoolean(1)));
                }
                
                _logger.LogInformation("Found {Count} existing records in destination for source system '{SourceSystem}'", 
                    existingRecords.Count, sourceSystemValue);
                _logger.LogInformation("Existing active records: [{Keys}]", 
                    string.Join(", ", existingRecords.Where(r => !r.IsDeleted).Select(r => r.Key).Take(10)) + 
                    (existingRecords.Count(r => !r.IsDeleted) > 10 ? "..." : ""));
                
                var orphanedKeys = existingRecords
                    .Where(r => !r.IsDeleted && !processedPrimaryKeys.Contains(r.Key))
                    .Select(r => r.Key)
                    .ToList();
                    
                _logger.LogInformation("Orphaned keys to mark as deleted ({Count}): [{Keys}]", 
                    orphanedKeys.Count, string.Join(", ", orphanedKeys));
            }
            
            // Build parameterized query to avoid SQL injection
            var keysParam = string.Join(",", processedPrimaryKeys.Select((_, i) => $"@key{i}"));
            
            // BEFORE deletion: Check how many records are eligible for deletion
            var beforeCountSql = $@"
                SELECT COUNT(*) 
                FROM {tableName} 
                WHERE ""{sourceSystemField}"" = @sourceSystem 
                  AND ""{isDeletedField}"" = false 
                  AND ""{primaryKeyField}"" NOT IN ({keysParam})";
            
            var beforeParams = new Dictionary<string, object?>
            {
                ["@sourceSystem"] = sourceSystemValue
            };
            
            // Add key parameters for before count
            var beforeKeyIndex = 0;
            foreach (var key in processedPrimaryKeys)
            {
                beforeParams[$"@key{beforeKeyIndex}"] = key;
                beforeKeyIndex++;
            }
            
            var recordsEligibleForDeletion = await ExecuteScalarQuery(connection, beforeCountSql, beforeParams);
            
            _logger.LogInformation("Pre-deletion check: {EligibleCount} records eligible for deletion (IsDeleted=false, not in processed keys)", 
                recordsEligibleForDeletion);
            
            // Now perform the actual deletion
            var sql = $@"
                UPDATE {tableName} 
                SET ""{isDeletedField}"" = true,
                    ""{destination.AuditFields.LastModifiedDate}"" = @now,
                    ""{destination.AuditFields.LastModifiedBy}"" = 0
                WHERE ""{sourceSystemField}"" = @sourceSystem
                  AND ""{isDeletedField}"" = false
                   AND ""{primaryKeyField}"" NOT IN ({keysParam})";

            _logger.LogDebug("Executing deletion of {RecordCount} stale records", recordsEligibleForDeletion);
            
            // Only log SQL details at Trace level to reduce log volume
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Deletion SQL: {SQL}", sql);
            }

            var parameters = new Dictionary<string, object?>
            {
                ["@sourceSystem"] = sourceSystemValue,
                ["@now"] = DateTime.UtcNow
            };

            // Add key parameters
            var keyIndex = 0;
            foreach (var key in processedPrimaryKeys)
            {
                parameters[$"@key{keyIndex}"] = key;
                keyIndex++;
            }

            var deletedCount = await ExecuteUpdateQuery(connection, sql, parameters);

            _logger.LogInformation("ExecuteUpdateQuery returned: {DeletedCount} for source system {SourceSystem}", 
                deletedCount, sourceSystemValue);
            
            // Post-deletion verification: Count total deleted records
            var totalDeletedSql = $@"
                SELECT COUNT(*) 
                FROM {tableName} 
                WHERE ""{sourceSystemField}"" = @sourceSystem 
                  AND ""{isDeletedField}"" = true";
            
            var totalDeletedParams = new Dictionary<string, object?>
            {
                ["@sourceSystem"] = sourceSystemValue
            };
            
            var totalDeletedCount = await ExecuteScalarQuery(connection, totalDeletedSql, totalDeletedParams);
            
            _logger.LogInformation("Post-deletion verification: Total records with IsDeleted=true: {TotalDeletedCount} for source system {SourceSystem}", 
                totalDeletedCount, sourceSystemValue);
                
            _logger.LogInformation("Deletion count validation: Expected newly deleted = {Expected}, ExecuteNonQuery returned = {Actual}", 
                recordsEligibleForDeletion, deletedCount);
                
            if (recordsEligibleForDeletion != deletedCount)
            {
                _logger.LogWarning("MISMATCH: Expected to delete {Expected} records but ExecuteNonQuery reported {Actual} records affected", 
                    recordsEligibleForDeletion, deletedCount);
            }
            
            _logger.LogInformation("Marked {DeletedCount} orphaned records as deleted for source system {SourceSystem}", 
                deletedCount, sourceSystemValue);
                
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling orphaned records for configuration {ConfigName}", 
                configuration.Metadata.Name);
            throw;
        }
    }

    private async Task<System.Data.Common.DbConnection> GetDatabaseConnectionAsync()
    {
        return await _databaseService.GetConnectionAsync();
    }

    private async Task<int> ExecuteUpdateQuery(System.Data.Common.DbConnection connection, string sql, Dictionary<string, object?> parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var param in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = param.Key;
            parameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteNonQueryAsync();
        _logger.LogDebug("ExecuteNonQueryAsync returned: {Result} affected rows", result);
        return result;
    }

    private async Task<int> ExecuteScalarQuery(System.Data.Common.DbConnection connection, string sql, Dictionary<string, object?> parameters)
    {
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
        return Convert.ToInt32(result ?? 0);
    }
}
