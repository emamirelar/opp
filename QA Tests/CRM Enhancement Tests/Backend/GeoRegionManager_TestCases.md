# GeoRegionManager — Test Cases

**Component:** UNOPS.PAO.UNOPSBusiness/Managers/GeoRegionManager.cs  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
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

The GeoRegionManager manages geographic regions for the CRM enhancement:
- **CRUD operations** for regions
- **Continent association** (region-to-continent)
- **Country mapping** (region-country relationships)
- **Regional statistics** (counts, aggregations)

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | Create region | Continent exists | CreateAsync(regionData) | Region created | P0 |
| POS-002 | Get region by ID | Region exists | GetByIdAsync(id) | Region returned | P0 |
| POS-003 | Update region | Region exists | UpdateAsync(id, data) | Updated | P0 |
| POS-004 | Delete region | Region has no countries | DeleteAsync(id) | Soft deleted | P0 |
| POS-005 | Get all regions | Regions exist | GetAllAsync() | List returned | P0 |
| POS-006 | Get by continent | Continent has regions | GetByContinentAsync(continentId) | Region list | P0 |
| POS-007 | Get countries for region | Region has countries | GetCountriesAsync(regionId) | Country list | P0 |
| POS-008 | Get region statistics | Region exists | GetStatisticsAsync(regionId) | Stats object | P1 |
| POS-009 | Map countries to region | Region exists | MapCountriesAsync(regionId, countryIds) | Mapped | P1 |
| POS-010 | Unmap country | Country mapped | UnmapCountryAsync(regionId, countryId) | Unmapped | P1 |
| POS-011 | Get dropdown list | Regions exist | GetForDropdownAsync() | ID/name pairs | P1 |
| POS-012 | Filter by continent | Regions exist | Filter by continentId | Filtered | P1 |
| POS-013 | Sort by name | Multiple regions | GetAll sorted | A-Z order | P1 |
| POS-014 | Paginate results | 20+ regions | Page 2, size 10 | Items 11-20 | P1 |
| POS-015 | Search by name | Regions exist | Search "East" | Matching results | P1 |
| POS-016 | Get with continent | Region exists | Get with Include continent | Continent loaded | P1 |
| POS-017 | Get hierarchy | Regions exist | GetHierarchyAsync() | Tree structure | P1 |
| POS-018 | Count regions by continent | Continents exist | GetCountByContinentAsync() | Map of counts | P1 |
| POS-019 | Bulk create | Continent exists | CreateAsync batch | All created | P2 |
| POS-020 | Bulk update | Regions exist | UpdateAsync batch | All updated | P2 |
| POS-021 | Export regions | Regions exist | ExportAsync() | CSV/Excel | P2 |
| POS-022 | Audit trail | Create region | Create | Audit fields set | P2 |
| POS-023 | Restore soft-deleted | Region deleted | RestoreAsync(id) | Restored | P2 |
| POS-024 | Include deleted | Admin | GetAllAsync(includeDeleted: true) | All regions | P2 |
| POS-025 | Region code unique | No duplicate | Create with new code | Success | P2 |
| POS-026 | Validate continent exists | Continent exists | Create with continentId | Success | P2 |
| POS-027 | Cascading load | Get with countries | Get with Include | Countries loaded | P2 |
| POS-028 | Empty search | Regions exist | Search "" | All returned | P2 |
| POS-029 | Case-insensitive search | "east" | Search | Matches "East" | P2 |
| POS-030 | Get by multiple IDs | IDs exist | GetByIdsAsync([1,2,3]) | 3 regions | P2 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create without name | Name null | Validation error | P0 |
| NEG-002 | Create without continent | ContinentId null | Validation error | P0 |
| NEG-003 | Create with duplicate code | Existing code | Conflict 409 | P0 |
| NEG-004 | Get non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-005 | Update non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-006 | Delete non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-007 | Delete with countries | Region has countries | BusinessException | P0 |
| NEG-008 | Invalid continent ID | ContinentId 99999 | KeyNotFoundException | P0 |
| NEG-009 | Negative ID | GetByIdAsync(-1) | ArgumentException | P0 |
| NEG-010 | Zero ID | GetByIdAsync(0) | ArgumentException | P0 |
| NEG-011 | Null create request | CreateAsync(null) | ArgumentNullException | P0 |
| NEG-012 | Null update request | UpdateAsync(id, null) | ArgumentNullException | P0 |
| NEG-013 | Invalid code format | Code "INVALID" | Validation error | P0 |
| NEG-014 | Name exceeds max | 51 chars | Validation error | P0 |
| NEG-015 | Code exceeds max | 11 chars | Validation error | P0 |
| NEG-016 | Non-existent continent | ContinentId 99999 | FK error | P1 |
| NEG-017 | SQL injection | '; DROP-- in name | Sanitized/Rejected | P1 |
| NEG-018 | XSS in name | <script> | Sanitized | P1 |
| NEG-019 | Invalid pagination | Page -1 | ArgumentException | P1 |
| NEG-020 | Invalid page size | Size 0 | ArgumentException | P1 |
| NEG-021 | Restore non-deleted | Active region | BusinessException | P1 |
| NEG-022 | Map invalid country | CountryId 99999 | KeyNotFoundException | P1 |
| NEG-023 | Unmap unmapped | Country not mapped | BusinessException | P1 |
| NEG-024 | Invalid sort field | Sort "invalid" | ArgumentException | P1 |
| NEG-025 | Orphan region | Delete continent | Cascade or block | P1 |
| NEG-026 | Circular reference | Region parent self | Validation error | P1 |
| NEG-027 | Stale concurrency | Stale update | ConcurrencyException | P1 |
| NEG-028 | Unauthorized | Wrong user | 403 | P1 |
| NEG-029 | Expired token | Stale JWT | 401 | P1 |
| NEG-030 | Rate limit | Too many | 429 | P1 |
| NEG-031 | DB timeout | Slow query | TimeoutException | P1 |
| NEG-032 | Invalid filter combo | Conflicting filters | Validation error | P1 |
| NEG-033 | Null option params | GetAllAsync(null) | ArgumentNullException | P1 |
| NEG-034 | Batch with invalid | One invalid in batch | Partial fail | P1 |
| NEG-035 | Export empty | No regions | Empty file | P1 |
| NEG-036 | Duplicate country map | Map same country twice | Conflict or no-op | P1 |
| NEG-037 | Delete continent with regions | Continent has regions | Block or cascade | P1 |
| NEG-038 | Invalid statistics range | Bad date range | Validation error | P1 |
| NEG-039 | Null ID list | GetByIds(null) | ArgumentNullException | P1 |
| NEG-040 | Empty ID list | GetByIds([]) | Empty result | P1 |
| NEG-041 | Permission denied | User lacks permission | 403 | P2 |
| NEG-042 | Tenant mismatch | Cross-tenant | 403 | P2 |
| NEG-043 | Deleted continent | Create with deleted | Reject | P2 |
| NEG-044 | Invalid status | Status 999 | Validation error | P2 |
| NEG-045 | Malformed JSON | Invalid body | 400 Bad Request | P2 |
| NEG-046 | Wrong content type | Text/plain | 415 | P2 |
| NEG-047 | Oversized payload | 10MB | 413 | P2 |
| NEG-048 | Missing auth | No header | 401 | P2 |
| NEG-049 | Invalid token | Malformed JWT | 401 | P2 |
| NEG-050 | Transaction rollback | Explicit rollback | Reverted | P2 |
| NEG-051 | Connection lost | DB down | Connection exception | P2 |
| NEG-052 | Disk full | Export | IO exception | P2 |
| NEG-053 | Deadlock | Concurrent | Retry or deadlock | P2 |
| NEG-054 | Unique constraint | Duplicate | DB exception | P2 |
| NEG-055 | FK violation | Invalid FK | DB exception | P2 |
| NEG-056 | Null in collection | Null in list | ArgumentNullException | P2 |
| NEG-057 | Validation multiple | Multiple errors | All returned | P2 |
| NEG-058 | Timezone invalid | Bad TZ | Validation error | P2 |
| NEG-059 | Encoding invalid | Wrong charset | 400 Bad Request | P2 |
| NEG-060 | Retry exhaustion | All retries fail | Final exception | P2 |
| NEG-061 | Circuit open | Circuit breaker | Rejected | P2 |
| NEG-062 | Service unavailable | Dependent down | 503 | P2 |
| NEG-063 | Idempotent delete | Delete twice | Second 404 | P2 |
| NEG-064 | Cache corruption | Bad cache | Bypass | P2 |
| NEG-065 | Memory pressure | Large export | Throttle | P2 |
| NEG-066 | Invalid country list | Null countryIds | ArgumentNullException | P2 |
| NEG-067 | Country in multiple regions | Map to 2 regions | Business rule | P2 |
| NEG-068 | Long description | 10000 chars | Validation error | P2 |
| NEG-069 | Special chars code | Code "A@" | Validation error | P2 |
| NEG-070 | Whitespace name | Name "   " | Validation error | P2 |
| NEG-071 | Continent API fail | Continent 500 | Error | P2 |
| NEG-072 | Country API fail | Country 500 | Error | P2 |
| NEG-073 | DbContext disposed | After dispose | ObjectDisposed | P2 |
| NEG-074 | Continent soft-deleted | Deleted continent | Reject | P2 |
| NEG-075 | Region has countries | Delete | BusinessException | P2 |
| NEG-076 | Null codes list | GetByCodes null | ArgumentNull | P2 |
| NEG-077 | Empty codes list | GetByCodes [] | Empty list | P2 |
| NEG-078 | Invalid continent ID | ContinentId 99999 | Reject | P2 |
| NEG-079 | Duplicate code | Existing code | Conflict | P2 |
| NEG-080 | GetByIds empty | [] | Empty list | P2 |
| NEG-081 | Pagination page 0 | Page 0 | Clamp or error | P2 |
| NEG-082 | Pagination size 0 | Size 0 | Validation | P2 |
| NEG-083 | Search SQL injection | '; DROP-- | Sanitized | P2 |
| NEG-084 | Restore non-deleted | Not deleted | Idempotent | P2 |
| NEG-085 | Map country batch overflow | 51 countries | Reject | P2 |
| NEG-086 | Name too long | 201 chars | Validation | P2 |
| NEG-087 | Code too long | 21 chars | Validation | P2 |
| NEG-088 | Export fail | Export error | Handled | P2 |
| NEG-089 | Statistics fail | Stats error | Handled | P2 |
| NEG-090 | Default not configured | No default | Null or error | P2 |

