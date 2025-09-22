using System.Text.Json;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;
using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public class SyncLoggingService : ISyncLoggingService
{
    private readonly IMonitoringDatabaseService _monitoringDbService;
    private readonly ILogger<SyncLoggingService> _logger;
    private readonly IConfiguration _configuration;

    public SyncLoggingService(
        IMonitoringDatabaseService monitoringDbService,
        ILogger<SyncLoggingService> logger,
        IConfiguration configuration)
    {
        _monitoringDbService = monitoringDbService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SyncExecutionLog> StartSyncExecutionAsync(
        SyncConfiguration configuration, 
        string triggeredBy, 
        string? triggerId = null)
    {
        var startTime = DateTime.UtcNow;
        var statusMessage = "Sync execution started";
        var configSnapshot = JsonSerializer.Serialize(configuration);
        var cloudRunInstance = Environment.GetEnvironmentVariable("K_SERVICE") ?? Environment.MachineName;

        // Use raw SQL for reliable insert
        var sql = """
            INSERT INTO external."SyncExecutionLogs" 
                ("ConfigurationName", "StartTime", "Status", "StatusMessage", "ConfigurationSnapshot", 
                 "SourceQuery", "DestinationTable", "TriggeredBy", "TriggerId", "CloudRunInstance", 
                 "TotalRecordsExtracted", "RecordsInserted", "RecordsUpdated", "RecordsFailed", 
                 "RecordsDeleted", "BatchesProcessed", "BatchesFailed", "CreatedAt")
            VALUES 
                (@configName, @startTime, @status, @statusMessage, @configSnapshot::jsonb, 
                 @sourceQuery, @destTable, @triggeredBy, @triggerId, @cloudRunInstance,
                 0, 0, 0, 0, 0, 0, 0, @createdAt)
            RETURNING "Id";
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@configName", configuration.Metadata.Name),
            CreateParameter(command, "@startTime", startTime),
            CreateParameter(command, "@status", (int)SyncStatus.Running),
            CreateParameter(command, "@statusMessage", statusMessage),
            CreateParameter(command, "@configSnapshot", configSnapshot),
            CreateParameter(command, "@sourceQuery", configuration.Source.Query),
            CreateParameter(command, "@destTable", configuration.Destination.TableName),
            CreateParameter(command, "@triggeredBy", triggeredBy),
            CreateParameter(command, "@triggerId", triggerId),
            CreateParameter(command, "@cloudRunInstance", cloudRunInstance),
            CreateParameter(command, "@createdAt", startTime)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        long executionId;
        try
        {
        _logger.LogDebug("Creating sync execution log for configuration: {ConfigName}", configuration.Metadata.Name);
        
        // Only log detailed SQL and parameters at Trace level to reduce log volume
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Executing SQL: {Sql}", sql);
            _logger.LogTrace("Parameters: {Parameters}", string.Join(", ", parameters.Select(p => $"{p.ParameterName}={p.Value}")));
        }
            
            var result = await command.ExecuteScalarAsync();
            _logger.LogDebug("Sync execution log created with ID: {ExecutionId}", result);
            
            if (result == null)
            {
                throw new InvalidOperationException("ExecuteScalarAsync returned null - no ID was returned from INSERT");
            }
            
            if (result is long longResult)
            {
                executionId = longResult;
            }
            else if (long.TryParse(result.ToString(), out var parsedId))
            {
                executionId = parsedId;
            }
            else
            {
                throw new InvalidOperationException($"ExecuteScalarAsync returned unexpected type {result.GetType().Name} with value '{result}' - expected long");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute sync execution log insert SQL");
            throw new InvalidOperationException("Failed to create sync execution log", ex);
        }

        _logger.LogInformation("Started sync execution {ExecutionId} for configuration {ConfigName}", 
            executionId, configuration.Metadata.Name);

        // Return a new SyncExecutionLog object with the inserted data
        var executionLog = new SyncExecutionLog
        {
            Id = executionId,
            ConfigurationName = configuration.Metadata.Name,
            StartTime = startTime,
            Status = SyncStatus.Running,
            StatusMessage = statusMessage,
            ConfigurationSnapshot = configSnapshot,
            SourceQuery = configuration.Source.Query,
            DestinationTable = configuration.Destination.TableName,
            TriggeredBy = triggeredBy,
            TriggerId = triggerId,
            CloudRunInstance = cloudRunInstance,
            CreatedAt = startTime
        };

        return executionLog;
    }

    public async Task UpdateSyncExecutionAsync(long executionId, SyncResult result)
    {
        var endTime = DateTime.UtcNow;
        var statusMessage = result.Status == SyncStatus.Failed 
            ? string.Join("; ", result.Errors.Select(e => e.Message))
            : $"Completed: {result.RecordsInserted} inserted, {result.RecordsUpdated} updated, {result.RecordsDeleted} deleted, {result.RecordsFailed} failed";

        _logger.LogInformation("Updating execution {ExecutionId} with RecordsDeleted = {RecordsDeleted}", 
            executionId, result.RecordsDeleted);

        // Calculate performance metrics
        var recordsPerMinute = 0m;
        if (result.TotalRecordsExtracted > 0)
        {
            var startTimeSql = """SELECT "StartTime" FROM external."SyncExecutionLogs" WHERE "Id" = @executionId""";
            using var perfConnection = await _monitoringDbService.GetConnectionAsync();
            using var perfCommand = perfConnection.CreateCommand();
            perfCommand.CommandText = startTimeSql;
            perfCommand.Parameters.Add(CreateParameter(perfCommand, "@executionId", executionId));
            
            var startTimeResult = await perfCommand.ExecuteScalarAsync();
            if (startTimeResult != null && startTimeResult is DateTime startTime)
            {
                var duration = endTime - startTime;
                if (duration.TotalMinutes > 0)
                {
                    recordsPerMinute = result.TotalRecordsExtracted / (decimal)duration.TotalMinutes;
                    _logger.LogInformation("Sync performance: {RecordsPerMinute:F1} records/minute", recordsPerMinute);
                }
            }
        }

        // Use raw SQL for reliable update
        var sql = """
            UPDATE external."SyncExecutionLogs" 
            SET "EndTime" = @endTime,
                "Status" = @status,
                "StatusMessage" = @statusMessage,
                "TotalRecordsExtracted" = @totalExtracted,
                "RecordsInserted" = @recordsInserted,
                "RecordsUpdated" = @recordsUpdated,
                "RecordsFailed" = @recordsFailed,
                "RecordsDeleted" = @recordsDeleted,
                "Duration" = @endTime - "StartTime",
                "LastUpdatedAt" = @endTime
            WHERE "Id" = @executionId
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@executionId", executionId),
            CreateParameter(command, "@endTime", endTime),
            CreateParameter(command, "@status", (int)result.Status),
            CreateParameter(command, "@statusMessage", statusMessage),
            CreateParameter(command, "@totalExtracted", result.TotalRecordsExtracted),
            CreateParameter(command, "@recordsInserted", result.RecordsInserted),
            CreateParameter(command, "@recordsUpdated", result.RecordsUpdated),
            CreateParameter(command, "@recordsFailed", result.RecordsFailed),
            CreateParameter(command, "@recordsDeleted", result.RecordsDeleted)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync();
        _logger.LogInformation("RAW SQL: Updated {RowsAffected} execution record(s) with RecordsDeleted = {RecordsDeleted}", 
            rowsAffected, result.RecordsDeleted);

        _logger.LogInformation("Updated sync execution {ExecutionId} with status {Status}", 
            executionId, result.Status);
    }

    public async Task<SyncBatchLog> StartBatchAsync(
        long executionId, 
        int batchNumber, 
        int recordCount, 
        string? firstRecordKey = null, 
        string? lastRecordKey = null)
    {
        var startTime = DateTime.UtcNow;
        var batchId = Guid.NewGuid().ToString();

        // Use raw SQL for reliable insert
        var sql = """
            INSERT INTO external."SyncBatchLogs" 
                ("SyncExecutionId", "BatchId", "BatchNumber", "StartTime", "Status", 
                 "RecordsInBatch", "RecordsInserted", "RecordsUpdated", "RecordsFailed", 
                 "RecordsSkipped", "FirstRecordKey", "LastRecordKey")
            VALUES 
                (@executionId, @batchId, @batchNumber, @startTime, @status, 
                 @recordsInBatch, 0, 0, 0, 0, @firstRecordKey, @lastRecordKey)
            RETURNING "Id";
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@executionId", executionId),
            CreateParameter(command, "@batchId", batchId),
            CreateParameter(command, "@batchNumber", batchNumber),
            CreateParameter(command, "@startTime", startTime),
            CreateParameter(command, "@status", (int)SyncBatchStatus.Processing),
            CreateParameter(command, "@recordsInBatch", recordCount),
            CreateParameter(command, "@firstRecordKey", firstRecordKey),
            CreateParameter(command, "@lastRecordKey", lastRecordKey)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        long batchLogId;
        try
        {
            _logger.LogDebug("Executing batch SQL: {Sql}", sql);
            
            var result = await command.ExecuteScalarAsync();
            _logger.LogDebug("Batch ExecuteScalarAsync returned: {Result} (Type: {Type})", result, result?.GetType()?.Name ?? "null");
            
            if (result == null)
            {
                throw new InvalidOperationException("ExecuteScalarAsync returned null - no batch ID was returned from INSERT");
            }
            
            if (result is long longResult)
            {
                batchLogId = longResult;
            }
            else if (long.TryParse(result.ToString(), out var parsedId))
            {
                batchLogId = parsedId;
            }
            else
            {
                throw new InvalidOperationException($"Batch ExecuteScalarAsync returned unexpected type {result.GetType().Name} with value '{result}' - expected long");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute sync batch log insert SQL");
            throw new InvalidOperationException("Failed to create sync batch log", ex);
        }

        // Return a new SyncBatchLog object with the inserted data
        var batchLog = new SyncBatchLog
        {
            Id = batchLogId,
            SyncExecutionId = executionId,
            BatchId = batchId,
            BatchNumber = batchNumber,
            StartTime = startTime,
            Status = SyncBatchStatus.Processing,
            RecordsInBatch = recordCount,
            FirstRecordKey = firstRecordKey,
            LastRecordKey = lastRecordKey
        };

        return batchLog;
    }

    public async Task CompleteBatchAsync(long batchId, SyncBatchResult result)
    {
        var endTime = DateTime.UtcNow;
        var status = result.HasErrors ? SyncBatchStatus.PartiallyCompleted : SyncBatchStatus.Completed;

        _logger.LogDebug("Completing batch {BatchId} with {RecordsInserted} inserted, {RecordsUpdated} updated, {RecordsFailed} failed (DB: {DbDuration}ms, FK: {FkDuration}ms)",
            batchId, result.RecordsInserted, result.RecordsUpdated, result.RecordsFailed,
            result.DatabaseOperationDuration?.TotalMilliseconds ?? 0,
            result.ForeignKeyResolutionDuration?.TotalMilliseconds ?? 0);

        // Use raw SQL for reliable update
        var sql = """
            UPDATE external."SyncBatchLogs" 
            SET "EndTime" = @endTime,
                "Status" = @status,
                "RecordsInserted" = @recordsInserted,
                "RecordsUpdated" = @recordsUpdated,
                "RecordsFailed" = @recordsFailed,
                "RecordsSkipped" = @recordsSkipped,
                "ProcessingDuration" = @endTime - "StartTime",
                "DatabaseOperationDuration" = @dbDuration,
                "ForeignKeyResolutionDuration" = @fkDuration
            WHERE "Id" = @batchId
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@batchId", batchId),
            CreateParameter(command, "@endTime", endTime),
            CreateParameter(command, "@status", (int)status),
            CreateParameter(command, "@recordsInserted", result.RecordsInserted),
            CreateParameter(command, "@recordsUpdated", result.RecordsUpdated),
            CreateParameter(command, "@recordsFailed", result.RecordsFailed),
            CreateParameter(command, "@recordsSkipped", result.RecordsSkipped),
            CreateParameter(command, "@dbDuration", result.DatabaseOperationDuration),
            CreateParameter(command, "@fkDuration", result.ForeignKeyResolutionDuration)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync();
        _logger.LogInformation("RAW SQL: Updated {RowsAffected} batch record(s)", rowsAffected);
    }

    public async Task LogErrorAsync(
        long executionId, 
        long? batchId, 
        SyncError error, 
        string? recordKey = null, 
        string? recordData = null)
    {
        var errorType = DetermineErrorType(error);
        var errorCode = error.Code ?? "UNKNOWN";
        var errorMessage = error.Message;
        var errorDetails = error.Exception?.ToString();
        var stackTrace = error.Exception?.StackTrace;
        var recordKeyValue = recordKey ?? error.RecordKey;
        var recordDataValue = recordData ?? error.RecordData;
        var fieldName = error.FieldName;
        var fieldValue = error.FieldValue;
        var occurredAt = DateTime.UtcNow;

        // Use raw SQL for reliable insert
        var sql = """
            INSERT INTO external."SyncErrorLogs" 
                ("SyncExecutionId", "SyncBatchId", "OccurredAt", "ErrorType", "ErrorCode", 
                 "ErrorMessage", "ErrorDetails", "StackTrace", "RecordKey", "RecordData", 
                 "FieldName", "FieldValue", "IsResolved")
            VALUES 
                (@executionId, @batchId, @occurredAt, @errorType, @errorCode, 
                 @errorMessage, @errorDetails, @stackTrace, @recordKey, @recordData, 
                 @fieldName, @fieldValue, false)
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@executionId", executionId),
            CreateParameter(command, "@batchId", batchId),
            CreateParameter(command, "@occurredAt", occurredAt),
            CreateParameter(command, "@errorType", (int)errorType),
            CreateParameter(command, "@errorCode", errorCode),
            CreateParameter(command, "@errorMessage", errorMessage),
            CreateParameter(command, "@errorDetails", errorDetails),
            CreateParameter(command, "@stackTrace", stackTrace),
            CreateParameter(command, "@recordKey", recordKeyValue),
            CreateParameter(command, "@recordData", recordDataValue),
            CreateParameter(command, "@fieldName", fieldName),
            CreateParameter(command, "@fieldValue", fieldValue)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        await command.ExecuteNonQueryAsync();

        _logger.LogError("Logged sync error {ErrorCode}: {ErrorMessage} for execution {ExecutionId}",
            errorCode, errorMessage, executionId);
    }

    public async Task<DateTime?> GetLastSuccessfulSyncDateAsync(string configurationName)
    {
        var sql = """
            SELECT "LastIncrementalSyncDate", "LastSuccessfulExecution"
            FROM external."ConfigurationExecutionHistories"
            WHERE "ConfigurationName" = @configurationName
            LIMIT 1
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(CreateParameter(command, "@configurationName", configurationName));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var lastIncrementalSync = reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0);
            var lastSuccessfulExecution = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
            return lastIncrementalSync ?? lastSuccessfulExecution;
        }

        return null;
    }

    public async Task UpdateConfigurationHistoryAsync(string configurationName, SyncResult result)
    {
        var duration = result.EndTime - result.StartTime;
        var currentTime = DateTime.UtcNow;
        var recordsPerMinute = duration.TotalMinutes > 0 ? (decimal)(result.TotalRecordsExtracted / duration.TotalMinutes) : 0m;

        // Use UPSERT (INSERT ... ON CONFLICT) to handle both insert and update cases
        var sql = """
            INSERT INTO external."ConfigurationExecutionHistories" 
                ("ConfigurationName", "TotalExecutions", "SuccessfulExecutions", "FailedExecutions", 
                 "TotalRecordsProcessed", "LastSuccessfulExecution", "LastFailedExecution", 
                 "LastIncrementalSyncDate", "AverageExecutionDuration", "AverageRecordsPerMinute", 
                 "CreatedAt", "LastUpdatedAt")
            VALUES 
                (@configName, 1, @successfulInc, @failedInc, @totalRecords, @lastSuccessful, @lastFailed, 
                 @lastIncremental, @duration, @recordsPerMinute, @currentTime, @currentTime)
            ON CONFLICT ("ConfigurationName") 
            DO UPDATE SET 
                "TotalExecutions" = external."ConfigurationExecutionHistories"."TotalExecutions" + 1,
                "SuccessfulExecutions" = external."ConfigurationExecutionHistories"."SuccessfulExecutions" + @successfulInc,
                "FailedExecutions" = external."ConfigurationExecutionHistories"."FailedExecutions" + @failedInc,
                "TotalRecordsProcessed" = external."ConfigurationExecutionHistories"."TotalRecordsProcessed" + @totalRecords,
                "LastSuccessfulExecution" = CASE WHEN @successfulInc > 0 THEN @currentTime ELSE external."ConfigurationExecutionHistories"."LastSuccessfulExecution" END,
                "LastFailedExecution" = CASE WHEN @failedInc > 0 THEN @currentTime ELSE external."ConfigurationExecutionHistories"."LastFailedExecution" END,
                "LastIncrementalSyncDate" = CASE WHEN @successfulInc > 0 THEN @currentTime ELSE external."ConfigurationExecutionHistories"."LastIncrementalSyncDate" END,
                "AverageExecutionDuration" = CASE 
                    WHEN external."ConfigurationExecutionHistories"."AverageExecutionDuration" IS NULL THEN @duration
                    ELSE make_interval(secs => (EXTRACT(epoch FROM external."ConfigurationExecutionHistories"."AverageExecutionDuration") + EXTRACT(epoch FROM @duration)) / 2)
                END,
                "AverageRecordsPerMinute" = CASE 
                    WHEN external."ConfigurationExecutionHistories"."AverageRecordsPerMinute" IS NULL THEN @recordsPerMinute
                    WHEN @recordsPerMinute > 0 THEN (external."ConfigurationExecutionHistories"."AverageRecordsPerMinute" + @recordsPerMinute) / 2
                    ELSE external."ConfigurationExecutionHistories"."AverageRecordsPerMinute"
                END,
                "LastUpdatedAt" = @currentTime
            RETURNING "TotalExecutions", "SuccessfulExecutions"
            """;

        var isSuccessful = result.Status == SyncStatus.Completed || result.Status == SyncStatus.CompletedWithErrors;
        var successfulInc = isSuccessful ? 1 : 0;
        var failedInc = isSuccessful ? 0 : 1;
        var lastSuccessful = isSuccessful ? currentTime : (DateTime?)null;
        var lastFailed = isSuccessful ? (DateTime?)null : currentTime;
        var lastIncremental = isSuccessful ? currentTime : (DateTime?)null;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameters = new[]
        {
            CreateParameter(command, "@configName", configurationName),
            CreateParameter(command, "@successfulInc", successfulInc),
            CreateParameter(command, "@failedInc", failedInc),
            CreateParameter(command, "@totalRecords", result.TotalRecordsExtracted),
            CreateParameter(command, "@lastSuccessful", lastSuccessful),
            CreateParameter(command, "@lastFailed", lastFailed),
            CreateParameter(command, "@lastIncremental", lastIncremental),
            CreateParameter(command, "@duration", duration),
            CreateParameter(command, "@recordsPerMinute", recordsPerMinute),
            CreateParameter(command, "@currentTime", currentTime)
        };

        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var totalExecutions = reader.GetInt32(0);
            var successfulExecutions = reader.GetInt32(1);

            _logger.LogInformation("Updated configuration history for {ConfigurationName}: {TotalExecutions} total, {SuccessfulExecutions} successful",
                configurationName, totalExecutions, successfulExecutions);
        }
    }

    public async Task<IEnumerable<SyncExecutionLog>> GetRecentExecutionsAsync(string? configurationName = null, int count = 50)
    {
        // First, get the execution logs
        var executionSql = """
            SELECT "Id", "ConfigurationName", "StartTime", "EndTime", "Status", "StatusMessage",
                   "TotalRecordsExtracted", "RecordsInserted", "RecordsUpdated", "RecordsFailed", "RecordsDeleted",
                   "BatchesProcessed", "BatchesFailed", "ConfigurationSnapshot", "SourceQuery", "DestinationTable",
                   "TriggeredBy", "TriggerId", "CloudRunInstance", "Duration", "MemoryUsedMB", "CpuTimeSeconds",
                   "CreatedAt", "LastUpdatedAt"
            FROM external."SyncExecutionLogs"
            WHERE (@configName IS NULL OR "ConfigurationName" = @configName)
            ORDER BY "StartTime" DESC
            LIMIT @count
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var executionCommand = connection.CreateCommand();
        executionCommand.CommandText = executionSql;
        executionCommand.Parameters.Add(CreateParameter(executionCommand, "@configName", string.IsNullOrEmpty(configurationName) ? null : configurationName));
        executionCommand.Parameters.Add(CreateParameter(executionCommand, "@count", count));

        var executions = new List<SyncExecutionLog>();
        var executionIds = new List<long>();

        using var executionReader = await executionCommand.ExecuteReaderAsync();
        while (await executionReader.ReadAsync())
        {
            var execution = new SyncExecutionLog
            {
                Id = executionReader.GetInt64(0),
                ConfigurationName = executionReader.GetString(1),
                StartTime = executionReader.GetDateTime(2),
                EndTime = executionReader.IsDBNull(3) ? null : executionReader.GetDateTime(3),
                Status = (SyncStatus)executionReader.GetInt32(4),
                StatusMessage = executionReader.IsDBNull(5) ? null : executionReader.GetString(5),
                TotalRecordsExtracted = executionReader.GetInt32(6),
                RecordsInserted = executionReader.GetInt32(7),
                RecordsUpdated = executionReader.GetInt32(8),
                RecordsFailed = executionReader.GetInt32(9),
                RecordsDeleted = executionReader.GetInt32(10),
                BatchesProcessed = executionReader.GetInt32(11),
                BatchesFailed = executionReader.GetInt32(12),
                ConfigurationSnapshot = executionReader.GetString(13),
                SourceQuery = executionReader.GetString(14),
                DestinationTable = executionReader.GetString(15),
                TriggeredBy = executionReader.IsDBNull(16) ? null : executionReader.GetString(16),
                TriggerId = executionReader.IsDBNull(17) ? null : executionReader.GetString(17),
                CloudRunInstance = executionReader.IsDBNull(18) ? null : executionReader.GetString(18),
                Duration = executionReader.IsDBNull(19) ? null : executionReader.GetFieldValue<TimeSpan>(19),
                MemoryUsedMB = executionReader.IsDBNull(20) ? null : executionReader.GetInt64(20),
                CpuTimeSeconds = executionReader.IsDBNull(21) ? null : executionReader.GetDecimal(21),
                CreatedAt = executionReader.GetDateTime(22),
                LastUpdatedAt = executionReader.IsDBNull(23) ? null : executionReader.GetDateTime(23),
                BatchLogs = new List<SyncBatchLog>(),
                ErrorLogs = new List<SyncErrorLog>()
            };
            
            executions.Add(execution);
            executionIds.Add(execution.Id);
        }
        executionReader.Close();

        if (executionIds.Count == 0)
            return executions;

        // Get batch logs for these executions
        var batchSql = $"""
            SELECT "Id", "SyncExecutionId", "BatchId", "BatchNumber", "StartTime", "EndTime", "Status", "StatusMessage",
                   "RecordsInBatch", "RecordsInserted", "RecordsUpdated", "RecordsFailed", "RecordsSkipped",
                   "ProcessingDuration", "DatabaseOperationDuration", "ForeignKeyResolutionDuration",
                   "FirstRecordKey", "LastRecordKey"
            FROM external."SyncBatchLogs"
            WHERE "SyncExecutionId" = ANY(@executionIds)
            ORDER BY "SyncExecutionId", "BatchNumber"
            """;

        using var batchCommand = connection.CreateCommand();
        batchCommand.CommandText = batchSql;
        batchCommand.Parameters.Add(CreateParameter(batchCommand, "@executionIds", executionIds.ToArray()));

        var batchesByExecutionId = new Dictionary<long, List<SyncBatchLog>>();
        using var batchReader = await batchCommand.ExecuteReaderAsync();
        while (await batchReader.ReadAsync())
        {
            var executionId = batchReader.GetInt64(1);
            var batch = new SyncBatchLog
            {
                Id = batchReader.GetInt64(0),
                SyncExecutionId = executionId,
                BatchId = batchReader.GetString(2),
                BatchNumber = batchReader.GetInt32(3),
                StartTime = batchReader.GetDateTime(4),
                EndTime = batchReader.IsDBNull(5) ? null : batchReader.GetDateTime(5),
                Status = (SyncBatchStatus)batchReader.GetInt32(6),
                StatusMessage = batchReader.IsDBNull(7) ? null : batchReader.GetString(7),
                RecordsInBatch = batchReader.GetInt32(8),
                RecordsInserted = batchReader.GetInt32(9),
                RecordsUpdated = batchReader.GetInt32(10),
                RecordsFailed = batchReader.GetInt32(11),
                RecordsSkipped = batchReader.GetInt32(12),
                ProcessingDuration = batchReader.IsDBNull(13) ? null : batchReader.GetFieldValue<TimeSpan>(13),
                DatabaseOperationDuration = batchReader.IsDBNull(14) ? null : batchReader.GetFieldValue<TimeSpan>(14),
                ForeignKeyResolutionDuration = batchReader.IsDBNull(15) ? null : batchReader.GetFieldValue<TimeSpan>(15),
                FirstRecordKey = batchReader.IsDBNull(16) ? null : batchReader.GetString(16),
                LastRecordKey = batchReader.IsDBNull(17) ? null : batchReader.GetString(17),
                ErrorLogs = new List<SyncErrorLog>()
            };

            if (!batchesByExecutionId.ContainsKey(executionId))
                batchesByExecutionId[executionId] = new List<SyncBatchLog>();
            batchesByExecutionId[executionId].Add(batch);
        }
        batchReader.Close();

        // Get error logs for these executions
        var errorSql = """
            SELECT "Id", "SyncExecutionId", "SyncBatchId", "OccurredAt", "ErrorType", "ErrorCode",
                   "ErrorMessage", "ErrorDetails", "StackTrace", "RecordKey", "RecordData",
                   "FieldName", "FieldValue", "IsResolved", "ResolvedAt", "ResolutionNotes"
            FROM external."SyncErrorLogs"
            WHERE "SyncExecutionId" = ANY(@executionIds)
            ORDER BY "SyncExecutionId", "OccurredAt"
            """;

        using var errorCommand = connection.CreateCommand();
        errorCommand.CommandText = errorSql;
        errorCommand.Parameters.Add(CreateParameter(errorCommand, "@executionIds", executionIds.ToArray()));

        var errorsByExecutionId = new Dictionary<long, List<SyncErrorLog>>();
        using var errorReader = await errorCommand.ExecuteReaderAsync();
        while (await errorReader.ReadAsync())
        {
            var executionId = errorReader.GetInt64(1);
            var error = new SyncErrorLog
            {
                Id = errorReader.GetInt64(0),
                SyncExecutionId = executionId,
                SyncBatchId = errorReader.IsDBNull(2) ? null : errorReader.GetInt64(2),
                OccurredAt = errorReader.GetDateTime(3),
                ErrorType = (SyncErrorType)errorReader.GetInt32(4),
                ErrorCode = errorReader.GetString(5),
                ErrorMessage = errorReader.GetString(6),
                ErrorDetails = errorReader.IsDBNull(7) ? null : errorReader.GetString(7),
                StackTrace = errorReader.IsDBNull(8) ? null : errorReader.GetString(8),
                RecordKey = errorReader.IsDBNull(9) ? null : errorReader.GetString(9),
                RecordData = errorReader.IsDBNull(10) ? null : errorReader.GetString(10),
                FieldName = errorReader.IsDBNull(11) ? null : errorReader.GetString(11),
                FieldValue = errorReader.IsDBNull(12) ? null : errorReader.GetString(12),
                IsResolved = errorReader.GetBoolean(13),
                ResolvedAt = errorReader.IsDBNull(14) ? null : errorReader.GetDateTime(14),
                ResolutionNotes = errorReader.IsDBNull(15) ? null : errorReader.GetString(15)
            };

            if (!errorsByExecutionId.ContainsKey(executionId))
                errorsByExecutionId[executionId] = new List<SyncErrorLog>();
            errorsByExecutionId[executionId].Add(error);
        }

        // Assign the related data to executions
        foreach (var execution in executions)
        {
            if (batchesByExecutionId.TryGetValue(execution.Id, out var batches))
                execution.BatchLogs = batches;

            if (errorsByExecutionId.TryGetValue(execution.Id, out var errors))
                execution.ErrorLogs = errors;
        }

        return executions;
    }

    public async Task<SyncExecutionLog?> GetExecutionAsync(long executionId)
    {
        // Get the execution log
        var executionSql = """
            SELECT "Id", "ConfigurationName", "StartTime", "EndTime", "Status", "StatusMessage",
                   "TotalRecordsExtracted", "RecordsInserted", "RecordsUpdated", "RecordsFailed", "RecordsDeleted",
                   "BatchesProcessed", "BatchesFailed", "ConfigurationSnapshot", "SourceQuery", "DestinationTable",
                   "TriggeredBy", "TriggerId", "CloudRunInstance", "Duration", "MemoryUsedMB", "CpuTimeSeconds",
                   "CreatedAt", "LastUpdatedAt"
            FROM external."SyncExecutionLogs"
            WHERE "Id" = @executionId
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var executionCommand = connection.CreateCommand();
        executionCommand.CommandText = executionSql;
        executionCommand.Parameters.Add(CreateParameter(executionCommand, "@executionId", executionId));

        SyncExecutionLog? execution = null;
        using var executionReader = await executionCommand.ExecuteReaderAsync();
        if (await executionReader.ReadAsync())
        {
            execution = new SyncExecutionLog
            {
                Id = executionReader.GetInt64(0),
                ConfigurationName = executionReader.GetString(1),
                StartTime = executionReader.GetDateTime(2),
                EndTime = executionReader.IsDBNull(3) ? null : executionReader.GetDateTime(3),
                Status = (SyncStatus)executionReader.GetInt32(4),
                StatusMessage = executionReader.IsDBNull(5) ? null : executionReader.GetString(5),
                TotalRecordsExtracted = executionReader.GetInt32(6),
                RecordsInserted = executionReader.GetInt32(7),
                RecordsUpdated = executionReader.GetInt32(8),
                RecordsFailed = executionReader.GetInt32(9),
                RecordsDeleted = executionReader.GetInt32(10),
                BatchesProcessed = executionReader.GetInt32(11),
                BatchesFailed = executionReader.GetInt32(12),
                ConfigurationSnapshot = executionReader.GetString(13),
                SourceQuery = executionReader.GetString(14),
                DestinationTable = executionReader.GetString(15),
                TriggeredBy = executionReader.IsDBNull(16) ? null : executionReader.GetString(16),
                TriggerId = executionReader.IsDBNull(17) ? null : executionReader.GetString(17),
                CloudRunInstance = executionReader.IsDBNull(18) ? null : executionReader.GetString(18),
                Duration = executionReader.IsDBNull(19) ? null : executionReader.GetFieldValue<TimeSpan>(19),
                MemoryUsedMB = executionReader.IsDBNull(20) ? null : executionReader.GetInt64(20),
                CpuTimeSeconds = executionReader.IsDBNull(21) ? null : executionReader.GetDecimal(21),
                CreatedAt = executionReader.GetDateTime(22),
                LastUpdatedAt = executionReader.IsDBNull(23) ? null : executionReader.GetDateTime(23),
                BatchLogs = new List<SyncBatchLog>(),
                ErrorLogs = new List<SyncErrorLog>()
            };
        }
        executionReader.Close();

        if (execution == null)
            return null;

        // Get batch logs for this execution
        var batchSql = """
            SELECT "Id", "SyncExecutionId", "BatchId", "BatchNumber", "StartTime", "EndTime", "Status", "StatusMessage",
                   "RecordsInBatch", "RecordsInserted", "RecordsUpdated", "RecordsFailed", "RecordsSkipped",
                   "ProcessingDuration", "DatabaseOperationDuration", "ForeignKeyResolutionDuration",
                   "FirstRecordKey", "LastRecordKey"
            FROM external."SyncBatchLogs"
            WHERE "SyncExecutionId" = @executionId
            ORDER BY "BatchNumber"
            """;

        using var batchCommand = connection.CreateCommand();
        batchCommand.CommandText = batchSql;
        batchCommand.Parameters.Add(CreateParameter(batchCommand, "@executionId", executionId));

        var batches = new List<SyncBatchLog>();
        using var batchReader = await batchCommand.ExecuteReaderAsync();
        while (await batchReader.ReadAsync())
        {
            var batch = new SyncBatchLog
            {
                Id = batchReader.GetInt64(0),
                SyncExecutionId = batchReader.GetInt64(1),
                BatchId = batchReader.GetString(2),
                BatchNumber = batchReader.GetInt32(3),
                StartTime = batchReader.GetDateTime(4),
                EndTime = batchReader.IsDBNull(5) ? null : batchReader.GetDateTime(5),
                Status = (SyncBatchStatus)batchReader.GetInt32(6),
                StatusMessage = batchReader.IsDBNull(7) ? null : batchReader.GetString(7),
                RecordsInBatch = batchReader.GetInt32(8),
                RecordsInserted = batchReader.GetInt32(9),
                RecordsUpdated = batchReader.GetInt32(10),
                RecordsFailed = batchReader.GetInt32(11),
                RecordsSkipped = batchReader.GetInt32(12),
                ProcessingDuration = batchReader.IsDBNull(13) ? null : batchReader.GetFieldValue<TimeSpan>(13),
                DatabaseOperationDuration = batchReader.IsDBNull(14) ? null : batchReader.GetFieldValue<TimeSpan>(14),
                ForeignKeyResolutionDuration = batchReader.IsDBNull(15) ? null : batchReader.GetFieldValue<TimeSpan>(15),
                FirstRecordKey = batchReader.IsDBNull(16) ? null : batchReader.GetString(16),
                LastRecordKey = batchReader.IsDBNull(17) ? null : batchReader.GetString(17),
                ErrorLogs = new List<SyncErrorLog>()
            };
            batches.Add(batch);
        }
        execution.BatchLogs = batches;
        batchReader.Close();

        // Get error logs for this execution
        var errorSql = """
            SELECT "Id", "SyncExecutionId", "SyncBatchId", "OccurredAt", "ErrorType", "ErrorCode",
                   "ErrorMessage", "ErrorDetails", "StackTrace", "RecordKey", "RecordData",
                   "FieldName", "FieldValue", "IsResolved", "ResolvedAt", "ResolutionNotes"
            FROM external."SyncErrorLogs"
            WHERE "SyncExecutionId" = @executionId
            ORDER BY "OccurredAt"
            """;

        using var errorCommand = connection.CreateCommand();
        errorCommand.CommandText = errorSql;
        errorCommand.Parameters.Add(CreateParameter(errorCommand, "@executionId", executionId));

        var errors = new List<SyncErrorLog>();
        using var errorReader = await errorCommand.ExecuteReaderAsync();
        while (await errorReader.ReadAsync())
        {
            var error = new SyncErrorLog
            {
                Id = errorReader.GetInt64(0),
                SyncExecutionId = errorReader.GetInt64(1),
                SyncBatchId = errorReader.IsDBNull(2) ? null : errorReader.GetInt64(2),
                OccurredAt = errorReader.GetDateTime(3),
                ErrorType = (SyncErrorType)errorReader.GetInt32(4),
                ErrorCode = errorReader.GetString(5),
                ErrorMessage = errorReader.GetString(6),
                ErrorDetails = errorReader.IsDBNull(7) ? null : errorReader.GetString(7),
                StackTrace = errorReader.IsDBNull(8) ? null : errorReader.GetString(8),
                RecordKey = errorReader.IsDBNull(9) ? null : errorReader.GetString(9),
                RecordData = errorReader.IsDBNull(10) ? null : errorReader.GetString(10),
                FieldName = errorReader.IsDBNull(11) ? null : errorReader.GetString(11),
                FieldValue = errorReader.IsDBNull(12) ? null : errorReader.GetString(12),
                IsResolved = errorReader.GetBoolean(13),
                ResolvedAt = errorReader.IsDBNull(14) ? null : errorReader.GetDateTime(14),
                ResolutionNotes = errorReader.IsDBNull(15) ? null : errorReader.GetString(15)
            };
            errors.Add(error);
        }
        execution.ErrorLogs = errors;

        return execution;
    }

    public async Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(string? configurationName = null)
    {
        string sql;
        
        if (string.IsNullOrEmpty(configurationName))
        {
            // Query for all configurations without parameters
            sql = """
                SELECT e."Id", e."SyncExecutionId", e."SyncBatchId", e."OccurredAt", e."ErrorType", e."ErrorCode",
                       e."ErrorMessage", e."ErrorDetails", e."StackTrace", e."RecordKey", e."RecordData",
                       e."FieldName", e."FieldValue", e."IsResolved", e."ResolvedAt", e."ResolutionNotes"
                FROM external."SyncErrorLogs" e
                INNER JOIN external."SyncExecutionLogs" se ON e."SyncExecutionId" = se."Id"
                WHERE (e."IsResolved" = false OR e."IsResolved" IS NULL)
                ORDER BY e."OccurredAt" DESC
                """;
        }
        else
        {
            // Query for specific configuration with parameter
            sql = """
                SELECT e."Id", e."SyncExecutionId", e."SyncBatchId", e."OccurredAt", e."ErrorType", e."ErrorCode",
                       e."ErrorMessage", e."ErrorDetails", e."StackTrace", e."RecordKey", e."RecordData",
                       e."FieldName", e."FieldValue", e."IsResolved", e."ResolvedAt", e."ResolutionNotes"
                FROM external."SyncErrorLogs" e
                INNER JOIN external."SyncExecutionLogs" se ON e."SyncExecutionId" = se."Id"
                WHERE (e."IsResolved" = false OR e."IsResolved" IS NULL)
                  AND se."ConfigurationName" = @configName
                ORDER BY e."OccurredAt" DESC
                """;
        }

        _logger.LogDebug("Getting unresolved errors for configurationName: {ConfigName}", configurationName ?? "ALL");

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        // Only add parameter if we have a specific configuration name
        if (!string.IsNullOrEmpty(configurationName))
        {
            command.Parameters.Add(CreateParameter(command, "@configName", configurationName));
        }

        var errors = new List<SyncErrorLog>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var error = new SyncErrorLog
            {
                Id = reader.GetInt64(0),
                SyncExecutionId = reader.GetInt64(1),
                SyncBatchId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                OccurredAt = reader.GetDateTime(3),
                ErrorType = (SyncErrorType)reader.GetInt32(4),
                ErrorCode = reader.GetString(5),
                ErrorMessage = reader.GetString(6),
                ErrorDetails = reader.IsDBNull(7) ? null : reader.GetString(7),
                StackTrace = reader.IsDBNull(8) ? null : reader.GetString(8),
                RecordKey = reader.IsDBNull(9) ? null : reader.GetString(9),
                RecordData = reader.IsDBNull(10) ? null : reader.GetString(10),
                FieldName = reader.IsDBNull(11) ? null : reader.GetString(11),
                FieldValue = reader.IsDBNull(12) ? null : reader.GetString(12),
                IsResolved = reader.GetBoolean(13),
                ResolvedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                ResolutionNotes = reader.IsDBNull(15) ? null : reader.GetString(15)
            };
            errors.Add(error);
        }

        _logger.LogInformation("Found {ErrorCount} unresolved errors for configurationName: {ConfigName}", 
            errors.Count, configurationName ?? "ALL");

        return errors;
    }

    public async Task<Dictionary<string, object>> GetSyncStatisticsAsync(string? configurationName = null, DateTime? fromDate = null)
    {
        var sql = """
            SELECT 
                COUNT(*) as TotalExecutions,
                COUNT(CASE WHEN "Status" IN (2, 3) THEN 1 END) as SuccessfulExecutions,
                COUNT(CASE WHEN "Status" = 4 THEN 1 END) as FailedExecutions,
                COALESCE(SUM("TotalRecordsExtracted"), 0) as TotalRecordsProcessed,
                COALESCE(SUM("RecordsInserted"), 0) as TotalRecordsInserted,
                COALESCE(SUM("RecordsUpdated"), 0) as TotalRecordsUpdated,
                COALESCE(SUM("RecordsFailed"), 0) as TotalRecordsFailed,
                COALESCE(AVG(EXTRACT(epoch FROM "Duration") / 60.0), 0) as AverageDurationMinutes
            FROM external."SyncExecutionLogs"
            WHERE (@configName IS NULL OR "ConfigurationName" = @configName)
              AND (@fromDate IS NULL OR "StartTime" >= @fromDate)
            """;

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(CreateParameter(command, "@configName", string.IsNullOrEmpty(configurationName) ? null : configurationName));
        command.Parameters.Add(CreateParameter(command, "@fromDate", fromDate));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var totalExecutions = reader.GetInt32(0);
            var successfulExecutions = reader.GetInt32(1);
            var failedExecutions = reader.GetInt32(2);
            var totalRecordsProcessed = reader.GetInt64(3);
            var totalRecordsInserted = reader.GetInt64(4);
            var totalRecordsUpdated = reader.GetInt64(5);
            var totalRecordsFailed = reader.GetInt64(6);
            var averageDuration = reader.GetDouble(7);

            return new Dictionary<string, object>
            {
                ["totalExecutions"] = totalExecutions,
                ["successfulExecutions"] = successfulExecutions,
                ["failedExecutions"] = failedExecutions,
                ["successRate"] = totalExecutions > 0 ? (double)successfulExecutions / totalExecutions * 100 : 0,
                ["totalRecordsProcessed"] = totalRecordsProcessed,
                ["totalRecordsInserted"] = totalRecordsInserted,
                ["totalRecordsUpdated"] = totalRecordsUpdated,
                ["totalRecordsFailed"] = totalRecordsFailed,
                ["averageDurationMinutes"] = averageDuration
            };
        }

        return new Dictionary<string, object>();
    }

    private SyncErrorType DetermineErrorType(SyncError error)
    {
        if (error.Exception != null)
        {
            var exceptionType = error.Exception.GetType().Name.ToLower();
            if (exceptionType.Contains("sql") || exceptionType.Contains("database"))
                return SyncErrorType.DatabaseInsertError;
            if (exceptionType.Contains("timeout"))
                return SyncErrorType.TimeoutError;
            if (exceptionType.Contains("connection"))
                return SyncErrorType.ConnectionError;
        }

        if (error.Code?.Contains("FK", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.ForeignKeyResolutionError;
        if (error.Code?.Contains("VALIDATION", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.ValidationError;
        if (error.Code?.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.TimeoutError;
        if (error.Code?.Contains("QUERY", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.QueryError;
        if (error.Code?.Contains("CONNECTION", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.ConnectionError;
        if (error.Code?.Contains("TRANSFORM", StringComparison.OrdinalIgnoreCase) == true) 
            return SyncErrorType.DataTransformationError;
        
        return SyncErrorType.UnknownError;
    }

    public async Task LogProcessedRecordAsync(long batchId, string primaryKey, string recordAction, string? errorMessage = null)
    {
        var sql = @"
            INSERT INTO external.""SyncProcessedRecords"" (""BatchId"", ""PrimaryKey"", ""RecordAction"", ""ErrorMessage"", ""ProcessedDate"")
            VALUES (@batchId, @primaryKey, @recordAction, @errorMessage, @processedDate)";

        await _monitoringDbService.ExecuteCommandAsync(sql, new
        {
            batchId,
            primaryKey,
            recordAction,
            errorMessage,
            processedDate = DateTime.UtcNow
        });
    }

    public async Task LogProcessedRecordsAsync(long batchId, IEnumerable<(string PrimaryKey, string RecordAction, string? ErrorMessage)> processedRecords)
    {
        var records = processedRecords.ToList();
        if (!records.Any()) return;

        var sql = @"
            INSERT INTO external.""SyncProcessedRecords"" (""BatchId"", ""PrimaryKey"", ""RecordAction"", ""ErrorMessage"", ""ProcessedDate"")
            VALUES ";

        var valuesClauses = new List<string>();
        var parameters = new Dictionary<string, object?>
        {
            ["@batchId"] = batchId,
            ["@processedDate"] = DateTime.UtcNow
        };

        for (int i = 0; i < records.Count; i++)
        {
            valuesClauses.Add($"(@batchId, @primaryKey{i}, @recordAction{i}, @errorMessage{i}, @processedDate)");
            parameters[$"@primaryKey{i}"] = records[i].PrimaryKey;
            parameters[$"@recordAction{i}"] = records[i].RecordAction;
            parameters[$"@errorMessage{i}"] = records[i].ErrorMessage;
        }

        sql += string.Join(", ", valuesClauses);

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var param in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = param.Key;
            parameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();
    }

    public async Task<HashSet<string>> GetProcessedPrimaryKeysForExecutionAsync(long executionId)
    {
        var sql = @"
            SELECT DISTINCT spr.""PrimaryKey""
            FROM external.""SyncProcessedRecords"" spr
            INNER JOIN external.""SyncBatchLogs"" sbl ON spr.""BatchId"" = sbl.""Id""
            WHERE sbl.""SyncExecutionId"" = @executionId
              AND spr.""RecordAction"" IN ('inserted', 'updated')";

        var processedKeys = new HashSet<string>();

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@executionId";
        parameter.Value = executionId;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            processedKeys.Add(reader.GetString(0));
        }

        return processedKeys;
    }

    public async Task<List<string>> GetProcessedPrimaryKeysForBatchAsync(long batchId)
    {
        var sql = @"
            SELECT spr.""PrimaryKey""
            FROM external.""SyncProcessedRecords"" spr
            WHERE spr.""BatchId"" = @batchId
            ORDER BY spr.""ProcessedDate""";

        var processedKeys = new List<string>();

        using var connection = await _monitoringDbService.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@batchId";
        parameter.Value = batchId;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            processedKeys.Add(reader.GetString(0));
        }

        return processedKeys;
    }
    
    public async Task<int> GetBatchCountForExecutionAsync(long executionId)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM external.\"SyncBatchLogs\" WHERE \"SyncExecutionId\" = @executionId";
            
            using var connection = await _monitoringDbService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter(command, "@executionId", executionId));
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch count for execution {ExecutionId}", executionId);
            return 0;
        }
    }
    
    public async Task<int> GetErrorCountForExecutionAsync(long executionId)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM external.\"SyncErrorLogs\" WHERE \"SyncExecutionId\" = @executionId";
            
            using var connection = await _monitoringDbService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(CreateParameter(command, "@executionId", executionId));
            
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error count for execution {ExecutionId}", executionId);
            return 0;
        }
    }
    
    public async Task<(int TotalRecordsExtracted, int RecordsInserted, int RecordsUpdated, int RecordsDeleted, int RecordsFailed, int BatchCount, int BatchesProcessed, int ErrorCount)> GetLiveExecutionStatsAsync(long executionId)
    {
        try
        {
            var batchStatsSql = @"
                SELECT 
                    COALESCE(SUM(""RecordsInBatch""), 0) as TotalRecordsExtracted,
                    COALESCE(SUM(""RecordsInserted""), 0) as RecordsInserted,
                    COALESCE(SUM(""RecordsUpdated""), 0) as RecordsUpdated,
                    0 as RecordsDeleted, -- RecordsDeleted is handled at execution level, not batch level
                    COALESCE(SUM(""RecordsFailed""), 0) as RecordsFailed,
                    COUNT(*) as BatchCount,
                    COUNT(CASE WHEN ""Status"" IN (2, 3, 4, 5) THEN 1 END) as BatchesProcessed -- Completed, Failed, PartiallyCompleted, Skipped
                FROM external.""SyncBatchLogs"" 
                WHERE ""SyncExecutionId"" = @executionId";

            var errorCountSql = @"
                SELECT COUNT(*) 
                FROM external.""SyncErrorLogs"" 
                WHERE ""SyncExecutionId"" = @executionId";

            using var connection = await _monitoringDbService.GetConnectionAsync();
            
            // Get batch statistics
            using var batchCommand = connection.CreateCommand();
            batchCommand.CommandText = batchStatsSql;
            batchCommand.Parameters.Add(CreateParameter(batchCommand, "@executionId", executionId));
            
            int totalExtracted = 0, inserted = 0, updated = 0, deleted = 0, failed = 0, batchCount = 0, batchesProcessed = 0;
            
            using var batchReader = await batchCommand.ExecuteReaderAsync();
            if (await batchReader.ReadAsync())
            {
                totalExtracted = batchReader.GetInt32(0); // TotalRecordsExtracted
                inserted = batchReader.GetInt32(1); // RecordsInserted
                updated = batchReader.GetInt32(2); // RecordsUpdated
                deleted = batchReader.GetInt32(3); // RecordsDeleted
                failed = batchReader.GetInt32(4); // RecordsFailed
                batchCount = batchReader.GetInt32(5); // BatchCount
                batchesProcessed = batchReader.GetInt32(6); // BatchesProcessed
            }
            batchReader.Close();
            
            // Get error count
            using var errorCommand = connection.CreateCommand();
            errorCommand.CommandText = errorCountSql;
            errorCommand.Parameters.Add(CreateParameter(errorCommand, "@executionId", executionId));
            
            var errorResult = await errorCommand.ExecuteScalarAsync();
            int errorCount = errorResult != null ? Convert.ToInt32(errorResult) : 0;
            
            return (totalExtracted, inserted, updated, deleted, failed, batchCount, batchesProcessed, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting live execution stats for execution {ExecutionId}", executionId);
            return (0, 0, 0, 0, 0, 0, 0, 0);
        }
    }

    private static System.Data.Common.DbParameter CreateParameter(System.Data.Common.DbCommand command, string parameterName, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }
}
