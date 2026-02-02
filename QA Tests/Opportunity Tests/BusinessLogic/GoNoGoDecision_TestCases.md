# GoNoGoDecision Test Cases

**Component:** Go/No-Go Decision Workflow Logic  
**Test Count:** 25+  
**Priority:** P0-P1 (Critical/High)  
**Created:** January 13, 2026  
**Last Updated:** February 2, 2026

---

## Overview

Test cases for Go/No-Go decision workflow orchestration, completeness validation, stakeholder notification, and conditional approvals.

> **NOTE:** This file contains generic workflow test cases. For PRD-specific test cases covering the "Send Opportunity for Go Decision" feature, see:
> 
> **[GoNoGoDecision_PRD_TestCases.md](./GoNoGoDecision_PRD_TestCases.md)** - **102 test cases** aligned with the PRD and Additional Acceptance Criteria covering:
> - DoA Level 2 Approver Lookup (FR-1)
> - Mandatory Field Validation (FR-2, US-3) - 20+ fields including UNCooperation Framework, Beneficiaries, High Risk
> - Roles, Responsibilities & Permissions (OM, Collaborator, role transfer)
> - Non-OM/Collaborator Submitter Warning (FR-6, US-2)
> - Country-Org Unit Relationship Warning (FR-7, US-4)
> - OM Recall Capability (FR-8, US-9)
> - Custom Rejection → NO GO (FR-14, US-7)
> - Cancel Opportunity (FR-17, US-11)
> - Reopen from NO GO and CANCELLED stages (FR-15, FR-18, US-8, US-12)
> - Stage Stepper Display Logic (FR-16)
> - Email Notifications with exact wording (FR-9, FR-10)
> - OIC Notifications
> - DoA Pathway Display (DoA2 and DoA3)
> - Visibility & Workflow Lock (inactive OM handling)
> - Cancellation/Archiving Restrictions

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Workflow Orchestration | 8 | P0 |
| Completeness Validation | 6 | P0 |
| Stakeholder Notifications | 5 | P1 |
| Conditional Approvals | 6 | P1 |

---

## 1. Workflow Orchestration (P0)

### TC-OPP-GONG-WF-001: Initiate Decision Process
**Priority:** P0  
**Test Steps:**
1. Opportunity development complete
2. Trigger decision process
3. Verify workflow initiated

**Expected Results:**
- Decision process started
- Status = "Pending Decision"
- DOA holder identified based on value
- Decision package assembled
- Deadline set (e.g., 10 business days)

---

### TC-OPP-GONG-WF-002: Route to Appropriate DOA Level
**Priority:** P0  
**Test Steps:**
1. $500K opportunity
2. $5M opportunity  
3. Verify routing logic

**Expected Results:**
- $500K → DOA3 holder
- $5M → DOA2 holder
- Authority limits checked
- Appropriate person notified

---

### TC-OPP-GONG-WF-003: Sequential Approval Stages
**Priority:** P0  
**Test Steps:**
1. Technical review first
2. Then financial review
3. Then DOA approval
4. Track progression

**Expected Results:**
- Stage 1: Technical (complete)
- Stage 2: Financial (complete)
- Stage 3: DOA decision (pending)
- Cannot skip stages
- Progress visible

---

### TC-OPP-GONG-WF-004: Parallel Review Paths
**Priority:** P1  
**Test Steps:**
1. Technical and financial review in parallel
2. Both complete independently
3. Route to DOA when both done

**Expected Results:**
- Parallel processing
- Either can complete first
- Advance only when both done
- Progress tracked separately

---

### TC-OPP-GONG-WF-005: Decision Deadline Management
**Priority:** P1  
**Test Steps:**
1. Decision requested on Day 1
2. Deadline Day 10
3. Track time remaining

**Expected Results:**
- Countdown displayed
- Reminders at Day 7, Day 9
- Escalation if deadline missed
- Can extend deadline with justification

---

