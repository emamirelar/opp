# Testing Against Real Backend - Guide for QA-007 and QA-008

**Purpose**: This guide explains how to test dialog functionality against a real backend to resolve QA-007 and QA-008.

**Date Created**: 2026-01-30  
**Related Issues**: QA-007 (Scanner), QA-008 (New Contact Dialog)  
**Status**: Ready for execution

---

## 🎯 Why Test Against Real Backend?

**Problem**: Dialog tests fail in Playwright with full API mocking:
- ✅ All API mocks work correctly
- ✅ Buttons are clickable
- ❌ Dialogs don't render (PrimeNG/Playwright interaction issue)

**Solution**: Test against real backend to:
1. Confirm dialogs work in production environment
2. See diagnostic console logs with real Angular services
3. Determine if this is a Playwright limitation or fixable issue

---

## 🔧 Prerequisites

### 1. Backend Server Running
- **Option A**: Local development server
  - Run: `dotnet run --project UNOPS.PAO.API`
  - URL: `https://localhost:7001` (or configured port)
  
- **Option B**: Integration/Staging environment
  - URL: Ask DevOps team for integration server URL
  - Requires VPN/network access

### 2. Database Seeded
- Ensure test data exists:
  - At least 1 user account for login
  - Sample contacts, partners (optional but helpful)
  
### 3. Update Playwright Configuration
- Temporarily disable API mocking
- Point tests to real backend URL

---

## 📝 Step-by-Step Instructions

### Step 1: Configure Backend URL

**File**: `playwright.config.ts`

```typescript
export default defineConfig({
  // ... existing config
  
  use: {
    // Point to real backend instead of localhost:4200
    baseURL: 'https://localhost:7001',  // ← Change this
    
    // Keep other settings
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  
  // ... rest of config
});
```

### Step 2: Disable API Mocking in Tests

**File**: `contacts.spec.ts`

Comment out the API mock setup in `test.beforeEach`:

```typescript
test.beforeEach(async ({ page }) => {
  contactsPage = new ContactsPage(page);
  
  // ❌ TEMPORARILY DISABLED FOR REAL BACKEND TESTING
  // await setupCameraMocks(page);
  // await loginAndNavigate(page, '/#/partnerships/contacts');
  
  // ✅ USE REAL LOGIN INSTEAD
  await page.goto('/login');
  
  // Login with real credentials
  await page.fill('[data-testid="username"]', 'testuser@example.com');
  await page.fill('[data-testid="password"]', 'TestPassword123');
  await page.click('[data-testid="login-button"]');
  
  // Wait for redirect to home
  await page.waitForURL(/\/#\//);
  
  // Navigate to contacts
  await page.goto('/#/partnerships/contacts');
  await contactsPage.waitForPermissions();
});
```

### Step 3: Run Specific Dialog Tests

Run ONLY the two dialog tests:

```bash
# Test both dialogs
npx playwright test contacts.spec.ts --grep "dialog" --project=chromium

# OR test individually:
npx playwright test contacts.spec.ts --grep "New Contact button" --project=chromium
npx playwright test contacts.spec.ts --grep "business card scanner" --project=chromium
```

### Step 4: Check Console Output

Look for the diagnostic logs we added:

**For Scanner (QA-007):**
```
[ContactList] openBusinessCardScanner() called
[ContactList] Permission check - canCreate: true
[ContactList] Setting showBusinessCardScanner signal to true
[ContactList] Signal set - current value: true
```

**For New Contact Dialog (QA-008):**
```
[ContactList] openContactEditDialog() called
[ContactList] DialogService available: true
[ContactList] Opening dialog with ContactEditDialogComponent
[ContactList] Dialog ref: [object Object]
[ContactList] Dialog ref type: object
[ContactList] Dialog opened, subscription created
```

### Step 5: Observe Browser Behavior

With real backend:
- ✅ **Expected**: Dialogs should appear and function normally
- ❌ **If still failing**: Playwright has fundamental incompatibility with PrimeNG

**Take screenshots/video** if issues persist!

---

## 🔍 What to Look For

### ✅ Success Indicators:
1. **Scanner test**:
   - Console shows: `[ContactList] Signal set - current value: true`
   - `<app-business-card-scanner>` appears in DOM
   - Video feed visible (or camera permission prompt)
   
2. **New Contact test**:
   - Console shows: `[ContactList] Dialog ref: [object Object]`
   - `.p-dynamic-dialog` appears in DOM (count > 0)
   - Form fields visible in dialog

### ❌ Failure Indicators:
1. **Scanner still fails**:
   - Signal is set but component doesn't render
   - **Conclusion**: CSS/z-index issue or Angular change detection problem
   
2. **Dialog still fails**:
   - DialogService returns null or undefined
   - **Conclusion**: PrimeNG incompatibility with Playwright
   
3. **Permission check fails**:
   - Console shows: `canCreate: false`
   - **Conclusion**: Real backend permission system different from mocks

---

## 📊 Expected Outcomes

### Scenario A: Tests Pass ✅
**Meaning**: Dialogs work with real backend, fail with mocks  
**Action**: 
- Update mocking strategy to better simulate real environment
- OR accept that dialog tests require integration environment
- Remove `.failing()` markers
- Update QA-007, QA-008 to "Resolved - requires real backend"

