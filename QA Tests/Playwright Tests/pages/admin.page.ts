/**
 * @fileoverview Admin Pages Object
 * Page objects for admin features: Translation Workbench, Entity Manager,
 * Entity Artifact Manager, User Management
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { waitForLoadingToComplete, waitForTableData, waitForVisible } from '../helpers/wait.helper';

// ==========================================
// Translation Workbench Page
// ==========================================

export class TranslationWorkbenchPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="translation-workbench-header"], h1, h2').first();
  }

  get translationTable(): Locator {
    return this.page.locator('p-table, .p-datatable, [data-testid="translation-table"]').first();
  }

  get searchInput(): Locator {
    return this.page.locator('[data-testid="translation-search"], input[placeholder*="Search"], input[type="search"]').first();
  }

  get languageSelector(): Locator {
    return this.page.locator('[data-testid="language-selector"], p-select, p-dropdown').first();
  }

  get saveButton(): Locator {
    return this.page.locator('[data-testid="save-translations"], button:has-text("Save")').first();
  }

  get tableRows(): Locator {
    return this.page.locator('p-table tbody tr, .p-datatable-tbody tr');
  }

  get editableCell(): Locator {
    return this.page.locator('p-table td input, p-table td textarea, .p-cell-editing input').first();
  }

  /** Coming Soon placeholder (when feature is not yet implemented) */
  get comingSoon(): Locator {
    return this.page.locator('app-coming-soon').first();
  }

  /** Translation-related heading or text */
  get translationHeading(): Locator {
    return this.page.getByText(/translation/i).first();
  }

  /** Visual indicator (icon, image, or svg) */
  get visualIndicator(): Locator {
    return this.page.locator('i[class*="pi-"], img, svg').first();
  }

  /** Link to translation workbench in admin sidebar (when on /admin) */
  get sidebarTranslationLink(): Locator {
    return this.page
      .locator(
        'app-sidebar a[href*="translations"], .layout-sidebar a[href*="translations"], a[routerlink*="translations"], a[href*="/admin/translations"]'
      )
      .first();
  }

  async navigate(): Promise<void> {
    await this.goto('/admin/translations');
  }

  async isPageLoaded(): Promise<boolean> {
    return await this.translationTable.isVisible().catch(() => false);
  }

  async searchTranslation(key: string): Promise<void> {
    await this.searchInput.fill(key);
    await waitForTableData(this.page);
  }

  async getRowCount(): Promise<number> {
    return await this.tableRows.count();
  }

  async selectLanguage(language: string): Promise<void> {
    await this.languageSelector.click();
    const option = this.page.locator(`.p-select-option:has-text("${language}"), .p-dropdown-item:has-text("${language}")`).first();
    await waitForVisible(option);
    await option.click();
    await waitForLoadingToComplete(this.page);
  }
}

// ==========================================
// Entity Manager Page
// ==========================================

