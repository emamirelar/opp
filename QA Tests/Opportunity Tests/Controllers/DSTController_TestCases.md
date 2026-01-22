# DSTController Test Cases

**Controller:** `DSTController`  
**Test Count:** 10+  
**Priority:** P1 (High)  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-DST-CTRL-001: POST /api/dst/profile
**Priority:** P1  
**Expected**: 201 Created, profile generated, async processing if needed

### TC-OPP-DST-CTRL-002: GET /api/dst/profile/{opportunityId}
**Priority:** P1  
**Expected**: 200 OK with profile, 404 if not generated

### TC-OPP-DST-CTRL-003: PUT /api/dst/profile/{id}/regenerate
**Priority:** P1  
**Expected**: 200 OK, new version created, changes highlighted

### TC-OPP-DST-CTRL-004: GET /api/dst/profile/{id}/recommendations
**Priority:** P1  
**Expected**: 200 OK with recommendations list, prioritized

### TC-OPP-DST-CTRL-005: PUT /api/dst/recommendations/{id}/accept
**Priority:** P1  
**Expected**: 200 OK, recommendation marked accepted, action recorded

### TC-OPP-DST-CTRL-006: PUT /api/dst/recommendations/{id}/reject
**Priority:** P1  
**Expected**: 200 OK, reason required, feedback captured

### TC-OPP-DST-CTRL-007: GET /api/dst/similar-opportunities/{opportunityId}
**Priority:** P1  
**Expected**: 200 OK with matches, similarity scores, lessons learned

### TC-OPP-DST-CTRL-008: GET /api/dst/profile/{id}/report
**Priority:** P2  
**Expected**: 200 OK PDF stream, proper headers, formatted report

### TC-OPP-DST-CTRL-009: GET /api/dst/profile/{id}/executive-summary
**Priority:** P2  
**Expected**: 200 OK PDF, 1-page summary, key findings

### TC-OPP-DST-CTRL-010: GET /api/dst/parameters/{opportunityId}
**Priority:** P1  
**Expected**: 200 OK, all 9 parameters with scores, color coding

---

**Status:** ✅ Ready for Implementation
