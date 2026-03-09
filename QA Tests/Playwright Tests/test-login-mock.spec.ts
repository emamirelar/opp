/**
 * @fileoverview Login mock verification test
 * Validates that the mock login flow completes and redirects to a valid post-login page.
 * 
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 0
 */

import { test, expect } from '@playwright/test';
import { login } from './helpers/auth.helper';
import { waitForPageReady } from './helpers/wait.helper';

test.describe('Login Mock Verification', () => {
  test.skip('should successfully mock login flow and redirect to home', async ({ page }) => {
    // No login page — app uses IAP authentication
    await login(page);

    await expect(page).not.toHaveURL(/\/login/);
    const url = page.url();
    expect(url).toBeTruthy();
    expect(url).not.toContain('/login');

    await waitForPageReady(page);

    const loadingOverlay = page.locator('.bg-black.bg-opacity-50').first();
    const overlayVisible = await loadingOverlay.isVisible().catch(() => false);
    expect(overlayVisible).toBe(false);
  });

  test.skip('should render main layout after login', async ({ page }) => {
    // No login page — app uses IAP authentication
    await login(page);
    await waitForPageReady(page);

    const appRoot = page.locator('app-root');
    await expect(appRoot).toBeVisible();

    const body = page.locator('body');
    const bodyText = await body.textContent();
    expect(bodyText).toBeTruthy();
    expect(bodyText!.trim().length).toBeGreaterThan(0);
  });
});
