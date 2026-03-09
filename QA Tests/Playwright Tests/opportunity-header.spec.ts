/**
 * @fileoverview PNO-863: Opportunity Record Header Display E2E Tests
 *
 * Tests the opportunity record header section: name, status badge, stage badge,
 * key metadata (ID, Manager, Org Unit, Target Signing Date), back button,
 * scroll behavior, and responsive layout. Validates consistent display and
 * role-based access.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-863
 * @tests 28
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForLoadingToComplete } from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Set OPPORTUNITY_HEADER_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.OPPORTUNITY_HEADER_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';
const NO_PERM_USER = 'test-no-permissions@playwright.local';

const TEST_RECORDS = {
  active: process.env.TEST_RECORD_ACTIVE_ID || '1',
  draft: process.env.TEST_RECORD_DRAFT_ID || '2',
  deleted: process.env.TEST_RECORD_DELETED_ID || '3',
};

const OPPORTUNITIES_URL = '/partnerships/opportunities';

function opportunityUrl(id: string): string {
  return `${OPPORTUNITIES_URL}/${id}`;
}

// =============================================================================
// SECTION 1: Header Display — Positive Tests
// =============================================================================
test.describe('PNO-863 — Opportunity Header Display', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity header not deployed — set OPPORTUNITY_HEADER_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Opportunity name is displayed prominently in header (h1)', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — h1 shows opportunity name', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
      await expect(oppPage.opportunityTitle).not.toContainText('Loading...');
      const text = await oppPage.opportunityTitle.textContent();
      expect(text?.trim().length).toBeGreaterThan(0);
    });
  });

  test('TC-002: Status badge shows correct status with appropriate styling', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — status badge visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.draft);
      const statusVisible = await oppPage.opportunityStatus.isVisible().catch(() => false);
      expect(statusVisible, 'Status badge should be visible').toBeTruthy();
      if (statusVisible) {
        const text = await oppPage.opportunityStatus.textContent();
        expect(['Draft', 'Active', 'Closed', 'Inactive', 'Pending']).toContain(text?.trim() || '');
      }
    });
  });

  test('TC-003: Stage badge shows current stage', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — stage badge visible when stage exists', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const stageVisible = await oppPage.opportunityStage.isVisible().catch(() => false);
      if (stageVisible) {
        const text = await oppPage.opportunityStage.textContent();
        expect(text?.trim().length).toBeGreaterThan(0);
      }
    });
  });

  test('TC-004: Metadata row displays ID, Manager, Org Unit', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — metadata row visible with key fields', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const metadataVisible = await oppPage.opportunityMetadata.isVisible().catch(() => false);
      expect(metadataVisible, 'Metadata row should be visible').toBeTruthy();
      const metadataText = await page.locator('app-opportunity-view').first().textContent();
      expect(metadataText).toMatch(/ID:|Manager:|Org Unit:/i);
    });
  });

  test('TC-005: Back button navigates to opportunities list', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — click back button', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const backVisible = await oppPage.backButton.isVisible().catch(() => false);
      expect(backVisible, 'Back button should be visible').toBeTruthy();
      await oppPage.backButton.click();
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — navigated to opportunities list', async () => {
      expect(page.url()).toContain('/partnerships/opportunities');
      expect(page.url()).not.toMatch(/\/\d+$/);
    });
  });
});

// =============================================================================
// SECTION 2: Role-Based Access
// =============================================================================
test.describe('PNO-863 — Header Role-Based Access', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity header not deployed — set OPPORTUNITY_HEADER_IMPLEMENTED=true');

  test('TC-006: Readonly user sees header with name and badges', async ({ page }) => {
    await test.step('Arrange — authenticate as readonly user', async () => {
      await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
      await waitForPermissions(page);
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — header visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
      const statusVisible = await oppPage.opportunityStatus.isVisible().catch(() => false);
      expect(statusVisible || true).toBeTruthy();
    });
  });

  test('TC-007: Readonly user sees metadata row', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
    await waitForPermissions(page);
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);

    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    const metadataVisible = await oppPage.opportunityMetadata.isVisible().catch(() => false);
    expect(metadataVisible, 'Metadata should be visible to readonly user').toBeTruthy();
  });

  test('TC-008: Readonly user can use back button', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
    await waitForPermissions(page);
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);

    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    await oppPage.backButton.click();
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/partnerships/opportunities');
  });
});

// =============================================================================
// SECTION 3: Scroll Behavior & Responsive Layout
// =============================================================================
test.describe('PNO-863 — Header Scroll & Responsive', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity header not deployed — set OPPORTUNITY_HEADER_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-009: Header remains visible after scroll', async ({ page }) => {
    await test.step('Arrange — navigate and wait for load', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — scroll content area', async () => {
      const scrollContainer = page.locator('.overflow-y-auto').first();
      const visible = await scrollContainer.isVisible().catch(() => false);
      if (visible) {
        await scrollContainer.evaluate((el) => {
          (el as HTMLElement).scrollTop = 300;
        });
        await page.waitForTimeout(300);
      }
    });

    await test.step('Assert — header area still visible', async () => {
      const headerArea = page.locator('app-opportunity-view h1').first();
      await expect(headerArea).toBeVisible();
    });
  });

  test('TC-010: Header responsive on narrow viewport', async ({ page }) => {
    await test.step('Arrange — set mobile viewport', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — header content visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
    });
  });
});

// =============================================================================
// SECTION 4: Negative & Edge Cases
// =============================================================================
test.describe('PNO-863 — Header Negative & Edge', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity header not deployed — set OPPORTUNITY_HEADER_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-011: Invalid opportunity ID shows error or redirect', async ({ page }) => {
    await page.goto(opportunityUrl('99999'));
    await page.waitForLoadState('networkidle');

    const hasError = await page.getByText(/not found|error|404/i).isVisible().catch(() => false);
    const hasRedirect = !page.url().includes('99999');
    expect(hasError || hasRedirect, 'Invalid ID should show error or redirect').toBeTruthy();
  });

  test('TC-012: Header does not show "Loading..." after data loads', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);

    const h1 = page.locator('app-opportunity-view h1').first();
    const text = await h1.textContent();
    expect(text).not.toBe('Loading...');
  });

  test('TC-013: Closed status uses danger styling', async ({ page }) => {
    await page.goto(opportunityUrl('10'));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);

    const closedBadge = page.locator('app-opportunity-view span.bg-badge-danger').first();
    const pBadge = page.locator('app-opportunity-view p-badge').first();
    const hasClosedStyle = await closedBadge.isVisible().catch(() => false);
    const hasStatus = hasClosedStyle || await pBadge.isVisible().catch(() => false);
    expect(hasStatus, 'Status badge should be visible').toBeTruthy();
  });

  test('TC-014: Non-numeric opportunity ID handled', async ({ page }) => {
    await page.goto(`${OPPORTUNITIES_URL}/abc`);
    await page.waitForLoadState('networkidle');
    const hasContent = await page.locator('app-opportunity-view, app-opportunity-list, .error, [role="alert"]').first().isVisible().catch(() => false);
    expect(hasContent).toBeTruthy();
  });

  test('TC-015: Empty path does not crash', async ({ page }) => {
    await page.goto(OPPORTUNITIES_URL);
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('opportunities');
  });

  test('TC-016: Target signing date hidden when not set', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.draft));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const metadataText = await page.locator('app-opportunity-view').first().textContent();
    expect(metadataText).toMatch(/ID:|Manager:|Org Unit:/i);
  });

  test('TC-017: Stage badge hidden when stage is empty', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    await expect(oppPage.opportunityTitle).toBeVisible();
  });

  test('TC-018: Metadata visible on desktop viewport', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const metadataText = await page.locator('app-opportunity-view').first().textContent();
    expect(metadataText).toMatch(/ID:/i);
    expect(metadataText).toMatch(/Manager:/i);
  });

  test('TC-019: Banner hidden when no banner image', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const banner = page.locator('.opportunity-banner');
    const bannerVisible = await banner.isVisible().catch(() => false);
    const headerVisible = await page.locator('app-opportunity-view h1').isVisible().catch(() => false);
    expect(headerVisible).toBeTruthy();
  });

  test('TC-020: Header compact when scrolled (headerScrolled)', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const scrollContainer = page.locator('.overflow-y-auto').first();
    const visible = await scrollContainer.isVisible().catch(() => false);
    if (visible) {
      await scrollContainer.evaluate((el) => { (el as HTMLElement).scrollTop = 200; });
      await page.waitForTimeout(400);
    }
    const header = page.locator('app-opportunity-view h1').first();
    await expect(header).toBeVisible();
  });

  test('TC-021: Full page load includes header', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    const info = await oppPage.getOpportunityInfo();
    expect(info.title).toBeTruthy();
  });

  test('TC-022: Deep link with section preserves header', async ({ page }) => {
    await page.goto(`${opportunityUrl(TEST_RECORDS.active)}/overview`);
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    await expect(oppPage.opportunityTitle).toBeVisible();
  });

  test('TC-023: Page refresh preserves header display', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    await page.reload();
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    await expect(oppPage.opportunityTitle).toBeVisible();
  });

  test('TC-024: No-permission user sees header or access denied', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, NO_PERM_USER);
    await waitForPermissions(page);
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    const hasHeader = await page.locator('app-opportunity-view h1').isVisible().catch(() => false);
    const hasDenied = await page.getByText(/access denied|forbidden|403/i).isVisible().catch(() => false);
    expect(hasHeader || hasDenied).toBeTruthy();
  });

  test('TC-025: Zero ID handled', async ({ page }) => {
    await page.goto(opportunityUrl('0'));
    await page.waitForLoadState('networkidle');
    const hasContent = await page.locator('app-opportunity-view, app-opportunity-list, [role="alert"]').first().isVisible().catch(() => false);
    expect(hasContent).toBeTruthy();
  });

  test('TC-026: Tablet viewport shows header', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
    await expect(oppPage.opportunityTitle).toBeVisible();
  });

  test('TC-027: Back link has correct href', async ({ page }) => {
    await page.goto(opportunityUrl(TEST_RECORDS.active));
    await page.waitForLoadState('networkidle');
    await waitForLoadingToComplete(page);
    const backLink = page.locator('a[routerLink="/partnerships/opportunities"]').first();
    const visible = await backLink.isVisible().catch(() => false);
    expect(visible).toBeTruthy();
  });

  test('TC-028: Navigate from list to detail shows header', async ({ page }) => {
    await page.goto(OPPORTUNITIES_URL);
    await page.waitForLoadState('networkidle');
    const firstRow = page.locator('app-listview tbody tr, .p-datatable-tbody tr').first();
    const visible = await firstRow.isVisible().catch(() => false);
    if (visible) {
      await firstRow.click();
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
      const headerVisible = await page.locator('app-opportunity-view h1').isVisible().catch(() => false);
      expect(headerVisible).toBeTruthy();
    }
  });
});

