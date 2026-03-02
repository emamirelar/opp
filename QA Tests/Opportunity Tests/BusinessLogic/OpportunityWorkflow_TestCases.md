# Opportunity Workflow (State Transitions, Approvals, Escalation) — Comprehensive Test Cases

**Feature:** Opportunity workflow engine — state transitions, approval chains, escalation rules, notifications, validation  
**Created:** 2026-01-24  
**Restructured:** 2026-02-11 (10-category standard)  
**Updated:** 2026-02-18 (mandatory ratio corrections)  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (N≥3P, E≥3P, F≥3P, I≥3P)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30=90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30=90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30=90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30=90 | ✅ |
| 6 | Security Tests | §6 | 50 | ≥50 | ✅ |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **462** | ✅ |

### Mandatory Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### State Transitions (12)

| ID | From → To | Trigger | Expected | Priority |
|----|-----------|---------|----------|----------|
| POS-001 | I&P/Draft → GO/Active | OM Submit | Stage=GO, Status=Active, workflow created | P0 |
| POS-002 | GO/Active → GO/Closed | DoA2 Approve | Approved, permanently read-only | P0 |
| POS-003 | GO/Active → NO GO/Closed | DoA2 Reject | Stage=NO GO, reason recorded | P0 |
| POS-004 | I&P/Draft → CANCELLED/Closed | OM Cancel | Cancelled, reason required | P0 |
| POS-005 | CANCELLED/Closed → I&P/Draft | OM Reopen | Editable again | P0 |
| POS-006 | NO GO/Closed → I&P/Draft | OM Reopen | Editable, can resubmit | P0 |
| POS-007 | GO/Active → I&P/Draft | OM Recall | Returned to draft, editable | P0 |
| POS-008 | Full cycle: Submit → Reject → Reopen → Submit → Approve | OM + DoA2 | All transitions succeed | P0 |
| POS-009 | Full cycle: Cancel → Reopen → Submit → Approve | OM + DoA2 | All transitions succeed | P0 |
| POS-010 | Multiple cycles (3+) | Repeated submit/reject/reopen | All recorded correctly | P1 |
| POS-011 | Transition preserves all opportunity data | Submit + Approve | Data intact after workflow | P0 |
| POS-012 | Transition from mobile browser | Submit on mobile | Works correctly | P2 |

### Approval Workflow (10)

| ID | Test Name | Steps | Expected | Priority |
|----|-----------|-------|----------|----------|
| POS-013 | DoA2 receives pending decision | Submit opp | DoA2 sees in pending list | P0 |
| POS-014 | DoA2 approves with comments | Approve + enter comment | Comments saved in history | P0 |
| POS-015 | DoA2 rejects with reason | Reject + enter reason | Reason saved, OM notified | P0 |
| POS-016 | OM sees approval result | After DoA2 decision | OM sees new stage + notification | P1 |
| POS-017 | Approval chain (DoA2 → DoA3 if needed) | High-value opp | Routes through chain | P1 |
| POS-018 | Delegation: delegate approves on behalf | DoA2 delegates | Delegate can approve | P1 |
| POS-019 | Approval SLA tracking | Submit → wait | SLA timer visible/tracked | P2 |
| POS-020 | Multiple approvers for same org unit | 2 DoA2 holders | Either can approve | P1 |
| POS-021 | Approval after previous rejection | Reopened opp resubmitted | New approval cycle | P1 |
| POS-022 | Approval history shows all cycles | Multiple cycles | Complete history | P1 |

### Notifications (8)

| ID | Test Name | Trigger | Expected | Priority |
|----|-----------|---------|----------|----------|
| POS-023 | Email to DoA2 on submit | OM submits | DoA2 gets email with opp details | P0 |
| POS-024 | Email to OM on approval | DoA2 approves | OM gets approval email | P0 |
| POS-025 | Email to OM on rejection | DoA2 rejects | OM gets rejection email + reason | P0 |
| POS-026 | Email on recall | OM recalls | DoA2 gets recall notification | P1 |
| POS-027 | In-app notification on submit | OM submits | DoA2 sees in-app alert | P1 |
| POS-028 | Stakeholder notification on GO | Approved | Stakeholders notified | P1 |
| POS-029 | Notification includes opp details | Any notification | Name, stage, action visible | P1 |
| POS-030 | Notification link navigates to opp | Click email link | Opens opp detail | P1 |

---

## §2 Negative Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 2.1 Invalid Transitions (15)

| ID | From | Action | Expected | Priority |
|----|------|--------|----------|----------|
| NEG-001 | I&P/Draft | Approve | Not available | P0 |
| NEG-002 | I&P/Draft | Reject (DoA2) | Not available | P0 |
| NEG-003 | GO/Active | Cancel | Blocked (in workflow) | P0 |
| NEG-004 | GO/Closed | Submit | Decided, not available | P0 |
| NEG-005 | GO/Closed | Cancel | Not available | P0 |
| NEG-006 | NO GO/Closed | Submit directly | Must reopen first | P0 |
| NEG-007 | CANCELLED/Closed | Approve | Not available | P1 |
| NEG-008 | GO/Active | Reopen | Must recall, not reopen | P1 |
| NEG-009 | I&P/Draft | Recall | Not submitted | P1 |
| NEG-010 | GO/Closed | Reopen (approved) | Final decision | P0 |
| NEG-011 | Skip stage: I&P → GO/Closed | Direct API | Rejected | P0 |
| NEG-012 | Skip stage: I&P → NO GO | Direct API | Rejected | P0 |
| NEG-013 | Reverse: GO → I&P (without recall) | Direct API | Rejected | P1 |
| NEG-014 | Set stage directly via PUT | Stage field | Ignored | P0 |
| NEG-015 | Set status directly via PUT | Status field | Ignored | P0 |

