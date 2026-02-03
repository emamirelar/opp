# QA Tester Playbook

**Version:** 1.0  
**Last Updated:** February 3, 2026  
**Audience:** QA Testers (New and Experienced)  
**Scope:** Universal guide applicable to any software project

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [QA Lifecycle Overview](#2-qa-lifecycle-overview)
3. [Phase 1: Project Onboarding](#3-phase-1-project-onboarding)
4. [Phase 2: Test Planning](#4-phase-2-test-planning)
5. [Phase 3: Test Development](#5-phase-3-test-development)
6. [Phase 4: Test Execution](#6-phase-4-test-execution)
7. [Phase 5: Defect Management](#7-phase-5-defect-management)
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
  - [ ] IDE (Visual Studio, VS Code, etc.)
  - [ ] Test runners (dotnet CLI, npm, pytest, etc.)
  - [ ] Browser testing tools (Playwright, Cypress, Selenium)
  - [ ] Database client (DBeaver, pgAdmin, SSMS)
  - [ ] API testing tool (Postman, Insomnia)

- [ ] **Clone repository** and verify build:
  ```bash
  git clone <repository-url>
  cd <project-folder>
  # Build and verify no errors
  dotnet build     # .NET projects
  npm install      # Node.js projects
  ```

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

### 3.2 Key Questions to Ask

| Question | Why It Matters |
|----------|----------------|
| What are the release cycles? | Determines testing timeline |
| What test types are prioritized? | Focuses your effort |
| What's the defect workflow? | Know how to log issues |
| Who are the subject matter experts? | Know who to ask questions |
| What's been problematic historically? | High-risk areas to focus on |
| What's the test data strategy? | Understand data dependencies |

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
| **Security Tests** | Injection, authorization, data exposure | 🔴 Critical |
| **Concurrency Tests** | Race conditions, duplicate submissions | 🟠 High |
| **Performance Tests** | Load, stress, response times | 🟡 Medium |
| **Accessibility Tests** | WCAG compliance, screen readers | 🟡 Medium |

### 4.3 Risk-Based Testing

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
QA Tests/
├── Documentation/
│   ├── QA_TESTER_PLAYBOOK.md        # This document
│   └── [other docs]
├── C# Tests/
│   ├── [Feature]/
│   │   ├── PositiveTests.cs
│   │   ├── NegativeTests.cs
│   │   └── EdgeCaseTests.cs
├── Integration Tests/
│   ├── [Feature]/
│   │   └── IntegrationTests.cs
├── Playwright Tests/
│   ├── [feature].spec.ts
│   └── [feature]-e2e.spec.ts
├── Test Execution Results/
│   └── [date]-results.md
└── Scripts/
    └── [setup scripts]
```

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
| ID | Title | Description | Reproduction Steps | Expected | Actual | Reported | Status |
|---|----|----|-----|-----|-----|-----|-----|
| DEF-XXX | [Brief title] | [Detailed description]<br/><br/>**Environment:** [ENV]<br/>**Browser:** [BROWSER]<br/>**User Role:** [ROLE] | 1. Step one<br/>2. Step two<br/>3. Step three | [Expected result] | [Actual result] | YYYY-MM-DD | Open |
```

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

| Category | Minimum Required | Formula |
|----------|------------------|---------|
| **Positive Tests** | 30-50 tests | Baseline (P) |
| **Negative Tests** | ≥50 AND ≥2×P | Max(50, 2×P) |
| **Edge Cases** | ≥50 AND ≥2×P | Max(50, 2×P) |
| **Security/Validation** | ≥50 (FIXED) | Always 50+ |
| **Concurrency** | ≥25 (FIXED) | Always 25+ |

### 9.3 Ratio Verification

```
REQUIREMENT: (Negative + Edge) ≥ 3 × Positive Tests
```

#### Example: 85 Positive Tests

| Category | Count | Calculation | Check |
|----------|-------|-------------|-------|
| Positive | 85 | Baseline | - |
| Negative | 170 | Max(50, 2×85) = 170 | ✅ |
| Edge Cases | 170 | Max(50, 2×85) = 170 | ✅ |
| Security | 50 | FIXED minimum | ✅ |
| Concurrency | 25 | FIXED minimum | ✅ |
| **Total** | **500** | - | - |
| **3:1 Check** | - | (170+170) = 340 ≥ 3×85 = 255 | ✅ |

### 9.4 Category Checklist

#### Positive Tests (Happy Path)
- [ ] Valid inputs with expected outputs
- [ ] Standard user workflows
- [ ] CRUD operations with valid data
- [ ] Successful authentication/authorization

#### Negative Tests (Failure Scenarios)
- [ ] Boundary violations (string too long, number out of range)
- [ ] Invalid data types (text in numeric fields)
- [ ] Special characters & injection attempts
- [ ] Null/empty/missing required fields
- [ ] Collection stress (empty, oversized)
- [ ] Date paradoxes (future dates, invalid ranges)
- [ ] Dependency failures (API timeout, DB error)

#### Edge Cases (Boundary Conditions)
- [ ] Financial precision (rounding, zero-sum)
- [ ] Temporal boundaries (fiscal year, leap year)
- [ ] Workflow state machine (illegal transitions)
- [ ] Threshold tests (exact limits, cumulative limits)
- [ ] Globalization (currency formats, multi-byte characters)

#### Security/Validation Tests
- [ ] SQL Injection prevention
- [ ] XSS (Cross-Site Scripting) prevention
- [ ] IDOR (Insecure Direct Object Reference)
- [ ] Privilege escalation prevention
- [ ] Authentication bypass attempts
- [ ] OWASP Top 10 coverage

#### Concurrency Tests
- [ ] Concurrent updates to same entity
- [ ] Double submit prevention
- [ ] Read during write (transaction isolation)
- [ ] Deadlock scenarios
- [ ] Race conditions in counters/aggregates

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

## 2. Validation Rules Testing

### TC-2.1: Required Field Validation

| Field | Test Input | Expected Error |
|-------|------------|----------------|
| Name | (empty) | "Name is required" |
| Email | (empty) | "Email is required" |

---

## 3. Status Transition Testing

### TC-3.1: Valid Status Transitions

| From Status | To Status | Expected |
|-------------|-----------|----------|
| Draft | Active | ✅ Allowed |
| Active | Inactive | ✅ Allowed |

---

## 4. Delete/Archive Functionality

### TC-4.1: Delete Allowed Entities

**Test Steps:**
1. Create entity in Draft status
2. Verify Delete button visible
3. Click Delete
4. Verify confirmation dialog
5. Confirm deletion
6. Verify soft delete in database

---

## 5. Filtering and Pagination

### TC-5.1: Search and Filter

| Search Term | Expected Results |
|-------------|------------------|
| "test" | Only matching entities |
| (clear) | All entities |

---

## Test Results Summary

**Test Date:** _______________
**Tester Name:** _______________
**Pass Rate:** ___/___  = ___%

### Sign-Off

| Role | Name | Date |
|------|------|------|
| Tester | | |
| Reviewer | | |
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
        
        [Fact]
        public async Task MethodName_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var input = CreateValidInput();
            
            // Act
            var result = await _fixture.Service.MethodAsync(input);
            
            // Assert
            Assert.NotNull(result);
        }
        
        [Fact]
        public async Task MethodName_NullInput_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _fixture.Service.MethodAsync(null));
        }
    }
}
```

### 12.2 E2E Test Template (Playwright/TypeScript)

```typescript
import { test, expect, Page } from '@playwright/test';

/**
 * JIRA-XXX: End-to-End Tests for [Feature Name]
 */
test.describe('[Feature Name] E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    // Authentication steps
  });

  test('should complete primary workflow successfully', async ({ page }) => {
    await page.goto('/feature-page');
    await page.fill('[data-testid="name-input"]', 'Test Name');
    await page.click('[data-testid="submit-button"]');
    
    await expect(page.locator('[data-testid="success-message"]'))
      .toBeVisible();
  });
});
```

---

## 13. Test Execution Reporting

### 13.1 Daily Test Execution Report Template

```markdown
# Daily Test Execution Report

