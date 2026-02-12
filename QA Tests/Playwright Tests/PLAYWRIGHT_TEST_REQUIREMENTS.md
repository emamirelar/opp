# Playwright E2E Test Requirements

**Last Updated:** 2026-02-12  
**Suite Size:** 994 tests (584 passing, 409 skipped, 1 known failure)  
**Runtime:** ~39 minutes (full chromium suite, single invocation)

---

## 1. Environment Requirements

### 1.1 Software Prerequisites

| Requirement | Version | Purpose |
|---|---|---|
| **Node.js** | 20.x+ | Playwright runtime, Angular dev server |
| **npm** | 10.x+ | Package management |
| **Angular CLI** | 19.x | `ng serve` for the Angular dev server |
| **.NET SDK** | 9.0 | Backend API (optional — tests use API mocks by default) |
| **PostgreSQL** | 15+ | Database (optional — only needed for real backend tests) |
| **Playwright** | Latest | E2E test framework (installed via `npm install`) |
| **Chromium** | Bundled | Primary test browser (installed via `npx playwright install chromium`) |

### 1.2 Network & Ports

| Service | URL | Required |
|---|---|---|
| Angular dev server | `http://127.0.0.1:4200` | Yes (auto-started by Playwright `webServer` config) |
| .NET Backend API | `http://localhost:5159` | No (API mocks handle all requests by default) |
| PostgreSQL | `localhost:5432` | No (only for real backend testing) |

### 1.3 Node.js Heap Configuration

The full 994-test suite requires increased heap memory to avoid OOM crashes:

```
NODE_OPTIONS=--max-old-space-size=4096
```

This is **automatically configured** in `playwright.config.ts`. No manual setup needed unless overriding externally.

---

## 2. Authentication Requirements

### 2.1 Mock Authentication (Default)

All tests use `authenticateWithRealBackend()` from `helpers/auth.helper.ts`, which:

1. Clears browser cookies
2. Sets up API route mocks via `setupAPIMocks()`
3. Injects authenticated user claims (`/user/claims` mock)
4. Sets authentication cookies (`dev-user-email`, `DevIAPAuth`)
5. Navigates to the target page
6. Waits for Angular to initialize

**Default test user:** `test@playwright.local` with `Administrator` role.

### 2.2 Restricted Test Users

For role-based access control tests, restricted users are defined in `auth.helper.ts`:

| Email | Role | Purpose |
|---|---|---|
| `test@playwright.local` | Administrator | Default — full access |
| `test-readonly@playwright.local` | UNOPS_GEN_USER | Read-only access testing |
| `test-no-permissions@playwright.local` | UNOPS_GEN_USER | No-permission testing |
| `viewer@example.com` | UNOPS_GEN_USER | Viewer role testing |
| `doa2@example.com` | UNOPS_GEN_USER | DoA2 approver testing |
| `other-user@example.com` | UNOPS_GEN_USER | Cross-user testing |

### 2.3 Real Backend Authentication (Optional)

For tests tagged with real backend requirements (e.g., `login.spec.ts`):

1. .NET backend must be running at `http://localhost:5159`
2. PostgreSQL must be running with `TestDb` database
3. Test user must exist (run `setup-test-user.sql`)
4. Entity permissions must be configured (run `setup-opportunity-permissions.sql`)

---

## 3. API Mocking Requirements

### 3.1 Mock Infrastructure

All API mocks are defined in `helpers/api-mocks.helper.ts`. The `setupAPIMocks()` function registers route intercepts for:

**Authentication Endpoints:**
- `/api/configuration` — App configuration
- `/user/claims` — User identity claims
- `/api/global/preferred-language` — Language preference
- `/user/login`, `/user/register`, `/user/googleSignIn` — Auth endpoints