### 2.2 Authorization Failures (15)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| NEG-016 | Collaborator | Submit | Denied | P0 |
| NEG-017 | Collaborator | Cancel | Denied | P0 |
| NEG-018 | Collaborator | Recall | Denied | P0 |
| NEG-019 | Collaborator | Reopen | Denied | P0 |
| NEG-020 | General User | Any action | Denied | P0 |
| NEG-021 | OM of different opp | Submit this opp | 403 | P0 |
| NEG-022 | DoA2 of different org | Approve this opp | 403 | P0 |
| NEG-023 | Non-DoA2 | Approve | 403 | P0 |
| NEG-024 | Non-DoA2 | Reject | 403 | P0 |
| NEG-025 | Unauthenticated | Any | 401 | P0 |
| NEG-026 | Expired session | Submit | 401 | P1 |
| NEG-027 | Tampered JWT | Approve | 401 | P1 |
| NEG-028 | Revoked permissions | Submit | 403 | P1 |
| NEG-029 | Partner User | Any | Not available | P1 |
| NEG-030 | Deactivated user | Any | Blocked | P1 |

### 2.3 Validation Failures (10)

| ID | Missing/Invalid | Action | Expected | Priority |
|----|----------------|--------|----------|----------|
| NEG-031 | Cancel reason empty | Cancel | Blocked | P0 |
| NEG-032 | Reject reason empty | Reject | Blocked | P0 |
| NEG-033 | Recall justification empty | Recall | Blocked | P0 |
| NEG-034 | Acknowledgement unchecked | Submit | Disabled | P0 |
| NEG-035 | No DoA2 for org unit | Submit | Blocked with error | P0 |
| NEG-036 | Missing mandatory opp fields | Submit | Validation list | P0 |
| NEG-037 | No Opportunity Statement | Submit | Blocked | P0 |
| NEG-038 | Reason = whitespace only | Cancel | Blocked | P1 |
| NEG-039 | Reason > max length | Any | Truncated/rejected | P1 |
| NEG-040 | Invalid workflow ID in API | POST with bad ID | 404 | P1 |

### 2.4 Approval Chain Failures (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-041 | DoA2 not configured for org unit | Submit blocked | P0 |
| NEG-042 | All DoA2 holders deactivated | Clear error message | P1 |
| NEG-043 | Delegation expired | Error: delegation expired | P1 |
| NEG-044 | Circular delegation chain | Handle gracefully | P2 |
| NEG-045 | Escalation with no higher authority | Error or stop at last level | P1 |
| NEG-046 | SLA breach with no escalation config | Warning logged | P2 |
| NEG-047 | Approval after DoA2 role removed | 403 | P1 |
| NEG-048 | Double approval (already decided) | Second ignored/409 | P0 |
| NEG-049 | Approve + Reject simultaneously | One wins | P0 |
| NEG-050 | Delegate approves after principal decides | Second blocked | P1 |

### 2.5 Notification Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-051 | Email service down | Transition succeeds, notification queued | P1 |
| NEG-052 | DoA2 email invalid | Notification error logged, transition succeeds | P1 |
| NEG-053 | OM email invalid | Error logged, decision recorded | P1 |
| NEG-054 | Notification template missing | Fallback text or error | P2 |
| NEG-055 | Duplicate notification | Deduplication applies | P1 |
| NEG-056 | Notification to deleted user | Skipped gracefully | P1 |
| NEG-057 | In-app notification fails | Email still sent | P2 |
| NEG-058 | Notification link expired | Redirect to login then opp | P1 |
| NEG-059 | Notification for deleted opportunity | Not sent or indicates deleted | P1 |
| NEG-060 | Bulk notification failure | Partial delivery, retry queued | P2 |

### 2.6 System/API Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-061 | DB timeout during transition | Rollback, error message | P1 |
| NEG-062 | Workflow engine crash | Transaction rolled back | P1 |
| NEG-063 | Network disconnect mid-transition | No partial state | P1 |
| NEG-064 | API malformed JSON | 400 Bad Request | P1 |
| NEG-065 | API missing required body | 400 | P1 |
| NEG-066 | API exceeds rate limit | 429 | P2 |
| NEG-067 | Concurrent modification conflict | 409 | P1 |
| NEG-068 | Service unavailable (maintenance) | 503 | P2 |
| NEG-069 | Payload too large | 413 | P2 |
| NEG-070 | Auth service timeout | Error, no partial change | P1 |

### 2.7 Workflow State & Data Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-071 | Submit opp with soft-deleted partner | 400 or blocked | P1 |
| NEG-072 | Submit opp with soft-deleted org unit | Blocked, clear error | P1 |
| NEG-073 | Approve with invalid workflow instance ID | 404 | P1 |
| NEG-074 | Recall when workflow already decided | 409 | P0 |
| NEG-075 | Reopen when opp not in closed state | 400 | P1 |
| NEG-076 | Submit with expired acknowledgement timestamp | Blocked | P1 |
| NEG-077 | Transition with mismatched opportunity version | 409 | P1 |
| NEG-078 | Approve with invalid DoA2 org unit mapping | 403 | P1 |
| NEG-079 | Cancel with reason containing only control chars | Blocked | P1 |
| NEG-080 | Submit when mandatory stakeholder missing | Validation error | P0 |

