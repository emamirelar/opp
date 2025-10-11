# Angular Testing Frameworks Guide

**Project**: UNOPS Opportunity Plus  
**Date**: October 11, 2025  
**Purpose**: Understanding the testing stack and why multiple frameworks are needed

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [The Testing Pyramid](#the-testing-pyramid)
3. [Framework #1: Karma + Jasmine (Unit Testing)](#framework-1-karma--jasmine-unit-testing)
4. [Framework #2: Playwright (E2E Testing)](#framework-2-playwright-e2e-testing)
5. [Why You Need Both](#why-you-need-both)
6. [Test Distribution Strategy](#test-distribution-strategy)
7. [Current Configuration](#current-configuration)
8. [How They Work Together in CI/CD](#how-they-work-together-in-cicd)
9. [Developer Workflow](#developer-workflow)
10. [Can You Use Just One?](#can-you-use-just-one)
11. [Setup Requirements](#setup-requirements)
12. [Real-World Example](#real-world-example)
13. [Quick Reference](#quick-reference)

---

## Overview

Your Angular application needs **two types of testing frameworks** because they serve completely different purposes:

1. **Karma + Jasmine**: Unit testing (tests individual pieces of code)
2. **Playwright**: End-to-End testing (tests complete user workflows)

**Think of it like building safety:**
- **Unit tests** = Check each brick is solid
- **E2E tests** = Check the whole house doesn't collapse

---

## The Testing Pyramid

```
┌─────────────────────────────────────────────────┐
│              Testing Pyramid                     │
├─────────────────────────────────────────────────┤
│                                                  │
│              E2E Tests (Few)                     │
│           ▲  Playwright ← Headless Browser      │
│          ╱ ╲  ~10% of tests                     │
│         ╱   ╲   Integration Tests (Some)        │
│        ╱     ╲  ~20% of tests                   │
│       ╱       ╲  Unit Tests (Many)              │
│      ╱         ╲ Karma + Jasmine                │
│     ╱___________╲ ~70% of tests                 │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Key Principle**: More tests at the bottom (fast, cheap) and fewer at the top (slow, expensive)

---

## Framework #1: Karma + Jasmine (Unit Testing)

### What It Does

Tests **individual units** of code in isolation (components, services, pipes, guards, directives, interceptors)

### Components

#### Karma
- **Role**: Test runner
- **Function**: Launches browsers, runs tests, reports results
- **Browsers**: Chrome (headless), Firefox, Safari, etc.

#### Jasmine
- **Role**: Testing framework
- **Function**: Provides test syntax and assertions
- **Syntax**: `describe()`, `it()`, `expect()`, `beforeEach()`, etc.

### How It Works

```
Developer writes test → Karma starts browser → 
Jasmine runs tests → Results displayed → Coverage calculated
```

1. You write tests using Jasmine syntax
2. Karma launches a real browser (usually Chrome Headless)
3. Tests execute in the browser's JavaScript engine
4. Each test runs in milliseconds
5. Coverage report generated (Istanbul)

### What It Tests

✅ **Services**
- Methods work correctly
- HTTP calls are made properly (mocked)
- Error handling works
- State management functions

✅ **Components**
- Properties are initialized
- Methods behave correctly
- Input/Output bindings work
- Template rendering is correct
- User interactions trigger expected behavior

✅ **Pipes**
- Data transformation is correct
- Edge cases handled (null, undefined, empty)

✅ **Guards**
- Routes are protected correctly
- Redirects happen as expected

✅ **Interceptors**
- Requests are modified correctly
- Responses are handled properly

✅ **Directives**
- DOM manipulation works
- Behavior is applied correctly

### Example Test

```typescript
// contact.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ContactService } from './contact.service';

describe('ContactService', () => {
  let service: ContactService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ContactService]
    });
    service = TestBed.inject(ContactService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Ensure no outstanding HTTP requests
  });

  it('should fetch contacts from API', () => {
    const mockContacts = [
      { id: 1, name: 'John Doe', email: 'john@example.com' },
      { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
    ];

    service.getContacts().subscribe(contacts => {
      expect(contacts).toEqual(mockContacts);
      expect(contacts.length).toBe(2);
    });

    const req = httpMock.expectOne('/api/contacts');
    expect(req.request.method).toBe('GET');
    req.flush(mockContacts); // Simulate HTTP response
  });

  it('should handle 404 error when fetching contacts', () => {
    service.getContacts().subscribe({
      error: (error) => {
        expect(error.status).toBe(404);
      }
    });

    const req = httpMock.expectOne('/api/contacts');
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
  });
});
```

### Configuration

**karma.conf.js**:
```javascript
module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-coverage')
    ],
    browsers: ['Chrome'],
    singleRun: false,
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage'),
      reporters: [
        { type: 'html' },
        { type: 'text-summary' },
        { type: 'lcovonly' }
      ],
      check: {
        global: {
          statements: 80,
          branches: 75,
          functions: 80,
          lines: 80
        }
      }
    }
  });
};
```

### Performance

| Metric | Value |
|--------|-------|
| **Speed** | ⚡ Very Fast (milliseconds per test) |
| **Typical Suite** | 1000+ tests in 2-5 minutes |
| **When to Run** | Every code change, continuously |
| **Parallelization** | Yes, multiple browsers simultaneously |

### Advantages

✅ **Extremely fast** - Run thousands of tests in minutes  
✅ **Isolated** - Each test is independent  
✅ **Detailed feedback** - Pinpoint exact line that failed  
✅ **Coverage metrics** - Know exactly what code is tested  
✅ **Easy to debug** - Can run single test, add breakpoints  
✅ **Cheap to maintain** - Simple mocks, clear structure  

### Disadvantages

❌ **No integration testing** - Can't verify components work together  
❌ **No real API calls** - Everything is mocked  
❌ **No user perspective** - Doesn't test actual workflows  
❌ **Can miss visual bugs** - Tests logic, not appearance  

---

## Framework #2: Playwright (E2E Testing)

### What It Does

Tests the **complete application** from a user's perspective in real browsers, simulating actual user interactions.

### How It Works

```
Developer writes scenario → Playwright launches browser → 
User actions simulated → Backend APIs called → 
Results verified → Screenshots/videos captured
```

1. Playwright launches a real browser (Chromium, Firefox, or WebKit)
2. Navigates to your application URL
3. Simulates user actions (click, type, scroll, etc.)
4. Makes **real HTTP calls** to your backend
5. Verifies expected results appear on screen
6. Captures screenshots/videos on failure

### What It Tests

✅ **Complete User Workflows**
- Login → Dashboard → Create Contact → View Contact → Logout

✅ **Navigation**
- Routes work correctly
- Breadcrumbs update
- Back button functions

✅ **Forms**
- Validation displays
- Submission works
- Error messages appear

✅ **Integration**
- Frontend + Backend working together
- Real API calls
- Database operations

✅ **Cross-Browser**
- Works in Chrome, Firefox, Safari
- Responsive design

✅ **Visual Elements**
- Buttons appear
- Modals open
- Notifications show

### Example Test

```typescript
// tests/e2e/contact-management.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Contact Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('http://localhost:44426/login');
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password123');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('/');
  });

  test('should create a new contact', async ({ page }) => {
    // Navigate to contacts page
    await page.goto('http://localhost:44426/partnerships/contacts');
    
    // Verify we're on the correct page
    await expect(page.locator('h1')).toContainText('Contacts');
    
    // Click "New Contact" button
    await page.click('[data-testid="new-contact-button"]');
    
    // Fill in contact form
    await page.fill('[data-testid="contact-name"]', 'John Doe');
    await page.fill('[data-testid="contact-email"]', 'john.doe@example.com');
    await page.fill('[data-testid="contact-phone"]', '+1234567890');
    await page.fill('[data-testid="contact-organization"]', 'ACME Corp');
    
    // Save contact
    await page.click('[data-testid="save-contact"]');
    
    // Verify success
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.success-message')).toContainText('Contact created successfully');
    
    // Verify contact appears in list
    await expect(page.locator('.contact-item')).toContainText('John Doe');
    await expect(page.locator('.contact-item')).toContainText('john.doe@example.com');
  });

  test('should edit an existing contact', async ({ page }) => {
    await page.goto('http://localhost:44426/partnerships/contacts');
    
    // Click edit on first contact
    await page.click('.contact-item:first-child [data-testid="edit-button"]');
    
    // Update name
    await page.fill('[data-testid="contact-name"]', 'Jane Doe Updated');
    await page.click('[data-testid="save-contact"]');
    
    // Verify update
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.contact-item:first-child')).toContainText('Jane Doe Updated');
  });

  test('should delete a contact', async ({ page }) => {
    await page.goto('http://localhost:44426/partnerships/contacts');
    
    const initialCount = await page.locator('.contact-item').count();
    
    // Delete first contact
    await page.click('.contact-item:first-child [data-testid="delete-button"]');
    await page.click('[data-testid="confirm-delete"]');
    
    // Verify deletion
    await expect(page.locator('.success-message')).toBeVisible();
    await expect(page.locator('.contact-item')).toHaveCount(initialCount - 1);
  });

  test('should search contacts', async ({ page }) => {
    await page.goto('http://localhost:44426/partnerships/contacts');
    
    // Enter search term
    await page.fill('[data-testid="search-input"]', 'John');
    await page.waitForTimeout(500); // Wait for debounce
    
    // Verify filtered results
    const contacts = page.locator('.contact-item');
    await expect(contacts.first()).toContainText('John');
  });

  test('should handle validation errors', async ({ page }) => {
    await page.goto('http://localhost:44426/partnerships/contacts');
    
    await page.click('[data-testid="new-contact-button"]');
    
    // Try to save without required fields
    await page.click('[data-testid="save-contact"]');
    
    // Verify validation errors
    await expect(page.locator('.error-message')).toContainText('Name is required');
    await expect(page.locator('.error-message')).toContainText('Email is required');
  });
});
```

### Configuration

**playwright.config.ts**:
```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  
  // Reporter options
  reporter: [
    ['html'],
    ['json', { outputFile: 'test-results.json' }],
    ['junit', { outputFile: 'test-results.xml' }]
  ],
  
  use: {
    baseURL: 'http://localhost:44426',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  // Test against multiple browsers
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        headless: true  // ⬅️ Headless mode for CI/CD
      },
    },
    {
      name: 'firefox',
      use: { 
        ...devices['Desktop Firefox'],
        headless: true
      },
    },
    {
      name: 'webkit',
      use: { 
        ...devices['Desktop Safari'],
        headless: true
      },
    },
    
    // Mobile viewports
    {
      name: 'Mobile Chrome',
      use: { 
        ...devices['Pixel 5'],
        headless: true
      },
    },
    {
      name: 'Mobile Safari',
      use: { 
        ...devices['iPhone 12'],
        headless: true
      },
    },
  ],

  // Start dev server before tests
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:44426',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

