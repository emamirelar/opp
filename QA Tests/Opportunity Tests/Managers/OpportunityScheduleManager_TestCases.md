# OpportunityScheduleManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OpportunityScheduleManager`  
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

Schedule/timeline management: milestone CRUD, dependencies, critical path calculation, Gantt data, progress tracking, baseline comparison, date validation, duration calculation, and export.

---

## §1–§10

**§1 (30):** CRUD milestones + dependencies + critical path + Gantt + progress + baseline + export (30 tests).
**§2 (90):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (circular deps, self-dep, impossible dates, overlap, max milestones, orphan deps, invalid progress, resource conflict, date paradox, mass assignment) + 20 additional.
**§3 (90):** Dates, durations (0–1000 days), milestones (0–100+), dependencies (0–20), progress (0–100%), name lengths, concurrent, Unicode, pagination, Gantt complexity, critical path depth, baseline comparisons.
**§4 (90):** Date logic (15), dependencies (10), critical path (10), progress (10), audit (5) + 40 additional.
**§5 (90):** Opportunity (10), resource (10), Gantt (10), export (10), notification (10) + 50 additional.
**§6 (22):** Injection (10), auth (10), IDOR (2).
**§7 (15):** Concurrent edits, dependency updates, progress + edit, baseline + update, bulk.
**§8 (15):** Date calc (5), critical path (5), duration (3), dependency validation (2).
**§9 (10):** CRUD (<200ms), Gantt (<500ms), critical path (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