### 2.8 Escalation & Delegation Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-081 | Escalation to non-existent authority | Error, fallback logged | P1 |
| NEG-082 | Delegate approves after delegation revoked | 403 | P1 |
| NEG-083 | Submit when DoA2 on leave, no delegate | Blocked or escalation | P1 |
| NEG-084 | Escalation config missing for org unit | Default path or error | P1 |
| NEG-085 | Approve with inactive DoA2 role | 403 | P1 |
| NEG-086 | Recall during escalation handoff | One wins, consistent | P0 |
| NEG-087 | SLA breach escalation with disabled escalation | Warning only | P2 |
| NEG-088 | Delegate chain exceeds max depth | Rejected or truncated | P2 |
| NEG-089 | Approve with wrong escalation level | 403 | P1 |
| NEG-090 | Submit when org unit has no escalation path | Blocked or normal flow | P1 |

---

## §3 Boundary Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 3.1 Text Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over | Priority |
|----|-------|-----|-----|--------|--------|------|----------|
| BND-001 | Cancel reason | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Reject reason | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Recall justification | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-004 | Approval comments | 0 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-005 | Reason = 1 char | ✅ | — | — | — | — | P2 |
| BND-006 | Reason = 2000 chars | — | ✅ | — | — | — | P2 |
| BND-007 | Reason = 2001 chars | — | — | — | — | ❌ | P2 |
| BND-008 | Reason with newlines | ✅ preserved | — | — | — | — | P2 |
| BND-009 | Reason with Unicode | ✅ | — | — | — | — | P1 |
| BND-010 | Reason with HTML | Escaped | — | — | — | — | P1 |
| BND-011 | Reason with SQL | Parameterized | — | — | — | — | P1 |
| BND-012 | Reason with emoji | ✅ or ❌ | — | — | — | — | P2 |
| BND-013 | Reason with diacritics | ✅ | — | — | — | — | P1 |
| BND-014 | Multiple paragraphs | ✅ | — | — | — | — | P2 |
| BND-015 | Comment = only whitespace | Trimmed to empty | — | — | — | — | P2 |

### 3.2 Value/Budget Thresholds (10)

| ID | Value Threshold | Expected Behavior | Priority |
|----|----------------|-------------------|----------|
| BND-016 | Budget = $0 | Normal workflow (no escalation) | P1 |
| BND-017 | Budget = $4,999,999 | Normal workflow | P1 |
| BND-018 | Budget = $5,000,000 (threshold) | Escalation triggered | P0 |
| BND-019 | Budget = $5,000,001 | Escalation triggered | P0 |
| BND-020 | Budget = $100 (very small) | Normal workflow | P1 |
| BND-021 | Budget = $999,999,999 (very large) | Escalation chain, handled | P1 |
| BND-022 | Budget changed after submit | New budget not retroactive | P1 |
| BND-023 | Multiple currencies at threshold | Currency conversion checked | P2 |
| BND-024 | Budget = exactly escalation boundary | Consistent behavior (≥ or >) | P1 |
| BND-025 | No budget set (null) | Normal workflow or warning | P1 |

### 3.3 Timing Boundaries (10)

| ID | Timing | Expected | Priority |
|----|--------|----------|----------|
| BND-026 | Submit at midnight UTC | Correct date | P1 |
| BND-027 | Approve 1ms after submit | Both timestamps distinct | P1 |
| BND-028 | SLA = exactly at deadline | Escalation triggered or not (consistent) | P1 |
| BND-029 | SLA = 1 second before deadline | Not escalated | P1 |
| BND-030 | SLA = 1 second after deadline | Escalated | P1 |
| BND-031 | Transition during DST change | UTC correct | P2 |
| BND-032 | Transition at year boundary | Correct year | P2 |
| BND-033 | Approval after 30-day wait | Still valid | P1 |
| BND-034 | Recall at exact moment DoA2 approves | One wins | P0 |
| BND-035 | Multiple transitions within 1 second | All recorded distinctly | P1 |

### 3.4 Workflow History Boundaries (10)

| ID | History State | Expected | Priority |
|----|-------------|----------|----------|
| BND-036 | 0 entries | Empty display | P1 |
| BND-037 | 1 entry | Single row | P1 |
| BND-038 | 100 entries | Paginated/scrollable | P1 |
| BND-039 | 1000+ entries | Performance OK | P2 |
| BND-040 | Entry with max-length reason | Truncated + "show more" | P2 |
| BND-041 | Entry from deleted user | User shown as "Deleted User" or name preserved | P1 |
| BND-042 | Entry from renamed user | Current name shown | P2 |
| BND-043 | History across multiple workflow cycles | All cycles visible | P1 |
| BND-044 | Concurrent history reads during write | Consistent | P2 |
| BND-045 | History export with 500+ entries | Export succeeds | P2 |

### 3.5 Escalation Boundaries (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-046 | 1 level of escalation | Normal chain | P1 |
| BND-047 | 5 levels of escalation | Full chain traversed | P1 |
| BND-048 | Escalation timeout = 0 days | Immediate escalation | P2 |
| BND-049 | Escalation timeout = 365 days | Very long wait | P2 |
| BND-050 | All escalation levels unavailable | Final error/fallback | P1 |
| BND-051 | Escalation with delegation active | Delegation respected | P1 |
| BND-052 | De-escalation (if supported) | Correct routing | P2 |
| BND-053 | Escalation for exactly $5M | Consistent boundary behavior | P1 |
| BND-054 | Re-escalation after recall and resubmit | New escalation cycle | P1 |
| BND-055 | Escalation notification queue full | Backpressure handled | P2 |

