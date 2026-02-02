# Blocker Resolution Tracker

**Last Updated:** February 2, 2026  
**Purpose:** Track status and resolution progress of all test execution blockers

---

## Executive Summary

| Category | Count | Tests Blocked |
|----------|-------|---------------|
| **Critical (Immediate Action)** | 2 | 235 |
| **High Priority (This Sprint)** | 4 | ~280 |
| **Medium Priority (Next Sprint)** | 3 | ~4,700 |
| **Total Unique Tests Blocked** | - | ~550* |

*Some tests are blocked by multiple issues

---

## Critical Blockers (Immediate Action Required)

### QA-017: Angular Dev Server Not Running - ✅ RESOLVED
| Attribute | Value |
|-----------|-------|
| **Status** | ✅ RESOLVED (2026-02-02) |
| **Owner** | QA Team |
| **Tests Blocked** | ~~206~~ → 0 (webServer auto-starts Angular) |
| **Effort** | Automated via playwright.config.ts |
| **Resolution** | webServer config in playwright.config.ts auto-starts Angular on port 4200 |

**Resolution Details:**
- Playwright's `webServer` config automatically starts `ng serve --port 4200 --host 127.0.0.1`
- No manual intervention needed
- Re-run results: **135 passed, 74 failed, 40 skipped** (Chromium only)
- Remaining failures are test-specific issues (DEF-001, etc.), not server connectivity

**Previous Action Required (no longer needed):**
```bash
# Terminal 1: Start Angular dev server
cd UNOPS.PAO.ClientApp
ng serve

# Wait for "Compiled successfully"
# Terminal 2: Run Playwright tests
cd "QA Tests/Playwright Tests"
npx playwright test
```

**Alternative (Recommended for CI):**
Configure tests to run against deployed environment by setting `BASE_URL` in `.env`

---

### DEF-001: Route Permission Guard Blocks Detail Pages
| Attribute | Value |
|-----------|-------|
| **Status** | 🔴 OPEN - HIGH |
| **Owner** | Development Team |
| **Tests Blocked** | 29 Playwright tests |
| **Effort** | 3-5 hours |
| **Analysis Document** | `DEF-001_RouteGuard_Analysis.md` |

**Investigation Required:**
1. Review `route-permission.guard.ts`
2. Check permission service response format
3. Compare API response with guard expectations

---

## High Priority Blockers (This Sprint)

### QA-014: oUP Integration Credentials Missing
| Attribute | Value |
|-----------|-------|
| **Status** | 🟡 WAITING - External |
| **Owner** | IT Team |
| **Tests Blocked** | 34 Playwright tests |
| **Effort** | 0 (waiting for credentials) |
| **Request Document** | `QA_Credentials_Request.md` |

**Credentials Needed:**
- [ ] `OUP_BASE_URL` - oUP test environment URL
- [ ] `OUP_USERNAME` / `OUP_PASSWORD` - Test user credentials
- [ ] `EMAIL_HOST` / credentials - Email testing
- [ ] `OPP_MANAGER_EMAIL`, `DOA2_EMAIL`, `BD_EMAIL` - Role accounts

---

### DEF-008: Go Decision Feature Incomplete
| Attribute | Value |
|-----------|-------|
| **Status** | 🟡 OPEN - Awaiting Implementation |
| **Owner** | Development Team |
| **Tests Blocked** | 98 manual test cases, 25 Playwright tests |
| **Effort** | 32-46 hours (full implementation) |
| **Dev Summary** | `DEF-008_GoDecision_DevTeam_Summary.md` |
| **Test Cases** | `GoNoGoDecision_PRD_TestCases.md` |

**Implementation Phases:**
1. [ ] Phase 1: Field Validations (8-12 hrs)
2. [ ] Phase 2: DoA2 Lookup (4-6 hrs)
3. [ ] Phase 3: Warnings (6-8 hrs)
4. [ ] Phase 4: Workflow (8-12 hrs)
5. [ ] Phase 5: Notifications (6-8 hrs)

---

