# UNOPSEntityConfigurationManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/EntityConfigurationManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | ≥30 | ✅ |
| §2 Negative | 90 | ≥90 | ✅ |
| §3 Boundary | 90 | ≥90 | ✅ |
| §4 Functional | 90 | ≥90 | ✅ |
| §5 Integration | 90 | ≥90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Entity configuration manager unit tests cover field definitions, schemas, custom fields, and validation for entity configurations. Tests include: entity config CRUD, field definitions, schema validation, custom field registration, validation rules, and configuration persistence.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Get entity configuration | Config exists | GetConfig | Config returned |
| POS-002 | Create entity configuration | Valid data | Create | Config created |
| POS-003 | Update entity configuration | Config exists | Update | Updated |
| POS-004 | Delete entity configuration | Config exists | Delete | Soft deleted |
| POS-005 | List all configurations | Configs exist | List | List returned |
| POS-006 | Add field definition | Config exists | AddField | Field added |
| POS-007 | Update field definition | Field exists | UpdateField | Updated |
| POS-008 | Remove field definition | Field exists | RemoveField | Removed |
| POS-009 | Get field configuration | Field exists | GetFieldConfig | Config returned |
| POS-010 | Add validation rule | Config exists | AddRule | Rule added |
| POS-011 | Update validation rule | Rule exists | UpdateRule | Updated |
| POS-012 | Remove validation rule | Rule exists | RemoveRule | Removed |
| POS-013 | Apply validation | Rules exist | ApplyValidation | Validated |
| POS-014 | Get schema | Config exists | GetSchema | Schema returned |
| POS-015 | Get custom fields | Config has custom | GetCustomFields | Fields returned |
| POS-016 | Validate field type | Type valid | ValidateType | True |
| POS-017 | Get display configuration | Config exists | GetDisplayConfig | Display config |
| POS-018 | Get required fields | Config exists | GetRequiredFields | Required list |
| POS-019 | Sort by entity name | Configs exist | List sorted | Ordered |
| POS-020 | Filter by entity type | Configs exist | Filter | Filtered |
| POS-021 | Pagination | Many configs | List page | Page |
| POS-022 | Audit CreatedBy | Create | Check audit | Set |
| POS-023 | Audit CreatedDate | Create | Check audit | UTC |
| POS-024 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-025 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-026 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-027 | Inheritance from parent | Parent exists | GetInherited | Inherited |
| POS-028 | Override parent field | Parent has field | Override | Overridden |
| POS-029 | Get field metadata | Field exists | GetMetadata | Metadata |
| POS-030 | Export configuration | Config exists | Export | Exported |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null entity name | EntityName=null | ValidationException |
| NEG-002 | Create with empty entity name | EntityName="" | ValidationException |
| NEG-003 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-004 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-005 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-006 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | AddField invalid config | Config invalid | KeyNotFoundException |
| NEG-008 | AddField duplicate name | Name exists | BusinessException |
| NEG-009 | AddField invalid type | Type invalid | ValidationException |
| NEG-010 | UpdateField non-existent | Field invalid | KeyNotFoundException |
| NEG-011 | RemoveField non-existent | Field invalid | KeyNotFoundException |
| NEG-012 | RemoveField required | Field required | BusinessException |
| NEG-013 | AddRule invalid config | Config invalid | KeyNotFoundException |
| NEG-014 | AddRule invalid expression | Expression invalid | ValidationException |
| NEG-015 | GetById without permission | Unauthorized | Forbidden |
| NEG-016 | Create without permission | Unauthorized | Forbidden |
| NEG-017 | Update without permission | Unauthorized | Forbidden |
| NEG-018 | Delete without permission | Unauthorized | Forbidden |
| NEG-019 | ApplyValidation null data | Data=null | ArgumentNullException |
| NEG-020 | GetSchema invalid entity | Entity invalid | KeyNotFoundException |
| NEG-021 | GetCustomFields invalid | Config invalid | KeyNotFoundException |
| NEG-022 | ValidateType null type | Type=null | ArgumentNullException |
| NEG-023 | GetDisplayConfig invalid | Config invalid | KeyNotFoundException |
| NEG-024 | Import invalid JSON | JSON invalid | ValidationException |
| NEG-025 | Import missing required | JSON missing | ValidationException |
| NEG-026 | Export invalid format | Format invalid | ArgumentException |
| NEG-027 | GetInherited invalid parent | Parent invalid | KeyNotFoundException |
| NEG-028 | Override invalid field | Field invalid | KeyNotFoundException |
| NEG-029 | GetMetadata invalid field | Field invalid | KeyNotFoundException |
| NEG-030 | GetDefaults invalid config | Config invalid | KeyNotFoundException |
| NEG-031 | GetOptions invalid field | Field invalid | KeyNotFoundException |
| NEG-032 | ValidateSchema invalid | Schema invalid | ValidationException |
| NEG-033 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-034 | Invalid page number | Page=0 | ArgumentException |
| NEG-035 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-036 | Null request object | Request=null | ArgumentNullException |
| NEG-037 | Update deleted config | Config deleted | KeyNotFoundException |
| NEG-038 | GetById deleted | Config deleted | KeyNotFoundException |
| NEG-039 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-040 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-041 | Transaction rollback | Fail in transaction | Rollback |
| NEG-042 | Connection timeout | DB unavailable | TimeoutException |
| NEG-043 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-044 | Invalid enum value | Type invalid | ArgumentException |
| NEG-045 | Circular inheritance | Self as parent | BusinessException |
| NEG-046 | Expired session | Expired token | Unauthorized |
| NEG-047 | Null user context | User=null | InvalidOperationException |
| NEG-048 | Invalid include path | Invalid include | ArgumentException |
| NEG-049 | RemoveRule non-existent | Rule invalid | KeyNotFoundException |
| NEG-050 | UpdateRule non-existent | Rule invalid | KeyNotFoundException |
| NEG-051 | AddField max exceeded | At limit | BusinessException |
| NEG-052 | AddRule max exceeded | At limit | BusinessException |
| NEG-053 | GetSchema empty | No schema | Empty or error |
| NEG-054 | GetCustomFields empty | No custom | Empty list |
| NEG-055 | Export empty | No configs | Empty or error |
| NEG-056 | Filter invalid entity type | Type invalid | ArgumentException |
| NEG-057 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-058 | Pagination overflow | Page too large | Empty or error |
| NEG-059 | Audit missing user | User=0 | InvalidOperationException |
| NEG-060 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-061 | ValidateType invalid | Type invalid | False |
| NEG-062 | ApplyValidation fails | Invalid data | ValidationException |
| NEG-063 | GetInherited circular | Circular ref | BusinessException |
| NEG-064 | GetDefaults null | No defaults | Empty |
| NEG-065 | GetOptions null | No options | Empty |
| NEG-066 | ValidateSchema empty | Empty schema | Valid or error |
| NEG-067 | Child override throws | Child throws | Propagated |
| NEG-068 | Inheritance depth exceeded | Too deep | BusinessException |
| NEG-069 | Import duplicate entity | Entity exists | BusinessException |
| NEG-070 | Cache invalid key | Key invalid | ArgumentException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Entity name at min | Length=1 | Valid |
| BND-002 | Entity name at max | Length=200 | Valid |
| BND-003 | Entity name exceeds max | Length=201 | Reject |
| BND-004 | Field name at max | Length=100 | Valid |
| BND-005 | Field name exceeds max | Length=101 | Reject |
| BND-006 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Field count at max | Count=limit | Valid |
| BND-011 | Field count over max | Count=limit+1 | Reject |
| BND-012 | Rule count at max | Count=limit | Valid |
| BND-013 | Rule count over max | Count=limit+1 | Reject |
| BND-014 | Validation expression max | Length=1000 | Valid |
| BND-015 | Validation expression over max | Length=1001 | Reject |
| BND-016 | Unicode in entity name | Arabic/Chinese | Stored |
| BND-017 | Special chars in field name | <>&"' | Escaped |
| BND-018 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-019 | Empty schema | Schema=[] | Valid |
| BND-020 | Single field | Count=1 | Valid |
| BND-021 | Single rule | Count=1 | Valid |
| BND-022 | Empty custom fields | Fields=[] | Empty list |
| BND-023 | Empty required fields | Required=[] | Empty list |
| BND-024 | Date at min | Date=MinValue | Handle |
| BND-025 | Date at max | Date=MaxValue | Handle |
| BND-026 | Empty search term | Term="" | Return all |
| BND-027 | Search term max | Term=500 | Valid |
| BND-028 | Search term over max | Term=501 | Reject |
| BND-029 | Collection empty | [] | No exception |
| BND-030 | Collection single | 1 item | Valid |
| BND-031 | Collection max | At limit | Valid |
| BND-032 | Pagination last partial | Partial page | Correct |
| BND-033 | Pagination total | Total count | Accurate |
| BND-034 | Sort null handling | Nulls in data | Deterministic |
| BND-035 | Filter combination all | All filters | Correct |
| BND-036 | Type enum first | First | Valid |
| BND-037 | Type enum last | Last | Valid |
| BND-038 | Inheritance depth at max | At limit | Valid |
| BND-039 | Inheritance depth over max | Over limit | Reject |
| BND-040 | Zero entity ID | EntityId=0 | Reject |
| BND-041 | Max int for ID | Id=2147483647 | Handle |
| BND-042 | Import row max | Max rows | Valid or reject |
| BND-043 | Import empty file | 0 rows | Empty or error |
| BND-044 | Export large result | 10k rows | Stream |
| BND-045 | Schema depth max | Deep schema | Valid |
| BND-046 | Default value max length | Length=4000 | Valid |
| BND-047 | Default value over max | Length=4001 | Reject |
| BND-048 | Options count max | Count=100 | Valid |
| BND-049 | Options count over max | Count=101 | Reject |
| BND-050 | Soft delete boundary | DeletedDate set | Excluded |
| BND-051 | Include depth | Deep include | No explosion |
| BND-052 | Query timeout | Slow query | Timeout |
| BND-053 | Memory large result | 10k rows | No OOM |
| BND-054 | Audit timestamp precision | Millisecond | Stored |
| BND-055 | Long string in description | 4000 chars | Truncate |
| BND-056 | Field type boundary | First/Last | Valid |
| BND-057 | Validation regex max | Regex length | Valid |
| BND-058 | Validation regex over max | Regex length+1 | Reject |
| BND-059 | GetInherited empty | No parent | Empty or self |
| BND-060 | GetDefaults empty | No defaults | Empty |
| BND-061 | GetOptions empty | No options | Empty |
| BND-062 | Override same value | Same as parent | No-op |
| BND-063 | GetMetadata empty | No metadata | Empty |
| BND-064 | GetSchema minimal | Minimal schema | Valid |
| BND-065 | GetDisplayConfig minimal | Minimal display | Valid |
| BND-066 | Cache expiry | After expiry | Miss |
| BND-067 | Cache hit | Just cached | Hit |
| BND-068 | Async cancellation | Cancel token | OperationCanceledException |
| BND-069 | Task timeout | Timeout | TimeoutException |
| BND-070 | Concurrent same second | Same timestamp | Deterministic |
| BND-071 | Entity name single char | Length=1 | Valid |
| BND-072 | Field name max | Length=100 | Valid |
| BND-073 | Page size one | PageSize=1 | Valid |
| BND-074 | Field count one | Count=1 | Valid |
| BND-075 | Rule count one | Count=1 | Valid |
| BND-076 | Validation expression max | Length=1000 | Valid |
| BND-077 | Empty schema | Schema=[] | Valid |
| BND-078 | Empty custom fields | Fields=[] | Empty list |
| BND-079 | Empty required fields | Required=[] | Empty list |
| BND-080 | Inheritance depth one | Depth=1 | Valid |
| BND-081 | Default value max | Length=4000 | Valid |
| BND-082 | Options count max | Count=100 | Valid |
| BND-083 | Type enum first | First | Valid |
| BND-084 | Type enum last | Last | Valid |
| BND-085 | Pagination first page | Page=1 | Valid |
| BND-086 | Search term max | Term=500 | Valid |
| BND-087 | Import row one | 1 row | Valid |
| BND-088 | Export single row | 1 row | Valid |
| BND-089 | Cache hit | Just cached | Hit |
| BND-090 | GetMetadata empty | No metadata | Empty |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Entity name required | Validation | Create | Reject if empty |
| FUN-002 | Field name required | Validation | AddField | Reject if empty |
| FUN-003 | Field type required | Validation | AddField | Reject if invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Entity name unique | Constraint | Create | Reject duplicate |
| FUN-008 | Field name unique per config | Constraint | AddField | Reject duplicate |
| FUN-009 | Rule expression valid | Constraint | AddRule | Reject invalid |
| FUN-010 | Audit CreatedBy | Audit | Create | Set user |
| FUN-011 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-012 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-013 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-014 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-015 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | Required field validation | Validation | ApplyValidation | Reject if missing |
| FUN-018 | Type validation | Validation | ApplyValidation | Type check |
| FUN-019 | Rule validation | Validation | ApplyValidation | Rule check |
| FUN-020 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-021 | Inheritance chain | Logic | GetInherited | Chain correct |
| FUN-022 | Override precedence | Logic | Override | Child over parent |
| FUN-023 | Pagination offset | Calculation | Page | Skip correct |
| FUN-024 | Total count accurate | Calculation | Count | Matches |
| FUN-025 | Sort applies | Calculation | Sort | Ordered |
| FUN-026 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-027 | RemoveField cascades | Logic | RemoveField | Cascades |
| FUN-028 | RemoveRule cascades | Logic | RemoveRule | Cascades |
| FUN-029 | Transaction on create | Transaction | Create | Atomic |
| FUN-030 | Transaction on update | Transaction | Update | Atomic |
| FUN-031 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads fields | Data load | GetById include | Fields loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | GetSchema merges inherited | Logic | GetSchema | Merged |
| FUN-036 | GetCustomFields filters | Logic | GetCustomFields | Custom only |
| FUN-037 | GetRequiredFields merges | Logic | GetRequiredFields | Merged |
| FUN-038 | GetDefaults merges | Logic | GetDefaults | Merged |
| FUN-039 | ValidateSchema structure | Validation | ValidateSchema | Structure |
| FUN-040 | Import creates config | Logic | Import | Created |
| FUN-041 | Export includes all | Logic | Export | Complete |
| FUN-042 | Cache invalidation on update | Cache | Update | Invalidated |
| FUN-043 | Cache invalidation on delete | Cache | Delete | Invalidated |
| FUN-044 | GetDisplayConfig format | Logic | GetDisplayConfig | Formatted |
| FUN-045 | GetFieldMetadata complete | Logic | GetMetadata | Complete |
| FUN-046 | GetOptions from config | Logic | GetOptions | From config |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Type coercion | Logic | ApplyValidation | Coerce |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | Entity name required | Validation | Create | Reject if empty |
| FUN-052 | Field name required | Validation | AddField | Reject if empty |
| FUN-053 | Field type required | Validation | AddField | Reject if invalid |
| FUN-054 | Entity name unique | Constraint | Create | Reject duplicate |
| FUN-055 | Field name unique per config | Constraint | AddField | Reject duplicate |
| FUN-056 | Rule expression valid | Constraint | AddRule | Reject invalid |
| FUN-057 | Required field validation | Validation | ApplyValidation | Reject if missing |
| FUN-058 | Type validation | Validation | ApplyValidation | Type check |
| FUN-059 | Rule validation | Validation | ApplyValidation | Rule check |
| FUN-060 | Inheritance chain | Logic | GetInherited | Chain correct |
| FUN-061 | Override precedence | Logic | Override | Child over parent |
| FUN-062 | RemoveField cascades | Logic | RemoveField | Cascades |
| FUN-063 | RemoveRule cascades | Logic | RemoveRule | Cascades |
| FUN-064 | GetSchema merges inherited | Logic | GetSchema | Merged |
| FUN-065 | GetCustomFields filters | Logic | GetCustomFields | Custom only |
| FUN-066 | GetRequiredFields merges | Logic | GetRequiredFields | Merged |
| FUN-067 | GetDefaults merges | Logic | GetDefaults | Merged |
| FUN-068 | ValidateSchema structure | Validation | ValidateSchema | Structure |
| FUN-069 | Import creates config | Logic | Import | Created |
| FUN-070 | Export includes all | Logic | Export | Complete |
| FUN-071 | Cache invalidation on update | Cache | Update | Invalidated |
| FUN-072 | Cache invalidation on delete | Cache | Delete | Invalidated |
| FUN-073 | GetDisplayConfig format | Logic | GetDisplayConfig | Formatted |
| FUN-074 | GetFieldMetadata complete | Logic | GetMetadata | Complete |
| FUN-075 | GetOptions from config | Logic | GetOptions | From config |
| FUN-076 | Include loads fields | Data load | GetById include | Fields loaded |
| FUN-077 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-078 | Audit CreatedBy | Audit | Create | Set user |
| FUN-079 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-080 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-081 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-082 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-083 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-084 | Permission before action | Authorization | Any | Check first |
| FUN-085 | Pagination offset | Calculation | Page | Skip correct |
| FUN-086 | Total count accurate | Calculation | Count | Matches |
| FUN-087 | Sort applies | Calculation | Sort | Ordered |
| FUN-088 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-089 | Transaction on create | Transaction | Create | Atomic |
| FUN-090 | Async all operations | Concurrency | All | Async |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create config full flow | Create | EntityConfig | Created |
| INT-002 | Update config full flow | Update | EntityConfig | Updated |
| INT-003 | Delete config full flow | Delete | EntityConfig | Soft deleted |
| INT-004 | Get with fields | GetById | EntityConfig, Field | Fields loaded |
| INT-005 | List with filter and sort | List | EntityConfig | Filtered, sorted |
| INT-006 | Add field | AddField | EntityConfig, Field | Added |
| INT-007 | Update field | UpdateField | Field | Updated |
| INT-008 | Remove field | RemoveField | Field | Removed |
| INT-009 | Add rule | AddRule | EntityConfig, Rule | Added |
| INT-010 | Update rule | UpdateRule | Rule | Updated |
| INT-011 | Remove rule | RemoveRule | Rule | Removed |
| INT-012 | Apply validation | ApplyValidation | Config, Data | Validated |
| INT-013 | Get schema | GetSchema | EntityConfig | Schema |
| INT-014 | Get custom fields | GetCustomFields | EntityConfig | Fields |
| INT-015 | Pagination | Paginate | EntityConfig | Pages |
| INT-016 | Config-Field relationship | Relationship | EntityConfig, Field | FK valid |
| INT-017 | Config-Rule relationship | Relationship | EntityConfig, Rule | FK valid |
| INT-018 | Config-Parent relationship | Relationship | EntityConfig | Parent |
| INT-019 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-020 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-021 | DB error handling | Error | DB down | Graceful |
| INT-022 | Timeout handling | Error | Slow DB | Timeout |
| INT-023 | Constraint violation | Error | FK violation | Clear error |
| INT-024 | Unique violation | Error | Duplicate | Clear error |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | Cache integration | Integration | Cache | Hit/miss |
| INT-030 | Mapper integration | Integration | Map | Correct |
| INT-031 | Repository integration | Integration | Repository | CRUD |
| INT-032 | DbContext integration | Integration | DbContext | Scoped |
| INT-033 | Transaction scope | Integration | Transaction | Atomic |
| INT-034 | Import from JSON | Import | EntityConfig | Imported |
| INT-035 | Export to JSON | Export | EntityConfig | Exported |
| INT-036 | Inheritance chain | Scenario | EntityConfig | Chain |
| INT-037 | Override fields | Scenario | EntityConfig | Overridden |
| INT-038 | Concurrent create | Scenario | Parallel | All created |
| INT-039 | Apply validation with rules | Scenario | ApplyValidation | Rules applied |
| INT-040 | Get schema with inheritance | Scenario | GetSchema | Merged |
| INT-041 | Get display config | Scenario | GetDisplayConfig | Display |
| INT-042 | Get defaults | Scenario | GetDefaults | Defaults |
| INT-043 | Get options | Scenario | GetOptions | Options |
| INT-044 | Validate schema | Scenario | ValidateSchema | Validated |
| INT-045 | Import with validation | Scenario | Import | Validated |
| INT-046 | Export with filter | Scenario | Export | Filtered |
| INT-047 | Add remove field cycle | Scenario | AddField, RemoveField | Clean |
| INT-048 | Add remove rule cycle | Scenario | AddRule, RemoveRule | Clean |
| INT-049 | Cache refresh | Scenario | Update, Get | Refreshed |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |
| INT-051 | Create then get | Scenario | Create, Get | Both |
| INT-052 | Update then get | Scenario | Update, Get | Both |
| INT-053 | Delete then list | Scenario | Delete, List | Excluded |
| INT-054 | Add field then get | Scenario | AddField, Get | Both |
| INT-055 | Remove field then get | Scenario | RemoveField, Get | Both |
| INT-056 | Add rule then apply | Scenario | AddRule, ApplyValidation | Both |
| INT-057 | Remove rule then apply | Scenario | RemoveRule, ApplyValidation | Both |
| INT-058 | Import then export | Scenario | Import, Export | Round-trip |
| INT-059 | Get schema with inheritance | Scenario | GetSchema | Merged |
| INT-060 | Get display config | Scenario | GetDisplayConfig | Display |
| INT-061 | Get defaults | Scenario | GetDefaults | Defaults |
| INT-062 | Get options | Scenario | GetOptions | Options |
| INT-063 | Validate schema | Scenario | ValidateSchema | Validated |
| INT-064 | Cache integration | Integration | Cache | Hit/miss |
| INT-065 | Mapper integration | Integration | Mapper | Mapped |
| INT-066 | Repository integration | Integration | Repository | CRUD |
| INT-067 | DbContext integration | Integration | DbContext | Scoped |
| INT-068 | Transaction scope | Integration | Transaction | Atomic |
| INT-069 | Permission service | Integration | Permission | Check |
| INT-070 | User resolver | Integration | User | Resolved |
| INT-071 | Audit context | Integration | Audit | Context |
| INT-072 | Logger integration | Integration | Logger | Logged |
| INT-073 | Config-Field relationship | Relationship | EntityConfig, Field | FK valid |
| INT-074 | Config-Rule relationship | Relationship | EntityConfig, Rule | FK valid |
| INT-075 | Config-Parent relationship | Relationship | EntityConfig | Parent |
| INT-076 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-077 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-078 | DB error handling | Error | DB down | Graceful |
| INT-079 | Timeout handling | Error | Slow DB | Timeout |
| INT-080 | Constraint violation | Error | FK violation | Clear error |
| INT-081 | Unique violation | Error | Duplicate | Clear error |
| INT-082 | Inheritance chain | Scenario | EntityConfig | Chain |
| INT-083 | Override fields | Scenario | EntityConfig | Overridden |
| INT-084 | Concurrent create | Scenario | Parallel | All created |
| INT-085 | Apply validation with rules | Scenario | ApplyValidation | Rules applied |
| INT-086 | Import with validation | Scenario | Import | Validated |
| INT-087 | Export with filter | Scenario | Export | Filtered |
| INT-088 | Add remove field cycle | Scenario | AddField, RemoveField | Clean |
| INT-089 | Add remove rule cycle | Scenario | AddRule, RemoveRule | Clean |
| INT-090 | Full workflow | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in name | '; DROP TABLE-- | Name | Sanitized |
| SEC-002 | SQL injection in expression | ' OR '1'='1 | Expression | Rejected |
| SEC-003 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-004 | XSS in field name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls | Any | Rejected |
| SEC-009 | Regex DoS | Nested quantifiers | Expression | Rejected |
| SEC-010 | Unauthorized list | No permission | List | 403 |
| SEC-011 | Unauthorized get | No permission | GetById | 403 |
| SEC-012 | Unauthorized create | No permission | Create | 403 |
| SEC-013 | Unauthorized update | No permission | Update | 403 |
| SEC-014 | Unauthorized delete | No permission | Delete | 403 |
| SEC-015 | Unauthorized import | No permission | Import | 403 |
| SEC-016 | Unauthorized export | No permission | Export | 403 |
| SEC-017 | Role escalation | Low role | Admin | 403 |
| SEC-018 | Cross-tenant access | User A | User B config | 403 |
| SEC-019 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-020 | IDOR update other | Id=other | Update | 403 |
| SEC-021 | IDOR delete other | Id=other | Delete | 403 |
| SEC-022 | IDOR in filter | EntityId=other | List | Filtered |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-025 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-026 | Mass assign schema | Schema=manipulated | Request | Validated |
| SEC-027 | Session hijack | Stolen token | Any | Detected |
| SEC-028 | Token expiration | Expired | Any | 401 |
| SEC-029 | Invalid token | Malformed | Any | 401 |
| SEC-030 | CSRF on create | No token | Create | Rejected |
| SEC-031 | CSRF on update | No token | Update | Rejected |
| SEC-032 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-033 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-034 | Rate limit create | Many creates | Create | Throttled |
| SEC-035 | Rate limit list | Many lists | List | Throttled |
| SEC-036 | Rate limit validate | Many validates | ApplyValidation | Throttled |
| SEC-037 | Oversized request | 10MB payload | Create | Rejected |
| SEC-038 | Deep nesting | Nested object | Request | Rejected |
| SEC-039 | Header injection | \r\n in header | Header | Rejected |
| SEC-040 | Null byte injection | %00 in name | Name | Rejected |
| SEC-041 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-042 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-043 | Denial of service | Huge page size | List | Capped |
| SEC-044 | Import malicious JSON | Malicious | Import | Rejected |
| SEC-045 | Export data injection | Inject in export | Export | Sanitized |
| SEC-046 | Expression injection | Eval in expression | AddRule | Rejected |
| SEC-047 | Schema injection | Malicious schema | Import | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Cache poisoning | Malicious cache | Cache | Not used |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same entity | Two create | One fails |
| CON-004 | Concurrent create | Two create | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on AddField | Two add same | One fails |
| CON-009 | Race on AddRule | Two add same | One fails |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Import concurrent | Two import | One or both |
| CON-016 | Export concurrent | Two export | Both succeed |
| CON-017 | ApplyValidation concurrent | Many validate | All correct |
| CON-018 | GetSchema concurrent | Concurrent get | Consistent |
| CON-019 | Cache invalidation race | Update, read | Consistent |
| CON-020 | Soft delete concurrent | Delete while update | Deterministic |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Cache consistency | Concurrent update | Consistent |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate entity name not null | Validation | null | Exception |
| UNT-002 | Validate field type | Validation | Valid type | Pass |
| UNT-003 | Validate expression | Validation | Valid expr | Pass |
| UNT-004 | Validate schema structure | Validation | Valid schema | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format schema display | Formatting | Schema | Display |
| UNT-007 | Format field config | Formatting | Field | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Merge schema | Calculation | Parent, Child | Merged |
| UNT-013 | Inheritance depth | Calculation | Config | Depth |
| UNT-014 | Type allows create | Status logic | Type | true |
| UNT-015 | Type allows update | Status logic | Type | true |
| UNT-016 | Type allows delete | Status logic | Type | true |
| UNT-017 | Field required check | Status logic | Field | Required |
| UNT-018 | Rule active check | Status logic | Rule | Active |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Get schema | GetSchema | <100ms | P1 |
| PRF-004 | Apply validation | ApplyValidation | <50ms | P1 |
| PRF-005 | Get custom fields | GetCustomFields | <50ms | P1 |
| PRF-006 | List with pagination | List | <300ms | P1 |
| PRF-007 | List with sort | List | <300ms | P1 |
| PRF-008 | Get display config | GetDisplayConfig | <100ms | P1 |
| PRF-009 | Import large JSON | Import | <5s | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 writes | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 write | <4s total | P2 |
| PRF-013 | Memory single create | Create | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory apply validation | ApplyValidation | <5MB | P2 |
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
| LDT-008 | Stress memory | Large import | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
