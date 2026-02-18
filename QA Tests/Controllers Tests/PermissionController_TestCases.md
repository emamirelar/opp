# PermissionController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Admin/PermissionController.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 35  

---

## Overview

The PermissionController manages the permission system:
- Permission CRUD operations
- Role-permission assignments
- Entity-level permissions
- Permission checking and validation
- Bulk permission operations

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Permission CRUD | 10 | P0 |
| Role-Permission Assignment | 8 | P0 |
| Permission Checking | 7 | P0 |
| Bulk Operations | 5 | P1 |
| Authorization | 5 | P0 |

---

## P0 - Critical Tests

### TC-PERM-001: Get all permissions
**Description**: Retrieve list of all system permissions  
**Preconditions**: User has admin access  
**Test Steps**:
1. Authenticate as admin
2. Call `GET /api/admin/permissions`
3. Verify all permissions returned
**Expected Result**: Complete list of system permissions

### TC-PERM-002: Get permission by ID
**Description**: Retrieve specific permission  
**Preconditions**: Permission exists  
**Test Steps**:
1. Call `GET /api/admin/permissions/{id}`
2. Verify correct permission returned
**Expected Result**: Permission with matching ID

### TC-PERM-003: Create new permission
**Description**: Create a new permission  
**Preconditions**: User has permission management access  
**Test Steps**:
1. Call `POST /api/admin/permissions` with permission data
2. Verify permission created
3. Verify permission retrievable
**Expected Result**: Permission created with generated ID

### TC-PERM-004: Create permission - duplicate name fails
**Description**: Cannot create permission with existing name  
**Preconditions**: Permission with name exists  
**Test Steps**:
1. Create permission "CanViewPartners"
2. Attempt to create another "CanViewPartners"
3. Verify duplicate error
**Expected Result**: 409 Conflict with duplicate message

### TC-PERM-005: Update permission
**Description**: Update existing permission  
**Preconditions**: Permission exists  
**Test Steps**:
1. Call `PUT /api/admin/permissions/{id}` with updated data
2. Verify permission updated
**Expected Result**: Permission updated successfully

### TC-PERM-006: Delete permission
**Description**: Delete unused permission  
**Preconditions**: Permission exists and not assigned  
**Test Steps**:
1. Call `DELETE /api/admin/permissions/{id}`
2. Verify permission deleted
**Expected Result**: Permission removed from system

### TC-PERM-007: Delete permission in use fails
**Description**: Cannot delete permission assigned to roles  
**Preconditions**: Permission assigned to one or more roles  
**Test Steps**:
1. Assign permission to role
2. Attempt to delete permission
3. Verify deletion prevented
**Expected Result**: 400 Bad Request - permission in use

### TC-PERM-008: Assign permission to role
**Description**: Assign a permission to a role  
**Preconditions**: Permission and role exist  
**Test Steps**:
1. Call `POST /api/admin/roles/{roleId}/permissions/{permissionId}`
2. Verify assignment
**Expected Result**: Permission assigned to role

### TC-PERM-009: Remove permission from role
**Description**: Remove a permission from a role  
**Preconditions**: Permission assigned to role  
**Test Steps**:
1. Call `DELETE /api/admin/roles/{roleId}/permissions/{permissionId}`
2. Verify removal
**Expected Result**: Permission removed from role

### TC-PERM-010: Get role permissions
**Description**: Get all permissions for a role  
**Preconditions**: Role has permissions assigned  
**Test Steps**:
1. Assign multiple permissions to role
2. Call `GET /api/admin/roles/{roleId}/permissions`
3. Verify all assigned permissions returned
**Expected Result**: List of all role permissions

### TC-PERM-011: Check user has permission
**Description**: Check if current user has specific permission  
**Preconditions**: User authenticated  
**Test Steps**:
1. Call `GET /api/permissions/check?permission=CanViewPartners`
2. Verify boolean response
**Expected Result**: true if user has permission, false otherwise

### TC-PERM-012: Get user effective permissions
**Description**: Get all effective permissions for user  
**Preconditions**: User has role(s) assigned  
**Test Steps**:
1. Assign user to roles with various permissions
2. Call `GET /api/permissions/my-permissions`
3. Verify all effective permissions returned
**Expected Result**: Combined permissions from all user roles

### TC-PERM-013: Get permissions by category
**Description**: Get permissions filtered by category  
**Preconditions**: Permissions with categories exist  
**Test Steps**:
1. Call `GET /api/admin/permissions?category=Partners`
2. Verify only partner permissions returned
**Expected Result**: Only permissions in Partners category

