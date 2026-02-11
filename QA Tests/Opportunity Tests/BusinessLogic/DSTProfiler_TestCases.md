# DST Profiler — Test Cases

**Component:** Opportunity DST (Decision Support Tool) Profiler  
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

Decision Support Tool profiler for opportunities. Analyzes country/region indicators (Fragile States Index, HDI, Corruption Perceptions, Political Stability, Ease of Doing Business, Global Peace Index), generates risk profiles, calculates composite scores, visualizes data, compares regions, tracks historical trends, maps to opportunity risk categories, and produces PDF reports.

---

## §1 Positive — 35

| ID | Test | Expected | Pr |
|----|------|----------|----|
| POS-001 | Load DST profile for country | All indices loaded | P0 |
| POS-002 | Calculate composite score | Weighted score calculated | P0 |
| POS-003 | Generate risk profile | Risk categories assigned | P0 |
| POS-004 | Link profile to opportunity | Profile associated | P0 |
| POS-005 | Export PDF report | Valid PDF with charts | P0 |
| POS-006 | Load Fragile States Index | FSI data loaded | P1 |
| POS-007 | Load HDI | HDI data loaded | P1 |
| POS-008 | Load Corruption Index | CPI data loaded | P1 |
| POS-009 | Load Political Stability | Data loaded | P1 |
| POS-010 | Load Ease of Business | Data loaded | P1 |
| POS-011 | Load Global Peace Index | GPI data loaded | P1 |
| POS-012 | Compare two countries | Side-by-side comparison | P1 |
| POS-013 | Compare regions | Regional aggregation | P1 |
| POS-014 | Historical trend (5yr) | Trend data displayed | P1 |
| POS-015 | Risk category mapping | High/Medium/Low assigned | P1 |
| POS-016 | Update index data | Data refreshed | P1 |
| POS-017 | Custom weighting | User-defined weights | P1 |
| POS-018 | Visualization data | Chart-ready data | P1 |
| POS-019 | Search countries | Name search works | P1 |
| POS-020 | Filter by region | Regional filter | P1 |
| POS-021 | Filter by risk level | Risk filter | P1 |
| POS-022 | Paginate countries | Paginated results | P2 |
| POS-023 | Sort by score | Score-sorted list | P2 |
| POS-024 | Sort by name | Alpha sort | P2 |
| POS-025 | Get supported indices | List of all indices | P2 |
| POS-026 | Cache profile | Profile cached | P2 |
| POS-027 | Audit trail | Profile access logged | P2 |
| POS-028 | Multiple indices per opp | All indices linked | P1 |
| POS-029 | Refresh cached data | Cache invalidated | P2 |
| POS-030 | Get index metadata | Description, source, year | P2 |
| POS-031 | Batch country profiles | Load 10 countries | P1 |
| POS-032 | Regional average | Average scores | P2 |
| POS-033 | Percentile ranking | Country percentile | P2 |
| POS-034 | Map to model | All fields mapped | P2 |
| POS-035 | Data year selection | Select specific year | P2 |

---

## §2 Negative — 70 | §3 Boundary — 70

**§2:** Input (null country, non-existent, deleted, invalid ISO code, null oppId, invalid indices, null weights, negative weights, weights>100%, weights sum≠100%), Auth (10), Data (missing FSI, missing HDI, stale data, no historical, corrupted index, partial data, future year, negative score, NaN score, division by zero), injection (SQL, XSS, path, command, template, LDAP, HTML, JSON), dependencies (API down, timeout, rate limit, DB error, cache failure, memory OOM, service unavailable, PDF gen fail), format/state (ID errors, pagination, sort, mass assign).

**§3:** Country count (1/50/100/195/196+), index values (0.0/0.5/1.0/min/max per index), scores (0-100, decimals, negative boundary, 100+), weights (0%/1%/50%/100%, sum boundaries), years (1990/2000/2020/2026/current), historical periods (1yr/5yr/10yr/20yr+), comparison (2/5/10/50 countries), chart data points, Unicode country names, search terms, pagination, concurrent.

---

## §4–§10

**§4 (50):** Score calculation (15), risk categorization (10), data loading (10), visualization (10), audit (5).
**§5 (50):** External index APIs (10), opportunity linking (10), PDF/chart services (10), cache (10), DB (10).
**§6 (50):** Injection (10), access control (10), IDOR (10), data exposure (10), API security (10).
**§7 (25):** Concurrent profile loads, score calculations, cache updates, data refresh, comparisons, etc.
**§8 (21):** Score calc (5), risk mapping (3), weighting (5), data validation (5), formatting (3).
**§9 (16):** Single country (<200ms), 10 countries (<1s), comparison (<500ms), PDF (<3s), historical (<1s), memory tests.
**§10 (10):** 50 concurrent profiles, 100 searches, spike, sustained, data refresh under load.

---

**Status:** Ready for Execution
