# [JIRA-ID]: [Feature Name] — Test Cases

**JIRA Reference:** [JIRA-ID](https://unops.atlassian.net/browse/[JIRA-ID])  
**Created:** YYYY-MM-DD  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | P | 30-50 | ⬜ |
| Negative Tests | §2 | N | Max(50, 3×P) | ⬜ |
| Boundary Tests | §3 | B | Max(50, 3×P) | ⬜ |
| Functional Tests | §4 | F | Max(50, 3×P) | ⬜ |
| Integration Tests | §5 | I | Max(50, 3×P) | ⬜ |
| Security Tests | §6 | S | ≥50 | ⬜ |
| Concurrency Tests | §7 | C | ≥25 | ⬜ |
| Unit Tests | §8 | U | ≥21 | ⬜ |
| Performance Tests | §9 | Pf | ≥16 | ⬜ |
| Load Tests | §10 | L | ≥10 | ⬜ |
| **TOTAL** | | **T** | **≥462** | ⬜ |

**Ratio Compliance:** N≥3P: ___≥___ ⬜ | E≥3P: ___≥___ ⬜ | F≥3P: ___≥___ ⬜ | I≥3P: ___≥___ ⬜

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: [Test Name]

**Priority:** P0  
**Precondition:** [Setup required]

**Steps:**
1. [Step 1]
2. [Step 2]

**Expected Result:** [What should happen]

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-0XX | [Name] | [Setup] | [Steps] | [Result] | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** Max(50, 2×P) tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | [Name] | [Input] | [Error message/code] | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-0XX | [Name] | [Role] | [Action] | Access Denied / 403 | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-0XX | [Name] | [State] | [Action] | [Error/Block] | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-0XX | [Name] | [Field] | [Validation msg] | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-0XX | [Name] | [e.g., DB timeout] | [Graceful handling] | P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** Max(50, 2×P) tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | [Field] | [0] | [255] | ✅ Accept | ✅ Accept | ❌ Reject | P1 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-0XX | [Field] | [0] | [999999] | ✅ | ❌ | ❌ | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-0XX | [Name] | [e.g., Feb 29 leap year] | [Result] | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-0XX | [Name] | [Empty/Single/Max] | [Result] | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-0XX | [Field] | [Arabic/Chinese/Emoji] | [Accept/Display correctly] | P2 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow rules (15), Validation rules (15), Constraint rules (10), Audit rules (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule Description | Trigger | Expected Outcome | Priority |
|----|-----------|-----------------|---------|-----------------|----------|
| FUN-001 | [Name] | [Rule] | [Trigger] | [Outcome] | P0 |

### 4.2 Validation Rules (15)

| ID | Test Name | Validation Rule | Valid Input | Invalid Input | Priority |
|----|-----------|----------------|------------|--------------|----------|
| FUN-0XX | [Name] | [Rule] | [Valid] | [Invalid] | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-0XX | [Name] | [Constraint] | [Input] | [Result] | P1 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-0XX | [Name] | [Action] | [Audit log entry] | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests | **Breakdown:** CRUD workflow (10), Search/filter (10), Pagination (5), Relationships (10), Error handling (15)

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|------------------|-----------------|----------|
| INT-001 | [Name] | [CRUD] | [Entities] | [Result] | P0 |

### 5.2 Search & Filter (10)

| ID | Test Name | Search/Filter Criteria | Expected Results | Priority |
|----|-----------|----------------------|-----------------|----------|
| INT-0XX | [Name] | [Criteria] | [Results] | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| INT-0XX | [Name] | [Page] | [Result] | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Test Scenario | Expected Result | Priority |
|----|-----------|-------------|--------------|-----------------|----------|
| INT-0XX | [Name] | [Rel] | [Scenario] | [Result] | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error Condition | Expected Response | Priority |
|----|-----------|----------------|------------------|----------|
| INT-0XX | [Name] | [Error] | [Response code + msg] | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests | **Coverage:** OWASP Top 10, injection, authorization, IDOR, mass assignment

### 6.1 Injection Prevention (10)

| ID | Test Name | Attack Vector | Target Field | Expected Block | Priority |
|----|-----------|--------------|-------------|---------------|----------|
| SEC-001 | [SQL injection in name] | `'; DROP TABLE--` | [Field] | Sanitized/Rejected | P0 |

### 6.2 Broken Access Control (10)

| ID | Test Name | User Role | Unauthorized Action | Expected Result | Priority |
|----|-----------|-----------|-------------------|-----------------|----------|
| SEC-0XX | [Name] | [Role] | [Action] | 403 Forbidden | P0 |

### 6.3 IDOR (Insecure Direct Object Reference) (10)

| ID | Test Name | Object | Manipulation | Expected Result | Priority |
|----|-----------|--------|-------------|-----------------|----------|
| SEC-0XX | [Name] | [Entity ID] | [Change ID to another user's] | 403 / Not Found | P0 |

### 6.4 Mass Assignment (5)

| ID | Test Name | Protected Field | Manipulation | Expected Result | Priority |
|----|-----------|----------------|-------------|-----------------|----------|
| SEC-0XX | [Name] | [Field] | [Include in request body] | Field not modified | P1 |

### 6.5 Authentication & Session (10)

| ID | Test Name | Attack Scenario | Expected Protection | Priority |
|----|-----------|----------------|-------------------|----------|
| SEC-0XX | [Name] | [Scenario] | [Protection] | P0 |

### 6.6 Data Exposure (5)

| ID | Test Name | Sensitive Data | Exposure Risk | Expected Protection | Priority |
|----|-----------|---------------|--------------|-------------------|----------|
| SEC-0XX | [Name] | [Data] | [Risk] | [Protection] | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests | **Coverage:** Race conditions, deadlocks, double submit, transaction isolation, cache poisoning

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | [Name] | [2 users update same entity] | [Optimistic concurrency / last-write-wins] | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests | **Breakdown:** Validation (5), Formatting (3), Calculations (5), Status logic (5), Collections (3)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | [Name] | Validation | [Input] | [Output] | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests | **Breakdown:** Single ops (2), Bulk ops (3), Search (5), Concurrent access (3), Memory (3)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | [Name] | [Operation] | [< X ms] | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests | **Breakdown:** Sustained load (3), Spike load (2), Stress limits (3), Recovery (2)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | [Name] | [Profile] | [Duration] | [Criteria] | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| [AC-1: Description] | POS-001, NEG-005, BND-010 |

---

## Test Environment Setup

**Prerequisites:**
- [List required setup]

---

**Last Updated:** YYYY-MM-DD  
**Status:** [Draft / Ready for Execution / In Progress]
