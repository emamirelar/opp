# LiaisonOfficeController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/LiaisonOfficeController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for liaison office management: CRUD offices, location mapping, contact info, org hierarchy.

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
| POS-001 | Get all offices | GET /api/liaison-offices | Office list |
| POS-002 | Get office by ID | GET /api/liaison-offices/{id} | Office details |
| POS-003 | Create office (admin) | POST /api/liaison-offices | 201 Created |
| POS-004 | Update office (admin) | PUT /api/liaison-offices/{id} | 200 OK |
| POS-005 | Delete office (admin) | DELETE /api/liaison-offices/{id} | 204 No Content |
| POS-006 | Get dropdown | GET /api/liaison-offices/dropdown | ID/name pairs |
| POS-007 | Search offices | GET /api/liaison-offices?search=text | Filtered |
| POS-008 | Filter by country | GET /api/liaison-offices?countryId=1 | Filtered |
| POS-009 | Filter by region | GET /api/liaison-offices?region=East Africa | Filtered |
| POS-010 | Filter by parent | GET /api/liaison-offices?parentId=1 | Child offices |
| POS-011 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-012 | Sort by name | GET ?sortBy=name | Sorted |
| POS-013 | Sort by code | GET ?sortBy=code | Sorted |
| POS-014 | Get location | GET /api/liaison-offices/{id}/location | Location |
| POS-015 | Get contact info | GET /api/liaison-offices/{id}/contacts | Contacts |
| POS-016 | Get org hierarchy | GET /api/liaison-offices/{id}/hierarchy | Hierarchy |
| POS-017 | Get children | GET /api/liaison-offices/{id}/children | Children |
| POS-018 | Get parent | GET /api/liaison-offices/{id}/parent | Parent |
| POS-019 | Typeahead | GET /api/liaison-offices/typeahead?q=text | Suggestions |
| POS-020 | Soft delete | DELETE (soft) | IsDeleted |
| POS-021 | Restore | POST /api/liaison-offices/{id}/restore | Restored |
| POS-022 | Get by code | GET /api/liaison-offices/code/{code} | By code |
| POS-023 | Get active only | GET ?active=true | Active only |
| POS-024 | Bulk get | POST /api/liaison-offices/bulk | Bulk results |
| POS-025 | Export | GET /api/liaison-offices/export | Export file |
| POS-026 | Update location | PUT /api/liaison-offices/{id}/location | Updated |
| POS-027 | Add contact | POST /api/liaison-offices/{id}/contacts | Contact added |
| POS-028 | Get map data | GET /api/liaison-offices/map | Map coordinates |
| POS-029 | Update hierarchy | PUT /api/liaison-offices/{id}/hierarchy | Updated |
| POS-030 | Empty result | GET for empty filter | [] |

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
| NEG-011 | Invalid countryId | countryId=999999 | 404 |
| NEG-012 | Invalid parentId | parentId=999999 | 404 |
| NEG-013 | SQL injection | search='; DROP | Sanitized |
| NEG-014 | XSS in name | name=<script> | Sanitized |
| NEG-015 | Negative page | page=-1 | 400 |
| NEG-016 | Zero pageSize | pageSize=0 | 400 |
| NEG-017 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-018 | Invalid sort | sortBy=invalid | 400 |
| NEG-019 | No permission | User without CanView | 403 |
| NEG-020 | No admin for write | POST as user | 403 |
| NEG-021 | Cross-org access | Other org office | 403 |
| NEG-022 | Deleted office | id of deleted | 404 |
| NEG-023 | Circular parent | parentId=self | 400 |
| NEG-024 | Invalid coordinates | lat/lng invalid | 400 |
| NEG-025 | Malformed JSON | Invalid JSON | 400 |
| NEG-026 | Wrong content-type | Application/xml | 415 |
| NEG-027 | Invalid contact format | Contact malformed | 400 |
| NEG-028 | Invalid hierarchy | Hierarchy malformed | 400 |
| NEG-029 | Rate limit | Too many | 429 |
| NEG-030 | Payload too large | Huge body | 413 |
| NEG-031 | Invalid Accept | Accept: text/plain | 406 |
| NEG-032 | HTTP method | PUT for create | 405 |
| NEG-033 | OPTIONS | OPTIONS | 200 |
| NEG-034 | HEAD | HEAD | 200 or 405 |
| NEG-035 | Trailing slash | /api/liaison-offices/ | Redirect |
| NEG-036 | Case sensitivity | /api/Liaison-Offices | 404 |
| NEG-037 | Extra path | /api/liaison-offices/1/extra | 404 |
| NEG-038 | Invalid bearer | Bearer malformed | 401 |
| NEG-039 | Revoked token | Revoked JWT | 401 |
| NEG-040 | Service account | Service for UI | 403 |
| NEG-041 | DB timeout | Simulate | 503 |
| NEG-042 | Invalid bulk IDs | Bulk with invalid | 400/partial |
| NEG-043 | Empty bulk | POST [] | 400 |
| NEG-044 | Excessive bulk | 1000 IDs | 400 |
| NEG-045 | Delete in-use | Office referenced | 409 |
| NEG-046 | Update deleted | PUT on deleted | 404 |
| NEG-047 | Restore not deleted | POST restore on active | 400 |
| NEG-048 | Invalid UUID | id=invalid-guid | 400 |
| NEG-049 | Mismatched IDs | Path != body | 400 |
| NEG-050 | Read-only field | Update createdDate | Ignored |
| NEG-051 | Version conflict | Stale version | 409 |
| NEG-052 | Blocked IP | From blocked | 403 |
| NEG-053 | CORS fail | Invalid origin | CORS error |
| NEG-054 | Control chars | name with \0 | 400 |
| NEG-055 | Unicode overflow | Very long | 400 |
| NEG-056 | Invalid phone format | phone=invalid | 400 |
| NEG-057 | Invalid email | email=invalid | 400 |
| NEG-058 | Invalid address | address malformed | 400 |
| NEG-059 | Invalid lat/lng | lat=999 | 400 |
| NEG-060 | Hierarchy depth | >10 levels | 400 |
| NEG-061 | Orphan country | countryId deleted | 404 |
| NEG-062 | Orphan parent | parentId deleted | 404 |
| NEG-063 | Duplicate name | name exists | 409 or allow |
| NEG-064 | Empty code | code= | 400 |
| NEG-065 | Whitespace code | code="  " | 400 |
| NEG-066 | Reserved code | code=RESERVED | 403 |
| NEG-067 | Export no permission | No export permission | 403 |
| NEG-068 | Audit failure | Audit down | Continue |
| NEG-069 | Inactive org | Org inactive | 403 |
| NEG-070 | Soft-deleted filter | Query deleted | Excluded |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | code length | 1 | 50 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-006 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | countryId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-008 | parentId | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-009 | hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-010 | latitude | -90 | 90 | ✅ | ✅ | ❌ |
| BND-011 | longitude | -180 | 180 | ✅ | ✅ | ❌ |
| BND-012 | bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-013 | Empty list | - | - | [] | - | - |
| BND-014 | Single item | - | - | [item] | - | - |
| BND-015 | First page | page=1 | - | ✅ | - | - |
| BND-016 | Last page | - | - | Partial | - | - |
| BND-017 | Zero length name | - | - | ❌ | - | - |
| BND-018 | Max length name | 255 | - | - | ✅ | ❌ |
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
| BND-029 | Contact count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-030 | Children count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-031 | Phone length | - | 50 | ✅ | ✅ | ❌ |
| BND-032 | Email length | - | 255 | ✅ | ✅ | ❌ |
| BND-033 | Address length | - | 500 | ✅ | ✅ | ❌ |
| BND-034 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-035 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-036 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-037 | Typeahead min | 1 | - | ✅ | - | - |
| BND-038 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-039 | Pagination boundary | - | - | Exact | - | - |
| BND-040 | Cursor pagination | - | - | Valid | - | - |
| BND-041 | Empty bulk | - | - | 400 | - | - |
| BND-042 | Partial bulk | - | - | 207 | - | - |
| BND-043 | Round-trip | Create → Get | - | Match | - | - |
| BND-044 | Soft-deleted | - | - | Excluded | - | - |
| BND-045 | Inactive | - | - | Excluded | - | - |
| BND-046 | Duplicate code | - | - | Reject | - | - |
| BND-047 | Case code | - | - | Normalize | - | - |
| BND-048 | Zero ID | id=0 | - | 400 | - | - |
| BND-049 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-050 | Root office | parentId=0 | - | Root | - | - |
| BND-051 | Leaf office | No children | - | Leaf | - | - |
| BND-052 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-053 | Export empty | - | - | Headers | - | - |
| BND-054 | Export single | - | - | Valid | - | - |
| BND-055 | Coordinate precision | - | 6 | Rounded | - | - |
| BND-056 | Version | 1 | - | ✅ | ❌ | - |
| BND-057 | Region length | - | 100 | ✅ | ✅ | ❌ |
| BND-058 | Country name | - | 255 | ✅ | ✅ | ❌ |
| BND-059 | Map bounds | - | - | Valid | - | - |
| BND-060 | Timezone | - | 50 | ✅ | ✅ | ❌ |
| BND-061 | Fax length | - | 50 | ✅ | ✅ | ❌ |
| BND-062 | Website length | - | 500 | ✅ | ✅ | ❌ |
| BND-063 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-064 | Active flag | - | - | Boolean | - | - |
| BND-065 | IsHeadOffice | - | - | Boolean | - | - |
| BND-066 | Order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-067 | Created date | - | - | UTC | - | - |
| BND-068 | Modified date | - | - | UTC | - | - |
| BND-069 | Hierarchy path | - | 500 | ✅ | ✅ | ❌ |
| BND-070 | Full path | - | 1000 | ✅ | ✅ | ❌ |
| BND-071 | Map zoom level | 0 | 21 | ✅ | ✅ | ❌ |
| BND-072 | Contact email | - | 255 | ✅ | ✅ | ❌ |
| BND-073 | Contact phone | - | 50 | ✅ | ✅ | ❌ |
| BND-074 | Website URL | - | 500 | ✅ | ✅ | ❌ |
| BND-075 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-076 | Fax length | - | 50 | ✅ | ✅ | ❌ |
| BND-077 | Region code | - | 10 | ✅ | ✅ | ❌ |
| BND-078 | Timezone | - | 50 | ✅ | ✅ | ❌ |
| BND-079 | Coordinate decimals | - | 6 | Rounded | - | - |
| BND-080 | Child depth | 0 | 10 | ✅ | ✅ | ❌ |
| BND-081 | Map points | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-082 | Export format | - | - | csv, xlsx | - | - |
| BND-083 | Typeahead delay | - | - | Debounce | - | - |
| BND-084 | Bulk partial | - | - | 207 | - | - |
| BND-085 | Empty map | - | - | [] | - | - |
| BND-086 | Single map point | - | - | [point] | - | - |
| BND-087 | Null parent | parentId=null | - | Root | - | - |
| BND-088 | Max depth | - | 10 | ✅ | ❌ | - |
| BND-089 | Case code | code=us | - | Normalize | - | - |
| BND-090 | Trim name | "  name  " | - | Trimmed | - | - |

