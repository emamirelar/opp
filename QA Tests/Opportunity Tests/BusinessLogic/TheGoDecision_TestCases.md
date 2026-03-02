# The Go Decision — Comprehensive Test Cases

**Component:** The Go Decision (DoA2 Decision-Maker Experience)  
**PRD:** `tasks/the-go-decision/the-go-decision-prd.md`  
**Test Plan:** `QA Tests/Test Plans/the-go-decision-feature-plan.md`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, MANDATORY 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 90 (= 3×30) | ✅ |
| 3 | Boundary Tests | §3 | 90 | 90 (= 3×30) | ✅ |
| 4 | Functional Tests | §4 | 90 | 90 (= 3×30) | ✅ |
| 5 | Integration Tests | §5 | 90 | 90 (= 3×30) | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **462** | ✅ |

### MANDATORY Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Traceability Matrix

| Requirement / AC | Test Cases |
|------------------|------------|
| **Actions Required Card** — Show pending Go decision tasks | POS-001, POS-002, NEG-001, NEG-002, FUN-001–003, BND-001–003 |
| **Notification Bell** — Pending Go decision notifications | POS-003, POS-004, NEG-003–005, FUN-004–006, BND-004–006 |
| **Decision-Maker Review UI** — Instructional guidance | POS-005, POS-006, NEG-006–010, FUN-007–010, BND-007–010 |
| **Highlighted Info Panel** — Initiative type, time to signing, DD, risks, remarks | FUN-011–015, NEG-011–020, BND-011–020 |
| **Go Decision Dialog** — Confirmation, rationale, Executive assignment | POS-007–012, NEG-021–040, FUN-016–025, BND-021–040 |
| **No-Go Decision Dialog** — Confirmation, rationale | POS-013–015, NEG-041–050, FUN-026–030, BND-041–050 |
| **Post-Decision Immutability** — Read-only artifact | POS-016–018, NEG-051–060, FUN-031–035, BND-051–060 |
| **Email CC Recipients** — OM, initiator, Director/Manager | POS-019–021, NEG-061–070, FUN-036–040, BND-061–070 |
| **Executive Assignment** — ExecutiveId field, mandatory on Go | POS-012, POS-022–024, NEG-071–080, FUN-041–050, BND-071–080 |
| **Workflow History** — Action records | FUN-046–050, INT-026–035 |
| **Decision-Maker Workflow** — DoA2/DoA3 assignment, fallback | NEG-076–080, NEG-081–085, BND-081–085, FUN-051–060 |
| **Notifications & Immutability** — Post-decision behavior | NEG-056–065, NEG-083–090, BND-086–090, FUN-061–070 |

---

## §1 Positive Tests (Happy Path) — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Actions Required Card

**POS-001: Pending Go decision appears on Actions Required card**  
Precondition: Opportunity submitted for Go decision. User is DoA2.  
Steps: Log in as DoA2 → Navigate to homepage → Check Actions Required card.  
Expected: Workflow Approvals section shows opportunity name, org unit, submitter, and submission date.

**POS-002: Clicking Actions Required item navigates to opportunity**  
Precondition: POS-001 confirmed.  
Steps: Click the pending approval item on Actions Required card.  
Expected: Navigates to opportunity detail page, Statement section visible.

### Notification Bell

**POS-003: Notification bell shows unread count for pending decision**  
Precondition: Opportunity submitted for Go decision. User is DoA2.  
Steps: Log in as DoA2 → Check notification bell icon.  
Expected: Unread count incremented, bell shows indicator.

**POS-004: Clicking notification navigates to opportunity**  
Precondition: POS-003 confirmed.  
Steps: Click notification bell → Click the Go decision notification.  
Expected: Navigates to opportunity detail page.

### Decision-Maker Review UI

**POS-005: Instructional guidance message displayed for DoA2**  
Precondition: DoA2 views pending opportunity.  
Steps: Navigate to opportunity in GO/Active stage.  
Expected: Blue info message with title "Action Required: Go/No-Go Decision" and review instructions.

**POS-006: Guidance hidden after decision is made**  
Precondition: DoA2 completed Go or No-Go decision.  
Steps: Reload opportunity page after decision.  
Expected: Instructional guidance no longer visible.

### Go Decision Dialog

**POS-007: Approve button visible for DoA2 on pending opportunity**  
Precondition: DoA2 viewing opportunity in GO/Active stage.  
Expected: "Approve" workflow action button visible.

**POS-008: Go Decision dialog opens on Approve click**  
Steps: Click "Approve" action.  
Expected: Dialog opens with title "Confirm Go Decision".

**POS-009: Confirmation statement contains org unit code and initiative type**  
Expected: Auto-generated statement text includes org unit code and initiative type.

**POS-010: Go decision submitted with all required fields**  
Steps: Check confirmation checkbox → Enter rationale → Select Executive → Click "Confirm Go Decision".  
Expected: Success toast, dialog closes, stage → GO, status remains Active.

**POS-011: Stage stepper updates to GO after approval**  
Precondition: POS-010 completed.  
Expected: Stage stepper shows GO as current stage.

**POS-012: Executive field populated on opportunity after Go**  
Precondition: POS-010 completed.  
Expected: Opportunity detail shows selected Executive name.

### No-Go Decision Dialog

