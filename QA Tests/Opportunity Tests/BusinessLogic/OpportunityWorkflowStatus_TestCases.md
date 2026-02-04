# Opportunity Workflow Status Test Cases

## Overview
Test cases for Opportunity Workflow Status Management including status transitions, validation rules, security, and concurrent access.

**JIRA Story:** PNO-940  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 45

---

## Test Case Summary

| Category | Count | Priority |
|----------|-------|----------|
| Positive Status Transition Tests | 12 | High |
| Negative Status Validation Tests | 10 | High |
| Security/Authorization Tests | 8 | Critical |
| Concurrency/Race Condition Tests | 6 | High |
| Boundary/Edge Case Tests | 5 | Normal |
| Audit Trail Tests | 4 | Normal |
| **TOTAL** | **45** | |

---

## 1. Positive Status Transition Tests

### POS_001 - Verify Draft to Active Transition
**Priority:** High  
**JIRA ID:** PNO-1050  
**Labels:** Opportunity, Status, Workflow

**Objective:** Verify that an Opportunity can successfully transition from Draft to Active when all mandatory requirements are met.

**Preconditions:**
- User has Editor permissions
- Opportunity is in Draft status
- All mandatory fields are completed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Draft opportunity | Opportunity loads |
| 2 | Verify all mandatory fields | Fields are populated |
| 3 | Click "Activate" or transition button | Confirmation dialog appears |
| 4 | Confirm transition | Status changes to Active |
| 5 | Verify status indicator | Shows "Active" |

**Expected Results:**
- Status changes from "Draft" to "Active"
- Transition timestamp recorded
- Audit trail entry created
- Notification sent to relevant stakeholders

---

### POS_002 - Verify Active to Pending Decision Transition
**Priority:** High  
**JIRA ID:** PNO-1051  
**Labels:** Opportunity, Status, Workflow

**Objective:** Verify that an Opportunity can transition from Active to Pending Decision for Go/No-Go workflow.

**Preconditions:**
- Opportunity is in Active status
- User has permission to submit for decision

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Active opportunity | Opportunity loads |
| 2 | Click "Send for Decision" | Validation runs |
| 3 | Complete submission form | All required info entered |
| 4 | Submit for decision | Status changes |

**Expected Results:**
- Status = "Pending Decision"
- Decision maker notified
- Deadline set for decision
- Record locked for editing

---

### POS_003 - Verify Decision Approval Transition (Go)
**Priority:** High  
**JIRA ID:** PNO-1052  
**Labels:** Opportunity, Status, Workflow, Decision

**Objective:** Verify that approved opportunities transition correctly to Active/Approved status.

**Preconditions:**
- Opportunity is in Pending Decision status
- Decision maker has reviewed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Decision Maker | Login successful |
| 2 | Navigate to pending opportunity | Opportunity loads |
| 3 | Review decision package | Package displays |
| 4 | Select "Approve" action | Confirmation appears |
| 5 | Confirm approval | Status updates |

**Expected Results:**
- Status = "Active - Approved" or "GO"
- Team receives approval notification
- Edit mode re-enabled for development
- Approval date/user recorded

---

### POS_004 - Verify Decision Rejection Transition (No-Go)
**Priority:** High  
**JIRA ID:** PNO-1053  
**Labels:** Opportunity, Status, Workflow, Decision

**Objective:** Verify that rejected opportunities transition to No-Go status with proper notification.

**Preconditions:**
- Opportunity is in Pending Decision status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Decision Maker | Login successful |
| 2 | Navigate to pending opportunity | Opportunity loads |
| 3 | Select "Reject" action | Rejection form appears |
| 4 | Enter rejection reason (mandatory) | Reason entered |
| 5 | Confirm rejection | Status updates |

**Expected Results:**
- Status = "NO GO"
- Rejection reason recorded
- Team notified of rejection
- Opportunity moved to No-Go list

---

### POS_005 - Verify Opportunity Cancellation
**Priority:** High  
**JIRA ID:** PNO-1054  
**Labels:** Opportunity, Status, Workflow, Cancellation

**Objective:** Verify that an Opportunity can be cancelled by authorized users.

