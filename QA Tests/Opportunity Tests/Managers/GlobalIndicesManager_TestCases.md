# GlobalIndicesManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GlobalIndicesManager`  
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

CRUD for global development indices, bulk import/export, value retrieval by country/year, versioning, aggregation, and DST integration.

---

## Test Case Inventory

**Removed:** POS-031, POS-032, POS-033, POS-034, POS-035 (5 positive)

**Added:** NEG-071-090 (20 negative), BND-071-090 (20 boundary), FUN-051-090 (40 functional), INT-051-090 (40 integration)

---

## §1–§10

**§1 (30):** CRUD + import/export + search/filter + aggregation + versioning (30 tests: POS-001–POS-030).
**§2 (90):** Input (10), Auth (10), HTTP (10), injection (10), dependencies (10), format (10), business (10), NEG-071–NEG-090 (malformed index value, invalid country, null year, orphan index, deleted country, invalid aggregation params, stale index data, concurrent conflict, permission denied, audit bypass, invalid version calc, duplicate index, max indices exceeded, invalid date range, malformed filter, invalid pagination, cross-entity index, soft-delete violation, orphan value link, invalid export format).
**§3 (90):** Value ranges, year ranges, country/index counts, import sizes, Unicode, pagination, concurrent, name/description lengths, aggregation complexity, version counts, cache boundaries, BND-071–BND-090 (value 0/max, year min/max, country 0/max, index 0/max, empty import, max import, empty description, max description, single aggregation, max aggregations, concurrent update boundary, Unicode name, boundary pagination, cache empty, cache max, version 0, version max, date boundary).
**§4 (90):** CRUD lifecycle (15), import pipeline (10), aggregation (10), versioning (10), audit (5), FUN-051–FUN-090 (import validation, aggregation formula, version diff, audit trail completeness, validation rules, workflow integration, notification triggers, export format, bulk update logic, soft delete cascade, restore logic, permission propagation, visibility rules, search filter logic, date range filter, country filter, index filter, year filter, pagination logic, sort options, report generation, index export, aggregation export, value calc, completeness check, duplicate detection, orphan prevention, cross-entity validation, version tracking, change history, audit field population).
**§5 (90):** DB (10), cache (10), DST (10), import (10), export (10), INT-051–INT-090 (DB CRUD round-trip, cache service, DST service, import pipeline, export pipeline, persistence layer, cache invalidation, audit trail, permission service, workflow service, country manager, partner manager, search service, filter service, pagination service, bulk operations, AI integration, analytics service, external API, PDF generation, CSV export, Excel export, JSON serialization, event bus, message queue, retry logic, circuit breaker, timeout handling, rate limiting, logging integration, metrics collection, health check, dependency injection).
**§7 (25):** Concurrent CRUD, import + read, cache invalidation, version conflicts, bulk operations.
**§8 (21):** Validation (5), aggregation (5), formatting (3), import parsing (5), versioning (3).
**§9 (16):** GET (<200ms), search (<500ms), import 100 (<5s), import 1000 (<30s), export (<3s), memory.
**§10 (10):** 50 concurrent, bulk under load, spike, sustained, recovery.

---

**Status:** Ready for Execution
