/**
 * @fileoverview Navigation Helper
 * Provides reusable navigation functions for E2E tests
 */

import { Page } from '@playwright/test';
import { waitForNetworkIdle } from './wait.helper';

/**
 * Navigate to a page and wait for it to load
 * @param page - Playwright page object
 * @param url - Relative or absolute URL
 */
export async function navigateTo(page: Page, url: string): Promise<void> {
  await page.goto(url);
  await waitForNetworkIdle(page);
}

/**
 * Navigate to Partners page
 * @param page - Playwright page object
 */
export async function navigateToPartners(page: Page): Promise<void> {
  await navigateTo(page, '/partners');
}

/**
 * Navigate to Contacts page
 * @param page - Playwright page object
 */
export async function navigateToContacts(page: Page): Promise<void> {
  await navigateTo(page, '/contacts');
}

/**
 * Navigate to Interactions page
 * @param page - Playwright page object
 */
export async function navigateToInteractions(page: Page): Promise<void> {
  await navigateTo(page, '/interactions');
}

/**
 * Navigate to Opportunities page
 * @param page - Playwright page object
 */
export async function navigateToOpportunities(page: Page): Promise<void> {
  await navigateTo(page, '/opportunities');
}

/**
 * Navigate to Dashboard/Home page
 * @param page - Playwright page object
 */
export async function navigateToDashboard(page: Page): Promise<void> {
  await navigateTo(page, '/home');
}

/**
 * Navigate to entity detail page
 * @param page - Playwright page object
 * @param entityType - Type of entity (partners, contacts, opportunities, etc.)
 * @param id - Entity ID
 */
export async function navigateToEntityDetail(
  page: Page,
  entityType: string,
  id: number | string
): Promise<void> {
  await navigateTo(page, `/${entityType}/${id}`);
}

/**
 * Use browser back button
 * @param page - Playwright page object
 */
export async function goBack(page: Page): Promise<void> {
  await page.goBack();
  await waitForNetworkIdle(page);
}

/**
 * Use browser forward button
 * @param page - Playwright page object
 */
export async function goForward(page: Page): Promise<void> {
  await page.goForward();
  await waitForNetworkIdle(page);
}

/**
 * Reload the current page
 * @param page - Playwright page object
 */
export async function reloadPage(page: Page): Promise<void> {
  await page.reload();
  await waitForNetworkIdle(page);
}
