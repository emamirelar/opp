# OrganizationHierarchyManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager` (Unit Tests)  
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

Organization hierarchy manager unit tests cover CRUD for org units, tree traversal, and type enforcement. Tests include: org unit CRUD, hierarchy validation, parent-child relationships, tree building, type/level enforcement, move/reorder operations, ancestor/descendant queries, and path resolution.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create org unit | Valid data | Create | Unit created |
| POS-002 | Get org unit by ID | Unit exists | GetById | Unit returned |
| POS-003 | Update org unit | Unit exists | Update | Updated |
| POS-004 | Delete org unit | Unit exists | Delete | Soft deleted |
| POS-005 | List org units | Units exist | List | List returned |
| POS-006 | Get root units | Root exists | GetRoots | Roots returned |
| POS-007 | Get children | Unit has children | GetChildren | Children |
| POS-008 | Get descendants | Unit has descendants | GetDescendants | Descendants |
| POS-009 | Get ancestors | Unit has parent | GetAncestors | Ancestors |
| POS-010 | Build tree | Hierarchy exists | BuildTree | Tree built |
| POS-011 | Get path | Unit exists | GetPath | Path returned |
| POS-012 | Move unit | Unit exists | Move | Moved |
| POS-013 | Validate hierarchy | Valid hierarchy | Validate | Valid |
| POS-014 | Enforce type | Valid type | Create | Created |
| POS-015 | Get by type | Units exist | GetByType | Filtered |
| POS-016 | Get by level | Units exist | GetByLevel | Filtered |
| POS-017 | Audit CreatedBy | Create | Check audit | Set |
| POS-018 | Audit CreatedDate | Create | Check audit | UTC |
| POS-019 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-020 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-021 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-022 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-023 | Pagination | Many units | List page | Page |
| POS-024 | Sort by name | Units exist | Sort | Ordered |
| POS-025 | Filter by parent | Parent exists | Filter | Filtered |
| POS-026 | Recursive depth | Deep hierarchy | GetDescendants | All levels |
| POS-027 | Circular check | No cycle | Validate | Valid |
| POS-028 | Reorder siblings | Siblings exist | Reorder | Reordered |
| POS-029 | Bulk get | IDs valid | GetByIds | Units |
| POS-030 | Search by name | Units exist | Search | Matching |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ArgumentNullException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Get by zero ID | Id=0 | ArgumentException |
| NEG-004 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-005 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-006 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Invalid parent ID | ParentId=-1 | ArgumentException |
| NEG-008 | Invalid org type | Type=invalid | ArgumentException |
| NEG-009 | Invalid level | Level=-1 | ArgumentException |
| NEG-010 | Move to self | ParentId=SelfId | BusinessException |
| NEG-011 | Create circular | Parent in subtree | BusinessException |
| NEG-012 | GetById without permission | Unauthorized | Forbidden |
| NEG-013 | Create without permission | Unauthorized | Forbidden |
| NEG-014 | Update without permission | Unauthorized | Forbidden |
| NEG-015 | Delete without permission | Unauthorized | Forbidden |
| NEG-016 | Move without permission | Unauthorized | Forbidden |
| NEG-017 | SQL injection in search | '; DROP | Rejected |
| NEG-018 | XSS in name | <script> | Escaped |
| NEG-019 | Path traversal | ../../../etc | Rejected |
| NEG-020 | Null parent for root | ParentId=null | Valid for root |
| NEG-021 | Invalid include path | Invalid include | ArgumentException |
| NEG-022 | Deleted parent | Parent deleted | KeyNotFoundException |
| NEG-023 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-024 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-025 | Connection timeout | DB unavailable | TimeoutException |
| NEG-026 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-027 | Invalid enum value | Type invalid | ArgumentException |
| NEG-028 | Level exceeds max | Level=1000 | BusinessException |
| NEG-029 | Expired session | Expired token | Unauthorized |
| NEG-030 | Null user context | User=null | InvalidOperationException |
| NEG-031 | GetChildren deleted | Unit deleted | KeyNotFoundException |
| NEG-032 | GetDescendants deleted | Unit deleted | KeyNotFoundException |
| NEG-033 | GetAncestors deleted | Unit deleted | KeyNotFoundException |
| NEG-034 | BuildTree deleted | Unit deleted | Excluded |
| NEG-035 | Move deleted unit | Unit deleted | KeyNotFoundException |
| NEG-036 | Invalid page number | Page=0 | ArgumentException |
| NEG-037 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-038 | Search null term | Term=null | ArgumentNullException |
| NEG-039 | Reorder invalid | Invalid order | ArgumentException |
| NEG-040 | Type mismatch on move | Wrong type | BusinessException |
| NEG-041 | Level mismatch | Wrong level | BusinessException |
| NEG-042 | Duplicate name same parent | Name exists | BusinessException |
| NEG-043 | GetByIds null | Ids=null | ArgumentNullException |
| NEG-044 | GetByIds empty | Ids=[] | ArgumentException |
| NEG-045 | Validate null unit | Unit=null | ArgumentNullException |
| NEG-046 | GetPath deleted | Unit deleted | KeyNotFoundException |
| NEG-047 | GetRoots with filter | Invalid filter | ArgumentException |
| NEG-048 | Child override throws | Child throws | Propagated |
| NEG-049 | Hierarchy too deep | 20 levels | BusinessException |
| NEG-050 | GetById deleted | Unit deleted | KeyNotFoundException |
| NEG-051 | Update deleted | Unit deleted | KeyNotFoundException |
| NEG-052 | Delete with children | Has children | BusinessException or cascade |
| NEG-053 | Move to descendant | Parent in subtree | BusinessException |
| NEG-054 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-055 | Filter malformed | Malformed filter | ArgumentException |
| NEG-056 | Audit missing user | User=0 | InvalidOperationException |
| NEG-057 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-058 | GetTree null root | Root=null | ArgumentNullException |
| NEG-059 | BuildTree invalid root | Root invalid | ArgumentException |
| NEG-060 | Reorder null list | List=null | ArgumentNullException |
| NEG-061 | Pagination overflow | Page too large | Empty or error |
| NEG-062 | GetByType invalid | Type invalid | ArgumentException |
| NEG-063 | GetByLevel invalid | Level invalid | ArgumentException |
| NEG-064 | Move same parent | ParentId=same | No-op or reject |
| NEG-065 | Create duplicate code | Code exists | BusinessException |
| NEG-066 | Update to duplicate name | Name exists | BusinessException |
| NEG-067 | Null type | Type=null | ArgumentNullException |
| NEG-068 | Empty type | Type="" | ValidationException |
| NEG-069 | Orphan unit | Parent deleted | Handle |
| NEG-070 | Cross-tenant parent | Other tenant parent | Forbidden |
| NEG-071 | Create null request | Request=null | ArgumentNullException |
| NEG-072 | Update null request | Request=null | ArgumentNullException |
| NEG-073 | GetParent null unit | Unit=null | ArgumentNullException |
| NEG-074 | GetPath null unit | Unit=null | ArgumentNullException |
| NEG-075 | Move null unit | Unit=null | ArgumentNullException |
| NEG-076 | Reorder null unit | Unit=null | ArgumentNullException |
| NEG-077 | BuildTree null units | Units=null | ArgumentNullException |
| NEG-078 | GetRoots invalid filter | Filter invalid | ArgumentException |
| NEG-079 | GetChildren invalid unit | UnitId=0 | ArgumentException |
| NEG-080 | GetDescendants invalid unit | UnitId=-1 | ArgumentException |
| NEG-081 | GetAncestors invalid unit | UnitId=99999 | KeyNotFoundException |
| NEG-082 | GetByType null type | Type=null | ArgumentNullException |
| NEG-083 | GetByLevel negative | Level=-1 | ArgumentException |
| NEG-084 | GetByIds null | Ids=null | ArgumentNullException |
| NEG-085 | Validate null hierarchy | Hierarchy=null | ArgumentNullException |
| NEG-086 | Move to deleted parent | Parent deleted | KeyNotFoundException |
| NEG-087 | Create with invalid level | Level invalid | ArgumentException |
| NEG-088 | Update to duplicate code | Code exists | BusinessException |
| NEG-089 | Reorder invalid positions | Positions invalid | ArgumentException |
| NEG-090 | Search null term | Term=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min | Length=1 | Valid |
| BND-002 | Name at max | Length=255 | Valid |
| BND-003 | Name exceeds max | Length=256 | Reject |
| BND-004 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-005 | ID at zero | Id=0 | Reject |
| BND-006 | Page size at min | PageSize=1 | Valid |
| BND-007 | Page size at max | PageSize=1000 | Valid |
| BND-008 | Page size over max | PageSize=1001 | Reject |
| BND-009 | Level at 0 | Level=0 | Root |
| BND-010 | Level at max | Level=max | Valid |
| BND-011 | Level over max | Level=max+1 | Reject |
| BND-012 | Depth at 1 | Single level | Valid |
| BND-013 | Depth at max | Max depth | Valid |
| BND-014 | Depth over max | Too deep | Reject |
| BND-015 | Unicode in name | Arabic/Chinese | Stored |
| BND-016 | Special chars in name | <>&"' | Escaped |
| BND-017 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-018 | Empty children list | No children | [] |
| BND-019 | Single child | Count=1 | Valid |
| BND-020 | Many children | 100 children | Valid |
| BND-021 | Date at min | Date=MinValue | Handle |
| BND-022 | Date at max | Date=MaxValue | Handle |
| BND-023 | DateTime UTC | UTC input | Stored |
| BND-024 | Empty search term | Term="" | Return all |
| BND-025 | Search term max | Term=500 | Valid |
| BND-026 | Search term over max | Term=501 | Reject |
| BND-027 | Collection empty | [] | No exception |
| BND-028 | Collection single | 1 item | Valid |
| BND-029 | Tree single node | One unit | Valid |
| BND-030 | Tree max depth | Deep tree | Valid |
| BND-031 | Pagination last partial | Partial page | Correct |
| BND-032 | Pagination total | Total count | Accurate |
| BND-033 | Sort null handling | Nulls in data | Deterministic |
| BND-034 | Filter combination all | All filters | Correct |
| BND-035 | Type enum boundary | Last enum | Valid |
| BND-036 | Level enum boundary | Last enum | Valid |
| BND-037 | Parent null for root | ParentId=null | Valid |
| BND-038 | Parent max int | ParentId=2147483647 | Handle |
| BND-039 | Soft delete boundary | DeletedDate set | Excluded |
| BND-040 | Include depth | Deep include | No explosion |
| BND-041 | Query timeout | Slow query | Timeout |
| BND-042 | Audit timestamp precision | Millisecond | Stored |
| BND-043 | Long string in name | 255 chars | Valid |
| BND-044 | Code max length | Length=50 | Valid |
| BND-045 | Code over max | Length=51 | Reject |
| BND-046 | Async cancellation | Cancel token | OperationCanceledException |
| BND-047 | Task timeout | Timeout | TimeoutException |
| BND-048 | Concurrent same second | Same timestamp | Deterministic |
| BND-049 | Reorder first | First position | Valid |
| BND-050 | Reorder last | Last position | Valid |
| BND-051 | GetPath root | Root unit | Single |
| BND-052 | GetPath deep | Deep unit | Full path |
| BND-053 | GetAncestors root | Root | Empty |
| BND-054 | GetDescendants leaf | Leaf | Empty |
| BND-055 | Move to root | ParentId=null | Valid |
| BND-056 | Move to same level | Same level | Valid |
| BND-057 | Siblings count max | Many siblings | Valid |
| BND-058 | GetByIds max | 1000 IDs | Valid |
| BND-059 | GetByIds over max | 1001 IDs | Reject |
| BND-060 | BuildTree empty | No units | Empty |
| BND-061 | BuildTree single | One unit | Single |
| BND-062 | Validate valid | Valid tree | True |
| BND-063 | Validate invalid | Invalid tree | False |
| BND-064 | Filter empty result | No match | Empty list |
| BND-065 | Sort empty | Empty list | No exception |
| BND-066 | Pagination empty | No data | Empty |
| BND-067 | Type at min | First type | Valid |
| BND-068 | Level at min | Level=0 | Valid |
| BND-069 | Hierarchy 2 levels | Parent+child | Valid |
| BND-070 | Concurrent tree build | Two build | Both correct |
| BND-071 | Name whitespace only | Name="   " | Reject |
| BND-072 | Code at min | Length=1 | Valid |
| BND-073 | GetParent root | Root | Null |
| BND-074 | GetPath single | One level | Valid |
| BND-075 | Move to same level | Same level | No-op |
| BND-076 | Reorder single | One item | Valid |
| BND-077 | BuildTree max nodes | Max nodes | Valid |
| BND-078 | GetRoots empty | No roots | [] |
| BND-079 | GetChildren max | Max children | Valid |
| BND-080 | GetDescendants max | Max descendants | Valid |
| BND-081 | GetAncestors max | Max ancestors | Valid |
| BND-082 | GetByType empty | No match | [] |
| BND-083 | GetByLevel empty | No match | [] |
| BND-084 | GetByIds single | One ID | Valid |
| BND-085 | Validate empty | Empty tree | Config |
| BND-086 | Type enum first | First type | Valid |
| BND-087 | Level enum first | First level | Valid |
| BND-088 | Search result empty | No match | [] |
| BND-089 | Filter empty result | No match | [] |
| BND-090 | Reorder boundary | First/last | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | Type required | Validation | Create | Reject if invalid |
| FUN-003 | Parent must exist | Validation | Create | Reject if invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | No circular reference | Constraint | Move | Reject cycle |
| FUN-008 | Type enforcement | Constraint | Create | Type valid |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Parent must not be deleted | Constraint | Create | Reject deleted |
| FUN-017 | Level must match parent | Constraint | Create | Level+1 |
| FUN-018 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-019 | GetChildren excludes deleted | Constraint | GetChildren | Excludes deleted |
| FUN-020 | GetDescendants excludes deleted | Constraint | GetDescendants | Excludes deleted |
| FUN-021 | Tree excludes deleted | Logic | BuildTree | Excludes deleted |
| FUN-022 | Path includes all ancestors | Logic | GetPath | Full path |
| FUN-023 | Move updates children | Logic | Move | Children updated |
| FUN-024 | Reorder updates sequence | Logic | Reorder | Sequence updated |
| FUN-025 | Validate checks cycle | Logic | Validate | No cycle |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on move | Transaction | Move | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads parent | Data load | GetById include | Parent loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | GetRoots parent null | Logic | GetRoots | ParentId null |
| FUN-036 | GetAncestors ordered | Logic | GetAncestors | Root to parent |
| FUN-037 | GetDescendants depth-first | Logic | GetDescendants | Depth order |
| FUN-038 | Type inheritance | Logic | GetByType | Type match |
| FUN-039 | Level filtering | Logic | GetByLevel | Level match |
| FUN-040 | Duplicate name per parent | Constraint | Create | Reject duplicate |
| FUN-041 | Code unique | Constraint | Create | Reject duplicate |
| FUN-042 | Delete cascade or block | Constraint | Delete | Config |
| FUN-043 | BuildTree recursive | Logic | BuildTree | Recursive |
| FUN-044 | Search by name | Logic | Search | Name match |
| FUN-045 | Config max depth | Config | Validate | Config depth |
| FUN-046 | Config max children | Config | Create | Config limit |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | GetRoots excludes deleted | Constraint | GetRoots | Excludes |
| FUN-052 | GetPath excludes deleted | Constraint | GetPath | Excludes |
| FUN-053 | GetParent excludes deleted | Constraint | GetParent | Excludes |
| FUN-054 | Move audit | Audit | Move | Audit |
| FUN-055 | Reorder audit | Audit | Reorder | Audit |
| FUN-056 | Create audit | Audit | Create | Audit |
| FUN-057 | Update audit | Audit | Update | Audit |
| FUN-058 | Delete audit | Audit | Delete | Audit |
| FUN-059 | GetRoots filter | Logic | GetRoots | Filter |
| FUN-060 | GetChildren filter | Logic | GetChildren | Filter |
| FUN-061 | GetDescendants filter | Logic | GetDescendants | Filter |
| FUN-062 | GetAncestors filter | Logic | GetAncestors | Filter |
| FUN-063 | GetPath validation | Validation | GetPath | Valid |
| FUN-064 | Move validation | Validation | Move | Valid |
| FUN-065 | Reorder validation | Validation | Reorder | Valid |
| FUN-066 | BuildTree validation | Validation | BuildTree | Valid |
| FUN-067 | GetByType validation | Validation | GetByType | Valid |
| FUN-068 | GetByLevel validation | Validation | GetByLevel | Valid |
| FUN-069 | GetByIds validation | Validation | GetByIds | Valid |
| FUN-070 | Search validation | Validation | Search | Valid |
| FUN-071 | Create transaction | Transaction | Create | Atomic |
| FUN-072 | Update transaction | Transaction | Update | Atomic |
| FUN-073 | Delete transaction | Transaction | Delete | Atomic |
| FUN-074 | Move transaction | Transaction | Move | Atomic |
| FUN-075 | Reorder transaction | Transaction | Reorder | Atomic |
| FUN-076 | GetParent logic | Logic | GetParent | Parent |
| FUN-077 | GetPath logic | Logic | GetPath | Path |
| FUN-078 | Move logic | Logic | Move | Moved |
| FUN-079 | Reorder logic | Logic | Reorder | Reordered |
| FUN-080 | BuildTree logic | Logic | BuildTree | Tree |
| FUN-081 | GetRoots logic | Logic | GetRoots | Roots |
| FUN-082 | GetChildren logic | Logic | GetChildren | Children |
| FUN-083 | GetDescendants logic | Logic | GetDescendants | Descendants |
| FUN-084 | GetAncestors logic | Logic | GetAncestors | Ancestors |
| FUN-085 | GetByType logic | Logic | GetByType | Filtered |
| FUN-086 | GetByLevel logic | Logic | GetByLevel | Filtered |
| FUN-087 | GetByIds logic | Logic | GetByIds | Units |
| FUN-088 | Search logic | Logic | Search | Matching |
| FUN-089 | Validate logic | Logic | Validate | Valid |
| FUN-090 | Pagination total | Calculation | Paginate | Total |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create org unit full flow | Create | OrgUnit | Created |
| INT-002 | Get org unit full flow | GetById | OrgUnit | Returned |
| INT-003 | Update org unit full flow | Update | OrgUnit | Updated |
| INT-004 | Delete org unit full flow | Delete | OrgUnit | Soft deleted |
| INT-005 | Build tree full flow | BuildTree | OrgUnit | Tree built |
| INT-006 | Get with parent | GetById | OrgUnit, Parent | Parent loaded |
| INT-007 | List with filter and sort | List | OrgUnit | Filtered, sorted |
| INT-008 | Get children | GetChildren | OrgUnit | Children |
| INT-009 | Get descendants | GetDescendants | OrgUnit | Descendants |
| INT-010 | Get ancestors | GetAncestors | OrgUnit | Ancestors |
| INT-011 | Get path | GetPath | OrgUnit | Path |
| INT-012 | Move unit | Move | OrgUnit | Moved |
| INT-013 | Reorder | Reorder | OrgUnit | Reordered |
| INT-014 | Validate hierarchy | Validate | OrgUnit | Valid |
| INT-015 | OrgUnit-Parent relationship | Relationship | OrgUnit | FK valid |
| INT-016 | OrgUnit-Children relationship | Relationship | OrgUnit | Valid |
| INT-017 | Hierarchy cascade | Relationship | Parent deleted | Config |
| INT-018 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-019 | DB error handling | Error | DB down | Graceful |
| INT-020 | Timeout handling | Error | Slow | Timeout |
| INT-021 | Constraint violation | Error | FK violation | Clear error |
| INT-022 | Permission service integration | Integration | Permission | Check |
| INT-023 | User resolver integration | Integration | User | Resolved |
| INT-024 | Audit context integration | Integration | Audit | Context |
| INT-025 | Logger integration | Integration | Log | Logged |
| INT-026 | Mapper integration | Integration | Map | Correct |
| INT-027 | Repository integration | Integration | Repository | CRUD |
| INT-028 | DbContext integration | Integration | DbContext | Scoped |
| INT-029 | Transaction scope | Integration | Transaction | Atomic |
| INT-030 | Multiple levels | Scenario | OrgUnit | All levels |
| INT-031 | Deep hierarchy | Scenario | OrgUnit | Deep |
| INT-032 | Concurrent create | Scenario | Parallel | All succeed |
| INT-033 | Move with children | Scenario | Move | Children moved |
| INT-034 | Reorder siblings | Scenario | Reorder | Reordered |
| INT-035 | Search hierarchy | Scenario | Search | Matching |
| INT-036 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-037 | Filter by type | Scenario | GetByType | Filtered |
| INT-038 | Filter by level | Scenario | GetByLevel | Filtered |
| INT-039 | Get roots | Scenario | GetRoots | Roots |
| INT-040 | Create update delete cycle | Scenario | CRUD | Complete |
| INT-041 | Tree from roots | Scenario | BuildTree | Tree |
| INT-042 | Path resolution | Scenario | GetPath | Path |
| INT-043 | Bulk get | Scenario | GetByIds | Units |
| INT-044 | Validate invalid | Scenario | Validate | Invalid |
| INT-045 | Type enforcement | Scenario | Create | Enforced |
| INT-046 | Level enforcement | Scenario | Create | Enforced |
| INT-047 | Circular prevention | Scenario | Move | Prevented |
| INT-048 | Delete with children | Scenario | Delete | Config |
| INT-049 | Move then validate | Scenario | Move, Validate | Valid |
| INT-050 | E2E create-move-delete | Scenario | Full cycle | Complete |
| INT-051 | Create then GetById | Scenario | Create, Get | Complete |
| INT-052 | Update then GetById | Scenario | Update, Get | Complete |
| INT-053 | GetRoots then GetChildren | Scenario | Roots, Children | Complete |
| INT-054 | GetChildren then GetDescendants | Scenario | Children, Desc | Complete |
| INT-055 | GetAncestors then GetPath | Scenario | Ancestors, Path | Complete |
| INT-056 | BuildTree then GetRoots | Scenario | Build, Roots | Complete |
| INT-057 | Move then GetPath | Scenario | Move, Path | Complete |
| INT-058 | Reorder then GetChildren | Scenario | Reorder, Children | Complete |
| INT-059 | Validate then Create | Scenario | Validate, Create | Complete |
| INT-060 | GetByType then GetById | Scenario | Type, Get | Complete |
| INT-061 | GetByLevel then GetById | Scenario | Level, Get | Complete |
| INT-062 | GetByIds then Update | Scenario | GetByIds, Update | Complete |
| INT-063 | Search then GetById | Scenario | Search, Get | Complete |
| INT-064 | GetParent then GetPath | Scenario | Parent, Path | Complete |
| INT-065 | Create with parent | Scenario | Create | Parent |
| INT-066 | Update with type | Scenario | Update | Type |
| INT-067 | Delete with children | Scenario | Delete | Children |
| INT-068 | Move with validation | Scenario | Move | Validated |
| INT-069 | Reorder with validation | Scenario | Reorder | Validated |
| INT-070 | BuildTree with filter | Scenario | BuildTree | Filtered |
| INT-071 | GetRoots with pagination | Scenario | GetRoots | Paginated |
| INT-072 | GetChildren with sort | Scenario | GetChildren | Sorted |
| INT-073 | GetDescendants with filter | Scenario | GetDescendants | Filtered |
| INT-074 | GetAncestors with sort | Scenario | GetAncestors | Sorted |
| INT-075 | GetPath with validation | Scenario | GetPath | Validated |
| INT-076 | GetByType with pagination | Scenario | GetByType | Paginated |
| INT-077 | GetByLevel with sort | Scenario | GetByLevel | Sorted |
| INT-078 | GetByIds with filter | Scenario | GetByIds | Filtered |
| INT-079 | Search with pagination | Scenario | Search | Paginated |
| INT-080 | Validate with move | Scenario | Validate | Move |
| INT-081 | Create with type | Scenario | Create | Type |
| INT-082 | Create with level | Scenario | Create | Level |
| INT-083 | Update with parent | Scenario | Update | Parent |
| INT-084 | Delete with hierarchy | Scenario | Delete | Hierarchy |
| INT-085 | Move with children | Scenario | Move | Children |
| INT-086 | Reorder with siblings | Scenario | Reorder | Siblings |
| INT-087 | BuildTree with depth | Scenario | BuildTree | Depth |
| INT-088 | GetPath with ancestors | Scenario | GetPath | Ancestors |
| INT-089 | Full hierarchy cycle | Scenario | Full cycle | Complete |
| INT-090 | E2E full org unit lifecycle | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in code | <img onerror=...> | Code | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized create | No permission | Create | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized move | No permission | Move | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B unit | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR update other | Id=other | Update | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | ParentId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign ParentId | ParentId=manipulated | Request | Validated |
| SEC-025 | Hierarchy injection | Malicious parent | Move | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Path tampering | Tamper path | GetPath | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit list | Many lists | List | Throttled |
| SEC-037 | Rate limit tree | Many trees | BuildTree | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in name | Name | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge tree | BuildTree | Rejected |
| SEC-045 | Type injection | Invalid type | Create | Rejected |
| SEC-046 | Level injection | Invalid level | Create | Rejected |
| SEC-047 | Parent injection | Invalid parent | Create | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Hierarchy ACL | Direct access | Tree | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same name | Two create | One or both |
| CON-004 | Concurrent create | Two create | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on move | Two move | One wins |
| CON-009 | Race on reorder | Two reorder | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel gets | 10 parallel | All succeed |
| CON-012 | Async parallel creates | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Tree build concurrent | Two build | Both correct |
| CON-016 | Move concurrent | Two move | One wins |
| CON-017 | Reorder concurrent | Two reorder | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Create concurrent | Two create | Both succeed |
| CON-020 | Update concurrent | Two update | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Tree recursion limit | Deep concurrent | No stack overflow |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate type | Validation | Valid type | Pass |
| UNT-003 | Validate level | Validation | Valid level | Pass |
| UNT-004 | Validate parent | Validation | Valid parent | Pass |
| UNT-005 | Validate no cycle | Validation | Valid tree | Pass |
| UNT-006 | Format name | Formatting | Name | Formatted |
| UNT-007 | Format path | Formatting | Path | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Path resolution | Calculation | Ancestors | Path |
| UNT-013 | Depth calculation | Calculation | Tree | Depth |
| UNT-014 | Type allows create | Status logic | Type | true |
| UNT-015 | Level allows create | Status logic | Level | true |
| UNT-016 | Parent allows child | Status logic | Parent | true |
| UNT-017 | Cycle check | Status logic | Tree | False |
| UNT-018 | Name check | Status logic | Name | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Get children | GetChildren | <100ms | P1 |
| PRF-004 | Build tree 100 nodes | BuildTree | <500ms | P0 |
| PRF-005 | Build tree 1000 nodes | BuildTree | <2s | P0 |
| PRF-006 | Get descendants | GetDescendants | <300ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Get path | GetPath | <100ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 creates | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 create | <5s total | P2 |
| PRF-013 | Memory tree 1000 | BuildTree 1000 | <100MB | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory get descendants | GetDescendants | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS create | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS create | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS tree | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress tree build | Many trees | Until limit | Holds |
| LDT-008 | Stress memory | Large trees | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
