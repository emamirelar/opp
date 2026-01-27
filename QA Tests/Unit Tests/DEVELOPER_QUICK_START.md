# Unit Test Implementation - Developer Quick Start Guide

**Project**: UNOPS Opportunity+ System  
**Purpose**: Implement unit tests from specifications to prevent production defects  
**Estimated Effort**: 6-8 weeks for complete implementation

---

## 📋 What Has Been Created

### Test Specifications (Ready for Implementation)

| Manager | File | Test Cases | Status | Priority |
|---------|------|------------|--------|----------|
| **UNOPSPartnerManager** | `Business/PartnerManager/UNOPSPartnerManager_UnitTests.md` | 40+ | ✅ READY | CRITICAL |
| **UNOPSContactManager** | `Business/ContactManager/UNOPSContactManager_UnitTests.md` | 50+ | ✅ READY | CRITICAL |
| **UNOPSInteractionManager** | `Business/InteractionManager/UNOPSInteractionManager_UnitTests.md` | 35+ | ✅ READY | HIGH |
| **Other Managers** | Folders created, specs pending | 175+ | 📝 PENDING | MEDIUM |

**Total Tests Specified**: 125+ (with 175+ more planned)  
**Total Tests Planned**: 300+

---

## 🎯 Immediate Priority - Prevent Defect Recurrence

### PNO-686: Partner Code Generation
**Manager**: UNOPSPartnerManager  
**Specification**: `Business/PartnerManager/UNOPSPartnerManager_UnitTests.md`

**Critical Tests to Implement First**:
1. **TC-PM-002**: Skip reserved range (8000-9999) ⚠️ CRITICAL
2. **TC-PM-003**: Empty database scenario
3. **TC-PM-004-006**: Boundary values
4. **TC-PM-008**: Include deleted partners
5. **TC-PM-012**: ErpDimValue assignment on approval

**Estimated Effort**: 2 days  
**Must Complete By**: Week 1

---

### PNO-676: Duplicate Detection
**Manager**: UNOPSContactManager  
**Specification**: `Business/ContactManager/UNOPSContactManager_UnitTests.md`

**Critical Tests to Implement First**:
1. **TC-CM-002**: Exclude own ID in duplicate check ⚠️ CRITICAL
2. **TC-CM-006**: Detect duplicates after edit ⚠️ CRITICAL
3. **TC-CM-046-048**: Import workflow duplicate handling
4. **TC-CM-051-052**: Import dialog state management

**Estimated Effort**: 2 days  
**Must Complete By**: Week 2

---

### PNO-677: Advanced Search Fields
**Manager**: UNOPSContactManager  
**Specification**: `Business/ContactManager/UNOPSContactManager_UnitTests.md`

**Critical Tests to Implement First**:
1. **TC-CM-011**: Search FirstName equals ⚠️ CRITICAL
2. **TC-CM-012**: Search FirstName contains ⚠️ CRITICAL
3. **TC-CM-015**: Search Email equals vs contains ⚠️ CRITICAL
4. **TC-CM-014**: Search by related entity (Partner name)
5. **TC-CM-016-017**: Multiple field search (AND/OR)

**Estimated Effort**: 2 days  
**Must Complete By**: Week 2

---

## 🚀 Getting Started (Day 1)

### Step 1: Create Test Project

```bash
# Navigate to tests directory
cd tests/Unit

# Create xUnit test project
dotnet new xunit -n UNOPS.PAO.Business.Tests

# Add to solution
dotnet sln add UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj

# Add project references
cd UNOPS.PAO.Business.Tests
dotnet add reference ../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj
dotnet add reference ../../UNOPS.PAO.Models/UNOPS.PAO.Models.csproj
```

---

### Step 2: Install Required Packages

```bash
cd UNOPS.PAO.Business.Tests

# Core testing packages
dotnet add package xunit --version 2.6.6
dotnet add package xunit.runner.visualstudio --version 2.5.6
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0

# Mocking and assertions
dotnet add package Moq --version 4.20.70
dotnet add package FluentAssertions --version 6.12.0

# AutoFixture for test data
dotnet add package AutoFixture --version 4.18.1
dotnet add package AutoFixture.Xunit2 --version 4.18.1
dotnet add package AutoFixture.AutoMoq --version 4.18.1

# Code coverage
dotnet add package coverlet.collector --version 6.0.0
dotnet add package coverlet.msbuild --version 6.0.0

# Entity Framework in-memory database for testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
```

