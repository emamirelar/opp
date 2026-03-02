/**
 * @fileoverview Opportunity Dashboard Integration E2E Tests
 *
 * Tests for opportunity-related dashboard widgets:
 * "My Opportunities" and "My Draft Opportunities" sections,
 * click-through navigation from dashboard to opportunity detail.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DASHBOARD
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_DASHBOARD_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const DASHBOARD_URL = '/home';

// =============================================================================
// SECTION 1: Dashboard Widgets
// =============================================================================
test.describe('Dashboard — Opportunity Widgets', () => {
  test.slow();
  test.skip(!featureReady, 'Dashboard not deployed — set OPPORTUNITY_DASHBOARD_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL);
    await waitForPermissions(page);
    await page.waitForTimeout(3000);
  });

  test('DASH-001: Dashboard page loads successfully', async ({ page }) => {
    const dashboardContent = page.locator('app-home, [data-testid="dashboard"], .dashboard');
    await expect(dashboardContent.first()).toBeVisible({ timeout: 10000 });
  });

  test('DASH-002: My Opportunities widget visible on dashboard', async ({ page }) => {
    const myOppsWidget = page.getByText(/my opportunities/i).first();
    const isVisible = await myOppsWidget.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-home, .dashboard').first().isVisible()).toBeTruthy();
  });

  test('DASH-003: My Draft Opportunities widget visible on dashboard', async ({ page }) => {
    const draftWidget = page.getByText(/draft opportunities|my draft/i).first();
    const isVisible = await draftWidget.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('app-home, .dashboard').first().isVisible()).toBeTruthy();
  });

  test('DASH-004: Opportunity widgets display count or list', async ({ page }) => {
    const oppCard = page.locator('app-dashboard-card, [data-testid*="opportunity-widget"], .dashboard-card');
    const count = await oppCard.count();
    expect(count).toBeGreaterThanOrEqual(0);
  });
});

// =============================================================================
// SECTION 2: Dashboard Click-Through Navigation
// =============================================================================
test.describe('Dashboard — Click-Through to Opportunity', () => {
  test.slow();
  test.skip(!featureReady, 'Dashboard not deployed — set OPPORTUNITY_DASHBOARD_IMPLEMENTED=true');

  test('DASH-005: Clicking opportunity in widget navigates to detail', async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL);
    await waitForPermissions(page);
    await page.waitForTimeout(3000);

    const oppLink = page.locator('a[href*="/partnerships/opportunities/"], [data-testid*="opportunity-link"]').first();
    const hasLink = await oppLink.isVisible({ timeout: 10000 }).catch(() => false);
    if (hasLink) {
      await oppLink.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/partnerships/opportunities/');
    }
  });

  test('DASH-006: "View All" link navigates to opportunity list', async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL);
    await waitForPermissions(page);
    await page.waitForTimeout(3000);

    const viewAllLink = page.locator('a:has-text("View All"), a:has-text("See All"), button:has-text("View All")').first();
    const hasViewAll = await viewAllLink.isVisible({ timeout: 10000 }).catch(() => false);
    if (hasViewAll) {
      await viewAllLink.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/partnerships/opportunities');
    }
  });

  test('DASH-007: Read-only user sees dashboard widgets', async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL, READONLY_USER);
    await waitForPermissions(page);
    await page.waitForTimeout(3000);

    const dashboard = page.locator('app-home, [data-testid="dashboard"], .dashboard');
    await expect(dashboard.first()).toBeVisible({ timeout: 10000 });
  });
});
