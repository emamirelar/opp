/**
 * @fileoverview PNO-669: Mobile Sidebar Close Button E2E Tests
 *
 * Bug: In mobile interface, users are unable to easily exit the menu screen
 * when text size is set too large. The sidebar fills the entire page.
 *
 * Fix: Added a close button (X icon) in sidebar-mobile-header that calls
 * closeSidebar() to reset overlayMenuActive and staticMenuMobileActive.
 *
 * Requirements tested:
 * - REQ-1: Close button markup exists and is clickable on mobile
 * - REQ-2: Close button is hidden on desktop, visible on mobile (max-width: 991px)
 * - REQ-3: Clicking close button hides the sidebar (resets layout state)
 * - REQ-4: Close button has proper aria-label for accessibility
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-669
 *
 * @tests 20
 */

import { test, expect } from '@playwright/test';
import { setupAPIMocks } from './helpers/api-mocks.helper';
import { authenticateWithMocks } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';
import { SidebarPage } from './pages/sidebar.page';

const ADMIN_USER = 'test@playwright.local';
const MOBILE_VIEWPORT = { width: 375, height: 812 };
const TABLET_VIEWPORT = { width: 768, height: 1024 };
const DESKTOP_VIEWPORT = { width: 1280, height: 900 };
const LARGE_TEXT_MOBILE = { width: 375, height: 667 };

