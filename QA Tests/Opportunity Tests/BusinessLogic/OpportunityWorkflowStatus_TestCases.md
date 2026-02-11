# Opportunity Workflow Status — Comprehensive Test Cases

**Feature:** Opportunity status transitions, stage management, and workflow state validation  
**Created:** 2026-01-25  
**Restructured:** 2026-02-11 (10-category standard)  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 35 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 70 | Max(50, 2×35=70) | ✅ |
| 3 | Boundary Tests | §3 | 70 | Max(50, 2×35=70) | ✅ |
| 4 | Functional Tests | §4 | 50 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 50 | ≥50 | ✅ |
| 6 | Security Tests | §6 | 50 | ≥50 | ✅ |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **397** | **≥347** | ✅ |

**3:1 Ratio Check:** (70 + 70) = **140** ≥ 3 × 35 = **105** → ✅ PASS

---

## Status/Stage Matrix

| Stage | Status | Description |
|-------|--------|-------------|
| Identify & Profile | Draft | Initial creation, editable |
| Identify & Profile | Active | In workflow (submitted for decision) |
| GO | Active | Submitted for Go decision, awaiting approval |
| GO | Closed | Approved — opportunity accepted |
| NO GO | Closed | Rejected — opportunity not pursued |
| CANCELLED | Closed | Cancelled by OM before decision |

---

## §1 Positive Tests (Happy Path)

> **Count: 35** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Transition | Role | Expected Result | Priority |
|----|-----------|------|-----------------|----------|
| POS-001 | I&P/Draft → GO/Active (Submit) | OM | Stage=GO, Status=Active | P0 |
| POS-002 | GO/Active → GO/Closed (Approve) | DoA2 | Status=Closed, PASS decision | P0 |
| POS-003 | GO/Active → NO GO/Closed (Reject) | DoA2 | Stage=NO GO, Status=Closed | P0 |
| POS-004 | I&P/Draft → CANCELLED/Closed (Cancel) | OM | Stage=CANCELLED, Status=Closed | P0 |
| POS-005 | CANCELLED/Closed → I&P/Draft (Reopen) | OM | Stage=I&P, Status=Draft | P0 |
| POS-006 | NO GO/Closed → I&P/Draft (Reopen) | OM | Stage=I&P, Status=Draft | P0 |
| POS-007 | GO/Active → I&P/Draft (Recall) | OM | Stage=I&P, Status=Draft | P0 |
| POS-008 | Status displayed correctly in list view | Any | Correct stage + status shown | P1 |
| POS-009 | Status badge color matches stage | Each stage | Correct color coding | P1 |
| POS-010 | Workflow history records every transition | Multiple | Full history visible | P0 |
| POS-011 | Status change triggers notification | Submit/Approve/Reject | Email sent to relevant users | P1 |
| POS-012 | Status timestamp recorded in UTC | Any transition | UTC timestamp in history | P1 |
| POS-013 | Transition user recorded in history | Any transition | User name + ID logged | P1 |
| POS-014 | Read-only enforced after approval | GO/Closed | All fields read-only | P0 |
| POS-015 | Editable after reopen | I&P/Draft (reopened) | Fields become editable | P0 |
| POS-016 | Sequential transitions work: Submit→Reject→Reopen→Submit→Approve | Full cycle | All transitions succeed | P0 |
| POS-017 | Sequential transitions: Cancel→Reopen→Submit→Approve | Full cycle | All transitions succeed | P0 |
| POS-018 | Status filter shows correct count | Filter by stage | Correct number displayed | P1 |
| POS-019 | Status shown in opportunity card view | Card layout | Stage + status visible | P1 |
| POS-020 | Status change reflects in real-time (same session) | Approve | Status updates without refresh | P1 |
| POS-021 | Status preserved after page refresh | Any stage | Same status shown after F5 | P1 |
| POS-022 | Workflow stepper shows current stage | Each stage | Active step highlighted | P1 |
| POS-023 | Completed steps shown as completed | After approval | All steps show check marks | P1 |
| POS-024 | In-workflow indicator visible | GO/Active | Badge/indicator shown | P1 |
| POS-025 | In-workflow indicator removed | After decision | Indicator removed | P1 |
| POS-026 | Multiple approvals in batch | DoA2, 5 pending | All processed independently | P1 |
| POS-027 | History shows action + reason | Reject with reason | Reason visible in history | P1 |
| POS-028 | Cancel reason displayed in history | Cancel with reason | Reason visible | P1 |
| POS-029 | Recall justification in history | Recall with text | Text visible in history | P1 |
| POS-030 | Status API returns correct state | GET /api/opportunity/{id} | stage + status fields correct | P1 |
| POS-031 | Bulk status query works | GET /api/opportunities?stage=GO | Filtered results correct | P1 |
| POS-032 | Stage stepper navigation (read-only) | Click previous step | Shows section, no state change | P2 |
| POS-033 | Status history paginated (if 100+ entries) | Long history | Paginated/scrollable | P2 |
| POS-034 | Export opportunity includes status | Export/report | Stage + status in export | P2 |
| POS-035 | Dashboard widget shows status distribution | Dashboard | Correct counts per stage | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 2.1 Invalid State Transitions (15 tests)