### Scenario B: Tests Still Fail ❌
**Meaning**: Fundamental Playwright/PrimeNG incompatibility  
**Action**:
- Document findings in QA-007, QA-008
- Consider alternatives:
  - Replace DynamicDialog with static `<p-dialog>`
  - Test dialogs via Cypress instead of Playwright
  - Accept limitation and test dialogs manually

### Scenario C: Permission Issues 🔐
**Meaning**: Real permission system blocks dialog opening  
**Action**:
- Create test user with proper permissions
- Update backend seed data
- OR update test to use admin credentials

---

## 🔄 Reverting Back to Mocked Tests

After testing, **REVERT CHANGES**:

1. Restore `playwright.config.ts`:
   ```typescript
   baseURL: 'http://127.0.0.1:4200',
   ```

2. Uncomment API mocking in `contacts.spec.ts`:
   ```typescript
   await setupCameraMocks(page);
   await loginAndNavigate(page, '/#/partnerships/contacts');
   ```

3. Commit any findings/updates to documentation

---

## 📝 Document Results

After testing, update these files:

### 1. QA Defect List (`QA Tests/Defect List for QA.md`)

**If tests pass:**
```markdown
| QA-007 | Business Card Scanner signal not set | **RESOLVED** - Works with real backend. Issue specific to API mocking environment. | 2026-01-30 | Resolved | QA Team |
| QA-008 | PrimeNG DynamicDialog not created | **RESOLVED** - Works with real backend. Issue specific to API mocking environment. | 2026-01-30 | Resolved | QA Team |
```

**If tests still fail:**
```markdown
| QA-007 | Business Card Scanner signal not set | **CONFIRMED** - Fails even with real backend. Fundamental Playwright/PrimeNG incompatibility. Recommend Cypress or manual testing. | 2026-01-30 | Open - Requires Alternative | QA Team |
| QA-008 | PrimeNG DynamicDialog not created | **CONFIRMED** - Fails even with real backend. Consider refactoring to static <p-dialog> for testability. | 2026-01-30 | Open - Requires Refactor | QA Team |
```

### 2. Test File (`contacts.spec.ts`)

Remove `.failing()` if tests pass:
```typescript
// Was: test.failing('should open business card scanner dialog'
// Now: test('should open business card scanner dialog'
```

### 3. Create Summary Document

Create: `QA Tests/Playwright Tests/REAL_BACKEND_TEST_RESULTS.md`

```markdown
# Real Backend Testing Results

**Date**: [Your test date]
**Tester**: [Your name]
**Backend**: [Local/Integration/Staging]
**Backend Version**: [Version number]

## Test Results

### QA-007: Business Card Scanner
- **Result**: [PASS/FAIL]
- **Console Output**: [Paste relevant logs]
- **Observations**: [What you saw]
- **Screenshots**: [Link if available]

### QA-008: New Contact Dialog
- **Result**: [PASS/FAIL]
- **Console Output**: [Paste relevant logs]
- **Observations**: [What you saw]
- **Screenshots**: [Link if available]

## Conclusions
[Your analysis of what the results mean]

## Recommendations
[What should be done next]
```

---

## 🚨 Common Issues

### Issue 1: CORS Errors
**Symptom**: Console shows `CORS policy: No 'Access-Control-Allow-Origin' header`  
**Fix**: Update backend CORS policy to allow `http://127.0.0.1:4200` origin

### Issue 2: SSL Certificate Errors
**Symptom**: `ERR_CERT_AUTHORITY_INVALID`  
**Fix**: Add to playwright.config.ts:
```typescript
use: {
  ignoreHTTPSErrors: true,
}
```

### Issue 3: Authentication Cookies Not Working
**Symptom**: Immediately redirected to login after successful login  
**Fix**: Check if backend uses httpOnly cookies and session storage

### Issue 4: Camera Permissions
**Symptom**: Scanner opens but camera permission denied  
**Fix**: Playwright configuration:
```typescript
use: {
  permissions: ['camera'],
}
```

---

## 📞 Support

**Questions about**:
- Backend setup → Contact DevOps team
- Test failures → Review QA-007, QA-008 defects
- Playwright configuration → Check official docs: https://playwright.dev

---

## ✅ Checklist

Before starting:
- [ ] Backend server is running and accessible
- [ ] Test user credentials available
- [ ] Database has seed data
- [ ] `playwright.config.ts` updated with backend URL
- [ ] API mocking disabled in test file
- [ ] Browser dev tools open to see console logs

During testing:
- [ ] Run scanner test and observe behavior
- [ ] Run dialog test and observe behavior
- [ ] Capture console output
- [ ] Take screenshots if issues occur
- [ ] Note any error messages

After testing:
- [ ] Revert playwright.config.ts changes
- [ ] Revert contacts.spec.ts changes
- [ ] Update QA defect list with results
- [ ] Create summary document
- [ ] Commit findings to repository

---

**Good luck with testing!** 🚀

The diagnostic logs will show exactly where the process breaks down, giving us definitive answers about whether this is fixable or a Playwright limitation.
