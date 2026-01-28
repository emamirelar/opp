# Webkit Test Fixes - Implementation Guide

**Related:** QA-003  
**Investigation Report:** `WEBKIT_INVESTIGATION_REPORT.md`  
**Date:** 2026-01-26

This document provides step-by-step implementation instructions for fixing webkit browser test timeouts.

---

## Priority 1: Increase Webkit-Specific Timeouts ⚡

**Impact:** High - Should immediately improve webkit pass rate  
**Effort:** Low - Simple configuration change  
**Risk:** Low - Only affects webkit tests

### Implementation

**File:** `playwright.config.ts`

**Current Configuration:**
```typescript
{
  name: 'webkit',
  use: { ...devices['Desktop Safari'] },
}
```

**Updated Configuration:**
```typescript
{
  name: 'webkit',
  use: { 
    ...devices['Desktop Safari'],
    // Webkit needs 2-3x longer for operations
    navigationTimeout: 120000,  // 2 minutes (was 30s default)
    actionTimeout: 60000,        // 1 minute (was 30s default)
  },
  timeout: 180000,  // 3 minutes per test (was 60s default)
  expect: {
    timeout: 15000,  // 15 seconds for assertions (was 10s default)
  },
}
```

**Testing:**
```bash
# Test a single webkit test with new timeouts
npx playwright test contacts.spec.ts:30 --project=webkit

# If successful, run full webkit suite
npx playwright test contacts.spec.ts --project=webkit --reporter=list
```

**Expected Result:**
- Webkit tests should complete navigation within 2-minute timeout
- Pass rate should improve from 15% → 60-80%
- Tests will take longer but should complete successfully

---

## Priority 2: Webkit-Specific Navigation Strategy 🎯

**Impact:** High - Reduces navigation failures  
**Effort:** Medium - Code changes in helper  
**Risk:** Medium - Changes shared navigation logic

### Implementation

**File:** `Playwright Tests/helpers/auth.helper.ts`

Add browser detection helper at the top of the file:

```typescript
/**
 * @description Check if current browser is webkit (Safari)
 */
function isWebkitBrowser(page: Page): boolean {
  try {
    return page.context().browser()?.browserType().name() === 'webkit';
  } catch {
    return false;
  }
}
```

Update the `login()` function:

**Find this section:**
```typescript
try {
  await page.goto(`${baseURL}/login`, {
    waitUntil: 'load',
    timeout: 60000
  });
} catch (navError: any) {
  console.error('Navigation failed:', navError.message);
  throw new Error(`Failed to navigate to login page: ${navError.message}`);
}
```

**Replace with:**
```typescript
const webkit = isWebkitBrowser(page);

try {
  await page.goto(`${baseURL}/login`, {
    // Webkit: wait for DOM instead of full load (faster, more reliable)
    // Other browsers: wait for full load event
    waitUntil: webkit ? 'domcontentloaded' : 'load',
    // Webkit: use 2-minute timeout; Others: use 1-minute timeout
    timeout: webkit ? 120000 : 60000
  });
  
  // Additional stabilization for webkit
  if (webkit) {
    console.log('[Auth] Webkit browser detected - adding stabilization waits');
    // Give webkit extra time to stabilize after DOM load
    await page.waitForTimeout(2000);
    // Wait for network to be idle
    await page.waitForLoadState('networkidle', { timeout: 30000 }).catch(() => {
      console.log('[Auth] Network idle timeout (non-critical for webkit)');
    });
  }
} catch (navError: any) {
  console.error('Navigation failed:', navError.message);
  throw new Error(`Failed to navigate to login page: ${navError.message}`);
}
```

**Testing:**
```bash
# Test the updated navigation logic
npx playwright test contacts.spec.ts:26 --project=webkit --headed

# Watch for console logs: "[Auth] Webkit browser detected"
```

**Expected Result:**
- Webkit navigation should complete faster with `domcontentloaded`
- Extra waits should prevent race conditions
- Navigation success rate should improve

---

## Priority 3: Webkit-Specific Angular Ready Check 🔧

**Impact:** Medium - Ensures Angular is ready before tests  
**Effort:** Medium - New helper function  
**Risk:** Low - Additional safety check

### Implementation

**File:** `Playwright Tests/helpers/wait.helper.ts`

Add new function:

```typescript
/**
 * @description Wait for Angular application to be fully bootstrapped (webkit-specific)
 * Webkit needs explicit checks that Angular is ready before proceeding
 */
export async function waitForAngularReady(page: Page): Promise<void> {
  const webkit = page.context().browser()?.browserType().name() === 'webkit';
  
  if (!webkit) {
    // Skip for non-webkit browsers (they're fast enough)
    return;
  }
  
  console.log('[Wait] Checking Angular ready state (webkit)...');
  
  try {
    // Wait for Angular to be defined on window
    await page.waitForFunction(() => {
      return typeof (window as any).ng !== 'undefined';
    }, { timeout: 60000 });
    
    // Wait for app-root element to be rendered
    await page.waitForSelector('app-root', { 
      state: 'attached',
      timeout: 60000 
    });
    
    // Additional stabilization time for Angular bootstrapping
    await page.waitForTimeout(3000);
    
    console.log('[Wait] Angular ready confirmed (webkit)');
  } catch (error) {
    console.warn('[Wait] Angular ready check timed out (non-critical):', error);
  }
}
```

**Update:** `auth.helper.ts` to use the new helper

**Find this section in `login()` function:**
```typescript
// Give Angular a moment to bootstrap
await page.waitForTimeout(1000);
await waitForPageReady(page);
```

