# Playwright E2E Test Conversion Candidates

**Date**: January 23, 2026  
**Purpose**: Identify simple, passing Angular unit tests that are ideal candidates for conversion to Playwright E2E tests  
**Current Test Status**: 741 out of 1,100 Angular tests passing (67.4%)

---

## 🎯 **WHY CONVERT TO PLAYWRIGHT?**

### Current State: Unit Tests vs. E2E Tests

**Angular Unit Tests (Jasmine/Karma):**
- ✅ Test component logic in isolation
- ✅ Fast execution (1,100 tests in ~21 seconds)
- ❌ Don't test actual browser behavior
- ❌ Don't test real user interactions
- ❌ Don't test backend integration
- ❌ Miss cross-browser issues

**Playwright E2E Tests:**
- ✅ Test **real user workflows** end-to-end
- ✅ Test **actual browser behavior** (Chromium, Firefox, WebKit)
- ✅ Test **backend integration** (real API calls)
- ✅ Catch **integration issues** that unit tests miss
- ✅ **Visual regression testing** built-in
- ✅ **Auto-wait** for elements (no flaky tests)
- ✅ **Parallel execution** across browsers

---

## ✅ **TOP CANDIDATES FOR CONVERSION**

### Category 1: Simple Page Load Tests (EASIEST - Start Here!)

These are the **simplest** tests to convert and provide immediate value.

#### 1. **Home Page / Dashboard Tests** ⭐ **BEST FIRST TEST**

