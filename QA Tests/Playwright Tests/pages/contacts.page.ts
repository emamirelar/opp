/**
 * @fileoverview Contacts Page Object
 * Page object for contacts list page
 */

import { Page, Locator } from '@playwright/test';
import { EntityListPage } from './entity-list.page';

export class ContactsPage extends EntityListPage {
  protected entityName = 'contacts';

  /** Contact list uses "New" (button.new), not "New Contact" - add class fallback */
  override get newButton(): Locator {
    return this.page
      .locator('.contact-new-button')
      .first()
      .or(this.page.getByRole('button', { name: /new contact/i }))
      .or(this.getByTestId('new-contact-button'));
  }

  constructor(page: Page) {
    super(page);
  }

  /**
   * Get export button - contact list uses .contact-export-button with icon pi-file-export
   */
  override get exportButton(): Locator {
    return this.page.locator('.contact-export-button, button:has(.pi-file-export), button:has(.pi-download)').first()
      .or(this.page.getByRole('button', { name: /export/i }).first())
      .or(this.getByTestId('export-button'));
  }

  /**
   * Get import button - contact list uses .contact-import-button with icon pi-file-import
   */
  override get importButton(): Locator {
    return this.page.locator('.contact-import-button, button:has(.pi-file-import), button:has(.pi-upload)').first()
      .or(this.page.getByRole('button', { name: /import/i }).first())
      .or(this.getByTestId('import-button'));
  }

  /**
   * Get business card scanner button
   * Contact list uses .contact-scanner-button with icon pi-camera (no data-testid)
   */
  get scannerButton(): Locator {
    return this.getByTestId('scan-business-card-button')
      .or(this.page.locator('p-button.contact-scanner-button, .contact-scanner-button, button:has(.pi-camera), button:has(.pi-qrcode)').first())
      .or(this.page.getByRole('button', { name: /scan|business card/i }).first());
  }
  
  /**
   * Get the business card scanner component element.
   * We target the inner fixed-position div rather than the host element because
   * Angular custom elements default to display:inline with 0 dimensions,
   * making the host "hidden" to Playwright even when the dialog is visible.
   */
  get scannerComponent(): Locator {
    return this.page.locator('app-business-card-scanner > div').first();
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
   * Click business card scanner button and wait for scanner component to render.
   * QA-007 FIX: The scanner uses a custom div-based modal (not p-dialog),
   * so we wait for the app-business-card-scanner component instead.
   */
  async clickScannerButton(): Promise<void> {
    await this.scannerButton.click();
    // Wait for the scanner component to appear in the DOM (custom modal, not p-dialog)
    await this.scannerComponent.waitFor({ state: 'visible', timeout: 5000 });
  }
  
  /**
   * Check if scanner component is visible in the DOM
   */
  async isScannerComponentVisible(): Promise<boolean> {
    return await this.scannerComponent.isVisible().catch(() => false);
  }
}
