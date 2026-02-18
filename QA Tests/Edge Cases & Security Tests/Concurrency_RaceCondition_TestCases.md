# Concurrency & Race Condition Test Cases

**Priority**: P1 - High  
**Total Test Cases**: 35  

---

## Overview

Concurrency tests covering:
- Simultaneous updates to same entity
- Optimistic locking conflicts
- Deadlock prevention
- Transaction isolation
- Counter/sequence integrity
- Cache consistency

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Optimistic Locking | 8 | P0 |
| Simultaneous Updates | 8 | P1 |
| Deadlock Prevention | 6 | P0 |
| Transaction Integrity | 6 | P1 |
| Counter Integrity | 4 | P1 |
| Cache Consistency | 3 | P2 |

---

## Optimistic Locking Tests

### TC-CON-001: Concurrent edit conflict detected
**Description**: Two users edit same record  
**Test Steps**:
1. User A reads partner (version 1)
2. User B reads partner (version 1)
3. User B saves (version 2)
4. User A tries save (version 1)
**Expected Result**: User A gets conflict error

### TC-CON-002: Version increment on save
**Description**: Version auto-increments  
**Test Steps**:
1. Read entity version
2. Update entity
3. Verify version + 1
**Expected Result**: Version incremented

### TC-CON-003: Stale data message
**Description**: User-friendly conflict message  
**Test Steps**:
1. Trigger conflict
2. Check error message
**Expected Result**: Clear message about stale data

### TC-CON-004: Refresh and retry option
**Description**: Can refresh and retry  
**Test Steps**:
1. Get conflict
2. Refresh data
3. Apply changes to new version
**Expected Result**: Save succeeds

### TC-CON-005: No conflict on same user
**Description**: Same user sequential edits  
**Test Steps**:
1. User edits and saves
2. User edits again and saves
**Expected Result**: No conflict

### TC-CON-006: Merge changes option
**Description**: Non-conflicting fields merge  
**Test Steps**:
1. User A edits field X
2. User B edits field Y
3. Both save
**Expected Result**: Both changes saved

### TC-CON-007: Conflict on same field
**Description**: Same field edited  
**Test Steps**:
1. Both users edit same field
2. Both save
**Expected Result**: Second gets conflict

### TC-CON-008: Force save override
**Description**: Admin force save  
**Test Steps**:
1. Admin saves despite conflict
**Expected Result**: Overwrites with warning

---

## Simultaneous Update Tests

### TC-CON-009: Concurrent partner creates
**Description**: 100 partners created simultaneously  
**Test Steps**:
1. 100 threads create partners
**Expected Result**: All created, unique IDs

### TC-CON-010: Concurrent contact creates
**Description**: 100 contacts created simultaneously  
**Test Steps**:
1. 100 threads create contacts
**Expected Result**: All created, no duplicates

### TC-CON-011: Concurrent updates same entity
**Description**: 10 updates same partner  
**Test Steps**:
1. 10 threads update same partner
**Expected Result**: Last write wins or conflicts

### TC-CON-012: Concurrent deletes
**Description**: Delete already deleted  
**Test Steps**:
1. Two threads delete same entity
**Expected Result**: One succeeds, one 404

### TC-CON-013: Concurrent create-delete
**Description**: Create while deleting related  
**Test Steps**:
1. Delete partner
2. Create contact for partner
**Expected Result**: FK constraint or proper error

### TC-CON-014: Concurrent association adds
**Description**: Add same association twice  
**Test Steps**:
1. Two threads add same contact to partner
**Expected Result**: One succeeds, one conflict

### TC-CON-015: Concurrent workflow transitions
**Description**: Two approvers approve same  
**Test Steps**:
1. Both click approve
**Expected Result**: One succeeds, one stale

### TC-CON-016: Concurrent document uploads
**Description**: Upload same filename  
**Test Steps**:
1. Two uploads same name
**Expected Result**: Both succeed with unique names

