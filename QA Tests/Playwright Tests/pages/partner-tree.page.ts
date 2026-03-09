/**
 * @fileoverview Partner Tree Page Object
 * Page object for partner tree navigation and management
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';
import { waitForLoadingToComplete } from '../helpers/wait.helper';

export class PartnerTreePage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  // ==========================================
  // LOCATORS
  // ==========================================

  get pageHeader(): Locator {
    return this.page.locator('[data-testid="partner-tree-header"], h1, h2').first();
  }

  get treeContainer(): Locator {
    return this.page.locator(
      'app-partner-tree, p-treetable, [data-testid="partner-tree"], .partner-tree, .p-treetable'
    ).first();
  }

  get treeNodes(): Locator {
    return this.page.locator(
      '.p-treetable-tbody tr, .p-treenode, [data-testid="tree-node"], app-partner-tree-item, .p-treetable-row'
    );
  }

  get expandableNodes(): Locator {
    return this.page.locator('.p-treenode-toggle, .p-tree-toggler, [data-testid="tree-expand"]');
  }

  get addNodeButton(): Locator {
    return this.page.locator('[data-testid="add-tree-node"], button:has-text("Add"), button:has-text("New")').first();
  }

  get editNodeButton(): Locator {
    return this.page.locator('[data-testid="edit-tree-node"], button:has-text("Edit")').first();
  }

  get deleteNodeButton(): Locator {
    return this.page.locator('[data-testid="delete-tree-node"], button:has-text("Delete")').first();
  }

  get searchInput(): Locator {
    return this.page.locator('[data-testid="tree-search"], input[placeholder*="Search"]').first();
  }

  get nodeDetailPanel(): Locator {
    return this.page.locator('[data-testid="tree-node-detail"], app-partner-tree-details, .node-detail-panel').first();
  }

  get navigationComponent(): Locator {
    return this.page.locator('app-partner-tree-view-navigation, [data-testid="tree-navigation"]').first();
  }

  get breadcrumb(): Locator {
    return this.page.locator('[data-testid="tree-breadcrumb"], .p-breadcrumb').first();
  }

  get nodeDialog(): Locator {
    return this.page.locator('p-dialog, [data-testid="tree-node-dialog"]').first();
  }

  // ==========================================
  // ACTIONS
  // ==========================================

  async navigate(): Promise<void> {
    await this.goto('/admin/partner-tree');
  }

  async navigateToPartnershipsView(): Promise<void> {
    await this.goto('/partnerships/partner-tree');
  }

  async isPageLoaded(): Promise<boolean> {
    return await this.treeContainer.isVisible().catch(() => false);
  }

  async getNodeCount(): Promise<number> {
    return await this.treeNodes.count();
  }

  async expandNode(index: number): Promise<void> {
    const toggle = this.expandableNodes.nth(index);
    if (await toggle.isVisible().catch(() => false)) {
      await toggle.click();
      await waitForLoadingToComplete(this.page);
    }
  }

  async clickNode(index: number): Promise<void> {
    await this.treeNodes.nth(index).click();
    try {
      await this.nodeDetailPanel.waitFor({ state: 'visible', timeout: 5000 });
    } catch {
      await waitForLoadingToComplete(this.page);
    }
  }

  async searchTree(query: string): Promise<void> {
    if (await this.searchInput.isVisible().catch(() => false)) {
      await this.searchInput.fill(query);
      await waitForLoadingToComplete(this.page);
    }
  }

  async clickAddNode(): Promise<void> {
    await this.addNodeButton.click();
    await this.nodeDialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
  }

  async isNodeDetailVisible(): Promise<boolean> {
    return await this.nodeDetailPanel.isVisible().catch(() => false);
  }

  async hasNavigation(): Promise<boolean> {
    return await this.navigationComponent.isVisible().catch(() => false);
  }
}
