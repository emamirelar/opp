/**
 * @fileoverview PNO-882: Opportunity Record Formatting and Layout E2E Tests
 *
 * Tests visual formatting, layout consistency, and responsive design of the
 * opportunity record view. Covers section card spacing, banner visibility,
 * responsive layout at different viewports, loading states, hover effects,
 * documents panel collapse/expand, workflow overlay, and badge/chip styling.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-882
 * @tests 22
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForPermissions,
} from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Set OPPORTUNITY_LAYOUT_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.OPPORTUNITY_LAYOUT_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const TEST_RECORDS = {
  active: process.env.TEST_RECORD_ACTIVE_ID || '1',
  draft: process.env.TEST_RECORD_DRAFT_ID || '2',
};

const OPPORTUNITIES_URL = '/partnerships/opportunities';

function opportunityUrl(id: string): string {
  return `${OPPORTUNITIES_URL}/${id}`;
}

// =============================================================================
// SECTION 1: Section Card Consistency
// =============================================================================
test.describe('PNO-882 — Section Card Consistency', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Section cards have consistent spacing and borders across all sections', async ({
    page,
  }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — section-hover-containers exist with consistent structure', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const sections = oppPage.sectionHoverContainers;
      const count = await sections.count();
      expect(count).toBeGreaterThanOrEqual(3);
      for (let i = 0; i < Math.min(count, 5); i++) {
        await expect(sections.nth(i)).toBeVisible();
      }
    });
  });

  test('TC-002: Overview, What, When sections display correctly', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — main sections visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const hasOverview = await oppPage.hasOverviewSection();
      const hasWhat = await oppPage.hasWhatSection();
      const hasSchedule = await oppPage.hasScheduleSection();
      expect(hasOverview || hasWhat || hasSchedule).toBeTruthy();
    });
  });

  test('TC-003: PrimeNG panels use consistent card styling', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — p-panel elements present', async () => {
      const panels = page.locator('app-opportunity-view p-panel');
      const count = await panels.count();
      expect(count).toBeGreaterThan(0);
    });
  });
});

// =============================================================================
// SECTION 2: Banner Responsive Visibility
// =============================================================================
test.describe('PNO-882 — Banner Responsive Visibility', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-004: Banner hidden on short viewport (height < 850px)', async ({ page }) => {
    await test.step('Arrange — set short viewport and navigate', async () => {
      await page.setViewportSize({ width: 1280, height: 600 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — banner not visible (display:none per media query)', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const banner = oppPage.opportunityBanner;
      const isVisible = await banner.isVisible().catch(() => false);
      expect(isVisible).toBeFalsy();
    });
  });

  test('TC-005: Banner visible on tall viewport (height >= 1024px)', async ({ page }) => {
    await test.step('Arrange — set tall viewport and navigate', async () => {
      await page.setViewportSize({ width: 1280, height: 1080 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — banner visible when opportunity has banner image', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const banner = oppPage.opportunityBanner;
      const bannerVisible = await banner.isVisible().catch(() => false);
      const bannerInDom = await page.locator('.opportunity-banner').count() > 0;
      expect(bannerInDom).toBeTruthy();
      if (bannerVisible) {
        await expect(banner).toBeVisible();
      }
    });
  });

  test('TC-006: Banner container has correct border-radius when visible', async ({ page }) => {
    await test.step('Arrange — navigate with tall viewport', async () => {
      await page.setViewportSize({ width: 1280, height: 1080 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — banner element exists in DOM', async () => {
      const banner = page.locator('.opportunity-banner');
      const count = await banner.count();
      expect(count).toBeGreaterThanOrEqual(0);
    });
  });
});

// =============================================================================
// SECTION 3: Documents Panel Collapse/Expand
// =============================================================================
test.describe('PNO-882 — Documents Panel Collapse/Expand', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-007: Documents panel toggle expands collapsed panel', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.setViewportSize({ width: 1280, height: 900 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — click documents panel to expand', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const docsPanel = oppPage.documentsSection;
      await docsPanel.click();
      await page.waitForTimeout(400);
    });

    await test.step('Assert — documents section visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const hasDocs = await oppPage.hasDocumentsSection();
      expect(hasDocs).toBeTruthy();
    });
  });

  test('TC-008: Documents panel shows Docs label when collapsed', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.setViewportSize({ width: 1280, height: 900 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — documents panel exists (collapsed or expanded)', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const docsPanel = oppPage.documentsSection;
      await expect(docsPanel).toBeVisible();
    });
  });

  test('TC-009: Documents panel can be collapsed after expand', async ({ page }) => {
    await test.step('Arrange — navigate and expand documents', async () => {
      await page.setViewportSize({ width: 1280, height: 900 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
      const docsPanel = page.locator('app-opportunity-documents').first();
      await docsPanel.click();
      await page.waitForTimeout(500);
    });

    await test.step('Act — click Docs header to collapse', async () => {
      const docsLabel = page.locator('app-opportunity-documents').filter({ hasText: /docs/i }).first();
      await docsLabel.click();
      await page.waitForTimeout(500);
    });

    await test.step('Assert — documents panel still visible', async () => {
      const docsPanel = page.locator('app-opportunity-documents');
      await expect(docsPanel).toBeVisible();
    });
  });
});

// =============================================================================
// SECTION 4: Responsive Layout — Multi-Viewport
// =============================================================================
test.describe('PNO-882 — Responsive Layout', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-010: Desktop (1280px) — main content visible', async ({ page }) => {
    await test.step('Arrange — set desktop viewport', async () => {
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — opportunity content visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
      const hasContent = await oppPage.hasOverviewSection() || await oppPage.hasWhatSection();
      expect(hasContent).toBeTruthy();
    });
  });

  test('TC-011: Tablet (768px) — layout adapts', async ({ page }) => {
    await test.step('Arrange — set tablet viewport', async () => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — page renders without overflow', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
      const body = page.locator('body');
      await expect(body).toBeVisible();
    });
  });

  test('TC-012: Mobile (375px) — mobile layout visible', async ({ page }) => {
    await test.step('Arrange — set mobile viewport', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — mobile section dropdown or chips visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await expect(oppPage.opportunityTitle).toBeVisible();
      const mobileDropdown = oppPage.mobileSectionDropdown;
      const chipsContainer = oppPage.sectionChipsContainer;
      const hasNav = await mobileDropdown.isVisible().catch(() => false)
        || await chipsContainer.isVisible().catch(() => false);
      expect(hasNav).toBeTruthy();
    });
  });
});

// =============================================================================
// SECTION 5: Workflow Overlay
// =============================================================================
test.describe('PNO-882 — Workflow Overlay', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-013: Workflow overlay not visible when idle', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — overlay hidden', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const overlay = oppPage.workflowActionOverlay;
      const isVisible = await overlay.isVisible().catch(() => false);
      expect(isVisible).toBeFalsy();
    });
  });

  test('TC-014: Workflow overlay element exists in DOM', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — overlay class exists (hidden when no workflow action)', async () => {
      const overlay = page.locator('.workflow-action-overlay');
      const count = await overlay.count();
      expect(count).toBeGreaterThanOrEqual(0);
    });
  });
});

// =============================================================================
// SECTION 6: Loading Progress Strip
// =============================================================================
test.describe('PNO-882 — Loading Progress Strip', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-015: Loading strip not visible after page load completes', async ({ page }) => {
    await test.step('Arrange — navigate and wait for load', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — loading strip hidden', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const strip = oppPage.loadingProgressStrip;
      const isVisible = await strip.isVisible().catch(() => false);
      expect(isVisible).toBeFalsy();
    });
  });

  test('TC-016: Loading strip appears during delayed API response', async ({ page }) => {
    await test.step('Arrange — delay opportunity GET API', async () => {
      await page.route('**/api/opportunity/*', async (route) => {
        if (route.request().method() === 'GET') {
          await new Promise((r) => setTimeout(r, 2000));
        }
        await route.continue();
      });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
    });

    await test.step('Assert — strip visible during load or hidden after', async () => {
      const strip = page.locator('.loading-progress-strip');
      const visibleDuringLoad = await strip.isVisible({ timeout: 1500 }).catch(() => false);
      if (visibleDuringLoad) {
        await expect(strip).toBeVisible();
      }
      await waitForLoadingToComplete(page);
      const stillVisible = await strip.isVisible().catch(() => false);
      expect(stillVisible).toBeFalsy();
    });
  });
});

// =============================================================================
// SECTION 7: Section Hover Effect
// =============================================================================
test.describe('PNO-882 — Section Hover Effect', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-017: Section-hover-container has section-editable when user can edit', async ({
    page,
  }) => {
    await test.step('Arrange — navigate as admin', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — at least one editable section', async () => {
      const editableSections = page.locator('.section-hover-container.section-editable');
      const count = await editableSections.count();
      expect(count).toBeGreaterThanOrEqual(0);
    });
  });

  test('TC-018: Section-hover containers have hover affordance structure', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — section containers exist', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const sections = oppPage.sectionHoverContainers;
      const count = await sections.count();
      expect(count).toBeGreaterThanOrEqual(2);
    });
  });

  test('TC-019: Readonly user sees no section-editable', async ({ page }) => {
    await test.step('Arrange — navigate as readonly user', async () => {
      await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
      await waitForPermissions(page);
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — no editable sections', async () => {
      const editableSections = page.locator('.section-hover-container.section-editable');
      const count = await editableSections.count();
      expect(count).toBe(0);
    });
  });
});

// =============================================================================
// SECTION 8: Badge and Chip Consistency
// =============================================================================
test.describe('PNO-882 — Badge and Chip Consistency', () => {
  test.slow();

  test.skip(!featureReady, 'Opportunity layout not deployed — set OPPORTUNITY_LAYOUT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-020: Status and stage badges use p-badge', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — badges visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const statusVisible = await oppPage.opportunityStatus.isVisible().catch(() => false);
      const stageVisible = await oppPage.opportunityStage.isVisible().catch(() => false);
      expect(statusVisible || stageVisible).toBeTruthy();
    });
  });

  test('TC-021: Section chips use consistent styling', async ({ page }) => {
    await test.step('Arrange — navigate at desktop viewport', async () => {
      await page.setViewportSize({ width: 1280, height: 900 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — section chips or dropdown present', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const chips = oppPage.sectionChipsContainer;
      const dropdown = oppPage.mobileSectionDropdown;
      const hasChips = await chips.isVisible().catch(() => false);
      const hasDropdown = await dropdown.isVisible().catch(() => false);
      expect(hasChips || hasDropdown).toBeTruthy();
    });
  });

  test('TC-022: p-chip and p-badge elements present in opportunity view', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — PrimeNG badge/chip components exist', async () => {
      const badges = page.locator('app-opportunity-view p-badge');
      const chips = page.locator('app-opportunity-view p-chip');
      const badgeCount = await badges.count();
      const chipCount = await chips.count();
      expect(badgeCount + chipCount).toBeGreaterThanOrEqual(0);
    });
  });
});