### TC-PERM-014: Check entity-level permission
**Description**: Check permission for specific entity  
**Preconditions**: Entity-level permissions configured  
**Test Steps**:
1. Configure entity permission for partner ID 123
2. Call permission check for that partner
3. Verify correct response
**Expected Result**: Permission status for specific entity

### TC-PERM-015: Permission inheritance from parent role
**Description**: Child role inherits parent permissions  
**Preconditions**: Role hierarchy exists  
**Test Steps**:
1. Create parent role with permissions
2. Create child role inheriting from parent
3. Check child role has parent permissions
**Expected Result**: Child has all parent permissions

---

## P1 - High Priority Tests

### TC-PERM-016: Bulk assign permissions to role
**Description**: Assign multiple permissions at once  
**Preconditions**: Role and permissions exist  
**Test Steps**:
1. Call `POST /api/admin/roles/{roleId}/permissions/bulk` with permission IDs
2. Verify all assigned
**Expected Result**: All permissions assigned in single operation

### TC-PERM-017: Bulk remove permissions from role
**Description**: Remove multiple permissions at once  
**Preconditions**: Role has multiple permissions  
**Test Steps**:
1. Call `DELETE /api/admin/roles/{roleId}/permissions/bulk` with permission IDs
2. Verify all removed
**Expected Result**: All specified permissions removed

### TC-PERM-018: Copy permissions between roles
**Description**: Copy all permissions from one role to another  
**Preconditions**: Source role has permissions  
**Test Steps**:
1. Call `POST /api/admin/roles/{targetRoleId}/copy-permissions/{sourceRoleId}`
2. Verify permissions copied
**Expected Result**: Target role has all source role permissions

### TC-PERM-019: Get permission usage report
**Description**: Report of which roles use which permissions  
**Preconditions**: Permissions assigned to roles  
**Test Steps**:
1. Call `GET /api/admin/permissions/{id}/usage`
2. Verify usage data
**Expected Result**: List of roles using the permission

### TC-PERM-020: Permission audit log
**Description**: Retrieve permission change history  
**Preconditions**: Permission changes have occurred  
**Test Steps**:
1. Make permission changes
2. Call `GET /api/admin/permissions/{id}/audit`
3. Verify audit entries
**Expected Result**: Complete audit trail of changes

---

## Authorization Tests

### TC-PERM-A001: Non-admin cannot manage permissions
**Description**: Regular user cannot access permission management  
**Preconditions**: User is not admin  
**Test Steps**:
1. Authenticate as regular user
2. Call permission management endpoints
3. Verify access denied
**Expected Result**: 403 Forbidden

### TC-PERM-A002: Admin can manage permissions
**Description**: Admin user can access permission management  
**Preconditions**: User has admin role  
**Test Steps**:
1. Authenticate as admin
2. Call permission management endpoints
3. Verify access granted
**Expected Result**: 200 OK

### TC-PERM-A003: Cannot elevate own permissions
**Description**: User cannot grant themselves higher permissions  
**Preconditions**: User can manage permissions but lacks target permission  
**Test Steps**:
1. Attempt to assign self a permission user lacks
2. Verify prevented
**Expected Result**: 403 Forbidden - cannot self-elevate

### TC-PERM-A004: System permissions are read-only
**Description**: Core system permissions cannot be modified  
**Preconditions**: System permission exists  
**Test Steps**:
1. Attempt to delete system permission
2. Verify prevented
**Expected Result**: 400 Bad Request - system permission protected

### TC-PERM-A005: Permission check caching
**Description**: Permission checks are cached for performance  
**Preconditions**: Caching enabled  
**Test Steps**:
1. Check permission (populates cache)
2. Check same permission again
3. Verify cached result used
**Expected Result**: Second check significantly faster

---

## Validation Tests

### TC-PERM-V001: Permission name validation
**Description**: Permission name must follow naming convention  
**Test Steps**:
1. Attempt to create permission with invalid name
2. Verify validation error
**Expected Result**: 400 Bad Request with validation message

### TC-PERM-V002: Permission description required
**Description**: Permissions must have description  
**Test Steps**:
1. Attempt to create permission without description
2. Verify validation error
**Expected Result**: 400 Bad Request

### TC-PERM-V003: Category must exist
**Description**: Permission category must be valid  
**Test Steps**:
1. Attempt to create permission with invalid category
2. Verify validation error
**Expected Result**: 400 Bad Request - invalid category

---

## Performance Tests

### TC-PERM-P001: Permission check performance
**Description**: Permission check completes quickly  
**Performance Criteria**: < 50ms

### TC-PERM-P002: Get all permissions performance
**Description**: List all permissions quickly  
**Preconditions**: 500 permissions exist  
**Performance Criteria**: < 500ms

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/PermissionControllerTests.cs`

