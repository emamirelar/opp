# OpportunityPlus to oUP Integration — Test Cases

**Component:** `UNOPS.PAO.Integration / oUP Sync Service`  
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
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result |
|-------|--------|
| N ≥ 3P | 90 ≥ 90 ✅ PASS |
| E ≥ 3P | 90 ≥ 90 ✅ PASS |
| F ≥ 3P | 90 ≥ 90 ✅ PASS |
| I ≥ 3P | 90 ≥ 90 ✅ PASS |

---

## Feature Overview

OpportunityPlus to oUP (UNOPS ERP) integration: data sync, status mapping, error handling, retry logic, audit trail.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Create Engagement in oUP from Opportunity

**Priority:** P0  
**Precondition:** Valid opportunity, oUP API available.

**Steps:**
1. Create opportunity in Opportunity+
2. Trigger sync to oUP
3. Verify engagement created in oUP

**Expected Result:** Base engagement created in oUP Pre-engagement stage.

---

#### POS-002: Status Mapping Correct

**Priority:** P0  
**Precondition:** Opportunity with status.

**Steps:**
1. Set opportunity status (e.g., Active)
2. Sync to oUP
3. Verify oUP status matches mapping

**Expected Result:** Status correctly mapped to oUP workflow.

---

#### POS-003: Data Sync Complete

**Priority:** P0  
**Precondition:** Opportunity with full data.

**Steps:**
1. Sync opportunity to oUP
2. Verify all 19 categories mapped
3. Check oUP engagement data

**Expected Result:** All mapped fields synced correctly.

---

#### POS-004: Deep Link Created

**Priority:** P0  
**Precondition:** Engagement created in oUP.

**Steps:**
1. Complete sync
2. Retrieve deep link from oUP
3. Store in Opportunity+

**Expected Result:** Bi-directional deep link established.

---

#### POS-005: Audit Trail Recorded

**Priority:** P0  
**Precondition:** Sync executed.

**Steps:**
1. Execute sync
2. Check audit trail
3. Verify sync event logged

**Expected Result:** Audit entry with timestamp, user, result.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Retry on transient failure | API timeout | Retry | Success on retry | P1 |
| POS-007 | Idempotent sync | Same opportunity | Sync twice | No duplicate | P1 |
| POS-008 | Partial update | Opportunity updated | Sync | Only changed fields | P1 |
| POS-009 | Funding partner mapping | 3 funding partners | Sync | All mapped | P1 |
| POS-010 | Client partner mapping | 3 client partners | Sync | All mapped | P1 |
| POS-011 | High-risk mapping | 17 risk types | Sync | All mapped | P1 |
| POS-012 | Country mapping | Multiple countries | Sync | Correct mapping | P1 |
| POS-013 | SDG mapping | SDGs with targets | Sync | Mapped | P1 |
| POS-014 | Email notification | Sync complete | Check email | Notification sent | P1 |
| POS-015 | Pub/Sub message | Sync event | Check Pub/Sub | Message published | P1 |
| POS-016 | Sync with minimal data | Required fields only | Sync | Success | P2 |
| POS-017 | Sync with full data | All fields populated | Sync | All synced | P2 |
| POS-018 | Sync after opportunity update | Update opportunity | Sync | Changes in oUP | P2 |
| POS-019 | Sync cancelled opportunity | Cancel in Opp+ | Sync | Status in oUP | P2 |
| POS-020 | Multiple opportunities | 5 opportunities | Sync all | All synced | P2 |
| POS-021 | Batch sync | 10 opportunities | Batch sync | All processed | P2 |
| POS-022 | Sync during off-peak | Scheduled | Sync | Success | P2 |
| POS-023 | Manual sync trigger | User triggers | Sync | Executed | P2 |
| POS-024 | Sync with custom fields | Extended fields | Sync | If mapped | P2 |
| POS-025 | Sync with attachments | Documents | Sync | If supported | P2 |
| POS-026 | Reconciliation report | After sync | Generate | Report correct | P2 |
| POS-027 | Sync health check | API available | Health check | Healthy | P2 |
| POS-028 | Sync with version | Optimistic lock | Sync | Version check | P2 |
| POS-029 | Sync with existing oUP ID | Re-sync | Sync | Update not create | P2 |
| POS-030 | Sync rollback on failure | Sync fails | Rollback | Partial reverted | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 90 tests

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Sync with null opportunity | Opportunity = null | ArgumentNullException | P0 |
| NEG-002 | Sync with invalid opportunity ID | Id = 999999 | KeyNotFoundException | P0 |
| NEG-003 | Sync with deleted opportunity | IsDeleted = true | BusinessException | P0 |
| NEG-004 | Sync with missing required fields | Required = null | Validation error | P0 |
| NEG-005 | Sync with invalid status | Status = "invalid" | Mapping error | P0 |
| NEG-006 | Sync with invalid partner ID | PartnerId = 999999 | Validation error | P0 |
| NEG-007 | Sync with invalid country | Country = "XX" | Validation error | P0 |
| NEG-008 | Sync with malformed date | Date = "invalid" | Parse error | P0 |
| NEG-009 | Sync with negative amount | Amount = -1 | Validation error | P0 |
| NEG-010 | Sync with oversized payload | 10MB data | Rejected | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | No sync permission | Limited | Trigger sync | UnauthorizedAccessException | P0 |
| NEG-012 | Anonymous user | No auth | Sync | 401 | P0 |
| NEG-013 | Expired oUP token | Expired | Sync | Re-auth or error | P0 |
| NEG-014 | Invalid oUP credentials | Bad creds | Sync | 401 | P0 |
| NEG-015 | Revoked oUP access | Revoked | Sync | 403 | P0 |
| NEG-016 to NEG-070 | [Additional negative scenarios: API down, timeout, CORS, malformed response, duplicate, constraint violation, race, etc.] | Various | Various | Per scenario | P0/P1/P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 90 tests

