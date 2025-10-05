# Session Configuration Architecture

## Overview

This document describes the centralized session configuration architecture implemented to ensure consistent `app_name` usage across all layers of the AI assistant system. The architecture eliminates hardcoded configuration values and provides a single source of truth for session-related settings.

## Architecture Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   .NET Backend  │    │  Python AI      │
│   (Angular)     │    │   (GeminiMgr)   │    │   Service       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         │  API calls            │  Fetches config       │  Provides config
         │  (no app_name)        │  from Python          │  via /configuration
         │                       │                       │
         │                       │  Uses app_name        │  Returns:
         │                       │  internally           │  - app_name
         │                       │                       │  - application_name
         │                       │                       │  - project_name
         │                       │                       │  - organization
         │                       │                       │  - environment
         │                       │                       │  - version
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    Session Configuration
                    (Single Source of Truth)
```

## Key Principles

### 1. Single Source of Truth
- **Python AI Service** is the authoritative source for all session configuration
- Configuration is defined in Python service's config files
- No hardcoded values in frontend or .NET backend

### 2. No Configuration Masking
- If Python service is unavailable, .NET backend fails fast with clear error
- No fallback to `appsettings.json` that could mask Python service issues
- Forces proper diagnosis of underlying problems

### 3. Clean Separation of Concerns
- **Frontend**: Pure UI layer with no knowledge of session configuration
- **.NET Backend**: Handles all AI service integration and configuration management
- **Python Service**: Provides configuration and handles AI processing

## Implementation Details

### Python AI Service (Source of Truth)

#### Configuration Endpoint
- **Route**: `GET /api/ai-assistant/configuration`
- **Location**: `routers/session.py`
- **Purpose**: Provides session configuration to .NET backend

```python
@router.get("/configuration")
async def get_session_configuration():
    """
    Get session configuration including app_name and other session-related settings.
    This endpoint is called by the .NET backend to get the correct configuration.
    """
    try:
        config = get_config()
        branding_config = config.get('branding', {})
        
        session_config = {
            "app_name": get_application_name(),
            "application_name": branding_config.get('application_name', 'AI Agent'),
            "project_name": branding_config.get('project_name', 'AI Agent'),
            "organization": branding_config.get('organization', 'UNOPS'),
            "environment": config.get('environment', 'local'),
            "version": "1.0.0"
        }
        
        logger.info(f"📋 Returning session configuration: {session_config}")
        return JSONResponse(content=session_config)
        
    except Exception as e:
        logger.error(f"❌ Error getting session configuration: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to get session configuration: {str(e)}")