---

## Deadlock Prevention Tests

### TC-CON-017: No deadlock on update order
**Description**: A updates X then Y, B updates Y then X  
**Test Steps**:
1. Thread A: lock X, then Y
2. Thread B: lock Y, then X
**Expected Result**: No deadlock, one waits

### TC-CON-018: Lock timeout
**Description**: Don't wait forever  
**Test Steps**:
1. Create lock contention
2. Wait for timeout
**Expected Result**: Timeout error, not hang

### TC-CON-019: Retry after deadlock
**Description**: Automatic retry  
**Test Steps**:
1. Trigger deadlock
2. Verify retry attempt
**Expected Result**: Transaction retried

### TC-CON-020: Cascading lock acquisition
**Description**: Parent-child locking  
**Test Steps**:
1. Update partner with contacts
**Expected Result**: Consistent locking order

### TC-CON-021: Read-write lock separation
**Description**: Reads don't block reads  
**Test Steps**:
1. Multiple concurrent reads
**Expected Result**: All proceed

### TC-CON-022: Write blocks write
**Description**: Writes serialize  
**Test Steps**:
1. Concurrent writes same row
**Expected Result**: Serialized execution

---

## Transaction Integrity Tests

### TC-CON-023: Atomic multi-table update
**Description**: All or nothing  
**Test Steps**:
1. Update partner and contacts
2. Fail on contact 2
**Expected Result**: All rolled back

### TC-CON-024: Rollback on exception
**Description**: Error triggers rollback  
**Test Steps**:
1. Start transaction
2. Throw exception
**Expected Result**: No partial data

### TC-CON-025: Isolation level respected
**Description**: Read committed isolation  
**Test Steps**:
1. Transaction A reads
2. Transaction B updates
3. Transaction A reads again
**Expected Result**: Sees committed data

### TC-CON-026: Phantom read prevention
**Description**: Repeatable reads  
**Test Steps**:
1. Query returns 10 rows
2. Another insert occurs
3. Same query in transaction
**Expected Result**: Still 10 rows

### TC-CON-027: Dirty read prevention
**Description**: No uncommitted reads  
**Test Steps**:
1. Transaction A updates (uncommitted)
2. Transaction B reads
**Expected Result**: B sees old value

### TC-CON-028: Long transaction timeout
**Description**: Transaction timeout  
**Test Steps**:
1. Start long transaction
2. Wait for timeout
**Expected Result**: Rolled back

---

## Counter Integrity Tests

### TC-CON-029: Reference number unique
**Description**: Concurrent ref generation  
**Test Steps**:
1. 100 concurrent creates
**Expected Result**: All unique ref numbers

### TC-CON-030: Sequence gap handling
**Description**: Gaps acceptable  
**Test Steps**:
1. Rollback after sequence get
**Expected Result**: Gap in sequence OK

### TC-CON-031: Auto-increment thread safe
**Description**: ID generation  
**Test Steps**:
1. 1000 concurrent inserts
**Expected Result**: All unique IDs

### TC-CON-032: Counter rollback
**Description**: Failed insert doesn't consume  
**Test Steps**:
1. Insert fails after ID assigned
**Expected Result**: ID may be lost (gap)

---

## Cache Consistency Tests

### TC-CON-033: Cache invalidation on update
**Description**: Update clears cache  
**Test Steps**:
1. Read (cached)
2. Update
3. Read again
**Expected Result**: Fresh data

### TC-CON-034: Distributed cache sync
**Description**: Multi-server cache  
**Test Steps**:
1. Server A updates
2. Server B reads
**Expected Result**: B sees update

### TC-CON-035: Cache race condition
**Description**: Stale cache write  
**Test Steps**:
1. Read (miss, fetch)
2. Update in DB
3. First fetch completes
**Expected Result**: Cache has stale data briefly

---

**Last Updated**: December 18, 2025

