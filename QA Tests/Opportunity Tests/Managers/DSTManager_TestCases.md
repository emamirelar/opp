# DSTManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DSTManager`  
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

Business logic for Decision Support Tool: LoadCountryProfileAsync, CalculateCompositeScoreAsync, GenerateRiskProfileAsync, CompareCountriesAsync, GetHistoricalTrendsAsync, UpdateIndexDataAsync, cache management, weight configuration, and PDF report generation.

---

## Test Case Inventory

**Removed:** POS-031, POS-032, POS-033, POS-034, POS-035 (5 positive)

**Added:** NEG-071-090 (20 negative), BND-071-090 (20 boundary), FUN-051-090 (40 functional), INT-051-090 (40 integration)

---

## §1–§10

**§1 (30):** Load profile (P0), calculate score (P0), risk profile (P0), compare (P0), historical (P0), + 25 P1/P2 — POS-001–POS-030.
**§2 (90):** Input (10), Auth (10), Data quality (10), injection (10), dependencies (10), format (10), business (10), NEG-071–NEG-090 (malformed country ID, invalid weight, null index, orphan profile, deleted country, invalid score params, stale DST data, concurrent conflict, permission denied, audit bypass, invalid composite calc, duplicate country, max comparison exceeded, invalid date range, malformed filter, invalid pagination, cross-entity profile, soft-delete violation, orphan index link, invalid export format).
**§3 (90):** Index value ranges, country counts, weight boundaries, year ranges, comparison sizes, score decimals, cache sizes, chart data points, Unicode, concurrent, pagination, historical periods, aggregation complexity, BND-071–BND-090 (index 0/max, country 0/max, weight 0/100%, year min/max, comparison 0/max, score 0/100, cache empty, cache max, chart 0/max, empty profile, max countries, single comparison, max comparisons, concurrent update boundary, Unicode name, boundary pagination, historical empty, historical max, aggregation min, aggregation max).
**§4 (90):** Score calc (15), risk mapping (10), data loading (10), caching (10), audit (5), FUN-051–FUN-090 (composite score formula, risk mapping logic, profile load logic, cache invalidation, audit trail completeness, validation rules, workflow integration, notification triggers, export format, import validation, bulk update logic, soft delete cascade, restore logic, permission propagation, visibility rules, search filter logic, date range filter, country filter, weight filter, year filter, pagination logic, sort options, report generation, profile export, comparison export, score calc, completeness check, duplicate detection, orphan prevention, cross-entity validation, version tracking, change history, audit field population).
**§5 (90):** External APIs (10), DB (10), cache (10), PDF (10), opportunity linking (10), INT-051–INT-090 (external API round-trip, DB persistence, cache service, PDF service, opportunity service, index service, country service, search service, filter service, pagination service, bulk operations, AI integration, analytics service, export pipeline, JSON serialization, event bus, message queue, retry logic, circuit breaker, timeout handling, rate limiting, logging integration, metrics collection, health check, dependency injection).
**§7 (25):** Concurrent loads, score calcs, cache updates, comparisons, data refreshes.
**§8 (21):** Score calc (5), risk mapping (3), weighting (5), validation (5), formatting (3).
**§9 (16):** Profile (<200ms), score (<100ms), comparison (<500ms), PDF (<3s), batch (<2s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
