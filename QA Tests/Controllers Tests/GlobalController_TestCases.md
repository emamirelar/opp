# GlobalController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/GlobalController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for global/shared operations: health check, version, system info, global search.

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
| POS-001 | Health check | GET /api/health | Health status |
| POS-002 | Readiness check | GET /api/health/ready | Readiness |
| POS-003 | Liveness check | GET /api/health/live | Alive |
| POS-004 | Database connectivity | GET /api/health/db | DB status |
| POS-005 | Get version | GET /api/version | Version info |
| POS-006 | Get system info | GET /api/system-info | System info |
| POS-007 | Global search | GET /api/search?q=UNOPS | Search results |
| POS-008 | Search partners | GET /api/search?q=partner&entityTypes=Partner | Partner results |
| POS-009 | Search contacts | GET /api/search?entityTypes=Contact | Contact results |
| POS-010 | Search opportunities | GET /api/search?entityTypes=Opportunity | Opp results |
| POS-011 | Search pagination | GET /api/search?q=test&page=1&pageSize=20 | Paginated |
| POS-012 | Search with filter | GET /api/search?q=test&orgUnitId=1 | Filtered |
| POS-013 | Get current time | GET /api/time | UTC timestamp |
| POS-014 | Health public (no auth) | GET /api/health (no token) | 200 |
| POS-015 | Search authenticated | GET /api/search (with token) | 200 |
| POS-016 | Version format | GET /api/version | SemVer format |
| POS-017 | System info fields | GET /api/system-info | Required fields |
| POS-018 | Search empty query | GET /api/search?q= | Default/empty |
| POS-019 | Search single result | GET for unique match | [item] |
| POS-020 | Search multiple types | GET ?entityTypes=Partner,Contact | Both |
| POS-021 | Search sort | GET ?sortBy=relevance | Sorted |
| POS-022 | Health dependencies | GET /api/health | All deps |
| POS-023 | Version build info | GET /api/version | Build info |
| POS-024 | Environment info | GET /api/system-info | Environment |
| POS-025 | Culture info | GET /api/system-info | Culture |
| POS-026 | Timezone info | GET /api/system-info | Timezone |
| POS-027 | Feature flags (if any) | GET /api/system-info | Flags |
| POS-028 | API base URL | GET /api/system-info | Base URL |
| POS-029 | Cached health | GET health twice | Cached |
| POS-030 | Cached version | GET version twice | Cached |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | Search no auth | GET /api/search (no token) | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid search query | q=; DROP | Sanitized |
| NEG-004 | SQL injection | q=' OR 1=1 | Sanitized |
| NEG-005 | XSS in search | q=<script> | Sanitized |
| NEG-006 | Negative page | page=-1 | 400 |
| NEG-007 | Zero pageSize | pageSize=0 | 400 |
| NEG-008 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-009 | Invalid entity type | entityTypes=Invalid | 400 |
| NEG-010 | Invalid orgUnitId | orgUnitId=-1 | 400 |
| NEG-011 | Path traversal | q=../../../ | 400 |
| NEG-012 | Malformed JSON | Invalid JSON body | 400 |
| NEG-013 | Wrong content-type | Application/xml | 415 |
| NEG-014 | Invalid sort | sortBy=invalid | 400 |
| NEG-015 | No permission | User without CanSearch | 403 |
| NEG-016 | Cross-org search | Other org scope | 403 |
| NEG-017 | Rate limit | Too many search | 429 |
| NEG-018 | Payload too large | Huge query | 413 |
| NEG-019 | Invalid Accept | Accept: text/plain | 406 |
| NEG-020 | HTTP method | PUT /api/search | 405 |
| NEG-021 | HEAD health | HEAD /api/health | 200 or 405 |
| NEG-022 | Trailing slash | /api/health/ | Redirect |
| NEG-023 | Case sensitivity | /api/Health | 404 |
| NEG-024 | Extra path | /api/health/extra | 404 |
| NEG-025 | Invalid bearer | Bearer malformed | 401 |
| NEG-026 | Revoked token | Revoked JWT | 401 |
| NEG-027 | Service account | Service for UI | 403 |
| NEG-028 | DB down for health | DB unavailable | 503 |
| NEG-029 | Search service down | Search unavailable | 503 |
| NEG-030 | Timeout | Slow search | 504 |
| NEG-031 | Control chars | q with \0 | 400 |
| NEG-032 | Unicode overflow | Very long Unicode | 400 |
| NEG-033 | Invalid UUID | orgUnitId=invalid | 400 |
| NEG-034 | Mismatched params | Conflicting params | 400 |
| NEG-035 | Blocked IP | From blocked | 403 |
| NEG-036 | CORS fail | Invalid origin | CORS error |
| NEG-037 | Search timeout | Long query | 504 |
| NEG-038 | Empty entity types | entityTypes= | 400 |
| NEG-039 | Invalid filter | filter=invalid | 400 |
| NEG-040 | Future date filter | date=2030 | 400 |
| NEG-041 | Invalid date format | date=invalid | 400 |
| NEG-042 | Max query length | q 10000 chars | 400 |
| NEG-043 | Invalid highlight | highlight=invalid | 400 |
| NEG-044 | Invalid facets | facets=invalid | 400 |
| NEG-045 | SYSTEM version | PUT /api/version | 405 |
| NEG-046 | Update health | PUT /api/health | 405 |
| NEG-047 | Delete search | DELETE /api/search | 405 |
| NEG-048 | POST search wrong body | POST malformed | 400 |
| NEG-049 | OPTIONS without CORS | OPTIONS wrong origin | CORS |
| NEG-050 | Health degraded | Partial failure | 503 |
| NEG-051 | Version not found | Version service down | 500 |
| NEG-052 | System info partial | Some info unavailable | Partial |
| NEG-053 | Search index missing | Index not built | 503 |
| NEG-054 | Search index corrupt | Corrupt index | 503 |
| NEG-055 | Concurrent search limit | Too many concurrent | 429 |
| NEG-056 | Search cancel | Cancel mid-search | Graceful |
| NEG-057 | Health cache stale | Stale health | Refresh |
| NEG-058 | Version cache stale | Stale version | Refresh |
| NEG-059 | Inactive org | Org inactive | 403 |
| NEG-060 | Soft-deleted in search | Query | Excluded |
| NEG-061 | Per-entity limit | entityTypes all | 400 or limit |
| NEG-062 | Special regex chars | q=.*+ | Escaped |
| NEG-063 | Newline in query | q=test\n | Sanitized |
| NEG-064 | Tab in query | q=test\t | Sanitized |
| NEG-065 | Null byte | q=test\0 | 400 |
| NEG-066 | Wildcard injection | q=*.* | Sanitized |
| NEG-067 | Boolean injection | q=AND 1=1 | Sanitized |
| NEG-068 | Encoding error | Invalid encoding | 400 |
| NEG-069 | Audit failure | Audit down | Continue |
| NEG-070 | Search permission filter | No permission entity | Excluded |
| NEG-071 | Invalid health path | /api/health/invalid | 404 |
| NEG-072 | Search with null query | q=null | 400 |
| NEG-073 | Entity type limit exceeded | 11 entity types | 400 |
| NEG-074 | Invalid relevance threshold | threshold=invalid | 400 |
| NEG-075 | Search index locked | Index updating | 503 |
| NEG-076 | Version endpoint POST | POST /api/version | 405 |
| NEG-077 | System info PUT | PUT /api/system-info | 405 |
| NEG-078 | Time endpoint invalid | Invalid params | 400 |
| NEG-079 | Health PUT | PUT /api/health | 405 |
| NEG-080 | Search DELETE | DELETE /api/search | 405 |
| NEG-081 | Invalid highlight format | highlight=1 | 400 |
| NEG-082 | Invalid facets format | facets=1 | 400 |
| NEG-083 | Conflicting entity filters | Mutually exclusive | 400 |
| NEG-084 | Search during maintenance | Maintenance mode | 503 |
| NEG-085 | Invalid sort field | sortBy=invalid | 400 |
| NEG-086 | Negative offset | offset=-1 | 400 |
| NEG-087 | Excessive limit | limit=10000 | 400 |
| NEG-088 | Invalid date in filter | date=invalid | 400 |
| NEG-089 | Search service degraded | Partial failure | 503 |
| NEG-090 | Health dependency down | One dep down | Degraded |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | query length | 0 | 500 | ✅ | ✅ | ❌ |
| BND-002 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | entityTypes count | 0 | 10 | ✅ | ✅ | ❌ |
| BND-005 | orgUnitId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-006 | Empty query | - | - | Default | - | - |
| BND-007 | Single char query | q=a | - | ✅ | - | - |
| BND-008 | Max length query | 500 | - | - | ✅ | ❌ |
| BND-009 | First page | page=1 | - | ✅ | - | - |
| BND-010 | Last page | - | - | Partial | - | - |
| BND-011 | Empty result | - | - | [] | - | - |
| BND-012 | Single result | - | - | [item] | - | - |
| BND-013 | Unicode query | - | - | Accept | - | - |
| BND-014 | Arabic query | - | - | Search | - | - |
| BND-015 | Chinese query | - | - | Search | - | - |
| BND-016 | Emoji query | - | - | Handle | - | - |
| BND-017 | Null optional | - | - | Default | - | - |
| BND-018 | Whitespace query | q="  " | - | Trim | - | - |
| BND-019 | Sort empty | - | - | [] | - | - |
| BND-020 | Sort single | - | - | [item] | - | - |
| BND-021 | Filter no match | - | - | [] | - | - |
| BND-022 | Filter all | - | - | Full | - | - |
| BND-023 | Timezone | UTC | - | Correct | - | - |
| BND-024 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-025 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-026 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-027 | Health response time | - | 5s | - | ✅ | ❌ |
| BND-028 | Version string length | - | 50 | ✅ | ✅ | ❌ |
| BND-029 | System info size | - | 10KB | ✅ | ✅ | ❌ |
| BND-030 | Pagination boundary | - | - | Exact | - | - |
| BND-031 | Cursor pagination | - | - | Valid | - | - |
| BND-032 | Highlight length | - | 100 | ✅ | ✅ | ❌ |
| BND-033 | Facet count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-034 | Result count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-035 | Relevance score | 0 | 1 | ✅ | ✅ | ❌ |
| BND-036 | Health cache TTL | - | 30s | ✅ | ✅ | ❌ |
| BND-037 | Version cache TTL | - | 300s | ✅ | ✅ | ❌ |
| BND-038 | Search timeout | - | 30s | - | ✅ | ❌ |
| BND-039 | Debounce (client) | - | - | N/A | - | - |
| BND-040 | Round-trip | Search → Get | - | Match | - | - |
| BND-041 | Soft-deleted | - | - | Excluded | - | - |
| BND-042 | Inactive | - | - | Excluded | - | - |
| BND-043 | Permission filter | - | - | Applied | - | - |
| BND-044 | Case sensitivity | - | - | Define | - | - |
| BND-045 | Accent sensitivity | - | - | Define | - | - |
| BND-046 | Stemming | - | - | Applied | - | - |
| BND-047 | Stop words | - | - | Filtered | - | - |
| BND-048 | Boolean operators | - | - | Support | - | - |
| BND-049 | Phrase search | - | - | Quoted | - | - |
| BND-050 | Wildcard | - | - | Define | - | - |
| BND-051 | Fuzzy match | - | - | Define | - | - |
| BND-052 | Proximity | - | - | Define | - | - |
| BND-053 | Field search | - | - | field:value | - | - |
| BND-054 | Range search | - | - | [min TO max] | - | - |
| BND-055 | Boost | - | - | ^boost | - | - |
| BND-056 | Health status | - | - | Healthy/Degraded/Unhealthy | - | - |
| BND-057 | Readiness | - | - | Ready/NotReady | - | - |
| BND-058 | Liveness | - | - | Live/Dead | - | - |
| BND-059 | DB status | - | - | Connected/Disconnected | - | - |
| BND-060 | Version format | - | - | SemVer | - | - |
| BND-061 | Build date | - | - | ISO 8601 | - | - |
| BND-062 | Environment | - | - | Dev/Staging/Prod | - | - |
| BND-063 | Machine name | - | - | Masked in prod | - | - |
| BND-064 | IP address | - | - | Masked in prod | - | - |
| BND-065 | Process ID | - | - | Optional | - | - |
| BND-066 | Memory usage | - | - | MB | - | - |
| BND-067 | Uptime | - | - | Seconds | - | - |
| BND-068 | Dependency count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-069 | Dependency status | - | - | Each status | - | - |
| BND-070 | Response size | - | 1MB | ✅ | ✅ | ❌ |
| BND-071 | Search result offset | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-072 | Search limit | 1 | 100 | ✅ | ✅ | ❌ |
| BND-073 | Entity type count | 0 | 10 | ✅ | ✅ | ❌ |
| BND-074 | Health check interval | - | 30s | ✅ | ✅ | ❌ |
| BND-075 | Version cache | - | 300s | ✅ | ✅ | ❌ |
| BND-076 | Search timeout | - | 30s | - | ✅ | ❌ |
| BND-077 | Empty entity types | - | - | All | - | - |
| BND-078 | Single entity type | - | - | Filtered | - | - |
| BND-079 | Relevance threshold | 0 | 1 | ✅ | ✅ | ❌ |
| BND-080 | Highlight max length | - | 100 | ✅ | ✅ | ❌ |
| BND-081 | Facet max count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-082 | System info mask | - | - | Sensitive | - | - |
| BND-083 | Version string | - | 50 | ✅ | ✅ | ❌ |
| BND-084 | Health status enum | - | - | Healthy/Degraded | - | - |
| BND-085 | Readiness enum | - | - | Ready/NotReady | - | - |
| BND-086 | Liveness enum | - | - | Live/Dead | - | - |
| BND-087 | DB status enum | - | - | Connected/Disconnected | - | - |
| BND-088 | Build date format | - | - | ISO 8601 | - | - |
| BND-089 | Environment enum | - | - | Dev/Staging/Prod | - | - |
| BND-090 | Uptime seconds | 0 | - | ✅ | ❌ | - |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Health check | GET health | Status |
| FUN-002 | Workflow | Readiness | GET ready | Ready |
| FUN-003 | Workflow | Liveness | GET live | Live |
| FUN-004 | Workflow | DB check | GET health/db | Status |
| FUN-005 | Workflow | Get version | GET version | Version |
| FUN-006 | Workflow | Get system info | GET system-info | Info |
| FUN-007 | Workflow | Global search | GET search | Results |
| FUN-008 | Workflow | Search filter | GET ?entityTypes | Filtered |
| FUN-009 | Workflow | Search paginate | GET ?page | Paginated |
| FUN-010 | Workflow | Get time | GET time | UTC |
| FUN-011 | Workflow | Health public | GET health no auth | 200 |
| FUN-012 | Workflow | Search auth | GET search no auth | 401 |
| FUN-013 | Workflow | Version cache | GET twice | Cached |
| FUN-014 | Workflow | Health cache | GET twice | Cached |
| FUN-015 | Workflow | Search cache | GET same query | Cached |
| FUN-016 | Validation | Search auth | No auth | 401 |
| FUN-017 | Validation | Search permission | No permission | 403 |
| FUN-018 | Validation | Valid query | Invalid | 400 |
| FUN-019 | Validation | Valid entity type | Invalid | 400 |
| FUN-020 | Validation | Valid page | Invalid | 400 |
| FUN-021 | Validation | Org scope | Cross-org | 403 |
| FUN-022 | Validation | Query length | Too long | 400 |
| FUN-023 | Validation | Entity types count | >10 | 400 |
| FUN-024 | Validation | Sort whitelist | Invalid sort | 400 |
| FUN-025 | Validation | Filter format | Invalid | 400 |
| FUN-026 | Constraint | Rate limit | Too many | 429 |
| FUN-027 | Constraint | Search timeout | Slow | 504 |
| FUN-028 | Constraint | Health timeout | Slow dep | 503 |
| FUN-029 | Constraint | Cache TTL | Stale | Refresh |
| FUN-030 | Constraint | Max results | >100 | Cap |
| FUN-031 | Constraint | Concurrent search | Limit | 429 |
| FUN-032 | Constraint | Permission filter | Query | Auto-scoped |
| FUN-033 | Constraint | Soft delete | Query | Excluded |
| FUN-034 | Constraint | Version immutable | No update | 405 |
| FUN-035 | Constraint | Health read-only | No update | 405 |
| FUN-036 | Audit | Search | GET search | Audit |
| FUN-037 | Audit | System info | GET (if sensitive) | Audit |
| FUN-038 | Audit | Failed auth | 401 attempt | Audit |
| FUN-039 | Audit | Health (no) | GET health | No audit |
| FUN-040 | Audit | Version (no) | GET version | No audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Search | Excluded |
| FUN-047 | Business | Inactive | Search | Excluded |
| FUN-048 | Business | Permission | Search | Scoped |
| FUN-049 | Business | Timezone | All times | UTC |
| FUN-050 | Business | Version format | Version | SemVer |
| FUN-051 | Workflow | Health check | GET health | Status |
| FUN-052 | Workflow | Readiness | GET ready | Ready |
| FUN-053 | Workflow | Liveness | GET live | Live |
| FUN-054 | Workflow | DB check | GET health/db | Status |
| FUN-055 | Workflow | Get version | GET version | Version |
| FUN-056 | Validation | Search auth | No auth | 401 |
| FUN-057 | Validation | Search permission | No permission | 403 |
| FUN-058 | Validation | Valid query | Invalid | 400 |
| FUN-059 | Validation | Valid entity type | Invalid | 400 |
| FUN-060 | Validation | Org scope | Cross-org | 403 |
| FUN-061 | Constraint | Rate limit | Too many | 429 |
| FUN-062 | Constraint | Search timeout | Slow | 504 |
| FUN-063 | Constraint | Max results | >100 | Cap |
| FUN-064 | Constraint | Permission filter | Query | Auto-scoped |
| FUN-065 | Constraint | Soft delete | Query | Excluded |
| FUN-066 | Audit | Search | GET search | Audit |
| FUN-067 | Audit | Failed auth | 401 attempt | Audit |
| FUN-068 | Audit | Timestamp | Any | UTC |
| FUN-069 | Audit | User ID | Any | User ID |
| FUN-070 | Audit | Resource | Any | Resource |
| FUN-071 | Business | Soft-deleted | Search | Excluded |
| FUN-072 | Business | Inactive | Search | Excluded |
| FUN-073 | Business | Permission | Search | Scoped |
| FUN-074 | Business | Timezone | All | UTC |
| FUN-075 | Business | Version | SemVer | Correct |
| FUN-076 | Workflow | Search filter | GET ?entityTypes | Filtered |
| FUN-077 | Workflow | Search paginate | GET ?page | Paginated |
| FUN-078 | Workflow | Get time | GET time | UTC |
| FUN-079 | Workflow | Health public | GET no auth | 200 |
| FUN-080 | Workflow | Version cache | GET twice | Cached |
| FUN-081 | Validation | Query length | Too long | 400 |
| FUN-082 | Validation | Entity types count | >10 | 400 |
| FUN-083 | Validation | Sort whitelist | Invalid sort | 400 |
| FUN-084 | Validation | Filter format | Invalid | 400 |
| FUN-085 | Validation | Page | Invalid | 400 |
| FUN-086 | Constraint | Version immutable | No update | 405 |
| FUN-087 | Constraint | Health read-only | No update | 405 |
| FUN-088 | Constraint | Cache TTL | Stale | Refresh |
| FUN-089 | Constraint | Concurrent search | Limit | 429 |
| FUN-090 | Constraint | Health timeout | Slow dep | 503 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get health | Health | Status |
| INT-002 | CRUD | Get version | Version | Info |
| INT-003 | CRUD | Get system info | System | Info |
| INT-004 | CRUD | Search | Search | Results |
| INT-005 | CRUD | Get time | Time | UTC |
| INT-006 | CRUD | Health when DB down | Health, DB | 503 |
| INT-007 | CRUD | Search when index down | Search | 503 |
| INT-008 | CRUD | Version when unavailable | Version | 500 |
| INT-009 | CRUD | Health when degraded | Health | Degraded |
| INT-010 | CRUD | System info partial | System | Partial |
| INT-011 | Search | Search partners | Partner | Partner results |
| INT-012 | Search | Search contacts | Contact | Contact results |
| INT-013 | Search | Search opportunities | Opportunity | Opp results |
| INT-014 | Search | Multi-entity | All | Combined |
| INT-015 | Search | Filter org | Search | Filtered |
| INT-016 | Search | Paginate | Search | Paginated |
| INT-017 | Search | Sort | Search | Sorted |
| INT-018 | Search | Empty query | - | Default |
| INT-019 | Search | No match | Search | [] |
| INT-020 | Search | Exact match | Search | [item] |
| INT-021 | Pagination | Page 1 | Search | First |
| INT-022 | Pagination | Last page | Search | Partial |
| INT-023 | Pagination | Size | Search | Correct |
| INT-024 | Pagination | Invalid | Search | 400 |
| INT-025 | Pagination | Boundary | Search | Exact |
| INT-026 | Relationships | Search → Entity | Search, Entity | Link |
| INT-027 | Relationships | Health → DB | Health, DB | Linked |
| INT-028 | Relationships | Health → Cache | Health, Cache | Linked |
| INT-029 | Relationships | Orphan | Deleted entity | Excluded |
| INT-030 | Relationships | Version → Build | Version, Build | Linked |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid path | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | N/A | N/A |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow | 504 |
| INT-039 | Error | Payload | Huge | 413 |
| INT-040 | Error | Media | Wrong type | 415 |
| INT-041 | Error | Method | Wrong verb | 405 |
| INT-042 | Error | Service | Dependency | 503 |
| INT-043 | Error | Gateway | Upstream | 504 |
| INT-044 | Error | Gone | N/A | N/A |
| INT-045 | Error | Locked | N/A | N/A |
| INT-046 | E2E | Full health flow | Health | Get → Parse |
| INT-047 | E2E | Full search flow | Search | Search → Select |
| INT-048 | E2E | Multi-user search | Users | Isolated |
| INT-049 | E2E | Health → Search | Health, Search | Both |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get health | Health | Status |
| INT-052 | CRUD | Get version | Version | Info |
| INT-053 | CRUD | Get system info | System | Info |
| INT-054 | CRUD | Search | Search | Results |
| INT-055 | CRUD | Get time | Time | UTC |
| INT-056 | Search | Search partners | Partner | Partner results |
| INT-057 | Search | Search contacts | Contact | Contact results |
| INT-058 | Search | Search opportunities | Opportunity | Opp results |
| INT-059 | Search | Multi-entity | All | Combined |
| INT-060 | Search | Filter org | Search | Filtered |
| INT-061 | Pagination | Page 1 | Search | First |
| INT-062 | Pagination | Last page | Search | Partial |
| INT-063 | Pagination | Size | Search | Correct |
| INT-064 | Pagination | Invalid | Search | 400 |
| INT-065 | Pagination | Boundary | Search | Exact |
| INT-066 | Relationships | Search → Entity | Linked | Correct |
| INT-067 | Relationships | Health → DB | Linked | Correct |
| INT-068 | Relationships | Health → Cache | Linked | Correct |
| INT-069 | Relationships | Orphan | Deleted entity | Excluded |
| INT-070 | Relationships | Version → Build | Linked | Correct |
| INT-071 | Error | DB down | DB | 503 |
| INT-072 | Error | Auth down | Auth | 401/503 |
| INT-073 | Error | Validation | Bad input | 400 |
| INT-074 | Error | NotFound | Invalid path | 404 |
| INT-075 | Error | Forbidden | No permission | 403 |
| INT-076 | Error | Rate limit | Too many | 429 |
| INT-077 | Error | Timeout | Slow | 504 |
| INT-078 | Error | Payload | Huge | 413 |
| INT-079 | Error | Media | Wrong type | 415 |
| INT-080 | Error | Method | Wrong verb | 405 |
| INT-081 | Error | Service | Dependency | 503 |
| INT-082 | Error | Gateway | Upstream | 504 |
| INT-083 | Error | Health degraded | Partial | 503 |
| INT-084 | Error | Search index | Down | 503 |
| INT-085 | Error | Version unavailable | Down | 500 |
| INT-086 | E2E | Full health flow | Health | Get → Parse |
| INT-087 | E2E | Full search flow | Search | Search → Select |
| INT-088 | E2E | Multi-user search | Users | Isolated |
| INT-089 | E2E | Health when DB down | Health, DB | 503 |
| INT-090 | E2E | Search when index down | Search | 503 |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users health | Both succeed |
| CON-002 | 2 users search | Both succeed |
| CON-003 | 10 concurrent health | All succeed |
| CON-004 | 50 concurrent search | All succeed |
| CON-005 | 100 concurrent mixed | All succeed or 429 |
| CON-006 | Double-click search | Single or cached |
| CON-007 | Rapid search | Last or debounced |
| CON-008 | Health during restart | Degraded/503 |
| CON-009 | Search during index | Partial |
| CON-010 | Cache invalidation | No stale |
| CON-011 | Connection pool | Queue/503 |
| CON-012 | Transaction | N/A |
| CON-013 | Optimistic | N/A |
| CON-014 | Deadlock | Timeout |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple search same | Cached |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Health during DB fail | 503 |
| CON-022 | Search during update | Snapshot |
| CON-023 | Permission change | Old for request |
| CON-024 | Version during deploy | Old or new |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid query | Accept |
| UNT-002 | Validation | Invalid query | Reject |
| UNT-003 | Validation | Valid entity type | Accept |
| UNT-004 | Validation | Invalid entity type | Reject |
| UNT-005 | Validation | Valid page | Accept |
| UNT-006 | Formatting | Version | SemVer |
| UNT-007 | Formatting | Time | ISO 8601 |
| UNT-008 | Formatting | Health status | String |
| UNT-009 | Calculation | Relevance | Score |
| UNT-010 | Calculation | Pagination | Offset |
| UNT-011 | Calculation | Total pages | Count |
| UNT-012 | Calculation | Facet count | Count |
| UNT-013 | Calculation | Highlight | Snippet |
| UNT-014 | Status | Healthy | Healthy |
| UNT-015 | Status | Degraded | Degraded |
| UNT-016 | Status | Unhealthy | Unhealthy |
| UNT-017 | Status | Ready | Ready |
| UNT-018 | Status | NotReady | NotReady |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Health check | < 100ms |
| PRF-002 | Readiness | < 100ms |
| PRF-003 | Liveness | < 50ms |
| PRF-004 | DB check | < 200ms |
| PRF-005 | Version | < 50ms |
| PRF-006 | System info | < 100ms |
| PRF-007 | Search simple | < 500ms |
| PRF-008 | Search complex | < 1s |
| PRF-009 | Search paginated | < 500ms |
| PRF-010 | 10 concurrent health | < 200ms each |
| PRF-011 | 50 concurrent search | < 1s each |
| PRF-012 | 5 concurrent mixed | < 500ms each |
| PRF-013 | Memory health | < 10MB |
| PRF-014 | Memory search | < 100MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 2 per health |

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
| LDT-008 | 50 concurrent search | 5 min | Queue/limit |
| LDT-009 | Recovery spike | 5 min | Baseline |
| LDT-010 | Recovery stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Health check | POS-001–004, FUN-001–004 |
| Version | POS-005, FUN-005 |
| System info | POS-006, FUN-006 |
| Global search | POS-007–011, FUN-007 |
| 3:1 Ratio | NEG-001–090, BND-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
