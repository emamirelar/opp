/**
 * @fileoverview Wait Helper
 * Provides reusable waiting functions for common UI states
 */

import { Page, Locator } from '@playwright/test';
import { getTimeout } from './test-config';

/**
 * Wait for loading overlays to disappear
 * @param page - Playwright page object
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForLoadingToComplete(page: Page, timeout?: number): Promise<void> {
  const maxTimeout = timeout || getTimeout('long');
  
  console.log('[Wait] Checking for loading overlays...');
  
  // Wait for the global loading overlay to be hidden
  // The overlay has class "bg-black bg-opacity-50" and contains p-progressSpinner
  const loadingOverlay = page.locator('.bg-black.bg-opacity-50').first();
  
  try {
    // Check if overlay exists and is visible
    const isVisible = await loadingOverlay.isVisible({ timeout: 1000 }).catch(() => false);
    
    if (isVisible) {
      console.log('[Wait] Loading overlay detected, waiting for it to disappear...');
      await loadingOverlay.waitFor({ state: 'hidden', timeout: maxTimeout });
      console.log('[Wait] Loading overlay disappeared');
    } else {
      console.log('[Wait] No loading overlay found');
    }
  } catch (error) {
    console.log('[Wait] Timeout waiting for loading overlay to disappear');
  }
  
  // Also wait for any PrimeNG progress spinners to disappear
  const progressSpinner = page.locator('p-progressSpinner').first();
  try {
    const spinnerVisible = await progressSpinner.isVisible({ timeout: 1000 }).catch(() => false);
    
    if (spinnerVisible) {
      console.log('[Wait] Progress spinner detected, waiting for it to disappear...');
      await progressSpinner.waitFor({ state: 'hidden', timeout: maxTimeout });
      console.log('[Wait] Progress spinner disappeared');
    }
  } catch (error) {
    console.log('[Wait] Timeout waiting for progress spinner to disappear');
  }
  
  // Small buffer after loading completes
  await page.waitForTimeout(500);
  console.log('[Wait] Loading complete');
}

/**
 * Wait for page to be ready for interaction
 * @param page - Playwright page object
 */
export async function waitForPageReady(page: Page): Promise<void> {
  console.log('[Wait] Waiting for page to be ready...');
  
  // Wait for network to be idle
  await page.waitForLoadState('networkidle', { timeout: getTimeout('long') }).catch(() => {
    console.log('[Wait] Network idle timeout - continuing anyway');
  });
  
  // Wait for any loading overlays
  await waitForLoadingToComplete(page);
  
  console.log('[Wait] Page is ready');
}

/**
 * Wait for element to be visible and stable
 * @param locator - Playwright locator
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForElementReady(locator: Locator, timeout?: number): Promise<void> {
  const maxTimeout = timeout || getTimeout('default');
  
  // Wait for element to be visible
  await locator.waitFor({ state: 'visible', timeout: maxTimeout });
  
  // Brief pause to allow animations/transitions to settle
  await locator.page().waitForTimeout(300);
}

/**
 * Wait for navigation and loading to complete
 * @param page - Playwright page object
 * @param urlPattern - Optional URL pattern to wait for
 */
export async function waitForNavigationComplete(
  page: Page,
  urlPattern?: string | RegExp
): Promise<void> {
  console.log('[Wait] Waiting for navigation to complete...');
  
  if (urlPattern) {
    await page.waitForURL(urlPattern, { timeout: getTimeout('default') });
  }
  
  await waitForPageReady(page);
  
  console.log('[Wait] Navigation complete');
}

/**
 * Wait for permissions to load
 * @param page - Playwright page object
 */
export async function waitForPermissions(page: Page): Promise<void> {
  console.log('[Wait] Waiting for permissions to load...');
  
  // Give time for permission API calls to complete
  await page.waitForTimeout(2000);
  
  // Wait for any loading overlays related to permissions
  await waitForLoadingToComplete(page);
  
  console.log('[Wait] Permissions loaded');
}