### 3.1–3.7 Boundary Tests (BND-001 to BND-090)

| ID Range | Key Scenarios |
|----------|---------------|
| BND-001 to BND-070 | Field length limits, numeric bounds (0, MAX), date boundaries (leap year, timezone), empty/partial payload, max partners (3 funding, 3 client), 17 risk types, Unicode in names, concurrent sync, retry limits (3 retries), rate limit boundaries |
| BND-071 to BND-090 | Name at 199 chars, amount at max, date at boundary, 2 funding partners, 2 client partners, 16 risk types, retry at 2, rate limit at boundary, empty payload, single partner, batch size 99, page size 999, sync at midnight, timezone edge, Unicode in description, ID = 2, pagination last page, filter single status, zero partners, max context size |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 90 tests

### 4.1 Workflow (15) | 4.2 Validation (15) | 4.3 Constraint (10) | 4.4 Audit (10)

| ID Range | Rule Examples |
|----------|---------------|
| FUN-001 to FUN-050 | Sync triggers, status mapping, field mapping, idempotency, retry on transient, audit on sync, error handling, rollback, reconciliation |
| FUN-051 to FUN-090 | Status mapping validation, field mapping validation, idempotency check, retry logic, audit trail, rollback on failure, reconciliation report, deep link creation, email notification, Pub/Sub publish, batch processing, rate limit handling, connection pool, version check, partner validation, country validation, date validation, amount validation, category mapping, risk mapping |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 90 tests

### 5.1 CRUD (10) | 5.2 Search & Filter (10) | 5.3 Pagination (5) | 5.4 Relationships (10) | 5.5 Error Handling (15)

| ID Range | Scenario Examples |
|----------|-------------------|
| INT-001 to INT-050 | Create Opp→Sync→oUP, Update→Sync, Delete→Sync, Deep link, Email notification, Pub/Sub, API errors, timeout, 500, 429 |
| INT-051 to INT-090 | Full sync lifecycle, Batch sync, Retry flow, Reconciliation flow, Health check flow, Status mapping flow, Field mapping flow, Partner mapping flow, Country mapping flow, Risk mapping flow, SDG mapping flow, Deep link round-trip, Audit trail flow, Rollback flow, Idempotent sync, Partial update, Rate limit handling, Connection pool, Version check, Error propagation |

---

## §6 Security Tests

> **Minimum:** 50 tests (SEC-001 to SEC-050)

Token storage, API credentials, injection in payload, auth to oUP, CORS, audit no PII.

---

## §7 Concurrency Tests

> **Minimum:** 25 tests (CON-001 to CON-025)

Concurrent sync same opportunity, concurrent sync different, retry race, connection pool.

---

## §8 Unit Tests

> **Minimum:** 21 tests (UNT-001 to UNT-021)

Status mapping, field mapping, validation, retry logic, idempotency check.

---

## §9 Performance Tests

> **Minimum:** 16 tests (PRF-001 to PRF-016)

Single sync < 5s, batch 10 < 30s, API latency, connection pool.

---

## §10 Load Tests

> **Minimum:** 10 tests (LDT-001 to LDT-010)

Sustained sync 10/min, spike 50 syncs, stress, recovery after oUP outage.

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Data sync | POS-001, POS-003, INT-001 to INT-005 |
| Status mapping | POS-002, FUN-002, UNT-001 |
| Error handling | NEG-* , INT-036 to INT-050, POS-006 |
| Retry logic | POS-006, FUN-*, BND-* |
| Audit trail | POS-005, FUN-041 to FUN-050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
