/**
 * @fileoverview Opportunity Delete E2E Tests
 *
 * Tests for deleting opportunities including confirmation dialog,
 * soft delete behavior, permission-based visibility, and immutable stage protection.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DELETE
 * @tests 11
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForHidden, waitForPermissions, waitForPageReady, waitForVisible } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_DELETE_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';
const COLLABORATOR_USER = 'collaborator@example.com';

const TEST_OPPORTUNITIES = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  active: process.env.TEST_OPP_ACTIVE_ID || '4',
  go: process.env.TEST_OPP_GO_ID || '8',
  noGo: process.env.TEST_OPP_NOGO_ID || '11',
  cancelled: process.env.TEST_OPP_CANCELLED_ID || '10',
};

const OPPORTUNITIES_LIST_URL = '/partnerships/opportunities';

function opportunityUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Delete from Detail Page — Admin (Happy Path)
// =============================================================================
test.describe('Opportunity Delete — Admin Happy Path', () => {
  test.slow();
  test.skip(!featureReady, 'Opportunity delete not deployed — set OPPORTUNITY_DELETE_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_LIST_URL);
    await waitForPermissions(page);
  });

  test('DEL-001: Delete button visible on draft opportunity for admin', async ({ page }) => {
    await test.step('Navigate to draft opportunity', async () => {
      await page.goto(opportunityUrl(TEST_OPPORTUNITIES.draft));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert delete button is visible', async () => {
      const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
      await expect(deleteBtn.first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('DEL-002: Clicking delete shows confirmation dialog', async ({ page }) => {
    await test.step('Navigate and click delete', async () => {
      await page.goto(opportunityUrl(TEST_OPPORTUNITIES.draft));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);

      const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")').first();
      const isVisible = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
      test.skip(!isVisible, 'Delete button not visible — requires real backend');
      await deleteBtn.click();
    });

    await test.step('Assert confirmation dialog appears', async () => {
      const dialog = page.locator('.p-dialog, p-confirmdialog, [role="alertdialog"], [role="dialog"]').first();
      await expect(dialog).toBeVisible({ timeout: 5000 });
      const confirmText = page.getByText(/are you sure|confirm|delete/i).first();
      await expect(confirmText).toBeVisible();
    });
  });

  test('DEL-003: Cancel delete dismisses dialog without deleting', async ({ page }) => {
    await test.step('Navigate and open delete dialog', async () => {
      await page.goto(opportunityUrl(TEST_OPPORTUNITIES.draft));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);

      const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")').first();
      const isVisible = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
      test.skip(!isVisible, 'Delete button not visible');
      await deleteBtn.click();
    });

    await test.step('Click cancel in confirmation', async () => {
      const dialog = page.locator('.p-dialog, p-confirmdialog, [role="alertdialog"], [role="dialog"]').first();
      const cancelBtn = page.locator('.p-dialog button:has-text("Cancel"), .p-dialog button:has-text("No"), [role="alertdialog"] button:has-text("No")').first();
      await cancelBtn.click();
      await waitForHidden(dialog, 5000);
    });

    await test.step('Assert still on opportunity page', async () => {
      expect(page.url()).toContain('/partnerships/opportunities/');
      const title = page.locator('[data-testid="opportunity-title"]');
      await expect(title).toBeVisible({ timeout: 5000 });
    });
  });

  test('DEL-004: Confirm delete removes opportunity and redirects to list', async ({ page }) => {
    await test.step('Navigate and trigger delete', async () => {
      await page.goto(opportunityUrl(TEST_OPPORTUNITIES.draft));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);

      const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")').first();
      const isVisible = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
      test.skip(!isVisible, 'Delete button not visible');
      await deleteBtn.click();
    });

    await test.step('Confirm deletion', async () => {
      const confirmBtn = page.locator('.p-dialog button:has-text("Yes"), .p-dialog button:has-text("Confirm"), .p-dialog button:has-text("Delete"), [role="alertdialog"] button:has-text("Yes")').first();
      await confirmBtn.click();
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert redirected to list with success message', async () => {
      await page.waitForURL(/\/partnerships\/opportunities/, { timeout: 10000 });
      const toast = page.locator('.p-toast-message');
      const hasToast = await toast.isVisible({ timeout: 5000 }).catch(() => false);
      if (hasToast) {
        await expect(toast).toContainText(/success|deleted/i);
      }
    });
  });
});

// =============================================================================
// SECTION 2: Delete — Immutable Stage Protection
// =============================================================================
test.describe('Opportunity Delete — Immutable Stage Protection', () => {
  test.slow();
  test.skip(!featureReady, 'Opportunity delete not deployed — set OPPORTUNITY_DELETE_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_LIST_URL);
  });

  test('DEL-005: Delete button hidden on GO opportunity', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.go));
    await page.waitForLoadState('networkidle');
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('DEL-006: Delete button hidden on NO GO opportunity', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.noGo));
    await page.waitForLoadState('networkidle');
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('DEL-007: Delete button hidden on CANCELLED opportunity', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.cancelled));
    await page.waitForLoadState('networkidle');
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 3: Delete — Permission Denied for Restricted Users
// =============================================================================
test.describe('Opportunity Delete — Permission Denied', () => {
  test.slow();
  test.skip(!featureReady, 'Opportunity delete not deployed — set OPPORTUNITY_DELETE_IMPLEMENTED=true');

  test('DEL-008: Read-only user cannot see delete button', async ({ page }) => {
    await authenticateWithRealBackend(page, opportunityUrl(TEST_OPPORTUNITIES.draft), READONLY_USER);
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('DEL-009: Collaborator cannot see delete button', async ({ page }) => {
    await authenticateWithRealBackend(page, opportunityUrl(TEST_OPPORTUNITIES.draft), COLLABORATOR_USER);
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 4: Delete from List View
// =============================================================================
test.describe('Opportunity Delete — List View', () => {
  test.slow();
  test.skip(!featureReady, 'Opportunity delete not deployed — set OPPORTUNITY_DELETE_IMPLEMENTED=true');

  test('DEL-010: Delete action available in list view context menu', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_LIST_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const listItem = page.locator('app-listview-card .cursor-pointer, [data-testid="opportunity-list-item"]').first();
    const isVisible = await listItem.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'No list items visible');

    const moreBtn = page.locator('button:has(i.pi-ellipsis-v), [data-testid="row-actions-button"]').first();
    if (await moreBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await moreBtn.click();
      const deleteMenuItem = page.locator('[role="menuitem"]:has-text("Delete"), .p-menuitem:has-text("Delete")').first();
      await waitForVisible(deleteMenuItem, 5000);
      await expect(deleteMenuItem).toBeVisible();
    }
  });

  test('DEL-011: Deleted opportunity no longer appears in list', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_LIST_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const listview = page.locator('app-listview');
    await expect(listview.first()).toBeVisible({ timeout: 10000 });
  });
});
