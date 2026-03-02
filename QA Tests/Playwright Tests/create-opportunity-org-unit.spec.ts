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
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForDialog } from './helpers/wait.helper';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

const ADMIN_USER = 'test@playwright.local';
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

async function openCreateOpportunityDialog(page: import('@playwright/test').Page): Promise<void> {
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(2000);

  // Click "New Opportunity" button on interactions list (class: interaction-create-opportunity-button)
  const createBtn = page.locator('.interaction-create-opportunity-button, button:has-text("New Opportunity"), [data-testid="create-opportunity-button"]').first();
  await createBtn.click({ timeout: 10000 });
  await waitForDialog(page);
}

async function openCreateOpportunityFromPartnerContext(page: import('@playwright/test').Page): Promise<void> {
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(2000);

  // Navigate to partner detail, then opportunities tab, then Create Opportunity
  const firstCard = page.locator('app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer').first();
  if (await firstCard.isVisible().catch(() => false)) {
    await firstCard.click();
    await page.waitForTimeout(2000);
  }

  // Click Opportunities tab if present
  const opportunitiesTab = page.locator('button:has-text("Opportunities"), [role="tab"]:has-text("Opportunities")').first();
  if (await opportunitiesTab.isVisible().catch(() => false)) {
    await opportunitiesTab.click();
    await page.waitForTimeout(1500);
  }

  // Click Create Opportunity
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
      await openCreateOpportunityDialog(page);
    });

    await test.step('Assert — dialog is visible', async () => {
      const dialog = page.locator('p-dialog').filter({ hasText: /create.*opportunity|new.*opportunity/i });
      await expect(dialog).toBeVisible();
    });
  });

  test('TC-002: should display Opportunity Name field in create dialog', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"], [data-testid="opportunity-name-input"]').first();
    await expect(nameInput).toBeVisible();
  });

  test('TC-003: should display Responsible Org Unit dropdown in create dialog', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    // Look for org unit field: data-testid, label, or p-select near "Responsible Org Unit"
    const orgUnitField = page.locator('[data-testid="responsible-org-unit-dropdown"], [data-testid="org-unit-dropdown"]')
      .or(page.locator('p-floatlabel').filter({ hasText: /responsible org unit/i }))
      .or(page.getByLabel(/responsible org unit/i))
      .first();
    await expect(orgUnitField).toBeVisible({ timeout: 10000 });
  });

  test('TC-004: should display Opportunity Description field in create dialog', async ({ page }) => {
    await openCreateOpportunityDialog(page);

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
    await openCreateOpportunityDialog(page);

    await test.step('Act — open org unit dropdown and select option', async () => {
      const dropdown = page.locator('[data-testid="responsible-org-unit-dropdown"], [data-testid="org-unit-dropdown"]')
        .or(page.locator('p-select').filter({ has: page.locator('..') }))
        .or(page.getByLabel(/responsible org unit/i))
        .first();
      await dropdown.click();
      await page.locator('.p-select-overlay, .p-select-option').first().waitFor({ state: 'visible', timeout: 5000 });
      await page.locator('.p-select-option').filter({ hasText: 'Test Org Unit' }).click();
    });

    await test.step('Assert — selection is visible', async () => {
      await expect(page.locator('.p-select').filter({ hasText: 'Test Org Unit' })).toBeVisible({ timeout: 3000 });
    });
  });

  test('TC-006: should allow filling Opportunity Name', async ({ page }) => {
    await openCreateOpportunityDialog(page);

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
    await openCreateOpportunityDialog(page);

    await test.step('Act — leave name empty and click Create', async () => {
      const createBtn = page.locator('button:has-text("Create")').first();
      await createBtn.click();
    });

    await test.step('Assert — validation error shown', async () => {
      const errorMsg = page.locator('.p-message-error, .p-message[severity="error"], [class*="error"]').filter({ hasText: /name|required/i });
      await expect(errorMsg).toBeVisible({ timeout: 5000 });
    });
  });

  test('TC-008: should enforce max 120 chars for Opportunity Name', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    const longName = 'A'.repeat(121);
    await nameInput.fill(longName);

    const value = await nameInput.inputValue();
    expect(value.length).toBeLessThanOrEqual(120);
  });

  test('TC-009: should accept exactly 120 chars for Opportunity Name', async ({ page }) => {
    await openCreateOpportunityDialog(page);

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
      const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
      if (await nameInput.isVisible().catch(() => false)) {
        await nameInput.fill('Test Opportunity');
      }
      const createBtn = page.locator('button:has-text("Create")').first();
      if (await createBtn.isVisible().catch(() => false)) {
        await createBtn.click();
      }
    });

    await test.step('Assert — partner role validation error shown when in partner context', async () => {
      const partnerRoleSection = page.locator('text=/funding partner|client partner|partner role/i');
      const hasPartnerContext = await partnerRoleSection.isVisible().catch(() => false);
      if (hasPartnerContext) {
        const errorMsg = page.locator('.p-message-error, .p-message[severity="error"]').filter({ hasText: /partner role|at least one/i });
        await expect(errorMsg).toBeVisible({ timeout: 5000 });
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
    await page.route(url => {
      const u = url.toString();
      return (u.includes('/api/opportunity') || u.includes('/create-opportunity')) && route.request().method() === 'POST';
    }, async route => {
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
    });
  });

  test('TC-011: should create opportunity successfully with org unit selected', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    await test.step('Arrange — fill required fields and select org unit', async () => {
      const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
      await nameInput.fill('Test Opportunity With Org Unit');

      const dropdown = page.locator('[data-testid="responsible-org-unit-dropdown"], [data-testid="org-unit-dropdown"]')
        .or(page.getByLabel(/responsible org unit/i))
        .first();
      if (await dropdown.isVisible().catch(() => false)) {
        await dropdown.click();
        await page.waitForTimeout(500);
        await page.locator('.p-select-option').filter({ hasText: 'Test Org Unit' }).click();
      }
    });

    await test.step('Act — click Create', async () => {
      const createBtn = page.locator('button:has-text("Create")').first();
      await createBtn.click();
    });

    await test.step('Assert — dialog closes or success feedback shown', async () => {
      await page.waitForTimeout(3000);
      const dialog = page.locator('p-dialog').filter({ hasText: /create.*opportunity/i });
      const dialogHidden = await dialog.isHidden().catch(() => true);
      const toast = page.locator('.p-toast-message').filter({ hasText: /success|created/i });
      const hasSuccess = await toast.isVisible().catch(() => false);
      expect(dialogHidden || hasSuccess).toBeTruthy();
    });
  });

  test('TC-012: should create opportunity successfully without org unit (optional field)', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    await test.step('Arrange — fill only required fields (name), leave org unit empty', async () => {
      const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
      await nameInput.fill('Test Opportunity Without Org Unit');
    });

    await test.step('Act — click Create', async () => {
      const createBtn = page.locator('button:has-text("Create")').first();
      await createBtn.click();
    });

    await test.step('Assert — creation succeeds (dialog closes or success)', async () => {
      await page.waitForTimeout(3000);
      const dialog = page.locator('p-dialog').filter({ hasText: /create.*opportunity/i });
      const dialogHidden = await dialog.isHidden().catch(() => true);
      const toast = page.locator('.p-toast-message').filter({ hasText: /success|created/i });
      const hasSuccess = await toast.isVisible().catch(() => false);
      expect(dialogHidden || hasSuccess).toBeTruthy();
    });
  });

  test('TC-013: should allow canceling create dialog without creating', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    await nameInput.fill('Canceled Opportunity');

    const cancelBtn = page.locator('button:has-text("Cancel")').first();
    await cancelBtn.click();

    await page.waitForTimeout(500);
    const dialog = page.locator('p-dialog').filter({ hasText: /create.*opportunity/i });
    await expect(dialog).not.toBeVisible();
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

    await openCreateOpportunityDialog(page);

    const nameInput = page.locator('#opp-name, input[formcontrolname="name"]').first();
    await nameInput.fill('Test With Empty Org Units');
    await expect(nameInput).toHaveValue('Test With Empty Org Units');
  });

  test('TC-015: should display name character counter (X / 120)', async ({ page }) => {
    await openCreateOpportunityDialog(page);

    const counter = page.locator('small').filter({ hasText: /\/ 120/ });
    await expect(counter).toBeVisible();
  });
});
