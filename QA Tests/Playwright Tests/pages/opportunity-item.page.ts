/**
 * @fileoverview Opportunity Item Page Object
 * Page object for opportunity detail/item page
 * 
 * Uses actual data-testid attributes from the Angular opportunity-view component:
 *   - opportunity-detail-header: Header wrapper
 *   - opportunity-title: Opportunity name (h1)
 *   - opportunity-status: Status badge (p-badge)
 *   - opportunity-stage: Workflow stage badge (p-badge)
 *   - opportunity-metadata: Metadata row
 *   - opportunity-id: ID display
 *   - opportunity-manager: Manager name
 *   - opportunity-orgunit: Responsible org unit
 *   - opportunity-target-signing-date: Target signing date
 * 
 * Section IDs available (used as anchors in the scrollable page):
 *   - #section-analysis, #section-overview, #section-what, #section-why,
 *   - #section-who, #section-where, #section-when, #section-risks,
 *   - #section-related, #section-collaboration, #section-statement, #section-team
 * 
 * Component selectors available:
 *   - app-opportunity-view: Main view component
 *   - app-stage-workflow: Workflow stage display
 *   - app-opportunity-documents: Documents panel
 *   - app-opportunity-overview-section, app-opportunity-what-section,
 *   - app-opportunity-who-section, app-opportunity-when-section,
 *   - app-opportunity-dst-section, app-opportunity-related-items, etc.
 * 
 * NOTE: The following do NOT have data-testid attributes:
 *   - Value/budget fields (within #section-what)
 *   - Start/end dates (within #section-when)
 *   - Description content (within #section-overview)
 *   - Partners/contacts/interactions sections
 *   - Workflow action buttons (submit, approve, activate)
 *   - DST section content
 */

import { Page, Locator } from '@playwright/test';
import { EntityDetailPage } from './entity-detail.page';
import { assertVisible } from '../helpers/assertions.helper';
import { waitForElementReady, waitForLoadingToComplete } from '../helpers/wait.helper';

export class OpportunityItemPage extends EntityDetailPage {
  protected entityName = 'opportunity';
  
  constructor(page: Page, opportunityId?: string | number) {
    super(page, opportunityId);
  }
  
  // ============================================
  // HEADER SECTION — Actual data-testid attributes
  // ============================================
  
  /**
   * Get opportunity title field
   * Uses actual data-testid="opportunity-title" (h1 element in header) or app-opportunity-view fallback
   */
  get opportunityTitle(): Locator {
    return this.getByTestId('opportunity-title')
      .or(this.page.locator('app-opportunity-view h1, app-opportunity-view .opportunity-title').first());
  }
  
  /**
   * Get opportunity status badge
   * Uses actual data-testid="opportunity-status" (p-badge in header).
   * Fallback: span.bg-badge-danger (Closed) or first p-badge when no data-testid.
   */
  get opportunityStatus(): Locator {
    return this.getByTestId('opportunity-status')
      .or(this.page.locator('app-opportunity-view span.bg-badge-danger'))
      .or(this.page.locator('app-opportunity-view p-badge').first());
  }
  
  /**
   * Get opportunity stage badge
   * Uses actual data-testid="opportunity-stage" (p-badge in header).
   * Fallback: last p-badge in header (stage is always last badge) when no data-testid.
   */
  get opportunityStage(): Locator {
    return this.getByTestId('opportunity-stage')
      .or(this.page.locator('app-opportunity-view p-badge').last());
  }
  
  /**
   * Get opportunity metadata row
   * Uses actual data-testid="opportunity-metadata" or PrimeNG panels/fieldsets with detail fields.
   * Fallback: sub-header metadata div (flex flex-wrap with ID, Manager, Org Unit, Target Signing Date).
   */
  get opportunityMetadata(): Locator {
    return this.getByTestId('opportunity-metadata')
      .or(this.page.locator('app-opportunity-view .metadata, app-opportunity-view [class*="metadata"]').first())
      .or(this.page.locator('app-opportunity-view .flex.flex-wrap.items-center.gap-x-2').filter({ hasText: /ID:|Manager:|Org Unit:/i }).first())
      .or(this.page.locator('app-opportunity-view p-panel, app-opportunity-view p-fieldset').first())
      .or(this.page.locator('app-opportunity-view #section-overview, app-opportunity-view #section-what').first());
  }
  
  /**
   * Get opportunity ID display
   * Uses actual data-testid="opportunity-id".
   * Fallback: metadata span containing "ID:" label.
   */
  get opportunityId(): Locator {
    return this.getByTestId('opportunity-id')
      .or(this.page.locator('app-opportunity-view').filter({ hasText: /ID:\s*\d+/ }).first());
  }
  
