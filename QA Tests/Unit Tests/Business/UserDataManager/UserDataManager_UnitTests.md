# UserDataManager - Unit Test Cases

**Manager**: `UserDataManager`  
**File**: `UNOPS.PAO.Business/Managers/UserDataManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UserDataManager` with focus on:
- User preferences management
- Settings persistence
- User profile data
- Default value handling
- Data validation

**Total Test Cases**: 15+

---

## 1. Get User Preferences Tests

### TC-UD-001: Get User Preferences
**Test**: `GetUserPreferences_Should_ReturnPreferences_When_UserExists`

**Arrange**:
```csharp
var userData = new UserData
{
    Id = 1,
    UserId = 100,
    Key = "preferences",
    Value = "{\"theme\":\"dark\",\"language\":\"en\"}"
};
await Context.UserData.AddAsync(userData);
```

**Act**:
```csharp
var result = await manager.GetUserPreferencesAsync(100);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Theme.Should().Be("dark");
result.Language.Should().Be("en");
```

---

### TC-UD-002: Get Non-Existent User Preferences Returns Default
**Test**: `GetUserPreferences_Should_ReturnDefaults_When_UserHasNoPreferences`

**Arrange**:
```csharp
// No user data
```

**Act**:
```csharp
var result = await manager.GetUserPreferencesAsync(100);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Theme.Should().Be("light"); // Default theme
result.Language.Should().Be("en"); // Default language
```

---

## 2. Save User Preferences Tests

### TC-UD-003: Save New Preferences
**Test**: `SaveUserPreferences_Should_CreateNew_When_NoPreferencesExist`

**Arrange**:
```csharp
var preferences = new UserPreferences
{
    Theme = "dark",
    Language = "fr"
};
```

**Act**:
```csharp
await manager.SaveUserPreferencesAsync(100, preferences);
```

**Assert**:
```csharp
var saved = await Context.UserData
    .FirstOrDefaultAsync(ud => ud.UserId == 100 && ud.Key == "preferences");
saved.Should().NotBeNull();
saved.Value.Should().Contain("dark");
saved.Value.Should().Contain("fr");
```

---

### TC-UD-004: Update Existing Preferences
**Test**: `SaveUserPreferences_Should_Update_When_PreferencesExist`

**Arrange**:
```csharp
var existing = new UserData
{
    Id = 1,
    UserId = 100,
    Key = "preferences",
    Value = "{\"theme\":\"light\"}"
};
await Context.UserData.AddAsync(existing);

var newPreferences = new UserPreferences { Theme = "dark" };
```

**Act**:
```csharp
await manager.SaveUserPreferencesAsync(100, newPreferences);
```

**Assert**:
```csharp
var updated = await Context.UserData.FindAsync(1);
updated.Value.Should().Contain("dark");
updated.Value.Should().NotContain("light");
```

---

## 3. User Settings Tests

### TC-UD-005: Get User Settings
**Test**: `GetUserSettings_Should_ReturnSettings_When_Saved`

**Arrange**:
```csharp
var userData = new UserData
{
    UserId = 100,
    Key = "settings",
    Value = "{\"emailNotifications\":true,\"smsNotifications\":false}"
};
```

**Act**:
```csharp
var result = await manager.GetUserSettingsAsync(100);
```

**Assert**:
```csharp
result.EmailNotifications.Should().BeTrue();
result.SmsNotifications.Should().BeFalse();
```

---

### TC-UD-006: Save User Settings
**Test**: `SaveUserSettings_Should_PersistSettings_When_ValidDataProvided`

**Arrange**:
```csharp
var settings = new UserSettings
{
    EmailNotifications = true,
    SmsNotifications = true
};
```

**Act**:
```csharp
await manager.SaveUserSettingsAsync(100, settings);
```

**Assert**:
```csharp
var saved = await Context.UserData
    .FirstOrDefaultAsync(ud => ud.UserId == 100 && ud.Key == "settings");
saved.Should().NotBeNull();
```

---

## 4. User Data Key-Value Tests

### TC-UD-007: Get Data by Key
**Test**: `GetUserData_Should_ReturnValue_When_KeyExists`

**Arrange**:
```csharp
var userData = new UserData
{
    UserId = 100,
    Key = "lastLogin",
    Value = "2024-01-15"
};
```

**Act**:
```csharp
var result = await manager.GetUserDataAsync(100, "lastLogin");
```

**Assert**:
```csharp
result.Should().Be("2024-01-15");
```

---

### TC-UD-008: Get Non-Existent Key Returns Null
**Test**: `GetUserData_Should_ReturnNull_When_KeyDoesNotExist`

**Arrange**:
```csharp
// No data for key
```

**Act**:
```csharp
var result = await manager.GetUserDataAsync(100, "nonExistentKey");
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-UD-009: Save Data by Key
**Test**: `SaveUserData_Should_PersistKeyValue_When_ValidDataProvided`

**Arrange**:
```csharp
// No existing data
```

**Act**:
```csharp
await manager.SaveUserDataAsync(100, "dashboard", "{\"layout\":\"grid\"}");
```

**Assert**:
```csharp
var saved = await Context.UserData
    .FirstOrDefaultAsync(ud => ud.UserId == 100 && ud.Key == "dashboard");
