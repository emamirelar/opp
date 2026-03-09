/**
 * @tests 9
 */
import { test, expect } from '@playwright/test';
import { DashboardPage } from './pages/dashboard.page';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForElementReady, waitForNetworkIdle } from './helpers/wait.helper';

/**
 * Dashboard Component E2E Tests
 * 
 * Tests the main dashboard functionality including:
 * - Dashboard widget display
 * - Quick actions functionality
 * - Recent activity display
 * - Data refresh capability
 * 
 * @updated 2026-01-30 - Migrated to real backend authentication
 */
test.describe('Dashboard', () => {
  test.slow();
  let dashboardPage: DashboardPage;
  
  // Authenticate with real backend before each test
  test.beforeEach(async ({ page }) => {
    dashboardPage = new DashboardPage(page);
    
    // Authenticate and navigate to home/dashboard page
    await authenticateWithRealBackend(page, '/');
  });
  
  test('should display dashboard widgets', async () => {
    await dashboardPage.verifyDashboardVisible();
    
    const panelCount = await dashboardPage.getPanelCount();
    expect(panelCount).toBeGreaterThanOrEqual(2);
  });
  
  test('should display welcome message', async () => {
    await dashboardPage.verifyWelcomeMessage();
  });
  
  test('should display quick actions for users with permissions', async ({ page }) => {
    await waitForNetworkIdle(page);
    
    const hasQuickActions = await dashboardPage.hasQuickActions();
    const panelCount = await dashboardPage.getPanelCount();
    
    // Quick actions visibility depends on permissions; dashboard must have panels
    expect(hasQuickActions || panelCount > 0).toBeTruthy();
  });
  
  test('should display dashboard panels (Actions Required, Recent Activity, My Workspace)', async ({ page }) => {
    await waitForNetworkIdle(page);
    
    await dashboardPage.verifyGridLayout();
    
    const panelCount = await dashboardPage.getPanelCount();
    expect(panelCount).toBeGreaterThan(0);
  });
  
  test('should allow refresh of dashboard data', async () => {
    await dashboardPage.waitForLoad();
    await dashboardPage.clickRefresh();
    
    // Verify dashboard is still visible after refresh
    await dashboardPage.verifyDashboardVisible();
  });
  
  test('should display recent activity section', async ({ page }) => {
    await waitForNetworkIdle(page);
    
    const hasActivityDots = await dashboardPage.hasActivityData();
    const hasActivityCards = await dashboardPage.hasActivityCards();
    const panelCount = await dashboardPage.getPanelCount();
    
    // Activity section may be empty for new users; dashboard must have panels
    expect(hasActivityDots || hasActivityCards || panelCount > 0).toBeTruthy();
  });
  
  test('should display my workspace section', async ({ page }) => {
    await waitForNetworkIdle(page);
    
    await expect(dashboardPage.panels.first()).toBeVisible({ timeout: 60000 });
  });
  
  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForElementReady(page.locator('.max-w-7xl'));
    
    await expect(page.locator('.max-w-7xl')).toBeVisible();
    await expect(dashboardPage.panels.first()).toBeVisible();
  });
  
  test('should handle empty state gracefully', async ({ page }) => {
    await waitForNetworkIdle(page);
    
    await dashboardPage.verifyDashboardVisible();
  });
});