```

#### Configuration Sources
- Uses existing config system (`get_application_name()`, `get_config()`)
- Reads from Python service's configuration files
- Supports environment-specific configurations

### .NET Backend (Configuration Management)

#### Session Configuration Service
- **Location**: `UNOPSGeminiManager.cs`
- **Purpose**: Fetches and caches session configuration from Python service
- **Caching**: 1-hour cache to reduce API calls

```csharp
public async Task<SessionConfiguration> GetSessionConfigurationAsync()
{
    // Try to get from cache first
    if (_memoryCache.TryGetValue(_sessionConfigCacheKey, out SessionConfiguration cachedConfig))
    {
        _logger.LogDebug("📋 Retrieved session configuration from cache");
        return cachedConfig;
    }

    // If not in cache, fetch from Python service
    try
    {
        var serviceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
        if (string.IsNullOrEmpty(serviceUrl))
        {
            _logger.LogError("❌ AgenticAi:ServiceURL is not configured");
            throw new InvalidOperationException("AgenticAi:ServiceURL is not configured");
        }

        var configUrl = $"{serviceUrl.TrimEnd('/')}/api/ai-assistant/configuration";
        _logger.LogInformation("🔍 Fetching session configuration from: {ConfigUrl}", configUrl);

        var response = await _httpClient.GetAsync(configUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<SessionConfiguration>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
        {
            throw new InvalidOperationException("Failed to deserialize session configuration");
        }

        // Cache the configuration
        _memoryCache.Set(_sessionConfigCacheKey, config, _sessionConfigCacheExpiration);
        _logger.LogInformation("✅ Session configuration cached successfully: {AppName}", config.AppName);

        return config;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Failed to fetch session configuration from Python service");
        throw new InvalidOperationException("AI service is unavailable - cannot fetch session configuration", ex);
    }
}
```

#### Automatic Configuration Usage
All AI-related methods automatically use the session configuration:

```csharp
// Get app_name from session configuration
var sessionConfig = await GetSessionConfigurationAsync();
var appName = sessionConfig.AppName;

// Use appName in API calls to Python service
var apiUrl = $"/session-with-chats?app_name={appName}&user_id={userId}&session_id={sessionId}";
```

#### Methods Updated
- `ChatWithGemini()`
- `ChatWithGeminiStreaming()`
- `GetSessionDataWithChats()`
- `GetSessionData()`
- `GetUserSessions()`

### Frontend (Clean UI Layer)

#### No Configuration Knowledge
- Frontend has zero knowledge of session configuration
- No hardcoded `app_name` values
- Simplified API calls without configuration parameters

#### Before (Hardcoded)
```typescript
// OLD: Frontend managed configuration
formData.append('app_name', 'opportunityplus');
const response = await this.http.get(
  `${this.aiAssistantUrl}/session-with-chats?app_name=opportunityplus&user_id=${userId}&session_id=${sessionId}`
);
```

#### After (Clean)
```typescript
// NEW: Frontend focuses on UI only
const response = await this.http.get(
  `${this.aiAssistantUrl}/session-with-chats?user_id=${userId}&session_id=${sessionId}`
);
```

## Data Flow

### 1. Initial Request
```
Frontend → .NET Backend → Python Service
   ↓           ↓              ↓
API call   Get config    Return config
(no app_name)  from Python   (app_name, etc.)
```

### 2. Configuration Caching
```
.NET Backend caches configuration for 1 hour
↓
Subsequent requests use cached config
↓
Reduces API calls to Python service
```

### 3. AI Service Calls
```
.NET Backend uses cached app_name
↓
Makes calls to Python service with correct app_name
↓
Python service processes with consistent configuration
```

## Configuration Structure

### SessionConfiguration Class
```csharp
public class SessionConfiguration
{
    public string AppName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
```

### Python Response Format
```json
{
    "app_name": "opportunityplus",
    "application_name": "AI Agent",
    "project_name": "AI Agent",
    "organization": "UNOPS",
    "environment": "local",
    "version": "1.0.0"
}
```

## Error Handling

### No Fallback Strategy
- If Python service is down, .NET backend throws exception
- No fallback to `appsettings.json`
- Forces proper diagnosis of Python service issues

### Clear Error Messages
```csharp
throw new InvalidOperationException("AI service is unavailable - cannot fetch session configuration", ex);
```

### Frontend Error Handling
- Frontend receives clear error messages from .NET backend
- No configuration-related errors in frontend
- Clean error boundaries

## Benefits

### 1. Consistency
- Single source of truth for all configuration
- No hardcoded values scattered across codebase
- Automatic consistency across all AI service calls

### 2. Maintainability
- Configuration changes only need to be made in Python service
- No need to update multiple files for configuration changes
- Centralized configuration management

### 3. Reliability
- No configuration masking of underlying issues
- Clear error messages when services are unavailable
- Proper separation of concerns

### 4. Performance
- Caching reduces API calls to Python service
- Efficient configuration retrieval
- Minimal overhead

### 5. Scalability
- Easy to add new configuration parameters
- Supports environment-specific configurations
- Clean architecture for future enhancements

## Environment Support

### Development
- Python service returns development configuration
- .NET backend uses development `app_name`
- Frontend works with development settings

### Staging/Production
- Python service returns environment-specific configuration
- .NET backend automatically uses correct settings
- No code changes needed for different environments

## Monitoring and Debugging

### Logging
- Comprehensive logging at all layers
- Clear identification of configuration sources
- Error tracking for configuration issues

### Debug Information
```csharp
_logger.LogInformation("🔍 Fetching session configuration from: {ConfigUrl}", configUrl);
_logger.LogInformation("✅ Session configuration cached successfully: {AppName}", config.AppName);
```

### Python Logging
```python
logger.info(f"📋 Returning session configuration: {session_config}")
logger.error(f"❌ Error getting session configuration: {e}")
```

## Future Enhancements

### 1. Configuration Validation
- Add validation for configuration values
- Ensure required fields are present
- Validate configuration format

### 2. Dynamic Configuration Updates
- Support for configuration updates without restart
- Cache invalidation on configuration changes
- Real-time configuration synchronization

### 3. Configuration Versioning
- Track configuration changes
- Support for configuration rollback
- Version compatibility checks

### 4. Multi-Environment Support
- Support for multiple Python services
- Environment-specific configuration routing
- Load balancing for configuration services

## Troubleshooting

### Common Issues

#### 1. Python Service Unavailable
**Symptoms**: .NET backend throws "AI service is unavailable" error
**Solution**: Check Python service status and configuration

#### 2. Configuration Cache Issues
**Symptoms**: Old configuration values being used
**Solution**: Clear cache or restart .NET backend

#### 3. Frontend API Errors
**Symptoms**: Frontend receives 500 errors
**Solution**: Check .NET backend logs for configuration issues

### Debug Steps

1. **Check Python Service**
   ```bash
   curl http://python-service/api/ai-assistant/configuration
   ```

2. **Check .NET Backend Logs**
   - Look for configuration fetch errors
   - Check cache status
   - Verify API calls to Python service

3. **Check Frontend Network Tab**
   - Verify API calls are being made
   - Check for configuration-related errors
   - Ensure no hardcoded values

## Conclusion

This architecture provides a robust, maintainable, and scalable solution for session configuration management. By centralizing configuration in the Python service and having the .NET backend handle all configuration management internally, we achieve:

- **Single Source of Truth**: Python service is the authoritative source
- **Clean Separation**: Frontend focuses on UI, .NET handles integration
- **No Configuration Masking**: Clear error handling when services are unavailable
- **Automatic Consistency**: All AI service calls use correct configuration
- **Easy Maintenance**: Configuration changes only need to be made in one place

The architecture ensures that the AI assistant system is reliable, maintainable, and ready for production use across different environments.
