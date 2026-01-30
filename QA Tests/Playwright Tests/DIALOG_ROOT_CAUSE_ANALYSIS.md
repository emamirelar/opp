# Root Cause Analysis: Dialog Rendering Failures in Playwright Tests

## Investigation Summary
**Date**: 2026-01-30  
**Status**: Root cause identified  
**Affected Tests**: 
- "should allow clicking New Contact button to open dialog"
- "should open business card scanner dialog"

---

## 🔍 Investigation Findings

### Test #1: New Contact Dialog Failure

**Current Behavior:**
- ✅ Button click succeeds
- ✅ All API calls for form data are intercepted (partners, org units, etc.)
- ✅ Mocks return valid JSON data
- ❌ Dialog component never renders

**Root Cause Discovered:**

#### Issue #1: CachedDataService Dependencies
```typescript
// From contact-edit-dialog.component.ts, line 181
ngOnInit() {
  this.cachedDataService.refreshPartners(); // ⚠️ Makes HTTP calls
  // ...
}
```

The dialog component uses `CachedDataService` to load dropdown data:
```typescript
// Lines 141-145
allSalutationsData = this.cachedDataService.allSalutations;
allStatusData = this.cachedDataService.allStatus;
allPronounsData = this.cachedDataService.allPronouns;
allPartners = this.cachedDataService.allPartners;
allOrganizationUnitsData = this.cachedDataService.allOrganizationUnits;
```

**Problem**: The `CachedDataService` may be making HTTP requests that aren't properly mocked, or it may be failing silently when it can't load reference data (salutations, pronouns, status).

#### Issue #2: PrimeNG DynamicDialog Rendering
```typescript
// From contact-list.component.ts, line 427
const ref = this.dialogService.open(ContactEditDialogComponent, {
  header: contactData.id ? 'Edit Contact' : 'New Contact',
  width: '40vw',
  breakpoints: { '960px': '95vw' },
  closable: true,
  data: {
    mode: contactData.id ? 'edit' : 'new',
    record: contactData,
  }
});
```

PrimeNG's `DynamicDialog` uses a different rendering mechanism than static `<p-dialog>` components. It:
1. Dynamically creates the dialog component
2. Injects it into a portal/overlay container
3. Manages its lifecycle separately from the parent component

**Problem**: The dynamic dialog may be failing to render due to:
- Missing `DynamicDialogRef` or `DynamicDialogConfig` injection context
- Component dependencies (services) that aren't properly available
- Angular change detection not triggering for dynamically created components

---

### Test #2: Business Card Scanner Dialog Failure

**Current Behavior:**
- ✅ Button click attempt
- ✅ Camera mocks in place
- ❌ Cannot click button (UI overlay blocking it)
- ❌ Dialog never found

**Root Cause Discovered:**

#### Issue #1: WRONG SELECTOR - Not a PrimeNG Dialog!
```html
<!-- From business-card-scanner.component.html, line 2 -->
<div [class]="dialogClasses()" (click)="!isMobile() && hide()">
  <!-- Custom div overlay, NOT <p-dialog> -->
</div>
```

**Problem**: 
- Test searches for: `p-dialog[role="dialog"]`
- Actual component uses: `<div>` with custom CSS classes
- **The dialog IS rendering, but we're looking for the wrong element!**

#### Issue #2: Component Rendered Conditionally
```html
<!-- From contact-list.component.html, lines 73-77 -->
<app-business-card-scanner 
  *ngIf="showBusinessCardScanner()"
  (onClose)="closeBusinessCardScanner()"
  (onScannedContact)="handleScannedContact($event)">
</app-business-card-scanner>
```

**Problem**:
The button click should set `showBusinessCardScanner()` to `true` via `openBusinessCardScanner()` method (line 468):
```typescript
openBusinessCardScanner() {
  if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
    // Show error toast
    return;
  }
  this.showBusinessCardScanner.set(true); // Should show the component
}
```

