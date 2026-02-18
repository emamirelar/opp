# UNOPS Opportunity+ System - Test Cases Index

## 📊 Executive Summary

This document provides a comprehensive index of all test cases for the UNOPS Opportunity+ CRM system, including documentation, executable C# tests, and frontend tests.

**Last Updated**: January 13, 2026

---

## 📈 Test Coverage Summary

| Category | Documentation | C# Tests | Status |
|----------|--------------|----------|--------|
| **Business Manager Functional Tests** | 16 files | 1,200+ tests | ✅ Complete |
| **Business Logic Tests** | 8 files | 400+ tests | ✅ Complete |
| **Controllers Tests** | 12 files | 400+ tests | ✅ Complete |
| **Services Tests** | 10 files | 250+ tests | ✅ Enhanced |
| **CRM Enhancement Tests** | 11 files | 200+ tests | ✅ Complete |
| **Edge Cases & Security Tests** | 6 files | 150+ tests | ✅ Complete |
| **Frontend Tests (Angular)** | 6 files | 100+ tests | ✅ Complete |
| **Integration Tests** | 3 files | 200+ tests | ✅ Complete |
| **Data Import Tests** | 3 files | 45+ tests | ✅ New |
| **Opportunity Tests** | 26 files | 565+ tests | ✅ Complete |
| **Total** | **101 files** | **~3,510+ tests** | ✅ Complete |

---

## 🆕 Recent Updates (January 13, 2026)

### Opportunity Feature Tests - Comprehensive Coverage

**Major Achievement:** Addressed critical gap identified in requirements analysis.

| Component | Files | Tests | Status |
|-----------|-------|-------|--------|
| Manager Documentation | 8 | 200+ | ✅ Complete |
| Business Logic Documentation | 6 | 150+ | ✅ Complete |
| Controller Documentation | 8 | 70+ | ✅ Complete |
| Service Documentation | 3 | 25+ | ✅ Complete |
| **Advanced Coverage** | 1 | 120+ | ✅ Complete |
| C# Manager Tests | 3 | 115+ | ✅ Complete |
| C# Advanced Tests | 1 | 25+ | ✅ Complete |
| **Total Opportunity Tests** | **30** | **705+** | ✅ Complete |

**Coverage Types:** Functional, Validation, Security, **Negative, Integration, Boundary, Edge Cases**

**Key Files:**
- `QA Tests/Opportunity Tests/README.md` - Complete overview
- `QA Tests/Opportunity Tests/ADVANCED_TEST_COVERAGE.md` - 120 advanced tests
- `QA Tests/Opportunity Tests/ADVANCED_COVERAGE_SUMMARY.md` - Comprehensive summary
- `C# Tests/.../Opportunity/Managers/OpportunityManagerTests.cs` - 30+ implemented tests
- `C# Tests/.../Opportunity/AdvancedTests/OpportunityAdvancedTests.cs` - 25+ advanced tests

**Features Covered:**
- Opportunity CRUD & Lifecycle
- Decision Support Tool (DST) with 9-parameter analysis
- Go/No-Go Decision Process
- Budget, Schedule, Resource Planning
- Document Upload & AI Extraction
- Partnership Agreement Library
- Risk Management
- Global Indices Integration

---

## 🆕 Previous Updates (January 7, 2026)

### New Test Files Added

| File | Location | Tests | Purpose |
|------|----------|-------|---------|
| `AuditDataFixTests.cs` | `DataImport/` | 10+ | Audit field fix for PR #479 |
| `PartnerErpDimValueFixTests.cs` | `DataImport/` | 15+ | ErpDimValue fix for PR #477 |
| `SequenceResyncTests.cs` | `DataImport/` | 13+ | Sequence resync for commit b1e1976c |
| `LiaisonOfficeServiceTests.cs` | `Services/` | 32+ | LiaisonOffice service coverage |

### Enhanced Test Files

| File | Location | Tests Added | Purpose |
|------|----------|-------------|---------|
| `CountryServiceTests.cs` | `Services/` | 14+ | Edge cases & DB integration |
| `OrganizationHierarchyLookupServiceTests.cs` | `Services/` | 20+ | Implemented placeholder tests |

See `Test Execution Results/TEST_COVERAGE_SUMMARY.md` for detailed coverage report.

---

## 📁 Directory Structure

