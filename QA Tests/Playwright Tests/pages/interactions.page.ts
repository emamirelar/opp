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
import {
  waitForPermissions,
  waitForDialog,
  waitForLoadingToComplete,
  waitForVisible,
  waitForPageReady,
} from '../helpers/wait.helper';

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
      'app-listview-card .group.cursor-pointer, app-listview-card .cursor-pointer, tbody tr'
    );
  }

  /**
   * Get count of visible interaction cards/rows.
   * Waits for listview to be visible before counting.
   */
  async getInteractionCount(): Promise<number> {
    await waitForLoadingToComplete(this.page);
    await waitForVisible(this.getListview(), 10000).catch(() => {});
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
    await waitForLoadingToComplete(this.page);
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
    await waitForDialog(this.page);
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
    await waitForLoadingToComplete(this.page);
  }

  /**
   * Get locator for empty state / "no data" message
   * Listview shows "No data available" (errors.noDataAvailable) with pi-info-circle
   */
  getEmptyStateMessage(): Locator {
    return this.page.locator(
      'span:has-text("No data available"), ' +
        '.pi-info-circle, [class*="noData"], ' +
        '[data-testid="no-data-message"], [data-testid="empty-state"]'
    ).first()
      .or(this.page.getByText(/no data available/i).first());
  }

  /**
   * Check if export button is visible
   */
  async isExportButtonVisible(): Promise<boolean> {
    const btn = this.page.getByRole('button', { name: /export/i })
      .or(this.page.locator('[data-testid="export-button"], .interaction-export-button')).first();
    return await btn.isVisible().catch(() => false);
  }

  /**
   * Check if import button is visible
   */
  async isImportButtonVisible(): Promise<boolean> {
    const btn = this.page.getByRole('button', { name: /import/i })
      .or(this.page.locator('[data-testid="import-button"], .interaction-import-button')).first();
    return await btn.isVisible().catch(() => false);
  }

  /**
   * Get page title/header text
   */
  async getPageTitle(): Promise<string> {
    const header = this.page.getByText('Interactions', { exact: true })
      .or(this.page.locator(
        '[data-testid="interactions-header"], [data-testid="interactions-title"], ' +
          '.interaction-section-header p, h1, .text-3xl.font-bold'
      )).first();
    const text = await header.textContent().catch(() => '');
    return (text || '').trim();
  }

  /**
   * Get New Interaction button locator (for visibility checks)
   */
  getNewButton(): Locator {
    return this.page.getByRole('button', { name: /new interaction/i })
      .or(this.page.locator('[data-testid="new-interaction-button"], .interaction-new-button')).first();
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
      'app-listview, [data-testid="interactions-listview"], .interaction-listview'
    ).first();
  }

  /**
   * Get Create Opportunity button locator (for visibility checks)
   * Flexible: getByRole, class, text, or data-testid for interactions list
   */
  getCreateOpportunityButton(): Locator {
    return this.page
      .getByRole('button', { name: /new opportunity|create opportunity/i })
      .or(
        this.page.locator(
          '.interaction-create-opportunity-button, button:has-text("New Opportunity"), button:has-text("Create Opportunity"), [data-testid="create-opportunity-button"]'
        )
      )
      .first();
  }

  /**
   * Check if Create Opportunity button is visible
   */
  async isCreateOpportunityButtonVisible(): Promise<boolean> {
    return await this.getCreateOpportunityButton().isVisible().catch(() => false);
  }

  /**
   * Open Create Opportunity dialog from interactions list.
   * Waits for page ready, clicks New Opportunity button, then waits for dialog.
   */
  async openCreateOpportunityDialog(): Promise<void> {
    await waitForPageReady(this.page);
    await this.getCreateOpportunityButton().click({ timeout: 10000 });
    await waitForDialog(this.page);
  }

  /**
   * Get search input locator
   */
  getSearchInput(): Locator {
    return this.page
      .locator(
        'input.quick-search, app-listview input[type="text"], [placeholder*="Search"]'
      )
      .first();
  }

  /**
   * Check if search input is visible
   */
  async isSearchInputVisible(): Promise<boolean> {
    return await this.getSearchInput().isVisible().catch(() => false);
  }
}
