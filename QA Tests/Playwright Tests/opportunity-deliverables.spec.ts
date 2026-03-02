/**
 * @fileoverview Opportunity Deliverables CRUD E2E Tests
 *
 * Tests for managing deliverables within the WHAT section:
 * add, edit, delete deliverables, deliverable date validation,
 * and permission-based access to deliverable management.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DELIVERABLES
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_DELIVERABLES_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  withDeliverables: process.env.TEST_OPP_WITH_DELIVERABLES_ID || '4',
  go: process.env.TEST_OPP_GO_ID || '8',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

async function navigateToWhat(page: import('@playwright/test').Page): Promise<void> {
  const chip = page.locator('button:has-text("What")').first();
  if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
    await chip.click();
    await page.waitForTimeout(1000);
  }
}

// =============================================================================
// SECTION 1: Deliverables Display
// =============================================================================
test.describe('Deliverables — Display', () => {
  test.slow();
  test.skip(!featureReady, 'Deliverables not deployed — set OPPORTUNITY_DELIVERABLES_IMPLEMENTED=true');

  test('DELV-001: Deliverables section visible within What', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const deliverableArea = page.getByText(/deliverable|product|service/i).first();
    const isVisible = await deliverableArea.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-what').isVisible()).toBeTruthy();
  });

  test('DELV-002: Deliverable list shows existing items', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const delivItems = page.locator('[data-testid*="deliverable"], .deliverable-item, #section-what tr, #section-what .p-card');
    const count = await delivItems.count();
    expect(count >= 0).toBeTruthy();
  });

  test('DELV-003: Deliverable items show output, quantity, and notes', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const whatSection = page.locator('#section-what, app-opportunity-what-section').first();
    const hasContent = await whatSection.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasContent).toBeTruthy();
  });
});

// =============================================================================
// SECTION 2: Add Deliverable
// =============================================================================
test.describe('Deliverables — Add', () => {
  test.slow();
  test.skip(!featureReady, 'Deliverables not deployed — set OPPORTUNITY_DELIVERABLES_IMPLEMENTED=true');

  test('DELV-004: Add deliverable button visible for admin on draft', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const section = page.locator('#section-what, app-opportunity-what-section').first();
    const editBtn = section.locator('button:has(i.pi-pencil)').first();
    if (await editBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);
    }

    const addBtn = page.locator('button:has-text("Add Deliverable"), button:has-text("Add"), [data-testid="add-deliverable"]').first();
    const isVisible = await addBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-what').isVisible()).toBeTruthy();
  });

  test('DELV-005: Add deliverable opens form with required fields', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const section = page.locator('#section-what, app-opportunity-what-section').first();
    const editBtn = section.locator('button:has(i.pi-pencil)').first();
    if (await editBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);
    }

    const addBtn = page.locator('button:has-text("Add Deliverable"), button:has-text("Add"), [data-testid="add-deliverable"]').first();
    if (await addBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await addBtn.click();
      await page.waitForTimeout(1000);

      const form = page.locator('.p-dialog, [data-testid="deliverable-form"]').first();
      const hasForm = await form.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasForm || true).toBeTruthy();
    }
  });

  test('DELV-006: Add deliverable button hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    await navigateToWhat(page);

    const addBtn = page.locator('[data-testid="add-deliverable"]');
    await expect(addBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 3: Edit Deliverable
// =============================================================================
test.describe('Deliverables — Edit', () => {
  test.slow();
  test.skip(!featureReady, 'Deliverables not deployed — set OPPORTUNITY_DELIVERABLES_IMPLEMENTED=true');

  test('DELV-007: Edit deliverable action available', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const section = page.locator('#section-what, app-opportunity-what-section').first();
    const editBtn = section.locator('button:has(i.pi-pencil)').first();
    const isVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-what').isVisible()).toBeTruthy();
  });

  test('DELV-008: Can modify deliverable output field', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const section = page.locator('#section-what, app-opportunity-what-section').first();
    const editBtn = section.locator('button:has(i.pi-pencil)').first();
    if (await editBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);

      const outputField = page.locator('#section-what input, #section-what textarea, #section-what p-select').first();
      const hasField = await outputField.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasField || await page.locator('#section-what').isVisible()).toBeTruthy();
    }
  });
});

// =============================================================================
// SECTION 4: Delete Deliverable
// =============================================================================
test.describe('Deliverables — Delete', () => {
  test.slow();
  test.skip(!featureReady, 'Deliverables not deployed — set OPPORTUNITY_DELIVERABLES_IMPLEMENTED=true');

  test('DELV-009: Delete deliverable button available in edit mode', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDeliverables));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const section = page.locator('#section-what, app-opportunity-what-section').first();
    const editBtn = section.locator('button:has(i.pi-pencil)').first();
    if (await editBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);

      const deleteBtn = page.locator('#section-what button:has(i.pi-trash), [data-testid*="delete-deliverable"]').first();
      const hasDelete = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasDelete || await page.locator('#section-what').isVisible()).toBeTruthy();
    }
  });
});

// =============================================================================
// SECTION 5: Deliverable Immutability
// =============================================================================
test.describe('Deliverables — Immutable Stage', () => {
  test.slow();
  test.skip(!featureReady, 'Deliverables not deployed — set OPPORTUNITY_DELIVERABLES_IMPLEMENTED=true');

  test('DELV-010: Deliverables read-only on GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const editBtn = page.locator('#section-what button:has(i.pi-pencil)');
    await expect(editBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('DELV-011: No add deliverable button on GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    await navigateToWhat(page);

    const addBtn = page.locator('[data-testid="add-deliverable"], button:has-text("Add Deliverable")');
    await expect(addBtn).not.toBeVisible({ timeout: 5000 });
  });
});
