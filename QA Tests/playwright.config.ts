/**
 * @fileoverview Playwright configuration for UNOPS Opportunity+ E2E test suite.
 *
 * Tiered execution strategy:
 *   - Smoke  (PR):      6 core specs, Chromium only, 3 workers     (~2 min)
 *   - Extended (PR→main): 20-25 critical specs, Chromium, 4 workers (~8 min)
 *   - Full   (nightly):  All specs, Chromium, 4 workers, sharded   (~22 min)
 *   - Cross-browser (weekly): All specs, 3 browsers, sharded       (~30 min)
 *
 * Run commands:
 *   cd "QA Tests"
 *   npx playwright test                             # Run all 3 browsers (~5,700 tests)
 *   npx playwright test --project=chromium          # Chromium only (~1,900 tests)
 *   npx playwright test --shard=1/4                 # Run 1 of 4 shards (for CI)
 *   npx playwright test contacts.spec.ts            # Run specific spec file
 *   npx playwright test --project=chromium --headed # Run with visible browser
 */

import { defineConfig, devices } from '@playwright/test';
import * as dotenv from 'dotenv';
import * as path from 'path';

// Load environment variables from the Playwright Tests folder
dotenv.config({ path: path.resolve(__dirname, 'Playwright Tests/.env') });

const BASE_URL = process.env.BASE_URL || 'http://localhost:4200';
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5159';
const IS_CI = !!process.env.CI;

// Resolve paths relative to this config file (QA Tests/)
const REPO_ROOT = path.resolve(__dirname, '..');
const SERVER_PROJECT = path.join(REPO_ROOT, 'UNOPS.PAO.Server');
const CLIENT_APP = path.join(REPO_ROOT, 'UNOPS.PAO.ClientApp');

export default defineConfig({
  // Root directory for spec files
  testDir: './Playwright Tests',

  // Match spec files
  testMatch: '**/*.spec.ts',

  // Workers: 3 in CI (GitHub runners have 2+ cores), 4 locally.
  // Override with PLAYWRIGHT_WORKERS env var.
  workers: IS_CI ? 3 : parseInt(process.env.PLAYWRIGHT_WORKERS || '4', 10),

  // Allow test files from different projects to run in parallel across workers.
  fullyParallel: true,

  // Retry flaky tests once in CI, not locally (noise reduction)
  retries: IS_CI ? 1 : 0,

  // =========================================================================
  // Group abandonment fix: raise maxFailures so early failures in one browser
  // do not abort tests still queued for other browsers / spec files.
  // Previous value of 30 caused ~275 chromium tests to "Did Not Run" because
  // 30 failures were hit across 4 workers before the suite could complete.
  // =========================================================================
  maxFailures: IS_CI ? 100 : 0,

  // Global timeout per test (30 seconds — generous for Angular app load times)
  timeout: 30_000,

  // Timeout for each assertion
  expect: {
    timeout: 5_000,
  },

  // Test reporter: line reporter for CI (compact), html for local (detailed)
  reporter: IS_CI
    ? [['line'], ['html', { outputFolder: 'TestResults/playwright-html-report', open: 'never' }]]
    : [['list'], ['html', { outputFolder: 'TestResults/playwright-html-report', open: 'on-failure' }]],

  // Shared settings for all projects
  use: {
    // Base URL — read from .env or default to localhost:4200
    baseURL: BASE_URL,

    // Run headless in CI; respect HEADLESS env var locally (default false → headed)
    headless: IS_CI ? true : (process.env.HEADLESS !== 'false'),

    // Slow motion for local debugging (0 = off)
    launchOptions: {
      slowMo: parseInt(process.env.SLOW_MO || '0', 10),
    },

    // Navigation timeout (Angular apps can be slow to hydrate)
    navigationTimeout: 20_000,

    // Action timeout (click, fill, etc.)
    actionTimeout: 10_000,

    // Capture artifacts on failure
    screenshot: 'only-on-failure',
    video: 'off',
    trace: 'off',
  },

  // Test projects — all three browser engines + real-API project.
  // Run a single browser with: npx playwright test --project=chromium
  // Run real-API tests:        npx playwright test --project=real-api
  projects: [
    {
      name: 'chromium',
      testMatch: /^(?!.*\.real\.spec\.ts$).*\.spec\.ts$/,
      use: {
        ...devices['Desktop Chrome'],
        launchOptions: {
          args: IS_CI
            ? ['--disable-gpu', '--disable-dev-shm-usage', '--no-sandbox', '--disable-setuid-sandbox']
            : [],
        },
      },
    },
    {
      name: 'firefox',
      testMatch: /^(?!.*\.real\.spec\.ts$).*\.spec\.ts$/,
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      testMatch: /^(?!.*\.real\.spec\.ts$).*\.spec\.ts$/,
      use: {
        ...devices['Desktop Safari'],
        actionTimeout: 15_000,
        navigationTimeout: 30_000,
      },
    },
    // =========================================================================
    // Real-API project: tests hit the actual .NET backend + PostgreSQL.
    // No API mocking — all requests go through Angular → .NET → PostgreSQL.
    //
    // Prerequisites:
    //   1. Cloud SQL proxy running (port 5432)
    //   2. Backend: dotnet run --project UNOPS.PAO.Server
    //   3. Frontend: ng serve
    //   4. Test user seeded (setup-test-user.sql)
    //
    // Run: npx playwright test --project=real-api
    // =========================================================================
    {
      name: 'real-api',
      testMatch: '**/*.real.spec.ts',
      timeout: 60_000,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: BASE_URL,
        navigationTimeout: 30_000,
        actionTimeout: 15_000,
      },
    },
  ],

  // Output directory for test artifacts (screenshots, videos, traces)
  outputDir: 'TestResults/playwright-artifacts',

  // Global setup: refresh gcloud IAM token for Cloud SQL proxy auth
  globalSetup: path.resolve(__dirname, 'Playwright Tests/global-setup.ts'),

  // ==========================================================================
  // Auto-start backend and frontend servers before tests.
  //
  // reuseExistingServer: true — always reuses already-running servers.
  //   If servers aren't running, Playwright will attempt to start them.
  //   The backend requires Google Cloud Secret Manager access (Development
  //   mode); if that fails, start servers manually before running tests:
  //
  //     # Terminal 1 — Backend (from repo root, or via Visual Studio)
  //     dotnet run --project UNOPS.PAO.Server
  //
  //     # Terminal 2 — Frontend
  //     cd UNOPS.PAO.ClientApp && ng serve --port 4200
  //
  //   Alternatively, run the helper script:
  //     QA Tests\start-servers.ps1
  //
  // To skip auto-start entirely:  set env SKIP_WEB_SERVER=1
  // ==========================================================================
  ...(process.env.SKIP_WEB_SERVER
    ? {}
    : {
        webServer: [
          {
            command: `dotnet run --no-launch-profile --project "${path.join(REPO_ROOT, 'QA Tests', 'TestApiServer')}"`,
            url: API_BASE_URL,
            reuseExistingServer: true,
            timeout: 300_000,
            stdout: 'pipe',
            stderr: 'pipe',
          },
          {
            command: `npx ng serve --port 4200`,
            url: BASE_URL,
            cwd: CLIENT_APP,
            reuseExistingServer: true,
            timeout: 300_000,
            stdout: 'pipe',
            stderr: 'pipe',
            env: {
              ...process.env,
              ASPNETCORE_URLS: API_BASE_URL,
            },
          },
        ],
      }),
});
