# Opportunity Management Test Suite

**Created:** January 13, 2026  
**Status:** ✅ Complete - Enhanced with Advanced Coverage + Additional E2E Scenarios  
**Coverage:** ~605+ test cases across all Opportunity-related features  
**Includes:** Functional, Validation, Negative, Integration, Boundary, Edge Case, and 40 Additional Complex E2E Scenarios

---

## Overview

This test suite provides comprehensive coverage for all Opportunity Management features in the UNOPS Opportunity+ system, based on requirements from the Opportunity Epics PRD.

---

## Test Coverage Summary

| Category | Documentation Files | C# Test Files | Test Count | Priority |
|----------|---------------------|---------------|------------|----------|
| **Managers** | 8 files | 8 files | 180+ | P0-P1 |
| **Business Logic** | 6 files | 6 files | 150+ | P0-P1 |
| **Controllers** | 8 files | Integrated | 70+ | P1 |
| **Services** | 3 files | 3 files | 25+ | P2 |
| **Total** | **25 files** | **17 files** | **425+** | |

---

## Folder Structure

```
QA Tests/
└── Opportunity Tests/
    ├── README.md                                    # This file
    ├── Managers/                                    # Manager-level tests
    │   ├── OpportunityManager_TestCases.md         # Core opportunity CRUD
    │   ├── DSTManager_TestCases.md                 # Decision Support Tool
    │   ├── OpportunityBudgetManager_TestCases.md   # Budget management
    │   ├── OpportunityScheduleManager_TestCases.md # Schedule management
    │   ├── ResourcePlanManager_TestCases.md        # Resource planning
    │   ├── RiskManager_TestCases.md                # Risk register
    │   ├── DecisionManager_TestCases.md            # Go/No-Go decisions
    │   └── GlobalIndicesManager_TestCases.md       # Global indices
    ├── BusinessLogic/                               # Business rule tests
    │   ├── OpportunityWorkflow_TestCases.md        # Opportunity workflows
    │   ├── DSTProfiler_TestCases.md                # DST profiling logic
    │   ├── DocumentExtraction_TestCases.md         # AI document extraction
    │   ├── OpportunityStatement_TestCases.md       # Statement generation
    │   ├── GoNoGoDecision_TestCases.md             # Decision workflows
    │   └── AgreementLibrary_TestCases.md           # Partnership agreements
    ├── Controllers/                                 # API controller tests
    │   ├── OpportunityController_TestCases.md
    │   ├── DSTController_TestCases.md
    │   ├── DecisionController_TestCases.md
    │   ├── OpportunityBudgetController_TestCases.md
    │   ├── OpportunityScheduleController_TestCases.md
    │   ├── ResourcePlanController_TestCases.md
    │   ├── GlobalIndicesController_TestCases.md
    │   └── PartnershipAgreementController_TestCases.md
    └── Services/                                    # Service-level tests
        ├── OpportunityService_TestCases.md
        ├── DSTAnalysisService_TestCases.md
        └── AgreementService_TestCases.md

C# Tests/UNOPS.PAO.Business.Tests/
└── Opportunity/
    ├── Managers/                                    # Executable manager tests
    │   ├── OpportunityManagerTests.cs              # 50+ tests
    │   ├── DSTManagerTests.cs                      # 45+ tests
    │   ├── OpportunityBudgetManagerTests.cs        # 20+ tests
    │   ├── OpportunityScheduleManagerTests.cs      # 15+ tests
    │   ├── ResourcePlanManagerTests.cs             # 15+ tests
    │   ├── RiskManagerTests.cs                     # 15+ tests
    │   ├── DecisionManagerTests.cs                 # 25+ tests
    │   └── GlobalIndicesManagerTests.cs            # 15+ tests
    ├── BusinessLogic/                               # Executable business logic tests
    │   ├── OpportunityWorkflowTests.cs             # 35+ tests
    │   ├── DSTProfilerTests.cs                     # 40+ tests
    │   ├── DocumentExtractionTests.cs              # 30+ tests
    │   ├── OpportunityStatementTests.cs            # 20+ tests
    │   ├── GoNoGoDecisionTests.cs                  # 25+ tests
    │   └── AgreementLibraryTests.cs                # 20+ tests
    └── Services/                                    # Executable service tests
        ├── OpportunityServiceTests.cs              # 10+ tests
        ├── DSTAnalysisServiceTests.cs              # 10+ tests
        └── AgreementServiceTests.cs                # 10+ tests
```

---

## Feature Coverage

### 1. Opportunity Management (50+ tests)
**Manager:** `OpportunityManager`  
**Documentation:** `Managers/OpportunityManager_TestCases.md`  
**C# Tests:** `Opportunity/Managers/OpportunityManagerTests.cs`

