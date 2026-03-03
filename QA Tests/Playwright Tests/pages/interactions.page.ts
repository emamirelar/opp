/**
 * @fileoverview Interactions List Page Object
 * Page object for the interactions list page at /partnerships/interactions
 *
 * Uses flexible selectors: data-testid first, then fallback to PrimeNG/class selectors
 * since the interaction-list component uses class names (interaction-new-button, etc.)
 *
 * @author UNOPS Opportunity+ QA Team
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { waitForPermissions } from '../helpers/wait.helper';

export class InteractionsPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to interactions list page
   */
  async navigateTo(): Promise<void> {
    await this.goto('/partnerships/interactions');
    await waitForPermissions(this.page);
  }

  /**
   * Get interaction list items (cards or rows)
   * Listview cards render as div with classes "group", "cursor-pointer", "rounded-xl"
   * inside app-listview-card. We use multiple fallback selectors for robustness.
   */
  getInteractionCards(): Locator {
    return this.page.locator(
      'app-listview-card .group.cursor-pointer'
    );
  }

  /**
   * Get count of visible interaction cards/rows.
   * First waits briefly for cards to render after data loads.
   */
  async getInteractionCount(): Promise<number> {
    await this.page.waitForTimeout(1000);
    const cards = this.getInteractionCards();
    const count = await cards.count();
    if (count > 0) return count;

    // Fallback: parse from "Showing X records" text
    const recordText = this.page.locator('text=/Showing \\d+ records?/i');
    const text = await recordText.textContent({ timeout: 5000 }).catch(() => '');
    const match = text?.match(/(\d+)/);
    return match ? parseInt(match[1], 10) : 0;
  }

  /**
   * Click a specific interaction card by index (0-based)
   */
  async clickInteraction(index: number): Promise<void> {
    const cards = this.getInteractionCards();
    const cardCount = await cards.count();
    if (cardCount === 0) {
      // Fallback: click a clickable element in the list area
      const fallback = this.page.locator('app-listview-card [class*="cursor-pointer"]');
      await fallback.nth(index).click({ timeout: 10000 });
    } else {
      await cards.nth(index).click({ timeout: 10000 });
    }
    await this.page.waitForTimeout(1000);
  }

  /**
   * Click the "New Interaction" button
   * Flexible: data-testid or class .interaction-new-button
   */
  async clickNewButton(): Promise<void> {
    const btn = this.page.locator(
      '[data-testid="new-interaction-button"], .interaction-new-button'
    ).first();
    await btn.click();
    await this.page.waitForTimeout(500);
  }

  /**
   * Type in the search box
   * Listview uses input.quick-search inside p-iconfield
   */
  async searchInteractions(query: string): Promise<void> {
    const searchInput = this.page.locator(
      'input.quick-search, ' +
        'app-listview input[type="text"], ' +
        '[data-testid="interaction-search"], [data-testid="search-input"]'
    ).first();
    await searchInput.fill(query);
    await this.page.keyboard.press('Enter');
    await this.page.waitForTimeout(1500); // Debounce + API response
  }

  /**
   * Get locator for empty state / "no data" message
   * Listview shows "No data available" (errors.noDataAvailable) with pi-info-circle
   */
  getEmptyStateMessage(): Locator {
    return this.page.locator(
      'span:has-text("No data available"), ' +
        '.pi-info-circle, [class*="noData"], ' +
        '[data-testid="no-data-message"], [data-testid="empty-state"], ' +
        'text=/no data available/i'
    ).first();
  }

  /**
   * Check if export button is visible
   */
  async isExportButtonVisible(): Promise<boolean> {
    const btn = this.page.locator(
      '[data-testid="export-button"], .interaction-export-button'
    ).first();
    return await btn.isVisible().catch(() => false);
  }

  /**
   * Check if import button is visible
   */
  async isImportButtonVisible(): Promise<boolean> {
    const btn = this.page.locator(
      '[data-testid="import-button"], .interaction-import-button'
    ).first();
    return await btn.isVisible().catch(() => false);
  }

  /**
   * Get page title/header text
   */
  async getPageTitle(): Promise<string> {
    const header = this.page.locator(
      '[data-testid="interactions-header"], [data-testid="interactions-title"], ' +
        '.interaction-section-header p, h1, .text-3xl.font-bold'
    ).first();
    const text = await header.textContent().catch(() => '');
    return (text || '').trim();
  }

  /**
   * Get New Interaction button locator (for visibility checks)
   */
  getNewButton(): Locator {
    return this.page.locator(
      '[data-testid="new-interaction-button"], .interaction-new-button'
    ).first();
  }

  /**
   * Check if New Interaction button is visible
   */
  async isNewButtonVisible(): Promise<boolean> {
    return await this.getNewButton().isVisible().catch(() => false);
  }

  /**
   * Get listview component locator
   */
  getListview(): Locator {
    return this.page.locator(
      '[data-testid="interactions-listview"], .interaction-listview, app-listview'
    ).first();
  }
}
