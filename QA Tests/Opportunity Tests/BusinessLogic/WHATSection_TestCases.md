# WHAT Section — Test Cases

**Component:** Opportunity WHAT Section (Scope, Deliverables, Activities)  
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

WHAT section of opportunities defines scope, deliverables, key activities, outcomes, indicators, service lines, modalities, UNOPS value proposition, and sustainability considerations. CRUD operations, rich text editing, AI-assisted drafting, version tracking, section completeness validation, and export.

---

## §1–§10

**§1 Positive (35):** Save WHAT section data (P0), load WHAT section (P0), update scope (P0), add deliverable (P0), completeness check (P0), + 30 P1/P2 (add activity, add outcome, add indicator, service line selection, modality selection, value proposition, sustainability, AI draft, version save, rich text, attachment, export, audit, validation pass, search, filter, pagination, sort, model mapping, section lock/unlock, compare versions, restore version, typeahead, count, clone section, template apply, comments, approval).

**§2 Negative (70):** Input (null oppId, non-existent, deleted, null scope, empty deliverables, invalid service line, invalid modality), Auth (10), State (edit closed, edit locked, edit during workflow, save approved, modify final), injection (SQL in scope, XSS in text, rich text XSS, HTML injection, template injection), AI (service down, timeout, inappropriate, hallucination, quota), dependencies (10), format/ID (10), business (incomplete mandatory, max deliverables exceeded, circular dependency, duplicate deliverable, orphan indicator, max chars exceeded, invalid rich text, mass assignment).

**§3 Boundary (70):** Scope length (0/100/1000/10000/max), deliverable count (0/1/10/50/100/101), activity count, outcome count, indicator count, rich text size, attachment size/count, version count, service line count, modality count, Unicode, concurrent edits, comparison complexity, date ranges, AI response length, template complexity, section nesting depth, comment count, search terms.

**§4 (50):** Section CRUD (15), validation rules (10), deliverable management (10), completeness (10), audit (5).
**§5 (50):** Opportunity service (10), AI service (10), document service (10), notification (10), export (10).
**§6 (50):** Injection (10), access control (10), IDOR (10), rich text security (10), AI security (10).
**§7 (25):** Concurrent edits, saves, AI drafts, version creation, deliverable adds.
**§8 (21):** Validation (5), completeness calc (5), formatting (3), deliverable linking (5), word count (3).
**§9 (16):** Save (<500ms), load (<300ms), AI draft (<5s), export (<3s), list (<300ms), memory.
**§10 (10):** 50 concurrent edits, spike, sustained, large sections, recovery.

---

**Status:** Ready for Execution
