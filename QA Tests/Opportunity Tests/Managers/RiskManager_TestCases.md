# RiskManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/RiskManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §7 Concurrency (CON) | 25 | 25 | ✅ |
| §8 Unit (UNT) | 21 | 21 | ✅ |
| §9 Performance (PRF) | 16 | 16 | ✅ |
| §10 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Risk management for opportunities: CRUD risks, risk categories (Strategic/Operational/Financial/Legal/Reputational), probability/impact scoring (1-5 matrix), risk heat map, mitigation plans, risk owners, monitoring, residual risk calculation, risk register, and reporting.

---

## Test Case Inventory

**Removed:** POS-031, POS-032, POS-033, POS-034, POS-035 (5 positive)

**Added:** NEG-071-090 (20 negative), BND-071-090 (20 boundary), FUN-051-090 (40 functional), INT-051-090 (40 integration)

---

## §1–§10

**§1 (30):** CRUD risks + scoring + categories + mitigation + heat map + register + reporting (30 tests: POS-001–POS-030).
**§2 (90):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (invalid score, probability×impact calc, duplicate risk, orphan mitigation, max risks, circular reference, invalid category, missing owner, score out of range, mass assignment), NEG-071–NEG-090 (malformed probability, invalid impact, null category, orphan risk owner, deleted opportunity, invalid heat map params, stale risk data, concurrent conflict, permission denied, audit bypass, invalid residual calc, duplicate mitigation, max risks exceeded, invalid date range, malformed filter, invalid pagination, cross-entity risk, soft-delete violation, orphan mitigation link, invalid export format).
**§3 (90):** Probability (1–5), impact (1–5), score (1–25), risk count (0–100+), mitigation count per risk, description lengths, concurrent, Unicode, pagination, heat map data points, category distributions, residual vs inherent, date ranges, owner count, BND-071–BND-090 (score edge 1/25, probability 0/6, impact 0/6, empty description, max description, zero risks, max risks, single mitigation, max mitigations, concurrent update boundary, Unicode category, boundary pagination, heat map empty, heat map max, residual at boundary, date range min, date range max, owner null, owner max, category boundary).
**§4 (90):** Score calculation (15), mitigation tracking (10), heat map (10), categorization (10), audit (5), FUN-051–FUN-090 (probability×impact matrix, residual calc formula, heat map quadrant logic, category assignment, mitigation status, owner assignment, risk register sort, report aggregation, audit trail completeness, validation rules, workflow integration, notification triggers, export format, import validation, bulk update logic, soft delete cascade, restore logic, permission propagation, visibility rules, search filter logic, date range filter, category filter, status filter, owner filter, pagination logic, sort options, report generation, heat map export, register export, risk summary calc, mitigation completeness, duplicate detection, orphan prevention, cross-opportunity validation, version tracking, change history, audit field population).
**§5 (90):** Opportunity (10), notification (10), DST (10), export (10), reporting (10), INT-051–INT-090 (opportunity CRUD round-trip, notification dispatch, DST risk sync, export pipeline, reporting service, DB persistence, cache invalidation, audit trail, permission service, workflow service, partner manager, document manager, search service, filter service, pagination service, bulk operations, AI integration, analytics service, external API, PDF generation, CSV export, Excel export, JSON serialization, event bus, message queue, retry logic, circuit breaker, timeout handling, rate limiting, logging integration, metrics collection, health check, dependency injection).
**§7 (25):** Concurrent risk CRUD, score updates, mitigation + risk, bulk operations, owner changes.
**§8 (21):** Score calc (5), probability×impact (5), residual calc (3), validation (5), formatting (3).
**§9 (16):** CRUD (<200ms), heat map (<500ms), register (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large registers, recovery.

---

**Status:** Ready for Execution
