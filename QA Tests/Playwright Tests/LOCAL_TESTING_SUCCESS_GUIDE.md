# 🎉 Playwright Local Testing - SUCCESS GUIDE

## ✅ **Status: ALL 13 CONTACT TESTS PASSING!**

**Test Results:**
```
13 passed (4.0m)
Exit code: 0
```

**Test Run Date:** 2026-01-30

---

## 🔧 **What Was Fixed**

### **1. Proxy Configuration** (`UNOPS.PAO.ClientApp/src/proxy.conf.js`)

**Problems Fixed:**
- ❌ Proxy target pointed to `https://localhost:7123` (wrong backend)
- ❌ `/dev-login` endpoint not forwarded to backend
- ❌ Angular dev server returned 404 for API calls

**Solutions Applied:**
```javascript
const target = "http://localhost:5159";  // ✅ Changed to local backend

const PROXY_CONFIG = [
  {
    context: ["/user/", "/api/**", "/dev-login"],  // ✅ Added /dev-login
    target: target,
    secure: false,
    changeOrigin: true,
  },
];
```

---

### **2. Test User Setup** (`setup-test-user.sql`)

**Problems Fixed:**
- ❌ Test users had no roles assigned (403 Access Denied)
- ❌ Users created via API got insufficient permissions

**Solutions Applied:**
Created dedicated test user via SQL with explicit Administrator role:

**Test User Credentials:**
- **Email**: `test@playwright.local`
- **Password**: `TestPassword123!`
- **Role**: Administrator
- **IsInternal**: true

**How to Create:**
```powershell
$env:PGPASSWORD='test'; & 'C:\Program Files\PostgreSQL\16\bin\psql.exe' `
  -h localhost -p 5432 -U test -d TestDb -f setup-test-user.sql
```

---

### **3. Test Authentication Flow** (`contacts.spec.ts`)

**Problems Fixed:**
- ❌ Tests navigated to `/login` (wrong page for dev mode)
- ❌ Cookies set for wrong domain (`localhost:5159` vs `127.0.0.1:4200`)
- ❌ Cookies set AFTER navigation (too late)
- ❌ Waited for `networkidle` (timeouts due to background requests)
- ❌ HTTP parsing errors for `/dev-login` endpoint

**Solutions Applied:**

```typescript
test.beforeEach(async ({ page }) => {
  const testUserEmail = 'test@playwright.local';
  
  // 1. Clear cookies first
  await page.context().clearCookies();
  
  // 2. Set authentication cookies BEFORE navigation (critical!)
  await page.context().addCookies([
    {
      name: 'dev-user-email',
      value: testUserEmail,
      domain: '127.0.0.1',  // Must match Angular dev server domain
      path: '/',
      httpOnly: false,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'DevIAPAuth',
      value: testUserEmail,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    }
  ]);
  
  // 3. Navigate directly to target page
  await page.goto('http://127.0.0.1:4200/#/partnerships/contacts');
  
  // 4. Wait for page load (not networkidle)
  await page.waitForLoadState('load', { timeout: 15000 });
  
  // 5. Give Angular time to initialize routing
  await page.waitForTimeout(2000);
  
  // 6. Wait for permissions
  await contactsPage.waitForPermissions();
});
```

---

## 🎯 **Key Insights**

### **Critical Success Factors:**

1. **Set Cookies BEFORE Navigation** ⭐
   - Playwright requires cookies to be set before the first `page.goto()`
   - Setting cookies after navigation doesn't work!

2. **Domain Must Match Exactly** ⭐
   - Cookies for `localhost` don't work on `127.0.0.1`
   - Use `127.0.0.1` to match Angular dev server

3. **Direct Navigation Works Best** ⭐
   - Navigate directly to test page (not home first)
   - Avoids unnecessary redirects and auth checks

4. **Use 'load' Not 'networkidle'** ⭐
   - Some background requests (AI, Google APIs) never complete
   - `'load'` is sufficient for testing

5. **Proxy Must Point to Correct Backend** ⭐
   - Default proxy pointed to production backend
   - Local testing needs local backend URL

---

## 📊 **Test Execution Flow**

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Clear Cookies                                            │
│    └─> Clean slate for each test                           │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Set Authentication Cookies                               │
│    - dev-user-email: test@playwright.local                  │
│    - DevIAPAuth: test@playwright.local                      │
│    - Domain: 127.0.0.1                                      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Navigate to Test Page                                    │
│    └─> http://127.0.0.1:4200/#/partnerships/contacts       │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Angular Detects Cookies                                  │
│    - AuthGuard checks dev-user-email cookie                 │
│    - Returns true (authenticated)                           │
│    - No API calls needed                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Component Loads                                          │
│    - ContactListComponent renders                           │
│    - Permissions API called: /api/contact/permissions       │
│    - Data API called: /api/contact (via proxy → backend)    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Test Executes                                            │
│    └─> All assertions pass! ✅                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 **Running Tests**

### **Run All Contact Tests:**
```powershell
cd "c:\Users\Leonardc\git\opportunityplus"
npx playwright test contacts.spec.ts --project=chromium
```

### **Run Single Test:**
```powershell
npx playwright test contacts.spec.ts --project=chromium --grep "should display contacts page header"
```

### **Run with UI (See Browser):**
```powershell
npx playwright test contacts.spec.ts --project=chromium --headed
```

### **Debug Mode:**
```powershell
npx playwright test contacts.spec.ts --project=chromium --debug
```

---

## 🔄 **Prerequisites**

Before running tests, ensure:

1. **PostgreSQL is running**
   - Port: 5432
   - Database: TestDb
   - User: test/test

2. **Backend API is running**
   - Command: `cd UNOPS.PAO.Server && dotnet run --launch-profile Local`
   - URL: `http://localhost:5159`
   - Environment: Development

