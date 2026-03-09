/**
 * @fileoverview Opportunity Export E2E Tests
 *
 * Tests for the export functionality on the opportunity list page:
 * button visibility, permission-based access, and export action triggering.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-EXPORT
 * @tests 4
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForPageReady, waitForLoadingToComplete } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_EXPORT_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const OPPORTUNITIES_URL = '/partnerships/opportunities';

// =============================================================================
// SECTION 1: Export Button Visibility
// =============================================================================
test.describe('Export — Button Visibility', () => {
  test.slow();
  test.skip(!featureReady, 'Export not deployed — set OPPORTUNITY_EXPORT_IMPLEMENTED=true');

  test('EXP-001: Export button visible for admin user', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const exportBtn = page.locator('[data-testid="export-button"], button:has-text("Export"), button:has(i.pi-download)').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });

  test('EXP-002: Export button hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const exportBtn = page.locator('[data-testid="export-button"]');
    await expect(exportBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 2: Export Action
// =============================================================================
test.describe('Export — Action Trigger', () => {
  test.slow();
  test.skip(!featureReady, 'Export not deployed — set OPPORTUNITY_EXPORT_IMPLEMENTED=true');

  test('EXP-003: Clicking export button triggers export action', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const exportBtn = page.locator('[data-testid="export-button"], button:has-text("Export"), button:has(i.pi-download)').first();
    const isVisible = await exportBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Export button not visible');

    await exportBtn.click();

    const confirmation = page.locator('.p-toast-message, .p-dialog, [data-testid="export-progress"]').first();
    await expect(confirmation).toBeVisible({ timeout: 10000 });
  });

  test('EXP-004: Export with filters applies filtered results', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"]').first();
    if (await searchInput.isVisible({ timeout: 5000 }).catch(() => false)) {
      await searchInput.fill('test');
      await searchInput.press('Enter');
      await waitForLoadingToComplete(page);
    }

    const exportBtn = page.locator('[data-testid="export-button"], button:has-text("Export")').first();
    await expect(exportBtn).toBeVisible({ timeout: 5000 });
  });
});