### Performance

| Metric | Value |
|--------|-------|
| **Speed** | 🐢 Slower (seconds per test) |
| **Typical Suite** | 10-50 tests in 5-15 minutes |
| **When to Run** | Before deployment, in CI/CD |
| **Parallelization** | Yes, but limited by resources |

### Advantages

✅ **Tests real user experience** - Exactly what users see  
✅ **Full integration** - Frontend + Backend + Database  
✅ **Cross-browser testing** - Chrome, Firefox, Safari  
✅ **No mocking needed** - Uses real APIs  
✅ **Catches integration bugs** - Components working together  
✅ **Visual regression** - Can capture screenshots  
✅ **Mobile testing** - Test responsive design  

### Disadvantages

❌ **Slow** - Takes seconds per test  
❌ **Expensive** - More effort to write and maintain  
❌ **Flaky** - Can fail due to timing issues, network  
❌ **Hard to debug** - Need to reproduce entire workflow  
❌ **No code coverage** - Doesn't measure what code ran  
❌ **Resource intensive** - Needs browsers, can't run thousands  

---

## Why You Need Both

### Comparison Table

| Aspect | Unit Tests (Karma) | E2E Tests (Playwright) |
|--------|-------------------|------------------------|
| **Purpose** | Verify individual pieces work correctly | Verify entire system works together |
| **Scope** | Single component/service | Complete user workflows |
| **Speed** | ⚡ Milliseconds per test | 🐢 Seconds per test |
| **Quantity** | 1000s of tests | 10-50 critical tests |
| **Mocking** | Everything is mocked | Nothing is mocked |
| **APIs** | Mocked HTTP calls | Real HTTP calls |
| **Database** | Not touched | Real database operations |
| **Browser** | Headless Chrome (fast) | Real browsers (Chrome, Firefox, Safari) |
| **When** | Every code change | Before each deployment |
| **Finds** | Logic bugs, edge cases | Integration issues, UX problems |
| **Cost** | 💰 Cheap to write & maintain | 💰💰💰 Expensive to write & maintain |
| **Debugging** | ⚡ Easy (single test, breakpoints) | 🐢 Hard (full workflow, screenshots) |
| **Coverage** | 📊 Measures code coverage | ❌ No coverage metrics |
| **CI/CD Time** | ~2-5 minutes | ~10-30 minutes |

