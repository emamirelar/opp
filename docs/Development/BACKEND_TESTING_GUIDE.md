# .NET Backend Testing Guide

**Project**: UNOPS Opportunity Plus Backend  
**Date**: January 15, 2025  
**Technology**: .NET 9.0, xUnit, Moq, FluentAssertions  
**Purpose**: Comprehensive testing strategy for the .NET backend

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [The Testing Pyramid](#the-testing-pyramid)
3. [Framework #1: xUnit + Moq (Unit Testing)](#framework-1-xunit--moq-unit-testing)
4. [Framework #2: Integration Tests](#framework-2-integration-tests)
5. [Why You Need Both](#why-you-need-both)
6. [Test Distribution Strategy](#test-distribution-strategy)
7. [Current Configuration](#current-configuration)
8. [Setup Requirements](#setup-requirements)
9. [Testing Standards by Layer](#testing-standards-by-layer)
10. [Code Coverage](#code-coverage)
11. [CI/CD Integration](#cicd-integration)
12. [Real-World Examples](#real-world-examples)
13. [Best Practices](#best-practices)
14. [Quick Reference](#quick-reference)

---

## Overview

Your .NET backend needs **multiple types of testing** because they serve completely different purposes:

1. **xUnit + Moq**: Unit testing (tests individual pieces of code in isolation)
2. **Integration Tests**: Tests API endpoints with real database and dependencies
3. **E2E Tests**: Tests complete workflows (optional, can be done from frontend)

**Think of it like building safety:**
- **Unit tests** = Check each component works correctly
- **Integration tests** = Check components work together
- **E2E tests** = Check the entire system functions

---

## The Testing Pyramid

```
┌─────────────────────────────────────────────────┐
│              Testing Pyramid                     │
├─────────────────────────────────────────────────┤
│                                                  │
│              E2E Tests (Few)                     │
│           ▲  Frontend or API Tests               │
│          ╱ ╲  ~10% of tests                     │
│         ╱   ╲   Integration Tests (Some)        │
│        ╱     ╲  WebApplicationFactory           │
│       ╱       ╲ ~20% of tests                   │
│      ╱         ╲  Unit Tests (Many)             │
│     ╱           ╲ xUnit + Moq                   │
│    ╱_____________╲ ~70% of tests                │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Key Principle**: More tests at the bottom (fast, cheap) and fewer at the top (slow, expensive)

---

## Framework #1: xUnit + Moq (Unit Testing)

### What It Does

Tests **individual units** of code in isolation (classes, methods, business logic)

### Components

#### xUnit
- **Role**: Test framework
- **Function**: Organizes and runs tests
- **Attributes**: `[Fact]`, `[Theory]`, `[InlineData]`

#### Moq
- **Role**: Mocking framework
- **Function**: Creates fake dependencies
- **Usage**: Mock services, repositories, database contexts

#### FluentAssertions
- **Role**: Assertion library
- **Function**: Provides readable, chainable assertions
- **Example**: `result.Should().BeOfType<OkResult>()`

### How It Works

```
Developer writes test → xUnit discovers test → 
Test runs with mocked dependencies → Assertions verify behavior →
Results displayed → Coverage calculated
```

1. You write tests using xUnit attributes
2. Moq creates fake dependencies (no real database/APIs)
3. Tests execute in milliseconds
4. FluentAssertions verify expected behavior
5. Code coverage is calculated

### What It Tests

✅ **Business Logic**
- Manager methods work correctly
- Service methods handle edge cases
- Business rules are enforced
- Calculations are accurate

✅ **Domain Logic**
- Entity validation
- Specification queries
- Domain rules

✅ **Controllers**
- HTTP responses (200, 404, 400, etc.)
- Request validation
- Action method logic
- Authorization checks

✅ **Extensions & Utilities**
- Extension methods
- Helper classes
- Utility functions

### Example Test

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class ContactManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<Contact>> _mockContactSet;
    private readonly ContactManager _sut; // System Under Test

    public ContactManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockContactSet = new Mock<DbSet<Contact>>();
        
        _mockDbContext.Setup(x => x.Contacts).Returns(_mockContactSet.Object);
        
        _sut = new ContactManager(_mockDbContext.Object);
    }

    [Fact]
    public async Task CreateContact_Should_Add_Contact_To_Database()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act
        var result = await _sut.CreateContactAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        
        _mockContactSet.Verify(
            x => x.AddAsync(It.IsAny<Contact>(), default),
            Times.Once
        );
        
        _mockDbContext.Verify(
            x => x.SaveChangesAsync(default),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateContact_Should_Throw_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            Email = email
        };

        // Act
        Func<Task> act = async () => await _sut.CreateContactAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*email*");
    }
}
```

### Performance

| Metric | Value |
|--------|-------|
| **Speed** | ⚡ Very Fast (milliseconds per test) |
| **Typical Suite** | 1000+ tests in 5-10 seconds |
| **When to Run** | Every code change, continuously |
| **Parallelization** | Yes, xUnit runs tests in parallel |

### Advantages

✅ **Extremely fast** - Run thousands of tests in seconds  
✅ **Isolated** - Each test is independent  
✅ **Detailed feedback** - Know exact line that failed  
✅ **Coverage metrics** - Measure exactly what's tested  
✅ **Easy to debug** - Can run single test, add breakpoints  
✅ **Cheap to maintain** - Simple mocks, clear structure  
✅ **TDD friendly** - Perfect for test-driven development  

### Disadvantages

❌ **No integration testing** - Can't verify components work together  
❌ **No real database** - Everything is mocked  
❌ **No real APIs** - External calls are faked  
❌ **Can miss integration bugs** - Tests pass but system fails  

---

## Framework #2: Integration Tests

### What It Does

Tests the **complete API** with real database and dependencies, verifying that layers work together correctly.

### How It Works

```
Test starts → WebApplicationFactory creates test server → 
Real DbContext with in-memory or test database → 
HTTP request sent to API → Real business logic executes →
Database operations occur → Response verified
```

1. `WebApplicationFactory` creates a test server
2. Test database is created (in-memory or real test DB)
3. HTTP requests sent to controllers
4. Real business logic + database operations
5. Responses and database state verified

### What It Tests

✅ **API Endpoints**
- Controllers work with real dependencies
- Request/response pipeline
- Model binding and validation
- Error handling middleware

✅ **Database Integration**
- Entity Framework queries
- Database constraints
- Transactions
- Migrations apply correctly

✅ **Authorization**
- Role-based access control
- Permission checks
- Context-based filtering

✅ **Business Logic Integration**
- Managers work with real repositories
- Services interact correctly
- Specifications execute properly

### Example Integration Test

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

public class ContactControllerIntegrationTests : IClassFixture<PAOWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PAOWebApplicationFactory _factory;

    public ContactControllerIntegrationTests(PAOWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetContact_Should_Return_Ok_With_Contact()
    {
        // Arrange
        var contactId = 1; // Assume seeded in test database

        // Act
        var response = await _client.GetAsync($"/api/contacts/{contactId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var contact = await response.Content.ReadFromJsonAsync<ContactModel>();
        contact.Should().NotBeNull();
        contact.Id.Should().Be(contactId);
    }

    [Fact]
    public async Task CreateContact_Should_Persist_To_Database()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = $"test-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdContact = await response.Content.ReadFromJsonAsync<ContactModel>();
        createdContact.Should().NotBeNull();
        createdContact.FirstName.Should().Be("Integration");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var dbContact = await dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == createdContact.Id);
        
        dbContact.Should().NotBeNull();
        dbContact.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task CreateContact_Should_Return_BadRequest_For_Invalid_Data()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "", // Invalid - required
            Email = "invalid-email" // Invalid format
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

### WebApplicationFactory Setup

```csharp
// Infrastructure/PAOWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

public class PAOWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to get the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Ensure database is created
            db.Database.EnsureCreated();

            // Seed test data
            SeedTestData(db);
        });
    }

    private void SeedTestData(AppDbContext context)
    {
        // Add test data
        context.Contacts.AddRange(
            new Contact { Id = 1, FirstName = "Test", LastName = "User1" },
            new Contact { Id = 2, FirstName = "Test", LastName = "User2" }
        );
        
        context.SaveChanges();
    }
}
```

### Performance

| Metric | Value |
|--------|-------|
| **Speed** | 🐢 Slower (seconds per test) |
| **Typical Suite** | 100-200 tests in 30-60 seconds |
| **When to Run** | Before commits, in CI/CD |
| **Parallelization** | Limited (database conflicts) |

### Advantages

✅ **Tests real integration** - All layers work together  
✅ **Catches integration bugs** - Finds issues unit tests miss  
✅ **Real database operations** - Verifies EF Core queries  
✅ **Tests middleware** - Validates entire request pipeline  
✅ **Authorization testing** - Real permission checks  

### Disadvantages

❌ **Slower** - Takes seconds per test  
❌ **More complex setup** - Requires test database  
❌ **Harder to debug** - Many moving parts  
❌ **Can be flaky** - Database state issues  
❌ **Limited coverage reporting** - Hard to measure  

---

## Why You Need Both

### Comparison Table

| Aspect | Unit Tests (xUnit + Moq) | Integration Tests |
|--------|--------------------------|-------------------|
| **Purpose** | Verify individual components | Verify components work together |
| **Scope** | Single class/method | Multiple layers (API → DB) |
| **Speed** | ⚡ Milliseconds | 🐢 Seconds |
| **Quantity** | 1000s of tests | 100-200 tests |
| **Dependencies** | All mocked | Real or in-memory DB |
| **Database** | Not touched | Real operations |
| **When** | Every code change | Before commit/deploy |
| **Finds** | Logic bugs, edge cases | Integration issues |
| **Cost** | 💰 Cheap | 💰💰 Moderate |
| **Debugging** | ⚡ Easy | 🐢 Moderate |
| **Coverage** | 📊 Precise | ❌ Hard to measure |
| **CI/CD Time** | ~5-10 seconds | ~30-60 seconds |

### What Each Framework Catches

#### Unit Tests Catch:

```csharp
// Logic error in business rule
public decimal CalculateDiscount(decimal price, int quantity)
{
    return price + quantity; // ❌ Should multiply, not add
}

// Null handling missing
public Contact GetContact(int id)
{
    return _contacts.First(c => c.Id == id); // ❌ Throws if not found
}

// Edge case not handled
public bool IsValidEmail(string email)
{
    return email.Contains("@"); // ❌ Too simplistic
}
```

#### Integration Tests Catch:

```csharp
// Integration issue:
// - Manager creates contact ✅
// - Database saves contact ✅  
// - But email notification fails ❌
// - Controller returns 200 ✅
// - User never notified ❌

// Authorization issue:
// - Controller action has [Authorize] ✅
// - User is authenticated ✅
// - But permission check fails ❌
// - User shouldn't see data ❌

// Database issue:
// - Business logic works ✅
// - EF Core query incorrect ❌
// - Wrong data returned ❌
```

### Real-World Scenario

**Bug**: Contact is created but doesn't appear in partner's contact list

**Unit Tests**:
```
✅ ContactManager.CreateContact() - PASS (creates contact)
✅ PartnerManager.GetContacts() - PASS (gets contacts)
✅ Contact entity validation - PASS
```

**Why they didn't catch it**: Each piece works in isolation, but the relationship isn't properly set up in the database.

**Integration Test**:
```
❌ POST /api/partners/123/contacts - PASS (creates contact)
❌ GET /api/partners/123/contacts - FAIL
   - Expected: Contact in list
   - Actual: Empty list
```

**Integration test catches it** because it tests the complete workflow with real database operations.

---

## Test Distribution Strategy

### Recommended Ratio

```
Total Tests: 100%
├── 70%  Unit Tests (xUnit + Moq)
│        → Fast feedback loop
│        → Test business logic, domain logic
│        → Mock all dependencies
│        → High coverage (80%+)
│
├── 25%  Integration Tests
│        → API endpoint tests
│        → Database integration tests
│        → Authorization/RBAC tests
│        → Real dependencies
│
└── 5%   E2E Tests (Optional - from frontend)
         → Critical user workflows
         → Can be done via frontend E2E tests
         → Or Postman/API tests
```

### What to Test with Each Type

#### Unit Tests (70%)

✅ **Must Have**:
- All business logic in Managers
- All domain logic (entities, specifications)
- All service methods
- Controller action methods (mocked dependencies)
- Extension methods
- Utility classes
- Validators

✅ **Test Scenarios**:
- Happy path
- Edge cases
- Error handling
- Null checks
- Boundary conditions
- Business rule validation

#### Integration Tests (25%)

✅ **Must Have**:
- CRUD operations via API
- Complex queries with specifications
- Authorization and RBAC
- Filtering and pagination
- Multi-entity operations
- Transaction handling

✅ **Test Scenarios**:
- Full API workflows
- Database constraints
- Permission checks
- Data integrity
- Error responses

#### E2E Tests (5%) - Optional

✅ **Must Have** (if doing backend E2E):
- Critical business workflows
- Multi-step operations
- Cross-entity relationships

**Note**: E2E tests can often be done from frontend testing instead.

### Example Test Suite

**Contact Feature** (~100 tests total):

```
Unit Tests (~70 tests):
├── ContactManager (30 tests)
│   ├── CreateContactAsync - success, validation errors, duplicates
│   ├── GetContactAsync - found, not found, deleted
│   ├── UpdateContactAsync - success, not found, validation
│   ├── DeleteContactAsync - success, not found, cascade
│   └── GetContactsWithFilters - various filter combinations
│
├── Contact Entity (15 tests)
│   ├── Validation rules
│   ├── Property setters
│   └── Business rules
│
├── ContactSpecifications (15 tests)
│   ├── ContactByOrgUnitSpec - various org units
│   ├── ContactByStatusSpec - various statuses
│   └── ContactCompositeSpec - combinations
│
└── ContactController (10 tests)
    ├── GetContact - 200, 404
    ├── CreateContact - 201, 400
    ├── UpdateContact - 200, 404, 400
    └── DeleteContact - 204, 404

Integration Tests (~25 tests):
├── API Endpoints (15 tests)
│   ├── CRUD operations via HTTP
│   ├── Filtering and pagination
│   ├── Authorization checks
│   └── Error handling
│
└── Database Operations (10 tests)
    ├── Complex queries
    ├── Transaction handling
    ├── Constraints
    └── Cascading deletes

E2E Tests (~5 tests) - Optional:
└── Complete Workflows
    ├── Create contact → Assign to partner → View in partner's list
    ├── Import contacts from CSV → Validate → Save
    └── Search contacts → Filter → Export
```

---

## Current Configuration

### What You Already Have ✅

Looking at your solution:

```
Current Test Infrastructure:
└── UNOPS.PAO.IntegrationTests/
    ├── xUnit framework ✅
    ├── WebApplicationFactory ✅
    ├── Controllers/ (API tests) ✅
    ├── UnitTests/ (some unit tests) ✅
    ├── Infrastructure/
    │   ├── IntegrationTestBase.cs ✅
    │   ├── PAOWebApplicationFactory.cs ✅
    │   └── Test helpers ✅
    └── TestData/ ✅
```

**Status**: Integration test infrastructure is **fully configured** ✅

### What You Need to Add ❌

```bash
# Unit test projects (currently missing)
tests/Unit/
├── UNOPS.PAO.Domain.Tests       ❌ Need to create
├── UNOPS.PAO.Business.Tests     ❌ Need to create
├── UNOPS.PAO.Presentation.Tests ❌ Need to create
└── UNOPS.PAO.Utilities.Tests    ❌ Need to create
```

**Status**: Unit test projects need to be created ❌

---

## Setup Requirements

### Prerequisites

```bash
# .NET SDK 9.0+
dotnet --version  # Should be 9.0.x or higher

# Verify solution builds
dotnet build
```

### Step 1: Integration Tests (Already Done! ✅)

Your integration tests are already configured. Verify they work:

```bash
cd UNOPS.PAO.IntegrationTests
dotnet test
```

### Step 2: Create Unit Test Projects

```bash
# Create test projects
dotnet new xunit -n UNOPS.PAO.Domain.Tests -o tests/Unit/UNOPS.PAO.Domain.Tests
dotnet new xunit -n UNOPS.PAO.Business.Tests -o tests/Unit/UNOPS.PAO.Business.Tests
dotnet new xunit -n UNOPS.PAO.Presentation.Tests -o tests/Unit/UNOPS.PAO.Presentation.Tests
dotnet new xunit -n UNOPS.PAO.Utilities.Tests -o tests/Unit/UNOPS.PAO.Utilities.Tests

# Add to solution
dotnet sln add tests/Unit/UNOPS.PAO.Domain.Tests/UNOPS.PAO.Domain.Tests.csproj
dotnet sln add tests/Unit/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj
dotnet sln add tests/Unit/UNOPS.PAO.Presentation.Tests/UNOPS.PAO.Presentation.Tests.csproj
dotnet sln add tests/Unit/UNOPS.PAO.Utilities.Tests/UNOPS.PAO.Utilities.Tests.csproj
```

### Step 3: Install Testing Packages

```bash
# Navigate to each test project and add packages

cd tests/Unit/UNOPS.PAO.Business.Tests

# Core testing packages
dotnet add package Moq                          # Mocking framework
dotnet add package FluentAssertions             # Better assertions
dotnet add package AutoFixture                  # Test data generation
dotnet add package AutoFixture.Xunit2           # AutoFixture + xUnit
dotnet add package AutoFixture.AutoMoq          # AutoFixture + Moq
dotnet add package coverlet.collector           # Code coverage
dotnet add package coverlet.msbuild             # Coverage in MSBuild
dotnet add package Microsoft.NET.Test.Sdk       # Test SDK

# For testing EF Core (Business tests)
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# For testing ASP.NET Core (Presentation tests)
dotnet add package Microsoft.AspNetCore.Mvc.Testing

# Repeat for other test projects
```

### Step 4: Create Base Test Classes

**Create `tests/Unit/UNOPS.PAO.Business.Tests/TestBase.cs`**:

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
        
        // Configure AutoFixture
        Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
```

### Step 5: Add Project References

```bash
cd tests/Unit/UNOPS.PAO.Domain.Tests
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj

cd ../UNOPS.PAO.Business.Tests
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj
dotnet add reference ../../../UNOPS.PAO.DataAccess/UNOPS.PAO.DataAccess.csproj

cd ../UNOPS.PAO.Presentation.Tests
dotnet add reference ../../../UNOPS.PAO.Presentation/UNOPS.PAO.Presentation.csproj
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../../UNOPS.PAO.Models/UNOPS.PAO.Models.csproj
```

### Step 6: Configure Code Coverage

**Update test projects** (.csproj):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    
    <!-- Code Coverage Configuration -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
    <Threshold>75</Threshold>
    <ThresholdType>line,branch</ThresholdType>
  </PropertyGroup>
</Project>
```

### Step 7: Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test project
dotnet test tests/Unit/UNOPS.PAO.Business.Tests

# Run single test
dotnet test --filter "FullyQualifiedName~ContactManagerTests.CreateContact_Should_Add_Contact"
```

### Total Setup Time

| Task | Time | Status |
|------|------|--------|
| Create test projects | 15 min | ⏳ Need to do |
| Install packages | 10 min | ⏳ Need to do |
| Add project references | 10 min | ⏳ Need to do |
| Configure coverage | 5 min | ⏳ Need to do |
| Write first tests | 30 min | ⏳ Need to do |
| Set up CI/CD | 15 min | ⏳ Optional |
| **Total** | **60-90 min** | |

---

## Testing Standards by Layer

### 1. Domain Layer Tests

**What to Test**:
- Entity validation
- Business rules in entities
- Specification query logic
- Enum behavior

**Example**:

```csharp
namespace UNOPS.PAO.Domain.Tests.Entities;

public class PartnerTests
{
    [Fact]
    public void Partner_Should_Require_Name()
    {
        // Arrange & Act
        var partner = new Partner();

        // Assert
        partner.Name.Should().BeNullOrEmpty();
        // Add validation logic to entity
    }

    [Fact]
    public void Partner_Should_Track_Audit_Information()
    {
        // Arrange
        var partner = new Partner
        {
            Name = "Test Partner",
            CreatedBy = "user1",
            CreatedDate = DateTime.UtcNow
        };

        // Act & Assert
        partner.CreatedBy.Should().Be("user1");
        partner.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
```

### 2. Business Layer Tests

**What to Test**:
- Manager methods (CRUD operations)
- Business logic
- Validation
- Error handling
- Service methods

**Example**:

```csharp
namespace UNOPS.PAO.Business.Tests.Managers;

public class ContactManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ContactManager _sut;

    public ContactManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _sut = new ContactManager(_mockDbContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateContact_Should_Call_SaveChanges()
    {
        // Arrange
        var request = new ContactRequest { FirstName = "John", LastName = "Doe" };
        var contact = new Contact { Id = 1, FirstName = "John" };
        
        _mockMapper.Setup(x => x.Map<Contact>(request)).Returns(contact);

        // Act
        await _sut.CreateContactAsync(request);

        // Assert
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
```

### 3. Presentation Layer Tests

**What to Test**:
- Controller actions return correct status codes
- Model validation
- Response objects
- Authorization attributes

**Example**:

```csharp
namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class ContactControllerTests
{
    private readonly Mock<IContactManager> _mockManager;
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _mockManager = new Mock<IContactManager>();
        _controller = new ContactController(_mockManager.Object);
    }

    [Fact]
    public async Task GetContact_Returns_Ok_When_Found()
    {
        // Arrange
        var contact = new ContactModel { Id = 1, FirstName = "John" };
        _mockManager.Setup(x => x.GetContactAsync(1)).ReturnsAsync(contact);

        // Act
        var result = await _controller.GetContact(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(contact);
    }

    [Fact]
    public async Task GetContact_Returns_NotFound_When_Missing()
    {
        // Arrange
        _mockManager.Setup(x => x.GetContactAsync(999)).ReturnsAsync((ContactModel)null);

        // Act
        var result = await _controller.GetContact(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
```

---

## Code Coverage

### Coverage Tools

```bash
# Install coverage tools
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
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

### Coverage Requirements

| Layer | Minimum Coverage | Target Coverage |
|-------|-----------------|-----------------|
| Domain | 80% | 90%+ |
| Business Logic | 80% | 85%+ |
| Controllers | 70% | 80%+ |
| Services | 80% | 85%+ |
| Utilities | 85% | 90%+ |
| **Overall** | **75%** | **80%+** |

### Enforce Coverage in CI/CD

```xml
<!-- In test project .csproj -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <Threshold>75</Threshold>
  <ThresholdType>line,branch,method</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
</PropertyGroup>
```

This will fail the build if coverage drops below 75%.

---

## CI/CD Integration

### GitHub Actions Example

```yaml
# .github/workflows/backend-tests.yml
name: Backend Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  # ────────────────────────────────────────
  # Step 1: Unit Tests
  # ────────────────────────────────────────
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run unit tests with coverage
        run: |
          dotnet test \
            --no-build \
            --filter "FullyQualifiedName!~IntegrationTests" \
            /p:CollectCoverage=true \
            /p:CoverletOutputFormat=cobertura \
            /p:Threshold=75
      
      - name: Generate coverage report
        run: |
          dotnet tool install -g dotnet-reportgenerator-globaltool
          reportgenerator \
            -reports:"**/coverage.cobertura.xml" \
            -targetdir:"coveragereport" \
            -reporttypes:"Html;Badges"
      
      - name: Upload coverage report
        uses: actions/upload-artifact@v3
        with:
          name: coverage-report
          path: coveragereport/
      
      - name: Comment coverage on PR
        if: github.event_name == 'pull_request'
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml
          fail_ci_if_error: true

  # ────────────────────────────────────────
  # Step 2: Integration Tests
  # ────────────────────────────────────────
  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: unit-tests  # Only run if unit tests pass
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run integration tests
        run: |
          dotnet test \
            UNOPS.PAO.IntegrationTests \
            --no-build \
            --logger "console;verbosity=detailed"
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: integration-test-results
          path: TestResults/

  # ────────────────────────────────────────
  # Step 3: Build & Deploy (if tests pass)
  # ────────────────────────────────────────
  deploy:
    name: Deploy
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests]
    if: github.ref == 'refs/heads/main'
    
    steps:
      - name: Deploy to production
        run: echo "Deploying..."
        # Your deployment script here
```

---

## Real-World Examples

### Complete Test Suite for Contact Feature

#### 1. Domain Tests

```csharp
// tests/Unit/UNOPS.PAO.Domain.Tests/Entities/ContactTests.cs
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Domain.Tests.Entities;

public class ContactTests
{
    [Fact]
    public void Contact_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        contact.Status.Should().Be(EntityStatus.Active);
        contact.IsDeleted.Should().BeFalse();
        contact.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("John", "Doe")]
    [InlineData("Jane", "Smith")]
    public void Contact_Should_Accept_Valid_Names(string firstName, string lastName)
    {
        // Arrange & Act
        var contact = new Contact
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Assert
        contact.FirstName.Should().Be(firstName);
        contact.LastName.Should().Be(lastName);
        contact.FullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void Contact_Should_Track_Modifications()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            ModifiedBy = "user1",
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };

        var originalDate = contact.ModifiedDate;

        // Act
        contact.FirstName = "Jane";
        contact.ModifiedBy = "user2";
        contact.ModifiedDate = DateTime.UtcNow;

        // Assert
        contact.FirstName.Should().Be("Jane");
        contact.ModifiedBy.Should().Be("user2");
        contact.ModifiedDate.Should().BeAfter(originalDate);
    }

    [Fact]
    public void Contact_Should_Support_Soft_Delete()
    {
        // Arrange
        var contact = new Contact { IsDeleted = false };

        // Act
        contact.IsDeleted = true;
        contact.DeletedDate = DateTime.UtcNow;
        contact.DeletedBy = "user1";

        // Assert
        contact.IsDeleted.Should().BeTrue();
        contact.DeletedDate.Should().NotBeNull();
        contact.DeletedBy.Should().Be("user1");
    }
}
```

#### 2. Specification Tests

```csharp
// tests/Unit/UNOPS.PAO.Domain.Tests/Specifications/ContactSpecificationTests.cs
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using Xunit;

namespace UNOPS.PAO.Domain.Tests.Specifications;

public class ContactSpecificationTests
{
    [Fact]
    public void ContactByOrgUnit_Should_Filter_By_Organization()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, OrganizationId = 100 },
            new() { Id = 2, OrganizationId = 200 },
            new() { Id = 3, OrganizationId = 100 }
        }.AsQueryable();

        var spec = new ContactByOrgUnitSpecification(100);

        // Act
        var result = contacts.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 3);
    }

    [Fact]
    public void ContactComposite_Should_Combine_Multiple_Filters()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 2, Status = EntityStatus.Inactive, IsDeleted = false },
            new() { Id = 3, Status = EntityStatus.Active, IsDeleted = true }
        }.AsQueryable();

        var spec = new ContactCompositeSpecification(
            status: EntityStatus.Active,
            includeDeleted: false
        );

        // Act
        var result = contacts.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }
}
```

#### 3. Business Logic Tests

```csharp
// tests/Unit/UNOPS.PAO.Business.Tests/Managers/ContactManagerTests.cs
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class ContactManagerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<Contact>> _mockContactSet;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ContactManager _sut;

    public ContactManagerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        
        _mockDbContext = new Mock<AppDbContext>();
        _mockContactSet = new Mock<DbSet<Contact>>();
        _mockMapper = new Mock<IMapper>();
        
        _mockDbContext.Setup(x => x.Contacts).Returns(_mockContactSet.Object);
        
        _sut = new ContactManager(
            _mockDbContext.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task CreateContact_Should_Add_Contact_And_Save()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890"
        };

        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _mockMapper.Setup(x => x.Map<Contact>(request)).Returns(contact);
        _mockMapper.Setup(x => x.Map<ContactModel>(contact))
            .Returns(new ContactModel { Id = 1, FirstName = "John" });

        // Act
        var result = await _sut.CreateContactAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        
        _mockContactSet.Verify(
            x => x.AddAsync(It.IsAny<Contact>(), default),
            Times.Once
        );
        
        _mockDbContext.Verify(
            x => x.SaveChangesAsync(default),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateContact_Should_Validate_Email_Format()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            Email = "invalid-email"  // Invalid format
        };

        // Act
        Func<Task> act = async () => await _sut.CreateContactAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task CreateContact_Should_Check_For_Duplicate_Email()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            Email = "existing@example.com"
        };

        var existingContacts = new List<Contact>
        {
            new() { Email = "existing@example.com" }
        }.AsQueryable();

        _mockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Provider).Returns(existingContacts.Provider);
        _mockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Expression).Returns(existingContacts.Expression);

        // Act
        Func<Task> act = async () => await _sut.CreateContactAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*duplicate*email*");
    }

    [Fact]
    public async Task GetContact_Should_Return_Contact_When_Found()
    {
        // Arrange
        var contactId = 1;
        var contact = new Contact { Id = contactId, FirstName = "John" };
        var expectedModel = new ContactModel { Id = contactId, FirstName = "John" };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(contact);
        _mockMapper.Setup(x => x.Map<ContactModel>(contact))
            .Returns(expectedModel);

        // Act
        var result = await _sut.GetContactAsync(contactId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contactId);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetContact_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var contactId = 999;
        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync((Contact)null);

        // Act
        var result = await _sut.GetContactAsync(contactId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateContact_Should_Modify_Existing_Contact()
    {
        // Arrange
        var contactId = 1;
        var existingContact = new Contact
        {
            Id = contactId,
            FirstName = "John",
            LastName = "Doe"
        };

        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe"
        };

        var updatedModel = new ContactModel
        {
            Id = contactId,
            FirstName = "Jane",
            LastName = "Doe"
        };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(existingContact);
        _mockMapper.Setup(x => x.Map<ContactModel>(existingContact))
            .Returns(updatedModel);

        // Act
        var result = await _sut.UpdateContactAsync(contactId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        existingContact.FirstName.Should().Be("Jane");
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteContact_Should_Soft_Delete()
    {
        // Arrange
        var contactId = 1;
        var contact = new Contact
        {
            Id = contactId,
            IsDeleted = false
        };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(contact);

        // Act
        await _sut.DeleteContactAsync(contactId);

        // Assert
        contact.IsDeleted.Should().BeTrue();
        contact.DeletedDate.Should().NotBeNull();
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

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
            new() { Id = 2, OrganizationId = 200, Status = EntityStatus.Active }
        }.AsQueryable();

        _mockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Provider).Returns(contacts.Provider);
        _mockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Expression).Returns(contacts.Expression);

        // Act
        var result = await _sut.GetContactsAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }
}
```

#### 4. Controller Tests

```csharp
// tests/Unit/UNOPS.PAO.Presentation.Tests/Controllers/ContactControllerTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class ContactControllerTests
{
    private readonly Mock<IContactManager> _mockContactManager;
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _mockContactManager = new Mock<IContactManager>();
        _controller = new ContactController(_mockContactManager.Object);
    }

    [Fact]
    public async Task GetContact_Should_Return_Ok_When_Contact_Exists()
    {
        // Arrange
        var contactId = 1;
        var contact = new ContactModel
        {
            Id = contactId,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockContactManager.Setup(x => x.GetContactAsync(contactId))
            .ReturnsAsync(contact);

        // Act
        var result = await _controller.GetContact(contactId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContact = okResult.Value.Should().BeOfType<ContactModel>().Subject;
        returnedContact.Id.Should().Be(contactId);
        returnedContact.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetContact_Should_Return_NotFound_When_Contact_Missing()
    {
        // Arrange
        var contactId = 999;
        _mockContactManager.Setup(x => x.GetContactAsync(contactId))
            .ReturnsAsync((ContactModel)null);

        // Act
        var result = await _controller.GetContact(contactId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateContact_Should_Return_Created_With_Location()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var createdContact = new ContactModel
        {
            Id = 123,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockContactManager.Setup(x => x.CreateContactAsync(request))
            .ReturnsAsync(createdContact);

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetContact));
        createdResult.RouteValues["id"].Should().Be(123);
        
        var contact = createdResult.Value.Should().BeOfType<ContactModel>().Subject;
        contact.Id.Should().Be(123);
        contact.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task CreateContact_Should_Return_BadRequest_When_ModelState_Invalid()
    {
        // Arrange
        var request = new ContactRequest();
        _controller.ModelState.AddModelError("FirstName", "Required");

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateContact_Should_Return_Ok_With_Updated_Contact()
    {
        // Arrange
        var contactId = 1;
        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe"
        };

        var updatedContact = new ContactModel
        {
            Id = contactId,
            FirstName = "Jane",
            LastName = "Doe"
        };

        _mockContactManager.Setup(x => x.UpdateContactAsync(contactId, updateRequest))
            .ReturnsAsync(updatedContact);

        // Act
        var result = await _controller.UpdateContact(contactId, updateRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var contact = okResult.Value.Should().BeOfType<ContactModel>().Subject;
        contact.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task UpdateContact_Should_Return_NotFound_When_Contact_Missing()
    {
        // Arrange
        var contactId = 999;
        var updateRequest = new UpdateContactRequest();

        _mockContactManager.Setup(x => x.UpdateContactAsync(contactId, updateRequest))
            .ReturnsAsync((ContactModel)null);

        // Act
        var result = await _controller.UpdateContact(contactId, updateRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteContact_Should_Return_NoContent_When_Successful()
    {
        // Arrange
        var contactId = 1;
        _mockContactManager.Setup(x => x.DeleteContactAsync(contactId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteContact(contactId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockContactManager.Verify(x => x.DeleteContactAsync(contactId), Times.Once);
    }

    [Fact]
    public async Task GetContacts_Should_Return_Paginated_Results()
    {
        // Arrange
        var filter = new ContactFilterRequest { PageSize = 10, PageNumber = 1 };
        var contacts = new List<ContactModel>
        {
            new() { Id = 1, FirstName = "John" },
            new() { Id = 2, FirstName = "Jane" }
        };

        _mockContactManager.Setup(x => x.GetContactsAsync(filter))
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.GetContacts(filter);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value.Should().BeAssignableTo<IEnumerable<ContactModel>>().Subject;
        returnedContacts.Should().HaveCount(2);
    }
}
```

---

## Best Practices

### DO's ✅

1. **Test behavior, not implementation**
   ```csharp
   // ✅ Good
   [Fact]
   public async Task CreateContact_Should_Return_Created_Status()
   {
       // Test what user experiences
   }
   
   // ❌ Bad
   [Fact]
   public void CreateContact_Should_Call_AddAsync()
   {
       // Testing internal implementation
   }
   ```

2. **Use descriptive test names**
   ```csharp
   // ✅ Good
   [Fact]
   public async Task GetContact_Should_Return_NotFound_When_Contact_Does_Not_Exist()
   
   // ❌ Bad
   [Fact]
   public async Task Test1()
   ```

3. **Follow AAA pattern** (Arrange, Act, Assert)
   ```csharp
   [Fact]
   public void AddNumbers_Should_Return_Sum()
   {
       // Arrange
       var calculator = new Calculator();
       
       // Act
       var result = calculator.Add(2, 3);
       
       // Assert
       result.Should().Be(5);
   }
   ```

4. **Test edge cases**
   ```csharp
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Validate_Should_Reject_Empty_Email(string email)
   {
       // Test edge cases
   }
   ```

5. **Mock all external dependencies**
   ```csharp
   var mockDbContext = new Mock<AppDbContext>();
   var mockEmailService = new Mock<IEmailService>();
   ```

### DON'Ts ❌

1. **Don't test framework code**
   ```csharp
   // ❌ Bad - testing Entity Framework
   [Fact]
   public void DbSet_Should_Add_Entity()
   {
       var dbSet = new Mock<DbSet<Contact>>();
       // Don't test EF Core itself
   }
   ```

2. **Don't write tests that depend on each other**
   ```csharp
   // ❌ Bad
   [Fact]
   public void Test1() { sharedState.Value = 5; }
   
   [Fact]
   public void Test2() { 
       // Depends on Test1
       Assert.Equal(5, sharedState.Value); 
   }
   ```

3. **Don't have multiple unrelated assertions**
   ```csharp
   // ❌ Bad
   [Fact]
   public void Test_Everything()
   {
       Assert.True(contact.IsValid);
       Assert.False(partner.IsDeleted);
       Assert.Equal(10, order.Total);
   }
   
   // ✅ Good - split into separate tests
   ```

4. **Don't use real database in unit tests**
   ```csharp
   // ❌ Bad
   [Fact]
   public async Task Test_With_Real_Database()
   {
       var dbContext = new AppDbContext(realConnectionString);
       // This is an integration test, not a unit test
   }
   
   // ✅ Good - use mocks
   var mockDbContext = new Mock<AppDbContext>();
   ```

---

## Quick Reference

### Commands Cheat Sheet

```bash
# ──────────────────────────────────────────
# Unit Tests
# ──────────────────────────────────────────

# Run all tests
dotnet test

# Run specific test project
dotnet test UNOPS.PAO.Business.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~ContactManagerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ContactManagerTests.CreateContact_Should_Add_Contact"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run in watch mode (continuous)
dotnet watch test

# ──────────────────────────────────────────
# Integration Tests
# ──────────────────────────────────────────

# Run only integration tests
dotnet test UNOPS.PAO.IntegrationTests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# ──────────────────────────────────────────
# Code Coverage
# ──────────────────────────────────────────

# Generate coverage report
dotnet test /p:CollectCoverage=true
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"

# View coverage report
start coveragereport/index.html  # Windows
open coveragereport/index.html   # macOS

# ──────────────────────────────────────────
# Build
# ──────────────────────────────────────────

# Restore packages
dotnet restore

# Build solution
dotnet build

# Clean build
dotnet clean && dotnet build
```

### Test Naming Conventions

```
# Test File Names
{ClassName}Tests.cs
ContactManagerTests.cs
PartnerServiceTests.cs

# Test Method Names
{Method}_Should_{ExpectedBehavior}_When_{Condition}

Examples:
GetContact_Should_Return_Contact_When_Found
CreateContact_Should_Throw_When_Email_Is_Duplicate
UpdateContact_Should_Return_NotFound_When_Contact_Missing
```

### Coverage Thresholds

| Layer | Minimum | Target |
|-------|---------|--------|
| Domain | 80% | 90%+ |
| Business | 80% | 85%+ |
| Controllers | 70% | 80%+ |
| Services | 80% | 85%+ |
| **Overall** | **75%** | **80%+** |

### When to Run Which Tests

| Situation | Run Unit Tests | Run Integration Tests |
|-----------|---------------|----------------------|
| **Writing code** | ✅ Continuously (watch mode) | ❌ No |
| **Before commit** | ✅ Yes | ✅ Yes |
| **Pull Request** | ✅ Automated in CI | ✅ Automated in CI |
| **Before deploy** | ✅ Yes | ✅ Yes |

---

## Summary

### The Bottom Line

**Yes, you need both unit tests and integration tests!**

| Framework | Purpose | Speed | Quantity | When |
|-----------|---------|-------|----------|------|
| **xUnit + Moq** | Unit testing | ⚡ Fast | Many (1000s) | Always |
| **Integration Tests** | API + DB testing | 🐢 Slower | Some (100-200) | Before deploy |

### Why Both?

- **Unit Tests** = Check each component works correctly ✅
- **Integration Tests** = Check components work together ✅
- **Together** = Confidence to deploy! 🚀

### What You Need to Do

1. ✅ **Integration Tests**: Already configured
2. ⏳ **Unit Tests**: Need to create projects (~60 min setup)
3. 📝 **Write tests**: Aim for 75%+ coverage
4. 🔄 **CI/CD**: Run both in automated pipeline
5. 🎯 **Target**: 80%+ coverage, all tests passing

### Next Steps

```bash
# 1. Create unit test projects (see Setup Requirements section)
dotnet new xunit -n UNOPS.PAO.Business.Tests

# 2. Install testing packages
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoFixture

# 3. Write your first test
# Create ContactManagerTests.cs

# 4. Run tests
dotnet test

# 5. Celebrate! 🎉
```

---

**Document Version**: 1.0  
**Last Updated**: January 15, 2025  
**Related Documents**: 
- [Backend Codebase Analysis](./BACKEND_CODEBASE_ANALYSIS.md)
- [Frontend Testing Guide](./UNOPS.PAO.ClientApp/ANGULAR_TESTING_FRAMEWORKS_GUIDE.md)

