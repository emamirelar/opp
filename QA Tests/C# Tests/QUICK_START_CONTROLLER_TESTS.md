# Quick Start - Controller Unit Tests

**Your controller test project is ready to use!** 🚀

---

## ✅ **What Was Set Up**

```
QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests/
├── Controllers/
│   └── PartnerControllerTests.cs          ✅ 16 example tests
├── TestBase/
│   └── ControllerTestBase.cs              ✅ Reusable base class
├── GlobalUsings.cs                        ✅ Common imports
├── UNOPS.PAO.Presentation.Tests.csproj    ✅ Project configured
└── README.md                              ✅ Full documentation
```

---

## 🚀 **Run Tests Now**

```bash
# Navigate to project
cd "c:\Users\Leonardc\git\opportunityplus\QA Tests\C# Tests\UNOPS.PAO.Presentation.Tests"

# Build project
dotnet build

# Run tests
dotnet test

# Expected result: 16 passing tests ✅
```

---

## 📝 **Create Your First Controller Test**

### **1. Copy the Template**

Use `PartnerControllerTests.cs` as your template:

```bash
# Copy partner tests as starting point
cp Controllers/PartnerControllerTests.cs Controllers/ContactControllerTests.cs
```

### **2. Update the Template**

Replace these items:
- `PartnerController` → `ContactController`
- `IPartnerManager` → `IContactManager`
- `PartnerModel` → `ContactModel`
- `CreatePartnerRequest` → `CreateContactRequest`

### **3. Run Your New Tests**

```bash
dotnet test --filter "ContactControllerTests"
```

---

## 🎯 **Test Pattern (Copy & Paste)**

```csharp
[Fact]
public async Task GetContact_WithValidId_ReturnsOk()
{
    // Arrange
    var contactId = 1;
    var expectedContact = new ContactModel
    {
        Id = contactId,
        FirstName = "John",
        LastName = "Doe"
    };

    _mockContactManager
        .Setup(m => m.GetContactAsync(contactId))
        .ReturnsAsync(expectedContact);

    SetupSuccessfulAuthorization();

    // Act
    var result = await _controller.GetContact(contactId);

    // Assert
    var okResult = AssertOkResult(result);
    var returnedContact = Assert.IsType<ContactModel>(okResult.Value);
    Assert.Equal(contactId, returnedContact.Id);
}
```

---

## 🛠️ **Common Helper Methods**

Available from `ControllerTestBase`:

```csharp
// Setup mocks
SetupSuccessfulAuthorization();  // User can access
SetupFailedAuthorization();       // User blocked

// Assert HTTP responses
AssertOkResult(result);           // 200 OK
AssertCreatedResult(result);      // 201 Created
AssertNotFoundResult(result);     // 404 Not Found
AssertBadRequestResult(result);   // 400 Bad Request
AssertForbidResult(result);       // 403 Forbidden

// Mock setup pattern
_mockManager
    .Setup(m => m.SomeMethod(param))
    .ReturnsAsync(expectedValue);
```

---

## 📊 **Test Categories to Add**

For each controller, add tests for:

1. **Constructor** (2 tests)
   - Valid dependencies
   - Null handling

2. **GET Tests** (4-5 tests)
   - Valid ID → 200 OK
   - Invalid ID → 404 Not Found
   - Deleted record → 404 Not Found
   - Unauthorized → 403 Forbidden

3. **POST Tests** (4-5 tests)
   - Valid data → 201 Created
   - Null request → 400 Bad Request
   - Invalid data → 400 Bad Request
   - Unauthorized → 403 Forbidden

4. **PUT Tests** (3-4 tests)
   - Valid data → 200 OK
   - Mismatched ID → 400 Bad Request
   - Not found → 404 Not Found

5. **DELETE Tests** (3 tests)
   - Valid ID → 204 No Content
   - Not found → 404 Not Found
   - Unauthorized → 403 Forbidden

**Total per controller**: ~40-45 tests

---

## 🎯 **Priority Order**

Test controllers in this order:

1. ✅ **PartnerController** (done - 16 tests)
2. ⏳ **ContactController** (next)
3. ⏳ **InteractionController**
4. ⏳ **OpportunityController**
5. ⏳ **DocumentController**
6. ... (33 more controllers)

---

## 📝 **Test Naming Convention**

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior

✅ Good:
GetPartner_WithValidId_ReturnsOk
GetPartner_WithInvalidId_ReturnsNotFound
CreatePartner_WithUnauthorizedUser_ReturnsForbid

❌ Bad:
Test1
PartnerTest
GetPartnerTest
```

---

## 🐛 **Common Issues & Fixes**

### **Issue 1: Mock not returning value**

```csharp
// ❌ Wrong - forgot .Object
var controller = new Controller(_mockManager);

// ✅ Correct - use .Object
var controller = new Controller(_mockManager.Object);
```

### **Issue 2: Test fails with NullReferenceException**

```csharp
// ❌ Wrong - mock not setup
_mockManager.Setup(...); // Never called

// ✅ Correct - setup in constructor
public MyTests()
{
    MockManager.Setup(m => m.MyManager).Returns(_mockMyManager.Object);
}
```

### **Issue 3: Authorization always fails**

```csharp
// ❌ Wrong - forgot to setup auth
var result = await _controller.GetPartner(1);

// ✅ Correct - setup authorization first
SetupSuccessfulAuthorization();
var result = await _controller.GetPartner(1);
```

---

## 📚 **Full Documentation**

- **Project README**: `UNOPS.PAO.Presentation.Tests/README.md`
- **Setup Complete**: `CONTROLLER_TESTS_SETUP_COMPLETE.md`
- **Coverage Analysis**: `../UNIT_TEST_COVERAGE_ANALYSIS.md`
- **Test Checklist**: `../TEST_COVERAGE_CHECKLIST.md`

---

## 🎉 **You're Ready!**

1. ✅ Project is created and configured
2. ✅ Example tests are working
3. ✅ Base class provides helpers
4. ✅ Template is ready to copy

**Next**: Create `ContactControllerTests.cs` using the pattern! 🚀

---

**Need Help?** Check the example: `Controllers/PartnerControllerTests.cs`
