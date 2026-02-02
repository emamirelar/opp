# Task 3.0 Completion Report: Backend: DoA Level 2 Approver Lookup

**Completed on:** 2026-01-29  
**Status:** ✅ COMPLETED

---

## Summary

Updated `PaoWorkflowApproverProvider` to look up DoA Level 2 holders from the opportunity's `ResponsibleOrgUnit` via `EntityUserRole` instead of using stakeholder-based lookup for GO transitions.

---

## Changes Made

### 1. Modified `PaoWorkflowApproverProvider.cs`

**File:** `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs`

**Changes:**
- Added `Microsoft.Extensions.Logging` import for logging support
- Added constant `DoA2RoleCode = "DoA2_OrganizationHierarchy"` for role lookup
- Added optional `ILogger<PaoWorkflowApproverProvider>` to constructor
- Updated `GetOpportunityApproversAsync()` to route GO transitions to DoA2 lookup
- Added new helper methods:
  - `GetDoA2HoldersForOpportunityAsync()` - Gets opportunity's ResponsibleOrgUnitId and calls DoA2 lookup
  - `GetDoA2HoldersForOrgUnitAsync()` - Queries `EntityUserRole` for DoA2 holders on org unit
  - `GetStakeholderApproversAsync()` - Original stakeholder-based lookup (for non-GO transitions)
  - `GetDoA2HolderTasksForOpportunityAsync()` - Task model version for approval configuration
- Updated `GetOpportunityApproverTasksAsync()` to also handle GO transition with DoA2 lookup
- Added `AsNoTracking()` for read-only queries (performance optimization)
- Added warning logging when:
  - Opportunity not found
  - Opportunity has no ResponsibleOrgUnitId
  - No DoA2 holders found for org unit

**Key Logic:**
```csharp
// For GO transition, use DoA Level 2 holders from ResponsibleOrgUnit
if (toStage == OpportunityWorkflow.Stages.Go)
{
    return await GetDoA2HoldersForOpportunityAsync(opportunityId, toStage);
}

// For other transitions, use stakeholder-based lookup
return await GetStakeholderApproversAsync(opportunityId, roleNames, toStage);
```

**DoA2 Lookup Query:**
```csharp
var doaHolders = await _context.Set<EntityUserRole>()
    .AsNoTracking()
    .Include(e => e.EntityRole)
    .Include(e => e.User)
        .ThenInclude(u => u!.UserProfile)
    .Where(e => !e.IsDeleted &&
               e.EntityType == "OrganizationHierarchy" &&
               e.EntityId == orgUnitId &&
               e.EntityRole != null &&
               e.EntityRole.Code == DoA2RoleCode)
    .ToListAsync();
```

### 2. Updated Unit Tests

**File:** `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs`

**New Test Methods Added:**
1. `GetApproversAsync_GoTransition_ReturnsDoA2HoldersFromEntityUserRole` - Verifies DoA2 lookup returns correct users
2. `GetApproversAsync_GoTransition_WithNoResponsibleOrgUnit_ReturnsEmptyList` - Handles null ResponsibleOrgUnitId
3. `GetApproversAsync_GoTransition_WithNoDoA2Holders_ReturnsEmptyList` - Handles org unit without DoA2 holders
4. `GetApprovalConfigurationAsync_GoTransition_ReturnsDoA2Configuration` - Tests approval task creation
5. `CanUserApproveAsync_GoTransition_WithDoA2Holder_ReturnsTrue` - DoA2 holder can approve
6. `CanUserApproveAsync_GoTransition_WithNonDoA2User_ReturnsFalse` - Non-DoA2 user cannot approve
7. `GetApproversAsync_GoTransition_WithMultipleDoA2Holders_ReturnsAll` - Multiple approvers supported
8. `GetApproversAsync_GoTransition_ExcludesDeletedEntityUserRoles` - Soft delete respected

**New Test Data Seeding Methods:**
- `SeedDoA2TestDataAsync()` - Seeds complete DoA2 test scenario with org unit, users, roles, and EntityUserRole
- `SeedOpportunityWithoutOrgUnitAsync()` - Seeds opportunity without ResponsibleOrgUnitId for edge case testing

---

## Technical Details

### Entity Relationships

```
Opportunity
    └── ResponsibleOrgUnitId → OrganizationHierarchy.Id

EntityUserRole
    ├── EntityType = "OrganizationHierarchy"
    ├── EntityId → OrganizationHierarchy.Id
    ├── EntityRoleId → EntityRole.Id (where Code = "DoA2_OrganizationHierarchy")
    └── UserId → PAOUser.Id
```

### Role Code Used

- **DoA2 Role Code:** `DoA2_OrganizationHierarchy`
- This is the standard role code for DoA Level 2 holders assigned to organization hierarchies

### Approver Model Output

For GO transitions, approvers are returned with:
- `UserId`: The DoA2 holder's user ID
- `Name`: User's full name from UserProfile
- `Email`: User's email address
- `Role`: "DoA Level 2" (hardcoded display name)
- `ToStage`: "GO"

---

## Behavior Summary

| Transition | Approver Source | Lookup Method |
|------------|-----------------|---------------|
| IDENTIFY & PROFILE → GO | EntityUserRole (DoA2 on ResponsibleOrgUnit) | `GetDoA2HoldersForOrgUnitAsync` |
| Any → NO GO | OpportunityStakeholder (role-based) | `GetStakeholderApproversAsync` |
| Any → CANCELLED | N/A (no approval required) | N/A |
| CANCELLED → IDENTIFY & PROFILE | N/A (no approval required) | N/A |
| NO GO → IDENTIFY & PROFILE | N/A (no approval required) | N/A |

---

## Files Modified

| File | Action | Lines Changed |
|------|--------|---------------|
| `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs` | MODIFIED | +157 lines (new methods, logging) |
| `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs` | MODIFIED | +245 lines (new tests, seeding) |

---

## Verification

- ✅ No linter errors in modified files
- ✅ All new tests follow existing test patterns
- ✅ DoA2 lookup bypasses seeder role definitions (works independently)
- ✅ Empty list returned gracefully for edge cases
- ✅ Soft delete (IsDeleted) properly filtered
- ✅ AsNoTracking used for read-only queries

---

## Next Task

Ready to proceed with **Task 4.0: Backend: WorkflowController Endpoints & Custom Actions**.