```
QA Tests/
├── TEST_CASES_INDEX.md                          # This file
├── Business Manager Functional Test List/       # Manager documentation (16 folders)
├── Business Logic Tests/                        # Business rule test documentation (8 files)
├── Controllers Tests/                           # Controller test documentation (18 files)
├── Services Tests/                              # Service test documentation (10 files)
├── CRM Enhancement Tests/                       # CRM PRD-specific tests (11 files)
├── Edge Cases & Security Tests/                 # Security & edge case tests (6 files)
├── Frontend Tests/                              # Angular Jasmine tests
├── C# Tests/                                    # Executable C# xUnit tests
│   └── UNOPS.PAO.Business.Tests/
│       ├── Managers/                            # Manager unit tests
│       ├── BusinessLogic/                       # Business logic tests
│       └── Services/                            # Service unit tests
├── Integration Tests/                           # Controller integration tests
│   └── Controllers/
└── Test Execution Results/                      # Test run results & reports
```

---

## 🔧 Business Manager Functional Tests (~1,200 cases)

### Documentation Files

| Manager | File | Test Count |
|---------|------|------------|
| PartnerManager | `PartnerManager/PartnerManager_TestCases.md` | 100+ |
| ContactManager | `ContactManager/ContactManager_TestCases.md` | 120+ |
| InteractionManager | `InteractionManager/InteractionManager_TestCases.md` | 100+ |
| DocumentManager | `DocumentManager/DocumentManager_TestCases.md` | 100+ |
| WorkflowManager | `WorkflowManager/WorkflowManager_TestCases.md` | 80+ |
| NotificationManager | `NotificationManager/NotificationManager_TestCases.md` | 80+ |
| UserDataManager | `UserDataManager/UserDataManager_TestCases.md` | 80+ |
| ProfileManager | `ProfileManager/ProfileManager_TestCases.md` | 60+ |
| PartnerTreeManager | `PartnerTreeManager/PartnerTreeManager_TestCases.md` | 65+ |
| OrganizationHierarchyManager | `OrganizationHierarchyManager/OrganizationHierarchyManager_TestCases.md` | 80+ |
| LinkManager | `LinkManager/LinkManager_TestCases.md` | 60+ |
| DocumentTypeManager | `DocumentTypeManager/DocumentTypeManager_TestCases.md` | 40+ |
| ValuesManager | `ValuesManager/ValuesManager_TestCases.md` | 40+ |
| SystemAdminManager | `SystemAdminManager/SystemAdminManager_TestCases.md` | 60+ |
| GeminiManager | `GeminiManager/GeminiManager_TestCases.md` | 60+ |
| GmailAddonManager | `GmailAddonManager/GmailAddonManager_TestCases.md` | 40+ |

### C# Test Files

| File | Location | Test Count |
|------|----------|------------|
| PartnerManagerTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 100 |
| ContactManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 120 |
| InteractionManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 100 |
| DocumentManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 100 |
| WorkflowManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 80 |
| NotificationManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 80 |
| UserDataManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 80 |
| ProfileManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 60 |
| PartnerTreeManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 65 |
| OrganizationHierarchyManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 80 |
| LinkManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 100 |
| SystemAdminGeminiManagerFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Managers/` | 160 |

---

## 📋 Business Logic Tests (~400 cases)

### Documentation Files

| File | Description | Test Count |
|------|-------------|------------|
| PartnerManager_BusinessLogic_TestCases.md | Partner approval, ERP integration, org units | 80+ |
| ContactManager_BusinessLogic_TestCases.md | Contact relationships, deduplication | 60+ |
| InteractionManager_BusinessLogic_TestCases.md | AI integration, calendar sync | 50+ |
| DocumentManager_BusinessLogic_TestCases.md | Storage, text extraction, OCR | 50+ |
| OrganizationHierarchyManager_BusinessLogic_TestCases.md | Hierarchy management | 40+ |
| DataImportFixes_TestCases.md | Import validation and fixes | 25+ |
| PartnerErpDimValueFix_TestCases.md | ERP dim value conflict resolution | 20+ |

### C# Test File

| File | Location | Test Count |
|------|----------|------------|
| PartnerBusinessLogicTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/BusinessLogic/` | 400+ |

---

## 📦 Data Import Tests (~45 cases) - NEW

### C# Test Files

