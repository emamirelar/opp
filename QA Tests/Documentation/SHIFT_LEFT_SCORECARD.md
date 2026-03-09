# Shift-Left Testing Scorecard — Measuring Success

**Version:** 1.2  
**Date:** March 9, 2026  
**Status:** Living document — update each sprint  
**Audience:** Solution Designers, PM/BAs, QA Lead, Engineering Lead, Scrum Masters, Product Owners, Engineering Leadership  
**Companion to:** [Shift-Left Testing Manifesto](SHIFT_LEFT_TESTING_MANIFESTO.md)

---

## Table of Contents

1. [Purpose](#1-purpose)
2. [Leading Indicators — Are We Shifting Left?](#2-leading-indicators--are-we-shifting-left)
3. [Quality Indicators — Is It Working?](#3-quality-indicators--is-it-working)
4. [Process Indicators — Are People Adopting the Practices?](#4-process-indicators--are-people-adopting-the-practices)
5. [Toolchain Utilization — Are We Using the Tools Effectively?](#5-toolchain-utilization--are-we-using-the-tools-effectively)
6. [Sprint-over-Sprint Dashboard](#6-sprint-over-sprint-dashboard)
7. [How You'll Know You're Succeeding](#7-how-youll-know-youre-succeeding)
8. [Data Collection Guide](#8-data-collection-guide)
9. [Maturity Model](#9-maturity-model)
10. [Retrospective Questions](#10-retrospective-questions)

---

## 1. Purpose

The Shift-Left Testing Manifesto defines *what* we do. This scorecard defines *how we know it's working*. It provides concrete, measurable criteria across four dimensions and a sprint-over-sprint tracking template.

**Update cadence:** Fill in the dashboard (Section 6) at the end of every sprint. Review trends monthly with leadership.

---

## 2. Leading Indicators — Are We Shifting Left?

Leading indicators tell you if the *process* is changing — are testing activities happening earlier?

### 2.1 Test Authorship Timing

| Metric | Lagging (Right) | Leading (Left) | How to Measure |
|---|---|---|---|
| When are tests written? | After code is merged | Before or alongside code | JIRA: compare test task completion date vs. dev task completion date |
| Who writes the first test? | QA after handoff | Dev or AI during development | Git blame on test files — dev vs. QA commits |
| Test PR coupling | Tests in separate PRs, days later | Tests in the same PR as production code | GitHub: % of PRs that include test files |
| Defect discovery phase | Found in QA/staging | Found during dev via failing spec tests | JIRA: defect `created_date` vs. sprint phase |

### 2.2 Pre-Development Validation (Gate 0)

These metrics track whether requirements and design are validated *before* code is written — the cheapest place to find defects (see Manifesto Section 4).

| Metric | Baseline | Target | How to Measure |
|---|---|---|---|
| Gate 0 Pass Rate | Not tracked | >95% | % of stories entering the sprint with PO-confirmed, testable ACs. Track via Jira "PO Approved" label or comment. |
| Requirements Change Rate | Not tracked | Decreasing trend | Number of AC changes after sprint commitment per sprint. Count Jira AC edits or comments indicating post-commitment changes. |
| Design Traceability Score | Not tracked | >90% | % of complex stories with a design-to-requirements traceability table. Verify during refinement. |
| Stories rejected at Gate 0 | Not tracked | <10% | Stories returned to backlog during refinement due to incomplete/untestable ACs. |
| PM/BA requirements quality | Not tracked | Increasing | Number of requirement-originated defects per sprint (should decrease). |

### 2.3 AI-Assisted Test Generation

| Metric | Baseline | Target | How to Measure |
|---|---|---|---|
| % of test files generated via Cursor/Claude | 0% | >60% | Track `create-tests` subagent invocations in agent transcripts |
| Time from JIRA story to first test file | Days | Hours | JIRA story transition timestamp vs. first test commit |
| Defect-exposing tests created proactively | Rare | Standard practice | Count of `[Trait("Defect", "DEF-XXX")]` tests created *before* bug is reported by end users |
| Requirement traceability coverage | Ad hoc | 100% of acceptance criteria mapped | Compliance with requirement traceability table in test file headers |

---

## 3. Quality Indicators — Is It Working?

Quality indicators tell you if the shift is *improving outcomes* — are we catching more bugs earlier?

### 3.1 Defect Migration

The most important shift-left metric: *where* defects are found. Over time, the distribution should shift toward Dev/CI.

| Metric | Poor | Good | Excellent |
|---|---|---|---|
| % defects found in dev/CI | <20% | 40-60% | >70% |
| % defects found in QA/staging | >60% | 30-40% | <20% |
| % defects found in production | >20% | <10% | <5% |
| Mean time to detect (MTTD) | Weeks | Days | Hours |

**How to measure:** JIRA — tag defects with `found_in_phase` label (Dev, CI, QA, Staging, Prod). Calculate the ratio at the end of each sprint.

### 3.2 Test Effectiveness

| Metric | How to Measure | Target |
|---|---|---|
| 3:1 ratio compliance | % of test suites passing all 4 ratio checks (N, E, F, I each >= 3P) | 100% |
| Defect escape rate | Defects found in prod / total defects | <5% |
| CI defect test failure trend | `dotnet test --filter "Defect~DEF"` — count of known defects still failing | Trending down sprint-over-sprint |
| Regression rate | Defects reopened / defects closed | <10% |
| Open DEF-XXX count | Count of open entries in `Defect List for Developers.md` | Trending down |
| Open QA-XXX count | Count of open entries in `Defect List for QA.md` | Stable or trending down |

---

## 4. Process Indicators — Are People Adopting the Practices?

Process indicators tell you if the *behaviors* are changing — are devs and QAs working toward the shift-left goal?

### 4.1 Developer Behaviors

| Behavior | Evidence to Look For | Where to Check | Current? (Y/N) |
|---|---|---|---|
| Devs run tests locally before PR | CI first-pass success rate >85% | GitHub Actions pipeline stats | |
| Devs write unit tests with their code | Test files committed in same PR as source | Git PR file lists | |
| Devs use Cursor/Claude for test generation | Agent transcripts, `create-tests` usage | Cursor agent transcript folder | |
| Devs review QA-generated tests | PR review comments on test files from dev accounts | GitHub PR reviews | |
| Devs fix `DEF-XXX` defects proactively | DEF defect status changes to "In Progress" without QA escalation | JIRA defect board + `Defect List for Developers.md` | |
| Devs follow the edge case checklist | Checklist items are addressed in PR description or code comments | JIRA ticket comments + PR descriptions | |
| Devs do not weaken test assertions | No PRs that change assertions to match broken behavior | PR review discipline | |

### 4.2 QA Behaviors

| Behavior | Evidence to Look For | Where to Check | Current? (Y/N) |
|---|---|---|---|
| QA writes tests from requirements (spec-based) | Requirement traceability tables present in test file headers | Test file headers per `requirement-driven-testing.mdc` | |
| QA creates tests before/during dev, not after | Test PRs overlap or precede dev PRs in the sprint | JIRA + Git timeline | |
| QA uses Playwright for E2E from sprint start | Playwright spec files created in first half of sprint | Git commit dates for `*.spec.ts` files | |
| QA logs defects with proper fix guidance | `Defect List for Developers.md` entries include Root Cause + Proper Fix | Review defect file quality | |
| QA provides edge case checklists per story | Checklists attached to JIRA tickets after Three Amigos | JIRA ticket attachments/comments | |
| QA uses AI (Cursor/Claude) to author tests | Agent transcripts show test authoring patterns | Cursor agent transcript folder | |
| QA automates, not manual regression | Ratio of automated vs. manual test cases increasing | JIRA test management, Confluence test plans | |

### 4.3 PM/BA Behaviors

| Behavior | Evidence to Look For | Where to Check | Current? (Y/N) |
|---|---|---|---|
| PM/BA validates ACs with PO before sprint commitment | Stories have "PO Approved" label or sign-off comment | JIRA tickets | |
| ACs are testable (specific, measurable, unambiguous) | QA can derive test cases directly from ACs without clarification | JIRA AC text quality review | |
| PM/BA documents business rules and edge cases | Business rule documentation attached to stories | JIRA attachments/comments, Confluence | |
| PM/BA participates in Three Amigos sessions | Meeting attendance and contribution to edge case checklists | JIRA ticket comments, calendar invites | |
| Requirements changes after commitment are rare | AC edit count after sprint commitment is low | JIRA history on AC fields | |
| PM/BA provides cross-feature impact assessment | New stories include impact notes for related features | JIRA ticket descriptions | |
| PM/BA confirms Gate 0 for all sprint stories | All committed stories have passed Gate 0 at sprint planning | Sprint planning notes, JIRA board | |

### 4.4 Solution Designer Behaviors

| Behavior | Evidence to Look For | Where to Check | Current? (Y/N) |
|---|---|---|---|
| SD validates design against requirements before dev starts | Design-to-requirements traceability table exists for complex stories | Confluence, JIRA attachments | |
| SD participates in Design Walkthrough ceremony | Design walkthrough meeting held within Day 1-2 of sprint | Calendar invites, meeting notes | |
| SD identifies NFR risks (performance, security, scalability) | NFR risks documented and shared with dev team | Confluence, JIRA ticket comments | |
| SD participates in testability review with QA | QA confirms design is testable before dev begins | Meeting notes, JIRA comments | |
| SD provides edge case input during Three Amigos | Design-specific edge cases are documented | JIRA edge case checklists | |
| SD reviews design changes during sprint | If design changes mid-sprint, SD re-validates traceability | JIRA comments, Confluence updates | |

### 4.5 Team Collaboration Behaviors

| Behavior | Evidence to Look For | Where to Check | Current? (Y/N) |
|---|---|---|---|
| Three Amigos sessions happen per story | Edge case checklists exist for each story | JIRA tickets | |
| QA test specs pushed by Day 3-5 | Git commit timestamps for test files | Git log | |
| Bug triage sessions held each sprint | Meeting notes or JIRA board updates | Confluence / JIRA | |
| Sprint demo includes test metrics | Demo slide deck or recording | Sprint demo artifacts | |
| Retro discusses shift-left progress | Retro notes reference testing practices | Confluence retro notes | |
| Post-mortems held for escaped defects | Post-mortem documents exist | Confluence | |

---

## 5. Toolchain Utilization — Are We Using the Tools Effectively?

### 5.1 Cursor + Claude

| Metric | How to Measure | Target |
|---|---|---|
| `create-tests` agent usage per sprint | Count agent invocations in transcript folder | Increasing trend |
| Test generation acceptance rate | % of AI-generated tests committed without major rework | >70% |
| Average tests per agent invocation | Total tests generated / total invocations | Increasing trend |
| Requirement-driven prompts | % of agent prompts that include JIRA acceptance criteria | >80% |
| Dev AI usage for unit tests | Developer agent transcripts with test-related prompts | Increasing trend |

### 5.2 Playwright

| Metric | How to Measure | Target |
|---|---|---|
| E2E test coverage by feature | Features with Playwright specs / total features | Increasing toward 100% |
| Playwright test count | Total `*.spec.ts` test count in `QA Tests/Playwright Tests/` (baseline: 108 specs / 1,629 tests) | Increasing trend |
| E2E suite execution time | CI pipeline duration for Playwright job | <30 minutes |
| Flaky test rate | Tests that fail intermittently / total E2E tests | <5% |
| Page Object Model coverage | Pages with POMs / total application pages (baseline: 22 POMs) | Increasing toward 100% |
| Katalon tests remaining | Tests in Katalon not yet migrated to Playwright | Trending to 0 |
| Playwright helper/fixture maturity | Helpers (13) + JSON fixtures (6) + workflow-mocks centralized | Stable or expanding |

### 5.3 JIRA

| Metric | How to Measure | Target |
|---|---|---|
| Stories with linked test tasks | % of stories that have a sub-task or linked issue for tests | >90% |
| Defect lead time | Average time from defect creation to resolution | Trending down |
| Defect age in backlog | Average days DEF-XXX issues stay open | Trending down |
| Test task completion within sprint | % of test tasks done in same sprint as dev tasks | >80% |
| Defects tagged with `found_in_phase` | % of defects with environment label | 100% |

### 5.4 Confluence

| Metric | How to Measure | Target |
|---|---|---|
| PRDs with testable acceptance criteria | % of PRDs that have numbered, testable acceptance criteria | >90% |
| Test plans linked to stories | % of features with Confluence test plans | Increasing trend |
| Living documentation updates | Are test plans updated when requirements change? | Yes (spot check) |
| Post-mortem documents | Post-mortems exist for production-escaped defects | 100% of prod defects |

---

## 6. Sprint-over-Sprint Dashboard

### Baseline Snapshot — March 9, 2026

This baseline was captured before the first sprint dashboard entry. Use these values as the "Sprint 0" reference point.

| Metric | Baseline Value | Notes |
|---|---|---|
| **C# test methods** | 10,040 | 9,840 [Fact] + 200 [Theory] across ~290 files |
| **C# test projects** | 4 | FastTests (~175), Business.Tests (~9,600), Presentation.Tests (~245), IntegrationTests (~5,500+) |
| **Playwright spec files** | 108 | 1,629 test() calls, 302 describe() blocks |
| **Playwright POMs** | 22 | Full Page Object Model coverage for core pages |
| **Playwright helpers** | 13 | Including workflow-mocks.helper, api-mocks.helper, test-data-builder |
| **Playwright fixtures** | 6 | JSON mock data files (reference-data, partners, contacts, opportunities, interactions, dashboard) |
| **Open DEF-XXX** | ~135 | Production code defects |
| **Resolved DEF-XXX** | ~8 | |
| **Active QA-XXX** | ~11 | 2 open + 9 workaround |
| **Resolved QA-XXX** | ~49 | |
| **Defect-tagged tests** | ~240 | `[Trait("Defect", "DEF-XXX")]` — run in non-blocking CI job |
| **Skipped tests** | ~44 | `[Fact(Skip = ...)]` — QA infrastructure issues |
| **Test data infrastructure** | Operational | TestEntityBuilder (C#), Bogus fake data, JSON fixtures (Playwright), workflow-mocks.helper |
| **CI/CD pipeline** | 11 jobs | Build, smoke, fast, business, presentation, frontend, integration, defect, playwright (3 tiers), summary |

### Top 12 Metrics Dashboard

Copy this table and fill it in at the end of every sprint. Track trends with arrows.

| # | Metric | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Trend |
|---|---|---|---|---|---|---|---|
| 1 | Gate 0 Pass Rate (% stories with PO-confirmed ACs) | | | | | | |
| 2 | Requirements Change Rate (AC changes post-commitment) | | | | | | |
| 3 | % PRs with test files included | | | | | | |
| 4 | % defects found in Dev/CI (vs. QA/Prod) | | | | | | |
| 5 | 3:1 ratio compliance rate | | | | | | |
| 6 | Defect escape rate (prod defects / total) | | | | | | |
| 7 | Mean time: story creation to first test commit | | | | | | |
| 8 | AI-generated test acceptance rate | | | | | | |
| 9 | Playwright E2E coverage % (features covered) | | | | | | |
| 10 | Open DEF-XXX count | | | | | | |
| 11 | CI first-pass success rate (PRs green on first try) | | | | | | |
| 12 | Dev test execution rate (devs who ran QA tests pre-PR) | | | | | | |

### Defect Distribution Dashboard

| Found In | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Target |
|---|---|---|---|---|---|---|
| Dev (local) | | | | | | Increasing |
| CI (automated) | | | | | | Increasing |
| QA (manual) | | | | | | Decreasing |
| Staging/UAT | | | | | | Decreasing |
| Production | | | | | | <5% |
| **Total Defects** | | | | | | |

### Test Suite Growth Dashboard

| Suite | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Sprint __ | Trend |
|---|---|---|---|---|---|---|
| Unit tests (dev-authored) | | | | | | |
| Business logic tests (QA) | | | | | | |
| Integration tests (QA) | | | | | | |
| Functional tests (QA) | | | | | | |
| Playwright E2E specs | | | | | | |
| Katalon tests remaining | | | | | | |
| **Total automated tests** | | | | | | |

---

## 7. How You'll Know You're Succeeding

### Green Flags — Shift-Left Is Working

1. **Defects move left** — More bugs caught in CI, fewer in QA/staging, almost none in production
2. **Tests arrive with code** — PRs routinely include test files; QA isn't waiting for handoffs
3. **AI accelerates, not replaces** — Cursor/Claude generates scaffolding; humans verify correctness against requirements
4. **DEF-XXX list shrinks** — `Defect List for Developers.md` has more resolved entries than open ones, and new entries slow down
5. **QA becomes a design partner** — QA contributes test scenarios *during* sprint planning, not after development
6. **Manual regression disappears** — Playwright covers critical paths through automated E2E scenario testing (happy paths and edge cases); manual testing is focused and creative, not repetitive
7. **The 3:1 ratio is natural** — Teams stop thinking about the ratio because negative/edge/functional tests are habitual
8. **CI is trusted** — Developers trust CI results and investigate failures immediately instead of ignoring them
9. **Post-mortems are rare** — Escaped defects become unusual enough that post-mortems are events, not routine
10. **Cycle time decreases** — Stories move from "In Development" to "Done" faster because QA round-trips are fewer

### Red Flags — Shift-Left Is Stalling

1. **Tests still arrive after code** — QA test specs are not pushed until development is complete
2. **Developers don't run QA tests locally** — CI catches everything, but developers don't catch anything pre-push
3. **AI-generated tests are committed without review** — Tests pass but don't actually validate requirements
4. **DEF-XXX list grows without resolution** — Defects accumulate but developers don't prioritize fixes
5. **3:1 ratio is gamed** — Tests are written to hit the ratio but don't provide meaningful coverage
6. **Edge case checklists are skipped** — Three Amigos sessions don't happen or don't produce checklists
7. **QA is still the bottleneck** — Stories pile up in "Ready for QA" because all testing waits for handoff
8. **Blame culture persists** — Failed CI builds are treated as personal failures rather than system wins
9. **Manual regression still dominates** — Playwright adoption is slow; E2E scenario coverage is minimal; the same manual test scripts run every sprint
10. **Metrics aren't collected** — This scorecard exists but is never filled in

---

## 8. Data Collection Guide

### Where to Find Each Metric

| Metric | Primary Source | Secondary Source | Collection Method |
|---|---|---|---|
| Gate 0 Pass Rate | JIRA "PO Approved" labels | Sprint planning notes | % of stories with PO sign-off before sprint commitment |
| Requirements Change Rate | JIRA AC edit history | Sprint retrospective notes | Count AC changes after sprint commitment per sprint |
| Design Traceability Score | Confluence design docs | JIRA attachments | % of complex stories with design-to-requirement traceability table |
| PRs with test files | GitHub PR file list | Git log | Manual review or script: `git log --name-only` filtered for test paths |
| Defect found-in phase | JIRA defect labels | `Defect List for Developers.md` | JIRA filter: `label = "found_in_dev"` etc. |
| 3:1 ratio compliance | Test file ratio tables | CI output | Review compliance tables printed in test files |
| Defect escape rate | JIRA | Production incident log | Count prod defects / total defects per sprint |
| Story-to-test time | JIRA transitions + Git commits | Agent transcripts | Compare JIRA "In Development" date with first test commit date |
| AI test acceptance | Agent transcripts + Git diffs | PR review comments | Count AI-generated files committed vs. discarded |
| Playwright coverage | `QA Tests/Playwright Tests/` file count | Feature inventory | Compare spec files to feature list |
| Open DEF-XXX count | `Defect List for Developers.md` | JIRA defect board | Count rows with Status = Open |
| CI first-pass rate | GitHub Actions run history | — | Success runs / total runs per PR |
| Dev test execution | PR descriptions, JIRA tickets | CI logs | Check for "QA tests run locally" confirmation |

### Automating Data Collection (Future)

Consider building these automations as the process matures:

- **GitHub Action** that counts test files in each PR and reports to a dashboard
- **JIRA webhook** that tags defects with `found_in_phase` based on workflow state
- **Script** that parses `Defect List for Developers.md` and counts open/resolved entries
- **CI job** that runs `dotnet test --filter "Defect~DEF" --list-tests` and reports the count
- **Git hook** that checks for test file presence before allowing a PR

---

## 9. Maturity Model

Use this model to assess where the team is and what to aim for next.

### Level 1: Awareness (Starting Point)

| Characteristic | Status |
|---|---|
| Team has read the Shift-Left Manifesto | ☐ |
| Team understands the cost-of-defect curve | ☐ |
| All roles (PM/BA, SD, Dev, QA) in shift-left are defined | ☐ |
| PM/BA and SD understand their Gate 0 responsibilities | ☐ |
| Tooling is available (Cursor, Playwright, CI) | ☐ |
| Training plan exists | ☐ |

### Level 2: Adoption (First 1-3 Sprints)

| Characteristic | Status |
|---|---|
| PM/BA validates ACs with PO before sprint commitment (Gate 0 started) | ☐ |
| SD participates in design walkthroughs for complex stories | ☐ |
| Three Amigos sessions happening for most stories (PM/BA + SD + Dev + QA) | ☐ |
| QA is pushing test specs before dev is done (at least some stories) | ☐ |
| Developers are running smoke tests locally before PRs | ☐ |
| CI pipeline includes test gates (smoke, business, integration) | ☐ |
| Edge case checklists are being created (at least for complex stories) | ☐ |
| AI (Cursor/Claude) is being used for test authoring | ☐ |
| Metrics collection has started (this scorecard is being filled in) | ☐ |

### Level 3: Consistency (Sprints 4-8)

| Characteristic | Status |
|---|---|
| Gate 0 Pass Rate >90% (PO-confirmed ACs before sprint commitment) | ☐ |
| Design traceability tables exist for >80% of complex stories | ☐ |
| Requirements change rate is decreasing sprint-over-sprint | ☐ |
| >80% of stories have QA test specs pushed by Day 5 | ☐ |
| >80% of PRs include test files | ☐ |
| >70% of defects found in Dev/CI (not QA/Prod) | ☐ |
| 3:1 ratio compliance at 100% | ☐ |
| Developers routinely run QA tests locally (>90% self-reported) | ☐ |
| Playwright E2E covers all critical user journeys | ☐ |
| Katalon migration at >80% | ☐ |
| Sprint demos include test metrics | ☐ |
| Retrospectives discuss shift-left progress | ☐ |

### Level 4: Optimization (Sprints 9+)

| Characteristic | Status |
|---|---|
| Gate 0 Pass Rate consistently >95% | ☐ |
| Requirement-originated defects are rare (<5% of total) | ☐ |
| Defect escape rate (production) consistently <5% | ☐ |
| CI first-pass success rate >90% | ☐ |
| Katalon fully decommissioned | ☐ |
| Mean story cycle time reduced by >30% vs. baseline | ☐ |
| Manual regression replaced by automated E2E scenario testing | ☐ |
| Post-mortems are rare events, not routine | ☐ |
| QA contributes to design/architecture discussions | ☐ |
| Test metrics are reviewed by leadership monthly | ☐ |
| New team members onboard to shift-left practices within 30 days | ☐ |

### Level 5: Culture (Ongoing)

| Characteristic | Status |
|---|---|
| Quality is genuinely everyone's responsibility — not a slogan | ☐ |
| PM/BA requirements quality is consistently high (Gate 0 is routine) | ☐ |
| SD design validation is embedded in the workflow | ☐ |
| Developers proactively write tests without being reminded | ☐ |
| QA focuses on E2E scenario testing (happy paths + edge cases) and quality strategy | ☐ |
| The team self-corrects when shift-left practices slip | ☐ |
| Shift-left metrics are stable and healthy without active management | ☐ |
| New projects adopt shift-left from Day 1 | ☐ |

---

## 10. Retrospective Questions

Use these questions at the end of each sprint to assess shift-left progress.

### For the Team

1. Were QA test specs ready early enough for devs to run before their PRs?
2. Did developers actually run QA tests locally? How do we know?
3. Were the edge case checklists useful? Did they catch anything?
4. How many defects were caught by CI vs. found in QA? Is the ratio improving?
5. Did AI-generated tests provide real value, or were they noise?

### For QA

1. Did you start writing tests before or after development was complete?
2. Were the requirements clear enough to write spec-based tests?
3. How much time did you spend on exploratory testing vs. regression vs. test authoring?
4. Are the defect lists being maintained? Are developers fixing DEF-XXX issues?
5. Is the Playwright suite growing? Is the Katalon migration on track?

### For Developers

1. Did running QA tests locally save you from a QA round-trip this sprint?
2. Were any QA test expectations wrong? How was the disagreement resolved?
3. Did you write unit tests for all new code? Were they caught by CI?
4. Do you trust the CI test results? Do you investigate failures immediately?
5. Is the shift-left process adding value or just adding overhead?

### For PM/BAs

1. Were all acceptance criteria confirmed by the PO before sprint commitment (Gate 0)?
2. How many ACs were changed after the sprint started? What caused the changes?
3. Were the business rules and edge cases documented clearly enough for QA to write tests?
4. Did you participate in Three Amigos sessions? Did your input prevent any requirement gaps?
5. Are there stories where requirement ambiguity caused defects? How can we prevent that?

### For Solution Designers

1. Did you provide design-to-requirements traceability for all complex stories?
2. Were your design walkthroughs held before development started (Day 1-2)?
3. Did the testability review with QA surface any design issues early?
4. Were there any design changes mid-sprint? If so, was the traceability re-validated?
5. Did you identify NFR risks (performance, security, scalability) that needed attention?

### For Leadership

1. Is the defect escape rate improving? Are fewer bugs reaching staging/production?
2. Is the cycle time (dev to done) decreasing?
3. Are QA and Dev collaborating more effectively than before?
4. Are we investing enough in training and tooling?
5. Is the team's morale improving with fewer late-cycle firefighting?

---

## Appendix: Related Documents

| Document | Location | Relationship |
|---|---|---|
| Shift-Left Testing Manifesto | `QA Tests/Documentation/SHIFT_LEFT_TESTING_MANIFESTO.md` | Defines the process this scorecard measures |
| Action Items (Dev + QA) | `QA Tests/Documentation/ACTION_ITEMS.md` | Living to-do list for developers and QA |
| QA Tester Playbook | `QA Tests/Documentation/QA_TESTER_PLAYBOOK.md` | Day-to-day QA practices |
| Onboarding Guide | `QA Tests/Documentation/ONBOARDING_GUIDE.md` | New hire integration into shift-left |
| Testing Structure | `QA Tests/Documentation/TESTING_STRUCTURE.md` | Repository organization |
| Defect List for Developers | `QA Tests/Defect List for Developers.md` | DEF-XXX defects (tracked in dashboard) |
| Defect List for QA | `QA Tests/Defect List for QA.md` | QA-XXX issues (tracked in dashboard) |

---

## Version History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | 2026-03-06 | QA Lead | Initial scorecard with 4 measurement dimensions, sprint dashboard, maturity model, and retrospective questions |
| 1.1 | 2026-03-09 | QA Lead | Added baseline snapshot (10,040 C# methods, 1,629 Playwright tests, 22 POMs, 13 helpers, 6 fixtures). Updated Playwright toolchain metrics with current counts. Added helper/fixture maturity metric. |
| 1.2 | 2026-03-09 | QA Lead | Aligned with Manifesto v1.4: Added Gate 0 / Pre-Development Validation metrics (Section 2.2) — Gate 0 Pass Rate, Requirements Change Rate, Design Traceability Score. Added PM/BA Behaviors (Section 4.3) and Solution Designer Behaviors (Section 4.4) to Process Indicators. Expanded Top 10 to Top 12 dashboard with Gate 0 metrics. Updated maturity model all levels to include pre-development validation milestones. Added PM/BA and Solution Designer retrospective questions. Added Gate 0 metrics to Data Collection Guide. Updated green flags/red flags to reference E2E scenario testing. Updated audience to include Solution Designers and PM/BAs. |
