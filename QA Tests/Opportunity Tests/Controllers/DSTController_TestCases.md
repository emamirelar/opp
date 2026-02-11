# DSTController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/DSTController`  
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

REST API for Decision Support Tool: get country profile, get indices (FSI, HDI, CPI, etc.), compare countries, get regional data, calculate composite score, get risk assessment, update index data, and historical trends.

---

## §1–§10

**§1 (35):** GET profile (P0), GET indices (P0), compare countries (P0), composite score (P0), risk assessment (P0), + 30 P1/P2 (regional data, historical, search, filter, pagination, sort, export, cache, refresh, metadata, supported indices, batch profiles, percentile, weight config, visualization data, audit, model map, typeahead, count, year selection, trend data, PDF report, chart data, available years, index detail, update index, bulk compare, region list, country list).

**§2 (70):** Input (null country, invalid ISO, non-existent, invalid index, null year), Auth (10), HTTP (10), injection (10), dependencies (external API down, timeout, stale data, quota, rate limit, cache failure, DB error, memory, service unavailable, PDF failure), format/ID (10), data quality (missing index, partial data, corrupted, future year, negative score, NaN, division by zero, invalid weights, weights sum, invalid region).

**§3 (70):** Country count (1/50/195), index values (0.0–1.0 ranges, min/max per index), scores (0-100 decimals), weights (0-100%), years (1990–2026), historical periods, comparison count, chart data points, Unicode names, pagination, search terms, response sizes, concurrent, cache sizes, regional aggregation, percentile boundaries.

**§4 (50):** Score calculation (15), risk mapping (10), data retrieval (10), caching (10), audit (5).
**§5 (50):** External indices API (10), DB (10), cache (10), PDF (10), opportunity linking (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API security (10).
**§7 (25):** Concurrent profiles, comparisons, cache refresh, score calculations, data updates.
**§8 (21):** Score calc (5), risk mapping (3), weighting (5), validation (5), formatting (3).
**§9 (16):** Single profile (<200ms), comparison (<500ms), historical (<1s), search (<500ms), batch (<2s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
