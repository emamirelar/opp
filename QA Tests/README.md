# UNOPS Opportunity+ QA Tests

Comprehensive test documentation and executable test code for the UNOPS Opportunity+ Partnership and Opportunity Management System.

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

# Run Frontend tests (Angular/Jasmine)
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

### 1. Business Manager Functional Tests (~1,200 tests)
Comprehensive CRUD and functional tests for all business managers:
- Partner, Contact, Interaction, Document management
- User, Permission, Role management
- Workflow, Notification, AI features

### 2. Business Logic Tests (~400 tests)
Complex business scenario testing:
- Multi-step workflows
- Cross-entity relationships
- Business rule validation
- Edge cases

### 3. Controller Tests (~400 tests)
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

### 5. CRM Enhancement Tests (~400 tests)
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

### 7. Frontend Tests (~200 tests)
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

Tests are automatically run on:
- Pull request creation
- Merge to development branch
- Nightly builds

## Coverage Goals

| Category | Target | Current |
|----------|--------|---------|
| Business Managers | 80% | TBD |
| Controllers | 75% | TBD |
| Services | 70% | TBD |
| Overall | 75% | TBD |

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
**Last Updated**: December 19, 2025
