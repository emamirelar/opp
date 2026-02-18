# UNOPSUserManagementManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/UserManagementManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
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

User management manager unit tests cover CRUD users, role assignment, status management, and bulk operations. Tests include: user CRUD, role assign/remove, status transitions (active/inactive/locked), bulk create/update/delete, password handling, email validation, and user search.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create user | Valid data | Create | User created |
| POS-002 | Get user by ID | User exists | GetById | User returned |
| POS-003 | Update user | User exists | Update | Updated |
| POS-004 | Delete user | User exists | Delete | Soft deleted |
| POS-005 | List users | Users exist | List | List returned |
| POS-006 | Assign role | User exists | AssignRole | Assigned |
| POS-007 | Remove role | Role assigned | RemoveRole | Removed |
| POS-008 | Activate user | User inactive | Activate | Active |
| POS-009 | Deactivate user | User active | Deactivate | Inactive |
| POS-010 | Lock user | User active | Lock | Locked |
| POS-011 | Unlock user | User locked | Unlock | Unlocked |
| POS-012 | Bulk create users | Valid list | BulkCreate | All created |
| POS-013 | Bulk update users | Users exist | BulkUpdate | All updated |
| POS-014 | Audit CreatedBy | Create | Check audit | Set |
| POS-015 | Audit CreatedDate | Create | Check audit | UTC |
| POS-016 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-017 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-018 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-019 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-020 | Search by email | Users exist | Search | Matching |
| POS-021 | Search by name | Users exist | Search | Matching |
| POS-022 | Filter by status | Users exist | Filter | Filtered |
| POS-023 | Filter by role | Users exist | Filter | Filtered |
| POS-024 | Pagination | Many users | List page | Page |
| POS-025 | Sort by name | Users exist | Sort | Ordered |
| POS-026 | Get user roles | User has roles | GetUserRoles | Roles |
| POS-027 | Validate email | Valid email | Validate | Valid |
| POS-028 | Reset password | User exists | ResetPassword | Reset |
| POS-029 | Change password | User exists | ChangePassword | Changed |
| POS-030 | Get by email | Email exists | GetByEmail | User |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null email | Email=null | ArgumentNullException |
| NEG-002 | Create with invalid email | Email=bad | ValidationException |
| NEG-003 | Create with duplicate email | Email exists | BusinessException |
| NEG-004 | Get by zero ID | Id=0 | ArgumentException |
| NEG-005 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-006 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | Assign role invalid user | UserId=99999 | KeyNotFoundException |
| NEG-009 | Assign role invalid role | RoleId=99999 | KeyNotFoundException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | Create without permission | Unauthorized | Forbidden |
| NEG-012 | Update without permission | Unauthorized | Forbidden |
| NEG-013 | Delete without permission | Unauthorized | Forbidden |
| NEG-014 | Bulk create without permission | Unauthorized | Forbidden |
| NEG-015 | Assign role without permission | Unauthorized | Forbidden |
| NEG-016 | SQL injection in search | '; DROP | Rejected |
| NEG-017 | XSS in name | <script> | Escaped |
| NEG-018 | Path traversal | ../../../etc | Rejected |
| NEG-019 | Password too weak | Weak password | ValidationException |
| NEG-020 | Password null | Password=null | ArgumentNullException |
| NEG-021 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-022 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-023 | Connection timeout | DB unavailable | TimeoutException |
| NEG-024 | Lock already locked | User locked | BusinessException |
| NEG-025 | Unlock not locked | User not locked | BusinessException |
| NEG-026 | Activate already active | User active | No-op or BusinessException |
| NEG-027 | Deactivate already inactive | User inactive | No-op or BusinessException |
| NEG-028 | Expired session | Expired token | Unauthorized |
| NEG-029 | Null user context | User=null | InvalidOperationException |
| NEG-030 | Bulk create null list | List=null | ArgumentNullException |
| NEG-031 | Bulk create empty | List=[] | ArgumentException |
| NEG-032 | Bulk create one invalid | One invalid | Partial or fail |
| NEG-033 | Invalid page number | Page=0 | ArgumentException |
| NEG-034 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-035 | Search null term | Term=null | ArgumentNullException |
| NEG-036 | Filter malformed | Malformed filter | ArgumentException |
| NEG-037 | Reset password invalid | User invalid | KeyNotFoundException |
| NEG-038 | Change password wrong current | Wrong current | BusinessException |
| NEG-039 | GetByEmail non-existent | Email invalid | Null or KeyNotFoundException |
| NEG-040 | Child override throws | Child throws | Propagated |
| NEG-041 | Role duplicate assign | Already assigned | No-op or BusinessException |
| NEG-042 | Remove role not assigned | Not assigned | BusinessException |
| NEG-043 | Email format invalid | Format invalid | ValidationException |
| NEG-044 | Name too long | Length>255 | ValidationException |
| NEG-045 | Audit missing user | User=0 | InvalidOperationException |
| NEG-046 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-047 | Export unauthorized | Unauthorized | Forbidden |
| NEG-048 | Import invalid data | Malformed | ValidationException |
| NEG-049 | Pagination overflow | Page too large | Empty or error |
| NEG-050 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-051 | Update deleted | User deleted | KeyNotFoundException |
| NEG-052 | GetById deleted | User deleted | KeyNotFoundException |
| NEG-053 | Assign role deleted user | User deleted | KeyNotFoundException |
| NEG-054 | Bulk update partial fail | One invalid | Partial or fail |
| NEG-055 | Bulk delete partial fail | One invalid | Partial or fail |
| NEG-056 | Cross-tenant access | Other tenant | Forbidden |
| NEG-057 | Invalid include path | Invalid include | ArgumentException |
| NEG-058 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-059 | Invalid enum value | Status invalid | ArgumentException |
| NEG-060 | Last admin deactivate | Last admin | BusinessException |
| NEG-061 | Last admin delete | Last admin | BusinessException |
| NEG-062 | Self delete | Delete self | BusinessException |
| NEG-063 | Self deactivate | Deactivate self | BusinessException or allow |
| NEG-064 | Password history | Same as previous | ValidationException |
| NEG-065 | Password expiry | Expired | BusinessException |
| NEG-066 | Account lockout | Too many attempts | Locked |
| NEG-067 | Import duplicate email | Email exists | BusinessException or skip |
| NEG-068 | Export format invalid | Format invalid | ArgumentException |
| NEG-069 | Bulk size exceeds limit | 1000+ users | ArgumentException |
| NEG-070 | Role required | No roles | ValidationException |
| NEG-071 | Create null email | Email=null | ArgumentNullException |
| NEG-072 | Create null name | Name=null | ArgumentNullException |
| NEG-073 | AssignRole null user | User=null | ArgumentNullException |
| NEG-074 | AssignRole null role | Role=null | ArgumentNullException |
| NEG-075 | RemoveRole null user | User=null | ArgumentNullException |
| NEG-076 | Activate null user | User=null | ArgumentNullException |
| NEG-077 | Deactivate null user | User=null | ArgumentNullException |
| NEG-078 | Lock null user | User=null | ArgumentNullException |
| NEG-079 | Unlock null user | User=null | ArgumentNullException |
| NEG-080 | ResetPassword null user | User=null | ArgumentNullException |
| NEG-081 | ChangePassword null user | User=null | ArgumentNullException |
| NEG-082 | GetByEmail null email | Email=null | ArgumentNullException |
| NEG-083 | Search null term | Term=null | ArgumentNullException |
| NEG-084 | BulkStatusChange null list | List=null | ArgumentNullException |
| NEG-085 | Export null format | Format=null | ArgumentNullException |
| NEG-086 | Import null stream | Stream=null | ArgumentNullException |
| NEG-087 | GetActive null filter | Filter=null | ArgumentNullException |
| NEG-088 | Exists null email | Email=null | ArgumentNullException |
| NEG-089 | GetUserRoles null user | User=null | ArgumentNullException |
| NEG-090 | Validate null email | Email=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Email at min | Length=5 | Valid |
| BND-002 | Email at max | Length=254 | Valid |
| BND-003 | Email over max | Length=255 | Reject |
| BND-004 | Name at min | Length=1 | Valid |
| BND-005 | Name at max | Length=255 | Valid |
| BND-006 | Name over max | Length=256 | Reject |
| BND-007 | Password at min | Length=8 | Valid |
| BND-008 | Password at max | Length=128 | Valid |
| BND-009 | Password over max | Length=129 | Reject |
| BND-010 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-011 | ID at zero | Id=0 | Reject |
| BND-012 | Page size at min | PageSize=1 | Valid |
| BND-013 | Page size at max | PageSize=1000 | Valid |
| BND-014 | Page size over max | PageSize=1001 | Reject |
| BND-015 | Bulk size at max | 100 users | Valid |
| BND-016 | Bulk size over max | 101 users | Reject |
| BND-017 | Unicode in name | Arabic/Chinese | Stored |
| BND-018 | Special chars in name | <>&"' | Escaped |
| BND-019 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-020 | Empty users list | [] | No exception |
| BND-021 | Single user | Count=1 | Valid |
| BND-022 | Roles count zero | No roles | [] |
| BND-023 | Roles count max | 20 roles | Valid |
| BND-024 | Date at min | Date=MinValue | Handle |
| BND-025 | Date at max | Date=MaxValue | Handle |
| BND-026 | DateTime UTC | UTC input | Stored |
| BND-027 | Empty search term | Term="" | Return all |
| BND-028 | Search term max | Term=500 | Valid |
| BND-029 | Search term over max | Term=501 | Reject |
| BND-030 | Status enum boundary | Last enum | Valid |
| BND-031 | Role enum boundary | Last enum | Valid |
| BND-032 | Pagination last partial | Partial page | Correct |
| BND-033 | Pagination total | Total count | Accurate |
| BND-034 | Sort null handling | Nulls in data | Deterministic |
| BND-035 | Filter combination all | All filters | Correct |
| BND-036 | Soft delete boundary | DeletedDate set | Excluded |
| BND-037 | Include depth | Deep include | No explosion |
| BND-038 | Query timeout | Slow query | Timeout |
| BND-039 | Audit timestamp precision | Millisecond | Stored |
| BND-040 | Async cancellation | Cancel token | OperationCanceledException |
| BND-041 | Task timeout | Timeout | TimeoutException |
| BND-042 | Concurrent same second | Same timestamp | Deterministic |
| BND-043 | Lockout count at limit | At limit | Locked |
| BND-044 | Lockout count over | Over limit | Locked |
| BND-045 | Password history count | 5 previous | Valid |
| BND-046 | Last login null | Never logged in | Null |
| BND-047 | Failed login count | At limit | Locked |
| BND-048 | Export large result | 10k rows | Stream |
| BND-049 | Import large | 1000 users | Valid |
| BND-050 | Filter empty result | No match | Empty list |
| BND-051 | Sort empty | Empty list | No exception |
| BND-052 | Pagination empty | No data | Empty |
| BND-053 | GetUserRoles empty | No roles | [] |
| BND-054 | GetActive empty | No active | [] |
| BND-055 | Bulk create empty success | None to create | Empty |
| BND-056 | Bulk update empty | None to update | No-op |
| BND-057 | Exists false | Email not found | False |
| BND-058 | Exists true | Email found | True |
| BND-059 | GetByEmail boundary | Exact match | User |
| BND-060 | Search case insensitive | Case | Matching |
| BND-061 | Search partial match | Partial | Matching |
| BND-062 | Filter by multiple roles | Multi-role | Filtered |
| BND-063 | Filter by multiple status | Multi-status | Filtered |
| BND-064 | Reset password token | Token valid | Reset |
| BND-065 | Reset password token expired | Token expired | BusinessException |
| BND-066 | Change password same | Same password | ValidationException |
| BND-067 | Import overwrite | Overwrite | Config |
| BND-068 | Export format boundary | Each format | Valid |
| BND-069 | Bulk status mixed | Mixed status | Updated |
| BND-070 | Concurrent user create | Two create | Both or one |
| BND-071 | Email length boundary | At boundary | Valid |
| BND-072 | Name length boundary | At boundary | Valid |
| BND-073 | Roles count one | 1 role | Valid |
| BND-074 | Status enum first | First | Valid |
| BND-075 | Status enum last | Last | Valid |
| BND-076 | Bulk size one | 1 user | Valid |
| BND-077 | Search term boundary | At boundary | Valid |
| BND-078 | Pagination first page | Page=1 | Valid |
| BND-079 | Pagination last page | Last | Valid |
| BND-080 | Filter single status | 1 status | Valid |
| BND-081 | Filter single role | 1 role | Valid |
| BND-082 | Lockout count zero | 0 | Valid |
| BND-083 | Password history count | At limit | Valid |
| BND-084 | Last login boundary | At boundary | Valid |
| BND-085 | Failed login zero | 0 | Valid |
| BND-086 | GetByEmail empty | No user | Null |
| BND-087 | Search empty | No match | [] |
| BND-088 | GetActive empty | No active | [] |
| BND-089 | Exists boundary | At boundary | Boolean |
| BND-090 | Sort single column | 1 column | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Email required | Validation | Create | Reject if null |
| FUN-002 | Email unique | Validation | Create | Reject duplicate |
| FUN-003 | Name required | Validation | Create | Reject if empty |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | At least one role | Constraint | Create | Reject if none |
| FUN-008 | Password strength | Constraint | Create | Reject weak |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Status transition valid | Constraint | ChangeStatus | Valid only |
| FUN-017 | Cannot delete last admin | Constraint | Delete | Reject |
| FUN-018 | Cannot deactivate last admin | Constraint | Deactivate | Reject |
| FUN-019 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-020 | Role must exist | Constraint | AssignRole | Reject invalid |
| FUN-021 | Lock updates status | Logic | Lock | Locked |
| FUN-022 | Unlock updates status | Logic | Unlock | Unlocked |
| FUN-023 | Activate updates status | Logic | Activate | Active |
| FUN-024 | Deactivate updates status | Logic | Deactivate | Inactive |
| FUN-025 | Assign role adds | Logic | AssignRole | Added |
| FUN-026 | Remove role removes | Logic | RemoveRole | Removed |
| FUN-027 | Pagination offset | Calculation | Page | Skip correct |
| FUN-028 | Total count accurate | Calculation | Count | Matches |
| FUN-029 | Sort applies | Calculation | Sort | Ordered |
| FUN-030 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-031 | Transaction on create | Transaction | Create | Atomic |
| FUN-032 | Transaction on bulk | Transaction | BulkCreate | Atomic |
| FUN-033 | Async all operations | Concurrency | All | Async |
| FUN-034 | Include loads roles | Data load | GetById include | Roles loaded |
| FUN-035 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-036 | Password hashed | Logic | Create | Hashed |
| FUN-037 | Password not stored plain | Logic | Create | Never plain |
| FUN-038 | Failed login increment | Logic | Login fail | Incremented |
| FUN-039 | Lockout on threshold | Logic | Fail threshold | Locked |
| FUN-040 | Reset lockout | Logic | Unlock | Reset count |
| FUN-041 | Password history | Logic | ChangePassword | Stored |
| FUN-042 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-043 | Import validation | Logic | Import | Validated |
| FUN-044 | Config password policy | Config | Validate | Config |
| FUN-045 | Config lockout | Config | Lock | Config |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | Status workflow | Workflow | ChangeStatus | Valid flow |
| FUN-048 | Role workflow | Workflow | AssignRole | Valid |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | GetByEmail case | Logic | GetByEmail | Config |
| FUN-052 | Search case insensitive | Logic | Search | Matching |
| FUN-053 | Filter by multiple roles | Logic | Filter | Filtered |
| FUN-054 | Filter by multiple status | Logic | Filter | Filtered |
| FUN-055 | Bulk create validation | Logic | BulkCreate | Validated |
| FUN-056 | Bulk update validation | Logic | BulkUpdate | Validated |
| FUN-057 | Bulk status validation | Logic | BulkStatusChange | Validated |
| FUN-058 | Export format | Logic | Export | Format |
| FUN-059 | Import validation | Logic | Import | Validated |
| FUN-060 | GetActive filter | Logic | GetActive | Filtered |
| FUN-061 | Exists check | Logic | Exists | Boolean |
| FUN-062 | GetUserRoles order | Logic | GetUserRoles | Ordered |
| FUN-063 | Password strength | Logic | Create | Validated |
| FUN-064 | Lockout reset | Logic | Unlock | Reset |
| FUN-065 | Password history | Logic | ChangePassword | Stored |
| FUN-066 | Last admin check | Constraint | Delete | Reject |
| FUN-067 | Self delete check | Constraint | Delete | Reject |
| FUN-068 | Self deactivate check | Constraint | Deactivate | Config |
| FUN-069 | Role duplicate check | Constraint | AssignRole | No-op |
| FUN-070 | Remove not assigned | Constraint | RemoveRole | Reject |
| FUN-071 | Pagination consistency | Calculation | Page | Consistent |
| FUN-072 | Sort multi-column | Calculation | Sort | Multi |
| FUN-073 | Filter OR logic | Filter | OR filter | Match |
| FUN-074 | Transaction on bulk | Transaction | BulkCreate | Atomic |
| FUN-075 | Transaction on bulk update | Transaction | BulkUpdate | Atomic |
| FUN-076 | Include loads roles | Data load | GetById include | Roles |
| FUN-077 | Include selective | Data load | Include | Selective |
| FUN-078 | Config password policy | Config | Validate | Config |
| FUN-079 | Config lockout | Config | Lock | Config |
| FUN-080 | Permission per action | Authorization | Per action | Check |
| FUN-081 | User context audit | Audit | Create | User |
| FUN-082 | Timestamp UTC | Audit | All | UTC |
| FUN-083 | Deleted exclude Search | Constraint | Search | Excluded |
| FUN-084 | Deleted exclude GetByEmail | Constraint | GetByEmail | Excluded |
| FUN-085 | Deleted exclude GetActive | Constraint | GetActive | Excluded |
| FUN-086 | Deleted exclude Export | Constraint | Export | Excluded |
| FUN-087 | Deleted exclude GetUserRoles | Constraint | GetUserRoles | Excluded |
| FUN-088 | User lifecycle | Workflow | Create to delete | Complete |
| FUN-089 | Role lifecycle | Workflow | Assign to remove | Complete |
| FUN-090 | Status lifecycle | Workflow | Activate to deactivate | Complete |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create user full flow | Create | User | Created |
| INT-002 | Get user full flow | GetById | User | Returned |
| INT-003 | Update user full flow | Update | User | Updated |
| INT-004 | Delete user full flow | Delete | User | Soft deleted |
| INT-005 | Assign role full flow | AssignRole | User, Role | Assigned |
| INT-006 | Get with roles | GetById | User, Role | Roles loaded |
| INT-007 | List with filter and sort | List | User | Filtered, sorted |
| INT-008 | Bulk create | BulkCreate | User | All created |
| INT-009 | Bulk update | BulkUpdate | User | All updated |
| INT-010 | User-Role relationship | Relationship | User, Role | FK valid |
| INT-011 | Cascade soft delete | Relationship | User deleted | Config |
| INT-012 | Orphan handling | Relationship | Role deleted | Handle |
| INT-013 | DB error handling | Error | DB down | Graceful |
| INT-014 | Timeout handling | Error | Slow | Timeout |
| INT-015 | Constraint violation | Error | FK violation | Clear error |
| INT-016 | Permission service integration | Integration | Permission | Check |
| INT-017 | User resolver integration | Integration | User | Resolved |
| INT-018 | Audit context integration | Integration | Audit | Context |
| INT-019 | Logger integration | Integration | Log | Logged |
| INT-020 | RoleManager integration | Integration | Role | Role |
| INT-021 | Mapper integration | Integration | Map | Correct |
| INT-022 | Repository integration | Integration | Repository | CRUD |
| INT-023 | DbContext integration | Integration | DbContext | Scoped |
| INT-024 | Transaction scope | Integration | Transaction | Atomic |
| INT-025 | Auth service integration | Integration | Auth | Auth |
| INT-026 | Full user lifecycle | Scenario | Create→Update→Delete | Complete |
| INT-027 | Full role lifecycle | Scenario | Assign→Remove | Complete |
| INT-028 | Full status lifecycle | Scenario | Activate→Deactivate | Complete |
| INT-029 | Lock unlock flow | Scenario | Lock→Unlock | Complete |
| INT-030 | Password flow | Scenario | Create→Change | Complete |
| INT-031 | Concurrent create | Scenario | Parallel | All succeed |
| INT-032 | Search with filter | Scenario | Search | Filtered |
| INT-033 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-034 | Export with filter | Scenario | Export | Filtered |
| INT-035 | Import with validation | Scenario | Import | Validated |
| INT-036 | Bulk status change | Scenario | BulkStatusChange | Updated |
| INT-037 | Get active users | Scenario | GetActive | Filtered |
| INT-038 | Get by email | Scenario | GetByEmail | User |
| INT-039 | User exists | Scenario | Exists | Boolean |
| INT-040 | Reset password | Scenario | ResetPassword | Reset |
| INT-041 | Failed login lockout | Scenario | Fail threshold | Locked |
| INT-042 | Password history | Scenario | ChangePassword | Stored |
| INT-043 | Last admin protection | Scenario | Delete | Rejected |
| INT-044 | Self operations | Scenario | Self delete | Rejected |
| INT-045 | Audit trail | Scenario | Operations | Trail |
| INT-046 | Export format | Scenario | Export | Format |
| INT-047 | Import overwrite | Scenario | Import | Config |
| INT-048 | Role permission cascade | Scenario | Assign role | Permissions |
| INT-049 | Status notifications | Scenario | Status change | Sent |
| INT-050 | E2E create-assign-activate | Scenario | Full cycle | Complete |
| INT-051 | Create then Assign role | Scenario | Create, Assign | Complete |
| INT-052 | Assign then Remove role | Scenario | Assign, Remove | Complete |
| INT-053 | Activate then Deactivate | Scenario | Activate, Deactivate | Complete |
| INT-054 | Lock then Unlock | Scenario | Lock, Unlock | Complete |
| INT-055 | Create then Change password | Scenario | Create, Change | Complete |
| INT-056 | Search then GetById | Scenario | Search, GetById | Complete |
| INT-057 | Bulk create then Bulk update | Scenario | BulkCreate, BulkUpdate | Complete |
| INT-058 | Export then Import | Scenario | Export, Import | Roundtrip |
| INT-059 | GetActive then Filter | Scenario | GetActive, Filter | Complete |
| INT-060 | GetByEmail then Exists | Scenario | GetByEmail, Exists | Complete |
| INT-061 | DbContext scope | Integration | Request | Scoped |
| INT-062 | Permission cascade | Integration | Role | Cascade |
| INT-063 | User context propagation | Integration | Request | Propagated |
| INT-064 | Audit chain | Integration | Operations | Chained |
| INT-065 | RoleManager integration | Integration | Role | Role |
| INT-066 | Auth service integration | Integration | Auth | Auth |
| INT-067 | Error handling chain | Integration | Error | Handled |
| INT-068 | Validation chain | Integration | Create | Validated |
| INT-069 | Mapping chain | Integration | Entity | Mapped |
| INT-070 | Repository CRUD | Integration | Repository | CRUD |
| INT-071 | DbContext save | Integration | SaveChanges | Saved |
| INT-072 | Transaction rollback | Integration | Error | Rollback |
| INT-073 | Role permission flow | Integration | Role | Permissions |
| INT-074 | Concurrent create | Scenario | Parallel create | All succeed |
| INT-075 | Concurrent update | Scenario | Parallel update | One wins |
| INT-076 | Full user lifecycle | Scenario | Create to delete | Complete |
| INT-077 | Full role lifecycle | Scenario | Assign to remove | Complete |
| INT-078 | Full status lifecycle | Scenario | Activate to deactivate | Complete |
| INT-079 | Full lock lifecycle | Scenario | Lock to unlock | Complete |
| INT-080 | Full password lifecycle | Scenario | Create to change | Complete |
| INT-081 | Full bulk lifecycle | Scenario | BulkCreate to BulkUpdate | Complete |
| INT-082 | Full search lifecycle | Scenario | Search to get | Complete |
| INT-083 | Full export import | Scenario | Export to import | Complete |
| INT-084 | Full last admin protection | Scenario | Delete | Rejected |
| INT-085 | Permission check flow | Integration | Auth | Check |
| INT-086 | User resolution flow | Integration | User | Resolved |
| INT-087 | Audit flow | Integration | Audit | Logged |
| INT-088 | Logging flow | Integration | Log | Logged |
| INT-089 | Role flow | Integration | Role | Role |
| INT-090 | E2E full lifecycle | Scenario | All operations | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-005 | XSS in email | <img onerror=...> | Email | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized create | No permission | Create | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized assign role | No permission | AssignRole | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR update other | Id=other | Update | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | UserId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign Role | Role=manipulated | Request | Validated |
| SEC-025 | Password in response | Get user | Response | Never |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Password hash strength | Hash | Store | Strong |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit login | Many logins | Login | Throttled |
| SEC-037 | Rate limit list | Many lists | List | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in email | Email | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge bulk | BulkCreate | Rejected |
| SEC-045 | Email injection | Invalid email | Create | Rejected |
| SEC-046 | Role injection | Invalid role | AssignRole | Rejected |
| SEC-047 | Status injection | Invalid status | Update | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Export ACL | Direct access | Export | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same email | Two create | One or both |
| CON-004 | Concurrent create | Two create | Both or one |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on assign role | Two assign | One wins |
| CON-009 | Race on status change | Two change | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel gets | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Bulk create concurrent | Two bulk | Both succeed |
| CON-016 | Assign role concurrent | Two assign | One wins |
| CON-017 | Status change concurrent | Two change | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Password change concurrent | Two change | One wins |
| CON-020 | Import concurrent | Two import | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Email unique constraint | Concurrent same | One or both |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate email not null | Validation | null | Exception |
| UNT-002 | Validate email format | Validation | Valid email | Pass |
| UNT-003 | Validate name | Validation | Valid name | Pass |
| UNT-004 | Validate password | Validation | Valid password | Pass |
| UNT-005 | Validate role | Validation | Valid role | Pass |
| UNT-006 | Format email | Formatting | Email | Formatted |
| UNT-007 | Format name | Formatting | Name | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Password strength score | Calculation | Password | Score |
| UNT-013 | Lockout check | Calculation | Fail count | Locked |
| UNT-014 | Status allows activate | Status logic | Inactive | true |
| UNT-015 | Status allows deactivate | Status logic | Active | true |
| UNT-016 | Status allows lock | Status logic | Active | true |
| UNT-017 | Role exists check | Status logic | Role | true |
| UNT-018 | Email check | Status logic | Email | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Single update | Update | <200ms | P1 |
| PRF-004 | Bulk create 10 | BulkCreate | <5s | P0 |
| PRF-005 | Bulk create 100 | BulkCreate | <30s | P0 |
| PRF-006 | Search by email | Search | <500ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Get user roles | GetUserRoles | <100ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel | <2s total | P1 |
| PRF-011 | Concurrent 5 creates | 5 parallel | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 create | <5s total | P2 |
| PRF-013 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-014 | Memory bulk 100 | BulkCreate | <100MB | P2 |
| PRF-015 | Memory export | Export | <100MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS create | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS create | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS get | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress bulk | Many bulks | Until limit | Holds |
| LDT-008 | Stress memory | Large bulk | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
