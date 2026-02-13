/**
 * @fileoverview User Management Admin E2E Tests
 * Tests for the User Management admin page.
 * 
 * Route: /admin/user-management
 * Component: app-user-management
 * Key elements: #search input, #roleFilter multiselect, p-table for users,
 *   p-dialog for role editing, p-paginator
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('User Management - Access Control', () => {
  test('UM-001: Admin can access user management page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management');
    await page.waitForTimeout(3000);

    // Page should load (not redirected)
    expect(page.url()).toContain('user-management');
    expect(page.url()).not.toContain('access-denied');
  });

  test('UM-002: Page has a header/title', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management');

    // Look for page header with "User Management" text
    const header = page.getByText(/user management/i).first();
    await expect(header).toBeVisible({ timeout: 10000 });
  });

  test('UM-003: Non-admin cannot access user management', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management', 'test-readonly@playwright.local');
    await page.waitForTimeout(3000);

    const url = page.url();
    const body = await page.textContent('body');
    const isBlocked = url.includes('access-denied') ||
                      url.includes('login') ||
                      !url.includes('user-management') ||
                      (body && /access denied|forbidden|unauthorized/i.test(body));
    expect(isBlocked).toBeTruthy();
  });
});

test.describe('User Management - Search & Filters', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management');
  });

  test('UM-004: Search input is visible', async ({ page }) => {
    const searchInput = page.locator('#search, input[type="text"]').first();
    await expect(searchInput).toBeVisible({ timeout: 10000 });
  });

  test('UM-005: Role filter multiselect is visible', async ({ page }) => {
    const roleFilter = page.locator('#roleFilter, p-multiSelect').first();
    await expect(roleFilter).toBeVisible({ timeout: 10000 });
  });

  test('UM-006: Org unit filter is visible', async ({ page }) => {
    const orgFilter = page.locator('#orgUnitFilter, p-multiSelect').nth(1);
    const orgFilterVisible = await orgFilter.isVisible({ timeout: 5000 }).catch(() => false);

    // Org unit filter should be present
    expect(orgFilterVisible).toBeTruthy();
  });

  test('UM-007: Clear filters button exists', async ({ page }) => {
    const clearBtn = page.getByText(/clear filters/i).first();
    const clearVisible = await clearBtn.isVisible({ timeout: 5000 }).catch(() => false);

    expect(clearVisible).toBeTruthy();
  });
});

test.describe('User Management - User List', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management');
  });

  test('UM-008: User list table is visible', async ({ page }) => {
    const table = page.locator('p-table, table').first();
    await expect(table).toBeVisible({ timeout: 10000 });
  });

  test('UM-009: User list has column headers', async ({ page }) => {
    const table = page.locator('p-table, table').first();
    await expect(table).toBeVisible({ timeout: 10000 });

    // Should have Name, Email, Org Unit, Roles columns
    const nameHeader = table.getByText(/name/i).first();
    const emailHeader = table.getByText(/email/i).first();

    const nameVisible = await nameHeader.isVisible({ timeout: 5000 }).catch(() => false);
    const emailVisible = await emailHeader.isVisible({ timeout: 5000 }).catch(() => false);

    expect(nameVisible || emailVisible).toBeTruthy();
  });

  test('UM-010: User list has rows', async ({ page }) => {
    const table = page.locator('p-table, table').first();
    await expect(table).toBeVisible({ timeout: 10000 });

    const rows = table.locator('tbody tr, .p-datatable-tbody tr');
    const rowCount = await rows.count();

    // Should have at least one user row
    expect(rowCount).toBeGreaterThan(0);
  });

  test('UM-011: Paginator is visible for user list', async ({ page }) => {
    const paginator = page.locator('p-paginator').first();
    const paginatorVisible = await paginator.isVisible({ timeout: 10000 }).catch(() => false);

    // Paginator should be present
    expect(paginatorVisible).toBeTruthy();
  });
});

test.describe('User Management - Actions', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/user-management');
  });

  test('UM-012: Refresh button is visible', async ({ page }) => {
    const refreshBtn = page.getByText(/refresh/i).first();
    const refreshVisible = await refreshBtn.isVisible({ timeout: 10000 }).catch(() => false);

    const refreshIcon = page.locator('button .pi-refresh, button[icon*="refresh"]').first();
    const iconVisible = await refreshIcon.isVisible({ timeout: 3000 }).catch(() => false);

    expect(refreshVisible || iconVisible).toBeTruthy();
  });

  test('UM-013: Import button is visible for admin', async ({ page }) => {
    const importBtn = page.getByText(/import/i).first();
    const importVisible = await importBtn.isVisible({ timeout: 10000 }).catch(() => false);

    expect(importVisible).toBeTruthy();
  });

  test('UM-014: User row has action buttons', async ({ page }) => {
    const table = page.locator('p-table, table').first();
    await expect(table).toBeVisible({ timeout: 10000 });

    // First row should have action buttons (edit roles)
    const firstRowActions = table.locator('tbody tr:first-child button, .p-datatable-tbody tr:first-child button').first();
    const actionsVisible = await firstRowActions.isVisible({ timeout: 5000 }).catch(() => false);

    expect(actionsVisible).toBeTruthy();
  });
});
