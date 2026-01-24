# Task List: Send Opportunity for Go Decision

**Generated from:** `send-opportunity-for-go-decision-prd.md`  
**Generated on:** 2026-01-23

---

## Relevant Files

### Backend Files (.NET Core)

**Workflow Infrastructure:**
- `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` - EXISTS - MODIFY (add CANCELLED stage constant and State definition)
- `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeSeeder.cs` - EXISTS - MODIFY (add Cancel and Reopen transitions)
- `UNOPS.PAO.Business/Workflow/Seeders/StateMachineStageChangeRoleSeeder.cs` - EXISTS - MODIFY (DoA2 role, OM-only Cancel/Reopen permissions)

**Requirements Validation:**
- `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirementsProvider.cs` - NEW (implements IStageRequirementsProvider)
- `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirements.cs` - EXISTS - DELETE (unused placeholder)

**Adapters:**
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowApproverProvider.cs` - EXISTS - MODIFY (DoA2 lookup from ResponsibleOrgUnit)
- `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` - EXISTS - MODIFY (actual email sending)

**Controllers & Models:**
- `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` - EXISTS - MODIFY (requirements endpoint, custom rejection → NO GO, Cancel/Reopen actions)
- `UNOPS.PAO.Models/Workflow/WorkflowModels.cs` - EXISTS - MODIFY (confirmation flags, warning types)

**Email Templates:**
- `UNOPS.PAO.Business/EmailTemplates/WorkflowApprovalRequest.html` - EXISTS - MODIFY (add statement link, update placeholders)
- `UNOPS.PAO.Business/EmailTemplates/WorkflowCompleted.html` - EXISTS - MODIFY (Go decision notification)
- `UNOPS.PAO.Business/EmailTemplates/WorkflowRejected.html` - EXISTS - MODIFY (No Go notification)
- `UNOPS.PAO.Business/EmailTemplates/WorkflowRecalled.html` - EXISTS - MODIFY (recall notification)

**Backend - Unit Tests:**
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/PaoWorkflowApproverProviderTests.cs` - EXISTS - MODIFY (add DoA2 lookup tests)
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/StateMachineStageChangeSeederTests.cs` - EXISTS - MODIFY (add CANCELLED transition tests)
- `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityStageRequirementsProviderTests.cs` - NEW
- `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs` - EXISTS - MODIFY (add requirements endpoint tests, custom actions)

### Frontend Files (Angular)

**Workflow Components:**
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/services/workflow.service.ts` - EXISTS - MODIFY (add getRequirementsForStageChange)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component.ts` - EXISTS - MODIFY (stepper display logic, Cancel/Reopen actions)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component.html` - EXISTS - MODIFY (action buttons, stepper)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/models/workflow.models.ts` - EXISTS - MODIFY (warning types)

**Requirements Validation (copied from submodule):**
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/models/requirement.models.ts` - NEW (copy from submodule)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/requirements-validation/requirements-validation.component.ts` - NEW (copy from submodule)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/requirements-validation/requirements-validation.component.html` - NEW (copy from submodule)
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/requirements-validation/requirements-validation.component.scss` - NEW (copy from submodule)

**Opportunity View:**
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.ts` - EXISTS - MODIFY (integrate requirements-validation)
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.html` - EXISTS - MODIFY (add requirements-validation component)

**Translation Files:**
- `UNOPS.PAO.ClientApp/src/assets/i18n/en.json` - EXISTS - MODIFY (add requirement translation keys)

**Frontend - Unit Tests:**
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/services/workflow.service.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/requirements-validation/requirements-validation.component.spec.ts` - NEW
- `UNOPS.PAO.ClientApp/src/app/features/partnerships/opportunities/components/opportunity/view/opportunity-view.component.spec.ts` - EXISTS - MODIFY

---

## ⚠️ CRITICAL Testing Requirements

### Testing Philosophy
All implementation tasks MUST include corresponding unit tests. Tests are integrated as sub-tasks within each parent task, not as a separate testing phase.

