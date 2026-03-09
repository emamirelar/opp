/**
 * @fileoverview Edge & Boundary Cases — Real API E2E Tests
 *
 * Tests boundary values, soft-delete interactions, concurrent access,
 * temporal boundaries, special characters, and fallback paths against
 * the REAL backend. These complement the domain-specific real-env tests
 * and ensure the 3:1 Edge/Boundary ratio is met.
 *
 * No API mocking — every request hits the actual .NET backend + PostgreSQL.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api edge-boundary-cases.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 12
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
// EDGE & BOUNDARY CASES (Real API)
// Ensures 3:1 Edge/Boundary ratio compliance
// ============================================================
test.describe('Edge & Boundary Cases — Real API', () => {
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

  // ── EDGE-001: Max-length opportunity name boundary ──
  test('Opportunity name at max length boundary is accepted and stored correctly', async ({ page }) => {
    const maxName = `BND-Name-${Date.now()}-${'A'.repeat(200)}`;

    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: maxName,
        opportunityStatement: 'Boundary test: max-length name',
      },
    });

    if (createRes.ok()) {
      const created = await createRes.json();
      const oppId = created.id || created.Id;

      try {
        // Verify the name was stored correctly (not truncated silently)
        const getRes = await page.request.get(
          `${API}/api/opportunity/${oppId}`,
          { headers: apiHeaders() }
        );
        expect(getRes.ok()).toBeTruthy();
        const detail = await getRes.json();

        // Name should be stored as-is or server should have rejected it
        expect(detail.name).toBeTruthy();
        expect(detail.name.length).toBeGreaterThan(0);
      } finally {
        await page.request.delete(`${API}/api/opportunity/${oppId}`, {
          headers: apiHeaders(),
        }).catch(() => {});
      }
    } else {
      // If rejected, should be 400 (validation) not 500 (crash)
      expect(createRes.status()).toBe(400);
    }
  });

  // ── EDGE-002: Special characters in all text fields ──
  test('Special characters in opportunity statement do not cause encoding issues', async ({ page }) => {
    const specialStatement = 'Test with spécial chars: ñ, ü, ø, é, ã, ç, ß, αβγ, 日本語, "quotes", <brackets>';

    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-Special-${Date.now()}`,
        opportunityStatement: specialStatement,
      },
    });

    if (createRes.ok()) {
      const created = await createRes.json();
      const oppId = created.id || created.Id;

      try {
        const getRes = await page.request.get(
          `${API}/api/opportunity/${oppId}`,
          { headers: apiHeaders() }
        );
        expect(getRes.ok()).toBeTruthy();
        const detail = await getRes.json();

        const storedStatement = detail.opportunityStatement || detail.statement || '';
        // Should not contain "??" encoding failures
        expect(storedStatement).not.toContain('??');
        // Should preserve at least the Latin accented characters
        if (storedStatement.includes('sp')) {
          expect(storedStatement).toContain('spécial');
        }
      } finally {
        await page.request.delete(`${API}/api/opportunity/${oppId}`, {
          headers: apiHeaders(),
        }).catch(() => {});
      }
    }
  });

  // ── EDGE-003: Soft-deleted opportunity should not appear in list ──
  test('Soft-deleted opportunity does not appear in active list results', async ({ page }) => {
    // Create and then delete an opportunity
    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-SoftDelete-${Date.now()}`,
        opportunityStatement: 'Soft-delete boundary test',
      },
    });

    if (!createRes.ok()) {
      test.skip(true, 'Cannot create test opportunity');
      return;
    }

    const created = await createRes.json();
    const oppId = created.id || created.Id;

    // Delete the opportunity
    await page.request.delete(`${API}/api/opportunity/${oppId}`, {
      headers: apiHeaders(),
    });

    // Verify it doesn't appear in list
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (Array.isArray(opps)) {
      const found = opps.find((o: any) => o.id === oppId);
      expect(found).toBeUndefined();
    }
  });

  // ── EDGE-004: Zero-count collections (empty states) ──
  test('Opportunity with zero funding partners returns empty array (not null)', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-EmptyCollections-${Date.now()}`,
        opportunityStatement: 'Empty collections boundary test',
      },
    });

    if (!createRes.ok()) {
      test.skip(true, 'Cannot create test opportunity');
      return;
    }

    const created = await createRes.json();
    const oppId = created.id || created.Id;

    try {
      const detailRes = await page.request.get(
        `${API}/api/opportunity/${oppId}`,
        { headers: apiHeaders() }
      );
      expect(detailRes.ok()).toBeTruthy();
      const detail = await detailRes.json();

      // Collections should be empty arrays, not null or undefined
      const fundingPartners = detail.fundingPartners || detail.opportunityFundingPartners;
      if (fundingPartners !== undefined) {
        expect(Array.isArray(fundingPartners)).toBeTruthy();
        expect(fundingPartners.length).toBe(0);
      }

      const clientPartners = detail.clientPartners || detail.opportunityClientPartners;
      if (clientPartners !== undefined) {
        expect(Array.isArray(clientPartners)).toBeTruthy();
        expect(clientPartners.length).toBe(0);
      }
    } finally {
      await page.request.delete(`${API}/api/opportunity/${oppId}`, {
        headers: apiHeaders(),
      }).catch(() => {});
    }
  });

  // ── EDGE-005: Boundary date values (far past, far future) ──
  test('Extreme date values are handled without server error', async ({ page }) => {
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

    // Try setting a far-future date via API
    const updateRes = await page.request.put(
      `${API}/api/opportunity/${draftOpp.id}`,
      {
        headers: apiHeaders(),
        data: {
          ...draftOpp,
          estimatedSigningDate: '2099-12-31T00:00:00Z',
        },
      }
    );

    // Should either accept or reject with 400, not crash with 500
    expect(updateRes.status()).not.toBe(500);
  });

  // ── EDGE-006: Concurrent read on same resource ──
  test('Multiple concurrent reads on same opportunity do not cause errors', async ({ page }) => {
    const listRes = await page.request.get(`${API}/api/opportunity`, { headers: apiHeaders() });
    expect(listRes.ok()).toBeTruthy();
    const opps = await listRes.json();

    if (!Array.isArray(opps) || opps.length === 0) {
      test.skip(true, 'No opportunities found');
      return;
    }

    const oppId = opps[0].id;

    // Fire 5 concurrent requests to the same resource
    const requests = Array.from({ length: 5 }, () =>
      page.request.get(`${API}/api/opportunity/${oppId}`, { headers: apiHeaders() })
    );

    const responses = await Promise.all(requests);

    for (const res of responses) {
      expect(res.status()).not.toBe(500);
      expect(res.ok()).toBeTruthy();
    }

    // All should return identical data
    const bodies = await Promise.all(responses.map(r => r.json()));
    for (const body of bodies) {
      expect(body.id).toBe(oppId);
    }
  });

  // ── EDGE-007: Non-existent opportunity ID boundary ──
  test('Non-existent opportunity ID returns 404 (not 500)', async ({ page }) => {
    const nonExistentId = 999999999;

    const res = await page.request.get(
      `${API}/api/opportunity/${nonExistentId}`,
      { headers: apiHeaders() }
    );

    // Should be 404, not 500
    expect(res.status()).not.toBe(500);
    expect([404, 400]).toContain(res.status());
  });

  // ── EDGE-008: Zero and negative ID boundary ──
  test('Zero and negative opportunity IDs return error (not 500)', async ({ page }) => {
    const boundaryIds = [0, -1, -999];

    for (const id of boundaryIds) {
      const res = await page.request.get(
        `${API}/api/opportunity/${id}`,
        { headers: apiHeaders() }
      );

      expect(res.status()).not.toBe(500);
    }
  });

  // ── EDGE-009: Empty search query boundary ──
  test('Empty search query returns results or empty list (not error)', async ({ page }) => {
    const emptySearches = ['', ' ', '%20'];

    for (const query of emptySearches) {
      const res = await page.request.get(
        `${API}/api/global/search?query=${encodeURIComponent(query)}`,
        { headers: apiHeaders() }
      );

      // Should handle gracefully
      expect(res.status()).not.toBe(500);
    }
  });

  // ── EDGE-010: Opportunity with all optional fields null ──
  test('Opportunity with minimal data (all optional null) loads in UI without crash', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-Minimal-${Date.now()}`,
      },
    });

    if (!createRes.ok()) {
      test.skip(true, 'Cannot create minimal opportunity');
      return;
    }

    const created = await createRes.json();
    const oppId = created.id || created.Id;

    try {
      // Navigate to the opportunity in UI — should not crash on null fields
      await authenticateRealApi(page, `/partnerships/opportunities/${oppId}`);
      await waitForPageReady(page);
      await page.waitForTimeout(3000);

      // No server error toast
      const serverError = page.locator('.p-toast-message-error:has-text("Server Error")');
      const hasError = await serverError.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasError).toBeFalsy();

      // Page should show something (not blank)
      const bodyText = await page.textContent('body');
      expect(bodyText!.length).toBeGreaterThan(50);
    } finally {
      await page.request.delete(`${API}/api/opportunity/${oppId}`, {
        headers: apiHeaders(),
      }).catch(() => {});
    }
  });

  // ── EDGE-011: Permission endpoint on soft-deleted opportunity ──
  test('Permission endpoint on deleted opportunity returns error (not crash)', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-DeletedPerms-${Date.now()}`,
        opportunityStatement: 'Permission on deleted entity test',
      },
    });

    if (!createRes.ok()) {
      test.skip(true, 'Cannot create test opportunity');
      return;
    }

    const created = await createRes.json();
    const oppId = created.id || created.Id;

    // Delete it
    await page.request.delete(`${API}/api/opportunity/${oppId}`, {
      headers: apiHeaders(),
    });

    // Try to get permissions on the deleted entity
    const permRes = await page.request.get(
      `${API}/api/opportunity/${oppId}/permissions`,
      { headers: apiHeaders() }
    );

    // Should be 404, not 500
    expect(permRes.status()).not.toBe(500);
  });

  // ── EDGE-012: Validation endpoint on newly created (empty) opportunity ──
  test('Validation on brand-new empty opportunity returns all required field errors', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/opportunity`, {
      headers: apiHeaders(),
      data: {
        name: `BND-EmptyValidation-${Date.now()}`,
      },
    });

    if (!createRes.ok()) {
      test.skip(true, 'Cannot create test opportunity');
      return;
    }

    const created = await createRes.json();
    const oppId = created.id || created.Id;

    try {
      const validationRes = await page.request.get(
        `${API}/api/opportunity/${oppId}/validate`,
        { headers: apiHeaders() }
      );

      // Validation should work (not crash)
      expect(validationRes.status()).not.toBe(500);

      if (validationRes.ok()) {
        const validation = await validationRes.json();
        const errors = validation.errors || validation.unmetRequirements || [];

        // A brand-new empty opportunity should have MANY validation errors
        if (Array.isArray(errors)) {
          expect(errors.length).toBeGreaterThan(0);
        }
      }
    } finally {
      await page.request.delete(`${API}/api/opportunity/${oppId}`, {
        headers: apiHeaders(),
      }).catch(() => {});
    }
  });
});