### What Each Framework Catches

#### Unit Tests Catch:
```typescript
// Logic error in component
calculateTotal() {
  return this.price + this.tax; // ❌ Should multiply tax
}

// Service not handling null
getUser(id: number) {
  return this.users.find(u => u.id === id); // ❌ What if not found?
}

// Pipe edge case
transform(value: string) {
  return value.toUpperCase(); // ❌ What if value is null?
}
```

#### E2E Tests Catch:
```typescript
// Integration issue:
// - Service returns data ✅
// - Component receives data ✅  
// - But template doesn't display it ❌

// Workflow issue:
// - Login works ✅
// - Navigation works ✅
// - But auth token not sent to API ❌

// UX issue:
// - Form validates ✅
// - Form submits ✅
// - But no success message appears ❌
```

### Real-World Scenario

Imagine a bug in your contact creation flow:

**Bug**: Contact is created but doesn't appear in the list immediately

**Unit Tests**:
```
✅ ContactService.createContact() - PASS (creates contact)
✅ ContactListComponent.loadContacts() - PASS (loads contacts)
✅ ContactListComponent.ngOnInit() - PASS (called on init)
```

**Why they didn't catch it**: Each piece works in isolation, but the component doesn't refresh after creation.

**E2E Test**:
```
❌ should create a new contact - FAIL
   - Contact created ✅
   - Success message appears ✅
   - Contact in list ❌ (TIMEOUT - element not found)
```

**E2E test catches it** because it tests the complete workflow from the user's perspective.

---

## Test Distribution Strategy

### Recommended Ratio

```
Total Tests: 100%
├── 70%  Unit Tests (Karma + Jasmine)
│        → Fast feedback loop
│        → Every service, component, pipe, guard
│        → Edge cases, error handling
│        → High coverage (80%+)
│
├── 20%  Integration Tests (Optional - also Karma)
│        → Components + Services working together
│        → Module-level testing
│        → Router + Components
│
└── 10%  E2E Tests (Playwright)
         → Critical user paths ONLY
         → Login/logout flow
         → CRUD operations (Create, Read, Update, Delete)
         → Main workflows
```

### What to Test with E2E

✅ **Must Have** (Priority 1):
- User authentication (login, logout, session)
- Core CRUD operations (create/edit/delete contacts, partners)
- Navigation between main pages
- Critical forms with validation

✅ **Should Have** (Priority 2):
- Search and filtering
- Bulk operations
- File uploads
- Export/import features

❌ **Don't E2E Test**:
- Edge cases (use unit tests)
- Error handling details (use unit tests)
- Individual component behavior (use unit tests)
- Every possible combination (too expensive)

### Example Test Suite

**Unit Tests** (~700 tests):
```
Contact Feature:
├── ContactService (50 tests)
│   ├── getContacts() - success, error, empty
│   ├── getContact() - found, not found, invalid ID
│   ├── createContact() - success, validation errors, server errors
│   ├── updateContact() - success, not found, validation
│   └── deleteContact() - success, not found, cascade delete
│
├── ContactListComponent (30 tests)
│   ├── ngOnInit() - loads contacts, handles errors
│   ├── search() - filters correctly, debounces, clears
│   ├── selectContact() - emits event, updates state
│   └── deleteContact() - confirms, calls service, refreshes
│
├── ContactEditDialog (40 tests)
│   ├── Form validation - required fields, email format, phone format
│   ├── Save - calls service, emits event, closes dialog
│   └── Cancel - discards changes, closes dialog
│
└── ... (repeat for other components, pipes, etc.)
```

**E2E Tests** (~10 tests):
```
Contact Feature:
├── Create contact workflow (1 test)
├── Edit contact workflow (1 test)
├── Delete contact workflow (1 test)
├── Search contacts workflow (1 test)
└── Bulk import contacts workflow (1 test)
```

---

