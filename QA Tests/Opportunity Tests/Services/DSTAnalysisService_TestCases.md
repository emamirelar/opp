# DSTAnalysisService — Test Cases

**Component:** DST Analysis Service Layer  
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

Service for DST data analysis: index data retrieval from external sources, composite score algorithms, trend analysis, regional aggregation, risk profiling, chart data preparation, caching, and PDF report rendering.

---

## §1–§10

**§1 (35):** Index retrieval, score calculation, trend analysis, aggregation, risk profiling, chart data, cache, PDF + 27 P1/P2.
**§2 (70):** Input (10), Auth (10), Data quality (10), injection (10), dependencies (external APIs, 10), format (10), business (10).
**§3 (70):** Index ranges, country counts, year ranges, weights, score precision, chart data points, cache sizes, aggregation levels, concurrent, trend periods, comparison counts, regional groupings.
**§4 (50):** Score algorithms (15), trend calculation (10), aggregation (10), caching (10), audit (5).
**§5 (50):** External index APIs (10), DB (10), cache (10), PDF (10), DSTManager (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API key security (10).
**§7 (25):** Concurrent analyses, cache refresh, API calls, score calculations, report generation.
**§8 (21):** Score algorithms (5), trend math (5), aggregation (3), weighting (5), formatting (3).
**§9 (16):** Single analysis (<500ms), batch (<3s), trend (<1s), PDF (<3s), cache hit (<50ms), memory.
**§10 (10):** 50 concurrent, spike, sustained, external API load, recovery.

---

**Status:** Ready for Execution
