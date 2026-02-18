# Workflow PRD Traceability Test Plan

**Document Version:** 1.0  
**Created:** 2026-02-18  
**Author:** QA Team  
**Status:** Approved for Execution  
**Scope:** Three Workflow PRDs — Complete Requirement-to-Test Mapping

---

## 1. Executive Summary

### 1.1 Overview

This document provides comprehensive traceability between **three Workflow Product Requirements Documents (PRDs)** and the existing test coverage across the Opportunity+ system. It enables stakeholders to:

- **Verify** that every PRD requirement has corresponding test cases
- **Identify** coverage gaps requiring additional test development
- **Track** implementation status of workflow features
- **Support** release readiness and audit compliance

### 1.2 PRD Requirements Summary

| PRD | Goals | User Stories | Functional Requirements | Acceptance Criteria | Total Requirements |
|-----|-------|--------------|-------------------------|-------------------|-------------------|
| **PRD 1:** The Go Decision (Phase 2) | 7 | 9 | 9 | 17 | **42** |
| **PRD 2:** Send Opportunity for Go Decision (Phase 1) | 7 | 12 | 18 | — | **37** |
| **PRD 3:** Workflow Submodule Integration | 8 | 8 | 18 | — | **34** |
| **TOTAL** | **22** | **29** | **45** | **17** | **113** |

### 1.3 Test Coverage Summary

| Test Asset Type | Count | Total Tests | Primary Coverage |
|-----------------|-------|-------------|------------------|
| **Markdown Test Case Documents** | 11 | ~5,082 | PRD 1, 2, 3 |
| **C# Integration/Unit Tests** | 10 | ~350+ | WorkflowController, Adapters, State Machine |
| **TOTAL UNIQUE TEST CASES** | — | **~3,200+** (deduplicated) | All PRDs |

### 1.4 Coverage Gaps Identified

| Priority | Gap Category | Count | Description |
|----------|--------------|-------|-------------|
| **High** | Missing E2E Tests | 8 | Actions Required card, Notification bell, Decision info panel — no automated E2E |
| **High** | Partial Coverage | 5 | Email CC recipients, Executive assignment — backend only |
| **Medium** | UI Component Tests | 6 | Approve/Reject dialogs, Stage stepper display logic |
| **Medium** | Submodule Integration | 4 | UNOPS.Workflow schema, adapter registration |
| **Low** | Documentation Tests | 3 | README, troubleshooting guide |

---

## 2. Implementation Status Matrix

