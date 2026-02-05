# Defect List for Developers

This document tracks **production code defects** discovered during testing. These are issues where implemented functionality does not match documented requirements (PRD, specifications, acceptance criteria).

**Scope:** All production code defects requiring developer intervention:
- ✅ **Functional:** Business logic bugs, incorrect behavior, missing features
- ✅ **API/Integration:** Contract violations, endpoint failures, data mapping issues
- ✅ **Architecture:** Design flaws, async/sync problems, dependency issues
- ✅ **Security:** Vulnerabilities, auth bypass, data exposure, injection flaws
- ✅ **Performance:** Slow queries, memory leaks, N+1 problems, resource exhaustion
- ✅ **Accessibility:** WCAG violations, keyboard nav, screen reader issues
- ✅ **Data Integrity:** Calculation errors, constraint violations, data corruption
- ✅ **Error Handling:** Unhandled exceptions, poor messages, silent failures
- ✅ **Compatibility:** Browser-specific bugs, device/OS issues, responsive layout
- ✅ **Internationalization:** Translation bugs, locale formatting, RTL issues
- ✅ **Concurrency:** Race conditions, deadlocks, thread safety problems
- ✅ **Observability:** Missing logs, audit gaps, inadequate telemetry
- ✅ **Configuration:** Wrong defaults, missing feature flags, env issues
- ✅ **Dependencies:** Vulnerable packages, version conflicts, deprecated APIs

**Prefix:** DEF-XXX  
**File Owner:** Development Team

> ⚠️ **Important Distinction**: This list is for **actual defects** in production code, NOT:
> - Test infrastructure issues → See `Defect List for QA.md`
> - Tests written for unimplemented features → Track in backlog/sprint planning
> - Missing test attributes → Track as Definition of Done improvements
> - Test environment limitations → See `Defect List for QA.md`

---

## Open Defects

| Defect ID | Title | Description | Reproduction Steps | Expected Result | Actual Result | Date Reported | Status |
|-----------|-------|-------------|-------------------|-----------------|---------------|---------------|---------|
| DEF-008 | Go Decision Feature Incomplete - PRD Requirements Not Implemented | **96% of PRD requirements not yet implemented** for "Send Opportunity for Go Decision" feature.<br/><br/>**📋 PRD Reference:** Product Requirements Document: Send Opportunity for Go Decision<br/>**📊 Test Cases Created:** 102 test cases aligned with PRD<br/>**⚠️ Tests Executable:** 4 (4%)<br/>**❌ Tests Blocked:** 98 (96%)<br/><br/>**Current Implementation (OpportunityStageRequirements.cs):**<br/>• ✅ Name validation<br/>• ✅ Description validation<br/>• ✅ ResponsibleOrgUnitId validation<br/>• ✅ InitiativeBudgetUSD validation (optional)<br/><br/>**Missing PRD Requirements (Not Implemented):**<br/><br/>**1. Mandatory Field Validation (16+ fields missing):**<br/>• ❌ Context & Challenges<br/>• ❌ UNOPS Strategic Mission(s) (minLength=1)<br/>• ❌ Expected Impact<br/>• ❌ Expected Outcomes<br/>• ❌ SDG Alignment (minLength=1)<br/>• ❌ Funding Partner with amount/currency<br/>• ❌ Client Partner<br/>• ❌ Products & Services<br/>• ❌ Countries of Implementation<br/>• ❌ Target Signing Date<br/>• ❌ Implementation Start/End Dates<br/>• ❌ Opportunity Manager role validation<br/>• ❌ Proposed Initiative Type<br/>• ❌ DoA Level 2 holder (server-side)<br/>• ❌ Opportunity Statement generated<br/>• ❌ UNCooperation Framework Outcome(s)<br/>• ❌ Estimated Beneficiaries OR acknowledgement<br/>• ❌ High Risk Acknowledgement<br/><br/>**2. DoA Level 2 Approver Lookup (FR-1):**<br/>• ❌ Query EntityUserRole with Code="DoA2_OrganizationHierarchy"<br/>• ❌ Block submission if no DoA2 found<br/>• ❌ Support multiple DoA2 holders<br/><br/>**3. Warnings & Acknowledgments:**<br/>• ❌ Non-OM submitter warning (Collaborator role)<br/>• ❌ Country-Org Unit mismatch warning<br/>• ❌ Mandatory acknowledgment statement<br/>• ❌ Additional remarks field<br/><br/>**4. Custom Workflow Behavior:**<br/>• ❌ Rejection → NO GO (not previous stage)<br/>• ❌ CANCELLED stage with cancel/reopen<br/>• ❌ OM recall (any OM, not just submitter)<br/>• ❌ OM role transfer (OM → Collaborator)<br/><br/>**5. Notifications:**<br/>• ❌ Email templates with exact wording<br/>• ❌ OIC notifications<br/>• ❌ Internal stakeholder notifications on GO<br/><br/>**6. UI Components:**<br/>• ❌ Stage stepper display logic (happy path only)<br/>• ❌ DoA pathway display (DoA2/DoA3 read-only)<br/>• ❌ Inactive OM visibility<br/>• ❌ In-workflow indicator on opportunity card<br/><br/>**📍 Test Case Location:**<br/>`QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_PRD_TestCases.md`<br/><br/>**📍 Execution Report:**<br/>`QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_TestExecution_Report.md`<br/><br/>**⏰ ESTIMATED EFFORT:** 80-120 hours (full feature implementation)<br/>**📊 PRIORITY:** P1 - Feature required for business workflow | 1. Review `OpportunityStageRequirements.cs`<br/>2. Compare with PRD requirements<br/>3. Observe: Only 4 of 20+ fields validated<br/>4. Review test cases in GoNoGoDecision_PRD_TestCases.md<br/>5. Attempt to execute any DoA2 lookup test<br/>6. Observe: No implementation exists | All 20+ mandatory fields validated. DoA2 lookup works from EntityUserRole. All warnings and acknowledgments implemented. Custom rejection → NO GO works. All 102 test cases pass. | Only 4 fields validated. No DoA2 lookup. No warnings. Standard rejection behavior. 98 of 102 tests blocked. | 2026-02-02 | Open |