## Current Configuration

### What You Already Have ✅

Looking at your `package.json`:

```json
{
  "devDependencies": {
    "jasmine-core": "^5.8.0",           // ✅ Testing framework
    "karma": "^6.4.4",                   // ✅ Test runner
    "karma-chrome-launcher": "^3.2.0",   // ✅ Browser launcher
    "karma-jasmine": "^5.1.0",           // ✅ Karma-Jasmine adapter
    "karma-jasmine-html-reporter": "^2.1.0", // ✅ HTML reporter
    "karma-coverage": "^2.2.1"           // ✅ Coverage reporter
  }
}
```

**Status**: Unit testing infrastructure is **fully configured** ✅

Your `karma.conf.js` is already set up and ready to run unit tests!

### What You Need to Add ❌

```bash
# Playwright for E2E testing
npm init playwright@latest
```

This will:
1. Install Playwright (~5 minutes)
2. Install browsers (Chromium, Firefox, WebKit)
3. Create `playwright.config.ts`
4. Create example test in `tests/` folder
5. Add npm scripts

**Status**: E2E testing needs to be added ❌

---

## How They Work Together in CI/CD

### GitHub Actions Example

```yaml
# .github/workflows/test.yml
name: Tests

on: 
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  # ────────────────────────────────────────
  # Step 1: Unit Tests (Always Run)
  # ────────────────────────────────────────
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run unit tests
        run: npm test -- --watch=false --code-coverage --browsers=ChromeHeadless
      
      - name: Check coverage threshold
        run: |
          # Fails if coverage < 80%
          echo "Coverage check complete"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/lcov.info
          flags: unittests
          name: codecov-umbrella
      
      - name: Upload coverage report
        uses: actions/upload-artifact@v3
        with:
          name: coverage-report
          path: coverage/
    
    # ⏱️ Takes ~2-5 minutes
    # ✅ Runs on every push/PR
    # ✅ Fast feedback for developers

  # ────────────────────────────────────────
  # Step 2: E2E Tests (Run After Unit Tests)
  # ────────────────────────────────────────
  e2e-tests:
    name: E2E Tests
    runs-on: ubuntu-latest
    needs: unit-tests  # Only run if unit tests pass
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
      - name: Start backend server
        run: |
          # Start your API server
          npm run start:backend &
          npx wait-on http://localhost:5000
      
      - name: Start Angular dev server
        run: |
          npm run start &
          npx wait-on http://localhost:44426
      
      - name: Run E2E tests
        run: npx playwright test
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: playwright-report/
      
      - name: Upload failure screenshots
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-screenshots
          path: test-results/
    
    # ⏱️ Takes ~10-30 minutes
    # ✅ Runs only if unit tests pass
    # ✅ Tests critical user workflows

  # ────────────────────────────────────────
  # Step 3: Deploy (Run After All Tests)
  # ────────────────────────────────────────
  deploy:
    name: Deploy to Production
    runs-on: ubuntu-latest
    needs: [unit-tests, e2e-tests]  # Requires both to pass
    if: github.ref == 'refs/heads/main'
    
    steps:
      - name: Deploy application
        run: |
          echo "Deploying to production..."
          # Your deployment script here
    
    # ✅ Only deploys if ALL tests pass
```

### Local Development Flow

```bash
# ─────────────────────────────────────────
# During Development (Continuous)
# ─────────────────────────────────────────
npm test
# → Karma watches for file changes
# → Runs unit tests automatically
# → Provides instant feedback (~100ms)
# → Run this in a terminal tab all day

# ─────────────────────────────────────────
# Before Committing (One-time Check)
# ─────────────────────────────────────────
npm run test:coverage
# → Runs all unit tests once
# → Generates coverage report
# → Fails if coverage < 80%
# → Takes 2-5 minutes

# ─────────────────────────────────────────
# Before Creating PR (Manual Check)
# ─────────────────────────────────────────
npm run e2e
# → Runs E2E tests
# → Tests critical user flows
# → Takes 10-20 minutes
# → Optional locally, required in CI/CD

# ─────────────────────────────────────────
# Before Deployment (Automated in CI/CD)
# ─────────────────────────────────────────
# GitHub Actions runs:
# 1. All unit tests ✓
# 2. All E2E tests ✓
# 3. Deployment (only if tests pass)
```

---

## Developer Workflow

### Day-to-Day Development

#### Morning: Start Development

```bash
# Terminal 1: Run app
npm start

# Terminal 2: Run unit tests (watch mode)
npm test

# Now code all day with instant test feedback!
```

#### While Coding

```
1. Write/modify component
   ↓
2. Karma automatically re-runs tests (~100ms)
   ↓
3. See results immediately
   ↓
4. Fix any failures
   ↓
5. Repeat
```

**Benefit**: Catch bugs within seconds of writing code!

#### Before Committing

```bash
# Run full test suite with coverage
npm run test:coverage

# Check results:
# ✅ All tests pass
# ✅ Coverage ≥ 80%
# ✅ No skipped tests

# Then commit
git add .
git commit -m "feat: add contact search"
git push
```

#### Before Creating Pull Request

```bash
# Optional: Run E2E tests locally
npm run e2e

# Or let CI/CD run them (recommended)
# Just create PR and wait for checks
```

### TDD (Test-Driven Development) Workflow

If you want to practice TDD:

```bash
# 1. Write failing test first
# contact.service.spec.ts
it('should create contact', () => {
  const contact = { name: 'John' };
  service.createContact(contact).subscribe(result => {
    expect(result).toBeTruthy();
  });
});

# 2. Run test - it fails (RED)
# 3. Write minimal code to make it pass
# 4. Run test - it passes (GREEN)
# 5. Refactor code
# 6. Run test - still passes (REFACTOR)
# 7. Repeat
```