### Required Tools
- **Backend:** xUnit, Moq, InMemory Database (EF Core), FluentAssertions
- **Frontend:** Jasmine, Karma, HttpTestingController, TestBed

### Test Coverage Expectations
- All new public methods must have unit tests
- All modified methods must have updated tests
- Edge cases and error conditions must be tested
- Validation logic must have comprehensive test coverage

### Mandatory Verification
Every unit test task includes: "Verify all tests compile and run successfully with no errors"

---

## Tasks

- [ ] **1.0 Backend: Workflow Stage Infrastructure (CANCELLED Stage)**
  > Add the CANCELLED stage to the opportunity workflow by updating OpportunityWorkflow.cs with the new stage constant and State definition, then adding the Cancel and Reopen transitions in StateMachineStageChangeSeeder.cs.

  - [ ] 1.1 Add CANCELLED stage constant to `OpportunityWorkflow.Stages` class
    - Add `public const string Cancelled = "CANCELLED";` following existing naming pattern
    - Update `AllStages` array to include `Stages.Cancelled`
  - [ ] 1.2 Add CANCELLED State to `StateMachine.States` array in `OpportunityWorkflow.cs`
    - Sequence = 4, StageCode = Stages.Cancelled, DisplayName = Stages.Cancelled, Facing = Internal
  - [ ] 1.3 Add Cancel transition to `StateMachineStageChangeSeeder.GetSeedStageChanges()`
    - FromStage = "IDENTIFY & PROFILE", ToStage = "CANCELLED"
    - Name = "Cancel", ApprovalRequired = false, CommentRequired = true
  - [ ] 1.4 Add Reopen from CANCELLED transition to `StateMachineStageChangeSeeder.GetSeedStageChanges()`
    - FromStage = "CANCELLED", ToStage = "IDENTIFY & PROFILE"
    - Name = "Reopen", ApprovalRequired = false, CommentRequired = true
  - [ ] 1.5 Add OM-only role permissions for Cancel/Reopen in `StateMachineStageChangeRoleSeeder`
    - Cancel: Only "Opportunity Manager" role can trigger
    - Reopen from CANCELLED: Only "Opportunity Manager" role can trigger
  - [ ] 1.6 Update existing Reopen from NO GO permission to OM-only if not already
  - [ ] 1.7 Update unit tests in `StateMachineStageChangeSeederTests.cs`
    - Test Cancel transition is seeded correctly
    - Test Reopen from CANCELLED transition is seeded correctly
    - Verify all tests compile and run successfully with no errors
  - [ ] 1.8 Review implementation: verify all stage constants, State definitions, transitions, and role permissions are consistent

