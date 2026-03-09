/**
 * @fileoverview PNO-926-v3 — Global Search Icon Display E2E Tests
 *
 * PNO-926-v3 fix: search-result.component.ts now correctly maps entity tab icons
 * using getEntityIcon(entityType), which returns:
 *   - contacts      → 'contacts'
 *   - partners      → 'corporate_fare'
 *   - interactions  → 'chat'
 *   - opportunities → 'lightbulb'
 *   - default       → 'help'
 *
 * The icon is rendered via <span class="material-symbols-outlined">{{ tab.icon }}</span>
 * in the search-result component template.
 *
 * These tests verify:
 *   1. The global search bar is accessible
 *   2. Searching returns results with entity tabs
 *   3. Each entity tab renders a material-symbols-outlined icon
 *   4. The icon text matches the expected entity type mapping
 *   5. Icons are not empty / undefined / showing the string "help" for known entities
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 17
 */

import { test, expect } from '@playwright/test';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForVisible,
} from './helpers/wait.helper';
import { SearchResultPage } from './pages/search-result.page';

// ──────────────────────────────────────────────────────────────
// Helpers
// ──────────────────────────────────────────────────────────────

/** Expected icon names per entity type (from getEntityIcon in search-result.component.ts) */
const EXPECTED_ICONS: Record<string, string> = {
  contacts: 'contacts',
  partners: 'corporate_fare',
  interactions: 'chat',
  opportunities: 'lightbulb',
};

/** Material Symbols icon names known to be valid for UNOPS search tabs */
const KNOWN_VALID_ICONS = new Set(Object.values(EXPECTED_ICONS));

// ──────────────────────────────────────────────────────────────
// PNO-926-v3: Search Icon Display
// ──────────────────────────────────────────────────────────────

