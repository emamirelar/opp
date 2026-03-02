# WHAT Section — Test Cases

**Component:** Opportunity WHAT Section (Scope, Deliverables, Activities)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:**
- N≥3P: 90≥90 ✅ PASS
- E≥3P: 90≥90 ✅ PASS
- F≥3P: 90≥90 ✅ PASS
- I≥3P: 90≥90 ✅ PASS

---

## Feature Overview

WHAT section of opportunities defines scope, deliverables, key activities, outcomes, indicators, service lines, modalities, UNOPS value proposition, and sustainability considerations. CRUD operations, rich text editing, AI-assisted drafting, version tracking, section completeness validation, and export.

---

## §1–§10

**§1 Positive (30):** Save WHAT section data (P0), load WHAT section (P0), update scope (P0), add deliverable (P0), completeness check (P0), + 25 P1/P2 (add activity, add outcome, add indicator, service line selection, modality selection, value proposition, sustainability, AI draft, version save, rich text, attachment, export, audit, validation pass, search, filter, pagination, sort, model mapping, section lock/unlock, compare versions, restore version, typeahead, count, clone section).

**§2 Negative (90):** Input (null oppId, non-existent, deleted, null scope, empty deliverables, invalid service line, invalid modality), Auth (10), State (edit closed, edit locked, edit during workflow, save approved, modify final), injection (SQL in scope, XSS in text, rich text XSS, HTML injection, template injection), AI (service down, timeout, inappropriate, hallucination, quota), dependencies (10), format/ID (10), business (incomplete mandatory, max deliverables exceeded, circular dependency, duplicate deliverable, orphan indicator, max chars exceeded, invalid rich text, mass assignment). **Products:** null product list, invalid product ID, deleted product reference, product not in scope, product quantity negative, product unit invalid, product budget negative, product-outcome mismatch, product without deliverable, duplicate product in scope. **Budget:** null budget total, negative budget, budget exceeds limit, budget currency invalid, budget decimal overflow, budget-per-deliverable mismatch, zero budget with deliverables, budget without products. **Beneficiaries:** null beneficiaries, invalid beneficiary type, negative beneficiary count, beneficiary without deliverable, duplicate beneficiary, beneficiary region invalid, beneficiary group empty, max beneficiaries exceeded. **Timeline:** null start date, null end date, end before start, timeline outside opportunity dates, invalid date format, past start date for draft, timeline without deliverables, concurrent timeline overlap.

**§3 Boundary (90):** Scope length (0/100/1000/10000/max), deliverable count (0/1/10/50/100/101), activity count, outcome count, indicator count, rich text size, attachment size/count, version count, service line count, modality count, Unicode, concurrent edits, comparison complexity, date ranges, AI response length, template complexity, section nesting depth, comment count, search terms. **Products:** product count (0/1/10/50/100/101), product quantity (0/1/999999/max), product budget (0/0.01/max decimal), product name length (0/1/255/max). **Budget:** total (0/0.01/999999999.99/max), per-deliverable (0/1/max), currency precision (2/3/4 decimals). **Beneficiaries:** count (0/1/1000/max), beneficiary type enum bounds, region code length, group name length. **Timeline:** start=end, start=min date, end=max date, duration (1 day/1 year/max), milestone count (0/1/50/100).

**§4 Functional (90):** Section CRUD (15), validation rules (10), deliverable management (10), completeness (10), audit (5). **Products:** add product (5), remove product (5), update product quantity (5), product-deliverable link (5), product budget calc (5). **Budget:** total calculation (5), budget allocation (5), currency conversion (5), budget validation (5). **Beneficiaries:** add beneficiary (5), beneficiary type filter (5), beneficiary-deliverable link (5), beneficiary aggregation (5). **Timeline:** set timeline (5), milestone CRUD (5), timeline validation (5), timeline-activity link (5).

**§5 Integration (90):** Opportunity service (10), AI service (10), document service (10), notification (10), export (10). **Products:** product catalog service (10), inventory service (5), pricing service (5). **Budget:** finance service (10), currency service (5), approval workflow (5). **Beneficiaries:** beneficiary registry (10), geography service (5), reporting service (5). **Timeline:** calendar service (10), scheduling service (5), dependency resolver (5).

**§6 Security (50):** Injection (10), access control (10), IDOR (10), rich text security (10), AI security (10).

**§7 Concurrency (25):** Concurrent edits, saves, AI drafts, version creation, deliverable adds.

**§8 Unit (21):** Validation (5), completeness calc (5), formatting (3), deliverable linking (5), word count (3).

**§9 Performance (16):** Save (<500ms), load (<300ms), AI draft (<5s), export (<3s), list (<300ms), memory.

**§10 Load (10):** 50 concurrent edits, spike, sustained, large sections, recovery.

---

**Status:** Ready for Execution