- [ ] **2.0 Backend: Requirements Validation Provider**
  > Create OpportunityStageRequirementsProvider implementing IStageRequirementsProvider (from UNOPS.Workflow submodule) with all mandatory field requirements for the IDENTIFY & PROFILE → GO transition.

  - [ ] 2.1 Create `OpportunityStageRequirementsProvider.cs` in `UNOPS.PAO.Business/Workflow/StageRequirements/`
    - Implement `IStageRequirementsProvider` interface from `UNOPS.Workflow.Business.Interfaces`
    - Add `EntityNames` property returning `["Opportunity"]`
  - [ ] 2.2 Implement `GetRequirementsForStageChange(string currentStage, string nextStage)` method (synchronous, per interface)
    - Return empty list if not IDENTIFY & PROFILE → GO transition
    - Return `List<StageRequirement>` (using `UNOPS.Workflow.Models.Requirements` namespace)
  - [ ] 2.3 Add required text field validations (see PRD FR-2.1):
    - name, description, challenges, expectedImpact, expectedOutcomes, opportunityStatementMarkdown
  - [ ] 2.4 Add required number field validation:
    - initiativeBudgetUSD (Proposed Budget)
  - [ ] 2.5 Add required array field validations (minLength = 1):
    - unopsMissions (Strategic Missions), sdgs (SDG Alignment), fundingPartners, clientPartners, deliverables (Products & Services), countries
  - [ ] 2.6 Add Beneficiaries conditional validation:
    - FieldType = "conditional", CustomValidator = "BeneficiariesValidator"
    - Validation rule: BeneficiariesToBeDetermined == true OR (EstimatedDirectBeneficiaries > 0 AND EstimatedIndirectBeneficiaries >= 0)
    - Description = "Beneficiaries information is required - either check 'to be determined' or provide both direct and indirect counts"
  - [ ] 2.7 Add required date field validations:
    - targetSigningDate, implementationStartDate, targetDeliveryDate
  - [ ] 2.8 Add required select field validations:
    - responsibleOrgUnitId, proposedInitiativeTypeId
  - [ ] 2.9 Add stakeholder role validation:
    - Require at least one stakeholder with "Opportunity Manager" role
  - [ ] 2.10 Add DoA2 holder server-side validation:
    - Mark with `onlyServerSideEvaluation = true`
    - Check if ResponsibleOrgUnit has DoA2 holders assigned
  - [ ] 2.11 Register `OpportunityStageRequirementsProvider` in DI container
    - Add to `WorkflowServiceExtensions.AddPaoWorkflowServices()`: `services.AddScoped<IStageRequirementsProvider, OpportunityStageRequirementsProvider>();`
  - [ ] 2.12 Delete unused `OpportunityStageRequirements.cs` (the static placeholder class)
  - [ ] 2.13 Create unit tests in `OpportunityStageRequirementsProviderTests.cs`
    - Test all mandatory field requirements are returned for GO transition
    - Test empty list returned for non-GO transitions
    - Test DoA2 validation fails when no holders found
    - Test Beneficiaries conditional validation (both scenarios: to-be-determined and direct/indirect counts)
    - Verify all tests compile and run successfully with no errors
  - [ ] 2.14 Review implementation: verify all 19 mandatory fields are covered per PRD FR-2.1

- [ ] **3.0 Backend: DoA Level 2 Approver Lookup**
  > Update PaoWorkflowApproverProvider to look up DoA Level 2 holders from the opportunity's ResponsibleOrgUnit via EntityUserRole instead of "Partnership Lead" stakeholders.

  - [ ] 3.1 Modify `GetOpportunityApproversAsync()` in `PaoWorkflowApproverProvider.cs`
    - Current: queries `OpportunityStakeholder` where `EntityRole.Name` matches role names from seeder
    - New: For IDENTIFY & PROFILE → GO transition, bypass stakeholder lookup and directly query `EntityUserRole`
    - Query pattern: `EntityUserRole` where `EntityType = "OrganizationHierarchy"`, `EntityId = opportunity.ResponsibleOrgUnitId`, `EntityRole.Code = "DoA2_OrganizationHierarchy"`
    - Join with `PAOUsers` and `UserProfile` to get names
  - [ ] 3.2 Add helper method `GetDoA2HoldersForOrgUnitAsync(int orgUnitId)`
    - Query `EntityUserRole` for DoA2 role holders on the org unit
    - Include: `Include(e => e.EntityRole)`, `Include(e => e.User).ThenInclude(u => u.UserProfile)`
    - Filter: `EntityType == "OrganizationHierarchy"`, `EntityId == orgUnitId`, `EntityRole.Code == "DoA2_OrganizationHierarchy"`
    - Return list of `WorkflowApproverModel` with UserId, Name, Email, Role="DoA Level 2"
  - [ ] 3.3 Update `GetOpportunityApproversAsync()` to use helper method
    - When `toStage == OpportunityWorkflow.Stages.Go`, call `GetDoA2HoldersForOrgUnitAsync(opportunity.ResponsibleOrgUnitId)`
    - For other transitions, keep existing stakeholder lookup logic
    - Note: This approach bypasses the seeder's role lookup for GO transition - the seeder can keep "Partnership Lead" or be updated later
  - [ ] 3.4 Fetch opportunity's ResponsibleOrgUnitId in approver lookup
    - Query `Opportunities` table to get `ResponsibleOrgUnitId` for the given opportunityId
    - Handle null ResponsibleOrgUnitId gracefully (return empty approvers)
  - [ ] 3.5 Handle case when no DoA2 holders found
    - Return empty approvers list (will trigger validation error on submit)
    - Log warning: "No DoA2 holders found for org unit {orgUnitId}"
  - [ ] 3.6 Update unit tests in `PaoWorkflowApproverProviderTests.cs`
    - Test DoA2 holders are returned correctly from org unit's EntityUserRole records
    - Test empty list when no DoA2 holders assigned to org unit
    - Test empty list when opportunity has no ResponsibleOrgUnitId
    - Test user names are included
    - Verify all tests compile and run successfully with no errors
  - [ ] 3.7 (Optional) Update seeder role from "Partnership Lead" to "DoA Holder" for documentation
    - This is cosmetic - the approver provider now bypasses seeder lookup for GO transition
    - May help with future understanding of the codebase
  - [ ] 3.8 Review implementation: verify DoA2 lookup works independently of seeder role definitions