### 3.6 Multi-Opportunity Boundaries (15)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-056 | OM submits 5 opps simultaneously | All independent workflows | P1 |
| BND-057 | DoA2 has 100 pending decisions | All actionable | P1 |
| BND-058 | Same opp submitted/recalled 10 times | All cycles recorded | P1 |
| BND-059 | Different opps at every possible stage | Each in correct state | P1 |
| BND-060 | Approval of last pending decision | Counter updates to 0 | P2 |
| BND-061 | Notification for 50 opps simultaneously | All delivered | P1 |
| BND-062 | Workflow engine processing 200 transitions/min | All complete | P2 |
| BND-063 | History query spanning all opps | Filtered correctly | P2 |
| BND-064 | OM with 0 opps | Empty list | P2 |
| BND-065 | Transition on opp with max data | Succeeds | P1 |
| BND-066 | Transition on opp with min data | Succeeds | P1 |
| BND-067 | Workflow for opp in every supported currency | Currency doesn't affect workflow | P2 |
| BND-068 | Transition for opp with 0 collaborators | Works (OM only) | P1 |
| BND-069 | Transition for opp with 50 stakeholders | All notified | P2 |
| BND-070 | First-ever transition in fresh system | Works without prior data | P1 |

### 3.7 Stage/Status Enum Boundaries (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-071 | Transition at I&P/Draft → first submit | Workflow created | P1 |
| BND-072 | Transition at GO/Active → last possible action | Recall or decision | P1 |
| BND-073 | Stage enum: unknown value in API | 400 | P1 |
| BND-074 | Status enum: null vs empty | Consistent handling | P2 |
| BND-075 | WorkflowStatus at boundary (Pending→Decided) | Atomic update | P0 |
| BND-076 | All 4 stage values exercised in one opp | Full cycle | P1 |
| BND-077 | Reopen from CANCELLED vs NO GO | Same behavior | P1 |
| BND-078 | Recall from exactly 1 second in workflow | Succeeds | P1 |
| BND-079 | Approve at exact SLA deadline | Decision recorded | P1 |
| BND-080 | Submit with 0 optional fields | Minimal valid opp | P1 |

### 3.8 API & Payload Boundaries (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-081 | Reason at exactly 2000 chars | Accepted | P1 |
| BND-082 | Reason at 1999 chars | Accepted | P1 |
| BND-083 | Empty array for optional IDs | Handled | P2 |
| BND-084 | Max allowed collaborators (e.g. 20) | All notified | P2 |
| BND-085 | Pagination: page=0 vs page=1 | Consistent | P2 |
| BND-086 | Pagination: page size = 1 | Single result | P2 |
| BND-087 | Pagination: page size = max (e.g. 100) | All returned | P2 |
| BND-088 | Filter with empty criteria | All or default | P2 |
| BND-089 | Date range: same start and end | Single day | P2 |
| BND-090 | Bulk action with 1 item | Same as single | P2 |

---

## §4 Functional Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 4.1 Workflow Rules (15)

| ID | Rule | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| FUN-001 | Only matrix-valid transitions allowed | Each transition | Enforced | P0 |
| FUN-002 | Cancel only from I&P, not in workflow | Cancel during GO/Active | Blocked | P0 |
| FUN-003 | Recall returns to I&P/Draft | OM recalls | State restored | P0 |
| FUN-004 | Approval is final | After GO/Closed | No actions | P0 |
| FUN-005 | Read-only during workflow | GO/Active | Fields disabled | P0 |
| FUN-006 | Read-only after decision | GO/Closed or NO GO | Permanent | P0 |
| FUN-007 | Editable after reopen | Reopened I&P | Fields enabled | P0 |
| FUN-008 | Each transition requires confirmation | Any action | Dialog shown | P0 |
| FUN-009 | Reason mandatory for cancel/reject/recall | Missing reason | Blocked | P0 |
| FUN-010 | Acknowledgement mandatory for submit | Unchecked | Blocked | P0 |
| FUN-011 | Workflow instance created on submit | First submit | Instance exists | P1 |
| FUN-012 | Workflow instance reused on resubmit | Reopened → resubmit | Same or new instance | P1 |
| FUN-013 | In-workflow flag set/unset | Submit sets, decision unsets | Correct | P1 |
| FUN-014 | Stage stepper reflects transitions | Each change | Stepper updates | P1 |
| FUN-015 | History entry created for each transition | Every action | Entry added | P0 |

### 4.2 Validation Rules (15)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-016 | All mandatory fields before submit | Missing fields | Error list | P0 |
| FUN-017 | DoA2 exists for org unit | No DoA2 | Blocked | P0 |
| FUN-018 | Statement generated | No statement | Blocked | P0 |
| FUN-019 | Only OM can submit/cancel/recall | Non-OM | Hidden | P0 |
| FUN-020 | Only DoA2 can approve/reject | Non-DoA2 | Hidden | P0 |
| FUN-021 | Reason non-blank | Whitespace | Rejected | P1 |
| FUN-022 | Reason within max length | Over limit | Rejected | P1 |
| FUN-023 | Server validates (bypass client) | API call | Server rejects | P0 |
| FUN-024 | Stage cannot be set directly | API PUT | Ignored | P0 |
| FUN-025 | Status cannot be set directly | API PUT | Ignored | P0 |
| FUN-026 | Deleted opp cannot transition | Soft-deleted | 404 | P1 |
| FUN-027 | One active workflow per opp | Double submit | Blocked | P1 |
| FUN-028 | Approval requires submitted state | Pre-submit | N/A | P1 |
| FUN-029 | Value threshold for escalation | Budget check | Correct routing | P1 |
| FUN-030 | All errors shown at once | 5 issues | All listed | P0 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Expected | Priority |
|----|-----------|----------|----------|
| FUN-031 | Atomic transitions | No partial state | P0 |
| FUN-032 | Audit immutability | Cannot modify history | P1 |
| FUN-033 | Notification failure ≠ transition failure | Email fails, transition succeeds | P1 |
| FUN-034 | Escalation chain respects hierarchy | Level 2 → 3 → OIC | P1 |
| FUN-035 | SLA calculation based on business days (if configured) | Weekends excluded | P2 |
| FUN-036 | Delegation respects date range | Active delegation only | P1 |
| FUN-037 | Concurrent transition safety | Optimistic concurrency | P0 |
| FUN-038 | Status enum validation | Invalid string | Rejected | P1 |
| FUN-039 | Max workflow cycles (if configured) | Limit enforcement | P2 |
| FUN-040 | Soft-deleted opps excluded from queries | Not returned | P1 |