---

### Step 3: Configure Code Coverage

**Edit**: `UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj`

Add the following `<PropertyGroup>`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    
    <!-- Code Coverage Configuration -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
    <Threshold>75</Threshold>
    <ThresholdType>line,branch,method</ThresholdType>
    <ExcludeByFile>**/Migrations/*.cs</ExcludeByFile>
  </PropertyGroup>

  <!-- Rest of the file -->
</Project>
```

---

### Step 4: Create Test Base Classes

**Create**: `UNOPS.PAO.Business.Tests/TestBase/ManagerTestBase.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using UNOPS.PAO.Data;

namespace UNOPS.PAO.Business.Tests.TestBase;

public abstract class ManagerTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected Mock<IMapper> MockMapper { get; private set; }
    protected IMapper Mapper => MockMapper.Object;

    protected ManagerTestBase()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        MockMapper = new Mock<IMapper>();
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Save changes to in-memory database
    /// </summary>
    protected async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Clear all entities from database
    /// </summary>
    protected void ClearDatabase()
    {
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }
}
```

---

### Step 5: Create Test Data Factories

**Create**: `UNOPS.PAO.Business.Tests/TestData/PartnerTestDataFactory.cs`

```csharp
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Business.Tests.TestData;

public class PartnerTestDataFactory
{
    private int _sequenceNumber = 1;

    public Partner CreatePartner(Action<Partner>? customize = null)
    {
        var partner = new Partner
        {
            Id = _sequenceNumber++,
            Name = $"Test Partner {_sequenceNumber}",
            PartnerShortDescription = $"Short description {_sequenceNumber}",
            Status = EntityStatus.Active,
            PartnerApprovalStatus = PartnerApprovalStatus.NotApproved,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            IsDeleted = false
        };

        customize?.Invoke(partner);
        return partner;
    }

    public Partner CreateApprovedPartner(int? erpDimValue = null)
    {
        return CreatePartner(p =>
        {
            p.PartnerApprovalStatus = PartnerApprovalStatus.Approved;
            p.PartnerApprovalDate = DateTime.UtcNow;
            p.ErpDimValue = erpDimValue ?? (1000 + _sequenceNumber);
            p.CanCreateNewOpportunities = true;
        });
    }

    public List<Partner> CreatePartnersWithErpDimValues(params int[] erpDimValues)
    {
        return erpDimValues.Select(value => CreateApprovedPartner(value)).ToList();
    }

    public Partner CreatePartnerInReservedRange()
    {
        return CreateApprovedPartner(erpDimValue: 8500); // In 8000-9999 range
    }
}
```

---

### Step 6: Create Your First Test

**Create**: `UNOPS.PAO.Business.Tests/Managers/PartnerManagerTests.cs`

```csharp
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Business.Tests.TestData;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class PartnerManagerTests : ManagerTestBase
{
    private readonly UNOPSPartnerManager _manager;
    private readonly PartnerTestDataFactory _factory;

    public PartnerManagerTests()
    {
        _manager = new UNOPSPartnerManager(Context, Mapper, /* other dependencies */);
        _factory = new PartnerTestDataFactory();
    }

    /// <summary>
    /// TC-PM-002: Reserved range exclusion (8000-9999)
    /// CRITICAL - Prevents PNO-686 recurrence
    /// </summary>
    [Fact]
    public async Task GetNextErpDimValue_Should_Skip_ReservedRange_When_ValuesInRange8000To9999Exist()
    {
        // Arrange
        var partners = new List<Partner>
        {
            _factory.CreateApprovedPartner(1961),  // Valid value
            _factory.CreatePartnerInReservedRange(), // 8500 - should be ignored
            _factory.CreateApprovedPartner(10000)  // Above reserved - should be ignored
        };
        
        await Context.Partners.AddRangeAsync(partners);
        await SaveChangesAsync();

        // Act
        var result = await _manager.GetNextErpDimValueAsync();

        // Assert
        result.Should().Be(1962, "should use 1961 + 1, ignoring reserved range values");
    }

    /// <summary>
    /// TC-PM-003: Empty database scenario
    /// </summary>
    [Fact]
    public async Task GetNextErpDimValue_Should_ReturnOne_When_NoPartnersExist()
    {
        // Arrange
        // Empty database - no partners

        // Act
        var result = await _manager.GetNextErpDimValueAsync();

        // Assert
        result.Should().Be(1, "should return 1 when database is empty");
    }

    // Add more tests following the specification...
}
```

---

### Step 7: Run Your Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~GetNextErpDimValue_Should_Skip_ReservedRange"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

---

### Step 8: Generate Code Coverage Report

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Install report generator globally (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**\coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report (Windows)
start coveragereport\index.html

# Open report (macOS)
open coveragereport/index.html
```