**Preconditions:**
- Opportunity is in Draft or Active status
- User has cancel permission

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to opportunity | Opportunity loads |
| 2 | Click "Cancel Opportunity" | Confirmation dialog appears |
| 3 | Enter cancellation reason | Reason entered |
| 4 | Confirm cancellation | Status changes |

**Expected Results:**
- Status = "Cancelled"
- Cancellation reason recorded
- All stakeholders notified
- Record becomes read-only

---

### POS_006 - Verify Reopen from No-Go Status
**Priority:** Normal  
**JIRA ID:** PNO-1055  
**Labels:** Opportunity, Status, Workflow

**Objective:** Verify that a No-Go opportunity can be reopened by authorized users.

**Preconditions:**
- Opportunity is in No-Go status
- User has reopen permission

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to No-Go opportunity | Opportunity loads |
| 2 | Click "Reopen Opportunity" | Reopen form appears |
| 3 | Enter justification | Justification entered |
| 4 | Confirm reopen | Status changes |

**Expected Results:**
- Status = "Draft" or "Active"
- Reopen justification recorded
- Stakeholders notified
- Edit mode re-enabled

---

### POS_007 - Verify Reopen from Cancelled Status
**Priority:** Normal  
**JIRA ID:** PNO-1056  
**Labels:** Opportunity, Status, Workflow

**Objective:** Verify that a Cancelled opportunity can be reopened.

**Preconditions:**
- Opportunity is in Cancelled status
- User has reopen permission

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Cancelled opportunity | Opportunity loads |
| 2 | Click "Reopen" button | Reopen form appears |
| 3 | Enter reopen reason | Reason entered |
| 4 | Confirm | Status changes to Draft |

---

### POS_008 - Verify Status Change Timestamp Recording
**Priority:** Normal  
**JIRA ID:** PNO-1057  
**Labels:** Opportunity, Status, Audit

**Objective:** Verify that all status changes record accurate timestamps.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform any status transition | Transition completes |
| 2 | View status history | History displays |
| 3 | Verify timestamp | UTC timestamp recorded |
| 4 | Verify timezone display | Local time shown to user |

---

### POS_009 - Verify Status Filter in Opportunity List
**Priority:** Normal  
**JIRA ID:** PNO-1058  
**Labels:** Opportunity, Status, UI

**Objective:** Verify that the opportunity list can be filtered by status.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity list | List displays |
| 2 | Apply status filter (e.g., Draft) | Filter applied |
| 3 | Verify results | Only Draft opportunities shown |
| 4 | Clear filter | All opportunities shown |

---

### POS_010 - Verify Status Badge Display
**Priority:** Low  
**JIRA ID:** PNO-1059  
**Labels:** Opportunity, Status, UI

**Objective:** Verify that status badges display correctly with proper colors.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View opportunities of different statuses | List displays |
| 2 | Verify Draft badge | Gray/neutral color |
| 3 | Verify Active badge | Green/success color |
| 4 | Verify No-Go badge | Red/danger color |
| 5 | Verify Cancelled badge | Gray/muted color |

---

### POS_011 - Verify OM Recall During Pending Decision
**Priority:** High  
**JIRA ID:** PNO-1060  
**Labels:** Opportunity, Status, Workflow

**Objective:** Verify Opportunity Manager can recall submission during Pending Decision.

**Preconditions:**
- Opportunity is in Pending Decision status
- User is the Opportunity Manager

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Opportunity Manager | Login successful |
| 2 | Navigate to pending opportunity | Opportunity loads |
| 3 | Click "Recall" button | Confirmation appears |
| 4 | Confirm recall | Status reverts to Active |
| 5 | Verify decision maker notified | Notification sent |

---

### POS_012 - Verify Conditional Approval Status
**Priority:** Normal  
**JIRA ID:** PNO-1061  
**Labels:** Opportunity, Status, Workflow, Decision

**Objective:** Verify that conditional approvals set appropriate status.

**Preconditions:**
- Opportunity is in Pending Decision status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Decision maker selects conditional approval | Form appears |
| 2 | Enter conditions | Conditions specified |
| 3 | Submit conditional approval | Status updates |
| 4 | Verify status | Shows "Approved with Conditions" |