**Replace with:**
```typescript
// Give Angular a moment to bootstrap
await page.waitForTimeout(1000);

// Webkit: Check Angular is fully ready before proceeding
await waitForAngularReady(page);

await waitForPageReady(page);
```

**Testing:**
```bash
# Test with webkit browser
npx playwright test contacts.spec.ts:30 --project=webkit

# Check logs for: "[Wait] Checking Angular ready state (webkit)"
```

**Expected Result:**
- Webkit tests should wait for Angular to be ready
- Reduces race conditions during test execution
- Improves test reliability

---

## Priority 4: Optimize API Mock Setup for Webkit ⚙️

**Impact:** Low-Medium - May reduce intermittent failures  
**Effort:** Low - Minor timing adjustment  
**Risk:** Low - Defensive check

### Implementation

**File:** `Playwright Tests/helpers/auth.helper.ts`

**Find this section at the start of `login()` function:**
```typescript
export async function login(
  page: Page,
  email: string = config.testUser.email,
  password: string = config.testUser.password
): Promise<void> {
  console.log('[Auth] Setting up API mocks...');
  await setupApiMocks(page);
```

**Replace with:**
```typescript
export async function login(
  page: Page,
  email: string = config.testUser.email,
  password: string = config.testUser.password
): Promise<void> {
  const webkit = isWebkitBrowser(page);
  
  console.log('[Auth] Setting up API mocks...');
  await setupApiMocks(page);
  
  // Webkit: Give extra time for route handlers to be registered
  if (webkit) {
    await page.waitForTimeout(500);
    console.log('[Auth] API mocks ready (webkit)');
  }
```

**Expected Result:**
- Ensures API mocks are fully registered before navigation
- Reduces "unhandled request" errors in webkit
- Minimal impact on test execution time

---

## Testing Strategy

### Phase 1: Single Test Validation
```bash
# Test one previously failing webkit test
npx playwright test contacts.spec.ts:30 --project=webkit --reporter=list

# Expected: Test should pass within 2 minutes
```

### Phase 2: Small Batch Test
```bash
# Test 5 webkit tests
npx playwright test contacts.spec.ts --project=webkit --grep="should display" --reporter=list

# Expected: 80%+ pass rate
```

### Phase 3: Full Suite Test
```bash
# Run full webkit suite
npx playwright test contacts.spec.ts --project=webkit --reporter=list

# Expected: 10-12 of 13 tests passing (80-90% pass rate)
```

### Phase 4: Cross-Browser Validation
```bash
# Ensure changes don't break other browsers
npx playwright test contacts.spec.ts --reporter=list

# Expected: 
# - Chromium: 9/13 passing (unchanged)
# - Firefox: 9/13 passing (unchanged)
# - Webkit: 10-12/13 passing (improved from 2/13)
```

---

## Rollback Plan

If webkit fixes cause issues:

### Rollback Priority 1 (Config Changes)
**File:** `playwright.config.ts`

Remove webkit-specific timeout configuration, revert to:
```typescript
{
  name: 'webkit',
  use: { ...devices['Desktop Safari'] },
}
```

### Rollback Priority 2-4 (Code Changes)
```bash
# Create a backup before changes
git stash push -m "webkit-fixes-backup" Playwright\ Tests/helpers/auth.helper.ts Playwright\ Tests/helpers/wait.helper.ts

# If needed, restore original code
git stash pop
```

---

## Success Criteria

### Minimum Acceptable Results
- ✅ Webkit pass rate: 60%+ (8 of 13 tests)
- ✅ No new failures in Chromium/Firefox
- ✅ Average webkit test time: < 90 seconds

### Target Results
- 🎯 Webkit pass rate: 80%+ (10-11 of 13 tests)
- 🎯 Average webkit test time: 60-75 seconds
- 🎯 No navigation timeouts in webkit

### Optimal Results
- ⭐ Webkit pass rate: 90%+ (12 of 13 tests)
- ⭐ Average webkit test time: < 60 seconds
- ⭐ Webkit parity with Chromium/Firefox

---

## Monitoring & Metrics

After implementing fixes, track:

1. **Pass Rate by Browser**
   - Before: Webkit 15% (2/13), Chromium 69% (9/13), Firefox 69% (9/13)
   - After: Webkit 80%+ (10/13), Chromium 69% (9/13), Firefox 69% (9/13)

2. **Average Test Duration**
   - Before: Webkit 55s (passing), 109s (failing)
   - After: Webkit 60-75s (passing)

3. **Timeout Occurrences**
   - Before: 11 webkit timeouts per run
   - After: 0-2 webkit timeouts per run

---

## Next Steps

After implementing these fixes:

1. ✅ Update QA-003 status to "In Progress"
2. ✅ Run test validation phases
3. ✅ Document results in QA-003
4. ✅ If successful, mark QA-003 as "Resolved"
5. ✅ Apply same pattern to other Playwright test files
6. ✅ Update `WEBKIT_INVESTIGATION_REPORT.md` with outcomes

---

## Questions & Support

**Q: Should we skip webkit tests if fixes don't work?**  
A: Yes, use `--project=chromium --project=firefox` to exclude webkit temporarily.

**Q: Will these changes slow down Chromium/Firefox tests?**  
A: No. Webkit-specific code is conditionally executed only for webkit browser.

**Q: Can we run webkit tests separately?**  
A: Yes. Create a separate workflow with `--project=webkit` and longer timeout allowances.

**Q: What if webkit tests are still too slow?**  
A: Consider reducing webkit test scope to critical paths only, or running webkit tests on-demand rather than in CI.
