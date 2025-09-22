# External Data Integration Service Implementation Plan

## Project Overview

This document outlines the implementation strategy for a configurable external data integration service that synchronizes data from BigQuery sources into the UNOPS PAO application database. The service operates on a configuration-driven approach, allowing multiple data sources to be integrated without code changes.

## Service Requirements Summary

### Core Functionality
1. **Configuration-Driven**: Read multiple configuration files from a designated folder
2. **BigQuery Integration**: Connect to and query BigQuery instances based on configuration
3. **Dynamic Table Management**: Create and update destination tables automatically
4. **Foreign Key Translation**: Map external references to internal application IDs
5. **Data Synchronization**: Maintain data consistency while preserving internal ID sequences
6. **Scheduled Execution**: Run automatically on a scheduled basis for ongoing updates

### Key Constraints
- Destination tables must follow application data model (Id PK with identity generation)
- All destination tables automatically include standard audit fields: CreatedBy (0), CreatedDate, LastModifiedBy (0), LastModifiedDate, plus sync-specific fields
- No domain entities, managers, or controllers created initially (future enhancement)
- FK relationships must be resolved through configuration-based lookup tables
- Source PK values used for record matching and updates

## Technical Architecture

### Technology Stack
- **Backend**: .NET 8 Service/Worker
- **Database**: PostgreSQL with Entity Framework Core
- **External Source**: Google BigQuery
- **Configuration**: YAML (recommended for readability and complex structures)
- **Scheduling**: Hosted Service with Quartz.NET or similar
- **Logging**: Serilog with structured logging

## Implementation Strategy

### Phase 1: Configuration System Design

#### 1.1 Configuration File Format

**Recommended Format**: YAML (better readability for complex nested structures)

```yaml
# config/partner-sync.yaml
metadata:
  name: "Partner Data Sync"
  description: "Synchronizes partner data from external CRM system"
  version: "1.0"
  enabled: true
  schedule_cron: "0 2 * * *" # Daily at 2 AM
  max_retry_attempts: 3
  timeout_minutes: 30

source:
  type: "bigquery"
  connection:
    project_id: "external-crm-project"
    dataset_id: "partners_dataset"
    credentials_file: "path/to/service-account.json"
    # Alternative: use environment variables for credentials
    use_environment_auth: true
  
  query: |
    SELECT 
      partner_external_id,
      partner_name,
      partner_type,
      registration_number,
      country_code,
      region_name,
      contact_email,
      status,
      created_date,
      last_modified_date
    FROM `external-crm-project.partners_dataset.active_partners`
    WHERE last_modified_date >= @last_sync_date
    ORDER BY last_modified_date ASC
  
  primary_key_field: "partner_external_id"
  incremental_field: "last_modified_date" # For incremental syncs
  batch_size: 1000

destination:
  table_name: "ExternalPartners"
  schema: "external" # Optional schema prefix
  
  # Field mapping and transformations
  field_mappings:
    - source_field: "partner_external_id"
      destination_field: "ExternalId"
      data_type: "varchar(50)"
      is_required: true
      is_unique: true
    
    - source_field: "partner_name" 
      destination_field: "Name"
      data_type: "varchar(255)"
      is_required: true
      transformations:
        - type: "trim"
        - type: "title_case"
    
    - source_field: "partner_type"
      destination_field: "PartnerType"
      data_type: "varchar(100)"
      is_required: false
    
    - source_field: "registration_number"
      destination_field: "RegistrationNumber" 
      data_type: "varchar(100)"
      is_required: false
    
    - source_field: "contact_email"
      destination_field: "ContactEmail"
      data_type: "varchar(255)"
      is_required: false
      transformations:
        - type: "lowercase"
        - type: "validate_email"
    
    - source_field: "status"
      destination_field: "Status"
      data_type: "varchar(50)"
      is_required: true
      default_value: "Active"
    
    - source_field: "created_date"
      destination_field: "ExternalCreatedDate"
      data_type: "timestamp"
      is_required: false
    
    - source_field: "last_modified_date"
      destination_field: "ExternalModifiedDate" 
      data_type: "timestamp"
      is_required: false

  # Foreign Key Mappings
  foreign_key_mappings:
    - source_field: "country_code"          # Field from BigQuery result
      lookup_table: "Countries"             # Existing app table to lookup from
      lookup_field: "CountryCode"           # Field in lookup table to match against
      destination_field: "CountryId"        # FK field in destination table
      lookup_return_field: "Id"             # Field to return from lookup table (usually Id)
      is_required: false
      on_lookup_fail: "log_warning"         # Options: log_warning, set_null, fail_record
      
    - source_field: "region_name"
      lookup_table: "GeoRegions" 
      lookup_field: "RegionName"
      destination_field: "RegionId"
      lookup_return_field: "Id"
      is_required: false
      on_lookup_fail: "set_null"

  # Audit fields (automatically added)
  audit_fields:
    created_by: "CreatedBy"              # Always set to 0 for external data
    created_date: "CreatedDate"
    last_modified_by: "LastModifiedBy"  # Always set to 0 for external data
    last_modified_date: "LastModifiedDate"
    sync_date: "SyncDate"
    sync_batch_id: "SyncBatchId"
    source_system: "SourceSystem"
    is_deleted: "IsDeleted"

sync_options:
  sync_mode: "upsert"                    # Options: upsert, insert_only, full_replace
  delete_missing_records: false         # Mark records as deleted if not in source
  preserve_destination_fields:          # Fields to never overwrite
    - "Id"
    - "CreatedDate"
    - "CreatedBy"
  
  conflict_resolution: "source_wins"     # Options: source_wins, destination_wins, manual
  batch_processing: true
  parallel_processing: true
  max_parallel_batches: 4

validation:
  required_source_fields:
    - "partner_external_id"
    - "partner_name"
  
  data_quality_checks:
    - field: "contact_email"
      rule: "valid_email_format"
      action: "log_warning"
    
    - field: "partner_name"
      rule: "min_length:2"
      action: "fail_record"
  
  duplicate_handling:
    check_field: "partner_external_id"
    action: "update_latest"              # Options: update_latest, fail, ignore

error_handling:
  on_connection_error: "retry_with_backoff"
  on_query_error: "fail_batch"
  on_mapping_error: "skip_record"
  on_fk_lookup_error: "log_and_continue"
  
  notification:
    on_failure: true
    on_success: false
    on_warning: true
    email_recipients:
      - "admin@unops.org"
    slack_webhook: "https://hooks.slack.com/..."

logging:
  log_level: "Information"
  log_sql_queries: true
  log_record_details: false             # Set to true for debugging
  retention_days: 30
```

#### 1.2 Configuration Validation Schema

Create JSON Schema for YAML validation:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["metadata", "source", "destination"],
  "properties": {
    "metadata": {
      "type": "object",
      "required": ["name", "enabled"],
      "properties": {
        "name": {"type": "string"},
        "description": {"type": "string"},
        "version": {"type": "string"},
        "enabled": {"type": "boolean"},
        "schedule_cron": {"type": "string"},
        "max_retry_attempts": {"type": "integer", "minimum": 1, "maximum": 10},
        "timeout_minutes": {"type": "integer", "minimum": 1}
      }
    },
    "source": {
      "type": "object",
      "required": ["type", "connection", "query", "primary_key_field"],
      "properties": {
        "type": {"enum": ["bigquery"]},
        "connection": {
          "type": "object",
          "required": ["project_id"],
          "properties": {
            "project_id": {"type": "string"},
            "dataset_id": {"type": "string"},
            "credentials_file": {"type": "string"},
            "use_environment_auth": {"type": "boolean"}
          }
        },
        "query": {"type": "string"},
        "primary_key_field": {"type": "string"},
        "incremental_field": {"type": "string"},
        "batch_size": {"type": "integer", "minimum": 1, "maximum": 10000}
      }
    },
    "destination": {
      "type": "object", 
      "required": ["table_name", "field_mappings"],
      "properties": {
        "table_name": {"type": "string"},
        "schema": {"type": "string"},
        "field_mappings": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "object",
            "required": ["source_field", "destination_field", "data_type"],
            "properties": {
              "source_field": {"type": "string"},
              "destination_field": {"type": "string"},
              "data_type": {"type": "string"},
              "is_required": {"type": "boolean"},
              "is_unique": {"type": "boolean"},
              "default_value": {"type": "string"},
              "transformations": {
                "type": "array",
                "items": {
                  "type": "object",
                  "required": ["type"],
                  "properties": {
                    "type": {"type": "string"},
                    "parameters": {"type": "object"}
                  }
                }
              }
            }
          }
        },
        "foreign_key_mappings": {
          "type": "array",
          "items": {
            "type": "object",
            "required": ["source_field", "lookup_table", "lookup_field", "destination_field"],
            "properties": {
              "source_field": {"type": "string"},
              "lookup_table": {"type": "string"},
              "lookup_field": {"type": "string"},
              "destination_field": {"type": "string"},
              "lookup_return_field": {"type": "string"},
              "is_required": {"type": "boolean"},
              "on_lookup_fail": {"enum": ["log_warning", "set_null", "fail_record"]}
            }
          }
        }
      }
    }
  }
}
```

### Phase 2: Core Service Architecture

#### 2.1 Service Structure

```
UNOPS.PAO.ExternalDataService/
├── Program.cs
├── Models/
│   ├── Configuration/
│   │   ├── SyncConfiguration.cs
│   │   ├── SourceConfiguration.cs
│   │   ├── DestinationConfiguration.cs
│   │   ├── FieldMapping.cs
│   │   └── ForeignKeyMapping.cs
│   ├── Sync/
│   │   ├── SyncBatch.cs
│   │   ├── SyncResult.cs
│   │   ├── SyncRecord.cs
│   │   └── SyncError.cs
│   └── External/
│       └── ExternalDataRecord.cs
├── Services/
│   ├── Configuration/
│   │   ├── IConfigurationService.cs
│   │   ├── ConfigurationService.cs
│   │   └── ConfigurationValidator.cs
│   ├── DataSource/
│   │   ├── IDataSourceService.cs
│   │   ├── BigQuerySourceService.cs
│   │   └── DataSourceFactory.cs
│   ├── Database/
│   │   ├── IDynamicTableService.cs
│   │   ├── DynamicTableService.cs
│   │   ├── IForeignKeyResolver.cs
│   │   └── ForeignKeyResolver.cs
│   ├── Sync/
│   │   ├── ISyncOrchestrator.cs
│   │   ├── SyncOrchestrator.cs
│   │   ├── ISyncProcessor.cs
│   │   └── SyncProcessor.cs
│   └── IExternalDataSyncService.cs
├── Workers/
│   └── SyncWorkerService.cs
├── Infrastructure/
│   ├── Database/
│   │   ├── ExternalDataDbContext.cs
│   │   └── Migrations/
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ConfigurationExtensions.cs
│   └── Utilities/
│       ├── SqlHelper.cs
│       ├── DataTypeMapper.cs
│       └── TransformationEngine.cs
└── appsettings.json
```

#### 2.2 Core Models

```csharp
// Models/Configuration/SyncConfiguration.cs
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
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool Enabled { get; set; } = true;
    public string ScheduleCron { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public int TimeoutMinutes { get; set; } = 30;
}

public class SourceConfiguration
{
    public string Type { get; set; } = "bigquery";
    public BigQueryConnection Connection { get; set; } = new();
    public string Query { get; set; } = string.Empty;
    public string PrimaryKeyField { get; set; } = string.Empty;
    public string? IncrementalField { get; set; }
    public int BatchSize { get; set; } = 1000;
}

