# User Impersonation Implementation Guide

## Overview

This document describes the implementation of the user impersonation feature for the IAP Authentication system. This allows trusted service accounts (like the AI service) to impersonate users and inherit their permissions for API requests.

## Problem Statement

The AI service (`pno-ai-service@unops-opportunityplus-*.iam.gserviceaccount.com`) was authenticating successfully but failing authorization checks because:

1. The service account authenticated with its own identity
2. The `x-unops-impersonated-user` header was being sent by the AI service but **not processed** by the backend
3. Permission checks used the service account's permissions (not the impersonated user's permissions)

**Result**: GET requests worked (read permissions) but POST requests failed with 403 (no create permissions).

## Solution Implemented

We implemented proper impersonation support in the IAP authentication handler, allowing trusted service accounts to assume the identity and permissions of other users.

---

## Files Modified

### 1. `UNOPS.PAO.UNOPSIdentity/Authentication/IAPAuthenticationHandler.cs`

#### Changes Made:

**A. Extended `IAPAuthenticationOptions` class:**
```csharp
public bool EnableImpersonation { get; set; } = false;
public List<string> TrustedServiceAccounts { get; set; } = new();
public string ImpersonationHeaderName { get; set; } = "x-unops-impersonated-user";
```

**B. Added impersonation logic in `HandleAuthenticateAsync()` method:**

```csharp
// Handle user impersonation if enabled and requested
PAOIdentityUser effectiveUser = user;
string authenticatedUserEmail = user.Email;
bool isImpersonating = false;

if (Options.EnableImpersonation && 
    Request.Headers.TryGetValue(Options.ImpersonationHeaderName, out var impersonatedEmailValues))
{
    var impersonatedEmail = impersonatedEmailValues.ToString()?.Trim();
    
    if (!string.IsNullOrEmpty(impersonatedEmail) && 
        impersonatedEmail != user.Email &&
        Options.TrustedServiceAccounts?.Any(sa => sa.Equals(user.Email, StringComparison.OrdinalIgnoreCase)) == true)
    {
        // Look up the impersonated user
        var impersonatedUser = await _userManager.FindByEmailAsync(impersonatedEmail);
        
        if (impersonatedUser != null)
        {
            effectiveUser = impersonatedUser;
            isImpersonating = true;
        }
    }
}
```

**C. Added audit claims for impersonation tracking:**
```csharp
if (isImpersonating)
{
    identity.AddClaim(new Claim("IsImpersonating", "true"));
    identity.AddClaim(new Claim("AuthenticatedServiceAccount", authenticatedUserEmail));
    identity.AddClaim(new Claim("ImpersonatedUser", effectiveUser.Email));
}
```

### 2. `UNOPS.PAO.Server/Startup.cs`

Added configuration loading for impersonation settings:

```csharp
// Configure user impersonation settings
options.EnableImpersonation = iapConfig.GetValue<bool>("EnableImpersonation", false);
options.ImpersonationHeaderName = iapConfig.GetValue<string>("ImpersonationHeaderName", "x-unops-impersonated-user");

// Load trusted service accounts list
options.TrustedServiceAccounts = new List<string>();
var trustedAccounts = iapConfig.GetSection("TrustedServiceAccounts");
if (trustedAccounts.Exists())
{
    foreach (var child in trustedAccounts.GetChildren())
    {
        var account = child.Value;
        if (!string.IsNullOrEmpty(account))
        {
            options.TrustedServiceAccounts.Add(account);
        }
    }
}

// Also add DefaultServiceAccount if specified
var defaultServiceAccount = iapConfig.GetValue<string>("DefaultServiceAccount", "");
if (!string.IsNullOrEmpty(defaultServiceAccount) && 
    !options.TrustedServiceAccounts.Contains(defaultServiceAccount))
{
    options.TrustedServiceAccounts.Add(defaultServiceAccount);
}
```

### 3. Configuration Files (All Environments)

Added to `appsettings.Dev.json`, `appsettings.QA.json`, `appsettings.Test.json`, and `appsettings.Production.json`:

```json
{
  "IAP": {
    // ... existing settings ...
    "EnableImpersonation": true,
    "ImpersonationHeaderName": "x-unops-impersonated-user",
    "TrustedServiceAccounts": [
      "pno-ai-service@unops-opportunityplus-[ENV].iam.gserviceaccount.com"
    ]
  }
}
```

---

## How It Works

### Authentication Flow with Impersonation

1. **AI Service makes request:**
   ```http
   POST /api/contact
   Authorization: Bearer eyJ...  (service account token)
   x-unops-impersonated-user: tushard@unops.org
   ```

2. **IAP authenticates the service account:**
   - Validates the JWT token
   - Identifies the authenticated user as `pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com`

3. **Impersonation check:**
   - Checks if `EnableImpersonation` is `true`
   - Checks if `x-unops-impersonated-user` header is present
   - Verifies the authenticated user is in `TrustedServiceAccounts` list
   - If all checks pass, looks up the impersonated user in the database