BUT:
- The test reports: "UI overlay (driver-overlay) intercepts pointer events"
- The click is being blocked by a tour/onboarding overlay
- Even with `force: true` click, the signal may not be set if Angular change detection doesn't run

---

## 🐛 Bugs Identified

### BUG #1: Business Card Scanner Test Uses Wrong Selector
**Severity**: High  
**Type**: Test Implementation Bug  
**Location**: `wait.helper.ts`, `contacts.spec.ts`, `assertions.helper.ts`

**Current Code:**
```typescript
// Looks for PrimeNG dialog
const dialog = page.locator('p-dialog[role="dialog"]:not([role="alertdialog"])').first();
```

**Actual Element:**
```html
<!-- Custom div overlay -->
<div class="fixed inset-0 z-[9999] bg-white">
```

**Fix Required:**
```typescript
// Test needs to look for the actual component or its custom div
const scanner = page.locator('app-business-card-scanner, [class*="fixed inset-0 z-\\[9999\\]"]');
await scanner.waitFor({ state: 'visible', timeout: 10000 });
```

---

### BUG #2: Missing API Mocks for CachedDataService
**Severity**: High  
**Type**: Test Configuration Bug  
**Location**: `api-mocks.helper.ts`

**Missing Endpoints:**
- `/api/values/salutations` - Dropdown options (Mr., Mrs., Ms., Dr., etc.)
- `/api/values/status` - Status options (Active, Inactive, etc.)
- `/api/values/pronouns` - Pronoun options (He/Him, She/Her, They/Them, etc.)
- `/api/values/countries` - Country list for mailing address
- `/api/values/states` - State/Province options

**Evidence:**
The form data endpoints we mocked ARE being called (visible in logs), but the CachedDataService reference data is loaded **globally on app init**, not when the dialog opens. If these aren't mocked, the CachedDataService signals remain empty, and form dropdowns may fail to initialize.

---

### BUG #3: Tour/Onboarding Overlay Blocks UI Interactions
**Severity**: Medium  
**Type**: Test Environment Issue  
**Location**: Test setup in `contacts.spec.ts`

**Error Message:**
```
<svg class="driver-overlay driver-overlay-animated"> subtree intercepts pointer events
```

**Problem:**
The application shows an onboarding tour dialog on first visit, which overlays the entire UI and blocks clicks. While the auth helper tries to dismiss this (`dismissWelcomeTourDialog()`), it may not be fully dismissed before the test proceeds.

**Current Workaround:**
```typescript
await scannerButton.click({ force: true }); // Bypasses click-blocking elements
```

**Proper Fix:**
Ensure tour is fully dismissed and animations complete before proceeding with tests, or disable tour in test environment.

---

## 🔧 Recommended Fixes

### Fix #1: Update Business Card Scanner Test Selector

**File**: `contacts.spec.ts`

```typescript
// BEFORE
test('should open business card scanner dialog', async ({ page }) => {
  await contactsPage.clickScannerButton();
  await assertDialogOpen(page); // ❌ Looks for p-dialog
});

// AFTER
test('should open business card scanner dialog', async ({ page }) => {
  await contactsPage.clickScannerButton();
  
  // Wait for the actual business card scanner component (custom div overlay)
  const scanner = page.locator('app-business-card-scanner').first();
  await scanner.waitFor({ state: 'attached', timeout: 10000 });
  
  // Verify the overlay div is visible
  const overlayVisible = await page.locator('[class*="fixed inset-0 z-"]').first().isVisible();
  expect(overlayVisible).toBe(true);
});
```

---

### Fix #2: Add Missing Reference Data API Mocks

**File**: `api-mocks.helper.ts`