**Reference Data Endpoints:**
- `/api/values/partners`, `/api/values/contacts`, `/api/values/organization-units`
- `/api/values/salutations`, `/api/values/status`, `/api/values/pronouns`
- `/api/values/countries`, `/api/values/states`, `/api/values/liaison-offices`
- `/api/values/users/paged`, `/api/partner-tree-structure`

**Entity List Endpoints:**
- `GET /api/partner`, `/api/contact`, `/api/interaction(s)`, `/api/opportunity` — List data
- `GET /api/{entity}/search` — Search results

**Entity Detail Endpoints:**
- `GET /api/partner/{id}`, `/api/contact/{id}`, `/api/interaction/{id}`, `/api/opportunity/{id}`
- `GET /api/partner/{id}/permissions`, `/api/opportunity/{id}/permissions`

**Workflow Endpoints:**
- `GET /api/workflow/{entity}/{id}` — Stage and actions

**Catch-All:**
- Any unmatched `/api/*` or `/user/*` request returns smart defaults (200 OK with empty arrays/objects).

### 3.2 Mock Logging Control

Mock logging is **off by default** to prevent memory exhaustion (QA-041). Enable for debugging:

| Environment Variable | Default | Effect |
|---|---|---|
| `PLAYWRIGHT_DEBUG_MOCKS` | `false` | Enables verbose API mock logging |
| `PLAYWRIGHT_DEBUG_AUTH` | `false` | Enables verbose auth flow logging |

```powershell
# Debug a specific test with verbose logging
$env:PLAYWRIGHT_DEBUG_MOCKS='true'; npx playwright test my-test.spec.ts --project=chromium
```

---

## 4. Test File Inventory

### 4.1 Spec Files (39 total)

#### Active Tests (passing)

| Spec File | Tests | Category | Description |
|---|---|---|---|
| `contacts.spec.ts` | 13 | CRUD | Contact list CRUD, search, business card scanner |
| `partners.spec.ts` | 11 | CRUD | Partner list CRUD, search, navigation |
| `interactions.spec.ts` | 13 | CRUD | Interaction list CRUD, create opportunity |
| `opportunities.spec.ts` | 11 | CRUD | Opportunity list CRUD, search, workflow |
| `partner-item.spec.ts` | 21 | Detail | Partner detail page — page-object-based |
| `partner-item-basic.spec.ts` | 10 | Detail | Partner detail basics |
| `contact-item.spec.ts` | 24 | Detail | Contact detail page — page-object-based |
| `contact-item-basic.spec.ts` | 3 | Detail | Contact detail basics |
| `interaction-item.spec.ts` | 27 | Detail | Interaction detail page — page-object-based |
| `interaction-item-basic.spec.ts` | 10 | Detail | Interaction detail basics |
| `opportunity-item.spec.ts` | 35 | Detail | Opportunity detail page — page-object-based |
| `opportunity-item-basic.spec.ts` | 3 | Detail | Opportunity detail basics |
| `opportunity-sections.spec.ts` | 54 | Feature | Team, Workflow, WHY, WHAT sections |
| `opportunity-creation.spec.ts` | varies | Feature | Opportunity creation from partners |
| `home.spec.ts` | varies | Navigation | Home page, dashboard |
| `dashboard.spec.ts` | varies | Navigation | Dashboard widgets, quick actions |
| `login.spec.ts` | varies | Auth | Login form, validation (requires real backend) |
| `test-login-mock.spec.ts` | varies | Auth | Mocked login flow |
| `form-validation.spec.ts` | varies | UI | Required fields, email, number, date |
| `navigation-tabs.spec.ts` | varies | UI | Responsive tabs (desktop/mobile) |
| `search-listviews.spec.ts` | varies | Search | Filtering, pagination, columns |
| `role-access-control.spec.ts` | varies | Security | RBAC for 5 roles across major pages |
| `partner-features.spec.ts` | varies | Feature | Partner ecosystem, hierarchy, intelligence |
| `admin-features.spec.ts` | varies | Admin | User roles, AI prompts, role matrix |
| `jira-requirements.spec.ts` | varies | Requirements | JIRA-derived requirements tests |
| `go-decision.spec.ts` | varies | Workflow | Go/No-Go decision flow |
| `oup-integration.spec.ts` | varies | Integration | oUP integration |
| `seed.spec.ts` | 1 | Setup | Minimal seed/placeholder test |

