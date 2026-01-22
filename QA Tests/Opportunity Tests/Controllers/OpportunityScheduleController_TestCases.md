# OpportunityScheduleController Test Cases

**Controller:** `OpportunityScheduleController`  
**Test Count:** 8+  
**Priority:** P1  
**Created:** January 13, 2026

---

## Test Cases

### TC-OPP-SCHCTRL-001: POST /api/opportunity-schedule/generate
**Priority:** P1  
**Expected**: 201 Created, schedule generated, WBS created

### TC-OPP-SCHCTRL-002: GET /api/opportunity-schedule/{opportunityId}
**Priority:** P1  
**Expected**: 200 OK, schedule with phases and milestones

### TC-OPP-SCHCTRL-003: PUT /api/opportunity-schedule/{id}
**Priority:** P1  
**Expected**: 200 OK, schedule updated, dependencies validated

### TC-OPP-SCHCTRL-004: GET /api/opportunity-schedule/{id}/wbs
**Priority:** P1  
**Expected**: 200 OK, hierarchical WBS structure

### TC-OPP-SCHCTRL-005: GET /api/opportunity-schedule/{id}/milestones
**Priority:** P1  
**Expected**: 200 OK, milestone list with dates and status

### TC-OPP-SCHCTRL-006: GET /api/opportunity-schedule/{id}/gantt
**Priority:** P2  
**Expected**: 200 OK, Gantt chart data, dependencies shown

### TC-OPP-SCHCTRL-007: GET /api/opportunity-schedule/{id}/critical-path
**Priority:** P2  
**Expected**: 200 OK, critical path identified, float calculated

### TC-OPP-SCHCTRL-008: POST /api/opportunity-schedule/{id}/export
**Priority:** P2  
**Expected**: 200 OK, MS Project compatible export

---

**Status:** ✅ Ready for Implementation
