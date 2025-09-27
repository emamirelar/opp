# Streaming Chat Authentication Fix

## Problem Summary

The `chatWithFilesStreaming` method in `ai-assistant.service.ts` was using the native `fetch()` API, which bypassed Angular's HTTP interceptors responsible for adding authentication headers. This caused authentication failures when deployed to Cloud Run behind Google IAP, while working fine in development due to dev cookie simulation.

## Root Cause

- **Development**: Uses `DevelopmentIAPAuthHandler` middleware that accepts dev cookies
- **Production**: Requires proper Google IAP headers (`x-goog-authenticated-user-email`, `x-goog-authenticated-user-id`, etc.)
- **Issue**: `fetch()` bypasses Angular's `authInterceptor` that adds these headers automatically

## Solution Implemented

### HttpClient-based Streaming

**Method**: `chatWithFilesStreaming()`

```typescript
// Uses Angular HttpClient with proper interceptor support
return this.http.post(`${this.aiAssistantUrl}/chat`, formData, {
  headers: {
    'Accept': 'text/event-stream',
    'Cache-Control': 'no-cache, no-store, must-revalidate',
    'Pragma': 'no-cache',
    'Expires': '0'
  },
  responseType: 'text',
  observe: 'events',
  reportProgress: true
}).pipe(
  filter(event => event.type === HttpEventType.DownloadProgress || event.type === HttpEventType.Response),
  switchMap(event => this.parseStreamingResponse(event)),
  catchError(error => throwError(() => error))
);
```

**Benefits**:
- ✅ Automatic authentication via Angular interceptors
- ✅ Consistent with other services in the application
- ✅ Proper IAP header handling
- ✅ Built-in retry and error handling
- ✅ Clean, maintainable code without fallback complexity

## Usage Instructions

### Using the Updated Method

No changes needed in existing calling code! The `chatWithFilesStreaming()` method signature remains the same, but now uses HttpClient internally for proper authentication:

```typescript
// Existing code works without modification
this.aiAssistantService.chatWithFilesStreaming(message, sessionId, files, state)
  .subscribe({
    next: ({ data, complete }) => {
      // Handle streaming data
      if (complete) {
        console.log('Stream completed');
      }
    },
    error: (error) => {
      console.error('Streaming error:', error);
    }
  });
```

## Key Changes Made

### 1. Updated Imports
```typescript
import { HttpClient, HttpErrorResponse, HttpResponse, HttpEventType } from '@angular/common/http';
import { filter, switchMap } from 'rxjs/operators';
```

### 2. New Helper Methods
- `parseStreamingChunks()` - Parses HttpClient streaming chunks
- `parseCompleteStreamingResponse()` - Handles final response

### 3. Authentication Improvements
- **HttpClient**: Automatic interceptor application for proper IAP header handling
- **Consistent**: Same authentication mechanism as all other services

## Testing Checklist

### Development Environment
- [ ] Streaming works with dev cookies
- [ ] Console shows "HttpClient" method being used
- [ ] Authentication headers are present in network requests

### Production Environment (Cloud Run + IAP)
- [ ] Streaming works without authentication errors
- [ ] IAP headers are properly forwarded
- [ ] No 401/403 errors in browser network tab
- [ ] Real-time streaming performance is maintained

## Troubleshooting

### If Authentication Still Fails
1. Check that Angular interceptors are properly configured in `app.config.ts`
2. Verify IAP headers are present in browser network tab
3. Check Cloud Run service authentication settings
4. Ensure the auth interceptor is being applied to the request

### Performance Issues
1. Monitor streaming latency in browser console
2. Check for any HttpClient-specific streaming issues
3. Verify Server-Sent Events are properly parsed

## Migration Path

**Immediate**: The existing `chatWithFilesStreaming()` method now uses HttpClient automatically. No code changes required - the solution maintains full backward compatibility while fixing the authentication issue for production deployments.
