# BaseEngagementManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/BaseEngagementManager` (Unit Tests)  
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

Base engagement manager provides shared engagement lifecycle logic, common patterns for engagement entities, and inheritance for partner/opportunity engagement flows. Tests cover: engagement CRUD, workflow integration, status management, shared validation, and inheritance behavior.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create engagement with valid data | User authorized | Create engagement | Engagement created |
| POS-002 | Get engagement by ID | Engagement exists | GetById | Returns engagement |
| POS-003 | Update engagement fields | Engagement exists | Update | Fields updated |
| POS-004 | Soft delete engagement | Engagement exists | Delete | IsDeleted=true |
| POS-005 | List engagements with filter | Engagements exist | List with filter | Filtered list returned |
| POS-006 | Submit engagement for workflow | Draft engagement | Submit | Status transitions |
| POS-007 | Approve engagement | Submitted engagement | Approve | Approved |
| POS-008 | Reject engagement | Submitted engagement | Reject | Rejected |
| POS-009 | Withdraw engagement | In workflow | Withdraw | Returns to draft |
| POS-010 | Get status history | Actions performed | GetHistory | Chronological history |
| POS-011 | Get engagement with includes | Engagement exists | GetById with includes | Related data loaded |
| POS-012 | Valid status transition | Valid current status | ChangeStatus | New status applied |
| POS-013 | Link engagement to partner | Partner exists | Link | Association created |
| POS-014 | Add participants | Engagement exists | AddParticipants | Participants added |
| POS-015 | Remove participant | Participant exists | Remove | Participant removed |
| POS-016 | Check edit permission | User has permission | CheckPermission | Returns true |
| POS-017 | Check view permission | User has permission | CheckPermission | Returns true |
| POS-018 | Get engagement by partner ID | Partner has engagements | GetByPartner | List returned |
| POS-019 | Get engagement by organization | Org has engagements | GetByOrg | List returned |
| POS-020 | Export engagement summary | Engagement exists | Export | Summary returned |
| POS-021 | Validate workflow transition | Valid transition | Validate | Validation passes |
| POS-022 | Get available actions | Engagement in state | GetActions | Actions returned |
| POS-023 | Inherited create from child | Child manager | Create | Base logic invoked |
| POS-024 | Inherited update from child | Child manager | Update | Base logic invoked |
| POS-025 | Inherited delete from child | Child manager | Delete | Base logic invoked |
| POS-026 | Shared validation passes | Valid input | Validate | No errors |
| POS-027 | Audit fields populated | Create/Update | Check audit | CreatedBy/Date set |
| POS-028 | Workflow status propagated | Status change | Check entity | WorkflowStatus updated |
| POS-029 | Engagement type resolved | Multi-type support | Create | Type correctly set |
| POS-030 | Bulk get by IDs | Multiple IDs | GetByIds | All returned |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ValidationException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Create with whitespace name | Name="   " | ValidationException |
| NEG-004 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-005 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-006 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | Submit already submitted | In workflow | BusinessException |
| NEG-009 | Approve as non-DoA | Wrong role | UnauthorizedAccessException |
| NEG-010 | Reject as non-DoA | Wrong role | UnauthorizedAccessException |
| NEG-011 | Invalid status transition | Invalid transition | BusinessException |
| NEG-012 | Cancel already approved | Approved | BusinessException |
| NEG-013 | Link to deleted partner | Partner IsDeleted | BusinessException |
| NEG-014 | Add null participant | Participant=null | ArgumentNullException |
| NEG-015 | Remove non-existent participant | Invalid id | KeyNotFoundException |
| NEG-016 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-017 | GetById without permission | Unauthorized user | Forbidden |
| NEG-018 | Create without permission | Unauthorized user | Forbidden |
| NEG-019 | Update without permission | Unauthorized user | Forbidden |
| NEG-020 | Delete without permission | Unauthorized user | Forbidden |
| NEG-021 | Submit without required fields | Missing fields | ValidationException |
| NEG-022 | Invalid partner ID | PartnerId=-1 | ArgumentException |
| NEG-023 | Invalid organization ID | OrgId=0 | ArgumentException |
| NEG-024 | Duplicate engagement name | Same name exists | BusinessException |
| NEG-025 | Withdraw not in workflow | Draft | BusinessException |
| NEG-026 | Approve closed engagement | Closed | BusinessException |
| NEG-027 | Reject closed engagement | Closed | BusinessException |
| NEG-028 | Null request object | Request=null | ArgumentNullException |
| NEG-029 | Null filter object | Filter=null | ArgumentNullException |
| NEG-030 | Invalid date range | End before start | ArgumentException |
| NEG-031 | Invalid page number | Page=0 | ArgumentException |
| NEG-032 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-033 | Negative page size | PageSize=-1 | ArgumentException |
| NEG-034 | Overflow page size | PageSize=10001 | ArgumentException |
| NEG-035 | Link to null entity | Entity=null | ArgumentNullException |
| NEG-036 | Invalid workflow action | Unknown action | ArgumentException |
| NEG-037 | Status history for invalid ID | Id=0 | KeyNotFoundException |
| NEG-038 | Get actions for invalid state | Invalid state | ArgumentException |
| NEG-039 | Export deleted engagement | IsDeleted=true | KeyNotFoundException |
| NEG-040 | Create with invalid type | Type=invalid | ArgumentException |
| NEG-041 | Update with invalid status | Status=invalid | ArgumentException |
| NEG-042 | Soft delete already deleted | IsDeleted=true | BusinessException |
| NEG-043 | Inherited validation fails | Child invalid data | ValidationException |
| NEG-044 | Circular reference | Self-reference | BusinessException |
| NEG-045 | Expired session | Expired token | Unauthorized |
| NEG-046 | Null user context | User=null | InvalidOperationException |
| NEG-047 | Invalid include path | Invalid include | ArgumentException |
| NEG-048 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-049 | Concurrent update conflict | Stale entity | DbUpdateConcurrencyException |
| NEG-050 | Transaction rollback | Fail in transaction | Rollback |
| NEG-051 | Connection timeout | DB unavailable | TimeoutException |
| NEG-052 | Null navigation property | Unloaded nav | NullReferenceException |
| NEG-053 | Invalid enum value | Out-of-range enum | ArgumentException |
| NEG-054 | Create with deleted related | Related deleted | BusinessException |
| NEG-055 | Update with stale version | Old version | ConcurrencyException |
| NEG-056 | Submit with missing DoA | No DoA configured | BusinessException |
| NEG-057 | Permission check null resource | Resource=null | ArgumentNullException |
| NEG-058 | Bulk get with null IDs | Ids=null | ArgumentNullException |
| NEG-059 | Bulk get with empty IDs | Ids=[] | Returns empty |
| NEG-060 | Invalid sort field | SortField=invalid | ArgumentException |
| NEG-061 | Invalid sort direction | SortDir=invalid | ArgumentException |
| NEG-062 | Filter by invalid status | Status=invalid | ArgumentException |
| NEG-063 | GetByPartner deleted partner | Partner deleted | Empty list |
| NEG-064 | GetByOrg invalid org | OrgId=-1 | ArgumentException |
| NEG-065 | Export format invalid | Format=invalid | ArgumentException |
| NEG-066 | Validate null transition | Transition=null | ArgumentNullException |
| NEG-067 | GetActions null state | State=null | ArgumentNullException |
| NEG-068 | Child override throws | Child throws | Propagated exception |
| NEG-069 | Audit trail missing user | User=0 | InvalidOperationException |
| NEG-070 | Workflow status invalid | Invalid status | ArgumentException |
| NEG-071 | AddAttachment null attachment | Attachment=null | ArgumentNullException |
| NEG-072 | RemoveAttachment non-existent | Id invalid | KeyNotFoundException |
| NEG-073 | GetAttachments invalid engagement | EngagementId=0 | ArgumentException |
| NEG-074 | SetDueDate invalid date | Date past | ArgumentException |
| NEG-075 | GetDueEngagements invalid range | End<Start | ArgumentException |
| NEG-076 | AssignOwner null owner | Owner=null | ArgumentNullException |
| NEG-077 | UnassignOwner not assigned | No owner | BusinessException |
| NEG-078 | AddComment null comment | Comment=null | ArgumentNullException |
| NEG-079 | GetComments invalid engagement | EngagementId=0 | ArgumentException |
| NEG-080 | SetPriority invalid priority | Priority invalid | ArgumentException |
| NEG-081 | GetByPriority invalid | Priority invalid | ArgumentException |
| NEG-082 | LinkDocument null document | Document=null | ArgumentNullException |
| NEG-083 | UnlinkDocument not linked | No link | BusinessException |
| NEG-084 | GetLinkedDocuments invalid | EngagementId=0 | ArgumentException |
| NEG-085 | CloneEngagement null source | Source=null | ArgumentNullException |
| NEG-086 | ArchiveEngagement deleted | Engagement deleted | KeyNotFoundException |
| NEG-087 | RestoreEngagement not archived | Not archived | InvalidOperationException |
| NEG-088 | ValidateBudget null budget | Budget=null | ArgumentNullException |
| NEG-089 | GetBudgetSummary invalid | EngagementId=0 | ArgumentException |
| NEG-090 | Duplicate participant | Same participant | BusinessException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min length | Name length=1 | Valid |
| BND-002 | Name at max length | Name length=200 | Valid |
| BND-003 | Name exceeds max | Name length=201 | Reject |
| BND-004 | Description at max | Description=4096 | Valid |
| BND-005 | Description over max | Description=4097 | Reject |
| BND-006 | ID at Int32.MaxValue | Id=2147483647 | Handle or reject |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Page number at 1 | Page=1 | Valid |
| BND-011 | Page number at max | Page=large | Return empty or valid |
| BND-012 | Empty participant list | Participants=[] | Valid |
| BND-013 | Single participant | Count=1 | Valid |
| BND-014 | Max participants | Count=limit | Valid or reject |
| BND-015 | Date at min | Date=DateTime.MinValue | Handle |
| BND-016 | Date at max | Date=DateTime.MaxValue | Handle |
| BND-017 | DateTime UTC | UTC input | Stored correctly |
| BND-018 | Timezone edge | DST transition | Correct handling |
| BND-019 | Decimal precision | Amount 2 decimals | Stored correctly |
| BND-020 | Decimal overflow | Very large amount | Reject or handle |
| BND-021 | Zero amount | Amount=0 | Valid if allowed |
| BND-022 | Negative amount | Amount=-1 | Reject |
| BND-023 | Unicode in name | Arabic/Chinese | Stored correctly |
| BND-024 | Emoji in name | Emoji chars | Sanitize or reject |
| BND-025 | Special chars in name | <>&"' | Escaped/sanitized |
| BND-026 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-027 | Newlines in description | \n\r | Handled |
| BND-028 | Empty filter | Filter empty | Return all |
| BND-029 | Filter all null | All null | Default behavior |
| BND-030 | Status enum first | Status=first | Valid |
| BND-031 | Status enum last | Status=last | Valid |
| BND-032 | Workflow state transition | Edge state | Correct transition |
| BND-033 | Concurrent same second | Same timestamp | Order deterministic |
| BND-034 | Long string in remarks | 4000 chars | Truncate or reject |
| BND-035 | Empty string optional | Optional=" " | Treat as empty |
| BND-036 | Zero partner ID | PartnerId=0 | Reject |
| BND-037 | Zero org ID | OrgId=0 | Reject |
| BND-038 | Max int for ID | Id=2147483647 | Handle |
| BND-039 | Collection empty | Collection=[] | No N+1 |
| BND-040 | Collection single item | Count=1 | Valid |
| BND-041 | Collection max items | At limit | Valid or paginate |
| BND-042 | Float precision | Float field | Correct precision |
| BND-043 | Boolean edge | True/False | Correct |
| BND-044 | Nullable null | Nullable=null | Valid |
| BND-045 | Nullable set | Nullable=value | Valid |
| BND-046 | Guid empty | Guid=empty | Reject |
| BND-047 | Guid valid | Valid Guid | Stored |
| BND-048 | JSON max size | Large JSON | Reject or stream |
| BND-049 | Attachment count zero | Count=0 | Valid |
| BND-050 | Attachment count max | At limit | Valid |
| BND-051 | Role enum boundary | First/Last | Valid |
| BND-052 | Permission mask all | All bits set | Valid |
| BND-053 | Permission mask none | No bits | Valid |
| BND-054 | Audit timestamp precision | Millisecond | Stored correctly |
| BND-055 | Soft delete Date | DeletedDate set | Query excludes |
| BND-056 | Include depth | Deep include | No Cartesian explosion |
| BND-057 | Query timeout | Slow query | Timeout or success |
| BND-058 | Memory large result | 10k rows | No OOM |
| BND-059 | Empty search term | Term="" | Return all |
| BND-060 | Search term max length | Term=500 | Valid |
| BND-061 | Search term over max | Term=501 | Reject |
| BND-062 | Filter combination all | All filters | Correct result |
| BND-063 | Sort multi-column | 3 columns | Correct order |
| BND-064 | Sort null handling | Nulls in data | Deterministic |
| BND-065 | Pagination last partial | Partial page | Correct count |
| BND-066 | Pagination total count | Total count | Accurate |
| BND-067 | Inheritance override | Child override | Child behavior |
| BND-068 | Base virtual call | Virtual method | Base or override |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Task timeout | TimeoutException |
| BND-071 | Name exactly 200 chars | Length=200 | Valid |
| BND-072 | Description exactly 4096 | 4096 chars | Valid |
| BND-073 | Participant count zero | 0 | Valid |
| BND-074 | Participant count max | Max | Valid |
| BND-075 | Due date at min | MinValue | Handle |
| BND-076 | Due date at max | MaxValue | Handle |
| BND-077 | Priority enum first | First | Valid |
| BND-078 | Priority enum last | Last | Valid |
| BND-079 | Comment count zero | 0 | Valid |
| BND-080 | Comment count max | Max | Valid |
| BND-081 | Attachment size zero | 0 bytes | Reject |
| BND-082 | Attachment size max | Max | Valid |
| BND-083 | Budget zero | 0 | Valid |
| BND-084 | Budget max | Max | Valid |
| BND-085 | Owner null | No owner | Valid |
| BND-086 | Owner set | Owner set | Valid |
| BND-087 | Document count zero | 0 | Valid |
| BND-088 | Document count max | Max | Valid |
| BND-089 | Clone preserves audit | Clone | Audit |
| BND-090 | Archive boundary | Archive | Archived |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Submit requires Draft | Workflow | Submit | Only from Draft |
| FUN-002 | Approve requires Submitted | Workflow | Approve | Only when submitted |
| FUN-003 | Reject requires Submitted | Workflow | Reject | Only when submitted |
| FUN-004 | Cancel requires Draft | Workflow | Cancel | Only from Draft |
| FUN-005 | Withdraw requires InWorkflow | Workflow | Withdraw | Only when in workflow |
| FUN-006 | Reopen from Cancelled | Workflow | Reopen | Returns to Draft |
| FUN-007 | Reopen from Rejected | Workflow | Reopen | Returns to Draft |
| FUN-008 | Status history ordered | Audit | GetHistory | Chronological |
| FUN-009 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-010 | Name required | Validation | Create | Reject if empty |
| FUN-011 | Partner required | Validation | Create | Reject if invalid |
| FUN-012 | Status transitions valid | Constraint | ChangeStatus | Only valid transitions |
| FUN-013 | DoA required for submit | Validation | Submit | Reject if no DoA |
| FUN-014 | Audit CreatedBy | Audit | Create | Set current user |
| FUN-015 | Audit CreatedDate | Audit | Create | Set UTC now |
| FUN-016 | Audit LastModifiedBy | Audit | Update | Set current user |
| FUN-017 | Audit LastModifiedDate | Audit | Update | Set UTC now |
| FUN-018 | Soft delete DeletedBy | Audit | Delete | Set current user |
| FUN-019 | Soft delete DeletedDate | Audit | Delete | Set UTC now |
| FUN-020 | Permission check before action | Authorization | Any action | Check first |
| FUN-021 | OM can edit own | Authorization | Edit | OM can edit |
| FUN-022 | DoA can approve | Authorization | Approve | DoA can approve |
| FUN-023 | Child inherits base validation | Inheritance | Child create | Base validation runs |
| FUN-024 | Child can override | Inheritance | Child method | Override used |
| FUN-025 | WorkflowStatus synced | Constraint | Status change | WorkflowStatus updated |
| FUN-026 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-027 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-028 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-029 | Participant unique | Constraint | Add | No duplicates |
| FUN-030 | Link unique per entity | Constraint | Link | No duplicate links |
| FUN-031 | Pagination offset correct | Calculation | List page 2 | Skip correct |
| FUN-032 | Total count accurate | Calculation | Count | Matches filtered |
| FUN-033 | Sort applies to result | Calculation | List sort | Ordered |
| FUN-034 | Filter AND logic | Calculation | Multi-filter | All match |
| FUN-035 | Include loads related | Data load | GetById include | Related loaded |
| FUN-036 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-037 | Async all operations | Concurrency | All methods | Async |
| FUN-038 | Transaction on create | Transaction | Create | Atomic |
| FUN-039 | Transaction on update | Transaction | Update | Atomic |
| FUN-040 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-041 | Reopen clears workflow | Workflow | Reopen | Resets state |
| FUN-042 | Submit locks editing | Workflow | Submit | Read-only |
| FUN-043 | Approve closes | Workflow | Approve | Closed |
| FUN-044 | Reject closes | Workflow | Reject | Closed |
| FUN-045 | Cancel closes | Workflow | Cancel | Closed |
| FUN-046 | Name uniqueness optional | Constraint | Create | Per config |
| FUN-047 | Type validation | Validation | Create | Type in allowed |
| FUN-048 | Date range validation | Validation | Date filter | Start≤End |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | AddAttachment creates | Data | AddAttachment | Created |
| FUN-052 | RemoveAttachment deletes | Data | RemoveAttachment | Removed |
| FUN-053 | GetAttachments ordered | Logic | GetAttachments | Ordered |
| FUN-054 | SetDueDate updates | Data | SetDueDate | Updated |
| FUN-055 | GetDueEngagements filters | Filter | GetDueEngagements | Filtered |
| FUN-056 | AssignOwner sets | Data | AssignOwner | Set |
| FUN-057 | UnassignOwner clears | Data | UnassignOwner | Cleared |
| FUN-058 | AddComment creates | Data | AddComment | Created |
| FUN-059 | GetComments ordered | Logic | GetComments | Chronological |
| FUN-060 | SetPriority updates | Data | SetPriority | Updated |
| FUN-061 | GetByPriority filters | Filter | GetByPriority | Filtered |
| FUN-062 | LinkDocument creates | Data | LinkDocument | Linked |
| FUN-063 | UnlinkDocument removes | Data | UnlinkDocument | Removed |
| FUN-064 | GetLinkedDocuments returns | Data | GetLinkedDocuments | Documents |
| FUN-065 | CloneEngagement copies | Data | CloneEngagement | Copied |
| FUN-066 | ArchiveEngagement archives | Logic | ArchiveEngagement | Archived |
| FUN-067 | RestoreEngagement restores | Logic | RestoreEngagement | Restored |
| FUN-068 | ValidateBudget validates | Validation | ValidateBudget | Valid |
| FUN-069 | GetBudgetSummary aggregates | Calculation | GetBudgetSummary | Correct |
| FUN-070 | Participant role validation | Validation | AddParticipants | Valid role |
| FUN-071 | Attachment size limit | Constraint | AddAttachment | Limit |
| FUN-072 | Due date in future | Validation | SetDueDate | Future |
| FUN-073 | Owner must exist | Constraint | AssignOwner | Reject invalid |
| FUN-074 | Comment max length | Constraint | AddComment | Max |
| FUN-075 | Priority in range | Validation | SetPriority | In range |
| FUN-076 | Document must exist | Constraint | LinkDocument | Reject invalid |
| FUN-077 | Clone excludes audit | Logic | Clone | Excludes |
| FUN-078 | Archive excludes deleted | Constraint | Archive | Reject |
| FUN-079 | Restore requires archived | Constraint | Restore | Reject |
| FUN-080 | Budget positive | Validation | ValidateBudget | Positive |
| FUN-081 | GetDueEngagements date range | Logic | GetDueEngagements | In range |
| FUN-082 | GetByPriority excludes deleted | Constraint | GetByPriority | Excludes |
| FUN-083 | GetComments excludes deleted | Constraint | GetComments | Excludes |
| FUN-084 | GetAttachments excludes deleted | Constraint | GetAttachments | Excludes |
| FUN-085 | GetLinkedDocuments excludes deleted | Constraint | GetLinkedDocuments | Excludes |
| FUN-086 | Export excludes deleted | Constraint | Export | Excludes |
| FUN-087 | Import rollback | Transaction | Import | Rollback |
| FUN-088 | Export format | Format | Export | Correct |
| FUN-089 | Search case handling | Config | Search | Per config |
| FUN-090 | Filter by priority | Filter | List | Priority filter |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create engagement full flow | Create | Engagement, Partner | Created and linked |
| INT-002 | Update engagement full flow | Update | Engagement | Updated |
| INT-003 | Delete engagement full flow | Delete | Engagement | Soft deleted |
| INT-004 | Get engagement with partner | GetById | Engagement, Partner | Partner loaded |
| INT-005 | List with filter and sort | List | Engagement | Filtered, sorted |
| INT-006 | Submit triggers notification | Submit | Engagement, User | Notification sent |
| INT-007 | Approve triggers notification | Approve | Engagement, User | Notification sent |
| INT-008 | Reject triggers notification | Reject | Engagement, User | Notification sent |
| INT-009 | Link to partner | Link | Engagement, Partner | Association created |
| INT-010 | Add participants | AddParticipants | Engagement, User | Participants added |
| INT-011 | Search by name | Search | Engagement | Matching results |
| INT-012 | Search by partner | Search | Engagement, Partner | Partner engagements |
| INT-013 | Search by status | Search | Engagement | By status |
| INT-014 | Search by date range | Search | Engagement | In range |
| INT-015 | Search combined | Search | Engagement | All filters |
| INT-016 | Pagination page 1 | Paginate | Engagement | First page |
| INT-017 | Pagination page 2 | Paginate | Engagement | Second page |
| INT-018 | Pagination last page | Paginate | Engagement | Last page |
| INT-019 | Pagination empty | Paginate | Engagement | Empty list |
| INT-020 | Pagination total | Paginate | Engagement | Total correct |
| INT-021 | Engagement-Partner relationship | Relationship | Engagement, Partner | FK valid |
| INT-022 | Engagement-User relationship | Relationship | Engagement, User | Participants |
| INT-023 | Engagement-Org relationship | Relationship | Engagement, Org | Org link |
| INT-024 | Cascade soft delete | Relationship | Engagement, Partner | Configurable |
| INT-025 | Orphan handling | Relationship | Partner deleted | Engagement retained |
| INT-026 | DB error handling | Error | DB down | Graceful error |
| INT-027 | Timeout handling | Error | Slow DB | Timeout |
| INT-028 | Constraint violation | Error | FK violation | Clear error |
| INT-029 | Unique violation | Error | Duplicate | Clear error |
| INT-030 | Null reference handling | Error | Null nav | Handled |
| INT-031 | Permission service integration | Integration | Permission check | Correct result |
| INT-032 | User resolver integration | Integration | Current user | Resolved |
| INT-033 | Notification service integration | Integration | Send notification | Sent |
| INT-034 | Audit context integration | Integration | Audit | Context set |
| INT-035 | Child manager integration | Integration | Child create | Base+child |
| INT-036 | Workflow service integration | Integration | Workflow | Status sync |
| INT-037 | Cache integration | Integration | Cache | Hit/miss |
| INT-038 | Logger integration | Integration | Log | Logged |
| INT-039 | Config integration | Integration | Config | Read |
| INT-040 | Mapper integration | Integration | Map | Correct mapping |
| INT-041 | Repository integration | Integration | Repository | CRUD works |
| INT-042 | DbContext integration | Integration | DbContext | Scoped |
| INT-043 | Transaction scope | Integration | Transaction | Atomic |
| INT-044 | Multiple engagements same partner | Scenario | Engagement, Partner | All linked |
| INT-045 | Engagement status history | Scenario | Multiple actions | Full history |
| INT-046 | Concurrent list and get | Scenario | Parallel | No conflict |
| INT-047 | Bulk create | Scenario | Multiple | All created |
| INT-048 | Export with related data | Scenario | Engagement, Partner | Export includes |
| INT-049 | Import validation | Scenario | Import data | Validated |
| INT-050 | E2E workflow cycle | Scenario | Full cycle | Submit→Approve |
| INT-051 | AddAttachment flow | Scenario | AddAttachment | Added |
| INT-052 | RemoveAttachment flow | Scenario | RemoveAttachment | Removed |
| INT-053 | SetDueDate flow | Scenario | SetDueDate | Set |
| INT-054 | GetDueEngagements flow | Scenario | GetDueEngagements | Filtered |
| INT-055 | AssignOwner flow | Scenario | AssignOwner | Assigned |
| INT-056 | UnassignOwner flow | Scenario | UnassignOwner | Unassigned |
| INT-057 | AddComment flow | Scenario | AddComment | Added |
| INT-058 | SetPriority flow | Scenario | SetPriority | Set |
| INT-059 | LinkDocument flow | Scenario | LinkDocument | Linked |
| INT-060 | UnlinkDocument flow | Scenario | UnlinkDocument | Unlinked |
| INT-061 | CloneEngagement flow | Scenario | CloneEngagement | Cloned |
| INT-062 | ArchiveEngagement flow | Scenario | ArchiveEngagement | Archived |
| INT-063 | RestoreEngagement flow | Scenario | RestoreEngagement | Restored |
| INT-064 | GetBudgetSummary flow | Scenario | GetBudgetSummary | Summary |
| INT-065 | Document manager integration | Integration | DocumentManager | Documents |
| INT-066 | Attachment service integration | Integration | Attachment | Attachments |
| INT-067 | Comment service integration | Integration | Comment | Comments |
| INT-068 | Multiple entity types | Scenario | Engagement | Multiple |
| INT-069 | Full workflow cycle | Scenario | Full | Complete |
| INT-070 | Pagination with filter | Scenario | Paginate | Filtered |
| INT-071 | Sort with filter | Scenario | List | Sorted, filtered |
| INT-072 | Search full | Scenario | Search | Results |
| INT-073 | Bulk operations | Scenario | Bulk | All |
| INT-074 | Concurrent operations | Scenario | Parallel | No conflict |
| INT-075 | Error recovery | Scenario | Error | Recover |
| INT-076 | Audit trail full | Scenario | CRUD | Full trail |
| INT-077 | Permission integration | Scenario | Permission | Enforced |
| INT-078 | User context integration | Scenario | User | Context |
| INT-079 | Logger integration flow | Scenario | Log | Logged |
| INT-080 | Mapper round-trip | Scenario | Map | Correct |
| INT-081 | Repository CRUD cycle | Scenario | Repository | CRUD |
| INT-082 | DbContext scoping | Scenario | DbContext | Scoped |
| INT-083 | Transaction rollback | Scenario | Transaction | Rollback |
| INT-084 | Notification flow | Scenario | Notification | Sent |
| INT-085 | Workflow state sync | Scenario | Workflow | Synced |
| INT-086 | Child override flow | Scenario | Child | Override |
| INT-087 | Inheritance chain | Scenario | Child | Chain |
| INT-088 | Attachment full flow | Scenario | Attachment | Complete |
| INT-089 | Comment full flow | Scenario | Comment | Complete |
| INT-090 | E2E with all features | Scenario | Full | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in name | '; DROP TABLE-- | Name | Sanitized/Rejected |
| SEC-002 | SQL injection in filter | 1; DELETE FROM | Filter | Rejected |
| SEC-003 | SQL injection in sort | id; DROP | Sort | Rejected |
| SEC-004 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-006 | XSS in remarks | javascript:alert(1) | Remarks | Sanitized |
| SEC-007 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-008 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-009 | Command injection | ; ls -la | Any | Rejected |
| SEC-010 | Path traversal | ../../../etc/passwd | File ref | Rejected |
| SEC-011 | Unauthorized list | No permission | List | 403 |
| SEC-012 | Unauthorized get | No permission | GetById | 403 |
| SEC-013 | Unauthorized create | No permission | Create | 403 |
| SEC-014 | Unauthorized update | No permission | Update | 403 |
| SEC-015 | Unauthorized delete | No permission | Delete | 403 |
| SEC-016 | Unauthorized submit | No permission | Submit | 403 |
| SEC-017 | Unauthorized approve | No permission | Approve | 403 |
| SEC-018 | Unauthorized export | No permission | Export | 403 |
| SEC-019 | Role escalation | Low role | Admin action | 403 |
| SEC-020 | Cross-tenant access | User A | User B data | 403 |
| SEC-021 | IDOR get other engagement | Id=other | GetById | 403/404 |
| SEC-022 | IDOR update other | Id=other | Update | 403 |
| SEC-023 | IDOR delete other | Id=other | Delete | 403 |
| SEC-024 | IDOR approve other | Id=other | Approve | 403 |
| SEC-025 | IDOR in filter | PartnerId=other | List | Filtered |
| SEC-026 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-027 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-028 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-029 | Mass assign DeletedBy | DeletedBy=null | Request | Ignored |
| SEC-030 | Mass assign WorkflowStatus | WorkflowStatus | Request | Ignored |
| SEC-031 | Session hijack | Stolen token | Any | Detected |
| SEC-032 | Token expiration | Expired token | Any | 401 |
| SEC-033 | Invalid token | Malformed token | Any | 401 |
| SEC-034 | CSRF on create | No CSRF token | Create | Rejected |
| SEC-035 | CSRF on update | No CSRF token | Update | Rejected |
| SEC-036 | Replay attack | Old request | Resubmit | Rejected |
| SEC-037 | Sensitive data in log | Log request | Log output | PII redacted |
| SEC-038 | Sensitive data in error | Error message | Stack trace | Sanitized |
| SEC-039 | Password in request | Password field | Log/response | Never logged |
| SEC-040 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-041 | Rate limit create | Many creates | Create | Throttled |
| SEC-042 | Rate limit list | Many lists | List | Throttled |
| SEC-043 | Rate limit search | Many searches | Search | Throttled |
| SEC-044 | Oversized request | 10MB payload | Create | Rejected |
| SEC-045 | Deep nesting | Nested object | Request | Rejected |
| SEC-046 | Header injection | \r\n in header | Header | Rejected |
| SEC-047 | Null byte injection | %00 in string | Name | Rejected |
| SEC-048 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-049 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-050 | Denial of service | Huge page size | List | Capped |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | User A, B update | Optimistic lock or last-write |
| CON-002 | Update and delete same | One updates, one deletes | Deterministic |
| CON-003 | Double submit | Same user submit twice | One succeeds |
| CON-004 | Concurrent approve/reject | DoA A approve, B reject | One wins |
| CON-005 | Concurrent create same name | Two create same | One fails or both |
| CON-006 | Read during write | Read while update | Consistent read |
| CON-007 | Transaction isolation | Parallel transactions | Serializable |
| CON-008 | Cache invalidation | Update invalidates cache | Fresh read |
| CON-009 | Cache poisoning | Malicious cache | Not used |
| CON-010 | Stale entity update | Old version | Concurrency handled |
| CON-011 | Race on status change | Two status changes | One wins |
| CON-012 | Race on participant add | Two add same | One or both |
| CON-013 | Deadlock scenario | Circular lock | Timeout or avoid |
| CON-014 | Lock escalation | Many locks | No escalation |
| CON-015 | Connection pool exhaustion | Many concurrent | Pool limit |
| CON-016 | DbContext concurrency | Share context | Not shared |
| CON-017 | Async parallel creates | 10 parallel create | All succeed |
| CON-018 | Async parallel reads | 10 parallel read | All succeed |
| CON-019 | Batch vs single | Batch vs loop | Same result |
| CON-020 | Pagination concurrent | Two paginate | Both correct |
| CON-021 | Filter concurrent update | Filter while update | Consistent |
| CON-022 | Soft delete concurrent | Delete while update | Deterministic |
| CON-023 | Workflow concurrent | Submit while withdraw | One wins |
| CON-024 | Audit concurrent | Two updates | Both audited |
| CON-025 | Idempotency | Same request twice | Same result |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate name not empty | Validation | "" | Exception |
| UNT-003 | Validate partner ID positive | Validation | -1 | Exception |
| UNT-004 | Validate status valid | Validation | Valid status | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format date UTC | Formatting | DateTime | UTC string |
| UNT-007 | Format status display | Formatting | Status | Display string |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Status transition matrix | Calculation | From, To | Valid/Invalid |
| UNT-013 | Permission combination | Calculation | Roles | Permissions |
| UNT-014 | Status allows submit | Status logic | Draft | true |
| UNT-015 | Status allows approve | Status logic | Submitted | true |
| UNT-016 | Status allows reject | Status logic | Submitted | true |
| UNT-017 | Status allows cancel | Status logic | Draft | true |
| UNT-018 | Status allows reopen | Status logic | Closed | true |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty handling | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Bulk create 100 | Create 100 | <5s | P0 |
| PRF-004 | Bulk create 1000 | Create 1000 | <30s | P0 |
| PRF-005 | Bulk update 100 | Update 100 | <5s | P1 |
| PRF-006 | Search by name | Search | <500ms | P1 |
| PRF-007 | Search with filter | Search | <500ms | P1 |
| PRF-008 | List with pagination | List | <300ms | P1 |
| PRF-009 | List with sort | List | <300ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 writes | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 write | <4s total | P2 |
| PRF-013 | Memory single create | Create | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory bulk 100 | Bulk create | <20MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query per | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 10 RPS create | 10 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 50 RPS | 0→50→0 | 1 min | No errors |
| LDT-005 | Spike 100 RPS | 0→100→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to failure | Until fail | Document limit |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large bulk | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return to normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | System recovers |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
