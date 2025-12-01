# Google Drive OAuth Changes - Summary

## What Changed

I've moved Google Drive OAuth initialization from **component-level** to **application-level** to eliminate unexpected OAuth popups when navigating to pages.

## Quick Summary

✅ **Before**: OAuth initialization happened when components loaded → Users saw popups when viewing opportunities  
✅ **After**: OAuth initialization happens at app startup → No popups until users actually use Drive features

## Key Changes

### 1. Application Bootstrap (`app.config.ts`)
Added Drive auth initialization to app startup:
```typescript
provideAppInitializer(() => {
  const googleDriveService = inject(GoogleDriveService);
  return async () => {
    await firstValueFrom(googleDriveService.initializeAuth());
  };
})
```

### 2. GoogleDriveService (`google-drive.service.ts`)
- Added `isInitialized` and `isInitializing` flags
- Prevents duplicate initialization attempts
- Added `isAuthAvailable()` method for components

### 3. Components (Documents & Dialogs)
Changed from:
```typescript
this.googleDriveService.initializeAuth().subscribe(...)
```

To:
```typescript
this.googleDriveAuthAvailable = this.googleDriveService.isAuthAvailable();
```

## User Experience

### Before
```
User navigates to opportunity view
    ↓
Component loads and calls initializeAuth()
    ↓
OAuth popup appears unexpectedly ❌
```

### After
```
App starts → Drive auth initializes silently in background
    ↓
User navigates to opportunity view → Page loads instantly ✅
    ↓
User clicks "Export to Drive" → OAuth popup appears (expected) ✅
```

## Testing

To verify it works:

1. **Clear browser storage** (fresh start)
2. **Reload the app** - Check console, should see:
   ```
   ✅ [AppInit] Google Drive auth initialized successfully
   ```
3. **Navigate to opportunity view** - Should load **without** OAuth popup
4. **Actually use export feature** - OAuth popup **should** appear (this is correct!)

## What This Fixes

- ❌ No more unexpected OAuth popups when browsing opportunities
- ❌ No more multiple initialization attempts
- ❌ No more component loading delays
- ✅ OAuth popup only appears when user actually needs Drive access
- ✅ Faster page loads (initialization already done)
- ✅ Better user experience

## Files Modified

1. `UNOPS.PAO.ClientApp/src/app/app.config.ts`
2. `UNOPS.PAO.ClientApp/src/app/shared/services/google-drive.service.ts`
3. `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/document/opportunity-documents.component.ts`
4. `UNOPS.PAO.ClientApp/src/app/features/partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component.ts`

## Important Notes

### When Will Users See OAuth Popup?

**NOT when**:
- App loads
- Navigating between pages
- Viewing opportunities
- Opening documents section

**ONLY when**:
- First time exporting document to Drive
- First time converting Office file
- Actually using any Drive feature

This is **correct and expected behavior** - users should only authenticate when they need Drive access.

### Two Different OAuth Flows

Remember there are TWO separate OAuth systems:

1. **ID Tokens** (`GoogleOAuthService`) - For API authentication
   - Used by: Export opportunity statement
   - Managed at: Application level (already was)

2. **Access Tokens** (`GoogleDriveService`) - For Drive API
   - Used by: Office file conversion, Drive uploads
   - Managed at: Application level (NOW changed)

This change only affects the second one (Drive access tokens).

## Need to Roll Back?

If you encounter issues, you can revert by:

1. Remove the `provideAppInitializer()` block from `app.config.ts`
2. Restore component-level `initializeAuth().subscribe()` calls
3. Keep the safety features in `GoogleDriveService` (they're harmless)

## Questions?

Check the full documentation in `docs/oauth-app-level-initialization.md`

---

**Status**: ✅ Ready to test  
**Next Step**: Clear browser storage and test the app

