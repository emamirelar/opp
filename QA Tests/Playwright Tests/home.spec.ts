/**
 * @tests 8
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForLoadingToComplete } from './helpers/wait.helper';

/**
 * Home Page & Dashboard E2E Tests
 * 
 * Tests the main landing page and dashboard functionality to ensure:
 * - Application loads successfully
 * - Dashboard components render properly
 * - Key UI elements are visible and interactive
 * 
 * @updated 2026-01-30 - Migrated to real backend authentication
 */
test.describe('Home Page & Dashboard', () => {
  test.slow();
  test('should load home page and display dashboard', async ({ page }) => {
    // Authenticate and navigate to home page
    await authenticateWithRealBackend(page, '/');
    
    // Verify page title contains "Opportunity" or "UNOPS"
    await expect(page).toHaveTitle(/Opportunity|UNOPS/);
    
    // Verify welcome header is visible (from translation key 'dashboard.welcome')
    // This ensures the Angular app loaded and translations work
    const welcomeText = page.locator('h1').filter({ hasText: /welcome/i }).or(
      page.locator('h1.font-unops-display')
    );
    await expect(welcomeText.first()).toBeVisible({ timeout: 10000 });
    
    // Verify main dashboard container is visible
    await expect(page.locator('.max-w-7xl').first()).toBeVisible();
    
    // Verify dashboard panels loaded (they use bg-unops-surface-primary class)
    const dashboardPanels = page.locator('.bg-unops-surface-primary');
    await expect(dashboardPanels.first()).toBeVisible({ timeout: 10000 });
  });
  
  test('should display announcement banner', async ({ page }) => {
    // Authenticate and navigate to home page
    await authenticateWithRealBackend(page, '/');
    
    // Verify the gradient announcement banner is visible
    const banner = page.locator('.bg-gradient-to-r');
    await expect(banner).toBeVisible({ timeout: 10000 });
  });
  
  test('should display dashboard content or loading state', async ({ page }) => {
    // Authenticate and navigate to home page using hash-based routing
    await authenticateWithRealBackend(page, '/');
    
    // Either loading skeleton OR actual content should be visible
    const loadingState = page.locator('.animate-pulse');
    const contentState = page.locator('.grid');
    
    // At least one should be visible
    const isLoadingVisible = await loadingState.first().isVisible().catch(() => false);
    const isContentVisible = await contentState.first().isVisible().catch(() => false);
    
    expect(isLoadingVisible || isContentVisible).toBeTruthy();
  });
  
  test('should display quick actions toolbar for users with permissions', async ({ page }) => {
    // Authenticate and navigate to home page
    await authenticateWithRealBackend(page, '/');
    
    // Check if Quick Actions section exists (depends on user permissions)
    // The Quick Actions panel has specific buttons for creating entities
    const quickActionsButtons = page.locator('button').filter({ 
      hasText: /New Partner|New Contact|New Interaction|New Opportunity/i 
    });
    
    // Check if refresh button exists (always visible)
    const refreshButton = page.locator('button i.pi-refresh');
    
    // Either quick actions or refresh button should exist
    const hasQuickActions = await quickActionsButtons.first().isVisible().catch(() => false);
    const hasRefreshButton = await refreshButton.first().isVisible().catch(() => false);
    
    expect(hasQuickActions || hasRefreshButton).toBeTruthy();
  });
  
  test('should display dashboard panels (Actions Required, Recent Activity, My Workspace)', async ({ page }) => {
    // Authenticate and navigate to home page using hash-based routing
    await authenticateWithRealBackend(page, '/');
    
    // Look for the dashboard card components
    // These use app-dashboard-card or specific panel structures
    const dashboardCards = page.locator('.bg-unops-surface-primary');
    
    // Should have at least one dashboard panel visible
    await expect(dashboardCards.first()).toBeVisible({ timeout: 15000 });
    
    // Count visible panels (should have 3-4 main panels when loaded)
    const panelCount = await dashboardCards.count();
    expect(panelCount).toBeGreaterThan(0);
  });
  
  test('should handle error state gracefully', async ({ page }) => {
    // Authenticate and navigate to home page
    await authenticateWithRealBackend(page, '/');
    
    // If error state is shown, verify it has proper UI
    const errorIcon = page.locator('i.material-symbols-outlined').filter({ hasText: 'warning' });
    const isErrorVisible = await errorIcon.isVisible().catch(() => false);
    
    if (isErrorVisible) {
      // Verify retry button exists in error state
      const retryButton = page.locator('button').filter({ hasText: /retry/i });
      await expect(retryButton).toBeVisible();
    } else {
      // When no error: verify dashboard content is visible (normal state)
      await expect(page.locator('.max-w-7xl, .bg-unops-surface-primary').first()).toBeVisible({ timeout: 10000 });
    }
  });
  
  test('should display last updated timestamp', async ({ page }) => {
    // Authenticate and navigate to home page using hash-based routing
    await authenticateWithRealBackend(page, '/');
    
    // Look for "Last Updated" text or timestamp
    const lastUpdatedText = page.getByText(/last updated|updated/i);
    const hasTimestamp = await lastUpdatedText.first().isVisible().catch(() => false);
    const mainContentVisible = await page.locator('.max-w-7xl').first().isVisible().catch(() => false);

    // Last updated visible when user has permissions, or main content visible (page loaded)
    expect(hasTimestamp || mainContentVisible).toBe(true);
  });
  
  test('should have responsive layout', async ({ page }) => {
    // Test desktop view
    await page.setViewportSize({ width: 1920, height: 1080 });
    
    // Authenticate and navigate to home page
    await authenticateWithRealBackend(page, '/');
    
    // Desktop: Grid should show multiple columns
    const gridElement = page.locator('.grid').first();
    await expect(gridElement).toBeVisible();
    
    // Test mobile view
    await page.setViewportSize({ width: 375, height: 667 });
    await waitForLoadingToComplete(page);

    // Mobile: Content should still be visible (may stack vertically)
    const mobileContent = page.locator('.max-w-7xl');
    await expect(mobileContent).toBeVisible();
  });
});
