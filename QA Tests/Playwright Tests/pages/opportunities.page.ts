/**
 * @fileoverview Opportunities Page Object
 * Page object for opportunities list page
 */

import { Page, Locator } from '@playwright/test';
import { EntityListPage } from './entity-list.page';

export class OpportunitiesPage extends EntityListPage {
  protected entityName = 'opportunities';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Get listview locator (app-listview with class opportunity-listview)
   * Overrides base to match actual template structure
   */
  override get listview(): Locator {
    return this.page.locator('[data-testid="opportunities-listview"], app-listview').first();
  }

  /**
   * Navigate to opportunities page
   */
  async navigate(): Promise<void> {
    await this.goto('/partnerships/opportunities');
  }
  
  /**
   * Navigate to specific opportunity detail
   */
  async navigateToOpportunityDetail(id: number): Promise<void> {
    await this.goto(`/opportunities/${id}`);
  }
}