| ID | Current State | Invalid Action | Expected Result | Priority |
|----|---------------|---------------|-----------------|----------|
| NEG-001 | I&P/Draft | Approve | Action not available | P0 |
| NEG-002 | I&P/Draft | Reject (DoA2) | Not available (not submitted) | P0 |
| NEG-003 | GO/Active | Cancel | Not available while in workflow | P0 |
| NEG-004 | GO/Closed (Approved) | Submit | Not available (decided) | P0 |
| NEG-005 | GO/Closed (Approved) | Cancel | Not available (decided) | P0 |
| NEG-006 | NO GO/Closed | Submit directly | Must reopen first | P0 |
| NEG-007 | NO GO/Closed | Cancel | Not available on rejected | P1 |
| NEG-008 | CANCELLED/Closed | Submit directly | Must reopen first | P0 |
| NEG-009 | CANCELLED/Closed | Approve | Not available | P1 |
| NEG-010 | CANCELLED/Closed | Reject | Not available | P1 |
| NEG-011 | GO/Active | Reopen (before decision) | Not available (use Recall) | P1 |
| NEG-012 | I&P/Draft | Recall (not submitted) | Not available | P1 |
| NEG-013 | GO/Closed | Reopen (approved) | Not available (final decision) | P0 |
| NEG-014 | Directly set Status=Closed via API | PUT with status | Status field ignored | P0 |
| NEG-015 | Directly set Stage=GO via API | PUT with stage | Stage field ignored | P0 |

### 2.2 Unauthorized Status Changes (15 tests)

| ID | User Role | Action | Expected | Priority |
|----|-----------|--------|----------|----------|
| NEG-016 | Collaborator | Submit for Go | Access Denied | P0 |
| NEG-017 | Collaborator | Cancel | Access Denied | P0 |
| NEG-018 | Collaborator | Reopen | Access Denied | P0 |
| NEG-019 | Collaborator | Recall | Access Denied | P0 |
| NEG-020 | General User | Any workflow action | Access Denied | P0 |
| NEG-021 | OM of different opp | Submit this opp | 403 | P0 |
| NEG-022 | DoA2 of different org | Approve this opp | 403 | P0 |
| NEG-023 | Unauthenticated | Any transition | 401 | P0 |
| NEG-024 | Expired session | Submit | 401 + redirect | P1 |
| NEG-025 | Tampered JWT | Approve | 401 | P1 |
| NEG-026 | Non-DoA2 user | Approve | 403 | P0 |
| NEG-027 | Non-DoA2 user | Reject | 403 | P0 |
| NEG-028 | Partner User | Any transition | Not available | P1 |
| NEG-029 | Service account (no UI) | POST transition | Per config | P2 |
| NEG-030 | Admin without OM role | Submit | Per permission config | P1 |

### 2.3 Missing Required Data for Transitions (10 tests)

| ID | Missing Requirement | Action | Expected Error | Priority |
|----|-------------------|--------|---------------|----------|
| NEG-031 | Cancel reason empty | Cancel | "Reason required" | P0 |
| NEG-032 | Reject reason empty | Reject | "Reason required" | P0 |
| NEG-033 | Recall justification empty | Recall | "Justification required" | P0 |
| NEG-034 | Acknowledgement not checked | Submit | Submit disabled | P0 |
| NEG-035 | DoA2 not found for org unit | Submit | "No decision maker found" | P0 |
| NEG-036 | Mandatory opp fields missing | Submit | Validation error list | P0 |
| NEG-037 | Opportunity Statement not generated | Submit | "Generate statement first" | P0 |
| NEG-038 | Cancel reason = whitespace only | Cancel | "Reason cannot be blank" | P1 |
| NEG-039 | Reject reason = whitespace only | Reject | "Reason cannot be blank" | P1 |
| NEG-040 | Approval comment = only special chars | Approve | ✅ Accept (optional) | P2 |

### 2.4 API-Level Failures (10 tests)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-041 | POST /api/opportunity/{id}/submit without body | 400 | P1 |
| NEG-042 | POST /api/opportunity/999999/submit (non-existent) | 404 | P1 |
| NEG-043 | POST /api/opportunity/{id}/approve without auth | 401 | P0 |
| NEG-044 | Malformed JSON in transition request | 400 | P1 |
| NEG-045 | POST transition with extra fields | Extra ignored | P2 |
| NEG-046 | PUT status directly (bypass workflow) | 400 or field ignored | P0 |
| NEG-047 | DELETE opportunity in GO/Active state | 403 (cannot delete while in workflow) | P1 |
| NEG-048 | Transition on soft-deleted opportunity | 404 | P1 |
| NEG-049 | Transition with concurrent modification | 409 Conflict | P1 |
| NEG-050 | Rate-limited transition requests | 429 | P2 |

