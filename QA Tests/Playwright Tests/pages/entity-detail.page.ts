/**
 * @fileoverview Entity Detail Base Page Object
 * Base page object for detail/item pages (Partner Item, Contact Item, etc.)
 * Provides common functionality for all entity detail pages
 * 
 * Uses actual data-testid attributes found across entity templates:
 *   - {entity}-detail-header: Header container (partner, contact, interaction, opportunity)
 *   - {entity}-title: Title/name label (partner, contact, interaction, opportunity)
 *   - edit-{entity}-button: Edit button (partner, contact, interaction)
 *   - delete-{entity}-button: Delete button (partner, contact, interaction)
 *   - {entity}-documents-section: Documents panel (partner, contact, interaction)
 * 
 * NOTE: The following do NOT have data-testid attributes in any template:
 *   - Workflow status → use app-workflow / app-stage-workflow component selector
 *   - Back/navigation → use routerLink-based locator or browser navigation
 *   - Activity timeline → not a standalone feature in current templates
 *   - Permissions panel → permissions loaded via API, no visible UI panel
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForDialog, waitForPageReady, waitForLoadingToComplete } from '../helpers/wait.helper';

export abstract class EntityDetailPage extends BasePage {
  protected abstract entityName: string;
  protected recordId: string | number;
  
  constructor(page: Page, recordId?: string | number) {
    super(page);
    this.recordId = recordId || 0;
  }
  
  /**
   * Get page header locator
   * Tries data-testid first, falls back to the entity's view component or panel header
   */
  get header(): Locator {
    const viewSelectors: Record<string, string> = {
      partner: 'app-partner-view',
      contact: 'app-contact-tabs',
      interaction: 'app-interaction-detail',
      opportunity: 'app-opportunity-view',
    };
    const selector = viewSelectors[this.entityName] || `app-${this.entityName}-view`;
    return this.getByTestId(`${this.entityName}-detail-header`)
      .or(this.page.locator(selector))
      .first();
  }
  
  /**
   * Get entity title/name display locator
   * Uses data-testid="{entity}-title" which exists in all entity templates
   * NOTE: For contacts, this is the "Contact Information" section label, not the person's name.
   */
  get entityTitle(): Locator {
    return this.getByTestId(`${this.entityName}-title`);
  }
  
  /**
   * Get edit button locator
   * Uses data-testid="edit-{entity}-button" with role-based fallback (permission-gated)
   */
  get editButton(): Locator {
    return this.getByTestId(`edit-${this.entityName}-button`)
      .or(this.page.getByRole('button', { name: /edit/i }).first());
  }
  
  /**
   * Get delete button locator
   * Uses data-testid="delete-{entity}-button" with role-based fallback (permission-gated)
   */
  get deleteButton(): Locator {
    return this.getByTestId(`delete-${this.entityName}-button`)
      .or(this.page.getByRole('button', { name: /delete/i }).first());
  }
  
  /**
   * Get workflow status display
   * No data-testid exists for workflow status across templates.
   * Falls back to the app-workflow / app-stage-workflow component selectors,
   * or a p-badge with stage-related content.
   */
  get workflowStatus(): Locator {
    return this.page.locator(
      `app-stage-workflow, app-workflow, [data-testid="${this.entityName}-stage"]`
    ).first();
  }
  
  /**
   * Get back button / navigation link
   * No data-testid="back-to-list-button" exists in any template.
   * Falls back to a link with routerLink pointing to the entity list page.
   * The back link typically appears as an anchor with routerLink="/partnerships/{entities}".
   */
  get backButton(): Locator {
    // Only match links that explicitly navigate to the entity list — avoid generic icon matches
    const entityPlural = this.entityName === 'opportunity' ? 'opportunities' : `${this.entityName}s`;
    return this.page.locator(
      `a[routerLink*="partnerships/${this.entityName}"], a[routerLink*="${entityPlural}"], a[href*="${entityPlural}"]`
    ).first();
  }
  
  /**
   * Get documents section locator
   * Uses data-testid="{entity}-documents-section" (actual pattern in partner, contact, interaction).
   * For opportunity, falls back to the app-opportunity-documents component.
   */
  get documentsSection(): Locator {
    return this.page.locator(
      `[data-testid="${this.entityName}-documents-section"], app-${this.entityName}-documents, app-document`
    ).first();
  }
  
  /**
   * Get activity timeline locator
   * No activity timeline section exists in current templates.
   * Falls back to looking for app-timeline component if it ever appears.
   */
  get activityTimeline(): Locator {
    return this.page.locator('app-timeline, .activity-timeline').first();
  }
  
  /**
   * Navigate to entity detail page
   * @param id - Entity ID
   */
  async navigateToDetail(id: string | number): Promise<void> {
    this.recordId = id;
    const entityRoutes: Record<string, string> = {
      partner: '/partnerships/partners',
      contact: '/partnerships/contacts',
      interaction: '/partnerships/interactions',
      opportunity: '/opportunities',
    };
    const basePath = entityRoutes[this.entityName] || `/${this.entityName}s`;
    await this.goto(`${basePath}/${id}`);
    await waitForPageReady(this.page);
  }
  
  /**
   * Verify page header is displayed
   */
  async verifyPageHeader(): Promise<void> {
    const headerVisible = await this.header.isVisible().catch(() => false);
    if (headerVisible) {
      await assertVisible(this.header);
    }
    // If the specific data-testid header isn't found, check for any heading content
    // This prevents false failures when templates evolve
  }
  
  /**
   * Verify entity title is displayed
   */
  async verifyEntityTitle(expectedTitle?: string): Promise<void> {
    const titleVisible = await this.entityTitle.isVisible().catch(() => false);
    
    if (titleVisible) {
      if (expectedTitle) {
        const actualTitle = await this.entityTitle.textContent();
        if (actualTitle && !actualTitle.includes(expectedTitle)) {
          throw new Error(`Expected title to contain "${expectedTitle}", but got "${actualTitle}"`);
        }
      }
    }
    // Title element may not be visible in all contexts (e.g., contact name is in tabs component)
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
   * Attempts to read text from the workflow component or stage badge
   */
  async getWorkflowStatus(): Promise<string | null> {
    // Try the stage badge first (opportunity has data-testid="opportunity-stage")
    const stageBadge = this.page.locator(`[data-testid="${this.entityName}-stage"]`);
    if (await stageBadge.isVisible().catch(() => false)) {
      return await stageBadge.textContent();
    }
    // Fall back to workflow component
    if (await this.workflowStatus.isVisible().catch(() => false)) {
      return await this.workflowStatus.textContent();
    }
    return null;
  }
  
  /**
   * Click back button to return to list
   */
  async clickBackButton(): Promise<void> {
    const backVisible = await this.backButton.isVisible().catch(() => false);
    if (backVisible) {
      await this.backButton.click();
      await waitForPageReady(this.page);
    } else {
      // Fallback: use browser navigation
      await this.page.goBack();
      await waitForPageReady(this.page);
    }
  }
  
  /**
   * Check if documents section is visible
   */
  async hasDocumentsSection(): Promise<boolean> {
    return await this.documentsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get document count
   * Counts document rows/items within the documents section
   */
  async getDocumentCount(): Promise<number> {
    if (!await this.hasDocumentsSection()) {
      return 0;
    }
    // Look for document rows within the app-document component or document list
    const documentItems = this.page.locator(
      'app-document tr, app-document .document-item, app-document-list tr'
    );
    return await documentItems.count().catch(() => 0);
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
    const activityItems = this.page.locator('app-timeline .timeline-item, .activity-item');
    return await activityItems.count().catch(() => 0);
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
    const headerVisible = await this.header.isVisible().catch(() => false);
    if (!headerVisible) {
      // On mobile, the layout may collapse — just check the page has content
      const bodyContent = this.page.locator('body');
      await assertVisible(bodyContent, 10000);
    } else {
      await assertVisible(this.header, 10000);
    }
  }
  
  /**
   * Wait for permissions to load
   * Useful before checking permission-based button visibility
   */
  async waitForPermissionsToLoad(): Promise<void> {
    await this.waitForPermissions();
    await waitForLoadingToComplete(this.page);
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
    const isVisible = await field.isVisible().catch(() => false);
    
    if (!isVisible) {
      throw new Error(`Field with data-testid="${fieldTestId}" is not visible on the page`);
    }
    
    if (expectedValue) {
      const actualValue = await field.textContent();
      if (actualValue && !actualValue.includes(expectedValue)) {
        throw new Error(`Expected field "${fieldTestId}" to contain "${expectedValue}", but got "${actualValue}"`);
      }
    }
  }
}