### TC-OPP-GONG-WF-006: Handle Returned Decisions
**Priority:** P0  
**Test Steps:**
1. DOA requests more information
2. Returns to development team
3. Resubmit when ready

**Expected Results:**
- Status = "More Info Needed"
- Feedback provided
- Team can address
- Resubmit to same DOA holder
- Clock resets

---

### TC-OPP-GONG-WF-007: Fast-Track for Urgent Opportunities
**Priority:** P1  
**Test Steps:**
1. Emergency/rapid response flagged
2. Apply fast-track process

**Expected Results:**
- Shortened timeline (5 days vs 10)
- Higher priority in queue
- Expedited reviews
- Escalation threshold lower

---

### TC-OPP-GONG-WF-008: Decision Withdrawal
**Priority:** P2  
**Test Steps:**
1. Decision submitted
2. Submitter withdraws before decision
3. Verify withdrawal handling

**Expected Results:**
- Withdrawal allowed before decision made
- DOA holder notified
- Status = "Withdrawn"
- Can resubmit later if needed

---

## 2. Completeness Validation (P0)

### TC-OPP-GONG-VAL-001: Validate Decision Package Completeness
**Priority:** P0  
**Test Steps:**
1. Attempt to submit incomplete package
2. Verify validation catches

**Expected Results:**
- Required components checked:
  - Opportunity statement ✓
  - DST profile ✓
  - Budget ✓
  - Risk register ✓
  - Due diligence ✓
- Missing items listed
- Cannot submit until complete

---

### TC-OPP-GONG-VAL-002: Validate DST Profile Exists
**Priority:** P0  
**Test Steps:**
1. No DST profile generated
2. Attempt decision submission
3. Verify blocked

**Expected Results:**
- DST profile mandatory
- Clear error message
- Link to generate profile
- Must complete before proceeding

---

### TC-OPP-GONG-VAL-003: Validate Budget Alignment
**Priority:** P0  
**Test Steps:**
1. Budget total ≠ estimated value
2. Flag discrepancy

**Expected Results:**
- Mismatch detected
- Warning displayed
- Must reconcile before submission
- Explanation required if intentional

---

### TC-OPP-GONG-VAL-004: Validate Risk Assessment Complete
**Priority:** P0  
**Test Steps:**
1. Critical risks without mitigation plans
2. Attempt submission
3. Verify validation

**Expected Results:**
- High/critical risks must have mitigations
- Incomplete risk register flagged
- Must address before decision

---

### TC-OPP-GONG-VAL-005: Validate Due Diligence Status
**Priority:** P1  
**Test Steps:**
1. Partner due diligence pending
2. Decision submission attempted
3. Verify handling

**Expected Results:**
- Pending due diligence flagged
- Warning but not blocker
- DOA aware of pending status
- Can proceed with condition

---

### TC-OPP-GONG-VAL-006: Validate Approvals Obtained
**Priority:** P0  
**Test Steps:**
1. Required technical approval missing
2. Attempt DOA decision
3. Verify blocked

**Expected Results:**
- Prerequisite approvals checked
- Missing approvals listed
- Cannot proceed until obtained
- Clear process to get approvals

---

## 3. Stakeholder Notifications (P1)

### TC-OPP-GONG-NOT-001: Notify DOA Holder
**Priority:** P1  
**Test Steps:**
1. Decision package submitted
2. Verify DOA holder notified

**Expected Results:**
- Email notification sent
- In-app notification
- Summary of opportunity
- Link to decision package
- Deadline clear

---

### TC-OPP-GONG-NOT-002: Notify Development Team of Decision
**Priority:** P1  
**Test Steps:**
1. DOA makes Go decision
2. Verify team notified

**Expected Results:**
- Immediate notification
- Decision summary
- Next steps outlined
- Authorization details
- Budget release confirmed

---

### TC-OPP-GONG-NOT-003: Notify on Decision Delays
**Priority:** P1  
**Test Steps:**
1. Decision pending >7 days
2. Verify reminder sent

