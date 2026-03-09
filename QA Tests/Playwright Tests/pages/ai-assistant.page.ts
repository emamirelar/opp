/**
 * @fileoverview AI Assistant Page Object
 * Page object for AI assistant panel and related AI components
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class AIAssistantPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  // ==========================================
  // LOCATORS - AI Assistant Panel
  // ==========================================

  /** AI assistant toggle button (typically in topbar or sidebar) */
  get assistantToggle(): Locator {
    return this.page.locator('[data-testid="ai-assistant-toggle"], button:has-text("AI Assistant"), .ai-assistant-trigger').first();
  }

  /** AI assistant panel container */
  get assistantPanel(): Locator {
    return this.page
      .locator('app-ai-assistant-panel, app-ai-panel, app-ai-assistant, .ai-assistant-panel')
      .first();
  }

  /** AI prompt input field */
  get promptInput(): Locator {
    return this.page
      .locator(
        '#messageInput, app-ai-assistant-panel textarea, app-ai-panel textarea, app-ai-assistant textarea, app-ai-assistant-panel input[type="text"], app-ai-panel input[type="text"], app-ai-assistant input[type="text"]'
      )
      .or(this.page.getByRole('textbox', { name: /message|prompt|ask/i }))
      .first();
  }

  /** Send prompt button */
  get sendButton(): Locator {
    return this.page
      .locator(
        'app-ai-assistant-panel button[type="submit"], app-ai-panel button[type="submit"], app-ai-assistant button[type="submit"], app-ai-assistant-panel button:has(i.pi-send), app-ai-panel button:has(i.pi-send), app-ai-assistant button:has(i.pi-send), app-ai-assistant-panel button:has(.pi-send), app-ai-panel button:has(.pi-send)'
      )
      .or(this.page.getByRole('button', { name: /send|submit/i }))
      .first();
  }

  /** AI response area */
  get responseArea(): Locator {
    return this.page.locator('[data-testid="ai-response"], app-ai-assistant .response-area, app-typewriter-markdown').first();
  }

  /** Loading indicator while AI processes */
  get loadingIndicator(): Locator {
    return this.page.locator('app-ai-assistant .loading, app-ai-assistant p-progressSpinner, [data-testid="ai-loading"]').first();
  }

  /** AI content renderer */
  get contentRenderer(): Locator {
    return this.page.locator('app-content-renderer, [data-testid="ai-content-renderer"]').first();
  }

  /** Entity grid in AI response */
  get entityGrid(): Locator {
    return this.page.locator('app-entity-grid, [data-testid="ai-entity-grid"]').first();
  }

  /** Chart in AI response */
  get chartComponent(): Locator {
    return this.page.locator('app-chart-js, canvas, [data-testid="ai-chart"]').first();
  }

  /** Collapsible thought sections */
  get thoughtSections(): Locator {
    return this.page.locator('app-collapsible-thought, [data-testid="ai-thought"]');
  }

  /** AI session list/history */
  get sessionList(): Locator {
    return this.page.locator('[data-testid="ai-session-list"], app-ai-assistant .session-list').first();
  }

  /** New session button */
  get newSessionButton(): Locator {
    return this.page.locator('[data-testid="ai-new-session"], button:has-text("New Chat"), button:has-text("New Session")').first();
  }

  // ==========================================
  // LOCATORS - AI Transcribe
  // ==========================================

  /** AI transcribe component */
  get transcribeComponent(): Locator {
    return this.page.locator('app-ai-transcribe, [data-testid="ai-transcribe"]').first();
  }

  /** Transcribe file input */
  get transcribeFileInput(): Locator {
    return this.page.locator('app-ai-transcribe input[type="file"], [data-testid="ai-transcribe-file-input"]').first();
  }

  /** Transcribe start button */
  get transcribeStartButton(): Locator {
    return this.page.locator('[data-testid="ai-transcribe-start"], app-ai-transcribe button:has-text("Transcribe")').first();
  }

  // ==========================================
  // LOCATORS - AI Comparison
  // ==========================================

  /** AI comparison component */
  get comparisonComponent(): Locator {
    return this.page.locator('app-ai-comparison, [data-testid="ai-comparison"]').first();
  }

  // ==========================================
  // LOCATORS - Context Scan
  // ==========================================

  /** AI scan/context button */
  get scanButton(): Locator {
    return this.page.locator('[data-testid="ai-scan-button"], app-ai-assistant-scan, app-ai-assistant-scan button').first();
  }

  // ==========================================
  // ACTIONS
  // ==========================================

  /** Navigate to AI assistant page */
  async navigate(): Promise<void> {
    await this.goto('/ai');
  }

  /** Open the AI assistant panel */
  async openAssistant(): Promise<void> {
    const isAlreadyOpen = await this.assistantPanel.isVisible().catch(() => false);
    if (!isAlreadyOpen) {
      await this.assistantToggle.click();
      await this.assistantPanel.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
    }
  }

  /** Close the AI assistant panel */
  async closeAssistant(): Promise<void> {
    const closeButton = this.page
      .locator('app-ai-assistant-panel button:has(i.pi-times), app-ai-panel button:has(i.pi-times), app-ai-assistant button:has(i.pi-times), app-ai-assistant-panel button:has(.pi-times), app-ai-panel button:has(.pi-times)')
      .or(this.page.getByRole('button', { name: /close/i }))
      .first();
    if (await closeButton.isVisible().catch(() => false)) {
      await closeButton.click();
    }
  }

  /** Send a prompt to the AI assistant */
  async sendPrompt(text: string): Promise<void> {
    await this.promptInput.fill(text);
    await this.sendButton.click();
  }

  /** Wait for AI response to appear */
  async waitForResponse(timeout = 30000): Promise<void> {
    // Wait for loading to appear then disappear
    await this.loadingIndicator.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
    await this.loadingIndicator.waitFor({ state: 'hidden', timeout }).catch(() => {});
    // Wait for response content
    await this.responseArea.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  /** Check if AI assistant panel is visible */
  async isAssistantOpen(): Promise<boolean> {
    return await this.assistantPanel.isVisible().catch(() => false);
  }

  /** Check if prompt input is ready */
  async isPromptInputReady(): Promise<boolean> {
    return await this.promptInput.isVisible().catch(() => false);
  }

  /** Check if response contains text */
  async responseContainsText(text: string): Promise<boolean> {
    const content = await this.responseArea.textContent().catch(() => '');
    return content?.includes(text) ?? false;
  }

  /** Get the number of thought sections in response */
  async getThoughtSectionCount(): Promise<number> {
    return await this.thoughtSections.count();
  }

  /** Check if entity grid is displayed in response */
  async hasEntityGrid(): Promise<boolean> {
    return await this.entityGrid.isVisible().catch(() => false);
  }

  /** Check if chart is displayed in response */
  async hasChart(): Promise<boolean> {
    return await this.chartComponent.isVisible().catch(() => false);
  }

  /** Start a new AI session */
  async startNewSession(): Promise<void> {
    if (await this.newSessionButton.isVisible().catch(() => false)) {
      await this.newSessionButton.click();
      // Wait for prompt input to be ready (new session state) instead of arbitrary timeout
      await this.promptInput.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
    }
  }
}
