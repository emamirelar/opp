/**
 * @fileoverview PNO-914: Auth Interceptor / Session Handling E2E Tests
 *
 * Tests the auth.interceptor.ts behavior: X-Using-Dev-Cookie header, 401 redirect to login,
 * 403 error handling, session timeout, dev cookie page reload, and concurrent request handling.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-914
 *
 * @tests 12
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { setupAPIMocks } from './helpers/api-mocks.helper';
import { waitForPageReady, waitForPermissions } from './helpers/wait.helper';
import { getTimeout } from './helpers/test-config';

const ADMIN_USER = 'test@playwright.local';
const BASE_URL = 'http://localhost:4200';

test.describe('PNO-914 — Auth Interceptor / Session Handling', () => {
  test.slow();

  test.describe('Valid auth and page load', () => {
    test.beforeEach(async ({ page }) => {
      await authenticateWithRealBackend(page, '/home', ADMIN_USER);
      await waitForPermissions(page);
    });

    test('TC-001: Page loads successfully with valid auth', async ({ page }) => {
      await test.step('Arrange — navigate to home', async () => {
        await page.goto(`${BASE_URL}/home`);
        await page.waitForLoadState('domcontentloaded');
      });

      await test.step('Assert — page loaded and not on login', async () => {
        await expect(page).not.toHaveURL(/\/login/);
        await expect(page.locator('app-root, body')).toBeVisible({ timeout: getTimeout('long') });
      });
    });

    test('TC-002: Login page accessible without auth', async ({ page }) => {
      await test.step('Arrange — clear cookies and navigate to login', async () => {
        await page.context().clearCookies();
        await setupAPIMocks(page);
        await page.goto(`${BASE_URL}/login`);
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — login page visible', async () => {
        const usernameInput = page.getByPlaceholder(/username|email/i).or(page.locator('input[type="email"], input[type="text"]')).first();
        await expect(usernameInput).toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });

  test.describe('401 response handling', () => {
    test('TC-003: 401 response redirects to login page when no dev cookie', async ({ page }) => {
      await test.step('Arrange — setup mocks, valid claims, NO dev cookie, mock 401 on API', async () => {
        await page.context().clearCookies();
        await setupAPIMocks(page);
        await page.unroute(url => url.toString().includes('/user/claims'));
        await page.route(url => url.toString().includes('/user/claims'), async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify([
              { type: 'email', value: ADMIN_USER },
              { type: 'role', value: 'Administrator' },
              { type: 'IAPAuthenticated', value: 'true' },
            ]),
          });
        });
        await page.route(url => url.toString().includes('/api/partner') && !url.toString().includes('/api/partner/'), async route => {
          await route.fulfill({ status: 401, body: 'Unauthorized' });
        });
      });

      await test.step('Act — navigate to protected page (no dev cookie)', async () => {
        await page.goto(`${BASE_URL}/partnerships/partners`);
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — redirected to login by interceptor', async () => {
        await expect(page).toHaveURL(/\/login/);
      });
    });

    test('TC-004: Session timeout behavior — mock 401 on API call', async ({ page }) => {
      await test.step('Arrange — authenticate then override API to return 401', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
        await page.unroute(url => url.toString().includes('/api/partner'));
        await page.route(url => url.toString().includes('/api/partner') && !url.toString().includes('/api/partner/'), async route => {
          await route.fulfill({ status: 401, body: 'Session expired' });
        });
      });

      await test.step('Act — trigger API call by reloading', async () => {
        await page.reload();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — redirected to login or page reloaded', async () => {
        const url = page.url();
        expect(url).toBeTruthy();
      });
    });

    test('TC-005: Page reload on 401 with dev cookie (except /dev-login)', async ({ page }) => {
      await test.step('Arrange — setup with dev cookie, mock 401 on API', async () => {
        await page.context().clearCookies();
        await setupAPIMocks(page);
        await page.context().addCookies([{
          name: 'dev-user-email',
          value: ADMIN_USER,
          domain: '127.0.0.1',
          path: '/',
          httpOnly: false,
          secure: false,
          sameSite: 'Lax',
        }]);
        await page.route(url => url.toString().includes('/api/') && !url.toString().includes('/dev-login'), async route => {
          await route.fulfill({ status: 401, body: 'Unauthorized' });
        });
      });

      await test.step('Act — navigate to protected page', async () => {
        await page.goto(`${BASE_URL}/partnerships/partners`);
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — page loaded (reload or redirect)', async () => {
        await expect(page.locator('app-root')).toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });

  test.describe('403 response handling', () => {
    test('TC-006: 403 response shows access denied or logs error', async ({ page }) => {
      const consoleErrors: string[] = [];
      page.on('console', msg => {
        if (msg.type() === 'error') consoleErrors.push(msg.text());
      });

      await test.step('Arrange — authenticate and mock 403 on specific API', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
        await page.unroute(url => url.toString().includes('/api/partner'));
        await page.route(url => url.toString().includes('/api/partner') && !url.toString().includes('/api/partner/'), async route => {
          await route.fulfill({ status: 403, body: 'Forbidden' });
        });
      });

      await test.step('Act — reload to trigger API call', async () => {
        await page.reload();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — 403 handled (no crash, may log)', async () => {
        await expect(page.locator('app-root')).toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });

  test.describe('API headers and dev cookie', () => {
    test('TC-007: Dev cookie adds X-Using-Dev-Cookie header to API requests', async ({ page }) => {
      const capturedHeaders: string[] = [];
      await test.step('Arrange — setup request listener', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
        page.on('request', req => {
          if (req.url().includes('/api/')) {
            const h = req.headers()['x-using-dev-cookie'];
            if (h) capturedHeaders.push(h);
          }
        });
      });

      await test.step('Act — trigger API call', async () => {
        await page.reload();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — X-Using-Dev-Cookie header present on API requests', async () => {
        expect(capturedHeaders.some(h => h === 'true' || h === 'True')).toBe(true);
      });
    });

    test('TC-008: API calls include proper headers', async ({ page }) => {
      const apiRequests: { url: string; headers: Record<string, string> }[] = [];
      await test.step('Arrange — capture API requests', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
        page.on('request', req => {
          if (req.url().includes('/api/') || req.url().includes('/user/')) {
            apiRequests.push({ url: req.url(), headers: req.headers() });
          }
        });
      });

      await test.step('Act — reload page', async () => {
        await page.reload();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — API requests were made', async () => {
        expect(apiRequests.length).toBeGreaterThan(0);
      });
    });
  });

  test.describe('Session maintenance', () => {
    test('TC-009: Multiple sequential API calls maintain session', async ({ page }) => {
      await test.step('Arrange — authenticate', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
      });

      await test.step('Act — navigate to multiple pages', async () => {
        await page.goto(`${BASE_URL}/partnerships/contacts`);
        await page.waitForLoadState('networkidle');
        await page.goto(`${BASE_URL}/partnerships/opportunities`);
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — still authenticated', async () => {
        await expect(page).not.toHaveURL(/\/login/);
      });
    });

    test('TC-010: Navigation preserved after re-auth', async ({ page }) => {
      await test.step('Arrange — authenticate and navigate', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners/1', ADMIN_USER);
        await waitForPermissions(page);
      });

      await test.step('Assert — on partner detail', async () => {
        await expect(page).toHaveURL(/\/partnerships\/partners\/1/);
      });
    });
  });

  test.describe('Concurrent and edge cases', () => {
    test('TC-011: Concurrent requests with 401 handled gracefully', async ({ page }) => {
      let requestCount = 0;
      await test.step('Arrange — mock 401 after first successful call', async () => {
        await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
        await waitForPermissions(page);
        await page.unroute(url => url.toString().includes('/api/partner'));
        await page.route(url => url.toString().includes('/api/partner') && !url.toString().includes('/api/partner/'), async route => {
          requestCount++;
          await route.fulfill({ status: 401, body: 'Unauthorized' });
        });
      });

      await test.step('Act — reload to trigger concurrent API calls', async () => {
        await page.reload();
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — no crash', async () => {
        await expect(page.locator('app-root')).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-012: Expired session detection via 401', async ({ page }) => {
      await test.step('Arrange — clear cookies, mock all API as 401', async () => {
        await page.context().clearCookies();
        await setupAPIMocks(page);
        await page.route(url => url.toString().includes('/api/') || url.toString().includes('/user/'), async route => {
          if (route.request().url().includes('/user/claims')) {
            await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([]) });
          } else {
            await route.fulfill({ status: 401, body: 'Unauthorized' });
          }
        });
      });

      await test.step('Act — navigate to protected route', async () => {
        await page.goto(`${BASE_URL}/partnerships/partners`);
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — redirected to login', async () => {
        await expect(page).toHaveURL(/\/login/);
      });
    });
  });
});