  /**
   * Get opportunity manager display
   * Uses actual data-testid="opportunity-manager".
   * Fallback: metadata span containing "Manager:" label.
   */
  get opportunityManager(): Locator {
    return this.getByTestId('opportunity-manager')
      .or(this.page.locator('app-opportunity-view').filter({ hasText: /Manager:/ }).first());
  }
  
  /**
   * Get opportunity org unit display
   * Uses actual data-testid="opportunity-orgunit".
   * Fallback: metadata span containing "Org Unit:" label.
   */
  get opportunityOrgUnit(): Locator {
    return this.getByTestId('opportunity-orgunit')
      .or(this.page.locator('app-opportunity-view').filter({ hasText: /Org Unit:/ }).first());
  }
  
  /**
   * Get opportunity target signing date
   * Uses actual data-testid="opportunity-target-signing-date".
   * Fallback: metadata span containing "Target Signing Date:" label.
   */
  get opportunityTargetSigningDate(): Locator {
    return this.getByTestId('opportunity-target-signing-date')
      .or(this.page.locator('app-opportunity-view').filter({ hasText: /Target Signing Date:/ }).first());
  }
  
  // ============================================
  // CONTENT SECTIONS — Using section IDs and component selectors
  // ============================================
  
  /**
   * Get overview/description section
   * No data-testid. Uses section ID #section-overview and component selector.
   */
  get overviewSection(): Locator {
    return this.page.locator('#section-overview, app-opportunity-overview-section').first();
  }
  
  /**
   * Get opportunity description content
   * No data-testid for description text. Falls back to overview section content.
   */
  get opportunityDescription(): Locator {
    return this.page.locator('app-opportunity-overview-section').first();
  }
  
  /**
   * Get "What" section (value/budget)
   * No data-testid. Uses section ID #section-what and component selector.
   */
  get whatSection(): Locator {
    return this.page.locator('#section-what, app-opportunity-what-section').first();
  }
  
  /**
   * Get opportunity value display
   * No data-testid for value/budget. Falls back to "What" section.
   */
  get opportunityValue(): Locator {
    return this.page.locator('#section-what, app-opportunity-what-section').first();
  }
  
  /**
   * Get What section chip/button (scrolls to #section-what)
   */
  get whatChip(): Locator {
    return this.page.locator('button:has-text("What")').first();
  }

  /**
   * Get Who section chip/button (scrolls to #section-who)
   */
  get whoChip(): Locator {
    return this.page.locator('button:has-text("Who")').first();
  }

  /**
   * Get Related section chip/button (scrolls to #section-related)
   */
  get relatedChip(): Locator {
    return this.page.locator('button:has-text("Related")').first();
  }

  /**
   * Get Risks/DST section chip/button (scrolls to #section-risks)
   */
  get risksChip(): Locator {
    return this.page.locator('button:has-text("Risks"), button:has-text("DST")').first();
  }

  /**
   * Get section nav chip by label (for PNO-877 section navigation tests).
   * Labels: Analysis, Overview, What, Why, Who, Where, When, Risks, Related, Comments, Statement, Team
   */
  getSectionChip(label: string): Locator {
    return this.page.locator(`button:has-text("${label}")`).first();
  }

  /**
   * Desktop section chips container (hidden on mobile via lg:hidden / hidden lg:block)
   */
  get sectionChipsContainer(): Locator {
    return this.page.locator('.hidden.lg\\:block .flex.items-center.gap-2').first();
  }

  /**
   * Mobile section dropdown (visible only on lg:hidden viewport)
   */
  get mobileSectionDropdown(): Locator {
    return this.page.locator('.lg\\:hidden p-select').first();
  }

  /**
   * Overflow "More..." dropdown (when chips overflow)
   */
  get overflowChipsDropdown(): Locator {
    return this.page.locator('p-select.more-chips-dropdown, p-select[styleclass="more-chips-dropdown"]').first();
  }

  /**
   * Active chip (has primary background)
   */
  get activeSectionChip(): Locator {
    return this.page.locator('button.bg-unops-primary.text-unops-primary-on').first();
  }

  /**
   * Get "Who" section (partners, contacts, stakeholders)
   * No data-testid. Uses section ID #section-who and component selector.
   */
  get whoSection(): Locator {
    return this.page.locator('#section-who, app-opportunity-who-section').first();
  }
  