| Backend Component | Path | PRD | Status | Test Coverage |
|-------------------|------|-----|--------|---------------|
| WorkflowController | `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | 2, 3 | ✅ Implemented | WorkflowControllerTests.cs (60 tests) |
| WorkflowManager (Submodule) | `UNOPS.Workflow/UNOPS.Workflow.Business/` | 3 | ✅ Implemented | OpportunityWorkflowTests.cs |
| PaoWorkflowApproverProvider | `UNOPS.PAO.Business/Workflow/Adapters/` | 2, 3 | ✅ Implemented | PaoWorkflowApproverProviderTests.cs |
| PaoEntityStageProvider | `UNOPS.PAO.Business/Workflow/Adapters/` | 3 | ✅ Implemented | PaoEntityStageProviderTests.cs |
| PaoWorkflowNotificationService | `UNOPS.PAO.Business/Workflow/Adapters/` | 2, 3 | ✅ Implemented | PaoWorkflowNotificationServiceCCTests.cs |
| PaoWorkflowUserContext | `UNOPS.PAO.Business/Workflow/` | 3 | ✅ Implemented | PaoWorkflowUserContextTests.cs |
| OpportunityStageRequirementsProvider | `UNOPS.PAO.Business/Workflow/StageRequirements/` | 2 | ✅ Implemented | WorkflowManager_TestCases.md |
| OpportunityWorkflow (State Machine) | `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs` | 3 | ✅ Implemented | OpportunityWorkflowTests.cs |
| StateMachineStageChangeSeeder | `UNOPS.PAO.Business/Workflow/Seeders/` | 2, 3 | ✅ Implemented | WorkflowManager_TestCases.md |
| ApproveOpportunityDialogComponent | `opportunity/approve-opportunity-dialog/` | 1 | ⚠️ Partial | TheGoDecision_TestCases.md (manual) |
| RejectOpportunityDialogComponent | `opportunity/reject-opportunity-dialog/` | 1 | ⚠️ Partial | TheGoDecision_TestCases.md (manual) |
| OpportunityDecisionInfoPanelComponent | `opportunity/opportunity-decision-info-panel/` | 1 | ⚠️ Partial | TheGoDecision_TestCases.md (manual) |
| Home Dashboard (Actions Required) | `features/home/components/home-dashboard/` | 1 | ⚠️ Partial | TheGoDecision_TestCases.md (manual) |
| Notification Bell (Workflow) | `layouts/components/topbar/` | 1 | ⚠️ Partial | TheGoDecision_TestCases.md (manual) |
| OpportunityManager (Immutability) | `UNOPS.PAO.Business/Managers/OpportunityManager.cs` | 1 | ✅ Implemented | TheGoDecision_TestCases.md |

---

## 3. PRD 1 Traceability: The Go Decision (Phase 2)

**PRD File:** `tasks/the-go-decision/the-go-decision-prd.md`

### 3.1 Goals Traceability

| Goal ID | Description | Test File(s) | Test IDs | Coverage Status |
|---------|-------------|--------------|----------|-----------------|
| G-1 | Integrate with Actions Required Card | TheGoDecision_TestCases.md | POS-001, POS-002, FUN-001–003, BND-001–003 | ✅ Covered |
| G-2 | Integrate with Notification Bell | TheGoDecision_TestCases.md | POS-003, POS-004, FUN-004–006, BND-004–006 | ✅ Covered |
| G-3 | Create Decision-Maker Review Interface | TheGoDecision_TestCases.md | POS-005, POS-006, FUN-007–010, BND-007–010 | ✅ Covered |
| G-4 | Implement Go Decision Workflow | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-007–012, FUN-016–025, POS-015–019 | ✅ Covered |
| G-5 | Implement No-Go Decision Workflow | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-013–015, FUN-026–030, POS-020–022 | ✅ Covered |
| G-6 | Enforce Post-Decision Immutability | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035, NEG-051–060 | ✅ Covered |
| G-7 | Add CC Recipients to Email Notifications | TheGoDecision_TestCases.md | POS-019–021, POS-030, FUN-036–040 | ✅ Covered |

### 3.2 User Stories Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| US-1 | Decision-Maker Sees Task in Actions Required Card | TheGoDecision_TestCases.md | POS-001, POS-002, NEG-001–002, FUN-001–003 | ✅ Covered |
| US-2 | Decision-Maker Sees Notification in Bell | TheGoDecision_TestCases.md | POS-003, POS-004, NEG-003–005, FUN-004–006 | ✅ Covered |
| US-3 | Decision-Maker Views Instructional Guidance | TheGoDecision_TestCases.md | POS-005, POS-006, NEG-006–010, FUN-007–010 | ✅ Covered |
| US-4 | Decision-Maker Views Highlighted Information Panel | TheGoDecision_TestCases.md | FUN-011–015, POS-027, POS-028, NEG-011–020 | ✅ Covered |
| US-5 | Decision-Maker Views Read-Only Record | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035, NEG-051–055 | ✅ Covered |
| US-6 | Decision-Maker Approves with Go Decision | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-007–012, POS-015–019, FUN-016–025, NEG-021–040 | ✅ Covered |
| US-7 | Decision-Maker Rejects with No-Go Decision | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-013–015, POS-020–022, FUN-026–030, NEG-041–050 | ✅ Covered |
| US-8 | Record Becomes Immutable After Decision | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035, NEG-051–060, BND-051–060 | ✅ Covered |
| US-9 | Email Notifications Include CC Recipients | TheGoDecision_TestCases.md | POS-019–021, POS-030, FUN-036–040 | ✅ Covered |

### 3.3 Functional Requirements Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| FR-1 | Integrate Workflow Tasks with Actions Required Card | TheGoDecision_TestCases.md | POS-001, POS-002, FUN-001–003, INT-001–005 | ⚠️ No E2E |
| FR-2 | Integrate Workflow Notifications with Notification Bell | TheGoDecision_TestCases.md | POS-003, POS-004, FUN-004–006 | ⚠️ No E2E |
| FR-3 | Create Opportunity Decision Info Panel Component | TheGoDecision_TestCases.md | FUN-011–015, POS-027, POS-028 | ⚠️ Manual only |
| FR-4 | Create Approve Opportunity Dialog Component | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-007–012, FUN-016–025, POS-015–019 | ✅ Covered |
| FR-5 | Create Reject Opportunity Dialog Component | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-013–015, FUN-026–030, POS-020–022 | ✅ Covered |
| FR-6 | Enhance Approve Endpoint with Rationale and Executive Assignment | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-015, NEG-021–030, Approve_* tests | ✅ Covered |
| FR-7 | Enhance Reject Endpoint with Rationale | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-020–022, Reject_* tests | ✅ Covered |
| FR-8 | Enforce Post-Decision Immutability | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035, NEG-051–060 | ✅ Covered |
| FR-9 | Add CC Recipients to Email Notifications | TheGoDecision_TestCases.md | POS-019–021, POS-030, FUN-036–040 | ⚠️ Backend only |

### 3.4 Acceptance Criteria Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| AC 1.1 | Verify email notification sent to all DoA holders with CC recipients | TheGoDecision_TestCases.md | POS-019–021, POS-030 | ✅ Covered |
| AC 1.4 | Verify task appears in Actions Required card and notification bell | TheGoDecision_TestCases.md | POS-001–004, FUN-001–006 | ✅ Covered |
| AC 2.1 | Verify notification directs to Statement section showing static snapshot | TheGoDecision_TestCases.md | POS-004, FUN-005 | ✅ Covered |
| AC 2.2 | Verify instructional text is displayed | TheGoDecision_TestCases.md | POS-005, FUN-007–010 | ✅ Covered |
| AC 2.3 | Verify highlighted data points (initiative type, time, DD status, risks, remarks) | TheGoDecision_TestCases.md | FUN-011–015, POS-027, POS-028 | ✅ Covered |
| AC 2.4 | Verify record is read-only while in workflow | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035 | ✅ Covered |
| AC 3.1 | Verify Go decision requires confirmation with Org Unit ID, Name, Initiative Type | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-009, FUN-016–020 | ✅ Covered |
| AC 3.2 | Verify Decision Rationale is mandatory with helper text | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-010, NEG-021–025 | ✅ Covered |
| AC 3.3 | Verify Executive dropdown shows active personnel and is mandatory | TheGoDecision_TestCases.md | POS-022–024, NEG-071–075 | ✅ Covered |
| AC 3.4 | Verify Director/Manager/OiC suggested as default Executive selection | TheGoDecision_TestCases.md | FUN-041–045 | ✅ Covered |
| AC 3.5 | Verify selected Executive is stored on Opportunity.ExecutiveId after Go decision | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-012, POS-023, POS-024 | ✅ Covered |
| AC 4.1 | Verify No-Go requires acknowledgment of specific statement | TheGoDecision_TestCases.md | POS-014, FUN-026–028 | ✅ Covered |
| AC 4.2 | Verify Decision Rationale is mandatory for rejection | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-015, NEG-041–045 | ✅ Covered |
| AC 4.3 | Verify stage updates to NO GO immediately upon rejection | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-015, POS-020 | ✅ Covered |
| AC 5.1 | Verify email notifications sent to appropriate recipients | TheGoDecision_TestCases.md | POS-019–021, POS-030 | ✅ Covered |
| AC 6.1 | Verify audit trail captures initiation and decision dates/users | TheGoDecision_TestCases.md, WorkflowManager_TestCases.md | POS-025, POS-026, FUN-046–050 | ✅ Covered |
| AC 6.2 | Verify record becomes static read-only artifact after decision | TheGoDecision_TestCases.md | POS-016–018, FUN-031–035 | ✅ Covered |

---

## 4. PRD 2 Traceability: Send Opportunity for Go Decision (Phase 1)

**PRD File:** `tasks/send-opportunity-for-go-decision/send-opportunity-for-go-decision-prd.md`

### 4.1 Goals Traceability

| Goal ID | Description | Test File(s) | Test IDs | Coverage Status |
|---------|-------------|--------------|----------|-----------------|
| G-1 | Implement DoA Level 2 approver lookup | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-004, NEG-030, NEG-041, FUN-* DoA2 | ✅ Covered |
| G-2 | Create OpportunityStageRequirements | WorkflowManager_TestCases.md, OpportunityStatementValidation_TestCases.md | POS-006, NEG-021–040, FUN-* requirements | ✅ Covered |
| G-3 | Trigger Opportunity Statement regeneration | WorkflowManager_TestCases.md, OpportunityStatementValidation_TestCases.md | POS-011, FUN-031–040 | ✅ Covered |
| G-4 | Implement email notifications | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-023–026, NEG-051–060 | ✅ Covered |
| G-5 | Add country-org unit relationship warning | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-009, NEG-009, Submit_* tests | ✅ Covered |
| G-6 | Enable OM recall | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-023–025, NEG-* recall | ✅ Covered |
| G-7 | Add non-OM submitter warning | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-008, NEG-* NonOM | ✅ Covered |

### 4.2 User Stories Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| US-1 | Opportunity Manager Submits for Go Decision | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-001–007, POS-011–014, NEG-011–030 | ✅ Covered |
| US-2 | Non-OM User Receives Warning Before Submission | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-008, NEG-* NonOM | ✅ Covered |
| US-3 | System Validates Mandatory Fields | WorkflowManager_TestCases.md, OpportunityStatementValidation_TestCases.md | POS-006, NEG-021–040, FUN-* requirements | ✅ Covered |
| US-4 | Country-Org Unit Relationship Warning | WorkflowManager_TestCases.md | POS-009, NEG-009 | ✅ Covered |
| US-5 | DoA Holder Receives Approval Request Notification | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-023, NEG-051–053 | ✅ Covered |
| US-6 | Decision Maker Approves Opportunity | WorkflowManager_TestCases.md, OpportunityWorkflow_TestCases.md | POS-015–019, POS-013–015 | ✅ Covered |
| US-7 | Decision Maker Rejects Opportunity (Sets to NO GO) | WorkflowManager_TestCases.md, OpportunityRejectStatus_TestCases.md | POS-020–022, NEG-* reject | ✅ Covered |
| US-8 | Opportunity Manager Reopens NO GO Opportunity | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-027, NEG-006 | ✅ Covered |
| US-9 | Opportunity Manager Recalls Submission | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-023–025, NEG-* recall | ✅ Covered |
| US-10 | User Views Opportunity in Workflow | WorkflowManager_TestCases.md, OpportunityWorkflow_TestCases.md | POS-002–005, FUN-* workflow display | ✅ Covered |
| US-11 | Opportunity Manager Cancels Opportunity | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-026, NEG-031 | ✅ Covered |
| US-12 | Opportunity Manager Reopens Cancelled Opportunity | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-028, NEG-007 | ✅ Covered |

### 4.3 Functional Requirements Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| FR-1 | Update Approver Lookup for DoA Level 2 | WorkflowManager_TestCases.md, PaoWorkflowApproverProviderTests.cs | POS-004, NEG-030, NEG-041 | ✅ Covered |
| FR-2 | Create OpportunityStageRequirements Class | WorkflowManager_TestCases.md, OpportunityStatementValidation_TestCases.md | POS-006, NEG-021–040 | ✅ Covered |
| FR-3 | Create RequirementsValidationManager | WorkflowManager_TestCases.md | GetRequirements tests | ✅ Covered |
| FR-4 | Add Requirements Validation API Endpoint | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-006, GET requirements tests | ✅ Covered |
| FR-5 | Trigger Opportunity Statement Regeneration | WorkflowManager_TestCases.md | POS-011 | ✅ Covered |
| FR-6 | Implement Non-OM Submitter Warning | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-008, Submit_NonOM_* | ✅ Covered |
| FR-7 | Implement Country-Org Unit Relationship Warning | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-009, Submit_OrgUnit_* | ✅ Covered |
| FR-8 | Enable OM Recall | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-023–025, Recall_* | ✅ Covered |
| FR-9 | Create Email Templates | PaoWorkflowNotificationServiceCCTests.cs | Notification tests | ✅ Covered |
| FR-10 | Update PaoWorkflowNotificationService | PaoWorkflowNotificationServiceCCTests.cs | All notification tests | ✅ Covered |
| FR-11 | Notify Internal Stakeholders on Go Decision | WorkflowManager_TestCases.md | POS-016, FUN-* stakeholder | ✅ Covered |
| FR-12 | Mandatory Acknowledgment Statement | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-010, NEG-* acknowledgment | ✅ Covered |
| FR-14 | Custom Rejection Handling (Rejection → NO GO) | WorkflowManager_TestCases.md, OpportunityRejectStatus_TestCases.md | POS-020–022, NEG-* reject | ✅ Covered |
| FR-15 | Reopen from NO GO Stage | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-027, Reopen_* | ✅ Covered |
| FR-16 | Stage Stepper Display Logic | OpportunityWorkflow_TestCases.md | FUN-* stepper | ⚠️ Partial |
| FR-17 | Cancel Opportunity (IDENTIFY & PROFILE → CANCELLED) | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-026, Cancel_* | ✅ Covered |
| FR-18 | Reopen from CANCELLED Stage | WorkflowManager_TestCases.md, WorkflowControllerTests.cs | POS-028, Reopen_* | ✅ Covered |

---

## 5. PRD 3 Traceability: Workflow Submodule Integration

**PRD File:** `tasks/workflow-submodule-integration/workflow-submodule-integration-prd.md`

### 5.1 Goals Traceability

| Goal ID | Description | Test File(s) | Test IDs | Coverage Status |
|---------|-------------|--------------|----------|-----------------|
| G-1 | Successfully integrate UNOPS.Workflow submodule | OpportunityWorkflowTests.cs, WorkflowManager_TestCases.md | Integration tests | ✅ Covered |
| G-2 | Replace existing WorkflowStage system | OpportunityWorkflowTests.cs | State machine tests | ✅ Covered |
| G-3 | Implement 4 required interfaces | PaoWorkflowUserContextTests, PaoEntityStageProviderTests, PaoWorkflowApproverProviderTests, PaoWorkflowNotificationServiceCCTests | All adapter tests | ✅ Covered |
| G-4 | Establish separate workflow database schema | OpportunityWorkflowTests.cs | Schema creation tests | ⚠️ Manual |
| G-5 | Create example Opportunity workflow | OpportunityWorkflowTests.cs, WorkflowManager_TestCases.md | State transition tests | ✅ Covered |
| G-6 | Replace existing Angular workflow components | OpportunityWorkflow_TestCases.md | UI workflow tests | ✅ Covered |
| G-7 | Maintain zero breaking changes | WorkflowManager_TestCases.md | Regression tests | ✅ Covered |
| G-8 | Enable future approval workflows | WorkflowManager_TestCases.md | Extensibility tests | ✅ Covered |

### 5.2 User Stories Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| US-1 | Developer Setting Up Workflow Infrastructure | OpportunityWorkflowTests.cs | Compilation, DI registration | ✅ Covered |
| US-2 | Developer Implementing Entity Workflow | OpportunityWorkflowTests.cs | StateMachine tests | ✅ Covered |
| US-3 | Developer Configuring Stage Transitions | StateMachineStageChangeSeederTests.cs, WorkflowManager_TestCases.md | Seeder, transition tests | ✅ Covered |
| US-4 | Developer Testing Opportunity Workflow | OpportunityWorkflowTests.cs, WorkflowManager_TestCases.md | Full workflow cycle | ✅ Covered |
| US-5 | System Administrator Managing Workflows | WorkflowManager_TestCases.md | Query tests | ⚠️ Manual |
| US-6 | Internal User Viewing Entity Workflow Status | OpportunityWorkflow_TestCases.md, WorkflowManager_TestCases.md | POS-002–005 | ✅ Covered |
| US-7 | Internal User Submitting for Approval | WorkflowManager_TestCases.md, PNO-969_GoDecision_TestCases.md | POS-007–014 | ✅ Covered |
| US-8 | Developer Extending to Other Entities | WorkflowManager_TestCases.md | Extensibility section | ⚠️ Future |

### 5.3 Functional Requirements Traceability

| Requirement ID | Description | Test File(s) | Test IDs | Coverage Status |
|----------------|-------------|--------------|----------|-----------------|
| FR-1 | Submodule Integration | OpportunityWorkflowTests.cs | Compilation tests | ✅ Covered |
| FR-2 | Entity Migration (Stage property) | OpportunityWorkflowTests.cs | Stage property tests | ✅ Covered |
| FR-2.5 | Add WorkflowStatus to Base Entity | OpportunityWorkflowTests.cs | WorkflowStatus tests | ✅ Covered |
| FR-3 | Delete Old Workflow System | OpportunityWorkflowTests.cs | No references to old entities | ✅ Covered |
| FR-4 | Implement IWorkflowUserContext | PaoWorkflowUserContextTests.cs | All UserContext tests | ✅ Covered |
| FR-5 | Implement IEntityStageProvider | PaoEntityStageProviderTests.cs | GetCurrentStage, UpdateStage tests | ✅ Covered |
| FR-6 | Implement IWorkflowApproverProvider | PaoWorkflowApproverProviderTests.cs | GetApprovers, CanUserApprove tests | ✅ Covered |
| FR-7 | Implement IWorkflowNotificationService | PaoWorkflowNotificationServiceCCTests.cs | Notify* tests | ✅ Covered |
| FR-8 | Database Schema Setup | OpportunityWorkflowTests.cs | Schema verification | ⚠️ Manual |
| FR-9 | Service Registration | OpportunityWorkflowTests.cs | DI resolution tests | ✅ Covered |
| FR-10 | Create Opportunity Workflow State Machine | OpportunityWorkflowTests.cs | StateMachine definition tests | ✅ Covered |
| FR-11 | Seed Opportunity Stage Transitions | StateMachineStageChangeSeederTests.cs, WorkflowManager_TestCases.md | Seeder, transition tests | ✅ Covered |
| FR-12 | Create API Endpoints | WorkflowControllerTests.cs, WorkflowManager_TestCases.md | All endpoint tests | ✅ Covered |
| FR-13 | Update OpportunityManager | WorkflowManager_TestCases.md | GetWorkflowState, ChangeStage | ✅ Covered |
| FR-14 | Integrate Submodule's Angular Workflow Library | OpportunityWorkflow_TestCases.md | UI integration tests | ✅ Covered |
| FR-15 | Integrate Workflow in Opportunity UI | OpportunityWorkflow_TestCases.md | StageWorkflowComponent tests | ✅ Covered |
| FR-16 | Update Opportunity Models | WorkflowManager_TestCases.md | Stage, WorkflowState in models | ✅ Covered |
| FR-17 | Create Unit Tests | All C# test files | 350+ tests | ✅ Covered |
| FR-18 | Documentation | — | — | ⚠️ No automated tests |

---

## 6. Gap Analysis

### 6.1 High Priority Gaps

| Gap ID | PRD | Requirement | Description | Recommended Action |
|--------|-----|-------------|-------------|---------------------|
| GAP-H1 | PRD 1 | FR-1, FR-2 | Actions Required card and Notification bell — no automated E2E tests | Create Playwright E2E: `workflow-actions-required.spec.ts`, `workflow-notification-bell.spec.ts` |
| GAP-H2 | PRD 1 | FR-3 | Opportunity Decision Info Panel — manual testing only | Create component unit tests + E2E for info panel display |
| GAP-H3 | PRD 1 | FR-9 | Email CC recipients — backend logic not directly asserted | Add PaoWorkflowNotificationService tests asserting CC list (OM, initiator, Director/Manager) |
| GAP-H4 | PRD 1 | US-6 | Executive assignment — no E2E for dropdown and persistence | Add E2E: Approve with Executive selection, verify ExecutiveId in DB |
| GAP-H5 | PRD 2 | FR-16 | Stage stepper display logic — limited coverage | Add frontend unit tests for `getDisplayStages()` happy path vs NO GO vs CANCELLED |

### 6.2 Medium Priority Gaps

| Gap ID | PRD | Requirement | Description | Recommended Action |
|--------|-----|-------------|-------------|---------------------|
| GAP-M1 | PRD 1 | FR-4, FR-5 | Approve/Reject dialogs — no isolated component tests | Create `approve-opportunity-dialog.spec.ts`, `reject-opportunity-dialog.spec.ts` |
| GAP-M2 | PRD 3 | FR-8 | Workflow schema auto-creation — manual verification | Add integration test: Verify workflow schema tables exist after DbContext init |
| GAP-M3 | PRD 3 | US-5 | Admin workflow config queries — manual only | Document manual test procedure for StateMachineStageChanges, WorkflowLogs |
| GAP-M4 | PRD 3 | FR-18 | Documentation — no automated checks | Add link-checker or doc-build validation to CI |
| GAP-M5 | PRD 2 | US-3 | 21 mandatory fields — edge cases for conditional validators | Add BND tests for BeneficiariesToBeDetermined, SDG minLength=1 edge cases |

### 6.3 Low Priority Gaps

| Gap ID | PRD | Requirement | Description | Recommended Action |
|--------|-----|-------------|-------------|---------------------|
| GAP-L1 | PRD 3 | US-8 | Extending to other entities — future scope | Defer until Partner/Contact workflow PRD |
| GAP-L2 | All | — | Test execution time tracking | Add duration metrics to test reports |
| GAP-L3 | All | — | Traceability automation | Consider requirement-to-test tagging in test IDs |

---

## 7. Cross-PRD Test Coverage Matrix

| Test File | PRD 1 | PRD 2 | PRD 3 | Primary Focus |
|-----------|-------|-------|-------|---------------|
| **WorkflowManager_TestCases.md** | ●● | ●●● | ●●● | Submit, Approve, Reject, Recall, Cancel, Reopen, Requirements |
| **TheGoDecision_TestCases.md** | ●●● | ● | — | Decision-maker UI, Actions Required, Notification, Dialogs, Immutability |
| **OpportunityWorkflow_TestCases.md** | ● | ●● | ●● | State transitions, approvals, notifications |
| **PNO-969_GoDecision_TestCases.md** | ● | ●●● | ● | Phase 1 send for go decision, stage matrix |
| **GoNoGoDecision_TestCases.md** | ● | ●● | ● | Legacy/supplementary |
| **GoNoGoDecision_PRD_TestCases.md** | ●● | ●● | ● | PRD-specific mappings |
| **OpportunityWorkflowStatus_TestCases.md** | ● | ●● | ●● | Status transitions |
| **OpportunityRejectStatus_TestCases.md** | ● | ●● | — | Reject → NO GO |
| **DOA3Fallback_TestCases.md** | — | ● | — | DoA3 fallback (PNO-1197) |
| **OpportunityStatementValidation_TestCases.md** | — | ●● | — | Statement validation, 21 fields |
| **SubmitApprovalDialogUX_TestCases.md** | ● | ● | — | Dialog UX |
| **WorkflowControllerTests.cs** | ● | ●●● | ●● | API endpoints, validation, warnings |
| **PaoWorkflowApproverProviderTests.cs** | — | ●● | ●● | DoA2 lookup |
| **PaoWorkflowNotificationServiceCCTests.cs** | ● | ●● | ●● | Email notifications |
| **PaoWorkflowUserContextTests.cs** | — | — | ●●● | User context |
| **PaoEntityStageProviderTests.cs** | — | — | ●●● | Stage provider |
| **OpportunityWorkflowTests.cs** | — | ● | ●●● | State machine, workflow engine |

**Legend:** ● = Partial coverage (1–33%), ●● = Good coverage (34–66%), ●●● = Strong coverage (67–100%)

---

## 8. Test Statistics

### 8.1 Total Unique Test Cases (Estimated)

| Category | Markdown Documents | C# Tests | Estimated Unique |
|----------|-------------------|----------|------------------|
| Positive | ~330 | ~80 | ~350 |
| Negative | ~990 | ~120 | ~950 |
| Boundary/Edge | ~990 | ~50 | ~900 |
| Functional | ~990 | ~60 | ~950 |
| Integration | ~990 | ~40 | ~850 |
| Security | ~50 | ~20 | ~60 |
| Concurrency | ~250 | ~10 | ~220 |
| Unit | ~210 | ~100 | ~250 |
| Performance | ~160 | ~5 | ~150 |
| Load | ~100 | ~0 | ~100 |
| **TOTAL** | **~5,082** | **~485** | **~3,200+** |

*Note: Deduplication applied — many test cases cover multiple PRD requirements.*

### 8.2 Distribution by Category (3:1 Ratio Compliance)

| Check | Required | Actual (Markdown) | Status |
|-------|----------|-------------------|--------|
| N ≥ 3P | Negative ≥ 3×Positive | 990 ≥ 3×330 = 990 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3×Positive | 990 ≥ 990 | ✅ |
| F ≥ 3P | Functional ≥ 3×Positive | 990 ≥ 990 | ✅ |
| I ≥ 3P | Integration ≥ 3×Positive | 990 ≥ 990 | ✅ |

### 8.3 Coverage Percentage per PRD

| PRD | Requirements | Covered | Partial | Gaps | Coverage % |
|-----|--------------|---------|---------|------|------------|
| PRD 1: The Go Decision | 42 | 35 | 5 | 2 | **83%** |
| PRD 2: Send Opportunity for Go Decision | 37 | 34 | 2 | 1 | **92%** |
| PRD 3: Workflow Submodule Integration | 34 | 28 | 4 | 2 | **82%** |
| **Overall** | **113** | **97** | **11** | **5** | **86%** |

---

## 9. Recommended Actions

### 9.1 Immediate (Sprint 1)

1. **Create E2E tests for Actions Required and Notification Bell** (GAP-H1)  
   - Playwright specs: `workflow-actions-required.spec.ts`, `workflow-notification-bell.spec.ts`  
   - Assert: Pending Go decision appears, click navigates to opportunity

2. **Add CC recipient assertions to PaoWorkflowNotificationServiceCCTests** (GAP-H3)  
   - Verify CC list includes OM, initiator (if different), Director/Manager  
   - Mock EntityUserRole for Director/Manager lookup

3. **Add Executive assignment E2E test** (GAP-H4)  
   - Approve with Executive selected → verify Opportunity.ExecutiveId in DB

### 9.2 Short-Term (Sprint 2)

4. **Create Opportunity Decision Info Panel component tests** (GAP-H2)  
   - Unit test: proposedInitiativeType, timeToSigning, concerningDDStatuses, highRisks, senderRemarks  
   - E2E: Panel visible for DoA2 on pending opportunity

5. **Add Stage stepper display logic tests** (GAP-H5)  
   - Frontend unit test: `getDisplayStages()` for happy path, NO GO, CANCELLED

6. **Create Approve/Reject dialog component specs** (GAP-M1)  
   - Isolated component tests for confirmation statement, rationale, Executive dropdown

### 9.3 Medium-Term (Backlog)

7. **Workflow schema integration test** (GAP-M2)  
   - Verify workflow.StateMachineStageChanges, StateMachineStageChangeRoles, WorkflowLogs exist

8. **Documentation validation** (GAP-M4)  
   - CI step: Link checker for README, troubleshooting guide

9. **Beneficiaries/SDG edge case tests** (GAP-M5)  
   - BND tests for conditional validators

---

## 10. Appendices

### Appendix A: State Machine Transitions

| # | From Stage | To Stage | Action | Approval Required | Trigger Role | Approve Role |
|---|------------|----------|--------|-------------------|--------------|--------------|
| 1 | IDENTIFY & PROFILE | GO | Submit for Go | Yes | Opportunity Manager | DoA2 Holder |
| 2 | IDENTIFY & PROFILE | NO GO | Submit for No Go | Yes | Opportunity Manager | DoA2 Holder |
| 3 | IDENTIFY & PROFILE | CANCELLED | Cancel | No | Opportunity Manager | N/A |
| 4 | NO GO | IDENTIFY & PROFILE | Reopen | No | Opportunity Manager | N/A |
| 5 | CANCELLED | IDENTIFY & PROFILE | Reopen | No | Opportunity Manager | N/A |

**Note:** GO is the final stage (no transitions out).

---

### Appendix B: 21 Mandatory GO Fields

| # | Field | Description Key | Validation |
|---|-------|-----------------|------------|
| 1 | Opportunity Name | message.requirements.opportunity.nameRequired | required |
| 2 | Description | message.requirements.opportunity.descriptionRequired | required |
| 3 | Proposed Budget | message.requirements.opportunity.budgetRequired | required |
| 4 | Context & Challenges | message.requirements.opportunity.challengesRequired | required |
| 5 | Strategic Missions | message.requirements.opportunity.missionsRequired | minLength = 1 |
| 6 | Expected Impact | message.requirements.opportunity.impactRequired | required |
| 7 | Expected Outcomes | message.requirements.opportunity.outcomesRequired | required |
| 8 | Beneficiaries | message.requirements.opportunity.beneficiariesRequired | conditional |
| 9 | SDG Alignment | message.requirements.opportunity.sdgRequired | minLength = 1 |
| 10 | Funding Partners | message.requirements.opportunity.fundingPartnerRequired | minLength = 1 |
| 11 | Client Partners | message.requirements.opportunity.clientPartnerRequired | minLength = 1 |
| 12 | Products & Services | message.requirements.opportunity.productsRequired | minLength = 1 |
| 13 | Countries | message.requirements.opportunity.countriesRequired | minLength = 1 |
| 14 | Target Signing Date | message.requirements.opportunity.signingDateRequired | required |
| 15 | Implementation Start | message.requirements.opportunity.startDateRequired | required |
| 16 | Implementation End | message.requirements.opportunity.endDateRequired | required |
| 17 | Opportunity Manager | message.requirements.opportunity.managerRequired | required |
| 18 | Responsible Org Unit | message.requirements.opportunity.orgUnitRequired | required |
| 19 | Initiative Type | message.requirements.opportunity.initiativeTypeRequired | required |
| 20 | DoA2 Holder | message.requirements.opportunity.doaHolderRequired | server-side only |
| 21 | Opportunity Statement | message.requirements.opportunity.statementRequired | required |

---

### Appendix C: API Endpoints Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/workflow/{entityName}` | Get workflow stages for entity type |
| GET | `/api/workflow/{entityName}/{id}` | Get current state and available actions |
| GET | `/api/workflow/{entityName}/{id}/details` | Get workflow details including approvers |
| GET | `/api/workflow/{entityName}/{id}/history` | Get stage change history |
| GET | `/api/workflow/{entityName}/{id}/requirements/{nextStage?}` | Get requirements for stage change |
| GET | `/api/workflow/pending-approvals` | Get pending approvals for current user (PRD 1) |
| POST | `/api/workflow/submit` | Submit/initiate workflow action |
| POST | `/api/workflow/approve` | Approve pending workflow (rationale, Executive required for Opportunity) |
| POST | `/api/workflow/reject` | Reject pending workflow (→ NO GO for Opportunity) |
| POST | `/api/workflow/recall` | Recall pending workflow |
| POST | `/api/workflow/cancel` | Cancel from IDENTIFY & PROFILE → CANCELLED |
| POST | `/api/workflow/reopen` | Reopen from NO GO or CANCELLED → IDENTIFY & PROFILE |

---

### Appendix D: Notification Matrix

| Event | To | CC | Template |
|-------|-----|-----|----------|
| **Submit for Go** | DoA2 holders | OM, initiator (if ≠ OM), Director/Manager | WorkflowApprovalRequest.html |
| **Approve (Go)** | Submitter, OM | — | WorkflowCompleted.html |
| **Reject (No-Go)** | Submitter, OM | — | WorkflowRejected.html |
| **Recall** | DoA2 holders | — | WorkflowRecalled.html |
| **Go Decision (Internal Stakeholders)** | Org units responsible for implementation countries (excluding opportunity's org unit) | — | Internal notification |

**Email Subject Patterns:**
- Approval Request: `PAO: [Opportunity Name] - Action Required`
- Go Approved: `PAO: [Opportunity Name] - Go Decision Approved`
- Set to NO GO: `PAO: [Opportunity Name] - Set to NO GO`
- Recalled: `PAO: [Opportunity Name] - Submission Recalled`

---

*Document Version: 1.0 | Created: 2026-02-18 | Last Updated: 2026-02-18*