### 2.5 Dependency & Network Failures (10 tests)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-051 | DB timeout during status change | Error msg, no partial change | P1 |
| NEG-052 | Email service down during notification | Transition succeeds, notification queued | P1 |
| NEG-053 | Workflow engine unavailable | Clear error message | P1 |
| NEG-054 | Auth token expires mid-transition | Redirect to login | P1 |
| NEG-055 | Network disconnect during approval | Transaction rollback | P1 |
| NEG-056 | Audit service unavailable | Transition blocked or succeeds with retry | P2 |
| NEG-057 | Concurrent DB lock | Optimistic concurrency error | P2 |
| NEG-058 | Statement generation timeout | Submit blocked with clear error | P1 |
| NEG-059 | DoA2 lookup timeout | Submit blocked with timeout error | P1 |
| NEG-060 | Cache stale after transition | Eventually consistent within seconds | P2 |

### 2.6 UI/Form Failures (10 tests)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-061 | Double-click Submit | Only one transition | P0 |
| NEG-062 | Double-click Approve | Only one approval | P0 |
| NEG-063 | Click Cancel then immediately Cancel again | Idempotent | P1 |
| NEG-064 | Browser back after submission | Cannot resubmit | P1 |
| NEG-065 | Refresh page during transition | No partial state | P1 |
| NEG-066 | Close browser tab during approval | Transaction rolls back | P1 |
| NEG-067 | JavaScript error during transition | Graceful error message | P2 |
| NEG-068 | Long-press on workflow button | Single action | P2 |
| NEG-069 | Keyboard shortcut triggers transition | Only explicit button click | P2 |
| NEG-070 | Screen reader announces transition result | Accessible feedback | P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 3.1 Reason/Comment Text Boundaries (15 tests)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Cancel reason | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Reject reason | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Recall justification | 1 | 2000 | ✅ | ✅ | ❌ | P1 |
| BND-004 | Approval comments | 0 | 2000 | ✅ (optional) | ✅ | ❌ | P1 |
| BND-005 | Cancel reason = 1 char | — | — | ✅ | — | — | P2 |
| BND-006 | Cancel reason = 2000 chars | — | — | — | ✅ | — | P2 |
| BND-007 | Cancel reason = 2001 chars | — | — | — | — | ❌ | P2 |
| BND-008 | Reject reason with newlines | Multiline | — | ✅ Preserved | — | — | P2 |
| BND-009 | Approval comment with only spaces | Whitespace | — | Trimmed to empty | — | — | P2 |
| BND-010 | Multiple paragraphs in reason | Long text | — | ✅ Formatted | — | — | P2 |
| BND-011 | Reason with special chars (!@#$%) | Special | — | ✅ Stored | — | — | P1 |
| BND-012 | Reason with HTML tags | HTML | — | Escaped | — | — | P1 |
| BND-013 | Reason with SQL-like text | SQL | — | Parameterized | — | — | P1 |
| BND-014 | Reason with Unicode | Arabic/Chinese | — | ✅ Stored correctly | — | — | P1 |
| BND-015 | Reason with emoji | Emoji | — | ✅ or ❌ with msg | — | — | P2 |

### 3.2 Timing Boundaries (15 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| BND-016 | Submit at midnight UTC | Correct date in history | P1 |
| BND-017 | Approve 1 second after submit | Both timestamps distinct | P1 |
| BND-018 | Transition at DST changeover | UTC timestamp correct | P2 |
| BND-019 | Transition at year boundary | Correct year in log | P2 |
| BND-020 | Session exactly at timeout threshold | Graceful redirect or extend | P1 |
| BND-021 | Token expires during dialog display | Handle on confirm click | P1 |
| BND-022 | Approval after very long delay (24 hours) | Still valid | P1 |
| BND-023 | Submit + immediate page navigation | Transition completes | P1 |
| BND-024 | Recall at exact moment DoA2 approves | One wins, other conflicts | P0 |
| BND-025 | Multiple transitions within 1 second | All recorded with distinct timestamps | P1 |
| BND-026 | Transition from different timezone | UTC stored, local displayed | P2 |
| BND-027 | Transition at leap second | Handled gracefully | P2 |
| BND-028 | History query for time range exactly matching one entry | Correct entry returned | P2 |
| BND-029 | Transition when server clock differs from client | Server time used | P1 |
| BND-030 | Cancel + Reopen within 1 minute | Both recorded correctly | P1 |

### 3.3 Workflow History Boundaries (10 tests)

| ID | History State | Expected Result | Priority |
|----|-------------|-----------------|----------|
| BND-031 | 0 history entries (new opp) | Empty history section or "No actions yet" | P1 |
| BND-032 | 1 history entry | Single entry displayed | P1 |
| BND-033 | 100 history entries | All displayed with scroll/pagination | P1 |
| BND-034 | 1000+ history entries | Performance acceptable (<3s load) | P2 |
| BND-035 | History with very long reason text | Text truncated with "show more" | P2 |
| BND-036 | History with HTML in stored reason | HTML escaped on display | P1 |
| BND-037 | History sorted chronologically | Latest first or oldest first (consistent) | P1 |
| BND-038 | History across page navigation | Preserved after nav | P2 |
| BND-039 | History export (if available) | All entries in export | P2 |
| BND-040 | History after soft-delete of opportunity | History preserved for audit | P1 |

### 3.4 Stage Stepper Boundaries (10 tests)

| ID | Stepper State | Expected Display | Priority |
|----|-------------|-----------------|----------|
| BND-041 | First stage (I&P) | Only first step active | P1 |
| BND-042 | Last stage (GO/Closed Approved) | All steps completed | P1 |
| BND-043 | CANCELLED (side path) | Cancelled shown as separate state | P1 |
| BND-044 | NO GO (side path) | NO GO shown distinctly | P1 |
| BND-045 | After Reopen (back to I&P) | Stepper resets to I&P, history preserved | P1 |
| BND-046 | Multiple cycles (Submit→Reject→Reopen→Submit) | Stepper shows current, history shows all | P1 |
| BND-047 | Stepper on mobile viewport | Responsive layout | P2 |
| BND-048 | Stepper with screen reader | Accessible labels for each step | P2 |
| BND-049 | Stepper animation during transition | Smooth transition animation | P2 |
| BND-050 | Stepper tooltip shows date | Hover shows transition date | P2 |

### 3.5 Multi-Opportunity Boundaries (10 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| BND-051 | OM has 100 opps in different stages | Each independent | P1 |
| BND-052 | DoA2 has 50 pending decisions | All actionable independently | P1 |
| BND-053 | Same partner linked to opps in all stages | Each opp has own status | P1 |
| BND-054 | Filter returns 0 results for stage | "No results" message | P1 |
| BND-055 | Filter returns 10,000 results | Paginated correctly | P2 |
| BND-056 | Dashboard with all stages having 0 count | All zeros displayed | P2 |
| BND-057 | Dashboard with extreme distribution (9999 in one stage) | Display handles large numbers | P2 |
| BND-058 | Transition when other opps in same org unit transition | Independent processing | P1 |
| BND-059 | All opps for org unit in same stage | Valid state | P2 |
| BND-060 | OM removed from all opps | OM field shows "Unassigned" or blocked | P1 |

### 3.6 Data State Boundaries (10 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| BND-061 | Transition on opp with maximum data (all fields, all collections) | Transition succeeds | P1 |
| BND-062 | Transition on opp with minimum data (only required) | Transition succeeds | P1 |
| BND-063 | Status query with null stage parameter | All stages returned | P2 |
| BND-064 | Status query with invalid stage value | 400 error | P1 |
| BND-065 | History entry with null user (system action) | "System" displayed | P2 |
| BND-066 | Status after org unit hierarchy change | Status unchanged, DoA2 may change | P1 |
| BND-067 | Status after OM deactivation | Status preserved, OM shown as inactive | P1 |
| BND-068 | Status after DoA2 deactivation | Decision still valid, new DoA2 for future | P1 |
| BND-069 | Transition on opp created by different system version | Compatible | P2 |
| BND-070 | Status with concurrent read from 100 users | All see consistent state | P1 |

---

## §4 Functional Tests (Business Rules)

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 4.1 Workflow Rules (15)

| ID | Rule | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| FUN-001 | Only valid transitions allowed | Matrix enforcement | Invalid transitions rejected | P0 |
| FUN-002 | Cancel only from I&P, not in workflow | Cancel from GO/Active | Blocked | P0 |
| FUN-003 | Recall returns to I&P/Draft | Recall from GO/Active | I&P/Draft restored | P0 |
| FUN-004 | Approval is final (no further transitions) | After GO/Closed (Approved) | No workflow actions available | P0 |
| FUN-005 | Reopen restores editability | Reopen from CANCELLED/NO GO | Fields editable | P0 |
| FUN-006 | Read-only during workflow | GO/Active | All content fields disabled | P0 |
| FUN-007 | Read-only after final decision | GO/Closed | Permanently read-only | P0 |
| FUN-008 | In-workflow indicator logic | Set/unset correctly | Matches actual workflow state | P1 |
| FUN-009 | Multiple workflow cycles allowed | Reopen + resubmit | No limit on cycles | P1 |
| FUN-010 | Each cycle has independent history | Multiple cycles | All recorded separately | P1 |
| FUN-011 | Stage change requires confirmation dialog | Any transition | User must confirm | P0 |
| FUN-012 | Cancel requires reason before confirm | Cancel without reason | Confirm disabled | P0 |
| FUN-013 | Reject requires reason before confirm | Reject without reason | Confirm disabled | P0 |
| FUN-014 | Recall requires justification | Recall without justification | Confirm disabled | P0 |
| FUN-015 | Submit requires acknowledgement | Submit without checkbox | Confirm disabled | P0 |

### 4.2 Validation Rules (15)

| ID | Rule | Test | Expected | Priority |
|----|------|------|----------|----------|
| FUN-016 | All mandatory fields before submit | Missing fields | Validation error list | P0 |
| FUN-017 | DoA2 must exist for submit | No DoA2 | Submit blocked | P0 |
| FUN-018 | Statement must exist for submit | No statement | Submit blocked | P0 |
| FUN-019 | Only OM can submit/cancel/recall | Non-OM | Action not available | P0 |
| FUN-020 | Only DoA2 can approve/reject | Non-DoA2 | Action not available | P0 |
| FUN-021 | Reason text validated (non-blank) | Whitespace only | Rejected | P1 |
| FUN-022 | Reason max length enforced | Over limit | Rejected | P1 |
| FUN-023 | Server validates even if client bypassed | Direct API | Server rejects invalid | P0 |
| FUN-024 | Status cannot be set directly via API | PUT with status field | Ignored | P0 |
| FUN-025 | Stage cannot be set directly via API | PUT with stage field | Ignored | P0 |
| FUN-026 | Deleted opp cannot transition | Soft-deleted | 404 | P1 |
| FUN-027 | Workflow instance unique per opp | Double submit | Second blocked | P1 |
| FUN-028 | Transition requires active user account | Deactivated user | Blocked | P1 |
| FUN-029 | Transition requires valid session | Expired session | 401 | P1 |
| FUN-030 | Validation errors shown in aggregate | 5 errors | All 5 displayed | P0 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Test | Expected | Priority |
|----|-----------|------|----------|----------|
| FUN-031 | One active workflow per opp | Submit while already submitted | Blocked | P0 |
| FUN-032 | Audit immutability | Try to edit history | Not possible | P1 |
| FUN-033 | Stage/status always consistent | Query after any transition | Valid combination per matrix | P0 |
| FUN-034 | Soft-deleted opps excluded from workflow queries | Filter active | Deleted not returned | P1 |
| FUN-035 | Status preserved across system restart | Restart service | Same status on return | P1 |
| FUN-036 | Transition atomic (all or nothing) | Partial failure scenario | Full rollback | P0 |
| FUN-037 | Notification failure doesn't block transition | Email fails | Transition succeeds | P1 |
| FUN-038 | Concurrent read during write | Read while transitioning | Consistent state (before or after) | P1 |
| FUN-039 | Maximum workflow cycles (if limited) | Exceed limit | Clear error or no limit | P2 |
| FUN-040 | Status enum validated | Invalid stage string | Rejected | P1 |

### 4.4 Audit Rules (10)

| ID | Audit Requirement | Test | Expected | Priority |
|----|------------------|------|----------|----------|
| FUN-041 | Every transition creates audit entry | All transitions | Entry for each | P0 |
| FUN-042 | Audit includes: user, timestamp, action, from-state, to-state | Any transition | All fields populated | P0 |
| FUN-043 | Reason included in audit | Cancel/Reject/Recall | Reason text in entry | P0 |
| FUN-044 | Approval comments in audit | Approve with comment | Comment in entry | P1 |
| FUN-045 | Audit entries immutable | Any attempt to modify | Cannot change | P0 |
| FUN-046 | Audit ordered chronologically | Multiple entries | Correct order | P1 |
| FUN-047 | Audit survives soft-delete | Delete opp | Audit preserved | P1 |
| FUN-048 | Audit accessible via API | GET audit endpoint | Returns entries | P1 |
| FUN-049 | Failed transitions logged | Failed attempt | Attempt logged (not as success) | P2 |
| FUN-050 | Audit includes IP address (if configured) | Any transition | IP logged | P2 |

---

## §5 Integration Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 5.1 CRUD Workflow (10)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-001 | Create opp → Submit → Approve → Read final state | GO/Closed, history complete | P0 |
| INT-002 | Create → Submit → Reject → Reopen → Edit → Resubmit → Approve | Full lifecycle | P0 |
| INT-003 | Create → Cancel → Reopen → Submit → Approve | Cancel recovery flow | P0 |
| INT-004 | Create → Submit → Recall → Modify → Resubmit | Recall flow | P0 |
| INT-005 | View opportunity detail after each transition | Status updated on each | P1 |
| INT-006 | View opportunity list after status change | List reflects new status | P1 |
| INT-007 | Status change visible in partner detail | Partner's opp list updated | P1 |
| INT-008 | Dashboard widget reflects status change | Widget counts updated | P1 |
| INT-009 | Export after status change | Export includes new status | P2 |
| INT-010 | Notification chain: Submit→DoA2 email→Approve→OM email | Full notification flow | P0 |

### 5.2 Search & Filter (10)

| ID | Search/Filter | Expected | Priority |
|----|--------------|----------|----------|
| INT-011 | Filter by stage=I&P | Only I&P opps | P1 |
| INT-012 | Filter by stage=GO | Only GO opps | P1 |
| INT-013 | Filter by stage=NO GO | Only rejected | P1 |
| INT-014 | Filter by stage=CANCELLED | Only cancelled | P1 |
| INT-015 | Filter by status=Draft | Only draft | P1 |
| INT-016 | Filter by status=Active | Only active (in workflow) | P1 |
| INT-017 | Filter by status=Closed | Only closed (decided) | P1 |
| INT-018 | Combined: stage=GO + status=Active | Awaiting decision | P1 |
| INT-019 | Sort by status change date | Chronological | P2 |
| INT-020 | Search by OM for specific stage | Filtered results | P1 |

### 5.3 Pagination (5)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-021 | Page 1 of 100+ GO opps | First page shown | P2 |
| INT-022 | Last page | Remaining items | P2 |
| INT-023 | Change page size | Updated display | P2 |
| INT-024 | Workflow history pagination (100+ entries) | Paginated | P2 |
| INT-025 | Empty result for rare stage | "No results" | P2 |

### 5.4 Relationships (10)

| ID | Relationship | Test | Expected | Priority |
|----|-------------|------|----------|----------|
| INT-026 | Opportunity ↔ WorkflowInstance | Submit creates instance | 1:1 correct | P0 |
| INT-027 | Opportunity ↔ WorkflowHistory (1:N) | Multiple actions | All linked | P0 |
| INT-028 | Opportunity ↔ DoA2 | Submit routes to DoA2 | Correct user | P0 |
| INT-029 | Opportunity ↔ Notifications | Submit generates notif | Linked to opp | P1 |
| INT-030 | Status preserved after partner update | Edit partner | Status unchanged | P1 |
| INT-031 | Status preserved after document upload | Upload doc | Status unchanged | P1 |
| INT-032 | WorkflowHistory ↔ User | Each entry has user ref | User data correct | P1 |
| INT-033 | WorkflowHistory ↔ Reason text | Entry with reason | Text preserved | P1 |
| INT-034 | Dashboard ↔ Status aggregation | Multiple opps | Counts correct | P1 |
| INT-035 | Audit trail ↔ Opportunity lifecycle | Full lifecycle | Complete trail | P0 |

### 5.5 Error Handling (15)

| ID | Error | Expected Response | Priority |
|----|-------|------------------|----------|
| INT-036 | POST /submit with missing fields | 400 + validation errors | P0 |
| INT-037 | POST /submit for non-existent opp | 404 | P0 |
| INT-038 | POST /approve for non-submitted opp | 400 "Not in workflow" | P1 |
| INT-039 | POST /approve for already decided | 409 "Already decided" | P1 |
| INT-040 | POST /cancel for in-workflow opp | 400 "Cannot cancel during workflow" | P1 |
| INT-041 | POST /recall for non-submitted opp | 400 "Not submitted" | P1 |
| INT-042 | POST without auth | 401 | P0 |
| INT-043 | POST with expired token | 401 | P1 |
| INT-044 | POST with malformed body | 400 | P1 |
| INT-045 | Concurrent submit + approve | One succeeds, one conflicts | P1 |
| INT-046 | POST /submit during DB maintenance | 503 | P2 |
| INT-047 | Notification delivery failure | Transition succeeds, notification retried | P1 |
| INT-048 | Workflow engine timeout | 504 with retry guidance | P2 |
| INT-049 | Rate-limited transition | 429 | P2 |
| INT-050 | Request exceeds payload limit | 413 | P2 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 6.1 Injection Prevention (10)

| ID | Vector | Target | Payload | Expected | Priority |
|----|--------|--------|---------|----------|----------|
| SEC-001 | SQL | Cancel reason | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL | Reject reason | `' OR 1=1--` | Escaped | P0 |
| SEC-003 | XSS | Recall justification | `<script>alert(1)</script>` | Escaped | P0 |
| SEC-004 | XSS | Approval comment | `<img onerror=alert(1)>` | Sanitized | P0 |
| SEC-005 | XSS | History display | `<svg onload=alert(1)>` | Escaped | P0 |
| SEC-006 | Command | Reason field | `$(whoami)` | Stored as text | P1 |
| SEC-007 | JSON | API body | `{"__proto__":{}}` | Blocked | P1 |
| SEC-008 | Header | API header | `\r\nInjected:true` | Blocked | P1 |
| SEC-009 | LDAP | User lookup | `*()|&` | Rejected | P2 |
| SEC-010 | Template | Reason | `{{7*7}}` | Not evaluated | P2 |

### 6.2 Access Control (10)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | GET workflow state | 401 | P0 |
| SEC-012 | No role | POST /submit | 403 | P0 |
| SEC-013 | Wrong OM | POST /submit (other opp) | 403 | P0 |
| SEC-014 | Wrong DoA2 | POST /approve (other org) | 403 | P0 |
| SEC-015 | Collaborator | POST /cancel | 403 | P0 |
| SEC-016 | Expired session | POST /submit | 401 | P1 |
| SEC-017 | Revoked permissions | POST /approve | 403 | P1 |
| SEC-018 | Privilege escalation attempt | Add admin role to request | Ignored | P1 |
| SEC-019 | Horizontal escalation | Change opp ID to other's | 403 | P0 |
| SEC-020 | Vertical escalation | Collaborator → OM action | 403 | P0 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Opportunity ID | Enumerate IDs | 403 for unauthorized | P0 |
| SEC-022 | Workflow ID | Access other's workflow | 403 | P0 |
| SEC-023 | History entry ID | Access other's history | 403 | P1 |
| SEC-024 | Notification ID | Access other's notif | 403 | P1 |
| SEC-025 | DoA2 user ID | Forge in approval | Validated server-side | P0 |
| SEC-026 | Negative ID | ID=-1 | 400 | P2 |
| SEC-027 | Very large ID | 9999999999 | 404 | P2 |
| SEC-028 | UUID instead of int | Invalid format | 400 | P2 |
| SEC-029 | Sequential scanning | Try all IDs | Rate limited | P1 |
| SEC-030 | Predictable workflow ID | Guess next ID | Non-sequential or auth-gated | P1 |

### 6.4 Mass Assignment (5)

| ID | Field | Expected | Priority |
|----|-------|----------|----------|
| SEC-031 | Stage | Ignored | P0 |
| SEC-032 | Status | Ignored | P0 |
| SEC-033 | WorkflowStatus | Ignored | P0 |
| SEC-034 | CreatedBy | Server-controlled | P1 |
| SEC-035 | IsDeleted | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| SEC-036 | Replay captured transition | Anti-replay token | P0 |
| SEC-037 | CSRF on workflow endpoint | Token required | P0 |
| SEC-038 | JWT tampering | Rejected | P0 |
| SEC-039 | Session fixation | New session | P1 |
| SEC-040 | Brute force transitions | Rate limited | P1 |
| SEC-041 | Token refresh during workflow | Seamless | P1 |
| SEC-042 | After logout | Redirect | P1 |
| SEC-043 | HttpOnly cookie | Set | P1 |
| SEC-044 | Secure cookie | Set | P1 |
| SEC-045 | HTTPS only | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected | Priority |
|----|------|----------|----------|
| SEC-046 | Error response | No stack trace | P0 |
| SEC-047 | Deleted opps in queries | Excluded | P1 |
| SEC-048 | Internal IDs in response | Display-safe | P2 |
| SEC-049 | Sensitive data in logs | None | P0 |
| SEC-050 | History includes other users' data | Only authorized | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two OMs submit same opp | First wins, second conflicts | P0 |
| CON-002 | OM recalls while DoA2 approves | One wins, other gets conflict | P0 |
| CON-003 | Two DoA2s approve simultaneously | First wins | P0 |
| CON-004 | Double-click Submit | Single submission | P0 |
| CON-005 | Double-click Approve | Single approval | P0 |
| CON-006 | Status transition under load (100 concurrent reads) | All see consistent state | P1 |
| CON-007 | Edit opp while it's being submitted | Submit wins or edit blocked | P1 |
| CON-008 | Delete opp while it's in workflow | Delete blocked or transition rolls back | P1 |
| CON-009 | Concurrent recall + reject | One succeeds | P0 |
| CON-010 | Parallel history writes | None lost | P1 |
| CON-011 | Concurrent notification dispatch | No duplicates | P1 |
| CON-012 | Cache invalidation on transition | Others see update | P1 |
| CON-013 | DB transaction isolation | ACID compliance | P1 |
| CON-014 | Load balancer + concurrent transitions | Data consistent | P2 |
| CON-015 | Concurrent reopen from two tabs | First succeeds | P1 |
| CON-016 | Bulk status update (multiple opps) | All independent | P1 |
| CON-017 | Status query during transition | Before or after, not partial | P1 |
| CON-018 | Concurrent approve + cancel (impossible combo) | Appropriate error | P1 |
| CON-019 | Session timeout during confirm dialog | Redirect, no partial change | P2 |
| CON-020 | Parallel DoA2 lookups | Both correct | P2 |
| CON-021 | Audit trail concurrent appends | All preserved | P1 |
| CON-022 | Dashboard count during mass transitions | Eventually consistent | P2 |
| CON-023 | Notification queue during high volume | All eventually delivered | P2 |
| CON-024 | Concurrent org unit update + transition | Transition uses consistent data | P2 |
| CON-025 | Multiple rapid transitions same opp | Sequential processing, all recorded | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | IsValidTransition(I&P→GO) | Valid | true | P0 |
| UNT-002 | IsValidTransition(GO→I&P) | Invalid | false | P0 |
| UNT-003 | IsValidTransition(CANCELLED→GO) | Invalid | false | P1 |
| UNT-004 | ValidateReason — non-empty | "Valid reason" | Passes | P1 |
| UNT-005 | ValidateReason — empty | "" | Error | P1 |

### Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | FormatStageLabel | GO | "GO" | P2 |
| UNT-007 | FormatStatusLabel | Closed | "Closed" | P2 |
| UNT-008 | FormatHistoryEntry | Action+User+Time | Formatted string | P1 |

### Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | CountPendingDecisions | DoA2 + opps | Correct count | P1 |
| UNT-010 | CalculateTimeInStage | Enter/Exit timestamps | Duration | P2 |
| UNT-011 | CountTransitions | History entries | Total count | P2 |
| UNT-012 | AverageDecisionTime | Multiple histories | Avg duration | P2 |
| UNT-013 | CalculateSLACompliance | Deadlines + actuals | Compliance % | P2 |

### Status Logic (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-014 | GetAvailableActions — I&P/Draft (OM) | Current state + role | [Submit, Cancel] | P0 |
| UNT-015 | GetAvailableActions — GO/Active (DoA2) | Current state + role | [Approve, Reject] | P0 |
| UNT-016 | GetAvailableActions — GO/Active (OM) | Current state + role | [Recall] | P0 |
| UNT-017 | GetAvailableActions — CANCELLED (OM) | Current state + role | [Reopen] | P1 |
| UNT-018 | GetAvailableActions — GO/Closed | Current state | [] (no actions) | P0 |

### Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | FilterByStage | All opps + stage filter | Correct subset | P1 |
| UNT-020 | SortHistoryEntries | Unsorted history | Chronological order | P2 |
| UNT-021 | GroupByStatus | Opps in various states | Correct groups | P2 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Submit for Go (API) | < 3 seconds | P1 |
| PRF-002 | DoA2 approval (API) | < 2 seconds | P1 |
| PRF-003 | Cancel (API) | < 2 seconds | P1 |
| PRF-004 | Recall (API) | < 2 seconds | P1 |
| PRF-005 | Reopen (API) | < 2 seconds | P1 |
| PRF-006 | Workflow history load (500 entries) | < 1 second | P1 |
| PRF-007 | Filter opps by stage (10K opps) | < 2 seconds | P1 |
| PRF-008 | Dashboard status distribution | < 3 seconds | P1 |
| PRF-009 | DoA2 pending decisions list | < 2 seconds | P1 |
| PRF-010 | Notification dispatch (single) | < 5 seconds | P2 |
| PRF-011 | 50 concurrent submissions | All < 10 seconds | P2 |
| PRF-012 | 20 concurrent approvals | All < 5 seconds | P2 |
| PRF-013 | 100 concurrent reads during transition | Avg < 3 seconds | P2 |
| PRF-014 | Memory during transition | < 30MB increase | P2 |
| PRF-015 | Memory for history page | < 20MB | P2 |
| PRF-016 | GC pressure during bulk transitions | No excessive GC pauses | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Profile | Duration | Success Criteria | Priority |
|----|---------|----------|-----------------|----------|
| LDT-001 | 100 workflow actions/hour | 1 hour | Error < 1% | P2 |
| LDT-002 | 50 concurrent DoA2 decisions | 30 min | All succeed | P2 |
| LDT-003 | 200 concurrent status queries | 1 hour | P99 < 5s | P2 |
| LDT-004 | 500 submissions in 1 minute | 1 min | All processed | P2 |
| LDT-005 | 1000 reads during mass approval | 5 min | Responsive | P2 |
| LDT-006 | Increase until 5% error rate | Until failure | Identify limits | P2 |
| LDT-007 | Fill notification queue | Until full | Backpressure handled | P2 |
| LDT-008 | 10000 concurrent reads | 5 min | Rate limited gracefully | P2 |
| LDT-009 | Recovery after spike | 5 min | Normal within 2 min | P2 |
| LDT-010 | Service restart during workflows | Recovery | No data loss | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|------------|------------|
| Valid state transitions | POS-001 to POS-007, FUN-001, UNT-001 to UNT-003 |
| Invalid transitions blocked | NEG-001 to NEG-015, FUN-001 |
| Role-based access | NEG-016 to NEG-030, SEC-011 to SEC-020 |
| Mandatory reasons | NEG-031 to NEG-040, FUN-012 to FUN-015 |
| Audit trail | FUN-041 to FUN-050, POS-010 |
| Read-only enforcement | POS-014, FUN-006, FUN-007 |
| Notification chain | POS-011, INT-010, INT-047 |
| Concurrency safety | CON-001 to CON-025, BND-024 |
| Performance targets | PRF-001 to PRF-016 |

---

**Last Updated:** 2026-02-11  
**Supersedes:** Previous version (45 tests, 5 categories)  
**Status:** Ready for Execution  
**Compliance:** ✅ 10-Category Standard, ✅ 3:1 Ratio (140 ≥ 105)
