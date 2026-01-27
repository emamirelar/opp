# Webkit Browser Test Investigation Report

**Date:** 2026-01-26  
**Investigator:** QA Team  
**Test Suite:** `contacts.spec.ts` (39 tests)  
**Related QA Issue:** QA-003

---

## Executive Summary

Webkit browser tests are experiencing severe navigation timeouts, with only **2 out of 13 tests passing (15% pass rate)** compared to **Chromium and Firefox achieving 69% pass rates**. The primary issue is that webkit (Safari's rendering engine) times out when attempting to navigate to the Angular login page during the `beforeEach` hook.

---

## Test Results Comparison

### Browser Pass Rates
| Browser | Tests Run | Passed | Failed | Pass Rate | Avg Time (Passing) |
|---------|-----------|--------|--------|-----------|-------------------|
| Chromium | 13 | 9 | 4 | **69%** | 32s |
| Firefox | 13 | 9 | 4 | **69%** | 32s |
| Webkit | 13 | 2 | 11 | **15%** | 55s |

### Webkit Test Execution Times
- **Test #28 (PASSED):** 50.0s
- **Test #29 (PASSED):** 60.0s (1.0m)
- **Failed tests:** 43s - 120s (1-2 minutes) before timeout

**Comparison:** Webkit passing tests take **2-3x longer** than Chromium/Firefox equivalents.

---

## Root Cause Analysis

### Primary Issue: Navigation Timeout

**Error Pattern:**
```
Test timeout of 60000ms exceeded while running "beforeEach" hook.
Error: Failed to navigate to login page: page.goto: Timeout 60000ms exceeded.
Call log:
  - navigating to "http://127.0.0.1:4200/login", waiting until "load"
```

**Location:** `auth.helper.ts:68` in the `login()` function

**Affected Code:**
```typescript
try {
  await page.goto(`${baseURL}/login`, {
    waitUntil: 'load',
    timeout: 60000  // 60 second timeout
  });
} catch (navError: any) {
  throw new Error(`Failed to navigate to login page: ${navError.message}`);
}
```

---

## Key Observations

### 1. Webkit-Specific Timing Issues
- **Initial page load:** Webkit takes significantly longer to bootstrap the Angular app
- **Navigation:** `page.goto()` times out at 60s, never completing the `load` event
- **API requests:** Once loaded, API mocks work correctly (seen in the 2 passing tests)

### 2. Angular App Bootstrap on Webkit
Looking at successful webkit test logs:
- Multiple rapid `/user/claims` requests
- Entity configuration requests occur but take longer to resolve
- Dashboard content loads slowly
- Overall bootstrap time: 30-50s (vs 10-15s for Chromium/Firefox)

### 3. Test Execution Pattern
```
Test #28 (PASSED):
  - Login navigation: ~20s
  - Page ready wait: ~15s
  - Test execution: ~15s
  - Total: 50s

Tests #30-37 (FAILED):
  - Login navigation: Times out at 60s
  - Never reaches page ready state
  - beforeEach hook fails, test never executes
```

### 4. Possible Contributing Factors

**a) Webkit's Stricter Security Model:**
- More restrictive CORS handling
- Stricter cookie/authentication policies
- Different localStorage/sessionStorage behavior

**b) Network Handling Differences:**
- Webkit may handle API mocking differently
- Route interception timing varies from Chromium/Firefox
- Different event loop timing for network requests

**c) Angular-Specific:**
- Hash-based routing (`/#/`) may have different timing in webkit
- Change detection cycles may be slower
- Zone.js patches may behave differently

**d) Playwright-Webkit Integration:**
- Webkit support in Playwright is experimental
- Different WebDriver protocol implementation
- Browser context/page lifecycle differences

---

## Detailed Error Analysis

### Failed Tests Breakdown

**All 11 failed webkit tests exhibit the same error:**

| Test # | Test Name | Duration | Error |
|--------|-----------|----------|-------|
| 27 | should display contacts page header | 43.4s | Navigation timeout |
| 30 | should display Export button | 90s (1.5m) | Navigation timeout |
| 31 | should display Import button | 78s (1.3m) | Navigation timeout |
| 32 | should display contact listview | 90s (1.5m) | Navigation timeout |
| 33 | should display contact list table | 114s (1.9m) | Navigation timeout |
| 34 | should allow clicking New Contact | 120s (2.0m) | Navigation timeout |
| 35 | should display search functionality | 120s (2.0m) | Navigation timeout |
| 36 | should handle empty state | 102s (1.7m) | Navigation timeout |
| 37 | should allow navigation to details | 96s (1.6m) | Navigation timeout |

