# Google OAuth Quick Reference Guide

## For Developers: How to Use Google OAuth in Your Components

### TL;DR

✅ **DO THIS**: Inject `GoogleOAuthService` and call `getValidIdToken()`  
❌ **DON'T DO THIS**: Inject `SocialAuthService` or trigger OAuth manually

---

## Basic Usage Pattern

```typescript
import { inject } from '@angular/core';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { TranslateService } from '@ngx-translate/core';

export class YourComponent {
  // Inject the centralized OAuth service
  private readonly googleOAuthService = inject(GoogleOAuthService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);

  async yourMethodNeedingOAuth(): Promise<void> {
    try {
      // ✅ Get valid token - handles everything automatically
      const idToken = await this.googleOAuthService.getValidIdToken();

      // Use token in your API call
      const response = await fetch('https://api.example.com/endpoint', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${idToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(yourData)
      });

      if (!response.ok) {
        // Handle auth errors - refresh and retry once
        if (response.status === 401 || response.status === 403) {
          await this.googleOAuthService.refreshToken();
          return this.yourMethodNeedingOAuth(); // Retry
        }
        throw new Error('API call failed');
      }

      // Success!
      this.feedbackService.showSuccessToast({
        summary: this.translateService.instant('message.success'),
        detail: 'Operation completed successfully'
      });

    } catch (error) {
      // Handle OAuth failure
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: 'Authentication failed. Please try again.'
      });
    }
  }
}
```

---

## What Happens Behind the Scenes

When you call `getValidIdToken()`:

1. **Service checks existing token**:
   - ✅ Valid & not expired → Returns immediately
   - ❌ Invalid or expired → Continues to step 2

2. **Service triggers OAuth popup**:
   - User sees Google sign-in popup
   - User authenticates
   - Google returns ID token

3. **Service stores token**:
   - Stores in memory (BehaviorSubject)
   - Tracks expiration time
   - Returns token to your component

4. **Future calls reuse token**:
   - No popup shown if token still valid
   - Automatic refresh on expiration

---

## Common Scenarios

### Scenario 1: Export Document to Google Drive

```typescript
export class DocumentExportComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);
  
  async exportToGoogleDoc(): Promise<void> {
    const token = await this.googleOAuthService.getValidIdToken();
    
    await fetch('https://api.ai.unops.org/v1/convert/markdown-to-google-doc', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: formData
    });
  }
}
```

### Scenario 2: Call Protected API Endpoint

```typescript
export class ProtectedApiComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);
  
  async callProtectedEndpoint(): Promise<void> {
    const token = await this.googleOAuthService.getValidIdToken();
    
    await fetch('/api/protected-resource', {
      headers: { 
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
  }
}
```

### Scenario 3: Force Token Refresh

```typescript
export class TokenRefreshComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);
  
  async forceRefresh(): Promise<void> {
    // Force new token even if current one is valid
    const freshToken = await this.googleOAuthService.refreshToken();
    // or
    const freshToken2 = await this.googleOAuthService.getValidIdToken(true);
  }
}
```

---

## API Reference

### GoogleOAuthService Methods

| Method | Return Type | Description |
|--------|------------|-------------|
| `getValidIdToken(forceRefresh?: boolean)` | `Promise<string>` | **PRIMARY METHOD** - Get valid token, trigger OAuth if needed |
| `isTokenValid()` | `boolean` | Check if current token is valid |
| `getCurrentIdToken()` | `string \| null` | Get current token without validation |
| `refreshToken()` | `Promise<string>` | Force refresh (same as `getValidIdToken(true)`) |
| `signOut()` | `Promise<void>` | Sign out and clear token |
| `getCurrentUser()` | `SocialUser \| null` | Get user info |

### GoogleOAuthService Observables

| Observable | Type | Description |
|------------|------|-------------|
| `token$` | `Observable<GoogleOAuthToken \| null>` | Stream of token state changes |

---

## Error Handling

### Handle OAuth Popup Blocked

```typescript
try {
  const token = await this.googleOAuthService.getValidIdToken();
} catch (error) {
  if (error.message?.includes('popup')) {
    this.feedbackService.showWarningToast({
      summary: 'Popup Blocked',
      detail: 'Please allow popups for this site and try again.'
    });
  }
}
```

### Handle API Authentication Errors

```typescript
const response = await fetch(apiUrl, {
  headers: { Authorization: `Bearer ${token}` }
});

if (response.status === 401 || response.status === 403) {
  // Token expired between validation and use - refresh and retry
  await this.googleOAuthService.refreshToken();
  return this.yourMethod(); // Retry once
}
```

### Handle User Cancellation

```typescript
try {
  const token = await this.googleOAuthService.getValidIdToken();
} catch (error) {
  // User closed popup or denied access
  this.feedbackService.showInfoToast({
    summary: 'Authentication Required',
    detail: 'You must sign in with Google to use this feature.'
  });
}
```

---

## Console Logging

The service provides helpful console logs for debugging:

