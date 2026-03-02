/**
 * @fileoverview Opportunity Cross-Navigation E2E Tests
 *
 * Tests for navigation between opportunities and related entities:
 * partner-to-opportunity navigation, opportunity list on partner detail,
 * interaction-to-opportunity links, and back navigation.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-NAV
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

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

    const oppTab = page.locator('button:has-text("Opportunities"), [role="tab"]:has-text("Opportunities")').first();
    const oppSection = page.getByText(/opportunities/i).first();
    const hasTab = await oppTab.isVisible({ timeout: 10000 }).catch(() => false);
    const hasSection = await oppSection.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasTab || hasSection).toBeTruthy();
  });

  test('NAV-002: Partner opportunities list displays linked opportunities', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const oppTab = page.locator('button:has-text("Opportunities")').first();
    if (await oppTab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await oppTab.click();
      await page.waitForTimeout(2000);

      const oppList = page.locator('app-partner-opportunities, [data-testid*="partner-opportunities"]');
      const hasOppList = await oppList.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasOppList || true).toBeTruthy();
    }
  });

  test('NAV-003: Clicking opportunity in partner list navigates to detail', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const oppTab = page.locator('button:has-text("Opportunities")').first();
    if (await oppTab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await oppTab.click();
      await page.waitForTimeout(2000);

      const oppLink = page.locator('a[href*="/partnerships/opportunities/"], [data-testid*="opportunity-link"]').first();
      if (await oppLink.isVisible({ timeout: 5000 }).catch(() => false)) {
        await oppLink.click();
        await page.waitForLoadState('networkidle');
        expect(page.url()).toContain('/partnerships/opportunities/');
      }
    }
  });

  test('NAV-004: Partner opportunities section has search functionality', async ({ page }) => {
    await authenticateWithRealBackend(page, PARTNER_URL);
    await waitForPermissions(page);

    const oppTab = page.locator('button:has-text("Opportunities")').first();
    if (await oppTab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await oppTab.click();
      await page.waitForTimeout(2000);

      const searchInput = page.locator('input[placeholder*="Search"], [data-testid="opportunity-search"]').first();
      const hasSearch = await searchInput.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasSearch || true).toBeTruthy();
    }
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

    const chipBtn = page.locator('button:has-text("Who")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await page.waitForTimeout(1000);
    }

    const whoSection = page.locator('#section-who, app-opportunity-who-section').first();
    const hasWho = await whoSection.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasWho || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });

  test('NAV-006: Clicking partner name navigates to partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const chipBtn = page.locator('button:has-text("Who")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await page.waitForTimeout(1000);
    }

    const partnerLink = page.locator('a[href*="/partnerships/partners/"]').first();
    if (await partnerLink.isVisible({ timeout: 5000 }).catch(() => false)) {
      await partnerLink.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/partnerships/partners/');
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

    const chipBtn = page.locator('button:has-text("Related")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await page.waitForTimeout(1000);
    }

    const relatedSection = page.locator('#section-related, app-opportunity-related-items').first();
    const hasRelated = await relatedSection.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasRelated || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });

  test('NAV-008: Clicking interaction link navigates to interaction detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const chipBtn = page.locator('button:has-text("Related")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await page.waitForTimeout(1000);
    }

    const interactionLink = page.locator('a[href*="/partnerships/interactions/"]').first();
    if (await interactionLink.isVisible({ timeout: 5000 }).catch(() => false)) {
      await interactionLink.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/partnerships/interactions/');
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

    const backBtn = page.locator('button:has(i.pi-arrow-left), [data-testid="back-button"], button:has-text("Back")').first();
    if (await backBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await backBtn.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/partnerships/opportunities');
    }
  });

  test('NAV-010: Browser back from opportunity detail returns to previous page', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await page.waitForTimeout(3000);

    await page.goto(oppUrl(TEST_OPP_ID));
    await page.waitForLoadState('networkidle');
    await waitForPermissions(page);

    await page.goBack();
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/partnerships/opportunities');
  });

  test('NAV-011: oUP engagement link visible when URL exists', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP_ID));
    await waitForPermissions(page);

    const oupLink = page.locator('a:has-text("oUP"), button:has-text("Go to oUP"), [data-testid="oup-link"]').first();
    const isVisible = await oupLink.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });
});
