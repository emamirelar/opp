# DecisionManager Test Cases

**Manager:** `DecisionManager`  
**Entity:** `OpportunityDecision`, `DecisionPackage`  
**Test Count:** 25+  
**Priority:** P0-P1 (Critical/High)  
**Created:** January 13, 2026

---

## Overview

Test cases for the Go/No-Go decision process for opportunities, including decision package assembly, DOA holder review, decision making, authorization, and audit trail.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Decision Package | 5 | P0 |
| Decision Making | 8 | P0 |
| Authorization | 5 | P0 |
| Delegation | 4 | P1 |
| Audit Trail | 3 | P1 |

---

## 1. Decision Package Assembly (P0)

### TC-OPP-DEC-F-001: Assemble Complete Decision Package
**Priority:** P0  
**Category:** Functional

**Description:**  
Assemble all required documents and data for Go/No-Go decision.

**Preconditions:**
- Opportunity in "Ready for Decision" status
- DST profile generated
- Opportunity statement finalized
- Concept note completed

**Test Steps:**
1. Call `AssembleDecisionPackageAsync(opportunityId)`
2. Verify all components included

**Expected Results:**
- Decision package created with:
  - Final opportunity statement
  - DST profile and insights
  - Draft concept note
  - Opportunity development plan
  - Budget and resource plan
  - Risk register
  - Due diligence status
  - Supporting documents
- Completeness check passed
- Package ready for DOA review

**Test Data:**
```json
{
  "opportunityId": 123,
  "includeAppendices": true,
  "formatForPresentation": true
}
```

---

### TC-OPP-DEC-F-002: Validate Package Completeness
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify all required components present before submission.

**Test Steps:**
1. Attempt to assemble package with missing DST profile
2. Verify validation error

**Expected Results:**
- Validation catches missing components
- Clear list of missing items provided
- Cannot submit incomplete package
- User can complete and retry

---

### TC-OPP-DEC-F-003: Include DST Insights in Package
**Priority:** P0  
**Category:** Functional

**Description:**  
Verify DST analysis properly integrated into decision package.

**Test Steps:**
1. Generate DST profile with recommendations
2. Assemble decision package
3. Verify DST insights included

**Expected Results:**
- DST complexity score prominently displayed
- All 9 parameter scores included
- Critical recommendations highlighted
- Risk assessment integrated
- Similar project lessons included

---

### TC-OPP-DEC-F-004: Configure Required Documentation Checklist
**Priority:** P1  
**Category:** Configuration

**Description:**  
DOA holder can configure required documentation for their level.

**Test Steps:**
1. DOA3 holder sets custom requirements
2. Opportunity submitted for decision
3. Verify custom checklist applied

**Expected Results:**
- Custom requirements saved
- Checklist visible to opportunity manager
- Validation enforces requirements
- Can differ by DOA level

---

### TC-OPP-DEC-F-005: Decision Package Versioning
**Priority:** P1  
**Category:** Audit

**Description:**  
Maintain version history of decision packages.

**Test Steps:**
1. Assemble decision package (v1)
2. Update opportunity
3. Reassemble package (v2)
4. Query package history

**Expected Results:**
- Both versions preserved
- Timestamps recorded
- Changes between versions tracked
- Can view historical packages

---

## 2. Decision Making (P0)

### TC-OPP-DEC-D-001: Record Go Decision
**Priority:** P0  
**Category:** Functional

**Description:**  
DOA holder makes Go decision on opportunity.

**Preconditions:**
- User has DOA authority for opportunity value
- Decision package complete
- Opportunity in correct status

**Test Steps:**
1. Review decision package
2. Call `RecordDecisionAsync(opportunityId, "Go", rationale)`
3. Verify decision recorded

**Expected Results:**
- Decision entity created
- Decision = "Go"
- DecisionDate = current timestamp
- DecisionMakerUserId = current user
- DOALevel recorded
- Rationale captured
- Opportunity status = "Approved"
- Budget and personnel authorized
- Notifications sent to team

**Test Data:**
```json
{
  "opportunityId": 123,
  "decision": "Go",
  "rationale": "Strong strategic alignment, manageable risks, experienced team available",
  "conditions": ["Complete environmental assessment before Q2 2026"],
  "notifyStakeholders": true
}
```

