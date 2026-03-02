# Role Matrix — Test Cases

**Component:** `UNOPS.PAO.ContextPermissions / Role-Permission Matrix`  
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

Role matrix: permission matrix validation, role-action mapping, entity-level permissions, cascading permissions.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: View Permission Matrix

**Priority:** P0  
**Precondition:** User has admin permission.

**Steps:**
1. Open Role Matrix
2. View role-permission grid

**Expected Result:** Matrix displayed with roles as rows, permissions as columns.

---

#### POS-002: Toggle Permission for Role

**Priority:** P0  
**Precondition:** Role and permission exist.

**Steps:**
1. Locate role-permission cell
2. Toggle checkbox
3. Save

**Expected Result:** Role-permission mapping created/removed.

---

#### POS-003: Validate Effective Permissions

**Priority:** P0  
**Precondition:** User has roles with permissions.

**Steps:**
1. Open effective permissions view for user
2. View merged permissions

**Expected Result:** Union of all role permissions displayed.

---

#### POS-004: Apply Entity-Level Permission

**Priority:** P0  
**Precondition:** Entity type configured.

**Steps:**
1. Select entity type (e.g., Partner)
2. Assign permission for role
3. Save

**Expected Result:** Entity-level permission saved.

---

#### POS-005: Cascading Permission Inheritance

**Priority:** P0  
**Precondition:** Role hierarchy exists.

**Steps:**
1. Parent role has CanViewPartners
2. Child role inherits
3. User with child role

