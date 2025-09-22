namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class SyncResult
{
    public string ConfigurationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SyncStatus Status { get; set; }
    public int TotalRecordsExtracted { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsDeleted { get; set; }
    public List<SyncError> Errors { get; set; } = new();
}

public class SyncBatchResult
{
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsDeleted { get; set; } // Always 0 for batches, deletions happen at execution level
    
    // Performance metrics
    public TimeSpan? DatabaseOperationDuration { get; set; }
    public TimeSpan? ForeignKeyResolutionDuration { get; set; }
    
    public bool HasErrors => Errors.Any();
    public List<SyncError> Errors { get; set; } = new();
}

public class SyncError
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Exception? Exception { get; set; }
    public string? FieldName { get; set; }
    public string? FieldValue { get; set; }
    public string? RecordKey { get; set; }
    public string? RecordData { get; set; }
}

public class ForeignKeyLookupResult
{
    public object? LookupValue { get; set; }
    public int? Id { get; set; }
}