3. **Test user exists in database**
   - Run `setup-test-user.sql` once
   - Verify with `verify-users.sql`

4. **Node dependencies installed**
   - `cd UNOPS.PAO.ClientApp && npm install`
   - Playwright browsers: `npx playwright install chromium`

---

## ⚠️ **Expected Warnings (Safe to Ignore)**

The following errors appear but don't affect tests:

1. **Google API initialization error: _.Vc**
   - Cause: Google API credentials not configured for local dev
   - Impact: None - tests don't use Google APIs
   - Status: Expected

2. **HTTP 500: /api/ai-assistant/get-user-sessions**
   - Cause: AI service not configured locally (pgvector disabled)
   - Impact: None - contact tests don't use AI features
   - Status: Expected

3. **HTTP 404: Various resources**
   - Cause: Some features disabled in local dev mode
   - Impact: None - tests only focus on contacts
   - Status: Expected

4. **Warning: 'NO_COLOR' env is ignored**
   - Cause: Node.js terminal color handling
   - Impact: None - cosmetic only
   - Status: Safe to ignore

---

## 📈 **Test Suite Summary**

### **Contacts Test Suite** (`contacts.spec.ts`)

**Total Tests:** 13
**Status:** ✅ All Passing
**Execution Time:** ~4 minutes (single worker)

**Test Coverage:**
- ✅ Page header display
- ✅ New Contact button (with permission check)
- ✅ Business Card Scanner button (with permission check)
- ✅ Export button (with permission check)
- ✅ Import button (with permission check)
- ✅ Contact listview component
- ✅ Filter panel visibility
- ✅ Table column headers
- ✅ Contact row display (when data exists)
- ✅ Contact row click navigation
- ✅ New contact dialog (permission-based)
- ✅ Edit contact functionality
- ✅ Business card scanner dialog

---

## 🎓 **Lessons Learned**

### **Authentication in Playwright**

**Don't:**
- ❌ Navigate to login pages during tests
- ❌ Fill forms with credentials
- ❌ Set cookies after navigation
- ❌ Use different domains for cookies vs app
- ❌ Rely on networkidle for page load

**Do:**
- ✅ Set cookies before any navigation
- ✅ Use exact domain match (127.0.0.1)
- ✅ Navigate directly to test pages
- ✅ Use 'load' wait state
- ✅ Add small timeout for Angular initialization

### **Proxy Configuration**

**Critical:** The Angular dev server proxy MUST point to the correct backend:
- Local dev: `http://localhost:5159`
- QA environment: Use environment-specific URL
- Production: Different configuration entirely

### **Test User Management**

**Best Practice:** Create test users via SQL for:
- ✅ Reliable role assignment
- ✅ Explicit permission control
- ✅ No API dependencies
- ✅ Fast test execution

---

## 📝 **Quick Reference Checklist**

Before running Playwright tests:

- [ ] PostgreSQL database running on port 5432
- [ ] Backend API running at `http://localhost:5159`
- [ ] Test user created (`setup-test-user.sql`)
- [ ] Proxy config points to `http://localhost:5159`
- [ ] Node modules installed in `UNOPS.PAO.ClientApp`
- [ ] Playwright browsers installed (`npx playwright install`)

---

## 🎊 **Success Metrics**

- **Test Reliability**: 100% (13/13 passing)
- **Execution Time**: 4 minutes for full suite
- **Authentication**: Cookie-based (no login forms)
- **Proxy Integration**: Working correctly
- **Database Integration**: Verified with real data

---

**Congratulations on getting Playwright tests working with your local development environment!** 🚀

---

## 📚 **Related Files**

- `setup-test-user.sql` - Test user creation script
- `verify-users.sql` - User verification query
- `UNOPS.PAO.ClientApp/src/proxy.conf.js` - Proxy configuration
- `UNOPS.PAO.Server/appsettings.Development.json` - Backend config
- `contacts.spec.ts` - Contact test suite
- `MISSING_FUNCTIONS_FIX.md` - Helper function fixes
- `REAL_BACKEND_TESTING_GUIDE.md` - Backend testing guide

---

**Last Updated:** 2026-01-30  
**Status:** ✅ **COMPLETE AND WORKING**
