# Workflow Submodule Integration - Implementation Tasks

## Relevant Files

**Backend Files (.NET Core) - NEW:**
- `UNOPS.PAO.Domain/Enums/WorkflowStatus.cs` - WorkflowStatus enum (None, InWorkflow)
- `UNOPS.PAO.Business/Workflow/Interfaces/IPaoWorkflowApproverProvider.cs` - Extended approver interface
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowUserContext.cs` - IWorkflowUserContext implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoEntityStageProvider.cs` - IEntityStageProvider implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs` - IPaoWorkflowApproverProvider implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` - IWorkflowNotificationService implementation
- `UNOPS.PAO.Business/Workflow/Adapters/WorkflowServiceExtensions.cs` - DI registration extension method
- `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` - Opportunity state machine definition (3 stages)
- `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs` - Stage transition seeder
- `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeRoleSeeder.cs` - Role permission seeder
- `UNOPS.PAO.Business/EmailTemplates/WorkflowApprovalRequest.html` - Email template
- `UNOPS.PAO.Business/EmailTemplates/WorkflowCompleted.html` - Email template
- `UNOPS.PAO.Business/EmailTemplates/WorkflowRejected.html` - Email template
- `UNOPS.PAO.Business/EmailTemplates/WorkflowRecalled.html` - Email template
- `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` - Workflow API endpoints (comprehensive)

**Backend Files (.NET Core) - MODIFY:**
- `UNOPS.PAO.Domain/Infrastructure/Audit/ModifiableDeletableEntity.cs` - Add WorkflowStatus property and IsInWorkflow computed property
- `UNOPS.PAO.Domain/Entities/Opportunity.cs` - Add Stage property, remove WorkflowStageId
- `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs` - Add Stage, WorkflowStatus, IsInWorkflow; remove WorkflowStageId/Name
- `UNOPS.PAO.Models/Opportunities/OpportunityListModel.cs` - Add Stage, WorkflowStatus, IsInWorkflow; remove WorkflowStageId/Name
- `UNOPS.PAO.Models/Opportunities/OpportunityRequest.cs` - Remove WorkflowStageId
- `UNOPS.PAO.Models/Opportunities/UpdateOpportunityRequest.cs` - Remove WorkflowStageId
- `UNOPS.PAO.Models/Opportunities/ApplyOpportunityAiChangesRequest.cs` - Remove WorkflowStageId
- `UNOPS.PAO.Models/Dashboard/DashboardModels.cs` - Replace WorkflowStageName with Stage
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - Integrate workflow methods (StartWorkflow, EndWorkflow, UpdateStageAsync)
- `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs` - Update mapping (remove WorkflowStageName, add Stage)
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` - Update WorkflowStageId references to use Stage
- `UNOPS.PAO.UNOPSBusiness/Managers/Mapping/OpportunityMappingProfile.cs` - Update mapping (remove WorkflowStageName, add Stage)
- `UNOPS.PAO.Server/Startup.cs` - Register workflow services in `ConfigureContainer()` method
- `UNOPS.PAO.Business/Interfaces/IManagerWrapper.cs` - Remove old IWorkflowManager property (submodule's IWorkflowManager injected via DI)
- `UNOPS.PAO.Business/Managers/ManagerWrapper.cs` - Remove old WorkflowManager instantiation and property
- `UNOPS.PAO.DataAccess/Context/AppDbContext.cs` - Remove WorkflowStage and WorkflowLog DbSets

**Backend Files (.NET Core) - DELETE:**
- `UNOPS.PAO.Domain/Entities/WorkflowStage.cs`
- `UNOPS.PAO.Domain/Entities/WorkflowLog.cs`
- `UNOPS.PAO.Business/Managers/WorkflowManager.cs`
- `UNOPS.PAO.Business/Interfaces/IWorkflowManager.cs`
- `UNOPS.PAO.Models/Workflow/` - Entire folder (submodule provides these in `UNOPS.Workflow.Models`)

**Backend - Unit Tests (NEW):**
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowUserContextTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoEntityStageProviderTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityWorkflowTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityWorkflowSeederTests.cs`
- `UNOPS.PAO.IntegrationTests/Controllers/WorkflowControllerTests.cs`

**Note on test patterns:** Follow existing test structure (see `UnitTests/Managers/UNOPSPartnerManagerTests.cs` for example)

**Frontend Files (Angular) - MODIFY:**
- `UNOPS.PAO.ClientApp/tsconfig.json` - Add @unops/workflow path alias
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts` - Integrate workflow
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.html` - Add workflow template
- `UNOPS.PAO.ClientApp/src/assets/i18n/en.json` - Add workflow translation keys

**Frontend Files (Angular) - DELETE (if exists, replaced by submodule):**
- `UNOPS.PAO.ClientApp/src/app/shared/components/workflows/` - Old workflow components (if any)
- `UNOPS.PAO.ClientApp/src/app/shared/services/domain/workflow.service.ts` - Old service (if any)

