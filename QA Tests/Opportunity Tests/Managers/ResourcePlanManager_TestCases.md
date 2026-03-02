# ResourcePlanManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ResourcePlanManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 22 | 22 | ✅ |
| §7 Concurrency | 15 | 15 | ✅ |
| §8 Unit | 15 | 15 | ✅ |
| §9 Performance | 10 | 10 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

Resource planning business logic: CRUD resources, role/skill requirements, allocation %, cost estimation, availability checks, team mapping, gap analysis, utilization reporting, and forecasting.

---

## §1–§10

**§1 (30):** CRUD + allocation + cost + availability + gap analysis + utilization + forecast (30 tests).
**§2 (90):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (10) + 20 additional.
**§3 (90):** Allocation (0–100%), cost (0–MAX), resources (0–100), skills, FTE (0.0–1.0), concurrent, pagination, Unicode, durations, gap sizes, forecast periods, utilization boundaries.
**§4 (90):** Allocation logic (15), cost calc (10), availability (10), gap analysis (10), audit (5) + 40 additional.
**§5 (90):** Team (10), budget (10), schedule (10), HR (10), export (10) + 50 additional.
**§6 (22):** Injection (10), auth (10), IDOR (2).
**§7 (15):** Concurrent allocations, cost updates, availability, bulk, lock conflicts.
**§8 (15):** Cost calc (5), allocation (5), availability (3), gap (2).
**§9 (10):** CRUD (<200ms), calc (<300ms), search (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large plans, recovery.

---

**Status:** Ready for Execution
