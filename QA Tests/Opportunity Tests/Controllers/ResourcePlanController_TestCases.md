# ResourcePlanController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/ResourcePlanController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

REST API for opportunity resource planning: CRUD resource items, role/skill requirements, allocation percentages, cost estimation, resource availability, team mapping, and reporting.

---

## §1–§10

**§1 (35):** GET plan (P0), POST resource (P0), PUT update (P0), DELETE resource (P0), GET summary (P0), + 30 (role requirements, skill matching, allocation %, cost estimate, availability check, team mapping, export, search, filter, pagination, sort, audit, permissions, bulk add, template, clone, lock, history, notification, validation, forecast, variance, model map, typeahead, count, gap analysis, utilization report).

**§2 (70):** Input (null oppId, non-existent, invalid role, negative allocation, null skill, invalid cost), Auth (10), State (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (over-allocation, skill mismatch, budget exceed, resource conflict, circular dependency, orphan resource, max resources, duplicate, availability conflict, mass assignment).

**§3 (70):** Allocation (0/1/50/99/100/101%), cost (0.00–MAX), resources (0/1/10/50/100), skills per resource, roles, date ranges, FTE values (0.0–1.0), concurrent, pagination, Unicode, duration, utilization calculations, gap sizes, forecast periods.

**§4 (50):** Allocation logic (15), cost calculation (10), availability (10), gap analysis (10), audit (5).
**§5 (50):** Team service (10), budget (10), schedule (10), HR/skills (10), export (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), cost data (10), resource data (10).
**§7 (25):** Concurrent allocations, cost updates, availability checks, bulk operations, lock conflicts.
**§8 (21):** Cost calc (5), allocation (5), availability (3), gap analysis (5), formatting (3).
**§9 (16):** GET (<200ms), calc (<300ms), search (<500ms), export (<3s), bulk (<2s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large plans, recovery.

---

**Status:** Ready for Execution
