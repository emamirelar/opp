using System.Data.Common;

namespace UNOPS.PAO.ExternalDataService.Infrastructure.Database;

/// <summary>
/// Simple database service for monitoring database operations
/// Handles table creation and basic operations without Entity Framework overhead
/// </summary>
public interface IMonitoringDatabaseService
{
    /// <summary>
    /// Gets a database connection to the monitoring database
    /// </summary>
    Task<DbConnection> GetConnectionAsync();
    
    /// <summary>
    /// Ensures all monitoring tables exist, creates them if they don't
    /// </summary>
    Task EnsureTablesExistAsync();
    
    /// <summary>
    /// Executes a raw SQL command
    /// </summary>
    Task<int> ExecuteCommandAsync(string sql, object? parameters = null);
    
    /// <summary>
    /// Executes a query and returns the first result
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null);
}

/// <summary>
/// Simple implementation using Npgsql directly
/// </summary>
public class MonitoringDatabaseService : IMonitoringDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<MonitoringDatabaseService> _logger;

    public MonitoringDatabaseService(string connectionString, ILogger<MonitoringDatabaseService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    public async Task<DbConnection> GetConnectionAsync()
    {
        var connection = new Npgsql.NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task EnsureTablesExistAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring monitoring tables exist...");
            
            // Create external schema if it doesn't exist
            await ExecuteCommandAsync("CREATE SCHEMA IF NOT EXISTS external;");
            
            // Check and create each table
            await CreateSyncExecutionLogsTableAsync();
            await CreateSyncBatchLogsTableAsync();
            await CreateSyncErrorLogsTableAsync();
            await CreateSyncProcessedRecordsTableAsync();
            await CreateConfigurationExecutionHistoriesTableAsync();
            
            // Apply schema migrations
            await ApplySchemaMigrationsAsync();
            
            _logger.LogInformation("Monitoring tables verified/created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure monitoring tables exist");
            throw;
        }
    }

    public async Task<int> ExecuteCommandAsync(string sql, object? parameters = null)
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 300;
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value) 
            return default;
        
        try
        {
            // Handle type conversion, especially for PostgreSQL's long/int differences
            if (typeof(T) == typeof(int) && result is long longValue)
            {
                return (T)(object)(int)longValue;
            }
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (InvalidCastException)
        {
            // Fallback for direct cast if conversion fails
            return (T)result;
        }
    }

    private async Task CreateSyncExecutionLogsTableAsync()
    {
        var tableExists = await ExecuteScalarAsync<long>(@"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = 'SyncExecutionLogs' 
            AND table_schema = 'external'");

        if (tableExists > 0)
        {
            _logger.LogDebug("SyncExecutionLogs table already exists");
            return;
        }

        var sql = @"
            CREATE TABLE external.""SyncExecutionLogs"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""ConfigurationName"" VARCHAR(255) NOT NULL,
                ""StartTime"" TIMESTAMP NOT NULL,
                ""EndTime"" TIMESTAMP NULL,
                ""Status"" INTEGER NOT NULL,
                ""StatusMessage"" VARCHAR(1000) NULL,
                ""TotalRecordsExtracted"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsInserted"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsUpdated"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsFailed"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsDeleted"" INTEGER NOT NULL DEFAULT 0,
                ""BatchesProcessed"" INTEGER NOT NULL DEFAULT 0,
                ""BatchesFailed"" INTEGER NOT NULL DEFAULT 0,
                ""ConfigurationSnapshot"" JSONB NOT NULL DEFAULT '{}',
                ""SourceQuery"" TEXT NOT NULL DEFAULT '',
                ""DestinationTable"" VARCHAR(255) NOT NULL DEFAULT '',
                ""TriggeredBy"" VARCHAR(50) NULL,
                ""TriggerId"" VARCHAR(255) NULL,
                ""CloudRunInstance"" VARCHAR(255) NULL,
                ""Duration"" INTERVAL NULL,
                ""MemoryUsedMB"" BIGINT NULL,
                ""CpuTimeSeconds"" DECIMAL(10,3) NULL,
                ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""LastUpdatedAt"" TIMESTAMP NULL
            );

            CREATE INDEX ""IX_SyncExecutionLogs_ConfigurationName"" ON external.""SyncExecutionLogs"" (""ConfigurationName"");
            CREATE INDEX ""IX_SyncExecutionLogs_StartTime"" ON external.""SyncExecutionLogs"" (""StartTime"");
            CREATE INDEX ""IX_SyncExecutionLogs_Status"" ON external.""SyncExecutionLogs"" (""Status"");
            CREATE INDEX ""IX_SyncExecutionLogs_ConfigurationName_StartTime"" ON external.""SyncExecutionLogs"" (""ConfigurationName"", ""StartTime"");";

        await ExecuteCommandAsync(sql);
        _logger.LogInformation("Created SyncExecutionLogs table");
    }

    private async Task CreateSyncBatchLogsTableAsync()
    {
        var tableExists = await ExecuteScalarAsync<long>(@"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = 'SyncBatchLogs' 
            AND table_schema = 'external'");

        if (tableExists > 0)
        {
            _logger.LogDebug("SyncBatchLogs table already exists");
            return;
        }

        var sql = @"
            CREATE TABLE external.""SyncBatchLogs"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""SyncExecutionId"" BIGINT NOT NULL,
                ""BatchId"" VARCHAR(50) NOT NULL,
                ""BatchNumber"" INTEGER NOT NULL,
                ""StartTime"" TIMESTAMP NOT NULL,
                ""EndTime"" TIMESTAMP NULL,
                ""Status"" INTEGER NOT NULL,
                ""StatusMessage"" VARCHAR(1000) NULL,
                ""RecordsInBatch"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsInserted"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsUpdated"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsFailed"" INTEGER NOT NULL DEFAULT 0,
                ""RecordsSkipped"" INTEGER NOT NULL DEFAULT 0,
                ""ProcessingDuration"" INTERVAL NULL,
                ""DatabaseOperationDuration"" INTERVAL NULL,
                ""ForeignKeyResolutionDuration"" INTERVAL NULL,
                ""FirstRecordKey"" VARCHAR(255) NULL,
                ""LastRecordKey"" VARCHAR(255) NULL,
                FOREIGN KEY (""SyncExecutionId"") REFERENCES external.""SyncExecutionLogs""(""Id"") ON DELETE CASCADE
            );

            CREATE INDEX ""IX_SyncBatchLogs_SyncExecutionId"" ON external.""SyncBatchLogs"" (""SyncExecutionId"");
            CREATE INDEX ""IX_SyncBatchLogs_StartTime"" ON external.""SyncBatchLogs"" (""StartTime"");
            CREATE INDEX ""IX_SyncBatchLogs_Status"" ON external.""SyncBatchLogs"" (""Status"");";

        await ExecuteCommandAsync(sql);
        _logger.LogInformation("Created SyncBatchLogs table");
    }

    private async Task CreateSyncErrorLogsTableAsync()
    {
        var tableExists = await ExecuteScalarAsync<long>(@"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = 'SyncErrorLogs' 
            AND table_schema = 'external'");

        if (tableExists > 0)
        {
            _logger.LogDebug("SyncErrorLogs table already exists");
            return;
        }

        var sql = @"
            CREATE TABLE external.""SyncErrorLogs"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""SyncExecutionId"" BIGINT NOT NULL,
                ""SyncBatchId"" BIGINT NULL,
                ""OccurredAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""ErrorType"" INTEGER NOT NULL,
                ""ErrorCode"" VARCHAR(50) NOT NULL,
                ""ErrorMessage"" VARCHAR(2000) NOT NULL,
                ""ErrorDetails"" TEXT NULL,
                ""StackTrace"" TEXT NULL,
                ""RecordKey"" VARCHAR(255) NULL,
                ""RecordData"" JSONB NULL,
                ""FieldName"" VARCHAR(255) NULL,
                ""FieldValue"" VARCHAR(1000) NULL,
                ""IsResolved"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""ResolvedAt"" TIMESTAMP NULL,
                ""ResolutionNotes"" VARCHAR(2000) NULL,
                FOREIGN KEY (""SyncExecutionId"") REFERENCES external.""SyncExecutionLogs""(""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""SyncBatchId"") REFERENCES external.""SyncBatchLogs""(""Id"") ON DELETE CASCADE
            );

            CREATE INDEX ""IX_SyncErrorLogs_SyncExecutionId"" ON external.""SyncErrorLogs"" (""SyncExecutionId"");
            CREATE INDEX ""IX_SyncErrorLogs_ErrorType"" ON external.""SyncErrorLogs"" (""ErrorType"");
            CREATE INDEX ""IX_SyncErrorLogs_OccurredAt"" ON external.""SyncErrorLogs"" (""OccurredAt"");
            CREATE INDEX ""IX_SyncErrorLogs_IsResolved"" ON external.""SyncErrorLogs"" (""IsResolved"");";

        await ExecuteCommandAsync(sql);
        _logger.LogInformation("Created SyncErrorLogs table");
    }

    private async Task CreateConfigurationExecutionHistoriesTableAsync()
    {
        var tableExists = await ExecuteScalarAsync<long>(@"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = 'ConfigurationExecutionHistories' 
            AND table_schema = 'external'");

        if (tableExists > 0)
        {
            _logger.LogDebug("ConfigurationExecutionHistories table already exists");
            return;
        }

        var sql = @"
            CREATE TABLE external.""ConfigurationExecutionHistories"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""ConfigurationName"" VARCHAR(255) NOT NULL,
                ""LastSuccessfulExecution"" TIMESTAMP NOT NULL,
                ""LastFailedExecution"" TIMESTAMP NULL,
                ""TotalExecutions"" INTEGER NOT NULL DEFAULT 0,
                ""SuccessfulExecutions"" INTEGER NOT NULL DEFAULT 0,
                ""FailedExecutions"" INTEGER NOT NULL DEFAULT 0,
                ""LastIncrementalSyncDate"" TIMESTAMP NULL,
                ""TotalRecordsProcessed"" BIGINT NOT NULL DEFAULT 0,
                ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""LastUpdatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""IsEnabled"" BOOLEAN NOT NULL DEFAULT TRUE,
                ""CurrentCronSchedule"" VARCHAR(100) NULL,
                ""NextScheduledExecution"" TIMESTAMP NULL,
                ""AverageExecutionDuration"" INTERVAL NULL,
                ""AverageRecordsPerMinute"" DECIMAL(10,2) NULL,
                ""AverageMemoryUsageMB"" BIGINT NULL
            );

            CREATE UNIQUE INDEX ""IX_ConfigurationExecutionHistories_ConfigurationName"" ON external.""ConfigurationExecutionHistories"" (""ConfigurationName"");
            CREATE INDEX ""IX_ConfigurationExecutionHistories_LastSuccessfulExecution"" ON external.""ConfigurationExecutionHistories"" (""LastSuccessfulExecution"");
            CREATE INDEX ""IX_ConfigurationExecutionHistories_NextScheduledExecution"" ON external.""ConfigurationExecutionHistories"" (""NextScheduledExecution"");";

        await ExecuteCommandAsync(sql);
        _logger.LogInformation("Created ConfigurationExecutionHistories table");
    }

    private async Task CreateSyncProcessedRecordsTableAsync()
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS external.""SyncProcessedRecords"" (
                ""Id"" BIGSERIAL PRIMARY KEY,
                ""BatchId"" BIGINT NOT NULL,
                ""PrimaryKey"" VARCHAR(255) NOT NULL,
                ""ProcessedDate"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ""RecordAction"" VARCHAR(50) NOT NULL,
                ""ErrorMessage"" TEXT NULL,
                FOREIGN KEY (""BatchId"") REFERENCES external.""SyncBatchLogs""(""Id"") ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ""IX_SyncProcessedRecords_BatchId"" ON external.""SyncProcessedRecords"" (""BatchId"");
            CREATE INDEX IF NOT EXISTS ""IX_SyncProcessedRecords_PrimaryKey"" ON external.""SyncProcessedRecords"" (""PrimaryKey"");
            CREATE INDEX IF NOT EXISTS ""IX_SyncProcessedRecords_ProcessedDate"" ON external.""SyncProcessedRecords"" (""ProcessedDate"");";

        await ExecuteCommandAsync(sql);
        _logger.LogInformation("Created SyncProcessedRecords table");
    }

    private void AddParameters(DbCommand command, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        foreach (var property in properties)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{property.Name}";
            parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    private async Task ApplySchemaMigrationsAsync()
    {
        try
        {
            _logger.LogInformation("Applying schema migrations...");
            
            // Migration: Remove BatchId column from SyncExecutionLogs table
            await RemoveBatchIdFromSyncExecutionLogsAsync();
            
            // Migration: Add RecordsDeleted columns to both SyncExecutionLogs and SyncBatchLogs
            await AddRecordsDeletedColumnsAsync();
            
            // Migration: Remove RecordsDeleted column from SyncBatchLogs table (batches don't delete records)
            await RemoveRecordsDeletedFromSyncBatchLogsAsync();
            
            _logger.LogInformation("Schema migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply schema migrations");
            throw;
        }
    }

    private async Task RemoveBatchIdFromSyncExecutionLogsAsync()
    {
        // Check if BatchId column exists
        var columnExists = await ExecuteScalarAsync<long>(@"
            SELECT COUNT(*) 
            FROM information_schema.columns 
            WHERE table_name = 'SyncExecutionLogs' 
            AND table_schema = 'external' 
            AND column_name = 'BatchId'");

        if (columnExists == 0)
        {
            _logger.LogDebug("BatchId column already removed from SyncExecutionLogs table");
            return;
        }

        _logger.LogInformation("Removing BatchId column from SyncExecutionLogs table");
        
        await ExecuteCommandAsync(@"ALTER TABLE external.""SyncExecutionLogs"" DROP COLUMN IF EXISTS ""BatchId""");
        
        _logger.LogInformation("BatchId column removed from SyncExecutionLogs table");
    }

    private async Task AddRecordsDeletedColumnsAsync()
    {
        try
        {
            _logger.LogInformation("Adding RecordsDeleted columns...");
            
            // Add RecordsDeleted to SyncExecutionLogs table
            var addToExecutionLogsQuery = @"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                 WHERE table_name='SyncExecutionLogs' 
                                 AND table_schema='external'
                                 AND column_name='RecordsDeleted') THEN
                        ALTER TABLE external.""SyncExecutionLogs"" 
                        ADD COLUMN ""RecordsDeleted"" INTEGER NOT NULL DEFAULT 0;
                    END IF;
                END $$;";
            
            await ExecuteCommandAsync(addToExecutionLogsQuery);
            
            // Add RecordsDeleted to SyncBatchLogs table  
            var addToBatchLogsQuery = @"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                 WHERE table_name='SyncBatchLogs' 
                                 AND table_schema='external'
                                 AND column_name='RecordsDeleted') THEN
                        ALTER TABLE external.""SyncBatchLogs"" 
                        ADD COLUMN ""RecordsDeleted"" INTEGER NOT NULL DEFAULT 0;
                    END IF;
                END $$;";
                
            await ExecuteCommandAsync(addToBatchLogsQuery);
            
            _logger.LogInformation("Successfully added RecordsDeleted columns");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add RecordsDeleted columns - this may be expected if they already exist");
            // Don't throw - this migration should be safe to run multiple times
        }
    }

    private async Task RemoveRecordsDeletedFromSyncBatchLogsAsync()
    {
        try
        {
            // Check if RecordsDeleted column exists in SyncBatchLogs
            var columnExists = await ExecuteScalarAsync<long>(@"
                SELECT COUNT(*) 
                FROM information_schema.columns 
                WHERE table_name = 'SyncBatchLogs' 
                AND table_schema = 'external' 
                AND column_name = 'RecordsDeleted'");

            if (columnExists == 0)
            {
                _logger.LogDebug("RecordsDeleted column already removed from SyncBatchLogs table");
                return;
            }

            _logger.LogInformation("Removing RecordsDeleted column from SyncBatchLogs table");
            
            await ExecuteCommandAsync(@"ALTER TABLE external.""SyncBatchLogs"" DROP COLUMN IF EXISTS ""RecordsDeleted""");
            
            _logger.LogInformation("RecordsDeleted column removed from SyncBatchLogs table");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove RecordsDeleted column from SyncBatchLogs - this may be expected if it doesn't exist");
            // Don't throw - this migration should be safe to run multiple times
        }
    }
}
