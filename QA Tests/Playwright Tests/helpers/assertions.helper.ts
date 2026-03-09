/**
 * @fileoverview Assertions Helper
 * Provides reusable assertion functions for common test scenarios
 */

import { Page, Locator, expect } from '@playwright/test';
import { getTimeout } from './test-config';
import { waitForLoadingToComplete } from './wait.helper';

/**
 * Assert element is visible with optional timeout
 * @param locator - Element locator
 * @param timeout - Optional timeout override
 */
export async function assertVisible(
  locator: Locator,
  timeout?: number
): Promise<void> {
  await expect(locator).toBeVisible({ 
    timeout: timeout || getTimeout('default') 
  });
}

/**
 * Assert element is hidden
 * @param locator - Element locator
 */
export async function assertHidden(locator: Locator): Promise<void> {
  await expect(locator).toBeHidden();
}

/**
 * Assert element contains text
 * @param locator - Element locator
 * @param text - Expected text (string or regex)
 */
export async function assertContainsText(
  locator: Locator,
  text: string | RegExp
): Promise<void> {
  await expect(locator).toContainText(text);
}

/**
 * Assert page header is visible with correct title
 * @param page - Playwright page object
 * @param entityName - Entity name (e.g., 'partners', 'contacts')
 */
export async function assertPageHeader(
  page: Page,
  entityName: string
): Promise<void> {
  await waitForLoadingToComplete(page);
  await page.waitForTimeout(1000);

  const displayName = entityName.charAt(0).toUpperCase() + entityName.slice(1);
  const header = page.locator('p, h1, h2').filter({ hasText: new RegExp(`^${displayName}$`, 'i') }).first()
    .or(page.locator(`[data-testid="${entityName}-header"]`));

  await assertVisible(header, getTimeout('navigation'));
}

/**
 * Assert listview is displayed
 * @param page - Playwright page object
 * @param entityName - Entity name (e.g., 'partners', 'contacts')
 */
export async function assertListviewVisible(
  page: Page,
  entityName: string
): Promise<void> {
  const listview = page.locator('app-listview').first()
    .or(page.locator(`[data-testid="${entityName}-listview"]`));
  await assertVisible(listview);
}

/**
 * Assert button is visible with correct text
 * @param page - Playwright page object
 * @param testId - data-testid attribute value
 * @param expectedText - Expected button text (optional)
 */
export async function assertButtonVisible(
  page: Page,
  testId: string,
  expectedText?: string | RegExp
): Promise<void> {
  const button = page.locator(`[data-testid="${testId}"]`);
  await assertVisible(button);
  
  if (expectedText) {
    await assertContainsText(button, expectedText);
  }
}

/**
 * Assert table has data
 * @param page - Playwright page object
 * @returns Number of rows found
 */
export async function assertTableHasData(page: Page): Promise<number> {
  const table = page.locator('p-table, .p-datatable, table').first();
  await assertVisible(table);
  
  const rows = page.locator('tbody tr, .p-datatable-tbody tr');
  const count = await rows.count();
  
  expect(count).toBeGreaterThan(0);
  return count;
}

/**
 * Assert URL matches pattern
 * @param page - Playwright page object
 * @param pattern - URL pattern (string or regex)
 */
export async function assertUrlMatches(
  page: Page,
  pattern: string | RegExp
): Promise<void> {
  await expect(page).toHaveURL(pattern, { timeout: getTimeout('default') });
}

/**
 * Assert dialog is open
 * @param page - Playwright page object
 */
export async function assertDialogOpen(page: Page): Promise<void> {
  // Match PrimeNG Dialog, DynamicDialog, and native role="dialog" elements.
  // Exclude PrimeNG confirm dialogs (role="alertdialog") which are always in the DOM.
  const dialog = page.locator(
    'p-dialog:not([role="alertdialog"]), p-dynamicdialog, [role="dialog"]:not([role="alertdialog"])'
  ).first();
  await assertVisible(dialog, getTimeout('short'));
}

/**
 * Assert dialog is closed
 * @param page - Playwright page object
 */
export async function assertDialogClosed(page: Page): Promise<void> {
  // Exclude PrimeNG confirm dialogs (role="alertdialog") which are always in the DOM
  const dialog = page.locator('p-dialog:not([role="alertdialog"]), [role="dialog"]:not([role="alertdialog"])').first();
  await assertHidden(dialog);
}

/**
 * Assert error message is displayed
 * @param page - Playwright page object
 * @param errorText - Optional specific error text to check
 */
export async function assertErrorDisplayed(
  page: Page,
  errorText?: string | RegExp
): Promise<void> {
  const errorLocator = page.locator(
    '.p-message-error, [role="alert"], .error-message'
  ).first();
  
  await assertVisible(errorLocator, getTimeout('short'));
  
  if (errorText) {
    await assertContainsText(errorLocator, errorText);
  }
}

/**
 * Assert success message is displayed
 * @param page - Playwright page object
 * @param successText - Optional specific success text to check
 */
export async function assertSuccessDisplayed(
  page: Page,
  successText?: string | RegExp
): Promise<void> {
  const successLocator = page.locator(
    '.p-message-success, .success-message'
  ).first();
  
  await assertVisible(successLocator, getTimeout('short'));
  
  if (successText) {
    await assertContainsText(successLocator, successText);
  }
}

/**
 * Assert element has attribute with value
 * @param locator - Element locator
 * @param attribute - Attribute name
 * @param value - Expected attribute value
 */
export async function assertHasAttribute(
  locator: Locator,
  attribute: string,
  value: string | RegExp
): Promise<void> {
  await expect(locator).toHaveAttribute(attribute, value);
}

/**
 * Assert page is responsive on mobile
 * @param page - Playwright page object
 * @param criticalElements - Array of critical element locators to verify
 */
export async function assertMobileResponsive(
  page: Page,
  criticalElements: Locator[]
): Promise<void> {
  // Switch to mobile viewport
  await page.setViewportSize({ width: 375, height: 667 });
  await page.waitForTimeout(1000);
  
  // Verify critical elements are still visible
  for (const element of criticalElements) {
    await assertVisible(element);
  }
}
