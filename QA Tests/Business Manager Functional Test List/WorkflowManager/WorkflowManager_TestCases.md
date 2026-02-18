# WorkflowManager — Comprehensive Test Cases

**Component:** `WorkflowController`, `WorkflowManager`, `PaoWorkflowApproverProvider`, `PaoEntityStageProvider`, `OpportunityStageRequirementsProvider`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (N≥3P, E≥3P, F≥3P, I≥3P)

---

## Implementation Status

| Component | Path | Status |
|-----------|------|--------|
| WorkflowController | `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | ✅ Fully Implemented |
| OpportunityWorkflow (State Machine) | `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` | ✅ Fully Implemented |
| WorkflowManager (Core) | `UNOPS.Workflow/UNOPS.Workflow.Business/Managers/WorkflowManager.cs` | ✅ Fully Implemented |
| PaoWorkflowApproverProvider | `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs` | ✅ Fully Implemented |
| PaoEntityStageProvider | `UNOPS.PAO.Business/Workflow/Adapters/PaoEntityStageProvider.cs` | ✅ Fully Implemented |
| OpportunityStageRequirementsProvider | `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirementsProvider.cs` | ✅ Fully Implemented |
| PaoWorkflowNotificationService | `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` | ✅ Fully Implemented |
| StageWorkflowComponent (Frontend) | `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/` | ✅ Fully Implemented |
| WorkflowService (Frontend) | `workflow.service.ts` | ✅ Fully Implemented |

### Workflow Architecture

```
┌──────────────────────────────────────────────────────────┐
│  Angular: StageWorkflowComponent → WorkflowService       │
└───────────────────────┬──────────────────────────────────┘
                        │ HTTP
