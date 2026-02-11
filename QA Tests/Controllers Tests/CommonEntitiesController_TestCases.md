# CommonEntitiesController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/CommonEntitiesController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for common/shared entities: lookup values, dropdown data, reference data, entity types.

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

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get lookup values | GET /api/common/lookups | Lookup list |
| POS-002 | Get lookup by type | GET /api/common/lookups?type=Status | Filtered |
| POS-003 | Get dropdown data | GET /api/common/dropdowns | Dropdown list |
| POS-004 | Get dropdown by entity | GET /api/common/dropdowns?entity=Partner | Entity dropdown |
| POS-005 | Get reference data | GET /api/common/reference | Reference data |
| POS-006 | Get entity types | GET /api/common/entity-types | Entity types |
| POS-007 | Get lookup by ID | GET /api/common/lookups/{id} | Lookup details |
| POS-008 | Typeahead search | GET /api/common/typeahead?q=text | Suggestions |
| POS-009 | Filter by parent | GET /api/common/lookups?parentId=1 | Child lookups |
| POS-010 | Filter by active | GET /api/common/lookups?active=true | Active only |
| POS-011 | Get localized lookups | GET /api/common/lookups?lang=fr | French labels |
| POS-012 | Get all dropdowns | GET /api/common/dropdowns/all | All dropdowns |
| POS-013 | Get countries dropdown | GET /api/common/dropdowns/countries | Countries |
| POS-014 | Get regions dropdown | GET /api/common/dropdowns/regions | Regions |
| POS-015 | Get partner types | GET /api/common/dropdowns/partner-types | Partner types |
| POS-016 | Get statuses | GET /api/common/dropdowns/statuses | Status list |
| POS-017 | Paginate lookups | GET /api/common/lookups?page=1 | Paginated |
| POS-018 | Sort lookups | GET /api/common/lookups?sortBy=name | Sorted |
| POS-019 | Search lookups | GET /api/common/lookups?search=text | Filtered |
| POS-020 | Get hierarchy | GET /api/common/lookups/hierarchy | Hierarchy tree |
| POS-021 | Get by code | GET /api/common/lookups/code/{code} | By code |
| POS-022 | Get metadata | GET /api/common/metadata | Metadata |
| POS-023 | Get validation rules | GET /api/common/validation-rules | Rules |
| POS-024 | Get enum values | GET /api/common/enums | Enum values |
| POS-025 | Get categories | GET /api/common/categories | Categories |
| POS-026 | Get tags | GET /api/common/tags | Tags |
| POS-027 | Get currencies | GET /api/common/currencies | Currencies |
| POS-028 | Get timezones | GET /api/common/timezones | Timezones |
| POS-029 | Get languages | GET /api/common/languages | Languages |
| POS-030 | Bulk lookup | POST /api/common/lookups/bulk | Bulk results |
| POS-031 | Cache hit | GET same endpoint twice | Cached response |
| POS-032 | Empty result | GET for empty type | [] |
| POS-033 | Single result | GET for single-item type | [item] |
| POS-034 | Authenticated access | GET with token | 200 OK |
| POS-035 | Public endpoint | GET /api/common/statuses | 200 (if public) |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth (if required) | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid type | type=Invalid | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Invalid parentId | parentId=999999 | 404 |
| NEG-007 | Invalid entity | entity=Invalid | 400 |
| NEG-008 | Invalid lang | lang=xx | 400 or default |
| NEG-009 | SQL injection | search='; DROP | Sanitized |
| NEG-010 | XSS in search | search=<script> | Sanitized |
| NEG-011 | Negative page | page=-1 | 400 |
| NEG-012 | Zero pageSize | pageSize=0 | 400 |
| NEG-013 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-014 | Invalid sortBy | sortBy=invalid | 400 |
| NEG-015 | Invalid code | code=invalid | 404 |
| NEG-016 | Empty code | code= | 400 |
| NEG-017 | Null required param | type=null | 400 |
| NEG-018 | Malformed JSON | Invalid JSON | 400 |
| NEG-019 | Wrong content-type | Application/xml | 415 |
| NEG-020 | No permission | User without access | 403 |
| NEG-021 | Cross-org lookup | Other org scope | 403 |
| NEG-022 | Deleted lookup | id of deleted | 404 |
| NEG-023 | Inactive lookup | inactive=true filter | Excluded |
| NEG-024 | Invalid bulk IDs | Bulk with invalid | 400/partial |
| NEG-025 | Empty bulk | POST [] | 400 |
| NEG-026 | Excessive bulk | 1000 IDs | 400 |
| NEG-027 | Rate limit | Too many requests | 429 |
| NEG-028 | Invalid enum name | enum=Invalid | 400 |
| NEG-029 | Path traversal | path=../../../ | 400 |
| NEG-030 | Reserved characters | code=\0 | 400 |
| NEG-031 | Control chars | name with \n | Sanitize |
| NEG-032 | Unicode overflow | Very long Unicode | 400 |
| NEG-033 | DB timeout | Simulate timeout | 503 |
| NEG-034 | Cache failure | Cache down | Fallback |
| NEG-035 | Missing locale | lang= | Default |
| NEG-036 | Invalid hierarchy | parentId=child | 400 |
| NEG-037 | Circular ref | Circular parent | 400 |
| NEG-038 | Duplicate code | Create duplicate | 409 |
| NEG-039 | Conflicting filters | Mutually exclusive | 400 |
| NEG-040 | Future date filter | date=2030 | 400 |
| NEG-041 | Invalid date format | date=invalid | 400 |
| NEG-042 | PUT on read-only | PUT /api/common/lookups | 405 |
| NEG-043 | DELETE on system | DELETE system lookup | 403 |
| NEG-044 | POST on read-only | POST (if read-only) | 405 |
| NEG-045 | OPTIONS | OPTIONS | 200 |
| NEG-046 | HEAD | HEAD | 200 or 405 |
| NEG-047 | Trailing slash | /api/common/lookups/ | Redirect |
| NEG-048 | Case sensitivity | /api/Common | 404 |
| NEG-049 | Extra path | /api/common/lookups/1/extra | 404 |
| NEG-050 | Invalid Accept | Accept: text/plain | 406 |
| NEG-051 | Payload too large | Huge body | 413 |
| NEG-052 | Method not allowed | PATCH | 405 |
| NEG-053 | Invalid bearer | Bearer malformed | 401 |
| NEG-054 | Revoked token | Revoked JWT | 401 |
| NEG-055 | Service account | Service for UI | 403 |
| NEG-056 | Audit failure | Audit down | Continue |
| NEG-057 | Orphan parent | parentId deleted | 404 |
| NEG-058 | Stale cache | TTL exceeded | Refresh |
| NEG-059 | Encoding error | Invalid charset | UTF-8 |
| NEG-060 | Empty type | type= | 400 |
| NEG-061 | Whitespace type | type="  " | 400 |
| NEG-062 | Max length exceeded | name 1000 chars | 400 |
| NEG-063 | Invalid UUID | id=invalid-guid | 400 |
| NEG-064 | Mismatched IDs | Path != body | 400 |
| NEG-065 | Read-only field | Update system field | Ignored |
| NEG-066 | Version conflict | Stale version | 409 |
| NEG-067 | Soft-delete filter | Query deleted | Excluded |
| NEG-068 | Inactive org | Org inactive | 403 |
| NEG-069 | Blocked IP | From blocked IP | 403 |
| NEG-070 | CORS preflight fail | Invalid origin | CORS error |

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
| BND-007 | parentId | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-008 | hierarchy depth | 1 | 10 | ✅ | ✅ | ❌ |
| BND-009 | bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-010 | type string | 1 | 100 | ✅ | ✅ | ❌ |
| BND-011 | Empty list | - | - | [] | - | - |
| BND-012 | Single item | - | - | [item] | - | - |
| BND-013 | First page | page=1 | - | ✅ | - | - |
| BND-014 | Last page | - | - | Partial | - | - |
| BND-015 | Zero length name | - | - | ❌ | - | - |
| BND-016 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-017 | Feb 29 | - | - | Valid | - | - |
| BND-018 | Unicode name | - | - | Accept | - | - |
| BND-019 | Emoji | - | - | Accept/reject | - | - |
| BND-020 | Arabic | - | - | Display | - | - |
| BND-021 | Chinese | - | - | Display | - | - |
| BND-022 | Null optional | - | - | Default | - | - |
| BND-023 | Empty string | - | - | No filter | - | - |
| BND-024 | Whitespace | - | - | Trim | - | - |
| BND-025 | Sort empty | - | - | [] | - | - |
| BND-026 | Sort single | - | - | [item] | - | - |
| BND-027 | Filter no match | - | - | [] | - | - |
| BND-028 | Filter all | - | - | Full | - | - |
| BND-029 | Timezone | UTC | - | Correct | - | - |
| BND-030 | Locale | en, fr, es, pt | - | Correct | - | - |
| BND-031 | Decimal precision | - | 2 | Rounded | - | - |
| BND-032 | Integer overflow | - | - | Use long | - | - |
| BND-033 | Sequence | 1 | - | ✅ | ❌ | - |
| BND-034 | Order/rank | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-035 | Active flag | - | - | true/false | - | - |
| BND-036 | IsDefault | - | - | One default | - | - |
| BND-037 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-038 | Cache TTL | - | - | Expire | - | - |
| BND-039 | Empty dropdown | - | - | [] | - | - |
| BND-040 | Single dropdown | - | - | [item] | - | - |
| BND-041 | Max dropdown items | - | 1000 | ✅ | ✅ | ❌ |
| BND-042 | Typeahead min | 1 | - | ✅ | - | - |
| BND-043 | Typeahead max | - | 20 | - | ✅ | ❌ |
| BND-044 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-045 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-046 | Nested lookup | - | 5 | ✅ | ✅ | ❌ |
| BND-047 | Duplicate code | - | - | Reject | - | - |
| BND-048 | Case sensitivity code | - | - | Define | - | - |
| BND-049 | Null code | - | - | Reject | - | - |
| BND-050 | Empty code | - | - | Reject | - | - |
| BND-051 | Special chars code | - | - | Define | - | - |
| BND-052 | Leading/trailing space | - | - | Trim | - | - |
| BND-053 | Zero ID | id=0 | - | 400 | - | - |
| BND-054 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-055 | Negative order | order=-1 | - | 400 | - | - |
| BND-056 | Pagination boundary | - | - | Exact | - | - |
| BND-057 | Cursor pagination | - | - | Valid | - | - |
| BND-058 | Empty bulk | - | - | 400 | - | - |
| BND-059 | Partial bulk | - | - | 207 | - | - |
| BND-060 | Round-trip | Get → use | - | Same | - | - |
| BND-061 | Soft-deleted | - | - | Excluded | - | - |
| BND-062 | Inactive | - | - | Filter | - | - |
| BND-063 | Hierarchy root | parentId=0 | - | Root | - | - |
| BND-064 | Hierarchy leaf | No children | - | Leaf | - | - |
| BND-065 | Missing locale | - | - | Default | - | - |
| BND-066 | Fallback locale | Lang not found | - | en | - | - |
| BND-067 | Label length | - | 500 | ✅ | ✅ | ❌ |
| BND-068 | Description length | - | 2000 | ✅ | ✅ | ❌ |
| BND-069 | Metadata size | - | 10KB | ✅ | ✅ | ❌ |
| BND-070 | Version | 1 | - | ✅ | ❌ | - |

