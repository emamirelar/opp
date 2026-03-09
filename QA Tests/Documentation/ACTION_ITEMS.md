# Action Items — Developers & QA

**Version:** 1.2  
**Last Updated:** March 9, 2026  
**Status:** Living document — update as items are completed or added  
**Audience:** Development Team, QA Team, Engineering Leadership

> **How to use this document:**  
> - Check off items as they are completed (change `[ ]` to `[x]`)  
> - Add the completion date and who completed it  
> - Add new items at the bottom of the appropriate section  
> - Move completed items to the "Completed Items" section at the bottom periodically  
> - Review this document in sprint planning and retrospectives

### Current State (as of March 9, 2026)

| Metric | Value |
|---|---|
| Total C# test methods | ~10,040 (9,840 [Fact] + 200 [Theory]) |
| Total C# test files | ~290 test files across 3 projects |
| Total Playwright specs | 108 |
| Total Playwright test() calls | 1,629 |
| Page Object Models | 22 |
| Playwright helpers | 13 |
| Playwright JSON fixtures | 6 |
| C# test projects | 4 (FastTests ~175, Business.Tests ~9,600, Presentation.Tests ~245, IntegrationTests ~5,500+) |
| Open DEF-XXX defects | ~135 |
| Resolved DEF-XXX defects | ~8 |
| Active QA-XXX issues | ~11 (2 open + 9 workaround) |
| Resolved QA-XXX issues | ~49 |
| Defect-tagged tests [Trait("Defect")] | ~240 |
| Skipped tests [Fact(Skip=)] | ~44 |
| CI/CD pipeline | Deployed — 11 jobs in `qa-tests.yml` |
| Documentation files | 17 (10 in `QA Tests/Documentation/` + 7 in `QA Tests/`) |
| Test data infrastructure | TestEntityBuilder (C#), Bogus fake data, JSON fixtures (Playwright), workflow-mocks.helper |

---

## Table of Contents

1. [Developer Action Items](#1-developer-action-items)
2. [QA Action Items](#2-qa-action-items)
3. [Joint Dev + QA Action Items](#3-joint-dev--qa-action-items)
4. [Infrastructure & DevOps Items](#4-infrastructure--devops-items)
5. [Completed Items Archive](#5-completed-items-archive)
6. [Historical Reference](#6-historical-reference)

---

## 1. Developer Action Items

### 1.1 Code Quality — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| D-001 | 🔴 High | Fix null safety warnings (CS86xx series) across Data Access, Domain, and Utilities layers | Backend Lead | TBD | ☐ Open | ~100+ warnings. See `DEVELOPER_ACTION_ITEMS_2026-01-15.md` for detailed file list and fix patterns |
| D-002 | 🔴 High | Fix `GetHashCode` override in `Enumeration<T>` (CS0659) | Backend Dev | TBD | ☐ Open | 15-minute fix. `UNOPS.PAO.Utilities/Helpers/Enumeration.cs` |
| D-003 | 🔴 High | Update obsolete `ISystemClock` to `TimeProvider` (CS0618) | Identity Team | TBD | ☐ Open | `UNOPS.PAO.UNOPSIdentity/Authentication/IAPAuthenticationHandler.cs` |
| D-004 | 🟡 Medium | Remove unused fields and variables (CS0169, CS0168, CS0219) | Backend Team | TBD | ☐ Open | `AppDbContext.cs`, `UserResolverService.cs`, `IAPAuthenticationHandler.cs` |
| D-005 | 🟡 Medium | Fix name hiding warnings (CS0108) in `ArtifactDataType`, `ArtifactType`, `EntityArtifact` | Backend Dev | TBD | ☐ Open | Use `new` keyword or rename properties |
| D-006 | 🟡 Medium | Fix type parameter shadowing (CS0693) in `Enumeration<T>` | Backend Dev | TBD | ☐ Open | Rename inner type parameter to `TEnum` |

### 1.2 Test Failures — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| D-007 | 🟠 High | Fix `UNOPSPartnerManager` mock parameter count mismatch (4 tests) | Backend Dev | TBD | ☐ Open | Constructor signature changed, test mocks outdated |
| D-008 | 🟡 Medium | Resolve specification logic mismatch for OrgUnit filtering (2 tests) | Backend Lead | TBD | ☐ Open | Business decision needed: update tests or fix specification |

### 1.3 Shift-Left Responsibilities — Ongoing

| # | Priority | Item | Owner | Frequency | Status | Notes |
|---|---|---|---|---|---|---|
| D-009 | 🔴 High | Write unit tests for all new/changed code before PR | All Devs | Every PR | ☐ Ongoing | Part of Definition of Done |
| D-010 | 🔴 High | Run relevant QA test suite locally before creating PR | All Devs | Every PR | ☐ Ongoing | `dotnet test --filter "FullyQualifiedName~FeatureArea"` |
| D-011 | 🔴 High | Review edge case checklists from QA (Three Amigos) | All Devs | Every story | ☐ Ongoing | Confirm each edge case is covered or documented |
| D-012 | 🟠 High | Fix DEF-XXX defects assigned to you | All Devs | Every sprint | ☐ Ongoing | Check `Defect List for Developers.md` |
| D-013 | 🟡 Medium | Do not weaken test assertions to make tests pass | All Devs | Always | ☐ Ongoing | See `never-weaken-tests.mdc` rule |

### 1.4 Performance & Architecture — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| D-014 | 🟢 Low | Reduce solution build time (target: <2 min full rebuild) | Architecture | TBD | ☐ Open | Investigate dependency graph, split large projects |
| D-015 | 🟢 Low | Add pre-commit hooks for build + smoke test | DevOps | TBD | ☐ Open | Catch issues before CI |

---

## 2. QA Action Items

### 2.1 Test Authoring — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| Q-001 | 🔴 High | Author integration test specs by Day 3-5 of each sprint | QA Team | Every sprint | ☐ Ongoing | Push to repo so devs can run them |
| Q-002 | 🔴 High | Provide edge case checklists per story (Three Amigos) | QA Team | Every sprint | ☐ Ongoing | Attach to JIRA tickets |
| Q-003 | 🟠 High | Maintain 3:1 ratio compliance across all test suites | QA Team | Every sprint | ☐ Ongoing | N, E, F, I each >= 3P |
| Q-004 | 🟠 High | Include requirement traceability headers in all test files | QA Team | Every sprint | ☐ Ongoing | Per `requirement-driven-testing.mdc` |

### 2.2 Playwright & Migration — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| Q-005 | 🔴 High | Continue Katalon to Playwright migration | QA Team | Ongoing | ☐ In Progress | 108 specs + 22 POMs + 1,629 tests + 6 JSON fixtures created. Continue expanding coverage |
| Q-006 | 🟠 High | Add Playwright specs for all critical user journeys | QA Team | TBD | ☐ In Progress | Core journeys covered (login, partners, contacts, opportunities, interactions, workflows, AI, admin, search, oUP). Expand edge cases |
| Q-007 | 🟡 Medium | Expand Page Object Model coverage for all pages | QA Team | TBD | ☐ In Progress | 22 POMs created (including admin, AI, workflow, base-engagements, entity-detail/list). Add POMs for remaining pages |

### 2.3 Defect Management — Ongoing

| # | Priority | Item | Owner | Frequency | Status | Notes |
|---|---|---|---|---|---|---|
| Q-008 | 🔴 High | Maintain `Defect List for Developers.md` (DEF-XXX) | QA Team | Continuous | ☐ Ongoing | Log all production code defects |
| Q-009 | 🔴 High | Maintain `Defect List for QA.md` (QA-XXX) | QA Team | Continuous | ☐ Ongoing | Log all test infrastructure issues |
| Q-010 | 🟠 High | Perform exploratory testing after dev handoff | QA Team | Every story | ☐ Ongoing | 30-60 min time-boxed sessions |
| Q-011 | 🟠 High | Run E2E regression before every release | QA Team | Every release | ☐ Ongoing | Full Playwright suite + manual critical paths |

### 2.4 Documentation & Process — Open

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| Q-012 | 🟡 Medium | Fill in Shift-Left Scorecard each sprint | QA Lead | Every sprint | ☐ Ongoing | `SHIFT_LEFT_SCORECARD.md` — Top 10 dashboard |
| Q-013 | 🟡 Medium | Update testing documentation when processes change | QA Team | As needed | ☐ Ongoing | Keep all docs in `QA Tests/Documentation/` current |
| Q-014 | 🟢 Low | Establish performance test baselines | QA Team | TBD | ☐ Open | Needed for Shift-Left Scorecard metrics |

---

## 3. Joint Dev + QA Action Items

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| J-001 | 🔴 High | Establish Three Amigos sessions for every story | Dev Lead + QA Lead | Next sprint | ☐ Open | PO + Dev + QA per story/group |
| J-002 | 🔴 High | Define and enforce "Ready for QA" gate criteria | Dev Lead + QA Lead | Next sprint | ☐ Open | CI green, tests run locally, checklist reviewed |
| J-003 | 🟠 High | Hold bug triage session at least once per sprint | Dev Lead + QA Lead | Every sprint | ☐ Ongoing | Review open DEF-XXX and QA-XXX issues |
| J-004 | 🟠 High | Include test metrics in sprint demo | Scrum Master | Every sprint | ☐ Ongoing | Coverage, pass rate, defect escape rate |
| J-005 | 🟡 Medium | Discuss shift-left progress in retrospectives | Scrum Master | Every sprint | ☐ Ongoing | Use questions from `SHIFT_LEFT_SCORECARD.md` Section 10 |
| J-006 | 🟡 Medium | Conduct blame-free post-mortems for escaped defects | Dev Lead + QA Lead | As needed | ☐ Ongoing | Focus on "which gate failed" not "who" |
| J-007 | 🟡 Medium | Start collecting baseline metrics for Shift-Left Scorecard | Dev Lead + QA Lead | This sprint | ☐ Open | Tag JIRA defects with `found_in_phase` |
| J-008 | 🟢 Low | Review and update shift-left maturity checklist quarterly | Dev Lead + QA Lead | Quarterly | ☐ Open | `SHIFT_LEFT_SCORECARD.md` Section 9 |

---

## 4. Infrastructure & DevOps Items

| # | Priority | Item | Owner | Target Sprint | Status | Notes |
|---|---|---|---|---|---|---|
| I-001 | 🟠 High | Add build performance monitoring to GitHub Actions | DevOps | TBD | ☐ Open | Track build + test execution times. CI pipeline is deployed (qa-tests.yml) with 11 jobs |
| I-002 | 🟡 Medium | Automate `found_in_phase` tagging in JIRA | DevOps | TBD | ☐ Open | Webhook or automation rule |
| I-003 | 🟡 Medium | Set up staging environment for integration test validation | DevOps | TBD | ☐ Open | Real PostgreSQL + Google Cloud creds + AI service |
| I-004 | 🟢 Low | Create script to count open DEF-XXX/QA-XXX defects | DevOps | TBD | ☐ Open | Parse markdown files, report in CI |
| I-005 | 🟢 Low | Add PR template checkbox: "I have run the relevant QA test suite locally" | DevOps | TBD | ☐ Open | Track dev test execution rate |

---

## 5. Completed Items Archive

Move items here when completed. Include the completion date and who completed it.

| # | Item | Completed By | Date | Notes |
|---|---|---|---|---|
| ~~D-100~~ | Fixed build timeout issue (critical blocker) | QA Team | 2026-01-15 | `dotnet build-server shutdown` + clean build |
| ~~D-101~~ | Fixed Secret Manager access in Startup.cs | QA Team | 2026-01-15 | Testing environment check added |
| ~~D-102~~ | Fixed gRPC authentication failures (17 tests) | QA Team | 2026-01-16 | Test mode detection in `AiContextualService` |
| ~~D-103~~ | Fixed legacy search endpoint missing (15 tests) | QA Team | 2026-01-16 | Backward-compatible endpoint added |
| ~~D-104~~ | Fixed PostgreSQL similarity function fallback (8 tests) | QA Team | 2026-01-16 | In-memory Levenshtein distance algorithm |
| ~~D-105~~ | Fixed DbContextFactory not registered (5 tests) | QA Team | 2026-01-16 | Registered in test DI container |
| ~~D-106~~ | Fixed French date parsing support (1 test) | QA Team | 2026-01-16 | Added FR, ES, PT relative date terms |
| ~~Q-100~~ | Created QA Tester Playbook | QA Lead | 2026-03-05 | `QA_TESTER_PLAYBOOK.md` |
| ~~Q-101~~ | Created Shift-Left Testing Manifesto | QA Lead | 2026-03-05 | `SHIFT_LEFT_TESTING_MANIFESTO.md` |
| ~~Q-102~~ | Created Onboarding Guide | QA Lead | 2026-03-05 | `ONBOARDING_GUIDE.md` |
| ~~Q-103~~ | Created Shift-Left Scorecard | QA Lead | 2026-03-06 | `SHIFT_LEFT_SCORECARD.md` |
| ~~Q-104~~ | Updated all Documentation files to current state | QA Lead | 2026-03-06 | All 9 docs updated with current test counts, CI pipeline, defect stats |
| ~~Q-105~~ | Implemented C# TestEntityBuilder fluent builders | QA Team | 2026-03-09 | Fluent builders for User, Partner, Opportunity, Currency, Country, SDG, OrgHierarchy, Contact, Interaction, EntityRole, InitiativeType, Output in `TestBase/TestEntityBuilder.cs` |
| ~~Q-106~~ | Unified C# test user creation pattern | QA Team | 2026-03-09 | Replaced raw SQL user creation with `TestDataHelper.GetOrCreateTestUserAsync()` across fixtures |
| ~~Q-107~~ | Added opt-in SQLite FK enforcement | QA Team | 2026-03-09 | `TestEnvironment.EnableForeignKeys` — set `SQLITE_ENABLE_FK=true` to detect FK constraint violations |
| ~~Q-108~~ | Extracted Playwright inline mock data to JSON fixtures | QA Team | 2026-03-09 | 6 JSON fixture files in `fixtures/`: reference-data, partners, contacts, opportunities, interactions, dashboard |
| ~~Q-109~~ | Fixed TestDataSeeder URL patterns | QA Team | 2026-03-09 | Corrected plural (`/api/partners/*`) to singular (`/api/partner/*`) to match actual API |
| ~~Q-110~~ | Isolated Playwright workflowMockState | QA Team | 2026-03-09 | `workflowMockState` now resets per-test via `resetWorkflowMockState()` — no more shared mutable state |
| ~~Q-111~~ | Created shared workflow-mocks.helper.ts | QA Team | 2026-03-09 | Centralized notification, opportunity detail, permission, and pending-approval mock helpers. Used by 4 spec files |
| ~~Q-112~~ | Added Bogus NuGet package for realistic fake data | QA Team | 2026-03-09 | `Bogus 35.6.5` integrated into `UNOPS.PAO.Business.Tests.csproj` |
| ~~Q-113~~ | Documented test data conventions in QA Playbook | QA Team | 2026-03-09 | Section 5.4 in `QA_TESTER_PLAYBOOK.md` covers all C# and Playwright test data patterns |
| ~~Q-114~~ | Updated all Documentation files with March 9 test results | QA Lead | 2026-03-09 | Updated TESTING_STRUCTURE, ACTION_ITEMS, ONBOARDING_GUIDE, SHIFT_LEFT_SCORECARD, DEV_CHECKLIST with latest counts |
| ~~I-100~~ | Deployed CI/CD pipeline (qa-tests.yml) | QA Team | 2026-02 | 11-job pipeline: build, smoke, fast, business, presentation, frontend, integration, defect, playwright (3 tiers) |
| ~~J-100~~ | Consolidated all action items into living document | QA Lead | 2026-03-06 | This document |

---

## 6. Historical Reference

The following point-in-time documents were created during the January 2026 test infrastructure stabilization effort. Their relevant items have been consolidated into this document. The dated files have been removed to avoid confusion — this single `ACTION_ITEMS.md` is the only active tracking document.

**Removed dated files (content merged here):**
- `DEVELOPER_ACTION_ITEMS_2026-01-14.md` — Initial JIRA defect analysis (34 production bugs)
- `DEVELOPER_ACTION_ITEMS_2026-01-15.md` — Build timeout fix + code quality items
- `DEVELOPER_ACTION_ITEMS_FINAL_2026-01-15.md` — Post-fix analysis (95.2% pass rate)
- `DEVELOPER_ACTION_ITEMS_FINAL_UPDATED_2026-01-15.md` — Updated after skip attributes
- `DEVELOPER_ACTION_ITEMS_2026-01-15_FINAL.md` — Final state (98.4% pass rate)
- `DEVELOPER_ACTION_ITEMS_2026-01-16_UPDATED.md` — Post-commit report (46 tests fixed)

**Remaining reference documents in `QA Tests/Documentation/`:**

| Document | Purpose | Status |
|---|---|---|
| `DEVELOPER_IMPLEMENTATION_CHECKLIST.md` | Opportunity feature implementation checklist | Historical reference |
| `PRODUCTION_READINESS_CHECKLIST.md` | Production deployment readiness checklist | Active reference |
| `OpportunityPlus_oUP_Integration_Checklist.md` | oUP integration field mapping validation | Active reference |

---

## How to Add a New Action Item

1. Choose the right section (Developer, QA, Joint, or Infrastructure)
2. Use the next available number in that section (D-0XX, Q-0XX, J-0XX, I-0XX)
3. Set priority: 🔴 Critical, 🟠 High, 🟡 Medium, 🟢 Low
4. Include: what needs to be done, who owns it, target sprint
5. When complete: check the box, add date and name, then periodically move to Section 5

---

## Version History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | 2026-03-06 | QA Lead | Initial consolidated document. Merged items from 6 historical developer action item files + 3 checklists. Added ongoing shift-left responsibilities. |
| 1.1 | 2026-03-06 | QA Lead | Added Current State summary table. Updated Playwright items (Q-005/Q-006/Q-007) to reflect 102 specs and 21 POMs. Added completed items (Q-104, I-100). |
| 1.2 | 2026-03-09 | QA Lead | Major metrics refresh: C# tests now 10,040 methods (up from ~3,800 Business.Tests). Playwright at 108 specs / 1,629 tests / 22 POMs / 13 helpers / 6 fixtures. DEF count updated to 135 open. Added 10 completed items (Q-105 through Q-114) for test data infrastructure improvements: fluent builders, Bogus, JSON fixtures, workflow-mocks.helper, SQLite FK enforcement, data isolation. |
