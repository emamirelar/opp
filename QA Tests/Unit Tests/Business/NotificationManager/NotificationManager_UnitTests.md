# NotificationManager - Unit Test Cases

**Manager**: `NotificationManager`  
**File**: `UNOPS.PAO.Business/Managers/NotificationManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `NotificationManager` with focus on:
- Notification retrieval and filtering
- Mark as read functionality
- Notification creation
- Status updates
- User-specific notifications

**Total Test Cases**: 20+

---

## 1. Get Notifications Tests

### TC-NM-001: Get Unread Notifications (Default)
**Test**: `GetNotifications_Should_ReturnUnread_When_NoFilterProvided`

**Arrange**:
```csharp
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, Message = "Unread 1", IsRead = false },
    new() { Id = 2, UserId = 100, Message = "Read 1", IsRead = true },
    new() { Id = 3, UserId = 100, Message = "Unread 2", IsRead = false }
};
await Context.Notifications.AddRangeAsync(notifications);
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().OnlyContain(n => !n.IsRead);
```

---

### TC-NM-002: Get All Notifications
**Test**: `GetNotifications_Should_ReturnAll_When_UnreadOnlyIsFalse`

**Arrange**:
```csharp
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, IsRead = false },
    new() { Id = 2, UserId = 100, IsRead = true },
    new() { Id = 3, UserId = 100, IsRead = false }
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100, unreadOnly: false);
```

**Assert**:
```csharp
result.Should().HaveCount(3);
```

---

### TC-NM-003: Get Only Read Notifications
**Test**: `GetNotifications_Should_ReturnRead_When_UnreadOnlyIsExplicitlyFalse`

**Arrange**:
```csharp
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, IsRead = false },
    new() { Id = 2, UserId = 100, IsRead = true }
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100, unreadOnly: false);
```

**Assert**:
```csharp
result.Should().Contain(n => n.IsRead);
```

---

### TC-NM-004: Filter by User ID
**Test**: `GetNotifications_Should_FilterByUserId_When_MultipleUsersExist`

**Arrange**:
```csharp
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, Message = "User 100", IsRead = false },
    new() { Id = 2, UserId = 200, Message = "User 200", IsRead = false },
    new() { Id = 3, UserId = 100, Message = "User 100-2", IsRead = false }
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().OnlyContain(n => n.Message.Contains("User 100"));
```

---

### TC-NM-005: Order by CreatedAt Descending
**Test**: `GetNotifications_Should_OrderByCreatedAtDesc_When_MultipleNotificationsExist`

**Arrange**:
```csharp
var baseDate = DateTime.UtcNow;
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, CreatedAt = baseDate.AddDays(-2), IsRead = false },
    new() { Id = 2, UserId = 100, CreatedAt = baseDate.AddDays(-1), IsRead = false },
    new() { Id = 3, UserId = 100, CreatedAt = baseDate, IsRead = false }
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
result.Should().HaveCount(3);
result[0].Id.Should().Be(3); // Most recent
result[2].Id.Should().Be(1); // Oldest
```

---

## 2. Mark As Read Tests

### TC-NM-006: Mark Notification As Read
**Test**: `MarkAsRead_Should_UpdateIsRead_When_NotificationExists`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    Message = "Test",
    IsRead = false
};
await Context.Notifications.AddAsync(notification);
await SaveChangesAsync();
```

**Act**:
```csharp
await manager.MarkAsRead(1, 100);
```

**Assert**:
```csharp
var updated = await Context.Notifications.FindAsync(1);
updated.IsRead.Should().BeTrue();
```

---

### TC-NM-007: Mark As Read - Wrong User
**Test**: `MarkAsRead_Should_NotUpdate_When_UserIdMismatch`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    IsRead = false
};
await Context.Notifications.AddAsync(notification);
```

**Act**:
```csharp
await manager.MarkAsRead(1, 999); // Different user
```

**Assert**:
```csharp
var unchanged = await Context.Notifications.FindAsync(1);
unchanged.IsRead.Should().BeFalse(); // Should not change
```

---

### TC-NM-008: Mark As Read - Non-Existent Notification
**Test**: `MarkAsRead_Should_DoNothing_When_NotificationDoesNotExist`

**Arrange**:
```csharp
// No notification with ID 999
```

**Act**:
```csharp
await manager.MarkAsRead(999, 100);
```

**Assert**:
```csharp
// Should not throw exception
var count = await Context.Notifications.CountAsync();
count.Should().Be(0);
```

---

## 3. Update Notification Tests

### TC-NM-009: Update Notification Message and Status
**Test**: `UpdateNotification_Should_UpdateFields_When_NotificationExists`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    Message = "Original Message",
    Status = NotificationStatus.Pending
};
await Context.Notifications.AddAsync(notification);
```

**Act**:
```csharp
await manager.UpdateNotification(1, "Updated Message", NotificationStatus.Completed);
```

**Assert**:
```csharp
var updated = await Context.Notifications.FindAsync(1);
updated.Message.Should().Be("Updated Message");
updated.Status.Should().Be(NotificationStatus.Completed);
```

---

### TC-NM-010: Update Non-Existent Notification
**Test**: `UpdateNotification_Should_DoNothing_When_NotificationDoesNotExist`

**Arrange**:
```csharp
// No notification
```

**Act**:
```csharp
await manager.UpdateNotification(999, "Updated", NotificationStatus.Completed);
```

**Assert**:
```csharp
// Should not throw exception
```

---

## 4. Create Notification Tests

### TC-NM-011: Create Notification
**Test**: `CreateNotification_Should_AddNotification_When_ValidDataProvided`

**Arrange**:
```csharp
var record = new { EntityId = 1, EntityType = "Partner" };
```

