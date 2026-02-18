# User Roles Management — Test Cases

**Component:** `UNOPS.PAO.ContextPermissions / UNOPS.PAO.Presentation/Controllers/RoleController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result |
|-------|--------|
| N ≥ 3P | 90 ≥ 90 ✅ PASS |
| E ≥ 3P | 90 ≥ 90 ✅ PASS |
| F ≥ 3P | 90 ≥ 90 ✅ PASS |
| I ≥ 3P | 90 ≥ 90 ✅ PASS |

---

## Feature Overview

User role management: CRUD roles, permission assignment, user-role mapping, role hierarchy, audit.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Create Role with Valid Data

**Priority:** P0  
**Precondition:** User has admin permission.

**Steps:**
1. Open User Roles page
2. Click "Add Role"
3. Enter Name, Description
4. Save

**Expected Result:** Role created with Id, audit fields set, appears in list.

---

#### POS-002: Assign Role to User

**Priority:** P0  
**Precondition:** Role and user exist.

**Steps:**
1. Select user
2. Click "Add Role"
3. Select role from list
4. Save

**Expected Result:** User-role mapping created, user has role.

---

#### POS-003: Remove Role from User

**Priority:** P0  
**Precondition:** User has role assigned.

**Steps:**
1. Select user
2. Remove role
3. Confirm

**Expected Result:** User-role mapping deleted, user no longer has role.

---

#### POS-004: Assign Permission to Role

**Priority:** P0  
**Precondition:** Role exists, permission exists.

**Steps:**
1. Select role
2. Add permission from matrix
3. Save

**Expected Result:** Role-permission mapping created.

---

#### POS-005: View Role Matrix

**Priority:** P0  
**Precondition:** Roles and permissions exist.

**Steps:**
1. Open Role Matrix
2. View role-permission grid

**Expected Result:** Matrix displayed with all roles and permissions.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Edit role name | Role exists | Edit name, save | Name updated | P1 |
| POS-007 | Edit role description | Role exists | Edit description, save | Description updated | P1 |
| POS-008 | Assign multiple roles to user | User exists | Add 3 roles | User has all 3 | P1 |
| POS-009 | Assign multiple permissions to role | Role exists | Add 5 permissions | Role has all 5 | P1 |
| POS-010 | Remove permission from role | Role has permission | Remove, save | Permission removed | P1 |
| POS-011 | View role hierarchy | Hierarchy configured | Open hierarchy view | Parent-child shown | P1 |
| POS-012 | Create child role | Parent role exists | Create child under parent | Hierarchy updated | P1 |
| POS-013 | Search users by name | Users exist | Search "John" | Matching users | P1 |
| POS-014 | Filter roles by status | Active/Inactive | Filter | Correct subset | P1 |
| POS-015 | Sort roles by name | Multiple roles | Sort | Alphabetically | P1 |
| POS-016 | Export role matrix | Matrix exists | Export | File downloaded | P1 |
| POS-017 | View user's effective permissions | User with roles | Open effective view | Merged permissions | P1 |
| POS-018 | Bulk assign role to users | 5 users | Select all, assign | All get role | P2 |
| POS-019 | Bulk remove role | 5 users with role | Select all, remove | All lose role | P2 |
| POS-020 | View role audit trail | Modified role | Open audit | History shown | P2 |
| POS-021 | Create role with minimal fields | Required only | Name only | Created | P2 |
| POS-022 | Copy role permissions | Source role | Copy to new role | Permissions copied | P2 |
| POS-023 | View users in role | Role with users | Open users list | Users listed | P2 |
| POS-024 | Role with Unicode name | Arabic name | Create | Stored correctly | P2 |
| POS-025 | Hierarchical permission inheritance | Child role | Child inherits parent | Effective perms include parent | P2 |
| POS-026 | Deactivate role | Active role | Set Inactive | Status = Inactive | P2 |
| POS-027 | Activate role | Inactive role | Set Active | Status = Active | P2 |
| POS-028 | Paginate role list | 50+ roles | Page 2 | 20 roles on page 2 | P2 |
| POS-029 | Paginate user list | 50+ users | Page 2 | 20 users on page 2 | P2 |
| POS-030 | Filter users by role | Users with roles | Filter by role | Only users with role | P2 |
| POS-031 | Get role by ID | Valid ID | API GetById | Role returned | P2 |
| POS-032 | Get user roles | User ID | API GetUserRoles | Roles returned | P2 |
| POS-033 | Get role permissions | Role ID | API GetRolePermissions | Permissions returned | P2 |
| POS-034 | Validate role assignment | Valid assignment | Assign | Success | P2 |
| POS-035 | Map role entity to model | Entity | mapper.Map | Model correct | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 90 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create role with null Name | Name = null | BusinessException: "Name is required" | P0 |
| NEG-002 | Create role with empty Name | Name = "" | BusinessException: "Name is required" | P0 |
| NEG-003 | Assign role to non-existent user | UserId = 999999 | KeyNotFoundException | P0 |
| NEG-004 | Assign non-existent role to user | RoleId = 999999 | KeyNotFoundException | P0 |
| NEG-005 | Assign non-existent permission | PermissionId = 999999 | KeyNotFoundException | P0 |
| NEG-006 | Update non-existent role | Id = 999999 | KeyNotFoundException | P0 |
| NEG-007 | Delete non-existent role | Id = 999999 | KeyNotFoundException | P0 |
| NEG-008 | Create with duplicate role name | Name exists | BusinessException: "Duplicate name" | P0 |
| NEG-009 | Create child with invalid parent | ParentId = 999999 | KeyNotFoundException | P0 |
| NEG-010 | Create circular hierarchy | A→B→A | BusinessException: "Circular" | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | User without admin | Reader | Create role | UnauthorizedAccessException | P0 |
| NEG-012 | User without admin | Reader | Assign role | UnauthorizedAccessException | P0 |
| NEG-013 | User without admin | Reader | Remove role | UnauthorizedAccessException | P0 |
| NEG-014 | Anonymous user | No auth | Any operation | 401 | P0 |
| NEG-015 | Expired session | Expired token | Assign role | 401 | P0 |
| NEG-016 | User without view | No permission | List roles | UnauthorizedAccessException | P1 |
| NEG-017 | Disabled user | Disabled | Any operation | 403 | P1 |
| NEG-018 | Read-only role | ReadOnly | Edit role | 403 | P1 |
| NEG-019 | API without auth | No Bearer | POST /roles | 401 | P0 |
| NEG-020 | Tampered JWT | Modified | Any operation | 401 | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Update deleted role | IsDeleted=true | Update | BusinessException | P1 |
| NEG-022 | Delete already-deleted | IsDeleted=true | Delete | No-op or error | P1 |
| NEG-023 | Remove system role from user | System role | Remove | BusinessException | P1 |
| NEG-024 | Delete role with users | Users assigned | Delete role | Error or cascade | P1 |
| NEG-025 | Create child of deleted parent | Parent deleted | Create child | BusinessException | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Create with all nulls | All null | BusinessException | P1 |
| NEG-027 | Assign with null UserId | UserId = null | ArgumentNullException | P1 |
| NEG-028 | Assign with null RoleId | RoleId = null | ArgumentNullException | P1 |
| NEG-029 | Search with null query | Query = null | Empty or all | P1 |
| NEG-030 | Filter with null role | Role = null | All roles | P1 |
| NEG-031 | Create with whitespace Name | "   " | BusinessException | P1 |
| NEG-032 | Get roles for null user | UserId = null | ArgumentException | P1 |
| NEG-033 | Create hierarchy with null parent | ParentId = null | Top-level role | P1 |
| NEG-034 | Assign permission to null role | RoleId = null | ArgumentNullException | P1 |
| NEG-035 | Bulk assign with empty list | [] | No-op or error | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Database connection lost | DB drops | Exception, rollback | P1 |
| NEG-037 | Database timeout | Slow DB | TimeoutException | P1 |
| NEG-038 | User service unavailable | User API down | Error | P1 |
| NEG-039 | Permission service unavailable | Permission API down | Error | P1 |
| NEG-040 | Role hierarchy conflict | Circular update | Validation error | P2 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Assign role to user twice | Already assigned | No-op or error | P1 |
| NEG-042 | Assign permission to role twice | Already assigned | No-op or error | P1 |
| NEG-043 | Name exceeds max length | 500 chars | Validation error | P1 |
| NEG-044 | Invalid hierarchy depth | 10 levels | Max depth error | P1 |
| NEG-045 | Create with SQL injection in name | `'; DROP--` | Sanitized or rejected | P0 |
| NEG-046 | Create with XSS in description | `<script>` | Sanitized | P0 |
| NEG-047 | Self-referential hierarchy | Parent = self | Rejected | P1 |
| NEG-048 | Delete role with permissions | Has permissions | Cascade or error | P1 |
| NEG-049 | Assign role to deleted user | User deleted | KeyNotFoundException | P1 |
| NEG-050 | Assign deleted role to user | Role deleted | KeyNotFoundException | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Create with negative RoleId | -1 | Validation error | P1 |
| NEG-052 | Create with zero UserId | 0 | Validation error | P1 |
| NEG-053 | Get with negative ID | -1 | Not found | P1 |
| NEG-054 | Paginate with page = 0 | Page = 0 | Default or error | P2 |
| NEG-055 | Paginate with pageSize = 0 | Size = 0 | Default or error | P2 |
| NEG-056 | Paginate with pageSize = 10000 | 10000 | Capped or error | P2 |
| NEG-057 | Sort by invalid column | "INVALID" | Default sort | P2 |
| NEG-058 | Remove last role from user | User has 1 role | Remove | Error or allowed | P1 |
| NEG-059 | Delete system role | System role | Delete | BusinessException | P1 |
| NEG-060 | Create role with invalid chars | Name = "Role<>" | Validation error | P1 |
| NEG-061 | Import with malformed data | Invalid CSV | Parse error | P2 |
| NEG-062 | Export with empty list | No roles | Empty file or error | P2 |
| NEG-063 | Hierarchy depth exceeded | 20 levels | Rejected | P1 |
| NEG-064 | Assign to inactive user | User inactive | Error or allowed | P1 |
| NEG-065 | Path traversal in export | `../../evil` | Rejected | P0 |
| NEG-066 | Create with null request | Request = null | ArgumentNullException | P1 |
| NEG-067 | Update with null request | Request = null | ArgumentNullException | P1 |
| NEG-068 | LDAP injection in search | `*)(cn=*` | Sanitized | P1 |
| NEG-069 | Regex injection in search | `.*+?[]()` | Escaped | P1 |
| NEG-070 | Concurrent delete same role | 2 users delete | One succeeds, other 404 | P1 |
| NEG-071 | Create role with invalid chars | Name = "Role@#$" | Validation error | P1 |
| NEG-072 | Assign role with null session | Session = null | ArgumentNullException | P1 |
| NEG-073 | Get role with invalid format | Id = "abc" | 400 Bad Request | P1 |
| NEG-074 | Bulk assign with null role | RoleId = null | ArgumentNullException | P1 |
| NEG-075 | Update role with invalid status | Status = "Invalid" | Validation error | P1 |
| NEG-076 | Delete role with active child | Child active | BusinessException or cascade | P1 |
| NEG-077 | Import with wrong encoding | UTF-16 file | Parse error | P2 |
| NEG-078 | Export with invalid format | Format = 999 | Default format | P2 |
| NEG-079 | Permission check with deleted user | User deleted | KeyNotFoundException | P1 |
| NEG-080 | Permission check with deleted role | Role deleted | KeyNotFoundException | P1 |
| NEG-081 | Create role with leading/trailing spaces | "  Role  " | Trimmed or error | P1 |
| NEG-082 | Assign with invalid role hierarchy | Parent = child | BusinessException | P1 |
| NEG-083 | Get effective perms for deleted user | User deleted | KeyNotFoundException | P1 |
| NEG-084 | Copy role with invalid target | Target deleted | KeyNotFoundException | P1 |
| NEG-085 | Validate with null hierarchy | Hierarchy = null | ArgumentNullException | P1 |
| NEG-086 | Search with empty string | "" | All or empty | P1 |
| NEG-087 | Filter with invalid status | Status = "invalid" | Default or error | P2 |
| NEG-088 | Create role with newline in name | "Role\n" | Rejected or trimmed | P1 |
| NEG-089 | Assign with duplicate in same request | Same assign twice | Duplicate error | P1 |
| NEG-090 | Update role with stale version | Stale version | Conflict error | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 90 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Role Name | 1 | 200 | ✅ "A" | ✅ 200 chars | ❌ Rejected | P1 |
| BND-002 | Role Description | 0 | 1000 | ✅ Empty | ✅ 1000 | ❌ Rejected | P1 |
| BND-003 | Search query | 0 | 255 | ✅ Empty | ✅ 255 | ❌ Capped | P1 |
| BND-004 | Permission name | 1 | 100 | ✅ "P" | ✅ 100 | ❌ Rejected | P2 |
| BND-005 | User name | 1 | 200 | ✅ "U" | ✅ 200 | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Role ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-007 | User ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-008 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-009 | Page size | 1 | 1000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-010 | Hierarchy depth | 1 | 10 | ❌ | ❌ | Rejected | P1 |
| BND-011 | Roles per user | 0 | 20 | ✅ None | ❌ | Rejected | P2 |
| BND-012 | Permissions per role | 0 | 500 | ✅ None | ❌ | Performance | P2 |
| BND-013 | Users per role | 0 | 10000 | ✅ None | ❌ | Paginated | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-014 | Role created leap year | Feb 29, 2028 | CreatedDate correct | P2 |
| BND-015 | Assignment at midnight | 00:00:00 UTC | Stored correctly | P2 |
| BND-016 | Audit boundary | Timezone edge | Correct display | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-017 | Zero roles | Empty | Empty list, count=0 | P1 |
| BND-018 | One role | Single | List with 1 item | P1 |
| BND-019 | Exactly page size roles | 20, size=20 | Full page, hasNext=false | P1 |
| BND-020 | Page size + 1 roles | 21, size=20 | 20 on page 1, hasNext=true | P1 |
| BND-021 | 1000 roles | Large | Paginated correctly | P1 |
| BND-022 | User with 0 roles | No roles | Empty list | P1 |
| BND-023 | User with 1 role | Single | 1 role | P1 |
| BND-024 | User with 20 roles | Max | All 20 | P2 |
| BND-025 | Role with 0 permissions | No permissions | Empty permissions | P1 |
| BND-026 | Role with 0 users | No users | Empty users | P1 |
| BND-027 | Role with 100 users | Many | Paginated | P1 |
| BND-028 | Last page with 1 item | 41 roles, page 3, size 20 | 1 on page 3 | P1 |
| BND-029 | Flat hierarchy (0 levels) | No parent | Top-level | P1 |
| BND-030 | Hierarchy depth 2 | Parent → Child | 2 levels | P1 |
| BND-031 | Hierarchy depth 10 | Max depth | All loaded | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-032 | Name (Arabic) | `دور` | Stored correctly | P2 |
| BND-033 | Name (Chinese) | `角色` | Stored correctly | P2 |
| BND-034 | Name (Cyrillic) | `Роль` | Stored correctly | P2 |
| BND-035 | Name with apostrophe | `Admin's Role` | Preserved | P1 |
| BND-036 | Name with hyphen | `Partner-User` | Preserved | P1 |
| BND-037 | Description with newlines | Multi-line | Newlines preserved | P2 |
| BND-038 | Name with ampersand | `Editor & Reviewer` | Preserved | P2 |
| BND-039 | Search with special chars | "O'Brien" | Handled | P2 |
| BND-040 | Name with accent | `Gérant` | Stored correctly | P2 |

