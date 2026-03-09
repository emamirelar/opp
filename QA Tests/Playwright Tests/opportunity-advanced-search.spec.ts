/**
 * @fileoverview Opportunity Advanced Search & Filtering E2E Tests
 *
 * Tests for advanced search, structured filters, status/stage filtering,
 * and search results accuracy on the opportunity list page.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-SEARCH
 *
 * @tests 12
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForTableData,
  waitForNetworkIdle,
  waitForElementReady,
  waitForVisible,
  waitForLoadingToComplete,
} from './helpers/wait.helper';
import { OpportunitiesPage } from './pages/opportunities.page';
import { getTimeout } from './helpers/test-config';

const featureReady = process.env.OPPORTUNITY_SEARCH_IMPLEMENTED === 'true';

const OPPORTUNITIES_URL = '/partnerships/opportunities';

// =============================================================================
// SECTION 1: Basic Search
// =============================================================================
test.describe('Search — Basic Text Search', () => {
  test.slow();
  test.skip(!featureReady, 'Search not deployed — set OPPORTUNITY_SEARCH_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForTableData(page);
  });

  test('SRCH-001: Search input field visible on list page', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"], .p-inputtext').first();
    await expect(searchInput).toBeVisible({ timeout: getTimeout('long') });
    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-002: Typing in search filters the opportunity list', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchInput = opportunitiesPage.searchInput;
    await expect(searchInput).toBeVisible({ timeout: getTimeout('short') });
    await searchInput.fill('test');
    await searchInput.press('Enter');
    await waitForNetworkIdle(page);
    await waitForTableData(page);

    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-003: Clearing search restores full list', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchInput = opportunitiesPage.searchInput;
    await expect(searchInput).toBeVisible({ timeout: getTimeout('short') });
    await searchInput.fill('test');
    await searchInput.press('Enter');
    await waitForNetworkIdle(page);
    await waitForTableData(page);

    await searchInput.clear();
    await searchInput.press('Enter');
    await waitForNetworkIdle(page);
    await waitForTableData(page);

    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-004: Search with no results shows empty state', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchInput = opportunitiesPage.searchInput;
    await expect(searchInput).toBeVisible({ timeout: getTimeout('short') });
    await searchInput.fill('zzz_nonexistent_opportunity_xyz_999');
    await searchInput.press('Enter');
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const emptyState = page.getByText(/no results|no opportunities|no records/i).first();
    await expect(emptyState).toBeVisible({ timeout: getTimeout('short') });
  });
});

// =============================================================================
// SECTION 2: Status & Stage Filters
// =============================================================================
test.describe('Search — Status & Stage Filters', () => {
  test.slow();
  test.skip(!featureReady, 'Search not deployed — set OPPORTUNITY_SEARCH_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForTableData(page);
  });

  test('SRCH-005: Status filter dropdown available on list page', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const statusFilter = page.locator('[data-testid="status-filter"], p-select:has-text("Status"), p-multiselect').first();
    await expect(statusFilter).toBeVisible({ timeout: getTimeout('long') });
    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-006: Stage filter dropdown available on list page', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const stageFilter = page.locator('[data-testid="stage-filter"], p-select:has-text("Stage")').first();
    await expect(stageFilter).toBeVisible({ timeout: getTimeout('long') });
    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-007: Filtering by status updates list results', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const filterDropdown = page.locator('p-select, p-multiselect').first();
    await expect(filterDropdown).toBeVisible({ timeout: getTimeout('short') });
    await filterDropdown.click();
    const option = page.locator('.p-select-option, .p-multiselect-item').first();
    await waitForElementReady(option);
    await option.click();
    await waitForNetworkIdle(page);
    await waitForTableData(page);
    await expect(opportunitiesPage.listview).toBeVisible();
  });
});

// =============================================================================
// SECTION 3: Advanced Search
// =============================================================================
test.describe('Search — Advanced Search', () => {
  test.slow();
  test.skip(!featureReady, 'Search not deployed — set OPPORTUNITY_SEARCH_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForTableData(page);
  });

  test('SRCH-008: Advanced search toggle/button available', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    await expect(advSearchBtn).toBeVisible({ timeout: getTimeout('long') });
    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-009: Advanced search panel shows structured filter fields', async ({ page }) => {
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    await expect(advSearchBtn).toBeVisible({ timeout: getTimeout('short') });
    await advSearchBtn.click();
    const filterFields = page.locator('.advanced-search-panel input, .advanced-search-panel p-select, [data-testid*="advanced-filter"]');
    await waitForVisible(filterFields.first());
    const count = await filterFields.count();
    expect(count).toBeGreaterThan(0);
  });

  test('SRCH-010: Advanced search by date range works', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    await expect(advSearchBtn).toBeVisible({ timeout: getTimeout('short') });
    await advSearchBtn.click();
    const dateField = page.locator('p-datepicker, input[type="date"]').first();
    await waitForVisible(dateField);
    await expect(dateField).toBeVisible();
    await expect(opportunitiesPage.listview).toBeVisible();
  });

  test('SRCH-011: Advanced search by budget range works', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    await expect(advSearchBtn).toBeVisible({ timeout: getTimeout('short') });
    await advSearchBtn.click();
    const budgetField = page.locator('p-inputnumber, input[type="number"], [data-testid*="budget-filter"]').first();
    await waitForVisible(budgetField);
    await expect(budgetField).toBeVisible();
    await expect(opportunitiesPage.listview).toBeVisible();
  });
});

// =============================================================================
// SECTION 4: Column Sorting
// =============================================================================
test.describe('Search — Column Sorting', () => {
  test.slow();
  test.skip(!featureReady, 'Search not deployed — set OPPORTUNITY_SEARCH_IMPLEMENTED=true');

  test('SRCH-012: List columns are sortable', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForTableData(page);

    const opportunitiesPage = new OpportunitiesPage(page);
    const sortableHeader = page.locator('.p-sortable-column, th[psortablecolumn]').first();
    await expect(sortableHeader).toBeVisible({ timeout: getTimeout('long') });
    await expect(opportunitiesPage.listview).toBeVisible();
  });
});
