/**
 * @fileoverview Admin Entity Configuration E2E Tests
 * Tests for the Entity Manager admin page.
 * 
 * Route: /admin/entity-manager
 * Component: app-entity-manager
 * Uses p-tabs, p-dropdown for entity selection, cdkDropList for field ordering
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Entity Config - Access', () => {
  test('EC-001: Admin can access entity manager page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager');
    await page.waitForTimeout(3000);

    expect(page.url()).toContain('entity-manager');
    expect(page.url()).not.toContain('access-denied');
  });

  test('EC-002: Page has Entity Manager heading', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager');

    const header = page.getByText(/entity manager/i).first();
    await expect(header).toBeVisible({ timeout: 10000 });
  });

  test('EC-003: Non-admin cannot access entity manager', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager', 'test-readonly@playwright.local');
    await page.waitForTimeout(3000);

    const url = page.url();
    const body = await page.textContent('body');
    const isBlocked = url.includes('access-denied') ||
                      !url.includes('entity-manager') ||
                      (body && /access denied|forbidden/i.test(body));
    expect(isBlocked).toBeTruthy();
  });
});

test.describe('Entity Config - Entity Selection', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager');
  });

  test('EC-004: Entity selector dropdown exists', async ({ page }) => {
    const entitySelector = page.locator('p-select, p-dropdown, .entity-manager-tabs select').first();
    await expect(entitySelector).toBeVisible({ timeout: 10000 });
  });

  test('EC-005: Page has tabs or entity type navigation', async ({ page }) => {
    const tabs = page.locator('p-tabs, .entity-manager-tabs').first();
    const tabsVisible = await tabs.isVisible({ timeout: 10000 }).catch(() => false);

    const entitySelector = page.locator('p-select, p-dropdown').first();
    const selectorVisible = await entitySelector.isVisible({ timeout: 5000 }).catch(() => false);

    expect(tabsVisible || selectorVisible).toBeTruthy();
  });
});

test.describe('Entity Config - Fields Management', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager');
  });

  test('EC-006: Available fields section exists', async ({ page }) => {
    const availableFields = page.locator('.available-fields-section').first();
    const fieldsText = page.getByText(/available fields/i).first();

    const sectionVisible = await availableFields.isVisible({ timeout: 10000 }).catch(() => false);
    const textVisible = await fieldsText.isVisible({ timeout: 5000 }).catch(() => false);

    expect(sectionVisible || textVisible).toBeTruthy();
  });

  test('EC-007: List view fields section exists', async ({ page }) => {
    const listViewFields = page.locator('.list-view-fields-section').first();
    const fieldsText = page.getByText(/list view/i).first();

    const sectionVisible = await listViewFields.isVisible({ timeout: 10000 }).catch(() => false);
    const textVisible = await fieldsText.isVisible({ timeout: 5000 }).catch(() => false);

    expect(sectionVisible || textVisible).toBeTruthy();
  });

  test('EC-008: Add field button exists', async ({ page }) => {
    const addFieldBtn = page.locator('.add-field-button').first();
    const addBtnText = page.getByText(/add field/i).first();

    const btnVisible = await addFieldBtn.isVisible({ timeout: 10000 }).catch(() => false);
    const textVisible = await addBtnText.isVisible({ timeout: 5000 }).catch(() => false);

    expect(btnVisible || textVisible).toBeTruthy();
  });

  test('EC-009: Entity settings button exists', async ({ page }) => {
    const settingsBtn = page.locator('.entity-settings-button').first();
    const settingsBtnText = page.getByText(/entity settings/i).first();

    const btnVisible = await settingsBtn.isVisible({ timeout: 10000 }).catch(() => false);
    const textVisible = await settingsBtnText.isVisible({ timeout: 5000 }).catch(() => false);

    expect(btnVisible || textVisible).toBeTruthy();
  });

  test('EC-010: Card preview section exists', async ({ page }) => {
    const previewSection = page.locator('.card-preview-section, app-listview-card').first();
    const previewText = page.getByText(/card preview/i).first();

    const sectionVisible = await previewSection.isVisible({ timeout: 10000 }).catch(() => false);
    const textVisible = await previewText.isVisible({ timeout: 5000 }).catch(() => false);

    expect(sectionVisible || textVisible).toBeTruthy();
  });
});
