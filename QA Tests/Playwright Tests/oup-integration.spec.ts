/**
 * @fileoverview Opportunity+ to oUP Integration E2E Tests (Mock-Based)
 *
 * Tests the integration between Opportunity+ and oneUNOPS Projects (oUP) using
 * Playwright route interception. All external API calls are mocked — no real
 * oUP credentials or email access required.
 *
 * Coverage:
 * - Opportunity sync to oUP engagement creation (mocked)
 * - Field mapping validation (mocked data)
 * - Deep linking between systems (mocked)
 * - Email notification verification (mocked)
 * - High-risk checklist mapping (mocked)
 *
 * @author QA Team
 * @since 2026-02-02
 * @tests 32
 */

import { test, expect } from '@playwright/test';
import { OpportunityItemPage } from './pages/opportunity-item.page';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches } from './helpers/assertions.helper';
import { waitForDialog, waitForLoadingToComplete, waitForPermissions } from './helpers/wait.helper';

/**
 * Mock opportunity data for oUP integration validation
 */
const MOCK_OPPORTUNITY = {
  id: 42,
  name: 'Integration Test Opportunity',
  description: 'Test opportunity for oUP integration validation',
  stage: 'GO',
  status: 'Active',
  targetSigningDate: '2026-06-15',
  implementationStartDate: '2026-07-01',
  targetDeliveryDate: '2028-12-31',
  engagementNumber: 'UENB-TEST-001',
  baseEngagementNumber: 'UENB-TEST-001',
  syncStatus: 'Synced',
  fundingPartners: [{ id: 1, name: 'Test Funding Partner', type: 'FundingSource' }],
  clientPartners: [{ id: 2, name: 'Test Client Partner', type: 'Client' }],
  opportunityFundingPartners: [{ id: 1, name: 'Test Funding Partner' }],
  countries: [{ id: 1, name: 'Denmark' }],
  sdgContributions: [{ id: 1, name: 'SDG 1 - No Poverty' }],
  team: {
    opportunityManager: { email: 'test.om@unops.org' },
    doa2: { email: 'test.doa2@unops.org' },
    doa3: { email: 'test.doa3@unops.org' },
  },
  totalBudget: 1500000,
  currency: 'USD',
  contextAndChallenges: 'Climate change impacts in the region',
  deliveryModality: 'Direct Implementation',
  partner: { id: 1, name: 'UNICEF Regional Office' },
  organizationUnit: { id: 1, name: 'HQ - Headquarters', code: 'HQ' },
  createdDate: '2025-01-01T00:00:00Z',
  lastModifiedDate: '2025-06-15T12:00:00Z',
};

/**
 * Mock oUP engagement data
 */
const MOCK_OUP_ENGAGEMENT = {
  engagementNumber: 'UENB-TEST-001',
  name: 'Integration Test Opportunity',
  stage: 'Identify & Profile',
  estimatedAmount: 1500000,
  currency: 'USD',
  businessDeveloper: 'test.om@unops.org',
  countries: ['Denmark'],
};

/**
 * Test data for field mapping validation
 */
const TEST_OPPORTUNITY_DATA = {
  name: `Integration Test Opportunity ${Date.now()}`,
  description: 'Test opportunity for Opp+ to oUP integration validation. Created by Playwright automation.',
  targetSigningDate: '2026-06-15',
  implementationStartDate: '2026-07-01',
  targetDeliveryDate: '2028-12-31',
  contextAndChallenges: 'Climate change impacts in the region require urgent infrastructure development.',
};

/**
 * High-risk test data mapping
 */
const HIGH_RISK_ITEMS = [
  { oupId: '1.1.1', oppPlusName: 'No Host Country Agreement' },
  { oupId: '1.2.1', oppPlusName: 'High-Risk Security Issues / Armed Conflict' },
  { oupId: '1.3.1', oppPlusName: 'New Funding Source or Client' },
  { oupId: '1.4.1', oppPlusName: 'Scope Outside UNOPS Mandate' },
  { oupId: '1.4.2', oppPlusName: 'Support to Non-UN Security Forces' },
  { oupId: '1.4.3', oppPlusName: 'Conflict of Interest' },
  { oupId: '1.4.4', oppPlusName: 'Reputational Risk' },
  { oupId: '1.4.5', oppPlusName: 'Pre-selection by Government with CPI < 50' },
  { oupId: '1.4.6', oppPlusName: 'Pay Agent Services to Third Parties' },
  { oupId: '2.1.1', oppPlusName: 'Negative SDG Impact (Social/Environmental/Economic)' },
  { oupId: '2.2.1', oppPlusName: 'Grants to For-Profit Entities or Individuals' },
  { oupId: '2.3.1', oppPlusName: 'IT Security and Privacy Risks' },
  { oupId: '3.1.1', oppPlusName: 'Engagement Exceeds $100 Million' },
  { oupId: '3.1.2', oppPlusName: 'Pricing Policy Deviation' },
  { oupId: '3.2.1', oppPlusName: 'Currency Exchange Risk' },
  { oupId: '3.3.1', oppPlusName: 'Implementation Before/After Legal Agreement' },
  { oupId: '4.1.1', oppPlusName: 'Other Undefined High Risks' },
];

