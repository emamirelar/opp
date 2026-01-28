using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.Admin;

/// <summary>
/// System administration model
/// </summary>
public class SystemAdminModel
{
    // ========== SYSTEM INFORMATION ==========
    public string SystemName { get; set; }
    public string Version { get; set; }
    public string? Environment { get; set; } // Development, Staging, Production
    public DateTime? LastDeploymentDate { get; set; }
    public string? DeployedBy { get; set; }

    // ========== SYSTEM HEALTH ==========
    public string? SystemStatus { get; set; } // Healthy, Warning, Critical
    public decimal? CpuUsage { get; set; }
    public decimal? MemoryUsage { get; set; }
    public decimal? DiskUsage { get; set; }
    public int? ActiveUsers { get; set; }
    public int? TotalUsers { get; set; }

    // ========== DATABASE INFORMATION ==========
    public string? DatabaseName { get; set; }
    public string? DatabaseVersion { get; set; }
    public long? DatabaseSize { get; set; }
    public DateTime? LastBackupDate { get; set; }

    // ========== STATISTICS ==========
    public int TotalPartners { get; set; }
    public int TotalContacts { get; set; }
    public int TotalInteractions { get; set; }
    public int TotalOpportunities { get; set; }
    public DateTime? StatisticsGeneratedDate { get; set; }
}

/// <summary>
/// Maintenance task model
/// </summary>
public class MaintenanceTaskModel
{
    public int Id { get; set; }
    public string TaskName { get; set; }
    public string? Description { get; set; }
    public string TaskType { get; set; } // Backup, Cleanup, Optimization, Migration
    public string Status { get; set; } // Pending, Running, Completed, Failed
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ResultMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public int? RecordsProcessed { get; set; }
    public int? RecordsAffected { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// System configuration model
/// </summary>
public class SystemConfigModel
{
    public int Id { get; set; }
    public string ConfigKey { get; set; }
    public string? ConfigValue { get; set; }
    public string? ConfigType { get; set; } // String, Boolean, Integer, JSON
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsSystemConfig { get; set; }
    public bool IsEncrypted { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

/// <summary>
/// System audit log model
/// </summary>
public class SystemAuditModel
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>
/// System metrics model
/// </summary>
public class SystemMetricsModel
{
    public DateTime Timestamp { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalRequests { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int ErrorCount { get; set; }
    public decimal ErrorRate { get; set; }
    public long TotalDatabaseQueries { get; set; }
    public decimal AverageQueryTime { get; set; }
    public Dictionary<string, int>? EndpointHitCounts { get; set; }
}

/// <summary>
/// Maintenance schedule model
/// </summary>
public class MaintenanceScheduleModel
{
    public int Id { get; set; }
    public string TaskName { get; set; }
    public string? Description { get; set; }
    public string TaskType { get; set; }
    public string Schedule { get; set; } // Cron expression
    public bool IsEnabled { get; set; }
    public DateTime? LastRunDate { get; set; }
    public DateTime? NextRunDate { get; set; }
    public string? LastRunStatus { get; set; }
    public int? LastRunDurationSeconds { get; set; }
}
