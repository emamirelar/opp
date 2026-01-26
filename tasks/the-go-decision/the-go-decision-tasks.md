# The Go Decision - Implementation Tasks

> **IMPORTANT**: This task list depends on the completion of the "Send Opportunity for Go Decision" feature (`tasks/send-opportunity-for-go-decision/`). Tasks 1.0-8.0 from that PRD MUST be completed before starting this implementation.

## Relevant Files

### Backend Files (.NET Core)

**Domain:**
- `UNOPS.PAO.Domain/Entities/Opportunity.cs` - MODIFY: Add ExecutiveId field and navigation property

**Models:**
- `UNOPS.PAO.Models/Workflow/WorkflowModels.cs` - MODIFY: Add ApproveWorkflowRequest, RejectWorkflowRequest, PendingApprovalResponse

**Managers:**
- `UNOPS.PAO.Business/Managers/OpportunityManager.cs` - MODIFY: Add AssignExecutiveAsync(), GetExecutivesForOrgUnitAsync(), immutability checks
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` - MODIFY: Add immutability checks to overridden methods (if applicable)
- `UNOPS.PAO.Business/Managers/NotificationManager.cs` - EXISTS: Use existing CreateNotification method

**Workflow Adapters:**
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` - MODIFY: Add NotificationManager dependency, create in-system notifications, add CC recipients to emails

**Controllers:**
- `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` - MODIFY: Enhance approve/reject endpoints, add pending-approvals endpoint
- `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs` - MODIFY: Add executives-for-orgunit endpoint (if not in WorkflowController)

**Migrations:**
- `UNOPS.PAO.UNOPSDataAccess/Migrations/[timestamp]_AddExecutiveIdToOpportunity.cs` - NEW: EF Core migration

**Mapping:**
- `UNOPS.PAO.Business/Managers/Mapping/MappingProfile.cs` - MODIFY: Add Executive mapping to OpportunityModel if needed

### Backend - Unit Tests

- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/OpportunityImmutabilityTests.cs` - NEW: Immutability enforcement tests
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowNotificationServiceTests.cs` - NEW: In-system notification and CC recipient tests
- `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs` - MODIFY: Add approve/reject endpoint enhancement tests (file already exists)

### Frontend Files (Angular)

**New Components (in opportunity feature folder - entity-specific dialogs):**
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.html` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.scss` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.html` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.scss` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.html` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.scss` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.spec.ts` - NEW

**Modified Components:**
- `UNOPS.PAO.ClientApp/src/app/features/home/components/home-dashboard/home-dashboard.component.ts` - MODIFY: Add workflow tasks to Actions Required card
- `UNOPS.PAO.ClientApp/src/app/features/home/components/home-dashboard/home-dashboard.component.html` - MODIFY: Display workflow approval tasks
- `UNOPS.PAO.ClientApp/src/app/layouts/components/topbar/topbar.component.ts` - MODIFY: Filter/style workflow notifications in notification bell
- `UNOPS.PAO.ClientApp/src/app/layouts/components/topbar/topbar.component.html` - MODIFY: Style for workflow notifications
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts` - MODIFY: Add customStageChangeHandler, decision-maker UI, immutability state
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.html` - MODIFY: Decision panels integration, read-only state

**Services:**
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/services/workflow.service.ts` - MODIFY: Add getPendingApprovalsForUser() method
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/services/opportunity.service.ts` - MODIFY: Add getExecutivesForOrgUnit(), approveOpportunity(), rejectOpportunity() methods (entity-specific workflow actions)

**Models:**
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/models/workflow.models.ts` - MODIFY: Add PendingApprovalModel interface
- `UNOPS.PAO.ClientApp/src/app/shared/models/opportunity.model.ts` - MODIFY: Add `isImmutable?: boolean` property to EntityPermissions interface, add GoDecisionPayload and NoGoDecisionPayload interfaces (opportunity-specific request models)

**Translation Files:**
- `UNOPS.PAO.ClientApp/src/assets/i18n/en.json` - MODIFY: Add decision dialog translation keys
- `UNOPS.PAO.ClientApp/src/assets/i18n/span.json` - MODIFY: Add decision dialog translation keys (Spanish)
- `UNOPS.PAO.ClientApp/src/assets/i18n/fr.json` - MODIFY: Add decision dialog translation keys
- `UNOPS.PAO.ClientApp/src/assets/i18n/pt.json` - MODIFY: Add decision dialog translation keys

### Frontend - Unit Tests

- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/services/workflow.service.spec.ts` - MODIFY: Add getPendingApprovalsForUser() tests (file created by prerequisite PRD Task 5.0)
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/services/opportunity.service.spec.ts` - NEW: Create test file with getExecutivesForOrgUnit, approveOpportunity, rejectOpportunity tests

### Notes

- This feature depends on the "Send Opportunity for Go Decision" prerequisite PRD (Tasks 1.0-8.0)
- The prerequisite PRD Task 5.0 creates the basic `PaoWorkflowNotificationService` email sending - this PRD enhances it with in-system notifications and CC recipients
- The prerequisite PRD Task 4.0 creates basic `WorkflowController.Approve()` and `WorkflowController.Reject()` endpoints - this PRD enhances them with structured requests
- Unit tests should be placed alongside the code files they are testing
- All backend unit tests use InMemory database and Moq for mocking
- All frontend unit tests use TestBed and HttpTestingController
- Run tests as needed during development