**POS-013: Reject button visible for DoA2 on pending opportunity**  
Expected: "Reject" workflow action button visible.

**POS-014: No-Go Decision dialog opens on Reject click**  
Steps: Click "Reject" action.  
Expected: Dialog opens with title "Confirm No-Go Decision".

**POS-015: No-Go submitted with rationale**  
Steps: Check confirmation checkbox → Enter rationale → Click "Confirm No-Go Decision".  
Expected: Success toast, dialog closes, stage → NO GO, status → Closed.

### Post-Decision Immutability

**POS-016: Opportunity read-only after Go decision**  
Precondition: Go decision completed.  
Expected: All fields read-only, no edit controls, no workflow actions except view.

**POS-017: Opportunity read-only after No-Go decision**  
Precondition: No-Go decision completed.  
Expected: All fields read-only, status Closed.

**POS-018: Existing documents still downloadable after decision**  
Precondition: Opportunity has documents, decision completed.  
Expected: Download button functional for existing documents.

### Email CC Recipients

**POS-019: Go decision email sent to DoA holders**  
Precondition: Go decision completed.  
Expected: Email received by DoA2 holder(s).

**POS-020: Go decision email CC includes OM**  
Expected: OM receives CC copy of decision email.

**POS-021: Go decision email CC includes Director/Manager**  
Expected: Director/Manager of responsible org unit in CC.

### Executive Assignment

**POS-022: Executive dropdown populated from EntityUserRole**  
Expected: Dropdown shows Director/Manager/OiC entries for responsible org unit.

**POS-023: Executive selection persisted on opportunity**  
Precondition: Go decision completed with Executive selected.  
Expected: `ExecutiveId` field populated in database.

**POS-024: Executive displayed on opportunity detail after Go**  
Expected: Opportunity detail page shows selected Executive name.

### Workflow History

**POS-025: Workflow history records Go decision**  
Precondition: POS-010 completed.  
Expected: Workflow history shows Go decision with timestamp, user, and rationale.

**POS-026: Workflow history records No-Go decision**  
Precondition: POS-015 completed.  
Expected: History shows No-Go with timestamp, user, and rationale.

### Info Panel & Notifications

**POS-027: Info panel displays initiative type**  
Precondition: DoA2 viewing pending opportunity with initiative type set.  
Expected: Info panel shows initiative type matching opportunity data.

**POS-028: Info panel displays time to signing**  
Expected: Shows calculated days with color indicator (green/amber/red based on proximity).

**POS-029: No new documents can be added after decision**  
Precondition: Decision completed.  
Expected: Add document button disabled/hidden.

**POS-030: Go decision email CC includes initiator (if different from OM)**  
Precondition: Workflow initiator is different from current OM.  
Expected: Original initiator in CC.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 90 (= 3×30)** | ✅ COMPLIANT

### Authorization Failures (NEG-001–015)

NEG-001: Collaborator attempts Go decision → Access Denied (collaborators cannot perform workflow actions).  
NEG-002: Collaborator attempts No-Go decision → Access Denied.  
NEG-003: Unauthenticated user accesses Go decision endpoint → 401 Unauthorized.  
NEG-004: User with no role accesses pending opportunity → Forbidden.  
NEG-005: OM attempts to approve own submission (if OM ≠ DoA2) → Access Denied.  
NEG-006: DoA2 from wrong org unit attempts decision → Access Denied.  
NEG-007: Disabled DoA2 user attempts decision → Account disabled error.  
NEG-008: Expired session during decision submission → 401, redirect to login.  
NEG-009: Tampered JWT token in decision request → 401 Unauthorized.  
NEG-010: Replay attack with previously valid token → Rejected.  
NEG-011: DoA3 attempts decision when DoA2 exists → Only DoA2 allowed.  
NEG-012: Non-DoA user accesses approve endpoint via direct API → 403 Forbidden.  
NEG-013: Collaborator attempts to recall opportunity → Access Denied.  
NEG-014: Viewer-only user attempts any workflow action → Access Denied.  
NEG-015: Cross-tenant user attempts decision on another tenant's opportunity → 403 Forbidden.

### Invalid State Transitions (NEG-016–030)

NEG-016: Go decision on opportunity already in GO stage → Invalid state error.  
NEG-017: No-Go decision on opportunity already in NO GO → Invalid state error.  
NEG-018: Go decision on CANCELLED opportunity → Invalid state error.  
NEG-019: Approve opportunity in I&P/Draft (not yet submitted) → Invalid state.  
NEG-020: Reject opportunity in I&P/Draft (not yet submitted) → Invalid state.  
NEG-021: Double-click approve button → Second request rejected.  
NEG-022: Go decision after opportunity already recalled → Invalid state.  
NEG-023: No-Go decision on opportunity where DoA2 already approved → State conflict.  
NEG-024: Cancel attempt by non-OM during workflow → Access Denied.  
NEG-025: Reopen attempt on Active (non-closed) opportunity → Invalid state.  
NEG-026: Submit for Go decision when already in workflow → Invalid state.  
NEG-027: Approve with stage transition already in progress → Conflict error.  
NEG-028: Decision on soft-deleted opportunity → 404 Not Found.  
NEG-029: Decision with invalid opportunity ID (non-existent) → 404 Not Found.  
NEG-030: Decision with opportunity ID = 0 → Validation error.

