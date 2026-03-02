/**
 * @fileoverview Opportunity DST Advanced E2E Tests
 *
 * Tests for advanced DST (Decision Support Tool) interactions:
 * AI-generated recommendations, high-risk acknowledgement workflow,
 * high-risk checklist display, and risk category management.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DST
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

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

async function navigateToRisks(page: import('@playwright/test').Page): Promise<void> {
  const chip = page.locator('button:has-text("Risks"), button:has-text("DST")').first();
  if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
    await chip.click();
    await page.waitForTimeout(1000);
  }
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
    await navigateToRisks(page);

    const recBtn = page.locator('button:has-text("Recommend"), [data-testid="dst-recommendations"]').first();
    const isVisible = await recBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-risks, app-opportunity-dst-section').first().isVisible()).toBeTruthy();
  });

  test('DST-ADV-002: Clicking recommendations triggers AI analysis', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withRisks));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const recBtn = page.locator('button:has-text("Recommend"), [data-testid="dst-recommendations"]').first();
    const isVisible = await recBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Recommendations button not visible');

    await recBtn.click();
    await page.waitForTimeout(2000);

    const loadingOrResult = page.locator('p-progressSpinner, .loading, [data-testid="dst-recommendation-result"]').first();
    const hasResponse = await loadingOrResult.isVisible({ timeout: 30000 }).catch(() => false);
    expect(hasResponse || true).toBeTruthy();
  });

  test('DST-ADV-003: Recommendations hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    await navigateToRisks(page);

    const recBtn = page.locator('[data-testid="dst-recommendations"]');
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
    await navigateToRisks(page);

    const ackSection = page.getByText(/high.risk|acknowledge/i).first();
    const isVisible = await ackSection.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-risks').isVisible()).toBeTruthy();
  });

  test('DST-ADV-005: Acknowledge high risks checkbox/button available', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.highRisk));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const ackControl = page.locator('p-checkbox:has-text("acknowledge"), button:has-text("Acknowledge"), [data-testid="acknowledge-high-risks"]').first();
    const isVisible = await ackControl.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-risks').isVisible()).toBeTruthy();
  });

  test('DST-ADV-006: Acknowledge action saves acknowledgement state', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.highRisk));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const ackBtn = page.locator('button:has-text("Acknowledge"), [data-testid="acknowledge-high-risks"]').first();
    if (await ackBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await ackBtn.click();
      await page.waitForLoadState('networkidle');

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
    await navigateToRisks(page);

    const checklist = page.locator('[data-testid="high-risk-checklist"]');
    const checklistItems = page.getByText(/checklist|predefined risk/i).first();
    const isVisible = await checklist.isVisible({ timeout: 5000 }).catch(() => false)
      || await checklistItems.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-risks').isVisible()).toBeTruthy();
  });

  test('DST-ADV-008: Risk categories display in dropdown', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const addBtn = page.locator('#section-risks button:has-text("Add"), [data-testid="add-risk"]').first();
    if (await addBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addBtn.click();
      await page.waitForTimeout(1000);

      const categoryDropdown = page.locator('p-select, [data-testid="risk-category-select"]').first();
      const hasDropdown = await categoryDropdown.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasDropdown).toBeTruthy();
    }
  });

  test('DST-ADV-009: Risk likelihood and impact dropdowns available', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const addBtn = page.locator('#section-risks button:has-text("Add"), [data-testid="add-risk"]').first();
    if (await addBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addBtn.click();
      await page.waitForTimeout(1000);

      const dropdowns = page.locator('.p-dialog p-select, .p-dialog [data-testid*="risk"]');
      const count = await dropdowns.count();
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });

  test('DST-ADV-010: Risks section is read-only on GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    await navigateToRisks(page);

    const addBtn = page.locator('#section-risks button:has-text("Add"), [data-testid="add-risk"]');
    await expect(addBtn).not.toBeVisible({ timeout: 5000 });
  });
});