---

## 2. Negative Status Validation Tests

### NEG_001 - Verify Cannot Activate with Missing Mandatory Fields
**Priority:** High  
**JIRA ID:** PNO-1070  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify that activation is blocked when mandatory fields are empty.

**Preconditions:**
- Opportunity in Draft status
- At least one mandatory field empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Draft opportunity | Opportunity loads |
| 2 | Leave mandatory field empty | Field is blank |
| 3 | Attempt to activate | Validation error displayed |
| 4 | Verify error message | Lists missing fields |
| 5 | Verify status unchanged | Still Draft |

---

### NEG_002 - Verify Cannot Submit for Decision Without DoA Holder
**Priority:** High  
**JIRA ID:** PNO-1071  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify submission blocked when no DoA2 holder exists for org unit.

**Preconditions:**
- Org unit has no assigned DoA2 holder

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to opportunity | Opportunity loads |
| 2 | Attempt to submit for decision | Validation runs |
| 3 | Verify blocking error | "No DoA Level 2 holder found" |
| 4 | Verify submit disabled | Cannot proceed |

---

### NEG_003 - Verify Cannot Transition Invalid Status Sequence
**Priority:** High  
**JIRA ID:** PNO-1072  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify that invalid status transitions are blocked.

**Preconditions:**
- Opportunity is in specific status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Draft opportunity exists | Status = Draft |
| 2 | Attempt direct transition to "No-Go" | Action not available |
| 3 | Verify workflow enforced | Must follow valid sequence |

**Invalid Transitions:**
- Draft → No-Go (must go through Pending Decision)
- Active → Cancelled → Active (need Reopen first)
- No-Go → Active (must Reopen first)

---

### NEG_004 - Verify Cannot Edit During Pending Decision
**Priority:** High  
**JIRA ID:** PNO-1073  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify record is locked during Pending Decision status.

**Preconditions:**
- Opportunity is in Pending Decision status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to pending opportunity | Opportunity loads |
| 2 | Attempt to edit any field | Edit is disabled |
| 3 | Verify lock message | "Record locked pending decision" |

---

### NEG_005 - Verify Cannot Cancel Already Cancelled Opportunity
**Priority:** Normal  
**JIRA ID:** PNO-1074  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify cancel button not available for already cancelled records.

**Preconditions:**
- Opportunity is in Cancelled status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to cancelled opportunity | Opportunity loads |
| 2 | Look for Cancel button | Button not visible |
| 3 | Verify available actions | Only "Reopen" available |

---

### NEG_006 - Verify Rejection Requires Reason
**Priority:** High  
**JIRA ID:** PNO-1075  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify that rejection without reason is blocked.

**Preconditions:**
- Decision maker reviewing opportunity

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to pending opportunity as decision maker | Opportunity loads |
| 2 | Click "Reject" button | Rejection form appears |
| 3 | Leave reason field empty | Field is blank |
| 4 | Attempt to submit rejection | Validation error |
| 5 | Verify error message | "Rejection reason is required" |

---

### NEG_007 - Verify Cannot Submit for Decision with Pending Due Diligence
**Priority:** Normal  
**JIRA ID:** PNO-1076  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify appropriate warning/blocking when due diligence is incomplete.

**Preconditions:**
- Partner due diligence is pending/incomplete

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to opportunity with pending DD | Opportunity loads |
| 2 | Attempt to submit for decision | Warning displayed |
| 3 | Verify warning content | Lists pending due diligence |
| 4 | Option to proceed with condition | Can proceed if acknowledged |

---

### NEG_008 - Verify Cannot Approve/Reject Without Decision Authority
**Priority:** High  
**JIRA ID:** PNO-1077  
**Labels:** Opportunity, Status, Permission, Negative

**Objective:** Verify only DoA holders can approve/reject.

**Preconditions:**
- Opportunity is in Pending Decision status
- User is NOT a DoA holder

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as non-DoA user | Login successful |
| 2 | Navigate to pending opportunity | Opportunity loads |
| 3 | Look for Approve/Reject buttons | Buttons not visible |
| 4 | Verify read-only access | Can only view |

---