---

## 📖 How to Read Test Specifications

Each test specification includes:

### 1. Test ID
**Format**: `TC-[Manager Initials]-[Number]`  
**Example**: `TC-PM-002` = Test Case - Partner Manager - 002

### 2. Test Name
**Pattern**: `Method_Should_ExpectedBehavior_When_Condition`  
**Example**: `GetNextErpDimValue_Should_Skip_ReservedRange_When_ValuesInRange8000To9999Exist`

### 3. Test Code
**Sections**:
- **Arrange**: Setup code (create test data)
- **Act**: Invoke the method being tested
- **Assert**: Verify expected outcomes

### 4. Priority
- **CRITICAL**: Must implement first (prevents known defects)
- **HIGH**: Core functionality
- **MEDIUM**: Important but not urgent

---

## 📊 Implementation Roadmap

### Week 1: Critical Tests (PNO-686 Prevention)
**Manager**: PartnerManager  
**Tests**: TC-PM-001 through TC-PM-010 (10 tests)  
**Effort**: 2-3 days  
**Goal**: Prevent ErpDimValue generation bugs

**Tasks**:
- [ ] Create test project
- [ ] Install packages
- [ ] Create test base classes
- [ ] Implement PartnerTestDataFactory
- [ ] Implement 10 ErpDimValue tests
- [ ] Achieve 90%+ coverage for GetNextErpDimValueAsync

---

### Week 2: Critical Tests (PNO-676, PNO-677 Prevention)
**Manager**: ContactManager  
**Tests**: TC-CM-001 through TC-CM-018 (18 tests)  
**Effort**: 3-4 days  
**Goal**: Prevent duplicate detection and search bugs

**Tasks**:
- [ ] Create ContactTestDataFactory
- [ ] Implement 10 duplicate detection tests (TC-CM-001 to TC-CM-010)
- [ ] Implement 8 advanced search tests (TC-CM-011 to TC-CM-018)
- [ ] Achieve 85%+ coverage for duplicate detection
- [ ] Achieve 95%+ coverage for advanced search

---

### Week 3: High Priority Tests
**Manager**: PartnerManager (remaining), ContactManager (remaining)  
**Tests**: 60+ additional tests  
**Effort**: 5 days  
**Goal**: Complete critical manager coverage

**Tasks**:
- [ ] Complete all PartnerManager tests (30 remaining)
- [ ] Complete all ContactManager tests (32 remaining)
- [ ] Achieve 90%+ overall coverage for both managers

---

### Week 4: High Priority Tests
**Manager**: InteractionManager  
**Tests**: TC-IM-001 through TC-IM-035 (35 tests)  
**Effort**: 4-5 days  
**Goal**: Core business logic coverage

**Tasks**:
- [ ] Create InteractionTestDataFactory
- [ ] Implement CRUD tests (7 tests)
- [ ] Implement interaction type tests (4 tests)
- [ ] Implement relationship tests (6 tests)
- [ ] Implement all remaining tests (18 tests)
- [ ] Achieve 85%+ coverage for InteractionManager

---

### Weeks 5-6: Medium Priority Tests
**Managers**: Remaining 10 managers  
**Tests**: 175+ tests  
**Effort**: 10 days  
**Goal**: Comprehensive coverage

**Tasks**:
- [ ] Create specifications for remaining managers
- [ ] Implement test factories for each manager
- [ ] Implement all tests
- [ ] Achieve 75%+ overall coverage
- [ ] All quality gates passing

---

## ✅ Daily Checklist

### Before Starting
- [ ] Read the test specification for your assigned manager
- [ ] Understand the defect being prevented (if applicable)
- [ ] Review the manager code you're testing