**Expected Results:**
- Reminder to DOA holder
- Copy to submitter
- Days remaining shown
- Escalation warning if near deadline

---

### TC-OPP-GONG-NOT-004: Notify Finance on Approval
**Priority:** P1  
**Test Steps:**
1. Go decision made
2. Budget authorized
3. Verify Finance notified

**Expected Results:**
- Finance system notification
- Budget amount and account
- Effective date
- Approval authority reference

---

### TC-OPP-GONG-NOT-005: Notify Stakeholders of Status Changes
**Priority:** P1  
**Test Steps:**
1. Status changes during process
2. Verify stakeholders informed

**Expected Results:**
- Partners notified (if appropriate)
- Interested parties updated
- Confidential decisions respected
- Timing of notifications appropriate

---

## 4. Conditional Approvals (P1)

### TC-OPP-GONG-COND-001: Approve with Conditions
**Priority:** P1  
**Test Steps:**
1. DOA approves with 3 conditions
2. Verify conditions tracked

**Expected Results:**
- Decision = "Go with Conditions"
- Conditions explicitly listed
- Responsibility assigned for each
- Deadline for each condition
- Status = "Approved - Pending Conditions"

---

### TC-OPP-GONG-COND-002: Track Condition Fulfillment
**Priority:** P1  
**Test Steps:**
1. Mark condition as complete
2. Verify tracking

**Expected Results:**
- Condition status updated
- Evidence of completion required
- Approver notified
- Progress toward full approval shown

---

### TC-OPP-GONG-COND-003: Full Approval After Conditions Met
**Priority:** P1  
**Test Steps:**
1. All conditions fulfilled
2. Verify full approval granted

**Expected Results:**
- Status = "Fully Approved"
- All authorizations released
- Team can proceed
- Notification sent

---

### TC-OPP-GONG-COND-004: Escalate Unfulfilled Conditions
**Priority:** P1  
**Test Steps:**
1. Condition deadline passed
2. Verify escalation

**Expected Results:**
- Escalation to manager
- Original approver notified
- Can grant extension
- Or revoke conditional approval

---

### TC-OPP-GONG-COND-005: Modify Conditions
**Priority:** P2  
**Test Steps:**
1. Original condition no longer relevant
2. DOA modifies condition

**Expected Results:**
- Can update condition
- Change documented
- Team notified of change
- Original condition preserved in history

---

### TC-OPP-GONG-COND-006: Reject if Conditions Not Feasible
**Priority:** P2  
**Test Steps:**
1. Condition cannot be met
2. Team requests reconsideration

**Expected Results:**
- Can escalate to DOA
- Explain why not feasible
- DOA can waive or modify
- Or convert to No-Go

---

## 5. Integration Tests

### TC-OPP-GONG-INT-001: End-to-End Decision Flow
**Priority:** P0  
**Test Steps:**
1. Submit → Review → Decision → Authorization
2. Verify complete flow

**Expected Results:**
- All stages complete
- Appropriate time at each stage
- All notifications sent
- Final state correct
- Budget and personnel authorized

---

### TC-OPP-GONG-INT-002: Multiple Opportunities in Decision Queue
**Priority:** P1  
**Test Steps:**
1. 10 opportunities awaiting decisions
2. Verify queue management

**Expected Results:**
- Priority ordering
- DOA can see all pending
- Can batch review similar opportunities
- Each tracked independently

---

### TC-OPP-GONG-INT-003: Decision Audit Trail
**Priority:** P0  
**Test Steps:**
1. Complete decision process
2. Generate audit report

**Expected Results:**
- Complete timeline
- All actions logged
- Who, what, when for each step
- Decisions and rationale captured
- Suitable for compliance review

---

## Summary

**Total Test Cases:** 25+  
**Critical (P0):** 15  
**High (P1):** 14  
**Medium (P2):** 3

**Execution Time:** ~8-10 minutes  
**Dependencies:** Decision, Opportunity, User, Notification

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `GoNoGoDecisionTests.cs`  
**Status:** ✅ Ready for Implementation
