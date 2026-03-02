# UNOPS Opportunity+ QA Tests

Comprehensive test documentation and executable test code for the UNOPS Opportunity+ Partnership and Opportunity Management System.

## 🎉 Latest Update - January 31, 2026

**Status**: ✅ **PLAYWRIGHT E2E MIGRATION COMPLETE** - 11 test files migrated  
**Passing Tests**: 48/181 passing (List views: 100% success rate)  
**Blocked Tests**: 133/181 blocked by DEF-001 (production defect in route guard)  
**Authentication**: Cookie-based real backend integration  

### **Playwright Test Suite Achievement**:
1. ✅ **11 Test Files Migrated** - All list views, detail pages, navigation/dashboard
2. ✅ **Real Backend Integration** - Tests run against actual PostgreSQL database and .NET API
3. ✅ **Cookie-Based Authentication** - Fast, reliable authentication without login forms
4. ✅ **Permission System** - Complete EntityPermissions configuration for all entities
5. ✅ **Test Data Infrastructure** - Automated seed scripts for test records
6. ✅ **CI/CD Ready** - GitHub Actions workflow for automated testing
7. ✅ **181 E2E Tests** - Ready to execute once DEF-001 is fixed

### **Current Status**:
- ✅ **List Views (48 tests):** ALL PASSING - Contacts (13), Partners (11), Interactions (13), Opportunities (11)
- ⚠️ **Detail Pages (85 tests):** Migrated, blocked by DEF-001 route permission guard bug
- ⚠️ **Nav/Dashboard (48 tests):** Migrated, partially blocked by DEF-001
- 🎯 **After DEF-001 fix:** Projected 140-160/181 passing (77-88% pass rate)

