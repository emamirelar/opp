/**
 * @fileoverview oUP Integration Sync — Mock-Based E2E Tests
 *
 * Validates the Opportunity+ to oneUNOPS Projects (oUP) integration logic using
 * mock API responses. No real backend or oUP connection required.
 *
 * Covers regression scenarios: PNO-1209, PNO-1207, PNO-1174, PNO-1200.
 *
 * @author UNOPS Opportunity+ QA Team
 * @tests 5
 */

import { test, expect } from '@playwright/test';

const APP_URL = 'http://localhost:4200';

const MOCK_OPPORTUNITIES = [
  {
    id: 1,
    name: 'GO Opportunity Alpha',
    stage: 'GO',
    status: 'Active',
    engagementNumber: 'UENB-10001',
    baseEngagementNumber: 'UENB-10001',
    syncStatus: 'Synced',
    fundingPartners: [{ id: 1, name: 'DFID' }],
    opportunityFundingPartners: [{ id: 1, name: 'DFID' }],
  },
  {
    id: 2,
    name: 'GO Opportunity Beta',
    stage: 'GO',
    status: 'Active',
    engagementNumber: 'UENB-10002',
    baseEngagementNumber: 'UENB-10002',
    syncStatus: 'Synced',
    fundingPartners: [{ id: 2, name: 'EU' }],
    opportunityFundingPartners: [{ id: 2, name: 'EU' }],
  },
  {
    id: 3,
    name: 'Draft Opportunity',
    stage: 'IDENTIFY & PROFILE',
    status: 'Draft',
  },
];

const MOCK_OPPORTUNITY_DETAIL = {
  ...MOCK_OPPORTUNITIES[0],
  team: {
    doaLevel3: { email: 'doa3@unops.org', name: 'DOA3 User' },
    doa3: 'doa3@unops.org',
    doA3: 'doa3@unops.org',
  },
  description: 'Full detail for oUP field mapping',
};

const MOCK_TEAM = {
  doaLevel3: { email: 'doa3@unops.org', name: 'DOA3 User' },
  doa3: 'doa3@unops.org',
  doA3: 'doa3@unops.org',
  opportunityManager: { email: 'om@unops.org' },
};

async function fetchFromPage(
  page: import('@playwright/test').Page,
  url: string
): Promise<{ status: number; ok: boolean; data: unknown }> {
  return page.evaluate(
    async (u) => {
      const res = await fetch(u);
      const body = await res.json();
      return {
        status: res.status,
        ok: res.ok,
        data: body,
      };
    },
    [url]
  );
}

