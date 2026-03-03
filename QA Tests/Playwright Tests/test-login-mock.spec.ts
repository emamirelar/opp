/**
 * @fileoverview Login mock verification test
 * Validates that the mock login flow completes and redirects to a valid post-login page.
 * 
 * @author UNOPS Opportunity+ QA Team
 */

import { test, expect } from '@playwright/test';
import { login } from './helpers/auth.helper';
import { waitForPageReady } from './helpers/wait.helper';

test.describe('Login Mock Verification', () => {
  test('should successfully mock login flow and redirect to home', async ({ page }) => {
    await login(page);

    await expect(page).not.toHaveURL(/\/login/);
    await expect(page).toHaveURL(/\/(home|dashboard)?$/);

    await waitForPageReady(page);

    const loadingOverlay = page.locator('.bg-black.bg-opacity-50').first();
    await expect(loadingOverlay).not.toBeVisible();
  });

  test('should render main layout after login', async ({ page }) => {
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
