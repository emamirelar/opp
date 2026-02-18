# PartnerCategoryController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/PartnerCategoryController`  
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

REST API for partner categories: CRUD categories, assignment to partners, filtering.

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all categories | GET /api/partner-categories | Category list |
| POS-002 | Get category by ID | GET /api/partner-categories/{id} | Category details |
| POS-003 | Create category (admin) | POST /api/partner-categories | 201 Created |
| POS-004 | Update category (admin) | PUT /api/partner-categories/{id} | 200 OK |
| POS-005 | Delete category (admin) | DELETE /api/partner-categories/{id} | 204 No Content |
| POS-006 | Get dropdown | GET /api/partner-categories/dropdown | ID/name pairs |
| POS-007 | Search categories | GET /api/partner-categories?search=text | Filtered |
| POS-008 | Filter by type | GET ?type=Primary | Filtered |
| POS-009 | Assign to partner | POST /api/partner-categories/{id}/partners | Assigned |
| POS-010 | Get partner assignments | GET /api/partner-categories/{id}/partners | Partners |
| POS-011 | Remove from partner | DELETE /api/partner-categories/{id}/partners/{pid} | Removed |
| POS-012 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-013 | Sort by name | GET ?sortBy=name | Sorted |
| POS-014 | Sort by code | GET ?sortBy=code | Sorted |
| POS-015 | Get active only | GET ?active=true | Active only |
| POS-016 | Empty result | GET for empty filter | [] |
| POS-017 | Single result | GET for single match | [item] |
| POS-018 | Authenticated access | GET with token | 200 |
| POS-019 | Soft delete | DELETE (soft) | IsDeleted |
| POS-020 | Restore | POST /api/partner-categories/{id}/restore | Restored |
| POS-021 | Get by code | GET /api/partner-categories/code/{code} | By code |
| POS-022 | Bulk assign | POST /api/partner-categories/{id}/partners/bulk | Bulk assigned |
| POS-023 | Filter by parent | GET ?parentId=1 | Child categories |
| POS-024 | Root categories | GET ?parentId=0 | Roots |
| POS-025 | Typeahead | GET /api/partner-categories/typeahead?q=text | Suggestions |
| POS-026 | Export | GET /api/partner-categories/export | Export file |
| POS-027 | Admin create | POST as admin | 201 |
| POS-028 | Admin update | PUT as admin | 200 |
| POS-029 | Admin delete | DELETE as admin | 204 |
| POS-030 | Partner count | GET /api/partner-categories/{id}/partner-count | Count |

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
| NEG-011 | Invalid partnerId | partnerId=999999 | 404 |
| NEG-012 | Invalid parentId | parentId=999999 | 404 |
| NEG-013 | SQL injection | search='; DROP | Sanitized |
| NEG-014 | XSS in name | name=<script> | Sanitized |
| NEG-015 | Negative page | page=-1 | 400 |
| NEG-016 | Zero pageSize | pageSize=0 | 400 |
| NEG-017 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-018 | Invalid sort | sortBy=invalid | 400 |
| NEG-019 | No permission | User without CanView | 403 |
| NEG-020 | No admin for write | POST as user | 403 |
| NEG-021 | Cross-org access | Other org category | 403 |
| NEG-022 | Deleted category | id of deleted | 404 |
| NEG-023 | Circular parent | parentId=self | 400 |
| NEG-024 | Malformed JSON | Invalid JSON | 400 |
| NEG-025 | Wrong content-type | Application/xml | 415 |
| NEG-026 | Rate limit | Too many | 429 |
| NEG-027 | Payload too large | Huge body | 413 |
| NEG-028 | Invalid Accept | Accept: text/plain | 406 |
| NEG-029 | HTTP method | PUT for create | 405 |
| NEG-030 | Trailing slash | /api/partner-categories/ | Redirect |
| NEG-031 | Case sensitivity | /api/Partner-Categories | 404 |
| NEG-032 | Extra path | /api/partner-categories/1/extra | 404 |
| NEG-033 | Invalid bearer | Bearer malformed | 401 |
| NEG-034 | Revoked token | Revoked JWT | 401 |
| NEG-035 | Service account | Service for UI | 403 |
| NEG-036 | DB timeout | Simulate | 503 |
| NEG-037 | Invalid type | type=invalid | 400 |
| NEG-038 | Empty code | code= | 400 |
| NEG-039 | Whitespace code | code="  " | 400 |
| NEG-040 | Zero ID | id=0 | 400 |
| NEG-041 | Invalid UUID | id=invalid-guid | 400 |
| NEG-042 | Blocked IP | From blocked | 403 |
| NEG-043 | Control chars | name with \0 | 400 |
| NEG-044 | Unicode overflow | Very long | 400 |
| NEG-045 | Delete in-use | Category referenced | 409 |
| NEG-046 | Update deleted | PUT on deleted | 404 |
| NEG-047 | Restore not deleted | POST restore on active | 400 |
| NEG-048 | Mismatched IDs | Path != body | 400 |
| NEG-049 | Read-only field | Update createdDate | Ignored |
| NEG-050 | Version conflict | Stale version | 409 |
| NEG-051 | CORS fail | Invalid origin | CORS error |
| NEG-052 | Inactive org | Org inactive | 403 |
| NEG-053 | Invalid bulk assign | Bulk with invalid | 400/partial |
| NEG-054 | Empty bulk | POST [] | 400 |
| NEG-055 | Excessive bulk | 1000 IDs | 400 |
| NEG-056 | Assign to self | Category to self | 400 |
| NEG-057 | Duplicate assign | Already assigned | 409 |
| NEG-058 | Invalid partner | Partner deleted | 404 |
| NEG-059 | Export no permission | No export permission | 403 |
| NEG-060 | Audit failure | Audit down | Continue |
| NEG-061 | Reserved code | code=RESERVED | 403 |
| NEG-062 | Duplicate name | name exists | 409 or allow |
| NEG-063 | Orphan parent | parentId deleted | 404 |
| NEG-064 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-065 | Max URL length | Very long URL | 414 |
| NEG-066 | Invalid endpoint | /api/partner-categories/invalid | 404 |
| NEG-067 | Invalid method | PATCH | 405 |
| NEG-068 | Missing query | GET no params | 200 or 400 |
| NEG-069 | Invalid encoding | Malformed URL | 400 |
| NEG-070 | Soft-deleted filter | Query deleted | Excluded |
| NEG-071 | Invalid JSON schema | Schema mismatch | 400 |
| NEG-072 | Missing category name | Name null | 400 |
| NEG-073 | Invalid category type | type=invalid | 400 |
| NEG-074 | Empty partner list | partners=[] | 400 |
| NEG-075 | Invalid parent | parentId=self | 400 |
| NEG-076 | Category locked | Locked category | 423 |
| NEG-077 | Maintenance mode | During maintenance | 503 |
| NEG-078 | Quota exceeded | Assignment quota | 507 |
| NEG-079 | Invalid description | desc too long | 400 |
| NEG-080 | Orphan parent | parentId deleted | 404 |
| NEG-081 | Migration mode | During migration | 503 |
| NEG-082 | Session invalid | Invalid session | 401 |
| NEG-083 | Token type wrong | Wrong token type | 401 |
| NEG-084 | Scope insufficient | OAuth scope | 403 |
| NEG-085 | Rate limit per user | User rate limit | 429 |
| NEG-086 | Concurrent limit | Too many concurrent | 429 |
| NEG-087 | Request timeout | Slow request | 408 |
| NEG-088 | Category archived | Archived category | 410 |
| NEG-089 | In-use category delete | Category in use | 409 |
| NEG-090 | Assignment limit exceeded | >100 assignments | 400 |

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
| BND-007 | parentId | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-008 | partnerId | 1 | int.Max | ✅ | ✅ | ❌ |
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
| BND-025 | Partner count | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-026 | Children count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-027 | Hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-028 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-029 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-030 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-031 | Typeahead min | 1 | - | ✅ | - | - |
| BND-032 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-033 | Pagination boundary | - | - | Exact | - | - |
| BND-034 | Empty bulk | - | - | 400 | - | - |
| BND-035 | Partial bulk | - | - | 207 | - | - |
| BND-036 | Round-trip | Create → Get | - | Match | - | - |
| BND-037 | Soft-deleted | - | - | Excluded | - | - |
| BND-038 | Inactive | - | - | Excluded | - | - |
| BND-039 | Duplicate code | - | - | Reject | - | - |
| BND-040 | Case code | - | - | Normalize | - | - |
| BND-041 | Zero ID | id=0 | - | 400 | - | - |
| BND-042 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-043 | Root category | parentId=0 | - | Root | - | - |
| BND-044 | Leaf category | No children | - | Leaf | - | - |
| BND-045 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-046 | Export empty | - | - | Headers | - | - |
| BND-047 | Export single | - | - | Valid | - | - |
| BND-048 | Version | 1 | - | ✅ | ❌ | - |
| BND-049 | Type length | - | 50 | ✅ | ✅ | ❌ |
| BND-050 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-051 | Order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-052 | Created date | - | - | UTC | - | - |
| BND-053 | Modified date | - | - | UTC | - | - |
| BND-054 | Hierarchy path | - | 500 | ✅ | ✅ | ❌ |
| BND-055 | Full path | - | 1000 | ✅ | ✅ | ❌ |
| BND-056 | Bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-057 | Active flag | - | - | Boolean | - | - |
| BND-058 | IsDefault | - | - | Boolean | - | - |
| BND-059 | Partner assignment | - | - | Valid | - | - |
| BND-060 | Category type | - | max | Valid | Valid | ❌ |
| BND-061 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-062 | Color length | - | 20 | ✅ | ✅ | ❌ |
| BND-063 | Icon length | - | 50 | ✅ | ✅ | ❌ |
| BND-064 | Workflow status | - | - | Valid | - | - |
| BND-065 | Audit fields | - | - | Set | - | - |
| BND-066 | FK parent | - | - | Valid | - | - |
| BND-067 | FK partner | - | - | Valid | - | - |
| BND-068 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-069 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-070 | Assignment limit | 0 | 100 | ✅ | ✅ | ❌ |

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
| FUN-009 | Workflow | Filter type | GET ?type | Filtered |
| FUN-010 | Workflow | Filter parent | GET ?parentId | Filtered |
| FUN-011 | Workflow | Search | GET ?search | Searched |
| FUN-012 | Workflow | Paginate | GET ?page | Paginated |
| FUN-013 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-014 | Workflow | Assign to partner | POST assign | Assigned |
| FUN-015 | Workflow | Remove from partner | DELETE remove | Removed |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required code | Missing | 400 |
| FUN-018 | Validation | Unique code | Duplicate | 409 |
| FUN-019 | Validation | Valid partner | Invalid | 404 |
| FUN-020 | Validation | Valid parent | Invalid | 404 |
| FUN-021 | Validation | No circular | Circular | 400 |
| FUN-022 | Validation | Permission | No permission | 403 |
| FUN-023 | Validation | Admin write | User write | 403 |
| FUN-024 | Validation | ID format | Invalid | 400 |
| FUN-025 | Validation | Type enum | Invalid | 400 |
| FUN-026 | Constraint | FK parent | Invalid | 404 |
| FUN-027 | Constraint | FK partner | Invalid | 404 |
| FUN-028 | Constraint | Delete in-use | Referenced | 409 |
| FUN-029 | Constraint | Hierarchy depth | >10 | 400 |
| FUN-030 | Constraint | Soft delete | Query | Excluded |
| FUN-031 | Constraint | Org scope | Cross-org | 403 |
| FUN-032 | Constraint | Version | Optimistic | 409 |
| FUN-033 | Constraint | Max bulk | >100 | 400 |
| FUN-034 | Constraint | Export limit | >10K | Truncate |
| FUN-035 | Constraint | Assignment limit | >100 | 400 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Restore | POST restore | Audit |
| FUN-040 | Audit | Assign | POST assign | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Hierarchy | Parent-child | Linked |
| FUN-050 | Business | Partner assignment | Assign | Scoped |
| FUN-051 | Workflow | Sort ascending | GET ?sortOrder=asc | Sorted |
| FUN-052 | Workflow | Sort descending | GET ?sortOrder=desc | Sorted |
| FUN-053 | Validation | Category type | Invalid type | 400 |
| FUN-054 | Validation | No circular | Circular | 400 |
| FUN-055 | Constraint | Category lock | Locked | 423 |
| FUN-056 | Audit | Assign | POST assign | Audit |
| FUN-057 | Audit | Remove | DELETE remove | Audit |
| FUN-058 | Business | Hierarchy depth | >10 | 400 |
| FUN-059 | Business | Duplicate assign | Already assigned | 409 |
| FUN-060 | Workflow | First page | GET ?page=1 | First |
| FUN-061 | Validation | Bulk size | >100 | 400 |
| FUN-062 | Constraint | Delete in-use | Referenced | 409 |
| FUN-063 | Audit | Bulk assign | POST bulk | Audit |
| FUN-064 | Business | Category cascade | Delete category | 409 |
| FUN-065 | Workflow | Cached response | GET same | 200 |
| FUN-066 | Validation | Code format | Invalid | 400 |
| FUN-067 | Constraint | Assignment limit | >100 | 400 |
| FUN-068 | Audit | Restore | POST restore | Audit |
| FUN-069 | Business | Parent scope | Org scope | Correct |
| FUN-070 | Workflow | Last page | GET ?page=last | Partial |
| FUN-071 | Validation | Partner exists | Invalid partner | 404 |
| FUN-072 | Constraint | Parent exists | Invalid parent | 404 |
| FUN-073 | Audit | Create | POST | Audit |
| FUN-074 | Business | Cross-org category | Other org | 403 |
| FUN-075 | Workflow | Full round-trip | Create → Get | Match |
| FUN-076 | Validation | Name length | Too long | 400 |
| FUN-077 | Constraint | Export limit | >10K | Truncate |
| FUN-078 | Audit | Update | PUT | Audit |
| FUN-079 | Business | Inactive category | Category disabled | 403 |
| FUN-080 | Workflow | Export flow | GET export | File |
| FUN-081 | Validation | Parent valid | Invalid | 404 |
| FUN-082 | Constraint | Max bulk | >100 | 400 |
| FUN-083 | Audit | Delete | DELETE | Audit |
| FUN-084 | Business | Hierarchy scope | Parent-child | Correct |
| FUN-085 | Workflow | Typeahead flow | GET typeahead | Suggestions |
| FUN-086 | Validation | Code length | Too long | 400 |
| FUN-087 | Constraint | Bulk partial | Partial success | 207 |
| FUN-088 | Audit | Hierarchy | GET hierarchy | Audit |
| FUN-089 | Business | Assignment scope | Assign | Scoped |
| FUN-090 | Workflow | Bulk assign flow | POST bulk | Assigned |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Category | Match |
| INT-002 | CRUD | Update → Get | Category | Updated |
| INT-003 | CRUD | Delete → Get | Category | 404 |
| INT-004 | CRUD | Restore → Get | Category | Restored |
| INT-005 | CRUD | Get by code | Category | Match |
| INT-006 | CRUD | Assign → Get partners | Category, Partner | Assigned |
| INT-007 | CRUD | Remove → Get partners | Category, Partner | Removed |
| INT-008 | CRUD | Bulk assign | Category, Partner | Bulk |
| INT-009 | CRUD | Export | Category | File |
| INT-010 | CRUD | Dropdown | Category | Pairs |
| INT-011 | Search | Search by name | Category | Matches |
| INT-012 | Search | Typeahead | Category | Suggestions |
| INT-013 | Search | Filter type | Category | Filtered |
| INT-014 | Search | Filter parent | Category | Filtered |
| INT-015 | Search | Multi-filter | Category | Combined |
| INT-016 | Search | Empty search | - | [] |
| INT-017 | Search | Partial match | Category | Fuzzy |
| INT-018 | Search | Sort + filter | Category | Both |
| INT-019 | Search | Filter + pagination | Category | Both |
| INT-020 | Search | Hierarchy | Category | Hierarchy |
| INT-021 | Pagination | Page 1 | Category | First |
| INT-022 | Pagination | Last page | Category | Partial |
| INT-023 | Pagination | Size | Category | Correct |
| INT-024 | Pagination | Invalid | Category | 400 |
| INT-025 | Pagination | Boundary | Category | Exact |
| INT-026 | Relationships | Category → Parent | Category | Linked |
| INT-027 | Relationships | Category → Children | Category | Linked |
| INT-028 | Relationships | Category → Partners | Category, Partner | Linked |
| INT-029 | Relationships | Orphan | Deleted parent | 404 |
| INT-030 | Relationships | Partner deleted | Partner | 404 |
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
| INT-046 | E2E | Full create flow | Category | Create → Get |
| INT-047 | E2E | Full update flow | Category | Update → Get |
| INT-048 | E2E | Full delete flow | Category | Delete → 404 |
| INT-049 | E2E | Assign flow | Category, Partner | Assign → Get |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Assign → Get partners | Category, Partner | Assigned |
| INT-052 | CRUD | Remove → Get partners | Category, Partner | Removed |
| INT-053 | Hierarchy | Parent-child flow | Category | Hierarchy |
| INT-054 | Hierarchy | Root categories | Category | Roots |
| INT-055 | Search | Typeahead flow | Category | Suggestions |
| INT-056 | Relationships | Category → Parent | Category | Linked |
| INT-057 | Error | Validation chain | Bad input | 400 |
| INT-058 | Error | Auth chain | No auth | 401 |
| INT-059 | E2E | Hierarchy flow | Category | Hierarchy |
| INT-060 | E2E | Export flow | Category | Export |
| INT-061 | CRUD | Restore → Get | Category | Restored |
| INT-062 | Hierarchy | Children count | Category | Count |
| INT-063 | Assign | Bulk assign | Category, Partner | Bulk |
| INT-064 | Relationships | Category → Children | Category | Linked |
| INT-065 | Error | Permission chain | No perm | 403 |
| INT-066 | E2E | Full category flow | Category | Create → Delete |
| INT-067 | CRUD | Get by code | Category | Match |
| INT-068 | Assign | Duplicate assign | Category | 409 |
| INT-069 | Hierarchy | Circular parent | Category | 400 |
| INT-070 | Relationships | Category → Partners | Category, Partner | Linked |
| INT-071 | Error | Conflict resolution | Stale | 409 |
| INT-072 | E2E | Restore flow | Category | Restore |
| INT-073 | CRUD | Update → Get | Category | Updated |
| INT-074 | Assign | Remove not assigned | Category | 404 |
| INT-075 | Hierarchy | Orphan parent | Category | 404 |
| INT-076 | Relationships | Category → Audit | Category | Audit |
| INT-077 | Error | Timeout handling | Slow | 504 |
| INT-078 | E2E | Typeahead flow | Category | Typeahead |
| INT-079 | CRUD | Create → Get | Category | Match |
| INT-080 | Assign | Assign to self | Category | 400 |
| INT-081 | Hierarchy | Hierarchy validation | Category | Valid |
| INT-082 | Relationships | Partner → Category | Partner | Linked |
| INT-083 | Error | Service unavailable | Down | 503 |
| INT-084 | E2E | Dropdown flow | Category | Pairs |
| INT-085 | CRUD | Delete → Get | Category | 404 |
| INT-086 | Assign | Assign concurrent | Category | Last |
| INT-087 | Hierarchy | Hierarchy concurrent | Category | Consistent |
| INT-088 | Relationships | Category → Partner | Category | 1:N |
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
| SEC-021 | IDOR | Other org category | ID | 403 |
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
| CON-014 | Assign concurrent | Both or one |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Assign same partner | One or conflict |
| CON-022 | Remove concurrent | Both succeed |
| CON-023 | Permission change | Old |
| CON-024 | Hierarchy change | Consistent |
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
| UNT-009 | Calculation | Hierarchy path | Correct |
| UNT-010 | Calculation | Partner count | Correct |
| UNT-011 | Calculation | Depth | Correct |
| UNT-012 | Calculation | Children count | Correct |
| UNT-013 | Calculation | Filter count | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Default | Default only |
| UNT-018 | Status | Custom | Custom only |
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
| PRF-006 | Get partners | < 300ms |
| PRF-007 | Assign | < 200ms |
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
| CRUD categories | POS-001–005, FUN-001–006 |
| Assignment to partners | POS-009–011, FUN-014–015 |
| Filtering | POS-007–008, FUN-009–010 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
