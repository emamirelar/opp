# Playwright E2E Tests

This folder contains end-to-end tests using Playwright.

## 📁 Folder Structure

```
opportunityplus/
├── Playwright Tests/      # ← You are here
│   ├── home.spec.ts       # Example test - Home page
│   ├── example.spec.ts    # Delete this - just a demo
│   └── README.md          # This file
├── playwright.config.ts   # Config at root level
└── package.json           # Scripts at root level
```

## 🚀 Quick Start

### Run All Tests
```bash
# From repository root
npm run test
```

### Run with UI Mode (Interactive)
```bash
npm run test:ui
```

### Run with Visible Browser (Headed Mode)
```bash
npm run test:headed
```

### Debug Mode
```bash
npm run test:debug
```

### Run Only Chrome Tests
```bash
npm run test:chrome
```

### View Last Test Report
```bash
npm run test:report
```

## ⏳ Waiting for Page Ready States

**Important**: The Angular application uses loading overlays and spinners. Always wait for these to disappear before making assertions.

### Automatic Waiting

The `login()` and `loginAndNavigate()` helpers automatically wait for loading to complete.

### Manual Waiting

```typescript
import { waitForPageReady, waitForLoadingToComplete, waitForElementReady } from './helpers/wait.helper';

// Wait for all loading overlays and network activity
await waitForPageReady(page);

// Wait specifically for loading overlays
await waitForLoadingToComplete(page);

// Wait for specific element to be ready
await waitForElementReady(page.locator('[data-testid="my-element"]'));
```

## 🔌 API Mocking

**Important**: The Angular application requires backend APIs to initialize. Playwright tests use API mocking to simulate backend responses without requiring a running backend server.

### Automatic Mocking

The following endpoints are automatically mocked when using the `login()` helper:

- **`/api/configuration`** - Returns mock configuration data
- **`/user/claims`** - Returns empty array (unauthenticated) or test user claims after login
- **`/api/global/preferred-language`** - Returns English as default language
- **`/user/login`** - Accepts credentials and returns success response
- **`**/api/**`** - Catch-all for any other API endpoints

### Usage

```typescript
import { login } from './helpers/auth.helper';

test.beforeEach(async ({ page }) => {
  // API mocks are automatically set up during login
  await login(page);
});
```

### Manual Mocking

For custom test scenarios:

```typescript
import { setupAPIMocks, setupAuthenticatedUserMock } from './helpers/api-mocks.helper';

// Mock APIs before navigation
await setupAPIMocks(page);
await page.goto('/');

// Or mock authenticated user
await setupAuthenticatedUserMock(page, 'test@example.com');
```

## 📝 Writing Tests

### Basic Test Structure

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do something', async ({ page }) => {
    // Navigate
    await page.goto('/your-route');
    
    // Interact
    await page.click('[data-testid="button"]');
    
    // Assert
    await expect(page.locator('[data-testid="result"]')).toBeVisible();
  });
});
```

### Use Data Test IDs

**Always use `data-testid` attributes** for stable selectors:

```typescript
// ✅ GOOD - Stable, explicit
await page.click('[data-testid="submit-button"]');

// ❌ BAD - Fragile, breaks on style changes
await page.click('.btn-primary.submit');
```

### Test Organization

Create separate files for each feature:

```
tests/
├── auth/
│   ├── login.spec.ts
│   └── logout.spec.ts
├── partners/
│   ├── partner-list.spec.ts
│   └── partner-create.spec.ts
└── contacts/
    └── contact-create.spec.ts
```

## 🎯 Best Practices

1. **Use Page Objects** - Create reusable page classes
2. **Use Fixtures** - Share setup code (like authentication)
3. **Test User Workflows** - Not individual components
4. **Keep Tests Independent** - Each test should work standalone
5. **Use Auto-Waiting** - Playwright waits automatically for elements

## 📚 Resources

- **Playwright Docs**: https://playwright.dev/
- **Best Practices**: https://playwright.dev/docs/best-practices
- **Test Generator**: Run `npx playwright codegen http://localhost:4200`

## 🐛 Debugging

### View Trace for Failed Test
After a failed test run:
```bash
npx playwright show-trace test-results/trace.zip
```

### Run Single Test File
```bash
npx playwright test tests/home.spec.ts
```

### Run Tests Matching Pattern
```bash
npx playwright test -g "should login"
```

## ⚙️ Configuration

See `playwright.config.ts` for:
- Browser configurations
- Timeout settings
- Retry logic
- Reporter settings
- Base URL configuration

## 📊 CI/CD

Playwright tests are configured to run in GitHub Actions:
- See `.github/workflows/playwright.yml`
- Tests run on every push
- Test reports saved as artifacts
