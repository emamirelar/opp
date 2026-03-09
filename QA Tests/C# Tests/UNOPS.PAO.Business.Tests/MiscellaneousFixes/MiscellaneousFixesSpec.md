# Miscellaneous Fixes — Test Specification

**Component:** PNO-805 (AI OM Assignment), PNO-801 (Side Panel Navigation)  
**Created:** 2026-03-09 | **Last Updated:** 2026-03-09  
**Author:** QA Team  
**Standard:** 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | Status |
|----------|-------|-----|--------|
| §1 Positive   | 10 | 10  | PASS |
| §2 Negative   | 30 | 30  | PASS |
| §3 Boundary   | 30 | 30  | PASS |
| §4 Functional | 30 | 30  | PASS |
| §5 Integration| 30 | 30  | PASS |
| **TOTAL**     | **130** | **≥130** | PASS |

**3:1 Ratio Checks:**
- N ≥ 3P: 30 ≥ 30 → PASS
- B ≥ 3P: 30 ≥ 30 → PASS
- F ≥ 3P: 30 ≥ 30 → PASS
- I ≥ 3P: 30 ≥ 30 → PASS

---

## Feature Overview

Consolidated test suite for remaining standalone tickets:

1. **PNO-805** — Opportunity created via AI: Opportunity Manager must be logged-in user (not service account)
2. **PNO-801** — Remove "Leads" and "Initiatives" from side panel (or replace with Project+/Projects)

---

## Source Tickets

| Ticket | Summary | Key Requirement |
|--------|---------|------------------|
| PNO-805 | Opportunity Manager is service account when creating via AI | Creator = logged-in user as Opportunity Manager |
| PNO-801 | Remove Leads and Initiatives from side panel | Side panel must not show Leads/Initiatives (or show Project+/Projects) |

---

## Production Code Reference

- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` — CreateOpportunityFromProposalAsync, AssignCreatorAsOpportunityManagerAsync
- `UNOPS.PAO.ClientApp/src/app/layouts/components/sidebar/sidebar.component.ts` — menu items (Leads/Initiatives commented out)
- `UNOPS.PAO.ClientApp/src/app/layouts/components/layout/breadcrumb/breadcrumb.component.ts` — labelMap (Leads/Initiatives mappings)

---

## PNO-801 Scope Note

PNO-801 is a **frontend-only** change. The sidebar component has Leads and Initiatives commented out (sidebar.component.ts lines 95-104). Backend C# tests cannot validate Angular UI. Full validation requires:
- **Playwright E2E** tests in `UNOPS.PAO.ClientApp/src/qa-frontend-tests/` to verify sidebar does not display Leads/Initiatives
- Breadcrumb labelMap still contains 'Leads' and 'Initiatives' — legacy mappings for URL-based navigation; consider cleanup (DEF-217)

---

## Defects Logged (DEF-217+)

| DEF | Title | Component |
|-----|-------|-----------|
| DEF-217 | Breadcrumb labelMap contains legacy Leads/Initiatives mappings — PNO-801 | breadcrumb.component.ts |
