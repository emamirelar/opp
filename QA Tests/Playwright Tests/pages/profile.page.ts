/**
 * @fileoverview Profile & Settings Page Object
 * Page object for user profile dialog, language selection, and org unit
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class ProfilePage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  // ==========================================
  // LOCATORS - Profile Menu
  // ==========================================

  get profileMenuButton(): Locator {
    return this.page
      .locator('.profile-menu-button, .profile-menu button')
      .or(this.page.locator('app-topbar .p-avatar, app-topbar button:has(.pi-user), app-topbar button:has(.p-avatar)'))
      .or(this.page.locator('app-topbar').getByRole('button'))
      .or(this.page.locator('.profile-avatar'))
      .or(this.page.locator('button:has(.pi-user), button:has(.p-avatar)'))
      .or(this.page.locator('app-profile-menubar'))
      .first();
  }

  get profileMenu(): Locator {
    return this.page.locator('.p-menu, .p-tieredmenu, [data-testid="profile-dropdown"]').first();
  }

  get profileDialogTrigger(): Locator {
    return this.page.getByText(/view profile|profile|my profile/i).first();
  }

  get logoutButton(): Locator {
    return this.page.locator('[data-testid="logout-button"], .p-menuitem:has-text("Logout"), a:has-text("Sign Out")').first();
  }

  // ==========================================
  // LOCATORS - Profile Dialog
  // ==========================================

  get profileDialog(): Locator {
    return this.page.locator('app-profile-dialog, [data-testid="profile-dialog"], p-dialog:has-text("Profile")').first();
  }

  get nameField(): Locator {
    return this.page.locator('[data-testid="profile-name"], app-profile-dialog input[formControlName="name"]').first();
  }

  get emailField(): Locator {
    return this.page.locator('[data-testid="profile-email"], app-profile-dialog input[formControlName="email"]').first();
  }

  get saveProfileButton(): Locator {
    return this.page.locator('[data-testid="save-profile"], app-profile-dialog button:has-text("Save")').first();
  }

  get cancelProfileButton(): Locator {
    return this.page.locator('[data-testid="cancel-profile"], app-profile-dialog button:has-text("Cancel")').first();
  }

  // ==========================================
  // LOCATORS - Language Selector
  // ==========================================

  get languageSelector(): Locator {
    return this.page.locator('app-language-selector, [data-testid="language-selector"]').first();
  }

  get languageOptions(): Locator {
    return this.page.locator('app-language-selector .p-select-option, .language-option');
  }

  // ==========================================
  // LOCATORS - Org Unit Selector
  // ==========================================

  get orgUnitSelector(): Locator {
    return this.page.locator('app-org-unit-selector, [data-testid="org-unit-selector"]').first();
  }

  get orgUnitDropdown(): Locator {
    return this.page.locator('app-org-unit-selector p-select, app-org-unit-selector p-dropdown').first();
  }

  // ==========================================
  // ACTIONS
  // ==========================================

  async openProfileMenu(): Promise<void> {
    await this.profileMenuButton.click();
    const menu = this.page.locator('.p-menu-overlay, [role="menu"], .p-menu').first();
    await menu.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async openProfileDialog(): Promise<void> {
    await this.openProfileMenu();
    await this.profileDialogTrigger.click();
    await this.profileDialog.waitFor({ state: 'visible', timeout: 5000 });
  }

  async isProfileDialogOpen(): Promise<boolean> {
    return await this.profileDialog.isVisible().catch(() => false);
  }

  async isLanguageSelectorVisible(): Promise<boolean> {
    return await this.languageSelector.isVisible().catch(() => false);
  }

  async isOrgUnitSelectorVisible(): Promise<boolean> {
    return await this.orgUnitSelector.isVisible().catch(() => false);
  }

  async selectLanguage(lang: string): Promise<void> {
    await this.languageSelector.click();
    await this.page.locator(`.p-select-option:has-text("${lang}"), .p-dropdown-item:has-text("${lang}")`).first().click();
    await this.page.locator('.p-select-overlay, .p-dropdown-panel').first().waitFor({ state: 'hidden', timeout: 3000 }).catch(() => {});
  }
}
