/**
 * @fileoverview Real API Authentication Helper (No Mocks)
 * 
 * Authenticates against the actual running backend at API_BASE_URL.
 * Unlike authenticateWithRealBackend() in auth.helper.ts, this does NOT
 * set up API mocks — all requests go to the real .NET backend + PostgreSQL.
 * 
 * Prerequisites:
 *   1. Cloud SQL proxy running (port 5432)
 *   2. .NET backend running (API_BASE_URL, default http://localhost:5159)
 *   3. Angular frontend running (BASE_URL, default http://localhost:4200)
 *   4. Test user exists in database (run setup-test-user.sql)
 * 
 * @author UNOPS Opportunity+ QA Team
 */

import { Page, expect } from '@playwright/test';

const DEBUG = process.env.PLAYWRIGHT_DEBUG_REAL_API === 'true';

function log(message: string): void {
  if (DEBUG) {
    console.log(`[RealAPI] ${message}`);
  }
}

/**
 * Authenticate with the real backend using development IAP cookies.
 * No API mocks are set up — all requests hit the real backend.
 * 
 * @param page - Playwright page object
 * @param targetUrl - URL to navigate to after authentication (e.g., '/opportunities')
 * @param testUserEmail - Email of the test user (default: leonardc@unops.org from .env)
 */
export async function authenticateRealApi(
  page: Page,
  targetUrl: string,
  testUserEmail?: string
): Promise<void> {
  const email = testUserEmail || process.env.TEST_USER_EMAIL || 'leonardc@unops.org';

  await page.context().clearCookies();

  // Set IAP simulation cookies (the backend's IAPVerificationMiddleware reads these)
  await page.context().addCookies([
    {
      name: 'dev-user-email',
      value: email,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: false,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'DevIAPAuth',
      value: email,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
  ]);

  // Prevent Driver.js welcome tour from blocking clicks
  await page.addInitScript(() => {
    const state = {
      hasSeenWelcome: true,
      hasCompletedHomepageTour: true,
      completedTours: ['homepage-tour'],
      firstVisitDate: new Date().toISOString(),
    };
    try {
      localStorage.setItem('unops-welcome-tour-state', JSON.stringify(state));
    } catch (_) {}
  });

  const baseURL = process.env.BASE_URL || 'http://localhost:4200';
  let cleanUrl = targetUrl;
  if (cleanUrl.startsWith('/#/')) cleanUrl = cleanUrl.substring(2);
  else if (cleanUrl.startsWith('#/')) cleanUrl = `/${cleanUrl.substring(2)}`;

  const fullUrl = cleanUrl.startsWith('http') ? cleanUrl : `${baseURL}${cleanUrl}`;
  log(`Navigating to ${fullUrl} (no mocks, real backend)`);

  await page.goto(fullUrl, { waitUntil: 'load', timeout: 30000 });

  // Wait for Angular to initialize
  await page.waitForTimeout(2000);

  // Dismiss overlays (Driver.js tour, toast messages)
  for (let attempt = 1; attempt <= 3; attempt++) {
    await page.waitForTimeout(500);
    const driverOverlay = page.locator('.driver-overlay');
    if (await driverOverlay.isVisible({ timeout: 300 }).catch(() => false)) {
      await page.locator('.driver-popover-close-btn').click({ timeout: 2000 }).catch(() => {});
      await page.waitForTimeout(800);
    } else {
      break;
    }
  }

  log('Authentication complete — all requests go to real backend');
}

/**
 * Create an entity via the real API and return its ID.
 * Useful for setting up test data directly through the API.
 */
export async function createViaApi(
  page: Page,
  endpoint: string,
  payload: Record<string, unknown>
): Promise<number> {
  const apiBase = process.env.API_BASE_URL || 'http://localhost:5159';
  const response = await page.request.post(`${apiBase}${endpoint}`, {
    data: payload,
    headers: {
      'Content-Type': 'application/json',
      'X-Goog-Authenticated-User-Email': `accounts.google.com:${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
      'X-Goog-Authenticated-User-ID': 'accounts.google.com:1',
      'Cookie': `DevIAPAuth=${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
    },
  });

  expect(response.ok(), `POST ${endpoint} should succeed: ${response.status()}`).toBeTruthy();
  const body = await response.json();
  return body.id ?? body.Id ?? body;
}

/**
 * Delete an entity via the real API (cleanup after tests).
 */
export async function deleteViaApi(
  page: Page,
  endpoint: string,
  id: number
): Promise<void> {
  const apiBase = process.env.API_BASE_URL || 'http://localhost:5159';
  await page.request.delete(`${apiBase}${endpoint}/${id}`, {
    headers: {
      'X-Goog-Authenticated-User-Email': `accounts.google.com:${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
      'X-Goog-Authenticated-User-ID': 'accounts.google.com:1',
      'Cookie': `DevIAPAuth=${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
    },
  });
}

/**
 * Check if the real backend is reachable before running tests.
 * Returns true if the backend responds, false otherwise.
 */
export async function isBackendAvailable(page: Page): Promise<boolean> {
  const apiBase = process.env.API_BASE_URL || 'http://localhost:5159';
  try {
    const response = await page.request.get(`${apiBase}/api/values/config`, {
      timeout: 5000,
      headers: {
        'X-Goog-Authenticated-User-Email': `accounts.google.com:${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
        'Cookie': `DevIAPAuth=${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
      },
    });
    return response.ok();
  } catch {
    return false;
  }
}