test.describe('PNO-669 — Mobile Sidebar Close Button (Mobile Viewport)', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(MOBILE_VIEWPORT);
    await setupAPIMocks(page, ADMIN_USER);
    await authenticateWithMocks(page, '/', ADMIN_USER);
    await waitForPermissions(page);
  });

  // ── Positive Tests ──────────────────────────────────────────────────────

  test('TC-001: [Positive] Close button is visible after opening sidebar on mobile', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar via hamburger', async () => {
      await sidebar.openSidebarViaHamburger();
    });

    await test.step('Assert close button is visible', async () => {
      const visible = await sidebar.isCloseButtonVisible();
      expect(visible, 'PNO-669: Close button must be visible on mobile when sidebar is open').toBe(true);
    });
  });

  test('TC-002: [Positive] Clicking close button hides the sidebar', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar via hamburger', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Click close button', async () => {
      await sidebar.clickCloseButton();
    });

    await test.step('Assert sidebar is hidden', async () => {
      const mobileActive = await sidebar.isMobileActive();
      expect(mobileActive, 'PNO-669: Sidebar must be hidden after clicking close button').toBe(false);
    });
  });

  // ── Negative Tests ──────────────────────────────────────────────────────

  test('TC-003: [Negative] Close button does NOT prevent sidebar from opening', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Verify sidebar can still be opened via hamburger', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      const sidebarVisible = await sidebar.sidebar.isVisible();
      expect(sidebarVisible, 'Sidebar must still open via hamburger button').toBe(true);
    });
  });

  test('TC-004: [Negative] Menu items remain accessible with close button present', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert menu items still visible alongside close button', async () => {
      const closeVisible = await sidebar.isCloseButtonVisible();
      const homeVisible = await sidebar.isMenuItemVisible('Home');
      expect(closeVisible, 'Close button should be visible').toBe(true);
      expect(homeVisible, 'Home menu item should still be visible with close button').toBe(true);
    });
  });

  test('TC-005: [Negative] Close button does not navigate to a different page', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar and note current URL', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    const urlBefore = page.url();

    await test.step('Click close button', async () => {
      await sidebar.clickCloseButton();
    });

    await test.step('Assert URL unchanged', async () => {
      const urlAfter = page.url();
      expect(urlAfter, 'Clicking close button must not navigate away').toBe(urlBefore);
    });
  });

  test('TC-006: [Negative] Sidebar re-opens after being closed via close button', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open → close → re-open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      await sidebar.clickCloseButton();
      await page.waitForTimeout(300);
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert sidebar is visible again', async () => {
      const sidebarVisible = await sidebar.sidebar.isVisible();
      expect(sidebarVisible, 'Sidebar must re-open after being closed with X button').toBe(true);
    });
  });

  test('TC-007: [Negative] Multiple rapid close clicks do not break sidebar state', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar and click close multiple times', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      await sidebar.closeButton.click();
      await sidebar.closeButton.click().catch(() => {});
      await sidebar.closeButton.click().catch(() => {});
      await page.waitForTimeout(500);
    });

    await test.step('Assert sidebar is closed and page is stable', async () => {
      const mobileActive = await sidebar.isMobileActive();
      expect(mobileActive, 'Sidebar state must remain stable after rapid clicks').toBe(false);
    });
  });

  test('TC-008: [Negative] Hamburger button still exists in topbar on mobile', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Assert hamburger is visible', async () => {
      const hamburgerVisible = await sidebar.hamburgerButton.isVisible();
      expect(hamburgerVisible, 'Hamburger menu button must still be visible on mobile').toBe(true);
    });
  });

  // ── Boundary Tests ──────────────────────────────────────────────────────

  test('TC-009: [Boundary] Close button visible at 768px tablet viewport', async ({ page }) => {
    await page.setViewportSize(TABLET_VIEWPORT);
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar at 768px', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button visible at tablet width', async () => {
      const visible = await sidebar.isCloseButtonVisible();
      expect(visible, 'PNO-669: Close button must be visible at 768px (below 991px breakpoint)').toBe(true);
    });
  });

  test('TC-010: [Boundary] Close button hidden at 1280px desktop viewport', async ({ page }) => {
    await page.setViewportSize(DESKTOP_VIEWPORT);
    const sidebar = new SidebarPage(page);

    await test.step('Wait for page to stabilize at desktop width', async () => {
      await page.waitForTimeout(500);
    });

    await test.step('Assert close button NOT visible on desktop', async () => {
      const visible = await sidebar.isCloseButtonVisible();
      expect(visible, 'Close button must be hidden on desktop (above 991px breakpoint)').toBe(false);
    });
  });

  test('TC-011: [Boundary] Close button works with small viewport simulating large text', async ({ page }) => {
    await page.setViewportSize(LARGE_TEXT_MOBILE);
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar in constrained viewport', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button is clickable and closes sidebar', async () => {
      const visible = await sidebar.isCloseButtonVisible();
      expect(visible, 'PNO-669: Close button must be visible even with small viewport').toBe(true);
      await sidebar.clickCloseButton();
      const mobileActive = await sidebar.isMobileActive();
      expect(mobileActive, 'Sidebar must close in small viewport scenario').toBe(false);
    });
  });

  test('TC-012: [Boundary] Sidebar does not overflow viewport on 320px width', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 568 });
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar on minimum width', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert sidebar is within viewport bounds', async () => {
      const box = await sidebar.sidebar.boundingBox();
      if (box) {
        expect(box.width, 'Sidebar width must not exceed viewport width (320px)').toBeLessThanOrEqual(320);
      }
    });
  });

  test('TC-013: [Boundary] Close button is above menu items (not scrolled out of view)', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button is above first menu item', async () => {
      const closeBox = await sidebar.closeButton.boundingBox();
      const menuBox = await sidebar.menuList.boundingBox();
      if (closeBox && menuBox) {
        expect(closeBox.y, 'Close button must be above the menu list').toBeLessThan(menuBox.y);
      }
    });
  });

  test('TC-014: [Boundary] Close button remains accessible after scrolling menu', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar and scroll within it', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      await sidebar.sidebar.evaluate((el) => el.scrollTop = el.scrollHeight);
      await page.waitForTimeout(200);
    });

    await test.step('Assert close button is still in fixed position', async () => {
      const closeBox = await sidebar.closeButton.boundingBox();
      expect(closeBox, 'Close button should still have a bounding box after scroll').not.toBeNull();
      if (closeBox) {
        expect(closeBox.y, 'Close button should remain near top of viewport').toBeLessThan(100);
      }
    });
  });

  // ── Functional Tests ────────────────────────────────────────────────────

  test('TC-015: [Functional] Close button has accessible aria-label', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button has aria-label attribute', async () => {
      const ariaLabel = await sidebar.getCloseButtonAriaLabel();
      expect(ariaLabel, 'PNO-669 REQ-4: Close button must have a non-empty aria-label').toBeTruthy();
      expect(ariaLabel!.length, 'aria-label must not be empty').toBeGreaterThan(0);
    });
  });

  test('TC-016: [Functional] Close button uses X icon (pi-times)', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button contains pi-times icon', async () => {
      const icon = sidebar.closeButton.locator('i.pi.pi-times, i.pi-times');
      const iconVisible = await icon.isVisible().catch(() => false);
      expect(iconVisible, 'Close button must display the X (pi-times) icon').toBe(true);
    });
  });

  test('TC-017: [Functional] Layout mask disappears after closing sidebar', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar — mask should appear', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Close sidebar — mask should disappear', async () => {
      await sidebar.clickCloseButton();
      await page.waitForTimeout(500);
      const maskVisible = await sidebar.isLayoutMaskVisible();
      expect(maskVisible, 'Layout mask must disappear after closing sidebar').toBe(false);
    });
  });

  test('TC-018: [Functional] Close button is a <button> element (not <a>)', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
    });

    await test.step('Assert close button is a <button> tag', async () => {
      const tagName = await sidebar.closeButton.evaluate(el => el.tagName.toLowerCase());
      expect(tagName, 'Close action must use <button> element for accessibility').toBe('button');
    });
  });

  // ── Integration Tests ───────────────────────────────────────────────────

  test('TC-019: [Integration] Full open-close-reopen flow on mobile', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar via hamburger', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      const visible = await sidebar.sidebar.isVisible();
      expect(visible).toBe(true);
    });

    await test.step('Close via X button', async () => {
      await sidebar.clickCloseButton();
      const mobileActive = await sidebar.isMobileActive();
      expect(mobileActive).toBe(false);
    });

    await test.step('Re-open via hamburger', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      const visible = await sidebar.sidebar.isVisible();
      expect(visible).toBe(true);
    });

    await test.step('Close again and verify clean state', async () => {
      await sidebar.clickCloseButton();
      const mobileActive = await sidebar.isMobileActive();
      expect(mobileActive).toBe(false);
    });
  });

  test('TC-020: [Integration] Navigation still works after close → page change → reopen', async ({ page }) => {
    const sidebar = new SidebarPage(page);

    await test.step('Open sidebar, close it, then navigate', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      await sidebar.clickCloseButton();
      await page.waitForTimeout(300);
    });

    await test.step('Navigate to a different route', async () => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Re-open sidebar and verify it works on new page', async () => {
      await sidebar.openSidebarViaHamburger();
      await sidebar.waitForSidebarVisible();
      const closeVisible = await sidebar.isCloseButtonVisible();
      expect(closeVisible, 'Close button must work after page navigation').toBe(true);
    });
  });
});
