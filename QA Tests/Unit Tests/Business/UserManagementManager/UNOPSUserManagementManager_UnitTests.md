# UNOPSUserManagementManager - Unit Test Cases

**Manager**: `UNOPSUserManagementManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSUserManagementManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: User management, Role assignment, Permission management, User lifecycle

**Total Test Cases**: 20+

---

## Test Categories

### 1. User CRUD (5 tests)
- TC-UM-001: Create user
- TC-UM-002: Get user
- TC-UM-003: Update user
- TC-UM-004: Deactivate user
- TC-UM-005: Reactivate user

### 2. Role Management (5 tests)
- TC-UM-006: Assign role
- TC-UM-007: Remove role
- TC-UM-008: Get user roles
- TC-UM-009: Update roles
- TC-UM-010: Role validation

### 3. Permission Management (4 tests)
- TC-UM-011: Grant permission
- TC-UM-012: Revoke permission
- TC-UM-013: Check permission
- TC-UM-014: Get user permissions

### 4. User Search & Filter (3 tests)
- TC-UM-015: Search users
- TC-UM-016: Filter by role
- TC-UM-017: Filter by status

### 5. User Lifecycle (3 tests)
- TC-UM-018: Invite user
- TC-UM-019: User activation
- TC-UM-020: Account lockout

**Coverage**: 70%+