---

## Can You Use Just One?

### ❌ Scenario 1: Only Karma (Unit Tests)

**What You Get**:
- Fast feedback
- Good code coverage
- Easy to debug
- Cheap to maintain

**What You Miss**:
- Integration issues between components
- Backend API integration problems
- User workflow bugs
- Browser compatibility issues
- Visual/UX problems

**Real Example of What Gets Missed**:
```typescript
// Unit tests all pass ✅
ContactService.createContact() ✅
ContactListComponent.loadContacts() ✅
ContactEditDialog.submit() ✅

// But in production:
// 1. User creates contact ✅
// 2. Contact saved to database ✅
// 3. User sees success message ✅
// 4. List doesn't refresh ❌
// 5. User confused - contact seems lost ❌
```

### ❌ Scenario 2: Only Playwright (E2E Tests)

**What You Get**:
- Real user workflows work
- Integration verified
- Cross-browser testing
- Visual verification

**What You Miss**:
- Slow feedback (10-30 minutes)
- Hard to debug failures
- Can't test edge cases (too expensive)
- No code coverage metrics
- Flaky tests (timing, network issues)

**Real Problems**:
```typescript
// E2E test fails ❌
test('should create contact', async ({ page }) => {
  await page.fill('[data-testid="contact-name"]', 'John');
  await page.click('[data-testid="save"]');
  // ❌ FAILS - but where exactly?
});

// Without unit tests:
// - Is it the service? The component? The template?
// - What's the error message?
// - Which line failed?
// - Need to add console.logs and re-run (slow!)

// With unit tests:
// - Run ContactService tests - PASS ✅
// - Run ContactComponent tests - FAIL ❌
// - Immediately know: component validation logic is broken
// - Fix in 2 minutes
```

### ✅ Scenario 3: Both Karma + Playwright

**What You Get**:
- **Fast unit tests** catch most bugs early (70% of bugs)
- **Detailed feedback** pinpoint exact issue
- **Slow E2E tests** catch integration bugs (30% of bugs)
- **Complete coverage** from unit to workflow
- **Confident deployments** all tests pass = safe to deploy

**Best Practice**:
```
Developer writes code
  ↓
Unit tests run automatically (instant feedback)
  ↓
Commit code
  ↓
CI/CD runs unit tests (2-5 min)
  ↓
If pass → Run E2E tests (10-30 min)
  ↓
If pass → Deploy to production
  ↓
😊 Confidence!
```

---

## Setup Requirements

### Prerequisites

Before setting up testing, ensure you have:

```bash
# Node.js 18+ and npm
node --version  # Should be 18.x or higher
npm --version   # Should be 9.x or higher

# Angular CLI
ng version      # Should show Angular 19.x
```

### Step 1: Karma + Jasmine (Already Done! ✅)

Your project already has this configured. Verify it works:

```bash
# Run existing tests
npm test

# Should open a browser and run tests
# If you have no tests yet, it will show 0 tests
```

If it doesn't work, check:
- `karma.conf.js` exists
- `tsconfig.spec.json` exists
- Dependencies in `package.json` are installed

### Step 2: Playwright (Need to Add)

#### Installation

```bash
# Initialize Playwright
npm init playwright@latest

# Answer the prompts:
# ? Do you want to use TypeScript or JavaScript? › TypeScript
# ? Where to put your end-to-end tests? › tests
# ? Add a GitHub Actions workflow? › true
# ? Install Playwright browsers? › true
```

This will:
1. Install `@playwright/test`
2. Download browsers (Chromium, Firefox, WebKit) (~400MB)
3. Create `playwright.config.ts`
4. Create `tests/` folder with example test
5. Add GitHub Actions workflow

#### Verify Installation

```bash
# Run example test
npx playwright test

# Run with UI (see tests execute)
npx playwright test --ui

# Generate test code
npx playwright codegen http://localhost:44426
```

#### Update package.json Scripts

Add these scripts:

```json
{
  "scripts": {
    "test": "ng test",
    "test:ci": "ng test --watch=false --code-coverage --browsers=ChromeHeadless",
    "test:coverage": "ng test --watch=false --code-coverage",
    "e2e": "playwright test",
    "e2e:headed": "playwright test --headed",
    "e2e:ui": "playwright test --ui",
    "e2e:debug": "playwright test --debug"
  }
}
```

### Step 3: Configure Coverage Thresholds

Update `karma.conf.js`:

```javascript
// Add or update coverageReporter section
coverageReporter: {
  dir: require('path').join(__dirname, './coverage'),
  subdir: '.',
  reporters: [
    { type: 'html' },
    { type: 'text-summary' },
    { type: 'lcovonly' }
  ],
  check: {
    global: {
      statements: 80,
      branches: 75,
      functions: 80,
      lines: 80
    },
    each: {
      statements: 70,
      branches: 65,
      functions: 70,
      lines: 70
    }
  }
}
```

### Step 4: Set Up CI/CD (Optional but Recommended)

Create `.github/workflows/test.yml` (if using GitHub):

```yaml
name: Tests

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: npm ci
      - run: npm run test:ci

  e2e-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: npm ci
      - run: npx playwright install --with-deps
      - run: npm run e2e
```

### Total Setup Time

| Task | Time | Status |
|------|------|--------|
| Verify Karma works | 5 min | ✅ Already done |
| Install Playwright | 10 min | ⏳ Need to do |
| Configure coverage | 5 min | ⏳ Need to do |
| Write first E2E test | 15 min | ⏳ Need to do |
| Set up CI/CD | 15 min | ⏳ Optional |
| **Total** | **30-50 min** | |

---

## Real-World Example

Let's walk through testing the "Create Contact" feature completely.

