# Task 3.0 Completion Report: Pending Approvals API & Executive Lookup

## Summary

Successfully created two lookup APIs:
1. **Pending workflow approvals endpoint** for current user (supports Actions Required card on dashboard)
2. **Executive lookup endpoint** for opportunity org unit (supports Approve dialog dropdown)

## Completed Subtasks

### 3.1 Create `PendingApprovalResponse` model ✅

**File:** `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`

```csharp
/// <summary>
/// Response model for pending workflow approval items.
/// Used by the Actions Required card on the home dashboard.
/// </summary>
public class PendingApprovalResponse
{
    public required string EntityName { get; set; }
    public int EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? CurrentStage { get; set; }
    public string? CurrentStageDisplayName { get; set; }
    public string? PendingStage { get; set; }
    public string? PendingStageDisplayName { get; set; }
    public string? SubmittedBy { get; set; }
    public int? SubmittedByUserId { get; set; }
    public DateTime? SubmittedOn { get; set; }
    public string? OrgUnitName { get; set; }
    public string? EntityUrl { get; set; }
}
```

### 3.2 Add `GetAllPendingTasksAsync()` method to workflow infrastructure ✅

**Files Updated:**
- `UNOPS.Workflow/UNOPS.Workflow.Business/Interfaces/IWorkflowRepository.cs` - Added interface method
- `UNOPS.Workflow/UNOPS.Workflow.DataAccess/WorkflowRepository.cs` - Added implementation
- `UNOPS.Workflow/UNOPS.Workflow.Integration/SampleWorkflowRepository.cs` - Added implementation
- `UNOPS.Workflow/UNOPS.Workflow.Business/Interfaces/IWorkflowManager.cs` - Added interface method
- `UNOPS.Workflow/UNOPS.Workflow.Business/Managers/WorkflowManager.cs` - Added implementation

```csharp
// IWorkflowRepository.cs
Task<IEnumerable<WorkflowLog>> GetAllPendingWorkflowLogsAsync();

// IWorkflowManager.cs
Task<IEnumerable<WorkflowLog>> GetAllPendingTasksAsync();
```

### 3.3 Create pending approvals endpoint in `WorkflowController` ✅

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

**Route:** `GET /api/workflow/pending-approvals`

```csharp
/// <summary>
/// Gets pending workflow approval tasks for the current user.
/// Returns only tasks where the current user is authorized to approve.
/// </summary>
[HttpGet(APIDictionary.Workflow + "/pending-approvals")]
public async Task<ActionResult<IEnumerable<PendingApprovalResponse>>> GetPendingApprovals()
{
    // 1. Get all pending workflow tasks
    // 2. For each, check if current user can approve
    // 3. Enrich with entity details (name, org unit, submitter)
    // 4. Return filtered list sorted by date
}
```

### 3.4 Add `GetExecutivesForOpportunityAsync()` method to OpportunityManager ✅

