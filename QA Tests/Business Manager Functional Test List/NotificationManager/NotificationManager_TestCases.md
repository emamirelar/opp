# NotificationManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/NotificationManager`  
**Created:** 2026-02-18 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30-50 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Concurrency (CON) | 25 | 25 | ✅ |
| §7 Unit (UNT) | 21 | 21 | ✅ |
| §8 Performance (PRF) | 16 | 16 | ✅ |
| §9 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Formula |
|-------|--------|---------|
| N≥3P? | ✅ | 90 ≥ 90 |
| E≥3P? | ✅ | 90 ≥ 90 |
| F≥3P? | ✅ | 90 ≥ 90 |
| I≥3P? | ✅ | 90 ≥ 90 |

---

## Feature Overview

**NotificationManager** manages in-app notifications for users. Key responsibilities: create notifications (via consumers like PaoWorkflowNotificationService, UNOPSGmailAddonManager, UNOPSGeminiManager), list notifications filtered by userId and unread status, mark as read, update message/status. RecordData stored as JSON; ParseRecordData supports array and single-object formats. Categories/ResponseTypes are free-form (e.g. NewApproval, Recalled, Rejected, Completed, System, User, Alert).

**Implementation:** Manager: `NotificationManager.cs` (no UNOPS override). Controller: `NotificationController.cs`. Entity: `Notification` (Id, UserId, Message, Category, ResponseType, RecordData, Entity, EntityId, IsRead, Status, CreatedAt). Status enum: Pending, Progress, Done, Error.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | GetNotifications returns unread only by default | User 100 has 5 unread, 3 read | GetNotifications(100, null) | 5 notifications, all unread, ordered by CreatedAt desc | P0 |
| POS-002 | GetNotifications with unreadOnly=true | User 100 has 5 unread | GetNotifications(100, true) | 5 unread notifications | P0 |
| POS-003 | GetNotifications with unreadOnly=false | User 100 has 3 read | GetNotifications(100, false) | 3 read notifications | P0 |
| POS-004 | MarkAsRead marks notification | User 100 owns notification 42 (unread) | MarkAsRead(42, 100) | Notification 42 IsRead=true | P0 |
| POS-005 | UpdateNotification updates message and status | Notification 42 exists | UpdateNotification(42, "Updated", NotificationStatus.Done) | Message and Status updated | P0 |
| POS-006 | CreateNotification creates with defaults | User 100, valid inputs | CreateNotification(100, "Msg", "System", "Alert", new { id = 1 }) | Notification created, IsRead=false, RecordData JSON | P0 |
| POS-007 | GetNotifications ordered by CreatedAt desc | User 100 has 3 notifications | GetNotifications(100, false) | Newest first | P0 |
| POS-008 | GetNotifications filters by userId | User 100 has 5, User 200 has 3 | GetNotifications(100, false) | Only user 100's 5 notifications | P0 |
| POS-009 | CreateNotification with workflow category | PaoWorkflowNotificationService | CreateNotification(uid, msg, "NewApproval", "workflow_approval", record) | Created with Category NewApproval | P1 |
| POS-010 | CreateNotification with Gmail category | UNOPSGmailAddonManager | CreateNotification with category from Gmail sync | Created | P1 |
| POS-011 | CreateNotification with AI category | UNOPSGeminiManager | CreateNotification with data modification | Created | P1 |
| POS-012 | ParseRecordData returns array format | RecordData = [{"id":1},{"id":2}] | GetNotifications returns Records | Records has 2 items | P1 |
| POS-013 | ParseRecordData returns single object format | RecordData = {"id":1} | GetNotifications returns Records | Records has 1 item | P1 |
| POS-014 | MarkAsRead idempotent | Notification already read | MarkAsRead(42, 100) | No error, remains read | P1 |
| POS-015 | UpdateNotification Pending to Progress | Notification Status=Pending | UpdateNotification(42, "Processing", Progress) | Status=Progress | P1 |
| POS-016 | UpdateNotification Progress to Done | Notification Status=Progress | UpdateNotification(42, "Complete", Done) | Status=Done | P1 |
| POS-017 | UpdateNotification to Error | Notification exists | UpdateNotification(42, "Failed", Error) | Status=Error | P1 |
| POS-018 | CreateNotification with empty message | Valid other fields | CreateNotification(100, "", "System", "Alert", new {}) | Created (message can be empty) | P1 |
| POS-019 | CreateNotification with complex record object | Nested object | CreateNotification(100, "Msg", "Cat", "Type", new { a = 1, b = new { c = 2 } }) | RecordData serialized correctly | P1 |
| POS-020 | GetNotifications empty for new user | User 999 has no notifications | GetNotifications(999, null) | Empty list | P1 |
| POS-021 | API GET /api/notifications returns 200 | Authenticated user | GET /api/notifications | 200 OK, list of NotificationModel | P0 |
| POS-022 | API GET with unreadOnly query | Authenticated user | GET /api/notifications?unreadOnly=true | 200 OK, unread only | P0 |
| POS-023 | API PUT mark as read returns 204 | Authenticated user owns notification | PUT /api/notifications/42/read | 204 No Content | P0 |
| POS-024 | API PUT update returns 204 | Authenticated user | PUT /api/notifications/42/update with body | 204 No Content | P0 |
| POS-025 | NotificationModel has Id Message Category ResponseType | GetNotifications returns | Inspect model | All fields populated | P1 |
| POS-026 | NotificationModel has Entity EntityId | Notification has Entity/EntityId set | GetNotifications | Entity, EntityId in response | P1 |
| POS-027 | CreateNotification with Entity/EntityId | Direct DbContext add (consumer pattern) | Add Notification with Entity="Opportunity", EntityId=123 | Stored and returned | P1 |
| POS-028 | Multiple consumers create notifications | Workflow, Gmail, Gemini each create | Sequential CreateNotification calls | All created, no conflict | P1 |
| POS-029 | CreateNotification record with list | record = new List<object>{ new { x = 1 } } | CreateNotification | Serialized as JSON array | P1 |
| POS-030 | GetNotifications returns Records parsed | RecordData valid JSON | GetNotifications | Records populated from ParseRecordData | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | MarkAsRead wrong userId | Notification 42 owned by user 100, call MarkAsRead(42, 200) | No change (notification not found for user 200) | P0 |
| NEG-002 | MarkAsRead non-existent notificationId | MarkAsRead(99999, 100) | No error, no-op | P0 |
| NEG-003 | MarkAsRead notificationId zero | MarkAsRead(0, 100) | No match, no-op | P1 |
| NEG-004 | MarkAsRead notificationId negative | MarkAsRead(-1, 100) | No match, no-op | P1 |
| NEG-005 | UpdateNotification non-existent ID | UpdateNotification(99999, "x", Done) | No error, no-op | P0 |
| NEG-006 | UpdateNotification notificationId zero | UpdateNotification(0, "x", Done) | No match, no-op | P1 |
| NEG-007 | GetNotifications userId zero | GetNotifications(0, null) | Empty list (no notifications for user 0) | P1 |
| NEG-008 | GetNotifications userId negative | GetNotifications(-1, null) | Empty list | P1 |
| NEG-009 | CreateNotification userId zero | CreateNotification(0, "Msg", "Cat", "Type", new {}) | May create (no validation) or error per design | P1 |
| NEG-010 | CreateNotification userId negative | CreateNotification(-1, "Msg", "Cat", "Type", new {}) | Per design | P1 |
| NEG-011 | CreateNotification null message | CreateNotification(100, null, "Cat", "Type", new {}) | NullReferenceException or validation | P0 |
| NEG-012 | CreateNotification null category | CreateNotification(100, "Msg", null, "Type", new {}) | Per design | P1 |
| NEG-013 | CreateNotification null responseType | CreateNotification(100, "Msg", "Cat", null, new {}) | Per design | P1 |
| NEG-014 | CreateNotification null record | CreateNotification(100, "Msg", "Cat", "Type", null) | NullReferenceException in JsonSerializer | P0 |
| NEG-015 | API GET unauthenticated | No auth token | GET /api/notifications | 401 Unauthorized | P0 |
| NEG-016 | API PUT mark read unauthenticated | No auth token | PUT /api/notifications/42/read | 401 Unauthorized | P0 |
| NEG-017 | API PUT update unauthenticated | No auth token | PUT /api/notifications/42/update | 401 Unauthorized | P0 |
| NEG-018 | API PUT mark read wrong user's notification | User A auth, notification owned by User B | PUT /api/notifications/42/read | 204 (no-op, no error) | P0 |
| NEG-019 | API PUT update missing request body | Valid auth | PUT /api/notifications/42/update with null/empty body | 400 or 500 per binding | P1 |
| NEG-020 | API PUT update invalid status value | Body { "Message":"x", "Status": 99 } | 400 or handled | P1 |
| NEG-021 | API PUT update invalid JSON | Malformed JSON body | 400 Bad Request | P1 |
| NEG-022 | API GET wrong method POST | POST /api/notifications | 405 Method Not Allowed | P1 |
| NEG-023 | API PUT mark read with GET | GET /api/notifications/42/read | 405 | P1 |
| NEG-024 | ParseRecordData null RecordData | RecordData = null in DB | ParseRecordData returns empty list | P1 |
| NEG-025 | ParseRecordData empty string | RecordData = "" | Returns empty list | P1 |
| NEG-026 | ParseRecordData invalid JSON | RecordData = "not json" | Fallback to single-item list with raw string | P1 |
| NEG-027 | ParseRecordData malformed JSON | RecordData = "{ invalid }" | JsonException caught, fallback | P1 |
| NEG-028 | UpdateNotification null message | UpdateNotification(42, null, Done) | NullReferenceException or DB constraint | P1 |
| NEG-029 | CreateNotification record causes serialization failure | record with circular reference | JsonSerializer throws | P1 |
| NEG-030 | GetNotifications with SQL injection in userId | userId from untrusted source | Parameterized query, no injection | P0 |
| NEG-031 | API path traversal | PUT /api/notifications/../other/read | 404 or sanitized | P1 |
| NEG-032 | API notificationId non-numeric | PUT /api/notifications/abc/read | 400 or 404 | P1 |
| NEG-033 | API notificationId overflow | PUT /api/notifications/2147483648/read | 400 or handled | P1 |
| NEG-034 | MarkAsRead with deleted notification | Notification soft-deleted (if applicable) | Per design | P1 |
| NEG-035 | UpdateNotification with empty message | UpdateNotification(42, "", Done) | Updated (empty string allowed) | P1 |
| NEG-036 | CreateNotification message XSS | Message = "<script>alert(1)</script>" | Stored as-is (sanitization at display layer) | P1 |
| NEG-037 | CreateNotification message SQL injection | Message = "'; DROP TABLE--" | Parameterized, no injection | P0 |
| NEG-038 | GetNotifications unreadOnly invalid type | unreadOnly="invalid" in query | Coerced to null or 400 | P1 |
| NEG-039 | API PUT update message XSS | Message with script tag | Stored, display layer sanitizes | P1 |
| NEG-040 | CreateNotification category empty string | Category = "" | Created | P1 |
| NEG-041 | CreateNotification responseType empty | ResponseType = "" | Created | P1 |
| NEG-042 | GetNotifications for non-existent user | userId = 999999 (no user) | Empty list | P1 |
| NEG-043 | MarkAsRead same notification twice | MarkAsRead(42, 100) then again | Idempotent, no error | P1 |
| NEG-044 | UpdateNotification same notification twice | Two UpdateNotification calls | Second overwrites first | P1 |
| NEG-045 | CreateNotification with very long message | Message 10000 chars | DB constraint or truncated | P1 |
| NEG-046 | CreateNotification with very long category | Category 1000 chars | Per DB schema | P1 |
| NEG-047 | CreateNotification with very long RecordData | record with huge object | Serialization/DB limit | P1 |
| NEG-048 | GetNotifications unreadOnly=null vs omitted | Both cases | Same behavior (default unread) | P1 |
| NEG-049 | API GET with extra query params | ?unreadOnly=true&foo=bar | Ignores foo or 400 | P1 |
| NEG-050 | UpdateNotification does not check userId | User A updates User B's notification | Updates (no ownership check in manager) | P0 |
| NEG-051 | CreateNotification with special chars in message | Message = "Test \"quotes\" & <html>" | Escaped in JSON, stored | P1 |
| NEG-052 | CreateNotification with unicode message | Message = "日本語テスト" | Stored correctly | P1 |
| NEG-053 | ParseRecordData deeply nested JSON | RecordData complex nested | Deserialized to List<object> | P1 |
| NEG-054 | CreateNotification record is JsonElement | record from previous deserialization | Serializes | P1 |
| NEG-055 | CreateNotification record is Dictionary | record = new Dictionary<string,object> | Serializes | P1 |
| NEG-056 | GetNotifications when DB unavailable | Simulate connection failure | Exception propagated | P1 |
| NEG-057 | MarkAsRead when DB unavailable | Simulate connection failure | Exception | P1 |
| NEG-058 | UpdateNotification when DB unavailable | Simulate connection failure | Exception | P1 |
| NEG-059 | CreateNotification when DB unavailable | Simulate connection failure | Exception | P1 |
| NEG-060 | API GET returns 500 on manager exception | Manager throws | 500, error message in response | P1 |
| NEG-061 | API PUT mark read returns 500 on exception | Manager throws | 500 | P1 |
| NEG-062 | API PUT update returns 500 on exception | Manager throws | 500 | P1 |
| NEG-063 | CreateNotification with DateTime in record | record = new { date = DateTime.UtcNow } | Serialized (ISO format) | P1 |
| NEG-064 | CreateNotification with Guid in record | record = new { id = Guid.NewGuid() } | Serialized | P1 |
| NEG-065 | CreateNotification with null in record | record = new { x = (string)null } | Serialized with null | P1 |
| NEG-066 | GetNotifications unreadOnly=false with no read | User has only unread | Empty list | P1 |
| NEG-067 | GetNotifications unreadOnly=true with no unread | User has only read | Empty list | P1 |
| NEG-068 | MarkAsRead notification from different tenant | Multi-tenant scenario | Per design | P2 |
| NEG-069 | UpdateNotification with invalid enum | Status = (NotificationStatus)999 | May throw or persist | P1 |
| NEG-070 | API PUT update missing Message in body | Body { "Status": 0 } | Default or validation | P1 |
| NEG-071 | API PUT update missing Status in body | Body { "Message": "x" } | Default Progress per UpdateNotificationRequest | P1 |
| NEG-072 | CreateNotification with anonymous type | record = new { Id = 1, Name = "Test" } | Serialized | P1 |
| NEG-073 | CreateNotification with array record | record = new[] { 1, 2, 3 } | Wrapped in List<object>, serialized | P1 |
| NEG-074 | ParseRecordData whitespace only | RecordData = "   " | Treated as empty per IsNullOrEmpty | P1 |
| NEG-075 | GetNotifications rapid sequential calls | 10 GET calls in 1 second | All return consistent data | P1 |
| NEG-076 | MarkAsRead then GetNotifications unreadOnly | Mark 42 read, then GET unreadOnly=true | 42 not in list | P1 |
| NEG-077 | CreateNotification duplicate for same user | Two CreateNotification same user, message | Both created | P1 |
| NEG-078 | UpdateNotification to same status | Status already Done | Update succeeds, no change | P1 |
| NEG-079 | UpdateNotification to same message | Message unchanged | Update succeeds | P1 |
| NEG-080 | GetNotifications with unreadOnly explicit true | unreadOnly=true | Same as null (unread only) | P1 |
| NEG-081 | CreateNotification record with binary data | record with byte[] | Serialization may fail or Base64 | P1 |
| NEG-082 | CreateNotification record with stream | record = new MemoryStream() | Serialization fails | P1 |
| NEG-083 | API GET with Accept: application/xml | Request XML response | Returns JSON (default) | P1 |
| NEG-084 | API PUT update with Content-Type wrong | Content-Type: text/plain | 415 or binding fails | P1 |
| NEG-085 | CreateNotification from multiple threads | Concurrent CreateNotification same user | All succeed or proper locking | P1 |
| NEG-086 | GetNotifications while CreateNotification | Concurrent read and create | Consistent view | P1 |
| NEG-087 | MarkAsRead while UpdateNotification | Concurrent mark read and update | Both succeed, final state consistent | P1 |
| NEG-088 | UpdateNotification non-existent no exception | UpdateNotification(99999, "x", Done) | Completes without throw | P1 |
| NEG-089 | GetNotifications userId int max | GetNotifications(2147483647, null) | Empty or per data | P1 |
| NEG-090 | CreateNotification userId int max | CreateNotification(2147483647, "x", "y", "z", new {}) | Per design | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | UserId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-002 | NotificationId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-003 | Message length | 0 | DB max | "" | Max | Max+1 | P1 |
| BND-004 | Category length | 0 | DB max | "" | Max | Max+1 | P1 |
| BND-005 | ResponseType length | 0 | DB max | "" | Max | Max+1 | P1 |
| BND-006 | RecordData length | 0 | DB max | "" | Large JSON | Exceeds | P1 |
| BND-007 | unreadOnly | — | — | null | true | false | P1 |
| BND-008 | NotificationStatus | Pending(0) | Error(3) | Pending | Error | Invalid | P1 |
| BND-009 | Entity length | 0 | DB max | null | "Opportunity" | — | P1 |
| BND-010 | EntityId | 0 | 2147483647 | null | Max | Overflow | P1 |
| BND-011 | CreatedAt | Min | Max | DateTime.MinValue | DateTime.UtcNow | — | P1 |
| BND-012 | Records count | 0 | — | 0 | Many | — | P1 |
| BND-013 | ParseRecordData empty array | — | — | "[]" | — | — | P1 |
| BND-014 | ParseRecordData single element array | — | — | "[{}]" | — | — | P1 |
| BND-015 | ParseRecordData large array | — | — | 1000 elements | — | — | P1 |
| BND-016 | GetNotifications zero results | 0 | — | User has 0 | — | — | P1 |
| BND-017 | GetNotifications one result | 1 | — | User has 1 | — | — | P1 |
| BND-018 | GetNotifications many results | — | — | User has 500 | — | — | P1 |
| BND-019 | CreateNotification message 1 char | 1 | — | "x" | — | — | P1 |
| BND-020 | CreateNotification message 255 chars | — | — | 255 char string | — | — | P1 |
| BND-021 | CreateNotification category 1 char | 1 | — | "x" | — | — | P1 |
| BND-022 | CreateNotification record empty object | — | — | new {} | — | — | P1 |
| BND-023 | CreateNotification record minimal | — | — | new { id = 1 } | — | — | P1 |
| BND-024 | MarkAsRead first notification | — | — | Id=1 | — | — | P1 |
| BND-025 | MarkAsRead last notification | — | — | Id=max | — | — | P1 |
| BND-026 | UpdateNotification status Pending | — | — | Update to Pending | — | — | P1 |
| BND-027 | UpdateNotification status Progress | — | — | Update to Progress | — | — | P1 |
| BND-028 | UpdateNotification status Done | — | — | Update to Done | — | — | P1 |
| BND-029 | UpdateNotification status Error | — | — | Update to Error | — | — | P1 |
| BND-030 | ParseRecordData single object | — | — | "{\"a\":1}" | — | — | P1 |
| BND-031 | ParseRecordData array of objects | — | — | "[{\"a\":1},{\"b\":2}]" | — | — | P1 |
| BND-032 | ParseRecordData array of primitives | — | — | "[1,2,3]" | — | — | P1 |
| BND-033 | ParseRecordData mixed types | — | — | "[1,\"x\",{}]" | — | — | P1 |
| BND-034 | ParseRecordData nested object | — | — | "{\"a\":{\"b\":1}}" | — | — | P1 |
| BND-035 | ParseRecordData invalid then fallback | — | — | "not json" | — | — | P1 |
| BND-036 | GetNotifications unreadOnly null | — | — | null | — | — | P1 |
| BND-037 | GetNotifications unreadOnly true | — | — | true | — | — | P1 |
| BND-038 | GetNotifications unreadOnly false | — | — | false | — | — | P1 |
| BND-039 | CreatedAt ordering tie | — | — | Same CreatedAt for 2 | — | — | P1 |
| BND-040 | CreatedAt millisecond precision | — | — | UtcNow | — | — | P1 |
| BND-041 | Entity null | — | — | null | — | — | P1 |
| BND-042 | EntityId null | — | — | null | — | — | P1 |
| BND-043 | Entity and EntityId both set | — | — | "Opportunity", 123 | — | — | P1 |
| BND-044 | IsRead false default | — | — | New notification | — | — | P1 |
| BND-045 | IsRead true after MarkAsRead | — | — | After mark | — | — | P1 |
| BND-046 | Status default Pending | — | — | New notification | — | — | P1 |
| BND-047 | Category NewApproval | — | — | Workflow | — | — | P1 |
| BND-048 | Category Recalled | — | — | Workflow | — | — | P1 |
| BND-049 | Category Rejected | — | — | Workflow | — | — | P1 |
| BND-050 | Category Completed | — | — | Workflow | — | — | P1 |
| BND-051 | Category System | — | — | System | — | — | P1 |
| BND-052 | Category User | — | — | User | — | — | P1 |
| BND-053 | Category Alert | — | — | Alert | — | — | P1 |
| BND-054 | ResponseType workflow_approval | — | — | PaoWorkflowNotificationService | — | — | P1 |
| BND-055 | ResponseType free-form | — | — | Any string | — | — | P1 |
| BND-056 | Message with newlines | — | — | "Line1\nLine2" | — | — | P1 |
| BND-057 | Message with tabs | — | — | "Col1\tCol2" | — | — | P1 |
| BND-058 | Message with unicode | — | — | "日本語" | — | — | P1 |
| BND-059 | Message with emoji | — | — | "Test 👍" | — | — | P1 |
| BND-060 | RecordData JSON escape | — | — | "{\"msg\":\"quote\\\"\"}" | — | — | P1 |
| BND-061 | RecordData unicode | — | — | "{\"name\":\"日本語\"}" | — | — | P1 |
| BND-062 | Multiple users same notification count | — | — | 2 users, 10 each | — | — | P1 |
| BND-063 | User with only read notifications | — | — | All IsRead=true | — | — | P1 |
| BND-064 | User with only unread | — | — | All IsRead=false | — | — | P1 |
| BND-065 | User with mixed read/unread | — | — | 5 read, 5 unread | — | — | P1 |
| BND-066 | CreateNotification rapid sequence | — | — | 10 in 1 sec | — | — | P1 |
| BND-067 | MarkAsRead batch | — | — | Mark 10 as read | — | — | P1 |
| BND-068 | UpdateNotification batch | — | — | Update 10 | — | — | P1 |
| BND-069 | GetNotifications then Create | — | — | GET, Create, GET | — | — | P1 |
| BND-070 | Create then Get unreadOnly | — | — | Create, GET null | — | — | P1 |
| BND-071 | MarkAsRead then Get unreadOnly false | — | — | Mark, GET false | — | — | P1 |
| BND-072 | NotificationId 0 | — | — | 0 | — | — | P1 |
| BND-073 | NotificationId -1 | — | — | -1 | — | — | P1 |
| BND-074 | UserId 0 | — | — | 0 | — | — | P1 |
| BND-075 | UserId -1 | — | — | -1 | — | — | P1 |
| BND-076 | Empty Records in model | — | — | RecordData null | — | — | P1 |
| BND-077 | Single Record in model | — | — | RecordData single object | — | — | P1 |
| BND-078 | Multiple Records in model | — | — | RecordData array | — | — | P1 |
| BND-079 | API route with trailing slash | — | — | /api/notifications/ | — | — | P1 |
| BND-080 | API route without trailing slash | — | — | /api/notifications | — | — | P1 |
| BND-081 | API PUT path param | — | — | /api/notifications/42/read | — | — | P1 |
| BND-082 | API PUT update path | — | — | /api/notifications/42/update | — | — | P1 |
| BND-083 | Query param unreadOnly true string | — | — | ?unreadOnly=true | — | — | P1 |
| BND-084 | Query param unreadOnly false string | — | — | ?unreadOnly=false | — | — | P1 |
| BND-085 | Query param unreadOnly omitted | — | — | No param | — | — | P1 |
| BND-086 | JsonSerializer options | — | — | Default options | — | — | P1 |
| BND-087 | ParseRecordData JsonElement in list | — | — | Deserialized array | — | — | P1 |
| BND-088 | CreateNotification DateTime.Kind | — | — | Utc vs Local | — | — | P1 |
| BND-089 | CreatedAt timezone | — | — | UTC | — | — | P1 |
| BND-090 | NotificationModel Status not mapped | — | — | Manager mapping | — | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | GetNotifications default shows unread only | unreadOnly null = unread only | GetNotifications(100, null) | Only IsRead=false | P0 |
| FUN-002 | GetNotifications unreadOnly true = unread | Filter | GetNotifications(100, true) | Only unread | P0 |
| FUN-003 | GetNotifications unreadOnly false = read | Filter | GetNotifications(100, false) | Only read | P0 |
| FUN-004 | GetNotifications filters by UserId | User isolation | GetNotifications(100, false) | Only user 100's | P0 |
| FUN-005 | GetNotifications ordered CreatedAt desc | Sort rule | GetNotifications | Newest first | P0 |
| FUN-006 | MarkAsRead sets IsRead true | Mark rule | MarkAsRead(42, 100) | IsRead=true | P0 |
| FUN-007 | MarkAsRead requires userId match | Ownership | MarkAsRead(42, 200) when owned by 100 | No change | P0 |
| FUN-008 | UpdateNotification updates Message | Update rule | UpdateNotification(42, "New", Done) | Message="New" | P0 |
| FUN-009 | UpdateNotification updates Status | Update rule | UpdateNotification(42, "x", Error) | Status=Error | P0 |
| FUN-010 | UpdateNotification does not check userId | No ownership | Update any notification by ID | Updates | P0 |
| FUN-011 | CreateNotification sets IsRead false | Default | CreateNotification | IsRead=false | P0 |
| FUN-012 | CreateNotification serializes record to JSON | Serialization | CreateNotification(100, "x", "y", "z", new { a = 1 }) | RecordData = [{"a":1}] | P0 |
| FUN-013 | CreateNotification sets CreatedAt | Timestamp | CreateNotification | CreatedAt = UtcNow | P0 |
| FUN-014 | ParseRecordData null/empty returns empty list | Fallback | ParseRecordData(null) | [] | P0 |
| FUN-015 | ParseRecordData array deserializes | Array format | RecordData = "[{},{}]" | Records count 2 | P0 |
| FUN-016 | ParseRecordData object deserializes to single | Object format | RecordData = "{}" | Records count 1 | P0 |
| FUN-017 | ParseRecordData invalid JSON fallback | Error handling | RecordData = "x" | Records = [raw string] | P0 |
| FUN-018 | NotificationModel maps Id Message Category | Mapping | GetNotifications | All fields correct | P0 |
| FUN-019 | NotificationModel Records from ParseRecordData | Mapping | GetNotifications | Records populated | P0 |
| FUN-020 | MarkAsRead idempotent | Idempotency | MarkAsRead twice | No error | P1 |
| FUN-021 | UpdateNotification overwrites | Overwrite | Two updates | Last wins | P1 |
| FUN-022 | CreateNotification assigns UserId | Assignment | CreateNotification(100, ...) | UserId=100 | P1 |
| FUN-023 | CreateNotification assigns Category | Assignment | CreateNotification(..., "System", ...) | Category=System | P1 |
| FUN-024 | CreateNotification assigns ResponseType | Assignment | CreateNotification(..., "Alert") | ResponseType=Alert | P1 |
| FUN-025 | GetNotifications empty returns empty list | Empty | User has none | [] | P1 |
| FUN-026 | MarkAsRead non-existent no-op | No-op | MarkAsRead(99999, 100) | Completes | P1 |
| FUN-027 | UpdateNotification non-existent no-op | No-op | UpdateNotification(99999, ...) | Completes | P1 |
| FUN-028 | Status transition Pending to Progress | Transition | UpdateNotification(..., Progress) | Status=Progress | P1 |
| FUN-029 | Status transition Progress to Done | Transition | UpdateNotification(..., Done) | Status=Done | P1 |
| FUN-030 | Status transition any to Error | Transition | UpdateNotification(..., Error) | Status=Error | P1 |
| FUN-031 | Workflow consumer creates NewApproval | Consumer | PaoWorkflowNotificationService | Category NewApproval | P1 |
| FUN-032 | Workflow consumer creates Recalled | Consumer | Recall workflow | Category Recalled | P1 |
| FUN-033 | Workflow consumer creates Rejected | Consumer | Reject workflow | Category Rejected | P1 |
| FUN-034 | Workflow consumer creates Completed | Consumer | Approve workflow | Category Completed | P1 |
| FUN-035 | Gmail consumer creates notification | Consumer | UNOPSGmailAddonManager | Notification created | P1 |
| FUN-036 | Gemini consumer creates notification | Consumer | UNOPSGeminiManager | Notification created | P1 |
| FUN-037 | DueDiligenceNotificationService | Consumer | Due diligence | Notification created | P1 |
| FUN-038 | Record wrapped in List for serialization | CreateNotification | record object | JsonSerializer.Serialize([record]) | P1 |
| FUN-039 | Entity EntityId not set by CreateNotification | CreateNotification | Manager method | Entity=null, EntityId=null | P1 |
| FUN-040 | Entity EntityId set by direct DbContext add | Consumer | PaoWorkflowNotificationService adds | Entity, EntityId set | P1 |
| FUN-041 | API uses CurrentUserId | Controller | GetNotifications | CurrentUserId from UserResolverService | P1 |
| FUN-042 | API mark read uses CurrentUserId | Controller | MarkAsRead | CurrentUserId passed | P1 |
| FUN-043 | API update does not pass userId | Controller | UpdateNotification | No userId in manager call | P1 |
| FUN-044 | API GET returns List<NotificationModel> | Response | GET /api/notifications | Ok(list) | P1 |
| FUN-045 | API PUT mark read returns NoContent | Response | PUT read | 204 No Content | P1 |
| FUN-046 | API PUT update returns NoContent | Response | PUT update | 204 No Content | P1 |
| FUN-047 | API exception returns 500 | Error handling | Manager throws | 500, error body | P1 |
| FUN-048 | ParseRecordData JsonException first branch | Array fail | Invalid array JSON | Try object branch | P1 |
| FUN-049 | ParseRecordData JsonException second branch | Object fail | Invalid object JSON | Fallback to raw string | P1 |
| FUN-050 | ParseRecordData null element in array | Edge | "[null,1]" | Handled | P1 |
| FUN-051 | CreateNotification record with JsonDocument | Serialization | JsonDocument | Serializes | P1 |
| FUN-052 | CreateNotification record with JsonNode | Serialization | JsonNode | Serializes | P1 |
| FUN-053 | GetNotifications does not expose IsRead in model | Model | NotificationModel | No IsRead property in model | P1 |
| FUN-054 | GetNotifications does not expose UserId in model | Model | NotificationModel | No UserId in model | P1 |
| FUN-055 | GetNotifications does not expose RecordData raw | Model | NotificationModel | Records not RecordData | P1 |
| FUN-056 | CreateNotification does not set Entity | CreateNotification | Manager | Entity left default | P1 |
| FUN-057 | CreateNotification does not set EntityId | CreateNotification | Manager | EntityId left default | P1 |
| FUN-058 | CreateNotification does not set Status | CreateNotification | Manager | Status defaults Pending | P1 |
| FUN-059 | UpdateNotification request Message required | UpdateNotificationRequest | Model | Message property | P1 |
| FUN-060 | UpdateNotification request Status defaults | UpdateNotificationRequest | Model | Status default Progress | P1 |
| FUN-061 | Multiple categories in system | Categories | Various consumers | NewApproval, System, etc. | P1 |
| FUN-062 | Multiple response types | ResponseTypes | Various consumers | workflow_approval, etc. | P1 |
| FUN-063 | CreateNotification from async context | Async | await CreateNotification | Completes | P1 |
| FUN-064 | GetNotifications from async context | Async | await GetNotifications | Completes | P1 |
| FUN-065 | MarkAsRead from async context | Async | await MarkAsRead | Completes | P1 |
| FUN-066 | UpdateNotification from async context | Async | await UpdateNotification | Completes | P1 |
| FUN-067 | SaveChangesAsync after MarkAsRead | Persistence | MarkAsRead | DB updated | P1 |
| FUN-068 | SaveChangesAsync after UpdateNotification | Persistence | UpdateNotification | DB updated | P1 |
| FUN-069 | SaveChangesAsync after CreateNotification | Persistence | CreateNotification | DB updated | P1 |
| FUN-070 | AddAsync before SaveChanges in Create | Order | CreateNotification | Add then Save | P1 |
| FUN-071 | FirstOrDefaultAsync in MarkAsRead | Query | MarkAsRead | Single match | P1 |
| FUN-072 | FirstOrDefaultAsync in UpdateNotification | Query | UpdateNotification | Single match | P1 |
| FUN-073 | ToListAsync in GetNotifications | Query | GetNotifications | Materialized list | P1 |
| FUN-074 | Where UserId in GetNotifications | Filter | GetNotifications | userId filter | P1 |
| FUN-075 | Where IsRead in GetNotifications | Filter | unreadOnly logic | IsRead filter | P1 |
| FUN-076 | OrderByDescending CreatedAt | Sort | GetNotifications | Order applied | P1 |
| FUN-077 | Select to NotificationModel | Projection | GetNotifications | Model mapping | P1 |
| FUN-078 | ParseRecordData called per notification | Per-item | GetNotifications | Each RecordData parsed | P1 |
| FUN-079 | JsonSerializer default options | Serialization | CreateNotification | No custom options | P1 |
| FUN-080 | DbContext scoped per request | DI | NotificationManager | AppDbContext injected | P1 |
| FUN-081 | UserResolverService for controller | DI | NotificationController | CurrentUserId | P1 |
| FUN-082 | No permission check on GetNotifications | Auth | Controller | [Authorize] only | P1 |
| FUN-083 | No permission check on MarkAsRead | Auth | Controller | [Authorize] only | P1 |
| FUN-084 | No permission check on UpdateNotification | Auth | Controller | [Authorize] only | P1 |
| FUN-085 | IApplicationService implementation | Interface | NotificationManager | Implements | P1 |
| FUN-086 | Notification entity table Notifications | DB | Entity | ToTable("Notifications") | P1 |
| FUN-087 | Notification primary key Id | DB | Entity | Id PK | P1 |
| FUN-088 | Notification foreign key UserId | DB | Entity | UserId to User | P1 |
| FUN-089 | CreateNotification no Entity/EntityId in manager | Manager | CreateNotification | Not set | P1 |
| FUN-090 | PaoWorkflowNotificationService uses NotificationManager | Integration | Workflow | CreateNotification called | P1 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | GET /api/notifications full flow | API to DB | Controller, Manager, DbContext | 200, list from DB | P0 |
| INT-002 | PUT mark read full flow | API to DB | Controller, Manager, DbContext | 204, IsRead updated | P0 |
| INT-003 | PUT update full flow | API to DB | Controller, Manager, DbContext | 204, Message/Status updated | P0 |
| INT-004 | CreateNotification then GetNotifications | Manager flow | Manager, DbContext | Created appears in GET | P0 |
| INT-005 | MarkAsRead then GetNotifications unreadOnly | Manager flow | Manager, DbContext | Marked not in unread list | P0 |
| INT-006 | Workflow approval creates notification | PaoWorkflowNotificationService | Workflow, NotificationManager | Notification created | P0 |
| INT-007 | Gmail sync creates notification | UNOPSGmailAddonManager | Gmail, NotificationManager | Notification created | P0 |
| INT-008 | Gemini AI creates notification | UNOPSGeminiManager | Gemini, NotificationManager | Notification created | P0 |
| INT-009 | Workflow recall marks notifications done | PaoWorkflowNotificationService | Workflow, DbContext | MarkWorkflowNotificationsAsRecalledAsync | P0 |
| INT-010 | Workflow reject marks notifications done | PaoWorkflowNotificationService | Workflow, DbContext | MarkWorkflowNotificationsAsRejectedAsync | P0 |
| INT-011 | Workflow approve marks notifications done | PaoWorkflowNotificationService | Workflow, DbContext | MarkWorkflowNotificationsAsApprovedAsync | P0 |
| INT-012 | Create then MarkAsRead then Get | Full flow | Manager | Create, Mark, Get unread=false shows it read | P1 |
| INT-013 | Create then UpdateNotification then Get | Full flow | Manager | Create, Update, Get shows new message | P1 |
| INT-014 | Multiple users CreateNotification | Multi-user | Manager | Each user's notifications isolated | P1 |
| INT-015 | GetNotifications with seeded data | DB seed | DbContext, Manager | Returns seeded notifications | P1 |
| INT-016 | API auth pipeline | Request | Auth middleware, Controller | 401 if no token | P1 |
| INT-017 | API model binding | Request | Controller, UpdateNotificationRequest | Body bound to request | P1 |
| INT-018 | API route matching | Request | Routing | /api/notifications matches | P1 |
| INT-019 | API route param notificationId | Request | Routing | {notificationId} bound | P1 |
| INT-020 | DbContext factory in PaoWorkflowNotificationService | Workflow | IDbContextFactory, DbContext | Separate context per operation | P1 |
| INT-021 | NotificationManager in ManagerWrapper | DI | ManagerWrapper | NotificationManager resolved | P1 |
| INT-022 | NotificationManager in UNOPSManagerWrapper | DI | UNOPSManagerWrapper | NotificationManager resolved | P1 |
| INT-023 | Controller receives NotificationManager | DI | Controller | NotificationManager injected | P1 |
| INT-024 | UserResolverService in Controller | DI | Controller | CurrentUserId from claims | P1 |
| INT-025 | Logger in Controller | DI | Controller | ILogger injected | P1 |
| INT-026 | AppDbContext Notifications DbSet | DbContext | AppDbContext | Notifications DbSet | P1 |
| INT-027 | Notification entity mapping | EF | DbContext | Notification configured | P1 |
| INT-028 | Migration includes Notifications table | Migration | EF migrations | Table exists | P1 |
| INT-029 | CreateNotification from Workflow with record | Workflow | WorkflowNotification, record | Record serialized | P1 |
| INT-030 | CreateNotification from Gmail with record | Gmail | CreationState, record | Record serialized | P1 |
| INT-031 | CreateNotification from Gemini with record | Gemini | Data modification, record | Record serialized | P1 |
| INT-032 | GetNotifications returns only current user | API | Controller, UserResolverService | User A cannot see User B's | P1 |
| INT-033 | MarkAsRead only own notification | API | Controller, Manager | User A cannot mark User B's | P1 |
| INT-034 | UpdateNotification any notification | API | Controller, Manager | No userId check, updates any | P1 |
| INT-035 | NotificationModel in API response | API | Controller, Model | JSON serialization | P1 |
| INT-036 | UpdateNotificationRequest from body | API | Controller, Model | JSON deserialization | P1 |
| INT-037 | APIDictionary.Notifications constant | Route | APIDictionary | "api/notifications" | P1 |
| INT-038 | APIDictionary.NotificationRead constant | Route | APIDictionary | "api/notifications/{notificationId}/read" | P1 |
| INT-039 | Update route hardcoded | Route | Controller | "api/notifications/{notificationId}/update" | P1 |
| INT-040 | CreateNotification AddAsync then SaveChanges | Manager | DbContext | Add then Save | P1 |
| INT-041 | MarkAsRead FirstOrDefault then SaveChanges | Manager | DbContext | Load, modify, Save | P1 |
| INT-042 | UpdateNotification FirstOrDefault then SaveChanges | Manager | DbContext | Load, modify, Save | P1 |
| INT-043 | GetNotifications ToListAsync | Manager | DbContext | Async enumeration | P1 |
| INT-044 | ParseRecordData in Select | Manager | GetNotifications | In-memory parse | P1 |
| INT-045 | JsonSerializer in CreateNotification | Manager | System.Text.Json | Serialize record | P1 |
| INT-046 | JsonSerializer.Deserialize in ParseRecordData | Manager | System.Text.Json | Deserialize RecordData | P1 |
| INT-047 | NotificationStatus enum in UpdateNotification | Manager | Domain.Enums | Status parameter | P1 |
| INT-048 | NotificationStatus in UpdateNotificationRequest | Model | UpdateNotificationRequest | Status property | P1 |
| INT-049 | NotificationStatus in NotificationModel | Model | NotificationModel | Status property | P1 |
| INT-050 | Notification entity Status property | Entity | Notification | Status column | P1 |
| INT-051 | Notification entity IsRead property | Entity | Notification | IsRead column | P1 |
| INT-052 | Notification entity RecordData property | Entity | Notification | RecordData column (JSON) | P1 |
| INT-053 | Notification entity CreatedAt property | Entity | Notification | CreatedAt column | P1 |
| INT-054 | Notification entity Entity EntityId | Entity | Notification | Nullable columns | P1 |
| INT-055 | PaoWorkflowNotificationService CreateInSystemNotificationsAsync | Workflow | Service | Calls NotificationManager | P1 |
| INT-056 | PaoWorkflowNotificationService MarkWorkflowNotificationsAsDoneAsync | Workflow | Service | Uses DbContext directly | P1 |
| INT-057 | Workflow approval email + in-system notification | Workflow | Email, Notification | Both sent | P1 |
| INT-058 | Gmail creation notification content | Gmail | NotificationManager | Message, Category set | P1 |
| INT-059 | Gemini data modification notification | Gemini | NotificationManager | Record has modification data | P1 |
| INT-060 | DueDiligenceNotificationService integration | Due diligence | Service | Creates notifications | P1 |
| INT-061 | AiContextualService direct DbContext | AI | AiContextualService | Adds to Notifications | P1 |
| INT-062 | AiContextualService vs NotificationManager | AI | Two paths | Both create notifications | P1 |
| INT-063 | NotificationController exception logging | Controller | ILogger | LogError on exception | P1 |
| INT-064 | NotificationController 500 response body | Controller | Exception handler | { error: "..." } | P1 |
| INT-065 | Global exception handler | Exception | Middleware | May handle 500 | P1 |
| INT-066 | Integration test GetNotifications_NoParams | Test | NotificationControllerTests | Returns Ok or empty | P1 |
| INT-067 | Integration test GetNotifications_UnreadOnlyTrue | Test | NotificationControllerTests | Filters | P1 |
| INT-068 | Integration test GetNotifications_UnreadOnlyFalse | Test | NotificationControllerTests | Filters | P1 |
| INT-069 | Integration test MarkAsRead workflow | Test | NotificationControllerTests | Mark then Get | P1 |
| INT-070 | Integration test UpdateNotification workflow | Test | NotificationControllerTests | Update then Get | P1 |
| INT-071 | Integration test GetNotifications_ReturnsOnlyCurrentUserData | Test | NotificationControllerTests | User isolation | P1 |
| INT-072 | Unit test NotificationManagerFullTests | Test | NotificationManagerFullTests | Manager tests | P1 |
| INT-073 | PAOWebApplicationFactory NotificationManager | Test | PAOWebApplicationFactory | NotificationManager registered | P1 |
| INT-074 | WorkflowControllerTests mock NotificationManager | Test | WorkflowControllerTests | Mock for workflow | P1 |
| INT-075 | PaoWorkflowNotificationServiceCCTests mock | Test | PaoWorkflowNotificationServiceCCTests | Mock NotificationManager | P1 |
| INT-076 | PNO-1166 tests mock NotificationManager | Test | PNO-1166 | Mock in fixture | P1 |
| INT-077 | PNO-1197 tests mock NotificationManager | Test | PNO-1197 | Mock in fixture | P1 |
| INT-078 | CreateNotification record round-trip | Serialization | Create, Get | RecordData -> Records | P1 |
| INT-079 | ParseRecordData array round-trip | Deserialization | RecordData array | Records matches | P1 |
| INT-080 | ParseRecordData object round-trip | Deserialization | RecordData object | Records matches | P1 |
| INT-081 | Status transition round-trip | Update, Get | Update status, verify | P1 |
| INT-082 | Message update round-trip | Update, Get | Update message, verify | P1 |
| INT-083 | MarkAsRead round-trip | Mark, Get | Mark read, GET unread=false | P1 |
| INT-084 | Multi-consumer notification mix | Workflow, Gmail, Gemini | All create, Get returns all | P1 |
| INT-085 | Category filter in consumer logic | Consumers | Filter by Category | Per consumer | P1 |
| INT-086 | ResponseType filter in consumer logic | Consumers | Filter by ResponseType | Per consumer | P1 |
| INT-087 | Entity EntityId for navigation | Model | Entity, EntityId | Link to entity | P1 |
| INT-088 | Workflow entity name in notification | Workflow | EntityName | Opportunity, Partner, etc. | P1 |
| INT-089 | Workflow entity ID in notification | Workflow | EntityId | Entity ID | P1 |
| INT-090 | Full E2E: Submit -> Notify -> Approve -> Mark done | E2E | Workflow, Notification | End-to-end flow | P1 |

