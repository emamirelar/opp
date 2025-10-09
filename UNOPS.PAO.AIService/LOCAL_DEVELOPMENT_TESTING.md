# Local Development Testing Guide

## Overview

This guide explains how to test the impersonation feature locally without needing to deploy to GCP.

## Prerequisites

1. Backend running locally (typically `https://localhost:44426`)
2. `appsettings.Local.json` configured with dev user
3. Test users exist in local database

## Configuration Check

Verify your `appsettings.Local.json` has these settings:

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
      "UserEmail": "tushard@unops.org",
      "SkipValidationInDevelopment": true
    }
  }
}
```

## How Local Dev Authentication Works

### Default Behavior (No Headers)

When you make a request **without** any special headers:

```http
GET https://localhost:44426/api/partner/search?query=World Bank
```

**What happens:**
1. ✅ You're authenticated as `tushard@unops.org` (from `Development:IAPSimulation:UserEmail`)
2. ✅ Permissions checked for `tushard@unops.org`
3. ✅ Request succeeds if `tushard@unops.org` has required permissions

**Logs you'll see:**
```
🔧 [DEV-MODE] Using configured development user: tushard@unops.org
```

### With Impersonation Header

When you want to test **as a different user**:

```http
POST https://localhost:44426/api/contact
Content-Type: application/json
x-unops-impersonated-user: anushas@unops.org

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.org",
  "partnerId": 987
}
```

**What happens:**
1. ✅ You're authenticated as `tushard@unops.org`
2. 🔧 Dev user is automatically trusted for impersonation
3. ✅ System switches to `anushas@unops.org`
4. ✅ Permissions checked for `anushas@unops.org`
5. ✅ Request succeeds if `anushas@unops.org` has required permissions

**Logs you'll see:**
```
🔧 [DEV-MODE] Using configured development user: tushard@unops.org
🔧 [DEV-MODE] Allowing impersonation for configured dev user: tushard@unops.org
🔄 [IMPERSONATION] tushard@unops.org requesting impersonation of anushas@unops.org
✅ [IMPERSONATION] Successfully impersonating anushas@unops.org (authenticated as tushard@unops.org)
🔐 [IMPERSONATION-AUDIT] Request authenticated as tushard@unops.org, acting as anushas@unops.org
```

## Testing Scenarios

### Scenario 1: Test as Default Dev User

**Goal:** Test with your default configured dev user's permissions

**Request:**
```http
GET https://localhost:44426/api/partner/1
```

**No special headers needed** - you'll automatically be `tushard@unops.org`

---

### Scenario 2: Test as Another User

**Goal:** Test with a different user's permissions (e.g., a user with fewer permissions)

**Request:**
```http
POST https://localhost:44426/api/contact
Content-Type: application/json
x-unops-impersonated-user: limiteduser@unops.org

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.org",
  "partnerId": 123
}
```

**Expected Result:**
- If `limiteduser@unops.org` has `CanCreate` permission on `Contact` → 200 OK ✅
- If `limiteduser@unops.org` lacks permission → 403 Forbidden ❌

---

### Scenario 3: Test AI Service Impersonation

**Goal:** Simulate what the AI service does in production

**Request:**
```http
POST https://localhost:44426/api/contact
Content-Type: application/json
x-unops-impersonated-user: tushard@unops.org

{
  "firstName": "AI",
  "lastName": "Test",
  "email": "aitest@example.org",
  "partnerId": 987
}
```

This simulates the AI service making a request on behalf of `tushard@unops.org`.

---

### Scenario 4: Test Permission Denial

**Goal:** Verify that permission checks work correctly

**Steps:**
1. Find a user in your local DB with **no** `CanCreate` permission for `Contact`
2. Make a request impersonating that user:

```http
POST https://localhost:44426/api/contact
Content-Type: application/json
x-unops-impersonated-user: readonly@unops.org

{
  "firstName": "Should",
  "lastName": "Fail",
  "email": "fail@example.org",
  "partnerId": 123
}
```

**Expected Result:** `403 Forbidden` ❌

**Expected Logs:**
```
✅ [IMPERSONATION] Successfully impersonating readonly@unops.org
❌ Permission check failed: readonly@unops.org does not have CanCreate on Contact
```

---

## Testing with Postman

### Setup Collection Variables

1. `base_url`: `https://localhost:44426`
2. `impersonated_user`: `anushas@unops.org`

