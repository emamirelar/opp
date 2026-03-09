/**
 * @fileoverview Dashboard Page Object
 * Page object for dashboard/home page
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForLoadingToComplete } from '../helpers/wait.helper';

export class DashboardPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Get dashboard panels
   */
  get panels(): Locator {
    return this.page.locator('.bg-unops-surface-primary');
  }
  
  /**
   * Get welcome header
   */
  get welcomeHeader(): Locator {
    return this.page.locator('h1').filter({ hasText: /welcome/i });
  }
  
  /**
   * Get refresh button
   */
  get refreshButton(): Locator {
    return this.page.locator('button i.pi-refresh').locator('..');
  }
  
  /**
   * Get grid layout
   */
  get gridLayout(): Locator {
    return this.page.locator('.grid').first();
  }
  
  /**
   * Get activity dots
   */
  get activityDots(): Locator {
    return this.page.locator('.w-2.h-2.rounded-full');
  }

  /**
   * Get activity cards
   */
  get activityCards(): Locator {
    return this.page.locator('.hover\\:border-unops-info\\/50');
  }
  
  /**
   * Navigate to dashboard
   */
  async navigate(): Promise<void> {
    await this.goto('/home');
  }
  
  /**
   * Verify dashboard is displayed
   */
  async verifyDashboardVisible(): Promise<void> {
    await assertVisible(this.panels.first(), 10000);
  }
  
  /**
   * Verify welcome message
   */
  async verifyWelcomeMessage(): Promise<void> {
    await assertVisible(this.welcomeHeader.first(), 10000);
  }
  
  /**
   * Get panel count
   */
  async getPanelCount(): Promise<number> {
    return await this.panels.count();
  }
  
  /**
   * Click refresh button
   */
  async clickRefresh(): Promise<void> {
    if (await this.refreshButton.isVisible().catch(() => false)) {
      await this.refreshButton.click();
      await waitForLoadingToComplete(this.page);
    }
  }
  
  /**
   * Check if quick actions are visible
   */
  async hasQuickActions(): Promise<boolean> {
    const quickActionButtons = this.page.locator('button').filter({ 
      hasText: /New Partner|New Contact|New Interaction|New Opportunity/i 
    });
    
    return await quickActionButtons.first().isVisible().catch(() => false);
  }
  
  /**
   * Verify grid layout is displayed
   */
  async verifyGridLayout(): Promise<void> {
    await assertVisible(this.gridLayout, 10000);
  }
  
  /**
   * Check if activity section has data (dots)
   */
  async hasActivityData(): Promise<boolean> {
    return await this.activityDots.first().isVisible().catch(() => false);
  }

  /**
   * Check if activity section has cards
   */
  async hasActivityCards(): Promise<boolean> {
    return await this.activityCards.first().isVisible().catch(() => false);
  }
  
  /**
   * Verify mobile responsiveness
   */
  async verifyMobileResponsive(): Promise<void> {
    await this.page.setViewportSize({ width: 375, height: 667 });
    await assertVisible(this.page.locator('.max-w-7xl'), 10000);
    await assertVisible(this.panels.first(), 10000);
  }
}
