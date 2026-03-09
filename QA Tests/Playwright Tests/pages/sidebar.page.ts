/**
 * @fileoverview Sidebar Navigation Page Object
 *
 * Provides locators and helpers for the main navigation sidebar.
 * Used for PNO-801: Verify Leads and Initiatives are removed from sidebar.
 * Extended for PNO-669: Mobile sidebar close button support.
 *
 * @author UNOPS Opportunity+ QA Team
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { waitForPermissions } from '../helpers/wait.helper';

export class SidebarPage extends BasePage {
  /** Sidebar container — uses CSS class; TODO: add data-testid="main-navigation-sidebar" to template */
  readonly sidebar: Locator;

  /** Menu container */
  readonly menuContainer: Locator;

  /** Menu list */
  readonly menuList: Locator;

  /** PNO-669: Mobile close button */
  readonly closeButton: Locator;

  /** PNO-669: Mobile header containing the close button */
  readonly mobileHeader: Locator;

  /** Hamburger menu toggle button in the topbar */
  readonly hamburgerButton: Locator;

  /** Layout wrapper (for checking mobile-active CSS class) */
  readonly layoutWrapper: Locator;

  constructor(page: Page) {
    super(page);
    this.sidebar = page.locator('.layout-sidebar, .main-navigation-sidebar').first();
    this.menuContainer = page.locator('.layout-menu-container').first();
    this.menuList = page.locator('.layout-menu').first();
    this.closeButton = page.locator('.sidebar-close-btn').first();
    this.mobileHeader = page.locator('.sidebar-mobile-header').first();
    this.hamburgerButton = page.locator('.layout-menu-button, .menu-toggle-button').first();
    this.layoutWrapper = page.locator('.layout-wrapper').first();
  }

  /**
   * Navigate to a route and wait for sidebar to be ready
   */
  async navigateTo(url: string): Promise<void> {
    await this.goto(url);
    await waitForPermissions(this.page);
  }

  /**
   * Check if a menu item with given text is visible in the sidebar
   */
  async isMenuItemVisible(text: string | RegExp): Promise<boolean> {
    const item =
      typeof text === 'string'
        ? this.sidebar.getByText(text, { exact: true })
        : this.sidebar.getByText(text);
    return item.isVisible().catch(() => false);
  }

  /**
   * Check if Leads menu item is visible (should be false after PNO-801)
   */
  async isLeadsMenuItemVisible(): Promise<boolean> {
    return this.isMenuItemVisible('Leads');
  }

  /**
   * Check if Initiatives menu item is visible (should be false after PNO-801)
   */
  async isInitiativesMenuItemVisible(): Promise<boolean> {
    return this.isMenuItemVisible('Initiatives');
  }

  /**
   * Check if Home menu item is visible (expected after PNO-801)
   */
  async isHomeMenuItemVisible(): Promise<boolean> {
    return this.isMenuItemVisible('Home');
  }

  /**
   * Check if Partnerships menu item/section is visible (expected after PNO-801)
   */
  async isPartnershipsMenuItemVisible(): Promise<boolean> {
    return this.isMenuItemVisible('Partnerships');
  }

  /**
   * Check if Admin menu item is visible for admin users (expected after PNO-801)
   */
  async isAdminMenuItemVisible(): Promise<boolean> {
    return this.isMenuItemVisible('Admin');
  }

  /**
   * Wait for sidebar to be visible
   */
  async waitForSidebarVisible(): Promise<void> {
    await this.sidebar.waitFor({ state: 'visible', timeout: 10000 });
  }

  // ── PNO-669: Mobile sidebar close button helpers ──────────────────────

  /**
   * Open the sidebar via the hamburger menu button (mobile viewport)
   */
  async openSidebarViaHamburger(): Promise<void> {
    await this.hamburgerButton.click();
    await this.page.waitForTimeout(300);
  }

  /**
   * Close the sidebar via the X close button (PNO-669)
   */
  async clickCloseButton(): Promise<void> {
    await this.closeButton.click();
    await this.page.waitForTimeout(300);
  }

  /**
   * Check if the mobile close button is visible
   */
  async isCloseButtonVisible(): Promise<boolean> {
    return this.closeButton.isVisible().catch(() => false);
  }

  /**
   * Check if the mobile header is visible
   */
  async isMobileHeaderVisible(): Promise<boolean> {
    return this.mobileHeader.isVisible().catch(() => false);
  }

  /**
   * Check if the sidebar is currently in mobile-active state
   */
  async isMobileActive(): Promise<boolean> {
    const wrapper = this.page.locator('.layout-wrapper').first();
    const cls = await wrapper.getAttribute('class').catch(() => '');
    return (cls ?? '').includes('layout-mobile-active');
  }

  /**
   * Check if the layout mask (backdrop) is visible
   */
  async isLayoutMaskVisible(): Promise<boolean> {
    return this.page.locator('.layout-mask').isVisible().catch(() => false);
  }

  /**
   * Get the close button's aria-label value
   */
  async getCloseButtonAriaLabel(): Promise<string | null> {
    return this.closeButton.getAttribute('aria-label');
  }
}
