/**
 * @fileoverview Entity List Base Page Object
 * Base page object for list pages (Partners, Contacts, Opportunities, etc.)
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible, assertPageHeader, assertListviewVisible } from '../helpers/assertions.helper';
import { waitForTableData, waitForDialog } from '../helpers/wait.helper';

export abstract class EntityListPage extends BasePage {
  protected abstract entityName: string;
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Get page header locator
   */
  get header(): Locator {
    return this.getByTestId(`${this.entityName}-header`);
  }
  
  /**
   * Get page icon locator
   */
  get icon(): Locator {
    return this.getByTestId(`${this.entityName}-icon`);
  }
  
  /**
   * Get page title locator
   */
  get title(): Locator {
    return this.getByTestId(`${this.entityName}-title`);
  }
  
  /**
   * Get listview component locator
   */
  get listview(): Locator {
    return this.getByTestId(`${this.entityName}-listview`);
  }
  
  /**
   * Get new entity button locator
   */
  get newButton(): Locator {
    return this.getByTestId(`new-${this.entityName.slice(0, -1)}-button`);
  }
  
  /**
   * Get export button locator
   */
  get exportButton(): Locator {
    return this.getByTestId('export-button');
  }
  
  /**
   * Get import button locator
   */
  get importButton(): Locator {
    return this.getByTestId('import-button');
  }
  
  /**
   * Get table rows
   */
  get tableRows(): Locator {
    return this.page.locator('tbody tr, .p-datatable-tbody tr');
  }
  
  /**
   * Get search input
   */
  get searchInput(): Locator {
    return this.page.locator('input[type="text"]').first();
  }
  
  /**
   * Verify page header is displayed
   */
  async verifyPageHeader(): Promise<void> {
    await assertPageHeader(this.page, this.entityName);
  }
  
  /**
   * Verify listview is displayed
   */
  async verifyListviewVisible(): Promise<void> {
    await assertListviewVisible(this.page, this.entityName);
  }
  
  /**
   * Check if new button is visible
   */
  async isNewButtonVisible(): Promise<boolean> {
    return await this.newButton.isVisible().catch(() => false);
  }
  
  /**
   * Click new entity button
   */
  async clickNewButton(): Promise<void> {
    await this.newButton.click();
    await waitForDialog(this.page);
  }
  
  /**
   * Check if export button is visible
   */
  async isExportButtonVisible(): Promise<boolean> {
    return await this.exportButton.isVisible().catch(() => false);
  }
  
  /**
   * Click export button
   */
  async clickExportButton(): Promise<void> {
    await this.exportButton.click();
  }
  
  /**
   * Check if import button is visible
   */
  async isImportButtonVisible(): Promise<boolean> {
    return await this.importButton.isVisible().catch(() => false);
  }
  
  /**
   * Click import button
   */
  async clickImportButton(): Promise<void> {
    await this.importButton.click();
  }
  
  /**
   * Get number of table rows
   */
  async getRowCount(): Promise<number> {
    await waitForTableData(this.page);
    return await this.tableRows.count();
  }
  
  /**
   * Click first table row
   */
  async clickFirstRow(): Promise<void> {
    await this.tableRows.first().click();
    await this.page.waitForTimeout(1000);
  }
  
  /**
   * Search for text
   */
  async search(searchText: string): Promise<void> {
    if (await this.searchInput.isVisible().catch(() => false)) {
      await this.searchInput.fill(searchText);
      await this.page.waitForTimeout(1000); // Wait for search debounce
    }
  }
  
  /**
   * Verify mobile responsiveness
   */
  async verifyMobileResponsive(): Promise<void> {
    await this.page.setViewportSize({ width: 375, height: 667 });
    await this.page.waitForTimeout(1000);
    
    await assertVisible(this.header);
    await assertVisible(this.listview);
  }
}
