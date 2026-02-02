# Task 1.0 Completion Report: Backend Data Model & Migration

## Summary
Added the `ExecutiveId` field to the Opportunity entity to store the assigned Executive (Director/Manager/OiC) when a Go decision is made. This field will be populated during the approval workflow by the decision-maker.

## Completed Subtasks

### 1.1 Add ExecutiveId field to Opportunity.cs entity ✅
**File Modified:** `UNOPS.PAO.Domain/Entities/Opportunity.cs`

Added:
```csharp
/// <summary>
/// The Executive assigned to direct Opportunity development after Go decision.
/// Set by the decision-maker during Go approval. Nullable until Go decision is made.
/// </summary>
public int? ExecutiveId { get; set; }

/// <summary>
/// Navigation property to the assigned Executive user.
/// </summary>
[ForeignKey(nameof(ExecutiveId))]
public virtual PAOUser? Executive { get; set; }
```

### 1.2 Configure ExecutiveId relationship in UNOPSAppDbContext ✅
**File Modified:** `UNOPS.PAO.UNOPSDataAccess/Context/UNOPSAppDbContext.cs`

Added new `#region Opportunity Configuration` with:
```csharp
modelBuilder.Entity<Opportunity>(entity =>
{
    // Executive assignment (set during Go decision)
    entity.HasOne(e => e.Executive)
          .WithMany()
          .HasForeignKey(e => e.ExecutiveId)
          .OnDelete(DeleteBehavior.SetNull); // Executive deletion shouldn't delete Opportunity
});
```

### 1.3 Generate EF Core migration ✅
**File Created:** `UNOPS.PAO.UNOPSDataAccess/Migrations/20260202161208_AddExecutiveIdToOpportunity.cs`

Migration includes:
- `ExecutiveId` nullable integer column on `Opportunities` table
- Index `IX_Opportunities_ExecutiveId` for query performance
- Foreign key `FK_Opportunities_AspNetUsers_ExecutiveId` with `SetNull` delete behavior
- Proper `Down()` method for rollback capability

### 1.4 Update OpportunityModel ✅
**File Modified:** `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs`

Added:
```csharp
/// <summary>
/// The Executive assigned to direct Opportunity development after Go decision.
/// Nullable until Go decision is made.
/// </summary>
public int? ExecutiveId { get; set; }

/// <summary>
/// Display name of the assigned Executive.
/// </summary>
public string? ExecutiveName { get; set; }
```

### 1.5 Update MappingProfile.cs ✅
**File Modified:** `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs`

Added to `CreateMap<Opportunity, OpportunityModel>()`:
```csharp
.ForMember(dest => dest.ExecutiveId, opt => opt.MapFrom(src => src.ExecutiveId))
.ForMember(dest => dest.ExecutiveName, opt => opt.MapFrom(src => 
    src.Executive != null && src.Executive.UserProfile != null 
        ? src.Executive.UserProfile.Name 
        : (src.Executive != null ? src.Executive.Email : null)));
```

### 1.6 Verify build succeeds ✅
Build completed successfully with no errors.

**Note:** The actual database update (`dotnet ef database update`) should be run against the target database when ready to deploy. The migration has been generated and verified.

## Files Modified/Created

| File | Action | Description |
|------|--------|-------------|
| `UNOPS.PAO.Domain/Entities/Opportunity.cs` | Modified | Added ExecutiveId field and Executive navigation property |
| `UNOPS.PAO.UNOPSDataAccess/Context/UNOPSAppDbContext.cs` | Modified | Added Opportunity → Executive relationship configuration |
| `UNOPS.PAO.UNOPSDataAccess/Migrations/20260202161208_AddExecutiveIdToOpportunity.cs` | Created | EF Core migration for ExecutiveId column |
| `UNOPS.PAO.UNOPSDataAccess/Migrations/20260202161208_AddExecutiveIdToOpportunity.Designer.cs` | Created | EF Core migration designer file |
| `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs` | Modified | Added ExecutiveId and ExecutiveName properties |
| `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs` | Modified | Added Executive mapping for API responses |

## Technical Notes for Future Tasks

1. **PAOUser Entity Location:** `UNOPS.PAO.Domain/Entities/PAOUser.cs`
   - Has `Id`, `Email`, `IsInternal`, and `UserProfile` navigation property
   - `Name` property returns `UserProfile.Name` or empty string

2. **Executive Display Name Pattern:** Following the same pattern as `CreatedByName` and `LastModifiedByName`:
   - Check `UserProfile.Name` first, then fall back to `Email`

3. **Delete Behavior:** `SetNull` ensures that if an Executive user is deleted, the Opportunity remains but with `ExecutiveId = null`

4. **Migration Naming Convention:** Follows existing pattern with timestamp prefix (YYYYMMDDHHMMSS)

## Dependencies for Next Tasks

This task enables:
- **Task 2.0:** `AssignExecutiveAsync()` method will use `ExecutiveId` to store the selected Executive
- **Task 3.0:** `GetExecutivesForOrgUnitAsync()` will populate the Executive dropdown
- **Task 7.0:** Frontend dialogs will use `ExecutiveId` and `ExecutiveName` for display

---
*Completed: February 2, 2026*
