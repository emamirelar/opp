/**
 * @fileoverview Opportunity Dashboard Integration E2E Tests
 *
 * Tests for opportunity-related dashboard widgets:
 * "My Opportunities" and "My Draft Opportunities" sections,
 * click-through navigation from dashboard to opportunity detail.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DASHBOARD
 * @tests 7
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForPageReady, waitForNetworkIdle } from './helpers/wait.helper';
import { DashboardPage } from './pages/dashboard.page';
import { assertVisible, assertUrlMatches } from './helpers/assertions.helper';

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
    await waitForPageReady(page);
  });

  test('DASH-001: Dashboard page loads successfully', async ({ page }) => {
    const dashboard = new DashboardPage(page);
    await dashboard.verifyDashboardVisible();
  });

  test('DASH-002: My Opportunities widget visible on dashboard', async ({ page }) => {
    const myOppsWidget = page.getByText(/my opportunities/i).first();
    await assertVisible(myOppsWidget, 10000);
  });

  test('DASH-003: My Draft Opportunities widget visible on dashboard', async ({ page }) => {
    const draftWidget = page.getByText(/draft opportunities|my draft/i).first();
    await assertVisible(draftWidget, 10000);
  });

  test('DASH-004: Opportunity widgets display count or list', async ({ page }) => {
    const dashboard = new DashboardPage(page);
    await dashboard.verifyDashboardVisible();
    const oppCard = page.locator('app-dashboard-card, [data-testid*="opportunity-widget"], .dashboard-card');
    const count = await oppCard.count();
    expect(count).toBeGreaterThan(0);
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
    await waitForPageReady(page);

    const oppLink = page.locator('a[href*="/partnerships/opportunities/"], [data-testid*="opportunity-link"]').first();
    await assertVisible(oppLink, 10000);
    await oppLink.click();
    await waitForNetworkIdle(page);
    await assertUrlMatches(page, /\/partnerships\/opportunities\/\d+/);
  });

  test('DASH-006: "View All" link navigates to opportunity list', async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const viewAllLink = page.locator('a:has-text("View All"), a:has-text("See All"), button:has-text("View All")').first();
    await assertVisible(viewAllLink, 10000);
    await viewAllLink.click();
    await waitForNetworkIdle(page);
    await assertUrlMatches(page, /\/partnerships\/opportunities/);
  });

  test('DASH-007: Read-only user sees dashboard widgets', async ({ page }) => {
    await authenticateWithRealBackend(page, DASHBOARD_URL, READONLY_USER);
    await waitForPermissions(page);
    await waitForPageReady(page);

    const dashboard = new DashboardPage(page);
    await dashboard.verifyDashboardVisible();
  });
});