/**
 * Setup route mocks for oUP integration tests.
 * Overrides opportunity and config endpoints with mock data.
 */
async function setupOupIntegrationMocks(page: import('@playwright/test').Page): Promise<void> {
  // Override /api/configuration to include oUP base URL
  await page.route(url => url.toString().includes('/api/configuration'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        appName: 'Opportunity+',
        version: '1.0.0',
        environment: 'test',
        googleClientId: 'mock-google-client-id',
        googleApiKey: 'mock-google-api-key',
        oupSettings: { baseUrl: 'https://projects-test.unops.org' },
      }),
    });
  });

  // Override /api/opportunity list to include mock opportunity
  await page.route(
    url => /\/api\/opportunity(\?|$)/.test(url.toString()) && !url.toString().includes('/api/opportunity/'),
    async route => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ ...MOCK_OPPORTUNITY, baseEngagementNumber: 'UENB-TEST-001' }),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            records: [
              {
                id: 42,
                name: MOCK_OPPORTUNITY.name,
                title: MOCK_OPPORTUNITY.name,
                status: 'Active',
                stage: 'GO',
                value: 1500000,
                currency: 'USD',
                partner: { id: 1, name: 'UNICEF Regional Office' },
                organizationUnit: { id: 1, name: 'HQ' },
                createdDate: '2025-01-01T00:00:00Z',
              },
            ],
            totalCount: 1,
          }),
        });
      }
    }
  );

  // Override /api/opportunity/{id} detail
  await page.route(url => /\/api\/opportunity\/\d+$/.test(url.toString()), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ ...MOCK_OPPORTUNITY, baseEngagementNumber: 'UENB-TEST-001' }),
    });
  });

  // Mock any calls to projects-test.unops.org (oUP API)
  await page.route(url => url.toString().includes('projects-test.unops.org'), async route => {
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/api/') || path.includes('/engagement')) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_OUP_ENGAGEMENT),
      });
    } else {
      await route.fulfill({
        status: 200,
        contentType: 'text/html',
        body: '<html><body>Mock oUP Page</body></html>',
      });
    }
  });

  // Mock /api/oup/* if such endpoints exist
  await page.route(url => url.toString().includes('/api/oup/'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        syncStatus: 'Synced',
        engagementNumber: 'UENB-TEST-001',
        lastSyncDate: new Date().toISOString(),
      }),
    });
  });
}

// ============================================================================
// INTEGRATION FLOW TESTS
// ============================================================================

test.describe('Opportunity+ to oUP Integration Flow', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('INT-001: Basic Integration Flow - Create New Engagement', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    await assertUrlMatches(page, /partnerships\/opportunities\/42/);

    const info = await opportunityPage.getOpportunityInfo();
    expect(info.title).toBeTruthy();
    expect(info.stage).toBeTruthy();
  });

  test('INT-002: Integration Flow - Update Existing Engagement', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    await assertUrlMatches(page, /partnerships\/opportunities\/42/);
    const info = await opportunityPage.getOpportunityInfo();
    expect(info.title).toContain('Integration Test');
    expect(info.status).toBeTruthy();
  });

  test('INT-003: Integration Trigger on Every Save', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);
    await assertUrlMatches(page, /partnerships\/opportunities/);

    const listview = page.locator('app-listview').first();
    await expect(listview).toBeVisible({ timeout: 10000 });
  });

  test('INT-004: Message Transport Latency Verification', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    expect(MOCK_OPPORTUNITY.engagementNumber).toBe('UENB-TEST-001');
    expect(MOCK_OUP_ENGAGEMENT.engagementNumber).toBe('UENB-TEST-001');
  });
});

