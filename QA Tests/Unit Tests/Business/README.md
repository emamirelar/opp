# Business Layer - Unit Test Suite

**Project**: UNOPS Opportunity+ System  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Overview

This folder contains comprehensive unit test case specifications for all Business layer managers. These test cases are designed to:

1. **Prevent Defect Recurrence** - Especially PNO-686, PNO-680, PNO-677, PNO-676
2. **Ensure Business Logic Correctness** - Validate all business rules
3. **Achieve 75%+ Code Coverage** - Meet quality standards
4. **Enable Test-Driven Development** - Specifications ready for implementation

---

## Test Suite Structure

```
Business/
├── PartnerManager/
│   └── UNOPSPartnerManager_UnitTests.md (40+ tests) ✅ CREATED
├── ContactManager/
│   └── UNOPSContactManager_UnitTests.md (35+ tests)
├── InteractionManager/
│   └── UNOPSInteractionManager_UnitTests.md (30+ tests)
├── DocumentManager/
│   └── UNOPSDocumentManager_UnitTests.md (25+ tests)
├── NotificationManager/
│   └── NotificationManager_UnitTests.md (20+ tests)
├── WorkflowManager/
│   └── WorkflowManager_UnitTests.md (15+ tests)
├── UserDataManager/
│   └── UserDataManager_UnitTests.md (15+ tests)
├── GeminiManager/
│   └── UNOPSGeminiManager_UnitTests.md (30+ tests)
├── SystemAdminManager/
│   └── UNOPSSystemAdminManager_UnitTests.md (20+ tests)
├── EntityConfigurationManager/
│   └── UNOPSEntityConfigurationManager_UnitTests.md (15+ tests)
├── PartnerTreeManager/
│   └── UNOPSPartnerTreeManager_UnitTests.md (15+ tests)
├── GoogleDriveDocumentManager/
│   └── GoogleDriveDocumentManager_UnitTests.md (20+ tests)
└── AiContextualService/
    └── AiContextualService_UnitTests.md (25+ tests)
```

**Total Test Cases**: 300+

---

## Test Coverage Goals

| Manager | Test Cases | Priority | Target Coverage | Defect Prevention |
|---------|------------|----------|-----------------|-------------------|
| **PartnerManager** | 40+ | CRITICAL | 90%+ | PNO-686 |
| **ContactManager** | 35+ | HIGH | 85%+ | PNO-677, PNO-676 |
| **InteractionManager** | 30+ | HIGH | 85%+ | - |
| **DocumentManager** | 25+ | MEDIUM | 80%+ | - |
| **GeminiManager** | 30+ | HIGH | 80%+ | - |
| **NotificationManager** | 20+ | MEDIUM | 75%+ | - |
| **WorkflowManager** | 15+ | MEDIUM | 75%+ | - |
| **Others** | 100+ | VARIED | 70%+ | - |

---

## Priority Implementation Order

### Phase 1: Critical (Week 1-2)
**Goal**: Prevent known defects from recurring

1. ✅ **PartnerManager** - 40+ tests
   - Focus: ErpDimValue generation (PNO-686)
   - Focus: Approval workflow
   - Target: 90%+ coverage

2. **ContactManager** - 35+ tests
   - Focus: Duplicate detection (PNO-676)
   - Focus: CRUD operations
   - Target: 85%+ coverage

3. **InteractionManager** - 30+ tests
   - Focus: Business logic validation
   - Focus: Partner/Contact relationships
   - Target: 85%+ coverage

**Effort**: 10-12 developer days

### Phase 2: High Priority (Week 3-4)
**Goal**: Cover core business functionality

4. **GeminiManager** - 30+ tests
   - Focus: AI integration
   - Focus: Error handling

5. **DocumentManager** - 25+ tests
   - Focus: File operations
   - Focus: Validation

6. **GoogleDriveDocumentManager** - 20+ tests
   - Focus: External service integration (PNO-680 related)
   - Focus: Error handling

**Effort**: 8-10 developer days

