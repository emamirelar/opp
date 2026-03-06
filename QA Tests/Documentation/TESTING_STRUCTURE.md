# UNOPS Opportunity+ Testing Structure

**Last Updated:** March 6, 2026  
**Status:** All test suites operational  
**Companion to:** [Shift-Left Testing Manifesto](SHIFT_LEFT_TESTING_MANIFESTO.md)

---

## Test Organization

The repository has **4 types of tests** organized as follows:

```
opportunityplus/
├── QA Tests/
│   ├── C# Tests/                    # ← Backend unit & business logic tests
│   │   ├── UNOPS.PAO.FastTests/     #    ~43 fast logic tests
│   │   ├── UNOPS.PAO.Business.Tests/#    ~3,800 business tests
│   │   └── UNOPS.PAO.Presentation.Tests/ # ~164 controller tests
│   ├── Integration Tests/           # ← API/controller integration tests
│   │   └── UNOPS.PAO.IntegrationTests/   # ~5,500+ integration tests
│   ├── Playwright Tests/            # ← E2E browser tests
│   │   ├── *.spec.ts                #    102 spec files
│   │   └── pages/*.page.ts          #    21 page objects
│   ├── Documentation/               # ← All QA documentation (this file)
│   ├── Defect List for Developers.md# ← DEF-XXX production defects
│   └── Defect List for QA.md        # ← QA-XXX test infrastructure issues
└── UNOPS.PAO.ClientApp/src/
    ├── app/**/*.spec.ts             # ← Angular component unit tests
    └── qa-frontend-tests/           # ← QA-owned frontend tests
```

---

## 1. C# Backend Tests

**Framework:** xUnit + FluentAssertions + Moq  
**Language:** C#  
**Run by:** Developers (locally + CI) and QA (CI + regression)

### Test Projects

| Project | Location | Tests | Purpose |
|---|---|---|---|
| **FastTests** | `QA Tests/C# Tests/UNOPS.PAO.FastTests/` | ~43 | Quick validation — smoke tests, critical logic |
| **Business.Tests** | `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/` | ~3,800 | Manager-level business logic, CRUD, validation, performance, load tests |
| **Presentation.Tests** | `QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests/` | ~164 | Controller/API endpoint tests |
| **IntegrationTests** | `QA Tests/Integration Tests/` | ~5,500+ | Full-stack integration: controllers, business rules, cross-entity flows |

### Integration Test Folders

The `QA Tests/Integration Tests/` project is organized by feature area:

```
Integration Tests/
├── Controllers/              # Controller endpoint tests
├── Dashboard/                # Dashboard tests
├── Documents/                # Document management tests
├── EntityConfiguration/      # Entity config tests
├── OrgHierarchy/             # Org unit hierarchy tests
├── PartnerTree/              # Partner tree tests
├── PartnerAnalytics/         # Partner analytics tests
├── ContactAnalytics/         # Contact analytics tests
├── UserManagement/           # User management tests
├── UserProfile/              # User profile tests
├── SystemAdmin/              # System admin tests
├── Permissions/              # Permission tests
├── LiaisonOffice/            # Liaison office tests
├── Workflow/                 # Workflow tests
├── PNO-*/                    # JIRA story-specific tests (PNO-914, PNO-1146, etc.)
├── DEF-*/                    # Defect regression tests
├── BugFix_Regressions/       # Bug fix regression tests
├── UnitTests/                # Unit-level tests within integration project
└── Infrastructure/           # Test infrastructure (factories, stubs, fixtures)
```

### Quick Start — Running C# Tests

```bash
# Smoke tests (fastest gate, ~30 seconds)
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "Category=Smoke"

# Fast tests (~43 tests, <10 seconds)
dotnet test "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj"

# Business tests (~3,800 tests, excludes known defects)
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "Defect!~DEF"

# Presentation tests (~164 tests)
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests"

# Integration tests (~5,500 tests, excludes known defects)
dotnet test "QA Tests/Integration Tests" --filter "Defect!~DEF"

# Feature-specific tests (example: Partner)
dotnet test "QA Tests/Integration Tests" --filter "FullyQualifiedName~Partner"

# Known defect tests only (informational, expected to fail)
dotnet test --filter "Defect~DEF"
```

---

## 2. Playwright E2E Tests

**Framework:** Playwright  
**Language:** TypeScript  
**Run by:** QA (100% QA-owned)  
**Location:** `QA Tests/Playwright Tests/`

### Test Structure

