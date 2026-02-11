# PNO-969: Go/No Go Decision — Comprehensive Test Cases

**JIRA Reference:** [PNO-969](https://unops.atlassian.net/browse/PNO-969) — Sending the Opportunity to decision makers (Go / No Go decision)  
**Epic:** The Go/No Go Decision  
**Sprint:** Opportunity+ Sprint #34  
**Priority:** High  
**Created:** 2026-02-11  
**Restructured:** 2026-02-11 (10-category standard)  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc` and `QA_TESTER_PLAYBOOK.md` v1.4)

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

## Key Design Decisions

| Item | Decision | Source |
|------|----------|--------|
| **Initial status** | **Draft** (not Active) | Issam workflow map (2026-02-10) |
| **Collaborator role** | **Not yet implemented** — only OM can perform workflow actions | Issam (2026-01-23) |
| **Cancel = Closed** | Cancel moves stage to CANCELLED, status to Closed | Fouad/Roz (2026-01-23) |
| **Rejection = NO GO** | Rejection moves stage to NO GO (not back to I&P) | PRD and AC Section 5 |
| **DoA2 = Decision Maker** | Lowest level DoA starting at Level 2 for responsible org unit | AC Section 5 |
| **Cancel requires reason** | Free text justification required | AC Section 5 |
| **OM role transfer** | OM→Collaborator when new OM assigned | AC Section 1 (Bug: PNO-1193) |

## Stage/Status Transition Matrix

| # | Role | Current Stage | Current Status | Action | Target Stage | Target Status |
|---|------|---------------|----------------|--------|-------------|---------------|
| 1 | OM | Identify & Profile | Draft | Submit for Go | GO | Active |
| 2 | Collaborator | Identify & Profile | Draft | Submit for Go | Access Denied | — |
| 3 | OM | Identify & Profile | Draft | Reject | NO GO | Closed |
| 4 | Collaborator | Identify & Profile | Draft | Reject | Access Denied | — |
| 5 | OM | Identify & Profile | Draft | Cancel | CANCELLED | Closed |
| 6 | Collaborator | Identify & Profile | Draft | Cancel | Access Denied | — |
| 7 | OM | Cancelled | Closed | Reopen | Identify & Profile | Draft |
| 8 | Collaborator | Cancelled | Closed | Reopen | Access Denied | — |
| 9 | OM | No-Go | Closed | Reopen | Identify & Profile | Draft |
| 10 | Collaborator | No-Go | Closed | Reopen | Access Denied | — |

---

## Known Issues & Blockers

| JIRA ID | Issue | Impact on Testing |
|---------|-------|-------------------|
| PNO-1193 | OM role transfer not working (DEF-010) | POS-029 blocked |
| PNO-1171 | Reject action appears twice in history (DEF-011) | FUN-040 affected |
| — | Collaborator role not implemented | NEG-001 through NEG-010 verify access denial (expected) |
| — | Initial status "Draft" vs AC saying "Active" | Pending requirements from Roz/Issam |
| — | Inactive OM visibility requires DB deactivation | FUN-033 blocked |
| — | Additional Remarks missing character count | BND-033 — refinement ticket needed |

---

## §1 Positive Tests (Happy Path)

> **Count: 35** | **Minimum: 30-50** | ✅ COMPLIANT

### Detailed Test Cases (P0)

#### POS-001: OM Submit for Go — I&P/Draft → GO/Active

**Priority:** P0 | **AC:** Section 1, Section 5  
**Precondition:** Opportunity in I&P/Draft. All mandatory fields populated. DoA2 exists. Opportunity Statement generated. User is OM.

**Steps:**
1. Log in as OM for the opportunity
2. Navigate to opportunity detail page
3. Verify "Submit for Go" action visible
4. Click "Submit for Go Decision"
5. Complete mandatory acknowledgement checkbox
6. Optionally enter additional remarks
7. Confirm submission

**Expected Result:**
- Stage → **GO**, Status → **Active**
- On-screen confirmation displayed
- Opportunity becomes read-only for OM
- "In Workflow" indicator visible in list view
- DoA2 receives email notification
- Workflow history records submission with timestamp and user

---

#### POS-002: OM Cancel — I&P/Draft → CANCELLED/Closed

**Priority:** P0 | **AC:** Section 5 | **Status:** PASS (Silvia, 2026-02-10)  
**Precondition:** Opportunity in I&P/Draft, not in workflow. User is OM.

**Steps:**
1. Log in as OM
2. Navigate to opportunity in I&P stage
3. Verify "Cancel" action available
4. Click "Cancel"
5. Enter reason for not pursuing (free text)
6. Confirm cancellation

**Expected Result:**
- Stage → **CANCELLED**, Status → **Closed**
- Opportunity becomes read-only
- Workflow history records: Action=Cancel, Reason=[text], Timestamp
- "Reopen" action becomes available

---

#### POS-003: OM Reopen from Cancelled — CANCELLED/Closed → I&P/Draft

**Priority:** P0 | **AC:** Section 5 | **Status:** PASS (Silvia, 2026-02-10)  
**Precondition:** Opportunity in CANCELLED/Closed. User is OM.

**Steps:**
1. Log in as OM
2. Navigate to cancelled opportunity
3. Verify "Reopen" action available
4. Click "Reopen"
5. Confirm reopen action

**Expected Result:**
- Stage → **Identify & Profile**, Status → **Draft**
- Opportunity becomes editable again
- Workflow history records: Action=Reopen, Timestamp

---

#### POS-004: OM Reopen from No-Go — NO GO/Closed → I&P/Draft

**Priority:** P0 | **AC:** Section 5  
**Precondition:** Opportunity in NO GO/Closed. User is OM.

**Steps:**
1. Log in as OM
2. Navigate to rejected opportunity
3. Click "Reopen"
4. Confirm action

**Expected Result:**
- Stage → **Identify & Profile**, Status → **Draft**
- OM can modify fields and resubmit
- Workflow history records: Action=Reopen

---

#### POS-005: DoA2 Approves — GO/Active → GO/Closed (PASS)

**Priority:** P0 | **AC:** Section 5  
**Precondition:** Opportunity in GO/Active (submitted, in workflow). User is DoA2 for responsible org unit.

**Steps:**
1. Log in as DoA2
2. Navigate to opportunity pending decision
3. Click "Approve" / "Go"
4. Enter optional approval comments
5. Confirm

**Expected Result:**
- Stage remains **GO**, Status → **Closed** (approved)
- OM receives approval notification email
- Internal stakeholders notified
- Workflow history records: Action=Approve, User=DoA2, Timestamp

---

#### POS-006: DoA2 Rejects — GO/Active → NO GO/Closed

**Priority:** P0 | **AC:** Section 5  
**Precondition:** Opportunity in GO/Active (in workflow). User is DoA2.

**Steps:**
1. Log in as DoA2
2. Navigate to opportunity
3. Click "Reject"
4. Enter mandatory rejection reason
5. Confirm

**Expected Result:**
- Stage → **NO GO**, Status → **Closed**
- OM receives rejection notification with reason
- Workflow history records: Action=Reject, Reason=[text]

---

#### POS-007: DoA2 Lookup for Single Holder

**Priority:** P0 | **AC:** Section 5 (FR-1)  
**Precondition:** Org unit B5503 (India) has exactly one DoA2 (Dominic).

**Steps:**
1. Set opportunity responsible org unit to B5503
2. System queries EntityUserRole (Code="DoA2_OrganizationHierarchy")
3. System identifies Dominic as DoA2

**Expected Result:** DoA2 correctly identified. Submission routed to Dominic.

---

#### POS-008: DoA2 Lookup with Hierarchy

**Priority:** P0 | **AC:** Section 5  
**Precondition:** Org unit B5505 (Sri Lanka) has DoA2 (Perminder).

**Steps:**
1. Set responsible org unit to B5505
2. System queries EntityUserRole with hierarchy chain

**Expected Result:** DoA2 = Perminder. Submission routed correctly.

---

#### POS-009: Mandatory Acknowledgement Statement

**Priority:** P0 | **AC:** Section 3  
**Precondition:** Opportunity ready for submission. User is OM.

**Steps:**
1. Click "Submit for Go Decision"
2. Verify acknowledgement dialog appears with org unit reference
3. Check the mandatory acknowledgement checkbox
4. Confirm

**Expected Result:** Submission proceeds only after acknowledgement checked.

---

#### POS-010: Additional Remarks on Submission

**Priority:** P0 | **AC:** Section 3  
**Precondition:** Submission dialog open.

**Steps:**
1. Enter additional remarks text in the remarks field
2. Confirm submission

**Expected Result:** Remarks saved and visible in workflow history.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-011 | Auto-regenerate Opportunity Statement | Opp in I&P with all fields | Submit triggers regeneration | Statement reflects latest data | P1 |
| POS-012 | Opportunity Statement review dialog | Statement exists | Click Submit → statement preview shown | OM can review before confirming | P1 |
| POS-013 | On-screen submission confirmation | Opp submitted | Submit confirmed | Success toast/message displayed | P1 |
| POS-014 | OM recall capability | Opp in GO/Active | OM clicks Recall | Opp returns to I&P/Draft | P0 |
| POS-015 | Recall with justification | Opp in GO/Active | OM enters justification + recalls | Justification recorded in history | P1 |
| POS-016 | OM field always populated | Any opportunity | View OM field | OM field never blank | P1 |
| POS-017 | Standardized position titles | View user roles | Check title display | Job titles shown in standard format | P2 |
| POS-018 | Original decision makers visible | Opp with DoA changes | View DoA section | Original + current DoA displayed | P1 |
| POS-019 | Stage stepper shows GO | Opp in GO stage | View stage stepper | GO step highlighted/active | P1 |
| POS-020 | Workflow history complete | Multiple actions taken | View history | All actions listed chronologically | P1 |
| POS-021 | Submit with all optional fields | All fields populated | Submit for Go | No errors, all data preserved | P2 |
| POS-022 | Submit with minimum required only | Only required fields | Submit for Go | Submission succeeds | P1 |
| POS-023 | Multiple countries of implementation | 3+ countries listed | Submit for Go | All countries preserved, DoA2 based on resp. org unit | P1 |
| POS-024 | DoA2 views pending decision list | DoA2 has pending items | Log in as DoA2 | Pending decisions visible in dashboard/list | P1 |
| POS-025 | OM receives approval notification | DoA2 approved | Check OM inbox | Approval email received with details | P1 |
| POS-026 | Cancel with detailed reason text | Opp in I&P | Cancel with 500+ char reason | Reason fully saved and displayed | P2 |
| POS-027 | Reopen and modify before resubmit | Opp reopened from NO GO | Edit fields + resubmit | Modified data in new submission | P1 |
| POS-028 | Multiple sequential workflow cycles | Opp: Submit→Reject→Reopen→Submit→Approve | Full cycle | All transitions recorded correctly | P1 |
| POS-029 | OM role transfer to Collaborator | OM changed | New OM assigned | Previous OM becomes Collaborator (**BLOCKED: PNO-1193**) | P0 |
| POS-030 | New DoA2 holder can act | DoA2 changed after submission | New DoA2 approves | Approval accepted from new holder | P1 |
| POS-031 | In-workflow indicator on list view | Opp in workflow | View opportunity list | "In Workflow" badge/indicator visible | P1 |
| POS-032 | DoA pathway display on detail | Opp submitted | View detail page | DoA2/DoA3 pathway shown read-only | P1 |
| POS-033 | End-to-end: Submit → Approve → GO | Complete opportunity | Full workflow | Final stage=GO, status=Closed(PASS) | P0 |
| POS-034 | End-to-end: Submit → Reject → Reopen → Resubmit | Rejected opp | Full cycle | Successfully resubmitted after fix | P0 |
| POS-035 | End-to-end: Cancel → Reopen → Submit → Approve | Cancelled opp | Full cycle | Workflow history: Cancel→Reopen→Submit→Approve | P0 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 2.1 Collaborator Access Denial (10 tests)

| ID | Action Attempted | Role | Current State | Expected Result | Priority |
|----|-----------------|------|---------------|-----------------|----------|
| NEG-001 | Submit for Go | Collaborator | I&P/Draft | Access Denied — Submit button not visible or disabled | P0 |
| NEG-002 | Reject workflow | Collaborator | I&P/Draft | Access Denied | P0 |
| NEG-003 | Cancel opportunity | Collaborator | I&P/Draft | Access Denied — Cancel action not available | P0 |
| NEG-004 | Reopen from Cancelled | Collaborator | CANCELLED/Closed | Access Denied | P0 |
| NEG-005 | Reopen from No-Go | Collaborator | NO GO/Closed | Access Denied | P0 |
| NEG-006 | Recall submission | Collaborator | GO/Active | Access Denied — Recall not available | P0 |
| NEG-007 | Approve opportunity | Collaborator | GO/Active | Access Denied — Approve not available | P0 |
| NEG-008 | Change OM assignment | Collaborator | I&P/Draft | Access Denied — OM field read-only | P1 |
| NEG-009 | Edit during workflow | Collaborator | GO/Active | All fields read-only | P1 |
| NEG-010 | View DoA pathway | Collaborator | GO/Active | Can view but not modify DoA pathway | P1 |

### 2.2 Invalid State Transitions (10 tests)

| ID | Current State | Invalid Action | Expected Result | Priority |
|----|---------------|---------------|-----------------|----------|
| NEG-011 | GO/Active (in workflow) | Cancel | Cancel not available while in workflow | P0 |
| NEG-012 | GO/Closed (approved) | Submit for Go | Submit not available — already decided | P0 |
| NEG-013 | NO GO/Closed | Submit for Go directly | Must Reopen first | P0 |
| NEG-014 | CANCELLED/Closed | Submit for Go directly | Must Reopen first | P0 |
| NEG-015 | I&P/Draft (in workflow) | Cancel | Cancel disabled during active workflow | P1 |
| NEG-016 | GO/Closed (approved) | Cancel | Cancel not available on decided opportunity | P1 |
| NEG-017 | GO/Active | Reopen | Reopen not available while pending decision | P1 |
| NEG-018 | I&P/Draft | Approve | Approve not available — not submitted | P1 |
| NEG-019 | I&P/Draft | Reject | Reject available only for submitted decisions | P1 |
| NEG-020 | CANCELLED/Closed | Reject | Reject not available on cancelled opportunity | P1 |

### 2.3 Mandatory Field Validation (16 tests)

| ID | Missing Field | Submit Action | Expected Error | Priority |
|----|--------------|--------------|---------------|----------|
| NEG-021 | Context & Challenges (Analysis section) | Submit for Go | "Analysis section incomplete" validation error | P0 |
| NEG-022 | UNOPS Strategic Mission(s) | Submit for Go | "At least one strategic mission required" | P0 |
| NEG-023 | Expected Impact | Submit for Go | "Expected Impact is required" | P0 |
| NEG-024 | Expected Outcomes | Submit for Go | "Expected Outcomes is required" | P0 |
| NEG-025 | SDG Alignment (min 1) | Submit for Go | "At least one SDG required" | P0 |
| NEG-026 | Funding Partner with amount | Submit for Go | "Funding partner and amount required" | P0 |
| NEG-027 | Client Partner | Submit for Go | "Client partner is required" | P0 |
| NEG-028 | Products & Services | Submit for Go | "At least one product/service required" | P0 |
| NEG-029 | Countries of Implementation | Submit for Go | "At least one country required" | P0 |
| NEG-030 | Target Signing Date | Submit for Go | "Target signing date is required" | P1 |
| NEG-031 | Implementation Start/End | Submit for Go | "Implementation dates required" | P1 |
| NEG-032 | Proposed Initiative Type | Submit for Go | "Initiative type is required" | P1 |
| NEG-033 | Opportunity Manager role | Submit for Go | "Opportunity Manager must be assigned" | P1 |
| NEG-034 | Opportunity Statement not generated | Submit for Go | "Generate Opportunity Statement first" | P0 |
| NEG-035 | Estimated Beneficiaries OR acknowledgement | Submit for Go | "Provide beneficiary estimate or acknowledge" | P1 |
| NEG-036 | High Risk not acknowledged | Submit for Go | "High risk acknowledgement required" | P1 |

### 2.4 DoA2 Lookup Failures (5 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| NEG-037 | No DoA2 configured for org unit | Submission blocked: "No decision maker found for [org unit]" | P0 |
| NEG-038 | DoA2 user account deactivated | Submission blocked or routed to next in hierarchy | P1 |
| NEG-039 | Org unit not in EntityUserRole table | Submission blocked with clear error | P1 |
| NEG-040 | DoA2 has expired delegation | Error: delegation expired, route to original | P2 |
| NEG-041 | Circular org unit hierarchy | System handles gracefully, no infinite loop | P2 |

### 2.5 Unauthorized Users (10 tests)

| ID | User Role | Action | Expected Result | Priority |
|----|-----------|--------|-----------------|----------|
| NEG-042 | General User (no opp access) | Submit for Go | 403 Forbidden / Action not visible | P0 |
| NEG-043 | Partner User | Approve decision | 403 Forbidden | P0 |
| NEG-044 | Org Unit Admin (not OM) | Cancel opportunity | Action not available | P1 |
| NEG-045 | DoA2 of different org unit | Approve | 403 — not authorized for this org unit | P0 |
| NEG-046 | Expired session user | Submit for Go | Redirect to login | P1 |
| NEG-047 | Non-OM user | Recall submission | Recall not available | P0 |
| NEG-048 | Authenticated user, no role | Any workflow action | All actions hidden/disabled | P1 |
| NEG-049 | OM of different opportunity | Submit for Go on this opp | 403 — not OM for this opportunity | P0 |
| NEG-050 | DoA3 (higher level) | Approve before DoA2 | Action not available at this stage | P1 |
| NEG-051 | System service account | Any UI action | UI not accessible / API returns 403 | P2 |

### 2.6 Input Validation Failures (9 tests)

| ID | Field | Invalid Input | Expected Error | Priority |
|----|-------|--------------|---------------|----------|
| NEG-052 | Cancel reason | Empty string | "Reason is required" | P0 |
| NEG-053 | Rejection reason | Empty string | "Rejection reason is required" | P0 |
| NEG-054 | Recall justification | Empty string | "Justification is required" | P0 |
| NEG-055 | Acknowledgement checkbox | Not checked | Submit button disabled | P0 |
| NEG-056 | Funding amount | Negative number | "Amount must be positive" | P1 |
| NEG-057 | Implementation end date | Before start date | "End date must be after start date" | P1 |
| NEG-058 | Target signing date | Past date | Warning: "Date is in the past" | P2 |
| NEG-059 | Budget USD | Non-numeric text | "Invalid number format" | P1 |
| NEG-060 | Additional remarks | Only whitespace | Treated as empty / trimmed | P2 |

### 2.7 Dependency & Network Failures (10 tests)

| ID | Failure Scenario | Action | Expected Behavior | Priority |
|----|-----------------|--------|-------------------|----------|
| NEG-061 | Database timeout during submit | Submit for Go | Error message, submission not saved, retry possible | P1 |
| NEG-062 | Email service unavailable | Submit (notification step) | Submission succeeds, notification queued for retry | P1 |
| NEG-063 | EntityUserRole table unavailable | DoA2 lookup | Clear error: "Cannot determine decision maker" | P1 |
| NEG-064 | Opportunity Statement gen fails | Pre-submit check | Error: "Statement generation failed" + retry | P1 |
| NEG-065 | Concurrent DB lock on opportunity | Submit for Go | Optimistic concurrency error, retry prompt | P2 |
| NEG-066 | API rate limit exceeded | Rapid re-submissions | 429 Too Many Requests | P2 |
| NEG-067 | File storage unavailable | Statement regeneration | Graceful error, previous statement preserved | P2 |
| NEG-068 | Auth token expired mid-workflow | Click Confirm | Redirect to login, preserve state | P1 |
| NEG-069 | Network disconnect during approval | DoA2 approves | Transaction rolls back, no partial state | P1 |
| NEG-070 | Org hierarchy service timeout | DoA2 lookup | Timeout error with retry option | P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 3.1 String Length Boundaries (15 tests)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Opportunity Name | 1 | 255 | ✅ Accept | ✅ Accept | ❌ Truncate/Reject | P1 |
| BND-002 | Description | 1 | 5000 | ✅ Accept | ✅ Accept | ❌ Reject | P1 |
| BND-003 | Cancel reason | 1 | 2000 | ✅ Accept | ✅ Accept | ❌ Reject | P1 |
| BND-004 | Rejection reason | 1 | 2000 | ✅ Accept | ✅ Accept | ❌ Reject | P1 |
| BND-005 | Additional remarks | 0 | 2000 | ✅ Accept (empty) | ✅ Accept | ❌ Reject | P1 |
| BND-006 | Recall justification | 1 | 2000 | ✅ Accept | ✅ Accept | ❌ Reject | P1 |
| BND-007 | Context & Challenges | 1 | 10000 | ✅ Accept | ✅ Accept | ❌ Reject | P2 |
| BND-008 | Expected Impact | 1 | 5000 | ✅ Accept | ✅ Accept | ❌ Reject | P2 |
| BND-009 | Expected Outcomes | 1 | 5000 | ✅ Accept | ✅ Accept | ❌ Reject | P2 |
| BND-010 | Approval comments | 0 | 2000 | ✅ (optional) | ✅ Accept | ❌ Reject | P2 |
| BND-011 | Opportunity Name exactly 1 char | — | — | ✅ | — | — | P2 |
| BND-012 | Opportunity Name exactly 255 chars | — | — | — | ✅ | — | P2 |
| BND-013 | Cancel reason exactly 1 char | — | — | ✅ | — | — | P2 |
| BND-014 | Rejection reason exactly max chars | — | — | — | ✅ | — | P2 |
| BND-015 | Description with only newlines | — | — | ✅ Accept | — | — | P2 |

### 3.2 Numeric Boundaries (10 tests)

| ID | Field | Zero | Negative | Very Large | Decimal Precision | Priority |
|----|-------|------|----------|-----------|------------------|----------|
| BND-016 | Budget USD | ✅ Accept | ❌ Reject | ✅ Accept (max DB limit) | 2 decimal places | P1 |
| BND-017 | Funding amount | ✅ Accept | ❌ Reject | ✅ up to 999,999,999.99 | 2 decimal places | P1 |
| BND-018 | Estimated beneficiaries | ✅ Accept (0) | ❌ Reject | ✅ Accept | Integer only | P1 |
| BND-019 | Budget = 0.01 (minimum positive) | — | — | ✅ Accept | — | P2 |
| BND-020 | Budget = 999,999,999.99 | — | — | ✅ Accept | — | P2 |
| BND-021 | Budget = 1,000,000,000 (overflow) | — | — | ❌ Reject | — | P2 |
| BND-022 | Funding amount with 3+ decimals | — | — | — | ❌ Round or reject | P2 |
| BND-023 | Beneficiaries = MAX_INT | — | — | Handle gracefully | — | P2 |
| BND-024 | Multiple funding partners sum to 0 | — | — | ❌ Reject (total must be >0) | — | P1 |
| BND-025 | Budget currency conversion edge | — | — | Precision preserved | — | P2 |

### 3.3 Date Boundaries (10 tests)

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-026 | Target signing = today | Current date | ✅ Accept | P1 |
| BND-027 | Target signing = tomorrow | Current date + 1 | ✅ Accept | P1 |
| BND-028 | Target signing = yesterday | Current date - 1 | ⚠️ Warning (past date) | P1 |
| BND-029 | Implementation start = signing date | Same date | ✅ Accept | P2 |
| BND-030 | Implementation end = start date | Same date | ✅ Accept (zero duration) | P2 |
| BND-031 | Implementation span = 10 years | Start to +10yr | ✅ Accept | P2 |
| BND-032 | Feb 29 leap year as signing date | 2028-02-29 | ✅ Accept (valid leap year) | P2 |
| BND-033 | Dec 31 → Jan 1 fiscal year boundary | Year boundary | ✅ Correct fiscal year assignment | P2 |
| BND-034 | Implementation end = year 2099 | Far future | ✅ Accept or ⚠️ Warning | P2 |
| BND-035 | All dates = same date | Start=End=Signing=Today | ✅ Accept | P2 |

### 3.4 Collection Boundaries (15 tests)

| ID | Collection | State | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-036 | Funding Partners | Exactly 1 | ✅ Accept (minimum) | P1 |
| BND-037 | Funding Partners | 20+ | ✅ Accept (many) | P1 |
| BND-038 | Client Partners | Exactly 1 | ✅ Accept | P1 |
| BND-039 | Client Partners | 50+ | ✅ Accept or defined limit | P2 |
| BND-040 | SDG Alignment | Exactly 1 | ✅ Accept (minimum) | P1 |
| BND-041 | SDG Alignment | All 17 SDGs | ✅ Accept (maximum) | P1 |
| BND-042 | Countries | Exactly 1 | ✅ Accept | P1 |
| BND-043 | Countries | 50+ countries | ✅ Accept or defined limit | P2 |
| BND-044 | Products & Services | Exactly 1 | ✅ Accept | P1 |
| BND-045 | Products & Services | 100+ | ✅ Accept or defined limit | P2 |
| BND-046 | Strategic Missions | Exactly 1 | ✅ Accept (minimum) | P1 |
| BND-047 | DoA2 holders | Multiple (2+) for same org unit | All receive notification | P1 |
| BND-048 | Workflow history entries | 100+ actions | ✅ Display with pagination/scroll | P2 |
| BND-049 | Stakeholders | 0 (none assigned) | ✅ Accept (optional field) | P2 |
| BND-050 | Risks | 0 (none identified) | ✅ Accept if risk ack provided | P2 |

### 3.5 Unicode & Special Characters (10 tests)

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-051 | Cancel reason | Arabic: "سبب الإلغاء" | ✅ Stored and displayed correctly | P1 |
| BND-052 | Opportunity Name | Chinese: "机会名称测试" | ✅ Stored and displayed correctly | P1 |
| BND-053 | Rejection reason | French accents: "Rejeté pour données incomplètes" | ✅ Stored correctly | P1 |
| BND-054 | Additional remarks | Emoji: "Great opportunity! 🎉🌍" | ✅ Accept or ❌ Reject with clear msg | P2 |
| BND-055 | Description | Mixed RTL/LTR text | ✅ Rendered correctly | P2 |
| BND-056 | Context | HTML entities: `&amp; &lt; &gt;` | ✅ Escaped, not rendered as HTML | P1 |
| BND-057 | Outcomes | Newlines and tabs | ✅ Preserved in display | P2 |
| BND-058 | Budget field | Locale-specific: "1.234,56" (EU format) | ✅ Parsed correctly or clear error | P1 |
| BND-059 | Name | Diacritics: "São Paulo Ünited Ñoño" | ✅ Stored correctly | P2 |
| BND-060 | Reason field | Max length with multibyte chars | ✅ Character count by chars, not bytes | P2 |

### 3.6 Workflow State Boundaries (10 tests)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-061 | Submit at exact moment DoA2 deactivated | Race between submit and user deactivation | Clear error or route to alternate | P1 |
| BND-062 | Reopen immediately after cancel | Cancel then instant Reopen | Both actions recorded, state correct | P1 |
| BND-063 | Multiple rapid submits (button spam) | Click Submit 5x rapidly | Only first processed, idempotent | P1 |
| BND-064 | Cancel with reason at exact max length | 2000 chars exactly | ✅ Accept, fully stored | P1 |
| BND-065 | Submission at midnight UTC | 23:59:59 → 00:00:00 | Correct date recorded | P2 |
| BND-066 | Reopen after system date change | DST transition | Timestamps correct | P2 |
| BND-067 | Workflow with exactly 1ms between actions | Rapid sequential actions | All recorded with distinct timestamps | P2 |
| BND-068 | Submit with all collections at minimum (1 each) | Minimum viable data | ✅ Accept | P1 |
| BND-069 | Stage stepper at last possible stage | GO/Closed (approved) | Stepper shows completion | P1 |
| BND-070 | Opportunity with 0 collaborators | OM only | ✅ All workflow actions work | P1 |

---

## §4 Functional Tests (Business Rules)

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 4.1 Workflow Rules (15 tests)

| ID | Business Rule | Test Scenario | Expected Outcome | Priority |
|----|--------------|--------------|-----------------|----------|
| FUN-001 | Rejection → NO GO (not previous stage) | DoA2 rejects | Stage=NO GO, NOT back to I&P | P0 |
| FUN-002 | Cancel only from I&P, not in workflow | Try cancel while in GO/Active | Cancel action hidden/disabled | P0 |
| FUN-003 | OM recall returns to I&P/Draft | OM recalls from GO/Active | Stage→I&P, Status→Draft | P0 |
| FUN-004 | Only OM can recall (not submitter-specific) | Any OM of the opp recalls | Recall succeeds regardless of who submitted | P0 |
| FUN-005 | Read-only after submission for OM | Opp in GO/Active | All content fields disabled/read-only for OM | P0 |
| FUN-006 | Read-only after submission for Collaborator | Opp in GO/Active | All fields read-only for Collaborator | P0 |
| FUN-007 | Reopen from NO GO restores editability | Reopen rejected opp | Fields become editable again | P1 |
| FUN-008 | Reopen from CANCELLED restores editability | Reopen cancelled opp | Fields become editable again | P1 |
| FUN-009 | GO decision is final (no further edits) | Opp approved (GO/Closed) | Permanently read-only | P0 |
| FUN-010 | Cancel requires mandatory reason | Cancel without reason | Submit disabled until reason entered | P0 |
| FUN-011 | Recall requires mandatory justification | Recall without justification | Action blocked | P0 |
| FUN-012 | Rejection requires mandatory reason | Reject without reason | Action blocked | P0 |
| FUN-013 | In-Workflow indicator shows when submitted | Submit for Go | "In Workflow" visible on list/card view | P1 |
| FUN-014 | In-Workflow indicator removed after decision | Approved or Rejected | "In Workflow" removed | P1 |
| FUN-015 | In-Workflow indicator removed after recall | OM recalls | "In Workflow" removed | P1 |

### 4.2 Validation Rules (15 tests)

| ID | Validation Rule | Valid Scenario | Invalid Scenario | Priority |
|----|----------------|---------------|-----------------|----------|
| FUN-016 | All mandatory fields must be populated before submit | All fields filled | Any field empty → error list | P0 |
| FUN-017 | Server-side validation shows all failures as list | 5 fields missing | All 5 shown at once (not one-at-a-time) | P0 |
| FUN-018 | DoA2 must exist for responsible org unit | DoA2 configured | No DoA2 → submission blocked | P0 |
| FUN-019 | Opportunity Statement must be generated | Statement exists | No statement → blocked | P0 |
| FUN-020 | Acknowledgement is mandatory | Checkbox checked | Unchecked → submit disabled | P0 |
| FUN-021 | Implementation end date >= start date | End after start | End before start → error | P1 |
| FUN-022 | At least 1 funding partner required | 1+ partners | 0 partners → error | P1 |
| FUN-023 | At least 1 SDG selected | 1+ SDGs | 0 SDGs → error | P1 |
| FUN-024 | At least 1 country of implementation | 1+ countries | 0 countries → error | P1 |
| FUN-025 | Budget amount must be non-negative | 0 or positive | Negative → error | P1 |
| FUN-026 | Funding partner must have currency | Amount specified | No currency → error | P1 |
| FUN-027 | Name field max length enforced | 255 chars | 256+ → error | P2 |
| FUN-028 | Country-Org Unit mismatch warning | Country ≠ org unit country | Warning displayed (non-blocking) | P1 |
| FUN-029 | Collaborator submission warning | Collaborator submits (future) | Non-OM submitter warning dialog | P1 |
| FUN-030 | High risk requires acknowledgement | High risk flagged | Must acknowledge before submit | P1 |

### 4.3 Constraint Rules (10 tests)

| ID | Constraint | Test | Expected Result | Priority |
|----|-----------|------|-----------------|----------|
| FUN-031 | Only one active workflow per opportunity | Submit while already in workflow | Second submit blocked | P0 |
| FUN-032 | OM field can never be blank | Remove OM | System prevents blank OM | P0 |
| FUN-033 | Inactive OM handling | OM deactivated | System shows inactive OM, allows reassignment (**BLOCKED: needs DB**) | P1 |
| FUN-034 | Unique opportunity per workflow instance | Same opp, two workflows | Prevented — one active at a time | P1 |
| FUN-035 | Stage must follow valid transitions only | Skip from I&P to GO/Closed | Invalid transition rejected | P1 |
| FUN-036 | Approval can only happen once per submission | DoA2 approves twice | Second approval ignored / idempotent | P1 |
| FUN-037 | Rejection can only happen once per submission | DoA2 rejects twice | Second rejection ignored | P1 |
| FUN-038 | Deleted opportunities cannot enter workflow | Soft-deleted opp | Submit not available | P1 |
| FUN-039 | Archived opportunities cannot enter workflow | Archived opp | Submit not available | P2 |
| FUN-040 | Workflow history entries are immutable | Try to edit history | History records cannot be modified (**Affected by PNO-1171**) | P1 |

### 4.4 Audit Rules (10 tests)

| ID | Action | Expected Audit Entry | Priority |
|----|--------|---------------------|----------|
| FUN-041 | Submit for Go | User, Timestamp, Action="Submit for Go", Target=DoA2 name | P0 |
| FUN-042 | Cancel with reason | User, Timestamp, Action="Cancel", Reason=[text] | P0 |
| FUN-043 | Reopen from Cancelled | User, Timestamp, Action="Reopen" | P1 |
| FUN-044 | Reopen from No-Go | User, Timestamp, Action="Reopen" | P1 |
| FUN-045 | DoA2 Approve | User, Timestamp, Action="Approve", Comments=[text] | P0 |
| FUN-046 | DoA2 Reject | User, Timestamp, Action="Reject", Reason=[text] | P0 |
| FUN-047 | OM Recall | User, Timestamp, Action="Recall", Justification=[text] | P0 |
| FUN-048 | OM Role Transfer | User, Timestamp, OldOM=[name], NewOM=[name] | P1 |
| FUN-049 | Acknowledgement recorded | User, Timestamp, OrgUnit=[name], Acknowledged=true | P1 |
| FUN-050 | Stage change logged | Every stage transition has audit record with before/after | P0 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 5.1 CRUD Workflow (10 tests)

| ID | Operation | Entities Involved | Expected Result | Priority |
|----|----------|------------------|-----------------|----------|
| INT-001 | Create opportunity → Submit for Go | Opportunity, WorkflowInstance, DoA2 | Full chain succeeds, DoA2 notified | P0 |
| INT-002 | Create → Submit → Approve → View | Opportunity, Workflow, Notification | GO stage visible, history complete | P0 |
| INT-003 | Create → Submit → Reject → View | Opportunity, Workflow | NO GO stage, rejection reason visible | P0 |
| INT-004 | Create → Cancel → Reopen → Edit | Opportunity, WorkflowHistory | Fields editable after reopen | P0 |
| INT-005 | Create → Submit → Recall → Modify → Resubmit | Full lifecycle | All state transitions recorded | P0 |
| INT-006 | View opportunity list after stage change | Opportunity, ListView | Stage/status updated in list view | P1 |
| INT-007 | Update mandatory field → Revalidate | Opportunity, Validation | Validation passes after fix | P1 |
| INT-008 | Delete funding partner → Validate → Block submit | Partner, Validation | Missing partner blocks submission | P1 |
| INT-009 | Add country → Recheck DoA2 | Country, EntityUserRole | DoA2 recalculated for new org unit | P1 |
| INT-010 | Change responsible org unit → DoA2 changes | OrgUnit, EntityUserRole | New DoA2 identified correctly | P0 |

### 5.2 Search & Filter (10 tests)

| ID | Search/Filter Criteria | Expected Results | Priority |
|----|----------------------|-----------------|----------|
| INT-011 | Filter opportunities by stage=GO | Only GO stage opps returned | P1 |
| INT-012 | Filter by stage=CANCELLED | Only cancelled opps returned | P1 |
| INT-013 | Filter by stage=NO GO | Only rejected opps returned | P1 |
| INT-014 | Filter by status=Active (in workflow) | Opps currently in workflow | P1 |
| INT-015 | Filter by status=Closed (decided) | Decided opps (approved/rejected/cancelled) | P1 |
| INT-016 | Search by OM name | Opportunities for that OM | P1 |
| INT-017 | Search by DoA2 name (decision maker) | Opps routed to that DoA2 | P2 |
| INT-018 | Filter by "In Workflow" indicator | Only submitted, pending decision | P1 |
| INT-019 | Sort by submission date | Chronological order | P2 |
| INT-020 | Combined filter: stage=GO AND status=Active | Submitted, awaiting decision | P1 |

### 5.3 Pagination (5 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| INT-021 | Page 1 of workflow history (10 entries per page) | First 10 entries shown | P2 |
| INT-022 | Last page of workflow history | Remaining entries shown | P2 |
| INT-023 | Change page size (10→50) | 50 entries per page | P2 |
| INT-024 | Opportunity list with 100+ GO stage opps | Paginated correctly | P2 |
| INT-025 | Empty result set (no opps in NO GO) | "No results" message | P2 |

### 5.4 Relationships (10 tests)

| ID | Relationship | Test Scenario | Expected Result | Priority |
|----|-------------|--------------|-----------------|----------|
| INT-026 | Opportunity → WorkflowInstance | Submit creates workflow | 1:1 mapping correct | P0 |
| INT-027 | Opportunity → WorkflowHistory (1:N) | Multiple actions | All entries linked to correct opp | P0 |
| INT-028 | Opportunity → DoA2 (via EntityUserRole) | DoA2 lookup | Correct user linked via org unit hierarchy | P0 |
| INT-029 | Opportunity → FundingPartner (N:M) | Partners preserved after workflow | Partners unchanged after stage change | P1 |
| INT-030 | Opportunity → OrgUnit → EntityUserRole | Hierarchy chain | Correct DoA2 resolved through chain | P1 |
| INT-031 | Opportunity → OpportunityStatement | Statement auto-regeneration | New version created on submit | P1 |
| INT-032 | OM User → Multiple Opportunities | Same OM, 3 opps | Each opp has independent workflow | P1 |
| INT-033 | DoA2 User → Multiple Pending Decisions | DoA2 has 5 pending | All visible, can act on each independently | P1 |
| INT-034 | Opportunity → Notifications (1:N) | Submit generates notifications | Multiple recipients each get notification | P1 |
| INT-035 | Opportunity → AuditTrail (1:N) | All actions audited | Complete trail for entire lifecycle | P0 |

### 5.5 Error Handling (15 tests)

| ID | Error Condition | Expected HTTP Response | Expected User Message | Priority |
|----|----------------|----------------------|----------------------|----------|
| INT-036 | Submit with missing required field | 400 Bad Request | Field-level validation errors | P0 |
| INT-037 | Submit for non-existent opportunity | 404 Not Found | "Opportunity not found" | P0 |
| INT-038 | Approve already-decided opportunity | 409 Conflict | "Decision already made" | P1 |
| INT-039 | Cancel already-cancelled opportunity | 409 Conflict | "Already cancelled" | P1 |
| INT-040 | Recall non-submitted opportunity | 400 Bad Request | "Not in workflow" | P1 |
| INT-041 | Submit with invalid org unit ID | 400 Bad Request | "Invalid organization unit" | P1 |
| INT-042 | Approve with invalid workflow ID | 404 Not Found | "Workflow not found" | P1 |
| INT-043 | Submit with concurrent modification | 409 Conflict | "Data modified by another user" | P1 |
| INT-044 | API call without auth token | 401 Unauthorized | "Authentication required" | P0 |
| INT-045 | API call with expired token | 401 Unauthorized | "Token expired" | P1 |
| INT-046 | API call exceeding rate limit | 429 Too Many Requests | "Rate limit exceeded" | P2 |
| INT-047 | Malformed JSON in request body | 400 Bad Request | "Invalid request format" | P1 |
| INT-048 | Submit with database connection failure | 500 (handled) | "Service temporarily unavailable" | P1 |
| INT-049 | Notification delivery failure | 202 (async) | Submission succeeds, notification queued | P1 |
| INT-050 | Workflow engine timeout | 504 Gateway Timeout | "Request timed out, please retry" | P2 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 6.1 Injection Prevention (10 tests)

| ID | Attack Vector | Target Field | Payload | Expected Block | Priority |
|----|--------------|-------------|---------|---------------|----------|
| SEC-001 | SQL Injection in cancel reason | Cancel reason | `'; DROP TABLE Opportunities;--` | Sanitized/Parameterized | P0 |
| SEC-002 | SQL Injection in rejection reason | Rejection reason | `' OR 1=1--` | Rejected/Escaped | P0 |
| SEC-003 | XSS in additional remarks | Additional remarks | `<script>alert('xss')</script>` | HTML escaped on display | P0 |
| SEC-004 | XSS in opportunity name | Name field | `<img onerror=alert(1) src=x>` | Sanitized | P0 |
| SEC-005 | XSS in workflow history display | Cancel reason | `<svg onload=alert(1)>` | Escaped in history view | P0 |
| SEC-006 | Command injection in description | Description | `$(rm -rf /)` or `; cat /etc/passwd` | Not executed, stored as text | P1 |
| SEC-007 | LDAP injection in user lookup | DoA2 search | `*)(objectClass=*)` | Rejected | P1 |
| SEC-008 | Path traversal in statement request | Statement filename | `../../etc/passwd` | Rejected | P1 |
| SEC-009 | JSON injection in API body | Request body | `{"__proto__":{"admin":true}}` | Prototype pollution blocked | P1 |
| SEC-010 | Header injection in notification | Email header | `\r\nBCC: attacker@evil.com` | Header injection blocked | P1 |

### 6.2 Broken Access Control (10 tests)

| ID | User Role | Unauthorized Action | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| SEC-011 | Unauthenticated user | GET /api/opportunity/{id}/workflow | 401 Unauthorized | P0 |
| SEC-012 | General User (no role) | POST /api/opportunity/{id}/submit | 403 Forbidden | P0 |
| SEC-013 | OM of opp A | POST /api/opportunity/{oppB}/submit | 403 Forbidden | P0 |
| SEC-014 | DoA2 of org unit X | POST /api/opportunity/{orgY}/approve | 403 Forbidden | P0 |
| SEC-015 | Collaborator | POST /api/opportunity/{id}/cancel | 403 Forbidden | P0 |
| SEC-016 | Partner User | POST /api/opportunity/{id}/recall | 403 Forbidden | P1 |
| SEC-017 | Expired session | POST /api/opportunity/{id}/submit | 401 + redirect to login | P1 |
| SEC-018 | Revoked permissions (mid-session) | Approve after role removed | 403 Forbidden | P1 |
| SEC-019 | Admin accessing workflow directly | Bypass normal UI flow via API | Allowed only if authorized | P1 |
| SEC-020 | Service account | POST /api/opportunity/{id}/approve | Depends on service account permissions | P2 |

### 6.3 IDOR (Insecure Direct Object Reference) (10 tests)

| ID | Object | Manipulation | Expected Result | Priority |
|----|--------|-------------|-----------------|----------|
| SEC-021 | Opportunity ID | Change ID in URL to another user's opp | 403 if not authorized | P0 |
| SEC-022 | Workflow ID | Enumerate workflow IDs | Only own workflows visible | P0 |
| SEC-023 | DoA2 user ID | Forge DoA2 user ID in approval | Server validates actual DoA2 | P0 |
| SEC-024 | Audit trail ID | Access another opp's audit trail | 403 Forbidden | P1 |
| SEC-025 | Notification ID | Access another user's notifications | 403 Forbidden | P1 |
| SEC-026 | Opportunity Statement ID | Download another opp's statement | 403 Forbidden | P1 |
| SEC-027 | Sequential ID enumeration | Try ID+1, ID+2, etc. | 403 for unauthorized IDs | P1 |
| SEC-028 | Negative ID | Use -1, 0 as opportunity ID | 400 Bad Request | P2 |
| SEC-029 | UUID instead of integer ID | Pass UUID where int expected | 400 Bad Request | P2 |
| SEC-030 | Very large ID | ID = 999999999999 | 404 Not Found | P2 |

### 6.4 Mass Assignment & Data Exposure (10 tests)

| ID | Protected Field/Data | Manipulation | Expected Result | Priority |
|----|---------------------|-------------|-----------------|----------|
| SEC-031 | Stage field in request body | Include stage=GO in create request | Stage field ignored (server-controlled) | P0 |
| SEC-032 | Status field in request body | Include status=Closed | Status field ignored | P0 |
| SEC-033 | WorkflowStatus in update | Set workflow status directly | Field ignored | P0 |
| SEC-034 | CreatedBy/ModifiedBy | Forge audit fields | Server overwrites with actual user | P1 |
| SEC-035 | IsDeleted flag | Set IsDeleted=false on deleted opp | Flag not modifiable via API | P1 |
| SEC-036 | DoA2 response exposes email | API response for DoA2 | Only name/title, not email/personal data | P1 |
| SEC-037 | Workflow history exposes internal IDs | View history | Uses display-safe identifiers | P2 |
| SEC-038 | Error response leaks stack trace | Trigger server error | Generic error message, no stack trace | P1 |
| SEC-039 | Password/token in logs | Submit action | No credentials in application logs | P0 |
| SEC-040 | API response includes soft-deleted data | List query | Deleted records excluded | P1 |

### 6.5 Authentication & Session (10 tests)

| ID | Attack Scenario | Expected Protection | Priority |
|----|----------------|-------------------|----------|
| SEC-041 | Replay captured submit request | Request rejected (anti-replay token) | P0 |
| SEC-042 | Session fixation on approval page | New session issued after auth | P1 |
| SEC-043 | CSRF on submit endpoint | CSRF token required and validated | P0 |
| SEC-044 | JWT token tampering | Modified token rejected | P0 |
| SEC-045 | Brute force workflow actions | Rate limiting applied | P1 |
| SEC-046 | Concurrent sessions, one logs out | Remaining session behavior defined | P2 |
| SEC-047 | Token refresh during long workflow | Token refreshed seamlessly | P1 |
| SEC-048 | Access workflow page after logout | Redirect to login | P1 |
| SEC-049 | Cookie security flags | HttpOnly, Secure, SameSite set | P1 |
| SEC-050 | API accessed via HTTP (not HTTPS) | Redirect to HTTPS or reject | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Concurrent Scenario | Expected Behavior | Priority |
|----|-------------------|-------------------|----------|
| CON-001 | Two OMs submit same opportunity simultaneously | First succeeds, second gets conflict error | P0 |
| CON-002 | OM submits while DoA2 is viewing | Submission succeeds, DoA2 sees updated state on refresh | P1 |
| CON-003 | OM recalls while DoA2 approves simultaneously | One wins, other gets conflict error | P0 |
| CON-004 | Two DoA2 holders approve same opportunity | First approval wins, second gets "already decided" | P0 |
| CON-005 | OM cancels while another user is editing | Cancel wins, editor gets notification | P1 |
| CON-006 | Rapid double-click on Submit button | Only one submission created (idempotent) | P0 |
| CON-007 | Rapid double-click on Approve button | Only one approval recorded | P0 |
| CON-008 | Rapid double-click on Cancel button | Only one cancel action | P1 |
| CON-009 | Edit opportunity while it's being submitted via API | Optimistic concurrency check | P1 |
| CON-010 | Notification sent while email service restarts | Notification queued and delivered after recovery | P2 |
| CON-011 | DB transaction isolation — submit reads own write | Submit → read status → see GO | P1 |
| CON-012 | Parallel DoA2 lookup for same org unit | Both get same result, no race | P1 |
| CON-013 | Concurrent reopen from two browser tabs | First succeeds, second gets conflict | P1 |
| CON-014 | Bulk approval of multiple opportunities by DoA2 | All approved independently, no interference | P1 |
| CON-015 | Workflow history written concurrently | All entries recorded, none lost | P1 |
| CON-016 | Audit trail concurrent writes | All audit entries preserved | P1 |
| CON-017 | Cache invalidation after stage change | Other users see updated stage | P1 |
| CON-018 | Session timeout during approval dialog | Approval fails gracefully, no partial state | P2 |
| CON-019 | Parallel statement regeneration | Only one regeneration occurs | P2 |
| CON-020 | Concurrent org unit hierarchy update + DoA2 lookup | Lookup uses consistent hierarchy state | P2 |
| CON-021 | Load balancer routes approval to different server | Transaction committed correctly across instances | P1 |
| CON-022 | Concurrent recall + approval race | Only one succeeds, clear winner | P0 |
| CON-023 | Parallel notification delivery | All recipients notified, no duplicates | P2 |
| CON-024 | Simultaneous stage change + list view query | List shows consistent state | P2 |
| CON-025 | Concurrent OM role transfer + submit | Transfer completes first, then submit by new OM | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (5)

| ID | Test Name | Input | Expected Output | Priority |
|----|-----------|-------|----------------|----------|
| UNT-001 | ValidateMandatoryFields — all present | Complete field set | Validation passes | P1 |
| UNT-002 | ValidateMandatoryFields — missing name | Name=null | Returns "Name required" error | P1 |
| UNT-003 | ValidateDoA2Exists — found | OrgUnit with DoA2 | Returns DoA2 user(s) | P1 |
| UNT-004 | ValidateDoA2Exists — not found | OrgUnit without DoA2 | Returns validation error | P1 |
| UNT-005 | ValidateDateRange — valid | Start < End | Passes | P1 |

### Formatting (3)

| ID | Test Name | Input | Expected Output | Priority |
|----|-----------|-------|----------------|----------|
| UNT-006 | FormatWorkflowHistoryEntry | Action + User + Time | "Submit for Go by [User] at [Time]" | P1 |
| UNT-007 | FormatNotificationEmail | Template + Data | Correct email body with placeholders replaced | P1 |
| UNT-008 | FormatStageDisplay | Stage enum | Human-readable label ("Identify & Profile") | P2 |

### Calculations (5)

| ID | Test Name | Input | Expected Output | Priority |
|----|-----------|-------|----------------|----------|
| UNT-009 | CalculateTotalBudget — single partner | 1 partner, $100K | Total = $100,000 | P1 |
| UNT-010 | CalculateTotalBudget — multiple partners | 3 partners, various | Sum of all amounts | P1 |
| UNT-011 | CalculateTotalBudget — zero | No funding | Total = $0 | P1 |
| UNT-012 | CalculateImplementationDuration | Start + End dates | Correct number of months | P1 |
| UNT-013 | CalculateWorkflowDuration | Submit time + Decision time | Correct elapsed time | P2 |

### Status Logic (5)

| ID | Test Name | Input | Expected Output | Priority |
|----|-----------|-------|----------------|----------|
| UNT-014 | GetNextValidStages — I&P/Draft | Current stage | [GO, CANCELLED] | P0 |
| UNT-015 | GetNextValidStages — GO/Active | Current stage | [GO/Closed, NO GO] (via DoA2) | P0 |
| UNT-016 | GetNextValidStages — CANCELLED | Current stage | [I&P (Reopen)] | P1 |
| UNT-017 | GetNextValidStages — NO GO | Current stage | [I&P (Reopen)] | P1 |
| UNT-018 | IsStageTransitionValid — invalid | I&P → NO GO (direct) | false | P1 |

### Collections (3)

| ID | Test Name | Input | Expected Output | Priority |
|----|-----------|-------|----------------|----------|
| UNT-019 | GetPendingDecisions — multiple opps | DoA2 with 5 pending | Returns 5 items | P1 |
| UNT-020 | FilterWorkflowHistory — by action | History + filter="Approve" | Only approve entries | P2 |
| UNT-021 | GroupNotificationsByRecipient | 10 notifications, 3 users | 3 groups, correctly assigned | P2 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

### Single Operations (2)

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Submit for Go Decision (API response) | < 3 seconds | P1 |
| PRF-002 | DoA2 approval (API response) | < 2 seconds | P1 |

### Bulk Operations (3)

| ID | Operation | Volume | Threshold | Priority |
|----|----------|--------|-----------|----------|
| PRF-003 | DoA2 lookup across org hierarchy | 5-level deep hierarchy | < 2 seconds | P1 |
| PRF-004 | Batch notification delivery | 50 notifications | < 30 seconds | P2 |
| PRF-005 | Opportunity Statement regeneration | Large opp (all fields populated) | < 10 seconds | P1 |

### Search (5)

| ID | Operation | Dataset | Threshold | Priority |
|----|----------|---------|-----------|----------|
| PRF-006 | Filter opportunities by stage | 10,000 opportunities | < 2 seconds | P1 |
| PRF-007 | Search by OM name | 10,000 opportunities | < 2 seconds | P1 |
| PRF-008 | Workflow history load | 500 history entries | < 1 second | P1 |
| PRF-009 | DoA2 pending decision list | 100 pending decisions | < 2 seconds | P1 |
| PRF-010 | Combined stage + status filter | 10,000 opps, 3 filters | < 3 seconds | P2 |

### Concurrent Access (3)

| ID | Operation | Concurrency | Threshold | Priority |
|----|----------|------------|-----------|----------|
| PRF-011 | 50 simultaneous opportunity submissions | 50 users | All complete < 10 seconds | P2 |
| PRF-012 | 20 simultaneous DoA2 approvals | 20 users | All complete < 5 seconds | P2 |
| PRF-013 | 100 concurrent list views during submissions | 100 users | Avg response < 3 seconds | P2 |

### Memory (3)

| ID | Operation | Observation | Threshold | Priority |
|----|----------|------------|-----------|----------|
| PRF-014 | Submit for Go (memory profile) | Memory delta during submit | < 50MB increase | P2 |
| PRF-015 | Workflow history page load | Memory for 500 entries | < 20MB | P2 |
| PRF-016 | Notification processing | Memory during batch notify | No memory leak after processing | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

### Sustained Load (3)

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 100 users performing workflow actions/hour | 1 hour | Error rate < 1%, avg response < 3s | P2 |
| LDT-002 | 50 concurrent DoA2 decision makers | 30 min | All decisions processed correctly | P2 |
| LDT-003 | 200 users viewing opportunity lists + filtering | 1 hour | P99 response < 5s | P2 |

### Spike Load (2)

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-004 | 500 simultaneous submissions in 1 minute | 1 min | All processed, no data loss | P2 |
| LDT-005 | 1000 concurrent page views during mass approval | 5 min | System remains responsive | P2 |

### Stress Limits (3)

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-006 | Increase load until error rate > 5% | Until failure | Identify breaking point, graceful degradation | P2 |
| LDT-007 | Fill notification queue to capacity | Until full | Queue handles overflow (backpressure) | P2 |
| LDT-008 | 10,000 concurrent read requests | 5 min | System serves or rate-limits gracefully | P2 |

### Recovery (2)

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-009 | Remove load after spike | 5 min recovery | System returns to normal within 2 min | P2 |
| LDT-010 | Restart service during active workflows | Recovery period | No data loss, workflows resume | P2 |

---

## Traceability Matrix

| AC Section | AC Requirement | Test Cases |
|------------|---------------|------------|
| **1. Roles** | OM is primary caretaker, field never blank | POS-016, FUN-032 |
| | Collaborator can edit content | NEG-001 to NEG-010 (access denied — not implemented) |
| | OM can transfer role | POS-029 (**BLOCKED: PNO-1193**), FUN-048 |
| | OM or Collaborator can initiate submission | POS-001, NEG-001, FUN-029 |
| | Standardized position titles | POS-017 |
| | Decision Maker = DoA2 | POS-007, POS-008, INT-028, INT-030 |
| | Original decision makers visible | POS-018 |
| | New DoA holder can approve | POS-030, INT-010 |
| **2. Visibility** | Read-only after submission | FUN-005, FUN-006 |
| | In workflow clearly visible | FUN-013, FUN-014, POS-031 |
| | Workflow history visible | POS-020, FUN-040, FUN-050 |
| | Inactive OM handling | FUN-033 (**BLOCKED**) |
| **3. Submission** | Mandatory acknowledgement | POS-009, FUN-020 |
| | Optional additional remarks | POS-010 |
| | Auto-regenerate Statement | POS-011, INT-031 |
| **4. Validations** | All mandatory fields | NEG-021 to NEG-036, FUN-016 to FUN-030 |
| | DoA2 server-side validation | NEG-037 to NEG-041, FUN-018 |
| | Country-Org Unit mismatch warning | FUN-028 |
| **5. Workflow** | Stage transitions (all) | POS-001 to POS-006, Matrix rows 1-10 |
| | Access denial (Collaborator) | NEG-001 to NEG-010 |
| | Cancel only from I&P not in workflow | FUN-002, NEG-011 |
| | DoA pathway display | POS-032 |
| | On-screen confirmation | POS-013 |
| **6. Post-Submission** | Notification to DoA2 and OIC | INT-034, POS-025 |
| | OM recall capability | POS-014, POS-015, FUN-003, FUN-004 |
| | Email content verification | SEC-010, INT-049 |
| | Internal stakeholder notification on GO | POS-005 (stakeholder notify) |

---

## Test Environment Setup

**Prerequisites for QA/TEST environment:**
- DoA2 for B5503 (India) = Dominic (configured by Tafazzul)
- DoA2 for B5505 (Sri Lanka) = Perminder (configured by Tafazzul)
- OM user with access to create/edit opportunities
- Non-OM user for access denial tests (Collaborator)
- General User account (no opportunity permissions)

**URLs:**
- QA: `https://opportunityplus.qa.unops.org`
- TEST: `https://opportunityplus.test.unops.org`

**Playwright E2E Config:**
- Feature flag: `GO_DECISION_IMPLEMENTED=true|false`
- Test opportunity IDs: `GO_TEST_OPP_IP_ID`, `GO_TEST_OPP_COMPLETE_ID`
- Test file: `QA Tests/Playwright Tests/go-decision.spec.ts`

---

## Priority Distribution

| Priority | Count | Description |
|----------|-------|-------------|
| P0 | 89 | Critical — must pass for feature acceptance |
| P1 | 196 | High — required for full feature coverage |
| P2 | 112 | Medium — edge cases, polish, non-functional |
| **Total** | **397** | |

---

**Last Updated:** 2026-02-11  
**Supersedes:** Previous version (55 tests, non-standard categories)  
**Status:** Ready for Execution  
**Compliance:** ✅ 10-Category Standard, ✅ 3:1 Ratio (140 ≥ 105)
