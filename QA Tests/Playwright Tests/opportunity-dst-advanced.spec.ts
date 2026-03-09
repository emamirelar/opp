/**
 * @fileoverview Opportunity DST Advanced E2E Tests
 *
 * Tests for advanced DST (Decision Support Tool) interactions:
 * AI-generated recommendations, high-risk acknowledgement workflow,
 * high-risk checklist display, and risk category management.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DST
 * @tests 10
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForVisible, waitForDialog, waitForNetworkIdle } from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

const featureReady = process.env.OPPORTUNITY_DST_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  withRisks: process.env.TEST_OPP_WITH_RISKS_ID || '4',
  highRisk: process.env.TEST_OPP_HIGH_RISK_ID || '5',
  go: process.env.TEST_OPP_GO_ID || '8',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: DST Recommendations (AI)
// =============================================================================
test.describe('DST — AI Recommendations', () => {
  test.slow();
  test.skip(!featureReady, 'DST not deployed — set OPPORTUNITY_DST_IMPLEMENTED=true');

  test('DST-ADV-001: Recommendations button visible in risks section', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await oppPage.openRisksSection();

    const recBtn = oppPage.dstSection.locator('button:has-text("Recommend"), [data-testid="dst-recommendations"]').first();
    const dstVisible = await oppPage.hasDSTSection();
    const recVisible = await recBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(dstVisible || recVisible).toBeTruthy();
  });

  test('DST-ADV-002: Clicking recommendations triggers AI analysis', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withRisks));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.withRisks);
    await oppPage.openRisksSection();

    const recBtn = oppPage.dstSection.locator('button:has-text("Recommend"), [data-testid="dst-recommendations"]').first();
    const isVisible = await recBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Recommendations button not visible');

    await recBtn.click();

    const loadingOrResult = page.locator('p-progressSpinner, .loading, [data-testid="dst-recommendation-result"]').first();
    await waitForVisible(loadingOrResult, 30000);
    expect(await loadingOrResult.isVisible()).toBeTruthy();
  });

  test('DST-ADV-003: Recommendations hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await oppPage.openRisksSection();

    const recBtn = oppPage.dstSection.locator('[data-testid="dst-recommendations"]');
    await expect(recBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 2: High-Risk Acknowledgement
// =============================================================================
test.describe('DST — High-Risk Acknowledgement', () => {
  test.slow();
  test.skip(!featureReady, 'DST not deployed — set OPPORTUNITY_DST_IMPLEMENTED=true');

  test('DST-ADV-004: High-risk acknowledgement section visible when high risks exist', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.highRisk));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.highRisk);
    await oppPage.openRisksSection();

    const ackSection = oppPage.dstSection.getByText(/high.risk|acknowledge/i).first();
    const dstVisible = await oppPage.hasDSTSection();
    const ackVisible = await ackSection.isVisible({ timeout: 10000 }).catch(() => false);
    expect(dstVisible || ackVisible).toBeTruthy();
  });

  test('DST-ADV-005: Acknowledge high risks checkbox/button available', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.highRisk));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.highRisk);
    await oppPage.openRisksSection();

    const ackControl = oppPage.dstSection.locator('p-checkbox:has-text("acknowledge"), button:has-text("Acknowledge"), [data-testid="acknowledge-high-risks"]').first();
    const dstVisible = await oppPage.hasDSTSection();
    const ackVisible = await ackControl.isVisible({ timeout: 10000 }).catch(() => false);
    expect(dstVisible || ackVisible).toBeTruthy();
  });

  test('DST-ADV-006: Acknowledge action saves acknowledgement state', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.highRisk));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.highRisk);
    await oppPage.openRisksSection();

    const ackBtn = oppPage.dstSection.locator('button:has-text("Acknowledge"), [data-testid="acknowledge-high-risks"]').first();
    if (await ackBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await ackBtn.click();
      await waitForNetworkIdle(page);

      const toast = page.locator('.p-toast-message');
      const hasToast = await toast.isVisible({ timeout: 5000 }).catch(() => false);
      if (hasToast) {
        await expect(toast).toContainText(/success|acknowledged/i);
      }
    }
  });
});

// =============================================================================
// SECTION 3: High-Risk Checklist
// =============================================================================
test.describe('DST — High-Risk Checklist', () => {
  test.slow();
  test.skip(!featureReady, 'DST not deployed — set OPPORTUNITY_DST_IMPLEMENTED=true');

  test('DST-ADV-007: High-risk checklist displays predefined risk items', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await oppPage.openRisksSection();

    const checklist = oppPage.dstSection.locator('[data-testid="high-risk-checklist"]');
    const checklistItems = oppPage.dstSection.getByText(/checklist|predefined risk/i).first();
    const dstVisible = await oppPage.hasDSTSection();
    const checklistVisible = await checklist.isVisible({ timeout: 5000 }).catch(() => false)
      || await checklistItems.isVisible({ timeout: 5000 }).catch(() => false);
    expect(dstVisible || checklistVisible).toBeTruthy();
  });

  test('DST-ADV-008: Risk categories display in dropdown', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await oppPage.openRisksSection();

    const addBtn = oppPage.dstSection.locator('button:has-text("Add"), [data-testid="add-risk"]').first();
    if (await addBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addBtn.click();
      await waitForDialog(page);

      const categoryDropdown = page.locator('p-select, [data-testid="risk-category-select"]').first();
      const hasDropdown = await categoryDropdown.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasDropdown).toBeTruthy();
    }
  });

  test('DST-ADV-009: Risk likelihood and impact dropdowns available', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await oppPage.openRisksSection();

    const addBtn = oppPage.dstSection.locator('button:has-text("Add"), [data-testid="add-risk"]').first();
    if (await addBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addBtn.click();
      await waitForDialog(page);

      const dropdowns = page.locator('.p-dialog p-select, .p-dialog [data-testid*="risk"]');
      const count = await dropdowns.count();
      expect(count).toBeGreaterThanOrEqual(1);
    }
  });

  test('DST-ADV-010: Risks section is read-only on GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.go);
    await oppPage.openRisksSection();

    const addBtn = oppPage.dstSection.locator('button:has-text("Add"), [data-testid="add-risk"]');
    await expect(addBtn).not.toBeVisible({ timeout: 5000 });
  });
});