| Message | Meaning |
|---------|---------|
| 🔑 Using existing Google OAuth token | Token valid, reusing |
| 🔐 Triggering Google OAuth authentication... | Showing OAuth popup |
| ✅ Google OAuth authentication successful | User authenticated successfully |
| ⏰ Google OAuth token has expired | Token expired, need refresh |
| 🔄 Signed out for token refresh | Forcing fresh authentication |
| 👋 Signed out from Google OAuth | User signed out |

---

## Testing Your Implementation

### Manual Test Steps

1. **Clear browser storage** (DevTools → Application → Clear storage)
2. **Trigger your OAuth feature** (e.g., click export button)
3. **Verify OAuth popup appears**
4. **Authenticate with Google**
5. **Verify feature completes successfully**
6. **Trigger feature again immediately**
7. **Verify NO popup appears** (token reused)
8. **Check console logs** for token reuse message

### Unit Test Example

```typescript
import { TestBed } from '@angular/core/testing';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';

describe('YourComponent', () => {
  let googleOAuthService: jasmine.SpyObj<GoogleOAuthService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('GoogleOAuthService', ['getValidIdToken']);
    
    TestBed.configureTestingModule({
      providers: [
        { provide: GoogleOAuthService, useValue: spy }
      ]
    });

    googleOAuthService = TestBed.inject(GoogleOAuthService) as jasmine.SpyObj<GoogleOAuthService>;
  });

  it('should request OAuth token when exporting', async () => {
    const mockToken = 'mock-id-token';
    googleOAuthService.getValidIdToken.and.returnValue(Promise.resolve(mockToken));

    await component.yourMethodNeedingOAuth();

    expect(googleOAuthService.getValidIdToken).toHaveBeenCalled();
  });
});
```

---

## Common Mistakes to Avoid

### ❌ DON'T: Inject SocialAuthService Directly

```typescript
// ❌ WRONG
export class MyComponent {
  private readonly socialAuthService = inject(SocialAuthService);
  
  async doSomething() {
    await this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }
}
```

### ✅ DO: Use GoogleOAuthService

```typescript
// ✅ CORRECT
export class MyComponent {
  private readonly googleOAuthService = inject(GoogleOAuthService);
  
  async doSomething() {
    const token = await this.googleOAuthService.getValidIdToken();
  }
}
```

### ❌ DON'T: Store Token Manually

```typescript
// ❌ WRONG
export class MyComponent {
  private myToken: string = '';
  
  async doSomething() {
    if (!this.myToken) {
      this.myToken = await this.googleOAuthService.getValidIdToken();
    }
    // Use this.myToken
  }
}
```

### ✅ DO: Let Service Manage Token

```typescript
// ✅ CORRECT
export class MyComponent {
  async doSomething() {
    // Service handles caching automatically
    const token = await this.googleOAuthService.getValidIdToken();
  }
}
```

### ❌ DON'T: Forget Error Handling

```typescript
// ❌ WRONG
async doSomething() {
  const token = await this.googleOAuthService.getValidIdToken();
  await fetch(apiUrl, { headers: { Authorization: `Bearer ${token}` } });
}
```

### ✅ DO: Handle Errors Gracefully

```typescript
// ✅ CORRECT
async doSomething() {
  try {
    const token = await this.googleOAuthService.getValidIdToken();
    const response = await fetch(apiUrl, { 
      headers: { Authorization: `Bearer ${token}` } 
    });
    
    if (!response.ok) {
      if (response.status === 401) {
        await this.googleOAuthService.refreshToken();
        return this.doSomething(); // Retry
      }
      throw new Error('API call failed');
    }
  } catch (error) {
    this.feedbackService.showErrorToast({
      detail: 'Operation failed. Please try again.'
    });
  }
}
```

---

## When to Use vs GoogleDriveService

| Feature | Use GoogleOAuthService | Use GoogleDriveService |
|---------|----------------------|----------------------|
| Export to Google Doc | ✅ | ❌ |
| Call protected API | ✅ | ❌ |
| Identity verification | ✅ | ❌ |
| Convert file via Drive API | ❌ | ✅ |
| Upload to Drive | ❌ | ✅ |
| Direct Drive operations | ❌ | ✅ |

**Key Difference**:
- **GoogleOAuthService**: ID tokens for identity/API auth
- **GoogleDriveService**: Access tokens for Drive API operations

---

## Need Help?

### Debug Checklist

- [ ] Injected `GoogleOAuthService` (not `SocialAuthService`)
- [ ] Called `getValidIdToken()` (not manual OAuth trigger)
- [ ] Wrapped in try-catch for error handling
- [ ] Checked console logs for token status
- [ ] Verified Google Client ID configured in `app.config.ts`
- [ ] Tested with cleared browser storage (fresh state)
- [ ] Verified API endpoint expects Google ID token

### Where to Look

- **Service implementation**: `UNOPS.PAO.ClientApp/src/app/core/services/auth/google-oauth.service.ts`
- **Configuration**: `UNOPS.PAO.ClientApp/src/app/app.config.ts`
- **Example usage**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/sections/statement/opportunity-statement-section.component.ts`
- **Architecture docs**: `docs/oauth-architecture.md`

---

**Last Updated**: 2025-01-30  
**Version**: 1.0.0

