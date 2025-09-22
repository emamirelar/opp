namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class SyncBatchLog
{
    public long Id { get; set; }
    public long SyncExecutionId { get; set; }
    public string BatchId { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SyncBatchStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    
    // Batch metrics
    public int RecordsInBatch { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsSkipped { get; set; }
    
    // Performance metrics
    public TimeSpan? ProcessingDuration { get; set; }
    public TimeSpan? DatabaseOperationDuration { get; set; }
    public TimeSpan? ForeignKeyResolutionDuration { get; set; }
    
    // Batch data range (for debugging)
    public string? FirstRecordKey { get; set; }
    public string? LastRecordKey { get; set; }
    
    // Navigation properties
    public virtual SyncExecutionLog SyncExecution { get; set; } = null!;
    public virtual ICollection<SyncErrorLog> ErrorLogs { get; set; } = new List<SyncErrorLog>();
}

public enum SyncBatchStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    PartiallyCompleted = 4,
    Skipped = 5
}
