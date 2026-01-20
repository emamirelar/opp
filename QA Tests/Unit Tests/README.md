# Unit Test Cases - UNOPS Opportunity+ System

**Purpose**: Comprehensive unit test case specifications for all layers  
**Framework**: xUnit + Moq + FluentAssertions  
**Target Coverage**: 75%+ overall, 90%+ for critical components

---

## Overview

This folder contains detailed unit test case specifications organized by architectural layer. These specifications are designed to be implemented by developers to create actual xUnit test code.

---

## Folder Structure

```
Unit Tests/
├── Business/                  ← Business logic managers (300+ test cases)
│   ├── README.md
│   ├── PartnerManager/
│   │   └── UNOPSPartnerManager_UnitTests.md (✅ 40+ tests)
│   ├── ContactManager/
│   ├── InteractionManager/
│   ├── DocumentManager/
│   ├── GeminiManager/
│   ├── NotificationManager/
│   ├── WorkflowManager/
│   ├── UserDataManager/
│   ├── SystemAdminManager/
│   ├── EntityConfigurationManager/
│   ├── PartnerTreeManager/
│   ├── GoogleDriveDocumentManager/
│   └── AiContextualService/
│
├── Domain/                    ← Entity and domain logic tests (planned)
│   ├── Entities/
│   └── Specifications/
│
└── Presentation/              ← Controller tests (planned)
    └── Controllers/
```

---

## Test Case Summary

| Layer | Managers/Components | Total Test Cases | Status | Priority |
|-------|---------------------|------------------|--------|----------|
| **Business** | 13 managers | 300+ | In Progress | HIGH |
| **Domain** | Entities, Specs | TBD | Planned | MEDIUM |
| **Presentation** | Controllers | TBD | Planned | MEDIUM |

---

## Current Status

### ✅ Completed (Week 1)
- [x] Business/PartnerManager - 40+ test cases
- [x] Business/README.md - Test suite overview
- [x] Test case template established
- [x] Test data factory pattern defined

### 🔄 In Progress (Week 1-2)
- [ ] Business/ContactManager - 35+ test cases
- [ ] Business/InteractionManager - 30+ test cases
- [ ] Business/GeminiManager - 30+ test cases

### 📋 Planned (Week 3-6)
- [ ] Remaining Business managers (200+ test cases)
- [ ] Domain entity tests
- [ ] Controller tests

---

## Purpose & Benefits

### 1. Defect Prevention
These test specifications directly address the defects identified in production:

| Defect | Related Tests | Prevention |
|--------|---------------|------------|
| **PNO-686** | PartnerManager ErpDimValue tests (TC-PM-001 to TC-PM-010) | 10 tests ensure sequence generation never fails |
| **PNO-680** | GoogleDriveDocumentManager tests | External service integration tested |
| **PNO-677** | ContactManager search tests | All searchable fields validated |
| **PNO-676** | ContactManager duplicate detection tests | State management tested |

### 2. Code Quality
- **90%+ coverage** for critical business logic
- **Regression prevention** - Tests catch bugs before production
- **Documentation** - Tests describe expected behavior
- **Confidence** - Developers can refactor safely

### 3. Development Velocity
- **Faster debugging** - Tests pinpoint exact failures
- **Better onboarding** - New developers learn from tests
- **Reduced manual testing** - Automated validation
- **Continuous delivery** - Tests enable frequent deployments

---

## How to Use These Specifications

### For Developers

1. **Read the specification** for your assigned manager
2. **Create the test project** if it doesn't exist
3. **Implement tests in order** - Critical first, then high priority
4. **Use the provided patterns** - Test data factories, helpers
5. **Run tests frequently** - Verify as you code
6. **Achieve coverage goals** - 75%+ minimum

### For Test Implementation

Each specification provides:
- ✅ **Test ID** - Unique identifier (e.g., TC-PM-001)
- ✅ **Test Name** - Descriptive, following AAA pattern
- ✅ **Arrange** - Complete setup code
- ✅ **Act** - Method to invoke
- ✅ **Assert** - Expected outcomes with FluentAssertions

**Example**:
```csharp
// From specification TC-PM-002
[Fact]
public async Task GetNextErpDimValue_Should_Skip_ReservedRange_When_ValuesInRange8000To9999Exist()
{
    // Arrange
    var partners = new List<Partner>
    {
        new() { Id = 1, ErpDimValue = 1961 },
        new() { Id = 2, ErpDimValue = 8500 }, // Reserved - ignored
        new() { Id = 3, ErpDimValue = 10000 }  // Above reserved - ignored
    };
    await context.Partners.AddRangeAsync(partners);
    
    // Act
    var result = await manager.GetNextErpDimValueAsync();
    
    // Assert
    result.Should().Be(1962); // Uses 1961 + 1
}
```

### For QA/Code Review

- ✅ Verify all critical paths tested
- ✅ Check edge cases covered
- ✅ Validate test naming conventions
- ✅ Ensure coverage meets targets
- ✅ Review assertion quality

---

## Test Coverage Goals

### Overall Targets

| Metric | Current | Week 2 | Week 4 | Week 6 | Final Goal |
|--------|---------|--------|--------|--------|------------|
| **Business Layer** | 0% | 30% | 60% | 80% | **85%+** |
| **Domain Layer** | 0% | 10% | 30% | 60% | **90%+** |
| **Presentation** | 0% | 5% | 20% | 50% | **70%+** |
| **Overall** | 0% | 20% | 40% | 65% | **75%+** |

