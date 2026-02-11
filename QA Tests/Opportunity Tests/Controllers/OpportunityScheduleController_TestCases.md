# OpportunityScheduleController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityScheduleController`  
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

REST API for opportunity schedule/timeline: CRUD milestones, Gantt data, dependencies, critical path, date validation, duration calculation, progress tracking, baseline comparison, and export.

---

## §1–§10

**§1 (35):** GET schedule (P0), POST milestone (P0), PUT update (P0), DELETE milestone (P0), GET Gantt data (P0), + 30 (dependencies, critical path, progress, baseline, export, search, filter, pagination, sort, audit, permissions, bulk add, duration calc, date validation, overlap check, resource linking, model map, typeahead, count, clone, template, lock, history, notification, variance, forecast, completeness, summary, PDF).

**§2 (70):** Input (null oppId, non-existent, invalid dates, end<start, null name, negative duration), Auth (10), State (edit closed, locked, approved), HTTP (10), injection (10), dependencies (10), format/ID (10), business (circular dependency, self-dependency, impossible dates, overlap violations, max milestones, orphan dependency, invalid progress %, resource conflict, date paradox, mass assignment).

**§3 (70):** Dates (today/past/future/far-future/leap/midnight/year-boundary), duration (0/1/30/365/1000 days), milestones (0/1/10/50/100/101), dependencies (0/1/5/20), progress (0/1/50/99/100/101%), name lengths, concurrent, Unicode, pagination, Gantt complexity, critical path depth, baseline comparisons, resource count.

**§4 (50):** Date logic (15), dependency management (10), critical path (10), progress tracking (10), audit (5).
**§5 (50):** Opportunity service (10), resource (10), Gantt rendering (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), schedule manipulation (10), data integrity (10).
**§7 (25):** Concurrent edits, dependency updates, progress + edit, baseline + update, bulk operations.
**§8 (21):** Date calculation (5), critical path (5), duration (3), dependency validation (5), progress (3).
**§9 (16):** GET (<200ms), Gantt (<500ms), critical path (<500ms), create (<300ms), export (<3s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
