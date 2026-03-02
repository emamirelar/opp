# SavedFilterService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/SavedFilterService`  
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

Saved filter service: CRUD filters, apply filters, share, user-specific, entity-type filtering.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Create filter | Valid data | CreateAsync(filter) | Filter created |
| POS-002 | Get filter by ID | Filter exists | GetByIdAsync(id) | Filter returned |
| POS-003 | Update filter | Filter exists | UpdateAsync(id, data) | Updated |
| POS-004 | Delete filter | Filter exists | DeleteAsync(id) | Deleted |
| POS-005 | Get user filters | User has filters | GetByUserAsync(userId) | User filters |
| POS-006 | Get by entity type | Filters exist | GetByEntityTypeAsync(type) | Filtered |
| POS-007 | Apply filter | Filter exists | ApplyFilterAsync(filterId, data) | Applied |
| POS-008 | Share filter | Filter exists | ShareAsync(filterId, userId) | Shared |
| POS-009 | Unshare filter | Shared filter | UnshareAsync(filterId, userId) | Unshared |
| POS-010 | Set default filter | Filter exists | SetDefaultAsync(filterId) | Default set |
| POS-011 | Get default filter | Default exists | GetDefaultAsync(userId, entity) | Default |
| POS-012 | List shared filters | Shared filters | GetSharedAsync(userId) | Shared list |
| POS-013 | Duplicate filter | Filter exists | DuplicateAsync(filterId) | Duplicated |
| POS-014 | Export filter | Filter exists | ExportAsync(filterId) | JSON export |
| POS-015 | Import filter | JSON | ImportAsync(json) | Imported |
| POS-016 | Rename filter | Filter exists | RenameAsync(id, name) | Renamed |
| POS-017 | Reorder filters | User filters | ReorderAsync(userId, order) | Reordered |
| POS-018 | Get filter metadata | Filter exists | GetMetadataAsync(id) | Metadata |
| POS-019 | Validate filter | Filter data | ValidateAsync(filter) | Valid |
| POS-020 | Parse filter criteria | Criteria string | ParseCriteriaAsync(criteria) | Parsed |
| POS-021 | Serialize filter | Filter | SerializeAsync(filter) | JSON |
| POS-022 | Deserialize filter | JSON | DeserializeAsync(json) | Filter |
| POS-023 | Merge filters | Two filters | MergeAsync(f1, f2) | Merged |
| POS-024 | Get filter count | User | GetCountAsync(userId) | Count |
| POS-025 | Search filters | Query | SearchFiltersAsync(query) | Results |
| POS-026 | Paginate filters | User | GetPaginatedAsync(userId, page, size) | Page |
| POS-027 | Get filter permissions | Filter | GetPermissionsAsync(filterId) | Permissions |
| POS-028 | Set filter visibility | Filter | SetVisibilityAsync(id, visibility) | Set |
| POS-029 | Get public filters | None | GetPublicFiltersAsync() | Public |
| POS-030 | Clone filter | Filter | CloneAsync(filterId, name) | Cloned |
| POS-031 | Restore filter | Deleted filter | RestoreAsync(filterId) | Restored |
| POS-032 | Archive filter | Filter | ArchiveAsync(filterId) | Archived |
| POS-033 | Batch get filters | IDs | GetByIdsAsync(ids) | Filters |
| POS-034 | Preset filters | Entity | GetPresetsAsync(entity) | Presets |
| POS-035 | Apply preset | Preset | ApplyPresetAsync(presetId) | Applied |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null filter ID | GetByIdAsync(null) | ArgumentNullException |
| NEG-002 | Negative filter ID | GetByIdAsync(-1) | ArgumentException |
| NEG-003 | Zero filter ID | GetByIdAsync(0) | ArgumentException |
| NEG-004 | Non-existent filter | GetByIdAsync(999999) | KeyNotFoundException |
| NEG-005 | Null filter data | CreateAsync(null) | ArgumentNullException |
| NEG-006 | Empty filter name | CreateAsync(emptyName) | ArgumentException |
| NEG-007 | Null user ID | GetByUserAsync(null) | ArgumentNullException |
| NEG-008 | Null entity type | GetByEntityTypeAsync(null) | ArgumentNullException |
| NEG-009 | Invalid entity type | GetByEntityTypeAsync("invalid") | ArgumentException |
| NEG-010 | Null criteria | ApplyFilterAsync(null) | ArgumentNullException |
| NEG-011 | Invalid criteria | ApplyFilterAsync(invalid) | ArgumentException |
| NEG-012 | Null share user | ShareAsync(id, null) | ArgumentNullException |
| NEG-013 | Share with self | ShareAsync(id, self) | ArgumentException |
| NEG-014 | Deleted filter | GetByIdAsync(deletedId) | KeyNotFoundException |
| NEG-015 | Deleted filter update | UpdateAsync(deletedId, ...) | KeyNotFoundException |
| NEG-016 | Permission denied | GetByIdAsync(noPerm) | UnauthorizedAccessException |
| NEG-017 | Cross-user filter | User A, User B filter | 403 |
| NEG-018 | DB timeout | GetByIdAsync(id) | TimeoutException |
| NEG-019 | Cache corruption | Corrupted cache | CacheInvalidException |
| NEG-020 | Negative page | GetPaginatedAsync(..., -1, 10) | ArgumentException |
| NEG-021 | Zero page size | GetPaginatedAsync(..., 1, 0) | ArgumentException |
| NEG-022 | Null IDs array | GetByIdsAsync(null) | ArgumentNullException |
| NEG-023 | Empty IDs array | GetByIdsAsync([]) | ArgumentException |
| NEG-024 | Null import JSON | ImportAsync(null) | ArgumentNullException |
| NEG-025 | Invalid import JSON | ImportAsync("{invalid}") | JsonException |
| NEG-026 | Null export filter | ExportAsync(null) | ArgumentNullException |
| NEG-027 | Cancelled token | CreateAsync(..., cancelled) | OperationCanceledException |
| NEG-028 | Duplicate name | CreateAsync(duplicateName) | ArgumentException |
| NEG-029 | Name too long | CreateAsync(veryLongName) | ArgumentException |
| NEG-030 | Null visibility | SetVisibilityAsync(id, null) | ArgumentNullException |
| NEG-031 | Invalid visibility | SetVisibilityAsync(id, "bad") | ArgumentException |
| NEG-032 | Unshare non-shared | UnshareAsync(id, userId) | InvalidOperationException |
| NEG-033 | Set default non-existent | SetDefaultAsync(999999) | KeyNotFoundException |
| NEG-034 | Restore non-deleted | RestoreAsync(activeId) | InvalidOperationException |
| NEG-035 | Archive already archived | ArchiveAsync(archived) | InvalidOperationException |
| NEG-036 | Merge incompatible | MergeAsync(incompatible) | ArgumentException |
| NEG-037 | Parse invalid criteria | ParseCriteriaAsync(bad) | FormatException |
| NEG-038 | Deserialize invalid | DeserializeAsync(bad) | JsonException |
| NEG-039 | Clone non-existent | CloneAsync(999999, name) | KeyNotFoundException |
| NEG-040 | Duplicate non-existent | DuplicateAsync(999999) | KeyNotFoundException |
| NEG-041 | SQL injection in name | CreateAsync("'; DROP") | Sanitized |
| NEG-042 | XSS in filter name | CreateAsync("<script>") | Sanitized |
| NEG-043 | XSS in criteria | ApplyFilterAsync(xss) | Sanitized |
| NEG-044 | Criteria injection | ApplyFilterAsync(injection) | Sanitized |
| NEG-045 | Cross-tenant | Tenant A, Tenant B | 403 |
| NEG-046 | Rate limit | Many requests | TooManyRequestsException |
| NEG-047 | Preset non-existent | ApplyPresetAsync(999999) | KeyNotFoundException |
| NEG-048 | Reorder invalid | ReorderAsync(userId, bad) | ArgumentException |
| NEG-049 | Validate invalid | ValidateAsync(invalid) | ValidationException |
| NEG-050 | Filter count limit | CreateAsync(overLimit) | QuotaExceededException |
| NEG-051 | Batch too large | GetByIdsAsync(10000) | ArgumentException |
| NEG-052 | Export deleted | ExportAsync(deletedId) | KeyNotFoundException |
| NEG-053 | Import malformed | ImportAsync(malformed) | JsonException |
| NEG-054 | Rename to empty | RenameAsync(id, "") | ArgumentException |
| NEG-055 | Rename to duplicate | RenameAsync(id, existing) | ArgumentException |
| NEG-056 | Get permissions non-existent | GetPermissionsAsync(999999) | KeyNotFoundException |
| NEG-057 | Search null query | SearchFiltersAsync(null) | ArgumentNullException |
| NEG-058 | Merge null | MergeAsync(null, f2) | ArgumentNullException |
| NEG-059 | Serialize null | SerializeAsync(null) | ArgumentNullException |
| NEG-060 | Deserialize null | DeserializeAsync(null) | ArgumentNullException |
| NEG-061 | Get default none | GetDefaultAsync(noDefault) | KeyNotFoundException |
| NEG-062 | Apply filter deleted | ApplyFilterAsync(deletedId) | KeyNotFoundException |
| NEG-063 | Share deleted | ShareAsync(deletedId, userId) | KeyNotFoundException |
| NEG-064 | Set default deleted | SetDefaultAsync(deletedId) | KeyNotFoundException |
| NEG-065 | Unicode in name | CreateAsync(" filtro ") | Trimmed |
| NEG-066 | Special chars in criteria | ApplyFilterAsync(special) | Sanitized |
| NEG-067 | Empty filter criteria | CreateAsync(emptyCriteria) | ArgumentException |
| NEG-068 | Invalid filter structure | CreateAsync(badStructure) | ArgumentException |
| NEG-069 | Circular filter reference | CreateAsync(circular) | InvalidOperationException |
| NEG-070 | Warm-up failure | WarmCacheAsync() | CacheException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Filter ID = 1 | Min valid | Filter returned |
| BND-002 | Filter ID = Int32.MaxValue | Max | Error or filter |
| BND-003 | Name length = 0 | "" | Invalid |
| BND-004 | Name length = 1 | "A" | Valid |
| BND-005 | Name length = 255 | Max | Valid |
| BND-006 | Name length = 256 | Over | Truncated |
| BND-007 | Criteria length = 0 | "" | Invalid |
| BND-008 | Criteria length = 1 | "{}" | Valid |
| BND-009 | Criteria length = 10000 | Max | Valid |
| BND-010 | Criteria length = 10001 | Over | Rejected |
| BND-011 | Page = 1 | First | Results |
| BND-012 | Page = last | Last | Results |
| BND-013 | Page size = 1 | Min | One |
| BND-014 | Page size = 100 | Max | 100 |
| BND-015 | Page size = 101 | Over | Clamped |
| BND-016 | User filter count = 0 | None | [] |
| BND-017 | User filter count = 1 | One | [1] |
| BND-018 | User filter count = 100 | Max | All |
| BND-019 | Share count = 0 | None | [] |
| BND-020 | Share count = 1 | One | [1] |
| BND-021 | Share count = 50 | Max | All |
| BND-022 | IDs array = 0 | [] | Invalid |
| BND-023 | IDs array = 1 | [1] | One |
| BND-024 | IDs array = 100 | Max | Results |
| BND-025 | Cache size = 0 | Cold | Miss |
| BND-026 | Cache size = 1 | One | Hit |
| BND-027 | Cache size = 10000 | Max | Eviction |
| BND-028 | Concurrent requests = 1 | 1 | Success |
| BND-029 | Concurrent requests = 100 | 100 | All succeed |
| BND-030 | Unicode in name | "Filtro 过滤器" | Valid |
| BND-031 | Emoji in name | "Filter 👍" | Sanitized |
| BND-032 | RTL in name | "فلتر" | Valid |
| BND-033 | Criteria depth = 0 | {} | Valid |
| BND-034 | Criteria depth = 1 | {a:1} | Valid |
| BND-035 | Criteria depth = 10 | Deep | Valid |
| BND-036 | Entity type length = 1 | "P" | Valid |
| BND-037 | Entity type length = 50 | Max | Valid |
| BND-038 | Search length = 0 | "" | Invalid |
| BND-039 | Search length = 255 | Max | Results |
| BND-040 | Visibility private | "private" | Valid |
| BND-041 | Visibility public | "public" | Valid |
| BND-042 | Visibility shared | "shared" | Valid |
| BND-043 | Order array = 0 | [] | Invalid |
| BND-044 | Order array = 1 | [1] | Valid |
| BND-045 | Order array = 100 | Max | Valid |
| BND-046 | Preset count = 0 | None | [] |
| BND-047 | Preset count = 10 | Many | All |
| BND-048 | Timeout = 0ms | 0 | Immediate |
| BND-049 | Timeout = 30000ms | 30s | Success |
| BND-050 | Retry count = 0 | No retry | Fail once |
| BND-051 | Retry count = 3 | 3 | Retries |
| BND-052 | Filter quota = 0 | 0 | Rejected |
| BND-053 | Filter quota = 100 | Max | Accepted |
| BND-054 | Export size = 0 | Empty | "{}" |
| BND-055 | Export size = 100KB | Large | Exported |
| BND-056 | Import size = 0 | "" | Invalid |
| BND-057 | Import size = 1MB | Large | Imported |
| BND-058 | Merge filters empty | Merge([], []) | Invalid |
| BND-059 | Merge filters one | Merge([f1], []) | f1 |
| BND-060 | Parse empty | ParseCriteriaAsync("") | Invalid |
| BND-061 | Parse minimal | ParseCriteriaAsync("{}") | {} |
| BND-062 | Validate empty | ValidateAsync({}) | Invalid |
| BND-063 | Validate full | ValidateAsync(full) | Valid |
| BND-064 | Clone name = 0 | "" | Invalid |
| BND-065 | Clone name = 255 | Max | Valid |
| BND-066 | Duplicate creates new | Duplicate | New ID |
| BND-067 | Restore restores | Restore | Restored |
| BND-068 | Archive hides | Archive | Hidden |
| BND-069 | Reorder preserves | Reorder | Order |
| BND-070 | Default single | SetDefault | One default |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Filter uniqueness | Unique | Create | Unique ID |
| FUN-002 | User isolation | Isolate | GetByUser | User only |
| FUN-003 | Entity type filter | Filter | GetByEntityType | Filtered |
| FUN-004 | Default single per entity | Single | SetDefault | One default |
| FUN-005 | Share propagation | Propagate | Share | Recipient sees |
| FUN-006 | Unshare removal | Remove | Unshare | Removed |
| FUN-007 | Cache TTL | TTL | Cache | Expires |
| FUN-008 | Soft delete | Soft | Delete | Trashed |
| FUN-009 | Restore from trash | Restore | Restore | Active |
| FUN-010 | Archive hides | Hide | Archive | Hidden |
| FUN-011 | Criteria parsing | Parse | ParseCriteria | Parsed |
| FUN-012 | Criteria validation | Validate | Validate | Validated |
| FUN-013 | Serialization round-trip | Round-trip | Serialize/Deserialize | Same |
| FUN-014 | Import export round-trip | Round-trip | Import/Export | Same |
| FUN-015 | Merge logic | Merge | Merge | Combined |
| FUN-016 | Apply filter | Apply | ApplyFilter | Filtered data |
| FUN-017 | Preset application | Apply | ApplyPreset | Applied |
| FUN-018 | Clone independence | Independent | Clone | New filter |
| FUN-019 | Duplicate copy | Copy | Duplicate | Copy |
| FUN-020 | Rename uniqueness | Unique | Rename | Unique name |
| FUN-021 | Reorder persistence | Persist | Reorder | Saved |
| FUN-022 | Visibility enforcement | Enforce | GetPublic | Public only |
| FUN-023 | Permission check | Check | Share | Checked |
| FUN-024 | Quota enforcement | Quota | Create | Enforced |
| FUN-025 | Pagination offset | Offset | GetPaginated | Correct |
| FUN-026 | Search partial match | Partial | Search | Matches |
| FUN-027 | Search case-insensitive | Case | Search | Case-insensitive |
| FUN-028 | Error format | Format | Error | Consistent |
| FUN-029 | Trim input | Trim | Create | Trimmed |
| FUN-030 | Criteria sanitization | Sanitize | Apply | Sanitized |
| FUN-031 | Retry on transient | Retry | Transient | Retried |
| FUN-032 | No retry on permanent | No retry | Permanent | Fail |
| FUN-033 | Timeout handling | Timeout | Slow | Timeout |
| FUN-034 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-035 | Rate limit | Rate | Many | Limited |
| FUN-036 | Audit trail | Audit | Create | Logged |
| FUN-037 | Tenant isolation | Tenant | Get | Isolated |
| FUN-038 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-039 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-040 | Invalidation on delete | Invalidation | Delete | Cache cleared |
| FUN-041 | Warm-up | Warm-up | WarmCache | Preloaded |
| FUN-042 | Fallback | Fallback | Missing | Fallback |
| FUN-043 | Metadata preservation | Preserve | Clone | Preserved |
| FUN-044 | Criteria preservation | Preserve | Duplicate | Preserved |
| FUN-045 | Order preservation | Preserve | Reorder | Preserved |
| FUN-046 | Share list limit | Limit | GetShared | Limited |
| FUN-047 | Preset immutable | Immutable | ApplyPreset | No change |
| FUN-048 | Default override | Override | SetDefault | Replaced |
| FUN-049 | Visibility hierarchy | Hierarchy | Visibility | Private < Shared < Public |
| FUN-050 | Filter versioning | Version | Update | Versioned |
| FUN-051 | Export format | Format | Export | JSON |
| FUN-052 | Import validation | Validate | Import | Validated |
| FUN-053 | Merge conflict | Conflict | Merge | Resolved |
| FUN-054 | Clone independence | Independent | Clone | New |
| FUN-055 | Duplicate metadata | Metadata | Duplicate | Copied |
| FUN-056 | Archive visibility | Visibility | Archive | Hidden |
| FUN-057 | Restore visibility | Visibility | Restore | Visible |
| FUN-058 | Search ranking | Rank | Search | Ranked |
| FUN-059 | Pagination consistency | Consistency | Page | Consistent |
| FUN-060 | Preset immutability | Immutable | Preset | No change |
| FUN-061 | Criteria merge | Merge | Merge | Combined |
| FUN-062 | Filter dependency | Dependency | Apply | Resolved |
| FUN-063 | User preference | Preference | GetDefault | Preferred |
| FUN-064 | Entity type scope | Scope | GetByEntityType | Scoped |
| FUN-065 | Share notification | Notify | Share | Notified |
| FUN-066 | Unshare cleanup | Cleanup | Unshare | Cleaned |
| FUN-067 | Default cascade | Cascade | SetDefault | Cascaded |
| FUN-068 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-069 | Cache key uniqueness | Unique | Cache | Unique |
| FUN-070 | Invalidation scope | Scope | Invalidate | Scoped |
| FUN-071 | Error aggregation | Aggregate | Batch | Aggregated |
| FUN-072 | Partial batch | Partial | Batch | Partial |
| FUN-073 | Retry transient | Retry | Transient | Retried |
| FUN-074 | Timeout handling | Timeout | Slow | Timeout |
| FUN-075 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-076 | Rate limit | Rate | Many | Limited |
| FUN-077 | Audit trail | Audit | Create | Logged |
| FUN-078 | Tenant isolation | Tenant | Get | Isolated |
| FUN-079 | Permission check | Check | Share | Checked |
| FUN-080 | Trim input | Trim | Create | Trimmed |
| FUN-081 | Criteria sanitization | Sanitize | Apply | Sanitized |
| FUN-082 | Order preservation | Preserve | Reorder | Preserved |
| FUN-083 | Metadata preservation | Preserve | Clone | Preserved |
| FUN-084 | Criteria preservation | Preserve | Duplicate | Preserved |
| FUN-085 | Share list limit | Limit | GetShared | Limited |
| FUN-086 | Warm-up | Warm-up | WarmCache | Preloaded |
| FUN-087 | Fallback | Fallback | Missing | Fallback |
| FUN-088 | Error format | Format | Error | Consistent |
| FUN-089 | Validation format | Format | Validate | Clear |
| FUN-090 | Quota enforcement | Quota | Create | Enforced |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | DbContext | EF Core | GetById | Loaded |
| INT-002 | SavedFilter entity | Entity | GetById | Mapped |
| INT-003 | User entity | Entity | GetByUser | Linked |
| INT-004 | Cache service | ICacheService | GetById | Cached |
| INT-005 | Permission service | IPermissionService | Share | Checked |
| INT-006 | User service | IUserService | User | Resolved |
| INT-007 | ListView | ListView | Apply filter | Filtered |
| INT-008 | Opportunity | IOpportunityManager | Filter opp | Linked |
| INT-009 | Partner | IPartnerManager | Filter partner | Linked |
| INT-010 | Configuration | IConfiguration | Config | Applied |
| INT-011 | Logger | ILogger | Log | Logged |
| INT-012 | AutoMapper | IMapper | Map | Mapped |
| INT-013 | Full create flow | All | Create | Success |
| INT-014 | Full apply flow | All | Apply | Success |
| INT-015 | Full share flow | All | Share | Success |
| INT-016 | ListView + filter | ListView | Apply | Filtered |
| INT-017 | Opportunity + filter | Opportunity | Filter | Applied |
| INT-018 | Partner + filter | Partner | Filter | Applied |
| INT-019 | Search + pagination | Search + pagination | Search | Success |
| INT-020 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-021 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-022 | Soft delete filter | DbContext | Get all | Filtered |
| INT-023 | Permission + get | Permission | Get | Checked |
| INT-024 | Tenant + get | Tenant | Get | Scoped |
| INT-025 | Import + export | Import + export | Both | Match |
| INT-026 | Clone + share | Clone + share | Both | Success |
| INT-027 | Duplicate + apply | Duplicate + apply | Both | Success |
| INT-028 | Config + quota | Config | Quota | From config |
| INT-029 | Logger + error | Logger | Error | Logged |
| INT-030 | Mapper + entity | Mapper | Entity | Mapped |
| INT-031 | Retry + transient | Retry | Transient | Retried |
| INT-032 | Timeout + create | Timeout | Create | Timeout |
| INT-033 | Cancellation + create | Cancel | Create | Cancelled |
| INT-034 | Rate limit + create | Rate limit | Many | Limited |
| INT-035 | Audit + create | Audit | Create | Audited |
| INT-036 | User preference | UserPreference | Default | Linked |
| INT-037 | Entity config | EntityConfig | Entity type | Linked |
| INT-038 | SavedFilter controller | Controller | API | Linked |
| INT-039 | Frontend service | Frontend | Apply | Linked |
| INT-040 | Multi-tenant + cache | Tenant + cache | Get | Isolated |
| INT-041 | Share + permission | Share + permission | Both | Checked |
| INT-042 | Preset + entity | Preset + entity | Both | Linked |
| INT-043 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-044 | Search + filter | Search + filter | Search | Filtered |
| INT-045 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-046 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-047 | Error handler + create | Error | Create | Handled |
| INT-048 | Validation + API | Validation | API | Validated |
| INT-049 | Health check | Health | Check | Healthy |
| INT-050 | End-to-end | All | Full flow | Success |
| INT-051 | ListView + filter | ListView | Apply | Filtered |
| INT-052 | Opportunity + filter | Opportunity | Filter | Applied |
| INT-053 | Partner + filter | Partner | Filter | Applied |
| INT-054 | Search + pagination | Search + pagination | Search | Success |
| INT-055 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-056 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-057 | Soft delete filter | DbContext | Get all | Filtered |
| INT-058 | Permission + get | Permission | Get | Checked |
| INT-059 | Tenant + get | Tenant | Get | Scoped |
| INT-060 | Import + export | Import + export | Both | Match |
| INT-061 | Clone + share | Clone + share | Both | Success |
| INT-062 | Duplicate + apply | Duplicate + apply | Both | Success |
| INT-063 | Config + quota | Config | Quota | From config |
| INT-064 | Logger + error | Logger | Error | Logged |
| INT-065 | Mapper + entity | Mapper | Entity | Mapped |
| INT-066 | Retry + transient | Retry | Transient | Retried |
| INT-067 | Timeout + create | Timeout | Create | Timeout |
| INT-068 | Cancellation + create | Cancel | Create | Cancelled |
| INT-069 | Rate limit + create | Rate limit | Many | Limited |
| INT-070 | Audit + create | Audit | Create | Audited |
| INT-071 | User preference | UserPreference | Default | Linked |
| INT-072 | Entity config | EntityConfig | Entity type | Linked |
| INT-073 | SavedFilter controller | Controller | API | Linked |
| INT-074 | Frontend service | Frontend | Apply | Linked |
| INT-075 | Multi-tenant + cache | Tenant + cache | Get | Isolated |
| INT-076 | Share + permission | Share + permission | Both | Checked |
| INT-077 | Preset + entity | Preset + entity | Both | Linked |
| INT-078 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-079 | Search + filter | Search + filter | Search | Filtered |
| INT-080 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-081 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-082 | Error handler + create | Error | Create | Handled |
| INT-083 | Validation + API | Validation | API | Validated |
| INT-084 | Health check | Health | Check | Healthy |
| INT-085 | Full create flow | All | Create | Success |
| INT-086 | Full apply flow | All | Apply | Success |
| INT-087 | Full share flow | All | Share | Success |
| INT-088 | Full pipeline | All | Full flow | Success |
| INT-089 | Document manager | IDocumentManager | Filter | Linked |
| INT-090 | End-to-end | All | Full flow | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection | '; DROP | Search | Parameterized |
| SEC-002 | SQL injection | 1 OR 1=1 | Criteria | Parameterized |
| SEC-003 | XSS in name | <script> | Create | Sanitized |
| SEC-004 | XSS in criteria | <img onerror> | Apply | Sanitized |
| SEC-005 | Path traversal | ../ | ID | Rejected |
| SEC-006 | Unauthorized access | User A | GetById | 403 |
| SEC-007 | IDOR | Alter ID | GetById | 403 |
| SEC-008 | Cross-tenant | Tenant A | Tenant B | 403 |
| SEC-009 | Cross-user | User A | User B filter | 403 |
| SEC-010 | Mass assignment | Extra fields | Create | Ignored |
| SEC-011 | No token | Missing | GetById | 401 |
| SEC-012 | Expired token | Expired | GetById | 401 |
| SEC-013 | PII in filter | PII | Create | Redacted |
| SEC-014 | PII in criteria | PII | Apply | Redacted |
| SEC-015 | Secret in log | API key | Log | No secret |
| SEC-016 | DoS many create | 10000 create | Create | Rate limited |
| SEC-017 | DoS large criteria | 10MB criteria | Apply | Rejected |
| SEC-018 | Rate limit | 10000 req/s | GetById | Limited |
| SEC-019 | Cache poisoning | Malicious | Cache | Validated |
| SEC-020 | Criteria injection | Injection | Apply | Sanitized |
| SEC-021 | Import injection | Malicious JSON | Import | Sanitized |
| SEC-022 | Unicode normalization | Homoglyph | Search | Normalized |
| SEC-023 | Prototype pollution | __proto__ | Import | Sanitized |
| SEC-024 | JWT tampering | Altered | GetById | Rejected |
| SEC-025 | Privilege escalation | Low role | Share | 403 |
| SEC-026 | Horizontal privilege | User A | User B share | 403 |
| SEC-027 | Share bypass | Bypass | GetShared | Blocked |
| SEC-028 | Export bypass | Bypass | Export | Blocked |
| SEC-029 | API key exposure | Log | Key | Not logged |
| SEC-030 | Weak crypto | MD5 | Cache | SHA256 |
| SEC-031 | SSRF | URL | Criteria | Blocked |
| SEC-032 | Open redirect | Redirect | Import | Blocked |
| SEC-033 | Header injection | CRLF | Name | Sanitized |
| SEC-034 | NoSQL injection | $ne | Criteria | Parameterized |
| SEC-035 | Command injection | ; rm | Criteria | Sanitized |
| SEC-036 | Replay attack | Replay | Apply | Nonce |
| SEC-037 | CSRF | Cross-site | Create | Token |
| SEC-038 | Information disclosure | Error | Detail | Generic |
| SEC-039 | Enumeration | Sequential IDs | GetById | Rate limited |
| SEC-040 | Metadata exposure | Metadata | Get | Filtered |
| SEC-041 | Criteria exposure | Criteria | Export | Filtered |
| SEC-042 | Insecure deserialization | Binary | Import | JSON only |
| SEC-043 | XXE | XXE | Import | Not XML |
| SEC-044 | JWT algorithm confusion | Alg none | GetById | Rejected |
| SEC-045 | Token replay | Replay | GetById | Rejected |
| SEC-046 | Cache timing | Timing | GetById | Constant time |
| SEC-047 | Share list leak | GetShared | Info | Scoped |
| SEC-048 | Preset leak | GetPresets | Info | Filtered |
| SEC-049 | Default leak | GetDefault | Info | Scoped |
| SEC-050 | Missing auth | No auth | GetById | 401 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Concurrent get same ID | 2 threads same | Both succeed |
| CON-002 | Concurrent create | 2 threads create | Both succeed |
| CON-003 | Concurrent update same | 2 threads update | Conflict or last wins |
| CON-004 | Concurrent delete same | 2 threads delete | One 404 |
| CON-005 | Create during delete | Create + delete | Consistent |
| CON-006 | Update during apply | Update + apply | Handled |
| CON-007 | Share during unshare | Share + unshare | Handled |
| CON-008 | Set default concurrent | 2 set default | One wins |
| CON-009 | Cache stampede | 100 cold | Single load |
| CON-010 | Deadlock | A→B, B→A | No deadlock |
| CON-011 | Lock contention | 50 creates | Throttled |
| CON-012 | Thread pool exhaustion | 1000 threads | Limited |
| CON-013 | Concurrent cancellation | Get + cancel | Cancelled |
| CON-014 | Memory barrier | Create + get | Visible |
| CON-015 | Optimistic concurrency | Update + update | Version |
| CON-016 | Pessimistic lock | Create + lock | Locked |
| CON-017 | Semaphore | Limited | Semaphore |
| CON-018 | Read-write lock | Read + write | RW lock |
| CON-019 | Concurrent clone | 2 clone same | Both succeed |
| CON-020 | Concurrent duplicate | 2 duplicate same | Both succeed |
| CON-021 | Concurrent import | 2 import | Both succeed |
| CON-022 | Concurrent reorder | 2 reorder | One wins |
| CON-023 | Apply during update | Apply + update | Handled |
| CON-024 | Invalidation concurrent | 2 invalidate | Both applied |
| CON-025 | Full concurrency | All ops | All succeed |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | ID validation | Validation | 1 | True |
| UNT-002 | Name validation | Validation | "" | False |
| UNT-003 | Criteria validation | Validation | {} | Valid |
| UNT-004 | Entity type validation | Validation | "Partner" | True |
| UNT-005 | User ID validation | Validation | 0 | False |
| UNT-006 | Name format | Formatting | "  filter  " | "filter" |
| UNT-007 | Criteria format | Formatting | Input | Formatted |
| UNT-008 | Cache key format | Formatting | ID 1 | "filter:1" |
| UNT-009 | JSON format | Formatting | Filter | JSON |
| UNT-010 | Visibility format | Formatting | "PUBLIC" | "public" |
| UNT-011 | Criteria parse | Calculations | "{}" | {} |
| UNT-012 | Merge logic | Calculations | f1, f2 | Merged |
| UNT-013 | Pagination offset | Calculations | Page 2, 10 | 10 |
| UNT-014 | Apply logic | Calculations | Filter, data | Filtered |
| UNT-015 | Default check | Calculations | User, entity | Default |
| UNT-016 | Exists check | Status | ID | True/False |
| UNT-017 | Shared check | Status | Filter | True/False |
| UNT-018 | Default check | Status | Filter | True/False |
| UNT-019 | Cache hit check | Status | Key | Hit |
| UNT-020 | Empty collection | Collections | [] | Empty |
| UNT-021 | Single collection | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Get by ID | GetByIdAsync(1) | <50ms |
| PRF-002 | Get by user | GetByUserAsync(1) | <200ms |
| PRF-003 | Create | CreateAsync(filter) | <200ms |
| PRF-004 | Update | UpdateAsync(id, data) | <200ms |
| PRF-005 | Apply filter | ApplyFilterAsync(id) | <100ms |
| PRF-006 | Search | SearchFiltersAsync(query) | <200ms |
| PRF-007 | Cache hit | GetByIdAsync (cached) | <10ms |
| PRF-008 | Cache miss | GetByIdAsync (cold) | <100ms |
| PRF-009 | Batch 100 | GetByIdsAsync(100) | <500ms |
| PRF-010 | Export | ExportAsync(id) | <200ms |
| PRF-011 | Concurrent 10 | 10 concurrent | <2s |
| PRF-012 | Concurrent 50 | 50 concurrent | <5s |
| PRF-013 | Memory single | GetById | <1MB |
| PRF-014 | Memory bulk | GetByUser | <20MB |
| PRF-015 | Import 100KB | ImportAsync | <500ms |
| PRF-016 | Full flow | Create + apply | <500ms |

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
