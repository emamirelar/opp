/**
 * @fileoverview Opportunity Item Page Object
 * Page object for opportunity detail/item page
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';

export class OpportunityItemPage extends EntityDetailPage {
  protected entityName = 'opportunity';
  
  constructor(page: Page, opportunityId?: string | number) {
    super(page, opportunityId);
  }
  
  /**
   * Get opportunity title field
   */
  get opportunityTitle(): Locator {
    return this.getByTestId('opportunity-title');
  }
  
  /**
   * Get opportunity value field
   */
  get opportunityValue(): Locator {
    return this.getByTestId('opportunity-value');
  }
  
  /**
   * Get opportunity stage field
   */
  get opportunityStage(): Locator {
    return this.getByTestId('opportunity-stage');
  }
  
  /**
   * Get opportunity start date field
   */
  get opportunityStartDate(): Locator {
    return this.getByTestId('opportunity-start-date');
  }
  
  /**
   * Get opportunity end date field
   */
  get opportunityEndDate(): Locator {
    return this.getByTestId('opportunity-end-date');
  }
  
  /**
   * Get opportunity description field
   */
  get opportunityDescription(): Locator {
    return this.getByTestId('opportunity-description');
  }
  
  /**
   * Get budget section
   */
  get budgetSection(): Locator {
    return this.getByTestId('opportunity-budget-section');
  }
  
  /**
   * Get schedule/timeline section
   */
  get scheduleSection(): Locator {
    return this.getByTestId('opportunity-schedule-section');
  }
  
  /**
   * Get partners section
   */
  get partnersSection(): Locator {
    return this.getByTestId('opportunity-partners-section');
  }
  
  /**
   * Get contacts section
   */
  get contactsSection(): Locator {
    return this.getByTestId('opportunity-contacts-section');
  }
  
  /**
   * Get interactions section
   */
  get interactionsSection(): Locator {
    return this.getByTestId('opportunity-interactions-section');
  }
  
  /**
   * Get DST (Decision Support Tool) section
   */
  get dstSection(): Locator {
    return this.getByTestId('opportunity-dst-section');
  }
  
  /**
   * Get workflow actions toolbar
   */
  get workflowActionsToolbar(): Locator {
    return this.getByTestId('opportunity-workflow-actions');
  }
  
  /**
   * Get submit button
   */
  get submitButton(): Locator {
    return this.getByTestId('submit-opportunity-button');
  }
  
  /**
   * Get approve button
   */
  get approveButton(): Locator {
    return this.getByTestId('approve-opportunity-button');
  }
  
  /**
   * Get activate button
   */
  get activateButton(): Locator {
    return this.getByTestId('activate-opportunity-button');
  }
  
  /**
   * Navigate to opportunity detail page
   */
  async navigate(opportunityId: string | number): Promise<void> {
    await this.navigateToDetail(opportunityId);
  }
  
  /**
   * Verify opportunity title is displayed
   */
  async verifyOpportunityTitle(expectedTitle?: string): Promise<void> {
    await assertVisible(this.opportunityTitle);
    
    if (expectedTitle) {
      const actualTitle = await this.opportunityTitle.textContent();
      if (actualTitle && !actualTitle.includes(expectedTitle)) {
        throw new Error(`Expected opportunity title to contain "${expectedTitle}", but got "${actualTitle}"`);
      }
    }
  }
  
  /**
   * Verify opportunity stage is displayed
   */
  async verifyOpportunityStage(expectedStage?: string): Promise<void> {
    await assertVisible(this.opportunityStage);
    
    if (expectedStage) {
      const actualStage = await this.opportunityStage.textContent();
      if (actualStage && !actualStage.includes(expectedStage)) {
        throw new Error(`Expected opportunity stage to contain "${expectedStage}", but got "${actualStage}"`);
      }
    }
  }
  
  /**
   * Get opportunity information
   */
  async getOpportunityInfo(): Promise<{
    title: string | null;
    value: string | null;
    stage: string | null;
    startDate: string | null;
    endDate: string | null;
    description: string | null;
  }> {
    return {
      title: await this.opportunityTitle.textContent(),
      value: await this.opportunityValue.textContent().catch(() => null),
      stage: await this.opportunityStage.textContent(),
      startDate: await this.opportunityStartDate.textContent().catch(() => null),
      endDate: await this.opportunityEndDate.textContent().catch(() => null),
      description: await this.opportunityDescription.textContent().catch(() => null),
    };
  }
  
  /**
   * Check if budget section is visible
   */
  async hasBudgetSection(): Promise<boolean> {
    return await this.budgetSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if schedule section is visible
   */
  async hasScheduleSection(): Promise<boolean> {
    return await this.scheduleSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if partners section is visible
   */
  async hasPartnersSection(): Promise<boolean> {
    return await this.partnersSection.isVisible().catch(() => false);
  }
  
  /**
   * Get partners count
   */
  async getPartnersCount(): Promise<number> {
    if (!await this.hasPartnersSection()) {
      return 0;
    }
    
    const partnerItems = this.page.locator('[data-testid="opportunity-partner-item"]');
    return await partnerItems.count();
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
    
    const contactItems = this.page.locator('[data-testid="opportunity-contact-item"]');
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
    
    const interactionItems = this.page.locator('[data-testid="opportunity-interaction-item"]');
    return await interactionItems.count();
  }
  
  /**
   * Check if DST section is visible
   */
  async hasDSTSection(): Promise<boolean> {
    return await this.dstSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if workflow actions toolbar is visible
   */
  async hasWorkflowActions(): Promise<boolean> {
    return await this.workflowActionsToolbar.isVisible().catch(() => false);
  }
  
  /**
   * Check if submit button is visible
   */
  async isSubmitButtonVisible(): Promise<boolean> {
    return await this.submitButton.isVisible().catch(() => false);
  }
  
  /**
   * Click submit button
   */
  async clickSubmitButton(): Promise<void> {
    if (await this.isSubmitButtonVisible()) {
      await this.submitButton.click();
      await this.page.waitForTimeout(1000);
    }
  }
  
  /**
   * Check if approve button is visible
   */
  async isApproveButtonVisible(): Promise<boolean> {
    return await this.approveButton.isVisible().catch(() => false);
  }
  
  /**
   * Click approve button
   */
  async clickApproveButton(): Promise<void> {
    if (await this.isApproveButtonVisible()) {
      await this.approveButton.click();
      await this.page.waitForTimeout(1000);
    }
  }
  
  /**
   * Check if activate button is visible
   */
  async isActivateButtonVisible(): Promise<boolean> {
    return await this.activateButton.isVisible().catch(() => false);
  }
  
  /**
   * Click activate button
   */
  async clickActivateButton(): Promise<void> {
    if (await this.isActivateButtonVisible()) {
      await this.activateButton.click();
      await this.page.waitForTimeout(1000);
    }
  }
  
  /**
   * Verify all main sections are displayed
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyOpportunityTitle();
    await this.verifyOpportunityStage();
  }
}
