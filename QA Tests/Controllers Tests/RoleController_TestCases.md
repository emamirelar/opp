# RoleController Test Cases

**Controller**: `UNOPS.PAO.UNOPSPresentation/Controllers/RoleController.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 25  

---

## Overview

The RoleController manages user roles and role assignments:
- Role CRUD operations
- User-role assignments
- Role hierarchy management
- Role-based access configuration

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Role CRUD | 8 | P0 |
| User-Role Assignment | 7 | P0 |
| Role Hierarchy | 5 | P1 |
| Authorization | 5 | P0 |

---

## P0 - Critical Tests

### TC-ROLE-001: Get all roles
**Description**: Retrieve list of all system roles  
**Preconditions**: User has admin access  
**Test Steps**:
1. Authenticate as admin
2. Call `GET /api/admin/roles`
3. Verify all roles returned
**Expected Result**: Complete list of roles with names, descriptions

### TC-ROLE-002: Get role by ID
**Description**: Retrieve specific role details  
**Preconditions**: Role exists  
**Test Steps**:
1. Call `GET /api/admin/roles/{id}`
2. Verify role details returned
**Expected Result**: Role with permissions and user count

### TC-ROLE-003: Create new role
**Description**: Create a new role  
**Preconditions**: User has role management permission  
**Test Steps**:
1. Call `POST /api/admin/roles` with role data
2. Verify role created
**Expected Result**: Role created with generated ID

### TC-ROLE-004: Create role - duplicate name fails
**Description**: Cannot create role with existing name  
**Preconditions**: Role with name exists  
**Test Steps**:
1. Attempt to create role with existing name
2. Verify duplicate error
**Expected Result**: 409 Conflict

### TC-ROLE-005: Update role
**Description**: Update existing role  
**Preconditions**: Role exists  
**Test Steps**:
1. Call `PUT /api/admin/roles/{id}`
2. Verify role updated
**Expected Result**: Role updated successfully

### TC-ROLE-006: Delete role
**Description**: Delete unused role  
**Preconditions**: Role exists and has no users  
**Test Steps**:
1. Call `DELETE /api/admin/roles/{id}`
2. Verify role deleted
**Expected Result**: Role removed from system

### TC-ROLE-007: Delete role with users fails
**Description**: Cannot delete role with assigned users  
**Preconditions**: Role has users assigned  
**Test Steps**:
1. Attempt to delete role with users
2. Verify deletion prevented
**Expected Result**: 400 Bad Request - role in use

### TC-ROLE-008: Assign role to user
**Description**: Assign a role to user  
**Preconditions**: Role and user exist  
**Test Steps**:
1. Call `POST /api/admin/users/{userId}/roles/{roleId}`
2. Verify assignment
**Expected Result**: User has role assigned

### TC-ROLE-009: Remove role from user
**Description**: Remove role assignment  
**Preconditions**: User has role  
**Test Steps**:
1. Call `DELETE /api/admin/users/{userId}/roles/{roleId}`
2. Verify removal
**Expected Result**: Role removed from user

### TC-ROLE-010: Get user roles
**Description**: Get all roles for user  
**Preconditions**: User has roles  
**Test Steps**:
1. Call `GET /api/admin/users/{userId}/roles`
2. Verify roles returned
**Expected Result**: All user roles listed

### TC-ROLE-011: Get role users
**Description**: Get all users with specific role  
**Preconditions**: Role has users  
**Test Steps**:
1. Call `GET /api/admin/roles/{roleId}/users`
2. Verify users returned
**Expected Result**: All users with role listed

### TC-ROLE-012: Assign multiple roles to user
**Description**: Assign multiple roles at once  
**Preconditions**: Roles and user exist  
**Test Steps**:
1. Call `POST /api/admin/users/{userId}/roles/bulk`
2. Verify all assignments
**Expected Result**: All roles assigned

---

## P1 - High Priority Tests

### TC-ROLE-013: Get role hierarchy
**Description**: Get parent-child role relationships  
**Preconditions**: Role hierarchy exists  
**Test Steps**:
1. Call `GET /api/admin/roles/{roleId}/hierarchy`
2. Verify hierarchy data
**Expected Result**: Parent and child roles returned

### TC-ROLE-014: Create child role
**Description**: Create role that inherits from parent  
**Preconditions**: Parent role exists  
**Test Steps**:
1. Create role with parentRoleId
2. Verify inheritance
**Expected Result**: Child role inherits parent permissions

### TC-ROLE-015: Prevent circular role hierarchy
**Description**: Cannot create circular parent relationships  
**Preconditions**: Role hierarchy exists  
**Test Steps**:
1. Attempt to set A as parent of B when B is parent of A
2. Verify error
**Expected Result**: 400 Bad Request - circular reference

### TC-ROLE-016: Clone role
**Description**: Clone existing role with new name  
**Preconditions**: Source role exists  
**Test Steps**:
1. Call `POST /api/admin/roles/{roleId}/clone`
2. Verify clone created
**Expected Result**: New role with same permissions

### TC-ROLE-017: Get role audit history
**Description**: View role change history  
**Preconditions**: Role has been modified  
**Test Steps**:
1. Call `GET /api/admin/roles/{roleId}/audit`
2. Verify audit entries
**Expected Result**: Complete change history

---

## Authorization Tests

### TC-ROLE-A001: Non-admin cannot manage roles
**Description**: Regular user cannot access role management  
**Preconditions**: User is not admin  
**Test Steps**:
1. Authenticate as regular user
2. Call role endpoints
3. Verify access denied
**Expected Result**: 403 Forbidden

### TC-ROLE-A002: Cannot delete system roles
**Description**: Core system roles protected  
**Preconditions**: System role exists  
**Test Steps**:
1. Attempt to delete system role
2. Verify prevented
**Expected Result**: 400 Bad Request - protected role

### TC-ROLE-A003: Cannot self-assign admin role
**Description**: User cannot grant themselves admin  
**Preconditions**: User can manage roles  
**Test Steps**:
1. Attempt to assign admin role to self
2. Verify prevented
**Expected Result**: 403 Forbidden

### TC-ROLE-A004: Role scope limits user visibility
**Description**: Role scope affects which users can be seen  
**Preconditions**: Role has org unit scope  
**Test Steps**:
1. Query role users with scope
2. Verify scoped results
**Expected Result**: Only scoped users visible

### TC-ROLE-A005: Validate role can be assigned
**Description**: Check if role can be assigned to user  
**Preconditions**: Various constraints  
**Test Steps**:
1. Call validation endpoint
2. Verify response
**Expected Result**: Clear yes/no with reason

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/RoleControllerTests.cs`

