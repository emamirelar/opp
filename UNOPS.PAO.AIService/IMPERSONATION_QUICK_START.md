# User Impersonation - Quick Start Guide

## What Was Implemented

The backend now supports **user impersonation** - allowing the AI service to act on behalf of users with their permissions.

## The Problem (Before)

```
AI Service → POST /api/contact
└─ Authenticated as: pno-ai-service@...
└─ Permissions checked for: pno-ai-service@...
└─ Result: 403 Forbidden ❌ (service account had no create permissions)
```

## The Solution (After)

```
AI Service → POST /api/contact
           + Header: x-unops-impersonated-user: tushard@unops.org
└─ Authenticated as: pno-ai-service@...
└─ Impersonating: tushard@unops.org
└─ Permissions checked for: tushard@unops.org ✅
└─ Result: 200 OK ✅ (user has create permissions)
```

## What Changed

### Backend Files Modified:
1. **`IAPAuthenticationHandler.cs`** - Added impersonation logic
2. **`Startup.cs`** - Added configuration loading
3. **`appsettings.*.json`** - Added impersonation settings (all environments)

### Key Configuration Added:
```json
{
  "IAP": {
    "EnableImpersonation": true,
    "ImpersonationHeaderName": "x-unops-impersonated-user",
    "TrustedServiceAccounts": [
      "pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com"
    ]
  }
}
```

## Local Development Mode

For **local development**, no deployment needed! The system automatically:

✅ Authenticates as the configured dev user from `appsettings.Local.json`:
```json
{
  "Development": {
    "IAPSimulation": {
      "Enabled": true,
      "UserEmail": "tushard@unops.org"  // ← Your dev user
    }
  }
}
```

✅ Allows the configured dev user to impersonate others:
```http
POST http://localhost:44426/api/contact
x-unops-impersonated-user: anushas@unops.org
```

✅ The dev user (`tushard@unops.org`) is automatically trusted for impersonation in development mode

**Expected Dev Logs:**
```
🔧 [DEV-MODE] Using configured development user: tushard@unops.org
🔄 [IMPERSONATION] tushard@unops.org requesting impersonation of anushas@unops.org
✅ [IMPERSONATION] Successfully impersonating anushas@unops.org
```

## How to Deploy & Test (Production/QA/Test Environments)

### 1. Deploy Backend
```bash
# Build and deploy the updated backend
cd UNOPS.PAO.Server
dotnet publish -c Release

# Or use your existing deployment pipeline
```

### 2. Verify Configuration
After deployment, check the startup logs for:
```
✅ IAP Authentication configured with impersonation enabled
✅ Trusted service accounts: pno-ai-service@...
```

### 3. Test with AI Service
The AI service **already sends** the `x-unops-impersonated-user` header, so no AI service changes needed!

Just try the same action that was failing before:
```
User: "Create a contact named John Doe with email john@worldbank.org for partner 987"
```

### 4. Check Logs
Look for these log entries in GCP Cloud Logs:
```
🔄 [IMPERSONATION] Service account pno-ai-service@... requesting impersonation of tushard@unops.org
✅ [IMPERSONATION] Successfully impersonating tushard@unops.org
🔐 [IMPERSONATION-AUDIT] Request authenticated as pno-ai-service@..., acting as tushard@unops.org
```

## Security Notes

✅ **Secure**: Only whitelisted service accounts can impersonate  
✅ **Audited**: All impersonation is logged with full details  
✅ **Controlled**: Can be disabled per environment  
✅ **Transparent**: Special claims track original authenticated user  

## Troubleshooting

### Still getting 403?

**Check 1: Is impersonation enabled?**
```json
"EnableImpersonation": true  // Must be true
```

**Check 2: Is service account trusted?**
```json
"TrustedServiceAccounts": [
  "pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com"  // Must match
]
```

**Check 3: Does impersonated user exist?**
```sql
-- Check in database
SELECT * FROM "AspNetUsers" WHERE "Email" = 'tushard@unops.org';
```

**Check 4: Does user have permissions?**
```sql
-- Check user roles
SELECT r."Name" 
FROM "AspNetRoles" r
JOIN "AspNetUserRoles" ur ON r."Id" = ur."RoleId"
JOIN "AspNetUsers" u ON u."Id" = ur."UserId"
WHERE u."Email" = 'tushard@unops.org';

-- Check entity permissions for those roles
SELECT * FROM "EntityPermissions" 
WHERE "Entity" = 'Contact' 
  AND "Role" IN (/* roles from above */);
```

## Documentation

See `IMPERSONATION_IMPLEMENTATION_GUIDE.md` for:
- Detailed technical documentation
- Complete code walkthrough
- Security considerations
- Advanced troubleshooting

## Next Steps

1. ✅ Code implemented
2. ✅ Configuration added
3. ⏳ Deploy to Dev environment
4. ⏳ Test with AI service
5. ⏳ Deploy to QA, Test, Production

---

**Questions?** Check the full implementation guide or reach out to the team!

