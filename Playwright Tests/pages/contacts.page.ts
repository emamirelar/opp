/**
 * @fileoverview Contacts Page Object
 * Page object for contacts list page
 */

import { Page, Locator } from '@playwright/test';
import { EntityListPage } from './entity-list.page';
import { waitForDialog } from '../helpers/wait.helper';

export class ContactsPage extends EntityListPage {
  protected entityName = 'contacts';
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Get business card scanner button
   */
  get scannerButton(): Locator {
    return this.getByTestId('scan-business-card-button');
  }
  
  /**
   * Navigate to contacts page
   */
  async navigate(): Promise<void> {
    await this.goto('/contacts');
  }
  
  /**
   * Navigate to specific contact detail
   */
  async navigateToContactDetail(id: number): Promise<void> {
    await this.goto(`/contacts/${id}`);
  }
  
  /**
   * Check if business card scanner button is visible
   */
  async isScannerButtonVisible(): Promise<boolean> {
    return await this.scannerButton.isVisible().catch(() => false);
  }
  
  /**
   * Click business card scanner button
   */
  async clickScannerButton(): Promise<void> {
    await this.scannerButton.click();
    await waitForDialog(this.page);
  }
}
