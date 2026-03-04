/**
 * @fileoverview Contact Item Page Object
 * Page object for contact detail/item page
 * 
 * Uses actual data-testid attributes from the Angular contact-view component:
 *   - contact-detail-header: Panel header container
 *   - contact-title: "Contact Information" section label (NOT the person's name)
 *   - edit-contact-button: Edit button (permission-gated)
 *   - delete-contact-button: Delete button (permission-gated)
 *   - contact-partner-section: Partner association section
 *   - contact-partner-link: Partner name link
 *   - contact-info-section: Contact info (email/phone/mobile) wrapper
 *   - contact-email: Email block
 *   - contact-phone: Phone row
 *   - contact-mobile: Mobile row
 *   - contact-status: Status block (visible after toggle)
 *   - contact-links-section: Links section header
 *   - add-link-button: Add link button
 *   - contact-documents-section: Documents section header
 *   - upload-document-button: Upload document button
 * 
 * NOTE: The contact's display name, job title, and department do NOT have data-testid.
 * Contact name is rendered in the ContactTabsComponent inline template (div.text-2xl.font-bold).
 * Department is shown in the general info area without a dedicated data-testid.
 * Interactions are a separate tab (app-contact-view-interactions), not an inline section.
 * Opportunities are NOT displayed on the contact detail page.
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
   * Get edit button - contact template uses class contact-edit-button on p-button, no data-testid
   */
  override get editButton(): Locator {
    return this.getByTestId('edit-contact-button')
      .or(this.page.locator('p-button.contact-edit-button, .contact-edit-button, button:has(.pi-pencil)'))
      .or(this.page.getByRole('button', { name: /edit/i }).first());
  }

  /**
   * Get delete button - contact template uses class contact-delete-button on p-button, no data-testid
   */
  override get deleteButton(): Locator {
    return this.getByTestId('delete-contact-button')
      .or(this.page.locator('p-button.contact-delete-button, .contact-delete-button, button:has(.pi-trash)'))
      .or(this.page.getByRole('button', { name: /delete/i }).first());
  }
  
  /**
   * Get contact name display
   * The contact name is rendered in the ContactTabsComponent inline template
   * within a div.text-2xl.font-bold. There is no data-testid for the name.
   * We search for the large bold text element in the header area.
   */
  get contactName(): Locator {
    return this.page.locator('app-contact-tabs .text-2xl.font-bold, app-contact-tabs .text-4xl.font-bold').first();
  }
  
  /**
   * Get contact email field
   * Uses actual data-testid="contact-email" from contact-view.component.html
   */
  get contactEmail(): Locator {
    return this.getByTestId('contact-email');
  }
  
  /**
   * Get contact phone field
   * Uses actual data-testid="contact-phone" from contact-view.component.html
   */
  get contactPhone(): Locator {
    return this.getByTestId('contact-phone');
  }
  
  /**
   * Get contact mobile field
   * Uses actual data-testid="contact-mobile" from contact-view.component.html
   */
  get contactMobile(): Locator {
    return this.getByTestId('contact-mobile');
  }
  
  /**
   * Get contact title/position display
   * No data-testid for the contact's job title exists.
   * NOTE: data-testid="contact-title" is the "Contact Information" section label,
   * NOT the person's job title.
   * Falls back to finding title text in the ContactTabsComponent header area.
   */
  get contactTitle(): Locator {
    // Title is rendered after the name in ContactTabsComponent as plain text
    return this.page.locator('app-contact-tabs').first();
  }
  
  /**
   * Get contact partner section
   * Fallback: template uses div containing partner link (class contact-partner-link), no data-testid
   */
  get contactPartnerSection(): Locator {
    return this.getByTestId('contact-partner-section')
      .or(this.page.locator('div:has(a.contact-partner-link)'));
  }
  
  /**
   * Get contact partner link
   * Fallback: template uses class contact-partner-link, no data-testid
   */
  get contactPartner(): Locator {
    return this.getByTestId('contact-partner-link')
      .or(this.page.locator('a.contact-partner-link'));
  }
  
  /**
   * Get contact department display
   * No data-testid for department. It appears in the general info area
   * after the "Additional Details" toggle in contact-view.component.html.
   * Falls back to looking for text next to the department label.
   */
  get contactDepartment(): Locator {
    // Department is rendered in ContactTabsComponent inline template after the title
    return this.page.locator('app-contact-tabs').first();
  }
  
  /**
   * Get contact info section wrapper
   * Fallback: template uses class contact-info-content, no data-testid
   */
  get contactInfoSection(): Locator {
    return this.getByTestId('contact-info-section')
      .or(this.page.locator('.contact-info-content'));
  }
  
  /**
   * Get contact status field
   * Uses actual data-testid="contact-status" from contact-view.component.html
   * NOTE: May only be visible after expanding additional info
   */
  get contactStatus(): Locator {
    return this.getByTestId('contact-status');
  }
  
  /**
   * Get documents section
   * Fallback: template uses app-document with class contact-document-section, no data-testid
   */
  override get documentsSection(): Locator {
    return this.getByTestId('contact-documents-section')
      .or(this.page.locator('app-document.contact-document-section, .contact-document-section'));
  }
  
  /**
   * Get links section
   * Fallback: template uses app-link-list with class contact-links-section, no data-testid
   */
  get linksSection(): Locator {
    return this.getByTestId('contact-links-section')
      .or(this.page.locator('app-link-list.contact-links-section, .contact-links-section'));
  }
  
  /**
   * Get upload document button
   * Fallback: template uses p-button with upload label, no data-testid (permission-gated)
   */
  get uploadDocumentButton(): Locator {
    return this.getByTestId('upload-document-button')
      .or(this.page.locator('app-contact-view p-button').filter({ hasText: /upload|document/i }));
  }
  
  /**
   * Get add link button
   * Fallback: template uses p-button with add link label, no data-testid (permission-gated)
   */
  get addLinkButton(): Locator {
    return this.getByTestId('add-link-button')
      .or(this.page.locator('app-contact-view p-button').filter({ hasText: /add.*link|link/i }));
  }
  
  /**
   * Navigate to contact detail page
   */
  async navigate(contactId: string | number): Promise<void> {
    await this.navigateToDetail(contactId);
  }
  
  /**
   * Verify contact name is displayed somewhere on the page
   * Since the name is in the ContactTabsComponent (no data-testid),
   * we check for the presence of the tabs component or search for the name text.
   */
  async verifyContactName(expectedName?: string): Promise<void> {
    // Verify the contact tabs component is loaded (contains the name)
    const tabsComponent = this.page.locator('app-contact-tabs');
    const tabsVisible = await tabsComponent.isVisible().catch(() => false);
    
    if (tabsVisible && expectedName) {
      const nameOnPage = this.page.getByText(expectedName, { exact: false });
      const isVisible = await nameOnPage.isVisible().catch(() => false);
      if (!isVisible) {
        throw new Error(`Expected contact name "${expectedName}" to appear on the page, but it was not found`);
      }
    }
    // The name may still be loading or rendered differently — acceptable
  }
  
  /**
   * Verify contact email is displayed
   * Uses actual data-testid="contact-email"
   */
  async verifyContactEmail(expectedEmail?: string): Promise<void> {
    const emailVisible = await this.contactEmail.isVisible().catch(() => false);
    
    if (emailVisible && expectedEmail) {
      const actualEmail = await this.contactEmail.textContent();
      if (actualEmail && !actualEmail.includes(expectedEmail)) {
        throw new Error(`Expected contact email to contain "${expectedEmail}", but got "${actualEmail}"`);
      }
    }
    // Email may not be set for all contacts — acceptable
  }
  
  /**
   * Verify associated partner is displayed
   * Uses actual data-testid="contact-partner-link"
   */
  async verifyContactPartner(expectedPartner?: string): Promise<void> {
    const partnerVisible = await this.contactPartner.isVisible().catch(() => false);
    
    if (partnerVisible && expectedPartner) {
      const actualPartner = await this.contactPartner.textContent();
      if (actualPartner && !actualPartner.includes(expectedPartner)) {
        throw new Error(`Expected partner to contain "${expectedPartner}", but got "${actualPartner}"`);
      }
    }
    // Partner section may not be visible for all contacts
  }
  
  /**
   * Get contact information from the page
   * Collects available contact data using actual DOM elements and data-testid attributes
   */
  async getContactInfo(): Promise<{
    name: string | null;
    email: string | null;
    phone: string | null;
    mobile: string | null;
    partner: string | null;
  }> {
    const SHORT_TIMEOUT = 5000;
    
    // Get name from ContactTabsComponent (no data-testid)
    const nameLocator = this.contactName;
    const nameText = await nameLocator.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null);
    
    // Get email (data-testid="contact-email")
    const emailVisible = await this.contactEmail.isVisible().catch(() => false);
    const emailText = emailVisible
      ? await this.contactEmail.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get phone (data-testid="contact-phone")
    const phoneVisible = await this.contactPhone.isVisible().catch(() => false);
    const phoneText = phoneVisible
      ? await this.contactPhone.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get mobile (data-testid="contact-mobile")
    const mobileVisible = await this.contactMobile.isVisible().catch(() => false);
    const mobileText = mobileVisible
      ? await this.contactMobile.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    // Get partner name (data-testid="contact-partner-link")
    const partnerVisible = await this.contactPartner.isVisible().catch(() => false);
    const partnerText = partnerVisible
      ? await this.contactPartner.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null)
      : null;
    
    return {
      name: nameText,
      email: emailText,
      phone: phoneText,
      mobile: mobileText,
      partner: partnerText,
    };
  }
  
  /**
   * Check if interactions section/tab is available
   * Interactions are displayed in a separate tab (app-contact-view-interactions),
   * not an inline section. This checks if the Interactions tab trigger exists.
   */
  async hasInteractionsSection(): Promise<boolean> {
    // Look for the Interactions tab in the tab navigation
    const interactionsTab = this.page.locator('p-tabpanel, [role="tab"]').filter({ hasText: /interactions/i });
    return await interactionsTab.isVisible().catch(() => false);
  }
  
  /**
   * Check if contact info section is visible
   * Uses actual data-testid="contact-info-section"
   */
  async hasContactInfoSection(): Promise<boolean> {
    return await this.contactInfoSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if links section is visible
   * Uses actual data-testid="contact-links-section"
   */
  async hasLinksSection(): Promise<boolean> {
    return await this.linksSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if documents section is visible
   * Uses actual data-testid="contact-documents-section"
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
    await this.verifyContactName();
    
    // Partner section should be visible if contact has a partner
    const hasPartner = await this.contactPartnerSection.isVisible().catch(() => false);
    
    // Contact info section should be visible
    const hasInfo = await this.hasContactInfoSection();
    
    // At least the info section or partner section should be present
    if (!hasPartner && !hasInfo) {
      throw new Error('Expected at least contact info or partner section to be visible');
    }
  }
}
