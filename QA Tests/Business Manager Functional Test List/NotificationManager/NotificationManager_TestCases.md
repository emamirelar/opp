# NotificationManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `NotificationManager`  
**Location**: `UNOPS.PAO.Business/Managers/NotificationManager.cs`  
**Purpose**: Manages user notifications, read/unread status, and notification delivery.

---

## Functional Test Cases (25 Cases)

### TC-NM-F001-025: Core Operations
- F001: Get user notifications - unread only (default)
- F002: Get user notifications - all (read + unread)
- F003: Get notifications for user with no notifications
- F004: Get notifications - read only filter
- F005: Get notifications - ordered by creation date descending
- F006: Mark notification as read - valid notification
- F007: Mark notification as read - wrong user (security)
- F008: Mark notification as read - non-existent notification
- F009: Mark already read notification as read (idempotent)
- F010: Update notification message and status
- F011: Update notification status to different values
- F012: Update non-existent notification
- F013: Create notification for user
- F014: Create notification with record data
- F015: Create notification with complex record object
- F016: Create notification with empty message
- F017: Parse record data - JSON array format
- F018: Parse record data - single object format
- F019: Parse record data - invalid JSON
- F020: Parse record data - null/empty string
- F021: Get notifications with category filter
- F022: Get notifications with response type filter
- F023: Notification with maximum message length
- F024: Bulk mark as read for user
- F025: Get notification count - unread for user

---

## Performance Test Cases (10 Cases)

### TC-NM-P001: Get Notifications - Large Unread Count
**Performance Criteria**: < 500ms for 500 unread notifications  
**Test Steps**: Query user with 500 unread notifications  
**Expected Result**: Query < 500ms  

### TC-NM-P002: Create Notification - Response Time
**Performance Criteria**: < 200ms per notification  
**Test Steps**: Create 100 notifications sequentially  
**Expected Result**: Average < 200ms  

### TC-NM-P003: Mark As Read - Bulk Operation
**Performance Criteria**: < 1000ms for 100 notifications  
**Test Steps**: Mark 100 notifications as read  
**Expected Result**: Bulk operation < 1000ms  

### TC-NM-P004: Get Notifications - Mixed Read/Unread
**Performance Criteria**: < 800ms on 10K total notifications  
**Test Steps**: Query unread from large notification set  
**Expected Result**: Filtered query < 800ms  

### TC-NM-P005: Notification Creation Throughput
**Performance Criteria**: > 100 notifications/second  
**Test Steps**: Create 1000 notifications rapidly  
**Expected Result**: Throughput > 100/sec  

### TC-NM-P006: Update Notification - Response Time
**Performance Criteria**: < 300ms per update  
**Test Steps**: Update 50 notifications  
**Expected Result**: Average < 300ms  

### TC-NM-P007: Parse Complex Record Data - Performance
**Performance Criteria**: < 50ms for large JSON  
**Test Steps**: Parse notification with 100KB JSON record  
**Expected Result**: Parse < 50ms  

### TC-NM-P008: Get Notifications - Multiple Users Simultaneously
**Performance Criteria**: < 600ms per user with contention  
**Test Steps**: 50 users query their notifications  
**Expected Result**: All queries < 600ms  

### TC-NM-P009: Notification Filtering Performance
**Performance Criteria**: < 400ms with multiple filters  
**Test Steps**: Filter by category, type, read status  
**Expected Result**: Complex filter < 400ms  

### TC-NM-P010: Get Notifications - Recent Only
**Performance Criteria**: < 300ms for last 24 hours  
**Test Steps**: Query recent notifications with date filter  
**Expected Result**: Query < 300ms  

---

## Concurrency Test Cases (10 Cases)

### TC-NM-C001: Concurrent Get Notifications - Same User
**Concurrency Scenario**: 10 threads query same user's notifications  
**Test Steps**:
1. Spawn 10 threads for same user
2. All call GetNotifications simultaneously
3. Verify consistent results

**Expected Result**: All return same notification list  
**Load**: 10 concurrent queries, same user  

---

### TC-NM-C002: Concurrent Mark As Read - Different Notifications
**Concurrency Scenario**: 20 threads mark different notifications as read  
**Test Steps**:
1. Spawn 20 threads with different notification IDs
2. All mark as read simultaneously
3. Verify all marked successfully

**Expected Result**: All 20 notifications marked read  
**Load**: 20 concurrent mark-as-read operations  

---

### TC-NM-C003: Concurrent Mark As Read - Same Notification
**Concurrency Scenario**: 5 threads mark same notification as read  
**Test Steps**:
1. 5 threads mark notification 100 as read
2. Execute simultaneously
3. Verify idempotent behavior

**Expected Result**: Notification marked read once, no conflicts  
**Load**: 5 concurrent operations on same notification  

---

### TC-NM-C004: Create Notifications During Query
**Concurrency Scenario**: Creating notifications while user queries list  
**Test Steps**:
1. Thread 1 creates 50 notifications for user
2. Thread 2 repeatedly queries notifications
3. Verify consistent query results

