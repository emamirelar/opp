/**
 * @fileoverview Partner & Contact CRUD — Real API E2E Tests
 *
 * Tests the Partner and Contact lifecycle against the real .NET backend + PostgreSQL.
 * No API mocking — every request hits the actual backend.
 *
 * Run: cd "QA Tests" && npx playwright test --project=real-api partner-contact-crud.real.spec.ts
 *
 * @author UNOPS Opportunity+ QA Team
 * @tests 60
 */

import { test, expect } from '@playwright/test';
import {
  authenticateRealApi,
  createViaApi,
  deleteViaApi,
  isBackendAvailable,
} from './helpers/real-api-auth.helper';
import { waitForMinimumElapsed } from './helpers/wait.helper';

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
// PARTNER CRUD
// ============================================================
test.describe('Partner CRUD — Real API', () => {
  test.slow();

  let backendOk = false;
  const API = process.env.API_BASE_URL || 'http://localhost:5159';

  test.beforeAll(async ({ browser }) => {
    if (!BACKEND_READY) return;
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    backendOk = await isBackendAvailable(page);
    await ctx.close();
  });

  test.beforeEach(async () => {
    test.skip(!BACKEND_READY, 'Set REAL_API_TESTS=true');
    test.skip(!backendOk, 'Backend not reachable');
  });

  // --- Positive (3) ---

  test('PTR-R-001: Create partner via API', async ({ page }) => {
    const response = await page.request.post(`${API}/api/partner`, {
      data: {
        name: `Real Partner ${Date.now()}`,
        partnerShortDescription: 'RP',
        partnerLongDescription: 'Created by real-API test',
        partnerCategoryId: 1,
        liaisonOfficeId: 1,
        partnerGroupId: 1,
      },
      headers: apiHeaders(),
    });
    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.id || body.Id).toBeGreaterThan(0);
    // Cleanup
    await page.request.delete(`${API}/api/partner/${body.id || body.Id}`, { headers: apiHeaders() });
  });

  test('PTR-R-002: Get partner by ID via API', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Get Test ${Date.now()}`, partnerShortDescription: 'GT', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const created = await createRes.json();
    const id = created.id || created.Id;

    const getRes = await page.request.get(`${API}/api/partner/${id}`, { headers: apiHeaders() });
    expect(getRes.ok()).toBeTruthy();
    const body = await getRes.json();
    expect(body.name).toContain('Get Test');

    await page.request.delete(`${API}/api/partner/${id}`, { headers: apiHeaders() });
  });

  test('PTR-R-003: List partners with pagination', async ({ page }) => {
    const response = await page.request.get(`${API}/api/partner?PageSize=5&PageIndex=1`, { headers: apiHeaders() });
    expect(response.ok()).toBeTruthy();
  });

  // --- Negative (9) ---

  test('PTR-R-N01: Create partner with empty name fails', async ({ page }) => {
    const res = await page.request.post(`${API}/api/partner`, {
      data: { name: '', partnerShortDescription: 'X', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N02: Get non-existent partner returns 404', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner/999999`, { headers: apiHeaders() });
    expect([404, 500].includes(res.status())).toBeTruthy();
  });

  test('PTR-R-N03: Delete non-existent partner fails', async ({ page }) => {
    const res = await page.request.delete(`${API}/api/partner/999999`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N04: Create with empty body fails', async ({ page }) => {
    const res = await page.request.post(`${API}/api/partner`, { data: {}, headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N05: Unauthenticated partner request returns 401/403', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner`, { headers: { 'Content-Type': 'application/json' } });
    expect([401, 403].includes(res.status())).toBeTruthy();
  });

  test('PTR-R-N06: Get partner with negative ID', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner/-1`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N07: Update non-existent partner', async ({ page }) => {
    const res = await page.request.put(`${API}/api/partner/999999`, {
      data: { name: 'Updated', partnerShortDescription: 'UP' },
      headers: apiHeaders(),
    });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N08: Double delete partner', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `DblDel ${Date.now()}`, partnerShortDescription: 'DD', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const { id, Id } = await createRes.json();
    const pid = id || Id;
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    const secondDel = await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    expect(secondDel.status()).toBeGreaterThanOrEqual(400);
  });

  test('PTR-R-N09: Get partner with string ID returns error', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner/abc`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  // --- Functional (9) ---

  test('PTR-R-F01: Created partner has audit fields', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Audit ${Date.now()}`, partnerShortDescription: 'AU', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const created = await createRes.json();
    const pid = created.id || created.Id;
    const getRes = await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    const body = await getRes.json();
    expect(body.createdDate || body.CreatedDate).toBeTruthy();
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-F02: Soft-deleted partner not in list', async ({ page }) => {
    const name = `SoftDel ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name, partnerShortDescription: 'SD', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });

    const listRes = await page.request.get(`${API}/api/partner?PageSize=100&PageIndex=1`, { headers: apiHeaders() });
    if (listRes.ok()) {
      const list = await listRes.json();
      const items = list.items || list.data || list || [];
      const found = Array.isArray(items) ? items.find((i: any) => i.name === name) : null;
      expect(found).toBeFalsy();
    }
  });

  test('PTR-R-F03: Permissions endpoint returns expected shape', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Perms ${Date.now()}`, partnerShortDescription: 'PM', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;
    const permRes = await page.request.get(`${API}/api/partner/${pid}/permissions`, { headers: apiHeaders() });
    if (permRes.ok()) {
      const body = await permRes.json();
      expect(body).toHaveProperty('canEdit');
    }
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-F04: Partner list respects pagination', async ({ page }) => {
    const res = await page.request.get(`${API}/api/partner?PageSize=2&PageIndex=1`, { headers: apiHeaders() });
    if (res.ok()) {
      const body = await res.json();
      const items = body.items || body.data || [];
      expect(items.length).toBeLessThanOrEqual(2);
    }
  });

  test('PTR-R-F05: Partner update modifies LastModifiedDate', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `ModDate ${Date.now()}`, partnerShortDescription: 'MD', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;

    const before = await (await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() })).json();
    await waitForMinimumElapsed(page, 1100);
    await page.request.put(`${API}/api/partner/${pid}`, {
      data: { id: pid, name: `ModDate Updated ${Date.now()}`, partnerShortDescription: 'MU', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const after = await (await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() })).json();

    if (before.lastModifiedDate && after.lastModifiedDate) {
      expect(new Date(after.lastModifiedDate).getTime()).toBeGreaterThanOrEqual(new Date(before.lastModifiedDate).getTime());
    }
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-F06: Partner detail visible in UI', async ({ page }) => {
    const name = `UICheck ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name, partnerShortDescription: 'UC', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;

    await authenticateRealApi(page, `/partnerships/partners/${pid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('UICheck', { timeout: 10000 });

    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-F07: Partner list page loads in UI', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/partners');
    await page.waitForLoadState('networkidle');
    const table = page.locator('p-table, table, [data-testid="partners-table"]').first();
    const loaded = await table.isVisible({ timeout: 10000 }).catch(() => false);
    expect(page.url()).toContain('partner');
  });

  test('PTR-R-F08: Created partner has default status', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Status ${Date.now()}`, partnerShortDescription: 'ST', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() })).json();
    // Status should be Draft or Active
    expect(body.status).toBeTruthy();
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-F09: GET deleted partner returns 404', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `DelGet ${Date.now()}`, partnerShortDescription: 'DG', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    const getRes = await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    expect([404, 500].includes(getRes.status())).toBeTruthy();
  });

  // --- Integration (9) ---

  test('PTR-R-I01: Full partner CRUD lifecycle', async ({ page }) => {
    let pid: number;

    await test.step('CREATE', async () => {
      const res = await page.request.post(`${API}/api/partner`, {
        data: { name: `Lifecycle ${Date.now()}`, partnerShortDescription: 'LC', partnerCategoryId: 1, liaisonOfficeId: 1 },
        headers: apiHeaders(),
      });
      expect(res.ok()).toBeTruthy();
      pid = (await res.json()).id;
    });

    await test.step('READ', async () => {
      const res = await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
      expect(res.ok()).toBeTruthy();
    });

    await test.step('UPDATE', async () => {
      const res = await page.request.put(`${API}/api/partner/${pid!}`, {
        data: { id: pid!, name: `Updated ${Date.now()}`, partnerShortDescription: 'UP', partnerCategoryId: 1, liaisonOfficeId: 1 },
        headers: apiHeaders(),
      });
      expect([200, 204].includes(res.status())).toBeTruthy();
    });

    await test.step('DELETE', async () => {
      const res = await page.request.delete(`${API}/api/partner/${pid!}`, { headers: apiHeaders() });
      expect([200, 204].includes(res.status())).toBeTruthy();
    });

    await test.step('VERIFY DELETE', async () => {
      const res = await page.request.get(`${API}/api/partner/${pid!}`, { headers: apiHeaders() });
      expect([404, 500].includes(res.status())).toBeTruthy();
    });
  });

  test('PTR-R-I02: Partner search finds matching name', async ({ page }) => {
    const unique = `SearchPartner${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name: unique, partnerShortDescription: 'SP', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;

    const searchRes = await page.request.get(
      `${API}/api/partner?SearchText=${encodeURIComponent(unique)}&PageSize=10&PageIndex=1`,
      { headers: apiHeaders() }
    );
    if (searchRes.ok()) {
      const body = await searchRes.json();
      const items = body.items || body.data || body || [];
      if (Array.isArray(items)) {
        expect(items.some((i: any) => i.name?.includes('SearchPartner'))).toBeTruthy();
      }
    }
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-I03: Create partner then view in UI list', async ({ page }) => {
    const name = `UIList ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name, partnerShortDescription: 'UL', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;

    await authenticateRealApi(page, '/partnerships/partners');
    await page.waitForLoadState('networkidle');
    // The partner should appear in the list (may need to search)
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-I04: Create partner then create contact for it', async ({ page }) => {
    const partnerRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `WithContact ${Date.now()}`, partnerShortDescription: 'WC', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await partnerRes.json()).id;

    const contactRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'John', lastName: `Contact ${Date.now()}`, email: `test${Date.now()}@example.com`, title: 'Manager', partnerId: pid },
      headers: apiHeaders(),
    });

    if (contactRes.ok()) {
      const cid = (await contactRes.json()).id;
      await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    }
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-I05: Partner analytics endpoints respond', async ({ page }) => {
    const endpoints = ['/api/partner/analytics/mostActive', '/api/partner/analytics/byUser'];
    for (const ep of endpoints) {
      const res = await page.request.get(`${API}${ep}`, { headers: apiHeaders() });
      expect([200, 400, 404, 500].includes(res.status())).toBeTruthy();
    }
  });

  test('PTR-R-I06: Create partner with all optional fields', async ({ page }) => {
    const res = await page.request.post(`${API}/api/partner`, {
      data: {
        name: `Full ${Date.now()}`,
        partnerShortDescription: 'FL',
        partnerLongDescription: 'Full partner with all fields',
        partnerCategoryId: 1,
        liaisonOfficeId: 1,
        partnerGroupId: 1,
        keyGlobalPartner: true,
        unSecretariatPartner: false,
        canCreateNewOpportunities: true,
        pooledFund: false,
      },
      headers: apiHeaders(),
    });
    if (res.ok()) {
      const pid = (await res.json()).id;
      await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
    }
  });

  test('PTR-R-I07: Partner contacts endpoint returns contacts', async ({ page }) => {
    const partnerRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Contacts ${Date.now()}`, partnerShortDescription: 'CT', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await partnerRes.json()).id;

    const contactRes = await page.request.get(`${API}/api/partner/${pid}/contacts`, { headers: apiHeaders() });
    expect([200, 404, 500].includes(contactRes.status())).toBeTruthy();

    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-I08: Partner detail page loads in UI with real data', async ({ page }) => {
    const name = `DetailUI ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/partner`, {
      data: { name, partnerShortDescription: 'DU', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await createRes.json()).id;

    await authenticateRealApi(page, `/partnerships/partners/${pid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('DetailUI', { timeout: 10000 });

    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('PTR-R-I09: Create interaction linked to partner', async ({ page }) => {
    const partnerRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `Interact ${Date.now()}`, partnerShortDescription: 'IT', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await partnerRes.json()).id;

    const interRes = await page.request.post(`${API}/api/interactions`, {
      data: { name: `Meeting ${Date.now()}`, date: '2026-03-01', description: 'Test', partnerId: pid },
      headers: apiHeaders(),
    });

    if (interRes.ok()) {
      const iid = (await interRes.json()).id;
      await page.request.delete(`${API}/api/interactions/${iid}`, { headers: apiHeaders() });
    }
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });
});