---

### TC-OPP-DEC-D-002: Record No-Go Decision
**Priority:** P0  
**Category:** Functional

**Description:**  
DOA holder makes No-Go decision on opportunity.

**Test Steps:**
1. Review decision package
2. Call `RecordDecisionAsync(opportunityId, "No-Go", rationale)`
3. Verify decision recorded

**Expected Results:**
- Decision entity created
- Decision = "No-Go"
- Rationale clearly documented
- Opportunity status = "Declined"
- Development budget released
- Team notified
- Lessons learned captured

---

### TC-OPP-DEC-D-003: Record Conditional Go Decision
**Priority:** P0  
**Category:** Functional

**Description:**  
DOA holder approves with conditions attached.

**Test Steps:**
1. Call `RecordDecisionAsync()` with conditions
2. Verify conditions tracked

**Expected Results:**
- Decision = "Go with Conditions"
- Conditions clearly listed
- Responsibility assigned for each condition
- Deadlines set for conditions
- Status = "Approved - Pending Conditions"
- Conditions must be met before proceeding

---

### TC-OPP-DEC-D-004: Provide Direction with Decision
**Priority:** P1  
**Category:** Functional

**Description:**  
DOA holder provides direction to development team.

**Test Steps:**
1. Record Go decision
2. Include direction (e.g., "Reduce scope to fit budget")
3. Verify direction captured

**Expected Results:**
- Direction text captured
- Linked to opportunity statement
- Team notified of direction
- Direction visible in opportunity record

---

### TC-OPP-DEC-D-005: Link Decision to Risks
**Priority:** P1  
**Category:** Functional

**Description:**  
DOA holder references specific risks in decision rationale.

**Test Steps:**
1. Reference 3 risks from risk register
2. Record Go decision with risk mitigation plan
3. Verify linkage

**Expected Results:**
- Risks explicitly linked to decision
- Mitigation plans referenced
- Accountability for risk management clear

---

### TC-OPP-DEC-D-006: Decision Without Required Authority
**Priority:** P0  
**Category:** Security

**Description:**  
Prevent decision by user without appropriate DOA level.

**Test Steps:**
1. User with DOA3 authority ($1M limit)
2. Attempt to approve $2.5M opportunity
3. Verify access denied

**Expected Results:**
- `UnauthorizedAccessException` thrown
- Error explains DOA level insufficient
- Decision not recorded
- Can delegate to higher authority

---

### TC-OPP-DEC-D-007: Decision Requires Justification
**Priority:** P0  
**Category:** Validation

**Description:**  
Both Go and No-Go decisions require rationale.

**Test Steps:**
1. Attempt to record decision without rationale
2. Verify validation error

**Expected Results:**
- `BusinessException` thrown
- Error indicates rationale required
- Minimum length enforced (e.g., 50 characters)
- Decision not recorded

---

### TC-OPP-DEC-D-008: Update Decision After Recording
**Priority:** P1  
**Category:** Audit

**Description:**  
Modify decision after initial recording (rare case).

**Test Steps:**
1. Record Go decision
2. Discover error
3. Update decision with justification
4. Verify audit trail

**Expected Results:**
- Original decision preserved
- Update recorded as new version
- Justification for change required
- Both versions visible in history
- Significant change triggers notifications

---

## 3. Authorization (P0)

### TC-OPP-DEC-A-001: Authorize Opportunity Budget
**Priority:** P0  
**Category:** Authorization

**Description:**  
Go decision authorizes use of opportunity development budget.

**Test Steps:**
1. Opportunity has development budget of $100K
2. Record Go decision
3. Verify budget authorized

**Expected Results:**
- Budget marked as "Authorized"
- Funds released for development activities
- Budget tracking begins
- Finance system notified

---

### TC-OPP-DEC-A-002: Authorize Personnel Allocation
**Priority:** P0  
**Category:** Authorization

**Description:**  
Go decision authorizes personnel assignments from development plan.

**Test Steps:**
1. Development plan includes 3 team members
2. Record Go decision
3. Verify personnel authorized

**Expected Results:**
- Team members officially assigned
- HR system notified
- Timesheets enabled
- Team notified of assignment

