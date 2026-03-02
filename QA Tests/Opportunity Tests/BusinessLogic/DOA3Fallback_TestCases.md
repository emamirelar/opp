# PNO-1197: DOA Level 3 Fallback — Comprehensive Test Cases

**JIRA Reference:** [PNO-1197](https://unops.atlassian.net/browse/PNO-1197) — Logic to fallback to DOA Level 3 when DOA Level 2 is not present on Responsible OrgUnit  
**Epic:** The Go/No Go Decision  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)  
**C# Test Files:** `QA Tests/Integration Tests/PNO-1197_DoA3Fallback/`

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30 = 90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30 = 90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30 = 90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30 = 90 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Feature Overview

When an Opportunity is submitted for Go decision, the system identifies the decision-maker (DoA = Delegation of Authority) by searching the `EntityUserRole` table for the responsible OrgUnit. The standard lookup targets **DoA Level 2** (EA DOA2). **PNO-1197** adds fallback logic: when no DoA Level 2 holder exists for the responsible OrgUnit, the system falls back to **DoA Level 3** (EA DOA3).

### Lookup Priority

```
1. Search for DoA Level 2 for responsible OrgUnit
   ├── Found → Use DoA Level 2 as decision-maker
   └── Not Found → 
       2. Search for DoA Level 3 for responsible OrgUnit
          ├── Found → Use DoA Level 3 as decision-maker
          └── Not Found → Error: "No decision authority found"
```

### Key Design Decisions

| Decision | Details |
|----------|---------|
| DoA2 takes priority | If both DoA2 and DoA3 exist, DoA2 is used |
| DoA3 is fallback only | DoA3 only used when DoA2 is completely absent |
| Same workflow for both | DoA3 has identical approval/reject capabilities as DoA2 |
| Notifications adapt | Emails/notifications sent to whoever is the identified decision-maker |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| **DoA2 present → DoA2 used** | POS-001–010, FUN-001–005 |
| **DoA2 absent, DoA3 present → DoA3 used** | POS-011–020, FUN-006–010 |
| **Both absent → Error** | NEG-001–010, FUN-011–015 |
| **DoA2 disabled → Fallback to DoA3** | POS-021–025, NEG-011–020, BND-001–010 |
| **OrgUnit hierarchy lookup** | BND-011–030, INT-001–015 |
| **Notification routing** | POS-026–030, FUN-016–025 |
| **Concurrency** | CON-001–025 |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### DoA2 Present (Happy Path)

POS-001: Submit with DoA2 present → Succeeds, DoA2 identified.  
POS-002: DoA2 holder correctly identified from EntityUserRole.  
POS-003: DoA2 receives email notification on submission.  
POS-004: DoA2 sees Actions Required card entry.  
POS-005: DoA2 notification bell shows pending decision.  
POS-006: DoA2 can approve (Go decision) → Stage → GO.  
POS-007: DoA2 can reject (No-Go decision) → Stage → NO GO.  
POS-008: Multiple DoA2 holders → All notified, first to decide wins.  
POS-009: DoA2 from correct OrgUnit only → Other OrgUnit DoA2 not involved.  
POS-010: DoA2 decision recorded in workflow history.

### DoA3 Fallback (PNO-1197 Core)

POS-011: No DoA2 for OrgUnit → DoA3 identified as decision-maker.  
POS-012: DoA3 receives email notification when fallback triggered.  
POS-013: DoA3 sees Actions Required card entry.  
POS-014: DoA3 notification bell shows pending decision.  
POS-015: DoA3 can approve (Go decision) → Stage → GO.  
POS-016: DoA3 can reject (No-Go decision) → Stage → NO GO.  
POS-017: DoA3 fallback logged in workflow audit trail.  
POS-018: DoA3 decision has identical workflow behavior to DoA2.  
POS-019: DoA3 fallback with Executive assignment → Works.  
POS-020: DoA3 fallback email CC includes OM and Director.

### Disabled DoA2 → DoA3 Fallback

POS-021: DoA2 exists but disabled → Fallback to DoA3.  
POS-022: DoA2 exists but deleted (soft-deleted) → Fallback to DoA3.  
POS-023: DoA2 exists but expired account → Fallback to DoA3.  
POS-024: All DoA2 holders disabled, active DoA3 → DoA3 used.  
POS-025: Disabled DoA2, multiple DoA3 holders → All DoA3 notified.

### Notification Routing

POS-026: Email sent to DoA2 when DoA2 present (standard path).  
POS-027: Email sent to DoA3 when fallback (DoA3 used).  
POS-028: In-system notification to DoA3 on fallback.  
POS-029: Actions Required shows for DoA3 on fallback.  
POS-030: Email CC list correct for DoA3 fallback (OM, Director).

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### No DoA Holder At All (NEG-001–015)

