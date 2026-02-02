# QA Test Dashboard

**Last Updated:** February 2, 2026  
**Sprint:** Current  
**Status:** ⚠️ Active Testing

---

## Executive Summary

| Metric | Value | Trend |
|--------|-------|-------|
| .NET Pass Rate | **94.6%** (2,247/2,374) | ✅ Stable |
| Playwright Pass Rate | **54%** (135/249) | ✅ ↑ (from 0%) |
| Tests Executed Today | 2,623 | ↑ |
| Tests Blocked | ~132 | → |
| QA-017 Status | ✅ RESOLVED | ↑ |
| Open Defects (Dev) | 8 | → |
| Open QA Issues | 9 | ↓ -1 |

---

## Test Execution Summary

### .NET Tests (Business.Tests)

| Category | Passed | Failed | Skipped | Total | Pass Rate |
|----------|--------|--------|---------|-------|-----------|
| **All Tests** | 2,246 | 46 | 82 | 2,374 | **94.6%** |
| Workflow Tests | 72 | 4 | 0 | 76 | 94.7% |
| Opportunity Tests | ~180 | ~38 | ~10 | ~228 | ~79% |
| Partner Tests | ~400 | ~5 | ~10 | ~415 | ~96% |
| Contact Tests | ~300 | ~3 | ~5 | ~308 | ~97% |

**Primary Failure Causes:**
- QA-009: Z.EntityFramework.Extensions InMemory DB (~38 tests)
- QA-010: AutoMapper EntityArtifactValueResolver (~5 tests)

### Playwright Tests (E2E) - Updated 2026-02-02

| Category | Tests | Status |
|----------|-------|--------|
| **Chromium Run** | 249 total | ✅ Completed |
| Passed | 135 (54%) | ✅ Working |
| Failed | 74 (30%) | ⚠️ Various issues |
| Skipped | 40 (16%) | ⏸️ Expected skips |
| oUP Integration | 34 | ⏸️ Blocked (QA-014) |
| Go Decision | 25 | ⏸️ Blocked (DEF-008) |

**✅ QA-017 Resolved:** webServer config auto-starts Angular on port 4200.
**Remaining failures:** DEF-001 (route guard), form validation issues, various test-specific problems.

---

## Test Coverage by Feature

| Feature | Unit Tests | Integration | E2E | Overall |
|---------|------------|-------------|-----|---------|
| Partners | 96% | 85% | 70% | **84%** |
| Contacts | 94% | 82% | 65% | **80%** |
| Interactions | 90% | 78% | 60% | **76%** |
| Opportunities | 88% | 75% | 55% | **73%** |
| Workflow | 94% | 70% | 40% | **68%** |
| Go Decision | 4% | 0% | 0% | **2%** |
| oUP Integration | 0% | 0% | 0% | **0%** |

---

## Blocking Issues

### Critical Blockers

| ID | Issue | Tests Blocked | Owner |
|----|-------|---------------|-------|
| DEF-001 | Route Guard | 29 | Dev |
| DEF-008 | Go Decision Feature | 98 | Dev |
| QA-009 | InMemory DB | 38-46 | QA/Dev |
| QA-014 | oUP Credentials | 34 | IT |

### High Priority

| ID | Issue | Tests Blocked | Owner |
|----|-------|---------------|-------|
| DEF-002 | data-testid (Views) | 50-90 | Dev |
| DEF-003 | data-testid (Forms) | 50-90 | Dev |
| QA-010 | AutoMapper DI | 5 | Dev |

---

## This Week's Progress

### Completed ✅

- [x] Created 102 Go Decision PRD test cases
- [x] Created Go Decision execution report
- [x] Created Dev team summary for DEF-008
- [x] Documented oUP credentials request
- [x] Analyzed QA-009 workaround options
- [x] Analyzed DEF-001 Route Guard
- [x] Created Go Decision Playwright tests (25 tests)
- [x] Ran .NET Business.Tests (2,374 tests)
- [x] Running Playwright tests (224 tests)

### In Progress ⏳

- [ ] Playwright test execution completing
- [ ] Comprehensive test report generation

### Blocked ❌

- [ ] oUP Integration testing (waiting for credentials)
- [ ] Go Decision automation (waiting for implementation)
- [ ] Phase 1B Playwright tests (waiting for data-testid)

---

## Test Artifacts Created Today

| Document | Purpose | Location |
|----------|---------|----------|
| GoNoGoDecision_PRD_TestCases.md | 102 manual test cases | Opportunity Tests/BusinessLogic/ |
| GoNoGoDecision_TestExecution_Report.md | Execution status | Opportunity Tests/BusinessLogic/ |
| DEF-008_GoDecision_DevTeam_Summary.md | Dev implementation guide | QA Tests/ |
| QA_Credentials_Request.md | IT credential request | QA Tests/ |
| QA-009_InMemoryDB_Workaround_Analysis.md | Technical analysis | QA Tests/ |
| DEF-001_RouteGuard_Analysis.md | Technical analysis | QA Tests/ |
| go-decision.spec.ts | Playwright tests | Playwright Tests/ |
| QA_Dashboard.md | This dashboard | QA Tests/ |

---

## Defect Summary

### Open Developer Defects (8)

| ID | Priority | Title | Tests Blocked |
|----|----------|-------|---------------|
| DEF-001 | Critical | Route Guard blocks access | 29 |
| DEF-002 | High | Missing data-testid (Views) | 50-90 |
| DEF-003 | High | Missing data-testid (Forms) | 50-90 |
| DEF-004 | Critical | AdvancedSearchService InMemory | 53 |
| DEF-005 | Critical | Missing Model Namespaces | 1,800 |
| DEF-006 | Medium | .NET 9 PipeWriter bug | Intermittent |
| DEF-007 | Critical | IntegrationTests out of sync | 4,675 errors |
| DEF-008 | High | Go Decision not implemented | 98 |

### Open QA Issues (9)

| ID | Priority | Title | Status |
|----|----------|-------|--------|
| QA-007 | High | Business Card Scanner signal | Open |
| QA-008 | High | PrimeNG DynamicDialog | Open |
| QA-009 | High | Z.EntityFramework InMemory | Open |
| QA-010 | High | AutoMapper DI | Open |
| QA-011 | Medium | 17 Playwright tests skipped | Open |
| QA-012 | Medium | 5 Business.Tests excluded | Open |
| QA-014 | High | oUP credentials needed | Open |
| QA-015 | Medium | oUP button prod-only | Open |
| QA-016 | High | Go Decision tests blocked | Open |

---

## Sprint Metrics

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| Test Pass Rate | 95% | 94.6% | 🟡 |
| Tests Created | 50 | 127 | ✅ |
| Blockers Resolved | 3 | 0 | 🔴 |
| Documentation | Complete | Complete | ✅ |

---

## Recommendations

### Immediate Actions

1. **🚨 Start Angular dev server before Playwright tests (QA-017)** - Unblocks 206 tests, 5 min effort
2. **Request oUP credentials** - Unblocks 34 tests
3. **Fix DEF-001 Route Guard** - Unblocks 29 tests, 3-5 hour effort
4. **Implement SQLite for tests** - Fixes QA-009, unblocks 38-46 tests

### Sprint Planning

1. **DEF-008 Go Decision** - Prioritize for implementation (102 test cases ready)
2. **DEF-002/003 data-testid** - Add attributes to enable Phase 1B tests
3. **DEF-005 Phase 2** - Create 9 manager classes to unblock 1,800 tests

---

## Next Update

- **Playwright results**: When current run completes
- **Full dashboard update**: Next sprint

---

*Dashboard maintained by QA Team*
