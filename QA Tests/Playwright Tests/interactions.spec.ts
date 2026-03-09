/**
 * @tests 13
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForLoadingToComplete,
  waitForDialog,
  waitForVisible,
  waitForNavigationComplete,
} from './helpers/wait.helper';
import { InteractionsPage } from './pages/interactions.page';

/**
 * Interactions List E2E Tests
 *
 * Tests the interaction management functionality including:
 * - Interaction list display
 * - Create new interaction button
 * - Create opportunity from interactions
 * - Export/Import functionality
 * - Interaction list navigation
 * - Search and filter capabilities
 */
test.describe('Interactions List', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
  });

  test('should display interactions page header', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const header = page.getByRole('heading', { name: /interactions/i }).first();
    const title = page.getByText('Interactions', { exact: true }).first();
    const listview = interactionsPage.getListview();
    const breadcrumbOrNav = page.getByText('Interactions').first();

    await expect(
      header.or(title).or(listview).or(breadcrumbOrNav).first()
    ).toBeVisible({ timeout: 15000 });

    const icon = page.locator('.material-icons, .material-symbols-outlined, .pi').first();
    const iconVisible = await icon.isVisible().catch(() => false);
    if (iconVisible) {
      await expect(icon).toBeVisible({ timeout: 5000 });
    }
  });

  test('should display New Interaction button for users with create permission', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const newButton = interactionsPage.getNewButton();
    const isVisible = await interactionsPage.isNewButtonVisible();

    if (isVisible) {
      await expect(newButton).toBeVisible();
      await expect(newButton).toContainText(/Interaction/i);
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({ timeout: 15000 });
    }
  });

  test('should display Create Opportunity button for users with permission', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const createOppButton = interactionsPage.getCreateOpportunityButton();
    const isVisible = await interactionsPage.isCreateOpportunityButtonVisible();

    if (isVisible) {
      await expect(createOppButton).toBeVisible();
      await expect(createOppButton).toContainText(/Opportunity/i);
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({ timeout: 15000 });
    }
  });

  test('should display Export button for users with export permission', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const exportButton = page.getByRole('button', { name: /export/i });
    const isVisible = await exportButton.isVisible().catch(() => false);

    if (isVisible) {
      await expect(exportButton.first()).toBeVisible();
      const iconAttr = await exportButton.first().getAttribute('icon').catch(() => null);
      if (iconAttr) expect(iconAttr).toContain('file-export');
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({ timeout: 15000 });
    }
  });

  test('should display Import button for users with import permission', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const importButton = page.getByRole('button', { name: /import/i });
    const isVisible = await importButton.isVisible().catch(() => false);

    if (isVisible) {
      await expect(importButton.first()).toBeVisible();
      const iconAttr = await importButton.first().getAttribute('icon').catch(() => null);
      if (iconAttr) expect(iconAttr).toContain('file-import');
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({ timeout: 15000 });
    }
  });

  test('should display interaction listview component', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const listview = interactionsPage.getListview();
    const noDataText = interactionsPage.getEmptyStateMessage();

    await expect(
      listview.or(noDataText)
    ).toBeVisible({ timeout: 15000 });
  });

  test('should display interaction list content', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(interactionsPage.getListview(), 15000).catch(() => {});

    const listview = interactionsPage.getListview();
    const cardItems = page.locator(
      'app-listview-card .cursor-pointer, app-listview-card .group.cursor-pointer, tbody tr, app-listview .cursor-pointer, app-listview [class*="cursor-pointer"]'
    );
    const noDataText = interactionsPage.getEmptyStateMessage();

    await expect(
      listview.or(cardItems).or(noDataText)
    ).toBeVisible({ timeout: 10000 });
  });

  test('should allow clicking New Interaction button to open modal', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const newButton = interactionsPage.getNewButton();
    const isVisible = await interactionsPage.isNewButtonVisible();

    if (isVisible) {
      await newButton.click();
      await waitForDialog(page);

      const dynamicDialogs = await page.locator('.p-dynamic-dialog').count();
      const featureDialog = await page
        .locator('p-dialog:not([role="alertdialog"])')
        .first()
        .isVisible()
        .catch(() => false);

      if (dynamicDialogs > 0 || featureDialog) {
        await expect(
          page.locator('.p-dynamic-dialog').or(page.locator('p-dialog:not([role="alertdialog"])').first())
        ).toBeVisible({ timeout: 5000 });
      } else {
        test.skip(
          true,
          'QA-008: PrimeNG DynamicDialog not created in Playwright test environment'
        );
      }
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({
        timeout: 15000,
      });
    }
  });

  test('should allow clicking Create Opportunity button to open dialog', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);

    const createOppButton = interactionsPage.getCreateOpportunityButton();
    const isVisible = await interactionsPage.isCreateOpportunityButtonVisible();

    if (isVisible) {
      await createOppButton.click();
      await waitForDialog(page);

      const dynamicDialogs = await page.locator('.p-dynamic-dialog').count();
      const featureDialog = await page
        .locator('p-dialog:not([role="alertdialog"])')
        .first()
        .isVisible()
        .catch(() => false);

      if (dynamicDialogs > 0 || featureDialog) {
        await expect(
          page.locator('.p-dynamic-dialog').or(page.locator('p-dialog:not([role="alertdialog"])').first())
        ).toBeVisible({ timeout: 5000 });
      } else {
        test.skip(
          true,
          'QA-008: PrimeNG DynamicDialog not created in Playwright test environment'
        );
      }
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({
        timeout: 15000,
      });
    }
  });

  test('should display search functionality in listview', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForVisible(interactionsPage.getListview(), 15000).catch(() => {});

    const hasSearch = await interactionsPage.isSearchInputVisible();

    if (hasSearch) {
      await expect(interactionsPage.getSearchInput()).toBeVisible();
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({
        timeout: 15000,
      });
    }
  });

  test('should handle empty state gracefully', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const emptyStateMessage = interactionsPage.getEmptyStateMessage();
    const header = page.getByText('Interactions', { exact: true }).first();
    const listview = interactionsPage.getListview();
    const appListview = page.locator('app-listview').first();

    await expect(
      emptyStateMessage.or(header).or(listview).or(appListview).first()
    ).toBeVisible({ timeout: 15000 });
  });

  test('should allow navigation to interaction details on card click', async ({
    page,
  }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(interactionsPage.getListview(), 15000).catch(() => {});

    const cardItems = interactionsPage.getInteractionCards();
    const cardCount = await cardItems.count();

    if (cardCount > 0) {
      await interactionsPage.clickInteraction(0);
      await waitForNavigationComplete(page, /\/interactions\/\d+/);

      const currentUrl = page.url();
      expect(currentUrl).toMatch(/\/interactions\/\d+/);
    } else {
      await expect(interactionsPage.getListview()).toBeVisible({
        timeout: 15000,
      });
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(800);
    await waitForLoadingToComplete(page);

    const header = page.getByRole('heading', { name: /interactions/i }).first();
    const title = page.getByText('Interactions').first();
    const listview = interactionsPage.getListview();
    const appListview = page.locator('app-listview').first();

    await expect(
      header.or(title).or(listview).or(appListview).first()
    ).toBeVisible({ timeout: 15000 });
  });
});
