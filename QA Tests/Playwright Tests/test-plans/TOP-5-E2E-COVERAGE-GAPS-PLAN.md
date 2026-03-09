# Playwright E2E Test Coverage — TOP 5 Impactful Gaps Plan

**Document:** Structured plan for the Test Generator agent  
**Date:** 2025-03-02  
**Application:** UNOPS Opportunity+ Angular (Partnership & Opportunity Management)  
**Existing:** 79 spec files, 15 page objects, ~1,025 tests across 3 browsers  

---

## Executive Summary

This plan identifies the **TOP 5 most impactful E2E coverage gaps** based on analysis of:
- 79 existing spec files
- 15 page objects
- Application routes and features
- Negative/error handling patterns
- Cross-feature integration scenarios
- 3:1 ratio compliance (Negative, Edge, Functional, Integration ≥ 3× Positive)

Each gap includes: **what to test**, **page objects to use**, **estimated test count**, **priority**, and **handoff-ready instructions** for the Test Generator agent.

---

## Gap 1: Cross-Entity Workflow Integration (Partner → Contact → Interaction → Opportunity)

### Priority: **High**

### Problem
- `cross-entity-workflows.spec.ts` **contains Comments content** (file appears overwritten/duplicated) — CEW-001 to CEW-010 tests are **missing**
- `opportunity-cross-navigation.spec.ts` is **feature-gated** (`OPPORTUNITY_CROSSNAV_IMPLEMENTED=true`) and tests Partner→Opportunity only
- No E2E coverage for: Partner → Contact creation, Contact → Interaction creation, Interaction → Create Opportunity flow
- Critical business workflow: **log interaction with partner/contact → create opportunity from interaction**

### What Should Be Tested

| Category | Scenarios | Est. Count |
|----------|-----------|------------|
| **Positive** | Partner detail → Contacts tab → New Contact → Save → Contact appears in list | 1 |
| **Positive** | Partner detail → Interactions tab → New Interaction → Save → Interaction appears | 1 |
| **Positive** | Interaction detail → Create Opportunity button → Dialog opens → Create → Navigate to opportunity | 1 |
| **Negative** | Create Opportunity from interaction with invalid/missing required fields → validation shown | 2 |
| **Edge** | Partner with no contacts → Contacts tab shows empty state | 1 |
| **Edge** | Interaction with no linked partners → Create Opportunity pre-fills from interaction context | 1 |
| **Functional** | Partner tabs (Contacts, Interactions, Opportunities) reflect correct counts | 2 |
| **Integration** | Full flow: Partner → Add Contact → Add Interaction → Create Opportunity → Verify all linked | 2 |

### Page Objects to Use

| POM | Exists? | Action |
|-----|---------|--------|
| `partner-item.page.ts` | ✅ Yes | Use for Partner detail, tabs, related entities |
| `contact-item.page.ts` | ✅ Yes | Use for Contact detail |
| `interaction-item.page.ts` | ✅ Yes | Use for Interaction detail, Create Opportunity |
| `opportunity-item.page.ts` | ✅ Yes | Use for Opportunity detail |
| `interactions.page.ts` | ❌ No | **Create new** — list page (header, new button, listview, search) |

### API Mocks Needed

| Endpoint | Mocked? | Notes |
|----------|---------|-------|
| `GET /api/partner/{id}` | ✅ | Already mocked |
| `GET /api/partner/{id}/contacts` | Check | May need for partner contacts tab |
| `GET /api/partner/{id}/interactions` | Check | May need for partner interactions tab |
| `GET /api/interaction/{id}` | ✅ | Already mocked |
| `GET /api/interaction/{id}/permissions` | ✅ | Already mocked |
| `POST /api/opportunity` (create from interaction) | Check | Create-opportunity-from-interaction flow |

### Estimated Test Cases: **11**

### 3:1 Ratio Target

| Category | Target | Notes |
|----------|--------|-------|
| Positive (P) | 3 | Partner→Contact, Partner→Interaction, Interaction→Opportunity |
| Negative (N) | 9+ | Validation, permission denied, invalid context |
| Edge (E) | 9+ | Empty tabs, missing links, fallback paths |
| Functional (F) | 9+ | Tab counts, permission-driven visibility |
| Integration (I) | 9+ | Full cross-entity flow |

---

## Gap 2: Form Validation & Negative E2E (Create/Edit Dialogs)

### Priority: **High**

### Problem
- `form-validation.spec.ts` has **11 tests** but many use weak assertions (`expect(true).toBeTruthy()`, `expect(errorCount).toBeGreaterThanOrEqual(0)`)
- Conditional passes (`if (hasVisible)`) reduce reliability
- **Mocked-environment** negative validation is thin for Partner, Contact, Interaction create/edit forms
- Real-API specs (`partner-contact-crud.real.spec.ts`, `opportunity-crud.real.spec.ts`) have good negative tests — but mocked specs do not