---

## §4 Functional Tests (50)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get lookups | GET | List |
| FUN-002 | Workflow | Filter by type | GET ?type | Filtered |
| FUN-003 | Workflow | Filter by parent | GET ?parentId | Filtered |
| FUN-004 | Workflow | Get dropdown | GET dropdown | Options |
| FUN-005 | Workflow | Typeahead | GET ?q | Suggestions |
| FUN-006 | Workflow | Localized | GET ?lang | Labels |
| FUN-007 | Workflow | Hierarchy | GET hierarchy | Tree |
| FUN-008 | Workflow | By code | GET code | Lookup |
| FUN-009 | Workflow | Bulk | POST bulk | Results |
| FUN-010 | Workflow | Paginate | GET ?page | Paginated |
| FUN-011 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-012 | Workflow | Search | GET ?search | Filtered |
| FUN-013 | Workflow | Active filter | GET ?active | Filtered |
| FUN-014 | Workflow | Admin create | POST (admin) | 201 |
| FUN-015 | Workflow | Admin update | PUT (admin) | 200 |
| FUN-016 | Validation | Required type | Missing type | 400 |
| FUN-017 | Validation | Valid ID | Invalid ID | 400 |
| FUN-018 | Validation | Valid code | Invalid code | 400 |
| FUN-019 | Validation | Valid lang | Invalid lang | 400 |
| FUN-020 | Validation | No duplicate code | Duplicate | 409 |
| FUN-021 | Validation | Permission | No permission | 403 |
| FUN-022 | Validation | Org scope | Cross-org | 403 |
| FUN-023 | Validation | Whitelist type | Invalid type | 400 |
| FUN-024 | Validation | Max length | Too long | 400 |
| FUN-025 | Validation | Format | Wrong format | 400 |
| FUN-026 | Constraint | Unique code | Duplicate | 409 |
| FUN-027 | Constraint | FK parent | Invalid parent | 404 |
| FUN-028 | Constraint | Max hierarchy | >10 | 400 |
| FUN-029 | Constraint | No circular | Circular | 400 |
| FUN-030 | Constraint | System lock | Update system | 403 |
| FUN-031 | Constraint | Soft delete | Query deleted | Excluded |
| FUN-032 | Constraint | Active default | One default | Enforce |
| FUN-033 | Constraint | Order unique | Same order | Allow/reject |
| FUN-034 | Constraint | Max bulk | >100 | 400 |
| FUN-035 | Constraint | Cache TTL | Stale | Refresh |
| FUN-036 | Audit | Read logged | GET (if sensitive) | Audit |
| FUN-037 | Audit | Create logged | POST | Audit |
| FUN-038 | Audit | Update logged | PUT | Audit |
| FUN-039 | Audit | Delete logged | DELETE | Audit |
| FUN-040 | Audit | Timestamp | Any | UTC |
| FUN-041 | Audit | User ID | Any | User ID |
| FUN-042 | Audit | IP | Any | IP |
| FUN-043 | Audit | Resource | Any | Resource ID |
| FUN-044 | Audit | Outcome | Any | Outcome |
| FUN-045 | Audit | Bulk | Bulk | Each or batch |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Filter |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Locale fallback | Missing | Default |
| FUN-050 | Business | Hierarchy sort | Children | Ordered |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get lookup | Lookup | Details |
| INT-002 | CRUD | Get by type | Lookup | Filtered |
| INT-003 | CRUD | Get by code | Lookup | Match |
| INT-004 | CRUD | Get hierarchy | Lookup | Tree |
| INT-005 | CRUD | Create (admin) | Lookup | 201 |
| INT-006 | CRUD | Update (admin) | Lookup | 200 |
| INT-007 | CRUD | Delete (admin) | Lookup | 204 |
| INT-008 | CRUD | Restore | Lookup | 200 |
| INT-009 | CRUD | Bulk get | Lookup | Results |
| INT-010 | CRUD | Dropdown all | Dropdown | All |
| INT-011 | Search | Search by name | Lookup | Matches |
| INT-012 | Search | Typeahead | Lookup | Suggestions |
| INT-013 | Search | Filter type | Lookup | Filtered |
| INT-014 | Search | Filter parent | Lookup | Filtered |
| INT-015 | Search | Multi-filter | Lookup | Combined |
| INT-016 | Search | Empty search | - | [] |
| INT-017 | Search | Partial match | Lookup | Fuzzy |
| INT-018 | Search | Sort + filter | Lookup | Both |
| INT-019 | Search | Filter + pagination | Lookup | Both |
| INT-020 | Search | Localized search | Lookup | Lang |
| INT-021 | Pagination | Page 1 | Lookup | First |
| INT-022 | Pagination | Last page | Lookup | Partial |
| INT-023 | Pagination | Size | Lookup | Correct |
| INT-024 | Pagination | Invalid | Lookup | 400 |
| INT-025 | Pagination | Boundary | Lookup | Exact |
| INT-026 | Relationships | Lookup → Parent | Lookup | Linked |
| INT-027 | Relationships | Lookup → Children | Lookup | Children |
| INT-028 | Relationships | Entity → Lookup | Entity, Lookup | Ref |
| INT-029 | Relationships | Orphan | Deleted parent | 404 |
| INT-030 | Relationships | Hierarchy | Lookup | Tree |
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
| INT-046 | E2E | Full get flow | Lookup | Get → Use |
| INT-047 | E2E | Dropdown flow | Dropdown | Get → Render |
| INT-048 | E2E | Typeahead flow | Typeahead | Type → Select |
| INT-049 | E2E | Multi-user | Users | No conflict |
| INT-050 | E2E | Session expiry | Auth | Clean fail |

