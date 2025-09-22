namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class SyncErrorLog
{
    public long Id { get; set; }
    public long SyncExecutionId { get; set; }
    public long? SyncBatchId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public SyncErrorType ErrorType { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; } // Full exception details
    public string? StackTrace { get; set; }
    
    // Context information
    public string? RecordKey { get; set; } // Source PK of record that failed
    public string? RecordData { get; set; } // JSON of record data that failed
    public string? FieldName { get; set; } // Specific field that caused error
    public string? FieldValue { get; set; } // Value that caused the error
    
    // Resolution tracking
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Navigation properties
    public virtual SyncExecutionLog SyncExecution { get; set; } = null!;
    public virtual SyncBatchLog? SyncBatch { get; set; }
}

public enum SyncErrorType
{
    ConnectionError = 1,
    QueryError = 2,
    DataTransformationError = 3,
    ForeignKeyResolutionError = 4,
    DatabaseInsertError = 5,
    DatabaseUpdateError = 6,
    ValidationError = 7,
    ConfigurationError = 8,
    TimeoutError = 9,
    UnknownError = 10
}