saved.Should().NotBeNull();
saved.Value.Should().Contain("grid");
```

---

### TC-UD-010: Update Data by Key
**Test**: `SaveUserData_Should_UpdateValue_When_KeyExists`

**Arrange**:
```csharp
var existing = new UserData
{
    Id = 1,
    UserId = 100,
    Key = "dashboard",
    Value = "old value"
};
```

**Act**:
```csharp
await manager.SaveUserDataAsync(100, "dashboard", "new value");
```

**Assert**:
```csharp
var updated = await Context.UserData.FindAsync(1);
updated.Value.Should().Be("new value");
```

---

## 5. Delete User Data Tests

### TC-UD-011: Delete User Data by Key
**Test**: `DeleteUserData_Should_RemoveEntry_When_KeyExists`

**Arrange**:
```csharp
var userData = new UserData
{
    Id = 1,
    UserId = 100,
    Key = "tempData"
};
await Context.UserData.AddAsync(userData);
```

**Act**:
```csharp
await manager.DeleteUserDataAsync(100, "tempData");
```

**Assert**:
```csharp
var deleted = await Context.UserData.FindAsync(1);
deleted.Should().BeNull();
```

---

### TC-UD-012: Delete Non-Existent Key Does Nothing
**Test**: `DeleteUserData_Should_DoNothing_When_KeyDoesNotExist`

**Arrange**:
```csharp
// No data
```

**Act**:
```csharp
await manager.DeleteUserDataAsync(100, "nonExistent");
```

**Assert**:
```csharp
// Should not throw exception
var count = await Context.UserData.CountAsync();
count.Should().Be(0);
```

---

## 6. User Profile Data Tests

### TC-UD-013: Get User Profile Data
**Test**: `GetUserProfile_Should_ReturnProfile_When_DataExists`

**Arrange**:
```csharp
var profileData = new UserData
{
    UserId = 100,
    Key = "profile",
    Value = "{\"displayName\":\"John Doe\",\"avatar\":\"url\"}"
};
```

**Act**:
```csharp
var result = await manager.GetUserProfileAsync(100);
```

**Assert**:
```csharp
result.DisplayName.Should().Be("John Doe");
result.Avatar.Should().Be("url");
```

---

### TC-UD-014: Save User Profile Data
**Test**: `SaveUserProfile_Should_PersistProfile_When_ValidDataProvided`

**Arrange**:
```csharp
var profile = new UserProfile
{
    DisplayName = "Jane Smith",
    Avatar = "avatar.jpg"
};
```

**Act**:
```csharp
await manager.SaveUserProfileAsync(100, profile);
```

**Assert**:
```csharp
var saved = await Context.UserData
    .FirstOrDefaultAsync(ud => ud.UserId == 100 && ud.Key == "profile");
saved.Should().NotBeNull();
saved.Value.Should().Contain("Jane Smith");
```

---

## 7. Multiple Users Tests

### TC-UD-015: Isolate User Data by User ID
**Test**: `GetUserData_Should_FilterByUserId_When_MultipleUsersExist`

**Arrange**:
```csharp
var userData = new List<UserData>
{
    new() { UserId = 100, Key = "pref", Value = "User100Data" },
    new() { UserId = 200, Key = "pref", Value = "User200Data" }
};
await Context.UserData.AddRangeAsync(userData);
```

**Act**:
```csharp
var result100 = await manager.GetUserDataAsync(100, "pref");
var result200 = await manager.GetUserDataAsync(200, "pref");
```

**Assert**:
```csharp
result100.Should().Be("User100Data");
result200.Should().Be("User200Data");
```

---

## Test Data Factory

```csharp
public class UserDataTestDataFactory
{
    public UserData CreateUserData(int userId, string key, string value)
    {
        return new UserData
        {
            UserId = userId,
            Key = key,
            Value = value,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    public UserData CreatePreferences(int userId, string theme = "light", string language = "en")
    {
        return CreateUserData(userId, "preferences", 
            $"{{\"theme\":\"{theme}\",\"language\":\"{language}\"}}");
    }

    public UserData CreateSettings(int userId, bool emailNotif = true, bool smsNotif = false)
    {
        return CreateUserData(userId, "settings",
            $"{{\"emailNotifications\":{emailNotif.ToString().ToLower()},\"smsNotifications\":{smsNotif.ToString().ToLower()}}}");
    }
}
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **User Preferences** | 4 tests | 90% |
| **User Settings** | 2 tests | 90% |
| **Key-Value Operations** | 4 tests | 85% |
| **Delete Operations** | 2 tests | 85% |
| **User Profile** | 2 tests | 85% |
| **Multi-User** | 1 test | 80% |
| **Overall** | **15+ tests** | **85%+** |

---

**End of UserDataManager Unit Test Cases**

