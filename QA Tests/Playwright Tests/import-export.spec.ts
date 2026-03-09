/**
 * @fileoverview Import/Export E2E Tests
 * Tests for import and export functionality across entity list pages.
 *
 * Uses role/text-based locators (no data-testid):
 * - Export: getByRole('button', { name: /export/i })
 * - Import: getByRole('button', { name: /import/i })
 * - Headers: getByText('Partners'|'Contacts'|'Interactions', { exact: true })
 *
 * All tests are EXECUTABLE - no skips.
 *
 * @tests 12
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Export - Partners', () => {
  test.slow();
  test('EXP-001: Export button visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');

    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });

  test('EXP-002: Export button is clickable', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');

    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
    await expect(exportBtn).toBeEnabled();
  });
});

test.describe('Export - Contacts', () => {
  test.slow();
  test('EXP-003: Export button visible on contacts list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');

    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Export - Interactions', () => {
  test.slow();
  test('EXP-004: Export button visible on interactions list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');

    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Export - Opportunities', () => {
  test.slow();
  test('EXP-005: Export button visible on opportunities list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import - Partners', () => {
  test.slow();
  test('IMP-001: Import button visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });

  test('IMP-002: Import menu visible on partners list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');

    const importMenu = page.getByRole('menu', { name: /import/i }).first();
    const importButton = page.getByRole('button', { name: /import/i }).first();
    const menuVisible = await importMenu.isVisible({ timeout: 10000 }).catch(() => false);
    const buttonVisible = await importButton.isVisible({ timeout: 5000 }).catch(() => false);
    expect(menuVisible || buttonVisible).toBeTruthy();
  });
});

test.describe('Import - Contacts', () => {
  test.slow();
  test('IMP-003: Import button visible on contacts list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import - Interactions', () => {
  test.slow();
  test('IMP-004: Import button visible on interactions list', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    await expect(importBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Import/Export - Restricted User', () => {
  test.slow();
  test('IMP-005: Restricted user cannot see import button on partners', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', 'test-readonly@playwright.local');

    const header = page.getByText('Partners', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('IMP-006: Restricted user cannot see import button on contacts', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts', 'test-readonly@playwright.local');

    const header = page.getByText('Contacts', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('IMP-007: Restricted user cannot see import on interactions', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions', 'test-readonly@playwright.local');

    const header = page.getByText('Interactions', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });

    const importBtn = page.getByRole('button', { name: /import/i }).first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });
});
