# UserPreferenceController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Users/UserPreferenceController.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 18  

---

## Overview

The UserPreferenceController manages user preferences:
- Display settings
- Notification preferences
- UI customization
- Default values

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Preference CRUD | 6 | P0 |
| Display Settings | 5 | P1 |
| Notifications | 4 | P1 |
| Defaults | 3 | P1 |

---

## P0 - Critical Tests

### TC-UPREF-001: Get all preferences
**Description**: Get user's preferences  
**Test Steps**:
1. Call `GET /api/users/preferences`
**Expected Result**: All preferences

### TC-UPREF-002: Get preference by key
**Description**: Get specific preference  
**Test Steps**:
1. Call `GET /api/users/preferences/{key}`
**Expected Result**: Preference value

### TC-UPREF-003: Set preference
**Description**: Set preference value  
**Test Steps**:
1. Call `PUT /api/users/preferences/{key}`
**Expected Result**: Preference saved

### TC-UPREF-004: Delete preference
**Description**: Remove preference  
**Test Steps**:
1. Call `DELETE /api/users/preferences/{key}`
**Expected Result**: Preference deleted

### TC-UPREF-005: Bulk update preferences
**Description**: Update multiple  
**Test Steps**:
1. Call `PUT /api/users/preferences`
**Expected Result**: All updated

### TC-UPREF-006: Reset to defaults
**Description**: Reset all preferences  
**Test Steps**:
1. Call `POST /api/users/preferences/reset`
**Expected Result**: Defaults restored

---

## P1 - High Priority Tests

### TC-UPREF-007: Set language preference
**Description**: Set preferred language  
**Test Steps**:
1. Set `language` = "fr"
**Expected Result**: French preferred

### TC-UPREF-008: Set theme preference
**Description**: Set UI theme  
**Test Steps**:
1. Set `theme` = "dark"
**Expected Result**: Dark theme set

### TC-UPREF-009: Set date format
**Description**: Set date format  
**Test Steps**:
1. Set `dateFormat` = "DD/MM/YYYY"
**Expected Result**: Format applied

### TC-UPREF-010: Set timezone
**Description**: Set user timezone  
**Test Steps**:
1. Set `timezone` = "Africa/Nairobi"
**Expected Result**: Timezone set

### TC-UPREF-011: Set page size
**Description**: Default page size  
**Test Steps**:
1. Set `pageSize` = 50
**Expected Result**: Lists show 50 items

### TC-UPREF-012: Email notification toggle
**Description**: Enable/disable email  
**Test Steps**:
1. Set `emailNotifications` = false
**Expected Result**: Emails disabled

### TC-UPREF-013: In-app notification toggle
**Description**: Enable/disable in-app  
**Test Steps**:
1. Set `inAppNotifications` = true
**Expected Result**: In-app enabled

### TC-UPREF-014: Notification frequency
**Description**: Set digest frequency  
**Test Steps**:
1. Set `notificationFrequency` = "daily"
**Expected Result**: Daily digest

### TC-UPREF-015: Default list view
**Description**: Set default view  
**Test Steps**:
1. Set `defaultView` = "table"
**Expected Result**: Table view default

### TC-UPREF-016: Set default dashboard
**Description**: Choose dashboard  
**Test Steps**:
1. Set `defaultDashboard` = "partnership"
**Expected Result**: Dashboard selected

### TC-UPREF-017: Set default org unit
**Description**: Default filter org  
**Test Steps**:
1. Set `defaultOrgUnit` = 123
**Expected Result**: Org unit default

### TC-UPREF-018: Preference validation
**Description**: Invalid value rejected  
**Test Steps**:
1. Set invalid preference value
**Expected Result**: Validation error

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/UserPreferenceControllerTests.cs`

