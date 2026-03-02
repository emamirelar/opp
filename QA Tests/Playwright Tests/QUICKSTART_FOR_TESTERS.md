# Playwright for Testers — Quickstart Guide

**Audience:** QA testers with little or no coding experience  
**Goal:** Run, read, modify, and write basic Playwright E2E tests  
**Time to get started:** ~30 minutes

---

## Table of Contents

1. [What is Playwright?](#1-what-is-playwright)
2. [Setup (One-Time)](#2-setup-one-time)
3. [Running Tests](#3-running-tests)
4. [Reading a Test (Anatomy)](#4-reading-a-test-anatomy)
5. [Your First Test](#5-your-first-test)
6. [Finding Elements on the Page](#6-finding-elements-on-the-page)
7. [Common Actions](#7-common-actions)
8. [Common Checks (Assertions)](#8-common-checks-assertions)
9. [Working with Our App (PrimeNG + Angular)](#9-working-with-our-app-primeng--angular)
10. [Using the Recorder (Codegen)](#10-using-the-recorder-codegen)
11. [Using AI to Write Tests](#11-using-ai-to-write-tests)
12. [Debugging a Failing Test](#12-debugging-a-failing-test)
13. [Cheat Sheet](#13-cheat-sheet)
14. [Glossary](#14-glossary)
15. [Getting Help](#15-getting-help)

---

## 1. What is Playwright?

Playwright is a tool that **automates a web browser**. It opens a browser, clicks buttons, fills forms, and checks that the right things appear on screen — exactly like a human tester would, but faster and repeatable.

### Katalon vs Playwright — Key Differences

| | Katalon | Playwright |
|---|---|---|
| **How you write tests** | Click-and-record, drag-and-drop keywords | Write code in TypeScript |
| **Where tests live** | Katalon Studio application | Text files (`.spec.ts`) in the project |
| **How you run tests** | Click "Run" in Katalon Studio | Type a command in the terminal |
| **How you find elements** | Object Repository (GUI) | Selectors in code (`data-testid`) |
| **Team sharing** | Export/import projects | Git (same as developer code) |

**The good news:** Our project has helpers that handle the hard parts (login, API mocking, waiting for pages to load). You only need to learn 3 things:

1. **Navigate** — go to a page
2. **Interact** — click, type, select
3. **Check** — verify something is visible, has the right text, etc.

---

## 2. Setup (One-Time)

### Prerequisites

| Software | Version | Download |
|---|---|---|
| Node.js | 20 or newer | https://nodejs.org/ |
| Git | Any recent | https://git-scm.com/ |
| VS Code or Cursor | Any recent | Your IDE |

### Installation steps

Open a terminal (PowerShell or Command Prompt) and run these commands one at a time:

```bash
# 1. Go to the project folder
cd c:\Users\YourName\git\opportunityplus

# 2. Go to the QA Tests folder
cd "QA Tests"

# 3. Install Playwright and dependencies
npm install

# 4. Install browser engines (Chromium, Firefox, WebKit)
npx playwright install
```

### Environment file

Copy the example environment file:

```bash
cd "Playwright Tests"
copy .env.example .env
```

You can leave the defaults — tests use API mocks by default and don't need a real backend.

### Verify it works

```bash
cd "QA Tests"
npx playwright test home.spec.ts --project=chromium
```

You should see output like:

```
Running 8 tests using 2 workers
  ✓ Home Page & Dashboard > should load home page (5.2s)
  ✓ Home Page & Dashboard > should display announcement banner (3.1s)
  ...
  8 passed (25.3s)
```

You're ready to go.

---

## 3. Running Tests

All commands are run from the `QA Tests` folder:

```bash
cd "QA Tests"
```

### Most common commands

| What you want to do | Command |
|---|---|
| Run ALL tests (all browsers) | `npx playwright test` |
| Run ALL tests (Chrome only) | `npx playwright test --project=chromium` |
| Run ONE spec file | `npx playwright test partners.spec.ts --project=chromium` |
| Run with VISIBLE browser | `npx playwright test partners.spec.ts --project=chromium --headed` |
| Run ONE specific test by name | `npx playwright test -g "should display partner list" --project=chromium` |
| Open INTERACTIVE mode | `npx playwright test --ui` |
| See the HTML report | `npx playwright show-report TestResults/playwright-html-report` |

### Understanding test output

```
  ✓  1 [chromium] > partners.spec.ts:25:5 > should display partner list (4.2s)     ← PASSED
  ✗  2 [chromium] > partners.spec.ts:40:5 > should create new partner (12.1s)       ← FAILED
  -  3 [chromium] > partners.spec.ts:60:5 > should delete partner (skipped)          ← SKIPPED
```

- **✓ Passed** — test worked as expected
- **✗ Failed** — something didn't match expectations (see error details below)
- **- Skipped** — test was deliberately skipped (usually because a feature isn't ready)

### Viewing the HTML report

After running tests, open the visual report:

```bash
npx playwright show-report TestResults/playwright-html-report
```

This opens a browser with:
- Pass/fail summary per browser
- Click any failed test to see the error, screenshot, and video
- Filter by status, browser, or search by test name

---

## 4. Reading a Test (Anatomy)

Here's a real test from our project, broken down line by line:

```typescript
// ─── IMPORTS ────────────────────────────────────────────────
// These load the tools we need. You'll always see these at the top.
import { test, expect } from '@playwright/test';                    // Playwright itself
import { authenticateWithRealBackend } from './helpers/auth.helper'; // Our login helper
import { waitForPermissions } from './helpers/wait.helper';          // Our wait helper

// ─── TEST GROUP ─────────────────────────────────────────────
// test.describe() groups related tests together. Think of it as a folder.
test.describe('Partner List Page', () => {

  // test.slow() gives tests more time (3x the normal timeout).
  // Use this when tests load pages with API calls.
  test.slow();

  // beforeEach runs BEFORE every single test in this group.
  // We use it to log in and navigate to the right page.
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
  });

  // ─── A SINGLE TEST ──────────────────────────────────────
  // test() defines one test case. The string is the test name.
  test('should display the partner list with data', async ({ page }) => {

    // ARRANGE — set up the conditions
    // (already done in beforeEach — we're on the partner list page)

    // ACT — do something (in this case, just wait for the table)
    await page.waitForLoadState('networkidle');

    // ASSERT — check that the right thing happened
    await expect(page.locator('p-table')).toBeVisible();
  });
});
```

### The pattern every test follows

```
1. ARRANGE  →  Set up the page (login, navigate, prepare data)
2. ACT      →  Do something (click, type, select)
3. ASSERT   →  Check that the result is correct
```

This is the same as manual testing:
1. **Go to** the partner list page
2. **Click** the "New Partner" button
3. **Verify** the create dialog appears

---

## 5. Your First Test

Let's write a test together. We'll verify that the partner list page loads correctly.

### Step 1: Create the file

Create a new file at:
```
QA Tests/Playwright Tests/my-first-test.spec.ts
```

### Step 2: Write the test

Copy and paste this:

```typescript
import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

test.describe('My First Test', () => {
  test.slow();

  test('Partner list page loads successfully', async ({ page }) => {
    // 1. ARRANGE — Log in and go to the partner list page
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);

    // 2. ACT — Wait for the page to finish loading
    await page.waitForLoadState('networkidle');

    // 3. ASSERT — Check that the page title is visible
    const pageTitle = page.locator('h1, h2, [data-testid="partners-title"]').first();
    await expect(pageTitle).toBeVisible();
  });
});
```

### Step 3: Run it

```bash
cd "QA Tests"
npx playwright test my-first-test.spec.ts --project=chromium --headed
```

The `--headed` flag opens a visible browser so you can watch your test run.

### Step 4: Celebrate

You just wrote and ran a Playwright test. Delete `my-first-test.spec.ts` when you're done practicing — it was just for learning.

---

## 6. Finding Elements on the Page

To interact with something on the page, you need to tell Playwright how to find it. This is called a **selector** or **locator**.

### Method 1: `data-testid` (preferred)

Our app adds special `data-testid` attributes to important elements. This is the most reliable way to find things:

```typescript
// Find an element by its test ID
page.locator('[data-testid="partner-edit-button"]')

// Find a text input inside a test ID container
page.locator('[data-testid="partner-name"] input')
```

### Method 2: Text content

Find an element by the text it displays:

```typescript
// Find a button with specific text
page.locator('button', { hasText: 'Save' })

// Find any element containing text
page.getByText('Partner Name')

// Find a heading
page.getByRole('heading', { name: 'Partners' })
```

### Method 3: Role-based (accessible)

Find elements by their role (button, link, heading, etc.):

```typescript
page.getByRole('button', { name: 'Save' })
page.getByRole('link', { name: 'View Details' })
page.getByRole('textbox', { name: 'Email' })
```

### How to discover selectors

**Option A — Browser DevTools:**

1. Open Chrome
2. Right-click on an element → "Inspect"
3. Look for `data-testid="..."` in the HTML
4. Use that value in your test

**Option B — Playwright Inspector:**

```bash
cd "QA Tests"
npx playwright codegen http://localhost:4200
```

This opens a browser + a code panel. Click on any element and Playwright shows you the selector.

**Option C — Playwright UI Mode:**

```bash
cd "QA Tests"
npx playwright test --ui
```

This opens an interactive runner where you can explore the page and pick selectors.

---

## 7. Common Actions

Here's a reference card of things you can do in a test:

### Navigate

```typescript
// Go to a page
await page.goto('/partnerships/partners');

// Wait for the page to load
await page.waitForLoadState('networkidle');

// Click a link that navigates
await page.locator('[data-testid="view-details-link"]').click();
```

### Click

```typescript
// Click a button
await page.locator('[data-testid="save-button"]').click();

// Double-click
await page.locator('[data-testid="row-1"]').dblclick();
```

### Type

```typescript
// Type into a text field
await page.locator('[data-testid="name-field"] input').fill('Test Partner');

// Clear a field first, then type
await page.locator('[data-testid="name-field"] input').clear();
await page.locator('[data-testid="name-field"] input').fill('New Name');

// Type one key at a time (for autocomplete)
await page.locator('[data-testid="search-field"] input').pressSequentially('UNICEF');
```

### Select from dropdown

```typescript
// Click the dropdown to open it
await page.locator('[data-testid="country-select"]').click();

// Click an option
await page.locator('.p-select-option', { hasText: 'United States' }).click();
```

### Wait

```typescript
// Wait for page load
await page.waitForLoadState('networkidle');

// Wait for a specific element to appear
await page.locator('[data-testid="results"]').waitFor({ state: 'visible' });

// Wait a fixed time (use sparingly — only when other waits don't work)
await page.waitForTimeout(2000);  // 2 seconds
```

### Keyboard

```typescript
// Press Enter
await page.keyboard.press('Enter');

// Press Escape (close a dialog)
await page.keyboard.press('Escape');

// Press Tab (move to next field)
await page.keyboard.press('Tab');
```

---

## 8. Common Checks (Assertions)

After performing an action, you check that the right thing happened:

### Element is visible / hidden

```typescript
// Check something IS visible
await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

// Check something is NOT visible
await expect(page.locator('[data-testid="error-message"]')).not.toBeVisible();

// Check something is NOT visible (alternative — "hidden")
await expect(page.locator('[data-testid="delete-button"]')).toBeHidden();
```

### Text content

```typescript
// Check element contains specific text
await expect(page.locator('[data-testid="partner-name"]')).toContainText('UNICEF');

// Check element has exact text
await expect(page.locator('[data-testid="status-badge"]')).toHaveText('Active');
```

### URL

```typescript
// Check we're on the right page
await expect(page).toHaveURL(/\/partnerships\/partners/);
```

### Input value

```typescript
// Check an input field has a value
await expect(page.locator('[data-testid="name-field"] input')).toHaveValue('Test Partner');
```

### Enabled / disabled

```typescript
// Check button is enabled
await expect(page.locator('[data-testid="save-button"]')).toBeEnabled();

// Check button is disabled
await expect(page.locator('[data-testid="save-button"]')).toBeDisabled();
```

### Count

```typescript
// Check that a table has 3 rows
await expect(page.locator('tbody tr')).toHaveCount(3);

// Check that there is at least 1 result
const count = await page.locator('tbody tr').count();
expect(count).toBeGreaterThan(0);
```

---

## 9. Working with Our App (PrimeNG + Angular)

Our app uses PrimeNG components (special UI widgets). Here's how to interact with them:

### PrimeNG Float Label Input

```typescript
// Fill a float-label input field
await page.locator('[data-testid="partner-name"] input').fill('My Partner');
```

### PrimeNG Dropdown (`p-select`)

```typescript
// Step 1: Click the dropdown to open it
await page.locator('[data-testid="country-select"]').click();

// Step 2: Click the option you want
await page.locator('.p-select-option', { hasText: 'United States' }).click();
```

### PrimeNG Dialog (Modal popup)

```typescript
// Wait for dialog to open
await expect(page.locator('.p-dialog')).toBeVisible();

// Fill a field inside the dialog
await page.locator('.p-dialog [data-testid="name-field"] input').fill('Test');

// Click the Save button inside the dialog
await page.locator('.p-dialog-footer button', { hasText: 'Save' }).click();

// Verify dialog closed
await expect(page.locator('.p-dialog')).not.toBeVisible();
```

### PrimeNG Table (`p-table`)

```typescript
// Check table is visible
await expect(page.locator('p-table')).toBeVisible();

// Count rows
const rowCount = await page.locator('tbody tr').count();
expect(rowCount).toBeGreaterThan(0);

// Click a specific row
await page.locator('tbody tr').first().click();
```

### PrimeNG Toast (Success/Error message)

```typescript
// Check success toast appeared
await expect(page.locator('.p-toast-message-success')).toBeVisible();

// Check error toast appeared
await expect(page.locator('.p-toast-message-error')).toBeVisible();
```

### Confirmation Dialog

```typescript
// After clicking Delete, a confirmation dialog appears
await page.locator('[data-testid="delete-button"]').click();

// Click "Yes" to confirm
await page.locator('.p-confirmdialog-accept-button, button', { hasText: /yes|confirm|ok/i }).click();
```

### Logging in as different users

```typescript
// Admin user (full access)
await authenticateWithRealBackend(page, '/partnerships/partners', 'test@playwright.local');

// Read-only user (can view but not edit)
await authenticateWithRealBackend(page, '/partnerships/partners', 'test-readonly@playwright.local');

// Collaborator (can edit content but not perform workflow actions)
await authenticateWithRealBackend(page, '/partnerships/partners', 'collaborator@example.com');

// User with no permissions
await authenticateWithRealBackend(page, '/partnerships/partners', 'test-no-permissions@playwright.local');
```

---

## 10. Using the Recorder (Codegen)

Playwright has a **visual recorder** similar to Katalon's recorder. It watches you click around the app and generates code.

### How to use it

```bash
cd "QA Tests"
npx playwright codegen http://localhost:4200
```

This opens:
1. **A browser window** — interact with the app normally
2. **A code panel** — shows the generated TypeScript code

Click, type, and navigate as you would normally test. The code panel updates in real time.

### What you get

The recorder generates code like:

```typescript
await page.goto('http://localhost:4200/partnerships/partners');
await page.locator('.p-datatable-tbody > tr:nth-child(1)').click();
await page.getByRole('button', { name: 'Edit' }).click();
```

### What you need to fix

The recorder's output needs a few adjustments:

1. **Add auth** — the recorder doesn't handle our login flow. Replace `page.goto()` with `authenticateWithRealBackend()`
2. **Replace CSS selectors** — change `.p-datatable-tbody > tr:nth-child(1)` to `[data-testid="..."]`
3. **Add waits** — add `waitForPermissions()` after navigation
4. **Wrap in test structure** — put the code inside `test()` and `test.describe()`

### Recorder workflow

1. Record your manual test steps
2. Copy the generated code
3. Paste into a `.spec.ts` file
4. Clean up selectors and add our helpers
5. Run to verify

---

## 11. Using AI to Write Tests

The fastest way to create tests is to **describe what you want in plain English** and let the AI agent generate the code.

### In Cursor

Just type a message like:

> "Create a Playwright test that verifies a read-only user cannot see the Edit button on the partner detail page"

The AI will produce a complete spec file using our project's helpers, mocks, and patterns.

### Example prompts

| What you want | What to ask |
|---|---|
| Test a new feature | "Write Playwright tests for the new notification panel" |
| Test permissions | "Create E2E tests verifying readonly users can't edit contacts" |
| Test a workflow | "Write tests for the opportunity Go Decision submit and cancel flow" |
| Test form validation | "Create tests for partner creation form — required fields, invalid email" |
| Convert a manual test case | "Convert this test case to Playwright: TC-042 — Admin creates a new contact with all required fields filled, verifies success toast and redirect to detail page" |

### Converting Katalon test cases

If you have a Katalon test case, describe the steps to the AI:

> "Convert this Katalon test to Playwright:
> 1. Open browser to partner list page
> 2. Click New Partner button
> 3. Fill Name field with 'Test Partner'
> 4. Select Country 'United States'
> 5. Click Save
> 6. Verify success message appears
> 7. Verify redirect to partner detail page"

The AI will produce a working Playwright spec file.

---

## 12. Debugging a Failing Test

When a test fails, here's how to figure out why:

### Step 1: Read the error message

The terminal output tells you:
- **Which test failed** — the test name and file
- **What went wrong** — the assertion or timeout error
- **Where it failed** — the line number in the spec file

Example:
```
Error: expect(locator).toBeVisible()
  Locator: locator('[data-testid="edit-button"]')
  Expected: visible
  Received: hidden
  Call log:
    - waiting for locator('[data-testid="edit-button"]')
```

This means the test expected an "Edit" button to be visible, but it wasn't on the page.

### Step 2: Run with visible browser

```bash
npx playwright test failing-test.spec.ts --project=chromium --headed
```

Watch what happens — you might see the page didn't load, or a dialog is blocking, or the element has a different name.

### Step 3: Check the HTML report

```bash
npx playwright show-report TestResults/playwright-html-report
```

Click the failed test to see:
- **Screenshot** at the moment of failure
- **Video** of the entire test run
- **Trace** (timeline of every action, network call, and DOM change)

### Step 4: Run with trace viewer

```bash
npx playwright test failing-test.spec.ts --project=chromium --trace on
```

Then open the trace:
```bash
npx playwright show-trace TestResults/playwright-artifacts/trace.zip
```

The trace viewer shows a timeline with:
- Every click, type, and navigation
- Screenshots at each step
- Network requests
- Console logs

### Common failure causes and fixes

| Error | Likely cause | Fix |
|---|---|---|
| `Timeout waiting for locator` | Element doesn't exist or has a different selector | Check the `data-testid` in the browser DevTools |
| `strict mode violation: locator resolved to X elements` | Selector matches multiple elements | Make the selector more specific (add `.first()` or a more unique `data-testid`) |
| `page.goto: net::ERR_CONNECTION_REFUSED` | Angular dev server is not running | Start it: `cd UNOPS.PAO.ClientApp && ng serve` |
| `Expected "Active" but received "Draft"` | Test data doesn't match expectations | Check the mock data in `api-mocks.helper.ts` |
| Test passes locally, fails in CI | Timing difference | Add explicit waits (`waitForPermissions`, `waitForLoadState`) |

---

## 13. Cheat Sheet

Print this page and keep it at your desk.

### File template (copy-paste starter)

```typescript
import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

test.describe('Feature Name', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/your/route', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: should do something', async ({ page }) => {
    // ACT
    await page.locator('[data-testid="some-button"]').click();

    // ASSERT
    await expect(page.locator('[data-testid="result"]')).toBeVisible();
  });
});
```

### Quick reference

| I want to... | Code |
|---|---|
| Log in as admin | `await authenticateWithRealBackend(page, '/route', 'test@playwright.local');` |
| Log in as readonly | `await authenticateWithRealBackend(page, '/route', 'test-readonly@playwright.local');` |
| Wait for page ready | `await waitForPermissions(page);` |
| Click a button | `await page.locator('[data-testid="btn"]').click();` |
| Type in a field | `await page.locator('[data-testid="field"] input').fill('text');` |
| Open a dropdown | `await page.locator('[data-testid="select"]').click();` |
| Pick dropdown option | `await page.locator('.p-select-option', { hasText: 'Option' }).click();` |
| Check visible | `await expect(page.locator('[data-testid="el"]')).toBeVisible();` |
| Check hidden | `await expect(page.locator('[data-testid="el"]')).not.toBeVisible();` |
| Check text | `await expect(page.locator('[data-testid="el"]')).toContainText('text');` |
| Check URL | `await expect(page).toHaveURL(/\/expected-path/);` |
| Check disabled | `await expect(page.locator('[data-testid="btn"]')).toBeDisabled();` |
| Wait for element | `await page.locator('[data-testid="el"]').waitFor({ state: 'visible' });` |
| Run one test | `npx playwright test file.spec.ts --project=chromium` |
| Run with browser | `npx playwright test file.spec.ts --project=chromium --headed` |
| See report | `npx playwright show-report TestResults/playwright-html-report` |
| Record a test | `npx playwright codegen http://localhost:4200` |

### Available test users

| User | Email | What they can do |
|---|---|---|
| Admin | `test@playwright.local` | Everything |
| Read-only | `test-readonly@playwright.local` | View only, no edit/delete |
| No permissions | `test-no-permissions@playwright.local` | Very limited access |
| Collaborator | `collaborator@example.com` | Edit content, no workflow actions |
| Viewer | `viewer@example.com` | View only |
| Other user | `other-user@example.com` | View only |

### Main app routes

| Page | Route |
|---|---|
| Home / Dashboard | `/` or `/home` |
| Partner list | `/partnerships/partners` |
| Partner detail | `/partnerships/partners/{id}` |
| Contact list | `/partnerships/contacts` |
| Contact detail | `/partnerships/contacts/{id}` |
| Interaction list | `/partnerships/interactions` |
| Interaction detail | `/partnerships/interactions/{id}` |
| Opportunity list | `/partnerships/opportunities` |
| Opportunity detail | `/partnerships/opportunities/{id}` |
| Admin - Entity Manager | `/admin/entity-manager` |
| Admin - User Management | `/admin/user-management` |
| Admin - Translations | `/admin/translations` |
| Login | `/login` |

---

## 14. Glossary

| Term | Meaning |
|---|---|
| **Spec file** | A test file (e.g., `partners.spec.ts`). Contains one or more tests. |
| **Test / test case** | A single scenario that checks one thing (e.g., "partner list loads"). |
| **Describe block** | A group of related tests — like a test suite or folder. |
| **Locator** | How Playwright finds an element on the page (e.g., `[data-testid="save-button"]`). |
| **Selector** | Same as locator — the address of an element. |
| **Assertion** | A check — "I expect this element to be visible." Uses `expect()`. |
| **data-testid** | A special HTML attribute our developers add to elements so tests can find them reliably. |
| **POM (Page Object Model)** | A helper file that groups all locators and actions for one page. Lives in `pages/`. |
| **Mock** | Fake API data. Our tests don't need a real backend — `api-mocks.helper.ts` provides fake responses. |
| **Headed** | Running with a visible browser window (you can watch). |
| **Headless** | Running without a visible browser (faster, used in CI). |
| **CI** | Continuous Integration — automated test runs on the server after code changes. |
| **Flaky test** | A test that sometimes passes and sometimes fails (unreliable). |
| **Timeout** | The maximum time Playwright waits before giving up. Default: 30 seconds per test. |
| **Feature gate** | An environment variable that skips tests for features not yet deployed. |
| **beforeEach** | Code that runs before EVERY test in a group (usually login + navigation). |
| **async / await** | TypeScript keywords that mean "wait for this to finish before continuing." You'll see them everywhere — just always include them. |

---

## 15. Getting Help

### Resources

| Resource | Location |
|---|---|
| This guide | `QA Tests/Playwright Tests/QUICKSTART_FOR_TESTERS.md` |
| Detailed requirements | `QA Tests/Playwright Tests/PLAYWRIGHT_TEST_REQUIREMENTS.md` |
| Playwright official docs | https://playwright.dev/docs/intro |
| PrimeNG component docs | https://primeng.org/ |

### Existing test examples

| What you want to test | Look at this spec file |
|---|---|
| Page loads correctly | `home.spec.ts` |
| Login flow | `login.spec.ts` |
| List page with table | `partners.spec.ts`, `contacts.spec.ts` |
| Detail page with tabs | `partner-item-basic.spec.ts`, `contact-item-basic.spec.ts` |
| Form validation | `form-validation.spec.ts` |
| Workflow actions | `go-decision.spec.ts` |
| Role-based access | `role-access-control.spec.ts` |
| Search functionality | `deep-search.spec.ts` |
| Document management | `document-management.spec.ts` |

### When you're stuck

1. **Read an existing spec file** that tests something similar to what you need
2. **Use the recorder** (`npx playwright codegen`) to generate starter code
3. **Ask the AI** in Cursor to generate the test from a plain English description
4. **Check the HTML report** for screenshots and traces of failed tests
5. **Ask a developer** to add a `data-testid` if the element you need doesn't have one