### Missing/Invalid Required Fields (NEG-031–045)

NEG-031: Go decision without confirmation checkbox → Validation error.  
NEG-032: Go decision without decision rationale → Validation error.  
NEG-033: Go decision without Executive selection → Validation error ("Executive assignment is mandatory").  
NEG-034: Go decision with empty rationale (whitespace only) → Validation error.  
NEG-035: No-Go decision without confirmation checkbox → Validation error.  
NEG-036: No-Go decision without rationale → Validation error.  
NEG-037: No-Go decision with empty rationale (whitespace only) → Validation error.  
NEG-038: Go decision with invalid ExecutiveId (non-existent user) → Validation error.  
NEG-039: Go decision with ExecutiveId of disabled user → Validation error.  
NEG-040: Go decision with ExecutiveId from wrong org unit → Validation error.  
NEG-041: Cancel without reason text → Validation error.  
NEG-042: Cancel with empty reason (whitespace only) → Validation error.  
NEG-043: Go decision with rationale containing only special characters → Validation error.  
NEG-044: Submission with all mandatory opportunity fields missing → Pre-validation blocks submit.  
NEG-045: Go decision with ExecutiveId = null in API request body → 400 Bad Request.

### Injection & Malformed Input (NEG-046–055)

NEG-046: SQL injection in decision rationale field → Sanitized, no SQL execution.  
NEG-047: XSS script in decision rationale → HTML escaped in display.  
NEG-048: SQL injection in cancel reason field → Sanitized.  
NEG-049: XSS in cancel reason → Escaped.  
NEG-050: HTML tags in confirmation statement field → Stripped/escaped.  
NEG-051: Extremely long string (100K chars) in rationale → 400 Bad Request or truncation.  
NEG-052: Null byte injection in rationale → Sanitized.  
NEG-053: CRLF injection in email notification fields → Sanitized.  
NEG-054: Path traversal in any file-related fields → Sanitized.  
NEG-055: JSON injection in API request body → Rejected.

### Notification Failures (NEG-056–065)

NEG-056: Email service unavailable during Go decision → Decision succeeds, notification queued/logged.  
NEG-057: Invalid email address for DoA2 holder → Decision succeeds, email failure logged.  
NEG-058: Email template missing for Go decision → Fallback template or graceful error.  
NEG-059: Notification service timeout → Decision not blocked by notification failure.  
NEG-060: Email bounce for CC recipient → Decision still valid, bounce logged.  
NEG-061: Notification bell service down → Decision succeeds, notification created when service recovers.  
NEG-062: Actions Required card API timeout → Card shows loading state, does not crash.  
NEG-063: Duplicate notifications for same decision → Deduplication prevents duplicates.  
NEG-064: Notification for deleted/deactivated user → Notification skipped, logged.  
NEG-065: Rate limiting on notifications → Notifications queued, not lost.

### Info Panel & Display Failures (NEG-066–075)

NEG-066: Opportunity with no partners → DD section shows "None" or is hidden.  
NEG-067: Opportunity with no risks → High Risks section hidden.  
NEG-068: Opportunity with no submitter remarks → Remarks section hidden.  
NEG-069: Time to signing with null/missing date → Shows "Not specified" or N/A.  
NEG-070: Info panel with all data missing → Panel shows gracefully with empty/N/A values.  
NEG-071: DD status calculation throws error → Panel shows "Unable to calculate" gracefully.  
NEG-072: Risk query returns error → High Risks section shows error state, doesn't crash page.  
NEG-073: Org unit with no name → Info panel handles gracefully.  
NEG-074: Initiative type not set on opportunity → Shows "Not specified".  
NEG-075: Budget amount is null → Shows "Not specified" instead of $0.

### DOA Fallback & Decision-Maker Workflow (NEG-076–090)

NEG-076: No DoA2 and no DoA3 for org unit → Error message "No decision authority found".  
NEG-077: DoA2 exists but is disabled → Falls back to DoA3 per PNO-1197.  
NEG-078: DoA3 exists but is disabled (and no DoA2) → Error "No active decision authority".  
NEG-079: DoA lookup with invalid org unit ID → Error handled gracefully.  
NEG-080: Multiple DoA2 holders on same org unit → All receive notification, first to decide wins.  
NEG-081: Go decision without valid DoA2 assignment for org unit → Error "No decision authority found".  
NEG-082: Decision-maker workflow action by user whose DoA2 role was just revoked → Access Denied.  
NEG-083: Notification request for deleted opportunity → 404 Not Found.  
NEG-084: Edit attempt on immutability-locked opportunity via API → 403 Forbidden.  
NEG-085: Executive assignment with user from deactivated org unit → Validation error.  
NEG-086: Go decision when selected Executive is concurrently deactivated → Validation error or retry prompt.  
NEG-087: Decision-maker with org unit changed mid-workflow → Access Denied on submit.  
NEG-088: No-Go decision notification sent to wrong org unit recipients → Notification scoped correctly.  
NEG-089: Immutability bypass via direct API update on GO opportunity → 403 or 400.  
NEG-090: Executive dropdown with only OiC when Director/Manager mandatory by policy → Validation or OiC accepted per rules.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 90 (= 3×30)** | ✅ COMPLIANT

### Field Length Boundaries (BND-001–020)

