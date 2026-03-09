/**
 * @fileoverview Entity List Base Page Object
 * Base page object for list pages (Partners, Contacts, Opportunities, etc.)
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible, assertPageHeader, assertListviewVisible } from '../helpers/assertions.helper';
import { waitForTableData, waitForDialog, waitForLoadingToComplete } from '../helpers/wait.helper';

export abstract class EntityListPage extends BasePage {
  protected abstract entityName: string;
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Display name for the entity (e.g., "Partners", "Contacts") used in text-based locators.
   * Override in subclass if the display name differs from entityName capitalization.
   */
  protected get displayName(): string {
    return this.entityName.charAt(0).toUpperCase() + this.entityName.slice(1);
  }

  /**
   * Singular display name (e.g., "Partner") derived from entityName by removing trailing 's'.
   */
  protected get singularDisplayName(): string {
    const name = this.displayName;
    if (name.endsWith('ies')) return name.slice(0, -3) + 'y';
    if (name.endsWith('s')) return name.slice(0, -1);
    return name;
  }

  get header(): Locator {
    return this.page.getByText(this.displayName, { exact: true }).first()
      .or(this.getByTestId(`${this.entityName}-header`));
  }

  get icon(): Locator {
    return this.getByTestId(`${this.entityName}-icon`)
      .or(this.page.locator('.material-icons, .material-symbols-outlined').first());
  }

  get title(): Locator {
    return this.page.getByText(this.displayName, { exact: true }).first()
      .or(this.getByTestId(`${this.entityName}-title`));
  }

  get listview(): Locator {
    return this.page.locator('app-listview').first()
      .or(this.getByTestId(`${this.entityName}-listview`));
  }

  get newButton(): Locator {
    return this.page.getByRole('button', { name: new RegExp(`new ${this.singularDisplayName}`, 'i') })
      .or(this.getByTestId(`new-${this.entityName.slice(0, -1)}-button`));
  }

  get exportButton(): Locator {
    return this.page.getByRole('button', { name: /export/i })
      .or(this.getByTestId('export-button'));
  }

  get importButton(): Locator {
    return this.page.getByRole('button', { name: /import/i })
      .or(this.getByTestId('import-button'));
  }

  get tableRows(): Locator {
    return this.page.locator('app-listview-card .cursor-pointer, tbody tr, .p-datatable-tbody tr');
  }

  get searchInput(): Locator {
    return this.page.getByPlaceholder(/search/i).first()
      .or(this.page.locator('input.quick-search, app-listview input[type="text"], [placeholder*="Search"]').first());
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
    await waitForLoadingToComplete(this.page);
  }
  
  /**
   * Search for text
   */
  async search(searchText: string): Promise<void> {
    if (await this.searchInput.isVisible().catch(() => false)) {
      await this.searchInput.fill(searchText);
      await waitForTableData(this.page);
    }
  }
  
  /**
   * Verify mobile responsiveness
   */
  async verifyMobileResponsive(): Promise<void> {
    await this.page.setViewportSize({ width: 375, height: 667 });
    await assertVisible(this.header, 10000);
    await assertVisible(this.listview, 10000);
  }
}