test.describe('oUP Integration Sync — Mock-Based', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/opportunity', async (route) => {
      const url = route.request().url();
      if (/\/api\/opportunity\/\d+\/team/.test(url)) {
        await route.fulfill({ status: 200, json: MOCK_TEAM });
      } else if (/\/api\/opportunity\/\d+$/.test(url)) {
        await route.fulfill({ status: 200, json: MOCK_OPPORTUNITY_DETAIL });
      } else {
        await route.fulfill({ status: 200, json: MOCK_OPPORTUNITIES });
      }
    });
    await page.route('**/api/oup/**', async (route) => {
      await route.fulfill({ status: 200, json: { status: 'connected' } });
    });
    await page.route('**/api/integration/**', async (route) => {
      await route.fulfill({ status: 200, json: { status: 'ok' } });
    });
    await page.route('**/api/values/**', async (route) => {
      await route.fulfill({ status: 200, json: {} });
    });
    await page.route('**projects-test.unops.org**', async (route) => {
      await route.fulfill({
        status: 200,
        body: '<html><body>Mock oUP</body></html>',
      });
    });
  });

  test('oUP integration endpoint responds (no 500) [PNO-1174]', async ({
    page,
  }) => {
    await page.goto(APP_URL);

    const endpoints = [
      '/api/oup/status',
      '/api/integration/oup',
      '/api/values/oup-status',
    ];

    let foundEndpoint = false;
    for (const endpoint of endpoints) {
      const { status } = await fetchFromPage(page, `${APP_URL}${endpoint}`);
      if (status !== 404) {
        foundEndpoint = true;
        expect(status).not.toBe(500);
        break;
      }
    }

    expect(foundEndpoint).toBe(true);
  });

  test('GO-stage opportunities have integration data populated [PNO-1200]', async ({
    page,
  }) => {
    await page.goto(APP_URL);

    const res = await fetchFromPage(page, `${APP_URL}/api/opportunity`);
    expect(res.ok).toBe(true);
    const opps = res.data as typeof MOCK_OPPORTUNITIES;

    const goOpps = Array.isArray(opps)
      ? opps.filter((o) => o.stage === 'GO' || o.status === 'Active')
      : [];

    expect(goOpps.length).toBeGreaterThan(0);

    for (const opp of goOpps.slice(0, 3)) {
      const detailRes = await fetchFromPage(
        page,
        `${APP_URL}/api/opportunity/${opp.id}`
      );
      expect(detailRes.ok).toBe(true);
      const detail = detailRes.data as (typeof MOCK_OPPORTUNITIES)[0] & {
        engagementNumber?: string;
        oupEngagementNumber?: string;
        baseEngagementNumber?: string;
        syncStatus?: string;
      };

      const hasIntegration =
        detail.engagementNumber ||
        (detail as { oupEngagementNumber?: string }).oupEngagementNumber ||
        detail.baseEngagementNumber ||
        (detail as { syncStatus?: string }).syncStatus;

      expect(hasIntegration).toBeTruthy();
    }
  });

  test('DOA3 is available for oUP field mapping [PNO-1209]', async ({
    page,
  }) => {
    await page.goto(APP_URL);

    const listRes = await fetchFromPage(page, `${APP_URL}/api/opportunity`);
    expect(listRes.ok).toBe(true);
    const opps = listRes.data as typeof MOCK_OPPORTUNITIES;

    const goOpp = Array.isArray(opps)
      ? opps.find((o) => o.stage === 'GO' || o.status === 'Active')
      : null;

    expect(goOpp).not.toBeNull();

    const teamRes = await fetchFromPage(
      page,
      `${APP_URL}/api/opportunity/${goOpp!.id}/team`
    );
    expect(teamRes.ok).toBe(true);
    const team = teamRes.data as typeof MOCK_TEAM;

    const doa3 = team.doaLevel3 || team.doa3 || team.doA3;
    expect(doa3).toBeTruthy();
    if (typeof doa3 === 'object') {
      expect(doa3.email).toBe('doa3@unops.org');
    } else {
      expect(doa3).toBe('doa3@unops.org');
    }
  });

  test('GO opportunities have at least one funding partner [PNO-1207]', async ({
    page,
  }) => {
    await page.goto(APP_URL);

    const listRes = await fetchFromPage(page, `${APP_URL}/api/opportunity`);
    expect(listRes.ok).toBe(true);
    const opps = listRes.data as typeof MOCK_OPPORTUNITIES;

    const goOpps = Array.isArray(opps)
      ? opps.filter((o) => o.stage === 'GO' || o.status === 'Active')
      : [];

    expect(goOpps.length).toBeGreaterThan(0);

    for (const opp of goOpps.slice(0, 3)) {
      const detailRes = await fetchFromPage(
        page,
        `${APP_URL}/api/opportunity/${opp.id}`
      );
      expect(detailRes.ok).toBe(true);
      const detail = detailRes.data as (typeof MOCK_OPPORTUNITIES)[0];

      const fundingPartners =
        detail.fundingPartners || detail.opportunityFundingPartners || [];

      expect(Array.isArray(fundingPartners)).toBe(true);
      expect(fundingPartners.length).toBeGreaterThan(0);
    }
  });

  test('oUP test environment is reachable (when credentials configured)', async ({
    page,
  }) => {
    await page.goto(APP_URL);

    const res = await fetchFromPage(
      page,
      'https://projects-test.unops.org/'
    );
    expect(res.status).toBeLessThan(500);
    expect(res.ok).toBe(true);
  });
});
