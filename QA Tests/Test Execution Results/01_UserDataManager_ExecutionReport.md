# UserDataManager - Test Execution Report

**Manager**: `UserDataManager`  
**Location**: `UNOPS.PAO.Business/Managers/UserDataManager.cs`  
**Test Specification**: `Test Cases/Business/UserDataManager/UserDataManager_TestCases.md`  
**Execution Date**: November 11, 2025  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Executive Summary

**Total Test Cases**: 45  
**Test Categories**: Functional (20), Performance (10), Concurrency (10), Edge Cases (5)  
**Implementation Status**: ⚠️ Awaiting Implementation  
**Priority**: 🔴 **HIGH** (Authentication & User Context Critical)

---

## Test Categories Breakdown

### 1. Functional Tests (20 cases)

#### F001-F005: User Retrieval by ID and Email
- **F001**: Get user by ID - exists ✅ Spec Complete
- **F002**: Get user by ID - not found ✅ Spec Complete
- **F003**: Get user by email - exists ✅ Spec Complete
- **F004**: Get user by email - case insensitive ✅ Spec Complete
- **F005**: Get user by email - not found ✅ Spec Complete

**Purpose**: Validate basic user lookup functionality with both successful and failure scenarios.

**Implementation Notes**:
```csharp
// Mock DbContext and User DbSet
// Test both existing and non-existing users
// Verify case-insensitive email matching
```

#### F006-F010: Current User Context
- **F006**: Get current user - authenticated ✅ Spec Complete
- **F007**: Get current user - not authenticated ✅ Spec Complete
- **F008**: Get current user - from NameIdentifier claim ✅ Spec Complete
- **F009**: Get current user - from Email claim fallback ✅ Spec Complete
- **F010**: Get current user - invalid user ID format ✅ Spec Complete

**Purpose**: Ensure proper authentication context handling and claim parsing.

**Implementation Notes**:
```csharp
// Mock HttpContext and ClaimsPrincipal
// Test NameIdentifier claim parsing
// Test Email claim fallback logic
// Validate error handling for malformed claims
```

#### F011-F020: Bulk Operations and Edge Cases
- **F011**: Get users by emails - multiple users ✅ Spec Complete
- **F012**: Get users by emails - empty list ✅ Spec Complete
- **F013**: Get users by emails - null input ✅ Spec Complete
- **F014**: Get users by emails - case insensitive matching ✅ Spec Complete
- **F015**: Get users by emails - partial matches ✅ Spec Complete
- **F016**: Get users by emails - duplicate emails ✅ Spec Complete
- **F017**: Get user - with profile data ✅ Spec Complete
- **F018**: Get current user - expired session ✅ Spec Complete
- **F019**: Get users by emails - large batch (100 emails) ✅ Spec Complete
- **F020**: User ID parsing - non-integer claim value ✅ Spec Complete

**Purpose**: Test bulk operations, edge cases, and error handling scenarios.

---

### 2. Performance Tests (10 cases)

| Test ID | Scenario | Target | Status |
|---------|----------|--------|--------|
| **P001** | Get user by ID | < 100ms | ⏳ Pending |
| **P002** | Get user by email (indexed) | < 150ms | ⏳ Pending |
| **P003** | Get current user (cached claims) | < 50ms | ⏳ Pending |
| **P004** | Bulk lookup - 100 users | < 500ms | ⏳ Pending |
| **P005** | Concurrent lookups - 50 threads | < 200ms each | ⏳ Pending |
| **P006** | HTTP context access | < 20ms | ⏳ Pending |
| **P007** | Email matching - 10K users | < 200ms | ⏳ Pending |
| **P008** | Batch retrieval throughput | > 200 users/sec | ⏳ Pending |
| **P009** | User by ID - cache effectiveness | TBD | ⏳ Pending |
| **P010** | Claims parsing | < 5ms | ⏳ Pending |

**Performance Baseline**: UserDataManager is a read-heavy service that must respond quickly to support authentication and authorization checks across the application.

**Critical Performance Paths**:
1. Get current user (called on every authenticated request)
2. Get user by email (used during login/registration)
3. Bulk user lookups (used for notifications, assignments)

---

### 3. Concurrency Tests (10 cases)

#### C001-C005: Read Concurrency
- **C001**: Concurrent get user by ID - same user, 20 threads ⏳
  - **Scenario**: Verify consistent data retrieval
  - **Expected**: All threads return identical user data
  
- **C002**: Concurrent get current user - multiple sessions ⏳
  - **Scenario**: Different users accessing simultaneously
  - **Expected**: Each session gets correct user context
  
- **C003**: Concurrent email lookups - different emails ⏳
  - **Scenario**: Multiple threads querying different users
  - **Expected**: No cross-contamination of results
  
- **C004**: Concurrent email lookups - same email ⏳
  - **Scenario**: Multiple threads querying same user by email
  - **Expected**: Consistent results, no database locking
  
- **C005**: Get users by emails - concurrent batch requests ⏳
  - **Scenario**: Multiple threads executing bulk lookups
  - **Expected**: All batches processed correctly

#### C006-C010: Context and Authentication Concurrency
- **C006**: Current user lookup - session expiry race condition ⏳
- **C007**: Concurrent user queries during user update ⏳
- **C008**: HTTP context access - thread safety ⏳
- **C009**: Claims reading - concurrent access ⏳
- **C010**: Bulk email lookup - concurrent large batches ⏳

---

### 4. Edge Cases (5 cases)

