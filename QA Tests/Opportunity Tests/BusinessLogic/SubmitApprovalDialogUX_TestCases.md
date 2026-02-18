# Submit/Approval Dialog UX — Comprehensive Test Cases

**Component:** Opportunity Submit → Approval Workflow — Dialogs, Locked Editing, Permissions  
**Frontend:** `opportunity-view.component`, `stage-workflow.component`, `workflow.component`, `approve-opportunity-dialog`, `reject-opportunity-dialog`  
**Backend:** `WorkflowController.cs` — Submit, Approve, Reject, Recall endpoints  
**Workflow States:** Draft → Submitted (InWorkflow) → Approved (GO) / Rejected (NO GO) / Recalled (Draft)  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

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

### Four Individual Ratio Checks

| Check | Formula | Actual | Required | Status |
|-------|---------|--------|----------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 3 × 30 | 90 ≥ 90 | ✅ PASS |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 3 × 30 | 90 ≥ 90 | ✅ PASS |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 3 × 30 | 90 ≥ 90 | ✅ PASS |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 3 × 30 | 90 ≥ 90 | ✅ PASS |

---

## Feature Overview

### Submit/Approval Workflow

```
Draft (editable)
  ↓ Submit for Go Decision
Submitted / InWorkflow (read-only, locked editing)
  ↓ Approve (Go)          ↓ Reject (No-Go)         ↓ Recall
GO (immutable)          NO GO (immutable/Closed)   Draft (editable again)
```

### Dialog Chain on Submit

1. **Non-OM Warning** — If submitter is not OM, warning dialog shown
2. **Org Unit Mismatch** — If org unit country doesn't match, mismatch dialog shown
3. **Unmet Requirements** — If stage requirements not met, requirements dialog shown
4. **Acknowledgment Dialog** — User must acknowledge and add remarks before submit
5. **Comment Dialog** — Optional comment for submission

### Locked Editing After Submit

- `canUpdate = false` from backend permissions when `isInWorkflow = true`
- Blue info box with lock icon (`pi pi-lock`) displays read-only message
- All section `[canUpdate]` bindings set to false
- `canChangeStage` remains true for Approve/Reject/Recall actions

### Dialog Components

| Dialog | Component | Purpose |
|--------|-----------|---------|
| Approve (Go) | `approve-opportunity-dialog` | Rationale, executive selection, confirmation checkbox |
| Reject (No-Go) | `reject-opportunity-dialog` | Rationale, confirmation checkbox |
| Recall | `workflow.component` (inline) | Justification textarea |
| Unmet Requirements | `workflow.component` (inline) | List of unmet stage requirements |
| Non-OM Warning | `workflow.component` (inline) | Warning about non-OM submission |
| Org Unit Mismatch | `workflow.component` (inline) | Country mismatch warning |
| Acknowledgment | `workflow.component` (inline) | Acknowledgment + remarks |

### i18n Key Reference

- `message.workflow.pendingApprovalReadOnlyInfo`: "This opportunity has been submitted for approval and is read-only while awaiting the decision..."
- `button.workflow.recall`, `message.workflow.recallTitle`, `recallQuestion`, `recallReason`, `recallSuccess`

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### Submit Flow (POS-001–010)

POS-001: OM submits opportunity with all requirements met → Submitted successfully.  
POS-002: Submit shows acknowledgment dialog → User acknowledges → Proceeds.  
POS-003: Submit with remarks → Remarks stored in workflow history.  
POS-004: Submit transitions opportunity to `isInWorkflow = true`.  
POS-005: Submit shows loading overlay during processing.  
POS-006: Submit success notification shown.  
POS-007: After submit, "Approval Pending" tag displayed.  
POS-008: After submit, Approvers tab visible.  
POS-009: After submit, read-only message with lock icon displayed.  
POS-010: Submit sends notification to DoA holders.

### Locked Editing (POS-011–020)

POS-011: `canUpdate = false` after submit → All sections read-only.  
POS-012: Edit buttons hidden on all sections after submit.  
POS-013: Form fields disabled on all sections after submit.  
POS-014: Add document button hidden after submit.  
POS-015: Add stakeholder button hidden after submit.  
POS-016: Add collaborator button hidden after submit.  
POS-017: Blue info box with lock icon (`pi pi-lock`) visible.  
POS-018: Read-only message text matches `pendingApprovalReadOnlyInfo`.  
POS-019: Read-only message translated (en/fr/es/pt).  
POS-020: `canChangeStage` remains true → Workflow actions still available.

