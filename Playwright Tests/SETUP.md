# Playwright E2E Tests - Setup Guide

Complete setup guide for UNOPS Opportunity+ Playwright E2E test suite.

## 📋 Prerequisites

- **Node.js**: v18+ (v20 recommended)
- **npm**: v9+
- **Angular Dev Server**: Running at `http://localhost:4200`
- **Backend API**: Running and accessible

## 🚀 Quick Start

### 1. Install Dependencies

```bash
# Install Playwright and dependencies
npm install

# Install Playwright browsers
npx playwright install
```

### 2. Configure Test Credentials

```bash
# Copy example environment file
cp .env.example .env
```

Edit `.env` with your test credentials:

```env
# Test User Credentials
TEST_USER_EMAIL=your-test-user@unops.org
TEST_USER_PASSWORD=YourSecurePassword123!

# Base URLs (adjust if different)
BASE_URL=http://localhost:4200
API_BASE_URL=http://localhost:5000
```

### 3. Run Tests

```bash
# Run all tests
npm test

# Run with UI (recommended for first run)
npm run test:ui
```

---

## 📦 Installation Details

### Install Playwright Browsers

Playwright requires browser binaries. Install them with:

```bash
npx playwright install
```

This installs Chromium, Firefox, and WebKit.

**Install specific browser:**
```bash
npx playwright install chromium
npx playwright install firefox
npx playwright install webkit
```

### Install System Dependencies (Linux only)

On Linux, you may need system dependencies:

```bash
npx playwright install-deps
```

---

## ⚙️ Configuration

### Environment Variables

The test suite uses environment variables for configuration. All variables have sensible defaults.

**Required:**
- `TEST_USER_EMAIL` - Test user email address
- `TEST_USER_PASSWORD` - Test user password

**Optional:**
- `BASE_URL` - Application URL (default: `http://localhost:4200`)
- `API_BASE_URL` - API base URL (default: `http://localhost:5000`)
- `DEFAULT_TIMEOUT` - Default timeout in ms (default: `10000`)
- `LONG_TIMEOUT` - Long timeout in ms (default: `30000`)
- `SHORT_TIMEOUT` - Short timeout in ms (default: `5000`)
- `HEADLESS` - Run headless (default: `false`)
- `DEBUG` - Enable debug mode (default: `false`)

### Playwright Configuration

Edit `playwright.config.ts` to customize:

- **Test directory**: `testDir: './Playwright Tests'`
- **Base URL**: `baseURL: 'http://localhost:4200'`
- **Browser projects**: Chromium, Firefox, WebKit
- **Retries**: Configured for CI
- **Reporters**: HTML report generation
- **Web server**: Auto-starts Angular dev server

---

## 🧪 Running Tests

### Basic Commands

```bash
# Run all tests
npm test

# Run with UI (interactive mode)
npm run test:ui

# Run in headed mode (see browser)
npm run test:headed

# Debug mode (step through)
npm run test:debug
```

### Browser-Specific Tests

```bash
# Chrome only
npm run test:chrome

# Firefox only
npm run test:firefox

# Safari (WebKit) only
npm run test:safari
```

### Specific Test Files

```bash
# Run single test file
npx playwright test login.spec.ts

# Run multiple specific files
npx playwright test login.spec.ts partners.spec.ts

# Run tests in folder
npx playwright test Playwright\ Tests/
```

### Filter by Test Name

```bash
# Run tests matching pattern
npx playwright test --grep "should display"

# Exclude tests matching pattern
npx playwright test --grep-invert "mobile"
```

### Parallel Execution

```bash
# Run in parallel (default)
npx playwright test

# Run in serial
npx playwright test --workers=1

# Specify number of workers
npx playwright test --workers=4
```

---

## 📊 Reports and Debugging

### View Test Report

After test run:

```bash
npm run test:report
```

Opens HTML report in browser with:
- Test results and timings
- Screenshots on failure
- Video recordings (if enabled)
- Trace files for debugging

### Generate Test Code

Use Playwright's codegen to record interactions:

```bash
npm run test:codegen
```

This opens a browser and generates test code as you interact with the application.

### Trace Viewer

View detailed trace of test execution:

```bash
npx playwright show-trace trace.zip
```

Traces include:
- Network activity
- Console logs
- Screenshots at each step
- DOM snapshots

### Screenshots

Automatically captured on failure. View in HTML report or:

```bash
# Find in test-results folder
ls test-results/*/screenshot.png
```

---

## 🏗️ Project Structure

