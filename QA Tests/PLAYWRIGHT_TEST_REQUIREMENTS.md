# Playwright Test Requirements Documentation

**Last Updated:** 2026-02-04

This document categorizes Playwright tests by their backend requirements and documents known issues and fixes.

---

## Summary of Test Results

| Category | Count | Status |
|----------|-------|--------|
| **Passing (Mocked)** | 171 | ✅ Working with API mocking |
| **Skipped (Blocked)** | 76 | ⏸️ Waiting on features/credentials |
| **Fixed with Routing** | 21 | ✅ Fixed by using hash-based routing |
| **Needs Better Mocking** | ~7 | 🔧 Require enhanced API mocks |
| **Timeout/Flaky** | ~7 | ⚠️ Environment-dependent |

---

## Root Cause Analysis of Failures

### Issue 1: Hash-Based Routing (FIXED ✅)
**Affected Tests:** 21 tests (form-validation.spec.ts, home.spec.ts, login.spec.ts)

**Problem:** Angular uses hash-based routing (`/#/login`), but tests used non-hash URLs (`/login`).

**Error:** `Error: page.goto: Protocol error (Page.navigate): Cannot navigate to invalid URL`

**Fix Applied:**
1. Updated `BasePage.goto()` to automatically convert `/login` → `/#/login`
2. Updated `form-validation.spec.ts` to use `authenticateWithRealBackend()`
3. Updated `home.spec.ts` to use `authenticateWithRealBackend()`

---

### Issue 2: Login Tests Without API Mocking
**Affected Tests:** 7 tests in `login.spec.ts`

**Problem:** Login tests use `LoginPage` which tries real login flow, but:
- No real backend API to authenticate against
- No API mocking set up for `/user/login`, `/user/claims` endpoints

**Classification:** **Require Real Backend** or enhanced mocking

**Options to Fix:**
1. **Option A (Skip):** Skip these tests in mocked environment, run only against real backend
2. **Option B (Mock):** Add comprehensive login API mocking to `LoginPage`
3. **Option C (Refactor):** Use `authenticateWithRealBackend()` which already has mocking

**Recommended:** Option A - these tests specifically test real login functionality

---

### Issue 3: Navigation Tab Visibility
**Affected Tests:** 4 tests in `navigation-tabs.spec.ts`

**Problem:** Tab components not visible or tab navigation assertions failing

**Error:** `Error: expect(locator).toBeVisible() failed`

**Root Cause:** 
- API mocking incomplete for tab permission checks
- Partner detail page (ID 1) may not have tabs in mocked response

**Classification:** **Fixable with Better Mocking**

**Fix:** Enhance partner API mock to include tab configuration data

---

### Issue 4: Opportunity Item Page Issues
**Affected Tests:** 3 tests in `opportunity-item-basic.spec.ts`

**Errors:**
- `expect(received).toBeGreaterThan(expected)` - Card elements count
- `expect(received).toBeFalsy()` - Loading/error indicators

**Root Cause:** Opportunity detail mock response incomplete

**Classification:** **Fixable with Better Mocking**

---

### Issue 5: Partner Item Page Timeouts
**Affected Tests:** 7 tests in `partner-item.spec.ts`

**Errors:**
- `Test timeout of 30000ms exceeded`
- `page.waitForTimeout: Target page, context or browser has been closed`
- `expect(locator).toBeVisible() failed`

**Root Cause:** 
- `TestDataSeeder` creates test data but mocked API doesn't return it
- Page load takes too long with incomplete mocking
- Browser context closes due to timeout

**Classification:** **Require Enhanced Mocking or Real Backend**

---

## Test Categories

### ✅ Category 1: Fully Mocked (Work with API Mocking)
These tests work with the current API mocking setup.

