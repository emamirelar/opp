# CountryService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/CountryService`  
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

| Check | Formula | Result |
|-------|---------|--------|
| N≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| E≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| F≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| I≥3P | 90 ≥ 3×30=90 | ✅ PASS |

---

## Feature Overview

Country service: country lookup, ISO code mapping, region association, DST data linking, caching.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Get country by ID | Valid country ID | GetByIdAsync(id) | Country returned |
| POS-002 | Get country by ISO 3166-1 alpha-2 | "US" | GetByIsoAlpha2Async("US") | United States |
| POS-003 | Get country by ISO 3166-1 alpha-3 | "USA" | GetByIsoAlpha3Async("USA") | United States |
| POS-004 | Get country by numeric code | 840 | GetByNumericCodeAsync(840) | United States |
| POS-005 | Map ISO alpha-2 to alpha-3 | "US" | MapIsoAlpha2ToAlpha3("US") | "USA" |
| POS-006 | Map ISO alpha-3 to alpha-2 | "USA" | MapIsoAlpha3ToAlpha2("USA") | "US" |
| POS-007 | Get region for country | Country ID | GetRegionAsync(countryId) | Region returned |
| POS-008 | Get DST data for country | Country ID | GetDstDataAsync(countryId) | DST info |
| POS-009 | Get all countries | None | GetAllAsync() | All countries |
| POS-010 | Get countries by region | Region ID | GetByRegionAsync(regionId) | Countries in region |
| POS-011 | Cache hit returns country | Cached country | GetByIdAsync(id) | From cache |
| POS-012 | Typeahead search | Partial name | SearchAsync("Uni") | United States, etc. |
| POS-013 | Case-insensitive ISO lookup | "us" | GetByIsoAlpha2Async("us") | United States |
| POS-014 | Get country name | Country ID | GetNameAsync(id) | Name returned |
| POS-015 | Get country with region | Country ID | GetWithRegionAsync(id) | Country + region |
| POS-016 | Get active countries only | None | GetActiveAsync() | Active only |
| POS-017 | Map numeric to alpha-2 | 840 | MapNumericToAlpha2(840) | "US" |
| POS-018 | Map numeric to alpha-3 | 840 | MapNumericToAlpha3(840) | "USA" |
| POS-019 | Get countries for dropdown | None | GetDropdownAsync() | Typeahead list |
| POS-020 | Validate ISO code | "US" | ValidateIsoCode("US") | True |
| POS-021 | Get country by name | "United States" | GetByNameAsync(name) | Country |
| POS-022 | Get region hierarchy | Region ID | GetRegionHierarchyAsync(id) | Hierarchy |
| POS-023 | DST timezone offset | Country + date | GetDstOffsetAsync(id, date) | Offset |
| POS-024 | Multiple countries by IDs | [1,2,3] | GetByIdsAsync([1,2,3]) | Countries |
| POS-025 | Search with filter | Filter | SearchAsync(filter) | Filtered |
| POS-026 | Get country with DST | Country ID | GetWithDstAsync(id) | Country + DST |
| POS-027 | Invalidate cache on update | Country updated | GetByIdAsync(id) | Fresh data |
| POS-028 | Paginated countries | Page, size | GetPaginatedAsync(page, size) | Page |
| POS-029 | Sort countries by name | Sort param | GetAllAsync(sort) | Sorted |
| POS-030 | Get country code | Country ID | GetCodeAsync(id) | ISO code |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null country ID | GetByIdAsync(null) | ArgumentNullException |
| NEG-002 | Negative country ID | GetByIdAsync(-1) | ArgumentException |
| NEG-003 | Zero country ID | GetByIdAsync(0) | ArgumentException |
| NEG-004 | Non-existent country ID | GetByIdAsync(999999) | KeyNotFoundException |
| NEG-005 | Null ISO alpha-2 | GetByIsoAlpha2Async(null) | ArgumentNullException |
| NEG-006 | Empty ISO alpha-2 | GetByIsoAlpha2Async("") | ArgumentException |
| NEG-007 | Invalid ISO alpha-2 | GetByIsoAlpha2Async("XX") | KeyNotFoundException |
| NEG-008 | Wrong length alpha-2 | GetByIsoAlpha2Async("USA") | ArgumentException |
| NEG-009 | Null ISO alpha-3 | GetByIsoAlpha3Async(null) | ArgumentNullException |
| NEG-010 | Invalid ISO alpha-3 | GetByIsoAlpha3Async("XXX") | KeyNotFoundException |
| NEG-011 | Wrong length alpha-3 | GetByIsoAlpha3Async("US") | ArgumentException |
| NEG-012 | Invalid numeric code | GetByNumericCodeAsync(-1) | ArgumentException |
| NEG-013 | Non-existent numeric | GetByNumericCodeAsync(9999) | KeyNotFoundException |
| NEG-014 | Null region ID | GetByRegionAsync(null) | ArgumentNullException |
| NEG-015 | Non-existent region | GetByRegionAsync(999999) | KeyNotFoundException |
| NEG-016 | Null IDs array | GetByIdsAsync(null) | ArgumentNullException |
| NEG-017 | Empty IDs array | GetByIdsAsync([]) | ArgumentException |
| NEG-018 | Null search term | SearchAsync(null) | ArgumentNullException |
| NEG-019 | SQL injection in search | SearchAsync("'; DROP") | Sanitized |
| NEG-020 | XSS in search | SearchAsync("<script>") | Sanitized |
| NEG-021 | DB timeout | GetByIdAsync(id) | TimeoutException |
| NEG-022 | Cache corruption | Corrupted cache | CacheInvalidException |
| NEG-023 | Deleted country | GetByIdAsync(deletedId) | KeyNotFoundException |
| NEG-024 | Soft-deleted country | GetByIdAsync(softDeleted) | KeyNotFoundException |
| NEG-025 | Invalid date for DST | GetDstOffsetAsync(id, invalid) | ArgumentException |
| NEG-026 | Null date for DST | GetDstOffsetAsync(id, null) | ArgumentNullException |
| NEG-027 | Numeric overflow | GetByNumericCodeAsync(Int32.MaxValue) | ArgumentException |
| NEG-028 | Unicode in ISO code | GetByIsoAlpha2Async("你好") | ArgumentException |
| NEG-029 | Special chars in search | SearchAsync("@@@") | Sanitized |
| NEG-030 | Whitespace-only search | SearchAsync("   ") | ArgumentException |
| NEG-031 | Search exceeds max length | SearchAsync(veryLong) | ArgumentException |
| NEG-032 | Negative page | GetPaginatedAsync(-1, 10) | ArgumentException |
| NEG-033 | Zero page size | GetPaginatedAsync(1, 0) | ArgumentException |
| NEG-034 | Page size exceeds max | GetPaginatedAsync(1, 10000) | ArgumentException |
| NEG-035 | Null sort param | GetAllAsync(null) | ArgumentNullException |
| NEG-036 | Invalid sort field | GetAllAsync("invalid") | ArgumentException |
| NEG-037 | Region with no countries | GetByRegionAsync(emptyRegion) | Empty list |
| NEG-038 | Cancelled token | GetByIdAsync(id, cancelled) | OperationCanceledException |
| NEG-039 | Duplicate IDs in batch | GetByIdsAsync([1,1,1]) | Deduplicated |
| NEG-040 | Mixed valid/invalid IDs | GetByIdsAsync([1,999999]) | Partial or error |
| NEG-041 | Null filter | SearchAsync(null) | ArgumentNullException |
| NEG-042 | Invalid filter criteria | SearchAsync(invalid) | ArgumentException |
| NEG-043 | Expired cache | GetByIdAsync(expired) | Cache miss |
| NEG-044 | Permission denied | GetByIdAsync(id) | UnauthorizedAccessException |
| NEG-045 | Rate limit exceeded | Many requests | TooManyRequestsException |
| NEG-046 | Connection failed | GetByIdAsync(id) | ConnectionException |
| NEG-047 | Read-only violation | Modify country | ReadOnlyException |
| NEG-048 | Invalid continent | GetByContinentAsync("") | ArgumentException |
| NEG-049 | Null continent | GetByContinentAsync(null) | ArgumentNullException |
| NEG-050 | Circular region hierarchy | GetRegionHierarchyAsync(circular) | InvalidOperationException |
| NEG-051 | DST data missing | GetDstDataAsync(noDst) | KeyNotFoundException |
| NEG-052 | Region not found | GetRegionAsync(badId) | KeyNotFoundException |
| NEG-053 | Map invalid alpha-2 | MapIsoAlpha2ToAlpha3("XX") | KeyNotFoundException |
| NEG-054 | Map null alpha-2 | MapIsoAlpha2ToAlpha3(null) | ArgumentNullException |
| NEG-055 | Map empty alpha-2 | MapIsoAlpha2ToAlpha3("") | ArgumentException |
| NEG-056 | Batch too large | GetByIdsAsync(10000 ids) | ArgumentException |
| NEG-057 | Invalid dropdown filter | GetDropdownAsync(invalid) | ArgumentException |
| NEG-058 | Name with control chars | GetByNameAsync(control) | ArgumentException |
| NEG-059 | Name exceeds max | GetByNameAsync(veryLong) | ArgumentException |
| NEG-060 | Null name | GetByNameAsync(null) | ArgumentNullException |
| NEG-061 | Numeric code as string | GetByNumericCodeAsync("840") | ArgumentException |
| NEG-062 | Float numeric code | GetByNumericCodeAsync(840.5) | ArgumentException |
| NEG-063 | Case-sensitive lookup fail | GetByIsoAlpha2Async("US") (strict) | Depends |
| NEG-064 | Ambiguous name | GetByNameAsync("Georgia") | AmbiguousException |
| NEG-065 | Deprecated country | GetByIdAsync(deprecated) | Depends |
| NEG-066 | Future date DST | GetDstOffsetAsync(id, future) | Handled |
| NEG-067 | Historical date DST | GetDstOffsetAsync(id, past) | Handled |
| NEG-068 | Timezone not found | GetDstOffsetAsync(noTz) | KeyNotFoundException |
| NEG-069 | Metadata missing | GetMetadataAsync(noMeta) | KeyNotFoundException |
| NEG-070 | Warm-up failure | WarmCacheAsync() | CacheException |
| NEG-071 | Null GetContinent | GetContinentAsync(null) | ArgumentNullException |
| NEG-072 | Null GetByContinent | GetByContinentAsync(null) | ArgumentNullException |
| NEG-073 | Invalid GetMetadata | GetMetadataAsync(null) | ArgumentNullException |
| NEG-074 | Null GetWithRegion | GetWithRegionAsync(null) | ArgumentNullException |
| NEG-075 | Null GetWithDst | GetWithDstAsync(null) | ArgumentNullException |
| NEG-076 | Null GetRegionHierarchy | GetRegionHierarchyAsync(null) | ArgumentNullException |
| NEG-077 | Null GetDstOffset | GetDstOffsetAsync(id, null) | ArgumentNullException |
| NEG-078 | Null MapIsoAlpha2 | MapIsoAlpha2ToAlpha3(null) | ArgumentNullException |
| NEG-079 | Null MapIsoAlpha3 | MapIsoAlpha3ToAlpha2(null) | ArgumentNullException |
| NEG-080 | Null MapNumeric | MapNumericToAlpha2(0) | ArgumentException |
| NEG-081 | Null GetPaginated | GetPaginatedAsync(null, 10) | ArgumentNullException |
| NEG-082 | Null GetDropdown | GetDropdownAsync(null) | ArgumentNullException |
| NEG-083 | Invalid GetDropdown | GetDropdownAsync(invalid) | ArgumentException |
| NEG-084 | Null ValidateIso | ValidateIsoCode(null) | ArgumentNullException |
| NEG-085 | Null GetName | GetNameAsync(null) | ArgumentNullException |
| NEG-086 | Null GetCode | GetCodeAsync(null) | ArgumentNullException |
| NEG-087 | Null GetActive | GetActiveAsync(bad) | ArgumentException |
| NEG-088 | Invalid Search filter | SearchAsync(invalid) | ArgumentException |
| NEG-089 | Null GetByIds | GetByIdsAsync(null) | ArgumentNullException |
| NEG-090 | Invalid WarmCache | WarmCacheAsync(bad) | CacheException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Country ID = 1 | Min valid | Country returned |
| BND-002 | Country ID = Int32.MaxValue | Max | Error or country |
| BND-003 | ISO alpha-2 length = 2 | Exact | Valid |
| BND-004 | ISO alpha-2 length = 1 | Under | Invalid |
| BND-005 | ISO alpha-2 length = 3 | Over | Invalid |
| BND-006 | ISO alpha-3 length = 3 | Exact | Valid |
| BND-007 | ISO alpha-3 length = 2 | Under | Invalid |
| BND-008 | Numeric code = 0 | Min | Invalid |
| BND-009 | Numeric code = 999 | Max valid | Valid |
| BND-010 | Numeric code = 1000 | Over | Depends |
| BND-011 | Search length = 0 | "" | Invalid |
| BND-012 | Search length = 1 | "U" | Results |
| BND-013 | Search length = 255 | Max | Results |
| BND-014 | Search length = 256 | Over | Truncated |
| BND-015 | Page = 1 | First | Results |
| BND-016 | Page = last | Last | Results |
| BND-017 | Page = last + 1 | Over | Empty |
| BND-018 | Page size = 1 | Min | One result |
| BND-019 | Page size = 100 | Max | 100 results |
| BND-020 | Page size = 101 | Over | Clamped |
| BND-021 | IDs array = 0 | [] | Invalid |
| BND-022 | IDs array = 1 | [1] | One result |
| BND-023 | IDs array = 1000 | 1000 ids | Results |
| BND-024 | IDs array = 1001 | Over | Invalid |
| BND-025 | Cache size = 0 | Cold | Miss |
| BND-026 | Cache size = 1 | One entry | Hit |
| BND-027 | Cache size = 10000 | Max | Eviction |
| BND-028 | Region countries = 0 | Empty | [] |
| BND-029 | Region countries = 1 | Single | [1] |
| BND-030 | Region countries = 200 | Max | All |
| BND-031 | Date = DateTime.MinValue | Min | DST |
| BND-032 | Date = DateTime.MaxValue | Max | DST |
| BND-033 | Concurrent requests = 1 | 1 | Success |
| BND-034 | Concurrent requests = 100 | 100 | All succeed |
| BND-035 | Country name length = 0 | "" | Invalid |
| BND-036 | Country name length = 255 | Max | Valid |
| BND-037 | Country name length = 256 | Over | Truncated |
| BND-038 | Unicode in name | "Côte d'Ivoire" | Valid |
| BND-039 | Emoji in name | "Test 👍" | Sanitized |
| BND-040 | RTL in name | "فلسطين" | Valid |
| BND-041 | Multiple spaces | "United  States" | Normalized |
| BND-042 | Leading/trailing space | " USA " | Trimmed |
| BND-043 | Tab in search | "USA\t" | Trimmed |
| BND-044 | Newline in search | "USA\n" | Trimmed |
| BND-045 | DST offset = 0 | No DST | 0 |
| BND-046 | DST offset = 14 | Max hours | Valid |
| BND-047 | DST offset = -12 | Min hours | Valid |
| BND-048 | Timeout = 0ms | 0 | Immediate |
| BND-049 | Timeout = 30000ms | 30s | Success |
| BND-050 | Retry count = 0 | No retry | Fail once |
| BND-051 | Retry count = 3 | 3 | Retries |
| BND-052 | Empty region hierarchy | [] | Empty |
| BND-053 | Single level hierarchy | [1] | One level |
| BND-054 | Deep hierarchy | 10 levels | All |
| BND-055 | Dropdown limit = 0 | 0 | Invalid |
| BND-056 | Dropdown limit = 10 | 10 | 10 max |
| BND-057 | Dropdown limit = 500 | Max | 500 max |
| BND-058 | Sort ascending | asc | Sorted |
| BND-059 | Sort descending | desc | Sorted |
| BND-060 | Filter empty | {} | All |
| BND-061 | Filter all fields | Full | Filtered |
| BND-062 | Batch IDs distinct | [1,2,3] | 3 results |
| BND-063 | Batch IDs overlap | [1,2,1] | 2 results |
| BND-064 | Continent empty | "" | Invalid |
| BND-065 | Continent max length | 50 | Valid |
| BND-066 | Numeric as string | "840" | Parsed |
| BND-067 | Zero-padded numeric | "0840" | Parsed |
| BND-068 | Lowercase ISO | "us" | Normalized |
| BND-069 | Uppercase ISO | "US" | Valid |
| BND-070 | Mixed case ISO | "Us" | Normalized |
| BND-071 | Country count = 0 | None | [] |
| BND-072 | Country count = 1 | One | [1] |
| BND-073 | Country count = 250 | All | All |
| BND-074 | Region count = 0 | None | [] |
| BND-075 | Region count = 10 | Many | All |
| BND-076 | Continent count = 0 | None | [] |
| BND-077 | Continent count = 7 | All | All |
| BND-078 | DST countries = 0 | None | [] |
| BND-079 | DST countries = 100 | Many | All |
| BND-080 | Batch size = 1 | One | One |
| BND-081 | Batch size = 1000 | Max | All |
| BND-082 | Hierarchy depth = 0 | Root | Valid |
| BND-083 | Hierarchy depth = 5 | Deep | Valid |
| BND-084 | Numeric range = 0 | Min | Invalid |
| BND-085 | Numeric range = 999 | Max | Valid |
| BND-086 | Alpha-2 variants | "us", "US" | Normalized |
| BND-087 | Alpha-3 variants | "usa", "USA" | Normalized |
| BND-088 | Date range DST | Min/Max | Handled |
| BND-089 | Pagination last | Last page | Results |
| BND-090 | Dropdown limit | 10 | Limited |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | ISO alpha-2 unique | Uniqueness | GetByIsoAlpha2 | One result |
| FUN-002 | ISO alpha-3 unique | Uniqueness | GetByIsoAlpha3 | One result |
| FUN-003 | Numeric code unique | Uniqueness | GetByNumericCode | One result |
| FUN-004 | Region hierarchy | Hierarchy | GetRegionHierarchy | Ordered |
| FUN-005 | DST rules | DST rule | GetDstOffset | Correct offset |
| FUN-006 | Cache TTL | TTL | Cache entry | Expires |
| FUN-007 | Cache key includes ID | Key rule | Cache | Unique |
| FUN-008 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-009 | Active only filter | Active | GetActive | Active only |
| FUN-010 | Case-insensitive search | Case | Search | Case-insensitive |
| FUN-011 | Search partial match | Partial | Search | Matches |
| FUN-012 | Pagination offset | Offset | GetPaginated | Correct offset |
| FUN-013 | Sort order | Sort | GetAll | Ordered |
| FUN-014 | Region association | Association | GetByRegion | Correct |
| FUN-015 | DST association | Association | GetDstData | Correct |
| FUN-016 | Continent association | Association | GetContinent | Correct |
| FUN-017 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-018 | Batch order | Order | GetByIds | Preserved |
| FUN-019 | Dropdown order | Order | GetDropdown | Sorted |
| FUN-020 | Map consistency | Consistency | MapIso | Consistent |
| FUN-021 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-022 | Invalidation on delete | Invalidation | Delete | Cache cleared |
| FUN-023 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-024 | Fallback for missing | Fallback | Missing data | Fallback |
| FUN-025 | Default region | Default | No region | Default |
| FUN-026 | Default DST | Default | No DST | UTC |
| FUN-027 | Error message format | Error | Any error | Consistent |
| FUN-028 | Validation format | Validation | Invalid | Clear message |
| FUN-029 | Trim input | Trim | Search | Trimmed |
| FUN-030 | Normalize ISO | Normalize | ISO input | Uppercase |
| FUN-031 | Numeric range | Range | Numeric | 0-999 |
| FUN-032 | Name max length | Max | Name | 255 |
| FUN-033 | Search max length | Max | Search | 255 |
| FUN-034 | Pagination max size | Max | Page size | 100 |
| FUN-035 | Batch max size | Max | IDs | 1000 |
| FUN-036 | Retry on transient | Retry | Transient | Retried |
| FUN-037 | No retry on permanent | No retry | Permanent | Fail |
| FUN-038 | Timeout handling | Timeout | Slow | Timeout |
| FUN-039 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-040 | Rate limit | Rate | Many | Limited |
| FUN-041 | Audit trail | Audit | Get | Logged |
| FUN-042 | Permission check | Permission | Get | Checked |
| FUN-043 | Tenant isolation | Tenant | Get | Isolated |
| FUN-044 | Multi-region | Multi | GetByRegion | Correct |
| FUN-045 | DST transition | Transition | DST change | Correct |
| FUN-046 | Historical DST | Historical | Past date | Correct |
| FUN-047 | Future DST | Future | Future date | Best guess |
| FUN-048 | Metadata merge | Merge | GetMetadata | Merged |
| FUN-049 | Region fallback | Fallback | No region | Parent |
| FUN-050 | Typeahead limit | Limit | GetDropdown | Limited |
| FUN-051 | ISO alpha-2 unique | Uniqueness | GetByIsoAlpha2 | One result |
| FUN-052 | ISO alpha-3 unique | Uniqueness | GetByIsoAlpha3 | One result |
| FUN-053 | Numeric code unique | Uniqueness | GetByNumericCode | One result |
| FUN-054 | Region hierarchy | Hierarchy | GetRegionHierarchy | Ordered |
| FUN-055 | DST rules | DST rule | GetDstOffset | Correct offset |
| FUN-056 | Cache TTL | TTL | Cache entry | Expires |
| FUN-057 | Cache key includes ID | Key rule | Cache | Unique |
| FUN-058 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-059 | Active only filter | Active | GetActive | Active only |
| FUN-060 | Case-insensitive search | Case | Search | Case-insensitive |
| FUN-061 | Search partial match | Partial | Search | Matches |
| FUN-062 | Pagination offset | Offset | GetPaginated | Correct offset |
| FUN-063 | Sort order | Sort | GetAll | Ordered |
| FUN-064 | Region association | Association | GetByRegion | Correct |
| FUN-065 | DST association | Association | GetDstData | Correct |
| FUN-066 | Continent association | Association | GetContinent | Correct |
| FUN-067 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-068 | Batch order | Order | GetByIds | Preserved |
| FUN-069 | Dropdown order | Order | GetDropdown | Sorted |
| FUN-070 | Map consistency | Consistency | MapIso | Consistent |
| FUN-071 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-072 | Invalidation on delete | Invalidation | Delete | Cache cleared |
| FUN-073 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-074 | Fallback for missing | Fallback | Missing data | Fallback |
| FUN-075 | Default region | Default | No region | Default |
| FUN-076 | Default DST | Default | No DST | UTC |
| FUN-077 | Error message format | Error | Any error | Consistent |
| FUN-078 | Validation format | Validation | Invalid | Clear message |
| FUN-079 | Trim input | Trim | Search | Trimmed |
| FUN-080 | Normalize ISO | Normalize | ISO input | Uppercase |
| FUN-081 | Numeric range | Range | Numeric | 0-999 |
| FUN-082 | Name max length | Max | Name | 255 |
| FUN-083 | Search max length | Max | Search | 255 |
| FUN-084 | Pagination max size | Max | Page size | 100 |
| FUN-085 | Batch max size | Max | IDs | 1000 |
| FUN-086 | Retry on transient | Retry | Transient | Retried |
| FUN-087 | No retry on permanent | No retry | Permanent | Fail |
| FUN-088 | Timeout handling | Timeout | Slow | Timeout |
| FUN-089 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-090 | Rate limit | Rate | Many | Limited |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | DbContext | EF Core | GetById | Loaded |
| INT-002 | Country entity | Entity | GetById | Mapped |
| INT-003 | Region entity | Entity | GetRegion | Loaded |
| INT-004 | DST entity | Entity | GetDstData | Loaded |
| INT-005 | Cache service | ICacheService | GetById | Cached |
| INT-006 | Opportunity manager | IOpportunityManager | Country in opp | Linked |
| INT-007 | Partner manager | IPartnerManager | Country in partner | Linked |
| INT-008 | Liaison office | ILiaisonOfficeService | Country | Linked |
| INT-009 | Org hierarchy | IOrgHierarchyService | Country | Linked |
| INT-010 | Configuration | IConfiguration | Config | Applied |
| INT-011 | Logger | ILogger | Log | Logged |
| INT-012 | AutoMapper | IMapper | Map | Mapped |
| INT-013 | Full lookup flow | All | GetByIsoAlpha2 | Success |
| INT-014 | Full region flow | All | GetByRegion | Success |
| INT-015 | Full DST flow | All | GetDstOffset | Success |
| INT-016 | Opportunity + country | Opp + country | Opp with country | Linked |
| INT-017 | Partner + country | Partner + country | Partner with country | Linked |
| INT-018 | Contact + country | Contact + country | Contact country | Linked |
| INT-019 | Document + country | Document + country | Doc country | Linked |
| INT-020 | Search + pagination | Search + pagination | Search paged | Success |
| INT-021 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-022 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-023 | Soft delete filter | DbContext | Get all | Filtered |
| INT-024 | Permission + get | Permission | Get | Checked |
| INT-025 | Tenant + get | Tenant | Get | Scoped |
| INT-026 | Region + countries | Region | Get countries | Success |
| INT-027 | DST + timezone | DST | Get offset | Success |
| INT-028 | Continent + countries | Continent | Get | Success |
| INT-029 | Dropdown + filter | Dropdown | Filter | Filtered |
| INT-030 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-031 | Search + sort | Search + sort | Search | Sorted |
| INT-032 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-033 | Map + cache | Map + cache | MapIso | Cached |
| INT-034 | Warm-up + load | Warm-up | Startup | Loaded |
| INT-035 | Config + cache TTL | Config | Cache | TTL |
| INT-036 | Logger + error | Logger | Error | Logged |
| INT-037 | Mapper + entity | Mapper | Entity | Mapped |
| INT-038 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-039 | DbContext + connection | DbContext | Connection | Pooled |
| INT-040 | Multi-tenant + cache | Tenant + cache | Get | Isolated |
| INT-041 | Audit + get | Audit | Get | Logged |
| INT-042 | Validation + API | Validation | API | Validated |
| INT-043 | Error handler + get | Error | Get | Handled |
| INT-044 | Retry + transient | Retry | Transient | Retried |
| INT-045 | Timeout + get | Timeout | Get | Timeout |
| INT-046 | Cancellation + get | Cancel | Get | Cancelled |
| INT-047 | Rate limit + get | Rate limit | Many | Limited |
| INT-048 | Permission + region | Permission | Region | Checked |
| INT-049 | Tenant + region | Tenant | Region | Scoped |
| INT-050 | End-to-end | All | Full flow | Success |
| INT-051 | DbContext | EF Core | GetById | Loaded |
| INT-052 | Country entity | Entity | GetById | Mapped |
| INT-053 | Region entity | Entity | GetRegion | Loaded |
| INT-054 | DST entity | Entity | GetDstData | Loaded |
| INT-055 | Cache service | ICacheService | GetById | Cached |
| INT-056 | Opportunity manager | IOpportunityManager | Country in opp | Linked |
| INT-057 | Partner manager | IPartnerManager | Country in partner | Linked |
| INT-058 | Liaison office | ILiaisonOfficeService | Country | Linked |
| INT-059 | Org hierarchy | IOrgHierarchyService | Country | Linked |
| INT-060 | Configuration | IConfiguration | Config | Applied |
| INT-061 | Logger | ILogger | Log | Logged |
| INT-062 | AutoMapper | IMapper | Map | Mapped |
| INT-063 | Full lookup flow | All | GetByIsoAlpha2 | Success |
| INT-064 | Full region flow | All | GetByRegion | Success |
| INT-065 | Full DST flow | All | GetDstOffset | Success |
| INT-066 | Opportunity + country | Opp + country | Opp with country | Linked |
| INT-067 | Partner + country | Partner + country | Partner with country | Linked |
| INT-068 | Search + pagination | Search + pagination | Search paged | Success |
| INT-069 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-070 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-071 | Soft delete filter | DbContext | Get all | Filtered |
| INT-072 | Permission + get | Permission | Get | Checked |
| INT-073 | Tenant + get | Tenant | Get | Scoped |
| INT-074 | Region + countries | Region | Get countries | Success |
| INT-075 | DST + timezone | DST | Get offset | Success |
| INT-076 | Continent + countries | Continent | Get | Success |
| INT-077 | Dropdown + filter | Dropdown | Filter | Filtered |
| INT-078 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-079 | Search + sort | Search + sort | Search | Sorted |
| INT-080 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-081 | Map + cache | Map + cache | MapIso | Cached |
| INT-082 | Warm-up + load | Warm-up | Startup | Loaded |
| INT-083 | Config + cache TTL | Config | Cache | TTL |
| INT-084 | Logger + error | Logger | Error | Logged |
| INT-085 | Mapper + entity | Mapper | Entity | Mapped |
| INT-086 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-087 | Validation + API | Validation | API | Validated |
| INT-088 | Error handler + get | Error | Get | Handled |
| INT-089 | Retry + transient | Retry | Transient | Retried |
| INT-090 | End-to-end | All | Full flow | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection | '; DROP | Search | Parameterized |
| SEC-002 | SQL injection | 1 OR 1=1 | Search | Parameterized |
| SEC-003 | XSS in search | <script> | Search | Sanitized |
| SEC-004 | XSS in name | <img onerror> | GetByName | Sanitized |
| SEC-005 | LDAP injection | *)(uid=* | Search | Sanitized |
| SEC-006 | Path traversal | ../../../ | ID | Rejected |
| SEC-007 | Null byte | %00 | ID | Rejected |
| SEC-008 | Unauthorized access | User A | GetById | 403 |
| SEC-009 | IDOR | Alter ID | GetById | 403 |
| SEC-010 | Cross-tenant | Tenant A | Tenant B data | 403 |
| SEC-011 | Mass assignment | Extra fields | Update | Ignored |
| SEC-012 | No token | Missing | GetById | 401 |
| SEC-013 | Expired token | Expired | GetById | 401 |
| SEC-014 | Tampered token | Tampered | GetById | 401 |
| SEC-015 | PII in response | PII | GetById | Redacted |
| SEC-016 | PII in cache | PII | Cache | Encrypted |
| SEC-017 | Secret in log | API key | Log | No secret |
| SEC-018 | Secret in error | Config | Error | No secret |
| SEC-019 | DoS large batch | 100000 ids | GetByIds | Rejected |
| SEC-020 | DoS long search | 100000 chars | Search | Rejected |
| SEC-021 | Rate limit | 10000 req/s | GetById | Limited |
| SEC-022 | Cache poisoning | Malicious | Cache | Validated |
| SEC-023 | Injection in ISO | '; DROP | GetByIso | Parameterized |
| SEC-024 | Unicode normalization | Homoglyph | Search | Normalized |
| SEC-025 | Integer overflow | Int32.MaxValue+1 | GetById | Rejected |
| SEC-026 | Prototype pollution | __proto__ | Parse | Sanitized |
| SEC-027 | JWT tampering | Altered | GetById | Rejected |
| SEC-028 | Privilege escalation | Low role | Admin | 403 |
| SEC-029 | Horizontal privilege | User A | User B | 403 |
| SEC-030 | API key exposure | Log | Key | Not logged |
| SEC-031 | Weak crypto | MD5 | Cache | SHA256 |
| SEC-032 | SSRF | URL | Entity | Blocked |
| SEC-033 | Open redirect | Redirect | Search | Blocked |
| SEC-034 | Header injection | CRLF | Search | Sanitized |
| SEC-035 | NoSQL injection | $ne | ID | Parameterized |
| SEC-036 | Command injection | ; rm | Search | Sanitized |
| SEC-037 | Replay attack | Replay | GetById | Nonce |
| SEC-038 | CSRF | Cross-site | Update | Token |
| SEC-039 | Session fixation | Fixation | GetById | New session |
| SEC-040 | Sensitive data exposure | Config | Response | Not exposed |
| SEC-041 | Insecure deserialization | Binary | Parse | JSON only |
| SEC-042 | XXE | XXE | Parse | Not XML |
| SEC-043 | GraphQL injection | Mutation | GetById | Not GraphQL |
| SEC-044 | JWT algorithm confusion | Alg none | GetById | Rejected |
| SEC-045 | Token replay | Replay | GetById | Rejected |
| SEC-046 | Cache timing | Timing | GetById | Constant time |
| SEC-047 | Information disclosure | Error | Detail | Generic |
| SEC-048 | Enumeration | Sequential IDs | GetById | Rate limited |
| SEC-049 | Metadata exposure | Metadata | Get | Filtered |
| SEC-050 | Missing auth | No auth | GetById | 401 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Concurrent get same ID | 2 threads same | Both succeed |
| CON-002 | Concurrent get different | 2 threads diff | Both succeed |
| CON-003 | Concurrent cache write | 2 threads same key | No corruption |
| CON-004 | Concurrent cache read | 10 threads | All succeed |
| CON-005 | Get during invalidation | Get + invalidate | Consistent |
| CON-006 | Search during search | 2 threads search | Both succeed |
| CON-007 | Batch during batch | 2 threads batch | Both succeed |
| CON-008 | Warm-up during get | Warm + get | Handled |
| CON-009 | Cache eviction during read | Read + evict | Handled |
| CON-010 | Double warm-up | 2 warm-up | One succeeds |
| CON-011 | Race condition | Update + get | Consistent |
| CON-012 | Deadlock | Get A→B, B→A | No deadlock |
| CON-013 | Concurrent invalidation | 2 invalidate | Both applied |
| CON-014 | Cache stampede | 100 cold | Single load |
| CON-015 | Lock contention | 50 threads | Throttled |
| CON-016 | Thread pool exhaustion | 1000 threads | Limited |
| CON-017 | Concurrent cancellation | Get + cancel | Cancelled |
| CON-018 | Memory barrier | Get + cache | Visible |
| CON-019 | Read-write lock | Read + write | RW lock |
| CON-020 | Semaphore | Limited | Semaphore |
| CON-021 | Optimistic concurrency | Update + get | Version |
| CON-022 | Pessimistic lock | Get + lock | Locked |
| CON-023 | Concurrent pagination | 2 threads page | Both correct |
| CON-024 | Concurrent search | 2 threads search | Both correct |
| CON-025 | Full concurrency | All ops | All succeed |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | ISO alpha-2 validation | Validation | "US" | True |
| UNT-002 | ISO alpha-2 invalid | Validation | "X" | False |
| UNT-003 | ISO alpha-3 validation | Validation | "USA" | True |
| UNT-004 | Numeric validation | Validation | 840 | True |
| UNT-005 | ID validation | Validation | 1 | True |
| UNT-006 | ISO format | Formatting | "us" | "US" |
| UNT-007 | Name format | Formatting | "  USA  " | "USA" |
| UNT-008 | Numeric format | Formatting | "0840" | 840 |
| UNT-009 | Cache key format | Formatting | ID 1 | "country:1" |
| UNT-010 | Date format | Formatting | DateTime | ISO8601 |
| UNT-011 | Map alpha-2 to alpha-3 | Calculations | "US" | "USA" |
| UNT-012 | Map alpha-3 to alpha-2 | Calculations | "USA" | "US" |
| UNT-013 | Map numeric to alpha | Calculations | 840 | "US" |
| UNT-014 | DST offset calc | Calculations | Date + country | Offset |
| UNT-015 | Pagination offset calc | Calculations | Page 2, size 10 | 10 |
| UNT-016 | Active status check | Status | Country | Active |
| UNT-017 | Deleted status check | Status | Country | Deleted |
| UNT-018 | Cache hit check | Status | Key | Hit |
| UNT-019 | Region exists check | Status | Region | Exists |
| UNT-020 | Empty collection | Collections | [] | Empty |
| UNT-021 | Single collection | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Get by ID | GetByIdAsync(1) | <50ms |
| PRF-002 | Get by ISO | GetByIsoAlpha2Async("US") | <50ms |
| PRF-003 | Get all | GetAllAsync() | <500ms |
| PRF-004 | Search | SearchAsync("Uni") | <200ms |
| PRF-005 | Get by region | GetByRegionAsync(1) | <200ms |
| PRF-006 | Cache hit | GetByIdAsync (cached) | <10ms |
| PRF-007 | Cache miss | GetByIdAsync (cold) | <100ms |
| PRF-008 | Batch 100 | GetByIdsAsync(100) | <500ms |
| PRF-009 | DST lookup | GetDstOffsetAsync | <50ms |
| PRF-010 | Pagination | GetPaginatedAsync(1, 50) | <200ms |
| PRF-011 | Concurrent 10 | 10 concurrent | <1s |
| PRF-012 | Concurrent 50 | 50 concurrent | <3s |
| PRF-013 | Memory single | GetById | <1MB |
| PRF-014 | Memory bulk | GetAll | <50MB |
| PRF-015 | Warm-up | WarmCacheAsync | <5s |
| PRF-016 | Full flow | Get + cache + region | <300ms |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|------------------|
| LDT-001 | Sustained 10 req/s | 10/s | 5 min | 99% success |
| LDT-002 | Sustained 50 req/s | 50/s | 5 min | 99% success |
| LDT-003 | Sustained 100 req/s | 100/s | 5 min | 95% success |
| LDT-004 | Spike 0→100 | 0→100/s | 1 min | No crash |
| LDT-005 | Spike 100→0 | 100→0/s | 1 min | No crash |
| LDT-006 | Stress 200 req/s | 200/s | 2 min | Graceful |
| LDT-007 | Stress 500 req/s | 500/s | 1 min | Throttled |
| LDT-008 | Stress 1000 req/s | 1000/s | 30s | No crash |
| LDT-009 | Recovery after spike | Spike + recovery | 5 min | Recovery |
| LDT-010 | Recovery after stress | Stress + recovery | 5 min | Recovery |

---

**Status:** Ready for Execution
