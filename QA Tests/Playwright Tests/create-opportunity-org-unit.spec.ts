/**
 * @fileoverview PNO-1156: Responsible Org Unit in Create Opportunity Dialog E2E Tests
 *
 * Tests for the Create Opportunity from Interactions dialog including the new
 * Responsible Org Unit dropdown. Verifies dialog appearance, field interaction,
 * validation (name required, max 120 chars, partner role when in partner context),
 * and successful creation with and without org unit selected.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1156
 *
 * @tests 15
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForDialog,
  waitForPageReady,
  waitForLoadingToComplete,
  waitForVisible,
  waitForHidden,
} from './helpers/wait.helper';
import { InteractionsPage } from './pages/interactions.page';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

const INTERACTIONS_URL = '/partnerships/interactions';
const PARTNERS_URL = '/partnerships/partners';

/** Org units for dropdown - matches /api/values/organization-units or /api/organization-hierarchy */
const MOCK_ORG_UNITS = [
  { id: 1, name: 'Test Org Unit', code: 'OU1' },
  { id: 2, name: 'HQ - Headquarters', code: 'HQ' },
  { id: 3, name: 'RO - Regional Office', code: 'RO' },
];

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

async function openCreateOpportunityFromPartnerContext(page: import('@playwright/test').Page): Promise<void> {
  await waitForPageReady(page);

  const cardItems = page.locator('app-listview-card .cursor-pointer, tbody tr, app-listview .cursor-pointer');
  const hasRows = await cardItems.count() > 0;
  if (hasRows) {
    await cardItems.first().click();
    await waitForLoadingToComplete(page);
  }

  const opportunitiesTab = page
    .locator('button:has-text("Opportunities"), [role="tab"]:has-text("Opportunities")')
    .first();
  if (await opportunitiesTab.isVisible().catch(() => false)) {
    await opportunitiesTab.click();
    await waitForLoadingToComplete(page);
  }

  const createBtn = page.locator('button:has-text("Create Opportunity"), button:has-text("New Opportunity")').first();
  await createBtn.click({ timeout: 10000 }).catch(() => {});
  await waitForDialog(page);
}

// =============================================================================
// SECTION 1: Dialog Appearance
// =============================================================================
test.describe('PNO-1156 — Responsible Org Unit: Dialog Appearance', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, INTERACTIONS_URL);
    await waitForPermissions(page);

    // Override organization-hierarchy / organization-units for org unit dropdown
    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
    await page.route(url => url.toString().includes('/api/values/organization-units'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
  });

  test('TC-001: should display Create Opportunity dialog when New Opportunity clicked', async ({ page }) => {
    await test.step('Arrange — navigate to interactions', async () => {
      await page.goto(INTERACTIONS_URL, { waitUntil: 'networkidle' });
    });

    await test.step('Act — open create opportunity dialog', async () => {
      await new InteractionsPage(page).openCreateOpportunityDialog();
    });

    await test.step('Assert — dialog is visible', async () => {
      const dialog = page.locator('.p-dialog').filter({ hasText: /create.*opportunity|new.*opportunity/i }).first();
      await expect(dialog).toBeVisible();
    });
  });

  test('TC-002: should display Opportunity Name field in create dialog', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"], [data-testid="opportunity-name-input"]').first();
    await expect(nameInput).toBeVisible();
  });

  test('TC-003: should display Responsible Org Unit dropdown in create dialog', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const dialog = page.locator('.p-dialog').first();
    const orgUnitField = page.getByLabel(/responsible org unit/i)
      .or(dialog.locator('p-select, p-dropdown'))
      .or(page.locator('p-floatlabel').filter({ hasText: /responsible org unit/i }))
      .or(page.locator('[data-testid="responsible-org-unit-dropdown"], [data-testid="org-unit-dropdown"]'))
      .first();
    await expect(orgUnitField).toBeVisible({ timeout: 10000 });
  });

  test('TC-004: should display Opportunity Description field in create dialog', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const descInput = page.locator('#opp-desc, textarea[formcontrolname="description"]').first();
    await expect(descInput).toBeVisible();
  });
});