NEG-001: No DoA2 and no DoA3 for OrgUnit → Error "No decision authority found".  
NEG-002: No DoA for correct OrgUnit (DoA exists for different OrgUnit) → Error.  
NEG-003: DoA lookup with null OrgUnit → Error handled.  
NEG-004: DoA lookup with invalid OrgUnit ID (0) → Error handled.  
NEG-005: DoA lookup with non-existent OrgUnit ID → Error handled.  
NEG-006: DoA lookup with soft-deleted OrgUnit → Error handled.  
NEG-007: All DoA2 and DoA3 holders disabled → Error "No active decision authority".  
NEG-008: All DoA2 and DoA3 holders soft-deleted → Error.  
NEG-009: DoA holder exists but for wrong entity type → Not matched.  
NEG-010: DoA holder exists but role revoked before submission → Error.  
NEG-011: DoA2 disabled, DoA3 disabled → Error "No active decision authority".  
NEG-012: DoA2 deleted, DoA3 deleted → Error.  
NEG-013: DoA2 expired, DoA3 expired → Error.  
NEG-014: DoA holder with null UserId in EntityUserRole → Skipped gracefully.  
NEG-015: DoA holder with UserId pointing to non-existent user → Error handled.

### Authorization Failures (NEG-016–030)

NEG-016: DoA3 attempts decision when DoA2 is active → Access Denied.  
NEG-017: Non-DoA user attempts decision → Access Denied.  
NEG-018: OM attempts to approve (not DoA) → Access Denied.  
NEG-019: Collaborator attempts decision → Access Denied.  
NEG-020: DoA3 from wrong OrgUnit attempts decision → Access Denied.  
NEG-021: DoA2 from parent OrgUnit (not responsible OrgUnit) → Access Denied.  
NEG-022: DoA3 from child OrgUnit (not responsible OrgUnit) → Access Denied.  
NEG-023: User with DoA2 role on different entity → Access Denied.  
NEG-024: Expired token for DoA3 holder → 401 Unauthorized.  
NEG-025: Disabled DoA3 attempts decision → Account disabled error.  
NEG-026: DoA holder attempts decision on opportunity in wrong stage → Invalid state.  
NEG-027: DoA holder attempts decision on cancelled opportunity → Invalid state.  
NEG-028: DoA holder attempts second decision (already decided) → Invalid state.  
NEG-029: DoA holder attempts decision on soft-deleted opportunity → 404.  
NEG-030: API call with forged DoA role claim → Authorization handler rejects.

### Data Integrity Failures (NEG-031–045)

NEG-031: Submit with OrgUnit that has orphaned DoA role → Graceful error.  
NEG-032: Submit when EntityUserRole table is empty → Error.  
NEG-033: DoA lookup query fails (DB error) → Transaction rolled back.  
NEG-034: DoA lookup returns duplicate entries → Deduplicated.  
NEG-035: DoA holder with null email → Decision succeeds, email skipped with log.  
NEG-036: DoA holder with invalid email format → Decision succeeds, email fails gracefully.  
NEG-037: Concurrent EntityUserRole update during DoA lookup → Consistent result.  
NEG-038: DoA role added after submission (late role assignment) → Not retroactive.  
NEG-039: DoA role removed after submission, before decision → Decision by already-notified DoA still valid.  
NEG-040: OrgUnit reassignment during workflow → Uses original OrgUnit.  
NEG-041: DoA2 re-enabled after DoA3 was notified → DoA3 still valid decision-maker.  
NEG-042: Multiple simultaneous DoA role changes → Final state consistent.  
NEG-043: EntityUserRole FK violation → Graceful error.  
NEG-044: DoA lookup with circular OrgUnit hierarchy → No infinite loop.  
NEG-045: DoA lookup timeout → Error with retry suggestion.

### Fallback Logic Failures (NEG-046–060)

NEG-046: Fallback to DoA3 when DoA2 exists but is for wrong role type → DoA2 still checked first.  
NEG-047: Fallback logic skips DoA4, DoA5 (only DoA2 and DoA3 supported).  
NEG-048: Fallback with DoA3 having IsDeleted=true → Not selected.  
NEG-049: Fallback with DoA3 having inactive status → Not selected.  
NEG-050: Fallback triggered but DoA3 role recently expired → Error.  
NEG-051: Fallback with OrgUnit merge/split → Uses current OrgUnit assignment.  
NEG-052: Fallback when responsible OrgUnit changed mid-workflow → Uses OrgUnit at submission time.  
NEG-053: DoA2 re-assigned to OrgUnit after fallback → Current decision-maker unchanged.  
NEG-054: DoA3 removed during decision review → Decision still valid if already loaded.  
NEG-055: Fallback logic with null DoA level value → Skipped.  
NEG-056: Fallback with DoA level = "" (empty string) → Skipped.  
NEG-057: Fallback with DoA level = "Level 4" (unknown) → Not used.  
NEG-058: Fallback with case mismatch "doa2" vs "DoA2" → Case-insensitive match.  
NEG-059: Fallback with whitespace in DoA level name → Trimmed and matched.  
NEG-060: Fallback query exceeds connection pool → Queued, not failed.

### Notification Failures on Fallback (NEG-061–070)