| File | Location | Test Count | Purpose |
|------|----------|------------|---------|
| AuditDataFixTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/DataImport/` | 10+ | Audit field corrections (PR #479) |
| PartnerErpDimValueFixTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/DataImport/` | 15+ | ErpDimValue range fixes (PR #477) |
| SequenceResyncTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/DataImport/` | 13+ | PostgreSQL sequence sync |

### Key Test Coverage

| Area | Tests | Description |
|------|-------|-------------|
| System User ID (-1) | 10 | Validates -1 as Opportunity+ System User |
| Legacy User Migration | 4 | Fixes CreatedBy/LastModifiedBy from 0 to -1 |
| ErpDimValue Range | 14 | Valid (1-7999), Reserved (8000-9999), Invalid (>9999) |
| Sequence Verification | 5 | Validates sequence > max ID |
| Concurrency | 3 | Thread-safe operations |

---

## 🌐 Controllers Tests (~400 cases)

### Documentation Files

Located in `Controllers Tests/`:

| Controller | Test Count |
|-----------|------------|
| PartnerController | 80+ |
| ContactController | 60+ |
| InteractionController | 50+ |
| DocumentController | 50+ |
| NotificationController | 40+ |
| WorkflowController | 40+ |
| UserController | 50+ |
| SearchController | 30+ |
| AIController | 40+ |
| ReportController | 30+ |
| AdminController | 40+ |
| OrganizationHierarchyController | 30+ |
| PartnerTreeController | 25+ |
| DashboardController | 20+ |
| PermissionController | 20+ |
| RoleController | 20+ |
| And 8 more... | 100+ |

### C# Test Files

| File | Location | Test Count |
|------|----------|------------|
| PartnerControllerFullTests.cs | `Integration Tests/Controllers/` | 80 |
| ContactControllerFullTests.cs | `Integration Tests/Controllers/` | 60 |
| InteractionControllerFullTests.cs | `Integration Tests/Controllers/` | 50 |
| DocumentControllerFullTests.cs | `Integration Tests/Controllers/` | 50 |
| AdditionalControllersTests.cs | `Integration Tests/Controllers/` | 200+ |

---

## 🎯 Opportunity Tests (~565 cases)

**Location:** `QA Tests/Opportunity Tests/`  
**Status:** ✅ Complete - All test types covered  
**Created:** January 13, 2026

### Overview

Comprehensive test coverage for all Opportunity management features including functional, validation, security, **negative, integration, boundary, and edge case tests**.

### Documentation Files

**Manager Tests** (8 files - 200+ tests):
- OpportunityManager_TestCases.md (50+ tests) - CRUD, lifecycle, AI, conversions
- DSTManager_TestCases.md (45+ tests) - DST profiling, 9 parameters, recommendations
- DecisionManager_TestCases.md (25+ tests) - Go/No-Go decisions, authorization
- OpportunityBudgetManager_TestCases.md (20+ tests) - Budget generation, fees
- OpportunityScheduleManager_TestCases.md (15+ tests) - Schedule, WBS, milestones
- ResourcePlanManager_TestCases.md (15+ tests) - Resource planning, personnel
- RiskManager_TestCases.md (15+ tests) - Risk identification, assessment
- GlobalIndicesManager_TestCases.md (15+ tests) - Global indices management

**Business Logic Tests** (6 files - 150+ tests):
- OpportunityWorkflow_TestCases.md (35+ tests) - State transitions, approvals
- DSTProfiler_TestCases.md (40+ tests) - Profiling algorithms, scoring
- DocumentExtraction_TestCases.md (30+ tests) - AI document extraction
- OpportunityStatement_TestCases.md (20+ tests) - Statement generation
- GoNoGoDecision_TestCases.md (25+ tests) - Decision workflows
- AgreementLibrary_TestCases.md (20+ tests) - Agreement management

**Controller Tests** (8 files - 70+ tests):
- OpportunityController_TestCases.md (12+ tests)
- DSTController_TestCases.md (10+ tests)
- DecisionController_TestCases.md (10+ tests)
- OpportunityBudgetController_TestCases.md (8+ tests)
- OpportunityScheduleController_TestCases.md (8+ tests)
- ResourcePlanController_TestCases.md (8+ tests)
- GlobalIndicesController_TestCases.md (7+ tests)
- PartnershipAgreementController_TestCases.md (7+ tests)

**Service Tests** (3 files - 25+ tests):
- OpportunityService_TestCases.md (10+ tests)
- DSTAnalysisService_TestCases.md (10+ tests)
- AgreementService_TestCases.md (5+ tests)

**Advanced Coverage** (1 file - 120+ tests):
- ADVANCED_TEST_COVERAGE.md
  - Negative Tests (45) - SQL injection, XSS, malicious input, error handling
  - Integration Tests (35) - E2E flows, cross-component, external services
  - Boundary Tests (25) - Min/max values, limits, thresholds
  - Edge Cases (15) - Special chars, multi-language, unusual scenarios

### C# Test Files

**Location:** `C# Tests/UNOPS.PAO.Business.Tests/Opportunity/`

**Manager Tests:**
- OpportunityManagerTests.cs (30+ tests) - ✅ Complete
- DSTManagerTests.cs (45+ tests) - ✅ Complete
- DecisionManagerTests.cs (40+ tests) - ✅ Complete

**Advanced Tests:**
- OpportunityAdvancedTests.cs (25+ tests) - ✅ Complete
  - Security: SQL injection, XSS prevention
  - Integration: E2E lifecycle, concurrent operations
  - Boundary: Max lengths, min/max values
  - Edge Cases: Special chars, multi-language, leap days

**Total Implemented:** 140+ executable tests

### Test Coverage By Type

| Type | Count | Purpose |
|------|-------|---------|
| **Functional** | 200+ | Core CRUD and operations |
| **Validation** | 85+ | Business rules and data integrity |
| **Security** | 50+ | Authorization and attack prevention |
| **Negative** | 45+ | Error handling and invalid inputs |
| **Integration** | 55+ | Cross-component and E2E flows |
| **Boundary** | 25+ | Limits and thresholds |
| **Edge Cases** | 15+ | Unusual but valid scenarios |
| **Performance** | 15+ | Load and concurrency |
| **Audit** | 10+ | Compliance and tracking |

### Running Opportunity Tests

```powershell
# All opportunity tests
dotnet test --filter "FullyQualifiedName~Opportunity"

# By category
dotnet test --filter "Type=Negative"
dotnet test --filter "Type=Integration"
dotnet test --filter "Type=Boundary"
dotnet test --filter "Type=EdgeCase"

# By priority
dotnet test --filter "Category=P0&FullyQualifiedName~Opportunity"
dotnet test --filter "Category=P1&FullyQualifiedName~Opportunity"
```

### Key Features Tested

✅ Opportunity Creation & Management  
✅ Decision Support Tool (9-parameter analysis)  
✅ Go/No-Go Decision Process  
✅ Budget, Schedule & Resource Planning  
✅ AI Document Extraction  
✅ Partnership Agreement Library  
✅ Risk Management  
✅ Global Indices Integration  
✅ Workflow & Approvals  
✅ Multi-User Collaboration  
✅ External System Integration  

---

## ⚙️ Services Tests (~200 cases)

### Documentation Files

Located in `Services Tests/`:

| Service | Test Count |
|---------|------------|
| GoogleCloudStorageService | 35+ |
| GoogleDriveDocumentManager | 25+ |
| GoogleTextToSpeechService | 18+ |
| TextExtractionService | 20+ |
| AiContextualService | 20+ |
| OrganizationHierarchyLookupService | 22+ |
| CountryService | 15+ |
| SavedFilterService | 20+ |
| AuthenticationService | 25+ |
| EmailService | 20+ |

### C# Test File

| File | Location | Test Count |
|------|----------|------------|
| AllServicesFullTests.cs | `C# Tests/UNOPS.PAO.Business.Tests/Services/` | 200+ |

---

## 🆕 CRM Enhancement Tests (~200 cases)

Based on the CRM Enhancement PRD requirements.

### Backend Tests

Located in `CRM Enhancement Tests/Backend/`:

| Manager | Test Count |
|---------|------------|
| EngagementManager | 40+ |
| PartnerLiaisonOfficeManager | 30+ |
| PartnerFocalPointManager | 30+ |
| GeoRegionManager | 30+ |
| ContinentManager | 25+ |

### Frontend Tests

Located in `CRM Enhancement Tests/Frontend/`:

| Component/Service | Test Count |
|-------------------|------------|
| BaseEntityViewComponent | 20+ |
| RelatedInfoPanelComponent | 20+ |
| PanelLayoutService | 15+ |
| EnhancedEntityLayoutComponent | 20+ |
| PartnerView_Enhanced | 25+ |
| ContactView_Enhanced | 25+ |

---

## 🛡️ Edge Cases & Security Tests (~150 cases)

Located in `Edge Cases & Security Tests/`:

| Category | Test Count |
|----------|------------|
| Security_Authorization | 40+ |
| Concurrency_RaceCondition | 25+ |
| DataIntegrity | 25+ |
| ErrorRecovery_Resilience | 25+ |
| BulkOperations | 20+ |
| AuditTrail | 20+ |

### C# Test Files

Located in `C# Tests/UNOPS.PAO.Business.Tests/EdgeCases/`:

| File | Test Count |
|------|------------|
| SecurityAuthorizationTests.cs | 40+ |
| ConcurrencyTests.cs | 25+ |
| DataIntegrityTests.cs | 25+ |
| ErrorRecoveryTests.cs | 25+ |
| BulkOperationsTests.cs | 20+ |
| AuditTrailTests.cs | 20+ |

---

## 🖥️ Frontend Tests (Angular/Jasmine) (~100 cases)

Located in `Frontend Tests/`:

| File | Component/Service | Test Count |
|------|-------------------|------------|
| base-entity-view.component.spec.ts | BaseEntityViewComponent | 20+ |
| related-info-panel.component.spec.ts | RelatedInfoPanelComponent | 20+ |
| enhanced-entity-layout.component.spec.ts | EnhancedEntityLayoutComponent | 15+ |
| partner-view-enhanced.component.spec.ts | PartnerViewComponent | 20+ |
| contact-view-enhanced.component.spec.ts | ContactViewComponent | 20+ |
| panel-layout.service.spec.ts | PanelLayoutService | 15+ |

### Running Frontend Tests

```bash
# Option 1: Use setup script
cd "QA Tests/Frontend Tests"
./setup-frontend-tests.ps1  # Windows
./setup-frontend-tests.sh   # Linux/Mac

# Option 2: Manual
cd UNOPS.PAO.ClientApp
npm test
```

---

## 🧪 Test Execution Results

Located in `Test Execution Results/`:

| File | Description |
|------|-------------|
| TEST_EXECUTION_REPORT.md | Latest comprehensive test execution report |
| BusinessTests_*.trx | Business layer test results |
| IntegrationTests_*.trx | Integration test results |
| FastTests_*.trx | Fast unit test results |
| SPECIFICATION_TESTS_REVIEW.md | Specification filtering issues analysis |
| REQUIREMENTS_GAP_ANALYSIS.md | PRD requirements gap analysis |

---

## 🚀 Running Tests

### C# Tests (Backend)

```powershell
# Run all business tests
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj"

# Run integration tests
dotnet test "QA Tests/Integration Tests/UNOPS.PAO.IntegrationTests.csproj"

# Run with TRX output
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory "QA Tests/Test Execution Results"

# Run specific test class
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Run specific test category
dotnet test --filter "Category=P0"
```

### Angular Tests (Frontend)

```bash
# Navigate to Angular project
cd UNOPS.PAO.ClientApp

# Run all tests
npm test

# Run with coverage
npm test -- --code-coverage

# Run specific file
npm test -- --include "**/partner*.spec.ts"
```

---

## 📊 Test Categories

### Priority Levels

| Priority | Description | Count |
|----------|-------------|-------|
| **P0** | Critical - Core business functionality | ~500 |
| **P1** | High - Important features | ~800 |
| **P2** | Medium - Secondary features | ~700 |
| **P3** | Low - Nice to have | ~300 |

### Test Types

| Type | Description | Count |
|------|-------------|-------|
| Unit | Isolated component tests | ~1,500 |
| Integration | Cross-component tests | ~600 |
| Edge Case | Boundary condition tests | ~200 |
| Security | Authorization/authentication | ~150 |
| Performance | Response time tests | ~150 |
| Concurrency | Race condition tests | ~100 |

---

## 📝 Notes

1. **Test ID Format**: `TC-[Component]-[Type]-[Number]`
   - TC-PM-F001 = Partner Manager Functional Test #001
   - TC-PM-BL-P0-001 = Partner Manager Business Logic P0 Test #001

2. **Status Legend**:
   - ✅ Complete - Tests written and passing
   - 🔄 In Progress - Tests being developed
   - ⏳ Pending - Tests planned but not started
   - ⚠️ Skipped - Tests temporarily disabled (see reason in test file)

3. **Skipped Tests**: Some tests are marked with `[Skip]` attribute due to:
   - Entities not yet implemented (CRM Enhancement features)
   - External service dependencies
   - Specification logic under review

4. **Test Data**: Test data is seeded using in-memory database providers for isolation.

---

## 🔗 Related Documentation

- [CRM Enhancement PRD](../docs/Development/crm-enhancement-implementation.md)
- [Angular Component Guidelines](.cursor/rules/angular-component-guidelines.mdc)
- [.NET Implementation Guidelines](.cursor/rules/dotnet-implementation-guidelines.mdc)
- [Test Execution Report](Test%20Execution%20Results/TEST_EXECUTION_REPORT.md)

---

*This index is automatically maintained. Last generated: December 19, 2025*
