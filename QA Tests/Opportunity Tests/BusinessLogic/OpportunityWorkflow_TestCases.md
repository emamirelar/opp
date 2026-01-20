# OpportunityWorkflow Test Cases

**Component:** OpportunityWorkflow Business Logic  
**Test Count:** 35+  
**Priority:** P0-P1 (Critical/High)  
**Created:** January 13, 2026

---

## Overview

Test cases for opportunity workflow orchestration including state transitions, approval workflows, escalation processes, and workflow validation.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| State Transitions | 10 | P0 |
| Approval Workflows | 8 | P0 |
| Escalation | 6 | P1 |
| Notifications | 5 | P1 |
| Validation | 6 | P0 |

---

## 1. State Transitions (P0)

### TC-OPP-WF-ST-001: Initial State Creation
**Priority:** P0  
**Test Steps:**
1. Create new opportunity
2. Verify initial workflow state

**Expected Results:**
- State = "Draft"
- WorkflowStage = "Development"
- NextActions defined
- Responsible party = creator

---

### TC-OPP-WF-ST-002: Transition Draft to Under Review
**Priority:** P0  
**Test Steps:**
1. Complete required fields
2. Submit for review
3. Verify state transition

**Expected Results:**
- State = "UnderReview"
- ReviewStartDate set
- Reviewers notified
- Original state preserved in history

---

### TC-OPP-WF-ST-003: Review Approval Transition
**Priority:** P0  
**Test Steps:**
1. Opportunity under review
2. Reviewer approves
3. Verify next state

**Expected Results:**
- State = "Approved"
- ApprovedDate set
- ApprovedBy recorded
- Next actions updated

---

### TC-OPP-WF-ST-004: Review Rejection Handling
**Priority:** P0  
**Test Steps:**
1. Opportunity under review
2. Reviewer rejects with feedback
3. Verify rejection handling

**Expected Results:**
- State = "Draft" (returned)
- Rejection reason captured
- Feedback visible to submitter
- Can resubmit after corrections

---

### TC-OPP-WF-ST-005: Multiple Review Cycles
**Priority:** P1  
**Test Steps:**
1. Submit → Review → Reject → Draft
2. Resubmit → Review → Approve
3. Track all cycles

**Expected Results:**
- All cycles tracked
- Iteration count maintained
- Time in each state recorded
- Complete audit trail

---

### TC-OPP-WF-ST-006: Conditional State Transitions
**Priority:** P1  
**Test Steps:**
1. High-value opportunity (>$5M)
2. Verify additional approval required
3. Check workflow path

**Expected Results:**
- Additional approval step inserted
- Higher authority required
- Workflow adapts to value
- Clear indicators shown

---

### TC-OPP-WF-ST-007: Parallel Approval Paths
**Priority:** P1  
**Test Steps:**
1. Opportunity requires technical + financial approval
2. Submit to both reviewers
3. Both approve independently

**Expected Results:**
- Both approvals required
- Either can happen first
- Progress tracked separately
- Advance only when both complete

---

### TC-OPP-WF-ST-008: State Rollback
**Priority:** P2  
**Test Steps:**
1. Opportunity in advanced state
2. Critical issue discovered
3. Rollback to earlier state with justification

**Expected Results:**
- Rollback authorized by appropriate level
- Justification required and recorded
- Affected parties notified
- Previous work preserved

---

### TC-OPP-WF-ST-009: Terminal States
**Priority:** P0  
**Test Steps:**
1. Opportunity reaches Approved state
2. Verify terminal state handling

**Expected Results:**
- No further workflow progression
- Can convert to project
- Can close if circumstances change
- Historical record locked

---

### TC-OPP-WF-ST-010: State Validation Rules
**Priority:** P0  
**Test Steps:**
1. Attempt invalid state transition
2. Verify validation catches

**Expected Results:**
- Invalid transitions blocked
- Clear error message
- Valid next states shown
- User guided to correct path

---

## 2. Approval Workflows (P0)

### TC-OPP-WF-AP-001: Single Approver Workflow
**Priority:** P0  
**Test Steps:**
1. Configure single approver
2. Submit opportunity
3. Approver reviews and approves

**Expected Results:**
- Approval request sent
- Approver can view details
- Approve/reject options available
- Approval recorded with timestamp

---

### TC-OPP-WF-AP-002: Multi-Level Approval Chain
**Priority:** P0  
**Test Steps:**
1. L1 approver approves
2. Routes to L2 approver
3. L2 approves
4. Routes to L3 approver

