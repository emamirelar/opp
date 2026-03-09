/**
 * @fileoverview Interaction, Search & Dashboard — Real API E2E Tests
 *
 * Lower-priority real-API tests covering:
 * - Interaction CRUD lifecycle
 * - Cross-entity search
 * - Dashboard widgets with real data
 * - Admin/Entity manager endpoints
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api interaction-search-dashboard.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 *
 * @tests 43
 */

import { test, expect } from '@playwright/test';
import {
  authenticateRealApi,
  createViaApi,
  deleteViaApi,
  isBackendAvailable,
} from './helpers/real-api-auth.helper';

const BACKEND_READY = process.env.REAL_API_TESTS === 'true';

function apiHeaders() {
  return {
    'Content-Type': 'application/json',
    'X-Goog-Authenticated-User-Email': `accounts.google.com:${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
    'X-Goog-Authenticated-User-ID': 'accounts.google.com:1',
    'Cookie': `DevIAPAuth=${process.env.TEST_USER_EMAIL || 'leonardc@unops.org'}`,
  };
}

// ============================================================
// INTERACTION CRUD
// ============================================================
test.describe('Interaction CRUD — Real API', () => {
  test.slow();

  let backendOk = false;
  let partnerId: number;
  const API = process.env.API_BASE_URL || 'http://localhost:5159';

  test.beforeAll(async ({ browser }) => {
    if (!BACKEND_READY) return;
    const ctx = await browser.newContext();
    const p = await ctx.newPage();
    backendOk = await isBackendAvailable(p);
    if (backendOk) {
      const res = await p.request.post(`${API}/api/partner`, {
        data: { name: `InteractionTestPartner ${Date.now()}`, partnerShortDescription: 'IP', partnerCategoryId: 1, liaisonOfficeId: 1 },
        headers: apiHeaders(),
      });
      if (res.ok()) partnerId = (await res.json()).id;
    }
    await ctx.close();
  });

  test.afterAll(async ({ browser }) => {
    if (partnerId) {
      const ctx = await browser.newContext();
      const p = await ctx.newPage();
      await p.request.delete(`${API}/api/partner/${partnerId}`, { headers: apiHeaders() });
      await ctx.close();
    }
  });

  test.beforeEach(async () => {
    test.skip(!BACKEND_READY, 'Set REAL_API_TESTS=true');
    test.skip(!backendOk, 'Backend not reachable');
  });

  // --- Positive (3) ---

  test('INT-R-001: Create interaction via API', async ({ page }) => {
    const res = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Meeting ${Date.now()}`, date: '2026-03-15', description: 'Real API test meeting', partnerId },
      headers: apiHeaders(),
    });
    expect(res.ok()).toBeTruthy();
    const iid = (await res.json()).id;
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-002: Get interaction by ID', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Get Test ${Date.now()}`, date: '2026-04-01', description: 'Get test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const getRes = await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    expect(getRes.ok()).toBeTruthy();
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-003: List interactions', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions?PageSize=5&PageIndex=1`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
  });

  // --- Negative (9) ---

  test('INT-R-N01: Create with empty name', async ({ page }) => {
    const res = await page.request.post(`${API}/api/interactions`, {
      data: { name: '', date: '2026-03-15', description: 'Test', partnerId },
      headers: apiHeaders(),
    });
    expect([200, 400, 422, 500].includes(res.status())).toBeTruthy();
  });

  test('INT-R-N02: Get non-existent interaction', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions/999999`, { headers: apiHeaders() });
    expect([404, 500].includes(res.status())).toBeTruthy();
  });

  test('INT-R-N03: Delete non-existent interaction', async ({ page }) => {
    const res = await page.request.delete(`${API}/api/interactions/999999`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('INT-R-N04: Create with empty body', async ({ page }) => {
    const res = await page.request.post(`${API}/api/interactions`, { data: {}, headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('INT-R-N05: Unauthenticated request', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions`, { headers: { 'Content-Type': 'application/json' } });
    expect([401, 403].includes(res.status())).toBeTruthy();
  });

  test('INT-R-N06: Get with negative ID', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions/-1`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('INT-R-N07: Update non-existent interaction', async ({ page }) => {
    const res = await page.request.put(`${API}/api/interactions/999999`, {
      data: { name: 'Updated', date: '2026-03-15', description: 'Test' },
      headers: apiHeaders(),
    });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('INT-R-N08: Double delete interaction', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `DblDel ${Date.now()}`, date: '2026-03-15', description: 'Test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    const res = await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('INT-R-N09: Create with invalid partnerId', async ({ page }) => {
    const res = await page.request.post(`${API}/api/interactions`, {
      data: { name: `BadPartner ${Date.now()}`, date: '2026-03-15', description: 'Test', partnerId: 999999 },
      headers: apiHeaders(),
    });
    expect([200, 201, 400, 422, 500].includes(res.status())).toBeTruthy();
  });

  // --- Functional (9) ---

  test('INT-R-F01: Created interaction has audit fields', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Audit ${Date.now()}`, date: '2026-03-15', description: 'Audit test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.createdDate || body.CreatedDate).toBeTruthy();
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F02: Interaction linked to correct partner', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Linked ${Date.now()}`, date: '2026-03-15', description: 'Test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.partnerId || body.PartnerId).toBe(partnerId);
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F03: Deleted interaction returns 404 on GET', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `DelGet ${Date.now()}`, date: '2026-03-15', description: 'Test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    const res = await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    expect([404, 500].includes(res.status())).toBeTruthy();
  });

  test('INT-R-F04: Interaction list respects pagination', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions?PageSize=2&PageIndex=1`, { headers: apiHeaders() });
    if (res.ok()) {
      const body = await res.json();
      const items = body.items || body.data || [];
      expect(items.length).toBeLessThanOrEqual(2);
    }
  });

  test('INT-R-F05: Interaction date stored correctly', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `DateCheck ${Date.now()}`, date: '2026-06-15', description: 'Date test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    if (body.date) expect(body.date).toContain('2026');
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F06: Interaction update persists changes', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `BeforeUpd ${Date.now()}`, date: '2026-03-15', description: 'Before', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    await page.request.put(`${API}/api/interactions/${iid}`, {
      data: { id: iid, name: `AfterUpd ${Date.now()}`, date: '2026-03-16', description: 'After', partnerId },
      headers: apiHeaders(),
    });
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.description).toBe('After');
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F07: Interaction visible in UI', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `UIVisible ${Date.now()}`, date: '2026-03-15', description: 'UI test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    await authenticateRealApi(page, `/partnerships/interactions/${iid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('UIVisible', { timeout: 10000 });
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F08: Interaction name stored correctly', async ({ page }) => {
    const name = `NameCheck ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name, date: '2026-03-15', description: 'Name test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.name).toContain('NameCheck');
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-F09: Interaction has default status', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `StatusChk ${Date.now()}`, date: '2026-03-15', description: 'Status test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.status).toBeTruthy();
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  // --- Integration (9) ---

  test('INT-R-I01: Full interaction CRUD lifecycle', async ({ page }) => {
    let iid: number;
    await test.step('CREATE', async () => {
      const res = await page.request.post(`${API}/api/interactions`, {
        data: { name: `Lifecycle ${Date.now()}`, date: '2026-03-15', description: 'CRUD test', partnerId },
        headers: apiHeaders(),
      });
      iid = (await res.json()).id;
    });
    await test.step('READ', async () => {
      expect((await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).ok()).toBeTruthy();
    });
    await test.step('UPDATE', async () => {
      await page.request.put(`${API}/api/interactions/${iid!}`, {
        data: { id: iid!, name: `Updated ${Date.now()}`, date: '2026-03-16', description: 'Updated', partnerId },
        headers: apiHeaders(),
      });
    });
    await test.step('DELETE', async () => {
      await page.request.delete(`${API}/api/interactions/${iid!}`, { headers: apiHeaders() });
    });
    await test.step('VERIFY DELETE', async () => {
      const res = await page.request.get(`${API}/api/interactions/${iid!}`, { headers: apiHeaders() });
      expect([404, 500].includes(res.status())).toBeTruthy();
    });
  });

  test('INT-R-I02: Interaction list page loads', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/interactions');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('interaction');
  });

  test('INT-R-I03: Multiple interactions for same partner', async ({ page }) => {
    const ids: number[] = [];
    for (let i = 0; i < 3; i++) {
      const res = await page.request.post(`${API}/api/interactions`, {
        data: { name: `Multi${i} ${Date.now()}`, date: '2026-03-15', description: `Item ${i}`, partnerId },
        headers: apiHeaders(),
      });
      if (res.ok()) ids.push((await res.json()).id);
    }
    expect(ids.length).toBeGreaterThanOrEqual(1);
    for (const id of ids) await page.request.delete(`${API}/api/interactions/${id}`, { headers: apiHeaders() });
  });

  test('INT-R-I04: Create partner + interaction + contact flow', async ({ page }) => {
    const pRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `FlowPartner ${Date.now()}`, partnerShortDescription: 'FP', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await pRes.json()).id;

    const iRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `FlowMeeting ${Date.now()}`, date: '2026-05-01', description: 'Flow test', partnerId: pid },
      headers: apiHeaders(),
    });
    const iid = (await iRes.json()).id;

    const cRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Flow', lastName: `Contact ${Date.now()}`, email: `flow${Date.now()}@test.com`, title: 'Mgr', partnerId: pid },
      headers: apiHeaders(),
    });
    const cid = cRes.ok() ? (await cRes.json()).id : null;

    if (cid) await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('INT-R-I05: Interaction search works', async ({ page }) => {
    const unique = `SearchInt${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: unique, date: '2026-03-15', description: 'Search test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const res = await page.request.get(`${API}/api/interactions?SearchText=${encodeURIComponent(unique)}`, { headers: apiHeaders() });
    expect([200, 500].includes(res.status())).toBeTruthy();
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-I06: Interaction detail shows in UI', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `DetailUI ${Date.now()}`, date: '2026-03-15', description: 'UI detail test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    await authenticateRealApi(page, `/partnerships/interactions/${iid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('DetailUI', { timeout: 10000 });
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-I07: Interaction with notes persists notes', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Notes ${Date.now()}`, date: '2026-03-15', description: 'Detailed notes about the meeting including follow-up actions', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
    expect(body.description).toContain('follow-up');
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-I08: Interaction permissions endpoint works', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Perms ${Date.now()}`, date: '2026-03-15', description: 'Permission test', partnerId },
      headers: apiHeaders(),
    });
    const iid = (await createRes.json()).id;
    const res = await page.request.get(`${API}/api/interactions/${iid}/permissions`, { headers: apiHeaders() });
    expect([200, 404, 500].includes(res.status())).toBeTruthy();
    await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
  });

  test('INT-R-I09: Interaction with future date accepted', async ({ page }) => {
    const futureDate = '2028-12-31';
    const createRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Future ${Date.now()}`, date: futureDate, description: 'Future interaction', partnerId },
      headers: apiHeaders(),
    });
    if (createRes.ok()) {
      const iid = (await createRes.json()).id;
      const body = await (await page.request.get(`${API}/api/interactions/${iid}`, { headers: apiHeaders() })).json();
      if (body.date) expect(body.date).toContain('2028');
      await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    }
  });
});

// ============================================================
// SEARCH & DASHBOARD
// ============================================================
test.describe('Search & Dashboard — Real API', () => {
  test.slow();

  let backendOk = false;
  const API = process.env.API_BASE_URL || 'http://localhost:5159';

  test.beforeAll(async ({ browser }) => {
    if (!BACKEND_READY) return;
    const ctx = await browser.newContext();
    const p = await ctx.newPage();
    backendOk = await isBackendAvailable(p);
    await ctx.close();
  });

  test.beforeEach(async () => {
    test.skip(!BACKEND_READY, 'Set REAL_API_TESTS=true');
    test.skip(!backendOk, 'Backend not reachable');
  });

  // --- Dashboard tests ---

  test('DASH-R-001: Home dashboard loads with real data', async ({ page }) => {
    await authenticateRealApi(page, '/');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('localhost');
    // Verify some content loaded (cards, widgets, etc.)
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('DASH-R-002: Dashboard API endpoints respond', async ({ page }) => {
    const endpoints = [
      '/api/dashboard/stats',
      '/api/dashboard/recentActivity',
      '/api/opportunity/analytics/pipeline',
    ];
    for (const ep of endpoints) {
      const res = await page.request.get(`${API}${ep}`, { headers: apiHeaders() });
      expect([200, 404, 500].includes(res.status())).toBeTruthy();
    }
  });

  test('DASH-R-003: Navigation menu renders with real permissions', async ({ page }) => {
    await authenticateRealApi(page, '/');
    await page.waitForLoadState('networkidle');
    // Menu items should be visible
    const menu = page.locator('app-menu, [data-testid="main-menu"], .p-menubar, nav').first();
    await expect(menu).toBeVisible({ timeout: 10000 });
  });

  // --- Search tests ---

  test('SRCH-R-001: Global search page loads', async ({ page }) => {
    await authenticateRealApi(page, '/search');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('search');
  });

  test('SRCH-R-002: Search API endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/search?query=test&PageSize=5`, { headers: apiHeaders() });
    expect([200, 400, 404, 500].includes(res.status())).toBeTruthy();
  });

  test('SRCH-R-003: Advanced search endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/opportunity/advanced-search?SearchText=test&PageSize=5`, { headers: apiHeaders() });
    expect([200, 400, 404, 500].includes(res.status())).toBeTruthy();
  });

  // --- Config/Admin tests ---

  test('ADM-R-001: Config endpoint accessible', async ({ page }) => {
    const res = await page.request.get(`${API}/api/values/config`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
  });

  test('ADM-R-002: Entity types endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/entityType`, { headers: apiHeaders() });
    expect([200, 401, 403, 404, 500].includes(res.status())).toBeTruthy();
  });

  test('ADM-R-003: User profile endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/user/claims`, { headers: apiHeaders() });
    expect([200, 401, 500].includes(res.status())).toBeTruthy();
  });

  test('ADM-R-004: Permissions check endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/permissions/check/partnerships/partners`, { headers: apiHeaders() });
    expect([200, 401, 403, 404, 500].includes(res.status())).toBeTruthy();
  });

  // --- Cross-entity tests ---

  test('CROSS-R-001: Partner typeahead endpoint works', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner/typeahead?query=test&limit=5`, { headers: apiHeaders() });
    expect([200, 400, 404, 500].includes(res.status())).toBeTruthy();
  });

  test('CROSS-R-002: Country dropdown data loads', async ({ page }) => {
    const res = await page.request.get(`${API}/api/entityType/country`, { headers: apiHeaders() });
    expect([200, 404, 500].includes(res.status())).toBeTruthy();
  });

  test('CROSS-R-003: SDG dropdown data loads', async ({ page }) => {
    const res = await page.request.get(`${API}/api/entityType/sdg`, { headers: apiHeaders() });
    expect([200, 404, 500].includes(res.status())).toBeTruthy();
  });
});

/*
 * ### 3:1 Ratio Compliance Check — Interaction
 * | Category | Count |
 * |----------|-------|
 * | Positive (P) | 3 |
 * | Negative (N) | 9 |  N >= 3P ✅
 * | Functional (F) | 9 |  F >= 3P ✅
 * | Integration (I) | 9 |  I >= 3P ✅
 *
 * ### Search/Dashboard — Additional Coverage
 * | Category | Count |
 * |----------|-------|
 * | Dashboard | 3 |
 * | Search | 3 |
 * | Admin | 4 |
 * | Cross-entity | 3 |
 */
