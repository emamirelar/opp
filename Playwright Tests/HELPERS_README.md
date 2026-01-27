# Playwright Test Helpers and Page Objects

Comprehensive helper utilities and page object models for the UNOPS Opportunity+ E2E test suite.

## 📁 Folder Structure

```
Playwright Tests/
├── helpers/                    # Reusable helper functions
│   ├── test-config.ts         # Centralized test configuration
│   ├── auth.helper.ts         # Authentication helpers
│   ├── wait.helper.ts         # Wait and timing utilities
│   ├── navigation.helper.ts   # Navigation helpers
│   └── assertions.helper.ts   # Reusable assertion functions
├── pages/                      # Page Object Models
│   ├── base.page.ts           # Base page with common functionality
│   ├── entity-list.page.ts    # Base for list pages
│   ├── login.page.ts          # Login page object
│   ├── dashboard.page.ts      # Dashboard page object
│   ├── partners.page.ts       # Partners page object
│   ├── contacts.page.ts       # Contacts page object
│   └── opportunities.page.ts  # Opportunities page object
├── *.spec.ts                   # Test specification files
├── .env                        # Environment configuration (create from .env.example)
└── .env.example                # Example environment configuration
```

---

## 🔧 Configuration

### Environment Variables

Create a `.env` file from `.env.example`:

```bash
cp .env.example .env
```

Update with your test credentials:

```env
TEST_USER_EMAIL=your-test-user@unops.org
TEST_USER_PASSWORD=YourSecurePassword123!
BASE_URL=http://localhost:4200
API_BASE_URL=http://localhost:5000
```

### Test Configuration (`helpers/test-config.ts`)

Centralized configuration management:

```typescript
import { getTestCredentials, getTimeout } from './helpers/test-config';

const credentials = getTestCredentials();
const timeout = getTimeout('long'); // 30 seconds
```

**Available Functions:**
- `getTestCredentials()` - Get test user credentials
- `getBaseUrl()` - Get application base URL
- `getApiBaseUrl()` - Get API base URL
- `getTimeout(type)` - Get timeout values ('default', 'long', 'short')

---

## 🔐 Authentication Helpers (`helpers/auth.helper.ts`)

Simplifies authentication flows in tests.

### Usage Examples

**Basic Login:**
```typescript
import { login } from './helpers/auth.helper';

test('my test', async ({ page }) => {
  await login(page);
  // User is now logged in
});
```

**Login and Navigate:**
```typescript
import { loginAndNavigate } from './helpers/auth.helper';

test('my test', async ({ page }) => {
  await loginAndNavigate(page, '/partners');
  // User is logged in and on /partners page
});
```

**Custom Credentials:**
```typescript
await login(page, 'admin@unops.org', 'AdminPassword123!');
```

**Available Functions:**
- `login(page, email?, password?)` - Login with credentials
- `loginAndNavigate(page, targetUrl, email?, password?)` - Login and navigate
- `isLoggedIn(page)` - Check if user is logged in
- `logout(page)` - Logout from application
- `verifyLoginPageElements(page)` - Verify login form elements

---

## ⏱️ Wait Helpers (`helpers/wait.helper.ts`)

Smart waiting strategies for reliable tests.

### Usage Examples

```typescript
import { waitForDialog, waitForNetworkIdle, smartWait } from './helpers/wait.helper';

// Wait for dialog to appear
await waitForDialog(page);

// Wait for network to be idle
await waitForNetworkIdle(page);

// Smart wait (combines multiple strategies)
await smartWait(page);

// Wait for specific API endpoint
await waitForDataLoad(page, '/api/partners');
```

**Available Functions:**
- `waitForVisible(locator, timeout?)` - Wait for element visibility
- `waitForHidden(locator, timeout?)` - Wait for element to hide
- `waitForNetworkIdle(page)` - Wait for network idle
- `waitForDataLoad(page, apiEndpoint?)` - Wait for API data
- `waitForDialog(page)` - Wait for dialog to open
- `waitForDialogClose(page)` - Wait for dialog to close
- `waitForLoadingComplete(page)` - Wait for spinners to disappear
- `waitForPermissions(page)` - Wait for permission-dependent UI
- `waitForTableData(page)` - Wait for table to load
- `smartWait(page)` - Combined wait strategy

---

## 🧭 Navigation Helpers (`helpers/navigation.helper.ts`)

Simplified navigation between pages.

### Usage Examples

```typescript
import { navigateToPartners, navigateToEntityDetail } from './helpers/navigation.helper';

// Navigate to partners page
await navigateToPartners(page);

// Navigate to specific partner
await navigateToEntityDetail(page, 'partners', 123);

// Use browser navigation
await goBack(page);
await goForward(page);
await reloadPage(page);
```

