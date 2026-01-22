# NotificationManager - Test Execution Report

**Manager**: `NotificationManager`  
**Location**: `UNOPS.PAO.Business/Managers/NotificationManager.cs`  
**Test Specification**: `Test Cases/Business/NotificationManager/NotificationManager_TestCases.md`  
**Execution Date**: November 11, 2025  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Executive Summary

**Total Test Cases**: 55  
**Test Categories**: Functional (25), Performance (10), Concurrency (10), Edge Cases (10)  
**Implementation Status**: ⚠️ Awaiting Implementation  
**Priority**: 🟡 **MEDIUM** (User Notifications & Communication)

---

## Test Categories Breakdown

### 1. Functional Tests (25 cases)

#### F001-F006: Basic Notification Retrieval
- **F001**: Get user notifications - unread only (default) ✅ Spec Complete
- **F002**: Get user notifications - all (read + unread) ✅ Spec Complete
- **F003**: Get notifications for user with no notifications ✅ Spec Complete
- **F004**: Get notifications - read only filter ✅ Spec Complete
- **F005**: Get notifications - ordered by creation date descending ✅ Spec Complete
- **F006**: Mark notification as read - valid notification ✅ Spec Complete

**Purpose**: Validate core notification retrieval with different filter combinations.

**Implementation Notes**:
```csharp
// Mock DbContext and Notification DbSet
// Test filtering by read status
// Verify ordering by CreatedDate DESC
// Test mark as read operation
```

#### F007-F013: Notification Status Management
- **F007**: Mark notification as read - wrong user (security) ✅ Spec Complete
- **F008**: Mark notification as read - non-existent notification ✅ Spec Complete
- **F009**: Mark already read notification as read (idempotent) ✅ Spec Complete
- **F010**: Update notification message and status ✅ Spec Complete
- **F011**: Update notification status to different values ✅ Spec Complete
- **F012**: Update non-existent notification ✅ Spec Complete
- **F013**: Create notification for user ✅ Spec Complete

**Purpose**: Ensure proper notification lifecycle management with security checks.

#### F014-F025: Advanced Features
- **F014**: Create notification with record data ✅ Spec Complete
- **F015**: Create notification with complex record object ✅ Spec Complete
- **F016**: Create notification with empty message ✅ Spec Complete
- **F017**: Parse record data - JSON array format ✅ Spec Complete
- **F018**: Parse record data - single object format ✅ Spec Complete
- **F019**: Parse record data - invalid JSON ✅ Spec Complete
- **F020**: Parse record data - null/empty string ✅ Spec Complete
- **F021**: Get notifications with category filter ✅ Spec Complete
- **F022**: Get notifications with response type filter ✅ Spec Complete
- **F023**: Notification with maximum message length ✅ Spec Complete
- **F024**: Bulk mark as read for user ✅ Spec Complete
- **F025**: Get notification count - unread for user ✅ Spec Complete

**Purpose**: Test complex JSON record data handling and filtering capabilities.

---

### 2. Performance Tests (10 cases)

| Test ID | Scenario | Target | Priority | Status |
|---------|----------|--------|----------|--------|
| **P001** | Get Notifications - 500 unread | < 500ms | 🔴 Critical | ⏳ Pending |
| **P002** | Create Notification | < 200ms | 🟡 High | ⏳ Pending |
| **P003** | Mark As Read - 100 notifications | < 1000ms | 🟡 High | ⏳ Pending |
| **P004** | Get Notifications - 10K total (filtered) | < 800ms | 🔴 Critical | ⏳ Pending |
| **P005** | Notification Creation Throughput | > 100/sec | 🟡 High | ⏳ Pending |
| **P006** | Update Notification | < 300ms | 🟢 Medium | ⏳ Pending |
| **P007** | Parse Complex Record Data (100KB JSON) | < 50ms | 🟡 High | ⏳ Pending |
| **P008** | Get Notifications - 50 users simultaneously | < 600ms each | 🔴 Critical | ⏳ Pending |
| **P009** | Notification Filtering | < 400ms | 🟡 High | ⏳ Pending |
| **P010** | Get Notifications - Recent 24 hours | < 300ms | 🟢 Medium | ⏳ Pending |

**Performance Baseline**: NotificationManager must handle high-volume notification queries efficiently, especially for users with many unread notifications.

**Critical Performance Paths**:
1. Get unread notifications (called frequently to show notification badge)
2. Mark as read (user interaction response time)
3. Create notification (bulk operations during system events)

---

### 3. Concurrency Tests (10 cases)

