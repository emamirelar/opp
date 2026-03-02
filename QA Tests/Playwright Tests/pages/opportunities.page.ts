/**
 * @fileoverview Opportunities Page Object
 * Page object for opportunities list page
 */

import { Page } from '@playwright/test';
import { EntityListPage } from './entity-list.page';

export class OpportunitiesPage extends EntityListPage {
  protected entityName = 'opportunities';
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Navigate to opportunities page
   */
  async navigate(): Promise<void> {
    await this.goto('/opportunities');
  }
  
  /**
   * Navigate to specific opportunity detail
   */
  async navigateToOpportunityDetail(id: number): Promise<void> {
    await this.goto(`/opportunities/${id}`);
  }
}
