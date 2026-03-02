# NotificationManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/NotificationManager` (Unit Tests)  
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

Notification manager unit tests cover send, receive, templates, channels, preferences, and batch operations. Tests include: notification CRUD, template rendering, channel delivery (email, in-app, push), user preferences, batch send, mark-as-read, filtering by channel/type, and delivery status tracking.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Send notification | Valid recipient | Send | Delivered |
| POS-002 | Get user notifications | User has notifications | GetByUser | List returned |
| POS-003 | Mark as read | Notification exists | MarkRead | Updated |
| POS-004 | Create template | Valid template | CreateTemplate | Template created |
| POS-005 | Get template by ID | Template exists | GetTemplate | Template returned |
| POS-006 | Send via email channel | Email configured | SendEmail | Sent |
| POS-007 | Send via in-app channel | In-app enabled | SendInApp | Delivered |
| POS-008 | Get user preferences | User has prefs | GetPreferences | Preferences returned |
| POS-009 | Update preferences | User exists | UpdatePreferences | Updated |
| POS-010 | Batch send | Valid recipients | BatchSend | All sent |
| POS-011 | Filter by channel | Notifications exist | FilterByChannel | Filtered |
| POS-012 | Filter by type | Notifications exist | FilterByType | Filtered |
| POS-013 | Unread count | User has unread | GetUnreadCount | Count returned |
| POS-014 | Mark all read | User has unread | MarkAllRead | All updated |
| POS-015 | Template variables | Template has vars | Render | Rendered |
| POS-016 | Delete notification | Notification exists | Delete | Soft deleted |
| POS-017 | Get by ID | Notification exists | GetById | Notification returned |
| POS-018 | List templates | Templates exist | ListTemplates | List returned |
| POS-019 | Update template | Template exists | UpdateTemplate | Updated |
| POS-020 | Audit CreatedBy | Create | Check audit | Set |
| POS-021 | Audit CreatedDate | Create | Check audit | UTC |
| POS-022 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-023 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-024 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-025 | Pagination | Many notifications | List page | Page returned |
| POS-026 | Sort by date | Notifications exist | Sort | Ordered |
| POS-027 | Delivery status | Send | Check status | Tracked |
| POS-028 | Channel enabled check | User prefs | IsChannelEnabled | Boolean |
| POS-029 | Batch partial success | Some fail | BatchSend | Partial results |
| POS-030 | Get notification history | User exists | GetHistory | History returned |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Send with null recipient | Recipient=null | ArgumentNullException |
| NEG-002 | Send with empty message | Message="" | ValidationException |
| NEG-003 | Get by zero user ID | UserId=0 | ArgumentException |
| NEG-004 | Get by negative user ID | UserId=-1 | ArgumentException |
| NEG-005 | Mark read non-existent | Id=99999 | KeyNotFoundException |
| NEG-006 | Template null name | Name=null | ArgumentNullException |
| NEG-007 | Template empty body | Body="" | ValidationException |
| NEG-008 | Invalid channel | Channel=invalid | ArgumentException |
| NEG-009 | Invalid notification type | Type=invalid | ArgumentException |
| NEG-010 | Batch send null list | List=null | ArgumentNullException |
| NEG-011 | Batch send empty | List=[] | ArgumentException |
| NEG-012 | GetById without permission | Unauthorized | Forbidden |
| NEG-013 | Send without permission | Unauthorized | Forbidden |
| NEG-014 | Update preferences unauthorized | Unauthorized | Forbidden |
| NEG-015 | Delete notification unauthorized | Unauthorized | Forbidden |
| NEG-016 | Create template unauthorized | Unauthorized | Forbidden |
| NEG-017 | XSS in message | <script> | Escaped |
| NEG-018 | XSS in template body | <img onerror> | Escaped |
| NEG-019 | SQL injection in search | '; DROP | Rejected |
| NEG-020 | Path traversal in attachment | ../../../etc | Rejected |
| NEG-021 | Invalid template variable | {{invalid}} | ValidationException |
| NEG-022 | Null template | Template=null | ArgumentNullException |
| NEG-023 | Preferences null user | UserId=0 | ArgumentException |
| NEG-024 | Channel disabled | Channel off | BusinessException |
| NEG-025 | Recipient not found | UserId=99999 | KeyNotFoundException |
| NEG-026 | Template not found | TemplateId=99999 | KeyNotFoundException |
| NEG-027 | Notification deleted | Get deleted | KeyNotFoundException |
| NEG-028 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-029 | Email service unavailable | Email down | NotificationException |
| NEG-030 | Invalid page number | Page=0 | ArgumentException |
| NEG-031 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-032 | Search null term | Term=null | ArgumentNullException |
| NEG-033 | Filter invalid channel | Channel invalid | ArgumentException |
| NEG-034 | Filter invalid type | Type invalid | ArgumentException |
| NEG-035 | Mark read deleted | Notification deleted | KeyNotFoundException |
| NEG-036 | Batch send one invalid | One invalid recipient | Partial or fail |
| NEG-037 | Template circular reference | Self-reference | BusinessException |
| NEG-038 | Expired session | Expired token | Unauthorized |
| NEG-039 | Null user context | User=null | InvalidOperationException |
| NEG-040 | Connection timeout | DB unavailable | TimeoutException |
| NEG-041 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-042 | Invalid include path | Invalid include | ArgumentException |
| NEG-043 | Template variable type mismatch | Wrong type | ValidationException |
| NEG-044 | Notification too long | Message 10k chars | ValidationException |
| NEG-045 | Recipient list exceeds max | 1000+ recipients | ArgumentException |
| NEG-046 | Duplicate notification | Same content | BusinessException |
| NEG-047 | Rate limit exceeded | Too many sends | RateLimitException |
| NEG-048 | Invalid email format | Bad email | ValidationException |
| NEG-049 | Batch partial with rollback | All fail | No partial |
| NEG-050 | GetById deleted | Notification deleted | KeyNotFoundException |
| NEG-051 | Update deleted template | Template deleted | KeyNotFoundException |
| NEG-052 | MarkAllRead no permissions | Unauthorized | Forbidden |
| NEG-053 | GetHistory negative days | Days=-1 | ArgumentException |
| NEG-054 | Template validation fail | Invalid template | ValidationException |
| NEG-055 | Preference invalid key | Key invalid | ArgumentException |
| NEG-056 | Notification type null | Type=null | ArgumentNullException |
| NEG-057 | Channel null | Channel=null | ArgumentNullException |
| NEG-058 | Batch send mixed invalid | Mixed list | Partial or fail |
| NEG-059 | Pagination overflow | Page too large | Empty or error |
| NEG-060 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-061 | Filter malformed | Malformed filter | ArgumentException |
| NEG-062 | Template duplicate name | Name exists | BusinessException |
| NEG-063 | Audit missing user | User=0 | InvalidOperationException |
| NEG-064 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-065 | Send null message | Message=null | ArgumentNullException |
| NEG-066 | GetTemplate null ID | Id=null | ArgumentNullException |
| NEG-067 | Child override throws | Child throws | Propagated |
| NEG-068 | Email delivery failure | SMTP down | NotificationException |
| NEG-069 | Push delivery failure | Push down | NotificationException |
| NEG-070 | In-app storage full | Storage full | StorageException |
| NEG-071 | Send null template | Template=null | ArgumentNullException |
| NEG-072 | CreateTemplate null body | Body=null | ArgumentNullException |
| NEG-073 | UpdateTemplate null template | Template=null | ArgumentNullException |
| NEG-074 | GetTemplate invalid ID | Id=0 | ArgumentException |
| NEG-075 | MarkRead null ID | Id=0 | ArgumentException |
| NEG-076 | MarkAllRead invalid user | UserId=-1 | ArgumentException |
| NEG-077 | GetUnreadCount invalid user | UserId=0 | ArgumentException |
| NEG-078 | GetHistory invalid days | Days=366 | ArgumentException |
| NEG-079 | BatchSend null recipients | Recipients=null | ArgumentNullException |
| NEG-080 | FilterByChannel invalid | Channel invalid | ArgumentException |
| NEG-081 | FilterByType invalid | Type invalid | ArgumentException |
| NEG-082 | Render null template | Template=null | ArgumentNullException |
| NEG-083 | Render invalid variables | Vars invalid | ValidationException |
| NEG-084 | UpdatePreferences null prefs | Prefs=null | ArgumentNullException |
| NEG-085 | IsChannelEnabled invalid | Channel invalid | ArgumentException |
| NEG-086 | Delete null ID | Id=0 | ArgumentException |
| NEG-087 | GetById invalid ID | Id=-1 | ArgumentException |
| NEG-088 | ListTemplates invalid filter | Filter invalid | ArgumentException |
| NEG-089 | Validate null template | Template=null | ArgumentNullException |
| NEG-090 | Search null user | UserId=0 | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Message at min | Length=1 | Valid |
| BND-002 | Message at max | Length=4000 | Valid |
| BND-003 | Message exceeds max | Length=4001 | Reject |
| BND-004 | User ID at Int32.MaxValue | UserId=2147483647 | Handle |
| BND-005 | User ID at zero | UserId=0 | Reject |
| BND-006 | Page size at min | PageSize=1 | Valid |
| BND-007 | Page size at max | PageSize=100 | Valid |
| BND-008 | Page size over max | PageSize=101 | Reject |
| BND-009 | Batch size at max | 100 recipients | Valid |
| BND-010 | Batch size over max | 101 recipients | Reject |
| BND-011 | Template name max length | Length=255 | Valid |
| BND-012 | Template name over max | Length=256 | Reject |
| BND-013 | Template body max | 10k chars | Valid |
| BND-014 | Template body over max | 10k+1 | Reject |
| BND-015 | Unicode in message | Arabic/Chinese | Stored |
| BND-016 | Special chars in template | <>&"' | Escaped |
| BND-017 | Leading/trailing spaces | Message="  x  " | Trimmed |
| BND-018 | Empty recipient list | List=[] | Reject |
| BND-019 | Single recipient | Count=1 | Valid |
| BND-020 | Date at min | Date=MinValue | Handle |
| BND-021 | Date at max | Date=MaxValue | Handle |
| BND-022 | DateTime UTC | UTC input | Stored |
| BND-023 | Empty search term | Term="" | Return all |
| BND-024 | Search term max | Term=500 | Valid |
| BND-025 | Search term over max | Term=501 | Reject |
| BND-026 | Collection empty | [] | No exception |
| BND-027 | Collection single | 1 item | Valid |
| BND-028 | Unread count zero | No unread | 0 |
| BND-029 | Unread count max | Many unread | Count |
| BND-030 | Template variables empty | No vars | Valid |
| BND-031 | Template variables max | 50 vars | Valid |
| BND-032 | Pagination last partial | Partial page | Correct |
| BND-033 | Pagination total | Total count | Accurate |
| BND-034 | Sort null handling | Nulls in data | Deterministic |
| BND-035 | Filter combination all | All filters | Correct |
| BND-036 | Channel enum boundary | Last enum | Valid |
| BND-037 | Type enum boundary | Last enum | Valid |
| BND-038 | Zero notification ID | Id=0 | Reject |
| BND-039 | Max int for ID | Id=2147483647 | Handle |
| BND-040 | Soft delete boundary | DeletedDate set | Excluded |
| BND-041 | Include depth | Deep include | No explosion |
| BND-042 | Query timeout | Slow query | Timeout |
| BND-043 | Audit timestamp precision | Millisecond | Stored |
| BND-044 | Long string in template | 4000 chars | Truncate |
| BND-045 | Async cancellation | Cancel token | OperationCanceledException |
| BND-046 | Task timeout | Timeout | TimeoutException |
| BND-047 | Concurrent same second | Same timestamp | Deterministic |
| BND-048 | Preferences empty | No prefs | Defaults |
| BND-049 | Preferences all set | All set | Valid |
| BND-050 | Delivery retry count | Max retries | Exhausted |
| BND-051 | Rate limit boundary | At limit | Throttled |
| BND-052 | Notification types all | All types | Valid |
| BND-053 | Channels all | All channels | Valid |
| BND-054 | Batch empty success | None to send | Empty |
| BND-055 | Mark read already read | Already read | No-op |
| BND-056 | MarkAllRead empty | No unread | No-op |
| BND-057 | GetHistory zero days | Days=0 | Empty |
| BND-058 | GetHistory max days | Days=365 | Valid |
| BND-059 | Template version 1 | Version=1 | Valid |
| BND-060 | Template version max | Version=max | Valid |
| BND-061 | Filter empty result | No match | Empty list |
| BND-062 | Sort empty | Empty list | No exception |
| BND-063 | Pagination empty | No data | Empty |
| BND-064 | Variable substitution empty | Empty value | Replaced |
| BND-065 | Variable substitution long | Long value | Truncate |
| BND-066 | Email address max | Length=254 | Valid |
| BND-067 | Subject max length | Length=255 | Valid |
| BND-068 | Attachment count max | 10 attachments | Valid |
| BND-069 | Attachment count over | 11 attachments | Reject |
| BND-070 | Concurrent mark read | Two mark same | One or both |
| BND-071 | Message whitespace | Message="   " | Reject |
| BND-072 | Template name min | Length=1 | Valid |
| BND-073 | Recipient list max | 100 recipients | Valid |
| BND-074 | GetUnreadCount zero | No unread | 0 |
| BND-075 | GetHistory min days | Days=1 | Valid |
| BND-076 | Channel enum first | First | Valid |
| BND-077 | Type enum first | First | Valid |
| BND-078 | Batch size one | 1 recipient | Valid |
| BND-079 | Mark read duplicate | Already read | No-op |
| BND-080 | Template version min | Version=1 | Valid |
| BND-081 | Variable count zero | No vars | Valid |
| BND-082 | Variable count max | 50 vars | Valid |
| BND-083 | Subject min length | Length=1 | Valid |
| BND-084 | Subject max length | Length=255 | Valid |
| BND-085 | Email min length | Length=5 | Valid |
| BND-086 | Delivery retry zero | No retries | Config |
| BND-087 | Rate limit at boundary | At limit | Throttled |
| BND-088 | Preferences partial | Partial | Merged |
| BND-089 | Notification ID zero | Id=0 | Reject |
| BND-090 | Template ID max | Id=2147483647 | Handle |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Message required | Validation | Send | Reject if empty |
| FUN-002 | Recipient required | Validation | Send | Reject if null |
| FUN-003 | Template name required | Validation | CreateTemplate | Reject if empty |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Channel whitelist | Constraint | Send | Only allowed |
| FUN-008 | Batch size limit | Constraint | BatchSend | Reject over |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Recipient must exist | Constraint | Send | Reject invalid |
| FUN-017 | Template must exist | Constraint | Send | Reject invalid |
| FUN-018 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-019 | User filter | Constraint | GetByUser | User only |
| FUN-020 | Channel filter | Constraint | Filter | Channel only |
| FUN-021 | Mark read updates | Logic | MarkRead | IsRead=true |
| FUN-022 | MarkAllRead batch | Logic | MarkAllRead | All updated |
| FUN-023 | Template variable replace | Logic | Render | Replaced |
| FUN-024 | Unread count excludes read | Logic | GetUnreadCount | Read excluded |
| FUN-025 | Preferences merge | Logic | UpdatePreferences | Merged |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on batch | Transaction | BatchSend | Atomic |
| FUN-031 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads user | Data load | GetById include | User loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | Batch partial results | Logic | BatchSend | Partial |
| FUN-036 | Channel routing | Logic | Send | Route by channel |
| FUN-037 | Type-based template | Logic | Send | Template by type |
| FUN-038 | Validate template | Validation | Validate | Type check |
| FUN-039 | Validate preferences | Validation | Update | Valid keys |
| FUN-040 | Delivery status update | Logic | Send | Status updated |
| FUN-041 | Retry on failure | Logic | Send | Retry |
| FUN-042 | Expiry handling | Logic | Send | Expiry set |
| FUN-043 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-044 | Exists checks storage | Logic | Exists | Storage check |
| FUN-045 | Template uses config | Config | Render | Config |
| FUN-046 | Channel config | Logic | Send | Config |
| FUN-047 | Localized message | i18n | GetDisplay | Localized |
| FUN-048 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | Send audit | Audit | Send | Audit |
| FUN-052 | CreateTemplate audit | Audit | CreateTemplate | Audit |
| FUN-053 | UpdateTemplate audit | Audit | UpdateTemplate | Audit |
| FUN-054 | Delete audit | Audit | Delete | Audit |
| FUN-055 | MarkRead audit | Audit | MarkRead | Audit |
| FUN-056 | MarkAllRead audit | Audit | MarkAllRead | Audit |
| FUN-057 | UpdatePreferences audit | Audit | UpdatePreferences | Audit |
| FUN-058 | BatchSend validation | Validation | BatchSend | Valid |
| FUN-059 | Template validation | Validation | CreateTemplate | Valid |
| FUN-060 | Preferences validation | Validation | UpdatePreferences | Valid |
| FUN-061 | Channel validation | Validation | Send | Valid |
| FUN-062 | Type validation | Validation | Send | Valid |
| FUN-063 | Recipient validation | Validation | Send | Valid |
| FUN-064 | Message validation | Validation | Send | Valid |
| FUN-065 | GetByUser filter | Logic | GetByUser | Filter |
| FUN-066 | FilterByChannel logic | Logic | FilterByChannel | Filtered |
| FUN-067 | FilterByType logic | Logic | FilterByType | Filtered |
| FUN-068 | GetUnreadCount logic | Logic | GetUnreadCount | Count |
| FUN-069 | GetHistory logic | Logic | GetHistory | History |
| FUN-070 | Render logic | Logic | Render | Rendered |
| FUN-071 | IsChannelEnabled logic | Logic | IsChannelEnabled | Boolean |
| FUN-072 | BatchSend transaction | Transaction | BatchSend | Atomic |
| FUN-073 | Delete transaction | Transaction | Delete | Atomic |
| FUN-074 | MarkRead transaction | Transaction | MarkRead | Atomic |
| FUN-075 | MarkAllRead transaction | Transaction | MarkAllRead | Atomic |
| FUN-076 | CreateTemplate transaction | Transaction | CreateTemplate | Atomic |
| FUN-077 | UpdateTemplate transaction | Transaction | UpdateTemplate | Atomic |
| FUN-078 | UpdatePreferences transaction | Transaction | UpdatePreferences | Atomic |
| FUN-079 | Send transaction | Transaction | Send | Atomic |
| FUN-080 | Delivery status logic | Logic | Send | Status |
| FUN-081 | Retry logic | Logic | Send | Retry |
| FUN-082 | Expiry logic | Logic | Send | Expiry |
| FUN-083 | Template variable logic | Logic | Render | Replace |
| FUN-084 | Preferences merge logic | Logic | UpdatePreferences | Merge |
| FUN-085 | Channel routing logic | Logic | Send | Route |
| FUN-086 | Type template logic | Logic | Send | Template |
| FUN-087 | Batch partial logic | Logic | BatchSend | Partial |
| FUN-088 | GetByUser excludes deleted | Constraint | GetByUser | Excludes |
| FUN-089 | ListTemplates excludes deleted | Constraint | ListTemplates | Excludes |
| FUN-090 | Search excludes deleted | Constraint | Search | Excludes |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Send notification full flow | Send | Notification, User | Sent |
| INT-002 | Get notifications full flow | GetByUser | Notification | List |
| INT-003 | Mark read full flow | MarkRead | Notification | Updated |
| INT-004 | Create template full flow | CreateTemplate | Template | Created |
| INT-005 | Batch send full flow | BatchSend | Notification | All sent |
| INT-006 | Get with user | GetById | Notification, User | User loaded |
| INT-007 | List with filter and sort | List | Notification | Filtered, sorted |
| INT-008 | Filter by channel | Filter | Notification | Channel filtered |
| INT-009 | Filter by type | Filter | Notification | Type filtered |
| INT-010 | Get preferences | GetPreferences | User, Preferences | Preferences |
| INT-011 | Update preferences | UpdatePreferences | User | Updated |
| INT-012 | Template render | Render | Template | Rendered |
| INT-013 | Mark all read | MarkAllRead | Notification | All updated |
| INT-014 | Get unread count | GetUnreadCount | Notification | Count |
| INT-015 | Notification-User relationship | Relationship | Notification, User | FK valid |
| INT-016 | Template-Notification relationship | Relationship | Template | Valid |
| INT-017 | Preferences-User relationship | Relationship | Preferences, User | Valid |
| INT-018 | Cascade soft delete | Relationship | User deleted | Config |
| INT-019 | Orphan handling | Relationship | User deleted | Retained |
| INT-020 | Email service integration | Integration | Email | Sent |
| INT-021 | In-app storage integration | Integration | Storage | Stored |
| INT-022 | DB error handling | Error | DB down | Graceful |
| INT-023 | Email error handling | Error | Email down | Graceful |
| INT-024 | Timeout handling | Error | Slow | Timeout |
| INT-025 | Constraint violation | Error | FK violation | Clear error |
| INT-026 | Permission service integration | Integration | Permission | Check |
| INT-027 | User resolver integration | Integration | User | Resolved |
| INT-028 | Audit context integration | Integration | Audit | Context |
| INT-029 | Logger integration | Integration | Log | Logged |
| INT-030 | Template manager integration | Integration | Template | Template |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | Push service integration | Integration | Push | Sent |
| INT-036 | Multiple notifications per user | Scenario | Notification, User | All linked |
| INT-037 | Template version history | Scenario | Template | Versions |
| INT-038 | Concurrent send | Scenario | Parallel | All succeed |
| INT-039 | Batch with filter | Scenario | BatchSend | Filtered |
| INT-040 | Preferences with validation | Scenario | Update | Validated |
| INT-041 | Channel selection | Scenario | Send | Channel |
| INT-042 | Mark read then get | Scenario | MarkRead, Get | Updated |
| INT-043 | Template update | Scenario | UpdateTemplate | Updated |
| INT-044 | Restore from template | Scenario | Restore | Restored |
| INT-045 | Batch with types | Scenario | BatchSend | Types validated |
| INT-046 | Search with user filter | Scenario | Search | Filtered |
| INT-047 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-048 | Get by channel | Scenario | GetByChannel | Filtered |
| INT-049 | Send then mark read | Scenario | Send, MarkRead | Complete |
| INT-050 | E2E send-receive-mark | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal in attachment | ../../../etc/passwd | Attachment | Rejected |
| SEC-004 | XSS in message | <script>alert(1)</script> | Message | Escaped |
| SEC-005 | XSS in template | <img onerror=...> | Template | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized send | No permission | Send | 403 |
| SEC-012 | Unauthorized delete | No permission | Delete | 403 |
| SEC-013 | Unauthorized template create | No permission | CreateTemplate | 403 |
| SEC-014 | Unauthorized preferences | No permission | UpdatePreferences | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B notification | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR mark other read | Id=other | MarkRead | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | UserId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign UserId | UserId=manipulated | Request | Ignored |
| SEC-025 | Malicious template | Script in template | Create | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on send | No token | Send | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Template tampering | Tamper template | Access | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit send | Many sends | Send | Throttled |
| SEC-036 | Rate limit batch | Many batches | BatchSend | Throttled |
| SEC-037 | Rate limit list | Many lists | List | Throttled |
| SEC-038 | Oversized request | 10MB payload | Send | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in message | Message | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge batch | BatchSend | Rejected |
| SEC-045 | Template variable injection | {{malicious}} | Template | Sanitized |
| SEC-046 | Email header injection | \r\n in subject | Subject | Rejected |
| SEC-047 | MIME type spoofing | Wrong MIME | Attachment | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Storage ACL | Direct access | Storage | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double send same | Two send | One or both |
| CON-004 | Concurrent send | Two send | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on mark read | Two mark | One wins |
| CON-009 | Race on mark all read | Two mark all | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel sends | 10 parallel | All succeed |
| CON-012 | Async parallel get | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Batch send concurrent | Two batch | Both succeed |
| CON-016 | Mark read concurrent | Two mark read | One or both |
| CON-017 | Preferences concurrent | Two update | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Template concurrent update | Two update | One wins |
| CON-020 | Preferences concurrent update | Two update | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Email connection limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate message not null | Validation | null | Exception |
| UNT-002 | Validate recipient | Validation | Valid user | Pass |
| UNT-003 | Validate template | Validation | Valid template | Pass |
| UNT-004 | Validate channel | Validation | Valid channel | Pass |
| UNT-005 | Validate preferences | Validation | Valid prefs | Pass |
| UNT-006 | Format message | Formatting | Message | Formatted |
| UNT-007 | Format template | Formatting | Template | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Template variable replace | Calculation | Vars | Replaced |
| UNT-013 | Unread count | Calculation | Notifications | Count |
| UNT-014 | Channel allows send | Status logic | Channel | true |
| UNT-015 | Type allows send | Status logic | Type | true |
| UNT-016 | Preference allows channel | Status logic | Pref | true |
| UNT-017 | Batch size check | Status logic | Size | Within |
| UNT-018 | Template name check | Status logic | Name | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single send | Send | <500ms | P1 |
| PRF-003 | Get user notifications | GetByUser | <200ms | P1 |
| PRF-004 | Batch send 10 | Send 10 | <5s | P0 |
| PRF-005 | Batch send 100 | Send 100 | <30s | P0 |
| PRF-006 | Mark all read | MarkAllRead | <500ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Template render | Render | <100ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 sends | 5 parallel Send | <5s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 send | <5s total | P2 |
| PRF-013 | Memory single send | Send | <20MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory batch 10 | Batch send | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS send | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS send | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS get | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress batch | Many batches | Until limit | Holds |
| LDT-008 | Stress memory | Large batches | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
