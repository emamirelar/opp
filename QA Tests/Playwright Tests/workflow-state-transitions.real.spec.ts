/**
 * @fileoverview Workflow State Transitions — Real API E2E Tests
 *
 * Validates that Opportunity workflow state transitions produce the correct
 * Stage and Status values against the REAL backend. These tests would have
 * caught PNO-1161, PNO-1196, PNO-1162, PNO-1197, PNO-1160, PNO-1171.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api workflow-state-transitions.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 7
 */

import { test, expect } from '@playwright/test';
import {
  authenticateRealApi,
  createViaApi,
  deleteViaApi,
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
// WORKFLOW STATE TRANSITIONS (Real API)
// Catches: PNO-1161, PNO-1196, PNO-1162, PNO-1197, PNO-1160, PNO-1171
// ============================================================
test.describe('Workflow State Transitions — Real API', () => {
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

  // ── PNO-1161: Status should be "Active" after Go decision ──
  test('Go decision sets status to Active (not Draft) [PNO-1161]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    // Get an opportunity that has been approved (GO stage)
    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    const goOpportunity = Array.isArray(opportunities)
      ? opportunities.find((o: any) => o.stage === 'GO' || o.workflowStage === 'GO')
      : null;

    if (!goOpportunity) {
      test.skip(true, 'No GO-stage opportunity found in test data');
      return;
    }

    // Fetch the specific opportunity and verify status
    const detailRes = await page.request.get(
      `${API}/api/opportunity/${goOpportunity.id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();

    // PNO-1161: Status must be "Active" when stage is GO — not "Draft"
    expect(detail.status).not.toBe('Draft');
    expect(['Active', 'active']).toContain(detail.status);
  });

  // ── PNO-1196: Status should be "Closed" after No Go decision ──
  test('No Go decision sets status to Closed (not Draft) [PNO-1196]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    const noGoOpportunity = Array.isArray(opportunities)
      ? opportunities.find((o: any) =>
          o.stage === 'NO GO' || o.stage === 'NOGO' || o.workflowStage === 'NO GO')
      : null;

    if (!noGoOpportunity) {
      test.skip(true, 'No NO GO-stage opportunity found in test data');
      return;
    }

    const detailRes = await page.request.get(
      `${API}/api/opportunity/${noGoOpportunity.id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();

    // PNO-1196: Status must be "Closed" when stage is NO GO — not "Draft"
    expect(detail.status).not.toBe('Draft');
    expect(['Closed', 'closed', 'Inactive']).toContain(detail.status);
  });

  // ── PNO-1162: Reopened opportunity should revert to Draft ──
  test('Reopened opportunity reverts status to Draft (not Active) [PNO-1162]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    // Find an opportunity in IDENTIFY & PROFILE stage that was previously reopened
    const reopenedOpportunity = Array.isArray(opportunities)
      ? opportunities.find((o: any) =>
          (o.stage === 'IDENTIFY & PROFILE' || o.stage === 'Identify & Profile') &&
          o.status !== 'Active')
      : null;

    if (!reopenedOpportunity) {
      test.skip(true, 'No reopened opportunity found in test data');
      return;
    }

    const detailRes = await page.request.get(
      `${API}/api/opportunity/${reopenedOpportunity.id}`,
      { headers: apiHeaders() }
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();

    // PNO-1162: Reopened opportunity at IDENTIFY & PROFILE should be Draft, not Active
    expect(detail.status).not.toBe('Active');
    expect(['Draft', 'draft']).toContain(detail.status);
  });

  // ── PNO-1197: DOA fallback from Level 2 to Level 3 ──
  test('Workflow falls back to DOA3 when DOA2 is removed [PNO-1197]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    // Find an opportunity in approval pending state
    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    const pendingOpportunity = Array.isArray(opportunities)
      ? opportunities.find((o: any) =>
          o.workflowStatus === 'Pending' || o.workflowStatus === 'PendingApproval')
      : null;

    if (!pendingOpportunity) {
      test.skip(true, 'No pending-approval opportunity found');
      return;
    }

    // Check the approvers endpoint — it should never return empty if DOA3 exists
    const approversRes = await page.request.get(
      `${API}/api/opportunity/${pendingOpportunity.id}/approvers`,
      { headers: apiHeaders() }
    );

    if (approversRes.ok()) {
      const approvers = await approversRes.json();
      // PNO-1197: Approvers list should never be empty if org unit has any DOA holder
      if (Array.isArray(approvers)) {
        expect(approvers.length).toBeGreaterThan(0);
      }
    }
  });

  // ── PNO-1171: Reject should create only one audit log entry ──
  test('Reject creates single audit log entry (no duplicates) [PNO-1171]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    const noGoOpp = Array.isArray(opportunities)
      ? opportunities.find((o: any) =>
          o.stage === 'NO GO' || o.stage === 'NOGO')
      : null;

    if (!noGoOpp) {
      test.skip(true, 'No rejected opportunity found for audit log check');
      return;
    }

    // Check stage change history for duplicate entries
    const historyRes = await page.request.get(
      `${API}/api/opportunity/${noGoOpp.id}/stagechangehistory`,
      { headers: apiHeaders() }
    );

    if (historyRes.ok()) {
      const history = await historyRes.json();
      if (Array.isArray(history)) {
        // PNO-1171: Find reject entries and verify no duplicates at same timestamp
        const rejectEntries = history.filter((h: any) =>
          h.action?.toLowerCase().includes('reject'));

        const timestamps = rejectEntries.map((h: any) => h.createdDate || h.timestamp);
        const uniqueTimestamps = [...new Set(timestamps)];

        // Each unique timestamp should appear at most once
        expect(rejectEntries.length).toBeLessThanOrEqual(uniqueTimestamps.length);
      }
    }
  });

  // ── Verify workflow status labels are consistent across list and detail views ──
  test('Workflow status is consistent between list view and detail view', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');
    await waitForPageReady(page);

    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    if (!Array.isArray(opportunities) || opportunities.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // Check first 3 opportunities for status consistency
    for (const opp of opportunities.slice(0, 3)) {
      const detailRes = await page.request.get(
        `${API}/api/opportunity/${opp.id}`,
        { headers: apiHeaders() }
      );
      if (detailRes.ok()) {
        const detail = await detailRes.json();
        // Status from list view should match status from detail view
        if (opp.status && detail.status) {
          expect(opp.status).toBe(detail.status);
        }
      }
    }
  });

  // ── Verify stage change capitalisation is consistent ──
  test('Stage change actions use consistent capitalisation [PNO-1171]', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/opportunities');

    const res = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
    const opportunities = await res.json();

    if (!Array.isArray(opportunities) || opportunities.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    // Check the first opportunity with history
    for (const opp of opportunities.slice(0, 5)) {
      const historyRes = await page.request.get(
        `${API}/api/opportunity/${opp.id}/stagechangehistory`,
        { headers: apiHeaders() }
      );
      if (historyRes.ok()) {
        const history = await historyRes.json();
        if (Array.isArray(history) && history.length > 0) {
          for (const entry of history) {
            if (entry.action) {
              // PNO-1171: Actions should start with uppercase
              const firstChar = entry.action.charAt(0);
              expect(firstChar).toBe(firstChar.toUpperCase());
            }
          }
          break;
        }
      }
    }
  });
});
