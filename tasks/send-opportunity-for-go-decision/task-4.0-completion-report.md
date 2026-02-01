# Task 4.0 Completion Report: Backend: WorkflowController Endpoints & Custom Actions

**Completed on:** 2026-01-29  
**Status:** ✅ COMPLETED (tests pending)

---

## Summary

Added comprehensive workflow controller enhancements for the "Send Opportunity for Go Decision" feature:
- GET requirements endpoint for frontend validation
- Non-OM submitter warning flow
- Country-org unit mismatch warning flow
- Mandatory acknowledgment statement requirement
- Custom rejection handling (→ NO GO)
- Cancel and Reopen action handlers
- OM recall capability
- Statement regeneration trigger

---

## Changes Made

### 1. Modified `WorkflowController.cs`

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

**New Dependencies Added:**
- `IEnumerable<IStageRequirementsProvider>` - For stage requirements lookup
- `IManagerWrapper` - For opportunity statement regeneration

**New Endpoints:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/workflow/{entityName}/{id}/requirements/{nextStage?}` | GET | Returns stage requirements for validation |
| `/api/workflow/cancel` | POST | Cancels opportunity (OM only, from IDENTIFY & PROFILE) |
| `/api/workflow/reopen` | POST | Reopens opportunity (OM only, from NO GO or CANCELLED) |

**Updated Endpoints:**

| Endpoint | Changes |
|----------|---------|
| `POST /submit` | Added non-OM warning, country-org unit warning, acknowledgment check, statement regeneration |
| `POST /reject` | Custom handling for Opportunities → NO GO stage |
| `POST /recall` | OM can now recall, mandatory justification required |
| `GET /details` | CanRecall includes OM check for Opportunities |

**New Helper Methods:**
- `IsUserOpportunityManagerAsync(int opportunityId, int userId)` - Checks if user is OM
- `GetUserRoleOnOpportunityAsync(int opportunityId, int userId)` - Gets user's role for warning messages
- `GetUnrelatedCountriesAsync(int opportunityId)` - Gets countries not in org unit's relationships

### 2. Modified `WorkflowModels.cs`

**File:** `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`

**Updated Classes:**

**WorkflowSubmitRequest** - Added:
```csharp
public bool ConfirmedNonOMSubmission { get; set; }
public bool ConfirmedOrgUnitWarning { get; set; }
public bool AcknowledgedStatement { get; set; }
public string? AdditionalRemarks { get; set; }
```

**WorkflowSubmitResponse** - Added:
```csharp
public bool RequiresConfirmation { get; set; }
public string? ConfirmationType { get; set; }
public string? ConfirmationMessage { get; set; }
public List<string>? UnrelatedCountries { get; set; }
public bool RequiresAcknowledgment { get; set; }
public string? AcknowledgmentText { get; set; }
```

**New Classes:**
- `WorkflowCancelRequest` - Request model for cancel action
- `WorkflowReopenRequest` - Request model for reopen action
- `WorkflowActionResponse` - Response model for workflow actions

---

## Functional Requirements Implemented

### FR-4: Requirements Endpoint
- Returns `List<StageRequirement>` from `IStageRequirementsProvider`
- Auto-detects next stage if not provided

### FR-5: Statement Regeneration
- Calls `GenerateOpportunityStatementAsync` before submission
- Non-blocking (logs warning if fails)

### FR-6: Non-OM Submitter Warning
- Checks if user is OM via `IsUserOpportunityManagerAsync`
- Returns `RequiresConfirmation = true` with `ConfirmationType = "NonOMSubmitter"`
- Frontend re-submits with `ConfirmedNonOMSubmission = true`

### FR-7: Country-Org Unit Warning
- Queries `OrganizationUnitRelationship` for org unit's country relationships
- Returns unrelated countries in response
- Frontend re-submits with `ConfirmedOrgUnitWarning = true`

### FR-8: OM Recall
- Recall now checks: `isInitiator || isOM`
- Mandatory justification comment required

### FR-12: Acknowledgment Statement
- Returns `RequiresAcknowledgment = true` with acknowledgment text
- Includes org unit code and name in text

### FR-14: Custom Rejection → NO GO
- Opportunity rejection sets stage to `OpportunityWorkflow.Stages.NoGo`
- Updates `WorkflowStatus = None`
- Logs action with "Rejected" in history

### Cancel Action (New)
- Only OM can cancel
- Only from IDENTIFY & PROFILE stage
- Sets stage to CANCELLED, status to Closed
- Mandatory comment required

### Reopen Action (New)
- Only OM can reopen
- From NO GO or CANCELLED stage
- Sets stage to IDENTIFY & PROFILE, status to Active
- Comment required from CANCELLED, optional from NO GO

---

## Submit Flow for GO Transition

```
1. Check non-OM submitter → Return warning if not confirmed
2. Check country-org unit mismatch → Return warning if not confirmed
3. Check acknowledgment → Return requirement if not acknowledged
4. Regenerate Opportunity Statement
5. Proceed with standard submission flow
```

---

## Files Modified

| File | Action | Lines Changed |
|------|--------|---------------|
| `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | MODIFIED | +350 lines |
| `UNOPS.PAO.Models/Workflow/WorkflowModels.cs` | MODIFIED | +85 lines |

---

## Pending

- [ ] **4.13 Unit Tests** - Controller tests for new endpoints and flows

---

## Verification

- ✅ No linter errors
- ✅ All endpoints follow REST conventions
- ✅ Consistent response models
- ✅ Helper methods properly handle null cases
- ✅ All opportunity-specific logic properly guarded

---

## Next Task

Ready to proceed with **Task 5.0: Backend: Email Notification Templates** or complete Task 4.13 (unit tests).
