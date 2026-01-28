/**
 * @fileoverview Partners Page Object
 * Page object for partners list page
 */

import { Page } from '@playwright/test';
import { EntityListPage } from './entity-list.page';

export class PartnersPage extends EntityListPage {
  protected entityName = 'partners';
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Navigate to partners page
   */
  async navigate(): Promise<void> {
    await this.goto('/partners');
  }
  
  /**
   * Navigate to specific partner detail
   */
  async navigateToPartnerDetail(id: number): Promise<void> {
    await this.goto(`/partners/${id}`);
  }
}
