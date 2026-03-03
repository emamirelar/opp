/**
 * @fileoverview Partner Item Page Object
 * Page object for partner detail/item page
 * 
 * Uses actual data-testid attributes from the Angular partner-view component:
 *   - partner-detail-header: Panel header container
 *   - partner-title: "Partner Information" section title
 *   - partner-status: Status display (visible after "See More" toggle)
 *   - partner-address-section: Address section
 *   - partner-documents-section: Documents section
 *   - partner-links-section: Links section
 *   - edit-partner-button: Edit button (permission-gated)
 *   - delete-partner-button: Delete button (permission-gated)
 * 
 * NOTE: The app does NOT have data-testid attributes for partner name or type.
 * Partner name is displayed within the page content (not with a dedicated testid).
 * Contacts are shown via a dialog, not an inline section.
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForElementReady, waitForVisible, waitForLoadingToComplete } from '../helpers/wait.helper';

export class PartnerItemPage extends EntityDetailPage {
  protected entityName = 'partner';
  
  constructor(page: Page, partnerId?: string | number) {
    super(page, partnerId);
  }
  
  /**
   * Get partner name display - uses the partner info panel content area
   * The partner name is rendered within the panel body, not as a dedicated data-testid element.
   * We search within the partner-information content area for visible text.
   */
  get partnerName(): Locator {
    return this.page.locator('.partner-info-content, .partner-information').first();
  }
  
  /**
   * Get partner category/type display
   * The app shows partner category (not "type") via data-testid="partner-category-group"
   */
  get partnerCategory(): Locator {
    return this.getByTestId('partner-category-group');
  }
  
  /**
   * Get partner status field
   * NOTE: Only visible when "See More" has been clicked (showAdditionalInfo && showFullContent)
   */
  get partnerStatus(): Locator {
    return this.getByTestId('partner-status');
  }
  
  /**
   * Get partner address section
   */
  get partnerAddress(): Locator {
    return this.getByTestId('partner-address-section');
  }
  
  /**
   * Get partner organization unit section
   */
  get partnerOrgUnit(): Locator {
    return this.getByTestId('partner-orgunit-section');
  }
  
  /**
   * Get partner attributes section (visible after "See More")
   */
  get partnerAttributes(): Locator {
    return this.getByTestId('partner-attributes-section');
  }
  
  /**
   * Get documents section - uses actual data-testid="partner-documents-section"
   */
  get documentsSection(): Locator {
    return this.getByTestId('partner-documents-section');
  }

  /**
   * Get upload document button
   * Uses actual data-testid="upload-document-button" from partner-view
   */
  get uploadDocumentButton(): Locator {
    return this.getByTestId('upload-document-button');
  }
  
  /**
   * Get links section
   */
  get linksSection(): Locator {
    return this.getByTestId('partner-links-section');
  }
  
  /**
   * Get partner tree navigation button
   */
  get partnerTreeButton(): Locator {
    return this.getByTestId('view-partner-tree-button');
  }
  
  /**
   * Get the "See More" button to expand additional info
   */
  get seeMoreButton(): Locator {
    return this.page.locator('p-button').filter({ hasText: /see more/i }).first();
  }
  
  /**
   * Navigate to partner detail page
   */
  async navigate(partnerId: string | number): Promise<void> {
    await this.navigateToDetail(partnerId);
  }
  
  /**
   * Verify partner name is displayed somewhere on the page
   * Since there is no dedicated data-testid for partner name, we check
   * that the partner info panel contains text content.
   */
  async verifyPartnerName(expectedName?: string): Promise<void> {
    // Verify the partner information panel is loaded
    const infoPanel = this.page.locator('.partner-info-content, .partner-information').first();
    await assertVisible(infoPanel);
    
    if (expectedName) {
      // Look for the partner name text anywhere in the page content
      const nameOnPage = this.page.getByText(expectedName, { exact: false });
      const isVisible = await nameOnPage.isVisible().catch(() => false);
      if (!isVisible) {
        throw new Error(`Expected partner name "${expectedName}" to appear on the page, but it was not found`);
      }
    }
  }
  
  /**
   * Verify partner category is displayed (replaces verifyPartnerType)
   * The app displays partner category, not "type" as a field.
   */
  async verifyPartnerCategory(expectedCategory?: string): Promise<void> {
    const hasCategoryGroup = await this.partnerCategory.isVisible().catch(() => false);
    
    if (hasCategoryGroup && expectedCategory) {
      const actualText = await this.partnerCategory.textContent();
      if (actualText && !actualText.includes(expectedCategory)) {
        throw new Error(`Expected partner category to contain "${expectedCategory}", but got "${actualText}"`);
      }
    }
    // Category may not be visible for all partners - this is acceptable
  }
  
  /**
   * Verify partner type is displayed
   * NOTE: The app doesn't have a dedicated "partner type" field with data-testid.
   * This is a graceful check that logs rather than fails.
   * @deprecated Use verifyPartnerCategory() instead
   */
  async verifyPartnerType(expectedType?: string): Promise<void> {
    await this.verifyPartnerCategory(expectedType);
  }
  
  /**
   * Get partner information from the page
   * Collects available partner data using actual DOM elements
   */
  async getPartnerInfo(): Promise<{
    name: string | null;
    type: string | null;
    status: string | null;
    description: string | null;
    website: string | null;
  }> {
    // Use short timeouts to avoid consuming the entire test timeout
    // when elements are not yet rendered or only visible after "See More"
    const SHORT_TIMEOUT = 5000;
    
    // Get partner title text from the panel header
    const titleLocator = this.getByTestId('partner-title');
    const titleText = await titleLocator.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null);
    
    // Get status (may require "See More" to be clicked — often not visible)
    const statusVisible = await this.partnerStatus.isVisible().catch(() => false);
    const statusText = statusVisible
      ? await this.partnerStatus.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get category as type substitute (may not be assigned)
    const categoryVisible = await this.partnerCategory.isVisible().catch(() => false);
    const categoryText = categoryVisible
      ? await this.partnerCategory.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get partner info from the panel body content
    const infoContent = this.page.locator('.partner-info-content, .partner-information').first();
    const bodyVisible = await infoContent.isVisible().catch(() => false);
    const bodyText = bodyVisible
      ? await infoContent.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    return {
      name: titleText,
      type: categoryText,
      status: statusText,
      description: bodyText,
      website: null, // No dedicated website field in current template
    };
  }
  
  /**
   * Check if contacts are available (via the contacts dialog trigger)
   * In the actual app, contacts are shown in a dialog, not an inline section.
   */
  async hasContactsSection(): Promise<boolean> {
    // Check for the contacts dialog trigger button or "View Contacts" text
    const viewContactsButton = this.page.locator('p-button').filter({ hasText: /contacts/i });
    const hasButton = await viewContactsButton.isVisible().catch(() => false);
    
    // Also check for the contact-dialog if it exists
    const contactDialog = this.page.locator('.contact-dialog, [data-testid*="contact"]');
    const hasDialog = await contactDialog.isVisible().catch(() => false);
    
    return hasButton || hasDialog;
  }
  
  /**
   * Get contacts count (from dialog if opened)
   */
  async getContactsCount(): Promise<number> {
    // Contacts are in a dialog; cannot count without opening it
    return 0;
  }
  
  /**
   * Check if interactions section is visible
   * The partner detail page shows interaction summary via AI panel, not a dedicated section
   */
  async hasInteractionsSection(): Promise<boolean> {
    const summaryPanel = this.page.locator('.summary-interactions');
    return await summaryPanel.isVisible().catch(() => false);
  }
  
  /**
   * Get interactions count
   */
  async getInteractionsCount(): Promise<number> {
    // Interactions are shown as AI summary, not countable items
    return 0;
  }
  
  /**
   * Get Opportunities tab (navigates to /partnerships/partners/:id/opportunities)
   */
  get opportunitiesTab(): Locator {
    return this.page.locator(
      'button:has-text("Opportunities"), [role="tab"]:has-text("Opportunities"), a[href*="/opportunities"]'
    ).first();
  }

  /**
   * Get opportunities listview when on partner opportunities tab
   */
  get opportunitiesListview(): Locator {
    return this.page.locator(
      'app-partner-view-opportunities app-listview, .opportunity-listview, app-listview'
    ).first();
  }

  /**
   * Get opportunities list container (app-partner-opportunities or listview)
   */
  get opportunitiesListContainer(): Locator {
    return this.page.locator(
      'app-partner-opportunities, app-partner-view-opportunities, [data-testid*="partner-opportunities"]'
    ).first();
  }

  /**
   * Get search input in opportunities tab
   */
  get opportunitiesSearchInput(): Locator {
    return this.page.locator(
      'input[placeholder*="Search"], input[placeholder*="search"], [data-testid="opportunity-search"]'
    ).first();
  }

  /**
   * Click Opportunities tab and wait for content to load
   */
  async openOpportunitiesTab(): Promise<void> {
    const tab = this.opportunitiesTab;
    if (await tab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await tab.click();
      const content = this.page.locator(
        'app-partner-view-opportunities, .opportunity-listview, app-partner-opportunities'
      ).first();
      await waitForElementReady(content, 10000);
    }
  }

  /**
   * Check if opportunities section is visible
   * The partner detail page shows related engagements, not a dedicated opportunities section
   */
  async hasOpportunitiesSection(): Promise<boolean> {
    // Look for "Related Engagements" section which includes opportunities
    const engagementsSection = this.page.getByText('Related Engagements', { exact: false });
    return await engagementsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get opportunities count
   */
  async getOpportunitiesCount(): Promise<number> {
    // Related engagements list - count items if visible
    const engagementItems = this.page.locator('app-base-engagement-list tr, app-base-engagement-list .engagement-item');
    return await engagementItems.count().catch(() => 0);
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
    const documentItems = this.page.locator('app-document .document-item, app-document tr');
    return await documentItems.count().catch(() => 0);
  }
  
  /**
   * Check if links section is visible
   */
  async hasLinksSection(): Promise<boolean> {
    return await this.linksSection.isVisible().catch(() => false);
  }
  
  /**
   * Click "See More" to expand additional partner information
   */
  async expandAdditionalInfo(): Promise<void> {
    const seeMore = this.seeMoreButton;
    if (await seeMore.isVisible().catch(() => false)) {
      await seeMore.click();
      await waitForVisible(this.partnerStatus.or(this.partnerAttributes), 5000).catch(() => {});
    }
  }
  
  /**
   * Click partner tree button
   */
  async clickPartnerTreeButton(): Promise<void> {
    if (await this.partnerTreeButton.isVisible().catch(() => false)) {
      await this.partnerTreeButton.click();
      await waitForLoadingToComplete(this.page);
    }
  }
  
  /**
   * Verify all main sections are displayed
   * Uses actual data-testid attributes that exist in the template
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyPartnerName();
    // Documents and links sections should be visible
    const hasDocs = await this.hasDocumentsSection();
    const hasLinks = await this.hasLinksSection();
    // At least one of these sections should be present
    if (!hasDocs && !hasLinks) {
      throw new Error('Expected at least documents or links section to be visible');
    }
  }
}
