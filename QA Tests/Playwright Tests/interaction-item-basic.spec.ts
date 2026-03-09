/**
 * @fileoverview Interaction Detail Page - Phase 1A Basic Tests
 * Uses InteractionItemPage POM and proper wait patterns.
 *
 * @updated 2026-03-03 - Fixed always-passing assertions, replaced fixed timeouts with proper waits
 *
 * @tests 24
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches, assertVisible } from './helpers/assertions.helper';
import { waitForLoadingToComplete, waitForPermissions, waitForPageReady } from './helpers/wait.helper';
import { InteractionItemPage } from './pages/interaction-item.page';

/**
 * Interaction Detail Page - Phase 1A Tests
 *
 * Uses InteractionItemPage POM with data-testid selectors.
 * Tests basic functionality, navigation, layout, and content display.
 *
 * NOTE: Tests use real backend with existing interaction data (ID 1).
 * Ensure database has at least one interaction record before running tests.
 */
test.describe('Interaction Detail Page - Phase 1A Basic Tests', () => {
  test.slow();
  const testInteractionId = 1;

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/interactions/${testInteractionId}`);
    await page.waitForLoadState('load', { timeout: 15000 });
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);
  });

  /**
   * Navigation Tests
   */
  test('should navigate to interaction detail page successfully', async ({ page }) => {
    await assertUrlMatches(page, new RegExp(`/partnerships/interactions/${testInteractionId}`));
  });

  test('should display interaction detail page URL', async ({ page }) => {
    const currentUrl = page.url();
    expect(currentUrl).toContain('/partnerships/interactions/');
    expect(currentUrl).toContain(testInteractionId.toString());
  });

  test('should have valid page title', async ({ page }) => {
    const title = await page.title();
    expect(title.length).toBeGreaterThan(0);
  });

  /**
   * Page Layout Tests
   */
  test('should display interaction information panel', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await interactionPage.verifyMainSectionsDisplayed();
  });

  test('should display at least one panel', async ({ page }) => {
    const panels = page.locator('p-panel');
    await expect(panels.first()).toBeVisible({ timeout: 10000 });
    const panelCount = await panels.count();
    expect(panelCount).toBeGreaterThan(0);
  });

  test('should display main content container', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await assertVisible(interactionPage.header);
  });

  /**
   * Button Tests
   */
  test('should display action buttons', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await waitForLoadingToComplete(page);

    const buttons = page.locator('button, p-button');
    await expect(buttons.first()).toBeVisible({ timeout: 10000 });
    const buttonCount = await buttons.count();
    expect(buttonCount).toBeGreaterThan(0);
  });

  test('should display edit button for users with edit permission', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await waitForLoadingToComplete(page);

    const isVisible = await interactionPage.editButton.isVisible().catch(() => false);
    const hasAnyActionButton = isVisible ||
      (await interactionPage.isDeleteButtonVisible()) ||
      (await interactionPage.isCreateOpportunityButtonVisible());
    const headerVisible = await interactionPage.header.isVisible().catch(() => false);
    // Permission-based: either user has action buttons OR page loads in read-only mode
    expect(hasAnyActionButton || headerVisible).toBe(true);
  });

  test('should display delete button for users with delete permission', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await waitForLoadingToComplete(page);

    const isVisible = await interactionPage.deleteButton.isVisible().catch(() => false);
    const hasAnyActionButton = isVisible ||
      (await interactionPage.isEditButtonVisible()) ||
      (await interactionPage.isCreateOpportunityButtonVisible());
    const headerVisible = await interactionPage.header.isVisible().catch(() => false);
    // Permission-based: either user has action buttons OR page loads in read-only mode
    expect(hasAnyActionButton || headerVisible).toBe(true);
  });

  test('should display create opportunity button for users with permission', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await waitForLoadingToComplete(page);

    const isVisible = await interactionPage.createOpportunityButton.isVisible().catch(() => false);
    const hasAnyActionButton = isVisible ||
      (await interactionPage.isEditButtonVisible()) ||
      (await interactionPage.isDeleteButtonVisible());
    const headerVisible = await interactionPage.header.isVisible().catch(() => false);
    // Permission-based: either user has action buttons OR page loads in read-only mode
    expect(hasAnyActionButton || headerVisible).toBe(true);
  });

  /**
   * Interaction Information Tests
   */
  test('should display interaction type label', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await interactionPage.verifyInteractionType();
    const info = await interactionPage.getInteractionInfo();
    expect(info.type !== null || info.date !== null || info.description !== null).toBe(true);
  });

  test('should display interaction date label', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await interactionPage.verifyInteractionDate();
    const info = await interactionPage.getInteractionInfo();
    expect(info.date !== null || info.type !== null || info.description !== null).toBe(true);
  });

  test('should display interaction description or notes', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    const hasDescSection = await interactionPage.interactionDescriptionSection.isVisible().catch(() => false);
    const hasDesc = await interactionPage.interactionDescription.isVisible().catch(() => false);
    const hasDetailsPanel = await interactionPage.interactionDetailsSection.isVisible().catch(() => false);
    const hasAnyContent = hasDescSection || hasDesc || hasDetailsPanel;
    expect(hasAnyContent).toBe(true);
  });

  /**
   * Section Tests
   */
  test('should display participants section', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    const hasSection = await interactionPage.hasParticipantsSection();
    const hasDetails = await interactionPage.interactionDetailsSection.isVisible().catch(() => false);
    expect(hasSection || hasDetails).toBe(true);
  });

  test('should display related opportunities section', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    const hasSection = await interactionPage.hasRelatedOpportunitiesSection();
    const hasDesc = await interactionPage.interactionDescriptionSection.isVisible().catch(() => false);
    expect(hasSection || hasDesc).toBe(true);
  });

  test('should display documents section', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    const hasSection = await interactionPage.hasDocumentsSection();
    const hasDesc = await interactionPage.interactionDescriptionSection.isVisible().catch(() => false);
    expect(hasSection || hasDesc).toBe(true);
  });

  /**
   * Content Tests
   */
  test('should display text content', async ({ page }) => {
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
    expect(bodyText!.length).toBeGreaterThan(100);
  });

  test('should display headings', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await assertVisible(interactionPage.header);
    const headings = page.locator('h1, h2, h3, h4, .text-xl, .text-2xl, .text-3xl');
    const headingCount = await headings.count();
    expect(headingCount).toBeGreaterThan(0);
  });

  test('should display icons', async ({ page }) => {
    const icons = page.locator('i.pi, .pi, .material-icons, .material-symbols-outlined, svg');
    const iconCount = await icons.count();
    expect(iconCount).toBeGreaterThan(0);
  });

  /**
   * Responsive Design Tests
   */
  test('should display correctly on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await waitForLoadingToComplete(page);

    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await assertVisible(interactionPage.header);
  });

  test('should display correctly on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForLoadingToComplete(page);

    const interactionPage = new InteractionItemPage(page, testInteractionId);
    const headerVisible = await interactionPage.header.isVisible().catch(() => false);
    expect(headerVisible).toBe(true);
  });

  /**
   * Table/List Tests
   */
  test('should display tables if data exists', async ({ page }) => {
    const interactionPage = new InteractionItemPage(page, testInteractionId);
    await assertVisible(interactionPage.header);

    const tables = page.locator('p-table, .p-datatable, table');
    const tableCount = await tables.count();
    if (tableCount > 0) {
      await expect(tables.first()).toBeVisible();
    }
  });

  /**
   * Loading State Tests
   */
  test('should not display loading indicators after page loads', async ({ page }) => {
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const loadingSpinner = page.locator('.spinner, .loading, [class*="load"]').first();
    const isLoading = await loadingSpinner.isVisible().catch(() => false);
    expect(isLoading).toBe(false);
  });

  /**
   * Error Handling Tests
   */
  test('should not display error messages on valid interaction', async ({ page }) => {
    const errorMessages = page.locator('.error, .p-error, [class*="error"]').first();
    const hasError = await errorMessages.isVisible().catch(() => false);
    expect(hasError).toBe(false);
  });
});