```typescript
// Add after existing values mocks:

// Mock /api/values/salutations - Salutation dropdown
await page.route(url => url.toString().includes('/api/values/salutations'), async (route) => {
  console.log('[API Mock] Intercepted: /api/values/salutations');
  await route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify([
      { id: 1, name: 'Mr.' },
      { id: 2, name: 'Ms.' },
      { id: 3, name: 'Mrs.' },
      { id: 4, name: 'Dr.' },
      { id: 5, name: 'Prof.' },
    ]),
  });
});

// Mock /api/values/status - Status dropdown
await page.route(url => url.toString().includes('/api/values/status'), async (route) => {
  console.log('[API Mock] Intercepted: /api/values/status');
  await route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify([
      { id: 1, name: 'Active' },
      { id: 2, name: 'Inactive' },
    ]),
  });
});

// Mock /api/values/pronouns - Pronouns dropdown
await page.route(url => url.toString().includes('/api/values/pronouns'), async (route) => {
  console.log('[API Mock] Intercepted: /api/values/pronouns');
  await route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify([
      { id: 1, name: 'He/Him' },
      { id: 2, name: 'She/Her' },
      { id: 3, name: 'They/Them' },
      { id: 4, name: 'Other' },
    ]),
  });
});

// Mock /api/values/countries - Countries dropdown
await page.route(url => url.toString().includes('/api/values/countries'), async (route) => {
  console.log('[API Mock] Intercepted: /api/values/countries');
  await route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify([
      { id: 'US', name: 'United States' },
      { id: 'GB', name: 'United Kingdom' },
      { id: 'FR', name: 'France' },
      { id: 'DE', name: 'Germany' },
      { id: 'CH', name: 'Switzerland' },
    ]),
  });
});
```

---

### Fix #3: Investigate Dynamic Dialog Rendering

**Approach**: Add browser console logging to understand if the `dialogService.open()` call succeeds but the component fails to render, or if the call itself fails.

**Test Enhancement:**
```typescript
test('should open New Contact dialog', async ({ page }) => {
  // Listen for console errors DURING dialog open
  const consoleErrors: string[] = [];
  page.on('console', msg => {
    if (msg.type() === 'error') {
      consoleErrors.push(msg.text());
    }
  });
  
  await contactsPage.waitForPermissions();
  await contactsPage.newButton.click();
  
  // Wait and check for dialog
  await page.waitForTimeout(3000);
  
  const dialogVisible = await page.locator('p-dialog[role="dialog"]').isVisible().catch(() => false);
  
  if (!dialogVisible) {
    console.error('[Test] Dialog did not render. Console errors:', consoleErrors);
    // Log what elements ARE visible
    const visibleDialogs = await page.locator('p-dialog, [role="dialog"], .p-dialog').count();
    console.log(`[Test] Found ${visibleDialogs} dialog-like elements`);
  }
  
  expect(dialogVisible).toBe(true);
});
```

---

### Fix #4: Ensure Tour is Fully Dismissed

**File**: `auth.helper.ts`

```typescript
// After dismissing welcome tour
export async function dismissWelcomeTourDialog(page: Page): Promise<void> {
  try {
    const welcomeDialog = page.locator('button:has-text("Start Tour"), button:has-text("Skip")').first();
    if (await welcomeDialog.isVisible({ timeout: 2000 })) {
      await welcomeDialog.click();
      console.log('[Auth] Welcome dialog dismissed');
      
      // ✅ ADD: Wait for overlay to fully disappear
      await page.waitForTimeout(1000);
      
      // ✅ ADD: Verify no blocking overlays remain
      const overlay = page.locator('.driver-overlay, [class*="driver-overlay"]');
      await overlay.waitFor({ state: 'hidden', timeout: 5000 }).catch(() => {
        console.warn('[Auth] Tour overlay may still be present');
      });
    }
  } catch (error) {
    console.log('[Auth] No welcome dialog found');
  }
}
```

---

## 📊 Hypothesis Testing Plan

