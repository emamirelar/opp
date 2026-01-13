# Product Requirements Document: UNOPS.Workflow Submodule Integration

## Initial Requirement

Integrate the UNOPS.Workflow submodule (used in GMS) into PAO to provide a reusable, production-proven workflow infrastructure for implementing future approval processes across all PAO entities.

---

## PRD

### 1. Introduction/Overview

The Partnerships and Opportunities (PAO) application currently uses a basic database-driven `WorkflowStage` system that lacks comprehensive approval workflow capabilities, audit trails, and role-based transition controls. This PRD outlines the integration of the **UNOPS.Workflow submodule** - the same battle-tested workflow library used by the Grants Management System (GMS) - to provide enterprise-grade workflow infrastructure.

**Problem Statement:** PAO needs a robust workflow system to support approval processes for entities like Opportunities, Partners, and Projects. The current `WorkflowStage` implementation is insufficient for:
- Multi-step approval workflows
- Role-based approval permissions
- Comprehensive audit trails
- Workflow action history (approve, reject, recall)
- Separation of internal vs external user workflows

**Solution:** Integrate UNOPS.Workflow as a Git submodule, deprecate the existing `WorkflowStage` system, and migrate to the state machine pattern used successfully in GMS.

**Goal:** Establish production-ready workflow infrastructure that enables implementation of any future approval process without rebuilding workflow logic.

---

### 2. Clarifying Questions and Responses

**Q1: Integration Method**
- Use Git submodule (Option A)
- Add separately to PAO (independent from GMS)

**Q2: Database Schema Strategy**
- Use separate `workflow` schema (Option A)
- Keep WorkflowDbContext separate from AppDbContext

**Q3: Existing PAO Workflow Infrastructure**
- Deprecate/remove existing WorkflowStage and WorkflowLog (Option A)
- Use GMS migration strategy for workflow data

**Q4: PAO-Specific Interface Implementations**
- Support only Opportunity entity initially
- Use string Stage field (not WorkflowStageId)
- Leverage PAO's existing EntityRole/EntityRolePerson system
- Integrate with PAO's email service
- Follow GMS implementation patterns

**Q5: Project Structure & Naming**
- Place implementations in `UNOPS.PAO.Business/Workflow/` (same as GMS)
- Use naming: `PaoEntityStageProvider`, `PaoWorkflowApproverProvider`, etc.

**Q6: Seeding & Configuration**
- Include example data for testing
- Use C# seeders (not SQL)

**Q7: Migration Strategy**
- Add string Stage property to Opportunity now
- Follow GMS database migration approach (auto-created by submodule)

**Q8: Testing & Validation**
- Include test/example workflow for Opportunity
- Stages: Identify & Profile → Decide → Go (final) or No Go
- Transitions are role-based (no approval workflow):
  * Opportunity Manager: Move to Decide, Reopen (from No Go)
  * DOA Holder: Move to Go, Move to No Go, Back to Identify & Profile
- Go is the final stage (no changes possible)
- No Go can be reopened back to Identify & Profile
- Include unit tests

**Q9: Scope & Boundaries**
- Include: Submodule integration, 4 interface implementations, database setup, DI registration
- Include: API endpoints (WorkflowController)
- Include: Angular components integration
- Include: Example Opportunity workflow
- Follow Migration-Guide-WorkflowStage-To-StateMachine.md

**Q10: Future Extensibility**
- Design for multiple entities (Partner, Contact, etc.)
- Follow GMS implementation pattern for extensibility

---

### 3. Goals

1. **Successfully integrate UNOPS.Workflow submodule** into PAO codebase as a Git submodule
2. **Deprecate existing WorkflowStage system** and migrate to state machine pattern
3. **Implement 4 required interfaces** to connect PAO-specific logic with the generic workflow engine
4. **Establish separate workflow database schema** auto-managed by the submodule
5. **Create example Opportunity workflow** to validate the integration
6. **Provide comprehensive documentation** for future workflow implementations
7. **Maintain zero breaking changes** to existing PAO functionality during migration
8. **Enable future approval workflows** for any PAO entity without rebuilding infrastructure

---

### 4. Architecture

#### Current Architecture (Before Migration)

```
PAO Application
├── Entities
│   └── Opportunity
│       └── WorkflowStageId (FK) → WorkflowStages table
├── WorkflowStages table (Database-driven stages)
├── WorkflowLog table (Basic audit)
└── WorkflowManager (Basic stage tracking)
```

**Limitations:**
- No approval workflow support
- No role-based permissions for transitions
- Limited audit trail
- Manual validation in code
- Not reusable across projects

#### Target Architecture (After Migration)

```
┌─────────────────────────────────────────────────────────────────┐
│                    PAO Application Layer                         │
├─────────────────────────────────────────────────────────────────┤
│  PAO Workflow Adapters (UNOPS.PAO.Business/Workflow/)           │
│  ├── PaoWorkflowUserContext : IWorkflowUserContext              │
│  ├── PaoEntityStageProvider : IEntityStageProvider              │
│  ├── PaoWorkflowApproverProvider : IWorkflowApproverProvider    │
│  └── PaoWorkflowNotificationService : IWorkflowNotificationS... │
├─────────────────────────────────────────────────────────────────┤
│                  UNOPS.Workflow Submodule                        │
│  ├── UNOPS.Workflow.Business (WorkflowManager)                  │
│  ├── UNOPS.Workflow.DataAccess (WorkflowDbContext)              │
│  ├── UNOPS.Workflow.Domain (Entities)                           │
│  ├── UNOPS.Workflow.Models (DTOs)                               │
│  └── unops-workflow-angular (Angular components)                │
├─────────────────────────────────────────────────────────────────┤
│                    PostgreSQL Database                           │
│  ├── public schema (PAO entities)                               │
│  │   └── Opportunities                                          │
│  │       └── Stage (string) - No FK!                            │
│  │                                                               │
│  └── workflow schema (auto-created by submodule)                │
│      ├── StateMachineStageChanges (allowed transitions)         │
│      ├── StateMachineStageChangeRoles (role permissions)        │
│      └── WorkflowLogs (complete audit trail)                    │
└─────────────────────────────────────────────────────────────────┘
```

#### Key Architecture Changes

1. **Entity Changes:**
   - Remove: `Opportunity.WorkflowStageId` (int FK)
   - Add: `Opportunity.Stage` (string property)
   - Remove: `Opportunity.WorkflowStage` navigation property

2. **Database Schema:**
   - Deprecate: `WorkflowStages` table
   - Deprecate: `WorkflowLog` table in public schema
   - Add: `workflow` schema (auto-created by submodule with 3 tables)

3. **Code Organization:**
   - Add: `UNOPS.Workflow/` submodule folder at solution root
   - Add: `UNOPS.PAO.Business/Workflow/` for PAO adapters
   - Add: `UNOPS.PAO.Business/Workflow/StateMachines/` for entity workflow definitions
   - Deprecate: `UNOPS.PAO.Business/Managers/WorkflowManager.cs` (replaced by submodule)

4. **Service Registration:**
   - Register workflow services in DI container
   - Register PAO-specific interface implementations
   - Configure WorkflowDbContext with connection string

5. **Angular Integration:**
   - Add workflow Angular component library
   - Update existing components to use new workflow API
   - Add workflow history display

---

### 5. User Stories

#### US-1: Developer Setting Up Workflow Infrastructure
**As a** PAO developer  
**I want to** integrate the UNOPS.Workflow submodule  
**So that** I have production-ready workflow infrastructure available for all future entity approvals

**Acceptance Criteria:**
- Git submodule is added and tracked in repository
- All workflow projects compile successfully
- WorkflowDbContext is registered and creates workflow schema automatically
- No breaking changes to existing PAO functionality
- Documentation is updated with integration details