### 4.4 Audit Rules (10)

| ID | Audit | Test | Expected | Priority |
|----|-------|------|----------|----------|
| FUN-041 | Submit action logged | Submit | User, time, action, DoA2 target | P0 |
| FUN-042 | Cancel action logged | Cancel | User, time, reason | P0 |
| FUN-043 | Approve action logged | Approve | User, time, comments | P0 |
| FUN-044 | Reject action logged | Reject | User, time, reason | P0 |
| FUN-045 | Recall action logged | Recall | User, time, justification | P0 |
| FUN-046 | Reopen action logged | Reopen | User, time | P1 |
| FUN-047 | Escalation logged | Escalate | From/To level, time | P1 |
| FUN-048 | Notification delivery logged | Email sent | Recipient, time, status | P1 |
| FUN-049 | Failed transition logged | Failed attempt | Reason for failure | P2 |
| FUN-050 | All entries include from/to state | Any transition | Before + after state | P0 |

### 4.5 Notification Rules (10)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-051 | Notification sent on submit | Submit | DoA2 notified | P0 |
| FUN-052 | Notification sent on approve | Approve | OM notified | P0 |
| FUN-053 | Notification sent on reject | Reject | OM notified | P0 |
| FUN-054 | Notification sent on recall | Recall | DoA2 notified | P1 |
| FUN-055 | Notification includes correct action | Any | Action matches trigger | P1 |
| FUN-056 | Notification respects user preferences | Opt-out | Not sent if disabled | P2 |
| FUN-057 | In-app + email both sent (if configured) | Submit | Both delivered | P1 |
| FUN-058 | Notification deduplication | Same event twice | Single notification | P1 |
| FUN-059 | Notification for escalation | Escalate | Next level notified | P1 |
| FUN-060 | Notification link includes opp ID | Click | Correct opp opened | P1 |

### 4.6 Escalation Rules (10)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-061 | Value threshold triggers escalation | Budget ≥$5M | Escalation path used | P1 |
| FUN-062 | Time threshold triggers escalation | SLA breach | Escalation sent | P1 |
| FUN-063 | Escalation chain order | DoA2→DoA3→OIC | Correct sequence | P1 |
| FUN-064 | Escalation skips unavailable | DoA2 absent | Next level | P1 |
| FUN-065 | Delegation respected in escalation | Delegate active | Delegate notified | P1 |
| FUN-066 | Escalation logged | Any escalation | Audit entry | P1 |
| FUN-067 | Escalation reminder before deadline | Configurable | Reminder sent | P2 |
| FUN-068 | No escalation below threshold | Budget <$5M | Normal flow | P1 |
| FUN-069 | Escalation cleared on decision | Approve/Reject | Escalation stops | P1 |
| FUN-070 | Re-escalation on resubmit | Reopen→Submit | New cycle | P1 |

### 4.7 UI/UX Workflow Rules (10)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-071 | Submit button disabled when invalid | Missing fields | Disabled | P0 |
| FUN-072 | Recall button visible only in workflow | GO/Active | Visible | P0 |
| FUN-073 | Approve/Reject visible only to DoA2 | Pending list | Correct visibility | P0 |
| FUN-074 | Stage stepper reflects current state | Any state | Correct step | P1 |
| FUN-075 | Confirmation dialog before destructive action | Cancel/Recall | Dialog shown | P0 |
| FUN-076 | Acknowledgement checkbox required | Submit | Must check | P0 |
| FUN-077 | Reason field required for Cancel/Reject/Recall | Empty reason | Blocked | P0 |
| FUN-078 | Read-only fields during workflow | GO/Active | Disabled | P0 |
| FUN-079 | Pending decisions badge count | DoA2 dashboard | Accurate | P1 |
| FUN-080 | Workflow history sorted by date desc | History list | Newest first | P1 |

### 4.8 Data Integrity Rules (10)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-081 | Opportunity data immutable during workflow | Submit→Approve | No edits | P0 |
| FUN-082 | Workflow history append-only | Any action | No deletions | P0 |
| FUN-083 | Soft-deleted opp excluded from workflow | Delete opp | 404 on transition | P1 |
| FUN-084 | Partner link preserved through workflow | Submit+Approve | Partners intact | P0 |
| FUN-085 | Org unit link preserved | Submit | DoA2 from org unit | P0 |
| FUN-086 | Version incremented on transition | Any transition | Version +1 | P1 |
| FUN-087 | Timestamp in UTC for all transitions | Any | Audit in UTC | P1 |
| FUN-088 | User ID recorded for each action | Any | CreatedBy/ModifiedBy | P0 |
| FUN-089 | Reason/comment stored verbatim | Reject | No truncation | P1 |
| FUN-090 | Workflow instance ID stable across resubmit | Reopen→Submit | Same or new per spec | P1 |

---