### Approve Dialog (POS-021–027)

POS-021: Approve dialog opens when DoA2/DoA3 clicks Approve.  
POS-022: Approve dialog has rationale textarea.  
POS-023: Approve dialog has executive selection dropdown.  
POS-024: Approve dialog has confirmation checkbox.  
POS-025: Approve dialog submit button disabled until confirmation checked.  
POS-026: Approve dialog submit → Opportunity set to GO stage.  
POS-027: Approve dialog close → No action taken.

### Reject Dialog (POS-028–030)

POS-028: Reject dialog opens when DoA2/DoA3 clicks Reject.  
POS-029: Reject dialog has rationale textarea.  
POS-030: Reject dialog has confirmation checkbox.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Submit Validation Failures (NEG-001–020)

NEG-001: Submit with unmet stage requirements → Unmet requirements dialog shown.  
NEG-002: Submit with missing opportunity statement → Requirement listed.  
NEG-003: Submit with missing budget → Requirement listed.  
NEG-004: Submit with missing org unit → Requirement listed.  
NEG-005: Submit with missing stakeholders → Requirement listed.  
NEG-006: Submit without acknowledgment → Blocked.  
NEG-007: Submit without remarks → Accepted or required per config.  
NEG-008: Submit by unauthorized user → 403.  
NEG-009: Submit by unauthenticated user → 401.  
NEG-010: Submit on already-submitted opportunity → Duplicate blocked.  
NEG-011: Submit on immutable opportunity (GO/NO GO/CANCELLED) → Blocked.  
NEG-012: Non-OM submits → Warning dialog shown → Can proceed or cancel.  
NEG-013: Non-OM submits and cancels warning → No submission.  
NEG-014: Submit with org unit mismatch → Mismatch dialog shown.  
NEG-015: Submit with org unit mismatch and cancel → No submission.  
NEG-016: Submit API with malformed JSON → 400.  
NEG-017: Submit API with missing entityId → 400.  
NEG-018: Submit API with missing entityName → 400.  
NEG-019: Submit during database outage → Error, no partial state.  
NEG-020: Submit during AI service outage (auto-generate) → Submit blocked.

### Locked Editing Violations (NEG-021–040)

NEG-021: Direct API call to edit opportunity while InWorkflow → Blocked by permission.  
NEG-022: Direct API call to add document while InWorkflow → Blocked.  
NEG-023: Direct API call to add stakeholder while InWorkflow → Blocked.  
NEG-024: Direct API call to add collaborator while InWorkflow → Blocked.  
NEG-025: Direct API call to change org unit while InWorkflow → Blocked.  
NEG-026: Direct API call to change budget while InWorkflow → Blocked.  
NEG-027: Direct API call to change OM while InWorkflow → Blocked.  
NEG-028: Try to submit while already InWorkflow → Double submit blocked.  
NEG-029: Try to edit via browser developer tools → API rejects.  
NEG-030: Try to modify form data via JavaScript → UI controls disabled.  
NEG-031: `canUpdate` forced to true in client → API still rejects.  
NEG-032: UI shows edit button (if client-side bug) → API-level protection.  
NEG-033: Concurrent edit attempt by another user → Blocked.  
NEG-034: Edit attempt during workflow action processing → Blocked.  
NEG-035: Upload document via drag-and-drop while InWorkflow → Blocked.  
NEG-036: Paste content while InWorkflow → Blocked (fields disabled).  
NEG-037: Keyboard shortcut to edit while InWorkflow → Blocked.  
NEG-038: Mobile view edit attempt while InWorkflow → Blocked.  
NEG-039: Tab to editable field while InWorkflow → Field not editable.  
NEG-040: Right-click paste in field while InWorkflow → Blocked.

### Approve Dialog Failures (NEG-041–050)

NEG-041: Approve without rationale → Validation error.  
NEG-042: Approve without executive selection → Validation error.  
NEG-043: Approve without confirmation checkbox → Submit button disabled.  
NEG-044: Approve with empty rationale (whitespace) → Validation error.  
NEG-045: Approve by non-DoA user → Button not visible or API rejects.  
NEG-046: Approve by wrong DoA level → Not authorized.  
NEG-047: Approve on already-approved opportunity → Invalid state.  
NEG-048: Approve dialog submit with network error → Error, no partial state.  
NEG-049: Approve dialog submit with database error → Rollback.  
NEG-050: Double-click approve submit → Only one execution.