**Date:** YYYY-MM-DD
**Tester:** [Name]

## Summary

| Metric | Count | Percentage |
|--------|-------|------------|
| **Total Test Cases** | | 100% |
| **Passed** | | % |
| **Failed** | | % |
| **Blocked** | | % |

## New Defects Found

| ID | Title | Severity |
|----|-------|----------|
| DEF-XXX | [Title] | 🔴 Critical |
```

---

## 14. Quick Reference Cards

### 14.1 Test Type Quick Reference

```
┌─────────────────────────────────────────────────────────────────┐
│                    TEST TYPE QUICK REFERENCE                     │
├──────────────┬───────────────────────────────────────────────────┤
│ Unit Tests   │ Individual functions, classes, methods            │
│ Integration  │ API endpoints, database operations                │
│ E2E Tests    │ Full user workflows through UI                    │
│ Smoke Tests  │ Critical paths only, quick sanity check           │
│ Regression   │ All existing functionality still works            │
│ Performance  │ Load, stress, response time                       │
└──────────────┴───────────────────────────────────────────────────┘
```

### 14.2 3:1 Ratio Quick Calculator

```
┌─────────────────────────────────────────────────────────────────┐
│                   3:1 RATIO CALCULATOR                           │
├─────────────────────────────────────────────────────────────────┤
│  If you have [P] Positive Tests:                                │
│                                                                  │
│  Negative Tests:  MAX(50, 2 × P)                                │
│  Edge Case Tests: MAX(50, 2 × P)                                │
│  Security Tests:  50 (FIXED)                                     │
│  Concurrency:     25 (FIXED)                                     │
│                                                                  │
│  VERIFY: (Negative + Edge) ≥ 3 × P                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 15. Troubleshooting Common Issues

| Problem | Possible Causes | Solutions |
|---------|-----------------|-----------|
| Tests won't compile | Missing dependencies | Run `dotnet restore` |
| All tests fail | Environment not configured | Check connection strings |
| Flaky tests | Race conditions | Add waits, use retries |

---

## 16. Glossary

| Term | Definition |
|------|------------|
| **Assertion** | A statement that checks if a condition is true/false |
| **E2E** | Testing complete user workflows through UI |
| **Flaky Test** | Test that sometimes passes, sometimes fails |
| **Happy Path** | Standard successful workflow (positive test) |
| **Regression** | Bug introduced by code changes |

---

## Appendix A: Related Documents

| Document | Location | Purpose |
|----------|----------|---------|
| Comprehensive Test Strategy | `.cursor/rules/comprehensive-test-strategy.mdc` | AI instruction rule with 3:1 ratio and code examples |
| Defect Management Standard | `.cursor/rules/defect-management.mdc` | How to log and manage defects |
| Test Execution Results | `QA Tests/Test Execution Results/` | Historical test results |
| Playwright Tests | `QA Tests/Playwright Tests/` | E2E test specifications |

---

## Appendix B: Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-03 | QA Team | Initial version - consolidated from multiple documents |

---

**End of QA Tester Playbook**

*"Quality is never an accident; it is always the result of intelligent effort." — John Ruskin*