4. **Load effective user identity:**
   - Loads the impersonated user's roles and permissions from the database
   - Creates identity claims using the impersonated user's information
   - Adds audit claims to track the original service account

5. **Authorization proceeds with impersonated user's permissions:**
   - Permission checks use the impersonated user's roles
   - Row-level filters apply to the impersonated user
   - All business logic sees the impersonated user

### Security Features

1. **Whitelist-based trust:** Only service accounts in `TrustedServiceAccounts` can impersonate
2. **Audit trail:** All impersonation is logged with both service account and impersonated user
3. **Claims tracking:** Special claims added for audit and debugging:
   - `IsImpersonating`: "true"
   - `AuthenticatedServiceAccount`: Original service account email
   - `ImpersonatedUser`: Impersonated user email

---

## Configuration

### Local Development Configuration

For **local development** (`appsettings.Local.json`):

```json
{
  "IAP": {
    "EnableImpersonation": true,
    "ImpersonationHeaderName": "x-unops-impersonated-user",
    "TrustedServiceAccounts": [
      "pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com"
    ]
  },
  "Development": {
    "IAPSimulation": {
      "Enabled": true,
      "UserEmail": "tushard@unops.org",  // ← Authenticated user in dev mode
      "SkipValidationInDevelopment": true
    }
  }
}
```

**Key Features for Local Development:**

1. **Auto-Authentication**: When running locally, you're automatically authenticated as the `UserEmail` specified in `Development:IAPSimulation:UserEmail`
2. **Dev User Trusted**: The configured dev user is automatically added to the trusted list in development mode
3. **No IAP Headers Required**: No need for real IAP tokens or headers when testing locally
4. **Full Impersonation Support**: Send `x-unops-impersonated-user` header to test impersonation locally

**Dev Mode Authentication Flow:**
```
1. No IAP headers → Check DevIAPAuth cookie
2. No cookie → Use Development:IAPSimulation:UserEmail
3. If x-unops-impersonated-user header present → Allow impersonation (dev user is auto-trusted)
4. Load permissions from impersonated user
```

### Enabling Impersonation (Production/QA/Test)

In your `appsettings.[Environment].json`:

```json
{
  "IAP": {
    "EnableImpersonation": true,
    "ImpersonationHeaderName": "x-unops-impersonated-user",
    "TrustedServiceAccounts": [
      "pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com",
      "other-trusted-service@project.iam.gserviceaccount.com"
    ]
  }
}
```