export class EntityManagerPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="entity-manager-header"], h1, h2').first();
  }

  get entityManagerHeading(): Locator {
    return this.page.getByText(/entity manager/i).first();
  }

  get entityList(): Locator {
    return this.page.locator('[data-testid="entity-list"], p-table, .p-datatable').first();
  }

  get entityCards(): Locator {
    return this.page.locator('[data-testid="entity-card"], .entity-card');
  }

  get configPanel(): Locator {
    return this.page.locator('[data-testid="entity-config-panel"], p-panel, .config-panel').first();
  }

  get saveConfigButton(): Locator {
    return this.page.locator('[data-testid="save-config"], button:has-text("Save")').first();
  }

  get fieldList(): Locator {
    return this.page.locator('[data-testid="field-list"], .field-list');
  }

  /** Entity selector: p-tabs (desktop) or p-dropdown/p-select (mobile) */
  get entitySelector(): Locator {
    return this.page.locator('p-tabs, p-dropdown, p-select, app-entity-manager').first();
  }

  /** First entity tab for selection */
  get firstEntityTab(): Locator {
    return this.page.locator(
      'p-tab, [role="tab"], .entity-manager-tabs p-tab, p-tabs button, p-dropdown'
    ).first();
  }

  /** Tabs or entity type navigation */
  get tabsOrSelector(): Locator {
    return this.page.locator(
      '.entity-manager-tabs, p-tabs, p-select, p-dropdown, app-entity-manager'
    ).first();
  }

  get availableFieldsSection(): Locator {
    return this.page
      .locator('.available-fields-section, app-entity-manager .flex.flex-col')
      .filter({ hasText: /available fields/i })
      .or(this.page.locator('div').filter({ hasText: /available fields/i }).first())
      .first();
  }

  get availableFieldsText(): Locator {
    return this.page.getByText(/available fields/i).first();
  }

  get listViewFieldsSection(): Locator {
    return this.page
      .locator('.list-view-fields-section, app-entity-manager .flex.flex-col')
      .filter({ hasText: /list view fields/i })
      .or(this.page.locator('div').filter({ hasText: /list view/i }).first())
      .first();
  }

  get listViewText(): Locator {
    return this.page.getByText(/list view fields|list view/i).first();
  }

  get addFieldButton(): Locator {
    return this.page
      .locator(
        'app-entity-manager p-button, app-entity-manager button, .add-field-button'
      )
      .filter({ hasText: /add/i })
      .or(this.page.getByRole('button', { name: /add/i }))
      .first();
  }

  get addFieldText(): Locator {
    return this.page.getByText(/add field/i).first();
  }

  get entitySettingsButton(): Locator {
    return this.page
      .locator('app-entity-manager p-button, app-entity-manager button, .entity-settings-button')
      .filter({ hasText: /settings/i })
      .or(this.page.getByRole('button', { name: /settings/i }))
      .first();
  }

  get entitySettingsText(): Locator {
    return this.page.getByText(/entity settings/i).first();
  }

  get cardPreviewSection(): Locator {
    return this.page
      .locator('app-listview-card, .card-preview-section, .list-view-fields-section, p-panel, .p-panel')
      .filter({ hasText: /card preview|list view/i })
      .first();
  }

  get cardPreviewText(): Locator {
    return this.page.getByText(/card preview|list view/i).first();
  }

  async navigate(): Promise<void> {
    await this.goto('/admin/entity-manager');
  }

  async isPageLoaded(): Promise<boolean> {
    const hasHeader = await this.pageHeader.isVisible().catch(() => false);
    const hasList = await this.entityList.isVisible().catch(() => false);
    return hasHeader || hasList;
  }

  async getEntityCardCount(): Promise<number> {
    return await this.entityCards.count();
  }

  async clickEntity(entityName: string): Promise<void> {
    await this.page.locator(`[data-testid="entity-card-${entityName}"], .entity-card:has-text("${entityName}")`).first().click();
    await waitForLoadingToComplete(this.page);
  }

  /**
   * Ensure first entity tab is selected; clicks it if visible and waits for content.
   * Uses waitForVisible on target section instead of fixed timeout.
   */
  async ensureFirstEntitySelected(targetSection: Locator): Promise<void> {
    const tab = this.firstEntityTab.or(this.page.locator('p-tab, [role="tab"], p-tabs button').first());
    const dropdown = this.page.locator('p-dropdown, p-select').first();
    if (await tab.isVisible({ timeout: 3000 }).catch(() => false)) {
      await tab.click();
      await this.page.waitForTimeout(500);
    } else if (await dropdown.isVisible({ timeout: 2000 }).catch(() => false)) {
      await dropdown.click();
      await this.page.locator('.p-dropdown-item, .p-select-option').first().click({ timeout: 3000 }).catch(() => {});
      await this.page.waitForTimeout(500);
    }
    await targetSection.waitFor({ state: 'visible', timeout: 15000 }).catch(() => {});
  }
}

// ==========================================
// Entity Artifact Manager Page
// ==========================================