### NEG_009 - Verify Cannot Transition with Invalid Value Range
**Priority:** Normal  
**JIRA ID:** PNO-1078  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify validation on value-dependent transitions.

**Preconditions:**
- Opportunity has value exceeding DoA2 authority

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set opportunity value above DoA2 threshold | Value set |
| 2 | Submit for decision | System identifies DoA3 needed |
| 3 | Verify routing | Routes to DoA3 not DoA2 |

---

### NEG_010 - Verify Empty Cancellation Reason Blocked
**Priority:** Normal  
**JIRA ID:** PNO-1079  
**Labels:** Opportunity, Status, Validation, Negative

**Objective:** Verify cancellation requires reason.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to opportunity | Opportunity loads |
| 2 | Click Cancel | Cancellation form appears |
| 3 | Leave reason empty | Field blank |
| 4 | Attempt to submit | Validation error |
| 5 | Verify message | "Cancellation reason is required" |

---

## 3. Security/Authorization Tests

### SEC_001 - Verify URL Manipulation Cannot Bypass Status
**Priority:** Critical  
**JIRA ID:** PNO-1080  
**Labels:** Opportunity, Status, Security

**Objective:** Verify that direct URL manipulation cannot change opportunity status.

**Preconditions:**
- Opportunity exists in Draft status
- User knows the API endpoint

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Capture the status change API endpoint | Endpoint identified |
| 2 | Craft direct POST request to change status to Active | Request prepared |
| 3 | Include valid auth token but skip validations | Request sent |
| 4 | Verify response | 400 Bad Request or 403 Forbidden |
| 5 | Verify status unchanged in database | Status still Draft |

---

### SEC_002 - Verify Cross-User Status Change Prevention
**Priority:** Critical  
**JIRA ID:** PNO-1081  
**Labels:** Opportunity, Status, Security

**Objective:** Verify users cannot change status of opportunities they don't have access to.

**Preconditions:**
- User A owns opportunity
- User B has no access

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User B | Login successful |
| 2 | Attempt to access User A's opportunity | Access denied |
| 3 | Attempt API call to change status | 403 Forbidden |

---

### SEC_003 - Verify Session Token Required for Status Change
**Priority:** Critical  
**JIRA ID:** PNO-1082  
**Labels:** Opportunity, Status, Security

**Objective:** Verify status change APIs require valid authentication.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call status change API without token | Request sent |
| 2 | Verify response | 401 Unauthorized |
| 3 | Call with invalid token | Request sent |
| 4 | Verify response | 401 Unauthorized |

---

### SEC_004 - Verify Role-Based Status Actions
**Priority:** High  
**JIRA ID:** PNO-1083  
**Labels:** Opportunity, Status, Security, Permission

**Objective:** Verify status actions match user roles.

**Test Data:**
| Role | Can Activate | Can Submit | Can Approve | Can Cancel |
|------|--------------|------------|-------------|------------|
| Viewer | No | No | No | No |
| Editor | Yes | Yes | No | No |
| OM | Yes | Yes | No | Yes |
| DoA2 | No | No | Yes | No |
| Admin | Yes | Yes | Yes | Yes |

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as each role | Login successful |
| 2 | Navigate to opportunity | Opportunity loads |
| 3 | Verify available actions | Match role permissions |
| 4 | Attempt unauthorized action | Action blocked |

---

### SEC_005 - Verify Expired Session Cannot Change Status
**Priority:** High  
**JIRA ID:** PNO-1084  
**Labels:** Opportunity, Status, Security

**Objective:** Verify expired sessions are properly rejected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in and initiate status change | Form appears |
| 2 | Wait for session to expire | Session expires |
| 3 | Attempt to complete status change | Redirect to login |
| 4 | Verify status unchanged | Status not changed |

---

### SEC_006 - Verify Inactive User Cannot Change Status
**Priority:** High  
**JIRA ID:** PNO-1085  
**Labels:** Opportunity, Status, Security

**Objective:** Verify deactivated users lose status change capability immediately.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | User logged in with valid session | Session active |
| 2 | Admin deactivates user | User marked inactive |
| 3 | User attempts status change | Action blocked |
| 4 | Verify appropriate error | "Account inactive" |

