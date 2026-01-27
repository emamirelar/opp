/**
 * @fileoverview Partner Item Page Object
 * Page object for partner detail/item page
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';

export class PartnerItemPage extends EntityDetailPage {
  protected entityName = 'partner';
  
  constructor(page: Page, partnerId?: string | number) {
    super(page, partnerId);
  }
  
  /**
   * Get partner name field
   */
  get partnerName(): Locator {
    return this.getByTestId('partner-name');
  }
  
  /**
   * Get partner type field
   */
  get partnerType(): Locator {
    return this.getByTestId('partner-type');
  }
  
  /**
   * Get partner status field
   */
  get partnerStatus(): Locator {
    return this.getByTestId('partner-status');
  }
  
  /**
   * Get partner description field
   */
  get partnerDescription(): Locator {
    return this.getByTestId('partner-description');
  }
  
  /**
   * Get partner website field
   */
  get partnerWebsite(): Locator {
    return this.getByTestId('partner-website');
  }
  
  /**
   * Get contacts tab/section
   */
  get contactsSection(): Locator {
    return this.getByTestId('partner-contacts-section');
  }
  
  /**
   * Get interactions tab/section
   */
  get interactionsSection(): Locator {
    return this.getByTestId('partner-interactions-section');
  }
  
  /**
   * Get opportunities tab/section
   */
  get opportunitiesSection(): Locator {
    return this.getByTestId('partner-opportunities-section');
  }
  
  /**
   * Get partner tree navigation button
   */
  get partnerTreeButton(): Locator {
    return this.getByTestId('view-partner-tree-button');
  }
  
  /**
   * Navigate to partner detail page
   */
  async navigate(partnerId: string | number): Promise<void> {
    await this.navigateToDetail(partnerId);
  }
  
  /**
   * Verify partner name is displayed
   */
  async verifyPartnerName(expectedName?: string): Promise<void> {
    await assertVisible(this.partnerName);
    
    if (expectedName) {
      const actualName = await this.partnerName.textContent();
      if (actualName && !actualName.includes(expectedName)) {
        throw new Error(`Expected partner name to contain "${expectedName}", but got "${actualName}"`);
      }
    }
  }
  
  /**
   * Verify partner type is displayed
   */
  async verifyPartnerType(expectedType?: string): Promise<void> {
    await assertVisible(this.partnerType);
    
    if (expectedType) {
      const actualType = await this.partnerType.textContent();
      if (actualType && !actualType.includes(expectedType)) {
        throw new Error(`Expected partner type to contain "${expectedType}", but got "${actualType}"`);
      }
    }
  }
  
  /**
   * Get partner information
   */
  async getPartnerInfo(): Promise<{
    name: string | null;
    type: string | null;
    status: string | null;
    description: string | null;
    website: string | null;
  }> {
    return {
      name: await this.partnerName.textContent(),
      type: await this.partnerType.textContent(),
      status: await this.partnerStatus.textContent().catch(() => null),
      description: await this.partnerDescription.textContent().catch(() => null),
      website: await this.partnerWebsite.textContent().catch(() => null),
    };
  }
  
  /**
   * Check if contacts section is visible
   */
  async hasContactsSection(): Promise<boolean> {
    return await this.contactsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get contacts count
   */
  async getContactsCount(): Promise<number> {
    if (!await this.hasContactsSection()) {
      return 0;
    }
    
    const contactItems = this.page.locator('[data-testid="partner-contact-item"]');
    return await contactItems.count();
  }
  
  /**
   * Check if interactions section is visible
   */
  async hasInteractionsSection(): Promise<boolean> {
    return await this.interactionsSection.isVisible().catch(() => false);
  }
  
  /**
   * Get interactions count
   */
  async getInteractionsCount(): Promise<number> {
    if (!await this.hasInteractionsSection()) {
      return 0;
    }
    
    const interactionItems = this.page.locator('[data-testid="partner-interaction-item"]');
    return await interactionItems.count();
  }
  
  /**
   * Check if opportunities section is visible
   */
  async hasOpportunitiesSection(): Promise<boolean> {
    return await this.opportunitiesSection.isVisible().catch(() => false);
  }
  
  /**
   * Get opportunities count
   */
  async getOpportunitiesCount(): Promise<number> {
    if (!await this.hasOpportunitiesSection()) {
      return 0;
    }
    
    const opportunityItems = this.page.locator('[data-testid="partner-opportunity-item"]');
    return await opportunityItems.count();
  }
  
  /**
   * Click partner tree button
   */
  async clickPartnerTreeButton(): Promise<void> {
    if (await this.partnerTreeButton.isVisible().catch(() => false)) {
      await this.partnerTreeButton.click();
      await this.page.waitForTimeout(1000);
    }
  }
  
  /**
   * Verify all main sections are displayed
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyPartnerName();
    await this.verifyPartnerType();
  }
}