### Feature Description

**User Story**: As a user, I want to create a new contact so I can track partnerships.

**Workflow**:
1. Navigate to Contacts page
2. Click "New Contact" button
3. Fill in contact form (name, email, phone, organization)
4. Click "Save"
5. See success message
6. See contact in the list

### Unit Tests (Karma + Jasmine)

We'll write ~15 unit tests covering all aspects:

#### 1. Service Tests

```typescript
// contact.service.spec.ts
describe('ContactService', () => {
  
  it('should create contact with valid data', () => {
    const newContact = {
      name: 'John Doe',
      email: 'john@example.com',
      phone: '+1234567890',
      organization: 'ACME Corp'
    };

    service.createContact(newContact).subscribe(result => {
      expect(result).toBeTruthy();
      expect(result.id).toBeDefined();
    });

    const req = httpMock.expectOne('/api/contacts');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newContact);
    req.flush({ id: 1, ...newContact });
  });

  it('should handle 400 validation error', () => {
    service.createContact({}).subscribe({
      error: (error) => {
        expect(error.status).toBe(400);
        expect(error.error).toContain('validation');
      }
    });

    const req = httpMock.expectOne('/api/contacts');
    req.flush({ message: 'Validation failed' }, { 
      status: 400, 
      statusText: 'Bad Request' 
    });
  });

  it('should handle 500 server error', () => {
    service.createContact({name: 'Test'}).subscribe({
      error: (error) => {
        expect(error.status).toBe(500);
      }
    });

    const req = httpMock.expectOne('/api/contacts');
    req.flush('Server error', { 
      status: 500, 
      statusText: 'Internal Server Error' 
    });
  });
});
```

#### 2. Component Tests

```typescript
// contact-edit-dialog.component.spec.ts
describe('ContactEditDialogComponent', () => {
  
  it('should initialize with empty form', () => {
    component.ngOnInit();
    
    expect(component.contactForm).toBeDefined();
    expect(component.contactForm.get('name').value).toBe('');
    expect(component.contactForm.get('email').value).toBe('');
  });

  it('should mark name as required', () => {
    const nameControl = component.contactForm.get('name');
    
    nameControl.setValue('');
    expect(nameControl.hasError('required')).toBeTrue();
    
    nameControl.setValue('John');
    expect(nameControl.hasError('required')).toBeFalse();
  });

  it('should validate email format', () => {
    const emailControl = component.contactForm.get('email');
    
    emailControl.setValue('invalid');
    expect(emailControl.hasError('email')).toBeTrue();
    
    emailControl.setValue('valid@example.com');
    expect(emailControl.hasError('email')).toBeFalse();
  });

  it('should disable save button when form invalid', () => {
    component.contactForm.get('name').setValue('');
    fixture.detectChanges();
    
    const saveButton = fixture.nativeElement.querySelector('[data-testid="save"]');
    expect(saveButton.disabled).toBeTrue();
  });

  it('should enable save button when form valid', () => {
    component.contactForm.patchValue({
      name: 'John Doe',
      email: 'john@example.com',
      phone: '+1234567890'
    });
    fixture.detectChanges();
    
    const saveButton = fixture.nativeElement.querySelector('[data-testid="save"]');
    expect(saveButton.disabled).toBeFalse();
  });

  it('should call service on save', () => {
    const contact = {
      name: 'John Doe',
      email: 'john@example.com'
    };
    
    contactService.createContact.and.returnValue(of({ id: 1, ...contact }));
    component.contactForm.patchValue(contact);
    
    component.save();
    
    expect(contactService.createContact).toHaveBeenCalledWith(contact);
  });

  it('should emit success event on successful save', () => {
    spyOn(component.contactSaved, 'emit');
    contactService.createContact.and.returnValue(of({ id: 1 }));
    
    component.save();
    
    expect(component.contactSaved.emit).toHaveBeenCalledWith({ id: 1 });
  });

  it('should show error message on save failure', () => {
    contactService.createContact.and.returnValue(
      throwError(() => new Error('Save failed'))
    );
    
    component.save();
    
    expect(component.errorMessage).toBe('Failed to save contact');
  });

  it('should close dialog on cancel', () => {
    spyOn(component.dialogRef, 'close');
    
    component.cancel();
    
    expect(component.dialogRef.close).toHaveBeenCalled();
  });
});
```

#### 3. List Component Tests

```typescript
// contact-list.component.spec.ts
describe('ContactListComponent', () => {
  
  it('should load contacts on init', () => {
    const mockContacts = [
      { id: 1, name: 'John' },
      { id: 2, name: 'Jane' }
    ];
    contactService.getContacts.and.returnValue(of(mockContacts));
    
    component.ngOnInit();
    
    expect(component.contacts).toEqual(mockContacts);
    expect(component.loading).toBeFalse();
  });

  it('should open create dialog', () => {
    spyOn(component.dialog, 'open').and.returnValue({
      afterClosed: () => of({ id: 1, name: 'New Contact' })
    } as any);
    
    component.openCreateDialog();
    
    expect(component.dialog.open).toHaveBeenCalled();
  });

  it('should refresh list after creating contact', () => {
    spyOn(component, 'loadContacts');
    spyOn(component.dialog, 'open').and.returnValue({
      afterClosed: () => of({ id: 1 })
    } as any);
    
    component.openCreateDialog();
    
    expect(component.loadContacts).toHaveBeenCalled();
  });

  it('should display contacts in template', () => {
    component.contacts = [
      { id: 1, name: 'John Doe', email: 'john@example.com' },
      { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
    ];
    fixture.detectChanges();
    
    const contactItems = fixture.nativeElement.querySelectorAll('.contact-item');
    expect(contactItems.length).toBe(2);
    expect(contactItems[0].textContent).toContain('John Doe');
    expect(contactItems[1].textContent).toContain('Jane Smith');
  });
});
```

