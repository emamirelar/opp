# SavedFilterController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/SavedFilterController.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 18  

---

## Overview

The SavedFilterController exposes saved filter APIs:
- Filter management endpoints
- Sharing endpoints
- Default filter handling

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD | 6 | P0 |
| Sharing | 5 | P1 |
| Defaults | 4 | P1 |
| Authorization | 3 | P0 |

---

## P0 - Critical Tests

### TC-SFC-001: Get user's saved filters
**Description**: List own filters  
**Test Steps**:
1. Call `GET /api/saved-filters`
**Expected Result**: User's filters

### TC-SFC-002: Get filter by ID
**Description**: Get specific filter  
**Test Steps**:
1. Call `GET /api/saved-filters/{id}`
**Expected Result**: Filter details

### TC-SFC-003: Create saved filter
**Description**: Create new filter  
**Test Steps**:
1. Call `POST /api/saved-filters`
**Expected Result**: Filter created

### TC-SFC-004: Update saved filter
**Description**: Update filter  
**Test Steps**:
1. Call `PUT /api/saved-filters/{id}`
**Expected Result**: Filter updated

### TC-SFC-005: Delete saved filter
**Description**: Delete filter  
**Test Steps**:
1. Call `DELETE /api/saved-filters/{id}`
**Expected Result**: Filter deleted

### TC-SFC-006: Get filters by entity type
**Description**: Filter by entity  
**Test Steps**:
1. Call `GET /api/saved-filters?entityType=Partner`
**Expected Result**: Partner filters

---

## P1 - High Priority Tests

### TC-SFC-007: Share filter with user
**Description**: Share filter  
**Test Steps**:
1. Call `POST /api/saved-filters/{id}/share/user/{userId}`
**Expected Result**: Filter shared

### TC-SFC-008: Share filter with role
**Description**: Share with role  
**Test Steps**:
1. Call `POST /api/saved-filters/{id}/share/role/{roleId}`
**Expected Result**: Filter shared with role

### TC-SFC-009: Get shared filters
**Description**: Filters shared with me  
**Test Steps**:
1. Call `GET /api/saved-filters/shared`
**Expected Result**: Shared filters

### TC-SFC-010: Remove share
**Description**: Unshare filter  
**Test Steps**:
1. Call `DELETE /api/saved-filters/{id}/share/user/{userId}`
**Expected Result**: Share removed

### TC-SFC-011: Set as default
**Description**: Make filter default  
**Test Steps**:
1. Call `POST /api/saved-filters/{id}/default`
**Expected Result**: Set as default

### TC-SFC-012: Get default filter
**Description**: Get entity's default  
**Test Steps**:
1. Call `GET /api/saved-filters/default?entityType=Partner`
**Expected Result**: Default filter

### TC-SFC-013: Clear default
**Description**: Remove default  
**Test Steps**:
1. Call `DELETE /api/saved-filters/default?entityType=Partner`
**Expected Result**: Default cleared

### TC-SFC-014: Duplicate filter
**Description**: Clone filter  
**Test Steps**:
1. Call `POST /api/saved-filters/{id}/duplicate`
**Expected Result**: New filter

### TC-SFC-015: Export filter
**Description**: Export JSON  
**Test Steps**:
1. Call `GET /api/saved-filters/{id}/export`
**Expected Result**: JSON export

---

## Authorization Tests

### TC-SFC-A001: Own filter access
**Expected Result**: Full access

### TC-SFC-A002: Shared filter read-only
**Expected Result**: Can read, cannot edit

### TC-SFC-A003: Cannot access other's private
**Expected Result**: 404 or 403

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/SavedFilterControllerTests.cs`