## §5 Integration Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 5.1 CRUD (10)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-001 | Create → Submit → Approve | Full success flow | P0 |
| INT-002 | Create → Submit → Reject → Reopen → Resubmit → Approve | Recovery flow | P0 |
| INT-003 | Create → Cancel → Reopen → Submit | Cancel recovery | P0 |
| INT-004 | Submit → Recall → Edit → Resubmit | Recall flow | P0 |
| INT-005 | Workflow visible in opp detail | After submit | Workflow section shown | P0 |
| INT-006 | Status visible in list view | After transition | Updated | P1 |
| INT-007 | History visible in opp detail | After actions | All entries | P1 |
| INT-008 | Dashboard reflects transitions | After approval | Widget updated | P1 |
| INT-009 | Partner detail shows opp status | After transition | Updated status | P1 |
| INT-010 | Export includes workflow data | After transitions | All data present | P2 |

### 5.2 Search & Filter (10)

| ID | Filter | Expected | Priority |
|----|--------|----------|----------|
| INT-011 | Stage=GO | Only GO opps | P1 |
| INT-012 | Stage=CANCELLED | Only cancelled | P1 |
| INT-013 | Stage=NO GO | Only rejected | P1 |
| INT-014 | Status=Active (in workflow) | In-workflow opps | P1 |
| INT-015 | In-workflow indicator filter | Only submitted pending | P1 |
| INT-016 | OM name + stage filter | Combined | P1 |
| INT-017 | DoA2 pending list | DoA2's decisions | P1 |
| INT-018 | Sort by submission date | Chronological | P2 |
| INT-019 | Search within workflow history | Action keyword | P2 |
| INT-020 | Combined: stage + status + date range | Intersection | P1 |

### 5.3 Pagination (5)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-021 | Workflow history pagination | Paginated | P2 |
| INT-022 | Pending decisions list (50+) | Paginated | P2 |
| INT-023 | Opportunity list by stage (1000+) | Paginated | P2 |
| INT-024 | Notification history | Paginated | P2 |
| INT-025 | Escalation queue | Paginated | P2 |

### 5.4 Relationships (10)

| ID | Relationship | Test | Expected | Priority |
|----|-------------|------|----------|----------|
| INT-026 | Opp → WorkflowInstance | Submit creates | Linked | P0 |
| INT-027 | Opp → WorkflowHistory (1:N) | Multiple actions | All linked | P0 |
| INT-028 | Opp → DoA2 (via org unit) | Submit | Correct DoA2 | P0 |
| INT-029 | Opp → Notifications (1:N) | Various triggers | All linked | P1 |
| INT-030 | WorkflowHistory → User | Each entry | User ref correct | P1 |
| INT-031 | Workflow → EscalationChain | High-value opp | Chain linked | P1 |
| INT-032 | Delegation → User → Approval | Delegate approves | Linked correctly | P1 |
| INT-033 | SLA → Workflow | Timer linked | Tracked | P2 |
| INT-034 | Audit → Transition | Each transition | 1:1 audit | P0 |
| INT-035 | Opp → Partner preserved through workflow | Submit + Approve | Partners intact | P1 |

### 5.5 Error Handling (15)

| ID | Error | Expected | Priority |
|----|-------|----------|----------|
| INT-036 | Submit missing fields | 400 + errors | P0 |
| INT-037 | Submit non-existent opp | 404 | P0 |
| INT-038 | Approve non-submitted | 400 | P1 |
| INT-039 | Approve already decided | 409 | P1 |
| INT-040 | Cancel in workflow | 400 | P1 |
| INT-041 | Without auth | 401 | P0 |
| INT-042 | Expired token | 401 | P1 |
| INT-043 | Malformed body | 400 | P1 |
| INT-044 | Concurrent conflict | 409 | P1 |
| INT-045 | DB maintenance | 503 | P2 |
| INT-046 | Notification failure | Transition OK, notif queued | P1 |
| INT-047 | Workflow engine timeout | 504 | P2 |
| INT-048 | Rate limit | 429 | P2 |
| INT-049 | Payload too large | 413 | P2 |
| INT-050 | Escalation service down | Escalation queued | P1 |

### 5.6 Workflow ↔ Opportunity Integration (15)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-051 | Create opp → Submit → Verify workflow section | Workflow visible | P0 |
| INT-052 | Submit → Edit blocked → Recall → Edit enabled | Edit state correct | P0 |
| INT-053 | Approve → Opp read-only in list and detail | Read-only enforced | P0 |
| INT-054 | Reject → Reopen → Edit → Resubmit | Full recovery flow | P0 |
| INT-055 | Cancel → Reopen → Verify data intact | No data loss | P0 |
| INT-056 | Workflow history in opp detail | All entries visible | P1 |
| INT-057 | Stage filter in opp list | Correct subset | P1 |
| INT-058 | Status filter (Active/Closed) | Correct subset | P1 |
| INT-059 | Opp export includes workflow fields | Stage, status, history | P2 |
| INT-060 | Opp dashboard widget by stage | Counts correct | P1 |
| INT-061 | Partner detail shows linked opp workflow status | Updated | P1 |
| INT-062 | Search by OM + stage | Combined filter works | P1 |
| INT-063 | Bulk export with workflow data | All opps include workflow | P2 |
| INT-064 | Opp audit log includes workflow actions | Audit trail complete | P1 |
| INT-065 | Opp permissions respect workflow state | CanEdit based on state | P0 |

### 5.7 Notification Integration (10)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-066 | Submit → Email service called | Email sent | P1 |
| INT-067 | Approve → OM email | Delivery confirmed | P1 |
| INT-068 | Reject → OM email with reason | Reason in body | P1 |
| INT-069 | Recall → DoA2 email | Notification delivered | P1 |
| INT-070 | In-app notification + email | Both systems | P1 |
| INT-071 | Notification link → Auth → Opp | Full navigation | P1 |
| INT-072 | Escalation → Next level notified | Chain works | P1 |
| INT-073 | Notification queue → Retry on failure | Retry logic | P2 |
| INT-074 | User preference: email only | No in-app | P2 |
| INT-075 | Deleted user in notification list | Skipped gracefully | P1 |

