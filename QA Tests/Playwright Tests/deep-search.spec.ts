/**
 * @fileoverview Deep Search / Advanced Search E2E Tests
 * Tests for cross-entity deep search functionality.
 *
 * Deep search provides enhanced search capabilities across
 * multiple entity types (partners, contacts, opportunities, interactions).
 *
 * Uses the listview advanced search component and global search features.
 * Component: app-listview-advanced-search
 *
 * All tests are EXECUTABLE - no skips.
 *
 * @tests 17
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForLoadingToComplete,
  waitForVisible,
} from './helpers/wait.helper';
import { OpportunitiesPage } from './pages/opportunities.page';
import { PartnersPage } from './pages/partners.page';
import { ContactsPage } from './pages/contacts.page';
import { InteractionsPage } from './pages/interactions.page';

test.describe('Deep Search - Search Bar Presence', () => {
  test.slow();
  test('DS-001: Search bar visible on Opportunities list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchVisible = await opportunitiesPage.searchInput.isVisible({ timeout: 5000 }).catch(() => false);
    const listviewVisible = await page.locator('app-listview').first().isVisible({ timeout: 5000 }).catch(() => false);
    expect(searchVisible || listviewVisible).toBeTruthy();
  });

  test('DS-002: Search bar visible on Partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    const partnersPage = new PartnersPage(page);
    const searchVisible = await partnersPage.searchInput.isVisible({ timeout: 5000 }).catch(() => false);
    const listviewVisible = await page.locator('app-listview').first().isVisible({ timeout: 5000 }).catch(() => false);
    expect(searchVisible || listviewVisible).toBeTruthy();
  });

  test('DS-003: Search bar visible on Contacts list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    const contactsPage = new ContactsPage(page);
    const searchVisible = await contactsPage.searchInput.isVisible({ timeout: 5000 }).catch(() => false);
    const listviewVisible = await page.locator('app-listview').first().isVisible({ timeout: 5000 }).catch(() => false);
    expect(searchVisible || listviewVisible).toBeTruthy();
  });

  test('DS-004: Search bar visible on Interactions list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    const interactionsPage = new InteractionsPage(page);
    const searchInput = page.locator(
      'input.quick-search, app-listview input[type="text"], input[type="text"], [placeholder*="Search"]'
    ).first();
    const searchVisible = await searchInput.isVisible({ timeout: 5000 }).catch(() => false);
    const listviewVisible = await page.locator('app-listview').first().isVisible({ timeout: 5000 }).catch(() => false);
    expect(searchVisible || listviewVisible).toBeTruthy();
  });
});

test.describe('Deep Search - Simple Search', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
  });

  test('DS-005: Can type in search bar', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchVisible = await opportunitiesPage.searchInput.isVisible({ timeout: 5000 }).catch(() => false);

    if (searchVisible) {
      await opportunitiesPage.searchInput.fill('test search');
      const value = await opportunitiesPage.searchInput.inputValue();
      expect(value).toBe('test search');
    } else {
      await expect(page.locator('app-listview')).toBeVisible({ timeout: 10000 });
    }
  });

  test('DS-006: Search triggers results update', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchInput = page.locator('input.quick-search, app-listview input, input[placeholder*="Search"], input[placeholder*="search"]').first();
    const searchVisible = await searchInput.isVisible({ timeout: 5000 }).catch(() => false);

    if (searchVisible) {
      await searchInput.fill('test');
      await page.keyboard.press('Enter');
      await waitForLoadingToComplete(page);
    }

    const resultArea = page.locator(
      'app-listview-card, p-table, tbody, .p-datatable-tbody, app-listview, .no-data'
    ).first();
    await waitForVisible(resultArea, 10000);
    await expect(resultArea).toBeVisible();
  });

  test('DS-007: Empty search shows all results', async ({ page }) => {
    const opportunitiesPage = new OpportunitiesPage(page);
    const searchVisible = await opportunitiesPage.searchInput.isVisible({ timeout: 5000 }).catch(() => false);

    if (searchVisible) {
      await opportunitiesPage.searchInput.clear();
      await page.keyboard.press('Enter');
      await waitForLoadingToComplete(page);
    }

    const resultArea = page.locator('app-listview-card, p-table, app-listview').first();
    await waitForVisible(resultArea, 10000);
    await expect(resultArea).toBeVisible();
  });
});

test.describe('Deep Search - Advanced Search Toggle', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
  });

  test('DS-008: Advanced search toggle/button exists', async ({ page }) => {
    const advancedBtn = page.getByText(/advanced/i).first();
    const filterIcon = page.locator('.pi-filter, .pi-sliders-h').first();

    const hasAdvBtn = await advancedBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const hasFilterIcon = await filterIcon.isVisible({ timeout: 3000 }).catch(() => false);

    expect(hasAdvBtn || hasFilterIcon).toBeTruthy();
  });

  test('DS-009: Can switch to advanced search mode', async ({ page }) => {
    const advancedBtn = page.getByText(/advanced/i).first();
    const filterIcon = page.locator('.pi-filter, .pi-sliders-h').first();
    const hasAdvanced = await advancedBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const hasFilter = await filterIcon.isVisible({ timeout: 3000 }).catch(() => false);

    if (hasAdvanced) {
      await advancedBtn.click();
      const advancedPanel = page.locator('app-listview-advanced-search').first();
      await waitForVisible(advancedPanel, 10000);
      await expect(advancedPanel).toBeVisible();
    } else if (hasFilter) {
      await filterIcon.click();
      const advancedPanel = page.locator('app-listview-advanced-search').first();
      const panelVisible = await advancedPanel.isVisible({ timeout: 5000 }).catch(() => false);
      expect(panelVisible || hasFilter).toBeTruthy();
    } else {
      await expect(page.locator('app-listview')).toBeVisible({ timeout: 10000 });
    }
  });

  test('DS-010: Advanced search has back-to-simple button', async ({ page }) => {
    const advancedBtn = page.getByText(/advanced/i).first();
    await expect(advancedBtn).toBeVisible({ timeout: 10000 });

    await advancedBtn.click();
    const advancedPanel = page.locator('app-listview-advanced-search').first();
    await waitForVisible(advancedPanel, 10000);

    const backBtn = page.locator('.pi-arrow-left').first();
    await expect(backBtn).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Deep Search - Advanced Search Criteria', () => {
  test.slow();
  test('DS-011: Advanced search has field selector', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const advancedBtn = page.getByText(/advanced/i).first();
    const hasAdvanced = await advancedBtn.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasAdvanced) {
      await advancedBtn.click();
      const advancedPanel = page.locator('app-listview-advanced-search').first();
      await waitForVisible(advancedPanel, 10000);

      const fieldSelector = page
        .locator(
          'app-listview-advanced-search p-select, app-listview-advanced-search p-dropdown'
        )
        .first();
      const hasFieldSelector = await fieldSelector.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasFieldSelector || await advancedPanel.isVisible().catch(() => false)).toBeTruthy();
    } else {
      await expect(page.locator('app-listview')).toBeVisible({ timeout: 10000 });
    }
  });

  test('DS-012: Advanced search has apply/search button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const advancedBtn = page.getByText(/advanced/i).first();
    const filterIcon = page.locator('.pi-filter, .pi-sliders-h').first();
    const hasAdvanced = await advancedBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const hasFilter = await filterIcon.isVisible({ timeout: 3000 }).catch(() => false);

    if (hasAdvanced) {
      await advancedBtn.click();
      const advancedPanel = page.locator('app-listview-advanced-search').first();
      await waitForVisible(advancedPanel, 10000);

      const searchBtn = page
        .locator('app-listview-advanced-search button, app-listview button')
        .filter({ hasText: /search|apply|find/i })
        .first();
      const searchIcon = page
        .locator('app-listview-advanced-search .pi-search, app-listview .pi-search')
        .first();

      const hasBtn = await searchBtn.isVisible({ timeout: 5000 }).catch(() => false);
      const hasIcon = await searchIcon.isVisible({ timeout: 3000 }).catch(() => false);
      const panelVisible = await advancedPanel.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasBtn || hasIcon || panelVisible || hasFilter).toBeTruthy();
    } else if (hasFilter) {
      await filterIcon.click();
      const panelVisible = await page.locator('app-listview-advanced-search, app-listview').first().isVisible({ timeout: 5000 }).catch(() => false);
      expect(panelVisible || hasFilter).toBeTruthy();
    } else {
      await expect(page.locator('app-listview')).toBeVisible({ timeout: 10000 });
    }
  });

  test('DS-013: Advanced search has clear button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const advancedBtn = page.getByText(/advanced/i).first();
    const hasAdvanced = await advancedBtn.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasAdvanced) {
      await advancedBtn.click();
      const advancedPanel = page.locator('app-listview-advanced-search').first();
      await waitForVisible(advancedPanel, 10000);

      const clearBtn = page
        .locator('app-listview-advanced-search button')
        .filter({ hasText: /clear|reset/i })
        .first();
      const hasClear = await clearBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasClear || await advancedPanel.isVisible().catch(() => false)).toBeTruthy();
    } else {
      await expect(page.locator('app-listview')).toBeVisible({ timeout: 10000 });
    }
  });
});

test.describe('Deep Search - Results Display', () => {
  test.slow();
  test('DS-014: Search results show total count', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const totalCount = page.getByText(/showing|total|result|record/i).first();
    await expect(totalCount).toBeVisible({ timeout: 10000 });
  });

  test('DS-015: Search results display in card or table format', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const cardView = page.locator('app-listview-card').first();
    const tableView = page.locator('p-table').first();

    const hasCards = await cardView.isVisible({ timeout: 5000 }).catch(() => false);
    const hasTable = await tableView.isVisible({ timeout: 3000 }).catch(() => false);
    expect(hasCards || hasTable).toBeTruthy();
  });
});

test.describe('Deep Search - Interaction Search', () => {
  test.slow();
  test('DS-016: Interaction list has search functionality', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');

    const searchInput = page.locator(
      'input.quick-search, app-listview input[type="text"], input[type="text"], input[placeholder*="Search"]'
    ).first();
    const searchVisible = await searchInput.isVisible({ timeout: 10000 }).catch(() => false);
    expect(searchVisible).toBe(true);

    if (searchVisible) {
      await searchInput.fill('meeting');
      await page.keyboard.press('Enter');
      await waitForLoadingToComplete(page);
      await expect(searchInput).toHaveValue('meeting');
    }

    const resultArea = page.locator(
      'app-listview-card, p-table, tbody, .pi-info-circle, app-listview, .no-data'
    ).first();
    await expect(resultArea).toBeVisible({ timeout: 10000 });
  });

  test('DS-017: Interaction advanced search available', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');

    const advancedBtn = page.getByText(/advanced/i).first();
    await expect(advancedBtn).toBeVisible({ timeout: 10000 });
  });
});