| Category | Spec Files | Examples |
|---|---|---|
| **Core** | 6 | `login.spec.ts`, `home.spec.ts`, `dashboard.spec.ts`, `partners.spec.ts`, `opportunities.spec.ts`, `interactions.spec.ts` |
| **Entity Detail** | 8 | `partner-item.spec.ts`, `opportunity-item.spec.ts`, `contact-item.spec.ts`, `interaction-item.spec.ts` (+ basic variants) |
| **Opportunity** | 9 | `opportunity-creation.spec.ts`, `opportunity-documents.spec.ts`, `opportunity-workflow-transitions.spec.ts`, etc. |
| **Workflow** | 6 | `workflow.spec.ts`, `go-decision.spec.ts`, `workflow-notifications.spec.ts`, etc. |
| **Search** | 4 | `deep-search.spec.ts`, `search-listviews.spec.ts`, `search-results-enhanced.spec.ts`, `search-icons.spec.ts` |
| **Admin** | 4 | `admin-features.spec.ts`, `admin-entity-config.spec.ts`, `admin-translation-workbench.spec.ts`, `user-management.spec.ts` |
| **Partner/Contact** | 7 | `contacts.spec.ts`, `partner-tree.spec.ts`, `partner-features.spec.ts`, `partner-detail-tabs.spec.ts`, etc. |
| **AI** | 3 | `ai-assistant.spec.ts`, `ai-assistant-negative.spec.ts`, `ai-comparison.spec.ts` |
| **Infrastructure** | 8 | `form-validation.spec.ts`, `role-access-control.spec.ts`, `accessibility.spec.ts`, `api-error-handling.spec.ts`, etc. |
| **Real/Advanced** | 14+ | `*.real.spec.ts` — tests against live environments |
| **Other** | 30+ | `notifications.spec.ts`, `comments.spec.ts`, `import-export.spec.ts`, `cross-entity-navigation.spec.ts`, etc. |
| **Total** | **102** | |

### Page Objects

21 page object models in `QA Tests/Playwright Tests/pages/`:

```
pages/
├── login.page.ts
├── home.page.ts
├── dashboard.page.ts
├── partners.page.ts
├── partner-item.page.ts
├── opportunities.page.ts
├── opportunity-item.page.ts
├── contacts.page.ts
├── contact-item.page.ts
├── interactions.page.ts
├── interaction-item.page.ts
└── ... (21 total)
```

### Quick Start — Running Playwright Tests

```bash
# From QA Tests/Playwright Tests/ folder

# Run all tests
npx playwright test

# Run with browser visible
npx playwright test --headed

# Run interactive UI mode
npx playwright test --ui

# Run specific spec
npx playwright test login.spec.ts

# Run smoke specs only (6 core specs)
npx playwright test login.spec.ts home.spec.ts dashboard.spec.ts partners.spec.ts opportunities.spec.ts interactions.spec.ts

# Generate HTML report
npx playwright show-report
```

### Documentation

- [Playwright Quickstart for Testers](../Playwright%20Tests/QUICKSTART_FOR_TESTERS.md)

---

## 3. Angular Unit Tests

**Framework:** Jasmine + Karma  
**Language:** TypeScript  
**Location:** `UNOPS.PAO.ClientApp/src/app/**/*.spec.ts`

### Quick Start

```bash
cd UNOPS.PAO.ClientApp

# Run in CI mode (headless)
npm run test:ci

# Run with watch mode (interactive)
ng test
```

---

## 4. CI/CD Pipeline

**File:** `.github/workflows/qa-tests.yml`

### Triggers

- **Push:** `main`, `dev-deploy`, `QA-Tests` branches
- **Pull Request:** `main`, `dev-deploy` branches
- **Schedule:** Nightly at 2:00 UTC
- **Manual:** `workflow_dispatch` with Playwright tier selection

### Pipeline Structure

```
Developer pushes code / creates PR
         │
         ▼
┌─────────────────────────────────────────────────┐
│           GitHub Actions Triggered               │
├─────────────────────────────────────────────────┤
│                                                  │
│  BUILD STAGE                                     │
│  ┌──────────────────┐  ┌──────────────────────┐ │
│  │ dotnet-build     │  │ angular-build        │ │
│  │ .NET 9, NuGet    │  │ npm, ESLint, audit   │ │
│  └──────────────────┘  └──────────────────────┘ │
│           │                       │              │
│           ▼                       ▼              │
│  TEST STAGE                                      │
│  ┌──────────────┐  ┌──────────────────────────┐ │
│  │ Smoke Tests  │  │ Fast Tests (~43)         │ │
│  │ (Category=   │  │ Code coverage            │ │
│  │  Smoke)      │  │ BLOCKING                 │ │
│  │ BLOCKING     │  └──────────────────────────┘ │
│  └──────────────┘                                │
│  ┌──────────────┐  ┌──────────────────────────┐ │
│  │ Business     │  │ Presentation Tests       │ │
│  │ Tests        │  │ (~164 tests)             │ │
│  │ (~3,800)     │  │ BLOCKING                 │ │
│  │ PostgreSQL   │  └──────────────────────────┘ │
│  │ BLOCKING     │                                │
│  └──────────────┘  ┌──────────────────────────┐ │
│  ┌──────────────┐  │ Frontend Tests           │ │
│  │ Integration  │  │ (Angular unit tests)     │ │
│  │ Tests        │  │ BLOCKING                 │ │
│  │ (~5,500)     │  └──────────────────────────┘ │
│  │ PostgreSQL   │                                │
│  │ CONTINUE     │  ┌──────────────────────────┐ │
│  └──────────────┘  │ Playwright Smoke (6 specs)│ │
│                    │ PR/push only              │ │
│  ┌──────────────┐  │ CONTINUE                 │ │
│  │ Defect Tests │  └──────────────────────────┘ │
│  │ (Defect~DEF) │                                │
│  │ Informational│  ┌──────────────────────────┐ │
│  │ CONTINUE     │  │ Playwright Extended      │ │
│  └──────────────┘  │ (25 specs) PR→main only  │ │
│                    └──────────────────────────┘ │
│                    ┌──────────────────────────┐ │
│                    │ Playwright Full (~96 specs)│ │
│                    │ Nightly or manual         │ │
│                    │ Sharded 4×                │ │
│                    └──────────────────────────┘ │
│                                                  │
│  SUMMARY                                         │
│  ┌──────────────────────────────────────────────┐│
│  │ test-summary: Aggregated report              ││
│  └──────────────────────────────────────────────┘│
└─────────────────────────────────────────────────┘
         │
         ▼
   PR merge allowed (if blocking gates pass)
```

