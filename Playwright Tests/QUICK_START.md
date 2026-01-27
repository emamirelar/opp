# Quick Start Guide - Playwright E2E Tests

Get started with UNOPS Opportunity+ E2E tests in 5 minutes! ⚡

## ⚡ 3-Step Quick Start

### Step 1: Install (30 seconds)

```bash
npm install
npx playwright install
```

### Step 2: Configure (1 minute)

```bash
cp .env.example .env
```

Edit `.env` with your test credentials:
```env
TEST_USER_EMAIL=your-test-user@unops.org
TEST_USER_PASSWORD=YourPassword123!
```

### Step 3: Run (3 seconds)

```bash
npm run test:ui
```

✅ **You're done!** The interactive test UI will open.

---

## 🎯 Essential Commands

| Command | What It Does |
|---------|--------------|
| `npm test` | Run all tests (headless) |
| `npm run test:ui` | Interactive test UI (recommended) |
| `npm run test:headed` | Run tests with visible browser |
| `npm run test:debug` | Step through tests with debugger |
| `npm run test:report` | View HTML test report |

---

## 📝 Writing Your First Test

### 1. Use Page Objects (Recommended)

```typescript
import { test } from '@playwright/test';
import { PartnersPage } from './pages/partners.page';
import { loginAndNavigate } from './helpers/auth.helper';

test('my partner test', async ({ page }) => {
  // Login and navigate
  const partnersPage = new PartnersPage(page);
  await loginAndNavigate(page, '/partners');
  
  // Verify page
  await partnersPage.verifyPageHeader();
  
  // Perform actions
  if (await partnersPage.isNewButtonVisible()) {
    await partnersPage.clickNewButton();
  }
});
```

### 2. Or Use Helpers Directly

```typescript
import { test } from '@playwright/test';
import { login } from './helpers/auth.helper';
import { assertVisible } from './helpers/assertions.helper';

test('my simple test', async ({ page }) => {
  // Login
  await login(page);
  
  // Navigate
  await page.goto('/partners');
  
  // Assert
  await assertVisible(page.locator('[data-testid="partners-header"]'));
});
```

---

## 🔍 Test Selectors

**Always use `data-testid` attributes:**

```html
<!-- Good selector -->
<button data-testid="new-partner-button">New Partner</button>
```

```typescript
// Find in test
const button = page.locator('[data-testid="new-partner-button"]');
await button.click();
```

---

## 🎨 Test UI Features

When you run `npm run test:ui`, you get:

✅ **Pick & Choose** - Select which tests to run
✅ **Watch Mode** - Auto-rerun on file changes
✅ **Time Travel** - Scrub through test execution
✅ **Assertions** - See exactly what failed
✅ **Network** - View all API calls
✅ **Console** - See console logs

**Pro Tip:** Click the eyeball icon 👁️ to watch tests in browser!

---

## 🐛 Debugging Tips

### Visual Debugging

```bash
npm run test:debug
```

- Pauses before each action
- Shows browser UI
- Step through with toolbar
- Inspect element states

### Screenshots on Failure

Automatically captured! Find them in:
```
test-results/
  ├── test-name/
  │   ├── screenshot.png
  │   └── trace.zip
```

### View Traces

```bash
npx playwright show-trace test-results/trace.zip
```

Shows complete test timeline with:
- Network requests
- DOM snapshots
- Console logs
- Screenshots at each step

---

## 📁 Project Structure (Key Files)

```
Playwright Tests/
├── helpers/           # Reusable utilities
│   ├── auth.helper.ts       # Login helpers
│   ├── wait.helper.ts       # Wait utilities
│   └── assertions.helper.ts # Common assertions
├── pages/             # Page objects
│   ├── login.page.ts
│   ├── partners.page.ts
│   └── dashboard.page.ts
├── *.spec.ts         # Your test files
└── .env              # Your credentials (git-ignored)
```

---

## 🚨 Common Issues & Fixes

### "Browser not found"
```bash
npx playwright install
```

### "Cannot find test user"
```bash
# Create .env with credentials
cp .env.example .env
# Edit .env
```

### "Timeout waiting for element"
```typescript
// Increase timeout
await element.waitFor({ timeout: 30000 });
```

### "Port 4200 not available"
```bash
# Start Angular manually
cd UNOPS.PAO.ClientApp
npm start
```

---

## 📚 Learn More

- **[SETUP.md](./SETUP.md)** - Complete setup guide
- **[HELPERS_README.md](./HELPERS_README.md)** - Helper documentation
- **[Playwright Docs](https://playwright.dev/)** - Official documentation

---

## 🎯 Next Steps

1. ✅ Run existing tests: `npm run test:ui`
2. ✅ Explore test files: `*.spec.ts`
3. ✅ Learn page objects: `pages/*.page.ts`
4. ✅ Write your first test
5. ✅ Check out helpers: `helpers/*`

---

## 💡 Pro Tips

### Tip 1: Use the Test Generator

```bash
npm run test:codegen
```

Playwright records your actions and generates test code!

### Tip 2: Run One Test

```bash
npx playwright test login.spec.ts
```

### Tip 3: Filter Tests

```bash
npx playwright test --grep "should display"
```

### Tip 4: Parallel Testing

Tests run in parallel by default. Speed up with more workers:

```bash
npx playwright test --workers=4
```

### Tip 5: Headed Mode for Debugging

```bash
npm run test:headed
```

See the browser while tests run!

---

## ✨ Best Practices

1. **Use Page Objects** - Cleaner, reusable code
2. **Use Helpers** - Don't repeat yourself
3. **Use data-testid** - Stable selectors
4. **Handle Permissions** - Check before clicking
5. **Smart Waits** - Let Playwright handle timing

---

## 🎉 You're Ready!

Now run your first test:

```bash
npm run test:ui
```

Happy Testing! 🚀
