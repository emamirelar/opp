/**
 * @fileoverview PNO-862: Unsaved Changes Warning/Guard E2E Tests
 *
 * Tests for the unsaved changes warning when navigating away from opportunity edit.
 * When a user edits an opportunity section and attempts to navigate away without saving,
 * a confirmation dialog should appear. User can stay and save, or discard and leave.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-862
 *
 * @tests 7
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForPermissions,
} from './helpers/wait.helper';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Feature gate: set UNSAVED_CHANGES_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.UNSAVED_CHANGES_IMPLEMENTED === 'true';

const TEST_RECORDS = {
  active: process.env.TEST_RECORD_ACTIVE_ID || '1',
  draft: process.env.TEST_RECORD_DRAFT_ID || '2',
};

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const OPPORTUNITIES_URL = '/partnerships/opportunities';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function opportunityUrl(id: string): string {
  return `${OPPORTUNITIES_URL}/${id}`;
}

/** Get Overview section edit button (pencil icon) - within #section-overview */
function getOverviewEditButton(page: import('@playwright/test').Page) {
  return page
    .locator('#section-overview')
    .locator('button')
    .filter({ has: page.locator('.pi-pencil') })
    .first()
    .or(page.locator('#section-overview').getByRole('button', { name: /edit/i }).first());
}

/** Get in-section unsaved warning bar - shows "You have unsaved changes" */
function getUnsavedWarningBar(page: import('@playwright/test').Page) {
  return page.getByText(/you have unsaved changes|unsaved changes/i).first();
}

/** Get Save Changes button within a section */
function getSaveChangesButton(page: import('@playwright/test').Page) {
  return page.getByRole('button', { name: /save changes/i }).first();
}

/** Get Discard Changes button within a section */
function getDiscardChangesButton(page: import('@playwright/test').Page) {
  return page.getByRole('button', { name: /discard changes/i }).first();
}

/** Get section chip by label (Overview, What, Why, etc.) */
function getSectionChip(page: import('@playwright/test').Page, label: string | RegExp) {
  return page.getByRole('button', { name: label }).first();
}