---

## §6 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent GetNotifications same user | 10 threads GetNotifications(100, null) | All return consistent data | P0 |
| CON-002 | Concurrent CreateNotification same user | 10 threads CreateNotification(100, ...) | All 10 created | P0 |
| CON-003 | Concurrent MarkAsRead same notification | 2 threads MarkAsRead(42, 100) | Idempotent, both succeed | P0 |
| CON-004 | Concurrent MarkAsRead different notifications | 10 threads MarkAsRead(id, 100) different ids | All succeed | P0 |
| CON-005 | Concurrent UpdateNotification same notification | 2 threads UpdateNotification(42, msg1, s1) and (42, msg2, s2) | One overwrites, no corruption | P0 |
| CON-006 | GetNotifications while CreateNotification | Thread 1 GET, Thread 2 Create | GET may or may not include new | P1 |
| CON-007 | MarkAsRead while GetNotifications | Thread 1 Mark, Thread 2 GET | Consistent view | P1 |
| CON-008 | CreateNotification while UpdateNotification | Different notifications | Both succeed | P1 |
| CON-009 | Multiple users concurrent CreateNotification | 5 users, 5 threads each | All 25 created | P1 |
| CON-010 | Concurrent GetNotifications different users | 10 users, 1 thread each | No cross-user data | P1 |
| CON-011 | CreateNotification rapid fire same user | 100 CreateNotification in parallel | All 100 created | P1 |
| CON-012 | MarkAsRead rapid fire different notifications | 50 MarkAsRead in parallel | All succeed | P1 |
| CON-013 | UpdateNotification rapid fire different | 50 UpdateNotification in parallel | All succeed | P1 |
| CON-014 | DbContext concurrent access | Single DbContext, parallel ops | EF handles or throws | P1 |
| CON-015 | PaoWorkflowNotificationService DbContextFactory | Workflow uses factory | Separate contexts, no conflict | P1 |
| CON-016 | API concurrent GET requests | 20 GET /api/notifications parallel | All 200 | P1 |
| CON-017 | API concurrent PUT mark read | 10 PUT mark read different ids | All 204 | P1 |
| CON-018 | API concurrent PUT update | 10 PUT update different ids | All 204 | P1 |
| CON-019 | CreateNotification and GetNotifications interleaved | Create, GET, Create, GET | No deadlock | P1 |
| CON-020 | MarkAsRead and UpdateNotification same id | Concurrent Mark and Update | Both succeed, final state consistent | P1 |
| CON-021 | SaveChangesAsync concurrent | Two managers, same DbContext | Scoped per request, separate instances | P1 |
| CON-022 | ParseRecordData thread safety | Multiple GetNotifications parallel | ParseRecordData is static, no shared state | P1 |
| CON-023 | JsonSerializer thread safety | Multiple CreateNotification parallel | JsonSerializer thread-safe | P1 |
| CON-024 | Workflow and Gmail concurrent CreateNotification | Both services create | Both succeed | P1 |
| CON-025 | Workflow mark done and MarkAsRead concurrent | MarkWorkflowNotificationsAsDoneAsync and MarkAsRead | Per implementation | P1 |

