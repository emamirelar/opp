# QA Tester Playbook

**Version:** 2.2  
**Last Updated:** March 9, 2026  
**Audience:** QA Testers (New and Experienced)  
**Scope:** Universal guide applicable to any software project

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [QA Lifecycle Overview](#2-qa-lifecycle-overview)
3. [Phase 1: Project Onboarding](#3-phase-1-project-onboarding)
   - [3.2 Test Case Locations & Project Map](#32-test-case-locations--project-map)
   - [3.4 Git Repository Tracking Verification](#34-git-repository-tracking-verification)
   - [3.5 Starting the Development Proxy for Real Backend Testing](#35-starting-the-development-proxy-for-real-backend-testing)
4. [Phase 2: Test Planning](#4-phase-2-test-planning)
   - [4.3 Stakeholder Alignment on Test Cases](#43-stakeholder-alignment-on-test-cases)
   - [4.5 What QA Should Receive Before Writing Tests](#45-what-qa-should-receive-before-writing-tests)
5. [Phase 3: Test Development](#5-phase-3-test-development)
6. [Phase 4: Test Execution](#6-phase-4-test-execution)
7. [Phase 5: Defect Management](#7-phase-5-defect-management)
   - [7.5 Developer Defect Resolution Workflow](#75-developer-defect-resolution-workflow)
   - [7.6 AI Tools & Cursor Subagents for QA](#76-ai-tools--cursor-subagents-for-qa)
   - [7.7 Playwright Setup & Running E2E Tests](#77-playwright-setup--running-e2e-tests)
8. [Manual vs Automated Testing Decision Guide](#8-manual-vs-automated-testing-decision-guide)
9. [The 3:1 Test Ratio Standard](#9-the-31-test-ratio-standard)
10. [Test Categories Deep Dive](#10-test-categories-deep-dive)
11. [Manual Test Case Templates](#11-manual-test-case-templates)
12. [Automated Test Templates](#12-automated-test-templates)
13. [Test Execution Reporting](#13-test-execution-reporting)
14. [Quick Reference Cards](#14-quick-reference-cards)
15. [Troubleshooting Common Issues](#15-troubleshooting-common-issues)
16. [Glossary](#16-glossary)

---

## 1. Introduction

### 1.1 Purpose

This playbook is the single source of truth for QA testing practices. It provides:

- **New QA Testers**: Step-by-step onboarding and learning path
- **Experienced QA Testers**: Quick reference for standards and best practices
- **Project Teams**: Consistent quality assurance across all projects

### 1.2 Core Principles

| Principle | Description |
|-----------|-------------|
| **Quality Over Speed** | Finding defects early saves time and money. Never rush testing. |
| **Test What Matters** | Focus on user-critical paths and high-risk areas first. |
| **Automate Strategically** | Not everything should be automated. Choose wisely. |
| **Document Everything** | If it's not documented, it didn't happen. |
| **Collaborate Early** | Involve QA from requirements gathering, not just before release. |

### 1.3 How to Use This Playbook

```
New QA Testers:        Read sections 1-7 sequentially, then reference as needed
Experienced QA:        Jump to specific sections or use Quick Reference Cards
Test Leads:            Use sections 2, 4, 9 for planning and standards
```

---

## 2. QA Lifecycle Overview

### 2.1 The Testing Lifecycle

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           QA LIFECYCLE                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐          │
│  │ PHASE 1  │───▶│ PHASE 2  │───▶│ PHASE 3  │───▶│ PHASE 4  │          │
│  │Onboarding│    │ Planning │    │Development│   │Execution │          │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘          │
│       │               │               │               │                  │
│       ▼               ▼               ▼               ▼                  │
│  • Environment    • Test         • Write         • Run tests            │
│  • Access           Strategy       test cases    • Log defects          │
│  • Documentation  • Coverage     • Build         • Report               │
│  • Tools            Planning       fixtures      • Retest               │
│                                                                          │
│                           ┌──────────┐                                  │
│                           │ PHASE 5  │                                  │
│                           │  Defect  │                                  │
│                           │Management│                                  │
│                           └──────────┘                                  │
│                                │                                         │
│                                ▼                                         │
│                       • Triage • Track                                  │
│                       • Verify • Close                                  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 QA Entry and Exit Criteria

#### Entry Criteria (When to Start Testing)

| Criteria | Required? | Description |
|----------|-----------|-------------|
| Requirements documented | ✅ Yes | User stories, PRD, or acceptance criteria exist |
| Build is deployable | ✅ Yes | Code compiles, no blocking build errors |
| Test environment ready | ✅ Yes | Database seeded, services running |
| Test data available | ✅ Yes | Realistic data for testing scenarios |
| Smoke test passes | ✅ Yes | Basic functionality works |

#### Exit Criteria (When Testing is Complete)

| Criteria | Required? | Description |
|----------|-----------|-------------|
| All test cases executed | ✅ Yes | 100% of planned tests run |
| Critical defects resolved | ✅ Yes | No P0/P1 defects open |
| Defect leakage < 5% | ⚠️ Target | Post-release defects minimal |
| Test coverage met | ⚠️ Target | Code coverage and requirement coverage goals |
| Sign-off obtained | ✅ Yes | Stakeholder approval documented |

---

## 3. Phase 1: Project Onboarding

> **Note:** This section covers generic project onboarding (tools, access, environment setup). For the full 30-60-90 day growth plan, buddy system, and shift-left onboarding track, see the [Onboarding Guide](ONBOARDING_GUIDE.md).

### 3.1 Onboarding Checklist

Use this checklist when joining a new project:

#### Day 1: Environment Setup

- [ ] **Request access** to:
  - [ ] Source code repository (Git, Azure DevOps, etc.)
  - [ ] Test management tool (JIRA, Azure Test Plans, etc.)
  - [ ] CI/CD pipeline dashboards
  - [ ] Test and staging environments
  - [ ] Documentation repositories

- [ ] **Install development tools**:
  - [ ] IDE (Cursor)
  - [ ] Test runners (dotnet CLI, npm, pytest, etc.)
  - [ ] Browser testing tools (Playwright)
  - [ ] Database client (pgAdmin) — connects to PostgreSQL on Google Cloud SQL using IAM Authentication
  - [ ] API testing (HttpClient via xUnit integration tests) — REST only

- [ ] **Clone repository** and verify build:
  ```bash
  git clone <repository-url>
  cd <project-folder>
  # Build and verify no errors
  dotnet build     # .NET projects
  npm install      # Node.js projects
  ```

- [ ] **Verify Git branch tracking** (see [Section 3.4](#34-git-repository-tracking-verification)):
  - [ ] Confirm you are on the correct branch (`git branch --show-current`)
  - [ ] Verify your branch tracks the correct remote branch (`git branch -vv`)
  - [ ] Verify the remote URL points to the correct repository (`git remote -v`)
  - [ ] If tracking is wrong, fix it immediately before making any commits

- [ ] **Verify development proxy setup** (see [Section 3.5](#35-starting-the-development-proxy-for-real-backend-testing)):
  - [ ] Confirm you know how to start the development proxy
  - [ ] Understand which test types require the proxy to be running
  - [ ] Add "start proxy" to your pre-testing checklist

#### Days 2-3: Documentation Review

- [ ] **Read key documents** (in this order):
  1. Project README and architecture overview
  2. Product Requirements Document (PRD) or user stories
  3. Existing test strategy/plan documents
  4. Known issues and defect backlog
  5. Previous test execution reports

- [ ] **Understand the application**:
  - [ ] What problem does it solve?
  - [ ] Who are the users? (personas)
  - [ ] What are the critical user journeys?
  - [ ] What integrations exist? (APIs, databases, third-party services)

#### Days 4-5: Hands-On Exploration

- [ ] **Exploratory testing** of the application:
  - [ ] Walk through all main features
  - [ ] Note questions and potential test areas
  - [ ] Identify high-risk areas

- [ ] **Run existing tests**:
  ```bash
  # Run all tests
  dotnet test                    # .NET
  npm test                       # Node.js
  npx playwright test           # Playwright E2E
  
  # Check test results
  # Note: Some failures are expected with stub implementations
  ```

- [ ] **Review test infrastructure**:
  - [ ] Test fixtures and data seeding
  - [ ] Stub/mock implementations
  - [ ] Test utilities and helpers

### 3.2 Test Case Locations & Project Map

Understanding where different types of tests live in the repository is essential. Use the directory map below to quickly locate test files by purpose.

#### Project Test Directory Map

```
opportunityplus/                                    (Repository Root)
│
├── QA Tests/                                       ─── TOP-LEVEL QA DIRECTORY ───
│   │
│   ├── Documentation/                              📖 QA guides and playbooks
│   │   ├── QA_TESTER_PLAYBOOK.md                      This document
│   │   ├── TESTING_STRUCTURE.md                       Test architecture overview
│   │   ├── SHIFT_LEFT_TESTING_MANIFESTO.md            Team strategy and roles
│   │   ├── SHIFT_LEFT_SCORECARD.md                    Measurement criteria and maturity model
│   │   ├── ACTION_ITEMS.md                            Living to-do for Dev and QA
│   │   ├── ONBOARDING_GUIDE.md                        30-60-90 day plan for new hires
│   │   └── PRODUCTION_READINESS_CHECKLIST.md          Pre-release checklist
│   │
│   ├── Defect List for Developers.md               🐛 Product defects (DEF-XXX)
│   ├── Defect List for QA.md                       🐛 Test infra issues (QA-XXX)
│   │
│   ├── Playwright Tests/                           🎭 E2E BROWSER TESTS (Playwright/TypeScript)
│   │   ├── *.spec.ts                                  102 spec files (login, partners, workflows, etc.)
│   │   ├── helpers/                                   Test utilities & data builders
│   │   └── pages/                                     21 Page Object Model classes
│   │
│   ├── Frontend Tests/                             🖥️ ANGULAR COMPONENT TESTS (Karma/Jasmine)
│   │   ├── components/                                Component-level spec files
│   │   └── services/                                  Service-level spec files
│   │
│   ├── C# Tests/                                   ⚙️ BACKEND UNIT & FUNCTIONAL TESTS (xUnit/C#)
│   │   ├── UNOPS.PAO.Business.Tests/                  Business layer tests
│   │   │   ├── Core/                                     Positive, Negative, Boundary tests
│   │   │   ├── Functional/                               Contact, Opportunity, Partner tests
│   │   │   ├── Security/                                 Security tests
│   │   │   ├── Concurrency/                              Race condition tests
│   │   │   ├── Performance/                              Performance & load tests
│   │   │   ├── EdgeCases/                                Edge case tests
│   │   │   ├── Unit/                                     Isolated unit tests
│   │   │   ├── Validation/                               Input validation tests
│   │   │   ├── Authorization/                            Role & permission tests
│   │   │   ├── JIRA/                                     Requirement-linked tests
│   │   │   ├── TestBase/                                 Test fixtures & base classes
│   │   │   └── TestData/                                 Test data builders
│   │   │
│   │   ├── UNOPS.PAO.Presentation.Tests/              Controller/API tests
│   │   │   └── Controllers/                              REST endpoint tests
│   │   │
│   │   └── UNOPS.PAO.FastTests/                       Lightweight logic-only tests
│   │       └── *.cs                                      Quick-running unit tests
│   │
│   ├── Integration Tests/                          🔗 INTEGRATION TESTS (xUnit/C#)
│   │   ├── Controllers/                               20+ controller integration tests
│   │   ├── Database/                                  Database integration tests
│   │   ├── Permissions/                               Permission tests
│   │   ├── AI/                                        AI feature integration tests
│   │   ├── DST/                                       DST analysis tests
│   │   └── Infrastructure/                            Infrastructure tests
│   │
│   ├── ───────────── TEST CASE DOCUMENTATION (Markdown) ──────────────
│   │
│   ├── Opportunity Tests/                          📝 Opportunity test case specs
│   │   ├── BusinessLogic/                             13 business logic test cases
│   │   ├── Controllers/                               8 controller test cases
│   │   ├── Managers/                                  8 manager test cases
│   │   └── Services/                                  3 service test cases
│   │
│   ├── Partner Tests/                              📝 Partner test case specs
│   │   └── *.md                                       Ecosystem, Hierarchy, Intelligence
│   │
│   ├── Admin Tests/                                📝 Admin feature test case specs
│   ├── AI Tests/                                   📝 AI assistant test case specs
│   ├── Authorization Tests/                        📝 Role matrix test case specs
│   ├── UI Tests/                                   📝 UI/UX test case specs
│   ├── Services Tests/                             📝 Service layer test case specs
│   ├── Business Logic Tests/                       📝 Business rule test case specs
│   ├── Controllers Tests/                          📝 Controller test case specs
│   ├── Edge Cases & Security Tests/                📝 Edge case & security test case specs
│   ├── CRM Enhancement Tests/                      📝 CRM feature test case specs
│   ├── Unit Tests/                                 📝 Unit test case documentation
│   │   └── Business/                                  25+ manager/service test docs
│   │
│   ├── Business Manager Functional Test List/      📝 Per-manager functional test lists
│   ├── Load Tests/                                 📝 Load test documentation
│   ├── Performance Tests/                          📝 Performance test documentation
│   ├── Security Tests/                             📝 Security test documentation
│   │
│   ├── ──────────────────── SUPPORTING FILES ─────────────────────────
│   │
│   ├── Test Plans/                                 📋 Test planning documents
│   ├── Test Execution Results/                     📊 Test run reports & summaries
│   ├── TestTemplates/                              📄 12 reusable test templates
│   ├── TestSpecification/                          📐 Test specification infrastructure
│   └── Scripts/                                    🛠️ PowerShell & SQL setup scripts
│
├── UNOPS.PAO.ClientApp/src/app/                    🖥️ ANGULAR IN-SOURCE COMPONENT TESTS
│   └── **/*.spec.ts                                   Component & service spec files
│
├── UNOPS.PAO.IntegrationTests/                     🔗 WORKFLOW INTEGRATION TESTS
│   └── UnitTests/Workflow/                            Workflow state machine tests
│
├── playwright.config.ts                            ⚙️ Playwright configuration (root)
│
└── .github/
    └── workflows/
        └── qa-tests.yml                            🚀 CI/CD pipeline (11-job build+test pipeline)
```

#### Test Types at a Glance

| Test Type | Language / Tool | Location | Purpose |
|-----------|----------------|----------|---------|
| **E2E Browser Tests** | TypeScript / Playwright | `QA Tests/Playwright Tests/*.spec.ts` | Simulate real user interactions through the browser to validate complete workflows end-to-end, including login, navigation, form submissions, and cross-page flows. |
| **Angular Component Tests** | TypeScript / Karma+Jasmine | `QA Tests/Frontend Tests/` and `UNOPS.PAO.ClientApp/src/app/**/*.spec.ts` | Test individual Angular components and services in isolation to verify rendering, data binding, event handling, and service logic without a full browser. |
| **Backend Unit Tests** | C# / xUnit | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/` | Test business logic, validation rules, calculations, and manager methods in isolation using stubs and mocks — no database or HTTP calls. |
| **Backend Fast Tests** | C# / xUnit | `QA Tests/C# Tests/UNOPS.PAO.FastTests/` | Lightweight, quick-running unit tests for specific logic (e.g., ERP dimension values, workflow logic) designed to run in seconds. |
| **Controller / API Tests** | C# / xUnit | `QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests/` | Test REST API controllers to verify routing, request/response mapping, authorization attributes, and HTTP status codes. |
| **Integration Tests** | C# / xUnit | `QA Tests/Integration Tests/` | Test multiple layers together (controller → manager → database) against a real or in-memory database to verify end-to-end data flows. |
| **Workflow Integration Tests** | C# / xUnit | `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/` | Verify workflow state machine transitions, stage providers, approver logic, and workflow user context. |
| **Test Case Documentation** | Markdown (`.md`) | `QA Tests/{Feature} Tests/*.md` | Human-readable test case specifications organized by feature area. Used for manual test planning, review, and traceability back to requirements. |
| **Smoke Tests** | TypeScript / Playwright | Integrated in `QA Tests/Playwright Tests/` + CI pipeline | A small, fast subset of critical-path E2E tests run after every deployment to confirm the build is viable for further testing. Configured in `.github/workflows/playwright-tests.yml`. |
| **Performance & Load Tests** | C# / xUnit | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Performance/` | Measure response times, throughput, and resource consumption under normal and stress conditions to ensure non-functional requirements are met. |
| **Security Tests** | C# / xUnit | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Security/` | Validate protection against OWASP Top 10 vulnerabilities including injection, broken access control, XSS, and privilege escalation. |
| **Test Scripts** | PowerShell / SQL | `QA Tests/Scripts/` | Automation scripts for test environment setup, database seeding, configuration validation, and test data management. |
| **Test Templates** | Various | `QA Tests/TestTemplates/` | Reusable scaffolding templates for creating new test files consistently across the project. |

#### How to Find the Right Tests

Use this decision guide when you need to locate or create tests:

```
What do you need to test?
    │
    ├── A user workflow in the browser?
    │   └── → QA Tests/Playwright Tests/*.spec.ts  (E2E)
    │
    ├── An Angular component or service?
    │   └── → UNOPS.PAO.ClientApp/src/app/**/*.spec.ts  (Component tests)
    │         or QA Tests/Frontend Tests/  (Standalone FE tests)
    │
    ├── A C# manager method or business rule?
    │   └── → QA Tests/C# Tests/UNOPS.PAO.Business.Tests/  (Unit/Functional)
    │
    ├── A REST API controller endpoint?
    │   └── → QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests/  (Controller tests)
    │
    ├── A full data flow (API → DB → response)?
    │   └── → QA Tests/Integration Tests/  (Integration tests)
    │
    ├── Test case specifications for manual testing or review?
    │   └── → QA Tests/{Feature} Tests/*.md  (Markdown docs)
    │
    ├── A quick sanity check after deployment?
    │   └── → Smoke tests in Playwright suite + CI workflow
    │
    └── Performance, security, or load validation?
        └── → QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Performance/ or /Security/
```

### 3.3 Key Questions to Ask

| Question | Why It Matters |
|----------|----------------|
| What are the release cycles? | Determines testing timeline |
| What test types are prioritized? | Focuses your effort |
| What's the defect workflow? | Know how to log issues |
| Who are the subject matter experts? | Know who to ask questions |
| What's been problematic historically? | High-risk areas to focus on |
| What's the test data strategy? | Understand data dependencies |

### 3.4 Git Repository Tracking Verification

**MANDATORY**: Before making any commits, verify your local branch is correctly tracking the intended remote branch. Incorrect tracking can cause commits to be pushed to the wrong branch or repository, leading to lost work, merge conflicts, or accidental code leakage.

#### Why This Matters

| Risk | Consequence |
|------|-------------|
| **Wrong remote branch** | Commits pushed to `main` instead of your feature/QA branch |
| **Wrong remote URL** | Code pushed to a different repository entirely |
| **Detached HEAD** | Commits not associated with any branch, easily lost |
| **Stale tracking** | Local branch tracks a deleted or renamed remote branch |
| **No upstream set** | `git push` fails or pushes to unexpected destination |

#### Verification Checklist

Run these commands **every time** you start working on a new branch or return to an existing one:

```bash
# 1. Check which branch you are on
git branch --show-current
# Expected: Your working branch (e.g., QA-Tests, feature/my-feature)

# 2. Check branch tracking (shows upstream remote/branch)
git branch -vv
# Expected: Your branch shows [origin/correct-branch-name]
# Example: * QA-Tests  abc1234 [origin/QA-Tests] Last commit message

# 3. Verify remote URL points to the correct repository
git remote -v
# Expected: origin  https://github.com/your-org/correct-repo.git (fetch)
#           origin  https://github.com/your-org/correct-repo.git (push)

# 4. Check the full tracking configuration
git config --get branch.$(git branch --show-current).remote
# Expected: origin (or the correct remote name)

git config --get branch.$(git branch --show-current).merge
# Expected: refs/heads/correct-branch-name
```

#### How to Fix Incorrect Tracking

```bash
# Fix 1: Set/change the upstream tracking branch
git branch --set-upstream-to=origin/correct-branch-name

# Fix 2: If the remote URL is wrong
git remote set-url origin https://github.com/your-org/correct-repo.git

# Fix 3: If you need to create a new tracking relationship
git push -u origin your-branch-name

# Fix 4: If on detached HEAD, create a branch first
git checkout -b my-branch-name
git push -u origin my-branch-name
```

#### When to Verify Tracking

Perform the verification checklist at these key moments:

| Moment | Why |
|--------|-----|
| **After cloning** a repository | Ensure default branch and remote are correct |
| **After checking out** a branch | Confirm the branch tracks the right upstream |
| **Before your first commit** of the day | Catch issues before they become problems |
| **After a `git fetch` or `git pull`** | Remote state may have changed |
| **After resolving merge conflicts** | Ensure you're still on the right branch |
| **When switching between projects** | Prevent cross-project confusion |
| **Before pushing** any commits | Last chance to catch tracking errors |

#### Quick Verification One-Liner

Use this single command to display all critical tracking information at a glance:

```bash
echo "Branch: $(git branch --show-current)" && echo "Remote: $(git remote -v | head -1)" && git branch -vv --list "$(git branch --show-current)"
```

#### Add to Git Aliases (Recommended)

Add this alias to your global Git configuration for quick verification:

```bash
git config --global alias.check-tracking '!echo "Branch: $(git branch --show-current)" && echo "---" && git remote -v && echo "---" && git branch -vv --list "$(git branch --show-current)"'
```

Then simply run:

```bash
git check-tracking
```

---

### 3.5 Starting the Development Proxy for Real Backend Testing

> **⚠️ MANDATORY before any real backend testing.** Forgetting to start the proxy is one of the most common reasons integration tests and E2E tests fail with connection errors. Make it part of your pre-test ritual.

#### What Is the Development Proxy?

The development proxy routes HTTP requests from the Angular frontend (or test client) to the .NET backend API. Without it running:

- The frontend cannot communicate with the backend
- Integration tests that make real API calls get `Connection refused` or `Network error` responses
- E2E Playwright tests that exercise real backend flows will fail silently or time out

#### When You Need the Proxy Running

| Test Type | Proxy Required? | Why |
|-----------|----------------|-----|
| **C# Unit tests** (stub/mock-based) | ❌ No | Tests use in-memory stubs — no HTTP calls |
| **C# Integration tests** (real DB) | ✅ Yes | Tests make real API calls via the proxy |
| **Playwright E2E tests** | ✅ Yes | Browser navigates through the real application stack |
| **Manual browser testing** | ✅ Yes | Frontend needs to reach backend APIs |
| **API testing (Postman, etc.)** | ✅ Yes | Calls route through the proxy layer |

#### How to Start the Proxy

From the repository root, start the development proxy **before** launching any tests that require real backend access:

```bash
# Terminal 1 — Start the .NET backend API
cd UNOPS.PAO.Server
dotnet run
# Backend listens on https://localhost:7123 by default
# (or whichever port is set in ASPNETCORE_HTTPS_PORT / ASPNETCORE_URLS)

# Terminal 2 — Start the Angular dev server (auto-loads proxy config)
cd UNOPS.PAO.ClientApp
npm start
# ng serve picks up src/proxy.conf.js automatically (configured in angular.json)
# Routes /user/* and /api/** to the .NET backend
```

> **Tip:** The proxy target port is read from the `ASPNETCORE_HTTPS_PORT` environment variable, then `ASPNETCORE_URLS`, and falls back to `https://localhost:7123`. Check `UNOPS.PAO.ClientApp/src/proxy.conf.js` for the full routing rules.

#### Proxy Readiness Checklist

Run through this checklist **before** executing integration or E2E tests:

- [ ] .NET backend API is running and listening (check terminal output for the port)
- [ ] Angular dev server is running (`npm start` completed, no port conflicts)
- [ ] Proxy configuration file exists (`UNOPS.PAO.ClientApp/src/proxy.conf.js`)
- [ ] Browser can reach `http://localhost:4200` and API calls return data (not network errors)
- [ ] No `EADDRINUSE` or port conflict errors in any terminal

#### Quick Verification

Open a browser and navigate to the application URL. Open DevTools → Network tab and trigger any API call. Verify:

- Request goes to `/api/...` (not a 404 or CORS error)
- Response is `200 OK` (not `ERR_CONNECTION_REFUSED`)

If you see connection errors, the proxy or backend is not running — stop and fix this before continuing with any real backend tests.

#### Common Proxy Failure Symptoms

| Symptom | Likely Cause |
|---------|-------------|
| `ERR_CONNECTION_REFUSED` on API calls | Backend not running |
| `404 Not Found` on `/api/*` routes | Proxy not started (Angular dev server down) |
| CORS errors in browser console | Proxy not routing correctly — check `src/proxy.conf.js` target port matches the running backend |
| Playwright tests time out on login | Proxy/backend not running when E2E suite starts |
| Integration tests fail with `HttpRequestException` | Test fixture pointing at real endpoint but proxy is down |

---

## 4. Phase 2: Test Planning

### 4.1 Test Strategy Development

Before writing tests, develop a strategy document covering:

```markdown
# Test Strategy Template

## 1. Scope
- Features in scope
- Features out of scope
- Test types to be performed

## 2. Test Approach
- Manual vs automated testing split
- Test levels (unit, integration, E2E)
- Test data requirements

## 3. Test Environment
- Environment URLs
- Browser/device coverage
- Test account credentials

## 4. Schedule
- Test phases and milestones
- Resource allocation
- Dependencies

## 5. Risks and Mitigations
- Identified risks
- Contingency plans

## 6. Deliverables
- Test cases
- Execution reports
- Defect reports
```

### 4.2 Test Coverage Planning

Use the **3:1 Ratio Rule** (detailed in [Section 9](#9-the-31-test-ratio-standard)):

```
For every 1 positive (happy path) test:
Create 3 negative/edge case tests

Total = Positive + (3 × Positive) = 4× coverage
```

#### Coverage Categories

| Category | What to Test | Priority |
|----------|--------------|----------|
| **Positive Tests** | Valid inputs, successful workflows | 🔴 Critical |
| **Negative Tests** | Invalid inputs, error handling | 🔴 Critical |
| **Edge Cases** | Boundary values, timing issues | 🔴 Critical |
| **Functional Tests** | Business rules, workflow logic, audit rules | 🔴 Critical |
| **Integration Tests** | End-to-end CRUD, relationships, search/filter | 🔴 Critical |
| **Security Tests** | Injection, authorization, data exposure | 🟠 High |
| **Concurrency Tests** | Race conditions, duplicate submissions | 🟠 High |
| **Performance Tests** | Load, stress, response times | 🟡 Medium |
| **Accessibility Tests** | WCAG compliance, screen readers | 🟡 Medium |

### 4.3 Stakeholder Alignment on Test Cases

**MANDATORY**: Before executing tests, share the created test cases with the **Project Manager (PM)** and/or **Business Analyst (BA)** for their review, agreement, and understanding of the level of testing planned for their project.

#### Why This Matters

| Reason | Benefit |
|--------|---------|
| **Shared understanding** | PM/BA confirm the testing scope matches business expectations |
| **Coverage validation** | Stakeholders can identify missing scenarios or priorities |
| **Risk alignment** | Ensures high-risk areas are tested to the level the business requires |
| **No surprises** | PM/BA are aware of what will and won't be tested before execution begins |
| **Traceability** | Documented agreement creates an audit trail for test scope decisions |

#### Process

1. **Prepare test case summary**: Compile the list of test cases organized by feature area, test type (positive, negative, edge case, security, etc.), and priority
2. **Schedule a review session**: Set up a brief meeting or send the test cases for asynchronous review
3. **Walk through coverage**: Explain the testing approach, the 3:1 ratio standard, and any risk-based prioritization decisions
4. **Capture feedback**: Document any additional scenarios, priority changes, or scope adjustments requested by PM/BA
5. **Obtain sign-off**: Get explicit agreement (email, meeting notes, or sign-off in test management tool) before proceeding to test execution
6. **Update test plan**: Incorporate any agreed changes into the test cases and re-share if significant modifications were made

#### What to Share

| Artifact | Format | Purpose |
|----------|--------|---------|
| Test case list with categories | Markdown table or spreadsheet | Shows breadth of coverage |
| Test coverage matrix | Requirements vs test cases mapping | Proves all requirements are covered |
| Risk-based priority assignments | Sorted list by risk score | Shows where effort is focused |
| Out-of-scope items | Explicit list | Prevents assumptions about untested areas |
| Test data requirements | Summary | Highlights any data dependencies or constraints |

#### Sign-Off Template

```markdown
## Test Case Review Sign-Off

**Project:** [Project Name]
**Feature/Sprint:** [Feature or Sprint Name]
**Total Test Cases:** [Count]
**Review Date:** YYYY-MM-DD

### Reviewers

| Role | Name | Agreement | Date | Notes |
|------|------|-----------|------|-------|
| PM | | ☐ Agreed / ☐ Changes Requested | | |
| BA | | ☐ Agreed / ☐ Changes Requested | | |
| QA Lead | | ☐ Agreed / ☐ Changes Requested | | |

### Scope Agreement
- [ ] Test coverage level is appropriate for project risk
- [ ] All critical user journeys are covered
- [ ] Out-of-scope items are acknowledged
- [ ] Test data requirements are feasible
```

> **Note:** This step is not a gate to slow down testing — it is a quality checkpoint to ensure testing effort is aligned with business priorities. Keep the review lightweight and focused.

### 4.4 Risk-Based Testing

Prioritize testing based on risk:

```
Risk Score = Probability × Impact

High Risk (Score 7-9):    Test extensively, automate critical paths
Medium Risk (Score 4-6):  Test thoroughly, automate key scenarios
Low Risk (Score 1-3):     Basic testing, manual coverage sufficient
```

| Risk Factor | Low (1) | Medium (2) | High (3) |
|-------------|---------|------------|----------|
| **Complexity** | Simple CRUD | Business logic | Complex workflows |
| **User Impact** | Admin only | Internal users | All customers |
| **Data Sensitivity** | Public info | Internal data | PII, financial |
| **Change Frequency** | Stable | Occasional | Frequent changes |
| **Integration Points** | None | Internal APIs | External systems |

### 4.5 What QA Should Receive Before Writing Tests

**MANDATORY**: Before authoring test specs for any story, verify that you have received the required inputs from PM/BA and Solution Designer. These inputs are defined by the [Shift-Left Manifesto Gate 0](SHIFT_LEFT_TESTING_MANIFESTO.md#8-quality-gates--the-four-checkpoints). If they are missing, raise the gap in standup — do not guess at requirements.

#### Required Inputs from PM/BA

| Input | What It Should Contain | Why QA Needs It | If Missing |
|-------|----------------------|-----------------|------------|
| **PO-confirmed acceptance criteria** | Specific, measurable criteria with PO sign-off documented in Jira | Your tests validate these criteria — they are the specification | Do not write tests. Raise in standup. Story is not ready. |
| **Documented business rules** | Explicit rules: who can approve, what triggers a notification, when a field is required, what formula calculates a value | Business rules become your test assertions — undocumented rules cannot be tested | Ask PM/BA to document them. Log as a gap in Three Amigos. |
| **Cross-feature impact assessment** | Which existing features the new requirement might affect | Determines your regression scope — which existing test suites to re-run | Ask PM/BA: "What else does this touch?" Add to Three Amigos agenda. |
| **Requirements change log** | Any changes to ACs after sprint commitment, with Dev + QA acknowledgement | Prevents testing against outdated requirements | If you discover a change during testing that you were not notified of, raise it immediately with PM/BA. |

#### Required Inputs from Solution Designer (Complex/New Features)

| Input | What It Should Contain | Why QA Needs It | If Missing |
|-------|----------------------|-----------------|------------|
| **Design-to-requirements traceability table** | Requirement → Design Component → How It Will Be Tested | Confirms every AC is addressed by the design — gaps here become untested areas | Ask SD during design walkthrough: "Which design component covers AC #3?" |
| **Integration point specifications** | API contracts (request/response schemas, status codes, error formats), data flows, external dependencies | You write integration tests and mocks from these specs — insufficient detail means wrong tests | Ask SD: "Can I write a mock or stub from this spec?" If not, it needs more detail. |
| **NFR validation** | Performance targets, security requirements, scalability constraints with design confirmation | Feeds your performance and security test scenarios | Ask SD: "What are the performance targets, and does the design meet them?" |
| **Testability assessment** | Confirmation from the design walkthrough that every component can be tested | Prevents writing tests for components that have no observable outputs | If you cannot figure out how to test a component, raise it at the design walkthrough. |

#### How to Use These Inputs

```
1. Receive PO-confirmed ACs from PM/BA           ──> Write positive tests (happy path)
2. Review business rules documentation            ──> Write functional and validation tests
3. Study integration point specs from SD          ──> Write integration and API tests
4. Combine with Three Amigos edge case checklist  ──> Write negative, boundary, and edge case tests
5. Review NFR targets                             ──> Write performance and security tests
6. Check cross-feature impact assessment          ──> Identify regression scope
```

#### Gate 0 Verification Checklist for QA

Before writing your first test for a story, confirm:

- [ ] Acceptance criteria exist in Jira and are marked as PO-confirmed
- [ ] Each AC is specific enough that you can write a test for it (testability gate)
- [ ] Business rules are documented — not just "the BA told me verbally"
- [ ] For complex stories: design-to-requirements traceability table exists
- [ ] For stories with API changes: integration point specifications are available
- [ ] Cross-feature impact assessment identifies which existing tests to re-run
- [ ] Three Amigos has been conducted and edge case checklist is attached to the ticket

If any item is missing, raise it in standup and with the PM/BA or SD directly. Do not proceed with test authoring based on assumptions — assumptions produce tests that validate guesses, not requirements.

> **See also:** [Manifesto Section 4](SHIFT_LEFT_TESTING_MANIFESTO.md#4-the-pre-development-validation-contracts) for the full PM/BA and Solution Designer validation contracts.

---

## 5. Phase 3: Test Development

### 5.1 Test Case Design Techniques

#### Equivalence Partitioning

Divide inputs into groups (partitions) that should behave the same way:

```
Example: Age field (valid: 0-120)

Partitions:
- Invalid (negative): -1, -100
- Valid (0-120): 0, 50, 120
- Invalid (>120): 121, 999
```

#### Boundary Value Analysis

Test at the edges of valid ranges:

```
Example: Username (3-20 characters)

Boundaries:
- Below minimum: 2 characters ❌
- At minimum: 3 characters ✅
- Just above minimum: 4 characters ✅
- Just below maximum: 19 characters ✅
- At maximum: 20 characters ✅
- Above maximum: 21 characters ❌
```

#### Decision Table Testing

For complex business rules with multiple conditions:

| Condition 1 | Condition 2 | Condition 3 | Expected Result |
|-------------|-------------|-------------|-----------------|
| True | True | True | Action A |
| True | True | False | Action B |
| True | False | True | Action C |
| ... | ... | ... | ... |

#### State Transition Testing

For workflows with distinct states:

```
Draft → Submitted → Under Review → Approved → Published
                              ↘ Rejected → Draft (resubmit)
```

Test:
- Valid transitions (Draft → Submitted)
- Invalid transitions (Draft → Published)
- State persistence after transitions

### 5.2 Test Data Management

#### Principles

1. **Independence**: Tests should not depend on each other's data
2. **Repeatability**: Same data produces same results
3. **Isolation**: Test data doesn't affect production
4. **Realism**: Data reflects real-world scenarios

#### Test Data Categories

| Category | Examples | Usage |
|----------|----------|-------|
| **Valid** | Real-looking names, valid emails | Positive tests |
| **Invalid** | Empty strings, special chars | Negative tests |
| **Boundary** | Max/min values, edge lengths | Edge case tests |
| **Malicious** | SQL injection, XSS payloads | Security tests |
| **Large Volume** | 10K+ records | Performance tests |

### 5.3 Test File Organization

Recommended folder structure for test projects:

```
Tests/
├── Documentation/
│   ├── QA_TESTER_PLAYBOOK.md        # This document
│   └── Archive/                      # Historical reports
├── TestBase/
│   ├── Fixtures/                     # Test setup classes
│   ├── Utilities/                    # Helper methods
│   ├── Stubs/                        # Mock implementations
│   └── TestData/                     # Seed data files
├── [Module_Name]/
│   └── [JIRA-ID]_[FeatureName]/
│       ├── README.md                 # Test case overview
│       ├── PositiveTests.cs          # Happy path tests
│       ├── NegativeTests.cs          # Error handling tests
│       ├── EdgeCaseTests.cs          # Boundary tests
│       ├── SecurityTests.cs          # Security validations
│       ├── ConcurrencyTests.cs       # Race condition tests
│       └── *.spec.ts                 # E2E Playwright tests
├── Defect List for Developers.md     # Product defects
├── Defect List for QA.md             # Test infrastructure issues
└── playwright.config.js              # E2E test configuration
```

### 5.4 Test Data Conventions & Infrastructure

This section documents the standard patterns for creating and managing test data across C# and Playwright tests. Following these conventions ensures consistency, reduces duplication, and improves data isolation.

#### C# Test Data: Fluent Builders (`TestEntityBuilder`)

Use the fluent builder API in `TestBase/TestEntityBuilder.cs` instead of raw SQL or per-fixture seed methods. Builders follow a **get-or-create** pattern for reference data (Currency, Country, SDG, OrgHierarchy, EntityRole, InitiativeType) and an **always-create** pattern for transactional entities (Partner, Opportunity, Contact, Interaction).

```csharp
// Create reference data (idempotent — returns existing ID if already seeded)
var currencyId = await TestEntityBuilder.Currency().WithCode("USD").BuildAsync(context);
var countryId  = await TestEntityBuilder.Country().WithIso2("US").WithName("United States").BuildAsync(context);
var orgId      = await TestEntityBuilder.OrgHierarchy().WithCode("HQ").WithName("Headquarters").BuildAsync(context);
var roleId     = await TestEntityBuilder.EntityRole().WithCode("Opportunity_Manager_Opportunity").BuildAsync(context);

// Create transactional entities (always creates a new record)
var partnerId = await TestEntityBuilder.Partner()
    .WithName("UNICEF")
    .WithStatus(EntityStatus.Active)
    .WithCreatedBy(userId)
    .BuildAsync(context);

var oppId = await TestEntityBuilder.Opportunity()
    .WithName("Infrastructure Project")
    .WithStage("IDENTIFY & PROFILE")
    .WithCreatedBy(userId)
    .WithResponsibleOrgUnit(orgId)
    .BuildAsync(context);
```

Available builders: `User`, `Partner`, `Opportunity`, `Currency`, `Country`, `SDG`, `OrgHierarchy`, `Contact`, `Interaction`, `EntityRole`, `InitiativeType`, `Output`.

#### C# Test Data: User Creation

Always use `TestDataHelper.GetOrCreateTestUserAsync()` (or the synchronous `GetOrCreateTestUser()`) for creating test users. Never use raw SQL `INSERT INTO "AspNetUsers"`.

```csharp
// In fixture constructors — two-phase pattern for PostgreSQL
using var tempCtx = TestDbContextFactory.CreateUNOPS(DbContextOptions);
TestUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "test@unops.org");
Context = TestDbContextFactory.CreateUNOPSWithUserId(DbContextOptions, TestUserId);
```

#### C# Database Modes

Tests can run against two database providers controlled by the `USE_POSTGRESQL` environment variable:

| Mode | Provider | Use Case | FK Enforcement |
|------|----------|----------|---------------|
| Default | **SQLite in-memory** | Fast local development, CI | Off by default; opt-in via `SQLITE_ENABLE_FK=true` |
| PostgreSQL | **Npgsql** | Full-fidelity integration testing | Always on (database enforced) |

Set `SQLITE_ENABLE_FK=true` to enable SQLite foreign key constraints when testing referential integrity.

#### C# Fake Data with Bogus

The `Bogus` NuGet package is available for generating realistic test data. Use it for strings, emails, names, and other values where realistic variety improves test quality.

```csharp
using Bogus;

var faker = new Faker();
var partnerId = await TestEntityBuilder.Partner()
    .WithName(faker.Company.CompanyName())
    .WithShortDescription(faker.Company.CatchPhrase())
    .BuildAsync(context);
```

#### Playwright Mock Data: JSON Fixtures

Static mock data for E2E tests lives in `QA Tests/Playwright Tests/fixtures/`:

| File | Contents |
|------|----------|
| `reference-data.json` | Dropdowns: partners, org units, countries, SDGs, currencies, salutations, pronouns, statuses, gemini models, document types |
| `partners.json` | Partner list, search results, detail template |
| `contacts.json` | Contact list, search results, detail template |
| `opportunities.json` | Opportunity list, search results |
| `interactions.json` | Interaction list, search results, detail, partner interactions, brief list |
| `dashboard.json` | Dashboard content, org unit recent updates |

Import fixtures in mock helpers:

```typescript
import referenceData from '../fixtures/reference-data.json';
import partnersFixture from '../fixtures/partners.json';
```

#### Playwright Mock Data: Workflow Helpers

Use the shared helpers in `helpers/workflow-mocks.helper.ts` instead of duplicating route mocks in each spec file:

```typescript
import {
  setupNotificationsMock,
  createWorkflowNotification,
  setupOpportunityMock,
  setupOpportunityPermissionsMock,
  getOpportunityPayload,
  getWorkflowOpportunityPayload,
  setupPendingApprovalsMock,
  createPendingApproval,
  FULL_PERMISSIONS,
  READONLY_PERMISSIONS,
  APPROVER_PERMISSIONS,
} from './helpers/workflow-mocks.helper';
```

#### Playwright Data Isolation

Call `resetWorkflowMockState()` in `beforeEach` to reset the in-memory workflow state between tests. The `setupAPIMocks()` function already resets this automatically, but call it explicitly when tests manipulate workflow state.

#### ID Conventions for Mock Data

| ID Range | Entity | Purpose |
|----------|--------|---------|
| 1–9 | Any | Standard happy-path entities |
| 10 | Opportunity | Cancelled opportunity |
| 11 | Opportunity | No-Go opportunity |
| 12 | Opportunity | In-workflow opportunity (pending approval) |
| 100+ | Any | Bulk/stress test entities |

---

## 6. Phase 4: Test Execution

### 6.1 Execution Order

Execute tests in this order for maximum effectiveness:

```
1. Smoke Tests          → Quick verification build is testable
2. Critical Path Tests  → Core functionality works
3. Regression Tests     → Existing features still work
4. New Feature Tests    → New functionality validates
5. Edge Case Tests      → Boundary conditions handled
6. Security Tests       → No vulnerabilities exposed
7. Performance Tests    → System meets NFRs
```

### 6.2 Running Automated Tests

> **⚠️ Pre-flight check — Start the proxy first!**
> Before running integration tests or Playwright E2E tests, verify the development proxy and backend are running. See [Section 3.5](#35-starting-the-development-proxy-for-real-backend-testing) for the full checklist. Unit tests (stub-based) do NOT require the proxy.

#### Command Line Execution

```bash
# .NET Tests
dotnet test                                          # Run all
dotnet test --filter "Category=Smoke"               # Run by category
dotnet test --filter "FullyQualifiedName~JIRA-123"  # Run by name pattern

# Playwright E2E Tests
npx playwright test                                  # Run all
npx playwright test --grep "login"                  # Run by pattern
npx playwright test --headed                        # Run with browser visible
npx playwright test --debug                         # Debug mode

# Generate Reports
dotnet test --logger "trx;LogFileName=results.trx"  # .NET TRX report
npx playwright test --reporter=html                 # Playwright HTML report
```

#### Test Execution Best Practices

| Practice | Description |
|----------|-------------|
| **Clean state** | Reset database/environment before test runs |
| **Parallel execution** | Run independent tests concurrently |
| **Retry flaky tests** | Configure 1-2 retries for intermittent failures |
| **Capture evidence** | Screenshots, logs, videos for failures |
| **Monitor resources** | Watch for memory leaks, CPU spikes |

### 6.3 Manual Test Execution

When executing manual tests:

1. **Prepare test environment**
   - Verify environment is in known state
   - Confirm test data is seeded
   - Clear browser cache/cookies if UI testing

2. **Execute step by step**
   - Follow test steps exactly as written
   - Capture screenshots of key steps
   - Note actual vs expected results

3. **Document results**
   - Mark Pass/Fail/Blocked/Skipped
   - Add notes for unexpected behavior
   - Link defects to failed tests

4. **Report immediately**
   - Log defects as soon as found
   - Don't batch defect reporting

### 6.4 Handling Test Failures

```
Test Failed
    │
    ▼
Is it a valid failure?
    │
    ├── YES → Log defect, link to test
    │
    ├── NO (Test issue) → Fix test, re-run
    │
    └── NO (Environment issue) → Mark blocked, investigate
```

---

## 7. Phase 5: Defect Management

### 7.1 Defect Classification

#### Two Defect Lists

| List | Scope | Prefix | Examples |
|------|-------|--------|----------|
| **Defect List for Developers** | Product bugs, missing features | DEF-XXX | Missing API endpoint, wrong calculation |
| **Defect List for QA** | Test infrastructure issues | QA-XXX | Stub incomplete, fixture broken |

#### Severity Levels

| Level | Icon | Description | Example |
|-------|------|-------------|---------|
| **Critical** | 🔴 | System crash, data loss, security hole | Payment processing fails |
| **High** | 🟠 | Major feature broken, no workaround | Cannot submit form |
| **Medium** | 🟡 | Feature impaired, workaround exists | Filter doesn't work, can search manually |
| **Low** | 🟢 | Minor issue, cosmetic | Typo in label |

### 7.2 Defect Template

```markdown
| ID | Severity | Title | Description | Reproduction Steps | Expected | Actual | Related Test | Reported | Status |
|---|---|----|----|-----|-----|-----|-----|-----|-----|
| DEF-XXX | 🔴/🟠/🟡/🟢 | [Brief title] | [Detailed description]<br/><br/>**Environment:** [ENV]<br/>**Browser:** [BROWSER]<br/>**User Role:** [ROLE] | 1. Step one<br/>2. Step two<br/>3. Step three | [Expected result] | [Actual result] | [TestFile.cs:line] or N/A | YYYY-MM-DD | Open |
```

**Related Test column guidance:**

| Discovery Context | Related Test Value | Example |
|---|---|---|
| Automated test failure | `FileName.cs:line` | `PositiveTests.cs:45` |
| Manual test case | Test case ID | `TC-FORM-001` |
| Exploratory testing | `N/A (exploratory)` | `N/A (exploratory)` |
| Code review | `N/A (code review)` | `N/A (code review)` |

### 7.3 Defect Triage

Use this decision tree:

```
Defect Discovered
    │
    ▼
Is it in product code?
    │
    ├── YES → Defect List for Developers (DEF-XXX)
    │         • Business logic bugs
    │         • API issues
    │         • Missing features
    │
    └── NO → Defect List for QA (QA-XXX)
             • Test fixture issues
             • Stub incomplete
             • Test data problems
```

### 7.4 Defect Lifecycle

```
Open → In Progress → Fixed → Ready for Verification → Verified → Closed
                        │                                    │
                        └── Failed Verification ─────────────┘
```

### 7.5 Developer Defect Resolution Workflow

This section describes the end-to-end process a developer follows to pick up a defect from the defect list, fix it using Claude (Cursor AI), verify the fix with the tests that exposed it, and close the defect.

#### Step 1: Review the Defect List

Open `QA Tests/Defect List for Developers.md` and review the **Open** defects table. Each row contains:

| Column | What it Tells You |
|---|---|
| **ID** | The defect identifier (e.g., `DEF-051`) — use this everywhere |
| **Severity** | Priority for fix order: 🔴 Critical → 🟠 High → 🟡 Medium → 🟢 Low |
| **Component** | Which manager, controller, or service is affected |
| **Description** | Root cause analysis, proper fix guidance, and anti-patterns to avoid |
| **Related Tests** | The test file and line number that exposed the defect |

**Pick the highest-severity open defect** in the component you're working on.

#### Step 2: Find the Failing Tests

Every defect has tests tagged with `[Trait("Defect", "DEF-XXX")]` that currently **run and fail** in CI. These tests are your verification targets.

```bash
# Find all tests tagged for a specific defect
dotnet test --filter "Defect=DEF-051" --no-build --verbosity normal
```

You can also search the test codebase directly:

```bash
# Find test files referencing the defect
rg "DEF-051" "QA Tests/" --files-with-matches
```

**Read the failing test carefully** — the test name and assertion describe what the code SHOULD do, and the `[Trait("Defect", "DEF-XXX")]` comment usually explains why it currently fails.

#### Step 3: Use Claude to Implement the Fix

In Cursor, ask Claude to fix the production code. Provide the defect context:

**Effective prompt template:**
```
Fix DEF-051 in [ComponentName].

Defect: [paste the Description column from the defect list]

The test that exposes this is in:
  [paste Related Tests path, e.g., QA Tests/Business Logic Tests/Feature/NegativeTests.cs:45]

The test expects: [paste the Expected column]
The code currently does: [paste the Actual column]

The proper fix described in the defect list is:
  [paste the Proper Fix bullets]

Do NOT: [paste the Wrong Fix anti-pattern]
```

**Important constraints for Claude:**
- The fix goes in **production code** (e.g., `UNOPS.PAO.Business/Managers/`, `UNOPS.PAO.API/Controllers/`)
- QA tests in `QA Tests/` must NOT be modified to make them pass — only production code changes
- Follow the `.cursor/rules/dotnet-implementation.mdc` patterns (IsDeleted filters, async/await, etc.)

#### Step 4: Run the Defect Tests to Verify

After Claude applies the fix, run the specific defect tests:

```bash
# Run only the tests for this defect
dotnet test --filter "Defect=DEF-051" --verbosity detailed

# If multiple test files are involved, run the full suite for the feature
dotnet test --filter "FullyQualifiedName~FeatureName" --verbosity normal
```

| Test Result | What to Do |
|---|---|
| **All tests pass** ✅ | Proceed to Step 5 |
| **Some tests still fail** ❌ | Review the failure, refine the fix, re-run |
| **New tests break** ⚠️ | The fix introduced a regression — investigate and adjust |

#### Step 5: Remove the Defect Trait

Once all defect-tagged tests pass, **remove the `[Trait("Defect", "DEF-XXX")]` attribute** from each test. This promotes the test back into the gating CI suite so it blocks future regressions.

```csharp
// BEFORE (defect test — runs in defect job, doesn't block PRs)
[Fact]
[Trait("Category", "Negative")]
[Trait("Defect", "DEF-051")]
public async Task Get_EmptyEnvironmentName_UsesHostEnvironment()
{
    response.Environment.Should().Be("Production");
}

// AFTER (regression test — runs in gating job, blocks PRs if it fails)
[Fact]
[Trait("Category", "Negative")]
public async Task Get_EmptyEnvironmentName_UsesHostEnvironment()
{
    response.Environment.Should().Be("Production");
}
```

#### Step 6: Update the Defect List

Update the defect row in `QA Tests/Defect List for Developers.md`:

1. Change **Status** from `Open` to `Resolved`
2. Add resolution details:
   - **Resolution Notes**: Brief description of what was changed
   - **Fixed By**: Your name
   - **Fix Commit/PR**: The commit hash or PR number
3. Move the row to the **Resolved Defects** section of the file

#### Step 7: Run the Full Test Suite

Before committing, run the broader test suite to catch any regressions:

```bash
# Run all non-defect tests (the gating suite)
dotnet test --filter "Defect!~DEF" --verbosity normal

# Optionally run the full suite including remaining defects
dotnet test --verbosity normal
```

#### Complete Developer Workflow — Quick Reference

```
┌─────────────────────────────────────────────────────────────────┐
│                  DEVELOPER DEFECT FIX WORKFLOW                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. READ    QA Tests/Defect List for Developers.md              │
│             Pick highest-severity Open defect                   │
│                          │                                      │
│  2. FIND    dotnet test --filter "Defect=DEF-XXX"               │
│             Read the failing test — it IS the specification     │
│                          │                                      │
│  3. FIX     Ask Claude in Cursor with full defect context       │
│             Fix goes in PRODUCTION code, never in test code     │
│                          │                                      │
│  4. VERIFY  dotnet test --filter "Defect=DEF-XXX"               │
│             All tagged tests must pass                          │
│                          │                                      │
│  5. PROMOTE Remove [Trait("Defect", "DEF-XXX")] from tests     │
│             Tests move to gating suite                          │
│                          │                                      │
│  6. UPDATE  Mark defect as Resolved in the defect list          │
│             Add resolution notes, commit/PR reference           │
│                          │                                      │
│  7. REGRESS dotnet test --filter "Defect!~DEF"                  │
│             Full gating suite must still pass                   │
│                          │                                      │
│  8. COMMIT  git add -A && git commit -m "fix: DEF-XXX ..."     │
│             Reference defect ID in commit message               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### Commit Message Format for Defect Fixes

```
fix(component): Brief description of the fix

Fixes DEF-XXX

- Root cause: [explanation from defect list]
- Solution: [what was changed]
- Testing: [X defect tests now pass, promoted to gating suite]
```

### 7.6 AI Tools & Cursor Subagents for QA

Cursor IDE provides specialized AI subagents that automate test creation, defect diagnosis, and E2E test management. QA testers and developers can invoke these via the Cursor chat interface using natural language prompts.

#### Available QA Subagents

| Subagent | What It Does | When to Use |
|---|---|---|
| **create-tests** | Generates full xUnit C# test suites (all 9 files) from PRDs, Jira stories, or feature descriptions | "Create tests for the Go/No-Go Decision feature" |
| **load-tests** | Generates `LoadTests.cs` with sustained load, spike, stress, and recovery tests | "Create load tests for the Partner Manager" |
| **performance-tests** | Generates `PerformanceTests.cs` with SLA, throughput, N+1, and memory tests | "Create performance tests for Opportunity search" |
| **playwright-test-generator** | Generates Playwright `.spec.ts` files and Page Object Models | "Create Playwright tests for the partner list page" |
| **playwright-test-healer** | Diagnoses and fixes broken Playwright tests (selectors, mocks, timing) | "Fix the failing partner-item Playwright tests" |
| **playwright-test-planner** | Plans E2E test coverage, identifies gaps, creates test strategies | "Plan Playwright test coverage for the Opportunities module" |

#### Available QA Skills (Agent Skills)

Skills are specialized instruction sets that guide Claude through complex test generation workflows. They are automatically activated when relevant.

| Skill | Location | Trigger |
|---|---|---|
| **generate-playwright** | `.cursor/skills/generate-playwright/SKILL.md` | "Write Playwright tests for ..." |
| **generate-load** | `.cursor/skills/generate-load/SKILL.md` | "Write load tests for ..." |
| **generate-performance** | `.cursor/skills/generate-performance/SKILL.md` | "Write performance tests for ..." |

#### How to Invoke a Subagent

In Cursor chat, simply describe what you need. Cursor automatically selects the right subagent:

```
User: "Create tests for the Recall Opportunity feature based on the PRD in tasks/recall-opportunity/"

→ Cursor invokes the create-tests subagent
→ Reads the PRD, extracts acceptance criteria
→ Generates all 9 C# test files with 3:1 ratio compliance
→ Logs any blockers as QA-XXX issues
```

```
User: "Create Playwright E2E tests for the partner detail page"

→ Cursor invokes the playwright-test-generator subagent
→ Reads existing POMs and spec files
→ Generates partner-item.spec.ts with mock setup
→ Creates/updates partner-item.page.ts POM
```

```
User: "The partner list Playwright tests are failing after the UI redesign, fix them"

→ Cursor invokes the playwright-test-healer subagent
→ Analyzes failure traces and screenshots
→ Updates selectors, fixes mocks, resolves timing issues
→ Logs any production bugs as DEF-XXX
```

#### Key Rules All Subagents Follow

1. **3:1 Ratio Enforcement** — Every test suite must have ≥3× negative, boundary, functional, and integration tests per positive test
2. **Never Weaken Tests** — If a test fails because the code is wrong, the subagent logs a `DEF-XXX` defect and tags the test with `[Trait("Defect", "DEF-XXX")]` — it never changes the assertion
3. **QA Write Boundaries** — Subagents only write to `QA Tests/` and `UNOPS.PAO.ClientApp/src/qa-frontend-tests/` — never to production code
4. **Requirement-Driven** — Tests validate the specification (what code SHOULD do), not just the current implementation
5. **Defect Management** — Blockers are logged in the appropriate defect list (`DEF-XXX` for product bugs, `QA-XXX` for test infrastructure)

#### Cursor Rules That Govern Testing

These `.cursor/rules/` files are automatically applied and control how Claude generates tests:

| Rule File | Purpose |
|---|---|
| `test-ratio-enforcement.mdc` | Enforces the 3:1 ratio — mandatory compliance check before completing |
| `comprehensive-test-strategy.mdc` | Full 10-category test standard with code examples |
| `never-weaken-tests.mdc` | Prevents changing test assertions to match broken code |
| `qa-write-boundaries.mdc` | Restricts AI writes to QA-owned folders only |
| `requirement-driven-testing.mdc` | Requires cross-referencing requirements before creating tests |
| `defect-management.mdc` | Templates and triage rules for defect logging |
| `dotnet-implementation.mdc` | .NET patterns (IsDeleted, async, repositories) that tests must validate |

### 7.7 Playwright Setup & Running E2E Tests

#### Prerequisites

| Software | Version | Download |
|---|---|---|
| Node.js | 20 or newer | https://nodejs.org/ |
| Git | Any recent | https://git-scm.com/ |
| Cursor (or VS Code) | Any recent | Your IDE |

#### First-Time Installation

```bash
# 1. Navigate to the project root
cd c:\Users\YourName\git\opportunityplus

# 2. Go to the QA Tests folder
cd "QA Tests"

# 3. Install Playwright and all dependencies
npm install

# 4. Install browser engines (Chromium, Firefox, WebKit)
npx playwright install

# 5. Copy the environment file
cd "Playwright Tests"
copy .env.example .env
```

#### Verify Installation

```bash
cd "QA Tests"
npx playwright test home.spec.ts --project=chromium
```

Expected output:
```
Running 8 tests using 2 workers
  ✓ Home Page & Dashboard > should load home page (5.2s)
  ✓ Home Page & Dashboard > should display announcement banner (3.1s)
  ...
  8 passed (25.3s)
```

#### Running Playwright Tests

All commands run from the `QA Tests` folder:

| What You Want | Command |
|---|---|
| Run ALL tests (all browsers) | `npx playwright test` |
| Run ALL tests (Chrome only) | `npx playwright test --project=chromium` |
| Run ONE spec file | `npx playwright test partners.spec.ts --project=chromium` |
| Run with VISIBLE browser | `npx playwright test partners.spec.ts --project=chromium --headed` |
| Run ONE test by name | `npx playwright test -g "should display partner list" --project=chromium` |
| Open INTERACTIVE mode | `npx playwright test --ui` |
| See the HTML report | `npx playwright show-report TestResults/playwright-html-report` |
| Debug a test | `npx playwright test partners.spec.ts --debug` |
| Record a test | `npx playwright codegen http://localhost:4200` |

#### Project Structure

```
QA Tests/Playwright Tests/
├── *.spec.ts                    # Test spec files (one per feature)
├── pages/
│   ├── base.page.ts             # BasePage — all POMs extend this
│   └── *.page.ts                # Page Object Models (one per page)
├── helpers/
│   ├── auth.helper.ts           # Login helpers
│   ├── api-mocks.helper.ts      # API mock setup (route interception)
│   ├── wait.helper.ts           # Wait utilities
│   ├── assertions.helper.ts     # Custom assertion helpers
│   ├── test-data-builder.ts     # Test data factories
│   ├── test-config.ts           # Config and credentials
│   └── role-test.helper.ts      # Role-based test helpers
└── TestResults/                 # Generated reports and artifacts
    └── playwright-html-report/  # HTML report output
```

#### Key Configuration Facts

| Setting | Value |
|---|---|
| Base URL | `http://localhost:4200` |
| Test timeout | 60 seconds (WebKit: 180 seconds) |
| Workers (local) | 2 |
| Workers (CI) | 4 |
| Retries (local) | 0 |
| Retries (CI) | 1 |
| API mocking | Enabled by default — real backend is optional |

#### When Do You Need the Real Backend?

| Test Type | Backend Required? |
|---|---|
| Most Playwright E2E tests | **No** — API mocks handle all backend calls |
| Integration tests hitting real APIs | **Yes** — see [Section 3.5](#35-starting-the-development-proxy-for-real-backend-testing) |
| Performance/load tests | **Yes** — must hit real endpoints |

#### Using AI to Write Playwright Tests

Instead of writing specs from scratch, ask Claude in Cursor:

```
"Create a Playwright spec for the partner detail page that tests
viewing, editing, and deleting a partner. Use the existing
partner-item.page.ts POM."
```

Claude will:
1. Read existing POMs and helpers
2. Generate a complete `.spec.ts` file with proper mock setup
3. Follow project conventions (auth, waits, assertions)
4. Add `data-testid` selectors where needed

For a full guide on reading, writing, and debugging Playwright tests, see the dedicated quickstart:

> 📖 **`QA Tests/Playwright Tests/QUICKSTART_FOR_TESTERS.md`** — A comprehensive guide for testers transitioning from Katalon or manual testing to Playwright.

---

## 8. Manual vs Automated Testing Decision Guide

### 8.1 Decision Matrix

Use this matrix to decide whether to automate a test:

```
                         FREQUENCY OF EXECUTION
                    │   Rarely    │   Sometimes   │   Often
    ────────────────┼─────────────┼───────────────┼───────────────
    Low Complexity  │   Manual    │    Manual     │   Automate
    ────────────────┼─────────────┼───────────────┼───────────────
    Med Complexity  │   Manual    │   Consider    │   Automate
    ────────────────┼─────────────┼───────────────┼───────────────
    High Complexity │   Manual    │    Manual     │   Consider
```

### 8.2 When to Use Manual Testing

✅ **Automate When:**

| Scenario | Why Automate |
|----------|--------------|
| Regression testing | Runs every build, catches regressions early |
| Data-driven tests | Same test, many data variations |
| Cross-browser/device testing | Tedious to repeat manually |
| Performance testing | Requires precise timing and load generation |
| API testing | Fast, repeatable, easy to automate |
| Smoke tests | Quick build verification |
| Security scans | Automated tools find common vulnerabilities |

❌ **Keep Manual When:**

| Scenario | Why Manual |
|----------|------------|
| Exploratory testing | Requires human intuition and creativity |
| Usability testing | Subjective user experience evaluation |
| One-time tests | Not worth automation investment |
| Rapidly changing features | Tests would need constant updates |
| Visual/aesthetic testing | Humans judge design quality |
| Ad-hoc testing | Unscripted investigation |
| Complex setup scenarios | Too fragile to automate reliably |

### 8.3 Automation ROI Calculation

```
Break-even point = Automation Cost / (Manual Cost per Run × Runs per Year)

Example:
- Automation cost: 16 hours to write and maintain
- Manual execution: 1 hour per run
- Runs per year: 52 (weekly releases)

Break-even = 16 / (1 × 52) = 0.3 years ≈ 4 months

If you'll run the test for more than 4 months, automate it.
```

### 8.4 Test Pyramid

Follow the test pyramid for optimal coverage:

```
                    ┌─────────┐
                    │   E2E   │  10-20% (Slow, Expensive)
                    │  Tests  │  Automate critical paths only
                    ├─────────┤
                    │         │
                    │  Integ. │  20-30% (Medium speed)
                    │  Tests  │  API, database, service tests
                    ├─────────┤
                    │         │
                    │  Unit   │  60-70% (Fast, Cheap)
                    │  Tests  │  Logic, validation, utilities
                    └─────────┘
```

---

## 9. The 3:1 Test Ratio Standard

### 9.1 The Core Rule

> **For every positive test, create THREE times as many negative and edge case tests.**

This ensures failure scenarios receive MORE attention than happy paths.

### 9.2 Minimum Test Counts

#### Core 5 Categories (3:1 Ratio Participants)

| Category | File Name | Minimum Required | Formula |
|----------|-----------|------------------|---------|
| **Positive Tests** | `PositiveTests.cs` | 30-50 tests | Baseline (P) |
| **Negative Tests** | `NegativeTests.cs` | ≥50 AND ≥2×P | Max(50, 2×P) |
| **Boundary Tests** | `BoundaryTests.cs` | ≥50 AND ≥2×P | Max(50, 2×P) |
| **Functional Tests** | `FunctionalTests.cs` | ≥50 (FIXED) | Always 50+ — workflow rules(15), validation rules(15), constraint rules(10), audit rules(10) |
| **Integration Tests** | `IntegrationTests.cs` | ≥50 (FIXED) | Always 50+ — CRUD workflow(10), search/filter(10), pagination(5), relationships(10), error handling(15) |

#### Additional 5 Mandatory Categories

| Category | File Name | Minimum Required | Coverage Areas |
|----------|-----------|------------------|----------------|
| **Security/Validation** | `SecurityTests.cs` | ≥50 (FIXED) | OWASP Top 10, injection prevention, authorization, IDOR, mass assignment |
| **Concurrency** | `ConcurrencyTests.cs` | ≥25 (FIXED) | race conditions, deadlocks, double submit, transaction isolation, cache poisoning |
| **Unit Tests** | `UnitTests.cs` | ≥21 | validation(5), formatting(3), calculations(5), status logic(5), collections(3) |
| **Performance Tests** | `PerformanceTests.cs` | ≥16 | single ops(2), bulk ops(3), search(5), concurrent(3), memory(3) |
| **Load Tests** | `LoadTests.cs` | ≥10 | sustained(3), spike(2), stress(3), recovery(2) |

**Total Mandatory Files: 10** (5 core + 5 additional) | **Grand Total Minimum: ~347+ tests per suite**

### 9.3 Ratio Verification

```
REQUIREMENT: Each category individually ≥ 3 × Positive Tests
- Negative ≥ 3 × Positive
- Edge/Boundary ≥ 3 × Positive
- Functional ≥ 3 × Positive
- Integration ≥ 3 × Positive
```

#### Example: 85 Positive Tests

| Category | Count | Calculation | Check |
|----------|-------|-------------|-------|
| Positive | 85 | Baseline | - |
| Negative | 170 | Max(50, 2×85) = 170 | ✅ |
| Boundary | 170 | Max(50, 2×85) = 170 | ✅ |
| Functional | 50 | FIXED minimum (Core) | ✅ |
| Integration | 50 | FIXED minimum (Core) | ✅ |
| Security | 50 | FIXED minimum (Additional) | ✅ |
| Concurrency | 25 | FIXED minimum (Additional) | ✅ |
| Unit | 21 | Per coverage areas | ✅ |
| Performance | 16 | Per coverage areas | ✅ |
| Load | 10 | Per coverage areas | ✅ |
| **Total** | **647** | - | - |
| **3:1 Check** | - | N≥3P, E≥3P, F≥3P, I≥3P (each individually) | ✅ |

### 9.4 Category Checklist

All 10 mandatory test file categories must be present per suite.

#### 1. Positive Tests (`PositiveTests.cs`) — Happy Path
- [ ] Valid inputs with expected outputs
- [ ] Standard user workflows
- [ ] CRUD operations with valid data
- [ ] Successful authentication/authorization

#### 2. Negative Tests (`NegativeTests.cs`) — Failure Scenarios
- [ ] Boundary violations (string too long, number out of range)
- [ ] Invalid data types (text in numeric fields)
- [ ] Special characters & injection attempts
- [ ] Null/empty/missing required fields
- [ ] Collection stress (empty, oversized)
- [ ] Date paradoxes (future dates, invalid ranges)
- [ ] Dependency failures (API timeout, DB error)

#### 3. Boundary Tests (`BoundaryTests.cs`) — Edge Cases
- [ ] Financial precision (rounding, zero-sum)
- [ ] Temporal boundaries (fiscal year, leap year)
- [ ] Workflow state machine (illegal transitions)
- [ ] Threshold tests (exact limits, cumulative limits)
- [ ] Globalization (currency formats, multi-byte characters)

#### 4. Security Tests (`SecurityTests.cs`) — Security & Validation
- [ ] SQL Injection prevention
- [ ] XSS (Cross-Site Scripting) prevention
- [ ] IDOR (Insecure Direct Object Reference)
- [ ] Privilege escalation prevention
- [ ] Authentication bypass attempts
- [ ] OWASP Top 10 coverage

#### 5. Concurrency Tests (`ConcurrencyTests.cs`) — Race Conditions
- [ ] Concurrent updates to same entity
- [ ] Double submit prevention
- [ ] Read during write (transaction isolation)
- [ ] Deadlock scenarios
- [ ] Race conditions in counters/aggregates

#### 6. Unit Tests (`UnitTests.cs`) — Isolated Logic
- [ ] Input validation rules (min/max, required, format)
- [ ] Data formatting and transformation
- [ ] Calculations and business math
- [ ] Status/state transition logic
- [ ] Collection manipulation and filtering

#### 7. Functional Tests (`FunctionalTests.cs`) — Business Rules
- [ ] Workflow rules and multi-step processes
- [ ] Business validation rules (cross-field, conditional)
- [ ] Constraint enforcement (uniqueness, referential)
- [ ] Audit trail and history tracking

#### 8. Integration Tests (`IntegrationTests.cs`) — End-to-End Flows
- [ ] Full CRUD workflow (create, read, update, delete)
- [ ] Search and filter operations
- [ ] Pagination and sorting
- [ ] Entity relationship operations (parent-child, many-to-many)
- [ ] Error handling across layers (API → service → DB)

#### 9. Performance Tests (`PerformanceTests.cs`) — Speed & Efficiency
- [ ] Single operation response time thresholds
- [ ] Bulk operation performance (batch create/update)
- [ ] Search performance with large datasets
- [ ] Concurrent access performance
- [ ] Memory usage and resource consumption

#### 10. Load Tests (`LoadTests.cs`) — Scalability
- [ ] Sustained load (normal traffic over extended period)
- [ ] Spike load (sudden traffic burst)
- [ ] Stress limits (beyond expected capacity)
- [ ] Recovery behavior (after load subsides)

---

## 10. Test Categories Deep Dive

### 10.1 Positive Tests (Happy Path)

**Purpose**: Verify the system works correctly with valid inputs.

**Examples**:
```
✅ Create user with valid email and strong password
✅ Submit form with all required fields completed
✅ Process payment with valid card details
✅ Search with valid filter criteria
✅ Export report in supported format
```

**Test Pattern**:
```
Given: Valid preconditions
When: User performs expected action
Then: System responds with expected success
```

### 10.2 Negative Tests (Failure Scenarios)

**Purpose**: Verify the system handles invalid inputs gracefully.

**The Three C's Framework**: Target **Crashes, Corruption, and Compliance**.

#### Input Validation Tests

| Test Type | Examples |
|-----------|----------|
| **Boundary Values** | 51 chars in 50-char field, -1 for positive numbers |
| **Invalid Types** | "abc" in numeric field, "not-a-date" in date field |
| **Special Characters** | `<script>`, `'; DROP TABLE --`, `../../etc/passwd` |
| **Malformed Input** | Invalid JSON, missing closing brackets |

#### Null Reference Tests

```
Test with null for every parameter that accepts reference types.
This is the #1 cause of NullReferenceException.
```

#### Dependency Failure Tests

```
Simulate:
- Database timeout
- API returns 503
- Network drop mid-upload
- Third-party service failure
```

#### Combinatorial Testing: Value Permutations

When testing multiple input fields, systematically combine values from **invalid value categories** to ensure comprehensive coverage.

##### Standard Invalid Value Categories

| Category | Examples | Use For |
|----------|----------|---------|
| **Null** | `null` | All nullable reference types |
| **Empty** | `""`, `[]`, `{}` | Strings, collections, objects |
| **Whitespace** | `"   "`, `"\t"`, `"\n"` | String fields |
| **Boundary Min-1** | `-1`, `0` (if min is 1) | Numeric fields |
| **Boundary Max+1** | `101` (if max is 100) | Numeric fields, string lengths |
| **Invalid Format** | `"abc"` for number, `"99/99/9999"` for date | Typed fields |
| **Special Characters** | `<script>`, `'; DROP`, `..\..\` | Text inputs |
| **Unicode/Multi-byte** | `"日本語"`, `"Ñoño"`, emojis | Text inputs |
| **Very Long** | 10,000+ characters | String fields |
| **Negative** | `-100`, `-0.01` | Unsigned numeric fields |

##### Pairwise Testing Strategy

For methods with multiple parameters, use **pairwise (all-pairs) testing** to reduce test count while maintaining coverage:

```
Example: Method with 3 parameters, each with 4 invalid states
- Full combinatorial: 4 × 4 × 4 = 64 tests ❌ (too many)
- Pairwise: ~16 tests ✅ (covers all pairs of values)
```

##### Data-Driven Test Patterns (xUnit)

**Pattern 1: InlineData for Small Value Sets**
```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
[InlineData("a")]  // Below minimum length
public async Task CreateEntity_InvalidName_ShouldThrowValidation(string? name)
{
    var request = new CreateRequest { Name = name };
    await Assert.ThrowsAsync<ValidationException>(
        () => _manager.CreateAsync(request));
}
```

**Pattern 2: MemberData for Complex Value Sets**
```csharp
public static IEnumerable<object[]> InvalidEmailTestData =>
    new List<object[]>
    {
        new object[] { null, "Email is required" },
        new object[] { "", "Email is required" },
        new object[] { "not-an-email", "Invalid email format" },
        new object[] { "missing@domain", "Invalid email format" },
        new object[] { "@nodomain.com", "Invalid email format" },
        new object[] { "spaces in@email.com", "Invalid email format" },
        new object[] { new string('a', 256) + "@test.com", "Email too long" },
    };

[Theory]
[MemberData(nameof(InvalidEmailTestData))]
public async Task Validate_InvalidEmail_ReturnsExpectedError(string? email, string expectedError)
{
    var result = await _validator.ValidateAsync(new Request { Email = email });
    Assert.Contains(expectedError, result.Errors.First().Message);
}
```

**Pattern 3: ClassData for Reusable Test Data**
```csharp
public class InvalidStringTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { null };
        yield return new object[] { "" };
        yield return new object[] { "   " };
        yield return new object[] { "\t\n" };
        yield return new object[] { new string('x', 10001) };  // Over max length
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(InvalidStringTestData))]
public async Task ValidateField_InvalidString_ShouldReject(string? value)
{
    // Test implementation
}
```

##### Multi-Field Permutation Example

When testing a method with multiple parameters, combine invalid values systematically:

```csharp
public static IEnumerable<object[]> CreateEntityInvalidCombinations()
{
    // Each field's invalid values
    var invalidNames = new[] { null, "", "   ", new string('a', 256) };
    var invalidEmails = new[] { null, "", "invalid", "no@domain" };
    var invalidAmounts = new[] { -1m, 0m, 1000001m };  // Outside valid range
    
    // Test each field independently with valid values for others
    foreach (var name in invalidNames)
        yield return new object[] { name, "valid@email.com", 100m, "Name" };
    
    foreach (var email in invalidEmails)
        yield return new object[] { "Valid Name", email, 100m, "Email" };
    
    foreach (var amount in invalidAmounts)
        yield return new object[] { "Valid Name", "valid@email.com", amount, "Amount" };
    
    // Key combinations (pairwise - invalid name + invalid email)
    yield return new object[] { null, null, 100m, "Name, Email" };
    yield return new object[] { "", "invalid", 100m, "Name, Email" };
}

[Theory]
[MemberData(nameof(CreateEntityInvalidCombinations))]
public async Task Create_InvalidInput_ShouldValidate(
    string? name, string? email, decimal amount, string expectedInvalidField)
{
    var request = new CreateRequest { Name = name, Email = email, Amount = amount };
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => _manager.CreateAsync(request));
    Assert.Contains(expectedInvalidField, exception.Message);
}
```

##### Coverage Checklist for Negative Value Permutations

For each input field, verify you have tests covering:

- [ ] `null` value
- [ ] Empty value (`""`, `[]`, `{}`)
- [ ] Whitespace-only (`"   "`, `"\t"`, `"\n"`)
- [ ] Below minimum length/value
- [ ] Above maximum length/value
- [ ] Invalid format/type
- [ ] Special characters (XSS, SQL injection)
- [ ] Unicode/multi-byte characters
- [ ] Very large values (stress test)
- [ ] Negative values (if unsigned expected)

For multi-field inputs, also verify:
- [ ] All fields null/empty simultaneously
- [ ] Pairwise combinations of invalid values
- [ ] One invalid field with all others valid (isolation)

### 10.3 Edge Cases (Boundary Conditions)

**Purpose**: Test unusual but valid scenarios at the edges of normal operation.

#### Financial Boundaries

| Test | Why It Matters |
|------|----------------|
| **Half-cent rounding** ($1.005) | Banker's rounding vs. standard rounding |
| **Zero-sum splits** ($100 ÷ 3) | Ghost penny remaining |
| **Extreme values** (1M units × $0.0001) | Precision and overflow |

#### Temporal Boundaries

| Test | Why It Matters |
|------|----------------|
| **Fiscal year rollover** (last second vs first second) | Correct year assignment |
| **Leap year dates** (Feb 29) | Date calculation accuracy |
| **Backdating** (received before ordered) | Business rule enforcement |

#### Workflow State Machine

| Test | Why It Matters |
|------|----------------|
| **Double-submit race condition** | Prevent duplicate records |
| **Impossible transitions** (Cancelled → Paid) | State machine integrity |
| **Out-of-order deletion** (vendor with active contracts) | Referential integrity |

### 10.4 Security Tests

**Purpose**: Verify the application is resistant to common attacks.

#### OWASP Top 10 Coverage

| Vulnerability | Test Approach |
|---------------|---------------|
| **A01: Broken Access Control** | Access resources without authorization |
| **A02: Cryptographic Failures** | Sensitive data exposure, weak encryption |
| **A03: Injection** | SQL, NoSQL, OS command injection |
| **A07: Cross-Site Scripting** | Reflected, stored, DOM-based XSS |

#### Security Test Examples

```
Input: '; DROP TABLE Users; --
Expected: Safely stored as string, NOT executed as SQL

Input: <script>alert('XSS')</script>
Expected: Encoded/sanitized, NOT executed in browser

Action: Access /api/users/123 without token
Expected: 401 Unauthorized, NOT user data
```

### 10.5 Concurrency Tests

**Purpose**: Verify the system handles simultaneous operations correctly.

#### Common Concurrency Issues

| Issue | Test Approach |
|-------|---------------|
| **Race conditions** | Fire two identical requests simultaneously |
| **Lost updates** | Two users update same record |
| **Deadlocks** | Multiple resources locked in conflicting order |
| **Counter corruption** | Increment counter from multiple threads |

#### Concurrency Test Pattern

```csharp
// Simulate double-click
var task1 = service.CreateAsync(request);
var task2 = service.CreateAsync(request);
await Task.WhenAll(task1, task2);

// Verify only ONE record created
var count = await repository.CountAsync();
Assert.Equal(1, count);
```

### 10.6 Unit Tests (Isolated Logic)

**Purpose**: Test individual methods and logic in isolation, without dependencies on databases, APIs, or other services.

#### Coverage Areas

| Area | Examples |
|------|----------|
| **Input validation** | Required field checks, format validation, min/max constraints |
| **Data formatting** | Date formatting, number formatting, string transformations |
| **Calculations** | Business math, totals, percentages, rounding |
| **Status/state logic** | Allowed transitions, status-dependent behavior |
| **Collection operations** | Filtering, grouping, sorting, aggregation |

#### Unit Test Pattern

```csharp
// Pure logic test — no database, no API
[Fact]
public void CalculateTotalBudget_WithMultipleLineItems_ReturnsSumOfAmounts()
{
    var lineItems = new List<BudgetLine>
    {
        new() { Amount = 1000m },
        new() { Amount = 2500m },
        new() { Amount = 750m }
    };

    var total = BudgetCalculator.CalculateTotal(lineItems);

    Assert.Equal(4250m, total);
}
```

### 10.7 Functional Tests (Business Rules)

**Purpose**: Verify that business rules, workflow logic, and domain constraints are enforced correctly.

#### Coverage Areas

| Area | Examples |
|------|----------|
| **Workflow rules** | Multi-step process enforcement, approval chains |
| **Validation rules** | Cross-field validation, conditional required fields |
| **Constraint rules** | Uniqueness, referential integrity, business invariants |
| **Audit rules** | History tracking, change logging, timestamps |

#### Functional Test Pattern

```csharp
// Business rule: Cannot close verification without answering all questions
[Fact]
public async Task CloseVerification_WithUnansweredQuestions_ShouldRejectClosure()
{
    // Arrange: Create verification with unanswered questions
    var verification = await CreateVerificationWithQuestions(answered: false);

    // Act & Assert: Business rule prevents closure
    var exception = await Assert.ThrowsAsync<BusinessRuleException>(
        () => _manager.CloseVerificationAsync(verification.Id));

    Assert.Contains("unanswered questions", exception.Message);
}
```

### 10.8 Integration Tests (End-to-End Flows)

**Purpose**: Test complete workflows across multiple layers (API, service, database) to verify components work together correctly.

#### Coverage Areas

| Area | Examples |
|------|----------|
| **CRUD workflows** | Create → Read → Update → Delete lifecycle |
| **Search & filter** | Query with various filter combinations |
| **Pagination** | Page size, page number, total count accuracy |
| **Relationships** | Parent-child creation, cascade operations |
| **Error handling** | Error propagation across layers, proper HTTP status codes |

#### Integration Test Pattern

```csharp
// Full CRUD lifecycle
[Fact]
public async Task Verification_FullCrudLifecycle_ShouldSucceed()
{
    // Create
    var created = await _manager.CreateAsync(validRequest);
    Assert.NotNull(created);

    // Read
    var retrieved = await _manager.GetByIdAsync(created.Id);
    Assert.Equal(created.Id, retrieved.Id);

    // Update
    retrieved.Title = "Updated Title";
    var updated = await _manager.UpdateAsync(retrieved);
    Assert.Equal("Updated Title", updated.Title);

    // Delete
    await _manager.DeleteAsync(updated.Id);
    var deleted = await _manager.GetByIdAsync(updated.Id);
    Assert.Null(deleted);
}
```

### 10.9 Performance Tests (Speed & Efficiency)

**Purpose**: Verify that operations complete within acceptable time thresholds and resource limits.

#### Coverage Areas

| Area | Examples |
|------|----------|
| **Single operations** | Individual CRUD under time threshold |
| **Bulk operations** | Batch create/update/delete performance |
| **Search performance** | Query time with large datasets |
| **Concurrent access** | Performance under parallel requests |
| **Memory usage** | No excessive allocations or leaks |

#### Performance Test Pattern

```csharp
[Fact]
public async Task BulkCreate_100Records_ShouldCompleteWithin5Seconds()
{
    var stopwatch = Stopwatch.StartNew();

    var requests = Enumerable.Range(1, 100)
        .Select(i => CreateValidRequest($"Item-{i}"))
        .ToList();

    await _manager.BulkCreateAsync(requests);

    stopwatch.Stop();
    Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5),
        $"Bulk create took {stopwatch.Elapsed.TotalSeconds}s, expected < 5s");
}
```

### 10.10 Load Tests (Scalability)

**Purpose**: Verify the system maintains stability and acceptable performance under sustained, peak, and beyond-capacity loads.

#### Coverage Areas

| Area | Examples |
|------|----------|
| **Sustained load** | Normal expected traffic over 5-10 minutes |
| **Spike load** | Sudden burst of requests (e.g., 10x normal) |
| **Stress limits** | Push beyond expected capacity to find breaking point |
| **Recovery** | System returns to normal after load subsides |

#### Load Test Pattern

```csharp
[Fact]
public async Task SustainedLoad_50ConcurrentUsers_ShouldMaintainResponseTimes()
{
    var tasks = Enumerable.Range(1, 50).Select(async i =>
    {
        var sw = Stopwatch.StartNew();
        for (int j = 0; j < 10; j++) // 10 operations per "user"
        {
            await _manager.GetByIdAsync(existingId);
        }
        sw.Stop();
        return sw.Elapsed;
    });

    var results = await Task.WhenAll(tasks);

    var avgTime = results.Average(r => r.TotalMilliseconds);
    var maxTime = results.Max(r => r.TotalMilliseconds);

    Assert.True(avgTime < 2000, $"Average response: {avgTime}ms, expected < 2000ms");
    Assert.True(maxTime < 10000, $"Max response: {maxTime}ms, expected < 10000ms");
}
```

---

## 11. Manual Test Case Templates

### 11.1 Standard Test Case Template

```markdown
## Test Case: [TC-XXX] [Test Name]

**Feature:** [Feature/Module Name]
**Requirement:** [JIRA/Requirement ID]
**Priority:** [High/Medium/Low]
**Type:** [Positive/Negative/Edge Case/Security]

### Preconditions
- [ ] User is logged in as [ROLE]
- [ ] Test data exists: [DATA REQUIREMENTS]
- [ ] Environment: [TEST/STAGING]

### Test Steps

| Step | Action | Expected Result | Pass/Fail | Notes |
|------|--------|-----------------|-----------|-------|
| 1 | Navigate to [URL/SCREEN] | Page loads successfully | | |
| 2 | Enter [VALUE] in [FIELD] | Value is accepted | | |
| 3 | Click [BUTTON] | [EXPECTED BEHAVIOR] | | |
| 4 | Verify [CONDITION] | [EXPECTED STATE] | | |

### Test Data
- Field 1: [VALUE]
- Field 2: [VALUE]

### Expected Result
[Overall expected outcome]

### Actual Result
[Fill during execution]

### Status
☐ Pass ☐ Fail ☐ Blocked ☐ Skipped

### Attachments
- Screenshot: [LINK]
- Defect: [DEFECT-ID if failed]
```

### 11.2 Validation Test Case Template

```markdown
## Validation Test Case: [TC-XXX] [Field] Validation

**Field:** [Field Name]
**Constraints:** [MIN]-[MAX] chars, [ALLOWED CHARACTERS]
**Type:** Input Validation

### Test Data Matrix

| Input | Type | Expected Result | Pass/Fail |
|-------|------|-----------------|-----------|
| (empty) | Missing required | "Field is required" error | |
| "a" | Below minimum (1 char) | "Minimum X characters" error | |
| "abc" | At minimum (3 chars) | Accepted ✅ | |
| [20 chars] | At maximum | Accepted ✅ | |
| [21 chars] | Above maximum | "Maximum 20 characters" error | |
| "Test<script>" | XSS attempt | Sanitized or rejected | |
| "Test'; DROP" | SQL injection | Safely handled | |
| "  Test  " | Leading/trailing spaces | Trimmed or rejected | |
| "Tëst Üsér" | Special characters | Handled per requirements | |
```

### 11.3 Integration Test Checklist Template

Use this comprehensive template for feature integration testing. Customize categories based on your feature requirements.

```markdown
# [Feature Name] - Integration Testing Checklist

**PRD Reference:** [document-name.md]
**JIRA ID:** [JIRA-XXX]
**Created:** [Date]
**Status:** Ready for Testing

---

## Overview

This checklist provides comprehensive testing coverage for [Feature Name]. 
All tests should be performed manually in a test environment before production deployment.

**Testing Prerequisites:**
- [ ] Backend API server running
- [ ] Frontend application running
- [ ] Database accessible with test data
- [ ] Test user with [REQUIRED_ROLE] role
- [ ] Required test data seeded (e.g., related entities, lookup data)

---

## 1. Complete Workflow Testing

### TC-1.1: Primary Workflow (Happy Path)

**Objective:** Verify complete workflow from start to finish

**Prerequisites:**
- Required related entities exist
- User has appropriate permissions

**Test Steps:**
1. Navigate to [starting page/URL]
2. Verify initial state displays correctly
3. Perform [primary action]
4. Verify [intermediate result]
5. Complete [workflow steps]
6. Verify [final result]

**Expected Results:**
- [ ] Entity created/updated successfully in database
- [ ] Status = [Expected status]
- [ ] All related records created correctly
- [ ] Audit fields populated (CreatedBy, CreatedDate)
- [ ] Success message displayed
- [ ] Navigation works correctly

**Pass Criteria:** All steps complete without errors

---

### TC-1.2: Alternative Workflow

**Objective:** Verify alternative path through the feature

**Test Steps:**
1. [Alternative starting point]
2. [Different path steps]
3. [Alternative completion]

**Expected Results:**
- [ ] Alternative flow completes successfully
- [ ] No conflicts with primary workflow

---

## 2. Validation Rules Testing

### TC-2.1: Required Field Validation

**Objective:** Verify required fields are enforced

| Field | Test Input | Expected Error |
|-------|------------|----------------|
| Name | (empty) | "Name is required" |
| Name | "   " (whitespace) | "Name is required" |
| Email | (empty) | "Email is required" |

---

### TC-2.2: Format Validation

**Objective:** Verify format constraints

| Field | Test Input | Expected Error |
|-------|------------|----------------|
| Email | "not-an-email" | "Invalid email format" |
| Phone | "abc123" | "Invalid phone format" |
| Date | "99/99/9999" | "Invalid date" |

---

### TC-2.3: Length Validation

**Objective:** Verify length constraints

| Field | Max Length | Test Input | Expected |
|-------|------------|------------|----------|
| Name | 255 | 255 chars | Accepted |
| Name | 255 | 256 chars | "Maximum 255 characters" |

---

### TC-2.4: Uniqueness Validation

**Objective:** Verify duplicate detection

**Test Steps:**
1. Create entity with name "Test Entity"
2. Try to create another with name "Test Entity"
3. Try to create with name "test entity" (case insensitive)

**Expected Results:**
- [ ] Exact match rejected with clear error
- [ ] Case-insensitive match rejected
- [ ] Unique names accepted

---

### TC-2.5: Business Rule Validation

**Objective:** Verify business logic constraints

| Rule | Test Scenario | Expected |
|------|---------------|----------|
| Only published forms allowed | Select draft form | Error displayed |
| End date after start date | End before start | Error displayed |
| Minimum items required | Submit with 0 items | Error displayed |

---

## 3. Status Transition Testing

### TC-3.1: Valid Status Transitions

| From Status | To Status | Expected |
|-------------|-----------|----------|
| Draft | Active | ✅ Allowed |
| Active | Inactive | ✅ Allowed |
| Inactive | Active | ✅ Allowed |

**Test Steps:**
1. Create entity in [initial status]
2. Change status to [target status]
3. Verify status updates in UI
4. Verify status updates in database
5. Verify audit trail updated

---

### TC-3.2: Invalid Status Transitions

| From Status | To Status | Expected |
|-------------|-----------|----------|
| Draft | Inactive | ❌ Rejected |
| Cancelled | Active | ❌ Rejected |
| Deprecated | Any | ❌ Rejected (read-only) |

**Expected Results:**
- [ ] Invalid transitions prevented
- [ ] Clear error message displayed
- [ ] No database changes made

---

## 4. Delete/Archive Functionality

### TC-4.1: Delete Allowed Entities

**Objective:** Verify deletion works for eligible entities

**Test Steps:**
1. Create entity in Draft status
2. Verify Delete button visible
3. Click Delete
4. Verify confirmation dialog
5. Confirm deletion
6. Verify soft delete in database (IsDeleted = true)
7. Verify cascading delete to child records

---

### TC-4.2: Delete Prevented for Protected Entities

**Objective:** Verify deletion blocked for active/protected entities

**Test Steps:**
1. Create entity and set status to Active
2. Verify Delete button NOT visible
3. Attempt API delete directly
4. Verify rejection with error message

---

## 5. Filtering and Pagination

### TC-5.1: Name Search Filter

**Test Data:** Multiple entities with varying names

| Search Term | Expected Results |
|-------------|------------------|
| "core" | All containing "Core", "CORE", "core" |
| "test" | Only matching entities |
| (clear) | All entities |

---

### TC-5.2: Status Filter

| Filter | Expected |
|--------|----------|
| Draft | Only Draft entities |
| Active | Only Active entities |
| (all) | All entities |

---

### TC-5.3: Pagination

**Prerequisites:** 30+ entities

| Test | Expected |
|------|----------|
| Default page size | 10 items |
| Navigate to page 2 | Next 10 items |
| Change to 20 per page | 20 items shown |
| Total count | Accurate count |

---

## 6. Inline Editing / Configuration

### TC-6.1: Edit Field Inline

**Test Steps:**
1. Navigate to entity detail/configuration
2. Click edit icon next to [field]
3. Change value
4. Click save (check button)
5. Verify success message
6. Verify value updated in UI
7. Verify value persisted in database
8. Verify ModifiedBy/ModifiedDate updated

---

### TC-6.2: Cancel Edit

**Test Steps:**
1. Begin editing field
2. Change value
3. Click cancel (X button)
4. Verify original value restored
5. Verify no API call made

---

### TC-6.3: Validation During Edit

**Test Steps:**
1. Begin editing
2. Enter invalid value (empty, too long, duplicate)
3. Attempt save
4. Verify inline error message
5. Verify save blocked

---

## 7. Referential Integrity

### TC-7.1: Foreign Key Relationships

**Objective:** Verify all relationships are valid

| Parent | Child | Test |
|--------|-------|------|
| Entity → Related1 | Query child.ParentId | All reference valid parent |
| Entity → Related2 | Query child.EntityId | All reference valid entity |

---

### TC-7.2: Cascading Relationships

**Objective:** Verify relationship chain integrity

**Test Steps:**
1. Create entity with children
2. Trace relationship chain through all levels
3. Verify all references valid
4. Verify no orphaned records

---

## 8. Audit Trail Verification

### TC-8.1: Creation Audit

| Field | Expected |
|-------|----------|
| CreatedBy | Current user ID |
| CreatedDate | Current timestamp (±1 min) |
| ModifiedBy | Same as CreatedBy |
| ModifiedDate | Same as CreatedDate |

---

### TC-8.2: Modification Audit

**Test Steps:**
1. Create entity as User A
2. Modify entity as User B
3. Query audit fields

| Field | Expected |
|-------|----------|
| CreatedBy | User A (unchanged) |
| CreatedDate | Original (unchanged) |
| ModifiedBy | User B (updated) |
| ModifiedDate | Current timestamp |

---

## 9. Error Handling

### TC-9.1: API Connection Error

**Objective:** Verify graceful handling of API failures

**Test Steps:**
1. Stop backend API
2. Attempt operation
3. Verify user-friendly error message
4. Verify app doesn't crash

---

### TC-9.2: Concurrent Modification

**Test Steps:**
1. User A opens entity
2. User B opens same entity
3. User A saves changes
4. User B attempts save
5. Verify appropriate handling (conflict message or last-write-wins)

---

### TC-9.3: Success/Error Messages

| Operation | Expected Message |
|-----------|------------------|
| Create | "Created successfully" |
| Update | "Updated successfully" |
| Delete | "Deleted successfully" |
| Validation error | Clear description of issue |

---

## 10. End-to-End Workflow

### TC-10.1: Complete User Journey

**Objective:** Verify full workflow from start to finish

**Scenario:** [Describe complete user story]

**Test Steps:**
1. Log in as [role]
2. Navigate to feature
3. Create new entity
4. Configure properties
5. Change status
6. Verify in list view
7. Create related entity
8. Return to dashboard
9. Verify all data persisted

---

### TC-10.2: Data Consistency

**Test Steps:**
1. Perform operations via UI
2. Query database directly
3. Compare UI data with database
4. Verify exact match

---

### TC-10.3: Performance

| Operation | Target | Actual |
|-----------|--------|--------|
| List load (100 items) | < 2 sec | |
| Create complex entity | < 5 sec | |
| Filter response | < 1 sec | |
| Update single field | < 1 sec | |

---

## Test Results Summary

**Test Date:** _______________
**Tester Name:** _______________
**Environment:** _______________

### Results by Category

| Category | Total | Passed | Failed | Blocked |
|----------|-------|--------|--------|---------|
| 1. Workflow | | | | |
| 2. Validation | | | | |
| 3. Status | | | | |
| 4. Delete | | | | |
| 5. Filtering | | | | |
| 6. Editing | | | | |
| 7. Referential | | | | |
| 8. Audit | | | | |
| 9. Error Handling | | | | |
| 10. E2E | | | | |
| **TOTAL** | | | | |

**Pass Rate:** ___/___  = ___%

### Critical Issues Found

| ID | Description | Severity |
|----|-------------|----------|
| | | |

### Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Tester | | | |
| Reviewer | | | |
| Approved | ☐ Yes ☐ No | | |
```

---

## 12. Automated Test Templates

### 12.1 Unit Test Template (C#/xUnit)

```csharp
namespace ProjectName.Tests.[ModuleName].[JiraId]_[FeatureName]
{
    using System;
    using System.Threading.Tasks;
    using Xunit;
    
    /// <summary>
    /// [JIRA-XXX]: Unit tests for [Feature Name]
    /// Tests core logic and validation
    /// </summary>
    public sealed class UnitTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        
        public UnitTests(TestFixture fixture)
        {
            _fixture = fixture;
        }
        
        #region Positive Tests
        
        [Fact]
        public async Task MethodName_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var input = CreateValidInput();
            
            // Act
            var result = await _fixture.Service.MethodAsync(input);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.Property);
        }
        
        #endregion
        
        #region Negative Tests
        
        [Fact]
        public async Task MethodName_NullInput_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _fixture.Service.MethodAsync(null));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task MethodName_InvalidString_ThrowsValidationException(string input)
        {
            // Arrange
            var request = new Request { Name = input };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _fixture.Service.MethodAsync(request));
        }
        
        #endregion
        
        #region Edge Cases
        
        [Theory]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task MethodName_BoundaryValue_HandlesCorrectly(int value)
        {
            // Arrange
            var request = new Request { Value = value };
            
            // Act
            var exception = await Record.ExceptionAsync(
                () => _fixture.Service.MethodAsync(request));
            
            // Assert
            // Document expected behavior for each boundary
        }
        
        #endregion
    }
}
```

### 12.2 Integration Test Template (C#/xUnit)

```csharp
namespace ProjectName.Tests.[ModuleName].[JiraId]_[FeatureName]
{
    /// <summary>
    /// [JIRA-XXX]: Integration tests for [Feature Name]
    /// Tests API endpoints and database integration
    /// </summary>
    public sealed class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        
        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task CreateEntity_ValidRequest_PersistsToDatabase()
        {
            // Arrange
            var request = new CreateRequest { /* valid data */ };
            
            // Act
            var result = await _fixture.ApiClient.CreateAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            
            // Verify persistence
            var saved = await _fixture.Repository.GetByIdAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal(request.Name, saved.Name);
        }
        
        [Fact]
        public async Task CreateEntity_DuplicateName_ReturnsConflict()
        {
            // Arrange
            await CreateEntityWithName("Existing Name");
            var duplicateRequest = new CreateRequest { Name = "Existing Name" };
            
            // Act
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _fixture.ApiClient.CreateAsync(duplicateRequest));
            
            // Assert
            Assert.Contains("already exists", exception.Message);
        }
    }
}
```

### 12.3 E2E Test Template (Playwright/TypeScript)

```typescript
import { test, expect, Page } from '@playwright/test';

/**
 * JIRA-XXX: End-to-End Tests for [Feature Name]
 */
test.describe('[Feature Name] E2E Tests', () => {
  let page: Page;

  test.beforeEach(async ({ page: testPage }) => {
    page = testPage;
    // Navigate and authenticate
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('/dashboard');
  });

  test.describe('Positive Scenarios', () => {
    test('should complete primary workflow successfully', async () => {
      // Arrange
      await page.goto('/feature-page');
      
      // Act
      await page.fill('[data-testid="name-input"]', 'Test Name');
      await page.click('[data-testid="submit-button"]');
      
      // Assert
      await expect(page.locator('[data-testid="success-message"]'))
        .toBeVisible();
      await expect(page.locator('[data-testid="success-message"]'))
        .toContainText('Created successfully');
    });
  });

  test.describe('Negative Scenarios', () => {
    test('should show validation error for empty required field', async () => {
      // Arrange
      await page.goto('/feature-page');
      
      // Act - submit without filling required field
      await page.click('[data-testid="submit-button"]');
      
      // Assert
      await expect(page.locator('[data-testid="name-error"]'))
        .toContainText('Name is required');
    });
  });

  test.describe('Edge Cases', () => {
    test('should handle maximum length input', async () => {
      const maxLengthInput = 'A'.repeat(255);
      
      await page.goto('/feature-page');
      await page.fill('[data-testid="name-input"]', maxLengthInput);
      await page.click('[data-testid="submit-button"]');
      
      // Should succeed with max length
      await expect(page.locator('[data-testid="success-message"]'))
        .toBeVisible();
    });
  });
});
```

---

## 13. Test Execution Reporting

### 13.1 Daily Test Execution Report Template

```markdown
# Daily Test Execution Report

**Date:** YYYY-MM-DD
**Sprint:** [Sprint Name/Number]
**Tester:** [Name]

## Summary

| Metric | Count | Percentage |
|--------|-------|------------|
| **Total Test Cases** | | 100% |
| **Executed** | | % |
| **Passed** | | % |
| **Failed** | | % |
| **Blocked** | | % |
| **Skipped** | | % |

## New Defects Found Today

| ID | Title | Severity | Status |
|----|-------|----------|--------|
| DEF-XXX | [Title] | 🔴 Critical | Open |
| DEF-XXX | [Title] | 🟠 High | Open |

## Blockers

| Issue | Impact | Mitigation |
|-------|--------|------------|
| [Description] | [Which tests blocked] | [Action taken] |

## Tomorrow's Plan

- [ ] Complete [X] test cases
- [ ] Retest [X] fixed defects
- [ ] [Other activities]

## Notes

[Any additional observations or concerns]
```

### 13.2 Final Test Summary Report Template

```markdown
# Test Summary Report

**Project:** [Project Name]
**Release:** [Version/Release Name]
**Test Period:** YYYY-MM-DD to YYYY-MM-DD
**Prepared By:** [Name]

## Executive Summary

[2-3 sentence overview of testing activities and overall quality assessment]

## Test Scope

### In Scope
- [Feature 1]
- [Feature 2]

### Out of Scope
- [Feature X]

## Test Results

### Overall Statistics

| Metric | Count | Target | Status |
|--------|-------|--------|--------|
| Total Tests | | | |
| Passed | | 95%+ | ✅/❌ |
| Failed | | <5% | ✅/❌ |
| Blocked | | 0% | ✅/❌ |
| Pass Rate | | 95%+ | ✅/❌ |

### By Test Type

| Type | Total | Passed | Failed | Pass Rate |
|------|-------|--------|--------|-----------|
| Smoke | | | | % |
| Functional | | | | % |
| Regression | | | | % |
| Integration | | | | % |
| E2E | | | | % |

## Defect Summary

### By Severity

| Severity | Found | Fixed | Open | Deferred |
|----------|-------|-------|------|----------|
| 🔴 Critical | | | | |
| 🟠 High | | | | |
| 🟡 Medium | | | | |
| 🟢 Low | | | | |

### Open Defects

| ID | Title | Severity | Status | Assigned |
|----|-------|----------|--------|----------|
| DEF-XXX | [Title] | [Level] | [Status] | [Name] |

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| [Risk description] | H/M/L | H/M/L | [Action] |

## Recommendations

1. **Release Decision:** ☐ Recommend Release ☐ Do Not Recommend
2. **Conditions:** [Any conditions for release]
3. **Known Issues:** [Issues going to production]

## Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| QA Lead | | | |
| Dev Lead | | | |
| Product Owner | | | |
```

---

## 14. Quick Reference Cards

### 14.1 Test Type Quick Reference

```
┌─────────────────────────────────────────────────────────────────┐
│                    TEST TYPE QUICK REFERENCE                     │
├──────────────┬───────────────────────────────────────────────────┤
│ Unit Tests   │ Individual functions, classes, methods            │
│              │ Fast, isolated, no external dependencies          │
│              │ Run: Every commit, every build                    │
├──────────────┼───────────────────────────────────────────────────┤
│ Integration  │ API endpoints, database operations                │
│ Tests        │ Medium speed, requires test database              │
│              │ Run: Every PR, daily builds                       │
├──────────────┼───────────────────────────────────────────────────┤
│ E2E Tests    │ Full user workflows through UI                    │
│              │ Slow, requires full environment                   │
│              │ Run: Nightly, before release                      │
├──────────────┼───────────────────────────────────────────────────┤
│ Smoke Tests  │ Critical paths only, quick sanity check           │
│              │ Fast, subset of regression                        │
│              │ Run: After every deployment                       │
├──────────────┼───────────────────────────────────────────────────┤
│ Regression   │ All existing functionality still works            │
│ Tests        │ Full suite, may be lengthy                        │
│              │ Run: Before release, major changes                │
├──────────────┼───────────────────────────────────────────────────┤
│ Performance  │ Load, stress, response time                       │
│ Tests        │ Requires production-like environment              │
│              │ Run: Before major releases                        │
└──────────────┴───────────────────────────────────────────────────┘
```

### 14.2 Severity Classification Quick Reference

```
┌─────────────────────────────────────────────────────────────────┐
│              SEVERITY CLASSIFICATION GUIDE                       │
├──────────┬───────────────────────────────────────────────────────┤
│          │ • System crash, data loss, data corruption           │
│ CRITICAL │ • Security vulnerability exposed                     │
│    🔴    │ • No workaround available                            │
│          │ • Blocks release                                      │
├──────────┼───────────────────────────────────────────────────────┤
│          │ • Major feature completely broken                    │
│   HIGH   │ • Significant user impact                            │
│    🟠    │ • No reasonable workaround                           │
│          │ • Should block release                               │
├──────────┼───────────────────────────────────────────────────────┤
│          │ • Feature partially working                          │
│  MEDIUM  │ • Workaround available                               │
│    🟡    │ • Moderate user impact                               │
│          │ • Fix before next release                            │
├──────────┼───────────────────────────────────────────────────────┤
│          │ • Minor cosmetic issue                               │
│   LOW    │ • Typo, alignment, minor UI                          │
│    🟢    │ • Minimal user impact                                │
│          │ • Fix when convenient                                 │
└──────────┴───────────────────────────────────────────────────────┘
```

### 14.3 3:1 Ratio Quick Calculator

```
┌─────────────────────────────────────────────────────────────────┐
│                   3:1 RATIO CALCULATOR                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  If you have [P] Positive Tests:                                │
│                                                                  │
│  CORE CATEGORIES:                                                │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ Negative Tests:    MAX(50, 2 × P) = ____               │    │
│  │ Edge Case Tests:   MAX(50, 2 × P) = ____               │    │
│  │ Functional Tests:  50 (FIXED)                           │    │
│  │ Integration Tests: 50 (FIXED)                           │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ADDITIONAL CATEGORIES:                                          │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ Security Tests:  50 (FIXED)                             │    │
│  │ Concurrency:     25 (FIXED)                             │    │
│  │ Unit Tests:      21 (FIXED)                             │    │
│  │ Performance:     16 (FIXED)                             │    │
│  │ Load Tests:      10 (FIXED)                             │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  VERIFY: N≥3P, E≥3P, F≥3P, I≥3P (each individually)             │
│                                                                  │
│  ─────────────────────────────────────────────────────────────  │
│  EXAMPLES (Core only):                                           │
│                                                                  │
│  30 Positive → 60 Neg, 60 Edge, 50 Func, 50 Int = 250 core     │
│  50 Positive → 100 Neg, 100 Edge, 50 Func, 50 Int = 350 core   │
│  85 Positive → 170 Neg, 170 Edge, 50 Func, 50 Int = 525 core   │
│                                                                  │
│  Add Additional (122) for Grand Total:                           │
│  30P → 372 total | 50P → 472 total | 85P → 647 total            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 14.4 Common Commands Quick Reference

```bash
# .NET Test Commands
dotnet test                                    # Run all tests
dotnet test --filter "Category=Smoke"          # By category
dotnet test --filter "FullyQualifiedName~JIRA" # By name
dotnet test --no-build                         # Skip build
dotnet test --verbosity detailed               # Verbose output

# Playwright Commands
npx playwright test                            # Run all
npx playwright test tests/login.spec.ts        # Specific file
npx playwright test --headed                   # Show browser
npx playwright test --debug                    # Debug mode
npx playwright test --reporter=html            # HTML report
npx playwright show-report                     # View report

# Git Commands for QA
git checkout QA-branch                         # Switch to QA branch
git pull origin QA-branch                      # Get latest
git status                                     # Check changes
git add -A && git commit -m "msg"              # Commit changes
git push origin QA-branch                      # Push changes
```

---

## 15. Troubleshooting Common Issues

### 15.1 Test Infrastructure Issues

| Problem | Possible Causes | Solutions |
|---------|-----------------|-----------|
| Tests won't compile | Missing dependencies, wrong SDK | Run `dotnet restore`, check SDK version |
| All tests fail | Environment not configured | Check connection strings, environment variables |
| Tests pass locally, fail in CI | Environment differences | Check CI logs, compare environments |
| Flaky tests | Race conditions, timing issues | Add waits, use retries, improve isolation |
| Slow tests | Database not cleaned, too many E2E | Use in-memory DB, reduce E2E count |
| Integration/E2E tests fail with connection errors | Development proxy not started | Start the backend (`dotnet run`) and Angular dev server (`npm start`) before testing — see [Section 3.5](#35-starting-the-development-proxy-for-real-backend-testing) |
| `ERR_CONNECTION_REFUSED` or CORS errors | Backend API not running, or proxy not active | Verify both the .NET API and Angular dev server are running; check `proxy.conf.json` target port |
| Playwright times out on login page | Proxy/backend not running when tests start | Start proxy and backend first; for CI, ensure startup order in pipeline config |

### 15.2 Common Error Messages

```
Error: "Sequence contains no elements"
Cause: Query returned empty collection, .First() or .Single() called
Fix: Use .FirstOrDefault() or ensure test data exists

Error: "Object reference not set to an instance"
Cause: Null object being accessed
Fix: Check test setup, verify object initialization

Error: "Connection refused" / "Unable to connect"
Cause: Service not running, wrong URL
Fix: Start required services, check configuration

Error: "Timeout expired"
Cause: Database/service too slow, query taking too long
Fix: Increase timeout, optimize query, check for locks

Error: "Element not found" (E2E)
Cause: Element not rendered, wrong selector
Fix: Add wait, verify selector, check for dynamic content
```

### 15.3 When You're Stuck

1. **Check logs** - Build output, test runner output, application logs
2. **Simplify** - Reduce test to minimal reproduction case
3. **Compare** - Look at similar working tests
4. **Search** - Check documentation, Stack Overflow, team chat
5. **Ask** - Reach out to team members, subject matter experts
6. **Document** - If you solve it, document for future reference

---

## 16. Glossary

| Term | Definition |
|------|------------|
| **Assertion** | A statement that checks if a condition is true/false |
| **Boundary Value** | Input at the edge of valid/invalid range |
| **E2E (End-to-End)** | Testing complete user workflows through UI |
| **Edge Case** | Unusual but valid scenario at system boundaries |
| **Equivalence Partitioning** | Dividing inputs into groups that behave similarly |
| **Fixture** | Test setup and teardown code |
| **Flaky Test** | Test that sometimes passes, sometimes fails unpredictably |
| **Happy Path** | Standard successful workflow (positive test) |
| **Integration Test** | Testing multiple components working together |
| **Mock** | Simulated object that mimics real behavior |
| **Negative Test** | Test with invalid input expecting failure |
| **Positive Test** | Test with valid input expecting success |
| **Regression** | Bug introduced by code changes |
| **Regression Test** | Test to ensure existing features still work |
| **Smoke Test** | Quick sanity check of critical functionality |
| **Stub** | Simplified implementation returning canned responses |
| **Test Case** | Single test scenario with steps and expected result |
| **Test Coverage** | Percentage of code/features exercised by tests |
| **Test Data** | Input values used for testing |
| **Test Fixture** | Known state used as baseline for tests |
| **Test Suite** | Collection of related test cases |
| **TRX** | Microsoft test results XML format |
| **Unit Test** | Testing isolated code units (functions, classes) |
| **WCAG** | Web Content Accessibility Guidelines |
| **XSS** | Cross-Site Scripting vulnerability |

---

## Appendix A: Related Documents

| Document | Location | Purpose |
|----------|----------|---------|
| Shift-Left Testing Manifesto | `QA Tests/Documentation/SHIFT_LEFT_TESTING_MANIFESTO.md` | Team strategy, role definitions, quality gates |
| Shift-Left Scorecard | `QA Tests/Documentation/SHIFT_LEFT_SCORECARD.md` | Sprint dashboard, maturity model, retrospective questions |
| Action Items (Dev + QA) | `QA Tests/Documentation/ACTION_ITEMS.md` | Living to-do list for developers and QA |
| Onboarding Guide | `QA Tests/Documentation/ONBOARDING_GUIDE.md` | 30-60-90 day plan for new QA and Dev hires |
| Testing Structure | `QA Tests/Documentation/TESTING_STRUCTURE.md` | Repo test organization, CI pipeline, test counts |
| Production Readiness Checklist | `QA Tests/Documentation/PRODUCTION_READINESS_CHECKLIST.md` | Pre-release deployment checklist |
| Comprehensive Test Strategy | `.cursor/rules/comprehensive-test-strategy.mdc` | AI instruction rule with 3:1 ratio and code examples |
| Defect Management Standard | `.cursor/rules/defect-management.mdc` | How to log and manage defects |
| Defect List for Developers | `QA Tests/Defect List for Developers.md` | Product defects (~48 open, ~40 resolved) |
| Defect List for QA | `QA Tests/Defect List for QA.md` | Test infrastructure issues (~13 active, ~60 resolved) |
| Playwright Quickstart for Testers | `QA Tests/Playwright Tests/QUICKSTART_FOR_TESTERS.md` | Playwright setup, running tests, writing tests |

---

## Appendix B: Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-03 | QA Team | Initial version - consolidated from multiple documents |
| 1.1 | 2026-02-06 | QA Team | Synced with PDJ project playbook, standardized 2×P formula |
| 1.2 | 2026-02-06 | QA Team | Added Combinatorial Testing: Value Permutations section with pairwise testing strategy and data-driven test patterns |
| 1.3 | 2026-02-07 | QA Team | Added Section 3.2: Test Case Locations & Project Map — directory map, test type reference table, and decision guide for locating tests by purpose |
| 1.4 | 2026-02-10 | QA Team | Restructured Core/Additional categories: Moved Functional Tests (≥50) and Integration Tests (≥50) to Core Categories; moved Security and Concurrency to Additional Categories. Updated all minimum counts, ratio examples, and calculator to reflect new structure. Grand Total Minimum updated from ~293+ to ~347+ per suite. |
| 1.5 | 2026-02-16 | QA Team | Added Section 3.4: Git Repository Tracking Verification — mandatory checklist and commands to verify branch tracking, remote URL, and upstream configuration before committing. Prevents accidental commits to wrong branch/repository. Added tracking verification step to Day 1 onboarding checklist. |
| 1.6 | 2026-02-17 | QA Team | Added Section 4.3: Stakeholder Alignment on Test Cases — mandatory step to share test cases with PM/BA for agreement on testing scope and coverage level before execution. Includes review process, what to share, and sign-off template. |
| 1.7 | 2026-02-17 | QA Team | Aligned with PDJ playbook: Expanded Section 11.3 Integration Test Checklist Template to comprehensive version with 10 test categories (workflow, validation, status, delete, filtering, editing, referential integrity, audit, error handling, E2E), detailed sub-cases, and structured results summary. |
| 1.8 | 2026-02-23 | QA Team | Added Section 3.5: Starting the Development Proxy for Real Backend Testing — mandatory pre-test checklist explaining when and how to start the proxy, which test types require it, readiness verification steps, and common failure symptoms. Added proxy reminder callout to Section 6.2 (Running Automated Tests), proxy troubleshooting rows to Section 15.1, and proxy setup step to Day 1 onboarding checklist. |
| 1.9 | 2026-03-06 | QA Lead | Updated Project Map: Playwright now 102 specs (was 25), 21 POMs. Updated Documentation folder listing to include Manifesto, Scorecard, Action Items, Onboarding Guide. Updated CI pipeline reference to qa-tests.yml (11-job pipeline). Updated Related Documents with full documentation index and current defect counts. |
| 2.0 | 2026-03-09 | QA Lead | Added Section 7.5: Developer Defect Resolution Workflow — step-by-step guide for developers to pick up defects, use Claude to implement fixes, run defect-tagged tests to verify, remove Trait tags to promote tests to gating suite, and update the defect list. Added Section 7.6: AI Tools & Cursor Subagents for QA — documents all available subagents (create-tests, load-tests, performance-tests, playwright-test-generator, playwright-test-healer, playwright-test-planner), skills, invocation patterns, and governing Cursor rules. Added Section 7.7: Playwright Setup & Running E2E Tests — first-time installation, verification, common commands, project structure, configuration, and AI-assisted test writing. Updated Table of Contents with new subsections. |
| 2.1 | 2026-03-09 | QA Lead | Added Section 5.4: Test Data Conventions & Infrastructure — documents TestEntityBuilder fluent builders, user creation patterns, database modes (SQLite/PostgreSQL), Bogus fake data, Playwright JSON fixtures, workflow mock helpers, data isolation, and mock ID conventions. |
| 2.2 | 2026-03-09 | QA Lead | Added Section 4.5: What QA Should Receive Before Writing Tests — documents required inputs from PM/BA (PO-confirmed ACs, business rules, cross-feature impact, change log) and Solution Designer (traceability table, integration point specs, NFR validation, testability assessment). Includes Gate 0 verification checklist for QA and cross-reference to Manifesto Section 4. |

---

## Appendix C: Feedback and Improvements

This playbook is a living document. To suggest improvements:

1. Identify the section needing improvement
2. Document the proposed change
3. Submit via team's change request process
4. Changes reviewed and incorporated quarterly

---

**End of QA Tester Playbook**

*"Quality is never an accident; it is always the result of intelligent effort." — John Ruskin*