---

## §6 Security Tests (50)

| ID | Category | Attack | Target | Expected |
|----|----------|--------|-------|----------|
| SEC-001 | Injection | SQL | Filter | Sanitized |
| SEC-002 | Injection | XSS | Search | Encoded |
| SEC-003 | Injection | Path traversal | Path | Rejected |
| SEC-004 | Injection | NoSQL | Filter | Rejected |
| SEC-005 | Injection | Command | Export | Rejected |
| SEC-006 | Injection | Header | Header | Rejected |
| SEC-007 | Injection | Log | Input | Sanitized |
| SEC-008 | Injection | LDAP | Search | Rejected |
| SEC-009 | Injection | Log4j | Input | Rejected |
| SEC-010 | Injection | Template | Input | Rejected |
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
| SEC-021 | IDOR | Other org | ID | 403 |
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
| SEC-036 | Data | PII | Export | Masked |
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
| CON-002 | 2 users create same code | One fails 409 |
| CON-003 | Update during read | Snapshot |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click get | Single |
| CON-007 | Rapid filter | Last wins |
| CON-008 | Cache invalidation | No stale |
| CON-009 | Bulk concurrent | Queue |
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
| CON-021 | Bulk during update | Consistent |
| CON-022 | Hierarchy change | Snapshot |
| CON-023 | Permission change | Old |
| CON-024 | Dropdown concurrent | Both |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid type | Accept |
| UNT-002 | Validation | Invalid type | Reject |
| UNT-003 | Validation | Valid ID | Accept |
| UNT-004 | Validation | Invalid ID | Reject |
| UNT-005 | Validation | Valid code | Accept |
| UNT-006 | Formatting | Date | ISO 8601 |
| UNT-007 | Formatting | Number | Localized |
| UNT-008 | Formatting | Label | Localized |
| UNT-009 | Calculation | Order | Correct |
| UNT-010 | Calculation | Rank | Correct |
| UNT-011 | Calculation | Count | Correct |
| UNT-012 | Calculation | Hierarchy depth | Correct |
| UNT-013 | Calculation | Path | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Default | Default only |
| UNT-018 | Status | Non-default | Exclude default |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get lookup | < 50ms |
| PRF-002 | List lookups | < 200ms |
| PRF-003 | Get dropdown | < 100ms |
| PRF-004 | Typeahead | < 200ms |
| PRF-005 | Get hierarchy | < 500ms |
| PRF-006 | Bulk get | < 1s |
| PRF-007 | Search | < 300ms |
| PRF-008 | Filter | < 200ms |
| PRF-009 | Paginate | < 300ms |
| PRF-010 | 10 concurrent | < 500ms each |
| PRF-011 | 50 concurrent | < 1s each |
| PRF-012 | 5 concurrent bulk | < 2s each |
| PRF-013 | Memory list | < 20MB |
| PRF-014 | Memory bulk | < 50MB |
| PRF-015 | Cache hit | > 90% |
| PRF-016 | DB queries | < 3 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users | 10 min | 95% < 200ms |
| LDT-002 | 50 users | 10 min | 95% < 500ms |
| LDT-003 | 100 users | 10 min | 95% < 1s |
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
| Lookup values | POS-001–002, FUN-001 |
| Dropdown data | POS-003–004, FUN-004 |
| Reference data | POS-005, INT-010 |
| Entity types | POS-006, NEG-003 |
| 3:1 Ratio | NEG-001–070, BND-001–070 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