BND-001: Decision rationale at exactly minimum length (1 char) → Accepted.  
BND-002: Decision rationale at maximum length (e.g., 4000 chars) → Accepted.  
BND-003: Decision rationale at max+1 → Rejected or truncated.  
BND-004: Cancel reason at exactly minimum length (1 char) → Accepted.  
BND-005: Cancel reason at maximum length → Accepted.  
BND-006: Cancel reason at max+1 → Rejected.  
BND-007: Submitter remarks at maximum display length → Displayed without overflow.  
BND-008: Confirmation statement with very long org unit name (255 chars) → No truncation or overflow.  
BND-009: Executive dropdown with display name at max length → Fully visible.  
BND-010: Rationale with exactly 0 chars (empty string) → Rejected as negative test.  
BND-011: Rationale with 1 char → Accepted (minimum valid).  
BND-012: Rationale with 100 chars → Accepted (typical).  
BND-013: Rationale with 1000 chars → Accepted.  
BND-014: Rationale with 2000 chars → Accepted.  
BND-015: Rationale with 4000 chars → At boundary.  
BND-016: Rationale with mixed Unicode (Arabic, Chinese, Emoji) → Accepted, displayed correctly.  
BND-017: Rationale with RTL text (Arabic/Hebrew) → Displayed correctly with RTL layout.  
BND-018: Confirmation statement with special chars in org unit code → No rendering issues.  
BND-019: Executive name with diacritical marks → Displayed correctly.  
BND-020: Rationale with newlines and paragraphs → Preserved in display.

### Decision Timing Boundaries (BND-021–035)

BND-021: Go decision immediately after submission (< 1 second) → Accepted.  
BND-022: Go decision 30 days after submission → Accepted (no time limit).  
BND-023: Go decision at exactly session timeout boundary → Session refreshed or re-auth required.  
BND-024: Decision submitted at midnight UTC boundary → Timestamp correct.  
BND-025: Decision during DST transition → Timestamps in UTC, no ambiguity.  
BND-026: Decision on Feb 29 (leap year) → Valid date recorded.  
BND-027: Recall and resubmit within 1 second → Both operations succeed.  
BND-028: Multiple rapid page refreshes during decision dialog → Dialog state preserved.  
BND-029: Decision with very slow network (5+ second latency) → Loading spinner shown, no timeout.  
BND-030: Decision with intermittent network → Retry or clear error message.  
BND-031: Decision after browser tab inactive for 30 minutes → Token refresh or re-auth.  
BND-032: Consecutive Go/Recall/Go cycles (10 times) → All state transitions valid.  
BND-033: Decision with concurrent page navigation → Decision takes priority, navigation blocked.  
BND-034: Dialog open for extended period (1+ hour) before confirming → Still succeeds if session valid.  
BND-035: Decision at exact same millisecond as another workflow action → One succeeds, one fails gracefully.

### Data Volume Boundaries (BND-036–050)

BND-036: Opportunity with 0 partners → Info panel DD section empty/hidden.  
BND-037: Opportunity with 1 partner → DD section shows single partner.  
BND-038: Opportunity with 50 partners (max typical) → DD section scrollable/paginated.  
BND-039: Opportunity with 0 risks → Risks section hidden.  
BND-040: Opportunity with 1 risk → Shown normally.  
BND-041: Opportunity with 100 risks → Performance acceptable, list manageable.  
BND-042: Org unit with 0 Executive candidates → Dropdown empty, Go button disabled.  
BND-043: Org unit with 1 Executive candidate → Pre-selected automatically.  
BND-044: Org unit with 50 Executive candidates → Dropdown scrollable/searchable.  
BND-045: Notification bell with 0 pending decisions → No badge shown.  
BND-046: Notification bell with 1 pending decision → Badge shows "1".  
BND-047: Notification bell with 99 pending decisions → Badge shows "99".  
BND-048: Notification bell with 100+ pending decisions → Badge shows "99+" or similar.  
BND-049: Actions Required with 0 items → Section hidden or shows "No pending actions".  
BND-050: Actions Required with 50 items → List scrollable, all items accessible.

### Workflow State Boundaries (BND-051–065)

BND-051: Decision on opportunity at exact stage boundary (I&P → GO transition point) → Valid.  
BND-052: Recall at exact moment of Go decision processing → One operation succeeds.  
BND-053: Status transition from Draft to Active (submit) → Correct intermediate state.  
BND-054: Status transition from Active to Closed (No-Go) → Final state correct.  
BND-055: Reopen after No-Go → Returns to I&P/Draft.  
BND-056: Reopen after Cancel → Returns to I&P/Draft.  
BND-057: Reopen does NOT reset workflow history → Previous entries preserved.  
BND-058: Go after previous No-Go + Reopen cycle → Full history maintained.  
BND-059: Multiple recalls on same opportunity → Each recall/resubmit recorded.  
BND-060: Decision on opportunity with ID = 1 (minimum valid) → Works.  
BND-061: Decision on opportunity with ID = MAX_INT → Works if exists.  
BND-062: Stage stepper with maximum number of stages → No UI overflow.  
BND-063: Workflow history with 100+ entries → Scrollable, performance acceptable.  
BND-064: Audit trail with maximum field count → All fields recorded.  
BND-065: Opportunity with all optional fields null → Decision still processes.

### Immutability Boundaries (BND-066–080)

