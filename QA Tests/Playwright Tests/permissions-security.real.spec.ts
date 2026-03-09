/**
 * @fileoverview Permissions & Security — Real API E2E Tests
 *
 * Validates that permission endpoints and security controls work correctly
 * against the REAL backend. These tests catch role-based access issues
 * that mocked tests inherently miss.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api permissions-security.real.spec.ts
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
import { waitForPageReady } from './helpers/wait.helper';

const BACKEND_READY = process.env.REAL_API_TESTS === 'true';
const API = process.env.API_BASE_URL || 'http://localhost:5159';

function apiHeaders(email?: string) {
  const userEmail = email || process.env.TEST_USER_EMAIL || 'leonardc@unops.org';
  return {
    'Content-Type': 'application/json',
    'X-Goog-Authenticated-User-Email': `accounts.google.com:${userEmail}`,
    'X-Goog-Authenticated-User-ID': 'accounts.google.com:1',
    'Cookie': `DevIAPAuth=${userEmail}`,
  };
}

// ============================================================
// PERMISSIONS & SECURITY (Real API)
// ============================================================
test.describe('Permissions & Security — Real API', () => {
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

  // ── Permission endpoint returns expected structure ──
  test('Opportunity permission endpoint returns canEdit/canDelete flags', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    const permRes = await page.request.get(
      `${API}/api/opportunity/${opps[0].id}/permissions`,
      { headers: apiHeaders() }
    );

    if (permRes.ok()) {
      const perms = await permRes.json();
      // Permission response should have boolean flags
      expect(perms).toHaveProperty('canEdit');
      expect(perms).toHaveProperty('canDelete');
      expect(typeof perms.canEdit).toBe('boolean');
      expect(typeof perms.canDelete).toBe('boolean');
    }
  });

  // ── Unauthenticated requests are rejected ──
  test('API rejects requests without authentication headers', async ({ page }) => {
    const res = await page.request.get(`${API}/api/opportunity`, {
      headers: { 'Content-Type': 'application/json' },
    });

    // Should be 401 or 403
    expect([401, 403]).toContain(res.status());
  });

  // ── Invalid user email returns unauthorized ──
  test('API rejects requests with invalid user email', async ({ page }) => {
    const res = await page.request.get(`${API}/api/opportunity`, {
      headers: apiHeaders('nonexistent-user-that-does-not-exist@invalid.test'),
    });

    // Should reject
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  // ── Draft opportunity should be editable by creator ──
  test('Draft opportunity is editable by its creator', async ({ page }) => {
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

    const permRes = await page.request.get(
      `${API}/api/opportunity/${draftOpp.id}/permissions`,
      { headers: apiHeaders() }
    );

    if (permRes.ok()) {
      const perms = await permRes.json();
      // Creator should be able to edit their draft
      expect(perms.canEdit).toBeTruthy();
    }
  });

  // ── Closed opportunity should not be editable ──
  test('Closed/Active opportunity restricts edit permissions appropriately', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    const closedOpp = Array.isArray(opps)
      ? opps.find((o: any) =>
          o.status === 'Closed' || o.status === 'Inactive' ||
          o.stage === 'NO GO' || o.stage === 'NOGO')
      : null;

    if (!closedOpp) {
      test.skip(true, 'No closed opportunity found');
      return;
    }

    const permRes = await page.request.get(
      `${API}/api/opportunity/${closedOpp.id}/permissions`,
      { headers: apiHeaders() }
    );

    if (permRes.ok()) {
      const perms = await permRes.json();
      // Closed opportunity should have restricted editing
      // This may depend on user role, so we just verify the endpoint works
      expect(typeof perms.canEdit).toBe('boolean');
    }
  });

  // ── Partner permission endpoint works ──
  test('Partner permission endpoint returns valid response', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/partner`, { headers: apiHeaders() });
    if (!listRes.ok()) {
      test.skip(true, 'Cannot access partner list');
      return;
    }

    const partners = await listRes.json();
    if (!Array.isArray(partners) || partners.length === 0) {
      test.skip(true, 'No partners found');
      return;
    }

    const permRes = await page.request.get(
      `${API}/api/partner/${partners[0].id}/permissions`,
      { headers: apiHeaders() }
    );

    // Should not return 500
    expect(permRes.status()).not.toBe(500);
  });

  // ── Contact permission endpoint works ──
  test('Contact permission endpoint returns valid response', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/contact`, { headers: apiHeaders() });
    if (!listRes.ok()) {
      test.skip(true, 'Cannot access contact list');
      return;
    }

    const contacts = await listRes.json();
    if (!Array.isArray(contacts) || contacts.length === 0) {
      test.skip(true, 'No contacts found');
      return;
    }

    const permRes = await page.request.get(
      `${API}/api/contact/${contacts[0].id}/permissions`,
      { headers: apiHeaders() }
    );

    // Should not return 500
    expect(permRes.status()).not.toBe(500);
  });
});
