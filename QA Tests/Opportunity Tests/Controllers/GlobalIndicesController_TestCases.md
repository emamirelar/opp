# GlobalIndicesController Test Cases

**Controller:** `GlobalIndicesController`  
**Test Count:** 7+  
**Priority:** P2  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-GICTL-001: POST /api/global-indices/upload
**Priority:** P2  
**Expected**: 201 Created, CSV processed, all countries updated

### TC-OPP-GICTL-002: GET /api/global-indices/current
**Priority:** P2  
**Expected**: 200 OK, current indices for all countries

### TC-OPP-GICTL-003: GET /api/global-indices/{countryId}/history
**Priority:** P2  
**Expected**: 200 OK, historical indices, trend data

### TC-OPP-GICTL-004: GET /api/global-indices/as-at/{date}
**Priority:** P2  
**Expected**: 200 OK, indices as they were at specified date

### TC-OPP-GICTL-005: PUT /api/global-indices/{id}/retire
**Priority:** P2  
**Expected**: 200 OK, index marked retired, no longer shown

### TC-OPP-GICTL-006: GET /api/global-indices/dashboard
**Priority:** P2  
**Expected**: 200 OK, summary statistics, coverage, data quality

### TC-OPP-GICTL-007: POST /api/global-indices/export
**Priority:** P2  
**Expected**: 200 OK Excel, comprehensive export, all data

---

**Status:** ✅ Ready for Implementation
