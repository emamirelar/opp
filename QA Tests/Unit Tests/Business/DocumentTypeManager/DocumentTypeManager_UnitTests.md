# DocumentTypeManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DocumentTypeManager` (Unit Tests)  
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

Document type manager unit tests cover CRUD for document types, MIME mapping, validation rules, and type configuration. Tests include: create/update/delete types, MIME type mapping, extension validation, size limits per type, and dropdown/selection behavior.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create document type | Valid data | Create | Type created |
| POS-002 | Get type by ID | Type exists | GetById | Type returned |
| POS-003 | Update document type | Type exists | Update | Updated |
| POS-004 | Delete document type | Type exists | Delete | Soft deleted |
| POS-005 | List all types | Types exist | List | List returned |
| POS-006 | Get by MIME type | MIME exists | GetByMime | Type returned |
| POS-007 | Get by extension | Extension exists | GetByExtension | Type returned |
| POS-008 | Validate file type | Type valid | Validate | True |
| POS-009 | Get MIME for extension | Extension valid | GetMime | MIME returned |
| POS-010 | Get extension for MIME | MIME valid | GetExtension | Extension returned |
| POS-011 | Get allowed types | Types configured | GetAllowed | Allowed list |
| POS-012 | Get dropdown options | Types exist | GetDropdown | Options |
| POS-013 | Check extension allowed | Extension in list | IsAllowed | True |
| POS-014 | Get max file size | Type configured | GetMaxSize | Size returned |
| POS-015 | Get validation rules | Type configured | GetRules | Rules returned |
| POS-016 | Sort by name | Types exist | List sorted | Ordered |
| POS-017 | Filter by category | Types exist | Filter | Filtered |
| POS-018 | Pagination | Many types | List page | Page |
| POS-019 | Audit CreatedBy | Create | Check audit | Set |
| POS-020 | Audit CreatedDate | Create | Check audit | UTC |
| POS-021 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-022 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-023 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-024 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-025 | Multiple MIME per type | Type has MIMEs | GetMimes | List |
| POS-026 | Multiple extensions per type | Type has exts | GetExtensions | List |
| POS-027 | Default type | Default configured | GetDefault | Default |
| POS-028 | Category hierarchy | Categories exist | GetHierarchy | Tree |
| POS-029 | Import types | CSV valid | Import | Imported |
| POS-030 | Export types | Types exist | Export | Exported |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ValidationException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Create with duplicate name | Name exists | BusinessException |
| NEG-004 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-005 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-006 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | GetByMime not found | MIME invalid | KeyNotFoundException |
| NEG-009 | GetByExtension not found | Ext invalid | KeyNotFoundException |
| NEG-010 | Validate invalid type | Type invalid | False |
| NEG-011 | Null MIME type | Mime=null | ArgumentNullException |
| NEG-012 | Empty MIME type | Mime="" | ArgumentException |
| NEG-013 | Null extension | Extension=null | ArgumentNullException |
| NEG-014 | Empty extension | Extension="" | ArgumentException |
| NEG-015 | GetById without permission | Unauthorized | Forbidden |
| NEG-016 | Create without permission | Unauthorized | Forbidden |
| NEG-017 | Update without permission | Unauthorized | Forbidden |
| NEG-018 | Delete without permission | Unauthorized | Forbidden |
| NEG-019 | Invalid MIME format | MIME=bad | ValidationException |
| NEG-020 | Invalid extension format | Ext=bad | ValidationException |
| NEG-021 | Duplicate MIME | MIME exists | BusinessException |
| NEG-022 | Duplicate extension | Extension exists | BusinessException |
| NEG-023 | GetMaxSize invalid type | Type invalid | ArgumentException |
| NEG-024 | GetRules invalid type | Type invalid | ArgumentException |
| NEG-025 | AddMime invalid type | Type invalid | KeyNotFoundException |
| NEG-026 | RemoveMime non-existent | Mapping invalid | KeyNotFoundException |
| NEG-027 | AddExtension invalid type | Type invalid | KeyNotFoundException |
| NEG-028 | RemoveExtension non-existent | Ext invalid | KeyNotFoundException |
| NEG-029 | Import invalid format | Bad CSV | ValidationException |
| NEG-030 | Import duplicate | CSV duplicate | BusinessException |
| NEG-031 | Export invalid format | Format invalid | ArgumentException |
| NEG-032 | GetDefault no default | No default | KeyNotFoundException |
| NEG-033 | GetForEntity invalid | Entity invalid | ArgumentException |
| NEG-034 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-035 | Invalid page number | Page=0 | ArgumentException |
| NEG-036 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-037 | Null request object | Request=null | ArgumentNullException |
| NEG-038 | Update deleted type | Type deleted | KeyNotFoundException |
| NEG-039 | GetById deleted | Type deleted | KeyNotFoundException |
| NEG-040 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-041 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-042 | Transaction rollback | Fail in transaction | Rollback |
| NEG-043 | Connection timeout | DB unavailable | TimeoutException |
| NEG-044 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-045 | Invalid enum value | Category invalid | ArgumentException |
| NEG-046 | Circular category | Self as parent | BusinessException |
| NEG-047 | Expired session | Expired token | Unauthorized |
| NEG-048 | Null user context | User=null | InvalidOperationException |
| NEG-049 | Invalid include path | Invalid include | ArgumentException |
| NEG-050 | GetMime null extension | Extension=null | ArgumentNullException |
| NEG-051 | GetExtension null MIME | Mime=null | ArgumentNullException |
| NEG-052 | IsAllowed null extension | Extension=null | ArgumentNullException |
| NEG-053 | Delete type with documents | Documents exist | BusinessException |
| NEG-054 | AddMime to deleted type | Type deleted | KeyNotFoundException |
| NEG-055 | GetHierarchy invalid root | Root invalid | ArgumentException |
| NEG-056 | Export empty | No types | Empty or error |
| NEG-057 | Filter invalid category | Category invalid | ArgumentException |
| NEG-058 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-059 | Pagination overflow | Page too large | Empty or error |
| NEG-060 | GetAllowed empty | No types | Empty list |
| NEG-061 | GetDropdown empty | No types | Empty list |
| NEG-062 | Audit missing user | User=0 | InvalidOperationException |
| NEG-063 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-064 | Validate null type | Type=null | ArgumentNullException |
| NEG-065 | GetMaxSize negative | Negative size | ArgumentException |
| NEG-066 | Child override throws | Child throws | Propagated |
| NEG-067 | MIME case sensitivity | Case mismatch | Config |
| NEG-068 | Extension case | .PDF vs .pdf | Config |
| NEG-069 | Reserved extension | .exe | Rejected |
| NEG-070 | Reserved MIME | application/x-msdownload | Rejected |
| NEG-071 | AddMime null type ID | TypeId=0 | ArgumentException |
| NEG-072 | RemoveMime null mapping ID | MappingId=0 | ArgumentException |
| NEG-073 | GetByType null type | Type=null | ArgumentNullException |
| NEG-074 | Import null stream | Stream=null | ArgumentNullException |
| NEG-075 | Export null format | Format=null | ArgumentNullException |
| NEG-076 | GetForEntity null entity | Entity=null | ArgumentNullException |
| NEG-077 | Validate empty whitelist | Whitelist empty | False |
| NEG-078 | GetRules null type | Type=null | ArgumentNullException |
| NEG-079 | Reorder invalid order | Order negative | ArgumentException |
| NEG-080 | SetDefault non-existent | Type invalid | KeyNotFoundException |
| NEG-081 | RemoveDefault no default | No default set | InvalidOperationException |
| NEG-082 | GetByCode null code | Code=null | ArgumentNullException |
| NEG-083 | BulkGet null IDs | Ids=null | ArgumentNullException |
| NEG-084 | ValidateFileSize null type | Type=null | ArgumentNullException |
| NEG-085 | GetCategory null type | Type=null | ArgumentNullException |
| NEG-086 | CloneType null source | Source=null | ArgumentNullException |
| NEG-087 | MergeTypes same type | Same ID | ArgumentException |
| NEG-088 | GetStatistics invalid range | End<Start | ArgumentException |
| NEG-089 | SyncFromExternal null source | Source=null | ArgumentNullException |
| NEG-090 | ValidateConfig null config | Config=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min length | Length=1 | Valid |
| BND-002 | Name at max length | Length=200 | Valid |
| BND-003 | Name exceeds max | Length=201 | Reject |
| BND-004 | MIME at max length | Length=127 | Valid |
| BND-005 | MIME over max | Length=128 | Reject |
| BND-006 | Extension at max | Length=20 | Valid |
| BND-007 | Extension over max | Length=21 | Reject |
| BND-008 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-009 | Page size at min | PageSize=1 | Valid |
| BND-010 | Page size at max | PageSize=1000 | Valid |
| BND-011 | Page size over max | PageSize=1001 | Reject |
| BND-012 | Max file size zero | Size=0 | Reject |
| BND-013 | Max file size at limit | Size=limit | Valid |
| BND-014 | Max file size over limit | Size=limit+1 | Reject |
| BND-015 | Unicode in name | Arabic/Chinese | Stored |
| BND-016 | Special chars in MIME | MIME spec | Valid |
| BND-017 | Extension with dot | .pdf | Handled |
| BND-018 | Extension without dot | pdf | Handled |
| BND-019 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-020 | Empty category | Category="" | Valid |
| BND-021 | Single MIME | Count=1 | Valid |
| BND-022 | Max MIMEs per type | At limit | Valid |
| BND-023 | Single extension | Count=1 | Valid |
| BND-024 | Max extensions per type | At limit | Valid |
| BND-025 | Date at min | Date=MinValue | Handle |
| BND-026 | Date at max | Date=MaxValue | Handle |
| BND-027 | Empty search term | Term="" | Return all |
| BND-028 | Search term max | Term=500 | Valid |
| BND-029 | Search term over max | Term=501 | Reject |
| BND-030 | Collection empty | [] | No exception |
| BND-031 | Collection single | 1 item | Valid |
| BND-032 | Collection max | At limit | Valid |
| BND-033 | Pagination last partial | Partial page | Correct |
| BND-034 | Pagination total | Total count | Accurate |
| BND-035 | Sort null handling | Nulls in data | Deterministic |
| BND-036 | Filter combination all | All filters | Correct |
| BND-037 | Category enum first | First | Valid |
| BND-038 | Category enum last | Last | Valid |
| BND-039 | Zero max size | MaxSize=0 | Reject |
| BND-040 | Negative max size | MaxSize=-1 | Reject |
| BND-041 | Max int for ID | Id=2147483647 | Handle |
| BND-042 | Import row max | Max rows | Valid or reject |
| BND-043 | Import empty file | 0 rows | Empty or error |
| BND-044 | Export large result | 10k rows | Stream |
| BND-045 | MIME case | image/JPEG | Case handle |
| BND-046 | Extension case | .PDF | Case handle |
| BND-047 | Soft delete boundary | DeletedDate set | Excluded |
| BND-048 | Include depth | Deep include | No explosion |
| BND-049 | Query timeout | Slow query | Timeout |
| BND-050 | Memory large result | 10k rows | No OOM |
| BND-051 | Audit timestamp precision | Millisecond | Stored |
| BND-052 | Long string in description | 4000 chars | Truncate |
| BND-053 | Hierarchy depth max | At limit | Valid |
| BND-054 | Hierarchy empty | No children | Empty |
| BND-055 | GetDefault multiple | Multiple defaults | First or config |
| BND-056 | GetAllowed empty | No types | Empty list |
| BND-057 | GetDropdown empty | No types | Empty list |
| BND-058 | AddMime duplicate | MIME exists | Reject |
| BND-059 | AddExtension duplicate | Ext exists | Reject |
| BND-060 | RemoveMime last | Last MIME | Reject or allow |
| BND-061 | RemoveExtension last | Last ext | Reject or allow |
| BND-062 | GetForEntity empty | No types | Empty list |
| BND-063 | GetByMime multiple types | Same MIME | First or config |
| BND-064 | GetByExtension multiple | Same ext | First or config |
| BND-065 | Validate empty extension | Ext="" | False |
| BND-066 | Validate empty MIME | MIME="" | False |
| BND-067 | GetRules empty | No rules | Empty |
| BND-068 | GetMaxSize unconfigured | No size | Default or error |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |
| BND-071 | Name exactly 200 chars | Length=200 | Valid |
| BND-072 | MIME exactly 127 chars | Length=127 | Valid |
| BND-073 | Extension exactly 20 chars | Length=20 | Valid |
| BND-074 | Page 1 first | Page=1 | First page |
| BND-075 | Page at last | Page=last | Last page |
| BND-076 | Zero results | No match | Empty list |
| BND-077 | Single result | One match | Single item |
| BND-078 | Int32.MinValue ID | Id=min | Reject or handle |
| BND-079 | Nullable parent zero | ParentId=0 | Root |
| BND-080 | Order at zero | Order=0 | Valid |
| BND-081 | Order at max | Order=max | Valid |
| BND-082 | Locale empty | Locale="" | Default |
| BND-083 | Locale max length | Locale length | Valid |
| BND-084 | Code min length | Length=1 | Valid |
| BND-085 | Code max length | At limit | Valid |
| BND-086 | Description max | 4000 chars | Truncate |
| BND-087 | Batch size min | Batch=1 | Valid |
| BND-088 | Batch size max | Batch=limit | Valid |
| BND-089 | Cache TTL zero | TTL=0 | No cache |
| BND-090 | Cache TTL max | TTL=max | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | MIME required | Validation | Create | Reject if empty |
| FUN-003 | Extension required | Validation | Create | Reject if empty |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Name unique | Constraint | Create | Reject duplicate |
| FUN-008 | MIME unique per type | Constraint | AddMime | Reject duplicate |
| FUN-009 | Extension unique per type | Constraint | AddExtension | Reject duplicate |
| FUN-010 | Audit CreatedBy | Audit | Create | Set user |
| FUN-011 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-012 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-013 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-014 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-015 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | MIME format validation | Validation | AddMime | Format check |
| FUN-018 | Extension format validation | Validation | AddExtension | Format check |
| FUN-019 | Max size positive | Constraint | SetMaxSize | Reject negative |
| FUN-020 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-021 | GetByMime excludes deleted | Constraint | GetByMime | Excludes deleted |
| FUN-022 | GetByExtension excludes deleted | Constraint | GetByExtension | Excludes deleted |
| FUN-023 | GetDropdown active only | Filter | GetDropdown | Active |
| FUN-024 | GetAllowed configured only | Filter | GetAllowed | Configured |
| FUN-025 | Pagination offset | Calculation | Page | Skip correct |
| FUN-026 | Total count accurate | Calculation | Count | Matches |
| FUN-027 | Sort applies | Calculation | Sort | Ordered |
| FUN-028 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-029 | Delete blocks if documents | Constraint | Delete | Reject |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on update | Transaction | Update | Atomic |
| FUN-032 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-033 | Async all operations | Concurrency | All | Async |
| FUN-034 | Include loads category | Data load | GetById include | Category loaded |
| FUN-035 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-036 | MIME to extension map | Logic | GetExtension | Mapped |
| FUN-037 | Extension to MIME map | Logic | GetMime | Mapped |
| FUN-038 | Validate uses whitelist | Logic | Validate | Whitelist |
| FUN-039 | GetMaxSize from config | Logic | GetMaxSize | From config |
| FUN-040 | GetRules from config | Logic | GetRules | From config |
| FUN-041 | Default type logic | Logic | GetDefault | Config-based |
| FUN-042 | Category hierarchy | Logic | GetHierarchy | Tree |
| FUN-043 | Import mapping | Validation | Import | Columns mapped |
| FUN-044 | Export format | Format | Export | Correct format |
| FUN-045 | Reserved types blocked | Constraint | Create | Reject reserved |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | AddMime creates mapping | Data | AddMime | Mapping |
| FUN-048 | RemoveMime deletes mapping | Data | RemoveMime | Removed |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | GetByType returns correct type | Data | GetByType | Correct type |
| FUN-052 | GetByCode case handling | Config | GetByCode | Per config |
| FUN-053 | Bulk get preserves order | Data | GetByIds | Order preserved |
| FUN-054 | Validate reference exists | Validation | Validate | Exists check |
| FUN-055 | GetDisplayValue format | Format | GetDisplayValue | Formatted |
| FUN-056 | Cache invalidation on create | Cache | Create | Invalidated |
| FUN-057 | Cache invalidation on update | Cache | Update | Invalidated |
| FUN-058 | Cache invalidation on delete | Cache | Delete | Invalidated |
| FUN-059 | Typeahead partial match | Search | Typeahead | Partial |
| FUN-060 | Typeahead limit | Constraint | Typeahead | Capped |
| FUN-061 | Reorder updates sequence | Update | Reorder | Order updated |
| FUN-062 | GetActive filters status | Filter | GetActive | Active only |
| FUN-063 | GetInactive includes inactive | Filter | GetInactive | Inactive |
| FUN-064 | Hierarchy depth limit | Constraint | GetHierarchy | Max depth |
| FUN-065 | Import duplicate handling | Validation | Import | Duplicate |
| FUN-066 | Export headers correct | Format | Export | Headers |
| FUN-067 | Localized fallback | i18n | GetWithLocale | Fallback |
| FUN-068 | Parent must exist | Constraint | Create | Reject invalid |
| FUN-069 | No circular parent | Constraint | Update | Reject |
| FUN-070 | Code unique per type | Constraint | Create | Reject duplicate |
| FUN-071 | Search case handling | Config | Search | Per config |
| FUN-072 | Filter by status | Filter | List | Status filter |
| FUN-073 | Filter by category | Filter | List | Category filter |
| FUN-074 | Sort multi-column | Calculation | Sort | Multi-col |
| FUN-075 | Pagination total pages | Calculation | Page | Total correct |
| FUN-076 | GetByIds dedup | Data | GetByIds | No duplicates |
| FUN-077 | Clone preserves config | Data | Clone | Config preserved |
| FUN-078 | Merge combines MIMEs | Data | Merge | Combined |
| FUN-079 | Sync updates from source | Data | Sync | Updated |
| FUN-080 | Statistics aggregation | Calculation | GetStatistics | Correct |
| FUN-081 | ValidateConfig checks | Validation | ValidateConfig | Valid |
| FUN-082 | GetCategory hierarchy | Data | GetCategory | Hierarchy |
| FUN-083 | ValidateFileSize limit | Validation | ValidateFileSize | Limit |
| FUN-084 | SetDefault updates | Update | SetDefault | Updated |
| FUN-085 | RemoveDefault clears | Update | RemoveDefault | Cleared |
| FUN-086 | Reorder validates | Validation | Reorder | Valid order |
| FUN-087 | Import rollback on error | Transaction | Import | Rollback |
| FUN-088 | Export streaming | Format | Export | Stream |
| FUN-089 | GetByType cache key | Cache | GetByType | Key correct |
| FUN-090 | Typeahead min chars | Constraint | Typeahead | Min chars |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create type full flow | Create | DocumentType | Created |
| INT-002 | Update type full flow | Update | DocumentType | Updated |
| INT-003 | Delete type full flow | Delete | DocumentType | Soft deleted |
| INT-004 | Get with category | GetById | DocumentType, Category | Category loaded |
| INT-005 | List with filter and sort | List | DocumentType | Filtered, sorted |
| INT-006 | GetByMime | GetByMime | DocumentType | Type |
| INT-007 | GetByExtension | GetByExtension | DocumentType | Type |
| INT-008 | Validate file | Validate | DocumentType | Valid |
| INT-009 | GetMime | GetMime | DocumentType | MIME |
| INT-010 | GetExtension | GetExtension | DocumentType | Extension |
| INT-011 | GetDropdown | GetDropdown | DocumentType | Options |
| INT-012 | GetAllowed | GetAllowed | DocumentType | Allowed |
| INT-013 | Pagination | Paginate | DocumentType | Pages |
| INT-014 | AddMime | AddMime | DocumentType | Added |
| INT-015 | RemoveMime | RemoveMime | DocumentType | Removed |
| INT-016 | AddExtension | AddExtension | DocumentType | Added |
| INT-017 | RemoveExtension | RemoveExtension | DocumentType | Removed |
| INT-018 | Import CSV | Import | DocumentType | Imported |
| INT-019 | Export CSV | Export | DocumentType | Exported |
| INT-020 | Type-Category relationship | Relationship | DocumentType, Category | FK valid |
| INT-021 | Type-MIME relationship | Relationship | DocumentType, MIME | Valid |
| INT-022 | Type-Extension relationship | Relationship | DocumentType, Extension | Valid |
| INT-023 | Cascade soft delete | Relationship | Category deleted | Config |
| INT-024 | Delete blocks documents | Relationship | Documents exist | Reject |
| INT-025 | DB error handling | Error | DB down | Graceful |
| INT-026 | Timeout handling | Error | Slow DB | Timeout |
| INT-027 | Constraint violation | Error | FK violation | Clear error |
| INT-028 | Unique violation | Error | Duplicate | Clear error |
| INT-029 | Permission service integration | Integration | Permission | Check |
| INT-030 | User resolver integration | Integration | User | Resolved |
| INT-031 | Audit context integration | Integration | Audit | Context |
| INT-032 | Logger integration | Integration | Log | Logged |
| INT-033 | DocumentManager integration | Integration | DocumentManager | Types |
| INT-034 | Mapper integration | Integration | Map | Correct |
| INT-035 | Repository integration | Integration | Repository | CRUD |
| INT-036 | DbContext integration | Integration | DbContext | Scoped |
| INT-037 | Transaction scope | Integration | Transaction | Atomic |
| INT-038 | Multiple MIMEs per type | Scenario | DocumentType | All MIMEs |
| INT-039 | Multiple extensions | Scenario | DocumentType | All extensions |
| INT-040 | Concurrent create | Scenario | Parallel | All created |
| INT-041 | Import with validation | Scenario | Import | Validated |
| INT-042 | Export with filter | Scenario | Export | Filtered |
| INT-043 | GetForEntity | Scenario | DocumentType, Entity | Types |
| INT-044 | GetHierarchy | Scenario | DocumentType | Tree |
| INT-045 | GetDefault | Scenario | DocumentType | Default |
| INT-046 | Validate with rules | Scenario | Validate | Rules applied |
| INT-047 | GetMaxSize per type | Scenario | GetMaxSize | Per type |
| INT-048 | GetRules per type | Scenario | GetRules | Per type |
| INT-049 | Add remove MIME cycle | Scenario | AddMime, RemoveMime | Clean |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |
| INT-051 | GetByType with cache | Scenario | GetByType | Cached |
| INT-052 | Typeahead full flow | Scenario | Typeahead | Matches |
| INT-053 | Bulk get by IDs | Scenario | GetByIds | All returned |
| INT-054 | Reorder full flow | Scenario | Reorder | Reordered |
| INT-055 | GetDisplayValue flow | Scenario | GetDisplayValue | Display |
| INT-056 | Validate reference flow | Scenario | Validate | True |
| INT-057 | GetCountries flow | Scenario | Country | Countries |
| INT-058 | GetRegions flow | Scenario | Region | Regions |
| INT-059 | Get localized flow | Scenario | GetWithLocale | Localized |
| INT-060 | Import export round-trip | Scenario | Import, Export | Match |
| INT-061 | Cache refresh flow | Scenario | Refresh | Updated |
| INT-062 | Clone type flow | Scenario | Clone | Cloned |
| INT-063 | Merge types flow | Scenario | Merge | Merged |
| INT-064 | SetDefault flow | Scenario | SetDefault | Set |
| INT-065 | RemoveDefault flow | Scenario | RemoveDefault | Cleared |
| INT-066 | GetStatistics flow | Scenario | GetStatistics | Stats |
| INT-067 | ValidateConfig flow | Scenario | ValidateConfig | Valid |
| INT-068 | Sync from external | Scenario | Sync | Synced |
| INT-069 | Config service integration | Integration | Config | Read |
| INT-070 | Cache service integration | Integration | Cache | Hit/miss |
| INT-071 | Multiple entity types | Scenario | DocumentType | Multiple |
| INT-072 | Hierarchy deep load | Scenario | GetHierarchy | Deep |
| INT-073 | Pagination with filter | Scenario | Paginate | Filtered |
| INT-074 | Sort with filter | Scenario | List | Sorted, filtered |
| INT-075 | Search typeahead | Scenario | Typeahead | Results |
| INT-076 | GetActive inactive | Scenario | GetActive, GetInactive | Correct |
| INT-077 | Bulk create types | Scenario | Create | All created |
| INT-078 | Bulk update types | Scenario | Update | All updated |
| INT-079 | Concurrent read | Scenario | Parallel | No conflict |
| INT-080 | Error recovery | Scenario | Error | Recover |
| INT-081 | Audit trail full | Scenario | CRUD | Full trail |
| INT-082 | Permission integration | Scenario | Permission | Enforced |
| INT-083 | User context integration | Scenario | User | Context |
| INT-084 | Logger integration flow | Scenario | Log | Logged |
| INT-085 | Mapper round-trip | Scenario | Map | Correct |
| INT-086 | Repository CRUD cycle | Scenario | Repository | CRUD |
| INT-087 | DbContext scoping | Scenario | DbContext | Scoped |
| INT-088 | Transaction rollback | Scenario | Transaction | Rollback |
| INT-089 | Full import export | Scenario | Import, Export | Complete |
| INT-090 | E2E with all features | Scenario | Full | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in name | '; DROP TABLE-- | Name | Sanitized |
| SEC-002 | SQL injection in MIME | ' OR '1'='1 | MIME | Rejected |
| SEC-003 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-004 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls | Any | Rejected |
| SEC-009 | Path traversal in extension | ../ | Extension | Rejected |
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
| SEC-022 | IDOR in filter | Category=other | List | Filtered |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-025 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-026 | Mass assign MIME | MIME=manipulated | Request | Validated |
| SEC-027 | Session hijack | Stolen token | Any | Detected |
| SEC-028 | Token expiration | Expired | Any | 401 |
| SEC-029 | Invalid token | Malformed | Any | 401 |
| SEC-030 | CSRF on create | No token | Create | Rejected |
| SEC-031 | CSRF on update | No token | Update | Rejected |
| SEC-032 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-033 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-034 | Rate limit create | Many creates | Create | Throttled |
| SEC-035 | Rate limit list | Many lists | List | Throttled |
| SEC-036 | Rate limit validate | Many validates | Validate | Throttled |
| SEC-037 | Oversized request | 10MB payload | Create | Rejected |
| SEC-038 | Deep nesting | Nested object | Request | Rejected |
| SEC-039 | Header injection | \r\n in header | Header | Rejected |
| SEC-040 | Null byte injection | %00 in name | Name | Rejected |
| SEC-041 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-042 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-043 | Denial of service | Huge page size | List | Capped |
| SEC-044 | Import malicious CSV | Malicious | Import | Rejected |
| SEC-045 | Export data injection | Inject in export | Export | Sanitized |
| SEC-046 | Reserved type bypass | Create reserved | Create | Rejected |
| SEC-047 | MIME spoofing | Wrong MIME | AddMime | Rejected |
| SEC-048 | Extension bypass | .exe as .pdf | AddExtension | Rejected |
| SEC-049 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-050 | Permission cached | Repeated check | Permission | Cached |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same name | Two create | One fails |
| CON-004 | Concurrent create | Two create | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on AddMime | Two add same MIME | One fails |
| CON-009 | Race on AddExtension | Two add same ext | One fails |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Import concurrent | Two import | One or both |
| CON-016 | Export concurrent | Two export | Both succeed |
| CON-017 | Validate concurrent | Many validate | All correct |
| CON-018 | GetByMime concurrent | Concurrent get | Consistent |
| CON-019 | GetByExtension concurrent | Concurrent get | Consistent |
| CON-020 | Soft delete concurrent | Delete while update | Deterministic |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Cache invalidation race | Update, read | Consistent |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate MIME format | Validation | Valid MIME | Pass |
| UNT-003 | Validate extension format | Validation | Valid ext | Pass |
| UNT-004 | Validate max size | Validation | Positive | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format MIME display | Formatting | MIME | Display |
| UNT-007 | Format extension display | Formatting | Extension | Display |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | MIME to extension map | Calculation | MIME | Extension |
| UNT-013 | Extension to MIME map | Calculation | Extension | MIME |
| UNT-014 | Type allows create | Status logic | Type | true |
| UNT-015 | Type allows update | Status logic | Type | true |
| UNT-016 | Type allows delete | Status logic | Type | true |
| UNT-017 | Extension allowed check | Status logic | Ext | Allowed |
| UNT-018 | MIME allowed check | Status logic | MIME | Allowed |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | GetByMime | GetByMime | <50ms | P1 |
| PRF-004 | GetByExtension | GetByExtension | <50ms | P1 |
| PRF-005 | Validate | Validate | <10ms | P1 |
| PRF-006 | GetDropdown | GetDropdown | <100ms | P1 |
| PRF-007 | GetAllowed | GetAllowed | <100ms | P1 |
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
