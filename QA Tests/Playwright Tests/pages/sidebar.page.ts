/**
 * @fileoverview Sidebar Navigation Page Object
 *
 * Provides locators and helpers for the main navigation sidebar.
 * Used for PNO-801: Verify Leads and Initiatives are removed from sidebar.
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

  constructor(page: Page) {
    super(page);
    this.sidebar = page.locator('.layout-sidebar, .main-navigation-sidebar').first();
    this.menuContainer = page.locator('.layout-menu-container').first();
    this.menuList = page.locator('.layout-menu').first();
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
}
