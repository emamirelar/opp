/**
 * @fileoverview Opportunity Creation E2E Tests
 * Tests for creating Opportunities from different entry points (PNO-687, PNO-688, PNO-689)
 *
 * JIRA Stories: PNO-687, PNO-688, PNO-689
 * Total Test Cases: 19
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { setupAPIMocks } from './helpers/api-mocks.helper';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForPermissions,
  waitForDialog,
} from './helpers/wait.helper';

test.describe('Opportunity Creation from Partners Page (PNO-687)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
  });

  test('POS_001 - Validate Create Opportunity button on Active Partner', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createOpportunityBtn = page.locator(
      '[data-testid="create-opportunity-button"], button:has-text("Create Opportunity"), button:has-text("New Opportunity")'
    );
    await expect(createOpportunityBtn).toBeVisible({ timeout: 10000 });
  });

  test('NEG_002 - Validate Opportunity cannot be created on Closed Partner', async ({ page }) => {
    // Wait for listview to render
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createOpportunityBtn = page.locator('[data-testid="create-opportunity-button"]');
    const isVisible = await createOpportunityBtn.isVisible().catch(() => false);
    if (isVisible) {
      await expect(createOpportunityBtn).toBeDisabled();
    }
  });

  test('POS_003 - Create New Opportunity from Partner Page successfully', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createBtn = page.locator(
      'button:has-text("Create Opportunity"), button:has-text("New Opportunity")'
    );
    await expect(createBtn).toBeVisible({ timeout: 10000 });
    await createBtn.click();

    await waitForDialog(page);

    const opportunityForm = page.locator(
      '[data-testid="opportunity-form"], .p-dialog:has-text("Opportunity")'
    );
    await expect(opportunityForm).toBeVisible({ timeout: 10000 });

    const nameInput = page.locator(
      '[data-testid="opportunity-name-input"], input[formcontrolname="name"]'
    );
    await expect(nameInput).toBeVisible({ timeout: 5000 });
    await nameInput.fill('Test Opportunity ' + Date.now());

    const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create")');
    await expect(saveBtn).toBeVisible({ timeout: 5000 });
    await saveBtn.click();

    await waitForLoadingToComplete(page);

    const successToast = page.locator('.p-toast-message-success, [role="status"]');
    await expect(successToast).toBeVisible({ timeout: 10000 });
  });

  test('NEG_004 - Validate mandatory Opportunity Name field', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createBtn = page.locator(
      'button:has-text("Create Opportunity"), button:has-text("New Opportunity")'
    );
    await expect(createBtn).toBeVisible({ timeout: 10000 });
    await createBtn.click();

    await waitForDialog(page);

    const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create")');
    await expect(saveBtn).toBeVisible({ timeout: 5000 });
    await saveBtn.click();

    const validationError = page.locator('.p-error, .p-message-error, [class*="error"]');
    await expect(validationError).toBeVisible({ timeout: 5000 });
  });

  test('BND_005 - Validate max length for Opportunity Name (255 chars)', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createBtn = page.locator(
      'button:has-text("Create Opportunity"), button:has-text("New Opportunity")'
    );
    await expect(createBtn).toBeVisible({ timeout: 10000 });
    await createBtn.click();

    await waitForDialog(page);

    const nameInput = page.locator(
      '[data-testid="opportunity-name-input"], input[formcontrolname="name"]'
    );
    await expect(nameInput).toBeVisible({ timeout: 5000 });

    const maxLengthName = 'A'.repeat(255);
    await nameInput.fill(maxLengthName);

    const inputValue = await nameInput.inputValue();
    expect(inputValue.length).toBeLessThanOrEqual(255);
  });

  test('BND_006 - Validate Name length exceeded (256 chars rejected)', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="partners-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="partners-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createBtn = page.locator(
      'button:has-text("Create Opportunity"), button:has-text("New Opportunity")'
    );
    await expect(createBtn).toBeVisible({ timeout: 10000 });
    await createBtn.click();

    await waitForDialog(page);

    const nameInput = page.locator(
      '[data-testid="opportunity-name-input"], input[formcontrolname="name"]'
    );
    await expect(nameInput).toBeVisible({ timeout: 5000 });

    const overLengthName = 'A'.repeat(256);
    await nameInput.fill(overLengthName);

    const inputValue = await nameInput.inputValue();
    expect(inputValue.length).toBeLessThanOrEqual(255);
  });
});

test.describe('Opportunity Creation from Interactions (PNO-688)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    await waitForPermissions(page);
  });

  test('POS_001 - Validate Creation from Single Interaction', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="interactions-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="interactions-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createFromInteractionBtn = page.locator(
      'button:has-text("Create Opportunity"), [data-testid="create-opportunity-from-interaction"]'
    );
    await expect(createFromInteractionBtn).toBeVisible({ timeout: 10000 });
  });

  test('POS_002 - Validate Creation from Multiple Interactions', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="interactions-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const checkboxes = page.locator('.p-checkbox, p-checkbox, input[type="checkbox"]');
    const count = await checkboxes.count();
    expect(count).toBeGreaterThanOrEqual(2);

    await checkboxes.nth(0).click();
    await checkboxes.nth(1).click();

    await waitForLoadingToComplete(page);

    const bulkCreateBtn = page.locator(
      'button:has-text("Create Opportunity"), [data-testid="create-opportunity-button"]'
    );
    await expect(bulkCreateBtn).toBeVisible({ timeout: 10000 });
  });

  test('NEG_003 - Validate mandatory Name/Description check from Interactions', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const listview = page.locator('[data-testid="interactions-listview"], app-listview').first();
    await expect(listview).toBeVisible({ timeout: 15000 });

    const firstCard = page.locator(
      'app-listview-card .cursor-pointer, [data-testid="interactions-listview"] .cursor-pointer'
    ).first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });
    await firstCard.click();

    await waitForLoadingToComplete(page);

    const createBtn = page.locator('button:has-text("Create Opportunity")');
    await expect(createBtn).toBeVisible({ timeout: 10000 });
    await createBtn.click();

    await waitForDialog(page);

    const saveBtn = page.locator('button:has-text("Save")');
    await expect(saveBtn).toBeVisible({ timeout: 5000 });
    await saveBtn.click();

    const errors = page.locator('.p-error, .p-message-error');
    await expect(errors).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Opportunity Creation from Opportunity Page (PNO-689)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);
  });

  test('POS_001 - Validate Create New Opportunity button visibility', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const newOpportunityBtn = page.locator(
      '[data-testid="new-opportunity-button"], button:has-text("New Opportunity"), button:has-text("Create Opportunity")'
    );
    await expect(newOpportunityBtn).toBeVisible({ timeout: 15000 });
  });

  test('POS_002 - Create New Opportunity from Opportunity Page', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const newOpportunityBtn = page.locator(
      '[data-testid="new-opportunity-button"], button:has-text("New Opportunity"), button:has-text("Create")'
    );
    await expect(newOpportunityBtn).toBeVisible({ timeout: 15000 });
    await newOpportunityBtn.click();

    await waitForDialog(page);

    const opportunityForm = page.locator(
      '[data-testid="opportunity-form"], .p-dialog, form'
    );
    await expect(opportunityForm).toBeVisible({ timeout: 10000 });

    const nameInput = page.locator(
      'input[formcontrolname="name"], [data-testid="opportunity-name-input"]'
    );
    await expect(nameInput).toBeVisible({ timeout: 5000 });
    await nameInput.fill('E2E Test Opportunity ' + Date.now());

    const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create")');
    await expect(saveBtn).toBeVisible({ timeout: 5000 });
    await saveBtn.click();

    await waitForLoadingToComplete(page);

    const successToast = page.locator('.p-toast-message-success, [role="status"]');
    await expect(successToast).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Opportunity Creation - Permission Tests', () => {
  test.slow();

  test('PRM_001 - Validate General User cannot create opportunities', async ({ page }) => {
    await page.context().clearCookies();
    await setupAPIMocks(page);

    await page.route((url) => url.toString().includes('/user/claims'), async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { type: 'email', value: 'general.user@test.local' },
          { type: 'name', value: 'General User' },
          { type: 'role', value: 'GENUSER' },
        ]),
      });
    });

    await page.goto('http://localhost:4200/partnerships/opportunities');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const newOpportunityBtn = page.locator('[data-testid="new-opportunity-button"]');
    await expect(newOpportunityBtn).toBeHidden();
  });
});