### While Coding
- [ ] Follow AAA pattern (Arrange, Act, Assert)
- [ ] Use FluentAssertions for readable assertions
- [ ] Use test data factories for consistent data
- [ ] Keep tests independent (no shared state)
- [ ] Write descriptive test names

### Before Committing
- [ ] All tests pass locally
- [ ] Code coverage meets threshold (75%+)
- [ ] No flaky tests
- [ ] Test names match specification
- [ ] Code follows team standards

---

## 🔧 Common Patterns

### Pattern 1: Testing Async Methods

```csharp
[Fact]
public async Task MethodName_Should_ExpectedBehavior_When_Condition()
{
    // Arrange
    var entity = new Entity { /* ... */ };
    await Context.Entities.AddAsync(entity);
    await SaveChangesAsync();

    // Act
    var result = await _manager.MethodAsync();

    // Assert
    result.Should().NotBeNull();
}
```

### Pattern 2: Testing Exceptions

```csharp
[Fact]
public async Task Method_Should_ThrowException_When_InvalidInput()
{
    // Arrange
    var invalidRequest = new Request { /* invalid data */ };

    // Act
    Func<Task> act = async () => await _manager.MethodAsync(invalidRequest);

    // Assert
    await act.Should().ThrowAsync<BusinessException>()
        .WithMessage("*expected*message*");
}
```

### Pattern 3: Testing with Theory (Multiple Inputs)

```csharp
[Theory]
[InlineData("", "Empty string")]
[InlineData(null, "Null value")]
[InlineData("   ", "Whitespace only")]
public async Task Method_Should_Validate_When_InvalidInputProvided(string input, string because)
{
    // Arrange
    var request = new Request { Name = input };

    // Act
    Func<Task> act = async () => await _manager.CreateAsync(request);

    // Assert
    await act.Should().ThrowAsync<BusinessException>(because);
}
```

---

## 📚 Resources

### Documentation
- [xUnit Documentation](https://xunit.net/docs)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/introduction)

### Project Files
- [Backend Testing Guide](../../docs/Development/BACKEND_TESTING_GUIDE.md)
- [Test Specifications](./Business/)
- [Implementation Action Plan](../../Test Execution Results/Recommendations/IMPLEMENTATION_ACTION_PLAN.md)

---

## 🆘 Getting Help

### Common Issues

**Issue**: `DbContext` not found in tests
**Solution**: Add reference to `UNOPS.PAO.Data` project

**Issue**: Tests fail with "collection was modified"
**Solution**: Use `.ToList()` before modifying collections in tests

**Issue**: Coverage not generating
**Solution**: Ensure coverlet packages are installed and `.csproj` is configured

**Issue**: Flaky tests (pass sometimes, fail sometimes)
**Solution**: Check for shared state between tests, use `.AsNoTracking()` for read operations

---

## ✨ Best Practices

1. **Test One Thing**: Each test should verify one specific behavior
2. **Use Descriptive Names**: Test names should explain what's being tested
3. **Keep Tests Fast**: Tests should run in milliseconds
4. **Avoid Logic in Tests**: No if/else, loops, or complex logic
5. **Use Test Data Factories**: Consistent, reusable test data
6. **Clean Up**: Dispose of resources properly
7. **Test Edge Cases**: Null, empty, boundary values
8. **Test Error Paths**: Don't just test happy path

---

## 🎯 Success Criteria

### For Each Manager
- ✅ All specified tests implemented
- ✅ Coverage meets target (75%+ minimum)
- ✅ All tests passing
- ✅ No flaky tests
- ✅ Tests run in < 5 seconds per manager

### For Overall Project
- ✅ 300+ tests implemented
- ✅ 75%+ overall code coverage
- ✅ All critical defects prevented (PNO-686, PNO-676, PNO-677)
- ✅ CI/CD integration complete
- ✅ Team trained on testing practices

---

**Ready to Start?**

1. Read this guide
2. Set up your test project (Steps 1-5)
3. Implement your first test (Step 6)
4. Run and verify (Steps 7-8)
5. Continue with remaining tests from specifications

**Questions?** Refer to the test specifications in the `Business/` folder or consult the team lead.

---

**Last Updated**: January 2025  
**Status**: Phase 1 Ready for Implementation  
**Next Review**: After Week 2 completion

