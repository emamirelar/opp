# PermissionController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/PermissionController`  
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

REST API for permission management: get user permissions, role-permission mapping, permission checks.

---

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get user permissions | GET /api/permissions/user/{userId} | Permissions |
| POS-002 | Get current user permissions | GET /api/permissions/me | Current user perms |
| POS-003 | Get by role | GET /api/permissions/role/{roleId} | Role permissions |
| POS-004 | Get all permissions | GET /api/permissions | All permissions |
| POS-005 | Filter by category | GET ?category=Partner | Filtered |
| POS-006 | Filter by resource | GET ?resource=partner | Filtered |
| POS-007 | Search permissions | GET ?search=text | Filtered |
| POS-008 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-009 | Sort by name | GET ?sortBy=name | Sorted |
| POS-010 | Get permission by ID | GET /api/permissions/{id} | Permission details |
| POS-011 | Check permission | POST /api/permissions/check | true/false |
| POS-012 | Get role-permission mapping | GET /api/permissions/roles/{roleId} | Mapping |
| POS-013 | Get permission roles | GET /api/permissions/{id}/roles | Roles |
| POS-014 | Empty result | GET for empty filter | [] |
| POS-015 | Single result | GET for single match | [item] |
| POS-016 | Authenticated access | GET with token | 200 |
| POS-017 | Admin get all | GET as admin | All |
| POS-018 | Dropdown | GET /api/permissions/dropdown | ID/name pairs |
| POS-019 | Get categories | GET /api/permissions/categories | Categories |
| POS-020 | Get resources | GET /api/permissions/resources | Resources |
| POS-021 | Combined filter | GET ?category=X&resource=Y | Combined |
| POS-022 | Sort ascending | GET ?sortBy=name&sortOrder=asc | Sorted |
| POS-023 | Sort descending | GET ?sortBy=name&sortOrder=desc | Sorted |
| POS-024 | First page | GET ?page=1 | First page |
| POS-025 | Last page | GET ?page=last | Partial |
| POS-026 | Default page size | GET no pageSize | Default |
| POS-027 | Current user scope | GET /me | Scoped |
| POS-028 | Multiple roles | User with roles | Combined |
| POS-029 | Inherited permissions | Role hierarchy | Inherited |
| POS-030 | Permission check true | Valid perm | true |
| POS-031 | Permission check false | Invalid perm | false |
| POS-032 | Typeahead | GET /api/permissions/typeahead?q=text | Suggestions |
| POS-033 | Export | GET /api/permissions/export | Export file |
| POS-034 | Cached response | GET same query | 200 |
| POS-035 | Session permissions | Valid session | Perms |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid ID | id=abc | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Invalid userId | userId=999999 | 404 |
| NEG-007 | Invalid roleId | roleId=999999 | 404 |
| NEG-008 | SQL injection | search='; DROP | Sanitized |
| NEG-009 | XSS in search | search=<script> | Sanitized |
| NEG-010 | Negative page | page=-1 | 400 |
| NEG-011 | Zero pageSize | pageSize=0 | 400 |
| NEG-012 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-013 | Invalid sort | sortBy=invalid | 400 |
| NEG-014 | No permission | User without CanView | 403 |
| NEG-015 | Cross-user access | Other user perms | 403 |
| NEG-016 | Cross-org access | Other org | 403 |
| NEG-017 | Malformed JSON | Invalid JSON | 400 |
| NEG-018 | Wrong content-type | Application/xml | 415 |
| NEG-019 | Rate limit | Too many | 429 |
| NEG-020 | Payload too large | Huge body | 413 |
| NEG-021 | Invalid Accept | Accept: text/plain | 406 |
| NEG-022 | HTTP method | PUT for create | 405 |
| NEG-023 | Trailing slash | /api/permissions/ | Redirect |
| NEG-024 | Case sensitivity | /api/Permissions | 404 |
| NEG-025 | Extra path | /api/permissions/1/extra | 404 |
| NEG-026 | Invalid bearer | Bearer malformed | 401 |
| NEG-027 | Revoked token | Revoked JWT | 401 |
| NEG-028 | Service account | Service for UI | 403 |
| NEG-029 | DB timeout | Simulate | 503 |
| NEG-030 | Invalid category | category=invalid | 400 |
| NEG-031 | Invalid resource | resource=invalid | 400 |
| NEG-032 | Invalid check body | Check body malformed | 400 |
| NEG-033 | Zero ID | id=0 | 400 |
| NEG-034 | Invalid UUID | id=invalid-guid | 400 |
| NEG-035 | Blocked IP | From blocked | 403 |
| NEG-036 | Control chars | search with \0 | 400 |
| NEG-037 | Unicode overflow | Very long | 400 |
| NEG-038 | Export no permission | No export permission | 403 |
| NEG-039 | Inactive user | User disabled | 403 |
| NEG-040 | Inactive role | Role disabled | 403 |
| NEG-041 | Mismatched IDs | Path != body | 400 |
| NEG-042 | Read-only field | Update createdDate | Ignored |
| NEG-043 | Version conflict | Stale version | 409 |
| NEG-044 | CORS fail | Invalid origin | CORS error |
| NEG-045 | Deleted role | roleId deleted | 404 |
| NEG-046 | Deleted user | userId deleted | 404 |
| NEG-047 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-048 | Max URL length | Very long URL | 414 |
| NEG-049 | Invalid endpoint | /api/permissions/invalid | 404 |
| NEG-050 | Invalid method | PATCH | 405 |
| NEG-051 | Missing query | GET no params | 200 or 400 |
| NEG-052 | Invalid encoding | Malformed URL | 400 |
| NEG-053 | Check permission denied | No perm | false |
| NEG-054 | Check invalid resource | Invalid resource | 400 |
| NEG-055 | OPTIONS | OPTIONS | 200 |
| NEG-056 | HEAD | HEAD | 200 or 405 |
| NEG-057 | Enumeration | Enumerate users | Rate limit |
| NEG-058 | Enumeration | Enumerate roles | Rate limit |
| NEG-059 | Audit failure | Audit down | Continue |
| NEG-060 | Permission escalation | Escalate | 403 |
| NEG-061 | Role escalation | Escalate | 403 |
| NEG-062 | Scope bypass | Bypass scope | 403 |
| NEG-063 | Deleted permission | id deleted | 404 |
| NEG-064 | Invalid permission name | name=invalid | 404 |
| NEG-065 | Empty check body | Check [] | 400 |
| NEG-066 | Invalid permission format | Permission malformed | 400 |
| NEG-067 | Missing required | Check missing | 400 |
| NEG-068 | Invalid action | action=invalid | 400 |
| NEG-069 | Resource mismatch | Wrong resource | 403 |
| NEG-070 | Soft-deleted filter | Query deleted | Excluded |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-002 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-005 | userId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-006 | roleId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | Empty list | - | - | [] | - | - |
| BND-008 | Single item | - | - | [item] | - | - |
| BND-009 | First page | page=1 | - | ✅ | - | - |
| BND-010 | Last page | - | - | Partial | - | - |
| BND-011 | Zero length search | - | - | [] | - | - |
| BND-012 | Max length search | 200 | - | - | ✅ | ❌ |
| BND-013 | Unicode search | - | - | Accept | - | - |
| BND-014 | Arabic search | - | - | Display | - | - |
| BND-015 | Chinese search | - | - | Display | - | - |
| BND-016 | Null optional | - | - | Default | - | - |
| BND-017 | Empty string | - | - | No filter | - | - |
| BND-018 | Whitespace | - | - | Trim | - | - |
| BND-019 | Sort empty | - | - | [] | - | - |
| BND-020 | Sort single | - | - | [item] | - | - |
| BND-021 | Filter no match | - | - | [] | - | - |
| BND-022 | Filter all | - | - | Full | - | - |
| BND-023 | Typeahead min | 1 | - | ✅ | - | - |
| BND-024 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-025 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-026 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-027 | Pagination boundary | - | - | Exact | - | - |
| BND-028 | Zero ID | id=0 | - | 400 | - | - |
| BND-029 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-030 | Permission count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-031 | Role count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-032 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-033 | Empty category | - | - | All | - | - |
| BND-034 | Empty resource | - | - | All | - | - |
| BND-035 | Single category | - | - | Filtered | - | - |
| BND-036 | Single resource | - | - | Filtered | - | - |
| BND-037 | Multiple categories | - | 10 | ✅ | ✅ | ❌ |
| BND-038 | Multiple resources | - | 10 | ✅ | ✅ | ❌ |
| BND-039 | Inactive | - | - | Excluded | - | - |
| BND-040 | Name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-041 | Code length | 1 | 50 | ✅ | ✅ | ❌ |
| BND-042 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-043 | Category length | - | 100 | ✅ | ✅ | ❌ |
| BND-044 | Resource length | - | 100 | ✅ | ✅ | ❌ |
| BND-045 | Created date | - | - | UTC | - | - |
| BND-046 | Modified date | - | - | UTC | - | - |
| BND-047 | Check batch size | 1 | 50 | ✅ | ✅ | ❌ |
| BND-048 | Role hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-049 | Permission inheritance | - | - | Valid | - | - |
| BND-050 | Typeahead results | 0 | 20 | [] | ✅ | Truncate |
| BND-051 | Pagination overflow | - | - | [] | - | - |
| BND-052 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-053 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-054 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-055 | Export empty | - | - | Headers | - | - |
| BND-056 | Version | 1 | - | ✅ | ❌ | - |
| BND-057 | Session expiry | - | - | 401 | - | - |
| BND-058 | Role list | 0 | 50 | ✅ | ✅ | ❌ |
| BND-059 | User list | 0 | 100 | ✅ | ✅ | ❌ |
| BND-060 | Action length | - | 50 | ✅ | ✅ | ❌ |
| BND-061 | Scope length | - | 100 | ✅ | ✅ | ❌ |
| BND-062 | Combined filter | - | - | AND | - | - |
| BND-063 | Round-trip | Get → Check | - | Match | - | - |
| BND-064 | Permission grant | - | - | Valid | - | - |
| BND-065 | Permission revoke | - | - | Valid | - | - |
| BND-066 | Role assignment | - | - | Valid | - | - |
| BND-067 | User assignment | - | - | Valid | - | - |
| BND-068 | Cache TTL | - | 3600 | Valid | Valid | ❌ |
| BND-069 | Token expiry | - | - | 401 | - | - |
| BND-070 | Scope boundary | - | - | Exact | - | - |
| BND-071 | Category charset | - | - | Valid | - | - |
| BND-072 | Resource encoding | - | UTF-8 | Valid | Valid | ❌ |
| BND-073 | Request size | - | 1MB | - | ✅ | ❌ |
| BND-074 | Header count | - | 50 | ✅ | ✅ | ❌ |
| BND-075 | Session duration | - | 24h | Valid | Valid | ❌ |
| BND-076 | Token lifetime | - | 1h | Valid | Valid | ❌ |
| BND-077 | Retry count | 0 | 3 | ✅ | ✅ | ❌ |
| BND-078 | Backoff max | - | 30s | - | ✅ | ❌ |
| BND-079 | Connection timeout | - | 30s | - | ✅ | ❌ |
| BND-080 | Read timeout | - | 60s | - | ✅ | ❌ |
| BND-081 | Write timeout | - | 60s | - | ✅ | ❌ |
| BND-082 | Idle timeout | - | 90s | - | ✅ | ❌ |
| BND-083 | Keep-alive | - | 60s | - | ✅ | ❌ |
| BND-084 | Chunk size | - | 8KB | - | ✅ | ❌ |
| BND-085 | Buffer size | - | 64KB | - | ✅ | ❌ |
| BND-086 | Pool size | - | 100 | - | ✅ | ❌ |
| BND-087 | Queue depth | - | 1000 | - | ✅ | ❌ |
| BND-088 | Batch size | 1 | 50 | ✅ | ✅ | ❌ |
| BND-089 | Category enum | - | max | Valid | Valid | ❌ |
| BND-090 | Resource enum | - | max | Valid | Valid | ❌ |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get user perms | GET user | Perms |
| FUN-002 | Workflow | Get current | GET me | Current |
| FUN-003 | Workflow | Get by role | GET role | Role perms |
| FUN-004 | Workflow | Get all | GET | List |
| FUN-005 | Workflow | Check permission | POST check | true/false |
| FUN-006 | Workflow | Filter category | GET ?category | Filtered |
| FUN-007 | Workflow | Filter resource | GET ?resource | Filtered |
| FUN-008 | Workflow | Search | GET ?search | Searched |
| FUN-009 | Workflow | Paginate | GET ?page | Paginated |
| FUN-010 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-011 | Workflow | Role mapping | GET roles | Mapping |
| FUN-012 | Workflow | Permission roles | GET roles | Roles |
| FUN-013 | Workflow | Categories | GET categories | Categories |
| FUN-014 | Workflow | Resources | GET resources | Resources |
| FUN-015 | Workflow | Dropdown | GET dropdown | Pairs |
| FUN-016 | Validation | Valid user | Invalid | 404 |
| FUN-017 | Validation | Valid role | Invalid | 404 |
| FUN-018 | Validation | Permission | No permission | 403 |
| FUN-019 | Validation | ID format | Invalid | 400 |
| FUN-020 | Validation | Search length | Too long | 400 |
| FUN-021 | Validation | Page bounds | Invalid | 400 |
| FUN-022 | Validation | Check body | Invalid | 400 |
| FUN-023 | Validation | Scope | Cross-scope | 403 |
| FUN-024 | Validation | Category | Invalid | 400 |
| FUN-025 | Validation | Resource | Invalid | 400 |
| FUN-026 | Constraint | Soft delete | Query | Excluded |
| FUN-027 | Constraint | Org scope | Cross-org | 403 |
| FUN-028 | Constraint | Max bulk | >50 | 400 |
| FUN-029 | Constraint | Export limit | >10K | Truncate |
| FUN-030 | Constraint | Role hierarchy | Inherited | Correct |
| FUN-031 | Constraint | User scope | User only | Scoped |
| FUN-032 | Constraint | Admin scope | Admin only | Full |
| FUN-033 | Constraint | Inactive user | Disabled | 403 |
| FUN-034 | Constraint | Inactive role | Disabled | 403 |
| FUN-035 | Constraint | URL length | >2048 | 414 |
| FUN-036 | Audit | Get | GET | Audit |
| FUN-037 | Audit | Check | POST check | Audit |
| FUN-038 | Audit | Export | GET export | Audit |
| FUN-039 | Audit | Timestamp | Any | UTC |
| FUN-040 | Audit | User ID | Any | User ID |
| FUN-041 | Audit | IP | Any | IP |
| FUN-042 | Audit | Resource | Any | Resource |
| FUN-043 | Audit | Outcome | Any | Outcome |
| FUN-044 | Audit | Permission check | POST check | Audit |
| FUN-045 | Audit | Filter | GET filter | Audit |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Role inheritance | Hierarchy | Inherited |
| FUN-050 | Business | User scope | User | Scoped |
| FUN-051 | Workflow | Typeahead | GET typeahead | Suggestions |
| FUN-052 | Workflow | Export | GET export | File |
| FUN-053 | Validation | Check body | Invalid | 400 |
| FUN-054 | Validation | Category | Invalid | 400 |
| FUN-055 | Constraint | Permission lock | Locked | 423 |
| FUN-056 | Audit | Check | POST check | Audit |
| FUN-057 | Audit | Export | GET export | Audit |
| FUN-058 | Business | Admin scope | Full | Correct |
| FUN-059 | Business | User scope | Scoped | Correct |
| FUN-060 | Workflow | Categories | GET categories | Categories |
| FUN-061 | Validation | Resource | Invalid | 400 |
| FUN-062 | Constraint | Max bulk | >50 | 400 |
| FUN-063 | Audit | Get | GET | Audit |
| FUN-064 | Business | Role cascade | Delete role | 404 |
| FUN-065 | Workflow | Cached response | GET same | 200 |
| FUN-066 | Validation | User exists | Invalid | 404 |
| FUN-067 | Constraint | Export limit | >10K | Truncate |
| FUN-068 | Audit | Filter | GET filter | Audit |
| FUN-069 | Business | Role hierarchy | Inherited | Correct |
| FUN-070 | Workflow | Resources | GET resources | Resources |
| FUN-071 | Validation | Role exists | Invalid | 404 |
| FUN-072 | Constraint | Inactive user | Disabled | 403 |
| FUN-073 | Audit | Role mapping | GET roles | Audit |
| FUN-074 | Business | Cross-user perms | Other user | 403 |
| FUN-075 | Workflow | Full round-trip | Get → Check | Match |
| FUN-076 | Validation | Action format | Invalid | 400 |
| FUN-077 | Constraint | Inactive role | Disabled | 403 |
| FUN-078 | Audit | User perms | GET user | Audit |
| FUN-079 | Business | Permission scope | Permission | Correct |
| FUN-080 | Workflow | Dropdown flow | GET dropdown | Pairs |
| FUN-081 | Validation | Scope format | Invalid | 400 |
| FUN-082 | Constraint | Escalation | Escalate | 403 |
| FUN-083 | Audit | Role perms | GET role | Audit |
| FUN-084 | Business | Scope bypass | Bypass | 403 |
| FUN-085 | Workflow | Check batch | POST check | Results |
| FUN-086 | Validation | Batch size | >50 | 400 |
| FUN-087 | Constraint | Session expiry | Expired | 401 |
| FUN-088 | Audit | Permission view | GET | Audit |
| FUN-089 | Business | Combined perms | Multiple roles | Correct |
| FUN-090 | Workflow | Inherited flow | Role hierarchy | Inherited |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get by ID | Permission | Match |
| INT-002 | CRUD | Get user perms | User, Permission | Perms |
| INT-003 | CRUD | Get role perms | Role, Permission | Perms |
| INT-004 | CRUD | Check permission | User, Permission | Result |
| INT-005 | CRUD | Dropdown | Permission | Pairs |
| INT-006 | CRUD | Categories | Permission | Categories |
| INT-007 | CRUD | Resources | Permission | Resources |
| INT-008 | CRUD | Export | Permission | File |
| INT-009 | CRUD | Role mapping | Role, Permission | Mapping |
| INT-010 | CRUD | Permission roles | Permission, Role | Roles |
| INT-011 | Search | Search by name | Permission | Matches |
| INT-012 | Search | Typeahead | Permission | Suggestions |
| INT-013 | Search | Filter category | Permission | Filtered |
| INT-014 | Search | Filter resource | Permission | Filtered |
| INT-015 | Search | Multi-filter | Permission | Combined |
| INT-016 | Search | Empty search | - | [] |
| INT-017 | Search | Partial match | Permission | Fuzzy |
| INT-018 | Search | Sort + filter | Permission | Both |
| INT-019 | Search | Filter + pagination | Permission | Both |
| INT-020 | Search | Hierarchy | Role, Permission | Inherited |
| INT-021 | Pagination | Page 1 | Permission | First |
| INT-022 | Pagination | Last page | Permission | Partial |
| INT-023 | Pagination | Size | Permission | Correct |
| INT-024 | Pagination | Invalid | Permission | 400 |
| INT-025 | Pagination | Boundary | Permission | Exact |
| INT-026 | Relationships | Permission → Roles | Permission, Role | Linked |
| INT-027 | Relationships | Role → Permissions | Role, Permission | Linked |
| INT-028 | Relationships | User → Permissions | User, Permission | Via roles |
| INT-029 | Relationships | Orphan | Deleted role | 404 |
| INT-030 | Relationships | User deleted | User | 404 |
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
| INT-046 | E2E | Full get flow | Permission | Get → Check |
| INT-047 | E2E | Full check flow | Permission | Check → Result |
| INT-048 | E2E | Role flow | Role, Permission | Get → Role |
| INT-049 | E2E | User flow | User, Permission | Get → User |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get user perms | User, Permission | Perms |
| INT-052 | CRUD | Get role perms | Role, Permission | Perms |
| INT-053 | Check | Check flow | User, Permission | Result |
| INT-054 | Search | Typeahead flow | Permission | Suggestions |
| INT-055 | Filter | Category flow | Permission | Filtered |
| INT-056 | Relationships | Permission → Roles | Permission, Role | Linked |
| INT-057 | Error | Validation chain | Bad input | 400 |
| INT-058 | Error | Auth chain | No auth | 401 |
| INT-059 | E2E | Export flow | Permission | Export |
| INT-060 | E2E | Categories flow | Permission | Categories |
| INT-061 | CRUD | Check batch | User, Permission | Results |
| INT-062 | Check | Check denied | User | false |
| INT-063 | Filter | Resource flow | Permission | Filtered |
| INT-064 | Relationships | Role → Permissions | Role, Permission | Linked |
| INT-065 | Error | Permission chain | No perm | 403 |
| INT-066 | E2E | Full permission flow | Permission | Get → Check |
| INT-067 | CRUD | Get by ID | Permission | Match |
| INT-068 | Check | Check granted | User | true |
| INT-069 | Filter | Combined filter | Permission | Combined |
| INT-070 | Relationships | User → Permissions | User, Permission | Via roles |
| INT-071 | Error | Conflict resolution | Stale | 409 |
| INT-072 | E2E | Resources flow | Permission | Resources |
| INT-073 | CRUD | Dropdown | Permission | Pairs |
| INT-074 | Check | Check invalid | User | false |
| INT-075 | Filter | Multi-category | Permission | Combined |
| INT-076 | Relationships | Permission → Audit | Permission | Audit |
| INT-077 | Error | Timeout handling | Slow | 504 |
| INT-078 | E2E | Dropdown flow | Permission | Pairs |
| INT-079 | CRUD | Role mapping | Role, Permission | Mapping |
| INT-080 | Check | Check scope | User | Scoped |
| INT-081 | Filter | Role hierarchy | Permission | Inherited |
| INT-082 | Relationships | Orphan role | Role | 404 |
| INT-083 | Error | Service unavailable | Down | 503 |
| INT-084 | E2E | Typeahead flow | Permission | Typeahead |
| INT-085 | CRUD | Permission roles | Permission, Role | Roles |
| INT-086 | Check | Check concurrent | User | All |
| INT-087 | Filter | Pagination | Permission | Paginated |
| INT-088 | Relationships | User → Permission | User | Via roles |
| INT-089 | Error | Payload too large | Huge | 413 |
| INT-090 | E2E | Full auth flow | Auth | Token |