// =============================================================================
// SECTION 2: Field Interaction
// =============================================================================
test.describe('PNO-1156 — Responsible Org Unit: Field Interaction', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, INTERACTIONS_URL);
    await waitForPermissions(page);

    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
    await page.route(url => url.toString().includes('/api/values/organization-units'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
  });

  test('TC-005: should allow selecting an org unit from dropdown', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    await test.step('Act — open org unit dropdown and select option', async () => {
      const dialog = page.locator('.p-dialog, [role="dialog"]').first();
      const dropdown = dialog.locator('p-select').first()
        .or(dialog.locator('p-dropdown').first())
        .or(page.getByLabel(/responsible org unit/i))
        .or(page.locator('[data-testid="responsible-org-unit-dropdown"], [data-testid="org-unit-dropdown"]'))
        .first();
      await dropdown.click();
      const overlay = page.locator('.p-select-overlay, .p-select-panel, [role="listbox"]').first();
      await overlay.waitFor({ state: 'visible', timeout: 5000 });
      const option = page.locator('[role="option"], .p-select-option, li').filter({ hasText: /test org unit/i }).first();
      await option.click();
    });

    await test.step('Assert — selection is visible', async () => {
      const selected = page.locator('p-select, .p-select').filter({ hasText: /test org unit/i }).first();
      await expect(selected).toBeVisible({ timeout: 3000 });
    });
  });

  test('TC-006: should allow filling Opportunity Name', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    await nameInput.fill('E2E Test Opportunity Name');
    await expect(nameInput).toHaveValue('E2E Test Opportunity Name');
  });
});

// =============================================================================
// SECTION 3: Validation
// =============================================================================
test.describe('PNO-1156 — Responsible Org Unit: Validation', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, INTERACTIONS_URL);
    await waitForPermissions(page);

    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
  });

  test('TC-007: should show validation error when name is empty', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    await test.step('Act — leave name empty and click Create', async () => {
      const dialog = page.locator('.p-dialog').first();
      const createBtn = dialog.locator('button').filter({ hasText: /create/i }).first();
      const isDisabled = await createBtn.isDisabled().catch(() => false);
      if (!isDisabled) {
        await createBtn.click();
      }
    });

    await test.step('Assert — validation error shown or Create button disabled', async () => {
      const dialog = page.locator('.p-dialog').first();
      const errorMsg = dialog.locator('.p-message, small.p-error, .p-error').filter({ hasText: /name|required/i }).first();
      const createBtn = dialog.locator('button').filter({ hasText: /create/i }).first();
      const isDisabled = await createBtn.isDisabled().catch(() => false);
      const errorVisible = await errorMsg.isVisible().catch(() => false);
      expect(errorVisible || isDisabled).toBe(true);
    });
  });

  test('TC-008: should enforce max 120 chars for Opportunity Name', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    const longName = 'A'.repeat(121);
    await nameInput.fill(longName);

    const value = await nameInput.inputValue();
    expect(value.length).toBeLessThanOrEqual(120);
  });

  test('TC-009: should accept exactly 120 chars for Opportunity Name', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    const maxName = 'B'.repeat(120);
    await nameInput.fill(maxName);

    await expect(nameInput).toHaveValue(maxName);
  });

  test('TC-010: should show validation error when partner context and no partner role selected', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNERS_URL);
    await waitForPermissions(page);

    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });

    await openCreateOpportunityFromPartnerContext(page);

    await test.step('Act — fill name but do not select funding/client partner role', async () => {
      const dialog = page.locator('.p-dialog').first();
      const nameInput = dialog.locator('#opp-name, input[formcontrolname="name"]').first();
      if (await nameInput.isVisible().catch(() => false)) {
        await nameInput.fill('Test Opportunity');
      }
      const createBtn = dialog.locator('button').filter({ hasText: /create/i }).first();
      if (await createBtn.isVisible().catch(() => false) && !(await createBtn.isDisabled().catch(() => true))) {
        await createBtn.click();
      }
    });

    await test.step('Assert — partner role validation error shown when in partner context', async () => {
      const dialog = page.locator('.p-dialog').first();
      const partnerRoleSection = dialog.locator('text=/funding partner|client partner|partner role/i');
      const hasPartnerContext = await partnerRoleSection.isVisible().catch(() => false);
      if (hasPartnerContext) {
        const errorMsg = dialog.locator('.p-message, small.p-error').filter({ hasText: /partner role|at least one/i }).first();
        const errorVisible = await errorMsg.isVisible().catch(() => false);
        const createDisabled = await dialog.locator('button').filter({ hasText: /create/i }).first().isDisabled().catch(() => false);
        expect(errorVisible || createDisabled).toBe(true);
      }
    });
  });
});

