/**
 * @fileoverview Entity Artifacts Admin Page E2E Tests
 * Tests for the Entity Artifacts and Bulk Update admin pages.
 * 
 * Covers scenarios: ADM-020 to ADM-025
 * 
 * Uses API mocks - fully executable.
 * Admin pages are accessible at:
 * - /admin/entity-artifacts
 * - /admin/bulk-entity-artifacts
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Entity Artifacts Admin Page', () => {
  test('ADM-020: Entity artifacts page loads for admin user', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-artifacts');
    
    // Page should load without redirect to access-denied
    await page.waitForTimeout(3000);
    const currentUrl = page.url();
    
    // Should not be redirected to access-denied or login
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');
    
    // Some content should be rendered
    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('ADM-021: Entity artifacts page has content structure', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-artifacts');
    
    // Wait for Angular to render
    await page.waitForTimeout(3000);
    
    // Look for headings or content indicating the page loaded
    const headings = page.locator('h1, h2, h3, [class*="header"], [class*="title"]');
    const headingCount = await headings.count();
    
    // Page should have at least one heading
    expect(headingCount).toBeGreaterThan(0);
  });

  test('ADM-022: Bulk entity artifacts page loads for admin user', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/bulk-entity-artifacts');
    
    await page.waitForTimeout(3000);
    const currentUrl = page.url();
    
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');
    
    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(50);
  });

  test('ADM-023: Entity manager page loads', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-manager');
    
    await page.waitForTimeout(3000);
    const currentUrl = page.url();
    
    expect(currentUrl).not.toContain('access-denied');
    expect(currentUrl).not.toContain('login');
  });

  test('ADM-024: Admin pages are accessible from sidebar navigation', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/');
    
    // Look for admin section in sidebar
    const adminLinks = page.locator('a[href*="/admin/"]');
    const adminLinkCount = await adminLinks.count();
    
    // Admin should see admin menu items in sidebar
    expect(adminLinkCount).toBeGreaterThan(0);
  });

  test('ADM-025: Restricted user cannot access admin pages', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/admin/entity-artifacts', 'test-readonly@playwright.local');
    
    await page.waitForTimeout(3000);
    
    // Restricted user should either see access denied or be redirected
    const currentUrl = page.url();
    const body = await page.textContent('body');
    
    // Either redirected away from admin OR access denied content shown
    const isBlocked = currentUrl.includes('access-denied') || 
                      currentUrl.includes('login') ||
                      !currentUrl.includes('admin') ||
                      (body && /access denied|forbidden|unauthorized/i.test(body));
    
    expect(isBlocked).toBeTruthy();
  });
});