### Phase 3: Medium Priority (Week 5-6)
**Goal**: Complete coverage for remaining managers

7. **SystemAdminManager** - 20+ tests
8. **NotificationManager** - 20+ tests
9. **WorkflowManager** - 15+ tests
10. **UserDataManager** - 15+ tests
11. **EntityConfigurationManager** - 15+ tests
12. **PartnerTreeManager** - 15+ tests
13. **AiContextualService** - 25+ tests

**Effort**: 10-12 developer days

---

## Test Case Template

Each manager test specification includes:

### 1. Test Suite Overview
- Manager name and file location
- Test framework and tools
- Total test count
- Coverage goals

### 2. Test Categories
- CRUD Operations
- Business Logic Validation
- Error Handling
- Permission/RBAC
- Edge Cases
- Concurrent Access (if applicable)
- Performance (if applicable)

### 3. Individual Test Cases
For each test:
- **Test ID**: Unique identifier (e.g., TC-PM-001)
- **Test Name**: Descriptive name following pattern `Method_Should_ExpectedBehavior_When_Condition`
- **Arrange**: Setup code with test data
- **Act**: Method invocation
- **Assert**: Expected outcomes with FluentAssertions

### 4. Test Data Factories
- Reusable test data generators
- Consistent test object creation
- Edge case data providers

### 5. Test Helpers
- Mock object creators
- Common assertion helpers
- Setup/teardown utilities

### 6. Execution Instructions
- How to run tests
- Filtering options
- Coverage reporting

---

## Testing Standards

### Naming Conventions

**Test Class**:
```csharp
public class UNOPSPartnerManagerTests
{
    // Test methods
}
```

**Test Methods**:
```csharp
[Fact]
public async Task MethodName_Should_ExpectedBehavior_When_Condition()
{
    // Arrange
    // Act
    // Assert
}

[Theory]
[InlineData(...)]
public async Task MethodName_Should_ExpectedBehavior_For_VariousInputs(params)
{
    // Arrange
    // Act  
    // Assert
}
```

### Required Packages

```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.6.6" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="AutoFixture" Version="4.18.1" />
  <PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
  <PackageReference Include="AutoFixture.AutoMoq" Version="4.18.1" />
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
  <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
</ItemGroup>
```

### Coverage Configuration

```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./coverage/</CoverletOutput>
  <Threshold>75</Threshold>
  <ThresholdType>line,branch,method</ThresholdType>
</PropertyGroup>
```

---

## Test Execution

### Run All Business Tests
```bash
dotnet test UNOPS.PAO.Business.Tests
```

### Run Specific Manager Tests
```bash
# Partner Manager tests
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Contact Manager tests
dotnet test --filter "FullyQualifiedName~ContactManagerTests"

# Critical tests only (PNO-686 prevention)
dotnet test --filter "FullyQualifiedName~GetNextErpDimValue"
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
start coveragereport/index.html
```

### Watch Mode (Continuous Testing)
```bash
dotnet watch test
```

---

## Success Metrics

### Coverage Targets

| Metric | Current | Week 2 | Week 4 | Week 6 |
|--------|---------|--------|--------|--------|
| **Overall Coverage** | 0% | 30% | 60% | 75%+ |
| **Critical Managers** | 0% | 80% | 90% | 90%+ |
| **Test Count** | 0 | 100+ | 200+ | 300+ |
| **Failed Tests** | N/A | 0 | 0 | 0 |
| **Build Time** | N/A | <30s | <1m | <2m |

### Quality Gates

- ✅ All tests must pass before merge
- ✅ Coverage must not decrease
- ✅ New code must have 75%+ coverage
- ✅ Critical paths must have 90%+ coverage
- ✅ No flaky tests allowed

---

## Implementation Checklist

### Setup (Week 1)
- [ ] Create `UNOPS.PAO.Business.Tests` project
- [ ] Install required NuGet packages
- [ ] Configure code coverage
- [ ] Create test base classes
- [ ] Set up test data factories
- [ ] Configure CI/CD integration

