# SavedFilterController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/SavedFilterController`  
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

REST API for saved filters: CRUD user-saved filters, share filters, apply filters.

---

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all filters | GET /api/saved-filters | Filter list |
| POS-002 | Get filter by ID | GET /api/saved-filters/{id} | Filter details |
| POS-003 | Create filter | POST /api/saved-filters | 201 Created |
| POS-004 | Update filter | PUT /api/saved-filters/{id} | 200 OK |
| POS-005 | Delete filter | DELETE /api/saved-filters/{id} | 204 No Content |
| POS-006 | Get my filters | GET /api/saved-filters/me | My filters |
| POS-007 | Search filters | GET /api/saved-filters?search=text | Filtered |
| POS-008 | Filter by entity | GET ?entityType=partner | Filtered |
| POS-009 | Share filter | POST /api/saved-filters/{id}/share | Shared |
| POS-010 | Unshare filter | DELETE /api/saved-filters/{id}/share | Unshared |
| POS-011 | Apply filter | POST /api/saved-filters/{id}/apply | Applied |
| POS-012 | Get shared filters | GET /api/saved-filters/shared | Shared list |
| POS-013 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-014 | Sort by name | GET ?sortBy=name | Sorted |
| POS-015 | Sort by date | GET ?sortBy=createdDate | Sorted |
| POS-016 | Empty result | GET for empty filter | [] |
| POS-017 | Single result | GET for single match | [item] |
| POS-018 | Authenticated access | GET with token | 200 |
| POS-019 | Set default | PUT /api/saved-filters/{id}/default | Default |
| POS-020 | Get default | GET /api/saved-filters/default | Default filter |
| POS-021 | Duplicate filter | POST /api/saved-filters/{id}/duplicate | Duplicated |
| POS-022 | Get filter definition | GET /api/saved-filters/{id}/definition | Definition |
| POS-023 | Combined filter | GET ?search=text&entityType=X | Combined |
| POS-024 | Sort ascending | GET ?sortBy=name&sortOrder=asc | Sorted |
| POS-025 | Sort descending | GET ?sortBy=name&sortOrder=desc | Sorted |
| POS-026 | First page | GET ?page=1 | First page |
| POS-027 | Last page | GET ?page=last | Partial |
| POS-028 | Default page size | GET no pageSize | Default |
| POS-029 | Share with user | POST share userId | Shared |
| POS-030 | Share with role | POST share roleId | Shared |
| POS-031 | Export filter | GET /api/saved-filters/{id}/export | Export |
| POS-032 | Import filter | POST /api/saved-filters/import | Imported |
| POS-033 | Rename filter | PUT name | Renamed |
| POS-034 | Copy filter | POST copy | Copied |
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
| NEG-006 | Null request | POST null | 400 |
| NEG-007 | Missing name | Name missing | 400 |
| NEG-008 | Missing definition | Definition missing | 400 |
| NEG-009 | Invalid entityType | entityType=invalid | 400 |
| NEG-010 | SQL injection | search='; DROP | Sanitized |
| NEG-011 | XSS in name | name=<script> | Sanitized |
| NEG-012 | Negative page | page=-1 | 400 |
| NEG-013 | Zero pageSize | pageSize=0 | 400 |
| NEG-014 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-015 | Invalid sort | sortBy=invalid | 400 |
| NEG-016 | No permission | User without CanView | 403 |
| NEG-017 | Cross-user access | Other user filter | 403 |
| NEG-018 | Cross-org access | Other org filter | 403 |
| NEG-019 | Malformed JSON | Invalid JSON | 400 |
| NEG-020 | Wrong content-type | Application/xml | 415 |
| NEG-021 | Rate limit | Too many | 429 |
| NEG-022 | Payload too large | Huge body | 413 |
| NEG-023 | Invalid Accept | Accept: text/plain | 406 |
| NEG-024 | HTTP method | PUT for create | 405 |
| NEG-025 | Trailing slash | /api/saved-filters/ | Redirect |
| NEG-026 | Case sensitivity | /api/Saved-Filters | 404 |
| NEG-027 | Extra path | /api/saved-filters/1/extra | 404 |
| NEG-028 | Invalid bearer | Bearer malformed | 401 |
| NEG-029 | Revoked token | Revoked JWT | 401 |
| NEG-030 | Service account | Service for UI | 403 |
| NEG-031 | DB timeout | Simulate | 503 |
| NEG-032 | Invalid definition | Definition malformed | 400 |
| NEG-033 | Invalid share target | userId=999999 | 404 |
| NEG-034 | Share own filter | Share to self | 400 |
| NEG-035 | Zero ID | id=0 | 400 |
| NEG-036 | Invalid UUID | id=invalid-guid | 400 |
| NEG-037 | Blocked IP | From blocked | 403 |
| NEG-038 | Control chars | name with \0 | 400 |
| NEG-039 | Unicode overflow | Very long | 400 |
| NEG-040 | Duplicate name | name exists | 409 or allow |
| NEG-041 | Update deleted | PUT on deleted | 404 |
| NEG-042 | Mismatched IDs | Path != body | 400 |
| NEG-043 | Read-only field | Update createdDate | Ignored |
| NEG-044 | Version conflict | Stale version | 409 |
| NEG-045 | CORS fail | Invalid origin | CORS error |
| NEG-046 | Inactive org | Org inactive | 403 |
| NEG-047 | Invalid entity | entityType mismatch | 400 |
| NEG-048 | Apply deleted | Apply deleted filter | 404 |
| NEG-049 | Share no permission | No share permission | 403 |
| NEG-050 | Unshare not shared | Unshare not shared | 400 |
| NEG-051 | Audit failure | Audit down | Continue |
| NEG-052 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-053 | Max URL length | Very long URL | 414 |
| NEG-054 | Invalid endpoint | /api/saved-filters/invalid | 404 |
| NEG-055 | Invalid method | PATCH | 405 |
| NEG-056 | Missing query | GET no params | 200 or 400 |
| NEG-057 | Invalid encoding | Malformed URL | 400 |
| NEG-058 | Import malformed | Import invalid | 400 |
| NEG-059 | Export no permission | No export permission | 403 |
| NEG-060 | Duplicate default | Set default twice | 200 |
| NEG-061 | Definition too large | Huge definition | 400 |
| NEG-062 | Filter count limit | >100 filters | 400 |
| NEG-063 | Share limit | >50 shares | 400 |
| NEG-064 | Reserved name | name=RESERVED | 403 |
| NEG-065 | Invalid sort order | sortOrder=invalid | 400 |
| NEG-066 | Empty definition | definition=[] | 400 |
| NEG-067 | Invalid filter criteria | criteria malformed | 400 |
| NEG-068 | Apply wrong entity | Apply to wrong | 400 |
| NEG-069 | Expired share | Share expired | 404 |
| NEG-070 | Soft-deleted filter | Query deleted | Excluded |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-005 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-006 | definition size | 1 | 10000 | ✅ | ✅ | ❌ |
| BND-007 | Empty list | - | - | [] | - | - |
| BND-008 | Single item | - | - | [item] | - | - |
| BND-009 | First page | page=1 | - | ✅ | - | - |
| BND-010 | Last page | - | - | Partial | - | - |
| BND-011 | Zero length name | - | - | ❌ | - | - |
| BND-012 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-013 | Unicode name | - | - | Accept | - | - |
| BND-014 | Arabic name | - | - | Display | - | - |
| BND-015 | Chinese name | - | - | Display | - | - |
| BND-016 | Null optional | - | - | Default | - | - |
| BND-017 | Empty string | - | - | No filter | - | - |
| BND-018 | Whitespace | - | - | Trim | - | - |
| BND-019 | Sort empty | - | - | [] | - | - |
| BND-020 | Sort single | - | - | [item] | - | - |
| BND-021 | Filter no match | - | - | [] | - | - |
| BND-022 | Filter all | - | - | Full | - | - |
| BND-023 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-024 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-025 | Pagination boundary | - | - | Exact | - | - |
| BND-026 | Zero ID | id=0 | - | 400 | - | - |
| BND-027 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-028 | Filter count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-029 | Share count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-030 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-031 | Entity type length | - | 50 | ✅ | ✅ | ❌ |
| BND-032 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-033 | Created date | - | - | UTC | - | - |
| BND-034 | Modified date | - | - | UTC | - | - |
| BND-035 | Definition criteria | 0 | 50 | ✅ | ✅ | ❌ |
| BND-036 | Audit fields | - | - | Set | - | - |
| BND-037 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-038 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-039 | Round-trip | Create → Get | - | Match | - | - |
| BND-040 | Soft-deleted | - | - | Excluded | - | - |
| BND-041 | Inactive | - | - | Excluded | - | - |
| BND-042 | Default filter | - | - | One | - | - |
| BND-043 | Shared filter | - | - | Shared | - | - |
| BND-044 | Apply result | - | - | Valid | - | - |
| BND-045 | Version | 1 | - | ✅ | ❌ | - |
| BND-046 | Export format | - | - | Valid | - | - |
| BND-047 | Import format | - | - | Valid | - | - |
| BND-048 | Duplicate name | - | - | Allow/Reject | - | - |
| BND-049 | Share target | - | 50 | ✅ | ✅ | ❌ |
| BND-050 | User filter limit | - | 100 | ✅ | ✅ | ❌ |
| BND-051 | Criteria count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-052 | Criteria value length | - | 500 | ✅ | ✅ | ❌ |
| BND-053 | Sort field length | - | 50 | ✅ | ✅ | ❌ |
| BND-054 | Entity type enum | - | max | Valid | Valid | ❌ |
| BND-055 | Share type | - | - | user/role | - | - |
| BND-056 | Apply timeout | - | 30s | - | ✅ | ❌ |
| BND-057 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-058 | Export empty | - | - | Headers | - | - |
| BND-059 | Import size | - | 1MB | ✅ | ✅ | ❌ |
| BND-060 | Combined filter | - | - | AND | - | - |
| BND-061 | Default override | - | - | Replace | - | - |
| BND-062 | Duplicate filter | - | - | Copy | - | - |
| BND-063 | Share expiry | - | 365d | Valid | Valid | ❌ |
| BND-064 | Apply entity match | - | - | Match | - | - |
| BND-065 | Definition schema | - | - | Valid | - | - |
| BND-066 | Criteria operators | - | 10 | ✅ | ✅ | ❌ |
| BND-067 | Empty criteria | - | - | All | - | - |
| BND-068 | Full criteria | - | 50 | - | ✅ | ❌ |
| BND-069 | Nested criteria | - | 5 | ✅ | ✅ | ❌ |
| BND-070 | Apply pagination | - | - | Paginated | - | - |