### What Should Be Tested

| Category | Scenarios | Est. Count |
|----------|-----------|------------|
| **Positive** | Partner create with valid data → Save → Success toast, redirect or list refresh | 1 |
| **Positive** | Contact create with valid data → Save → Success | 1 |
| **Negative** | Partner create — empty name → Validation error, submit blocked | 2 |
| **Negative** | Partner create — invalid email format → Email validation error | 1 |
| **Negative** | Contact create — empty required field → Validation error | 2 |
| **Negative** | Interaction create — empty subject/date → Validation error | 2 |
| **Edge** | Max-length input (255 chars) → Accepted or truncated | 1 |
| **Edge** | Special characters in name → Accepted, no XSS | 1 |
| **Functional** | Submit button disabled when form invalid | 1 |
| **Functional** | Validation clears when field corrected | 1 |

### Page Objects to Use

| POM | Exists? | Action |
|-----|---------|--------|
| `partners.page.ts` | ✅ Yes | `clickNew()`, wait for dialog |
| `contacts.page.ts` | ✅ Yes | `clickNew()`, wait for dialog |
| `entity-list.page.ts` | ✅ Yes | Generic list actions |
| `interactions.page.ts` | ❌ No | Create if not done in Gap 1 |

### API Mocks Needed

- Use existing `setupAPIMocks()` — validation is client-side
- Optionally mock `POST` with 400 for server-side validation tests

### Estimated Test Cases: **13**

### 3:1 Ratio Target

| Category | Target |
|----------|--------|
| Positive (P) | 2–3 |
| Negative (N) | 7+ |
| Edge (E) | 6+ |
| Functional (F) | 6+ |
| Integration (I) | 6+ |

---

## Gap 3: Interactions List — POM + Negative/Error Coverage

### Priority: **High**

### Problem
- **No `interactions.page.ts`** — `interactions.spec.ts` uses raw locators
- `interactions.spec.ts` has **15 tests** — mostly display/visibility, many conditional passes
- Two tests **skip** when DynamicDialog doesn’t render (QA-008)
- No negative tests: empty list, invalid ID, 404, permission denied
- No search/filter validation, no create-flow coverage

### What Should Be Tested

| Category | Scenarios | Est. Count |
|----------|-----------|------------|
| **Positive** | List loads with mocked data, cards/rows visible | 1 |
| **Positive** | Click card → Navigate to interaction detail | 1 |
| **Negative** | Navigate to `/partnerships/interactions/99999` → 404 or error message | 1 |
| **Negative** | Readonly user → New Interaction button hidden | 1 |
| **Negative** | API returns 500 → Error state or retry shown | 1 |
| **Edge** | Empty list (0 records) → "No data" message, no crash | 1 |
| **Edge** | Search with no results → Empty state message | 1 |
| **Functional** | Export/Import buttons visibility by permission | 1 |
| **Functional** | Create Opportunity button visibility by permission | 1 |
| **Integration** | List → Detail → Back to list → List still correct | 1 |

### Page Objects to Use

| POM | Exists? | Action |
|-----|---------|--------|
| `interactions.page.ts` | ❌ No | **Create new** — extend `EntityListPage` or `BasePage` |
| `interaction-item.page.ts` | ✅ Yes | Use for detail navigation |

### API Mocks Needed

| Endpoint | Mocked? | Notes |
|----------|---------|-------|
| `GET /api/interactions` | ✅ | Already mocked |
| `GET /api/interaction/{id}` | ✅ | Already mocked |
| `GET /api/interaction/{id}/permissions` | ✅ | Already mocked |
| 404/500 override | Add | For negative tests |

### Estimated Test Cases: **10**

### 3:1 Ratio Target

| Category | Target |
|----------|--------|
| Positive (P) | 2 |
| Negative (N) | 6+ |
| Edge (E) | 6+ |
| Functional (F) | 6+ |
| Integration (I) | 6+ |

---

## Gap 4: AI Assistant — Negative, Error & Permission Coverage

### Priority: **Medium**

### Problem
- `ai-assistant.page.ts` is **rich** (prompt, response, transcribe, comparison)
- `ai-assistant.spec.ts` has ~10 tests — mostly visibility and chat container
- No negative tests: empty prompt, API error, timeout
- No permission tests (e.g. AI disabled for restricted role)
- No integration with opportunity context (e.g. "summarize this opportunity")

### What Should Be Tested

| Category | Scenarios | Est. Count |
|----------|-----------|------------|
| **Positive** | Open AI panel, send valid prompt, response visible | 1 |
| **Negative** | Send empty prompt → Submit disabled or validation message | 1 |
| **Negative** | API returns 500 → Error message, no crash | 1 |
| **Negative** | API timeout → Loading stops, error or retry shown | 1 |
| **Edge** | AI panel closed → Reopen → Session preserved or new session | 1 |
| **Functional** | AI response contains markdown/formatting | 1 |
| **Integration** | From opportunity detail → Open AI → Context includes opportunity | 1 |

