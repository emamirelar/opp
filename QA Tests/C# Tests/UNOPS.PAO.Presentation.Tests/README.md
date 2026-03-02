# UNOPS.PAO.Presentation.Tests

**Controller Unit Tests** for the UNOPS Opportunity+ Partnership and Opportunity Management System.

## 📋 **Overview**

This project contains **unit tests for API controllers** (Presentation layer). These tests validate:
- ✅ HTTP contract (status codes, routing, model binding)
- ✅ Authorization and permission checks
- ✅ Request/response handling
- ✅ Error handling and validation

**Important**: These are **UNIT TESTS**, not integration tests. All dependencies (managers, services) are **mocked**.

---

## 🏗️ **Test Architecture**

### **Test Pattern: AAA (Arrange-Act-Assert)**

```csharp
[Fact]
public async Task GetPartner_WithValidId_ReturnsOk()
{
    // Arrange - Setup test data and mocks
    var partnerId = 1;
    _mockPartnerManager
        .Setup(m => m.GetPartnerAsync(partnerId))
        .ReturnsAsync(new PartnerModel { Id = partnerId, Name = "Test" });

    // Act - Call controller method
    var result = await _controller.GetPartner(partnerId);

    // Assert - Verify HTTP response
    var okResult = AssertOkResult(result);
    Assert.NotNull(okResult.Value);
}
```

---

## 🔧 **Test Base Classes**

### **ControllerTestBase**

All controller tests inherit from `ControllerTestBase.cs`, which provides:

```csharp
public class MyControllerTests : ControllerTestBase
{
    // Access to:
    protected Mock<IManagerWrapper> MockManager;
    protected Mock<IAuthorizationService> MockAuthorizationService;
    protected Mock<IMapper> MockMapper;
    
    // Helper methods:
    protected HttpContext CreateMockHttpContext()
    protected void SetupSuccessfulAuthorization()
    protected OkObjectResult AssertOkResult(IActionResult result)
    protected NotFoundObjectResult AssertNotFoundResult(IActionResult result)
    // ... and more
}
```

---

## 📊 **Test Coverage by Controller**

### **Current Status:**

| Controller | Tests | Status | Priority |
|-----------|-------|--------|----------|
| PartnerController | 15 tests | ✅ Implemented | P0 |
| ContactController | 0 tests | ⏳ Todo | P0 |
| InteractionController | 0 tests | ⏳ Todo | P0 |
| OpportunityController | 0 tests | ⏳ Todo | P0 |
| DocumentController | 0 tests | ⏳ Todo | P1 |
| UserManagementController | 0 tests | ⏳ Todo | P1 |
| WorkflowController | 0 tests | ⏳ Todo | P1 |
| ... | 0 tests | ⏳ Todo | P1-P2 |

**Target**: 37 controllers × ~40 tests = ~1,480 tests

---

## 🎯 **Test Categories**

Each controller test suite should include:

### **1. Constructor Tests** (~2-3 tests)
- Valid dependency injection
- Null argument handling
- Controller initialization

### **2. Authorization Tests** (~8-10 tests)
- Unauthorized user (401)
- Forbidden access (403)
- Permission checks for each action
- Anonymous user handling

### **3. CRUD Operation Tests** (~15-20 tests)
- GET (retrieve records)
- POST (create records)
- PUT (update records)
- DELETE (soft delete records)
- Success and failure scenarios

### **4. Validation Tests** (~8-10 tests)
- Invalid model binding
- Required field validation
- Business rule enforcement
- Data type validation

### **5. Error Handling Tests** (~8-10 tests)
- Not found (404)
- Bad request (400)
- Internal server error (500)
- Exception handling

### **6. Soft Delete Tests** (~3-5 tests)
- Deleted records filtered out
- IsDeleted flag validation
- DeletedBy/DeletedDate set correctly

---

## 🚀 **Running Tests**

### **Run All Controller Tests:**
```bash
cd "QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests"
dotnet test
```

### **Run Specific Controller Tests:**
```bash
dotnet test --filter "FullyQualifiedName~PartnerControllerTests"
```

### **Run with Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### **Run Specific Test:**
```bash
dotnet test --filter "GetPartner_WithValidId_ReturnsOk"
```

---