#### C001-C003: Read Operations Concurrency
- **C001**: Concurrent Get Notifications - Same User ⏳
  - **Scenario**: 10 threads query same user's notifications
  - **Expected**: All return same notification list
  - **Risk**: Caching issues or inconsistent reads
  
- **C002**: Concurrent Mark As Read - Different Notifications ⏳
  - **Scenario**: 20 threads mark different notifications as read
  - **Expected**: All 20 notifications marked read
  - **Risk**: Database deadlocks
  
- **C003**: Concurrent Mark As Read - Same Notification ⏳
  - **Scenario**: 5 threads mark same notification as read
  - **Expected**: Notification marked read once, no conflicts
  - **Risk**: Lost updates or duplicate processing

#### C004-C007: Write Operations Concurrency
- **C004**: Create Notifications During Query ⏳
  - **Scenario**: Creating notifications while user queries list
  - **Expected**: Queries always return valid state
  - **Risk**: Dirty reads or missing notifications
  
- **C005**: Concurrent Updates - Same Notification ⏳
  - **Scenario**: 3 threads update same notification with different messages
  - **Expected**: Consistent final state
  - **Risk**: Data corruption or lost updates
  
- **C006**: Bulk Create for Multiple Users ⏳
  - **Scenario**: 10 threads each create 100 notifications
  - **Expected**: 1000 total notifications created
  - **Risk**: Transaction isolation issues
  
- **C007**: Concurrent Read Status Changes ⏳
  - **Scenario**: User reading notifications while system marks them as read
  - **Expected**: Query returns snapshot, no errors
  - **Risk**: Stale data

#### C008-C010: Complex Concurrency Scenarios
- **C008**: Concurrent Notification Creation - Same User ⏳
  - **Scenario**: 15 threads create notifications for same user
  - **Expected**: All 15 created successfully
  - **Risk**: Unique constraint violations
  
- **C009**: Concurrent Category Filtering ⏳
  - **Scenario**: 8 threads query with different category filters
  - **Expected**: All filtered queries return correct data
  
- **C010**: Concurrent Record Data Parsing ⏳
  - **Scenario**: 20 threads parsing complex record JSON
  - **Expected**: All parse successfully
  - **Risk**: JSON parsing thread safety

---

### 4. Edge Cases (10 cases)

| Test ID | Scenario | Expected Behavior | Risk Level | Status |
|---------|----------|-------------------|------------|--------|
| **E001** | Notification With Null Message | Accept null or validation error | 🟡 Medium | ⏳ Pending |
| **E002** | Record Data - Malformed JSON | Return raw string in list | 🔴 High | ⏳ Pending |
| **E003** | Mark As Read - Different User Attempting | Security check, notification not found | 🔴 Critical | ⏳ Pending |
| **E004** | Notification With Very Long Message (10K chars) | Accept and store/truncate | 🟡 Medium | ⏳ Pending |
| **E005** | Get Notifications - User With 10K Notifications | Query completes successfully | 🔴 High | ⏳ Pending |
| **E006** | Record Data - Empty Array vs Null | Both return empty list | 🟢 Low | ⏳ Pending |
| **E007** | Notification Status - All Enum Values | All status values handled | 🟡 Medium | ⏳ Pending |
| **E008** | Mark As Read - Already Deleted Notification | Handle gracefully | 🟡 Medium | ⏳ Pending |
| **E009** | Concurrent Mark As Read - Race Condition | Idempotent, no errors | 🔴 High | ⏳ Pending |
| **E010** | Record Data - Deeply Nested JSON (10 levels) | Parse without stack overflow | 🟡 Medium | ⏳ Pending |

---

## Risk Assessment

### Critical Risks 🔴

1. **Notification Security Breach**
   - Impact: Users see other users' notifications
   - Test Coverage: F007, E003
   - Mitigation: Security validation tests, user ownership checks

2. **Performance Degradation with Large Datasets**
   - Impact: Slow notification loading, poor UX
   - Test Coverage: P001, P004, P008, E005
   - Mitigation: Pagination, indexing, query optimization tests

3. **JSON Parsing Failures**
   - Impact: Notification record data lost or corrupted
   - Test Coverage: F017-F020, E002, E010, C010
   - Mitigation: Robust JSON parsing with error handling

### High Risks 🟡

1. **Concurrent Mark-as-Read Issues**
   - Impact: Notifications not marked read, duplicate operations
   - Test Coverage: F009, C002-C003, C007, E009
   - Mitigation: Idempotency and race condition tests

2. **Bulk Operation Performance**
   - Impact: System slowdown during mass notifications
   - Test Coverage: P003, P005, C006
   - Mitigation: Bulk operation optimization and batching

---

## Implementation Checklist

