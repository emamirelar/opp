# Shift-Left Testing Manifesto — New Way of Working

**Version:** 1.4  
**Date:** March 9, 2026  
**Status:** Draft — for team review and adoption  
**Audience:** Solution Designers, PM/BAs, QA Testers, Developers, Scrum Masters, Product Owners, Engineering Leadership  
**Scope:** UNOPS Opportunity+ and all projects under the QA team

---

## Table of Contents

1. [Vision and Principles](#1-vision-and-principles)
2. [The Testing Pyramid and Ownership Model](#2-the-testing-pyramid-and-ownership-model)
3. [Test Type Definitions and Responsibilities](#3-test-type-definitions-and-responsibilities)
4. [The Pre-Development Validation Contracts](#4-the-pre-development-validation-contracts)
5. [The Developer Testing Contract](#5-the-developer-testing-contract)
6. [The QA Testing Contract](#6-the-qa-testing-contract)
7. [Handshake Points — Team Collaboration Ceremonies](#7-handshake-points--team-collaboration-ceremonies)
8. [Quality Gates — The Four Checkpoints](#8-quality-gates--the-four-checkpoints)
9. [Defect Management Workflow](#9-defect-management-workflow)
10. [Katalon to Playwright Transition Plan](#10-katalon-to-playwright-transition-plan)
11. [Training Roadmap](#11-training-roadmap)
12. [Metrics and Success Criteria](#12-metrics-and-success-criteria)
13. [Tooling Reference](#13-tooling-reference)

**Appendices:**
- [Appendix A: Quick Reference — "Who Does What?"](#appendix-a-quick-reference--who-does-what)
- [Appendix B: Jira Workflow Integration](#appendix-b-jira-workflow-integration)
- [Appendix C: FAQ](#appendix-c-faq)
- [Appendix D: Related Documents](#appendix-d-related-documents)
- [Appendix E: Onboarding New Team Members](#appendix-e-onboarding-new-team-members)

---

## 1. Vision and Principles

### Why Shift Left

Historically, our developers write code and hand it to QA for all testing. Defects discovered late in the cycle are expensive: they require context-switching, re-work, and delay releases. Shift-left means moving testing activities earlier in the development lifecycle so that bugs are caught when they are cheapest to fix — while the code is still on the developer's machine.

### The Cost of Finding Bugs Late

Industry data consistently shows that defect cost increases exponentially the later it is found:

| Phase Found         | Relative Cost | Example                                      |
|---------------------|---------------|----------------------------------------------|
| Requirements/Design | ~$80          | QA catches ambiguity in Three Amigos session |
| Development (local) | ~$240         | Developer's unit test catches a logic error   |
| QA/Testing          | ~$960         | QA finds bug after handoff, dev context-switches back |
| Staging/UAT         | ~$3,840       | Bug found during acceptance, delays release   |
| Production          | ~$7,600+      | Customer-facing bug, emergency hotfix, reputation damage |

A single bug that costs 5 minutes to fix on a developer's machine can cost hours or days if it reaches QA, and days or weeks if it reaches production. Shift-left is not about adding more work — it is about doing the right work at the right time to avoid the expensive rework later.

This is not about making developers into testers, or making testers into developers. It is about giving both teams the right tools and the right responsibilities so that quality is built in from the start, not bolted on at the end.

### What Changes

| Before (Old Way)                                | After (New Way)                                            |
|-------------------------------------------------|------------------------------------------------------------|
| Requirements assumed complete, ambiguities found during QA | PM/BA validates requirements with PO before sprint commitment |
| Design reviewed only in code review (too late)   | Solution Designer validates design against requirements before dev starts |
| Developers write code, throw it over the wall   | Developers run tests before creating a Pull Request        |
| QA does all testing after development is done    | QA authors test specs early; devs run them pre-handoff     |
| Defects found late in QA/Staging                 | Defects caught on developer machines and in CI             |
| Katalon for UI and API automation                | Playwright for E2E; xUnit for backend; Cursor + Claude AI  |
| Manual testing is the primary approach           | Manual exploratory testing complements automated suites    |
| QA and Dev operate in silos                      | All roles collaborate through defined handshake points     |

### What Stays the Same

- QA remains the guardian of end-user quality and the voice of the customer
- QA owns exploratory testing, E2E regression, and acceptance testing
- Defect management follows the established DEF-XXX / QA-XXX process
- The 3:1 test ratio standard applies to all test authoring
- Jira remains the primary project and defect tracking tool

### Core Principles

1. **Quality is everyone's responsibility.** Solution Designers own the quality of the design. PM/BAs own the quality of the requirements. Developers own the quality of their code. QA owns the quality of the product experience. None can succeed alone.
2. **Start before code.** The cheapest defect is one prevented by a clear requirement and a validated design. PM/BAs and Solution Designers are the first line of quality defence.
3. **Test early, fail fast.** A bug found on a developer's machine costs minutes to fix. The same bug found in Staging costs days. A requirements gap found during refinement costs even less.
4. **Automate the repeatable, explore the unknown.** Regression and smoke tests should be automated. Exploratory testing should be manual, creative, and session-based.
5. **AI is a force multiplier, not a replacement.** Cursor and Claude help both teams write tests faster. The human decides what to test and whether the result is correct.
6. **Measure and improve.** Track metrics, discuss them in retrospectives, and adjust the process.

---

## 2. The Testing Pyramid and Ownership Model

### The Pyramid

```
                            /\
                           /  \            Manual, QA + PO
                          / L5  \          ── Exploratory Testing
                         / (QA)  \         ── User Acceptance Testing (UAT)
                        /----------\
                       /    L4      \      QA owns 100%, CI + QA executes
                      /  (QA owns)   \     ── E2E Browser Tests (Playwright)
                     /----------------\    ── Playwright Test Automation (specs, POM, mocks)
                    /       L3         \   QA authors (with AI), Dev + CI executes
                   /  (QA authors,      \  ── API / Controller Integration Tests
                  /    Dev runs)         \ ── Business Logic Tests
                 /                       \── Functional Tests
                /                         \─ Validation Tests
               /                           \  Security Tests
              /-----------------------------\─ Concurrency Tests
             /            L2                 \  Scheduled / on-demand
            /   (QA authors with AI,          \ ── Performance Tests
           /     Dev + QA execute)             \── Load Tests
          /-------------------------------------\
         /              L1                       \  Dev authors, Dev + CI executes
        /       (Dev owns fully)                  \ ── Unit Tests
       /                                           \── Component Tests
      /---------------------------------------------\─ Smoke Tests (Dev + QA co-own)

  ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄
  Cross-cutting: Regression Testing spans L1–L4
  (automated xUnit suites + Playwright E2E suite
   + manual regression for high-risk areas)
  ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄

  ══════════════════════════════════════════════════════
  L0 — PRE-DEVELOPMENT VALIDATION (below the pyramid)
  ══════════════════════════════════════════════════════
  ┌───────────────────────────────────────────────────┐
  │  Requirements Validation (PM/BA + PO)             │
  │  ── Completeness checks, testability gate,        │
  │     business rule documentation, PO sign-off      │
  │                                                   │
  │  Design Validation (Solution Designer + QA)       │
  │  ── Design-to-requirements traceability,          │
  │     testability review, NFR validation,            │
  │     integration point specification               │
  └───────────────────────────────────────────────────┘
  These activities prevent defects from entering the
  pyramid at all — the cheapest quality investment.
```

**All Test Types by Pyramid Level:**

| Level | Test Types | Primary Owner | Devs Involved? |
|-------|-----------|---------------|----------------|
| **L0 — Pre-Development** | Requirements Validation, Design Validation | PM/BA + Solution Designer | No (provides inputs for Dev and QA) |
| **L1 — Foundation** | Unit Tests, Component Tests, Smoke Tests | Dev (Smoke co-owned with QA) | Yes — authors and executes |
| **L2 — Non-Functional** | Performance Tests, Load Tests | QA (with AI), reviewed by Dev | On demand only |
| **L3 — Integration** | API/Controller Integration Tests, Business Logic Tests, Functional Tests, Validation Tests, Security Tests, Concurrency Tests | QA (with AI), Dev runs pre-PR | Yes — must run before PR |
| **L4 — E2E & Automation** | E2E Browser Tests (Playwright), Playwright Test Automation | QA owns 100% | No — devs do not write or maintain |
| **L5 — Manual** | Exploratory Testing, User Acceptance Testing (UAT) | QA + PO | No (PO participates in UAT) |
| **Cross-cutting** | Regression Testing | QA (automated suites run in CI) | Indirectly via CI |

### Test Types vs. Test Categories (3:1 Ratio)

The pyramid above shows **test types** — they define *what* is being tested and *where* in the stack. Each test type is implemented as separate test files organized by feature area.

Within each test type, individual tests are classified into **test categories** that enforce the 3:1 ratio standard. These categories are not separate test types — they are a quality discipline applied across all test types:

| Category | What It Means | File Suffix | Repo Count |
|----------|--------------|-------------|------------|
| **Positive** | Happy path — feature works as designed | `*PositiveTests.cs` | ~23 files |
| **Negative** | Invalid input, unauthorized access, expected failures | `*NegativeTests.cs` | ~42 files |
| **Edge/Boundary** | Boundary values, soft-delete interactions, type mismatches, fallback paths | `*EdgeCaseTests.cs`, `*BoundaryTests.cs` | ~46 files |

**The rule:** For every 1 Positive test, there must be at least 3 Negative, 3 Edge/Boundary, 3 Functional, and 3 Integration tests. This ratio applies within each feature area, not per test type.

**Example:** For the Partner feature area, the test files are organized as:
```
QA Tests/Integration Tests/PartnerTree/
├── PartnerTreePositiveTests.cs      ← Positive (happy path)
├── PartnerTreeNegativeTests.cs      ← Negative (3× positive)
├── PartnerTreeEdgeCaseTests.cs      ← Edge/Boundary (3× positive)
├── PartnerTreeFunctionalTests.cs    ← Functional (3× positive)
├── PartnerTreeIntegrationTests.cs   ← Integration (3× positive)
└── PartnerTreeValidationTests.cs    ← Validation (additional coverage)
```

See `.cursor/rules/test-ratio-enforcement.mdc` for the full ratio standard.

### Ownership Matrix

| Level | Test Type                          | Creates        | Updates        | Executes (Local)  | Executes (CI)     | Reviews         |
|-------|------------------------------------|----------------|----------------|-------------------|-------------------|-----------------|
| L0    | Requirements Validation            | PM/BA          | PM/BA          | PM/BA + PO        | N/A (human)       | PO              |
| L0    | Design Validation                  | Solution Designer | Solution Designer | SD + QA + Dev  | N/A (human)       | Dev Lead + QA   |
| L1    | Unit Tests                         | Dev            | Dev            | Dev               | Automatic on PR   | Dev (PR review) |
| L1    | Component Tests                    | Dev            | Dev            | Dev               | Automatic on PR   | Dev (PR review) |
| L1    | Smoke Tests                        | Dev + QA       | Dev + QA       | Dev               | Automatic on PR   | QA Lead         |
| L2    | Performance Tests                  | QA (with AI)   | QA + Dev       | On demand         | Scheduled         | QA + Dev        |
| L2    | Load Tests                         | QA (with AI)   | QA + Dev       | On demand         | Scheduled         | QA + Dev        |
| L3    | API/Integration Tests              | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L3    | Business Logic Tests               | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L3    | Functional Tests                   | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L3    | Validation Tests                   | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L3    | Security Tests                     | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L3    | Concurrency Tests                  | QA (with AI)   | QA + Dev       | Dev (pre-PR)      | Automatic on PR   | QA              |
| L4    | E2E Browser Tests (Playwright)     | QA             | QA             | QA                | Scheduled + PR    | QA Lead         |
| L4    | Playwright Test Automation (POM, mocks, fixtures) | QA | QA          | QA                | N/A (infrastructure) | QA Lead      |
| L5    | Exploratory Testing                | QA             | QA             | QA                | N/A (manual)      | QA Lead         |
| L5    | Acceptance Testing (UAT)           | QA + PO        | QA             | QA + PO           | N/A               | PO              |
| —     | Regression Testing (cross-cutting) | QA             | QA             | QA                | Automatic (suite) | QA Lead         |

### The Integration Test Distinction

"Integration tests" is a broad term. In this codebase, it covers two distinct categories with different ownership:

**Developer-owned integration tests** are unit-level tests that verify a single manager or service works correctly with its mocked dependencies. These live in the production test projects, and developers own the full lifecycle: authoring, updating, running, and reviewing.

**QA-authored API/integration tests** are the ~5,500+ tests in `QA Tests/Integration Tests/` that test controllers via HTTP, cross-service flows, and business rules using fixtures, stubs, and in-memory databases. QA authors these using Cursor and Claude AI, defining the scenarios and edge cases. Developers are required to run them locally before creating a Pull Request. QA runs the full suite again as part of regression.

The shift-left mechanism is this: QA authors the test, pushes it to the repo, and the developer must make their code pass those tests before handing off. The developer is not writing these tests — they are running them as a quality gate.

---

## 3. Test Type Definitions and Responsibilities

### 3.1 Unit Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify individual methods, functions, or classes in isolation        |
| **Created by**  | Developers                                                          |
| **Maintained by** | Developers                                                        |
| **Executed by** | Developers locally + CI on every PR                                 |
| **Tools**       | xUnit, FluentAssertions, Moq                                       |
| **Location**    | Production test projects (adjacent to source code)                  |
| **Example**     | `PartnerManager.CreatePartner()` returns correct model with audit fields populated |

QA's role: Provide edge case scenarios during Three Amigos sessions. QA does not write unit tests.

Dev's role: Write unit tests for all new and changed code. Unit tests must pass before creating a PR.

### 3.2 Smoke Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Fast gate to verify the system is fundamentally operational          |
| **Created by**  | Dev + QA collaboratively                                            |
| **Maintained by** | Dev + QA                                                          |
| **Executed by** | CI on every PR; devs can run locally                                |
| **Tools**       | xUnit (Category=Smoke)                                              |
| **Location**    | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Smoke/`                |
| **Example**     | ~14 tests verifying core services resolve and basic operations work |

### 3.3 API / Controller Integration Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Test API endpoints, request/response contracts, authorization, and HTTP behavior |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA primarily; Dev when production code changes break tests         |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR) + QA (regression)     |
| **Tools**       | xUnit, FluentAssertions, WebApplicationFactory, in-memory fixtures  |
| **Location**    | `QA Tests/Integration Tests/Controllers/`                           |
| **Example**     | `ContactAnalyticsControllerTests.cs` — verifies GET/POST endpoints return correct status codes, data shapes, and enforce authorization |

**How the handoff works:**
1. During sprint planning, QA identifies the controllers and endpoints a story touches.
2. QA authors integration test specs and pushes them to the repo by Day 3-5 of the sprint.
3. Dev pulls the tests and runs them locally as part of their Definition of Done.
4. If tests fail, Dev fixes their code (not the test). If the test expectation is wrong, Dev raises it with QA.
5. CI enforces the tests on every Pull Request.

### 3.4 Business Logic Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify manager-level business rules, validation, state transitions, and computed values |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA primarily; Dev when production changes break tests              |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR) + QA (regression)     |
| **Tools**       | xUnit, FluentAssertions, Moq, AutoMapper                           |
| **Location**    | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/`             |
| **Example**     | `PartnerManagerTests.cs` — verifies soft-delete sets `IsDeleted` flag, audit fields populate correctly, validation rejects invalid input |

### 3.5 Functional Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify business rules, validation logic, state transitions, computed values, and data transformations at the integration level |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA primarily; Dev when production changes break tests              |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR) + QA (regression)     |
| **Tools**       | xUnit, FluentAssertions, in-memory fixtures, stubs                  |
| **Location**    | `QA Tests/Integration Tests/*/` (files ending in `*FunctionalTests.cs`) and `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Functional/` |
| **Example**     | `PartnerFunctionalTests.cs` — verifies audit fields populate on create, permissions are enforced per workflow stage, data transformations produce correct output |

**How Functional Tests differ from Business Logic Tests:** Business Logic Tests (3.4) focus on individual manager methods in isolation with mocks. Functional Tests verify that the business rules hold when multiple components interact — including validation, state transitions, and computed values through the integration layer. Both are mandatory categories in the 3:1 ratio rule.

### 3.6 Validation Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify input validation rules, model constraints, data format enforcement, and field-level validation logic |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA primarily; Dev when production changes break tests              |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR) + QA (regression)     |
| **Tools**       | xUnit, FluentAssertions, in-memory fixtures, stubs                  |
| **Location**    | `QA Tests/Integration Tests/*/` (files ending in `*ValidationTests.cs`) and `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/` (e.g., `MappingProfileValidationTests.cs`, `JiraInputValidationTests.cs`) |
| **Example**     | `OrgHierarchyValidationTests.cs` — verifies required fields are enforced, max-length constraints reject oversized input, mapping profiles produce valid output for all entity types |

**How Validation Tests differ from Functional Tests:** Validation Tests focus narrowly on input/output constraints — required fields, max lengths, format rules, and mapping correctness. Functional Tests (3.5) verify broader business rules, state transitions, and computed values. Both are mandatory categories in the 3:1 ratio rule.

### 3.7 Security Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify authorization, permission enforcement, and access control     |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA + Dev                                                           |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR)                       |
| **Tools**       | xUnit, FluentAssertions, mock authorization handlers                |
| **Location**    | `QA Tests/Integration Tests/` (files ending in `*SecurityTests.cs`) |
| **Example**     | `DashboardSecurityTests.cs` — verifies users without `CanViewDashboard` permission receive 403 Forbidden |

### 3.8 Concurrency Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify system behavior under concurrent access — race conditions, thread safety, parallel operations, and data consistency with simultaneous users |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA + Dev                                                           |
| **Executed by** | Dev (locally, pre-PR) + CI (automatic on PR)                       |
| **Tools**       | xUnit, FluentAssertions, Task.WhenAll, SemaphoreSlim, concurrent collections |
| **Location**    | `QA Tests/Integration Tests/` (files ending in `*ConcurrencyTests.cs`) and `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/` |
| **Example**     | `ContactConcurrencyTests.cs` — verifies that two users updating the same contact simultaneously do not corrupt data or lose changes silently |

### 3.9 E2E Browser Tests (Playwright)

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Test complete user workflows through the browser, end to end         |
| **Created by**  | QA                                                                  |
| **Maintained by** | QA                                                                |
| **Executed by** | QA (locally during development) + CI (scheduled and on PR)          |
| **Tools**       | Playwright, TypeScript, Page Object Model                           |
| **Location**    | `QA Tests/Playwright Tests/`                                        |
| **Example**     | `partner-list.spec.ts` — navigates to partner list, filters by status, verifies table displays correct data, creates new partner via dialog |

This is 100% QA-owned. Developers do not write or maintain Playwright tests. This aligns with the QA team's strength in understanding end-user workflows and testing from the front-end perspective.

**See also:** [Playwright Quickstart for Testers](../Playwright%20Tests/QUICKSTART_FOR_TESTERS.md) for setup and writing guides.

### 3.10 Performance Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify response times, throughput, and SLA compliance under normal load |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA + Dev                                                           |
| **Executed by** | CI (scheduled nightly) + QA (on demand before releases)             |
| **Tools**       | xUnit, Stopwatch, BenchmarkDotNet                                   |
| **Location**    | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Performance/`          |
| **Example**     | `OpportunityManagerPerformanceTests.cs` — verifies `GetOpportunityDetailsForAIAsync` completes within 5 seconds |

### 3.11 Load Tests

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify system stability under concurrent users and sustained load    |
| **Created by**  | QA (using Cursor + Claude AI)                                       |
| **Maintained by** | QA + Dev                                                           |
| **Executed by** | CI (scheduled) + QA (on demand before releases)                     |
| **Tools**       | xUnit, Task.WhenAll, concurrent test patterns                       |
| **Location**    | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Performance/`          |
| **Example**     | `PartnerManagerLoadTests.cs` — simulates 50 concurrent users querying the partner list |

### 3.12 Exploratory Testing

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Discover defects that automated tests cannot catch through creative, unscripted human investigation |
| **Created by**  | QA (session charters)                                               |
| **Maintained by** | QA                                                                |
| **Executed by** | QA only (manual, on QA environment)                                 |
| **Tools**       | Browser, Jira for notes, screen recording                           |
| **Location**    | N/A (manual activity, results logged in Jira)                       |
| **Example**     | "Explore the partner creation workflow focusing on edge cases with special characters in the partner name, rapid form submissions, and browser back-button behavior" |

Exploratory testing is a uniquely human activity. It cannot be automated and should not be reduced. After shift-left, QA will have more time for exploratory testing because developers will have caught the obvious bugs before handoff.

### 3.13 Regression Testing

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify that new changes have not broken existing functionality       |
| **Created by**  | QA                                                                  |
| **Maintained by** | QA                                                                |
| **Executed by** | CI (automated suite on every PR and nightly) + QA (manual regression for high-risk areas before release) |
| **Tools**       | xUnit (full suite), Playwright (E2E suite), manual checklists       |
| **Location**    | Across all test suites                                              |
| **Example**     | Before a release: CI runs full ~11,000 test suite. QA performs manual regression on critical user journeys. |

### 3.14 User Acceptance Testing (UAT)

| Attribute       | Detail                                                              |
|-----------------|---------------------------------------------------------------------|
| **Purpose**     | Verify the delivered feature meets business requirements from the stakeholder's perspective |
| **Created by**  | QA (drafts acceptance criteria with PO during refinement)           |
| **Maintained by** | QA                                                                |
| **Executed by** | QA facilitates, PO/stakeholders execute on Staging environment       |
| **Tools**       | Manual walkthroughs, Jira acceptance criteria                       |
| **Location**    | Jira tickets (acceptance criteria section)                          |
| **Example**     | PO walks through the new partner onboarding flow on Staging and confirms it matches the requirements |

---

## 4. The Pre-Development Validation Contracts

This section defines what Solution Designers and PM/BAs commit to under the shift-left model. These are the earliest quality activities — they happen before a single line of code is written. Defects prevented here cost a fraction of defects found later.

### 4.1 The Solution Designer Validation Contract

The Solution Designer is responsible for verifying that the proposed technical solution actually addresses the stated requirements. A design that looks elegant on a whiteboard but cannot be tested, does not meet NFRs, or leaves integration points undefined is a defect waiting to happen.

#### Before Development Begins, the Solution Designer MUST

1. **Validate design-to-requirements traceability.**
   - For every acceptance criterion in the Jira story, confirm the design explicitly addresses it.
   - If a requirement says "users must be notified when an opportunity is approved," the design must specify where that notification originates, who receives it, and through what channel.
   - Produce a traceability table: Requirement → Design Component → How It Will Be Tested.

2. **Conduct a testability review with QA.**
   - Walk through the design with QA before development starts.
   - QA asks: "How do we test this? What are the boundaries? What happens when this external service is down?"
   - If any part of the design cannot be tested (black-box integration, no observable outputs, no error surface), it must be redesigned or a monitoring/observability plan must be agreed.

3. **Validate non-functional requirements.**
   - If the requirement says "page loads in under 3 seconds," confirm the design can achieve this. If the design calls for 15 sequential API calls, that is a design defect.
   - Review performance, security, scalability, and availability requirements against the proposed architecture.
   - Document any NFR risks and proposed mitigations.

4. **Specify integration points with enough detail for test authoring.**
   - Define all API contracts (request/response schemas, status codes, error formats).
   - Document data flows between components.
   - Specify external system dependencies and their failure modes.
   - QA uses these specifications to begin writing integration test specs before development starts.

5. **Document architecture decisions.**
   - Record key design decisions (why this database, why this service boundary, why this caching strategy) so that testers understand the rationale and can test against it.
   - Use Architecture Decision Records (ADRs) or inline documentation in the design document.

6. **Present a design walkthrough.**
   - Conduct a design walkthrough with Dev + QA before development begins.
   - The walkthrough is not a rubber-stamp — it is a validation session where the design is challenged.
   - Attendees should ask: "What could go wrong at the boundaries? How does this handle concurrent access? What happens when data is soft-deleted?"

#### Design Validation Outputs

| Output | When | Delivered To | Purpose |
|--------|------|-------------|---------|
| Design-to-requirements traceability table | Before sprint commitment | PO, Dev, QA | Confirms every requirement is addressed by the design |
| Testability assessment | During design walkthrough | QA | Confirms QA can test every component of the design |
| NFR risk register | Before sprint commitment | Dev Lead, QA | Documents known performance/security/scalability risks |
| Integration point specifications | By Day 2 of sprint | QA, Dev | Enables QA to start writing integration tests early |
| Architecture Decision Records | Before sprint commitment | Dev, QA, future maintainers | Documents the "why" behind design choices |

#### The Solution Designer SHOULD NOT

- Hand off a design without confirming it addresses every acceptance criterion. An untraceable design is an incomplete design.
- Assume testability. If QA cannot explain how they would test a component, the design needs more thought.
- Defer NFR validation to "later." Performance, security, and scalability must be designed in, not bolted on.
- Design in isolation. The design walkthrough with Dev and QA is mandatory, not optional.

### 4.2 The PM/BA Requirements Validation Contract

The PM/BA is the bridge between the Product Owner's business intent and the team's ability to build and test. Incomplete, ambiguous, or untestable requirements are the root cause of the most expensive defects — the ones where the code works exactly as built but not as intended.

#### Before a Story Enters the Sprint, the PM/BA MUST

1. **Validate requirements completeness with the PO.**
   - Every story entering the sprint must have defined acceptance criteria. A story with "As a user I want to manage partners" and no acceptance criteria is not ready for sprint commitment.
   - Walk through each acceptance criterion with the PO and confirm it represents the business need.
   - Document the PO's confirmation in Jira (a comment such as "PO confirmed ACs on [date]" or a "PO Approved" label).

2. **Apply the testability gate.**
   - Each acceptance criterion must be specific enough that QA can write a test for it.
   - "The system should be fast" is not testable. "The partner list loads in under 2 seconds for 1,000 records" is testable.
   - "The UI should be user-friendly" is not testable. "All form fields have labels, error messages display below the field, and the tab order follows the visual layout" is testable.
   - If an acceptance criterion cannot be tested, it must be rewritten before sprint commitment.

3. **Document all business rules explicitly.**
   - Business rules (who can approve, what triggers a notification, when is a field required, what formula calculates the budget) must be written down, not left as tribal knowledge.
   - These documented rules become the specification that QA tests against and that developers build against.
   - Undocumented business rules are the #1 source of "it works as coded but not as expected" defects.

4. **Perform cross-feature impact analysis.**
   - When a new requirement is introduced, assess which existing features might be affected.
   - Example: "Adding a new partner category field — does this affect the search filters? The export report? The oUP sync? The partner list columns?"
   - This analysis feeds directly into QA's regression scope and helps QA prioritize which existing test suites to re-run.

5. **Identify and surface missing scenarios during refinement.**
   - Actively look for gaps: "What happens if the user does X and then Y? What if this field is empty? What if the approver has been deactivated? What if the partner is soft-deleted?"
   - These questions surface requirements gaps before they become code defects.
   - Document the answers as additional acceptance criteria or notes on the story.

6. **Manage requirement changes.**
   - When requirements change mid-sprint (and they will), ensure the change is documented in Jira, communicated to Dev and QA, and acceptance criteria are updated.
   - An undocumented requirement change is the #1 cause of "Dev built the old spec, QA tested the new spec" conflicts.
   - All requirement changes after sprint commitment must be explicitly acknowledged by Dev and QA.

#### Requirements Validation Outputs

| Output | When | Delivered To | Purpose |
|--------|------|-------------|---------|
| PO-confirmed acceptance criteria | Before sprint commitment | Dev, QA, Solution Designer | Single source of truth for what to build and test |
| Testability-reviewed criteria | Before sprint commitment | QA | Confirms every AC can be verified with a test |
| Documented business rules | Before sprint commitment | Dev, QA | Explicit specification for implementation and testing |
| Cross-feature impact assessment | During refinement | QA, Dev | Identifies regression scope and affected areas |
| Requirements change log | Ongoing during sprint | Dev, QA | Tracks what changed, when, and who acknowledged it |

#### The PM/BA SHOULD NOT

- Allow stories into the sprint without PO-confirmed acceptance criteria. An unconfirmed story is a best guess, not a requirement.
- Assume developers and testers understand the business context. If a business rule is "obvious" to the PM/BA, it still needs to be written down.
- Treat QA as the last line of defence for requirements quality. If QA is discovering requirements gaps during testing, the PM/BA validation failed.
- Allow scope changes without updating acceptance criteria and notifying the team. Silent scope creep is the enemy of quality.

### How L0 Feeds the Rest of the Pyramid

```
PM/BA validates requirements ──┐
                                ├──> Three Amigos: shared understanding
Solution Designer validates ───┘          │
design against requirements               │
                                           ▼
                                 QA authors edge case checklists
                                 QA begins writing test specs (L3)
                                 Dev begins coding with clear spec
                                           │
                                           ▼
                              L1–L5 test execution (existing flow)
```

When L0 is done well, the downstream effects are significant:
- **QA writes better tests** because the specification is clear, not guessed.
- **Developers build the right thing** because requirements are unambiguous.
- **Fewer defects escape** because the most common root cause (unclear requirements) is addressed.
- **Three Amigos sessions are more productive** because participants arrive with validated requirements and a reviewed design, not raw ideas.

---

## 5. The Developer Testing Contract

This section defines what developers commit to under the shift-left model. This is not optional — it is part of the Definition of Done. (For the pre-development contracts that feed into developer work, see [Section 4](#4-the-pre-development-validation-contracts).)

### Before Creating a Pull Request, Developers MUST

1. **Write unit tests** for all new and changed code.
   - Every new public method must have at least one positive test and one negative test.
   - Changed code must have tests updated to reflect the new behavior.

2. **Run the relevant QA-authored test suite locally.**
   - If the story touches `PartnerManager`, run `dotnet test --filter "FullyQualifiedName~Partner"` against the QA test projects.
   - If the story touches a controller, run the corresponding controller integration tests.
   - The PR template should list which QA test suites were run.

3. **Verify no new test failures are introduced.**
   - All smoke tests must pass: `dotnet test --filter "Category=Smoke"`.
   - All tests that were passing before the change must still pass after.
   - New test failures must be investigated, not ignored.

4. **Review the QA-provided edge case checklist** (attached to the Jira ticket after Three Amigos).
   - Confirm each edge case is either covered by a test or explicitly handled in code.

5. **Do not modify QA test files without QA approval.**
   - If a QA test expectation appears wrong, raise it with QA — do not change the assertion.
   - See the "Never Weaken Tests" rule: a failing test may indicate a production bug, not a test bug.

### Developers SHOULD NOT

- Skip running tests to meet deadlines. A PR without test evidence should not be merged.
- Weaken test assertions to make them pass (see `never-weaken-tests.mdc` rule).
- Modify files inside `QA Tests/` without coordinating with QA (see `qa-write-boundaries.mdc` rule).
- Treat test failures as someone else's problem. If your code breaks a test, you own the fix.

### How Developers Run QA Tests Locally

```bash
# Run smoke tests (fast gate, ~30 seconds)
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "Category=Smoke"

# Run integration tests for a specific feature area
dotnet test "QA Tests/Integration Tests" --filter "FullyQualifiedName~Partner"
dotnet test "QA Tests/Integration Tests" --filter "FullyQualifiedName~Dashboard"

# Run all business logic tests for a manager
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "FullyQualifiedName~PartnerManager"

# Run the full integration suite (before major changes)
dotnet test "QA Tests/Integration Tests"
```

---

## 6. The QA Testing Contract

This section defines what QA testers commit to under the shift-left model.

### During the Sprint, QA Testers MUST

1. **Author test specs early — not after development is done.**
   - Integration and E2E test scenarios must be written by Day 3-5 of the sprint (while devs are coding).
   - Tests are pushed to the repo so devs can pull and run them.
   - This is the single most important shift-left activity for QA.

2. **Use Cursor + Claude AI to author tests.**
   - QA defines the test scenarios in natural language (what to test, expected behavior, edge cases).
   - Claude generates the test code based on the existing codebase patterns.
   - QA reviews, adjusts, and commits the generated tests.
   - QA does not need to write C# or TypeScript from scratch.

3. **Provide edge case checklists per story.**
   - After Three Amigos, QA attaches an edge case checklist to the Jira ticket.
   - The checklist covers: boundary values, null/empty inputs, unauthorized access, concurrent operations, soft-delete interactions, and error conditions.
   - Developers use this checklist to guide their unit test authoring.

4. **Perform exploratory testing after dev handoff.**
   - When a story moves to "Ready for QA," QA performs manual exploratory testing on the QA environment.
   - Exploratory testing is session-based: 30-60 minute time-boxed sessions with a charter.
   - Findings are logged as Jira defects with DEF-XXX references.

5. **Run E2E regression before every release.**
   - Full Playwright suite on QA or Staging environment.
   - Manual regression of critical user journeys.
   - Sign-off documented in Jira.

6. **Maintain defect lists.**
   - Production defects: `QA Tests/Defect List for Developers.md` (DEF-XXX prefix).
   - Test infrastructure issues: `QA Tests/Defect List for QA.md` (QA-XXX prefix).
   - See [Defect Management Workflow](#9-defect-management-workflow) for details.

### QA Testers SHOULD NOT

- Write unit tests. That is the developer's responsibility. QA influences unit tests through edge case checklists and Three Amigos, but does not write the code.
- Modify production source code. If production code is missing something a test needs, log a DEF-XXX defect and skip or mock (see `qa-write-boundaries.mdc` rule).
- Block a release without documented justification. All blocking defects must be logged in Jira with severity and business impact.
- Wait until development is done to start testing activities. Test authoring begins at sprint planning, not at handoff.

### QA's Shift-Left Role in Practice

```
BEFORE SHIFT-LEFT                           AFTER SHIFT-LEFT

                                             Pre-Sprint (Refinement):
                                               PM/BA: Validates ACs with PO — Gate 0
Requirements assumed complete,                 SD: Traces design to requirements
ambiguities surface during QA                  SD + QA: Testability review
                                               PM/BA: Documents business rules

Sprint Day 1-8:                              Sprint Day 1-2:
  QA: "Waiting for dev to finish"              SD: Design walkthrough with Dev + QA
                                               QA: Three Amigos, edge case checklists
                                               QA: Start writing test specs with AI

Sprint Day 9-10:                             Sprint Day 3-5:
  QA: "Everything landed, now I test"          QA: Push test specs to repo
  QA: Finds bugs, logs them                    Dev: Pulls tests, runs them, fixes bugs
  Dev: Context-switches back to fix            QA: Continues writing E2E tests

                                             Sprint Day 6-8:
                                               Dev: "Ready for QA" — tests already passing
                                               QA: Exploratory testing + E2E regression
                                               QA: Finds subtle bugs devs couldn't catch

                                             Sprint Day 9-10:
                                               Joint bug triage
                                               Sprint demo with test metrics
```

---

## 7. Handshake Points — Team Collaboration Ceremonies

The following touchpoints define when and how the team interacts within a 2-week sprint. These are not new meetings — they are defined moments within existing ceremonies where all roles explicitly collaborate.

### Requirements Validation (Pre-Sprint / Refinement)

**Who:** PM/BA + PO (Solution Designer joins for complex stories)  
**PM/BA's role:**
- Walk through each acceptance criterion with the PO and confirm it represents the business need.
- Apply the testability gate: can QA write a test for each AC? If not, rewrite it.
- Document all business rules explicitly — nothing left as tribal knowledge.
- Perform cross-feature impact analysis for new requirements.
- Record PO confirmation in Jira ("PO confirmed ACs on [date]").

**Solution Designer's role (for complex/new features):**
- Confirm the proposed design addresses every acceptance criterion.
- Produce a design-to-requirements traceability table.
- Identify NFR risks (performance, security, scalability).

**Output:** Stories with PO-confirmed, testable acceptance criteria and documented business rules. This is **Gate 0** — stories that don't pass this gate are not ready for sprint commitment.

### Sprint Planning (Day 1)

**Who:** Full team (PM/BA + Solution Designer + Dev + QA + PO + SM)  
**PM/BA's role:**
- Confirm that all stories entering the sprint have passed Gate 0 (PO-confirmed, testable ACs).
- Present the cross-feature impact assessment for new stories.
- Highlight any requirement changes since refinement.

**Solution Designer's role:**
- Present the design approach for complex stories.
- Confirm integration points are specified well enough for QA to begin test authoring.

**QA's role:**
- Review each story and identify which test types will be needed.
- Flag stories that are high-risk or require complex test setup.
- Estimate QA effort for test authoring and execution.
- Confirm acceptance criteria are specific, measurable, and testable (testability gate).

**Output:** Each story has a "Test Approach" note: which test types apply, rough scenario count, any data setup needed.

### Design Walkthrough (Day 1-2, for complex stories)

**Who:** Solution Designer + Dev + QA  
**Purpose:** Validate the technical design before development begins.  
**Format:** Solution Designer presents; Dev and QA challenge.

**Solution Designer presents:**
- Design-to-requirements traceability (every AC mapped to a design component)
- Integration point specifications (API contracts, data flows)
- NFR validation (how performance/security/scalability targets will be met)

**QA asks:**
- "How do we test this component?"
- "What happens at the boundaries?"
- "What happens when this external service is unavailable?"
- "How do we verify this data flow end-to-end?"

**Dev asks:**
- "Is this feasible within the sprint?"
- "Are there dependencies that could block us?"
- "Does this require a migration?"

**Output:** Validated design with confirmed testability. Integration point specifications that QA can use to start writing test specs immediately.

### Three Amigos Session (Day 1-2)

**Who:** PO + Dev + QA + PM/BA (per story or group of stories; Solution Designer joins for complex stories)  
**Purpose:** Ensure shared understanding before work begins.  
**Format:** QA leads with the question: *"What could go wrong?"*

**PM/BA's role:**
- Confirm the acceptance criteria are still accurate and PO-approved.
- Surface any business rules that may not be written down yet.
- Identify cross-feature impacts the team should be aware of.

**QA prepares:**
- Edge case scenarios (boundary values, nulls, invalid inputs)
- Permission/authorization scenarios
- Concurrent operation scenarios
- Soft-delete and state transition scenarios
- Cross-entity impact scenarios

**Output:** An edge case checklist attached to the Jira ticket. Example:

```
Edge Case Checklist — PNO-1234: Add Partner Category Field
- [ ] Category is null/empty on create
- [ ] Category exceeds max length (255 chars)
- [ ] Category contains special characters (< > & " ')
- [ ] Category is updated on a soft-deleted partner
- [ ] Two users update category simultaneously
- [ ] User without CanEditPartners permission attempts update
- [ ] Category dropdown shows only active (non-deleted) categories
```

### Test Spec Handoff (Day 3-5)

**Who:** QA -> Dev  
**How it works:**
1. QA pushes integration and/or E2E test specs to the repo on a feature branch or directly to the QA test branch.
2. QA posts a message in the team channel: *"Tests for PNO-1234 are ready in `QA Tests/Integration Tests/PartnerCategory/`. Please run before your PR."*
3. Dev pulls the tests, runs them locally, and ensures their code passes.
4. If a test expectation seems wrong, Dev discusses with QA before changing anything.

**This is the core shift-left handshake.** QA writes tests before dev is done. Dev runs those tests as a quality gate.

### Daily Standup (Daily)

**QA reports:**
- Which test specs were authored/pushed yesterday
- Blockers (waiting for clarification, test environment issues)
- Defects found during exploratory testing

**Dev reports:**
- Which QA tests were run and their results
- Any test failures being investigated
- Blockers related to test infrastructure

### PR Review Gate (Ongoing)

**Automated (CI):**
- Smoke tests run on every PR (blocking gate)
- Integration tests run on every PR (blocking gate)
- Playwright smoke specs run on every PR (non-blocking initially, blocking after Phase 2)

**Optional human review:**
- QA can be tagged as a reviewer on PRs that touch areas with known test complexity.
- QA reviews are not required on every PR — only when the change touches test-adjacent code or high-risk areas.

### QA Handoff (Day 6-8)

**Who:** Dev -> QA  
**Process:**
1. Dev moves the Jira ticket to "Ready for QA."
2. Dev includes in the ticket: which QA test suites were run locally, pass/fail results, any known issues.
3. Code is deployed to the QA environment via Jenkins.
4. QA performs exploratory testing on the QA environment.
5. QA runs the full E2E suite against the QA environment.
6. Defects are logged in Jira with DEF-XXX references and linked to the story.

**Definition of "Ready for QA":**
- Code is merged to the deployment branch.
- Smoke and integration tests pass in CI.
- Dev has listed QA test suites run locally (in the Jira ticket or PR description).
- No known critical/high defects in the feature area.

### Bug Triage (Day 8-9)

**Who:** QA Lead + Dev Lead (or full team for critical items)  
**Frequency:** At least once per sprint, more if defect volume is high.  
**Agenda:**
1. Review all open defects from the current sprint.
2. Agree on severity (Critical / High / Medium / Low).
3. Assign each defect: fix in current sprint, defer to backlog, or mark "won't fix" with justification.
4. Discuss patterns: are the same types of bugs recurring? Is a specific area fragile?

### Sprint Demo (Day 10)

**QA presents:**
- Test coverage summary: how many tests authored, how many passing, how many skipped.
- Defect summary: defects found, defects fixed, defect escape rate.
- Shift-left metrics: how many defects were caught by devs running QA tests (vs. found during QA).

### Retrospective (Day 10)

**Shift-left specific questions:**
- Were QA test specs ready early enough for devs to use?
- Did developers run the QA tests before PRs?
- Were the edge case checklists useful?
- What friction points need to be addressed?
- What went well that we should continue?

### Blame-Free Post-Mortems (When Bugs Escape)

Bugs will occasionally slip through all three quality gates and reach Staging or Production. When this happens, the team conducts a short post-mortem (15-30 minutes) focused on the process, not the person.

**The question is never:** "Who broke this?"  
**The question is always:** "Which quality gate did this slip through, and how do we close that gap?"

**Post-mortem template:**
1. **What happened?** Describe the defect and its impact.
2. **When was it introduced?** Which sprint, which PR, which change.
3. **Which gate should have caught it?** Gate 0 (requirements/design), Gate 1 (unit test), Gate 2 (CI), or Gate 3 (QA handoff)?
4. **Why did it slip through?** Missing test coverage, untested edge case, environment difference, timing issue?
5. **What action do we take?** Add a test, update a checklist, improve a gate, or accept the risk?

**For new team members:** Reviewing a recent post-mortem is one of the best ways to learn about the system's weak points without having to discover them the hard way. Include post-mortem reviews as part of onboarding (see the [Onboarding Guide](ONBOARDING_GUIDE.md)).

---

## 8. Quality Gates — The Four Checkpoints

Quality gates are hard enforcement points where work must prove itself before it can move forward. Unlike ceremonies (which are collaborative), gates are non-negotiable. If work fails a gate, it stops.

### Gate 0: Pre-Development (Requirements & Design)

**When:** Before a story is committed to the sprint and before development begins.  
**Who enforces:** PM/BA (requirements), Solution Designer (design), PO (sign-off).  
**What must pass:**
- Acceptance criteria are defined, specific, and testable (PM/BA validates)
- PO has confirmed the acceptance criteria represent the business need (documented in Jira)
- All business rules are explicitly documented, not assumed as tribal knowledge
- Solution design addresses every acceptance criterion (design-to-requirements traceability)
- Solution Designer has conducted a testability review with QA
- Non-functional requirements (performance, security, scalability) are validated against the design
- Integration point specifications are defined with enough detail for QA to begin writing tests
- Cross-feature impact assessment is completed

**If the gate criteria are not met:** The story is not ready for sprint commitment. It goes back to refinement with a clear note on what is missing. This is not about blocking work — it is about preventing the most expensive category of defects: building the wrong thing.

**Cultural note:** This gate may feel like "slowing down." In reality, a 30-minute requirements validation session prevents days of rework when Dev builds to one interpretation, QA tests to another, and the PO expected a third.

### Gate 1: Pre-Commit (Developer's Machine)

**When:** Before the developer pushes code to the remote branch.  
**Who enforces:** The developer themselves.  
**What must pass:**
- Unit tests for the changed code (`dotnet test` on the relevant project)
- Smoke tests (`dotnet test --filter "Category=Smoke"`)
- Code compiles without errors or new warnings

**Cultural note:** This gate relies on developer discipline. CI will catch failures that slip through, but the goal is to catch them here first — it is faster for the developer and avoids noisy CI failures for the rest of the team.

### Gate 2: Pull Request (CI Pipeline)

**When:** When a developer creates or updates a Pull Request on GitHub.  
**Who enforces:** GitHub Actions (automated), plus human reviewers.  
**What must pass:**
- All smoke tests (blocking — PR cannot merge if these fail)
- All fast tests (blocking)
- All business logic tests (blocking)
- All presentation/controller tests (blocking)
- Integration test suite (continue-on-error initially, blocking after stabilization)
- Playwright E2E smoke specs (continue-on-error initially, blocking after Phase 2)
- At least one peer code review approval

**The "safety net, not shame" principle:** When a PR build fails, it is the system doing its job — catching a problem before it reaches QA or customers. A failed build is not a personal failure. It is the exact moment shift-left is working. The question is never "who broke the build?" but "what did the build just save us from?"

### Gate 3: QA Handoff (Ready for QA)

**When:** When a developer marks a story "Ready for QA" in Jira.  
**Who enforces:** QA Lead verifies the gate criteria before accepting the handoff.  
**What must pass:**
- All CI gates (Gate 2) are green for the merged PR
- Developer has confirmed which QA test suites were run locally (documented in Jira ticket or PR)
- Edge case checklist has been reviewed by the developer (all items addressed or scoped out)
- No known Critical or High defects in the feature area
- Code is deployed to the QA environment

**If the gate criteria are not met:** QA rejects the handoff and moves the ticket back to "In Development" with a comment explaining what is missing. This is not adversarial — it protects QA from spending time on code that is not ready, and it protects the developer from rework on a larger surface area.

### Gate Summary

```
Requirements & Design   Developer's Machine     GitHub Actions CI        QA Environment
┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   GATE 0         │    │   GATE 1         │    │   GATE 2         │    │   GATE 3        │
│   Pre-Development│───>│   Pre-Commit     │───>│   Pull Request   │───>│   QA Handoff    │
│                  │    │                  │    │                  │    │                 │
│ • ACs confirmed  │    │ • Unit tests     │    │ • Smoke tests    │    │ • CI green      │
│ • PO sign-off    │    │ • Smoke tests    │    │ • Business tests │    │ • Tests run     │
│ • Design traced  │    │ • Code compiles  │    │ • Integration    │    │ • Checklist     │
│ • Testability OK │    │                  │    │ • Peer review    │    │ • Deployed      │
│ • NFRs validated │    │                  │    │                  │    │                 │
└──────────────────┘    └──────────────────┘    └──────────────────┘    └─────────────────┘
     PM/BA + SD              Developer               Automated               QA Lead
     (human-enforced)        (self-enforced)          (system-enforced)       (human-verified)
```

---

## 9. Defect Management Workflow

### Dual Defect List System

The project maintains two defect lists. This separation ensures that production code bugs are tracked separately from test infrastructure issues.

| File                                     | Prefix   | Scope                                  | Logged by | Fixed by | Verified by |
|------------------------------------------|----------|----------------------------------------|-----------|----------|-------------|
| `QA Tests/Defect List for Developers.md` | DEF-XXX  | Production code defects (functional, API, security, performance, data integrity) | QA        | Dev      | QA          |
| `QA Tests/Defect List for QA.md`         | QA-XXX   | Test infrastructure issues (mocks, fixtures, CI, tooling, flaky tests)           | QA        | QA       | QA Lead     |

### Defect Lifecycle

```
  ┌─────────┐      ┌─────────────┐      ┌──────────┐      ┌──────────┐      ┌────────┐
  │  Open   │─────>│ In Progress │─────>│ Resolved │─────>│ Verified │─────>│ Closed │
  └─────────┘      └─────────────┘      └──────────┘      └──────────┘      └────────┘
       │                                      │                                   ▲
       │                                      └───── Reopen if fix is ────────────┘
       │                                              incomplete
       └──── Won't Fix / Duplicate ──── (with documented justification)
```

| State         | Who Acts       | What Happens                                             |
|---------------|----------------|----------------------------------------------------------|
| **Open**      | QA logs it     | Defect discovered, documented with repro steps, severity, expected vs. actual |
| **In Progress** | Dev picks it up | Dev investigates, references DEF-XXX in branch/commit    |
| **Resolved**  | Dev marks done | Fix committed, PR merged, deployed to QA environment      |
| **Verified**  | QA retests     | QA confirms the fix on QA environment, runs regression    |
| **Closed**    | QA closes      | Fix is confirmed, defect moved to "Resolved" section in markdown |

### Severity Classification and SLAs

| Severity   | Description                               | Target Resolution      | Examples                                  |
|------------|-------------------------------------------|------------------------|-------------------------------------------|
| Critical   | System down, data loss, security breach    | Same business day      | Auth bypass, data corruption, crash       |
| High       | Major feature broken, no workaround        | Within current sprint  | Core workflow blocked, 500 errors         |
| Medium     | Feature issue with workaround available    | Next sprint            | Edge case failure, UI inconsistency       |
| Low        | Cosmetic, minor enhancement               | Backlog                | Alignment issue, minor wording            |

### Defect Logging in Jira

When QA finds a defect:
1. Log it in Jira with the defect template (summary, repro steps, expected, actual, severity, screenshots).
2. Add the DEF-XXX entry to `Defect List for Developers.md` with the Jira ticket reference.
3. Link the defect to the originating story in Jira.
4. If the defect blocks a test, add `[Fact(Skip = "DEF-XXX: description")]` to the test and note it in the defect entry.

### Cross-Referencing

- Every DEF-XXX in markdown should reference the corresponding Jira ticket ID.
- Every Jira defect should reference the DEF-XXX ID in the description.
- Commit messages for fixes should include `Fixes DEF-XXX`.
- If a QA-XXX test infrastructure issue is caused by a DEF-XXX production defect, both should cross-reference each other.

---

## 10. Katalon to Playwright Transition Plan

### Current State

- Katalon is used for both UI and API test automation.
- Playwright already has ~1,500+ tests and 96 spec files in the repo.
- QA team is primarily experienced with Katalon (record-and-playback, keyword-driven).
- The Playwright infrastructure (config, CI integration, page objects, mock helpers) is already established.

### Phase 0 — Foundation (Weeks 1-2)

**Goal:** Understand what exists and what needs to migrate.

| Activity                                      | Owner    | Output                                |
|-----------------------------------------------|----------|---------------------------------------|
| Audit all Katalon test cases                   | QA Lead  | Spreadsheet of all Katalon tests with names, areas, and last run dates |
| Categorize each Katalon test                   | QA Team  | Each test marked as: "Must Migrate", "Already Covered by Playwright", or "Can Retire" |
| Identify Katalon tests with no Playwright equivalent | QA Lead  | Priority migration list sorted by business risk |
| Verify Playwright infrastructure works for all team members | QA Lead + Dev | All 8 testers can run `npx playwright test` locally |
| Complete Playwright Quickstart training        | QA Team  | All testers complete exercises in `QUICKSTART_FOR_TESTERS.md` |

### Phase 1 — Parallel Running (Weeks 3-6)

**Goal:** Build confidence in Playwright while Katalon is still available.

| Activity                                      | Owner    | Output                                |
|-----------------------------------------------|----------|---------------------------------------|
| All NEW tests written in Playwright only       | QA Team  | No new Katalon tests created after this phase starts |
| Migrate top 20% highest-value Katalon tests    | QA Team  | ~20% of "Must Migrate" list converted using Cursor + Claude |
| Run both Katalon and Playwright suites in parallel | QA Lead  | Comparison report: coverage overlap, failure differences |
| Pair programming: experienced Playwright user + new tester | QA Lead  | Each tester has migrated at least 2 tests |
| Weekly migration stand-up (15 min)             | QA Lead  | Track progress, surface blockers       |

### Phase 2 — Migration Sprint (Weeks 7-12)

**Goal:** Migrate the bulk of remaining Katalon tests.

| Activity                                      | Owner    | Output                                |
|-----------------------------------------------|----------|---------------------------------------|
| Dedicated 2-3 hours per tester per week on migration | QA Team  | Steady migration progress             |
| Use Claude to batch-convert Katalon scripts    | QA Team  | Describe Katalon test in natural language, Claude generates Playwright spec |
| Create page objects for any pages not yet covered | QA Team  | Page object coverage matches Katalon coverage |
| Target: 80% of "Must Migrate" tests converted  | QA Lead  | Migration tracking spreadsheet at 80%+ |
| Identify any Katalon tests that cannot be converted | QA Lead  | Decision: manual test, retire, or alternative automation |

### Phase 3 — Katalon Sunset (Week 13+)

**Goal:** Decommission Katalon entirely.

| Activity                                      | Owner    | Output                                |
|-----------------------------------------------|----------|---------------------------------------|
| Final audit: any remaining Katalon-only coverage | QA Lead  | Confirmation that all critical paths are in Playwright |
| Decommission Katalon infrastructure            | QA Lead + Infra | Katalon licenses returned, servers decommissioned |
| Redirect Katalon license budget                | QA Lead  | Budget allocated to training, tooling, or headcount |
| Update all QA documentation to remove Katalon references | QA Team  | Clean documentation                   |
| Celebrate the milestone                        | Everyone | Team recognition                      |

### Migration Tips for Using Cursor + Claude

When converting a Katalon test to Playwright:

1. Describe what the Katalon test does in plain English: *"This test logs in as an admin, navigates to the partner list, filters by 'Active' status, and verifies that only active partners are shown."*
2. Ask Claude to generate a Playwright spec following the existing patterns in the repo.
3. Review the generated code: does it use the correct page objects? Are the selectors stable (`data-testid` preferred)?
4. Run the test locally with `npx playwright test my-new-test.spec.ts --headed` to see it in action.
5. Commit and push.

---

## 11. Training Roadmap

### For QA Testers (Non-Technical Background)

The QA team's current strength is manual testing from the end-user perspective. This transition builds on that strength by adding AI-assisted test authoring — testers describe what to test, and Claude writes the code.

#### Weeks 1-2: Foundations

| Topic                       | Format              | Materials                              |
|-----------------------------|----------------------|----------------------------------------|
| Cursor IDE basics            | Workshop (2 hours)   | Installation, navigation, terminal     |
| Claude AI interaction        | Workshop (2 hours)   | Prompting, reviewing AI output, iterating |
| Git fundamentals             | Workshop (2 hours)   | Branch, commit, push, pull, PR basics  |
| Command line basics          | Self-paced (1 hour)  | `cd`, `ls`, `dotnet test`, `npm`, `npx`|

**Milestone:** Every tester can open Cursor, ask Claude a question, commit a file, and push to a branch.

#### Weeks 3-4: Playwright Fundamentals

| Topic                       | Format              | Materials                              |
|-----------------------------|----------------------|----------------------------------------|
| What is Playwright           | Workshop (1 hour)    | Quickstart guide walkthrough            |
| Running existing tests       | Hands-on (1 hour)    | `npx playwright test`, `--headed`, `--ui` |
| Reading a test file          | Workshop (1 hour)    | Anatomy of a `.spec.ts` file            |
| Writing a basic test         | Pair programming     | Each tester writes one simple test with a buddy |

**Milestone:** Every tester can run the Playwright suite, read an existing test, and write a simple test with Claude's help.

**Materials:** [Playwright Quickstart for Testers](../Playwright%20Tests/QUICKSTART_FOR_TESTERS.md)

#### Weeks 5-6: AI-Assisted Test Authoring

| Topic                       | Format              | Materials                              |
|-----------------------------|----------------------|----------------------------------------|
| Describing test scenarios to Claude | Workshop (2 hours) | Prompt templates, examples            |
| Reviewing AI-generated tests | Workshop (1 hour)    | What to check: assertions, selectors, edge cases |
| Iterating with Claude        | Hands-on (1 hour)    | Fixing issues, adding scenarios, refining |
| Committing and pushing tests | Practice (ongoing)   | Real sprint work                       |

**Milestone:** Every tester can describe a test scenario to Claude, review the generated code, and commit a working test.

#### Weeks 7-8: C# Integration Test Authoring

| Topic                       | Format              | Materials                              |
|-----------------------------|----------------------|----------------------------------------|
| Reading a C# test file       | Workshop (1 hour)    | Anatomy of an xUnit test               |
| Describing integration test scenarios | Workshop (2 hours) | Prompt templates for controller tests |
| Reviewing AI-generated C# tests | Hands-on (1 hour) | What to check: assertions, mocks, fixtures |
| 3:1 ratio compliance         | Workshop (1 hour)    | Applying the test ratio standard        |

**Milestone:** Every tester can author a basic integration test spec using Claude and verify it compiles.

**Materials:** [QA Tester Playbook — Test Categories Deep Dive](QA_TESTER_PLAYBOOK.md#10-test-categories-deep-dive)

#### Weeks 9-10: Advanced Topics

| Topic                       | Format              | Materials                              |
|-----------------------------|----------------------|----------------------------------------|
| Debugging failing tests      | Workshop (1 hour)    | Reading error messages, trace files     |
| Reading CI logs              | Workshop (1 hour)    | GitHub Actions output, finding failures |
| Updating page objects        | Hands-on (1 hour)    | When selectors break after UI changes   |
| Test maintenance             | Discussion (1 hour)  | When to update vs. skip vs. retire      |

**Milestone:** Testers can independently debug a failing test, read CI output, and update a broken selector.

#### Ongoing Activities

- **Weekly test clinic** (30 minutes): Open forum for questions, live debugging, tips sharing.
- **Pair programming rotation**: Each tester pairs with a different colleague weekly.
- **Monthly skill assessment**: QA Lead checks progress, identifies additional training needs.

### For Developers (Shift-Left Onboarding)

Developers may be skeptical about shift-left. The training focuses on demonstrating value: fewer QA round-trips, faster PR merges, and less context-switching.

#### Session 1: Why Shift-Left Benefits You (1 hour)

- Demo: a bug caught by running QA tests locally vs. the same bug discovered 3 days later by QA.
- Time comparison: 5-minute fix on your machine vs. 2-hour context-switch after QA handoff.
- Data: show defect escape rates and rework costs from the team's Jira history.
- Key message: shift-left reduces your rework, not your velocity.

#### Session 2: Running QA Tests Locally (30 minutes, hands-on)

- Walk through the commands in [Section 5 — Developer Testing Contract](#5-the-developer-testing-contract).
- Run smoke tests, run feature-specific integration tests, interpret results.
- Practice: each developer runs tests for their current story and reports pass/fail.

#### Session 3: Writing Unit Tests with Cursor + Claude (1 hour, hands-on)

- Demo: describe a unit test scenario to Claude, review the generated code, run it.
- Practice: each developer writes one unit test for their current code using AI.
- Discuss: when to write a test manually vs. when to use AI.

#### Ongoing Support

- Developers can tag QA in Slack/Teams when they need help interpreting a test failure.
- QA Lead holds monthly office hours for developer test questions.
- CI dashboards are visible to all — test health is a shared concern.

---

## 12. Metrics and Success Criteria

These metrics help the team track whether shift-left is working. They should be reviewed in sprint retrospectives and monthly leadership reviews.

### Primary Metrics

| Metric                        | Definition                                                          | Baseline (Current) | Target (6 Months) |
|-------------------------------|---------------------------------------------------------------------|---------------------|--------------------|
| **Defect Escape Rate**        | % of defects found in QA/Staging that could have been caught earlier | TBD (measure now)   | 50% reduction      |
| **PR Test Pass Rate**         | % of PRs where CI tests pass on first attempt                       | TBD (measure now)   | > 80%              |
| **Dev Test Execution**        | % of PRs where devs ran QA tests locally (self-reported or CI evidence) | ~0%                 | > 90%              |
| **QA Test Authoring Lead Time** | Average days from sprint start to QA test specs pushed to repo    | N/A (not done yet)  | < 5 days           |
| **Cycle Time (Dev to QA Verified)** | Average days from "dev complete" to "QA verified"              | TBD (measure now)   | < 3 days           |

### Secondary Metrics

| Metric                        | Definition                                                          | Target              |
|-------------------------------|---------------------------------------------------------------------|---------------------|
| **Gate 0 Pass Rate**          | % of stories entering the sprint with PO-confirmed, testable ACs    | > 95%               |
| **Requirements Change Rate**  | Number of AC changes after sprint commitment per sprint              | Decreasing trend    |
| **Design Traceability Score** | % of ACs with documented design-to-requirement traceability (complex stories) | > 90%         |
| **Unit Test Count (Dev-authored)** | Number of unit tests written by developers per sprint            | Increasing trend    |
| **Katalon Test Count**        | Remaining Katalon tests not yet migrated to Playwright              | 0 (at sunset)       |
| **Playwright Test Count**     | Total Playwright E2E test count                                     | Increasing trend    |
| **Flaky Test Rate**           | % of tests that fail intermittently without code changes            | < 5%                |
| **Regression Suite Duration** | Time to run the full Playwright E2E suite                           | < 30 minutes        |
| **Exploratory Testing Sessions** | Number of time-boxed exploratory sessions per sprint             | >= 4 per sprint     |

### How to Collect Metrics

- **Gate 0 Pass Rate:** Track whether each story entering the sprint has a "PO Approved" label or comment in Jira. Calculate per sprint.
- **Requirements Change Rate:** Count Jira AC edits or comments indicating requirement changes after sprint commitment. Decreasing trend indicates improving requirements quality.
- **Design Traceability Score:** For stories flagged as "complex" during refinement, verify whether a design-to-requirements traceability table exists. Calculate per sprint.
- **Defect Escape Rate:** Tag Jira defects with the environment where they were found (Dev, QA, Staging, Prod). Calculate the ratio monthly.
- **PR Test Pass Rate:** GitHub Actions provides pass/fail data per PR. Extract from the `qa-tests.yml` workflow.
- **Dev Test Execution:** Add a checkbox to the PR template: "I have run the relevant QA test suite locally." Track completion rate.
- **QA Test Authoring Lead Time:** Track the date when QA pushes test specs to the repo vs. sprint start date.
- **Cycle Time:** Measure Jira ticket transitions from "In Development" to "Done" (after QA verification).

---

## 13. Tooling Reference

### Tool Inventory

| Tool                  | Purpose                                   | Used By      | Documentation                          |
|-----------------------|-------------------------------------------|--------------|----------------------------------------|
| Cursor                | IDE for coding and test authoring          | Dev + QA     | https://cursor.sh/docs                 |
| Claude (Anthropic)    | AI assistant for code and test generation  | Dev + QA     | Integrated in Cursor                   |
| Playwright            | E2E browser test automation                | QA           | `Playwright Tests/QUICKSTART_FOR_TESTERS.md` |
| xUnit                 | C# unit and integration test framework     | Dev + QA     | https://xunit.net/                     |
| FluentAssertions      | Readable C# test assertions                | Dev + QA     | https://fluentassertions.com/          |
| Moq                   | C# mocking framework                      | Dev + QA     | https://github.com/moq/moq            |
| Jira                  | Defect tracking and sprint management      | Dev + QA + PO | Organization-provided                 |
| Git / GitHub          | Source control, PRs, code review           | Dev + QA     | https://docs.github.com/               |
| GitHub Actions        | CI: automated test execution on PR/push    | Automatic    | `.github/workflows/qa-tests.yml`       |
| Jenkins               | CD: deployment to Dev/QA/Staging/Prod      | Infra        | `deployments/CI-CD/Jenkinsfile`        |
| PostgreSQL            | Production database                        | Dev          | Managed by Infra                       |
| Testcontainers        | Disposable PostgreSQL for test isolation    | QA (tests)   | Integrated in test projects            |
| BenchmarkDotNet       | Performance benchmarking                   | QA (tests)   | Integrated in test projects            |

### Environment Map

| Environment | Purpose                          | Deployed By | Tested By       | URL Pattern         |
|-------------|----------------------------------|-------------|-----------------|---------------------|
| Local       | Developer workstation            | Developer   | Dev (unit + QA tests) | `localhost:5159` / `localhost:4200` |
| Dev         | Integration environment          | Jenkins     | Dev + CI        | Internal            |
| QA          | QA testing and exploratory       | Jenkins     | QA              | Internal            |
| Staging     | Pre-production validation, UAT   | Jenkins     | QA + PO         | Internal            |
| Production  | Live system                      | Jenkins     | Monitoring only | Public              |

### CI Pipeline — Test Execution Flow

```
Developer pushes code / creates PR
         │
         ▼
┌─────────────────────────────────────────┐
│         GitHub Actions Triggered         │
├─────────────────────────────────────────┤
│                                          │
│  ┌──────────────┐  ┌──────────────────┐ │
│  │ Smoke Tests  │  │ Fast Tests       │ │
│  │ (~14 tests)  │  │ (~43 tests)      │ │
│  │ BLOCKING     │  │ BLOCKING         │ │
│  └──────────────┘  └──────────────────┘ │
│           │                  │           │
│           ▼                  ▼           │
│  ┌──────────────┐  ┌──────────────────┐ │
│  │ Business     │  │ Presentation     │ │
│  │ Tests        │  │ Tests            │ │
│  │ (~4,000)     │  │ (~164 tests)     │ │
│  │ BLOCKING     │  │ BLOCKING         │ │
│  └──────────────┘  └──────────────────┘ │
│           │                  │           │
│           ▼                  ▼           │
│  ┌──────────────┐  ┌──────────────────┐ │
│  │ Integration  │  │ Playwright       │ │
│  │ Tests        │  │ Smoke E2E        │ │
│  │ (~5,500)     │  │ (~6 specs)       │ │
│  │ CONTINUE     │  │ CONTINUE         │ │
│  └──────────────┘  └──────────────────┘ │
│                                          │
│  ┌──────────────────────────────────┐   │
│  │ Test Summary — Aggregated Report │   │
│  └──────────────────────────────────┘   │
│                                          │
└─────────────────────────────────────────┘
         │
         ▼
   PR merge allowed (if blocking gates pass)
         │
         ▼
   Jenkins deploys to target environment
```

---

## Appendix A: Quick Reference — "Who Does What?"

A one-page summary for printing or pinning to the team board.

```
╔═════════════════════════════════════════════════════════════════════════════════════╗
║                         SHIFT-LEFT QUICK REFERENCE                                 ║
╠═════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                    ║
║  PM/BA MUST:                        SOLUTION DESIGNER MUST:                        ║
║  ───────────                        ────────────────────────                        ║
║  ✓ Validate ACs with PO            ✓ Trace design to every AC                     ║
║  ✓ Apply testability gate           ✓ Conduct testability review with QA           ║
║  ✓ Document all business rules      ✓ Validate NFRs against design                ║
║  ✓ Assess cross-feature impact      ✓ Specify integration points for test authoring║
║  ✓ Manage requirement changes       ✓ Present design walkthrough to Dev + QA       ║
║                                                                                    ║
║  PM/BA MUST NOT:                    SOLUTION DESIGNER MUST NOT:                    ║
║  ────────────────                   ───────────────────────────                     ║
║  ✗ Allow stories without PO-        ✗ Hand off design without                      ║
║    confirmed ACs into sprint          traceability to ACs                           ║
║  ✗ Leave business rules as          ✗ Assume testability — QA must                 ║
║    tribal knowledge                   confirm they can test it                     ║
║  ✗ Allow silent scope changes       ✗ Defer NFR validation to "later"              ║
║                                                                                    ║
║  DEVELOPERS MUST:                   QA MUST:                                       ║
║  ─────────────────                  ────────                                       ║
║  ✓ Write unit tests                 ✓ Author integration/E2E test specs            ║
║  ✓ Run QA tests locally pre-PR      ✓ Provide edge case checklists                ║
║  ✓ Fix code when tests fail         ✓ Perform exploratory testing                  ║
║  ✓ Review edge case checklists      ✓ Run E2E regression before release            ║
║  ✓ Pass CI gates before merge       ✓ Log and track defects                        ║
║                                                                                    ║
║  DEVELOPERS MUST NOT:               QA MUST NOT:                                   ║
║  ────────────────────               ────────────                                   ║
║  ✗ Skip running QA tests            ✗ Write unit tests                             ║
║  ✗ Weaken test assertions           ✗ Modify production source code                ║
║  ✗ Modify QA tests without          ✗ Wait until dev is done to start              ║
║    QA approval                         authoring tests                             ║
║                                                                                    ║
║  QUALITY GATES:                                                                    ║
║  ───────────────                                                                   ║
║  Gate 0:  Pre-Development (ACs confirmed, design validated, testability OK)        ║
║  Gate 1:  Pre-Commit (unit tests, smoke tests, code compiles)                      ║
║  Gate 2:  Pull Request (CI: smoke, business, integration, peer review)             ║
║  Gate 3:  QA Handoff (CI green, tests run, checklist reviewed, deployed)           ║
║                                                                                    ║
║  HANDSHAKE POINTS:                                                                 ║
║  ──────────────────                                                                ║
║  Pre-Sprint:  Requirements validation (PM/BA + PO) — Gate 0                       ║
║  Day 1-2:  Design walkthrough (SD + Dev + QA) + Three Amigos (edge cases)          ║
║  Day 3-5:  QA pushes test specs → Dev pulls and runs                               ║
║  Day 6-8:  Dev marks "Ready for QA" → QA explores + regresses                      ║
║  Day 8-9:  Joint bug triage                                                        ║
║  Day 10:   Sprint demo (test metrics) + Retrospective                              ║
║                                                                                    ║
╚═════════════════════════════════════════════════════════════════════════════════════╝
```

---

## Appendix B: Jira Workflow Integration

### Recommended Jira Ticket States (Enhanced for Shift-Left)

```
┌──────────┐   ┌──────────────┐   ┌──────────────────┐   ┌───────────────┐
│  To Do   │──>│In Development│──>│ Ready for QA     │──>│  In QA Test   │
└──────────┘   └──────────────┘   └──────────────────┘   └───────────────┘
                      │                                          │
                      │                                          ▼
                      │                                   ┌───────────────┐
                      │                                   │    Done       │
                      │                                   └───────────────┘
                      │                                          ▲
                      ▼                                          │
                ┌──────────────┐                                 │
                │ QA Tests     │  (QA authors tests in parallel  │
                │ In Progress  │   while dev codes)              │
                └──────────────┘─────────────────────────────────┘
```

### Definition of Done (Updated for Shift-Left)

A story is "Done" when ALL of the following are true:

- [ ] Acceptance criteria were PO-confirmed before sprint commitment (Gate 0).
- [ ] Design-to-requirements traceability was validated (for complex stories).
- [ ] Code is written and peer-reviewed (PR approved).
- [ ] Unit tests written by dev and passing in CI.
- [ ] QA-authored integration tests passing in CI.
- [ ] Smoke tests passing in CI.
- [ ] Developer has run QA test suite locally (confirmed in PR or ticket).
- [ ] Edge case checklist reviewed by dev (all items addressed or documented as out of scope).
- [ ] Code deployed to QA environment.
- [ ] QA exploratory testing completed (session notes logged).
- [ ] E2E regression passing for affected area.
- [ ] No open Critical or High defects against the story.
- [ ] PO accepts the story (UAT on Staging if required).

---

## Appendix C: FAQ

**Q: Do developers need to learn Playwright?**  
A: No. Developers run C# tests (unit, integration, smoke) using `dotnet test`. Playwright E2E tests are owned entirely by QA and run in CI. Developers are not expected to write or maintain Playwright tests.

**Q: What if QA test specs are not ready when the developer needs them?**  
A: This is a process failure, not a blocker. The developer should continue with their unit tests and raise the gap in standup. QA should prioritize getting the test specs out. Over time, QA writing tests early will become routine.

**Q: What if a developer disagrees with a QA test expectation?**  
A: The developer should discuss it with QA. If the test expectation is correct (the code should do what the test says), the developer fixes their code. If the test expectation is wrong (based on an incorrect requirement), QA updates the test. Neither side should silently change assertions.

**Q: Will this slow down development?**  
A: Initially, there may be a small increase in developer time per story (running tests, reviewing checklists). However, this is offset by a large reduction in rework cycles. Teams that shift left typically see a net reduction in total cycle time after 2-3 sprints.

**Q: What if developers refuse to run QA tests?**  
A: CI enforces the tests automatically — PRs that fail integration tests cannot be merged. Running tests locally is recommended (faster feedback) but CI is the enforced gate. Leadership support is essential to make this cultural shift stick.

**Q: Does this mean PM/BAs and Solution Designers are now "testers"?**  
A: No. They are validating their own deliverables — requirements and designs — against clear quality criteria. Just as developers "test" their code with unit tests, PM/BAs "test" their requirements for completeness and testability, and Solution Designers "test" their designs for traceability and feasibility. The testing pyramid starts before code.

**Q: What if the PM/BA cannot get PO confirmation before sprint commitment?**  
A: The story is not ready. An unconfirmed story is a best guess, not a requirement. Committing to it risks building the wrong thing. If PO availability is the bottleneck, escalate — this is a process issue that affects the entire team's effectiveness, not just QA.

**Q: How does this work with our current (imperfect) Scrum process?**  
A: Shift-left does not require perfect Scrum. It requires four things: (1) PM/BA validates requirements with PO before commitment, (2) Solution Designer validates design before coding starts, (3) QA writes tests early, (4) devs run tests before handoff. Everything else is an improvement opportunity, not a prerequisite.

**Q: What happens to manual testing?**  
A: Manual testing becomes more focused and valuable. Instead of manually clicking through the same regression paths every sprint, QA uses that time for end-to-end scenario testing — validating complete happy-path workflows and edge cases across the system that automated unit and integration tests cannot fully cover. This includes testing complex multi-step user journeys, cross-feature interactions, and real-world data scenarios that reveal issues only visible when the full system is exercised together.

---

## Appendix D: Related Documents

| Document                          | Location                                          | Purpose                              |
|-----------------------------------|---------------------------------------------------|--------------------------------------|
| Shift-Left Scorecard              | `QA Tests/Documentation/SHIFT_LEFT_SCORECARD.md` | Measurement criteria, sprint dashboard, maturity model |
| Action Items (Dev + QA)           | `QA Tests/Documentation/ACTION_ITEMS.md`         | Living to-do list for developers and QA |
| Onboarding Guide                  | `QA Tests/Documentation/ONBOARDING_GUIDE.md`     | 30-60-90 day plan for new QA and Dev hires |
| QA Tester Playbook                | `QA Tests/Documentation/QA_TESTER_PLAYBOOK.md`   | Day-to-day QA practices and standards |
| Playwright Quickstart for Testers | `QA Tests/Playwright Tests/QUICKSTART_FOR_TESTERS.md` | Playwright setup and writing guide  |
| Testing Structure                 | `QA Tests/Documentation/TESTING_STRUCTURE.md`     | Repository test organization map      |
| Defect List for Developers        | `QA Tests/Defect List for Developers.md`          | Production code defects (DEF-XXX)    |
| Defect List for QA                | `QA Tests/Defect List for QA.md`                  | Test infrastructure issues (QA-XXX)  |
| Test Ratio Enforcement Rule       | `.cursor/rules/test-ratio-enforcement.mdc`        | 3:1 ratio standard                   |
| QA Write Boundaries Rule          | `.cursor/rules/qa-write-boundaries.mdc`           | QA file modification restrictions    |
| Never Weaken Tests Rule           | `.cursor/rules/never-weaken-tests.mdc`            | Test assertion integrity             |
| Defect Management Standard        | `.cursor/rules/defect-management.mdc`             | Defect logging and triage process    |

---

## Appendix E: Onboarding New Team Members

For the full onboarding guide — including the 30-60-90 day framework, buddy system, shadowing/reverse shadowing, role-specific checklists, and first-sprint walkthrough — see the standalone document:

**[Onboarding Guide — Joining a Shift-Left Team](ONBOARDING_GUIDE.md)**

That guide is the single source of truth for onboarding. Hand it to new hires on Day 1 alongside this manifesto.

---

## Version History

| Version | Date       | Author     | Changes                    |
|---------|------------|------------|----------------------------|
| 1.0     | 2026-03-05 | QA Lead    | Initial draft for team review |
| 1.1     | 2026-03-05 | QA Lead    | Added: Quality Gates section (Section 7), defect cost statistics, blame-free post-mortems |
| 1.2     | 2026-03-05 | QA Lead    | Extracted onboarding content from Appendix E into standalone ONBOARDING_GUIDE.md |
| 1.3     | 2026-03-05 | QA Lead    | Comprehensive test type coverage: Restructured Testing Pyramid to 5 labeled levels (L1–L5) showing all 16 test types. Added Functional Tests (3.5), Validation Tests (3.6), Concurrency Tests (3.8), Playwright Test Automation (L4). Moved Regression Testing to cross-cutting. Added "Test Types vs. Test Categories (3:1 Ratio)" section explaining relationship between pyramid test types and ratio categories (Positive, Negative, Edge/Boundary). Added Level column to Ownership Matrix. Renumbered Section 3 subsections (3.1–3.14). Synced documentation to PDJ project. |
| 1.4     | 2026-03-09 | QA Lead    | Added pre-development validation roles: New L0 layer in Testing Pyramid for Requirements Validation (PM/BA) and Design Validation (Solution Designer). New Section 4 "Pre-Development Validation Contracts" with Solution Designer Validation Contract (4.1) and PM/BA Requirements Validation Contract (4.2). Added Gate 0 (Pre-Development) to Quality Gates — now four gates instead of three. Updated Handshake Points with Requirements Validation, Design Walkthrough, and PM/BA participation in Three Amigos. Added Solution Designer and PM/BA to Ownership Matrix, Quick Reference (Appendix A), Core Principles, and Audience. Added Gate 0 metrics (Gate 0 Pass Rate, Requirements Change Rate, Design Traceability Score). Added FAQ entries for PM/BA and SD roles. Renumbered Sections 4–12 to 5–13. |
