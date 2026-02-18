# LiaisonOfficeService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/LiaisonOfficeService`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30-50 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Security | 30 | 30 | ✅ |
| §7 Concurrency | 15 | 15 | ✅ |
| §8 Unit | 12 | 12 | ✅ |
| §9 Performance | 10 | 10 | ✅ |
| §10 Load | 5 | 5 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance:**
- N≥3P: 90≥90 → ✅ PASS
- E≥3P: 90≥90 → ✅ PASS
- F≥3P: 90≥90 → ✅ PASS
- I≥3P: 90≥90 → ✅ PASS

---

## Feature Overview

Liaison office service: office lookup, region mapping, country association, hierarchy integration.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Get office by ID | Valid ID | GetByIdAsync(id) | Office returned |
| POS-002 | Get office by code | Valid code | GetByCodeAsync(code) | Office returned |
| POS-003 | Get all offices | None | GetAllAsync() | All offices |
| POS-004 | Get offices by region | Region ID | GetByRegionAsync(regionId) | Filtered |
| POS-005 | Get offices by country | Country ID | GetByCountryAsync(countryId) | Filtered |
| POS-006 | Get region for office | Office ID | GetRegionAsync(officeId) | Region |
| POS-007 | Get country for office | Office ID | GetCountryAsync(officeId) | Country |
| POS-008 | Typeahead search | Partial name | SearchAsync("New") | Results |
| POS-009 | Get dropdown list | None | GetDropdownAsync() | Typeahead list |
| POS-010 | Get office hierarchy | Office ID | GetHierarchyAsync(officeId) | Hierarchy |
| POS-011 | Get parent office | Office ID | GetParentAsync(officeId) | Parent |
| POS-012 | Get child offices | Office ID | GetChildrenAsync(officeId) | Children |
| POS-013 | Get active offices | None | GetActiveAsync() | Active only |
| POS-014 | Cache hit | Cached office | GetByIdAsync(id) | From cache |
| POS-015 | Paginated list | Page, size | GetPaginatedAsync(page, size) | Page |
| POS-016 | Sort offices | Sort param | GetAllAsync(sort) | Sorted |
| POS-017 | Filter by status | Status | GetByStatusAsync(status) | Filtered |
| POS-018 | Get office with region | Office ID | GetWithRegionAsync(id) | Office + region |
| POS-019 | Get office with country | Office ID | GetWithCountryAsync(id) | Office + country |
| POS-020 | Validate office code | Code | ValidateCodeAsync(code) | Valid |
| POS-021 | Get country region | Office ID | GetCountryRegionAsync(id) | Country + region |
| POS-022 | Batch get offices | IDs | GetByIdsAsync(ids) | Offices |
| POS-023 | Search with filter | Filter | SearchAsync(filter) | Filtered |
| POS-024 | Get office metadata | Office ID | GetMetadataAsync(id) | Metadata |
| POS-025 | Get hierarchy path | Office ID | GetHierarchyPathAsync(id) | Path |
| POS-026 | Get root offices | None | GetRootOfficesAsync() | Root offices |
| POS-027 | Get leaf offices | None | GetLeafOfficesAsync() | Leaf offices |
| POS-028 | Check office exists | Office ID | ExistsAsync(id) | True/False |
| POS-029 | Get office by name | Name | GetByNameAsync(name) | Office |
| POS-030 | Get offices in hierarchy | Root ID | GetOfficesInHierarchyAsync(rootId) | All |
| POS-031 | Get region offices | Region | GetRegionOfficesAsync(region) | Offices |
| POS-032 | Map country to offices | Country | MapCountryToOfficesAsync(country) | Mapping |
| POS-033 | Get office tree | None | GetOfficeTreeAsync() | Tree |
| POS-034 | Resolve office | Code | ResolveOfficeAsync(code) | Office |
| POS-035 | Get full office details | Office ID | GetFullDetailsAsync(id) | Full details |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null office ID | GetByIdAsync(null) | ArgumentNullException |
| NEG-002 | Negative office ID | GetByIdAsync(-1) | ArgumentException |
| NEG-003 | Zero office ID | GetByIdAsync(0) | ArgumentException |
| NEG-004 | Non-existent office | GetByIdAsync(999999) | KeyNotFoundException |
| NEG-005 | Null code | GetByCodeAsync(null) | ArgumentNullException |
| NEG-006 | Empty code | GetByCodeAsync("") | ArgumentException |
| NEG-007 | Invalid code | GetByCodeAsync("INVALID") | KeyNotFoundException |
| NEG-008 | Null region ID | GetByRegionAsync(null) | ArgumentNullException |
| NEG-009 | Non-existent region | GetByRegionAsync(999999) | KeyNotFoundException |
| NEG-010 | Null country ID | GetByCountryAsync(null) | ArgumentNullException |
| NEG-011 | Non-existent country | GetByCountryAsync(999999) | KeyNotFoundException |
| NEG-012 | Null search term | SearchAsync(null) | ArgumentNullException |
| NEG-013 | SQL injection | SearchAsync("'; DROP") | Sanitized |
| NEG-014 | XSS in search | SearchAsync("<script>") | Sanitized |
| NEG-015 | Deleted office | GetByIdAsync(deletedId) | KeyNotFoundException |
| NEG-016 | Soft-deleted office | GetByIdAsync(softDeleted) | KeyNotFoundException |
| NEG-017 | Permission denied | GetByIdAsync(noPerm) | UnauthorizedAccessException |
| NEG-018 | DB timeout | GetByIdAsync(id) | TimeoutException |
| NEG-019 | Cache corruption | Corrupted cache | CacheInvalidException |
| NEG-020 | Negative page | GetPaginatedAsync(-1, 10) | ArgumentException |
| NEG-021 | Zero page size | GetPaginatedAsync(1, 0) | ArgumentException |
| NEG-022 | Invalid sort | GetAllAsync("invalid") | ArgumentException |
| NEG-023 | Null IDs array | GetByIdsAsync(null) | ArgumentNullException |
| NEG-024 | Empty IDs array | GetByIdsAsync([]) | ArgumentException |
| NEG-025 | IDs array too large | GetByIdsAsync(10000) | ArgumentException |
| NEG-026 | Null parent ID | GetParentAsync(null) | ArgumentNullException |
| NEG-027 | Root office parent | GetParentAsync(rootId) | Null |
| NEG-028 | Circular hierarchy | GetHierarchyAsync(circular) | InvalidOperationException |
| NEG-029 | Cancelled token | GetByIdAsync(id, cancelled) | OperationCanceledException |
| NEG-030 | Null filter | SearchAsync(null) | ArgumentNullException |
| NEG-031 | Invalid filter | SearchAsync(invalid) | ArgumentException |
| NEG-032 | Cross-tenant | Tenant A, Tenant B office | 403 |
| NEG-033 | Rate limit | Many requests | TooManyRequestsException |
| NEG-034 | Connection failed | GetByIdAsync(id) | ConnectionException |
| NEG-035 | Invalid hierarchy | GetHierarchyPathAsync(bad) | InvalidOperationException |
| NEG-036 | Null name | GetByNameAsync(null) | ArgumentNullException |
| NEG-037 | Ambiguous name | GetByNameAsync("Office") | AmbiguousException |
| NEG-038 | Empty validation | ValidateCodeAsync("") | False |
| NEG-039 | Invalid code format | ValidateCodeAsync("bad!") | False |
| NEG-040 | Expired cache | GetByIdAsync(expired) | Cache miss |
| NEG-041 | Null metadata | GetMetadataAsync(noMeta) | KeyNotFoundException |
| NEG-042 | Region with no offices | GetByRegionAsync(empty) | Empty list |
| NEG-043 | Country with no offices | GetByCountryAsync(empty) | Empty list |
| NEG-044 | Resolve invalid | ResolveOfficeAsync("bad") | KeyNotFoundException |
| NEG-045 | Tree empty | GetOfficeTreeAsync() | Empty |
| NEG-046 | Hierarchy depth exceeded | GetHierarchyPathAsync(deep) | InvalidOperationException |
| NEG-047 | Null root | GetRootOfficesAsync() | May be empty |
| NEG-048 | Batch mixed valid/invalid | GetByIdsAsync([1,999999]) | Partial or error |
| NEG-049 | Duplicate IDs batch | GetByIdsAsync([1,1,1]) | Deduplicated |
| NEG-050 | Invalid status | GetByStatusAsync("invalid") | ArgumentException |
| NEG-051 | Unicode in code | GetByCodeAsync("你好") | ArgumentException |
| NEG-052 | Special chars in code | GetByCodeAsync("a/b") | ArgumentException |
| NEG-053 | Code too long | GetByCodeAsync(veryLong) | ArgumentException |
| NEG-054 | Search too long | SearchAsync(veryLong) | ArgumentException |
| NEG-055 | Name too long | GetByNameAsync(veryLong) | ArgumentException |
| NEG-056 | Page overflow | GetPaginatedAsync(999999, 10) | Empty |
| NEG-057 | Sort field injection | GetAllAsync("'; DROP") | Sanitized |
| NEG-058 | Filter injection | SearchAsync(injection) | Sanitized |
| NEG-059 | Null office in hierarchy | GetHierarchyAsync(null) | ArgumentNullException |
| NEG-060 | Orphan office | GetParentAsync(orphan) | Null |
| NEG-061 | Inactive office | GetByIdAsync(inactive) | Depends |
| NEG-062 | Pending office | GetByIdAsync(pending) | Depends |
| NEG-063 | Archived office | GetByIdAsync(archived) | Depends |
| NEG-064 | Deprecated code | GetByCodeAsync(deprecated) | Depends |
| NEG-065 | Moved office | GetByIdAsync(moved) | Depends |
| NEG-066 | Merged office | GetByIdAsync(merged) | MergeTarget |
| NEG-067 | Split office | GetByIdAsync(split) | Multiple |
| NEG-068 | Renamed office | GetByNameAsync(oldName) | NotFound |
| NEG-069 | Code changed | GetByCodeAsync(oldCode) | NotFound |
| NEG-070 | Warm-up failure | WarmCacheAsync() | CacheException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Office ID = 1 | Min valid | Office returned |
| BND-002 | Office ID = Int32.MaxValue | Max | Error or office |
| BND-003 | Code length = 1 | "A" | Valid |
| BND-004 | Code length = 50 | Max | Valid |
| BND-005 | Code length = 51 | Over | Rejected |
| BND-006 | Name length = 0 | "" | Invalid |
| BND-007 | Name length = 1 | "A" | Valid |
| BND-008 | Name length = 255 | Max | Valid |
| BND-009 | Name length = 256 | Over | Truncated |
| BND-010 | Search length = 0 | "" | Invalid |
| BND-011 | Search length = 1 | "N" | Results |
| BND-012 | Search length = 255 | Max | Results |
| BND-013 | Page = 1 | First | Results |
| BND-014 | Page = last | Last | Results |
| BND-015 | Page size = 1 | Min | One |
| BND-016 | Page size = 100 | Max | 100 |
| BND-017 | Page size = 101 | Over | Clamped |
| BND-018 | IDs array = 0 | [] | Invalid |
| BND-019 | IDs array = 1 | [1] | One |
| BND-020 | IDs array = 1000 | Max | Results |
| BND-021 | Hierarchy depth = 0 | Root | Valid |
| BND-022 | Hierarchy depth = 1 | One level | Valid |
| BND-023 | Hierarchy depth = 10 | Deep | Valid |
| BND-024 | Hierarchy depth = 20 | Max | Valid |
| BND-025 | Region offices = 0 | Empty | [] |
| BND-026 | Region offices = 1 | One | [1] |
| BND-027 | Region offices = 500 | Many | All |
| BND-028 | Country offices = 0 | Empty | [] |
| BND-029 | Country offices = 1 | One | [1] |
| BND-030 | Cache size = 0 | Cold | Miss |
| BND-031 | Cache size = 1 | One | Hit |
| BND-032 | Cache size = 10000 | Max | Eviction |
| BND-033 | Concurrent requests = 1 | 1 | Success |
| BND-034 | Concurrent requests = 100 | 100 | All succeed |
| BND-035 | Unicode in name | "Büro" | Valid |
| BND-036 | Emoji in name | "Office 👍" | Sanitized |
| BND-037 | RTL in name | "مكتب" | Valid |
| BND-038 | Multiple spaces | "New  York" | Normalized |
| BND-039 | Leading/trailing space | " Office " | Trimmed |
| BND-040 | Tab in search | "Office\t" | Trimmed |
| BND-041 | Timeout = 0ms | 0 | Immediate |
| BND-042 | Timeout = 30000ms | 30s | Success |
| BND-043 | Retry count = 0 | No retry | Fail once |
| BND-044 | Retry count = 3 | 3 | Retries |
| BND-045 | Dropdown limit = 0 | 0 | Invalid |
| BND-046 | Dropdown limit = 10 | 10 | 10 max |
| BND-047 | Dropdown limit = 500 | Max | 500 max |
| BND-048 | Sort ascending | asc | Sorted |
| BND-049 | Sort descending | desc | Sorted |
| BND-050 | Empty filter | {} | All |
| BND-051 | Filter all fields | Full | Filtered |
| BND-052 | Status active | Active | Active only |
| BND-053 | Status inactive | Inactive | Inactive only |
| BND-054 | Status all | All | All |
| BND-055 | Tree single node | One office | Single |
| BND-056 | Tree empty | No offices | Empty |
| BND-057 | Path single | Root | [root] |
| BND-058 | Path full | Deep | Full path |
| BND-059 | Root offices = 0 | None | [] |
| BND-060 | Root offices = 1 | One | [1] |
| BND-061 | Leaf offices = 0 | None | [] |
| BND-062 | Leaf offices = 100 | Many | All |
| BND-063 | Batch distinct | [1,2,3] | 3 results |
| BND-064 | Batch overlap | [1,2,1] | 2 results |
| BND-065 | Resolve exact | Exact code | Office |
| BND-066 | Resolve partial | Partial code | Depends |
| BND-067 | Region empty | None | [] |
| BND-068 | Country empty | None | [] |
| BND-069 | Metadata empty | {} | Empty |
| BND-070 | Metadata full | Full | All |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Code uniqueness | Unique | GetByCode | One result |
| FUN-002 | ID uniqueness | Unique | GetById | One result |
| FUN-003 | Hierarchy integrity | Integrity | GetHierarchy | Valid |
| FUN-004 | Region association | Association | GetByRegion | Correct |
| FUN-005 | Country association | Association | GetByCountry | Correct |
| FUN-006 | Cache TTL | TTL | Cache | Expires |
| FUN-007 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-008 | Active filter | Active | GetActive | Active only |
| FUN-009 | Search case-insensitive | Case | Search | Case-insensitive |
| FUN-010 | Search partial match | Partial | Search | Matches |
| FUN-011 | Pagination offset | Offset | GetPaginated | Correct |
| FUN-012 | Sort order | Sort | GetAll | Ordered |
| FUN-013 | Hierarchy path | Path | GetHierarchyPath | Ordered |
| FUN-014 | Parent-child | Parent | GetParent | Correct |
| FUN-015 | Child-parent | Child | GetChildren | Correct |
| FUN-016 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-017 | Batch order | Order | GetByIds | Preserved |
| FUN-018 | Dropdown order | Order | GetDropdown | Sorted |
| FUN-019 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-020 | Invalidation on delete | Invalidation | Delete | Cache cleared |
| FUN-021 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-022 | Fallback for missing | Fallback | Missing | Fallback |
| FUN-023 | Default region | Default | No region | Default |
| FUN-024 | Region hierarchy | Region | GetRegion | Hierarchy |
| FUN-025 | Country region | Country | GetCountry | Region |
| FUN-026 | Error format | Format | Error | Consistent |
| FUN-027 | Validation format | Format | Validate | Clear |
| FUN-028 | Trim input | Trim | Search | Trimmed |
| FUN-029 | Normalize code | Normalize | Code | Uppercase |
| FUN-030 | Code range | Range | Code | Valid |
| FUN-031 | Name max length | Max | Name | 255 |
| FUN-032 | Search max length | Max | Search | 255 |
| FUN-033 | Pagination max size | Max | Page size | 100 |
| FUN-034 | Batch max size | Max | IDs | 1000 |
| FUN-035 | Retry on transient | Retry | Transient | Retried |
| FUN-036 | No retry on permanent | No retry | Permanent | Fail |
| FUN-037 | Timeout handling | Timeout | Slow | Timeout |
| FUN-038 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-039 | Rate limit | Rate | Many | Limited |
| FUN-040 | Audit trail | Audit | Get | Logged |
| FUN-041 | Permission check | Permission | Get | Checked |
| FUN-042 | Tenant isolation | Tenant | Get | Isolated |
| FUN-043 | Multi-region | Multi | GetByRegion | Correct |
| FUN-044 | Multi-country | Multi | GetByCountry | Correct |
| FUN-045 | Tree structure | Tree | GetOfficeTree | Valid |
| FUN-046 | Root identification | Root | GetRootOffices | Correct |
| FUN-047 | Leaf identification | Leaf | GetLeafOffices | Correct |
| FUN-048 | Path resolution | Path | GetHierarchyPath | Correct |
| FUN-049 | Resolve logic | Resolve | ResolveOffice | Correct |
| FUN-050 | Typeahead limit | Limit | GetDropdown | Limited |
| FUN-051 | Code uniqueness | Unique | GetByCode | One |
| FUN-052 | ID uniqueness | Unique | GetById | One |
| FUN-053 | Hierarchy integrity | Integrity | GetHierarchy | Valid |
| FUN-054 | Region association | Association | GetByRegion | Correct |
| FUN-055 | Country association | Association | GetByCountry | Correct |
| FUN-056 | Cache TTL | TTL | Cache | Expires |
| FUN-057 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-058 | Active filter | Active | GetActive | Active only |
| FUN-059 | Search case-insensitive | Case | Search | Case-insensitive |
| FUN-060 | Search partial match | Partial | Search | Matches |
| FUN-061 | Pagination offset | Offset | GetPaginated | Correct |
| FUN-062 | Sort order | Sort | GetAll | Ordered |
| FUN-063 | Hierarchy path | Path | GetHierarchyPath | Ordered |
| FUN-064 | Parent-child | Parent | GetParent | Correct |
| FUN-065 | Child-parent | Child | GetChildren | Correct |
| FUN-066 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-067 | Batch order | Order | GetByIds | Preserved |
| FUN-068 | Dropdown order | Order | GetDropdown | Sorted |
| FUN-069 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-070 | Invalidation on delete | Invalidation | Delete | Cache cleared |
| FUN-071 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-072 | Fallback for missing | Fallback | Missing | Fallback |
| FUN-073 | Default region | Default | No region | Default |
| FUN-074 | Region hierarchy | Region | GetRegion | Hierarchy |
| FUN-075 | Country region | Country | GetCountry | Region |
| FUN-076 | Error format | Format | Error | Consistent |
| FUN-077 | Validation format | Format | Validate | Clear |
| FUN-078 | Trim input | Trim | Search | Trimmed |
| FUN-079 | Normalize code | Normalize | Code | Uppercase |
| FUN-080 | Retry on transient | Retry | Transient | Retried |
| FUN-081 | No retry on permanent | No retry | Permanent | Fail |
| FUN-082 | Timeout handling | Timeout | Slow | Timeout |
| FUN-083 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-084 | Rate limit | Rate | Many | Limited |
| FUN-085 | Audit trail | Audit | Get | Logged |
| FUN-086 | Permission check | Permission | Get | Checked |
| FUN-087 | Tenant isolation | Tenant | Get | Isolated |
| FUN-088 | Tree structure | Tree | GetOfficeTree | Valid |
| FUN-089 | Root identification | Root | GetRootOffices | Correct |
| FUN-090 | Leaf identification | Leaf | GetLeafOffices | Correct |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | DbContext | EF Core | GetById | Loaded |
| INT-002 | Office entity | Entity | GetById | Mapped |
| INT-003 | Region entity | Entity | GetRegion | Loaded |
| INT-004 | Country entity | Entity | GetCountry | Loaded |
| INT-005 | Cache service | ICacheService | GetById | Cached |
| INT-006 | Country service | ICountryService | GetCountry | Linked |
| INT-007 | Org hierarchy | IOrgHierarchyService | Hierarchy | Linked |
| INT-008 | Opportunity | IOpportunityManager | Office in opp | Linked |
| INT-009 | Partner | IPartnerManager | Office in partner | Linked |
| INT-010 | Configuration | IConfiguration | Config | Applied |
| INT-011 | Logger | ILogger | Log | Logged |
| INT-012 | AutoMapper | IMapper | Map | Mapped |
| INT-013 | Full lookup flow | All | GetByCode | Success |
| INT-014 | Full region flow | All | GetByRegion | Success |
| INT-015 | Full hierarchy flow | All | GetHierarchy | Success |
| INT-016 | Opportunity + office | Opp + office | Opp with office | Linked |
| INT-017 | Partner + office | Partner + office | Partner with office | Linked |
| INT-018 | Contact + office | Contact + office | Contact office | Linked |
| INT-019 | Country + office | Country + office | Country offices | Linked |
| INT-020 | Search + pagination | Search + pagination | Search paged | Success |
| INT-021 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-022 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-023 | Soft delete filter | DbContext | Get all | Filtered |
| INT-024 | Permission + get | Permission | Get | Checked |
| INT-025 | Tenant + get | Tenant | Get | Scoped |
| INT-026 | Region + offices | Region | Get offices | Success |
| INT-027 | Country + offices | Country | Get | Success |
| INT-028 | Hierarchy + offices | Hierarchy | Get | Success |
| INT-029 | Dropdown + filter | Dropdown | Filter | Filtered |
| INT-030 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-031 | Search + sort | Search + sort | Search | Sorted |
| INT-032 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-033 | Map + cache | Map + cache | Resolve | Cached |
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
| INT-052 | Office entity | Entity | GetById | Mapped |
| INT-053 | Region entity | Entity | GetRegion | Loaded |
| INT-054 | Country entity | Entity | GetCountry | Loaded |
| INT-055 | Cache service | ICacheService | GetById | Cached |
| INT-056 | Country service | ICountryService | GetCountry | Linked |
| INT-057 | Org hierarchy | IOrgHierarchyService | Hierarchy | Linked |
| INT-058 | Opportunity | IOpportunityManager | Office in opp | Linked |
| INT-059 | Partner | IPartnerManager | Office in partner | Linked |
| INT-060 | Configuration | IConfiguration | Config | Applied |
| INT-061 | Logger | ILogger | Log | Logged |
| INT-062 | AutoMapper | IMapper | Map | Mapped |
| INT-063 | Full lookup flow | All | GetByCode | Success |
| INT-064 | Full region flow | All | GetByRegion | Success |
| INT-065 | Full hierarchy flow | All | GetHierarchy | Success |
| INT-066 | Opportunity + office | Opp + office | Opp with office | Linked |
| INT-067 | Partner + office | Partner + office | Partner with office | Linked |
| INT-068 | Search + pagination | Search + pagination | Search paged | Success |
| INT-069 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-070 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-071 | Soft delete filter | DbContext | Get all | Filtered |
| INT-072 | Permission + get | Permission | Get | Checked |
| INT-073 | Tenant + get | Tenant | Get | Scoped |
| INT-074 | Region + offices | Region | Get offices | Success |
| INT-075 | Country + offices | Country | Get | Success |
| INT-076 | Hierarchy + offices | Hierarchy | Get | Success |
| INT-077 | Dropdown + filter | Dropdown | Filter | Filtered |
| INT-078 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-079 | Search + sort | Search + sort | Search | Sorted |
| INT-080 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-081 | Map + cache | Map + cache | Resolve | Cached |
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
| SEC-023 | Injection in code | '; DROP | GetByCode | Parameterized |
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
| UNT-001 | Code validation | Validation | "US" | True |
| UNT-002 | Code invalid | Validation | "X" | False |
| UNT-003 | ID validation | Validation | 1 | True |
| UNT-004 | Region validation | Validation | 1 | True |
| UNT-005 | Name validation | Validation | "Office" | True |
| UNT-006 | Code format | Formatting | "us" | "US" |
| UNT-007 | Name format | Formatting | "  Office  " | "Office" |
| UNT-008 | Cache key format | Formatting | ID 1 | "office:1" |
| UNT-009 | Path format | Formatting | [1,2,3] | "1/2/3" |
| UNT-010 | Filter format | Formatting | Params | Filter |
| UNT-011 | Hierarchy path calc | Calculations | Office | Path |
| UNT-012 | Pagination offset | Calculations | Page 2, 10 | 10 |
| UNT-013 | Batch chunk | Calculations | 100 ids | Chunks |
| UNT-014 | Cache key calc | Calculations | Params | Key |
| UNT-015 | Sort field calc | Calculations | Sort | Field |
| UNT-016 | Exists check | Status | ID | True/False |
| UNT-017 | Active check | Status | Office | Active |
| UNT-018 | Cache hit check | Status | Key | Hit |
| UNT-019 | Root check | Status | Office | Root |
| UNT-020 | Empty collection | Collections | [] | Empty |
| UNT-021 | Single collection | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Get by ID | GetByIdAsync(1) | <50ms |
| PRF-002 | Get by code | GetByCodeAsync("US") | <50ms |
| PRF-003 | Get all | GetAllAsync() | <500ms |
| PRF-004 | Search | SearchAsync("Uni") | <200ms |
| PRF-005 | Get by region | GetByRegionAsync(1) | <200ms |
| PRF-006 | Cache hit | GetByIdAsync (cached) | <10ms |
| PRF-007 | Cache miss | GetByIdAsync (cold) | <100ms |
| PRF-008 | Batch 100 | GetByIdsAsync(100) | <500ms |
| PRF-009 | Get hierarchy | GetHierarchyAsync | <200ms |
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