### Setup (30 minutes)
- [ ] Create `NotificationManagerTests.cs` in unit test project
- [ ] Set up test fixtures and mock data
- [ ] Mock `AppDbContext`, `Notification` DbSet
- [ ] Create sample notifications with various statuses and record data

### Functional Tests (2.5 hours)
- [ ] Implement F001-F006: Basic notification operations
- [ ] Implement F007-F013: Status management and security
- [ ] Implement F014-F020: Record data and JSON parsing
- [ ] Implement F021-F025: Filtering and bulk operations

### Performance Tests (1 hour)
- [ ] Implement P001-P004: Core operation benchmarks
- [ ] Implement P005-P007: Throughput and parsing benchmarks
- [ ] Implement P008-P010: Concurrent and filtered query benchmarks

### Concurrency Tests (1 hour)
- [ ] Implement C001-C003: Read operation concurrency
- [ ] Implement C004-C007: Write operation concurrency
- [ ] Implement C008-C010: Complex concurrency scenarios

### Edge Cases (30 minutes)
- [ ] Implement E001-E010: All edge case scenarios

---

## Code Coverage Goals

**Target Coverage**: 85%+

### Critical Paths (Must Cover)
✅ GetNotificationsAsync(userId, includeRead)  
✅ MarkAsReadAsync(userId, notificationId)  
✅ CreateNotificationAsync(userId, message, recordData)  
✅ ParseRecordData(recordDataJson)  
✅ UpdateNotificationAsync(notificationId, status)

### Secondary Paths (Should Cover)
✅ Filtering by category and response type  
✅ Bulk mark as read  
✅ Get unread count  
✅ Security validation (user ownership)  
✅ JSON error handling

---

