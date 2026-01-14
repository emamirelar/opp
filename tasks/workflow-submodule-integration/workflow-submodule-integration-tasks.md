# Workflow Submodule Integration - Implementation Tasks

## Relevant Files

**Backend Files (.NET Core) - NEW:**
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowUserContext.cs` - IWorkflowUserContext implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoEntityStageProvider.cs` - IEntityStageProvider implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs` - IWorkflowApproverProvider implementation
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` - IWorkflowNotificationService implementation
- `UNOPS.PAO.Business/Workflow/Adapters/WorkflowServiceExtensions.cs` - DI registration extension method
- `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` - Opportunity state machine definition
- `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs` - Stage transition seeder
- `UNOPS.PAO.Business/EmailTemplates/WorkflowApprovalRequest.html` - Email template
- `UNOPS.PAO.Business/EmailTemplates/WorkflowCompleted.html` - Email template
- `UNOPS.PAO.Business/EmailTemplates/WorkflowRejected.html` - Email template
- `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` - Workflow API endpoints (inherits from `BaseController`)

**Backend Files (.NET Core) - MODIFY:**
- `UNOPS.PAO.Domain/Entities/Opportunity.cs` - Add Stage property, remove WorkflowStageId
- `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs` - Add Stage property (note: models are in feature subfolders)
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - Integrate workflow methods
- `UNOPS.PAO.Business/Mapping/MappingProfile.cs` - Update Opportunity mapping
- `UNOPS.PAO.Server/Startup.cs` - Register workflow services in `ConfigureContainer()` method
- `UNOPS.PAO.Business/Interfaces/IManagerWrapper.cs` - Remove IWorkflowManager property
- `UNOPS.PAO.Business/Managers/ManagerWrapper.cs` - Remove WorkflowManager instantiation
- `UNOPS.PAO.DataAccess/Context/AppDbContext.cs` - Remove WorkflowStage and WorkflowLog DbSets

**Backend Files (.NET Core) - DELETE:**
- `UNOPS.PAO.Domain/Entities/WorkflowStage.cs`
- `UNOPS.PAO.Domain/Entities/WorkflowLog.cs`
- `UNOPS.PAO.Business/Managers/WorkflowManager.cs`
- `UNOPS.PAO.Business/Interfaces/IWorkflowManager.cs`
- `UNOPS.PAO.Models/Workflow/` - Entire folder (submodule provides these in `UNOPS.Workflow.Models`)

**Backend - Unit Tests:**
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowUserContextTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoEntityStageProviderTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityWorkflowTests.cs`
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityWorkflowSeederTests.cs`
- `UNOPS.PAO.IntegrationTests/Controllers/WorkflowControllerTests.cs`

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
- PAO does NOT have `ServiceExtensions.cs` - services are registered in `Startup.cs.ConfigureContainer()`
- PAO managers are instantiated directly in `ManagerWrapper.cs`, NOT via DI
- Controllers inherit from `BaseController` and use `APIDictionary` for route constants
- Angular components are in `features/partnerships/opportunities/` (not `features/opportunities/`)
- Follow PAO coding standards and naming conventions
- Reference Migration-Guide-WorkflowStage-To-StateMachine.md for detailed guidance
- **Use `UNOPS.Workflow.Models` (submodule) directly** - do NOT duplicate models locally

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
    - Remove `IWorkflowManager WorkflowManager { get; }` from `UNOPS.PAO.Business/Interfaces/IManagerWrapper.cs`
    - Remove `private IWorkflowManager workflowManager;` field from `UNOPS.PAO.Business/Managers/ManagerWrapper.cs`
    - Remove `workflowManager = new WorkflowManager(context);` from ManagerWrapper constructor
    - Remove `public virtual IWorkflowManager WorkflowManager => workflowManager;` property
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
  - [ ] 2.1 Modify Opportunity entity to add Stage property
    - Add `public string? Stage { get; set; }` property
    - Add `[MaxLength(100)]` attribute
    - Keep WorkflowStageId temporarily for data migration
  - [ ] 2.2 Create EF Core migration for Stage column
    - Run: `dotnet ef migrations add AddStageToOpportunity`
    - Verify migration adds Stage column to Opportunities table
  - [ ] 2.3 Create data migration script to populate Stage from WorkflowStageId
    - Write SQL to copy WorkflowStage.Name to Opportunity.Stage
    - Handle null WorkflowStageId values
    - Test script in development database
  - [ ] 2.4 Remove WorkflowStageId from Opportunity entity
    - Remove `WorkflowStageId` property
    - Remove `WorkflowStage` navigation property
    - Remove any `[ForeignKey]` attributes
  - [ ] 2.5 Create migration to drop WorkflowStages table
    - Run: `dotnet ef migrations add DropWorkflowStagesTable`
    - Verify migration drops the table and FK constraint
  - [ ] 2.6 Configure WorkflowDbContext in Startup.cs
    - In `ConfigureContainer(ServiceRegistry services)` method
    - Add workflow DbContext with same connection string but `workflow` schema
    - Register `AddWorkflowServices()` with PostgreSQL storage
    - Configure schema name as "workflow"
    - Follow existing DbContext registration pattern (see `AppDbContext` registration)
  - [ ] 2.7 Verify workflow schema is auto-created on startup
    - Run application and check database
    - Verify `workflow.StateMachineStageChanges` table exists
    - Verify `workflow.StateMachineStageChangeRoles` table exists
    - Verify `workflow.WorkflowLogs` table exists
  - [ ] 2.8 Review implementation
    - Verify all migrations apply cleanly
    - Verify data migration preserves existing Stage data
    - Verify workflow schema is properly isolated