  /**
   * Get partners section
   * Partners are within the "Who" section. No dedicated data-testid.
   */
  get partnersSection(): Locator {
    return this.page.locator('#section-who, app-opportunity-who-section').first();
  }
  
  /**
   * Get contacts section
   * Contacts/stakeholders are within the "Who" section. No dedicated data-testid.
   */
  get contactsSection(): Locator {
    return this.page.locator('#section-who, app-opportunity-who-section').first();
  }
  
  /**
   * Get "When" section (dates, timeline)
   * No data-testid. Uses section ID #section-when and component selector.
   */
  get whenSection(): Locator {
    return this.page.locator('#section-when, app-opportunity-when-section').first();
  }
  
  /**
   * Get opportunity start/end dates section
   * No data-testid for individual dates. Falls back to "When" section.
   */
  get scheduleSection(): Locator {
    return this.page.locator('#section-when, app-opportunity-when-section').first();
  }

  /**
   * Get When section chip/button (scrolls to #section-when)
   */
  get whenChip(): Locator {
    return this.page.locator('button:has-text("When")').first();
  }

  // ── PNO-1182: Date field floating label locators ──────────────────────

  /** Target Signing Date datepicker */
  get targetSigningDateField(): Locator {
    return this.whenSection.locator('#targetSigningDate, [id="targetSigningDate"]').first();
  }

  /** Implementation Start Date datepicker */
  get implementationStartDateField(): Locator {
    return this.whenSection.locator('#implementationStartDate, [id="implementationStartDate"]').first();
  }

  /** Target Delivery Date datepicker */
  get targetDeliveryDateField(): Locator {
    return this.whenSection.locator('#targetDeliveryDate, [id="targetDeliveryDate"]').first();
  }

  /** Submission Deadline / Proposal Submission Date datepicker */
  get submissionDeadlineField(): Locator {
    return this.whenSection.locator('#submissionDeadline, [id="submissionDeadline"]').first();
  }

  /** All floating labels within the When section */
  get whenFloatLabels(): Locator {
    return this.whenSection.locator('p-floatlabel');
  }

  /** All date labels in the When section */
  get whenDateLabels(): Locator {
    return this.whenSection.locator('p-floatlabel label');
  }

