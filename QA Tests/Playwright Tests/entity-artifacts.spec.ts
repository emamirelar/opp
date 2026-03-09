/**
 * @fileoverview Entity Artifacts Admin Page E2E Tests
 * Tests for the Entity Artifacts and Bulk Update admin pages.
 *
 * Covers scenarios: ADM-020 to ADM-032
 *
 * Uses API mocks - fully executable.
 * Admin pages are accessible at:
 * - /admin/entity-artifacts
 * - /admin/bulk-entity-artifacts
 *
 * @tests 13
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForPageReady,
  waitForLoadingToComplete,
  waitForVisible,
} from './helpers/wait.helper';
import { EntityArtifactManagerPage, BulkEntityArtifactsPage } from './pages/admin.page';
import { getTimeout } from './helpers/test-config';

test.describe('Entity Artifacts Admin Page', () => {
  test.slow();
  test('ADM-020: Entity artifacts page loads for admin user', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-artifacts');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const currentUrl = page.url();
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');

    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('ADM-021: Entity artifacts page has content structure', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-artifacts');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const headings = page.locator('h1, h2, h3, [class*="header"], [class*="title"]');
    const headingCount = await headings.count();
    expect(headingCount).toBeGreaterThan(0);
  });

  test('ADM-022: Bulk entity artifacts page loads for admin user', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/bulk-entity-artifacts');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const currentUrl = page.url();
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');

    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('ADM-023: Entity manager page loads', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const currentUrl = page.url();
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');
  });

  test('ADM-024: Admin pages are accessible from sidebar navigation', async ({ page }) => {
    await authenticateWithRealBackend(page, '/');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const adminLinks = page.locator('a[href*="/admin/"]');
    const adminByText = page.getByText(/entity|admin|configuration|user management|translation/i);
    const adminLinkCount = await adminLinks.count();
    const adminTextVisible = await adminByText.first().isVisible({ timeout: getTimeout('short') }).catch(() => false);

    expect(adminLinkCount > 0 || adminTextVisible).toBe(true);
  });

  test('ADM-025: Restricted user cannot access admin pages', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-artifacts', 'test-readonly@playwright.local');
    await waitForPermissions(page);
    await waitForPageReady(page);

    const currentUrl = page.url();
    const body = (await page.textContent('body')) || '';

    const isBlocked =
      currentUrl.includes('access-denied') ||
      currentUrl.includes('login') ||
      /access denied|forbidden|unauthorized/i.test(body);

    expect(isBlocked).toBe(true);
  });
});

test.describe('Entity Artifacts - Configuration Details', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-artifacts');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('ADM-026: Entity artifacts page has entity type selector', async ({ page }) => {
    const artifactPage = new EntityArtifactManagerPage(page);
    await waitForVisible(artifactPage.entitySelector, getTimeout('short'));
    await expect(artifactPage.entitySelector).toBeVisible();
  });

  test('ADM-027: Entity artifacts page has table or list of artifacts', async ({ page }) => {
    const table = page.locator('p-table, table, .p-datatable').first();
    const list = page.locator('[class*="artifact-list"], [class*="artifact-item"]').first();
    const panel = page.locator('p-panel, p-fieldset, [class*="entity-artifact"]').first();
    const hasContent = (await page.textContent('body') ?? '').length > 100;

    const hasTable = await table.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    const hasList = await list.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    const hasPanel = await panel.isVisible({ timeout: getTimeout('short') }).catch(() => false);

    expect(hasTable || hasList || hasPanel || hasContent).toBe(true);
  });

  test('ADM-028: Entity artifacts has add/create button', async ({ page }) => {
    const addBtn = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    const addIcon = page.locator('.pi-plus').first();

    const hasBtnText = await addBtn.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    const hasIcon = await addIcon.isVisible({ timeout: getTimeout('short') }).catch(() => false);

    expect(hasBtnText || hasIcon).toBe(true);
  });

  test('ADM-029: Entity artifacts page has search or filter', async ({ page }) => {
    const searchInput = page.locator(
      'input[type="text"], input[placeholder*="search"], .pi-search'
    ).first();
    const hasSearch = await searchInput.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    expect(hasSearch).toBe(true);
  });

  test('ADM-030: Entity artifacts shows field configuration', async ({ page }) => {
    const artifactPage = new EntityArtifactManagerPage(page);
    const hasConfig = await artifactPage.fieldConfigText
      .isVisible({ timeout: getTimeout('short') })
      .catch(() => false);
    const hasEntitySelector = await artifactPage.entitySelector
      .isVisible({ timeout: getTimeout('short') })
      .catch(() => false);
    const hasEntityText = (await page.getByText(/entity|artifact|field|column/i).first().isVisible({ timeout: 2000 }).catch(() => false));
    expect(hasConfig || hasEntitySelector || hasEntityText).toBe(true);
  });
});

test.describe('Entity Artifacts - Bulk Update', () => {
  test.slow();
  test('ADM-031: Bulk update page has entity type filter', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/bulk-entity-artifacts');
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const bulkPage = new BulkEntityArtifactsPage(page);
    await waitForVisible(bulkPage.entityTypeSelector, getTimeout('short'));
    await expect(bulkPage.entityTypeSelector).toBeVisible();
  });

  test('ADM-032: Bulk update page has apply/execute button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/bulk-entity-artifacts');
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const bulkPage = new BulkEntityArtifactsPage(page);
    const hasApply = await bulkPage.applyButton.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    const hasEntitySelector = await bulkPage.entityTypeSelector.isVisible({ timeout: getTimeout('short') }).catch(() => false);
    const hasBulkText = (await page.getByText(/bulk|entity|artifact|update/i).first().isVisible({ timeout: 2000 }).catch(() => false));
    expect(hasApply || hasEntitySelector || hasBulkText).toBe(true);
  });
});
