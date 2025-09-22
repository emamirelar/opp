using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using UNOPS.PAO.ExternalDataService.Services;

namespace UNOPS.PAO.ExternalDataService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SyncController : ControllerBase
{
    private readonly IExternalDataSyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        IExternalDataSyncService syncService,
        ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a specific sync configuration
    /// </summary>
    /// <param name="configurationName">Name of the configuration to execute</param>
    /// <returns>Sync execution result</returns>
    [HttpPost("execute/{configurationName}")]
    public async Task<IActionResult> ExecuteSync(string configurationName)
    {
        try
        {
            _logger.LogInformation("Received sync request for configuration: {ConfigName} from {RemoteIP}", 
                configurationName, HttpContext.Connection.RemoteIpAddress);
            
            var result = await _syncService.ExecuteConfigurationByNameAsync(configurationName);
            
            if (result == null)
            {
                return NotFound(new { 
                    error = $"Configuration '{configurationName}' not found",
                    timestamp = DateTime.UtcNow
                });
            }

            var response = new
            {
                configurationName = result.ConfigurationName,
                status = result.Status.ToString(),
                startTime = result.StartTime,
                endTime = result.EndTime,
                duration = (result.EndTime - result.StartTime).TotalSeconds,
                recordsExtracted = result.TotalRecordsExtracted,
                recordsInserted = result.RecordsInserted,
                recordsUpdated = result.RecordsUpdated,
                recordsFailed = result.RecordsFailed,
                errors = result.Errors.Select(e => new
                {
                    message = e.Message,
                    code = e.Code,
                    fieldName = e.FieldName,
                    recordKey = e.RecordKey
                }).ToList(),
                timestamp = DateTime.UtcNow
            };

            // Return appropriate HTTP status based on sync result
            return result.Status switch
            {
                Models.Sync.SyncStatus.Completed => Ok(response),
                Models.Sync.SyncStatus.CompletedWithErrors => Ok(response),
                Models.Sync.SyncStatus.Failed => StatusCode(500, response),
                Models.Sync.SyncStatus.Cancelled => StatusCode(409, response),
                Models.Sync.SyncStatus.Timeout => StatusCode(408, response),
                _ => Ok(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sync for configuration {ConfigName}", configurationName);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                configurationName,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Execute all enabled sync configurations
    /// </summary>
    /// <returns>Results for all executed configurations</returns>
    [HttpPost("execute-all")]
    public async Task<IActionResult> ExecuteAllSyncs()
    {
        try
        {
            _logger.LogInformation("Received request to execute all sync configurations from {RemoteIP}", 
                HttpContext.Connection.RemoteIpAddress);
            
            var results = await _syncService.ExecuteAllConfigurationsAsync(HttpContext.RequestAborted);
            
            var response = new
            {
                executedConfigurations = results.Count(),
                completedSuccessfully = results.Count(r => r.Status == Models.Sync.SyncStatus.Completed),
                completedWithErrors = results.Count(r => r.Status == Models.Sync.SyncStatus.CompletedWithErrors),
                failed = results.Count(r => r.Status == Models.Sync.SyncStatus.Failed),
                cancelled = results.Count(r => r.Status == Models.Sync.SyncStatus.Cancelled),
                totalRecordsProcessed = results.Sum(r => r.TotalRecordsExtracted),
                totalRecordsInserted = results.Sum(r => r.RecordsInserted),
                totalRecordsUpdated = results.Sum(r => r.RecordsUpdated),
                totalRecordsFailed = results.Sum(r => r.RecordsFailed),
                results = results.Select(result => new
                {
                    configurationName = result.ConfigurationName,
                    status = result.Status.ToString(),
                    duration = (result.EndTime - result.StartTime).TotalSeconds,
                    recordsExtracted = result.TotalRecordsExtracted,
                    recordsProcessed = result.RecordsInserted + result.RecordsUpdated,
                    recordsFailed = result.RecordsFailed,
                    errorCount = result.Errors.Count
                }).ToList(),
                timestamp = DateTime.UtcNow
            };

            // Return 207 Multi-Status if there were any failures, otherwise 200 OK
            var hasFailures = results.Any(r => r.Status == Models.Sync.SyncStatus.Failed);
            return hasFailures ? StatusCode(207, response) : Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing all sync configurations");
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Handle Pub/Sub trigger for sync execution
    /// </summary>
    /// <param name="pubSubMessage">Pub/Sub message containing sync trigger request</param>
    /// <returns>Sync execution result</returns>
    [HttpPost("pubsub")]
    public async Task<IActionResult> HandlePubSubTrigger([FromBody] PubSubMessage pubSubMessage)
    {
        try
        {
            // Validate Pub/Sub message format
            if (pubSubMessage?.Data == null)
            {
                return BadRequest(new { 
                    error = "Invalid Pub/Sub message format",
                    timestamp = DateTime.UtcNow
                });
            }

            // Decode the Pub/Sub message
            var messageData = Encoding.UTF8.GetString(Convert.FromBase64String(pubSubMessage.Data));
            _logger.LogDebug("Received Pub/Sub message: {MessageData}", messageData);

            var triggerRequest = JsonSerializer.Deserialize<SyncTriggerRequest>(messageData);
            
            if (string.IsNullOrEmpty(triggerRequest?.ConfigurationName))
            {
                return BadRequest(new { 
                    error = "Pub/Sub message must contain 'configurationName' field",
                    receivedData = messageData,
                    timestamp = DateTime.UtcNow
                });
            }

            _logger.LogInformation("Received Pub/Sub sync trigger for configuration: {ConfigName} (MessageId: {MessageId})", 
                triggerRequest.ConfigurationName, pubSubMessage.MessageId);
            
            var result = await _syncService.ExecuteConfigurationByNameAsync(triggerRequest.ConfigurationName);
            
            if (result == null)
            {
                return NotFound(new { 
                    error = $"Configuration '{triggerRequest.ConfigurationName}' not found",
                    messageId = pubSubMessage.MessageId,
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new { 
                message = "Sync completed via Pub/Sub trigger", 
                status = result.Status.ToString(),
                configurationName = result.ConfigurationName,
                messageId = pubSubMessage.MessageId,
                recordsProcessed = result.RecordsInserted + result.RecordsUpdated,
                duration = (result.EndTime - result.StartTime).TotalSeconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing Pub/Sub message JSON");
            return BadRequest(new { 
                error = "Invalid JSON in Pub/Sub message",
                details = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Pub/Sub trigger");
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get sync status for all or specific configuration
    /// </summary>
    /// <param name="configurationName">Optional configuration name to get specific status</param>
    /// <returns>Sync status information</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetSyncStatus([FromQuery] string? configurationName = null)
    {
        try
        {
            var status = await _syncService.GetSyncStatusAsync(configurationName);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status for configuration {ConfigName}", configurationName);
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get list of available sync configurations
    /// </summary>
    /// <returns>List of configuration names</returns>
    [HttpGet("configurations")]
    public async Task<IActionResult> GetConfigurations()
    {
        try
        {
            var configurations = await _syncService.GetAvailableConfigurationsAsync();
            return Ok(new
            {
                configurations = configurations.ToList(),
                count = configurations.Count(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available configurations");
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var health = await _syncService.GetSystemHealthAsync();
            
            // Return 200 for healthy, 503 for unhealthy
            var isHealthy = health.GetValueOrDefault("status")?.ToString() == "Healthy";
            return isHealthy ? Ok(health) : StatusCode(503, health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            return StatusCode(503, new { 
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Validate a specific configuration
    /// </summary>
    /// <param name="configurationName">Name of the configuration to validate</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/{configurationName}")]
    public async Task<IActionResult> ValidateConfiguration(string configurationName)
    {
        try
        {
            var isValid = await _syncService.ValidateConfigurationAsync(configurationName);
            
            return Ok(new
            {
                configurationName,
                isValid,
                message = isValid ? "Configuration is valid" : "Configuration validation failed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration {ConfigName}", configurationName);
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                configurationName,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

/// <summary>
/// Pub/Sub message format
/// </summary>
public class PubSubMessage
{
    public string Data { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public string MessageId { get; set; } = string.Empty;
    public DateTime PublishTime { get; set; }
}

/// <summary>
/// Sync trigger request format for Pub/Sub messages
/// </summary>
public class SyncTriggerRequest
{
    public string ConfigurationName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