## Sample Test Implementation

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class NotificationManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<Notification>> _mockNotificationSet;
    private readonly NotificationManager _sut;

    public NotificationManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockNotificationSet = new Mock<DbSet<Notification>>();
        
        _mockDbContext.Setup(x => x.Notifications).Returns(_mockNotificationSet.Object);
        
        _sut = new NotificationManager(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetNotifications_Should_Return_Unread_Only_By_Default()
    {
        // Arrange
        var userId = 1;
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, IsRead = false, CreatedDate = DateTime.UtcNow },
            new() { Id = 2, UserId = userId, IsRead = true, CreatedDate = DateTime.UtcNow.AddHours(-1) },
            new() { Id = 3, UserId = userId, IsRead = false, CreatedDate = DateTime.UtcNow.AddMinutes(-30) }
        }.AsQueryable();

        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Provider).Returns(notifications.Provider);
        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Expression).Returns(notifications.Expression);

        // Act
        var result = await _sut.GetNotificationsAsync(userId, includeRead: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => !n.IsRead);
        result.Should().BeInDescendingOrder(n => n.CreatedDate);
    }

    [Fact]
    public async Task MarkAsRead_Should_Update_Notification_Status()
    {
        // Arrange
        var userId = 1;
        var notificationId = 100;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            IsRead = false,
            Message = "Test notification"
        };

        _mockNotificationSet.Setup(x => x.FindAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        await _sut.MarkAsReadAsync(userId, notificationId);

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadDate.Should().NotBeNull();
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task MarkAsRead_Should_Not_Update_Other_Users_Notification()
    {
        // Arrange
        var userId = 1;
        var otherUserId = 2;
        var notificationId = 100;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = otherUserId, // Different user
            IsRead = false
        };

        _mockNotificationSet.Setup(x => x.FindAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _sut.MarkAsReadAsync(userId, notificationId);

        // Assert
        result.Should().BeFalse(); // Security check failed
        notification.IsRead.Should().BeFalse(); // Not updated
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_Should_Be_Idempotent()
    {
        // Arrange
        var userId = 1;
        var notificationId = 100;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            IsRead = true, // Already read
            ReadDate = DateTime.UtcNow.AddHours(-1)
        };

        _mockNotificationSet.Setup(x => x.FindAsync(notificationId))
            .ReturnsAsync(notification);

        var originalReadDate = notification.ReadDate;

        // Act
        await _sut.MarkAsReadAsync(userId, notificationId);

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadDate.Should().Be(originalReadDate); // Unchanged
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateNotification_Should_Add_Notification()
    {
        // Arrange
        var userId = 1;
        var message = "Test notification message";
        var recordData = "{\"entityId\": 123, \"entityType\": \"FundingOpportunity\"}";

        // Act
        await _sut.CreateNotificationAsync(userId, message, recordData);

        // Assert
        _mockNotificationSet.Verify(
            x => x.AddAsync(
                It.Is<Notification>(n => 
                    n.UserId == userId && 
                    n.Message == message && 
                    n.RecordData == recordData &&
                    !n.IsRead),
                default),
            Times.Once
        );

        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Theory]
    [InlineData("[{\"id\": 1}, {\"id\": 2}]")] // JSON array
    [InlineData("{\"id\": 1}")] // Single object
    public void ParseRecordData_Should_Parse_Valid_JSON(string json)
    {
        // Act
        var result = _sut.ParseRecordData(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ParseRecordData_Should_Handle_Invalid_JSON()
    {
        // Arrange
        var invalidJson = "{invalid json";

        // Act
        var result = _sut.ParseRecordData(invalidJson);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result.First().Should().Be(invalidJson); // Returns raw string
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("[]")]
    public void ParseRecordData_Should_Return_Empty_For_Null_Or_Empty(string json)
    {
        // Act
        var result = _sut.ParseRecordData(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNotifications_Should_Filter_By_Category()
    {
        // Arrange
        var userId = 1;
        var category = "SystemAlert";
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, Category = "SystemAlert", IsRead = false },
            new() { Id = 2, UserId = userId, Category = "UserAction", IsRead = false },
            new() { Id = 3, UserId = userId, Category = "SystemAlert", IsRead = false }
        }.AsQueryable();

        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Provider).Returns(notifications.Provider);
        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Expression).Returns(notifications.Expression);

        // Act
        var result = await _sut.GetNotificationsAsync(userId, category: category);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => n.Category == category);
    }

    [Fact]
    public async Task GetUnreadCount_Should_Return_Correct_Count()
    {
        // Arrange
        var userId = 1;
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, IsRead = false },
            new() { Id = 2, UserId = userId, IsRead = true },
            new() { Id = 3, UserId = userId, IsRead = false },
            new() { Id = 4, UserId = userId, IsRead = false }
        }.AsQueryable();

        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Provider).Returns(notifications.Provider);
        _mockNotificationSet.As<IQueryable<Notification>>()
            .Setup(m => m.Expression).Returns(notifications.Expression);

        // Act
        var count = await _sut.GetUnreadCountAsync(userId);

        // Assert
        count.Should().Be(3);
    }
}
```

---

## Execution Timeline

### Week 1: Implementation
- **Day 1**: Functional tests F001-F010
- **Day 2**: Functional tests F011-F025
- **Day 3**: Performance tests P001-P010
- **Day 4**: Concurrency tests C001-C010
- **Day 5**: Edge cases E001-E010 + Review

### Week 2: Validation
- **Day 1**: Execute all tests, fix failures
- **Day 2**: Performance validation with large datasets
- **Day 3**: Security testing (user ownership validation)
- **Day 4**: Code coverage analysis (target: 85%+)
- **Day 5**: Documentation and CI/CD integration

---

## Success Criteria

✅ All 55 test cases implemented  
✅ All tests passing (100% pass rate)  
✅ Code coverage ≥ 85%  
✅ Performance targets met:
  - Get 500 notifications < 500ms
  - Create notification < 200ms
  - Mark 100 as read < 1000ms  
✅ Security checks working (user ownership validation)  
✅ JSON parsing robust with error handling  
✅ No concurrency issues in stress tests  
✅ CI/CD integration complete  

---

## Dependencies

### Required Packages
- xUnit (test framework)
- Moq (mocking)
- FluentAssertions (readable assertions)
- Microsoft.EntityFrameworkCore.InMemory (EF Core testing)
- Newtonsoft.Json or System.Text.Json (JSON parsing tests)

### Test Data Requirements
- Sample notifications with various statuses
- Users with different notification counts (0, few, many)
- JSON record data (valid, invalid, complex nested)
- Large datasets for performance testing (10K+ notifications)

---

## Related Components

This test suite validates:
- Notification delivery system
- User notification preferences
- Record data (entity references in JSON)
- Read/unread status tracking
- Category-based filtering

Impacts:
- User notification bell/badge
- Notification center UI
- Email notification triggers
- System alerts and warnings

---

## Next Steps

1. **Create test project infrastructure** (if not exists)
2. **Implement Priority 1: Functional tests** (F001-F025)
3. **Run initial validation**
4. **Add security tests** (user ownership validation)
5. **Performance test with large datasets**
6. **Stress test JSON parsing**
7. **Generate coverage reports**
8. **Document findings**

---

**Report Status**: Specification Complete ✅ | Implementation Pending ⚠️  
**Estimated Implementation Time**: 3 hours  
**Priority**: 🟡 MEDIUM (Important for user communication, not mission-critical)