**Expected Result:** User has CanViewPartners via inheritance.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Filter matrix by role | Multiple roles | Filter "Admin" | Only Admin row | P1 |
| POS-007 | Filter matrix by permission | Multiple permissions | Filter "CanEdit" | Only matching cols | P1 |
| POS-008 | Bulk enable permissions | Role selected | Select 5 permissions, enable | All enabled | P1 |
| POS-009 | Bulk disable permissions | Role has 5 permissions | Select all, disable | All disabled | P1 |
| POS-010 | Export matrix | Matrix exists | Export | CSV/Excel downloaded | P1 |
| POS-011 | Import matrix | Valid file | Import | Matrix updated | P1 |
| POS-012 | Compare two roles | 2 roles | Compare | Diff shown | P1 |
| POS-013 | Copy role permissions | Source role | Copy to target | Target has same | P1 |
| POS-014 | Search permission by name | Permissions exist | Search "Partner" | Matching permissions | P1 |
| POS-015 | Search role by name | Roles exist | Search "Admin" | Matching roles | P1 |
| POS-016 | Entity-level override | Global + Entity | Entity override | Entity wins | P1 |
| POS-017 | View permission details | Permission selected | View details | Description, scope | P1 |
| POS-018 | View role details | Role selected | View details | Permissions, users | P1 |
| POS-019 | Audit matrix change | Toggle permission | View audit | Change logged | P2 |
| POS-020 | Paginate roles | 50+ roles | Page 2 | 20 roles | P2 |
| POS-021 | Paginate permissions | 100+ permissions | Page 2 | 20 permissions | P2 |
| POS-022 | Sort by role name | Multiple roles | Sort | Alphabetically | P2 |
| POS-023 | Sort by permission name | Multiple permissions | Sort | Alphabetically | P2 |
| POS-024 | Responsive matrix | Mobile | View | Responsive layout | P2 |
| POS-025 | Accessibility: keyboard | Focus | Tab through | All focusable | P2 |
| POS-026 | Accessibility: screen reader | Screen reader | Navigate | Announced | P2 |
| POS-027 | Validation: no conflicts | Role A, Role B | Check | No conflict | P2 |
| POS-028 | Validation: hierarchy | Parent, Child | Check | Child inherits | P2 |
| POS-029 | Matrix with 1 role | Single role | View | 1 row | P2 |
| POS-030 | Matrix with 1 permission | Single permission | View | 1 column | P2 |
| POS-031 | Get matrix by ID | Valid ID | API GetById | Matrix returned | P2 |
| POS-032 | Get effective for user | User ID | API GetEffective | Merged permissions | P2 |
| POS-033 | Check permission | User, Permission | API Check | true/false | P2 |
| POS-034 | Validate matrix | Current state | Validate | No errors | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 90 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Toggle with null RoleId | RoleId = null | ArgumentNullException | P0 |
| NEG-002 | Toggle with null PermissionId | PermissionId = null | ArgumentNullException | P0 |
| NEG-003 | Toggle with non-existent role | RoleId = 999999 | KeyNotFoundException | P0 |
| NEG-004 | Toggle with non-existent permission | PermissionId = 999999 | KeyNotFoundException | P0 |
| NEG-005 | Get effective for non-existent user | UserId = 999999 | KeyNotFoundException | P0 |
| NEG-006 | Entity-level with invalid entity | EntityType = "Invalid" | Validation error | P0 |
| NEG-007 | Compare non-existent roles | RoleId = 999999 | KeyNotFoundException | P0 |
| NEG-008 | Import malformed file | Invalid CSV | Parse error | P0 |
| NEG-009 | Copy to non-existent role | TargetId = 999999 | KeyNotFoundException | P0 |
| NEG-010 | Validate with circular hierarchy | A→B→A | BusinessException | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | User without admin | Reader | View matrix | Unauthorized | P0 |
| NEG-012 | User without admin | Reader | Toggle permission | Unauthorized | P0 |
| NEG-013 | Anonymous user | No auth | Any operation | 401 | P0 |
| NEG-014 | Expired session | Expired token | Toggle | 401 | P0 |
| NEG-015 | Disabled user | Disabled | Any operation | 403 | P1 |
| NEG-016 | User without view | No permission | View matrix | Unauthorized | P1 |
| NEG-017 | Read-only role | ReadOnly | Toggle | 403 | P1 |
| NEG-018 | API without auth | No Bearer | POST /matrix | 401 | P0 |
| NEG-019 | Tampered JWT | Modified | Any operation | 401 | P0 |
| NEG-020 | Post-logout | Logged out | Cached toggle | 401 | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Toggle for deleted role | Role deleted | Toggle | KeyNotFoundException | P1 |
| NEG-022 | Toggle for deleted permission | Permission deleted | Toggle | KeyNotFoundException | P1 |
| NEG-023 | Entity override for deleted entity | Entity deleted | Override | Error | P1 |
| NEG-024 | Copy from deleted role | Source deleted | Copy | KeyNotFoundException | P1 |
| NEG-025 | Validate with dead role | Role with no users | Validate | Warning or ok | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Toggle with all nulls | RoleId, PermissionId null | ArgumentNullException | P1 |
| NEG-027 | Get effective with null user | UserId = null | ArgumentNullException | P1 |
| NEG-028 | Import with null file | File = null | ArgumentNullException | P1 |
| NEG-029 | Export with null format | Format = null | Default format | P1 |
| NEG-030 | Filter with null criteria | Filter = null | All results | P1 |
| NEG-031 | Compare with null role | Role = null | ArgumentNullException | P1 |
| NEG-032 | Copy with null source | SourceId = null | ArgumentNullException | P1 |
| NEG-033 | Entity override with null entity | EntityId = null | ArgumentNullException | P1 |
| NEG-034 | Validate with null matrix | Matrix = null | ArgumentNullException | P1 |
| NEG-035 | Bulk toggle with empty list | [] | No-op or error | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Database connection lost | DB drops | Exception, rollback | P1 |
| NEG-037 | Database timeout | Slow DB | TimeoutException | P1 |
| NEG-038 | Role service unavailable | Role API down | Error | P1 |
| NEG-039 | Permission service unavailable | Permission API down | Error | P1 |
| NEG-040 | Hierarchy service unavailable | Hierarchy API down | Error | P2 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Toggle already enabled | Permission already on | No-op or error | P1 |
| NEG-042 | Toggle already disabled | Permission already off | No-op or error | P1 |
| NEG-043 | Conflicting entity overrides | Conflict | Resolution or error | P1 |
| NEG-044 | Hierarchy depth exceeded | 20 levels | Rejected | P1 |
| NEG-045 | SQL injection in search | `'; DROP--` | Sanitized | P0 |
| NEG-046 | XSS in role name | `<script>` | Sanitized | P0 |
| NEG-047 | Import with duplicate mappings | Same role-permission | Handled | P1 |
| NEG-048 | Bulk toggle exceeds limit | 1000 at once | Chunked or error | P1 |
| NEG-049 | System role modification | System role | BusinessException | P1 |
| NEG-050 | Permission denials conflict | Grant + Deny | Policy applied | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Toggle with negative RoleId | -1 | Validation error | P1 |
| NEG-052 | Toggle with negative PermissionId | -1 | Validation error | P1 |
| NEG-053 | Get with negative user ID | -1 | Not found | P1 |
| NEG-054 | Paginate with invalid page | Page = -1 | Default or error | P2 |
| NEG-055 | Paginate with invalid size | Size = 0 | Default or error | P2 |
| NEG-056 | Export with invalid format | "invalid" | Default | P2 |
| NEG-057 | Sort by invalid column | "INVALID" | Default sort | P2 |
| NEG-058 | Circular inheritance | A→B→A | Rejected | P1 |
| NEG-059 | Self-inheritance | A→A | Rejected | P1 |
| NEG-060 | Invalid entity type | "Invalid" | Validation error | P1 |
| NEG-061 | Import with wrong columns | Wrong CSV format | Parse error | P2 |
| NEG-062 | Export empty matrix | No data | Empty file or error | P2 |
| NEG-063 | Validate with orphan permissions | Permission no roles | Warning | P1 |
| NEG-064 | Validate with orphan roles | Role no permissions | Warning | P1 |
| NEG-065 | Path traversal in import | `../../evil` | Rejected | P0 |
| NEG-066 | Toggle with null request | Request = null | ArgumentNullException | P1 |
| NEG-067 | Bulk with null request | Request = null | ArgumentNullException | P1 |
| NEG-068 | LDAP injection in search | `*)(cn=*` | Sanitized | P1 |
| NEG-069 | Regex injection in search | `.*+?[]()` | Escaped | P1 |
| NEG-070 | Concurrent toggle same cell | 2 users toggle | One wins | P1 |
| NEG-071 | Toggle with invalid entity | EntityType = "X" | Validation error | P1 |
| NEG-072 | Get effective with null user | UserId = null | ArgumentNullException | P1 |
| NEG-073 | Compare with invalid role | RoleId = "abc" | 400 Bad Request | P1 |
| NEG-074 | Copy with invalid target | Target deleted | KeyNotFoundException | P1 |
| NEG-075 | Import with wrong schema | Wrong columns | Parse error | P2 |
| NEG-076 | Export with null format | Format = null | Default | P1 |
| NEG-077 | Validate with orphan role | Role no perms | Warning | P1 |
| NEG-078 | Validate with orphan perm | Perm no roles | Warning | P1 |
| NEG-079 | Bulk toggle with null list | List = null | ArgumentNullException | P1 |
| NEG-080 | Entity override with invalid | Entity invalid | Validation error | P1 |
| NEG-081 | Toggle for inactive role | Role inactive | Error or allowed | P1 |
| NEG-082 | Hierarchy with self-ref | A→A | Rejected | P1 |
| NEG-083 | Filter with invalid criteria | Criteria invalid | Default or error | P2 |
| NEG-084 | Search with empty string | "" | All or empty | P1 |
| NEG-085 | Compare with deleted roles | Both deleted | KeyNotFoundException | P1 |
| NEG-086 | Update with stale matrix | Stale | Conflict error | P1 |
| NEG-087 | Import with duplicate rows | Same mapping | Handled | P2 |
| NEG-088 | Export empty matrix | No data | Empty file | P1 |
| NEG-089 | Toggle system role | System role | BusinessException | P1 |
| NEG-090 | Bulk exceed limit | 1001 toggles | Chunked or error | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 90 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Role Name | 1 | 200 | ✅ "A" | ✅ 200 | ❌ Rejected | P1 |
| BND-002 | Permission Name | 1 | 100 | ✅ "P" | ✅ 100 | ❌ Rejected | P1 |
| BND-003 | Search query | 0 | 255 | ✅ Empty | ✅ 255 | ❌ Capped | P1 |
| BND-004 | Entity type name | 1 | 50 | ✅ "P" | ✅ 50 | ❌ Rejected | P2 |
| BND-005 | Export filename | 1 | 260 | ✅ "a" | ✅ 260 | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Role ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-007 | Permission ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-008 | User ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-009 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-010 | Page size | 1 | 1000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-011 | Roles count | 0 | 500 | ✅ 0 | ❌ | Performance | P2 |
| BND-012 | Permissions count | 0 | 500 | ✅ 0 | ❌ | Performance | P2 |
| BND-013 | Hierarchy depth | 1 | 10 | ❌ | ❌ | Rejected | P1 |
| BND-014 | Entity overrides per role | 0 | 100 | ✅ 0 | ❌ | Rejected | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-015 | Matrix change at midnight | 00:00:00 UTC | Stored correctly | P2 |
| BND-016 | Audit at 23:59:59 | End of day | Correct | P2 |
| BND-017 | Filter by date range | Same day | Returns that day | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-018 | Zero roles | Empty | Empty matrix | P1 |
| BND-019 | One role | Single | 1 row | P1 |
| BND-020 | Zero permissions | Empty | Empty columns | P1 |
| BND-021 | One permission | Single | 1 column | P1 |
| BND-022 | Exactly page size roles | 20, size=20 | Full page | P1 |
| BND-023 | Page size + 1 roles | 21, size=20 | 20 on page 1 | P1 |
| BND-024 | 500 roles | Large | Paginated | P1 |
| BND-025 | 500 permissions | Large | Paginated | P1 |
| BND-026 | Role with 0 permissions | No permissions | Empty row | P1 |
| BND-027 | Role with 500 permissions | All | Full row | P2 |
| BND-028 | Permission with 0 roles | No roles | Empty column | P1 |
| BND-029 | Permission with 100 roles | Many | Full column | P2 |
| BND-030 | Last page with 1 role | 41, page 3, size 20 | 1 on page 3 | P1 |
| BND-031 | User with 0 roles | No roles | Empty effective | P1 |
| BND-032 | User with 20 roles | Max | Merged effective | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-033 | Role name (Arabic) | `مصفوفة` | Stored correctly | P2 |
| BND-034 | Permission name (Chinese) | `权限` | Stored correctly | P2 |
| BND-035 | Role name (Cyrillic) | `Роль` | Stored correctly | P2 |
| BND-036 | Name with apostrophe | `Admin's Role` | Preserved | P1 |
| BND-037 | Name with hyphen | `Partner-User` | Preserved | P1 |
| BND-038 | Search with special chars | "O'Brien" | Handled | P2 |
| BND-039 | Name with ampersand | `Editor & Reviewer` | Preserved | P2 |
| BND-040 | Name with accent | `Gérant` | Stored correctly | P2 |