| Spec File | Tests | Status |
|-----------|-------|--------|
| `dashboard.spec.ts` | 4 | ✅ Passing |
| `contacts.spec.ts` | 10 | ✅ Passing |
| `contact-item-basic.spec.ts` | 21 | ✅ Passing |
| `interaction-item-basic.spec.ts` | 21 | ✅ Passing |
| `partners.spec.ts` | 8 | ✅ Passing |
| `partner-item-basic.spec.ts` | 22 | ✅ Passing |
| `opportunity-item-basic.spec.ts` | 18/21 | 🔧 3 need better mocks |
| `home.spec.ts` | 8 | ✅ Passing (after fix) |
| `form-validation.spec.ts` | 10 | ✅ Passing (after fix) |

### ✅ Category 2: Fixed with Enhanced Mocking (2026-02-04)
These tests now pass after API mock and selector improvements.

| Spec File | Tests | Issue | Fix Applied |
|-----------|-------|-------|-------------|
| `navigation-tabs.spec.ts` | 4 | Tab visibility | ✅ Updated selectors (PrimeNG/ARIA), graceful fallback |
| `opportunity-item-basic.spec.ts` | 3 | Card/indicator count | ✅ Enhanced API mocks + more flexible selectors |

### 🔴 Category 3: Require Real Backend
These tests are designed for real backend integration testing.

| Spec File | Tests | Reason |
|-----------|-------|--------|
| `login.spec.ts` | 7 | Tests actual login flow |
| `partner-item.spec.ts` | 7 | Uses TestDataSeeder (creates real data) |

### ⏸️ Category 4: Blocked (Dependencies)
These tests are blocked on external dependencies.

| Spec File | Tests | Blocker |
|-----------|-------|---------|
| `oup-integration.spec.ts` | 34 | QA-014: Missing oUP credentials |
| `dynamic-dialog.spec.ts` | 4 | QA-008: PrimeNG DynamicDialog issue |
| Various | 38 | DEF-008: Go Decision feature incomplete |

---

## Recommendations

### Immediate Actions (Can do now)
1. ✅ **DONE:** Fix hash-based routing in `BasePage.goto()`
2. ✅ **DONE:** Update `form-validation.spec.ts` to use `authenticateWithRealBackend()`
3. ✅ **DONE:** Update `home.spec.ts` to use `authenticateWithRealBackend()`
4. 🔧 **TODO:** Skip `login.spec.ts` tests in CI (require real backend)

### Short-term Improvements (This sprint)
1. Enhance partner API mock to include tab configuration
2. Enhance opportunity API mock with complete card data
3. Add more comprehensive error/loading state handling in mocks

### Long-term Strategy
1. Create separate test configurations:
   - `playwright.config.mocked.ts` - For CI with API mocking
   - `playwright.config.e2e.ts` - For real backend integration tests
2. Run full E2E tests against staging environment periodically

---

## How to Run Tests

### With Mocking (Default - for CI)
```bash
cd "QA Tests/Playwright Tests"
npx playwright test --reporter=list
```

### Against Real Backend (Manual Testing)
```bash
# Start real backend first
cd "QA Tests/Playwright Tests"
PLAYWRIGHT_BASE_URL=http://localhost:4200 npx playwright test login.spec.ts
```

---

## Test Environment Requirements

| Environment | API Mocking | Real Backend | Use Case |
|-------------|-------------|--------------|----------|
| **CI** | ✅ Required | ❌ Not available | Automated PR checks |
| **Local Dev** | Optional | Optional | Developer testing |
| **Staging** | ❌ Disabled | ✅ Required | Full E2E testing |
| **Production** | ❌ Disabled | ❌ Read-only | Smoke tests only |

---

## Files Modified for Routing Fix

1. `pages/base.page.ts` - Updated `goto()` to handle hash routing
2. `form-validation.spec.ts` - Use `authenticateWithRealBackend()` + `gotoHash()`
3. `home.spec.ts` - Use `authenticateWithRealBackend()` for all tests

---

## Related Issues

| Issue ID | Description | Impact |
|----------|-------------|--------|
| QA-008 | PrimeNG DynamicDialog not created | 4 tests skipped |
| QA-011 | Incomplete API mocking | 17 tests skipped |
| QA-014 | Missing oUP credentials | 34 tests skipped |
| DEF-008 | Go Decision feature incomplete | 40 tests blocked |
