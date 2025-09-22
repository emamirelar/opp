namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class SyncExecutionLog
{
    public long Id { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SyncStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    
    // Metrics
    public int TotalRecordsExtracted { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsDeleted { get; set; }
    public int BatchesProcessed { get; set; }
    public int BatchesFailed { get; set; }
    
    // Configuration snapshot
    public string ConfigurationSnapshot { get; set; } = string.Empty; // JSON of config used
    public string SourceQuery { get; set; } = string.Empty;
    public string DestinationTable { get; set; } = string.Empty;
    
    // Execution context
    public string? TriggeredBy { get; set; } // "scheduler", "manual", "pubsub"
    public string? TriggerId { get; set; } // Cloud Scheduler job name, user ID, etc.
    public string? CloudRunInstance { get; set; } // For distributed deployments
    
    // Duration and performance
    public TimeSpan? Duration { get; set; }
    public long? MemoryUsedMB { get; set; }
    public decimal? CpuTimeSeconds { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<SyncBatchLog> BatchLogs { get; set; } = new List<SyncBatchLog>();
    public virtual ICollection<SyncErrorLog> ErrorLogs { get; set; } = new List<SyncErrorLog>();
}

public enum SyncStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4,
    Cancelled = 5,
    Timeout = 6
}
