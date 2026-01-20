# ResourcePlanController Test Cases

**Controller:** `ResourcePlanController`  
**Test Count:** 8+  
**Priority:** P1  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-RESCTRL-001: POST /api/resource-plan/generate
**Priority:** P1  
**Expected**: 201 Created, roles identified, FTE calculated

### TC-OPP-RESCTRL-002: GET /api/resource-plan/{opportunityId}
**Priority:** P1  
**Expected**: 200 OK, resource plan with roles and requirements

### TC-OPP-RESCTRL-003: PUT /api/resource-plan/{id}
**Priority:** P1  
**Expected**: 200 OK, plan updated, costs recalculated

### TC-OPP-RESCTRL-004: GET /api/resource-plan/{id}/availability
**Priority:** P1  
**Expected**: 200 OK, availability check results, conflicts flagged

### TC-OPP-RESCTRL-005: POST /api/resource-plan/{id}/reserve
**Priority:** P1  
**Expected**: 200 OK, resources reserved, tentative bookings created

### TC-OPP-RESCTRL-006: GET /api/resource-plan/{id}/skills-matrix
**Priority:** P2  
**Expected**: 200 OK, skills required vs available, gaps identified

### TC-OPP-RESCTRL-007: GET /api/resource-plan/{id}/org-chart
**Priority:** P2  
**Expected**: 200 OK, org chart visualization data

### TC-OPP-RESCTRL-008: GET /api/resource-plan/{id}/cost-breakdown
**Priority:** P1  
**Expected**: 200 OK, personnel costs by role and phase

---

**Status:** ✅ Ready for Implementation
