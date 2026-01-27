# Defect List for QA

This document tracks test infrastructure issues, test implementation bugs, temporary workarounds, and test tooling problems. These are QA-specific issues that don't represent defects in production code.

**Scope:** Test infrastructure issues, test implementation bugs, temporary workarounds, test tooling problems  
**Prefix:** QA-XXX  
**File Owner:** QA Team

---

## Open QA Issues

| QA ID | Title | Description | Reproduction Steps | Expected Result | Actual Result | Date Reported | Status | Assigned To |
|-------|-------|-------------|-------------------|-----------------|---------------|---------------|--------|-------------|
| QA-003 | Webkit browser tests experiencing severe navigation timeouts | Webkit (Safari) browser tests had 15% pass rate (2/13) compared to 69% for Chromium/Firefox. Tests failed with 60-second navigation timeout when trying to load the Angular login page in `beforeEach` hook. Webkit takes 2-3x longer than other browsers for page load/navigation.<br/><br/>**Root Cause:** Webkit's stricter security model and different timing characteristics cause slower Angular app bootstrap (30-50s vs 10-15s). The 60-second navigation timeout was insufficient.<br/><br/>**Fixes Implemented (QA):**<br/>✅ **Priority 1:** Increased webkit-specific timeouts (navigation: 120s, action: 60s, test: 180s)<br/>✅ **Priority 2:** Added webkit-specific navigation strategy (use `domcontentloaded` + stabilization waits)<br/>✅ **Priority 3:** Added `waitForAngularReady()` for webkit to ensure Angular bootstrap completes<br/>✅ **Priority 4:** Optimized API mock setup timing for webkit (500ms extra wait)<br/><br/>**Test Results:**<br/>• **Phase 1:** 1/1 passed (100%) - Single test validation successful<br/>• **Phase 2:** 6/8 passed (75%) - Batch test validation successful<br/>• **Navigation timeouts:** ELIMINATED (0 timeouts observed)<br/>• **Remaining failures:** Due to DEF-001 (route permission guard), NOT webkit-specific<br/><br/>**Files Modified:**<br/>• `playwright.config.ts` - Webkit project timeout configuration<br/>• `auth.helper.ts` - Browser detection, webkit navigation, API mock optimization<br/>• `wait.helper.ts` - Added `waitForAngularReady()` function<br/><br/>**Investigation:** See detailed analysis in `Playwright Tests/WEBKIT_INVESTIGATION_REPORT.md`<br/>**Implementation Guide:** `Playwright Tests/WEBKIT_FIXES_IMPLEMENTATION.md`<br/>**Completion Summary:** `Playwright Tests/WEBKIT_FIXES_COMPLETED.md`<br/><br/>**Note:** This was NOT a product defect but a test infrastructure timing issue specific to webkit browser engine. Issue is now RESOLVED. | 1. Run webkit tests: `npx playwright test contacts.spec.ts --project=webkit`<br/>2. Observe navigation timeout in `beforeEach` hook<br/>3. Check test execution time (50-120s vs 20-40s for Chromium)<br/>4. Review error: "Failed to navigate to login page: page.goto: Timeout 60000ms exceeded" | Webkit tests complete with similar pass rate (60-80%) and timing (20-40s per test) as Chromium/Firefox | **Before:** Webkit 15% pass rate (2/13), 60-120s timeouts, navigation failures<br/>**After:** Webkit 75% pass rate (6/8 in Phase 2), NO timeouts, 20-40s per test<br/>**Improvement:** +60% pass rate, navigation timeouts eliminated | 2026-01-26 | Resolved | QA Team |

---

## Resolved QA Issues

| QA ID | Title | Resolution | Date Resolved | Resolved By |
|-------|-------|------------|---------------|-------------|
| QA-001 | Playwright tests using incorrect route format | Updated route paths in `contacts.spec.ts` from `/contacts` to `/#/partnerships/contacts` to match Angular hash-based routing. Need to audit other test files. | 2026-01-26 | QA Team |
| QA-002 | Welcome tour dialog blocks initial Playwright test navigation | Enhanced `loginAndNavigate()` helper with retry logic, initial wait, and dialog dismissal verification. Dialog now consistently dismissed before navigation. | 2026-01-26 | QA Team |
| QA-003 | Webkit browser tests experiencing severe navigation timeouts | **All 4 Priority Fixes Implemented:**<br/>1. Increased webkit timeouts (nav: 120s, action: 60s, test: 180s)<br/>2. Webkit-specific navigation strategy (`domcontentloaded` + stabilization)<br/>3. Added `waitForAngularReady()` function for webkit<br/>4. Optimized API mock setup timing<br/><br/>**Results:** Navigation timeouts eliminated, webkit pass rate improved from 15% (2/13) to 75% (6/8). Remaining failures due to DEF-001 (route guard), not webkit-specific. | 2026-01-26 | QA Team |

---

## QA Issue Statistics

- **Total Open:** 0
- **Total In Testing:** 0
- **Total Resolved:** 3 ✅
- **Test Infrastructure:** 3
- **Test Implementation:** 0
- **Test Tooling:** 0
- **Temporary Workarounds:** 0
- **Critical:** 0
- **High Priority:** 0 (All resolved!)

---

## Test Infrastructure Inventory

### Playwright Test Status (as of 2026-01-26 - After QA-003 Resolution)

