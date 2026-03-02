# The Go Decision Feature — Comprehensive Test Plan

**Feature:** The Go Decision (DoA2 Decision-Maker Experience)  
**PRD:** `tasks/the-go-decision/the-go-decision-prd.md`  
**Prerequisite Feature:** Send Opportunity for Go Decision (`tasks/send-opportunity-for-go-decision/`)  
**Created:** 2026-02-17  
**Author:** QA Team  
**Status:** Ready for Execution  
**Priority:** CRITICAL — New feature from `development` merge

---

## 1. Scope

This test plan covers **Phase 2: The Go Decision** — the decision-maker (DoA Level 2) experience after an opportunity has been submitted for Go decision. This is distinct from the "Send for Go Decision" flow (Phase 1) which is covered by `PNO-969_GoDecision_TestCases.md`.

### In Scope

| Area | Description |
|------|-------------|
| **In-System Notifications** | Actions Required card on homepage, notification bell integration |
| **Decision-Maker Review UI** | Instructional guidance, highlighted info panel (initiative type, time to signing, DD status, high risks, sender remarks) |
| **Go Decision Workflow** | Confirmation statement with org unit details, mandatory decision rationale, mandatory Executive assignment |
| **No-Go Decision Workflow** | Confirmation statement, mandatory decision rationale |
| **Post-Decision Immutability** | Record becomes read-only artifact after Go, No-Go, or Cancel |
| **Email Notifications (CC)** | CC recipients: OM, initiator, Director/Manager of responsible org unit |
| **DOA Level 3 Fallback (PNO-1197)** | Fallback to DOA Level 3 when DOA Level 2 is not present on responsible OrgUnit |
| **Executive Assignment** | New `ExecutiveId` field, dropdown from EntityUserRole table, mandatory on Go |
| **Custom Dialogs** | Approve, Reject, Recall custom dialogs replacing generic workflow dialogs |
| **Locked Editing** | `canUpdate` permission flag enforcement during and after workflow |
| **Workflow History** | Display of workflow actions with timestamps and users |

### Out of Scope

| Area | Reason |
|------|--------|
| Send for Go Decision flow | Covered by `PNO-969_GoDecision_TestCases.md` |
| PDF generation | Separate future PRD per Q3 clarification |
| Security penetration testing | Handled by Infrastructure/Security team |

---

## 2. Test Environment

### Prerequisites

| Requirement | Details |
|-------------|---------|
| **Application** | Deployed and running (local or staging) |
| **Database** | PostgreSQL with latest migrations applied (including `AddExecutiveIdToOpportunity`, `AddAuditTrackingForOpportunity`) |
| **Workflow Submodule** | Initialized and accessible (or conditional compilation guards active) |
| **Email Service** | Configured and accessible for notification verification |

### Test Users

| User | Role | Purpose |
|------|------|---------|
| **User A** | Opportunity Manager (OM) | Creates and submits opportunities |
| **User B** | DoA2 (EA DOA Level 2) | Makes Go/No-Go decisions |
| **User C** | DoA3 (EA DOA Level 3) | Fallback decision-maker when DoA2 absent |
| **User D** | Director/Manager | CC recipient, Executive candidate |
| **User E** | Collaborator (assigned) | Can edit content but cannot perform workflow actions |
| **User F** | Unauthenticated / no role | Negative access testing |

### Test Data

| Data | Details |
|------|---------|
| **Opportunity** | In I&P/Draft stage with all mandatory fields populated |
| **Org Unit** | With DoA2 and Director/Manager assigned via EntityUserRole |
| **Org Unit (no DoA2)** | For PNO-1197 DOA3 fallback testing |
| **Partners** | With varied DD statuses (Pending, Approved, Expired, Expiring Soon) |
| **Risks** | Including high organizational risks (PreDefinedHighRiskId not null) |

---

## 3. Related Test Documentation

| Document | Coverage |
|----------|----------|
| `QA Tests/Opportunity Tests/BusinessLogic/PNO-969_GoDecision_TestCases.md` | Send for Go Decision (Phase 1) — 397 tests |
| `QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_PRD_TestCases.md` | PRD-specific requirements — 397 tests |
| `QA Tests/Opportunity Tests/BusinessLogic/OpportunityWorkflowStatus_TestCases.md` | Stage/status transitions — 397 tests |
| `tasks/the-go-decision/e2e-test-plan.md` | Manual E2E walkthrough plan |
| `QA Tests/Opportunity Tests/BusinessLogic/TheGoDecision_TestCases.md` | **NEW** — This plan's test cases (created alongside this plan) |
| `QA Tests/Opportunity Tests/BusinessLogic/DOA3Fallback_TestCases.md` | **NEW** — PNO-1197 DOA3 fallback test cases |
| `QA Tests/Playwright Tests/go-decision.spec.ts` | Existing Playwright E2E tests |

