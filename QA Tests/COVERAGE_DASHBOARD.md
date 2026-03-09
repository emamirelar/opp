# Test Coverage Dashboard — UNOPS Opportunity+

**Last Updated:** 2026-03-09  
**Status:** Active — Updated after each test creation session  
**Governed by:** `.cursor/rules/test-coverage-matrix.mdc`

---

## Executive Summary

| Metric | Value | Target | Status |
|---|---|---|---|
| **Total C# Test Files** | ~313 files (~10,630 test methods) | — | — |
| **Total Playwright Specs** | 110 (1,669 tests) | — | — |
| **Tier 1 Coverage** | 5/5 types | 5/5 | ✅ Complete |
| **Tier 2 Coverage** | 5/5 types | 5/5 | ✅ Complete |
| **Tier 3 Coverage** | 6/6 types | 6/6 | ✅ Complete (all gaps closed 2026-03-09) |
| **Tier 4 Coverage** | 4/4 types | 4/4 | ✅ Complete (axe-core added 2026-03-09) |

---

## Tier 1 — Core Test Types (BLOCKING)

| # | Test Type | Files | Est. Tests | Status | Notes |
|---|---|---|---|---|---|
| T1 | Positive | 56 | ~560 | ✅ Complete | All major features covered |
| T2 | Negative | 65 | ~650 | ✅ Complete | 3:1 ratio enforced |
| T3 | Boundary | 45 | ~450 | ✅ Complete | Includes soft-delete edge cases |
| T4 | Functional | 62 | ~620 | ✅ Complete | Business rules, audit, permissions |
| T5 | Integration | 73 | ~730 | ✅ Complete | Full CRUD, cross-service flows |

**Tier 1 Health:** All 5 core types are present with healthy coverage.

---

## Tier 2 — Extended Test Types (REQUIRED)

| # | Test Type | Files | Est. Tests | Status | Gap Action |
|---|---|---|---|---|---|
| T6 | Unit | 16 | ~160 | ✅ Complete | Validation, formatting, calculations |
| T7 | Concurrency | 10 | ~100 | ✅ Complete | Race conditions, double-submit |
| T8 | Performance | 29 | ~290 | ✅ Complete | Response time, N+1 detection |
| T9 | Load | 27 | ~270 | ✅ Complete | Sustained, spike, stress |
| T10 | Data Entry Permutations | 16 | ~627 | ✅ Complete | Created 2026-03-09, all 16 entities |

**Tier 2 Health:** All 5 extended types are present.

---

## Tier 3 — Cross-Cutting Test Types (REQUIRED — system-wide)

| # | Test Type | Files | Est. Tests | Status | Priority | Gap Action |
|---|---|---|---|---|---|---|
| T11 | Security | 20 | ~200 | ✅ Exists | — | Auth, injection, CSRF covered |
| T12 | API Contract | 1 | 65 | ✅ Created | — | `ApiContractTests.cs` — 65 tests covering all endpoint groups |
| T13 | Accessibility | 2 | ~25 | ✅ Enhanced | — | `accessibility.spec.ts` (manual) + `accessibility-axe.spec.ts` (axe-core, 15 tests) |
| T14 | Internationalization (i18n) | 1 | 50 | ✅ Created | — | `I18nTests.cs` — 50 tests for translation completeness |
| T15 | Error Recovery | 2 | ~60 | ✅ Created | — | `ErrorRecoveryTests.cs` — 50 tests for graceful degradation |
| T16 | Rate Limiting | 2 | ~40 | ✅ Enhanced | — | `RateLimitingTests.cs` — 30 tests (8 with DEF-220 for missing middleware) |

**Tier 3 Health:** All 6 types now have coverage. API contract (65 tests), i18n (50 tests), error recovery (50 tests), and rate limiting (30 tests) created 2026-03-09.

---

## Tier 4 — E2E Test Types (Playwright)

| # | Test Type | Specs | Status | Gap Action |
|---|---|---|---|---|
| T17 | E2E Smoke | 6 | ✅ Complete | Core CRM flows covered |
| T18 | E2E Feature | 110 total (1,669 tests) | ✅ Complete | All major features have specs. PNO-669, PNO-1182 E2E added 2026-03-09 |
| T19 | E2E Cross-Browser | Manual trigger | ⚠️ Exists | Chrome, Firefox, WebKit configured |
| T20 | E2E Accessibility (axe-core) | 1 with axe | ✅ Created | — | `accessibility-axe.spec.ts` with 15 axe-core WCAG tests |

**Tier 4 Health:** All 4 types now have coverage. axe-core integration added 2026-03-09 (requires `npm install -D @axe-core/playwright`).

---

## Gap Tracker — Action Items