**Act**:
```csharp
await manager.CreateNotification(100, "Test Message", "Info", "standard", record);
```

**Assert**:
```csharp
var notification = await Context.Notifications.FirstOrDefaultAsync();
notification.Should().NotBeNull();
notification.UserId.Should().Be(100);
notification.Message.Should().Be("Test Message");
notification.Category.Should().Be("Info");
notification.IsRead.Should().BeFalse();
notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
```

---

### TC-NM-012: Create Notification with Record Data
**Test**: `CreateNotification_Should_SerializeRecordData_When_RecordProvided`

**Arrange**:
```csharp
var record = new { Id = 1, Name = "Test" };
```

**Act**:
```csharp
await manager.CreateNotification(100, "Message", "Category", "type", record);
```

**Assert**:
```csharp
var notification = await Context.Notifications.FirstAsync();
notification.RecordData.Should().NotBeNullOrEmpty();
notification.RecordData.Should().Contain("\"Id\":1");
```

---

### TC-NM-013: Create Multiple Notifications for User
**Test**: `CreateNotification_Should_AllowMultiple_When_CalledMultipleTimes`

**Arrange**:
```csharp
var record1 = new { Id = 1 };
var record2 = new { Id = 2 };
```

**Act**:
```csharp
await manager.CreateNotification(100, "Message 1", "Cat1", "type1", record1);
await manager.CreateNotification(100, "Message 2", "Cat2", "type2", record2);
```

**Assert**:
```csharp
var notifications = await Context.Notifications.Where(n => n.UserId == 100).ToListAsync();
notifications.Should().HaveCount(2);
```

---

## 5. Parse Record Data Tests

### TC-NM-014: Parse Array Record Data
**Test**: `GetNotifications_Should_ParseArrayRecordData_When_ArrayFormat`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    Message = "Test",
    RecordData = "[{\"Id\":1},{\"Id\":2}]", // Array format
    IsRead = false
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
var notif = result.First();
notif.Records.Should().NotBeNull();
notif.Records.Should().HaveCount(2);
```

---

### TC-NM-015: Parse Object Record Data
**Test**: `GetNotifications_Should_ParseObjectRecordData_When_ObjectFormat`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    RecordData = "{\"Id\":1,\"Name\":\"Test\"}", // Object format
    IsRead = false
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
var notif = result.First();
notif.Records.Should().NotBeNull();
notif.Records.Should().HaveCount(1); // Wrapped in array
```

---

## 6. Edge Case Tests

### TC-NM-016: Empty Notifications List
**Test**: `GetNotifications_Should_ReturnEmpty_When_NoNotificationsExist`

**Arrange**:
```csharp
// No notifications
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
result.Should().BeEmpty();
```

---

### TC-NM-017: User with No Notifications
**Test**: `GetNotifications_Should_ReturnEmpty_When_UserHasNoNotifications`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 200, // Different user
    IsRead = false
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100); // User 100
```

**Assert**:
```csharp
result.Should().BeEmpty();
```

---

### TC-NM-018: All Notifications Read
**Test**: `GetNotifications_Should_ReturnEmpty_When_AllNotificationsRead`

**Arrange**:
```csharp
var notifications = new List<Notification>
{
    new() { Id = 1, UserId = 100, IsRead = true },
    new() { Id = 2, UserId = 100, IsRead = true }
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100); // Default: unread only
```

**Assert**:
```csharp
result.Should().BeEmpty();
```

---

### TC-NM-019: Null Record Data
**Test**: `GetNotifications_Should_HandleNullRecordData_When_RecordDataIsNull`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    RecordData = null, // Null record data
    IsRead = false
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
var notif = result.First();
notif.Records.Should().NotBeNull(); // Should be empty list, not null
notif.Records.Should().BeEmpty();
```

---

### TC-NM-020: Invalid JSON in Record Data
**Test**: `GetNotifications_Should_HandleInvalidJSON_When_RecordDataMalformed`

**Arrange**:
```csharp
var notification = new Notification
{
    Id = 1,
    UserId = 100,
    RecordData = "invalid{json}", // Malformed JSON
    IsRead = false
};
```

**Act**:
```csharp
var result = await manager.GetNotifications(100);
```

**Assert**:
```csharp
// Should not throw exception
var notif = result.First();
notif.Records.Should().NotBeNull();
```

---

## Test Data Factory

```csharp
public class NotificationTestDataFactory
{
    private int _sequenceNumber = 1;

    public Notification CreateNotification(Action<Notification>? customize = null)
    {
        var notification = new Notification
        {
            Id = _sequenceNumber++,
            UserId = 100,
            Message = $"Notification {_sequenceNumber}",
            Category = "Info",
            ResponseType = "standard",
            RecordData = "[{\"Id\":1}]",
            IsRead = false,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        customize?.Invoke(notification);
        return notification;
    }

    public Notification CreateReadNotification(int userId)
    {
        return CreateNotification(n =>
        {
            n.UserId = userId;
            n.IsRead = true;
        });
    }

    public Notification CreateUnreadNotification(int userId)
    {
        return CreateNotification(n =>
        {
            n.UserId = userId;
            n.IsRead = false;
        });
    }

    public List<Notification> CreateNotificationsForUser(int userId, int count, bool isRead = false)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateNotification(n =>
            {
                n.UserId = userId;
                n.IsRead = isRead;
            }))
            .ToList();
    }
}
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **Get Notifications** | 5 tests | 90% |
| **Mark As Read** | 3 tests | 90% |
| **Update Notification** | 2 tests | 85% |
| **Create Notification** | 3 tests | 90% |
| **Parse Record Data** | 2 tests | 85% |
| **Edge Cases** | 5 tests | 80% |
| **Overall** | **20+ tests** | **85%+** |

---

**End of NotificationManager Unit Test Cases**