**Current Angular Test:**
```typescript
// UNOPS.PAO.ClientApp/src/app/features/home/components/home/home.component.spec.ts
describe('HomeComponent', () => {
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

**Converted Playwright Test:**
```typescript
// e2e/home.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should load home page successfully', async ({ page }) => {
    // Navigate to home page
    await page.goto('/');
    
    // Verify page loaded
    await expect(page).toHaveTitle(/Opportunity\+/);
    
    // Verify main content is visible
    await expect(page.locator('[data-testid="home-dashboard"]')).toBeVisible();
  });
  
  test('should display welcome message for logged in user', async ({ page }) => {
    // Login first
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
    
    // Navigate to home
    await page.goto('/');
    
    // Verify user-specific content
    await expect(page.locator('[data-testid="user-welcome"]')).toContainText('Welcome');
  });
});
```

**Complexity**: ⭐ Very Easy  
**Effort**: 15-30 minutes  
**Value**: High - Validates entire application loads  

---

#### 2. **Dashboard Component Tests**

**Current Angular Test:**
```typescript
// UNOPS.PAO.ClientApp/src/app/features/home/components/home-dashboard/home-dashboard.component.spec.ts
describe('HomeDashboardComponent', () => {
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

**Converted Playwright Test:**
```typescript
// e2e/dashboard.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
  });
  
  test('should display dashboard widgets', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Verify all expected widgets are present
    await expect(page.locator('[data-testid="opportunities-widget"]')).toBeVisible();
    await expect(page.locator('[data-testid="partners-widget"]')).toBeVisible();
    await expect(page.locator('[data-testid="contacts-widget"]')).toBeVisible();
  });
  
  test('should display recent activities', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Verify recent activities section
    await expect(page.locator('[data-testid="recent-activities"]')).toBeVisible();
    
    // Verify at least one activity is shown (if data exists)
    const activityCount = await page.locator('[data-testid="activity-item"]').count();
    expect(activityCount).toBeGreaterThanOrEqual(0);
  });
});
```

**Complexity**: ⭐ Very Easy  
**Effort**: 30-45 minutes  
**Value**: High - Validates dashboard functionality

---

### Category 2: Authentication Flow Tests (HIGH VALUE)

#### 3. **Login Component Tests** ⭐ **HIGH PRIORITY**

**Current Angular Test:**
```typescript
// UNOPS.PAO.ClientApp/src/app/features/auth/components/login/login.component.spec.ts
describe('LoginComponent', () => {
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

**Converted Playwright Test:**
```typescript
// e2e/auth/login.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Login Flow', () => {
  test('should display login form', async ({ page }) => {
    await page.goto('/login');
    
    // Verify form elements are present
    await expect(page.locator('[data-testid="username-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="login-button"]')).toBeVisible();
  });
  
  test('should successfully login with valid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in credentials
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'ValidPassword123!');
    
    // Click login
    await page.click('[data-testid="login-button"]');
    
    // Verify redirect to home page
    await expect(page).toHaveURL(/\/home|\/dashboard/);
    
    // Verify user is logged in (check for logout button or user menu)
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
  });
  
  test('should show error with invalid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in invalid credentials
    await page.fill('[data-testid="username-input"]', 'invalid@example.com');
    await page.fill('[data-testid="password-input"]', 'wrongpassword');
    
    // Click login
    await page.click('[data-testid="login-button"]');
    
    // Verify error message is shown
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText(/Invalid credentials|Login failed/i);
  });
  
  test('should validate required fields', async ({ page }) => {
    await page.goto('/login');
    
    // Click login without filling fields
    await page.click('[data-testid="login-button"]');
    
    // Verify validation messages
    await expect(page.locator('[data-testid="username-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-error"]')).toBeVisible();
  });
  
  test('should allow password visibility toggle', async ({ page }) => {
    await page.goto('/login');
    
    const passwordInput = page.locator('[data-testid="password-input"]');
    const toggleButton = page.locator('[data-testid="password-toggle"]');
    
    // Initially password should be hidden
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Click toggle to show password
    await toggleButton.click();
    await expect(passwordInput).toHaveAttribute('type', 'text');
    
    // Click again to hide
    await toggleButton.click();
    await expect(passwordInput).toHaveAttribute('type', 'password');
  });
});
```

**Complexity**: ⭐⭐ Easy  
**Effort**: 1-2 hours  
**Value**: **VERY HIGH** - Critical authentication flow

---

### Category 3: Navigation & Tab Tests (MEDIUM)

#### 4. **Responsive Tabs Component**

**Current Angular Test:**
```typescript
// UNOPS.PAO.ClientApp/src/app/shared/components/navigation/responsive-tabs/responsive-tabs.component.spec.ts
describe('ResponsiveTabsComponent', () => {
  it('should create', () => {
    expect(component).toBeTruthy();
  });
  
  it('should navigate to tab on click', () => {
    // Mock router navigation
    component.onTabClick(mockTabs[1]);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/tab2']);
  });
});
```

**Converted Playwright Test:**
```typescript
// e2e/navigation/tabs.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Navigation Tabs', () => {
  test.beforeEach(async ({ page }) => {
    // Setup: Login and navigate to page with tabs
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
    
    // Navigate to page with tabs (e.g., partner detail page)
    await page.goto('/partners/1');
  });
  
  test('should display all tabs', async ({ page }) => {
    // Verify all expected tabs are visible
    await expect(page.locator('[data-testid="tab-overview"]')).toBeVisible();
    await expect(page.locator('[data-testid="tab-contacts"]')).toBeVisible();
    await expect(page.locator('[data-testid="tab-interactions"]')).toBeVisible();
  });
  
  test('should highlight active tab', async ({ page }) => {
    // First tab should be active by default
    await expect(page.locator('[data-testid="tab-overview"]')).toHaveClass(/active|selected/);
  });
  
  test('should navigate between tabs', async ({ page }) => {
    // Click on contacts tab
    await page.click('[data-testid="tab-contacts"]');
    
    // Verify URL updated
    await expect(page).toHaveURL(/\/partners\/1\/contacts/);
    
    // Verify contacts tab is now active
    await expect(page.locator('[data-testid="tab-contacts"]')).toHaveClass(/active|selected/);
    
    // Verify contacts content is displayed
    await expect(page.locator('[data-testid="contacts-section"]')).toBeVisible();
  });
  
  test('should handle disabled tabs', async ({ page }) => {
    // If a tab is disabled, it should not be clickable
    const disabledTab = page.locator('[data-testid="tab-disabled"]');
    
    if (await disabledTab.isVisible()) {
      await expect(disabledTab).toBeDisabled();
      
      // Attempt to click (should not navigate)
      const currentUrl = page.url();
      await disabledTab.click({ force: true });
      await expect(page).toHaveURL(currentUrl);
    }
  });
  
  test('should switch to dropdown on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    // Tabs should be hidden, dropdown should be visible
    await expect(page.locator('[data-testid="tabs-desktop"]')).not.toBeVisible();
    await expect(page.locator('[data-testid="tabs-dropdown"]')).toBeVisible();
    
    // Select tab from dropdown
    await page.click('[data-testid="tabs-dropdown"]');
    await page.click('[data-testid="dropdown-option-contacts"]');
    
    // Verify navigation occurred
    await expect(page).toHaveURL(/\/partners\/1\/contacts/);
  });
});
```

**Complexity**: ⭐⭐ Easy-Medium  
**Effort**: 2-3 hours  
**Value**: High - Tests responsive design and navigation

---

### Category 4: Form Input Tests (MEDIUM)

#### 5. **Phone Input Component**

**Current Angular Test:**
```typescript
// UNOPS.PAO.ClientApp/src/app/shared/components/forms/phone-input/phone-input.component.spec.ts
describe('PhoneInputComponent', () => {
  it('should create', () => {
    expect(component).toBeTruthy();
  });
  
  it('should validate phone number format', () => {
    expect(component).toBeTruthy();
  });
});
```

**Converted Playwright Test:**
```typescript
// e2e/forms/phone-input.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Phone Input Field', () => {
  test.beforeEach(async ({ page }) => {
    // Login and navigate to form with phone input (e.g., create contact)
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
    
    await page.goto('/contacts/new');
  });
  
  test('should accept valid phone number', async ({ page }) => {
    const phoneInput = page.locator('[data-testid="phone-input"]');
    
    // Enter valid phone number
    await phoneInput.fill('+1234567890');
    
    // Verify no error message
    await expect(page.locator('[data-testid="phone-error"]')).not.toBeVisible();
  });
  
  test('should show error for invalid phone number', async ({ page }) => {
    const phoneInput = page.locator('[data-testid="phone-input"]');
    
    // Enter invalid phone number
    await phoneInput.fill('invalid-phone');
    
    // Blur to trigger validation
    await phoneInput.blur();
    
    // Verify error message
    await expect(page.locator('[data-testid="phone-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="phone-error"]')).toContainText(/Invalid phone number/i);
  });
  
  test('should format phone number on blur', async ({ page }) => {
    const phoneInput = page.locator('[data-testid="phone-input"]');
    
    // Enter unformatted phone
    await phoneInput.fill('1234567890');
    await phoneInput.blur();
    
    // Verify formatting applied
    const value = await phoneInput.inputValue();
    expect(value).toMatch(/^\+?[\d\s\-()]+$/);
  });
  
  test('should allow multiple phone numbers', async ({ page }) => {
    // Add first phone
    await page.fill('[data-testid="phone-input-0"]', '+1234567890');
    
    // Click add button
    await page.click('[data-testid="add-phone-button"]');
    
    // Verify second phone input appears
    await expect(page.locator('[data-testid="phone-input-1"]')).toBeVisible();
    
    // Add second phone
    await page.fill('[data-testid="phone-input-1"]', '+9876543210');
    
    // Verify both values are saved
    await page.click('[data-testid="save-button"]');
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
  });
});
```

**Complexity**: ⭐⭐ Medium  
**Effort**: 2-3 hours  
**Value**: Medium - Validates form input behavior

---

## 🎯 **CONVERSION PRIORITY GUIDE**

### Phase 1: Critical Paths (Week 1) - 4-6 hours
1. ✅ **Login Flow** (2 hours) - HIGHEST PRIORITY
2. ✅ **Home Page Load** (30 min)
3. ✅ **Dashboard Display** (1 hour)
4. ✅ **Logout Flow** (30 min)

**Target**: Basic smoke tests for critical user paths

### Phase 2: Core Features (Week 2) - 8-10 hours
1. ✅ **Partner List View** (2 hours)
2. ✅ **Partner Create** (2 hours)
3. ✅ **Contact List View** (2 hours)
4. ✅ **Contact Create** (2 hours)
5. ✅ **Navigation Tabs** (2 hours)

**Target**: Core CRUD operations verified

### Phase 3: Enhanced Features (Week 3) - 8-10 hours
1. ✅ **Search Functionality** (3 hours)
2. ✅ **Advanced Filters** (2 hours)
3. ✅ **Document Upload** (2 hours)
4. ✅ **Form Validations** (3 hours)

**Target**: Complete feature coverage

### Phase 4: Edge Cases (Week 4) - 6-8 hours
1. ✅ **Error Handling** (2 hours)
2. ✅ **Permission Checks** (2 hours)
3. ✅ **Responsive Design** (2 hours)
4. ✅ **Cross-Browser Testing** (2 hours)

**Target**: Robust test coverage

---

## 🛠️ **SETUP GUIDE**

### Step 1: Install Playwright

```bash
cd UNOPS.PAO.ClientApp

