/**
 * @fileoverview Contact Item Page Object
 * Page object for contact detail/item page
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';

export class ContactItemPage extends EntityDetailPage {
  protected entityName = 'contact';
  
  constructor(page: Page, contactId?: string | number) {
    super(page, contactId);
  }
  
  /**
   * Get contact name field
   */
  get contactName(): Locator {
    return this.getByTestId('contact-name');
  }
  
  /**
   * Get contact email field
   */
  get contactEmail(): Locator {
    return this.getByTestId('contact-email');
  }
  
  /**
   * Get contact phone field
   */
  get contactPhone(): Locator {
    return this.getByTestId('contact-phone');
  }
  
  /**
   * Get contact title/position field
   */
  get contactTitle(): Locator {
    return this.getByTestId('contact-title');
  }
  
  /**
   * Get contact partner field
   */
  get contactPartner(): Locator {
    return this.getByTestId('contact-partner');
  }
  
  /**
   * Get contact department field
   */
  get contactDepartment(): Locator {
    return this.getByTestId('contact-department');
  }
  
  /**
   * Get interactions tab/section
   */
  get interactionsSection(): Locator {
    return this.getByTestId('contact-interactions-section');
  }
  
  /**
   * Get opportunities tab/section
   */
  get opportunitiesSection(): Locator {
    return this.getByTestId('contact-opportunities-section');
  }
  
  /**
   * Navigate to contact detail page
   */
  async navigate(contactId: string | number): Promise<void> {
    await this.navigateToDetail(contactId);
  }
  
  /**
   * Verify contact name is displayed
   */
  async verifyContactName(expectedName?: string): Promise<void> {
    await assertVisible(this.contactName);
    
    if (expectedName) {
      const actualName = await this.contactName.textContent();
      if (actualName && !actualName.includes(expectedName)) {
        throw new Error(`Expected contact name to contain "${expectedName}", but got "${actualName}"`);
      }
    }
  }
  
  /**
   * Verify contact email is displayed
   */
  async verifyContactEmail(expectedEmail?: string): Promise<void> {
    await assertVisible(this.contactEmail);
    
    if (expectedEmail) {
      const actualEmail = await this.contactEmail.textContent();
      if (actualEmail && !actualEmail.includes(expectedEmail)) {
        throw new Error(`Expected contact email to contain "${expectedEmail}", but got "${actualEmail}"`);
      }
    }
  }
  
  /**
   * Verify associated partner is displayed
   */
  async verifyContactPartner(expectedPartner?: string): Promise<void> {
    await assertVisible(this.contactPartner);
    
    if (expectedPartner) {
      const actualPartner = await this.contactPartner.textContent();
      if (actualPartner && !actualPartner.includes(expectedPartner)) {
        throw new Error(`Expected partner to contain "${expectedPartner}", but got "${actualPartner}"`);
      }
    }
  }
  
  /**
   * Get contact information
   */
  async getContactInfo(): Promise<{
    name: string | null;
    email: string | null;
    phone: string | null;
    title: string | null;
    partner: string | null;
    department: string | null;
  }> {
    return {
      name: await this.contactName.textContent(),
      email: await this.contactEmail.textContent(),
      phone: await this.contactPhone.textContent().catch(() => null),
      title: await this.contactTitle.textContent().catch(() => null),
      partner: await this.contactPartner.textContent(),
      department: await this.contactDepartment.textContent().catch(() => null),
    };
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
    
    const interactionItems = this.page.locator('[data-testid="contact-interaction-item"]');
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
    
    const opportunityItems = this.page.locator('[data-testid="contact-opportunity-item"]');
    return await opportunityItems.count();
  }
  
  /**
   * Verify all main sections are displayed
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyContactName();
    await this.verifyContactEmail();
    await this.verifyContactPartner();
  }
}
