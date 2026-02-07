/**
 * @fileoverview Base Page Object
 * Provides common functionality for all page objects
 */

import { Page, Locator } from '@playwright/test';
import { waitForNetworkIdle, waitForVisible, waitForPermissions } from '../helpers/wait.helper';
import { assertVisible } from '../helpers/assertions.helper';

export abstract class BasePage {
  protected readonly page: Page;
  
  constructor(page: Page) {
    this.page = page;
  }
  
  /**
   * Navigate to the page
   * @param url - Relative URL (with or without hash prefix)
   * @description Handles Angular hash-based routing by ensuring URLs are prefixed with /#/
   */
  async goto(url: string): Promise<void> {
    const BASE_URL = 'http://127.0.0.1:4200';
    
    // Angular uses hash-based routing - ensure URLs are prefixed with /#/
    let hashUrl = url;
    if (!url.startsWith('/#/') && !url.startsWith('#/')) {
      // Convert /login to /#/login
      hashUrl = url.startsWith('/') ? `/#${url}` : `/#/${url}`;
    } else if (url.startsWith('#/')) {
      // Convert #/login to /#/login
      hashUrl = `/${url}`;
    }
    await this.page.goto(`${BASE_URL}${hashUrl}`);
    await waitForNetworkIdle(this.page);
  }
  
  /**
   * Get page URL
   */
  getUrl(): string {
    return this.page.url();
  }
  
  /**
   * Wait for page to load
   */
  async waitForLoad(): Promise<void> {
    await waitForNetworkIdle(this.page);
  }
  
  /**
   * Wait for permissions to load
   */
  async waitForPermissions(): Promise<void> {
    await waitForPermissions(this.page);
  }
  
  /**
   * Get element by test ID
   */
  getByTestId(testId: string): Locator {
    return this.page.locator(`[data-testid="${testId}"]`);
  }
  
  /**
   * Assert element is visible by test ID
   */
  async assertElementVisible(testId: string): Promise<void> {
    await assertVisible(this.getByTestId(testId));
  }
  
  /**
   * Click element by test ID
   */
  async clickByTestId(testId: string): Promise<void> {
    await this.getByTestId(testId).click();
  }
  
  /**
   * Fill input by test ID
   */
  async fillByTestId(testId: string, value: string): Promise<void> {
    await this.getByTestId(testId).fill(value);
  }
  
  /**
   * Check if button is visible
   */
  async isButtonVisible(testId: string): Promise<boolean> {
    return await this.getByTestId(testId).isVisible().catch(() => false);
  }
  
  /**
   * Get current page title
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }
  
  /**
   * Take screenshot
   */
  async takeScreenshot(name: string): Promise<void> {
    await this.page.screenshot({ path: `screenshots/${name}.png` });
  }
  
  /**
   * Reload page
   */
  async reload(): Promise<void> {
    await this.page.reload();
    await waitForNetworkIdle(this.page);
  }
}
