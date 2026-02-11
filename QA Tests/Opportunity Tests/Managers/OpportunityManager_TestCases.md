# OpportunityManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OpportunityManager`  
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

Core opportunity CRUD, section management (WHY/WHAT/Team/Budget/Schedule), status/stage lifecycle, partner linking, document management, workflow integration, search/filter/pagination, export, AI integration, and comprehensive audit trail.

---

## §1–§10

**§1 (35):** CreateAsync (P0), GetByIdAsync (P0), UpdateAsync (P0), DeleteAsync (P0), GetListAsync (P0), + 30 (GetSections, UpdateSection, LinkPartner, GetDocuments, GetTeam, GetBudget, GetSchedule, SearchAsync, FilterByStage, FilterByStatus, FilterByPartner, FilterByOM, Pagination, Sort, ExportCSV, ExportPDF, GetPermissions, GetWorkflowHistory, Clone, Archive, Restore, Validate, GetAIDetails, GetCount, GetSummary, model mapping, typeahead, bulk export, GetForUser, notification).

**§2 (70):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (missing OM, invalid partner link, section incomplete for Go, circular relationship, max opportunities per user, duplicate name, stale data, orphan sections, permission cascade, mass assignment).

**§3 (70):** Name/description lengths, list sizes (0–10000), section field limits, team sizes, budget lines, document counts, partner counts, concurrent, Unicode, date ranges, filter combinations, search complexity, export sizes, pagination, version counts, AI response sizes.

**§4 (50):** CRUD lifecycle (15), section management (10), workflow integration (10), search/filter (10), audit (5).
**§5 (50):** Partner service (10), workflow (10), AI (10), export (10), notification (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API security (10).
**§7 (25):** Concurrent CRUD, section edits, workflow + edit, search + update, bulk operations.
**§8 (21):** Validation (5), status logic (5), search parsing (3), mapping (5), completeness (3).
**§9 (16):** GET (<200ms), list (<500ms), search (<500ms), create (<500ms), AI details (<5s), memory.
**§10 (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