---

### SEC_007 - Verify API Rate Limiting on Status Changes
**Priority:** Normal  
**JIRA ID:** PNO-1086  
**Labels:** Opportunity, Status, Security

**Objective:** Verify rate limiting prevents rapid status change attempts.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Send 100 rapid status change requests | Requests sent |
| 2 | Verify rate limiting triggers | 429 Too Many Requests |
| 3 | Verify legitimate request after cooldown | Succeeds |

---

### SEC_008 - Verify Audit Log for All Status Changes
**Priority:** High  
**JIRA ID:** PNO-1087  
**Labels:** Opportunity, Status, Security, Audit

**Objective:** Verify complete audit trail for status changes.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform status change | Change completes |
| 2 | View audit log | Log entry exists |
| 3 | Verify log contains: | User, timestamp, old status, new status, IP address |
| 4 | Verify log immutability | Cannot edit log entry |

---

## 4. Concurrency/Race Condition Tests

### CONC_001 - Verify Duplicate Submit Prevention
**Priority:** High  
**JIRA ID:** PNO-1090  
**Labels:** Opportunity, Status, Concurrency

**Objective:** Verify rapid duplicate submissions are prevented.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open opportunity for activation | Ready to submit |
| 2 | Double-click Activate button rapidly | Two requests sent |
| 3 | Verify only one status change | Only one transition recorded |
| 4 | Verify UI feedback | Second click shows "Already processing" |

---

### CONC_002 - Verify Concurrent User Status Conflict
**Priority:** High  
**JIRA ID:** PNO-1091  
**Labels:** Opportunity, Status, Concurrency

**Objective:** Verify two users cannot simultaneously change status.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | User A opens opportunity edit | Page loaded |
| 2 | User B opens same opportunity | Page loaded |
| 3 | User A activates opportunity | Succeeds, status = Active |
| 4 | User B attempts to cancel (still sees Draft) | Error: "Status has changed" |
| 5 | User B page refreshes | Shows updated Active status |

---

### CONC_003 - Verify Optimistic Locking on Status
**Priority:** High  
**JIRA ID:** PNO-1092  
**Labels:** Opportunity, Status, Concurrency

**Objective:** Verify optimistic concurrency control works.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Load opportunity with version 1 | Page loaded |
| 2 | Another process updates to version 2 | Update occurs |
| 3 | Attempt status change with version 1 | Conflict detected |
| 4 | Verify error message | "Record has been modified" |

---

### CONC_004 - Verify Decision Locking During Approval
**Priority:** High  
**JIRA ID:** PNO-1093  
**Labels:** Opportunity, Status, Concurrency

**Objective:** Verify only one approver can approve at a time.

**Preconditions:**
- Multiple DoA2 holders exist
- Opportunity is pending

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | DoA2 User A starts approval | Form open |
| 2 | DoA2 User B attempts approval | Warning or blocked |
| 3 | User A completes approval | Status = GO |
| 4 | User B page refreshes | Shows already approved |

---

### CONC_005 - Verify Recall During Simultaneous Approval
**Priority:** Normal  
**JIRA ID:** PNO-1094  
**Labels:** Opportunity, Status, Concurrency

**Objective:** Verify recall conflicts with simultaneous approval.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | OM initiates recall | Recall started |
| 2 | Simultaneously, DoA approves | Approval submitted |
| 3 | Verify conflict resolution | First to complete wins |
| 4 | Loser gets conflict error | "Status has changed" |

---

### CONC_006 - Verify Status Transition Queue Under Load
**Priority:** Normal  
**JIRA ID:** PNO-1095  
**Labels:** Opportunity, Status, Concurrency, Load

**Objective:** Verify status changes process correctly under heavy load.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create 100 concurrent status change requests | Requests submitted |
| 2 | Monitor processing | All processed |
| 3 | Verify each opportunity | Correct final status |
| 4 | Verify no data corruption | All records valid |

---

## 5. Boundary/Edge Case Tests

### EDGE_001 - Verify Status Change at Midnight (Day Boundary)
**Priority:** Normal  
**JIRA ID:** PNO-1100  
**Labels:** Opportunity, Status, Boundary