```
Playwright Tests/
├── helpers/                    # Reusable helper functions
│   ├── test-config.ts         # Configuration management
│   ├── auth.helper.ts         # Authentication helpers
│   ├── wait.helper.ts         # Wait utilities
│   ├── navigation.helper.ts   # Navigation helpers
│   └── assertions.helper.ts   # Assertion helpers
├── pages/                      # Page Object Models
│   ├── base.page.ts           # Base page class
│   ├── entity-list.page.ts    # Base for list pages
│   ├── login.page.ts          # Login page
│   ├── dashboard.page.ts      # Dashboard page
│   ├── partners.page.ts       # Partners page
│   ├── contacts.page.ts       # Contacts page
│   └── opportunities.page.ts  # Opportunities page
├── *.spec.ts                   # Test specification files
├── .env                        # Environment config (git-ignored)
├── .env.example                # Example environment file
├── SETUP.md                    # This file
├── HELPERS_README.md           # Helper documentation
└── README.md                   # Test suite overview
```

---

## 🔧 Troubleshooting

### Tests Fail with "Cannot find test user"

**Problem:** Test credentials not configured.

**Solution:** Create `.env` file with valid test credentials:
```bash
cp .env.example .env
# Edit .env with valid credentials
```

### "Browser not found" Error

**Problem:** Playwright browsers not installed.

**Solution:**
```bash
npx playwright install
```

### Angular Dev Server Not Starting

**Problem:** Port 4200 already in use or Angular not starting.

**Solution:**
1. Check if Angular is running: `http://localhost:4200`
2. Start manually: `cd UNOPS.PAO.ClientApp && npm start`
3. Update `playwright.config.ts` if using different port

### Timeout Errors

**Problem:** Default timeouts too short for slow environment.

**Solution:** Increase timeouts in `.env`:
```env
DEFAULT_TIMEOUT=20000
LONG_TIMEOUT=60000
```

### Permission-Dependent Tests Fail

**Problem:** Test user lacks necessary permissions.

**Solution:** 
1. Verify test user has appropriate roles
2. Tests gracefully handle missing permissions (check test logs)
3. Update test expectations if needed

### Network Errors

**Problem:** API not accessible or wrong URL.

**Solution:** Verify API URL in `.env`:
```env
API_BASE_URL=http://localhost:5000
```

### Database State Issues

**Problem:** Tests fail due to unexpected database state.

**Solution:**
1. Use isolated test data
2. Clean up after tests
3. Consider database reset between test runs

---

## 🎯 Best Practices

### 1. Use Environment Variables

```typescript
// ✅ GOOD
const credentials = getTestCredentials();

// ❌ BAD
const email = 'hardcoded@email.com';
```

### 2. Use Page Objects

```typescript
// ✅ GOOD
await partnersPage.clickNewButton();

// ❌ BAD
await page.locator('[data-testid="new-partner-button"]').click();
```

### 3. Use Helpers for Common Operations

```typescript
// ✅ GOOD
await loginAndNavigate(page, '/partners');

// ❌ BAD
await page.goto('/login');
// ... repeated login code
```

### 4. Handle Permissions Gracefully

```typescript
// ✅ GOOD
if (await partnersPage.isNewButtonVisible()) {
  await partnersPage.clickNewButton();
}

// ❌ BAD
await partnersPage.clickNewButton(); // Fails if no permission
```

### 5. Use Smart Waits

```typescript
// ✅ GOOD
await smartWait(page);

// ❌ BAD
await page.waitForTimeout(5000);
```

---

## 🔄 Continuous Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: npm install
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
      - name: Run tests
        env:
          TEST_USER_EMAIL: ${{ secrets.TEST_USER_EMAIL }}
          TEST_USER_PASSWORD: ${{ secrets.TEST_USER_PASSWORD }}
        run: npm test
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: playwright-report/
```

### Environment Variables in CI

Set secrets in GitHub repository settings:
- `TEST_USER_EMAIL`
- `TEST_USER_PASSWORD`

---

## 📚 Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Helper Functions](./HELPERS_README.md)
- [Test Suite Summary](./TEST_SUITE_SUMMARY.md)
- [Getting Started Guide](./GETTING_STARTED.md)

---

## 🆘 Getting Help

1. Check [Playwright Documentation](https://playwright.dev/docs/intro)
2. Review [HELPERS_README.md](./HELPERS_README.md) for usage examples
3. Run tests with `--debug` flag for step-by-step debugging
4. Use `test:ui` mode for interactive debugging

---

## 📝 Next Steps

After setup:

1. ✅ Run test suite: `npm run test:ui`
2. ✅ Review test results and reports
3. ✅ Add new tests using page objects and helpers
4. ✅ Integrate into CI/CD pipeline
5. ✅ Maintain test data and credentials

Happy Testing! 🎉
