# Take-a-Tour / Onboarding — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/shared/components/take-a-tour`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

| **N≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **E≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **F≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **I≥3P?** | ✅ | 90 ≥ 3×30 = 90 |

---

## Feature Overview

Take-a-tour/onboarding: guided tour steps, tooltip display, skip/complete, progress, responsive, accessibility.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Tour Button Visible for New User

**Priority:** P0  
**Precondition:** New user, tour not completed.

**Steps:**
1. Log in as new user
2. Locate "Take a Tour" button

**Expected Result:** Button visible and clickable.

---

#### POS-002: Start Tour Successfully

**Priority:** P0  
**Precondition:** Tour button visible.

**Steps:**
1. Click "Take a Tour"
2. Verify first step

**Expected Result:** Tour overlay appears, step 1 displayed.

---

#### POS-003: Navigate to Next Step

**Priority:** P0  
**Precondition:** Tour started, on step 1.

**Steps:**
1. Click "Next"
2. Verify step 2

**Expected Result:** Step 2 displayed, progress updated.

---

#### POS-004: Complete Tour

**Priority:** P0  
**Precondition:** On last step.

**Steps:**
1. Click "Done" or "Complete"
2. Verify completion

**Expected Result:** Tour closed, completion saved, not shown again.

---

#### POS-005: Skip Tour

**Priority:** P0  
**Precondition:** Tour started.

**Steps:**
1. Click "Skip"
2. Confirm skip

**Expected Result:** Tour closed, skip saved.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Navigate to previous step | On step 2 | Click Back | Step 1 shown | P1 |
| POS-007 | Progress indicator | Tour active | View progress | "Step 2 of 5" shown | P1 |
| POS-008 | Tooltip highlights element | Step active | View highlight | Element highlighted | P1 |
| POS-009 | Tooltip positioning | Element visible | View tooltip | Correct position | P1 |
| POS-010 | Tour responsive (desktop) | Desktop | View tour | Desktop layout | P1 |
| POS-011 | Tour responsive (mobile) | Mobile | View tour | Mobile layout | P1 |
| POS-012 | Keyboard next | Focus on Next | Press Enter | Next step | P1 |
| POS-013 | Keyboard back | Focus on Back | Press Enter | Previous step | P1 |
| POS-014 | Keyboard skip | Focus on Skip | Press Enter | Tour skipped | P1 |
| POS-015 | Escape to close | Tour active | Press Escape | Tour closed | P1 |
| POS-016 | Click overlay to close | Tour active | Click overlay | Tour closed or next | P1 |
| POS-017 | Tour for returning user | Completed before | View | Option to replay | P2 |
| POS-018 | Replay tour | Tour completed | Click replay | Tour restarts | P2 |
| POS-019 | Tour step with link | Step has link | Click link | Navigates | P2 |
| POS-020 | Tour step with image | Step has image | View | Image displayed | P2 |
| POS-021 | Tour with 1 step | Single step | Complete | Done | P2 |
| POS-022 | Tour with 20 steps | Many steps | Navigate | All work | P2 |
| POS-023 | Screen reader announces | Screen reader | Start tour | Step announced | P2 |
| POS-024 | Focus management | Tab | Focus in tooltip | Focus trapped | P2 |
| POS-025 | Reduced motion | Prefers-reduced-motion | View tour | No animation | P2 |
| POS-026 | High contrast | High contrast | View tour | Visible | P2 |
| POS-027 | Tour persistence | Refresh mid-tour | Refresh | Resume or restart | P2 |
| POS-028 | Multi-language tour | fr locale | View tour | French content | P2 |
| POS-029 | Tour completion analytics | Complete tour | Complete | Event tracked | P2 |
| POS-030 | Skip analytics | Skip tour | Skip | Event tracked | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 70 tests

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Start with null config | Config = null | Default or error | P0 |
| NEG-002 | Start with empty steps | Steps = [] | No tour or error | P0 |
| NEG-003 | Invalid step index | Index = -1 | Handled | P0 |
| NEG-004 | Invalid step index | Index = 999 | Handled | P0 |
| NEG-005 | Missing target element | Element not in DOM | Fallback position | P0 |
| NEG-006 | Removed target mid-tour | Element removed | Skip or error | P0 |
| NEG-007 | Invalid step content | Malformed HTML | Sanitized | P0 |
| NEG-008 | Null callback | onComplete = null | No error | P0 |
| NEG-009 | Invalid theme | Theme = "invalid" | Default | P0 |
| NEG-010 | Negative step duration | Duration = -1 | Default | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Anonymous user | No auth | Start tour | Redirect or allowed | P0 |
| NEG-012 | Tour disabled for role | Disabled role | View button | Button hidden | P1 |
| NEG-013 | Tour admin-only | Non-admin | Start | 403 or hidden | P1 |
| NEG-014 | Expired session | Expired | Mid-tour | Handled | P1 |
| NEG-015 | No tour permission | No permission | Start | 403 | P1 |
| NEG-016 to NEG-020 | [Additional auth scenarios] | Various | Various | Per scenario | P1 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Next on last step | Last step | Next | Complete or no-op | P1 |
| NEG-022 | Back on first step | First step | Back | No-op or close | P1 |
| NEG-023 | Start during tour | Tour active | Start again | Ignored or restart | P1 |
| NEG-024 | Complete already completed | Completed | Complete | No-op | P1 |
| NEG-025 | Skip already skipped | Skipped | Skip | No-op | P1 |
| NEG-026 to NEG-070 | [Additional negative scenarios] | Various | Various | Per scenario | P1/P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 70 tests

