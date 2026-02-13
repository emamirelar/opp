/**
 * @fileoverview Admin Translation Workbench E2E Tests
 * Tests for the Translation Workbench admin page.
 * 
 * Route: /admin/translations
 * Component: Currently shows app-coming-soon with featureName="Translation Workbench"
 * 
 * Since the feature is "Coming Soon", tests verify the page loads
 * and shows the appropriate placeholder.
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Translation Workbench - Access', () => {
  test('TW-001: Admin can access translation workbench page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/translations');
    await page.waitForTimeout(3000);

    expect(page.url()).toContain('translations');
    expect(page.url()).not.toContain('access-denied');
  });

  test('TW-002: Translation workbench page loads with content', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/translations');

    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('TW-003: Translation workbench shows Coming Soon or feature content', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/translations');

    // Feature shows "Coming Soon" placeholder
    const comingSoon = page.locator('app-coming-soon').first();
    const comingSoonVisible = await comingSoon.isVisible({ timeout: 10000 }).catch(() => false);

    // Or translation workbench heading
    const heading = page.getByText(/translation/i).first();
    const headingVisible = await heading.isVisible({ timeout: 5000 }).catch(() => false);

    expect(comingSoonVisible || headingVisible).toBeTruthy();
  });

  test('TW-004: Non-admin cannot access translation workbench', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/translations', 'test-readonly@playwright.local');
    await page.waitForTimeout(3000);

    const url = page.url();
    const body = await page.textContent('body');
    const isBlocked = url.includes('access-denied') ||
                      !url.includes('translations') ||
                      (body && /access denied|forbidden/i.test(body));
    expect(isBlocked).toBeTruthy();
  });
});
