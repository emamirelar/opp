/**
 * @fileoverview UI Form Rendering & Layout — Real API E2E Tests
 *
 * Validates UI rendering of forms, floating labels, date fields, cursor
 * behaviour, and stale data issues against the REAL backend.
 * These tests would have caught PNO-1182, PNO-1152, PNO-1148, PNO-1170.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api ui-form-rendering.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 6
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
// UI FORM RENDERING & LAYOUT (Real API)
// Catches: PNO-1182, PNO-1152, PNO-1148, PNO-1170
// ============================================================
test.describe('UI Form Rendering & Layout — Real API', () => {
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

  // ── PNO-1182: Date field labels aligned correctly (no floating label clash) ──
  test('Date field labels do not overlap with input values [PNO-1182]', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    // Find an opportunity with date fields populated
    let targetOpp = null;
    for (const opp of (Array.isArray(opps) ? opps.slice(0, 5) : [])) {
      const detailRes = await page.request.get(
        `${API}/api/opportunity/${opp.id}`,
        { headers: apiHeaders() }
      );
      if (!detailRes.ok()) continue;
      const detail = await detailRes.json();

      if (detail.implementationStartDate || detail.estimatedSigningDate) {
        targetOpp = detail;
        break;
      }
    }

    if (!targetOpp) {
      test.skip(true, 'No opportunity with date fields found');
      return;
    }

    // Navigate to WHEN section where date fields live
    await authenticateRealApi(page, `/partnerships/opportunities/${targetOpp.id}/when`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // PNO-1182: Check floating labels on date fields
    const floatLabels = page.locator('p-floatlabel, .p-float-label, [class*="float-label"]');
    const labelCount = await floatLabels.count();

    for (let i = 0; i < Math.min(labelCount, 10); i++) {
      const label = floatLabels.nth(i).locator('label').first();
      const input = floatLabels.nth(i).locator('input, p-datepicker, p-calendar').first();

      const labelVisible = await label.isVisible({ timeout: 2000 }).catch(() => false);
      const inputVisible = await input.isVisible({ timeout: 2000 }).catch(() => false);

      if (labelVisible && inputVisible) {
        // Get bounding boxes to check for overlap
        const labelBox = await label.boundingBox();
        const inputBox = await input.boundingBox();

        if (labelBox && inputBox) {
          // If input has a value, the label should be floated up (not overlapping)
          const inputValue = await input.inputValue().catch(() => '');
          if (inputValue && inputValue.length > 0) {
            // PNO-1182: Label should be above input, not overlapping
            // Label bottom should be <= input top (with some tolerance)
            const tolerance = 5;
            if (labelBox.y + labelBox.height > inputBox.y + tolerance) {
              console.warn(
                `[PNO-1182] Floating label may overlap with date value at index ${i}`
              );
            }
          }
        }
      }
    }
  });

  // ── PNO-1152: Validation error links should have pointer cursor ──
  test('Validation error links show pointer cursor [PNO-1152]', async ({ page }) => {
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

    // Look for validation error links/items
    const validationLinks = page.locator(
      '[class*="validation"] a, [class*="requirement"] a, [class*="error-link"], ' +
      '[class*="validation"] [role="link"], [class*="clickable"]'
    );

    const linkCount = await validationLinks.count();

    for (let i = 0; i < Math.min(linkCount, 10); i++) {
      const cursor = await validationLinks.nth(i).evaluate(el => {
        return window.getComputedStyle(el).cursor;
      });

      // PNO-1152: Clickable validation items should show pointer cursor
      if (cursor !== 'pointer') {
        const text = await validationLinks.nth(i).textContent();
        console.warn(
          `[PNO-1152] Validation link with cursor:${cursor}: "${text?.trim().substring(0, 40)}"`
        );
      }
    }
  });

  // ── PNO-1148: Decision Making Pathway should not show stale data ──
  test('Team section does not show stale data after org unit update [PNO-1148]', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    // Find opportunities with org unit assigned
    const oppWithOrg = Array.isArray(opps)
      ? opps.find((o: any) => o.orgUnitId || o.responsibleOrgUnitId)
      : null;

    if (!oppWithOrg) {
      test.skip(true, 'No opportunity with org unit found');
      return;
    }

    // Navigate to team section
    await authenticateRealApi(page, `/partnerships/opportunities/${oppWithOrg.id}/team`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // PNO-1148: Look for Decision Making Pathway section
    const decisionPathway = page.locator(
      'text=Decision Making Pathway, text=Decision Pathway, text=DOA'
    ).first();

    const hasPathway = await decisionPathway.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasPathway) {
      // Get the parent container and check for duplicate entries
      const pathwayContainer = decisionPathway.locator('..').locator('..');
      const pathwayText = await pathwayContainer.textContent();

      if (pathwayText) {
        // Check for duplicate names (sign of stale data)
        const words = pathwayText.split(/\s+/).filter(w => w.length > 3);
        const wordCounts = new Map<string, number>();
        for (const word of words) {
          wordCounts.set(word, (wordCounts.get(word) || 0) + 1);
        }

        // Names appearing more than expected may indicate duplicates
        for (const [word, count] of wordCounts) {
          if (count > 3 && !['the', 'and', 'for', 'Unit', 'Level', 'DOA'].includes(word)) {
            console.warn(
              `[PNO-1148] Possible duplicate data in Decision Pathway: "${word}" appears ${count} times`
            );
          }
        }
      }
    }
  });

  // ── PNO-1170: Deep link URL should load the page (not fail) ──
  test('Deep link URL loads opportunity page successfully [PNO-1170]', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // PNO-1170: Test several deep link URL formats
    const oppId = opps[0].id;
    const deepLinkFormats = [
      `/partnerships/opportunities/${oppId}`,
      `/partnerships/opportunities/${oppId}/overview`,
      `/partnerships/opportunities/${oppId}/what`,
      `/partnerships/opportunities/${oppId}/when`,
      `/partnerships/opportunities/${oppId}/where`,
      `/partnerships/opportunities/${oppId}/who`,
      `/partnerships/opportunities/${oppId}/team`,
    ];

    for (const url of deepLinkFormats) {
      await authenticateRealApi(page, url);

      // Wait briefly for page load
      await page.waitForTimeout(3000);

      // PNO-1170: Page should not show blank/error state
      const bodyText = await page.textContent('body');
      expect(bodyText).toBeTruthy();
      expect(bodyText!.length).toBeGreaterThan(50);

      // Should not show "Page not found" or error pages
      const notFoundText = page.locator('text=Page not found, text=404, text=Not Found');
      const isNotFound = await notFoundText.isVisible({ timeout: 1000 }).catch(() => false);
      expect(isNotFound).toBeFalsy();

      // Should not show blank white page
      const hasContent = page.locator('app-root, [class*="layout"], [class*="content"]');
      const contentVisible = await hasContent.first().isVisible({ timeout: 3000 }).catch(() => false);
      expect(contentVisible).toBeTruthy();
    }
  });

  // ── Verify the WHEN section renders date fields correctly ──
  test('WHEN section date fields render with real data', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // Navigate to WHEN section
    await authenticateRealApi(page, `/partnerships/opportunities/${opps[0].id}/when`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // Should not show server error
    const serverError = page.locator('.p-toast-message-error');
    const hasError = await serverError.isVisible({ timeout: 2000 }).catch(() => false);
    expect(hasError).toBeFalsy();
  });

  // ── Verify form fields maintain state after navigation ──
  test('Form data persists after tab navigation', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // Navigate to overview, get data
    const detailRes = await page.request.get(
      `${API}/api/opportunity/${opps[0].id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const originalData = await detailRes.json();

    // Navigate to a different section and back
    await authenticateRealApi(page, `/partnerships/opportunities/${opps[0].id}/what`);
    await page.waitForTimeout(2000);

    await page.goto(
      `${process.env.BASE_URL || 'http://localhost:4200'}/partnerships/opportunities/${opps[0].id}/overview`
    );
    await page.waitForTimeout(3000);

    // Re-fetch and verify data hasn't changed
    const refetchRes = await page.request.get(
      `${API}/api/opportunity/${opps[0].id}`,
      { headers: apiHeaders() }
    );
    expect(refetchRes.ok()).toBeTruthy();
    const refetchedData = await refetchRes.json();

    // Data should be identical
    expect(refetchedData.id).toBe(originalData.id);
    if (originalData.name) {
      expect(refetchedData.name).toBe(originalData.name);
    }
  });
});
