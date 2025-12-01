# Google Drive OAuth - App-Level Initialization Implementation

## Problem Statement

Google Drive OAuth popups were appearing when navigating to the opportunity view component, even before the user invoked any export/Drive features. This created a poor user experience because:

1. Users saw OAuth prompts unexpectedly when just viewing opportunities
2. OAuth initialization was happening multiple times (once per component that needed Drive features)
3. No clear separation between app-level setup and component-level usage

## Root Cause

The `GoogleDriveService.initializeAuth()` method was being called in component `ngOnInit()` methods:

**Before (Component-Level Initialization)**:
```typescript
// OpportunityDocumentsComponent.ngOnInit()
this.googleDriveService.initializeAuth().subscribe({
  next: (authAvailable) => {
    this.googleDriveAuthAvailable = authAvailable;
  }
});
```

This caused:
- Initialization to happen when components loaded (on navigation)
- Multiple initialization attempts if multiple components loaded
- Potential race conditions and duplicate OAuth flows

## Solution Implemented

### 1. App-Level Initialization (`app.config.ts`)

Added Google Drive auth initialization to the application bootstrap phase using `provideAppInitializer()`:

```typescript
// UNOPS.PAO.ClientApp/src/app/app.config.ts

provideAppInitializer(() => {
  const googleDriveService = inject(GoogleDriveService);
  return async () => {
    try {
      const authAvailable = await firstValueFrom(googleDriveService.initializeAuth());
      if (authAvailable) {
        console.log('✅ [AppInit] Google Drive auth initialized successfully');
      } else {
        console.warn('⚠️ [AppInit] Google Drive auth initialization failed');
      }
    } catch (error) {
      console.error('❌ [AppInit] Error initializing Google Drive auth:', error);
      // Don't throw - allow app to continue even if Drive auth fails
    }
  };
}),
```

**Key Benefits**:
- ✅ Initialization happens **once** when app starts
- ✅ Happens **before** any components load
- ✅ **Silent initialization** - no popup unless user actually uses Drive features
- ✅ All components benefit from pre-initialized state

### 2. Prevent Re-Initialization (`GoogleDriveService`)

Added initialization state tracking to prevent multiple initialization attempts:

```typescript
// UNOPS.PAO.ClientApp/src/app/shared/services/google-drive.service.ts

// New state flags
private isInitialized = false;
private isInitializing = false;

private async initializeAuthAsync(): Promise<boolean> {
  // Return immediately if already initialized
  if (this.isInitialized) {
    console.log('✅ [GoogleDrive] Already initialized, skipping re-initialization');
    return true;
  }

  // If currently initializing, wait for completion
  if (this.isInitializing) {
    console.log('⏳ [GoogleDrive] Initialization already in progress, waiting...');
    // Wait for initialization to complete...
    return true;
  }

  // Mark as initializing
  this.isInitializing = true;

  try {
    // ... initialization logic ...
    
    // Mark as successfully initialized
    this.isInitialized = true;
    this.isInitializing = false;
    
    return true;
  } catch (error) {
    this.isInitializing = false;
    return false;
  }
}
```

**Key Benefits**:
- ✅ Multiple calls to `initializeAuth()` are safe - returns immediately if already done
- ✅ Concurrent calls wait for in-progress initialization
- ✅ Prevents duplicate API calls and race conditions

### 3. Added Status Check Method

Added a simple method for components to check if Drive auth is available:

```typescript
// UNOPS.PAO.ClientApp/src/app/shared/services/google-drive.service.ts

/**
 * @description Check if Google Drive authentication is available
 * @returns {boolean} True if Drive auth is initialized and ready
 */
public isAuthAvailable(): boolean {
  return this.isInitialized;
}
```

### 4. Simplified Component Usage

Updated components to simply check if auth is available rather than re-initializing:

**Before**:
```typescript
ngOnInit(): void {
  this.googleDriveService.initializeAuth().subscribe({
    next: (authAvailable) => {
      this.googleDriveAuthAvailable = authAvailable;
    }
  });
}
```

**After**:
```typescript
ngOnInit(): void {
  // Check if Google Drive auth is available (initialized at app level)
  this.googleDriveAuthAvailable = this.googleDriveService.isAuthAvailable();
  
  if (!this.googleDriveAuthAvailable) {
    console.warn('⚠️ Google Drive auth not available');
  }
}
```

**Key Benefits**:
- ✅ Synchronous check - no Observable complexity
- ✅ Immediate response - no waiting
- ✅ Clear separation: app initializes, components check status

## Files Modified

1. **`UNOPS.PAO.ClientApp/src/app/app.config.ts`**
   - Added `GoogleDriveService` import
   - Added `firstValueFrom` import from rxjs
   - Added `provideAppInitializer()` for Drive auth initialization

2. **`UNOPS.PAO.ClientApp/src/app/shared/services/google-drive.service.ts`**
   - Added `isInitialized` and `isInitializing` state flags
   - Updated `initializeAuthAsync()` to prevent re-initialization
   - Added `isAuthAvailable()` public method

3. **`UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/document/opportunity-documents.component.ts`**
   - Replaced `initializeAuth().subscribe()` with `isAuthAvailable()` check

4. **`UNOPS.PAO.ClientApp/src/app/features/partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component.ts`**
   - Replaced `initializeAuth().subscribe()` with `isAuthAvailable()` check

## Architecture Flow

### Before (Component-Level)
```
App Starts
    ↓
User navigates to Opportunity View
    ↓
OpportunityDocumentsComponent loads
    ↓
Component calls initializeAuth()
    ↓
Google APIs load (delays component rendering)
    ↓
OAuth popup appears (confusing to user)
    ↓
Component ready
```

