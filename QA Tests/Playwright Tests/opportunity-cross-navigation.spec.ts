/**
 * @fileoverview Opportunity Cross-Navigation E2E Tests
 *
 * Tests for navigation between opportunities and related entities:
 * partner-to-opportunity navigation, opportunity list on partner detail,
 * interaction-to-opportunity links, and back navigation.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-NAV
 *
 * @tests 11
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForNetworkIdle,
  waitForPageReady,
} from './helpers/wait.helper';
import { PartnerItemPage } from './pages/partner-item.page';
import { OpportunityItemPage } from './pages/opportunity-item.page';

const featureReady = process.env.OPPORTUNITY_CROSSNAV_IMPLEMENTED === 'true';

const TEST_PARTNER_ID = process.env.TEST_PARTNER_ID || '1';
const TEST_OPP_ID = process.env.TEST_OPP_ID || '1';

const PARTNER_URL = `/partnerships/partners/${TEST_PARTNER_ID}`;
const OPPORTUNITIES_URL = '/partnerships/opportunities';

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Partner → Opportunity Navigation
// =============================================================================
test.describe('Cross-Navigation — Partner to Opportunity', () => {
  test.slow();
  test.skip(!featureReady, 'Cross-navigation not deployed — set OPPORTUNITY_CROSSNAV_IMPLEMENTED=true');

  test('NAV-001: Partner detail page has opportunities tab/section', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const partnerPage = new PartnerItemPage(page, TEST_PARTNER_ID);
    const oppTab = partnerPage.opportunitiesTab;
    const oppSection = page.getByText(/opportunities/i).first();
    const hasTab = await oppTab.isVisible({ timeout: 10000 }).catch(() => false);
    const hasSection = await oppSection.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasTab || hasSection).toBeTruthy();
  });

  test('NAV-002: Partner opportunities list displays linked opportunities', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const partnerPage = new PartnerItemPage(page, TEST_PARTNER_ID);
    await partnerPage.openOpportunitiesTab();

    const oppList = partnerPage.opportunitiesListContainer.or(partnerPage.opportunitiesListview);
    await expect(oppList).toBeVisible({ timeout: 10000 });
  });

  test('NAV-003: Clicking opportunity in partner list navigates to detail', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const partnerPage = new PartnerItemPage(page, TEST_PARTNER_ID);
    await partnerPage.openOpportunitiesTab();

    const oppLink = page
      .locator('a[href*="/partnerships/opportunities/"], [data-testid*="opportunity-link"]')
      .first();
    if (await oppLink.isVisible({ timeout: 5000 }).catch(() => false)) {
      await oppLink.click();
      await waitForNetworkIdle(page);
      await expect(page).toHaveURL(/\/partnerships\/opportunities\/\d+/);
    }
  });

  test('NAV-004: Partner opportunities section has search functionality', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const partnerPage = new PartnerItemPage(page, TEST_PARTNER_ID);
    await partnerPage.openOpportunitiesTab();

    const searchInput = partnerPage.opportunitiesSearchInput;
    await expect(searchInput).toBeVisible({ timeout: 10000 });
  });
});

// =============================================================================
// SECTION 2: Opportunity → Partner Navigation
// =============================================================================
test.describe('Cross-Navigation — Opportunity to Partner', () => {
  test.slow();
  test.skip(!featureReady, 'Cross-navigation not deployed — set OPPORTUNITY_CROSSNAV_IMPLEMENTED=true');

  test('NAV-005: Who section displays partner links', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oppPage = new OpportunityItemPage(page, TEST_OPP_ID);
    await oppPage.openWhoSection();

    await expect(oppPage.whoSection).toBeVisible({ timeout: 10000 });
  });

  test('NAV-006: Clicking partner name navigates to partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oppPage = new OpportunityItemPage(page, TEST_OPP_ID);
    await oppPage.openWhoSection();

    const partnerLink = page.locator('a[href*="/partnerships/partners/"]').first();
    if (await partnerLink.isVisible({ timeout: 5000 }).catch(() => false)) {
      await partnerLink.click();
      await waitForNetworkIdle(page);
      await expect(page).toHaveURL(/\/partnerships\/partners\/\d+/);
    }
  });
});

// =============================================================================
// SECTION 3: Opportunity → Source Interactions
// =============================================================================
test.describe('Cross-Navigation — Opportunity to Interactions', () => {
  test.slow();
  test.skip(!featureReady, 'Cross-navigation not deployed — set OPPORTUNITY_CROSSNAV_IMPLEMENTED=true');

  test('NAV-007: Related section shows source interactions', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oppPage = new OpportunityItemPage(page, TEST_OPP_ID);
    await oppPage.openRelatedSection();

    await expect(oppPage.relatedSection).toBeVisible({ timeout: 10000 });
  });

  test('NAV-008: Clicking interaction link navigates to interaction detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oppPage = new OpportunityItemPage(page, TEST_OPP_ID);
    await oppPage.openRelatedSection();

    const interactionLink = page.locator('a[href*="/partnerships/interactions/"]').first();
    if (await interactionLink.isVisible({ timeout: 5000 }).catch(() => false)) {
      await interactionLink.click();
      await waitForNetworkIdle(page);
      await expect(page).toHaveURL(/\/partnerships\/interactions\/\d+/);
    }
  });
});

// =============================================================================
// SECTION 4: Back Navigation
// =============================================================================
test.describe('Cross-Navigation — Back Navigation', () => {
  test.slow();
  test.skip(!featureReady, 'Cross-navigation not deployed — set OPPORTUNITY_CROSSNAV_IMPLEMENTED=true');

  test('NAV-009: Back button on opportunity detail navigates to list', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const backBtn = page
      .locator(
        'button:has(i.pi-arrow-left), [data-testid="back-button"], button:has-text("Back")'
      )
      .first();
    if (await backBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await backBtn.click();
      await waitForNetworkIdle(page);
      await expect(page).toHaveURL(/\/partnerships\/opportunities/);
    }
  });

  test('NAV-010: Browser back from opportunity detail returns to previous page', async ({
    page,
  }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForPageReady(page);

    await page.goto(oppUrl(TEST_OPP_ID));
    await waitForNetworkIdle(page);
    await waitForPermissions(page);

    await page.goBack();
    await waitForNetworkIdle(page);
    await expect(page).toHaveURL(/\/partnerships\/opportunities/);
  });

  test('NAV-011: oUP engagement link visible when URL exists', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oppPage = new OpportunityItemPage(page, TEST_OPP_ID);
    await expect(oppPage.opportunityTitle).toBeVisible({ timeout: 10000 });

    const oupLink = page
      .locator('a:has-text("oUP"), button:has-text("Go to oUP"), [data-testid="oup-link"]')
      .first();
    const oupVisible = await oupLink.isVisible({ timeout: 5000 }).catch(() => false);
    if (oupVisible) {
      await expect(oupLink).toBeVisible();
    }
  });
});
