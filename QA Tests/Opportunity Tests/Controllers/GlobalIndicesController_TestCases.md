# GlobalIndicesController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/GlobalIndicesController`  
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

REST API for global development indices: CRUD for index definitions, get index values by country/year, bulk import index data, search/filter indices, data versioning, and admin management.

---

## §1–§10

**§1 (35):** GET all indices (P0), GET index by ID (P0), GET values for country (P0), POST create index (P0), PUT update index (P0), + 30 P1/P2 (DELETE, search, filter by type, filter by year, pagination, sort, bulk import, export, get sources, get years, get countries for index, historical values, versioning, audit, metadata, cache, batch values, validation, model map, typeahead, count, compare, aggregate, trend, PDF, admin operations).

**§2 (70):** Input (null ID, non-existent, deleted, invalid type, null name, duplicate name, invalid year, future year, null value, negative value), Auth (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (duplicate index, invalid source URL, stale data, import format error, version conflict, bulk limit exceeded, circular reference, missing required field, invalid aggregation, mass assignment).

**§3 (70):** Value ranges (0.0–1.0, 0–100, custom), year ranges (1990–2026), country count (1–195), index count, name lengths, description lengths, source URL length, pagination, search terms, bulk import sizes (1/100/1000/10000), Unicode, concurrent, version count, data point count per index, aggregation complexity.

**§4 (50):** CRUD lifecycle (15), validation (10), import pipeline (10), versioning (10), audit (5).
**§5 (50):** DB (10), cache (10), import service (10), DST integration (10), export (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), admin operations (10), data integrity (10).
**§7 (25):** Concurrent CRUD, import + read, cache invalidation, version conflicts, bulk operations.
**§8 (21):** Validation (5), aggregation (5), formatting (3), import parsing (5), version numbering (3).
**§9 (16):** GET (<200ms), search (<500ms), import 100 (<5s), import 1000 (<30s), export (<3s), memory.
**§10 (10):** 50 concurrent, bulk import under load, spike, sustained, recovery.

---

**Status:** Ready for Execution