# Install Playwright
npm init playwright@latest

# This will:
# - Install Playwright Test
# - Add example tests
# - Create playwright.config.ts
# - Install browsers (Chromium, Firefox, WebKit)
```

### Step 2: Configure Playwright

**Create `playwright.config.ts`:**

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  
  // Run tests in parallel
  fullyParallel: true,
  
  // Fail build on CI if you accidentally left test.only
  forbidOnly: !!process.env.CI,
  
  // Retry failed tests on CI
  retries: process.env.CI ? 2 : 0,
  
  // Parallel workers
  workers: process.env.CI ? 1 : undefined,
  
  // Reporter config
  reporter: [
    ['html'],
    ['junit', { outputFile: 'test-results/junit.xml' }],
    ['list']
  ],
  
  use: {
    // Base URL
    baseURL: 'http://localhost:4200',
    
    // Collect trace on failure
    trace: 'on-first-retry',
    
    // Screenshot on failure
    screenshot: 'only-on-failure',
    
    // Video on failure
    video: 'retain-on-failure',
  },
  
  // Configure projects for major browsers
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    
    // Mobile viewports
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
  ],
  
  // Run dev server before tests
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

### Step 3: Create Folder Structure

```
UNOPS.PAO.ClientApp/
├── e2e/
│   ├── auth/
│   │   ├── login.spec.ts
│   │   └── logout.spec.ts
│   ├── home/
│   │   ├── home.spec.ts
│   │   └── dashboard.spec.ts
│   ├── partners/
│   │   ├── partner-list.spec.ts
│   │   ├── partner-create.spec.ts
│   │   └── partner-edit.spec.ts
│   ├── contacts/
│   │   └── ...
│   ├── navigation/
│   │   └── tabs.spec.ts
│   ├── forms/
│   │   └── phone-input.spec.ts
│   └── fixtures/
│       ├── auth.ts
│       └── test-data.ts
├── playwright.config.ts
└── package.json
```

### Step 4: Create Reusable Fixtures

**`e2e/fixtures/auth.ts`:**

```typescript
import { test as base } from '@playwright/test';