### Reject Dialog Failures (NEG-051–060)

NEG-051: Reject without rationale → Validation error.  
NEG-052: Reject without confirmation checkbox → Submit button disabled.  
NEG-053: Reject with empty rationale (whitespace) → Validation error.  
NEG-054: Reject by non-DoA user → Button not visible or API rejects.  
NEG-055: Reject on already-rejected opportunity → Invalid state.  
NEG-056: Reject dialog submit with network error → Error, no partial state.  
NEG-057: Reject dialog submit with database error → Rollback.  
NEG-058: Double-click reject submit → Only one execution.  
NEG-059: Reject then immediately approve → Second action blocked.  
NEG-060: Approve then immediately reject → Second action blocked.

### Recall Failures (NEG-061–070)

NEG-061: Recall without justification → Blocked (required field).  
NEG-062: Recall with empty justification (whitespace) → Blocked.  
NEG-063: Recall by non-OM/non-submitter → Button not visible or API rejects.  
NEG-064: Recall on opportunity not InWorkflow → Invalid state.  
NEG-065: Recall on approved opportunity → Invalid state (immutable).  
NEG-066: Recall on rejected opportunity → Invalid state (immutable).  
NEG-067: Recall API with malformed request → 400.  
NEG-068: Recall with network error → Error, state unchanged.  
NEG-069: Recall with database error → Rollback, state unchanged.  
NEG-070: Double-click recall submit → Only one execution.

### Acknowledgment & Requirements Dialog Failures (NEG-071–090)

NEG-071: Acknowledgment dialog closed without acknowledging → Submit blocked.  
NEG-072: Acknowledgment dialog closed via Escape → Submit cancelled.  
NEG-073: Acknowledgment dialog closed via backdrop click → Submit cancelled (if modal).  
NEG-074: Submit with acknowledgment but empty remarks when required → Blocked.  
NEG-075: Unmet requirements dialog closed without fixing → Submit blocked.  
NEG-076: Unmet requirements dialog shows stale list after fix → Re-check required.  
NEG-077: Non-OM warning dialog closed without choice → No submission.  
NEG-078: Org unit mismatch dialog closed without choice → No submission.  
NEG-079: Submit API with invalid entityId (non-existent) → 404.  
NEG-080: Submit API with entityId of wrong entity type → 400.  
NEG-081: Approve API with invalid entityId → 404.  
NEG-082: Reject API with invalid entityId → 404.  
NEG-083: Recall API with invalid entityId → 404.  
NEG-084: Submit with expired session → 401, re-auth required.  
NEG-085: Approve with expired session → 401, re-auth required.  
NEG-086: Reject with expired session → 401, re-auth required.  
NEG-087: Recall with expired session → 401, re-auth required.  
NEG-088: Submit acknowledgment with XSS in remarks → Sanitized.  
NEG-089: Approve rationale with XSS → Sanitized.  
NEG-090: Reject rationale with XSS → Sanitized.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Dialog Input Boundaries (BND-001–020)

BND-001: Approve rationale = 1 character → Accepted.  
BND-002: Approve rationale = 100 characters → Accepted.  
BND-003: Approve rationale = 1000 characters → Accepted.  
BND-004: Approve rationale = max length → Accepted.  
BND-005: Approve rationale = max+1 → Rejected or truncated.  
BND-006: Reject rationale = 1 character → Accepted.  
BND-007: Reject rationale = max length → Accepted.  
BND-008: Reject rationale = max+1 → Rejected or truncated.  
BND-009: Recall justification = 1 character → Accepted.  
BND-010: Recall justification = max length → Accepted.  
BND-011: Recall justification = max+1 → Rejected or truncated.  
BND-012: Remarks in acknowledgment dialog = 1 character → Accepted.  
BND-013: Remarks = max length → Accepted.  
BND-014: Rationale with Unicode characters → Stored correctly.  
BND-015: Rationale with emoji → Stored correctly.  
BND-016: Rationale with newlines → Preserved.  
BND-017: Rationale with HTML tags → Escaped.  
BND-018: Rationale with script tags → XSS prevented.  
BND-019: Rationale with SQL injection → Sanitized.  
BND-020: Rationale with null bytes → Sanitized.

### Workflow State Boundaries (BND-021–040)

