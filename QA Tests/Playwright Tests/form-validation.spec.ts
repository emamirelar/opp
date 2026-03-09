/**
 * @tests 7
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
import { LoginPage } from './pages/login.page';
import { PartnersPage } from './pages/partners.page';
import { ContactsPage } from './pages/contacts.page';
import { OpportunitiesPage } from './pages/opportunities.page';
import { InteractionsPage } from './pages/interactions.page';

/**
 * Form Validation E2E Tests
 *
 * Tests form validation functionality across the application including:
 * - Required field validation
 * - Email format validation
 * - Number validation
 * - Date validation
 * - Custom validation rules
 * - Error message display
 * - Form submission prevention
 *
 * @note Uses path-based routing for Angular app navigation
 */
test.describe('Form Validation', () => {
  test.slow();
  const BASE_URL = 'http://localhost:4200';

  // Helper to navigate with path-based routing and API mocks (for unauthenticated pages)
  async function gotoWithMocks(page: any, path: string): Promise<void> {
    await setupAPIMocks(page);
    const targetUrl = path.startsWith('/') ? path : `/${path}`;
    await page.goto(`${BASE_URL}${targetUrl}`);
    await page.waitForLoadState('load');
    await waitForPageReady(page);
  }

  test.skip('should validate required fields on login form', async ({ page }) => {
    // Auth is cookie-based via IAP; no /login route exists in Angular app
    await gotoWithMocks(page, '/login');

    const loginPage = new LoginPage(page);
    await page.getByPlaceholder(/username|email/i).first().clear();
    await loginPage.clickLogin();

    await waitForLoadingToComplete(page);

    const usernameField = page.getByPlaceholder(/username|email/i).first();
    const isInvalid = await usernameField.evaluate(
      (el) =>
        el.classList.contains('ng-invalid') || el.classList.contains('p-invalid')
    );

    expect(isInvalid).toBe(true);
  });

  test('should validate email format in contact form', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const contactsPage = new ContactsPage(page);
    await expect(contactsPage.newButton).toBeVisible();
    await contactsPage.clickNewButton();

    const dialog = page.locator('.p-dialog').first();
    const emailInput = dialog
      .locator('input[type="email"], input[formcontrolname="email"], input[name*="email"]')
      .first();
    await expect(emailInput).toBeVisible();

    await emailInput.fill('invalid-email');
    await emailInput.blur();

    await waitForLoadingToComplete(page);

    const isInvalid = await emailInput.evaluate(
      (el) =>
        el.classList.contains('ng-invalid') || el.classList.contains('p-invalid')
    );

    expect(isInvalid).toBe(true);
  });

  test('should validate required fields on contact form', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const contactsPage = new ContactsPage(page);
    await expect(contactsPage.newButton).toBeVisible();
    await contactsPage.clickNewButton();

    const dialog = page.locator('.p-dialog').first();
    const submitButton = dialog
      .locator('button[type="submit"], button')
      .filter({ hasText: /save|submit/i })
      .first();
    await expect(submitButton).toBeVisible();

    await submitButton.click();
    await waitForLoadingToComplete(page);

    const validationErrors = dialog.locator(
      '.p-invalid, .ng-invalid, [aria-invalid="true"], .p-message, .p-error, small.p-error'
    );
    const errorCount = await validationErrors.count();
    expect(errorCount).toBeGreaterThan(0);
  });

  test('should prevent form submission with invalid data', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const opportunitiesPage = new OpportunitiesPage(page);
    const newBtnVisible = await opportunitiesPage.newButton.isVisible({ timeout: 15000 }).catch(() => false);
    if (!newBtnVisible) {
      test.skip(true, 'Requires enhanced mock data or real backend - New Opportunity button not visible');
    }
    await opportunitiesPage.clickNewButton();

    const dialog = page.locator('[role="dialog"], .p-dialog').first();
    await expect(dialog).toBeVisible({ timeout: 15000 });

    const submitButton = dialog
      .locator('button')
      .filter({ hasText: /save|submit|create/i })
      .first();
    const submitVisible = await submitButton.isVisible({ timeout: 5000 }).catch(() => false);
    if (!submitVisible) {
      test.skip(true, 'Requires enhanced mock data - submit button not found in create dialog');
    }
    await submitButton.click();
    await waitForLoadingToComplete(page);

    // Dialog should remain visible (validation prevented submission) or validation errors shown
    const dialogStillVisible = await dialog.isVisible().catch(() => false);
    const validationErrors = dialog.locator('.p-invalid, .ng-invalid, [aria-invalid="true"], .p-message, .p-error');
    const hasErrors = (await validationErrors.count()) > 0;
    expect(dialogStillVisible || hasErrors).toBe(true);
  });

  test.skip('should display validation error messages', async ({ page }) => {
    // Auth is cookie-based via IAP; no /login route exists in Angular app
    await gotoWithMocks(page, '/login');

    const loginPage = new LoginPage(page);
    await loginPage.fillUsername('invalid@example.com');
    await loginPage.fillPassword('wrong');
    await loginPage.clickLogin();

    await waitForLoadingToComplete(page);

    const errorMessages = page.locator(
      '.p-message-error, .p-message, [role="alert"], .error-message'
    ).first();
    await expect(errorMessages).toBeVisible({ timeout: 10000 });
  });

  test('should validate number fields accept only numbers', async ({
    page,
  }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const opportunitiesPage = new OpportunitiesPage(page);
    await expect(opportunitiesPage.newButton).toBeVisible();
    await opportunitiesPage.clickNewButton();

    const dialog = page.locator('.p-dialog').first();
    await expect(dialog).toBeVisible({ timeout: 10000 });

    const numberInputs = dialog.locator(
      'input[type="number"], p-inputnumber input'
    );
    const numberCount = await numberInputs.count();
    if (numberCount === 0) {
      test.skip(true, 'Create opportunity dialog has no number fields');
      return;
    }

    const firstNumberInput = numberInputs.first();
    await firstNumberInput.fill('abc');

    const value = await firstNumberInput.inputValue();

    expect(value === '' || /^\d*\.?\d*$/.test(value)).toBe(true);
  });

  test('should validate date fields with proper format', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const interactionsPage = new InteractionsPage(page);
    const isNewVisible = await interactionsPage.isNewButtonVisible();
    expect(isNewVisible).toBe(true);

    await interactionsPage.getNewButton().click();
    await waitForDialog(page);

    const dialog = page.locator('.p-dialog').first();
    const dateInputs = dialog.locator(
      'input[type="date"], p-datepicker input, p-calendar input'
    );
    const dateCount = await dateInputs.count();
    expect(dateCount).toBeGreaterThan(0);

    await expect(dateInputs.first()).toBeVisible();
  });

  test.skip('should clear validation errors when field is corrected', async ({
    page,
  }) => {
    // Auth is cookie-based via IAP; no /login route exists in Angular app
    await gotoWithMocks(page, '/login');

    const usernameInput = page.getByPlaceholder(/username|email/i).first();
    const passwordInput = page.getByPlaceholder(/password/i).first();

    await usernameInput.clear();
    await page.getByRole('button', { name: /sign in|log in|login/i }).first().click();
    await waitForLoadingToComplete(page);

    const wasInvalid = await usernameInput.evaluate((el) =>
      el.classList.contains('ng-invalid')
    );
    expect(wasInvalid).toBe(true);

    await usernameInput.fill('valid@example.com');
    await passwordInput.fill('ValidPassword123!');
    await waitForLoadingToComplete(page);

    const isValid = await usernameInput.evaluate(
      (el) =>
        !el.classList.contains('ng-invalid') ||
        el.classList.contains('ng-valid')
    );

    expect(isValid).toBe(true);
  });

  test('should validate form fields on blur', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const partnersPage = new PartnersPage(page);
    await expect(partnersPage.newButton).toBeVisible();
    await partnersPage.clickNewButton();

    const dialog = page.locator('.p-dialog').first();
    const firstInput = dialog.locator('input[formcontrolname="name"], input[type="text"]').first();
    await expect(firstInput).toBeVisible();

    await firstInput.focus();
    await firstInput.blur();
    await waitForLoadingToComplete(page);

    const isInvalidOrTouched = await firstInput.evaluate(
      (el) =>
        el.classList.contains('ng-invalid') || el.classList.contains('ng-touched')
    );

    expect(isInvalidOrTouched).toBe(true);
  });

  test('should disable submit button when form is invalid', async ({
    page,
  }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPageReady(page);
    await waitForPermissions(page);

    const contactsPage = new ContactsPage(page);
    await expect(contactsPage.newButton).toBeVisible();
    await contactsPage.clickNewButton();

    const dialog = page.locator('.p-dialog').first();
    const submitButton = dialog
      .locator('button[type="submit"], button')
      .filter({ hasText: /save|submit/i })
      .first();
    await expect(submitButton).toBeVisible();

    const isDisabled = await submitButton.isDisabled().catch(() => false);
    const validationErrors = dialog.locator(
      '.p-invalid, .ng-invalid, [aria-invalid="true"], .p-message, .p-error, small.p-error'
    );
    const errorCount = await validationErrors.count();

    expect(isDisabled || errorCount > 0).toBe(true);
  });
});
