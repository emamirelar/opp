# PNO-1196: Opportunity Status After Reject — Comprehensive Test Cases

**JIRA Reference:** [PNO-1196](https://unops.atlassian.net/browse/PNO-1196) — Updated logic for Opportunity EntityStatus after Rejecting (set to Closed)  
**Component:** Opportunity Workflow State Machine  
**Implementation:** `WorkflowController.Reject` — custom rejection path for Opportunities sets Stage→NO GO, Status→Closed  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | Max(50, 2×30=60) | ✅ |
| 3 | Boundary Tests | §3 | 90 | Max(50, 2×30=60) | ✅ |
| 4 | Functional Tests | §4 | 90 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 90 | ≥50 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:**
- N≥3P: 90≥90 → ✅ PASS
- E≥3P: 90≥90 → ✅ PASS
- F≥3P: 90≥90 → ✅ PASS
- I≥3P: 90≥90 → ✅ PASS

---

## Feature Overview

When an Opportunity is **rejected** (No-Go decision), the system must:
1. Set `Stage` → `"NO GO"` (via `OpportunityWorkflow.Stages.NoGo`)
2. Set `Status` → `EntityStatus.Closed`
3. Set `WorkflowStatus` → `WorkflowStatus.None`
4. Update audit fields (`LastModifiedBy`, `LastModifiedDate`)
5. Complete the workflow task via `_workflowManager.Reject()`
6. Send rejection notifications
7. Enforce post-rejection immutability (read-only artifact)

### Stage/Status Transition Matrix

| # | Action | From Stage | From Status | To Stage | To Status | WorkflowStatus | Actor |
|---|--------|------------|-------------|----------|-----------|----------------|-------|
| 1 | **Reject** | GO (Active in workflow) | Active | **NO GO** | **Closed** | None | DoA2/DoA3 |
| 2 | Cancel | I&P | Draft | CANCELLED | Closed | None | OM |
| 3 | Reopen (from NO GO) | NO GO | Closed | I&P | Draft | None | OM |
| 4 | Reopen (from CANCELLED) | CANCELLED | Closed | I&P | Draft | None | OM |
| 5 | Approve (Go) | GO (Active in workflow) | Active | GO | Active | None | DoA2/DoA3 |

### Key Implementation Details

- **Code location:** `WorkflowController.cs` lines 792-828
- **Custom path:** Opportunity rejection uses a dedicated code path (not the generic `_workflowManager.Reject`)
- **Immutable stages:** `"GO"`, `"NO GO"`, `"CANCELLED"` — all enforce read-only after transition
- **Related test:** `PNO-1166/FunctionalTests.cs:FUN_002_Reject_SetsStatusToClosed`

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| **Reject → Stage NO GO** | POS-001–005, FUN-001–005, INT-001–005 |
| **Reject → Status Closed** | POS-006–010, FUN-006–010, INT-006–010 |
| **WorkflowStatus → None** | POS-011–013, FUN-011–013 |
| **Audit fields updated** | POS-014–016, FUN-014–016, UNT-001–005 |
| **Post-reject immutability** | POS-017–022, NEG-001–015, BND-001–015, FUN-017–025 |
| **Reject notifications** | POS-023–027, INT-011–020 |
| **Reopen from NO GO** | POS-028–030, FUN-026–035, INT-021–030 |
| **Invalid reject attempts** | NEG-016–045 |
| **Boundary conditions** | BND-016–070 |
| **Reject workflow failures** | NEG-071–090, BND-071–090, FUN-051–090, INT-051–090 |
| **Concurrency** | CON-001–025 |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### Reject → State Change

POS-001: DoA2 rejects opportunity → Stage changes to "NO GO".  
POS-002: DoA2 rejects opportunity → Status changes to EntityStatus.Closed.  
POS-003: DoA2 rejects opportunity → WorkflowStatus changes to None.  
POS-004: DoA3 rejects opportunity (fallback) → Same state change (NO GO/Closed).  
POS-005: Reject with rationale → Rationale stored in workflow history.  
POS-006: Reject response returns `Success = true`.  
POS-007: Reject response returns `NewStage = "NO GO"`.  
POS-008: Reject response message contains "NO GO".  
POS-009: Opportunity entity persisted in DB with Stage="NO GO" after reject.  
POS-010: Opportunity entity persisted in DB with Status=Closed after reject.

### Audit Fields

POS-011: LastModifiedBy set to rejecting user's ID.  
POS-012: LastModifiedDate set to current UTC time.  
POS-013: WorkflowStatus set to None (not InProgress/Pending).

### Audit Trail

POS-014: Workflow history records rejection action.  
POS-015: Workflow history records rejecting user.  
POS-016: Workflow history records rejection timestamp.

### Post-Reject Immutability

POS-017: Opportunity is read-only after rejection (UI).  
POS-018: Edit API call returns 403/400 for rejected opportunity.  
POS-019: Add document API call rejected for NO GO opportunity.  
POS-020: Add stakeholder API call rejected for NO GO opportunity.  
POS-021: canUpdate permission returns false for NO GO opportunity.  
POS-022: Existing documents still downloadable after rejection.

### Notifications

POS-023: Rejection email sent to OM.  
POS-024: Rejection email sent to DoA holders.  
POS-025: Rejection email CC includes Director/Manager.  
POS-026: In-system notification created for rejection.  
POS-027: Actions Required card cleared after rejection.

### Reopen from NO GO

POS-028: OM reopens NO GO opportunity → Stage returns to I&P.  
POS-029: OM reopens NO GO opportunity → Status returns to Draft.  
POS-030: Reopened opportunity is editable again.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### Authorization Failures (NEG-001–015)

NEG-001: OM attempts to reject (not DoA) → Access Denied.  
NEG-002: Collaborator attempts to reject → Access Denied.  
NEG-003: Unauthenticated user calls reject endpoint → 401.  
NEG-004: User with no role calls reject endpoint → 403.  
NEG-005: DoA2 from wrong org unit attempts reject → Access Denied.  
NEG-006: Disabled DoA2 user attempts reject → Account disabled error.  
NEG-007: Expired session during reject → 401.  
NEG-008: Tampered JWT in reject request → 401.  
NEG-009: DoA3 attempts reject when DoA2 exists → Only DoA2 allowed (unless fallback).  
NEG-010: Non-DoA user sends reject via direct API → 403.  
NEG-011: Replay attack with old reject request → Rejected.  
NEG-012: Cross-tenant reject attempt → 403.  
NEG-013: Viewer-only user attempts reject → Access Denied.  
NEG-014: API call with forged user claim → Authorization handler rejects.  
NEG-015: Reject with missing authorization header → 401.

### Invalid State Transitions (NEG-016–030)

NEG-016: Reject opportunity already in NO GO stage → Invalid state error.  
NEG-017: Reject opportunity in CANCELLED stage → Invalid state error.  
NEG-018: Reject opportunity in I&P/Draft (not submitted for Go) → Invalid state.  
NEG-019: Reject opportunity that was already approved (GO) → Invalid state.  
NEG-020: Reject soft-deleted opportunity → 404.  
NEG-021: Reject opportunity with ID=0 → Validation error.  
NEG-022: Reject opportunity with negative ID → Validation error.  
NEG-023: Reject non-existent opportunity → 404.  
NEG-024: Reject opportunity during recall in progress → State conflict.  
NEG-025: Reject after another DoA already rejected → Already decided.  
NEG-026: Reject after another DoA already approved → Already decided.  
NEG-027: Double-click reject button → Second request rejected.  
NEG-028: Reject with stage transition already processing → Conflict error.  
NEG-029: Reject with database lock on opportunity row → Retry or timeout.  
NEG-030: Reject with concurrent status change → Optimistic concurrency violation.

### Missing/Invalid Required Fields (NEG-031–045)

NEG-031: Reject without rationale text → Validation error.  
NEG-032: Reject with empty rationale (whitespace only) → Validation error.  
NEG-033: Reject without confirmation acknowledgment → Validation error.  
NEG-034: Reject with null EntityName → 400 Bad Request.  
NEG-035: Reject with empty EntityName → 400 Bad Request.  
NEG-036: Reject with EntityName != "opportunity" → Wrong rejection path.  
NEG-037: Reject with null EntityId → 400 Bad Request.  
NEG-038: Reject with malformed JSON body → 400 Bad Request.  
NEG-039: Reject with extra unknown fields → Ignored or 400.  
NEG-040: Reject with EntityName case mismatch "Opportunity" vs "opportunity" → Handled.  
NEG-041: SQL injection in rationale field → Sanitized.  
NEG-042: XSS in rationale field → HTML escaped.  
NEG-043: Rationale with null bytes → Sanitized.  
NEG-044: Rationale with CRLF injection → Sanitized.  
NEG-045: Rationale exceeding max length → Rejected or truncated.

### Post-Reject Violations (NEG-046–060)

NEG-046: Edit opportunity fields after reject → Blocked (immutable).  
NEG-047: Change Stage via API after reject → Blocked.  
NEG-048: Change Status via API after reject → Blocked.  
NEG-049: Add document after reject → Blocked.  
NEG-050: Add comment after reject → Blocked.  
NEG-051: Add stakeholder after reject → Blocked.  
NEG-052: Modify budget after reject → Blocked.  
NEG-053: Update org unit after reject → Blocked.  
NEG-054: Submit for Go decision on NO GO opportunity → Blocked.  
NEG-055: Change Executive after reject → Blocked.  
NEG-056: Bulk update including NO GO opportunity → Skipped with error.  
NEG-057: Import data into NO GO opportunity → Blocked.  
NEG-058: AI content generation for NO GO opportunity → Blocked or read-only.  
NEG-059: Modify collaborators after reject → Blocked.  
NEG-060: Delete NO GO opportunity → Only soft-delete allowed.

### Error Handling (NEG-061–070)

NEG-061: Database error during reject → Transaction rolled back, no partial state.  
NEG-062: Notification service failure → Reject still succeeds, notification queued.  
NEG-063: Email service failure → Reject still succeeds, email queued.  
NEG-064: Workflow engine failure → Reject fails cleanly, state unchanged.  
NEG-065: Connection timeout during reject → Error message, retry possible.  
NEG-066: Reject with DbContext disposed → Internal error handled.  
NEG-067: Reject during database maintenance → Queued or retried.  
NEG-068: Partial SaveChanges (stage changed but notification failed) → Atomic.  
NEG-069: Reject with concurrent opportunity deletion → 404 or conflict.  
NEG-070: Reject API returns correct error structure (ProblemDetails).

### Reject Workflow Failures (NEG-071–090)

NEG-071: Reject when workflow task already completed → Invalid state error.  
NEG-072: Reject with workflow engine unavailable → Graceful failure, retry suggested.  
NEG-073: Reject with workflow context missing → 400 Bad Request.  
NEG-074: Reject with invalid workflow transition → State machine rejects.  
NEG-075: Reject when workflow task expired → Task expired error.  
NEG-076: Reject with workflow definition not found → Configuration error.  
NEG-077: Reject when workflow is locked by another process → Lock conflict.  
NEG-078: Reject with workflow version mismatch → Upgrade required error.  
NEG-079: Reject when workflow actor not in allowed list → Authorization denied.  
NEG-080: Reject with workflow payload validation failure → Validation error.  
NEG-081: Reject when workflow engine times out → Timeout error, no partial state.  
NEG-082: Reject with workflow history write failure → Transaction rolled back.  
NEG-083: Reject when workflow task not found → 404 or invalid state.  
NEG-084: Reject with workflow state corruption → Error logged, safe failure.  
NEG-085: Reject when workflow completion callback fails → Retry or error.  
NEG-086: Reject with workflow notification dispatch failure → Reject succeeds, notification retried.  
NEG-087: Reject when workflow audit trail write fails → Atomic rollback.  
NEG-088: Reject with workflow engine connection pool exhausted → 503 or retry.  
NEG-089: Reject when workflow stage validation fails → Invalid transition error.  
NEG-090: Reject with workflow circular dependency detected → Configuration error.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### Status Transition Boundaries (BND-001–015)

BND-001: Status before reject is exactly Active → Becomes Closed.  
BND-002: Status before reject is exactly Draft (shouldn't be possible) → Error.  
BND-003: Status before reject is Closed (already) → Invalid state.  
BND-004: Status before reject is OnHold → Invalid state.  
BND-005: Status before reject is Archived → Invalid state.  
BND-006: Stage before reject is exactly "GO" (in workflow) → Becomes "NO GO".  
BND-007: Stage before reject is "IDENTIFY & PROFILE" → Invalid (not submitted).  
BND-008: Stage before reject is "CANCELLED" → Invalid.  
BND-009: Stage before reject is "NO GO" → Invalid (already rejected).  
BND-010: Stage string comparison: "GO" vs "Go" vs "go" → Normalized correctly.  
BND-011: EntityStatus.Closed value as integer → Correct enum value (3).  
BND-012: WorkflowStatus.None value after reject → Correct enum value (0).  
BND-013: Stage="NO GO" stored with exact casing → Matches OpportunityWorkflow.Stages.NoGo.  
BND-014: Multiple sequential reject attempts → Only first succeeds.  
BND-015: Reject then immediately query status → Consistent (Closed).

### Rationale Field Boundaries (BND-016–030)

BND-016: Rationale at 1 character → Accepted.  
BND-017: Rationale at 100 characters → Accepted.  
BND-018: Rationale at 1000 characters → Accepted.  
BND-019: Rationale at max length (4000 chars) → Accepted.  
BND-020: Rationale at max+1 → Rejected.  
BND-021: Rationale with Unicode (Arabic, Chinese) → Stored and displayed correctly.  
BND-022: Rationale with emoji → Stored correctly.  
BND-023: Rationale with newlines → Preserved.  
BND-024: Rationale with HTML tags → Escaped.  
BND-025: Rationale with only spaces → Rejected (whitespace-only).  
BND-026: Rationale with leading/trailing whitespace → Trimmed.  
BND-027: Rationale with tab characters → Stored correctly.  
BND-028: Rationale with single quote → Stored correctly (no SQL issues).  
BND-029: Rationale with double quote → Stored correctly.  
BND-030: Rationale with backslash → Stored correctly.

### Timing Boundaries (BND-031–045)

BND-031: Reject immediately after submission (< 1s) → Accepted.  
BND-032: Reject 30 days after submission → Accepted (no time limit).  
BND-033: Reject at midnight UTC → Correct timestamp.  
BND-034: Reject during DST transition → UTC timestamp unambiguous.  
BND-035: Reject on Feb 29 → Valid date.  
BND-036: Reject with LastModifiedDate = DateTime.UtcNow → Accurate within 1s.  
BND-037: Reject at session timeout boundary → Re-auth or error.  
BND-038: Reject with slow network (5s latency) → Completes or clear timeout.  
BND-039: Reject during concurrent page refresh → Consistent state.  
BND-040: Reject after dialog open for 1+ hour → Still succeeds if session valid.  
BND-041: Reject within 1ms of another action on same opportunity → One succeeds.  
BND-042: WorkflowHistory timestamp matches rejection timestamp → Consistent.  
BND-043: Notification timestamp matches rejection timestamp → Consistent.  
BND-044: Audit CreatedDate and LastModifiedDate relationship → Modified >= Created.  
BND-045: Reject at exact API timeout limit → Clear result (success or timeout).

### Reopen Boundaries (BND-046–060)

BND-046: Reopen immediately after reject (< 1s) → Accepted.  
BND-047: Reopen 365 days after reject → Accepted.  
BND-048: Reopen → Status returns to exactly Draft.  
BND-049: Reopen → Stage returns to exactly "IDENTIFY & PROFILE".  
BND-050: Reopen → WorkflowStatus returns to None.  
BND-051: Reopen preserves all opportunity data → No field loss.  
BND-052: Reopen preserves workflow history → Previous reject entry remains.  
BND-053: Reopen → canUpdate permission returns true.  
BND-054: Reopen → Re-submit → Re-reject → Status Closed again.  
BND-055: 10 cycles of reject/reopen → All transitions correct.  
BND-056: Reopen by different OM (after OM transfer) → Accepted.  
BND-057: Reopen when OM is disabled → Access Denied.  
BND-058: Reopen preserves attached documents → Still accessible.  
BND-059: Reopen preserves stakeholders → Still listed.  
BND-060: Reopen preserves collaborators → Still assigned.

### Display Boundaries (BND-061–070)

BND-061: "Closed" displayed for status on detail page → Correct text.  
BND-062: "Closed" displayed in light red color → CSS class applied.  
BND-063: "NO GO" displayed for stage on list page → Correct text.  
BND-064: "NO GO" displayed for stage on detail page → Correct text.  
BND-065: Status filter "Closed" includes NO GO opportunities → Correct.  
BND-066: Status filter "Closed" includes CANCELLED opportunities → Correct.  
BND-067: Stage filter "NO GO" returns only rejected opportunities → Correct.  
BND-068: Sort by status → NO GO (Closed) sorted with other Closed.  
BND-069: Export includes correct status (Closed) for NO GO → Correct.  
BND-070: API response for NO GO opportunity includes Status="Closed" → Correct JSON.

### Workflow Failure Boundaries (BND-071–090)

BND-071: Reject at workflow engine timeout boundary → Clear success or timeout.  
BND-072: Reject when workflow task expires in 1 second → Reject before expiry succeeds.  
BND-073: Reject with rationale at exactly 4000 chars → Accepted.  
BND-074: Reject with rationale at 4001 chars → Rejected.  
BND-075: Reject with EntityId at max int boundary → Handled or overflow error.  
BND-076: Reject with workflow history at max entries → No overflow.  
BND-077: Reject when workflow lock held for exactly timeout period → Released, retry succeeds.  
BND-078: Reject with 0-length rationale → Rejected (empty).  
BND-079: Reject with workflow engine at 99% capacity → Queued or succeeds.  
BND-080: Reject at workflow batch boundary → Single item processed correctly.  
BND-081: Reject with LastModifiedDate at DateTime.MaxValue edge → Handled.  
BND-082: Reject with workflow version at upgrade boundary → Correct path taken.  
BND-083: Reject when notification queue at capacity → Reject succeeds, queue overflow handled.  
BND-084: Reject with concurrent workflow completions (N) → Exactly one succeeds.  
BND-085: Reject at database connection pool limit → Queued or 503.  
BND-086: Reject with workflow state at transition boundary → Correct final state.  
BND-087: Reject when audit log at retention boundary → No truncation during reject.  
BND-088: Reject with workflow payload at max size → Accepted or rejected consistently.  
BND-089: Reject at midnight boundary (23:59:59.999) → Timestamp correct.  
BND-090: Reject with workflow retry count at max → Final failure or success.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### Stage/Status Rules (FUN-001–015)

FUN-001: Reject sets Stage to OpportunityWorkflow.Stages.NoGo ("NO GO").  
FUN-002: Reject sets Status to EntityStatus.Closed.  
FUN-003: Reject sets WorkflowStatus to WorkflowStatus.None.  
FUN-004: Cancel sets Stage to "CANCELLED" (different from reject).  
FUN-005: Cancel also sets Status to EntityStatus.Closed (same as reject).  
FUN-006: Approve (Go) keeps Status as Active (different from reject).  
FUN-007: Approve (Go) sets Stage to "GO" (different from NO GO).  
FUN-008: Reopen from NO GO → Stage="IDENTIFY & PROFILE", Status=Draft.  
FUN-009: Reopen from CANCELLED → Stage="IDENTIFY & PROFILE", Status=Draft.  
FUN-010: Non-Opportunity entities use generic reject path (different behavior).  
FUN-011: WorkflowStatus=None means opportunity is not in any workflow.  
FUN-012: Rejected opportunity not counted in "Active" dashboard metrics.  
FUN-013: Rejected opportunity counted in "Closed" dashboard metrics.  
FUN-014: Rejected opportunity Stage="NO GO" distinct from Stage="CANCELLED".  
FUN-015: Both NO GO and CANCELLED result in Status=Closed.

### Immutability Rules (FUN-016–030)

FUN-016: IsImmutable("NO GO") returns true.  
FUN-017: IsImmutable("CANCELLED") returns true.  
FUN-018: IsImmutable("GO") returns true.  
FUN-019: IsImmutable("IDENTIFY & PROFILE") returns false.  
FUN-020: canUpdate flag = false for NO GO opportunity.  
FUN-021: canUpdate flag = false for CANCELLED opportunity.  
FUN-022: canUpdate flag = false for GO (approved) opportunity.  
FUN-023: canUpdate flag = true for I&P/Draft opportunity.  
FUN-024: Edit endpoint checks immutability before processing.  
FUN-025: Immutability applies to ALL fields (budget, org unit, stakeholders).  
FUN-026: Reopen removes immutability → fields editable.  
FUN-027: Reopen does not clear any field values.  
FUN-028: Reopen does not delete attached documents.  
FUN-029: Reopen does not remove stakeholders.  
FUN-030: Reopen triggers fresh permission check.

### Notification Rules (FUN-031–040)

FUN-031: Rejection notification email subject contains opportunity name.  
FUN-032: Rejection notification email body says "NO GO".  
FUN-033: Rejection notification includes rationale text.  
FUN-034: Rejection notification CC includes OM.  
FUN-035: Rejection notification CC includes Director/Manager.  
FUN-036: In-system notification created with "Rejected" icon/styling.  
FUN-037: Actions Required cleared for all DoA holders after rejection.  
FUN-038: Notification bell count decremented after rejection.  
FUN-039: Rejection notification timestamp matches workflow history.  
FUN-040: No duplicate notifications for same rejection.

### Audit & History Rules (FUN-041–050)

FUN-041: Workflow history entry created for rejection.  
FUN-042: History entry includes action type "Reject".  
FUN-043: History entry includes rejecting user display name.  
FUN-044: History entry includes rationale.  
FUN-045: History entry includes timestamp.  
FUN-046: History entries ordered chronologically.  
FUN-047: Rejection history preserved after reopen.  
FUN-048: LastModifiedBy updated to rejecting user.  
FUN-049: LastModifiedDate updated to rejection time.  
FUN-050: Audit trail captures stage transition (GO → NO GO).

### Reject Workflow Rules (FUN-051–090)

FUN-051: Workflow Reject() called with correct entity type.  
FUN-052: Workflow Reject() receives correct opportunity ID.  
FUN-053: Workflow Reject() receives rationale in payload.  
FUN-054: Workflow completion marks task as done.  
FUN-055: Workflow engine validates stage before reject.  
FUN-056: Workflow engine validates actor before reject.  
FUN-057: Workflow state machine allows GO→NO GO transition.  
FUN-058: Workflow state machine denies NO GO→NO GO.  
FUN-059: Workflow task lookup by opportunity ID succeeds.  
FUN-060: Workflow task completion is idempotent (second call no-op).  
FUN-061: Workflow history write occurs after stage change.  
FUN-062: Workflow notification dispatch after DB commit.  
FUN-063: Workflow engine returns success when all steps complete.  
FUN-064: Workflow engine returns failure when any step fails.  
FUN-065: Workflow Reject uses Opportunity custom path (not generic).  
FUN-066: Workflow controller checks EntityName before custom path.  
FUN-067: Workflow rejection does not affect other opportunities.  
FUN-068: Workflow rejection does not affect other entity types.  
FUN-069: Workflow status None set before notification send.  
FUN-070: Workflow audit fields updated in same transaction.  
FUN-071: Workflow engine timeout does not leave partial state.  
FUN-072: Workflow retry on transient failure uses same payload.  
FUN-073: Workflow lock released on failure.  
FUN-074: Workflow lock released on success.  
FUN-075: Workflow task expiry checked before processing.  
FUN-076: Workflow actor validated against DoA list.  
FUN-077: Workflow fallback to DoA3 when DoA2 unavailable.  
FUN-078: Workflow completion callback invoked on success.  
FUN-079: Workflow completion callback not invoked on failure.  
FUN-080: Workflow history includes workflow task ID.  
FUN-081: Workflow rejection triggers permission cache invalidation.  
FUN-082: Workflow rejection triggers search index update.  
FUN-083: Workflow rejection triggers dashboard metric refresh.  
FUN-084: Workflow engine connection reused from pool.  
FUN-085: Workflow payload serialization preserves rationale.  
FUN-086: Workflow payload deserialization validates structure.  
FUN-087: Workflow rejection idempotent for duplicate requests.  
FUN-088: Workflow rejection clears pending actions.  
FUN-089: Workflow rejection updates last activity timestamp.  
FUN-090: Workflow rejection respects feature flag if present.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### Reject Workflow End-to-End (INT-001–015)

INT-001: Submit → Reject → Verify Stage=NO GO in DB.  
INT-002: Submit → Reject → Verify Status=Closed in DB.  
INT-003: Submit → Reject → Verify WorkflowStatus=None in DB.  
INT-004: Submit → Reject → Verify LastModifiedBy = DoA2 user ID.  
INT-005: Submit → Reject → Verify workflow history row created.  
INT-006: Reject API call → Workflow engine Reject() called.  
INT-007: Reject API call → Opportunity SaveChangesAsync() called.  
INT-008: Reject API call → Notification service called.  
INT-009: Reject API call → Email service called.  
INT-010: Full cycle: Create → Submit → Reject → Verify all DB state.  
INT-011: Full cycle: Create → Submit → Reject → Verify email sent.  
INT-012: Full cycle: Create → Submit → Reject → Verify notification created.  
INT-013: Reject with DoA3 fallback → Same DB state as DoA2 rejection.  
INT-014: Reject → Check Opportunity permissions endpoint → canEdit=false.  
INT-015: Reject → Check opportunity list API → Shows Closed status.

### Reopen After Reject (INT-016–030)

INT-016: Reject → Reopen → Verify Stage=I&P in DB.  
INT-017: Reject → Reopen → Verify Status=Draft in DB.  
INT-018: Reject → Reopen → Verify all fields preserved.  
INT-019: Reject → Reopen → Re-submit → Verify DoA lookup fresh.  
INT-020: Reject → Reopen → Re-submit → Re-reject → Stage=NO GO again.  
INT-021: Reject → Reopen → Verify workflow history has both entries.  
INT-022: Reject → Reopen → Verify canEdit=true.  
INT-023: Reject → Reopen → Verify documents still accessible.  
INT-024: Reject → Reopen → Verify collaborators still assigned.  
INT-025: Reject → Reopen → Verify stakeholders still assigned.  
INT-026: Reject → Reopen → Verify budget unchanged.  
INT-027: Reject → Reopen → Verify org unit unchanged.  
INT-028: Reject → Reopen by different OM (after OM transfer).  
INT-029: Reject → Reopen → Verify Actions Required cleared.  
INT-030: Reject → Reopen → Verify opportunity no longer in "Closed" filter.

### Search & Filter Integration (INT-031–040)

INT-031: Search by Stage "NO GO" → Returns rejected opportunity.  
INT-032: Filter by Status "Closed" → Includes NO GO and CANCELLED.  
INT-033: Filter by Status "Active" → Excludes NO GO.  
INT-034: Dashboard "Closed" count includes NO GO.  
INT-035: Dashboard "Active" count excludes NO GO.  
INT-036: Sort by Stage → "NO GO" sorted correctly.  
INT-037: Sort by Status → "Closed" grouped.  
INT-038: Pagination with NO GO opportunities → Correct count.  
INT-039: Export with NO GO opportunities → Status=Closed in export.  
INT-040: Advanced search across stages → NO GO included.

### Error Recovery (INT-041–050)

INT-041: DB error during reject → No partial state change.  
INT-042: Reject + notification failure → Reject persisted, notification retried.  
INT-043: Reject + email failure → Reject persisted, email queued.  
INT-044: Reject + workflow engine error → Reject fails completely, no state change.  
INT-045: Concurrent reject + approve → One succeeds, one fails.  
INT-046: Concurrent reject + cancel → One succeeds, one fails.  
INT-047: Concurrent reject + reopen → Reject completes first.  
INT-048: Transaction isolation: reject doesn't see uncommitted changes.  
INT-049: Connection pool under load → Reject queued, not failed.  
INT-050: API response includes correct HTTP status code (200 OK on success).

### Workflow Integration (INT-051–090)

INT-051: Reject API → WorkflowController.Reject invoked.  
INT-052: Reject API → WorkflowManager.Reject invoked for opportunity.  
INT-053: Reject API → OpportunityManager.UpdateAsync for stage/status.  
INT-054: Reject API → NotificationService.SendRejectionNotification.  
INT-055: Reject API → EmailService.QueueRejectionEmail.  
INT-056: Reject API → AuditService.LogWorkflowAction.  
INT-057: Workflow engine + DB in same transaction → Atomic.  
INT-058: Workflow engine + notification in separate transaction → Eventually consistent.  
INT-059: Reject → Workflow task marked complete in workflow DB.  
INT-060: Reject → Opportunity row updated in main DB.  
INT-061: Reject → Workflow history in main DB.  
INT-062: Reject with workflow engine down → Graceful degradation.  
INT-063: Reject with notification service down → Reject succeeds.  
INT-064: Reject with email service down → Reject succeeds.  
INT-065: Reject → Permission service cache invalidated.  
INT-066: Reject → Search index updated asynchronously.  
INT-067: Reject → Dashboard metric service notified.  
INT-068: Reject → Actions Required service updated.  
INT-069: Reject → User notification bell updated.  
INT-070: Reject across multiple services → Distributed transaction or saga.  
INT-071: Reject API + Workflow API consistency → Same result.  
INT-072: Reject from UI → Same as Reject from API.  
INT-073: Reject from batch job → Same validation as API.  
INT-074: Reject with workflow engine version upgrade → Backward compatible.  
INT-075: Reject with DB migration in progress → Handled or queued.  
INT-076: Reject with notification queue full → Reject succeeds, overflow logged.  
INT-077: Reject with audit service slow → Reject completes, audit async.  
INT-078: Reject with multiple DoA holders → Correct notification list.  
INT-079: Reject with OM in different org unit → Notification routing correct.  
INT-080: Reject with Director/Manager lookup → CC list correct.  
INT-081: Reject → oUP integration notified (if configured).  
INT-082: Reject → External system webhook (if configured).  
INT-083: Reject with feature flag disabled → Reject blocked or legacy path.  
INT-084: Reject with A/B test variant → Correct behavior.  
INT-085: Reject in multi-tenant env → Tenant isolation maintained.  
INT-086: Reject with tenant-specific workflow config → Correct path.  
INT-087: Reject → Logging service receives structured log.  
INT-088: Reject → Telemetry service receives event.  
INT-089: Reject → APM traces capture full flow.  
INT-090: Reject end-to-end latency within SLA.

---

## §6 Security Tests — OUT OF SCOPE

> Security testing is handled by the Infrastructure and Security teams per project policy.

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two DoA holders reject simultaneously → One succeeds, one gets "already decided".  
CON-002: Reject and Approve at same time → One wins, consistent final state.  
CON-003: Reject and Cancel at same time → One wins.  
CON-004: Reject and Recall at same time → One wins.  
CON-005: Double-click reject → Only one DB write.  
CON-006: Reject during page refresh → Consistent state.  
CON-007: Reject from two browser tabs → First succeeds.  
CON-008: Reject during concurrent opportunity update → Conflict resolution.  
CON-009: Reject + notification send race → Both complete or both roll back.  
CON-010: Reject + audit write atomicity → Both succeed or both fail.  
CON-011: Reject + email send → Not dependent on email success.  
CON-012: Concurrent rejects on different opportunities → Independent processing.  
CON-013: Reject + DbContext concurrent access → Factory provides isolation.  
CON-014: Transaction isolation level for reject → Read Committed or higher.  
CON-015: Optimistic concurrency on Opportunity row → Detected and handled.  
CON-016: Reject during DB backup → Queued or completed.  
CON-017: Reject during migration execution → Error handled gracefully.  
CON-018: Concurrent status reads during reject → Consistent (old or new).  
CON-019: Reject + workflow history write → Atomic.  
CON-020: Reject + notification mark-as-rejected → Atomic.  
CON-021: Parallel rejection notification sends → No duplicates.  
CON-022: Concurrent workflow completion → Single completion.  
CON-023: Cache invalidation after reject → Next read sees NO GO/Closed.  
CON-024: Reject during search index update → Index eventually consistent.  
CON-025: Reject + concurrent dashboard metric calculation → Correct counts.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (UNT-001–005)

UNT-001: ValidateRejectRequest returns error when rationale is null.  
UNT-002: ValidateRejectRequest returns error when rationale is empty.  
UNT-003: ValidateRejectRequest returns error when confirmation is false.  
UNT-004: ValidateRejectRequest passes for valid request.  
UNT-005: ValidateRejectRequest passes with rationale at max length.

### Formatting (UNT-006–008)

UNT-006: FormatRejectionNotification includes "NO GO" in subject.  
UNT-007: FormatRejectionNotification includes rationale in body.  
UNT-008: FormatRejectionEmail builds correct CC list.

### Calculations (UNT-009–013)

UNT-009: GetTargetStage("Reject") returns "NO GO".  
UNT-010: GetTargetStatus("Reject") returns EntityStatus.Closed.  
UNT-011: IsImmutableStage("NO GO") returns true.  
UNT-012: IsImmutableStage("CANCELLED") returns true.  
UNT-013: IsImmutableStage("IDENTIFY & PROFILE") returns false.

### Status Logic (UNT-014–018)

UNT-014: EntityStatus.Closed equals integer value 3.  
UNT-015: WorkflowStatus.None equals integer value 0.  
UNT-016: OpportunityWorkflow.Stages.NoGo equals "NO GO".  
UNT-017: CanReopen returns true for NO GO stage.  
UNT-018: CanReopen returns true for CANCELLED stage.

### Collections (UNT-019–021)

UNT-019: GetImmutableStages returns ["GO", "NO GO", "CANCELLED"].  
UNT-020: GetClosedStatuses returns opportunities with Status=Closed.  
UNT-021: GetRejectedOpportunities filters by Stage="NO GO".

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Reject API call completes in < 500ms.  
PRF-002: Status change persisted in < 100ms.  
PRF-003: Workflow history write < 50ms.  
PRF-004: Notification creation < 200ms.  
PRF-005: Email send queue < 100ms.  
PRF-006: Full reject cycle (API → DB → notification) < 2s.  
PRF-007: Reopen API call completes in < 500ms.  
PRF-008: Reject under 10 concurrent requests < 1s each.  
PRF-009: Status filter query for Closed < 300ms with 10K opportunities.  
PRF-010: Stage filter query for NO GO < 300ms.  
PRF-011: Dashboard metric calculation with NO GO < 500ms.  
PRF-012: Reject memory usage stable (no leak over 100 rejections).  
PRF-013: Reopen cycle memory stable.  
PRF-014: Reject + notification async pipeline < 3s total.  
PRF-015: Concurrent reject on different opportunities < 2s each.  
PRF-016: Workflow history query with 1000 entries < 500ms.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 20 concurrent rejections per minute for 10 minutes → All succeed.  
LDT-002: 50 concurrent reject + reopen cycles → All complete correctly.  
LDT-003: 100 concurrent status filter queries for Closed → 95th percentile < 1s.  
LDT-004: Spike: 50 rejections in 5 seconds → No failures.  
LDT-005: Spike: 100 concurrent notification sends → All queued.  
LDT-006: Stress: max concurrent rejections before error rate > 1%.  
LDT-007: Stress: max workflow history entries before query > 1s.  
LDT-008: Email queue: 200 rejection emails in 1 minute → All queued.  
LDT-009: Recovery after reject overload → Normal within 30s.  
LDT-010: Recovery after notification queue overflow → All eventually delivered.

---

## Status: Ready for Implementation
