# WHY Section — Test Cases

**Component:** Opportunity WHY Section (Rationale, Strategic Alignment, Context)  
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

WHY section of opportunities captures rationale, strategic alignment (SDGs, UNOPS Strategic Plan), country/regional context, partner needs assessment, risk context (DST integration), development challenges, and UNOPS mandate alignment. Features: rich text editing, SDG mapping, context data auto-population from DST, AI-assisted drafting, version tracking, completeness validation, and export.

---

## §1–§10

**§1 Positive (35):** Save WHY section (P0), load WHY section (P0), update rationale (P0), map SDGs (P0), completeness check (P0), + 30 P1/P2 (strategic alignment, country context, partner needs, risk context from DST, development challenges, mandate alignment, AI draft, version save, rich text, SDG multi-select, auto-populate from DST, export, audit, validation pass, search, filter, pagination, sort, model mapping, section lock, compare versions, restore, typeahead, count, clone, template, comments, approval, SDG indicator linking).

**§2 Negative (70):** Input (null oppId, non-existent, deleted, null rationale, invalid SDG, no SDGs selected), Auth (10), State (edit closed, locked, during workflow, approved, final), injection (SQL, XSS, rich text XSS, HTML, template), AI (down, timeout, inappropriate, hallucination, quota), DST (service down, stale data, missing country, invalid indices), dependencies (10), format/ID (10), business (incomplete mandatory, max SDGs, invalid alignment, circular reference, missing context, max chars, invalid rich text, mass assignment).

**§3 Boundary (70):** Rationale length (0/100/10000/max), SDG count (0/1/5/17/18), strategic alignment items (0/1/10/50), context fields, rich text size, version count, DST data points, AI response length, concurrent edits, Unicode, search terms, pagination, date ranges, comparison, nesting, comment count, attachment count/size.

**§4 (50):** Section CRUD (15), SDG management (10), DST integration (10), validation (10), audit (5).
**§5 (50):** Opportunity (10), SDG service (10), DST service (10), AI (10), export (10).
**§6 (50):** Injection (10), access control (10), IDOR (10), rich text (10), AI security (10).
**§7 (25):** Concurrent edits, SDG assignments, DST refreshes, AI drafts, version creation.
**§8 (21):** Validation (5), SDG mapping (5), completeness (3), formatting (5), DST score parsing (3).
**§9 (16):** Save (<500ms), load (<300ms), DST auto-populate (<2s), AI draft (<5s), export (<3s), memory.
**§10 (10):** 50 concurrent edits, spike, sustained, DST under load, recovery.

---

**Status:** Ready for Execution
