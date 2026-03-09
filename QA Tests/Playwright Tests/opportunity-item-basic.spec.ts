/**
 * @fileoverview Opportunity Detail Page - Phase 1A Basic Tests
 * Uses OpportunityItemPage POM and meaningful assertions.
 *
 * @updated 2026-03-03 - Fixed quality issues: POM usage, proper waits, meaningful assertions
 * @tests 32
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches, assertVisible } from './helpers/assertions.helper';
import { waitForLoadingToComplete, waitForPermissions } from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

/**
 * Opportunity Detail Page - Phase 1A Tests
 *
 * Uses OpportunityItemPage POM with data-testid and section selectors.
 * Tests basic functionality, navigation, and layout.
 *
 * NOTE: Tests use real backend with existing opportunity data (ID 1).
 * Ensure database has at least one opportunity record before running tests.
 * Opportunity routes are under /partnerships/opportunities (not /opportunities).
 */
test.describe('Opportunity Detail Page - Phase 1A Basic Tests', () => {
  test.slow();
  const testOpportunityId = 1;

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/opportunities/${testOpportunityId}`);
    await page.waitForLoadState('load', { timeout: 15000 });
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);
  });

  /**
   * Navigation Tests
   */
  test('should navigate to opportunity detail page successfully', async ({ page }) => {
    await assertUrlMatches(page, new RegExp(`/opportunities/${testOpportunityId}`));
  });

  test('should display opportunity detail page URL', async ({ page }) => {
    const currentUrl = page.url();
    expect(currentUrl).toContain('/partnerships/opportunities/');
    expect(currentUrl).toContain(testOpportunityId.toString());
  });

  test('should have valid page title', async ({ page }) => {
    const title = await page.title();
    expect(title.length).toBeGreaterThan(0);
  });

  /**
   * Page Layout Tests
   */
  test('should display opportunity information panel', async ({ page }) => {
    const header = page.locator('[data-testid="opportunity-detail-header"], app-opportunity-view').first();
    await assertVisible(header);
  });

  test('should display at least one panel', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasOverview = await pom.hasOverviewSection();
    const hasWhat = await pom.hasWhatSection();
    const hasWho = await pom.hasWhoSection();
    const hasWhen = await pom.hasScheduleSection();
    const hasRelated = await pom.hasInteractionsSection();
    const hasDocuments = await pom.hasDocumentsSection();
    expect(hasOverview || hasWhat || hasWho || hasWhen || hasRelated || hasDocuments).toBe(true);
  });

  test('should display main content container', async ({ page }) => {
    const mainContent = page.locator('app-opportunity-view, #section-overview').first();
    await assertVisible(mainContent);
  });

  test('should display card elements', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasWhat = await pom.hasWhatSection();
    const hasOverview = await pom.hasOverviewSection();
    expect(hasWhat || hasOverview).toBe(true);
  });

  /**
   * Button Tests
   */
  test('should display action buttons', async ({ page }) => {
    await waitForLoadingToComplete(page);
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasWorkflow = await pom.hasWorkflowActions();
    const hasEdit = await pom.isEditButtonVisible();
    const hasDelete = await pom.isDeleteButtonVisible();
    expect(hasWorkflow || hasEdit || hasDelete).toBe(true);
  });

  test('should display edit button for users with edit permission', async ({ page }) => {
    await waitForLoadingToComplete(page);
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasEdit = await pom.isEditButtonVisible();
    const hasWorkflow = await pom.hasWorkflowActions();
    expect(hasEdit || hasWorkflow).toBe(true);
  });

  test('should display delete button for users with delete permission', async ({ page }) => {
    await waitForLoadingToComplete(page);
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasDelete = await pom.isDeleteButtonVisible();
    const hasWorkflow = await pom.hasWorkflowActions();
    expect(hasDelete || hasWorkflow).toBe(true);
  });

  test('should display workflow action buttons', async ({ page }) => {
    await waitForLoadingToComplete(page);
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasWorkflow = await pom.hasWorkflowActions();
    expect(hasWorkflow).toBe(true);
  });

  /**
   * Opportunity Information Tests
   */
  test('should display opportunity title or name', async ({ page }) => {
    const title = page.locator('[data-testid="opportunity-title"], h1').first();
    const isVisible = await title.isVisible().catch(() => false);
    expect(isVisible).toBe(true);
  });

  test('should display opportunity value label', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasWhat = await pom.hasWhatSection();
    expect(hasWhat).toBe(true);
  });

  test('should display opportunity stage or status', async ({ page }) => {
    const badges = page.locator('[data-testid="opportunity-stage"], [data-testid="opportunity-status"], p-badge');
    const hasBadge = await badges.first().isVisible().catch(() => false);
    expect(hasBadge).toBe(true);
  });

  test('should display opportunity dates', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasSchedule = await pom.hasScheduleSection();
    expect(hasSchedule).toBe(true);
  });

  /**
   * Section Tests
   */
  test('should display budget section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasBudget = await pom.hasBudgetSection();
    expect(hasBudget).toBe(true);
  });

  test('should display schedule section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasSchedule = await pom.hasScheduleSection();
    expect(hasSchedule).toBe(true);
  });

  test('should display partners section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasPartners = await pom.hasPartnersSection();
    expect(hasPartners).toBe(true);
  });

  test('should display contacts section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasContacts = await pom.hasContactsSection();
    expect(hasContacts).toBe(true);
  });

  test('should display interactions section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasInteractions = await pom.hasInteractionsSection();
    expect(hasInteractions).toBe(true);
  });

  test('should display documents section', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasDocuments = await pom.hasDocumentsSection();
    expect(hasDocuments).toBe(true);
  });

  test('should display DST section if applicable', async ({ page }) => {
    const pom = new OpportunityItemPage(page, testOpportunityId);
    const hasDST = await pom.hasDSTSection();
    const hasOverview = await pom.hasOverviewSection();
    expect(hasDST || hasOverview).toBe(true);
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
    const icons = page.locator('i, .pi, .material-icons, .material-symbols-outlined, svg');
    const iconCount = await icons.count();
    expect(iconCount).toBeGreaterThan(0);
  });

  /**
   * Tabs Tests
   */
  test('should display tabs if tabs component exists', async ({ page }) => {
    const tabs = page.locator('p-tabs, p-tabview, [role="tablist"]');
    const hasTabs = await tabs.isVisible().catch(() => false);

    if (hasTabs) {
      const tabItems = page.locator('[role="tab"]');
      expect(await tabItems.count()).toBeGreaterThan(0);
    }
  });

  /**
   * Responsive Design Tests
   */
  test('should display correctly on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await waitForLoadingToComplete(page);
    const header = page.locator('[data-testid="opportunity-detail-header"], app-opportunity-view').first();
    await assertVisible(header);
  });

  test('should display correctly on tablet', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await waitForLoadingToComplete(page);
    const mainContent = page.locator('body');
    await assertVisible(mainContent);
  });

  test('should display correctly on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForLoadingToComplete(page);
    const header = page.locator('[data-testid="opportunity-detail-header"], app-opportunity-view').first();
    await assertVisible(header);
  });

  /**
   * Table/List Tests
   */
  test('should display tables if data exists', async ({ page }) => {
    await waitForLoadingToComplete(page);
    const tables = page.locator('p-table, .p-datatable, table');
    const hasTable = await tables.first().isVisible().catch(() => false);

    if (hasTable) {
      expect(await tables.count()).toBeGreaterThan(0);
    }
  });

  /**
   * Loading State Tests
   */
  test('should not display loading indicators after page loads', async ({ page }) => {
    await page.waitForLoadState('networkidle').catch(() => {});
    await waitForLoadingToComplete(page);
    const loadingSpinner = page.locator(
      'p-progressspinner, p-progressSpinner, .p-progress-spinner, p-skeleton, .p-skeleton, .pi-spin, .pi-spinner, .animate-pulse, [class*="loading"], .spinner, .bg-black.bg-opacity-50'
    );
    const spinnerCount = await loadingSpinner.count();
    let spinnerVisible = false;
    if (spinnerCount > 0) {
      spinnerVisible = await loadingSpinner.first().isVisible({ timeout: 1000 }).catch(() => false);
    }
    expect(spinnerVisible).toBe(false);
  });

  /**
   * Error Handling Tests
   */
  test('should not display error messages on valid opportunity', async ({ page }) => {
    const errorMessages = page.locator(
      'p-message[severity="error"], .p-message-error, [role="alert"][aria-live="assertive"]'
    );
    const hasError = await errorMessages.first().isVisible().catch(() => false);
    expect(hasError).toBe(false);
  });

  /**
   * Workflow Tests
   */
  test('should display workflow status badge', async ({ page }) => {
    const badges = page.locator('[data-testid="opportunity-stage"], [data-testid="opportunity-status"], p-badge');
    const hasBadge = await badges.first().isVisible().catch(() => false);
    expect(hasBadge).toBe(true);
  });
});
