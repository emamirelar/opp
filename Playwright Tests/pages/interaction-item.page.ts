/**
 * @fileoverview Interaction Item Page Object
 * Page object for interaction detail/item page
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';

export class InteractionItemPage extends EntityDetailPage {
  protected entityName = 'interaction';
  
  constructor(page: Page, interactionId?: string | number) {
    super(page, interactionId);
  }
  
  /**
   * Get interaction type field
   */
  get interactionType(): Locator {
    return this.getByTestId('interaction-type');
  }
  
  /**
   * Get interaction date field
   */
  get interactionDate(): Locator {
    return this.getByTestId('interaction-date');
  }
  
  /**
   * Get interaction description/notes field
   */
  get interactionDescription(): Locator {
    return this.getByTestId('interaction-description');
  }
  
  /**
   * Get interaction location field
   */
  get interactionLocation(): Locator {
    return this.getByTestId('interaction-location');
  }
  
  /**
   * Get participants section
   */
  get participantsSection(): Locator {
    return this.getByTestId('interaction-participants-section');
  }
  
  /**
   * Get related opportunities section
   */
  get relatedOpportunitiesSection(): Locator {
    return this.getByTestId('interaction-opportunities-section');
  }
  
  /**
   * Get create opportunity button
   */
  get createOpportunityButton(): Locator {
    return this.getByTestId('create-opportunity-from-interaction-button');
  }
  
  /**
   * Navigate to interaction detail page
   */
  async navigate(interactionId: string | number): Promise<void> {
    await this.navigateToDetail(interactionId);
  }
  
  /**
   * Verify interaction type is displayed
   */
  async verifyInteractionType(expectedType?: string): Promise<void> {
    await assertVisible(this.interactionType);
    
    if (expectedType) {
      const actualType = await this.interactionType.textContent();
      if (actualType && !actualType.includes(expectedType)) {
        throw new Error(`Expected interaction type to contain "${expectedType}", but got "${actualType}"`);
      }
    }
  }
  
  /**
   * Verify interaction date is displayed
   */
  async verifyInteractionDate(expectedDate?: string): Promise<void> {
    await assertVisible(this.interactionDate);
    
    if (expectedDate) {
      const actualDate = await this.interactionDate.textContent();
      if (actualDate && !actualDate.includes(expectedDate)) {
        throw new Error(`Expected interaction date to contain "${expectedDate}", but got "${actualDate}"`);
      }
    }
  }
  
  /**
   * Get interaction information
   */
  async getInteractionInfo(): Promise<{
    type: string | null;
    date: string | null;
    description: string | null;
    location: string | null;
  }> {
    return {
      type: await this.interactionType.textContent(),
      date: await this.interactionDate.textContent(),
      description: await this.interactionDescription.textContent().catch(() => null),
      location: await this.interactionLocation.textContent().catch(() => null),
    };
  }
  
  /**
   * Check if participants section is visible
   */
  async hasParticipantsSection(): Promise<boolean> {
    return await this.participantsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get participants count
   */
  async getParticipantsCount(): Promise<number> {
    if (!await this.hasParticipantsSection()) {
      return 0;
    }
    
    const participantItems = this.page.locator('[data-testid="interaction-participant-item"]');
    return await participantItems.count();
  }
  
  /**
   * Check if related opportunities section is visible
   */
  async hasRelatedOpportunitiesSection(): Promise<boolean> {
    return await this.relatedOpportunitiesSection.isVisible().catch(() => false);
  }
  
  /**
   * Get related opportunities count
   */
  async getRelatedOpportunitiesCount(): Promise<number> {
    if (!await this.hasRelatedOpportunitiesSection()) {
      return 0;
    }
    
    const opportunityItems = this.page.locator('[data-testid="interaction-opportunity-item"]');
    return await opportunityItems.count();
  }
  
  /**
   * Check if create opportunity button is visible
   */
  async isCreateOpportunityButtonVisible(): Promise<boolean> {
    return await this.createOpportunityButton.isVisible().catch(() => false);
  }
  
  /**
   * Click create opportunity button
   */
  async clickCreateOpportunityButton(): Promise<void> {
    if (await this.isCreateOpportunityButtonVisible()) {
      await this.createOpportunityButton.click();
      await this.page.waitForTimeout(1000);
    }
  }
  
  /**
   * Verify all main sections are displayed
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyInteractionType();
    await this.verifyInteractionDate();
  }
}