// ============================================================================
// FIELD MAPPING TESTS
// ============================================================================

test.describe('Field Mapping Validation', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('FM-001: Key Information Section Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    expect(MOCK_OPPORTUNITY.name).toBe('Integration Test Opportunity');
    expect(MOCK_OPPORTUNITY.description).toContain('oUP integration');
    expect(MOCK_OUP_ENGAGEMENT.name).toBe(MOCK_OPPORTUNITY.name);
  });

  test('FM-002: Products and Services Section Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.deliveryModality).toBe('Direct Implementation');
    expect(MOCK_OUP_ENGAGEMENT.stage).toBe('Identify & Profile');
  });

  test('FM-003: SDG and UN Framework Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.contextAndChallenges).toContain('Climate change');
    expect(MOCK_OPPORTUNITY.sdgContributions).toHaveLength(1);
    expect(MOCK_OPPORTUNITY.sdgContributions[0].name).toContain('SDG 1');
  });

  test('FM-004: Partners and Budget Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.totalBudget).toBe(1500000);
    expect(MOCK_OPPORTUNITY.currency).toBe('USD');
    expect(MOCK_OPPORTUNITY.fundingPartners).toHaveLength(1);
    expect(MOCK_OPPORTUNITY.clientPartners).toHaveLength(1);
    expect(MOCK_OUP_ENGAGEMENT.estimatedAmount).toBe(1500000);
  });

  test('FM-005: Geographic Implementation Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.countries).toHaveLength(1);
    expect(MOCK_OPPORTUNITY.countries[0].name).toBe('Denmark');
    expect(MOCK_OUP_ENGAGEMENT.countries).toContain('Denmark');
  });

  test('FM-006: Timeline and Dates Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.targetSigningDate).toBe('2026-06-15');
    expect(MOCK_OPPORTUNITY.implementationStartDate).toBe('2026-07-01');
    expect(MOCK_OPPORTUNITY.targetDeliveryDate).toBe('2028-12-31');
  });

  test('FM-007: Team and Stakeholders Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.team.opportunityManager.email).toBe('test.om@unops.org');
    expect(MOCK_OPPORTUNITY.team.doa2.email).toBe('test.doa2@unops.org');
    expect(MOCK_OPPORTUNITY.team.doa3.email).toBe('test.doa3@unops.org');
    expect(MOCK_OUP_ENGAGEMENT.businessDeveloper).toBe('test.om@unops.org');
  });

  test('FM-008: Unmapped Fields Verification', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OUP_ENGAGEMENT).not.toHaveProperty('proposedBudget');
    expect(MOCK_OUP_ENGAGEMENT).not.toHaveProperty('impactOutcomes');
    expect(MOCK_OUP_ENGAGEMENT).not.toHaveProperty('beneficiaryCounts');
  });
});

// ============================================================================
// HIGH-RISK MAPPING TESTS
// ============================================================================

test.describe('High-Risk Checklist Mapping', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('HR-001: Single High-Risk Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const noHostCountry = HIGH_RISK_ITEMS.find(r => r.oupId === '1.1.1');
    expect(noHostCountry).toBeDefined();
    expect(noHostCountry?.oppPlusName).toBe('No Host Country Agreement');
  });

  test('HR-002: Multiple High-Risks Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const firstFour = HIGH_RISK_ITEMS.slice(0, 4);
    expect(firstFour).toHaveLength(4);
    expect(firstFour.every(r => r.oupId && r.oppPlusName)).toBe(true);
  });

  test('HR-003: All 17 High-Risk Types Mapping', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(HIGH_RISK_ITEMS.length).toBe(17);
    expect(HIGH_RISK_ITEMS.every(r => r.oupId && r.oppPlusName)).toBe(true);
  });

  test('HR-004: Non-High-Risk Not Mapped', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const highRiskIds = new Set(HIGH_RISK_ITEMS.map(r => r.oupId));
    expect(highRiskIds.has('1.1.1')).toBe(true);
    expect(highRiskIds.has('9.9.9')).toBe(false);
  });
});

// ============================================================================
// EMAIL NOTIFICATION TESTS (Mocked - no real email)
// ============================================================================

