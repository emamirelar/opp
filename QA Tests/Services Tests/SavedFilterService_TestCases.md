# SavedFilterService Test Cases

**Service**: `UNOPS.PAO.Business/Services/SavedFilterService.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 20  

---

## Overview

The SavedFilterService manages user-defined saved filters:
- Filter CRUD operations
- User-specific filter storage
- Filter sharing and permissions
- Default filter management

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| User Preferences | 5 | P1 |
| Sharing | 4 | P1 |
| Validation | 3 | P2 |

---

## P0 - Critical Tests

### TC-SF-001: Create saved filter
**Description**: Create new saved filter  
**Test Steps**:
1. Call `CreateAsync(filterData)`
2. Verify filter created
**Expected Result**: Filter with generated ID

### TC-SF-002: Get saved filter by ID
**Description**: Retrieve filter by ID  
**Test Steps**:
1. Create filter
2. Call `GetByIdAsync(id)`
**Expected Result**: Correct filter returned

### TC-SF-003: Update saved filter
**Description**: Update existing filter  
**Test Steps**:
1. Create filter
2. Call `UpdateAsync(id, updatedData)`
**Expected Result**: Filter updated

### TC-SF-004: Delete saved filter
**Description**: Delete filter  
**Test Steps**:
1. Create filter
2. Call `DeleteAsync(id)`
**Expected Result**: Filter removed

### TC-SF-005: Get user's saved filters
**Description**: Get all filters for current user  
**Test Steps**:
1. Create multiple filters for user
2. Call `GetByUserAsync(userId)`
**Expected Result**: All user's filters returned

### TC-SF-006: Get filters by entity type
**Description**: Get filters for specific entity  
**Test Steps**:
1. Create filters for different entities
2. Call `GetByEntityTypeAsync("Partner")`
**Expected Result**: Only Partner filters

### TC-SF-007: Set default filter
**Description**: Mark filter as default  
**Test Steps**:
1. Create filter
2. Call `SetDefaultAsync(filterId)`
**Expected Result**: Filter marked as default

### TC-SF-008: Get default filter
**Description**: Retrieve user's default filter  
**Test Steps**:
1. Set default filter
2. Call `GetDefaultAsync(userId, entityType)`
**Expected Result**: Default filter returned

---

## P1 - High Priority Tests

### TC-SF-009: Share filter with user
**Description**: Share filter with another user  
**Test Steps**:
1. Create filter
2. Call `ShareWithUserAsync(filterId, targetUserId)`
**Expected Result**: Filter shared

### TC-SF-010: Share filter with role
**Description**: Share filter with role  
**Test Steps**:
1. Create filter
2. Call `ShareWithRoleAsync(filterId, roleId)`
**Expected Result**: Filter visible to role

### TC-SF-011: Get shared filters
**Description**: Get filters shared with user  
**Test Steps**:
1. Share filter with user
2. Call `GetSharedWithMeAsync(userId)`
**Expected Result**: Shared filters returned

### TC-SF-012: Unshare filter
**Description**: Remove sharing  
**Test Steps**:
1. Share filter
2. Call `UnshareAsync(filterId, targetUserId)`
**Expected Result**: Sharing removed

### TC-SF-013: Duplicate filter
**Description**: Clone existing filter  
**Test Steps**:
1. Create filter
2. Call `DuplicateAsync(filterId)`
**Expected Result**: New filter created

### TC-SF-014: Apply filter
**Description**: Apply filter to query  
**Test Steps**:
1. Create filter with criteria
2. Call `ApplyFilterAsync(query, filterId)`
**Expected Result**: Query filtered

### TC-SF-015: Filter name unique per user
**Description**: Prevent duplicate names  
**Test Steps**:
1. Create filter with name
2. Try create another with same name
**Expected Result**: Conflict error

### TC-SF-016: Clear default filter
**Description**: Remove default status  
**Test Steps**:
1. Set default filter
2. Call `ClearDefaultAsync(userId, entityType)`
**Expected Result**: No default set

### TC-SF-017: Export filter
**Description**: Export filter to JSON  
**Test Steps**:
1. Call `ExportAsync(filterId)`
**Expected Result**: JSON export

---

## Validation Tests

### TC-SF-V001: Filter name required
**Description**: Name cannot be empty

### TC-SF-V002: Filter criteria required
**Description**: Must have at least one criterion

### TC-SF-V003: Entity type required
**Description**: Must specify entity type

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/SavedFilterServiceTests.cs`

