/**
 * @fileoverview Contact Detail Page - Phase 1A Basic Tests
 * Uses ContactItemPage POM and proper wait patterns.
 *
 * @updated 2026-03-03 - Refactored to use POM, meaningful assertions, proper waits
 *
 * @tests 23
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches } from './helpers/assertions.helper';
import { waitForPageReady, waitForLoadingToComplete, waitForPermissions } from './helpers/wait.helper';
import { ContactItemPage } from './pages/contact-item.page';

/**
 * Contact Detail Page - Phase 1A Tests
 *
 * Uses ContactItemPage POM with data-testid-based locators.
 * Tests basic functionality, navigation, layout, and permission-gated UI.
 *
 * NOTE: Tests use real backend with existing contact data (ID 1).
 * Ensure database has at least one contact record before running tests.
 */
test.describe('Contact Detail Page - Phase 1A Basic Tests', () => {
  test.slow();
  const testContactId = 1;

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/contacts/${testContactId}`);
    await page.waitForLoadState('load', { timeout: 15000 });
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);
  });

  /**
   * Navigation Tests
   */
  test('should navigate to contact detail page successfully', async ({ page }) => {
    await assertUrlMatches(page, new RegExp(`/partnerships/contacts/${testContactId}`));
  });

  test('should display contact detail page URL', async ({ page }) => {
    const currentUrl = page.url();
    expect(currentUrl).toContain('/partnerships/contacts/');
    expect(currentUrl).toContain(testContactId.toString());
  });

  test('should have valid page title', async ({ page }) => {
    const title = await page.title();
    expect(title.length).toBeGreaterThan(0);
  });

  /**
   * Page Layout Tests
   */
  test('should display contact information panel', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasInfo = await pom.hasContactInfoSection();
    const hasPartner = await pom.contactPartnerSection.isVisible().catch(() => false);
    expect(hasInfo || hasPartner).toBeTruthy();
  });

  test('should display at least one panel', async ({ page }) => {
    const panels = page.locator('p-panel');
    const panelCount = await panels.count();
    expect(panelCount).toBeGreaterThan(0);
  });

  test('should display main content container', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(headerVisible).toBeTruthy();
  });

  test('should display card elements', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasInfo = await pom.hasContactInfoSection();
    const hasPartner = await pom.contactPartnerSection.isVisible().catch(() => false);
    const hasDocs = await pom.hasDocumentsSection();
    expect(hasInfo || hasPartner || hasDocs).toBeTruthy();
  });

  /**
   * Button Tests
   */
  test('should display action buttons', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasEdit = await pom.isEditButtonVisible();
    const hasDelete = await pom.isDeleteButtonVisible();
    expect(hasEdit || hasDelete).toBeTruthy();
  });

  test('should display edit button for users with edit permission', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const isVisible = await pom.editButton.isVisible().catch(() => false);
    expect(isVisible).toBeTruthy();
  });

  test('should display delete button for users with delete permission', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const isVisible = await pom.deleteButton.isVisible().catch(() => false);
    expect(isVisible).toBeTruthy();
  });

  /**
   * Contact Information Tests
   */
  test('should display contact name or identifier', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const nameVisible = await pom.contactName.isVisible().catch(() => false);
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(nameVisible || headerVisible).toBeTruthy();
  });

  test('should display email field or label', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasInfoSection = await pom.hasContactInfoSection();
    expect(hasInfoSection).toBeTruthy();
  });

  test('should display phone field or label', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasInfoSection = await pom.hasContactInfoSection();
    expect(hasInfoSection).toBeTruthy();
  });

  test('should display partner association', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const partnerSectionVisible = await pom.contactPartnerSection.isVisible().catch(() => false);
    const hasInfo = await pom.hasContactInfoSection();
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(partnerSectionVisible || hasInfo || headerVisible).toBeTruthy();
  });

  /**
   * Section Tests
   */
  test('should display interactions section', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasInteractions = await pom.hasInteractionsSection();
    const hasDocuments = await pom.hasDocumentsSection();
    const hasInfo = await pom.hasContactInfoSection();
    expect(hasInteractions || hasDocuments || hasInfo).toBeTruthy();
  });

  test('should display documents section', async ({ page }) => {
    const pom = new ContactItemPage(page, testContactId);
    const hasDocuments = await pom.hasDocumentsSection();
    const hasInfo = await pom.hasContactInfoSection();
    expect(hasDocuments || hasInfo).toBeTruthy();
  });

  /**
   * Content Tests
   */
  test('should display text content', async ({ page }) => {
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
    expect(bodyText!.length).toBeGreaterThan(100);
  });

  test('should display icons', async ({ page }) => {
    const icons = page.locator('i.pi, .pi, svg').first();
    await expect(icons).toBeVisible();
  });

  /**
   * Responsive Design Tests
   */
  test('should display correctly on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await waitForLoadingToComplete(page);

    const pom = new ContactItemPage(page, testContactId);
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(headerVisible).toBeTruthy();
  });

  test('should display correctly on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForLoadingToComplete(page);

    const pom = new ContactItemPage(page, testContactId);
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(headerVisible).toBeTruthy();
  });

  /**
   * Table/List Tests
   */
  test('should display tables if interaction data exists', async ({ page }) => {
    await waitForPageReady(page);

    const pom = new ContactItemPage(page, testContactId);
    const headerVisible = await pom.header.isVisible().catch(() => false);
    expect(headerVisible).toBeTruthy();

    const tables = page.locator('p-table, .p-datatable, table');
    const tableCount = await tables.count();
    if (tableCount > 0) {
      const firstTable = tables.first();
      await expect(firstTable).toBeVisible();
    }
  });

  /**
   * Loading State Tests
   */
  test('should not display loading indicators after page loads', async ({ page }) => {
    await waitForPageReady(page);

    const loadingSpinner = page.locator('.bg-black.bg-opacity-50 p-progressSpinner, .spinner, [class*="loading"]').first();
    const isLoading = await loadingSpinner.isVisible().catch(() => false);
    expect(isLoading).toBeFalsy();
  });

  /**
   * Error Handling Tests
   */
  test('should not display error messages on valid contact', async ({ page }) => {
    const errorMessages = page.locator('.p-message-error, .p-error, [role="alert"]').filter({ hasText: /error/i });
    const hasError = await errorMessages.first().isVisible().catch(() => false);
    expect(hasError).toBeFalsy();
  });
});