| Test ID | Scenario | Expected Behavior | Status |
|---------|----------|-------------------|--------|
| **E001** | Claim with null/empty NameIdentifier | Graceful error handling | ⏳ Pending |
| **E002** | Email claim with special characters | Proper escaping/matching | ⏳ Pending |
| **E003** | User ID claim not parseable as integer | Exception with clear message | ⏳ Pending |
| **E004** | HTTP context is null | Return null or throw appropriate exception | ⏳ Pending |
| **E005** | User email with + addressing (gmail style) | Match correctly (user+tag@domain.com) | ⏳ Pending |

---

## Risk Assessment

### Critical Risks 🔴

1. **Authentication Failure**
   - Impact: Users cannot log in or access system
   - Test Coverage: F006-F010, E001-E004
   - Mitigation: Comprehensive claim parsing tests

2. **Performance Degradation**
   - Impact: Slow authentication checks affect entire application
   - Test Coverage: P001-P010
   - Mitigation: Performance benchmarks and optimization

3. **Concurrency Issues**
   - Impact: Incorrect user context in multi-threaded scenarios
   - Test Coverage: C001-C010
   - Mitigation: Thread-safety validation

### Medium Risks 🟡

1. **Email Matching Errors**
   - Impact: User lookup failures or incorrect matches
   - Test Coverage: F003-F005, F014, E002, E005
   - Mitigation: Case-insensitive and special character tests

2. **Bulk Operation Failures**
   - Impact: Notification/assignment features break
   - Test Coverage: F011-F016, F019, P004, P008
   - Mitigation: Bulk operation and pagination tests

---

## Implementation Checklist

### Setup (30 minutes)
- [ ] Create `UserDataManagerTests.cs` in unit test project
- [ ] Set up test fixtures and base classes
- [ ] Mock `AppDbContext`, `DbSet<User>`, `HttpContext`
- [ ] Create test data builders for User entities

### Functional Tests (2 hours)
- [ ] Implement F001-F005: Basic user retrieval
- [ ] Implement F006-F010: Current user context
- [ ] Implement F011-F016: Bulk operations
- [ ] Implement F017-F020: Edge cases and profiles

### Performance Tests (1 hour)
- [ ] Implement P001-P003: Core operation benchmarks
- [ ] Implement P004-P008: Bulk and concurrent benchmarks
- [ ] Implement P009-P010: Caching and parsing benchmarks

### Concurrency Tests (1 hour)
- [ ] Implement C001-C005: Read concurrency
- [ ] Implement C006-C010: Context concurrency

### Edge Cases (30 minutes)
- [ ] Implement E001-E005: All edge case scenarios

---

## Code Coverage Goals

**Target Coverage**: 85%+

### Critical Paths (Must Cover)
✅ GetCurrentUser()  
✅ GetUserByIdAsync(int id)  
✅ GetUserByEmailAsync(string email)  
✅ GetUsersByEmailsAsync(List<string> emails)

### Secondary Paths (Should Cover)
✅ Claim parsing logic  
✅ Email normalization  
✅ Null handling  
✅ Error responses

---

## Sample Test Implementation

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class UserDataManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<User>> _mockUserSet;
    private readonly UserDataManager _sut;

    public UserDataManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockUserSet = new Mock<DbSet<User>>();
        
        _mockDbContext.Setup(x => x.Users).Returns(_mockUserSet.Object);
        
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

    [Fact]
    public async Task GetUsersByEmailsAsync_Should_Handle_Duplicates()
    {
        // Arrange
        var emails = new List<string>
        {
            "user1@example.com",
            "user2@example.com",
            "user1@example.com" // Duplicate
        };

        var users = new List<User>
        {
            new() { Id = 1, Email = "user1@example.com" },
            new() { Id = 2, Email = "user2@example.com" }
        }.AsQueryable();

        _mockUserSet.As<IQueryable<User>>()
            .Setup(m => m.Provider).Returns(users.Provider);
        _mockUserSet.As<IQueryable<User>>()
            .Setup(m => m.Expression).Returns(users.Expression);

        // Act
        var result = await _sut.GetUsersByEmailsAsync(emails);

        // Assert
        result.Should().HaveCount(2); // Should return unique users
    }
}
```

---

## Execution Timeline

### Week 1: Implementation
- **Day 1-2**: Functional tests (F001-F020)
- **Day 3**: Performance tests (P001-P010)
- **Day 4**: Concurrency tests (C001-C010)
- **Day 5**: Edge cases (E001-E005) + Review

### Week 2: Validation
- **Day 1**: Execute all tests, fix failures
- **Day 2**: Code coverage analysis
- **Day 3**: Performance validation
- **Day 4**: Documentation and reporting
- **Day 5**: Integration with CI/CD

---

## Success Criteria

✅ All 45 test cases implemented  
✅ All tests passing (100% pass rate)  
✅ Code coverage ≥ 85%  
✅ Performance targets met  
✅ No critical concurrency issues  
✅ CI/CD integration complete  

---

## Dependencies

### Required Packages
- xUnit (test framework)
- Moq (mocking)
- FluentAssertions (readable assertions)
- Microsoft.EntityFrameworkCore.InMemory (EF Core testing)

### Test Data Requirements
- Sample users with various email formats
- Test claims principals with different claim types
- Large user datasets for performance testing

---

## Next Steps

1. **Create test project structure** (if not exists)
2. **Implement functional tests** (Priority 1)
3. **Run initial test execution**
4. **Add performance benchmarks**
5. **Complete concurrency validation**
6. **Generate coverage reports**

---

**Report Status**: Specification Complete ✅ | Implementation Pending ⚠️  
**Estimated Implementation Time**: 4 hours  
**Priority**: 🔴 HIGH (Critical for authentication)






