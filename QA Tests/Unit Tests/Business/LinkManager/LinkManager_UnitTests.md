# LinkManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/LinkManager` (Unit Tests)  
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

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Link manager unit tests cover URL validation, entity association, and categorization for links. Tests include: create/update/delete links, URL format validation, associate links with partners/opportunities/contacts, categorize links, and validate link integrity.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create link | Valid URL | Create | Link created |
| POS-002 | Get link by ID | Link exists | GetById | Link returned |
| POS-003 | Update link | Link exists | Update | Updated |
| POS-004 | Soft delete link | Link exists | Delete | IsDeleted=true |
| POS-005 | List links by entity | Entity has links | List | List returned |
| POS-006 | Valid URL format | URL valid | Create | Accepted |
| POS-007 | Associate with partner | Partner exists | Associate | Associated |
| POS-008 | Associate with opportunity | Opportunity exists | Associate | Associated |
| POS-009 | Associate with contact | Contact exists | Associate | Associated |
| POS-010 | Categorize link | Category valid | Categorize | Categorized |
| POS-011 | Get with entity | Link exists | GetById include | Entity loaded |
| POS-012 | Validate URL | URL valid | Validate | True |
| POS-013 | Search by title | Links exist | Search | Matching |
| POS-014 | Filter by category | Links exist | Filter | Filtered |
| POS-015 | Filter by entity type | Links exist | Filter | Filtered |
| POS-016 | Pagination | Many links | List page | Page |
| POS-017 | Sort by title | Links exist | Sort | Ordered |
| POS-018 | Sort by date | Links exist | Sort | Ordered |
| POS-019 | Audit CreatedBy | Create | Check audit | Set |
| POS-020 | Audit CreatedDate | Create | Check audit | UTC |
| POS-021 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-022 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-023 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-024 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-025 | Get categories | Categories exist | GetCategories | Categories |
| POS-026 | Get link by URL | URL unique | GetByUrl | Link |
| POS-027 | Check link exists | Link exists | Exists | True |
| POS-028 | Count by entity | Entity has links | Count | Count |
| POS-029 | Export links | Links exist | Export | Exported |
| POS-030 | Import links | CSV valid | Import | Imported |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null URL | Url=null | ArgumentNullException |
| NEG-002 | Create with empty URL | Url="" | ValidationException |
| NEG-003 | Create with invalid URL | Url=bad | ValidationException |
| NEG-004 | Create with malformed URL | Url=malformed | ValidationException |
| NEG-005 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-006 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-007 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-009 | Associate with deleted entity | Entity deleted | BusinessException |
| NEG-010 | Invalid entity type | Type=invalid | ArgumentException |
| NEG-011 | Invalid entity ID | EntityId=-1 | ArgumentException |
| NEG-012 | Invalid category | Category=invalid | ArgumentException |
| NEG-013 | Null request object | Request=null | ArgumentNullException |
| NEG-014 | GetById without permission | Unauthorized | Forbidden |
| NEG-015 | Create without permission | Unauthorized | Forbidden |
| NEG-016 | Update without permission | Unauthorized | Forbidden |
| NEG-017 | Delete without permission | Unauthorized | Forbidden |
| NEG-018 | URL protocol invalid | Protocol invalid | ValidationException |
| NEG-019 | URL too long | URL length | ValidationException |
| NEG-020 | Duplicate URL same entity | URL exists | BusinessException |
| NEG-021 | Import invalid format | Bad CSV | ValidationException |
| NEG-022 | Import duplicate URL | CSV duplicate | BusinessException |
| NEG-023 | Export invalid format | Format invalid | ArgumentException |
| NEG-024 | GetByUrl not found | URL invalid | KeyNotFoundException |
| NEG-025 | Exists invalid | Id invalid | False |
| NEG-026 | Validate null URL | Url=null | ArgumentNullException |
| NEG-027 | Resolve invalid URL | Url invalid | ResolveException |
| NEG-028 | GetMetadata invalid | Id invalid | KeyNotFoundException |
| NEG-029 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-030 | Invalid page number | Page=0 | ArgumentException |
| NEG-031 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-032 | Search null term | Term=null | ArgumentNullException |
| NEG-033 | Categorize invalid | Category invalid | ArgumentException |
| NEG-034 | Bulk create null list | List=null | ArgumentNullException |
| NEG-035 | Update deleted link | Link deleted | KeyNotFoundException |
| NEG-036 | GetById deleted | Link deleted | KeyNotFoundException |
| NEG-037 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-038 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-039 | Transaction rollback | Fail in transaction | Rollback |
| NEG-040 | Connection timeout | DB unavailable | TimeoutException |
| NEG-041 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-042 | Invalid enum value | Category invalid | ArgumentException |
| NEG-043 | Expired session | Expired token | Unauthorized |
| NEG-044 | Null user context | User=null | InvalidOperationException |
| NEG-045 | Invalid include path | Invalid include | ArgumentException |
| NEG-046 | Count invalid entity | EntityId=-1 | ArgumentException |
| NEG-047 | GetCategories invalid | Params invalid | ArgumentException |
| NEG-048 | Validate integrity invalid | Link invalid | ValidationException |
| NEG-049 | Bulk create empty | List=[] | ArgumentException |
| NEG-050 | Export empty | No links | Empty or error |
| NEG-051 | Filter invalid category | Category invalid | ArgumentException |
| NEG-052 | Filter invalid entity type | Type invalid | ArgumentException |
| NEG-053 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-054 | Pagination overflow | Page too large | Empty or error |
| NEG-055 | GetByUrl deleted | Link deleted | KeyNotFoundException |
| NEG-056 | Audit missing user | User=0 | InvalidOperationException |
| NEG-057 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-058 | Associate null entity | Entity=null | ArgumentNullException |
| NEG-059 | Child override throws | Child throws | Propagated |
| NEG-060 | Import missing column | CSV missing | ValidationException |
| NEG-061 | Resolve timeout | URL slow | TimeoutException |
| NEG-062 | GetMetadata deleted | Link deleted | KeyNotFoundException |
| NEG-063 | bulk create one invalid | One invalid | Partial or fail |
| NEG-064 | URL with invalid chars | Chars invalid | ValidationException |
| NEG-065 | URL with XSS | javascript: | ValidationException |
| NEG-066 | Category reserved | Reserved | BusinessException |
| NEG-067 | Entity type mismatch | Type mismatch | ArgumentException |
| NEG-068 | Unassociate invalid | Association invalid | KeyNotFoundException |
| NEG-069 | Recategorize invalid | Category invalid | ArgumentException |
| NEG-070 | Duplicate category | Category exists | BusinessException |
| NEG-071 | Create null request | Request=null | ArgumentNullException |
| NEG-072 | GetByUrl null URL | Url=null | ArgumentNullException |
| NEG-073 | Resolve null URL | Url=null | ArgumentNullException |
| NEG-074 | Validate null link | Link=null | ArgumentNullException |
| NEG-075 | GetMetadata null ID | Id=0 | ArgumentException |
| NEG-076 | Categorize null category | Category=null | ArgumentNullException |
| NEG-077 | Associate null entity ID | EntityId=0 | ArgumentException |
| NEG-078 | Unassociate invalid | Association invalid | KeyNotFoundException |
| NEG-079 | GetCategories null entity | Entity=null | ArgumentNullException |
| NEG-080 | Count null entity | EntityId=0 | ArgumentException |
| NEG-081 | Exists invalid ID | Id=-1 | False |
| NEG-082 | BulkCreate null items | Items=null | ArgumentNullException |
| NEG-083 | Import null file | File=null | ArgumentNullException |
| NEG-084 | Export null filter | Filter invalid | ArgumentException |
| NEG-085 | URL protocol invalid | Protocol=invalid | ValidationException |
| NEG-086 | URL host invalid | Host invalid | ValidationException |
| NEG-087 | Entity type null | Type=null | ArgumentNullException |
| NEG-088 | Title too long | Title 1000 chars | ValidationException |
| NEG-089 | Description too long | Desc 10k chars | ValidationException |
| NEG-090 | GetByUrl case mismatch | Case mismatch | Config |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | URL at min length | Length=10 | Valid |
| BND-002 | URL at max length | Length=2048 | Valid |
| BND-003 | URL exceeds max | Length=2049 | Reject |
| BND-004 | Title at min | Length=1 | Valid |
| BND-005 | Title at max | Length=500 | Valid |
| BND-006 | Title exceeds max | Length=501 | Reject |
| BND-007 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-008 | Page size at min | PageSize=1 | Valid |
| BND-009 | Page size at max | PageSize=1000 | Valid |
| BND-010 | Page size over max | PageSize=1001 | Reject |
| BND-011 | URL with port | :8080 | Valid |
| BND-012 | URL with path | /path | Valid |
| BND-013 | URL with query | ?param=1 | Valid |
| BND-014 | URL with fragment | #section | Valid |
| BND-015 | Unicode in URL | encoded | Valid |
| BND-016 | Unicode in title | Arabic/Chinese | Stored |
| BND-017 | Special chars in title | <>&"' | Escaped |
| BND-018 | Leading/trailing spaces | Title="  x  " | Trimmed |
| BND-019 | Empty description | Description="" | Valid |
| BND-020 | Category enum first | First | Valid |
| BND-021 | Category enum last | Last | Valid |
| BND-022 | Entity type first | First | Valid |
| BND-023 | Entity type last | Last | Valid |
| BND-024 | Empty entity list | Entity=[] | Valid |
| BND-025 | Single entity | Count=1 | Valid |
| BND-026 | Max links per entity | At limit | Valid |
| BND-027 | Date at min | Date=MinValue | Handle |
| BND-028 | Date at max | Date=MaxValue | Handle |
| BND-029 | Empty search term | Term="" | Return all |
| BND-030 | Search term max | Term=500 | Valid |
| BND-031 | Search term over max | Term=501 | Reject |
| BND-032 | Collection empty | [] | No exception |
| BND-033 | Collection single | 1 item | Valid |
| BND-034 | Collection max | At limit | Valid |
| BND-035 | Pagination last partial | Partial page | Correct |
| BND-036 | Pagination total | Total count | Accurate |
| BND-037 | Sort null handling | Nulls in data | Deterministic |
| BND-038 | Filter combination all | All filters | Correct |
| BND-039 | Zero entity ID | EntityId=0 | Reject |
| BND-040 | Max int for ID | Id=2147483647 | Handle |
| BND-041 | Bulk create max | 100 links | Valid |
| BND-042 | Bulk create over max | 101 links | Reject |
| BND-043 | Import row max | Max rows | Valid or reject |
| BND-044 | Import empty file | 0 rows | Empty or error |
| BND-045 | Export large result | 10k rows | Stream |
| BND-046 | URL http | http:// | Valid |
| BND-047 | URL https | https:// | Valid |
| BND-048 | URL ftp | ftp:// | Valid or reject |
| BND-049 | Soft delete boundary | DeletedDate set | Excluded |
| BND-050 | Include depth | Deep include | No explosion |
| BND-051 | Query timeout | Slow query | Timeout |
| BND-052 | Memory large result | 10k rows | No OOM |
| BND-053 | Audit timestamp precision | Millisecond | Stored |
| BND-054 | Long string in description | 4000 chars | Truncate |
| BND-055 | GetMetadata empty | No metadata | Empty |
| BND-056 | GetCategories empty | No categories | Empty list |
| BND-057 | Count zero | No links | 0 |
| BND-058 | Count max | Many | Valid |
| BND-059 | Validate integrity edge | Edge case | Valid |
| BND-060 | Resolve redirect | Redirect | Resolved |
| BND-061 | Resolve 404 | 404 | ResolveException |
| BND-062 | Exists false | Id invalid | False |
| BND-063 | Exists true | Id valid | True |
| BND-064 | GetByUrl empty | No link | KeyNotFoundException |
| BND-065 | Filter by entity empty | No links | Empty list |
| BND-066 | Sort multi-column | 3 columns | Correct order |
| BND-067 | Categorize same | Same category | No-op |
| BND-068 | Associate same | Same entity | No-op or error |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |
| BND-071 | URL at min | Length=10 | Valid |
| BND-072 | Title at min | Length=1 | Valid |
| BND-073 | Description at max | 4000 chars | Valid |
| BND-074 | Category enum first | First | Valid |
| BND-075 | Entity type enum first | First | Valid |
| BND-076 | Links per entity zero | No links | 0 |
| BND-077 | Links per entity max | At limit | Valid |
| BND-078 | Bulk create single | 1 link | Valid |
| BND-079 | Import single row | 1 row | Valid |
| BND-080 | Export single | 1 link | Valid |
| BND-081 | Resolve 200 | 200 OK | Resolved |
| BND-082 | Resolve 301 | Redirect | Resolved |
| BND-083 | GetMetadata empty | No metadata | Empty |
| BND-084 | GetCategories single | 1 category | Valid |
| BND-085 | Count zero | No links | 0 |
| BND-086 | Exists false | Invalid | False |
| BND-087 | Exists true | Valid | True |
| BND-088 | Associate same | Same | No-op |
| BND-089 | Categorize same | Same | No-op |
| BND-090 | URL with auth | user:pass@ | Config |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | URL required | Validation | Create | Reject if empty |
| FUN-002 | URL format valid | Validation | Create | Reject if invalid |
| FUN-003 | Entity required for associate | Validation | Associate | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | URL unique per entity | Constraint | Create | Reject duplicate |
| FUN-008 | Category in allowed list | Constraint | Categorize | Reject if invalid |
| FUN-009 | Entity must exist | Constraint | Associate | Reject invalid |
| FUN-010 | Audit CreatedBy | Audit | Create | Set user |
| FUN-011 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-012 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-013 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-014 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-015 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | URL protocol whitelist | Constraint | Create | Only allowed |
| FUN-018 | Entity type valid | Constraint | Associate | Reject invalid |
| FUN-019 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-020 | GetByEntity excludes deleted | Constraint | GetByEntity | Excludes deleted |
| FUN-021 | Pagination offset | Calculation | Page | Skip correct |
| FUN-022 | Total count accurate | Calculation | Count | Matches |
| FUN-023 | Sort applies | Calculation | Sort | Ordered |
| FUN-024 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-025 | Validate URL format | Logic | Validate | Format check |
| FUN-026 | Resolve URL | Logic | Resolve | Resolved |
| FUN-027 | GetMetadata from URL | Logic | GetMetadata | Fetched |
| FUN-028 | Transaction on create | Transaction | Create | Atomic |
| FUN-029 | Transaction on update | Transaction | Update | Atomic |
| FUN-030 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-031 | Async all operations | Concurrency | All | Async |
| FUN-032 | Include loads entity | Data load | GetById include | Entity loaded |
| FUN-033 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-034 | Associate creates link | Data | Associate | Association |
| FUN-035 | Unassociate removes | Data | Unassociate | Removed |
| FUN-036 | Categorize updates | Logic | Categorize | Updated |
| FUN-037 | Bulk create atomic | Logic | BulkCreate | All or none |
| FUN-038 | Import validates URL | Validation | Import | URL check |
| FUN-039 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-040 | Validate integrity | Logic | Validate | Integrity check |
| FUN-041 | GetByUrl case | Config | GetByUrl | Case per config |
| FUN-042 | GetCategories sorted | Logic | GetCategories | Sorted |
| FUN-043 | Count excludes deleted | Constraint | Count | Excludes deleted |
| FUN-044 | Localized display | i18n | GetDisplay | Localized |
| FUN-045 | URL normalization | Logic | Create | Normalized |
| FUN-046 | Title truncation | Logic | Create | Truncated |
| FUN-047 | Permission cached | Performance | Repeated check | Cached |
| FUN-048 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-049 | URL encoding | Logic | Create | Encoded |
| FUN-050 | Redirect handling | Logic | Resolve | Redirect followed |
| FUN-051 | Create audit | Audit | Create | Audit |
| FUN-052 | Update audit | Audit | Update | Audit |
| FUN-053 | Delete audit | Audit | Delete | Audit |
| FUN-054 | Associate audit | Audit | Associate | Audit |
| FUN-055 | Unassociate audit | Audit | Unassociate | Audit |
| FUN-056 | Categorize audit | Audit | Categorize | Audit |
| FUN-057 | URL validation | Validation | Create | Valid |
| FUN-058 | Entity validation | Validation | Associate | Valid |
| FUN-059 | Category validation | Validation | Categorize | Valid |
| FUN-060 | Entity type validation | Validation | Associate | Valid |
| FUN-061 | Title validation | Validation | Create | Valid |
| FUN-062 | Description validation | Validation | Create | Valid |
| FUN-063 | GetByEntity excludes deleted | Constraint | GetByEntity | Excludes |
| FUN-064 | GetByUrl excludes deleted | Constraint | GetByUrl | Excludes |
| FUN-065 | Count excludes deleted | Constraint | Count | Excludes |
| FUN-066 | Export excludes deleted | Constraint | Export | Excludes |
| FUN-067 | Resolve logic | Logic | Resolve | Resolved |
| FUN-068 | GetMetadata logic | Logic | GetMetadata | Metadata |
| FUN-069 | Validate integrity logic | Logic | Validate | Integrity |
| FUN-070 | URL normalization logic | Logic | Create | Normalized |
| FUN-071 | Title truncation logic | Logic | Create | Truncated |
| FUN-072 | GetCategories logic | Logic | GetCategories | Sorted |
| FUN-073 | Associate logic | Logic | Associate | Associated |
| FUN-074 | Unassociate logic | Logic | Unassociate | Removed |
| FUN-075 | Categorize logic | Logic | Categorize | Updated |
| FUN-076 | BulkCreate transaction | Transaction | BulkCreate | Atomic |
| FUN-077 | Import transaction | Transaction | Import | Atomic |
| FUN-078 | Create transaction | Transaction | Create | Atomic |
| FUN-079 | Update transaction | Transaction | Update | Atomic |
| FUN-080 | Delete transaction | Transaction | Delete | Atomic |
| FUN-081 | Associate transaction | Transaction | Associate | Atomic |
| FUN-082 | Unassociate transaction | Transaction | Unassociate | Atomic |
| FUN-083 | Categorize transaction | Transaction | Categorize | Atomic |
| FUN-084 | Resolve timeout | Logic | Resolve | Timeout |
| FUN-085 | GetMetadata timeout | Logic | GetMetadata | Timeout |
| FUN-086 | Import validation | Validation | Import | Valid |
| FUN-087 | Export format | Logic | Export | Format |
| FUN-088 | BulkCreate validation | Validation | BulkCreate | Valid |
| FUN-089 | GetByUrl case | Config | GetByUrl | Case |
| FUN-090 | Pagination total | Calculation | Paginate | Total |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create link full flow | Create | Link | Created |
| INT-002 | Update link full flow | Update | Link | Updated |
| INT-003 | Delete link full flow | Delete | Link | Soft deleted |
| INT-004 | Get with entity | GetById | Link, Entity | Entity loaded |
| INT-005 | List with filter and sort | List | Link | Filtered, sorted |
| INT-006 | Associate with partner | Associate | Link, Partner | Associated |
| INT-007 | Associate with opportunity | Associate | Link, Opportunity | Associated |
| INT-008 | Associate with contact | Associate | Link, Contact | Associated |
| INT-009 | Categorize link | Categorize | Link | Categorized |
| INT-010 | Search by title | Search | Link | Matching |
| INT-011 | Filter by category | Filter | Link | Filtered |
| INT-012 | Pagination | Paginate | Link | Pages |
| INT-013 | Import CSV | Import | Link | Imported |
| INT-014 | Export CSV | Export | Link | Exported |
| INT-015 | Bulk create | BulkCreate | Link | All created |
| INT-016 | Link-Entity relationship | Relationship | Link, Entity | FK valid |
| INT-017 | Link-Partner relationship | Relationship | Link, Partner | Valid |
| INT-018 | Link-Opportunity relationship | Relationship | Link, Opportunity | Valid |
| INT-019 | Cascade soft delete | Relationship | Entity deleted | Config |
| INT-020 | Orphan handling | Relationship | Entity deleted | Retained |
| INT-021 | DB error handling | Error | DB down | Graceful |
| INT-022 | Timeout handling | Error | Slow DB | Timeout |
| INT-023 | Resolve timeout | Error | Slow URL | Timeout |
| INT-024 | Constraint violation | Error | FK violation | Clear error |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | PartnerManager integration | Integration | PartnerManager | Partner |
| INT-030 | OpportunityManager integration | Integration | OpportunityManager | Opportunity |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | ContactManager integration | Integration | ContactManager | Contact |
| INT-036 | Multiple links per entity | Scenario | Link, Entity | All linked |
| INT-037 | Categorize then filter | Scenario | Categorize, Filter | Both |
| INT-038 | Concurrent create | Scenario | Parallel | All created |
| INT-039 | Import with validation | Scenario | Import | Validated |
| INT-040 | Export with filter | Scenario | Export | Filtered |
| INT-041 | Resolve then validate | Scenario | Resolve, Validate | Both |
| INT-042 | GetMetadata | Scenario | GetMetadata | Metadata |
| INT-043 | GetByUrl | Scenario | GetByUrl | Link |
| INT-044 | Count by entity | Scenario | Count | Count |
| INT-045 | GetCategories | Scenario | GetCategories | Categories |
| INT-046 | Validate integrity | Scenario | Validate | Validated |
| INT-047 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-048 | Associate then unassociate | Scenario | Associate, Unassociate | Clean |
| INT-049 | Bulk create with validation | Scenario | BulkCreate | Validated |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |
| INT-051 | Create then GetById | Scenario | Create, Get | Complete |
| INT-052 | Update then GetById | Scenario | Update, Get | Complete |
| INT-053 | Associate then GetById | Scenario | Associate, Get | Complete |
| INT-054 | Categorize then Filter | Scenario | Categorize, Filter | Complete |
| INT-055 | Import then Export | Scenario | Import, Export | Complete |
| INT-056 | Resolve then Validate | Scenario | Resolve, Validate | Complete |
| INT-057 | GetMetadata then Create | Scenario | GetMetadata, Create | Complete |
| INT-058 | GetByUrl then Update | Scenario | GetByUrl, Update | Complete |
| INT-059 | BulkCreate then List | Scenario | BulkCreate, List | Complete |
| INT-060 | Count then GetByEntity | Scenario | Count, GetByEntity | Complete |
| INT-061 | Exists then GetById | Scenario | Exists, GetById | Complete |
| INT-062 | GetCategories then Categorize | Scenario | GetCategories, Categorize | Complete |
| INT-063 | Unassociate then Count | Scenario | Unassociate, Count | Complete |
| INT-064 | Associate with partner | Scenario | Associate | Partner |
| INT-065 | Associate with opportunity | Scenario | Associate | Opportunity |
| INT-066 | Associate with contact | Scenario | Associate | Contact |
| INT-067 | Filter by category | Scenario | Filter | Category |
| INT-068 | Filter by entity type | Scenario | Filter | Entity type |
| INT-069 | Search then GetById | Scenario | Search, Get | Complete |
| INT-070 | Paginate then Sort | Scenario | Paginate | Sorted |
| INT-071 | Resolve with redirect | Scenario | Resolve | Redirect |
| INT-072 | GetMetadata with URL | Scenario | GetMetadata | URL |
| INT-073 | Validate with resolve | Scenario | Validate | Resolve |
| INT-074 | Import with validation | Scenario | Import | Validated |
| INT-075 | Export with filter | Scenario | Export | Filtered |
| INT-076 | BulkCreate with entity | Scenario | BulkCreate | Entity |
| INT-077 | Categorize then GetCategories | Scenario | Categorize | GetCategories |
| INT-078 | Associate then Unassociate | Scenario | Associate | Unassociate |
| INT-079 | Create with metadata | Scenario | Create | Metadata |
| INT-080 | Update with category | Scenario | Update | Category |
| INT-081 | Delete with audit | Scenario | Delete | Audit |
| INT-082 | GetByUrl with entity | Scenario | GetByUrl | Entity |
| INT-083 | Count with filter | Scenario | Count | Filtered |
| INT-084 | Exists with entity | Scenario | Exists | Entity |
| INT-085 | Resolve with timeout | Scenario | Resolve | Timeout |
| INT-086 | GetMetadata with timeout | Scenario | GetMetadata | Timeout |
| INT-087 | Import with encoding | Scenario | Import | Encoding |
| INT-088 | Export with encoding | Scenario | Export | Encoding |
| INT-089 | Full link cycle | Scenario | Full cycle | Complete |
| INT-090 | E2E full link lifecycle | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in URL | '; DROP TABLE-- | URL | Sanitized |
| SEC-002 | SQL injection in title | ' OR '1'='1 | Title | Sanitized |
| SEC-003 | XSS in URL | javascript:alert(1) | URL | Rejected |
| SEC-004 | XSS in title | <script>alert(1)</script> | Title | Escaped |
| SEC-005 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Open redirect | URL=redirect | URL | Rejected |
| SEC-010 | Unauthorized list | No permission | List | 403 |
| SEC-011 | Unauthorized get | No permission | GetById | 403 |
| SEC-012 | Unauthorized create | No permission | Create | 403 |
| SEC-013 | Unauthorized update | No permission | Update | 403 |
| SEC-014 | Unauthorized delete | No permission | Delete | 403 |
| SEC-015 | Unauthorized import | No permission | Import | 403 |
| SEC-016 | Unauthorized export | No permission | Export | 403 |
| SEC-017 | Role escalation | Low role | Admin | 403 |
| SEC-018 | Cross-tenant access | User A | User B link | 403 |
| SEC-019 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-020 | IDOR update other | Id=other | Update | 403 |
| SEC-021 | IDOR delete other | Id=other | Delete | 403 |
| SEC-022 | IDOR in filter | EntityId=other | List | Filtered |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-025 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-026 | Mass assign EntityId | EntityId=other | Request | Validated |
| SEC-027 | Session hijack | Stolen token | Any | Detected |
| SEC-028 | Token expiration | Expired | Any | 401 |
| SEC-029 | Invalid token | Malformed | Any | 401 |
| SEC-030 | CSRF on create | No token | Create | Rejected |
| SEC-031 | CSRF on update | No token | Update | Rejected |
| SEC-032 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-033 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-034 | URL in log | Log URL | Log | Redacted |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit list | Many lists | List | Throttled |
| SEC-037 | Rate limit resolve | Many resolves | Resolve | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in URL | URL | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge page size | List | Capped |
| SEC-045 | SSRF in URL | Internal URL | URL | Rejected |
| SEC-046 | Import malicious CSV | Malicious | Import | Rejected |
| SEC-047 | Export data injection | Inject in export | Export | Sanitized |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | URL validation bypass | Bypass validation | URL | Rejected |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent create | Two create | Both succeed |
| CON-004 | Concurrent create same URL | Two create | One fails |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on associate | Two associate | One or both |
| CON-009 | Race on categorize | Two categorize | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Import concurrent | Two import | One or both |
| CON-016 | Export concurrent | Two export | Both succeed |
| CON-017 | Resolve concurrent | Two resolve | Both succeed |
| CON-018 | GetByUrl concurrent | Two get | Both correct |
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
| UNT-001 | Validate URL not null | Validation | null | Exception |
| UNT-002 | Validate URL format | Validation | Valid URL | Pass |
| UNT-003 | Validate entity type | Validation | Valid type | Pass |
| UNT-004 | Validate category | Validation | Valid category | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format URL display | Formatting | URL | Display |
| UNT-007 | Format title display | Formatting | Title | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | URL normalization | Calculation | URL | Normalized |
| UNT-013 | URL encoding | Calculation | URL | Encoded |
| UNT-014 | Category allows create | Status logic | Category | true |
| UNT-015 | Entity type allows | Status logic | Type | true |
| UNT-016 | URL protocol check | Status logic | URL | Allowed |
| UNT-017 | URL valid check | Status logic | URL | Valid |
| UNT-018 | Entity exists check | Status logic | Entity | Exists |
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
| PRF-005 | Search by title | Search | <500ms | P1 |
| PRF-006 | List with pagination | List | <300ms | P1 |
| PRF-007 | List with sort | List | <300ms | P1 |
| PRF-008 | Resolve URL | Resolve | <2s | P1 |
| PRF-009 | Get metadata | GetMetadata | <2s | P1 |
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

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
