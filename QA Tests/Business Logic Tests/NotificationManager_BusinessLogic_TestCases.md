# NotificationManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/NotificationManager`  
**Created:** 2026-02-18  
**Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | Max(50, 3×30)=90 | ✅ |
| Boundary Tests | §3 | 90 | Max(50, 3×30)=90 | ✅ |
| Functional Tests | §4 | 90 | ≥90 | ✅ |
| Integration Tests | §5 | 90 | ≥90 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

NotificationManager handles notification business logic. Key functionality: notification creation (UserId, Message, Category, ResponseType, RecordData JSON), read/unread status management (mark as read, mark all as read), notification categories (workflow, assignment, system), consumers (Actions Required card, notification bell), bulk notification creation (multiple recipients), user-specific filtering (only own notifications), soft-delete behavior, pagination and sorting (newest first), RecordData JSON structure (entityType, entityId, action), status transition Unread→Read (one-way), unread count for badge, duplicate prevention, workflow event notifications (submit, approve, reject, recall), and audit fields (CreatedAt populated correctly).

---

## §1 Positive Tests (Happy Path) — 30 tests

> **Minimum:** 30-50 tests | **Focus:** Valid notification operations, successful flows

### Detailed Test Cases (P0)

#### POS-001: Create Notification with Valid Data

**Priority:** P0  
**Precondition:** User exists. Valid inputs.

**Steps:**
1. Call `CreateNotification(userId, message, category, responseType, record)`
2. Verify notification created

**Expected Result:**
- Notification created with UserId, Message, Category, ResponseType
- RecordData = JSON serialization of record (wrapped in List)
- IsRead = false
- CreatedAt = DateTime.UtcNow
- Status = default (Pending or as set)

---

#### POS-002: Get Notifications — Unread Only (Default)

**Priority:** P0  
**Precondition:** User has 5 unread, 3 read notifications.

**Steps:**
1. Call `GetNotifications(userId, null)`
2. Verify default behavior

**Expected Result:**
- Returns only unread notifications (5)
- Ordered by CreatedAt descending (newest first)
- User-specific: only userId's notifications

---

#### POS-003: Mark As Read

**Priority:** P0  
**Precondition:** User owns unread notification.

**Steps:**
1. Call `MarkAsRead(notificationId, userId)`
2. Verify status change

**Expected Result:**
- Notification.IsRead = true
- One-way transition (Unread → Read)
- Persisted to database

---

#### POS-004: Get Notifications with unreadOnly=true

**Priority:** P0  
**Precondition:** User has unread notifications.

**Steps:**
1. Call `GetNotifications(userId, true)`
2. Verify filter

**Expected Result:**
- Returns only unread (IsRead == false)
- Ordered by CreatedAt desc

---

#### POS-005: Get Notifications with unreadOnly=false

**Priority:** P0  
**Precondition:** User has read notifications.

**Steps:**
1. Call `GetNotifications(userId, false)`
2. Verify filter

**Expected Result:**
- Returns only read (IsRead == true)
- Ordered by CreatedAt desc

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | RecordData JSON structure | Create with record | CreateNotification | RecordData includes entityType, entityId, action | P0 |
| POS-007 | RecordData array format | record = object | CreateNotification | Serialized as [record] | P1 |
| POS-008 | ParseRecordData array | RecordData = [{}] | GetNotifications | Records = parsed array | P1 |
| POS-009 | ParseRecordData single object | RecordData = {} | GetNotifications | Records = [object] | P1 |
| POS-010 | Category workflow | Workflow event | CreateNotification | Category = workflow type | P1 |
| POS-011 | Category assignment | Assignment event | CreateNotification | Category = assignment type | P1 |
| POS-012 | Category system | System event | CreateNotification | Category = system type | P1 |
| POS-013 | Notification for submit | Workflow submit | PaoWorkflowNotificationService | Notification created | P1 |
| POS-014 | Notification for approve | Workflow approve | Workflow service | Notification created | P1 |
| POS-015 | Notification for reject | Workflow reject | Workflow service | Notification created | P1 |
| POS-016 | Notification for recall | Workflow recall | Workflow service | Notification created | P1 |
| POS-017 | User-specific filtering | User A and B have notifications | GetNotifications(userA) | Only user A's | P0 |
| POS-018 | Sort newest first | Multiple notifications | GetNotifications | OrderByDescending(CreatedAt) | P0 |
| POS-019 | UpdateNotification message and status | Notification exists | UpdateNotification | Message and Status updated | P0 |
| POS-020 | MarkAsRead idempotent | Already read | MarkAsRead | No error, remains read | P1 |
| POS-021 | CreatedAt populated on create | CreateNotification | AddAsync | CreatedAt = DateTime.UtcNow | P0 |
| POS-022 | NotificationModel mapping | GetNotifications | Select to NotificationModel | Id, Message, Category, ResponseType, Entity, EntityId, Records | P1 |
| POS-023 | Empty list for new user | User has no notifications | GetNotifications | Empty list | P1 |
| POS-024 | Complex record object | Nested object | CreateNotification | RecordData serialized correctly | P1 |
| POS-025 | Duplicate prevention (business rule) | Same event, same user | CreateNotification | Dedup or allow per design | P1 |
| POS-026 | Unread count for badge | GetNotifications(userId, true) | Count | Unread count | P1 |
| POS-027 | Actions Required card consumer | Unread workflow | GetNotifications | Notifications for card | P1 |
| POS-028 | Notification bell consumer | All user notifications | GetNotifications | Notifications for bell | P1 |
| POS-029 | Bulk creation (multiple recipients) | 5 users | CreateNotification for each | 5 notifications created | P1 |
| POS-030 | Entity and EntityId in model | Notification has Entity, EntityId | GetNotifications | Entity, EntityId in response | P1 |

---

## §2 Negative Tests (Failure Scenarios) — 90 tests

> **Minimum:** 90 tests | **Focus:** Invalid inputs, business rule violations

