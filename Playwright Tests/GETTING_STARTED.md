# Getting Started with Playwright Tests

## 📋 Prerequisites

1. **Node.js** installed (v18+)
2. **Angular app running** at `http://localhost:4200`
3. **Playwright installed** (already configured in this project)

---

## 🚀 First Time Setup

### 1. Install Dependencies

```bash
# From repository root
npm install
```

This installs Playwright and all required browsers (Chrome, Firefox, Safari).

### 2. Verify Installation

```bash
# Check Playwright is installed
npx playwright --version
```

---

## 🎯 Running Your First Test

### Option 1: UI Mode (Recommended for First Time)

```bash
npm run test:ui
```

This opens an interactive window where you can:
- ✅ See all available tests
- ✅ Click to run individual tests
- ✅ Watch tests execute in real-time
- ✅ See detailed results instantly

### Option 2: Run All Tests (Headless)

```bash
npm run test
```

Tests run in the background across all browsers (Chrome, Firefox, Safari).

### Option 3: Run with Visible Browser

```bash
npm run test:headed
```

Watch Chrome execute your tests (helpful for debugging).

---

## ✍️ Writing Your First Test

### Step 1: Create a Test File

```bash
# Create tests folder structure
mkdir -p "Playwright Tests/auth"

# Create your first test
touch "Playwright Tests/auth/login.spec.ts"
```

### Step 2: Write the Test

```typescript
import { test, expect } from '@playwright/test';

test.describe('Login Flow', () => {
  test('should successfully login', async ({ page }) => {
    // Navigate to login page
    await page.goto('/login');
    
    // Fill in credentials
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password123');
    
    // Click login button
    await page.click('[data-testid="login-button"]');
    
    // Verify redirect to dashboard
    await expect(page).toHaveURL(/\/dashboard|\/home/);
    
    // Verify user is logged in
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
  });
});
```

### Step 3: Add Data Test IDs to Your Components

**In your Angular components**, add `data-testid` attributes:

```html
<!-- Login Component Template -->
<input 
  data-testid="username-input"
  [(ngModel)]="username" 
  type="text"
/>

<input 
  data-testid="password-input"
  [(ngModel)]="password" 
  type="password"
/>

<button 
  data-testid="login-button"
  (click)="login()">
  Login
</button>
```

### Step 4: Run Your Test

```bash
# Run just your new test
npx playwright test auth/login.spec.ts

# Or run in UI mode
npm run test:ui
```

---

## 🧪 Test Organization

### Recommended Folder Structure

```
Playwright Tests/
├── auth/
│   ├── login.spec.ts
│   ├── logout.spec.ts
│   └── signup.spec.ts
├── partners/
│   ├── partner-list.spec.ts
│   ├── partner-create.spec.ts
│   └── partner-edit.spec.ts
├── contacts/
│   ├── contact-list.spec.ts
│   └── contact-create.spec.ts
├── fixtures/
│   ├── auth.fixture.ts      # Reusable authentication
│   └── test-data.fixture.ts # Test data helpers
└── README.md
```

---

## 🛠️ Common Commands

### Run Specific Tests

```bash
# Run single file
npx playwright test auth/login.spec.ts

# Run tests matching pattern
npx playwright test -g "should login"

# Run specific browser only
npm run test:chrome
npm run test:firefox
npm run test:safari
```

### Debugging

```bash
# Debug mode (pause execution, step through)
npm run test:debug

# Run with browser visible
npm run test:headed

# Generate test code from browser interactions
npm run test:codegen
```

### View Results

```bash
# View HTML report
npm run test:report

# View trace of failed test
npx playwright show-trace test-results/trace.zip
```

---

## 💡 Pro Tips

### 1. Use Page Objects for Reusability

```typescript
// pages/login.page.ts
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
}

// In your test
const loginPage = new LoginPage(page);
await loginPage.goto();
await loginPage.login('user@example.com', 'password');
```

### 2. Create Authentication Fixtures

```typescript
// fixtures/auth.ts
import { test as base } from '@playwright/test';

export const test = base.extend({
  authenticatedPage: async ({ page }, use) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('[data-testid="username-input"]', 'testuser@unops.org');
    await page.fill('[data-testid="password-input"]', 'password');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL(/\/dashboard|\/home/);
    
    await use(page);
  },
});

// In your test - already logged in!
import { test, expect } from './fixtures/auth';

test('should access protected page', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/partners');
  // Already authenticated, no login needed
});
```

### 3. Use Test Generator

```bash
npm run test:codegen
```

This opens a browser where you can:
- Click around your app
- Playwright generates test code automatically
- Copy/paste into your test files

---

## 🐛 Troubleshooting

### Tests Timeout

**Problem**: Tests fail with timeout errors

**Solution**: Increase timeout in `playwright.config.ts`:

```typescript
export default defineConfig({
  timeout: 60000, // 60 seconds per test
  // ...
});
```

### Browser Not Found

**Problem**: Error about missing browsers

**Solution**: Install browsers:

```bash
npx playwright install
```

### Angular App Not Starting

**Problem**: Tests fail because app isn't running

**Solution**: The config auto-starts the app, but you can start it manually:

```bash
cd UNOPS.PAO.ClientApp
npm start
```

Then run tests with:

```bash
npm run test
```

---

## 📚 Resources

- **Playwright Documentation**: https://playwright.dev/
- **Best Practices**: https://playwright.dev/docs/best-practices
- **API Reference**: https://playwright.dev/docs/api/class-playwright
- **Test Generator**: https://playwright.dev/docs/codegen
- **VS Code Extension**: https://playwright.dev/docs/getting-started-vscode

---

## 🎯 Next Steps

1. **Delete `example.spec.ts`** - It's just a demo
2. **Create your first real test** - Start with login flow
3. **Add data-testid attributes** - To key UI elements
4. **Run tests in CI/CD** - Already configured in `.github/workflows/playwright.yml`

---

## 🤝 Contributing

When adding new tests:

1. ✅ Use meaningful test descriptions
2. ✅ Use `data-testid` for selectors (not CSS classes)
3. ✅ Keep tests independent (each test should work alone)
4. ✅ Use Page Objects for complex pages
5. ✅ Add comments for complex test logic
6. ✅ Group related tests in describe blocks

---

**Happy Testing!** 🎉