**Expected Result**: Queries always return valid state  
**Load**: 50 creates + continuous queries  

---

### TC-NM-C005: Concurrent Updates - Same Notification
**Concurrency Scenario**: 3 threads update same notification  
**Test Steps**:
1. 3 threads update notification 100 with different messages
2. Execute simultaneously
3. Verify one update wins, no data corruption

**Expected Result**: Consistent final state  
**Load**: 3 concurrent updates  

---

### TC-NM-C006: Bulk Create for Multiple Users
**Concurrency Scenario**: 10 threads create notifications for different users  
**Test Steps**:
1. 10 threads each create 100 notifications
2. Each thread targets different user
3. Verify all 1000 notifications created

**Expected Result**: 1000 total notifications created  
**Load**: 10 threads × 100 notifications  

---

### TC-NM-C007: Concurrent Read Status Changes
**Concurrency Scenario**: User reading notifications while system marks them as read  
**Test Steps**:
1. Thread 1 queries unread notifications
2. Thread 2 marks notifications as read during query
3. Verify consistent behavior

**Expected Result**: Query returns snapshot, no errors  
**Load**: Concurrent read and status change  

---

### TC-NM-C008: Concurrent Notification Creation - Same User
**Concurrency Scenario**: 15 threads create notifications for same user  
**Test Steps**:
1. 15 threads create notifications for user 500
2. Execute simultaneously
3. Verify all notifications created with unique IDs

**Expected Result**: All 15 created successfully  
**Load**: 15 concurrent creates for same user  

---

### TC-NM-C009: Concurrent Category Filtering
**Concurrency Scenario**: Multiple threads filter by different categories  
**Test Steps**:
1. 8 threads query notifications with different category filters
2. Execute simultaneously
3. Verify correct results for each category

**Expected Result**: All filtered queries return correct data  
**Load**: 8 concurrent category queries  

---

### TC-NM-C010: Concurrent Record Data Parsing
**Concurrency Scenario**: Multiple threads parsing complex record data  
**Test Steps**:
1. 20 threads get notifications with complex record JSON
2. All parse record data simultaneously
3. Verify no parsing errors

**Expected Result**: All parse successfully  
**Load**: 20 concurrent parse operations  

---

## Edge Cases (10 Cases)

### TC-NM-E001: Notification With Null Message
**Description**: Create notification with null message  
**Test Steps**:
1. Create notification with Message = null
2. Verify handled gracefully

**Expected Result**: Notification created or validation error  

---

### TC-NM-E002: Record Data - Malformed JSON
**Description**: Notification with invalid JSON in RecordData  
**Test Steps**:
1. Create notification with RecordData = "{invalid json"
2. Call ParseRecordData
3. Verify returns raw string in list

**Expected Result**: Returns List<object> with raw string  

---

### TC-NM-E003: Mark As Read - Different User Attempting
**Description**: User A tries to mark User B's notification as read  
**Test Steps**:
1. User A calls MarkAsRead for notification owned by User B
2. Verify security check (notification not found)

**Expected Result**: Notification not marked (security)  

---

### TC-NM-E004: Notification With Very Long Message
**Description**: Create notification with 10,000 character message  
**Test Steps**:
1. Create notification with maximum length message
2. Verify stored and retrieved correctly

**Expected Result**: Long message handled  

---

### TC-NM-E005: Get Notifications - User With 10,000 Notifications
**Description**: Query notifications for very active user  
**Test Steps**:
1. User has 10,000 notifications
2. Query unread only
3. Verify performance acceptable

**Expected Result**: Query completes successfully  

---

### TC-NM-E006: Record Data - Empty Array vs Null
**Description**: Distinguish between null and empty array in record data  
**Test Steps**:
1. Create notification with RecordData = null
2. Create notification with RecordData = "[]"
3. Verify both parsed correctly

**Expected Result**: Both return empty list  

---

### TC-NM-E007: Notification Status - All Enum Values
**Description**: Test all NotificationStatus enum values  
**Test Steps**:
1. Create notifications with each status value
2. Update notification to each status
3. Verify all statuses handled

**Expected Result**: All status values work  

---

### TC-NM-E008: Mark As Read - Already Deleted Notification
**Description**: Mark as read a soft-deleted notification  
**Test Steps**:
1. Soft delete notification
2. Attempt to mark as read
3. Verify handled gracefully

**Expected Result**: Operation handles deletion  

---

### TC-NM-E009: Concurrent Mark As Read - Race Condition
**Description**: Race condition marking same notification from multiple sessions  
**Test Steps**:
1. User has two browser sessions
2. Both mark same notification simultaneously
3. Verify no errors, consistent state

**Expected Result**: Idempotent operation, no errors  

---

### TC-NM-E010: Record Data - Nested Complex Objects
**Description**: Parse notification with deeply nested JSON  
**Test Steps**:
1. Create notification with 10-level nested JSON object
2. Call ParseRecordData
3. Verify parses without stack overflow

**Expected Result**: Nested JSON parsed successfully  

---


