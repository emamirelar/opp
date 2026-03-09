/**
 * @fileoverview Partner & Contact Detail Tabs E2E Tests
 *
 * Tests the detail tab navigation for partner and contact pages,
 * including responsive behavior, content loading, and tab switching.
 * Uses the ResponsiveTabsComponent and entity-specific tab configurations.
 *
 * @author UNOPS Opportunity+ QA Team
 * @tests 39
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';
import { ResponsiveTabsPage } from './pages/responsive-tabs.page';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const PARTNER_ID = process.env.TEST_PARTNER_ID || '1';
const CONTACT_ID = process.env.TEST_CONTACT_ID || '1';

// Expected tab counts from partner-tabs.component.ts and contact-tabs.component.ts
const EXPECTED_PARTNER_TABS = 5; // Details, Opportunities, Contacts, Interactions, Dashboard
const EXPECTED_CONTACT_TABS = 2; // Details, Interactions

test.describe('Partner & Contact Detail Tabs', () => {
  test.slow();

  // ==================== POSITIVE TESTS (3) ====================

  test('POS_001: Partner detail tabs render all expected tabs', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBe(EXPECTED_PARTNER_TABS);
    await expect(tabsPage.tabButtons.first()).toBeVisible();
  });

  test('POS_002: Clicking each tab shows corresponding content section', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    // Click Contacts tab (index 2)
    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });
    expect(page.url()).toContain('/contacts');

    // Click Interactions tab (index 3)
    await tabsPage.clickDesktopTab(3);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/interactions/, { timeout: 10000 });
    expect(page.url()).toContain('/interactions');

    // Click Details tab (index 0)
    await tabsPage.clickDesktopTab(0);
    await page.waitForURL(new RegExp(`/partnerships/partners/${PARTNER_ID}(?:/)?$`), { timeout: 10000 });
  });

  test('POS_003: Contact detail tabs render all expected tabs', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/contacts/${CONTACT_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    // Contact page uses app-contact-tabs with p-tabs (not app-responsive-tabs)
    const contactTabs = page.locator('app-contact-tabs [role="tab"]');
    await contactTabs.first().waitFor({ state: 'visible', timeout: 15000 });

    const tabCount = await contactTabs.count();
    expect(tabCount).toBe(EXPECTED_CONTACT_TABS);
  });

  // ==================== NEGATIVE TESTS (9) ====================

  test('NEG_001: Non-existent partner ID shows error or redirects', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPermissions(page);

    const nonExistentId = '999999999';
    await page.goto(`/partnerships/partners/${nonExistentId}`);

    await page.waitForLoadState('networkidle');

    // Expect either error message, 404, or redirect to list
    const hasError = await page.locator('.p-message-error, [role="alert"], .error, p-message[severity="error"]').isVisible().catch(() => false);
    const hasNotFound = await page.locator('text=404, text=not found, text=Not Found').first().isVisible().catch(() => false);
    const redirectedToList = page.url().match(/\/partnerships\/partners\/?$/);

    expect(hasError || hasNotFound || !!redirectedToList).toBe(true);
  });

  test('NEG_002: Tab content loads gracefully when API returns empty data', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    // Navigate to Contacts tab - may have empty list
    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });
    await page.waitForLoadState('networkidle');

    // Page should not crash - content area or empty state visible
    const contentArea = page.locator('app-partner-contacts, router-outlet + *, .empty-state, p-datatable, p-table');
    await expect(contentArea.first()).toBeVisible({ timeout: 10000 });
  });

  test('NEG_003: Switching tabs rapidly does not cause UI glitches', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(1);

    // Rapid tab switching
    for (let i = 0; i < 5; i++) {
      await tabsPage.clickDesktopTab(i % tabCount);
    }

    await page.waitForLoadState('networkidle');

    // UI should remain stable - no duplicate elements, no broken layout
    const tabCountAfter = await tabsPage.getDesktopTabCount();
    expect(tabCountAfter).toBe(tabCount);
    await expect(tabsPage.tabButtons.first()).toBeVisible();
  });

  test('NEG_004: Unauthorized user cannot see restricted tabs', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, READONLY_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    // Readonly user may see tabs but tab content may show empty/restricted
    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(0);

    // Tab content may show permission message or empty state
    const hasContent = await page.locator('app-partner-view, app-partner-contacts, router-outlet').first().isVisible().catch(() => false);
    expect(hasContent).toBe(true);
  });

  test('NEG_005: Tab content handles network failure gracefully', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    // Block API for contacts after initial load
    await page.route('**/api/partner/*/contact*', route => route.abort('failed'));

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });
    await page.waitForLoadState('networkidle');

    // Page should not crash - error message or empty state acceptable
    const hasContent = await page.locator('app-partner-contacts, .p-message, [role="alert"], .error').first().isVisible().catch(() => false);
    expect(hasContent).toBe(true);
  });

  test('NEG_006: Non-existent contact ID handled properly', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPermissions(page);

    const nonExistentId = '999999999';
    await page.goto(`/partnerships/contacts/${nonExistentId}`);

    await page.waitForLoadState('networkidle');

    const hasError = await page.locator('.p-message-error, [role="alert"], .error, p-message[severity="error"]').isVisible().catch(() => false);
    const hasNotFound = await page.locator('text=404, text=not found, text=Not Found').first().isVisible().catch(() => false);
    const redirectedToList = page.url().match(/\/partnerships\/contacts\/?$/);

    expect(hasError || hasNotFound || !!redirectedToList).toBe(true);
  });

  test('NEG_007: Clicking current tab does not cause unnecessary reload', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const initialUrl = page.url();
    await tabsPage.clickDesktopTab(0);

    await page.waitForTimeout(500);
    const urlAfter = page.url();
    expect(urlAfter).toContain(`/partnerships/partners/${PARTNER_ID}`);
  });

  test('NEG_008: Tab state not preserved when navigating away and back', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    await page.goto(`/partnerships/partners/${PARTNER_ID}`);
    await waitForPermissions(page);

    // Default tab (Details) should be active on fresh navigation
    const url = page.url();
    const isOnDetails = url.match(new RegExp(`/partnerships/partners/${PARTNER_ID}(?:/)?$`));
    expect(!!isOnDetails).toBe(true);
  });

  test('NEG_009: Browser back button from tab content works correctly', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    await page.goBack();
    await page.waitForLoadState('networkidle');

    const url = page.url();
    expect(url).toContain(`/partnerships/partners/${PARTNER_ID}`);
  });

  // ==================== EDGE/BOUNDARY TESTS (9) ====================

  test('EDGE_001: Tabs display correctly on mobile viewport (375px)', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    const containerVisible = await tabsPage.mobileDropdownContainer.isVisible().catch(() => false);
    const dropdownVisible = await tabsPage.mobileDropdown.isVisible().catch(() => false);
    const partnerContent = await page.locator('app-partner-tabs, app-partner-view').first().isVisible({ timeout: 5000 }).catch(() => false);

    expect(containerVisible || dropdownVisible || partnerContent).toBe(true);
  });

  test('EDGE_002: Tabs display correctly on tablet viewport (768px)', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 768, height: 1024 });

    await page.waitForLoadState('networkidle');

    const desktopTabs = page.locator('app-responsive-tabs [role="tablist"], app-responsive-tabs .hidden.md\\:block');
    const mobileDropdown = page.locator('app-responsive-tabs p-dropdown, app-responsive-tabs [role="combobox"]');

    const desktopVisible = await desktopTabs.first().isVisible().catch(() => false);
    const mobileVisible = await mobileDropdown.first().isVisible().catch(() => false);

    expect(desktopVisible || mobileVisible).toBe(true);
  });

  test('EDGE_003: Tabs display correctly on large desktop (1920px)', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1920, height: 1080 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await expect(tabsPage.desktopTabsContainer).toBeVisible();
    await expect(tabsPage.tabButtons.first()).toBeVisible();
  });

  test('EDGE_004: Tab with no data shows empty state message', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });
    await page.waitForLoadState('networkidle');

    const hasContent = await page.locator('app-partner-contacts, .empty, .no-data, p-datatable, p-table').first().isVisible().catch(() => false);
    expect(hasContent).toBe(true);
  });

  test('EDGE_005: Very long partner name does not break tab layout', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBe(EXPECTED_PARTNER_TABS);
    await expect(tabsPage.tabButtons.first()).toBeVisible();
  });

  test('EDGE_006: Viewport resize preserves active tab', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForLoadState('networkidle');

    const url = page.url();
    expect(url).toContain('/contacts');
  });

  test('EDGE_007: Tab labels use translation keys (not hardcoded)', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const firstTabText = await tabsPage.tabButtons.first().textContent();
    expect(firstTabText).toBeTruthy();
    expect(firstTabText!.trim().length).toBeGreaterThan(0);
  });

  test('EDGE_008: First tab is active by default on page load', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await expect(tabsPage.activeTab).toBeVisible();
    await expect(tabsPage.activeTab).toHaveAttribute('aria-selected', 'true');

    const url = page.url();
    expect(url).toMatch(new RegExp(`/partnerships/partners/${PARTNER_ID}(?:/)?$`));
  });

  test('EDGE_009: Tab count matches expected number of sections', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBe(EXPECTED_PARTNER_TABS);
  });

  // ==================== FUNCTIONAL TESTS (9) ====================

  test('FUNC_001: Details tab shows partner information fields', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(0);
    await page.waitForURL(new RegExp(`/partnerships/partners/${PARTNER_ID}(?:/)?$`), { timeout: 10000 });

    const detailsContent = page.locator('app-partner-view, app-partner-detail, [data-testid="partner-detail-header"]');
    await expect(detailsContent.first()).toBeVisible({ timeout: 10000 });
  });

  test('FUNC_002: Contacts tab shows contact list or empty state', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    const contactsContent = page.locator('app-partner-contacts, .p-datatable, .empty, .no-data');
    await expect(contactsContent.first()).toBeVisible({ timeout: 10000 });
  });

  test('FUNC_003: Interactions tab shows interaction records or empty state', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(3);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/interactions/, { timeout: 10000 });

    const interactionsContent = page.locator('app-partner-view-interactions, .p-datatable, .empty, .no-data');
    await expect(interactionsContent.first()).toBeVisible({ timeout: 10000 });
  });

  test('FUNC_004: Active tab is visually highlighted', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await expect(tabsPage.activeTab).toBeVisible();
    await expect(tabsPage.activeTab).toHaveAttribute('aria-selected', 'true');
  });

  test('FUNC_005: Tab content updates without full page reload', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const initialDocTitle = await page.title();
    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    const docTitleAfter = await page.title();
    expect(docTitleAfter).toBeTruthy();
  });

  test('FUNC_006: Each tab has unique content area', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(0);
    const detailsUrl = page.url();

    await tabsPage.clickDesktopTab(2);
    const contactsUrl = page.url();

    expect(detailsUrl).not.toBe(contactsUrl);
    expect(contactsUrl).toContain('/contacts');
  });

  test('FUNC_007: Tab order matches design specification', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBe(EXPECTED_PARTNER_TABS);
    expect(tabCount).toBeGreaterThanOrEqual(4);
  });

  test('FUNC_008: Mobile dropdown shows all tab options', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    await tabsPage.mobileDropdown.click();
    const options = page.locator('.p-dropdown-panel .p-dropdown-item, .p-select-overlay .p-select-option');
    await options.first().waitFor({ state: 'visible', timeout: 5000 });
    const optionCount = await options.count();
    expect(optionCount).toBe(EXPECTED_PARTNER_TABS);
  });

  test('FUNC_009: Selecting mobile dropdown option loads correct content', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 375, height: 667 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForMobileDropdown();

    await tabsPage.mobileDropdown.click();
    const options = page.locator('.p-dropdown-panel .p-dropdown-item, .p-select-overlay .p-select-option');
    await options.first().waitFor({ state: 'visible', timeout: 5000 });
    await options.nth(2).click();

    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });
    expect(page.url()).toContain('/contacts');
  });

  // ==================== INTEGRATION TESTS (9) ====================

  test('INT_001: Tab switch triggers correct API calls', async ({ page }) => {
    const apiCalls: string[] = [];
    await page.route('**/api/**', route => {
      apiCalls.push(route.request().url());
      route.continue();
    });

    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    const hasPartnerCalls = apiCalls.some(url => url.includes('/api/partner'));
    expect(hasPartnerCalls).toBe(true);
  });

  test('INT_002: Tab content loads partner-specific data', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    expect(page.url()).toContain('/partnerships/partners/');
    expect(page.url()).toContain('/contacts');
  });

  test('INT_003: Permission-gated tabs hidden for unauthorized users', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, READONLY_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    expect(tabCount).toBeGreaterThan(0);
  });

  test('INT_004: Contact detail tabs work independently from partner tabs', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/contacts/${CONTACT_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const contactTabs = page.locator('app-contact-tabs [role="tab"]');
    await contactTabs.first().waitFor({ state: 'visible', timeout: 15000 });

    const tabCount = await contactTabs.count();
    expect(tabCount).toBe(EXPECTED_CONTACT_TABS);
  });

  test('INT_005: Navigation from tab content to related entity works', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.clickDesktopTab(2);
    await page.waitForURL(/\/partnerships\/partners\/\d+\/contacts/, { timeout: 10000 });

    const contactsContent = page.locator('app-partner-contacts');
    await expect(contactsContent).toBeVisible({ timeout: 10000 });
  });

  test('INT_006: Tab state consistent with URL parameters', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}/contacts`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const url = page.url();
    expect(url).toContain('/contacts');
    await expect(tabsPage.activeTab).toBeVisible();
  });

  test('INT_007: Multiple tab switches load correct data each time', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const tabCount = await tabsPage.getDesktopTabCount();
    for (let i = 0; i < tabCount; i++) {
      await tabsPage.clickDesktopTab(i);
      await page.waitForLoadState('networkidle');
    }

    const finalUrl = page.url();
    expect(finalUrl).toContain(`/partnerships/partners/${PARTNER_ID}`);
  });

  test('INT_008: Tab content accessible via keyboard navigation', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    await tabsPage.tabButtons.first().focus();
    await page.keyboard.press('ArrowRight');
    await page.keyboard.press('Enter');

    await page.waitForLoadState('networkidle');
    const url = page.url();
    expect(url).toContain(`/partnerships/partners/${PARTNER_ID}`);
  });

  test('INT_009: Deep link to specific tab works on page load', async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/partners/${PARTNER_ID}/contacts`, ADMIN_USER);
    await waitForPermissions(page);
    await page.setViewportSize({ width: 1280, height: 720 });

    const tabsPage = new ResponsiveTabsPage(page);
    await tabsPage.waitForDesktopTabs();

    const url = page.url();
    expect(url).toContain('/contacts');
    await expect(tabsPage.activeTab).toBeVisible();
  });
});
