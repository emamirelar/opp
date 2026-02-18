# ResourcePlanController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/ResourcePlanController`  
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

**3:1 Ratio Checks:**

| Check | Result |
|-------|--------|
| N≥3P | 90≥90 ✅ |
| E≥3P | 90≥90 ✅ |
| F≥3P | 90≥90 ✅ |
| I≥3P | 90≥90 ✅ |

---

## Feature Overview

REST API for opportunity resource planning: CRUD resource items, role/skill requirements, allocation percentages, cost estimation, resource availability, team mapping, and reporting.

---

## §1–§10

**§1 (30):** GET plan (P0), POST resource (P0), PUT update (P0), DELETE resource (P0), GET summary (P0), + 25 (role requirements, skill matching, allocation %, cost estimate, availability check, team mapping, export, search, filter, pagination, sort, audit, permissions, bulk add, template, clone, lock, history, notification, validation, forecast, variance, model map, typeahead, count, gap analysis, utilization report).

**§2 (90):** NEG-001–070: Input (null oppId, non-existent, invalid role, negative allocation, null skill, invalid cost), Auth (10), State (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (over-allocation, skill mismatch, budget exceed, resource conflict, circular dependency, orphan resource, max resources, duplicate, availability conflict, mass assignment). NEG-071–090: Invalid role ID, invalid skill ID, malformed allocation payload, missing required fields, invalid date range, soft-deleted opportunity, invalid FTE, negative cost, invalid resource type, duplicate resource assignment, expired availability window, invalid team ID, cross-opportunity resource leak, invalid pagination params, invalid sort field, invalid filter combination, bulk limit exceeded, template not found, clone source deleted, lock conflict.

**§3 (90):** BND-001–070: Allocation (0/1/50/99/100/101%), cost (0.00–MAX), resources (0/1/10/50/100), skills per resource, roles, date ranges, FTE values (0.0–1.0), concurrent, pagination, Unicode, duration, utilization calculations, gap sizes, forecast periods. BND-071–090: Min allocation step, max allocation sum, zero-cost edge, max decimal precision, empty skill list, single skill, max skills per resource, min/max date range, same-day allocation, year boundary, leap year dates, max resource count, empty plan, single resource, max pagination page, pageSize=1, pageSize=max, sort by nullable field, filter empty result, concurrent identical update.

**§4 (90):** FUN-001–050: Allocation logic (15), cost calculation (10), availability (10), gap analysis (10), audit (5). FUN-051–090: Role-to-skill mapping, allocation sum validation, cost rollup accuracy, availability overlap check, utilization formula, gap severity classification, forecast extrapolation, variance delta calculation, bulk add atomicity, template application, clone deep copy, lock release on save, history versioning, notification trigger, validation rule order, permission inheritance, model mapping completeness, typeahead filtering, count aggregation, export format, resource conflict detection, team capacity check, skill gap reporting, allocation rounding, cost currency handling, audit trail integrity, soft-delete propagation, workflow status sync, date range validation, FTE conversion.

**§5 (90):** INT-001–050: Team service (10), budget (10), schedule (10), HR/skills (10), export (10). INT-051–090: Team service round-trip, budget service sync, schedule conflict check, HR skills API, export pipeline, opportunity context, partner linkage, workflow integration, notification service, audit service, permission service, manager delegation, DbContext scope, transaction boundary, cache invalidation, external API fallback, mapper chain, validation pipeline, error propagation, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution.

**§7 (25):** Concurrent allocations, cost updates, availability checks, bulk operations, lock conflicts.
**§8 (21):** Cost calc (5), allocation (5), availability (3), gap analysis (5), formatting (3).
**§9 (16):** GET (<200ms), calc (<300ms), search (<500ms), export (<3s), bulk (<2s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large plans, recovery.

---

**Status:** Ready for Execution