#### Skipped Tests (scaffolded, awaiting feature readiness)

| Spec File | Tests | Reason Skipped |
|---|---|---|
| `ai-assistant.spec.ts` | ~20 | AI services not available in test env |
| `admin-entity-config.spec.ts` | ~15 | Feature not fully testable |
| `admin-translation-workbench.spec.ts` | ~15 | Feature not fully testable |
| `comments.spec.ts` | ~15 | Feature not fully testable |
| `document-management.spec.ts` | ~20 | Feature not fully testable |
| `import-export.spec.ts` | ~30 | Feature not fully testable |
| `partner-tree.spec.ts` | ~10 | Feature not fully testable |
| `product-service-search.spec.ts` | ~15 | Feature not fully testable |
| `profile-settings.spec.ts` | ~10 | Feature not fully testable |
| `user-management.spec.ts` | ~15 | Feature not fully testable |
| `workflow.spec.ts` | ~15 | Feature not fully testable |

### 4.2 Page Objects (17 total)

| Page Object | Base Class | Entity | Purpose |
|---|---|---|---|
| `base.page.ts` | — | Generic | Common helpers (`getByTestId`, `goto`, `waitForLoad`) |
| `entity-list.page.ts` | BasePage | Generic | List view shared methods |
| `entity-detail.page.ts` | BasePage | Generic | Detail view shared methods (workflow, back button, documents) |
| `partner-item.page.ts` | EntityDetailPage | Partner | Partner detail — resilient selectors |
| `contact-item.page.ts` | EntityDetailPage | Contact | Contact detail — resilient selectors |
| `interaction-item.page.ts` | EntityDetailPage | Interaction | Interaction detail — resilient selectors |
| `opportunity-item.page.ts` | EntityDetailPage | Opportunity | Opportunity detail — resilient selectors |
| `partners.page.ts` | BasePage | Partner | Partner list page |
| `contacts.page.ts` | BasePage | Contact | Contact list page |
| `opportunities.page.ts` | BasePage | Opportunity | Opportunity list page |
| `login.page.ts` | BasePage | Auth | Login form |
| `dashboard.page.ts` | BasePage | Dashboard | Dashboard widgets |
| `admin.page.ts` | BasePage | Admin | Admin features |
| `ai-assistant.page.ts` | BasePage | AI | AI assistant panel |
| `partner-tree.page.ts` | BasePage | Partner | Partner hierarchy tree |
| `workflow.page.ts` | BasePage | Workflow | Workflow stage management |
| `profile.page.ts` | BasePage | User | Profile settings |

### 4.3 Helpers (10 total)

| Helper File | Purpose |
|---|---|
| `auth.helper.ts` | `authenticateWithRealBackend()`, `login()`, `loginAndNavigate()` |
| `api-mocks.helper.ts` | `setupAPIMocks()`, `clearAPIMocks()`, `setupCameraMocks()` |
| `assertions.helper.ts` | `assertUrlMatches()`, `assertDialogOpen()` |
| `wait.helper.ts` | `waitForPageReady()`, `waitForAngularReady()` |
| `role-test.helper.ts` | `setupUserRoleMock()`, role-specific authentication |
| `navigation.helper.ts` | Navigation utilities |
| `oup-integration.helper.ts` | oUP integration test helpers |
| `test-config.ts` | `getTestCredentials()`, `getTimeout()` |
| `test-data-seeder.ts` | Test data seeding utilities |
| `test-data-builder.ts` | Test data builder patterns |

---

## 5. Configuration Requirements

### 5.1 Playwright Config (`playwright.config.ts`)