### Hypothesis #1: CachedDataService HTTP Calls Fail
**Test**: Add console logging for ALL API requests during dialog open  
**Expected**: Should see requests for `/api/values/salutations`, `/api/values/pronouns`, etc.  
**Action**: If these requests appear, add mocks. If they don't, CachedDataService may be failing silently.

### Hypothesis #2: DynamicDialog Component Fails to Inject Dependencies
**Test**: Check browser console for Angular dependency injection errors  
**Expected**: May see errors like "NullInjectorError: No provider for X"  
**Action**: Identify missing service providers and add test stubs

### Hypothesis #3: Business Card Scanner IS Rendering (Wrong Selector)
**Test**: Change test selector from `p-dialog[role="dialog"]` to `app-business-card-scanner`  
**Expected**: Scanner component found and visible  
**Action**: Update test assertions to match actual component structure

---

## ✅ Action Items

### Priority 1: Fix Business Card Scanner Test (Easy Win)
- [ ] Update selector from `p-dialog[role="dialog"]` to `app-business-card-scanner`
- [ ] Verify scanner component renders when button is clicked
- [ ] Expected result: Test passes

### Priority 2: Add Missing Reference Data Mocks
- [ ] Add mocks for salutations, status, pronouns, countries
- [ ] Update catch-all filter to exclude these endpoints
- [ ] Verify CachedDataService receives valid data

### Priority 3: Add Console Error Capture
- [ ] Implement browser console monitoring in tests
- [ ] Log all errors during dialog open attempt
- [ ] Identify specific Angular/PrimeNG errors preventing render

### Priority 4: Debug DynamicDialog Initialization
- [ ] Check if `dialogService.open()` returns valid `DynamicDialogRef`
- [ ] Verify component dependencies can be resolved
- [ ] Test with actual backend vs mocked environment

---

## 🎯 Expected Outcomes

### After Applying Fixes:

**Business Card Scanner Test:**
- **Current**: ❌ Fails (wrong selector)
- **Expected**: ✅ Passes (correct selector finds custom overlay)

**New Contact Dialog Test:**
- **Current**: ❌ Fails (dialog doesn't render)
- **Expected**: ⚠️ May still fail (needs deeper investigation)
- **Fallback**: May need to test against real backend or skip test with documented issue

---

## 📝 Decision: Approach Selection

### Option A: Fix What We Can (Recommended)
1. ✅ Fix business card scanner test (easy, clear fix)
2. ✅ Add missing reference data mocks (low effort, may help)
3. ⚠️ Mark New Contact dialog test as `.failing()` with investigation needed

### Option B: Skip Both Tests (Pragmatic)
1. Mark both as `.skip()` with detailed TODO comments
2. Document that these require real backend or deeper Angular debugging
3. Focus on tests that verify core functionality

### Option C: Deep Investigation (Time Intensive)
1. Set up Angular dev server with full backend integration
2. Run tests against real backend to verify feature works
3. Debug why mocked environment prevents DynamicDialog rendering
4. May require changes to application code or test environment

---

## 🚦 Recommendation

**Immediate Action** (Option A):
1. **Fix business card scanner test** - Update selector to `app-business-card-scanner`
2. **Add reference data mocks** - Complete the mocking for CachedDataService
3. **Re-run tests** - See if New Contact dialog now renders
4. **If still fails** - Mark New Contact test as `.failing()` with this analysis linked

This approach:
- ✅ Fixes one test definitively (scanner)
- ✅ Improves mocking completeness (benefits all tests)
- ✅ Documents the remaining issue for future investigation
- ✅ Maintains honest test results (failing tests show real issues)

---

## 📎 References
- **New Contact Dialog Component**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component.ts`
- **Contact List Component**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/list/contact-list.component.ts`
- **Business Card Scanner**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/list/business-card-scanner/business-card-scanner.component.ts`
- **PrimeNG DynamicDialog Docs**: https://primeng.org/dynamicdialog
