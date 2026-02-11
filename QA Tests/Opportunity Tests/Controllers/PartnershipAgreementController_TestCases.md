# PartnershipAgreementController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/PartnershipAgreementController`  
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

REST API for partnership agreements linked to opportunities: CRUD agreements, template selection, document generation, approval workflow, linking to partners/opportunities, and export.

---

## §1–§10

**§1 (35):** POST create (P0), GET by ID (P0), GET for opportunity (P0), PUT update (P0), DELETE (P0), + 30 (link to partner, template select, generate PDF, approval workflow, search, filter by type, filter by status, pagination, sort, export, audit, permissions, clone, amend, sign, version history, bulk operations, model map, typeahead, count, renewal, expiry check, compliance, notification, compare, archive).

**§2 (70):** Input (null, non-existent, deleted, invalid type, invalid partner, missing required), Auth (10), State (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (duplicate, expired template, invalid clause, circular link, max amendments, signing without approval, expired cert, role violation, conflicting terms, mass assignment).

**§3 (70):** Name/clause lengths, agreement count (0–100+), amendment count, signature count, version count, template count, date ranges, file sizes, pagination, Unicode, concurrent, compliance checks, renewal periods, party count.

**§4 (50):** CRUD (15), approval (10), document generation (10), linking (10), audit (5).
**§5 (50):** Partner service (10), opportunity (10), PDF/document (10), notification (10), approval (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), document security (10), signing security (10).
**§7 (25):** Concurrent edits, approvals, signatures, generation, amendments.
**§8 (21):** Validation (5), generation (5), linking (3), status (5), formatting (3).
**§9 (16):** GET (<200ms), generate PDF (<3s), search (<500ms), list (<300ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large agreements, recovery.

---

**Status:** Ready for Execution
