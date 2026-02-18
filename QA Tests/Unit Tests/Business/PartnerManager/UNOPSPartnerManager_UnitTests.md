# UNOPSPartnerManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerManager` (Unit Tests)  
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

Partner manager unit tests cover CRUD, approval workflow, ERP dim value sequence (PNO-686), and status lifecycle. Tests include: partner CRUD, approval workflow, ErpDimValue generation (reserved range 8000-9999 exclusion), status transitions, validation, hierarchy, and business rules.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create partner | Valid data | Create | Partner created |
| POS-002 | Get partner by ID | Partner exists | GetById | Partner returned |
| POS-003 | Update partner | Partner exists | Update | Updated |
| POS-004 | Delete partner | Partner exists | Delete | Soft deleted |
| POS-005 | List partners | Partners exist | List | List returned |
| POS-006 | GetNextErpDimValue sequential | Partners exist | GetNextErpDimValue | Next value |
| POS-007 | GetNextErpDimValue skip reserved | Values in 8000-9999 | GetNextErpDimValue | Skips reserved |
| POS-008 | Submit for approval | Partner draft | Submit | Submitted |
| POS-009 | Approve partner | Partner submitted | Approve | Approved |
| POS-010 | Reject partner | Partner submitted | Reject | Rejected |
| POS-011 | Activate partner | Partner approved | Activate | Active |
| POS-012 | Deactivate partner | Partner active | Deactivate | Inactive |
| POS-013 | Audit CreatedBy | Create | Check audit | Set |
| POS-014 | Audit CreatedDate | Create | Check audit | UTC |
| POS-015 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-016 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-017 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-018 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-019 | Pagination | Many partners | List page | Page |
| POS-020 | Sort by name | Partners exist | Sort | Ordered |
| POS-021 | Filter by status | Partners exist | Filter | Filtered |
| POS-022 | Search by name | Partners exist | Search | Matching |
| POS-023 | Get hierarchy | Partner has parent | GetHierarchy | Hierarchy |
| POS-024 | Validate partner | Valid partner | Validate | Valid |
| POS-025 | ErpDimValue unique | New partner | Create | Unique value |
| POS-026 | Status transition valid | Valid transition | ChangeStatus | Changed |
| POS-027 | Get by category | Partners exist | GetByCategory | Filtered |
| POS-028 | Get by type | Partners exist | GetByType | Filtered |
| POS-029 | Bulk get | IDs valid | GetByIds | Partners |
| POS-030 | Export partners | Partners exist | Export | Exported |

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
| NEG-008 | Invalid status | Status=invalid | ArgumentException |
| NEG-009 | Submit wrong status | Partner active | BusinessException |
| NEG-010 | Approve wrong status | Partner draft | BusinessException |
| NEG-011 | GetById without permission | Unauthorized | Forbidden |
| NEG-012 | Create without permission | Unauthorized | Forbidden |
| NEG-013 | Update without permission | Unauthorized | Forbidden |
| NEG-014 | Delete without permission | Unauthorized | Forbidden |
| NEG-015 | Approve without permission | Unauthorized | Forbidden |
| NEG-016 | ErpDimValue reserved range | Value 8000-9999 | BusinessException |
| NEG-017 | SQL injection in search | '; DROP | Rejected |
| NEG-018 | XSS in name | <script> | Escaped |
| NEG-019 | Duplicate ErpDimValue | Value exists | BusinessException |
| NEG-020 | Duplicate name same parent | Name exists | BusinessException |
| NEG-021 | Invalid category | Category invalid | ArgumentException |
| NEG-022 | Invalid type | Type invalid | ArgumentException |
| NEG-023 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-024 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-025 | Connection timeout | DB unavailable | TimeoutException |
| NEG-026 | Move to self | ParentId=SelfId | BusinessException |
| NEG-027 | Create circular | Parent in subtree | BusinessException |
| NEG-028 | Expired session | Expired token | Unauthorized |
| NEG-029 | Null user context | User=null | InvalidOperationException |
| NEG-030 | GetNextErpDimValue no partners | No partners | Returns first |
| NEG-031 | Activate wrong status | Partner draft | BusinessException |
| NEG-032 | Deactivate wrong status | Partner draft | BusinessException |
| NEG-033 | Reject wrong status | Partner active | BusinessException |
| NEG-034 | Invalid page number | Page=0 | ArgumentException |
| NEG-035 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-036 | Search null term | Term=null | ArgumentNullException |
| NEG-037 | Filter malformed | Malformed filter | ArgumentException |
| NEG-038 | Update deleted | Partner deleted | KeyNotFoundException |
| NEG-039 | GetById deleted | Partner deleted | KeyNotFoundException |
| NEG-040 | Submit deleted | Partner deleted | KeyNotFoundException |
| NEG-041 | Child override throws | Child throws | Propagated |
| NEG-042 | ErpDimValue overflow | Max int | Handle |
| NEG-043 | Category required | Category=null | ValidationException |
| NEG-044 | Type required | Type=null | ValidationException |
| NEG-045 | Audit missing user | User=0 | InvalidOperationException |
| NEG-046 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-047 | GetByIds null | Ids=null | ArgumentNullException |
| NEG-048 | GetByIds empty | Ids=[] | ArgumentException |
| NEG-049 | Pagination overflow | Page too large | Empty or error |
| NEG-050 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-051 | Export invalid format | Format invalid | ArgumentException |
| NEG-052 | Parent deleted | Parent deleted | KeyNotFoundException |
| NEG-053 | Hierarchy invalid | Invalid hierarchy | BusinessException |
| NEG-054 | Status transition invalid | Invalid transition | BusinessException |
| NEG-055 | ErpDimValue negative | Negative value | ValidationException |
| NEG-056 | Name too long | Length>255 | ValidationException |
| NEG-057 | Code duplicate | Code exists | BusinessException |
| NEG-058 | Bulk create invalid | One invalid | Partial or fail |
| NEG-059 | Export empty | No partners | Empty or error |
| NEG-060 | GetHierarchy deleted | Partner deleted | KeyNotFoundException |
| NEG-061 | AddContact invalid | Contact invalid | ArgumentException |
| NEG-062 | AddDocument invalid | Document invalid | ArgumentException |
| NEG-063 | Workflow blocked | Blocked state | BusinessException |
| NEG-064 | Cross-tenant parent | Other tenant | Forbidden |
| NEG-065 | Invalid include path | Invalid include | ArgumentException |
| NEG-066 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-067 | Invalid enum value | Status invalid | ArgumentException |
| NEG-068 | Reject without reason | Reason empty | ValidationException |
| NEG-069 | Approve without role | Wrong role | Forbidden |
| NEG-070 | ErpDimValue collision | Race on create | Handled |
| NEG-071 | Create null request | Request=null | ArgumentNullException |
| NEG-072 | Update null request | Request=null | ArgumentNullException |
| NEG-073 | GetWorkflowStatus invalid | Id=0 | ArgumentException |
| NEG-074 | Submit null reason | Reason=null | ArgumentNullException |
| NEG-075 | Reject null reason | Reason=null | ArgumentNullException |
| NEG-076 | Activate wrong workflow | Wrong stage | BusinessException |
| NEG-077 | Deactivate wrong workflow | Wrong stage | BusinessException |
| NEG-078 | GetByCategory invalid | Category invalid | ArgumentException |
| NEG-079 | GetByType invalid | Type invalid | ArgumentException |
| NEG-080 | GetHierarchy null partner | Partner=null | ArgumentNullException |
| NEG-081 | Validate null partner | Partner=null | ArgumentNullException |
| NEG-082 | GetNextErpDimValue invalid | Config invalid | ArgumentException |
| NEG-083 | AddContact null contact | Contact=null | ArgumentNullException |
| NEG-084 | AddDocument null document | Document=null | ArgumentNullException |
| NEG-085 | Export null filter | Filter=null | ArgumentNullException |
| NEG-086 | GetByIds duplicate IDs | Duplicate IDs | Handle |
| NEG-087 | Create with reserved ErpDimValue | Value 8000 | BusinessException |
| NEG-088 | Update to reserved ErpDimValue | Value 8000 | BusinessException |
| NEG-089 | Submit without required fields | Missing fields | ValidationException |
| NEG-090 | Approve without submit | Direct approve | BusinessException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min | Length=1 | Valid |
| BND-002 | Name at max | Length=255 | Valid |
| BND-003 | Name exceeds max | Length=256 | Reject |
| BND-004 | ErpDimValue at 7999 | Value=7999 | Valid |
| BND-005 | ErpDimValue at 8000 | Value=8000 | Reserved |
| BND-006 | ErpDimValue at 9999 | Value=9999 | Reserved |
| BND-007 | ErpDimValue at 10000 | Value=10000 | Valid |
| BND-008 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-009 | ID at zero | Id=0 | Reject |
| BND-010 | Page size at min | PageSize=1 | Valid |
| BND-011 | Page size at max | PageSize=1000 | Valid |
| BND-012 | Page size over max | PageSize=1001 | Reject |
| BND-013 | First ErpDimValue | No partners | 1 or config |
| BND-014 | ErpDimValue sequence gap | Gap in sequence | Next available |
| BND-015 | Unicode in name | Arabic/Chinese | Stored |
| BND-016 | Special chars in name | <>&"' | Escaped |
| BND-017 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-018 | Empty children | No children | [] |
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
| BND-029 | Status enum boundary | Last enum | Valid |
| BND-030 | Category enum boundary | Last enum | Valid |
| BND-031 | Pagination last partial | Partial page | Correct |
| BND-032 | Pagination total | Total count | Accurate |
| BND-033 | Sort null handling | Nulls in data | Deterministic |
| BND-034 | Filter combination all | All filters | Correct |
| BND-035 | Parent null for root | ParentId=null | Valid |
| BND-036 | Parent max int | ParentId=2147483647 | Handle |
| BND-037 | Soft delete boundary | DeletedDate set | Excluded |
| BND-038 | Include depth | Deep include | No explosion |
| BND-039 | Query timeout | Slow query | Timeout |
| BND-040 | Audit timestamp precision | Millisecond | Stored |
| BND-041 | Code max length | Length=50 | Valid |
| BND-042 | Code over max | Length=51 | Reject |
| BND-043 | Async cancellation | Cancel token | OperationCanceledException |
| BND-044 | Task timeout | Timeout | TimeoutException |
| BND-045 | Concurrent same second | Same timestamp | Deterministic |
| BND-046 | Workflow status boundary | Each status | Valid |
| BND-047 | ErpDimValue max int | 2147483647 | Handle |
| BND-048 | Export large result | 10k rows | Stream |
| BND-049 | Hierarchy depth max | Max depth | Valid |
| BND-050 | Hierarchy depth over | Too deep | Reject |
| BND-051 | Filter empty result | No match | Empty list |
| BND-052 | Sort empty | Empty list | No exception |
| BND-053 | Pagination empty | No data | Empty |
| BND-054 | GetByIds max | 1000 IDs | Valid |
| BND-055 | GetByIds over max | 1001 IDs | Reject |
| BND-056 | Contacts count max | Many contacts | Valid |
| BND-057 | Documents count max | Many documents | Valid |
| BND-058 | Approval date boundary | Approval date | Stored |
| BND-059 | Rejection date boundary | Rejection date | Stored |
| BND-060 | Submit date boundary | Submit date | Stored |
| BND-061 | Reserved range boundary | 8000-9999 | Excluded |
| BND-062 | ErpDimValue before reserved | 7999 | Valid |
| BND-063 | ErpDimValue after reserved | 10000 | Valid |
| BND-064 | Bulk create max | 100 partners | Valid |
| BND-065 | Bulk create over max | 101 partners | Reject |
| BND-066 | Status transition all | All transitions | Valid |
| BND-067 | Category all | All categories | Valid |
| BND-068 | Type all | All types | Valid |
| BND-069 | Concurrent GetNextErpDimValue | Two concurrent | Unique |
| BND-070 | Export format boundary | Each format | Valid |
| BND-071 | Name whitespace only | Name="   " | Reject |
| BND-072 | Code at min | Length=1 | Valid |
| BND-073 | Code empty | Code="" | Config |
| BND-074 | GetWorkflowStatus boundary | Each status | Valid |
| BND-075 | Submit-approve flow | Full flow | Valid |
| BND-076 | Submit-reject flow | Full flow | Valid |
| BND-077 | ErpDimValue at 1 | First | Valid |
| BND-078 | Hierarchy single level | One level | Valid |
| BND-079 | GetByCategory empty | No match | [] |
| BND-080 | GetByType empty | No match | [] |
| BND-081 | GetByIds single | One ID | Valid |
| BND-082 | Export empty result | No data | Empty |
| BND-083 | Search partial match | Partial | Matching |
| BND-084 | Filter multi-status | Multiple | Filtered |
| BND-085 | Sort multi-column | 3 columns | Correct |
| BND-086 | Parent assignment boundary | Root to child | Valid |
| BND-087 | Contact count zero | No contacts | [] |
| BND-088 | Document count zero | No documents | [] |
| BND-089 | Workflow status transition | Each transition | Valid |
| BND-090 | ErpDimValue sequence boundary | At boundary | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | Category required | Validation | Create | Reject if invalid |
| FUN-003 | Type required | Validation | Create | Reject if invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | ErpDimValue reserved range | Constraint | GetNextErpDimValue | Skip 8000-9999 |
| FUN-008 | ErpDimValue unique | Constraint | Create | Unique value |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Status transition valid | Constraint | ChangeStatus | Valid only |
| FUN-017 | Submit requires draft | Constraint | Submit | Draft only |
| FUN-018 | Approve requires submitted | Constraint | Approve | Submitted only |
| FUN-019 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-020 | No circular hierarchy | Constraint | Create/Move | Reject cycle |
| FUN-021 | GetNextErpDimValue sequential | Logic | GetNextErpDimValue | Max+1 |
| FUN-022 | Submit updates status | Logic | Submit | Submitted |
| FUN-023 | Approve updates status | Logic | Approve | Approved |
| FUN-024 | Reject updates status | Logic | Reject | Rejected |
| FUN-025 | Activate updates status | Logic | Activate | Active |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on update | Transaction | Update | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads parent | Data load | GetById include | Parent loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | ErpDimValue on create | Logic | Create | Assigned |
| FUN-036 | Hierarchy validation | Logic | Create | Valid |
| FUN-037 | Duplicate name check | Logic | Create | Reject duplicate |
| FUN-038 | Duplicate code check | Logic | Create | Reject duplicate |
| FUN-039 | Status workflow | Logic | ChangeStatus | Valid flow |
| FUN-040 | Approval workflow | Logic | Approve | Complete |
| FUN-041 | Rejection workflow | Logic | Reject | Complete |
| FUN-042 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-043 | Search by name | Logic | Search | Name match |
| FUN-044 | Filter by status | Logic | Filter | Status match |
| FUN-045 | Config ErpDimValue range | Config | GetNextErpDimValue | Config |
| FUN-046 | Config reserved range | Config | GetNextErpDimValue | 8000-9999 |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Workflow notifications | Workflow | Status change | Sent |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | GetHierarchy excludes deleted | Constraint | GetHierarchy | Excludes |
| FUN-052 | GetByCategory excludes deleted | Constraint | GetByCategory | Excludes |
| FUN-053 | GetByType excludes deleted | Constraint | GetByType | Excludes |
| FUN-054 | GetByIds excludes deleted | Constraint | GetByIds | Excludes |
| FUN-055 | Submit audit | Audit | Submit | Audit |
| FUN-056 | Approve audit | Audit | Approve | Audit |
| FUN-057 | Reject audit | Audit | Reject | Audit |
| FUN-058 | Activate audit | Audit | Activate | Audit |
| FUN-059 | Deactivate audit | Audit | Deactivate | Audit |
| FUN-060 | GetWorkflowStatus logic | Logic | GetWorkflowStatus | Status |
| FUN-061 | AddContact association | Logic | AddContact | Associated |
| FUN-062 | AddDocument association | Logic | AddDocument | Associated |
| FUN-063 | Parent assignment validation | Validation | Create | Valid |
| FUN-064 | Category validation | Validation | Create | Valid |
| FUN-065 | Type validation | Validation | Create | Valid |
| FUN-066 | Status validation | Validation | ChangeStatus | Valid |
| FUN-067 | ErpDimValue validation | Validation | Create | Valid |
| FUN-068 | Name validation | Validation | Create | Valid |
| FUN-069 | Code validation | Validation | Create | Valid |
| FUN-070 | Reject reason required | Validation | Reject | Required |
| FUN-071 | GetNextErpDimValue transaction | Transaction | GetNextErpDimValue | Atomic |
| FUN-072 | Submit transaction | Transaction | Submit | Atomic |
| FUN-073 | Approve transaction | Transaction | Approve | Atomic |
| FUN-074 | Reject transaction | Transaction | Reject | Atomic |
| FUN-075 | Activate transaction | Transaction | Activate | Atomic |
| FUN-076 | Deactivate transaction | Transaction | Deactivate | Atomic |
| FUN-077 | Export format | Logic | Export | Format |
| FUN-078 | GetByIds order | Logic | GetByIds | Order |
| FUN-079 | Search relevance | Logic | Search | Relevance |
| FUN-080 | Filter combination | Logic | Filter | Combined |
| FUN-081 | Sort multi-field | Logic | Sort | Multi-field |
| FUN-082 | Pagination total | Calculation | Paginate | Total |
| FUN-083 | Hierarchy depth | Logic | GetHierarchy | Depth |
| FUN-084 | Validate required fields | Validation | Validate | Required |
| FUN-085 | Contact count limit | Constraint | AddContact | Limit |
| FUN-086 | Document count limit | Constraint | AddDocument | Limit |
| FUN-087 | Workflow state machine | Logic | ChangeStatus | State |
| FUN-088 | ErpDimValue format | Logic | GetNextErpDimValue | Format |
| FUN-089 | Export encoding | Logic | Export | Encoding |
| FUN-090 | Bulk get validation | Validation | GetByIds | Valid |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create partner full flow | Create | Partner | Created |
| INT-002 | Get partner full flow | GetById | Partner | Returned |
| INT-003 | Update partner full flow | Update | Partner | Updated |
| INT-004 | Delete partner full flow | Delete | Partner | Soft deleted |
| INT-005 | GetNextErpDimValue full flow | GetNextErpDimValue | Partner | Value |
| INT-006 | Submit-approve full flow | Submit, Approve | Partner | Approved |
| INT-007 | Submit-reject full flow | Submit, Reject | Partner | Rejected |
| INT-008 | Get with contacts | GetById | Partner, Contact | Contacts loaded |
| INT-009 | List with filter and sort | List | Partner | Filtered, sorted |
| INT-010 | Get hierarchy | GetHierarchy | Partner | Hierarchy |
| INT-011 | Partner-Parent relationship | Relationship | Partner | FK valid |
| INT-012 | Partner-Contact relationship | Relationship | Partner, Contact | Valid |
| INT-013 | Partner-Document relationship | Relationship | Partner, Document | Valid |
| INT-014 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-015 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-016 | DB error handling | Error | DB down | Graceful |
| INT-017 | Timeout handling | Error | Slow | Timeout |
| INT-018 | Constraint violation | Error | FK violation | Clear error |
| INT-019 | Permission service integration | Integration | Permission | Check |
| INT-020 | User resolver integration | Integration | User | Resolved |
| INT-021 | Audit context integration | Integration | Audit | Context |
| INT-022 | Logger integration | Integration | Log | Logged |
| INT-023 | WorkflowManager integration | Integration | Workflow | Status |
| INT-024 | Mapper integration | Integration | Map | Correct |
| INT-025 | Repository integration | Integration | Repository | CRUD |
| INT-026 | DbContext integration | Integration | DbContext | Scoped |
| INT-027 | Transaction scope | Integration | Transaction | Atomic |
| INT-028 | ContactManager integration | Integration | Contact | Contact |
| INT-029 | DocumentManager integration | Integration | Document | Document |
| INT-030 | ErpDimValue sequence | Scenario | Create multiple | Sequential |
| INT-031 | Reserved range exclusion | Scenario | Partners with 8000-9999 | Skip |
| INT-032 | Full approval workflow | Scenario | Draft→Submit→Approve | Complete |
| INT-033 | Full rejection workflow | Scenario | Draft→Submit→Reject | Complete |
| INT-034 | Concurrent create | Scenario | Parallel | All succeed |
| INT-035 | Hierarchy with children | Scenario | Partner | Children |
| INT-036 | Search with filter | Scenario | Search | Filtered |
| INT-037 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-038 | Export with filter | Scenario | Export | Filtered |
| INT-039 | Bulk get | Scenario | GetByIds | Partners |
| INT-040 | Status transitions all | Scenario | All transitions | Complete |
| INT-041 | ErpDimValue overflow | Scenario | Many partners | Handled |
| INT-042 | Parent assignment | Scenario | Create with parent | Assigned |
| INT-043 | Contact association | Scenario | AddContact | Associated |
| INT-044 | Document association | Scenario | AddDocument | Associated |
| INT-045 | Workflow integration | Scenario | ChangeStatus | Updated |
| INT-046 | Audit trail | Scenario | Create, Update | Trail |
| INT-047 | Soft delete cascade | Scenario | Delete | Deleted |
| INT-048 | Restore from delete | Scenario | Restore | Restored |
| INT-049 | PNO-686 regression | Scenario | ErpDimValue | No regression |
| INT-050 | E2E create-approve-activate | Scenario | Full cycle | Complete |
| INT-051 | Create then GetById | Scenario | Create, Get | Complete |
| INT-052 | Update then GetById | Scenario | Update, Get | Complete |
| INT-053 | Submit then Approve | Scenario | Submit, Approve | Complete |
| INT-054 | Submit then Reject | Scenario | Submit, Reject | Complete |
| INT-055 | Approve then Activate | Scenario | Approve, Activate | Complete |
| INT-056 | Activate then Deactivate | Scenario | Activate, Deactivate | Complete |
| INT-057 | GetNextErpDimValue then Create | Scenario | GetNext, Create | Complete |
| INT-058 | GetHierarchy then GetById | Scenario | Hierarchy, Get | Complete |
| INT-059 | AddContact then GetById | Scenario | AddContact, Get | Complete |
| INT-060 | AddDocument then GetById | Scenario | AddDocument, Get | Complete |
| INT-061 | Search then GetById | Scenario | Search, Get | Complete |
| INT-062 | Filter then Paginate | Scenario | Filter, Paginate | Complete |
| INT-063 | Export then Import | Scenario | Export, Import | Complete |
| INT-064 | GetByCategory then GetById | Scenario | Category, Get | Complete |
| INT-065 | GetByType then GetById | Scenario | Type, Get | Complete |
| INT-066 | GetByIds then Update | Scenario | GetByIds, Update | Complete |
| INT-067 | GetWorkflowStatus then Submit | Scenario | Status, Submit | Complete |
| INT-068 | Validate then Create | Scenario | Validate, Create | Complete |
| INT-069 | Create with parent | Scenario | Create | Parent |
| INT-070 | Update with contacts | Scenario | Update | Contacts |
| INT-071 | Delete with documents | Scenario | Delete | Documents |
| INT-072 | GetHierarchy with parent | Scenario | GetHierarchy | Parent |
| INT-073 | Search with category | Scenario | Search | Category |
| INT-074 | Filter with type | Scenario | Filter | Type |
| INT-075 | Export with sort | Scenario | Export | Sorted |
| INT-076 | GetByIds with filter | Scenario | GetByIds | Filtered |
| INT-077 | Submit with validation | Scenario | Submit | Validated |
| INT-078 | Approve with audit | Scenario | Approve | Audit |
| INT-079 | Reject with reason | Scenario | Reject | Reason |
| INT-080 | Activate with workflow | Scenario | Activate | Workflow |
| INT-081 | Deactivate with workflow | Scenario | Deactivate | Workflow |
| INT-082 | GetNextErpDimValue with reserved | Scenario | GetNext | Reserved |
| INT-083 | Create with category | Scenario | Create | Category |
| INT-084 | Create with type | Scenario | Create | Type |
| INT-085 | Update with status | Scenario | Update | Status |
| INT-086 | Delete with hierarchy | Scenario | Delete | Hierarchy |
| INT-087 | Export with encoding | Scenario | Export | Encoding |
| INT-088 | GetWorkflowStatus all | Scenario | GetWorkflowStatus | All |
| INT-089 | ErpDimValue sequence full | Scenario | Full sequence | Complete |
| INT-090 | E2E full partner lifecycle | Scenario | Full cycle | Complete |

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
| SEC-014 | Unauthorized approve | No permission | Approve | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B partner | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR update other | Id=other | Update | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | ParentId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign ErpDimValue | ErpDimValue=manipulated | Request | Ignored |
| SEC-025 | ErpDimValue manipulation | Tamper value | Create | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Approval tampering | Tamper approval | Approve | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit list | Many lists | List | Throttled |
| SEC-037 | Rate limit approve | Many approves | Approve | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in name | Name | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge export | Export | Rejected |
| SEC-045 | ErpDimValue injection | Invalid value | Create | Rejected |
| SEC-046 | Status injection | Invalid status | Update | Rejected |
| SEC-047 | Parent injection | Invalid parent | Create | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Export ACL | Direct access | Export | Denied |

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
| CON-008 | Race on GetNextErpDimValue | Two concurrent | Unique values |
| CON-009 | Race on approve | Two approve | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel gets | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Export concurrent | Two export | Both succeed |
| CON-016 | Submit concurrent | Two submit | One wins |
| CON-017 | Approve concurrent | Two approve | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Create concurrent | Two create | Both succeed |
| CON-020 | Update concurrent | Two update | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | ErpDimValue pool | Many concurrent | Unique |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate category | Validation | Valid category | Pass |
| UNT-003 | Validate type | Validation | Valid type | Pass |
| UNT-004 | Validate status | Validation | Valid status | Pass |
| UNT-005 | Validate ErpDimValue | Validation | Valid value | Pass |
| UNT-006 | Format name | Formatting | Name | Formatted |
| UNT-007 | Format code | Formatting | Code | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | ErpDimValue next | Calculation | Current | Next |
| UNT-013 | Reserved range check | Calculation | Value | In/out |
| UNT-014 | Status allows submit | Status logic | Draft | true |
| UNT-015 | Status allows approve | Status logic | Submitted | true |
| UNT-016 | Status allows activate | Status logic | Approved | true |
| UNT-017 | Hierarchy check | Status logic | Parent | Valid |
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
| PRF-003 | GetNextErpDimValue | GetNextErpDimValue | <50ms | P1 |
| PRF-004 | List 100 | List | <300ms | P0 |
| PRF-005 | List 1000 | List | <2s | P0 |
| PRF-006 | Search by name | Search | <500ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Get hierarchy | GetHierarchy | <200ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 creates | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 create | <5s total | P2 |
| PRF-013 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-014 | Memory export 10k | Export | <100MB | P2 |
| PRF-015 | Memory hierarchy | GetHierarchy | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS create | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS create | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS GetNextErpDimValue | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress create | Many creates | Until limit | Holds |
| LDT-008 | Stress memory | Large export | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