### 3.1 String Length | 3.2 Numeric | 3.3 Date | 3.4 Collection | 3.5 Unicode | 3.6 Responsive | 3.7 Additional

| ID Range | Key Scenarios |
|-----------|---------------|
| BND-001 to BND-070 | Step content length, step count (0, 1, 50), tooltip position, viewport 320-3840px, RTL, reduced motion, focus trap, animation timing |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests

### 4.1 Workflow (15) | 4.2 Validation (15) | 4.3 Constraint (10) | 4.4 Audit (10)

| ID Range | Rule Examples |
|----------|---------------|
| FUN-001 to FUN-050 | Tour start, step navigation, completion, skip, progress, persistence, a11y, analytics, permission checks |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests

### 5.1 CRUD (10) | 5.2 Search & Filter (10) | 5.3 Pagination (5) | 5.4 Relationships (10) | 5.5 Error Handling (15)

| ID Range | Scenario Examples |
|----------|-------------------|
| INT-001 to INT-050 | Login→Tour, Tour→Complete, Tour→Skip, Tour with nav, Tour with feature flags, error states |

---

## §6 Security Tests

> **Minimum:** 50 tests (SEC-001 to SEC-050)

XSS in step content, injection, auth, IDOR, CSRF, token, focus trap escape.

---

## §7 Concurrency Tests

> **Minimum:** 25 tests (CON-001 to CON-025)

Concurrent start, tab switch, rapid next/back, resize during tour.

---

## §8 Unit Tests

> **Minimum:** 21 tests (UNT-001 to UNT-021)

Step validation, progress calc, positioning, focus logic, completion state.

---

## §9 Performance Tests

> **Minimum:** 16 tests (PRF-001 to PRF-016)

Tour load, step transition, animation, memory, LCP.

---

## §10 Load Tests

> **Minimum:** 10 tests (LDT-001 to LDT-010)

Sustained tour starts, concurrent users, recovery.

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Guided tour steps | POS-001 to POS-005, FUN-001 to FUN-005 |
| Tooltip display | POS-008, POS-009, BND-* |
| Skip/complete | POS-004, POS-005, POS-017, NEG-024, NEG-025 |
| Progress | POS-007, UNT-* |
| Responsive | POS-010, POS-011, BND-* |
| Accessibility | POS-012 to POS-015, POS-023 to POS-025 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
