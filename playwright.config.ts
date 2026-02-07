import { defineConfig, devices } from '@playwright/test';
import path from 'path';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// import dotenv from 'dotenv';
// dotenv.config({ path: path.resolve(__dirname, '.env') });

// Get absolute paths based on config file location
const configDir = __dirname;
const testDir = path.join(configDir, 'QA Tests', 'Playwright Tests');
const clientAppDir = path.join(configDir, 'UNOPS.PAO.ClientApp');

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: testDir,
  /* Output directory for test results and reports */
  outputDir: path.join(testDir, 'test-results'),
  /* Maximum time one test can run for */
  timeout: 60000,  // 60 seconds per test (reduced from 120s - mocked tests are fast)
  /* Maximum time expect() should wait for the condition to be met */
  expect: {
    timeout: 10000,  // 10 seconds for assertions
  },
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only - reduced from 2 to 1 to speed up CI */
  retries: process.env.CI ? 1 : 0,
  /* Limit workers to prevent overwhelming the Angular dev server
   * - CI: 4 workers (reduced from 6 for stability)
   * - Local: 2 workers (was unlimited, causing server crashes with 449 tests)
   */
  workers: process.env.CI ? 4 : 2,
  /* Stop test run early if too many tests fail (indicates server crash or systemic issue) */
  maxFailures: process.env.CI ? 50 : 20,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [['html', { outputFolder: path.join(testDir, 'playwright-report') }]],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('')`. */
    baseURL: 'http://127.0.0.1:4200',  // ← Your Angular app URL (using IPv4)

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
    
    /* Screenshot on failure */
    screenshot: 'only-on-failure',
    
    /* Video on failure */
    video: 'retain-on-failure',
    
    /* Navigation timeout - give server time to respond under load */
    navigationTimeout: 30000,  // 30 seconds for navigation
    
    /* Action timeout for clicks, fills, etc. */
    actionTimeout: 15000,  // 15 seconds for actions
  },

  /* Configure projects for major browsers */
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
      use: { 
        ...devices['Desktop Safari'],
        // Webkit (Safari) needs 2-3x longer timeouts than Chromium/Firefox
        // due to slower page load and stricter security model
        navigationTimeout: 120000,  // 2 minutes for navigation (vs 30s default)
        actionTimeout: 60000,        // 1 minute for actions (vs 30s default)
      },
      // Webkit tests take longer to complete
      timeout: 180000,  // 3 minutes per test (vs 60s default)
      expect: {
        timeout: 15000,  // 15 seconds for assertions (vs 10s default)
      },
    },

    /* Test against mobile viewports. */
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
    // {
    //   name: 'Mobile Safari',
    //   use: { ...devices['iPhone 12'] },
    // },

    /* Test against branded browsers. */
    // {
    //   name: 'Microsoft Edge',
    //   use: { ...devices['Desktop Edge'], channel: 'msedge' },
    // },
    // {
    //   name: 'Google Chrome',
    //   use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    // },
  ],

  /* Run your local dev server before starting the tests */
  webServer: {
    command: process.platform === 'win32' 
      ? 'npx ng serve --port 4200 --host 127.0.0.1 --no-open' 
      : 'npx ng serve --port 4200 --host 127.0.0.1 --no-open',
    cwd: clientAppDir, // Absolute path to Angular app directory
    url: 'http://127.0.0.1:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 360000,  // 6 minutes for Angular to compile and start (increased for slower machines)
    stdout: 'pipe',   // Show stdout to help debug startup issues
    stderr: 'pipe',   // Show stderr to help debug startup issues
  },
});