// =============================================================================
// PNO-862 — Unsaved Changes Warning
// =============================================================================
test.describe('PNO-862 — Unsaved Changes Warning/Guard', () => {
  test.slow();

  test.skip(!featureReady, 'Unsaved changes feature not deployed — set UNSAVED_CHANGES_IMPLEMENTED=true to run');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  // TC-001: Happy path — Edit section, make change → in-section warning bar appears
  test('TC-001: Edit Overview section and make change → unsaved warning bar appears', async ({ page }) => {
    await test.step('Arrange — navigate to draft opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — enter edit mode and modify name field', async () => {
      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible — requires editable opportunity');
      }
      await editBtn.click();
      await page.waitForTimeout(300);

      const nameInput = page.locator('#section-overview input').first();
      await nameInput.waitFor({ state: 'visible', timeout: 5000 });
      await nameInput.fill('E2E Test - Unsaved Changes ' + Date.now());
    });

    await test.step('Assert — unsaved warning bar is visible', async () => {
      const warningBar = getUnsavedWarningBar(page);
      await expect(warningBar).toBeVisible({ timeout: 5000 });
    });
  });

  // TC-002: Positive — Save changes → warning disappears
  test('TC-002: Save changes → unsaved warning disappears', async ({ page }) => {
    await test.step('Arrange — navigate and make unsaved change', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);

      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible');
      }
      await editBtn.click();
      await page.waitForTimeout(300);

      const nameInput = page.locator('#section-overview input').first();
      await nameInput.waitFor({ state: 'visible', timeout: 5000 });
      await nameInput.fill('E2E Saved ' + Date.now());

      await expect(getUnsavedWarningBar(page)).toBeVisible({ timeout: 5000 });
    });

    await test.step('Act — click Save Changes', async () => {
      const saveBtn = getSaveChangesButton(page);
      await saveBtn.click();
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — warning bar no longer visible', async () => {
      const warningBar = getUnsavedWarningBar(page);
      await expect(warningBar).not.toBeVisible({ timeout: 10000 });
    });
  });

  // TC-003: Discard flow — Discard changes → warning disappears
  test('TC-003: Discard changes → unsaved warning disappears', async ({ page }) => {
    await test.step('Arrange — navigate and make unsaved change', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);

      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible');
      }
      await editBtn.click();
      await page.waitForTimeout(300);

      const nameInput = page.locator('#section-overview input').first();
      await nameInput.waitFor({ state: 'visible', timeout: 5000 });
      await nameInput.fill('E2E Discarded ' + Date.now());

      await expect(getUnsavedWarningBar(page)).toBeVisible({ timeout: 5000 });
    });

    await test.step('Act — click Discard Changes (in-section bar reverts without confirm)', async () => {
      const discardBtn = getDiscardChangesButton(page);
      await discardBtn.click();
      await page.waitForTimeout(500);
    });

    await test.step('Assert — warning bar no longer visible', async () => {
      const warningBar = getUnsavedWarningBar(page);
      await expect(warningBar).not.toBeVisible({ timeout: 5000 });
    });
  });

  // TC-004: Navigation between sections — Click section chip with unsaved changes → warning/confirm
  test('TC-004: Navigate to different section with unsaved changes → confirmation or warning shown', async ({
    page,
  }) => {
    await test.step('Arrange — navigate, edit Overview, make change', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);

      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible');
      }
      await editBtn.click();
      await page.waitForTimeout(300);

      const nameInput = page.locator('#section-overview input').first();
      await nameInput.waitFor({ state: 'visible', timeout: 5000 });
      await nameInput.fill('E2E Nav Test ' + Date.now());

      await expect(getUnsavedWarningBar(page)).toBeVisible({ timeout: 5000 });
    });

    await test.step('Act — click What section chip', async () => {
      const whatChip = getSectionChip(page, /what/i);
      const chipVisible = await whatChip.isVisible({ timeout: 3000 }).catch(() => false);
      if (!chipVisible) {
        test.skip(true, 'What chip not visible — may need larger viewport');
      }
      await whatChip.click();
      await page.waitForTimeout(500);
    });

    await test.step('Assert — confirmation dialog appears (per PNO-862 AC3)', async () => {
      const confirmDialog = page
        .locator('[role="dialog"], .p-dialog, .p-confirm-dialog')
        .filter({ hasText: /unsaved|discard|save|changes/i });
      await expect(confirmDialog).toBeVisible({ timeout: 3000 });
    });
  });

  // TC-005: Readonly user — No edit button, no unsaved warning possible
  test('TC-005: Readonly user — no edit button in Overview section', async ({ page }) => {
    await test.step('Arrange — authenticate as readonly user', async () => {
      await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
      await waitForPermissions(page);
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — Overview section has no edit button (or edit not visible)', async () => {
      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
      expect(editVisible).toBe(false);
    });

    await test.step('Assert — no unsaved warning bar present', async () => {
      const warningBar = getUnsavedWarningBar(page);
      await expect(warningBar).not.toBeVisible();
    });
  });

  // TC-006: Negative — No warning when no changes made
  test('TC-006: Enter edit mode without changing field → no unsaved warning', async ({ page }) => {
    await test.step('Arrange — navigate and enter edit mode only', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);

      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible');
      }
      await editBtn.click();
      await page.waitForTimeout(300);
    });

    await test.step('Assert — no unsaved warning (no changes made)', async () => {
      const warningBar = getUnsavedWarningBar(page);
      await expect(warningBar).not.toBeVisible();
    });
  });

  // TC-007: After save, navigation proceeds without warning
  test('TC-007: After saving, section chip navigation proceeds without warning', async ({ page }) => {
    await test.step('Arrange — navigate, edit, save', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);

      const editBtn = getOverviewEditButton(page);
      const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (!editVisible) {
        test.skip(true, 'Overview edit button not visible');
      }
      await editBtn.click();
      await page.waitForTimeout(300);

      const nameInput = page.locator('#section-overview input').first();
      await nameInput.waitFor({ state: 'visible', timeout: 5000 });
      await nameInput.fill('E2E Post-Save Nav ' + Date.now());

      const saveBtn = getSaveChangesButton(page);
      await saveBtn.click();
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — click What section chip', async () => {
      const whatChip = getSectionChip(page, /what/i);
      const chipVisible = await whatChip.isVisible({ timeout: 3000 }).catch(() => false);
      if (!chipVisible) {
        test.skip(true, 'What chip not visible');
      }
      await whatChip.click();
      await page.waitForTimeout(800);
    });

    await test.step('Assert — no confirmation dialog, What section visible', async () => {
      const confirmDialog = page.locator('[role="dialog"]').filter({ hasText: /unsaved|discard/i });
      await expect(confirmDialog).not.toBeVisible();

      const whatSection = page.locator('#section-what');
      await expect(whatSection).toBeVisible();
    });
  });
});