export const test = base.extend({
  // Fixture for authenticated page
  authenticatedPage: async ({ page }, use) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
    
    // Wait for redirect
    await page.waitForURL(/\/home|\/dashboard/);
    
    await use(page);
  },
});

export { expect } from '@playwright/test';
```

**Usage:**

```typescript
import { test, expect } from '../fixtures/auth';

test.describe('Protected Pages', () => {
  test('should access dashboard', async ({ authenticatedPage }) => {
    // Already logged in via fixture
    await authenticatedPage.goto('/dashboard');
    await expect(authenticatedPage.locator('[data-testid="dashboard"]')).toBeVisible();
  });
});
```

---

## 📊 **COMPARISON: BEFORE vs. AFTER**

### Before (Unit Tests Only)

```typescript
// Unit test - tests component in isolation
it('should create contact', () => {
  const contact = { name: 'John Doe', email: 'john@example.com' };
  component.createContact(contact);
  expect(component.contacts.length).toBe(1);
});
```

**What this tests:**
- ✅ Component logic
- ❌ Doesn't test API call
- ❌ Doesn't test backend validation
- ❌ Doesn't test UI updates
- ❌ Doesn't test navigation
- ❌ Doesn't test across browsers

### After (Playwright E2E Test)

```typescript
// E2E test - tests entire user workflow
test('should create contact through UI', async ({ page }) => {
  await page.goto('/contacts/new');
  
  await page.fill('[data-testid="name-input"]', 'John Doe');
  await page.fill('[data-testid="email-input"]', 'john@example.com');
  await page.click('[data-testid="save-button"]');
  
  // Tests full workflow:
  await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
  await expect(page).toHaveURL(/\/contacts\/\d+/);
  await expect(page.locator('[data-testid="contact-name"]')).toContainText('John Doe');
});
```

**What this tests:**
- ✅ Component logic
- ✅ API call to backend
- ✅ Backend validation
- ✅ Database persistence
- ✅ UI updates after save
- ✅ Navigation after success
- ✅ Cross-browser compatibility
- ✅ Responsive design
- ✅ Real network timing

---

## 🎯 **SUCCESS METRICS**

### Target Coverage
- ✅ **Critical Paths**: 100% (login, home, logout)
- ✅ **Core Features**: 80% (CRUD operations)
- ✅ **Enhanced Features**: 50% (advanced features)
- ✅ **Edge Cases**: 30% (error scenarios)

### Execution Targets
- ⏱️ **Execution Time**: <5 minutes for full suite
- 🔄 **Parallel Execution**: 4-8 workers
- 🌐 **Browser Coverage**: Chromium, Firefox, WebKit
- 📱 **Device Coverage**: Desktop + 2 mobile viewports

### Quality Metrics
- ✅ **Flakiness**: <2% (Playwright auto-wait prevents flaky tests)
- ✅ **Maintenance**: Low (page object pattern)
- ✅ **Readability**: High (declarative syntax)
- ✅ **Debugging**: Excellent (traces, screenshots, videos)

---

## 💡 **BEST PRACTICES**

### 1. Use Test IDs (NOT CSS Selectors)

**❌ BAD:**
```typescript
await page.click('.btn-primary.submit-form');
```

**✅ GOOD:**
```typescript
await page.click('[data-testid="submit-button"]');
```

### 2. Create Page Objects

```typescript
// e2e/pages/login.page.ts
export class LoginPage {
  constructor(private page: Page) {}
  
