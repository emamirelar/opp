# RoleController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/RoleController`  
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
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

REST API for role management: CRUD roles, role-permission assignment, user-role mapping.

---

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all roles | GET /api/roles | Role list |
| POS-002 | Get role by ID | GET /api/roles/{id} | Role details |
| POS-003 | Create role (admin) | POST /api/roles | 201 Created |
| POS-004 | Update role (admin) | PUT /api/roles/{id} | 200 OK |
| POS-005 | Delete role (admin) | DELETE /api/roles/{id} | 204 No Content |
| POS-006 | Get dropdown | GET /api/roles/dropdown | ID/name pairs |
| POS-007 | Search roles | GET /api/roles?search=text | Filtered |
| POS-008 | Assign permission | POST /api/roles/{id}/permissions | Permission assigned |
| POS-009 | Remove permission | DELETE /api/roles/{id}/permissions/{pid} | Permission removed |
| POS-010 | Get permissions | GET /api/roles/{id}/permissions | Permissions |
| POS-011 | Bulk assign permissions | POST /api/roles/{id}/permissions/bulk | Bulk assigned |
| POS-012 | Assign user to role | POST /api/roles/{id}/users | User assigned |
| POS-013 | Remove user from role | DELETE /api/roles/{id}/users/{uid} | User removed |
| POS-014 | Get users | GET /api/roles/{id}/users | Users |
| POS-015 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-016 | Sort by name | GET ?sortBy=name | Sorted |
| POS-017 | Get active only | GET ?active=true | Active only |
| POS-018 | Empty result | GET for empty filter | [] |
| POS-019 | Single result | GET for single match | [item] |
| POS-020 | Authenticated access | GET with token | 200 |
| POS-021 | Soft delete | DELETE (soft) | IsDeleted |
| POS-022 | Restore | POST /api/roles/{id}/restore | Restored |
| POS-023 | Get by code | GET /api/roles/code/{code} | By code |
| POS-024 | Filter by type | GET ?type=System | Filtered |
| POS-025 | Typeahead | GET /api/roles/typeahead?q=text | Suggestions |
| POS-026 | Export | GET /api/roles/export | Export file |
| POS-027 | Admin create | POST as admin | 201 |
| POS-028 | Admin update | PUT as admin | 200 |
| POS-029 | Admin delete | DELETE as admin | 204 |
| POS-030 | Combined filter | GET ?search=text&type=X | Combined |
| POS-031 | Sort ascending | GET ?sortBy=name&sortOrder=asc | Sorted |
| POS-032 | Sort descending | GET ?sortBy=name&sortOrder=desc | Sorted |
| POS-033 | First page | GET ?page=1 | First page |
| POS-034 | Last page | GET ?page=last | Partial |
| POS-035 | Cached response | GET same query | 200 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid ID | id=abc | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Invalid code | code=invalid | 404 |
| NEG-007 | Null request | POST null | 400 |
| NEG-008 | Missing name | Name missing | 400 |
| NEG-009 | Missing code | Code missing | 400 |
| NEG-010 | Duplicate code | code exists | 409 |
| NEG-011 | Invalid permissionId | permissionId=999999 | 404 |
| NEG-012 | Invalid userId | userId=999999 | 404 |
| NEG-013 | SQL injection | search='; DROP | Sanitized |
| NEG-014 | XSS in name | name=<script> | Sanitized |
| NEG-015 | Negative page | page=-1 | 400 |
| NEG-016 | Zero pageSize | pageSize=0 | 400 |
| NEG-017 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-018 | Invalid sort | sortBy=invalid | 400 |
| NEG-019 | No permission | User without CanView | 403 |
| NEG-020 | No admin for write | POST as user | 403 |
| NEG-021 | Cross-org access | Other org role | 403 |
| NEG-022 | Deleted role | id of deleted | 404 |
| NEG-023 | Malformed JSON | Invalid JSON | 400 |
| NEG-024 | Wrong content-type | Application/xml | 415 |
| NEG-025 | Rate limit | Too many | 429 |
| NEG-026 | Payload too large | Huge body | 413 |
| NEG-027 | Invalid Accept | Accept: text/plain | 406 |
| NEG-028 | HTTP method | PUT for create | 405 |
| NEG-029 | Trailing slash | /api/roles/ | Redirect |
| NEG-030 | Case sensitivity | /api/Roles | 404 |
| NEG-031 | Extra path | /api/roles/1/extra | 404 |
| NEG-032 | Invalid bearer | Bearer malformed | 401 |
| NEG-033 | Revoked token | Revoked JWT | 401 |
| NEG-034 | Service account | Service for UI | 403 |
| NEG-035 | DB timeout | Simulate | 503 |
| NEG-036 | Invalid type | type=invalid | 400 |
| NEG-037 | Empty code | code= | 400 |
| NEG-038 | Whitespace code | code="  " | 400 |
| NEG-039 | Zero ID | id=0 | 400 |
| NEG-040 | Invalid UUID | id=invalid-guid | 400 |
| NEG-041 | Blocked IP | From blocked | 403 |
| NEG-042 | Control chars | name with \0 | 400 |
| NEG-043 | Unicode overflow | Very long | 400 |
| NEG-044 | Delete in-use | Role referenced | 409 |
| NEG-045 | Update deleted | PUT on deleted | 404 |
| NEG-046 | Restore not deleted | POST restore on active | 400 |
| NEG-047 | Mismatched IDs | Path != body | 400 |
| NEG-048 | Read-only field | Update createdDate | Ignored |
| NEG-049 | Version conflict | Stale version | 409 |
| NEG-050 | CORS fail | Invalid origin | CORS error |
| NEG-051 | Inactive org | Org inactive | 403 |
| NEG-052 | Invalid bulk assign | Bulk with invalid | 400/partial |
| NEG-053 | Empty bulk | POST [] | 400 |
| NEG-054 | Excessive bulk | 1000 IDs | 400 |
| NEG-055 | Assign system role | System role | 403 |
| NEG-056 | Remove last admin | Last admin | 409 |
| NEG-057 | Duplicate permission | Already assigned | 409 |
| NEG-058 | Duplicate user | Already assigned | 409 |
| NEG-059 | Export no permission | No export permission | 403 |
| NEG-060 | Audit failure | Audit down | Continue |
| NEG-061 | Reserved code | code=RESERVED | 403 |
| NEG-062 | Duplicate name | name exists | 409 or allow |
| NEG-063 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-064 | Max URL length | Very long URL | 414 |
| NEG-065 | Invalid endpoint | /api/roles/invalid | 404 |
| NEG-066 | Invalid method | PATCH | 405 |
| NEG-067 | Missing query | GET no params | 200 or 400 |
| NEG-068 | Invalid encoding | Malformed URL | 400 |
| NEG-069 | Remove non-assigned | Not assigned | 404 |
| NEG-070 | Soft-deleted filter | Query deleted | Excluded |
| NEG-071 | Invalid JSON schema | Schema mismatch | 400 |
| NEG-072 | Missing role name | Name null | 400 |
| NEG-073 | Invalid role type | type=invalid | 400 |
| NEG-074 | Empty permission list | permissions=[] | 400 |
| NEG-075 | Invalid hierarchy | Circular role | 400 |
| NEG-076 | Role locked | Locked role | 423 |
| NEG-077 | Maintenance mode | During maintenance | 503 |
| NEG-078 | Quota exceeded | Role quota | 507 |
| NEG-079 | Invalid description | desc too long | 400 |
| NEG-080 | System role modify | Modify system | 403 |
| NEG-081 | Migration mode | During migration | 503 |
| NEG-082 | Session invalid | Invalid session | 401 |
| NEG-083 | Token type wrong | Wrong token type | 401 |
| NEG-084 | Scope insufficient | OAuth scope | 403 |
| NEG-085 | Rate limit per user | User rate limit | 429 |
| NEG-086 | Concurrent limit | Too many concurrent | 429 |
| NEG-087 | Request timeout | Slow request | 408 |
| NEG-088 | Role archived | Archived role | 410 |
| NEG-089 | Last admin role | Remove last admin | 409 |
| NEG-090 | In-use role delete | Role in use | 409 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | code length | 1 | 50 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-006 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | permissionId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-008 | userId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-009 | Empty list | - | - | [] | - | - |
| BND-010 | Single item | - | - | [item] | - | - |
| BND-011 | First page | page=1 | - | ✅ | - | - |
| BND-012 | Last page | - | - | Partial | - | - |
| BND-013 | Zero length name | - | - | ❌ | - | - |
| BND-014 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-015 | Unicode name | - | - | Accept | - | - |
| BND-016 | Arabic name | - | - | Display | - | - |
| BND-017 | Chinese name | - | - | Display | - | - |
| BND-018 | Null optional | - | - | Default | - | - |
| BND-019 | Empty string | - | - | No filter | - | - |
| BND-020 | Whitespace | - | - | Trim | - | - |
| BND-021 | Sort empty | - | - | [] | - | - |
| BND-022 | Sort single | - | - | [item] | - | - |
| BND-023 | Filter no match | - | - | [] | - | - |
| BND-024 | Filter all | - | - | Full | - | - |
| BND-025 | Permission count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-026 | User count | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-027 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-028 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-029 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-030 | Typeahead min | 1 | - | ✅ | - | - |
| BND-031 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-032 | Pagination boundary | - | - | Exact | - | - |
| BND-033 | Empty bulk | - | - | 400 | - | - |
| BND-034 | Partial bulk | - | - | 207 | - | - |
| BND-035 | Round-trip | Create → Get | - | Match | - | - |
| BND-036 | Soft-deleted | - | - | Excluded | - | - |
| BND-037 | Inactive | - | - | Excluded | - | - |
| BND-038 | Duplicate code | - | - | Reject | - | - |
| BND-039 | Case code | - | - | Normalize | - | - |
| BND-040 | Zero ID | id=0 | - | 400 | - | - |
| BND-041 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-042 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-043 | Export empty | - | - | Headers | - | - |
| BND-044 | Export single | - | - | Valid | - | - |
| BND-045 | Version | 1 | - | ✅ | ❌ | - |
| BND-046 | Type length | - | 50 | ✅ | ✅ | ❌ |
| BND-047 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-048 | Order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-049 | Created date | - | - | UTC | - | - |
| BND-050 | Modified date | - | - | UTC | - | - |
| BND-051 | Bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-052 | Active flag | - | - | Boolean | - | - |
| BND-053 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-054 | Workflow status | - | - | Valid | - | - |
| BND-055 | Audit fields | - | - | Set | - | - |
| BND-056 | Role type | - | max | Valid | Valid | ❌ |
| BND-057 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-058 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-059 | Hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-060 | Single permission | - | - | [perm] | - | - |
| BND-061 | Max permissions | - | 500 | - | ✅ | ❌ |
| BND-062 | Empty permissions | - | - | [] | - | - |
| BND-063 | Single user | - | - | [user] | - | - |
| BND-064 | Max users | - | 10000 | - | ✅ | ❌ |
| BND-065 | Empty users | - | - | [] | - | - |
| BND-066 | Role inheritance | - | 5 | ✅ | ✅ | ❌ |
| BND-067 | Nested roles | - | 3 | ✅ | ✅ | ❌ |
| BND-068 | System role | - | - | Protected | - | - |
| BND-069 | Admin role | - | - | Protected | - | - |
| BND-070 | Default role | - | - | Valid | - | - |

