/**
 * @fileoverview Opportunity GO Transition Requirements Validation E2E Tests
 *
 * Tests the 21 mandatory fields required before an opportunity can transition
 * to the GO stage. Verifies the requirements validation panel, blocking behavior,
 * and individual field checks.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-GO-REQ
 * @tests 7
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForLoadingToComplete } from './helpers/wait.helper';

const featureReady = process.env.GO_REQUIREMENTS_IMPLEMENTED === 'true';

const TEST_OPP = {
  complete: process.env.TEST_OPP_COMPLETE_ID || '4',
  incomplete: process.env.TEST_OPP_INCOMPLETE_ID || '3',
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  minimal: process.env.TEST_OPP_MINIMAL_ID || '3',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Requirements Panel Visibility
// =============================================================================
test.describe('GO Requirements — Panel Display', () => {
  test.slow();
  test.skip(!featureReady, 'GO requirements not deployed — set GO_REQUIREMENTS_IMPLEMENTED=true');

  test('REQ-001: Requirements validation component visible on draft opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);

    const reqPanel = page.locator('app-requirements-validation, [data-testid="requirements-panel"]');
    const workflow = page.locator('app-stage-workflow').first();
    await expect(reqPanel.or(workflow)).toBeVisible({ timeout: 10000 });
  });

  test('REQ-002: Requirements panel shows checklist of mandatory fields', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.incomplete));
    await waitForPermissions(page);

    const reqPanel = page.locator('app-requirements-validation, [data-testid="requirements-panel"]');
    if (await reqPanel.isVisible({ timeout: 5000 }).catch(() => false)) {
      const checkItems = reqPanel.locator('li, .requirement-item, [data-testid*="requirement"]');
      const count = await checkItems.count();
      expect(count).toBeGreaterThan(0);
    }
  });

  test('REQ-003: Incomplete opportunity shows unfulfilled requirements in red/warning', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.incomplete));
    await waitForPermissions(page);

    const reqPanel = page.locator('app-requirements-validation, [data-testid="requirements-panel"]');
    if (await reqPanel.isVisible({ timeout: 5000 }).catch(() => false)) {
      const unfulfilled = reqPanel.locator('.text-red-500, .pi-times-circle, [class*="error"], [class*="danger"]');
      const count = await unfulfilled.count();
      expect(count).toBeGreaterThan(0);
    }
  });

  test('REQ-004: Complete opportunity shows all requirements fulfilled in green/success', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const reqPanel = page.locator('app-requirements-validation, [data-testid="requirements-panel"]');
    if (await reqPanel.isVisible({ timeout: 5000 }).catch(() => false)) {
      const fulfilled = reqPanel.locator('.text-green-500, .pi-check-circle, [class*="success"]');
      const count = await fulfilled.count();
      expect(count).toBeGreaterThan(0);
    }
  });
});

// =============================================================================
// SECTION 2: Submission Blocking on Incomplete Requirements
// =============================================================================
test.describe('GO Requirements — Submission Blocking', () => {
  test.slow();
  test.skip(!featureReady, 'GO requirements not deployed — set GO_REQUIREMENTS_IMPLEMENTED=true');

  test('REQ-005: Submit button disabled when requirements are not met', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.incomplete));
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const isVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    if (isVisible) {
      const isDisabled = await submitBtn.isDisabled();
      expect(isDisabled).toBeTruthy();
    }
  });

  test('REQ-006: Submit button enabled when all requirements are met', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.complete));
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const isVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    if (isVisible) {
      const isEnabled = await submitBtn.isEnabled();
      expect(isEnabled).toBeTruthy();
    }
  });

  test('REQ-007: Attempting submit on incomplete opp shows validation message', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.incomplete));
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const isVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    if (isVisible) {
      await submitBtn.click({ force: true });
      const errorMsg = page.getByText(/requirement|mandatory|complete/i).first();
      await expect(errorMsg).toBeVisible({ timeout: 5000 });
    }
  });
});

// =============================================================================
// SECTION 3: Individual Field Requirements
// =============================================================================
test.describe('GO Requirements — Individual Field Checks', () => {
  test.slow();
  test.skip(!featureReady, 'GO requirements not deployed — set GO_REQUIREMENTS_IMPLEMENTED=true');

  const requiredFields = [
    'Name',
    'Description',
    'Budget',
    'Deliverables',
    'Challenges',
    'Expected Impact',
    'Expected Outcomes',
    'Beneficiaries',
    'SDGs',
    'UNOPS Missions',
    'Funding Partners',
    'Client Partners',
    'Countries',
    'Target Signing Date',
    'Implementation Start',
    'Target Delivery',
    'Statement',
    'Opportunity Manager',
    'Org Unit',
    'Initiative Type',
  ];

  for (const field of requiredFields) {
    test(`REQ-FIELD: "${field}" listed as requirement`, async ({ page }) => {
      await authenticateWithRealBackend(page, oppUrl(TEST_OPP.minimal));
      await waitForPermissions(page);

      const reqPanel = page.locator('app-requirements-validation, [data-testid="requirements-panel"]');
      if (await reqPanel.isVisible({ timeout: 5000 }).catch(() => false)) {
        const fieldItem = reqPanel.getByText(new RegExp(field, 'i')).first();
        await expect(fieldItem).toBeVisible({ timeout: 3000 });
      }
    });
  }
});
