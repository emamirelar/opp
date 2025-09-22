namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class ConfigurationExecutionHistory
{
    public int Id { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public DateTime LastSuccessfulExecution { get; set; }
    public DateTime? LastFailedExecution { get; set; }
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public DateTime? LastIncrementalSyncDate { get; set; } // For incremental syncs
    public long TotalRecordsProcessed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Current status
    public bool IsEnabled { get; set; } = true;
    public string? CurrentCronSchedule { get; set; }
    public DateTime? NextScheduledExecution { get; set; }
    
    // Performance metrics (averages)
    public TimeSpan? AverageExecutionDuration { get; set; }
    public decimal? AverageRecordsPerMinute { get; set; }
    public long? AverageMemoryUsageMB { get; set; }
}
