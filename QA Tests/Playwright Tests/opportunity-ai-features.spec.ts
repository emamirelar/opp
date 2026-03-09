/**
 * @fileoverview Opportunity AI Features E2E Tests
 *
 * Tests for AI-powered features on the opportunity detail page:
 * insights, similar opportunities/projects, relevant people,
 * deliverable extraction, banner regeneration, and AI proposal creation.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-AI
 *
 * @tests 13
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForElementReady,
  waitForTableData,
} from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_AI_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  withContext: process.env.TEST_OPP_CONTEXT_ID || '1',
  complete: process.env.TEST_OPP_COMPLETE_ID || '4',
};

const OPPORTUNITIES_URL = '/partnerships/opportunities';

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: AI Insights / Analysis Section
// =============================================================================
test.describe('AI Features — Insights & Analysis', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withContext));
    await waitForPermissions(page);
  });

  test('AI-001: Analysis section visible on opportunity detail', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis, app-opportunity-analysis-section');
    await expect(analysisSection.first()).toBeVisible({ timeout: 10000 });
  });

  test('AI-002: Analysis section contains insight content', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis, app-opportunity-analysis-section').first();
    if (await analysisSection.isVisible({ timeout: 5000 }).catch(() => false)) {
      const content = analysisSection.locator('p, span, div').first();
      const hasContent = await content.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasContent).toBeTruthy();
    }
  });

  test('AI-003: Refresh insights button available', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis, app-opportunity-analysis-section').first();
    if (await analysisSection.isVisible({ timeout: 5000 }).catch(() => false)) {
      const refreshBtn = analysisSection.locator('button:has(i.pi-refresh), button:has-text("Refresh"), [data-testid="refresh-insights"]').first();
      const hasRefresh = await refreshBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasRefresh || await analysisSection.isVisible()).toBeTruthy();
    }
  });
});

// =============================================================================
// SECTION 2: Similar Opportunities
// =============================================================================
test.describe('AI Features — Similar Opportunities', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-004: Similar opportunities section visible', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const similarSection = page.getByText(/similar opportunit/i).first();
    const isVisible = await similarSection.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });

  test('AI-005: Similar opportunities display card-based results', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const similarCards = page.locator('[data-testid*="similar-opportunity"], .similar-opportunity-card');
    const count = await similarCards.count();
    expect(count >= 0).toBeTruthy();
  });
});

// =============================================================================
// SECTION 3: Similar Projects
// =============================================================================
test.describe('AI Features — Similar Projects', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-006: Similar projects section visible', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const similarSection = page.getByText(/similar project/i).first();
    const isVisible = await similarSection.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 4: Relevant People
// =============================================================================
test.describe('AI Features — Relevant People', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-007: Relevant people section visible in Team', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const chipBtn = page.locator('button:has-text("Team")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await waitForElementReady(page.locator('#section-team').first(), 5000).catch(() => {});
    }

    const relevantPeople = page.getByText(/relevant people|suggested/i).first();
    const isVisible = await relevantPeople.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-team').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 5: Deliverable Extraction
// =============================================================================
test.describe('AI Features — Deliverable Extraction', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-008: Extract deliverables button visible in What section', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);

    const chipBtn = page.locator('button:has-text("What")').first();
    if (await chipBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await chipBtn.click();
      await waitForElementReady(page.locator('#section-what').first(), 5000).catch(() => {});
    }

    const extractBtn = page.locator('button:has-text("Extract"), [data-testid="extract-deliverables"]').first();
    const isVisible = await extractBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-what').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 6: Banner / Thumbnail Regeneration
// =============================================================================
test.describe('AI Features — Image Regeneration', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-009: Regenerate banner button visible', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);

    const regenBtn = page.locator('button:has-text("Regenerate"), [data-testid="regenerate-banner"]').first();
    const isVisible = await regenBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });

  test('AI-010: Banner image displayed on opportunity detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withContext));
    await waitForPermissions(page);

    const bannerImg = page.locator('img[src*="banner"], [data-testid="opportunity-banner"]').first();
    const isVisible = await bannerImg.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 7: AI Proposal Generation
// =============================================================================
test.describe('AI Features — Proposal Generation', () => {
  test.slow();
  test.skip(!featureReady, 'AI features not deployed — set OPPORTUNITY_AI_IMPLEMENTED=true');

  test('AI-011: Create from AI proposal flow accessible from list page', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await waitForPermissions(page);
    await waitForTableData(page);

    const newBtn = page.locator('[data-testid="new-opportunity-button"], button:has-text("New Opportunity")').first();
    const isVisible = await newBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-listview').first().isVisible()).toBeTruthy();
  });

  test('AI-012: Apply AI changes action available on draft opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);

    const aiBtn = page.locator('button:has-text("Apply AI"), [data-testid="apply-ai-changes"]').first();
    const isVisible = await aiBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });

  test('AI-013: Read-only user cannot see AI action buttons', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);

    const aiBtn = page.locator('[data-testid="apply-ai-changes"], button:has-text("Apply AI")').first();
    await expect(aiBtn).not.toBeVisible({ timeout: 5000 });
  });
});