export class EntityArtifactManagerPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="entity-artifact-header"], h1, h2').first();
  }

  get artifactTable(): Locator {
    return this.page.locator('p-table, .p-datatable, [data-testid="artifact-table"]').first();
  }

  get addArtifactButton(): Locator {
    return this.page.locator('[data-testid="add-artifact"], button:has-text("Add"), button:has-text("New")').first();
  }

  get bulkUpdateButton(): Locator {
    return this.page.locator('[data-testid="bulk-update-artifacts"], button:has-text("Bulk Update")').first();
  }

  get entitySelector(): Locator {
    return this.page.locator('[data-testid="entity-selector"], p-select, p-dropdown').first();
  }

  get artifactRows(): Locator {
    return this.page.locator('p-table tbody tr, .p-datatable-tbody tr');
  }

  get editDialog(): Locator {
    return this.page.locator('p-dialog, [data-testid="artifact-edit-dialog"]').first();
  }

  async navigate(): Promise<void> {
    await this.goto('/admin/entity-artifacts');
  }

  async isPageLoaded(): Promise<boolean> {
    return await this.artifactTable.isVisible().catch(() => false);
  }

  async getArtifactCount(): Promise<number> {
    return await this.artifactRows.count();
  }

  async clickAddArtifact(): Promise<void> {
    await this.addArtifactButton.click();
    await this.editDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async clickBulkUpdate(): Promise<void> {
    await this.bulkUpdateButton.click();
    await waitForLoadingToComplete(this.page);
  }

  async selectEntity(entityName: string): Promise<void> {
    await this.entitySelector.click();
    const option = this.page.locator(`.p-select-option:has-text("${entityName}"), .p-dropdown-item:has-text("${entityName}")`).first();
    await waitForVisible(option);
    await option.click();
    await waitForLoadingToComplete(this.page);
  }

  get searchOrFilterInput(): Locator {
    return this.page.locator('[data-testid="artifact-search"], input[placeholder*="search"], input[type="text"]').first();
  }

  get fieldConfigText(): Locator {
    return this.page.getByText(/field|column|attribute|property|label/i).first();
  }
}

// ==========================================
// Bulk Entity Artifacts Page
// ==========================================

export class BulkEntityArtifactsPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="bulk-entity-artifacts-header"], h1, h2').first();
  }

  get entityTypeSelector(): Locator {
    return this.page.locator('p-select, p-dropdown, select').first();
  }

  get applyButton(): Locator {
    return this.page.locator('button').filter({ hasText: /apply|update|execute|save/i }).first();
  }

  async navigate(): Promise<void> {
    await this.goto('/admin/bulk-entity-artifacts');
  }

  async isPageLoaded(): Promise<boolean> {
    return await this.entityTypeSelector.isVisible().catch(() => false);
  }
}

// ==========================================
// User Management Page
// ==========================================

export class UserManagementPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="user-management-header"], h1, h2').first();
  }

  get userTable(): Locator {
    return this.page.locator('p-table, .p-datatable, [data-testid="user-table"]').first();
  }

  get userRows(): Locator {
    return this.page.locator('p-table tbody tr, .p-datatable-tbody tr');
  }

  get addUserButton(): Locator {
    return this.page.locator('[data-testid="add-user"], button:has-text("Add User"), button:has-text("Invite")').first();
  }

  get searchInput(): Locator {
    return this.page.locator('#search, [data-testid="user-search"], input[placeholder*="Search"], input[type="search"]').first();
  }

  get roleDropdown(): Locator {
    return this.page.locator('[data-testid="role-dropdown"], p-select, p-dropdown, p-multiselect').first();
  }

  get userDialog(): Locator {
    return this.page.locator('p-dialog, [data-testid="user-dialog"]').first();
  }

  get permissionMatrix(): Locator {
    return this.page.locator('[data-testid="permission-matrix"], .permission-matrix, p-table').first();
  }

  async navigate(): Promise<void> {
    await this.goto('/admin/user-management');
  }

  async isPageLoaded(): Promise<boolean> {
    const hasHeader = await this.pageHeader.isVisible().catch(() => false);
    const hasTable = await this.userTable.isVisible().catch(() => false);
    return hasHeader || hasTable;
  }

  async getUserCount(): Promise<number> {
    return await this.userRows.count();
  }

  async searchUser(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await waitForTableData(this.page);
  }

  async clickAddUser(): Promise<void> {
    await this.addUserButton.click();
    await this.userDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async clickUserRow(index: number): Promise<void> {
    await this.userRows.nth(index).click();
    await waitForLoadingToComplete(this.page);
  }
}