NEG-061: Email to DoA3 fails → Decision queued, notification retry.  
NEG-062: DoA3 has no email configured → Decision succeeds, logged as warning.  
NEG-063: Email template references DoA2 specific text when DoA3 is recipient → Template adapts.  
NEG-064: Notification to DoA3 when notification service is down → Queued.  
NEG-065: Actions Required card for DoA3 fails to render → Graceful degradation.  
NEG-066: Duplicate fallback notifications → Deduplicated.  
NEG-067: Notification for DoA3 when DoA3 has notifications disabled → Respected.  
NEG-068: Email CC includes DoA2 holder when DoA3 is decision-maker → DoA2 NOT in CC (not applicable).  
NEG-069: Recall notification sent to DoA3 when original DoA was DoA2 → Uses current decision-maker.  
NEG-070: Fallback notification after system restart → Notifications still delivered.

### Extended Negative Tests (NEG-071–090)

NEG-071: Submit with opportunity in Draft status (not yet submitted) → DoA lookup not triggered.  
NEG-072: DoA lookup with EntityUserRole.EntityType null → Skipped, fallback error.  
NEG-073: DoA lookup with EntityUserRole.EntityType mismatch (Partner vs Opportunity) → Not matched.  
NEG-074: DoA3 holder with WorkflowStatus = Archived → Not selected for fallback.  
NEG-075: DoA lookup with OrgUnit having IsDeleted=true → Error before lookup.  
NEG-076: Submit with opportunity having null ResponsibleOrgUnitId → Error before DoA lookup.  
NEG-077: DoA holder with LastModifiedDate > CreatedDate (modified role) → Still included if valid.  
NEG-078: DoA lookup with malformed EntityUserRole JSON (if extended) → Graceful error.  
NEG-079: DoA3 receives decision request for opportunity in different OrgUnit → Access Denied.  
NEG-080: Submit with opportunity in GO status (already decided) → No DoA lookup, error.  
NEG-081: DoA lookup with DbContext disposed before lookup completes → ObjectDisposedException handled.  
NEG-082: DoA lookup with read-only replica lag → Uses primary for consistency.  
NEG-083: DoA role with EffectiveDate in future → Not included in lookup.  
NEG-084: DoA role with ExpiryDate in past → Not included in lookup.  
NEG-085: DoA lookup when User table has no matching record → Skipped.  
NEG-086: DoA role with OrgUnitId pointing to soft-deleted OrgUnit → Error.  
NEG-087: Submit with duplicate opportunity ID → Error before DoA lookup.  
NEG-088: DoA lookup with SQL injection attempt in OrgUnit filter → Parameterized query safe.  
NEG-089: DoA3 receives notification for wrong opportunity (ID mix-up) → Correct opportunity verified.  
NEG-090: Fallback when DoA2 exists but User.IsDeleted=true → DoA2 treated as absent, fallback to DoA3.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### DoA Level Boundaries (BND-001–020)

BND-001: Exactly 1 DoA2 holder → Used directly.  
BND-002: Exactly 0 DoA2, 1 DoA3 → DoA3 used.  
BND-003: Exactly 0 DoA2, 0 DoA3 → Error.  
BND-004: 1 active DoA2, 1 active DoA3 → DoA2 used (priority).  
BND-005: 1 disabled DoA2, 1 active DoA3 → DoA3 used (fallback).  
BND-006: 1 active DoA2, 1 disabled DoA3 → DoA2 used.  
BND-007: 5 DoA2 holders → All 5 notified.  
BND-008: 50 DoA2 holders → All notified, performance acceptable.  
BND-009: 0 DoA2, 5 DoA3 holders → All 5 DoA3 notified.  
BND-010: 0 DoA2, 50 DoA3 holders → All notified, performance acceptable.  
BND-011: DoA2 holder with exactly minimum required fields → Works.  
BND-012: DoA2 holder with all optional fields null → Works.  
BND-013: DoA3 holder with exactly minimum required fields → Works.  
BND-014: DoA3 holder with all optional fields null → Works.  
BND-015: DoA role assignment effective date = today → Included.  
BND-016: DoA role assignment effective date = tomorrow → Not included.  
BND-017: DoA role assignment effective date = yesterday → Included.  
BND-018: DoA role expiry date = today → Included or excluded (edge).  
BND-019: DoA role expiry date = yesterday → Excluded.  
BND-020: DoA role with no expiry date → Perpetual, included.

### OrgUnit Hierarchy Boundaries (BND-021–040)

