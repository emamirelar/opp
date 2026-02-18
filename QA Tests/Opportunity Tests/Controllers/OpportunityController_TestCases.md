# OpportunityController Test Cases

**Controller:** `OpportunityController`  
**Test Count:** 12+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Overview

API endpoint tests for opportunity CRUD operations, filtering, and status updates.

---

## Test Cases

### TC-OPP-CTRL-001: GET /api/opportunities
**Priority:** P1  
**Expected Results:**
- 200 OK with paginated list
- Includes filtering parameters
- Row-level security applied
- Response time < 500ms

---

### TC-OPP-CTRL-002: GET /api/opportunities/{id}
**Priority:** P1  
**Expected Results:**
- 200 OK with opportunity details
- 404 if not found
- 403 if no permission
- Includes related entities if requested

---

### TC-OPP-CTRL-003: POST /api/opportunities
**Priority:** P1  
**Expected Results:**
- 201 Created with location header
- 400 if validation fails
- 403 if no permission
- Audit trail created

---

### TC-OPP-CTRL-004: PUT /api/opportunities/{id}
**Priority:** P1  
**Expected Results:**
- 200 OK with updated entity
- 404 if not found
- 409 if concurrency conflict
- Versioning handled

---

### TC-OPP-CTRL-005: DELETE /api/opportunities/{id}
**Priority:** P1  
**Expected Results:**
- 204 No Content
- 404 if not found
- Soft delete applied
- Related entities handled

---

### TC-OPP-CTRL-006: PUT /api/opportunities/{id}/status
**Priority:** P1  
**Expected Results:**
- 200 OK
- Validates state transitions
- Creates history entry
- Notifications triggered

---

### TC-OPP-CTRL-007: POST /api/opportunities/{id}/convert-to-project
**Priority:** P1  
**Expected Results:**
- 200 OK with project details
- 400 if not approved
- Creates project entity
- Links opportunity to project

---

### TC-OPP-CTRL-008: GET /api/opportunities/by-status/{status}
**Priority:** P1  
**Expected Results:**
- 200 OK with filtered list
- Validates status parameter
- Applies pagination

---

### TC-OPP-CTRL-009: GET /api/opportunities/by-org-unit/{orgUnitId}
**Priority:** P1  
**Expected Results:**
- 200 OK with filtered list
- Includes child org units if requested
- Permission check applied

---

### TC-OPP-CTRL-010: POST /api/opportunities/{id}/ai-suggestions
**Priority:** P1  
**Expected Results:**
- 200 OK with suggestions
- AI service called
- Results cached
- Timeout handling

---

### TC-OPP-CTRL-011: Authorization Header Required
**Priority:** P1  
**Expected Results:**
- 401 Unauthorized if no token
- Token validation
- Claims extracted

---

### TC-OPP-CTRL-012: Content-Type Validation
**Priority:** P2  
**Expected Results:**
- Accepts application/json
- Rejects invalid content types
- Returns appropriate error

---

**Status:** ✅ Ready for Implementation