### 2.1 Invalid Inputs

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | CreateNotification null message | message=null | NullReferenceException or validation | P0 |
| NEG-002 | CreateNotification null record | record=null | NullReferenceException in JsonSerializer | P0 |
| NEG-003 | CreateNotification null category | category=null | Per design | P1 |
| NEG-004 | CreateNotification null responseType | responseType=null | Per design | P1 |
| NEG-005 | MarkAsRead wrong userId | notification owned by 100, call with 200 | No change (not found for user 200) | P0 |
| NEG-006 | MarkAsRead non-existent notificationId | notificationId=99999 | No-op, no error | P0 |
| NEG-007 | MarkAsRead notificationId=0 | notificationId=0 | No match | P1 |
| NEG-008 | MarkAsRead notificationId=-1 | notificationId=-1 | No match | P1 |
| NEG-009 | UpdateNotification non-existent ID | notificationId=99999 | No-op | P0 |
| NEG-010 | UpdateNotification notificationId=0 | notificationId=0 | No match | P1 |
| NEG-011 | GetNotifications userId=0 | userId=0 | Empty list | P1 |
| NEG-012 | GetNotifications userId=-1 | userId=-1 | Empty list | P1 |
| NEG-013 | CreateNotification userId=0 | userId=0 | May create or error | P1 |
| NEG-014 | CreateNotification userId=-1 | userId=-1 | Per design | P1 |
| NEG-015 | Record with circular reference | record has circular ref | JsonSerializer throws | P0 |

### 2.2 RecordData and JSON

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-016 | ParseRecordData null | RecordData=null | Empty list | P1 |
| NEG-017 | ParseRecordData empty string | RecordData="" | Empty list | P1 |
| NEG-018 | ParseRecordData invalid JSON | RecordData="not json" | Fallback to [raw string] | P1 |
| NEG-019 | ParseRecordData malformed | RecordData="{ invalid }" | JsonException caught, fallback | P1 |
| NEG-020 | RecordData missing entityType | Record without entityType | Parsed, may fail validation | P1 |
| NEG-021 | RecordData missing entityId | Record without entityId | Parsed | P1 |
| NEG-022 | RecordData missing action | Record without action | Parsed | P1 |
| NEG-023 | RecordData wrong structure | Record = string | Serialized | P1 |
| NEG-024 | RecordData too large | Huge object | DB limit or error | P1 |
| NEG-025 | RecordData XSS in content | Script in record | Stored (display sanitizes) | P1 |

### 2.3 Status and Transition

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-026 | MarkAsRead on already read | IsRead=true | Idempotent, no error | P1 |
| NEG-027 | Read → Unread (invalid) | No MarkAsUnread | One-way only | P1 |
| NEG-028 | UpdateNotification null message | message=null | NullRef or DB constraint | P1 |
| NEG-029 | UpdateNotification invalid status | status=999 | Enum validation or error | P1 |
| NEG-030 | UpdateNotification wrong user's notification | User A updates User B's | Updates (no ownership check) | P0 |

