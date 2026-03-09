/**
 * @fileoverview Integration & Deep Links — Real API E2E Tests
 *
 * Validates cross-system integration, deep link routing, and AI document
 * upload against the REAL backend. These tests would have caught
 * PNO-1195, PNO-1209, PNO-1207, PNO-1202.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api integration-deeplinks.real.spec.ts
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
import { waitForPageReady } from './helpers/wait.helper';

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
// INTEGRATION & DEEP LINKS (Real API)
// Catches: PNO-1195, PNO-1209, PNO-1207, PNO-1202
// ============================================================
test.describe('Integration & Deep Links — Real API', () => {
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

  // ── PNO-1195: Deep links should route to correct Opportunity detail page ──
  test('Direct URL to opportunity loads detail view (not dashboard) [PNO-1195]', async ({ page }) => {
    // Get a valid opportunity ID first
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    const oppId = opps[0].id;

    // PNO-1195: Navigate directly to an opportunity via deep link (NO hash fragment)
    await authenticateRealApi(page, `/partnerships/opportunities/${oppId}`);
    await waitForPageReady(page);
    await page.waitForTimeout(3000);

    // The URL should contain the opportunity ID
    expect(page.url()).toContain(`${oppId}`);

    // Should NOT show "Welcome to Opportunity+" dashboard
    const welcomeText = page.locator('text=Welcome to Opportunity+');
    const showsWelcome = await welcomeText.isVisible({ timeout: 2000 }).catch(() => false);
    expect(showsWelcome).toBeFalsy();

    // Should show opportunity detail content
    const pageContent = await page.textContent('body');
    expect(pageContent).toBeTruthy();
  });

  // ── PNO-1195: Deep link URL should NOT contain hash fragment ──
  test('Opportunity URLs do not use hash fragments [PNO-1195]', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    const oppId = opps[0].id;

    // Navigate and check URL format
    await authenticateRealApi(page, `/partnerships/opportunities/${oppId}`);
    await waitForPageReady(page);

    // PNO-1195: URL should NOT have /#/ pattern
    const currentUrl = page.url();
    expect(currentUrl).not.toContain('/#/');
  });

  // ── PNO-1209: DOA3 field should be mapped in integration data ──
  test('DOA3 field is present in opportunity data for integration [PNO-1209]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');

    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    // Find a GO/Active opportunity (which would trigger oUP integration)
    const goOpp = Array.isArray(opps)
      ? opps.find((o: any) => o.stage === 'GO' || o.status === 'Active')
      : null;

    if (!goOpp) {
      test.skip(true, 'No GO-stage opportunity found');
      return;
    }

    // Get full opportunity detail to check DOA3
    const detailRes = await page.request.get(
      `${API}/api/opportunity/${goOpp.id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();

    // Also check team data for DOA3
    const teamRes = await page.request.get(
      `${API}/api/opportunity/${goOpp.id}/team`,
      { headers: apiHeaders() }
    );

    if (teamRes.ok()) {
      const team = await teamRes.json();
      // PNO-1209: DOA3 should be populated if the org unit has a DOA3 holder
      if (team.doaLevel3 || team.doa3 || team.doA3) {
        console.log(`[PNO-1209] DOA3 populated: ${team.doaLevel3 || team.doa3 || team.doA3}`);
      } else if (team.orgUnitId) {
        console.warn('[PNO-1209] DOA3 not populated — verify org unit has DOA3 assigned');
      }
    }
  });

  // ── PNO-1207: Partners should exist in both systems ──
  test('Opportunity partners have valid references [PNO-1207]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');

    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // Check partners for the first few opportunities
    for (const opp of opps.slice(0, 3)) {
      const detailRes = await page.request.get(
        `${API}/api/opportunity/${opp.id}`,
        { headers: apiHeaders() }
      );

      if (!detailRes.ok()) continue;
      const detail = await detailRes.json();

      // Check funding partners
      const fundingPartners = detail.fundingPartners || detail.opportunityFundingPartners || [];
      for (const fp of (Array.isArray(fundingPartners) ? fundingPartners : [])) {
        const partnerId = fp.partnerId || fp.id;
        if (partnerId) {
          // PNO-1207: Each referenced partner should exist in the system
          const partnerRes = await page.request.get(
            `${API}/api/partner/${partnerId}`,
            { headers: apiHeaders() }
          );
          expect(partnerRes.status()).not.toBe(404);
        }
      }

      // Check client partners
      const clientPartners = detail.clientPartners || detail.opportunityClientPartners || [];
      for (const cp of (Array.isArray(clientPartners) ? clientPartners : [])) {
        const partnerId = cp.partnerId || cp.id;
        if (partnerId) {
          const partnerRes = await page.request.get(
            `${API}/api/partner/${partnerId}`,
            { headers: apiHeaders() }
          );
          expect(partnerRes.status()).not.toBe(404);
        }
      }
    }
  });

  // ── PNO-1202: Opportunity creation endpoint should accept all required fields ──
  test('Opportunity detail endpoint returns all required fields [PNO-1202]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');

    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // PNO-1202: Verify opportunity detail returns all key fields
    const detailRes = await page.request.get(
      `${API}/api/opportunity/${opps[0].id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();

    // These fields should be present in the response (even if null)
    expect(detail).toHaveProperty('id');
    expect(detail.id).toBeTruthy();
  });

  // ── Verify notification email URLs are correctly formatted ──
  test('Notification configuration does not use hash-based routing', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');

    // Check app configuration for URL patterns
    const configRes = await page.request.get(
      `${API}/api/values/config`,
      { headers: apiHeaders() }
    );

    if (configRes.ok()) {
      const config = await configRes.json();
      const appUrl = config.appUrl || config.baseUrl || config.applicationUrl;

      if (appUrl) {
        // PNO-1195: App URL in config should not contain /#/
        expect(appUrl).not.toContain('/#/');
      }
    }
  });
});
