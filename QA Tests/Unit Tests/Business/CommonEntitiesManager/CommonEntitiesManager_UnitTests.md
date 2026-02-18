# CommonEntitiesManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/CommonEntitiesManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
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

**Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Common entities manager provides lookup values, dropdown data, and reference data CRUD for shared entities (countries, regions, types, etc.). Tests cover: CRUD for reference entities, lookup by type, dropdown population, caching, and validation across entity types.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Get lookup by type | Lookups exist | GetByType | List returned |
| POS-002 | Get dropdown data | Entity type valid | GetDropdown | Options returned |
| POS-003 | Create reference entity | Valid data | Create | Entity created |
| POS-004 | Update reference entity | Entity exists | Update | Updated |
| POS-005 | Delete reference entity | Entity exists | Delete | Soft deleted |
| POS-006 | Get by ID | Entity exists | GetById | Entity returned |
| POS-007 | List all by type | Type exists | List | All returned |
| POS-008 | Get active only | Mix active/inactive | GetActive | Active only |
| POS-009 | Search by name | Entities exist | Search | Matching returned |
| POS-010 | Get countries | Countries seeded | GetCountries | Countries list |
| POS-011 | Get regions | Regions seeded | GetRegions | Regions list |
| POS-012 | Get entity types | Types seeded | GetTypes | Types list |
| POS-013 | Get with parent | Hierarchy exists | GetWithParent | Parent loaded |
| POS-014 | Get by code | Code unique | GetByCode | Entity returned |
| POS-015 | Bulk get by IDs | IDs valid | GetByIds | All returned |
| POS-016 | Validate reference exists | Entity exists | Validate | True |
| POS-017 | Get display value | ID valid | GetDisplayValue | Display string |
| POS-018 | Sort by name | Entities exist | List sorted | Ordered |
| POS-019 | Filter by status | Entities exist | Filter | Filtered |
| POS-020 | Cache hit | Cached | GetByType | From cache |
| POS-021 | Cache refresh | Stale | Refresh | Updated |
| POS-022 | Typeahead lookup | Partial name | Typeahead | Matching |
| POS-023 | Multiple types | Several types | GetMultiple | Combined |
| POS-024 | Hierarchy depth | Hierarchical | GetHierarchy | Depth correct |
| POS-025 | Default value | Default exists | GetDefault | Default returned |
| POS-026 | Import reference data | CSV valid | Import | Imported |
| POS-027 | Export reference data | Data exists | Export | Exported |
| POS-028 | Get localized names | i18n | GetWithLocale | Localized |
| POS-029 | Audit on create | Create | Check audit | Audit set |
| POS-030 | Audit on update | Update | Check audit | Audit set |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ValidationException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-004 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-005 | Get by invalid type | Type=invalid | ArgumentException |
| NEG-006 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | GetByCode not found | Code=invalid | KeyNotFoundException |
| NEG-009 | Duplicate code | Code exists | BusinessException |
| NEG-010 | Duplicate name | Name exists | BusinessException |
| NEG-011 | Invalid parent ID | ParentId=-1 | ArgumentException |
| NEG-012 | Circular parent | Self as parent | BusinessException |
| NEG-013 | Invalid status | Status=invalid | ArgumentException |
| NEG-014 | Null type | Type=null | ArgumentNullException |
| NEG-015 | Empty type | Type="" | ArgumentException |
| NEG-016 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-017 | GetById without permission | Unauthorized | Forbidden |
| NEG-018 | Create without permission | Unauthorized | Forbidden |
| NEG-019 | Update without permission | Unauthorized | Forbidden |
| NEG-020 | Delete without permission | Unauthorized | Forbidden |
| NEG-021 | Import invalid format | Bad CSV | ValidationException |
| NEG-022 | Import missing required | CSV missing col | ValidationException |
| NEG-023 | Export invalid type | Type=invalid | ArgumentException |
| NEG-024 | Search null term | Term=null | ArgumentNullException |
| NEG-025 | Reorder invalid IDs | IDs invalid | ArgumentException |
| NEG-026 | GetDefault no default | No default | KeyNotFoundException |
| NEG-027 | GetHierarchy invalid root | Root invalid | ArgumentException |
| NEG-028 | Null request object | Request=null | ArgumentNullException |
| NEG-029 | Invalid page number | Page=0 | ArgumentException |
| NEG-030 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-031 | Invalid locale | Locale=invalid | ArgumentException |
| NEG-032 | Bulk get null IDs | Ids=null | ArgumentNullException |
| NEG-033 | Validate non-existent | Id=99999 | False |
| NEG-034 | GetDisplayValue invalid | Id=0 | KeyNotFoundException |
| NEG-035 | Typeahead invalid type | Type=invalid | ArgumentException |
| NEG-036 | Refresh cache invalid | Invalid key | ArgumentException |
| NEG-037 | Create with deleted parent | Parent deleted | BusinessException |
| NEG-038 | Update deleted entity | Entity deleted | KeyNotFoundException |
| NEG-039 | GetByType empty type | Type="" | ArgumentException |
| NEG-040 | GetDropdown invalid | Entity invalid | ArgumentException |
| NEG-041 | Hierarchy depth exceeded | Too deep | BusinessException |
| NEG-042 | Invalid code format | Code invalid | ValidationException |
| NEG-043 | Name exceeds max | Name length | ValidationException |
| NEG-044 | Code exceeds max | Code length | ValidationException |
| NEG-045 | Description exceeds max | Desc length | ValidationException |
| NEG-046 | Null code | Code=null | ValidationException |
| NEG-047 | Invalid sort field | Sort invalid | ArgumentException |
| NEG-048 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-049 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-050 | Transaction rollback | Fail in transaction | Rollback |
| NEG-051 | Connection timeout | DB unavailable | TimeoutException |
| NEG-052 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-053 | Invalid enum value | Out-of-range | ArgumentException |
| NEG-054 | Import duplicate key | CSV duplicate | BusinessException |
| NEG-055 | Export empty | No data | Empty or error |
| NEG-056 | GetByCode case mismatch | Case sensitive | Config-dependent |
| NEG-057 | Permission check null | Resource=null | ArgumentNullException |
| NEG-058 | Bulk get empty | Ids=[] | Returns empty |
| NEG-059 | Count invalid type | Type=invalid | ArgumentException |
| NEG-060 | Invalid sort direction | SortDir invalid | ArgumentException |
| NEG-061 | Filter invalid status | Status invalid | ArgumentException |
| NEG-062 | GetInactive invalid type | Type invalid | ArgumentException |
| NEG-063 | Reorder empty | Ids=[] | ArgumentException |
| NEG-064 | CheckDuplicate null | Name=null | ArgumentNullException |
| NEG-065 | Get localized invalid | Locale invalid | ArgumentException |
| NEG-066 | Validate null ID | Id=null | ArgumentNullException |
| NEG-067 | GetDisplayValue null | Id=null | ArgumentNullException |
| NEG-068 | Child override throws | Child throws | Propagated |
| NEG-069 | Audit missing user | User=0 | InvalidOperationException |
| NEG-070 | Cache invalid key | Key invalid | ArgumentException |
| NEG-071 | GetMultiple null types | Types=null | ArgumentNullException |
| NEG-072 | GetMultiple empty | Types=[] | Returns empty |
| NEG-073 | GetWithParent invalid parent | Parent invalid | ArgumentException |
| NEG-074 | SetDefault invalid entity | Entity invalid | KeyNotFoundException |
| NEG-075 | ClearDefault no default | No default | InvalidOperationException |
| NEG-076 | SyncFromExternal null | Source=null | ArgumentNullException |
| NEG-077 | MergeEntities same entity | Same ID | ArgumentException |
| NEG-078 | CloneEntity null source | Source=null | ArgumentNullException |
| NEG-079 | GetStatistics invalid range | End<Start | ArgumentException |
| NEG-080 | ValidateConfig null | Config=null | ArgumentNullException |
| NEG-081 | GetByExternalId invalid | Id invalid | KeyNotFoundException |
| NEG-082 | MapToExternal null | Entity=null | ArgumentNullException |
| NEG-083 | ResolveReference null | Ref=null | ArgumentNullException |
| NEG-084 | GetAncestors invalid | Id invalid | KeyNotFoundException |
| NEG-085 | GetDescendants invalid | Id invalid | KeyNotFoundException |
| NEG-086 | SetOrder invalid order | Order negative | ArgumentException |
| NEG-087 | GetByOrder invalid | Order invalid | ArgumentException |
| NEG-088 | Activate deleted | Entity deleted | KeyNotFoundException |
| NEG-089 | Deactivate deleted | Entity deleted | KeyNotFoundException |
| NEG-090 | GetVersion invalid | Version invalid | KeyNotFoundException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min length | Length=1 | Valid |
| BND-002 | Name at max length | Length=200 | Valid |
| BND-003 | Name exceeds max | Length=201 | Reject |
| BND-004 | Code at max | Code length | Valid |
| BND-005 | Code over max | Code length+1 | Reject |
| BND-006 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Page number at 1 | Page=1 | Valid |
| BND-011 | Empty type list | Types=[] | Return empty |
| BND-012 | Single type | Types=1 | Valid |
| BND-013 | Max types | Types=max | Valid |
| BND-014 | Date at min | Date=MinValue | Handle |
| BND-015 | Date at max | Date=MaxValue | Handle |
| BND-016 | Unicode in name | Arabic/Chinese | Stored |
| BND-017 | Emoji in name | Emoji | Sanitize or reject |
| BND-018 | Special chars in code | <>&"' | Escaped |
| BND-019 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-020 | Empty filter | Filter empty | Return all |
| BND-021 | Status enum first | First | Valid |
| BND-022 | Status enum last | Last | Valid |
| BND-023 | Zero parent ID | ParentId=0 | Root or reject |
| BND-024 | Max hierarchy depth | At limit | Valid or reject |
| BND-025 | Long string in description | 4000 chars | Truncate or reject |
| BND-026 | Empty search term | Term="" | Return all |
| BND-027 | Search term max | Term=500 | Valid |
| BND-028 | Search term over max | Term=501 | Reject |
| BND-029 | Collection empty | [] | No exception |
| BND-030 | Collection single | 1 item | Valid |
| BND-031 | Collection max | At limit | Valid |
| BND-032 | Decimal precision | 2 decimals | Correct |
| BND-033 | Zero amount | 0 | Valid if allowed |
| BND-034 | Negative amount | -1 | Reject |
| BND-035 | Nullable null | Null | Valid |
| BND-036 | Nullable set | Value | Valid |
| BND-037 | Sort null handling | Nulls in data | Deterministic |
| BND-038 | Pagination last partial | Partial page | Correct |
| BND-039 | Pagination total | Total count | Accurate |
| BND-040 | Cache hit timing | Just cached | Hit |
| BND-041 | Cache expiry | After expiry | Miss |
| BND-042 | Locale boundary | Default locale | Fallback |
| BND-043 | Order at min | Order=0 | Valid |
| BND-044 | Order at max | Order=max | Valid |
| BND-045 | Duplicate order | Same order | Allowed or reject |
| BND-046 | Import row max | Max rows | Valid or reject |
| BND-047 | Import empty file | 0 rows | Empty or error |
| BND-048 | Export large result | 10k rows | Stream or chunk |
| BND-049 | Typeahead min chars | 1 char | Valid |
| BND-050 | Typeahead max results | Limit=100 | Capped |
| BND-051 | Bulk get max | 1000 IDs | Valid |
| BND-052 | Bulk get over max | 1001 IDs | Reject |
| BND-053 | GetDisplayValue empty | Empty display | Fallback |
| BND-054 | Validate edge ID | Id=1 | Valid |
| BND-055 | Soft delete boundary | DeletedDate set | Excluded |
| BND-056 | Include depth | Deep include | No explosion |
| BND-057 | Query timeout | Slow query | Timeout |
| BND-058 | Memory large result | 10k rows | No OOM |
| BND-059 | Filter combination all | All filters | Correct |
| BND-060 | Sort multi-column | 3 columns | Correct order |
| BND-061 | Reorder same | Same order | No-op |
| BND-062 | Hierarchy root | Root | No parent |
| BND-063 | Hierarchy leaf | Leaf | No children |
| BND-064 | Default value edge | Multiple defaults | First or config |
| BND-065 | GetActive empty | No active | Empty list |
| BND-066 | GetByType empty | No for type | Empty list |
| BND-067 | GetCountries empty | No countries | Empty or error |
| BND-068 | Localized missing | No translation | Fallback |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |
| BND-071 | Name exactly 200 chars | Length=200 | Valid |
| BND-072 | Code exactly max | At limit | Valid |
| BND-073 | Description exactly 4000 | 4000 chars | Valid |
| BND-074 | Page 1 first | Page=1 | First page |
| BND-075 | Page at last | Page=last | Last page |
| BND-076 | Zero results | No match | Empty list |
| BND-077 | Single result | One match | Single item |
| BND-078 | Int32.MinValue ID | Id=min | Reject |
| BND-079 | External ID boundary | External ID | Valid |
| BND-080 | Version at 1 | Version=1 | Valid |
| BND-081 | Version at max | Version=max | Valid |
| BND-082 | Order at zero | Order=0 | Valid |
| BND-083 | Order at max int | Order=max | Valid |
| BND-084 | Locale max length | Locale length | Valid |
| BND-085 | Typeahead 1 char | 1 char | Valid |
| BND-086 | Typeahead 100 results | 100 | Capped |
| BND-087 | Ancestors empty | No ancestors | Empty |
| BND-088 | Descendants empty | No descendants | Empty |
| BND-089 | Merge conflict resolution | Conflict | Config |
| BND-090 | Clone preserves config | Clone | Config |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | Code required | Validation | Create | Reject if empty |
| FUN-003 | Type required | Validation | GetByType | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Code unique per type | Constraint | Create | Reject duplicate |
| FUN-008 | Name unique per type | Constraint | Create | Per config |
| FUN-009 | Parent must exist | Constraint | Create | Reject invalid |
| FUN-010 | No circular parent | Constraint | Update parent | Reject |
| FUN-011 | Audit CreatedBy | Audit | Create | Set user |
| FUN-012 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-013 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-014 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-015 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-016 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-017 | Permission before action | Authorization | Any | Check first |
| FUN-018 | Cache invalidation on create | Cache | Create | Invalidated |
| FUN-019 | Cache invalidation on update | Cache | Update | Invalidated |
| FUN-020 | Cache invalidation on delete | Cache | Delete | Invalidated |
| FUN-021 | GetActive filters status | Filter | GetActive | Status=Active |
| FUN-022 | GetByCode case | Config | GetByCode | Case per config |
| FUN-023 | Sort default | Sort | List | Default field |
| FUN-024 | Pagination offset | Calculation | Page | Skip correct |
| FUN-025 | Total count accurate | Calculation | Count | Matches |
| FUN-026 | Hierarchy depth limit | Constraint | GetHierarchy | Max depth |
| FUN-027 | GetDropdown active only | Filter | GetDropdown | Active |
| FUN-028 | Typeahead limit | Constraint | Typeahead | Capped |
| FUN-029 | Import mapping | Validation | Import | Columns mapped |
| FUN-030 | Export format | Format | Export | Correct format |
| FUN-031 | Localized fallback | i18n | GetWithLocale | Fallback |
| FUN-032 | Reorder updates | Update | Reorder | Order updated |
| FUN-033 | Validate checks existence | Validation | Validate | Exists check |
| FUN-034 | GetDisplayValue format | Format | GetDisplayValue | Formatted |
| FUN-035 | Default value logic | Logic | GetDefault | Config-based |
| FUN-036 | Bulk get dedup | Data | GetByIds | No duplicates |
| FUN-037 | Search case | Config | Search | Case per config |
| FUN-038 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-039 | Transaction on create | Transaction | Create | Atomic |
| FUN-040 | Transaction on update | Transaction | Update | Atomic |
| FUN-041 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-042 | Async all operations | Concurrency | All | Async |
| FUN-043 | GetByType returns type | Data | GetByType | Correct type |
| FUN-044 | GetCountries returns countries | Data | GetCountries | Countries |
| FUN-045 | GetRegions returns regions | Data | GetRegions | Regions |
| FUN-046 | Hierarchy parent-child | Data | GetHierarchy | Correct tree |
| FUN-047 | Typeahead partial match | Search | Typeahead | Partial |
| FUN-048 | Import duplicate | Validation | Import | Duplicate handling |
| FUN-049 | Export headers | Format | Export | Headers correct |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | GetMultiple combines | Data | GetMultiple | Combined |
| FUN-052 | GetWithParent loads | Data | GetWithParent | Parent |
| FUN-053 | SetDefault updates | Update | SetDefault | Updated |
| FUN-054 | ClearDefault clears | Update | ClearDefault | Cleared |
| FUN-055 | SyncFromExternal syncs | Data | Sync | Synced |
| FUN-056 | MergeEntities merges | Data | Merge | Merged |
| FUN-057 | CloneEntity copies | Data | Clone | Copied |
| FUN-058 | GetStatistics aggregates | Calculation | GetStatistics | Correct |
| FUN-059 | ValidateConfig validates | Validation | ValidateConfig | Valid |
| FUN-060 | GetByExternalId finds | Data | GetByExternalId | Found |
| FUN-061 | MapToExternal maps | Data | MapToExternal | Mapped |
| FUN-062 | ResolveReference resolves | Logic | ResolveReference | Resolved |
| FUN-063 | GetAncestors returns | Data | GetAncestors | Ancestors |
| FUN-064 | GetDescendants returns | Data | GetDescendants | Descendants |
| FUN-065 | SetOrder updates | Update | SetOrder | Updated |
| FUN-066 | GetByOrder returns | Data | GetByOrder | Ordered |
| FUN-067 | Activate sets status | Update | Activate | Active |
| FUN-068 | Deactivate sets status | Update | Deactivate | Inactive |
| FUN-069 | GetVersion returns | Data | GetVersion | Version |
| FUN-070 | External ID mapping | Logic | Map | Mapped |
| FUN-071 | Version tracking | Audit | Update | Versioned |
| FUN-072 | Ancestor chain correct | Logic | GetAncestors | Chain |
| FUN-073 | Descendant tree correct | Logic | GetDescendants | Tree |
| FUN-074 | Order sequence | Logic | Reorder | Sequence |
| FUN-075 | Status transition | Workflow | Activate/Deactivate | Valid |
| FUN-076 | Import rollback | Transaction | Import | Rollback |
| FUN-077 | Export streaming | Format | Export | Stream |
| FUN-078 | Cache key format | Cache | GetByType | Key |
| FUN-079 | Locale fallback chain | i18n | GetWithLocale | Fallback |
| FUN-080 | Hierarchy validation | Validation | Create | Valid |
| FUN-081 | Bulk get order | Data | GetByIds | Order |
| FUN-082 | Typeahead min chars | Constraint | Typeahead | Min |
| FUN-083 | Reorder validation | Validation | Reorder | Valid |
| FUN-084 | Default uniqueness | Constraint | SetDefault | One |
| FUN-085 | Sync conflict resolution | Logic | Sync | Resolved |
| FUN-086 | Merge conflict resolution | Logic | Merge | Resolved |
| FUN-087 | Clone excludes audit | Logic | Clone | Excludes |
| FUN-088 | Statistics calculation | Calculation | GetStatistics | Correct |
| FUN-089 | Config validation rules | Validation | ValidateConfig | Rules |
| FUN-090 | External ID uniqueness | Constraint | Create | Unique |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create reference full flow | Create | Reference | Created |
| INT-002 | Update reference full flow | Update | Reference | Updated |
| INT-003 | Delete reference full flow | Delete | Reference | Soft deleted |
| INT-004 | Get with parent | GetById | Reference, Parent | Parent loaded |
| INT-005 | List with filter and sort | List | Reference | Filtered, sorted |
| INT-006 | GetByType with cache | GetByType | Reference | Cached |
| INT-007 | GetDropdown for entity | GetDropdown | Reference | Options |
| INT-008 | Search by name | Search | Reference | Matching |
| INT-009 | GetCountries | GetCountries | Country | Countries |
| INT-010 | GetRegions | GetRegions | Region | Regions |
| INT-011 | Pagination | Paginate | Reference | Pages |
| INT-012 | Pagination total | Paginate | Reference | Total |
| INT-013 | GetByCode | GetByCode | Reference | Code |
| INT-014 | Hierarchy load | GetHierarchy | Reference | Tree |
| INT-015 | Typeahead | Typeahead | Reference | Matches |
| INT-016 | Import CSV | Import | Reference | Imported |
| INT-017 | Export CSV | Export | Reference | Exported |
| INT-018 | Get localized | GetWithLocale | Reference | Localized |
| INT-019 | Reorder | Reorder | Reference | Reordered |
| INT-020 | Validate reference | Validate | Reference | True |
| INT-021 | Reference-Parent relationship | Relationship | Reference | FK valid |
| INT-022 | Reference-Child relationship | Relationship | Reference | Children |
| INT-023 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-024 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-025 | DB error handling | Error | DB down | Graceful |
| INT-026 | Timeout handling | Error | Slow DB | Timeout |
| INT-027 | Constraint violation | Error | FK violation | Clear error |
| INT-028 | Unique violation | Error | Duplicate | Clear error |
| INT-029 | Cache service integration | Integration | Cache | Hit/miss |
| INT-030 | Permission service integration | Integration | Permission | Check |
| INT-031 | User resolver integration | Integration | User | Resolved |
| INT-032 | Audit context integration | Integration | Audit | Context |
| INT-033 | Logger integration | Integration | Log | Logged |
| INT-034 | Config integration | Integration | Config | Read |
| INT-035 | Mapper integration | Integration | Map | Correct |
| INT-036 | Repository integration | Integration | Repository | CRUD |
| INT-037 | DbContext integration | Integration | DbContext | Scoped |
| INT-038 | Transaction scope | Integration | Transaction | Atomic |
| INT-039 | Multiple types | Scenario | Reference | Multiple |
| INT-040 | Bulk create | Scenario | Reference | All created |
| INT-041 | Bulk update | Scenario | Reference | All updated |
| INT-042 | Concurrent get | Scenario | Parallel | No conflict |
| INT-043 | Concurrent create | Scenario | Parallel | All created |
| INT-044 | Import with validation | Scenario | Import | Validated |
| INT-045 | Export with filter | Scenario | Export | Filtered |
| INT-046 | Hierarchy deep | Scenario | Reference | Deep tree |
| INT-047 | Typeahead with filter | Scenario | Typeahead | Filtered |
| INT-048 | GetDropdown with cache | Scenario | Dropdown | Cached |
| INT-049 | Refresh cache after create | Scenario | Create, Cache | Refreshed |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |
| INT-051 | GetMultiple flow | Scenario | GetMultiple | Combined |
| INT-052 | GetWithParent flow | Scenario | GetWithParent | Parent |
| INT-053 | SetDefault flow | Scenario | SetDefault | Set |
| INT-054 | ClearDefault flow | Scenario | ClearDefault | Cleared |
| INT-055 | SyncFromExternal flow | Scenario | Sync | Synced |
| INT-056 | MergeEntities flow | Scenario | Merge | Merged |
| INT-057 | CloneEntity flow | Scenario | Clone | Cloned |
| INT-058 | GetStatistics flow | Scenario | GetStatistics | Stats |
| INT-059 | ValidateConfig flow | Scenario | ValidateConfig | Valid |
| INT-060 | GetByExternalId flow | Scenario | GetByExternalId | Found |
| INT-061 | GetAncestors flow | Scenario | GetAncestors | Ancestors |
| INT-062 | GetDescendants flow | Scenario | GetDescendants | Descendants |
| INT-063 | SetOrder flow | Scenario | SetOrder | Updated |
| INT-064 | Activate flow | Scenario | Activate | Active |
| INT-065 | Deactivate flow | Scenario | Deactivate | Inactive |
| INT-066 | Config service integration | Integration | Config | Read |
| INT-067 | Cache service integration | Integration | Cache | Hit/miss |
| INT-068 | Multiple entity types | Scenario | Reference | Multiple |
| INT-069 | Hierarchy full load | Scenario | GetHierarchy | Full |
| INT-070 | Pagination with filter | Scenario | Paginate | Filtered |
| INT-071 | Sort with filter | Scenario | List | Sorted, filtered |
| INT-072 | Typeahead full | Scenario | Typeahead | Results |
| INT-073 | Import export round-trip | Scenario | Import, Export | Match |
| INT-074 | Bulk operations | Scenario | Bulk | All |
| INT-075 | Concurrent operations | Scenario | Parallel | No conflict |
| INT-076 | Error recovery | Scenario | Error | Recover |
| INT-077 | Audit trail full | Scenario | CRUD | Full trail |
| INT-078 | Permission integration | Scenario | Permission | Enforced |
| INT-079 | User context integration | Scenario | User | Context |
| INT-080 | Logger integration flow | Scenario | Log | Logged |
| INT-081 | Mapper round-trip | Scenario | Map | Correct |
| INT-082 | Repository CRUD cycle | Scenario | Repository | CRUD |
| INT-083 | DbContext scoping | Scenario | DbContext | Scoped |
| INT-084 | Transaction rollback | Scenario | Transaction | Rollback |
| INT-085 | Cache invalidation | Scenario | Update | Invalidated |
| INT-086 | Locale fallback chain | Scenario | GetWithLocale | Fallback |
| INT-087 | Hierarchy validation | Scenario | Create | Valid |
| INT-088 | External sync full | Scenario | Sync | Synced |
| INT-089 | Merge conflict handling | Scenario | Merge | Handled |
| INT-090 | E2E with all features | Scenario | Full | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in name | '; DROP TABLE-- | Name | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | SQL injection in search | ' OR '1'='1 | Search | Rejected |
| SEC-004 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls | Any | Rejected |
| SEC-009 | Path traversal | ../../../etc/passwd | File | Rejected |
| SEC-010 | Unauthorized list | No permission | List | 403 |
| SEC-011 | Unauthorized get | No permission | GetById | 403 |
| SEC-012 | Unauthorized create | No permission | Create | 403 |
| SEC-013 | Unauthorized update | No permission | Update | 403 |
| SEC-014 | Unauthorized delete | No permission | Delete | 403 |
| SEC-015 | Unauthorized import | No permission | Import | 403 |
| SEC-016 | Unauthorized export | No permission | Export | 403 |
| SEC-017 | Role escalation | Low role | Admin | 403 |
| SEC-018 | Cross-tenant access | User A | User B data | 403 |
| SEC-019 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-020 | IDOR update other | Id=other | Update | 403 |
| SEC-021 | IDOR delete other | Id=other | Delete | 403 |
| SEC-022 | IDOR in filter | Type=other | List | Filtered |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-025 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-026 | Mass assign Code | Code=manipulated | Request | Validated |
| SEC-027 | Mass assign Type | Type=invalid | Request | Ignored |
| SEC-028 | Session hijack | Stolen token | Any | Detected |
| SEC-029 | Token expiration | Expired | Any | 401 |
| SEC-030 | Invalid token | Malformed | Any | 401 |
| SEC-031 | CSRF on create | No token | Create | Rejected |
| SEC-032 | CSRF on update | No token | Update | Rejected |
| SEC-033 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-034 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit list | Many lists | List | Throttled |
| SEC-037 | Rate limit search | Many searches | Search | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in string | Name | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge page size | List | Capped |
| SEC-045 | Import malicious CSV | Malicious | Import | Rejected |
| SEC-046 | Export data injection | Inject in export | Export | Sanitized |
| SEC-047 | Cache poisoning | Malicious cache | Cache | Not used |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Bulk get IDOR | Other user IDs | GetByIds | Filtered |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same code | Two create | One fails |
| CON-004 | Concurrent create | Two create | Both succeed |
| CON-005 | Cache invalidation race | Update, read | Consistent |
| CON-006 | Read during write | Read while update | Consistent |
| CON-007 | Transaction isolation | Parallel transactions | Serializable |
| CON-008 | Stale entity update | Old version | Concurrency handled |
| CON-009 | Race on reorder | Two reorder | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Import concurrent | Two import | One or both |
| CON-016 | Export concurrent | Two export | Both succeed |
| CON-017 | GetByType cache race | Concurrent get | Cache consistent |
| CON-018 | Hierarchy concurrent | Two get hierarchy | Both correct |
| CON-019 | Soft delete concurrent | Delete while update | Deterministic |
| CON-020 | Bulk create | Concurrent bulk | All succeed |
| CON-021 | Filter concurrent update | Filter while update | Consistent |
| CON-022 | Idempotency | Same request twice | Same result |
| CON-023 | Lock escalation | Many locks | No escalation |
| CON-024 | Connection pool | Many concurrent | Pool limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate code not empty | Validation | "" | Exception |
| UNT-003 | Validate type valid | Validation | Valid type | Pass |
| UNT-004 | Validate parent exists | Validation | Parent | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format display value | Formatting | Entity | Display string |
| UNT-007 | Format export row | Formatting | Entity | CSV row |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Code uniqueness check | Calculation | Code, Type | Duplicate |
| UNT-013 | Hierarchy depth calc | Calculation | Node | Depth |
| UNT-014 | Type allows create | Status logic | Type | true |
| UNT-015 | Type allows update | Status logic | Type | true |
| UNT-016 | Type allows delete | Status logic | Type | true |
| UNT-017 | Status active check | Status logic | Status | Active |
| UNT-018 | Status inactive check | Status logic | Status | Inactive |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Bulk create 100 | Create 100 | <5s | P0 |
| PRF-004 | Bulk create 1000 | Create 1000 | <30s | P0 |
| PRF-005 | GetByType | GetByType | <100ms | P1 |
| PRF-006 | Search by name | Search | <500ms | P1 |
| PRF-007 | GetDropdown | GetDropdown | <200ms | P1 |
| PRF-008 | List with pagination | List | <300ms | P1 |
| PRF-009 | List with sort | List | <300ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 writes | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 write | <4s total | P2 |
| PRF-013 | Memory single create | Create | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory bulk 100 | Bulk create | <20MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 10 RPS create | 10 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 50 RPS | 0→50→0 | 1 min | No errors |
| LDT-005 | Spike 100 RPS | 0→100→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large bulk | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