**Unit Test Summary**:
- ✅ 15 unit tests written
- ✅ Service layer tested (API calls, error handling)
- ✅ Component logic tested (form validation, save/cancel)
- ✅ Template rendering tested (buttons, fields, lists)
- ✅ Takes ~200ms to run all tests
- ✅ Provides 90%+ code coverage

### E2E Test (Playwright)

We'll write **1 comprehensive E2E test** covering the entire workflow:

```typescript
// tests/e2e/contact-management.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Contact Management - Create Contact', () => {
  
  test.beforeEach(async ({ page }) => {
    // Prerequisite: User must be logged in
    await page.goto('http://localhost:44426/login');
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password123');
    await page.click('[data-testid="login-button"]');
    await expect(page).toHaveURL('http://localhost:44426/');
  });

  test('should complete full contact creation workflow', async ({ page }) => {
    // Step 1: Navigate to Contacts page
    await page.goto('http://localhost:44426/partnerships/contacts');
    await expect(page.locator('h1')).toContainText('Contacts');
    
    // Step 2: Get initial count of contacts
    const initialContactCount = await page.locator('.contact-item').count();
    
    // Step 3: Click "New Contact" button
    await page.click('[data-testid="new-contact-button"]');
    await expect(page.locator('.dialog-title')).toContainText('New Contact');
    
    // Step 4: Verify save button is disabled initially
    const saveButton = page.locator('[data-testid="save-contact"]');
    await expect(saveButton).toBeDisabled();
    
    // Step 5: Fill in contact form
    const timestamp = Date.now();
    const contactData = {
      name: `Test Contact ${timestamp}`,
      email: `test${timestamp}@example.com`,
      phone: '+1234567890',
      organization: 'ACME Corp',
      title: 'Senior Manager'
    };
    
    await page.fill('[data-testid="contact-name"]', contactData.name);
    await page.fill('[data-testid="contact-email"]', contactData.email);
    await page.fill('[data-testid="contact-phone"]', contactData.phone);
    await page.fill('[data-testid="contact-organization"]', contactData.organization);
    await page.fill('[data-testid="contact-title"]', contactData.title);
    
    // Step 6: Verify save button is now enabled
    await expect(saveButton).toBeEnabled();
    
    // Step 7: Click save button
    await saveButton.click();
    
    // Step 8: Verify dialog closes
    await expect(page.locator('.dialog-title')).not.toBeVisible();
    
    // Step 9: Verify success message appears
    const successMessage = page.locator('.success-message');
    await expect(successMessage).toBeVisible();
    await expect(successMessage).toContainText('Contact created successfully');
    
    // Step 10: Verify contact appears in the list
    await expect(page.locator('.contact-item')).toHaveCount(initialContactCount + 1);
    
    // Step 11: Verify contact details are visible
    const newContact = page.locator('.contact-item').filter({ hasText: contactData.name });
    await expect(newContact).toBeVisible();
    await expect(newContact).toContainText(contactData.name);
    await expect(newContact).toContainText(contactData.email);
    await expect(newContact).toContainText(contactData.organization);
    
    // Step 12: Click on contact to view details
    await newContact.click();
    await expect(page.locator('.contact-details h2')).toContainText(contactData.name);
    await expect(page.locator('.contact-email')).toContainText(contactData.email);
    await expect(page.locator('.contact-phone')).toContainText(contactData.phone);
    
    // Step 13: Verify we can navigate back
    await page.click('[data-testid="back-button"]');
    await expect(page.locator('h1')).toContainText('Contacts');
    
    // Cleanup: Delete the test contact (optional)
    await newContact.locator('[data-testid="delete-button"]').click();
    await page.click('[data-testid="confirm-delete"]');
    await expect(successMessage).toContainText('Contact deleted successfully');
  });

  test('should show validation errors for invalid data', async ({ page }) => {
    await page.goto('http://localhost:44426/partnerships/contacts');
    await page.click('[data-testid="new-contact-button"]');
    
    // Try to save without filling required fields
    await page.click('[data-testid="save-contact"]');
    
    // Should still be on dialog (doesn't close)
    await expect(page.locator('.dialog-title')).toBeVisible();
    
    // Verification errors should appear
    await expect(page.locator('.error-name')).toContainText('Name is required');
    await expect(page.locator('.error-email')).toContainText('Email is required');
    
    // Fill invalid email
    await page.fill('[data-testid="contact-email"]', 'invalid-email');
    await page.click('[data-testid="contact-name"]'); // Blur event
    
    await expect(page.locator('.error-email')).toContainText('Invalid email format');
  });
});
```

**E2E Test Summary**:
- ✅ 1-2 comprehensive tests
- ✅ Tests complete user workflow
- ✅ Verifies frontend + backend integration
- ✅ Tests real API calls (no mocking)
- ✅ Verifies visual elements appear
- ✅ Takes ~10-20 seconds per test
- ✅ Catches integration and UX issues

### What Each Test Type Catches

In this example:

**Unit Tests Caught**:
- ✅ Service method returns correct data structure
- ✅ Form validation rules work
- ✅ Error handling displays messages
- ✅ Component methods called correctly
- ✅ Template bindings work

**E2E Test Caught**:
- ✅ Complete workflow functions end-to-end
- ✅ Navigation between pages works
- ✅ API integration works (real HTTP calls)
- ✅ Success message appears to user
- ✅ Contact appears in list immediately
- ✅ User can view created contact

**Together They Ensure**:
- ✅ Each piece works (unit tests)
- ✅ All pieces work together (E2E test)
- ✅ User experience is correct
- ✅ Confident to deploy to production