---

## §7 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | ParseRecordData null | ParseRecordData | null | [] | P0 |
| UNT-002 | ParseRecordData empty string | ParseRecordData | "" | [] | P0 |
| UNT-003 | ParseRecordData "[]" | ParseRecordData | "[]" | [] | P0 |
| UNT-004 | ParseRecordData "[{}]" | ParseRecordData | "[{}]" | [{}] | P0 |
| UNT-005 | ParseRecordData "[{\"a\":1}]" | ParseRecordData | "[{\"a\":1}]" | 1 element | P0 |
| UNT-006 | ParseRecordData "{\"a\":1}" | ParseRecordData | "{\"a\":1}" | 1 element (object) | P0 |
| UNT-007 | ParseRecordData invalid JSON | ParseRecordData | "invalid" | [raw string] | P0 |
| UNT-008 | ParseRecordData "[1,2,3]" | ParseRecordData | "[1,2,3]" | 3 elements | P1 |
| UNT-009 | CreateNotification serialization | CreateNotification | record = new { x = 1 } | RecordData contains x:1 | P0 |
| UNT-010 | GetNotifications mapping Id | GetNotifications | Notification with Id=42 | Model Id=42 | P0 |
| UNT-011 | GetNotifications mapping Message | GetNotifications | Notification with Message | Model Message set | P0 |
| UNT-012 | GetNotifications mapping Category | GetNotifications | Notification with Category | Model Category set | P0 |
| UNT-013 | GetNotifications mapping ResponseType | GetNotifications | Notification with ResponseType | Model ResponseType set | P0 |
| UNT-014 | GetNotifications mapping Entity EntityId | GetNotifications | Notification with Entity, EntityId | Model has both | P0 |
| UNT-015 | GetNotifications mapping Records | GetNotifications | Notification with RecordData | Records from ParseRecordData | P0 |
| UNT-016 | MarkAsRead condition userId match | MarkAsRead | notification.UserId == userId | Updates | P0 |
| UNT-017 | MarkAsRead condition userId mismatch | MarkAsRead | notification.UserId != userId | No update | P0 |
| UNT-018 | UpdateNotification condition exists | UpdateNotification | notification != null | Updates | P0 |
| UNT-019 | UpdateNotification condition not exists | UpdateNotification | notification == null | No update | P0 |
| UNT-020 | GetNotifications unreadOnly null branch | GetNotifications | unreadOnly = null | Where IsRead == false | P0 |
| UNT-021 | GetNotifications unreadOnly has value branch | GetNotifications | unreadOnly = true/false | Where IsRead == !unreadOnly | P0 |