BND-021: OrgUnit at root level → DoA lookup works.  
BND-022: OrgUnit at depth 1 → DoA lookup works.  
BND-023: OrgUnit at depth 10 (deep nesting) → DoA lookup works.  
BND-024: OrgUnit with 0 children → DoA lookup for this unit only.  
BND-025: OrgUnit with 100 children → DoA lookup for this unit only.  
BND-026: OrgUnit with name at max length (255 chars) → No issues.  
BND-027: OrgUnit with Unicode name → No issues.  
BND-028: OrgUnit with special characters in code → No issues.  
BND-029: Responsible OrgUnit changed to one with DoA2 → DoA2 used on next submit.  
BND-030: Responsible OrgUnit changed to one without DoA2 → DoA3 fallback on next submit.  
BND-031: OrgUnit with DoA2 who is also DoA3 → DoA2 role takes priority.  
BND-032: Same user assigned DoA2 for one OrgUnit and DoA3 for another → Correct role per OrgUnit.  
BND-033: OrgUnit ID = 1 (minimum valid) → Lookup works.  
BND-034: OrgUnit ID = MAX_INT → Lookup works if exists.  
BND-035: OrgUnit with exactly 1 EntityUserRole entry → Found.  
BND-036: OrgUnit with 1000 EntityUserRole entries → Performance acceptable.  
BND-037: OrgUnit with mixed active/inactive roles → Only active filtered.  
BND-038: OrgUnit reassigned during fallback lookup → Consistent result.  
BND-039: Cross-OrgUnit DoA role (if supported) → Correctly scoped.  
BND-040: OrgUnit with DoA2 having IsDeleted=true, DoA3 active → DoA3 used.

### Concurrent Decision Boundaries (BND-041–055)

BND-041: DoA2 and DoA3 both attempt decision simultaneously → One succeeds.  
BND-042: Two DoA3 holders decide simultaneously → First wins.  
BND-043: Decision at exact DB transaction isolation boundary → Consistent.  
BND-044: Fallback lookup during EntityUserRole table update → Consistent.  
BND-045: Decision during OrgUnit table update → Uses snapshot at lookup time.  
BND-046: Rapid DoA2 enable/disable toggling during lookup → Final state used.  
BND-047: Concurrent submissions for same opportunity → One succeeds.  
BND-048: DoA3 decides while DoA2 is being re-enabled → DoA3 decision valid.  
BND-049: 10 concurrent DoA lookups for different opportunities → All resolve.  
BND-050: 100 concurrent DoA lookups for same OrgUnit → Cached or handled.  
BND-051: DoA lookup with DB connection at pool limit → Queued, not failed.  
BND-052: Fallback triggered at exact same time as DoA2 role creation → Consistent.  
BND-053: Decision timeout at exactly configured limit → Clear timeout error.  
BND-054: Notification delivery at rate limit boundary → Queued, not lost.  
BND-055: Audit trail write at high-concurrency boundary → All entries persisted.

### Data Type Boundaries (BND-056–070)

BND-056: EntityUserRole with DoA level string "2" vs "Level 2" → Matched correctly.  
BND-057: EntityUserRole with DoA level string "3" vs "Level 3" → Matched correctly.  
BND-058: EntityUserRole with DoA level int vs string → Handled.  
BND-059: EntityUserRole with null DoA level → Skipped.  
BND-060: EntityUserRole with empty DoA level → Skipped.  
BND-061: UserId = 0 in EntityUserRole → Skipped.  
BND-062: UserId = negative in EntityUserRole → Skipped.  
BND-063: OrgUnitId = 0 on opportunity → Error before DoA lookup.  
BND-064: OrgUnitId = null on opportunity → Error before DoA lookup.  
BND-065: Opportunity with responsible OrgUnit FK pointing to deleted OrgUnit → Error.  
BND-066: EntityUserRole created after opportunity but before submission → Found.  
BND-067: EntityUserRole with future effective date → Not included.  
BND-068: EntityUserRole with past expiry date → Not included.  
BND-069: Mixed active/deleted DoA holders → Only active selected.  
BND-070: DoA lookup with maximum EntityUserRole records (10K) → Performance < 1s.

### Extended Boundary Tests (BND-071–090)

BND-071: DoA level string "EA DOA2" vs "DoA Level 2" → Matched per configuration.  
BND-072: DoA level string "EA DOA3" vs "DoA Level 3" → Matched per configuration.  
BND-073: DoA lookup with EntityUserRole count = 0 for OrgUnit → Fallback to DoA3 or error.  
BND-074: DoA lookup with EntityUserRole count = 1 (DoA3 only) → DoA3 used.  
BND-075: Opportunity stage boundary: Draft → Submit → DoA lookup triggered.  
BND-076: Opportunity stage boundary: Pending Decision → Decision allowed.  
BND-077: Opportunity stage boundary: GO → Decision blocked.  
BND-078: Opportunity stage boundary: NO GO → Decision blocked.  
BND-079: EffectiveDate = submission timestamp minus 1 second → Included.  
BND-080: EffectiveDate = submission timestamp plus 1 second → Excluded.  
BND-081: ExpiryDate = submission timestamp minus 1 second → Excluded.  
BND-082: ExpiryDate = submission timestamp plus 1 second → Included.  
BND-083: DoA lookup with OrgUnit having ParentId = null (root) → Works.  
BND-084: DoA lookup with OrgUnit having ParentId = self (invalid) → Error or skipped.  
BND-085: OrgUnit hierarchy depth = 0 (no parent) → DoA lookup for this unit only.  
BND-086: OrgUnit hierarchy depth = 20 (max expected) → DoA lookup works.  
BND-087: Notification recipient list length = 100 → All handled.  
BND-088: Email CC list length = 50 → All handled.  
BND-089: Workflow history entries = 1000 for opportunity → DoA level queryable.  
BND-090: DoA lookup with EntityUserRole.EntityId = 0 → Skipped or error.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Workflow Rules (FUN-001–015)

