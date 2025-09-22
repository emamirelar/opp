using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public interface ISyncLoggingService
{
    Task<SyncExecutionLog> StartSyncExecutionAsync(SyncConfiguration configuration, string triggeredBy, string? triggerId = null);
    Task UpdateSyncExecutionAsync(long executionId, SyncResult result);
    Task<SyncBatchLog> StartBatchAsync(long executionId, int batchNumber, int recordCount, string? firstRecordKey = null, string? lastRecordKey = null);
    Task CompleteBatchAsync(long batchId, SyncBatchResult result);
    Task LogErrorAsync(long executionId, long? batchId, SyncError error, string? recordKey = null, string? recordData = null);
    Task<DateTime?> GetLastSuccessfulSyncDateAsync(string configurationName);
    Task UpdateConfigurationHistoryAsync(string configurationName, SyncResult result);
    Task<IEnumerable<SyncExecutionLog>> GetRecentExecutionsAsync(string? configurationName = null, int count = 50);
    Task<SyncExecutionLog?> GetExecutionAsync(long executionId);
    Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(string? configurationName = null);
    Task<Dictionary<string, object>> GetSyncStatisticsAsync(string? configurationName = null, DateTime? fromDate = null);
    Task LogProcessedRecordAsync(long batchId, string primaryKey, string recordAction, string? errorMessage = null);
    Task LogProcessedRecordsAsync(long batchId, IEnumerable<(string PrimaryKey, string RecordAction, string? ErrorMessage)> processedRecords);
    Task<HashSet<string>> GetProcessedPrimaryKeysForExecutionAsync(long executionId);
    
    /// <summary>
    /// Get processed records (primary keys) for a specific batch
    /// </summary>
    Task<List<string>> GetProcessedPrimaryKeysForBatchAsync(long batchId);
    
    /// <summary>
    /// Get real-time batch count for an execution by querying database directly
    /// </summary>
    Task<int> GetBatchCountForExecutionAsync(long executionId);
    
    /// <summary>
    /// Get real-time error count for an execution by querying database directly
    /// </summary>
    Task<int> GetErrorCountForExecutionAsync(long executionId);
    
    /// <summary>
    /// Get live execution statistics by aggregating from batch logs for running executions
    /// </summary>
    Task<(int TotalRecordsExtracted, int RecordsInserted, int RecordsUpdated, int RecordsDeleted, int RecordsFailed, int BatchCount, int BatchesProcessed, int ErrorCount)> GetLiveExecutionStatsAsync(long executionId);
}
