# Admin Monitoring and Batch Management Guide

This guide explains the enhanced admin dashboard capabilities for monitoring external data sync operations, viewing batch progress, and managing running executions.

## 🎯 Features Overview

### 1. Enhanced Dashboard Navigation
- **Clickable Configurations**: Configuration names in the main dashboard are now clickable links
- **Configuration Details View**: Dedicated page for each configuration with comprehensive execution history
- **Real-time Updates**: Auto-refresh functionality with customizable intervals

### 2. Execution History Monitoring
- **Comprehensive Execution List**: View all historical executions for a specific configuration
- **Detailed Execution Information**: 
  - Execution ID and Batch ID
  - Start/End times and duration
  - Status and status messages
  - Record processing metrics (extracted, inserted, updated, failed)
  - Batch count and error count
  - Triggered by information (manual, scheduler, etc.)

### 3. Batch Progress Monitoring
- **Real-time Batch Status**: View individual batch processing within executions
- **Batch-level Metrics**:
  - Records in each batch
  - Processing progress percentage
  - Individual batch status (Processing, Completed, Failed, etc.)
  - Duration and performance metrics
  - Error counts per batch

### 4. Error Log Management
- **Comprehensive Error Tracking**: View all errors for specific executions
- **Error Details**:
  - Error types and codes
  - Error messages and stack traces
  - Record-specific context (record key, field name, field value)
  - Batch association for batch-specific errors
  - Resolution tracking
- **Error Summary Statistics**: Grouped by error type and batch

### 5. Execution Control
- **Manual Execution**: Start sync for specific configurations
- **Execution Cancellation**: Cancel running executions with confirmation
- **Bulk Operations**: Execute all enabled configurations

## 🚀 Getting Started

### Accessing the Admin Dashboard

1. **Main Dashboard**: Navigate to `/admin`
   - Shows system overview with health metrics
   - Lists all configurations with quick status indicators
   - Provides quick action buttons

2. **Configuration Details**: Click on any configuration name or visit `/admin/configuration/{configurationName}`
   - Shows execution history for that specific configuration
   - Provides detailed monitoring capabilities
   - Allows configuration-specific actions

### Understanding the Interface

#### Main Dashboard Components
- **System Health Metrics**: Total configurations, enabled count, running executions, unresolved errors
- **Configuration Cards**: Color-coded status indicators with execution buttons
- **Auto-refresh Controls**: Customizable refresh intervals (default: 30 seconds)

#### Configuration Details Components
- **Execution History Tab**: Paginated list of all executions
- **Live Monitor Tab**: Real-time monitoring for running executions
- **Quick Actions Panel**: Execute now and cancel running execution buttons

## 🔍 Monitoring Capabilities

### 1. Execution Status Indicators

| Status | Indicator | Description |
|--------|-----------|-------------|
| Running | 🔵 Spinner | Currently executing |
| Completed | ✅ Green | Successfully completed |
| CompletedWithErrors | ⚠️ Yellow | Completed but with non-fatal errors |
| Failed | ❌ Red | Failed to complete |
| Cancelled | ⏸️ Gray | Cancelled by user |

### 2. Batch Status Monitoring

#### Batch Status Types:
- **Pending**: Waiting to be processed
- **Processing**: Currently being processed
- **Completed**: Successfully processed all records
- **PartiallyCompleted**: Completed with some errors
- **Failed**: Failed to process
- **Skipped**: Batch was skipped

#### Progress Calculation:
- Overall execution progress based on batch completion
- Individual batch progress based on record processing
- Real-time updates for running executions

### 3. Error Classification

#### Error Types:
- **ConnectionError**: Database or external system connectivity issues
- **QueryError**: SQL query execution problems
- **DataTransformationError**: Data format or transformation issues
- **ForeignKeyResolutionError**: Foreign key lookup failures
- **DatabaseInsertError**: Record insertion failures
- **DatabaseUpdateError**: Record update failures
- **ValidationError**: Data validation failures
- **ConfigurationError**: Configuration setup problems
- **TimeoutError**: Operation timeout issues
- **UnknownError**: Unclassified errors