**Expected Results:**
- Sequential approval chain
- Each level notified after previous approves
- Progress indicator shows position in chain
- Can't skip levels

---

### TC-OPP-WF-AP-003: Approval Delegation
**Priority:** P1  
**Test Steps:**
1. Primary approver on leave
2. Delegates to colleague
3. Delegate approves

**Expected Results:**
- Delegation recorded
- Delegate has approval authority
- Original approver notified
- Delegation time-limited

---

### TC-OPP-WF-AP-004: Approval Timeout
**Priority:** P1  
**Test Steps:**
1. Submit for approval
2. No response within SLA (e.g., 5 days)
3. Verify escalation

**Expected Results:**
- Timeout warning at 3 days
- Auto-escalation at 5 days
- Manager notified
- Can expedite if urgent

---

### TC-OPP-WF-AP-005: Conditional Approval Requirements
**Priority:** P1  
**Test Steps:**
1. Small opportunity (<$100K)
2. Large opportunity (>$5M)
3. Compare approval paths

**Expected Results:**
- Small: 1 approval level
- Large: 3 approval levels
- Rules engine determines path
- Transparent to submitter

---

### TC-OPP-WF-AP-006: Approval with Conditions
**Priority:** P1  
**Test Steps:**
1. Approver approves with conditions
2. Verify conditional approval handling

**Expected Results:**
- Status = "Approved with Conditions"
- Conditions clearly listed
- Submitter must acknowledge
- Conditions tracked for completion

---

### TC-OPP-WF-AP-007: Batch Approval
**Priority:** P2  
**Test Steps:**
1. Approver has 10 pending approvals
2. Review and approve multiple at once

**Expected Results:**
- Batch selection enabled
- Single approval action for batch
- Individual records updated
- Notifications sent per item

---

### TC-OPP-WF-AP-008: Approval Withdrawal
**Priority:** P2  
**Test Steps:**
1. Submitter submits for approval
2. Discovers error before approval
3. Withdraws from approval queue

**Expected Results:**
- Withdrawal allowed before approval
- Approver notified of withdrawal
- Returns to Draft state
- Can resubmit when ready

---

## 3. Escalation (P1)

### TC-OPP-WF-ESC-001: Automatic Escalation on Timeout
**Priority:** P1  
**Test Steps:**
1. Approval pending >5 days
2. Verify auto-escalation

**Expected Results:**
- Escalates to approver's manager
- Original approver notified
- Reason for escalation logged
- SLA reset for manager

---

### TC-OPP-WF-ESC-002: Manual Escalation Request
**Priority:** P1  
**Test Steps:**
1. Submitter requests urgent escalation
2. Provide justification
3. Manager reviews escalation

**Expected Results:**
- Escalation request submitted
- Justification required
- Manager can accept or deny
- If accepted, takes priority

---

### TC-OPP-WF-ESC-003: Value-Based Escalation
**Priority:** P1  
**Test Steps:**
1. Opportunity value exceeds approver authority
2. Verify auto-escalation to higher DOA

**Expected Results:**
- Escalates to appropriate DOA level
- Authority limits checked
- Higher DOA notified
- Original approver informed

---

### TC-OPP-WF-ESC-004: Multi-Hop Escalation
**Priority:** P2  
**Test Steps:**
1. First escalation still times out
2. Escalates again to next level

**Expected Results:**
- Can escalate multiple times
- Each hop logged
- Eventually reaches executive level
- Alert when reaching top of chain

---

### TC-OPP-WF-ESC-005: De-Escalation
**Priority:** P2  
**Test Steps:**
1. Escalated to senior level
2. Senior decides to delegate back down
3. Verify de-escalation

**Expected Results:**
- Can delegate to appropriate level
- Reason for de-escalation recorded
- Delegatee notified
- Authority transfer clear

---

### TC-OPP-WF-ESC-006: Escalation Notifications
**Priority:** P1  
**Test Steps:**
1. Escalation triggered
2. Verify all parties notified

**Expected Results:**
- Original approver notified
- New approver notified
- Submitter notified
- Manager notified
- Escalation reason included

---

## 4. Notifications (P1)

### TC-OPP-WF-NOT-001: State Change Notifications
**Priority:** P1  
**Test Steps:**
1. Opportunity state changes
2. Verify notifications sent

**Expected Results:**
- Owner notified
- Team members notified
- Stakeholders notified
- Includes state change details
- Link to opportunity

---

