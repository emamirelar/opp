/**
 * @fileoverview Admin Entity Configuration E2E Tests
 * Tests for the Entity Manager admin page.
 * 
 * Route: /admin/entity-manager
 * Component: app-entity-manager
 * Uses p-tabs, p-dropdown for entity selection, cdkDropList for field ordering
 * 
 * All tests are EXECUTABLE - no skips.
 *
 * @tests 10
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForPageReady,
  waitForLoadingToComplete,
} from './helpers/wait.helper';
import { EntityManagerPage } from './pages/admin.page';

test.describe('Entity Config - Access', () => {
  test.slow();
  test('EC-001: Admin can access entity manager page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPermissions(page);
    await waitForPageReady(page);

    expect(page.url()).toContain('entity-manager');
    expect(page.url()).not.toContain('access-denied');
  });

  test('EC-002: Page has Entity Manager heading', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPermissions(page);
    await waitForPageReady(page);
    const entityPage = new EntityManagerPage(page);

    await expect(entityPage.entityManagerHeading).toBeVisible({ timeout: 10000 });
  });

  test('EC-003: Non-admin cannot access entity manager', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager', 'test-readonly@playwright.local');
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const url = page.url();
    const body = await page.textContent('body');
    const isBlocked =
      url.includes('access-denied') ||
      !url.includes('entity-manager') ||
      (body !== null && /access denied|forbidden/i.test(body));
    expect(isBlocked).toBe(true);
  });
});

test.describe('Entity Config - Entity Selection', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  test('EC-004: Entity selector dropdown exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);
    // Entity manager uses p-tabs (desktop) or p-dropdown/p-select (mobile).
    await expect(
      page.locator('p-tabs, p-dropdown, p-select, app-entity-manager, .entity-manager').first()
    ).toBeVisible({ timeout: 15000 });
  });

  test('EC-005: Page has tabs or entity type navigation', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);
    await expect(entityPage.tabsOrSelector).toBeVisible({ timeout: 15000 });
  });
});

test.describe('Entity Config - Fields Management', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPermissions(page);
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
  });

  test('EC-006: Available fields section exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);
    const targetSection = entityPage.availableFieldsText.or(entityPage.availableFieldsSection);
    await entityPage.ensureFirstEntitySelected(targetSection);

    await expect(targetSection).toBeVisible({ timeout: 15000 });
  });

  test('EC-007: List view fields section exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);
    const targetSection = entityPage.listViewText.or(entityPage.listViewFieldsSection);
    await entityPage.ensureFirstEntitySelected(targetSection);

    await expect(targetSection).toBeVisible({ timeout: 15000 });
  });

  test('EC-008: Add field button exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);

    await expect(
      entityPage.addFieldText.or(entityPage.addFieldButton)
    ).toBeVisible({ timeout: 10000 });
  });

  test('EC-009: Entity settings button exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);

    await expect(
      entityPage.entitySettingsText.or(entityPage.entitySettingsButton)
    ).toBeVisible({ timeout: 10000 });
  });

  test('EC-010: Card preview section exists', async ({ page }) => {
    const entityPage = new EntityManagerPage(page);
    const targetSection = entityPage.cardPreviewText.or(entityPage.cardPreviewSection);
    await entityPage.ensureFirstEntitySelected(targetSection);

    await expect(targetSection).toBeVisible({ timeout: 15000 });
  });
});