BND-066: Edit attempt via UI immediately after Go decision (before page refresh) → Blocked.  
BND-067: Edit attempt via API after Go decision → 403 or 400.  
BND-068: Edit attempt via API with admin role after Go decision → Still blocked (immutable).  
BND-069: Add document via API after Go → Rejected.  
BND-070: Add comment via API after Go → Rejected.  
BND-071: Modify stakeholders after Go → Rejected.  
BND-072: Change org unit after Go → Rejected.  
BND-073: Update budget after Go → Rejected.  
BND-074: Download existing document after Go → Allowed (read-only, not edit).  
BND-075: View workflow history after Go → Allowed.  
BND-076: Search/filter for GO opportunities → Returns results.  
BND-077: Export GO opportunity data → Allowed (read-only operation).  
BND-078: API bulk update including GO opportunity → GO opportunity skipped with error, others succeed.  
BND-079: Immutability on No-Go decision (same rules as Go) → All fields locked.  
BND-080: Immutability on Cancel decision (same rules as Go) → All fields locked.

### Decision-Maker & Executive Boundaries (BND-081–090)

BND-081: Go decision when DoA2 role just assigned (same second) → Correct assignment.  
BND-082: Executive reassignment attempt after Go (immutability) → Rejected.  
BND-083: Notification delivery at exact DST boundary → Timestamp correct.  
BND-084: Decision-maker with multiple org units — pending items from both → Both shown correctly.  
BND-085: Empty Executive list with fallback to OiC only → Accepts if OiC valid per policy.  
BND-086: Go decision with maximum allowed rationale length at boundary → Accepted.  
BND-087: Concurrent notification mark-as-read → No duplicate or lost updates.  
BND-088: Immutability check with soft-deleted related entity (e.g., Executive) → Graceful handling.  
BND-089: Decision workflow with org unit having no Director/Manager (OiC only) → OiC accepted.  
BND-090: Notification bell with exactly 100 pending → Badge shows "100" or "99+" per design.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 90 (= 3×30)** | ✅ COMPLIANT

### Workflow Rules (FUN-001–015)

FUN-001: Actions Required card shows correct count of pending decisions.  
FUN-002: Actions Required card item removed after decision completed.  
FUN-003: Actions Required card displays submission date correctly.  
FUN-004: Notification bell count decremented after decision.  
FUN-005: Notification marked as read after clicking.  
FUN-006: Notification text shows correct opportunity name.  
FUN-007: Instructional guidance shows for DoA2 only.  
FUN-008: Instructional guidance hides for OM viewing same opportunity.  
FUN-009: Instructional guidance hides after decision.  
FUN-010: Instructional guidance text matches PRD specification.  
FUN-011: Info panel shows correct initiative type.  
FUN-012: Info panel time-to-signing calculation correct.  
FUN-013: Info panel DD status uses CalculateDDStatus() output.  
FUN-014: Info panel high risks filter: PreDefinedHighRiskId not null OR impact contains "High".  
FUN-015: Info panel submitter remarks from workflow log comment.

### Validation Rules (FUN-016–030)

FUN-016: Go dialog requires confirmation checkbox checked.  
FUN-017: Go dialog requires non-empty rationale.  
FUN-018: Go dialog requires Executive selection.  
FUN-019: Go dialog "Confirm" button disabled until all fields valid.  
FUN-020: No-Go dialog requires confirmation checkbox.  
FUN-021: No-Go dialog requires non-empty rationale.  
FUN-022: No-Go dialog "Confirm" button disabled until all fields valid.  
FUN-023: Cancel dialog requires reason text.  
FUN-024: Recall dialog requires OM role.  
FUN-025: Recall resets opportunity to I&P/Draft.  
FUN-026: Go sets stage to GO, status remains Active.  
FUN-027: No-Go sets stage to NO GO, status to Closed.  
FUN-028: Cancel sets stage to CANCELLED, status to Closed.  
FUN-029: Reopen after No-Go sets stage to I&P, status to Draft.  
FUN-030: Reopen after Cancel sets stage to I&P, status to Draft.

### Constraint Rules (FUN-031–040)

FUN-031: Immutability enforced: all fields read-only after Go.  
FUN-032: Immutability enforced: all fields read-only after No-Go.  
FUN-033: Immutability enforced: all fields read-only after Cancel.  
FUN-034: ExecutiveId persisted in Opportunity table after Go.  
FUN-035: ExecutiveId NOT set after No-Go decision.  
FUN-036: Email sent to DoA holders on Go decision.  
FUN-037: Email CC includes OM on Go decision.  
FUN-038: Email CC includes initiator when different from OM.  
FUN-039: Email CC includes Director/Manager.  
FUN-040: Email content identical for all recipients (TO and CC).

### Audit Rules (FUN-041–050)

FUN-041: Workflow history records Go decision with timestamp.  
FUN-042: Workflow history records user who made Go decision.  
FUN-043: Workflow history records decision rationale.  
FUN-044: Workflow history records No-Go decision.  
FUN-045: Workflow history records Cancel with reason.  
FUN-046: Workflow history records Recall action.  
FUN-047: Workflow history records submission action.  
FUN-048: Audit trail captures ExecutiveId assignment.  
FUN-049: Audit trail captures stage transition.  
FUN-050: Workflow history chronologically ordered (newest first or last).

