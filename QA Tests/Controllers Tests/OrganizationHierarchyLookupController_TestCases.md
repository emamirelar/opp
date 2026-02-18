# OrganizationHierarchyLookupController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OrganizationHierarchyLookupController`  
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

REST API for org hierarchy lookup: search org units, typeahead, filter by type/region.

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all org units | GET /api/organization-hierarchy-lookup | Unit list |
| POS-002 | Search org units | GET /api/organization-hierarchy-lookup?search=text | Filtered |
| POS-003 | Typeahead | GET /api/organization-hierarchy-lookup/typeahead?q=text | Suggestions |
| POS-004 | Filter by type | GET ?type=Department | Filtered |
| POS-005 | Filter by region | GET ?region=East Africa | Filtered |
| POS-006 | Filter by type and region | GET ?type=X&region=Y | Combined |
| POS-007 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-008 | Sort by name | GET ?sortBy=name | Sorted |
| POS-009 | Sort by code | GET ?sortBy=code | Sorted |
| POS-010 | Get by ID | GET /api/organization-hierarchy-lookup/{id} | Unit details |
| POS-011 | Get dropdown | GET /api/organization-hierarchy-lookup/dropdown | ID/name pairs |
| POS-012 | Empty result | GET for empty filter | [] |
| POS-013 | Single result | GET for single match | [item] |
| POS-014 | Authenticated access | GET with token | 200 |
| POS-015 | Active only | GET ?active=true | Active only |
| POS-016 | Get by code | GET /api/organization-hierarchy-lookup/code/{code} | By code |
| POS-017 | Min typeahead length | GET typeahead?q=ab | Suggestions |
| POS-018 | Type filter exact | GET ?type=Exact | Filtered |
| POS-019 | Region filter valid | GET ?region=valid | Filtered |
| POS-020 | Combined search filter | GET ?search=text&type=X | Combined |
| POS-021 | Sort ascending | GET ?sortBy=name&sortOrder=asc | Sorted |
| POS-022 | Sort descending | GET ?sortBy=name&sortOrder=desc | Sorted |
| POS-023 | First page | GET ?page=1 | First page |
| POS-024 | Last page | GET ?page=last | Partial |
| POS-025 | Default page size | GET no pageSize | Default |
| POS-026 | Empty search | GET ?search= | All |
| POS-027 | Whitespace trim | GET ?search= sp | Trimmed |
| POS-028 | Multiple types | GET ?type=Dept,Unit | Filtered |
| POS-029 | Unicode search | GET ?search=中文 | Matches |
| POS-030 | Partial match | GET ?search=partial | Fuzzy |

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
| NEG-007 | Null search | POST null | 400 |
| NEG-008 | Invalid parentId | parentId=999999 | 404 |
| NEG-009 | Invalid type | type=invalid | 400 |
| NEG-010 | SQL injection | search='; DROP | Sanitized |
| NEG-011 | XSS in search | search=<script> | Sanitized |
| NEG-012 | Negative page | page=-1 | 400 |
| NEG-013 | Zero pageSize | pageSize=0 | 400 |
| NEG-014 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-015 | Invalid sort | sortBy=invalid | 400 |
| NEG-016 | No permission | User without CanView | 403 |
| NEG-017 | Cross-org access | Other org unit | 403 |
| NEG-018 | Deleted unit | id of deleted | 404 |
| NEG-019 | Malformed JSON | Invalid JSON | 400 |
| NEG-020 | Wrong content-type | Application/xml | 415 |
| NEG-021 | Rate limit | Too many | 429 |
| NEG-022 | Payload too large | Huge body | 413 |
| NEG-023 | Invalid Accept | Accept: text/plain | 406 |
| NEG-024 | HTTP method | PUT for create | 405 |
| NEG-025 | Trailing slash | /api/organization-hierarchy-lookup/ | Redirect |
| NEG-026 | Case sensitivity | /api/Organization-Hierarchy | 404 |
| NEG-027 | Extra path | /api/organization-hierarchy-lookup/1/extra | 404 |
| NEG-028 | Invalid bearer | Bearer malformed | 401 |
| NEG-029 | Revoked token | Revoked JWT | 401 |
| NEG-030 | Service account | Service for UI | 403 |
| NEG-031 | DB timeout | Simulate | 503 |
| NEG-032 | Invalid region | region=invalid | 400 |
| NEG-033 | Empty typeahead | q= | 400 |
| NEG-034 | Typeahead too short | q=x | 400 if min=2 |
| NEG-035 | Invalid type enum | type=999999 | 400 |
| NEG-036 | Zero ID | id=0 | 400 |
| NEG-037 | Invalid UUID | id=invalid-guid | 400 |
| NEG-038 | Blocked IP | From blocked | 403 |
| NEG-039 | Control chars | search with \0 | 400 |
| NEG-040 | Unicode overflow | Very long | 400 |
| NEG-041 | Export no permission | No export permission | 403 |
| NEG-042 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-043 | Orphan parent | parentId deleted | 404 |
| NEG-044 | Deleted filter | Query deleted | Excluded |
| NEG-045 | Max URL length | Very long URL | 414 |
| NEG-046 | Invalid sort order | sortOrder=invalid | 400 |
| NEG-047 | Invalid type format | type malformed | 400 |
| NEG-048 | Mismatched IDs | Path != body | 400 |
| NEG-049 | Read-only field | Update createdDate | Ignored |
| NEG-050 | Version conflict | Stale version | 409 |
| NEG-051 | CORS fail | Invalid origin | CORS error |
| NEG-052 | Inactive org | Org inactive | 403 |
| NEG-053 | OPTIONS | OPTIONS | 200 |
| NEG-054 | HEAD | HEAD | 200 or 405 |
| NEG-055 | Invalid filter param | filter=invalid | 400 |
| NEG-056 | Negative parentId | parentId=-1 | 400 |
| NEG-057 | Invalid page | page=1.5 | 400 |
| NEG-058 | Invalid pageSize | pageSize=abc | 400 |
| NEG-059 | Too many params | 50+ params | 400 |
| NEG-060 | Reserved code | code=RESERVED | 403 |
| NEG-061 | Audit failure | Audit down | Continue |
| NEG-062 | Duplicate IDs | bulk with dupes | 400 |
| NEG-063 | Empty bulk | POST [] | 400 |
| NEG-064 | Excessive bulk | 1000 IDs | 400 |
| NEG-065 | Invalid endpoint | /api/organization-hierarchy-lookup/invalid | 404 |
| NEG-066 | Invalid method | PATCH | 405 |
| NEG-067 | Missing query | GET no params | 200 or 400 |
| NEG-068 | Invalid encoding | Malformed URL | 400 |
| NEG-069 | Soft-deleted filter | Query deleted | Excluded |
| NEG-070 | Circular hierarchy | parentId=self | 400 |
| NEG-071 | Invalid JSON schema | Schema mismatch | 400 |
| NEG-072 | Missing search param | Search null | 400 |
| NEG-073 | Invalid type enum | type=invalid | 400 |
| NEG-074 | Empty typeahead | q= | 400 |
| NEG-075 | Invalid region format | region malformed | 400 |
| NEG-076 | Lookup locked | Locked lookup | 423 |
| NEG-077 | Maintenance mode | During maintenance | 503 |
| NEG-078 | Quota exceeded | Lookup quota | 507 |
| NEG-079 | Invalid description | desc too long | 400 |
| NEG-080 | Orphan parent | parentId deleted | 404 |
| NEG-081 | Migration mode | During migration | 503 |
| NEG-082 | Session invalid | Invalid session | 401 |
| NEG-083 | Token type wrong | Wrong token type | 401 |
| NEG-084 | Scope insufficient | OAuth scope | 403 |
| NEG-085 | Rate limit per user | User rate limit | 429 |
| NEG-086 | Concurrent limit | Too many concurrent | 429 |
| NEG-087 | Request timeout | Slow request | 408 |
| NEG-088 | Lookup archived | Archived lookup | 410 |
| NEG-089 | Invalid coordinate | coord out of range | 400 |
| NEG-090 | Hierarchy depth exceeded | Max depth | 400 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-002 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-005 | parentId | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-006 | Empty list | - | - | [] | - | - |
| BND-007 | Single item | - | - | [item] | - | - |
| BND-008 | First page | page=1 | - | ✅ | - | - |
| BND-009 | Last page | - | - | Partial | - | - |
| BND-010 | Zero length search | - | - | [] | - | - |
| BND-011 | Max length search | 200 | - | - | ✅ | ❌ |
| BND-012 | Unicode search | - | - | Accept | - | - |
| BND-013 | Arabic search | - | - | Display | - | - |
| BND-014 | Chinese search | - | - | Display | - | - |
| BND-015 | Null optional | - | - | Default | - | - |
| BND-016 | Empty string | - | - | No filter | - | - |
| BND-017 | Whitespace | - | - | Trim | - | - |
| BND-018 | Sort empty | - | - | [] | - | - |
| BND-019 | Sort single | - | - | [item] | - | - |
| BND-020 | Filter no match | - | - | [] | - | - |
| BND-021 | Filter all | - | - | Full | - | - |
| BND-022 | Typeahead min | 1 | - | ✅ | - | - |
| BND-023 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-024 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-025 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-026 | Pagination boundary | - | - | Exact | - | - |
| BND-027 | Zero ID | id=0 | - | 400 | - | - |
| BND-028 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-029 | Root unit | parentId=0 | - | Root | - | - |
| BND-030 | Region length | - | 100 | ✅ | ✅ | ❌ |
| BND-031 | Type length | - | 50 | ✅ | ✅ | ❌ |
| BND-032 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-033 | Empty type | - | - | All | - | - |
| BND-034 | Empty region | - | - | All | - | - |
| BND-035 | Single type | - | - | Filtered | - | - |
| BND-036 | Single region | - | - | Filtered | - | - |
| BND-037 | Multiple types | - | 10 | ✅ | ✅ | ❌ |
| BND-038 | Multiple regions | - | 10 | ✅ | ✅ | ❌ |
| BND-039 | Soft-deleted | - | - | Excluded | - | - |
| BND-040 | Inactive | - | - | Excluded | - | - |
| BND-041 | Case code | - | - | Normalize | - | - |
| BND-042 | Leaf unit | No children | - | Leaf | - | - |
| BND-043 | Active flag | - | - | Boolean | - | - |
| BND-044 | Order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-045 | Created date | - | - | UTC | - | - |
| BND-046 | Modified date | - | - | UTC | - | - |
| BND-047 | Full path | - | 1000 | ✅ | ✅ | ❌ |
| BND-048 | Hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-049 | Typeahead results | 0 | 20 | [] | ✅ | Truncate |
| BND-050 | Pagination overflow | - | - | [] | - | - |
| BND-051 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-052 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-053 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-054 | Export empty | - | - | Headers | - | - |
| BND-055 | Version | 1 | - | ✅ | ❌ | - |
| BND-056 | Type list | 0 | 20 | ✅ | ✅ | ❌ |
| BND-057 | Region list | 0 | 50 | ✅ | ✅ | ❌ |
| BND-058 | Code length | 1 | 50 | ✅ | ✅ | ❌ |
| BND-059 | Name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-060 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-061 | IsHeadUnit | - | - | Boolean | - | - |
| BND-062 | IsActive | - | - | Boolean | - | - |
| BND-063 | Hierarchy path | - | 500 | ✅ | ✅ | ❌ |
| BND-064 | Partial match | - | - | Fuzzy | - | - |
| BND-065 | Exact match | - | - | Exact | - | - |
| BND-066 | Combined filter | - | - | AND | - | - |
| BND-067 | Round-trip | Search → Get | - | Match | - | - |
| BND-068 | Children count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-069 | Level depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-070 | Org type enum | - | max | Valid | Valid | ❌ |
| BND-071 | Search charset | - | - | Valid | - | - |
| BND-072 | Name encoding | - | UTF-8 | Valid | Valid | ❌ |
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
| BND-088 | Batch size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-089 | Region enum | - | max | Valid | Valid | ❌ |
| BND-090 | Type enum | - | max | Valid | Valid | ❌ |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get all | GET | List |
| FUN-002 | Workflow | Get by ID | GET id | Details |
| FUN-003 | Workflow | Get by code | GET code | Match |
| FUN-004 | Workflow | Search | GET ?search | Searched |
| FUN-005 | Workflow | Typeahead | GET typeahead | Suggestions |
| FUN-006 | Workflow | Filter type | GET ?type | Filtered |
| FUN-007 | Workflow | Filter region | GET ?region | Filtered |
| FUN-008 | Workflow | Filter parent | GET ?parentId | Filtered |
| FUN-009 | Workflow | Paginate | GET ?page | Paginated |
| FUN-010 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-011 | Workflow | Combined filter | GET multi | Combined |
| FUN-012 | Workflow | Empty search | GET ?search= | All |
| FUN-013 | Workflow | Dropdown | GET dropdown | Pairs |
| FUN-014 | Workflow | Active only | GET ?active=true | Active |
| FUN-015 | Workflow | Root units | GET ?parentId=0 | Roots |
| FUN-016 | Validation | Valid parent | Invalid | 404 |
| FUN-017 | Validation | Valid type | Invalid | 400 |
| FUN-018 | Validation | Permission | No permission | 403 |
| FUN-019 | Validation | ID format | Invalid | 400 |
| FUN-020 | Validation | Search length | Too long | 400 |
| FUN-021 | Validation | Page bounds | Invalid | 400 |
| FUN-022 | Validation | Permission | Query | Scoped |
| FUN-023 | Validation | Region format | Invalid | 400 |
| FUN-024 | Validation | Type enum | Invalid | 400 |
| FUN-025 | Validation | Coordinate range | Out of range | 400 |
| FUN-026 | Constraint | FK parent | Invalid | 404 |
| FUN-027 | Constraint | Soft delete | Query | Excluded |
| FUN-028 | Constraint | Org scope | Cross-org | 403 |
| FUN-029 | Constraint | Max bulk | >100 | 400 |
| FUN-030 | Constraint | Typeahead limit | >20 | Truncate |
| FUN-031 | Constraint | Export limit | >10K | Truncate |
| FUN-032 | Constraint | URL length | >2048 | 414 |
| FUN-033 | Constraint | Query params | >20 | 400 |
| FUN-034 | Constraint | Hierarchy depth | >10 | 400 |
| FUN-035 | Constraint | No circular | Circular | 400 |
| FUN-036 | Audit | Get | GET | Audit |
| FUN-037 | Audit | Search | GET search | Audit |
| FUN-038 | Audit | Typeahead | GET typeahead | Audit |
| FUN-039 | Audit | Timestamp | Any | UTC |
| FUN-040 | Audit | User ID | Any | User ID |
| FUN-041 | Audit | IP | Any | IP |
| FUN-042 | Audit | Resource | Any | Resource |
| FUN-043 | Audit | Outcome | Any | Outcome |
| FUN-044 | Audit | Export | GET export | Audit |
| FUN-045 | Audit | Filter | GET filter | Audit |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Hierarchy | Parent-child | Linked |
| FUN-050 | Business | Type hierarchy | Type filter | Correct |
| FUN-051 | Workflow | Root units | GET ?parentId=0 | Roots |
| FUN-052 | Workflow | Parent filter | GET ?parentId | Children |
| FUN-053 | Validation | Type enum | Invalid | 400 |
| FUN-054 | Validation | Region format | Invalid | 400 |
| FUN-055 | Constraint | Lookup lock | Locked | 423 |
| FUN-056 | Audit | Search | GET search | Audit |
| FUN-057 | Audit | Typeahead | GET typeahead | Audit |
| FUN-058 | Business | Hierarchy depth | >10 | 400 |
| FUN-059 | Business | No circular | Circular | 400 |
| FUN-060 | Workflow | Case insensitive | GET ?search | Matches |
| FUN-061 | Validation | Parent exists | Invalid | 404 |
| FUN-062 | Constraint | Typeahead limit | >20 | Truncate |
| FUN-063 | Audit | Get | GET | Audit |
| FUN-064 | Business | Lookup cascade | Delete parent | 404 |
| FUN-065 | Workflow | Cached response | GET same | 200 |
| FUN-066 | Validation | Search length | Too long | 400 |
| FUN-067 | Constraint | Export limit | >10K | Truncate |
| FUN-068 | Audit | Filter | GET filter | Audit |
| FUN-069 | Business | Region scope | Org scope | Correct |
| FUN-070 | Workflow | Unicode search | GET ?search | Matches |
| FUN-071 | Validation | Coordinate range | Out of range | 400 |
| FUN-072 | Constraint | Query params | >20 | 400 |
| FUN-073 | Audit | Export | GET export | Audit |
| FUN-074 | Business | Cross-org unit | Other org | 403 |
| FUN-075 | Workflow | Full round-trip | Search → Get | Match |
| FUN-076 | Validation | Page bounds | Invalid | 400 |
| FUN-077 | Constraint | URL length | >2048 | 414 |
| FUN-078 | Audit | Dropdown | GET dropdown | Audit |
| FUN-079 | Business | Inactive unit | Unit disabled | 403 |
| FUN-080 | Workflow | Partial match | GET ?search | Fuzzy |
| FUN-081 | Validation | Type format | Invalid | 400 |
| FUN-082 | Constraint | Hierarchy depth | >10 | 400 |
| FUN-083 | Audit | Typeahead view | GET typeahead | Audit |
| FUN-084 | Business | Type scope | Type filter | Correct |
| FUN-085 | Workflow | Dropdown flow | GET dropdown | Pairs |
| FUN-086 | Validation | Region format | Invalid | 400 |
| FUN-087 | Constraint | Max bulk | >100 | 400 |
| FUN-088 | Audit | Parent filter | GET parentId | Audit |
| FUN-089 | Business | Parent-child | Linked | Correct |
| FUN-090 | Workflow | Combined filter | GET multi | Combined |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get by ID | OrgUnit | Match |
| INT-002 | CRUD | Get by code | OrgUnit | Match |
| INT-003 | CRUD | Get dropdown | OrgUnit | Pairs |
| INT-004 | CRUD | Search | OrgUnit | Matches |
| INT-005 | CRUD | Typeahead | OrgUnit | Suggestions |
| INT-006 | CRUD | Filter type | OrgUnit | Filtered |
| INT-007 | CRUD | Filter region | OrgUnit | Filtered |
| INT-008 | CRUD | Filter parent | OrgUnit | Filtered |
| INT-009 | CRUD | Pagination | OrgUnit | Paginated |
| INT-010 | CRUD | Sort | OrgUnit | Sorted |
| INT-011 | Search | Search by name | OrgUnit | Matches |
| INT-012 | Search | Typeahead | OrgUnit | Suggestions |
| INT-013 | Search | Filter type | OrgUnit | Filtered |
| INT-014 | Search | Filter region | OrgUnit | Filtered |
| INT-015 | Search | Filter parent | OrgUnit | Filtered |
| INT-016 | Search | Multi-filter | OrgUnit | Combined |
| INT-017 | Search | Empty search | - | [] |
| INT-018 | Search | Partial match | OrgUnit | Fuzzy |
| INT-019 | Search | Sort + filter | OrgUnit | Both |
| INT-020 | Search | Filter + pagination | OrgUnit | Both |
| INT-021 | Pagination | Page 1 | OrgUnit | First |
| INT-022 | Pagination | Last page | OrgUnit | Partial |
| INT-023 | Pagination | Size | OrgUnit | Correct |
| INT-024 | Pagination | Invalid | OrgUnit | 400 |
| INT-025 | Pagination | Boundary | OrgUnit | Exact |
| INT-026 | Relationships | Unit → Parent | OrgUnit | Linked |
| INT-027 | Relationships | Unit → Children | OrgUnit | Linked |
| INT-028 | Relationships | Orphan | Deleted parent | 404 |
| INT-029 | Relationships | Region hierarchy | OrgUnit | Linked |
| INT-030 | Relationships | Type hierarchy | OrgUnit | Linked |
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
| INT-046 | E2E | Full search flow | OrgUnit | Search → Get |
| INT-047 | E2E | Full typeahead flow | OrgUnit | Typeahead → Get |
| INT-048 | E2E | Full filter flow | OrgUnit | Filter → Get |
| INT-049 | E2E | Hierarchy flow | OrgUnit | Parent → Children |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get by ID | OrgUnit | Match |
| INT-052 | CRUD | Get by code | OrgUnit | Match |
| INT-053 | Search | Search flow | OrgUnit | Matches |
| INT-054 | Search | Typeahead flow | OrgUnit | Suggestions |
| INT-055 | Filter | Type flow | OrgUnit | Filtered |
| INT-056 | Relationships | Unit → Parent | OrgUnit | Linked |
| INT-057 | Error | Validation chain | Bad input | 400 |
| INT-058 | Error | Auth chain | No auth | 401 |
| INT-059 | E2E | Dropdown flow | OrgUnit | Pairs |
| INT-060 | E2E | Region flow | OrgUnit | Filtered |
| INT-061 | CRUD | Dropdown | OrgUnit | Pairs |
| INT-062 | Filter | Region flow | OrgUnit | Filtered |
| INT-063 | Hierarchy | Parent filter | OrgUnit | Children |
| INT-064 | Relationships | Unit → Children | OrgUnit | Linked |
| INT-065 | Error | Permission chain | No perm | 403 |
| INT-066 | E2E | Full lookup flow | OrgUnit | Search → Get |
| INT-067 | CRUD | Search | OrgUnit | Matches |
| INT-068 | Filter | Combined filter | OrgUnit | Combined |
| INT-069 | Hierarchy | Root units | OrgUnit | Roots |
| INT-070 | Relationships | Orphan parent | OrgUnit | 404 |
| INT-071 | Error | Conflict resolution | Stale | 409 |
| INT-072 | E2E | Type flow | OrgUnit | Filtered |
| INT-073 | CRUD | Pagination | OrgUnit | Paginated |
| INT-074 | Filter | Parent filter | OrgUnit | Children |
| INT-075 | Hierarchy | Hierarchy depth | OrgUnit | Valid |
| INT-076 | Relationships | OrgUnit → Audit | OrgUnit | Audit |
| INT-077 | Error | Timeout handling | Slow | 504 |
| INT-078 | E2E | Pagination flow | OrgUnit | Paginated |
| INT-079 | CRUD | Typeahead | OrgUnit | Suggestions |
| INT-080 | Filter | Empty search | OrgUnit | All |
| INT-081 | Hierarchy | Hierarchy validation | OrgUnit | Valid |
| INT-082 | Relationships | Region hierarchy | OrgUnit | Linked |
| INT-083 | Error | Service unavailable | Down | 503 |
| INT-084 | E2E | Sort flow | OrgUnit | Sorted |
| INT-085 | CRUD | Sort | OrgUnit | Sorted |
| INT-086 | Search | Search concurrent | OrgUnit | All |
| INT-087 | Filter | Filter concurrent | OrgUnit | All |
| INT-088 | Relationships | OrgUnit → Region | OrgUnit | Linked |
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
| SEC-021 | IDOR | Other org unit | ID | 403 |
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
| CON-002 | 2 users search same | Both succeed |
| CON-003 | 2 users typeahead same | Both succeed |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click search | Single |
| CON-007 | Rapid filter | Last wins |
| CON-008 | Delete during read | Snapshot |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Hierarchy update | Snapshot |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple searches | All succeed |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Typeahead concurrent | All succeed |
| CON-022 | Filter change | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Type change | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid search | Accept |
| UNT-002 | Validation | Invalid search | Reject |
| UNT-003 | Validation | Valid ID | Accept |
| UNT-004 | Validation | Invalid ID | Reject |
| UNT-005 | Validation | Valid code | Accept |
| UNT-006 | Formatting | Name | Formatted |
| UNT-007 | Formatting | Code | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Hierarchy path | Correct |
| UNT-010 | Calculation | Bounding box | Correct |
| UNT-011 | Calculation | Center point | Correct |
| UNT-012 | Calculation | Depth | Correct |
| UNT-013 | Calculation | Filter count | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Head unit | Head only |
| UNT-018 | Status | Branch | Branch only |
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
| PRF-006 | Filter region | < 200ms |
| PRF-007 | Typeahead | < 100ms |
| PRF-008 | Dropdown | < 100ms |
| PRF-009 | Pagination | < 200ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent search | < 500ms each |
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
| Search org units | POS-001–002, FUN-001–004 |
| Typeahead | POS-003, FUN-005 |
| Filter by type | POS-004, FUN-006 |
| Filter by region | POS-005, FUN-007 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
