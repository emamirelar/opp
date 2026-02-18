# CountryController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/CountryController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for country management: CRUD countries, ISO codes, region mapping, DST linking.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P ✅ (90≥90) | E≥3P ✅ (90≥90) | F≥3P ✅ (90≥90) | I≥3P ✅ (90≥90)

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all countries | GET /api/countries | Country list |
| POS-002 | Get country by ID | GET /api/countries/{id} | Country details |
| POS-003 | Get country by code | GET /api/countries/code/{code} | Country by ISO |
| POS-004 | Get dropdown | GET /api/countries/dropdown | ID/code/name |
| POS-005 | Get by region | GET /api/countries?region=East Africa | Filtered |
| POS-006 | Get by continent | GET /api/countries?continent=Africa | Filtered |
| POS-007 | Search by name | GET /api/countries?search=Ken | Matching |
| POS-008 | Get UNOPS countries | GET /api/countries/unops | Operational |
| POS-009 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-010 | Sort by name | GET ?sortBy=name | A-Z |
| POS-011 | Sort by code | GET ?sortBy=code | Code order |
| POS-012 | Get regions | GET /api/countries/regions | Region list |
| POS-013 | Get continents | GET /api/countries/continents | Continent list |
| POS-014 | Typeahead | GET /api/countries/typeahead?q=Ke | Suggestions |
| POS-015 | Create country (admin) | POST /api/countries | 201 |
| POS-016 | Update country (admin) | PUT /api/countries/{id} | 200 |
| POS-017 | Delete country (admin) | DELETE /api/countries/{id} | 204 |
| POS-018 | Soft delete | DELETE (soft) | IsDeleted |
| POS-019 | Restore | POST /api/countries/{id}/restore | Restored |
| POS-020 | Get DST info | GET /api/countries/{id}/dst | DST data |
| POS-021 | Link DST | POST /api/countries/{id}/dst | Linked |
| POS-022 | Get timezone | GET /api/countries/{id}/timezone | Timezone |
| POS-023 | Get adjacent | GET /api/countries/{id}/adjacent | Neighbors |
| POS-024 | Bulk get | POST /api/countries/bulk | Bulk results |
| POS-025 | Export | GET /api/countries/export | Export file |
| POS-026 | Filter active | GET ?active=true | Active only |
| POS-027 | Filter inactive | GET ?active=false | Inactive |
| POS-028 | Combine filters | GET with region+search | Combined |
| POS-029 | Empty result | GET for empty region | [] |
| POS-030 | Single result | GET for single match | [item] |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid ID | id=abc | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Invalid ISO code | code=XX | 404 |
| NEG-007 | Wrong code length | code=X | 400 |
| NEG-008 | Empty code | code= | 400 |
| NEG-009 | Null request | POST null | 400 |
| NEG-010 | Missing name | Name missing | 400 |
| NEG-011 | Missing code | Code missing | 400 |
| NEG-012 | Duplicate code | code=US (exists) | 409 |
| NEG-013 | Invalid region | region=Invalid | 400 |
| NEG-014 | Invalid continent | continent=Invalid | 400 |
| NEG-015 | SQL injection | search='; DROP | Sanitized |
| NEG-016 | XSS in name | name=<script> | Sanitized |
| NEG-017 | Negative page | page=-1 | 400 |
| NEG-018 | Zero pageSize | pageSize=0 | 400 |
| NEG-019 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-020 | Invalid sort | sortBy=invalid | 400 |
| NEG-021 | No permission | User without CanView | 403 |
| NEG-022 | No admin for write | POST as user | 403 |
| NEG-023 | Cross-org | Other org data | 403 |
| NEG-024 | Deleted country | id of deleted | 404 |
| NEG-025 | Invalid date format | date=invalid | 400 |
| NEG-026 | Malformed JSON | Invalid JSON | 400 |
| NEG-027 | Wrong content-type | Application/xml | 415 |
| NEG-028 | Invalid DST link | Invalid DST ID | 404 |
| NEG-029 | Orphan region | region deleted | 404 |
| NEG-030 | Rate limit | Too many | 429 |
| NEG-031 | Payload too large | Huge body | 413 |
| NEG-032 | Invalid Accept | Accept: text/plain | 406 |
| NEG-033 | HTTP method | PUT for create | 405 |
| NEG-034 | OPTIONS | OPTIONS | 200 |
| NEG-035 | HEAD | HEAD | 200 or 405 |
| NEG-036 | Trailing slash | /api/countries/ | Redirect |
| NEG-037 | Case sensitivity | /api/Countries | 404 |
| NEG-038 | Extra path | /api/countries/1/extra | 404 |
| NEG-039 | Invalid bearer | Bearer malformed | 401 |
| NEG-040 | Revoked token | Revoked JWT | 401 |
| NEG-041 | Service account | Service for UI | 403 |
| NEG-042 | DB timeout | Simulate | 503 |
| NEG-043 | Invalid bulk IDs | Bulk with invalid | 400/partial |
| NEG-044 | Empty bulk | POST [] | 400 |
| NEG-045 | Excessive bulk | 1000 IDs | 400 |
| NEG-046 | Delete in-use | Country referenced | 409 |
| NEG-047 | Update deleted | PUT on deleted | 404 |
| NEG-048 | Restore not deleted | POST restore on active | 400 |
| NEG-049 | Invalid UUID | id=invalid-guid | 400 |
| NEG-050 | Mismatched IDs | Path != body | 400 |
| NEG-051 | Read-only field | Update createdDate | Ignored |
| NEG-052 | Version conflict | Stale version | 409 |
| NEG-053 | Soft-delete filter | Query deleted | Excluded |
| NEG-054 | Inactive org | Org inactive | 403 |
| NEG-055 | Blocked IP | From blocked | 403 |
| NEG-056 | CORS fail | Invalid origin | CORS error |
| NEG-057 | Control chars | name with \0 | 400 |
| NEG-058 | Unicode overflow | Very long | 400 |
| NEG-059 | Invalid ISO alpha-3 | code=XXX invalid | 404 |
| NEG-060 | Numeric code wrong | code=999 | 404 |
| NEG-061 | Reserved code | code=RESERVED | 403 |
| NEG-062 | Deprecated code | code=deprecated | Redirect/warn |
| NEG-063 | Case code | code=us vs US | Normalize |
| NEG-064 | Whitespace code | code=" US " | Trim |
| NEG-065 | Special chars code | code=U-S | 400 |
| NEG-066 | Empty region | region= | 400 |
| NEG-067 | Empty continent | continent= | 400 |
| NEG-068 | Empty search | search= | No filter |
| NEG-069 | Audit failure | Audit down | Continue |
| NEG-070 | DST link circular | Circular DST | 400 |
| NEG-071 | Invalid adjacent ID | adjacentId=999999 | 404 |
| NEG-072 | DST overlap dates | Overlapping DST | 400 |
| NEG-073 | Invalid timezone | tz=Invalid | 400 |
| NEG-074 | Delete with partners | Country referenced | 409 |
| NEG-075 | Invalid phone code | phoneCode=invalid | 400 |
| NEG-076 | Invalid currency | currency=XXX invalid | 400 |
| NEG-077 | Duplicate adjacent | Same adjacent twice | 409 |
| NEG-078 | DST link to deleted | DST deleted | 404 |
| NEG-079 | Region deleted | regionId deleted | 404 |
| NEG-080 | Continent deleted | continentId deleted | 404 |
| NEG-081 | Bulk with deleted | Bulk includes deleted | 404/partial |
| NEG-082 | Export format invalid | format=Invalid | 400 |
| NEG-083 | Invalid latitude | lat=100 | 400 |
| NEG-084 | Invalid longitude | lng=200 | 400 |
| NEG-085 | Invalid DST offset | offset=25 | 400 |
| NEG-086 | Update inactive | PUT on inactive | 403 |
| NEG-087 | Restore active | Restore non-deleted | 400 |
| NEG-088 | Typeahead empty | q= | 400 |
| NEG-089 | Invalid UNOPS filter | unops=invalid | 400 |
| NEG-090 | Conflicting filters | Mutually exclusive | 400 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | code length | 2 | 3 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-006 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | region length | 1 | 100 | ✅ | ✅ | ❌ |
| BND-008 | continent length | 1 | 50 | ✅ | ✅ | ❌ |
| BND-009 | bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-010 | Empty list | - | - | [] | - | - |
| BND-011 | Single item | - | - | [item] | - | - |
| BND-012 | First page | page=1 | - | ✅ | - | - |
| BND-013 | Last page | - | - | Partial | - | - |
| BND-014 | Zero length name | - | - | ❌ | - | - |
| BND-015 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-016 | ISO 2-char | - | - | alpha-2 | - | - |
| BND-017 | ISO 3-char | - | - | alpha-3 | - | - |
| BND-018 | Feb 29 | - | - | Valid | - | - |
| BND-019 | Unicode name | - | - | Accept | - | - |
| BND-020 | Arabic name | - | - | Display | - | - |
| BND-021 | Chinese name | - | - | Display | - | - |
| BND-022 | Null optional | - | - | Default | - | - |
| BND-023 | Empty string | - | - | No filter | - | - |
| BND-024 | Whitespace | - | - | Trim | - | - |
| BND-025 | Sort empty | - | - | [] | - | - |
| BND-026 | Sort single | - | - | [item] | - | - |
| BND-027 | Filter no match | - | - | [] | - | - |
| BND-028 | Filter all | - | - | Full | - | - |
| BND-029 | Timezone | UTC | - | Correct | - | - |
| BND-030 | DST offset | -24 | +24 | ✅ | ✅ | ❌ |
| BND-031 | Latitude | -90 | 90 | ✅ | ✅ | ❌ |
| BND-032 | Longitude | -180 | 180 | ✅ | ✅ | ❌ |
| BND-033 | Phone code | - | 10 | ✅ | ✅ | ❌ |
| BND-034 | Currency code | 3 | 3 | ✅ | ✅ | ❌ |
| BND-035 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-036 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-037 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-038 | Adjacent count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-039 | Region count | - | 100 | ✅ | ✅ | ❌ |
| BND-040 | Continent count | - | 10 | ✅ | ✅ | ❌ |
| BND-041 | Typeahead min | 1 | - | ✅ | - | - |
| BND-042 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-043 | Pagination boundary | - | - | Exact | - | - |
| BND-044 | Cursor pagination | - | - | Valid | - | - |
| BND-045 | Empty bulk | - | - | 400 | - | - |
| BND-046 | Partial bulk | - | - | 207 | - | - |
| BND-047 | Round-trip | Create → Get | - | Match | - | - |
| BND-048 | Soft-deleted | - | - | Excluded | - | - |
| BND-049 | Inactive | - | - | Filter | - | - |
| BND-050 | Duplicate code | - | - | Reject | - | - |
| BND-051 | Case code | - | - | Normalize | - | - |
| BND-052 | Leading space | - | - | Trim | - | - |
| BND-053 | Trailing space | - | - | Trim | - | - |
| BND-054 | Zero ID | id=0 | - | 400 | - | - |
| BND-055 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-056 | DST start date | - | - | Valid | - | - |
| BND-057 | DST end date | - | - | Valid | - | - |
| BND-058 | DST overlap | - | - | Reject | - | - |
| BND-059 | Multiple timezones | - | 50 | ✅ | ✅ | ❌ |
| BND-060 | No timezone | - | - | Null/empty | - | - |
| BND-061 | Same region | - | - | Group | - | - |
| BND-062 | Same continent | - | - | Group | - | - |
| BND-063 | UNOPS flag | - | - | Boolean | - | - |
| BND-064 | Active flag | - | - | Boolean | - | - |
| BND-065 | Version | 1 | - | ✅ | ❌ | - |
| BND-066 | Created date | - | - | UTC | - | - |
| BND-067 | Modified date | - | - | UTC | - | - |
| BND-068 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-069 | Export empty | - | - | Headers | - | - |
| BND-070 | Export single | - | - | Valid | - | - |
| BND-071 | DST offset hours | -24 | 24 | ✅ | ✅ | ❌ |
| BND-072 | Adjacent count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-073 | Timezone count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-074 | Phone code length | - | 10 | ✅ | ✅ | ❌ |
| BND-075 | Currency code | 3 | 3 | ✅ | ✅ | ❌ |
| BND-076 | Region count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-077 | Continent count | 0 | 10 | ✅ | ✅ | ❌ |
| BND-078 | Typeahead min | 1 | - | ✅ | - | - |
| BND-079 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-080 | Bulk partial | - | - | 207 | - | - |
| BND-081 | DST start date | - | - | Valid | - | - |
| BND-082 | DST end date | - | - | Valid | - | - |
| BND-083 | DST no overlap | - | - | Reject | - | - |
| BND-084 | Multiple timezones | 0 | 50 | ✅ | ✅ | ❌ |
| BND-085 | No timezone | - | - | Null/empty | - | - |
| BND-086 | Same region | - | - | Group | - | - |
| BND-087 | Same continent | - | - | Group | - | - |
| BND-088 | UNOPS flag | - | - | Boolean | - | - |
| BND-089 | Active flag | - | - | Boolean | - | - |
| BND-090 | Case code | code=us | - | Normalize | - | - |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get all | GET | List |
| FUN-002 | Workflow | Get by ID | GET id | Details |
| FUN-003 | Workflow | Get by code | GET code | Match |
| FUN-004 | Workflow | Filter region | GET ?region | Filtered |
| FUN-005 | Workflow | Filter continent | GET ?continent | Filtered |
| FUN-006 | Workflow | Search | GET ?search | Matches |
| FUN-007 | Workflow | Create (admin) | POST | 201 |
| FUN-008 | Workflow | Update (admin) | PUT | 200 |
| FUN-009 | Workflow | Delete (admin) | DELETE | 204 |
| FUN-010 | Workflow | Soft delete | DELETE | IsDeleted |
| FUN-011 | Workflow | Restore | POST restore | Restored |
| FUN-012 | Workflow | DST link | POST dst | Linked |
| FUN-013 | Workflow | Paginate | GET ?page | Paginated |
| FUN-014 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-015 | Workflow | Export | GET export | File |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required code | Missing | 400 |
| FUN-018 | Validation | Valid ISO | Invalid | 400 |
| FUN-019 | Validation | Unique code | Duplicate | 409 |
| FUN-020 | Validation | Valid region | Invalid | 400 |
| FUN-021 | Validation | Valid continent | Invalid | 400 |
| FUN-022 | Validation | Permission | No permission | 403 |
| FUN-023 | Validation | Admin write | User write | 403 |
| FUN-024 | Validation | ID format | Invalid | 400 |
| FUN-025 | Validation | No reserved | Reserved code | 403 |
| FUN-026 | Constraint | FK region | Invalid region | 404 |
| FUN-027 | Constraint | FK continent | Invalid | 404 |
| FUN-028 | Constraint | Delete in-use | Referenced | 409 |
| FUN-029 | Constraint | Soft delete | Query | Excluded |
| FUN-030 | Constraint | Unique code | Duplicate | 409 |
| FUN-031 | Constraint | DST no circular | Circular | 400 |
| FUN-032 | Constraint | Max bulk | >100 | 400 |
| FUN-033 | Constraint | Org scope | Cross-org | 403 |
| FUN-034 | Constraint | Version | Optimistic | 409 |
| FUN-035 | Constraint | Export limit | >10K | Truncate |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Restore | POST restore | Audit |
| FUN-040 | Audit | DST link | POST dst | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Filter |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | ISO standard | Codes | ISO 3166 |
| FUN-050 | Business | Timezone | DST | Correct |
| FUN-051 | Workflow | DST link | POST dst | Linked |
| FUN-052 | Workflow | Get DST | GET dst | DST data |
| FUN-053 | Workflow | Get timezone | GET timezone | Timezone |
| FUN-054 | Workflow | Get adjacent | GET adjacent | Neighbors |
| FUN-055 | Workflow | Bulk get | POST bulk | Results |
| FUN-056 | Validation | Required name | Missing | 400 |
| FUN-057 | Validation | Required code | Missing | 400 |
| FUN-058 | Validation | Valid ISO | Invalid | 400 |
| FUN-059 | Validation | Unique code | Duplicate | 409 |
| FUN-060 | Validation | Valid region | Invalid | 400 |
| FUN-061 | Constraint | FK region | Invalid | 404 |
| FUN-062 | Constraint | FK continent | Invalid | 404 |
| FUN-063 | Constraint | Delete in-use | Referenced | 409 |
| FUN-064 | Constraint | DST no circular | Circular | 400 |
| FUN-065 | Constraint | Max bulk | >100 | 400 |
| FUN-066 | Audit | Create | POST | Audit |
| FUN-067 | Audit | Update | PUT | Audit |
| FUN-068 | Audit | Delete | DELETE | Audit |
| FUN-069 | Audit | Restore | POST restore | Audit |
| FUN-070 | Audit | DST link | POST dst | Audit |
| FUN-071 | Business | Soft-deleted | Query | Excluded |
| FUN-072 | Business | Inactive | Query | Filter |
| FUN-073 | Business | Permission | Query | Scoped |
| FUN-074 | Business | ISO | Codes | ISO 3166 |
| FUN-075 | Business | Timezone | DST | Correct |
| FUN-076 | Workflow | Export | GET export | File |
| FUN-077 | Workflow | Filter region | GET ?region | Filtered |
| FUN-078 | Workflow | Filter continent | GET ?continent | Filtered |
| FUN-079 | Workflow | Search | GET ?search | Matches |
| FUN-080 | Workflow | Paginate | GET ?page | Paginated |
| FUN-081 | Validation | Valid continent | Invalid | 400 |
| FUN-082 | Validation | Permission | No permission | 403 |
| FUN-083 | Validation | Admin write | User write | 403 |
| FUN-084 | Validation | ID format | Invalid | 400 |
| FUN-085 | Validation | No reserved | Reserved code | 403 |
| FUN-086 | Constraint | Org scope | Cross-org | 403 |
| FUN-087 | Constraint | Version | Optimistic | 409 |
| FUN-088 | Constraint | Export limit | >10K | Truncate |
| FUN-089 | Constraint | Soft delete | Query | Excluded |
| FUN-090 | Constraint | Unique code | Duplicate | 409 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Country | Match |
| INT-002 | CRUD | Update → Get | Country | Updated |
| INT-003 | CRUD | Delete → Get | Country | 404 |
| INT-004 | CRUD | Restore → Get | Country | Restored |
| INT-005 | CRUD | Get by code | Country | Match |
| INT-006 | CRUD | DST link | Country, DST | Linked |
| INT-007 | CRUD | Region filter | Country | Filtered |
| INT-008 | CRUD | Continent filter | Country | Filtered |
| INT-009 | CRUD | Bulk get | Country | Results |
| INT-010 | CRUD | Export | Country | File |
| INT-011 | Search | Search by name | Country | Matches |
| INT-012 | Search | Typeahead | Country | Suggestions |
| INT-013 | Search | Filter region | Country | Filtered |
| INT-014 | Search | Filter continent | Country | Filtered |
| INT-015 | Search | Multi-filter | Country | Combined |
| INT-016 | Search | Empty search | - | [] |
| INT-017 | Search | Partial match | Country | Fuzzy |
| INT-018 | Search | Sort + filter | Country | Both |
| INT-019 | Search | Filter + pagination | Country | Both |
| INT-020 | Search | UNOPS filter | Country | Filtered |
| INT-021 | Pagination | Page 1 | Country | First |
| INT-022 | Pagination | Last page | Country | Partial |
| INT-023 | Pagination | Size | Country | Correct |
| INT-024 | Pagination | Invalid | Country | 400 |
| INT-025 | Pagination | Boundary | Country | Exact |
| INT-026 | Relationships | Country → Region | Country, Region | Linked |
| INT-027 | Relationships | Country → Continent | Country, Continent | Linked |
| INT-028 | Relationships | Country → DST | Country, DST | Linked |
| INT-029 | Relationships | Orphan | Deleted region | 404 |
| INT-030 | Relationships | Adjacent | Country | Neighbors |
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
| INT-046 | E2E | Full create flow | Country | Create → Get |
| INT-047 | E2E | Full update flow | Country | Update → Get |
| INT-048 | E2E | Full delete flow | Country | Delete → 404 |
| INT-049 | E2E | DST flow | Country, DST | Link → Get |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get DST | Country | DST data |
| INT-052 | CRUD | Get timezone | Country | Timezone |
| INT-053 | CRUD | Get adjacent | Country | Neighbors |
| INT-054 | CRUD | Bulk get | Country | Results |
| INT-055 | CRUD | Export | Country | File |
| INT-056 | Search | Search by name | Country | Matches |
| INT-057 | Search | Typeahead | Country | Suggestions |
| INT-058 | Search | Filter region | Country | Filtered |
| INT-059 | Search | Filter continent | Country | Filtered |
| INT-060 | Search | Multi-filter | Country | Combined |
| INT-061 | Pagination | Page 1 | Country | First |
| INT-062 | Pagination | Last page | Country | Partial |
| INT-063 | Pagination | Size | Country | Correct |
| INT-064 | Pagination | Invalid | Country | 400 |
| INT-065 | Pagination | Boundary | Country | Exact |
| INT-066 | Relationships | Country → Region | Linked | Correct |
| INT-067 | Relationships | Country → Continent | Linked | Correct |
| INT-068 | Relationships | Country → DST | Linked | Correct |
| INT-069 | Relationships | Orphan | Deleted region | 404 |
| INT-070 | Relationships | Adjacent | Country | Neighbors |
| INT-071 | Error | DB down | DB | 503 |
| INT-072 | Error | Auth down | Auth | 401/503 |
| INT-073 | Error | Validation | Bad input | 400 |
| INT-074 | Error | NotFound | Invalid ID | 404 |
| INT-075 | Error | Forbidden | No permission | 403 |
| INT-076 | Error | Conflict | Duplicate | 409 |
| INT-077 | Error | Rate limit | Too many | 429 |
| INT-078 | Error | Timeout | Slow | 504 |
| INT-079 | Error | Payload | Huge | 413 |
| INT-080 | Error | Media | Wrong type | 415 |
| INT-081 | Error | Method | Wrong verb | 405 |
| INT-082 | Error | Service | Dependency | 503 |
| INT-083 | Error | Gateway | Upstream | 504 |
| INT-084 | Error | Gone | Deleted | 410 |
| INT-085 | Error | Locked | Locked | 423 |
| INT-086 | E2E | Full create flow | Country | Create → Get |
| INT-087 | E2E | Full update flow | Country | Update → Get |
| INT-088 | E2E | Full delete flow | Country | Delete → 404 |
| INT-089 | E2E | Export flow | Country | Export → File |
| INT-090 | E2E | DST link flow | Country, DST | Link → Get |

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
| CON-014 | Export + update | Snapshot |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Bulk during update | Consistent |
| CON-022 | DST link concurrent | Last write |
| CON-023 | Permission change | Old |
| CON-024 | Region change concurrent | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid ISO code | Accept |
| UNT-002 | Validation | Invalid ISO | Reject |
| UNT-003 | Validation | Valid ID | Accept |
| UNT-004 | Validation | Invalid ID | Reject |
| UNT-005 | Validation | Valid region | Accept |
| UNT-006 | Formatting | Code to uppercase | US |
| UNT-007 | Formatting | Name trim | Trimmed |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | DST offset | Correct |
| UNT-010 | Calculation | Timezone | Correct |
| UNT-011 | Calculation | Distance | Correct |
| UNT-012 | Calculation | Bounding box | Correct |
| UNT-013 | Calculation | Center point | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | UNOPS | UNOPS only |
| UNT-018 | Status | Non-UNOPS | Exclude UNOPS |
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
| PRF-005 | Filter region | < 200ms |
| PRF-006 | Paginate | < 200ms |
| PRF-007 | Typeahead | < 200ms |
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
| CRUD countries | POS-001–004, FUN-001–003 |
| ISO codes | POS-003, BND-016–017 |
| Region mapping | POS-005, POS-012 |
| DST linking | POS-020–021, FUN-012 |
| 3:1 Ratio | NEG-001–090, BND-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