**Frontend - Unit Tests:**
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.spec.ts`

**Git Submodule:**
- `UNOPS.Workflow/` - Git submodule folder (entire directory)

### Notes

- Backend unit tests are in `UNOPS.PAO.IntegrationTests/UnitTests/` folder (NOT `UNOPS.PAO.Tests/`)
- Backend tests use xUnit, Moq, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory
- Frontend tests use Jasmine, TestBed, HttpTestingController
- PAO uses Lamar DI (ServiceRegistry is compatible with IServiceCollection extension methods)
- PAO does NOT have `ServiceExtensions.cs` - services are registered in `Startup.cs.ConfigureContainer()`
- PAO managers are instantiated directly in `ManagerWrapper.cs`, NOT via DI (except IWorkflowManager from submodule)
- Controllers inherit from `BaseController` and use `APIDictionary` for route constants
- Controllers use `[Route("/")]` at class level and `[HttpGet(APIDictionary.Xxx)]` for methods
- Angular components are in `features/partnerships/opportunities/` (not `features/opportunities/`)
- Mapping profiles are separate files (e.g., `OpportunityMappingProfile.cs`), not in main `MappingProfile.cs`
- Follow PAO coding standards and naming conventions
- **Use `UNOPS.Workflow.Models` (submodule) directly** - do NOT duplicate models locally
- **Opportunity Workflow**: 3 stages (IDENTIFY & PROFILE → GO/NO GO) with approval workflow

---

## ⚠️ CRITICAL Testing Requirements

### Testing Philosophy
All new code MUST have corresponding unit tests. Tests are not optional - they are mandatory for code quality and maintainability.

### Required Tools
- **Backend:** xUnit, Moq, Microsoft.EntityFrameworkCore.InMemory
- **Frontend:** Jasmine, TestBed, HttpTestingController

### Test Coverage Expectations
- All interface implementations must be tested
- All API endpoints must have integration tests
- State machine and seeder logic must be validated
- Edge cases (null values, invalid IDs, unauthorized users) must be covered

### Mandatory Verification Steps
Each unit test task MUST include:
1. Verify all tests compile without errors
2. Verify all tests run successfully
3. Verify no existing tests are broken
4. Verify test coverage meets minimum threshold (80%)

---

## Tasks

- [ ] 1.0 Project Setup & Submodule Integration
  - [ ] 1.1 Add UNOPS.Workflow Git submodule to repository root
    - Run: `git submodule add https://github.com/UNOPS-ITG/unops-workflow.git UNOPS.Workflow`
    - Verify submodule folder is created at `business-partners-and-opportunities/UNOPS.Workflow/`
  - [ ] 1.2 Add project references to UNOPS.PAO.Business.csproj
    - Add reference to `UNOPS.Workflow.Business` (for IWorkflowManager, interfaces)
    - Add reference to `UNOPS.Workflow.DataAccess` (for WorkflowDbContext)
    - Add reference to `UNOPS.Workflow.Models` (for StateMachine, State, Facing, DTOs)
    - Add reference to `UNOPS.Workflow.Domain` (for StateMachineStageChange entity used in seeders)
  - [ ] 1.3 Delete old PAO workflow files from Domain layer
    - Delete `UNOPS.PAO.Domain/Entities/WorkflowStage.cs`
    - Delete `UNOPS.PAO.Domain/Entities/WorkflowLog.cs`
    - Remove DbSet properties from `UNOPS.PAO.DataAccess/Context/AppDbContext.cs`
  - [ ] 1.4 Delete old PAO workflow files from Business layer
    - Delete `UNOPS.PAO.Business/Managers/WorkflowManager.cs`
    - Delete `UNOPS.PAO.Business/Interfaces/IWorkflowManager.cs`
    - Edit `UNOPS.PAO.Business/Interfaces/IManagerWrapper.cs`:
      * Remove `IWorkflowManager WorkflowManager { get; }` property
    - Edit `UNOPS.PAO.Business/Managers/ManagerWrapper.cs`:
      * Remove `private IWorkflowManager workflowManager;` field (line 19)
      * Remove `workflowManager = new WorkflowManager(context);` from constructor (line 48)
      * Remove `public virtual IWorkflowManager WorkflowManager => workflowManager;` property (line 85)
    - **Note:** The submodule's IWorkflowManager will be injected via DI, NOT through ManagerWrapper
  - [ ] 1.5 Delete PAO workflow models folder
    - Delete entire `UNOPS.PAO.Models/Workflow/` folder
    - **Reason:** The submodule provides these models in `UNOPS.Workflow.Models`
    - PAO should use `using UNOPS.Workflow.Models;` directly
    - Update any existing references from `UNOPS.PAO.Models.Workflow` to `UNOPS.Workflow.Models`
  - [ ] 1.6 Verify solution compiles successfully
    - Build entire solution
    - Fix any remaining references to deleted files
    - Ensure no compilation errors
  - [ ] 1.7 Review implementation
    - Verify submodule is properly tracked in .gitmodules
    - Verify all old workflow code is removed
    - Verify project references are correct