---

## Resolved Defects

_(No resolved defects yet)_

---

## Reclassified Items

The following items were previously logged as developer defects but have been reclassified to more appropriate categories:

### Moved to Technical Debt / Process Improvements

| Former ID | Title | Why It's Not a Defect | Recommendation |
|-----------|-------|----------------------|----------------|
| DEF-002 | Missing data-testid attributes on detail pages | Components work correctly. This is a **testability improvement**, not a defect. | Add to Definition of Done: "New components must include data-testid attributes" |
| DEF-003 | Missing data-testid attributes on forms | Components work correctly. This is a **testability improvement**, not a defect. | Add to Definition of Done: "New forms must include data-testid attributes" |

### Moved to Backlog (Tests Written for Unimplemented Features)

| Former ID | Title | Why It's Not a Defect | Recommendation |
|-----------|-------|----------------------|----------------|
| DEF-005 | Missing Model Namespaces (7 namespaces) | Tests were written **ahead of implementation**. Models don't exist because features aren't built yet. | Track as planned feature work in sprint backlog. Tests serve as specifications. |
| DEF-007 | IntegrationTests Out of Sync (4,675 errors) | Tests reference APIs that **were never implemented** or were changed. Test code is wrong, not production code. | Audit tests, delete obsolete ones, create backlog items for missing APIs if needed. |

---

## Defect Statistics

- **Total Open:** 1
- **Total Resolved:** 0
- **Total Reclassified:** 4 (moved to appropriate trackers)
- 🔴 **Critical:** 0
- 🟠 **High Priority:** 1 (DEF-008 - Go Decision feature incomplete)
- 🟡 **Medium Priority:** 0
- 🟢 **Low Priority:** 0

---

## Latest Test Results (2026-02-05)

### .NET C# Tests - Combined Summary

| Test Suite | Passed | Failed | Skipped | Total | Pass Rate |
|------------|--------|--------|---------|-------|-----------|
| **Business.Tests** | 2,725 | 2 | 273 | 3,000 | 90.8% |
| **FastTests** | 78 | 0 | 0 | 78 | 100% |
| **Presentation.Tests** | 29 | 0 | 0 | 29 | 100% |
| **Total** | **2,832** | **2** | **273** | **3,107** | **91.2%** |
| **Duration** | 10s | - | - | - | - |

### C# Test Failures (2 failures)

| Test | Category | Error | Root Cause | Action |
|------|----------|-------|------------|--------|
| `AccessibilityTests.A11Y010_Links_ShouldHaveDescriptiveText` | Accessibility | Expected <2 non-descriptive links, found 2 | Test stub - not fully implemented | QA to fix test (QA-026) |
| `PartnerByOrgUnitWithRelationsSpecificationTests.Criteria_FiltersPartnersByBothDirectAndIndirectRelations` | Specification | Expected 2 results, found 1 | Test data setup incomplete | QA to fix test (QA-027) |

**Note:** Both failures are test implementation issues (QA responsibility), not production defects.

### Playwright E2E Tests (2026-02-05)

| Metric | Count | Notes |
|--------|-------|-------|
| **Passed** | 265 | 59% pass rate |
| **Failed** | 76 | Unmocked API endpoints, test-specific issues |
| **Skipped** | 71 | Blocked tests (Go Decision, oUP, Login) |
| **Total** | 449 | - |
| **Duration** | ~30m | - |

**Note:** QA-028 (webServer not starting) has been resolved. Remaining failures are due to missing API mocks and test-specific issues.

---

## What Belongs in This List?

### ✅ Log as Developer Defect (DEF-XXX)

- Implemented feature doesn't match PRD/specification
- API returns incorrect data or status codes in production
- Business logic produces wrong results
- Security vulnerability in production code
- Performance issue in production (not test environment)
- Data corruption or loss in production

### ❌ Do NOT Log as Developer Defect

| Issue Type | Where to Track |
|------------|----------------|
| Test infrastructure issues | `Defect List for QA.md` |
| Tests fail due to test configuration | `Defect List for QA.md` |
| InMemory DB can't run raw SQL | `Defect List for QA.md` |
| Missing test attributes (data-testid) | Definition of Done / Tech Debt |
| Tests written for features not yet built | Sprint Backlog / Feature Requests |
| .NET/Angular framework bugs | External issue tracker (GitHub) |
| Test environment limitations | `Defect List for QA.md` |

---

## How to Use This Document

### For Developers:
1. Review during sprint planning for genuine bugs to fix
2. Update **Status** as work progresses
3. Move resolved defects to "Resolved Defects" section
4. Reference defect IDs in commits (e.g., "DEF-008: Implement DoA2 lookup")
5. **Challenge defects that aren't production issues** - help QA categorize correctly

### For QA Team:
1. **Before logging**: Ask "Is this a production code issue or a test issue?"
2. Use the "What Belongs in This List?" section as a guide
3. If uncertain, discuss with development team before logging
4. Cross-reference with `Defect List for QA.md` for test infrastructure issues

### For Project Managers:
1. This list should be short - most issues are test infrastructure or planned work
2. Use defect count as a quality metric for implemented features
3. Track reclassified items to understand categorization patterns