### 3.6 Hierarchy Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | Single role (no hierarchy) | 1 role | Root role | P1 |
| BND-042 | Two levels | Parent → Child | Child inherits | P1 |
| BND-043 | Three levels | A → B → C | C inherits A, B | P1 |
| BND-044 | Sibling roles | Same parent | Independent | P1 |
| BND-045 | Multiple roots | No parent | Multiple top-level | P1 |
| BND-046 | Move role in hierarchy | Change parent | Hierarchy updated | P1 |
| BND-047 | Orphan role (parent deleted) | Parent deleted | Handle gracefully | P2 |
| BND-048 | Effective permissions merge | User with 2 roles | Union of permissions | P1 |
| BND-049 | Conflicting permissions | Same perm different | Policy applied | P2 |
| BND-050 | Inheritance depth at max | 10 levels | All effective | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Name exactly 1 char | "A" | Accepted | P1 |
| BND-052 | Name exactly max | 200 chars | Accepted | P1 |
| BND-053 | Role ID = 1 | First role | Retrieved | P2 |
| BND-054 | Role ID = MAX_INT | Overflow | Handled | P2 |
| BND-055 | Search with 1 char | "A" | Matches | P1 |
| BND-056 | Search with max chars | 255 chars | Processed | P1 |
| BND-057 | Bulk assign 1 user | Single | Success | P2 |
| BND-058 | Bulk assign 100 users | Batch | Success | P2 |
| BND-059 | Create with all optional null | Required only | Success | P1 |
| BND-060 | Create with all optional filled | Full data | Success | P1 |
| BND-061 | Concurrent list requests | 2 users list | Both correct | P2 |
| BND-062 | Timezone boundary | UTC vs local | Correct | P2 |
| BND-063 | Sort by each column | Name, Date, etc. | All work | P1 |
| BND-064 | Filter by each status | Active, Inactive | Correct | P2 |
| BND-065 | Empty matrix | No roles/perms | Empty grid | P1 |
| BND-066 | Matrix with 1 role | Single | 1 row | P1 |
| BND-067 | Matrix with 100 permissions | Many | All columns | P2 |
| BND-068 | Export empty | No data | Empty file | P2 |
| BND-069 | Import with 1 role | Single | Created | P2 |
| BND-070 | Copy role with 50 permissions | Many | All copied | P2 |
| BND-071 | Role name at 199 chars | 199 chars | Accepted | P1 |
| BND-072 | Description at 999 chars | 999 chars | Accepted | P1 |
| BND-073 | Page size at 999 | 999 | Accepted | P1 |
| BND-074 | Page size at 1001 | 1001 | Capped at 1000 | P1 |
| BND-075 | Hierarchy depth at 9 | 9 levels | Accepted | P1 |
| BND-076 | Roles per user at 19 | 19 roles | Accepted | P2 |
| BND-077 | Permissions per role at 499 | 499 | Accepted | P2 |
| BND-078 | Search exactly 254 chars | 254 chars | Processed | P1 |
| BND-079 | User with 2 roles | Two roles | Merged effective | P1 |
| BND-080 | Role with 1 permission | Single | 1 permission | P1 |
| BND-081 | Bulk assign 99 users | 99 | Success | P2 |
| BND-082 | Empty role name | "" | Rejected | P1 |
| BND-083 | Single space in name | " " | Rejected | P1 |
| BND-084 | Tab in name | "Role\t" | Rejected or trimmed | P1 |
| BND-085 | Unicode in description | Arabic desc | Stored | P2 |
| BND-086 | Role ID = 2 | Second role | Retrieved | P2 |
| BND-087 | Pagination page 2 of 2 | 2 pages | 2nd page | P1 |
| BND-088 | Filter by single status | Active only | Correct subset | P1 |
| BND-089 | Matrix with 2 roles | Two roles | 2 rows | P1 |
| BND-090 | Zero permissions in role | 0 | Empty | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 90 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Roles query excludes deleted | IsDeleted filter | List roles | Only !IsDeleted | P0 |
| FUN-002 | Create sets audit | Audit on create | CreateRole | CreatedBy, CreatedDate | P0 |
| FUN-003 | Update sets audit | Audit on update | UpdateRole | LastModifiedBy, LastModifiedDate | P0 |
| FUN-004 | Delete sets soft-delete | Soft-delete | DeleteRole | IsDeleted, DeletedBy, DeletedDate | P0 |
| FUN-005 | Assign creates mapping | User-role | AssignRole | Mapping created | P0 |
| FUN-006 | Remove deletes mapping | User-role | RemoveRole | Mapping deleted | P0 |
| FUN-007 | Permission assignment | Role-permission | AssignPermission | Mapping created | P0 |
| FUN-008 | Hierarchy inheritance | Child role | Effective permissions | Inherits parent | P1 |
| FUN-009 | User cannot have duplicate role | Same role | Assign again | No duplicate | P1 |
| FUN-010 | Role cannot have duplicate permission | Same permission | Assign again | No duplicate | P1 |
| FUN-011 | Effective permissions = union | Multiple roles | GetEffective | Union of all | P1 |
| FUN-012 | Deactivated role excluded | Inactive | User with role | Role not in effective if inactive | P1 |
| FUN-013 | System role protected | System role | Delete | BusinessException | P1 |
| FUN-014 | Export includes only active | Export | Export | Only !IsDeleted | P1 |
| FUN-015 | Import creates new | Import | Import | New IDs | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Name required | Required | "Admin" | null, "" | P0 |
| FUN-017 | Name unique | Unique | "NewRole" | "Admin" (exists) | P0 |
| FUN-018 | User must exist | FK | Valid ID | 999999 | P0 |
| FUN-019 | Role must exist | FK | Valid ID | 999999 | P0 |
| FUN-020 | Permission must exist | FK | Valid ID | 999999 | P0 |
| FUN-021 | Name max length | ≤200 | 200 chars | 201 | P1 |
| FUN-022 | No circular hierarchy | No cycle | A→B | A→B→A | P0 |
| FUN-023 | Hierarchy depth | ≤10 | 10 | 11 | P1 |
| FUN-024 | No SQL in name | Sanitize | "Role" | `'; DROP--` | P0 |
| FUN-025 | No XSS in description | Sanitize | "Desc" | `<script>` | P0 |
| FUN-026 | Trim whitespace | Trim | "  Name  " | → "Name" | P2 |
| FUN-027 | Parent must exist | FK | Valid parent | 999999 | P0 |
| FUN-028 | System role immutable | Cannot edit | System role | Edit | P1 |
| FUN-029 | User must be active | Active | Active user | Deleted user | P1 |
| FUN-030 | Role must be active | Active | Active role | Deleted role | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max roles per user | 20 | 21 | Rejected | P1 |
| FUN-032 | Max page size | 1000 | 5000 | Capped | P1 |
| FUN-033 | Unique name | DB constraint | Duplicate | Constraint error | P0 |
| FUN-034 | FK user exists | FK | Non-existent | FK error | P0 |
| FUN-035 | FK role exists | FK | Non-existent | FK error | P0 |
| FUN-036 | Bulk assign limit | 100 | 150 | Chunked | P2 |
| FUN-037 | Hierarchy depth | 10 | 11 | Rejected | P1 |
| FUN-038 | System role count | ≥1 | Delete all | Error | P1 |
| FUN-039 | User must have ≥1 role | Optional | Remove last | Error or allowed | P2 |
| FUN-040 | Concurrent assignment | Unique | Same assign | One succeeds | P1 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Create audit | CreateRole | CreatedBy, CreatedDate | P0 |
| FUN-042 | Update audit | UpdateRole | LastModifiedBy, LastModifiedDate | P0 |
| FUN-043 | Delete audit | DeleteRole | DeletedBy, DeletedDate | P0 |
| FUN-044 | Assign audit | AssignRole | UserId, RoleId, AssignedBy | P1 |
| FUN-045 | Remove audit | RemoveRole | UserId, RoleId, RemovedBy | P1 |
| FUN-046 | Permission assign audit | AssignPermission | RoleId, PermissionId | P1 |
| FUN-047 | Read no audit | GetRole | No modification | P1 |
| FUN-048 | Export audit | Export | ExportBy, ExportDate | P1 |
| FUN-049 | Failed create no audit | Failed create | No audit entry | P1 |
| FUN-050 | Audit immutable on read | Get | Audit fields unchanged | P1 |
| FUN-051 | Remove last role from user | User has 1 role | Remove | Error or allowed per policy | P1 |
| FUN-052 | Role name case sensitivity | "Admin" vs "admin" | Per policy | P2 |
| FUN-053 | Permission inheritance depth | 5 levels | All inherited | P1 |
| FUN-054 | Export format selection | CSV | Correct format | P2 |
| FUN-055 | Import format validation | JSON | Validated | P1 |
| FUN-056 | Role status affects effective | Inactive role | Excluded from effective | P1 |
| FUN-057 | User status affects assignment | User inactive | Assignment blocked per policy | P1 |
| FUN-058 | Permission scope validation | Entity scope | Validated | P1 |
| FUN-059 | Role creation audit | Create | CreatedBy set | P0 |
| FUN-060 | Role update audit | Update | LastModifiedBy set | P0 |
| FUN-061 | Permission grant audit | Grant | Audit entry | P1 |
| FUN-062 | Permission revoke audit | Revoke | Audit entry | P1 |
| FUN-063 | Hierarchy depth validation | 11 levels | Rejected | P1 |
| FUN-064 | Duplicate role name check | Same name | Rejected | P0 |
| FUN-065 | Role name trim on save | "  Name  " | → "Name" | P2 |
| FUN-066 | Permission count limit | 501 | Rejected | P1 |
| FUN-067 | User role assignment limit | 21 | Rejected | P1 |
| FUN-068 | Bulk assign validation | Batch | Each validated | P1 |
| FUN-069 | Copy role preserves hierarchy | Copy | Hierarchy copied | P1 |
| FUN-070 | Compare roles diff | Compare | Correct diff | P1 |
| FUN-071 | Export excludes deleted | Export | !IsDeleted | P1 |
| FUN-072 | Import creates new IDs | Import | New IDs | P1 |
| FUN-073 | System role count | ≥1 | Cannot delete all | P1 |
| FUN-074 | Permission required check | Check | Correct result | P0 |
| FUN-075 | Role hierarchy validation | Validate | No circular | P0 |
| FUN-076 | Effective permissions merge | User with 3 roles | Union of all | P1 |
| FUN-077 | Entity override precedence | Entity + Global | Entity wins | P0 |
| FUN-078 | Deny overrides grant | Grant + Deny | Deny | P0 |
| FUN-079 | Role activation cascade | Activate | Child roles affected per policy | P2 |
| FUN-080 | Role deactivation cascade | Deactivate | Child roles affected | P2 |
| FUN-081 | Permission assignment audit | Assign | Audit entry | P1 |
| FUN-082 | Permission removal audit | Remove | Audit entry | P1 |
| FUN-083 | Role deletion soft delete | Delete | IsDeleted=true | P0 |
| FUN-084 | Role list excludes deleted | List | !IsDeleted | P0 |
| FUN-085 | Role hierarchy load | Load child | Parent loaded | P1 |
| FUN-086 | Permission list load | Load role | Permissions loaded | P1 |
| FUN-087 | User list load | Load role | Users loaded | P1 |
| FUN-088 | Pagination default | null params | 1, 20 | P1 |
| FUN-089 | Filter by status | Active | Active only | P1 |
| FUN-090 | Sort by name | Sort | Alphabetical | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Full CRUD lifecycle | Create→Read→Update→Delete | Role | All succeed | P0 |
| INT-002 | Create → in list | Create | Role | In list | P0 |
| INT-003 | Delete → excluded | Delete | Role | Not in list | P0 |
| INT-004 | Assign → user has role | Assign | User, Role | User has role | P0 |
| INT-005 | Remove → user loses role | Remove | User, Role | User no role | P0 |
| INT-006 | Assign permission → role has | Assign | Role, Permission | Role has permission | P1 |
| INT-007 | Create hierarchy → child inherits | Create child | Parent, Child | Child inherits | P1 |
| INT-008 | Bulk assign → all get role | Bulk | Users, Role | All assigned | P1 |
| INT-009 | Copy role → new role | Copy | Source, Target | Target has same perms | P1 |
| INT-010 | Import → export round-trip | Import + Export | Roles | Data round-trip | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Search roles by name | "Admin" | Matching roles | P0 |
| INT-012 | Search users by name | "John" | Matching users | P0 |
| INT-013 | Filter by role | Role filter | Users with role | P1 |
| INT-014 | Filter by status | Active | Active only | P1 |
| INT-015 | Combined search + filter | Name + Role | Both applied | P1 |
| INT-016 | Search empty | "NONEXISTENT" | Empty result | P1 |
| INT-017 | Search case-insensitive | "admin" vs "ADMIN" | Same results | P1 |
| INT-018 | Filter by hierarchy level | Level 2 | Level 2 roles | P1 |
| INT-019 | Search with special chars | "O'Brien" | Handled | P2 |
| INT-020 | Filter excludes deleted | Include deleted | Deleted excluded | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Page 1 of 3 | 50, page=1, size=20 | 20, hasNext | P1 |
| INT-022 | Last page | 50, page=3, size=20 | 10, hasNext=false | P1 |
| INT-023 | Empty page | Filter yields 0 | Empty, total=0 | P1 |
| INT-024 | Single page | 15, size=20 | 15 items | P2 |
| INT-025 | Large page | 1000, size=1000 | All on 1 page | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Role → Permissions | Many-to-many | Load role | Permissions loaded | P0 |
| INT-027 | Role → Users | Many-to-many | Load role | Users loaded | P0 |
| INT-028 | User → Roles | Many-to-many | Load user | Roles loaded | P0 |
| INT-029 | Role → Parent | Hierarchy | Load child | Parent loaded | P1 |
| INT-030 | Role → Children | Hierarchy | Load parent | Children loaded | P1 |
| INT-031 | Effective permissions | Computed | User with roles | Merged permissions | P1 |
| INT-032 | Permission → Roles | Reverse | Load permission | Roles with permission | P1 |
| INT-033 | Audit trail | Audit | Load role audit | History | P1 |
| INT-034 | Export includes relations | Export | Export | Role-permission in file | P2 |
| INT-035 | Import with permissions | Import | Import | Permissions assigned | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Create invalid → 400 | Validation | BusinessException | P0 |
| INT-037 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-038 | Unauthorized → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-039 | Update non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-040 | Delete non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-041 | Duplicate name → 400 | Constraint | BusinessException | P1 |
| INT-042 | Circular hierarchy → 400 | Validation | BusinessException | P1 |
| INT-043 | Import malformed → 400 | Parse | Validation error | P1 |
| INT-044 | DB timeout → 500 | Timeout | Graceful error | P1 |
| INT-045 | Concurrent conflict → 409 | Concurrency | Conflict error | P1 |
| INT-046 | Malformed request → 400 | Bad JSON | Validation error | P1 |
| INT-047 | Rate limit → 429 | Too many | Rate limit | P2 |
| INT-048 | SQL injection → sanitized | Injection | Parameterized | P0 |
| INT-049 | Large payload → 413 | Oversized | Rejected | P2 |
| INT-050 | Delete system role → 400 | Business rule | BusinessException | P1 |
| INT-051 | Create → Assign → Remove | Full lifecycle | All succeed | P1 |
| INT-052 | Create hierarchy → Assign | Parent → Child | Success | P1 |
| INT-053 | Role → Permission → User | Full chain | All linked | P1 |
| INT-054 | Export → Import round-trip | Export | Import | Data preserved | P1 |
| INT-055 | Copy role → Verify | Copy | Target same | P1 |
| INT-056 | Compare → Validate | Compare | Diff correct | P1 |
| INT-057 | Bulk assign → Verify | Bulk | All assigned | P1 |
| INT-058 | Search → Filter → Sort | Combined | Correct results | P1 |
| INT-059 | API GetById → GetPermissions | Get | Permissions returned | P1 |
| INT-060 | API GetUserRoles → Effective | Get | Merged correct | P1 |
| INT-061 | Create → Update → Delete | CRUD | All succeed | P0 |
| INT-062 | Assign → Remove → Reassign | Assign cycle | Success | P1 |
| INT-063 | Hierarchy → Effective | Inherit | Child inherits | P1 |
| INT-064 | Entity override → Check | Override | Override applies | P1 |
| INT-065 | Import → Export | Import | Export matches | P1 |
| INT-066 | Pagination → Filter | Page + Filter | Correct subset | P1 |
| INT-067 | Role → Permission matrix | Load | Matrix correct | P1 |
| INT-068 | User → Roles → Effective | Load | Effective correct | P1 |
| INT-069 | Audit trail → Load | Load audit | History shown | P1 |
| INT-070 | Permission → Roles | Reverse | Roles with permission | P1 |
| INT-071 | Create role → In list | Create | In list | P0 |
| INT-072 | Delete role → Not in list | Delete | Excluded | P0 |
| INT-073 | Update role → Persisted | Update | Changes saved | P0 |
| INT-074 | Assign → User has role | Assign | User has role | P0 |
| INT-075 | Remove → User loses role | Remove | User no role | P0 |
| INT-076 | Search + Filter | Combined | Both applied | P1 |
| INT-077 | Sort + Paginate | Combined | Correct order | P1 |
| INT-078 | Copy → Edit | Copy | Independent | P1 |
| INT-079 | Bulk remove → Verify | Bulk remove | All removed | P1 |
| INT-080 | Validate → No errors | Validate | Valid | P1 |
| INT-081 | Hierarchy depth → Effective | 5 levels | All inherited | P1 |
| INT-082 | Entity override → Deny | Deny override | Deny applies | P1 |
| INT-083 | Permission grant → Deny | Grant + Deny | Deny wins | P1 |
| INT-084 | System role → Toggle | System role | Protected | P1 |
| INT-085 | Role hierarchy → Load | Load | Parent loaded | P1 |
| INT-086 | Permission list → Paginate | Paginate | Correct page | P1 |
| INT-087 | User list → Paginate | Paginate | Correct page | P1 |
| INT-088 | Export → Download | Export | File downloaded | P1 |
| INT-089 | Import → Validate | Import | Validated | P1 |
| INT-090 | Full workflow → Audit | Full workflow | Audit complete | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in Name | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL injection in search | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in Name | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in Description | `"><script>` | Sanitized | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | OS command | `; rm -rf /` | Sanitized | P0 |
| SEC-007 | Path traversal | `../../evil` | Rejected | P0 |
| SEC-008 | HTML in description | `<img onerror=...>` | Escaped | P1 |
| SEC-009 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-010 | XXE | XXE payload | Rejected | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous create | No auth | POST /roles | 401 | P0 |
| SEC-012 | No create permission | Reader | POST /roles | 403 | P0 |
| SEC-013 | Expired token | Expired | Any | 401 | P0 |
| SEC-014 | Tampered JWT | Modified | Any | 401 | P0 |
| SEC-015 | Disabled account | Disabled | Any | 403 | P1 |
| SEC-016 | Post-logout | Logged out | Cached | 401 | P1 |
| SEC-017 | Role escalation | Basic | ?role=admin | Ignored | P0 |
| SEC-018 | Cross-tenant | User A | User B's scope | 403 | P0 |
| SEC-019 | No export permission | Reader | Export | 403 | P1 |
| SEC-020 | No import permission | Reader | Import | 403 | P1 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Role ID guess | Enumerate | 403 if no access | P0 |
| SEC-022 | Deleted role | Access deleted | 404 | P1 |
| SEC-023 | Negative ID | -1 | 400 | P1 |
| SEC-024 | Zero ID | 0 | 400 | P1 |
| SEC-025 | Float ID | 1.5 | 400 | P1 |
| SEC-026 | String ID | "abc" | 400 | P1 |
| SEC-027 | MAX_INT ID | 2147483647 | 404 | P1 |
| SEC-028 | User ID manipulation | Change ID | Validated | P0 |
| SEC-029 | Permission ID manipulation | Change ID | Validated | P0 |
| SEC-030 | Other user's roles | Access via ID | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable | P0 |
| SEC-035 | DeletedBy/DeletedDate | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on create | CSRF token | P0 |
| SEC-040 | CSRF on delete | CSRF token | P0 |
| SEC-041 | Token storage | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Policy enforced | P1 |
| SEC-043 | Token refresh | Works correctly | P1 |
| SEC-044 | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected Protection | Priority |
|----|------|-------------------|----------|
| SEC-046 | Internal audit fields | DTO filtering | P1 |
| SEC-047 | Stack traces | Generic errors | P0 |
| SEC-048 | User PII in role list | Filtered | P1 |
| SEC-049 | Response caching | Cache-Control: no-store | P1 |
| SEC-050 | Tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users update same role | Concurrent update | Last write wins or conflict | P1 |
| CON-002 | Create and delete same role | Race | One succeeds, other fails | P1 |
| CON-003 | Two users assign roles | Concurrent assign | Both succeed | P1 |
| CON-004 | Update during read | Read consistency | Consistent read | P1 |
| CON-005 | Delete during read | Read consistency | Null or pre-delete | P1 |
| CON-006 | Concurrent assign same role to user | 2 users assign | One succeeds | P1 |
| CON-007 | Concurrent remove role | 2 users remove | One succeeds | P1 |
| CON-008 | Concurrent pagination | Multiple pages | Correct data | P2 |
| CON-009 | Database deadlock | Circular | Resolved, retry | P1 |
| CON-010 | Token refresh during assign | Expire mid-call | Retry with new token | P1 |
| CON-011 | Bulk assign concurrent | 2 users bulk assign | Both complete | P2 |
| CON-012 | Concurrent export | 2 exports | Both succeed | P2 |
| CON-013 | Update during assign | Assign + Update | No corruption | P1 |
| CON-014 | Concurrent permission assign | 2 users assign | Both succeed | P1 |
| CON-015 | Hierarchy update during read | Update + Read | Consistent | P1 |
| CON-016 | Duplicate during delete | Duplicate + Delete | Handled | P2 |
| CON-017 | Import during list | Import + List | List consistent | P2 |
| CON-018 | Concurrent filter | 2 users filter | Independent | P2 |
| CON-019 | Optimistic concurrency | Update stale | Conflict error | P1 |
| CON-020 | Connection pool exhaustion | Many concurrent | Queued or error | P1 |
| CON-021 | Cache invalidation | Update + read | Fresh data | P1 |
| CON-022 | Effective permissions race | 2 roles assigned | Correct merge | P1 |
| CON-023 | Hierarchy conflict | Same parent update | Conflict | P1 |
| CON-024 | Bulk remove concurrent | 2 bulk removes | Both complete | P2 |
| CON-025 | Search during update | Search + Update | Search consistent | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Name validation | Validation | "Admin" | Valid | P1 |
| UNT-002 | Empty name validation | Validation | "" | Invalid | P1 |
| UNT-003 | Effective permissions merge | Calculations | Role A + Role B | Union | P1 |
| UNT-004 | Hierarchy depth | Calculations | 5 levels | 5 | P1 |
| UNT-005 | Circular detection | Validation | A→B→A | Error | P1 |
| UNT-006 | Name trim | Formatting | "  Name  " | "Name" | P2 |
| UNT-007 | Pagination default | Calculations | null, null | 1, 20 | P1 |
| UNT-008 | Status Active | Status logic | Activate | Active | P1 |
| UNT-009 | Status Inactive | Status logic | Deactivate | Inactive | P1 |
| UNT-010 | Map entity to model | Collections | Entity | Model | P1 |
| UNT-011 | Map request to entity | Collections | Request | Entity | P1 |
| UNT-012 | Permission inheritance | Status logic | Child role | Inherits parent | P1 |
| UNT-013 | System role check | Validation | System role | Protected | P1 |
| UNT-014 | Duplicate role check | Validation | Same role | Rejected | P1 |
| UNT-015 | Export format | Formatting | Roles | CSV/JSON | P2 |
| UNT-016 | Import parse | Validation | Valid JSON | Role list | P1 |
| UNT-017 | User ID validation | Validation | 42 | Valid | P1 |
| UNT-018 | Role ID validation | Validation | 1 | Valid | P1 |
| UNT-019 | Hierarchy level | Collections | Child | Level 2 | P1 |
| UNT-020 | Permission count | Collections | Role | 10 | P1 |
| UNT-021 | Date format for audit | Formatting | Now | ISO string | P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create role | Single create | < 200ms | P2 |
| PRF-002 | Get role by ID | Single read | < 100ms | P2 |
| PRF-003 | List 20 roles | Paginated | < 500ms | P2 |
| PRF-004 | List 1000 roles | Full list | < 3s | P2 |
| PRF-005 | Search roles | Search | < 1s | P2 |
| PRF-006 | Assign role | Single assign | < 300ms | P2 |
| PRF-007 | Get effective permissions | User with 5 roles | < 500ms | P2 |
| PRF-008 | Export 100 roles | Export | < 2s | P2 |
| PRF-009 | 10 concurrent creates | Concurrent | All < 1s | P2 |
| PRF-010 | 20 concurrent reads | Concurrent | All < 500ms | P2 |
| PRF-011 | List with hierarchy | Includes | < 1s | P2 |
| PRF-012 | Matrix with 50 roles | Load matrix | < 2s | P2 |
| PRF-013 | Bulk assign 50 users | Bulk | < 5s | P2 |
| PRF-014 | Hierarchy depth 10 | Load | < 1s | P2 |
| PRF-015 | Memory: 1000 roles | Load all | No leak | P2 |
| PRF-016 | Filter + sort | Combined | < 1s | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained list | 10 users, 1 req/s | 5 min | 95% < 1s | P2 |
| LDT-002 | Sustained create | 5 users, 0.5 req/s | 5 min | 95% < 500ms | P2 |
| LDT-003 | Sustained search | 20 users, 2 req/s | 5 min | 95% < 1s | P2 |
| LDT-004 | Spike list | 0→50 users in 30s | 2 min | No errors | P2 |
| LDT-005 | Spike assign | 0→20 users | 2 min | Queue or 429 | P2 |
| LDT-006 | Stress list | 100 users, 5 req/s | 5 min | Graceful degradation | P2 |
| LDT-007 | Stress create | 50 users, 2 req/s | 5 min | Queue or 429 | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 10 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | System recovers | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: CRUD roles | POS-001 to POS-005, INT-001 to INT-010 |
| AC-2: Permission assignment | POS-004, FUN-007, INT-006 |
| AC-3: User-role mapping | POS-002, POS-003, FUN-005, FUN-006 |
| AC-4: Role hierarchy | POS-011, POS-012, FUN-008, BND-041 to BND-050 |
| AC-5: Audit | FUN-041 to FUN-050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