**Available Functions:**
- `navigateTo(page, url)` - Navigate to any URL
- `navigateToPartners(page)` - Go to partners page
- `navigateToContacts(page)` - Go to contacts page
- `navigateToInteractions(page)` - Go to interactions page
- `navigateToOpportunities(page)` - Go to opportunities page
- `navigateToDashboard(page)` - Go to dashboard
- `navigateToEntityDetail(page, entityType, id)` - Go to entity detail
- `goBack(page)` - Browser back button
- `goForward(page)` - Browser forward button
- `reloadPage(page)` - Reload current page

---

## ✅ Assertion Helpers (`helpers/assertions.helper.ts`)

Reusable assertions for common test scenarios.

### Usage Examples

```typescript
import { 
  assertVisible, 
  assertPageHeader,
  assertTableHasData,
  assertDialogOpen 
} from './helpers/assertions.helper';

// Assert element is visible
await assertVisible(page.locator('[data-testid="my-button"]'));

// Assert page header
await assertPageHeader(page, 'partners');

// Assert table has data
const rowCount = await assertTableHasData(page);

// Assert dialog is open
await assertDialogOpen(page);
```

**Available Functions:**
- `assertVisible(locator, timeout?)` - Assert element visible
- `assertHidden(locator)` - Assert element hidden
- `assertContainsText(locator, text)` - Assert text content
- `assertPageHeader(page, entityName)` - Assert page header
- `assertListviewVisible(page, entityName)` - Assert listview
- `assertButtonVisible(page, testId, text?)` - Assert button
- `assertTableHasData(page)` - Assert table has rows
- `assertUrlMatches(page, pattern)` - Assert URL pattern
- `assertDialogOpen(page)` - Assert dialog is open
- `assertDialogClosed(page)` - Assert dialog is closed
- `assertErrorDisplayed(page, errorText?)` - Assert error message
- `assertSuccessDisplayed(page, successText?)` - Assert success message
- `assertHasAttribute(locator, attribute, value)` - Assert attribute
- `assertMobileResponsive(page, elements)` - Assert mobile layout

---

## 📄 Page Object Models

### Base Page (`pages/base.page.ts`)

Foundation for all page objects with common functionality.

```typescript
export abstract class BasePage {
  protected readonly page: Page;
  
  // Navigation
  async goto(url: string): Promise<void>
  getUrl(): string
  
  // Common actions
  getByTestId(testId: string): Locator
  async clickByTestId(testId: string): Promise<void>
  async fillByTestId(testId: string, value: string): Promise<void>
  async assertElementVisible(testId: string): Promise<void>
  
  // Utilities
  async waitForLoad(): Promise<void>
  async waitForPermissions(): Promise<void>
  async reload(): Promise<void>
  async takeScreenshot(name: string): Promise<void>
}
```

### Login Page (`pages/login.page.ts`)

```typescript
import { LoginPage } from './pages/login.page';

const loginPage = new LoginPage(page);

// Navigate and perform login
await loginPage.navigate();
await loginPage.login('user@unops.org', 'password');

// Or individual actions
await loginPage.fillUsername('user@unops.org');
await loginPage.fillPassword('password');
await loginPage.clickLogin();

// Verify form
await loginPage.verifyLoginFormVisible();
await loginPage.verifyFormLabels();

// Password visibility
await loginPage.togglePasswordVisibility();
const fieldType = await loginPage.getPasswordFieldType();
```

### Entity List Page (`pages/entity-list.page.ts`)

Base class for list pages (Partners, Contacts, Opportunities).

**Inherited Properties:**
- `header` - Page header locator
- `icon` - Page icon locator
- `title` - Page title locator
- `listview` - Listview component locator
- `newButton` - New entity button
- `exportButton` - Export button
- `importButton` - Import button
- `tableRows` - Table rows locator
- `searchInput` - Search input locator

**Common Methods:**
```typescript
// Verify page elements
await entityPage.verifyPageHeader();
await entityPage.verifyListviewVisible();

// Check permissions
const canCreate = await entityPage.isNewButtonVisible();
const canExport = await entityPage.isExportButtonVisible();

// Actions
await entityPage.clickNewButton();
await entityPage.clickExportButton();
await entityPage.search('search text');

// Data operations
const rowCount = await entityPage.getRowCount();
await entityPage.clickFirstRow();

// Mobile
await entityPage.verifyMobileResponsive();
```

### Partners Page (`pages/partners.page.ts`)