---

#### US-2: Developer Implementing Entity Workflow
**As a** PAO developer  
**I want to** create a workflow state machine for an entity  
**So that** I can define valid stages and transitions without writing custom logic

**Acceptance Criteria:**
- Can create a StateMachine class for any entity (e.g., `OpportunityWorkflow`)
- Can define states with sequence, facing (Internal/External), and display names
- StateMachine is tracked in code (Git version control)
- Changes to state definitions don't require database migrations

---

#### US-3: Developer Configuring Stage Transitions
**As a** PAO developer  
**I want to** seed stage transition rules in the database  
**So that** I can control which stage changes are allowed, require approval, or need comments

**Acceptance Criteria:**
- Can create seeder class for StateMachineStageChanges
- Can specify FromStage, ToStage, ApprovalRequired, CommentRequired flags
- Can control Internal vs External user access to transitions
- Seeder is idempotent (safe to run multiple times)

---

#### US-4: Developer Testing Opportunity Workflow
**As a** PAO developer  
**I want to** validate the workflow integration with a working Opportunity example  
**So that** I can verify all components are correctly integrated

**Acceptance Criteria:**
- Example Opportunity workflow is implemented:
  * IDENTIFY & PROFILE → DECIDE (Opportunity Manager)
  * DECIDE → GO (DOA Holder, final stage)
  * DECIDE → NO GO (DOA Holder)
  * DECIDE → IDENTIFY & PROFILE (DOA Holder)
  * NO GO → IDENTIFY & PROFILE (Opportunity Manager, reopen)
- GO stage is final - no transitions out
- Role-based permissions control action availability
- Status changes correctly: Draft → Active (GO) or Draft → Closed (NO GO)
- Workflow actions are visible in UI
- Stage changes are logged in workflow.WorkflowLogs table
- Unit tests validate workflow operations

---

#### US-5: System Administrator Managing Workflows
**As a** system administrator  
**I want to** view and manage workflow configurations  
**So that** I can troubleshoot issues and update transition rules

**Acceptance Criteria:**
- Can query StateMachineStageChanges table to see all configured transitions
- Can query StateMachineStageChangeRoles to see permission mappings
- Can query WorkflowLogs to see complete audit trail
- Database schema is well-documented

---

#### US-6: Internal User Viewing Entity Workflow Status
**As an** internal PAO user  
**I want to** see the current workflow stage and available actions for an Opportunity  
**So that** I know what actions I can take

**Acceptance Criteria:**
- Opportunity detail page displays current stage
- Available actions are shown based on user permissions
- Workflow component shows stage progression
- User cannot perform actions they don't have permission for

---

#### US-7: Internal User Submitting for Approval (Future)
**As an** internal PAO user  
**I want to** submit an Opportunity for approval  
**So that** it can be reviewed by authorized approvers

**Acceptance Criteria:**
- Can click "Submit for Approval" action
- System validates approval configuration exists
- Pending approval is logged in WorkflowLogs
- Approvers are notified via email
- Stage shows as "Awaiting Approval"

**Note:** Full approval implementation is out of scope for this PRD (infrastructure only)

---

#### US-8: Developer Extending to Other Entities (Future)
**As a** PAO developer  
**I want to** easily add workflow to Partner or other entities  
**So that** I can reuse the infrastructure without rebuilding

**Acceptance Criteria:**
- Can add string Stage property to any entity
- Can create new StateMachine class
- Can add entity case to PaoEntityStageProvider
- Can add seeder for entity's stage changes
- No changes needed to workflow submodule

---

### 6. Functional Requirements

#### FR-1: Submodule Integration
1. Add UNOPS.Workflow as Git submodule in PAO repository root
2. Repository URL: `https://github.com/UNOPS-ITG/unops-workflow.git`
3. Submodule path: `business-partners-and-opportunities/UNOPS.Workflow`
4. Add project references to:
   - `UNOPS.Workflow.Business`
   - `UNOPS.Workflow.DataAccess`
   - `UNOPS.Workflow.Models`
5. Verify all projects compile successfully

#### FR-2: Entity Migration
1. Add `Stage` string property to `Opportunity` entity
2. Add MaxLength attribute: `[MaxLength(100)]`
3. Make Stage nullable initially for migration: `public string? Stage { get; set; }`
4. Remove `WorkflowStageId` property
5. Remove `WorkflowStage` navigation property
6. Create EF Core migration for these changes
7. Add data migration script to populate Stage from existing WorkflowStageId

#### FR-3: Deprecate Old Workflow System
1. Mark `WorkflowStage` entity with `[Obsolete]` attribute
2. Mark old `WorkflowLog` entity with `[Obsolete]` attribute
3. Mark `WorkflowManager` class with `[Obsolete]` attribute
4. Add deprecation comments with migration instructions
5. Do NOT delete old entities (maintain backward compatibility temporarily)

#### FR-4: Implement IWorkflowUserContext
1. Create `PaoWorkflowUserContext` class in `UNOPS.PAO.Business/Workflow/`
2. Implement `IWorkflowUserContext` interface from submodule
3. Properties to implement:
   - `CurrentUserId` - Get from ClaimTypes.NameIdentifier
   - `CurrentUserName` - Get from Identity.Name
   - `CurrentUserEmail` - Get from ClaimTypes.Email
   - `CurrentUserRoles` - Get from ClaimTypes.Role claims
   - `Environment` - Get from ASPNETCORE_ENVIRONMENT
   - `IsAuthenticated` - Check HttpContext.User.Identity.IsAuthenticated
4. Use IHttpContextAccessor for claims access
5. Return sensible defaults when user is not authenticated

#### FR-5: Implement IEntityStageProvider
1. Create `PaoEntityStageProvider` class in `UNOPS.PAO.Business/Workflow/`
2. Implement `IEntityStageProvider` interface from submodule
3. Methods to implement:
   - `GetCurrentStageAsync(entityName, entityId)` - Return entity's Stage property
   - `UpdateStageAsync(entityName, entityId, newStage, userId)` - Update Stage and audit fields
   - `IsEntityValidAsync(entityName, entityId)` - Check entity exists and !IsDeleted
   - `GetEntityDisplayNameAsync(entityName, entityId)` - Return entity Name or identifier
4. Support "opportunity" entity initially (use switch expression on entityName.ToLowerInvariant())
5. Design for easy extension to other entities
6. Use proper async/await patterns
7. Filter by !IsDeleted in all queries

#### FR-6: Implement IWorkflowApproverProvider
1. Create `PaoWorkflowApproverProvider` class in `UNOPS.PAO.Business/Workflow/`
2. Implement `IWorkflowApproverProvider` interface from submodule
3. Methods to implement:
   - `GetApproversAsync(entityName, entityId, fromStage, toStage)` - Return list of users who can approve
   - `GetApprovalConfigurationAsync(entityName, entityId, fromStage, toStage)` - Return approval config with roles
   - `GetTriggerConfigurationAsync(entityName, entityId, fromStage, toStage)` - Return trigger config with roles
   - `CanUserApproveAsync(entityName, entityId, userId, fromStage, toStage)` - Check specific user permission