/**
 * Wait for element to be visible (convenience wrapper)
 * @param locator - Playwright locator
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForVisible(locator: Locator, timeout?: number): Promise<void> {
  await locator.waitFor({ state: 'visible', timeout: timeout || getTimeout('default') });
}

/**
 * Wait for element to be hidden
 * @param locator - Playwright locator
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForHidden(locator: Locator, timeout?: number): Promise<void> {
  await locator.waitFor({ state: 'hidden', timeout: timeout || getTimeout('default') });
}

/**
 * Wait for keyboard focus to move from the given element tag.
 * Uses page.waitForFunction (no arbitrary timeout) for deterministic focus settling.
 * @param page - Playwright page object
 * @param previousTag - Tag name of the previously focused element (e.g. 'BODY', 'A', 'BUTTON')
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForFocusChange(
  page: Page,
  previousTag: string,
  timeout?: number
): Promise<void> {
  const maxTimeout = timeout || 2000;
  await page.waitForFunction(
    (tag: string) => {
      const el = document.activeElement;
      return el != null && el.tagName !== tag;
    },
    previousTag,
    { timeout: maxTimeout }
  );
}

/**
 * Wait for network idle
 * @param page - Playwright page object
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForNetworkIdle(page: Page, timeout?: number): Promise<void> {
  await page.waitForLoadState('networkidle', { timeout: timeout || getTimeout('long') }).catch(() => {
    console.log('[Wait] Network idle timeout - continuing');
  });
}

/**
 * Wait for Angular application to be fully bootstrapped (webkit-specific)
 * Webkit needs explicit checks that Angular is ready before proceeding
 * @param page - Playwright page object
 */
export async function waitForAngularReady(page: Page): Promise<void> {
  const webkit = page.context().browser()?.browserType().name() === 'webkit';
  
  if (!webkit) {
    // Skip for non-webkit browsers (they're fast enough)
    return;
  }
  
  console.log('[Wait] Checking Angular ready state (webkit)...');
  
  try {
    // Wait for Angular to be defined on window
    await page.waitForFunction(() => {
      return typeof (window as any).ng !== 'undefined';
    }, { timeout: 60000 });
    
    // Wait for app-root element to be rendered
    await page.waitForSelector('app-root', { 
      state: 'attached',
      timeout: 60000 
    });
    
    // Additional stabilization time for Angular bootstrapping
    await page.waitForTimeout(3000);
    
    console.log('[Wait] Angular ready confirmed (webkit)');
  } catch (error) {
    console.warn('[Wait] Angular ready check timed out (non-critical):', error);
  }
}

/**
 * Wait for PrimeNG dialog to be visible and ready
 * NOTE: This waits for feature dialogs (p-dialog), NOT confirmation dialogs (p-confirmDialog)
 * PrimeNG v19 renders: <p-dialog> → <div class="p-dialog" role="dialog">
 * The role attribute is on the INNER div, not on the p-dialog component element.
 * @param page - Playwright page object
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForDialog(page: Page, timeout?: number): Promise<void> {
  const maxTimeout = timeout || getTimeout('default');
  
  console.log('[Wait] Waiting for dialog to appear...');
  
  // PrimeNG v19: role="dialog" is on the inner div, role="alertdialog" is on confirm dialogs.
  // Use the Playwright role locator to find visible feature dialogs, excluding alertdialogs.
  const dialog = page.locator('[role="dialog"]:visible, .p-dynamic-dialog:visible').first();
  await dialog.waitFor({ state: 'visible', timeout: maxTimeout });
  
  // Wait for dialog animation to complete
  await page.waitForTimeout(500);
  
  console.log('[Wait] Dialog is visible and ready');
}

/**
 * Wait for a minimum elapsed time. Use sparingly — only when time must pass
 * (e.g., timestamp resolution, rate limits). Prefer element/network-based waits when possible.
 * @param page - Playwright page object
 * @param ms - Minimum milliseconds to wait
 */
export async function waitForMinimumElapsed(page: Page, ms: number): Promise<void> {
  await page.waitForTimeout(ms);
}

/**
 * Wait for table data to load
 * @param page - Playwright page object
 * @param timeout - Optional timeout in milliseconds
 */
export async function waitForTableData(page: Page, timeout?: number): Promise<void> {
  const maxTimeout = timeout || getTimeout('default');
  
  console.log('[Wait] Waiting for table data to load...');
  
  // Wait for loading to complete first
  await waitForLoadingToComplete(page);
  
  // Wait for table body to be present
  const tableBody = page.locator('tbody, .p-datatable-tbody').first();
  await tableBody.waitFor({ state: 'attached', timeout: maxTimeout }).catch(() => {
    console.log('[Wait] Table body not found - may be empty table');
  });
  
  // Small buffer for data rendering
  await page.waitForTimeout(500);
  
  console.log('[Wait] Table data loaded');
}