```typescript
import { PartnersPage } from './pages/partners.page';

const partnersPage = new PartnersPage(page);
await partnersPage.navigate();
await partnersPage.navigateToPartnerDetail(123);
```

### Contacts Page (`pages/contacts.page.ts`)

Extends `EntityListPage` with business card scanner.

```typescript
import { ContactsPage } from './pages/contacts.page';

const contactsPage = new ContactsPage(page);

// Business card scanner
if (await contactsPage.isScannerButtonVisible()) {
  await contactsPage.clickScannerButton();
}
```

### Opportunities Page (`pages/opportunities.page.ts`)

```typescript
import { OpportunitiesPage } from './pages/opportunities.page';

const opportunitiesPage = new OpportunitiesPage(page);
await opportunitiesPage.navigate();
await opportunitiesPage.navigateToOpportunityDetail(456);
```

### Dashboard Page (`pages/dashboard.page.ts`)

```typescript
import { DashboardPage } from './pages/dashboard.page';

const dashboardPage = new DashboardPage(page);

await dashboardPage.navigate();
await dashboardPage.verifyDashboardVisible();
await dashboardPage.verifyWelcomeMessage();

const panelCount = await dashboardPage.getPanelCount();
const hasActions = await dashboardPage.hasQuickActions();

await dashboardPage.clickRefresh();
await dashboardPage.verifyMobileResponsive();
```

---

## 📝 Complete Test Example

```typescript
import { test, expect } from '@playwright/test';
import { PartnersPage } from './pages/partners.page';
import { loginAndNavigate } from './helpers/auth.helper';
import { assertUrlMatches } from './helpers/assertions.helper';

test.describe('Partners', () => {
  let partnersPage: PartnersPage;
  
  test.beforeEach(async ({ page }) => {
    partnersPage = new PartnersPage(page);
    await loginAndNavigate(page, '/partners');
  });
  
  test('should display partners page', async () => {
    await partnersPage.verifyPageHeader();
    await partnersPage.verifyListviewVisible();
  });
  
  test('should create new partner if permitted', async ({ page }) => {
    await partnersPage.waitForPermissions();
    
    if (await partnersPage.isNewButtonVisible()) {
      await partnersPage.clickNewButton();
      // Dialog should open
      await expect(page.locator('[role="dialog"]')).toBeVisible();
    }
  });
  
  test('should navigate to partner detail', async ({ page }) => {
    const rowCount = await partnersPage.getRowCount();
    
    if (rowCount > 0) {
      await partnersPage.clickFirstRow();
      await assertUrlMatches(page, /\/partners\/\d+/);
    }
  });
});
```

---

## 🎯 Best Practices

### 1. Use Page Objects for All Interactions
```typescript
// ✅ GOOD - Using page object
await partnersPage.clickNewButton();

// ❌ BAD - Direct page interaction
await page.locator('[data-testid="new-partner-button"]').click();
```

### 2. Use Helpers for Common Operations
```typescript
// ✅ GOOD - Using helper
await loginAndNavigate(page, '/partners');

// ❌ BAD - Repeating login code
await page.goto('/login');
await page.locator('[data-testid="username-input"]').fill('...');
// ... more repetitive code
```

### 3. Use Configuration for Test Data
```typescript
// ✅ GOOD - Using config
const credentials = getTestCredentials();
await login(page, credentials.email, credentials.password);

// ❌ BAD - Hardcoded credentials
await login(page, 'testuser@unops.org', 'TestPassword123!');
```

### 4. Use Smart Waits
```typescript
// ✅ GOOD - Smart wait
await smartWait(page);

// ❌ BAD - Arbitrary timeout
await page.waitForTimeout(5000);
```

### 5. Handle Permissions Gracefully
```typescript
// ✅ GOOD - Check permission-dependent elements
if (await partnersPage.isNewButtonVisible()) {
  await partnersPage.clickNewButton();
}

// ❌ BAD - Assume element exists
await partnersPage.clickNewButton(); // Fails if no permission
```

---

## 🚀 Running Tests

```bash
# Run all tests
npm test

# Run with UI
npm run test:ui

# Run specific test file
npx playwright test login.spec.ts

# Debug mode
npm run test:debug

# Run with specific browser
npm run test:chrome
```

---

## 📚 Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Page Object Model Pattern](https://playwright.dev/docs/pom)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [Test Selectors](https://playwright.dev/docs/selectors)

---

## 🤝 Contributing

When adding new page objects or helpers:

1. Follow existing patterns and naming conventions
2. Add comprehensive JSDoc comments
3. Update this README with usage examples
4. Add tests for new functionality
5. Keep helpers focused and reusable