### Playwright Tiers

| Tier | Specs | When | Purpose |
|---|---|---|---|
| **Smoke** | 6 core specs | Every PR/push | Fast E2E validation |
| **Extended** | 25 specs | PR to main, manual | Core CRM + validation + permissions |
| **Full** | ~96 specs (sharded 4×) | Nightly, manual | Complete E2E regression |
| **Cross-browser** | All specs × 3 browsers | Manual only | Chrome, Firefox, Safari |

---

## Defect Tracking

| File | Prefix | Current Count | Purpose |
|---|---|---|---|
| `Defect List for Developers.md` | DEF-XXX | ~48 open, ~40 resolved | Production code defects |
| `Defect List for QA.md` | QA-XXX | ~13 active, ~60 resolved | Test infrastructure issues |

Defect tests use `[Trait("Defect", "DEF-XXX")]` and run in a separate non-blocking CI job.

---

## Test Ownership Summary

| Test Type | Count | Owner | Dev Runs? | CI Gate? |
|---|---|---|---|---|
| Smoke Tests | ~14 | QA + Dev | Yes | Blocking |
| Fast Tests | ~43 | QA | Yes | Blocking |
| Business Tests | ~3,800 | QA (with AI) | Yes (pre-PR) | Blocking |
| Presentation Tests | ~164 | QA (with AI) | Yes (pre-PR) | Blocking |
| Integration Tests | ~5,500+ | QA (with AI) | Yes (pre-PR) | Continue-on-error |
| Playwright E2E | 102 specs | QA (100%) | No | Smoke: continue; Full: nightly |
| Angular Unit Tests | Varies | Dev + QA | Yes | Blocking |
| Defect Tests | Varies | QA | No | Informational |

---

## Documentation Index

All QA documentation is centralized in `QA Tests/Documentation/`:

| Document | Purpose |
|---|---|
| [Shift-Left Testing Manifesto](SHIFT_LEFT_TESTING_MANIFESTO.md) | Team strategy, roles, handshake points, quality gates |
| [Shift-Left Scorecard](SHIFT_LEFT_SCORECARD.md) | Measurement criteria, sprint dashboard, maturity model |
| [Action Items](ACTION_ITEMS.md) | Living to-do list for developers and QA |
| [QA Tester Playbook](QA_TESTER_PLAYBOOK.md) | Day-to-day QA practices, test categories, templates |
| [Onboarding Guide](ONBOARDING_GUIDE.md) | 30-60-90 day plan for new hires |
| [Testing Structure](TESTING_STRUCTURE.md) | This file — repo test organization |
| [Production Readiness Checklist](PRODUCTION_READINESS_CHECKLIST.md) | Pre-release checklist |
| [Developer Implementation Checklist](DEVELOPER_IMPLEMENTATION_CHECKLIST.md) | Opportunity feature implementation checklist |
| [oUP Integration Checklist](OpportunityPlus_oUP_Integration_Checklist.md) | oUP sync field mapping validation |

### Developer Quick Reference

| I want to... | Command |
|---|---|
| Run smoke tests | `dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "Category=Smoke"` |
| Run tests for my feature | `dotnet test "QA Tests/Integration Tests" --filter "FullyQualifiedName~FeatureName"` |
| Run all blocking tests | `dotnet test --filter "Defect!~DEF"` |
| See known defects | `dotnet test --filter "Defect~DEF"` |
| Run Playwright (QA only) | `npx playwright test --ui` |

---

## Version History

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0 | 2026-01-23 | QA Team | Initial version |
| 2.0 | 2026-03-06 | QA Lead | Complete rewrite: Updated all test counts (568 C# files, 102 Playwright specs, 21 POMs). Updated CI pipeline to match qa-tests.yml (build stage, test stage with 11 jobs, Playwright tiers). Added integration test folder structure. Updated defect tracking counts. Added test ownership summary. Centralized documentation index. Removed stale references. |
