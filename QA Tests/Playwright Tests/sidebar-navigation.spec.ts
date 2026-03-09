/**
 * @fileoverview PNO-801: Remove Leads and Initiatives from Side Panel E2E Tests
 *
 * Validates that legacy "Leads" and "Initiatives" menu items are removed from
 * the sidebar navigation. Direct URL navigation to /leads and /initiatives
 * should not show active feature content (Coming Soon placeholder is acceptable).
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-801
 *
 * @tests 10
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';
import { SidebarPage } from './pages/sidebar.page';

/** PNO-801 is frontend-only; sidebar removal is complete — tests always run */
const featureReady = process.env.PNO_801_SIDEBAR_IMPLEMENTED !== 'false';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

test.describe('PNO-801 — Sidebar Navigation (Leads/Initiatives Removed)', () => {
  test.slow();

  test.skip(!featureReady, 'PNO-801 sidebar not deployed — set PNO_801_SIDEBAR_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Sidebar — Leads menu item is NOT displayed', async ({ page }) => {
    await test.step('Arrange — navigate to home with sidebar visible', async () => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Leads is not in sidebar', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const leadsVisible = await sidebarPage.isLeadsMenuItemVisible();
      expect(leadsVisible, 'Leads menu item should NOT be visible in sidebar').toBe(false);
    });
  });

  test('TC-002: Sidebar — Initiatives menu item is NOT displayed', async ({ page }) => {
    await test.step('Arrange — navigate to home with sidebar visible', async () => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Initiatives is not in sidebar', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const initiativesVisible = await sidebarPage.isInitiativesMenuItemVisible();
      expect(initiativesVisible, 'Initiatives menu item should NOT be visible in sidebar').toBe(false);
    });
  });

  test('TC-003: Sidebar — Expected menu items (Home, Partnerships) are displayed', async ({ page }) => {
    await test.step('Arrange — navigate to home', async () => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Home and Partnerships visible', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const homeVisible = await sidebarPage.isHomeMenuItemVisible();
      const partnershipsVisible = await sidebarPage.isPartnershipsMenuItemVisible();
      expect(homeVisible, 'Home menu item should be visible').toBe(true);
      expect(partnershipsVisible, 'Partnerships menu item should be visible').toBe(true);
    });
  });

  test('TC-004: Sidebar — Neither Leads nor Initiatives appear in menu', async ({ page }) => {
    await test.step('Arrange — navigate to partnerships page', async () => {
      await page.goto('/partnerships/partners');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — both legacy items absent', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const leadsVisible = await sidebarPage.isLeadsMenuItemVisible();
      const initiativesVisible = await sidebarPage.isInitiativesMenuItemVisible();
      expect(leadsVisible, 'Leads should not appear').toBe(false);
      expect(initiativesVisible, 'Initiatives should not appear').toBe(false);
    });
  });
});

test.describe('PNO-801 — Direct URL Navigation', () => {
  test.slow();

  test.skip(!featureReady, 'PNO-801 sidebar not deployed');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-005: Navigation — Direct /leads URL does NOT show active feature content', async ({
    page,
  }) => {
    await test.step('Arrange — navigate directly to /leads', async () => {
      await page.goto('/leads');
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — Coming Soon or placeholder, not active list/detail', async () => {
      const url = page.url();
      expect(url).toContain('/leads');

      const comingSoon = page.getByText(/coming soon|stay tuned/i);
      const hasPlaceholder = await comingSoon.isVisible().catch(() => false);

      const partnersList = page.locator('app-listview, [data-testid="partners-list"]');
      const hasActiveContent = await partnersList.isVisible().catch(() => false);

      expect(
        hasPlaceholder || !hasActiveContent,
        'Should show Coming Soon placeholder, not active feature content'
      ).toBe(true);
    });
  });

  test('TC-006: Navigation — Direct /initiatives URL does NOT show active feature content', async ({
    page,
  }) => {
    await test.step('Arrange — navigate directly to /initiatives', async () => {
      await page.goto('/initiatives');
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — Coming Soon or placeholder, not active list/detail', async () => {
      const url = page.url();
      expect(url).toContain('/initiatives');

      const comingSoon = page.getByText(/coming soon|stay tuned/i);
      const hasPlaceholder = await comingSoon.isVisible().catch(() => false);

      const opportunitiesList = page.locator('app-listview, [data-testid="opportunities-list"]');
      const hasActiveContent = await opportunitiesList.isVisible().catch(() => false);

      expect(
        hasPlaceholder || !hasActiveContent,
        'Should show Coming Soon placeholder, not active feature content'
      ).toBe(true);
    });
  });
});

