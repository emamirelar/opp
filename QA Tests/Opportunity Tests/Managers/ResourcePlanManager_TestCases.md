# ResourcePlanManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ResourcePlanManager`  
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

Resource planning business logic: CRUD resources, role/skill requirements, allocation %, cost estimation, availability checks, team mapping, gap analysis, utilization reporting, and forecasting.

---

## §1–§10

**§1 (35):** CRUD + allocation + cost + availability + gap analysis + utilization + forecast (35 tests).
**§2 (70):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (10).
**§3 (70):** Allocation (0–100%), cost (0–MAX), resources (0–100), skills, FTE (0.0–1.0), concurrent, pagination, Unicode, durations, gap sizes, forecast periods, utilization boundaries.
**§4 (50):** Allocation logic (15), cost calc (10), availability (10), gap analysis (10), audit (5).
**§5 (50):** Team (10), budget (10), schedule (10), HR (10), export (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), cost data (10), resource data (10).
**§7 (25):** Concurrent allocations, cost updates, availability, bulk, lock conflicts.
**§8 (21):** Cost calc (5), allocation (5), availability (3), gap (5), formatting (3).
**§9 (16):** CRUD (<200ms), calc (<300ms), search (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large plans, recovery.

---

**Status:** Ready for Execution