### **Previous Update - January 16, 2026**:
**Status**: ✅ **CODE IMPROVEMENTS DEPLOYED**  
**Pass Rate**: 89.2% → ~98.4% (+9.2% improvement in C# tests)

**Code Improvements Implemented**:
1. ✅ Test Environment Detection
2. ✅ Database Compatibility  
3. ✅ API Backward Compatibility
4. ✅ International Support
5. ✅ Improved Test Isolation

**Documentation**:
- 📄 `Playwright Tests/ALL_TESTS_PASSING_SUMMARY.md` - Complete Playwright setup and results ⭐ NEW
- 📄 `Playwright Tests/LOCAL_TESTING_SUCCESS_GUIDE.md` - Local environment setup guide ⭐ NEW
- 📄 `Playwright Tests/TEST_SUITE_RESULTS.md` - Detailed test analysis ⭐ NEW
- 📄 `Test Execution Results/Recommendations/CODE_FIXES_IMPLEMENTED_2026-01-16.md` - C# test improvements
- 📄 `Test Execution Results/Recommendations/EXECUTIVE_SUMMARY_2026-01-16.md` - Executive summary

---

## Overview

This folder contains all QA test artifacts including:
- **Test Case Documentation** (Markdown files defining test scenarios)
- **Executable C# Tests** (xUnit test code)
- **Integration Tests** (API-level testing)
- **Test Execution Results** (Reports and analysis)
- **Summary Reports** (Session, delivery, implementation summaries)
- **Opportunity Tests Phased Plan** (`Summary Reports/OPPORTUNITY_TESTS_PHASED_PLAN.md`)

## Quick Start

### Running Tests

#### **Playwright E2E Tests** ⭐ RECOMMENDED

**Prerequisites:**
1. PostgreSQL 16 running on `localhost:5432`
2. Test database `TestDb` created
3. .NET backend running at `http://localhost:5159`
4. Angular dev server running at `http://127.0.0.1:4200`

**One-Time Setup:**
```powershell
# Create test user with Administrator role
$env:PGPASSWORD='test'; & 'C:\Program Files\PostgreSQL\16\bin\psql.exe' `
  -h localhost -p 5432 -U test -d TestDb -f "QA Tests/Scripts/setup-test-user.sql"

# Add Opportunity entity permissions
$env:PGPASSWORD='test'; & 'C:\Program Files\PostgreSQL\16\bin\psql.exe' `
  -h localhost -p 5432 -U test -d TestDb -f "QA Tests/Scripts/setup-opportunity-permissions.sql"
```

**Run Tests:**
```powershell
# Start backend (Terminal 1)
cd UNOPS.PAO.Server
dotnet run

# Run all partnership tests (Terminal 2)
cd "QA Tests"
npx playwright test contacts.spec.ts partners.spec.ts interactions.spec.ts opportunities.spec.ts `
  --project=chromium --workers=1

# Run specific suite
npx playwright test contacts.spec.ts --project=chromium

# Run with UI
npx playwright test contacts.spec.ts --headed

# Debug mode
npx playwright test contacts.spec.ts --debug
```

**Expected Results:**
- ✅ 48/48 tests passing
- ⏱️ ~8-14 minutes execution time
- 🎯 100% success rate

**Test Coverage:**
- Contacts: 13 tests (Create, Read, Update, Delete, Search, Export, Import)
- Partners: 11 tests (CRUD, Search, Navigation, Responsive design)
- Interactions: 13 tests (CRUD, Opportunity creation, Search, Mobile)
- Opportunities: 11 tests (CRUD, Search, Workflow, Detail navigation)

**Documentation:**
- 📄 `Playwright Tests/ALL_TESTS_PASSING_SUMMARY.md` - Complete setup and results
- 📄 `Playwright Tests/LOCAL_TESTING_SUCCESS_GUIDE.md` - Detailed configuration
- 📄 `Playwright Tests/helpers/auth.helper.ts` - Authentication patterns
- 📄 `Scripts/setup-test-user.sql` - Test user creation script
- 📄 `Scripts/setup-opportunity-permissions.sql` - Permission configuration

---

#### **C# Unit Tests**

```bash
# Navigate to C# tests
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
dotnet test

# Run integration tests
cd "QA Tests/Integration Tests"
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

#### **Angular/Jasmine Frontend Tests**

```bash
# Run Frontend tests
# First copy spec files to component folders, then:
cd UNOPS.PAO.ClientApp
ng test

# Run with coverage
ng test --code-coverage
```

### Viewing Test Documentation

All test case documentation is in Markdown format:
- `TEST_CASES_INDEX.md` - Complete index of all tests
- Individual folders contain domain-specific test cases

## Folder Structure

```
QA Tests/
├── README.md                              # This file
├── TEST_CASES_INDEX.md                    # Master index of all tests
│
├── Scripts/                               # Test setup SQL and PowerShell scripts
│   ├── setup-test-user.sql                # Playwright test user setup
│   ├── setup-opportunity-permissions.sql  # Entity permissions setup
│   ├── assign-test-roles.sql              # Role assignment script
│   ├── create-test-users.sql              # Test user creation
│   └── setup-database.ps1                 # Database setup scripts
│
├── Playwright Tests/                      # E2E tests (48 passing) ⭐ NEW
│   ├── ALL_TESTS_PASSING_SUMMARY.md       # Complete success documentation
│   ├── LOCAL_TESTING_SUCCESS_GUIDE.md     # Setup guide
│   ├── TEST_SUITE_RESULTS.md              # Detailed test analysis
│   ├── contacts.spec.ts                   # Contact tests (13 passing)
│   ├── partners.spec.ts                   # Partner tests (11 passing)
│   ├── interactions.spec.ts               # Interaction tests (13 passing)
│   ├── opportunities.spec.ts              # Opportunity tests (11 passing)
│   ├── helpers/
│   │   ├── auth.helper.ts                 # Authentication utilities
│   │   ├── api-mocks.helper.ts            # API mock setup
│   │   └── wait.helper.ts                 # Wait strategies
│   └── pages/
│       ├── base.page.ts                   # Base page object
│       ├── contacts.page.ts               # Contact page objects
│       ├── partners.page.ts               # Partner page objects
│       └── ... (other page objects)
│
├── Business Manager Functional Test List/ # Manager-level test cases
│   ├── AllManagers_Summary.md
│   ├── PartnerManager/
│   ├── ContactManager/
│   ├── InteractionManager/
│   ├── DocumentManager/
│   └── ... (13 managers total)
│
├── Business Logic Tests/                  # Complex business scenarios
│   ├── PartnerManager_BusinessLogic_TestCases.md
│   ├── ContactManager_BusinessLogic_TestCases.md
│   └── ... (8 test files)
│
├── Controllers Tests/                     # API controller tests
│   ├── README.md
│   ├── DashboardController_TestCases.md
│   └── ... (18 controllers)
│
├── Services Tests/                        # Service layer tests
│   ├── README.md
│   ├── GoogleCloudStorageService_TestCases.md
│   └── ... (10 services)
│
├── CRM Enhancement Tests/                 # PRD-based enhancement tests
│   ├── README.md
│   ├── Backend/                           # 5 manager tests
│   └── Frontend/                          # 6 component tests
│
├── Edge Cases & Security Tests/           # Non-functional tests
│   ├── README.md
│   ├── Security_Authorization_TestCases.md
│   ├── Concurrency_RaceCondition_TestCases.md
│   ├── DataIntegrity_TestCases.md
│   ├── ErrorRecovery_Resilience_TestCases.md
│   ├── BulkOperations_TestCases.md
│   └── AuditTrail_TestCases.md
│
├── Frontend Tests/                        # Angular Jasmine tests
│   ├── README.md
│   ├── components/                        # Component spec files
│   └── services/                          # Service spec files
│
├── C# Tests/                              # Executable unit tests
│   └── UNOPS.PAO.Business.Tests/
│       ├── Managers/                      # Manager tests
│       ├── Services/                      # Service tests
│       ├── EdgeCases/                     # Edge case & security tests
│       ├── TestBase/                      # Test infrastructure
│       └── Helpers/                       # Test utilities
│
├── Integration Tests/                     # Integration test project
│   └── UNOPS.PAO.IntegrationTests/
│       ├── Controllers/                   # Controller tests
│       └── Infrastructure/                # Test infrastructure
│
├── Summary Reports/                       # Session and delivery summaries
│   ├── DELIVERY_SUMMARY.md
│   ├── QUICK_TEST_SUMMARY.md
│   └── *_SUMMARY_*.md
│
└── Test Execution Results/                # Test run outputs
    ├── REQUIREMENTS_GAP_ANALYSIS.md
    ├── SPECIFICATION_TESTS_REVIEW.md
    └── test_execution_*.md
```

## Test Categories

### 1. Playwright E2E Tests (48 tests) ⭐ NEW
**Status**: ✅ **100% PASSING** - All 48 tests passing with real backend integration

End-to-end testing with real PostgreSQL database and .NET API:
- **Contacts** (13 tests) - CRUD, search, export, import, business card scanner
- **Partners** (11 tests) - CRUD, search, navigation, responsive design
- **Interactions** (13 tests) - CRUD, opportunity creation, search, mobile layout
- **Opportunities** (11 tests) - CRUD, search, workflow, detail navigation

**Key Features:**
- ✅ Real backend integration (not mocked)
- ✅ Cookie-based authentication
- ✅ Complete permission system setup
- ✅ Page object pattern
- ✅ Reusable authentication helper
- ✅ CI/CD ready with GitHub Actions

**Documentation:**
- `Playwright Tests/ALL_TESTS_PASSING_SUMMARY.md` - Complete guide
- `Playwright Tests/LOCAL_TESTING_SUCCESS_GUIDE.md` - Setup instructions
- `.github/workflows/playwright-tests.yml` - CI/CD configuration

---

### 2. Business Manager Functional Tests (~1,200 tests)
Comprehensive CRUD and functional tests for all business managers:
- Partner, Contact, Interaction, Document management
- User, Permission, Role management
- Workflow, Notification, AI features

### 3. Business Logic Tests (~400 tests)
Complex business scenario testing:
- Multi-step workflows
- Cross-entity relationships
- Business rule validation
- Edge cases

### 4. Controller Tests (~400 tests)
API-level testing:
- HTTP endpoint verification
- Request/response validation
- Authorization checks
- Error handling

### 4. Service Tests (~200 tests)
Service layer testing:
- External integrations (Google Cloud, AI)
- Internal services
- Cache operations

### 6. CRM Enhancement Tests (~400 tests)
PRD-based feature tests:
- New entities (Engagement, GeoRegion, etc.)
- Enhanced views (Partner, Contact)
- New components

### 6. Edge Cases & Security (~205 tests)
Non-functional testing:
- Security & authorization
- Concurrency & race conditions
- Data integrity
- Error recovery
- Audit trails

### 8. Frontend Tests (~200 tests)
Angular component/service tests (Jasmine/Karma):
- BaseEntityViewComponent
- RelatedInfoPanelComponent
- EnhancedEntityLayoutComponent
- PartnerViewEnhanced
- ContactViewEnhanced
- PanelLayoutService

## Test Documentation Format

Each test case document follows this format:

```markdown
# [Manager/Controller/Service] Test Cases

**Component**: Path to component
**Priority**: P0/P1/P2
**Total Test Cases**: N

## Overview
Brief description of what's being tested.

## Test Categories
| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 10 | P0 |
| Business Logic | 8 | P1 |
| Validation | 6 | P0 |

## P0 - Critical Tests

### TC-XXX-001: Test name
**Description**: What the test verifies
**Test Steps**:
1. Step one
2. Step two
**Expected Result**: Expected outcome
```

## Priority Levels

| Priority | Description | Must Pass for Release |
|----------|-------------|----------------------|
| P0 | Critical - Security, data integrity | ✅ Yes |
| P1 | High - Core functionality | ✅ Yes (recommended) |
| P2 | Medium - Enhanced features | ⚠️ Nice to have |

## Test Status Legend

| Status | Description |
|--------|-------------|
| ✅ Active | Tests are implemented and running |
| ⏳ Scaffolded | Test structure exists, awaiting entity |
| 🔴 Skipped | Temporarily skipped due to known issues |
| 📝 Documented | Test cases documented, not yet implemented |

## CI/CD Integration

### **GitHub Actions Workflows** ⭐ NEW

**Playwright E2E Tests** (`.github/workflows/playwright-tests.yml`):
- **Trigger**: Push to main/dev/QA-Tests, Pull Requests, Manual dispatch
- **Jobs**:
  1. **Main Tests** - Contacts, Partners, Interactions, Opportunities (48 tests)
  2. **Detail Page Tests** - Partner, Contact, Interaction, Opportunity detail pages
  3. **Navigation Tests** - Home, Dashboard, Navigation tabs
- **Database**: PostgreSQL 16 service container
- **Backend**: .NET 9.0 with health checks
- **Frontend**: Node.js 20 with Angular dev server
- **Artifacts**: Test reports, screenshots, videos (30-day retention)

**Features:**
- ✅ Parallel job execution (faster CI runs)
- ✅ Automatic database setup (migrations + test data)
- ✅ Health check verification before tests
- ✅ Comprehensive test reporting
- ✅ Artifact upload on failure

**C# Unit Tests**:
- Pull request creation
- Merge to development branch
- Nightly builds

## Code Coverage

### Coverage Tracking ✅ NEW

Code coverage is collected during CI/CD runs and displayed in the **GitHub Actions job summary**.

**Coverage is collected for:**
- ✅ FastTests (78 tests)
- ✅ Business Tests (2,135 tests)
- 📋 Integration Tests (when enabled)

**Coverage reports are available as downloadable artifacts** after each workflow run.

### Coverage Goals

| Category | Target | Current |
|----------|--------|---------|
| Business Managers | 75% | TBD |
| Controllers | 70% | TBD |
| Domain Models | 80% | TBD |
| Overall | 75% | TBD |

### Running Coverage Locally

#### C# Tests with Coverage

```powershell
# Navigate to test project
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report (one-time tool install)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report from coverage data
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open the HTML report
start coveragereport\index.html
```

#### Angular Tests with Coverage

```powershell
# Navigate to Angular app
cd UNOPS.PAO.ClientApp

# Run tests with coverage
ng test --code-coverage --watch=false

# Coverage report is generated at: coverage/UNOPS.PAO.ClientApp/index.html
start coverage\UNOPS.PAO.ClientApp\index.html
```

### CI/CD Coverage Integration

Coverage is automatically collected on:
- Push to `main`, `dev-deploy`, `QA-Tests` branches
- Pull requests to `main`, `dev-deploy`
- Daily scheduled runs (2 AM UTC)

**How to view coverage:**
1. Go to the **Actions** tab in GitHub
2. Click on a workflow run
3. View the **Summary** tab for coverage metrics
4. Download the **coverage-report** artifact for detailed HTML reports

## Contributing

1. Create test documentation first
2. Follow existing patterns and naming conventions
3. Use appropriate priority levels
4. Link to related PRDs/issues
5. Update TEST_CASES_INDEX.md

## Related Documentation

- `docs/Development/crm-enhancement-implementation.md` - CRM Enhancement PRD
- `UNOPS.PAO.Business/` - Business layer source code
- `UNOPS.PAO.Presentation/` - Controller source code

---

**Maintained by**: UNOPS Opportunity+ Development Team  
**Last Updated**: January 31, 2026