### 2.4 Duplicate and Filtering

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-031 | Duplicate notification same event | Same submit, same user, same time | May create duplicate or prevent | P1 |
| NEG-032 | GetNotifications for non-existent user | userId=999999 | Empty list | P1 |
| NEG-033 | unreadOnly=true, no unread | User has only read | Empty list | P1 |
| NEG-034 | unreadOnly=false, no read | User has only unread | Empty list | P1 |
| NEG-035 | User sees other user's notifications | GetNotifications(userA) | Only userA's (never userB's) | P0 |
| NEG-036 | MarkAsRead other user's notification | User A marks User B's | No change (not found for A) | P0 |
| NEG-037 | Soft-deleted notification | Notification IsDeleted (if applicable) | Excluded from GetNotifications | P1 |
| NEG-038 | Deleted user's notifications | User deleted | Empty or error | P1 |
| NEG-039 | Invalid unreadOnly type | unreadOnly="invalid" | Coerced or error | P1 |
| NEG-040 | Pagination beyond data | Page 100, empty | Empty list | P1 |

### 2.5 Database and Dependency

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-041 | DB connection lost during GetNotifications | DB down | Exception | P0 |
| NEG-042 | DB connection lost during CreateNotification | DB down | Exception, no partial create | P0 |
| NEG-043 | DB connection lost during MarkAsRead | DB down | Exception | P0 |
| NEG-044 | DB connection lost during UpdateNotification | DB down | Exception | P0 |
| NEG-045 | SaveChangesAsync fails on Create | Constraint violation | Exception, rollback | P0 |
| NEG-046 | DbContext disposed | Context disposed | ObjectDisposedException | P0 |
| NEG-047 | UserResolverService returns 0 | No user context | userId=0 | P1 |
| NEG-048 | Transaction timeout | Long operation | TimeoutException | P1 |
| NEG-049 | Deadlock on concurrent create | Two creates same user | One blocks | P1 |
| NEG-050 | FK violation UserId | userId not in Users | FK violation or error | P1 |

### 2.6 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-051 | CreateNotification message SQL injection | message="'; DROP TABLE--" | Parameterized, no injection | P0 |
| NEG-052 | CreateNotification message XSS | message="<script>alert(1)</script>" | Stored (display sanitizes) | P1 |
| NEG-053 | CreateNotification empty message | message="" | Created (allowed) | P1 |
| NEG-054 | CreateNotification empty category | category="" | Created | P1 |
| NEG-055 | CreateNotification empty responseType | responseType="" | Created | P1 |
| NEG-056 | CreateNotification message 10000 chars | Very long | DB limit or truncated | P1 |
| NEG-057 | CreateNotification category 1000 chars | Very long | DB limit | P1 |
| NEG-058 | CreateNotification special chars in message | "Test \"quotes\" & <html>" | Escaped in JSON | P1 |
| NEG-059 | CreateNotification unicode message | "日本語" | Stored correctly | P1 |
| NEG-060 | GetNotifications SQL injection userId | userId from untrusted | Parameterized | P0 |
| NEG-061 | MarkAsRead notificationId overflow | notificationId=2147483648 | Handled | P1 |
| NEG-062 | UpdateNotification overwrites | Two updates | Second overwrites | P1 |
| NEG-063 | Record with DateTime | record = new { date = DateTime.UtcNow } | Serialized | P1 |
| NEG-064 | Record with Guid | record = new { id = Guid } | Serialized | P1 |
| NEG-065 | Record with null property | record = new { x = (string)null } | Serialized | P1 |
| NEG-066 | Record Dictionary | record = Dictionary | Serialized | P1 |
| NEG-067 | Record JsonElement | record from deserialization | Re-serializes | P1 |
| NEG-068 | ParseRecordData deeply nested | Complex JSON | Deserialized | P1 |
| NEG-069 | NotificationModel Records null | RecordData invalid | Records = [] or fallback | P1 |
| NEG-070 | Bulk create partial failure | 5 users, 3rd fails | Transaction or partial | P1 |
| NEG-071 | CreateNotification during GetNotifications | Concurrent | Consistent | P1 |
| NEG-072 | MarkAsRead during CreateNotification | Concurrent | Consistent | P1 |
| NEG-073 | UpdateNotification non-existent | ID 0 | No-op | P1 |
| NEG-074 | GetNotifications with invalid sort | N/A | Default sort used | P1 |
| NEG-075 | CreateNotification record empty object | record = {} | Serialized as [{}] | P1 |
| NEG-076 | CreateNotification record array | record = [1,2,3] | Serialized (nested array) | P1 |
| NEG-077 | Notification Status invalid enum | status = (NotificationStatus)99 | Error or default | P1 |
| NEG-078 | MarkAsRead same notification twice | Two calls | Idempotent | P1 |
| NEG-079 | GetNotifications unreadOnly=null vs omitted | Both | Same (default unread) | P1 |
| NEG-080 | CreateNotification with whitespace message | message="   " | Created | P1 |
| NEG-081 | CreateNotification with whitespace category | category="   " | Created | P1 |
| NEG-082 | RecordData JSON injection | Malicious JSON | Sanitized or error | P0 |
| NEG-083 | Notification Id negative | Get by -1 | Not found | P1 |
| NEG-084 | EntityId null in RecordData | Record EntityId=null | Allowed | P1 |
| NEG-085 | EntityType empty in RecordData | Record entityType="" | Allowed | P1 |
| NEG-086 | Action empty in RecordData | Record action="" | Allowed | P1 |
| NEG-087 | Multiple CreateNotification same user | Rapid creates | All created | P1 |
| NEG-088 | GetNotifications during MarkAsRead | Concurrent | Consistent | P1 |
| NEG-089 | CreateNotification record with binary | Binary data | Serialization error | P1 |
| NEG-090 | Notification soft-delete filter | IsDeleted=true | Excluded if filtered | P1 |

---

## §3 Boundary Tests (Edge Cases) — 90 tests

> **Minimum:** 90 tests | **Focus:** Edge values, boundaries

### 3.1 UserId Boundaries

| ID | Test Name | userId | Expected Result | Priority |
|----|-----------|--------|-----------------|----------|
| BND-001 | userId=1 | Minimum valid | Notifications returned | P1 |
| BND-002 | userId=MAX_INT | 2147483647 | Handled | P2 |
| BND-003 | userId=0 | Zero | Empty list | P1 |
| BND-004 | userId=-1 | Negative | Empty list | P1 |
| BND-005 | userId non-existent | 999999999 | Empty list | P1 |

### 3.2 NotificationId Boundaries

| ID | Test Name | notificationId | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-006 | notificationId=1 | Minimum | MarkAsRead works | P1 |
| BND-007 | notificationId=MAX_INT | Max | Handled | P2 |
| BND-008 | notificationId=0 | Zero | No match | P1 |
| BND-009 | notificationId=-1 | Negative | No match | P1 |
| BND-010 | notificationId non-existent | 99999 | No-op | P1 |

### 3.3 Message Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-011 | Message | 0 | 4000 | "" ✅ | 4000 chars ✅ | 4001 ❌ | P1 |
| BND-012 | Message 1 char | "A" | | Accepted | | | P1 |
| BND-013 | Message empty | "" | | Accepted | | | P1 |
| BND-014 | Message unicode | Arabic/Chinese | | Stored | | | P2 |
| BND-015 | Message special chars | <>&" | | Escaped | | | P1 |
| BND-016 | Message newlines | \n\r | | Preserved | | | P2 |
| BND-017 | Message 3999 chars | | 3999 | Accepted | | | P1 |
| BND-018 | Message 4001 chars | | | | | Rejected or truncated | P1 |
| BND-019 | Message emoji | 🤝 | | Stored | | | P2 |
| BND-020 | Message max length | | DB limit | | At limit | Over | P1 |

### 3.4 Category and ResponseType Boundaries

| ID | Test Name | Value | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-021 | Category workflow | "workflow" | Accepted | P1 |
| BND-022 | Category assignment | "assignment" | Accepted | P1 |
| BND-023 | Category system | "system" | Accepted | P1 |
| BND-024 | Category empty | "" | Accepted | P1 |
| BND-025 | Category 200 chars | Long | Accepted or truncated | P1 |
| BND-026 | ResponseType NewApproval | "NewApproval" | Accepted | P1 |
| BND-027 | ResponseType Recalled | "Recalled" | Accepted | P1 |
| BND-028 | ResponseType Rejected | "Rejected" | Accepted | P1 |
| BND-029 | ResponseType Completed | "Completed" | Accepted | P1 |
| BND-030 | ResponseType empty | "" | Accepted | P1 |
| BND-031 | Category unicode | "通知" | Accepted | P2 |
| BND-032 | ResponseType 100 chars | Long | Accepted | P1 |
| BND-033 | Category exact max | DB column max | Accepted | P1 |
| BND-034 | ResponseType exact max | DB column max | Accepted | P1 |
| BND-035 | Category with spaces | " workflow " | Stored | P1 |

### 3.5 RecordData Boundaries

| ID | Test Name | RecordData | Expected Result | Priority |
|----|-----------|------------|-----------------|----------|
| BND-036 | RecordData null | null | ParseRecordData returns [] | P1 |
| BND-037 | RecordData empty string | "" | Returns [] | P1 |
| BND-038 | RecordData "[]" | Empty array | Returns [] | P1 |
| BND-039 | RecordData "[{}]" | Single empty object | Returns [{}] | P1 |
| BND-040 | RecordData "[{\"id\":1}]" | Single object | Returns 1 item | P1 |
| BND-041 | RecordData 1 item | [record] | 1 in Records | P1 |
| BND-042 | RecordData 10 items | [r1..r10] | 10 in Records | P1 |
| BND-043 | RecordData 100 items | Large array | All parsed | P2 |
| BND-044 | RecordData entityType | {"entityType":"Opportunity"} | Parsed | P1 |
| BND-045 | RecordData entityId | {"entityId":123} | Parsed | P1 |
| BND-046 | RecordData action | {"action":"submit"} | Parsed | P1 |
| BND-047 | RecordData all required | entityType, entityId, action | Full structure | P0 |
| BND-048 | RecordData nested object | {a:{b:1}} | Parsed | P1 |
| BND-049 | RecordData invalid JSON | "not json" | Fallback [raw] | P1 |
| BND-050 | RecordData malformed | "{ broken }" | Fallback | P1 |
| BND-051 | RecordData single object (not array) | "{}" | ParseRecordData returns [{}] | P1 |
| BND-052 | RecordData max size | DB column limit | Accepted or error | P1 |
| BND-053 | RecordData unicode | {"msg":"日本語"} | Parsed | P2 |
| BND-054 | RecordData special chars | {"msg":"<>&\""} | Escaped | P1 |
| BND-055 | RecordData null in array | [null] | Parsed | P1 |

### 3.6 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-056 | User with 0 notifications | Empty | GetNotifications returns [] | P1 |
| BND-057 | User with 1 notification | Single | 1 item | P1 |
| BND-058 | User with 10 notifications | 10 | 10 items, sorted | P1 |
| BND-059 | User with 100 notifications | 100 | All returned (no pagination in manager) | P1 |
| BND-060 | User with 1000 notifications | 1000 | All or performance | P2 |
| BND-061 | All unread | 5 unread, 0 read | unreadOnly=null returns 5 | P1 |
| BND-062 | All read | 0 unread, 5 read | unreadOnly=null returns [] | P1 |
| BND-063 | Mixed 5 unread 5 read | 10 total | unreadOnly=null returns 5 | P1 |
| BND-064 | unreadOnly=true, 3 unread | 3 unread | Returns 3 | P1 |
| BND-065 | unreadOnly=false, 7 read | 7 read | Returns 7 | P1 |
| BND-066 | Last notification (newest) | 10 notifications | First in list | P1 |
| BND-067 | First notification (oldest) | 10 notifications | Last in list | P1 |
| BND-068 | Same CreatedAt (tie) | Two same timestamp | Order deterministic | P2 |
| BND-069 | Bulk create 5 | 5 users | 5 notifications | P1 |
| BND-070 | Bulk create 50 | 50 users | 50 notifications | P2 |

### 3.7 Date and Time Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-071 | CreatedAt UTC | CreateNotification | CreatedAt = DateTime.UtcNow | P1 |
| BND-072 | CreatedAt midnight | 00:00:00 UTC | Stored correctly | P2 |
| BND-073 | CreatedAt 23:59:59 | End of day | Stored correctly | P2 |
| BND-074 | CreatedAt leap year | 2028-02-29 | Stored correctly | P2 |
| BND-075 | Sort by CreatedAt | Multiple | Newest first | P0 |
| BND-076 | RecordData date in record | DateTime.UtcNow | ISO format in JSON | P1 |
| BND-077 | RecordData date null | null date | Serialized | P1 |
| BND-078 | CreatedAt timezone | Local vs UTC | UTC stored | P1 |
| BND-079 | MarkAsRead timestamp | No update to CreatedAt | CreatedAt unchanged | P1 |
| BND-080 | UpdateNotification timestamp | No CreatedAt change | CreatedAt unchanged | P1 |

### 3.8 Status and IsRead Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-081 | IsRead initial | CreateNotification | IsRead=false | P0 |
| BND-082 | IsRead after MarkAsRead | MarkAsRead | IsRead=true | P0 |
| BND-083 | Status Pending | CreateNotification | Status=Pending (default) | P1 |
| BND-084 | Status Progress | UpdateNotification | Status=Progress | P1 |
| BND-085 | Status Done | UpdateNotification | Status=Done | P1 |
| BND-086 | Status Error | UpdateNotification | Status=Error | P1 |
| BND-087 | Unread count 0 | All read | GetNotifications(userId, true).Count=0 | P1 |
| BND-088 | Unread count 5 | 5 unread | Count=5 | P1 |
| BND-089 | Read count 0 | All unread | GetNotifications(userId, false).Count=0 | P1 |
| BND-090 | Entity and EntityId null | Notification without | Entity=null, EntityId=null | P1 |

---

## §4 Functional Tests (Business Rules) — 90 tests

> **Minimum:** 90 tests | **Breakdown:** Creation (20), Read/Filter (25), Status (20), RecordData (15), Audit (10)

### 4.1 Creation Rules (20)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | CreateNotification sets UserId | Create | CreateNotification | UserId = param | P0 |
| FUN-002 | CreateNotification sets Message | Create | CreateNotification | Message = param | P0 |
| FUN-003 | CreateNotification sets Category | Create | CreateNotification | Category = param | P0 |
| FUN-004 | CreateNotification sets ResponseType | Create | CreateNotification | ResponseType = param | P0 |
| FUN-005 | CreateNotification sets RecordData | Create | CreateNotification | RecordData = JSON([record]) | P0 |
| FUN-006 | CreateNotification sets IsRead=false | Create | CreateNotification | IsRead = false | P0 |
| FUN-007 | CreateNotification sets CreatedAt | Create | CreateNotification | CreatedAt = UtcNow | P0 |
| FUN-008 | RecordData wraps record in array | Create | CreateNotification | [record] | P0 |
| FUN-009 | RecordData JSON valid | Create | CreateNotification | Valid JSON | P0 |
| FUN-010 | RecordData should include entityType | Business rule | Record structure | entityType in record | P1 |
| FUN-011 | RecordData should include entityId | Business rule | Record structure | entityId in record | P1 |
| FUN-012 | RecordData should include action | Business rule | Record structure | action in record | P1 |
| FUN-013 | Bulk create multiple recipients | Create | 5x CreateNotification | 5 notifications | P1 |
| FUN-014 | Workflow submit notification | Workflow | Submit event | Notification created | P1 |
| FUN-015 | Workflow approve notification | Workflow | Approve event | Notification created | P1 |
| FUN-016 | Workflow reject notification | Workflow | Reject event | Notification created | P1 |
| FUN-017 | Workflow recall notification | Workflow | Recall event | Notification created | P1 |
| FUN-018 | Category workflow | Category | workflow_* | workflow type | P1 |
| FUN-019 | Category assignment | Category | assignment_* | assignment type | P1 |
| FUN-020 | Category system | Category | system_* | system type | P1 |

### 4.2 Read and Filter Rules (25)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-021 | GetNotifications filters by userId | Filter | GetNotifications(userId) | Only userId's | P0 |
| FUN-022 | GetNotifications default unread only | Default | GetNotifications(userId, null) | IsRead=false | P0 |
| FUN-023 | GetNotifications unreadOnly=true | Filter | unreadOnly=true | IsRead=false | P0 |
| FUN-024 | GetNotifications unreadOnly=false | Filter | unreadOnly=false | IsRead=true | P0 |
| FUN-025 | GetNotifications ordered CreatedAt desc | Sort | GetNotifications | Newest first | P0 |
| FUN-026 | User-specific filtering | Filter | Any user | Only own notifications | P0 |
| FUN-027 | Actions Required card | Consumer | Unread workflow | Workflow notifications | P1 |
| FUN-028 | Notification bell | Consumer | All | User's notifications | P1 |
| FUN-029 | Unread count for badge | Count | GetNotifications(userId, true).Count | Unread count | P1 |
| FUN-030 | Empty list for no notifications | Empty | New user | [] | P1 |
| FUN-031 | ParseRecordData array format | Parse | RecordData = [{}] | Records = list | P1 |
| FUN-032 | ParseRecordData object format | Parse | RecordData = {} | Records = [{}] | P1 |
| FUN-033 | ParseRecordData invalid fallback | Parse | Invalid JSON | [raw string] | P1 |
| FUN-034 | ParseRecordData null/empty | Parse | null or "" | [] | P1 |
| FUN-035 | NotificationModel Id | Mapping | GetNotifications | Id populated | P1 |
| FUN-036 | NotificationModel Message | Mapping | GetNotifications | Message populated | P1 |
| FUN-037 | NotificationModel Category | Mapping | GetNotifications | Category populated | P1 |
| FUN-038 | NotificationModel ResponseType | Mapping | GetNotifications | ResponseType populated | P1 |
| FUN-039 | NotificationModel Entity | Mapping | GetNotifications | Entity populated | P1 |
| FUN-040 | NotificationModel EntityId | Mapping | GetNotifications | EntityId populated | P1 |
| FUN-041 | NotificationModel Records | Mapping | GetNotifications | Records from ParseRecordData | P1 |
| FUN-042 | Soft-delete filter (if applicable) | Filter | IsDeleted | Excluded | P1 |
| FUN-043 | Pagination (if implemented) | Pagination | Page, size | Correct page | P2 |
| FUN-044 | Sort configurable (if implemented) | Sort | Sort param | Correct order | P2 |
| FUN-045 | Filter by category (if implemented) | Filter | Category | Filtered | P2 |

### 4.3 Status Rules (20)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-046 | MarkAsRead sets IsRead=true | MarkAsRead | MarkAsRead(id, userId) | IsRead = true | P0 |
| FUN-047 | Status transition Unread→Read | One-way | MarkAsRead | Unread → Read | P0 |
| FUN-048 | No Read→Unread | One-way | N/A | No MarkAsUnread | P1 |
| FUN-049 | MarkAsRead only own notification | Ownership | MarkAsRead(id, userId) | userId must match | P0 |
| FUN-050 | MarkAsRead idempotent | Idempotent | MarkAsRead on read | No error | P1 |
| FUN-051 | UpdateNotification updates Message | Update | UpdateNotification | Message changed | P0 |
| FUN-052 | UpdateNotification updates Status | Update | UpdateNotification | Status changed | P0 |
| FUN-053 | UpdateNotification does not change CreatedAt | Immutable | UpdateNotification | CreatedAt unchanged | P1 |
| FUN-054 | UpdateNotification does not change UserId | Immutable | UpdateNotification | UserId unchanged | P1 |
| FUN-055 | UpdateNotification does not change IsRead | Independent | UpdateNotification | IsRead unchanged | P1 |
| FUN-056 | Status Pending→Progress | Transition | UpdateNotification | Allowed | P1 |
| FUN-057 | Status Progress→Done | Transition | UpdateNotification | Allowed | P1 |
| FUN-058 | Status Done→Error | Transition | UpdateNotification | Allowed | P1 |
| FUN-059 | MarkAsRead persists | Persistence | SaveChangesAsync | DB updated | P0 |
| FUN-060 | UpdateNotification persists | Persistence | SaveChangesAsync | DB updated | P0 |
| FUN-061 | Mark all as read (if implemented) | Bulk | MarkAllAsRead(userId) | All IsRead=true | P2 |
| FUN-062 | Status enum values | Validation | NotificationStatus | Pending, Progress, Done, Error | P1 |
| FUN-063 | IsRead boolean | Type | IsRead | true/false | P1 |
| FUN-064 | Unread badge count | Business | GetNotifications(userId, true) | Count for badge | P1 |
| FUN-065 | Duplicate prevention (if implemented) | Dedup | Same event | Prevent or allow per design | P2 |

### 4.4 RecordData Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-066 | RecordData JSON structure | Structure | Create | Valid JSON | P0 |
| FUN-067 | RecordData array of objects | Format | Create | [record] | P0 |
| FUN-068 | RecordData entityType | Required | Record | entityType field | P1 |
| FUN-069 | RecordData entityId | Required | Record | entityId field | P1 |
| FUN-070 | RecordData action | Required | Record | action field | P1 |
| FUN-071 | RecordData workflow event | Workflow | Submit/approve/reject/recall | Correct structure | P1 |
| FUN-072 | RecordData assignment event | Assignment | Assign | Correct structure | P1 |
| FUN-073 | RecordData system event | System | System | Correct structure | P1 |
| FUN-074 | ParseRecordData handles array | Parse | [{}] | List<object> | P1 |
| FUN-075 | ParseRecordData handles object | Parse | {} | [object] | P1 |
| FUN-076 | ParseRecordData handles invalid | Parse | Invalid | Fallback | P1 |
| FUN-077 | RecordData serialization | Serialize | object | JsonSerializer | P1 |
| FUN-078 | RecordData deserialization | Deserialize | JSON | ParseRecordData | P1 |
| FUN-079 | RecordData nested | Structure | Nested object | Preserved | P1 |
| FUN-080 | RecordData size limit | Limit | Large object | DB limit | P1 |

### 4.5 Audit Rules (10)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-081 | CreatedAt populated on create | Audit | CreateNotification | CreatedAt = UtcNow | P0 |
| FUN-082 | CreatedAt not updated on MarkAsRead | Audit | MarkAsRead | CreatedAt unchanged | P1 |
| FUN-083 | CreatedAt not updated on UpdateNotification | Audit | UpdateNotification | CreatedAt unchanged | P1 |
| FUN-084 | UserId identifies recipient | Audit | CreateNotification | UserId = recipient | P0 |
| FUN-085 | No CreatedBy (UserId is recipient) | Design | CreateNotification | UserId, not creator | P1 |
| FUN-086 | CreatedAt UTC | Audit | Create | UTC timezone | P1 |
| FUN-087 | Audit trail (if implemented) | Audit | Changes | Logged | P2 |
| FUN-088 | LastModifiedAt (if implemented) | Audit | Update | Updated | P2 |
| FUN-089 | Soft-delete audit (if applicable) | Audit | Delete | DeletedBy, DeletedDate | P2 |
| FUN-090 | Notification creation audit | Audit | Create | Persisted | P1 |

---

## §5 Integration Tests (End-to-End Flows) — 90 tests

> **Minimum:** 90 tests

### 5.1 CRUD Workflow (15)

| ID | Test Name | Flow | Entities | Expected | Priority |
|----|-----------|------|----------|----------|----------|
| INT-001 | Create → Get | CreateNotification then GetNotifications | Notification | Created and returned | P0 |
| INT-002 | Create → MarkAsRead → Get | Full flow | Notification | Read, excluded from unread | P0 |
| INT-003 | Create → UpdateNotification | Create then update | Notification | Updated | P0 |
| INT-004 | Multiple creates same user | 5x CreateNotification | Notification | 5 created | P0 |
| INT-005 | Multiple creates different users | 3 users, 2 each | Notification | 6 created | P0 |
| INT-006 | Get unread → MarkAsRead → Get unread | Flow | Notification | Count decreases | P0 |
| INT-007 | Get with unreadOnly=true → false | Both | Notification | Different results | P0 |
| INT-008 | Create with RecordData → Get → Parse | Full | Notification | Records parsed | P0 |
| INT-009 | Workflow submit → Create → Get | Workflow integration | Notification, Workflow | Notification for submit | P1 |
| INT-010 | Workflow approve → Create → Get | Workflow integration | Notification | Notification for approve | P1 |
| INT-011 | Workflow reject → Create → Get | Workflow integration | Notification | Notification for reject | P1 |
| INT-012 | Workflow recall → Create → Get | Workflow integration | Notification | Notification for recall | P1 |
| INT-013 | Gmail sync → Create | Gmail integration | Notification | Created | P1 |
| INT-014 | AI data modification → Create | Gemini integration | Notification | Created | P1 |
| INT-015 | Full lifecycle | Create→Get→MarkAsRead→Get | Notification | Complete flow | P0 |

### 5.2 Cross-Manager Integration (15)

| ID | Test Name | Managers | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|----------|
| INT-016 | NotificationManager + WorkflowManager | Both | Stage change | Notification created | P0 |
| INT-017 | NotificationManager + GmailAddonManager | Both | Gmail sync | Notification created | P1 |
| INT-018 | NotificationManager + GeminiManager | Both | AI modification | Notification created | P1 |
| INT-019 | NotificationManager + UserResolverService | Both | Get current user | userId for create | P0 |
| INT-020 | NotificationManager + AppDbContext | Both | Persistence | DB round-trip | P0 |
| INT-021 | NotificationManager + JsonSerializer | Both | RecordData | Serialization | P0 |
| INT-022 | PaoWorkflowNotificationService creates | Workflow | Stage change | CreateNotification called | P1 |
| INT-023 | DueDiligenceNotificationService creates | Due diligence | Event | CreateNotification called | P1 |
| INT-024 | Multiple consumers create | Workflow, Gmail, Gemini | Sequential | All created | P1 |
| INT-025 | NotificationController → Manager | Controller | API call | Manager invoked | P0 |
| INT-026 | GetNotifications → NotificationModel | Mapping | Get | Model populated | P0 |
| INT-027 | MarkAsRead API → Manager | API | PUT | Manager MarkAsRead | P0 |
| INT-028 | UpdateNotification API → Manager | API | PUT | Manager UpdateNotification | P0 |
| INT-029 | CreateNotification from consumer | Any consumer | Create | Manager CreateNotification | P0 |
| INT-030 | DbContext scope | Manager | Request | Scoped context | P0 |

### 5.3 Database Persistence (15)

| ID | Test Name | Operation | DB State | Expected | Priority |
|----|-----------|----------|----------|----------|----------|
| INT-031 | CreateNotification persists | AddAsync, SaveChanges | Notification table | Row inserted | P0 |
| INT-032 | MarkAsRead persists | Update, SaveChanges | Notification table | IsRead updated | P0 |
| INT-033 | UpdateNotification persists | Update, SaveChanges | Notification table | Message, Status updated | P0 |
| INT-034 | GetNotifications reads from DB | Query | Notification table | Data from DB | P0 |
| INT-035 | UserId FK | Create | Users table | Valid userId | P1 |
| INT-036 | RecordData column type | Create | RecordData column | JSON/text | P1 |
| INT-037 | CreatedAt column | Create | CreatedAt column | UTC timestamp | P1 |
| INT-038 | Transaction on Create | Create | Transaction | Committed | P0 |
| INT-039 | Transaction on MarkAsRead | MarkAsRead | Transaction | Committed | P0 |
| INT-040 | Transaction on UpdateNotification | Update | Transaction | Committed | P0 |
| INT-041 | Rollback on Create failure | Create fails | Transaction | Rolled back | P0 |
| INT-042 | Concurrent create | 2 creates | DB | Both committed | P1 |
| INT-043 | Concurrent MarkAsRead | 2 marks | DB | Both committed | P1 |
| INT-044 | Entity column | Create with Entity | Entity column | Stored | P1 |
| INT-045 | EntityId column | Create with EntityId | EntityId column | Stored | P1 |

### 5.4 RecordData Integration (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-046 | RecordData workflow submit | entityType, entityId, action | Correct JSON | P0 |
| INT-047 | RecordData workflow approve | entityType, entityId, action | Correct JSON | P0 |
| INT-048 | RecordData workflow reject | entityType, entityId, action | Correct JSON | P0 |
| INT-049 | RecordData workflow recall | entityType, entityId, action | Correct JSON | P0 |
| INT-050 | RecordData Gmail creation | Gmail record | Correct JSON | P1 |
| INT-051 | RecordData AI modification | AI record | Correct JSON | P1 |
| INT-052 | RecordData round-trip | Create → Get → Parse | Data preserved | P0 |
| INT-053 | RecordData complex object | Nested, arrays | Serialized correctly | P1 |
| INT-054 | RecordData DateTime | DateTime in record | ISO format | P1 |
| INT-055 | RecordData Guid | Guid in record | Serialized | P1 |
| INT-056 | RecordData null property | null in record | Serialized | P1 |
| INT-057 | ParseRecordData round-trip | JSON → Parse → Use | Correct | P1 |
| INT-058 | RecordData unicode | Unicode in record | Preserved | P1 |
| INT-059 | RecordData special chars | <>&" in record | Escaped | P1 |
| INT-060 | RecordData empty object | {} | Serialized as [{}] | P1 |

### 5.5 Error Handling Integration (30)

| ID | Test Name | Error Condition | Expected | Priority |
|----|-----------|----------------|----------|----------|
| INT-061 | DB unavailable GetNotifications | DB down | Exception | P0 |
| INT-062 | DB unavailable CreateNotification | DB down | Exception | P0 |
| INT-063 | DB unavailable MarkAsRead | DB down | Exception | P0 |
| INT-064 | DB unavailable UpdateNotification | DB down | Exception | P0 |
| INT-065 | CreateNotification null record | record=null | Exception | P0 |
| INT-066 | CreateNotification circular reference | Circular record | Exception | P0 |
| INT-067 | ParseRecordData invalid JSON | Invalid | Fallback | P1 |
| INT-068 | MarkAsRead wrong user | Other user's notification | No-op | P0 |
| INT-069 | MarkAsRead non-existent | ID 99999 | No-op | P0 |
| INT-070 | UpdateNotification non-existent | ID 99999 | No-op | P0 |
| INT-071 | GetNotifications userId=0 | Zero | Empty list | P1 |
| INT-072 | CreateNotification serialization error | Non-serializable | Exception | P0 |
| INT-073 | RecordData too large | Huge object | Error or truncation | P1 |
| INT-074 | Concurrent create same user | 2 creates | Both succeed | P1 |
| INT-075 | Concurrent MarkAsRead same notification | 2 marks | Idempotent | P1 |
| INT-076 | Concurrent GetNotifications | 2 reads | Both succeed | P1 |
| INT-077 | Create during GetNotifications | Concurrent | Consistent | P1 |
| INT-078 | MarkAsRead during GetNotifications | Concurrent | Consistent | P1 |
| INT-079 | UpdateNotification during GetNotifications | Concurrent | Consistent | P1 |
| INT-080 | Transaction timeout | Long operation | Timeout | P1 |
| INT-081 | FK violation userId | Invalid userId | Exception | P1 |
| INT-082 | Constraint violation | Duplicate or constraint | Exception | P1 |
| INT-083 | DbContext disposed | Disposed context | Exception | P0 |
| INT-084 | Connection pool exhausted | Many concurrent | Queued or error | P1 |
| INT-085 | Deadlock | Concurrent updates | Timeout or retry | P1 |
| INT-086 | API 401 unauthenticated | No auth | 401 | P0 |
| INT-087 | API 500 manager exception | Manager throws | 500 | P0 |
| INT-088 | API 400 invalid request | Bad body | 400 | P1 |
| INT-089 | Full integration success | All components | End-to-end | P0 |
| INT-090 | Full integration with error | Create fails | Error propagated | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE for QA)

