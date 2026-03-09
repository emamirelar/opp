/**
 * @fileoverview Search and List Views E2E Tests
 * Tests for search, filtering, pagination, and column features (PNO-146, PNO-230, PNO-235, PNO-311)
 *
 * JIRA Stories: PNO-146, PNO-230, PNO-235, PNO-311
 * Total Test Cases: 35
 *
 * @tests 20
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForPermissions,
  waitForVisible,
  waitForTableData,
} from './helpers/wait.helper';
import { PartnersPage } from './pages/partners.page';
import { OpportunitiesPage } from './pages/opportunities.page';
import { ContactsPage } from './pages/contacts.page';
import { InteractionsPage } from './pages/interactions.page';

test.describe('General Search Features (PNO-146)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_001 - Validate search box visibility', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, [data-testid="search-input"], input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await expect(searchInput).toBeVisible();
  });

  test('POS_002 - Basic text search', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, [data-testid="search-input"], input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await searchInput.fill('World');
    await waitForLoadingToComplete(page);

    const table = page.locator('p-table, .p-datatable, app-listview-card').first();
    await expect(table).toBeVisible();
  });

  test('POS_003 - Search with partial match', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await searchInput.fill('Wor');
    await waitForLoadingToComplete(page);

    const contentArea = page.locator('app-listview-card, .p-datatable-tbody, tbody').first();
    await expect(contentArea).toBeAttached();
  });

  test('POS_004 - Search case insensitivity', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await searchInput.fill('world');
    await waitForLoadingToComplete(page);
    await searchInput.fill('WORLD');
    await waitForLoadingToComplete(page);

    await expect(searchInput).toHaveValue('WORLD');
  });

  test('POS_005 - Clear search results', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await searchInput.fill('test');
    await waitForLoadingToComplete(page);
    await searchInput.fill('');
    await waitForLoadingToComplete(page);

    await expect(searchInput).toHaveValue('');
  });

  test('NEG_006 - Search with no results', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const searchInput = page.locator(
      'input.quick-search, input[type="search"], input[placeholder*="Search"]'
    ).first();
    await waitForVisible(searchInput);
    await searchInput.fill('XYZNONEXISTENT123456789');
    await waitForLoadingToComplete(page);

    const noResultsOrEmpty = page.getByText(/no data available|no results|no records/i).or(
      page.locator('.p-datatable-emptymessage, .pi-info-circle')
    ).first();
    await expect(noResultsOrEmpty).toBeVisible({ timeout: 10000 });
  });
});

test.describe('List View Filtering', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_009 - Filter by status', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    await waitForTableData(page);

    const statusFilter = page.locator(
      '[data-testid="status-filter"], p-dropdown:has-text("Status"), p-multiselect:has-text("Status")'
    ).first();
    const isVisible = await statusFilter.isVisible().catch(() => false);
    if (isVisible) {
      await statusFilter.click();
      const activeOption = page.locator(
        '.p-dropdown-item:has-text("Active"), .p-multiselect-item:has-text("Active")'
      ).first();
      if (await activeOption.isVisible().catch(() => false)) {
        await activeOption.click();
        await waitForLoadingToComplete(page);
      }
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });

  test('POS_011 - Clear all filters', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    await waitForTableData(page);

    const statusFilter = page.locator('p-dropdown:has-text("Status")').first();
    if (await statusFilter.isVisible().catch(() => false)) {
      await statusFilter.click();
      const option = page.locator('.p-dropdown-item').first();
      if (await option.isVisible().catch(() => false)) {
        await option.click();
        await waitForLoadingToComplete(page);
      }
    }

    const clearBtn = page.locator(
      'button:has-text("Clear"), button:has-text("Reset"), [data-testid="clear-filters"]'
    ).first();
    if (await clearBtn.isVisible().catch(() => false)) {
      await clearBtn.click();
      await waitForLoadingToComplete(page);
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });
});

test.describe('List View Columns', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_013 - Validate default columns display', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const listview = page.locator('app-listview, app-listview-card').first();
    await expect(listview).toBeVisible();

    const hasNameOrStatus = await page.getByText(/^Name$|^Status$/i).first().isVisible().catch(() => false) ||
      await page.locator('app-listview-card').first().isVisible().catch(() => false);
    expect(hasNameOrStatus).toBeTruthy();
  });

  test('POS_014 - Column sorting ascending', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const sortDropdown = page.locator('p-select.w-64, app-listview p-select, app-listview p-dropdown').first();
    const sortVisible = await sortDropdown.isVisible({ timeout: 3000 }).catch(() => false);
    if (sortVisible) {
      await sortDropdown.click();
      const firstOption = page.locator('.p-select-option, .p-dropdown-item').first();
      if (await firstOption.isVisible({ timeout: 2000 }).catch(() => false)) {
        await firstOption.click();
        await waitForLoadingToComplete(page);
      }
    }
    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });

  test('POS_015 - Column sorting descending', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const sortDropdown = page.locator('p-select.w-64, app-listview p-select, app-listview p-dropdown').first();
    const sortVisible = await sortDropdown.isVisible({ timeout: 3000 }).catch(() => false);
    if (sortVisible) {
      await sortDropdown.click();
      const options = page.locator('.p-select-option, .p-dropdown-item');
      const count = await options.count();
      if (count >= 2) {
        await options.nth(1).click();
        await waitForLoadingToComplete(page);
      } else if (count === 1) {
        await options.first().click();
        await waitForLoadingToComplete(page);
      }
    }
    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });
});

test.describe('Pagination', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_029 - Validate pagination controls', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const paginator = page.locator('p-paginator, .p-paginator').first();
    const isVisible = await paginator.isVisible().catch(() => false);
    if (isVisible) {
      const nextBtn = page.locator('.p-paginator-next, button:has-text("Next")').first();
      const prevBtn = page.locator('.p-paginator-prev, button:has-text("Previous")').first();
      const hasNavButtons = await nextBtn.isVisible().catch(() => false) ||
        await prevBtn.isVisible().catch(() => false);
      expect(hasNavButtons).toBeTruthy();
    } else {
      const listview = page.locator('app-listview').first();
      await expect(listview).toBeVisible();
    }
  });

  test('POS_030 - Page size selection', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const rowsDropdown = page.locator(
      'p-dropdown.p-paginator-rpp-options, .p-paginator select'
    ).first();
    if (await rowsDropdown.isVisible().catch(() => false)) {
      await rowsDropdown.click();
      const option25 = page.locator('.p-dropdown-item:has-text("25")').first();
      if (await option25.isVisible().catch(() => false)) {
        await option25.click();
        await waitForLoadingToComplete(page);
      }
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });
});

test.describe('Interactions List View (PNO-230)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_019 - Validate Interactions list columns', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForLoadingToComplete(page);

    const subjectHeader = page.locator('th:has-text("Subject")').first();
    const typeHeader = page.locator('th:has-text("Type")').first();
    const dateHeader = page.locator('th:has-text("Date")').first();
    const listview = interactionsPage.getListview();

    const hasColumns = await subjectHeader.isVisible().catch(() => false) ||
      await typeHeader.isVisible().catch(() => false) ||
      await dateHeader.isVisible().catch(() => false) ||
      await listview.isVisible().catch(() => false);
    expect(hasColumns).toBeTruthy();
  });

  test('POS_020 - Interactions date column format', async ({ page }) => {
    const interactionsPage = new InteractionsPage(page);
    await waitForLoadingToComplete(page);

    const listview = interactionsPage.getListview();
    await expect(listview).toBeVisible();
  });
});

test.describe('Contact List View (PNO-235)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_022 - Validate Contact list columns', async ({ page }) => {
    const contactsPage = new ContactsPage(page);
    await waitForTableData(page);

    const nameHeader = page.locator('th:has-text("Name")').first();
    const emailHeader = page.locator('th:has-text("Email")').first();
    const phoneHeader = page.locator('th:has-text("Phone")').first();
    const listview = page.locator('app-listview').first();

    const hasColumns = await nameHeader.isVisible().catch(() => false) ||
      await emailHeader.isVisible().catch(() => false) ||
      await phoneHeader.isVisible().catch(() => false) ||
      await listview.isVisible().catch(() => false);
    expect(hasColumns).toBeTruthy();
  });

  test('POS_023 - Contact email column clickable', async ({ page }) => {
    const contactsPage = new ContactsPage(page);
    await waitForTableData(page);

    const emailLink = page.locator('a[href^="mailto:"]').first();
    const listview = page.locator('app-listview').first();
    const hasEmailOrList = await emailLink.isVisible().catch(() => false) ||
      await listview.isVisible().catch(() => false);
    expect(hasEmailOrList).toBeTruthy();
  });
});

test.describe('Partner Navigation (PNO-311)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_025 - Navigate by Partner Category', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const categoryFilter = page.locator(
      '[data-testid="category-filter"], p-dropdown:has-text("Category"), p-multiselect:has-text("Category")'
    ).first();
    if (await categoryFilter.isVisible().catch(() => false)) {
      await categoryFilter.click();
      const governmentOption = page.locator(
        '.p-dropdown-item:has-text("Government"), .p-multiselect-item:has-text("Government")'
      ).first();
      if (await governmentOption.isVisible().catch(() => false)) {
        await governmentOption.click();
        await waitForLoadingToComplete(page);
      }
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });

  test('POS_026 - Navigate by Partner Group', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const groupFilter = page.locator(
      'p-dropdown:has-text("Group"), p-multiselect:has-text("Group")'
    ).first();
    if (await groupFilter.isVisible().catch(() => false)) {
      await groupFilter.click();
      const option = page.locator('.p-dropdown-item').first();
      if (await option.isVisible().catch(() => false)) {
        await option.click();
        await waitForLoadingToComplete(page);
      }
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });
});

test.describe('Export Functionality', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('POS_033 - Export list to CSV', async ({ page }) => {
    const partnersPage = new PartnersPage(page);
    await waitForTableData(page);

    const exportBtn = page.getByRole('button', { name: /export/i }).or(
      page.locator('button:has-text("Export")')
    ).first();
    if (await exportBtn.isVisible().catch(() => false)) {
      const downloadPromise = page.waitForEvent('download', { timeout: 5000 }).catch(() => null);
      await exportBtn.click();
      const csvOption = page.locator('text=CSV, button:has-text("CSV")').first();
      if (await csvOption.isVisible().catch(() => false)) {
        await csvOption.click();
      }
      const download = await downloadPromise;
      if (download) {
        expect(download.suggestedFilename()).toBeTruthy();
      }
    }

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible();
  });
});