BND-021: Submit from Draft → InWorkflow.  
BND-022: Submit from Active → InWorkflow (if allowed).  
BND-023: Submit from Closed → Blocked (invalid).  
BND-024: Approve from InWorkflow → GO.  
BND-025: Reject from InWorkflow → NO GO / Closed.  
BND-026: Recall from InWorkflow → Draft.  
BND-027: Recall immediately after submit (< 1s) → Accepted.  
BND-028: Recall 30 days after submit → Accepted (no time limit).  
BND-029: Approve immediately after submit (< 1s) → Accepted.  
BND-030: Approve 30 days after submit → Accepted.  
BND-031: Submit + immediate page close → Submit still processes.  
BND-032: Approve + immediate page close → Approve still processes.  
BND-033: Reject + immediate page close → Reject still processes.  
BND-034: Recall + immediate page close → Recall still processes.  
BND-035: `isInWorkflow = true` → Locked editing applied.  
BND-036: `isInWorkflow = false` → Editing unlocked.  
BND-037: `isApprovalPending = true` → Workflow actions available.  
BND-038: `canUpdate = false` while `canChangeStage = true` → Correct combo.  
BND-039: `canUpdate = true` while `canChangeStage = false` → Correct combo.  
BND-040: Both `canUpdate` and `canChangeStage` false → Read-only, no actions.

### Dialog UX Boundaries (BND-041–060)

BND-041: Dialog opens → Focus on first input.  
BND-042: Dialog opens → Escape key closes.  
BND-043: Dialog opens → Click outside closes (or not, per modal).  
BND-044: Dialog opens → Tab navigates between controls.  
BND-045: Dialog opens → Submit button initially disabled.  
BND-046: Dialog confirmation checked → Submit button enabled.  
BND-047: Dialog confirmation unchecked after check → Submit button disabled again.  
BND-048: Dialog submit → Loading indicator on button.  
BND-049: Dialog submit → Button disabled during processing.  
BND-050: Dialog submit success → Dialog closes automatically.  
BND-051: Dialog submit failure → Dialog stays open with error.  
BND-052: Dialog text content translated (en) → English text correct.  
BND-053: Dialog text content translated (fr) → French text correct.  
BND-054: Dialog text content translated (es) → Spanish text correct.  
BND-055: Dialog text content translated (pt) → Portuguese text correct.  
BND-056: Dialog responsive on mobile → Controls accessible.  
BND-057: Dialog responsive on tablet → Controls accessible.  
BND-058: Dialog with very long rationale → Textarea scrollable.  
BND-059: Dialog with very long requirement list → Scrollable.  
BND-060: Dialog at different zoom levels (75%-200%) → Usable.

### Overlay Boundaries (BND-061–070)

BND-061: Workflow action overlay shows during Submit → Blur effect.  
BND-062: Overlay shows during Approve → Blur effect.  
BND-063: Overlay shows during Reject → Blur effect.  
BND-064: Overlay shows during Recall → Blur effect.  
BND-065: Overlay shows during Cancel → Blur effect.  
BND-066: Overlay shows during Reopen → Blur effect.  
BND-067: Overlay hides after action completes → Page interactive.  
BND-068: Overlay hides after action fails → Page interactive.  
BND-069: Overlay prevents interaction with page → Clicks blocked.  
BND-070: Overlay z-index above all content → No clickthrough.

### Acknowledgment & Requirements Boundaries (BND-071–090)

BND-071: Acknowledgment remarks = 0 chars when optional → Accepted.  
BND-072: Acknowledgment remarks = max length → Accepted.  
BND-073: Acknowledgment remarks = max+1 → Truncated or rejected.  
BND-074: Unmet requirements list = 1 item → Dialog shows single item.  
BND-075: Unmet requirements list = 20 items → Dialog scrollable.  
BND-076: Unmet requirements list = 0 items → Proceeds to acknowledgment.  
BND-077: Non-OM warning shown when submitter ≠ OM → Correct.  
BND-078: Non-OM warning not shown when submitter = OM → Correct.  
BND-079: Org unit mismatch when country differs → Mismatch dialog.  
BND-080: Org unit match when country same → No mismatch dialog.  
BND-081: Submit button in acknowledgment disabled until checkbox → Correct.  
BND-082: Submit button in acknowledgment enabled after checkbox → Correct.  
BND-083: Executive dropdown empty (no eligible users) → Validation error.  
BND-084: Executive dropdown with 1 user → Selection required.  
BND-085: Executive dropdown with 100 users → Filterable, scrollable.  
BND-086: Confirmation checkbox unchecked → Submit disabled (Approve/Reject).  
BND-087: Confirmation checkbox checked → Submit enabled.  
BND-088: Dialog open with `workflowActionInProgress = true` → Overlay visible.  
BND-089: Multiple unmet requirements → All listed in dialog.  
BND-090: Requirement text exceeds 500 chars → Truncated or wrapped.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Submit Workflow Rules (FUN-001–015)

