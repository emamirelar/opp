# Test Implementation & Execution Guide

**Purpose**: Step-by-step guide for implementing and executing the test specifications  
**Target Audience**: Developers implementing the test suite  
**Framework**: xUnit + Moq + FluentAssertions

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Setup](#project-setup)
3. [Test Implementation Patterns](#test-implementation-patterns)
4. [Running Tests](#running-tests)
5. [Code Coverage](#code-coverage)
6. [Troubleshooting](#troubleshooting)
7. [Best Practices](#best-practices)

---

## Prerequisites

### Required Software

```bash
# Verify .NET SDK installed
dotnet --version  # Should be 9.0 or higher

# Verify Git
git --version

# Optional: Visual Studio 2022 or VS Code
```

### Knowledge Requirements

- C# programming
- xUnit framework basics
- Moq mocking framework
- Entity Framework Core
- Async/await patterns

---

## Project Setup

### Step 1: Create Test Projects

```bash
# Navigate to solution root
cd c:\Users\Leonardc\git\opportunityplus

# Create unit test projects
dotnet new xunit -n UNOPS.PAO.Domain.Tests -o tests/Unit/UNOPS.PAO.Domain.Tests
dotnet new xunit -n UNOPS.PAO.Business.Tests -o tests/Unit/UNOPS.PAO.Business.Tests
dotnet new xunit -n UNOPS.PAO.Presentation.Tests -o tests/Unit/UNOPS.PAO.Presentation.Tests

# Add to solution
dotnet sln add tests/Unit/UNOPS.PAO.Domain.Tests/UNOPS.PAO.Domain.Tests.csproj
dotnet sln add tests/Unit/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj
dotnet sln add tests/Unit/UNOPS.PAO.Presentation.Tests/UNOPS.PAO.Presentation.Tests.csproj
```

### Step 2: Install NuGet Packages

```bash
# Navigate to Business test project
cd tests/Unit/UNOPS.PAO.Business.Tests

# Install core testing packages
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoFixture
dotnet add package AutoFixture.Xunit2
dotnet add package AutoFixture.AutoMoq

# Install code coverage
dotnet add package coverlet.collector
dotnet add package coverlet.msbuild

# Install EF Core testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# Ensure Microsoft.NET.Test.Sdk is installed (usually auto-installed with xUnit)
dotnet add package Microsoft.NET.Test.Sdk
```

### Step 3: Add Project References

```bash
# From Business test project
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj
dotnet add reference ../../../UNOPS.PAO.DataAccess/UNOPS.PAO.DataAccess.csproj
dotnet add reference ../../../UNOPS.PAO.Models/UNOPS.PAO.Models.csproj
```

### Step 4: Configure Code Coverage

Edit `UNOPS.PAO.Business.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    
    <!-- Code Coverage Configuration -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
    <Threshold>75</Threshold>
    <ThresholdType>line,branch</ThresholdType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="AutoFixture" Version="4.18.1" />
    <PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
    <PackageReference Include="AutoFixture.AutoMoq" Version="4.18.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  </ItemGroup>
</Project>
```

### Step 5: Create Base Test Class

Create `tests/Unit/UNOPS.PAO.Business.Tests/TestBase.cs`:

```csharp
using AutoFixture;
using AutoFixture.AutoMoq;
using Xunit.Abstractions;

namespace UNOPS.PAO.Business.Tests;

public abstract class TestBase
{
    protected IFixture Fixture { get; }
    protected ITestOutputHelper Output { get; }

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
        
        // Configure AutoFixture to handle circular references
        Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
```

---

## Test Implementation Patterns

### Pattern 1: Basic Manager Test Structure

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace UNOPS.PAO.Business.Tests.Managers;

public class UserDataManagerTests : TestBase
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<User>> _mockUserSet;
    private readonly UserDataManager _sut; // System Under Test

    public UserDataManagerTests(ITestOutputHelper output) : base(output)
    {
        // Arrange: Set up mocks
        _mockDbContext = new Mock<AppDbContext>();
        _mockUserSet = new Mock<DbSet<User>>();
        
        _mockDbContext.Setup(x => x.Users).Returns(_mockUserSet.Object);
        
        // Create System Under Test
        _sut = new UserDataManager(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_Should_Return_User_When_Exists()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserSet.Setup(x => x.FindAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetUserByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var userId = 999;
        _mockUserSet.Setup(x => x.FindAsync(userId))
            .ReturnsAsync((User)null);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}
```

### Pattern 2: Testing with Multiple Mocks

```csharp
public class InteractionManagerTests : TestBase
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<Interaction>> _mockInteractionSet;
    private readonly Mock<DbSet<InteractionContact>> _mockInteractionContactSet;
    private readonly Mock<IMapper> _mockMapper;
    private readonly InteractionManager _sut;

    public InteractionManagerTests(ITestOutputHelper output) : base(output)
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockInteractionSet = new Mock<DbSet<Interaction>>();
        _mockInteractionContactSet = new Mock<DbSet<InteractionContact>>();
        _mockMapper = new Mock<IMapper>();
        
        _mockDbContext.Setup(x => x.Interactions).Returns(_mockInteractionSet.Object);
        _mockDbContext.Setup(x => x.InteractionContacts).Returns(_mockInteractionContactSet.Object);
        
        _sut = new InteractionManager(_mockDbContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateInteraction_Should_Create_Junction_Records()
    {
        // Arrange
        var request = new InteractionRequest
        {
            Title = "Meeting",
            ContactIds = new List<int> { 101, 102 }
        };

        var interaction = new Interaction { Id = 1 };
        _mockMapper.Setup(x => x.Map<Interaction>(request)).Returns(interaction);

        // Act
        await _sut.CreateInteractionAsync(request);

        // Assert
        _mockInteractionSet.Verify(x => x.AddAsync(It.IsAny<Interaction>(), default), Times.Once);
        _mockInteractionContactSet.Verify(x => x.AddAsync(It.IsAny<InteractionContact>(), default), Times.Exactly(2));
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
```

### Pattern 3: Theory Tests with InlineData

```csharp
[Theory]
[InlineData("test@example.com")]
[InlineData("TEST@EXAMPLE.COM")]
[InlineData("Test@Example.Com")]
public async Task GetUserByEmailAsync_Should_Be_Case_Insensitive(string email)
{
    // Arrange
    var users = new List<User>
    {
        new() { Id = 1, Email = "test@example.com" }
    }.AsQueryable();

    _mockUserSet.As<IQueryable<User>>()
        .Setup(m => m.Provider).Returns(users.Provider);
    _mockUserSet.As<IQueryable<User>>()
        .Setup(m => m.Expression).Returns(users.Expression);

    // Act
    var result = await _sut.GetUserByEmailAsync(email);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
}
```

### Pattern 4: Testing IQueryable with Specifications

```csharp
[Fact]
public async Task GetContactsByFilter_Should_Apply_Specifications()
{
    // Arrange
    var filter = new ContactFilterRequest
    {
        OrganizationId = 100,
        Status = EntityStatus.Active
    };

    var contacts = new List<Contact>
    {
        new() { Id = 1, OrganizationId = 100, Status = EntityStatus.Active },
        new() { Id = 2, OrganizationId = 200, Status = EntityStatus.Active },
        new() { Id = 3, OrganizationId = 100, Status = EntityStatus.Inactive }
    }.AsQueryable();

    _mockContactSet.As<IQueryable<Contact>>()
        .Setup(m => m.Provider).Returns(contacts.Provider);
    _mockContactSet.As<IQueryable<Contact>>()
        .Setup(m => m.Expression).Returns(contacts.Expression);

    // Act
    var result = await _sut.GetContactsAsync(filter);

    // Assert
    result.Should().HaveCount(1);
    result.First().Id.Should().Be(1);
}
```

### Pattern 5: Performance Testing

```csharp
[Fact]
public async Task GetUserByIdAsync_Should_Complete_Within_100ms()
{
    // Arrange
    var userId = 1;
    var user = new User { Id = userId };
    _mockUserSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

    var stopwatch = Stopwatch.StartNew();

    // Act
    await _sut.GetUserByIdAsync(userId);

    // Assert
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    Output.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds}ms");
}
```

### Pattern 6: Concurrency Testing

```csharp
[Fact]
public async Task GetUserByIdAsync_Should_Handle_Concurrent_Requests()
{
    // Arrange
    var userId = 1;
    var user = new User { Id = userId, Email = "test@example.com" };
    _mockUserSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

    // Act: Run 20 concurrent requests
    var tasks = Enumerable.Range(0, 20)
        .Select(_ => _sut.GetUserByIdAsync(userId))
        .ToArray();

    var results = await Task.WhenAll(tasks);

    // Assert
    results.Should().HaveCount(20);
    results.Should().OnlyContain(r => r.Id == userId);
    results.Should().OnlyContain(r => r.Email == "test@example.com");
}
```

---

## Running Tests

### Command Line Execution

```bash
# Run all tests in solution
dotnet test

# Run tests in specific project
dotnet test tests/Unit/UNOPS.PAO.Business.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserDataManagerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~UserDataManagerTests.GetUserByIdAsync_Should_Return_User_When_Exists"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run in parallel (faster)
dotnet test --parallel
```

### Watch Mode (Continuous Testing)

```bash
# Run tests automatically when code changes
dotnet watch test
```

### Visual Studio

1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" or select specific tests
3. View results in Test Explorer window

### VS Code

1. Install C# Extension
2. Install .NET Core Test Explorer extension
3. Tests appear in Test Explorer sidebar
4. Click play button to run tests

---

## Code Coverage

### Generate Coverage Reports

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Install report generator (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
start coveragereport/index.html  # Windows
open coveragereport/index.html   # macOS
xdg-open coveragereport/index.html  # Linux
```

### Coverage by Manager

Check coverage for specific managers:

```bash
# UserDataManager
dotnet test tests/Unit/UNOPS.PAO.Business.Tests --filter "FullyQualifiedName~UserDataManagerTests" /p:CollectCoverage=true

# WorkflowManager
dotnet test tests/Unit/UNOPS.PAO.Business.Tests --filter "FullyQualifiedName~WorkflowManagerTests" /p:CollectCoverage=true
```

### Coverage Thresholds

The test projects are configured with 75% minimum coverage threshold. Builds will fail if coverage drops below this.

To adjust threshold, edit `.csproj`:

```xml
<Threshold>75</Threshold>  <!-- Change to desired percentage -->
```

---

## Troubleshooting

### Issue: Tests Not Discovered

**Problem**: Tests don't appear in Test Explorer

**Solution**:
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test --list-tests
```

### Issue: Mock Setup Not Working

**Problem**: Mock returns null or default values

**Solution**:
```csharp
// Ensure mock setup returns correct type
_mockUserSet.Setup(x => x.FindAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => users.FirstOrDefault(u => u.Id == id));

// Not just:
_mockUserSet.Setup(x => x.FindAsync(It.IsAny<int>()))
    .ReturnsAsync(user); // May return same user for all IDs
```

### Issue: IQueryable Tests Failing

**Problem**: LINQ queries on mocked DbSet fail

**Solution**:
```csharp
// Set up all IQueryable members
var data = new List<User> { user1, user2 }.AsQueryable();

_mockUserSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(data.Provider);
_mockUserSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(data.Expression);
_mockUserSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(data.ElementType);
_mockUserSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
```

### Issue: Async Tests Hanging

**Problem**: Test never completes

**Solution**:
```csharp
// Don't mix sync and async
// BAD:
var result = _sut.GetUserByIdAsync(1).Result; // Deadlock risk

// GOOD:
var result = await _sut.GetUserByIdAsync(1);
```

### Issue: Flaky Concurrency Tests

**Problem**: Concurrency tests sometimes pass, sometimes fail

**Solution**:
```csharp
// Add delays to increase chance of race condition
await Task.Delay(10);

// Use SemaphoreSlim for controlled concurrency
var semaphore = new SemaphoreSlim(0, 20);
```

---

## Best Practices

### 1. Test Naming

```csharp
// Pattern: MethodName_Should_ExpectedBehavior_When_Condition

// Good
GetUserByIdAsync_Should_Return_User_When_Exists()
GetUserByIdAsync_Should_Return_Null_When_Not_Found()
CreateContact_Should_Throw_When_Email_Is_Invalid()

// Bad
TestGetUser()
Test1()
UserTest()
```

### 2. Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange: Set up test data and mocks
    var userId = 1;
    var user = new User { Id = userId };
    _mockUserSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

    // Act: Execute the method being tested
    var result = await _sut.GetUserByIdAsync(userId);

    // Assert: Verify the results
    result.Should().NotBeNull();
    result.Id.Should().Be(userId);
}
```

### 3. One Assert Concept Per Test

```csharp
// Good: Tests one concept
[Fact]
public void User_Should_Require_Email()
{
    var user = new User();
    user.Email.Should().BeNullOrEmpty();
}

// Bad: Tests multiple unrelated concepts
[Fact]
public void User_Tests()
{
    var user = new User();
    user.Email.Should().BeNullOrEmpty(); // Email validation
    user.Id.Should().Be(0); // ID default
    user.IsActive.Should().BeTrue(); // Active status
}
```

### 4. Use FluentAssertions

```csharp
// Good: Readable, descriptive
result.Should().NotBeNull();
result.Should().BeOfType<User>();
result.Email.Should().Be("test@example.com");
list.Should().HaveCount(5);
list.Should().Contain(x => x.Id == 1);

// Avoid: Traditional Assert
Assert.NotNull(result);
Assert.True(result is User);
Assert.Equal("test@example.com", result.Email);
```

### 5. Mock Verification

```csharp
// Verify method was called
_mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);

// Verify method was called with specific arguments
_mockUserSet.Verify(
    x => x.AddAsync(It.Is<User>(u => u.Email == "test@example.com"), default),
    Times.Once
);

// Verify method was never called
_mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
```

### 6. Test Data Builders

```csharp
// Create reusable test data builders
public static class TestDataBuilder
{
    public static User CreateUser(int id = 1, string email = "test@example.com")
    {
        return new User
        {
            Id = id,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            CreatedDate = DateTime.UtcNow
        };
    }

    public static List<User> CreateUsers(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateUser(i, $"user{i}@example.com"))
            .ToList();
    }
}

// Usage
var user = TestDataBuilder.CreateUser();
var users = TestDataBuilder.CreateUsers(10);
```

### 7. Cleanup Resources

```csharp
public class ResourceTest : IDisposable
{
    private readonly SomeResource _resource;

    public ResourceTest()
    {
        _resource = new SomeResource();
    }

    [Fact]
    public void Test_Something()
    {
        // Test using _resource
    }

    public void Dispose()
    {
        _resource?.Dispose();
    }
}
```

---

## Implementation Checklist

### For Each Manager

- [ ] Create test class file
- [ ] Set up constructor with mocks
- [ ] Implement functional tests (F001-F0XX)
- [ ] Implement performance tests (P001-P010)
- [ ] Implement concurrency tests (C001-C010)
- [ ] Implement edge cases (E001-E0XX)
- [ ] Run all tests, verify passing
- [ ] Check code coverage (≥ 75%)
- [ ] Review and refactor
- [ ] Commit changes

### Project-Wide

- [ ] All 5 managers have test implementations
- [ ] Overall code coverage ≥ 75%
- [ ] All tests passing
- [ ] CI/CD integration working
- [ ] Documentation updated

---

## Next Steps

1. **Start with UserDataManager** (highest priority, simplest)
2. **Move to WorkflowManager** (high priority, medium complexity)
3. **Implement InteractionManager** (high priority, highest complexity)
4. **Complete NotificationManager** (medium priority)
5. **Finish with DocumentManager** (medium priority)

---

**Guide Version**: 1.0  
**Last Updated**: November 11, 2025  
**Related Documents**:
- Test Specifications: `Test Cases/Business/`
- Execution Reports: `Test Execution Results/`
- Testing Guide: `docs/Development/BACKEND_TESTING_GUIDE.md`