**Key Areas:**
- Create opportunity with P3M entity properties
- AI-powered data suggestions (SDGs, UN frameworks)
- Opportunity status lifecycle (Active, On Hold, Closed, Recovered)
- Convert opportunity to Project/Programme/Portfolio
- Link to partnerships and engagements
- Version control and audit trail

---

### 2. Decision Support Tool (DST) (85+ tests)
**Managers:** `DSTManager`, `DSTAnalysisService`  
**Documentation:** `Managers/DSTManager_TestCases.md`, `BusinessLogic/DSTProfiler_TestCases.md`  
**C# Tests:** `Opportunity/Managers/DSTManagerTests.cs`, `Opportunity/BusinessLogic/DSTProfilerTests.cs`

**Key Areas:**
- Analyze feasibility, complexity, risk
- Assess strategic alignment with UNOPS goals
- Nine parameter evaluation:
  1. Strategic alignment
  2. Partners and stakeholders
  3. Physical implementation
  4. Context
  5. Scope and scale
  6. Timeframe
  7. Budget and resourcing
  8. Outcome and impact
  9. Safeguards, ethics, legal
- Generate profile reports
- Actionable recommendations (risks, personnel, structure)
- Similar project matching

---

### 3. Document Upload & AI Extraction (30+ tests)
**Business Logic:** `DocumentExtraction`  
**Documentation:** `BusinessLogic/DocumentExtraction_TestCases.md`  
**C# Tests:** `Opportunity/BusinessLogic/DocumentExtractionTests.cs`

**Key Areas:**
- Upload concept notes, correspondence, strategic plans
- Machine reading to extract structured data
- User verification of extracted data
- AI proposes data when not found directly
- Match document content to opportunity fields
- Handle multiple document types (PDF, Word, Excel)

---

### 4. Go/No-Go Decision Process (50+ tests)
**Managers:** `DecisionManager`  
**Documentation:** `Managers/DecisionManager_TestCases.md`, `BusinessLogic/GoNoGoDecision_TestCases.md`  
**C# Tests:** `Opportunity/Managers/DecisionManagerTests.cs`, `Opportunity/BusinessLogic/GoNoGoDecisionTests.cs`

**Key Areas:**
- Review decision package
- DOA holder decision making with rationale
- Decision authorization (budget, personnel)
- Audit trail and version control
- Decision delegation
- Required documentation checklist

---

### 5. Opportunity Management Products (65+ tests)
**Managers:** `OpportunityBudgetManager`, `OpportunityScheduleManager`, `ResourcePlanManager`, `RiskManager`

**Key Areas:**
- Draft high-level budget with fee calculations
- Draft high-level schedule (WBS from deliverables)
- Resource plan (development and implementation roles)
- Draft risk register linked to opportunity
- Spend rate visualization
- Personnel budgeting

---

### 6. Partnership Agreement Library (20+ tests)
**Business Logic:** `AgreementLibrary`  
**Documentation:** `BusinessLogic/AgreementLibrary_TestCases.md`  
**C# Tests:** `Opportunity/BusinessLogic/AgreementLibraryTests.cs`

**Key Areas:**
- Store agreements as artifacts
- Extract key terms (geography, scope, pricing)
- Link agreements to opportunities early
- Pre-populate opportunity fields from agreements
- Validate scope and pricing against agreements

---

### 7. Opportunity Statement & Concept Note (20+ tests)
**Business Logic:** `OpportunityStatement`  
**Documentation:** `BusinessLogic/OpportunityStatement_TestCases.md`  
**C# Tests:** `Opportunity/BusinessLogic/OpportunityStatementTests.cs`

**Key Areas:**
- Draft internal opportunity statement
- Use templates for structure
- Pre-populate with captured data
- Generate narrative text from data
- Support real-time collaboration
- Draft partner-facing concept note
- Tailor format to partner expectations

---

### 8. Global Indices Management (15+ tests)
**Manager:** `GlobalIndicesManager`  
**Documentation:** `Managers/GlobalIndicesManager_TestCases.md`  
**C# Tests:** `Opportunity/Managers/GlobalIndicesManagerTests.cs`

**Key Areas:**
- Periodically upload global indices
- Update all country records simultaneously
- Replace previous versions with current data
- Add new fields when indices are introduced
- Retire indices no longer relevant
- Maintain historical "as-at" views
- Use indices in business rules and risk assessment

---

## Test Categories

### By Priority

| Priority | Description | Test Count | Status |
|----------|-------------|------------|--------|
| **P0** | Critical - Core opportunity functionality | 150+ | ✅ Complete |
| **P1** | High - Important DST and decision features | 180+ | ✅ Complete |
| **P2** | Medium - Supporting features | 70+ | ✅ Complete |
| **P3** | Low - Nice to have enhancements | 25+ | ✅ Complete |