FUN-001: Submit checks all stage requirements before proceeding.  
FUN-002: Unmet requirements → Dialog lists all unmet items.  
FUN-003: All requirements met → Acknowledgment dialog shown.  
FUN-004: Non-OM submitter → Warning dialog shown first.  
FUN-005: Org unit mismatch → Mismatch dialog shown.  
FUN-006: Auto-generate statement if missing → Generated before submit.  
FUN-007: `AcknowledgedStatement` flag set in request.  
FUN-008: Submit API: `POST /api/workflow/submit`.  
FUN-009: Submit creates workflow task for DoA holders.  
FUN-010: Submit sends in-system notification.  
FUN-011: Submit sends email notification.  
FUN-012: Submit updates Actions Required card for DoA holders.  
FUN-013: Submit records history entry with submitter and timestamp.  
FUN-014: Submit updates `WorkflowStatus` to InProgress/InWorkflow.  
FUN-015: `workflowActionInProgress` signal → Full-page blur overlay.

### Locked Editing Rules (FUN-016–025)

FUN-016: `canUpdate` derived from `opp.permissions.canUpdate` (backend).  
FUN-017: `isInWorkflow` flag from opportunity API response.  
FUN-018: `canChangeStage` computed: `isApprovalPending || isInWorkflow` allows actions.  
FUN-019: All sections receive `[canUpdate]="canUpdate()"` input.  
FUN-020: `@if (canUpdate())` guards edit controls in templates.  
FUN-021: Lock message uses `pendingApprovalReadOnlyInfo` translation key.  
FUN-022: Lock icon is `pi pi-lock`.  
FUN-023: Lock message is blue info box (p-message severity="info").  
FUN-024: Approvers tab visible when `isInWorkflow`.  
FUN-025: "Approval Pending" tag visible when `isInWorkflow`.

### Approve Dialog Rules (FUN-026–035)

FUN-026: `customStageChangeHandler` opens approve dialog for "Approve" action.  
FUN-027: Approve dialog `visible` bound to `showApproveDialog` signal.  
FUN-028: Approve dialog `isSubmitting` signal controls loading state.  
FUN-029: Approve dialog `canSubmit` computed → Requires rationale + confirmation.  
FUN-030: Approve dialog emits `onApproveConfirmed` with rationale and executive.  
FUN-031: `onApproveConfirmed` calls `approveOpportunity` API.  
FUN-032: Approve API sets stage to GO.  
FUN-033: Approve API sets WorkflowStatus to None.  
FUN-034: Approve dialog executive dropdown loads users.  
FUN-035: Approve dialog confirmation checkbox required.

### Reject/Recall Dialog Rules (FUN-036–050)

FUN-036: Reject dialog `visible` bound to `showRejectDialog` signal.  
FUN-037: Reject dialog `isSubmitting` signal controls loading state.  
FUN-038: Reject dialog `canSubmit` → Requires rationale + confirmation.  
FUN-039: Reject dialog emits result → Sets stage to NO GO, status to Closed.  
FUN-040: Recall button visible when `workflowInfo()?.canRecall`.  
FUN-041: `canRecall = true` for submitter OR Opportunity Manager.  
FUN-042: Recall dialog opens on Recall button click.  
FUN-043: Recall dialog requires justification (non-empty).  
FUN-044: Recall dialog submit → `POST /api/workflow/recall`.  
FUN-045: Recall API returns opportunity to Draft, `isInWorkflow = false`.  
FUN-046: After recall, `canUpdate = true` → Editing unlocked.  
FUN-047: After recall, lock message hidden.  
FUN-048: After recall, "Approval Pending" tag hidden.  
FUN-049: After recall, Approvers tab hidden.  
FUN-050: `onStageChangeSuccess` reloads opportunity data.

### Acknowledgment & Requirements Rules (FUN-051–070)