### Request Template

```
POST {{base_url}}/api/contact
Headers:
  Content-Type: application/json
  x-unops-impersonated-user: {{impersonated_user}}

Body:
{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@example.org",
  "partnerId": 987
}
```

---

## Testing with cURL

### Without Impersonation
```bash
curl -k https://localhost:44426/api/partner/search?query=World
```

### With Impersonation
```bash
curl -k https://localhost:44426/api/contact \
  -H "Content-Type: application/json" \
  -H "x-unops-impersonated-user: anushas@unops.org" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.org",
    "partnerId": 987
  }'
```

---

## Testing with AI Service Locally

### 1. Start Backend
```bash
cd UNOPS.PAO.Server
dotnet run
```

### 2. Start AI Service
```bash
cd UNOPS.PAO.AIService
python main.py
```

### 3. Test via AI Chat

**User:** "Create a contact named John Doe with email john@worldbank.org for partner 987"

**What happens:**
1. AI service authenticates (in local mode, uses configured credentials)
2. AI service sends `x-unops-impersonated-user: tushard@unops.org` header
3. Backend processes as `tushard@unops.org`
4. Contact created if permissions allow

**Check Logs:**
- **AI Service logs:** Should show header being sent
- **Backend logs:** Should show impersonation messages

---

## Debugging Tips

### Problem: "Not authenticated"

**Cause:** Development IAP simulation not enabled

**Solution:** Check `appsettings.Local.json`:
```json
{
  "Development": {
    "IAPSimulation": {
      "Enabled": true,  // ← Must be true
      "UserEmail": "tushard@unops.org"  // ← Must be set
    }
  }
}
```

---

### Problem: "Impersonation denied"

**Cause:** Dev user not being trusted

**Check:** Look for this log:
```
🔧 [DEV-MODE] Allowing impersonation for configured dev user: tushard@unops.org
```

**If missing:** Make sure you're running in Development environment and the authenticated user matches `Development:IAPSimulation:UserEmail`

---

### Problem: "Impersonated user not found"

**Cause:** User doesn't exist in local database

**Solution:** 
1. Check your local database:
```sql
SELECT * FROM "AspNetUsers" WHERE "Email" = 'anushas@unops.org';
```

2. If missing, create the user or use a different user for testing

---

### Problem: 403 Forbidden despite impersonation

**Cause:** Impersonated user lacks required permissions

**Debug:**
1. Check user roles:
```sql
SELECT r."Name" 
FROM "AspNetRoles" r
JOIN "AspNetUserRoles" ur ON r."Id" = ur."RoleId"
JOIN "AspNetUsers" u ON u."Id" = ur."UserId"
WHERE u."Email" = 'anushas@unops.org';
```

2. Check entity permissions:
```sql
SELECT * FROM "EntityPermissions" 
WHERE "Entity" = 'Contact' 
  AND "Role" IN (/* roles from step 1 */);
```

---

## Viewing Logs

### Console Output

When running locally with `dotnet run`, logs appear in the console. Look for:
- 🔧 `[DEV-MODE]` markers
- 🔄 `[IMPERSONATION]` markers
- ✅ Success indicators
- 🚫 Denial messages

### Log Levels

To see debug-level impersonation logs, update `appsettings.Local.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "UNOPS.PAO.UNOPSIdentity": "Debug"
    }
  }
}
```

---

## Quick Reference

| Scenario | Header Needed? | Authenticated As | Permissions From |
|----------|----------------|------------------|------------------|
| Default dev request | ❌ No | `tushard@unops.org` | `tushard@unops.org` |
| Impersonate user | ✅ Yes | `tushard@unops.org` | Impersonated user |
| AI service simulation | ✅ Yes | Dev user or service account | Header user |

---

## Next Steps

Once tested locally:
1. ✅ Verify all scenarios work as expected
2. ✅ Commit changes
3. ✅ Deploy to Dev environment
4. ✅ Test in Dev with real AI service
5. ✅ Deploy to QA, Test, Production

---

**Need Help?** Check the full implementation guide at `IMPERSONATION_IMPLEMENTATION_GUIDE.md`