FUN-001: DoA2 present → DoA2 used as decision-maker.  
FUN-002: DoA2 absent → Fallback to DoA3.  
FUN-003: Fallback logged with reason "DoA Level 2 not found for OrgUnit".  
FUN-004: DoA2 disabled → Treated as absent, fallback triggered.  
FUN-005: DoA2 soft-deleted → Treated as absent, fallback triggered.  
FUN-006: DoA3 found on fallback → Used as decision-maker.  
FUN-007: DoA3 has full approval capability (Go decision).  
FUN-008: DoA3 has full rejection capability (No-Go decision).  
FUN-009: DoA3 receives same notification format as DoA2.  
FUN-010: DoA3 decision recorded with DoA level in audit.  
FUN-011: Neither DoA2 nor DoA3 → Submission blocked with error.  
FUN-012: Error message specifies "No decision authority" for OrgUnit.  
FUN-013: Error prevents workflow state change.  
FUN-014: Error logged with OrgUnit ID for debugging.  
FUN-015: Submission rollback on DoA lookup failure (no partial state).

### Validation Rules (FUN-016–030)

FUN-016: DoA lookup queries only responsible OrgUnit.  
FUN-017: DoA lookup filters by IsDeleted = false.  
FUN-018: DoA lookup filters by active user status.  
FUN-019: DoA lookup orders by DoA level (Level 2 first, then Level 3).  
FUN-020: Multiple DoA2 holders → All receive notification.  
FUN-021: Multiple DoA3 holders (fallback) → All receive notification.  
FUN-022: First DoA holder to decide → Decision recorded.  
FUN-023: Subsequent attempts by other DoA holders → Rejected (already decided).  
FUN-024: Email notification adapts recipient list based on DoA level used.  
FUN-025: In-system notification adapts based on DoA level used.  
FUN-026: Actions Required card shows for all relevant DoA holders.  
FUN-027: After decision, Actions Required cleared for all DoA holders.  
FUN-028: Workflow history records which DoA level made decision.  
FUN-029: Executive dropdown available for DoA3 on Go decision.  
FUN-030: Executive candidates same for DoA2 and DoA3 (from OrgUnit).

### Constraint Rules (FUN-031–040)

FUN-031: DoA3 cannot override DoA2 decision (if DoA2 decided first).  
FUN-032: DoA2 cannot override DoA3 decision (if DoA3 decided first via fallback).  
FUN-033: Fallback only checks DoA3, not DoA4 or higher.  
FUN-034: Fallback does not check parent OrgUnit DoA holders.  
FUN-035: Fallback does not check child OrgUnit DoA holders.  
FUN-036: DoA lookup result cached for duration of submission request.  
FUN-037: DoA re-lookup on resubmit (after recall) → Fresh lookup.  
FUN-038: OrgUnit change on opportunity → Next submit uses new OrgUnit's DoA.  
FUN-039: DoA holder with multiple OrgUnits → Matched by responsible OrgUnit.  
FUN-040: DoA role type must match exactly (not substring match).

### Audit Rules (FUN-041–050)

FUN-041: Audit trail records DoA level used (2 or 3).  
FUN-042: Audit trail records fallback reason (if DoA3 used).  
FUN-043: Audit trail records OrgUnit queried.  
FUN-044: Audit trail records number of DoA holders found.  
FUN-045: Audit trail records all notified DoA holders.  
FUN-046: Audit trail records decision-maker user ID.  
FUN-047: Audit trail records decision timestamp.  
FUN-048: Audit trail preserves data even if DoA role later removed.  
FUN-049: Audit queryable by DoA level filter.  
FUN-050: Audit queryable by OrgUnit filter.

### Extended Functional Tests (FUN-051–090)