- [ ] 3.0 Backend Interface Implementations & Service Registration
  - [ ] 3.1 Create Workflow folder structure in Business project
    - Create `UNOPS.PAO.Business/Workflow/` folder (for workflow definitions)
    - Create `UNOPS.PAO.Business/Workflow/Adapters/` subfolder (for interface implementations)
    - Create `UNOPS.PAO.Business/Workflow/Seeders/` subfolder (for seeder classes)
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
    - Create extension method for PAO-specific workflow registration:
    ```csharp
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
    - Place after database context registration
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
    - Use submodule's models directly:
    ```csharp
    using UNOPS.Workflow.Models;  // Use submodule's models
    
    namespace UNOPS.PAO.Business.Workflow;
    
    public class OpportunityWorkflow
    {
        public static StateMachine StateMachine
        {
            get
            {
                return new StateMachine()
                {
                    EntityType = "Opportunity",
                    States =
                    [
                        new State() { Sequence = 1, StageCode = "Identify & Profile", Facing = Facing.Internal },
                        new State() { Sequence = 2, StageCode = "Decide", Facing = Facing.Internal },
                        new State() { Sequence = 3, StageCode = "Go", Facing = Facing.Internal },
                        new State() { Sequence = 4, StageCode = "No Go", Facing = Facing.Internal }
                    ]
                };
            }
        }
    }
    ```
    - Note: IsFinalStage logic is handled in seeder/business logic, not in State class
  - [ ] 4.2 Create StateMachineStageChangeSeeder class
    - Create `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs`
    - Create as static class with extension method:
    ```csharp
    public static class StateMachineStageChangeSeeder
    {
        private static List<StateMachineStageChange> GetSeedStageChanges()
        {
            return new List<StateMachineStageChange>
            {
                new StateMachineStageChange {
                    EntityName = "Opportunity",
                    FromStage = "Identify & Profile",
                    ToStage = "Decide",
                    Sequence = 1,
                    CommentRequired = false, CommentOptional = true,
                    ApprovalRequired = false,
                    Internal = true, External = false,
                    Name = "Move to Decide",
                    Status = EntityStatus.Active
                },
                // ... add all 5 transitions per PRD
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
    - Add all 5 transitions per PRD workflow definition
    - Make seeder idempotent (check existing records, handle duplicates, reactivate deleted)
  - [ ] 4.3 Create StateMachineStageChangeRoleSeeder class
    - Create `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeRoleSeeder.cs`
    - Create as static class with async method
    - Look up PAO roles (Opportunity Manager, DOA Holder) from database
    - Create `StateMachineStageChangeRole` entries:
      - Opportunity Manager: Can trigger "Identify & Profile → Decide"
      - DOA Holder: Can trigger "Decide → Go", "Decide → No Go", "Decide → Identify & Profile"
      - Opportunity Manager: Can trigger "No Go → Identify & Profile"
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
    - Test StateMachine has 4 states (Identify & Profile, Decide, Go, No Go)
    - Test state sequences are correct (1, 2, 3, 4)
    - Test all states have correct Facing configuration (Facing.Internal)
    - Verify all tests compile and run successfully with no errors
  - [ ] 4.6 Create unit tests for StateMachineStageChangeSeeder (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/StateMachineStageChangeSeederTests.cs`
    - Use InMemory database for testing
    - Test seeder creates all 5 transitions
    - Test seeder is idempotent (running twice creates same result)
    - Test transitions have correct role requirements
    - Test comment requirements are set correctly for GO/NO GO transitions
    - Verify all tests compile and run successfully with no errors
  - [ ] 4.7 (OPTIONAL) Create OpportunityStageRequirements class
    - Create `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirements.cs` if needed
    - Define field validation requirements for each stage transition
    - Return list of required fields, validation rules, error messages
    - This is OPTIONAL for initial implementation - can be added when requirements are clear
  - [ ] 4.8 Review implementation
    - Verify state machine matches PRD workflow diagram
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
  - [ ] 5.4 Implement POST /api/workflow endpoint
    - Accept entityName, entityId, newStage, comment in body
    - Validate transition is allowed
    - Check user has permission for transition
    - Execute stage change via IWorkflowManager
    - Return success response with new stage
  - [ ] 5.5 Implement GET /api/workflow/{entityName}/{id}/history endpoint
    - Return stage change history from WorkflowLogs
    - Order by CompletedOn descending
    - Include user, action, comment, dates
  - [ ] 5.6 Verify APIDictionary workflow constant
    - Verify `Workflow = APIPrefix + "workflow"` exists in `UNOPS.PAO.Presentation/Helpers/APIDictionary.cs`
    - Add any additional endpoint path constants if needed
  - [ ] 5.7 Update OpportunityManager to integrate workflow
    - Update GetById to include Stage in response model
    - Add `GetWorkflowState(id)` method
    - Add `ChangeStage(id, newStage, comment)` method
    - Remove old WorkflowStage navigation property usage
  - [ ] 5.8 Update OpportunityModel DTO
    - Edit `UNOPS.PAO.Models/Opportunities/OpportunityModel.cs`
    - Add `public string? Stage { get; set; }` property
    - Add `WorkflowState` property (optional, for current state details)
    - Remove `WorkflowStageId` property if present
  - [ ] 5.9 Update AutoMapper OpportunityMappingProfile
    - Edit `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs`
    - Map `Opportunity.Stage` to `OpportunityModel.Stage`
    - Remove `WorkflowStageId` mapping if present
  - [ ] 5.10 Create unit tests for WorkflowController (MANDATORY)
    - Create `UNOPS.PAO.IntegrationTests/Controllers/WorkflowControllerTests.cs`
    - Follow existing controller test pattern
    - Use Moq to mock IWorkflowManager and IEntityStageProvider
    - Test GET `/api/workflow/{entityName}` returns correct stages
    - Test GET `/api/workflow/{entityName}/{id}` returns current state and actions
    - Test POST `/api/workflow` validates and executes transitions
    - Test POST endpoint returns 400 for invalid transitions
    - Test POST endpoint returns 403 for unauthorized users
    - Test GET `/api/workflow/{entityName}/{id}/history` returns ordered history
    - Test 404 returned for non-existent entities
    - Verify all tests compile and run successfully with no errors
  - [ ] 5.11 Review implementation
    - Verify all endpoints follow PAO controller patterns
    - Verify authorization is correctly applied
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
  - [ ] 6.2 Delete old PAO workflow Angular components (if exist)
    - Delete `src/app/shared/components/workflows/` folder if it exists
    - Delete `src/app/shared/services/domain/workflow.service.ts` if it exists
    - Update `src/app/shared/services/domain/index.ts` to remove workflow exports
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
  - [ ] 7.3 Perform integration testing
    - Create test opportunity
    - Change stage via API: IDENTIFY & PROFILE → DECIDE
    - Change stage via API: DECIDE → GO
    - Verify Stage property updated in database
    - Verify WorkflowLog created in workflow schema
    - Test NO GO → IDENTIFY & PROFILE (reopen)
  - [ ] 7.4 Test permission-based access
    - Test Opportunity Manager can move to DECIDE
    - Test DOA Holder can move to GO/NO GO
    - Test unauthorized user cannot change stage
    - Verify 403 returned for unauthorized attempts
  - [ ] 7.5 Test workflow UI end-to-end
    - Login as Opportunity Manager
    - Navigate to opportunity detail page
    - Verify workflow component displays
    - Click "Move to Decide" and verify success
    - Check workflow history shows the change
  - [ ] 7.6 Update README with workflow integration instructions
    - Document Git submodule setup commands
    - Document how to run seeders
    - Document API endpoints
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
