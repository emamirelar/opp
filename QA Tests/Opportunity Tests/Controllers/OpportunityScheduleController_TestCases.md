# OpportunityScheduleController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityScheduleController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

REST API for opportunity schedule/timeline: CRUD milestones, Gantt data, dependencies, critical path, date validation, duration calculation, progress tracking, baseline comparison, and export.

---

## §1 Positive — 30

| ID | Test | Endpoint | Expected | Pr |
|----|------|----------|----------|----|
| POS-001 | GET schedule | GET /schedule/{oppId} | 200, schedule data | P0 |
| POS-002 | POST milestone | POST /schedule/{oppId}/milestones | 201, milestone created | P0 |
| POS-003 | PUT update milestone | PUT /schedule/{oppId}/milestones/{id} | 200, milestone updated | P0 |
| POS-004 | DELETE milestone | DELETE /schedule/{oppId}/milestones/{id} | 200, milestone deleted | P0 |
| POS-005 | GET Gantt data | GET /schedule/{oppId}/gantt | 200, Gantt data | P0 |
| POS-006–030 | Dependencies, critical path, progress, baseline, export, search, filter, pagination, sort, audit, permissions, bulk add, duration calc, date validation, overlap check, resource linking, model map, typeahead, count, clone, template, lock, history, notification, variance, forecast, completeness, summary, PDF | Various | 200/201 responses | P1–P2 |

## §2 Negative — 90

NEG-001–070: Input (null oppId, non-existent, invalid dates, end<start, null name, negative duration), Auth (10), State (edit closed, locked, approved), HTTP (10), injection (10), dependencies (10), format/ID (10), business (circular dependency, self-dependency, impossible dates, overlap violations, max milestones, orphan dependency, invalid progress %, resource conflict, date paradox, mass assignment).

NEG-071–090: Invalid milestone ID, invalid dependency target, malformed milestone payload, missing required fields, invalid date format, soft-deleted opportunity, invalid duration, negative progress, invalid baseline ref, duplicate milestone name, expired baseline window, invalid parent milestone ID, cross-opportunity dependency leak, invalid pagination params, invalid sort field, invalid filter combination, bulk limit exceeded, template not found, clone source deleted, lock conflict.

## §3 Boundary — 90

BND-001–070: Dates (today/past/future/far-future/leap/midnight/year-boundary), duration (0/1/30/365/1000 days), milestones (0/1/10/50/100/101), dependencies (0/1/5/20), progress (0/1/50/99/100/101%), name lengths, concurrent, Unicode, pagination, Gantt complexity, critical path depth, baseline comparisons, resource count.

BND-071–090: Min duration step, max duration sum, zero-progress edge, max decimal precision, empty milestone list, single milestone, max milestones per schedule, min/max date range, same-day milestone, year boundary, leap year dates, max dependency count, empty schedule, single dependency, max pagination page, pageSize=1, pageSize=max, sort by nullable field, filter empty result, concurrent identical update.

## §4 Functional — 90

FUN-001–050: Date logic (15), dependency management (10), critical path (10), progress tracking (10), audit (5).

FUN-051–090: Date validation rule order, dependency cycle detection, critical path calculation accuracy, progress rollup formula, baseline comparison logic, duration calculation consistency, overlap detection, resource conflict check, variance delta calculation, bulk add atomicity, template application, clone deep copy, lock release on save, history versioning, notification trigger, permission inheritance, model mapping completeness, typeahead filtering, count aggregation, export format, milestone conflict detection, schedule completeness check, progress rounding, date format handling, audit trail integrity, soft-delete propagation, workflow status sync, date range validation, duration unit conversion, forecast extrapolation.

## §5 Integration — 90

INT-001–050: Opportunity service (10), resource (10), Gantt rendering (10), export (10), notification (10).

INT-051–090: Opportunity service round-trip, resource service sync, Gantt rendering pipeline, export service, notification service, audit service, permission service, manager delegation, DbContext scope, transaction boundary, cache invalidation, external API fallback, mapper chain, validation pipeline, error propagation, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution, schedule manager, milestone service, dependency service, baseline service.

## §6–§10

**§6 (50):** Injection (10), auth (10), IDOR (10), schedule manipulation (10), data integrity (10).
**§7 (25):** Concurrent edits, dependency updates, progress + edit, baseline + update, bulk operations.
**§8 (21):** Date calculation (5), critical path (5), duration (3), dependency validation (5), progress (3).
**§9 (16):** GET (<200ms), Gantt (<500ms), critical path (<500ms), create (<300ms), export (<3s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
