# DSTController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/DSTController`  
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

REST API for Decision Support Tool: get country profile, get indices (FSI, HDI, CPI, etc.), compare countries, get regional data, calculate composite score, get risk assessment, update index data, and historical trends.

---

## §1–§10

**§1 (30):** GET profile (P0), GET indices (P0), compare countries (P0), composite score (P0), risk assessment (P0), + 25 (regional data, historical, search, filter, pagination, sort, export, cache, refresh, metadata, supported indices, batch profiles, percentile, weight config, visualization data, audit, model map, typeahead, count, year selection, trend data, PDF report, chart data, available years, index detail, update index, bulk compare, region list, country list).

**§2 (90):** NEG-001–070: Input (null country, invalid ISO, non-existent, invalid index, null year), Auth (10), HTTP (10), injection (10), dependencies (external API down, timeout, stale data, quota, rate limit, cache failure, DB error, memory, service unavailable, PDF failure), format/ID (10), data quality (missing index, partial data, corrupted, future year, negative score, NaN, division by zero, invalid weights, weights sum, invalid region). NEG-071–090: Invalid ISO code, invalid index ID, malformed compare payload, missing required fields, invalid weight config, soft-deleted country, invalid year range, duplicate compare pair, cross-region mismatch, invalid pagination, invalid sort field, invalid filter combo, bulk limit exceeded, external API error, cache miss, PDF generation failure, chart data error, percentile calc failure, trend extrapolation error, opportunity link failure.

**§3 (90):** BND-001–070: Country count (1/50/195), index values (0.0–1.0 ranges, min/max per index), scores (0-100 decimals), weights (0-100%), years (1990–2026), historical periods, comparison count, chart data points, Unicode names, pagination, search terms, response sizes, concurrent, cache sizes, regional aggregation, percentile boundaries. BND-071–090: Min country count, max compare count, zero index value, max index value, score 0 edge, score 100 edge, weights sum 100, empty weight, year boundary, leap year, max pagination page, pageSize=1, pageSize=max, sort by nullable field, filter empty result, concurrent profile, cache size edge, regional empty, percentile 0/100, chart point zero.

**§4 (90):** FUN-001–050: Score calculation (15), risk mapping (10), data retrieval (10), caching (10), audit (5). FUN-051–090: Composite formula, weight application, risk tier mapping, percentile calculation, trend extrapolation, compare diff, chart data transform, cache key generation, cache TTL, export format, model mapping, typeahead filter, count aggregation, year selection, index filtering, region aggregation, opportunity linking, validation rule order, audit trail integrity, soft-delete handling, batch processing, metadata merge, visualization transform, PDF layout, historical range, data quality check, missing index handling, NaN handling, overflow handling, rounding consistency.

**§5 (90):** INT-001–050: External indices API (10), DB (10), cache (10), PDF (10), opportunity linking (10). INT-051–090: External API round-trip, DB read, cache read/write, PDF generation, opportunity service, audit service, permission service, manager delegation, DbContext scope, transaction boundary, cache invalidation, GlobalIndices service, mapper chain, validation pipeline, error propagation, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution, DST manager, country service, region service, chart service.

**§7 (25):** Concurrent profiles, comparisons, cache refresh, score calculations, data updates.
**§8 (21):** Score calc (5), risk mapping (3), weighting (5), validation (5), formatting (3).
**§9 (16):** Single profile (<200ms), comparison (<500ms), historical (<1s), search (<500ms), batch (<2s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