  /**
   * Click When chip and wait for section content to be visible
   */
  async openWhenSection(): Promise<void> {
    const chip = this.whenChip;
    if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chip.click();
      await waitForElementReady(this.whenSection, 5000);
    }
  }
  
  /**
   * Get "Related" section (interactions, source interactions)
   * No data-testid. Uses section ID #section-related and component selector.
   */
  get relatedSection(): Locator {
    return this.page.locator('#section-related, app-opportunity-related-items').first();
  }

  /**
   * Get collaboration/comments section
   * No data-testid. Uses section ID #section-collaboration, app-opportunity-collaboration, or app-comment.
   */
  get collaborationSection(): Locator {
    return this.page.locator('#section-collaboration, app-opportunity-collaboration, app-comment, [class*="comment"], [class*="collaboration"]').first();
  }

  /**
   * Get statement section
   * No data-testid. Uses section ID #section-statement.
   */
  get statementSection(): Locator {
    return this.page.locator('#section-statement').first();
  }
  
  /**
   * Get interactions section
   * Interactions are within the "Related" section. No dedicated data-testid.
   */
  get interactionsSection(): Locator {
    return this.page.locator('#section-related, app-opportunity-related-items').first();
  }
  
  /**
   * Get DST (Decision Support Tool / Risks) section
   * No data-testid. Uses section ID #section-risks and component selector.
   */
  get dstSection(): Locator {
    return this.page.locator('#section-risks, app-opportunity-dst-section').first();
  }
  
  /**
   * Get analysis section
   * No data-testid. Uses section ID #section-analysis and component selector.
   */
  get analysisSection(): Locator {
    return this.page.locator('#section-analysis, app-opportunity-analysis-section').first();
  }

  /**
   * Get Why section (context, SDGs)
   * No data-testid. Uses section ID #section-why.
   */
  get whySection(): Locator {
    return this.page.locator('#section-why, app-opportunity-why-section').first();
  }

  /**
   * Get Team section (collaborators, org unit)
   * No data-testid. Uses section ID #section-team.
   */
  get teamSection(): Locator {
    return this.page.locator('#section-team, app-opportunity-team-section').first();
  }
  
  /**
   * Get budget section
   * Budget is within the "What" section. No dedicated data-testid or section.
   */
  get budgetSection(): Locator {
    return this.whatSection;
  }
  
  /**
   * Get documents section
   * No data-testid. Uses the app-opportunity-documents component selector.
   */
  override get documentsSection(): Locator {
    return this.page.locator('app-opportunity-documents').first();
  }

  // ============================================
  // LAYOUT — PNO-882 visual consistency selectors
  // ============================================

  /** Banner image container — visibility based on viewport height (min-height: 850px) */
  get opportunityBanner(): Locator {
    return this.page.locator('.opportunity-banner').first();
  }

  /** Workflow action overlay — shown during Submit/Approve/Reject/Recall */
  get workflowActionOverlay(): Locator {
    return this.page.locator('.workflow-action-overlay').first();
  }

  /** Loading progress strip — shown during initial data load */
  get loadingProgressStrip(): Locator {
    return this.page.locator('.loading-progress-strip').first();
  }

  /** Section hover containers — editable sections show border glow on hover */
  get sectionHoverContainers(): Locator {
    return this.page.locator('.section-hover-container');
  }

  /** Documents panel toggle (collapsed state) — click to expand */
  get documentsPanelToggle(): Locator {
    return this.page.locator('app-opportunity-documents').first();
  }

  // ============================================
  // WORKFLOW — Using component selectors
  // ============================================
  
  /**
   * Get workflow component
   * No data-testid. Uses app-stage-workflow or app-workflow component selector.
   */
  get workflowActionsToolbar(): Locator {
    return this.page.locator('app-stage-workflow, app-workflow').first();
  }
  
  /**
   * Get submit button
   * No data-testid for workflow action buttons.
   * Falls back to finding a button with "Submit" text within the workflow component.
   */
  get submitButton(): Locator {
    return this.page.locator('app-stage-workflow p-button, app-workflow p-button')
      .filter({ hasText: /submit/i }).first();
  }
  
  /**
   * Get approve button
   * No data-testid for workflow action buttons.
   * Falls back to finding a button with "Approve" text within the workflow component.
   */
  get approveButton(): Locator {
    return this.page.locator('app-stage-workflow p-button, app-workflow p-button')
      .filter({ hasText: /approve/i }).first();
  }
  
  /**
   * Get activate button
   * No data-testid for workflow action buttons.
   * Falls back to finding a button with "Activate" text within the workflow component.
   */
  get activateButton(): Locator {
    return this.page.locator('app-stage-workflow p-button, app-workflow p-button')
      .filter({ hasText: /activate/i }).first();
  }
  
  // ============================================
  // NAVIGATION
  // ============================================
  
  /**
   * Navigate to opportunity detail page
   */
  async navigate(opportunityId: string | number): Promise<void> {
    await this.navigateToDetail(opportunityId);
  }
  
  // ============================================
  // VERIFICATION METHODS
  // ============================================
  
  /**
   * Verify opportunity title is displayed
   * Uses actual data-testid="opportunity-title"
   */
  async verifyOpportunityTitle(expectedTitle?: string): Promise<void> {
    const titleVisible = await this.opportunityTitle.isVisible().catch(() => false);
    
    if (titleVisible) {
      if (expectedTitle) {
        const actualTitle = await this.opportunityTitle.textContent();
        if (actualTitle && !actualTitle.includes(expectedTitle)) {
          throw new Error(`Expected opportunity title to contain "${expectedTitle}", but got "${actualTitle}"`);
        }
      }
    }
  }
  
  /**
   * Verify opportunity stage is displayed
   * Uses actual data-testid="opportunity-stage"
   */
  async verifyOpportunityStage(expectedStage?: string): Promise<void> {
    const stageVisible = await this.opportunityStage.isVisible().catch(() => false);
    
    if (stageVisible && expectedStage) {
      const actualStage = await this.opportunityStage.textContent();
      if (actualStage && !actualStage.includes(expectedStage)) {
        throw new Error(`Expected opportunity stage to contain "${expectedStage}", but got "${actualStage}"`);
      }
    }
  }
  
  /**
   * Verify opportunity status is displayed
   * Uses actual data-testid="opportunity-status"
   */
  async verifyOpportunityStatus(expectedStatus?: string): Promise<void> {
    const statusVisible = await this.opportunityStatus.isVisible().catch(() => false);
    
    if (statusVisible && expectedStatus) {
      const actualStatus = await this.opportunityStatus.textContent();
      if (actualStatus && !actualStatus.includes(expectedStatus)) {
        throw new Error(`Expected opportunity status to contain "${expectedStatus}", but got "${actualStatus}"`);
      }
    }
  }
  
  // ============================================
  // DATA RETRIEVAL
  // ============================================
  
  /**
   * Get opportunity information from the page header
   * Uses actual data-testid attributes for header fields
   */
  async getOpportunityInfo(): Promise<{
    title: string | null;
    status: string | null;
    stage: string | null;
    manager: string | null;
    orgUnit: string | null;
    targetSigningDate: string | null;
  }> {
    const SHORT_TIMEOUT = 5000;
    
    const getTextSafe = async (locator: Locator): Promise<string | null> => {
      const visible = await locator.isVisible().catch(() => false);
      return visible ? await locator.textContent({ timeout: SHORT_TIMEOUT }).catch(() => null) : null;
    };
    
    return {
      title: await getTextSafe(this.opportunityTitle),
      status: await getTextSafe(this.opportunityStatus),
      stage: await getTextSafe(this.opportunityStage),
      manager: await getTextSafe(this.opportunityManager),
      orgUnit: await getTextSafe(this.opportunityOrgUnit),
      targetSigningDate: await getTextSafe(this.opportunityTargetSigningDate),
    };
  }
  
  // ============================================
  // SECTION VISIBILITY CHECKS
  // ============================================
  
  /**
   * Check if overview/description section is visible
   */
  async hasOverviewSection(): Promise<boolean> {
    return await this.overviewSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if "What" (value/budget) section is visible
   */
  async hasWhatSection(): Promise<boolean> {
    return await this.whatSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if budget section is visible (alias for hasWhatSection)
   */
  async hasBudgetSection(): Promise<boolean> {
    return await this.hasWhatSection();
  }
  
  /**
   * Click What chip and wait for section content to be visible
   */
  async openWhatSection(): Promise<void> {
    const chip = this.whatChip;
    if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chip.click();
      await waitForElementReady(this.whatSection, 5000);
    }
  }

  /**
   * Click Who chip and wait for section content to be visible
   */
  async openWhoSection(): Promise<void> {
    const chip = this.whoChip;
    if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chip.click();
      await waitForElementReady(this.whoSection, 5000);
    }
  }

  /**
   * Click Related chip and wait for section content to be visible
   */
  async openRelatedSection(): Promise<void> {
    const chip = this.relatedChip;
    if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chip.click();
      await waitForElementReady(this.relatedSection, 5000);
    }
  }

  /**
   * Click Risks/DST chip and wait for section content to be visible
   */
  async openRisksSection(): Promise<void> {
    const chip = this.risksChip;
    if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chip.click();
      await waitForElementReady(this.dstSection, 5000);
    }
  }

  /**
   * Check if "Who" (partners/contacts) section is visible
   */
  async hasWhoSection(): Promise<boolean> {
    return await this.whoSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if partners section is visible (within "Who")
   */
  async hasPartnersSection(): Promise<boolean> {
    return await this.hasWhoSection();
  }
  
  /**
   * Check if contacts section is visible (within "Who")
   */
  async hasContactsSection(): Promise<boolean> {
    return await this.hasWhoSection();
  }
  
  /**
   * Check if schedule/dates section is visible
   */
  async hasScheduleSection(): Promise<boolean> {
    return await this.whenSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if interactions section is visible (within "Related")
   */
  async hasInteractionsSection(): Promise<boolean> {
    return await this.relatedSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if DST section is visible
   */
  async hasDSTSection(): Promise<boolean> {
    return await this.dstSection.isVisible().catch(() => false);
  }

  /**
   * Check if Why section is visible
   */
  async hasWhySection(): Promise<boolean> {
    return await this.whySection.isVisible().catch(() => false);
  }

  /**
   * Check if Team section is visible
   */
  async hasTeamSection(): Promise<boolean> {
    return await this.teamSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if analysis section is visible
   */
  async hasAnalysisSection(): Promise<boolean> {
    return await this.analysisSection.isVisible().catch(() => false);
  }
  
  /**
   * Check if documents section is visible
   */
  override async hasDocumentsSection(): Promise<boolean> {
    return await this.documentsSection.isVisible().catch(() => false);
  }
  
  // ============================================
  // WORKFLOW ACTION CHECKS
  // ============================================
  
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
      await waitForLoadingToComplete(this.page);
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
      await waitForLoadingToComplete(this.page);
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
      await waitForLoadingToComplete(this.page);
    }
  }
  
  // ============================================
  // COMPOSITE VERIFICATION
  // ============================================
  
  /**
   * Verify all main sections are displayed
   * Uses actual data-testid attributes and section IDs
   */
  async verifyMainSectionsDisplayed(): Promise<void> {
    await this.verifyPageHeader();
    await this.verifyOpportunityTitle();
    await this.verifyOpportunityStage();
  }
}