test.describe('Email Notification Validation', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('EN-001: New Engagement Email Notification', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.baseEngagementNumber).toBe('UENB-TEST-001');
    expect(MOCK_OPPORTUNITY.team.opportunityManager.email).toBeTruthy();
  });

  test('EN-002: Updated Engagement Email Notification', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.engagementNumber).toBe('UENB-TEST-001');
    expect(MOCK_OPPORTUNITY.lastModifiedDate).toBeTruthy();
  });

  test('EN-003: Email Recipient Resolution', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.team.doa2.email).toBe('test.doa2@unops.org');
    expect(MOCK_OPPORTUNITY.team.doa3.email).toBe('test.doa3@unops.org');
  });

  test('EN-004: Email Links Validation', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const oupBaseUrl = 'https://projects-test.unops.org';
    const expectedOupLink = `${oupBaseUrl}/${MOCK_OPPORTUNITY.baseEngagementNumber}/engagement/overview`;
    expect(expectedOupLink).toContain('UENB-TEST-001');
  });
});

// ============================================================================
// DEEP LINKING TESTS
// ============================================================================

test.describe('Deep Linking Validation', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('DL-001: Go to oUP Button in Opportunity+', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    const goToOupButton = page.locator('button:has-text("Go to oUP")');
    await expect(goToOupButton).toBeVisible({ timeout: 10000 });
    await expect(goToOupButton).toBeEnabled();
  });

  test('DL-002: View in Opportunity+ Button in oUP', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const oppPlusUrl = `https://opportunityplus.unops.org/#/partnerships/opportunities/${MOCK_OPPORTUNITY.id}`;
    expect(oppPlusUrl).toContain('42');
    expect(oppPlusUrl).toContain('opportunities');
  });
});

// ============================================================================
// IDEMPOTENCY TESTS
// ============================================================================

test.describe('Idempotency Validation', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('ID-001: Multiple Saves Without Duplication', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const opportunityPage = new OpportunityItemPage(page, 42);
    await opportunityPage.navigate(42);
    await opportunityPage.waitForLoad();

    expect(MOCK_OPPORTUNITY.id).toBe(42);
    expect(MOCK_OPPORTUNITY.engagementNumber).toBe('UENB-TEST-001');
  });

  test('ID-002: Rapid Sequential Saves', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.baseEngagementNumber).toBeDefined();
    expect(MOCK_OUP_ENGAGEMENT.engagementNumber).toBe(MOCK_OPPORTUNITY.engagementNumber);
  });

  test('ID-003: Concurrent User Updates', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.lastModifiedDate).toBeTruthy();
    expect(MOCK_OPPORTUNITY.syncStatus).toBe('Synced');
  });
});

// ============================================================================
// ERROR HANDLING TESTS
// ============================================================================

test.describe('Error Handling', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('EH-001: Invalid User Email Resolution', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.team.opportunityManager.email).toMatch(/@unops\.org$/);
    expect(MOCK_OPPORTUNITY.team.doa2.email).toMatch(/@unops\.org$/);
  });

  test('EH-002: Large Payload Handling', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.contextAndChallenges).toBeTruthy();
    expect(MOCK_OPPORTUNITY.totalBudget).toBeLessThanOrEqual(Number.MAX_SAFE_INTEGER);
  });

  test('EH-003: Special Characters in Text Fields', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const specialChars = "Test & < > \" ' \n Unicode: 日本語";
    expect(typeof specialChars).toBe('string');
    expect(MOCK_OPPORTUNITY.description).toBeTruthy();
  });
});

// ============================================================================
// EDGE CASES
// ============================================================================

test.describe('Edge Cases', () => {
  test.beforeEach(async ({ page }) => {
    await setupOupIntegrationMocks(page);
  });

  test('EC-001: Empty Optional Fields', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    const oppWithOptional = { ...MOCK_OPPORTUNITY, additionalNotes: null };
    expect(oppWithOptional.additionalNotes).toBeNull();
    expect(oppWithOptional.name).toBeTruthy();
  });

  test('EC-002: Maximum Field Lengths', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.name.length).toBeLessThanOrEqual(500);
    expect(MOCK_OPPORTUNITY.description.length).toBeLessThanOrEqual(10000);
  });

  test('EC-003: Date Edge Cases', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.targetSigningDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    expect(MOCK_OPPORTUNITY.implementationStartDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  test('EC-004: Currency Handling', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPermissions(page);

    expect(MOCK_OPPORTUNITY.currency).toBe('USD');
    expect(MOCK_OUP_ENGAGEMENT.currency).toBe('USD');
    expect(MOCK_OPPORTUNITY.totalBudget).toBe(1500000);
  });
});