┌───────────────────────▼──────────────────────────────────┐
│  WorkflowController (/api/workflow/*)                    │
│  11 endpoints: stages, state, details, requirements,     │
│  submit, approve, reject, recall, cancel, reopen, history│
└───────────────────────┬──────────────────────────────────┘
                        │
┌───────────────────────▼──────────────────────────────────┐
│  PAO Adapters → UNOPS.Workflow Submodule                 │
│  PaoEntityStageProvider, PaoWorkflowApproverProvider,    │
│  OpportunityStageRequirementsProvider (21 validations)   │
└──────────────────────────────────────────────────────────┘
```

### State Machine (4 stages, 5 transitions)

```
IDENTIFY & PROFILE ──[Submit for Go]──→ GO (requires approval)
IDENTIFY & PROFILE ──[Submit for No Go]──→ NO GO (requires approval)
IDENTIFY & PROFILE ──[Cancel]──→ CANCELLED (OM only, no approval)
NO GO ──[Reopen]──→ IDENTIFY & PROFILE (OM only, no approval)
CANCELLED ──[Reopen]──→ IDENTIFY & PROFILE (OM only, no approval)
```

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
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Mandatory Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## §1 Positive Tests (30)

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Stage Configuration & State Retrieval (6)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | GetWorkflowStages returns all 4 stages for opportunity | Authenticated user | GET /api/workflow/opportunity | Returns 4 stages: IDENTIFY & PROFILE (seq 1), GO (seq 2), NO GO (seq 3), CANCELLED (seq 4); ordered by sequence | P0 |
| POS-002 | GetWorkflowState returns current stage and actions for I&P opportunity | Opportunity at IDENTIFY & PROFILE stage, user is OM | GET /api/workflow/opportunity/{id} | CurrentStage="IDENTIFY & PROFILE", IsInWorkflow=false, AvailableActions includes GO and NO GO targets | P0 |
| POS-003 | GetWorkflowState shows pending stage when in workflow | Opportunity submitted for GO, pending approval | GET /api/workflow/opportunity/{id} | IsInWorkflow=true, PendingStage="GO", AvailableActions is empty | P0 |
| POS-004 | GetWorkflowDetails returns approvers list during pending approval | Opportunity submitted for GO, DoA2 holder exists | GET /api/workflow/opportunity/{id}/details | IsInWorkflow=true, Approvers list includes DoA2 holder with name/email/role, CanApprove correct | P0 |
| POS-005 | GetWorkflowHistory returns ordered history entries | Opportunity with multiple transitions | GET /api/workflow/opportunity/{id}/history | Returns history entries with FromStage, ToStage, Action, PerformedBy, PerformedOn, Comment; chronologically ordered | P0 |
| POS-006 | GetRequirements returns 21 requirements for GO transition | Opportunity at I&P stage | GET /api/workflow/opportunity/{id}/requirements/GO | Returns exactly 20 client-side requirements (21st is server-side only); includes name, description, fieldName, fieldType | P0 |

### Submit Workflow (8)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-007 | Submit for GO with all confirmations triggers approval workflow | Fully completed opportunity, user is OM | POST /api/workflow/submit with ConfirmedNonOMSubmission=true (or user is OM), ConfirmedOrgUnitWarning=true, AcknowledgedStatement=true | Success=true, ApprovalRequired=true, PendingStage="GO", WorkflowStatus=InWorkflow | P0 |
| POS-008 | Submit triggers Non-OM warning when submitter is not OM | User is a collaborator (not OM), opportunity complete | POST /api/workflow/submit (no confirmation flags) | Success=false, RequiresConfirmation=true, ConfirmationType="NonOMSubmitter", ConfirmationMessage includes user's role | P0 |
| POS-009 | Submit triggers OrgUnit mismatch warning for unrelated countries | Opportunity has countries not mapped to responsible OrgUnit | POST /api/workflow/submit with ConfirmedNonOMSubmission=true | Success=false, RequiresConfirmation=true, ConfirmationType="OrgUnitCountryMismatch", UnrelatedCountries list populated | P0 |
| POS-010 | Submit triggers acknowledgment step after confirmations | All prior confirmations passed | POST /api/workflow/submit with prior confirmations=true, AcknowledgedStatement=false | Success=false, RequiresAcknowledgment=true, AcknowledgmentText includes org unit name | P0 |
| POS-011 | Submit regenerates Opportunity Statement before approval | Opportunity complete, GeminiManager available | POST /api/workflow/submit with all flags true | OpportunityStatementMarkdown regenerated via GeminiManager; submission proceeds even if regeneration fails | P0 |
| POS-012 | Submit for NO GO transitions directly (no approval needed) | Opportunity at I&P, user is OM, NO GO has no approval seeded | POST /api/workflow/submit with NewStage="NO GO" | Success=true, ApprovalRequired=false, NewStage="NO GO" (if no approval), or ApprovalRequired=true if approval configured | P1 |
| POS-013 | Submit creates WorkflowLog entry | Opportunity at I&P, all validations pass | POST /api/workflow/submit | WorkflowLog created with EntityName="Opportunity", Stage="IDENTIFY & PROFILE", NewStage="GO", Action="Submit", UserId set | P0 |
| POS-014 | Submit with optional comment stores comment in log | Opportunity at I&P | POST /api/workflow/submit with Comment="Important context" | WorkflowLog.Comment = "Important context" | P1 |

### Approve Workflow (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-015 | Approve GO decision with rationale, confirmation, and Executive | Pending GO approval, user is DoA2 holder | POST /api/workflow/approve with Rationale="Strategic fit", ConfirmationAcknowledged=true, ExecutiveId=valid | Success, Stage="GO", Status=Active, ExecutiveId assigned, WorkflowStatus=None | P0 |
| POS-016 | Approve sends internal stakeholder notification for GO | Pending GO approval, other org unit stakeholders exist | POST /api/workflow/approve | NotifyInternalStakeholdersOnGoDecisionAsync called, notifications sent to cross-org stakeholders | P0 |
| POS-017 | Approve marks in-system notifications as done | Pending GO approval | POST /api/workflow/approve | MarkWorkflowNotificationsAsApprovedAsync called for this entity | P1 |
| POS-018 | Approve sets Opportunity Status to Active for GO stage | Pending GO approval | POST /api/workflow/approve | Opportunity.Status = EntityStatus.Active | P0 |
| POS-019 | Approve logs completed workflow entry | Pending approval | POST /api/workflow/approve | WorkflowLog updated with CompletedOn timestamp, rationale as comment | P1 |

### Reject Workflow (3)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-020 | Reject sets Opportunity to NO GO/Closed | Pending GO approval, user is DoA2 | POST /api/workflow/reject with Rationale="Not aligned", ConfirmationAcknowledged=true | Stage="NO GO", Status=Closed, WorkflowStatus=None, success message | P0 |
| POS-021 | Reject stores rationale in workflow log | Pending approval | POST /api/workflow/reject | WorkflowLog.Comment = rationale text | P0 |
| POS-022 | Reject marks notifications as rejected | Pending approval | POST /api/workflow/reject | MarkWorkflowNotificationsAsRejectedAsync called | P1 |

### Recall Workflow (3)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-023 | Recall by submitter restores entity to pre-workflow state | Pending approval, current user is submitter | POST /api/workflow/recall with Comment="Need to update data" | WorkflowStatus=None, entity returns to editable state, success | P0 |
| POS-024 | Recall by OM succeeds for Opportunity | Pending approval, current user is OM (not submitter) | POST /api/workflow/recall with Comment="Corrections needed" | WorkflowStatus=None, success | P0 |
| POS-025 | Recall marks notifications as recalled | Pending approval | POST /api/workflow/recall | MarkWorkflowNotificationsAsRecalledAsync called | P1 |

### Cancel & Reopen (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-026 | Cancel opportunity from I&P by OM | Opportunity at I&P, not in workflow, user is OM | POST /api/workflow/cancel with Comment="No longer relevant" | Stage="CANCELLED", Status=Closed, WorkflowStatus=None, WorkflowLog with Action="Cancelled" | P0 |
| POS-027 | Reopen from NO GO by OM | Opportunity at NO GO stage, user is OM | POST /api/workflow/reopen | Stage="IDENTIFY & PROFILE", Status=Draft, WorkflowStatus=None, WorkflowLog with Action="Reopened" | P0 |
| POS-028 | Reopen from CANCELLED by OM with comment | Opportunity at CANCELLED stage, user is OM | POST /api/workflow/reopen with Comment="New information available" | Stage="IDENTIFY & PROFILE", Status=Draft | P0 |
| POS-029 | Cancel creates audit trail entry | OM cancels from I&P | POST /api/workflow/cancel | WorkflowLog: Stage=I&P, NewStage=CANCELLED, Action="Cancelled", CompletedOn set | P1 |
| POS-030 | Reopen creates audit trail entry | OM reopens from NO GO | POST /api/workflow/reopen | WorkflowLog: Stage=NO GO, NewStage=IDENTIFY & PROFILE, Action="Reopened", CompletedOn set | P1 |

---

## §2 Negative Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Invalid Entity / Route (10)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | GetWorkflowStages for unknown entity type | GET /api/workflow/contract | 404 Not Found: "Workflow not found for entity type 'contract'" | P0 |
| NEG-002 | GetWorkflowState for non-existent entity ID | GET /api/workflow/opportunity/999999 | 404 Not Found: entity not found | P0 |
| NEG-003 | GetWorkflowState for deleted (soft-deleted) opportunity | Opportunity with IsDeleted=true | 404 Not Found: entity not found | P0 |
| NEG-004 | GetWorkflowDetails for unknown entity type | GET /api/workflow/partner/1/details | 404 Not Found: workflow not found | P0 |
| NEG-005 | GetWorkflowHistory for non-existent entity | GET /api/workflow/opportunity/0/history | 404 Not Found | P0 |
| NEG-006 | GetRequirements for unknown entity type | GET /api/workflow/partner/1/requirements | 404 Not Found | P0 |
| NEG-007 | GetWorkflowStages with empty entity name | GET /api/workflow/ | 404 or route mismatch | P1 |
| NEG-008 | GetWorkflowState with negative ID | GET /api/workflow/opportunity/-1 | 404 or validation error | P1 |
| NEG-009 | GetWorkflowState with string ID instead of int | GET /api/workflow/opportunity/abc | 400 Bad Request / model binding error | P1 |
| NEG-010 | GetWorkflowDetails for entity with no stage set | Entity with Stage=null or empty | 400 "Entity has no workflow stage" | P1 |

### Submit Validation Failures (20)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-011 | Submit with unknown entity type | EntityName="contract" | 404 Not Found: workflow not found | P0 |
| NEG-012 | Submit for non-existent entity ID | EntityId=999999 | 404 Not Found: entity not found | P0 |
| NEG-013 | Submit for entity already in workflow | Entity has pending approval | 400 "Entity is already in a workflow approval process" | P0 |
| NEG-014 | Submit with invalid transition (GO→GO) | Opportunity already at GO | 400 "Transition from 'GO' to 'GO' is not allowed" | P0 |
| NEG-015 | Submit from NO GO→GO directly | Opportunity at NO GO, NewStage="GO" | 400 "Transition not allowed" (must reopen first) | P0 |
| NEG-016 | Submit from CANCELLED→GO directly | Opportunity at CANCELLED | 400 "Transition not allowed" | P0 |
| NEG-017 | Submit with empty EntityName | EntityName="" | 404 or validation error | P1 |
| NEG-018 | Submit with null NewStage | NewStage=null | Model binding or validation error | P1 |
| NEG-019 | Submit with empty NewStage | NewStage="" | 400 "Transition not allowed" | P1 |
| NEG-020 | Submit when mandatory comment missing (non-Go flow) | Comment required by action config, Comment=null | 400 "Comment is required for this transition" | P0 |
| NEG-021 | Submit for GO with unmet requirements (no name) | Opportunity.Name is null | Success=false, RequirementsNotMet=true, UnmetRequirements includes nameRequired | P0 |
| NEG-022 | Submit for GO with unmet requirements (no description) | Description is null/empty | RequirementsNotMet=true, includes descriptionRequired | P0 |
| NEG-023 | Submit for GO with unmet requirements (no budget) | InitiativeBudgetUSD is null or 0 | RequirementsNotMet=true, includes budgetRequired | P0 |
| NEG-024 | Submit for GO with unmet requirements (no deliverables) | Deliverables collection empty | RequirementsNotMet=true, includes productsRequired | P0 |
| NEG-025 | Submit for GO with unmet requirements (no countries) | Countries collection empty | RequirementsNotMet=true, includes countriesRequired | P0 |
| NEG-026 | Submit for GO with unmet requirements (no SDGs) | SDGs collection empty | RequirementsNotMet=true, includes sdgRequired | P0 |
| NEG-027 | Submit for GO with unmet requirements (no funding partner) | FundingPartners empty | RequirementsNotMet=true, includes fundingPartnerRequired | P0 |
| NEG-028 | Submit for GO with unmet requirements (no client partner) | ClientPartners empty | RequirementsNotMet=true, includes clientPartnerRequired | P0 |
| NEG-029 | Submit for GO with unmet requirements (no OM stakeholder) | No stakeholder with "Opportunity Manager" role | RequirementsNotMet=true, includes managerRequired | P0 |
| NEG-030 | Submit for GO with unmet requirements (no DoA holder) | ResponsibleOrgUnit has no DoA2 or DoA3 | RequirementsNotMet=true, includes doaHolderRequired | P0 |

### More Requirement Failures (10)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-031 | Submit GO: no challenges text | Challenges=null | RequirementsNotMet, includes challengesRequired | P1 |
| NEG-032 | Submit GO: no expected impact | ExpectedImpact=null | RequirementsNotMet, includes impactRequired | P1 |
| NEG-033 | Submit GO: no expected outcomes | ExpectedOutcomes=null | RequirementsNotMet, includes outcomesRequired | P1 |
| NEG-034 | Submit GO: no beneficiaries and TBD not checked | BeneficiariesToBeDetermined=false, DirectBeneficiaries=0 | RequirementsNotMet, includes beneficiariesRequired | P1 |
| NEG-035 | Submit GO: no UNOPS missions and NotApplicable unchecked | UNOPSMissionsNotApplicable=false, Missions empty | RequirementsNotMet, includes missionsRequired | P1 |
| NEG-036 | Submit GO: no target signing date | TargetSigningDate=null | RequirementsNotMet, includes signingDateRequired | P1 |
| NEG-037 | Submit GO: no implementation start date | ImplementationStartDate=null | RequirementsNotMet, includes startDateRequired | P1 |
| NEG-038 | Submit GO: no target delivery date | TargetDeliveryDate=null | RequirementsNotMet, includes endDateRequired | P1 |
| NEG-039 | Submit GO: no opportunity statement | OpportunityStatementMarkdown=null | RequirementsNotMet, includes statementRequired | P1 |
| NEG-040 | Submit GO: no responsible org unit | ResponsibleOrgUnitId=null | RequirementsNotMet, includes orgUnitRequired | P1 |

### Approve Validation Failures (15)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-041 | Approve with empty rationale | Rationale="" | 400 "Decision rationale is required" | P0 |
| NEG-042 | Approve with null rationale | Rationale=null | 400 "Decision rationale is required" | P0 |
| NEG-043 | Approve with whitespace-only rationale | Rationale="   " | 400 "Decision rationale is required" | P0 |
| NEG-044 | Approve without confirmation acknowledged | ConfirmationAcknowledged=false | 400 "Confirmation statement must be acknowledged" | P0 |
| NEG-045 | Approve Opportunity without ExecutiveId | ExecutiveId=0 | 400 "Executive assignment is required for Go decision" | P0 |
| NEG-046 | Approve Opportunity with negative ExecutiveId | ExecutiveId=-1 | 400 "Executive assignment is required for Go decision" | P0 |
| NEG-047 | Approve when no pending workflow exists | No pending task for entity | 400 "No pending workflow found for this entity" | P0 |
| NEG-048 | Approve by user without approval permission | User not in approvers list | 403 "You do not have permission to approve" | P0 |
| NEG-049 | Approve own submission (self-approval blocked) | User submitted AND is DoA2 | CanApprove=false in details; approve returns 403 | P0 |
| NEG-050 | Approve for non-existent entity | EntityId=999999 | Error (pending task not found) | P1 |
| NEG-051 | Approve for unknown entity type | EntityName="contract" | 400 or error | P1 |
| NEG-052 | Approve after entity is soft-deleted | Opportunity soft-deleted during pending | Error condition | P1 |
| NEG-053 | Approve when WorkflowManager.Approve returns empty stage | Internal failure | 500 "Failed to approve workflow" | P1 |
| NEG-054 | Approve with ExecutiveId pointing to non-existent user | ExecutiveId=999999 | Error or executive not assigned | P1 |
| NEG-055 | Approve for entity that was already approved (duplicate) | PendingTask already completed | 400 "No pending workflow found" | P1 |

### Reject Validation Failures (10)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-056 | Reject with empty rationale | Rationale="" | 400 "Decision rationale is required" | P0 |
| NEG-057 | Reject with null rationale | Rationale=null | 400 "Decision rationale is required" | P0 |
| NEG-058 | Reject without confirmation acknowledged | ConfirmationAcknowledged=false | 400 "Confirmation statement must be acknowledged" | P0 |
| NEG-059 | Reject when no pending workflow | No pending task | 400 "No pending workflow found" | P0 |
| NEG-060 | Reject by user without approval permission | User not in approvers list | 403 "You do not have permission to reject" | P0 |
| NEG-061 | Reject for unknown entity type | EntityName="unknown" | Error | P1 |
| NEG-062 | Reject for non-existent entity | EntityId=0 | Error | P1 |
| NEG-063 | Reject own submission | User is both submitter and approver | 403 (blocked by CanApprove=false) | P1 |
| NEG-064 | Reject with whitespace-only rationale | Rationale="   \n\t  " | 400 "Decision rationale is required" | P1 |
| NEG-065 | Reject for Opportunity not found in DB | Opportunity physically missing | Error on Opportunity lookup | P1 |

### Recall Validation Failures (10)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-066 | Recall when no pending workflow | No pending task | 400 "No pending workflow found" | P0 |
| NEG-067 | Recall without mandatory comment | Comment=null or empty | 400 "Justification is required when recalling" | P0 |
| NEG-068 | Recall by user who is not submitter and not OM | Random user | 403 "Only the submitter or Opportunity Manager can recall" | P0 |
| NEG-069 | Recall with whitespace-only comment | Comment="   " | 400 "Justification is required" | P0 |
| NEG-070 | Recall for unknown entity type | EntityName="partner" | Error (no pending task found for non-workflow entities) | P1 |
| NEG-071 | Recall for non-existent entity | EntityId=999999 | 400 "No pending workflow found" | P1 |
| NEG-072 | Recall when WorkflowManager.Recall fails | Internal failure | 500 "Failed to recall workflow" | P1 |
| NEG-073 | Recall after approval already completed | Task completed | 400 "No pending workflow found" | P1 |
| NEG-074 | Recall for entity that was soft-deleted | Entity IsDeleted=true | Error | P1 |
| NEG-075 | Recall by former OM (role removed during workflow) | User was OM at submit time, role removed since | 403 access denied (OM check uses current roles) | P2 |

### Cancel Validation Failures (8)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-076 | Cancel for non-Opportunity entity | EntityName="partner" | 400 "Cancel action is only supported for Opportunities" | P0 |
| NEG-077 | Cancel without mandatory comment | Comment=null or empty | 400 "Comment is required when cancelling" | P0 |
| NEG-078 | Cancel from GO stage | Opportunity at GO stage | 400 "Opportunity can only be cancelled from IDENTIFY & PROFILE" | P0 |
| NEG-079 | Cancel from NO GO stage | Opportunity at NO GO | 400 same error as above | P0 |
| NEG-080 | Cancel from CANCELLED stage | Already cancelled | 400 same error | P1 |
| NEG-081 | Cancel by non-OM user | User is collaborator, not OM | 403 "Only the Opportunity Manager can cancel" | P0 |
| NEG-082 | Cancel while in workflow (pending approval) | Entity has pending task | 400 "Cannot cancel while in workflow approval process" | P0 |
| NEG-083 | Cancel for soft-deleted opportunity | IsDeleted=true | 404 "Opportunity not found" | P1 |

### Reopen Validation Failures (7)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-084 | Reopen for non-Opportunity entity | EntityName="contact" | 400 "Reopen action is only supported for Opportunities" | P0 |
| NEG-085 | Reopen from I&P stage | Opportunity at IDENTIFY & PROFILE | 400 "Opportunity can only be reopened from NO GO or CANCELLED" | P0 |
| NEG-086 | Reopen from GO stage | Opportunity at GO | 400 same error | P0 |
| NEG-087 | Reopen by non-OM user | User is not OM | 403 "Only the Opportunity Manager can reopen" | P0 |
| NEG-088 | Reopen from CANCELLED without comment | Stage=CANCELLED, Comment=null | 400 "Comment is required when reopening from CANCELLED" | P0 |
| NEG-089 | Reopen for soft-deleted opportunity | IsDeleted=true | 404 "Opportunity not found" | P1 |
| NEG-090 | Reopen for non-existent entity | EntityId=999999 | 404 "Opportunity not found" | P1 |

---

## §3 Boundary/Edge Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Stage Transition Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-001 | Submit from I&P to GO with exactly 1 of each required field | Minimal valid data | Requirements pass, submission proceeds | P0 |
| BND-002 | Submit with budget of exactly $0.01 (minimum positive) | InitiativeBudgetUSD=0.01 | Budget requirement met | P0 |
| BND-003 | Submit with budget of exactly $0 | InitiativeBudgetUSD=0 | Budget requirement NOT met | P0 |
| BND-004 | Submit with negative budget | InitiativeBudgetUSD=-100 | Budget requirement NOT met | P0 |
| BND-005 | Submit with extremely large budget | InitiativeBudgetUSD=999999999999 | Budget requirement met, no overflow | P1 |
| BND-006 | Multiple stage transitions in rapid succession | Submit→Recall→Submit within seconds | Each transition creates proper log entries, no state corruption | P0 |
| BND-007 | Reopen from NO GO with empty optional comment | Stage=NO GO, Comment="" | Succeeds (comment optional from NO GO) | P0 |
| BND-008 | Reopen from NO GO with null comment | Stage=NO GO, Comment=null | Succeeds (comment optional from NO GO) | P0 |
| BND-009 | Reopen from CANCELLED with minimum 1-char comment | Stage=CANCELLED, Comment="x" | Succeeds (comment not blank) | P1 |
| BND-010 | Cancel with minimum 1-char comment | Comment="x" | Succeeds | P1 |
| BND-011 | Submit with all 21 requirements barely met | Each field has minimal valid value | All requirements pass | P0 |
| BND-012 | Submit with 20 of 21 requirements met (missing 1) | Only org unit missing | RequirementsNotMet=true, exactly 1 unmet | P0 |
| BND-013 | Submit with 0 of 21 requirements met | Completely empty opportunity | RequirementsNotMet=true, all 21 items listed | P0 |
| BND-014 | Stage name case sensitivity: "go" vs "GO" | Submit with NewStage="go" | Transition validates correctly (case-insensitive comparison) | P1 |
| BND-015 | Entity name case sensitivity: "Opportunity" vs "opportunity" | GET /api/workflow/OPPORTUNITY | NormalizeEntityNameForWorkflow handles capitalization | P1 |

### Beneficiaries Edge Cases (10)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-016 | Beneficiaries: TBD=true with zero direct/indirect | BeneficiariesToBeDetermined=true, Direct=0, Indirect=0 | Requirement met (TBD overrides) | P0 |
| BND-017 | Beneficiaries: TBD=false, Direct=1, Indirect=0 | Minimum valid non-TBD | Requirement met (Direct>0, Indirect>=0) | P0 |
| BND-018 | Beneficiaries: TBD=false, Direct=0, Indirect=100 | Direct=0 but Indirect set | Requirement NOT met (Direct must be >0) | P0 |
| BND-019 | Beneficiaries: TBD=false, Direct=-1, Indirect=0 | Negative direct | Requirement NOT met | P1 |
| BND-020 | Beneficiaries: TBD=false, Direct=1, Indirect=-1 | Negative indirect | Requirement NOT met (Indirect must be >=0) | P1 |
| BND-021 | Beneficiaries: TBD=null, Direct=null, Indirect=null | All null | Requirement NOT met | P1 |
| BND-022 | Beneficiaries: TBD=true with Direct=999999999 | TBD + large values | Requirement met (TBD takes precedence) | P2 |
| BND-023 | Beneficiaries: Direct=int.MaxValue | Extreme large number | Requirement met, no overflow | P2 |
| BND-024 | Beneficiaries: TBD=false, Direct=0, Indirect=0 | Both zero, not TBD | Requirement NOT met (Direct not >0) | P1 |
| BND-025 | Beneficiaries: TBD=true, Direct=-100 | TBD overrides even negative | Requirement met | P2 |

### UNOPS Missions Edge Cases (5)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-026 | Missions: NotApplicable=true with empty missions | MissionsNotApplicable=true, Missions=[] | Requirement met | P0 |
| BND-027 | Missions: NotApplicable=false with 1 mission | MissionsNotApplicable=false, 1 mission | Requirement met | P0 |
| BND-028 | Missions: NotApplicable=false with 0 missions | MissionsNotApplicable=false, Missions=[] | Requirement NOT met | P0 |
| BND-029 | Missions: NotApplicable=true with missions present | Both flag and missions set | Requirement met (flag takes precedence) | P1 |
| BND-030 | Missions: NotApplicable=null (default false) | Not explicitly set | Missions collection required | P2 |

### Approver Resolution Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-031 | Approver: DoA2 exists on OrgUnit | Standard approval path | DoA2 holder returned as approver | P0 |
| BND-032 | Approver: No DoA2, DoA3 fallback | OrgUnit has DoA3 but no DoA2 | DoA3 holder returned as approver (fallback) | P0 |
| BND-033 | Approver: Neither DoA2 nor DoA3 | OrgUnit has no DoA holders | Empty approvers list or requirement fails | P0 |
| BND-034 | Approver: Multiple DoA2 holders on same OrgUnit | 2+ users with DoA2 role | All DoA2 holders returned | P1 |
| BND-035 | Approver: DoA2 holder is also submitter | OM has DoA2 role, submits | Removed from approvers in details; CanApprove=false | P0 |
| BND-036 | Approver: DoA2 holder is soft-deleted user | EntityUserRole IsDeleted=true | Excluded from approvers list | P1 |
| BND-037 | Approver: OrgUnit has DoA1 only (not DoA2/3) | Only DoA1 role exists | DoA1 not valid approver, empty list | P1 |
| BND-038 | Approver: OrgUnit itself is soft-deleted | OrgUnit IsDeleted=true | Edge case handling for approval | P2 |
| BND-039 | Approver details when submitter removed from approvers | Submitter is also in approvers | Submitter filtered out of Approvers list | P0 |
| BND-040 | Approver: CanRecall for submitter | Current user = pending task UserId | CanRecall=true | P0 |
| BND-041 | Approver: CanRecall for OM (not submitter) | Current user is OM, not submitter | CanRecall=true (OM can recall for Opportunity) | P0 |
| BND-042 | Approver: CanRecall for non-OM non-submitter | User is neither | CanRecall=false | P0 |
| BND-043 | Workflow details with no initiator user found | pendingTask.UserId points to deleted user | InitiatedBy=null, no crash | P2 |
| BND-044 | DoA level resolution in history for non-Opportunity entity | EntityName != "Opportunity" | Falls back to entity-level DOA lookup | P2 |
| BND-045 | DoA level resolution when OrgUnit has no DOA roles | History user has no DOA | DoaLevel=null in history | P1 |

### Country-OrgUnit Mismatch Boundaries (10)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-046 | Submit GO: All countries mapped to OrgUnit | 3 countries, all in OrgUnit relationships | No mismatch warning, proceeds to acknowledgment | P0 |
| BND-047 | Submit GO: 1 of 3 countries unmapped | 2 mapped, 1 not | Mismatch warning, UnrelatedCountries has 1 entry | P0 |
| BND-048 | Submit GO: All countries unmapped | 3 countries, none in OrgUnit | Mismatch warning, all 3 in UnrelatedCountries | P0 |
| BND-049 | Submit GO: No countries on opportunity | Countries collection empty | Requirements check fails first (countriesRequired) | P1 |
| BND-050 | Submit GO: OrgUnit has no country relationships | OrgUnit relationships empty | All countries are "unrelated" | P1 |
| BND-051 | Submit GO: Country with null name in mismatch | Country.Name=null | Excluded from unrelated list (null filter) | P2 |
| BND-052 | Submit GO: ResponsibleOrgUnitId is null | No org unit selected | No mismatch check (orgUnitRequired fails first) | P1 |
| BND-053 | Submit GO: Soft-deleted OrgUnit relationship | Relationship IsDeleted=true | Not counted as mapped | P1 |
| BND-054 | CountryMappings ordered alphabetically | Multiple countries | CountryMappings sorted by CountryName | P2 |
| BND-055 | CountryMappings shows IsMapped status correctly | Mix of mapped/unmapped | Each entry correctly flags IsMapped | P1 |

### Non-OM Submitter Warning Boundaries (5)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-056 | Submit GO: User is OM → no NonOM warning | OM submits | Skips directly to country mismatch check | P0 |
| BND-057 | Submit GO: User is collaborator → NonOM warning | Collaborator submits | RequiresConfirmation, ConfirmationType="NonOMSubmitter" | P0 |
| BND-058 | Submit GO: User is stakeholder with no role → NonOM warning | No EntityRole on stakeholder | Warning shows "stakeholder" as fallback role | P1 |
| BND-059 | Submit GO: User is not a stakeholder at all | No stakeholder record | Warning shows null role as fallback | P1 |
| BND-060 | Submit GO: OM info includes name and email | OM exists | OpportunityManagerInfo formatted as "Name (email)" | P1 |

### Workflow State Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-061 | WorkflowStatus transitions: None → InWorkflow on submit | Submit with approval | WorkflowStatus set to InWorkflow | P0 |
| BND-062 | WorkflowStatus transitions: InWorkflow → None on approve | Approve pending | WorkflowStatus set to None | P0 |
| BND-063 | WorkflowStatus transitions: InWorkflow → None on reject | Reject pending | WorkflowStatus set to None | P0 |
| BND-064 | WorkflowStatus transitions: InWorkflow → None on recall | Recall pending | WorkflowStatus set to None | P0 |
| BND-065 | IsInWorkflow property reflects WorkflowStatus | WorkflowStatus=InWorkflow | IsInWorkflow returns true | P0 |
| BND-066 | IsInWorkflow false when WorkflowStatus=None | WorkflowStatus=None | IsInWorkflow returns false | P0 |
| BND-067 | Opportunity status after GO approval: Active | Approved to GO | Opportunity.Status = Active | P0 |
| BND-068 | Opportunity status after NO GO rejection: Closed | Rejected to NO GO | Opportunity.Status = Closed | P0 |
| BND-069 | Opportunity status after Cancel: Closed | Cancelled | Opportunity.Status = Closed | P0 |
| BND-070 | Opportunity status after Reopen from NO GO: Draft | Reopened | Opportunity.Status = Draft | P0 |
| BND-071 | Opportunity status after Reopen from CANCELLED: Draft | Reopened | Opportunity.Status = Draft | P0 |
| BND-072 | Direct transition (no approval): stage changes immediately | Transition with approvalRequired=false | Stage updated, CompletedOn set, no pending task | P0 |
| BND-073 | Approval-required transition: stage stays until approved | Transition with approvalRequired=true | Stage remains at current, PendingTask created | P0 |
| BND-074 | UpdateEntityWorkflowStatus throws for unsupported entity | EntityName="Partner" | NotImplementedException thrown | P2 |
| BND-075 | Concurrent recall and approve: first wins | Two users act simultaneously | One succeeds, other gets "No pending workflow found" | P1 |

### History Display Edge Cases (10)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-076 | History with no entries | New opportunity, never transitioned | Returns empty list | P0 |
| BND-077 | History with single Submit entry | One submission only | Single entry with Action="Submit" | P0 |
| BND-078 | History with full lifecycle | Submit→Reject→Reopen→Submit→Approve | 5 entries in chronological order | P0 |
| BND-079 | History entry with deleted user | User soft-deleted after action | PerformedBy may be null or show cached name | P2 |
| BND-080 | History entry with missing UserProfile | User exists but UserProfile null | UserName falls back to Email | P1 |
| BND-081 | History stage display names resolve correctly | StageNames dictionary used | FromStageDisplayName and ToStageDisplayName populated | P0 |
| BND-082 | History entry with empty comment | Comment="" | Comment field present but empty | P1 |
| BND-083 | History entry with very long comment (5000 chars) | Long rationale text | Full comment stored and returned | P2 |
| BND-084 | History DoA level resolved for approver | Approver has DoA2 on OrgUnit | PerformedBy.DoaLevel = "DoA2" | P1 |
| BND-085 | History for entity with 100+ transitions | Heavy workflow history | All entries returned, performance acceptable | P2 |

### Pending Approvals Boundary (5)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-086 | GetPendingApprovals returns empty (stub) | Any user | Returns empty list (TODO stub) | P0 |
| BND-087 | GetPendingApprovals with authenticated user | Valid auth | 200 OK with empty array | P0 |
| BND-088 | GetPendingApprovals without authentication | No auth header | 401 Unauthorized | P1 |
| BND-089 | NormalizeEntityNameForWorkflow for "opportunity" | Input="opportunity" | Returns "Opportunity" | P1 |
| BND-090 | NormalizeEntityNameForWorkflow for unknown entity | Input="contract" | Returns "contract" unchanged | P1 |

---

## §4 Functional Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Full Workflow Lifecycle Tests (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-001 | Complete GO lifecycle: I&P → Submit → Approve → GO | OM creates opportunity, fills all 21 fields, submits, DoA2 approves | Stage=GO, Status=Active, ExecutiveId set, WorkflowStatus=None, history has 2 entries | P0 |
| FUN-002 | Complete NO GO lifecycle: I&P → Submit → Reject → NO GO | OM submits, DoA2 rejects | Stage=NO GO, Status=Closed, WorkflowStatus=None, rationale in history | P0 |
| FUN-003 | Cancel lifecycle: I&P → Cancel → CANCELLED | OM cancels with reason | Stage=CANCELLED, Status=Closed, comment in history | P0 |
| FUN-004 | Reopen from NO GO: NO GO → Reopen → I&P → Submit → Approve → GO | Full recovery path | All transitions logged, final state GO/Active | P0 |
| FUN-005 | Reopen from CANCELLED: CANCELLED → Reopen → I&P → Submit → Approve | Full recovery from cancel | All transitions logged | P0 |
| FUN-006 | Recall lifecycle: I&P → Submit → Recall → I&P → Submit → Approve | Submission recalled then resubmitted | WorkflowStatus toggles correctly, 5 history entries | P0 |
| FUN-007 | Multiple rejection cycles: Submit → Reject → Reopen → Submit → Reject → Reopen | 2+ rejection cycles | Each cycle logged, data integrity maintained | P0 |
| FUN-008 | Cancel then reopen then cancel again | I&P→Cancel→Reopen→Cancel | Both cancel+reopen cycles logged correctly | P1 |
| FUN-009 | Full cycle with Non-OM submitter confirmations | Non-OM user goes through all 3 confirmation steps | Each step returns correct confirmation type, final submit succeeds | P0 |
| FUN-010 | Full cycle with OrgUnit mismatch confirmations | Opportunity has unmapped countries | OrgUnitCountryMismatch step triggered and confirmed | P0 |
| FUN-011 | Full cycle with acknowledgment step | All prior steps pass, acknowledgment required | AcknowledgmentText includes OrgUnit name, acknowledged=true proceeds | P0 |
| FUN-012 | Submit → Statement regeneration → Approval | Statement regenerated during submit | New statement saved before approval | P1 |
| FUN-013 | Submit when statement regeneration fails (graceful fallback) | GeminiManager throws exception | Submission still proceeds, warning logged | P0 |
| FUN-014 | Workflow with DoA3 fallback (no DoA2 on OrgUnit) | Only DoA3 exists | DoA3 holder is approver, approval succeeds | P0 |
| FUN-015 | Workflow with multiple DoA2 holders (any can approve) | 3 DoA2 holders | All shown as approvers, first to approve wins | P1 |

### Requirements Validation Functional Tests (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-016 | All 21 requirements met → submit proceeds | Fully filled opportunity | No RequirementsNotMet response, proceed to confirmations | P0 |
| FUN-017 | Requirements check blocks submit before confirmations | Missing name field | RequirementsNotMet returned BEFORE any confirmation step | P0 |
| FUN-018 | Requirements endpoint filters server-side-only requirements | GET requirements for GO | DoA holder requirement (OnlyServerSideEvaluation=true) NOT in response | P0 |
| FUN-019 | Requirements for non-GO transitions return empty | Transition to NO GO or CANCELLED | Empty requirements list | P0 |
| FUN-020 | Requirements provider handles unknown entity name | EntityName="partner" | No matching provider, returns empty list | P1 |
| FUN-021 | Beneficiaries conditional: TBD=true bypasses number check | BeneficiariesToBeDetermined=true | Requirement passes regardless of beneficiary numbers | P0 |
| FUN-022 | Beneficiaries conditional: TBD=false requires Direct>0 | BeneficiariesToBeDetermined=false | Direct must be positive for requirement to pass | P0 |
| FUN-023 | UNOPS Missions conditional: NotApplicable=true bypasses list | UNOPSMissionsNotApplicable=true | Requirement passes with empty missions | P0 |
| FUN-024 | UNOPS Missions conditional: NotApplicable=false requires list | UNOPSMissionsNotApplicable=false | Must have at least 1 mission | P0 |
| FUN-025 | Stakeholder role validation: OM role exists | Stakeholder with "Opportunity Manager" EntityRole | managerRequired passes | P0 |
| FUN-026 | Stakeholder role validation: OM role missing | No stakeholder with OM role | managerRequired fails | P0 |
| FUN-027 | DoA holder validation: DoA2 exists | EntityUserRole with DoA2_OrganizationHierarchy code | doaHolderRequired passes | P0 |
| FUN-028 | DoA holder validation: DoA3 fallback exists | No DoA2 but DoA3_OrganizationHierarchy exists | doaHolderRequired passes | P0 |
| FUN-029 | DoA holder validation: neither exists | No DoA2 or DoA3 | doaHolderRequired fails | P0 |
| FUN-030 | DoA holder check uses ResponsibleOrgUnitId | OrgUnit specified | EntityUserRoles filtered by OrgUnit EntityId | P1 |

### Confirmation Flow Order (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-031 | Step 1: Requirements validated first | Submit with missing name | RequirementsNotMet response before any confirmation | P0 |
| FUN-032 | Step 2: Non-OM warning after requirements pass | Non-OM user, all requirements met | NonOMSubmitter confirmation after requirements pass | P0 |
| FUN-033 | Step 3: OrgUnit mismatch after Non-OM confirmed | Countries unmapped, NonOM confirmed | OrgUnitCountryMismatch confirmation | P0 |
| FUN-034 | Step 4: Acknowledgment after all confirmations | All priors confirmed | RequiresAcknowledgment response | P0 |
| FUN-035 | Step 5: Final submit after acknowledgment | All flags set to true | Success=true, workflow initiated | P0 |
| FUN-036 | OM submitter skips Non-OM warning | User is OM | Goes directly from requirements to country check | P0 |
| FUN-037 | No unmapped countries skips mismatch warning | All countries mapped | Goes directly from Non-OM step to acknowledgment | P0 |
| FUN-038 | OM with all countries mapped goes directly to acknowledgment | OM user, all mapped | Requirements → Acknowledgment (skip 2 steps) | P0 |
| FUN-039 | Partial confirmation resend: only missing flags | Send with NonOM=true but Ack=false | Returns RequiresAcknowledgment (skips NonOM) | P1 |
| FUN-040 | All confirmations in single request | All flags set from first request | Proceeds directly to workflow initiation | P1 |

### Notification Functional Tests (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-041 | Submit initiates approval notification to approvers | Submit for GO | _workflowManager.Initiate called with correct parameters | P0 |
| FUN-042 | Approve triggers GO notification to internal stakeholders | Approve to GO stage | NotifyInternalStakeholdersOnGoDecisionAsync called | P0 |
| FUN-043 | Approve marks workflow notifications as approved | Approve action | MarkWorkflowNotificationsAsApprovedAsync called | P1 |
| FUN-044 | Reject marks notifications as rejected | Reject action | MarkWorkflowNotificationsAsRejectedAsync called | P1 |
| FUN-045 | Recall marks notifications as recalled | Recall action | MarkWorkflowNotificationsAsRecalledAsync called | P1 |
| FUN-046 | Submit includes entity URL with statement anchor | Submit for GO | entityUrl ends with "#statement" | P1 |
| FUN-047 | Approve includes entity URL | Approve action | entityUrl includes opportunity ID | P1 |
| FUN-048 | Entity display name passed to notifications | Submit/Approve | Opportunity name passed to notification service | P1 |
| FUN-049 | Current user name resolved for GO notification | Approve to GO | GetCurrentUserNameAsync returns proper name or email fallback | P1 |
| FUN-050 | Notification user name uses FirstName LastName or email | User with/without profile | Full name or email used as fallback | P2 |

### Workflow Log Functional Tests (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-051 | Submit creates pending log entry | Approval-required submit | Log: Status=Active, CompletedOn=null, RequiresApproval=true | P0 |
| FUN-052 | Direct transition creates completed log entry | No-approval transition | Log: Action="StageChanged", CompletedOn set | P0 |
| FUN-053 | Cancel creates completed log entry | Cancel action | Log: Action="Cancelled", CompletedOn set | P0 |
| FUN-054 | Reopen creates completed log entry | Reopen action | Log: Action="Reopened", CompletedOn set | P0 |
| FUN-055 | Log stores correct EntityName (normalized) | Submit as "opportunity" | Log.EntityName = "Opportunity" (normalized) | P0 |
| FUN-056 | Log stores correct EntityId | Submit entity #42 | Log.EntityId = "42" (string) | P1 |
| FUN-057 | Log stores correct UserId | User #10 submits | Log.UserId = 10 | P1 |
| FUN-058 | Log stores correct stage and newStage | I&P → GO | Log.Stage = "IDENTIFY & PROFILE", NewStage = "GO" | P0 |
| FUN-059 | Log stores submit comment | Comment="Important" | Log.Comment = "Important" | P1 |
| FUN-060 | Approval log stores rationale as comment | Rationale="Strategic" | WorkflowLog comment = "Strategic" | P0 |

### Executive Assignment Functional Tests (5)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-061 | Approve GO assigns Executive to Opportunity | ExecutiveId=valid user | AssignExecutiveAsync called with correct IDs | P0 |
| FUN-062 | Executive not assigned for non-Opportunity entity | EntityName != "Opportunity" | AssignExecutiveAsync NOT called | P1 |
| FUN-063 | Executive assigned after workflow approval succeeds | Approve success | Executive assignment happens AFTER stage update | P1 |
| FUN-064 | Executive assignment failure doesn't roll back approval | AssignExecutiveAsync throws | Approval still succeeds (executive is separate concern) | P2 |
| FUN-065 | Executive ID persisted on Opportunity entity | Approve with ExecutiveId=5 | Opportunity.ExecutiveId = 5 in database | P0 |

### Stage Provider Functional Tests (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-066 | GetCurrentStageAsync returns correct stage | Opportunity at GO | Returns "GO" | P0 |
| FUN-067 | IsEntityValidAsync returns true for existing entity | Valid opportunity ID | Returns true | P0 |
| FUN-068 | IsEntityValidAsync returns false for deleted entity | IsDeleted=true | Returns false | P0 |
| FUN-069 | IsEntityValidAsync returns false for non-existent ID | ID=999999 | Returns false | P0 |
| FUN-070 | UpdateStageAsync changes entity stage | Update to GO | Opportunity.Stage = "GO" | P0 |
| FUN-071 | GetEntityDisplayNameAsync returns opportunity name | Named opportunity | Returns opportunity name/title | P1 |
| FUN-072 | OM check uses EntityRole.Name exact match | Role="Opportunity Manager" | Match is exact, not partial | P0 |
| FUN-073 | OM check uses OpportunityStakeholder table | User in stakeholders | Joins OpportunityStakeholder with EntityRole | P0 |
| FUN-074 | GetStateMachine returns null for unknown entity | entityName="contract" | Returns null → 404 | P0 |
| FUN-075 | GetStateMachine returns OpportunityWorkflow for "opportunity" | entityName="opportunity" | Returns OpportunityWorkflow.StateMachine | P0 |

### Trigger Permission Functional Tests (5)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-076 | Available actions only include triggerable transitions | User is OM | Actions for which GetTriggerConfigurationAsync returns user | P0 |
| FUN-077 | Non-triggerable transitions excluded from actions | User is collaborator, no trigger permission | AvailableActions empty or filtered | P0 |
| FUN-078 | Actions empty when entity is in workflow | IsInWorkflow=true | AvailableActions is empty list | P0 |
| FUN-079 | Actions show RequiresApproval flag correctly | GO transition requires approval | RequiresApproval=true in action model | P0 |
| FUN-080 | Actions show CommentRequired/Optional flags | Action has "mandatory" comment config | CommentRequired=true, CommentOptional=false | P1 |

### Reject-Specific Functional Tests (5)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-081 | Opportunity rejection goes to NO GO (custom behavior) | Reject Opportunity workflow | Stage = NO GO (not back to previous stage) | P0 |
| FUN-082 | Opportunity rejection sets Status to Closed | Reject | Opportunity.Status = EntityStatus.Closed | P0 |
| FUN-083 | Opportunity rejection sets WorkflowStatus to None | Reject | WorkflowStatus = None | P0 |
| FUN-084 | Non-Opportunity rejection uses standard workflow reject | EntityName != "Opportunity" | _workflowManager.Reject called, standard behavior | P1 |
| FUN-085 | Rejection updates LastModifiedBy and LastModifiedDate | Reject | CurrentUserId and DateTime.UtcNow set | P1 |

### Cancel/Reopen State Consistency (5)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-086 | Cancel sets all state fields consistently | Cancel opportunity | Stage=CANCELLED, Status=Closed, WorkflowStatus=None, LastModifiedBy set | P0 |
| FUN-087 | Reopen resets all state fields consistently | Reopen from NO GO | Stage=I&P, Status=Draft, WorkflowStatus=None, LastModifiedBy set | P0 |
| FUN-088 | Cancel preserves previousStage in log | Cancel from I&P | WorkflowLog.Stage = "IDENTIFY & PROFILE" | P1 |
| FUN-089 | Reopen preserves previousStage in log | Reopen from CANCELLED | WorkflowLog.Stage = "CANCELLED" | P1 |
| FUN-090 | Reopen message includes previous stage name | Reopen from CANCELLED | Message includes "from CANCELLED" | P2 |

---

## §5 Integration Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### API Endpoint Integration (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-001 | GET /api/workflow/opportunity returns stages JSON | Full HTTP request | 200 OK, JSON array with 4 stage objects | P0 |
| INT-002 | GET /api/workflow/opportunity/{id} returns state JSON | Valid ID | 200 OK, WorkflowStateResponse with CurrentStage, AvailableActions | P0 |
| INT-003 | GET /api/workflow/opportunity/{id}/details returns details | Pending workflow | 200 OK, WorkflowDetailsResponse with Approvers, CanApprove, CanRecall | P0 |
| INT-004 | GET /api/workflow/opportunity/{id}/requirements returns list | I&P stage | 200 OK, List of StageRequirement objects | P0 |
| INT-005 | GET /api/workflow/opportunity/{id}/requirements/GO explicit target | Explicit stage | 200 OK, same requirements as auto-detected | P0 |
| INT-006 | POST /api/workflow/submit creates workflow | Valid request | 200 OK, WorkflowSubmitResponse with Success=true | P0 |
| INT-007 | POST /api/workflow/approve completes workflow | Valid approval | 200 OK, success with newStage | P0 |
| INT-008 | POST /api/workflow/reject completes as NO GO | Valid rejection | 200 OK, WorkflowActionResponse with NewStage=NO GO | P0 |
| INT-009 | POST /api/workflow/recall cancels pending | Valid recall | 200 OK, success message | P0 |
| INT-010 | POST /api/workflow/cancel sets CANCELLED | Valid cancel | 200 OK, WorkflowActionResponse with NewStage=CANCELLED | P0 |
| INT-011 | POST /api/workflow/reopen sets I&P | Valid reopen | 200 OK, WorkflowActionResponse with NewStage=I&P | P0 |
| INT-012 | GET /api/workflow/opportunity/{id}/history returns array | Valid ID | 200 OK, WorkflowHistoryResponse array | P0 |
| INT-013 | GET /api/workflow/pending-approvals returns empty (stub) | Authenticated | 200 OK, empty array | P0 |
| INT-014 | All endpoints require IAP authentication | No auth header | 401 Unauthorized | P0 |
| INT-015 | Submit returns 404 for invalid entity type | EntityName="invalid" | 404 Not Found | P0 |
| INT-016 | Submit returns 404 for non-existent entity | EntityId=999999 | 404 Not Found | P0 |
| INT-017 | Approve returns 400 when no pending task | No pending | 400 Bad Request | P0 |
| INT-018 | Cancel returns 400 for non-Opportunity | EntityName="partner" | 400 Bad Request | P0 |
| INT-019 | Reopen returns 400 from invalid stage | Stage=I&P | 400 Bad Request | P0 |
| INT-020 | History returns 404 for non-existent entity | Invalid ID | 404 Not Found | P0 |

### Database Integration (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-021 | Submit creates WorkflowLog row in database | Submit for GO | SELECT from workflow.WorkflowLogs shows new entry | P0 |
| INT-022 | Approve updates Opportunity.Stage in database | Approve | Opportunity record Stage="GO" | P0 |
| INT-023 | Approve updates Opportunity.Status in database | Approve to GO | Status=Active | P0 |
| INT-024 | Approve updates Opportunity.WorkflowStatus | Approve | WorkflowStatus=None (from InWorkflow) | P0 |
| INT-025 | Approve assigns Executive in database | ExecutiveId provided | Opportunity.ExecutiveId set in DB | P0 |
| INT-026 | Reject updates Opportunity stage and status | Reject | Stage=NO GO, Status=Closed in DB | P0 |
| INT-027 | Cancel updates all Opportunity fields | Cancel | Stage=CANCELLED, Status=Closed, WorkflowStatus=None | P0 |
| INT-028 | Reopen resets Opportunity fields | Reopen from NO GO | Stage=I&P, Status=Draft in DB | P0 |
| INT-029 | WorkflowLog preserves Stage and NewStage | Multiple transitions | Correct from/to stages for each log entry | P0 |
| INT-030 | WorkflowLog preserves UserId | Multiple users act | Correct UserId for each action | P0 |
| INT-031 | Opportunity split queries load all collections | Submit for GO | Countries, SDGs, FundingPartners, ClientPartners, Deliverables, UNOPSMissions, Stakeholders all loaded | P0 |
| INT-032 | AsNoTracking used for read-only queries | Submit, Details, History | No change tracking overhead | P1 |
| INT-033 | Country-OrgUnit relationship lookup joins correctly | Submit with mismatch | OrganizationUnitRelationship filtered by OrgUnit and EntityType="Country" | P1 |
| INT-034 | EntityUserRole DoA lookup uses correct EntityType | History with DoA | EntityType="OrganizationHierarchy" filter used | P1 |
| INT-035 | PAOUsers.Include(UserProfile) resolves names | History, Details | UserProfile.Name or Email returned | P1 |
| INT-036 | Opportunity.Stakeholders loaded with EntityRole | Submit GO | Stakeholder.EntityRole.Name available for OM check | P0 |
| INT-037 | Soft-deleted EntityUserRoles excluded from DoA check | IsDeleted=true on role | Not counted as DoA holder | P1 |
| INT-038 | Multiple SaveChangesAsync calls in reject don't corrupt | Reject updates multiple fields | Single SaveChangesAsync for stage/status, separate for workflow log | P1 |
| INT-039 | Concurrent database operations don't deadlock | Parallel submit + read | No deadlock detected | P1 |
| INT-040 | Transaction consistency across submit steps | Submit with approval | All-or-nothing: log + status update either both succeed or both fail | P1 |

### Workflow Submodule Integration (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-041 | WorkflowManager.PendingTask returns correct task | Active pending task | Returns task with EntityName, EntityId, NewStage, UserId | P0 |
| INT-042 | WorkflowManager.PendingTask returns null when none | No pending task | Returns null | P0 |
| INT-043 | WorkflowManager.NextActions returns available transitions | From I&P stage | Returns GO and NO GO targets | P0 |
| INT-044 | WorkflowManager.ApprovalNeeded returns true for GO | I&P → GO | Returns true | P0 |
| INT-045 | WorkflowManager.Approve completes task | Valid approval | Returns new stage, task completed | P0 |
| INT-046 | WorkflowManager.Reject marks task | Valid rejection | Task rejected in submodule | P0 |
| INT-047 | WorkflowManager.Recall cancels task | Valid recall | Task recalled, returns success | P0 |
| INT-048 | WorkflowManager.AddLog persists entry | New log entry | Entry saved in workflow schema | P0 |
| INT-049 | WorkflowManager.GetWorkflowHistory returns chronological | Multiple entries | Ordered by time | P0 |
| INT-050 | WorkflowManager.Initiate sends notifications | Initiation request | Notification service invoked | P1 |
| INT-051 | WorkflowManager.WorkflowStateByStage resolves state | I&P stage code | Returns State object with transitions | P0 |
| INT-052 | PaoEntityStageProvider bridges PAO to workflow submodule | Entity operations | Stage read/write through provider | P0 |
| INT-053 | PaoWorkflowApproverProvider resolves DoA2 holders | OrgUnit with DoA2 | Returns approver list | P0 |
| INT-054 | PaoWorkflowApproverProvider DoA3 fallback | OrgUnit without DoA2 | Falls back to DoA3 holders | P0 |
| INT-055 | PaoWorkflowApproverProvider.CanUserApproveAsync | DoA2 user | Returns true | P0 |

### Frontend-Backend Integration (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-056 | WorkflowService.getWorkFlowForEntity maps to GET stages | Frontend call | Correct API path, response parsed to WorkflowStageModel[] | P0 |
| INT-057 | WorkflowService.getNextWorkFlowActionsForARecordById maps to GET state | Frontend call | Response parsed to WorkflowStateModel | P0 |
| INT-058 | WorkflowService.getWorkflowDetails maps to GET details | Frontend call | Response parsed with approvers list | P0 |
| INT-059 | WorkflowService.getStageChangeHistory maps to GET history | Frontend call | Response parsed to WorkflowHistoryModel[] | P0 |
| INT-060 | WorkflowService.getRequirementsForStageChange maps correctly | Frontend call | StageRequirement[] returned | P0 |
| INT-061 | WorkflowService.submitForGoDecision handles multi-step responses | Submit flow | Handles RequiresConfirmation, RequiresAcknowledgment, Success | P0 |
| INT-062 | WorkflowService.cancelOpportunity sends correct payload | Cancel call | POST /api/workflow/cancel with EntityName, EntityId, Comment | P0 |
| INT-063 | WorkflowService.reopenOpportunity sends correct payload | Reopen call | POST /api/workflow/reopen with EntityName, EntityId, Comment | P0 |
| INT-064 | StageWorkflowComponent displays correct stages | Loaded stages | PrimeNG p-steps shows ordered stages | P0 |
| INT-065 | StageWorkflowComponent shows approvers when in workflow | IsInWorkflow=true | Approvers table visible with names/roles | P0 |
| INT-066 | StageWorkflowComponent shows history table | History loaded | Table with From/To stage, action, date, user | P0 |
| INT-067 | WorkflowComponent shows primary action button | Not in workflow, actions available | Button with first action's display name | P0 |
| INT-068 | WorkflowComponent shows Recall/Approve/Reject buttons | In workflow | Correct buttons based on CanApprove, CanRecall | P0 |
| INT-069 | RequirementsValidationComponent shows unmet requirements | RequirementsNotMet=true | Requirements panel with unmet items listed | P0 |
| INT-070 | Submit response mapping: confirmation dialogs | Multi-step responses | Frontend shows correct dialog for each confirmation type | P0 |

### Adapter Integration (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-071 | PaoWorkflowNotificationService CC recipients | GO notification | CC includes OM, initiator, Director/Manager | P1 |
| INT-072 | PaoWorkflowUserContext resolves current user | Any workflow action | User ID from ClaimsPrincipal | P0 |
| INT-073 | PaoWorkflowUserContext resolves org unit | User with org unit | Correct org unit resolved | P1 |
| INT-074 | WorkflowServiceExtensions registers all services | DI container | All workflow services resolvable | P0 |
| INT-075 | StateMachineStageChangeSeeder creates transitions | Database seed | 5 transitions seeded correctly | P0 |
| INT-076 | StateMachineStageChangeRoleSeeder assigns roles | Database seed | Role-based permissions seeded | P0 |
| INT-077 | IStageRequirementsProvider DI resolution | Multiple providers | Correct provider found for "Opportunity" | P0 |
| INT-078 | Multiple IStageRequirementsProvider instances | Future entity types | Each provider found by EntityNames match | P1 |
| INT-079 | PaoWorkflowApproverProvider.GetTriggerConfigurationAsync | Trigger check | Returns triggers for current transition | P0 |
| INT-080 | PaoWorkflowApproverProvider trigger includes OM | OM user | OM in triggers list for submit action | P0 |

### Cross-Entity Integration (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-081 | Opportunity CRUD unaffected by workflow | Create/Read/Update opportunity | Workflow fields initialized (Stage=I&P, WorkflowStatus=None) | P0 |
| INT-082 | Opportunity editing blocked during InWorkflow | WorkflowStatus=InWorkflow | UNOPSOpportunityManager.IsInWorkflow blocks edits | P0 |
| INT-083 | Opportunity editing allowed after recall | WorkflowStatus=None after recall | Editing enabled | P0 |
| INT-084 | Opportunity search includes workflow stage | Search/filter | Stage field available in search results | P1 |
| INT-085 | Opportunity list shows current stage | List view | Stage displayed for each opportunity | P1 |
| INT-086 | Home dashboard Actions Required card | Pending approvals | Card uses getPendingApprovalsForUser() | P1 |
| INT-087 | Opportunity detail page loads StageWorkflowComponent | Navigate to /opportunity/{id} | StageWorkflowComponent rendered | P0 |
| INT-088 | Opportunity detail page passes correct entityName/entityId | Component inputs | entityName="opportunity", entityId=valid | P0 |
| INT-089 | Opportunity permissions include canChangeStage | Permission endpoint | Permission flag available for workflow button | P0 |
| INT-090 | Workflow history accessible from opportunity detail | UI navigation | History section visible in stage workflow | P0 |

---

## §6 Security Tests (50)

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

| ID | Test Name | Security Concern | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| SEC-001 | Unauthenticated access to workflow stages | No IAP auth | 401 Unauthorized | P0 |
| SEC-002 | Unauthenticated submit attempt | No auth header | 401 Unauthorized | P0 |
| SEC-003 | Unauthenticated approve attempt | No auth header | 401 Unauthorized | P0 |
| SEC-004 | Unauthenticated reject attempt | No auth header | 401 Unauthorized | P0 |
| SEC-005 | Unauthenticated recall attempt | No auth header | 401 Unauthorized | P0 |
| SEC-006 | Unauthenticated cancel attempt | No auth header | 401 Unauthorized | P0 |
| SEC-007 | Unauthenticated reopen attempt | No auth header | 401 Unauthorized | P0 |
| SEC-008 | Unauthenticated history access | No auth header | 401 Unauthorized | P0 |
| SEC-009 | Unauthenticated details access | No auth header | 401 Unauthorized | P0 |
| SEC-010 | Unauthenticated requirements access | No auth header | 401 Unauthorized | P0 |
| SEC-011 | Non-OM cannot cancel opportunity | Collaborator user | 403 Forbidden | P0 |
| SEC-012 | Non-OM cannot reopen opportunity | Collaborator user | 403 Forbidden | P0 |
| SEC-013 | Non-approver cannot approve | User not in DoA list | 403 Forbidden | P0 |
| SEC-014 | Non-approver cannot reject | User not in DoA list | 403 Forbidden | P0 |
| SEC-015 | Non-submitter non-OM cannot recall | Random user | 403 Forbidden | P0 |
| SEC-016 | Self-approval blocked | Submitter = DoA holder | CanApprove=false, 403 on approve | P0 |
| SEC-017 | Self-rejection blocked | Submitter = DoA holder | Cannot reject own submission | P0 |
| SEC-018 | Submit only by authorized trigger users | Non-trigger user | AvailableActions empty for this user | P0 |
| SEC-019 | IDOR: access workflow of other user's opportunity | User A accesses User B's opportunity workflow | Proper access control (if implemented) | P0 |
| SEC-020 | SQL injection in entityName parameter | entityName="'; DROP TABLE--" | No SQL executed, 404 returned | P0 |
| SEC-021 | SQL injection in comment field | Comment="'; DROP TABLE--" | Comment stored as text, no SQL execution | P0 |
| SEC-022 | SQL injection in rationale field | Rationale="'; DELETE FROM--" | Rationale stored as text safely | P0 |
| SEC-023 | XSS in comment field | Comment="<script>alert(1)</script>" | Script not executed in history display | P0 |
| SEC-024 | XSS in rationale field | Rationale="<img onerror=alert(1)>" | Not rendered as HTML | P0 |
| SEC-025 | Path traversal in entityName | entityName="../../etc/passwd" | 404, no file access | P1 |
| SEC-026 | Integer overflow in entityId | EntityId=int.MaxValue+1 | Model binding error or 400 | P1 |
| SEC-027 | Malformed JSON body on submit | Invalid JSON | 400 Bad Request | P1 |
| SEC-028 | Missing required fields in submit body | EntityName missing | 400 / validation error | P1 |
| SEC-029 | Missing required fields in approve body | Rationale missing | 400 "Decision rationale is required" | P1 |
| SEC-030 | Extra fields in request body ignored | Extra "admin": true field | Extra fields not processed | P1 |
| SEC-031 | Rate limiting on submit endpoint | 100 rapid submits | Some form of rate limiting or throttling | P2 |
| SEC-032 | Rate limiting on approve endpoint | Rapid approve attempts | Throttling | P2 |
| SEC-033 | Token expiry during workflow action | Token expires mid-request | 401 on next request | P1 |
| SEC-034 | Workflow actions don't leak internal error details | Server throws | ProblemDetails without stack trace in production | P0 |
| SEC-035 | Approver list doesn't expose excessive user data | GET details | Only userId, name, email, role exposed | P1 |
| SEC-036 | History doesn't expose sensitive user data | GET history | Only userId, name, email, position, doaLevel | P1 |
| SEC-037 | Workflow state doesn't expose internal stage codes | GET state | Display names used where appropriate | P2 |
| SEC-038 | ExecutiveId validated to be real user | Fake ExecutiveId | Assignment fails or ignored | P1 |
| SEC-039 | Cannot submit for entity type user doesn't have access to | Future entity without permissions | Proper access control | P2 |
| SEC-040 | Cannot manipulate WorkflowStatus directly via API | Try to set WorkflowStatus via update endpoint | WorkflowStatus only changed by workflow actions | P0 |
| SEC-041 | CSRF protection on POST endpoints | No CSRF token | Protected by IAP auth scheme | P1 |
| SEC-042 | Content-Type validation on POST endpoints | Sending text/plain | 415 Unsupported Media Type or proper handling | P2 |
| SEC-043 | Large payload on submit (DoS attempt) | 10MB JSON body | Request rejected or handled gracefully | P2 |
| SEC-044 | Concurrent approve attempts from different users | Two DoA2 holders approve | Only first succeeds, second gets "no pending" | P0 |
| SEC-045 | Workflow log immutability | Try to modify/delete log entries | No API to modify history, logs are append-only | P0 |
| SEC-046 | UserId in log matches authenticated user | Spoofed UserId in request | Server uses CurrentUserId from auth, not request | P0 |
| SEC-047 | OM role check is exact match | EntityRole.Name="Opportunity Manager (temp)" | Exact match "Opportunity Manager" required | P1 |
| SEC-048 | DoA role code check is exact prefix match | EntityRole.Code="DoA2_Custom" | Code.StartsWith("DoA") check used | P1 |
| SEC-049 | Workflow operations audit-trailed | Any workflow action | Action logged with user, timestamp, entity | P0 |
| SEC-050 | Deleted opportunities cannot have workflow actions | IsDeleted=true | All endpoints return 404 | P0 |

---

## §7 Concurrency Tests (25)

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Test Name | Concurrent Scenario | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| CON-001 | Two users submit same opportunity simultaneously | Both POST /submit | One succeeds, other gets "already in workflow" | P0 |
| CON-002 | Approve and reject same workflow simultaneously | DoA2-A approves, DoA2-B rejects | One succeeds, other gets "no pending workflow" | P0 |
| CON-003 | Approve and recall same workflow simultaneously | DoA2 approves, OM recalls | One succeeds, other fails | P0 |
| CON-004 | Submit while entity is being updated | Edit + submit race | Either submit sees consistent data or proper error | P0 |
| CON-005 | Cancel while submit is processing | OM cancels during submit | One action wins, consistent state | P0 |
| CON-006 | Two OMs recall simultaneously | Both POST /recall | One succeeds, other gets "no pending" | P1 |
| CON-007 | Reopen while another user is viewing | OM reopens, viewer refreshes | Viewer sees updated stage | P1 |
| CON-008 | Rapid submit-recall-submit cycle | OM submits, recalls, submits again quickly | Each transition creates proper log, no orphaned states | P0 |
| CON-009 | Concurrent requirements check and submit | GET requirements during submit | Both return consistent data | P1 |
| CON-010 | Concurrent history read during approve | GET history while approve processing | History eventually consistent | P1 |
| CON-011 | Multiple SaveChangesAsync in reject don't interleave | Reject updates stage + logs | Both writes complete atomically or sequentially | P0 |
| CON-012 | WorkflowStatus update is thread-safe | InWorkflow flag toggled | Final state is consistent (InWorkflow or None) | P0 |
| CON-013 | Concurrent access to PendingTask | Multiple readers | PendingTask returns same result | P1 |
| CON-014 | Database connection pool under workflow load | 50 concurrent workflow operations | No connection pool exhaustion | P1 |
| CON-015 | Entity deleted during pending approval | Soft-delete while pending | Approve/reject handles missing entity gracefully | P0 |
| CON-016 | OM role removed during pending approval | Role changed while pending | Recall check uses current roles | P1 |
| CON-017 | DoA2 role removed during pending approval | Role removed after submit | Approve fails (user no longer has permission) | P1 |
| CON-018 | Executive assignment concurrent with approve | Two approves with different executives | First wins, single executive assigned | P1 |
| CON-019 | Notification send during concurrent actions | Submit sends notifications while recall | No duplicate or orphaned notifications | P2 |
| CON-020 | Concurrent access to workflow stages (read-only) | 100 simultaneous GET stages | All return same 4 stages consistently | P1 |
| CON-021 | Database timeout during submit | Slow DB query | Proper timeout error, no partial state | P1 |
| CON-022 | Concurrent submit for different opportunities | Two different opportunities | Both succeed independently | P0 |
| CON-023 | Retry after transient DB failure | Temporary connection issue | Retry succeeds, data consistent | P2 |
| CON-024 | Concurrent approve creates single log entry | Race condition on log | Only one completed log entry, not duplicated | P1 |
| CON-025 | Concurrent statement regeneration during submit | GeminiManager called concurrently | Statement generated once, no corruption | P2 |

---

## §8 Unit Tests (21)

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

| ID | Test Name | Unit Under Test | Verification | Priority |
|----|-----------|----------------|--------------|----------|
| UNT-001 | OpportunityWorkflow.Stages constants correct | Stage constants | IdentifyAndProfile, Go, NoGo, Cancelled values match expected | P0 |
| UNT-002 | OpportunityWorkflow.AllStages contains 4 stages | AllStages array | Length=4, all expected values present | P0 |
| UNT-003 | OpportunityWorkflow.IsValidStage true for valid | "GO" | Returns true | P0 |
| UNT-004 | OpportunityWorkflow.IsValidStage false for invalid | "INVALID" | Returns false | P0 |
| UNT-005 | OpportunityWorkflow.IsValidStage false for null | null | Returns false | P0 |
| UNT-006 | OpportunityWorkflow.IsValidStage false for empty | "" | Returns false | P0 |
| UNT-007 | OpportunityWorkflow.StateMachine has 4 states | StateMachine property | States.Count = 4 | P0 |
| UNT-008 | OpportunityWorkflow.StateMachine EntityType is "Opportunity" | StateMachine.EntityType | Equals "Opportunity" | P0 |
| UNT-009 | StateMachine states ordered by sequence | States | Sequences are 1,2,3,4 | P0 |
| UNT-010 | NormalizeEntityNameForWorkflow handles "opportunity" | Input lowercase | Returns "Opportunity" | P0 |
| UNT-011 | NormalizeEntityNameForWorkflow handles mixed case | "OPPORTUNITY" | Returns "Opportunity" | P0 |
| UNT-012 | NormalizeEntityNameForWorkflow passes unknown through | "contract" | Returns "contract" unchanged | P0 |
| UNT-013 | GetStateMachine returns machine for "opportunity" | entityName="opportunity" | Returns OpportunityWorkflow.StateMachine | P0 |
| UNT-014 | GetStateMachine returns null for unknown | entityName="partner" | Returns null | P0 |
| UNT-015 | WorkflowSubmitRequest model binds correctly | JSON deserialization | All properties populated | P0 |
| UNT-016 | ApproveWorkflowRequest model binds correctly | JSON deserialization | Rationale, ConfirmationAcknowledged, ExecutiveId | P0 |
| UNT-017 | RejectWorkflowRequest model binds correctly | JSON deserialization | Rationale, ConfirmationAcknowledged | P0 |
| UNT-018 | WorkflowStateResponse initializes AvailableActions | Default construction | AvailableActions is empty list (not null) | P0 |
| UNT-019 | WorkflowDetailsResponse initializes Approvers | Default construction | Approvers is empty list (not null) | P0 |
| UNT-020 | OpportunityStageRequirementsProvider.EntityNames | Property | Contains "Opportunity" | P0 |
| UNT-021 | OpportunityStageRequirementsProvider.GetRequirementsForStageChange returns 21 for GO | I&P → GO | Returns 21 requirements | P0 |

---

## §9 Performance Tests (16)

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Test Name | Performance Scenario | Target | Priority |
|----|-----------|---------------------|--------|----------|
| PRF-001 | GetWorkflowStages response time | GET stages | < 100ms | P0 |
| PRF-002 | GetWorkflowState response time | GET state with actions | < 500ms | P0 |
| PRF-003 | GetWorkflowDetails response time | GET details with approvers | < 500ms | P0 |
| PRF-004 | GetRequirements response time | GET requirements for GO | < 200ms | P0 |
| PRF-005 | Submit response time (full GO flow) | POST submit with all confirmations | < 2s | P0 |
| PRF-006 | Approve response time | POST approve | < 1s | P0 |
| PRF-007 | Reject response time | POST reject | < 1s | P0 |
| PRF-008 | Recall response time | POST recall | < 500ms | P0 |
| PRF-009 | Cancel response time | POST cancel | < 500ms | P1 |
| PRF-010 | Reopen response time | POST reopen | < 500ms | P1 |
| PRF-011 | History response time with 50+ entries | GET history | < 1s | P1 |
| PRF-012 | Submit split queries vs single Cartesian | Compare query strategies | Split queries 60-80% faster | P1 |
| PRF-013 | AsNoTracking impact on read queries | With vs without | 5-15% improvement with AsNoTracking | P2 |
| PRF-014 | Requirements validation with all 21 checks | Server-side validation | < 200ms for all 21 checks | P1 |
| PRF-015 | Statement regeneration impact on submit time | GeminiManager call | Timeout handling (no indefinite block) | P1 |
| PRF-016 | Parallel initial validation checks in Submit | entityValid + currentStage + pendingTask | Parallel faster than sequential | P2 |

---

## §10 Load Tests (10)

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Test Name | Load Scenario | Target | Priority |
|----|-----------|---------------|--------|----------|
| LDT-001 | 50 concurrent GET stages requests | Simultaneous reads | All return 200, < 200ms avg | P0 |
| LDT-002 | 50 concurrent GET state requests | Different opportunities | All return 200, < 1s avg | P0 |
| LDT-003 | 20 concurrent submit requests (different opportunities) | Parallel submissions | All succeed independently | P0 |
| LDT-004 | 10 concurrent approve requests (different workflows) | Parallel approvals | All succeed independently | P0 |
| LDT-005 | 100 concurrent history requests | Read-heavy load | All return 200, no timeouts | P1 |
| LDT-006 | Sustained workflow operations over 10 minutes | Mixed submit/approve/recall | No memory leaks, stable response times | P1 |
| LDT-007 | Database connection pool under 50 concurrent workflow operations | Heavy DB usage | No connection pool exhaustion | P1 |
| LDT-008 | 200 concurrent GET requirements requests | Read-heavy load | All return 200, < 500ms avg | P1 |
| LDT-009 | Submit under high DB load | Other heavy queries running | Timeout graceful handling, no data corruption | P2 |
| LDT-010 | Workflow operations with 1000+ history entries | Large history dataset | History endpoint still performs < 2s | P2 |

---

## Appendix A: Workflow Stage Transitions Reference

| From Stage | To Stage | Action | Approval Required | Who Can Trigger | Comment |
|-----------|----------|--------|-------------------|----------------|---------|
| IDENTIFY & PROFILE | GO | Submit for Go Decision | Yes (DoA2/DoA3) | OM / Stakeholders with trigger permission | 21 requirements validated, multi-step confirmation |
| IDENTIFY & PROFILE | NO GO | Submit for No Go | Yes (DoA2/DoA3) | OM / Stakeholders | Requires approval |
| IDENTIFY & PROFILE | CANCELLED | Cancel | No | OM only | Mandatory comment, not allowed while in workflow |
| NO GO | IDENTIFY & PROFILE | Reopen | No | OM only | Optional comment |
| CANCELLED | IDENTIFY & PROFILE | Reopen | No | OM only | Mandatory comment |

## Appendix B: GO Transition 21 Requirements

| # | Requirement | Field | Type | Section |
|---|------------|-------|------|---------|
| 1 | Opportunity Name | name | Text | Overview |
| 2 | Description | description | Text | Overview |
| 3 | Proposed Budget | initiativeBudgetUSD | Number (>0) | Overview |
| 4 | Products & Services | deliverables | Array (≥1) | What |
| 5 | Context & Challenges | challenges | Text | Why |
| 6 | Expected Impact | expectedImpact | Text | Why |
| 7 | Expected Outcomes | expectedOutcomes | Text | Why |
| 8 | Beneficiaries | conditional | TBD=true OR Direct>0+Indirect≥0 | Why |
| 9 | SDG Alignment | sdgs | Array (≥1) | Why |
| 10 | Strategic Missions | unopsMissions | Array (≥1) unless NotApplicable | Why |
| 11 | Funding Partners | fundingPartners | Array (≥1) | Who |
| 12 | Client Partners | clientPartners | Array (≥1) | Who |
| 13 | Countries | countries | Array (≥1) | Where |
| 14 | Target Signing Date | targetSigningDate | Date | When |
| 15 | Implementation Start | implementationStartDate | Date | When |
| 16 | Implementation End | targetDeliveryDate | Date | When |
| 17 | Opportunity Statement | opportunityStatementMarkdown | Text | Statement |
| 18 | Opportunity Manager | stakeholders (role) | Role check | Team |
| 19 | Responsible Org Unit | responsibleOrgUnitId | Select | Team |
| 20 | Proposed Initiative Type | proposedInitiativeTypeId | Select | Team |
| 21 | DoA Holder (server-only) | doaHolders | DoA2/DoA3 check | Team |

## Appendix C: GO Submission Multi-Step Confirmation Flow

```
Step 1: Requirements Validation (21 checks)
  ↓ All met
Step 2: Non-OM Submitter Warning (if applicable)
  ↓ Confirmed or skipped (user is OM)
Step 3: Country-OrgUnit Mismatch Warning (if applicable)
  ↓ Confirmed or skipped (all countries mapped)
Step 4: Mandatory Acknowledgment Statement
  ↓ Acknowledged
Step 5: Opportunity Statement Regeneration (AI)
  ↓ Completes or fails gracefully
Step 6: Workflow Initiation (pending approval created)
```
