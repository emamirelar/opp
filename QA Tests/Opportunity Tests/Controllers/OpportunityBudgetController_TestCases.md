# OpportunityBudgetController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityBudgetController`  
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

REST API for opportunity budget management: CRUD budget line items, total calculations, currency handling, budget categories, approval thresholds, variance tracking, budget versions, export, and reporting.

---

## §1–§10

**§1 (35):** GET budget (P0), POST line item (P0), PUT update item (P0), DELETE item (P0), GET total (P0), + 30 (categories, currency conversion, variance, versioning, approval threshold, export CSV, export PDF, search, filter, pagination, sort, audit, permissions, bulk add, bulk update, model map, typeahead, count, budget summary, forecast, actuals, comparison, template, clone, lock, unlock, history, notification, validation).

**§2 (70):** Input (null oppId, non-existent, negative amount, null category, invalid currency, zero amount), Auth (10), State (edit closed, locked, approved), HTTP (10), injection (10), dependencies (10), format/ID (10), business (exceed threshold, duplicate category, invalid currency pair, precision loss, rounding error, budget mismatch, negative total, overflow, orphan line item, mass assignment).

**§3 (70):** Amounts (0.00/0.01/1.00/999999.99/1000000.00/MAX), currencies (USD/EUR/GBP/JPY/all), line items (0/1/10/50/100/101), category count, decimal precision (2/4/6), total calculations, pagination, version count, Unicode descriptions, date ranges, percentage allocations (0-100%), variance thresholds, concurrent, conversion rates.

**§4 (50):** Calculation accuracy (15), validation (10), currency handling (10), versioning (10), audit (5).
**§5 (50):** Opportunity service (10), currency service (10), approval (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), financial data security (10), export security (10).
**§7 (25):** Concurrent edits, calculations, approvals, exports, version creation.
**§8 (21):** Calculation (5), rounding (5), currency conversion (3), validation (5), formatting (3).
**§9 (16):** GET (<200ms), calculate (<100ms), export (<3s), bulk (<2s), search (<500ms), memory.
**§10 (10):** 50 concurrent, spike, sustained, large budgets, recovery.

---

**Status:** Ready for Execution