---

## §8 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetNotifications 100 notifications | GetNotifications(100, false) | < 500ms | P0 |
| PRF-002 | GetNotifications 1000 notifications | GetNotifications(100, false) | < 2s | P0 |
| PRF-003 | CreateNotification single | CreateNotification(...) | < 100ms | P0 |
| PRF-004 | MarkAsRead single | MarkAsRead(42, 100) | < 100ms | P0 |
| PRF-005 | UpdateNotification single | UpdateNotification(42, "x", Done) | < 100ms | P0 |
| PRF-006 | GetNotifications with ParseRecordData | 50 notifications with RecordData | < 300ms | P1 |
| PRF-007 | CreateNotification with large record | record 10KB | < 200ms | P1 |
| PRF-008 | ParseRecordData large JSON | RecordData 50KB | < 50ms | P1 |
| PRF-009 | API GET /api/notifications | Full request | < 500ms | P0 |
| PRF-010 | API PUT mark read | Full request | < 200ms | P0 |
| PRF-011 | API PUT update | Full request | < 200ms | P0 |
| PRF-012 | GetNotifications empty | User has 0 | < 100ms | P1 |
| PRF-013 | CreateNotification 10 sequential | 10 CreateNotification | < 2s total | P1 |
| PRF-014 | MarkAsRead 10 sequential | 10 MarkAsRead | < 1s total | P1 |
| PRF-015 | GetNotifications ordered | 500 items OrderByDescending | < 1s | P1 |
| PRF-016 | ParseRecordData 100 items in array | RecordData 100 elements | < 100ms | P1 |