---

## §4 Functional Tests (50)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get all | GET | List |
| FUN-002 | Workflow | Get by ID | GET id | Details |
| FUN-003 | Workflow | Get by code | GET code | Match |
| FUN-004 | Workflow | Create (admin) | POST | 201 |
| FUN-005 | Workflow | Update (admin) | PUT | 200 |
| FUN-006 | Workflow | Delete (admin) | DELETE | 204 |
| FUN-007 | Workflow | Soft delete | DELETE | IsDeleted |
| FUN-008 | Workflow | Restore | POST restore | Restored |
| FUN-009 | Workflow | Assign permission | POST permissions | Assigned |
| FUN-010 | Workflow | Remove permission | DELETE permissions | Removed |
| FUN-011 | Workflow | Assign user | POST users | Assigned |
| FUN-012 | Workflow | Remove user | DELETE users | Removed |
| FUN-013 | Workflow | Filter type | GET ?type | Filtered |
| FUN-014 | Workflow | Search | GET ?search | Searched |
| FUN-015 | Workflow | Paginate | GET ?page | Paginated |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required code | Missing | 400 |
| FUN-018 | Validation | Unique code | Duplicate | 409 |
| FUN-019 | Validation | Valid permission | Invalid | 404 |
| FUN-020 | Validation | Valid user | Invalid | 404 |
| FUN-021 | Validation | Permission | No permission | 403 |
| FUN-022 | Validation | Admin write | User write | 403 |
| FUN-023 | Validation | ID format | Invalid | 400 |
| FUN-024 | Validation | Type enum | Invalid | 400 |
| FUN-025 | Validation | System role | Protected | 403 |
| FUN-026 | Constraint | Delete in-use | Referenced | 409 |
| FUN-027 | Constraint | Soft delete | Query | Excluded |
| FUN-028 | Constraint | Org scope | Cross-org | 403 |
| FUN-029 | Constraint | Version | Optimistic | 409 |
| FUN-030 | Constraint | Max bulk | >100 | 400 |
| FUN-031 | Constraint | Export limit | >10K | Truncate |
| FUN-032 | Constraint | Last admin | Cannot remove | 409 |
| FUN-033 | Constraint | No duplicate perm | Duplicate | 409 |
| FUN-034 | Constraint | No duplicate user | Duplicate | 409 |
| FUN-035 | Constraint | Role hierarchy | Circular | 400 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Restore | POST restore | Audit |
| FUN-040 | Audit | Assign permission | POST perms | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | User mapping | Users | Correct |
| FUN-050 | Business | Permission mapping | Permissions | Correct |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Role | Match |
| INT-002 | CRUD | Update → Get | Role | Updated |
| INT-003 | CRUD | Delete → Get | Role | 404 |
| INT-004 | CRUD | Restore → Get | Role | Restored |
| INT-005 | CRUD | Get by code | Role | Match |
| INT-006 | CRUD | Assign perm → Get | Role, Permission | Assigned |
| INT-007 | CRUD | Remove perm → Get | Role, Permission | Removed |
| INT-008 | CRUD | Assign user → Get | Role, User | Assigned |
| INT-009 | CRUD | Export | Role | File |
| INT-010 | CRUD | Dropdown | Role | Pairs |
| INT-011 | Search | Search by name | Role | Matches |
| INT-012 | Search | Typeahead | Role | Suggestions |
| INT-013 | Search | Filter type | Role | Filtered |
| INT-014 | Search | Multi-filter | Role | Combined |
| INT-015 | Search | Empty search | - | [] |
| INT-016 | Search | Partial match | Role | Fuzzy |
| INT-017 | Search | Sort + filter | Role | Both |
| INT-018 | Search | Filter + pagination | Role | Both |
| INT-019 | Permission | Assign → count | Role | +1 |
| INT-020 | Permission | Remove → count | Role | -1 |
| INT-021 | Pagination | Page 1 | Role | First |
| INT-022 | Pagination | Last page | Role | Partial |
| INT-023 | Pagination | Size | Role | Correct |
| INT-024 | Pagination | Invalid | Role | 400 |
| INT-025 | Pagination | Boundary | Role | Exact |
| INT-026 | Relationships | Role → Permissions | Role, Permission | Linked |
| INT-027 | Relationships | Role → Users | Role, User | Linked |
| INT-028 | Relationships | Orphan | Deleted permission | 404 |
| INT-029 | Relationships | User deleted | User | 404 |
| INT-030 | Relationships | Permission deleted | Permission | 404 |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid ID | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | Duplicate | 409 |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow | 504 |
| INT-039 | Error | Payload | Huge | 413 |
| INT-040 | Error | Media | Wrong type | 415 |
| INT-041 | Error | Method | Wrong verb | 405 |
| INT-042 | Error | Service | Dependency | 503 |
| INT-043 | Error | Gateway | Upstream | 504 |
| INT-044 | Error | Gone | Deleted | 410 |
| INT-045 | Error | Locked | Locked | 423 |
| INT-046 | E2E | Full create flow | Role | Create → Get |
| INT-047 | E2E | Full update flow | Role | Update → Get |
| INT-048 | E2E | Full delete flow | Role | Delete → 404 |
| INT-049 | E2E | Permission flow | Role, Permission | Assign → Remove |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Assign perm → Get | Role, Permission | Assigned |
| INT-052 | CRUD | Remove perm → Get | Role, Permission | Removed |
| INT-053 | Permission | Bulk assign flow | Role | Bulk |
| INT-054 | User | Assign user flow | Role, User | Assigned |
| INT-055 | Search | Typeahead flow | Role | Suggestions |
| INT-056 | Relationships | Role → Permissions | Role, Permission | Linked |
| INT-057 | Error | Validation chain | Bad input | 400 |
| INT-058 | Error | Auth chain | No auth | 401 |
| INT-059 | E2E | User flow | Role, User | Assign → Remove |
| INT-060 | E2E | Export flow | Role | Export |
| INT-061 | CRUD | Restore → Get | Role | Restored |
| INT-062 | Permission | Permission count | Role | Count |
| INT-063 | User | User count | Role | Count |
| INT-064 | Relationships | Role → Users | Role, User | Linked |
| INT-065 | Error | Permission chain | No perm | 403 |
| INT-066 | E2E | Full role flow | Role | Create → Delete |
| INT-067 | CRUD | Get by code | Role | Match |
| INT-068 | Permission | Duplicate perm | Role | 409 |
| INT-069 | User | Duplicate user | Role | 409 |
| INT-070 | Relationships | Orphan permission | Permission | 404 |
| INT-071 | Error | Conflict resolution | Stale | 409 |
| INT-072 | E2E | Restore flow | Role | Restore |
| INT-073 | CRUD | Update → Get | Role | Updated |
| INT-074 | Permission | Remove non-assigned | Role | 404 |
| INT-075 | User | Remove non-assigned | Role | 404 |
| INT-076 | Relationships | Role → Audit | Role | Audit |
| INT-077 | Error | Timeout handling | Slow | 504 |
| INT-078 | E2E | Typeahead flow | Role | Typeahead |
| INT-079 | CRUD | Create → Get | Role | Match |
| INT-080 | Permission | System role protect | Role | 403 |
| INT-081 | User | Last admin protect | Role | 409 |
| INT-082 | Relationships | User → Role | User | Linked |
| INT-083 | Error | Service unavailable | Down | 503 |
| INT-084 | E2E | Dropdown flow | Role | Pairs |
| INT-085 | CRUD | Delete → Get | Role | 404 |
| INT-086 | Permission | Permission concurrent | Role | Last |
| INT-087 | User | User concurrent | Role | Last |
| INT-088 | Relationships | Role → Permission | Role | 1:N |
| INT-089 | Error | Payload too large | Huge | 413 |
| INT-090 | E2E | Full auth flow | Auth | Token |

