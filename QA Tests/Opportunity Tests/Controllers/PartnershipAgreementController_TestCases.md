# PartnershipAgreementController Test Cases

**Controller:** `PartnershipAgreementController`  
**Test Count:** 7+  
**Priority:** P1  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-PACTL-001: POST /api/partnership-agreements/upload
**Priority:** P1  
**Expected**: 201 Created, agreement stored, terms extracted

### TC-OPP-PACTL-002: GET /api/partnership-agreements/{id}
**Priority:** P1  
**Expected**: 200 OK, agreement details and extracted terms

### TC-OPP-PACTL-003: GET /api/partnership-agreements/by-partner/{partnerId}
**Priority:** P1  
**Expected**: 200 OK, all agreements for partner, sorted by date

### TC-OPP-PACTL-004: PUT /api/partnership-agreements/{id}
**Priority:** P1  
**Expected**: 200 OK, metadata updated, version incremented

### TC-OPP-PACTL-005: GET /api/partnership-agreements/{id}/opportunities
**Priority:** P1  
**Expected**: 200 OK, linked opportunities, utilization shown

### TC-OPP-PACTL-006: POST /api/partnership-agreements/search
**Priority:** P1  
**Expected**: 200 OK, full-text search results, filters applied

### TC-OPP-PACTL-007: GET /api/partnership-agreements/expiring
**Priority:** P2  
**Expected**: 200 OK, agreements expiring within 90 days

---

**Status:** ✅ Ready for Implementation