### QA-009: InMemory Database Limitation
| Attribute | Value |
|-----------|-------|
| **Status** | 🟡 OPEN - Workaround Available |
| **Owner** | QA Team / Dev Team |
| **Tests Blocked** | ~38 .NET tests |
| **Effort** | 6-7 hours |
| **Analysis Document** | `QA-009_InMemoryDB_Workaround_Analysis.md` |

**Recommended Fix:** Implement SQLite for tests

---

### QA-010: AutoMapper DI Issue
| Attribute | Value |
|-----------|-------|
| **Status** | 🟡 OPEN - Investigation Needed |
| **Owner** | Dev Team |
| **Tests Blocked** | ~5 .NET tests |
| **Effort** | 2-4 hours |

**Root Cause:** `EntityArtifactValueResolver` requires DI container

---

## Medium Priority Blockers (Next Sprint)

### DEF-007: IntegrationTests Out of Sync
| Attribute | Value |
|-----------|-------|
| **Status** | 🟠 OPEN - Major Effort |
| **Owner** | Development Team |
| **Tests Blocked** | ~4,000 integration tests (4,675 errors) |
| **Effort** | 20-40 hours |

**Issue:** Integration test code references outdated API signatures

---

### DEF-002 & DEF-003: Missing data-testid Attributes
| Attribute | Value |
|-----------|-------|
| **Status** | 🟠 OPEN |
| **Owner** | Development Team |
| **Tests Blocked** | 50-90 Playwright tests (Phase 1B) |
| **Effort** | 12-22 hours |

**Components Needing data-testid:**
- DEF-002: 4 view components
- DEF-003: 12 form components

---

### DEF-005: Missing Manager Classes
| Attribute | Value |
|-----------|-------|
| **Status** | 🟠 OPEN |
| **Owner** | Development Team |
| **Tests Blocked** | ~1,800 integration tests |
| **Effort** | 20-40 hours |

**Missing Managers:** 9 managers need creation

---

## Resolution Timeline

### Week 1 (Immediate)
| Day | Action | Owner | Unblocks |
|-----|--------|-------|----------|
| 1 | Start Angular dev server (QA-017) | QA | 206 tests |
| 1 | Submit credentials request (QA-014) | QA | - |
| 2-3 | Fix DEF-001 Route Guard | Dev | 29 tests |
| 4-5 | Implement SQLite fix (QA-009) | QA/Dev | 38 tests |

### Week 2
| Day | Action | Owner | Unblocks |
|-----|--------|-------|----------|
| 1-2 | Receive oUP credentials (QA-014) | IT | 34 tests |
| 2-5 | Begin DEF-008 Phase 1 (Field Validation) | Dev | Partial |

### Week 3+
- DEF-008 Phases 2-5
- DEF-002/003 data-testid attributes
- DEF-007 IntegrationTests reconciliation

---

## Quick Win Summary

| Fix | Effort | Tests Unblocked | ROI | Status |
|-----|--------|-----------------|-----|--------|
| ~~QA-017 (Start Angular)~~ | ~~5 min~~ | ~~206~~ | ~~⭐⭐⭐⭐⭐~~ | ✅ DONE |
| DEF-001 (Route Guard) | 3-5 hrs | 29 | ⭐⭐⭐⭐ | 🔴 Open |
| QA-009 (SQLite) | 6-7 hrs | 38 | ⭐⭐⭐ | 🔴 Open |
| QA-014 (Credentials) | 0 hrs | 34 | ⭐⭐⭐⭐⭐ | 🟡 Pending |

---

## Status Legend

| Symbol | Meaning |
|--------|---------|
| 🔴 | Critical - Blocking majority of tests |
| 🟡 | High - Significant impact |
| 🟠 | Medium - Can be deferred |
| 🟢 | Low - Minor impact |
| ✅ | Resolved |

---

## Next Review

- **Daily:** Check QA-017 (dev server) status
- **Weekly:** Review all blocker progress
- **Sprint:** Update resolution timeline

---

*Tracker maintained by QA Team*