### 5.8 Escalation & DoA Integration (10)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-076 | High-value opp → DoA3 in chain | Correct routing | P1 |
| INT-077 | DoA2 lookup by org unit | Correct DoA2 | P0 |
| INT-078 | Delegation active → Delegate receives | Delegate in chain | P1 |
| INT-079 | SLA timer → Escalation at breach | Escalation triggered | P1 |
| INT-080 | Escalation → OIC when DoA3 absent | Fallback works | P1 |
| INT-081 | Multiple DoA2 for org → Either can approve | Both valid | P1 |
| INT-082 | DoA2 role change during workflow | Consistent behavior | P1 |
| INT-083 | Org unit change before submit | New DoA2 used | P1 |
| INT-084 | Escalation config per org unit | Org-specific | P1 |
| INT-085 | Budget change after submit | No retroactive escalation | P1 |

### 5.9 API Contract Integration (5)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-086 | Submit API → Response schema | Matches contract | P1 |
| INT-087 | Approve API → Response schema | Matches contract | P1 |
| INT-088 | Workflow history API → Pagination | Correct structure | P1 |
| INT-089 | Pending decisions API → Filter params | Filter works | P1 |
| INT-090 | Transition API versioning | Backward compatible | P2 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 6.1 Injection (10)

| ID | Vector | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL | Cancel reason | Parameterized | P0 |
| SEC-002 | SQL | Reject reason | Escaped | P0 |
| SEC-003 | XSS | Recall justification | Escaped | P0 |
| SEC-004 | XSS | Approval comment | Sanitized | P0 |
| SEC-005 | XSS | History display | Escaped | P0 |
| SEC-006 | Command | Reason | Stored as text | P1 |
| SEC-007 | JSON | API body | Blocked | P1 |
| SEC-008 | Header | API | Blocked | P1 |
| SEC-009 | Template | Reason | Not evaluated | P2 |
| SEC-010 | Path traversal | Attachment | Rejected | P1 |

### 6.2 Access Control (10)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Any | 401 | P0 |
| SEC-012 | No role | Submit | 403 | P0 |
| SEC-013 | Wrong OM | Submit | 403 | P0 |
| SEC-014 | Wrong DoA2 | Approve | 403 | P0 |
| SEC-015 | Collaborator | Submit | 403 | P0 |
| SEC-016 | Expired session | Submit | 401 | P1 |
| SEC-017 | Revoked | Approve | 403 | P1 |
| SEC-018 | Horizontal | Other opp | 403 | P0 |
| SEC-019 | Vertical | Collab → OM | 403 | P0 |
| SEC-020 | Privilege escalation | Add role in request | Ignored | P1 |

### 6.3 IDOR (10)

| ID | Object | Expected | Priority |
|----|--------|----------|----------|
| SEC-021 | Opp ID | 403 | P0 |
| SEC-022 | Workflow ID | 403 | P0 |
| SEC-023 | History ID | 403 | P1 |
| SEC-024 | Notification ID | 403 | P1 |
| SEC-025 | DoA2 ID | Validated | P0 |
| SEC-026 | Negative ID | 400 | P2 |
| SEC-027 | Large ID | 404 | P2 |
| SEC-028 | UUID | 400 | P2 |
| SEC-029 | Sequential | Rate limited | P1 |
| SEC-030 | Predictable | Auth gated | P1 |

### 6.4 Mass Assignment (5)

| ID | Field | Expected | Priority |
|----|-------|----------|----------|
| SEC-031 | Stage | Ignored | P0 |
| SEC-032 | Status | Ignored | P0 |
| SEC-033 | WorkflowStatus | Ignored | P0 |
| SEC-034 | CreatedBy | Server-controlled | P1 |
| SEC-035 | IsDeleted | Not modifiable | P1 |

### 6.5 Auth & Session (10)

| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| SEC-036 | Replay | Blocked | P0 |
| SEC-037 | CSRF | Token required | P0 |
| SEC-038 | JWT tamper | Rejected | P0 |
| SEC-039 | Session fixation | New session | P1 |
| SEC-040 | Brute force | Rate limited | P1 |
| SEC-041 | Token refresh | Seamless | P1 |
| SEC-042 | After logout | Redirect | P1 |
| SEC-043 | HttpOnly | Set | P1 |
| SEC-044 | Secure | Set | P1 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected | Priority |
|----|------|----------|----------|
| SEC-046 | Stack trace | Not exposed | P0 |
| SEC-047 | Deleted opps | Excluded | P1 |
| SEC-048 | Internal IDs | Safe | P2 |
| SEC-049 | Sensitive logs | None | P0 |
| SEC-050 | Other users' data in history | Auth gated | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two OMs submit same opp | First wins | P0 |
| CON-002 | OM recalls while DoA2 approves | One wins | P0 |
| CON-003 | Two DoA2s approve | First wins | P0 |
| CON-004 | Double-click Submit | Single submit | P0 |
| CON-005 | Double-click Approve | Single approve | P0 |
| CON-006 | Edit during submission | Edit blocked | P1 |
| CON-007 | Delete during workflow | Delete blocked | P1 |
| CON-008 | Concurrent recall + reject | One wins | P0 |
| CON-009 | Parallel notifications | No duplicates | P1 |
| CON-010 | DB transaction isolation | ACID | P1 |
| CON-011 | Cache invalidation | Consistent | P1 |
| CON-012 | Concurrent history writes | All preserved | P1 |
| CON-013 | Load balancer routing | Consistent | P2 |
| CON-014 | Session timeout during dialog | Redirect | P1 |
| CON-015 | Parallel escalation checks | Correct | P2 |
| CON-016 | Concurrent reopen (two tabs) | First wins | P1 |
| CON-017 | Bulk approve (5 opps) | All independent | P1 |
| CON-018 | Status query during write | Before or after | P1 |
| CON-019 | Notification queue overflow | Backpressure | P2 |
| CON-020 | Parallel DoA2 lookups | Both correct | P2 |
| CON-021 | Concurrent SLA check + approval | SLA cancelled on approval | P1 |
| CON-022 | Parallel delegation + approval | Correct authority | P1 |
| CON-023 | Multiple workflow engines (HA) | Consistent | P2 |
| CON-024 | Concurrent audit appends | All preserved | P1 |
| CON-025 | Race: submit + org unit change | Consistent DoA2 | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

