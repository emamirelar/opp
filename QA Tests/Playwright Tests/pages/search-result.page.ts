/**
 * @fileoverview Search Result Page Object
 * Page object for global search results page (/search?q=...)
 * PNO-926-v3: Entity tab icons (material-symbols-outlined)
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class SearchResultPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  /**
   * Get search result component container
   */
  get searchResultContainer(): Locator {
    return this.page.locator('app-search-result').first();
  }

  /**
   * Get all material-symbols-outlined icon spans (entity tab icons)
   */
  get entityTabIcons(): Locator {
    return this.page.locator('.material-symbols-outlined');
  }

  /**
   * Get entity tab icons within tablist
   */
  get tablistIcons(): Locator {
    return this.page.locator(
      'p-tablist .material-symbols-outlined, [role="tablist"] .material-symbols-outlined'
    );
  }

  /**
   * Get entity tabs
   */
  get tabs(): Locator {
    return this.page.locator('[role="tab"], p-tab');
  }

  /**
   * Get icon for specific entity type
   */
  getIconForEntity(entityType: string): Locator {
    return this.page
      .locator(
        `[class*="tab"][class*="${entityType}"], [data-entity="${entityType}"]`
      )
      .first()
      .locator('.material-symbols-outlined')
      .first();
  }

  /**
   * Get icon by expected icon name (e.g. 'corporate_fare', 'lightbulb')
   */
  getIconByText(iconText: string): Locator {
    return this.page
      .locator('.material-symbols-outlined')
      .filter({ hasText: iconText });
  }

  /**
   * Get global search trigger (topbar)
   */
  get globalSearchTrigger(): Locator {
    return this.page
      .locator(
        '[data-testid="global-search"], input[placeholder*="Search" i], .search-bar, app-global-search-bar'
      )
      .first();
  }

  /**
   * Get error overlay (for negative assertions)
   */
  get errorOverlay(): Locator {
    return this.page.locator(
      '[data-testid="error-overlay"], .error-page, .critical-error'
    );
  }
}