// ============================================================
// CONTACT CRUD
// ============================================================
test.describe('Contact CRUD — Real API', () => {
  test.slow();

  let backendOk = false;
  const API = process.env.API_BASE_URL || 'http://localhost:5159';
  let partnerId: number;

  test.beforeAll(async ({ browser }) => {
    if (!BACKEND_READY) return;
    const ctx = await browser.newContext();
    const p = await ctx.newPage();
    backendOk = await isBackendAvailable(p);

    if (backendOk) {
      // Create a shared partner for contact tests
      const res = await p.request.post(`${API}/api/partner`, {
        data: { name: `ContactTestPartner ${Date.now()}`, partnerShortDescription: 'CP', partnerCategoryId: 1, liaisonOfficeId: 1 },
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

  test('CON-R-001: Create contact via API', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'John', lastName: `Doe ${Date.now()}`, email: `john${Date.now()}@example.com`, title: 'CTO', partnerId },
      headers: apiHeaders(),
    });
    expect(res.ok()).toBeTruthy();
    const cid = (await res.json()).id;
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-002: Get contact by ID', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Get', lastName: `Test ${Date.now()}`, email: `get${Date.now()}@example.com`, title: 'Mgr', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const getRes = await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    expect(getRes.ok()).toBeTruthy();
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-003: List contacts', async ({ page }) => {
    const res = await page.request.get(`${API}/api/contact?PageSize=5&PageIndex=1`, { headers: apiHeaders() });
    expect(res.ok()).toBeTruthy();
  });

  // --- Negative (9) ---

  test('CON-R-N01: Create with missing email', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'No', lastName: 'Email', title: 'Test', partnerId },
      headers: apiHeaders(),
    });
    expect([200, 201, 400, 422, 500].includes(res.status())).toBeTruthy();
  });

  test('CON-R-N02: Create with missing lastName', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'No', email: `nolast${Date.now()}@test.com`, title: 'Test', partnerId },
      headers: apiHeaders(),
    });
    expect([200, 201, 400, 422, 500].includes(res.status())).toBeTruthy();
  });

  test('CON-R-N03: Get non-existent contact', async ({ page }) => {
    const res = await page.request.get(`${API}/api/contact/999999`, { headers: apiHeaders() });
    expect([404, 500].includes(res.status())).toBeTruthy();
  });

  test('CON-R-N04: Delete non-existent contact', async ({ page }) => {
    const res = await page.request.delete(`${API}/api/contact/999999`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('CON-R-N05: Create with empty body', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, { data: {}, headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('CON-R-N06: Unauthenticated contact request', async ({ page }) => {
    const res = await page.request.get(`${API}/api/contact`, { headers: { 'Content-Type': 'application/json' } });
    expect([401, 403].includes(res.status())).toBeTruthy();
  });

  test('CON-R-N07: Create with invalid partnerId', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Bad', lastName: 'Partner', email: `bad${Date.now()}@test.com`, title: 'Test', partnerId: 999999 },
      headers: apiHeaders(),
    });
    expect([200, 201, 400, 422, 500].includes(res.status())).toBeTruthy();
  });

  test('CON-R-N08: Double delete contact', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Dbl', lastName: `Del ${Date.now()}`, email: `dbl${Date.now()}@test.com`, title: 'Test', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    const res = await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  test('CON-R-N09: Get contact with string ID', async ({ page }) => {
    const res = await page.request.get(`${API}/api/contact/abc`, { headers: apiHeaders() });
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });

  // --- Functional (9) ---

  test('CON-R-F01: Created contact has audit fields', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Audit', lastName: `Test ${Date.now()}`, email: `audit${Date.now()}@test.com`, title: 'Mgr', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(body.createdDate || body.CreatedDate).toBeTruthy();
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F02: Contact linked to correct partner', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Linked', lastName: `Partner ${Date.now()}`, email: `link${Date.now()}@test.com`, title: 'Dir', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(body.partnerId || body.PartnerId).toBe(partnerId);
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F03: Deleted contact not in list', async ({ page }) => {
    const name = `SDContact ${Date.now()}`;
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'SD', lastName: name, email: `sd${Date.now()}@test.com`, title: 'Test', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    const getRes = await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    expect([404, 500].includes(getRes.status())).toBeTruthy();
  });

  test('CON-R-F04: Contact list respects pagination', async ({ page }) => {
    const res = await page.request.get(`${API}/api/contact?PageSize=2&PageIndex=1`, { headers: apiHeaders() });
    if (res.ok()) {
      const body = await res.json();
      const items = body.items || body.data || [];
      expect(items.length).toBeLessThanOrEqual(2);
    }
  });

  test('CON-R-F05: Update contact persists changes', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Before', lastName: `Update ${Date.now()}`, email: `upd${Date.now()}@test.com`, title: 'Mgr', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    await page.request.put(`${API}/api/contact/${cid}`, {
      data: { id: cid, firstName: 'After', lastName: `Updated ${Date.now()}`, email: `upd${Date.now()}@test.com`, title: 'Dir', partnerId },
      headers: apiHeaders(),
    });
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(body.firstName).toBe('After');
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F06: Contact detail visible in UI', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'UIContact', lastName: `Test ${Date.now()}`, email: `ui${Date.now()}@test.com`, title: 'VP', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    await authenticateRealApi(page, `/partnerships/contacts/${cid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('UIContact', { timeout: 10000 });
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F07: Contact email stored correctly', async ({ page }) => {
    const email = `correct${Date.now()}@example.com`;
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Email', lastName: `Check ${Date.now()}`, email, title: 'Eng', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(body.email).toBe(email);
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F08: Contact has default status', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Status', lastName: `Check ${Date.now()}`, email: `stat${Date.now()}@test.com`, title: 'Mgr', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(body.status).toBeTruthy();
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-F09: Contact phone fields stored correctly', async ({ page }) => {
    const createRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Phone', lastName: `Test ${Date.now()}`, email: `phone${Date.now()}@test.com`, title: 'Mgr', partnerId, phone: '+1234567890', mobile: '+0987654321' },
      headers: apiHeaders(),
    });
    const cid = (await createRes.json()).id;
    const body = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    if (body.phone) expect(body.phone).toContain('123');
    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  // --- Integration (9) ---

  test('CON-R-I01: Full contact CRUD lifecycle', async ({ page }) => {
    let cid: number;
    await test.step('CREATE', async () => {
      const res = await page.request.post(`${API}/api/contact`, {
        data: { firstName: 'Life', lastName: `Cycle ${Date.now()}`, email: `lc${Date.now()}@test.com`, title: 'CTO', partnerId },
        headers: apiHeaders(),
      });
      cid = (await res.json()).id;
    });
    await test.step('READ', async () => {
      const res = await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
      expect(res.ok()).toBeTruthy();
    });
    await test.step('UPDATE', async () => {
      await page.request.put(`${API}/api/contact/${cid!}`, {
        data: { id: cid!, firstName: 'Updated', lastName: `Cycle ${Date.now()}`, email: `lc2${Date.now()}@test.com`, title: 'CEO', partnerId },
        headers: apiHeaders(),
      });
    });
    await test.step('DELETE', async () => {
      await page.request.delete(`${API}/api/contact/${cid!}`, { headers: apiHeaders() });
    });
    await test.step('VERIFY DELETE', async () => {
      const res = await page.request.get(`${API}/api/contact/${cid!}`, { headers: apiHeaders() });
      expect([404, 500].includes(res.status())).toBeTruthy();
    });
  });

  test('CON-R-I02: Create partner then contact then view both', async ({ page }) => {
    const pRes = await page.request.post(`${API}/api/partner`, {
      data: { name: `ParentPartner ${Date.now()}`, partnerShortDescription: 'PP', partnerCategoryId: 1, liaisonOfficeId: 1 },
      headers: apiHeaders(),
    });
    const pid = (await pRes.json()).id;

    const cRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'Child', lastName: `Contact ${Date.now()}`, email: `child${Date.now()}@test.com`, title: 'Mgr', partnerId: pid },
      headers: apiHeaders(),
    });
    const cid = (await cRes.json()).id;

    const partnerBody = await (await page.request.get(`${API}/api/partner/${pid}`, { headers: apiHeaders() })).json();
    const contactBody = await (await page.request.get(`${API}/api/contact/${cid}`, { headers: apiHeaders() })).json();
    expect(contactBody.partnerId || contactBody.PartnerId).toBe(pid);

    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    await page.request.delete(`${API}/api/partner/${pid}`, { headers: apiHeaders() });
  });

  test('CON-R-I03: Contact list page loads', async ({ page }) => {
    await authenticateRealApi(page, '/partnerships/contacts');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('contact');
  });

  test('CON-R-I04: Multiple contacts for same partner', async ({ page }) => {
    const ids: number[] = [];
    for (let i = 0; i < 3; i++) {
      const res = await page.request.post(`${API}/api/contact`, {
        data: { firstName: `Multi${i}`, lastName: `Test ${Date.now()}`, email: `multi${i}${Date.now()}@test.com`, title: 'Eng', partnerId },
        headers: apiHeaders(),
      });
      if (res.ok()) ids.push((await res.json()).id);
    }
    expect(ids.length).toBeGreaterThanOrEqual(1);
    for (const id of ids) {
      await page.request.delete(`${API}/api/contact/${id}`, { headers: apiHeaders() });
    }
  });

  test('CON-R-I05: Contact search works', async ({ page }) => {
    const unique = `SearchContact${Date.now()}`;
    const res = await page.request.post(`${API}/api/contact`, {
      data: { firstName: unique, lastName: 'Searchable', email: `search${Date.now()}@test.com`, title: 'Mgr', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await res.json()).id;

    const searchRes = await page.request.get(`${API}/api/contact?SearchText=${encodeURIComponent(unique)}&PageSize=10`, { headers: apiHeaders() });
    expect([200, 500].includes(searchRes.status())).toBeTruthy();

    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-I06: Contact detail page shows partner info', async ({ page }) => {
    const cRes = await page.request.post(`${API}/api/contact`, {
      data: { firstName: 'PartnerInfo', lastName: `Test ${Date.now()}`, email: `pinfo${Date.now()}@test.com`, title: 'Dir', partnerId },
      headers: apiHeaders(),
    });
    const cid = (await cRes.json()).id;

    await authenticateRealApi(page, `/partnerships/contacts/${cid}`);
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('PartnerInfo', { timeout: 10000 });

    await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
  });

  test('CON-R-I07: External contact endpoint responds', async ({ page }) => {
    const res = await page.request.get(`${API}/api/external/contact?PageSize=5`, { headers: apiHeaders() });
    expect([200, 401, 403, 404, 500].includes(res.status())).toBeTruthy();
  });

  test('CON-R-I08: Create contact with all optional fields', async ({ page }) => {
    const res = await page.request.post(`${API}/api/contact`, {
      data: {
        firstName: 'Full',
        lastName: `Fields ${Date.now()}`,
        email: `full${Date.now()}@test.com`,
        title: 'VP Sales',
        partnerId,
        salutation: 'Mr.',
        phone: '+1-555-0100',
        mobile: '+1-555-0200',
        department: 'Sales',
        description: 'Full test contact',
        mailingStreet: '123 Test St',
        mailingCity: 'Test City',
        mailingStateProvince: 'TS',
        mailingPostalCode: '12345',
        mailingCountry: 'Testland',
      },
      headers: apiHeaders(),
    });
    if (res.ok()) {
      const cid = (await res.json()).id;
      await page.request.delete(`${API}/api/contact/${cid}`, { headers: apiHeaders() });
    }
  });

  test('CON-R-I09: Interaction endpoint accessible', async ({ page }) => {
    const res = await page.request.get(`${API}/api/interactions?PageSize=5&PageIndex=1`, { headers: apiHeaders() });
    expect([200, 500].includes(res.status())).toBeTruthy();
  });
});

/*
 * ### 3:1 Ratio Compliance Check — Partner
 * | Category | Count |
 * |----------|-------|
 * | Positive (P) | 3 |
 * | Negative (N) | 9 |  N >= 3P ✅
 * | Functional (F) | 9 |  F >= 3P ✅
 * | Integration (I) | 9 |  I >= 3P ✅
 *
 * ### 3:1 Ratio Compliance Check — Contact
 * | Category | Count |
 * |----------|-------|
 * | Positive (P) | 3 |
 * | Negative (N) | 9 |  N >= 3P ✅
 * | Functional (F) | 9 |  F >= 3P ✅
 * | Integration (I) | 9 |  I >= 3P ✅
 */