- [ ] **4.0 Backend: WorkflowController Endpoints & Custom Actions**
  > Add GET requirements endpoint, implement custom rejection behavior (→ NO GO instead of previous stage), add Cancel and Reopen action handlers, and implement warning responses for non-OM submitter and country-org unit mismatch.

  - [ ] 4.1 Add GET requirements endpoint to `WorkflowController.cs`
    - Route: `GET /api/workflow/{entityName}/{id}/requirements?currentStage={currentStage}` (matches submodule's service pattern)
    - Inject `IStageRequirementsProvider` (from `UNOPS.Workflow.Business.Interfaces`)
    - Call `provider.GetRequirementsForStageChange(currentStage, nextStage)` where nextStage is determined from available actions
    - Return `List<StageRequirement>` (from `UNOPS.Workflow.Models.Requirements`)
  - [ ] 4.2 Add helper method `IsUserOpportunityManagerAsync(int entityId, int userId)`
    - Check if user has "Opportunity Manager" role on the opportunity via stakeholders
  - [ ] 4.3 Add helper method `GetUnrelatedCountriesAsync(int opportunityId)`
    - Get countries not in org unit's relationship list (see PRD FR-7)
    - Return list of country names for warning display
  - [ ] 4.4 Implement non-OM submitter warning in Submit action
    - Check if current user is OM, if not and `ConfirmedNonOMSubmission` is false, return warning response
    - Add `ConfirmedNonOMSubmission` flag to WorkflowSubmitRequest
  - [ ] 4.5 Implement country-org unit mismatch warning in Submit action
    - Call `GetUnrelatedCountriesAsync()`, if any and `ConfirmedOrgUnitWarning` is false, return warning response
    - Add `ConfirmedOrgUnitWarning` flag to WorkflowSubmitRequest
  - [ ] 4.6 Implement mandatory acknowledgment statement in Submit action
    - Require `AcknowledgedStatement` flag to be true before proceeding
    - Add optional `AdditionalRemarks` field
  - [ ] 4.7 Trigger Opportunity Statement regeneration before submission
    - Call `_opportunityManager.GenerateOpportunityStatementAsync(entityId)` in Submit
  - [ ] 4.8 Implement custom rejection handling for opportunities
    - When Action = "reject" and entityName = "opportunity", set stage to "NO GO" (not previous stage)
    - Set WorkflowStatus to None, clear pending task
  - [ ] 4.9 Implement Cancel action handler
    - Validate: only OM can cancel, only from IDENTIFY & PROFILE stage, comment required
    - Set `Stage = "CANCELLED"`, `Status = EntityStatus.Closed`, `WorkflowStatus = WorkflowStatus.None`
    - Log action in workflow history
  - [ ] 4.10 Implement Reopen action handler
    - Validate: only OM can reopen, only from NO GO or CANCELLED stage
    - For CANCELLED, require comment; for NO GO, comment optional
    - Set `Stage = "IDENTIFY & PROFILE"`, `Status = EntityStatus.Active`, `WorkflowStatus = WorkflowStatus.None`
    - Log action in workflow history
  - [ ] 4.11 Update `WorkflowModels.cs` with new request/response properties
    - WorkflowSubmitRequest: add ConfirmedNonOMSubmission, ConfirmedOrgUnitWarning, AcknowledgedStatement, AdditionalRemarks
    - WorkflowSubmitResponse: add RequiresConfirmation, ConfirmationType, ConfirmationMessage, UnrelatedCountries
    - WorkflowActionResponse: add NewStage, Message
  - [ ] 4.12 Enable OM recall (not just submitter)
    - Modify Recall action to allow if user is submitter OR OM
    - Require mandatory justification comment
  - [ ] 4.13 Update controller tests in `WorkflowControllerTests.cs`
    - Test requirements endpoint returns correct requirements
    - Test non-OM submitter warning flow
    - Test country-org unit mismatch warning flow
    - Test custom rejection → NO GO for opportunities
    - Test Cancel action permissions and stage change
    - Test Reopen action from NO GO and CANCELLED
    - Test OM recall permission
    - Verify all tests compile and run successfully with no errors
  - [ ] 4.14 Review implementation: verify all endpoints follow REST conventions and return consistent responses

- [ ] **5.0 Backend: Email Notification Templates**
  > Update email templates with exact PRD wording, add Opportunity Statement links, and configure PaoWorkflowNotificationService to send actual emails.

  - [ ] 5.1 Update `WorkflowApprovalRequest.html` template
    - Subject: "PAO: [Opportunity Name] - Action Required"
    - Include greeting with DoA holder names
    - Include DoA level and org unit info
    - Include opportunity name with link
    - Include submitter name
    - Add link to statement section: `{baseUrl}/opportunity/{id}#statement`
    - Include note about Internal Stakeholder notifications
  - [ ] 5.2 Update `WorkflowCompleted.html` template
    - Subject: "PAO: [Opportunity Name] - Go Decision Approved"
    - Notify submitter that opportunity has been approved
    - Include approver name
    - Include link to opportunity
  - [ ] 5.3 Update `WorkflowRejected.html` template
    - Subject: "PAO: [Opportunity Name] - Set to NO GO"
    - Notify submitter that opportunity has been set to NO GO
    - Include rejection reason from DoA holder
    - Explain that org unit will not proceed with development
    - Note that OM can reopen if circumstances change
    - Include link to opportunity
  - [ ] 5.4 Update `WorkflowRecalled.html` template
    - Subject: "PAO: [Opportunity Name] - Submission Recalled"
    - Include recall justification
    - Include link to opportunity
  - [ ] 5.5 Update `PaoWorkflowNotificationService.cs` to send actual emails
    - Implement `NotifyNewApprovalRequestAsync()` to send WorkflowApprovalRequest email
    - Implement `NotifyApprovalCompletedAsync()` to send WorkflowCompleted email
    - Implement `NotifyRejectionToNoGoAsync()` to send WorkflowRejected email
    - Implement `NotifyRecallAsync()` to send WorkflowRecalled email
  - [ ] 5.6 Add method to get recipient emails from user IDs
  - [ ] 5.7 Add method to notify Internal Stakeholders on Go decision
    - Get org units normally responsible for implementation countries
    - Exclude the opportunity's own org unit
    - Send notification to stakeholders in those org units
  - [ ] 5.8 Test notification service methods
    - Notification service methods are tested via controller integration tests (Task 4.13)
    - Verify email templates render correctly with sample data
  - [ ] 5.9 Review implementation: verify all email templates match PRD wording exactly

- [ ] **6.0 Frontend: Requirements Validation Integration**
  > Copy and adapt the requirements-validation component from the submodule to PAO (GMS pattern - blue collapsible info panel showing only unmet requirements), then integrate into the opportunity-view component.

  - [ ] 6.1 Copy `StageRequirement` interface and related models to PAO
    - Source: `UNOPS.Workflow/unops-workflow-angular/src/lib/models/requirement.models.ts`
    - Target: `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/models/requirement.models.ts`
    - Include: `StageRequirement`, `RequirementValidation`, `ConditionalValidation`, `FieldTypes`, `isBuiltInFieldType()`
  - [ ] 6.2 Copy requirements-validation component to PAO
    - Source: `UNOPS.Workflow/unops-workflow-angular/src/lib/components/requirements-validation/`
    - Target: `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/components/requirements-validation/`
    - Copy: `.ts`, `.html`, `.scss` files
    - Update imports to use PAO's local paths
  - [ ] 6.3 Adapt component selector to PAO convention
    - Change selector from `lib-requirements-validation` to `app-requirements-validation`
    - Update import paths to use `@shared/` aliases
  - [ ] 6.4 Add `getRequirementsForStageChange()` method to PAO's `workflow.service.ts`
    - Call GET `/api/workflow/{entityName}/{id}/requirements?currentStage={currentStage}`
    - Return `Observable<StageRequirement[]>`
    - Import `StageRequirement` from local `requirement.models.ts`
  - [ ] 6.5 Export new models and component from workflow module
    - Add exports to PAO's workflow barrel file (if exists) or update component imports
  - [ ] 6.6 Add requirements loading logic to `opportunity-view.component.ts`
    - Fetch requirements on component init when stage is "IDENTIFY & PROFILE"
    - Store requirements in component property
    - Track which requirements are met/unmet
  - [ ] 6.7 Add requirements-validation component to `opportunity-view.component.html`
    - Position above the stage-workflow component
    - Pass requirements array to component
    - Only show when there are unmet requirements (GMS pattern)
  - [ ] 6.8 Style requirements panel following GMS pattern
    - Blue info-style colors (not error red)
    - Collapsible with chevron icon
    - Message: "The Opportunity cannot proceed to the Go stage until the following conditions are met."
    - Show only unmet requirements as bullet list (no checkmarks)
  - [ ] 6.9 Add translation keys to `en.json` for all requirement messages
    - Add all keys from PRD Appendix B (message.requirements.opportunity.*)
  - [ ] 6.10 Create unit tests for requirements-validation component
    - Create `requirements-validation.component.spec.ts`
    - Test component renders unmet requirements
    - Test collapsible behavior
    - Test validation logic for built-in field types
    - Verify all tests compile and run successfully with no errors
  - [ ] 6.11 Create unit tests in `workflow.service.spec.ts`
    - Test getRequirementsForStageChange calls correct endpoint
    - Test response mapping to StageRequirement[]
    - Verify all tests compile and run successfully with no errors
  - [ ] 6.12 Update tests in `opportunity-view.component.spec.ts`
    - Test requirements are fetched on init
    - Test requirements panel shows only when unmet requirements exist
    - Verify all tests compile and run successfully with no errors
  - [ ] 6.13 Review implementation: verify requirements panel matches GMS visual pattern

- [ ] **7.0 Frontend: Workflow UI Updates**
  > Update stage-workflow component with happy-path stepper display logic, add Cancel/Reopen action buttons for OM, and implement warning confirmation dialogs.

  - [ ] 7.1 Add `getDisplayStages()` method to `stage-workflow.component.ts`
    - Filter stages based on current stage for happy-path display
    - Default (IDENTIFY & PROFILE or GO): show only IDENTIFY & PROFILE → GO
    - NO GO: show IDENTIFY & PROFILE → NO GO
    - CANCELLED: show IDENTIFY & PROFILE → CANCELLED
  - [ ] 7.2 Update stepper template in `stage-workflow.component.html`
    - Use filtered stages from getDisplayStages() instead of all stages
  - [ ] 7.3 Add `cancelOpportunity()` method to `workflow.service.ts`
    - POST to `/api/workflow` with action = "cancel"
  - [ ] 7.4 Add `reopenOpportunity()` method to `workflow.service.ts`
    - POST to `/api/workflow` with action = "reopen"
  - [ ] 7.5 Add Cancel button to stage-workflow component
    - Only show for OM when stage is "IDENTIFY & PROFILE"
    - Show confirmation dialog with mandatory justification field
  - [ ] 7.6 Add Reopen button to stage-workflow component
    - Only show for OM when stage is "NO GO" or "CANCELLED"
    - For CANCELLED: require mandatory reason field
    - For NO GO: reason optional
  - [ ] 7.7 Implement non-OM submitter warning dialog
    - Show when API returns RequiresConfirmation with ConfirmationType = "NonOMSubmitter"
    - Display user's current role in message
    - On confirm, resubmit with ConfirmedNonOMSubmission = true
  - [ ] 7.8 Implement country-org unit mismatch warning dialog
    - Show when API returns ConfirmationType = "OrgUnitCountryMismatch"
    - Display list of unrelated countries
    - On confirm, resubmit with ConfirmedOrgUnitWarning = true
  - [ ] 7.9 Implement acknowledgment statement dialog for submission
    - Show acknowledgment text from PRD FR-13
    - Require checkbox or button acknowledgment
    - Include optional "Additional remarks" field
    - Submit with AcknowledgedStatement = true
  - [ ] 7.10 Add rejection confirmation dialog for approvers
    - Message: "Rejecting this opportunity will set its stage to NO GO. The Opportunity Manager can reopen it later if circumstances change. Are you sure you want to proceed?"
  - [ ] 7.11 Update workflow.models.ts with new types
    - WorkflowSubmitRequest: add ConfirmedNonOMSubmission, ConfirmedOrgUnitWarning, AcknowledgedStatement, AdditionalRemarks
    - WorkflowSubmitResponse: add RequiresConfirmation, ConfirmationType, ConfirmationMessage, UnrelatedCountries
  - [ ] 7.12 Create unit tests in `stage-workflow.component.spec.ts`
    - Test getDisplayStages() returns correct stages for each current stage
    - Test Cancel button visibility based on role and stage
    - Test Reopen button visibility based on role and stage
    - Test confirmation dialogs show correctly
    - Verify all tests compile and run successfully with no errors
  - [ ] 7.13 Update `workflow.service.spec.ts` tests
    - Test cancelOpportunity calls correct endpoint
    - Test reopenOpportunity calls correct endpoint
    - Verify all tests compile and run successfully with no errors
  - [ ] 7.14 Review implementation: verify all UI flows match PRD mockups and acceptance criteria

- [ ] **8.0 Integration & End-to-End Testing**
  > Verify complete workflow flows: submit → approve/reject, cancel, reopen, with all notifications and validations working correctly.

  - [ ] 8.1 Test Submit Flow: Happy Path
    - All requirements met, OM submits, confirmation dialogs work
    - Workflow created, approvers notified via email
    - Opportunity Statement regenerated
  - [ ] 8.2 Test Submit Flow: Non-OM Submitter Warning
    - Non-OM user attempts submit, warning shown
    - Confirm and resubmit works
  - [ ] 8.3 Test Submit Flow: Country-Org Unit Mismatch Warning
    - Opportunity has countries outside org unit relationships
    - Warning shows correct country names
    - Confirm and resubmit works
  - [ ] 8.4 Test Submit Flow: Requirements Not Met
    - Missing mandatory fields, submit blocked
    - Requirements panel shows unmet items
    - After filling fields, panel updates and submit allowed
  - [ ] 8.5 Test Approve Flow
    - DoA2 holder approves, stage changes to GO
    - Submitter notified via email
    - Internal Stakeholders notified (if applicable)
  - [ ] 8.6 Test Reject Flow: Custom NO GO Behavior
    - DoA2 holder rejects, stage changes to NO GO (not IDENTIFY & PROFILE)
    - Submitter notified with rejection reason
    - Workflow history shows "Rejected → NO GO"
  - [ ] 8.7 Test Recall Flow
    - OM recalls (not original submitter)
    - Mandatory justification required
    - Approvers notified of recall
    - Opportunity unlocked for editing
  - [ ] 8.8 Test Cancel Flow
    - OM cancels from IDENTIFY & PROFILE
    - Mandatory justification required
    - Stage changes to CANCELLED, Status changes to EntityStatus.Closed
    - Workflow history shows cancellation
  - [ ] 8.9 Test Reopen Flow: From NO GO
    - OM reopens, no mandatory reason
    - Stage returns to IDENTIFY & PROFILE
    - Opportunity editable again
  - [ ] 8.10 Test Reopen Flow: From CANCELLED
    - OM reopens, mandatory reason required
    - Stage returns to IDENTIFY & PROFILE, Status returns to EntityStatus.Active
    - Opportunity editable again
  - [ ] 8.11 Test Stepper Display Logic
    - In IDENTIFY & PROFILE: shows IDENTIFY & PROFILE → GO
    - In GO: shows IDENTIFY & PROFILE → GO (completed)
    - In NO GO: shows IDENTIFY & PROFILE → NO GO
    - In CANCELLED: shows IDENTIFY & PROFILE → CANCELLED
  - [ ] 8.12 Verify all email notifications sent correctly
    - Approval request to DoA2 holders
    - Go decision to submitter
    - NO GO notification with reason
    - Recall notification with justification
  - [ ] 8.13 Document any issues found and create follow-up tasks if needed
  - [ ] 8.14 Review complete implementation against all PRD user stories and acceptance criteria

---

## Notes

- Unit tests follow PAO convention: `UNOPS.PAO.IntegrationTests/UnitTests/` for domain tests, `QA Tests/Integration Tests/Controllers/` for controller tests
- Frontend tests use `.spec.ts` suffix in same directory as component
- The workflow submodule (`UNOPS.Workflow/`) should NOT be modified - it provides interfaces like `IStageRequirementsProvider`
- Stage constants use UPPERCASE format as defined in `OpportunityWorkflow.cs` (e.g., "IDENTIFY & PROFILE", "GO", "CANCELLED")
- All seeders are idempotent - safe to run multiple times
- Custom rejection behavior (→ NO GO) is specific to opportunities; other entities use standard workflow rejection

### Codebase Notes
- `EntityStatus` enum (Active, Closed, etc.) is inherited from `ModifiableDeletableEntity` base class
- `WorkflowStatus` enum has only two values: `None` and `InWorkflow`
- `IStageRequirementsProvider.GetRequirementsForStageChange()` is a synchronous method (not async)
- Stages are defined in `OpportunityWorkflow.cs`; transitions are seeded in `StateMachineStageChangeSeeder.cs`

### Submodule Integration Notes
- **Backend (C#):** Reference submodule directly via namespaces
  - `UNOPS.Workflow.Business.Interfaces` for `IStageRequirementsProvider`
  - `UNOPS.Workflow.Models.Requirements` for `StageRequirement`
- **Frontend (Angular):** Copy and adapt components from submodule to PAO (no direct import)
  - Source: `UNOPS.Workflow/unops-workflow-angular/src/lib/`
  - Target: `UNOPS.PAO.ClientApp/src/app/shared/reusables/components/workflow/`
  - Update selector prefix from `lib-` to `app-` when copying
  - PAO's `workflow.service.ts` is separate from the submodule's `WorkflowService`

### DoA2 Approver Architecture Notes
- DoA2 roles are `EntityRole` records with `EntityType = "OrganizationHierarchy"` and `Code = "DoA2_OrganizationHierarchy"`
- DoA2 holders are linked via `EntityUserRole` to their organization hierarchy unit
- This differs from opportunity stakeholders which are `OpportunityStakeholder` records
- The approver provider must query `EntityUserRole` directly for DoA2 holders, not `OpportunityStakeholder`