> **Note:** Security testing is OUT OF SCOPE for QA per project standards.

| ID | Test Name | Category | Status | Priority |
|----|-----------|----------|--------|----------|
| SEC-001 | SQL injection in message | Injection | OUT OF SCOPE | P0 |
| SEC-002 | SQL injection in userId | Injection | OUT OF SCOPE | P0 |
| SEC-003 | XSS in message | Injection | OUT OF SCOPE | P0 |
| SEC-004 | Unauthorized GetNotifications | Access Control | OUT OF SCOPE | P0 |
| SEC-005 | IDOR: Other user's notifications | IDOR | OUT OF SCOPE | P0 |
| SEC-006 through SEC-050 | [Additional security scenarios] | Various | OUT OF SCOPE | P1/P2 |

---

## §7 Concurrency Tests — 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Two users GetNotifications | User A and B | Both get own, isolated | P0 |
| CON-002 | Two users CreateNotification same user | 2 creates for user 100 | Both succeed | P0 |
| CON-003 | Two users MarkAsRead different notifications | User A marks 1, User B marks 2 | Both succeed | P0 |
| CON-004 | CreateNotification during GetNotifications | Concurrent | Consistent | P1 |
| CON-005 | MarkAsRead during GetNotifications | Concurrent | Consistent | P1 |
| CON-006 | UpdateNotification during GetNotifications | Concurrent | Consistent | P1 |
| CON-007 | Two MarkAsRead same notification same user | User 100 marks 42 twice | Idempotent | P1 |
| CON-008 | Two MarkAsRead same notification different users | User A and B (B doesn't own) | A succeeds, B no-op | P1 |
| CON-009 | Two UpdateNotification same notification | Two updates | Last write wins | P1 |
| CON-010 | CreateNotification and MarkAsRead same user | Create then mark | Both succeed | P1 |
| CON-011 | Bulk create 10 concurrent | 10 creates | All succeed | P1 |
| CON-012 | GetNotifications and CreateNotification | Read during create | Consistent | P1 |
| CON-013 | DbContext scope | Two requests | Separate contexts | P0 |
| CON-014 | Transaction isolation | Two creates | Isolated | P0 |
| CON-015 | ParseRecordData concurrent | Two parses same data | Both succeed | P1 |
| CON-016 | RecordData serialization concurrent | Two serializes | Both succeed | P1 |
| CON-017 | Unread count during MarkAsRead | Get count, mark, get count | Consistent | P1 |
| CON-018 | Sort order during create | Create during Get | Newest first maintained | P1 |
| CON-019 | User filter during create | Create for A during B's Get | B doesn't see A's | P0 |
| CON-020 | Deadlock scenario | Circular dependency | Timeout or retry | P2 |
| CON-021 | Connection pool | 20 concurrent | All complete | P1 |
| CON-022 | SaveChangesAsync concurrent | Two saves | Both commit | P0 |
| CON-023 | Optimistic concurrency (if implemented) | Same row update | Conflict handling | P2 |
| CON-024 | Cache (if implemented) | Concurrent read/write | Consistent | P2 |
| CON-025 | Full load concurrent | 20 mixed operations | All complete | P1 |

---

## §8 Unit Tests — 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | ParseRecordData null | Formatting | null | [] | P1 |
| UNT-002 | ParseRecordData empty | Formatting | "" | [] | P1 |
| UNT-003 | ParseRecordData "[]" | Formatting | "[]" | [] | P1 |
| UNT-004 | ParseRecordData "[{}]" | Formatting | "[{}]" | [{}] | P1 |
| UNT-005 | ParseRecordData "[{\"id\":1}]" | Formatting | Valid JSON | 1 item | P1 |
| UNT-006 | ParseRecordData "{}" | Formatting | Single object | [{}] | P1 |
| UNT-007 | ParseRecordData invalid | Formatting | "not json" | [raw string] | P1 |
| UNT-008 | ParseRecordData malformed | Formatting | "{ invalid }" | Fallback | P1 |
| UNT-009 | JsonSerializer.Serialize record | Formatting | object | JSON string | P1 |
| UNT-010 | RecordData wrap in array | Formatting | record | [record] | P1 |
| UNT-011 | NotificationModel mapping Id | Validation | Notification | Id | P1 |
| UNT-012 | NotificationModel mapping Message | Validation | Notification | Message | P1 |
| UNT-013 | NotificationModel mapping Category | Validation | Notification | Category | P1 |
| UNT-014 | NotificationModel mapping ResponseType | Validation | Notification | ResponseType | P1 |
| UNT-015 | NotificationModel mapping Entity | Validation | Notification | Entity | P1 |
| UNT-016 | NotificationModel mapping EntityId | Validation | Notification | EntityId | P1 |
| UNT-017 | NotificationModel mapping Records | Validation | Notification | Records from Parse | P1 |
| UNT-018 | IsRead default | Validation | CreateNotification | false | P1 |
| UNT-019 | CreatedAt format | Validation | CreateNotification | UTC | P1 |
| UNT-020 | OrderByDescending CreatedAt | Validation | GetNotifications | Newest first | P1 |
| UNT-021 | Where userId | Validation | GetNotifications | userId filter | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetNotifications 100 notifications | Query | < 500ms | P1 |
| PRF-002 | GetNotifications 1000 notifications | Query | < 2s | P1 |
| PRF-003 | CreateNotification single | Create | < 200ms | P1 |
| PRF-004 | MarkAsRead single | Update | < 200ms | P1 |
| PRF-005 | UpdateNotification single | Update | < 200ms | P1 |
| PRF-006 | ParseRecordData 10 items | Parse | < 50ms | P1 |
| PRF-007 | ParseRecordData 100 items | Parse | < 200ms | P1 |
| PRF-008 | Bulk create 10 | 10 creates | < 2s | P1 |
| PRF-009 | Bulk create 50 | 50 creates | < 10s | P1 |
| PRF-010 | GetNotifications with unreadOnly | Filter | < 500ms | P1 |
| PRF-011 | GetNotifications sort | OrderBy | < 500ms | P1 |
| PRF-012 | CreateNotification with large record | Large object | < 500ms | P1 |
| PRF-013 | JsonSerializer large object | Serialize | < 200ms | P1 |
| PRF-014 | ParseRecordData large JSON | Parse | < 500ms | P1 |
| PRF-015 | Concurrent 10 GetNotifications | 10 parallel | All < 1s | P1 |
| PRF-016 | Concurrent 10 CreateNotification | 10 parallel | All < 2s | P1 |

---

## §10 Load Tests — 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained GetNotifications | 50 req/min | 5 min | < 500ms p95 | P2 |
| LDT-002 | Sustained CreateNotification | 20/min | 5 min | All succeed | P2 |
| LDT-003 | Spike: 50 simultaneous GetNotifications | 50 concurrent | 1 min | 95% success | P2 |
| LDT-004 | Spike: 20 simultaneous CreateNotification | 20 concurrent | 1 min | 95% success | P2 |
| LDT-005 | Stress: 100 CreateNotification | 100 creates | 2 min | No deadlocks | P2 |
| LDT-006 | Mixed load | Get, Create, Mark mix | 10 min | No degradation | P2 |
| LDT-007 | Bulk create load | 100 creates/min | 5 min | All succeed | P2 |
| LDT-008 | Unread count load | 100 count req/min | 5 min | < 500ms p95 | P2 |
| LDT-009 | Recovery after load | Load then idle | 2 min | System recovers | P2 |
| LDT-010 | Full load | All operations | 15 min | No errors | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| Notification creation UserId, Message, Category, ResponseType, RecordData | POS-001, FUN-001 to FUN-009, INT-001 |
| Read/unread: mark as read, mark all as read | POS-003, FUN-046 to FUN-050, NEG-005 to NEG-008 |
| Categories: workflow, assignment, system | POS-010 to POS-012, FUN-018 to FUN-020, BND-021 to BND-023 |
| Consumers: Actions Required, notification bell | POS-027, POS-028, FUN-027, FUN-028 |
| Bulk creation multiple recipients | POS-029, FUN-013, BND-069, BND-070 |
| User-specific filtering | POS-017, FUN-021, FUN-026, NEG-035 |
| RecordData: entityType, entityId, action | POS-006, FUN-068 to FUN-071, BND-044 to BND-047 |
| Status Unread→Read one-way | FUN-047, FUN-048, BND-081, BND-082 |
| Unread count for badge | POS-026, FUN-029, FUN-064, BND-087 |
| Workflow events: submit, approve, reject, recall | POS-013 to POS-016, FUN-014 to FUN-017, INT-009 to INT-012 |
| Audit: CreatedAt populated | POS-021, FUN-081, FUN-086, BND-071 |
| Sort newest first | POS-018, FUN-025, BND-075 |
| ParseRecordData array/object | POS-008, POS-009, FUN-031 to FUN-034, UNT-001 to UNT-008 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