---

## 4. Test Categories and Counts

### Pre-Implementation Ratio Calculation

```
Planned Positive Tests: P = 30

Core category minimums (each ≥ 3 × P = 90):
- Negative: 90
- Edge/Boundary: 90
- Functional: 90
- Integration: 90

Additional category minimums:
- Unit: 21 (FIXED)
- Concurrency: 25 (FIXED)
- Performance: 16 (FIXED)
- Load: 10 (FIXED)
- (Security: OUT OF SCOPE for QA)

Individual ratio checks (each must pass):
- N≥3P: 90 ≥ 90 ✅
- E≥3P: 90 ≥ 90 ✅
- F≥3P: 90 ≥ 90 ✅
- I≥3P: 90 ≥ 90 ✅
```

### Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | ≥30 | ✅ |
| 2 | Negative Tests | §2 | 90 | ≥90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | ≥90 | ✅ |
| 4 | Functional Tests | §4 | 90 | ≥90 | ✅ |
| 5 | Integration Tests | §5 | 90 | ≥90 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## 5. Test Suites

### Suite 1: The Go Decision — Decision-Maker Experience

**Test Cases Document:** `QA Tests/Opportunity Tests/BusinessLogic/TheGoDecision_TestCases.md`

**C# Test Files Location:** `QA Tests/Integration Tests/TheGoDecision/`

| File | Category | Count |
|------|----------|-------|
| `PositiveTests.cs` | §1 Positive | 30 |
| `NegativeTests.cs` | §2 Negative | 90 |
| `BoundaryTests.cs` | §3 Boundary/Edge | 90 |
| `FunctionalTests.cs` | §4 Functional | 90 |
| `IntegrationTests.cs` | §5 Integration | 90 |
| `ConcurrencyTests.cs` | §7 Concurrency | 25 |
| `UnitTests.cs` | §8 Unit | 21 |
| `PerformanceTests.cs` | §9 Performance | 16 |
| `LoadTests.cs` | §10 Load | 10 |

### Suite 2: DOA Level 3 Fallback (PNO-1197)

**Test Cases Document:** `QA Tests/Opportunity Tests/BusinessLogic/DOA3Fallback_TestCases.md`

**C# Test Files Location:** `QA Tests/Integration Tests/PNO-1197_DoA3Fallback/` (already exists)

---

## 6. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Workflow submodule unavailable in CI | High | High | Conditional compilation (`WORKFLOW_AVAILABLE`) already in place |
| DoA2/DoA3 test data not seeded | Medium | High | Create seed scripts for EntityUserRole entries |
| Email service unavailable in test env | Medium | Medium | Mock email service; verify DB notification records |
| Concurrent approval by multiple DoA holders | Low | Critical | Concurrency tests verify single-decision enforcement |
| Executive dropdown empty (no Director/Manager) | Medium | High | Boundary tests cover empty EntityUserRole scenarios |
| Post-decision immutability bypass via API | Low | Critical | Integration tests verify API-level enforcement |

---

## 7. Execution Strategy

### Phase 1: Unit & Functional Tests (No external dependencies)
1. Unit tests for validation logic, formatting, status calculations
2. Functional tests for workflow rules, field validation, audit rules
3. Can run in CI without Workflow submodule (conditional compilation)

### Phase 2: Integration Tests (Requires database)
1. CRUD workflow tests
2. Relationship tests (Opportunity → Executive, Opportunity → DoA)
3. Search and filter by workflow status

### Phase 3: Concurrency & Performance (Requires test environment)
1. Concurrent Go/No-Go decisions
2. Double-submit prevention
3. Performance baselines for decision operations

### Phase 4: E2E / Playwright Tests (Requires running application)
1. Full decision-maker flow
2. Notification integration
3. Immutability enforcement through UI

---

## 8. Entry / Exit Criteria

### Entry Criteria
- [ ] All "Send for Go Decision" (Phase 1) tests passing
- [ ] Database migrations applied (ExecutiveId, AuditTracking)
- [ ] Test users created with correct roles
- [ ] Test opportunities in correct stages

### Exit Criteria
- [ ] All 372 test cases executed
- [ ] Pass rate ≥ 95%
- [ ] Zero critical/high defects open
- [ ] Ratio compliance verified (N≥3P, E≥3P, F≥3P, I≥3P)
- [ ] All 9 mandatory test files present