**Objective:** Verify status changes work correctly at day boundaries.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Initiate status change at 23:59:58 | Change initiated |
| 2 | Complete at 00:00:01 | Change completes |
| 3 | Verify timestamp | Correct date recorded |
| 4 | Verify deadline calculations | Based on correct date |

---

### EDGE_002 - Verify Status Change Spanning Timezone Change
**Priority:** Low  
**JIRA ID:** PNO-1101  
**Labels:** Opportunity, Status, Boundary

**Objective:** Verify DST changes don't affect status transitions.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Initiate status change before DST change | Started |
| 2 | Complete after DST change | Completed |
| 3 | Verify timestamps | UTC preserved correctly |

---

### EDGE_003 - Verify Very Long Status History
**Priority:** Low  
**JIRA ID:** PNO-1102  
**Labels:** Opportunity, Status, Boundary

**Objective:** Verify system handles opportunities with many status changes.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create opportunity with 50+ status changes | History created |
| 2 | View status history | Loads without timeout |
| 3 | Verify pagination | History is paginated |
| 4 | Verify performance | Loads within 3 seconds |

---

### EDGE_004 - Verify Status Change with Special Characters in Reason
**Priority:** Normal  
**JIRA ID:** PNO-1103  
**Labels:** Opportunity, Status, Boundary

**Objective:** Verify special characters in cancellation/rejection reasons.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter reason with: <script>, "quotes", émojis 🎉 | Special chars entered |
| 2 | Submit cancellation | Succeeds |
| 3 | View saved reason | Characters preserved and safe |
| 4 | Verify no XSS execution | Scripts not executed |

---

### EDGE_005 - Verify Status with Maximum Value Opportunity
**Priority:** Normal  
**JIRA ID:** PNO-1104  
**Labels:** Opportunity, Status, Boundary

**Objective:** Verify status transitions work for maximum value opportunities.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create opportunity with max value ($999,999,999.99) | Created |
| 2 | Submit for decision | Validates correctly |
| 3 | Verify DoA routing | Routes to highest authority |
| 4 | Complete approval workflow | All steps complete |

---

## 6. Audit Trail Tests

### AUDIT_001 - Verify Complete Status Change History
**Priority:** Normal  
**JIRA ID:** PNO-1110  
**Labels:** Opportunity, Status, Audit

**Objective:** Verify all status changes are recorded.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform multiple status changes | Changes complete |
| 2 | View audit history | All changes listed |
| 3 | Verify each entry | Date, user, old→new status |

---

### AUDIT_002 - Verify Audit Export
**Priority:** Low  
**JIRA ID:** PNO-1111  
**Labels:** Opportunity, Status, Audit

**Objective:** Verify audit history can be exported.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View opportunity audit history | History displays |
| 2 | Click Export | Download initiates |
| 3 | Verify export format | PDF/Excel with all entries |

---

### AUDIT_003 - Verify Audit Immutability
**Priority:** High  
**JIRA ID:** PNO-1112  
**Labels:** Opportunity, Status, Audit, Security

**Objective:** Verify audit entries cannot be modified.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create audit entry via status change | Entry created |
| 2 | Attempt to modify entry via API | Rejected |
| 3 | Attempt to delete entry | Rejected |
| 4 | Verify original entry unchanged | Entry intact |

---

### AUDIT_004 - Verify Audit Includes IP Address
**Priority:** Normal  
**JIRA ID:** PNO-1113  
**Labels:** Opportunity, Status, Audit

**Objective:** Verify IP address captured in audit.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Perform status change | Change recorded |
| 2 | View audit entry details | Details display |
| 3 | Verify IP address field | User's IP recorded |

---

## Summary

| Category | Count |
|----------|-------|
| Positive Status Transitions | 12 |
| Negative Validations | 10 |
| Security/Authorization | 8 |
| Concurrency | 6 |
| Boundary/Edge Cases | 5 |
| Audit Trail | 4 |
| **TOTAL** | **45** |

---

**C# Test Class:** `OpportunityWorkflowStatusTests.cs`  
**Playwright Test File:** `opportunity-workflow-status.spec.ts`  
**Status:** ✅ Aligned with JIRA PNO-940