- [ ] 2.0 Database Migration & Entity Changes
  - [ ] 2.1 Create WorkflowStatus enum
    - Create `UNOPS.PAO.Domain/Enums/WorkflowStatus.cs`:
    ```csharp
    public enum WorkflowStatus
    {
        None,
        InWorkflow
    }
    ```
  - [ ] 2.2 Update ModifiableDeletableEntity base class
    - Edit `UNOPS.PAO.Domain/Infrastructure/Audit/ModifiableDeletableEntity.cs`
    - Add `public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.None;`
    - Add `public bool IsInWorkflow => WorkflowStatus == WorkflowStatus.InWorkflow;`
    - This enables entities to track pending approval workflows
  - [ ] 2.3 Modify Opportunity entity to add Stage property
    - Edit `UNOPS.PAO.Domain/Entities/Opportunity.cs`
    - Add new property after line 18:
      ```csharp
      [MaxLength(100)]
      public string? Stage { get; set; }
      ```
    - Keep `WorkflowStageId` (line 19) and `WorkflowStage` (line 20) temporarily for data migration
  - [ ] 2.4 Create EF Core migration for Stage and WorkflowStatus columns
    - Run: `dotnet ef migrations add AddWorkflowPropertiesToEntities`
    - Verify migration adds Stage column to Opportunities table
    - Verify migration adds WorkflowStatus column to entities inheriting ModifiableDeletableEntity
  - [ ] 2.5 Create data migration script to populate Stage from WorkflowStageId
    - Write SQL to copy WorkflowStage.Name to Opportunity.Stage
    - Map old stage names to new 3-stage workflow (IDENTIFY & PROFILE is default)
    - Handle null WorkflowStageId values
    - Test script in development database
  - [ ] 2.6 Remove WorkflowStageId from Opportunity entity
    - Edit `UNOPS.PAO.Domain/Entities/Opportunity.cs`
    - **Remove:** `public int? WorkflowStageId { get; set; }` (line 19)
    - **Remove:** `public virtual WorkflowStage? WorkflowStage { get; set; }` (line 20)
    - Make Stage non-nullable: change `public string? Stage` to `public string Stage { get; set; } = "IDENTIFY & PROFILE";`
  - [ ] 2.7 Create migration to drop WorkflowStages table
    - Run: `dotnet ef migrations add DropWorkflowStagesTable`
    - Verify migration drops the table and FK constraint
  - [ ] 2.8 Configure WorkflowDbContext in Startup.cs
    - In `ConfigureContainer(ServiceRegistry services)` method
    - Add workflow DbContext with same connection string but `workflow` schema
    - Register `AddWorkflowServices()` with PostgreSQL storage
    - Configure schema name as "workflow"
    - Follow existing DbContext registration pattern (see `AppDbContext` registration)
  - [ ] 2.9 Verify workflow schema is auto-created on startup
    - Run application and check database
    - Verify `workflow.StateMachineStageChanges` table exists
    - Verify `workflow.StateMachineStageChangeRoles` table exists
    - Verify `workflow.WorkflowLogs` table exists
  - [ ] 2.10 Review implementation
    - Verify all migrations apply cleanly
    - Verify data migration preserves existing Stage data
    - Verify workflow schema is properly isolated
    - Verify WorkflowStatus defaults to None