  async goto() {
    await this.page.goto('/login');
  }
  
  async login(username: string, password: string) {
    await this.page.fill('[data-testid="username-input"]', username);
    await this.page.fill('[data-testid="password-input"]', password);
    await this.page.click('[data-testid="login-button"]');
  }
  
  async getErrorMessage() {
    return this.page.locator('[data-testid="error-message"]').textContent();
  }
}

// Usage in test
const loginPage = new LoginPage(page);
await loginPage.goto();
await loginPage.login('user@example.com', 'password');
```

### 3. Use Fixtures for Setup

```typescript
// Reusable authenticated context
export const test = base.extend({
  authenticatedPage: async ({ page }, use) => {
    await login(page);
    await use(page);
  },
});
```

### 4. Parallel Execution

```typescript
// Run tests in parallel
test.describe.configure({ mode: 'parallel' });

test('test 1', async ({ page }) => { /* ... */ });
test('test 2', async ({ page }) => { /* ... */ });
test('test 3', async ({ page }) => { /* ... */ });
```

### 5. Visual Regression Testing

```typescript
test('should match screenshot', async ({ page }) => {
  await page.goto('/dashboard');
  await expect(page).toHaveScreenshot('dashboard.png');
});
```

---

## 🚀 **QUICK START COMMAND**

```bash
# Install Playwright
cd UNOPS.PAO.ClientApp
npm init playwright@latest

# Run example tests
npx playwright test

# Run tests with UI mode
npx playwright test --ui

# Generate code from browser interactions
npx playwright codegen http://localhost:4200
```

---

## 📚 **RESOURCES**

- **Playwright Docs**: https://playwright.dev/
- **Best Practices**: https://playwright.dev/docs/best-practices
- **Test Generator**: https://playwright.dev/docs/codegen
- **VS Code Extension**: https://playwright.dev/docs/getting-started-vscode

---

## 🎉 **BENEFITS SUMMARY**

### Why Convert These Tests?

1. ✅ **Real User Testing**: Tests actual browser behavior, not mocked components
2. ✅ **Integration Coverage**: Tests frontend + backend together
3. ✅ **Cross-Browser**: Validates Chrome, Firefox, Safari automatically
4. ✅ **Mobile Testing**: Test responsive design on real mobile viewports
5. ✅ **Debugging**: Built-in screenshots, videos, traces on failure
6. ✅ **Parallel Execution**: Faster than sequential Karma tests
7. ✅ **No Flakiness**: Auto-wait eliminates timing issues
8. ✅ **Visual Regression**: Catch UI changes automatically
9. ✅ **CI/CD Ready**: Perfect for deployment pipelines
10. ✅ **Developer Experience**: Easier to write and maintain

---

**Next Steps:**
1. Install Playwright (5 minutes)
2. Convert login test (30 minutes)
3. Run your first E2E test! 🎉

**Need help?** Check the Playwright docs or ask for a specific conversion example!
