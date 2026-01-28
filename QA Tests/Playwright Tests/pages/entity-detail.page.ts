/**
 * @fileoverview Entity Detail Base Page Object
 * Base page object for detail/item pages (Partner Item, Contact Item, etc.)
 * Provides common functionality for all entity detail pages
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForDialog, waitForPageReady } from '../helpers/wait.helper';

export abstract class EntityDetailPage extends BasePage {
  protected abstract entityName: string;
  protected recordId: string | number;
  
  constructor(page: Page, recordId?: string | number) {
    super(page);
    this.recordId = recordId || 0;
  }
  
  /**
   * Get page header locator
   */
  get header(): Locator {
    return this.getByTestId(`${this.entityName}-detail-header`);
  }
  
  /**
   * Get entity title/name display locator
   */
  get entityTitle(): Locator {
    return this.getByTestId(`${this.entityName}-title`);
  }
  
  /**
   * Get edit button locator
   */
  get editButton(): Locator {
    return this.getByTestId(`edit-${this.entityName}-button`);
  }
  
  /**
   * Get delete button locator
   */
  get deleteButton(): Locator {
    return this.getByTestId(`delete-${this.entityName}-button`);
  }
  
  /**
   * Get workflow status badge locator
   */
  get workflowStatus(): Locator {
    return this.getByTestId(`${this.entityName}-workflow-status`);
  }
  
  /**
   * Get back button locator (navigate to list)
   */
  get backButton(): Locator {
    return this.getByTestId('back-to-list-button');
  }
  
  /**
   * Get documents section locator
   */
  get documentsSection(): Locator {
    return this.getByTestId(`${this.entityName}-documents`);
  }
  
  /**
   * Get activity timeline locator
   */
  get activityTimeline(): Locator {
    return this.getByTestId(`${this.entityName}-activity-timeline`);
  }
  
  /**
   * Get permissions panel locator
   */
  get permissionsPanel(): Locator {
    return this.getByTestId(`${this.entityName}-permissions`);
  }
  
  /**
   * Navigate to entity detail page
   * @param id - Entity ID
   */
  async navigateToDetail(id: string | number): Promise<void> {
    this.recordId = id;
    await this.goto(`/${this.entityName}s/${id}`);
    await waitForPageReady(this.page);
  }
  
  /**
   * Verify page header is displayed
   */
  async verifyPageHeader(): Promise<void> {
    await assertVisible(this.header);
  }
  
  /**
   * Verify entity title is displayed
   */
  async verifyEntityTitle(expectedTitle?: string): Promise<void> {
    await assertVisible(this.entityTitle);
    
    if (expectedTitle) {
      const actualTitle = await this.entityTitle.textContent();
      if (actualTitle && !actualTitle.includes(expectedTitle)) {
        throw new Error(`Expected title to contain "${expectedTitle}", but got "${actualTitle}"`);
      }
    }
  }
  
  /**
   * Check if edit button is visible
   */
  async isEditButtonVisible(): Promise<boolean> {
    return await this.editButton.isVisible().catch(() => false);
  }
  
  /**
   * Click edit button
   */
  async clickEditButton(): Promise<void> {
    await this.editButton.click();
    await waitForDialog(this.page);
  }
  
  /**
   * Check if delete button is visible
   */
  async isDeleteButtonVisible(): Promise<boolean> {
    return await this.deleteButton.isVisible().catch(() => false);
  }
  
  /**
   * Click delete button
   */
  async clickDeleteButton(): Promise<void> {
    await this.deleteButton.click();
    await waitForDialog(this.page);
  }
  
  /**
   * Get workflow status text
   */
  async getWorkflowStatus(): Promise<string | null> {
    if (await this.workflowStatus.isVisible().catch(() => false)) {
      return await this.workflowStatus.textContent();
    }
    return null;
  }
  
  /**
   * Click back button to return to list
   */
  async clickBackButton(): Promise<void> {
    await this.backButton.click();
    await waitForPageReady(this.page);
  }
  
  /**
   * Check if documents section is visible
   */
  async hasDocumentsSection(): Promise<boolean> {
    return await this.documentsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get document count
   */
  async getDocumentCount(): Promise<number> {
    if (!await this.hasDocumentsSection()) {
      return 0;
    }
    
    const documentItems = this.page.locator(`[data-testid="${this.entityName}-document-item"]`);
    return await documentItems.count();
  }
  
  /**
   * Check if activity timeline is visible
   */
  async hasActivityTimeline(): Promise<boolean> {
    return await this.activityTimeline.isVisible().catch(() => false);
  }
  
  /**
   * Get activity count
   */
  async getActivityCount(): Promise<number> {
    if (!await this.hasActivityTimeline()) {
      return 0;
    }
    
    const activityItems = this.page.locator(`[data-testid="${this.entityName}-activity-item"]`);
    return await activityItems.count();
  }
  
  /**
   * Verify page loads successfully
   */
  async verifyPageLoaded(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyEntityTitle();
  }
  
  /**
   * Verify mobile responsive layout
   */
  async verifyMobileResponsive(): Promise<void> {
    await this.page.setViewportSize({ width: 375, height: 667 });
    await this.page.waitForTimeout(1000);
    
    await assertVisible(this.header);
    await assertVisible(this.entityTitle);
  }
  
  /**
   * Wait for permissions to load
   * Useful before checking permission-based button visibility
   */
  async waitForPermissionsToLoad(): Promise<void> {
    await this.waitForPermissions();
    await this.page.waitForTimeout(1000); // Extra time for UI updates
  }
  
  /**
   * Get detail field value by test ID
   * @param fieldTestId - Test ID of the field
   */
  async getFieldValue(fieldTestId: string): Promise<string | null> {
    const field = this.getByTestId(fieldTestId);
    if (await field.isVisible().catch(() => false)) {
      return await field.textContent();
    }
    return null;
  }
  
  /**
   * Verify field is displayed with expected value
   * @param fieldTestId - Test ID of the field
   * @param expectedValue - Expected field value (optional)
   */
  async verifyField(fieldTestId: string, expectedValue?: string): Promise<void> {
    const field = this.getByTestId(fieldTestId);
    await assertVisible(field);
    
    if (expectedValue) {
      const actualValue = await field.textContent();
      if (actualValue && !actualValue.includes(expectedValue)) {
        throw new Error(`Expected field "${fieldTestId}" to contain "${expectedValue}", but got "${actualValue}"`);
      }
    }
  }
}
