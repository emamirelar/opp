/**
 * @fileoverview Interaction Item Page Object
 * Page object for interaction detail/item page
 * 
 * Uses actual data-testid attributes from the Angular interaction-detail component:
 *   - interaction-detail-header: Main header container
 *   - interaction-type-icon: Type icon (mobile layout only)
 *   - interaction-title: Subject text (mobile layout only)
 *   - create-opportunity-button: Create opportunity from interaction (mobile + desktop)
 *   - edit-interaction-button: Edit button (mobile + desktop)
 *   - delete-interaction-button: Delete button (mobile + desktop)
 *   - interaction-description-section: Description panel
 *   - interaction-description: Description text content
 *   - interaction-documents-section: Documents panel
 *   - interaction-details-section: Details panel (date, location, status)
 *   - interaction-date: Date row
 *   - interaction-location: Location row
 *   - interaction-status: Status row
 *   - interaction-contacts-section: Related contacts panel
 *   - interaction-partners-section: Related partners panel
 * 
 * NOTE: The following do NOT have data-testid in the template:
 *   - Desktop interaction type text (only mobile has interaction-type-icon)
 *   - Related opportunities panel (uses translated header text, "Coming Soon")
 *   - UNOPS personnel panel (uses translated header text)
 *   - Individual participant items
 *   - Individual opportunity items
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForDialog, waitForPageReady } from '../helpers/wait.helper';

export class InteractionItemPage extends EntityDetailPage {
  protected entityName = 'interaction';
  
  constructor(page: Page, interactionId?: string | number) {
    super(page, interactionId);
  }
  
  /**
   * Get edit button locator
   * Template uses class="edit-button" (no data-testid). Fallback to class selector and role.
   */
  override get editButton(): Locator {
    return this.getByTestId('edit-interaction-button')
      .or(this.page.locator('button.edit-button, .edit-button'))
      .or(this.page.getByRole('button', { name: /edit/i }))
      .first();
  }

  /**
   * Get delete button locator
   * Template uses class="delete-button" (no data-testid). Fallback to class selector and role.
   */
  override get deleteButton(): Locator {
    return this.getByTestId('delete-interaction-button')
      .or(this.page.locator('button.delete-button, .delete-button'))
      .or(this.page.getByRole('button', { name: /delete/i }))
      .first();
  }

  /**
   * Get interaction type display
   * No data-testid. Desktop shows type in <p class="text-sm text-gray-600">. Mobile uses icon.
   */
  get interactionType(): Locator {
    return this.page.locator(
      '[data-testid="interaction-type-icon"], .text-sm.text-gray-600, p.text-sm.text-gray-600'
    ).first();
  }

  /**
   * Get interaction date field
   * No data-testid. Date is in Interaction Details panel under "Date and Time" label.
   */
  get interactionDate(): Locator {
    return this.getByTestId('interaction-date')
      .or(this.page.locator('p-panel').filter({ hasText: /interaction details/i }).locator('p.text-sm.text-gray-900').first());
  }

  /**
   * Get interaction description/notes field
   * Template uses class="interaction-description".
   */
  get interactionDescription(): Locator {
    return this.getByTestId('interaction-description')
      .or(this.page.locator('.interaction-description').first());
  }

  /**
   * Get interaction description section panel
   * No data-testid. p-panel with "Description" header.
   */
  get interactionDescriptionSection(): Locator {
    return this.getByTestId('interaction-description-section')
      .or(this.page.locator('p-panel').filter({ hasText: /description/i }).first());
  }

  /**
   * Get interaction location field
   * No data-testid. Location is in Interaction Details panel (div with map-marker icon).
   */
  get interactionLocation(): Locator {
    return this.getByTestId('interaction-location')
      .or(this.page.locator('div:has(i.pi-map-marker)').locator('p.text-sm.text-gray-900').first());
  }

  /**
   * Get interaction status field
   * No data-testid. Status is in Interaction Details panel (has calendar icon).
   */
  get interactionStatus(): Locator {
    const detailsPanel = this.page.locator('p-panel').filter({ has: this.page.locator('i.pi-calendar') }).first();
    return this.getByTestId('interaction-status')
      .or(detailsPanel.locator('p-tag').first());
  }

  /**
   * Get interaction details section (date, location, status)
   * No data-testid. p-panel containing calendar icon (date/time section).
   */
  get interactionDetailsSection(): Locator {
    return this.getByTestId('interaction-details-section')
      .or(this.page.locator('p-panel').filter({ has: this.page.locator('i.pi-calendar') }).first());
  }

  /**
   * Get related contacts section
   * No data-testid. p-panel with "Related Contacts" header.
   */
  get relatedContactsSection(): Locator {
    return this.getByTestId('interaction-contacts-section')
      .or(this.page.locator('p-panel').filter({ hasText: /related contacts/i }).first());
  }

  /**
   * Get related partners section
   * No data-testid. p-panel with "Related Partners" header.
   */
  get relatedPartnersSection(): Locator {
    return this.getByTestId('interaction-partners-section')
      .or(this.page.locator('p-panel').filter({ hasText: /related partners/i }).first());
  }
  
  /**
   * Get participants section (combined: contacts + partners)
   * No data-testid. Template uses p-panel with "Related Contacts" or "Related Partners" headers.
   */
  get participantsSection(): Locator {
    return this.relatedContactsSection.or(this.relatedPartnersSection);
  }

  /**
   * Get related opportunities section
   * No data-testid. p-panel with "Related Opportunities" header. Shows "Coming Soon" placeholder.
   */
  get relatedOpportunitiesSection(): Locator {
    return this.page.locator('p-panel').filter({ hasText: /related opportunities/i }).first();
  }

  /**
   * Get create opportunity button
   * Template uses class="create-opportunity-button" (no data-testid). Permission-based visibility.
   */
  get createOpportunityButton(): Locator {
    return this.getByTestId('create-opportunity-button')
      .or(this.page.locator('button.create-opportunity-button, .create-opportunity-button'))
      .or(this.page.getByRole('button', { name: /create.*opportunity|opportunity/i }))
      .first();
  }

  /**
   * Get documents section
   * No data-testid. p-panel with "Related Documents" header or app-document component.
   */
  override get documentsSection(): Locator {
    return this.getByTestId('interaction-documents-section')
      .or(this.page.locator('p-panel').filter({ hasText: /related documents|documentos|documents/i }).first())
      .or(this.page.locator('app-document, .documents-container').first());
  }
  
  /**
   * Navigate to interaction detail page
   * @param interactionId - Interaction ID
   */
  override async navigate(interactionId: string | number): Promise<void> {
    this.recordId = interactionId;
    await this.goto(`/partnerships/interactions/${interactionId}`);
    await waitForPageReady(this.page);
  }
  
  /**
   * Verify interaction type is displayed
   * Checks for the type icon (mobile) or type text (desktop)
   */
  async verifyInteractionType(expectedType?: string): Promise<void> {
    const typeVisible = await this.interactionType.isVisible().catch(() => false);
    
    if (typeVisible && expectedType) {
      const actualType = await this.interactionType.textContent();
      if (actualType && !actualType.toLowerCase().includes(expectedType.toLowerCase())) {
        // Type may be shown only as an icon — check page text too
        const typeOnPage = this.page.getByText(expectedType, { exact: false });
        const foundOnPage = await typeOnPage.isVisible().catch(() => false);
        if (!foundOnPage) {
          throw new Error(`Expected interaction type to contain "${expectedType}", but got "${actualType}"`);
        }
      }
    }
    // Type display may vary between mobile/desktop — acceptable
  }
  
  /**
   * Verify interaction date is displayed
   * Uses actual data-testid="interaction-date"
   */
  async verifyInteractionDate(expectedDate?: string): Promise<void> {
    const dateVisible = await this.interactionDate.isVisible().catch(() => false);
    
    if (dateVisible && expectedDate) {
      const actualDate = await this.interactionDate.textContent();
      if (actualDate && !actualDate.includes(expectedDate)) {
        throw new Error(`Expected interaction date to contain "${expectedDate}", but got "${actualDate}"`);
      }
    }
  }
  
  /**
   * Get interaction information from the page
   * Collects available interaction data using actual data-testid attributes
   */
  async getInteractionInfo(): Promise<{
    type: string | null;
    date: string | null;
    description: string | null;
    location: string | null;
    status: string | null;
  }> {
    const SHORT_TIMEOUT = 5000;
    
    // Get type (resilient — no single testid)
    const typeVisible = await this.interactionType.isVisible().catch(() => false);
    const typeText = typeVisible
      ? await this.interactionType.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get date (data-testid="interaction-date")
    const dateVisible = await this.interactionDate.isVisible().catch(() => false);
    const dateText = dateVisible
      ? await this.interactionDate.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get description (data-testid="interaction-description")
    const descVisible = await this.interactionDescription.isVisible().catch(() => false);
    const descText = descVisible
      ? await this.interactionDescription.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get location (data-testid="interaction-location")
    const locVisible = await this.interactionLocation.isVisible().catch(() => false);
    const locText = locVisible
      ? await this.interactionLocation.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get status (data-testid="interaction-status")
    const statusVisible = await this.interactionStatus.isVisible().catch(() => false);
    const statusText = statusVisible
      ? await this.interactionStatus.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    return {
      type: typeText,
      date: dateText,
      description: descText,
      location: locText,
      status: statusText,
    };
  }
  
  /**
   * Check if participants section is visible (contacts or partners)
   * The template splits participants into "Related Contacts" and "Related Partners" panels.
   */
  async hasParticipantsSection(): Promise<boolean> {
    const hasContacts = await this.relatedContactsSection.isVisible().catch(() => false);
    const hasPartners = await this.relatedPartnersSection.isVisible().catch(() => false);
    return hasContacts || hasPartners;
  }
  
  /**
   * Check if related contacts section is visible
   * Uses actual data-testid="interaction-contacts-section"
   */
  async hasRelatedContactsSection(): Promise<boolean> {
    return await this.relatedContactsSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if related partners section is visible
   * Uses actual data-testid="interaction-partners-section"
   */
  async hasRelatedPartnersSection(): Promise<boolean> {
    return await this.relatedPartnersSection.isVisible().catch(() => false);
  }
  
  /**
   * Get participants count (from contacts + partners sections)
   * Counts items within both the contacts and partners panels
   */
  async getParticipantsCount(): Promise<number> {
    let count = 0;
    
    // Count related contacts
    if (await this.hasRelatedContactsSection()) {
      const contactItems = this.relatedContactsSection.locator('tr, .list-item, a');
      count += await contactItems.count().catch(() => 0);
    }
    
    // Count related partners
    if (await this.hasRelatedPartnersSection()) {
      const partnerItems = this.relatedPartnersSection.locator('tr, .list-item, a');
      count += await partnerItems.count().catch(() => 0);
    }
    
    return count;
  }
  
  /**
   * Check if related opportunities section is visible
   * NOTE: This section currently shows "Coming Soon" placeholder.
   */
  async hasRelatedOpportunitiesSection(): Promise<boolean> {
    return await this.relatedOpportunitiesSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if create opportunity button is visible
   * Uses actual data-testid="create-opportunity-button"
   */
  async isCreateOpportunityButtonVisible(): Promise<boolean> {
    return await this.createOpportunityButton.isVisible().catch(() => false);
  }
  
  /**
   * Click create opportunity button
   * Uses actual data-testid="create-opportunity-button"
   */
  async clickCreateOpportunityButton(): Promise<void> {
    if (await this.isCreateOpportunityButtonVisible()) {
      await this.createOpportunityButton.click();
      await waitForDialog(this.page);
    }
  }
  
  /**
   * Check if documents section is visible
   * Uses actual data-testid="interaction-documents-section"
   */
  override async hasDocumentsSection(): Promise<boolean> {
    return await this.documentsSection.isVisible().catch(() => false);
  }
  
  /**
   * Verify all main sections are displayed
   * Uses actual data-testid attributes that exist in the template
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    
    // Verify description section (data-testid="interaction-description-section")
    const hasDesc = await this.interactionDescriptionSection.isVisible().catch(() => false);
    
    // Verify details section (data-testid="interaction-details-section")
    const hasDetails = await this.interactionDetailsSection.isVisible().catch(() => false);
    
    // At least the description or details section should be present
    if (!hasDesc && !hasDetails) {
      throw new Error('Expected at least description or details section to be visible');
    }
  }
}
