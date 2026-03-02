# UNOPSPartnerTreeManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerTreeManager` (Unit Tests)  
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

Partner tree manager unit tests cover tree building, hierarchy, expand/collapse, and search. Tests include: tree construction, parent-child relationships, root partners, descendants, ancestors, expand/collapse state, search within tree, move partner, circular reference prevention, and tree traversal.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Build tree | Partners exist | BuildTree | Tree built |
| POS-002 | Get root partners | Roots exist | GetRoots | Roots returned |
| POS-003 | Get children | Partner has children | GetChildren | Children |
| POS-004 | Get descendants | Partner has descendants | GetDescendants | Descendants |
| POS-005 | Get ancestors | Partner has parent | GetAncestors | Ancestors |
| POS-006 | Get parent path | Partner has parent | GetParentPath | Path |
| POS-007 | Add child | Valid parent/child | AddChild | Added |
| POS-008 | Remove child | Child exists | RemoveChild | Removed |
| POS-009 | Move partner | Valid new parent | MovePartner | Moved |
| POS-010 | Expand node | Node collapsed | Expand | Expanded |
| POS-011 | Collapse node | Node expanded | Collapse | Collapsed |
| POS-012 | Search in tree | Tree exists | Search | Matching |
| POS-013 | Find in tree | Partner in tree | FindInTree | Found |
| POS-014 | Get siblings | Siblings exist | GetSiblings | Siblings |
| POS-015 | Depth-first traversal | Tree exists | TraverseDepthFirst | Visited |
| POS-016 | Breadth-first traversal | Tree exists | TraverseBreadthFirst | Visited |
| POS-017 | Get tree depth | Tree exists | GetDepth | Depth |
| POS-018 | Audit CreatedBy | AddChild | Check audit | Set |
| POS-019 | Audit CreatedDate | AddChild | Check audit | UTC |
| POS-020 | Audit LastModifiedBy | MovePartner | Check audit | Set |
| POS-021 | Audit LastModifiedDate | MovePartner | Check audit | UTC |
| POS-022 | Pagination | Many partners | List page | Page |
| POS-023 | Sort by name | Partners exist | Sort | Ordered |
| POS-024 | Filter by level | Partners exist | Filter | Filtered |
| POS-025 | Validate no cycle | Valid tree | Validate | Valid |
| POS-026 | Update parent | Valid parent | UpdateParent | Updated |
| POS-027 | Get direct children only | Partner has children | GetChildren | Direct only |
| POS-028 | Expand all | Tree exists | ExpandAll | All expanded |
| POS-029 | Collapse all | Tree exists | CollapseAll | All collapsed |
| POS-030 | Search case insensitive | Tree exists | Search | Matching |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | BuildTree null partners | Partners=null | ArgumentNullException |
| NEG-002 | BuildTree empty | Partners=[] | Empty tree |
| NEG-003 | GetRoots null | Context=null | ArgumentNullException |
| NEG-004 | GetChildren invalid ID | Id=-1 | ArgumentException |
| NEG-005 | GetChildren non-existent | Id=99999 | KeyNotFoundException |
| NEG-006 | AddChild null parent | Parent=null | ArgumentNullException |
| NEG-007 | AddChild null child | Child=null | ArgumentNullException |
| NEG-008 | MovePartner circular | Parent in subtree | BusinessException |
| NEG-009 | MovePartner to self | ParentId=SelfId | BusinessException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | BuildTree without permission | Unauthorized | Forbidden |
| NEG-012 | MovePartner without permission | Unauthorized | Forbidden |
| NEG-013 | AddChild without permission | Unauthorized | Forbidden |
| NEG-014 | RemoveChild without permission | Unauthorized | Forbidden |
| NEG-015 | Search without permission | Unauthorized | Forbidden |
| NEG-016 | SQL injection in search | '; DROP | Rejected |
| NEG-017 | XSS in search term | <script> | Escaped |
| NEG-018 | Path traversal | ../../../etc | Rejected |
| NEG-019 | Invalid parent ID | ParentId=-1 | ArgumentException |
| NEG-020 | Deleted parent | Parent deleted | KeyNotFoundException |
| NEG-021 | Deleted child | Child deleted | KeyNotFoundException |
| NEG-022 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-023 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-024 | Connection timeout | DB unavailable | TimeoutException |
| NEG-025 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-026 | FindInTree non-existent | Partner not in tree | Null or exception |
| NEG-027 | GetSiblings no siblings | No siblings | Empty list |
| NEG-028 | GetAncestors root | Root partner | Empty list |
| NEG-029 | GetDescendants leaf | Leaf partner | Empty list |
| NEG-030 | Expand already expanded | Already expanded | No-op |
| NEG-031 | Collapse already collapsed | Already collapsed | No-op |
| NEG-032 | Search null term | Term=null | ArgumentNullException |
| NEG-033 | Search empty term | Term="" | Return all or empty |
| NEG-034 | Invalid page number | Page=0 | ArgumentException |
| NEG-035 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-036 | MovePartner deleted | Partner deleted | KeyNotFoundException |
| NEG-037 | AddChild deleted parent | Parent deleted | KeyNotFoundException |
| NEG-038 | Child override throws | Child throws | Propagated |
| NEG-039 | Hierarchy too deep | 20 levels | BusinessException |
| NEG-040 | Expired session | Expired token | Unauthorized |
| NEG-041 | Null user context | User=null | InvalidOperationException |
| NEG-042 | Invalid include path | Invalid include | ArgumentException |
| NEG-043 | GetSubtree deleted | Partner deleted | KeyNotFoundException |
| NEG-044 | GetDepth empty | Empty tree | 0 or exception |
| NEG-045 | Traverse empty | Empty tree | No visit |
| NEG-046 | CountNodes null | Tree=null | ArgumentNullException |
| NEG-047 | IsLeaf root | Root partner | False |
| NEG-048 | IsRoot child | Child partner | False |
| NEG-049 | GetByLevel invalid | Level=-1 | ArgumentException |
| NEG-050 | UpdateParent invalid | Parent invalid | ArgumentException |
| NEG-051 | RemoveChild not child | Not a child | BusinessException |
| NEG-052 | AddChild already child | Already child | BusinessException |
| NEG-053 | Pagination overflow | Page too large | Empty or error |
| NEG-054 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-055 | Filter malformed | Malformed filter | ArgumentException |
| NEG-056 | Audit missing user | User=0 | InvalidOperationException |
| NEG-057 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-058 | ExpandAll empty | Empty tree | No-op |
| NEG-059 | CollapseAll empty | Empty tree | No-op |
| NEG-060 | Cross-tenant parent | Other tenant | Forbidden |
| NEG-061 | Cross-tenant child | Other tenant | Forbidden |
| NEG-062 | GetParentPath root | Root | Empty |
| NEG-063 | MovePartner to descendant | Descendant as parent | BusinessException |
| NEG-064 | Validate circular | Circular tree | Invalid |
| NEG-065 | GetChildren deleted | Partner deleted | KeyNotFoundException |
| NEG-066 | GetDescendants deleted | Partner deleted | KeyNotFoundException |
| NEG-067 | GetAncestors deleted | Partner deleted | KeyNotFoundException |
| NEG-068 | BuildTree orphan | Orphan partners | Handle |
| NEG-069 | Traverse while modifying | Concurrent | Consistent |
| NEG-070 | Search deleted | Include deleted | Config |
| NEG-071 | GetRoots invalid filter | Filter invalid | ArgumentException |
| NEG-072 | GetChildren invalid include | Include invalid | ArgumentException |
| NEG-073 | AddChild parent equals child | Parent=Child | BusinessException |
| NEG-074 | MovePartner same parent | Same parent | No-op or reject |
| NEG-075 | RemoveChild null child | Child=null | ArgumentNullException |
| NEG-076 | UpdateParent null parent | Parent=null | ArgumentNullException |
| NEG-077 | GetSubtree null partner | Partner=null | ArgumentNullException |
| NEG-078 | FindInTree null term | Term=null | ArgumentNullException |
| NEG-079 | GetByLevel over max | Level=100 | ArgumentException |
| NEG-080 | Validate null tree | Tree=null | ArgumentNullException |
| NEG-081 | Expand null node | Node=null | ArgumentNullException |
| NEG-082 | Collapse null node | Node=null | ArgumentNullException |
| NEG-083 | TraverseDepthFirst null | Tree=null | ArgumentNullException |
| NEG-084 | TraverseBreadthFirst null | Tree=null | ArgumentNullException |
| NEG-085 | CountNodes invalid tree | Tree invalid | ArgumentException |
| NEG-086 | GetSiblings null partner | Partner=null | ArgumentNullException |
| NEG-087 | GetParentPath null partner | Partner=null | ArgumentNullException |
| NEG-088 | BuildTree duplicate IDs | Duplicate IDs | Handle |
| NEG-089 | MovePartner to deleted | Parent deleted | KeyNotFoundException |
| NEG-090 | AddChild max depth | At max depth | BusinessException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Tree single node | One partner | Valid |
| BND-002 | Tree two levels | Parent+child | Valid |
| BND-003 | Tree max depth | Max depth | Valid |
| BND-004 | Tree depth over max | Too deep | Reject |
| BND-005 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-006 | ID at zero | Id=0 | Reject |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Children count zero | No children | [] |
| BND-011 | Children count one | One child | Valid |
| BND-012 | Children count many | 100 children | Valid |
| BND-013 | Siblings count zero | No siblings | [] |
| BND-014 | Siblings count many | Many siblings | Valid |
| BND-015 | Search term max | Term=500 | Valid |
| BND-016 | Search term over max | Term=501 | Reject |
| BND-017 | Empty search term | Term="" | Return all |
| BND-018 | Unicode in search | Arabic/Chinese | Matching |
| BND-019 | Special chars in name | <>&"' | Escaped |
| BND-020 | Leading/trailing spaces | Term="  x  " | Trimmed |
| BND-021 | Date at min | Date=MinValue | Handle |
| BND-022 | Date at max | Date=MaxValue | Handle |
| BND-023 | DateTime UTC | UTC input | Stored |
| BND-024 | Collection empty | [] | No exception |
| BND-025 | Collection single | 1 item | Valid |
| BND-026 | Pagination last partial | Partial page | Correct |
| BND-027 | Pagination total | Total count | Accurate |
| BND-028 | Sort null handling | Nulls in data | Deterministic |
| BND-029 | Filter combination all | All filters | Correct |
| BND-030 | Level at 0 | Level=0 | Root |
| BND-031 | Level at max | Level=max | Valid |
| BND-032 | Depth at 1 | Single level | 1 |
| BND-033 | Depth at max | Max depth | Valid |
| BND-034 | Parent null for root | ParentId=null | Valid |
| BND-035 | Parent max int | ParentId=2147483647 | Handle |
| BND-036 | Soft delete boundary | DeletedDate set | Excluded |
| BND-037 | Include depth | Deep include | No explosion |
| BND-038 | Query timeout | Slow query | Timeout |
| BND-039 | Audit timestamp precision | Millisecond | Stored |
| BND-040 | Async cancellation | Cancel token | OperationCanceledException |
| BND-041 | Task timeout | Timeout | TimeoutException |
| BND-042 | Concurrent same second | Same timestamp | Deterministic |
| BND-043 | Expand state boundary | All expanded | Valid |
| BND-044 | Collapse state boundary | All collapsed | Valid |
| BND-045 | Traverse visit count | All nodes | Count match |
| BND-046 | GetSubtree single | Single node | Valid |
| BND-047 | GetSubtree large | Large subtree | Valid |
| BND-048 | GetParentPath single | One ancestor | Valid |
| BND-049 | GetParentPath long | Many ancestors | Valid |
| BND-050 | FindInTree root | Root | Found |
| BND-051 | FindInTree leaf | Leaf | Found |
| BND-052 | FindInTree middle | Middle | Found |
| BND-053 | GetByLevel root | Level=0 | Roots |
| BND-054 | GetByLevel deep | Level=max | Valid |
| BND-055 | CountNodes zero | Empty | 0 |
| BND-056 | CountNodes one | One node | 1 |
| BND-057 | CountNodes many | Many nodes | Count |
| BND-058 | IsLeaf leaf | Leaf | True |
| BND-059 | IsLeaf parent | Parent | False |
| BND-060 | IsRoot root | Root | True |
| BND-061 | IsRoot child | Child | False |
| BND-062 | AddChild first | First child | Valid |
| BND-063 | AddChild last | Last child | Valid |
| BND-064 | MovePartner to root | ParentId=null | Valid |
| BND-065 | MovePartner to leaf | Leaf as parent | Valid |
| BND-066 | RemoveChild last | Last child | Valid |
| BND-067 | BuildTree orphan roots | Multiple roots | Valid |
| BND-068 | Filter empty result | No match | Empty list |
| BND-069 | Sort empty | Empty list | No exception |
| BND-070 | Concurrent tree build | Two build | Both correct |
| BND-071 | Tree depth zero | Empty | 0 |
| BND-072 | GetRoots single | One root | Valid |
| BND-073 | GetRoots multiple | Multiple roots | Valid |
| BND-074 | GetChildren max | Max children | Valid |
| BND-075 | GetDescendants max | Max descendants | Valid |
| BND-076 | GetAncestors max | Max ancestors | Valid |
| BND-077 | Search result max | Max results | Paginate |
| BND-078 | Expand state persist | Expand | Persisted |
| BND-079 | Collapse state persist | Collapse | Persisted |
| BND-080 | Traverse order | Traverse | Order |
| BND-081 | Validate valid tree | Valid | True |
| BND-082 | Validate invalid tree | Invalid | False |
| BND-083 | UpdateParent same | Same parent | No-op |
| BND-084 | RemoveChild first | First child | Valid |
| BND-085 | AddChild middle | Middle position | Valid |
| BND-086 | MovePartner level change | Level change | Valid |
| BND-087 | GetParentPath empty | Root | Empty |
| BND-088 | FindInTree case | Case | Config |
| BND-089 | GetByLevel empty | No at level | [] |
| BND-090 | CountNodes large tree | 10000 nodes | Count |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Parent required for child | Validation | AddChild | Reject if null |
| FUN-002 | Child required | Validation | AddChild | Reject if null |
| FUN-003 | No circular reference | Constraint | MovePartner | Reject cycle |
| FUN-004 | Soft delete excludes | Constraint | BuildTree | Excludes IsDeleted |
| FUN-005 | GetChildren excludes deleted | Constraint | GetChildren | Excludes deleted |
| FUN-006 | GetDescendants excludes deleted | Constraint | GetDescendants | Excludes deleted |
| FUN-007 | GetAncestors excludes deleted | Constraint | GetAncestors | Excludes deleted |
| FUN-008 | Move updates hierarchy | Logic | MovePartner | Hierarchy updated |
| FUN-009 | Audit CreatedBy | Audit | AddChild | Set user |
| FUN-010 | Audit CreatedDate | Audit | AddChild | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | MovePartner | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | MovePartner | Set UTC |
| FUN-013 | Permission before action | Authorization | Any | Check first |
| FUN-014 | Parent must exist | Constraint | AddChild | Reject invalid |
| FUN-015 | Child must exist | Constraint | AddChild | Reject invalid |
| FUN-016 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-017 | Search excludes deleted | Constraint | Search | Excludes deleted |
| FUN-018 | Expand state persisted | Logic | Expand | State saved |
| FUN-019 | Collapse state persisted | Logic | Collapse | State saved |
| FUN-020 | Depth-first order | Logic | TraverseDepthFirst | Depth order |
| FUN-021 | Breadth-first order | Logic | TraverseBreadthFirst | Level order |
| FUN-022 | GetRoots parent null | Logic | GetRoots | ParentId null |
| FUN-023 | GetChildren direct only | Logic | GetChildren | Direct children |
| FUN-024 | GetDescendants recursive | Logic | GetDescendants | All levels |
| FUN-025 | GetAncestors root to parent | Logic | GetAncestors | Ordered |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on move | Transaction | MovePartner | Atomic |
| FUN-031 | Transaction on add | Transaction | AddChild | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads parent | Data load | GetById include | Parent loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | FindInTree recursive | Logic | FindInTree | Recursive |
| FUN-036 | GetSubtree includes self | Logic | GetSubtree | Includes self |
| FUN-037 | GetDepth recursive | Logic | GetDepth | Max depth |
| FUN-038 | CountNodes recursive | Logic | CountNodes | Total |
| FUN-039 | Search case insensitive | Logic | Search | Case insensitive |
| FUN-040 | Validate checks cycle | Logic | Validate | No cycle |
| FUN-041 | UpdateParent validates | Logic | UpdateParent | Valid parent |
| FUN-042 | RemoveChild breaks link | Logic | RemoveChild | Link removed |
| FUN-043 | AddChild creates link | Logic | AddChild | Link created |
| FUN-044 | Config max depth | Config | Validate | Config depth |
| FUN-045 | Config expand default | Config | BuildTree | Config |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Tree caching | Performance | Repeated build | Cached |
| FUN-051 | GetRoots excludes deleted | Constraint | GetRoots | Excludes |
| FUN-052 | GetParentPath ordered | Logic | GetParentPath | Ordered |
| FUN-053 | GetSiblings same level | Logic | GetSiblings | Same level |
| FUN-054 | ExpandAll recursive | Logic | ExpandAll | Recursive |
| FUN-055 | CollapseAll recursive | Logic | CollapseAll | Recursive |
| FUN-056 | MovePartner updates path | Logic | MovePartner | Path |
| FUN-057 | AddChild updates count | Logic | AddChild | Count |
| FUN-058 | RemoveChild updates count | Logic | RemoveChild | Count |
| FUN-059 | GetByLevel excludes deleted | Constraint | GetByLevel | Excludes |
| FUN-060 | FindInTree excludes deleted | Constraint | FindInTree | Excludes |
| FUN-061 | Traverse excludes deleted | Constraint | Traverse | Excludes |
| FUN-062 | CountNodes excludes deleted | Constraint | CountNodes | Excludes |
| FUN-063 | GetDepth excludes deleted | Constraint | GetDepth | Excludes |
| FUN-064 | GetSubtree excludes deleted | Constraint | GetSubtree | Excludes |
| FUN-065 | BuildTree order | Logic | BuildTree | Order |
| FUN-066 | Search relevance | Logic | Search | Relevance |
| FUN-067 | Filter by parent | Logic | Filter | Parent |
| FUN-068 | Sort by level | Logic | Sort | Level |
| FUN-069 | Pagination total | Calculation | Paginate | Total |
| FUN-070 | AddChild audit | Audit | AddChild | Audit |
| FUN-071 | RemoveChild audit | Audit | RemoveChild | Audit |
| FUN-072 | MovePartner audit | Audit | MovePartner | Audit |
| FUN-073 | UpdateParent audit | Audit | UpdateParent | Audit |
| FUN-074 | GetRoots filter | Logic | GetRoots | Filter |
| FUN-075 | GetChildren filter | Logic | GetChildren | Filter |
| FUN-076 | GetDescendants filter | Logic | GetDescendants | Filter |
| FUN-077 | GetAncestors filter | Logic | GetAncestors | Filter |
| FUN-078 | Validate depth | Logic | Validate | Depth |
| FUN-079 | Validate structure | Logic | Validate | Structure |
| FUN-080 | Expand state validation | Validation | Expand | Valid |
| FUN-081 | Collapse state validation | Validation | Collapse | Valid |
| FUN-082 | MovePartner validation | Validation | MovePartner | Valid |
| FUN-083 | AddChild validation | Validation | AddChild | Valid |
| FUN-084 | RemoveChild validation | Validation | RemoveChild | Valid |
| FUN-085 | UpdateParent validation | Validation | UpdateParent | Valid |
| FUN-086 | GetSubtree validation | Validation | GetSubtree | Valid |
| FUN-087 | FindInTree validation | Validation | FindInTree | Valid |
| FUN-088 | GetByLevel validation | Validation | GetByLevel | Valid |
| FUN-089 | CountNodes validation | Validation | CountNodes | Valid |
| FUN-090 | GetDepth validation | Validation | GetDepth | Valid |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Build tree full flow | BuildTree | Partner | Tree built |
| INT-002 | Get roots full flow | GetRoots | Partner | Roots |
| INT-003 | Get children full flow | GetChildren | Partner | Children |
| INT-004 | Move partner full flow | MovePartner | Partner | Moved |
| INT-005 | Add child full flow | AddChild | Partner | Added |
| INT-006 | Get with parent | GetById | Partner, Parent | Parent loaded |
| INT-007 | List with filter and sort | List | Partner | Filtered, sorted |
| INT-008 | Search in tree | Search | Partner | Matching |
| INT-009 | Get descendants | GetDescendants | Partner | Descendants |
| INT-010 | Get ancestors | GetAncestors | Partner | Ancestors |
| INT-011 | Partner-Parent relationship | Relationship | Partner | FK valid |
| INT-012 | Partner-Children relationship | Relationship | Partner | Valid |
| INT-013 | Hierarchy cascade | Relationship | Parent deleted | Config |
| INT-014 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-015 | DB error handling | Error | DB down | Graceful |
| INT-016 | Timeout handling | Error | Slow | Timeout |
| INT-017 | Constraint violation | Error | FK violation | Clear error |
| INT-018 | Permission service integration | Integration | Permission | Check |
| INT-019 | User resolver integration | Integration | User | Resolved |
| INT-020 | Audit context integration | Integration | Audit | Context |
| INT-021 | Logger integration | Integration | Log | Logged |
| INT-022 | PartnerManager integration | Integration | Partner | Partner |
| INT-023 | Mapper integration | Integration | Map | Correct |
| INT-024 | Repository integration | Integration | Repository | CRUD |
| INT-025 | DbContext integration | Integration | DbContext | Scoped |
| INT-026 | Transaction scope | Integration | Transaction | Atomic |
| INT-027 | Multi-level hierarchy | Scenario | Partner | All levels |
| INT-028 | Expand collapse cycle | Scenario | Expand, Collapse | Complete |
| INT-029 | Search then expand | Scenario | Search, Expand | Complete |
| INT-030 | Move with children | Scenario | MovePartner | Children moved |
| INT-031 | Concurrent build | Scenario | Parallel | All succeed |
| INT-032 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-033 | Filter by level | Scenario | GetByLevel | Filtered |
| INT-034 | Traverse then modify | Scenario | Traverse, Move | Consistent |
| INT-035 | Find then get | Scenario | FindInTree, GetById | Complete |
| INT-036 | Add remove cycle | Scenario | AddChild, RemoveChild | Complete |
| INT-037 | Get subtree export | Scenario | GetSubtree | Exported |
| INT-038 | Depth calculation | Scenario | GetDepth | Correct |
| INT-039 | Siblings filter | Scenario | GetSiblings | Filtered |
| INT-040 | Parent path resolution | Scenario | GetParentPath | Path |
| INT-041 | Circular prevention | Scenario | MovePartner | Prevented |
| INT-042 | Orphan roots | Scenario | BuildTree | Handled |
| INT-043 | Large tree | Scenario | 1000 nodes | Built |
| INT-044 | Deep tree | Scenario | 20 levels | Built |
| INT-045 | Breadth traversal | Scenario | TraverseBreadthFirst | Complete |
| INT-046 | Depth traversal | Scenario | TraverseDepthFirst | Complete |
| INT-047 | Count integration | Scenario | CountNodes | Correct |
| INT-048 | Level filter integration | Scenario | GetByLevel | Correct |
| INT-049 | Expand state integration | Scenario | Expand, Build | State |
| INT-050 | E2E build-move-search | Scenario | Full cycle | Complete |
| INT-051 | BuildTree then GetRoots | Scenario | Build, GetRoots | Complete |
| INT-052 | AddChild then GetChildren | Scenario | Add, GetChildren | Complete |
| INT-053 | MovePartner then GetParentPath | Scenario | Move, GetPath | Complete |
| INT-054 | RemoveChild then GetChildren | Scenario | Remove, GetChildren | Complete |
| INT-055 | Search then FindInTree | Scenario | Search, Find | Complete |
| INT-056 | ExpandAll then CollapseAll | Scenario | Expand, Collapse | Complete |
| INT-057 | GetDescendants then GetAncestors | Scenario | Desc, Anc | Complete |
| INT-058 | UpdateParent then GetParentPath | Scenario | Update, GetPath | Complete |
| INT-059 | Validate then BuildTree | Scenario | Validate, Build | Complete |
| INT-060 | GetByLevel then GetChildren | Scenario | ByLevel, Children | Complete |
| INT-061 | TraverseDepthFirst then Count | Scenario | Traverse, Count | Complete |
| INT-062 | TraverseBreadthFirst then Count | Scenario | Traverse, Count | Complete |
| INT-063 | GetSubtree then GetDepth | Scenario | Subtree, Depth | Complete |
| INT-064 | FindInTree then GetById | Scenario | Find, Get | Complete |
| INT-065 | GetSiblings then GetChildren | Scenario | Siblings, Children | Complete |
| INT-066 | BuildTree with filter | Scenario | Build | Filtered |
| INT-067 | MovePartner with validation | Scenario | Move | Validated |
| INT-068 | AddChild with audit | Scenario | Add | Audit |
| INT-069 | RemoveChild with audit | Scenario | Remove | Audit |
| INT-070 | Search with pagination | Scenario | Search | Paginated |
| INT-071 | Filter with sort | Scenario | Filter | Sorted |
| INT-072 | GetRoots with pagination | Scenario | GetRoots | Paginated |
| INT-073 | GetChildren with sort | Scenario | GetChildren | Sorted |
| INT-074 | GetDescendants with filter | Scenario | GetDescendants | Filtered |
| INT-075 | GetAncestors with sort | Scenario | GetAncestors | Sorted |
| INT-076 | Expand with state | Scenario | Expand | State |
| INT-077 | Collapse with state | Scenario | Collapse | State |
| INT-078 | BuildTree with expand | Scenario | Build | Expand |
| INT-079 | MovePartner with children | Scenario | Move | Children |
| INT-080 | AddChild with siblings | Scenario | Add | Siblings |
| INT-081 | RemoveChild with siblings | Scenario | Remove | Siblings |
| INT-082 | UpdateParent with validation | Scenario | Update | Validated |
| INT-083 | GetSubtree with filter | Scenario | GetSubtree | Filtered |
| INT-084 | FindInTree with expand | Scenario | Find | Expand |
| INT-085 | GetByLevel with sort | Scenario | GetByLevel | Sorted |
| INT-086 | CountNodes with filter | Scenario | CountNodes | Filtered |
| INT-087 | GetDepth with validate | Scenario | GetDepth | Validate |
| INT-088 | GetParentPath with ancestors | Scenario | GetPath | Ancestors |
| INT-089 | GetSiblings with filter | Scenario | GetSiblings | Filtered |
| INT-090 | E2E full tree lifecycle | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in search | <script>alert(1)</script> | Search | Escaped |
| SEC-005 | XSS in name | <img onerror=...> | Name | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized build | No permission | BuildTree | 403 |
| SEC-012 | Unauthorized move | No permission | MovePartner | 403 |
| SEC-013 | Unauthorized add child | No permission | AddChild | 403 |
| SEC-014 | Unauthorized remove child | No permission | RemoveChild | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B partner | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR move other | Id=other | MovePartner | 403 |
| SEC-019 | IDOR add child other | Id=other | AddChild | 403 |
| SEC-020 | IDOR in filter | ParentId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign ParentId | ParentId=manipulated | Request | Validated |
| SEC-023 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-024 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-025 | Hierarchy injection | Malicious parent | MovePartner | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on move | No token | MovePartner | Rejected |
| SEC-030 | CSRF on add child | No token | AddChild | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Path tampering | Tamper path | GetParentPath | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit build | Many builds | BuildTree | Throttled |
| SEC-036 | Rate limit search | Many searches | Search | Throttled |
| SEC-037 | Rate limit list | Many lists | List | Throttled |
| SEC-038 | Oversized request | 10MB payload | BuildTree | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in search | Search | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge tree | BuildTree | Rejected |
| SEC-045 | Parent injection | Invalid parent | AddChild | Rejected |
| SEC-046 | Child injection | Invalid child | AddChild | Rejected |
| SEC-047 | Move injection | Invalid move | MovePartner | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Tree ACL | Direct access | Tree | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users move same | A, B move | Optimistic lock |
| CON-002 | Move and add child same | Move, add | Deterministic |
| CON-003 | Double add same child | Two add | One or both |
| CON-004 | Concurrent build | Two build | Both succeed |
| CON-005 | Read during write | Read while move | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on move | Two move | One wins |
| CON-009 | Race on add child | Two add | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel builds | 10 parallel | All succeed |
| CON-012 | Async parallel gets | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Search concurrent | Two search | Both correct |
| CON-016 | Move concurrent | Two move | One wins |
| CON-017 | AddChild concurrent | Two add | One wins |
| CON-018 | Soft delete concurrent | Delete while move | Deterministic |
| CON-019 | Build concurrent | Two build | Both correct |
| CON-020 | Expand concurrent | Two expand | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Tree recursion limit | Deep concurrent | No stack overflow |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate parent not null | Validation | null | Exception |
| UNT-002 | Validate child | Validation | Valid child | Pass |
| UNT-003 | Validate no cycle | Validation | Valid tree | Pass |
| UNT-004 | Validate level | Validation | Valid level | Pass |
| UNT-005 | Validate search term | Validation | Valid term | Pass |
| UNT-006 | Format path | Formatting | Path | Formatted |
| UNT-007 | Format tree node | Formatting | Node | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Depth calculation | Calculation | Tree | Depth |
| UNT-013 | Count calculation | Calculation | Tree | Count |
| UNT-014 | IsLeaf check | Status logic | Leaf | true |
| UNT-015 | IsRoot check | Status logic | Root | true |
| UNT-016 | Parent allows child | Status logic | Parent | true |
| UNT-017 | Cycle check | Status logic | Tree | False |
| UNT-018 | Search match | Status logic | Term | Match |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Build tree 100 | BuildTree | <500ms | P1 |
| PRF-003 | Build tree 1000 | BuildTree | <2s | P1 |
| PRF-004 | Get children | GetChildren | <100ms | P0 |
| PRF-005 | Get descendants | GetDescendants | <300ms | P0 |
| PRF-006 | Search in tree | Search | <500ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Move partner | MovePartner | <200ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 builds | 5 parallel BuildTree | <10s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 build | <10s total | P2 |
| PRF-013 | Memory tree 1000 | BuildTree 1000 | <100MB | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory get descendants | GetDescendants | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS build | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS build | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS search | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress tree build | Many builds | Until limit | Holds |
| LDT-008 | Stress memory | Large trees | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