- [ ] 3.0 Backend Interface Implementations & Service Registration
  - [ ] 3.1 Create Workflow folder structure in Business project
    - Create `UNOPS.PAO.Business/Workflow/` folder (for workflow definitions)
    - Create `UNOPS.PAO.Business/Workflow/Interfaces/` subfolder (for PAO-specific interfaces)
    - Create `UNOPS.PAO.Business/Workflow/Adapters/` subfolder (for interface implementations)
    - Create `UNOPS.PAO.Business/Workflow/Seeders/` subfolder (for seeder classes)
  - [ ] 3.1.5 Create IPaoWorkflowApproverProvider interface
    - Create `UNOPS.PAO.Business/Workflow/Interfaces/IPaoWorkflowApproverProvider.cs`
    - Extend base `IWorkflowApproverProvider` from submodule
    - Keep empty initially as placeholder for future PAO-specific methods:
    ```csharp
    using UNOPS.Workflow.Business.Interfaces;
    
    namespace UNOPS.PAO.Business.Workflow.Interfaces;
    
    /// <summary>
    /// PAO-specific workflow approver provider interface.
    /// Extends base interface for future PAO-specific methods.
    /// </summary>
    public interface IPaoWorkflowApproverProvider : IWorkflowApproverProvider
    {
        // Placeholder for future PAO-specific approval methods
        // e.g., Task<bool> CanUserApproveOpportunityAsync(int opportunityId, int userId);
    }
    ```
  - [ ] 3.2 Implement PaoWorkflowUserContext class
    - Create `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowUserContext.cs`
    - Implement `IWorkflowUserContext` from submodule
    - Inject `IHttpContextAccessor`, `IConfiguration`, `IManagerWrapper`
    - Implement properties:
    ```csharp
    public int CurrentUserId => int.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        out var id) ? id : 0;
    
    public string CurrentUserName
    {
        get
        {
            var userId = CurrentUserId;
            if (userId == 0) return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            // Query user profile from AppDbContext via IManagerWrapper.Context
            var user = _managerWrapper.Context.Users.FirstOrDefault(u => u.Id == userId);
            return user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";
        }
    }
    ```
    - Implement `CurrentUserRoles` from `ClaimTypes.Role` claims
    - Implement `Environment` from `IConfiguration.GetValue<string>("AppConfig:Environment")`
    - Implement `IsAuthenticated` from `_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated`
  - [ ] 3.3 Implement PaoEntityStageProvider class
    - Create `UNOPS.PAO.Business/Workflow/Adapters/PaoEntityStageProvider.cs`
    - Implement `IEntityStageProvider` from submodule
    - Inject `AppDbContext`
    - Use lowercase entity name `"opportunity"` in switch expression
    - Example implementation:
    ```csharp
    public async Task<string?> GetCurrentStageAsync(string entityName, string entityId)
    {
        if (!int.TryParse(entityId, out var id)) return null;
        return entityName switch
        {
            "opportunity" => await _context.Opportunities
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => x.Stage)
                .FirstOrDefaultAsync(),
            _ => null
        };
    }
    
    private async Task<bool> UpdateOpportunityStageAsync(int id, string newStage, int userId)
    {
        var entity = await _context.Opportunities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return false;
        entity.Stage = newStage;
        entity.LastModifiedBy = userId;
        entity.LastModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    ```
    - Create separate private method for each entity type's update
    - Implement `GetEntityDisplayNameAsync` to return Opportunity.Name
  - [ ] 3.4 Implement PaoWorkflowApproverProvider class
    - Create `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs`
    - Implement `IWorkflowApproverProvider` from submodule
    - Inject `AppDbContext` and `WorkflowDbContext`
    - Create `GetStageChangeRoles()` helper method to query workflow schema:
    ```csharp
    private List<(int RoleId, string RoleName, bool CanApprove, bool CanTrigger)> GetStageChangeRoles(
        string entityType, string fromStage, string toStage)
    {
        return _workflowContext.StateMachineStageChangeRoles
            .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active &&
                        x.EntityType == entityType &&
                        x.FromStage == fromStage && x.ToStage == toStage)
            .Select(x => new { x.RoleId, x.RoleName, x.CanApprove, x.CanTrigger })
            .ToList()
            .Select(x => (x.RoleId, x.RoleName ?? string.Empty, x.CanApprove, x.CanTrigger))
            .ToList();
    }
    ```
    - Create `GetOpportunityApprovers()` and `GetOpportunityTriggers()` methods
    - Query PAO's `EntityUserRole` table to find users with required roles
    - Return `List<WorkflowTaskModel>` with UserId and Role
  - [ ] 3.5 Implement PaoWorkflowNotificationService class
    - Create `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs`
    - Implement `IWorkflowNotificationService` from submodule
    - Inject `IEmailSender` from UNOPS.PAO.MailSender
    - Implement `NotifyNewApprovalRequestAsync`
    - Implement `NotifyWorkflowCompletedAsync`
    - Implement `NotifyWorkflowRejectedAsync`
    - Implement `NotifyWorkflowRecalledAsync`
    - Handle multiple recipients gracefully
  - [ ] 3.6 Create email templates for workflow notifications
    - Create `EmailTemplates/WorkflowApprovalRequest.html`
    - Create `EmailTemplates/WorkflowCompleted.html`
    - Create `EmailTemplates/WorkflowRejected.html`
    - Include entity name, URL, performer, comment placeholders
  - [ ] 3.7 Create WorkflowServiceExtensions.cs
    - Create `UNOPS.PAO.Business/Workflow/Adapters/WorkflowServiceExtensions.cs`
    - Follow GMS pattern - use `IServiceCollection` (compatible with Lamar's ServiceRegistry)
    - Create extension method for PAO-specific workflow registration:
    ```csharp
    using Microsoft.Extensions.DependencyInjection;
    using UNOPS.Workflow.DataAccess;
    
    public static class WorkflowServiceExtensions
    {
        public static IServiceCollection AddPaoWorkflowServices(
            this IServiceCollection services,
            Action<WorkflowOptions> configure)
        {
            // Register submodule's core services
            services.AddWorkflowServices(configure);
            
            // Register PAO-specific implementations
            services.AddScoped<IWorkflowUserContext, PaoWorkflowUserContext>();
            services.AddScoped<IEntityStageProvider, PaoEntityStageProvider>();
            services.AddScoped<IWorkflowApproverProvider, PaoWorkflowApproverProvider>();
            services.AddScoped<IWorkflowNotificationService, PaoWorkflowNotificationService>();
            
            return services;
        }
    }
    ```
  - [ ] 3.8 Register workflow services in Startup.cs
    - In `UNOPS.PAO.Server/Startup.cs` `ConfigureContainer(ServiceRegistry services)` method
    - Add: `services.AddPaoWorkflowServices(options => options.UsePostgreSqlStorage(connectionString));`
    - Place after existing DbContext registration (around line 183)
    - Note: IHttpContextAccessor is already registered in Startup.cs
  - [ ] 3.9 Create unit tests for PaoWorkflowUserContext (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowUserContextTests.cs`
    - Follow existing test pattern (see `UNOPSPartnerManagerTests.cs`)
    - Use xUnit, Moq, FluentAssertions
    - Test CurrentUserId extraction from claims
    - Test CurrentUserName extraction
    - Test CurrentUserEmail extraction
    - Test CurrentUserRoles extraction
    - Test unauthenticated user returns defaults
    - Test Environment property
    - Verify all tests compile and run successfully with no errors
  - [ ] 3.10 Create unit tests for PaoEntityStageProvider (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoEntityStageProviderTests.cs`
    - Use InMemory database (see existing test pattern)
    - Test GetCurrentStageAsync returns correct stage for Opportunity
    - Test GetCurrentStageAsync returns null for non-existent entity
    - Test UpdateStageAsync updates Stage and audit fields
    - Test IsEntityValidAsync returns false for deleted entities
    - Test GetEntityDisplayNameAsync returns entity Name
    - Test unsupported entity types handled gracefully
    - Verify all tests compile and run successfully with no errors
  - [ ] 3.11 Create unit tests for PaoWorkflowApproverProvider (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs`
    - Use InMemory database and Moq for mocking
    - Test GetApproversAsync returns correct approvers
    - Test GetApprovalConfigurationAsync returns correct roles
    - Test CanUserApproveAsync returns true for authorized user
    - Test CanUserApproveAsync returns false for unauthorized user
    - Test empty list returned for unconfigured transitions
    - Verify all tests compile and run successfully with no errors
  - [ ] 3.12 Review implementation
    - Verify all interfaces are correctly implemented
    - Verify service registration order is correct
    - Verify all unit tests pass
    - Check code follows PAO patterns

- [ ] 4.0 Opportunity Workflow Configuration (State Machine & Seeder)
  - [ ] 4.1 Create OpportunityWorkflow state machine class
    - Create `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs`
    - Use submodule's models directly with **3 stages** (no Decide stage):
    ```csharp
    using UNOPS.Workflow.Models;  // Use submodule's models
    
    namespace UNOPS.PAO.Business.Workflow;
    
    public static class OpportunityWorkflow
    {
        public const string EntityName = "Opportunity";
        
        public static class Stages
        {
            public const string IdentifyAndProfile = "IDENTIFY & PROFILE";
            public const string Go = "GO";
            public const string NoGo = "NO GO";
        }
        
        public static StateMachine StateMachine => new()
        {
            EntityType = EntityName,
            States =
            [
                new State() { Sequence = 1, StageCode = Stages.IdentifyAndProfile, Facing = Facing.Internal },
                new State() { Sequence = 2, StageCode = Stages.Go, Facing = Facing.Internal },
                new State() { Sequence = 3, StageCode = Stages.NoGo, Facing = Facing.Internal }
            ]
        };
    }
    ```
    - Note: GO is final stage (no transitions out), NO GO can be reopened
  - [ ] 4.2 Create StateMachineStageChangeSeeder class
    - Create `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs`
    - Create as static class with extension method:
    - **3 transitions with approval workflow**:
    ```csharp
    public static class StateMachineStageChangeSeeder
    {
        private static List<StateMachineStageChange> GetSeedStageChanges()
        {
            return new List<StateMachineStageChange>
            {
                // Transition 1: IDENTIFY & PROFILE → GO (requires approval)
                new StateMachineStageChange {
                    EntityName = "Opportunity",
                    FromStage = "IDENTIFY & PROFILE",
                    ToStage = "GO",
                    Sequence = 1,
                    CommentRequired = true, CommentOptional = false,
                    ApprovalRequired = true,  // Requires DOA Holder approval
                    Internal = true, External = false,
                    Name = "Submit for Go",
                    Status = EntityStatus.Active
                },
                // Transition 2: IDENTIFY & PROFILE → NO GO (requires approval)
                new StateMachineStageChange {
                    EntityName = "Opportunity",
                    FromStage = "IDENTIFY & PROFILE",
                    ToStage = "NO GO",
                    Sequence = 2,
                    CommentRequired = true, CommentOptional = false,
                    ApprovalRequired = true,  // Requires DOA Holder approval
                    Internal = true, External = false,
                    Name = "Submit for No Go",
                    Status = EntityStatus.Active
                },
                // Transition 3: NO GO → IDENTIFY & PROFILE (reopen, no approval)
                new StateMachineStageChange {
                    EntityName = "Opportunity",
                    FromStage = "NO GO",
                    ToStage = "IDENTIFY & PROFILE",
                    Sequence = 1,
                    CommentRequired = false, CommentOptional = true,
                    ApprovalRequired = false,  // No approval needed for reopen
                    Internal = true, External = false,
                    Name = "Reopen",
                    Status = EntityStatus.Active
                }
            };
        }
        
        public static async Task SeedStateMachineStageChangesAsync(this IServiceProvider services)
        {
            var workflowContext = services.GetRequiredService<WorkflowDbContext>();
            var logger = services.GetRequiredService<ILogger<WorkflowDbContext>>();
            // Idempotent seeding logic - check existing, add new, update changed
        }
    }
    ```
    - Add all 3 transitions per PRD workflow definition (with approval)
    - Make seeder idempotent (check existing records, handle duplicates, reactivate deleted)
  - [ ] 4.3 Create StateMachineStageChangeRoleSeeder class
    - Create `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeRoleSeeder.cs`
    - Create as static class with async method
    - Look up PAO roles (Opportunity Manager, DOA Holder) from database
    - Create `StateMachineStageChangeRole` entries for **approval workflow**:
      - **IDENTIFY & PROFILE → GO:**
        * Opportunity Manager: CanTrigger = true, CanApprove = false
        * DOA Holder: CanTrigger = false, CanApprove = true
      - **IDENTIFY & PROFILE → NO GO:**
        * Opportunity Manager: CanTrigger = true, CanApprove = false
        * DOA Holder: CanTrigger = false, CanApprove = true
      - **NO GO → IDENTIFY & PROFILE (Reopen):**
        * Opportunity Manager: CanTrigger = true, CanApprove = false (no approval needed)
    - Create `SeedStateMachineStageChangeRolesAsync(this IServiceProvider services)` extension method
    - Make seeder idempotent
  - [ ] 4.4 Register seeders to run on application startup
    - Add to `Program.cs` or `Startup.cs` after building the app:
    ```csharp
    // Seed workflow data
    await app.Services.SeedStateMachineStageChangesAsync();
    await app.Services.SeedStateMachineStageChangeRolesAsync();
    ```
    - Or add to existing seed endpoint in `SystemAdminController`
    - Seeders are idempotent so safe to run multiple times
  - [ ] 4.5 Create unit tests for OpportunityWorkflow (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityWorkflowTests.cs`
    - Test StateMachine has correct EntityType = "Opportunity"
    - Test StateMachine has 3 states (IDENTIFY & PROFILE, GO, NO GO)
    - Test state sequences are correct (1, 2, 3)
    - Test all states have correct Facing configuration (Facing.Internal)
    - Verify all tests compile and run successfully with no errors
  - [ ] 4.6 Create unit tests for StateMachineStageChangeSeeder (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/StateMachineStageChangeSeederTests.cs`
    - Use InMemory database for testing
    - Test seeder creates all 3 transitions
    - Test seeder is idempotent (running twice creates same result)
    - Test transitions have correct ApprovalRequired flags (true for Go/No Go, false for Reopen)
    - Test comment requirements are set correctly
    - Verify all tests compile and run successfully with no errors
  - [ ] 4.7 (OPTIONAL) Create OpportunityStageRequirements class
    - Create `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirements.cs` if needed
    - Define field validation requirements for each stage transition
    - Return list of required fields, validation rules, error messages
    - This is OPTIONAL for initial implementation - can be added when requirements are clear
  - [ ] 4.8 Review implementation
    - Verify state machine matches PRD workflow diagram (3 stages, approval workflow)
    - Verify all transitions are seeded correctly
    - Verify role permissions are correct
    - Run seeder and verify database records

- [ ] 5.0 API Endpoints & Backend Integration
  - [ ] 5.1 Create WorkflowController
    - Create `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`
    - Inherit from `BaseController` (NOT ControllerBase)
    - Add `[Route("/")]` at class level (PAO convention)
    - Add `[Authorize(AuthenticationSchemes = "IAP")]` attribute
    - Inject dependencies via constructor (follow `OpportunityController` pattern):
      - `ILogger<WorkflowController> logger`
      - `IAuthorizationService authorizationService`
      - `UserResolverService<int> userResolverService`
    - Inject `IWorkflowManager` from submodule (via DI, not ManagerWrapper)
    - Inject `IEntityStageProvider`
    - Use `APIDictionary.Workflow` constant for route paths (already defined)
  - [ ] 5.2 Implement GET /api/workflow/{entityName} endpoint
    - Return workflow stages for entity type
    - Use OpportunityWorkflow.StateMachine for "opportunity"
    - Return 404 for unsupported entity types
  - [ ] 5.3 Implement GET /api/workflow/{entityName}/{id} endpoint
    - Return current state and available actions
    - Query entity's current stage
    - Calculate available transitions based on user role
    - Return 404 if entity not found
  - [ ] 5.4 Implement GET /api/workflow/{entityName}/{id}/details endpoint
    - Return workflow details including approval status
    - Return current stage, next stage (if pending), approvers list
    - Return `isInWorkflow` flag indicating pending approval
  - [ ] 5.5 Implement POST /api/workflow/submit endpoint
    - Accept entityName, entityId, newStage, comment in body
    - Validate transition is allowed
    - Check user has trigger permission for transition
    - If ApprovalRequired = true:
      * Set entity.WorkflowStatus = InWorkflow
      * Call IWorkflowManager.Initiate() to start approval
      * Send notification to approvers
    - If ApprovalRequired = false:
      * Execute stage change directly
    - Return success response with workflow status
  - [ ] 5.6 Implement POST /api/workflow/approve endpoint
    - Accept entityName, entityId, comment in body
    - Check user has approval permission
    - Call IWorkflowManager.Approve() to complete workflow
    - Set entity.WorkflowStatus = None
    - Update entity Stage to target stage
    - Send completion notification
    - Return success response
  - [ ] 5.7 Implement POST /api/workflow/reject endpoint
    - Accept entityName, entityId, comment in body
    - Check user has approval permission
    - Call IWorkflowManager.Reject() to cancel workflow
    - Set entity.WorkflowStatus = None
    - Entity Stage stays at current stage
    - Send rejection notification
    - Return success response
  - [ ] 5.8 Implement POST /api/workflow/recall endpoint
    - Accept entityName, entityId in body
    - Check user is the one who initiated the workflow
    - Call IWorkflowManager.Recall() to cancel
    - Set entity.WorkflowStatus = None
    - Return success response
  - [ ] 5.9 Implement GET /api/workflow/{entityName}/{id}/history endpoint
    - Return stage change history from WorkflowLogs
    - Order by CompletedOn descending
    - Include user, action, comment, dates
  - [ ] 5.10 Verify APIDictionary workflow constant
    - **Already exists:** `Workflow = APIPrefix + "workflow"` on line 57 of `UNOPS.PAO.Presentation/Helpers/APIDictionary.cs`
    - No action needed unless additional endpoint path constants are required
  - [ ] 5.11 Update OpportunityManager to integrate workflow
    - Update GetById to include Stage, WorkflowStatus in response model
    - Add `StartWorkflow(id)` method to set WorkflowStatus = InWorkflow
    - Add `EndWorkflow(id)` method to set WorkflowStatus = None
    - Add `UpdateStageAsync(id, newStage, baseUri)` method for stage changes
    - Remove old WorkflowStage navigation property usage
    - Integrate with IWorkflowManager from submodule
  - [ ] 5.12 Update Opportunity-related DTOs
    - Edit `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs`:
      - **Remove:** `public int? WorkflowStageId { get; set; }` (line 14)
      - **Remove:** `public string? WorkflowStageName { get; set; }` (line 15)
      - **Add:** `public string? Stage { get; set; }` property
      - **Add:** `public WorkflowStatus WorkflowStatus { get; set; }` property
      - **Add:** `public bool IsInWorkflow { get; set; }` property
      - **Update** `CalculateConditionalTags()` method (lines 168, 172): replace `WorkflowStageName` with `Stage`
    - Edit `UNOPS.PAO.Models/Opportunities/OpportunityListModel.cs`:
      - **Remove:** `public int? WorkflowStageId { get; set; }` (line 30)
      - **Remove:** `public string? WorkflowStageName { get; set; }` (line 31)
      - **Add:** `public string? Stage { get; set; }` property
      - **Add:** `public WorkflowStatus WorkflowStatus { get; set; }` property
      - **Add:** `public bool IsInWorkflow { get; set; }` property
      - **Update** `CalculateConditionalTags()` method (lines 82, 85): replace `WorkflowStageName` with `Stage`
    - Edit `UNOPS.PAO.Models/Opportunities/OpportunityRequest.cs`:
      - **Remove:** `public int? WorkflowStageId { get; set; }` (line 8)
    - Edit `UNOPS.PAO.Models/Opportunities/UpdateOpportunityRequest.cs`:
      - **Remove:** `public int? WorkflowStageId { get; set; }` (line 9)
    - Edit `UNOPS.PAO.Models/Opportunities/ApplyOpportunityAiChangesRequest.cs`:
      - **Remove:** `public int? WorkflowStageId { get; set; }` (line 62)
    - Edit `UNOPS.PAO.Models/Dashboard/DashboardModels.cs`:
      - **Remove:** `public string? WorkflowStageName { get; set; }` (line 83)
      - **Add:** `public string? Stage { get; set; }` property
  - [ ] 5.13 Update AutoMapper OpportunityMappingProfiles
    - Edit `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs`:
      - **Remove:** `.ForMember(dest => dest.WorkflowStageName, opt => opt.MapFrom(src => src.WorkflowStage != null ? src.WorkflowStage.Name : null))`
      - **Add:** `.ForMember(dest => dest.Stage, opt => opt.MapFrom(src => src.Stage))`
      - **Add:** `.ForMember(dest => dest.WorkflowStatus, opt => opt.MapFrom(src => src.WorkflowStatus))`
      - **Add:** `.ForMember(dest => dest.IsInWorkflow, opt => opt.MapFrom(src => src.IsInWorkflow))`
      - Remove any `WorkflowStageId` mapping if present
    - Edit `UNOPS.PAO.UNOPSBusiness/Managers/Mapping/OpportunityMappingProfile.cs`:
      - **Remove:** `.ForMember(dest => dest.WorkflowStageName, ...)` mappings (lines 18, 42)
      - **Add:** `.ForMember(dest => dest.Stage, opt => opt.MapFrom(src => src.Stage))`
      - **Add:** `.ForMember(dest => dest.WorkflowStatus, opt => opt.MapFrom(src => src.WorkflowStatus))`
      - **Add:** `.ForMember(dest => dest.IsInWorkflow, opt => opt.MapFrom(src => src.IsInWorkflow))`
  - [ ] 5.13.1 Update UNOPSOpportunityManager
    - Edit `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs`:
      - **Update lines 99-101:** Replace `WorkflowStageId` logic with `Stage` initialization
        - Change: `if (entity.WorkflowStageId == null || entity.WorkflowStageId == 0) { entity.WorkflowStageId = 1; }`
        - To: `if (string.IsNullOrEmpty(entity.Stage)) { entity.Stage = "IDENTIFY & PROFILE"; }`
      - **Update lines 3090-3092:** Remove `WorkflowStageId` update logic (stage changes via workflow API now)
      - **Update lines 3988-3989:** Replace `workflowStageId` and `workflowStageName` with `stage` in AI context
  - [ ] 5.14 Create unit tests for WorkflowController (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/Controllers/WorkflowControllerTests.cs`
    - Follow existing controller test pattern
    - Use Moq to mock IWorkflowManager and IEntityStageProvider
    - Test GET endpoints return correct data
    - Test POST /submit initiates approval workflow correctly
    - Test POST /approve completes workflow and updates stage
    - Test POST /reject cancels workflow and keeps current stage
    - Test POST /recall allows user to cancel their own pending approval
    - Test 400 returned for invalid transitions
    - Test 403 returned for unauthorized users
    - Test 404 returned for non-existent entities
    - Verify all tests compile and run successfully with no errors
  - [ ] 5.15 Review implementation
    - Verify all endpoints follow PAO controller patterns
    - Verify authorization is correctly applied
    - Verify approval workflow works end-to-end
    - Verify DTOs are returned, never entities
    - Test endpoints manually using Swagger/Postman

- [ ] 6.0 Frontend Integration
  - [ ] 6.1 Configure Angular path alias for workflow submodule **(Recommended approach)**
    - Edit `UNOPS.PAO.ClientApp/tsconfig.json`
    - Add path aliases:
    ```json
    {
      "compilerOptions": {
        "paths": {
          "@unops/workflow": ["../UNOPS.Workflow/unops-workflow-angular/src/public-api.ts"],
          "@unops/workflow/*": ["../UNOPS.Workflow/unops-workflow-angular/src/*"]
        }
      }
    }
    ```
    - **Why path alias:** No build step needed, direct source access, easy debugging
  - [ ] 6.2 Delete old PAO workflow Angular components
    - **Delete files that exist:**
      - `src/app/shared/services/domain/workflow.service.ts`
      - `src/app/shared/services/domain/workflow.service.spec.ts`
      - `src/app/shared/components/workflows/workflow/workflow.component.ts`
      - `src/app/shared/components/workflows/workflow/workflow.component.spec.ts`
    - Delete entire `src/app/shared/components/workflows/` folder
    - Update `src/app/shared/services/domain/index.ts` to remove workflow exports (if present)
  - [ ] 6.3 Add workflow translation keys to i18n files
    - Edit `UNOPS.PAO.ClientApp/src/assets/i18n/en.json`
    - Add `title.stage`, `label.workflow.currentStage`, `label.workflow.nextStage`
    - Add `label.workflow.approvalPending`, `label.workflow.overview`
    - Add `label.workflow.approvers`, `label.workflow.stageChangeHistory`
    - Add button labels: `button.workflow.recall`, `button.workflow.approve`, `button.workflow.reject`
    - Add `message.noRecordsFound`
    - See submodule README for complete list
  - [ ] 6.4 Import StageWorkflowComponent in opportunity-view component
    - Edit `src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts`
    - Add import using path alias:
    ```typescript
    import { StageWorkflowComponent } from '@unops/workflow';
    ```
    - Add to component `imports` array (standalone component pattern)
  - [ ] 6.5 Add workflow section to opportunity-view template
    - Edit `src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.html`
    - Add `<app-stage-workflow>` component:
    ```html
    <app-stage-workflow
      #stageWorkflowComponent
      [entityName]="'opportunity'"
      [entityId]="opportunityId().toString()"
      [canChangeStage]="canChangeStage()"
      (onStageChangeSuccess)="handleStageChangeSuccess()"
    ></app-stage-workflow>
    ```
    - Note: Use lowercase `'opportunity'` to match backend entity name convention
  - [ ] 6.6 Implement workflow-related component logic
    - Add ViewChild reference: `@ViewChild('stageWorkflowComponent') stageWorkflowComponent!: StageWorkflowComponent;`
    - Add `canChangeStage` computed property using existing permissions
    - Add `handleStageChangeSuccess()` method to refresh opportunity data:
    ```typescript
    handleStageChangeSuccess() {
      // Reload opportunity data to reflect stage change
      this.loadOpportunity();
    }
    ```
    - (Optional) Add `validateAndSaveBeforeStageChange` callback for pre-transition validation
    - Update `Opportunity` model interface with `stage?: string`
  - [ ] 6.7 Style workflow component placement
    - Position workflow component in header area (below opportunity name/status)
    - Follow existing PAO panel styling conventions
    - Ensure responsive layout using PrimeNG grid
  - [ ] 6.8 Update opportunity-view component unit tests (MANDATORY)
    - Edit/create test file if not exists
    - Test StageWorkflowComponent is rendered when opportunity loaded
    - Test canChangeStage is correctly computed based on permissions
    - Test handleStageChangeSuccess calls loadOpportunity
    - Mock workflow API responses
    - Use HttpTestingController for HTTP mocking
    - Verify all tests compile and run successfully with no errors
  - [ ] 6.9 Review implementation
    - Verify workflow component displays correctly
    - Verify stage changes work end-to-end
    - Verify translations are displayed
    - Test on different screen sizes

- [ ] 7.0 Testing & Documentation
  - [ ] 7.1 Run all backend unit tests
    - Execute: `dotnet test`
    - Verify all tests pass
    - Fix any failing tests
    - Check code coverage meets 80% threshold
  - [ ] 7.2 Run all frontend unit tests
    - Execute: `ng test`
    - Verify all tests pass
    - Fix any failing tests
  - [ ] 7.3 Perform integration testing (3-stage approval workflow)
    - Create test opportunity (starts at IDENTIFY & PROFILE)
    - **Test approval workflow to GO:**
      1. Login as Opportunity Manager
      2. Submit for Go via API (starts approval workflow)
      3. Verify entity.WorkflowStatus = InWorkflow
      4. Verify WorkflowLog created with RequiresApproval = true
      5. Login as DOA Holder
      6. Approve via API
      7. Verify Stage changed to GO
      8. Verify entity.WorkflowStatus = None
    - **Test approval workflow to NO GO:**
      1. Create new opportunity
      2. Submit for No Go (starts approval workflow)
      3. DOA Holder approves
      4. Verify Stage changed to NO GO
    - **Test reopen (no approval):**
      1. From NO GO stage, click Reopen
      2. Verify Stage changed directly to IDENTIFY & PROFILE (no approval needed)
    - Verify GO is final stage (no further transitions)
  - [ ] 7.4 Test approval workflow edge cases
    - **Test Reject:** DOA Holder rejects → Stage stays at IDENTIFY & PROFILE
    - **Test Recall:** Opportunity Manager recalls their own pending request
    - Test unauthorized user cannot approve/reject
    - Verify 403 returned for unauthorized attempts
    - Test email notifications are sent (check logs or mock email service)
  - [ ] 7.5 Test workflow UI end-to-end
    - Login as Opportunity Manager
    - Navigate to opportunity detail page
    - Verify workflow component displays current stage
    - Click "Submit for Go" and verify approval pending indicator
    - Login as DOA Holder
    - Navigate to same opportunity
    - Verify Approve/Reject buttons appear
    - Click Approve and verify stage change
    - Check workflow history shows all actions
  - [ ] 7.6 Update README with workflow integration instructions
    - Document Git submodule setup commands
    - Document how to run seeders
    - Document API endpoints (including approval workflow endpoints)
    - Include troubleshooting section
  - [ ] 7.7 Document how to add workflow to new entities
    - Create step-by-step guide
    - Reference OpportunityWorkflow as example
    - List required changes (entity, state machine, seeder, provider)
  - [ ] 7.8 Add code comments to interface implementations
    - Add XML documentation to all public methods
    - Document any complex logic
    - Reference PRD and Migration Guide where appropriate
  - [ ] 7.9 Create workflow diagram for documentation
    - Document Opportunity workflow stages
    - Document transitions and role requirements
    - Add to PRD appendix or separate doc
  - [ ] 7.10 Final review and sign-off
    - Code review by senior developer
    - Verify all acceptance criteria met
    - Verify no linter errors or warnings
    - Verify all documentation complete
    - Get stakeholder approval
