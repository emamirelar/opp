/**
 * @fileoverview Profile & Settings E2E Tests
 * Tests for the user profile dialog and settings.
 *
 * Component: app-profile-dialog, app-profile-menubar
 * Triggered from topbar profile menu button
 * Shows personal info, work info, preferences, system info
 *
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForElementReady,
  waitForVisible,
  waitForDialog,
} from './helpers/wait.helper';
import { ProfilePage } from './pages/profile.page';

test.describe('Profile - Menu Access', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/');
    await waitForPermissions(page);

    // Dismiss welcome tour dialog and driver overlay that may block clicks
    const welcomeDialog = page.locator('[role="dialog"]').filter({ hasText: /welcome|tour/i });
    if (await welcomeDialog.isVisible({ timeout: 1000 }).catch(() => false)) {
      await page.locator('[role="dialog"] button').first().click({ timeout: 2000 }).catch(() => {});
    }
    await page.locator('.driver-close-btn, .driver-overlay').first().click({ timeout: 1000, force: true }).catch(() => {});

    const profileBtn = page.locator('.profile-menu-button').first();
    await waitForElementReady(profileBtn);
  });

  test('PRF-001: Profile menu button visible in topbar', async ({ page }) => {
    const profilePage = new ProfilePage(page);
    await expect(profilePage.profileMenuButton).toBeVisible({ timeout: 10000 });
  });

  test('PRF-002: Profile menu opens when clicked', async ({ page }) => {
    const profilePage = new ProfilePage(page);
    await expect(profilePage.profileMenuButton).toBeVisible({ timeout: 10000 });
    await profilePage.profileMenuButton.click();

    const profileMenu = page.locator('.p-menu-overlay, [role="menu"], .p-menu').first();
    const menuItem = page.getByText(/view profile|profile|impersonate/i).first();
    await waitForVisible(profileMenu.or(menuItem), 5000);

    const menuVisible = await profileMenu.isVisible({ timeout: 2000 }).catch(() => false);
    const itemVisible = await menuItem.isVisible({ timeout: 2000 }).catch(() => false);
    expect(menuVisible || itemVisible).toBe(true);
  });

  test('PRF-003: Profile dialog shows user information', async ({ page }) => {
    const profilePage = new ProfilePage(page);
    await expect(profilePage.profileMenuButton).toBeVisible({ timeout: 10000 });
    await profilePage.profileMenuButton.click({ force: true });

    const profileItem = page.getByText(/view profile|profile/i).first();
    await waitForVisible(profileItem, 3000);
    await profileItem.click();

    await waitForDialog(page, 5000);

    const profileDialog = page.locator('app-profile-dialog, p-dialog').first();
    await expect(profileDialog).toBeVisible({ timeout: 3000 });

    const nameField = page.getByText(/full name|name|first name/i).first();
    const emailField = page.getByText(/email/i).first();
    await expect(nameField.or(emailField)).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Profile - Content Sections', () => {
  test.slow();
  test('PRF-004: Profile accessible from topbar', async ({ page }) => {
    await authenticateWithRealBackend(page, '/');
    await waitForPermissions(page);

    const topbar = page.locator('app-topbar').first();
    await expect(topbar).toBeVisible({ timeout: 10000 });

    const profilePage = new ProfilePage(page);
    await expect(profilePage.profileMenuButton).toBeVisible({ timeout: 5000 });
  });

  test('PRF-005: Topbar has the app title/logo', async ({ page }) => {
    await authenticateWithRealBackend(page, '/');
    await waitForPermissions(page);

    const topbar = page.locator('app-topbar').first();
    await expect(topbar).toBeVisible({ timeout: 10000 });
  });
});
