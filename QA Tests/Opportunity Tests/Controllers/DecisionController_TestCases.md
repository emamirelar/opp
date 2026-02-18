# DecisionController Test Cases

**Controller:** `DecisionController`  
**Test Count:** 10+  
**Priority:** P0-P1  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-DEC-CTRL-001: POST /api/decisions/package
**Priority:** P0  
**Expected**: 201 Created, decision package assembled, validation complete

### TC-OPP-DEC-CTRL-002: GET /api/decisions/package/{opportunityId}
**Priority:** P0  
**Expected**: 200 OK with complete package, all documents included

### TC-OPP-DEC-CTRL-003: POST /api/decisions
**Priority:** P0  
**Expected**: 201 Created, Go/No-Go recorded, authorization triggered

### TC-OPP-DEC-CTRL-004: PUT /api/decisions/{id}
**Priority:** P1  
**Expected**: 200 OK, update recorded, justification required

### TC-OPP-DEC-CTRL-005: GET /api/decisions/pending
**Priority:** P1  
**Expected**: 200 OK, user's pending decisions, prioritized by deadline

### TC-OPP-DEC-CTRL-006: POST /api/decisions/{id}/delegate
**Priority:** P1  
**Expected**: 200 OK, delegation recorded, delegate notified

### TC-OPP-DEC-CTRL-007: POST /api/decisions/{id}/escalate
**Priority:** P1  
**Expected**: 200 OK, escalation recorded, higher authority notified

### TC-OPP-DEC-CTRL-008: GET /api/decisions/{id}/audit-trail
**Priority:** P1  
**Expected**: 200 OK, complete history, suitable for compliance

### TC-OPP-DEC-CTRL-009: PUT /api/decisions/{id}/authorization
**Priority:** P0  
**Expected**: 200 OK, budget/personnel authorized, systems notified

### TC-OPP-DEC-CTRL-010: POST /api/decisions/compare
**Priority:** P2  
**Expected**: 200 OK, side-by-side comparison, ranking support

---

**Status:** ✅ Ready for Implementation