### Component-Specific Targets

| Component | Target | Reason |
|-----------|--------|--------|
| **PartnerManager** | 90%+ | Critical - ErpDimValue (PNO-686) |
| **ContactManager** | 85%+ | High - Duplicate detection (PNO-676) |
| **InteractionManager** | 85%+ | High - Core business logic |
| **GeminiManager** | 80%+ | High - AI integration |
| **DocumentManager** | 80%+ | High - File operations |
| **Others** | 75%+ | Standard - Business logic |

---

## Implementation Phases

### Phase 1: Critical Managers (Week 1-2)
**Goal**: Prevent known defects

**Managers**:
1. ✅ PartnerManager (40+ tests) - COMPLETED
2. ContactManager (35+ tests) - IN PROGRESS
3. InteractionManager (30+ tests)

**Deliverables**:
- Test specifications written
- Test projects created
- Critical tests implemented
- 30% coverage achieved

**Effort**: 10-12 developer days

---

### Phase 2: High Priority (Week 3-4)
**Goal**: Core business logic coverage

**Managers**:
4. GeminiManager (30+ tests)
5. DocumentManager (25+ tests)
6. GoogleDriveDocumentManager (20+ tests)

**Deliverables**:
- 100+ additional tests
- 60% coverage achieved
- Integration with CI/CD

**Effort**: 8-10 developer days

---

### Phase 3: Remaining Managers (Week 5-6)
**Goal**: Comprehensive coverage

**Managers**:
7-13. All remaining Business managers (100+ tests)

**Deliverables**:
- 300+ total tests
- 75%+ coverage achieved
- All quality gates passing

**Effort**: 10-12 developer days

---

## Test Execution

### Quick Commands

```bash
# Run all unit tests
dotnet test UNOPS.PAO.Business.Tests

# Run all unit tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific manager tests
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Run critical tests only
dotnet test --filter "Priority=Critical"

# Watch mode (continuous testing)
dotnet watch test
```

### Coverage Reporting

```bash
# Generate coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
start coveragereport/index.html  # Windows
open coveragereport/index.html   # macOS
```

---

## Quality Standards

### Test Requirements

Every test must:
- ✅ Follow AAA pattern (Arrange, Act, Assert)
- ✅ Have descriptive name (`Method_Should_Behavior_When_Condition`)
- ✅ Test one thing only
- ✅ Be independent (no test depends on another)
- ✅ Be deterministic (same result every time)
- ✅ Run fast (< 1 second per test)
- ✅ Use FluentAssertions for readability

### Code Review Checklist

- [ ] All tests pass locally
- [ ] Coverage meets minimum threshold
- [ ] Test names are descriptive
- [ ] Edge cases are covered
- [ ] Error conditions are tested
- [ ] No flaky tests
- [ ] Test data is clean and minimal
- [ ] Mocks are properly configured

---

## Test Case Template

Each manager specification includes:

### 1. Overview
- Manager name and file path
- Test framework
- Total test count
- Coverage goal

### 2. Test Categories
- CRUD operations
- Business logic
- Validation
- Error handling
- Permissions/RBAC
- Edge cases

### 3. Individual Tests
- Test ID
- Test name
- Arrange/Act/Assert code
- Priority level

### 4. Test Helpers
- Test data factories
- Mock creators
- Common assertions

### 5. Execution Guide
- Run commands
- Filtering options
- Coverage reporting

---

## Success Metrics

### Code Coverage
- Overall: **75%+**
- Critical managers: **90%+**
- Business layer: **85%+**

### Test Quality
- All tests passing: **100%**
- Test execution time: **< 5 minutes**
- Flaky tests: **0**

### Development Impact
- Bugs caught before production: **80%+**
- Time to detect defects: **< 1 hour**
- Developer confidence: **High**

---

## Related Documentation

### Implementation Guides
- [Backend Testing Guide](../../docs/Development/BACKEND_TESTING_GUIDE.md)
- [Implementation Action Plan](../../Test Execution Results/Recommendations/IMPLEMENTATION_ACTION_PLAN.md)

### Defect Analysis
- [Defect Prevention Recommendations](../../Test Execution Results/Recommendations/DEFECT_ANALYSIS_AND_PREVENTION_RECOMMENDATIONS.md)
- [Implementation Status Report](../../Test Execution Results/Recommendations/IMPLEMENTATION_STATUS_REPORT.md)

### Test Cases (Integration)
- [Business Layer Integration Tests](../Business/)
- [Test Cases Index](../TEST_CASES_INDEX.md)

---

## Getting Started

### For Developers

1. **Review** [Business/README.md](./Business/README.md) for overview
2. **Read** specifications for your assigned manager
3. **Create** test project if needed
4. **Implement** tests in priority order
5. **Verify** coverage meets goals

### For Managers

1. **Review** implementation status reports
2. **Assign** developers to test implementation
3. **Track** progress against Phase timelines
4. **Monitor** coverage metrics
5. **Ensure** quality gates are met

---

## Contact

**Questions About Specifications**:
- QA Lead
- Technical Architect

**Questions About Implementation**:
- Development Manager
- Senior Developers

**Questions About Coverage**:
- QA Team

---

**Created**: January 2025  
**Last Updated**: January 2025  
**Status**: Phase 1 In Progress  
**Next Review**: After Phase 1 completion