FUN-051: Acknowledgment dialog shown before submit when requirements met.  
FUN-052: Acknowledgment dialog has checkbox for user confirmation.  
FUN-053: Acknowledgment dialog has remarks textarea.  
FUN-054: Acknowledgment checkbox required to enable submit.  
FUN-055: Remarks required or optional per configuration.  
FUN-056: Unmet requirements dialog lists each unmet item by name.  
FUN-057: Unmet requirements dialog blocks submit until fixed.  
FUN-058: Unmet requirements dialog has "Fix" or "Cancel" actions.  
FUN-059: Non-OM warning dialog shows when submitter is not OM.  
FUN-060: Non-OM warning dialog has "Proceed" and "Cancel" options.  
FUN-061: Org unit mismatch dialog shows when country differs.  
FUN-062: Org unit mismatch dialog has "Proceed" and "Cancel" options.  
FUN-063: Dialog chain order: Requirements → Non-OM → Mismatch → Acknowledgment.  
FUN-064: Each dialog in chain blocks next until resolved.  
FUN-065: Cancel at any dialog → No submission, opportunity unchanged.  
FUN-066: Submit button in acknowledgment disabled until checkbox checked.  
FUN-067: Submit button in acknowledgment shows loading during submit.  
FUN-068: Acknowledgment remarks stored in workflow history.  
FUN-069: Unmet requirements re-validated on re-open submit flow.  
FUN-070: Stage requirements config drives unmet list.

### Dialog State & UX Rules (FUN-071–090)

FUN-071: Approve dialog closes on successful submit.  
FUN-072: Reject dialog closes on successful submit.  
FUN-073: Recall dialog closes on successful submit.  
FUN-074: Dialog stays open on validation failure.  
FUN-075: Dialog stays open on API error.  
FUN-076: Error message displayed in dialog on API failure.  
FUN-077: `isSubmitting` prevents double submit in all dialogs.  
FUN-078: Escape key closes dialog (no action taken).  
FUN-079: Backdrop click closes dialog per modal config.  
FUN-080: Focus trapped within dialog when open.  
FUN-081: Focus returns to trigger element on close.  
FUN-082: Approve dialog executive dropdown filtered by DoA level.  
FUN-083: Reject dialog rationale required (non-empty, non-whitespace).  
FUN-084: Recall dialog justification required (non-empty, non-whitespace).  
FUN-085: All dialogs use translation keys for labels.  
FUN-086: All dialogs use translation keys for buttons.  
FUN-087: All dialogs use translation keys for error messages.  
FUN-088: Workflow actions refresh permissions after completion.  
FUN-089: Lock message hidden when `canUpdate = true`.  
FUN-090: Lock message shown when `canUpdate = false` and `isInWorkflow`.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Submit End-to-End (INT-001–015)

INT-001: Fill opportunity → Submit → Verify InWorkflow in DB.  
INT-002: Submit → Verify locked editing on page.  
INT-003: Submit → Verify lock message displayed.  
INT-004: Submit → Verify "Approval Pending" tag displayed.  
INT-005: Submit → Verify Approvers tab shows DoA holders.  
INT-006: Submit → Verify notification sent to DoA holders.  
INT-007: Submit → Verify email sent to DoA holders.  
INT-008: Submit → Verify workflow history entry created.  
INT-009: Submit → Verify all sections have `canUpdate=false`.  
INT-010: Submit → Verify edit buttons hidden on all sections.  
INT-011: Submit with unmet requirements → Dialog → Fix → Re-submit → Succeeds.  
INT-012: Submit with non-OM warning → Proceed → Succeeds.  
INT-013: Submit with org unit mismatch → Proceed → Succeeds.  
INT-014: Submit → Page refresh → Still locked.  
INT-015: Submit → Navigate away → Return → Still locked.

### Approve End-to-End (INT-016–025)

INT-016: Submit → Approve with rationale + executive → Stage=GO.  
INT-017: Approve → Verify Stage=GO in DB.  
INT-018: Approve → Verify status=Active in DB.  
INT-019: Approve → Verify opportunity immutable.  
INT-020: Approve → Verify notification sent to OM.  
INT-021: Approve → Verify email sent to OM.  
INT-022: Approve → Verify executive assignment saved.  
INT-023: Approve → Page refresh → GO stage displayed.  
INT-024: Approve → Actions Required cleared.  
INT-025: Approve → Workflow history shows approval.

### Reject End-to-End (INT-026–033)

INT-026: Submit → Reject with rationale → Stage=NO GO, Status=Closed.  
INT-027: Reject → Verify Stage=NO GO in DB.  
INT-028: Reject → Verify Status=Closed in DB.  
INT-029: Reject → Verify opportunity immutable.  
INT-030: Reject → Verify notification sent to OM.  
INT-031: Reject → Verify email sent.  
INT-032: Reject → Page refresh → NO GO displayed.  
INT-033: Reject → Workflow history shows rejection.