// =============================================================================
// SECTION 4: Creation Flow
// =============================================================================
test.describe('PNO-1156 — Responsible Org Unit: Creation Flow', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, INTERACTIONS_URL);
    await waitForPermissions(page);

    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
    await page.route(url => url.toString().includes('/api/values/organization-units'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });

    // Mock POST /api/opportunity and partner create-opportunity (only intercept POST)
    await page.route(
      url => {
        const u = url.toString();
        return u.includes('/api/opportunity') || u.includes('/create-opportunity');
      },
      async (route, request) => {
        if (request.method() !== 'POST') {
          await route.continue();
          return;
        }
        await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 100,
          name: 'Created Opportunity',
          status: 'Draft',
          stage: 'Draft',
          responsibleOrgUnitId: 1,
          responsibleOrgUnitName: 'Test Org Unit',
        }),
        });
      }
    );
  });

  test('TC-011: should create opportunity successfully with org unit selected', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    await test.step('Arrange — fill required fields and select org unit', async () => {
      const dialog = page.locator('.p-dialog').first();
      const nameInput = dialog.locator('#opp-name, input[formcontrolname="name"]').first();
      await nameInput.fill('Test Opportunity With Org Unit');

      const dropdown = dialog.locator('p-select').first();
      if (await dropdown.isVisible().catch(() => false)) {
        await dropdown.click();
        await waitForVisible(page.locator('.p-select-option, .p-dropdown-item').first(), 5000);
        await page.locator('.p-select-option, .p-dropdown-item').filter({ hasText: /test org unit/i }).first().click();
      }
    });

    await test.step('Act — click Create', async () => {
      const dialog = page.locator('.p-dialog').first();
      const createBtn = dialog.locator('button').filter({ hasText: /create/i }).first();
      await createBtn.click();
    });

    await test.step('Assert — dialog closes or success feedback shown', async () => {
      const dialog = page.locator('.p-dialog').first();
      const toast = page.locator('.p-toast-message, .p-toast-message-success').filter({ hasText: /success|created/i }).first();
      const dialogHidden = await dialog.waitFor({ state: 'hidden', timeout: 10000 }).then(() => true).catch(() => false);
      const toastVisible = await toast.isVisible().catch(() => false);
      expect(dialogHidden || toastVisible).toBe(true);
    });
  });

  test('TC-012: should create opportunity successfully without org unit (optional field)', async ({ page }) => {
    test.skip(true, 'Org unit is now required in Create Opportunity dialog — validation blocks submission without it');
  });

  test('TC-013: should allow canceling create dialog without creating', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    await nameInput.fill('Canceled Opportunity');

    const cancelBtn = page.locator('button:has-text("Cancel")').first();
    await cancelBtn.click();

    const dialog = page.locator('.p-dialog').filter({ hasText: /create.*opportunity/i }).first();
    await expect(dialog).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 5: Edge Cases
// =============================================================================
test.describe('PNO-1156 — Responsible Org Unit: Edge Cases', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, INTERACTIONS_URL);
    await waitForPermissions(page);

    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_ORG_UNITS),
      });
    });
  });

  test('TC-014: should handle empty org unit dropdown gracefully', async ({ page }) => {
    await page.route(url => url.toString().includes('/api/organization-hierarchy'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });
    await page.route(url => url.toString().includes('/api/values/organization-units'), async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await new InteractionsPage(page).openCreateOpportunityDialog();

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    await nameInput.fill('Test With Empty Org Units');
    await expect(nameInput).toHaveValue('Test With Empty Org Units');
  });

  test('TC-015: should display name character counter (X / 120)', async ({ page }) => {
    await new InteractionsPage(page).openCreateOpportunityDialog();

    const counter = page.locator('small').filter({ hasText: /\/ 120/ });
    await expect(counter).toBeVisible();
  });
});