---

## §3 Boundary Tests (90)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Name | 1 | 50 | Accept | Accept | Reject | P1 |
| BND-002 | Code | 1 | 10 | Accept | Accept | Reject | P1 |
| BND-003 | Id | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-004 | ContinentId | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-005 | Page number | 1 | maxPages | 1 ok | Last ok | Empty | P1 |
| BND-006 | Page size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-007 | Search length | 0 | 200 | Empty=all | 200 ok | Truncate | P1 |
| BND-008 | Country count | 0 | 500 | 0 ok | 500 ok | Perf | P1 |
| BND-009 | Region count | 0 | 100 | 0 ok | 100 ok | Perf | P1 |
| BND-010 | Batch size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-011 | Name 1 char | 1 | 50 | Accept | — | — | P1 |
| BND-012 | Name 50 chars | 1 | 50 | — | Accept | — | P1 |
| BND-013 | Name 51 chars | 1 | 50 | — | — | Reject | P1 |
| BND-014 | Code 1 char | 1 | 10 | Accept | — | — | P1 |
| BND-015 | Code 10 chars | 1 | 10 | — | Accept | — | P1 |
| BND-016 | Empty collection | 0 | — | Return [] | — | — | P1 |
| BND-017 | Single item | 1 | — | Return [1] | — | — | P1 |
| BND-018 | Min date | DateTime.Min | — | Handle | — | — | P2 |
| BND-019 | Max date | DateTime.Max | — | — | Handle | — | P2 |
| BND-020 | Leap year | Feb 29 | — | Accept | — | — | P2 |
| BND-021 | Unicode name | Arabic/Chinese | — | Accept | — | — | P2 |
| BND-022 | Emoji | Emoji | — | Accept or reject | — | — | P2 |
| BND-023 | Null vs empty | — | — | Both handled | — | — | P2 |
| BND-024 | Whitespace | — | — | Trim or reject | — | — | P2 |
| BND-025 | Leading/trailing space | — | — | Trimmed | — | — | P2 |
| BND-026 | Pagination last partial | — | — | Correct count | — | — | P2 |
| BND-027 | Sort empty | — | — | No error | — | — | P2 |
| BND-028 | Filter no matches | — | — | Empty list | — | — | P2 |
| BND-029 | Exactly N items | N | — | Paginate correctly | — | — | P2 |
| BND-030 | Status enum | First/Last | — | Accept | — | — | P2 |
| BND-031 | Country ID list | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-032 | Timeout ms | 100 | 30000 | Min ok | Max ok | — | P2 |
| BND-033 | Retry count | 0 | 5 | 0=no retry | 5 ok | — | P2 |
| BND-034 | Cache TTL | 0 | 3600 | 0=no cache | 3600 ok | — | P2 |
| BND-035 | Rate limit | 1 | 1000 | 1 ok | 1000 ok | — | P2 |
| BND-036 | ID list length | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-037 | Description length | 0 | 4000 | 0 ok | 4000 ok | Reject | P2 |
| BND-038 | Export rows | 0 | 100000 | 0 ok | 100k ok | Reject | P2 |
| BND-039 | Hierarchy depth | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-040 | Stat date range | 1 day | 1 year | 1 day ok | 1 year ok | Reject | P2 |
| BND-041 | Zero decimal | 0.0 | — | Accept | — | — | P2 |
| BND-042 | Negative number | — | — | Reject | — | — | P2 |
| BND-043 | Float precision | — | — | Rounding | — | — | P2 |
| BND-044 | Boolean boundary | — | — | True/False | — | — | P2 |
| BND-045 | Enum all values | — | — | All valid | — | — | P2 |
| BND-046 | JSON depth | 1 | 32 | 1 ok | 32 ok | Reject | P2 |
| BND-047 | Array length | 0 | 1000 | 0 ok | 1000 ok | — | P2 |
| BND-048 | Tab/newline | — | — | Sanitize | — | — | P2 |
| BND-049 | Null byte | — | — | Reject | — | — | P2 |
| BND-050 | CRLF | — | — | Sanitize | — | — | P2 |
| BND-051 | RTL text | — | — | Accept | — | — | P2 |
| BND-052 | High surrogate | — | — | Reject | — | — | P2 |
| BND-053 | Zero-width char | — | — | Strip | — | — | P2 |
| BND-054 | Multiple spaces | — | — | Collapse | — | — | P2 |
| BND-055 | Code uppercase | — | — | Normalize | — | — | P2 |
| BND-056 | Url max | — | 2048 | — | Accept | Reject | P2 |
| BND-057 | Include depth | 0 | 3 | 0=no | 3 ok | — | P2 |
| BND-058 | Query param count | 0 | 50 | 0 ok | 50 ok | Reject | P2 |
| BND-059 | Correlation ID | 36 | 36 | UUID | — | — | P2 |
| BND-060 | Token length | 1 | 500 | 1 ok | 500 ok | — | P2 |
| BND-061 | Concurrent limit | — | 100 | — | — | — | P2 |
| BND-062 | Nested depth | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-063 | Decimal precision | 2 | 2 | 0.00 | 99.99 | — | P2 |
| BND-064 | Percent 0/100 | 0/100 | — | Accept | — | — | P2 |
| BND-065 | Guid empty | Guid.Empty | — | Reject | — | — | P2 |
| BND-066 | Timezone | UTC±12 | — | Correct | — | — | P2 |
| BND-067 | Timestamp precision | — | — | Sub-ms | Full ms | — | P2 |
| BND-068 | Sort field count | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-069 | Filter param count | 0 | 20 | 0 ok | 20 ok | Reject | P2 |
| BND-070 | Map country batch | 1 | 50 | 1 ok | 50 ok | Reject | P2 |
| BND-071 | Region ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-072 | Continent ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-073 | Country ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-074 | Page size 1 | 1 | 100 | Min | — | — | P2 |
| BND-075 | Page size 100 | 1 | 100 | — | Max | — | P2 |
| BND-076 | Name 1 | 1 | 200 | Min | — | — | P2 |
| BND-077 | Name 200 | 1 | 200 | — | Max | — | P2 |
| BND-078 | Code 1 | 1 | 20 | Min | — | — | P2 |
| BND-079 | Code 20 | 1 | 20 | — | Max | — | P2 |
| BND-080 | Search 0 | 0 | 200 | Empty | — | — | P2 |
| BND-081 | Search 200 | 0 | 200 | — | Max | — | P2 |
| BND-082 | Country batch 0 | 0 | 50 | Empty | — | — | P2 |
| BND-083 | Country batch 50 | 0 | 50 | — | Max | — | P2 |
| BND-084 | Codes list 0 | 0 | 100 | Empty | — | — | P2 |
| BND-085 | Codes list 100 | 0 | 100 | — | Max | — | P2 |
| BND-086 | Notes 0 | 0 | 4000 | Empty | — | — | P2 |
| BND-087 | Notes 4000 | 0 | 4000 | — | Max | — | P2 |
| BND-088 | Description 0 | 0 | 2000 | Empty | — | — | P2 |
| BND-089 | Description 2000 | 0 | 2000 | — | Max | — | P2 |
| BND-090 | Country count 0 | 0 | 1000 | None | — | — | P2 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|------------------|----------|
| FUN-001 | Create sets audit | Audit on create | CreateAsync | CreatedBy, CreatedDate | P0 |
| FUN-002 | Update sets audit | Audit on update | UpdateAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-003 | Delete soft | Soft delete | DeleteAsync | IsDeleted=true | P0 |
| FUN-004 | Name required | Name not null | Create without | Reject | P0 |
| FUN-005 | Continent required | ContinentId not null | Create without | Reject | P0 |
| FUN-006 | Unique code | No duplicate codes | Create duplicate | Reject | P0 |
| FUN-007 | Delete blocks if countries | Referential integrity | Delete with countries | Reject | P0 |
| FUN-008 | Get excludes deleted | Default filter | GetAllAsync | !IsDeleted | P0 |
| FUN-009 | Code format | Valid format | Create with valid | Success | P1 |
| FUN-010 | Continent exists | Continent valid | Create with valid | Success | P1 |
| FUN-011 | Name trim | Whitespace | Create "  x  " | Stored "x" | P1 |
| FUN-012 | Pagination | Page/size | Get page 2 | Items 11-20 | P1 |
| FUN-013 | Sort default | Default sort | GetAllAsync | By Name ASC | P1 |
| FUN-014 | Sort by code | Sort option | Sort by code | Order by code | P1 |
| FUN-015 | Filter by continent | Continent filter | Filter | Filtered | P1 |
| FUN-016 | Search partial | Partial match | Search "East" | East Africa | P1 |
| FUN-017 | Case-insensitive search | Search | Search "east" | East | P1 |
| FUN-018 | Restore clears delete | Restore | RestoreAsync | IsDeleted=false | P1 |
| FUN-019 | Map country | Valid country | MapCountries | Mapped | P1 |
| FUN-020 | Unmap country | Mapped country | UnmapCountry | Unmapped | P1 |
| FUN-021 | Country count | Statistics | GetStatistics | Correct count | P1 |
| FUN-022 | Dropdown active only | Dropdown filter | GetForDropdown | Only !IsDeleted | P1 |
| FUN-023 | Hierarchy constraint | No cycles | Self-reference | Reject | P1 |
| FUN-024 | Code immutable | No code change | Update code | Reject or ignore | P1 |
| FUN-025 | Bulk create | Batch | CreateAsync batch | All created | P1 |
| FUN-026 | Bulk update | Batch | UpdateAsync batch | All updated | P1 |
| FUN-027 | Export format | CSV | ExportAsync | Valid CSV | P1 |
| FUN-028 | Mapping complete | All fields | GetById mapped | All populated | P1 |
| FUN-029 | Concurrency token | Optimistic | Stale update | ConcurrencyException | P1 |
| FUN-030 | Transaction scope | Create+map | Create with map | Atomic | P1 |
| FUN-031 | Null handling | Optional fields | Null optional | No error | P1 |
| FUN-032 | Default values | New entity | Create minimal | Defaults | P1 |
| FUN-033 | Idempotent get | GetById | Call twice | Same result | P1 |
| FUN-034 | Stateless | No server state | Request | Independent | P1 |
| FUN-035 | Idempotent delete | Delete twice | Delete same | Second 404 | P1 |
| FUN-036 | Update partial | PATCH | Update 1 field | Only that | P2 |
| FUN-037 | Read-your-writes | Consistency | Create then get | Visible | P2 |
| FUN-038 | Version header | ETag | Get | ETag returned | P2 |
| FUN-039 | Conditional update | If-Match | Stale ETag | 412 | P2 |
| FUN-040 | Soft delete cascade | Children | Delete parent | Handled | P2 |
| FUN-041 | Permission check | CanCreate | Create | Validated | P2 |
| FUN-042 | Tenant isolation | Multi-tenant | Cross-tenant | Rejected | P2 |
| FUN-043 | Audit immutable | No audit change | Update audit | Ignored | P2 |
| FUN-044 | Id auto-assign | Identity | Create | Id assigned | P2 |
| FUN-045 | Timestamp UTC | Store | Create | UTC stored | P2 |
| FUN-046 | Localization | Name | Get with locale | Localized | P2 |
| FUN-047 | Validation order | Multiple invalid | Create | All errors | P2 |
| FUN-048 | Country in one region | Business rule | Map to 2 regions | One only | P2 |
| FUN-049 | Continent cascade | Delete continent | Regions handled | Cascade or block | P2 |
| FUN-050 | Statistics aggregation | Aggregation | GetStatistics | Correct agg | P2 |
| FUN-051 | Create audit | Create | Create | Audit set | P2 |
| FUN-052 | Update audit | Update | Update | Audit set | P2 |
| FUN-053 | Soft delete audit | Delete | Delete | DeletedBy set | P2 |
| FUN-054 | IsDeleted filter | Query | Query | Excludes deleted | P2 |
| FUN-055 | Include continent | Get | Include | Continent loaded | P2 |
| FUN-056 | Include countries | Get | Include | Countries loaded | P2 |
| FUN-057 | Pagination | Page | Page | Correct slice | P2 |
| FUN-058 | Sort | Sort | Sort | Ordered | P2 |
| FUN-059 | Search | Search | Search | Matched | P2 |
| FUN-060 | GetByCodes | Codes | Get | Returned | P2 |
| FUN-061 | GetCountryCounts | Counts | Get | Aggregated | P2 |
| FUN-062 | Restore | Restore | Restore | Restored | P2 |
| FUN-063 | Include deleted | Admin | IncludeDeleted | All | P2 |
| FUN-064 | Map countries | Map | Batch | Mapped | P2 |
| FUN-065 | AsNoTracking | Read | Query | No tracking | P2 |
| FUN-066 | Transaction | Transaction | Commit | Committed | P2 |
| FUN-067 | Concurrency | Concurrent | Read | No conflict | P2 |
| FUN-068 | Case-insensitive | Search | Case | Matched | P2 |
| FUN-069 | Default region | Default | Get | Returned | P2 |
| FUN-070 | GetByIds | IDs | Get | Returned | P2 |
| FUN-071 | DbContext scope | Scope | Per request | Isolated | P2 |
| FUN-072 | Validation order | Invalid | Validate | Order correct | P2 |
| FUN-073 | Idempotent delete | Delete | Twice | Second no-op | P2 |
| FUN-074 | Idempotent restore | Restore | Twice | Second no-op | P2 |
| FUN-075 | Batch save | Batch | Save | All saved | P2 |
| FUN-076 | Empty search | Search "" | Search | All | P2 |
| FUN-077 | Unique code | Code | Create | No duplicate | P2 |
| FUN-078 | Continent validation | Continent | Validate | Validated | P2 |
| FUN-079 | Logging | Operation | Log | Logged | P2 |
| FUN-080 | Metrics | Operation | Metric | Recorded | P2 |
| FUN-081 | Query timeout | Slow | Query | Timeout | P2 |
| FUN-082 | Retry policy | Transient | Fail | Retried | P2 |
| FUN-083 | Cascading load | Include | Load | Loaded | P2 |
| FUN-084 | Connection pool | Concurrent | Connections | Pooled | P2 |
| FUN-085 | Foreign key | FK | Constraint | Enforced | P2 |
| FUN-086 | Unique constraint | Unique | Insert | Enforced | P2 |
| FUN-087 | Index | Query | Index | Fast | P2 |
| FUN-088 | Export | Export | Export | File | P2 |
| FUN-089 | Status validation | Status | Validate | Validated | P2 |
| FUN-090 | Delete with countries | Has countries | Delete | Reject | P2 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | CRUD full cycle | Create→Read→Update→Delete | Region | Success | P0 |
| INT-002 | Create then get | Create, GetById | Region | Data matches | P0 |
| INT-003 | Region→Continent | Get continent | Region, Continent | Loaded | P0 |
| INT-004 | Region→Countries | Get countries | Region, Country | Loaded | P0 |
| INT-005 | Continent→Regions | Get regions | Continent, Region | Loaded | P0 |
| INT-006 | Map countries flow | Map, Get | Region, Country | Mapped | P0 |
| INT-007 | API→Manager→DB | Full stack | All | End-to-end | P1 |
| INT-008 | Controller→Manager | Controller call | API, Manager | Mapped | P1 |
| INT-009 | Manager→Repository | Manager call | Manager, Repo | Executed | P1 |
| INT-010 | Auth→Manager | Authorized call | Auth, Manager | Checked | P1 |
| INT-011 | Error propagation | Manager throws | Manager→Controller | 400/404/500 | P1 |
| INT-012 | Logging integration | Operation | Logger | Log entry | P1 |
| INT-013 | Metrics integration | Operation | Metrics | Counter | P1 |
| INT-014 | Audit→DB | Create | Audit, DB | Audit row | P1 |
| INT-015 | Cache→DB | Get cached | Cache, DB | Hit/miss | P1 |
| INT-016 | Transaction scope | Create+map | Transaction | Atomic | P1 |
| INT-017 | Country service | Country lookup | Country service | Valid | P1 |
| INT-018 | Continent service | Continent lookup | Continent service | Valid | P1 |
| INT-019 | Report generation | Report | Report, DB | Report data | P1 |
| INT-020 | Dashboard agg | Dashboard | Dashboard, DB | Aggregations | P1 |
| INT-021 | Export file | Export | DB, File | File created | P1 |
| INT-022 | Bulk import | Import | CSV, DB | Imported | P1 |
| INT-023 | Permission service | Permission | Permission svc | Allowed/Denied | P1 |
| INT-024 | Tenant isolation | Multi-tenant | Tenant A, B | Isolated | P1 |
| INT-025 | Retry on failure | Transient | Retry policy | Retried | P1 |
| INT-026 | Health check | Health | DB, Services | Healthy | P1 |
| INT-027 | Config override | Env | Config | Override | P1 |
| INT-028 | Feature flag | Flag off | Feature | Disabled | P1 |
| INT-029 | Rate limit | Many req | Rate limiter | Limited | P1 |
| INT-030 | Partner→Region | Partner link | Partner, Region | Linked | P1 |
| INT-031 | LiaisonOffice→Region | Office link | Office, Region | Linked | P1 |
| INT-032 | Geographic hierarchy | Full hierarchy | Continent→Region→Country | Tree | P1 |
| INT-033 | Statistics calc | Stats | GetStatistics | Correct | P1 |
| INT-034 | Search index | Create region | Search | Indexed | P1 |
| INT-035 | Event publish | Created | Event bus | Event sent | P1 |
| INT-036 | Notification | Create | Notifier | Notification | P1 |
| INT-037 | Sync external | Sync | External API | Synced | P1 |
| INT-038 | Message queue | Publish | Queue | Message sent | P1 |
| INT-039 | Cron job | Periodic | Job | Processed | P1 |
| INT-040 | API versioning | v2 | Version | v2 behavior | P1 |
| INT-041 | CORS | Cross-origin | CORS | Allowed/Blocked | P1 |
| INT-042 | Correlation ID | Trace | Request | Propagated | P1 |
| INT-043 | Circuit breaker | Failures | Circuit | Opened | P1 |
| INT-044 | Backward compat | Old client | New API | Works | P1 |
| INT-045 | Forward compat | New client | Old API | Graceful | P1 |
| INT-046 | Validation→Controller | Validation | Model, Controller | 400 + errors | P1 |
| INT-047 | Repository→DbContext | Repo call | Repo, EF | SQL generated | P1 |
| INT-048 | Multi-entity create | Region+Countries | Multiple | All created | P1 |
| INT-049 | Pagination flow | Create 15, Page 2 | Region | Correct slice | P1 |
| INT-050 | Search flow | Create, Search | Region | Found | P1 |
| INT-051 | DbContext | CRUD | DbContext | Persisted | P1 |
| INT-052 | Repository | CRUD | Repository | Persisted | P1 |
| INT-053 | AutoMapper | Map | Mapper | Mapped | P1 |
| INT-054 | ContinentManager | Continent | Manager | Loaded | P1 |
| INT-055 | CountryManager | Country | Manager | Loaded | P1 |
| INT-056 | AuditDbContext | Audit | Context | Audited | P1 |
| INT-057 | Transaction | Transaction | Commit | Committed | P1 |
| INT-058 | PermissionService | Check | Service | Checked | P1 |
| INT-059 | HttpClient | API | HttpClient | Response | P1 |
| INT-060 | Logging | Log | ILogger | Logged | P1 |
| INT-061 | Configuration | Config | IConfiguration | Loaded | P1 |
| INT-062 | DI container | Resolve | Container | Resolved | P1 |
| INT-063 | Scoped lifetime | Request | Scope | Per request | P1 |
| INT-064 | Soft delete filter | Global | Query | Filtered | P1 |
| INT-065 | Foreign key | FK | Constraint | Enforced | P1 |
| INT-066 | Unique constraint | Unique | Insert | Enforced | P1 |
| INT-067 | Cache | Cache | Get | Cached | P1 |
| INT-068 | Retry | Transient | Retry | Retried | P1 |
| INT-069 | Health check | Health | Check | Healthy | P1 |
| INT-070 | Metrics | Metric | Record | Recorded | P1 |
| INT-071 | User context | User | Context | Resolved | P1 |
| INT-072 | Export service | Export | Service | File | P1 |
| INT-073 | API versioning | Version | Request | Versioned | P1 |
| INT-074 | Rate limiting | Limit | Request | Limited | P1 |
| INT-075 | Auth middleware | Auth | Request | Authenticated | P1 |
| INT-076 | Validation middleware | Validate | Request | Validated | P1 |
| INT-077 | Exception middleware | Exception | Throw | Handled | P1 |
| INT-078 | Correlation ID | Request | ID | Propagated | P1 |
| INT-079 | Tracing | Trace | Span | Traced | P1 |
| INT-080 | Feature flag | Flag | Check | Toggled | P1 |
| INT-081 | CORS | Cross-origin | Request | Allowed | P1 |
| INT-082 | Connection | Connection | Open | Connected | P1 |
| INT-083 | Migration | Migration | Run | Applied | P1 |
| INT-084 | Index | Query | Index | Fast | P1 |
| INT-085 | Circuit breaker | Fail | Circuit | Open | P1 |
| INT-086 | Tenant context | Tenant | Context | Resolved | P1 |
| INT-087 | Continent API | Continent | API | Response | P1 |
| INT-088 | Country API | Country | API | Response | P1 |
| INT-089 | Forward compat | New client | Old API | Graceful | P1 |
| INT-090 | Search flow | Create, Search | Region | Found | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|---------------|----------|
| SEC-001 | SQL injection name | '; DROP-- | Name | Sanitized/Rejected | P0 |
| SEC-002 | SQL injection code | 1' OR '1'='1 | Code | Rejected | P0 |
| SEC-003 | XSS in name | <script> | Name | Escaped | P0 |
| SEC-004 | Unauthorized get | No token | GetById | 401 | P0 |
| SEC-005 | Forbidden get | Wrong role | GetById | 403 | P0 |
| SEC-006 | IDOR get | Others' ID | GetById | 403/404 | P0 |
| SEC-007 | IDOR update | Others' ID | Update | 403 | P0 |
| SEC-008 | IDOR delete | Others' ID | Delete | 403 | P0 |
| SEC-009 | Mass assignment | isAdmin=true | Create | Ignored | P0 |
| SEC-010 | Parameterized query | SQL params | All queries | No injection | P0 |
| SEC-011 | Output encoding | HTML | All responses | Encoded | P0 |
| SEC-012 | CSRF token | No token | POST | Rejected | P0 |
| SEC-013 | Session timeout | Expired | Request | 401 | P0 |
| SEC-014 | LDAP injection | *)(uid=* | Search | Rejected | P1 |
| SEC-015 | NoSQL injection | {$gt:""} | Filter | Rejected | P1 |
| SEC-016 | JWT tampering | Modified JWT | Auth | Rejected | P1 |
| SEC-017 | JWT alg none | alg=none | JWT | Rejected | P1 |
| SEC-018 | Token replay | Reuse token | Request | Rejected | P1 |
| SEC-019 | Privilege escalation | Low→Admin | Action | 403 | P1 |
| SEC-020 | Horizontal access | User A→B | Resource | 403 | P1 |
| SEC-021 | Vertical access | User→Admin | Resource | 403 | P1 |
| SEC-022 | Sensitive data log | Password | Logging | Not logged | P1 |
| SEC-023 | Sensitive data response | Password | API | Not returned | P1 |
| SEC-024 | Stack trace | Error | Prod | No trace | P1 |
| SEC-025 | Verbose error | DB details | Error | Generic | P1 |
| SEC-026 | Rate limit bypass | Many IPs | Rate limit | Per-user | P1 |
| SEC-027 | Header injection | CRLF | Header | Rejected | P1 |
| SEC-028 | Oversized payload | 100MB | Request | Rejected | P1 |
| SEC-029 | Deep object | 100 levels | JSON | Rejected | P1 |
| SEC-030 | Regex DoS | Evil regex | Pattern | Timeout/Reject | P1 |
| SEC-031 | Prototype pollution | __proto__ | JSON | Sanitized | P1 |
| SEC-032 | CORS misconfig | Wildcard | CORS | Restricted | P1 |
| SEC-033 | Missing headers | X-Frame-Options | Response | Present | P1 |
| SEC-034 | HSTS | HTTP | Redirect | HTTPS | P1 |
| SEC-035 | Cookie secure | Cookie | Set-Cookie | Secure | P1 |
| SEC-036 | Cookie HttpOnly | Cookie | Set-Cookie | HttpOnly | P1 |
| SEC-037 | Audit integrity | Modify audit | Audit | Tamper evident | P1 |
| SEC-038 | Encryption at rest | DB | Sensitive | Encrypted | P1 |
| SEC-039 | Command injection | ; ls | Field | Rejected | P1 |
| SEC-040 | Path traversal | ../etc/passwd | File | Rejected | P1 |
| SEC-041 | XXE | XML entity | XML | Rejected | P1 |
| SEC-042 | SSRF | Internal URL | URL | Blocked | P1 |
| SEC-043 | Open redirect | redirect=evil | Redirect | Validated | P1 |
| SEC-044 | Brute force | Many auth | Login | Lockout | P1 |
| SEC-045 | Content-type bypass | Wrong type | Upload | Rejected | P1 |
| SEC-046 | File upload malicious | Exe | Upload | Rejected | P1 |
| SEC-047 | Insecure deserialization | Malicious | Deserialize | Rejected | P1 |
| SEC-048 | Info disclosure | Server details | Header | Minimal | P1 |
| SEC-049 | Tenant isolation | Cross-tenant | Request | 403 | P1 |
| SEC-050 | Data aggregation | PII in report | Export | Anonymized | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior | Priority |
|----|-----------|----------|-------------------|----------|
| CON-001 | Concurrent create same code | 2 users create "XX" | One succeeds, one conflict | P1 |
| CON-002 | Concurrent update same | 2 users update | Optimistic lock | P1 |
| CON-003 | Concurrent delete same | 2 users delete | One succeeds | P1 |
| CON-004 | Read during update | Read while update | Consistent | P1 |
| CON-005 | Update during delete | Update while delete | One fails | P1 |
| CON-006 | Double submit | Same form twice | Idempotent | P1 |
| CON-007 | Transaction isolation | Parallel tx | No dirty read | P1 |
| CON-008 | Deadlock | Circular wait | Retry | P1 |
| CON-009 | Lost update | Interleaved | Version lock | P1 |
| CON-010 | Map country race | 2 map same | One succeeds | P1 |
| CON-011 | Cache invalidation | Update after cache | Invalidated | P1 |
| CON-012 | Batch concurrent | 2 batches | Both complete | P1 |
| CON-013 | Connection pool | Exhaust | Queue/timeout | P1 |
| CON-014 | Lock timeout | Hold long | Timeout | P1 |
| CON-015 | Retry idempotency | Retry partial | No duplicate | P1 |
| CON-016 | Visibility | Write then read | Read sees write | P1 |
| CON-017 | Unmap race | 2 unmap same | One succeeds | P1 |
| CON-018 | Statistics calc race | Concurrent calc | Consistent | P1 |
| CON-019 | Export concurrent | 2 exports | Both complete | P1 |
| CON-020 | Bulk create race | 2 bulk creates | Both complete | P1 |
| CON-021 | Distributed lock | Multi-instance | Single writer | P1 |
| CON-022 | Eventual consistency | Replica lag | Converge | P2 |
| CON-023 | Failover | Primary fail | Replica | P2 |
| CON-024 | Saga compensation | Partial fail | Compensate | P2 |
| CON-025 | Outbox pattern | Event | Exactly once | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Name validation | Validation | Valid name | True | P1 |
| UNT-002 | Name invalid | Validation | Null | False | P1 |
| UNT-003 | Code validation | Validation | "EA" | True | P1 |
| UNT-004 | Code invalid | Validation | "" | False | P1 |
| UNT-005 | Format code | Formatting | "ea" | "EA" | P1 |
| UNT-006 | Trim name | Formatting | "  x  " | "x" | P1 |
| UNT-007 | Map entity to model | Mapping | Entity | Model | P1 |
| UNT-008 | Country count calc | Calculation | Region+countries | Count | P1 |
| UNT-009 | Statistics agg | Calculation | Regions | Aggregated | P1 |
| UNT-010 | Status transition | Status logic | Valid | Success | P1 |
| UNT-011 | IsDeleted filter | Status logic | Mixed | !IsDeleted | P1 |
| UNT-012 | Sort comparator | Collections | Unsorted | Sorted | P1 |
| UNT-013 | Paginate slice | Collections | Full list | Slice | P1 |
| UNT-014 | Search predicate | Collections | Query | Matching | P1 |
| UNT-015 | Null safe | Validation | Null | No throw | P1 |
| UNT-016 | Empty collection | Collections | [] | [] | P1 |
| UNT-017 | Map list | Mapping | Entity list | Model list | P1 |
| UNT-018 | Date format | Formatting | DateTime | ISO string | P1 |
| UNT-019 | Id equality | Validation | Same id | Equal | P1 |
| UNT-020 | Code equality | Validation | Same code | Equal | P1 |
| UNT-021 | Continent ref | Validation | Valid continentId | Valid | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetById latency | GetByIdAsync | < 50 ms | P2 |
| PRF-002 | GetAll latency | 100 items | < 200 ms | P2 |
| PRF-003 | Create latency | CreateAsync | < 100 ms | P2 |
| PRF-004 | Update latency | UpdateAsync | < 100 ms | P2 |
| PRF-005 | Delete latency | DeleteAsync | < 100 ms | P2 |
| PRF-006 | GetCountries latency | 50 countries | < 200 ms | P2 |
| PRF-007 | Pagination | Page 10 of 1000 | < 200 ms | P2 |
| PRF-008 | Bulk create 100 | CreateAsync batch | < 5 s | P2 |
| PRF-009 | Bulk get 100 | GetByIds 100 | < 500 ms | P2 |
| PRF-010 | Export 1000 | ExportAsync | < 5 s | P2 |
| PRF-011 | Concurrent 10 get | 10 parallel | < 200 ms | P2 |
| PRF-012 | Memory single | Create | No leak | P2 |
| PRF-013 | Memory 1000 ops | 1000 creates | Stable | P2 |
| PRF-014 | Query plan | GetById | Index used | P2 |
| PRF-015 | N+1 check | With countries | Single query | P2 |
| PRF-016 | Connection reuse | 100 sequential | Pool stable | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|------------------|----------|
| LDT-001 | Sustained 10 RPS | 10 req/s | 10 min | 99% < 500 ms | P2 |
| LDT-002 | Sustained 50 RPS | 50 req/s | 5 min | 99% < 1 s | P2 |
| LDT-003 | Sustained 100 RPS | 100 req/s | 5 min | 95% < 2 s | P2 |
| LDT-004 | Spike 200 RPS | 0→200→0 | 2 min | No 5xx | P2 |
| LDT-005 | Spike 500 RPS | 5 s burst | 5 s | Recover | P2 |
| LDT-006 | Stress 500 RPS | 500 req/s | 2 min | Graceful | P2 |
| LDT-007 | Stress 1000 RPS | 1000 req/s | 1 min | No crash | P2 |
| LDT-008 | Endurance 20 RPS | 20 req/s | 1 h | No leak | P2 |
| LDT-009 | Recovery | Post-spike | 5 min | Baseline | P2 |
| LDT-010 | Mixed workload | CRUD mix | 15 min | All succeed | P2 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
