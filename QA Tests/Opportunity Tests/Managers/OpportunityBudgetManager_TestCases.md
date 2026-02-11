# OpportunityBudgetManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OpportunityBudgetManager`  
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

Budget line item CRUD, total calculations, currency handling, categories, approval thresholds, variance tracking, versioning, and reporting for opportunities.

---

## §1–§10

**§1 (35):** CRUD + totals + currency + categories + variance + versioning + export (35 tests).
**§2 (70):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (negative amounts, currency precision, rounding, overflow, threshold exceeded, orphan items, duplicate categories, budget lock violations, calculation errors, mass assignment).
**§3 (70):** Amounts (0.00–MAX decimal), currencies (all supported), line items (0–100+), categories, decimal precision (2/4/6), percentage allocations (0–100%), concurrent, pagination, Unicode descriptions, date ranges, version counts, variance thresholds, total calculations at boundary.
**§4 (50):** Calculation accuracy (15), validation (10), currency (10), versioning (10), audit (5).
**§5 (50):** Opportunity (10), currency service (10), approval (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), financial data (10), export security (10).
**§7 (25):** Concurrent edits, calculations, approval, version creation, bulk operations.
**§8 (21):** Calculation (5), rounding (5), conversion (3), validation (5), formatting (3).
**§9 (16):** CRUD (<200ms), calculate (<100ms), export (<3s), bulk (<2s), search (<500ms), memory.
**§10 (10):** 50 concurrent, spike, sustained, large budgets, recovery.

---

**Status:** Ready for Execution
