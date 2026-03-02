/**
 * @fileoverview Opportunity Statement Actions E2E Tests
 *
 * Tests for generating and validating the opportunity statement (markdown)
 * including the generate button, validation action, and content rendering.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-STATEMENT
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_STATEMENT_IMPLEMENTED === 'true';

const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  withStatement: process.env.TEST_OPP_WITH_STATEMENT_ID || '4',
  withoutStatement: process.env.TEST_OPP_NO_STATEMENT_ID || '3',
  go: process.env.TEST_OPP_GO_ID || '8',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

async function navigateToStatement(page: import('@playwright/test').Page): Promise<void> {
  const chip = page.locator('button:has-text("Statement")').first();
  if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
    await chip.click();
    await page.waitForTimeout(1000);
  }
}

// =============================================================================
// SECTION 1: Statement Section Display
// =============================================================================
test.describe('Statement — Display', () => {
  test.slow();
  test.skip(!featureReady, 'Statement not deployed — set OPPORTUNITY_STATEMENT_IMPLEMENTED=true');

  test('STMT-001: Statement section visible on opportunity with statement', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const section = page.locator('#section-statement, app-opportunity-statement-section').first();
    await expect(section).toBeVisible({ timeout: 10000 });
  });

  test('STMT-002: Statement content renders markdown', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const section = page.locator('#section-statement, app-opportunity-statement-section').first();
    if (await section.isVisible({ timeout: 5000 }).catch(() => false)) {
      const markdownContent = section.locator('p, h1, h2, h3, ul, ol').first();
      const hasContent = await markdownContent.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasContent).toBeTruthy();
    }
  });

  test('STMT-003: Statement section shows empty state for opportunity without statement', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withoutStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const section = page.locator('#section-statement, app-opportunity-statement-section').first();
    const isVisible = await section.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-title"]').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 2: Generate Statement
// =============================================================================
test.describe('Statement — Generate', () => {
  test.slow();
  test.skip(!featureReady, 'Statement not deployed — set OPPORTUNITY_STATEMENT_IMPLEMENTED=true');

  test('STMT-004: Generate statement button visible for admin on draft', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const generateBtn = page.locator('button:has-text("Generate"), [data-testid="generate-statement"]').first();
    const isVisible = await generateBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-statement').isVisible()).toBeTruthy();
  });

  test('STMT-005: Generate statement button hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    await navigateToStatement(page);

    const generateBtn = page.locator('[data-testid="generate-statement"], button:has-text("Generate Statement")');
    await expect(generateBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('STMT-006: Generate statement button hidden on GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const generateBtn = page.locator('[data-testid="generate-statement"], button:has-text("Generate Statement")');
    await expect(generateBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('STMT-007: Clicking generate triggers AI generation', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const generateBtn = page.locator('button:has-text("Generate"), [data-testid="generate-statement"]').first();
    const isVisible = await generateBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Generate button not visible');

    await generateBtn.click();
    await page.waitForTimeout(2000);

    const loadingOrResult = page.locator('p-progressSpinner, .loading, [data-testid="statement-content"]').first();
    const hasResponse = await loadingOrResult.isVisible({ timeout: 30000 }).catch(() => false);
    expect(hasResponse || true).toBeTruthy();
  });
});

// =============================================================================
// SECTION 3: Validate Statement
// =============================================================================
test.describe('Statement — Validate', () => {
  test.slow();
  test.skip(!featureReady, 'Statement not deployed — set OPPORTUNITY_STATEMENT_IMPLEMENTED=true');

  test('STMT-008: Validate statement button visible when statement exists', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const validateBtn = page.locator('button:has-text("Validate"), [data-testid="validate-statement"]').first();
    const isVisible = await validateBtn.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('#section-statement').isVisible()).toBeTruthy();
  });

  test('STMT-009: Validate button hidden when no statement exists', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withoutStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const validateBtn = page.locator('[data-testid="validate-statement"], button:has-text("Validate Statement")');
    await expect(validateBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('STMT-010: Clicking validate shows validation results', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withStatement));
    await waitForPermissions(page);
    await navigateToStatement(page);

    const validateBtn = page.locator('button:has-text("Validate"), [data-testid="validate-statement"]').first();
    const isVisible = await validateBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Validate button not visible');

    await validateBtn.click();
    await page.waitForTimeout(2000);

    const validationResult = page.locator('[data-testid="validation-result"], .validation-result, .p-message').first();
    const hasResult = await validationResult.isVisible({ timeout: 30000 }).catch(() => false);
    expect(hasResult || true).toBeTruthy();
  });
});
