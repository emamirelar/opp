/**
 * @fileoverview Workflow Page Object
 * Page object for workflow component interactions across entities
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { waitForLoadingToComplete } from '../helpers/wait.helper';

export class WorkflowPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  // ==========================================
  // LOCATORS
  // ==========================================

  /** The workflow component container */
  get workflowComponent(): Locator {
    return this.page.locator('app-workflow, [data-testid="workflow-component"]').first();
  }

  /** Current stage display */
  get currentStage(): Locator {
    return this.page.locator('[data-testid="workflow-current-stage"], app-workflow .current-stage, .workflow-stage.active').first();
  }

  /** All stage indicators */
  get stageIndicators(): Locator {
    return this.page.locator('[data-testid="workflow-stage"], app-workflow .workflow-stage, app-stage-workflow .stage');
  }

  /** Stage transition action buttons */
  get actionButtons(): Locator {
    return this.page.locator('app-workflow button, [data-testid="workflow-action"]');
  }

  /** Submit button (if visible) */
  get submitButton(): Locator {
    return this.page.locator('[data-testid="workflow-submit"], app-workflow button:has-text("Submit")').first();
  }

  /** Approve button (if visible) */
  get approveButton(): Locator {
    return this.page.locator('[data-testid="workflow-approve"], app-workflow button:has-text("Approve")').first();
  }

  /** Reject button (if visible) */
  get rejectButton(): Locator {
    return this.page.locator('[data-testid="workflow-reject"], app-workflow button:has-text("Reject")').first();
  }

  /** Activate button (if visible) */
  get activateButton(): Locator {
    return this.page.locator('[data-testid="workflow-activate"], app-workflow button:has-text("Activate")').first();
  }

  /** Cancel button (if visible) */
  get cancelButton(): Locator {
    return this.page.locator('[data-testid="workflow-cancel"], app-workflow button:has-text("Cancel")').first();
  }

  /** Reopen button (if visible) */
  get reopenButton(): Locator {
    return this.page.locator('[data-testid="workflow-reopen"], app-workflow button:has-text("Reopen")').first();
  }

  /** Workflow history/audit trail */
  get workflowHistory(): Locator {
    return this.page.locator('[data-testid="workflow-history"], .workflow-history, app-timeline').first();
  }

  /** Confirmation dialog (appears after clicking action) */
  get confirmationDialog(): Locator {
    return this.page.locator('p-dialog, p-confirmdialog, [data-testid="workflow-confirmation"]').first();
  }

  /** Confirm button in dialog */
  get confirmButton(): Locator {
    return this.page.locator('p-dialog button:has-text("Confirm"), p-dialog button:has-text("Yes"), [data-testid="confirm-action"]').first();
  }

  // ==========================================
  // ACTIONS
  // ==========================================

  async isWorkflowVisible(): Promise<boolean> {
    return await this.workflowComponent.isVisible().catch(() => false);
  }

  async getCurrentStageName(): Promise<string | null> {
    if (await this.currentStage.isVisible().catch(() => false)) {
      return await this.currentStage.textContent();
    }
    return null;
  }

  async getStageCount(): Promise<number> {
    return await this.stageIndicators.count();
  }

  async getAvailableActionCount(): Promise<number> {
    return await this.actionButtons.count();
  }

  async isSubmitAvailable(): Promise<boolean> {
    return await this.submitButton.isVisible().catch(() => false);
  }

  async isApproveAvailable(): Promise<boolean> {
    return await this.approveButton.isVisible().catch(() => false);
  }

  async isRejectAvailable(): Promise<boolean> {
    return await this.rejectButton.isVisible().catch(() => false);
  }

  async isActivateAvailable(): Promise<boolean> {
    return await this.activateButton.isVisible().catch(() => false);
  }

  async isCancelAvailable(): Promise<boolean> {
    return await this.cancelButton.isVisible().catch(() => false);
  }

  async clickSubmit(): Promise<void> {
    await this.submitButton.click();
    await this.confirmationDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async clickApprove(): Promise<void> {
    await this.approveButton.click();
    await this.confirmationDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async clickReject(): Promise<void> {
    await this.rejectButton.click();
    await this.confirmationDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async confirmAction(): Promise<void> {
    if (await this.confirmButton.isVisible().catch(() => false)) {
      await this.confirmButton.click();
      await waitForLoadingToComplete(this.page);
    }
  }

  async hasWorkflowHistory(): Promise<boolean> {
    return await this.workflowHistory.isVisible().catch(() => false);
  }
}