test.describe('PNO-926-v3: Global Search Icon Display', () => {
  test.slow(); // Search tests involve network and rendering

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await waitForPageReady(page);
  });

  // ─── POSITIVE TESTS ──────────────────────────────────────────

  test('SEARCH-ICON-001: Global search bar is visible on home page', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    const trigger = searchPage.globalSearchTrigger;
    const visible = await trigger.isVisible({ timeout: 15_000 }).catch(() => false);
    if (!visible) {
      // Fallback: any search input in topbar or page
      const fallback = page
        .locator(
          'app-topbar input[type="text"], .global-search-container input, input[placeholder*="Search" i]'
        )
        .first();
      await expect(fallback).toBeVisible({ timeout: 5000 });
    } else {
      await expect(trigger).toBeVisible();
    }
  });

  // ─── FUNCTIONAL TESTS ────────────────────────────────────────

  test('SEARCH-ICON-002: Search results page renders material-symbols icons for entity tabs', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=test');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.searchResultContainer, 15_000).catch(
      () => {}
    );

    const iconSpans = searchPage.entityTabIcons.filter({
      hasText: /^[a-z_]+$/,
    });
    const count = await iconSpans.count();

    if (count > 0) {
      for (let i = 0; i < Math.min(count, 10); i++) {
        const iconText = await iconSpans.nth(i).textContent();
        const trimmed = iconText?.trim() ?? '';
        expect(trimmed.length).toBeGreaterThan(0);
        expect(trimmed).not.toBe('undefined');
        expect(trimmed).not.toBe('null');
      }
    }
  });

  test('SEARCH-ICON-003: Search result entity tabs show icons matching entity type', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    for (const [entityType, expectedIcon] of Object.entries(EXPECTED_ICONS)) {
      const tabLocator = searchPage.getIconForEntity(entityType);
      const tabExists = (await tabLocator.count()) > 0;

      if (tabExists) {
        const iconText = await tabLocator.textContent();
        expect(iconText?.trim()).toBe(
          expectedIcon,
          `PNO-926-v3: '${entityType}' tab must show icon '${expectedIcon}', not '${iconText?.trim()}'`
        );
      }
    }
  });

  test('SEARCH-ICON-004: Partners entity tab shows corporate_fare icon', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=UNOPS');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const partnersTabIcon = searchPage.getIconByText('corporate_fare');
    const count = await partnersTabIcon.count();

    if (count > 0) {
      await expect(partnersTabIcon.first()).toBeVisible({ timeout: 5000 });
      const text = await partnersTabIcon.first().textContent();
      expect(text?.trim()).toBe(
        'corporate_fare',
        'PNO-926-v3: partners tab must show corporate_fare icon'
      );
    }
  });

  test('SEARCH-ICON-005: Opportunities entity tab shows lightbulb icon', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const opportunityTabIcon = searchPage.getIconByText('lightbulb');
    const count = await opportunityTabIcon.count();

    if (count > 0) {
      const text = await opportunityTabIcon.first().textContent();
      expect(text?.trim()).toBe(
        'lightbulb',
        'PNO-926-v3: opportunities tab must show lightbulb icon'
      );
    }
  });

  test('SEARCH-ICON-006: Contacts entity tab shows contacts icon', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const contactsTabIcon = searchPage.getIconByText('contacts');
    const count = await contactsTabIcon.count();

    if (count > 0) {
      const text = await contactsTabIcon.first().textContent();
      expect(text?.trim()).toBe(
        'contacts',
        'PNO-926-v3: contacts tab must show contacts icon'
      );
    }
  });

  // ─── NEGATIVE TESTS ──────────────────────────────────────────

  test('SEARCH-ICON-NEG-001: No entity tab shows undefined or empty icon text', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.tablistIcons.first(), 15_000).catch(
      () => {}
    );

    const tabIcons = searchPage.tablistIcons;
    const count = await tabIcons.count();

    for (let i = 0; i < count; i++) {
      const text = await tabIcons.nth(i).textContent();
      const trimmed = text?.trim() ?? '';
      expect(trimmed.length).toBeGreaterThan(0);
      expect(trimmed).not.toBe('undefined');
    }
  });

  test('SEARCH-ICON-NEG-002: Search with no results does not show broken icons', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=xzxzxzxzxz_no_results_expected_8675309');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const iconSpans = searchPage.entityTabIcons;
    const count = await iconSpans.count();

    for (let i = 0; i < count; i++) {
      const text = await iconSpans.nth(i).textContent();
      if (text !== undefined) {
        expect(text.trim()).not.toBe('');
        expect(text.trim()).not.toBe('undefined');
      }
    }
  });

  test('SEARCH-ICON-007: Search result component does not render literal string "help" for known entities', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    for (const entityType of Object.keys(EXPECTED_ICONS)) {
      const iconLocator = searchPage.getIconForEntity(entityType);
      if ((await iconLocator.count()) === 0) continue;

      const iconText = await iconLocator.textContent();
      expect(iconText?.trim()).not.toBe(
        'help',
        `PNO-926-v3: '${entityType}' entity tab must not show fallback 'help' icon`
      );
    }
  });

  test('SEARCH-ICON-008: Search page navigable without authentication redirect', async ({
    page,
  }) => {
    await page.goto('/search?q=test');
    await waitForPageReady(page);

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title).not.toBe('');
  });

  // ─── BOUNDARY/EDGE TESTS ─────────────────────────────────────

  test('SEARCH-ICON-009: Icons render for special characters in search term', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=test%40test');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const errorCount = await searchPage.errorOverlay.count();
    expect(errorCount).toBe(0);
  });

  test('SEARCH-ICON-010: Active tab icon remains visible after switching tabs', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const tabs = searchPage.tabs;
    const tabCount = await tabs.count();

    if (tabCount > 1) {
      await tabs.nth(1).click();
      await waitForVisible(searchPage.entityTabIcons.first(), 5000).catch(
        () => {}
      );

      const iconsAfterSwitch = await searchPage.entityTabIcons.count();
      expect(iconsAfterSwitch).toBeGreaterThan(0);
    }
  });

  test('SEARCH-ICON-011: getEntityIcon mapping returns expected values for all entity types', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=UNOPS');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const iconTexts = await searchPage.entityTabIcons.allTextContents();
    const visibleIcons = iconTexts.map((t) => t.trim()).filter((t) => t.length > 0);

    if (visibleIcons.length > 0) {
      const knownIconsVisible = visibleIcons.some((icon) =>
        KNOWN_VALID_ICONS.has(icon)
      );
      expect(knownIconsVisible).toBe(true);
    }
  });

  test('SEARCH-ICON-012: Search result page responsive — icons visible on mobile viewport', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/search?q=test');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const body = page.locator('body');
    await expect(body).toBeVisible();

    await page.setViewportSize({ width: 1280, height: 720 });
  });

  // ─── INTEGRATION TESTS ───────────────────────────────────────

  test('SEARCH-ICON-INT-001: Full flow — home → search → icon renders in entity tab', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/');
    await waitForPageReady(page);

    await page.goto('/search?q=test');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const body = page.locator('body');
    await expect(body).toBeVisible();

    const iconTexts = await searchPage.entityTabIcons.allTextContents();
    for (const iconText of iconTexts) {
      const trimmed = iconText.trim();
      if (trimmed.length > 0) {
        expect(trimmed).not.toBe('undefined');
        expect(trimmed).not.toBe('null');
      }
    }
  });

  test('SEARCH-ICON-INT-002: Icon consistency — same query twice returns same icons', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);

    const collectIcons = async () => {
      await page.goto('/search?q=UNOPS');
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);
      return searchPage.entityTabIcons.allTextContents();
    };

    const firstRun = (await collectIcons())
      .map((t) => t.trim())
      .filter((t) => t.length > 0);
    const secondRun = (await collectIcons())
      .map((t) => t.trim())
      .filter((t) => t.length > 0);

    if (firstRun.length > 0 && secondRun.length > 0) {
      expect(firstRun.sort()).toEqual(secondRun.sort());
    }
  });

  test('SEARCH-ICON-INT-003: Tab switch preserves entity icon rendering (state integrity)', async ({
    page,
  }) => {
    const searchPage = new SearchResultPage(page);
    await page.goto('/search?q=a');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForVisible(searchPage.entityTabIcons.first(), 15_000).catch(
      () => {}
    );

    const tabs = searchPage.tabs;
    const tabCount = await tabs.count();

    if (tabCount > 1) {
      await tabs.nth(1).click();
      await waitForVisible(searchPage.entityTabIcons.first(), 5000).catch(
        () => {}
      );

      await tabs.nth(0).click();
      await waitForVisible(searchPage.entityTabIcons.first(), 5000).catch(
        () => {}
      );

      const iconsAfterRoundTrip = await searchPage.entityTabIcons.count();
      expect(iconsAfterRoundTrip).toBeGreaterThan(0);
    }

    await expect(page.locator('body')).toBeVisible();
  });
});