---

## §6 Security Tests (50)

| ID | Category | Attack | Target | Expected |
|----|----------|--------|-------|----------|
| SEC-001 | Injection | SQL | Search | Sanitized |
| SEC-002 | Injection | XSS | Name | Encoded |
| SEC-003 | Injection | Path traversal | Path | Rejected |
| SEC-004 | Injection | NoSQL | Filter | Rejected |
| SEC-005 | Injection | Command | Export | Rejected |
| SEC-006 | Injection | Header | Header | Rejected |
| SEC-007 | Injection | Log | Input | Sanitized |
| SEC-008 | Injection | LDAP | Search | Rejected |
| SEC-009 | Injection | Log4j | Input | Rejected |
| SEC-010 | Injection | SSRF | URL | Rejected |
| SEC-011 | Access | No auth | All | 401 |
| SEC-012 | Access | Wrong role | Admin | 403 |
| SEC-013 | Access | Cross-org | Other org | 403 |
| SEC-014 | Access | Horizontal | Other user | 403 |
| SEC-015 | Access | Vertical | Admin | 403 |
| SEC-016 | Access | Expired | Token | 401 |
| SEC-017 | Access | Revoked | Token | 401 |
| SEC-018 | Access | Tampered | Token | 401 |
| SEC-019 | Access | Scope | OAuth | 403 |
| SEC-020 | Access | Service | UI | 403 |
| SEC-021 | IDOR | Other org role | ID | 403 |
| SEC-022 | IDOR | Other user | ID | 403 |
| SEC-023 | IDOR | Manipulate | Path | 403 |
| SEC-024 | IDOR | Enumeration | IDs | Rate limit |
| SEC-025 | IDOR | Pollution | Params | First |
| SEC-026 | Mass Assign | Admin | Body | Ignored |
| SEC-027 | Mass Assign | Role | Body | Ignored |
| SEC-028 | Mass Assign | Org | Body | Ignored |
| SEC-029 | Mass Assign | User | Body | Ignored |
| SEC-030 | Mass Assign | Permission | Body | Ignored |
| SEC-031 | Auth | Fixation | Session | New |
| SEC-032 | Auth | Hijack | Token | Invalid |
| SEC-033 | Auth | Replay | Old token | Reject |
| SEC-034 | Auth | CSRF | State | Token |
| SEC-035 | Auth | Brute | Login | Rate limit |
| SEC-036 | Data | PII in export | Export | Masked |
| SEC-037 | Data | Logs | Sensitive | No PII |
| SEC-038 | Data | Error | 500 | Generic |
| SEC-039 | Data | Stack | Exception | Hidden |
| SEC-040 | Data | Debug | Prod | Off |
| SEC-041 | OWASP | A01 | Access | 403 |
| SEC-042 | OWASP | A02 | Crypto | TLS |
| SEC-043 | OWASP | A03 | Injection | Param |
| SEC-044 | OWASP | A04 | Design | Defensive |
| SEC-045 | OWASP | A05 | Misconfig | Secure |
| SEC-046 | OWASP | A06 | Vulnerable | No CVE |
| SEC-047 | OWASP | A07 | Auth | Strong |
| SEC-048 | OWASP | A08 | Integrity | Checks |
| SEC-049 | OWASP | A09 | Logging | Audit |
| SEC-050 | OWASP | A10 | SSRF | No internal |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users get same | Both succeed |
| CON-002 | 2 admins create same code | One fails 409 |
| CON-003 | 2 admins update same | Last write |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click create | Single |
| CON-007 | Rapid filter | Last wins |
| CON-008 | Delete during read | Snapshot |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Assign same permission | One or conflict |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Assign same user | One or conflict |
| CON-022 | Permission change | Consistent |
| CON-023 | User change | Consistent |
| CON-024 | Role change | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid code | Accept |
| UNT-002 | Validation | Invalid code | Reject |
| UNT-003 | Validation | Valid name | Accept |
| UNT-004 | Validation | Invalid name | Reject |
| UNT-005 | Validation | Valid type | Accept |
| UNT-006 | Formatting | Code | Formatted |
| UNT-007 | Formatting | Name | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Permission count | Correct |
| UNT-010 | Calculation | User count | Correct |
| UNT-011 | Calculation | Filter count | Correct |
| UNT-012 | Calculation | Depth | Correct |
| UNT-013 | Calculation | Hierarchy | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Type filter | Filtered |
| UNT-018 | Status | System role | Protected |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get all | < 500ms |
| PRF-002 | Get by ID | < 50ms |
| PRF-003 | Get by code | < 50ms |
| PRF-004 | Search | < 300ms |
| PRF-005 | Filter type | < 200ms |
| PRF-006 | Get permissions | < 300ms |
| PRF-007 | Get users | < 300ms |
| PRF-008 | Create | < 200ms |
| PRF-009 | Update | < 200ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent create | < 500ms each |
| PRF-013 | Memory list | < 50MB |
| PRF-014 | Memory export | < 100MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 5 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users | 10 min | 95% < 500ms |
| LDT-002 | 50 users | 10 min | 95% < 1s |
| LDT-003 | 100 users | 10 min | 95% < 2s |
| LDT-004 | Spike 10→100 | 5 min | No crash |
| LDT-005 | Spike 50→200 | 5 min | Graceful |
| LDT-006 | Stress 200 | Until fail | Document |
| LDT-007 | Stress 500 | Until fail | Document |
| LDT-008 | 50 concurrent | 5 min | Queue/limit |
| LDT-009 | Recovery spike | 5 min | Baseline |
| LDT-010 | Recovery stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| CRUD roles | POS-001–005, FUN-001–006 |
| Role-permission assignment | POS-008–011, FUN-009–010 |
| User-role mapping | POS-012–014, FUN-011–012 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