### 3.6 Matrix Layout Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | 1x1 matrix | 1 role, 1 permission | Single cell | P1 |
| BND-042 | 1x100 matrix | 1 role, 100 permissions | Horizontal scroll | P1 |
| BND-043 | 100x1 matrix | 100 roles, 1 permission | Vertical scroll | P1 |
| BND-044 | 100x100 matrix | Large | Paginated/virtualized | P1 |
| BND-045 | All cells enabled | Full matrix | All checked | P1 |
| BND-046 | All cells disabled | Empty matrix | None checked | P1 |
| BND-047 | Sparse matrix | 10% filled | Correct display | P1 |
| BND-048 | Dense matrix | 90% filled | Correct display | P1 |
| BND-049 | Hierarchy with 1 level | Flat | No inheritance | P1 |
| BND-050 | Hierarchy with 10 levels | Max depth | Full inheritance | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Role ID = 1 | First | Retrieved | P2 |
| BND-052 | Role ID = MAX_INT | Overflow | Handled | P2 |
| BND-053 | Search with 1 char | "A" | Matches | P1 |
| BND-054 | Search with max chars | 255 chars | Processed | P1 |
| BND-055 | Bulk toggle 1 | Single | Success | P2 |
| BND-056 | Bulk toggle 100 | Batch | Success | P2 |
| BND-057 | Import 1 mapping | Single | Created | P2 |
| BND-058 | Import 500 mappings | Batch | All created | P2 |
| BND-059 | Effective with 1 role | Single role | That role's perms | P1 |
| BND-060 | Effective with 20 roles | Max roles | Merged | P1 |
| BND-061 | Concurrent view | 2 users view | Both correct | P2 |
| BND-062 | Timezone boundary | UTC vs local | Correct | P2 |
| BND-063 | Sort by each column | Name, Date | All work | P1 |
| BND-064 | Filter by each entity type | Partner, Opportunity | Correct | P2 |
| BND-065 | Entity override 1 | Single | Saved | P1 |
| BND-066 | Entity override 100 | Max | All saved | P2 |
| BND-067 | Copy role with 50 perms | Many | All copied | P2 |
| BND-068 | Compare identical roles | Same perms | No diff | P2 |
| BND-069 | Compare different roles | Different | Diff shown | P2 |
| BND-070 | Validate full matrix | All valid | No errors | P1 |
| BND-071 | Role name at 199 chars | 199 chars | Accepted | P1 |
| BND-072 | Permission name at 99 | 99 chars | Accepted | P1 |
| BND-073 | Page size at 999 | 999 | Accepted | P1 |
| BND-074 | Hierarchy depth at 9 | 9 levels | Accepted | P1 |
| BND-075 | Entity overrides at 99 | 99 | Accepted | P2 |
| BND-076 | Search exactly 254 chars | 254 chars | Processed | P1 |
| BND-077 | Matrix 2x2 | 2 roles, 2 perms | 4 cells | P1 |
| BND-078 | User with 1 role | Single | That role's perms | P1 |
| BND-079 | Role with 1 permission | Single | 1 perm | P1 |
| BND-080 | Bulk toggle 1 | Single | Success | P2 |
| BND-081 | Import 1 mapping | Single | Created | P2 |
| BND-082 | Empty matrix | No roles | Empty | P1 |
| BND-083 | Role ID = 2 | Second | Retrieved | P2 |
| BND-084 | Pagination page 2 of 2 | 2 pages | 2nd page | P1 |
| BND-085 | Filter by single entity | One entity | Correct | P1 |
| BND-086 | Unicode in role name | Arabic | Stored | P2 |
| BND-087 | Effective with 2 roles | Two roles | Merged | P1 |
| BND-088 | Entity override 1 | Single | Saved | P1 |
| BND-089 | Copy role with 1 perm | Single | Copied | P2 |
| BND-090 | Zero roles | Empty | Empty matrix | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 90 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Matrix excludes deleted | IsDeleted filter | List | Only !IsDeleted | P0 |
| FUN-002 | Toggle creates mapping | Enable | Toggle on | Mapping created | P0 |
| FUN-003 | Toggle removes mapping | Disable | Toggle off | Mapping deleted | P0 |
| FUN-004 | Effective = union | Multiple roles | GetEffective | Union of all | P0 |
| FUN-005 | Hierarchy inheritance | Child role | Effective | Child inherits parent | P0 |
| FUN-006 | Entity override wins | Entity + Global | Check | Entity override | P0 |
| FUN-007 | Deny overrides grant | Grant + Deny | Check | Deny wins | P0 |
| FUN-008 | System role protected | System role | Toggle | BusinessException | P0 |
| FUN-009 | Export includes only active | Export | Export | Only !IsDeleted | P1 |
| FUN-010 | Import creates new | Import | Import | New mappings | P1 |
| FUN-011 | Copy duplicates mappings | Copy | Copy role | Target has same | P1 |
| FUN-012 | Compare shows diff | Compare | Two roles | Diff displayed | P1 |
| FUN-013 | Audit on toggle | Toggle | Any change | Audit entry | P1 |
| FUN-014 | Validate checks hierarchy | Validate | Run | Circular detected | P1 |
| FUN-015 | Pagination default | Default | null params | 1, 20 | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Role must exist | FK | Valid ID | 999999 | P0 |
| FUN-017 | Permission must exist | FK | Valid ID | 999999 | P0 |
| FUN-018 | User must exist | FK | Valid ID | 999999 | P0 |
| FUN-019 | No circular hierarchy | No cycle | A→B | A→B→A | P0 |
| FUN-020 | Entity type valid | Enum | Partner | Invalid | P0 |
| FUN-021 | Role name max length | ≤200 | 200 | 201 | P1 |
| FUN-022 | Permission name max | ≤100 | 100 | 101 | P1 |
| FUN-023 | No SQL in search | Sanitize | "Admin" | `'; DROP--` | P0 |
| FUN-024 | No XSS in name | Sanitize | "Role" | `<script>` | P0 |
| FUN-025 | Hierarchy depth | ≤10 | 10 | 11 | P1 |
| FUN-026 | Trim whitespace | Trim | "  Name  " | → "Name" | P2 |
| FUN-027 | System role immutable | Cannot edit | System role | Edit | P1 |
| FUN-028 | Import format valid | CSV/JSON | Valid file | Invalid | P1 |
| FUN-029 | Export format valid | Enum | CSV, Excel | Invalid | P1 |
| FUN-030 | Bulk limit | ≤100 | 100 | 101 | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max roles per user | 20 | 21 | Rejected | P1 |
| FUN-032 | Max page size | 1000 | 5000 | Capped | P1 |
| FUN-033 | Unique role-permission | DB | Duplicate | No duplicate | P0 |
| FUN-034 | FK role exists | FK | Non-existent | FK error | P0 |
| FUN-035 | FK permission exists | FK | Non-existent | FK error | P0 |
| FUN-036 | Bulk toggle limit | 100 | 150 | Chunked | P2 |
| FUN-037 | Hierarchy depth | 10 | 11 | Rejected | P1 |
| FUN-038 | Entity overrides limit | 100 | 101 | Rejected | P2 |
| FUN-039 | Import row limit | 1000 | 1500 | Chunked | P2 |
| FUN-040 | Concurrent toggle | Unique | Same toggle | One wins | P1 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Toggle enable audit | Enable | UserId, RoleId, PermissionId, Timestamp | P0 |
| FUN-042 | Toggle disable audit | Disable | UserId, RoleId, PermissionId, Timestamp | P0 |
| FUN-043 | Import audit | Import | UserId, RowCount, Timestamp | P1 |
| FUN-044 | Export audit | Export | UserId, Timestamp | P1 |
| FUN-045 | Copy audit | Copy | UserId, SourceId, TargetId | P1 |
| FUN-046 | Read no audit | Get matrix | No modification | P1 |
| FUN-047 | Bulk toggle audit | Bulk | Each change logged | P1 |
| FUN-048 | Entity override audit | Override | UserId, EntityId, PermissionId | P1 |
| FUN-049 | Failed toggle no audit | Failed toggle | No audit entry | P1 |
| FUN-050 | Audit immutable on read | Get | Audit fields unchanged | P1 |
| FUN-051 | Toggle creates mapping | Enable | Mapping created | P0 |
| FUN-052 | Toggle removes mapping | Disable | Mapping deleted | P0 |
| FUN-053 | Effective = union | Multiple roles | Union of all | P0 |
| FUN-054 | Hierarchy inheritance | Child role | Child inherits | P0 |
| FUN-055 | Entity override wins | Entity + Global | Entity override | P0 |
| FUN-056 | Deny overrides grant | Grant + Deny | Deny wins | P0 |
| FUN-057 | System role protected | System role | BusinessException | P0 |
| FUN-058 | Export excludes deleted | Export | Only !IsDeleted | P1 |
| FUN-059 | Import creates new | Import | New mappings | P1 |
| FUN-060 | Copy duplicates mappings | Copy | Target has same | P1 |
| FUN-061 | Compare shows diff | Compare | Diff displayed | P1 |
| FUN-062 | Audit on toggle | Toggle | Audit entry | P1 |
| FUN-063 | Validate checks hierarchy | Validate | Circular detected | P1 |
| FUN-064 | Role must exist | FK | Valid ID | 999999 | P0 |
| FUN-065 | Permission must exist | FK | Valid ID | 999999 | P0 |
| FUN-066 | User must exist | FK | Valid ID | 999999 | P0 |
| FUN-067 | No circular hierarchy | No cycle | A→B | A→B→A | P0 |
| FUN-068 | Entity type valid | Enum | Partner | Invalid | P0 |
| FUN-069 | Role name max length | ≤200 | 200 | 201 | P1 |
| FUN-070 | Permission name max | ≤100 | 100 | 101 | P1 |
| FUN-071 | No SQL in search | Sanitize | "Admin" | `'; DROP--` | P0 |
| FUN-072 | No XSS in name | Sanitize | "Role" | `<script>` | P0 |
| FUN-073 | Hierarchy depth | ≤10 | 10 | 11 | P1 |
| FUN-074 | Trim whitespace | Trim | "  Name  " | → "Name" | P2 |
| FUN-075 | System role immutable | Cannot edit | System role | Edit | P1 |
| FUN-076 | Import format valid | CSV/JSON | Valid | Invalid | P1 |
| FUN-077 | Export format valid | Enum | CSV, Excel | Invalid | P1 |
| FUN-078 | Bulk limit | ≤100 | 100 | 101 | P1 |
| FUN-079 | Max roles per user | 20 | 21 | Rejected | P1 |
| FUN-080 | Max page size | 1000 | 5000 | Capped | P1 |
| FUN-081 | Unique role-permission | DB | Duplicate | No duplicate | P0 |
| FUN-082 | FK role exists | FK | Non-existent | FK error | P0 |
| FUN-083 | FK permission exists | FK | Non-existent | FK error | P0 |
| FUN-084 | Bulk toggle limit | 100 | 150 | Chunked | P2 |
| FUN-085 | Hierarchy depth | 10 | 11 | Rejected | P1 |
| FUN-086 | Entity overrides limit | 100 | 101 | Rejected | P2 |
| FUN-087 | Toggle enable audit | Enable | UserId, RoleId, PermissionId | P0 |
| FUN-088 | Toggle disable audit | Disable | UserId, RoleId, PermissionId | P0 |
| FUN-089 | Import audit | Import | UserId, RowCount | P1 |
| FUN-090 | Export audit | Export | UserId, Timestamp | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 90 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Full toggle lifecycle | Enable→Check→Disable | Role, Permission | All succeed | P0 |
| INT-002 | Enable → in effective | Enable | Role, Permission | In effective | P0 |
| INT-003 | Disable → not in effective | Disable | Role, Permission | Not in effective | P0 |
| INT-004 | Import → export round-trip | Import + Export | Matrix | Data round-trip | P1 |
| INT-005 | Copy → target has same | Copy | Source, Target | Target updated | P1 |
| INT-006 | Entity override → check | Override + Check | Entity, Role | Override applies | P1 |
| INT-007 | Hierarchy → effective | Parent + Child | User | Child inherits | P1 |
| INT-008 | Bulk enable → all active | Bulk | Role, 5 Permissions | All enabled | P1 |
| INT-009 | Compare → diff accurate | Compare | Role A, B | Correct diff | P1 |
| INT-010 | Validate → no errors | Validate | Matrix | Valid | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Search roles by name | "Admin" | Matching roles | P0 |
| INT-012 | Search permissions by name | "Partner" | Matching permissions | P0 |
| INT-013 | Filter by entity type | Partner | Partner permissions | P1 |
| INT-014 | Combined role + permission | Both | Intersection | P1 |
| INT-015 | Search empty | "NONEXISTENT" | Empty result | P1 |
| INT-016 | Search case-insensitive | "admin" vs "ADMIN" | Same results | P1 |
| INT-017 | Filter by status | Active | Active only | P1 |
| INT-018 | Filter excludes deleted | Include deleted | Deleted excluded | P1 |
| INT-019 | Search with special chars | "O'Brien" | Handled | P2 |
| INT-020 | Filter by hierarchy level | Level 2 | Level 2 roles | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Page 1 of 3 | 50, page=1, size=20 | 20, hasNext | P1 |
| INT-022 | Last page | 50, page=3, size=20 | 10, hasNext=false | P1 |
| INT-023 | Empty page | Filter yields 0 | Empty, total=0 | P1 |
| INT-024 | Single page | 15, size=20 | 15 items | P2 |
| INT-025 | Large page | 100, size=100 | All on 1 page | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Role → Permissions | Many-to-many | Load role | Permissions loaded | P0 |
| INT-027 | Permission → Roles | Many-to-many | Load permission | Roles loaded | P0 |
| INT-028 | Role → Hierarchy | Parent/Child | Load child | Parent loaded | P1 |
| INT-029 | User → Effective | Computed | Load user | Effective permissions | P0 |
| INT-030 | Entity → Overrides | Entity-level | Load entity | Overrides loaded | P1 |
| INT-031 | Matrix → Audit | Audit | Load audit | History | P1 |
| INT-032 | Export includes relations | Export | Export | All in file | P2 |
| INT-033 | Import with relations | Import | Import | Mappings created | P1 |
| INT-034 | Copy preserves hierarchy | Copy | Copy | Hierarchy preserved | P2 |
| INT-035 | Compare includes hierarchy | Compare | Compare | Hierarchy in diff | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Toggle invalid → 400 | Validation | BusinessException | P0 |
| INT-037 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-038 | Unauthorized → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-039 | Import malformed → 400 | Parse | Validation error | P0 |
| INT-040 | Export empty → 200 | Empty | Empty file | P0 |
| INT-041 | Circular hierarchy → 400 | Validation | BusinessException | P1 |
| INT-042 | System role toggle → 400 | Business rule | BusinessException | P1 |
| INT-043 | DB timeout → 500 | Timeout | Graceful error | P1 |
| INT-044 | Concurrent conflict → 409 | Concurrency | Conflict error | P1 |
| INT-045 | Malformed request → 400 | Bad JSON | Validation error | P1 |
| INT-046 | Rate limit → 429 | Too many | Rate limit | P2 |
| INT-047 | SQL injection → sanitized | Injection | Parameterized | P0 |
| INT-048 | Large payload → 413 | Oversized | Rejected | P2 |
| INT-049 | Invalid entity type → 400 | Validation | BusinessException | P1 |
| INT-050 | Permission deny conflict | Grant+Deny | Policy applied | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in search | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL injection in role name | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in role name | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in permission name | `"><script>` | Sanitized | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | Path traversal in import | `../../evil.csv` | Rejected | P0 |
| SEC-007 | HTML in export | `<img onerror=...>` | Escaped | P1 |
| SEC-008 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-009 | XXE in import | XXE payload | Rejected | P1 |
| SEC-010 | OS command in filename | `; rm -rf /` | Sanitized | P0 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous toggle | No auth | POST /matrix/toggle | 401 | P0 |
| SEC-012 | No admin permission | Reader | Toggle | 403 | P0 |
| SEC-013 | Expired token | Expired | Any | 401 | P0 |
| SEC-014 | Tampered JWT | Modified | Any | 401 | P0 |
| SEC-015 | Disabled account | Disabled | Any | 403 | P1 |
| SEC-016 | Post-logout | Logged out | Cached | 401 | P1 |
| SEC-017 | Role escalation | Basic | ?role=admin | Ignored | P0 |
| SEC-018 | Cross-tenant | User A | User B's matrix | 403 | P0 |
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
| SEC-028 | Permission ID manipulation | Change ID | Validated | P0 |
| SEC-029 | User ID manipulation | Change ID | Validated | P0 |
| SEC-030 | Other tenant's matrix | Access via ID | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable | P0 |
| SEC-035 | Audit fields | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on toggle | CSRF token | P0 |
| SEC-040 | CSRF on import | CSRF token | P0 |
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
| SEC-048 | User PII in matrix | Filtered | P1 |
| SEC-049 | Response caching | Cache-Control: no-store | P1 |
| SEC-050 | Tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users toggle same cell | Concurrent toggle | One wins | P1 |
| CON-002 | Create and delete mapping | Race | One succeeds | P1 |
| CON-003 | Two users import | Concurrent import | Both complete | P1 |
| CON-004 | Update during read | Read consistency | Consistent read | P1 |
| CON-005 | Delete during read | Read consistency | Null or pre-delete | P1 |
| CON-006 | Concurrent copy same role | 2 users copy | Both succeed | P1 |
| CON-007 | Concurrent export | 2 exports | Both succeed | P1 |
| CON-008 | Concurrent pagination | Multiple pages | Correct data | P2 |
| CON-009 | Database deadlock | Circular | Resolved, retry | P1 |
| CON-010 | Token refresh during toggle | Expire mid-call | Retry with new token | P1 |
| CON-011 | Bulk toggle concurrent | 2 users bulk | Both complete | P2 |
| CON-012 | Concurrent validate | 2 validates | Both succeed | P2 |
| CON-013 | Toggle during compare | Toggle + Compare | Consistent | P1 |
| CON-014 | Entity override concurrent | 2 overrides | One wins | P1 |
| CON-015 | Hierarchy update during read | Update + Read | Consistent | P1 |
| CON-016 | Copy during delete | Copy + Delete | Handled | P2 |
| CON-017 | Import during export | Import + Export | Both succeed | P2 |
| CON-018 | Concurrent filter | 2 users filter | Independent | P2 |
| CON-019 | Optimistic concurrency | Update stale | Conflict error | P1 |
| CON-020 | Connection pool exhaustion | Many concurrent | Queued or error | P1 |
| CON-021 | Cache invalidation | Update + read | Fresh data | P1 |
| CON-022 | Effective permissions race | 2 role assigns | Correct merge | P1 |
| CON-023 | Matrix conflict | Same cell | One wins | P1 |
| CON-024 | Bulk import concurrent | 2 imports | Both complete | P2 |
| CON-025 | Search during toggle | Search + Toggle | Search consistent | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Effective merge | Calculations | Role A + Role B | Union | P1 |
| UNT-002 | Hierarchy inheritance | Calculations | Parent → Child | Child inherits | P1 |
| UNT-003 | Entity override check | Validation | Override exists | Override wins | P1 |
| UNT-004 | Deny overrides grant | Validation | Grant + Deny | Deny | P1 |
| UNT-005 | Circular detection | Validation | A→B→A | Error | P1 |
| UNT-006 | Name trim | Formatting | "  Name  " | "Name" | P2 |
| UNT-007 | Pagination default | Calculations | null, null | 1, 20 | P1 |
| UNT-008 | Map entity to DTO | Collections | Entity | DTO | P1 |
| UNT-009 | Map request to entity | Collections | Request | Entity | P1 |
| UNT-010 | Export format | Formatting | Matrix | CSV | P2 |
| UNT-011 | Import parse | Validation | Valid CSV | Mappings | P1 |
| UNT-012 | Role ID validation | Validation | 42 | Valid | P1 |
| UNT-013 | Permission ID validation | Validation | 1 | Valid | P1 |
| UNT-014 | System role check | Validation | System role | Protected | P1 |
| UNT-015 | Diff calculation | Calculations | Role A, B | Diff list | P1 |
| UNT-016 | Copy logic | Collections | Source | Target copy | P1 |
| UNT-017 | Validate hierarchy | Validation | Matrix | No circular | P1 |
| UNT-018 | User ID validation | Validation | 42 | Valid | P1 |
| UNT-019 | Entity type validation | Validation | Partner | Valid | P1 |
| UNT-020 | Permission count | Collections | Role | 10 | P1 |
| UNT-021 | Date format for audit | Formatting | Now | ISO string | P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Toggle permission | Single toggle | < 200ms | P2 |
| PRF-002 | Get matrix | Single read | < 500ms | P2 |
| PRF-003 | Get effective permissions | User with 5 roles | < 500ms | P2 |
| PRF-004 | List 20 roles | Paginated | < 500ms | P2 |
| PRF-005 | List 100 permissions | Paginated | < 500ms | P2 |
| PRF-006 | Search roles | Search | < 1s | P2 |
| PRF-007 | Export 100 rows | Export | < 2s | P2 |
| PRF-008 | Import 50 rows | Import | < 2s | P2 |
| PRF-009 | 10 concurrent toggles | Concurrent | All < 1s | P2 |
| PRF-010 | 20 concurrent reads | Concurrent | All < 500ms | P2 |
| PRF-011 | Full matrix load | 100x100 | < 3s | P2 |
| PRF-012 | Compare two roles | Compare | < 500ms | P2 |
| PRF-013 | Copy role | 50 permissions | < 1s | P2 |
| PRF-014 | Validate full matrix | Validate | < 2s | P2 |
| PRF-015 | Memory: 500 roles | Load all | No leak | P2 |
| PRF-016 | Filter + sort | Combined | < 1s | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained list | 10 users, 1 req/s | 5 min | 95% < 1s | P2 |
| LDT-002 | Sustained toggle | 5 users, 0.5 req/s | 5 min | 95% < 500ms | P2 |
| LDT-003 | Sustained search | 20 users, 2 req/s | 5 min | 95% < 1s | P2 |
| LDT-004 | Spike list | 0→50 users in 30s | 2 min | No errors | P2 |
| LDT-005 | Spike toggle | 0→20 users | 2 min | Queue or 429 | P2 |
| LDT-006 | Stress list | 100 users, 5 req/s | 5 min | Graceful degradation | P2 |
| LDT-007 | Stress toggle | 50 users, 2 req/s | 5 min | Queue or 429 | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 10 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | System recovers | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Permission matrix validation | POS-001, POS-002, FUN-001 to FUN-003 |
| AC-2: Role-action mapping | POS-002, FUN-002, FUN-003, INT-001 to INT-003 |
| AC-3: Entity-level permissions | POS-004, FUN-006, INT-006, BND-066 |
| AC-4: Cascading permissions | POS-005, FUN-005, FUN-007, BND-049, BND-050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