FUN-051: DoA lookup excludes EntityUserRole with EntityType ≠ Opportunity.  
FUN-052: DoA lookup uses EntityId for OrgUnit scoping when applicable.  
FUN-053: Fallback reason message includes OrgUnit name for audit.  
FUN-054: DoA3 fallback does not trigger DoA2 notification retry.  
FUN-055: Recall action clears DoA decision-maker from pending state.  
FUN-056: Resubmit after recall triggers fresh DoA lookup.  
FUN-057: DoA2 added after DoA3 was notified → DoA3 still valid decision-maker.  
FUN-058: DoA role EffectiveDate/ExpiryDate checked at submission time.  
FUN-059: DoA lookup uses AsNoTracking for read-only performance.  
FUN-060: DoA lookup does not modify EntityUserRole records.  
FUN-061: Notification template uses DoA level variable for personalization.  
FUN-062: Actions Required card shows DoA level for decision-maker.  
FUN-063: Workflow history shows "DoA Level 2" or "DoA Level 3" in display.  
FUN-064: No-Go decision from DoA3 closes opportunity with correct status.  
FUN-065: Go decision from DoA3 requires Executive assignment (if configured).  
FUN-066: DoA lookup with DbContextFactory creates isolated context.  
FUN-067: DoA lookup timeout returns clear error message.  
FUN-068: DoA lookup retry on transient DB failure (if configured).  
FUN-069: Email CC list excludes DoA holders (they are in TO).  
FUN-070: In-app notification bell count includes DoA3 fallback decisions.  
FUN-071: Opportunity list filter by "DoA decision-maker" includes DoA3.  
FUN-072: Opportunity detail shows "Approved by [Name] (DoA Level 3)".  
FUN-073: Fallback rate metric: DoA3 decisions / total decisions.  
FUN-074: OrgUnit report: OrgUnits without DoA2 (fallback candidates).  
FUN-075: DoA lookup with OrgUnit having IsDeleted=false only.  
FUN-076: DoA lookup with User having Status=Active only.  
FUN-077: DoA lookup joins User table for email verification.  
FUN-078: DoA lookup joins OrgUnit table for name display.  
FUN-079: Notification service receives DoA holder list from lookup.  
FUN-080: Workflow engine receives DoA holder list from lookup.  
FUN-081: DoA lookup result used for Actions Required card population.  
FUN-082: DoA lookup result used for notification bell count.  
FUN-083: DoA lookup with split query strategy (no Cartesian product).  
FUN-084: DoA lookup batch query for multiple opportunities (if supported).  
FUN-085: DoA lookup parallel execution for independent requests.  
FUN-086: DoA lookup connection pool usage within limits.  
FUN-087: DoA lookup transaction scope appropriate for read-only.  
FUN-088: DoA lookup does not block other operations.  
FUN-089: DoA lookup result serializable for caching.  
FUN-090: DoA lookup idempotent for same OrgUnit + timestamp.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### DoA Lookup Integration (INT-001–015)

INT-001: DoA2 lookup queries EntityUserRole table correctly.  
INT-002: DoA3 fallback queries EntityUserRole table correctly.  
INT-003: Lookup joins with User table to verify active status.  
INT-004: Lookup joins with OrgUnit table for correct scoping.  
INT-005: Lookup returns user email for notification.  
INT-006: Lookup returns user name for display.  
INT-007: Lookup handles DB connection retry.  
INT-008: Lookup timeout configured and enforced.  
INT-009: Lookup result used by notification service.  
INT-010: Lookup result used by workflow engine.  
INT-011: Lookup result cached in request scope.  
INT-012: Cache invalidated on next request.  
INT-013: Lookup with AsNoTracking for read-only performance.  
INT-014: Lookup filters soft-deleted EntityUserRole records.  
INT-015: Lookup filters IsDeleted=true users.

### Workflow + DoA Integration (INT-016–030)

INT-016: Submit → DoA2 lookup → Notification → Actions Required → Go decision → Stage change.  
INT-017: Submit → DoA3 fallback → Notification → Actions Required → Go decision → Stage change.  
INT-018: Submit → No DoA → Error → No stage change → No notification sent.  
INT-019: Submit → DoA2 → Recall → Resubmit → Same DoA2 (if still assigned).  
INT-020: Submit → DoA3 → Recall → DoA2 added → Resubmit → DoA2 used.  
INT-021: Go decision → Email to DoA + CC (OM, Director) → Workflow history.  
INT-022: No-Go decision → Email to DoA + CC → Workflow history → Status Closed.  
INT-023: Cancel → No DoA involved → OM action only.  
INT-024: Reopen → No DoA involved → OM action only.  
INT-025: Full cycle: Submit → DoA3 Go → Verify executive → Verify immutability.  
INT-026: Full cycle: Submit → DoA3 No-Go → Verify status closed → Reopen.  
INT-027: Multiple opportunities, same OrgUnit → Same DoA holders for each.  
INT-028: Multiple opportunities, different OrgUnits → Different DoA holders.  
INT-029: DoA lookup + parallel opportunity submissions → Independent lookups.  
INT-030: DoA role change between two opportunities → Each uses role at submission time.

### Error Recovery (INT-031–040)

INT-031: DB error during DoA lookup → Submission fails, no partial state.  
INT-032: Network timeout during notification to DoA3 → Decision not blocked.  
INT-033: Email service error for DoA3 → Decision succeeds, email queued.  
INT-034: Concurrent DoA role deletion during workflow → Decision by already-notified DoA valid.  
INT-035: Connection pool exhaustion during DoA lookup → Queued, retried.  
INT-036: Transaction deadlock during DoA lookup + role update → Retry.  
INT-037: Partial notification failure (email succeeds, in-app fails) → Both tracked.  
INT-038: DbContextFactory creates isolated context for DoA lookup.  
INT-039: DoA lookup under high DB load → Timeout config respected.  
INT-040: Audit trail write failure → Decision still persisted, audit retried.

