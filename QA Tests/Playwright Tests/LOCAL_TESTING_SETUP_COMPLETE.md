# Playwright Local Testing Setup - Complete Guide

## 🎉 What We Accomplished

Successfully configured Playwright tests to run against a local development environment with:
- ✅ PostgreSQL database running locally
- ✅ .NET backend API at `http://localhost:5159`
- ✅ Angular dev server at `http://127.0.0.1:4200`
- ✅ Test user with Administrator role created
- ✅ Authentication working (no more Access Denied!)

---

## 📋 Files Created

### 1. **`setup-test-user.sql`** - Creates Test User
Creates `test@playwright.local` with Administrator role for testing.

```sql
-- Run this once to create the test user:
-- C:\Program Files\PostgreSQL\16\bin\psql.exe -h localhost -p 5432 -U test -d TestDb -f setup-test-user.sql
```

**User Credentials:**
- **Email**: `test@playwright.local`
- **Password**: `TestPassword123!`
- **Role**: Administrator
- **IsInternal**: true

### 2. **`verify-users.sql`** - Verify User Setup
Query to check all users and their roles in the database.

---

## 🔧 Key Issues Discovered & Fixed

### Issue 1: Cookie Domain Mismatch
**Problem:** Backend `/dev-login` sets cookies for `localhost:5159`, but Angular runs on `127.0.0.1:4200`. Cookies don't transfer between domains.

**Solution:** Set cookies directly in Playwright for the `127.0.0.1` domain.

### Issue 2: Angular HTTP Parsing Error
**Problem:** When navigating to `/dev-login` from Angular, it tries to parse the HTML response as JSON, causing:
```
[CONSOLE ERROR]: [HTTP] HTTP 200: Error 200 {detail: Http failure during parsing for http://127.0.0.1:4200/dev-login...
```

**Solution:** Don't navigate to `/dev-login` from Angular; set cookies programmatically.

### Issue 3: User Had No Roles
**Problem:** Initial test user had no roles assigned, causing 403 Access Denied.

**Solution:** Created user via SQL with explicit Administrator role assignment.

---

## ✅ Working Authentication Pattern

```typescript
test.beforeEach(async ({ page }) => {
  const testUserEmail = 'test@playwright.local';
  
  // Step 1: Clear cookies/storage
  await page.context().clearCookies();
  
  // Step 2: Set authentication cookies for Angular domain
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
  
  // Step 3: Navigate to Angular app
  await page.goto('http://127.0.0.1:4200/');
  await page.waitForLoadState('networkidle');
  
  // Step 4: Navigate to test page
  await page.goto('http://127.0.0.1:4200/#/partnerships/contacts');
});
```

---

## 🐛 Remaining Issue: Component Not Rendering

**Current Status:** Authentication works, but contact list component doesn't render.

**Evidence:**
- ✅ URL correct: `http://127.0.0.1:4200/#/partnerships/contacts`
- ✅ No Access Denied error
- ❌ Body is empty (no components render)
- ❌ All selectors return 0 count

**Possible Causes:**
1. **Permissions API not responding correctly**
2. **Angular routing issue**
3. **Component error (silent failure)**
4. **Data loading issue**

---

## 🔍 Debugging Steps Taken

1. **Verified user in database** ✓
   - User ID 1: `test@playwright.local` with Administrator role

2. **Captured console errors** ✓
   - Found HTTP parsing error from `/dev-login` navigation

3. **Checked page content** ✓
   - Body is completely empty despite correct URL

4. **Added debug logging** ✓
   - Confirmed no elements exist on page

---

## 📝 Next Steps to Complete Setup

### Option A: Fix Angular Component Rendering (Recommended)

**Action Items:**
1. Check Angular console for additional errors
2. Verify the contacts route is properly configured
3. Check if permissions API (`/api/contact/{id}/permissions`) is working
4. Test navigation manually in browser with same cookies

### Option B: Use Different Test Approach

**Alternative:** Test with mock data instead of real backend:
- Use API mocking (`helpers/api-mocks.helper.ts`)
- Mock permission responses
- Focus on UI behavior rather than integration

---

## 💡 Lessons Learned

1. **Cookie Domains Matter**: Cookies set for one domain don't work on another
2. **Backend vs Frontend URLs**: Must be consistent for authentication
3. **SQL is Reliable**: Direct database setup ensures correct user/role configuration
4. **Console Errors are Gold**: Always capture `page.on('console')` and `page.on('pageerror')`
5. **Empty Body = Component Error**: If URL is correct but body is empty, the Angular component is failing to render

---

## 🚀 Quick Start Command

```bash
# 1. Ensure backend is running
cd UNOPS.PAO.Server
dotnet run --launch-profile Local

# 2. Run a single test
cd "QA Tests/Playwright Tests"
npx playwright test contacts.spec.ts --project=chromium --workers=1 --grep "should display contacts page header"
```

---

## 📚 Related Documentation

- `setup-test-user.sql` - User creation script
- `verify-users.sql` - User verification query
- `REAL_BACKEND_TESTING_GUIDE.md` - Backend testing guidelines
- `MISSING_FUNCTIONS_FIX.md` - Test helper functions

---

**Last Updated:** 2026-01-30
**Status:** 🟡 In Progress - Authentication Working, Component Rendering Issue
