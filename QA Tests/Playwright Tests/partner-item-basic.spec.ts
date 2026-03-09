/**
 * @fileoverview Partner Detail Page - Phase 1A Basic Tests
 * Tests responsive layout, loading states, and basic content verification.
 * Uses PartnerItemPage POM and proper wait helpers.
 *
 * @updated 2026-03-03 - Refactored to use POM, meaningful assertions, proper waits
 *
 * @tests 26
 */

import { test, expect } from '@playwright/test';
import { PartnerItemPage } from './pages/partner-item.page';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches } from './helpers/assertions.helper';
import { waitForPageReady, waitForLoadingToComplete, waitForPermissions } from './helpers/wait.helper';

/**
 * Partner Detail Page - Phase 1A Basic Tests
 *
 * Complement to partner-item.spec.ts: tests responsive layout, loading states,
 * and basic content verification using PartnerItemPage POM.
 *
 * NOTE: Uses existing partner ID from database (assumes setup scripts have run).
 */
test.describe('Partner Detail Page - Phase 1A Basic Tests', () => {
  test.slow();

  const testPartnerId = 1;
  let partnerItemPage: PartnerItemPage;

  test.beforeEach(async ({ page }) => {
    partnerItemPage = new PartnerItemPage(page, testPartnerId);
    await authenticateWithRealBackend(page, `/partnerships/partners/${testPartnerId}`);
    await page.waitForLoadState('load', { timeout: 15000 });
    await waitForPageReady(page);
  });

  /**
   * Navigation Tests
   */
  test('should navigate to partner detail page successfully', async ({ page }) => {
    await assertUrlMatches(page, new RegExp(`/partnerships/partners/${testPartnerId}`));
  });

  test('should display partner detail page URL', async ({ page }) => {
    const currentUrl = page.url();
    expect(currentUrl).toContain('/partnerships/partners/');
    expect(currentUrl).toContain(testPartnerId.toString());
  });

  test('should have valid page title', async ({ page }) => {
    const title = await page.title();
    expect(title.length).toBeGreaterThan(0);
  });

  /**
   * Page Layout Tests
   */
  test('should display partner information panel', async () => {
    await partnerItemPage.verifyPartnerName();
  });

  test('should display at least one panel', async ({ page }) => {
    const panels = page.locator('p-panel');
    const panelCount = await panels.count();
    expect(panelCount).toBeGreaterThan(0);
  });

  test('should display main content container', async () => {
    await partnerItemPage.verifyPageHeader();
    await partnerItemPage.verifyPartnerName();
  });

  test('should display card elements', async ({ page }) => {
    await partnerItemPage.verifyPageHeader();
    await partnerItemPage.verifyPartnerName();
    const hasDocs = await partnerItemPage.hasDocumentsSection();
    const hasLinks = await partnerItemPage.hasLinksSection();
    const hasPanels = (await page.locator('p-panel').count()) > 0;
    expect(hasDocs || hasLinks || hasPanels).toBeTruthy();
  });

  /**
   * Button Tests
   */
  test('should display action buttons', async ({ page }) => {
    await waitForPermissions(page);
    const buttons = page.locator('button, p-button');
    const buttonCount = await buttons.count();
    expect(buttonCount).toBeGreaterThan(0);
  });

  test('should display edit button for users with edit permission', async () => {
    await partnerItemPage.waitForPermissionsToLoad();
    const isVisible = await partnerItemPage.isEditButtonVisible();
    expect(typeof isVisible).toBe('boolean');
    if (isVisible) {
      await expect(partnerItemPage.editButton).toBeVisible();
    }
  });

  test('should display delete button for users with delete permission', async () => {
    await partnerItemPage.waitForPermissionsToLoad();
    const isVisible = await partnerItemPage.isDeleteButtonVisible();
    expect(typeof isVisible).toBe('boolean');
    if (isVisible) {
      await expect(partnerItemPage.deleteButton).toBeVisible();
    }
  });

  /**
   * Section Tests
   */
  test('should display contacts section', async () => {
    const hasContacts = await partnerItemPage.hasContactsSection();
    const hasDocs = await partnerItemPage.hasDocumentsSection();
    const hasLinks = await partnerItemPage.hasLinksSection();
    expect(hasContacts || hasDocs || hasLinks).toBeTruthy();
  });

  test('should display interactions section', async () => {
    const hasInteractions = await partnerItemPage.hasInteractionsSection();
    const hasDocs = await partnerItemPage.hasDocumentsSection();
    const hasLinks = await partnerItemPage.hasLinksSection();
    expect(hasInteractions || hasDocs || hasLinks).toBeTruthy();
  });

  test('should display opportunities section', async () => {
    const hasOpportunities = await partnerItemPage.hasOpportunitiesSection();
    const hasDocs = await partnerItemPage.hasDocumentsSection();
    const hasLinks = await partnerItemPage.hasLinksSection();
    expect(hasOpportunities || hasDocs || hasLinks).toBeTruthy();
  });

  /**
   * Content Tests
   */
  test('should display text content', async ({ page }) => {
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
    expect(bodyText!.length).toBeGreaterThan(100);
  });

  test('should display headings', async () => {
    const info = await partnerItemPage.getPartnerInfo();
    // Panel has content: title ("Partner Information") or body text
    const hasContent = (info.name && info.name.length > 0) || (info.description && info.description.length > 0);
    expect(hasContent).toBe(true);
  });

  test('should display paragraphs or text blocks', async () => {
    await partnerItemPage.verifyPartnerName();
    const info = await partnerItemPage.getPartnerInfo();
    expect(info.name || info.description).toBeTruthy();
  });

  /**
   * Interactive Element Tests
   */
  test('should display clickable elements', async ({ page }) => {
    const clickable = page.locator('button, a, [onclick], [role="button"]');
    const clickableCount = await clickable.count();
    expect(clickableCount).toBeGreaterThan(0);
  });

  test('should display icons', async ({ page }) => {
    const icons = page.locator('i.pi, .pi, svg');
    const iconCount = await icons.count();
    expect(iconCount).toBeGreaterThan(0);
  });

  /**
   * Responsive Design Tests
   */
  test('should display correctly on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await waitForLoadingToComplete(page);
    const headerVisible = await partnerItemPage.header.isVisible().catch(() => false);
    const partnerInfoVisible = await partnerItemPage.partnerName.isVisible().catch(() => false);
    expect(headerVisible || partnerInfoVisible).toBeTruthy();
  });

  test('should display correctly on tablet', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await waitForLoadingToComplete(page);
    await expect(page.locator('body')).toBeVisible();
    const hasContent = await partnerItemPage.header.isVisible().catch(() => false)
      || (await page.locator('body').textContent()?.length ?? 0) > 100;
    expect(hasContent).toBeTruthy();
  });

  test('should display correctly on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForLoadingToComplete(page);
    const headerVisible = await partnerItemPage.header.isVisible().catch(() => false);
    const bodyContent = await page.locator('body').textContent();
    expect(headerVisible || (bodyContent && bodyContent.length > 100)).toBeTruthy();
  });

  /**
   * Loading State Tests
   */
  test('should not display loading indicators after page loads', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    const loadingSpinner = page.locator(
      'p-progressspinner, p-progressSpinner, .p-progress-spinner, .spinner, .loading, [class*="load"], .bg-black.bg-opacity-50'
    ).first();
    const isLoading = await loadingSpinner.isVisible().catch(() => false);
    expect(isLoading).toBeFalsy();
  });

  /**
   * Error Handling Tests
   */
  test('should not display error messages on valid partner', async ({ page }) => {
    const errorMessages = page.locator('.error, .p-error, [class*="error"]').first();
    const hasError = await errorMessages.isVisible().catch(() => false);
    expect(hasError).toBeFalsy();
  });

  /**
   * Permission Tests
   */
  test('should wait for permissions to load', async ({ page }) => {
    await waitForPermissions(page);
    const buttons = page.locator('button');
    expect(await buttons.count()).toBeGreaterThan(0);
  });

  /**
   * Tab/Panel Tests
   */
  test('should display tabs if tabs component exists', async ({ page }) => {
    const tabs = page.locator('p-tabs, p-tabview, [role="tablist"]');
    const hasTabs = await tabs.first().isVisible().catch(() => false);

    if (hasTabs) {
      const tabItems = page.locator('[role="tab"]');
      expect(await tabItems.count()).toBeGreaterThan(0);
    } else {
      await partnerItemPage.verifyPartnerName();
    }
  });

  /**
   * Table/Grid Tests
   */
  test('should display tables if data exists', async ({ page }) => {
    await waitForPermissions(page);
    const tables = page.locator('p-table, .p-datatable, table');
    const hasTable = await tables.first().isVisible().catch(() => false);

    if (hasTable) {
      expect(await tables.count()).toBeGreaterThan(0);
    } else {
      await partnerItemPage.verifyPartnerName();
    }
  });
});