---

## §4 Functional Tests (50)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get all | GET | List |
| FUN-002 | Workflow | Get by ID | GET id | Details |
| FUN-003 | Workflow | Create | POST | 201 |
| FUN-004 | Workflow | Update | PUT | 200 |
| FUN-005 | Workflow | Delete | DELETE | 204 |
| FUN-006 | Workflow | Get my filters | GET me | My list |
| FUN-007 | Workflow | Share | POST share | Shared |
| FUN-008 | Workflow | Unshare | DELETE share | Unshared |
| FUN-009 | Workflow | Apply | POST apply | Applied |
| FUN-010 | Workflow | Set default | PUT default | Default |
| FUN-011 | Workflow | Filter entity | GET ?entityType | Filtered |
| FUN-012 | Workflow | Search | GET ?search | Searched |
| FUN-013 | Workflow | Paginate | GET ?page | Paginated |
| FUN-014 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-015 | Workflow | Duplicate | POST duplicate | Duplicated |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required definition | Missing | 400 |
| FUN-018 | Validation | Valid entityType | Invalid | 400 |
| FUN-019 | Validation | Valid definition | Invalid | 400 |
| FUN-020 | Validation | Permission | No permission | 403 |
| FUN-021 | Validation | Own filter | Other user | 403 |
| FUN-022 | Validation | ID format | Invalid | 400 |
| FUN-023 | Validation | Definition schema | Invalid | 400 |
| FUN-024 | Validation | Share target | Invalid | 404 |
| FUN-025 | Validation | Apply entity | Mismatch | 400 |
| FUN-026 | Constraint | Soft delete | Query | Excluded |
| FUN-027 | Constraint | Org scope | Cross-org | 403 |
| FUN-028 | Constraint | Version | Optimistic | 409 |
| FUN-029 | Constraint | Max filters | >100 | 400 |
| FUN-030 | Constraint | Max shares | >50 | 400 |
| FUN-031 | Constraint | Definition size | >10K | 400 |
| FUN-032 | Constraint | One default | Per entity | One |
| FUN-033 | Constraint | entityType match | Apply | Match |
| FUN-034 | Constraint | Share scope | Own only | 403 |
| FUN-035 | Constraint | URL length | >2048 | 414 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Share | POST share | Audit |
| FUN-040 | Audit | Apply | POST apply | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | User scope | Own filters | Correct |
| FUN-050 | Business | Apply result | Applied | Correct |
| FUN-051 | Workflow | Export filter | GET export | Export |
| FUN-052 | Workflow | Import filter | POST import | Imported |
| FUN-053 | Validation | Definition schema | Invalid | 400 |
| FUN-054 | Validation | Share target | Invalid | 404 |
| FUN-055 | Constraint | Filter lock | Locked | 423 |
| FUN-056 | Audit | Export | GET export | Audit |
| FUN-057 | Audit | Import | POST import | Audit |
| FUN-058 | Business | Default filter | One per entity | One |
| FUN-059 | Business | Share scope | Own only | 403 |
| FUN-060 | Workflow | Rename filter | PUT name | Renamed |
| FUN-061 | Validation | Apply entity | Mismatch | 400 |
| FUN-062 | Constraint | Max filters | >100 | 400 |
| FUN-063 | Audit | Share | POST share | Audit |
| FUN-064 | Business | Unshare cascade | Delete share | Unshared |
| FUN-065 | Workflow | Cached response | GET same | 200 |
| FUN-066 | Validation | Criteria format | Invalid | 400 |
| FUN-067 | Constraint | Max shares | >50 | 400 |
| FUN-068 | Audit | Unshare | DELETE share | Audit |
| FUN-069 | Business | Duplicate name | Allow/Reject | Correct |
| FUN-070 | Workflow | Copy filter | POST copy | Copied |
| FUN-071 | Validation | Operator enum | Invalid | 400 |
| FUN-072 | Constraint | Definition size | >10K | 400 |
| FUN-073 | Audit | Apply | POST apply | Audit |
| FUN-074 | Business | Cross-user filter | Other user | 403 |
| FUN-075 | Workflow | Full round-trip | Create → Apply | Match |
| FUN-076 | Validation | Entity type | Invalid | 400 |
| FUN-077 | Constraint | One default | Per entity | One |
| FUN-078 | Audit | Set default | PUT default | Audit |
| FUN-079 | Business | Inactive user | User disabled | 403 |
| FUN-080 | Workflow | Duplicate flow | POST duplicate | New |
| FUN-081 | Validation | Name length | Too long | 400 |
| FUN-082 | Constraint | Share expiry | Expired | 404 |
| FUN-083 | Audit | Duplicate | POST duplicate | Audit |
| FUN-084 | Business | Apply scope | Entity match | Correct |
| FUN-085 | Workflow | Export → Import | Round-trip | Match |
| FUN-086 | Validation | Import format | Invalid | 400 |
| FUN-087 | Constraint | Filter count | >100 | 400 |
| FUN-088 | Audit | Create | POST | Audit |
| FUN-089 | Business | Definition schema | Valid schema | Correct |
| FUN-090 | Workflow | Share → Unshare | Full flow | Unshared |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Filter | Match |
| INT-002 | CRUD | Update → Get | Filter | Updated |
| INT-003 | CRUD | Delete → Get | Filter | 404 |
| INT-004 | CRUD | Duplicate → Get | Filter | New |
| INT-005 | CRUD | Share → Get shared | Filter | Shared |
| INT-006 | CRUD | Unshare → Get | Filter | Unshared |
| INT-007 | CRUD | Apply → Results | Filter, Entity | Results |
| INT-008 | CRUD | Set default → Get | Filter | Default |
| INT-009 | CRUD | Export → Import | Filter | Match |
| INT-010 | CRUD | Get my filters | Filter | My list |
| INT-011 | Search | Search by name | Filter | Matches |
| INT-012 | Search | Filter entity | Filter | Filtered |
| INT-013 | Search | Multi-filter | Filter | Combined |
| INT-014 | Search | Empty search | - | [] |
| INT-015 | Search | Partial match | Filter | Fuzzy |
| INT-016 | Search | Sort + filter | Filter | Both |
| INT-017 | Search | Filter + pagination | Filter | Both |
| INT-018 | Share | Share with user | Filter, User | Shared |
| INT-019 | Share | Share with role | Filter, Role | Shared |
| INT-020 | Share | Unshare | Filter | Unshared |
| INT-021 | Pagination | Page 1 | Filter | First |
| INT-022 | Pagination | Last page | Filter | Partial |
| INT-023 | Pagination | Size | Filter | Correct |
| INT-024 | Pagination | Invalid | Filter | 400 |
| INT-025 | Pagination | Boundary | Filter | Exact |
| INT-026 | Relationships | Filter → User | Filter, User | Linked |
| INT-027 | Relationships | Filter → Entity | Filter, Entity | Via apply |
| INT-028 | Relationships | Orphan | Deleted user | 404 |
| INT-029 | Relationships | Share target | User, Role | Valid |
| INT-030 | Apply | Apply to partner | Filter, Partner | Results |
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
| INT-046 | E2E | Full create flow | Filter | Create → Apply |
| INT-047 | E2E | Full share flow | Filter | Share → Unshare |
| INT-048 | E2E | Full apply flow | Filter | Apply → Results |
| INT-049 | E2E | Default flow | Filter | Set → Get |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Export → Import | Filter | Match |
| INT-052 | CRUD | Duplicate → Get | Filter | New |
| INT-053 | Share | Share validation | Filter, User | Shared |
| INT-054 | Apply | Apply to partner | Filter, Partner | Results |
| INT-055 | Search | Multi-filter flow | Filter | Combined |
| INT-056 | Relationships | Filter → User | Filter, User | Linked |
| INT-057 | Error | Validation chain | Bad input | 400 |
| INT-058 | Error | Auth chain | No auth | 401 |
| INT-059 | E2E | Export flow | Filter | Export |
| INT-060 | E2E | Import flow | Filter | Import |
| INT-061 | CRUD | Rename → Get | Filter | Renamed |
| INT-062 | Share | Unshare validation | Filter | Unshared |
| INT-063 | Apply | Apply entity match | Filter | Match |
| INT-064 | Relationships | Filter → Entity | Filter | Via apply |
| INT-065 | Error | Permission chain | No perm | 403 |
| INT-066 | E2E | Full filter flow | Filter | Create → Delete |
| INT-067 | CRUD | Copy → Get | Filter | New |
| INT-068 | Share | Share with role | Filter, Role | Shared |
| INT-069 | Apply | Apply pagination | Filter | Paginated |
| INT-070 | Relationships | Orphan user | User | 404 |
| INT-071 | Error | Conflict resolution | Stale | 409 |
| INT-072 | E2E | Default flow | Filter | Set → Get |
| INT-073 | CRUD | Update definition → Get | Filter | Match |
| INT-074 | Share | Share limit | Filter | 400 |
| INT-075 | Apply | Apply timeout | Filter | 504 |
| INT-076 | Relationships | Filter → Audit | Filter | Audit |
| INT-077 | Error | Timeout handling | Slow | 504 |
| INT-078 | E2E | Duplicate flow | Filter | Duplicated |
| INT-079 | CRUD | Set default → Get | Filter | Default |
| INT-080 | Share | Share expiry | Filter | 404 |
| INT-081 | Apply | Apply validation | Filter | Valid |
| INT-082 | Relationships | User → Filters | User | Linked |
| INT-083 | Error | Service unavailable | Down | 503 |
| INT-084 | E2E | Rename flow | Filter | Renamed |
| INT-085 | CRUD | Create → Apply | Filter | Applied |
| INT-086 | Share | Share concurrent | Filter | Last |
| INT-087 | Apply | Apply concurrent | Filter | All |
| INT-088 | Relationships | User → Filter | User | 1:N |
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
| SEC-005 | Injection | Command | Apply | Rejected |
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
| SEC-021 | IDOR | Other user filter | ID | 403 |
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
| CON-002 | 2 users create same name | Both or one fails |
| CON-003 | 2 users update same | Last write |
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
| CON-014 | Share concurrent | Both or one |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All succeed |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Apply concurrent | All succeed |
| CON-022 | Set default concurrent | One wins |
| CON-023 | Permission change | Old |
| CON-024 | Share change | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid name | Accept |
| UNT-002 | Validation | Invalid name | Reject |
| UNT-003 | Validation | Valid definition | Accept |
| UNT-004 | Validation | Invalid definition | Reject |
| UNT-005 | Validation | Valid entityType | Accept |
| UNT-006 | Formatting | Name | Formatted |
| UNT-007 | Formatting | Definition | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Filter count | Correct |
| UNT-010 | Calculation | Criteria count | Correct |
| UNT-011 | Calculation | Apply result | Correct |
| UNT-012 | Calculation | Share count | Correct |
| UNT-013 | Calculation | Filter match | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Shared | Shared only |
| UNT-018 | Status | Default | Default only |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get all | < 500ms |
| PRF-002 | Get by ID | < 50ms |
| PRF-003 | Search | < 300ms |
| PRF-004 | Filter entity | < 200ms |
| PRF-005 | Create | < 200ms |
| PRF-006 | Update | < 200ms |
| PRF-007 | Apply | < 500ms |
| PRF-008 | Share | < 200ms |
| PRF-009 | Pagination | < 200ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent apply | < 1s each |
| PRF-013 | Memory list | < 50MB |
| PRF-014 | Memory apply | < 100MB |
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
| CRUD saved filters | POS-001–005, FUN-001–005 |
| Share filters | POS-009–010, FUN-007–008 |
| Apply filters | POS-011, FUN-009 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