test.describe('PNO-801 — Role-Based Sidebar (Readonly User)', () => {
  test.slow();

  test.skip(!featureReady, 'PNO-801 sidebar not deployed');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/', READONLY_USER);
    await waitForPermissions(page);
  });

  test('TC-007: Role — Readonly user does NOT see Leads or Initiatives in sidebar', async ({
    page,
  }) => {
    await test.step('Arrange — navigate as readonly user', async () => {
      await page.goto('/partnerships/contacts');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — legacy items not visible', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const leadsVisible = await sidebarPage.isLeadsMenuItemVisible();
      const initiativesVisible = await sidebarPage.isInitiativesMenuItemVisible();
      expect(leadsVisible).toBe(false);
      expect(initiativesVisible).toBe(false);
    });
  });

  test('TC-008: Role — Readonly user sees Partnerships submenu without Leads/Initiatives', async ({
    page,
  }) => {
    await test.step('Arrange — navigate as readonly user', async () => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Partnerships visible, legacy items absent', async () => {
      const sidebarPage = new SidebarPage(page);
      await sidebarPage.waitForSidebarVisible();
      const partnershipsVisible = await sidebarPage.isPartnershipsMenuItemVisible();
      const leadsVisible = await sidebarPage.isLeadsMenuItemVisible();
      const initiativesVisible = await sidebarPage.isInitiativesMenuItemVisible();
      expect(partnershipsVisible).toBe(true);
      expect(leadsVisible).toBe(false);
      expect(initiativesVisible).toBe(false);
    });
  });
});

test.describe('PNO-801 — DEF-217 Breadcrumb Legacy Mappings', () => {
  test.slow();

  test.skip(!featureReady, 'PNO-801 sidebar not deployed');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-009: DEF-217 — Breadcrumb on /leads should not display legacy Leads label', async ({
    page,
  }) => {
    test.info().annotations.push({ type: 'defect', description: 'DEF-217' });

    await test.step('Arrange — navigate to /leads', async () => {
      await page.goto('/leads');
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — breadcrumb does not show legacy Leads label', async () => {
      const breadcrumb = page.locator(
        '.breadcrumb-bar, .breadcrumb, [data-testid="breadcrumb"], p-breadcrumb, nav[aria-label="Breadcrumb"]'
      ).first();
      const breadcrumbVisible = await breadcrumb.isVisible().catch(() => false);

      if (breadcrumbVisible) {
        const breadcrumbText = await breadcrumb.textContent();
        expect(
          breadcrumbText,
          'DEF-217: Breadcrumb labelMap should not contain legacy Leads mapping'
        ).not.toContain('Leads');
      }
    });
  });

  test('TC-010: DEF-217 — Breadcrumb on /initiatives should not display legacy Initiatives label', async ({
    page,
  }) => {
    test.info().annotations.push({ type: 'defect', description: 'DEF-217' });

    await test.step('Arrange — navigate to /initiatives', async () => {
      await page.goto('/initiatives');
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — breadcrumb does not show legacy Initiatives label', async () => {
      const breadcrumb = page.locator(
        '.breadcrumb-bar, .breadcrumb, [data-testid="breadcrumb"], p-breadcrumb, nav[aria-label="Breadcrumb"]'
      ).first();
      const breadcrumbVisible = await breadcrumb.isVisible().catch(() => false);

      if (breadcrumbVisible) {
        const breadcrumbText = await breadcrumb.textContent();
        expect(
          breadcrumbText,
          'DEF-217: Breadcrumb labelMap should not contain legacy Initiatives mapping'
        ).not.toContain('Initiatives');
      }
    });
  });
});