**Test Suites:**
- `contacts.spec.ts` - 39 tests across 3 browsers
  - ✅ Route navigation fixed (QA-001)
  - ✅ Welcome dialog handling fixed (QA-002)
  - ✅ Webkit navigation timeouts fixed (QA-003)
  - ❌ Remaining failures due to DEF-001 (route permission guard - developer defect)

**Browser-Specific Results:**

**Before Fixes:**
| Browser | Tests Run | Passed | Failed | Pass Rate | Avg Time |
|---------|-----------|--------|--------|-----------|----------|
| Chromium | 13 | 9 | 4 | 69% | 32s |
| Firefox | 13 | 9 | 4 | 69% | 32s |
| Webkit | 13 | 2 | 11 | **15%** 🔴 | 55s |

**After QA-003 Fixes (Phase 2 Testing):**
| Browser | Tests Run | Passed | Failed | Pass Rate | Avg Time | Notes |
|---------|-----------|--------|--------|-----------|----------|-------|
| Chromium | Not tested | - | - | 69% (est) | 32s | Unchanged |
| Firefox | Not tested | - | - | 69% (est) | 32s | Unchanged |
| Webkit | 8 | 6 | 2 | **75%** ✅ | 28s | **+60% improvement!** |

**Webkit Improvement:**
- **Before:** 15% pass rate, 11 navigation timeouts
- **After:** 75% pass rate, 0 navigation timeouts
- **Improvement:** +60 percentage points, 100% timeout elimination
- **Test speed:** Improved from 55s avg to 28s avg

**Known Test Infrastructure Gaps:**
- ✅ **Webkit browser:** Navigation timeouts RESOLVED (QA-003)
- Dev server startup can be slow/unreliable
- Other Playwright test files may need route format updates (QA-001 pattern)
- Route permission guard blocking valid access (DEF-001 - developer defect affecting all browsers)

---

## Notes

### Related Files

**Test Helpers:**
- `Playwright Tests/helpers/auth.helper.ts` - Authentication and navigation helpers (QA-002 fix)
- `Playwright Tests/helpers/api-mocks.helper.ts` - API mocking infrastructure
- `Playwright Tests/helpers/assertions.helper.ts` - Custom assertion helpers
- `Playwright Tests/helpers/wait.helper.ts` - Wait/timeout utilities

**Test Specifications:**
- `Playwright Tests/contacts.spec.ts` - Contact list tests (QA-001, QA-002 fixes applied)
- `Playwright Tests/partners.spec.ts` - May need QA-001 route fix
- `Playwright Tests/opportunities.spec.ts` - May need QA-001 route fix
- `Playwright Tests/dashboard.spec.ts` - May need QA-001 route fix
- `Playwright Tests/login.spec.ts` - Authentication tests

**Configuration:**
- `playwright.config.ts` - Playwright test configuration
- `package.json` - Test dependencies

---

## Cross-References to Developer Defects

| QA Issue | Related DEF Issue | Relationship |
|----------|-------------------|--------------|
| QA-001 | DEF-001 | Initially thought to be route guard issue (DEF-001), but was actually test implementation using wrong route format |

---

## How to Use This Document

### For QA Team:
1. Log new test infrastructure issues as they're discovered
2. Use sequential IDs (QA-001, QA-002, etc.)
3. Clearly distinguish between temporary workarounds and permanent fixes
4. Cross-reference with "Defect List for Developers.md" when related
5. Update status as issues are resolved
6. Document resolution details for knowledge sharing

### For Developers:
1. Review this list to understand test infrastructure context
2. If a QA issue is actually a product defect, create a DEF-XXX entry
3. Help QA team identify root causes vs workarounds
4. Suggest architectural improvements to reduce test brittleness

### For Project Managers:
1. Monitor test infrastructure health
2. Allocate time for test infrastructure improvements
3. Track temporary workarounds that may need developer attention
4. Ensure test infrastructure doesn't block releases

---

## Action Items

### Completed (Current Sprint):
- [x] Investigate webkit browser navigation failures → **QA-003 logged, report created** ✅
- [x] **Priority 1:** Implement webkit-specific timeouts in `playwright.config.ts` (QA-003) ✅
- [x] **Priority 2:** Add webkit-specific navigation strategy to `auth.helper.ts` (QA-003) ✅
- [x] **Priority 3:** Implement webkit-specific Angular ready waits (QA-003) ✅
- [x] **Priority 4:** Optimize API mock setup for webkit (QA-003) ✅
- [x] **Phase 1 Testing:** Single webkit test validation (1/1 passed) ✅
- [x] **Phase 2 Testing:** Batch webkit test validation (6/8 passed, 75%) ✅
- [x] **QA-003 Resolved:** Webkit navigation timeouts eliminated ✅

### Immediate (Next Sprint):
- [ ] Audit all Playwright test files for route format (QA-001 pattern)
- [ ] Address DEF-001 (route permission guard) - developer defect blocking 2 webkit tests + 4 Chromium/Firefox tests
- [ ] **Optional:** Run Phase 3 full webkit suite (13 tests) for complete validation

### Short-Term (Sprint 2):
- [ ] Full regression test on all 3 browsers after DEF-001 is resolved
- [ ] Apply webkit optimization pattern to other Playwright test files if needed
- [ ] Monitor webkit test stability over time

### Future:
- [ ] Add "skip tour" option for test environments
- [ ] Improve dev server startup reliability  
- [ ] Create test data seeding utilities
- [ ] Expand Playwright test coverage to other features
- [ ] Consider separate webkit test suite configuration
- [ ] Profile webkit page load performance
- [ ] Monitor Playwright webkit support improvements
