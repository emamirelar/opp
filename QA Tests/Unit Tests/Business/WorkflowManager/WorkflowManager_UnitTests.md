# WorkflowManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/WorkflowManager` (Unit Tests)  
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

**Ratio Compliance:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

Workflow manager unit tests cover stage transitions, status management, history tracking, permissions, and notifications. Tests include: workflow path generation, status transitions, state machine navigation, internal/external facing states, workflow logging, action permissions, and transition notifications.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Get workflow path | State machine exists | GetWorkflowPath | Path returned |
| POS-002 | Get current stage | Entity has stage | GetCurrentStage | Stage |
| POS-003 | Transition to next | Valid transition | Transition | Transitioned |
| POS-004 | Get available actions | Entity in stage | GetAvailableActions | Actions |
| POS-005 | Get transition history | History exists | GetHistory | History |
| POS-006 | Add history entry | Valid data | AddHistory | Added |
| POS-007 | Get internal states | Internal facing | GetInternalStates | States |
| POS-008 | Get external states | External facing | GetExternalStates | States |
| POS-009 | Validate transition | Valid transition | ValidateTransition | Valid |
| POS-010 | Get next stage | Current stage | GetNextStage | Next |
| POS-011 | Get previous stage | Current stage | GetPreviousStage | Previous |
| POS-012 | Audit CreatedBy | AddHistory | Check audit | Set |
| POS-013 | Audit CreatedDate | AddHistory | Check audit | UTC |
| POS-014 | Can transition | User has permission | CanTransition | Boolean |
| POS-015 | Get stage by code | Code exists | GetStageByCode | Stage |
| POS-016 | Get actions for stage | Stage exists | GetActionsForStage | Actions |
| POS-017 | Get workflow by entity | Entity exists | GetWorkflowByEntity | Workflow |
| POS-018 | Get all stages | Workflow exists | GetAllStages | Stages |
| POS-019 | Get stage sequence | Stage exists | GetSequence | Sequence |
| POS-020 | Get facing | Stage exists | GetFacing | Facing |
| POS-021 | Send transition notification | Transition | SendNotification | Sent |
| POS-022 | Check permission | Action required | CheckPermission | Boolean |
| POS-023 | Get state machine | Entity type exists | GetStateMachine | Machine |
| POS-024 | Get initial stage | Workflow exists | GetInitialStage | Stage |
| POS-025 | Get final stages | Workflow exists | GetFinalStages | Stages |
| POS-026 | Can revert | Transition allows | CanRevert | Boolean |
| POS-027 | Revert transition | Valid revert | Revert | Reverted |
| POS-028 | Get pending actions | Entity pending | GetPendingActions | Actions |
| POS-029 | Batch transition | Multiple entities | BatchTransition | All |
| POS-030 | Get workflow status | Entity exists | GetWorkflowStatus | Status |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Get path null state machine | Machine=null | ArgumentNullException |
| NEG-002 | Transition null entity | Entity=null | ArgumentNullException |
| NEG-003 | Transition invalid stage | Stage invalid | BusinessException |
| NEG-004 | Transition without permission | Unauthorized | Forbidden |
| NEG-005 | Get current stage null entity | Entity=null | ArgumentNullException |
| NEG-006 | Add history null entity | Entity=null | ArgumentNullException |
| NEG-007 | Validate transition invalid | Transition invalid | BusinessException |
| NEG-008 | Get next stage final | Final stage | Null or BusinessException |
| NEG-009 | Get previous stage initial | Initial stage | Null or BusinessException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | Transition without permission | Unauthorized | Forbidden |
| NEG-012 | Revert without permission | Unauthorized | Forbidden |
| NEG-013 | Get history unauthorized | Unauthorized | Forbidden |
| NEG-014 | Update workflow unauthorized | Unauthorized | Forbidden |
| NEG-015 | Batch transition unauthorized | Unauthorized | Forbidden |
| NEG-016 | SQL injection in filter | '; DROP | Rejected |
| NEG-017 | XSS in comment | <script> | Escaped |
| NEG-018 | Path traversal | ../../../etc | Rejected |
| NEG-019 | Invalid entity type | Type invalid | ArgumentException |
| NEG-020 | Invalid entity ID | EntityId=-1 | ArgumentException |
| NEG-021 | Invalid stage code | Code invalid | ArgumentException |
| NEG-022 | Invalid action | Action invalid | ArgumentException |
| NEG-023 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-024 | Concurrent transition conflict | Stale entity | ConcurrencyException |
| NEG-025 | Connection timeout | DB unavailable | TimeoutException |
| NEG-026 | Transition to same stage | Same stage | BusinessException |
| NEG-027 | Transition to non-adjacent | Skip stage | BusinessException |
| NEG-028 | Revert invalid | Revert invalid | BusinessException |
| NEG-029 | Expired session | Expired token | Unauthorized |
| NEG-030 | Null user context | User=null | InvalidOperationException |
| NEG-031 | GetStageByCode non-existent | Code invalid | KeyNotFoundException |
| NEG-032 | GetWorkflowByEntity non-existent | Entity invalid | KeyNotFoundException |
| NEG-033 | Invalid page number | Page=0 | ArgumentException |
| NEG-034 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-035 | Filter malformed | Malformed filter | ArgumentException |
| NEG-036 | Child override throws | Child throws | Propagated |
| NEG-037 | State machine not found | Type invalid | KeyNotFoundException |
| NEG-038 | Circular transition | Circular | BusinessException |
| NEG-039 | Audit missing user | User=0 | InvalidOperationException |
| NEG-040 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-041 | Pagination overflow | Page too large | Empty or error |
| NEG-042 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-043 | Batch transition null list | List=null | ArgumentNullException |
| NEG-044 | Batch transition empty | List=[] | ArgumentException |
| NEG-045 | Batch transition one invalid | One invalid | Partial or fail |
| NEG-046 | AddHistory invalid data | Data invalid | ValidationException |
| NEG-047 | GetHistory null entity | Entity=null | ArgumentNullException |
| NEG-048 | GetActionsForStage invalid | Stage invalid | ArgumentException |
| NEG-049 | Cross-tenant access | Other tenant | Forbidden |
| NEG-050 | Invalid include path | Invalid include | ArgumentException |
| NEG-051 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-052 | Invalid enum value | Facing invalid | ArgumentException |
| NEG-053 | Transition blocked | Blocked | BusinessException |
| NEG-054 | Requirements not met | Missing req | BusinessException |
| NEG-055 | Notification failure | Send fails | Handle |
| NEG-056 | GetInitialStage empty | No stages | BusinessException |
| NEG-057 | GetFinalStages empty | No final | Empty |
| NEG-058 | CanRevert invalid | Invalid | False |
| NEG-059 | Revert no history | No history | BusinessException |
| NEG-060 | GetPendingActions invalid | Entity invalid | ArgumentException |
| NEG-061 | GetConfig invalid type | Type invalid | ArgumentException |
| NEG-062 | GetDisplayName invalid | Stage invalid | ArgumentException |
| NEG-063 | GetRequirements invalid | Transition invalid | ArgumentException |
| NEG-064 | ValidateState invalid | Entity invalid | InvalidOperationException |
| NEG-065 | GetAllowedTransitions final | Final stage | Empty |
| NEG-066 | GetWorkflowStatus deleted | Entity deleted | KeyNotFoundException |
| NEG-067 | Transition deleted entity | Entity deleted | KeyNotFoundException |
| NEG-068 | AddHistory deleted entity | Entity deleted | KeyNotFoundException |
| NEG-069 | State machine corrupted | Corrupt config | InvalidOperationException |
| NEG-070 | Duplicate stage code | Code exists | BusinessException |
| NEG-071 | GetWorkflowPath null type | Type=null | ArgumentNullException |
| NEG-072 | Transition null target stage | TargetStage=null | ArgumentNullException |
| NEG-073 | AddHistory null comment | Comment=null | ArgumentNullException |
| NEG-074 | GetStageByCode empty code | Code="" | ArgumentException |
| NEG-075 | GetWorkflowByEntity zero ID | EntityId=0 | ArgumentException |
| NEG-076 | BatchTransition null entity | Entity=null | ArgumentNullException |
| NEG-077 | Revert null entity | Entity=null | ArgumentNullException |
| NEG-078 | GetHistory zero entity | EntityId=0 | ArgumentException |
| NEG-079 | GetAvailableActions null entity | Entity=null | ArgumentNullException |
| NEG-080 | GetCurrentStage null entity | Entity=null | ArgumentNullException |
| NEG-081 | GetNextStage null entity | Entity=null | ArgumentNullException |
| NEG-082 | GetPreviousStage null entity | Entity=null | ArgumentNullException |
| NEG-083 | GetStateMachine null type | Type=null | ArgumentNullException |
| NEG-084 | GetConfig null type | Type=null | ArgumentNullException |
| NEG-085 | GetDisplayName null stage | Stage=null | ArgumentNullException |
| NEG-086 | GetRequirements null transition | Transition=null | ArgumentNullException |
| NEG-087 | ValidateTransition null args | Args=null | ArgumentNullException |
| NEG-088 | CanTransition null entity | Entity=null | ArgumentNullException |
| NEG-089 | CanRevert null entity | Entity=null | ArgumentNullException |
| NEG-090 | GetWorkflowStatus null entity | Entity=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Workflow path single stage | One stage | Valid |
| BND-002 | Workflow path max stages | 100 stages | Valid |
| BND-003 | Stage code at min | Length=1 | Valid |
| BND-004 | Stage code at max | Length=50 | Valid |
| BND-005 | Stage code over max | Length=51 | Reject |
| BND-006 | Entity ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-007 | Entity ID at zero | Id=0 | Reject |
| BND-008 | Page size at min | PageSize=1 | Valid |
| BND-009 | Page size at max | PageSize=1000 | Valid |
| BND-010 | Page size over max | PageSize=1001 | Reject |
| BND-011 | Sequence at 0 | Sequence=0 | Initial |
| BND-012 | Sequence at max | Sequence=max | Final |
| BND-013 | History count zero | No history | [] |
| BND-014 | History count max | 10000 entries | Valid |
| BND-015 | Unicode in comment | Arabic/Chinese | Stored |
| BND-016 | Special chars in comment | <>&"' | Escaped |
| BND-017 | Leading/trailing spaces | Comment="  x  " | Trimmed |
| BND-018 | Empty workflow | No stages | Empty |
| BND-019 | Single transition | One transition | Valid |
| BND-020 | Date at min | Date=MinValue | Handle |
| BND-021 | Date at max | Date=MaxValue | Handle |
| BND-022 | DateTime UTC | UTC input | Stored |
| BND-023 | Facing enum boundary | Last enum | Valid |
| BND-024 | Action enum boundary | Last enum | Valid |
| BND-025 | Pagination last partial | Partial page | Correct |
| BND-026 | Pagination total | Total count | Accurate |
| BND-027 | Sort null handling | Nulls in data | Deterministic |
| BND-028 | Filter combination all | All filters | Correct |
| BND-029 | Transition first to second | First | Valid |
| BND-030 | Transition last to previous | Last | Valid |
| BND-031 | Revert one step | One step | Valid |
| BND-032 | Revert multiple | Multiple | Valid |
| BND-033 | Soft delete boundary | DeletedDate set | Excluded |
| BND-034 | Include depth | Deep include | No explosion |
| BND-035 | Query timeout | Slow query | Timeout |
| BND-036 | Audit timestamp precision | Millisecond | Stored |
| BND-037 | Async cancellation | Cancel token | OperationCanceledException |
| BND-038 | Task timeout | Timeout | TimeoutException |
| BND-039 | Concurrent same second | Same timestamp | Deterministic |
| BND-040 | Two-face workflow | TwoFace | Both |
| BND-041 | Internal only workflow | Internal | Internal |
| BND-042 | External only workflow | External | External |
| BND-043 | Mixed facing workflow | Mixed | Both |
| BND-044 | GetNextStage boundary | Last stage | Null |
| BND-045 | GetPreviousStage boundary | First stage | Null |
| BND-046 | GetAvailableActions empty | No actions | [] |
| BND-047 | GetAllowedTransitions empty | Final | [] |
| BND-048 | GetPendingActions empty | No pending | [] |
| BND-049 | Batch transition max | 100 entities | Valid |
| BND-050 | Batch transition over max | 101 entities | Reject |
| BND-051 | Filter empty result | No match | Empty list |
| BND-052 | Sort empty | Empty list | No exception |
| BND-053 | Pagination empty | No data | Empty |
| BND-054 | GetHistory empty | No history | [] |
| BND-055 | GetInitialStage | First | Valid |
| BND-056 | GetFinalStages | Last | Valid |
| BND-057 | CanTransition true | Allowed | True |
| BND-058 | CanTransition false | Not allowed | False |
| BND-059 | CanRevert true | Allowed | True |
| BND-060 | CanRevert false | Not allowed | False |
| BND-061 | Notification batch | Many | Sent |
| BND-062 | Permission check | Check | Boolean |
| BND-063 | ValidateTransition valid | Valid | True |
| BND-064 | ValidateTransition invalid | Invalid | False |
| BND-065 | GetStageByCode exact | Exact | Found |
| BND-066 | GetWorkflowByEntity | Entity | Workflow |
| BND-067 | GetStateMachine | Type | Machine |
| BND-068 | GetWorkflowConfig | Type | Config |
| BND-069 | GetDisplayName | Stage | Name |
| BND-070 | Concurrent transition | Two transition | One wins |
| BND-071 | Stage sequence first | First | Valid |
| BND-072 | Stage sequence last | Last | Valid |
| BND-073 | History count one | 1 entry | Valid |
| BND-074 | Comment length max | Max length | Valid |
| BND-075 | Comment length over | Over max | Reject |
| BND-076 | Workflow path two stages | Two | Valid |
| BND-077 | Transition chain | Multiple | Valid |
| BND-078 | Revert chain | Multiple | Valid |
| BND-079 | Batch transition one | 1 entity | Valid |
| BND-080 | GetWorkflowPath empty | No path | Empty |
| BND-081 | GetNextStage null | No next | Null |
| BND-082 | GetPreviousStage null | No prev | Null |
| BND-083 | GetActionsForStage one | 1 action | Valid |
| BND-084 | GetAllowedTransitions one | 1 transition | Valid |
| BND-085 | GetPendingActions one | 1 pending | Valid |
| BND-086 | GetRequirements empty | No req | Empty |
| BND-087 | GetRequirements many | Many | All |
| BND-088 | ValidateState valid | Valid | True |
| BND-089 | GetWorkflowStatus boundary | Status | Valid |
| BND-090 | Entity type boundary | Type | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Entity required | Validation | GetCurrentStage | Reject if null |
| FUN-002 | Stage required | Validation | Transition | Reject if null |
| FUN-003 | Valid transition only | Constraint | Transition | Reject invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Permission required | Constraint | Transition | Reject unauthorized |
| FUN-008 | Sequential transition | Constraint | Transition | Reject skip |
| FUN-009 | Audit CreatedBy | Audit | AddHistory | Set user |
| FUN-010 | Audit CreatedDate | Audit | AddHistory | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Transition requirements | Constraint | Transition | Reject unmet |
| FUN-017 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-018 | GetHistory excludes deleted | Constraint | GetHistory | Excludes deleted |
| FUN-019 | Facing filter | Logic | GetPath | Filtered |
| FUN-020 | Sequence order | Logic | GetWorkflowPath | Ordered |
| FUN-021 | Available actions | Logic | GetAvailableActions | By stage |
| FUN-022 | Allowed transitions | Logic | GetAllowedTransitions | By stage |
| FUN-023 | History chronological | Logic | GetHistory | Ordered |
| FUN-024 | Notification on transition | Logic | Transition | Sent |
| FUN-025 | Revert adds history | Logic | Revert | History added |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on transition | Transaction | Transition | Atomic |
| FUN-031 | Transaction on revert | Transaction | Revert | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads entity | Data load | GetById include | Entity loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | State machine config | Logic | GetStateMachine | Config |
| FUN-036 | Initial stage | Logic | GetInitialStage | First |
| FUN-037 | Final stages | Logic | GetFinalStages | Last |
| FUN-038 | Display name | Logic | GetDisplayName | Localized |
| FUN-039 | Requirements check | Logic | GetRequirements | Check |
| FUN-040 | Validate before transition | Logic | Transition | Validated |
| FUN-041 | Batch atomic | Logic | BatchTransition | Atomic |
| FUN-042 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-043 | Workflow status | Logic | GetWorkflowStatus | Status |
| FUN-044 | Config stages | Config | GetConfig | Config |
| FUN-045 | Config permissions | Config | Transition | Config |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | Status workflow | Workflow | Transition | Valid flow |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Path caching | Performance | GetWorkflowPath | Cached |
| FUN-051 | GetNextStage logic | Logic | GetNextStage | Next |
| FUN-052 | GetPreviousStage logic | Logic | GetPreviousStage | Previous |
| FUN-053 | CanTransition logic | Logic | CanTransition | Boolean |
| FUN-054 | CanRevert logic | Logic | CanRevert | Boolean |
| FUN-055 | ValidateTransition logic | Logic | ValidateTransition | Validated |
| FUN-056 | GetStageByCode logic | Logic | GetStageByCode | Stage |
| FUN-057 | GetWorkflowByEntity logic | Logic | GetWorkflowByEntity | Workflow |
| FUN-058 | GetActionsForStage logic | Logic | GetActionsForStage | Actions |
| FUN-059 | GetPendingActions logic | Logic | GetPendingActions | Actions |
| FUN-060 | AddHistory logic | Logic | AddHistory | Added |
| FUN-061 | Revert logic | Logic | Revert | Reverted |
| FUN-062 | Batch transition logic | Logic | BatchTransition | All |
| FUN-063 | Notification logic | Logic | SendNotification | Sent |
| FUN-064 | CheckPermission logic | Logic | CheckPermission | Boolean |
| FUN-065 | GetSequence logic | Logic | GetSequence | Sequence |
| FUN-066 | GetFacing logic | Logic | GetFacing | Facing |
| FUN-067 | GetAllStages logic | Logic | GetAllStages | Stages |
| FUN-068 | GetInternalStates logic | Logic | GetInternalStates | States |
| FUN-069 | GetExternalStates logic | Logic | GetExternalStates | States |
| FUN-070 | Transition requirements | Constraint | Transition | Reject unmet |
| FUN-071 | Revert requirements | Constraint | Revert | Reject unmet |
| FUN-072 | Batch requirements | Constraint | BatchTransition | Reject unmet |
| FUN-073 | History order | Logic | GetHistory | Ordered |
| FUN-074 | Stage order | Logic | GetWorkflowPath | Ordered |
| FUN-075 | Action order | Logic | GetAvailableActions | Ordered |
| FUN-076 | Transition order | Logic | GetAllowedTransitions | Ordered |
| FUN-077 | Pagination consistency | Calculation | Page | Consistent |
| FUN-078 | Sort multi-column | Calculation | Sort | Multi |
| FUN-079 | Filter OR logic | Filter | OR filter | Match |
| FUN-080 | Transaction on add | Transaction | AddHistory | Atomic |
| FUN-081 | Transaction on batch | Transaction | BatchTransition | Atomic |
| FUN-082 | Include selective | Data load | Include | Selective |
| FUN-083 | Config workflow | Config | GetConfig | Config |
| FUN-084 | Config notification | Config | SendNotification | Config |
| FUN-085 | Permission per action | Authorization | Per action | Check |
| FUN-086 | User context audit | Audit | AddHistory | User |
| FUN-087 | Timestamp UTC | Audit | All | UTC |
| FUN-088 | Deleted exclude GetHistory | Constraint | GetHistory | Excluded |
| FUN-089 | Deleted exclude Transition | Constraint | Transition | Rejected |
| FUN-090 | Workflow lifecycle | Workflow | Full cycle | Complete |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Get path full flow | GetWorkflowPath | StateMachine | Path |
| INT-002 | Transition full flow | Transition | Entity | Transitioned |
| INT-003 | Get history full flow | GetHistory | Entity | History |
| INT-004 | Revert full flow | Revert | Entity | Reverted |
| INT-005 | Get actions full flow | GetAvailableActions | Entity | Actions |
| INT-006 | Get with entity | GetById | Workflow, Entity | Entity loaded |
| INT-007 | List with filter and sort | List | Workflow | Filtered, sorted |
| INT-008 | Add history | AddHistory | Entity | Added |
| INT-009 | Batch transition | BatchTransition | Entity | All |
| INT-010 | Workflow-Entity relationship | Relationship | Workflow, Entity | FK valid |
| INT-011 | History-Entity relationship | Relationship | History, Entity | Valid |
| INT-012 | Cascade soft delete | Relationship | Entity deleted | Config |
| INT-013 | Orphan handling | Relationship | Entity deleted | Retained |
| INT-014 | DB error handling | Error | DB down | Graceful |
| INT-015 | Timeout handling | Error | Slow | Timeout |
| INT-016 | Constraint violation | Error | FK violation | Clear error |
| INT-017 | Permission service integration | Integration | Permission | Check |
| INT-018 | User resolver integration | Integration | User | Resolved |
| INT-019 | Audit context integration | Integration | Audit | Context |
| INT-020 | Logger integration | Integration | Log | Logged |
| INT-021 | NotificationManager integration | Integration | Notification | Sent |
| INT-022 | Mapper integration | Integration | Map | Correct |
| INT-023 | Repository integration | Integration | Repository | CRUD |
| INT-024 | DbContext integration | Integration | DbContext | Scoped |
| INT-025 | Transaction scope | Integration | Transaction | Atomic |
| INT-026 | Full workflow cycle | Scenario | All transitions | Complete |
| INT-027 | Revert cycle | Scenario | Transition, Revert | Complete |
| INT-028 | Permission flow | Scenario | Check, Transition | Complete |
| INT-029 | Notification flow | Scenario | Transition | Sent |
| INT-030 | Concurrent transition | Scenario | Parallel | One wins |
| INT-031 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-032 | Filter by stage | Scenario | Filter | Filtered |
| INT-033 | Get internal external | Scenario | GetPath | Both |
| INT-034 | Get next previous | Scenario | GetNext, GetPrevious | Correct |
| INT-035 | Validate transition | Scenario | Validate | Valid |
| INT-036 | Get requirements | Scenario | GetRequirements | Requirements |
| INT-037 | Get config | Scenario | GetConfig | Config |
| INT-038 | Get display name | Scenario | GetDisplayName | Name |
| INT-039 | Get workflow status | Scenario | GetWorkflowStatus | Status |
| INT-040 | Get allowed transitions | Scenario | GetAllowedTransitions | Transitions |
| INT-041 | Get pending actions | Scenario | GetPendingActions | Actions |
| INT-042 | Can transition | Scenario | CanTransition | Boolean |
| INT-043 | Can revert | Scenario | CanRevert | Boolean |
| INT-044 | State machine | Scenario | GetStateMachine | Machine |
| INT-045 | Initial final stages | Scenario | GetInitial, GetFinal | Stages |
| INT-046 | Audit trail | Scenario | Operations | Trail |
| INT-047 | Batch transition | Scenario | BatchTransition | All |
| INT-048 | Workflow by entity type | Scenario | GetWorkflowByEntity | Workflow |
| INT-049 | Stage by code | Scenario | GetStageByCode | Stage |
| INT-050 | E2E transition-history-revert | Scenario | Full cycle | Complete |
| INT-051 | Transition then revert | Scenario | Transition, Revert | Complete |
| INT-052 | Add history then get | Scenario | Add, Get | Complete |
| INT-053 | Batch then get | Scenario | Batch, Get | Complete |
| INT-054 | Get path then transition | Scenario | Path, Transition | Complete |
| INT-055 | Validate then transition | Scenario | Validate, Transition | Complete |
| INT-056 | Get actions then transition | Scenario | Actions, Transition | Complete |
| INT-057 | Get next then transition | Scenario | Next, Transition | Complete |
| INT-058 | Get previous then revert | Scenario | Previous, Revert | Complete |
| INT-059 | Check permission then transition | Scenario | Check, Transition | Complete |
| INT-060 | Get config then transition | Scenario | Config, Transition | Complete |
| INT-061 | DbContext scope | Integration | Request | Scoped |
| INT-062 | Permission cascade | Integration | Role | Cascade |
| INT-063 | User context propagation | Integration | Request | Propagated |
| INT-064 | Audit chain | Integration | Operations | Chained |
| INT-065 | Notification chain | Integration | Transition | Sent |
| INT-066 | Error handling chain | Integration | Error | Handled |
| INT-067 | Validation chain | Integration | Transition | Validated |
| INT-068 | Mapping chain | Integration | Entity | Mapped |
| INT-069 | Repository CRUD | Integration | Repository | CRUD |
| INT-070 | DbContext save | Integration | SaveChanges | Saved |
| INT-071 | Transaction rollback | Integration | Error | Rollback |
| INT-072 | State machine flow | Integration | Config | Flow |
| INT-073 | Notification flow | Integration | Notification | Flow |
| INT-074 | Concurrent transition | Scenario | Parallel | One wins |
| INT-075 | Concurrent revert | Scenario | Parallel | One wins |
| INT-076 | Full transition chain | Scenario | All stages | Complete |
| INT-077 | Full revert chain | Scenario | All stages | Complete |
| INT-078 | Full history chain | Scenario | All entries | Complete |
| INT-079 | Full batch chain | Scenario | All entities | Complete |
| INT-080 | Full permission chain | Scenario | All checks | Complete |
| INT-081 | Full notification chain | Scenario | All transitions | Complete |
| INT-082 | Full config chain | Scenario | All config | Complete |
| INT-083 | Full state machine chain | Scenario | All states | Complete |
| INT-084 | Full workflow chain | Scenario | All workflows | Complete |
| INT-085 | Permission check flow | Integration | Auth | Check |
| INT-086 | User resolution flow | Integration | User | Resolved |
| INT-087 | Audit flow | Integration | Audit | Logged |
| INT-088 | Logging flow | Integration | Log | Logged |
| INT-089 | Notification flow | Integration | Notification | Sent |
| INT-090 | E2E full lifecycle | Scenario | All operations | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in filter | '; DROP TABLE-- | Filter | Sanitized |
| SEC-002 | SQL injection in comment | 1; DELETE | Comment | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in comment | <script>alert(1)</script> | Comment | Escaped |
| SEC-005 | XSS in display name | <img onerror=...> | Name | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized transition | No permission | Transition | 403 |
| SEC-012 | Unauthorized revert | No permission | Revert | 403 |
| SEC-013 | Unauthorized history | No permission | GetHistory | 403 |
| SEC-014 | Unauthorized batch | No permission | BatchTransition | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B entity | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR transition other | Id=other | Transition | 403 |
| SEC-019 | IDOR revert other | Id=other | Revert | 403 |
| SEC-020 | IDOR in filter | EntityId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign Stage | Stage=manipulated | Request | Validated |
| SEC-025 | Transition bypass | Invalid transition | Transition | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on transition | No token | Transition | Rejected |
| SEC-030 | CSRF on revert | No token | Revert | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | History tampering | Tamper history | Access | Detected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit transition | Many transitions | Transition | Throttled |
| SEC-036 | Rate limit get | Many gets | Get | Throttled |
| SEC-037 | Rate limit batch | Many batches | BatchTransition | Throttled |
| SEC-038 | Oversized request | 10MB payload | AddHistory | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in comment | Comment | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge batch | BatchTransition | Rejected |
| SEC-045 | Stage injection | Invalid stage | Transition | Rejected |
| SEC-046 | Action injection | Invalid action | Action | Rejected |
| SEC-047 | Config injection | Invalid config | GetConfig | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Workflow ACL | Direct access | Workflow | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users transition same | A, B transition | Optimistic lock |
| CON-002 | Transition and revert same | Transition, revert | Deterministic |
| CON-003 | Double add history | Two add | One or both |
| CON-004 | Concurrent transition | Two transition | One wins |
| CON-005 | Read during write | Read while transition | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on transition | Two transition | One wins |
| CON-009 | Race on revert | Two revert | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel gets | 10 parallel | All succeed |
| CON-012 | Async parallel transitions | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Batch transition concurrent | Two batch | Both succeed |
| CON-016 | Add history concurrent | Two add | Both succeed |
| CON-017 | Get actions concurrent | Two get | Both correct |
| CON-018 | Soft delete concurrent | Delete while transition | Deterministic |
| CON-019 | Transition concurrent | Two transition | One wins |
| CON-020 | Revert concurrent | Two revert | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Stage lock | Concurrent transition | Serialized |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate entity not null | Validation | null | Exception |
| UNT-002 | Validate stage | Validation | Valid stage | Pass |
| UNT-003 | Validate transition | Validation | Valid transition | Pass |
| UNT-004 | Validate action | Validation | Valid action | Pass |
| UNT-005 | Validate state machine | Validation | Valid machine | Pass |
| UNT-006 | Format comment | Formatting | Comment | Formatted |
| UNT-007 | Format stage name | Formatting | Name | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Sequence order | Calculation | Stages | Ordered |
| UNT-013 | Next stage | Calculation | Current | Next |
| UNT-014 | Transition allows | Status logic | Transition | true |
| UNT-015 | Revert allows | Status logic | Revert | true |
| UNT-016 | Action allows | Status logic | Action | true |
| UNT-017 | Stage valid | Status logic | Stage | true |
| UNT-018 | Facing check | Status logic | Facing | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get path | GetWorkflowPath | <100ms | P1 |
| PRF-002 | Single transition | Transition | <200ms | P1 |
| PRF-003 | Get current stage | GetCurrentStage | <50ms | P1 |
| PRF-004 | Get history | GetHistory | <200ms | P0 |
| PRF-005 | Get available actions | GetAvailableActions | <100ms | P0 |
| PRF-006 | Get allowed transitions | GetAllowedTransitions | <100ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Add history | AddHistory | <100ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel | <2s total | P1 |
| PRF-011 | Concurrent 5 transitions | 5 parallel | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 transition | <5s total | P2 |
| PRF-013 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-014 | Memory history 10k | GetHistory | <100MB | P2 |
| PRF-015 | Memory batch | BatchTransition | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS transition | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS transition | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS get | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress batch | Many batches | Until limit | Holds |
| LDT-008 | Stress memory | Large history | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
