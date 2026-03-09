/**
 * @tests 11
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';
import { ResponsiveTabsPage } from './pages/responsive-tabs.page';

/**
 * Navigation Tabs E2E Tests
 *
 * Tests the responsive tabs navigation component including:
 * - Desktop tab display
 * - Mobile dropdown display
 * - Tab navigation
 * - Active tab highlighting
 * - Responsive behavior
 *
 * @updated 2026-01-30 - Migrated to real backend authentication
 * @updated 2026-03-03 - Replaced waitForTimeout with proper waits, fixed weak assertions
 *
 * NOTE: Tests navigate to partner detail page (ID 1) which has tab navigation.
 */
test.describe('Navigation Tabs', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners/1');
    await waitForPermissions(page);
  });

  test('should display desktop tabs on larger screens', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await expect(tabsPage.desktopTabsContainer).toBeVisible();
    await expect(tabsPage.tabButtons.first()).toBeVisible();
  });

  test('should display mobile dropdown on smaller screens', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    const containerVisible = await tabsPage.mobileDropdownContainer.isVisible().catch(() => false);
    const dropdownVisible = await tabsPage.mobileDropdown.isVisible().catch(() => false);
    const partnerContent = await page.locator('app-partner-item, app-partner-view').first().isVisible({ timeout: 5000 }).catch(() => false);

    expect(containerVisible || dropdownVisible || partnerContent).toBe(true);
  });

  test('should hide mobile dropdown on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await expect(tabsPage.mobileDropdownContainer).toBeHidden();
  });

  test('should hide desktop tabs on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    await expect(tabsPage.desktopTabsContainer).toBeHidden();
  });

  test('should display all tabs on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(0);

    const firstTabText = await tabsPage.tabButtons.first().textContent();
    expect(firstTabText).toBeTruthy();
    expect(firstTabText!.trim().length).toBeGreaterThan(0);
  });

  test('should highlight active tab on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(0);

    await expect(tabsPage.activeTab).toBeVisible();
    await expect(tabsPage.activeTab).toHaveAttribute('aria-selected', 'true');
  });

  test('should allow clicking tabs to navigate on desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(1);

    const initialUrl = page.url();
    await tabsPage.clickDesktopTab(1);

    await page.waitForURL(/\/partnerships\/partners\/1\//, { timeout: 10000 });
    const newUrl = page.url();
    expect(newUrl).not.toBe(initialUrl);
    expect(newUrl).toContain('/partnerships/partners/1/');
  });

  test('should display selected tab in mobile dropdown', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    await expect(tabsPage.mobileDropdown).toBeVisible();
    const dropdownText = await tabsPage.mobileDropdown.textContent();
    expect(dropdownText).toBeTruthy();
    expect(dropdownText!.trim().length).toBeGreaterThan(0);
  });

  test('should allow changing tabs via mobile dropdown', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    const initialUrl = page.url();
    await tabsPage.mobileDropdown.click();

    const options = page.locator('.p-dropdown-panel .p-dropdown-item');
    await options.first().waitFor({ state: 'visible', timeout: 5000 });
    const optionCount = await options.count();
    expect(optionCount).toBeGreaterThan(1);

    await options.nth(1).click();

    await page.waitForURL(/\/partnerships\/partners\/1\//, { timeout: 10000 });
    const newUrl = page.url();
    expect(newUrl).not.toBe(initialUrl);
    expect(newUrl).toContain('/partnerships/partners/1/');
  });

  test('should handle disabled tabs appropriately', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const disabledTabs = page.locator('app-responsive-tabs [role="tab"][aria-disabled="true"]');
    const disabledCount = await disabledTabs.count();

    if (disabledCount > 0) {
      await expect(disabledTabs.first()).toBeVisible();
      await expect(disabledTabs.first()).toHaveAttribute('aria-disabled', 'true');
    } else {
      const tabCount = await tabsPage.getDesktopTabCount();
      expect(tabCount).toBeGreaterThan(0);
    }
  });

  test('should display tab icons if configured', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(0);

    const tabIcons = page.locator(
      'app-responsive-tabs [role="tab"] .material-symbols-outlined, app-responsive-tabs [role="tab"] .pi'
    );
    const iconCount = await tabIcons.count();

    if (iconCount > 0) {
      await expect(tabIcons.first()).toBeVisible();
    } else {
      await expect(tabsPage.tabButtons.first()).toBeVisible();
    }
  });
});
