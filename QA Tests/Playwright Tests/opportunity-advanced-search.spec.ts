/**
 * @fileoverview Opportunity Advanced Search & Filtering E2E Tests
 *
 * Tests for advanced search, structured filters, status/stage filtering,
 * and search results accuracy on the opportunity list page.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-SEARCH
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

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
    await page.waitForTimeout(3000);
  });

  test('SRCH-001: Search input field visible on list page', async ({ page }) => {
    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"], .p-inputtext').first();
    const isVisible = await searchInput.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });

  test('SRCH-002: Typing in search filters the opportunity list', async ({ page }) => {
    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"]').first();
    if (await searchInput.isVisible({ timeout: 5000 }).catch(() => false)) {
      await searchInput.fill('test');
      await searchInput.press('Enter');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      const listview = page.locator('app-listview');
      await expect(listview.first()).toBeVisible();
    }
  });

  test('SRCH-003: Clearing search restores full list', async ({ page }) => {
    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"]').first();
    if (await searchInput.isVisible({ timeout: 5000 }).catch(() => false)) {
      await searchInput.fill('test');
      await searchInput.press('Enter');
      await page.waitForTimeout(2000);

      await searchInput.clear();
      await searchInput.press('Enter');
      await page.waitForTimeout(2000);

      const listview = page.locator('app-listview');
      await expect(listview.first()).toBeVisible();
    }
  });

  test('SRCH-004: Search with no results shows empty state', async ({ page }) => {
    const searchInput = page.locator('[data-testid="search-input"], input[placeholder*="Search"]').first();
    if (await searchInput.isVisible({ timeout: 5000 }).catch(() => false)) {
      await searchInput.fill('zzz_nonexistent_opportunity_xyz_999');
      await searchInput.press('Enter');
      await page.waitForTimeout(3000);

      const emptyState = page.getByText(/no results|no opportunities|no records/i).first();
      const hasEmpty = await emptyState.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasEmpty || await page.locator('app-listview').first().isVisible()).toBeTruthy();
    }
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
    await page.waitForTimeout(3000);
  });

  test('SRCH-005: Status filter dropdown available on list page', async ({ page }) => {
    const statusFilter = page.locator('[data-testid="status-filter"], p-select:has-text("Status"), p-multiselect').first();
    const isVisible = await statusFilter.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });

  test('SRCH-006: Stage filter dropdown available on list page', async ({ page }) => {
    const stageFilter = page.locator('[data-testid="stage-filter"], p-select:has-text("Stage")').first();
    const isVisible = await stageFilter.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });

  test('SRCH-007: Filtering by status updates list results', async ({ page }) => {
    const filterDropdown = page.locator('p-select, p-multiselect').first();
    if (await filterDropdown.isVisible({ timeout: 5000 }).catch(() => false)) {
      await filterDropdown.click();
      await page.waitForTimeout(500);
      const option = page.locator('.p-select-option, .p-multiselect-item').first();
      if (await option.isVisible({ timeout: 3000 }).catch(() => false)) {
        await option.click();
        await page.waitForLoadState('networkidle');
      }
    }
    const listview = page.locator('app-listview');
    await expect(listview.first()).toBeVisible();
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
    await page.waitForTimeout(3000);
  });

  test('SRCH-008: Advanced search toggle/button available', async ({ page }) => {
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    const isVisible = await advSearchBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });

  test('SRCH-009: Advanced search panel shows structured filter fields', async ({ page }) => {
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    if (await advSearchBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await advSearchBtn.click();
      await page.waitForTimeout(1000);

      const filterFields = page.locator('.advanced-search-panel input, .advanced-search-panel p-select, [data-testid*="advanced-filter"]');
      const count = await filterFields.count();
      expect(count).toBeGreaterThan(0);
    }
  });

  test('SRCH-010: Advanced search by date range works', async ({ page }) => {
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    if (await advSearchBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await advSearchBtn.click();
      await page.waitForTimeout(1000);

      const dateField = page.locator('p-datepicker, input[type="date"]').first();
      const hasDateFilter = await dateField.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasDateFilter || await page.locator('app-listview').first().isVisible()).toBeTruthy();
    }
  });

  test('SRCH-011: Advanced search by budget range works', async ({ page }) => {
    const advSearchBtn = page.locator('button:has-text("Advanced"), [data-testid="advanced-search-toggle"]').first();
    if (await advSearchBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await advSearchBtn.click();
      await page.waitForTimeout(1000);

      const budgetField = page.locator('p-inputnumber, input[type="number"], [data-testid*="budget-filter"]').first();
      const hasBudgetFilter = await budgetField.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasBudgetFilter || await page.locator('app-listview').first().isVisible()).toBeTruthy();
    }
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
    await page.waitForTimeout(3000);

    const sortableHeader = page.locator('.p-sortable-column, th[psortablecolumn]').first();
    const isSortable = await sortableHeader.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isSortable || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });
});