| ID | Cat | Test | Input | Expected | Priority |
|----|-----|------|-------|----------|----------|
| UNT-001 | Val | IsValidTransition(I&P→GO) | Valid | true | P0 |
| UNT-002 | Val | IsValidTransition(GO→I&P) | Invalid | false | P0 |
| UNT-003 | Val | IsValidTransition(CANCELLED→GO) | Invalid | false | P1 |
| UNT-004 | Val | ValidateReason("text") | Non-empty | Passes | P1 |
| UNT-005 | Val | ValidateReason("") | Empty | Error | P1 |
| UNT-006 | Fmt | FormatStageLabel(GO) | Enum | "GO" | P2 |
| UNT-007 | Fmt | FormatHistoryEntry | Data | Formatted | P1 |
| UNT-008 | Fmt | FormatNotificationEmail | Template | Populated | P1 |
| UNT-009 | Calc | CountPendingDecisions | DoA2 | Count | P1 |
| UNT-010 | Calc | CalculateEscalationLevel | Budget | Level | P1 |
| UNT-011 | Calc | CalculateSLARemaining | Submit time | Days | P2 |
| UNT-012 | Calc | IsEscalationRequired | $5M+ | true | P1 |
| UNT-013 | Calc | IsEscalationRequired | $100K | false | P1 |
| UNT-014 | Status | GetAvailableActions(I&P, OM) | State+Role | [Submit, Cancel] | P0 |
| UNT-015 | Status | GetAvailableActions(GO, DoA2) | State+Role | [Approve, Reject] | P0 |
| UNT-016 | Status | GetAvailableActions(GO, OM) | State+Role | [Recall] | P0 |
| UNT-017 | Status | GetAvailableActions(Closed) | Final | [] | P0 |
| UNT-018 | Status | GetNextApprover | Org unit | DoA2 user | P1 |
| UNT-019 | Coll | GetPendingDecisions | Filter | Subset | P1 |
| UNT-020 | Coll | FilterHistoryByAction | Filter | Matching entries | P2 |
| UNT-021 | Coll | GroupNotifications | Recipients | Grouped | P2 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Submit for Go | < 3s | P1 |
| PRF-002 | Approve | < 2s | P1 |
| PRF-003 | Reject | < 2s | P1 |
| PRF-004 | Cancel | < 2s | P1 |
| PRF-005 | Recall | < 2s | P1 |
| PRF-006 | Reopen | < 2s | P1 |
| PRF-007 | History load (500 entries) | < 1s | P1 |
| PRF-008 | DoA2 pending list (100) | < 2s | P1 |
| PRF-009 | Stage filter (10K opps) | < 2s | P1 |
| PRF-010 | Notification dispatch | < 5s | P2 |
| PRF-011 | 50 concurrent submissions | < 10s all | P2 |
| PRF-012 | 20 concurrent approvals | < 5s all | P2 |
| PRF-013 | 100 reads during transitions | Avg < 3s | P2 |
| PRF-014 | Memory during transition | < 30MB | P2 |
| PRF-015 | Escalation check | < 1s | P2 |
| PRF-016 | SLA calculation (1000 opps) | < 5s | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 100 workflow actions/hour | 1 hour | Error < 1% | P2 |
| LDT-002 | 50 concurrent DoA2 decisions | 30 min | All succeed | P2 |
| LDT-003 | 200 concurrent reads | 1 hour | P99 < 5s | P2 |
| LDT-004 | 500 transitions in 1 min | 1 min | All processed | P2 |
| LDT-005 | 1000 reads during bulk approval | 5 min | Responsive | P2 |
| LDT-006 | Load until 5% errors | Until failure | Find limits | P2 |
| LDT-007 | Notification queue capacity | Until full | Backpressure | P2 |
| LDT-008 | Escalation service under load | 1 hour | All escalations processed | P2 |
| LDT-009 | Recovery after spike | 5 min | Normal in 2 min | P2 |
| LDT-010 | Service restart during active workflows | Recovery | No data loss | P2 |

---

## Traceability Matrix

| Feature | Test Cases |
|---------|------------|
| State transitions | POS-001 to POS-012, NEG-001 to NEG-015, UNT-001 to UNT-003 |
| Approval chain | POS-013 to POS-022, NEG-041 to NEG-050 |
| Notifications | POS-023 to POS-030, NEG-051 to NEG-060 |
| Escalation | POS-031 to POS-035, BND-046 to BND-055 |
| Validation | FUN-016 to FUN-030, NEG-031 to NEG-040 |
| Concurrency | CON-001 to CON-025, BND-034 |
| Security | SEC-001 to SEC-050 |
| Performance | PRF-001 to PRF-016, LDT-001 to LDT-010 |

---

**Last Updated:** 2026-02-11  
**Supersedes:** Previous version (38 tests, domain groups)  
**Status:** Ready for Execution  
**Compliance:** ✅ 10-Category Standard, ✅ 3:1 Ratio (140 ≥ 105)