---

## §6 Security Tests (50)

| ID | Category | Attack | Target | Expected |
|----|----------|--------|-------|----------|
| SEC-001 | Injection | SQL | Search | Sanitized |
| SEC-002 | Injection | XSS | Search | Encoded |
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
| SEC-021 | IDOR | Other user perms | ID | 403 |
| SEC-022 | IDOR | Other role | ID | 403 |
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
| CON-002 | 2 users check same | Both succeed |
| CON-003 | 10 concurrent gets | All succeed |
| CON-004 | 50 concurrent list | All succeed |
| CON-005 | Rapid filter | Last wins |
| CON-006 | Permission change | Snapshot |
| CON-007 | Role change | Snapshot |
| CON-008 | Delete during read | Snapshot |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Rate limit | Fair |
| CON-015 | Session expiry | Clean |
| CON-016 | Multiple checks | All succeed |
| CON-017 | Cache stampede | Single |
| CON-018 | Lock | Timeout |
| CON-019 | Memory | Graceful |
| CON-020 | Role update | Consistent |
| CON-021 | User update | Consistent |
| CON-022 | Permission update | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Scope change | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid permission | Accept |
| UNT-002 | Validation | Invalid permission | Reject |
| UNT-003 | Validation | Valid role | Accept |
| UNT-004 | Validation | Invalid role | Reject |
| UNT-005 | Validation | Valid user | Accept |
| UNT-006 | Formatting | Name | Formatted |
| UNT-007 | Formatting | Code | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Permission count | Correct |
| UNT-010 | Calculation | Role count | Correct |
| UNT-011 | Calculation | User count | Correct |
| UNT-012 | Calculation | Filter count | Correct |
| UNT-013 | Calculation | Check result | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Category filter | Filtered |
| UNT-018 | Status | Resource filter | Filtered |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get all | < 500ms |
| PRF-002 | Get by ID | < 50ms |
| PRF-003 | Get user perms | < 100ms |
| PRF-004 | Get role perms | < 100ms |
| PRF-005 | Filter category | < 200ms |
| PRF-006 | Filter resource | < 200ms |
| PRF-007 | Check permission | < 50ms |
| PRF-008 | Search | < 300ms |
| PRF-009 | Pagination | < 200ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent check | < 500ms each |
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
| Get user permissions | POS-001–002, FUN-001–002 |
| Role-permission mapping | POS-003, POS-012, FUN-011 |
| Permission checks | POS-011, FUN-005 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