### Search & Reporting (INT-041–050)

INT-041: Filter opportunities by DoA level of decision-maker → Correct results.  
INT-042: Report: decisions made by DoA3 fallback → List available.  
INT-043: Report: OrgUnits without DoA2 → List for admin action.  
INT-044: Search workflow history by DoA level → Filterable.  
INT-045: Dashboard metric: fallback rate (DoA3/total decisions) → Calculable.  
INT-046: Export decisions with DoA level column → Included.  
INT-047: API: GET pending decisions by DoA level → Correct results.  
INT-048: API: GET decision history includes DoA level → Field present.  
INT-049: Opportunity list shows decision-maker name → DoA2 or DoA3 name.  
INT-050: Opportunity detail shows "Approved by DoA Level X" → Correct level.

### Extended Integration Tests (INT-051–090)

INT-051: EntityUserRole → User → OrgUnit full join chain → Correct DoA resolution.  
INT-052: Opportunity → ResponsibleOrgUnit → EntityUserRole lookup → End-to-end.  
INT-053: DoA lookup → NotificationService → Email queue → Delivery.  
INT-054: DoA lookup → WorkflowEngine → StageChange → Audit trail.  
INT-055: DoA lookup → ActionsRequiredService → Card render → UI.  
INT-056: DoA lookup → NotificationBellService → Badge count → UI.  
INT-057: Submit API → OpportunityManager → DoA lookup → Response.  
INT-058: Approve API → AuthorizationService → DoA verification → Decision.  
INT-059: Recall API → WorkflowManager → DoA clear → Resubmit ready.  
INT-060: Resubmit API → Fresh DoA lookup → New decision-maker.  
INT-061: DoA lookup with EntityUserRole soft-delete filter → Excluded.  
INT-062: DoA lookup with User soft-delete filter → Excluded.  
INT-063: DoA lookup with OrgUnit soft-delete filter → Error before lookup.  
INT-064: DoA lookup with EntityUserRole.EffectiveDate filter → Correct.  
INT-065: DoA lookup with EntityUserRole.ExpiryDate filter → Correct.  
INT-066: DoA lookup with EntityUserRole.EntityType = Opportunity → Scoped.  
INT-067: DoA lookup with EntityUserRole.EntityId = OrgUnit → Scoped.  
INT-068: DoA lookup result → Notification template → Recipient substitution.  
INT-069: DoA lookup result → Notification template → CC list substitution.  
INT-070: DoA lookup result → Workflow history → DoA level field.  
INT-071: DoA lookup result → Audit trail → Decision-maker ID.  
INT-072: DoA lookup result → Audit trail → Fallback reason.  
INT-073: DoA lookup + PermissionService → DoA2/DoA3 authorization.  
INT-074: DoA lookup + UserResolverService → Current user context.  
INT-075: DoA lookup + ConfigurationService → Timeout, retry settings.  
INT-076: DoA lookup + LoggingService → Log entries for fallback.  
INT-077: DoA lookup + IMapper → Entity to model mapping.  
INT-078: DoA lookup + IManagerWrapper → Cross-manager dependencies.  
INT-079: Submit → Opportunity → Partner → OrgUnit chain → DoA.  
INT-080: DoA decision → Opportunity → Stage → Status propagation.  
INT-081: DoA decision → Opportunity → Executive assignment (Go).  
INT-082: DoA decision → Opportunity → Closed status (No-Go).  
INT-083: DoA lookup + parallel DbContextFactory → Isolation.  
INT-084: DoA lookup + connection pool → No exhaustion.  
INT-085: DoA lookup + transaction scope → Read consistency.  
INT-086: DoA lookup + cache (if any) → Invalidation on role change.  
INT-087: DoA lookup + API gateway → Request routing.  
INT-088: DoA lookup + authentication middleware → User context.  
INT-089: DoA lookup + health check → Dependency reporting.  
INT-090: DoA lookup + telemetry → Metrics for fallback rate.

---

## §6 Security Tests — OUT OF SCOPE