### After (App-Level)
```
App Starts
    ↓
App initializer calls initializeAuth()
    ↓
Google APIs load (silent, in background)
    ↓
App ready (all components can use Drive features)
    ↓
User navigates to Opportunity View
    ↓
OpportunityDocumentsComponent loads
    ↓
Component checks isAuthAvailable() → true
    ↓
Component ready immediately (no delay, no popup)
    ↓
[Later] User clicks "Export to Drive"
    ↓
OAuth popup appears ONLY NOW (expected by user)
```

## Important Notes

### When Does OAuth Popup Appear?

**What Doesn't Trigger Popup** (Silent initialization):
- ✅ App startup
- ✅ Navigating to components
- ✅ Loading pages with Drive features
- ✅ Calling `initializeAuth()`
- ✅ Calling `isAuthAvailable()`

**What DOES Trigger Popup** (User action required):
- ⚠️ First time user exports a document to Drive
- ⚠️ First time user converts an Office file
- ⚠️ First time user uses any feature that calls `requestAccessToken()`

This is **correct behavior** - users should only see OAuth prompts when they actually use Drive features.

### Initialization vs Token Request

Understanding the difference is crucial:

| Method | Purpose | Triggers Popup? | When Called |
|--------|---------|----------------|-------------|
| `initializeAuth()` | Load Google APIs, prepare token client | ❌ No | App startup |
| `requestAccessToken()` | Request Drive API access token | ✅ Yes | When user uses Drive feature |
| `isAuthAvailable()` | Check if ready to request tokens | ❌ No | Anytime |

### Token Types

This implementation manages **Google Drive Access Tokens** (different from ID tokens):

| Token Type | Managed By | Purpose | Scopes |
|------------|------------|---------|--------|
| **ID Token** | `GoogleOAuthService` | Identity verification for API calls | Basic profile |
| **Access Token** | `GoogleDriveService` | Direct Drive API operations | `drive.file` |

Both are needed for different features:
- Export opportunity statement → Uses `GoogleOAuthService` (ID token)
- Convert Office file via Drive → Uses `GoogleDriveService` (Access token)

## Testing Checklist

Test the implementation with these steps:

### Test 1: App Initialization
- [ ] Clear browser storage
- [ ] Reload application
- [ ] Open browser console
- [ ] Verify log: `✅ [AppInit] Google Drive auth initialized successfully`
- [ ] Verify **NO** OAuth popup appears

### Test 2: Navigate to Opportunity View
- [ ] Navigate to an opportunity (with documents section)
- [ ] Verify page loads immediately
- [ ] Check console for: `✅ [OpportunityDocuments] Google Drive auth is available`
- [ ] Verify **NO** OAuth popup appears

### Test 3: Multiple Components
- [ ] Navigate between multiple opportunities
- [ ] Open create opportunity dialog
- [ ] Verify no duplicate initialization logs
- [ ] Verify **NO** OAuth popups during navigation

### Test 4: Actually Use Drive Features
- [ ] Click "Export" on an opportunity statement (ID token flow)
- [ ] Verify OAuth popup **DOES** appear (expected)
- [ ] Authenticate
- [ ] Upload an Office file (Access token flow)
- [ ] Verify conversion works (may trigger second OAuth if different scopes)

### Test 5: Re-initialization Safety
- [ ] Reload app
- [ ] Check console for single initialization message
- [ ] Navigate to components using Drive features
- [ ] Verify no duplicate initialization attempts

## Console Logging

You should see these log messages in the correct order:

**App Startup**:
```
✅ [AppInit] Google Drive auth initialized successfully
✅ [GoogleDrive] Initialization complete
```

**Component Load** (OpportunityDocumentsComponent):
```
✅ [OpportunityDocuments] Google Drive auth is available
```

**Multiple Components** (CreateOpportunityDialog):
```
✅ [CreateOpportunity] Google Drive auth is available for document conversion
```

**Re-initialization Attempt** (if any):
```
✅ [GoogleDrive] Already initialized, skipping re-initialization
```

**Concurrent Initialization** (if any):
```
⏳ [GoogleDrive] Initialization already in progress, waiting...
```

## Error Handling

If Drive initialization fails at app level:

```
❌ [AppInit] Error initializing Google Drive auth: [error details]
```

Components will see:
```
⚠️ [OpportunityDocuments] Google Drive auth not available - Office file conversion will not be possible
```

The app continues to function - Drive features are simply unavailable.

## Rollback Plan

If issues arise, you can rollback by:

1. **Remove app-level initializer** from `app.config.ts`:
   - Delete the `provideAppInitializer()` block for GoogleDriveService

2. **Restore component-level initialization**:
   - Revert `OpportunityDocumentsComponent.ngOnInit()`
   - Revert `CreateOpportunityFromInteractionsDialogComponent.ngOnInit()`
   - Change back from `isAuthAvailable()` to `initializeAuth().subscribe()`

3. **Keep the re-initialization safety** in `GoogleDriveService`:
   - The state tracking is harmless and prevents issues even with component-level init

## Future Enhancements

Potential improvements to consider:

1. **Token Persistence**:
   - Store Drive access token in localStorage
   - Auto-restore token on app reload
   - Skip OAuth popup for returning users

2. **Background Token Refresh**:
   - Refresh tokens before expiration
   - Prevent mid-session OAuth interruptions

3. **Unified OAuth Management**:
   - Consolidate ID token and access token management
   - Single OAuth flow for both if possible

4. **Progressive Enhancement**:
   - Load Google APIs lazily (only when needed)
   - Reduce initial app bundle size

---

**Implementation Date**: 2025-01-30  
**Status**: ✅ Complete  
**Tested**: Pending user verification