### Decision-Maker Workflow (FUN-051–060)

FUN-051: DoA2 assignment resolved from org unit EntityUserRole.  
FUN-052: DoA3 fallback when DoA2 absent (PNO-1197).  
FUN-053: Only DoA2 (or DoA3 fallback) can perform Go/No-Go.  
FUN-054: Decision-maker sees only opportunities for assigned org units.  
FUN-055: Multiple DoA2 holders receive notification; first to decide wins.  
FUN-056: Disabled DoA2 excluded from assignment; DoA3 used.  
FUN-057: DoA lookup caches result for request scope.  
FUN-058: Org unit change mid-session invalidates DoA assignment.  
FUN-059: Decision-maker role check before dialog open.  
FUN-060: Decision-maker role check before submit.

### Notifications & Immutability (FUN-061–070)

FUN-061: Go decision triggers notification to all DoA holders.  
FUN-062: Notification payload includes opportunity ID, org unit, submitter.  
FUN-063: Notification deduplication for same decision event.  
FUN-064: Immutability enforced at API layer, not just UI.  
FUN-065: Immutability applies to all opportunity fields (not just Statement).  
FUN-066: Document add blocked after Go/No-Go/Cancel.  
FUN-067: Stakeholder edit blocked after decision.  
FUN-068: Budget edit blocked after decision.  
FUN-069: Read operations (view, download, export) allowed after decision.  
FUN-070: Immutability flag persisted in database.

### Executive Assignment (FUN-071–080)

FUN-071: Executive candidates filtered by Director/Manager/OiC roles.  
FUN-072: Executive candidates scoped to opportunity org unit.  
FUN-073: ExecutiveId mandatory on Go, optional on No-Go.  
FUN-074: Executive dropdown shows display name, persists ID.  
FUN-075: Executive from wrong org unit rejected.  
FUN-076: Disabled Executive rejected.  
FUN-077: Executive assignment recorded in workflow history.  
FUN-078: Executive displayed on opportunity detail after Go.  
FUN-079: Executive search/filter in dropdown when > 20 candidates.  
FUN-080: OiC accepted when no Director/Manager (per policy).

### Go/No-Go Logic (FUN-081–090)

FUN-081: Go sets stage GO, status Active, ExecutiveId set.  
FUN-082: No-Go sets stage NO GO, status Closed, ExecutiveId null.  
FUN-083: Cancel sets stage CANCELLED, status Closed.  
FUN-084: Confirmation statement auto-generated from org unit + initiative type.  
FUN-085: Rationale required for both Go and No-Go.  
FUN-086: Confirmation checkbox required for both dialogs.  
FUN-087: Stage stepper reflects current stage after decision.  
FUN-088: Workflow actions (Approve/Reject) hidden after decision.  
FUN-089: Reopen restores editable state; previous decision preserved in history.  
FUN-090: Re-submit after Reopen creates new workflow instance.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 90 (= 3×30)** | ✅ COMPLIANT

### CRUD Workflow (INT-001–010)

INT-001: Create opportunity → Submit for Go → Go decision → Verify final state.  
INT-002: Create opportunity → Submit → No-Go → Verify stage NO GO, status Closed.  
INT-003: Create opportunity → Submit → Recall → Verify back to I&P/Draft.  
INT-004: Create opportunity → Cancel → Verify CANCELLED/Closed.  
INT-005: Create opportunity → Submit → No-Go → Reopen → Re-submit → Go.  
INT-006: Create opportunity → Cancel → Reopen → Submit → Go.  
INT-007: Go decision updates ExecutiveId in database.  
INT-008: No-Go decision does not set ExecutiveId.  
INT-009: Cancel does not set ExecutiveId.  
INT-010: Reopen clears workflow-in-progress flags.

### Search & Filter (INT-011–020)

INT-011: Search opportunities by stage "GO" → Returns approved opportunities.  
INT-012: Search by stage "NO GO" → Returns rejected opportunities.  
INT-013: Search by stage "CANCELLED" → Returns cancelled opportunities.  
INT-014: Filter by status "Active" → Includes GO opportunities.  
INT-015: Filter by status "Closed" → Includes NO GO and CANCELLED.  
INT-016: Search by Executive name → Returns opportunities assigned to that executive.  
INT-017: Sort by decision date → Correct ordering.  
INT-018: Combined filter: stage + status + org unit → Correct results.  
INT-019: Search across all stages → Returns opportunities in all states.  
INT-020: Pagination on filtered results → Correct page counts.

### Pagination (INT-021–025)

INT-021: Actions Required card pagination with > 10 items → Pages correctly.  
INT-022: Notification list pagination → Loads more on scroll.  
INT-023: Workflow history pagination → All entries accessible.  
INT-024: Executive dropdown with > 20 items → Filter/search within dropdown.  
INT-025: Risk list in info panel pagination → All risks viewable.

### Relationships (INT-026–035)

INT-026: Go decision → Opportunity → Executive relationship persisted.  
INT-027: Decision → Workflow History → User relationship correct.  
INT-028: Decision → Notifications → Recipients relationship correct.  
INT-029: Decision → Email → CC Recipients relationship correct.  
INT-030: Opportunity → Partners → DD Status calculation correct.  
INT-031: Opportunity → Risks → High Risk filter correct.  
INT-032: Opportunity → OrgUnit → DoA2 lookup correct.  
INT-033: Opportunity → OrgUnit → Executive candidates correct.  
INT-034: DoA2 fallback to DoA3 when DoA2 absent (PNO-1197).  
INT-035: Multiple opportunities for same DoA2 → All appear in Actions Required.