| Setting | Value | Notes |
|---|---|---|
| `testDir` | `QA Tests/Playwright Tests` | Relative to repo root |
| `timeout` | 60,000ms (60s) | Per-test timeout |
| `expect.timeout` | 10,000ms (10s) | Assertion timeout |
| `fullyParallel` | `true` | Tests run in parallel within files |
| `workers` | 2 (local) / 4 (CI) | Limits concurrent browser instances |
| `maxFailures` | 20 (local) / 50 (CI) | Stops run early on systemic failure |
| `retries` | 0 (local) / 1 (CI) | CI retries once on failure |
| `trace` | `on-first-retry` | Captures trace on retry |
| `screenshot` | `only-on-failure` | Captures screenshot on failure |
| `video` | `off` (default) | Disabled for memory (enable via `PLAYWRIGHT_VIDEO`) |
| `navigationTimeout` | 30,000ms | Page navigation timeout |
| `actionTimeout` | 15,000ms | Click/fill/etc timeout |
| `NODE_OPTIONS` | `--max-old-space-size=4096` | Auto-set for heap headroom |

### 5.2 Browser Projects

| Project | Browser | Special Config |
|---|---|---|
| `chromium` | Desktop Chrome | Default timeouts |
| `firefox` | Desktop Firefox | Default timeouts |
| `webkit` | Desktop Safari | 2x navigation timeout, 3min test timeout |

### 5.3 Web Server (auto-started)

| Setting | Value |
|---|---|
| Command | `npx ng serve --port 4200 --host 127.0.0.1 --no-open` |
| Working directory | `UNOPS.PAO.ClientApp/` |
| URL | `http://127.0.0.1:4200` |
| Startup timeout | 360,000ms (6 minutes) |
| Reuse existing | Yes (local) / No (CI) |

---

## 6. Running Tests

### 6.1 Full Suite

```powershell
# Run all chromium tests (994 tests, ~39 minutes)
npx playwright test --project=chromium

# With verbose reporter
npx playwright test --project=chromium --reporter=list
```

### 6.2 Specific Files

```powershell
# Run a single spec file
npx playwright test partner-item.spec.ts --project=chromium

# Run multiple spec files
npx playwright test contact-item.spec.ts interaction-item.spec.ts --project=chromium

# Run by grep pattern
npx playwright test --grep "should display" --project=chromium
```

### 6.3 Debugging

```powershell
# Headed mode (see the browser)
npx playwright test partner-item.spec.ts --headed

# Debug mode (step through)
npx playwright test partner-item.spec.ts --debug

# With verbose mock logging
$env:PLAYWRIGHT_DEBUG_MOCKS='true'; npx playwright test partner-item.spec.ts --project=chromium

# With video recording
$env:PLAYWRIGHT_VIDEO='retain-on-failure'; npx playwright test partner-item.spec.ts --project=chromium

# View HTML report
npx playwright show-report "QA Tests/Playwright Tests/playwright-report"
```

### 6.4 CI/CD

```bash
# CI invocation (auto-detects CI env)
CI=true npx playwright test --project=chromium

# With sharding for parallel CI jobs
npx playwright test --project=chromium --shard=1/2
npx playwright test --project=chromium --shard=2/2
```

---

## 7. Selector Strategy

### 7.1 Priority Order

