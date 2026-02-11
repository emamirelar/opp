# OpportunityScheduleManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OpportunityScheduleManager`  
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

Schedule/timeline management: milestone CRUD, dependencies, critical path calculation, Gantt data, progress tracking, baseline comparison, date validation, duration calculation, and export.

---

## §1–§10

**§1 (35):** CRUD milestones + dependencies + critical path + Gantt + progress + baseline + export (35 tests).
**§2 (70):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (circular deps, self-dep, impossible dates, overlap, max milestones, orphan deps, invalid progress, resource conflict, date paradox, mass assignment).
**§3 (70):** Dates, durations (0–1000 days), milestones (0–100+), dependencies (0–20), progress (0–100%), name lengths, concurrent, Unicode, pagination, Gantt complexity, critical path depth, baseline comparisons.
**§4 (50):** Date logic (15), dependencies (10), critical path (10), progress (10), audit (5).
**§5 (50):** Opportunity (10), resource (10), Gantt (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), schedule manipulation (10), data integrity (10).
**§7 (25):** Concurrent edits, dependency updates, progress + edit, baseline + update, bulk.
**§8 (21):** Date calc (5), critical path (5), duration (3), dependency validation (5), progress (3).
**§9 (16):** CRUD (<200ms), Gantt (<500ms), critical path (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
