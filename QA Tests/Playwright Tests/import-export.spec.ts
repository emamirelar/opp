/**
 * @fileoverview Import/Export E2E Tests
 * Tests for import and export functionality across entity list pages.
 * 
 * Uses data-testid selectors:
 * - data-testid="export-button" (partners, contacts, interactions, opportunities)
 * - data-testid="import-button" (partners, contacts, interactions)
 * - data-testid="import-menu" (partners, contacts, interactions)
 * - app-import-dialog for import dialogs
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Export - Partners', () => {
  test('EXP-001: Export button visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');

    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });

  test('EXP-002: Export button is clickable', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');

    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
    await expect(exportBtn).toBeEnabled();
  });
});

test.describe('Export - Contacts', () => {
  test('EXP-003: Export button visible on contacts list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts');

    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Export - Interactions', () => {
  test('EXP-004: Export button visible on interactions list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/interactions');

    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Export - Opportunities', () => {
  test('EXP-005: Export button visible on opportunities list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities');

    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import - Partners', () => {
  test('IMP-001: Import button visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');

    const importBtn = page.locator('[data-testid="import-button"]').first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });

  test('IMP-002: Import menu visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');

    const importMenu = page.locator('[data-testid="import-menu"]').first();
    await expect(importMenu).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import - Contacts', () => {
  test('IMP-003: Import button visible on contacts list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts');

    const importBtn = page.locator('[data-testid="import-button"]').first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import - Interactions', () => {
  test('IMP-004: Import button visible on interactions list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/interactions');

    const importBtn = page.locator('[data-testid="import-button"]').first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import/Export - Restricted User', () => {
  test('IMP-005: Restricted user cannot see import button on partners', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners', 'test-readonly@playwright.local');

    const header = page.locator('[data-testid="partners-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.locator('[data-testid="import-button"]').first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('IMP-006: Restricted user cannot see import button on contacts', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts', 'test-readonly@playwright.local');

    const header = page.locator('[data-testid="contacts-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.locator('[data-testid="import-button"]').first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('IMP-007: Restricted user cannot see import on interactions', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/interactions', 'test-readonly@playwright.local');

    const header = page.locator('[data-testid="interactions-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.locator('[data-testid="import-button"]').first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });
});
