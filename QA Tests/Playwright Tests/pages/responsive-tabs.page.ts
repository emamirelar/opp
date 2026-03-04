import { type Page, type Locator } from '@playwright/test';

export class ResponsiveTabsPage {
  readonly page: Page;
  readonly desktopTabsContainer: Locator;
  readonly mobileDropdownContainer: Locator;
  readonly mobileDropdown: Locator;
  readonly tabButtons: Locator;
  readonly activeTab: Locator;

  constructor(page: Page) {
    this.page = page;
    this.desktopTabsContainer = page.locator('app-responsive-tabs .desktop-tabs, app-responsive-tabs [role="tablist"]');
    this.mobileDropdownContainer = page.locator(
      'app-responsive-tabs .mobile-dropdown, app-responsive-tabs p-select, app-responsive-tabs p-dropdown, app-responsive-tabs [role="combobox"]'
    );
    this.mobileDropdown = page.locator(
      'app-responsive-tabs p-select, app-responsive-tabs p-dropdown, app-responsive-tabs [role="combobox"], app-responsive-tabs .mobile-dropdown'
    );
    this.tabButtons = page.locator('app-responsive-tabs [role="tab"]');
    this.activeTab = page.locator('app-responsive-tabs [role="tab"][aria-selected="true"]');
  }

  async waitForDesktopTabs(): Promise<void> {
    await this.desktopTabsContainer.first().waitFor({ state: 'visible', timeout: 15000 });
  }

  async waitForMobileDropdown(): Promise<void> {
    await this.mobileDropdownContainer.first().waitFor({ state: 'visible', timeout: 15000 });
  }

  async getDesktopTabCount(): Promise<number> {
    return this.tabButtons.count();
  }

  async clickDesktopTab(index: number): Promise<void> {
    await this.tabButtons.nth(index).click();
  }
}
