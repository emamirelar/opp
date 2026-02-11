# PartnerGroupController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/PartnerGroupController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

REST API for partner groups: CRUD groups, membership management, group-level operations.

---

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get all groups | GET /api/partner-groups | Group list |
| POS-002 | Get group by ID | GET /api/partner-groups/{id} | Group details |
| POS-003 | Create group (admin) | POST /api/partner-groups | 201 Created |
| POS-004 | Update group (admin) | PUT /api/partner-groups/{id} | 200 OK |
| POS-005 | Delete group (admin) | DELETE /api/partner-groups/{id} | 204 No Content |
| POS-006 | Get dropdown | GET /api/partner-groups/dropdown | ID/name pairs |
| POS-007 | Search groups | GET /api/partner-groups?search=text | Filtered |
| POS-008 | Add member | POST /api/partner-groups/{id}/members | Member added |
| POS-009 | Remove member | DELETE /api/partner-groups/{id}/members/{pid} | Member removed |
| POS-010 | Get members | GET /api/partner-groups/{id}/members | Members |
| POS-011 | Bulk add members | POST /api/partner-groups/{id}/members/bulk | Bulk added |
| POS-012 | Bulk remove members | DELETE /api/partner-groups/{id}/members/bulk | Bulk removed |
| POS-013 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-014 | Sort by name | GET ?sortBy=name | Sorted |
| POS-015 | Get active only | GET ?active=true | Active only |
| POS-016 | Empty result | GET for empty filter | [] |
| POS-017 | Single result | GET for single match | [item] |
| POS-018 | Authenticated access | GET with token | 200 |
| POS-019 | Soft delete | DELETE (soft) | IsDeleted |
| POS-020 | Restore | POST /api/partner-groups/{id}/restore | Restored |
| POS-021 | Get by code | GET /api/partner-groups/code/{code} | By code |
| POS-022 | Group-level operation | POST /api/partner-groups/{id}/action | Operation |
| POS-023 | Get member count | GET /api/partner-groups/{id}/member-count | Count |
| POS-024 | Filter by type | GET ?type=Strategic | Filtered |
| POS-025 | Typeahead | GET /api/partner-groups/typeahead?q=text | Suggestions |
| POS-026 | Export | GET /api/partner-groups/export | Export file |
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
| NEG-011 | Invalid partnerId | partnerId=999999 | 404 |
| NEG-012 | SQL injection | search='; DROP | Sanitized |
| NEG-013 | XSS in name | name=<script> | Sanitized |
| NEG-014 | Negative page | page=-1 | 400 |
| NEG-015 | Zero pageSize | pageSize=0 | 400 |
| NEG-016 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-017 | Invalid sort | sortBy=invalid | 400 |
| NEG-018 | No permission | User without CanView | 403 |
| NEG-019 | No admin for write | POST as user | 403 |
| NEG-020 | Cross-org access | Other org group | 403 |
| NEG-021 | Deleted group | id of deleted | 404 |
| NEG-022 | Malformed JSON | Invalid JSON | 400 |
| NEG-023 | Wrong content-type | Application/xml | 415 |
| NEG-024 | Rate limit | Too many | 429 |
| NEG-025 | Payload too large | Huge body | 413 |
| NEG-026 | Invalid Accept | Accept: text/plain | 406 |
| NEG-027 | HTTP method | PUT for create | 405 |
| NEG-028 | Trailing slash | /api/partner-groups/ | Redirect |
| NEG-029 | Case sensitivity | /api/Partner-Groups | 404 |
| NEG-030 | Extra path | /api/partner-groups/1/extra | 404 |
| NEG-031 | Invalid bearer | Bearer malformed | 401 |
| NEG-032 | Revoked token | Revoked JWT | 401 |
| NEG-033 | Service account | Service for UI | 403 |
| NEG-034 | DB timeout | Simulate | 503 |
| NEG-035 | Invalid type | type=invalid | 400 |
| NEG-036 | Empty code | code= | 400 |
| NEG-037 | Whitespace code | code="  " | 400 |
| NEG-038 | Zero ID | id=0 | 400 |
| NEG-039 | Invalid UUID | id=invalid-guid | 400 |
| NEG-040 | Blocked IP | From blocked | 403 |
| NEG-041 | Control chars | name with \0 | 400 |
| NEG-042 | Unicode overflow | Very long | 400 |
| NEG-043 | Delete in-use | Group referenced | 409 |
| NEG-044 | Update deleted | PUT on deleted | 404 |
| NEG-045 | Restore not deleted | POST restore on active | 400 |
| NEG-046 | Mismatched IDs | Path != body | 400 |
| NEG-047 | Read-only field | Update createdDate | Ignored |
| NEG-048 | Version conflict | Stale version | 409 |
| NEG-049 | CORS fail | Invalid origin | CORS error |
| NEG-050 | Inactive org | Org inactive | 403 |
| NEG-051 | Invalid bulk add | Bulk with invalid | 400/partial |
| NEG-052 | Empty bulk | POST [] | 400 |
| NEG-053 | Excessive bulk | 1000 IDs | 400 |
| NEG-054 | Add non-existent | partnerId=999999 | 404 |
| NEG-055 | Duplicate add | Already member | 409 |
| NEG-056 | Remove non-member | Not in group | 404 |
| NEG-057 | Export no permission | No export permission | 403 |
| NEG-058 | Audit failure | Audit down | Continue |
| NEG-059 | Reserved code | code=RESERVED | 403 |
| NEG-060 | Duplicate name | name exists | 409 or allow |
| NEG-061 | Invalid filter combo | Invalid filter combo | 400 |
| NEG-062 | Max URL length | Very long URL | 414 |
| NEG-063 | Invalid endpoint | /api/partner-groups/invalid | 404 |
| NEG-064 | Invalid method | PATCH | 405 |
| NEG-065 | Missing query | GET no params | 200 or 400 |
| NEG-066 | Invalid encoding | Malformed URL | 400 |
| NEG-067 | Group operation no perm | No op permission | 403 |
| NEG-068 | Invalid operation | operation=invalid | 400 |
| NEG-069 | Empty member list | Get members empty | [] |
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
| BND-007 | partnerId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-008 | Empty list | - | - | [] | - | - |
| BND-009 | Single item | - | - | [item] | - | - |
| BND-010 | First page | page=1 | - | ✅ | - | - |
| BND-011 | Last page | - | - | Partial | - | - |
| BND-012 | Zero length name | - | - | ❌ | - | - |
| BND-013 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-014 | Unicode name | - | - | Accept | - | - |
| BND-015 | Arabic name | - | - | Display | - | - |
| BND-016 | Chinese name | - | - | Display | - | - |
| BND-017 | Null optional | - | - | Default | - | - |
| BND-018 | Empty string | - | - | No filter | - | - |
| BND-019 | Whitespace | - | - | Trim | - | - |
| BND-020 | Sort empty | - | - | [] | - | - |
| BND-021 | Sort single | - | - | [item] | - | - |
| BND-022 | Filter no match | - | - | [] | - | - |
| BND-023 | Filter all | - | - | Full | - | - |
| BND-024 | Member count | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-025 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-026 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-027 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-028 | Typeahead min | 1 | - | ✅ | - | - |
| BND-029 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-030 | Pagination boundary | - | - | Exact | - | - |
| BND-031 | Empty bulk | - | - | 400 | - | - |
| BND-032 | Partial bulk | - | - | 207 | - | - |
| BND-033 | Round-trip | Create → Get | - | Match | - | - |
| BND-034 | Soft-deleted | - | - | Excluded | - | - |
| BND-035 | Inactive | - | - | Excluded | - | - |
| BND-036 | Duplicate code | - | - | Reject | - | - |
| BND-037 | Case code | - | - | Normalize | - | - |
| BND-038 | Zero ID | id=0 | - | 400 | - | - |
| BND-039 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-040 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-041 | Export empty | - | - | Headers | - | - |
| BND-042 | Export single | - | - | Valid | - | - |
| BND-043 | Version | 1 | - | ✅ | ❌ | - |
| BND-044 | Type length | - | 50 | ✅ | ✅ | ❌ |
| BND-045 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-046 | Order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-047 | Created date | - | - | UTC | - | - |
| BND-048 | Modified date | - | - | UTC | - | - |
| BND-049 | Hierarchy path | - | 500 | ✅ | ✅ | ❌ |
| BND-050 | Full path | - | 1000 | ✅ | ✅ | ❌ |
| BND-051 | Bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-052 | Active flag | - | - | Boolean | - | - |
| BND-053 | Notes length | - | 2000 | ✅ | ✅ | ❌ |
| BND-054 | Workflow status | - | - | Valid | - | - |
| BND-055 | Audit fields | - | - | Set | - | - |
| BND-056 | Member limit | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-057 | Group type | - | max | Valid | Valid | ❌ |
| BND-058 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-059 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-060 | Single member | - | - | [member] | - | - |
| BND-061 | Max members | - | 1000 | - | ✅ | ❌ |
| BND-062 | Empty members | - | - | [] | - | - |
| BND-063 | Operation params | - | 20 | ✅ | ✅ | ❌ |
| BND-064 | Operation result | - | - | Valid | - | - |
| BND-065 | Group hierarchy | - | 5 | ✅ | ✅ | ❌ |
| BND-066 | Nested groups | - | 3 | ✅ | ✅ | ❌ |
| BND-067 | Parent group | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-068 | Child groups | 0 | 50 | ✅ | ✅ | ❌ |
| BND-069 | Member roles | - | 5 | ✅ | ✅ | ❌ |
| BND-070 | Operation timeout | - | 30s | - | ✅ | ❌ |

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
| FUN-009 | Workflow | Add member | POST members | Added |
| FUN-010 | Workflow | Remove member | DELETE members | Removed |
| FUN-011 | Workflow | Filter type | GET ?type | Filtered |
| FUN-012 | Workflow | Search | GET ?search | Searched |
| FUN-013 | Workflow | Paginate | GET ?page | Paginated |
| FUN-014 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-015 | Workflow | Group operation | POST action | Operation |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required code | Missing | 400 |
| FUN-018 | Validation | Unique code | Duplicate | 409 |
| FUN-019 | Validation | Valid partner | Invalid | 404 |
| FUN-020 | Validation | Permission | No permission | 403 |
| FUN-021 | Validation | Admin write | User write | 403 |
| FUN-022 | Validation | ID format | Invalid | 400 |
| FUN-023 | Validation | Type enum | Invalid | 400 |
| FUN-024 | Validation | Operation valid | Invalid | 400 |
| FUN-025 | Validation | Member exists | Invalid | 404 |
| FUN-026 | Constraint | Delete in-use | Referenced | 409 |
| FUN-027 | Constraint | Soft delete | Query | Excluded |
| FUN-028 | Constraint | Org scope | Cross-org | 403 |
| FUN-029 | Constraint | Version | Optimistic | 409 |
| FUN-030 | Constraint | Max bulk | >100 | 400 |
| FUN-031 | Constraint | Export limit | >10K | Truncate |
| FUN-032 | Constraint | Member limit | >1000 | 400 |
| FUN-033 | Constraint | No duplicate member | Duplicate | 409 |
| FUN-034 | Constraint | Operation permission | No perm | 403 |
| FUN-035 | Constraint | Group hierarchy | Circular | 400 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Restore | POST restore | Audit |
| FUN-040 | Audit | Add member | POST members | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Membership | Member list | Correct |
| FUN-050 | Business | Group operation | Operation | Scoped |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Group | Match |
| INT-002 | CRUD | Update → Get | Group | Updated |
| INT-003 | CRUD | Delete → Get | Group | 404 |
| INT-004 | CRUD | Restore → Get | Group | Restored |
| INT-005 | CRUD | Get by code | Group | Match |
| INT-006 | CRUD | Add member → Get | Group, Partner | Added |
| INT-007 | CRUD | Remove member → Get | Group, Partner | Removed |
| INT-008 | CRUD | Bulk add | Group, Partner | Bulk |
| INT-009 | CRUD | Export | Group | File |
| INT-010 | CRUD | Dropdown | Group | Pairs |
| INT-011 | Search | Search by name | Group | Matches |
| INT-012 | Search | Typeahead | Group | Suggestions |
| INT-013 | Search | Filter type | Group | Filtered |
| INT-014 | Search | Multi-filter | Group | Combined |
| INT-015 | Search | Empty search | - | [] |
| INT-016 | Search | Partial match | Group | Fuzzy |
| INT-017 | Search | Sort + filter | Group | Both |
| INT-018 | Search | Filter + pagination | Group | Both |
| INT-019 | Membership | Add → count | Group | +1 |
| INT-020 | Membership | Remove → count | Group | -1 |
| INT-021 | Pagination | Page 1 | Group | First |
| INT-022 | Pagination | Last page | Group | Partial |
| INT-023 | Pagination | Size | Group | Correct |
| INT-024 | Pagination | Invalid | Group | 400 |
| INT-025 | Pagination | Boundary | Group | Exact |
| INT-026 | Relationships | Group → Partners | Group, Partner | Linked |
| INT-027 | Relationships | Orphan | Deleted partner | 404 |
| INT-028 | Relationships | Partner deleted | Partner | 404 |
| INT-029 | Operation | Group action | Group | Result |
| INT-030 | Operation | Operation audit | Group | Audit |
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
| INT-046 | E2E | Full create flow | Group | Create → Get |
| INT-047 | E2E | Full update flow | Group | Update → Get |
| INT-048 | E2E | Full delete flow | Group | Delete → 404 |
| INT-049 | E2E | Membership flow | Group, Partner | Add → Remove |
| INT-050 | E2E | Session expiry | Auth | Clean fail |

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
| SEC-021 | IDOR | Other org group | ID | 403 |
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
| CON-014 | Add same member | One or conflict |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Remove concurrent | Both succeed |
| CON-022 | Operation concurrent | Queue or last |
| CON-023 | Permission change | Old |
| CON-024 | Member change | Consistent |
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
| UNT-009 | Calculation | Member count | Correct |
| UNT-010 | Calculation | Filter count | Correct |
| UNT-011 | Calculation | Depth | Correct |
| UNT-012 | Calculation | Children count | Correct |
| UNT-013 | Calculation | Operation result | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Type filter | Filtered |
| UNT-018 | Status | Member filter | Filtered |
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
| PRF-006 | Get members | < 300ms |
| PRF-007 | Add member | < 200ms |
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
| CRUD groups | POS-001–005, FUN-001–006 |
| Membership management | POS-008–012, FUN-009–010 |
| Group-level operations | POS-022, FUN-015 |
| 3:1 Ratio | NEG-001–070, BND-001–070 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