---

## §9 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | GET /api/notifications sustained | 20 req/s | 5 min | 95% < 500ms | P0 |
| LDT-002 | PUT mark read sustained | 10 req/s | 5 min | 95% < 200ms | P0 |
| LDT-003 | PUT update sustained | 10 req/s | 5 min | 95% < 200ms | P0 |
| LDT-004 | Mixed GET and PUT | 15 GET/s, 5 PUT/s | 5 min | 95% < 500ms | P0 |
| LDT-005 | CreateNotification burst | 50 CreateNotification in 1s | 1 burst | All succeed | P1 |
| LDT-006 | GetNotifications burst | 100 GET in 1s | 1 burst | 95% < 500ms | P1 |
| LDT-007 | MarkAsRead burst | 50 PUT in 1s | 1 burst | All 204 | P1 |
| LDT-008 | Multi-user load | 50 users, 2 req/s each | 5 min | No errors | P1 |
| LDT-009 | Ramp-up GET | 0 to 30 req/s over 2 min | 2 min | No timeout | P1 |
| LDT-010 | Steady state mixed | 20 req/s mixed | 10 min | 99% success | P0 |

---

## Implementation Status

| Section | Total | Implemented | Automated | Status |
|---------|-------|-------------|-----------|--------|
| §1 Positive | 30 | 12 | NotificationManagerFullTests, NotificationControllerTests | Partial |
| §2 Negative | 90 | 15 | NotificationControllerTests | Partial |
| §3 Boundary | 90 | 5 | NotificationManagerFullTests | Partial |
| §4 Functional | 90 | 8 | NotificationManagerFullTests | Partial |
| §5 Integration | 90 | 12 | NotificationControllerTests, Workflow tests | Partial |
| §6 Concurrency | 25 | 2 | NotificationControllerTests | Partial |
| §7 Unit | 21 | 21 | NotificationManagerFullTests (parse, mapping) | Partial |
| §8 Performance | 16 | 1 | NotificationControllerTests | Partial |
| §9 Load | 10 | 0 | — | Not started |
| **TOTAL** | **462** | **76** | — | **Partial** |

**Notes:**
- NotificationManager has no UNOPS override; tests apply to base implementation.
- API endpoints: GET /api/notifications, PUT /api/notifications/{id}/read, PUT /api/notifications/{id}/update.
- Consumers: PaoWorkflowNotificationService, UNOPSGmailAddonManager, UNOPSGeminiManager, DueDiligenceNotificationService, AiContextualService (direct DbContext).
- UpdateNotification does not validate userId; MarkAsRead does.
- ParseRecordData: null/empty → []; array → deserialize; object → [object]; invalid → [raw string].

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