### Recall End-to-End (INT-034–042)

INT-034: Submit → Recall → Opportunity returns to Draft.  
INT-035: Recall → Verify `isInWorkflow = false` in DB.  
INT-036: Recall → Verify editing unlocked.  
INT-037: Recall → Verify lock message hidden.  
INT-038: Recall → Verify "Approval Pending" tag hidden.  
INT-039: Recall → Verify all sections editable.  
INT-040: Recall → Verify Actions Required cleared.  
INT-041: Recall → Re-submit → InWorkflow again.  
INT-042: Recall → Workflow history shows recall.

### Cross-Component (INT-043–050)

INT-043: Submit/Approve/Reject/Recall → Full-page overlay during action.  
INT-044: Overlay clears after action completes.  
INT-045: Submit → oUP button still visible (not affected by workflow).  
INT-046: Submit → Statement section read-only.  
INT-047: Submit → Team section read-only.  
INT-048: Submit → Documents section read-only.  
INT-049: All dialog translations work (en/fr/es/pt).  
INT-050: All workflow actions log audit trail entries.

### Acknowledgment & Requirements Integration (INT-051–070)

INT-051: Submit flow → Unmet requirements dialog → Fix → Acknowledgment → Submit.  
INT-052: Submit flow → Non-OM warning → Proceed → Acknowledgment → Submit.  
INT-053: Submit flow → Org unit mismatch → Proceed → Acknowledgment → Submit.  
INT-054: Acknowledgment remarks → Stored in workflow history.  
INT-055: Acknowledgment checkbox unchecked → Submit disabled.  
INT-056: Acknowledgment checkbox checked → Submit enabled.  
INT-057: Unmet requirements dialog → Cancel → No submission.  
INT-058: Non-OM warning → Cancel → No submission.  
INT-059: Org unit mismatch → Cancel → No submission.  
INT-060: Acknowledgment → Cancel → No submission.  
INT-061: Submit with all dialogs in chain → Single successful submission.  
INT-062: Submit → Verify `AcknowledgedStatement` in API request.  
INT-063: Unmet requirements → Fix one → Re-open → Remaining listed.  
INT-064: Unmet requirements → Fix all → Acknowledgment shown.  
INT-065: Requirements config change → Unmet list reflects new config.  
INT-066: Acknowledgment with long remarks → Stored correctly.  
INT-067: Acknowledgment with special chars in remarks → Escaped.  
INT-068: Submit from different locales → Dialogs translated.  
INT-069: Submit → Notification includes submitter and remarks.  
INT-070: Submit → Workflow history includes acknowledgment details.

### Dialog Chain & Lock Integration (INT-071–090)

