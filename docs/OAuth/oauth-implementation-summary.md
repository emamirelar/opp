# Google OAuth Implementation Summary

## Current State: ✅ Already Correctly Implemented

Good news! Your Google OAuth architecture is **already implemented at the application level** exactly as you requested. The system does not trigger OAuth at the component level.

## What Already Exists

### Application-Level OAuth Service

**Service**: `GoogleOAuthService` (`UNOPS.PAO.ClientApp/src/app/core/services/auth/google-oauth.service.ts`)

This service is already:
- ✅ A singleton (`providedIn: 'root'`) - instantiated once for the entire application
- ✅ Configured at app startup in `app.config.ts`
- ✅ Managing token lifecycle automatically
- ✅ Validating token expiration before use
- ✅ Only triggering OAuth when token is invalid or expired

### How It Works (As Currently Implemented)

```typescript
// 1. Application starts
// GoogleOAuthService is instantiated as singleton

// 2. Component needs ID token for API call
export class OpportunityStatementSectionComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);

  async exportToGoogleDoc(): Promise<void> {
    // 3. Request valid token - service handles everything
    const token = await this.googleOAuthService.getValidIdToken();
    
    // Behind the scenes, the service:
    // - Checks if token exists
    // - Checks if token is expired (55min buffer on 1hr token)
    // - Returns existing token if valid
    // - OR triggers OAuth popup if needed
    // - Stores new token for future use
    
    // 4. Use token for API call
    const response = await fetch('https://api.ai.unops.org/...', {
      headers: { Authorization: `Bearer ${token}` }
    });
  }
}
```

## Current Usage Verification

I've verified the codebase and found:

### ✅ Correct Usage
- **OpportunityStatementSectionComponent**: Uses `googleOAuthService.getValidIdToken()` for export to Google Docs
- **GoogleOAuthService**: Only place that directly accesses `SocialAuthService`
- **SocialAuthComponent**: Login page (appropriate direct use)

### ⚠️ No Issues Found
- No components are triggering OAuth directly
- No components are injecting `SocialAuthService` improperly
- All OAuth flows go through the centralized service

## What You Don't Need to Change

### 1. No Component-Level Changes Needed
Your components (like `OpportunityViewComponent`) are **not** triggering OAuth. They don't need to be modified because they're already using the pattern correctly.

### 2. GoogleOAuthService Already Validates Tokens
The service already has this logic built-in:

```typescript
public async getValidIdToken(forceRefresh: boolean = false): Promise<string> {
  // If token is valid and not forcing refresh, return it
  if (!forceRefresh && this.isTokenValid()) {
    const token = this.getCurrentIdToken();
    if (token) {
      console.log('🔑 Using existing Google OAuth token');
      return token;  // ✅ Reuse existing valid token
    }
  }

  // Token is invalid or expired, need to authenticate
  console.log('🔐 Triggering Google OAuth authentication...');
  
  // Trigger Google sign-in
  const user = await this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);
  // ... store and return new token
}
```

### 3. Token Expiration Already Managed
The service already checks expiration automatically:

```typescript
public isTokenValid(): boolean {
  const token = this.currentToken$.value;
  
  if (!token || !token.idToken) {
    return false;
  }
  
  // Check if token is expired
  const now = Date.now();
  if (now >= token.expiresAt) {
    console.log('⏰ Google OAuth token has expired');
    return false;
  }
  
  return true;
}
```

## Adding OAuth to New Components

When you need to add OAuth functionality to a new component, follow this pattern:

```typescript
export class YourNewComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);

  async performActionNeedingOAuth(): Promise<void> {
    try {
      // Get valid ID token - triggers OAuth if needed
      const idToken = await this.googleOAuthService.getValidIdToken();

      // Use token for your API call
      const response = await fetch('https://your-api-endpoint', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${idToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(yourData)
      });

      if (!response.ok) {
        // Handle 401/403 - token might have expired between check and use
        if (response.status === 401 || response.status === 403) {
          console.log('🔄 Token expired, refreshing and retrying...');
          await this.googleOAuthService.refreshToken();
          return this.performActionNeedingOAuth(); // Retry once
        }
        
        throw new Error(`API error: ${response.statusText}`);
      }

      const result = await response.json();
      
      // Success handling
      this.feedbackService.showSuccessToast({
        summary: this.translateService.instant('message.success'),
        detail: this.translateService.instant('message.actionSuccess')
      });

    } catch (authError) {
      // Handle authentication errors
      console.error('❌ OAuth authentication failed:', authError);
      
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.authenticationRequired')
      });
    }
  }
}
```

## Separate OAuth Flow: GoogleDriveService

Note: `GoogleDriveService` has a **separate** OAuth flow because it needs different permissions:

- **GoogleOAuthService**: Manages **ID tokens** for identity verification
- **GoogleDriveService**: Manages **access tokens** for Drive API operations

This separation is correct and intentional. They serve different purposes:

| Service | Token Type | Purpose | Scopes |
|---------|-----------|---------|--------|
| GoogleOAuthService | ID Token | Identity verification for API calls | Basic profile |
| GoogleDriveService | Access Token | Direct Drive API operations | `drive.file` |

## Summary

**What you asked for**: OAuth flow at application level, with components checking validity before use

**What you already have**: Exactly that! The `GoogleOAuthService` is already:
1. ✅ Instantiated at application level (singleton)
2. ✅ Managing token lifecycle centrally
3. ✅ Validating token expiration before use
4. ✅ Only triggering OAuth when necessary
5. ✅ Providing clean API for components to request valid tokens

**What you need to do**: Nothing! The architecture is already correct.

**Future components**: Just inject `GoogleOAuthService` and call `await getValidIdToken()` when you need an OAuth token.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│              Application Bootstrap                   │
│  (app.config.ts)                                    │
│                                                      │
│  - GoogleOAuthService (singleton)                   │
│  - SocialAuthService configured                     │
└─────────────────────────────────────────────────────┘
                       │
                       │ Token state managed here
                       │
┌─────────────────────────────────────────────────────┐
│          GoogleOAuthService                          │
│  (Core service - application level)                 │
│                                                      │
│  - Token storage (in-memory)                        │
│  - Expiration tracking                              │
│  - Validation logic                                 │
│  - OAuth trigger (only when needed)                 │
└─────────────────────────────────────────────────────┘
                       ▲
                       │
                       │ inject() and call
                       │ getValidIdToken()
                       │
┌─────────────────────────────────────────────────────┐
│          Components                                  │
│  (Feature level)                                    │
│                                                      │
│  - OpportunityStatementSectionComponent ✅          │
│  - YourNewComponent (future) ✅                     │
│  - AnotherComponent (future) ✅                     │
│                                                      │
│  All components use the same service instance       │
│  No component-level OAuth triggering                │
└─────────────────────────────────────────────────────┘
```

## Verification

To verify the implementation is working:

1. **Clear browser storage** (simulate fresh user)
2. **Navigate to opportunity statement section**
3. **Click "Export to Google Doc"**
4. **Observe**:
   - OAuth popup appears (first time)
   - User authenticates
   - Export completes
5. **Click "Export to Google Doc" again**
6. **Observe**:
   - NO OAuth popup (token reused)
   - Export completes immediately
7. **Check console logs**:
   - Should see: `🔑 Using existing Google OAuth token`

## Conclusion

Your OAuth architecture is already correctly implemented at the application level. The `GoogleOAuthService` manages everything centrally, and components simply request valid tokens when needed. No changes are required to move OAuth from component-level to app-level—it's already there! 🎉