### By Type

| Type | Description | Test Count |
|------|-------------|------------|
| **Unit** | Isolated component tests | 250+ |
| **Integration** | Cross-component tests | 100+ |
| **Business Logic** | Workflow and rule tests | 75+ |

---

## Running Tests

### All Opportunity Tests

```powershell
# Run all opportunity tests
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --filter "FullyQualifiedName~Opportunity"

# Run with detailed output
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --filter "FullyQualifiedName~Opportunity" --logger "console;verbosity=detailed"

# Run with TRX output
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --filter "FullyQualifiedName~Opportunity" --logger "trx;LogFileName=OpportunityTests.trx" --results-directory "QA Tests/Test Execution Results"
```

### Specific Manager Tests

```powershell
# OpportunityManager tests
dotnet test --filter "FullyQualifiedName~OpportunityManagerTests"

# DSTManager tests
dotnet test --filter "FullyQualifiedName~DSTManagerTests"

# DecisionManager tests
dotnet test --filter "FullyQualifiedName~DecisionManagerTests"
```

### By Priority

```powershell
# Critical tests only
dotnet test --filter "Category=P0"

# High priority tests
dotnet test --filter "Category=P1"
```

---

## Test Data Strategy

### In-Memory Database
All tests use in-memory EF Core database provider for isolation and speed.

### Seed Data
Each test suite sets up required seed data:
- Countries with profiles
- Organizational units
- Sample partnerships
- User accounts with permissions
- Sample opportunities for testing

### Test Fixtures
Reusable test fixtures for common scenarios:
- `OpportunityTestFixture` - Standard opportunity setup
- `DSTTestFixture` - DST analysis scenarios
- `DecisionTestFixture` - Decision workflow setup

---

## Dependencies

### Required Entities
Tests assume the following entities exist:
- `Opportunity` - Core opportunity entity
- `OpportunityBudget` - Budget details
- `OpportunitySchedule` - Schedule/timeline
- `ResourcePlan` - Resource planning
- `DSTProfile` - DST analysis results
- `DSTRecommendation` - DST recommendations
- `OpportunityDecision` - Go/No-Go decisions
- `PartnershipAgreement` - Agreement library
- `GlobalIndex` - Global indices data
- `CountryProfile` - Country-specific data

### Required Managers
- `OpportunityManager`
- `DSTManager`
- `OpportunityBudgetManager`
- `OpportunityScheduleManager`
- `ResourcePlanManager`
- `RiskManager`
- `DecisionManager`
- `GlobalIndicesManager`
- `AgreementManager`

### Required Services
- `DSTAnalysisService`
- `DocumentExtractionService`
- `OpportunityService`
- `AgreementService`

---

## Test Execution Results

Test execution results are stored in `Test Execution Results/Opportunity/`:
- `OpportunityTests_YYYYMMDD_HHMMSS.trx` - Test run results
- `OpportunityTestCoverage.xml` - Code coverage reports
- `OpportunityTestSummary.md` - Human-readable summaries

---

## Related Documentation

- **PRD Source:** `tasks/opportunity-ux/Opportunity Epics.md`
- **CRM Tests:** `QA Tests/CRM Enhancement Tests/`
- **Partnership Tests:** `QA Tests/Business Manager Functional Test List/PartnerManager/`
- **Gap Analysis:** `QA Tests/REQUIREMENTS_GAP_ANALYSIS.md`
- **Phased Implementation Plan:** `QA Tests/Summary Reports/OPPORTUNITY_TESTS_PHASED_PLAN.md`

---

## Contributing

### Adding New Tests

1. **Documentation First:**
   - Add test cases to appropriate markdown file
   - Follow existing format and naming conventions
   - Include test ID, description, steps, expected results

2. **Implement C# Tests:**
   - Create test class in appropriate folder
   - Follow xUnit patterns from existing tests
   - Use descriptive test method names
   - Add appropriate `[Fact]` or `[Theory]` attributes
   - Include `[Trait("Category", "P0")]` for priority

3. **Update README:**
   - Update test counts in this file
   - Add new test categories if needed

---

## Notes

1. **Test ID Format:** `TC-OPP-[Component]-[Type]-[Number]`
   - TC-OPP-OM-F-001 = Opportunity Manager Functional Test #001
   - TC-OPP-DST-BL-001 = DST Business Logic Test #001

2. **Skipped Tests:** Some tests may be marked `[Skip]` due to:
   - Entities not yet implemented
   - External service dependencies
   - Feature under development

3. **Test Isolation:** All tests are designed to run independently in any order.

4. **Performance:** Full suite runs in ~3-5 minutes on standard hardware.

---

**Last Updated:** January 13, 2026  
**Test Coverage:** 425+ tests covering 200+ requirements  
**Status:** ✅ Production Ready
