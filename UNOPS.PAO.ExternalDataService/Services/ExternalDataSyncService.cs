using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;
using UNOPS.PAO.ExternalDataService.Services.Configuration;
using UNOPS.PAO.ExternalDataService.Services.Sync;

namespace UNOPS.PAO.ExternalDataService.Services;

public class ExternalDataSyncService : IExternalDataSyncService
{
    private readonly IConfigurationService _configurationService;
    private readonly ISyncOrchestrator _syncOrchestrator;
    private readonly ISyncLoggingService _syncLoggingService;
    private readonly ILogger<ExternalDataSyncService> _logger;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1); // Ensure only one sync runs at a time per instance

    public ExternalDataSyncService(
        IConfigurationService configurationService,
        ISyncOrchestrator syncOrchestrator,
        ISyncLoggingService syncLoggingService,
        ILogger<ExternalDataSyncService> logger)
    {
        _configurationService = configurationService;
        _syncOrchestrator = syncOrchestrator;
        _syncLoggingService = syncLoggingService;
        _logger = logger;
    }

    public async Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (!configuration.Metadata.Enabled)
        {
            _logger.LogWarning("Sync configuration {ConfigName} is disabled, skipping execution", configuration.Metadata.Name);
            return new SyncResult
            {
                ConfigurationName = configuration.Metadata.Name,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = SyncStatus.Cancelled,
                Errors = new List<SyncError>
                {
                    new() { Message = "Configuration is disabled", Code = "CONFIG_DISABLED" }
                }
            };
        }

        await _syncSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Starting sync execution for configuration {ConfigName}", configuration.Metadata.Name);
            
            var result = await _syncOrchestrator.ExecuteSyncAsync(configuration, cancellationToken);
            
            _logger.LogInformation("Sync execution completed for {ConfigName} with status {Status}", 
                configuration.Metadata.Name, result.Status);
                
            return result;
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }

    public async Task<IEnumerable<SyncResult>> ExecuteAllConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting execution of all sync configurations");
        
        var configurations = await _configurationService.LoadAllConfigurationsAsync();
        var enabledConfigurations = configurations.Where(c => c.Metadata.Enabled).ToList();
        
        _logger.LogInformation("Found {TotalConfigs} configurations, {EnabledConfigs} enabled", 
            configurations.Count(), enabledConfigurations.Count);

        if (!enabledConfigurations.Any())
        {
            _logger.LogWarning("No enabled configurations found");
            return Enumerable.Empty<SyncResult>();
        }

        var results = new List<SyncResult>();

        foreach (var configuration in enabledConfigurations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Cancellation requested, stopping execution after {CompletedCount} configurations", 
                    results.Count);
                break;
            }

            try
            {
                var result = await ExecuteSyncAsync(configuration, cancellationToken);
                results.Add(result);
                
                // Add delay between configurations to prevent overwhelming the system
                if (configuration != enabledConfigurations.Last())
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute sync for configuration {ConfigName}", configuration.Metadata.Name);
                
                results.Add(new SyncResult
                {
                    ConfigurationName = configuration.Metadata.Name,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Status = SyncStatus.Failed,
                    Errors = new List<SyncError>
                    {
                        new() { Message = ex.Message, Exception = ex, Code = "EXECUTION_ERROR" }
                    }
                });
            }
        }

        var successCount = results.Count(r => r.Status == SyncStatus.Completed || r.Status == SyncStatus.CompletedWithErrors);
        var failureCount = results.Count(r => r.Status == SyncStatus.Failed);
        
        _logger.LogInformation("Completed execution of all configurations: {SuccessCount} succeeded, {FailureCount} failed", 
            successCount, failureCount);

        return results;
    }

    public async Task<SyncResult?> ExecuteConfigurationByNameAsync(string configurationName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(configurationName))
        {
            throw new ArgumentNullException(nameof(configurationName));
        }

        _logger.LogInformation("Executing sync for configuration {ConfigName}", configurationName);

        var configuration = await _configurationService.LoadConfigurationAsync(configurationName);
        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigName} not found", configurationName);
            return null;
        }

        return await ExecuteSyncAsync(configuration, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetAvailableConfigurationsAsync()
    {
        try
        {
            var configurations = await _configurationService.LoadAllConfigurationsAsync();
            return configurations.Select(c => c.Metadata.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available configurations");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<Dictionary<string, object?>> GetSyncStatusAsync(string? configurationName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(configurationName))
            {
                // Return status for all configurations
                var configurations = await _configurationService.LoadAllConfigurationsAsync();
                var allStatuses = new List<object>();
                var runningExecutions = 0;

                foreach (var config in configurations)
                {
                    var recentExecutions = await _syncLoggingService.GetRecentExecutionsAsync(config.Metadata.Name, 1);
                    var lastExecution = recentExecutions.FirstOrDefault();
                    var isRunning = lastExecution?.Status == SyncStatus.Running;
                    
                    if (isRunning)
                        runningExecutions++;

                    allStatuses.Add(new
                    {
                        configurationName = config.Metadata.Name,
                        enabled = config.Metadata.Enabled,
                        description = config.Metadata.Description,
                        schedule = config.Metadata.ScheduleCron,
                        isRunning = isRunning,
                        lastExecutionTime = (object?)lastExecution?.StartTime,
                        lastExecutionStatus = (object?)lastExecution?.Status.ToString(),
                        lastExecutionDuration = (object?)lastExecution?.Duration?.TotalSeconds
                    });
                }

                // Get unresolved errors count
                var unresolvedErrors = await _syncLoggingService.GetUnresolvedErrorsAsync(configurationName: null);
                var unresolvedErrorsCount = unresolvedErrors.Count();

                return new Dictionary<string, object?>
                {
                    ["configurations"] = allStatuses,
                    ["totalConfigurations"] = configurations.Count(),
                    ["enabledConfigurations"] = configurations.Count(c => c.Metadata.Enabled),
                    ["runningExecutions"] = runningExecutions,
                    ["unresolvedErrors"] = unresolvedErrorsCount,
                    ["timestamp"] = DateTime.UtcNow
                };
            }
            else
            {
                // Return detailed status for specific configuration
                var configuration = await _configurationService.LoadConfigurationAsync(configurationName);
                if (configuration == null)
                {
                    return new Dictionary<string, object?>
                    {
                        ["error"] = $"Configuration '{configurationName}' not found",
                        ["timestamp"] = DateTime.UtcNow
                    };
                }

                var recentExecutions = await _syncLoggingService.GetRecentExecutionsAsync(configurationName, 10);
                var lastExecution = recentExecutions.FirstOrDefault();
                var unresolvedErrors = await _syncLoggingService.GetUnresolvedErrorsAsync(configurationName);
                var statistics = await _syncLoggingService.GetSyncStatisticsAsync(configurationName, DateTime.UtcNow.AddDays(-30));

                return new Dictionary<string, object?>
                {
                    ["configurationName"] = configurationName,
                    ["enabled"] = configuration.Metadata.Enabled,
                    ["description"] = configuration.Metadata.Description,
                    ["schedule"] = configuration.Metadata.ScheduleCron,
                    ["destinationTable"] = $"{configuration.Destination.Schema ?? "public"}.{configuration.Destination.TableName}",
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
                        recordsExtracted = e.TotalRecordsExtracted,
                        recordsInserted = e.RecordsInserted,
                        recordsUpdated = e.RecordsUpdated,
                        recordsFailed = e.RecordsFailed,
                        duration = e.Duration?.TotalSeconds,
                        batchCount = e.BatchLogs.Count,
                        errorCount = e.ErrorLogs.Count
                    }).Take(10).ToList(),
                    ["unresolvedErrorCount"] = unresolvedErrors.Count(),
                    ["statistics"] = statistics,
                    ["timestamp"] = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status for configuration {ConfigName}", configurationName);
            
            return new Dictionary<string, object?>
            {
                ["error"] = $"Error retrieving status: {ex.Message}",
                ["timestamp"] = DateTime.UtcNow
            };
        }
    }

    public async Task<bool> ValidateConfigurationAsync(string configurationName)
    {
        try
        {
            var configuration = await _configurationService.LoadConfigurationAsync(configurationName);
            if (configuration == null)
            {
                _logger.LogWarning("Configuration {ConfigName} not found for validation", configurationName);
                return false;
            }

            var isValid = await _configurationService.ValidateConfigurationAsync(configuration);
            if (isValid)
            {
                // Also validate with the orchestrator for additional checks
                isValid = await _syncOrchestrator.ValidateConfigurationAsync(configuration);
            }

            _logger.LogInformation("Configuration validation for {ConfigName}: {IsValid}", configurationName, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration {ConfigName}", configurationName);
            return false;
        }
    }

    public async Task<Dictionary<string, object?>> GetSystemHealthAsync()
    {
        try
        {
            var configurations = await _configurationService.LoadAllConfigurationsAsync();
            var recentExecutions = await _syncLoggingService.GetRecentExecutionsAsync(count: 100);
            var unresolvedErrors = await _syncLoggingService.GetUnresolvedErrorsAsync();

            var runningExecutions = recentExecutions.Count(e => e.Status == SyncStatus.Running);
            var recentFailures = recentExecutions
                .Where(e => e.StartTime >= DateTime.UtcNow.AddHours(-24))
                .Count(e => e.Status == SyncStatus.Failed);

            var healthStatus = "Healthy";
            if (runningExecutions > 5)
                healthStatus = "Warning - Many running executions";
            else if (recentFailures > 10)
                healthStatus = "Warning - High failure rate";
            else if (unresolvedErrors.Count() > 50)
                healthStatus = "Warning - Many unresolved errors";

            return new Dictionary<string, object?>
            {
                ["status"] = healthStatus,
                ["totalConfigurations"] = configurations.Count(),
                ["enabledConfigurations"] = configurations.Count(c => c.Metadata.Enabled),
                ["runningExecutions"] = runningExecutions,
                ["recentFailures24h"] = recentFailures,
                ["unresolvedErrors"] = unresolvedErrors.Count(),
                ["lastUpdate"] = DateTime.UtcNow,
                ["version"] = "1.0.0",
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health status");
            
            return new Dictionary<string, object?>
            {
                ["status"] = "Error",
                ["error"] = ex.Message,
                ["lastUpdate"] = DateTime.UtcNow
            };
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _syncSemaphore?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
