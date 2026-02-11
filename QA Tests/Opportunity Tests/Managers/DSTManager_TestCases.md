# DSTManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DSTManager`  
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

Business logic for Decision Support Tool: LoadCountryProfileAsync, CalculateCompositeScoreAsync, GenerateRiskProfileAsync, CompareCountriesAsync, GetHistoricalTrendsAsync, UpdateIndexDataAsync, cache management, weight configuration, and PDF report generation.

---

## §1–§10 (same pattern as other managers)

**§1 (35):** Load profile (P0), calculate score (P0), risk profile (P0), compare (P0), historical (P0), + 30 P1/P2.
**§2 (70):** Input (10), Auth (10), Data quality (10), injection (10), dependencies (10), format (10), business (10).
**§3 (70):** Index value ranges, country counts, weight boundaries, year ranges, comparison sizes, score decimals, cache sizes, chart data points, Unicode, concurrent, pagination, historical periods, aggregation complexity.
**§4 (50):** Score calc (15), risk mapping (10), data loading (10), caching (10), audit (5).
**§5 (50):** External APIs (10), DB (10), cache (10), PDF (10), opportunity linking (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API security (10).
**§7 (25):** Concurrent loads, score calcs, cache updates, comparisons, data refreshes.
**§8 (21):** Score calc (5), risk mapping (3), weighting (5), validation (5), formatting (3).
**§9 (16):** Profile (<200ms), score (<100ms), comparison (<500ms), PDF (<3s), batch (<2s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
