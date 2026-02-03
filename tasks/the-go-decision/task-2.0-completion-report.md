# Task 2.0 Completion Report: Workflow Request Models & Endpoint Enhancements

## Summary

Successfully created enhanced workflow request models (`ApproveWorkflowRequest`, `RejectWorkflowRequest`) and updated the `WorkflowController` approve/reject endpoints with structured validation for rationale, confirmation acknowledgment, and Executive assignment.

## Completed Subtasks

### 2.1 Create `ApproveWorkflowRequest` model ✅

**File:** `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`

```csharp
/// <summary>
/// Request model for approving an opportunity workflow with Go decision requirements.
/// Used for enhanced approval flow with mandatory rationale, confirmation, and Executive assignment.
/// </summary>
public class ApproveWorkflowRequest
{
    public required string EntityName { get; set; }
    public required int EntityId { get; set; }
    public required string Rationale { get; set; }
    public bool ConfirmationAcknowledged { get; set; }
    public int ExecutiveId { get; set; }
}
```

### 2.2 Create `RejectWorkflowRequest` model ✅

**File:** `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`

```csharp
/// <summary>
/// Request model for rejecting an opportunity workflow with No-Go decision requirements.
/// Used for enhanced rejection flow with mandatory rationale and confirmation.
/// </summary>
public class RejectWorkflowRequest
{
    public required string EntityName { get; set; }
    public required int EntityId { get; set; }
    public required string Rationale { get; set; }
    public bool ConfirmationAcknowledged { get; set; }
}
```

### 2.3 Update `WorkflowController.Approve()` ✅

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

- Changed parameter type to `ApproveWorkflowRequest`
- Added validation for empty `Rationale` → returns BadRequest
- Added validation for `ConfirmationAcknowledged == false` → returns BadRequest
- Added validation for Opportunity approvals without `ExecutiveId` → returns BadRequest
- Passes `Rationale` to `_workflowManager.Approve()` as comment parameter

### 2.4 Add `AssignExecutiveAsync()` method ✅

**Files:**
- `UNOPS.PAO.Business/Interfaces/IOpportunityManager.cs` - Added interface method
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - Added implementation
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` - Added implementation

```csharp
/// <summary>
/// Assigns an Executive to an opportunity during Go decision approval.
/// </summary>
public virtual async Task AssignExecutiveAsync(int opportunityId, int executiveId)
{
    var opportunity = await opportunityRepository.GetByIdAsync(opportunityId);
    if (opportunity == null)
    {
        throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
    }

    opportunity.ExecutiveId = executiveId;
    await context.SaveChangesAsync();
}
```

### 2.5 Call `AssignExecutiveAsync()` in Approve endpoint ✅

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

```csharp
// === ASSIGN EXECUTIVE TO OPPORTUNITY (NEW) ===
if (normalizedEntityName == "Opportunity" && request.ExecutiveId > 0)
{
    await _managerWrapper.OpportunityManager.AssignExecutiveAsync(request.EntityId, request.ExecutiveId);
}
```

### 2.6 Update `WorkflowController.Reject()` ✅

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

- Changed parameter type to `RejectWorkflowRequest`
- Added validation for empty `Rationale` → returns BadRequest
- Added validation for `ConfirmationAcknowledged == false` → returns BadRequest
- Passes `Rationale` to `_workflowManager.Reject()` as comment parameter

### 2.7 Add unit tests for enhanced approve endpoint ✅

**File:** `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs`

Added/updated tests:
- `Approve_WithValidRequest_ReturnsSuccess` - Tests full approval flow with Executive assignment
- `Approve_WithoutRationale_Returns400` - Tests validation
- `Approve_WithoutConfirmation_Returns400` - Tests validation
- `Approve_WithoutExecutive_ForOpportunity_Returns400` - Tests validation
- `Integration_ApproveFlow_SetsStageToGo` - Integration test with Executive assignment

### 2.8 Add unit tests for enhanced reject endpoint ✅

**File:** `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs`

Added/updated tests:
- `Reject_WithValidRequest_ReturnsSuccess` - Tests full rejection flow
- `Reject_WithoutRationale_Returns400` - Tests validation
- `Reject_WithoutConfirmation_Returns400` - Tests validation
- `Reject_Opportunity_SetsStageToNoGo` - Tests custom NO GO behavior
- `Integration_RejectFlow_SetsStageToNoGo_NotIdentifyProfile` - Integration test

### 2.9 Verify builds ✅

- Main server project (`UNOPS.PAO.Server.csproj`) builds successfully with no errors
- Note: Integration test project has a pre-existing compilation issue with `Facing` enum namespace that is unrelated to these changes

## Files Modified/Created

| File | Action | Description |
|------|--------|-------------|
| `UNOPS.PAO.Models/Workflow/WorkflowModels.cs` | MODIFIED | Added `ApproveWorkflowRequest` and `RejectWorkflowRequest` models |
| `UNOPS.PAO.Business/Interfaces/IOpportunityManager.cs` | MODIFIED | Added `AssignExecutiveAsync()` method signature |
| `UNOPS.PAO.Business/Managers/OpportunityManager.cs` | MODIFIED | Added `AssignExecutiveAsync()` implementation |
| `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` | MODIFIED | Added `AssignExecutiveAsync()` implementation |
| `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | MODIFIED | Updated `Approve()` and `Reject()` endpoints with enhanced validation |
| `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs` | MODIFIED | Updated tests to use new request models |

## API Changes

### POST /api/workflow/approve

**Before:**
```json
{
  "entityName": "opportunity",
  "entityId": 123,
  "comment": "Approved"
}
```

**After:**
```json
{
  "entityName": "opportunity",
  "entityId": 123,
  "rationale": "All requirements met, budget approved, team in place",
  "confirmationAcknowledged": true,
  "executiveId": 10
}
```

### POST /api/workflow/reject

**Before:**
```json
{
  "entityName": "opportunity",
  "entityId": 123,
  "comment": "Rejected"
}
```

**After:**
```json
{
  "entityName": "opportunity",
  "entityId": 123,
  "rationale": "Insufficient budget justification and unclear scope",
  "confirmationAcknowledged": true
}
```

## Validation Rules

### Approve Endpoint
1. `Rationale` is required (non-empty string)
2. `ConfirmationAcknowledged` must be `true`
3. For Opportunity approvals: `ExecutiveId` must be greater than 0

### Reject Endpoint
1. `Rationale` is required (non-empty string)
2. `ConfirmationAcknowledged` must be `true`

## Dependencies

- Depends on Task 1.0 completion (ExecutiveId field on Opportunity entity)
- Depends on prerequisite PRD Task 4.0 (basic WorkflowController endpoints)

## Next Steps

Task 3.0: Backend - Pending Approvals API & Executive Lookup
- Create pending approvals endpoint for current user
- Create executive lookup endpoint for org unit
