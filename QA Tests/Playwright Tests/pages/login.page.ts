/**
 * @fileoverview Login Page Object
 * Page object for login page interactions
 */

import { Page } from '@playwright/test';
import { BasePage } from './base.page';
import { assertVisible, assertContainsText } from '../helpers/assertions.helper';
import { waitForVisible } from '../helpers/wait.helper';

export class LoginPage extends BasePage {
  // Selectors (no data-testid in production - use placeholder/role-based)
  private readonly usernameInput = this.page.getByPlaceholder(/username|email/i).or(this.page.locator('input[type="email"], input[name="username"]')).first();
  private readonly passwordInput = this.page.getByPlaceholder(/password/i).or(this.page.locator('input[type="password"]')).first();
  private readonly loginButton = this.page.getByRole('button', { name: /sign in|log in|login/i });
  private readonly signupSection = this.page.locator('[data-testid="signup-section"]').first();
  private readonly signupButton = this.page.locator('[data-testid="signup-button"]').first();
  
  constructor(page: Page) {
    super(page);
  }
  
  /**
   * Navigate to login page
   */
  async navigate(): Promise<void> {
    await this.goto('/login');
  }
  
  /**
   * Fill username field
   */
  async fillUsername(username: string): Promise<void> {
    await this.usernameInput.fill(username);
  }
  
  /**
   * Fill password field (handles PrimeNG p-password component)
   */
  async fillPassword(password: string): Promise<void> {
    const passwordField = this.page.getByPlaceholder(/password/i).or(this.page.locator('input[type="password"]')).first();
    await passwordField.fill(password);
  }
  
  /**
   * Click login button
   */
  async clickLogin(): Promise<void> {
    await this.loginButton.click();
  }
  
  /**
   * Perform complete login flow
   */
  async login(username: string, password: string): Promise<void> {
    await this.fillUsername(username);
    await this.fillPassword(password);
    await this.clickLogin();
    await this.page.waitForURL(/\/home|\/dashboard/, { timeout: 10000 });
  }
  
  /**
   * Verify login form elements are visible
   */
  async verifyLoginFormVisible(): Promise<void> {
    await assertVisible(this.usernameInput);
    await assertVisible(this.passwordInput);
    await assertVisible(this.loginButton);
  }
  
  /**
   * Verify email and password labels
   */
  async verifyFormLabels(): Promise<void> {
    const emailLabel = this.page.locator('label[for="userEmail"]');
    const passwordLabel = this.page.locator('label[for="password"]');
    
    await assertContainsText(emailLabel, /Email/i);
    await assertContainsText(passwordLabel, /Password/i);
  }
  
  /**
   * Toggle password visibility
   */
  async togglePasswordVisibility(): Promise<void> {
    const toggleButton = this.page.locator('p-password button, .p-password button').first();
    await toggleButton.click();
  }
  
  /**
   * Get password field type
   */
  async getPasswordFieldType(): Promise<string | null> {
    return await this.page.locator('input[type="password"]').first().getAttribute('type');
  }
  
  /**
   * Check if signup section is visible
   */
  async isSignupSectionVisible(): Promise<boolean> {
    return await this.signupSection.isVisible().catch(() => false);
  }
  
  /**
   * Click signup button
   */
  async clickSignup(): Promise<void> {
    await this.signupButton.click();
  }
  
  /**
   * Verify error message is displayed
   */
  async verifyErrorMessage(): Promise<void> {
    const errorMessage = this.page.locator(
      '.p-message-error, [role="alert"], .error-message'
    ).first();
    
    await waitForVisible(errorMessage, 5000);
  }
  
  /**
   * Check if field has validation error
   */
  async hasValidationError(): Promise<boolean> {
    const usernameInvalid = await this.usernameInput
      .evaluate(el => el.classList.contains('ng-invalid'))
      .catch(() => false);
    
    const passwordInvalid = await this.passwordInput
      .evaluate(el => el.classList.contains('ng-invalid'))
      .catch(() => false);
    
    return usernameInvalid || passwordInvalid;
  }
}
