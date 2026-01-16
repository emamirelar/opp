# Opportunity Tests - Phased Implementation Plan

**Created:** January 16, 2026  
**Scope:** Outstanding Opportunity tests to implement, sprint-by-sprint  
**Source Docs:** `QA Tests/Opportunity Tests/README.md` and current C# test inventory

---

## Current C# Test Inventory (Detected)

These exist today in `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/`:

- `Opportunity/UNOPSOpportunityManagerTests.cs`
- `Opportunity/OpportunityValidationTests.cs`
- `Opportunity/OpportunityPermissionTests.cs`
- `Opportunity/OpportunityIntegrationTests.cs`
- `Opportunity/OpportunityAdvancedFeaturesTests.cs`
- `Validation/OpportunityFieldLengthValidationTests.cs`

**Gap:** Manager, business logic, controller, service, and E2E test suites listed in the Opportunity test documentation are not yet implemented in C# (or are only scaffolded in archive). The plan below maps those documented specs to sprint deliverables.

---

## Phasing Principles

- **Sprint 1–2:** Core manager coverage (P0/P1) + validation/permission gaps
- **Sprint 3:** Business logic + workflow and AI extraction rules
- **Sprint 4:** Controllers + services + integration/E2E scenarios
- **Do not block CI:** Keep Opportunity tests isolated until the backend is ready

---

## Sprint Plan (4 Sprints)

### Sprint 1 — Core Opportunity Managers (P0)
**Goal:** Core CRUD, lifecycle, and DST foundations

**Implement C# tests for:**
- `OpportunityManagerTests.cs`
- `DSTManagerTests.cs`
- `DecisionManagerTests.cs`

**Doc mapping:**
- `Opportunity Tests/Managers/OpportunityManager_TestCases.md`
- `Opportunity Tests/Managers/DSTManager_TestCases.md`
- `Opportunity Tests/Managers/DecisionManager_TestCases.md`

**Target folders:**
- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/Managers/`

**Exit criteria:**
- Core create/update/state transition tests pass
- DST profile generation and decision package tests pass

---

### Sprint 2 — Opportunity Products (P0/P1)
**Goal:** Budget, schedule, resources, risk, indices

**Implement C# tests for:**
- `OpportunityBudgetManagerTests.cs`
- `OpportunityScheduleManagerTests.cs`
- `ResourcePlanManagerTests.cs`
- `RiskManagerTests.cs`
- `GlobalIndicesManagerTests.cs`

**Doc mapping:**
- `Opportunity Tests/Managers/OpportunityBudgetManager_TestCases.md`
- `Opportunity Tests/Managers/OpportunityScheduleManager_TestCases.md`
- `Opportunity Tests/Managers/ResourcePlanManager_TestCases.md`
- `Opportunity Tests/Managers/RiskManager_TestCases.md`
- `Opportunity Tests/Managers/GlobalIndicesManager_TestCases.md`

**Target folders:**
- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/Managers/`

**Exit criteria:**
- Budget, schedule, resource, risk workflows validated
- Indices upload and versioning tests pass

---

### Sprint 3 — Business Logic (P1)
**Goal:** Workflow rules, AI extraction, statements, agreements

**Implement C# tests for:**
- `OpportunityWorkflowTests.cs`
- `DSTProfilerTests.cs`
- `DocumentExtractionTests.cs`
- `OpportunityStatementTests.cs`
- `GoNoGoDecisionTests.cs`
- `AgreementLibraryTests.cs`

**Doc mapping:**
- `Opportunity Tests/BusinessLogic/OpportunityWorkflow_TestCases.md`
- `Opportunity Tests/BusinessLogic/DSTProfiler_TestCases.md`
- `Opportunity Tests/BusinessLogic/DocumentExtraction_TestCases.md`
- `Opportunity Tests/BusinessLogic/OpportunityStatement_TestCases.md`
- `Opportunity Tests/BusinessLogic/GoNoGoDecision_TestCases.md`
- `Opportunity Tests/BusinessLogic/AgreementLibrary_TestCases.md`

**Target folders:**
- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/BusinessLogic/`

**Exit criteria:**
- Workflow transitions, statement generation, and extraction rules pass
- Agreement pre-population and validation rules pass

---

### Sprint 4 — Controllers, Services, Integration/E2E (P1/P2)
**Goal:** API surface + orchestration + key integration paths

**Implement C# tests for:**
- Controllers:
  - `OpportunityControllerTests.cs`
  - `DSTControllerTests.cs`
  - `DecisionControllerTests.cs`
  - `OpportunityBudgetControllerTests.cs`
  - `OpportunityScheduleControllerTests.cs`
  - `ResourcePlanControllerTests.cs`
  - `GlobalIndicesControllerTests.cs`
  - `PartnershipAgreementControllerTests.cs`
- Services:
  - `OpportunityServiceTests.cs`
  - `DSTAnalysisServiceTests.cs`
  - `AgreementServiceTests.cs`
- Integration/E2E:
  - Start with scenarios in `Opportunity Tests/ADDITIONAL_E2E_SCENARIOS.md`
  - Use `Opportunity Tests/E2E_SCENARIOS_SUMMARY.md` for prioritization

**Doc mapping:**
- `Opportunity Tests/Controllers/*.md`
- `Opportunity Tests/Services/*.md`
- `Opportunity Tests/ADDITIONAL_E2E_SCENARIOS.md`
- `Opportunity Tests/E2E_SCENARIOS_SUMMARY.md`

**Target folders:**
- `QA Tests/Integration Tests/` (controllers/integration)
- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/Services/`
- `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/E2E/`

**Exit criteria:**
- Controller endpoints validated for core flows
- Service orchestration tests pass
- At least one E2E path per major workflow passes

---

## Sprint-to-File Mapping Summary

| Sprint | Primary Area | Test Case Docs | C# Target Folder |
|--------|--------------|----------------|------------------|
| 1 | Core Managers | `Managers/*.md` (Opportunity, DST, Decision) | `Opportunity/Managers/` |
| 2 | Product Managers | `Managers/*.md` (Budget, Schedule, Resource, Risk, Indices) | `Opportunity/Managers/` |
| 3 | Business Logic | `BusinessLogic/*.md` | `Opportunity/BusinessLogic/` |
| 4 | Controllers + Services + E2E | `Controllers/*.md`, `Services/*.md`, `ADDITIONAL_E2E_SCENARIOS.md` | `Integration Tests/`, `Opportunity/Services/`, `Opportunity/E2E/` |

---

## Dependencies / Notes

- Opportunity backend must exist for full execution; keep tests isolated until implementation stabilizes.
- Use existing helpers where available (e.g., `Helpers/OpportunityTestBuilder.cs`).
- Track progress in `QA Tests/Opportunity Tests/IMPLEMENTATION_STATUS.md` after each sprint.

---

## Recommended Next Step

Start Sprint 1 by implementing the three core manager test suites listed above, using their corresponding test case documents as the authoritative spec.
