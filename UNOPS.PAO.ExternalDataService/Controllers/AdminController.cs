using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.ExternalDataService.Services;
using UNOPS.PAO.ExternalDataService.Services.Sync;

namespace UNOPS.PAO.ExternalDataService.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly IExternalDataSyncService _syncService;
    private readonly ISyncLoggingService _syncLoggingService;
    private readonly ILogger<AdminController> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AdminController(
        IExternalDataSyncService syncService,
        ISyncLoggingService syncLoggingService,
        ILogger<AdminController> logger,
        IServiceProvider serviceProvider)
    {
        _syncService = syncService;
        _syncLoggingService = syncLoggingService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Admin dashboard main page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var systemHealth = await _syncService.GetSystemHealthAsync();
            var allStatus = await _syncService.GetSyncStatusAsync();
            
            ViewBag.SystemHealth = systemHealth;
            ViewBag.AllStatus = allStatus;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    /// <summary>
    /// Execute a single sync configuration
    /// </summary>
    [HttpPost("sync/{configurationName}")]
    public IActionResult ExecuteSync(string configurationName)
    {
        try
        {
            _logger.LogInformation("Manual sync execution triggered for {ConfigName} from admin interface", configurationName);
            
            // Start sync execution in background with proper service scope
            // This allows the execution to appear in the list immediately
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Background task started for {ConfigName}", configurationName);
                    
                    // Create a new service scope for the background task
                    using var scope = _serviceProvider.CreateScope();
                    _logger.LogInformation("Service scope created for background task {ConfigName}", configurationName);
                    
                    var backgroundSyncService = scope.ServiceProvider.GetRequiredService<IExternalDataSyncService>();
                    var backgroundLogger = scope.ServiceProvider.GetRequiredService<ILogger<AdminController>>();
                    
                    backgroundLogger.LogInformation("Starting background sync execution for {ConfigName}", configurationName);
                    
                    var result = await backgroundSyncService.ExecuteConfigurationByNameAsync(configurationName);
                    
                    if (result == null)
                    {
                        backgroundLogger.LogError("Background sync returned null result for {ConfigName} - configuration may not exist", configurationName);
                    }
                    else
                    {
                        backgroundLogger.LogInformation("Background sync execution completed for {ConfigName}: Status={Status}, Records: {Inserted} inserted, {Updated} updated, {Failed} failed", 
                            configurationName, result.Status, result.RecordsInserted, result.RecordsUpdated, result.RecordsFailed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background sync execution failed for {ConfigName} - StackTrace: {StackTrace}", configurationName, ex.StackTrace);
                }
            });
            
            // Immediately show success message and redirect
            TempData["Success"] = $"Sync execution started for '{configurationName}'. The execution will appear in the list below and can be monitored in real-time.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting sync execution for {ConfigName} from admin interface", configurationName);
            TempData["Error"] = $"Error starting sync execution: {ex.Message}";
        }

        return RedirectToAction("ConfigurationDetails", new { configurationName });
    }

    /// <summary>
    /// Execute all enabled sync configurations
    /// </summary>
    [HttpPost("sync-all")]
    public async Task<IActionResult> ExecuteAllSyncs()
    {
        try
        {
            _logger.LogInformation("Manual execution of all syncs triggered from admin interface");
            
            var results = await _syncService.ExecuteAllConfigurationsAsync();
            var resultsList = results.ToList();
            
            var successful = resultsList.Count(r => r.Status == Models.Sync.SyncStatus.Completed);
            var withErrors = resultsList.Count(r => r.Status == Models.Sync.SyncStatus.CompletedWithErrors);
            var failed = resultsList.Count(r => r.Status == Models.Sync.SyncStatus.Failed);
            
            var message = $"Executed {resultsList.Count} configurations: " +
                         $"{successful} succeeded, {withErrors} with errors, {failed} failed";
            
            if (failed == 0 && withErrors == 0)
            {
                TempData["Success"] = message;
            }
            else if (failed == 0)
            {
                TempData["Warning"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing all syncs from admin interface");
            TempData["Error"] = $"Error executing all syncs: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Get detailed execution information
    /// </summary>
    [HttpGet("execution/{executionId}")]
    public async Task<IActionResult> GetExecutionDetails(long executionId)
    {
        try
        {
            var execution = await _syncLoggingService.GetExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new { error = "Execution not found" });
            }

            var details = new
            {
                ExecutionId = execution.Id,
                execution.ConfigurationName,
                execution.StartTime,
                execution.EndTime,
                execution.Status,
                execution.StatusMessage,
                execution.TotalRecordsExtracted,
                execution.RecordsInserted,
                execution.RecordsUpdated,
                execution.RecordsDeleted,
                execution.RecordsFailed,
                execution.Duration,
                execution.TriggeredBy,
                execution.TriggerId,
                BatchLogs = execution.BatchLogs.Select(b => new
                {
                    b.Id,
                    b.BatchNumber,
                    b.StartTime,
                    b.EndTime,
                    b.Status,
                    b.RecordsInBatch,
                    b.RecordsInserted,
                    b.RecordsUpdated,
                    b.RecordsFailed,
                    b.ProcessingDuration
                }).ToList(),
                ErrorLogs = execution.ErrorLogs.Select(e => new
                {
                    e.Id,
                    e.OccurredAt,
                    e.ErrorType,
                    e.ErrorCode,
                    e.ErrorMessage,
                    e.RecordKey,
                    e.FieldName,
                    e.FieldValue
                }).ToList()
            };

            return Json(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution details for {ExecutionId}", executionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check for the admin interface
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var health = await _syncService.GetSystemHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin health check");
            return StatusCode(500, new { 
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// API endpoint for real-time status updates
    /// </summary>
    [HttpGet("api/status")]
    public async Task<IActionResult> GetStatusApi([FromQuery] string? configurationName = null)
    {
        try
        {
            var status = await _syncService.GetSyncStatusAsync(configurationName);
            return Json(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for API");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Configuration-specific execution history and monitoring view
    /// </summary>
    [HttpGet("configuration/{configurationName}")]
    public async Task<IActionResult> ConfigurationDetails(string configurationName)
    {
        try
        {
            // Verify configuration exists
            var status = await _syncService.GetSyncStatusAsync(configurationName);
            if (status.ContainsKey("error"))
            {
                TempData["Error"] = status["error"]?.ToString();
                return RedirectToAction("Index");
            }

            ViewBag.ConfigurationName = configurationName;
            ViewBag.Status = status;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration details for {ConfigName}", configurationName);
            TempData["Error"] = $"Error loading configuration details: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// API endpoint for configuration-specific execution history
    /// </summary>
    [HttpGet("api/configuration/{configurationName}/executions")]
    public async Task<IActionResult> GetConfigurationExecutions(
        string configurationName,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            _logger.LogInformation("Getting executions for configuration: {ConfigName}, limit: {Limit}, offset: {Offset}", 
                configurationName, limit, offset);

            var executions = await _syncLoggingService.GetRecentExecutionsAsync(configurationName, limit + offset);
            
            _logger.LogInformation("Found {ExecutionCount} executions for {ConfigName}", 
                executions.Count(), configurationName);

            var paginatedExecutions = executions.Skip(offset).Take(limit).ToList();

            // Get REAL-TIME live data for each execution
            var executionsWithLiveCounts = new List<object>();
            foreach (var e in paginatedExecutions)
            {
                bool isRunning = e.Status == Models.Sync.SyncStatus.Running;
                
                if (isRunning)
                {
                    // For running executions: Get live stats from batch logs aggregation
                    var liveStats = await _syncLoggingService.GetLiveExecutionStatsAsync(e.Id);
                    
                    executionsWithLiveCounts.Add(new
                    {
                        ExecutionId = e.Id,
                        e.StartTime,
                        e.EndTime,
                        Status = e.Status.ToString(),
                        e.StatusMessage,
                        TotalRecordsExtracted = liveStats.TotalRecordsExtracted, // LIVE from batches
                        RecordsInserted = liveStats.RecordsInserted, // LIVE from batches
                        RecordsUpdated = liveStats.RecordsUpdated, // LIVE from batches
                        RecordsDeleted = liveStats.RecordsDeleted, // LIVE from batches
                        RecordsFailed = liveStats.RecordsFailed, // LIVE from batches
                        BatchesProcessed = liveStats.BatchesProcessed, // LIVE from batches
                        e.BatchesFailed,
                        Duration = e.Duration?.TotalSeconds,
                        e.TriggeredBy,
                        e.TriggerId,
                        BatchCount = liveStats.BatchCount, // LIVE count from database
                        ErrorCount = liveStats.ErrorCount, // LIVE count from database
                        IsRunning = true
                    });
                }
                else
                {
                    // For completed executions: Use stored values from execution record
                    executionsWithLiveCounts.Add(new
                    {
                        ExecutionId = e.Id,
                        e.StartTime,
                        e.EndTime,
                        Status = e.Status.ToString(),
                        e.StatusMessage,
                        e.TotalRecordsExtracted, // Final values from execution record
                        e.RecordsInserted, // Final values from execution record
                        e.RecordsUpdated, // Final values from execution record
                        e.RecordsDeleted, // Final values from execution record
                        e.RecordsFailed, // Final values from execution record
                        e.BatchesProcessed,
                        e.BatchesFailed,
                        Duration = e.Duration?.TotalSeconds,
                        e.TriggeredBy,
                        e.TriggerId,
                        BatchCount = e.BatchLogs.Count, // Use navigation property for completed
                        ErrorCount = e.ErrorLogs.Count, // Use navigation property for completed
                        IsRunning = false
                    });
                }
            }

            var result = new
            {
                configurationName,
                executions = executionsWithLiveCounts,
                hasMore = executions.Count() > offset + limit,
                total = executions.Count(),
                debug = new
                {
                    originalConfigName = configurationName,
                    executionCountBeforePagination = executions.Count(),
                    executionCountAfterPagination = paginatedExecutions.Count,
                    requestedLimit = limit,
                    requestedOffset = offset
                }
            };

            _logger.LogInformation("Returning {ReturnedCount} executions out of {TotalCount} for {ConfigName}", 
                paginatedExecutions.Count, executions.Count(), configurationName);

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executions for {ConfigName}", configurationName);
            return StatusCode(500, new { 
                error = ex.Message,
                configurationName,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Test endpoint to verify API connectivity and data availability
    /// </summary>
    [HttpGet("api/test")]
    public async Task<IActionResult> TestApi()
    {
        try
        {
            var allExecutions = await _syncLoggingService.GetRecentExecutionsAsync(null, 10);
            var systemHealth = await _syncService.GetSystemHealthAsync();
            
            return Ok(new 
            {
                message = "API is working",
                timestamp = DateTime.UtcNow,
                totalExecutionsInDatabase = allExecutions.Count(),
                systemHealth,
                sampleExecution = allExecutions.FirstOrDefault() != null ? new
                {
                    id = allExecutions.First().Id,
                    configName = allExecutions.First().ConfigurationName,
                    startTime = allExecutions.First().StartTime,
                    status = allExecutions.First().Status.ToString()
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = "API test failed", 
                message = ex.Message,
                stackTrace = ex.StackTrace 
            });
        }
    }

    /// <summary>
    /// API endpoint for detailed batch information within an execution
    /// </summary>
    [HttpGet("api/execution/{executionId}/batches")]
    public async Task<IActionResult> GetExecutionBatches(long executionId)
    {
        try
        {
            var execution = await _syncLoggingService.GetExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new { error = "Execution not found" });
            }

            var batches = new List<object>();
            
            foreach (var b in execution.BatchLogs.OrderBy(batch => batch.BatchNumber))
            {
                // Get primary keys for this batch
                var primaryKeys = await _syncLoggingService.GetProcessedPrimaryKeysForBatchAsync(b.Id);
                var commaSeparatedKeys = string.Join(", ", primaryKeys);
                
                batches.Add(new
                {
                    b.Id,
                    b.BatchId,
                    b.BatchNumber,
                    b.StartTime,
                    b.EndTime,
                    Status = b.Status.ToString(),
                    b.StatusMessage,
                    b.RecordsInBatch,
                    b.RecordsInserted,
                    b.RecordsUpdated,
                    b.RecordsFailed,
                    b.RecordsSkipped,
                    ProcessingDurationSeconds = b.ProcessingDuration?.TotalSeconds,
                    DatabaseOperationDurationSeconds = b.DatabaseOperationDuration?.TotalSeconds,
                    ForeignKeyResolutionDurationSeconds = b.ForeignKeyResolutionDuration?.TotalSeconds,
                    b.FirstRecordKey,
                    b.LastRecordKey,
                    ErrorCount = b.ErrorLogs.Count,
                    PrimaryKeysProcessed = primaryKeys.Count,
                    CommaSeparatedKeys = commaSeparatedKeys,
                    IsCompleted = b.Status == Models.Sync.SyncBatchStatus.Completed ||
                                b.Status == Models.Sync.SyncBatchStatus.PartiallyCompleted ||
                                b.Status == Models.Sync.SyncBatchStatus.Failed ||
                                b.Status == Models.Sync.SyncBatchStatus.Skipped,
                    Progress = b.RecordsInBatch > 0 ? 
                        Math.Round((double)(b.RecordsInserted + b.RecordsUpdated + b.RecordsFailed + b.RecordsSkipped) / b.RecordsInBatch * 100, 1) : 0
                });
            }

            // Calculate summary from the original batch logs
            var batchLogs = execution.BatchLogs.ToList();
            var completedBatches = batchLogs.Count(b => 
                b.Status == Models.Sync.SyncBatchStatus.Completed ||
                b.Status == Models.Sync.SyncBatchStatus.PartiallyCompleted ||
                b.Status == Models.Sync.SyncBatchStatus.Failed ||
                b.Status == Models.Sync.SyncBatchStatus.Skipped);
            var totalRecords = batchLogs.Sum(b => b.RecordsInBatch);
            var processedRecords = batchLogs.Sum(b => b.RecordsInserted + b.RecordsUpdated + b.RecordsFailed + b.RecordsSkipped);

            var result = new
            {
                ExecutionId = executionId,
                execution.ConfigurationName,
                ExecutionStatus = execution.Status.ToString(),
                batches,
                summary = new
                {
                    totalBatches = batches.Count,
                    completedBatches,
                    runningBatches = batches.Count - completedBatches,
                    totalRecords,
                    processedRecords,
                    overallProgress = totalRecords > 0 ? 
                        Math.Round((double)processedRecords / totalRecords * 100, 1) : 0
                }
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch details for execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// API endpoint for execution error logs
    /// </summary>
    [HttpGet("api/execution/{executionId}/errors")]
    public async Task<IActionResult> GetExecutionErrors(long executionId)
    {
        try
        {
            var execution = await _syncLoggingService.GetExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new { error = "Execution not found" });
            }

            var errors = execution.ErrorLogs.OrderByDescending(e => e.OccurredAt).Select(e => new
            {
                e.Id,
                e.OccurredAt,
                ErrorType = e.ErrorType.ToString(),
                e.ErrorCode,
                e.ErrorMessage,
                e.ErrorDetails,
                e.RecordKey,
                e.FieldName,
                e.FieldValue,
                e.IsResolved,
                e.ResolvedAt,
                e.ResolutionNotes,
                BatchNumber = e.SyncBatch?.BatchNumber,
                BatchId = e.SyncBatch?.BatchId
            }).ToList();

            var result = new
            {
                ExecutionId = executionId,
                execution.ConfigurationName,
                errors,
                summary = new
                {
                    totalErrors = errors.Count,
                    unresolvedErrors = errors.Count(e => !e.IsResolved),
                    errorsByType = errors.GroupBy(e => e.ErrorType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    errorsByBatch = errors.Where(e => e.BatchNumber.HasValue)
                        .GroupBy(e => e.BatchNumber!.Value)
                        .ToDictionary(g => g.Key, g => g.Count())
                }
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting errors for execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a running execution
    /// </summary>
    [HttpPost("api/execution/{executionId}/cancel")]
    public async Task<IActionResult> CancelExecution(long executionId)
    {
        try
        {
            var execution = await _syncLoggingService.GetExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new { error = "Execution not found" });
            }

            if (execution.Status != Models.Sync.SyncStatus.Running)
            {
                return BadRequest(new { error = "Execution is not running" });
            }

            // For now, we'll mark the execution as cancelled in the database
            // In a more sophisticated implementation, you might want to signal the running process
            // This could be done through a cancellation token system or message queue
            
            var result = new Models.Sync.SyncResult
            {
                Status = Models.Sync.SyncStatus.Cancelled,
                EndTime = DateTime.UtcNow
            };

            await _syncLoggingService.UpdateSyncExecutionAsync(executionId, result);

            _logger.LogInformation("Execution {ExecutionId} cancelled by user request", executionId);

            return Ok(new { 
                message = "Execution cancelled successfully",
                executionId,
                status = "Cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel all running executions for a specific configuration
    /// </summary>
    [HttpPost("api/configuration/{configurationName}/cancel")]
    public async Task<IActionResult> CancelConfigurationExecutions(string configurationName)
    {
        try
        {
            // Get all running executions for this configuration
            var executions = await _syncLoggingService.GetRecentExecutionsAsync(configurationName, 100);
            var runningExecutions = executions.Where(e => e.Status == Models.Sync.SyncStatus.Running).ToList();

            if (!runningExecutions.Any())
            {
                return BadRequest(new { error = "No running executions found for this configuration" });
            }

            var cancelledCount = 0;
            var errors = new List<string>();

            foreach (var execution in runningExecutions)
            {
                try
                {
                    var result = new Models.Sync.SyncResult
                    {
                        Status = Models.Sync.SyncStatus.Cancelled,
                        EndTime = DateTime.UtcNow
                    };

                    await _syncLoggingService.UpdateSyncExecutionAsync(execution.Id, result);
                    cancelledCount++;
                    
                    _logger.LogInformation("Execution {ExecutionId} for configuration {ConfigurationName} cancelled by user request", 
                        execution.Id, configurationName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling execution {ExecutionId} for configuration {ConfigurationName}", 
                        execution.Id, configurationName);
                    errors.Add($"Failed to cancel execution {execution.Id}: {ex.Message}");
                }
            }

            var message = $"Cancelled {cancelledCount} of {runningExecutions.Count} running executions for configuration '{configurationName}'";
            
            if (errors.Any())
            {
                message += $". Errors: {string.Join("; ", errors)}";
            }

            _logger.LogInformation("Cancelled {CancelledCount} of {TotalCount} running executions for configuration {ConfigurationName}", 
                cancelledCount, runningExecutions.Count, configurationName);

            return Ok(new { 
                message,
                configurationName,
                totalRunning = runningExecutions.Count,
                cancelled = cancelledCount,
                errors = errors.Count,
                status = "Cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling executions for configuration {ConfigurationName}", configurationName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// API endpoint to get processed primary keys for a specific batch
    /// </summary>
    [HttpGet("api/batch/{batchId}/primary-keys")]
    public async Task<IActionResult> GetBatchPrimaryKeys(long batchId)
    {
        try
        {
            var primaryKeys = await _syncLoggingService.GetProcessedPrimaryKeysForBatchAsync(batchId);
            
            return Json(new
            {
                BatchId = batchId,
                PrimaryKeys = primaryKeys,
                Count = primaryKeys.Count,
                CommaSeparatedKeys = string.Join(", ", primaryKeys)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary keys for batch {BatchId}", batchId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