---

### TC-OPP-DEC-A-003: Authorization Limits by DOA Level
**Priority:** P0  
**Category:** Security

**Description:**  
Different DOA levels have different authorization limits.

**Test Steps:**
1. DOA3 holder ($1M limit)
2. Approve $750K opportunity
3. Verify full authorization
4. Attempt to approve $1.5M opportunity
5. Verify partial authorization only

**Expected Results:**
- Within limit: Full authorization
- Above limit: Requires escalation
- Partial approvals possible
- Clear limits communicated

---

### TC-OPP-DEC-A-004: Revoke Authorization
**Priority:** P1  
**Category:** Authorization

**Description:**  
Ability to revoke authorization if circumstances change.

**Test Steps:**
1. Opportunity approved with budget authorized
2. Major risk discovered
3. Call `RevokeAuthorizationAsync(opportunityId, reason)`
4. Verify authorization revoked

**Expected Results:**
- Authorization status = "Revoked"
- Funds frozen
- Team notified immediately
- Work must stop
- Requires new approval to proceed

---

### TC-OPP-DEC-A-005: Authorization Expiration
**Priority:** P2  
**Category:** Configuration

**Description:**  
Authorization can have expiration date.

**Test Steps:**
1. Record Go decision with 90-day authorization
2. Wait 91 days (simulated)
3. Verify authorization expired

**Expected Results:**
- Authorization expires automatically
- Team notified before expiration
- Can request extension
- Expired authorization requires re-approval

---

## 4. Delegation (P1)

### TC-OPP-DEC-DEL-001: Delegate Decision Authority
**Priority:** P1  
**Category:** Delegation

**Description:**  
DOA holder delegates decision-making to subordinate.

**Test Steps:**
1. DOA2 holder delegates to DOA3 colleague
2. DOA3 makes decision
3. Verify delegation recorded

**Expected Results:**
- Delegation recorded in system
- Delegatee has temporary authority
- Original DOA holder notified
- Delegation audit trail maintained

---

### TC-OPP-DEC-DEL-002: Validate Delegation Authority
**Priority:** P1  
**Category:** Validation

**Description:**  
Can only delegate to users with base DOA authority.

**Test Steps:**
1. Attempt to delegate to user without any DOA
2. Verify validation error

**Expected Results:**
- `BusinessException` thrown
- Error indicates user lacks base authority
- Can only delegate to authorized users
- List of valid delegatees provided

---

### TC-OPP-DEC-DEL-003: Temporary Delegation
**Priority:** P1  
**Category:** Delegation

**Description:**  
Delegation can be time-limited (e.g., during absence).

**Test Steps:**
1. Delegate authority for 2 weeks
2. Verify delegation active during period
3. Verify delegation expires after period

**Expected Results:**
- Start and end dates recorded
- Delegation only valid within period
- Automatic expiration
- Notification before expiration

---

### TC-OPP-DEC-DEL-004: Escalate to Higher Authority
**Priority:** P1  
**Category:** Delegation

**Description:**  
Opportunity manager escalates to higher DOA level.

**Test Steps:**
1. Opportunity value exceeds DOA3 limit
2. Call `EscalateDecisionAsync(opportunityId, higherDOAUserId)`
3. Verify escalation recorded

**Expected Results:**
- Escalation request created
- Higher DOA holder notified
- Escalation reason required
- Decision package transferred
- Timeline for response set

---

## 5. Audit Trail (P1)

### TC-OPP-DEC-AUD-001: Store Versioned Opportunity Statement
**Priority:** P1  
**Category:** Audit

**Description:**  
Opportunity statement versioned and stored at decision point.

**Test Steps:**
1. Finalize opportunity statement
2. Submit for decision
3. Record decision
4. Verify statement version captured

**Expected Results:**
- Statement version created
- Timestamp of snapshot recorded
- Linked to decision entity
- Cannot be modified after decision
- Historical view available

---

### TC-OPP-DEC-AUD-002: Decision History Timeline
**Priority:** P1  
**Category:** Audit

**Description:**  
Complete timeline of decision process visible.