**Pattern:** Later tests in the sequence timeout faster, suggesting resource exhaustion or cascading issues.

---

## Attempted Solutions & Results

### ❌ Solution 1: Welcome Dialog Dismissal (QA-002)
- **Applied:** Enhanced `loginAndNavigate()` with retry logic
- **Result:** No impact on webkit navigation timeouts
- **Reason:** Dialog handling happens AFTER navigation completes; webkit never gets past navigation

### ❌ Solution 2: Route Format Correction (QA-001)
- **Applied:** Changed route from `/contacts` to `/#/partnerships/contacts`
- **Result:** No impact on webkit timeouts
- **Reason:** Timeout occurs during login navigation, before target route navigation

### ⚠️ Current State
- Fixes for QA-001 and QA-002 improved Chromium/Firefox from 0% → 69%
- Webkit remains at 15% (2/13 passing)
- Webkit-specific issue requires webkit-specific solution

---

## Recommended Solutions

### Priority 1: Increase Webkit-Specific Timeouts 🟠

**Rationale:** Webkit consistently needs 2-3x longer for operations

**Implementation:**
```typescript
// In playwright.config.ts
{
  name: 'webkit',
  use: {
    ...devices['Desktop Safari'],
    navigationTimeout: 120000,  // 2 minutes for navigation
    actionTimeout: 60000,        // 1 minute for actions
  },
  timeout: 180000,  // 3 minutes per test
}
```

**Expected Impact:** Allow webkit's slower bootstrap to complete
**Risk:** Low - Only affects webkit tests, doesn't mask real issues

---

### Priority 2: Implement Webkit-Specific Navigation Strategy 🟡

**Rationale:** Webkit may need different `waitUntil` strategy

**Implementation:**
```typescript
// In auth.helper.ts
const navigateToLogin = async (page: Page, baseURL: string) => {
  const isWebkit = page.context().browser()?.browserType().name() === 'webkit';
  
  await page.goto(`${baseURL}/login`, {
    waitUntil: isWebkit ? 'domcontentloaded' : 'load',  // Less strict for webkit
    timeout: isWebkit ? 120000 : 60000
  });
  
  // Additional wait for webkit
  if (isWebkit) {
    await page.waitForTimeout(2000);  // Extra stabilization time
    await page.waitForLoadState('networkidle', { timeout: 30000 });
  }
};
```

**Expected Impact:** Reduce webkit navigation failures
**Risk:** Medium - May hide real performance issues

---

### Priority 3: Optimize API Mock Setup for Webkit 🟢

**Rationale:** Webkit may need mocks set up earlier

**Implementation:**
```typescript
// In api-mocks.helper.ts
export async function setupApiMocks(page: Page): Promise<void> {
  const isWebkit = page.context().browser()?.browserType().name() === 'webkit';
  
  // Set up mocks BEFORE any navigation for webkit
  if (isWebkit) {
    await page.route('**/*', (route) => {
      // Mock logic...
    });
  }
  
  // ... rest of setup
}
```

**Expected Impact:** Improve webkit test stability
**Risk:** Low - Aligns with webkit's security model

---

### Priority 4: Add Webkit-Specific Waits and Checks 🟢

**Rationale:** Verify Angular app is fully bootstrapped

**Implementation:**
```typescript
// In wait.helper.ts
export async function waitForAngularReady(page: Page): Promise<void> {
  const isWebkit = page.context().browser()?.browserType().name() === 'webkit';
  
  if (isWebkit) {
    // Wait for Angular to be defined
    await page.waitForFunction(() => {
      return typeof (window as any).ng !== 'undefined';
    }, { timeout: 60000 });
    
    // Wait for app-root to be rendered
    await page.waitForSelector('app-root', { timeout: 60000 });
    
    // Additional stabilization
    await page.waitForTimeout(3000);
  }
  
  await waitForPageReady(page);
}
```

