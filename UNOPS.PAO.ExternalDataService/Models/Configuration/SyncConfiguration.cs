using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.ExternalDataService.Models.Configuration;

public class SyncConfiguration
{
    public SyncMetadata Metadata { get; set; } = new();
    public SourceConfiguration Source { get; set; } = new();
    public DestinationConfiguration Destination { get; set; } = new();
    public SyncOptions SyncOptions { get; set; } = new();
    public ValidationConfiguration Validation { get; set; } = new();
    public ErrorHandlingConfiguration ErrorHandling { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
}

public class SyncMetadata
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool Enabled { get; set; } = true;
    public string ScheduleCron { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public int TimeoutMinutes { get; set; } = 30;
}

public class SyncOptions
{
    public SyncMode SyncMode { get; set; } = SyncMode.Upsert;
    public bool DeleteMissingRecords { get; set; } = false;
    public List<string> PreserveDestinationFields { get; set; } = new() { "Id", "CreatedDate", "CreatedBy" };
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.SourceWins;
    public bool BatchProcessing { get; set; } = true;
    public bool ParallelProcessing { get; set; } = true;
    public int MaxParallelBatches { get; set; } = 4;
}

public class ValidationConfiguration
{
    public List<string> RequiredSourceFields { get; set; } = new();
    public List<DataQualityCheck> DataQualityChecks { get; set; } = new();
    public DuplicateHandling DuplicateHandling { get; set; } = new();
}

public class DataQualityCheck
{
    public string Field { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty;
    public ValidationAction Action { get; set; } = ValidationAction.LogWarning;
}

public class DuplicateHandling
{
    public string CheckField { get; set; } = string.Empty;
    public DuplicateAction Action { get; set; } = DuplicateAction.UpdateLatest;
}

public class ErrorHandlingConfiguration
{
    public ErrorAction OnConnectionError { get; set; } = ErrorAction.RetryWithBackoff;
    public ErrorAction OnQueryError { get; set; } = ErrorAction.FailBatch;
    public ErrorAction OnMappingError { get; set; } = ErrorAction.SkipRecord;
    public ErrorAction OnFKLookupError { get; set; } = ErrorAction.LogAndContinue;
    public NotificationConfiguration Notification { get; set; } = new();
}

public class NotificationConfiguration
{
    public bool OnFailure { get; set; } = true;
    public bool OnSuccess { get; set; } = false;
    public bool OnWarning { get; set; } = true;
    public List<string> EmailRecipients { get; set; } = new();
    public string SlackWebhook { get; set; } = string.Empty;
}

public class LoggingConfiguration
{
    public string LogLevel { get; set; } = "Information";
    public bool LogSqlQueries { get; set; } = true;
    public bool LogRecordDetails { get; set; } = false;
    public int RetentionDays { get; set; } = 30;
}

// Enums
public enum SyncMode
{
    Upsert = 0,
    InsertOnly = 1,
    FullReplace = 2
}

public enum ConflictResolution
{
    SourceWins = 0,
    DestinationWins = 1,
    Manual = 2
}

public enum ValidationAction
{
    LogWarning = 0,
    FailRecord = 1,
    Skip = 2
}

public enum DuplicateAction
{
    UpdateLatest = 0,
    Fail = 1,
    Ignore = 2
}

public enum ErrorAction
{
    RetryWithBackoff = 0,
    FailBatch = 1,
    SkipRecord = 2,
    LogAndContinue = 3
}