> Security testing is handled by the Infrastructure and Security teams per project policy.

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: DoA2 and DoA3 both attempt decision → DoA2 priority, or first-wins if fallback.  
CON-002: Two DoA3 holders decide simultaneously → First wins.  
CON-003: DoA lookup during EntityUserRole UPDATE → Read consistency.  
CON-004: DoA role INSERT during active lookup → Not included (snapshot).  
CON-005: DoA role DELETE during active lookup → Still found (snapshot).  
CON-006: Concurrent submissions for two opportunities → Independent DoA lookups.  
CON-007: DoA2 disable + fallback trigger race → Consistent decision-maker.  
CON-008: Parallel notification sends → No duplicates.  
CON-009: Concurrent workflow history writes → All persisted.  
CON-010: Decision + recall at same millisecond → One succeeds.  
CON-011: DbContextFactory isolation for parallel DoA lookups.  
CON-012: Connection pool sharing across concurrent lookups.  
CON-013: Email queue concurrent access for DoA notifications.  
CON-014: Actions Required concurrent update → Correct count.  
CON-015: Notification bell concurrent update → Correct badge count.  
CON-016: OrgUnit reassignment during DoA lookup → Snapshot consistency.  
CON-017: Multiple rapid resubmissions → Each has fresh DoA lookup.  
CON-018: Concurrent audit trail writes from different decisions.  
CON-019: DoA3 decision during DoA2 re-enablement → DoA3 decision valid.  
CON-020: Parallel DoA lookups for same OrgUnit → Both succeed.  
CON-021: Transaction isolation level appropriate for DoA lookup.  
CON-022: Optimistic concurrency on Opportunity update during decision.  
CON-023: Concurrent Executive selection by parallel DoA holders → First wins.  
CON-024: Decision finalization race with notification delivery.  
CON-025: Cache consistency across parallel DoA queries.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (UNT-001–005)

UNT-001: GetDoAHolder returns DoA2 when both DoA2 and DoA3 exist.  
UNT-002: GetDoAHolder returns DoA3 when only DoA3 exists.  
UNT-003: GetDoAHolder returns null when neither exists.  
UNT-004: GetDoAHolder filters by correct OrgUnit ID.  
UNT-005: GetDoAHolder filters by IsDeleted = false.

### Formatting (UNT-006–008)

UNT-006: FormatDoANotification includes DoA level in subject.  
UNT-007: FormatDoANotification includes OrgUnit name.  
UNT-008: FormatFallbackLog includes reason and OrgUnit.

### Calculations (UNT-009–013)

UNT-009: DoA priority: Level 2 > Level 3.  
UNT-010: DoA count for OrgUnit returns correct number.  
UNT-011: Active DoA filter excludes disabled users.  
UNT-012: Active DoA filter excludes soft-deleted roles.  
UNT-013: DoA query includes only matching entity type.

### Status Logic (UNT-014–018)

UNT-014: IsFallbackRequired returns true when DoA2 count = 0.  
UNT-015: IsFallbackRequired returns false when DoA2 count > 0.  
UNT-016: GetDoALevel returns "Level 2" when DoA2 used.  
UNT-017: GetDoALevel returns "Level 3" when fallback used.  
UNT-018: IsDoAAuthorized returns true for correct DoA holder.

### Collections (UNT-019–021)

UNT-019: GetAllDoAHolders returns both DoA2 and DoA3.  
UNT-020: GetNotificationRecipients returns only relevant DoA level.  
UNT-021: GetCCRecipients excludes DoA holders (they are in TO).

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: DoA2 lookup < 100ms.  
PRF-002: DoA3 fallback lookup < 200ms (two queries).  
PRF-003: DoA lookup with 100 EntityUserRole records < 100ms.  
PRF-004: DoA lookup with 10K EntityUserRole records < 500ms.  
PRF-005: Notification send (email + in-app) to DoA3 < 2s.  
PRF-006: Full submit workflow with DoA3 fallback < 3s.  
PRF-007: DoA lookup with AsNoTracking vs tracked → AsNoTracking faster.  
PRF-008: Concurrent DoA lookups (10 parallel) < 500ms each.  
PRF-009: Actions Required card load with 50 DoA3 items < 1s.  
PRF-010: Notification bell update for DoA3 < 200ms.  
PRF-011: Workflow history with DoA level filter < 300ms.  
PRF-012: OrgUnit DoA report generation < 2s.  
PRF-013: Memory stable over 100 DoA lookups (no leak).  
PRF-014: DbContextFactory context creation < 10ms per lookup.  
PRF-015: Email template rendering for DoA3 < 100ms.  
PRF-016: Audit trail write for DoA3 decision < 50ms.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 50 concurrent DoA lookups per minute for 10 minutes → All succeed.  
LDT-002: 100 concurrent submissions with DoA3 fallback → 95th percentile < 3s.  
LDT-003: 200 DoA3 notifications per minute → Email queue handles.  
LDT-004: Spike: 100 DoA lookups in 5 seconds → No failures.  
LDT-005: Spike: 50 concurrent DoA3 decisions → First-wins enforced.  
LDT-006: Stress: max concurrent DoA lookups before error rate > 1%.  
LDT-007: Stress: max EntityUserRole records before DoA lookup > 1s.  
LDT-008: Stress: email queue capacity for DoA notifications.  
LDT-009: Recovery after DoA lookup overload → Normal within 30s.  
LDT-010: Recovery after notification queue overflow → All notifications eventually delivered.

---

## Status: Ready for Execution

**C# Test Files:** Already exist in `QA Tests/Integration Tests/PNO-1197_DoA3Fallback/`  
**Next Steps:**
1. Verify C# test files align with these test case IDs
2. Execute tests and record results
3. Map any gaps between markdown and C# implementations
