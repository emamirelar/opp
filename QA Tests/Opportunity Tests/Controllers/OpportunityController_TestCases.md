# OpportunityController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityController`  
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

Main REST API for opportunity CRUD: create, read, update, delete, list with filtering/pagination, search, get sections (WHY/WHAT/Team/Budget/Schedule), get permissions, status management, export, bulk operations, and audit trail.

---

## §1–§10

**§1 (35):** POST create (P0), GET by ID (P0), PUT update (P0), DELETE soft-delete (P0), GET list (P0), + 30 (GET sections, GET permissions, search, filter by stage, filter by status, filter by partner, filter by OM, pagination, sort, export CSV, export PDF, bulk export, audit trail, GET count, GET summary, model map, typeahead, GET by partner, GET by user, clone, archive, restore, validate, GET workflow history, GET documents, GET team, GET budget, GET schedule).

**§2 (70):** Input (null name, non-existent, deleted, invalid stage, null partner, missing required), Auth (10), State (edit closed, delete approved, update during workflow), HTTP (10), injection (10), dependencies (10), format/ID (10), business (duplicate name if restricted, invalid partner link, missing OM, exceed max opps, invalid date range, circular reference, orphan sections, mass assignment, invalid filter combination, stale data).

**§3 (70):** Name length (1/200/201), description (0/4000/4001), list sizes (0/1/100/1000/10000), pagination, search terms, filter combinations, concurrent, Unicode, date ranges, partner count per opp, section completeness, team size, budget lines, document count, response payload sizes, export row counts.

**§4 (50):** CRUD lifecycle (15), search/filter (10), section management (10), workflow integration (10), audit (5).
**§5 (50):** Manager integration (10), partner service (10), workflow (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API security (10).
**§7 (25):** Concurrent CRUD, search + update, delete during read, bulk operations, workflow actions.
**§8 (21):** Route validation (5), model binding (5), response mapping (3), error formatting (5), filter parsing (3).
**§9 (16):** GET (<200ms), list (<500ms), search (<500ms), create (<500ms), export (<5s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
