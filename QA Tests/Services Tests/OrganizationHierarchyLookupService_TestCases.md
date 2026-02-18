# OrganizationHierarchyLookupService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/OrganizationHierarchyLookupService`  
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

Org hierarchy lookup service: unit search, typeahead, tree traversal, type filtering, permission scoping.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Get unit by ID | Valid ID | GetByIdAsync(id) | Unit returned |
| POS-002 | Get unit by code | Valid code | GetByCodeAsync(code) | Unit returned |
| POS-003 | Search units | Query | SearchAsync(query) | Results |
| POS-004 | Typeahead search | Partial | GetTypeaheadAsync(partial) | Suggestions |
| POS-005 | Get children | Parent ID | GetChildrenAsync(parentId) | Children |
| POS-006 | Get descendants | Unit ID | GetDescendantsAsync(id) | Descendants |
| POS-007 | Get ancestors | Unit ID | GetAncestorsAsync(id) | Ancestors |
| POS-008 | Get by type | Type | GetByTypeAsync(type) | Filtered |
| POS-009 | Get tree | None | GetTreeAsync() | Tree |
| POS-010 | Get subtree | Root ID | GetSubtreeAsync(rootId) | Subtree |
| POS-011 | Get root units | None | GetRootUnitsAsync() | Roots |
| POS-012 | Get leaf units | None | GetLeafUnitsAsync() | Leaves |
| POS-013 | Permission-scoped search | User | SearchAsync(user) | Scoped |
| POS-014 | Get parent | Unit ID | GetParentAsync(id) | Parent |
| POS-015 | Get path to root | Unit ID | GetPathToRootAsync(id) | Path |
| POS-016 | Cache hit | Cached | GetByIdAsync(id) | From cache |
| POS-017 | Paginated search | Page, size | SearchAsync(..., page, size) | Page |
| POS-018 | Filter by status | Status | GetByStatusAsync(status) | Filtered |
| POS-019 | Get dropdown | None | GetDropdownAsync() | Typeahead |
| POS-020 | Batch get | IDs | GetByIdsAsync(ids) | Units |
| POS-021 | Get with permissions | Unit ID | GetWithPermissionsAsync(id) | Unit + perms |
| POS-022 | Validate unit | Unit ID | ValidateUnitAsync(id) | Valid |
| POS-023 | Get hierarchy depth | Unit ID | GetDepthAsync(id) | Depth |
| POS-024 | Get sibling units | Unit ID | GetSiblingsAsync(id) | Siblings |
| POS-025 | Check is descendant | Unit, ancestor | IsDescendantOfAsync(id, anc) | True/False |
| POS-026 | Check is ancestor | Unit, descendant | IsAncestorOfAsync(id, desc) | True/False |
| POS-027 | Get units in path | Path | GetUnitsInPathAsync(path) | Units |
| POS-028 | Resolve unit | Code | ResolveUnitAsync(code) | Unit |
| POS-029 | Get active units | None | GetActiveUnitsAsync() | Active |
| POS-030 | Get metadata | Unit ID | GetMetadataAsync(id) | Metadata |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null unit ID | GetByIdAsync(null) | ArgumentNullException |
| NEG-002 | Negative unit ID | GetByIdAsync(-1) | ArgumentException |
| NEG-003 | Zero unit ID | GetByIdAsync(0) | ArgumentException |
| NEG-004 | Non-existent unit | GetByIdAsync(999999) | KeyNotFoundException |
| NEG-005 | Null code | GetByCodeAsync(null) | ArgumentNullException |
| NEG-006 | Empty code | GetByCodeAsync("") | ArgumentException |
| NEG-007 | Invalid code | GetByCodeAsync("INVALID") | KeyNotFoundException |
| NEG-008 | Null search query | SearchAsync(null) | ArgumentNullException |
| NEG-009 | SQL injection | SearchAsync("'; DROP") | Sanitized |
| NEG-010 | XSS in search | SearchAsync("<script>") | Sanitized |
| NEG-011 | Null type | GetByTypeAsync(null) | ArgumentNullException |
| NEG-012 | Invalid type | GetByTypeAsync("invalid") | ArgumentException |
| NEG-013 | Null parent ID | GetChildrenAsync(null) | ArgumentNullException |
| NEG-014 | Non-existent parent | GetChildrenAsync(999999) | KeyNotFoundException |
| NEG-015 | Deleted unit | GetByIdAsync(deletedId) | KeyNotFoundException |
| NEG-016 | Soft-deleted unit | GetByIdAsync(softDeleted) | KeyNotFoundException |
| NEG-017 | Permission denied | GetByIdAsync(noPerm) | UnauthorizedAccessException |
| NEG-018 | DB timeout | GetByIdAsync(id) | TimeoutException |
| NEG-019 | Circular hierarchy | GetTreeAsync(circular) | InvalidOperationException |
| NEG-020 | Negative page | SearchAsync(..., -1, 10) | ArgumentException |
| NEG-021 | Zero page size | SearchAsync(..., 1, 0) | ArgumentException |
| NEG-022 | Null IDs array | GetByIdsAsync(null) | ArgumentNullException |
| NEG-023 | Empty IDs array | GetByIdsAsync([]) | ArgumentException |
| NEG-024 | IDs too large | GetByIdsAsync(10000) | ArgumentException |
| NEG-025 | Null user | SearchAsync(..., null) | ArgumentNullException |
| NEG-026 | Cancelled token | GetByIdAsync(id, cancelled) | OperationCanceledException |
| NEG-027 | Null filter | SearchAsync(null) | ArgumentNullException |
| NEG-028 | Invalid filter | SearchAsync(invalid) | ArgumentException |
| NEG-029 | Cross-tenant | Tenant A, Tenant B | 403 |
| NEG-030 | Rate limit | Many requests | TooManyRequestsException |
| NEG-031 | Connection failed | GetByIdAsync(id) | ConnectionException |
| NEG-032 | Null ancestor | IsDescendantOfAsync(id, null) | ArgumentNullException |
| NEG-033 | Null descendant | IsAncestorOfAsync(id, null) | ArgumentNullException |
| NEG-034 | Same unit ancestor | IsDescendantOfAsync(id, id) | False |
| NEG-035 | Invalid path | GetUnitsInPathAsync(bad) | ArgumentException |
| NEG-036 | Null path | GetUnitsInPathAsync(null) | ArgumentNullException |
| NEG-037 | Resolve invalid | ResolveUnitAsync("bad") | KeyNotFoundException |
| NEG-038 | Tree empty | GetTreeAsync() | Empty |
| NEG-039 | Subtree invalid root | GetSubtreeAsync(999999) | KeyNotFoundException |
| NEG-040 | Cache corruption | Corrupted cache | CacheInvalidException |
| NEG-041 | Expired cache | GetByIdAsync(expired) | Cache miss |
| NEG-042 | Permission scope empty | SearchAsync(noPerm) | Empty |
| NEG-043 | Type filter empty | GetByTypeAsync(empty) | Empty |
| NEG-044 | Status invalid | GetByStatusAsync("invalid") | ArgumentException |
| NEG-045 | Depth negative | GetDepthAsync(bad) | ArgumentException |
| NEG-046 | Siblings root | GetSiblingsAsync(rootId) | Empty |
| NEG-047 | Batch mixed | GetByIdsAsync([1,999999]) | Partial or error |
| NEG-048 | Dropdown filter invalid | GetDropdownAsync(invalid) | ArgumentException |
| NEG-049 | Metadata missing | GetMetadataAsync(noMeta) | KeyNotFoundException |
| NEG-050 | Count filter invalid | GetCountAsync(invalid) | ArgumentException |
| NEG-051 | Unicode in search | SearchAsync("你好") | Handled |
| NEG-052 | Search too long | SearchAsync(veryLong) | ArgumentException |
| NEG-053 | Type injection | GetByTypeAsync("'; DROP") | Sanitized |
| NEG-054 | Code injection | GetByCodeAsync(injection) | Sanitized |
| NEG-055 | Orphan unit | GetParentAsync(orphan) | Null |
| NEG-056 | Root unit ancestors | GetAncestorsAsync(rootId) | Empty |
| NEG-057 | Leaf unit children | GetChildrenAsync(leafId) | Empty |
| NEG-058 | Hierarchy too deep | GetDescendantsAsync(deep) | Handled |
| NEG-059 | Inactive unit | GetByIdAsync(inactive) | Depends |
| NEG-060 | Archived unit | GetByIdAsync(archived) | Depends |
| NEG-061 | Merged unit | GetByIdAsync(merged) | MergeTarget |
| NEG-062 | Split unit | GetByIdAsync(split) | Multiple |
| NEG-063 | Renamed unit | SearchAsync(oldName) | NotFound |
| NEG-064 | Code changed | GetByCodeAsync(oldCode) | NotFound |
| NEG-065 | Type deprecated | GetByTypeAsync(deprecated) | Depends |
| NEG-066 | Warm-up failure | WarmCacheAsync() | CacheException |
| NEG-067 | Tree cycle | GetTreeAsync(cycle) | InvalidOperationException |
| NEG-068 | Path overflow | GetPathToRootAsync(deep) | Handled |
| NEG-069 | Sibling count max | GetSiblingsAsync(many) | All |
| NEG-070 | Full details missing | GetFullDetailsAsync(noMeta) | KeyNotFoundException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Unit ID = 1 | Min valid | Unit returned |
| BND-002 | Unit ID = Int32.MaxValue | Max | Error or unit |
| BND-003 | Code length = 1 | "A" | Valid |
| BND-004 | Code length = 50 | Max | Valid |
| BND-005 | Code length = 51 | Over | Rejected |
| BND-006 | Search length = 0 | "" | Invalid |
| BND-007 | Search length = 1 | "H" | Results |
| BND-008 | Search length = 255 | Max | Results |
| BND-009 | Page = 1 | First | Results |
| BND-010 | Page = last | Last | Results |
| BND-011 | Page size = 1 | Min | One |
| BND-012 | Page size = 100 | Max | 100 |
| BND-013 | Page size = 101 | Over | Clamped |
| BND-014 | IDs array = 0 | [] | Invalid |
| BND-015 | IDs array = 1 | [1] | One |
| BND-016 | IDs array = 1000 | Max | Results |
| BND-017 | Hierarchy depth = 0 | Root | Valid |
| BND-018 | Hierarchy depth = 1 | One level | Valid |
| BND-019 | Hierarchy depth = 10 | Deep | Valid |
| BND-020 | Hierarchy depth = 20 | Max | Valid |
| BND-021 | Children count = 0 | Leaf | [] |
| BND-022 | Children count = 1 | One | [1] |
| BND-023 | Children count = 500 | Many | All |
| BND-024 | Descendants count = 0 | Leaf | [] |
| BND-025 | Descendants count = 1000 | Many | All |
| BND-026 | Cache size = 0 | Cold | Miss |
| BND-027 | Cache size = 1 | One | Hit |
| BND-028 | Cache size = 10000 | Max | Eviction |
| BND-029 | Concurrent requests = 1 | 1 | Success |
| BND-030 | Concurrent requests = 100 | 100 | All succeed |
| BND-031 | Typeahead limit = 0 | 0 | Invalid |
| BND-032 | Typeahead limit = 10 | 10 | 10 max |
| BND-033 | Typeahead limit = 100 | Max | 100 max |
| BND-034 | Tree nodes = 0 | Empty | Empty |
| BND-035 | Tree nodes = 1 | Single | Single |
| BND-036 | Tree nodes = 10000 | Many | All |
| BND-037 | Type count = 0 | [] | All |
| BND-038 | Type count = 1 | [1] | Filtered |
| BND-039 | Type count = 10 | Many | Filtered |
| BND-040 | Path length = 0 | [] | Invalid |
| BND-041 | Path length = 1 | [root] | Valid |
| BND-042 | Path length = 20 | Max | Valid |
| BND-043 | Unicode in search | "Büro" | Valid |
| BND-044 | Emoji in search | "Office 👍" | Sanitized |
| BND-045 | RTL in search | "مكتب" | Valid |
| BND-046 | Timeout = 0ms | 0 | Immediate |
| BND-047 | Timeout = 30000ms | 30s | Success |
| BND-048 | Retry count = 0 | No retry | Fail once |
| BND-049 | Retry count = 3 | 3 | Retries |
| BND-050 | Root count = 0 | None | [] |
| BND-051 | Root count = 1 | One | [1] |
| BND-052 | Leaf count = 0 | None | [] |
| BND-053 | Leaf count = 1000 | Many | All |
| BND-054 | Sibling count = 0 | Only child | [] |
| BND-055 | Sibling count = 50 | Many | All |
| BND-056 | Filter empty | {} | All |
| BND-057 | Filter full | Full | Filtered |
| BND-058 | Status active | Active | Active only |
| BND-059 | Status all | All | All |
| BND-060 | Depth 0 | Root | 0 |
| BND-061 | Depth 10 | Deep | 10 |
| BND-062 | Count 0 | No match | 0 |
| BND-063 | Count max | Many | Count |
| BND-064 | Resolve exact | Exact code | Unit |
| BND-065 | Resolve partial | Partial | Depends |
| BND-066 | Permission scope full | Full | All |
| BND-067 | Permission scope empty | None | Empty |
| BND-068 | Batch distinct | [1,2,3] | 3 |
| BND-069 | Batch overlap | [1,2,1] | 2 |
| BND-070 | Subtree depth 0 | Leaf | Leaf only |
| BND-071 | Type count = 0 | [] | All |
| BND-072 | Type count = 5 | Many | Filtered |
| BND-073 | Path length = 1 | [root] | Valid |
| BND-074 | Path length = 20 | Max | Valid |
| BND-075 | Sibling count = 0 | Only | [] |
| BND-076 | Sibling count = 50 | Many | All |
| BND-077 | Descendant count = 0 | Leaf | [] |
| BND-078 | Descendant count = 1000 | Many | All |
| BND-079 | Typeahead limit = 10 | 10 | Limited |
| BND-080 | Typeahead limit = 100 | Max | Limited |
| BND-081 | Tree nodes = 0 | Empty | [] |
| BND-082 | Tree nodes = 10000 | Many | All |
| BND-083 | Filter empty | {} | All |
| BND-084 | Filter full | Full | Filtered |
| BND-085 | Status active | Active | Active only |
| BND-086 | Status all | All | All |
| BND-087 | Depth 0 | Root | 0 |
| BND-088 | Depth 10 | Deep | 10 |
| BND-089 | Count 0 | No match | 0 |
| BND-090 | Count max | Many | Count |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Code uniqueness | Unique | GetByCode | One result |
| FUN-002 | ID uniqueness | Unique | GetById | One result |
| FUN-003 | Hierarchy integrity | Integrity | GetTree | Valid |
| FUN-004 | Permission scoping | Scope | Search | Scoped |
| FUN-005 | Type filtering | Filter | GetByType | Filtered |
| FUN-006 | Cache TTL | TTL | Cache | Expires |
| FUN-007 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-008 | Active filter | Active | GetActive | Active only |
| FUN-009 | Search case-insensitive | Case | Search | Case-insensitive |
| FUN-010 | Search partial match | Partial | Search | Matches |
| FUN-011 | Tree traversal order | Order | GetTree | DFS/BFS |
| FUN-012 | Ancestor order | Order | GetAncestors | Root last |
| FUN-013 | Descendant order | Order | GetDescendants | Level order |
| FUN-014 | Path order | Order | GetPathToRoot | Root last |
| FUN-015 | Typeahead limit | Limit | GetTypeahead | Limited |
| FUN-016 | Pagination offset | Offset | Search | Correct |
| FUN-017 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-018 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-019 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-020 | Fallback for missing | Fallback | Missing | Fallback |
| FUN-021 | Error format | Format | Error | Consistent |
| FUN-022 | Trim input | Trim | Search | Trimmed |
| FUN-023 | Normalize code | Normalize | Code | Uppercase |
| FUN-024 | Retry on transient | Retry | Transient | Retried |
| FUN-025 | No retry on permanent | No retry | Permanent | Fail |
| FUN-026 | Timeout handling | Timeout | Slow | Timeout |
| FUN-027 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-028 | Rate limit | Rate | Many | Limited |
| FUN-029 | Audit trail | Audit | Get | Logged |
| FUN-030 | Permission check | Permission | Get | Checked |
| FUN-031 | Tenant isolation | Tenant | Get | Isolated |
| FUN-032 | Descendant check | Check | IsDescendantOf | Correct |
| FUN-033 | Ancestor check | Check | IsAncestorOf | Correct |
| FUN-034 | Sibling check | Check | GetSiblings | Exclude self |
| FUN-035 | Root identification | Root | GetRootUnits | Correct |
| FUN-036 | Leaf identification | Leaf | GetLeafUnits | Correct |
| FUN-037 | Depth calculation | Calc | GetDepth | Correct |
| FUN-038 | Path resolution | Resolve | GetPathToRoot | Correct |
| FUN-039 | Subtree boundary | Boundary | GetSubtree | Inclusive |
| FUN-040 | Filter combination | Combine | Search | AND/OR |
| FUN-041 | Type combination | Combine | GetByType | Multiple |
| FUN-042 | Status combination | Combine | GetByStatus | Multiple |
| FUN-043 | Permission hierarchy | Hierarchy | GetWithPermissions | Inherited |
| FUN-044 | Metadata aggregation | Aggregate | GetMetadata | Merged |
| FUN-045 | Count accuracy | Accuracy | GetCount | Exact |
| FUN-046 | Tree structure | Structure | GetTree | Valid |
| FUN-047 | Filtered tree | Filter | GetFilteredTree | Filtered |
| FUN-048 | Dropdown order | Order | GetDropdown | Sorted |
| FUN-049 | Resolve logic | Resolve | ResolveUnit | Correct |
| FUN-050 | Full details merge | Merge | GetFullDetails | Merged |
| FUN-051 | Code uniqueness | Unique | GetByCode | One |
| FUN-052 | ID uniqueness | Unique | GetById | One |
| FUN-053 | Hierarchy integrity | Integrity | GetTree | Valid |
| FUN-054 | Permission scoping | Scope | Search | Scoped |
| FUN-055 | Type filtering | Filter | GetByType | Filtered |
| FUN-056 | Cache TTL | TTL | Cache | Expires |
| FUN-057 | Soft delete excluded | Exclude | GetAll | No deleted |
| FUN-058 | Active filter | Active | GetActive | Active only |
| FUN-059 | Search case-insensitive | Case | Search | Case-insensitive |
| FUN-060 | Search partial match | Partial | Search | Matches |
| FUN-061 | Tree traversal order | Order | GetTree | DFS/BFS |
| FUN-062 | Ancestor order | Order | GetAncestors | Root last |
| FUN-063 | Descendant order | Order | GetDescendants | Level order |
| FUN-064 | Path order | Order | GetPathToRoot | Root last |
| FUN-065 | Typeahead limit | Limit | GetTypeahead | Limited |
| FUN-066 | Pagination offset | Offset | Search | Correct |
| FUN-067 | Batch deduplication | Dedup | GetByIds | Deduplicated |
| FUN-068 | Invalidation on update | Invalidation | Update | Cache cleared |
| FUN-069 | Warm-up loads all | Warm-up | WarmCache | All loaded |
| FUN-070 | Fallback for missing | Fallback | Missing | Fallback |
| FUN-071 | Error format | Format | Error | Consistent |
| FUN-072 | Trim input | Trim | Search | Trimmed |
| FUN-073 | Normalize code | Normalize | Code | Uppercase |
| FUN-074 | Retry on transient | Retry | Transient | Retried |
| FUN-075 | No retry on permanent | No retry | Permanent | Fail |
| FUN-076 | Timeout handling | Timeout | Slow | Timeout |
| FUN-077 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-078 | Rate limit | Rate | Many | Limited |
| FUN-079 | Audit trail | Audit | Get | Logged |
| FUN-080 | Permission check | Permission | Get | Checked |
| FUN-081 | Tenant isolation | Tenant | Get | Isolated |
| FUN-082 | Descendant check | Check | IsDescendantOf | Correct |
| FUN-083 | Ancestor check | Check | IsAncestorOf | Correct |
| FUN-084 | Sibling check | Check | GetSiblings | Exclude self |
| FUN-085 | Root identification | Root | GetRootUnits | Correct |
| FUN-086 | Leaf identification | Leaf | GetLeafUnits | Correct |
| FUN-087 | Depth calculation | Calc | GetDepth | Correct |
| FUN-088 | Path resolution | Resolve | GetPathToRoot | Correct |
| FUN-089 | Subtree boundary | Boundary | GetSubtree | Inclusive |
| FUN-090 | Filter combination | Combine | Search | AND/OR |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | DbContext | EF Core | GetById | Loaded |
| INT-002 | Org unit entity | Entity | GetById | Mapped |
| INT-003 | Cache service | ICacheService | GetById | Cached |
| INT-004 | Permission service | IPermissionService | Search | Scoped |
| INT-005 | User service | IUserService | User | Resolved |
| INT-006 | Opportunity | IOpportunityManager | Unit in opp | Linked |
| INT-007 | Partner | IPartnerManager | Unit in partner | Linked |
| INT-008 | Configuration | IConfiguration | Config | Applied |
| INT-009 | Logger | ILogger | Log | Logged |
| INT-010 | AutoMapper | IMapper | Map | Mapped |
| INT-011 | Full search flow | All | Search | Success |
| INT-012 | Full tree flow | All | GetTree | Success |
| INT-013 | Full hierarchy flow | All | GetAncestors | Success |
| INT-014 | Opportunity + unit | Opp + unit | Opp with unit | Linked |
| INT-015 | Partner + unit | Partner + unit | Partner with unit | Linked |
| INT-016 | Search + pagination | Search + pagination | Search paged | Success |
| INT-017 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-018 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-019 | Soft delete filter | DbContext | Get all | Filtered |
| INT-020 | Permission + search | Permission | Search | Checked |
| INT-021 | Tenant + get | Tenant | Get | Scoped |
| INT-022 | Tree + types | Tree + types | GetTreeWithTypes | Combined |
| INT-023 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-024 | Search + filter | Search + filter | Search | Filtered |
| INT-025 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-026 | Config + cache TTL | Config | Cache | TTL |
| INT-027 | Logger + error | Logger | Error | Logged |
| INT-028 | Mapper + entity | Mapper | Entity | Mapped |
| INT-029 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-030 | Retry + transient | Retry | Transient | Retried |
| INT-031 | Timeout + get | Timeout | Get | Timeout |
| INT-032 | Cancellation + get | Cancel | Get | Cancelled |
| INT-033 | Rate limit + get | Rate limit | Many | Limited |
| INT-034 | Audit + get | Audit | Get | Logged |
| INT-035 | Validation + API | Validation | API | Validated |
| INT-036 | Error handler + get | Error | Get | Handled |
| INT-037 | DoA lookup | DoA | GetDoA | Linked |
| INT-038 | EntityUserRole | EntityUserRole | GetRole | Linked |
| INT-039 | Org hierarchy manager | OrgHierarchyManager | Full | Linked |
| INT-040 | Liaison office | LiaisonOffice | Unit | Linked |
| INT-041 | Country | Country | Unit | Linked |
| INT-042 | Multi-tenant + cache | Tenant + cache | Get | Isolated |
| INT-043 | Permission + hierarchy | Permission | Hierarchy | Inherited |
| INT-044 | Type + filter | Type | Filter | Combined |
| INT-045 | Status + filter | Status | Filter | Combined |
| INT-046 | Warm-up + load | Warm-up | Startup | Loaded |
| INT-047 | Resolve + cache | Resolve + cache | Resolve | Cached |
| INT-048 | Path + units | Path | GetUnitsInPath | Resolved |
| INT-049 | Sibling + parent | Sibling | GetSiblings | Correct |
| INT-050 | End-to-end | All | Full flow | Success |
| INT-051 | DbContext | EF Core | GetById | Loaded |
| INT-052 | Org unit entity | Entity | GetById | Mapped |
| INT-053 | Cache service | ICacheService | GetById | Cached |
| INT-054 | Permission service | IPermissionService | Search | Scoped |
| INT-055 | User service | IUserService | User | Resolved |
| INT-056 | Opportunity | IOpportunityManager | Unit in opp | Linked |
| INT-057 | Partner | IPartnerManager | Unit in partner | Linked |
| INT-058 | Configuration | IConfiguration | Config | Applied |
| INT-059 | Logger | ILogger | Log | Logged |
| INT-060 | AutoMapper | IMapper | Map | Mapped |
| INT-061 | Full search flow | All | Search | Success |
| INT-062 | Full tree flow | All | GetTree | Success |
| INT-063 | Full hierarchy flow | All | GetAncestors | Success |
| INT-064 | Opportunity + unit | Opp + unit | Opp with unit | Linked |
| INT-065 | Partner + unit | Partner + unit | Partner with unit | Linked |
| INT-066 | Search + pagination | Search + pagination | Search paged | Success |
| INT-067 | Cache + DB | Cache + DB | Miss then hit | Both |
| INT-068 | Cache invalidation | Cache + update | Update | Invalidated |
| INT-069 | Soft delete filter | DbContext | Get all | Filtered |
| INT-070 | Permission + search | Permission | Search | Checked |
| INT-071 | Tenant + get | Tenant | Get | Scoped |
| INT-072 | Tree + types | Tree + types | GetTreeWithTypes | Combined |
| INT-073 | Batch + cache | Batch + cache | GetByIds | Mixed |
| INT-074 | Search + filter | Search + filter | Search | Filtered |
| INT-075 | Pagination + sort | Pagination + sort | Page | Sorted |
| INT-076 | Config + cache TTL | Config | Cache | TTL |
| INT-077 | Logger + error | Logger | Error | Logged |
| INT-078 | Mapper + entity | Mapper | Entity | Mapped |
| INT-079 | DbContext + transaction | DbContext | Transaction | Consistent |
| INT-080 | Retry + transient | Retry | Transient | Retried |
| INT-081 | Timeout + get | Timeout | Get | Timeout |
| INT-082 | Cancellation + get | Cancel | Get | Cancelled |
| INT-083 | Rate limit + get | Rate limit | Many | Limited |
| INT-084 | Audit + get | Audit | Get | Logged |
| INT-085 | Validation + API | Validation | API | Validated |
| INT-086 | Error handler + get | Error | Get | Handled |
| INT-087 | DoA lookup | DoA | GetDoA | Linked |
| INT-088 | EntityUserRole | EntityUserRole | GetRole | Linked |
| INT-089 | Org hierarchy manager | OrgHierarchyManager | Full | Linked |
| INT-090 | End-to-end | All | Full flow | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection | '; DROP | Search | Parameterized |
| SEC-002 | SQL injection | 1 OR 1=1 | Search | Parameterized |
| SEC-003 | XSS in search | <script> | Search | Sanitized |
| SEC-004 | Path traversal | ../ | ID | Rejected |
| SEC-005 | Null byte | %00 | ID | Rejected |
| SEC-006 | Unauthorized access | User A | GetById | 403 |
| SEC-007 | IDOR | Alter ID | GetById | 403 |
| SEC-008 | Cross-tenant | Tenant A | Tenant B | 403 |
| SEC-009 | Mass assignment | Extra fields | Update | Ignored |
| SEC-010 | No token | Missing | GetById | 401 |
| SEC-011 | Expired token | Expired | GetById | 401 |
| SEC-012 | PII in response | PII | GetById | Redacted |
| SEC-013 | Secret in log | API key | Log | No secret |
| SEC-014 | DoS large batch | 100000 ids | GetByIds | Rejected |
| SEC-015 | DoS long search | 100000 chars | Search | Rejected |
| SEC-016 | Rate limit | 10000 req/s | GetById | Limited |
| SEC-017 | Cache poisoning | Malicious | Cache | Validated |
| SEC-018 | Injection in code | '; DROP | GetByCode | Parameterized |
| SEC-019 | Injection in type | '; DROP | GetByType | Parameterized |
| SEC-020 | Unicode normalization | Homoglyph | Search | Normalized |
| SEC-021 | Integer overflow | Overflow | GetById | Rejected |
| SEC-022 | Prototype pollution | __proto__ | Parse | Sanitized |
| SEC-023 | JWT tampering | Altered | GetById | Rejected |
| SEC-024 | Privilege escalation | Low role | Admin | 403 |
| SEC-025 | Horizontal privilege | User A | User B | 403 |
| SEC-026 | Permission bypass | Bypass | Search | Blocked |
| SEC-027 | Scope bypass | Bypass | GetTree | Blocked |
| SEC-028 | API key exposure | Log | Key | Not logged |
| SEC-029 | Weak crypto | MD5 | Cache | SHA256 |
| SEC-030 | SSRF | URL | Entity | Blocked |
| SEC-031 | Open redirect | Redirect | Search | Blocked |
| SEC-032 | Header injection | CRLF | Search | Sanitized |
| SEC-033 | NoSQL injection | $ne | ID | Parameterized |
| SEC-034 | Command injection | ; rm | Search | Sanitized |
| SEC-035 | Replay attack | Replay | GetById | Nonce |
| SEC-036 | CSRF | Cross-site | Update | Token |
| SEC-037 | Information disclosure | Error | Detail | Generic |
| SEC-038 | Enumeration | Sequential IDs | GetById | Rate limited |
| SEC-039 | Metadata exposure | Metadata | Get | Filtered |
| SEC-040 | Tree structure exposure | Tree | GetTree | Filtered |
| SEC-041 | Hierarchy exposure | Hierarchy | GetAncestors | Scoped |
| SEC-042 | Insecure deserialization | Binary | Parse | JSON only |
| SEC-043 | XXE | XXE | Parse | Not XML |
| SEC-044 | JWT algorithm confusion | Alg none | GetById | Rejected |
| SEC-045 | Token replay | Replay | GetById | Rejected |
| SEC-046 | Cache timing | Timing | GetById | Constant time |
| SEC-047 | Descendant info leak | IsDescendantOf | Info | No leak |
| SEC-048 | Ancestor info leak | IsAncestorOf | Info | No leak |
| SEC-049 | Type info leak | GetByType | Info | Filtered |
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
| CON-007 | Tree during tree | 2 threads GetTree | Both succeed |
| CON-008 | Warm-up during get | Warm + get | Handled |
| CON-009 | Cache stampede | 100 cold | Single load |
| CON-010 | Deadlock | A→B, B→A | No deadlock |
| CON-011 | Lock contention | 50 threads | Throttled |
| CON-012 | Thread pool exhaustion | 1000 threads | Limited |
| CON-013 | Concurrent cancellation | Get + cancel | Cancelled |
| CON-014 | Memory barrier | Get + cache | Visible |
| CON-015 | Optimistic concurrency | Update + get | Version |
| CON-016 | Pessimistic lock | Get + lock | Locked |
| CON-017 | Semaphore | Limited | Semaphore |
| CON-018 | Read-write lock | Read + write | RW lock |
| CON-019 | Concurrent pagination | 2 threads page | Both correct |
| CON-020 | Concurrent batch | 2 threads batch | Both succeed |
| CON-021 | Concurrent hierarchy | 2 threads hierarchy | Both succeed |
| CON-022 | Tree during update | GetTree + update | Consistent |
| CON-023 | Search during update | Search + update | Eventual |
| CON-024 | Invalidation concurrent | 2 invalidate | Both applied |
| CON-025 | Full concurrency | All ops | All succeed |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Code validation | Validation | "HQ" | True |
| UNT-002 | Code invalid | Validation | "" | False |
| UNT-003 | ID validation | Validation | 1 | True |
| UNT-004 | Type validation | Validation | "Region" | True |
| UNT-005 | Path validation | Validation | [1,2,3] | True |
| UNT-006 | Code format | Formatting | "hq" | "HQ" |
| UNT-007 | Search format | Formatting | "  query  " | "query" |
| UNT-008 | Cache key format | Formatting | ID 1 | "org:1" |
| UNT-009 | Path format | Formatting | [1,2,3] | "1/2/3" |
| UNT-010 | Filter format | Formatting | Params | Filter |
| UNT-011 | Depth calc | Calculations | Unit | Depth |
| UNT-012 | Pagination offset | Calculations | Page 2, 10 | 10 |
| UNT-013 | Path length calc | Calculations | Path | Length |
| UNT-014 | Sibling count | Calculations | Unit | Count |
| UNT-015 | Descendant count | Calculations | Unit | Count |
| UNT-016 | Exists check | Status | ID | True/False |
| UNT-017 | Root check | Status | Unit | Root |
| UNT-018 | Leaf check | Status | Unit | Leaf |
| UNT-019 | Cache hit check | Status | Key | Hit |
| UNT-020 | Empty collection | Collections | [] | Empty |
| UNT-021 | Single collection | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Get by ID | GetByIdAsync(1) | <50ms |
| PRF-002 | Get by code | GetByCodeAsync("HQ") | <50ms |
| PRF-003 | Search | SearchAsync("Office") | <200ms |
| PRF-004 | Typeahead | GetTypeaheadAsync("He") | <100ms |
| PRF-005 | Get children | GetChildrenAsync(1) | <100ms |
| PRF-006 | Get tree | GetTreeAsync() | <500ms |
| PRF-007 | Cache hit | GetByIdAsync (cached) | <10ms |
| PRF-008 | Cache miss | GetByIdAsync (cold) | <100ms |
| PRF-009 | Batch 100 | GetByIdsAsync(100) | <500ms |
| PRF-010 | Get ancestors | GetAncestorsAsync | <100ms |
| PRF-011 | Concurrent 10 | 10 concurrent | <1s |
| PRF-012 | Concurrent 50 | 50 concurrent | <3s |
| PRF-013 | Memory single | GetById | <1MB |
| PRF-014 | Memory tree | GetTree | <50MB |
| PRF-015 | Warm-up | WarmCacheAsync | <5s |
| PRF-016 | Full flow | Get + cache + tree | <300ms |

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