---

## Quick Reference

### Commands Cheat Sheet

```bash
# ──────────────────────────────────────────
# Unit Tests (Karma + Jasmine)
# ──────────────────────────────────────────

# Run tests in watch mode (development)
npm test

# Run tests once with coverage
npm run test:coverage

# Run tests in CI (headless, no watch)
npm run test:ci

# Run specific test file
npm test -- --include='**/contact.service.spec.ts'

# Run tests matching pattern
npm test -- --grep='ContactService'

# ──────────────────────────────────────────
# E2E Tests (Playwright)
# ──────────────────────────────────────────

# Run all E2E tests (headless)
npx playwright test

# Run with UI (see test execution)
npx playwright test --ui

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific test file
npx playwright test contact-management.spec.ts

# Run specific test by name
npx playwright test -g "should create contact"

# Debug mode (step through test)
npx playwright test --debug

# Generate test code (record actions)
npx playwright codegen http://localhost:44426

# View HTML report
npx playwright show-report

# Run only Chrome tests
npx playwright test --project=chromium

# Run tests in parallel
npx playwright test --workers=4
```

### File Structure Quick Reference

```
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── auth.service.spec.ts          ← Unit test
│   │   └── guards/
│   │       ├── auth.guard.ts
│   │       └── auth.guard.spec.ts            ← Unit test
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   └── phone-input/
│   │   │       ├── phone-input.component.ts
│   │   │       ├── phone-input.component.spec.ts  ← Unit test
│   │   │       ├── phone-input.component.html
│   │   │       └── phone-input.component.scss
│   │   └── pipes/
│   │       ├── markdown.pipe.ts
│   │       └── markdown.pipe.spec.ts         ← Unit test
│   │
│   └── features/
│       └── partnerships/
│           └── contacts/
│               ├── components/
│               │   └── contact-list/
│               │       ├── contact-list.component.ts
│               │       ├── contact-list.component.spec.ts  ← Unit test
│               │       ├── contact-list.component.html
│               │       └── contact-list.component.scss
│               └── services/
│                   ├── contact.service.ts
│                   └── contact.service.spec.ts      ← Unit test
│
└── tests/
    ├── e2e/                                         ← E2E tests
    │   ├── auth.spec.ts
    │   ├── contact-management.spec.ts
    │   └── partner-management.spec.ts
    ├── fixtures/
    │   └── contact.fixtures.ts
    └── helpers/
        └── test-utils.ts
```

### Test Naming Conventions

```typescript
// ──────────────────────────────────────────
// Unit Test File Names
// ──────────────────────────────────────────
component-name.component.spec.ts
service-name.service.spec.ts
pipe-name.pipe.spec.ts
guard-name.guard.spec.ts
directive-name.directive.spec.ts

// ──────────────────────────────────────────
// E2E Test File Names
// ──────────────────────────────────────────
feature-name.spec.ts
workflow-name.spec.ts

// ──────────────────────────────────────────
// Test Description Format
// ──────────────────────────────────────────
describe('ComponentName', () => {
  describe('methodName()', () => {
    it('should [expected behavior] when [condition]', () => {
      // Arrange
      // Act
      // Assert
    });
  });
});

// Examples:
it('should return user when valid ID provided', () => {});
it('should throw error when user not found', () => {});
it('should disable button when form is invalid', () => {});
```

### Coverage Thresholds

| Component Type | Minimum Coverage | Target Coverage |
|---------------|------------------|-----------------|
| Services | 90% | 95%+ |
| Components | 80% | 85%+ |
| Pipes | 95% | 100% |
| Guards | 100% | 100% |
| Interceptors | 100% | 100% |
| Directives | 90% | 95%+ |
| **Overall** | **80%** | **85%+** |

### When to Run Which Tests

| Situation | Run Unit Tests | Run E2E Tests |
|-----------|---------------|---------------|
| **Writing code** | ✅ Continuously (watch mode) | ❌ No |
| **Before commit** | ✅ Once with coverage | ❌ No (optional) |
| **Pull Request** | ✅ Automated in CI | ✅ Automated in CI |
| **Before deploy** | ✅ Yes | ✅ Yes |
| **After deploy** | ❌ No | ✅ Smoke tests |

---

## Summary

### The Bottom Line

**Yes, you need both Karma and Playwright!**

| Framework | Purpose | Speed | Quantity | When |
|-----------|---------|-------|----------|------|
| **Karma + Jasmine** | Unit testing | ⚡ Fast | Many (1000s) | Always |
| **Playwright** | E2E testing | 🐢 Slow | Few (10-50) | Before deploy |

### Why Both?

- **Karma** = Check each brick is solid ✅
- **Playwright** = Check the house doesn't collapse ✅
- **Together** = Confidence to deploy! 🚀

### What You Need to Do

1. ✅ **Karma**: Already configured, start writing unit tests
2. ⏳ **Playwright**: Install and set up (~30 minutes)
3. 📝 **Write tests**: Unit tests for everything, E2E for critical paths
4. 🔄 **CI/CD**: Run both in automated pipeline
5. 🎯 **Target**: 80%+ coverage with unit tests, 10+ E2E tests

### Next Steps

```bash
# 1. Install Playwright
npm init playwright@latest

# 2. Write your first E2E test
# Create tests/e2e/login.spec.ts

# 3. Run it!
npx playwright test

# 4. Celebrate! 🎉
```

---

**Document Version**: 1.0  
**Last Updated**: October 11, 2025  
**Related Documents**: 
- [Angular Codebase Analysis](./ANGULAR_CODEBASE_ANALYSIS.md)
- [Testing Standards Section](./ANGULAR_CODEBASE_ANALYSIS.md#testing-standards--implementation-guide)