### Error Handling (INT-036–050)

INT-036: Database connection failure during decision → Retry with error message.  
INT-037: Transaction rollback on partial decision failure → No partial state.  
INT-038: Concurrent decision + recall → One succeeds, one returns conflict.  
INT-039: API timeout during Go decision → Clear error, no partial commit.  
INT-040: Notification service failure doesn't block decision completion.  
INT-041: Email service failure doesn't block decision completion.  
INT-042: Invalid OpportunityId in API call → 404 Not Found.  
INT-043: Malformed JSON in decision request → 400 Bad Request.  
INT-044: Missing required headers in API request → 401 Unauthorized.  
INT-045: Decision on opportunity with broken FK (orphaned org unit) → Graceful error.  
INT-046: Decision with DbContext concurrency exception → Retry logic.  
INT-047: Decision with optimistic concurrency violation → Error with retry.  
INT-048: Webhook/event failure after decision → Decision persisted, event retried.  
INT-049: Large payload in decision request (above limit) → 413 or 400.  
INT-050: Decision API response includes updated opportunity state.

### Decision-Maker Workflow Integration (INT-051–060)

INT-051: DoA2 login → Actions Required shows pending → Go decision → Success.  
INT-052: DoA3 login (no DoA2) → Actions Required shows pending → Go decision → Success.  
INT-053: DoA2 from OrgUnit A cannot see OrgUnit B opportunities in Actions Required.  
INT-054: DoA2 with multiple org units → Sees pending from all assigned org units.  
INT-055: DoA2 disabled mid-session → Next request returns 403.  
INT-056: DoA role change (DoA2 → DoA3) → Pending list updates.  
INT-057: Notification bell → DoA2 receives notification → Clicks → Navigates to opportunity.  
INT-058: OM submits → DoA2 receives notification → DoA2 decides → OM receives CC.  
INT-059: Initiator (different from OM) receives CC on Go decision.  
INT-060: Director/Manager receives CC on Go decision.

### Notifications & Email Integration (INT-061–070)

INT-061: Go decision → Email sent → All CC recipients receive.  
INT-062: No-Go decision → Email sent to OM and initiator.  
INT-063: Email template includes opportunity name, org unit, rationale.  
INT-064: Notification created in DB → Bell count incremented.  
INT-065: Notification marked read → Bell count decremented.  
INT-066: Multiple notifications for same opportunity → Deduplicated or grouped.  
INT-067: Email queue failure → Decision still persisted.  
INT-068: Notification service down → Decision succeeds, notification queued.  
INT-069: Rate limit on notifications → Queued, not dropped.  
INT-070: Notification for deleted user → Skipped, logged.

### Immutability Integration (INT-071–080)

INT-071: Go decision → PUT opportunity API → 403.  
INT-072: Go decision → Add document API → 403 or 400.  
INT-073: Go decision → Update stakeholder API → 403.  
INT-074: Go decision → Update budget API → 403.  
INT-075: Go decision → GET opportunity API → 200, read-only.  
INT-076: Go decision → Download document API → 200.  
INT-077: No-Go decision → Same immutability as Go.  
INT-078: Cancel decision → Same immutability as Go.  
INT-079: Bulk update API with GO opportunity → GO skipped, others updated.  
INT-080: Export API includes GO opportunities.

### Executive Assignment Integration (INT-081–090)

INT-081: Go decision with Executive → ExecutiveId in DB → Opportunity detail shows Executive.  
INT-082: Search by ExecutiveId → Returns assigned opportunities.  
INT-083: Executive dropdown → Org unit Director/Manager → Select → Persisted.  
INT-084: Org unit with no Director/Manager → OiC shown if available.  
INT-085: Org unit with no Executive candidates → Go disabled, validation message.  
INT-086: Executive deactivated after assignment → Opportunity still shows name (historical).  
INT-087: Executive from different org unit → Rejected at validation.  
INT-088: Multiple Executives in org unit → All shown in dropdown.  
INT-089: Executive assignment → Workflow history records assignment.  
INT-090: Executive filter on opportunity list → Correct results.

---

## §6 Security Tests — OUT OF SCOPE