---

## ⚠️ CRITICAL Testing Requirements

### Testing Philosophy
- Every new component, service, and manager method MUST have corresponding unit tests
- Tests must compile and run without errors before marking tasks as complete
- Test coverage should include happy path, error cases, and edge cases

### Required Testing Tools

**Backend (.NET Core):**
- InMemory database for data access testing
- Moq for mocking dependencies
- xUnit as the testing framework
- FluentAssertions for readable assertions

**Frontend (Angular):**
- TestBed for component testing
- HttpTestingController for service HTTP calls
- Jasmine/Karma for test execution
- Signal testing utilities for Angular 19 signals

### Mandatory Verification Steps
1. All tests must compile without errors
2. All tests must pass when run
3. No skipped or ignored tests without documented reason
4. Code coverage should target critical business logic

---

## Tasks

- [ ] **1.0 Backend: Data Model & Migration**
  > Add ExecutiveId field to Opportunity entity and create database migration for storing the assigned Executive.

  - [ ] 1.1 Add `ExecutiveId` nullable int field to `Opportunity.cs` entity
    - Add `public int? ExecutiveId { get; set; }` property
    - Add `[ForeignKey(nameof(ExecutiveId))]` attribute
    - Add navigation property: `public virtual PAOUser? Executive { get; set; }`
  - [ ] 1.2 Configure ExecutiveId relationship in `UNOPSAppDbContext.OnModelCreating()`
    - Add entity configuration for Opportunity → PAOUser relationship via ExecutiveId
    - Set delete behavior to `SetNull` (Executive deletion shouldn't delete Opportunity)
  - [ ] 1.3 Generate EF Core migration using CLI
    - Run: `dotnet ef migrations add AddExecutiveIdToOpportunity --context UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess`
    - Verify migration includes `ExecutiveId` column with foreign key to `PAOUsers` table
  - [ ] 1.4 Update `OpportunityModel` to include Executive information (if needed for API responses)
    - Add `ExecutiveId` and `ExecutiveName` properties to model
  - [ ] 1.5 Update `MappingProfile.cs` to map Executive navigation property
    - Add mapping for `ExecutiveName` from `Executive.DisplayName` or similar
  - [ ] 1.6 Verify migration applies cleanly to local database
    - Run: `dotnet ef database update --context UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess`

- [ ] **2.0 Backend: Workflow Request Models & Endpoint Enhancements**
  > Create ApproveWorkflowRequest, RejectWorkflowRequest models and enhance the existing WorkflowController approve/reject endpoints (from prerequisite Task 4.0) with structured validation for rationale, confirmation acknowledgment, and Executive assignment.

  - [ ] 2.1 Create `ApproveWorkflowRequest` model in `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`
    - Add `EntityName` (required string) property
    - Add `EntityId` (required int) property
    - Add `Rationale` (required string) property - decision rationale stored in WorkflowLog.Comment
    - Add `ConfirmationAcknowledged` (bool) property - user acknowledged confirmation statement
    - Add `ExecutiveId` (int) property - required for Opportunity approvals
  - [ ] 2.2 Create `RejectWorkflowRequest` model in `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`
    - Add `EntityName` (required string) property
    - Add `EntityId` (required int) property
    - Add `Rationale` (required string) property - decision rationale
    - Add `ConfirmationAcknowledged` (bool) property
  - [ ] 2.3 Update `WorkflowController.Approve()` to use `ApproveWorkflowRequest`
    - Change parameter from existing request type to `ApproveWorkflowRequest`
    - Add validation: return BadRequest if `Rationale` is empty
    - Add validation: return BadRequest if `ConfirmationAcknowledged` is false
    - Add validation: return BadRequest if `EntityName == "Opportunity"` and `ExecutiveId <= 0`
    - Pass `Rationale` to `_workflowManager.Approve()` as comment parameter
  - [ ] 2.4 Add `AssignExecutiveAsync()` method to `OpportunityManager`
    - Method signature: `Task AssignExecutiveAsync(int opportunityId, int executiveId)`
    - Fetch opportunity by ID, throw `KeyNotFoundException` if not found
    - Set `opportunity.ExecutiveId = executiveId`
    - Save changes to database
  - [ ] 2.5 Call `AssignExecutiveAsync()` in `WorkflowController.Approve()` for Opportunity approvals
    - After successful `_workflowManager.Approve()` call
    - Only call when `EntityName == "Opportunity"` and `ExecutiveId > 0`
  - [ ] 2.6 Update `WorkflowController.Reject()` to use `RejectWorkflowRequest`
    - Change parameter from existing request type to `RejectWorkflowRequest`
    - Add validation: return BadRequest if `Rationale` is empty
    - Add validation: return BadRequest if `ConfirmationAcknowledged` is false
    - Pass `Rationale` to `_workflowManager.Reject()` as comment parameter
  - [ ] 2.7 Add unit tests for enhanced approve endpoint in `WorkflowControllerTests.cs`
    - Test: Approve with valid ApproveWorkflowRequest succeeds
    - Test: Approve without rationale returns BadRequest
    - Test: Approve without confirmation acknowledgment returns BadRequest
    - Test: Approve Opportunity without ExecutiveId returns BadRequest
    - Test: Approve assigns ExecutiveId to Opportunity record
  - [ ] 2.8 Add unit tests for enhanced reject endpoint in `WorkflowControllerTests.cs`
    - Test: Reject with valid RejectWorkflowRequest succeeds
    - Test: Reject without rationale returns BadRequest
    - Test: Reject without confirmation acknowledgment returns BadRequest
  - [ ] 2.9 Verify all tests compile and run successfully with no errors

- [ ] **3.0 Backend: Pending Approvals API & Executive Lookup**
  > Create two lookup APIs: (1) Pending workflow approvals endpoint for current user to support Actions Required card (FR-1), and (2) Executive lookup endpoint to retrieve Director/Manager/OiC options for org unit to support Approve dialog dropdown (FR-4).

  - [ ] 3.1 Create `PendingApprovalResponse` model in `UNOPS.PAO.Models/Workflow/WorkflowModels.cs`
    - Add `EntityName` (string) property
    - Add `EntityId` (int) property
    - Add `EntityDisplayName` (string) property
    - Add `CurrentStage` (string) property
    - Add `PendingStage` (string) property
    - Add `SubmittedBy` (string) property - submitter display name
    - Add `SubmittedOn` (DateTime) property
    - Add `OrgUnitName` (string) property
  - [ ] 3.2 Add `GetPendingTasksForApproverAsync()` method to `WorkflowManager` (or appropriate manager)
    - Query `WorkflowLog` for pending tasks where current user is an approver
    - Return list of pending approval details with entity information
  - [ ] 3.3 Create pending approvals endpoint in `WorkflowController`
    - Route: `GET /api/workflow/pending-approvals`
    - Call `GetPendingTasksForApproverAsync()` for current user
    - Map results to `List<PendingApprovalResponse>`
    - Return 200 OK with list
  - [ ] 3.4 Add `GetExecutivesForOrgUnitAsync()` method to `OpportunityManager`
    - Query `EntityUserRole` table for Director/Deputy Director roles
    - Filter by `EntityType == "OrganizationHierarchy"` and `EntityId == orgUnitId`
    - Filter by role codes: `OrgUnit_Director_OrganizationHierarchy`, `OrgUnit_Deputy_Director_OrganizationHierarchy`, `Regional_Director_OrganizationHierarchy`, `MCO_Director_OrganizationHierarchy`
    - Include `User` navigation property for user details
    - Return list of `TypeaheadInput` with `Label` (user display name + role) and `Value` (user ID as string)
    - Use `Description` field to mark the OrgUnit_Director as "Suggested" for UI default selection
  - [ ] 3.5 Create executive lookup endpoint in `OpportunityController`
    - Route: `GET /api/opportunity/{opportunityId}/executives`
    - Fetch opportunity to get `ResponsibleOrgUnitId`
    - Call `GetExecutivesForOrgUnitAsync()` with org unit ID
    - Return list of executives with suggested flag
  - [ ] 3.6 Add unit tests for pending approvals endpoint
    - Test: Returns pending approvals for current user
    - Test: Returns empty list when no pending approvals
    - Test: Response includes all required fields (EntityName, EntityId, SubmittedBy, etc.)
  - [ ] 3.7 Add unit tests for executive lookup endpoint
    - Test: Returns executives for valid opportunity org unit
    - Test: Returns empty list when no executives assigned
    - Test: Director/Manager marked as suggested
    - Test: Returns 404 for non-existent opportunity
  - [ ] 3.8 Verify all tests compile and run successfully with no errors

- [ ] **4.0 Backend: In-System Notifications Integration**
  > Enhance PaoWorkflowNotificationService (from prerequisite Task 5.0) to create in-system notifications via NotificationManager when workflow approval is requested, and mark them as done when decision is made.

  - [ ] 4.1 Add `NotificationManager` dependency to `PaoWorkflowNotificationService` constructor
    - Add `private readonly NotificationManager _notificationManager` field
    - Update constructor to accept and assign `NotificationManager`
  - [ ] 4.2 Update DI registration for `PaoWorkflowNotificationService` to include `NotificationManager`
    - Verify `NotificationManager` is already registered in DI container
    - Update service registration if needed
  - [ ] 4.3 Enhance `NotifyNewApprovalRequestAsync()` to create in-system notifications
    - After sending email (existing), create in-system notification
    - For each approver user ID in `notification.RecipientUserIds`:
      - Call `_notificationManager.CreateNotification()` with:
        - `userId`: approver user ID
        - `message`: `"Go Decision Required: {notification.EntityDisplayName}"`
        - `category`: `"workflow_approval"`
        - `responseType`: `"approval_request"`
        - `record`: the `WorkflowNotification` object (for context)
  - [ ] 4.4 Add `Entity` and `EntityId` to notification record for navigation
    - Ensure `Entity = notification.EntityName` and `EntityId = int.Parse(notification.EntityId)`
  - [ ] 4.5 Enhance `NotifyWorkflowCompletedAsync()` to mark notifications as done
    - Query `Notifications` for matching entity/category/unread
    - Filter: `Entity == notification.EntityName`, `EntityId == notification.EntityId`, `Category == "workflow_approval"`, `!IsRead`
    - Update each: `IsRead = true`, `Status = NotificationStatus.Done`
    - Save changes
  - [ ] 4.6 Enhance `NotifyWorkflowRejectedAsync()` to mark notifications as done
    - Same logic as `NotifyWorkflowCompletedAsync()` for marking notifications done
  - [ ] 4.7 Create `PaoWorkflowNotificationServiceTests.cs` in `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/`
    - Test: `NotifyNewApprovalRequestAsync` creates in-system notification for each approver
    - Test: Notification has correct category (`workflow_approval`) and responseType (`approval_request`)
    - Test: Notification message includes entity display name
    - Test: `NotifyWorkflowCompletedAsync` marks pending notifications as done
    - Test: `NotifyWorkflowRejectedAsync` marks pending notifications as done
  - [ ] 4.8 Verify all tests compile and run successfully with no errors

- [ ] **5.0 Backend: Email CC Recipients**
  > Enhance PaoWorkflowNotificationService email sending (from prerequisite Task 5.0) to include CC recipients: Opportunity Manager, workflow initiator, and Director/Manager of responsible org unit.

  - [ ] 5.1 Add helper method `GetOpportunityManagerEmailAsync(int entityId)` to `PaoWorkflowNotificationService`
    - Query Opportunity by ID with Stakeholders included
    - Find stakeholder with "Opportunity Manager" role
    - Return email address (or null if not found)
  - [ ] 5.2 Add helper method `GetUserEmailAsync(int userId)` to `PaoWorkflowNotificationService`
    - Query PAOUser by ID
    - Return email address (or null if not found)
  - [ ] 5.3 Add helper method `GetDirectorManagerEmailAsync(int orgUnitId)` to `PaoWorkflowNotificationService`
    - Query `EntityUserRole` for Director/Deputy Director of org unit
    - Filter by role codes: `OrgUnit_Director_OrganizationHierarchy`, `OrgUnit_Deputy_Director_OrganizationHierarchy`, `Regional_Director_OrganizationHierarchy`, `MCO_Director_OrganizationHierarchy`
    - Return email address of first match (or null if not found)
  - [ ] 5.4 Enhance `NotifyNewApprovalRequestAsync()` to build CC recipient list
    - Initialize `List<string> ccRecipients`
    - Add Opportunity Manager email (if not empty and entity is Opportunity)
    - Add workflow initiator email (if different from OM)
    - Add Director/Manager email of org unit
    - Remove duplicates from CC list
  - [ ] 5.5 Update email sending to include CC recipients
    - Modify `_emailSender.SendEmailAsync()` call to pass CC list
    - Verify `IEmailSender` interface supports CC parameter (update if needed)
  - [ ] 5.6 Add unit tests for CC recipient logic in `PaoWorkflowNotificationServiceTests.cs`
    - Test: CC includes Opportunity Manager email
    - Test: CC includes workflow initiator (if different from OM)
    - Test: CC includes Director/Manager of org unit
    - Test: CC list has no duplicates
    - Test: CC is empty list (not null) when no recipients found
  - [ ] 5.7 Verify all tests compile and run successfully with no errors

- [ ] **6.0 Backend: Post-Decision Immutability**
  > Add immutability enforcement in OpportunityManager and permission handler. Manager methods throw BusinessException for modifications after Go/No-Go/Cancelled decisions. Permission endpoint returns `canUpdate: false` and `isImmutable: true` for terminal stages, allowing frontend to use existing permission-driven patterns.
  >
  > **Note:** Immutability is based on **current stage** (not a permanent flag). This correctly handles the Reopen workflow from prerequisite PRD: when OM reopens a NO GO or CANCELLED opportunity, stage changes to IDENTIFY & PROFILE, and the record becomes editable again. Only GO is truly permanent.

  - [ ] 6.1 Add `IsOpportunityImmutable()` private helper method to `OpportunityManager`
    - Define immutable stages array: `new[] { "GO", "NO GO", "CANCELLED" }`
    - Return `true` if opportunity's current `Stage` is in the array
  - [ ] 6.2 Add immutability check to `UpdateOpportunityAsync()` method
    - Fetch opportunity, call `IsOpportunityImmutable()`
    - If immutable, throw `BusinessException("This opportunity record is locked and cannot be modified after a decision has been made.")`
    - Check BEFORE any update logic
  - [ ] 6.3 Add immutability check to `AddStakeholderAsync()` method (if exists)
    - Same pattern as UpdateOpportunityAsync
  - [ ] 6.4 Add immutability check to `AddDocumentAsync()` or related document methods
    - Same pattern - prevent adding documents to immutable opportunities
  - [ ] 6.5 Add immutability check to `AddRiskAsync()` method (if exists)
    - Same pattern
  - [ ] 6.6 Add immutability check to `AddCommentAsync()` method (if exists)
    - Same pattern
  - [ ] 6.7 Review and add immutability checks to any other modification methods
    - Scan OpportunityManager for all methods that modify opportunity data
    - Add immutability check to each
  - [ ] 6.8 Update `GetOpportunityPermissionsAsync()` (or permission handler) to return immutability flag
    - Check `IsOpportunityImmutable()` FIRST
    - If immutable: return `CanUpdate = false`, `CanDelete = false`, `CanAddDocuments = false`, `CanAddComments = false`, `IsImmutable = true`
    - If not immutable: continue with normal permission checks
  - [ ] 6.9 Add `IsImmutable` property to permission response model
    - Location: `UNOPS.PAO.Models` or appropriate permission model
    - Type: `bool`
  - [ ] 6.10 Add immutability checks to `UNOPSOpportunityManager` if it overrides modification methods
    - Ensure override methods also check immutability before modifying
  - [ ] 6.11 Create `OpportunityImmutabilityTests.cs` in `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/`
    - Test: `UpdateOpportunityAsync` throws BusinessException for GO stage
    - Test: `UpdateOpportunityAsync` throws BusinessException for NO GO stage
    - Test: `UpdateOpportunityAsync` throws BusinessException for CANCELLED stage
    - Test: `UpdateOpportunityAsync` succeeds for IDENTIFY & PROFILE stage
    - Test: Permission endpoint returns `IsImmutable = true` for GO stage
    - Test: Permission endpoint returns `CanUpdate = false` for immutable stages
    - Test: Reopened opportunity (back to IDENTIFY & PROFILE) becomes editable again
  - [ ] 6.12 Verify all tests compile and run successfully with no errors

- [ ] **7.0 Frontend: Decision Dialog Components**
  > Create ApproveOpportunityDialogComponent with confirmation statement, rationale field, and mandatory Executive dropdown. Create RejectOpportunityDialogComponent with confirmation statement and rationale field. Add entity-specific service methods to opportunity.service.ts (`approveOpportunity()`, `rejectOpportunity()`) for calling enhanced approve/reject endpoints with structured payloads (rationale, confirmation, executiveId). Add translation keys to all 4 i18n files (en.json, span.json, fr.json, pt.json) for dialog labels, buttons, messages, and validation errors.
  >
  > **Note:** The existing workflow component uses direct HttpClient calls for generic approve/reject. For opportunity-specific Go/No-Go decisions with additional fields (Executive assignment, confirmation statement), the dialogs should call opportunity.service.ts methods that post to the enhanced `/api/workflow/approve` and `/api/workflow/reject` endpoints with the full request models.

  - [ ] 7.1 Add `GoDecisionPayload` interface to `opportunity.model.ts`
    - Add `rationale` (string) property
    - Add `executiveId` (number) property
    - Add `confirmationAcknowledged` (boolean) property
  - [ ] 7.2 Add `NoGoDecisionPayload` interface to `opportunity.model.ts`
    - Add `rationale` (string) property
    - Add `confirmationAcknowledged` (boolean) property
  - [ ] 7.3 Add `getExecutivesForOrgUnit()` method to `opportunity.service.ts`
    - Method signature: `getExecutivesForOrgUnit(opportunityId: number): Observable<TypeaheadInput[]>`
    - Call `GET /api/opportunity/{opportunityId}/executives`
    - Return list with `label`, `value`, and `description` (where description="Suggested" indicates default selection)
  - [ ] 7.4 Add `approveOpportunity()` method to `opportunity.service.ts`
    - Method signature: `approveOpportunity(entityId: number, payload: GoDecisionPayload): Observable<any>`
    - Build request body with `entityName: 'Opportunity'`, `entityId`, and payload fields
    - Call `POST /api/workflow/approve`
  - [ ] 7.5 Add `rejectOpportunity()` method to `opportunity.service.ts`
    - Method signature: `rejectOpportunity(entityId: number, payload: NoGoDecisionPayload): Observable<any>`
    - Build request body with `entityName: 'Opportunity'`, `entityId`, and payload fields
    - Call `POST /api/workflow/reject`
  - [ ] 7.6 Create `approve-opportunity-dialog` component folder structure
    - Create folder: `features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/`
    - Create files: `.component.ts`, `.component.html`, `.component.scss`
  - [ ] 7.7 Implement `ApproveOpportunityDialogComponent` TypeScript
    - Use `model<boolean>()` for `visible` (two-way binding)
    - Use `input.required<Opportunity>()` for `opportunity`
    - Use `output<GoDecisionPayload>()` for `decisionConfirmed`
    - Add `confirmationAcknowledged = signal(false)`
    - Add `decisionRationale = signal('')`
    - Add `selectedExecutiveId = signal<string | null>(null)`
    - Add `executives = signal<TypeaheadInput[]>([])`
    - Add computed `confirmationStatement` using org unit code, name, and initiative type
    - Add computed `canSubmit` requiring all three fields (confirmation, rationale, executiveId)
    - Implement `loadExecutives()` calling `opportunityService.getExecutivesForOrgUnit()`
    - Pre-select suggested executive (where `description === "Suggested"`) as default
    - Implement `onSubmit()` emitting payload and closing dialog
  - [ ] 7.8 Implement `ApproveOpportunityDialogComponent` template (HTML)
    - Use `p-dialog` with `[(visible)]="visible"`
    - Display confirmation statement with checkbox
    - Add `pTextarea` for rationale with helper text
    - Add `p-select` for Executive dropdown (required, pre-populated with suggested)
    - Add Cancel and Confirm buttons, Confirm disabled until `canSubmit()` is true
  - [ ] 7.9 Create `reject-opportunity-dialog` component folder structure
    - Create folder: `features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/`
    - Create files: `.component.ts`, `.component.html`, `.component.scss`
  - [ ] 7.10 Implement `RejectOpportunityDialogComponent` TypeScript
    - Use `model<boolean>()` for `visible`
    - Use `output<NoGoDecisionPayload>()` for `decisionConfirmed`
    - Add `confirmationAcknowledged = signal(false)`
    - Add `decisionRationale = signal('')`
    - Static `confirmationStatement`: "The information presented is insufficient or I do not consider this to be an initiative UNOPS should pursue."
    - Add computed `canSubmit` requiring confirmation and rationale
    - Implement `onSubmit()` emitting payload and closing dialog
  - [ ] 7.11 Implement `RejectOpportunityDialogComponent` template (HTML)
    - Use `p-dialog` with `[(visible)]="visible"`
    - Display warning message about No-Go status
    - Display confirmation statement with checkbox
    - Add `pTextarea` for rationale with helper text
    - Add Cancel and Confirm buttons, Confirm disabled until `canSubmit()` is true
  - [ ] 7.12 Add translation keys to `en.json`
    - Add keys under `workflow.goDecision` namespace:
      - `dialog.approve.title`: "Confirm Go Decision"
      - `dialog.approve.confirmationLabel`: "I confirm that..."
      - `dialog.approve.rationaleLabel`: "Decision Rationale"
      - `dialog.approve.rationaleHint`: "Add the reason for your decision..."
      - `dialog.approve.executiveLabel`: "Assigned Executive"
      - `dialog.approve.executiveHint`: "Director/Manager/OiC of the responsible Org Unit suggested"
      - `dialog.approve.cancelButton`: "Cancel"
      - `dialog.approve.confirmButton`: "Confirm Go Decision"
      - `dialog.reject.title`: "Confirm No-Go Decision"
      - `dialog.reject.warning`: "This action will set the opportunity to NO GO status..."
      - `dialog.reject.confirmationLabel`: "The information presented is insufficient..."
      - `dialog.reject.confirmButton`: "Confirm No-Go Decision"
      - `validation.rationaleRequired`: "Decision rationale is required"
      - `validation.executiveRequired`: "Executive selection is required"
      - `validation.confirmationRequired`: "You must acknowledge the confirmation statement"
  - [ ] 7.13 Add translation keys to `span.json` (Spanish)
    - Same key structure as en.json with Spanish translations
  - [ ] 7.14 Add translation keys to `fr.json` (French)
    - Same key structure as en.json with French translations
  - [ ] 7.15 Add translation keys to `pt.json` (Portuguese)
    - Same key structure as en.json with Portuguese translations
  - [ ] 7.16 Create `approve-opportunity-dialog.component.spec.ts` unit tests
    - Test: Dialog opens when `visible` is true
    - Test: Confirmation statement displays org unit and initiative type
    - Test: Submit button disabled when confirmation not acknowledged
    - Test: Submit button disabled when rationale empty
    - Test: Submit button disabled when executive not selected
    - Test: Submit button enabled when all fields valid
    - Test: `decisionConfirmed` emits correct payload on submit
    - Test: Dialog closes after submit
  - [ ] 7.17 Create `reject-opportunity-dialog.component.spec.ts` unit tests
    - Test: Dialog opens when `visible` is true
    - Test: Warning message displayed
    - Test: Submit button disabled when confirmation not acknowledged
    - Test: Submit button disabled when rationale empty
    - Test: Submit button enabled when all fields valid
    - Test: `decisionConfirmed` emits correct payload on submit
  - [ ] 7.18 Create/modify `opportunity.service.spec.ts` unit tests
    - Test: `getExecutivesForOrgUnit` calls correct API endpoint
    - Test: `approveOpportunity` calls correct API endpoint with payload
    - Test: `rejectOpportunity` calls correct API endpoint with payload
  - [ ] 7.19 Run ESLint on all new/modified files and fix any errors
    - Run: `npx eslint --fix` on all dialog component files
    - Run: `npx eslint --fix` on opportunity.service.ts and opportunity.model.ts
  - [ ] 7.20 Verify all tests compile and run successfully with no errors

- [ ] **8.0 Frontend: Decision-Maker UI Integration**
  > Create OpportunityDecisionInfoPanelComponent for highlighted information (partner DD statuses, high risks, time to signing). Display instructional guidance message prominently in opportunity-view when stage is PENDING_GO_DECISION (per US-3). Integrate decision dialogs with opportunity-view using customStageChangeHandler. Update frontend permission model to include `isImmutable` flag. For immutability, use the backend-driven `isImmutable` flag from permission response (existing `canUpdate` will be `false` for terminal stages) - no frontend stage-checking logic needed.

  - [ ] 8.1 Create `opportunity-decision-info-panel` component folder structure
    - Create folder: `features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/`
    - Create files: `.component.ts`, `.component.html`, `.component.scss`
  - [ ] 8.2 Implement `OpportunityDecisionInfoPanelComponent` TypeScript
    - Use `input.required<Opportunity>()` for `opportunity`
    - Use `input<WorkflowDetails>()` for `workflowDetails` (to get sender remarks)
    - Add computed `proposedInitiativeType` from opportunity
    - Add computed `timeToSigning` calculating days from target signing date
    - Add computed `concerningDDStatuses` filtering partners with "Pending", "Expired", "Expiring Soon"
    - Add computed `highRisks` filtering risks with `preDefinedHighRiskId` or high impact level
    - Add computed `senderRemarks` from workflow submission comment
  - [ ] 8.3 Implement `OpportunityDecisionInfoPanelComponent` template (HTML)
    - Use `p-panel` with header "Key Information for Your Decision"
    - Display initiative type and time to signing in a table/grid
    - Display concerning DD statuses in warning-styled section (only if any)
    - Display high risks in warning-styled section (only if any)
    - Display sender remarks in info-styled section (only if present)
  - [ ] 8.4 Add `isImmutable` property to `EntityPermissions` interface in `opportunity.model.ts`
    - Add `isImmutable?: boolean` optional property
  - [ ] 8.5 Add `isImmutable` computed signal to `opportunity-view.component.ts`
    - Computed from `recordPermissions()?.isImmutable ?? false`
  - [ ] 8.6 Add instructional guidance display to `opportunity-view.component.ts`
    - Add computed `showDecisionGuidance` checking if user can approve workflow and stage is pending
    - Add `instructionalGuidanceText` constant with the PRD-defined text
  - [ ] 8.7 Add decision dialog signals to `opportunity-view.component.ts`
    - Add `showApproveDialog = signal(false)`
    - Add `showRejectDialog = signal(false)`
    - Add `approveDialogResultHandler` for promise resolution
    - Add `rejectDialogResultHandler` for promise resolution
  - [ ] 8.8 Implement `customStageChangeHandler` in `opportunity-view.component.ts`
    - Method signature: `async (nextStage: string, actionName: string): Promise<CustomStageChangeResult | undefined>`
    - If `actionName === 'Approve'`: set `showApproveDialog(true)`, return promise
    - If `actionName === 'Reject'`: set `showRejectDialog(true)`, return promise
    - Otherwise return `undefined` (use default behavior)
  - [ ] 8.9 Implement `onApproveConfirmed()` handler in `opportunity-view.component.ts`
    - Call `opportunityService.approveOpportunity()` with payload
    - On success: resolve `approveDialogResultHandler` with `{ proceed: true, comment: payload.rationale }`
    - Reload opportunity data
    - Show success toast
  - [ ] 8.10 Implement `onRejectConfirmed()` handler in `opportunity-view.component.ts`
    - Call `opportunityService.rejectOpportunity()` with payload
    - On success: resolve `rejectDialogResultHandler` with `{ proceed: true, comment: payload.rationale }`
    - Reload opportunity data
    - Show success toast
  - [ ] 8.11 Update `opportunity-view.component.html` to display instructional guidance
    - Add `p-message` component with guidance text (info severity)
    - Display only when `showDecisionGuidance()` is true
    - Position prominently at top of content area
  - [ ] 8.12 Update `opportunity-view.component.html` to include decision info panel
    - Add `<app-opportunity-decision-info-panel>` component
    - Pass `opportunity()` and workflow details
    - Display only when in pending approval workflow stage
  - [ ] 8.13 Update `opportunity-view.component.html` to include decision dialogs
    - Add `<app-approve-opportunity-dialog>` with bindings
    - Add `<app-reject-opportunity-dialog>` with bindings
    - Connect `decisionConfirmed` outputs to handlers
  - [ ] 8.14 Update `opportunity-view.component.html` to pass `customStageChangeHandler` to workflow
    - Add `[customStageChangeHandler]="customStageChangeHandler"` to `<app-stage-workflow>`
  - [ ] 8.15 Add translation keys for instructional guidance to all 4 i18n files
    - Key: `workflow.goDecision.guidance.message` with full guidance text
  - [ ] 8.16 Create `opportunity-decision-info-panel.component.spec.ts` unit tests
    - Test: Displays proposed initiative type
    - Test: Calculates and displays time to signing
    - Test: Displays concerning DD statuses with warning styling
    - Test: Displays high risks with warning styling
    - Test: Displays sender remarks when present
    - Test: Hides sections when no concerning data
  - [ ] 8.17 Update opportunity-view tests for new functionality
    - Test: Instructional guidance shown when user can approve
    - Test: Decision info panel shown when in pending approval stage
    - Test: `customStageChangeHandler` opens approve dialog for 'Approve' action
    - Test: `customStageChangeHandler` opens reject dialog for 'Reject' action
    - Test: `onApproveConfirmed` calls service and resolves handler
  - [ ] 8.18 Run ESLint on all modified files and fix any errors
  - [ ] 8.19 Verify all tests compile and run successfully with no errors

- [ ] **9.0 Frontend: Notifications Integration**
  > Add workflow approval tasks to the Actions Required card on home dashboard, and ensure notification bell displays and styles workflow_approval category notifications correctly.

  - [ ] 9.1 Add `PendingApprovalModel` interface to `workflow.models.ts`
    - Add `entityName` (string) property
    - Add `entityId` (number) property
    - Add `entityDisplayName` (string) property
    - Add `currentStage` (string) property
    - Add `pendingStage` (string) property
    - Add `submittedBy` (string) property
    - Add `submittedOn` (Date) property
    - Add `orgUnitName` (string) property
  - [ ] 9.2 Add `getPendingApprovalsForUser()` method to `workflow.service.ts`
    - Method signature: `getPendingApprovalsForUser(): Observable<PendingApprovalModel[]>`
    - Call `GET /api/workflow/pending-approvals`
  - [ ] 9.3 Update `home-dashboard.component.ts` to fetch pending workflow approvals
    - Add `pendingApprovals = signal<PendingApprovalModel[]>([])`
    - Add `loadPendingApprovals()` method calling `workflowService.getPendingApprovalsForUser()`
    - Call `loadPendingApprovals()` in `ngOnInit`
  - [ ] 9.4 Update `home-dashboard.component.ts` to compute combined Actions Required count
    - Combine existing actions with workflow approval count
    - Update total count display
  - [ ] 9.5 Update `home-dashboard.component.html` to display workflow approval tasks in Actions Required card
    - Add section for "Workflow Approval" category tasks
    - Display opportunity name, org unit, submitted by, submitted date
    - Add "Review for Go Decision" action button/link
    - Navigate to opportunity on click
  - [ ] 9.6 Update `topbar.component.ts` to handle workflow_approval notifications
    - No changes needed if existing notification polling already fetches all categories
    - Verify `workflow_approval` category notifications are included in response
  - [ ] 9.7 Update `topbar.component.html` to style workflow_approval notifications distinctively
    - Add specific icon or styling for workflow approval notifications
    - Display "Go Decision Required: [Name]" format
    - Add click handler to navigate to opportunity
  - [ ] 9.8 Add translation keys for Actions Required card labels
    - `home.actionsRequired.workflowApproval`: "Workflow Approval"
    - `home.actionsRequired.reviewGoDecision`: "Review for Go Decision"
  - [ ] 9.9 Add unit tests for workflow service pending approvals method
    - Test: `getPendingApprovalsForUser` calls correct API endpoint
    - Test: Returns empty array on 200 with empty list
    - Test: Maps response to `PendingApprovalModel[]`
  - [ ] 9.10 Update home-dashboard tests for workflow approval integration
    - Test: Pending approvals loaded on init
    - Test: Workflow tasks displayed in Actions Required card
    - Test: Click navigates to opportunity
    - Test: Count includes workflow approvals
  - [ ] 9.11 Run ESLint on all modified files and fix any errors
  - [ ] 9.12 Verify all tests compile and run successfully with no errors

- [ ] **10.0 Integration & End-to-End Validation**
  > Verify complete workflow flows including: notifications creation/display, decision dialogs, Executive assignment persistence, immutability enforcement, and email CC recipients.

  - [ ] 10.1 Manual E2E Test: Go Decision Flow
    - Submit opportunity for Go decision (prerequisite feature)
    - Verify email sent to DoA2 with CC recipients (OM, initiator, Director)
    - Verify in-system notification created for DoA2 approver
    - Verify task appears in Actions Required card
    - Verify notification appears in notification bell
    - Navigate to opportunity from notification
    - Verify instructional guidance displayed
    - Verify decision info panel displays correct data
    - Open Approve dialog
    - Verify confirmation statement shows org unit and initiative type
    - Verify Executive dropdown pre-populated with Director/Manager suggestion
    - Complete Go decision with rationale and Executive selection
    - Verify stage changes to GO
    - Verify Executive stored on opportunity record
    - Verify notification marked as done
    - Verify record is immutable (edit attempts fail)
  - [ ] 10.2 Manual E2E Test: No-Go Decision Flow
    - Submit opportunity for Go decision
    - Navigate to opportunity as DoA2
    - Open Reject dialog
    - Verify warning message displayed
    - Verify confirmation statement is correct
    - Complete No-Go decision with rationale
    - Verify stage changes to NO GO
    - Verify notification marked as done
    - Verify record is immutable
  - [ ] 10.3 Manual E2E Test: Reopen Flow (from prerequisite PRD)
    - After No-Go decision, verify OM can Reopen
    - Verify stage changes to IDENTIFY & PROFILE
    - Verify record becomes editable again (immutability lifted)
    - Verify permissions return `canUpdate: true`, `isImmutable: false`
  - [ ] 10.4 Manual E2E Test: Immutability Enforcement
    - After Go decision, attempt to edit opportunity via UI
    - Verify edit controls are disabled/hidden
    - Attempt to edit via API (Postman/curl)
    - Verify 400 Bad Request with immutability error message
    - Attempt to add document via API
    - Verify rejected
  - [ ] 10.5 Manual E2E Test: Email CC Recipients
    - Submit opportunity for Go decision
    - Check email received by DoA2 holders
    - Verify CC includes Opportunity Manager
    - Verify CC includes workflow initiator (if different)
    - Verify CC includes Director/Manager of org unit
    - Verify email content is identical for all recipients
  - [ ] 10.6 Regression Test: Existing Workflow Features
    - Verify Submit workflow still works correctly
    - Verify Recall workflow still works correctly
    - Verify Cancel workflow still works correctly
    - Verify existing notifications are not affected
  - [ ] 10.7 Document any bugs found and create fix tasks
  - [ ] 10.8 Verify all automated tests pass before declaring integration complete