### Page Objects to Use

| POM | Exists? | Action |
|-----|---------|--------|
| `ai-assistant.page.ts` | ✅ Yes | Use `sendPrompt()`, `waitForResponse()`, `isAssistantOpen()` |

### API Mocks Needed

- AI/LLM endpoints — check `api-mocks.helper.ts` for existing AI mocks
- Add 500/timeout overrides for error tests

### Estimated Test Cases: **7**

### 3:1 Ratio Target

| Category | Target |
|----------|--------|
| Positive (P) | 1 |
| Negative (N) | 3+ |
| Edge (E) | 3+ |
| Functional (F) | 3+ |
| Integration (I) | 3+ |

---

## Gap 5: API Error Handling & Network Failure E2E

### Priority: **Medium**

### Problem
- `auth-session-handling.spec.ts` has **403** handling
- `google-analytics.spec.ts` has **404, 500, network abort** — but only for GA
- **Most specs** do not test: API 500, network timeout, 404 on entity fetch
- Critical for production: user sees error, not crash or blank screen

### What Should Be Tested

| Category | Scenarios | Est. Count |
|----------|-----------|------------|
| **Positive** | Partner list loads → Data displayed | 1 |
| **Negative** | Partner list API returns 500 → Error toast or message, no crash | 1 |
| **Negative** | Opportunity detail API returns 404 → Not found message or redirect | 1 |
| **Negative** | Network timeout on entity fetch → Retry or error message | 1 |
| **Edge** | Permission endpoint returns 403 → UI reflects read-only (no edit buttons) | 1 |
| **Functional** | Global HTTP error handler shows toast | 1 |
| **Integration** | 500 on list → User can retry or navigate away | 1 |

### Page Objects to Use

| POM | Exists? | Action |
|-----|---------|--------|
| `partners.page.ts` | ✅ Yes | `navigateTo()`, `search()` |
| `opportunity-item.page.ts` | ✅ Yes | `navigate()` |

### API Mocks Needed

- Override specific routes in test with `page.route()` to return 404, 500, or `route.abort()`

### Estimated Test Cases: **7**

### 3:1 Ratio Target

| Category | Target |
|----------|--------|
| Positive (P) | 1 |
| Negative (N) | 3+ |
| Edge (E) | 3+ |
| Functional (F) | 3+ |
| Integration (I) | 3+ |

---

## Summary Table

| Gap | Priority | Est. Tests | New POM | Key Deliverables |
|-----|----------|------------|---------|------------------|
| 1. Cross-Entity Workflows | High | 11 | `interactions.page.ts` | Fix `cross-entity-workflows.spec.ts`, add CEW tests |
| 2. Form Validation | High | 13 | — | Strengthen `form-validation.spec.ts` or new `form-validation-negative.spec.ts` |
| 3. Interactions List | High | 10 | `interactions.page.ts` | New POM, extend `interactions.spec.ts` |
| 4. AI Assistant | Medium | 7 | — | Extend `ai-assistant.spec.ts` |
| 5. API Error Handling | Medium | 7 | — | New `api-error-handling.spec.ts` or extend `auth-session-handling.spec.ts` |

**Total estimated new tests:** ~48

---

## Handoff Instructions for Test Generator Agent

1. **Read** `.cursor/skills/generate-playwright/SKILL.md` for spec structure, POM patterns, and API mocking.
2. **Follow** `.cursor/rules/test-ratio-enforcement.mdc` — ensure N, E, F, I ≥ 3×P for each spec.
3. **Use** `[data-testid="..."]` selectors exclusively; add `// TODO: add data-testid` when missing.
4. **Add** new API mocks to `helpers/api-mocks.helper.ts` as needed.
5. **Implement** in this order: Gap 1 (cross-entity) → Gap 3 (interactions POM) → Gap 2 (form validation) → Gap 4 (AI) → Gap 5 (API errors).
6. **Fix** `cross-entity-workflows.spec.ts` — replace Comments content with correct CEW-001 to CEW-010 cross-entity workflow tests.

---

## Reference Files

| File | Purpose |
|------|---------|
| `go-decision.spec.ts` | Full-featured spec reference |
| `contacts.spec.ts` | List + detail spec pattern |
| `pages/base.page.ts` | Base class for POMs |
| `pages/opportunity-item.page.ts` | Rich POM example |
| `helpers/api-mocks.helper.ts` | Route mocks |
| `helpers/auth.helper.ts` | Test users, `authenticateWithRealBackend` |
| `helpers/wait.helper.ts` | `waitForPermissions` |
