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
 *
 * @tests 9
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPageReady, waitForNavigationComplete } from './helpers/wait.helper';
import { TranslationWorkbenchPage } from './pages/admin.page';

test.describe('Translation Workbench - Access', () => {
  test.slow();
  test('TW-001: Admin can access translation workbench page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/translations');
    await waitForPageReady(page);

    expect(page.url()).toContain('translations');
    expect(page.url()).not.toContain('access-denied');
  });

  test('TW-002: Translation workbench page loads with content', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/translations');
    await waitForPageReady(page);

    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('TW-003: Translation workbench shows Coming Soon or feature content', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/translations');
    await waitForPageReady(page);

    const twPage = new TranslationWorkbenchPage(page);
    const comingSoonVisible = await twPage.comingSoon.isVisible({ timeout: 10000 }).catch(() => false);
    const headingVisible = await twPage.translationHeading.isVisible({ timeout: 5000 }).catch(() => false);

    expect(comingSoonVisible || headingVisible).toBeTruthy();
  });

  test('TW-004: Non-admin cannot access translation workbench', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/translations', 'test-readonly@playwright.local');
    await waitForPageReady(page);

    const url = page.url();
    const body = await page.textContent('body');
    const isBlocked = url.includes('access-denied') ||
                      !url.includes('translations') ||
                      (body && /access denied|forbidden/i.test(body));
    expect(isBlocked).toBeTruthy();
  });
});

test.describe('Translation Workbench - Feature Content', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/translations');
  });

  test('TW-005: Page shows feature name in content', async ({ page }) => {
    const twPage = new TranslationWorkbenchPage(page);
    await expect(twPage.translationHeading).toBeVisible({ timeout: 10000 });
  });

  test('TW-006: Page has visual indicator (icon or image)', async ({ page }) => {
    const twPage = new TranslationWorkbenchPage(page);
    const hasIcon = await twPage.visualIndicator.isVisible({ timeout: 5000 }).catch(() => false);

    expect(hasIcon).toBeTruthy();
  });

  test('TW-007: Page accessible from admin sidebar', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin');
    await waitForPageReady(page);

    // Expand admin submenu if needed (sidebar uses nested app-menu)
    const adminParent = page
      .locator('app-sidebar, .layout-sidebar')
      .getByText(/admin/i)
      .first();
    const adminVisible = await adminParent.isVisible({ timeout: 3000 }).catch(() => false);
    if (adminVisible) {
      await adminParent.click();
      await page.waitForTimeout(400); // Wait for submenu animation
    }

    const twPage = new TranslationWorkbenchPage(page);
    const linkVisible = await twPage.sidebarTranslationLink.isVisible({ timeout: 5000 }).catch(() => false);

    expect(linkVisible).toBeTruthy();
    await twPage.sidebarTranslationLink.click();
    await waitForNavigationComplete(page, /translations/);
    expect(page.url()).toContain('translations');
  });

  test('TW-008: Coming Soon displays correct feature name', async ({ page }) => {
    const twPage = new TranslationWorkbenchPage(page);
    const comingSoonVisible = await twPage.comingSoon.isVisible({ timeout: 10000 }).catch(() => false);

    expect(comingSoonVisible).toBeTruthy();
    const text = await twPage.comingSoon.textContent();
    expect(text?.toLowerCase()).toContain('translation');
  });

  test('TW-009: Page renders without console errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => {
      errors.push(error.message);
    });

    await authenticateWithRealBackend(page, '/admin/translations');
    await waitForPageReady(page);

    const criticalErrors = errors.filter(e => !e.includes('Warning') && !e.includes('deprecated'));
    expect(criticalErrors).toHaveLength(0);
  });
});