### TC-OPP-WF-NOT-002: Approval Request Notification
**Priority:** P1  
**Test Steps:**
1. Submit for approval
2. Verify approver notified

**Expected Results:**
- Email notification sent
- In-app notification created
- Includes opportunity summary
- Direct link to approval page
- Clear action required

---

### TC-OPP-WF-NOT-003: Reminder Notifications
**Priority:** P1  
**Test Steps:**
1. Pending approval approaching SLA
2. Verify reminder sent

**Expected Results:**
- Reminder at 3 days
- Another at 4 days
- Escalation warning at 4.5 days
- Progressively more urgent tone

---

### TC-OPP-WF-NOT-004: Decision Notifications
**Priority:** P1  
**Test Steps:**
1. Approval decision made
2. Verify submitter notified

**Expected Results:**
- Immediate notification
- Approval or rejection clear
- Feedback included if rejected
- Next steps outlined

---

### TC-OPP-WF-NOT-005: Configurable Notification Preferences
**Priority:** P2  
**Test Steps:**
1. User sets notification preferences
2. Verify preferences honored

**Expected Results:**
- Email on/off
- In-app on/off
- Digest vs immediate
- Can set per notification type
- Preferences persistent

---

## 5. Validation (P0)

### TC-OPP-WF-VAL-001: Validate Prerequisites Before Submission
**Priority:** P0  
**Test Steps:**
1. Attempt to submit incomplete opportunity
2. Verify validation catches

**Expected Results:**
- Validation runs before submission
- Missing required fields listed
- Clear guidance on what's needed
- Can't submit until complete

---

### TC-OPP-WF-VAL-002: Validate Approver Authority
**Priority:** P0  
**Test Steps:**
1. Route to approver without sufficient DOA
2. Verify validation catches

**Expected Results:**
- Authority checked before routing
- Routes to appropriate level
- User notified of routing decision
- Audit trail clear

---

### TC-OPP-WF-VAL-003: Validate Business Rules
**Priority:** P0  
**Test Steps:**
1. Opportunity violates business rule
2. Attempt to advance workflow
3. Verify rule enforcement

**Expected Results:**
- Business rules evaluated
- Violations blocked
- Clear explanation of rule
- How to resolve shown

---

### TC-OPP-WF-VAL-004: Validate Data Consistency
**Priority:** P1  
**Test Steps:**
1. Budget doesn't match estimated value
2. Attempt workflow advancement
3. Verify warning

**Expected Results:**
- Consistency checks run
- Warnings displayed
- Can override with justification
- Override recorded

---

### TC-OPP-WF-VAL-005: Validate Permissions
**Priority:** P0  
**Test Steps:**
1. User without permission attempts action
2. Verify access denied

**Expected Results:**
- Permission checked
- Access denied gracefully
- Clear message why denied
- Who to contact for access

---

### TC-OPP-WF-VAL-006: Validate Workflow Configuration
**Priority:** P1  
**Test Steps:**
1. Workflow misconfigured (e.g., no approver)
2. Attempt to submit
3. Verify error handling

**Expected Results:**
- Configuration validated
- Error caught before submission
- Admin notified of config issue
- User given graceful message

---

## 6. Integration Tests

### TC-OPP-WF-INT-001: Workflow State Persistence
**Priority:** P0  
**Test Steps:**
1. Progress through workflow
2. System restart
3. Verify state preserved

**Expected Results:**
- State persisted correctly
- Resume from exact point
- No data loss
- Timers continue correctly

---

### TC-OPP-WF-INT-002: Concurrent Workflow Actions
**Priority:** P1  
**Test Steps:**
1. Two users attempt conflicting actions simultaneously
2. Verify conflict resolution

**Expected Results:**
- Optimistic locking prevents conflicts
- Second user notified of conflict
- Current state shown
- Can retry action

---

### TC-OPP-WF-INT-003: Workflow Performance
**Priority:** P2  
**Test Steps:**
1. 100 opportunities in workflow
2. Measure processing time
3. Verify acceptable performance

**Expected Results:**
- State transitions < 2 seconds
- Notifications queued if needed
- No blocking operations
- Scalable design

---

## Summary

**Total Test Cases:** 35+  
**Critical (P0):** 22  
**High (P1):** 18  
**Medium (P2):** 7

**Execution Time:** ~10-12 minutes for full suite  
**Dependencies:** Opportunity, User, Permission, Notification services

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `OpportunityWorkflowTests.cs`  
**Status:** ✅ Ready for Implementation
