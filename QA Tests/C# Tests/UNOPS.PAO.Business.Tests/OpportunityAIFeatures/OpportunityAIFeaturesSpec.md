# Opportunity AI Features — Test Specification

**Component:** AI-Assisted Opportunity Creation & Editing  
**Created:** 2026-03-09 | **Last Updated:** 2026-03-09  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

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

Tests cover AI-related opportunity functionality per Jira tickets PNO-694, PNO-803, PNO-804, PNO-805, PNO-873:

1. **PNO-694** — AI Assistant must be functional (events table schema, AI response flow)
2. **PNO-803** — Creating AI opportunity with empty name must show specific validation error, not generic
3. **PNO-804** — Editing AI-generated opportunity must enforce mandatory Name (and Description per AC) validation
4. **PNO-805** — Opportunity Manager must be logged-in user, not service account, when creating via AI
5. **PNO-873** — Budget extracted by AI from documents must align with funding partner calculated totals

---

## Source Tickets

| Ticket | Summary | Key Requirement |
|--------|---------|------------------|
| PNO-694 | AI is not working | AI Assistant responsive; events.usage_metadata, events.citation_metadata columns |
| PNO-803 | Generic Error when creating AI Opportunity with empty Name | Specific "Opportunity Name is required" message |
| PNO-804 | System accepts empty Name/Description when editing AI-generated opportunity | Block save, show validation errors |
| PNO-805 | Opportunity Manager is service account when creating via AI | Creator = logged-in user as Opportunity Manager |
| PNO-873 | Budget discrepancy between docs and funding partner total | AI-extracted budget linked to funding partners |

---

## Production Code Reference

- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs` — ApplyAiChangesAsync, CreateOpportunityFromProposalAsync, AssignCreatorAsOpportunityManagerAsync
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs` — AI proposal generation
- `UNOPS.PAO.UNOPSBusiness/Managers/AiContextualService.cs` — partnerBudgets, BuildOpportunityCollectionObjects
- `UNOPS.PAO.Models/Opportunities/ApplyOpportunityAiChangesRequest.cs`
- `UNOPS.PAO.Models/Opportunities/CreateOpportunityFromInteractionsRequest.cs`

---

## Defects Logged (DEF-175 through DEF-179)

| DEF | Title | Component |
|-----|-------|-----------|
| DEF-175 | ApplyAiChangesAsync does not validate empty/whitespace Name | UNOPSOpportunityManager |
| DEF-176 | ApplyAiChangesAsync does not validate empty/whitespace Description (if mandatory per AC) | UNOPSOpportunityManager |
| DEF-177 | CreateOpportunityFromProposalAsync empty Name error message not specific (if generic) | UNOPSOpportunityManager |
| DEF-178 | Budget from AI docs not linked to funding partners (if discrepancy exists) | AiContextualService / UNOPSGeminiManager |
| DEF-179 | CreateOpportunityFromProposalAsync with empty Description creates opportunity (per PNO-803 comment) | UNOPSOpportunityManager |
