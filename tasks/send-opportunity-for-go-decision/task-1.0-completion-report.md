# Task 1.0 Completion Report: Backend: Workflow Stage Infrastructure (CANCELLED Stage)

**Completed:** 2026-01-29

---

## Summary

Successfully added the CANCELLED stage to the opportunity workflow infrastructure. This enables Opportunity Managers to cancel opportunities from the "IDENTIFY & PROFILE" stage and later reopen them if needed.

---

## Files Modified

| File | Changes |
|------|---------|
| `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` | Added CANCELLED stage constant, State definition, and updated AllStages array |
| `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs` | Added Cancel and Reopen from CANCELLED transitions |
| `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeRoleSeeder.cs` | Added OM-only role permissions for Cancel and Reopen from CANCELLED |
| `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/StateMachineStageChangeSeederTests.cs` | Added tests for new transitions, updated existing test counts |

---

## Implementation Details

### OpportunityWorkflow.cs

Added new stage constant:
```csharp
public const string Cancelled = "CANCELLED";
```

Added State definition:
```csharp
new State 
{ 
    Sequence = 4, 
    StageCode = Stages.Cancelled, 
    DisplayName = Stages.Cancelled,
    Facing = Facing.Internal 
}
```

### StateMachineStageChangeSeeder.cs

Added 2 new transitions (total now 5):

1. **Cancel Transition** (IDENTIFY & PROFILE → CANCELLED)
   - `ApprovalRequired = false` - No approval needed
   - `CommentRequired = true` - Mandatory justification
   - `Name = "Cancel"`

2. **Reopen from CANCELLED** (CANCELLED → IDENTIFY & PROFILE)
   - `ApprovalRequired = false` - No approval needed
   - `CommentRequired = true` - Mandatory reason
   - `Name = "Reopen"`

### StateMachineStageChangeRoleSeeder.cs

Added 2 new role permissions (OM-only):

1. **Cancel**: Only Opportunity Manager can trigger
2. **Reopen from CANCELLED**: Only Opportunity Manager can trigger

Also renamed existing "Reopen" permission to "Reopen from No Go" for clarity.

---

## Workflow State Transitions (After)

```
IDENTIFY & PROFILE ─────► GO (requires DoA2 approval)
        │
        ├───────────────► NO GO (requires DoA2 approval)
        │
        └───────────────► CANCELLED (OM only, no approval)

NO GO ──────────────────► IDENTIFY & PROFILE (Reopen, OM only)

CANCELLED ──────────────► IDENTIFY & PROFILE (Reopen, OM only)
```

---

## Unit Tests Added

| Test Name | Description |
|-----------|-------------|
| `ShouldCreateFiveTransitions` | Updated from 3 to verify 5 transitions are seeded |
| `ShouldCreateCancelTransition` | Verifies Cancel transition is seeded correctly |
| `ShouldCreateReopenFromCancelledTransition` | Verifies Reopen from CANCELLED is seeded correctly |
| `ShouldBeIdempotent` | Updated to expect 5 transitions |

---

## Key Design Decisions

1. **CommentRequired = true for Cancel**: Per PRD, cancellation requires mandatory justification
2. **CommentRequired = true for Reopen from CANCELLED**: Per PRD, reopening from CANCELLED requires mandatory reason (unlike Reopen from NO GO which is optional)
3. **OM-only permissions**: Both Cancel and Reopen from CANCELLED can only be triggered by Opportunity Manager
4. **No approval required**: These are OM-driven actions that don't need DoA2 approval

---

## Notes for Future Tasks

- The CANCELLED stage will be used in Task 4.0 (WorkflowController) for the Cancel and Reopen action handlers
- The Cancel transition will set `Status = EntityStatus.Closed` (handled in controller, not seeder)
- The Reopen from CANCELLED transition will set `Status = EntityStatus.Active` (handled in controller)
- Frontend (Task 7.0) will need to show Cancel/Reopen buttons based on stage and user role

---

## Verification Checklist

- [x] Stage constants use UPPERCASE format consistently
- [x] All transitions use constants from `OpportunityWorkflow.Stages`
- [x] Role permissions are OM-only for Cancel and both Reopen transitions
- [x] Unit tests cover all new transitions
- [x] No linting errors
- [x] Seeders remain idempotent (safe to run multiple times)