### Configuration Options

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableImpersonation` | boolean | `false` | Master switch for impersonation feature |
| `ImpersonationHeaderName` | string | `"x-unops-impersonated-user"` | HTTP header name to check for impersonated user email |
| `TrustedServiceAccounts` | string[] | `[]` | List of service account emails allowed to impersonate |

---

## Testing

### Test Case 1: Successful Impersonation

**Request:**
```http
POST https://opportunityplus.dev.unops.org/api/contact
Content-Type: application/json
Authorization: Bearer [SERVICE_ACCOUNT_TOKEN]
x-unops-impersonated-user: tushard@unops.org

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.org",
  "partnerId": 987
}
```

**Expected Behavior:**
- ✅ Service account authenticates successfully
- ✅ Impersonation header is processed
- ✅ `tushard@unops.org` user is loaded from database
- ✅ Permission check uses `tushard@unops.org`'s roles
- ✅ Request succeeds if `tushard@unops.org` has `CanCreate` permission on `Contact` entity

**Expected Logs:**
```
🔄 [IMPERSONATION] Service account pno-ai-service@...dev.iam.gserviceaccount.com requesting impersonation of tushard@unops.org
✅ [IMPERSONATION] Successfully impersonating tushard@unops.org (authenticated as pno-ai-service@...dev.iam.gserviceaccount.com)
🔐 [IMPERSONATION-AUDIT] Request authenticated as pno-ai-service@...dev.iam.gserviceaccount.com, acting as tushard@unops.org
```

### Test Case 2: Impersonation by Non-Trusted Account

**Request:**
```http
POST /api/contact
Authorization: Bearer [REGULAR_USER_TOKEN]
x-unops-impersonated-user: admin@unops.org
```

**Expected Behavior:**
- ❌ Impersonation is **denied** (not in trusted service accounts list)
- ✅ Request proceeds with the authenticated user's own permissions

**Expected Logs:**
```
🚫 [IMPERSONATION] User regularuser@unops.org is not in trusted service accounts list. Impersonation denied.
```

### Test Case 3: Impersonating Non-Existent User

**Request:**
```http
POST /api/contact
Authorization: Bearer [SERVICE_ACCOUNT_TOKEN]
x-unops-impersonated-user: nonexistent@unops.org
```

**Expected Behavior:**
- ⚠️ Impersonated user not found
- ✅ Request proceeds with service account's own permissions

**Expected Logs:**
```
⚠️ [IMPERSONATION] Impersonated user not found: nonexistent@unops.org. Proceeding with service account.
```

### Test Case 4: Impersonation Disabled

**Configuration:**
```json
{
  "IAP": {
    "EnableImpersonation": false
  }
}
```

**Expected Behavior:**
- ✅ Impersonation header is **ignored**
- ✅ Request always uses authenticated user's permissions

---

## Debugging

### Checking Impersonation Claims

You can inspect the user's claims in any controller or service:

```csharp
public class MyController : BaseController
{
    public async Task<IActionResult> MyAction()
    {
        var user = HttpContext.User;
        
        // Check if request is impersonated
        var isImpersonating = user.HasClaim("IsImpersonating", "true");
        
        if (isImpersonating)
        {
            var serviceAccount = user.FindFirst("AuthenticatedServiceAccount")?.Value;
            var impersonatedUser = user.FindFirst("ImpersonatedUser")?.Value;
            
            _logger.LogInformation("Request by {ServiceAccount} impersonating {ImpersonatedUser}",
                serviceAccount, impersonatedUser);
        }
        
        // Current user email (impersonated if applicable)
        var currentUserEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        
        // ... rest of your code
    }
}
```

### Log Monitoring

Search for impersonation-related logs:

```bash
# In GCP Cloud Logs
resource.type="cloud_run_revision"
"[IMPERSONATION]"
```

**Log Markers:**
- `🔄 [IMPERSONATION]` - Impersonation requested
- `✅ [IMPERSONATION]` - Impersonation successful
- `⚠️ [IMPERSONATION]` - Impersonation warning (user not found)
- `🚫 [IMPERSONATION]` - Impersonation denied (untrusted account)
- `🔐 [IMPERSONATION-AUDIT]` - Audit log for successful impersonation

---

## Security Considerations

### 1. Trusted Service Accounts
- Only add service accounts that require impersonation capabilities
- Use dedicated service accounts for different purposes
- Regularly audit the `TrustedServiceAccounts` list

### 2. Audit Logging
- All impersonation requests are logged with both service account and target user
- Use these logs for security auditing and compliance
- Monitor for unusual impersonation patterns

### 3. Disabling Impersonation
- Set `EnableImpersonation: false` in production if not needed
- Impersonation can be enabled/disabled per environment

### 4. Header Validation
- The impersonation header is only checked if the feature is enabled
- Non-trusted accounts are denied even if they send the header
- Empty or malformed headers are safely ignored

---

## Troubleshooting

### Problem: 403 Forbidden despite impersonation header

**Possible Causes:**
1. `EnableImpersonation` is `false` in configuration
2. Service account not in `TrustedServiceAccounts` list
3. Impersonated user doesn't exist in database
4. Impersonated user lacks required permissions

**Solution:**
- Check configuration: `EnableImpersonation: true`
- Verify service account is in `TrustedServiceAccounts`
- Check logs for impersonation warnings
- Verify impersonated user has appropriate roles in database

### Problem: No impersonation logs appearing

**Possible Causes:**
1. Impersonation header not being sent
2. Header name mismatch
3. Feature disabled

**Solution:**
- Verify AI service is sending `x-unops-impersonated-user` header
- Check configuration: `ImpersonationHeaderName` matches header being sent
- Confirm `EnableImpersonation: true`

### Problem: Service account impersonating itself

**Expected Behavior:**
- If `impersonatedEmail == authenticatedEmail`, impersonation is skipped
- This is normal and not an error

---

## Integration with AI Service

The AI service in `UNOPS.PAO.AIService` already sends the impersonation header via the `build_request_headers()` function in `auth_helpers.py`:

```python
# Add impersonation header for ALL APIs when user email is available
if user_email:
    request_headers['x-unops-impersonated-user'] = user_email
```

**No changes needed** to the AI service - it will automatically work once the backend is deployed with this implementation.

---

## Deployment Checklist

- [x] Code changes implemented in `IAPAuthenticationHandler.cs`
- [x] Configuration loading added to `Startup.cs`
- [x] Configuration files updated for all environments:
  - [x] Dev
  - [x] QA
  - [x] Test
  - [x] Production
- [ ] Deploy backend to target environment
- [ ] Verify configuration is loaded (check startup logs)
- [ ] Test with AI service
- [ ] Monitor logs for impersonation activity
- [ ] Verify permissions are correctly applied

---

## Next Steps

1. **Deploy the changes** to the Dev environment first
2. **Test with the AI service** - try creating a contact via the AI agent
3. **Monitor logs** for impersonation-related messages
4. **Verify permissions** - ensure impersonated user's permissions are respected
5. **Deploy to other environments** once validated in Dev

---

## Summary

This implementation provides a secure, auditable way for trusted service accounts to impersonate users. The AI service can now:

- ✅ Authenticate as `pno-ai-service@...`
- ✅ Send `x-unops-impersonated-user` header with target user email
- ✅ Have requests processed with the impersonated user's permissions
- ✅ Create, update, and delete resources based on user permissions
- ✅ Maintain full audit trail of impersonation

All while maintaining security through:
- Whitelist-based trust model
- Comprehensive logging
- Feature toggle control
- Per-environment configuration