public class BigQueryConnection
{
    public string ProjectId { get; set; } = string.Empty;
    public string? DatasetId { get; set; }
    public string? CredentialsFile { get; set; }
    public bool UseEnvironmentAuth { get; set; } = true;
}

public class DestinationConfiguration
{
    public string TableName { get; set; } = string.Empty;
    public string? Schema { get; set; }
    public List<FieldMapping> FieldMappings { get; set; } = new();
    public List<ForeignKeyMapping> ForeignKeyMappings { get; set; } = new();
    public AuditFieldsConfiguration AuditFields { get; set; } = new();
}

public class FieldMapping
{
    public string SourceField { get; set; } = string.Empty;
    public string DestinationField { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public bool IsUnique { get; set; } = false;
    public string? DefaultValue { get; set; }
    public List<FieldTransformation> Transformations { get; set; } = new();
}

public class FieldTransformation
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ForeignKeyMapping
{
    public string SourceField { get; set; } = string.Empty;
    public string LookupTable { get; set; } = string.Empty;
    public string LookupField { get; set; } = string.Empty;
    public string DestinationField { get; set; } = string.Empty;
    public string LookupReturnField { get; set; } = "Id";
    public bool IsRequired { get; set; } = false;
    public LookupFailAction OnLookupFail { get; set; } = LookupFailAction.LogWarning;
}

public enum LookupFailAction
{
    LogWarning,
    SetNull,
    FailRecord
}

public class AuditFieldsConfiguration
{
    public string CreatedBy { get; set; } = "CreatedBy";
    public string CreatedDate { get; set; } = "CreatedDate";
    public string LastModifiedBy { get; set; } = "LastModifiedBy";
    public string LastModifiedDate { get; set; } = "LastModifiedDate";
    public string SyncDate { get; set; } = "SyncDate";
    public string SyncBatchId { get; set; } = "SyncBatchId";
    public string SourceSystem { get; set; } = "SourceSystem";
    public string IsDeleted { get; set; } = "IsDeleted";
}
```

#### 2.2.1 Sync Logging Entities

The service requires additional database tables to track sync executions, batches, and errors:

```csharp
// Models/Sync/SyncExecutionLog.cs
public class SyncExecutionLog
{
    public int Id { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SyncStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    
    // Metrics
    public int TotalRecordsExtracted { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
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

// Models/Sync/SyncBatchLog.cs
public class SyncBatchLog
{
    public int Id { get; set; }
    public int SyncExecutionId { get; set; }
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

// Models/Sync/SyncErrorLog.cs
public class SyncErrorLog
{
    public int Id { get; set; }
    public int SyncExecutionId { get; set; }
    public int? SyncBatchId { get; set; }
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

// Models/Sync/ConfigurationExecutionHistory.cs
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

// Enums
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

public enum SyncBatchStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    PartiallyCompleted = 4,
    Skipped = 5
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

// Additional models needed for sync processing
public class SyncResult
{
    public string ConfigurationName { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SyncStatus Status { get; set; }
    public int TotalRecordsExtracted { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public List<SyncError> Errors { get; set; } = new();
}

public class SyncBatchResult
{
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public int RecordsSkipped { get; set; }
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
```

#### 2.2.2 Dedicated Sync Database Context

Create a separate `ExternalDataSyncDbContext` for better separation of concerns:

```csharp
// Infrastructure/Database/ExternalDataSyncDbContext.cs
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.ExternalDataService.Models.Sync;

public class ExternalDataSyncDbContext : DbContext
{
    public ExternalDataSyncDbContext(DbContextOptions<ExternalDataSyncDbContext> options)
        : base(options)
    {
    }

    // Sync logging tables
    public DbSet<SyncExecutionLog> SyncExecutionLogs { get; set; }
    public DbSet<SyncBatchLog> SyncBatchLogs { get; set; }
    public DbSet<SyncErrorLog> SyncErrorLogs { get; set; }
    public DbSet<ConfigurationExecutionHistory> ConfigurationExecutionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    
    // SyncExecutionLog configuration
    modelBuilder.Entity<SyncExecutionLog>(entity =>
    {
        entity.ToTable("SyncExecutionLogs", "external");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ConfigurationName).HasMaxLength(255).IsRequired();
        entity.Property(e => e.BatchId).HasMaxLength(50).IsRequired();
        entity.Property(e => e.StatusMessage).HasMaxLength(1000);
        entity.Property(e => e.TriggeredBy).HasMaxLength(50);
        entity.Property(e => e.TriggerId).HasMaxLength(255);
        entity.Property(e => e.CloudRunInstance).HasMaxLength(255);
        entity.Property(e => e.SourceQuery).HasColumnType("text");
        entity.Property(e => e.ConfigurationSnapshot).HasColumnType("jsonb");
        
        entity.HasIndex(e => e.ConfigurationName);
        entity.HasIndex(e => e.StartTime);
        entity.HasIndex(e => e.Status);
        entity.HasIndex(e => new { e.ConfigurationName, e.StartTime });
    });
    
    // SyncBatchLog configuration
    modelBuilder.Entity<SyncBatchLog>(entity =>
    {
        entity.ToTable("SyncBatchLogs", "external");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.BatchId).HasMaxLength(50).IsRequired();
        entity.Property(e => e.StatusMessage).HasMaxLength(1000);
        entity.Property(e => e.FirstRecordKey).HasMaxLength(255);
        entity.Property(e => e.LastRecordKey).HasMaxLength(255);
        
        entity.HasOne(e => e.SyncExecution)
              .WithMany(e => e.BatchLogs)
              .HasForeignKey(e => e.SyncExecutionId)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasIndex(e => e.SyncExecutionId);
        entity.HasIndex(e => e.StartTime);
        entity.HasIndex(e => e.Status);
    });
    
    // SyncErrorLog configuration
    modelBuilder.Entity<SyncErrorLog>(entity =>
    {
        entity.ToTable("SyncErrorLogs", "external");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ErrorCode).HasMaxLength(50).IsRequired();
        entity.Property(e => e.ErrorMessage).HasMaxLength(2000).IsRequired();
        entity.Property(e => e.ErrorDetails).HasColumnType("text");
        entity.Property(e => e.StackTrace).HasColumnType("text");
        entity.Property(e => e.RecordKey).HasMaxLength(255);
        entity.Property(e => e.RecordData).HasColumnType("jsonb");
        entity.Property(e => e.FieldName).HasMaxLength(255);
        entity.Property(e => e.FieldValue).HasMaxLength(1000);
        entity.Property(e => e.ResolutionNotes).HasMaxLength(2000);
        
        entity.HasOne(e => e.SyncExecution)
              .WithMany(e => e.ErrorLogs)
              .HasForeignKey(e => e.SyncExecutionId)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasOne(e => e.SyncBatch)
              .WithMany(e => e.ErrorLogs)
              .HasForeignKey(e => e.SyncBatchId)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasIndex(e => e.SyncExecutionId);
        entity.HasIndex(e => e.ErrorType);
        entity.HasIndex(e => e.OccurredAt);
        entity.HasIndex(e => e.IsResolved);
    });
    
    // ConfigurationExecutionHistory configuration
    modelBuilder.Entity<ConfigurationExecutionHistory>(entity =>
    {
        entity.ToTable("ConfigurationExecutionHistories", "external");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ConfigurationName).HasMaxLength(255).IsRequired();
        entity.Property(e => e.CurrentCronSchedule).HasMaxLength(100);
        
        entity.HasIndex(e => e.ConfigurationName).IsUnique();
        entity.HasIndex(e => e.LastSuccessfulExecution);
        entity.HasIndex(e => e.NextScheduledExecution);
    });
    }

    // Optional: Override SaveChangesAsync to add automatic audit field population
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-populate LastUpdatedAt fields
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is SyncExecutionLog execution && entry.State == EntityState.Modified)
            {
                execution.LastUpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ConfigurationExecutionHistory history && entry.State == EntityState.Modified)
            {
                history.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

#### 2.2.3 Sync Logging Service

Create a dedicated service for managing sync logs:

```csharp
// Services/Sync/ISyncLoggingService.cs
public interface ISyncLoggingService
{
    Task<SyncExecutionLog> StartSyncExecutionAsync(SyncConfiguration configuration, string triggeredBy, string? triggerId = null);
    Task UpdateSyncExecutionAsync(int executionId, SyncResult result);
    Task<SyncBatchLog> StartBatchAsync(int executionId, int batchNumber, int recordCount, string? firstRecordKey = null, string? lastRecordKey = null);
    Task CompleteBatchAsync(int batchId, SyncBatchResult result);
    Task LogErrorAsync(int executionId, int? batchId, SyncError error, string? recordKey = null, string? recordData = null);
    Task<DateTime?> GetLastSuccessfulSyncDateAsync(string configurationName);
    Task UpdateConfigurationHistoryAsync(string configurationName, SyncResult result);
    Task<IEnumerable<SyncExecutionLog>> GetRecentExecutionsAsync(string? configurationName = null, int count = 50);
    Task<SyncExecutionLog?> GetExecutionAsync(int executionId);
    Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(string? configurationName = null);
}

// Services/Sync/SyncLoggingService.cs
public class SyncLoggingService : ISyncLoggingService
{
    private readonly ExternalDataSyncDbContext _syncContext;
    private readonly ILogger<SyncLoggingService> _logger;
    private readonly IConfiguration _configuration;

    public SyncLoggingService(
        ExternalDataSyncDbContext syncContext,
        ILogger<SyncLoggingService> logger,
        IConfiguration configuration)
    {
        _syncContext = syncContext;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SyncExecutionLog> StartSyncExecutionAsync(
        SyncConfiguration configuration, 
        string triggeredBy, 
        string? triggerId = null)
    {
        var executionLog = new SyncExecutionLog
        {
            ConfigurationName = configuration.Metadata.Name,
            BatchId = Guid.NewGuid().ToString(),
            StartTime = DateTime.UtcNow,
            Status = SyncStatus.Running,
            StatusMessage = "Sync execution started",
            ConfigurationSnapshot = JsonSerializer.Serialize(configuration),
            SourceQuery = configuration.Source.Query,
            DestinationTable = configuration.Destination.TableName,
            TriggeredBy = triggeredBy,
            TriggerId = triggerId,
            CloudRunInstance = Environment.GetEnvironmentVariable("K_SERVICE") // Cloud Run service name
        };

        _syncContext.SyncExecutionLogs.Add(executionLog);
        await _syncContext.SaveChangesAsync();

        _logger.LogInformation("Started sync execution {ExecutionId} for configuration {ConfigName}", 
            executionLog.Id, configuration.Metadata.Name);

        return executionLog;
    }

    public async Task UpdateSyncExecutionAsync(int executionId, SyncResult result)
    {
        var execution = await _syncContext.SyncExecutionLogs.FindAsync(executionId);
        if (execution == null)
        {
            _logger.LogWarning("Sync execution {ExecutionId} not found for update", executionId);
            return;
        }

        execution.EndTime = DateTime.UtcNow;
        execution.Status = result.Status;
        execution.StatusMessage = result.Status == SyncStatus.Failed 
            ? string.Join("; ", result.Errors.Select(e => e.Message))
            : $"Completed: {result.RecordsInserted} inserted, {result.RecordsUpdated} updated, {result.RecordsFailed} failed";
        
        execution.TotalRecordsExtracted = result.TotalRecordsExtracted;
        execution.RecordsInserted = result.RecordsInserted;
        execution.RecordsUpdated = result.RecordsUpdated;
        execution.RecordsFailed = result.RecordsFailed;
        execution.Duration = execution.EndTime - execution.StartTime;
        execution.LastUpdatedAt = DateTime.UtcNow;

        await _syncContext.SaveChangesAsync();

        _logger.LogInformation("Updated sync execution {ExecutionId} with status {Status}", 
            executionId, result.Status);
    }

    public async Task<SyncBatchLog> StartBatchAsync(
        int executionId, 
        int batchNumber, 
        int recordCount, 
        string? firstRecordKey = null, 
        string? lastRecordKey = null)
    {
        var batchLog = new SyncBatchLog
        {
            SyncExecutionId = executionId,
            BatchId = Guid.NewGuid().ToString(),
            BatchNumber = batchNumber,
            StartTime = DateTime.UtcNow,
            Status = SyncBatchStatus.Processing,
            RecordsInBatch = recordCount,
            FirstRecordKey = firstRecordKey,
            LastRecordKey = lastRecordKey
        };

        _syncContext.SyncBatchLogs.Add(batchLog);
        await _syncContext.SaveChangesAsync();

        return batchLog;
    }

    public async Task CompleteBatchAsync(int batchId, SyncBatchResult result)
    {
        var batch = await _syncContext.SyncBatchLogs.FindAsync(batchId);
        if (batch == null)
        {
            _logger.LogWarning("Batch {BatchId} not found for completion", batchId);
            return;
        }

        batch.EndTime = DateTime.UtcNow;
        batch.Status = result.HasErrors ? SyncBatchStatus.PartiallyCompleted : SyncBatchStatus.Completed;
        batch.RecordsInserted = result.RecordsInserted;
        batch.RecordsUpdated = result.RecordsUpdated;
        batch.RecordsFailed = result.RecordsFailed;
        batch.RecordsSkipped = result.RecordsSkipped;
        batch.ProcessingDuration = batch.EndTime - batch.StartTime;

        await _syncContext.SaveChangesAsync();
    }

    public async Task LogErrorAsync(
        int executionId, 
        int? batchId, 
        SyncError error, 
        string? recordKey = null, 
        string? recordData = null)
    {
        var errorLog = new SyncErrorLog
        {
            SyncExecutionId = executionId,
            SyncBatchId = batchId,
            ErrorType = DetermineErrorType(error),
            ErrorCode = error.Code ?? "UNKNOWN",
            ErrorMessage = error.Message,
            ErrorDetails = error.Exception?.ToString(),
            StackTrace = error.Exception?.StackTrace,
            RecordKey = recordKey,
            RecordData = recordData,
            FieldName = error.FieldName,
            FieldValue = error.FieldValue
        };

        _syncContext.SyncErrorLogs.Add(errorLog);
        await _syncContext.SaveChangesAsync();
    }

    public async Task<DateTime?> GetLastSuccessfulSyncDateAsync(string configurationName)
    {
        var history = await _syncContext.ConfigurationExecutionHistories
            .FirstOrDefaultAsync(h => h.ConfigurationName == configurationName);
            
        return history?.LastIncrementalSyncDate ?? history?.LastSuccessfulExecution;
    }

    public async Task UpdateConfigurationHistoryAsync(string configurationName, SyncResult result)
    {
        var history = await _syncContext.ConfigurationExecutionHistories
            .FirstOrDefaultAsync(h => h.ConfigurationName == configurationName);

        if (history == null)
        {
            history = new ConfigurationExecutionHistory
            {
                ConfigurationName = configurationName,
                CreatedAt = DateTime.UtcNow
            };
            _syncContext.ConfigurationExecutionHistories.Add(history);
        }

        history.TotalExecutions++;
        history.TotalRecordsProcessed += result.TotalRecordsExtracted;
        history.LastUpdatedAt = DateTime.UtcNow;

        if (result.Status == SyncStatus.Completed || result.Status == SyncStatus.CompletedWithErrors)
        {
            history.LastSuccessfulExecution = DateTime.UtcNow;
            history.SuccessfulExecutions++;
            history.LastIncrementalSyncDate = DateTime.UtcNow; // Update for incremental syncs
        }
        else
        {
            history.LastFailedExecution = DateTime.UtcNow;
            history.FailedExecutions++;
        }

        // Update averages
        var duration = result.EndTime - result.StartTime;
        if (history.AverageExecutionDuration == null)
        {
            history.AverageExecutionDuration = duration;
        }
        else
        {
            // Simple moving average
            var currentMs = history.AverageExecutionDuration.Value.TotalMilliseconds;
            var newMs = duration.TotalMilliseconds;
            history.AverageExecutionDuration = TimeSpan.FromMilliseconds((currentMs + newMs) / 2);
        }

        await _syncContext.SaveChangesAsync();
    }

    private SyncErrorType DetermineErrorType(SyncError error)
    {
        if (error.Exception is SqlException) return SyncErrorType.DatabaseInsertError;
        if (error.Code?.Contains("FK") == true) return SyncErrorType.ForeignKeyResolutionError;
        if (error.Code?.Contains("VALIDATION") == true) return SyncErrorType.ValidationError;
        if (error.Code?.Contains("TIMEOUT") == true) return SyncErrorType.TimeoutError;
        if (error.Code?.Contains("QUERY") == true) return SyncErrorType.QueryError;
        if (error.Code?.Contains("CONNECTION") == true) return SyncErrorType.ConnectionError;
        
        return SyncErrorType.UnknownError;
    }

    // Additional query methods...
    public async Task<IEnumerable<SyncExecutionLog>> GetRecentExecutionsAsync(string? configurationName = null, int count = 50)
    {
        var query = _context.SyncExecutionLogs
            .Include(e => e.BatchLogs)
            .Include(e => e.ErrorLogs)
            .AsQueryable();

        if (!string.IsNullOrEmpty(configurationName))
        {
            query = query.Where(e => e.ConfigurationName == configurationName);
        }

        return await query
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<SyncExecutionLog?> GetExecutionAsync(int executionId)
    {
        return await _syncContext.SyncExecutionLogs
            .Include(e => e.BatchLogs)
            .Include(e => e.ErrorLogs)
            .FirstOrDefaultAsync(e => e.Id == executionId);
    }

    public async Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(string? configurationName = null)
    {
        var query = _context.SyncErrorLogs
            .Include(e => e.SyncExecution)
            .Where(e => !e.IsResolved);

        if (!string.IsNullOrEmpty(configurationName))
        {
            query = query.Where(e => e.SyncExecution.ConfigurationName == configurationName);
        }

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync();
    }
}
```

#### 2.3 Core Service Interfaces

```csharp
// Services/IExternalDataSyncService.cs
public interface IExternalDataSyncService
{
    Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncResult>> ExecuteAllConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<SyncResult> ExecuteConfigurationByNameAsync(string configurationName, CancellationToken cancellationToken = default);
}

// Services/Configuration/IConfigurationService.cs
public interface IConfigurationService
{
    Task<IEnumerable<SyncConfiguration>> LoadAllConfigurationsAsync();
    Task<SyncConfiguration?> LoadConfigurationAsync(string name);
    Task<bool> ValidateConfigurationAsync(SyncConfiguration configuration);
    Task RefreshConfigurationsAsync();
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
}

// Services/DataSource/IDataSourceService.cs
public interface IDataSourceService
{
    Task<IEnumerable<ExternalDataRecord>> ExtractDataAsync(SourceConfiguration sourceConfig, DateTime? lastSyncDate = null);
    Task<bool> TestConnectionAsync(SourceConfiguration sourceConfig);
    Task<DataSchema> GetSourceSchemaAsync(SourceConfiguration sourceConfig);
}

// Services/Database/IDynamicTableService.cs
public interface IDynamicTableService
{
    Task<bool> TableExistsAsync(string tableName, string? schema = null);
    Task CreateTableAsync(DestinationConfiguration destinationConfig);
    Task UpdateTableSchemaAsync(DestinationConfiguration destinationConfig);
    Task<TableSchema> GetTableSchemaAsync(string tableName, string? schema = null);
    Task<IEnumerable<string>> GetExistingRecordKeysAsync(string tableName, string keyField, IEnumerable<string> sourceKeys);
}

// Services/Database/IForeignKeyResolver.cs
public interface IForeignKeyResolver
{
    Task<Dictionary<string, int?>> ResolveForeignKeysAsync(
        ForeignKeyMapping mapping, 
        IEnumerable<string> sourceValues);
    
    Task<int?> ResolveSingleForeignKeyAsync(
        ForeignKeyMapping mapping, 
        string sourceValue);
}

// Services/Database/ForeignKeyResolver.cs
public class ForeignKeyResolver : IForeignKeyResolver
{
    private readonly UNOPSAppDbContext _appContext; // Uses main app context for FK lookups
    private readonly ILogger<ForeignKeyResolver> _logger;

    public ForeignKeyResolver(
        UNOPSAppDbContext appContext,
        ILogger<ForeignKeyResolver> logger)
    {
        _appContext = appContext;
        _logger = logger;
    }

    public async Task<Dictionary<string, int?>> ResolveForeignKeysAsync(
        ForeignKeyMapping mapping, 
        IEnumerable<string> sourceValues)
    {
        var result = new Dictionary<string, int?>();
        var distinctValues = sourceValues.Distinct().Where(v => !string.IsNullOrEmpty(v)).ToList();

        if (!distinctValues.Any())
            return result;

        try
        {
            // Build dynamic query to lookup FK values
            var sql = $@"
                SELECT {mapping.LookupField} as LookupValue, {mapping.LookupReturnField} as Id 
                FROM {mapping.LookupTable} 
                WHERE {mapping.LookupField} = ANY(@values)";

            var lookupResults = await _appContext.Database
                .SqlQueryRaw<ForeignKeyLookupResult>(sql, distinctValues.ToArray())
                .ToListAsync();

            // Map results back to source values
            foreach (var sourceValue in distinctValues)
            {
                var lookup = lookupResults.FirstOrDefault(r => 
                    string.Equals(r.LookupValue?.ToString(), sourceValue, StringComparison.OrdinalIgnoreCase));
                
                result[sourceValue] = lookup?.Id;

                if (lookup == null)
                {
                    _logger.LogWarning("Foreign key lookup failed for {LookupTable}.{LookupField} = '{SourceValue}'", 
                        mapping.LookupTable, mapping.LookupField, sourceValue);
                }
            }

            _logger.LogDebug("Resolved {ResolvedCount}/{TotalCount} foreign keys for {LookupTable}", 
                result.Count(r => r.Value.HasValue), distinctValues.Count, mapping.LookupTable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving foreign keys for {LookupTable}.{LookupField}", 
                mapping.LookupTable, mapping.LookupField);
                
            // Return empty results for all values on error
            foreach (var sourceValue in distinctValues)
            {
                result[sourceValue] = null;
            }
        }

        return result;
    }

    public async Task<int?> ResolveSingleForeignKeyAsync(
        ForeignKeyMapping mapping, 
        string sourceValue)
    {
        if (string.IsNullOrEmpty(sourceValue))
            return null;

        var results = await ResolveForeignKeysAsync(mapping, new[] { sourceValue });
        return results.GetValueOrDefault(sourceValue);
    }
}

// Helper class for FK lookup results
public class ForeignKeyLookupResult
{
    public object? LookupValue { get; set; }
    public int? Id { get; set; }
}

// Services/Sync/ISyncOrchestrator.cs
public interface ISyncOrchestrator
{
    Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default);
}
```

#### 2.2.3 Context Separation Strategy

The service uses two separate database contexts for clean separation of concerns:

1. **ExternalDataSyncDbContext**: Handles all sync logging and metadata
   - SyncExecutionLogs, SyncBatchLogs, SyncErrorLogs
   - ConfigurationExecutionHistory
   - Independent schema and migrations
   - Can be deployed to separate database if needed

2. **UNOPSAppDbContext**: Used only for FK resolution and destination table operations
   - Access to existing application tables (Countries, Partners, etc.)
   - Used by ForeignKeyResolver for lookup operations
   - Used by DynamicTableService for destination table management

**Benefits of this approach:**
- **Clean Separation**: Sync operations don't pollute main application context
- **Independent Scaling**: Sync database can be optimized differently
- **Easier Testing**: Can mock sync context without affecting main app data
- **Migration Independence**: Sync schema changes don't impact main app
- **Optional Database Separation**: Can move sync tables to separate database later if needed

### Phase 3: Implementation Details

#### 3.1 Configuration Service Implementation

```csharp
// Services/Configuration/ConfigurationService.cs
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConfigurationValidator _validator;
    private readonly Dictionary<string, SyncConfiguration> _configurations = new();
    private readonly FileSystemWatcher _fileWatcher;
    private readonly string _configurationPath;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(
        ILogger<ConfigurationService> logger,
        IConfiguration configuration,
        ConfigurationValidator validator)
    {
        _logger = logger;
        _configuration = configuration;
        _validator = validator;
        _configurationPath = _configuration["ExternalDataService:ConfigurationPath"] 
            ?? Path.Combine(AppContext.BaseDirectory, "config");

        InitializeFileWatcher();
    }

    public async Task<IEnumerable<SyncConfiguration>> LoadAllConfigurationsAsync()
    {
        var configurations = new List<SyncConfiguration>();
        var configFiles = Directory.GetFiles(_configurationPath, "*.yaml", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(_configurationPath, "*.yml", SearchOption.TopDirectoryOnly));

        foreach (var configFile in configFiles)
        {
            try
            {
                var config = await LoadConfigurationFromFileAsync(configFile);
                if (config != null && await _validator.ValidateAsync(config))
                {
                    configurations.Add(config);
                    _configurations[config.Metadata.Name] = config;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration from {ConfigFile}", configFile);
            }
        }

        _logger.LogInformation("Loaded {Count} valid configurations", configurations.Count);
        return configurations;
    }

    public async Task<SyncConfiguration?> LoadConfigurationAsync(string name)
    {
        if (_configurations.TryGetValue(name, out var cachedConfig))
        {
            return cachedConfig;
        }

        // Try to find and load the configuration file
        var configFiles = Directory.GetFiles(_configurationPath, "*.yaml", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(_configurationPath, "*.yml", SearchOption.TopDirectoryOnly));

        foreach (var configFile in configFiles)
        {
            var config = await LoadConfigurationFromFileAsync(configFile);
            if (config?.Metadata.Name == name)
            {
                if (await _validator.ValidateAsync(config))
                {
                    _configurations[name] = config;
                    return config;
                }
                break;
            }
        }

        return null;
    }

    private async Task<SyncConfiguration?> LoadConfigurationFromFileAsync(string filePath)
    {
        try
        {
            var yaml = await File.ReadAllTextAsync(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<SyncConfiguration>(yaml);
            
            _logger.LogDebug("Successfully loaded configuration from {FilePath}", filePath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse configuration file {FilePath}", filePath);
            return null;
        }
    }

    private void InitializeFileWatcher()
    {
        if (!Directory.Exists(_configurationPath))
        {
            Directory.CreateDirectory(_configurationPath);
        }

        _fileWatcher = new FileSystemWatcher(_configurationPath, "*.yaml")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
        };

        _fileWatcher.Changed += OnConfigurationFileChanged;
        _fileWatcher.Created += OnConfigurationFileChanged;
        _fileWatcher.Deleted += OnConfigurationFileChanged;
        _fileWatcher.Renamed += OnConfigurationFileRenamed;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private async void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        await Task.Delay(1000); // Debounce multiple rapid changes
        
        try
        {
            var config = await LoadConfigurationFromFileAsync(e.FullPath);
            if (config != null)
            {
                _configurations[config.Metadata.Name] = config;
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(config, ChangeType.Updated));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing configuration file change: {FilePath}", e.FullPath);
        }
    }
}
```

#### 3.2 BigQuery Data Source Service

```csharp
// Services/DataSource/BigQuerySourceService.cs
public class BigQuerySourceService : IDataSourceService
{
    private readonly ILogger<BigQuerySourceService> _logger;

    public BigQuerySourceService(ILogger<BigQuerySourceService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<ExternalDataRecord>> ExtractDataAsync(
        SourceConfiguration sourceConfig, 
        DateTime? lastSyncDate = null)
    {
        var client = await CreateBigQueryClientAsync(sourceConfig.Connection);
        
        var query = ProcessQueryParameters(sourceConfig.Query, lastSyncDate);
        var queryJob = await client.CreateQueryJobAsync(query);
        var result = await queryJob.GetQueryResultsAsync();

        var records = new List<ExternalDataRecord>();
        
        await foreach (var row in result.GetRowsAsync())
        {
            var record = new ExternalDataRecord
            {
                PrimaryKey = row[sourceConfig.PrimaryKeyField]?.ToString() ?? string.Empty,
                Data = new Dictionary<string, object?>()
            };

            foreach (var column in result.Schema.Fields)
            {
                var value = ConvertBigQueryValue(row[column.Name], column.Type);
                record.Data[column.Name] = value;
            }

            records.Add(record);
        }

        _logger.LogInformation("Extracted {Count} records from BigQuery", records.Count);
        return records;
    }

    public async Task<bool> TestConnectionAsync(SourceConfiguration sourceConfig)
    {
        try
        {
            var client = await CreateBigQueryClientAsync(sourceConfig.Connection);
            
            // Simple test query
            var testQuery = $"SELECT 1 as test_column LIMIT 1";
            var queryJob = await client.CreateQueryJobAsync(testQuery);
            var result = await queryJob.GetQueryResultsAsync();
            
            return await result.GetRowsAsync().AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BigQuery connection test failed");
            return false;
        }
    }

    private async Task<BigQueryClient> CreateBigQueryClientAsync(BigQueryConnection connection)
    {
        if (connection.UseEnvironmentAuth)
        {
            return BigQueryClient.Create(connection.ProjectId);
        }
        else if (!string.IsNullOrEmpty(connection.CredentialsFile))
        {
            var credential = GoogleCredential.FromFile(connection.CredentialsFile);
            return BigQueryClient.Create(connection.ProjectId, credential);
        }
        else
        {
            throw new InvalidOperationException("No valid authentication method configured for BigQuery");
        }
    }

    private string ProcessQueryParameters(string query, DateTime? lastSyncDate)
    {
        var processedQuery = query;
        
        if (lastSyncDate.HasValue && query.Contains("@last_sync_date"))
        {
            var parameterValue = lastSyncDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            processedQuery = processedQuery.Replace("@last_sync_date", $"'{parameterValue}'");
        }

        return processedQuery;
    }

    private object? ConvertBigQueryValue(object? value, BigQueryDbType type)
    {
        if (value == null) return null;

        return type switch
        {
            BigQueryDbType.String => value.ToString(),
            BigQueryDbType.Int64 => Convert.ToInt64(value),
            BigQueryDbType.Float64 => Convert.ToDouble(value),
            BigQueryDbType.Bool => Convert.ToBoolean(value),
            BigQueryDbType.DateTime => Convert.ToDateTime(value),
            BigQueryDbType.Date => DateOnly.FromDateTime(Convert.ToDateTime(value)),
            BigQueryDbType.Time => TimeOnly.FromTimeSpan(TimeSpan.Parse(value.ToString()!)),
            BigQueryDbType.Timestamp => Convert.ToDateTime(value),
            _ => value.ToString()
        };
    }
}
```

#### 3.3 Dynamic Table Service Implementation

```csharp
// Services/Database/DynamicTableService.cs
public class DynamicTableService : IDynamicTableService
{
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<DynamicTableService> _logger;
    private readonly DataTypeMapper _dataTypeMapper;

    public DynamicTableService(
        UNOPSAppDbContext context,
        ILogger<DynamicTableService> logger,
        DataTypeMapper dataTypeMapper)
    {
        _syncContext = context;
        _logger = logger;
        _dataTypeMapper = dataTypeMapper;
    }

    public async Task<bool> TableExistsAsync(string tableName, string? schema = null)
    {
        var fullTableName = schema != null ? $"{schema}.{tableName}" : tableName;
        
        var sql = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = @tableName 
            AND table_schema = @schema";

        var schemaName = schema ?? "public";
        var result = await _syncContext.Database.SqlQueryRaw<int>(sql, tableName, schemaName).FirstAsync();
        
        return result > 0;
    }

    public async Task CreateTableAsync(DestinationConfiguration destinationConfig)
    {
        var tableName = destinationConfig.TableName;
        var schema = destinationConfig.Schema ?? "public";
        var fullTableName = $"{schema}.{tableName}";

        var sqlBuilder = new StringBuilder();
        
        // Start table creation
        sqlBuilder.AppendLine($"CREATE TABLE {fullTableName} (");
        
        // Always add the Id column as primary key
        sqlBuilder.AppendLine("    Id SERIAL PRIMARY KEY,");

        // Add mapped fields
        foreach (var mapping in destinationConfig.FieldMappings)
        {
            var columnDefinition = BuildColumnDefinition(mapping);
            sqlBuilder.AppendLine($"    {columnDefinition},");
        }

        // Add foreign key columns
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            var nullable = fkMapping.IsRequired ? "NOT NULL" : "NULL";
            sqlBuilder.AppendLine($"    {fkMapping.DestinationField} INTEGER {nullable},");
        }

        // Add audit fields
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.CreatedBy} INTEGER NOT NULL DEFAULT 0,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.CreatedDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.LastModifiedBy} INTEGER NOT NULL DEFAULT 0,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.LastModifiedDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.SyncDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.SyncBatchId} VARCHAR(50) NOT NULL,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.SourceSystem} VARCHAR(100) NOT NULL,");
        sqlBuilder.AppendLine($"    {destinationConfig.AuditFields.IsDeleted} BOOLEAN NOT NULL DEFAULT FALSE");

        sqlBuilder.AppendLine(");");

        // Add indexes
        var uniqueFields = destinationConfig.FieldMappings.Where(f => f.IsUnique).ToList();
        foreach (var field in uniqueFields)
        {
            sqlBuilder.AppendLine($"CREATE UNIQUE INDEX IX_{tableName}_{field.DestinationField} ON {fullTableName} ({field.DestinationField});");
        }

        // Add foreign key constraints
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            sqlBuilder.AppendLine($@"
                ALTER TABLE {fullTableName} 
                ADD CONSTRAINT FK_{tableName}_{fkMapping.DestinationField}
                FOREIGN KEY ({fkMapping.DestinationField}) 
                REFERENCES {fkMapping.LookupTable}(Id);");
        }

        var sql = sqlBuilder.ToString();
        
        _logger.LogInformation("Creating table {TableName} with SQL: {SQL}", fullTableName, sql);
        
        await _syncContext.Database.ExecuteSqlRawAsync(sql);
        
        _logger.LogInformation("Successfully created table {TableName}", fullTableName);
    }

    public async Task UpdateTableSchemaAsync(DestinationConfiguration destinationConfig)
    {
        var tableName = destinationConfig.TableName;
        var schema = destinationConfig.Schema ?? "public";
        var fullTableName = $"{schema}.{tableName}";

        var currentSchema = await GetTableSchemaAsync(tableName, schema);
        var requiredColumns = GetRequiredColumns(destinationConfig);

        foreach (var column in requiredColumns)
        {
            if (!currentSchema.Columns.ContainsKey(column.Key))
            {
                var addColumnSql = $"ALTER TABLE {fullTableName} ADD COLUMN {column.Value}";
                await _syncContext.Database.ExecuteSqlRawAsync(addColumnSql);
                _logger.LogInformation("Added column {ColumnName} to table {TableName}", column.Key, fullTableName);
            }
        }
    }

    private string BuildColumnDefinition(FieldMapping mapping)
    {
        var postgresType = _dataTypeMapper.MapToPostgreSqlType(mapping.DataType);
        var nullable = mapping.IsRequired ? "NOT NULL" : "NULL";
        var defaultValue = !string.IsNullOrEmpty(mapping.DefaultValue) ? $"DEFAULT '{mapping.DefaultValue}'" : "";
        
        return $"{mapping.DestinationField} {postgresType} {nullable} {defaultValue}".Trim();
    }

    private Dictionary<string, string> GetRequiredColumns(DestinationConfiguration destinationConfig)
    {
        var columns = new Dictionary<string, string>();

        // Add mapped fields
        foreach (var mapping in destinationConfig.FieldMappings)
        {
            columns[mapping.DestinationField] = BuildColumnDefinition(mapping);
        }

        // Add FK columns
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            var nullable = fkMapping.IsRequired ? "NOT NULL" : "NULL";
            columns[fkMapping.DestinationField] = $"{fkMapping.DestinationField} INTEGER {nullable}";
        }

        // Add audit fields
        columns[destinationConfig.AuditFields.CreatedBy] = $"{destinationConfig.AuditFields.CreatedBy} INTEGER NOT NULL DEFAULT 0";
        columns[destinationConfig.AuditFields.CreatedDate] = $"{destinationConfig.AuditFields.CreatedDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.LastModifiedBy] = $"{destinationConfig.AuditFields.LastModifiedBy} INTEGER NOT NULL DEFAULT 0";
        columns[destinationConfig.AuditFields.LastModifiedDate] = $"{destinationConfig.AuditFields.LastModifiedDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.SyncDate] = $"{destinationConfig.AuditFields.SyncDate} TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.SyncBatchId] = $"{destinationConfig.AuditFields.SyncBatchId} VARCHAR(50) NOT NULL";
        columns[destinationConfig.AuditFields.SourceSystem] = $"{destinationConfig.AuditFields.SourceSystem} VARCHAR(100) NOT NULL";
        columns[destinationConfig.AuditFields.IsDeleted] = $"{destinationConfig.AuditFields.IsDeleted} BOOLEAN NOT NULL DEFAULT FALSE";

        return columns;
    }
}
```

#### 3.4 Sync Orchestrator Implementation

```csharp
// Services/Sync/SyncOrchestrator.cs
public class SyncOrchestrator : ISyncOrchestrator
{
    private readonly IDataSourceService _dataSourceService;
    private readonly IDynamicTableService _tableService;
    private readonly IForeignKeyResolver _fkResolver;
    private readonly ISyncProcessor _syncProcessor;
    private readonly ISyncLoggingService _syncLoggingService;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(
        IDataSourceService dataSourceService,
        IDynamicTableService tableService,
        IForeignKeyResolver fkResolver,
        ISyncProcessor syncProcessor,
        ISyncLoggingService syncLoggingService,
        ILogger<SyncOrchestrator> logger)
    {
        _dataSourceService = dataSourceService;
        _tableService = tableService;
        _fkResolver = fkResolver;
        _syncProcessor = syncProcessor;
        _syncLoggingService = syncLoggingService;
        _logger = logger;
    }

    public async Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default)
    {
        // Step 0: Start logging
        var executionLog = await _syncLoggingService.StartSyncExecutionAsync(
            configuration, 
            "scheduler", // This could be passed as a parameter
            Environment.GetEnvironmentVariable("K_SERVICE") ?? "manual"
        );

        var syncResult = new SyncResult
        {
            ConfigurationName = configuration.Metadata.Name,
            StartTime = DateTime.UtcNow,
            BatchId = executionLog.BatchId
        };

        try
        {
            _logger.LogInformation("Starting sync for configuration {ConfigName} (Execution ID: {ExecutionId})", 
                configuration.Metadata.Name, executionLog.Id);

            // Step 1: Prepare destination table
            await PrepareDestinationTableAsync(configuration.Destination);

            // Step 2: Extract data from source
            var lastSyncDate = await _syncLoggingService.GetLastSuccessfulSyncDateAsync(configuration.Metadata.Name);
            var sourceRecords = await _dataSourceService.ExtractDataAsync(configuration.Source, lastSyncDate);
            
            syncResult.TotalRecordsExtracted = sourceRecords.Count();

            if (!sourceRecords.Any())
            {
                _logger.LogInformation("No records to sync for configuration {ConfigName}", configuration.Metadata.Name);
                syncResult.Status = SyncStatus.Completed;
                return syncResult;
            }

            // Step 3: Resolve foreign keys
            var recordsWithResolvedFKs = await ResolveForeignKeysAsync(sourceRecords, configuration.Destination.ForeignKeyMappings);

            // Step 4: Process records in batches
            // Note: SyncProcessor will automatically populate audit fields:
            //   - CreatedBy/LastModifiedBy = 0 (external system user)
            //   - CreatedDate/LastModifiedDate = current timestamp
            //   - SyncDate = current timestamp, SyncBatchId = syncResult.BatchId
            var batches = recordsWithResolvedFKs.Chunk(configuration.Source.BatchSize);
            var batchNumber = 1;
            
            foreach (var batch in batches)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Start batch logging
                var firstRecordKey = batch.FirstOrDefault()?.PrimaryKey;
                var lastRecordKey = batch.LastOrDefault()?.PrimaryKey;
                var batchLog = await _syncLoggingService.StartBatchAsync(
                    executionLog.Id, 
                    batchNumber++, 
                    batch.Length, 
                    firstRecordKey, 
                    lastRecordKey
                );

                try
                {
                    var batchResult = await _syncProcessor.ProcessBatchAsync(batch, configuration, syncResult.BatchId);
                    
                    syncResult.RecordsInserted += batchResult.RecordsInserted;
                    syncResult.RecordsUpdated += batchResult.RecordsUpdated;
                    syncResult.RecordsFailed += batchResult.RecordsFailed;
                    syncResult.Errors.AddRange(batchResult.Errors);

                    // Complete batch logging
                    await _syncLoggingService.CompleteBatchAsync(batchLog.Id, batchResult);

                    // Log any batch-specific errors
                    foreach (var error in batchResult.Errors)
                    {
                        await _syncLoggingService.LogErrorAsync(
                            executionLog.Id, 
                            batchLog.Id, 
                            error, 
                            error.RecordKey, 
                            error.RecordData
                        );
                    }
                }
                catch (Exception batchEx)
                {
                    _logger.LogError(batchEx, "Error processing batch {BatchNumber} for configuration {ConfigName}", 
                        batchNumber - 1, configuration.Metadata.Name);
                        
                    var batchError = new SyncError 
                    { 
                        Message = $"Batch processing failed: {batchEx.Message}", 
                        Exception = batchEx,
                        Code = "BATCH_ERROR"
                    };
                    
                    syncResult.Errors.Add(batchError);
                    syncResult.RecordsFailed += batch.Length;
                    
                    await _syncLoggingService.LogErrorAsync(executionLog.Id, batchLog.Id, batchError);
                    
                    // Mark batch as failed
                    var failedBatchResult = new SyncBatchResult 
                    { 
                        HasErrors = true, 
                        RecordsFailed = batch.Length,
                        Errors = new List<SyncError> { batchError }
                    };
                    await _syncLoggingService.CompleteBatchAsync(batchLog.Id, failedBatchResult);
                }
            }

            // Step 5: Mark sync as complete
            syncResult.Status = syncResult.RecordsFailed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
            syncResult.EndTime = DateTime.UtcNow;

            // Update execution log
            await _syncLoggingService.UpdateSyncExecutionAsync(executionLog.Id, syncResult);
            
            // Update configuration history
            await _syncLoggingService.UpdateConfigurationHistoryAsync(configuration.Metadata.Name, syncResult);

            _logger.LogInformation("Sync completed for {ConfigName}: {Inserted} inserted, {Updated} updated, {Failed} failed",
                configuration.Metadata.Name, syncResult.RecordsInserted, syncResult.RecordsUpdated, syncResult.RecordsFailed);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed for configuration {ConfigName}", configuration.Metadata.Name);
            syncResult.Status = SyncStatus.Failed;
            syncResult.EndTime = DateTime.UtcNow;
            
            var syncError = new SyncError 
            { 
                Message = ex.Message, 
                Exception = ex, 
                Code = "SYNC_FAILURE"
            };
            syncResult.Errors.Add(syncError);
            
            // Log the failure
            await _syncLoggingService.LogErrorAsync(executionLog.Id, null, syncError);
            await _syncLoggingService.UpdateSyncExecutionAsync(executionLog.Id, syncResult);
            await _syncLoggingService.UpdateConfigurationHistoryAsync(configuration.Metadata.Name, syncResult);
        }
        finally
        {
            if (syncResult.EndTime == DateTime.MinValue)
            {
                syncResult.EndTime = DateTime.UtcNow;
            }
        }

        return syncResult;
    }

    private async Task PrepareDestinationTableAsync(DestinationConfiguration destination)
    {
        var tableExists = await _tableService.TableExistsAsync(destination.TableName, destination.Schema);
        
        if (!tableExists)
        {
            await _tableService.CreateTableAsync(destination);
        }
        else
        {
            await _tableService.UpdateTableSchemaAsync(destination);
        }
    }

    private async Task<IEnumerable<ExternalDataRecord>> ResolveForeignKeysAsync(
        IEnumerable<ExternalDataRecord> records, 
        List<ForeignKeyMapping> fkMappings)
    {
        foreach (var record in records)
        {
            record.ResolvedForeignKeys = new Dictionary<string, int?>();
            
            foreach (var fkMapping in fkMappings)
            {
                var sourceValue = record.Data.GetValueOrDefault(fkMapping.SourceField)?.ToString();
                if (!string.IsNullOrEmpty(sourceValue))
                {
                    var resolvedId = await _fkResolver.ResolveSingleForeignKeyAsync(fkMapping, sourceValue);
                    record.ResolvedForeignKeys[fkMapping.DestinationField] = resolvedId;
                }
            }
        }

        return records;
    }

    // These methods are now handled by ISyncLoggingService
    // No longer needed in the orchestrator
}
```

### Phase 4: Worker Service and Scheduling

#### 4.1 Cloud Run Deployment Strategy

Based on the current `DueDiligenceNotificationService` pattern and Google Cloud Platform deployment requirements, here are the recommended approaches:

**Option 1: Integrated Background Service (Recommended for Low-Frequency Syncs)**
- Deploy as part of the main Cloud Run instance
- Follow the same pattern as `DueDiligenceNotificationService`
- Use `AddHostedService<SyncWorkerService>()` registration
- Configure CPU to be always allocated to prevent throttling

**Option 2: Dedicated Cloud Run Instance (Recommended for High-Frequency/Resource-Intensive Syncs)**
- Deploy external data service as a separate Cloud Run instance
- Triggered by Cloud Scheduler via HTTP endpoints
- Better resource isolation and scaling control
- More cost-effective for infrequent but resource-intensive operations

**Option 3: Hybrid Approach (Recommended for Production)**
- Light scheduling service integrated in main app
- Heavy processing in dedicated Cloud Run instance triggered via Pub/Sub
- Best of both worlds: responsive scheduling with scalable processing

#### 4.1.1 Cloud Run Configuration Requirements

```yaml
# For integrated approach (Option 1)
apiVersion: serving.knative.dev/v1
kind: Service
metadata:
  annotations:
    run.googleapis.com/cpu-throttling: "false"  # Always allocate CPU
    run.googleapis.com/execution-environment: gen2
    run.googleapis.com/min-instances: "1"        # Keep at least 1 instance running
spec:
  template:
    metadata:
      annotations:
        run.googleapis.com/execution-environment: gen2
    spec:
      containers:
      - image: gcr.io/project/unops-pao-app
        resources:
          limits:
            cpu: "2"
            memory: "4Gi"
        env:
        - name: ExternalDataService__Enabled
          value: "true"
        - name: ExternalDataService__ConfigurationPath
          value: "/app/config/external-data"
```

```yaml
# For dedicated service approach (Option 2)
apiVersion: serving.knative.dev/v1
kind: Service
metadata:
  name: unops-pao-external-data-service
  annotations:
    run.googleapis.com/ingress: all
spec:
  template:
    spec:
      containers:
      - image: gcr.io/project/unops-pao-external-data-service
        resources:
          limits:
            cpu: "4"
            memory: "8Gi"
        env:
        - name: DATABASE_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: database-secrets
              key: connection-string
```

#### 4.1.2 Cloud Scheduler Configuration

```json
# Cloud Scheduler Job for Option 2 (Dedicated Service)
{
  "name": "external-data-sync-partners",
  "description": "Trigger partner data sync from external CRM",
  "schedule": "0 6 * * *",
  "timeZone": "UTC",
  "httpTarget": {
    "uri": "https://unops-pao-external-data-service-xyz-uc.a.run.app/api/sync/execute/partners-basic",
    "httpMethod": "POST",
    "headers": {
      "Content-Type": "application/json"
    },
    "oidcToken": {
      "serviceAccountEmail": "scheduler@project-id.iam.gserviceaccount.com"
    }
  },
  "retryConfig": {
    "retryCount": 3,
    "maxBackoffDuration": "300s",
    "minBackoffDuration": "5s",
    "maxRetryDuration": "3600s"
  }
}
```

#### 4.1.3 Pub/Sub Trigger Configuration (Option 3)

```json
# Pub/Sub Topic and Subscription
{
  "topic": {
    "name": "projects/project-id/topics/external-data-sync",
    "messageRetentionDuration": "86400s"
  },
  "subscription": {
    "name": "projects/project-id/subscriptions/external-data-sync-sub",
    "pushConfig": {
      "pushEndpoint": "https://unops-pao-external-data-service-xyz-uc.a.run.app/api/sync/pubsub",
      "oidcToken": {
        "serviceAccountEmail": "pubsub-invoker@project-id.iam.gserviceaccount.com"
      }
    },
    "retryPolicy": {
      "minimumBackoff": "10s",
      "maximumBackoff": "300s"
    }
  }
}
```

#### 4.2 Recommended Implementation: Option 2 (Dedicated Service)

**Rationale:**
- External data syncs are typically resource-intensive and long-running
- Better isolation prevents impact on main application performance
- Cloud Scheduler provides more reliable and observable scheduling than background timers
- Easier to scale resources independently based on sync requirements
- Better cost control (only runs when needed)

#### 4.3 Worker Service Implementation

```csharp
// For Option 1: Integrated Background Service (similar to DueDiligenceNotificationService)
// Workers/SyncWorkerService.cs
public class SyncWorkerService : BackgroundService
{
    private readonly ILogger<SyncWorkerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly int _checkIntervalMinutes;
    private readonly bool _enabled;

    public SyncWorkerService(
        ILogger<SyncWorkerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        var externalDataSettings = configuration.GetSection("ExternalDataService");
        _checkIntervalMinutes = int.Parse(externalDataSettings["CheckIntervalMinutes"] ?? "60");
        _enabled = bool.Parse(externalDataSettings["Enabled"] ?? "true");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("External Data Sync Service is disabled in configuration");
            return;
        }

        _logger.LogInformation("External Data Sync Service started. Check interval: {CheckIntervalMinutes} minutes", _checkIntervalMinutes);

        // Run immediately on startup
        await CheckAndExecuteScheduledSyncs();

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_checkIntervalMinutes));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;
                
            await CheckAndExecuteScheduledSyncs();
        }
    }

    private async Task CheckAndExecuteScheduledSyncs()
    {
        try
        {
            _logger.LogInformation("Checking for scheduled syncs...");

            using var scope = _serviceProvider.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            var syncService = scope.ServiceProvider.GetRequiredService<IExternalDataSyncService>();

            var configurations = await configService.LoadAllConfigurationsAsync();
            var now = DateTimeOffset.UtcNow;

            foreach (var config in configurations.Where(c => c.Metadata.Enabled && !string.IsNullOrEmpty(c.Metadata.ScheduleCron)))
            {
                try
                {
                    if (await ShouldExecuteSync(config, now))
                    {
                        _logger.LogInformation("Executing scheduled sync for {ConfigName}", config.Metadata.Name);
                        
                        var result = await syncService.ExecuteSyncAsync(config);
                        
                        _logger.LogInformation("Scheduled sync completed for {ConfigName} with status {Status}", 
                            config.Metadata.Name, result.Status);
                        
                        await RecordSyncExecution(config.Metadata.Name, now, result.Status);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing sync for configuration {ConfigName}", config.Metadata.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled sync check");
        }
    }

    private async Task<bool> ShouldExecuteSync(SyncConfiguration config, DateTimeOffset currentTime)
    {
        try
        {
            var cronExpression = CronExpression.Parse(config.Metadata.ScheduleCron);
            var lastExecution = await GetLastSyncExecution(config.Metadata.Name);
            
            if (lastExecution == null)
            {
                // Never executed, run if it's time
                var nextRun = cronExpression.GetPreviousOccurrence(currentTime);
                return nextRun.HasValue && (currentTime - nextRun.Value).TotalMinutes <= _checkIntervalMinutes;
            }
            
            // Check if it's time for the next execution
            var nextExecutionTime = cronExpression.GetNextOccurrence(lastExecution.Value);
            return nextExecutionTime.HasValue && nextExecutionTime.Value <= currentTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if sync should execute for {ConfigName}", config.Metadata.Name);
            return false;
        }
    }

    private async Task<DateTimeOffset?> GetLastSyncExecution(string configName)
    {
        using var scope = _serviceProvider.CreateScope();
        var syncLoggingService = scope.ServiceProvider.GetRequiredService<ISyncLoggingService>();
        
        var lastSync = await syncLoggingService.GetLastSuccessfulSyncDateAsync(configName);
        return lastSync?.ToUniversalTime();
    }

    private async Task RecordSyncExecution(string configName, DateTimeOffset executionTime, SyncStatus status)
    {
        // This is handled by the SyncOrchestrator through SyncLoggingService
        // No additional recording needed here as the orchestrator manages all logging
    }
}

// For Option 2: Dedicated Cloud Run Service with HTTP Triggers
// Controllers/SyncController.cs
[Route("api/[controller]")]
[ApiController]
public class SyncController : ControllerBase
{
    private readonly IExternalDataSyncService _syncService;
    private readonly IConfigurationService _configService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        IExternalDataSyncService syncService,
        IConfigurationService configService,
        ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _configService = configService;
        _logger = logger;
    }

    [HttpPost("execute/{configurationName}")]
    public async Task<IActionResult> ExecuteSync(string configurationName)
    {
        try
        {
            _logger.LogInformation("Received sync request for configuration: {ConfigName}", configurationName);
            
            var result = await _syncService.ExecuteConfigurationByNameAsync(configurationName);
            
            if (result == null)
            {
                return NotFound(new { error = $"Configuration '{configurationName}' not found" });
            }

            var response = new
            {
                configurationName = result.ConfigurationName,
                status = result.Status.ToString(),
                startTime = result.StartTime,
                endTime = result.EndTime,
                duration = result.EndTime - result.StartTime,
                recordsExtracted = result.TotalRecordsExtracted,
                recordsInserted = result.RecordsInserted,
                recordsUpdated = result.RecordsUpdated,
                recordsFailed = result.RecordsFailed,
                errors = result.Errors.Select(e => e.Message)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sync for configuration {ConfigName}", configurationName);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("execute-all")]
    public async Task<IActionResult> ExecuteAllSyncs()
    {
        try
        {
            _logger.LogInformation("Received request to execute all sync configurations");
            
            var results = await _syncService.ExecuteAllConfigurationsAsync();
            
            var response = results.Select(result => new
            {
                configurationName = result.ConfigurationName,
                status = result.Status.ToString(),
                duration = result.EndTime - result.StartTime,
                recordsExtracted = result.TotalRecordsExtracted,
                recordsProcessed = result.RecordsInserted + result.RecordsUpdated,
                recordsFailed = result.RecordsFailed
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing all sync configurations");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("pubsub")]
    public async Task<IActionResult> HandlePubSubTrigger([FromBody] PubSubMessage pubSubMessage)
    {
        try
        {
            // Decode the Pub/Sub message
            var messageData = Encoding.UTF8.GetString(Convert.FromBase64String(pubSubMessage.Data));
            var triggerRequest = JsonSerializer.Deserialize<SyncTriggerRequest>(messageData);
            
            if (triggerRequest?.ConfigurationName == null)
            {
                return BadRequest(new { error = "Invalid Pub/Sub message format" });
            }

            _logger.LogInformation("Received Pub/Sub sync trigger for configuration: {ConfigName}", triggerRequest.ConfigurationName);
            
            var result = await _syncService.ExecuteConfigurationByNameAsync(triggerRequest.ConfigurationName);
            
            return Ok(new { message = "Sync completed", status = result?.Status.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Pub/Sub trigger");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var configurations = await _configService.LoadAllConfigurationsAsync();
            
            var status = configurations.Select(config => new
            {
                name = config.Metadata.Name,
                enabled = config.Metadata.Enabled,
                schedule = config.Metadata.ScheduleCron,
                description = config.Metadata.Description
            });

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

// Supporting models
public class PubSubMessage
{
    public string Data { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public string MessageId { get; set; } = string.Empty;
    public DateTime PublishTime { get; set; }
}

public class SyncTriggerRequest
{
    public string ConfigurationName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
```

## Implementation Timeline

### Phase 1: Core Infrastructure (3-4 weeks)
- **Week 1**: Configuration system and models
- **Week 2**: Data source services (BigQuery integration)
- **Week 3**: Dynamic table management service
- **Week 4**: Foreign key resolution and basic sync logic

### Phase 2: Sync Processing (2-3 weeks)
- **Week 1**: Sync orchestrator and processor implementation
- **Week 2**: Error handling, validation, and transformation engine
- **Week 3**: Testing and debugging

### Phase 3: Service Integration & Cloud Run Deployment (3-4 weeks)
- **Week 1**: HTTP controllers for Cloud Scheduler integration
- **Week 2**: Cloud Run configuration and deployment scripts
- **Week 3**: Cloud Scheduler and Pub/Sub setup
- **Week 4**: Integration testing with GCP services

### Phase 4: Production Readiness (2-3 weeks)
- **Week 1**: Monitoring, alerting, and logging integration with Google Cloud Operations
- **Week 2**: Security review, IAM setup, and performance optimization
- **Week 3**: Load testing, documentation, and go-live preparation

## Deployment Architecture Recommendation

### Recommended Approach: Option 2 (Dedicated Cloud Run Service)

**Benefits:**
1. **Resource Isolation**: External data syncs won't impact main application performance
2. **Cost Efficiency**: Only consumes resources when syncs are running
3. **Scalability**: Can allocate more CPU/memory specifically for data processing
4. **Reliability**: Cloud Scheduler provides more reliable scheduling than background timers
5. **Observability**: Better monitoring and logging of sync operations
6. **Maintenance**: Independent deployments and updates

**Architecture Components:**
```
┌─────────────────┐    ┌──────────────────┐    ┌────────────────────────┐
│ Cloud Scheduler │────│ HTTP POST Request │────│ External Data Service  │
│                 │    │                  │    │ (Dedicated Cloud Run)  │
├─────────────────┤    └──────────────────┘    ├────────────────────────┤
│ • Daily at 6 AM │                           │ • Configuration Files │
│ • Partners Sync │    ┌──────────────────┐    │ • BigQuery Client      │
│ • Projects Sync │    │   Pub/Sub Topic  │    │ • PostgreSQL Client    │
│ • Other Syncs   │    │  (Optional)      │    │ • Logging & Monitoring │
└─────────────────┘    └──────────────────┘    └────────────────────────┘
                                ↓                           ↓
                       ┌──────────────────┐    ┌────────────────────────┐
                       │  Event Triggers  │    │     UNOPS PAO DB      │
                       │  (Future)        │    │   (Cloud SQL/        │
                       └──────────────────┘    │    PostgreSQL)       │
                                                └────────────────────────┘
```

**Service Registration:**
```csharp
// For dedicated service - register only sync components
services.AddScoped<IExternalDataSyncService, ExternalDataSyncService>();
services.AddScoped<IConfigurationService, ConfigurationService>();
services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
// ... other sync-related services

// No background service registration - use HTTP triggers instead
```

**Configuration:**
```json
// appsettings.json for dedicated service
{
  "ExternalDataService": {
    "ConfigurationPath": "/app/config",
    "BigQuery": {
      "DefaultProjectId": "your-project-id",
      "UseApplicationDefaultCredentials": true
    },
    "Database": {
      "ConnectionString": "${DATABASE_CONNECTION_STRING}"
    },
    "Logging": {
      "LogLevel": "Information",
      "RetentionDays": 30
    }
  }
}
```

## Configuration Examples

### Simple Partner Sync
```yaml
# config/partners-basic.yaml
metadata:
  name: "Basic Partner Sync"
  enabled: true
  schedule_cron: "0 6 * * *"  # Daily at 6 AM

source:
  type: bigquery
  connection:
    project_id: "crm-system-prod"
    use_environment_auth: true
  query: |
    SELECT 
      external_id,
      partner_name,
      country_code,
      created_date
    FROM partners.active_partners
    WHERE last_updated >= @last_sync_date
  primary_key_field: "external_id"
  incremental_field: "last_updated"

destination:
  table_name: "ExternalPartners"
  # Note: Audit fields (CreatedBy, CreatedDate, LastModifiedBy, LastModifiedDate, 
  #       SyncDate, SyncBatchId, SourceSystem, IsDeleted) are automatically added
  field_mappings:
    - source_field: "external_id"
      destination_field: "ExternalId" 
      data_type: "varchar(100)"
      is_required: true
      is_unique: true
    - source_field: "partner_name"
      destination_field: "Name"
      data_type: "varchar(255)"
      is_required: true
    - source_field: "created_date"
      destination_field: "ExternalCreatedDate"
      data_type: "timestamp"
  
  foreign_key_mappings:
    - source_field: "country_code"
      lookup_table: "Countries"
      lookup_field: "CountryCode" 
      destination_field: "CountryId"

sync_options:
  sync_mode: "upsert"
  batch_processing: true
```

### Complex Project Sync with Transformations
```yaml
# config/projects-advanced.yaml
metadata:
  name: "Advanced Project Sync"
  enabled: true
  schedule_cron: "0 */4 * * *"  # Every 4 hours

source:
  type: bigquery
  connection:
    project_id: "project-management-system"
    dataset_id: "projects"
    use_environment_auth: true
  query: |
    SELECT 
      p.project_id,
      p.project_name,
      p.status,
      p.start_date,
      p.end_date,
      p.budget_usd,
      p.partner_external_id,
      p.region_code,
      c.category_name,
      p.last_modified
    FROM projects p
    LEFT JOIN categories c ON p.category_id = c.id
    WHERE p.last_modified >= @last_sync_date
  primary_key_field: "project_id"
  batch_size: 500

destination:
  table_name: "ExternalProjects"
  schema: "integration"
  
  field_mappings:
    - source_field: "project_id"
      destination_field: "ExternalProjectId"
      data_type: "varchar(50)"
      is_required: true
      is_unique: true
    
    - source_field: "project_name"
      destination_field: "ProjectName"
      data_type: "varchar(500)"
      is_required: true
      transformations:
        - type: "trim"
        - type: "title_case"
    
    - source_field: "status"
      destination_field: "Status"
      data_type: "varchar(50)"
      is_required: true
      transformations:
        - type: "uppercase"
        - type: "status_mapping"
          parameters:
            mapping:
              "ACTIVE": "Active"
              "COMPLETED": "Completed"
              "ON_HOLD": "On Hold"
              "CANCELLED": "Cancelled"
    
    - source_field: "budget_usd"
      destination_field: "BudgetUSD"
      data_type: "decimal(18,2)"
      transformations:
        - type: "round"
          parameters:
            decimal_places: 2
    
    - source_field: "start_date"
      destination_field: "StartDate"
      data_type: "date"
      is_required: true
    
    - source_field: "end_date"
      destination_field: "EndDate"
      data_type: "date"
    
    - source_field: "category_name"
      destination_field: "CategoryName"
      data_type: "varchar(200)"

  foreign_key_mappings:
    - source_field: "partner_external_id"
      lookup_table: "ExternalPartners"
      lookup_field: "ExternalId"
      destination_field: "PartnerId"
      is_required: true
      on_lookup_fail: "fail_record"
    
    - source_field: "region_code"
      lookup_table: "GeoRegions"
      lookup_field: "RegionCode"
      destination_field: "RegionId"
      on_lookup_fail: "log_warning"

sync_options:
  sync_mode: "upsert"
  delete_missing_records: true
  conflict_resolution: "source_wins"
  parallel_processing: true
  max_parallel_batches: 3

validation:
  data_quality_checks:
    - field: "budget_usd"
      rule: "min_value:0"
      action: "log_warning"
    
    - field: "start_date"
      rule: "not_future_date"
      action: "log_warning"

error_handling:
  notification:
    on_failure: true
    email_recipients:
      - "integration-team@unops.org"
```

## Key Benefits

1. **Flexibility**: Configuration-driven approach allows new data sources without code changes
2. **Cloud-Native Scalability**: Optimized for Google Cloud Platform with auto-scaling capabilities
3. **Cost Efficiency**: Pay-per-use model with Cloud Run - only consume resources during sync operations
4. **Reliability**: Cloud Scheduler + Cloud Run provides enterprise-grade scheduling and execution
5. **Observability**: Integrated with Google Cloud Operations for comprehensive monitoring and logging
6. **Maintainability**: Clean separation between main application and data integration concerns
7. **Data Integrity**: FK resolution, data validation, and comprehensive audit trails
8. **Performance**: Incremental sync support, batch processing, and optimized database operations
9. **Security**: IAM-based access control and secure credential management
10. **Resilience**: Built-in retry mechanisms and error handling with Cloud Scheduler

## Cloud Run Deployment Commands

```bash
# Build and deploy dedicated external data service
gcloud run deploy unops-pao-external-data-service \
  --image gcr.io/PROJECT_ID/unops-pao-external-data-service \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated=false \
  --cpu-throttling=false \
  --min-instances 0 \
  --max-instances 10 \
  --memory 4Gi \
  --cpu 2 \
  --timeout 3600 \
  --set-env-vars="DATABASE_CONNECTION_STRING=${DB_CONNECTION_STRING}" \
  --set-env-vars="GOOGLE_CLOUD_PROJECT=${PROJECT_ID}"

# Create Cloud Scheduler jobs
gcloud scheduler jobs create http partner-sync-daily \
  --location=us-central1 \
  --schedule="0 6 * * *" \
  --time-zone="UTC" \
  --uri="https://unops-pao-external-data-service-xyz-uc.a.run.app/api/sync/execute/partners-basic" \
  --http-method=POST \
  --oidc-service-account-email="scheduler@PROJECT_ID.iam.gserviceaccount.com" \
  --max-retry-attempts=3 \
  --max-retry-duration=3600s

gcloud scheduler jobs create http projects-sync-daily \
  --location=us-central1 \
  --schedule="0 7 * * *" \
  --time-zone="UTC" \
  --uri="https://unops-pao-external-data-service-xyz-uc.a.run.app/api/sync/execute/projects-advanced" \
  --http-method=POST \
  --oidc-service-account-email="scheduler@PROJECT_ID.iam.gserviceaccount.com" \
  --max-retry-attempts=3 \
  --max-retry-duration=3600s
```

## Service Registration

For both deployment approaches, register the sync logging services in `Startup.cs` or `Program.cs`:

```csharp
// Add to Startup.cs ConfigureServices method or Program.cs

// Database contexts - separate contexts for clean separation
services.AddDbContext<UNOPSAppDbContext>(options =>
    options.UseNpgsql(connectionString));

services.AddDbContext<ExternalDataSyncDbContext>(options =>
    options.UseNpgsql(connectionString)); // Same DB, different context
    // Or use separate connection string: options.UseNpgsql(syncConnectionString)

// Core external data services
services.AddScoped<IExternalDataSyncService, ExternalDataSyncService>();
services.AddScoped<IConfigurationService, ConfigurationService>();
services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
services.AddScoped<ISyncProcessor, SyncProcessor>();
services.AddScoped<ISyncLoggingService, SyncLoggingService>();

// Data source services
services.AddScoped<IDataSourceService, BigQuerySourceService>();
services.AddScoped<DataSourceFactory>();

// Database services
services.AddScoped<IDynamicTableService, DynamicTableService>();
services.AddScoped<IForeignKeyResolver, ForeignKeyResolver>();

// Utilities
services.AddScoped<DataTypeMapper>();
services.AddScoped<TransformationEngine>();
services.AddScoped<ConfigurationValidator>();

// For Option 1: Integrated background service
services.AddHostedService<SyncWorkerService>();

// Google Cloud services
services.AddSingleton(BigQueryClient.Create(projectId));
```

## Database Migration

Generate and apply the database migration for sync logging tables using the dedicated sync context:

```bash
# Generate migration for sync logging tables
dotnet ef migrations add InitialSyncLogging --context ExternalDataSyncDbContext

# Apply migration
dotnet ef database update --context ExternalDataSyncDbContext
```

**Alternative: Separate Database Deployment**

If you want to deploy sync tables to a completely separate database:

```bash
# Use separate connection string in appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=main-db;Database=unops_pao;...",
    "SyncLoggingConnection": "Host=sync-db;Database=unops_pao_sync;..."
  }
}

# Register with separate connection
services.AddDbContext<ExternalDataSyncDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("SyncLoggingConnection")));

# Apply migrations to separate database
dotnet ef database update --context ExternalDataSyncDbContext --connection-string "Host=sync-db;Database=unops_pao_sync;..."
```

## Context Separation Benefits

Using separate database contexts (`ExternalDataSyncDbContext` vs `UNOPSAppDbContext`) provides several architectural advantages:

### **1. Clean Separation of Concerns**
- Sync logging is isolated from main application data
- No pollution of main application context with sync-specific entities
- Clear boundaries between operational and logging data

### **2. Independent Schema Management**
- Sync tables can have their own migration timeline
- Changes to sync schema don't affect main application
- Easier to add/modify logging fields without touching main app

### **3. Performance Optimization**
- Sync context can be tuned for write-heavy operations
- Main app context optimized for business logic
- Different connection pooling strategies if needed

### **4. Testing and Development**
- Mock sync context without affecting main app data
- Easier integration testing with isolated sync data
- Can disable sync logging in development environments

### **5. Deployment Flexibility**
- Start with same database, different contexts
- Migrate to separate databases later if needed
- Scale sync database independently

### **6. Operational Benefits**
- Separate backup strategies for sync vs business data
- Different retention policies (sync logs can be archived/purged)
- Independent monitoring and alerting

## Development Workflow and Local Testing

### Development Environment Setup

#### Configuration File Management

**Local Development Structure:**
```
UNOPS.PAO.ExternalDataService/
├── config/
│   ├── development/          # Local dev configs
│   │   ├── partners-test.yaml
│   │   ├── projects-test.yaml
│   │   └── sample-config.yaml
│   ├── staging/             # Staging configs
│   │   ├── partners-staging.yaml
│   │   └── projects-staging.yaml
│   └── production/          # Production configs (deployed separately)
│       ├── partners-prod.yaml
│       └── projects-prod.yaml
├── config-templates/        # Template files for developers
│   ├── basic-sync-template.yaml
│   └── complex-sync-template.yaml
└── appsettings.Development.json
```

**Configuration in appsettings.Development.json:**
```json
{
  "ExternalDataService": {
    "ConfigurationPath": "./config/development",
    "Enabled": true,
    "CheckIntervalMinutes": 1,  // Short interval for dev testing
    "AutoCreateTables": true,   // Automatically create destination tables
    "TestMode": true            // Enable test mode features
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=unops_pao_dev;Username=dev;Password=dev;",
    "SyncLoggingConnection": "Host=localhost;Database=unops_pao_sync_dev;Username=dev;Password=dev;"
  },
  "BigQuery": {
    "DefaultProjectId": "unops-dev-project",
    "UseEmulator": true,        // Use BigQuery emulator for local testing
    "EmulatorHost": "localhost:9050"
  }
}
```

#### Sample Development Configuration

Developers can start with this template (`config/development/partners-test.yaml`):

```yaml
metadata:
  name: "Partners Test Sync"
  description: "Local development test for partner data sync"
  enabled: true
  # No schedule for manual testing
  max_retry_attempts: 1
  timeout_minutes: 5

source:
  type: bigquery
  connection:
    project_id: "unops-dev-project"
    use_environment_auth: true
  query: |
    SELECT 
      'TEST-' || CAST(partner_id as STRING) as external_id,
      partner_name,
      'US' as country_code,
      CURRENT_TIMESTAMP() as created_date
    FROM `unops-dev-project.test_data.sample_partners`
    LIMIT 10  -- Small dataset for testing
  primary_key_field: "external_id"
  batch_size: 5  -- Small batches for testing

destination:
  table_name: "TestExternalPartners"
  field_mappings:
    - source_field: "external_id"
      destination_field: "ExternalId" 
      data_type: "varchar(100)"
      is_required: true
      is_unique: true
    - source_field: "partner_name"
      destination_field: "Name"
      data_type: "varchar(255)"
      is_required: true
    - source_field: "created_date"
      destination_field: "ExternalCreatedDate"
      data_type: "timestamp"
  
  foreign_key_mappings:
    - source_field: "country_code"
      lookup_table: "Countries"
      lookup_field: "CountryCode" 
      destination_field: "CountryId"
      on_lookup_fail: "log_warning"  # Don't fail in development

sync_options:
  sync_mode: "upsert"
  batch_processing: true

validation:
  required_source_fields:
    - "external_id"
    - "partner_name"

error_handling:
  on_connection_error: "retry_with_backoff"
  on_mapping_error: "skip_record"
  
logging:
  log_level: "Debug"
  log_sql_queries: true
  log_record_details: true  # Enable for development debugging
```

### Local Development Commands

#### Development CLI Interface

Create a simple CLI for developers:

```bash
# List all available configurations
dotnet run -- sync list

# Run a specific configuration
dotnet run -- sync run partners-test --verbose

# Test connection for a configuration
dotnet run -- sync test-connection partners-test

# Validate configuration file
dotnet run -- sync validate partners-test

# Initialize sync database
dotnet run -- sync init-db
```

#### Automatic Database Table Creation

Tables are created automatically during development when `AutoCreateTables: true` is set:

- **Sync logging tables**: Created automatically on first run via Entity Framework migrations
- **Destination tables**: Created automatically based on configuration field mappings
- **Schema updates**: Applied automatically when field mappings change

### Developer Admin Web Interface

#### Simple Admin Dashboard

For local development, create a lightweight admin interface accessible at `https://localhost:5001/admin`:

**Features:**
- **Configuration Overview**: List all available sync configurations with status
- **Manual Execution**: Click "Run" to execute individual configurations
- **Batch Execution**: "Execute All" button to run multiple configurations
- **Real-time Status**: View execution progress and completion status
- **Error Details**: Click on executions to see detailed error information
- **Performance Metrics**: View duration, records processed, and throughput

**Admin Controller Endpoints:**
```csharp
[Route("admin")]
public class AdminController : Controller
{
    [HttpGet] // Dashboard view
    public async Task<IActionResult> Index()
    
    [HttpPost("sync/{configurationName}")] // Execute single config
    public async Task<IActionResult> ExecuteSync(string configurationName)
    
    [HttpPost("sync-all")] // Execute all enabled configs
    public async Task<IActionResult> ExecuteAllSyncs()
    
    [HttpGet("execution/{executionId}")] // Get execution details
    public async Task<IActionResult> GetExecutionDetails(int executionId)
    
    [HttpGet("health")] // Health check
    public IActionResult Health()
}
```

**Dashboard Features:**
- ✅ **Status Cards**: Total configurations, enabled count, recent executions
- 📊 **Configuration List**: Name, description, destination table, enabled status
- ▶️ **Action Buttons**: Run, Validate buttons for each configuration
- 📝 **Execution History**: Recent runs with status, records processed, errors
- ⚠️ **Error Alerts**: Toast notifications for success/failure messages
- 🔄 **Auto-refresh**: Page refreshes after execution to show updated status

### Complete Development Workflow

#### Step-by-Step Developer Process

1. **Setup Local Environment:**
   ```bash
   # Clone repository and setup database
   git clone <repo-url>
   cd UNOPS.PAO.ExternalDataService
   docker run --name postgres-dev -e POSTGRES_PASSWORD=dev -d -p 5432:5432 postgres:13
   
   # Initialize sync database (tables created automatically)
   dotnet run -- sync init-db
   ```

2. **Create Configuration:**
   - Copy template from `config-templates/basic-sync-template.yaml`
   - Save as `config/development/my-test-sync.yaml`
   - Modify connection details and query for your data source

3. **Test Configuration:**
   ```bash
   dotnet run -- sync validate my-test-sync
   dotnet run -- sync test-connection my-test-sync
   dotnet run -- sync run my-test-sync --verbose
   ```

4. **Monitor Results:**
   - Open browser to `https://localhost:5001/admin`
   - View execution details, errors, and performance metrics
   - Use admin interface for easy testing and monitoring

#### Auto-Creation Features

**Database Tables:**
- ✅ **Sync logging tables**: Auto-created via EF migrations on startup
- ✅ **Destination tables**: Auto-created from configuration field mappings
- ✅ **Schema updates**: Applied automatically when mappings change
- ✅ **Foreign key constraints**: Added automatically based on FK mappings

**Development vs Production:**
- **Development**: `AutoCreateTables: true` - everything created automatically
- **Production**: `AutoCreateTables: false` - manual table creation required for safety

**Error Handling:**
- Missing tables trigger clear error messages with resolution steps
- Configuration validation catches common issues before execution
- Detailed logging shows exactly what tables/columns are being created

#### Environment-Specific Configuration

**Development Environment:**
- Small datasets with `LIMIT` clauses for fast testing
- Short execution intervals (1 minute) for rapid iteration
- Detailed logging with `log_record_details: true`
- Auto-table creation enabled for convenience
- Test mode features and validation relaxed

**Staging Environment:**
- Production-like data volumes for realistic testing
- Real scheduling intervals to validate timing
- Standard logging levels
- Manual table creation to match production process
- Full validation of production configurations

**Production Environment:**
- Full datasets with proper incremental sync support
- Production schedules via Cloud Scheduler
- Error-only logging for performance
- Strict validation and manual deployment
- Comprehensive monitoring and alerting

### Development Best Practices

#### Configuration Templates

Provide standard templates to speed up development:

```yaml
# config-templates/basic-sync-template.yaml
metadata:
  name: "CHANGE_ME - Descriptive Name"
  description: "CHANGE_ME - What does this sync do?"
  enabled: false  # Start disabled for safety

source:
  type: bigquery
  connection:
    project_id: "CHANGE_ME"
    use_environment_auth: true
  query: |
    SELECT 
      -- CHANGE_ME: Add your fields here
      id as external_id,
      name,
      updated_at
    FROM `CHANGE_ME.dataset.table`
    WHERE updated_at >= @last_sync_date
    LIMIT 10  -- Remove for production
  primary_key_field: "external_id"
  batch_size: 100

destination:
  table_name: "CHANGE_ME_TableName"
  field_mappings:
    - source_field: "external_id"
      destination_field: "ExternalId"
      data_type: "varchar(100)"
      is_required: true
      is_unique: true
    # Add more field mappings as needed

sync_options:
  sync_mode: "upsert"
  batch_processing: true

logging:
  log_level: "Information"
  log_record_details: false  # Set to true for debugging
```

#### Testing Checklist

**Before First Run:**
- ☐ Configuration validates successfully
- ☐ Source connection test passes
- ☐ Query returns expected data structure
- ☐ Field mappings align with source data
- ☐ FK mappings reference existing tables

**During Development:**
- ☐ Small test dataset (< 100 records) processes successfully
- ☐ All field transformations work correctly
- ☐ Foreign key resolution succeeds
- ☐ Error handling works as expected
- ☐ Incremental sync logic functions properly

**Before Production:**
- ☐ Full dataset test completed in staging
- ☐ Performance benchmarks meet requirements
- ☐ Monitoring and alerting configured
- ☐ Rollback plan documented
- ☐ Production tables pre-created and validated

This comprehensive development workflow ensures developers can quickly create, test, and deploy external data integrations with confidence, while maintaining the production-ready architecture and monitoring capabilities needed for enterprise-scale data operations., cloud-native foundation for integrating external data sources into the UNOPS PAO system while maintaining clean architectural boundaries and leveraging Google Cloud Platform's managed services for optimal performance, reliability, and cost-effectiveness.
