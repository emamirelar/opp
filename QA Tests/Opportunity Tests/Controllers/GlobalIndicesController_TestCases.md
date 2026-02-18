# GlobalIndicesController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/GlobalIndicesController`  
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

REST API for global development indices: CRUD for index definitions, get index values by country/year, bulk import index data, search/filter indices, data versioning, and admin management.

---

## §1–§10

**§1 (30):** GET all indices (P0), GET index by ID (P0), GET values for country (P0), POST create index (P0), PUT update index (P0), + 25 (DELETE, search, filter by type, filter by year, pagination, sort, bulk import, export, get sources, get years, get countries for index, historical values, versioning, audit, metadata, cache, batch values, validation, model map, typeahead, count, compare, aggregate, trend, PDF, admin operations).

**§2 (90):** NEG-001–070: Input (null ID, non-existent, deleted, invalid type, null name, duplicate name, invalid year, future year, null value, negative value), Auth (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (duplicate index, invalid source URL, stale data, import format error, version conflict, bulk limit exceeded, circular reference, missing required field, invalid aggregation, mass assignment). NEG-071–090: Invalid index type ID, invalid country code, malformed import payload, missing required fields, invalid year range, soft-deleted index, invalid version ref, duplicate value key, cross-index reference, invalid pagination, invalid sort field, invalid filter combo, bulk limit exceeded, import service unavailable, cache failure, DST integration error, export failure, compare different types, aggregate empty set, trend calc failure.

**§3 (90):** BND-001–070: Value ranges (0.0–1.0, 0–100, custom), year ranges (1990–2026), country count (1–195), index count, name lengths, description lengths, source URL length, pagination, search terms, bulk import sizes (1/100/1000/10000), Unicode, concurrent, version count, data point count per index, aggregation complexity. BND-071–090: Min value, max value, zero country, single country, max countries, empty index list, single index, min/max name length, max URL length, year boundary, leap year, max pagination page, pageSize=1, pageSize=max, sort by nullable field, filter empty result, concurrent import, version edge, data point zero, aggregation depth.

**§4 (90):** FUN-001–050: CRUD lifecycle (15), validation (10), import pipeline (10), versioning (10), audit (5). FUN-051–090: Create validation, update partial, delete soft-delete, import parsing, value validation, version increment, aggregation formula, compare logic, trend calculation, cache invalidation, export format, model mapping, typeahead filter, count aggregation, historical range, source validation, metadata handling, batch processing, duplicate detection, soft-delete propagation, workflow sync, validation rule order, audit trail integrity, country code mapping, year range validation, data point ordering, percentile calc, DST integration, admin permission, bulk atomicity, format conversion.

**§5 (90):** INT-001–050: DB (10), cache (10), import service (10), DST integration (10), export (10). INT-051–090: DB round-trip, cache read/write, import pipeline, DST service, export service, audit service, permission service, manager delegation, DbContext scope, transaction boundary, cache invalidation, external API, mapper chain, validation pipeline, error propagation, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution, bulk import service, version service, country service, aggregation service.

**§7 (25):** Concurrent CRUD, import + read, cache invalidation, version conflicts, bulk operations.
**§8 (21):** Validation (5), aggregation (5), formatting (3), import parsing (5), version numbering (3).
**§9 (16):** GET (<200ms), search (<500ms), import 100 (<5s), import 1000 (<30s), export (<3s), memory.
**§10 (10):** 50 concurrent, bulk import under load, spike, sustained, recovery.

---

**Status:** Ready for Execution
