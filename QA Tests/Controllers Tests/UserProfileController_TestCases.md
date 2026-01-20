# UserProfileController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Users/UserProfileController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The UserProfileController manages user profiles:
- Profile CRUD operations
- Avatar/photo management
- Profile preferences
- User information display

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Profile CRUD | 8 | P0 |
| Avatar Management | 5 | P1 |
| Preferences | 6 | P1 |
| Authorization | 6 | P0 |

---

## P0 - Critical Tests

### TC-UP-001: Get current user profile
**Description**: Get authenticated user's profile  
**Test Steps**:
1. Call `GET /api/users/profile`
**Expected Result**: Current user profile

### TC-UP-002: Get user profile by ID
**Description**: Get specific user profile  
**Test Steps**:
1. Call `GET /api/users/{userId}/profile`
**Expected Result**: User profile

### TC-UP-003: Update own profile
**Description**: Update current user profile  
**Test Steps**:
1. Call `PUT /api/users/profile` with data
**Expected Result**: Profile updated

### TC-UP-004: Update profile - validation
**Description**: Validate profile data  
**Test Steps**:
1. Update with invalid email
**Expected Result**: Validation error

### TC-UP-005: Cannot update other user's profile
**Description**: Permission check  
**Test Steps**:
1. Try update another user's profile
**Expected Result**: 403 Forbidden

### TC-UP-006: Admin can update any profile
**Description**: Admin override  
**Test Steps**:
1. Admin updates user profile
**Expected Result**: Profile updated

### TC-UP-007: Get profile includes org unit
**Description**: Org unit in profile  
**Test Steps**:
1. Get profile
2. Verify org unit data
**Expected Result**: Org unit included

### TC-UP-008: Get profile includes roles
**Description**: Roles in profile  
**Test Steps**:
1. Get profile
2. Verify roles data
**Expected Result**: Roles included

---

## P1 - High Priority Tests

### TC-UP-009: Upload avatar
**Description**: Upload profile picture  
**Test Steps**:
1. Call `POST /api/users/profile/avatar` with image
**Expected Result**: Avatar uploaded

### TC-UP-010: Get avatar
**Description**: Retrieve avatar image  
**Test Steps**:
1. Upload avatar
2. Call `GET /api/users/{userId}/avatar`
**Expected Result**: Image returned

### TC-UP-011: Delete avatar
**Description**: Remove avatar  
**Test Steps**:
1. Upload avatar
2. Call `DELETE /api/users/profile/avatar`
**Expected Result**: Avatar removed

### TC-UP-012: Avatar size limit
**Description**: Enforce max size  
**Test Steps**:
1. Upload 10MB image
**Expected Result**: Size exceeded error

### TC-UP-013: Avatar format validation
**Description**: Only images allowed  
**Test Steps**:
1. Upload non-image file
**Expected Result**: Invalid format error

### TC-UP-014: Update notification preferences
**Description**: Update notification settings  
**Test Steps**:
1. Call `PUT /api/users/profile/notifications`
**Expected Result**: Preferences updated

### TC-UP-015: Update display preferences
**Description**: Update UI preferences  
**Test Steps**:
1. Call `PUT /api/users/profile/display`
**Expected Result**: Preferences updated

### TC-UP-016: Update language preference
**Description**: Set preferred language  
**Test Steps**:
1. Set language = "fr"
**Expected Result**: Language updated

### TC-UP-017: Update timezone preference
**Description**: Set timezone  
**Test Steps**:
1. Set timezone = "Africa/Nairobi"
**Expected Result**: Timezone updated

### TC-UP-018: Get activity history
**Description**: Get user's activity  
**Test Steps**:
1. Call `GET /api/users/profile/activity`
**Expected Result**: Activity log

### TC-UP-019: Get login history
**Description**: Get login sessions  
**Test Steps**:
1. Call `GET /api/users/profile/sessions`
**Expected Result**: Session history

---

## Authorization Tests

### TC-UP-A001: Unauthenticated denied
**Expected Result**: 401 Unauthorized

### TC-UP-A002: View own profile
**Expected Result**: 200 OK

### TC-UP-A003: View other profile (permitted)
**Expected Result**: 200 OK if permitted

### TC-UP-A004: View other profile (denied)
**Expected Result**: 403 if no permission

### TC-UP-A005: Admin view any profile
**Expected Result**: 200 OK

### TC-UP-A006: Edit own profile only
**Expected Result**: Only own profile editable

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/UserProfileControllerTests.cs`

