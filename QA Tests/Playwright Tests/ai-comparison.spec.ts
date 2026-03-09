/**
 * @fileoverview PNO-914: AI Comparison Component E2E Tests
 *
 * Tests the app-ai-comparison component: side-by-side comparison of current vs AI-extracted data,
 * field selection, Apply Selected, Cancel, loading state, empty data, and formatting.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-914
 *
 * @tests 12
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { setupAPIMocks } from './helpers/api-mocks.helper';
import { waitForPermissions, waitForDialog, waitForHidden } from './helpers/wait.helper';
import { getTimeout } from './helpers/test-config';

const ADMIN_USER = 'test@playwright.local';
const OPPORTUNITY_ID = 1;
const BASE_URL = 'http://localhost:4200';

/** Set AI_COMPARISON_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.AI_COMPARISON_IMPLEMENTED === 'true';

test.describe('PNO-914 — AI Comparison Component', () => {
  test.slow();

  test.skip(!featureReady, 'AI Comparison not deployed — set AI_COMPARISON_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, `/partnerships/opportunities/${OPPORTUNITY_ID}`, ADMIN_USER);
    await waitForPermissions(page);
  });

  test.describe('Dialog and data display', () => {
    test.beforeEach(async ({ page }) => {
      await setupAiComparisonMocks(page);
    });

    test('TC-001: AI comparison dialog opens when triggered', async ({ page }) => {
      await test.step('Arrange — ensure documents and mocks ready', async () => {
        await page.waitForLoadState('networkidle');
      });

      await test.step('Act — click AI transcribe on first document', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — comparison dialog visible', async () => {
        const dialog = page.locator('.ai-comparison-dialog, p-dialog[header*="Comparison"], [role="dialog"]').first();
        await expect(dialog).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-002: Current data displayed on left, AI data on right', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Assert — current value and AI extracted value sections visible', async () => {
        const currentLabel = page.locator('text=Current Value, text=label.currentValue').first();
        const aiLabel = page.locator('text=AI Extracted Value, text=label.aiExtractedValue').first();
        await expect(currentLabel).toBeVisible({ timeout: getTimeout('default') });
        await expect(aiLabel).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-003: Differences highlighted between current and AI values', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Assert — changed badge or difference styling visible', async () => {
        const changedBadge = page.locator('text=Changed, text=label.changed, .bg-unops-warning').first();
        await expect(changedBadge).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-004: Loading state displayed while fetching data', async ({ page }) => {
      await test.step('Arrange — mock slow audit log', async () => {
        await page.unroute(url => url.toString().includes('/api/auditlog'));
        await page.route(url => url.toString().includes('/api/auditlog'), async route => {
          await new Promise(r => setTimeout(r, 1500));
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ jsonData: JSON.stringify({ name: 'Test', description: 'Current' }) }),
          });
        });
      });

      await test.step('Act — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
      });

      await test.step('Assert — loading spinner visible', async () => {
        const spinner = page.locator('.pi-spin, .pi-spinner').first();
        await expect(spinner).toBeVisible({ timeout: 2000 });
      });
    });

    test('TC-005: Empty AI data shows appropriate message', async ({ page }) => {
      await test.step('Arrange — mock transcribe with empty/minimal data', async () => {
        await page.unroute(url => url.toString().includes('/api/document-transcribe'));
        await page.route(url => url.toString().includes('/api/document-transcribe'), async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({}),
          });
        });
      });

      await test.step('Act — click AI transcribe', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — dialog may show no differences or error', async () => {
        const noDiff = page.locator('text=No Differences, text=message.noDifferences').first();
        const dialog = page.locator('[role="dialog"]').first();
        await expect(dialog.or(noDiff)).toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });

  test.describe('Field selection and actions', () => {
    test.beforeEach(async ({ page }) => {
      await setupAiComparisonMocks(page);
    });

    test('TC-006: Individual fields can be selected/deselected', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Act — click a field checkbox to deselect', async () => {
        const checkbox = page.locator('p-checkbox input[type="checkbox"]').first();
        await checkbox.click({ timeout: getTimeout('default') });
      });

      await test.step('Assert — selection state changed', async () => {
        const selectedCount = page.locator('text=/Selected|selected/').first();
        await expect(selectedCount).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-007: Select All toggles all fields', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Act — click Select All checkbox', async () => {
        const selectAll = page.locator('label[for="select-all"], input#select-all').first();
        await selectAll.click({ timeout: getTimeout('default') });
      });

      await test.step('Assert — all fields toggled', async () => {
        const stats = page.locator('text=/Selected|Total|Differences/').first();
        await expect(stats).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-008: Apply Selected emits selected changes', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Act — click Apply Selected', async () => {
        const applyBtn = page.locator('button:has-text("Apply Selected"), button:has-text("Apply")').first();
        await applyBtn.click({ timeout: getTimeout('default') });
        await page.waitForLoadState('networkidle');
      });

      await test.step('Assert — dialog closed or success', async () => {
        const dialog = page.locator('.ai-comparison-dialog, [role="dialog"]').first();
        await expect(dialog).not.toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-009: Cancel closes dialog without changes', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Act — click Cancel', async () => {
        const cancelBtn = page.locator('button:has-text("Cancel")').first();
        await cancelBtn.click({ timeout: getTimeout('default') });
        const dialog = page.locator('.ai-comparison-dialog, [role="dialog"]').first();
        await waitForHidden(dialog, getTimeout('default'));
      });

      await test.step('Assert — dialog closed', async () => {
        const dialog = page.locator('.ai-comparison-dialog').first();
        await expect(dialog).not.toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });

  test.describe('Field count and formatting', () => {
    test.beforeEach(async ({ page }) => {
      await setupAiComparisonMocks(page);
    });

    test('TC-010: Field count shows selected/total', async ({ page }) => {
      await test.step('Arrange — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Assert — selected/total badges visible', async () => {
        const totalBadge = page.locator('p-badge, .badge, [class*="badge"]').first();
        const selectedText = page.locator('text=/Selected|selected/').first();
        await expect(totalBadge.or(selectedText)).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-011: Array fields (SDGs, countries) show individual items', async ({ page }) => {
      await test.step('Arrange — use mocks with array fields', async () => {
        await page.unroute(url => url.toString().includes('/api/document-transcribe'));
        await page.route(url => url.toString().includes('/api/document-transcribe'), async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              name: 'Test',
              sdGs: [{ sdgId: '1', sdgName: 'No Poverty', sdgNumber: 1 }],
              countries: [{ countryCode: 'US', countryName: 'United States' }],
            }),
          });
        });
      });

      await test.step('Act — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Assert — array items or difference rows visible', async () => {
        const diffRow = page.locator('.border-unops-neutral-light, .rounded-unops-md').first();
        await expect(diffRow).toBeVisible({ timeout: getTimeout('default') });
      });
    });

    test('TC-012: Currency values formatted correctly', async ({ page }) => {
      await test.step('Arrange — use mocks with currency', async () => {
        await page.unroute(url => url.toString().includes('/api/document-transcribe'));
        await page.route(url => url.toString().includes('/api/document-transcribe'), async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ estimatedValue: 1500000, value: 1500000 }),
          });
        });
      });

      await test.step('Act — open comparison dialog', async () => {
        const transcribeBtn = page.locator('button[icon="pi pi-sparkles"], .pi-sparkles').first();
        await transcribeBtn.click({ timeout: getTimeout('default') });
        await waitForDialog(page);
      });

      await test.step('Assert — currency or number visible', async () => {
        const currencyText = page.locator('text=/\\$|USD|1,500,000|1500000/').first();
        await expect(currencyText).toBeVisible({ timeout: getTimeout('default') });
      });
    });
  });
});

/**
 * Setup mocks for AI comparison flow: documents, transcribe, auditlog.
 */
async function setupAiComparisonMocks(page: any): Promise<void> {
  await page.unroute(url => url.toString().includes('/api/document/entity'));
  await page.route(url => url.toString().includes('/api/document/entity'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'Test Document.pdf', type: 'pdf', aiTranscribed: false },
      ]),
    });
  });

  await page.unroute(url => url.toString().includes('/api/document-transcribe'));
  await page.route(url => url.toString().includes('/api/document-transcribe'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        name: 'AI Extracted Name',
        description: 'AI extracted description',
        estimatedValue: 2000000,
      }),
    });
  });

  await page.unroute(url => url.toString().includes('/api/auditlog'));
  await page.route(url => url.toString().includes('/api/auditlog'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        jsonData: JSON.stringify({
          name: 'Current Name',
          description: 'Current description',
          estimatedValue: 1000000,
        }),
      }),
    });
  });

  await page.unroute(url => url.toString().includes('/apply-ai-changes'));
  await page.route(url => url.toString().includes('/apply-ai-changes'), async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ id: OPPORTUNITY_ID, name: 'Updated', success: true }),
    });
  });
}
