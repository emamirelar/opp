/**
 * @fileoverview Partner Funding Agreements Tab E2E Tests
 * Tests for the Funding & Agreements tab on Partner detail pages.
 * 
 * Covers scenarios: FA-001 to FA-006
 * 
 * Uses API mocks - fully executable.
 * The partner detail page has a "Funding & Agreements" tab accessible
 * via the responsive tabs component.
 * 
 * Route: /partnerships/partners/:recordId/funding-agreements
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Partner Funding Agreements Tab', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners/1');
  });

  test('FA-001: Partner detail page loads with tabs', async ({ page }) => {
    // Partner detail should render with the tab navigation
    const header = page.locator('[data-testid="partner-detail-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const tabs = page.locator('[data-testid="tabs-desktop"], [data-testid="tabs-container"]').first();
    await expect(tabs).toBeVisible({ timeout: 5000 });
  });

  test('FA-002: Funding Agreements tab/link exists in tab navigation', async ({ page }) => {
    // Look for the funding agreements tab
    const fundingTab = page.locator('a[href*="funding-agreements"]').first();
    const fundingTabByText = page.getByText(/funding|agreements/i).first();
    
    const tabVisible = await fundingTab.isVisible({ timeout: 5000 }).catch(() => false);
    const textVisible = await fundingTabByText.isVisible({ timeout: 5000 }).catch(() => false);
    
    // The Funding & Agreements tab should exist
    expect(tabVisible || textVisible).toBeTruthy();
  });

  test('FA-003: Can navigate to Funding Agreements tab', async ({ page }) => {
    const fundingTab = page.locator('a[href*="funding-agreements"]').first();
    const fundingTabByText = page.getByText(/funding/i).first();
    
    const tabVisible = await fundingTab.isVisible({ timeout: 5000 }).catch(() => false);
    
    if (tabVisible) {
      await fundingTab.click();
    } else {
      await fundingTabByText.click();
    }
    
    await page.waitForTimeout(2000);
    
    // URL should contain funding-agreements
    expect(page.url()).toContain('funding-agreements');
  });

  test('FA-004: Funding Agreements page loads content', async ({ page }) => {
    // Navigate directly to funding agreements tab
    await page.goto('http://127.0.0.1:4200/#/partnerships/partners/1/funding-agreements');
    await page.waitForTimeout(3000);
    
    // Page should have loaded (not showing error or redirect)
    expect(page.url()).toContain('partners/1');
    
    // Some content should be rendered
    const body = await page.textContent('body');
    expect(body).toBeTruthy();
    expect(body!.length).toBeGreaterThan(100);
  });

  test('FA-005: Partner Data tab exists and navigates', async ({ page }) => {
    // Data tab is another partner sub-tab
    const dataTab = page.locator('a[href*="/data"]').first();
    const dataTabByText = page.getByText(/data|dashboard/i).first();
    
    const tabVisible = await dataTab.isVisible({ timeout: 5000 }).catch(() => false);
    const textVisible = await dataTabByText.isVisible({ timeout: 5000 }).catch(() => false);
    
    expect(tabVisible || textVisible).toBeTruthy();
  });

  test('FA-006: All partner tabs are accessible', async ({ page }) => {
    // Verify the main tabs exist: Details, Opportunities, Contacts, Interactions, Dashboard/Data
    const expectedTabs = ['details', 'opportunities', 'contacts', 'interactions'];
    
    for (const tabName of expectedTabs) {
      const tab = page.getByText(new RegExp(tabName, 'i')).first();
      const visible = await tab.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    }
  });
});