## 📝 **Test Writing Guidelines**

### **Naming Convention:**

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task GetPartner_WithValidId_ReturnsOk() { }

[Fact]
public async Task GetPartner_WithInvalidId_ReturnsNotFound() { }

[Fact]
public async Task CreatePartner_WithUnauthorizedUser_ReturnsForbid() { }
```

### **Mock Setup:**

```csharp
// ✅ CORRECT - Setup specific behavior
_mockPartnerManager
    .Setup(m => m.GetPartnerAsync(partnerId))
    .ReturnsAsync(expectedPartner);

// ❌ WRONG - Don't use real implementations
var realManager = new PartnerManager(realContext); // No!
```

### **Assertion Helpers:**

```csharp
// Use base class assertion helpers
var okResult = AssertOkResult(result);           // 200 OK
var createdResult = AssertCreatedResult(result); // 201 Created
var notFoundResult = AssertNotFoundResult(result); // 404 Not Found
var badRequestResult = AssertBadRequestResult(result); // 400 Bad Request
AssertForbidResult(result);                      // 403 Forbidden
```

---

## 🛠️ **Test Template**

Use this template when creating new controller tests:

```csharp
/**
 * @fileoverview Unit tests for [Controller]
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.Presentation.Controllers.[Area];

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class [Controller]Tests : ControllerTestBase
{
    private readonly Mock<I[Manager]> _mock[Manager];
    private readonly Mock<ILogger<[Controller]>> _mockLogger;
    private readonly [Controller] _controller;

    public [Controller]Tests()
    {
        // Setup mocks
        _mock[Manager] = new Mock<I[Manager]>();
        _mockLogger = new Mock<ILogger<[Controller]>>();

        // Setup manager wrapper
        MockManager.Setup(m => m.[Manager]).Returns(_mock[Manager].Object);

        // Create controller
        _controller = new [Controller](
            MockManager.Object,
            MockAuthorizationService.Object,
            _mockLogger.Object
        );

        SetupControllerContext(_controller);
    }

    #region Constructor Tests
    [Fact]
    public void Constructor_WithValidDependencies_CreatesController() { }
    #endregion

    #region GET Tests
    [Fact]
    public async Task Get_WithValidId_ReturnsOk() { }
    
    [Fact]
    public async Task Get_WithInvalidId_ReturnsNotFound() { }
    #endregion

    #region POST Tests
    [Fact]
    public async Task Create_WithValidData_ReturnsCreated() { }
    
    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest() { }
    #endregion

    #region PUT Tests
    [Fact]
    public async Task Update_WithValidData_ReturnsOk() { }
    #endregion

    #region DELETE Tests
    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent() { }
    #endregion

    #region Authorization Tests
    [Fact]
    public async Task Get_WithUnauthorizedUser_ReturnsForbid() { }
    #endregion

    public override void Dispose()
    {
        _controller?.Dispose();
        base.Dispose();
    }
}
```

---

## 📈 **Test Metrics**

### **Current Status:**
- **Total Tests**: 15
- **Passing**: 15 ✅
- **Failing**: 0 ❌
- **Skipped**: 0 ⏭️
- **Coverage**: PartnerController (~30%)

### **Target Goals:**
- **Total Tests**: ~1,480
- **Coverage**: 80% of all controllers
- **Pass Rate**: 100%

---

## 🔗 **Related Projects**

| Project | Purpose | Test Type |
|---------|---------|-----------|
| **UNOPS.PAO.Presentation.Tests** | Controller unit tests | Unit Tests |
| UNOPS.PAO.Business.Tests | Manager/service unit tests | Unit Tests |
| UNOPS.PAO.IntegrationTests | Multi-component integration | Integration Tests |
| Playwright Tests | Full app E2E tests | E2E Tests |

---

## 📚 **Resources**

- **Test Coverage Analysis**: `QA Tests/UNIT_TEST_COVERAGE_ANALYSIS.md`
- **Test Checklist**: `QA Tests/TEST_COVERAGE_CHECKLIST.md`
- **Implementation Guidelines**: `.cursor/rules/dotnet-implementation.mdc`

---

**Created**: January 30, 2026  
**Status**: Initial Implementation  
**Next Steps**: Add ContactController, InteractionController, OpportunityController tests