4. Leverage PAO's existing EntityRole and EntityRolePerson tables
5. Query StateMachineStageChangeRoles from workflow schema
6. Join entity role assignments with workflow role permissions
7. Return empty lists for unconfigured transitions (no approvers = can't start workflow)

#### FR-7: Implement IWorkflowNotificationService
1. Create `PaoWorkflowNotificationService` class in `UNOPS.PAO.Business/Workflow/`
2. Implement `IWorkflowNotificationService` interface from submodule
3. Methods to implement:
   - `NotifyNewApprovalRequestAsync(notification)` - Email approvers about new request
   - `NotifyWorkflowCompletedAsync(notification)` - Email submitter about approval
   - `NotifyWorkflowRejectedAsync(notification)` - Email submitter about rejection
   - `NotifyWorkflowRecalledAsync(notification)` - Email approvers about recall
4. Use PAO's existing email service (IEmailService or similar)
5. Create email templates for each notification type
6. Include entity name, URL, performer, comment in emails
7. Handle multiple recipients gracefully

#### FR-8: Database Schema Setup
1. Configure WorkflowDbContext in Program.cs/Startup.cs
2. Add workflow database connection string (use same DB, different schema)
3. Register `AddWorkflowServices()` extension method in DI
4. Configure PostgreSQL storage: `.UsePostgreSqlStorage(connectionString, "workflow")`
5. Verify workflow schema is auto-created on startup
6. Verify 3 tables are created:
   - `workflow.StateMachineStageChanges`
   - `workflow.StateMachineStageChangeRoles`
   - `workflow.WorkflowLogs`
7. Add DbContext to DbContext factory if needed for migrations

#### FR-9: Service Registration
1. Register workflow services in DI container:
   ```csharp
   services.AddWorkflowServices(options =>
   {
       options.UsePostgreSqlStorage(connectionString, "workflow");
   });
   ```
2. Register PAO implementations:
   ```csharp
   services.AddScoped<IWorkflowUserContext, PaoWorkflowUserContext>();
   services.AddScoped<IEntityStageProvider, PaoEntityStageProvider>();
   services.AddScoped<IWorkflowApproverProvider, PaoWorkflowApproverProvider>();
   services.AddScoped<IWorkflowNotificationService, PaoWorkflowNotificationService>();
   ```
3. Ensure services are registered in correct order
4. Verify IHttpContextAccessor is registered

#### FR-10: Create Opportunity Workflow State Machine
1. Create `OpportunityWorkflow` class in `UNOPS.PAO.Business/Workflow/StateMachines/`
2. Define static StateMachine property
3. Include states:
   - "Identify & Profile" (Sequence: 1, Status: Draft)
   - "Decide" (Sequence: 2, Status: Draft)
   - "Go" (Sequence: 3, IsFinalStage: true, Status: Active)
   - "No Go" (Sequence: 4, IsFinalStage: false, Status: Closed)
4. Set EntityType = "Opportunity"
5. Configure facing (Internal) for all states
6. Note: Go is the ONLY final stage - No Go can be reopened
7. Role-based permissions (no approval workflow):
   - Opportunity Manager: Move to Decide, Reopen
   - DOA Holder: Move to Go, Move to No Go, Back to Identify & Profile
8. Follow GMS pattern for structure

#### FR-11: Seed Opportunity Stage Transitions
1. Create `OpportunityWorkflowSeeder` in `UNOPS.PAO.Business/Workflow/Seeders/`
2. Seed stage changes (role-based, no approval workflow):
   - IDENTIFY & PROFILE → DECIDE (Action: "Move to Decide")
     * Role Required: Opportunity Manager
     * Status: remains Draft
   - DECIDE → GO (Action: "Move to Go")
     * Role Required: DOA Holder
     * Status changes to Active
     * Note: GO is FINAL - no transitions out
   - DECIDE → NO GO (Action: "Move to No Go")
     * Role Required: DOA Holder
     * Status changes to Closed
   - DECIDE → IDENTIFY & PROFILE (Action: "Back to Identify & Profile")
     * Role Required: DOA Holder
     * Status: remains Draft
   - NO GO → IDENTIFY & PROFILE (Action: "Reopen")
     * Role Required: Opportunity Manager
     * Status changes to Draft
3. Note: No approval workflow - role determines action availability
4. Configure comment requirements for Go/No Go transitions
5. Set Internal access flags (all internal only)
6. Create seeder runner in Program.cs or seed controller
7. Make seeder idempotent (check existing records)

#### FR-12: Create API Endpoints
1. Create `WorkflowController` in `UNOPS.PAO.Presentation/Controllers/`
2. Implement endpoints:
   - `GET /api/workflow/{entityName}` - Get workflow stages for entity type
   - `GET /api/workflow/{entityName}/{id}` - Get current state and available actions
   - `POST /api/workflow` - Execute workflow action (stage change)
   - `GET /api/workflow/{entityName}/{id}/history` - Get stage change history
3. Use authorization attributes for permission checks
4. Return DTOs, never entities directly
5. Add to APIDictionary constants
6. Follow PAO controller patterns exactly

#### FR-13: Update OpportunityManager
1. Inject IWorkflowManager via IManagerWrapper
2. Update GetById to include Stage in response model
3. Add method to get workflow state: `GetWorkflowState(id)`
4. Add method to execute stage change: `ChangeStage(id, newStage, comment)`
5. Use WorkflowManager from submodule
6. Remove old WorkflowStage navigation property usage
7. Add workflow validation before stage changes

#### FR-14: Create Angular Workflow Component
1. Copy workflow Angular components from `UNOPS.Workflow/unops-workflow-angular/`
2. Place in `ClientApp/src/app/shared/reusables/components/workflow/`
3. Components needed:
   - `workflow.component.ts` - Main workflow display and actions
   - `workflow-history.component.ts` - Stage change history
   - `workflow.service.ts` - API service
4. Update API endpoints to match PAO's base URL
5. Add translation keys to i18n files

#### FR-15: Integrate Workflow in Opportunity UI
1. Update `opportunity-item.component.ts`
2. Import WorkflowComponent
3. Add workflow section to template
4. Pass entityName="opportunity" and entityId
5. Handle onStageChangeSuccess event (reload opportunity)
6. Show workflow based on permissions
7. Add beforeStageChange hook for validation (optional)

#### FR-16: Update Opportunity Models
1. Add `Stage` property to OpportunityModel DTO
2. Add `WorkflowState` property (optional, for current state details)
3. Remove `WorkflowStageId` from models
4. Update AutoMapper profiles
5. Ensure backward compatibility during migration

#### FR-17: Create Unit Tests
1. Test PaoWorkflowUserContext:
   - Test CurrentUserId extraction from claims
   - Test unauthenticated user handling
   - Test environment detection
2. Test PaoEntityStageProvider:
   - Test GetCurrentStageAsync for Opportunity
   - Test UpdateStageAsync updates Stage and audit fields
   - Test IsEntityValidAsync checks IsDeleted
   - Test unsupported entity types return null/false
3. Test OpportunityWorkflow:
   - Test StateMachine has correct states
   - Test state sequences
   - Test facing configurations
4. Test WorkflowController:
   - Test GET endpoints return correct data
   - Test POST endpoint validates and executes transitions
   - Test permission checks
5. Test workflow integration end-to-end:
   - Create test opportunity
   - Change stage using workflow API
   - Verify Stage property updated
   - Verify WorkflowLog created

#### FR-18: Documentation
1. Update README with workflow integration instructions
2. Document how to add workflow to new entities
3. Add code comments to interface implementations
4. Document seeder usage
5. Add troubleshooting guide
6. Include example workflow diagrams
7. Reference Migration-Guide-WorkflowStage-To-StateMachine.md

---

### 7. Non-Goals (Out of Scope)

This PRD specifically does NOT include:

1. ❌ **Full Approval Workflow Implementation** - Infrastructure only; no complete approve/reject/recall flows
2. ❌ **Email Template Design** - Basic email notifications only; no styled HTML templates
3. ❌ **Complex Multi-Level Approvals** - Simple approval setup; no escalation rules
4. ❌ **Workflow for Other Entities** - Only Opportunity example; Partner/Contact out of scope
5. ❌ **Migration of Historical Data** - WorkflowLog migration optional; focus on new workflow
6. ❌ **Workflow Analytics/Reporting** - No dashboards or reports on workflow metrics
7. ❌ **Workflow Automation** - No automatic stage transitions or scheduled actions
8. ❌ **External User Workflow UI** - Focus on internal users; external portal updates separate
9. ❌ **Workflow Permissions Management UI** - Role configuration via database seeders only
10. ❌ **Workflow Engine Modifications** - Use submodule as-is; no changes to workflow submodule code
11. ❌ **Complete Deprecation of Old System** - Old entities marked obsolete but not removed
12. ❌ **Production Deployment** - Development/test environment only initially

---

### 8. Design Considerations

#### 8.1 UI/UX Design

**Workflow Component Location:**
- Add workflow component below Opportunity header on detail page
- Display current stage prominently
- Show available actions as buttons
- Include workflow history section (collapsible)

**Stage Display:**
```
Current Stage: IDENTIFY & PROFILE

● IDENTIFY & PROFILE ──────○ DECIDE ──────○ GO/NO GO

Available Actions: [Move to Decide]
```

**Workflow History:**
```
┌─ Workflow History ──────────────────────────┐
│ Date         User          Action     Stage │
│ 2024-01-15   John Doe      Created    IDENTIFY & PROFILE │
└─────────────────────────────────────────────┘
```

#### 8.2 Component Integration Pattern

Follow PAO's established workflow component pattern (from component-development.mdc):

```typescript
@Component({
  selector: 'app-opportunity-item',
  imports: [WorkflowComponent]
})
export class OpportunityItemComponent {
  opportunityId = signal<number>(0);
  canChangeStage = signal<boolean>(false);

  handleStageChangeSuccess(): void {
    this.loadOpportunity(); // Reload after stage change
  }
}
```

```html
<app-workflow
  [entityName]="'opportunity'"
  [entityId]="opportunityId().toString()"
  [canChangeStage]="canChangeStage()"
  (onStageChangeSuccess)="handleStageChangeSuccess()"
/>
```

#### 8.3 API Design

**RESTful Endpoints:**

```
GET    /api/workflow/opportunity              → Get all stages for Opportunity entity
GET    /api/workflow/opportunity/123          → Get current state and actions for Opportunity #123
POST   /api/workflow                          → Execute stage change
GET    /api/workflow/opportunity/123/history  → Get stage change history
```

**POST Request Body:**
```json
{
  "entityName": "opportunity",
  "entityId": "123",
  "newStage": "DECIDE",
  "comment": "Moving to decision stage"
}
```

**Response:**
```json
{
  "success": true,
  "newStage": "DECIDE",
  "message": "Stage changed successfully"
}
```

---

### 9. Technical Considerations

#### 9.1 Database Considerations

**Schema Isolation:**
- Workflow tables in separate `workflow` schema
- PAO entities remain in `public` schema
- No foreign keys between schemas
- WorkflowDbContext manages workflow schema independently
- AppDbContext manages PAO entities

**Connection String:**
- Use same PostgreSQL database
- Single connection string shared between contexts
- Schema separation handled by EF Core

**Migration Strategy:**
```sql
-- Migration to add Stage to Opportunity
ALTER TABLE public."Opportunities" ADD COLUMN "Stage" VARCHAR(100);

-- Data migration (populate from existing WorkflowStageId)
UPDATE public."Opportunities" o
SET "Stage" = ws."Name"
FROM public."WorkflowStages" ws
WHERE o."WorkflowStageId" = ws."Id";

-- Later: Drop old columns (after validation)
-- ALTER TABLE public."Opportunities" DROP COLUMN "WorkflowStageId";
```

#### 9.2 Performance Considerations

**Query Optimization:**
- Index on `Opportunity.Stage` for filtering by stage
- Composite index on WorkflowLogs (EntityName, EntityId, CreatedDate)
- Use `.AsNoTracking()` for read-only queries
- Eager load related entities when needed

**Caching Strategy:**
- Cache StateMachine definitions (static, never change at runtime)
- Cache StateMachineStageChanges per entity type (rarely changes)
- Don't cache WorkflowLogs (always fetch fresh for audit trail)

#### 9.3 Security Considerations

**Authorization:**
- All workflow API endpoints require authentication
- Stage change permissions checked via IWorkflowApproverProvider
- Use PAO's existing authorization handlers
- Never expose workflow configuration to client (server-side only)

**Self-Approval Prevention:**
- Workflow engine prevents self-approval in Test/Production environments
- Development environment allows for testing

**Audit Trail:**
- All stage changes logged with user ID, timestamp, comment
- WorkflowLogs immutable (append-only)
- Denormalized user names for display independence

#### 9.4 Error Handling

**Validation Errors:**
- Invalid stage transitions return 400 Bad Request
- Missing approvers return 400 with descriptive message
- Entity not found returns 404

**Exception Handling:**
- Use PAO's BusinessException for business rule violations
- Log all errors with context (entity type, ID, user)
- Return user-friendly error messages

**Rollback Strategy:**
- Database transactions ensure Stage update and WorkflowLog creation are atomic
- Failed transitions don't leave partial data

#### 9.5 Testing Strategy

**Unit Tests:**
- Test each interface implementation independently
- Mock dependencies (DbContext, HttpContextAccessor)
- Test edge cases (null values, invalid IDs)
- Verify correct SQL queries generated

**Integration Tests:**
- Test full workflow cycle: create → stage change → verify log
- Test API endpoints with test database
- Verify WorkflowDbContext auto-creates schema
- Test seeder idempotency

**Manual Testing Checklist:**
- Create new Opportunity → verify Stage is null initially
- Change stage via UI → verify Stage updated and log created
- Check workflow history → verify entries shown correctly
- Test permission checks → verify unauthorized users blocked
- Test navigation → verify workflow component displays correctly

#### 9.6 Deployment Considerations

**Deployment Order:**
1. Deploy database migration (add Stage column)
2. Run data migration (populate Stage from WorkflowStageId)
3. Deploy backend code (new workflow services)
4. Run workflow seeders (populate stage transitions)
5. Deploy frontend code (new workflow components)
6. Verify workflow schema created automatically
7. Test stage changes in production

**Rollback Plan:**
- Keep WorkflowStageId column temporarily
- Keep old WorkflowLog table
- Can revert code changes without data loss
- Stage column can remain empty if rollback needed

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT` - Used by workflow for self-approval prevention
- No new environment variables required
- Use existing connection string

#### 9.7 Dependencies

**New NuGet Packages:**
- None (all provided by submodule)

**Submodule Dependencies:**
- Entity Framework Core (already in PAO)
- AutoMapper (already in PAO)
- Npgsql (already in PAO)

**Angular Dependencies:**
- None (workflow components are standalone)

#### 9.8 Code Quality Standards

**Follow PAO Patterns:**
- Inherit entities from `ModifiableDeletableEntity`
- Use `IManagerWrapper` for manager injection
- Return DTOs from controllers, never entities
- Use async/await throughout
- Filter by `!IsDeleted` in all queries
- Add proper XML comments to all public methods

**Follow C# Guidelines:**
- Use sealed classes where appropriate
- Use records for DTOs
- Use switch expressions for entity type routing
- Use `nameof()` for parameter names in exceptions
- Follow PAO naming conventions

**Code Organization:**
```
UNOPS.PAO.Business/Workflow/
├── PaoWorkflowUserContext.cs
├── PaoEntityStageProvider.cs
├── PaoWorkflowApproverProvider.cs
├── PaoWorkflowNotificationService.cs
├── StateMachines/
│   └── OpportunityWorkflow.cs
└── Seeders/
    └── OpportunityWorkflowSeeder.cs
```

---

### 10. Success Metrics

**Technical Metrics:**
1. ✅ All projects compile without errors
2. ✅ All unit tests pass (100% pass rate)
3. ✅ Workflow schema auto-created successfully
4. ✅ Zero breaking changes to existing functionality
5. ✅ Code coverage > 80% for new workflow code

**Functional Metrics:**
1. ✅ Opportunity can change stage via API
2. ✅ Stage changes logged in workflow.WorkflowLogs table
3. ✅ Workflow history displays correctly in UI
4. ✅ Example workflow works end-to-end:
   - IDENTIFY & PROFILE → DECIDE (Opportunity Manager)
   - DECIDE → GO (DOA Holder, final stage)
   - DECIDE → NO GO (DOA Holder)
   - DECIDE → IDENTIFY & PROFILE (DOA Holder)
   - NO GO → IDENTIFY & PROFILE (Opportunity Manager, reopen)
5. ✅ GO stage is final - no transitions out
6. ✅ Role-based permissions enforce correct access
7. ✅ Status changes correctly with stage transitions
5. ✅ User with permissions can change stage
6. ✅ User without permissions cannot change stage

**Quality Metrics:**
1. ✅ Code review approved by senior developer
2. ✅ Documentation complete and reviewed
3. ✅ No linter errors or warnings
4. ✅ All database migrations tested
5. ✅ Integration validated in test environment

**Timeline Metrics:**
1. ✅ Submodule integration: 1-2 hours
2. ✅ Interface implementations: 6-8 hours
3. ✅ API and UI integration: 4-6 hours
4. ✅ Testing and documentation: 3-4 hours
5. ✅ Total: 14-20 hours (2-3 days)

**Adoption Metrics (Post-Implementation):**
1. 📊 Number of entities using workflow (target: 1 initially, 3+ within 3 months)
2. 📊 Developer satisfaction with workflow integration ease
3. 📊 Time to add workflow to new entity (target: < 2 hours)

---

### 11. User Interface Mockups

The following mockups are based on the **actual GMS workflow component implementation** from `UNOPS.Workflow/unops-workflow-angular/`. These reflect the real structure used in production.

---

#### Mockup 1: Stage Workflow Component (Normal State - Not in Approval Workflow)

Based on `stage-workflow.component.html` - Uses `p-panel`, `p-tabs`, and `p-steps`.

```
┌─ Stage ─────────────────────────────────────────────────────────────────────────────────┬─────────────────────────┐
│                                                                                         │                         │
│  Stage                                                                  [Move to Decide ▼]                        │
│                                                                                                                   │
├───────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                                   │
│  ┌─ Overview ─────────┬─ Stage Change History ─┐                                                                 │
│  └────────────────────┴────────────────────────┘                                                                 │
│                                                                                                                   │
│     ┌─────────────────────┐      ┌─────────────────────┐      ┌─────────────────────┐      ┌─────────────────────┐│
│     │                     │      │                     │      │                     │      │                     ││
│     │   (●) IDENTIFY      │──────│   ( ) DECIDE        │──────│   ( ) GO            │──────│   ( ) NO GO         ││
│     │       & PROFILE     │      │                     │      │                     │      │                     ││
│     │                     │      │                     │      │                     │      │                     ││
│     └─────────────────────┘      └─────────────────────┘      └─────────────────────┘      └─────────────────────┘│
│                                                                                                                   │
└───────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- Header shows "Stage" title
- Action buttons via `p-splitButton` in header icons area
- `p-tabs` with "Overview" and "Stage Change History" tabs
- `p-steps` component shows stage progression (NOT custom dots/lines)

---

#### Mockup 2: Stage Workflow Component (In Workflow - Pending Approval)

When `workflowData()?.isInWorkflow == true`, shows approval pending tag and approvers tab.

```
┌─ Stage ─────────────────────────────────────────────────────────────────────────────────────────────────────┬─────┐
│                                                                                                             │     │
│  Stage    ┌──────────────────────┐   Current Stage : DECIDE    Next Stage : GO                              │[Recall]
│           │ ⚠ Approval Pending   │                                                                          │     │
│           └──────────────────────┘                                                                          │     │
│                                                                                                             │     │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┴─────┤
│                                                                                                                   │
│  ┌─ Overview ─────────┬─ Approvers ──────────────┬─ Stage Change History ─┐                                      │
│  └────────────────────┴──────────────────────────┴────────────────────────┘                                      │
│                                                                                                                   │
│     ┌─────────────────────┐      ┌─────────────────────┐      ┌─────────────────────┐      ┌─────────────────────┐│
│     │                     │      │                     │      │                     │      │                     ││
│     │   (●) IDENTIFY      │──────│   (●) DECIDE        │──────│   ( ) GO            │──────│   ( ) NO GO         ││
│     │       & PROFILE     │      │                     │      │                     │      │                     ││
│     │                     │      │                     │      │                     │      │                     ││
│     └─────────────────────┘      └─────────────────────┘      └─────────────────────┘      └─────────────────────┘│
│                                                                                                                   │
└───────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- `p-tag` with "Approval Pending" (severity="warn")
- Shows "Current Stage" and "Next Stage" labels
- Additional "Approvers" tab appears when in workflow
- Recall button shown if `workflowInfo()?.canRecall`

---

#### Mockup 3: Approvers Tab (When In Workflow)

Based on approvers table from `stage-workflow.component.html` lines 96-118.

```
┌─ Stage ─────────────────────────────────────────────────────────────────────────────────────────────────────┬─────┐
│                                                                                                             │     │
│  Stage    ┌──────────────────────┐   Current Stage : DECIDE    Next Stage : GO                              │[Recall]
│           │ ⚠ Approval Pending   │                                                                          │     │
│           └──────────────────────┘                                                                          │     │
│                                                                                                             │     │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┴─────┤
│                                                                                                                   │
│  ┌─ Overview ─────────┬─ Approvers ──────────────┬─ Stage Change History ─┐                                      │
│                       └──────────────────────────┘                                                               │
│                                                                                                                   │
│  ┌───────────────────────────────────────────────────────────────────────────────────────────────────────────────┐│
│  │ User                                              │ Role                                                      ││
│  ├───────────────────────────────────────────────────┼───────────────────────────────────────────────────────────┤│
│  │ Sarah Johnson                                     │ Regional Director                                         ││
│  ├───────────────────────────────────────────────────┼───────────────────────────────────────────────────────────┤│
│  │ Michael Chen                                      │ Senior Manager                                            ││
│  └───────────────────────────────────────────────────┴───────────────────────────────────────────────────────────┘│
│                                                                                                                   │
└───────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- `p-table` with two columns: "User" and "Role"
- Shows `getUserNameToDisplay(approver)` for user name
- Shows `approver.role` for role

---

#### Mockup 4: Stage Change History Tab

Based on stage change history table from `stage-workflow.component.html` lines 154-182.

```
┌─ Stage ─────────────────────────────────────────────────────────────────────────────────────────────────────┬───────────────────┐
│                                                                                                             │                   │
│  Stage                                                                                                      │[Move to Decide ▼] │
│                                                                                                             │                   │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────┴───────────────────┤
│                                                                                                                                 │
│  ┌─ Overview ─────────┬─ Stage Change History ─┐                                                                               │
│                       └────────────────────────┘                                                                               │
│                                                                                                                                 │
│  ┌────────────────┬───────────────┬────────────────────────┬───────────────┬──────────────────────────┬─────────────────────────┐│
│  │ From Stage     │ To Stage      │ Completed On           │ Action        │ Comment                  │ User                    ││
│  ├────────────────┼───────────────┼────────────────────────┼───────────────┼──────────────────────────┼─────────────────────────┤│
│  │ IDENTIFY &     │ DECIDE        │ 15-Jan-2024 10:30      │ Move to       │ All profile info         │ Jane Smith              ││
│  │ PROFILE        │               │                        │ Decide        │ collected                │                         ││
│  ├────────────────┼───────────────┼────────────────────────┼───────────────┼──────────────────────────┼─────────────────────────┤│
│  │ --             │ IDENTIFY &    │ 10-Jan-2024 14:15      │ Created       │ Initial creation         │ John Doe                ││
│  │                │ PROFILE       │                        │               │                          │                         ││
│  └────────────────┴───────────────┴────────────────────────┴───────────────┴──────────────────────────┴─────────────────────────┘│
│                                                                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- `p-table` with six columns: From Stage, To Stage, Completed On, Action, Comment, User
- Date format: `dd-MMM-yyyy HH:mm`
- User displayed via `getUserNameToDisplay(historyItem.user)`

---

#### Mockup 5: Workflow Action Buttons (workflow.component.html)

Based on `workflow.component.html` - shows different states.

**State A: Not In Workflow - Actions Available**
```
┌───────────────────────────────────────────┐
│                                           │
│  ┌─────────────────────┬───┐              │
│  │ Move to Decide      │ ▼ │              │   ← p-splitButton
│  └─────────────────────┴───┘              │
│                                           │
│  Dropdown items:                          │
│  ├── Go                                   │
│  ├── No Go                                │
│  └── Back to Profile                      │
│                                           │
└───────────────────────────────────────────┘
```

**State B: In Workflow - Approval Actions (for Approver)**
```
┌───────────────────────────────────────────────────────────────────────┐
│                                                                       │
│  ┌──────────────┐  ┌─────────────────────┐  ┌────────────────────┐   │
│  │    Recall    │  │      Approve        │  │       Reject       │   │
│  └──────────────┘  └─────────────────────┘  └────────────────────┘   │
│   (secondary)       (success/green)          (danger/red)            │
│                                                                       │
│  Only shown based on permissions:                                     │
│  - Recall shown if canRecall = true                                   │
│  - Approve shown if canApprove = true                                 │
│  - Reject shown if canReject = true                                   │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

---

#### Mockup 6: Comment Dialog

Based on `workflow.component.html` lines 43-76.

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   Comment                                                [  ✕ ] │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Comment                                                       │
│   (shown if mandatory: * )                                      │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                                                         │   │
│   │   All profile information has been gathered.            │   │
│   │   Partners confirmed and budget validated.              │   │
│   │   Ready to proceed to decision stage.                   │   │
│   │                                                         │   │
│   │                                                         │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│                           ┌──────────┐  ┌──────────────────┐    │
│                           │  Cancel  │  │       Save       │    │
│                           └──────────┘  └──────────────────┘    │
│                           (secondary)    (primary)              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- Dialog header: "Comment" (translated)
- Label: "Comment" with optional `*` for mandatory
- `pTextarea` with 5 rows
- Footer: Cancel (secondary) + Save (primary) buttons
- Width: 25rem

---

#### Mockup 7: Full Page Context - GMS Funding Opportunity Pattern

Based on `fundingOpportunityItem.component.html` showing how the workflow is placed.

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                 │
│  💰 FO-2024-001 - South Sudan Water Infrastructure Development                                                  │
│     Funding Opportunity                                                                             [Clone]    │
│                                                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                                 │
│  ┌─ Requirements Validation (if applicable) ────────────────────────────────────────────────────────────────┐  │
│  │  (Validation component shown before workflow if canTriggerWorkflow is true)                              │  │
│  └──────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                 │
│  ┌─ Stage ──────────────────────────────────────────────────────────────────────────────────────┬─────────────┐│
│  │                                                                                              │             ││
│  │  Stage                                                                                       │[Publish ▼] ││
│  │                                                                                              │             ││
│  ├──────────────────────────────────────────────────────────────────────────────────────────────┴─────────────┤│
│  │                                                                                                            ││
│  │  ┌─ Overview ─────────┬─ Stage Change History ─┐                                                          ││
│  │  └────────────────────┴────────────────────────┘                                                          ││
│  │                                                                                                            ││
│  │  ┌───────────────┐   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐        ││
│  │  │(●) Not yet    │───│( ) Open       │───│( ) Evaluation │───│( ) Closed     │───│( ) Cancelled  │        ││
│  │  │    open       │   │               │   │               │   │               │   │               │        ││
│  │  └───────────────┘   └───────────────┘   └───────────────┘   └───────────────┘   └───────────────┘        ││
│  │                                                                                                            ││
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────┘│
│                                                                                                                 │
│  ┌─ Proposal Statistics ────────────────────────────────────────────────────────────────────────────────────┐  │
│  │  (Statistics component)                                                                                   │  │
│  └──────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                 │
│  ┌─ Publicly Available Link ────────────────────────────────────────────────────────────────────────────────┐  │
│  │  (Public URL section)                                                                                     │  │
│  └──────────────────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐│
│  │ ┌─ Setup ─┬─ Revisions ─┬─ Review Committee ─┬─ Proposals ─┬─ Review/Evaluation ─┬─ Comments ─┬─ ... ─┐   ││
│  │ └─────────┴─────────────┴────────────────────┴─────────────┴─────────────────────┴────────────┴───────┘   ││
│  │                                                                                                            ││
│  │  (Tab content - Setup form, etc.)                                                                          ││
│  │                                                                                                            ││
│  └────────────────────────────────────────────────────────────────────────────────────────────────────────────┘│
│                                                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Key Elements (from actual code):**
- Page header with icon, reference number, name, and entity type
- `app-stage-workflow` component placed BEFORE main tabs
- Inputs: `entityName`, `entityId`, `canChangeStage`, `beforeStageChange`, `customStageChangeHandler`
- Output: `onStageChangeSuccess` event

---

#### Mockup 8: PAO Opportunity Page with Workflow Integration

Applying GMS pattern to PAO's existing opportunity-view layout.

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                 │
│  ← Back                                                                                                         │
│                                                                                                                 │
│  South Sudan Water Infrastructure Development Project                                                           │
│  ══════════════════════════════════════════════════════                     ┌─────────┐ ┌─────────────────────┐ │
│  ID: 123  |  Manager: Jane Smith  |  Org Unit: AFRO  |  Mar 15, 2024       │ Active  │ │ IDENTIFY & PROFILE  │ │
│                                                                             └─────────┘ └─────────────────────┘ │
│                                                                                                                 │
│  ┌──────────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────┐  │
│  │ 📊 Analysis  │ │ 📄 Overview│ │ 🎯 What  │ │ ❓ Why   │ │ 👥 Who   │ │ 👷 Team  │ │ 🌍 Where │ │ More... ▼ │  │
│  └──────────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘ └───────────┘  │
│                                                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                                 │
│ ┌──────────┐ ┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│ │          │ │                                                                                                  │
│ │ 📁       │ │┌─ Stage ───────────────────────────────────────────────────────────────────────┬───────────────┐│
│ │Documents │ ││                                                                               │               ││
│ │          │ ││ Stage                                                                         │[Move to Decide ▼]│
│ │──────────│ ││                                                                               │               ││
│ │          │ │├───────────────────────────────────────────────────────────────────────────────┴───────────────┤│
│ │📄 ToR.pdf│ ││                                                                                               ││
│ │          │ ││ ┌─ Overview ─────────┬─ Stage Change History ─┐                                              ││
│ │📄 Budget │ ││ └────────────────────┴────────────────────────┘                                              ││
│ │ .xlsx    │ ││                                                                                               ││
│ │          │ ││ ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐           ││
│ │[📎Upload]│ ││ │(●) IDENTIFY     │──│( ) DECIDE       │──│( ) GO           │──│( ) NO GO        │           ││
│ │[🔗 Link] │ ││ │    & PROFILE    │  │                 │  │                 │  │                 │           ││
│ │          │ ││ └─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────────┘           ││
│ │  « Hide  │ ││                                                                                               ││
│ │          │ │└───────────────────────────────────────────────────────────────────────────────────────────────┘│
│ └──────────┘ │                                                                                                  │
│              │┌─ 📊 Analysis ─────────────────────────────────────────────────────────────────────────────────┐│
│              ││ (Analysis section content)                                                                     ││
│              │└────────────────────────────────────────────────────────────────────────────────────────────────┘│
│              │                                                                                                  │
│              │┌─ 📄 Overview ─────────────────────────────────────────────────────────────── [✏️ Edit] ────────┐│
│              ││ (Overview section content)                                                                     ││
│              │└────────────────────────────────────────────────────────────────────────────────────────────────┘│
│              │                                                                                                  │
│              │ ... (remaining sections) ...                                                                     │
│              │                                                                                                  │
│              └──────────────────────────────────────────────────────────────────────────────────────────────────┘
│                                                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

**Implementation Notes - Based on Actual GMS Code:**

1. **Component Structure:**
   - `app-stage-workflow` is the main component (wrapper)
   - Contains `app-workflow` internally for action buttons
   - Uses `p-panel`, `p-tabs`, `p-steps`, `p-table` from PrimeNG

2. **Workflow States:**
   - `isInWorkflow = false`: Shows `p-splitButton` for stage actions
   - `isInWorkflow = true`: Shows Recall/Approve/Reject buttons based on permissions

3. **Tabs:**
   - Always: "Overview" (p-steps), "Stage Change History" (p-table)
   - When in workflow: Additional "Approvers" tab

4. **History Table Columns:**
   - From Stage, To Stage, Completed On, Action, Comment, User
   - Date format: `dd-MMM-yyyy HH:mm`

5. **Approvers Table Columns:**
   - User, Role (only 2 columns)

6. **Dialog:**
   - Simple comment dialog with textarea
   - Cancel + Save buttons
   - Mandatory comment indicated by `*`

---

### 12. Open Questions

1. **Email Service Integration**
   - Q: Does PAO have an existing `IEmailService` or similar?
   - Action: Confirm email service interface and integrate accordingly
   - Fallback: Use console logging for email notifications initially

2. **EntityRole Configuration**
   - Q: Should we create new EntityRole records for Opportunity workflow roles (e.g., "Opportunity_Approver")?
   - Action: Review existing EntityRole codes and determine naming convention
   - Decision Needed: Who should be the default approver for testing?

3. **Stage Transition Permissions**
   - Q: For the example workflow, which roles should be able to approve "Go" and "No Go" decisions?
   - Action: Define initial role permissions for testing
   - Suggestion: Use existing admin roles for initial testing

4. **WorkflowStageId Data Migration**
   - Q: Should we migrate existing Opportunity.WorkflowStageId data to Stage field immediately?
   - Action: Create data migration script or leave Stage null initially?
   - Decision Needed: Timeline for complete deprecation of WorkflowStageId

5. **Frontend Permissions**
   - Q: How should the Angular component determine if user can change stage?
   - Action: Add `canChangeStage` flag to OpportunityModel?
   - Alternative: Check permissions in component using auth service?

6. **Notification Recipients**
   - Q: For the example workflow, who should receive email notifications?
   - Action: Define recipient logic (role-based? entity-specific?)
   - Suggestion: Use EntityRolePerson assignments for Opportunity

7. **Testing Data**
   - Q: Should we seed test opportunities with different stages?
   - Action: Create test data seeder for development environment?
   - Benefit: Easier to validate workflow UI

8. **Backward Compatibility Window**
   - Q: How long should we keep WorkflowStage/WorkflowLog tables before removal?
   - Action: Define timeline for complete deprecation
   - Suggestion: 1-2 releases (2-3 months)

9. **Angular Component Customization**
   - Q: Should we customize the workflow Angular component for PAO branding?
   - Action: Use as-is from submodule or create PAO-specific wrapper?
   - Decision: Follow GMS pattern (use as-is) or customize?

10. **Performance Baseline**
    - Q: Should we establish performance benchmarks before migration?
    - Action: Measure current Opportunity load times for comparison
    - Benefit: Quantify any performance impact

---

## Next Steps

After PRD approval:

1. ✅ **Review and approve PRD** with stakeholders
2. ✅ **Create task list** using generate-tasks.mdc rule
3. ✅ **Set up development environment** with Git submodule
4. ✅ **Begin Phase 1**: Add UNOPS.Workflow submodule
5. ✅ **Implement interfaces** in order: UserContext → EntityStageProvider → ApproverProvider → NotificationService
6. ✅ **Create example workflow** for Opportunity
7. ✅ **Test integration** thoroughly
8. ✅ **Update documentation**
9. ✅ **Code review** and QA
10. ✅ **Deploy to test environment**

**Estimated Total Time:** 14-20 hours (2-3 days for one developer)

**Dependencies:** Access to UNOPS.Workflow repository, database access, test environment

**Risks:** 
- Migration complexity (Medium) - Mitigated by following GMS patterns
- Breaking changes (Low) - Keeping old entities during transition
- Performance impact (Low) - Separate schema minimizes impact

---

## Appendix

### A. Reference Documents

1. **Migration-Guide-WorkflowStage-To-StateMachine.md** - Comprehensive migration guide (2607 lines)
2. **GMS Workflow Implementation** - Production reference in business-gms-plus
3. **UNOPS.Workflow README.md** - Submodule documentation
4. **PAO Component Development Guide** - `.cursor/rules/component-development.mdc`
5. **PAO .NET Implementation Guide** - `.cursor/rules/dotnet-implementation.mdc`

### B. Key Submodule Files to Review

```
UNOPS.Workflow/
├── README.md                                    ← Overview and integration guide
├── UNOPS.Workflow.Business/
│   ├── Interfaces/
│   │   ├── IWorkflowManager.cs                 ← Core workflow operations
│   │   ├── IWorkflowUserContext.cs             ← To implement
│   │   ├── IEntityStageProvider.cs             ← To implement
│   │   ├── IWorkflowApproverProvider.cs        ← To implement
│   │   └── IWorkflowNotificationService.cs     ← To implement
│   └── Managers/
│       └── WorkflowManager.cs                   ← Core implementation
├── UNOPS.Workflow.DataAccess/
│   ├── WorkflowDbContext.cs                    ← Separate context
│   └── Migrations/                             ← Auto-created migrations
├── UNOPS.Workflow.Domain/
│   └── Entities/
│       ├── WorkflowLog.cs                      ← Audit trail
│       ├── StateMachineStageChange.cs          ← Transition rules
│       └── StateMachineStageChangeRole.cs      ← Role permissions
└── UNOPS.Workflow.Models/
    ├── StateMachine.cs                         ← State machine definition
    ├── State.cs                                ← State/stage definition
    └── WorkflowNotification.cs                 ← Email notification model
```

### C. PAO Implementation Files to Create

```
business-partners-and-opportunities/
├── UNOPS.Workflow/                             ← Git submodule (new)
├── UNOPS.PAO.Business/
│   └── Workflow/                               ← New folder
│       ├── PaoWorkflowUserContext.cs           ← New
│       ├── PaoEntityStageProvider.cs           ← New
│       ├── PaoWorkflowApproverProvider.cs      ← New
│       ├── PaoWorkflowNotificationService.cs   ← New
│       ├── StateMachines/                      ← New folder
│       │   └── OpportunityWorkflow.cs          ← New
│       └── Seeders/                            ← New folder
│           └── OpportunityWorkflowSeeder.cs    ← New
├── UNOPS.PAO.Presentation/
│   └── Controllers/
│       └── WorkflowController.cs               ← New
├── UNOPS.PAO.Domain/
│   └── Entities/
│       ├── Opportunity.cs                      ← Modify (add Stage)
│       ├── WorkflowStage.cs                    ← Mark [Obsolete]
│       └── WorkflowLog.cs                      ← Mark [Obsolete]
├── UNOPS.PAO.Models/
│   └── OpportunityModel.cs                     ← Modify (add Stage)
└── ClientApp/
    └── src/app/
        └── shared/reusables/components/
            └── workflow/                        ← Copy from submodule
                ├── workflow.component.ts        ← New
                ├── workflow-history.component.ts ← New
                └── workflow.service.ts          ← New
```

### D. Database Schema After Migration

```sql
-- public schema (PAO entities)
public."Opportunities"
  - Id (int, PK)
  - Name (varchar)
  - Stage (varchar(100))  ← NEW!
  - WorkflowStageId (int) ← DEPRECATED, will remove later
  - Status (int)
  - IsDeleted (bool)
  - ...

-- workflow schema (auto-created by submodule)
workflow."StateMachineStageChanges"
  - Id (int, PK)
  - EntityName (varchar)
  - FromStage (varchar)
  - ToStage (varchar)
  - Name (varchar)       -- Action name
  - ApprovalRequired (bool)
  - CommentRequired (bool)
  - CommentOptional (bool)
  - Internal (bool)
  - External (bool)
  - Sequence (int)
  - Status (int)
  - ...audit fields...

workflow."StateMachineStageChangeRoles"
  - Id (int, PK)
  - EntityName (varchar)
  - FromStage (varchar)
  - ToStage (varchar)
  - RoleId (int)
  - RoleName (varchar)
  - CanTrigger (bool)
  - CanApprove (bool)
  - ...audit fields...

workflow."WorkflowLogs"
  - Id (int, PK)
  - EntityName (varchar)
  - EntityId (varchar)
  - Stage (varchar)
  - NewStage (varchar)
  - Action (varchar)
  - Comment (varchar)
  - UserId (int)
  - UserName (varchar)
  - Role (varchar)
  - CompletedOn (timestamp)
  - RequiresApproval (bool)
  - Status (int)
  - ...audit fields...
```

### E. Example Opportunity Workflow Configuration

**State Machine Definition** (`OpportunityWorkflow.cs`):
```csharp
public static StateMachine StateMachine => new()
{
    EntityType = "Opportunity",
    States = new[]
    {
        new State 
        { 
            StageCode = "IDENTIFY & PROFILE",
            DisplayName = "Identify & Profile",
            Sequence = 1,
            Facing = Facing.Internal,
            IsFinalStage = false
            // Status: Draft
        },
        new State 
        { 
            StageCode = "DECIDE",
            DisplayName = "Decide",
            Sequence = 2,
            Facing = Facing.Internal,
            IsFinalStage = false
            // Status: Draft
        },
        new State 
        { 
            StageCode = "GO",
            DisplayName = "Go",
            Sequence = 3,
            Facing = Facing.Internal,
            IsFinalStage = true  // FINAL - No transitions out
            // Status: Active
        },
        new State 
        { 
            StageCode = "NO GO",
            DisplayName = "No Go",
            Sequence = 4,
            Facing = Facing.Internal,
            IsFinalStage = false  // Can be reopened
            // Status: Closed
        }
    }
};
```

**Stage Transitions** (Seeded data):
```
1. IDENTIFY & PROFILE → DECIDE
   Action: "Move to Decide"
   Role Required: Opportunity Manager
   CommentRequired: false
   Status Change: None (remains Draft)

2. DECIDE → GO
   Action: "Move to Go"
   Role Required: DOA Holder
   CommentRequired: true
   Status Change: Draft → Active
   Note: GO is FINAL - no further transitions

3. DECIDE → NO GO
   Action: "Move to No Go"
   Role Required: DOA Holder
   CommentRequired: true
   Status Change: Draft → Closed

4. DECIDE → IDENTIFY & PROFILE
   Action: "Back to Identify & Profile"
   Role Required: DOA Holder
   CommentRequired: false
   Status Change: None (remains Draft)

5. NO GO → IDENTIFY & PROFILE
   Action: "Reopen"
   Role Required: Opportunity Manager
   CommentRequired: false
   Status Change: Closed → Draft
```

**Workflow Diagram:**
```
                    ┌────────────────────────┐
        ┌──────────►│  IDENTIFY & PROFILE    │◄────────────────┐
        │           │      (Draft)           │                 │
        │           └───────────┬────────────┘                 │
        │                       │                              │
        │                       │ Move to Decide               │
        │                       │ [Opp Manager]                │
        │                       ▼                              │
        │               ┌───────────────┐                      │
        │               │    DECIDE     │──────────────────────┘
        │               │    (Draft)    │  Back to Identify & Profile
        │               └───────┬───────┘  [DOA Holder]
        │                       │
        │           ┌───────────┴───────────┐
        │           │                       │
        │           │ Move to Go            │ Move to No Go
        │           │ [DOA Holder]          │ [DOA Holder]
        │           ▼                       ▼
        │   ┌───────────────┐       ┌───────────────┐
        │   │      GO       │       │    NO GO      │
        │   │   (Active)    │       │   (Closed)    │
        │   │   ★ FINAL ★   │       └───────┬───────┘
        │   └───────────────┘               │
        │                                   │ Reopen
        │                                   │ [Opp Manager]
        └───────────────────────────────────┘

Legend:
  ★ FINAL ★    = No transitions possible from this stage
  [Role]       = Role required to perform the action
  Opp Manager  = Opportunity Manager
  DOA Holder   = Delegation of Authority holder
```

**Status Mapping:**
| Stage               | Opportunity.Status |
|---------------------|-------------------|
| Identify & Profile  | Draft             |
| Decide              | Draft             |
| Go                  | Active            |
| No Go               | Closed            |

**Role Permissions (No Approval Workflow):**
| Transition                        | Role Required       |
|-----------------------------------|---------------------|
| Identify & Profile → Decide       | Opportunity Manager |
| Decide → Go                       | DOA Holder          |
| Decide → No Go                    | DOA Holder          |
| Decide → Identify & Profile       | DOA Holder          |
| No Go → Identify & Profile        | Opportunity Manager |
| Go → (none)                       | N/A (final stage)   |

**Note:** There is no approval workflow. The user's role determines which actions they can perform directly.

### F. Glossary

- **StateMachine**: Code-based definition of all possible states/stages for an entity type
- **State**: A stage in the workflow (e.g., "IDENTIFY & PROFILE", "DECIDE")
- **StateMachineStageChange**: Database record defining an allowed transition between states
- **WorkflowLog**: Audit trail entry recording a stage change action
- **Facing**: Enum defining whether a state/action is visible to Internal users, External users, or TwoFace (both)
- **ApprovalRequired**: Flag indicating a transition needs approval workflow (Initiate → Approve/Reject/Recall)
- **EntityStageProvider**: PAO implementation that reads/updates Stage property on entities
- **WorkflowApproverProvider**: PAO implementation that determines who can approve transitions
- **WorkflowUserContext**: PAO implementation providing current user information
- **WorkflowNotificationService**: PAO implementation that sends workflow emails
- **Submodule**: Git feature to include another repository within a repository as a folder

---

**Document Version:** 1.0  
**Created:** 2024-01-16  
**Author:** AI Assistant  
**Status:** Draft - Pending Review  
**Estimated Effort:** 14-20 hours (2-3 days)