## 🛠️ API Endpoints

### Configuration-Specific Endpoints

```
GET /admin/configuration/{configurationName}
- Configuration details view page

GET /admin/api/configuration/{configurationName}/executions
- Get execution history with pagination
- Query parameters: limit (default: 20), offset (default: 0)

GET /admin/api/execution/{executionId}/batches
- Get detailed batch information for an execution
- Includes batch progress and performance metrics

GET /admin/api/execution/{executionId}/errors
- Get error logs for a specific execution
- Includes error summaries and statistics

POST /admin/api/execution/{executionId}/cancel
- Cancel a running execution
- Requires execution to be in "Running" status
```

### Response Formats

#### Execution History Response:
```json
{
  "configurationName": "example-config",
  "executions": [
    {
      "Id": 123,
      "BatchId": "uuid-here",
      "StartTime": "2024-01-01T10:00:00Z",
      "EndTime": "2024-01-01T10:05:00Z",
      "Status": "Completed",
      "TotalRecordsExtracted": 1000,
      "RecordsInserted": 950,
      "RecordsUpdated": 50,
      "RecordsFailed": 0,
      "Duration": 300,
      "BatchCount": 10,
      "ErrorCount": 0,
      "IsRunning": false
    }
  ],
  "hasMore": false,
  "total": 1
}
```

#### Batch Details Response:
```json
{
  "executionId": 123,
  "configurationName": "example-config",
  "batches": [
    {
      "Id": 456,
      "BatchNumber": 1,
      "Status": "Completed",
      "RecordsInBatch": 100,
      "RecordsInserted": 95,
      "RecordsUpdated": 5,
      "RecordsFailed": 0,
      "Progress": 100.0,
      "ProcessingDurationSeconds": 30.5
    }
  ],
  "summary": {
    "totalBatches": 10,
    "completedBatches": 10,
    "overallProgress": 100.0
  }
}
```

## 🔧 Configuration

### Auto-refresh Settings
- Default refresh interval: 30 seconds
- Can be paused/resumed via UI controls
- Countdown timer shows next refresh time

### Pagination Settings
- Default page size: 20 executions
- Load more functionality for viewing older executions
- Real-time updates for new executions

## 🚨 Troubleshooting

### Common Issues

1. **Configuration Not Found**
   - Ensure configuration file exists and is properly formatted
   - Check configuration name spelling and case sensitivity

2. **Execution Cancellation**
   - Only running executions can be cancelled
   - Cancellation marks execution as "Cancelled" in database
   - For distributed deployments, cancellation is database-only (graceful)

3. **Performance Considerations**
   - Large execution histories may take time to load
   - Use pagination for better performance
   - Consider archiving old execution logs

### Monitoring Tips

1. **Use Filters**: Focus on specific time ranges or execution types
2. **Watch Error Patterns**: Look for recurring error types across batches
3. **Monitor Performance**: Track duration trends and batch processing times
4. **Set Alerts**: Use error counts and failure rates for alerting

## 🔐 Security

### Authentication
- Production environments require API key authentication
- Set `AdminApiKey` configuration for secured access
- Development mode allows unauthenticated access

### Authorization
- Admin interface provides full system control
- Ensure proper access controls in production
- Monitor admin activity through logs

## 📈 Best Practices

### Monitoring Strategy
1. **Regular Health Checks**: Monitor system health metrics daily
2. **Error Resolution**: Address unresolved errors promptly  
3. **Performance Tracking**: Monitor execution durations and success rates
4. **Capacity Planning**: Track record volumes and processing times

### Operational Excellence
1. **Documentation**: Keep configuration documentation up to date
2. **Testing**: Test configurations in development before production
3. **Monitoring**: Set up alerts for critical failures
4. **Maintenance**: Regular cleanup of old execution logs

## 🆘 Support

For additional support or feature requests:
1. Check the main README.md for system setup
2. Review configuration examples in the config/ directory
3. Check logs for detailed error information
4. Consult the IMPLEMENTATION_SUMMARY.md for technical details