**Test Steps:**
1. Submit opportunity for decision
2. DOA holder requests clarification
3. Opportunity manager responds
4. DOA holder makes decision
5. Query decision timeline

**Expected Results:**
- All steps recorded
- Timestamps for each action
- Users involved captured
- Comments and clarifications preserved
- Can reconstruct entire process

---

### TC-OPP-DEC-AUD-003: Export Decision Audit Report
**Priority:** P2  
**Category:** Reporting

**Description:**  
Generate comprehensive audit report for decision.

**Test Steps:**
1. Record decision
2. Call `GenerateDecisionAuditReportAsync(decisionId)`
3. Verify report content

**Expected Results:**
- PDF report generated
- Includes:
  - Decision details
  - DOA holder information
  - Rationale
  - Conditions
  - Authorization details
  - Timeline
  - Supporting documents
- Suitable for compliance review

---

## Additional Test Scenarios

### TC-OPP-DEC-INT-001: Multiple Opportunities Comparison
**Priority:** P2  
**Category:** Integration

**Description:**  
DOA holder compares multiple opportunities before decision.

**Test Steps:**
1. Prepare 3 opportunities for decision
2. Call `CompareOpportunitiesAsync([id1, id2, id3])`
3. Review comparison report

**Expected Results:**
- Side-by-side comparison
- Key metrics compared:
  - Strategic alignment
  - Complexity score
  - Risk level
  - Resource requirements
- Ranking/prioritization supported
- Helps portfolio decision making

---

### TC-OPP-DEC-INT-002: Decision Notification Workflow
**Priority:** P1  
**Category:** Integration

**Description:**  
Verify all stakeholders notified of decision.

**Test Steps:**
1. Record Go decision
2. Verify notifications sent

**Expected Results:**
- Notifications sent to:
  - Opportunity manager
  - Development team members
  - Finance (for budget)
  - HR (for personnel)
  - Partners (if appropriate)
- Email and in-app notifications
- Includes decision summary
- Links to full details

---

### TC-OPP-DEC-INT-003: Decision Updates Opportunity Workflow
**Priority:** P1  
**Category:** Integration

**Description:**  
Decision triggers appropriate workflow state transitions.

**Test Steps:**
1. Opportunity in "Decision Pending" status
2. Record Go decision
3. Verify workflow progression

**Expected Results:**
- Status changes to "Approved"
- Next steps auto-created
- Development phase begins
- Milestones generated
- Team can proceed with work

---

## Performance Tests

### TC-OPP-DEC-PERF-001: Decision Package Assembly Performance
**Priority:** P2  
**Category:** Performance

**Description:**  
Verify decision package assembly completes quickly.

**Test Steps:**
1. Assemble package for complex opportunity
2. Measure execution time

**Expected Results:**
- Package assembled in < 10 seconds
- Documents retrieved efficiently
- PDF generation optimized
- Can run async if needed

---

### TC-OPP-DEC-PERF-002: Concurrent Decisions
**Priority:** P2  
**Category:** Performance

**Description:**  
Multiple DOA holders making decisions simultaneously.

**Test Steps:**
1. 3 DOA holders make decisions concurrently
2. Verify all processed correctly

**Expected Results:**
- No locking issues
- All decisions recorded
- No data corruption
- Audit trail accurate

---

## Summary

**Total Test Cases:** 25+  
**Critical (P0):** 15  
**High (P1):** 12  
**Medium (P2):** 5

**Execution Time:** ~8-10 minutes for full suite  
**Dependencies:** Opportunity, User (DOA levels), Budget, Personnel  
**Key Integrations:** Workflow engine, Notification service

---

## Test Data Requirements

### DOA Users
- DOA1: > $5M authority
- DOA2: $1M - $5M authority
- DOA3: $100K - $1M authority
- DOA4: < $100K authority

### Sample Opportunities
- Small: $50K (within DOA4)
- Medium: $500K (within DOA3)
- Large: $2.5M (requires DOA2)
- Very Large: $10M (requires DOA1)

### Decision Scenarios
- Clear Go (all green)
- Clear No-Go (multiple red flags)
- Borderline (mixed signals)
- Conditional approval
- Escalation required

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `DecisionManagerTests.cs`  
**Status:** ✅ Ready for Implementation
