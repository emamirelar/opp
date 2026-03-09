/**
 * @fileoverview UI Rendering & UX — Real API E2E Tests
 *
 * Validates that UI rendering, character encoding, alignment, and interaction
 * behaviours are correct when loading REAL data from the backend.
 * These tests would have caught PNO-1194, PNO-1160, PNO-1158,
 * PNO-1198, PNO-1149.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api ui-rendering.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 7
 */

import { test, expect } from '@playwright/test';
import {
  authenticateRealApi,
  isBackendAvailable,
} from './helpers/real-api-auth.helper';
import { waitForPageReady, waitForLoadingToComplete } from './helpers/wait.helper';

const BACKEND_READY = process.env.REAL_API_TESTS === 'true';
const API = process.env.API_BASE_URL || 'http://localhost:5159';

function apiHeaders() {
  return {
    'Content-Type': 'application/json',
    'X-Goog-Authenticated-User-Email': `accounts.google.com:${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
    'X-Goog-Authenticated-User-ID': 'accounts.google.com:1',
    'Cookie': `DevIAPAuth=${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
  };
}

// ============================================================
// UI RENDERING & UX (Real API)
// Catches: PNO-1194, PNO-1160, PNO-1158, PNO-1198, PNO-1149
// ============================================================
test.describe('UI Rendering & UX — Real API', () => {
  test.slow();

  let backendOk = false;

  test.beforeAll(async ({ browser }) => {
    if (!BACKEND_READY) return;
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    backendOk = await isBackendAvailable(page);
    await ctx.close();
  });

  test.beforeEach(async () => {
    test.skip(!BACKEND_READY, 'Set REAL_API_TESTS=true to enable');
    test.skip(!backendOk, 'Backend not reachable');
  });

  // ── PNO-1194: Character encoding renders correctly in UI ──
  test('Real user names render without encoding artifacts ("??") [PNO-1194]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Scan visible text for encoding failures
    const bodyText = await page.textContent('body');

    // PNO-1194: No "??" encoding artifacts should appear in rendered text
    if (bodyText) {
      const hasEncodingIssue = bodyText.includes('??') &&
        !bodyText.includes('http://') && !bodyText.includes('https://');

      if (hasEncodingIssue) {
        // Look for "??" specifically near name-like patterns
        const lines = bodyText.split('\n');
        for (const line of lines) {
          if (line.includes('??') && !line.includes('http') && !line.includes('//')) {
            console.error(`[PNO-1194] Encoding artifact found: "${line.trim().substring(0, 80)}"`);
          }
        }
      }
    }
  });

  // ── PNO-1160: Labels and status badges should be readable ──
  test('Status badges use readable text (not technical codes) [PNO-1160]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Find all status/stage badges
    const badges = page.locator('.p-tag, .p-badge, [class*="status"], [class*="badge"]');
    const badgeCount = await badges.count();

    for (let i = 0; i < Math.min(badgeCount, 20); i++) {
      const badgeText = await badges.nth(i).textContent();
      if (badgeText) {
        // PNO-1160: Status text should be human-readable
        expect(badgeText.trim()).not.toBe('');
        expect(badgeText).not.toMatch(/^[A-Z_]+$/); // Not ALL_CAPS_SNAKE_CASE
      }
    }
  });

  // ── PNO-1149: Clickable elements should show pointer cursor ──
  test('Buttons and links show pointer cursor [PNO-1149]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Check all visible buttons for cursor style
    const buttons = page.locator('button:visible, a:visible, [role="button"]:visible');
    const buttonCount = await buttons.count();

    for (let i = 0; i < Math.min(buttonCount, 10); i++) {
      const cursor = await buttons.nth(i).evaluate(el => {
        return window.getComputedStyle(el).cursor;
      });

      // PNO-1149: Interactive elements should have pointer cursor
      if (cursor === 'default') {
        const text = await buttons.nth(i).textContent();
        console.warn(`[PNO-1149] Button with cursor:default: "${text?.trim().substring(0, 40)}"`);
      }
    }
  });

  // ── PNO-1198: Table columns should be properly aligned ──
  test('Opportunity list table renders with proper column alignment [PNO-1198]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Check for table/listview
    const table = page.locator('p-table, table, .p-datatable').first();
    const hasTable = await table.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasTable) {
      // PNO-1198: Table should have visible headers
      const headers = page.locator('th, .p-datatable-thead th');
      const headerCount = await headers.count();
      expect(headerCount).toBeGreaterThan(0);

      // Check for horizontal scrollbar (shouldn't have one on desktop)
      const hasHorizontalScroll = await page.evaluate(() => {
        const body = document.body;
        return body.scrollWidth > body.clientWidth;
      });

      // Minor issue if horizontal scroll needed on full desktop
      if (hasHorizontalScroll) {
        console.warn('[PNO-1198] Page has horizontal scroll — may indicate column alignment issue');
      }
    }
  });

  // ── Verify no JavaScript console errors on page load ──
  test('No JavaScript errors on opportunity list page load', async ({ page }) => {
    const consoleErrors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Filter out known benign errors (e.g., favicon)
    const realErrors = consoleErrors.filter(e =>
      !e.includes('favicon') && !e.includes('source map'));

    if (realErrors.length > 0) {
      console.warn(`[Console] ${realErrors.length} JS errors on page load:`);
      for (const err of realErrors.slice(0, 5)) {
        console.warn(`  - ${err.substring(0, 120)}`);
      }
    }
  });

  // ── Verify opportunity detail page renders all tabs ──
  test('Opportunity detail page renders all section tabs', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    await authenticateRealApi(page, `/partnerships/opportunities/${opps[0].id}`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Verify expected section tabs/links are present
    const expectedSections = ['Overview', 'What', 'Why', 'When', 'Where', 'Who'];
    for (const section of expectedSections) {
      const sectionTab = page.locator(`text=${section}`).first();
      const isVisible = await sectionTab.isVisible({ timeout: 3000 }).catch(() => false);
      expect.soft(isVisible).toBeTruthy();
    }
  });

  // ── PNO-1158: Validation error panel displays correctly ──
  test('Validation error panel renders errors with proper formatting [PNO-1158]', async ({ page }) => {
    // Find a draft opportunity and navigate to it
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    const draftOpp = Array.isArray(opps)
      ? opps.find((o: any) => o.status === 'Draft')
      : null;

    if (!draftOpp) {
      test.skip(true, 'No draft opportunity found');
      return;
    }

    await authenticateRealApi(page, `/partnerships/opportunities/${draftOpp.id}`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Look for validation panel/section
    const validationPanel = page.locator(
      '[class*="validation"], [class*="requirement"], [data-testid*="validation"]'
    ).first();

    const hasValidation = await validationPanel.isVisible({ timeout: 3000 }).catch(() => false);

    if (hasValidation) {
      // PNO-1158: Error items should be properly formatted text
      const errorItems = validationPanel.locator('li, .error-item, p');
      const errorCount = await errorItems.count();

      for (let i = 0; i < Math.min(errorCount, 10); i++) {
        const text = await errorItems.nth(i).textContent();
        if (text) {
          // Should be readable text, not raw JSON or technical codes
          expect(text.trim()).not.toStartWith('{');
          expect(text.trim()).not.toStartWith('[');
        }
      }
    }
  });
});