INT-071: Approve dialog → Submit → Lock released, GO stage.  
INT-072: Reject dialog → Submit → Lock released, NO GO stage.  
INT-073: Recall dialog → Submit → Lock released, Draft stage.  
INT-074: Approve dialog open → Overlay visible.  
INT-075: Reject dialog open → Overlay visible.  
INT-076: Recall dialog open → Overlay visible.  
INT-077: Acknowledgment dialog open → Overlay visible during submit.  
INT-078: Unmet requirements dialog → No overlay (pre-submit).  
INT-079: Lock message → Refreshes after recall.  
INT-080: Lock message → Persists after page refresh when InWorkflow.  
INT-081: Edit buttons → Reappear after recall.  
INT-082: Add document button → Reappears after recall.  
INT-083: Add stakeholder button → Reappears after recall.  
INT-084: Approvers tab → Hidden after recall.  
INT-085: Approvers tab → Visible after re-submit.  
INT-086: Submit → Approve → Verify executive in DB.  
INT-087: Submit → Reject → Verify rationale in workflow history.  
INT-088: Submit → Recall → Verify justification in workflow history.  
INT-089: Multiple workflow actions in sequence → Correct final state.  
INT-090: Permission endpoint returns `canUpdate=false` when InWorkflow.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users submit same opportunity → One succeeds, one blocked.  
CON-002: Submit + approve simultaneously → Submit must complete first.  
CON-003: Submit + reject simultaneously → Submit must complete first.  
CON-004: Approve + reject simultaneously → One succeeds, one blocked.  
CON-005: Approve + recall simultaneously → One succeeds, one blocked.  
CON-006: Reject + recall simultaneously → One succeeds, one blocked.  
CON-007: Double-click submit button → Only one submission.  
CON-008: Double-click approve button → Only one approval.  
CON-009: Double-click reject button → Only one rejection.  
CON-010: Double-click recall button → Only one recall.  
CON-011: Submit from two browser tabs → One succeeds.  
CON-012: Approve from two browser tabs → One succeeds.  
CON-013: Edit attempt during concurrent submit → Edit blocked.  
CON-014: Edit attempt during concurrent approve → Edit blocked.  
CON-015: Submit during page refresh → One action completes.  
CON-016: Approve during page refresh → One action completes.  
CON-017: Concurrent notification sends → No duplicate notifications.  
CON-018: Concurrent email sends → No duplicate emails.  
CON-019: Concurrent workflow history writes → All recorded.  
CON-020: Submit + concurrent opportunity save → Submit takes precedence.  
CON-021: Recall + concurrent edit → Recall completes, then edit allowed.  
CON-022: Lock message display + concurrent unlock → Correct final state.  
CON-023: Overlay display + concurrent completion → Overlay clears.  
CON-024: Dialog open + concurrent state change → Dialog shows updated state.  
CON-025: Multiple approvers viewing simultaneously → Both see pending state.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `canUpdate` computed from `permissions.canUpdate` → Correct.  
UNT-002: `canChangeStage` computed → True when `isApprovalPending || isInWorkflow`.  
UNT-003: `isInWorkflow` flag parsing from API → Correct boolean.  
UNT-004: `showApproveDialog` signal default → false.  
UNT-005: `showRejectDialog` signal default → false.  
UNT-006: `workflowActionInProgress` signal default → false.  
UNT-007: `customStageChangeHandler("Approve")` → Sets `showApproveDialog = true`.  
UNT-008: `customStageChangeHandler("Reject")` → Sets `showRejectDialog = true`.  
UNT-009: Approve dialog `canSubmit` → Requires rationale + confirmation.  
UNT-010: Approve dialog `canSubmit` false without confirmation → Correct.  
UNT-011: Reject dialog `canSubmit` → Requires rationale + confirmation.  
UNT-012: Reject dialog `canSubmit` false without confirmation → Correct.  
UNT-013: Recall dialog justification validation → Non-empty required.  
UNT-014: `pendingApprovalReadOnlyInfo` translation key exists.  
UNT-015: Lock icon class → `pi pi-lock`.  
UNT-016: Overlay blur effect applied correctly.  
UNT-017: `onStageChangeSuccess` triggers data reload.  
UNT-018: `isSubmitting` signal prevents multiple submissions.  
UNT-019: `approveDialogResolver` callback pattern → Correct.  
UNT-020: `canRecall` → True for submitter or OM.  
UNT-021: `canRecall` → False for other users.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Submit API call → < 3s total (including auto-generate).  
PRF-002: Approve API call → < 1s.  
PRF-003: Reject API call → < 1s.  
PRF-004: Recall API call → < 1s.  
PRF-005: Dialog open → < 200ms.  
PRF-006: Dialog close → < 100ms.  
PRF-007: Overlay show → < 50ms.  
PRF-008: Overlay hide → < 50ms.  
PRF-009: Permission check for `canUpdate` → < 100ms.  
PRF-010: Lock message render → < 50ms.  
PRF-011: Section `canUpdate` binding evaluation → < 10ms per section.  
PRF-012: Notification send (async) → < 500ms.  
PRF-013: Email send (async) → < 1s.  
PRF-014: Workflow history write → < 100ms.  
PRF-015: Page reload after workflow action → < 1s.  
PRF-016: Concurrent workflow actions under load → < 3s each.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 20 concurrent submit requests (different opportunities) → All succeed.  
LDT-002: 10 concurrent approve requests → All succeed.  
LDT-003: 10 concurrent reject requests → All succeed.  
LDT-004: 10 concurrent recall requests → All succeed.  
LDT-005: 50 concurrent page loads of submitted (locked) opportunities → All render.  
LDT-006: 100 concurrent permission checks → All return correct `canUpdate`.  
LDT-007: Spike: 20 submits in 5 seconds → All processed.  
LDT-008: Sustained workflow actions (50/hour) → Stable.  
LDT-009: Recovery after workflow API failure → Retry succeeds.  
LDT-010: Recovery after notification service failure → Notifications queued.

---

## Status: Ready for Implementation