| Priority | Gap | Assigned To | Target Sprint | Status | Tracking |
|---|---|---|---|---|---|
| **P1** | ~~T12: API Contract Tests~~ | QA Team | Current | ✅ Complete | 65 tests in `ApiContractTests.cs` |
| **P2** | ~~T13: axe-core Playwright integration~~ | QA Team | Current | ✅ Complete | 15 tests in `accessibility-axe.spec.ts` |
| **P2** | ~~T14: i18n Test Suite~~ | QA Team | Current | ✅ Complete | 50 tests in `I18nTests.cs` |
| **P3** | ~~T15: Error Recovery expansion~~ | QA Team | Current | ✅ Complete | 50 tests in `ErrorRecoveryTests.cs` |
| **P3** | ~~T16: Rate Limiting verification~~ | QA Team | Current | ✅ Complete | 30 tests in `RateLimitingTests.cs` (DEF-220) |
| **P3** | ~~T9: Performance baselines~~ | QA Team | Current | ✅ Complete | 30 tests in `PerformanceBaselineTests.cs` |
| **P4** | ~~E2E Scenario Gaps (PSS-007–012)~~ | QA Team | Current | ✅ Complete | 6 tests added to `product-service-search.spec.ts` |

---

## Feature-Level Coverage Matrix

### Legend
- ✅ = Tests exist and pass minimum counts
- ⚠️ = Tests exist but below minimum or incomplete
- ❌ = No tests exist
- N/A = Not applicable to this feature

| Feature | T1 Pos | T2 Neg | T3 Bnd | T4 Fnc | T5 Int | T6 Unt | T7 Con | T8 Perf | T9 Load | T10 Perm | T12 API | T15 Err |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Partner | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Contact | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Interaction | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Opportunity | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Go Decision | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Document | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ | ⚠️ | ⚠️ | N/A | ❌ | ❌ |
| Risk | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❌ | ❌ |
| Stakeholder | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❌ | ❌ |
| Notification | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | N/A | ❌ | ❌ |
| Search | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ✅ | ✅ | ❌ | ❌ |
| AI | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ✅ | ✅ | ❌ | ❌ |
| User Mgmt | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❌ | ❌ |
| Entity Config | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❌ | ❌ |

### System-Wide Cross-Cutting Coverage

| Test Type | Status | File Count | Notes |
|---|---|---|---|
| T11 Security | ✅ | 20 | Auth, injection, CSRF |
| T12 API Contract | ✅ | 1 | 65 tests — status codes, response shapes, error contracts |
| T13 Accessibility | ✅ | 2 | Manual + axe-core WCAG 2.1 AA scans |
| T14 i18n | ✅ | 1 | 50 tests — translation completeness, placeholders, key consistency |
| T15 Error Recovery | ✅ | 2 | 50 tests — exception handling, graceful degradation, ProblemDetails |
| T16 Rate Limiting | ✅ | 2 | 30 tests — throttle enforcement, headers, DEF-220 for missing middleware |

---

## How to Update This Dashboard

### After Creating Tests
1. Update the appropriate row in the Feature-Level Coverage Matrix
2. Update the file counts in the Tier summary tables
3. Move completed gap items from "Not Started" to "Complete"
4. Update the Last Updated date

### After Each Sprint
1. Review all ❌ and ⚠️ items
2. Prioritize the highest-priority gaps
3. Create sprint backlog items for gap closure
4. Update status after sprint completion

### Automated Update (Future)
A CI job should be created to automatically count test files by Trait category and update this dashboard. Until then, manual updates are required after each test creation session.

---

## Version History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | 2026-03-09 | QA Team | Initial dashboard creation. Baseline coverage analysis across all 4 tiers and 16 test types. Identified T12 (API Contract) and T14 (i18n) as critical gaps. |
| 1.1 | 2026-03-09 | QA Team | All gaps closed: API Contract (65 tests), i18n (50 tests), Error Recovery (50 tests), Rate Limiting (30 tests), Performance Baselines (30 tests), axe-core Playwright (15 tests), PSS-007–012 E2E specs (6 tests). Updated create-tests agent with coverage matrix check. |
| 1.2 | 2026-03-09 | QA Team | PNO-1166/PNO-669/PNO-1182 test session: C# suites created and repaired (169 QATestingCode, 170 MobileSidebarClose, ~170 LabelAlignment). Playwright specs added (sidebar-mobile-close, when-date-label-alignment). DEF-251 logged (OpportunityWhenSection missing features). QA-106 (InMemory deprecation), QA-107 (authenticateWithMocks resolved). |
| 1.3 | 2026-03-09 | QA Team | dev-deploy merge test coverage: SDGProcessing (130 tests — dedup + primary fallback), ConnectionStringEncoding (156 tests — UTF-8 encoding fix), DeliverablesProposal (136 tests — model/template refactor). All 422 new tests passing. |