> Security testing is handled by the Infrastructure and Security teams per project policy.

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two DoA2 holders submit Go decision simultaneously → Only one succeeds.  
CON-002: Go and No-Go submitted at same time by same DoA2 (two tabs) → One succeeds, one fails.  
CON-003: Go decision and Recall at exact same time → One succeeds.  
CON-004: Double-click on Confirm Go Decision button → Only one request processed.  
CON-005: Double-click on Confirm No-Go Decision → Single processing.  
CON-006: Concurrent notification reads → All marked correctly.  
CON-007: Concurrent Actions Required card updates → Correct count.  
CON-008: Decision during database migration → Graceful handling.  
CON-009: 10 users loading same pending opportunity simultaneously → All see correct state.  
CON-010: Decision commit + page refresh race → Consistent state shown.  
CON-011: Transaction isolation: decision doesn't see uncommitted data.  
CON-012: Optimistic locking on Opportunity entity during decision.  
CON-013: Email send queued concurrently for Go decision → No duplicates.  
CON-014: Concurrent notification creation → Correct count per user.  
CON-015: DbContextFactory creates separate contexts per parallel task.  
CON-016: Concurrent ExecutiveId updates → Last write wins with audit.  
CON-017: Parallel Go decisions on different opportunities → Independent processing.  
CON-018: Decision during opportunity save (OM saves while DoA2 decides) → Conflict resolution.  
CON-019: Cache invalidation after decision → Next read sees updated state.  
CON-020: Session refresh during decision submission → Decision completes.  
CON-021: Websocket notification push during decision → Notification delivered.  
CON-022: Concurrent workflow history writes → All entries persisted.  
CON-023: Parallel email CC sending → All CCs delivered.  
CON-024: Decision + audit trail write atomicity → Both succeed or both fail.  
CON-025: Multiple opportunities recalled simultaneously by same OM → All process independently.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (UNT-001–005)

UNT-001: ValidateGoDecision returns error when rationale is null.  
UNT-002: ValidateGoDecision returns error when ExecutiveId is null.  
UNT-003: ValidateGoDecision returns error when confirmation is false.  
UNT-004: ValidateNoGoDecision returns error when rationale is null.  
UNT-005: ValidateNoGoDecision returns error when confirmation is false.

### Formatting (UNT-006–008)

UNT-006: FormatConfirmationStatement includes org unit code.  
UNT-007: FormatConfirmationStatement includes initiative type.  
UNT-008: FormatDecisionEmail formats CC recipients correctly.

### Calculations (UNT-009–013)

UNT-009: CalculateTimeToSigning returns correct days from today.  
UNT-010: CalculateTimeToSigning returns "N/A" for null signing date.  
UNT-011: CalculateDDStatus returns correct aggregate across partners.  
UNT-012: FilterHighRisks returns risks with PreDefinedHighRiskId not null.  
UNT-013: FilterHighRisks returns risks with impact level containing "High".

### Status Logic (UNT-014–018)

UNT-014: GetTargetStage("Go") returns Stage.GO.  
UNT-015: GetTargetStage("NoGo") returns Stage.NO_GO.  
UNT-016: GetTargetStatus("Go") returns Status.Active.  
UNT-017: GetTargetStatus("NoGo") returns Status.Closed.  
UNT-018: IsImmutable returns true for GO, NO_GO, CANCELLED stages.

### Collections (UNT-019–021)

UNT-019: GetExecutiveCandidates returns only Director/Manager/OiC roles.  
UNT-020: GetCCRecipients includes OM + initiator + Director.  
UNT-021: GetPendingDecisions returns only GO/Active opportunities for user's org unit.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

### Single Operations (PRF-001–002)

PRF-001: Go decision API call completes in < 500ms.  
PRF-002: No-Go decision API call completes in < 500ms.

### Bulk Operations (PRF-003–005)

PRF-003: Actions Required card loads < 1s with 50 pending items.  
PRF-004: Notification list loads < 1s with 100 notifications.  
PRF-005: Executive dropdown populates < 300ms with 50 candidates.

### Search (PRF-006–010)

PRF-006: Filter by stage "GO" < 500ms with 10K opportunities.  
PRF-007: Search opportunities by Executive < 500ms.  
PRF-008: Workflow history load < 300ms with 100 entries.  
PRF-009: Info panel data load < 500ms (DD, risks, remarks aggregated).  
PRF-010: Opportunity detail page load < 2s including info panel.

### Concurrent Access (PRF-011–013)

PRF-011: 10 DoA2 users loading pending decisions simultaneously < 2s each.  
PRF-012: 50 users viewing GO opportunities simultaneously < 2s each.  
PRF-013: Decision API under 20 concurrent requests < 1s per request.

### Memory (PRF-014–016)

PRF-014: Decision processing memory stable (no leak over 100 decisions).  
PRF-015: Info panel rendering memory < 50MB for opportunity with 100 risks.  
PRF-016: Notification polling memory stable over 1 hour.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

### Sustained Load (LDT-001–003)

LDT-001: 20 concurrent Go decisions per minute for 10 minutes → All succeed, avg < 1s.  
LDT-002: 100 concurrent Actions Required card loads → 95th percentile < 2s.  
LDT-003: 50 concurrent notification queries per minute → Stable response time.

### Spike Load (LDT-004–005)

LDT-004: 100 Go decisions in 10 seconds (burst) → No failures, queue handles overflow.  
LDT-005: 200 notification reads in 5 seconds → System recovers within 10s.

### Stress Limits (LDT-006–008)

LDT-006: Determine max concurrent Go decisions before error rate > 1%.  
LDT-007: Determine max pending notifications per user before UI degrades.  
LDT-008: Email queue capacity: 500 CC emails in 1 minute → All queued.

### Recovery (LDT-009–010)

LDT-009: System recovery after Go decision overload → Normal performance within 30s.  
LDT-010: Decision processing after database connection pool exhaustion → Graceful degradation, recovery.

---

## Status: Ready for Implementation

**Next Steps:**
1. Create C# test files in `QA Tests/Integration Tests/TheGoDecision/`
2. Map each test case ID to a `[Fact]` or `[Theory]` test method
3. Implement test fixture with proper mock setup
4. Execute and record results