Per [Playwright best practices](https://playwright.dev/docs/locators#quick-guide):

1. **`data-testid`** — Preferred, stable across refactors
2. **ARIA role** — `getByRole('button', { name: 'Submit' })`
3. **Component selector** — `app-workflow`, `app-contact-tabs`
4. **CSS class** — `.text-2xl.font-bold`, `#section-what`
5. **Text content** — `getByText('Submit for Review')`

### 7.2 Resilient Selector Pattern

Page objects use multi-selector fallbacks for elements without `data-testid`:

```typescript
// Primary: data-testid, Fallback: component selector + CSS
get contactName(): Locator {
  return this.page.locator(
    '[data-testid="contact-name"], app-contact-tabs .text-2xl.font-bold'
  ).first();
}
```

### 7.3 Known data-testid Coverage

| Entity | Has data-testid | Uses Fallback Selectors |
|---|---|---|
| Partner detail | Header, edit/delete buttons, partner-type, links, documents | Name, category, status (CSS) |
| Contact detail | Header, email, phone, mobile, partner-link, status, documents, links | Name, title, department (CSS) |
| Interaction detail | Header, type-icon, date, location, status, description, contacts, partners | Type text (CSS), opportunities (text) |
| Opportunity detail | Header, title, stage, status, metadata, ID, manager, orgunit, signing-date | Value, dates, sections (IDs), workflow buttons (text) |

---

## 8. Known Issues & Defects

### 8.1 Open QA Issues (affecting tests)

| ID | Issue | Impact |
|---|---|---|
| QA-007 | Business Card Scanner signal not set | Camera mock workaround applied |
| QA-008 | DynamicDialog not interceptable | 1 contact test skipped |
| QA-014 | oUP integration needs credentials | oUP tests require env config |
| QA-015 | Go to oUP button not testable | Frontend not implemented in non-prod |
| QA-019 | Async wrappers incomplete | C# test limitation |
| QA-020 | Stateful mock tracking needed | C# test limitation |

### 8.2 Recently Resolved

| ID | Issue | Resolution Date |
|---|---|---|
| QA-036 | Non-existent data-testid selectors | 2026-02-12 |
| QA-041 | Full suite OOM crash after ~287 tests | 2026-02-12 |

---

## 9. Test Data Requirements

### 9.1 Mock Data (Default)

All tests use mock data injected via API route intercepts. No external database or backend required. Mock data includes:

- **3 partners** (UNICEF, Red Cross, World Bank)
- **3 contacts** (John Smith, Jane Doe, Bob Johnson)
- **3 interactions** (Meeting, Call, Visit)
- **3 opportunities** (Infrastructure, Education, Healthcare)
- **Dropdown data** (salutations, countries, org units, etc.)
- **Permission data** (full admin for default user, restricted for test users)
- **Workflow data** (Draft/Active stages with actions)

### 9.2 Real Backend Data (Optional)

For tests requiring real backend (`login.spec.ts`, etc.):

```powershell
# Create test user
psql -h localhost -U test -d TestDb -f setup-test-user.sql

# Add opportunity permissions
psql -h localhost -U test -d TestDb -f setup-opportunity-permissions.sql

# Verify setup
psql -h localhost -U test -d TestDb -f verify-users.sql
```

---

## 10. Maintenance & Conventions

### 10.1 Adding New Tests

1. Create spec file following naming convention: `{feature}.spec.ts` or `{entity}-{scope}.spec.ts`
2. Use `authenticateWithRealBackend()` in `beforeEach`
3. Use page objects for element interaction (create new page object if needed)
4. Prefer `data-testid` selectors; use fallback pattern for missing ones
5. Use `test.skip(!condition, 'reason')` for conditionally skipped tests
6. Keep `console.log` minimal — use `mockLog()`/`authLog()` patterns if needed

### 10.2 Adding New Page Objects

1. Extend `EntityDetailPage` (for detail pages) or `BasePage` (for other pages)
2. Place in `pages/` directory with naming: `{entity-name}.page.ts` or `{entity-name}-item.page.ts`
3. Use resilient selectors (data-testid primary, CSS/component fallback)
4. Document which `data-testid` attributes are real vs which use fallback selectors

### 10.3 Updating API Mocks

1. Add new mock routes to `helpers/api-mocks.helper.ts`
2. Use `mockLog()` instead of `console.log()` for all mock logging
3. If adding a new specific route, add its URL pattern to the catch-all exclusion list
4. Test that the catch-all doesn't intercept your new specific route

---

*This document was created 2026-02-12 to fulfill the documentation requirement referenced in the QA defect list.*
