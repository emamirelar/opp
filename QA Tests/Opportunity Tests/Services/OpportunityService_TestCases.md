# OpportunityService Test Cases

**Service:** `OpportunityService`  
**Test Count:** 10+  
**Priority:** P1-P2  
**Created:** January 13, 2026

---

## Overview

Service-layer business logic tests for opportunity orchestration, coordination, and integration.

---

## Test Cases

### TC-OPP-SVC-001: Coordinate Opportunity Creation Flow
**Priority:** P1  
**Test Steps:**
1. Create opportunity
2. Generate DST profile
3. Create risk register
4. Initialize budget
5. Verify coordination

**Expected Results:**
- All related entities created
- Transaction rollback if any step fails
- Audit trail complete
- User notified of progress

---

### TC-OPP-SVC-002: Orchestrate Status Change
**Priority:** P1  
**Test Steps:**
1. Status change requested
2. Validate transition
3. Update related entities
4. Trigger notifications

**Expected Results:**
- Status updated
- History recorded
- Stakeholders notified
- Dependent workflows triggered

---

### TC-OPP-SVC-003: Cache Frequently Accessed Data
**Priority:** P1  
**Test Steps:**
1. Get opportunity details (cold)
2. Get again (cached)
3. Measure performance improvement

**Expected Results:**
- First call: 200ms
- Cached call: <50ms
- Cache invalidated on update
- Proper TTL applied

---

### TC-OPP-SVC-004: Coordinate Decision Package Assembly
**Priority:** P1  
**Test Steps:**
1. Gather all components
2. Validate completeness
3. Generate package
4. Handle missing pieces

**Expected Results:**
- All documents retrieved
- Validation checks passed
- Package assembled efficiently
- Error handling graceful

---

### TC-OPP-SVC-005: Batch Operations
**Priority:** P2  
**Test Steps:**
1. Bulk update opportunities
2. Verify transaction handling

**Expected Results:**
- All or nothing semantics
- Performance optimized
- Audit trails for all
- Progress reporting

---

### TC-OPP-SVC-006: External System Integration
**Priority:** P2  
**Test Steps:**
1. Opportunity approved
2. Sync to ERP system
3. Sync to project management
4. Verify integration

**Expected Results:**
- Successful sync
- Retry logic for failures
- Data mapping correct
- Error logging comprehensive

---

### TC-OPP-SVC-007: Background Job Processing
**Priority:** P1  
**Test Steps:**
1. Schedule DST regeneration
2. Process asynchronously
3. Verify completion

**Expected Results:**
- Job queued
- Processed in background
- Status trackable
- Notification on completion

---

### TC-OPP-SVC-008: Search and Filter
**Priority:** P1  
**Test Steps:**
1. Complex search query
2. Multiple filters
3. Verify results

**Expected Results:**
- Full-text search works
- Filters applied correctly
- Performance acceptable
- Relevance scoring

---

### TC-OPP-SVC-009: Data Export
**Priority:** P2  
**Test Steps:**
1. Export opportunities to Excel
2. Verify data completeness

**Expected Results:**
- All data exported
- Formatting preserved
- Performance good even for large datasets
- Can import back

---

### TC-OPP-SVC-010: Error Handling and Resilience
**Priority:** P1  
**Test Steps:**
1. Simulate external service failure
2. Verify graceful degradation

**Expected Results:**
- Circuit breaker pattern
- Fallback behavior
- Error logged
- User informed appropriately

---

**Status:** ✅ Ready for Implementation
