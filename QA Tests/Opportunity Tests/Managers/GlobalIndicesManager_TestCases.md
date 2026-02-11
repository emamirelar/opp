# GlobalIndicesManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GlobalIndicesManager`  
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

CRUD for global development indices, bulk import/export, value retrieval by country/year, versioning, aggregation, and DST integration.

---

## §1–§10

**§1 (35):** CRUD + import/export + search/filter + aggregation + versioning (35 tests).
**§2 (70):** Input (10), Auth (10), HTTP (10), injection (10), dependencies (10), format (10), business (10).
**§3 (70):** Value ranges, year ranges, country/index counts, import sizes, Unicode, pagination, concurrent, name/description lengths, aggregation complexity, version counts, cache boundaries.
**§4 (50):** CRUD lifecycle (15), import pipeline (10), aggregation (10), versioning (10), audit (5).
**§5 (50):** DB (10), cache (10), DST (10), import (10), export (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), admin ops (10), data integrity (10).
**§7 (25):** Concurrent CRUD, import + read, cache invalidation, version conflicts, bulk operations.
**§8 (21):** Validation (5), aggregation (5), formatting (3), import parsing (5), versioning (3).
**§9 (16):** GET (<200ms), search (<500ms), import 100 (<5s), import 1000 (<30s), export (<3s), memory.
**§10 (10):** 50 concurrent, bulk under load, spike, sustained, recovery.

---

**Status:** Ready for Execution