**Files Updated:**
- `UNOPS.PAO.Business/Interfaces/IOpportunityManager.cs` - Added interface method
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - Added implementation
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` - Added implementation

```csharp
/// <summary>
/// Gets executives (Director/Manager/OiC) for an opportunity's responsible org unit.
/// </summary>
Task<IEnumerable<TypeaheadInput>> GetExecutivesForOpportunityAsync(int opportunityId);
```

**Director Role Codes Supported:**
- `OrgUnit_Director_OrganizationHierarchy`
- `OrgUnit_Deputy_Director_OrganizationHierarchy`
- `Regional_Director_OrganizationHierarchy`
- `Regional_Deputy_Director_OrganizationHierarchy`
- `MCO_Director_OrganizationHierarchy`
- `MCO_Deputy_Director_OrganizationHierarchy`

### 3.5 Create executive lookup endpoint in `OpportunityController` ✅

**File:** `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs`

**Route:** `GET /api/opportunity/{id}/executives`

```csharp
/// <summary>
/// Gets executives (Director/Manager/OiC) for an opportunity's responsible org unit.
/// Used to populate the Executive dropdown in the Go Decision approval dialog.
/// </summary>
[HttpGet(APIDictionary.Opportunity + "/{id}/executives")]
[AccessControlled(EntityTypes.Opportunity, "read")]
public async Task<ActionResult> GetExecutives(int id)
```

### 3.6 Add unit tests for pending approvals endpoint ✅

**File:** `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs`

Added tests:
- `GetPendingApprovals_ReturnsPendingApprovalsForCurrentUser`
- `GetPendingApprovals_ReturnsEmptyList_WhenNoPendingApprovals`
- `GetPendingApprovals_FiltersTasksUserCannotApprove`

### 3.7 Add unit tests for executive lookup endpoint ✅

**File:** `QA Tests/Integration Tests/Controllers/OpportunityControllerExecutivesTests.cs` (NEW)

Added tests:
- `GetExecutives_ReturnsExecutivesForValidOpportunity`
- `GetExecutives_ReturnsEmptyList_WhenNoExecutivesAssigned`
- `GetExecutives_Returns404_ForNonExistentOpportunity`
- `GetExecutives_DirectorMarkedAsSuggested`

### 3.8 Verify builds ✅

- Main server project (`UNOPS.PAO.Server.csproj`) builds successfully with no errors
- Note: Integration test project has a pre-existing compilation issue with `Facing` enum namespace (unrelated to these changes)

## Files Modified/Created

| File | Action | Description |
|------|--------|-------------|
| `UNOPS.PAO.Models/Workflow/WorkflowModels.cs` | MODIFIED | Added `PendingApprovalResponse` model |
| `UNOPS.Workflow/.../IWorkflowRepository.cs` | MODIFIED | Added `GetAllPendingWorkflowLogsAsync()` interface method |
| `UNOPS.Workflow/.../WorkflowRepository.cs` | MODIFIED | Added `GetAllPendingWorkflowLogsAsync()` implementation |
| `UNOPS.Workflow/.../SampleWorkflowRepository.cs` | MODIFIED | Added `GetAllPendingWorkflowLogsAsync()` implementation |
| `UNOPS.Workflow/.../IWorkflowManager.cs` | MODIFIED | Added `GetAllPendingTasksAsync()` interface method |
| `UNOPS.Workflow/.../WorkflowManager.cs` | MODIFIED | Added `GetAllPendingTasksAsync()` implementation |
| `UNOPS.PAO.Business/Interfaces/IOpportunityManager.cs` | MODIFIED | Added `GetExecutivesForOpportunityAsync()` interface method |
| `UNOPS.PAO.Business/Managers/OpportunityManager.cs` | MODIFIED | Added `GetExecutivesForOpportunityAsync()` and `GetExecutivesForOrgUnitAsync()` implementations |
| `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` | MODIFIED | Added `GetExecutivesForOpportunityAsync()` and `GetExecutivesForOrgUnitAsync()` implementations |
| `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | MODIFIED | Added `GetPendingApprovals()` endpoint |
| `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs` | MODIFIED | Added `GetExecutives()` endpoint |
| `QA Tests/.../WorkflowControllerTests.cs` | MODIFIED | Added pending approvals tests |
| `QA Tests/.../OpportunityControllerExecutivesTests.cs` | CREATED | New test file for executives endpoint |

## API Endpoints

### GET /api/workflow/pending-approvals

Returns pending workflow approvals for the current user.

**Response:**
```json
[
  {
    "entityName": "Opportunity",
    "entityId": 123,
    "entityDisplayName": "New Partnership Initiative",
    "currentStage": "IDENTIFY & PROFILE",
    "currentStageDisplayName": "Identify & Profile",
    "pendingStage": "GO",
    "pendingStageDisplayName": "Go",
    "submittedBy": "John Smith",
    "submittedByUserId": 10,
    "submittedOn": "2026-02-01T10:30:00Z",
    "orgUnitName": "Europe Regional Office",
    "entityUrl": "/opportunity/123"
  }
]
```

### GET /api/opportunity/{id}/executives

Returns executives for an opportunity's responsible org unit.

**Response:**
```json
[
  {
    "label": "John Director (Director)",
    "value": "10",
    "description": "Suggested"
  },
  {
    "label": "Jane Deputy (Deputy Director)",
    "value": "11",
    "description": null
  }
]
```

## Key Features

### Pending Approvals
- Queries all pending workflow logs from database
- Filters to only tasks where current user can approve (via `CanUserApproveAsync`)
- Enriches response with entity details (display name, org unit, submitter name)
- Sorted by submission date (most recent first)

### Executive Lookup
- Queries `EntityUserRole` for Director/Deputy Director roles on org unit
- Supports 6 different director role types
- Directors marked as "Suggested" for UI default selection
- Results sorted: Directors first, then by name

## Dependencies

- Depends on Task 2.0 completion (enhanced approval endpoints)
- Depends on existing `IPaoWorkflowApproverProvider.CanUserApproveAsync()` method

## Next Steps

Task 4.0: Backend - In-System Notifications Integration
- Add `NotificationManager` dependency to `PaoWorkflowNotificationService`
- Create in-system notifications when approval is requested
- Mark notifications as done when decision is made