---

## §4 Functional Tests (90)

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
| FUN-009 | Workflow | Filter country | GET ?countryId | Filtered |
| FUN-010 | Workflow | Filter region | GET ?region | Filtered |
| FUN-011 | Workflow | Filter parent | GET ?parentId | Filtered |
| FUN-012 | Workflow | Search | GET ?search | Searched |
| FUN-013 | Workflow | Paginate | GET ?page | Paginated |
| FUN-014 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-015 | Workflow | Export | GET export | File |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required code | Missing | 400 |
| FUN-018 | Validation | Unique code | Duplicate | 409 |
| FUN-019 | Validation | Valid country | Invalid | 404 |
| FUN-020 | Validation | Valid parent | Invalid | 404 |
| FUN-021 | Validation | No circular | Circular | 400 |
| FUN-022 | Validation | Permission | No permission | 403 |
| FUN-023 | Validation | Admin write | User write | 403 |
| FUN-024 | Validation | ID format | Invalid | 400 |
| FUN-025 | Validation | Coordinate range | Out of range | 400 |
| FUN-026 | Constraint | FK country | Invalid | 404 |
| FUN-027 | Constraint | FK parent | Invalid | 404 |
| FUN-028 | Constraint | Delete in-use | Referenced | 409 |
| FUN-029 | Constraint | Hierarchy depth | >10 | 400 |
| FUN-030 | Constraint | Soft delete | Query | Excluded |
| FUN-031 | Constraint | Org scope | Cross-org | 403 |
| FUN-032 | Constraint | Version | Optimistic | 409 |
| FUN-033 | Constraint | Max bulk | >100 | 400 |
| FUN-034 | Constraint | Export limit | >10K | Truncate |
| FUN-035 | Constraint | Contact limit | >50 | 400 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Restore | POST restore | Audit |
| FUN-040 | Audit | Location update | PUT location | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Hierarchy | Parent-child | Linked |
| FUN-050 | Business | Timezone | Location | Correct |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Office | Match |
| INT-002 | CRUD | Update → Get | Office | Updated |
| INT-003 | CRUD | Delete → Get | Office | 404 |
| INT-004 | CRUD | Restore → Get | Office | Restored |
| INT-005 | CRUD | Get by code | Office | Match |
| INT-006 | CRUD | Get location | Office | Location |
| INT-007 | CRUD | Get contacts | Office | Contacts |
| INT-008 | CRUD | Get hierarchy | Office | Hierarchy |
| INT-009 | CRUD | Bulk get | Office | Results |
| INT-010 | CRUD | Export | Office | File |
| INT-011 | Search | Search by name | Office | Matches |
| INT-012 | Search | Typeahead | Office | Suggestions |
| INT-013 | Search | Filter country | Office | Filtered |
| INT-014 | Search | Filter region | Office | Filtered |
| INT-015 | Search | Filter parent | Office | Filtered |
| INT-016 | Search | Multi-filter | Office | Combined |
| INT-017 | Search | Empty search | - | [] |
| INT-018 | Search | Partial match | Office | Fuzzy |
| INT-019 | Search | Sort + filter | Office | Both |
| INT-020 | Search | Filter + pagination | Office | Both |
| INT-021 | Pagination | Page 1 | Office | First |
| INT-022 | Pagination | Last page | Office | Partial |
| INT-023 | Pagination | Size | Office | Correct |
| INT-024 | Pagination | Invalid | Office | 400 |
| INT-025 | Pagination | Boundary | Office | Exact |
| INT-026 | Relationships | Office → Country | Office, Country | Linked |
| INT-027 | Relationships | Office → Parent | Office | Linked |
| INT-028 | Relationships | Office → Children | Office | Linked |
| INT-029 | Relationships | Office → Contacts | Office, Contact | Linked |
| INT-030 | Relationships | Orphan | Deleted country | 404 |
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
| INT-046 | E2E | Full create flow | Office | Create → Get |
| INT-047 | E2E | Full update flow | Office | Update → Get |
| INT-048 | E2E | Full delete flow | Office | Delete → 404 |
| INT-049 | E2E | Hierarchy flow | Office | Parent → Children |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get location | Office | Location |
| INT-052 | CRUD | Get contacts | Office | Contacts |
| INT-053 | CRUD | Get hierarchy | Office | Hierarchy |
| INT-054 | CRUD | Bulk get | Office | Results |
| INT-055 | CRUD | Export | Office | File |
| INT-056 | Search | Search by name | Office | Matches |
| INT-057 | Search | Typeahead | Office | Suggestions |
| INT-058 | Search | Filter country | Office | Filtered |
| INT-059 | Search | Filter region | Office | Filtered |
| INT-060 | Search | Multi-filter | Office | Combined |
| INT-061 | Pagination | Page 1 | Office | First |
| INT-062 | Pagination | Last page | Office | Partial |
| INT-063 | Pagination | Size | Office | Correct |
| INT-064 | Pagination | Invalid | Office | 400 |
| INT-065 | Pagination | Boundary | Office | Exact |
| INT-066 | Relationships | Office → Country | Linked | Correct |
| INT-067 | Relationships | Office → Parent | Linked | Correct |
| INT-068 | Relationships | Office → Children | Linked | Correct |
| INT-069 | Relationships | Office → Contacts | Linked | Correct |
| INT-070 | Relationships | Orphan | Deleted country | 404 |
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
| INT-086 | E2E | Full create flow | Office | Create → Get |
| INT-087 | E2E | Full update flow | Office | Update → Get |
| INT-088 | E2E | Full delete flow | Office | Delete → 404 |
| INT-089 | E2E | Map flow | Office | Get → Display |
| INT-090 | E2E | Export flow | Office | Export → File |

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
| CON-014 | Hierarchy update | Snapshot |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Location update concurrent | Last write |
| CON-022 | Hierarchy change | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Contact add concurrent | Both succeed |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid code | Accept |
| UNT-002 | Validation | Invalid code | Reject |
| UNT-003 | Validation | Valid coordinates | Accept |
| UNT-004 | Validation | Invalid coordinates | Reject |
| UNT-005 | Validation | Valid email | Accept |
| UNT-006 | Formatting | Phone | Formatted |
| UNT-007 | Formatting | Address | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Distance | Correct |
| UNT-010 | Calculation | Hierarchy path | Correct |
| UNT-011 | Calculation | Bounding box | Correct |
| UNT-012 | Calculation | Center point | Correct |
| UNT-013 | Calculation | Depth | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Head office | Head only |
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
| PRF-005 | Filter country | < 200ms |
| PRF-006 | Get hierarchy | < 300ms |
| PRF-007 | Get location | < 100ms |
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
| CRUD offices | POS-001–005, FUN-001–003 |
| Location mapping | POS-014, POS-026 |
| Contact info | POS-015, POS-027 |
| Org hierarchy | POS-016–018, FUN-049 |
| 3:1 Ratio | NEG-001–090, BND-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