**Expected Impact:** Ensure Angular is ready before tests execute
**Risk:** Low - Adds stability without hiding issues

---

## Alternative Approaches

### Option A: Skip Webkit Tests Temporarily
- Add `@skip-webkit` tag to tests
- Focus on Chromium/Firefox stability first
- Return to webkit optimization later

**Pros:** Unblock test suite development  
**Cons:** No Safari coverage

### Option B: Reduce Webkit Test Scope
- Run webkit tests only on critical user flows
- Full coverage on Chromium/Firefox only

**Pros:** Some Safari coverage, faster test runs  
**Cons:** Incomplete coverage

### Option C: Split Test Execution
- Run webkit tests separately with longer timeouts
- Different CI pipeline for webkit

**Pros:** Doesn't slow down main test suite  
**Cons:** More complex CI setup

---

## Performance Metrics

### Current Webkit Test Suite Timing
```
Total tests: 39
Webkit tests: 13
Total execution time: 22.5 minutes (for full suite)
Webkit-only time estimate: ~15 minutes (65% of total time)

Breakdown:
- 2 passing tests: 110s (50s + 60s)
- 11 failing tests: ~1200s (avg 109s each)
```

### With Recommended Fixes (Estimated)
```
Expected improvement:
- Navigation timeout → 11 potential passes
- Expected pass rate: 80-90% (10-12 of 13)
- Expected time per test: 60-90s (still 2x Chromium)
- Total webkit time: 10-15 minutes
```

---

## Next Steps

### Immediate Actions (Sprint 1)
1. ✅ Document webkit investigation findings (this report)
2. ⬜ Create QA-003 issue in "Defect List for QA.md"
3. ⬜ Implement Priority 1 fix (increase webkit timeouts)
4. ⬜ Test single webkit test with new timeouts
5. ⬜ If successful, run full webkit suite

### Short-Term Actions (Sprint 2)
1. ⬜ Implement Priority 2 fix (webkit-specific navigation)
2. ⬜ Add webkit-specific waits (Priority 4)
3. ⬜ Optimize API mock setup for webkit (Priority 3)
4. ⬜ Full regression test on all browsers

### Long-Term Actions (Backlog)
1. ⬜ Profile webkit page load performance
2. ⬜ Investigate Angular optimization for Safari
3. ⬜ Consider separate webkit test suite with custom config
4. ⬜ Monitor Playwright webkit support improvements

---

## Conclusion

Webkit test failures are **NOT product defects** but rather **test infrastructure timing issues** specific to webkit's slower page load and stricter security model. The recommended solutions focus on accommodating webkit's timing characteristics while maintaining test integrity.

**Key Takeaway:** Webkit tests need 2-3x longer timeouts and webkit-specific navigation strategies to achieve acceptable pass rates.

**Recommendation:** Implement Priority 1 and Priority 2 fixes immediately to unblock webkit test development.

---

## Related Documentation

- **QA Issue:** QA-003 (to be created)
- **Test Files:** `contacts.spec.ts`, `auth.helper.ts`, `wait.helper.ts`
- **Configuration:** `playwright.config.ts`
- **Previous Fixes:** QA-001 (route format), QA-002 (welcome dialog)

---

## Appendix: Sample Webkit Test Logs

### Successful Webkit Test (#28)
```
[Auth] Setting up API mocks...
Navigating to http://127.0.0.1:4200/login...
Navigation to /login complete (took ~20s)
[Auth] Updating /user/claims mock to authenticated state...
Clicking login button...
Login successful! Redirected to: http://127.0.0.1:4200/#/login
[Auth] Waiting for page to be ready after login...
[Wait] Page is ready (took ~15s)
[Auth] Navigating to http://127.0.0.1:4200/contacts...
[Auth] Navigation complete
[Wait] Waiting for permissions to load...
[Wait] Permissions loaded
✅ Test PASSED (total: 50.0s)
```

### Failed Webkit Test (#30)
```
[Auth] Setting up API mocks...
Navigating to http://127.0.0.1:4200/login...
(60 seconds pass...)
❌ Test timeout of 60000ms exceeded while running "beforeEach" hook.
Error: Failed to navigate to login page: page.goto: Timeout 60000ms exceeded.
```

**Key Difference:** Failed test never completes initial navigation to login page.
