# PartnerGroupController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/PartnerTrees/PartnerGroupController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 22  

---

## Overview

The PartnerGroupController manages partner groups:
- Group CRUD operations
- Group membership management
- Group-based permissions
- Group reporting

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| CRUD Operations | 8 | P0 |
| Membership | 8 | P1 |
| Permissions | 3 | P0 |
| Authorization | 3 | P0 |

---

## P0 - Critical Tests

### TC-PGC-001: Get all groups
**Description**: List all groups  
**Test Steps**:
1. Call `GET /api/partner-groups`
**Expected Result**: Group list

### TC-PGC-002: Get group by ID
**Description**: Retrieve specific group  
**Test Steps**:
1. Call `GET /api/partner-groups/{id}`
**Expected Result**: Group details

### TC-PGC-003: Create group
**Description**: Create new group  
**Test Steps**:
1. Call `POST /api/partner-groups`
**Expected Result**: Group created

### TC-PGC-004: Update group
**Description**: Update existing group  
**Test Steps**:
1. Call `PUT /api/partner-groups/{id}`
**Expected Result**: Group updated

### TC-PGC-005: Delete group
**Description**: Remove group  
**Test Steps**:
1. Call `DELETE /api/partner-groups/{id}`
**Expected Result**: Group deleted

### TC-PGC-006: Get by ID - not found
**Description**: Non-existing ID  
**Test Steps**:
1. Call with invalid ID
**Expected Result**: 404 Not Found

### TC-PGC-007: Create - validation
**Description**: Validate required fields  
**Test Steps**:
1. Submit without name
**Expected Result**: Validation error

### TC-PGC-008: Duplicate name prevented
**Description**: Unique name required  
**Test Steps**:
1. Create with existing name
**Expected Result**: Conflict error

---

## P1 - High Priority Tests

### TC-PGC-009: Get group members
**Description**: List partners in group  
**Test Steps**:
1. Call `GET /api/partner-groups/{id}/members`
**Expected Result**: Partner list

### TC-PGC-010: Add member
**Description**: Add partner to group  
**Test Steps**:
1. Call `POST /api/partner-groups/{id}/members/{partnerId}`
**Expected Result**: Member added

### TC-PGC-011: Remove member
**Description**: Remove partner from group  
**Test Steps**:
1. Call `DELETE /api/partner-groups/{id}/members/{partnerId}`
**Expected Result**: Member removed

### TC-PGC-012: Add multiple members
**Description**: Bulk add  
**Test Steps**:
1. Call `POST /api/partner-groups/{id}/members/bulk`
**Expected Result**: Multiple added

### TC-PGC-013: Remove multiple members
**Description**: Bulk remove  
**Test Steps**:
1. Call `DELETE /api/partner-groups/{id}/members/bulk`
**Expected Result**: Multiple removed

### TC-PGC-014: Get member count
**Description**: Count members  
**Test Steps**:
1. Call `GET /api/partner-groups/{id}/members/count`
**Expected Result**: Count returned

### TC-PGC-015: Check membership
**Description**: Is partner in group  
**Test Steps**:
1. Call `GET /api/partner-groups/{id}/members/{partnerId}/check`
**Expected Result**: Boolean result

### TC-PGC-016: Get partner's groups
**Description**: Groups containing partner  
**Test Steps**:
1. Call `GET /api/partners/{id}/groups`
**Expected Result**: Group list

### TC-PGC-017: Get for dropdown
**Description**: Simplified list  
**Test Steps**:
1. Call `GET /api/partner-groups/dropdown`
**Expected Result**: ID/name pairs

### TC-PGC-018: Search groups
**Description**: Search by name  
**Test Steps**:
1. Call with search parameter
**Expected Result**: Matching groups

### TC-PGC-019: Filter by type
**Description**: Filter by group type  
**Test Steps**:
1. Call with type parameter
**Expected Result**: Filtered groups

---

## Authorization Tests

### TC-PGC-A001: Read requires auth
**Expected Result**: 401 without auth

### TC-PGC-A002: Write requires admin
**Expected Result**: 403 for non-admin

### TC-PGC-A003: Member management permissions
**Expected Result**: Based on role

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/PartnerGroupControllerTests.cs`

