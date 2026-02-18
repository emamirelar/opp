# OpportunityBudgetController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityBudgetController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | ≥30 | ✅ |
| §2 Negative | 90 | ≥90 | ✅ |
| §3 Boundary | 90 | ≥90 | ✅ |
| §4 Functional | 90 | ≥90 | ✅ |
| §5 Integration | 90 | ≥90 | ✅ |
| §6 Concurrency | 25 | 25 | ✅ |
| §7 Unit | 21 | 21 | ✅ |
| §8 Performance | 16 | 16 | ✅ |
| §9 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

REST API for opportunity budget management: CRUD budget line items, total calculations, currency handling, budget categories, approval thresholds, variance tracking, budget versions, export, and reporting.

---

## §1–§9

**§1 (30):** GET budget (P0), POST line item (P0), PUT update item (P0), DELETE item (P0), GET total (P0), + 25 (categories, currency conversion, variance, versioning, approval threshold, export CSV, export PDF, search, filter, pagination, sort, audit, permissions, bulk add, bulk update, model map, typeahead, count, budget summary, forecast, actuals, comparison, template, clone, lock).

**§2 (90):** Input (null oppId, non-existent, negative amount, null category, invalid currency, zero amount), Auth (10), State (edit closed, locked, approved), HTTP (10), injection (10), dependencies (10), format/ID (10), business (exceed threshold, duplicate category, invalid currency pair, precision loss, rounding error, budget mismatch, negative total, overflow, orphan line item, mass assignment), additional error handling (invalid budget type, malformed payload, missing required fields, soft-deleted opportunity, expired fiscal year, invalid approval threshold, cross-opportunity budget leak, invalid pagination params, invalid sort field, invalid filter combination, bulk limit exceeded, template not found, clone source deleted, lock conflict, currency conversion rate missing, invalid decimal precision, invalid date range, concurrent lock violation, invalid export format, unauthorized budget access).

**§3 (90):** Amounts (0.00/0.01/1.00/999999.99/1000000.00/MAX), currencies (USD/EUR/GBP/JPY/all), line items (0/1/10/50/100/101), category count, decimal precision (2/4/6), total calculations, pagination, version count, Unicode descriptions, date ranges, percentage allocations (0-100%), variance thresholds, concurrent, conversion rates, min amount step, max amount sum, zero-amount edge, max decimal precision, empty line list, single line item, max lines per budget, min/max fiscal range, same-day fiscal, year boundary, leap year dates, max budget total, empty budget, single line, max pagination page, pageSize=1, pageSize=max, sort by nullable field, filter empty result, concurrent identical update.

**§4 (90):** Calculation accuracy (15), validation (10), currency handling (10), versioning (10), audit (5), category-to-line mapping, amount sum validation, total rollup accuracy, variance threshold check, currency conversion formula, approval threshold logic, version diff calculation, bulk add atomicity, template application, clone deep copy, lock release on save, history versioning, notification trigger, validation rule order, permission inheritance, model mapping completeness, typeahead filtering, count aggregation, export format validation, budget mismatch detection, forecast accuracy, actuals reconciliation, comparison delta, rounding rule, currency symbol handling, audit trail integrity, soft-delete propagation, workflow status sync, fiscal range validation, precision handling, forecast extrapolation, budget category ordering, line item sequencing, total recalculation on delete, approval chain validation.

**§5 (90):** Opportunity service (10), currency service (10), approval (10), export (10), notification (10), partner linkage, workflow integration, audit service, permission service, manager delegation, DbContext scope, transaction boundary, cache invalidation, external API fallback, mapper chain, validation pipeline, error propagation, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution, budget manager round-trip, currency service sync, approval workflow check, export pipeline, notification service integration, opportunity context, Gantt rendering pipeline, template service integration, version service round-trip, fiscal year service, category service, line item service.

**§6 (25):** Concurrent edits, calculations, approvals, exports, version creation.
**§7 (21):** Calculation (5), rounding (5), currency conversion (3), validation (5), formatting (3).
**§8 (16):** GET (<200ms), calculate (<100ms), export (<3s), bulk (<2s), search (<500ms), memory.
**§9 (10):** 50 concurrent, spike, sustained, large budgets, recovery.

---

**Status:** Ready for Execution