### Phase 1 Implementation (Week 1-2)
- [x] PartnerManager tests (40+ tests)
- [ ] ContactManager tests (35+ tests)
- [ ] InteractionManager tests (30+ tests)

### Phase 2 Implementation (Week 3-4)
- [ ] GeminiManager tests (30+ tests)
- [ ] DocumentManager tests (25+ tests)
- [ ] GoogleDriveDocumentManager tests (20+ tests)

### Phase 3 Implementation (Week 5-6)
- [ ] SystemAdminManager tests (20+ tests)
- [ ] NotificationManager tests (20+ tests)
- [ ] WorkflowManager tests (15+ tests)
- [ ] UserDataManager tests (15+ tests)
- [ ] EntityConfigurationManager tests (15+ tests)
- [ ] PartnerTreeManager tests (15+ tests)
- [ ] AiContextualService tests (25+ tests)

### Completion
- [ ] All 300+ tests implemented
- [ ] 75%+ overall coverage achieved
- [ ] All tests passing in CI/CD
- [ ] Test documentation complete
- [ ] Team training completed

---

## Common Test Patterns

### 1. CRUD Operation Pattern

```csharp
[Fact]
public async Task CreateEntity_Should_ReturnCreated_When_ValidDataProvided()
{
    // Arrange
    var request = new CreateRequest { Name = "Test Entity" };
    
    // Act
    var result = await manager.CreateEntityAsync(user, request);
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Test Entity");
    mockRepository.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);
}
```

### 2. Validation Pattern

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public async Task CreateEntity_Should_ThrowException_When_NameIsInvalid(string name)
{
    // Arrange
    var request = new CreateRequest { Name = name };
    
    // Act
    Func<Task> act = async () => await manager.CreateEntityAsync(user, request);
    
    // Assert
    await act.Should().ThrowAsync<BusinessException>()
        .WithMessage("*name*required*");
}
```

### 3. Permission Pattern

```csharp
[Fact]
public async Task DeleteEntity_Should_ThrowUnauthorized_When_UserLacksPermission()
{
    // Arrange
    var entity = new Entity { Id = 1 };
    var userWithoutPermission = CreateUserWithoutDeletePermission();
    
    // Act
    Func<Task> act = async () => await manager.DeleteEntityAsync(userWithoutPermission, 1);
    
    // Assert
    await act.Should().ThrowAsync<UnauthorizedAccessException>();
}
```

### 4. Business Rule Pattern

```csharp
[Fact]
public async Task ApproveEntity_Should_SetApprovalFields_When_ValidConditionsMet()
{
    // Arrange
    var entity = new Entity { Status = EntityStatus.Active };
    var admin = CreateAdminUser();
    
    // Act
    var result = await manager.ApproveEntityAsync(admin, entity.Id);
    
    // Assert
    result.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
    result.ApprovalDate.Should().NotBeNull();
    result.ApprovedBy.Should().Contain(admin.Identity.Name);
}
```

---

## Resources

### Documentation
- [xUnit Documentation](https://xunit.net/docs)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [AutoFixture Cheat Sheet](https://github.com/AutoFixture/AutoFixture/wiki/Cheat-Sheet)

### Related Documents
- [Backend Testing Guide](../../../docs/Development/BACKEND_TESTING_GUIDE.md)
- [Implementation Action Plan](../../../Test Execution Results/Recommendations/IMPLEMENTATION_ACTION_PLAN.md)
- [Defect Analysis](../../../Test Execution Results/Recommendations/DEFECT_ANALYSIS_AND_PREVENTION_RECOMMENDATIONS.md)

### Internal Links
- [Integration Test Cases](../../Business/)
- [Test Execution Reports](../../../Test Execution Results/)

---

## Contact & Support

**Questions About Test Cases**:
- Technical Lead
- QA Lead

**Questions About Implementation**:
- Development Manager
- Senior Developers

**Questions About Coverage**:
- QA Team

---

**Last Updated**: January 2025  
**Status**: In Progress - PartnerManager tests specified  
**Next Update**: After Phase 1 completion

