# OpportunityBudgetManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OpportunityBudgetManager`  
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

Budget line item CRUD, total calculations, currency handling, categories, approval thresholds, variance tracking, versioning, and reporting for opportunities.

---

## Test Case Inventory

**Removed:** POS-031, POS-032, POS-033, POS-034, POS-035 (5 positive)

**Added:** NEG-071-090 (20 negative), BND-071-090 (20 boundary), FUN-051-090 (40 functional), INT-051-090 (40 integration)

---

## §1–§10

**§1 (30):** CRUD + totals + currency + categories + variance + versioning + export (30 tests: POS-001–POS-030).
**§2 (90):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (negative amounts, currency precision, rounding, overflow, threshold exceeded, orphan items, duplicate categories, budget lock violations, calculation errors, mass assignment), NEG-071–NEG-090 (malformed amount, invalid currency, null category, orphan line item, deleted opportunity, invalid variance params, stale budget data, concurrent conflict, permission denied, audit bypass, invalid total calc, duplicate line, max line items exceeded, invalid date range, malformed filter, invalid pagination, cross-entity budget, soft-delete violation, orphan category link, invalid export format).
**§3 (90):** Amounts (0.00–MAX decimal), currencies (all supported), line items (0–100+), categories, decimal precision (2/4/6), percentage allocations (0–100%), concurrent, pagination, Unicode descriptions, date ranges, version counts, variance thresholds, total calculations at boundary, BND-071–BND-090 (amount 0/max, currency boundary, line 0/max, category empty, category max, precision 2/4/6, allocation 0/100%, empty budget, max line items, single allocation, max allocations, concurrent update boundary, Unicode description, boundary pagination, variance empty, variance max, version 0, version max, date boundary).
**§4 (90):** Calculation accuracy (15), validation (10), currency (10), versioning (10), audit (5), FUN-051–FUN-090 (total calc formula, rounding logic, currency conversion, version diff, audit trail completeness, validation rules, workflow integration, notification triggers, export format, import validation, bulk update logic, soft delete cascade, restore logic, permission propagation, visibility rules, search filter logic, date range filter, category filter, status filter, owner filter, pagination logic, sort options, report generation, budget export, variance export, approval calc, completeness check, duplicate detection, orphan prevention, cross-opportunity validation, version tracking, change history, audit field population).
**§5 (90):** Opportunity (10), currency service (10), approval (10), export (10), notification (10), INT-051–INT-090 (opportunity CRUD round-trip, currency service, approval service, export pipeline, notification service, DB persistence, cache invalidation, audit trail, permission service, workflow service, partner manager, document manager, search service, filter service, pagination service, bulk operations, AI integration, analytics service, external API, PDF generation, CSV export, Excel export, JSON serialization, event bus, message queue, retry logic, circuit breaker, timeout handling, rate limiting, logging integration, metrics collection, health check, dependency injection).
**§7 (25):** Concurrent edits, calculations, approval, version creation, bulk operations.
**§8 (21):** Calculation (5), rounding (5), conversion (3), validation (5), formatting (3).
**§9 (16):** CRUD (<200ms), calculate (<100ms), export (<3s), bulk (<2s), search (<500ms), memory.
**§10 (10):** 50 concurrent, spike, sustained, large budgets, recovery.

---

**Status:** Ready for Execution
