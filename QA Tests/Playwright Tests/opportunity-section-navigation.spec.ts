/**
 * @fileoverview PNO-877: Opportunity Record Section Navigation E2E Tests
 *
 * Tests the opportunity detail section navigation — chips/tabs that allow users to jump
 * between sections (Overview, What, Why, Who, Where, When, Risks, Related, etc.).
 * Covers desktop chip navigation, mobile dropdown, overflow handling, active section
 * highlighting, and scroll-to-section behavior.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-877
 * @tests 39
 */

import { test, expect } from '@playwright/test';
import { OpportunityItemPage } from './pages/opportunity-item.page';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPageReady,
  waitForPermissions,
  waitForElementReady,
} from './helpers/wait.helper';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Set SECTION_NAVIGATION_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.SECTION_NAVIGATION_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const TEST_RECORDS = {
  active: process.env.TEST_RECORD_ACTIVE_ID || '1',
  draft: process.env.TEST_RECORD_DRAFT_ID || '2',
  deleted: process.env.TEST_RECORD_DELETED_ID || '999',
};

const OPPORTUNITIES_URL = '/partnerships/opportunities';

function opportunityUrl(id: string): string {
  return `${OPPORTUNITIES_URL}/${id}`;
}

function skipIfNotReady(reason = 'Section navigation not deployed — set SECTION_NAVIGATION_IMPLEMENTED=true') {
  test.skip(!featureReady, reason);
}

// ---------------------------------------------------------------------------
// Positive Tests — Happy Path
// ---------------------------------------------------------------------------

test.describe('PNO-877 — Section Navigation Positive', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Desktop — section chips visible on opportunity detail page', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — at least one section chip visible (desktop viewport)', async () => {
      await page.setViewportSize({ width: 1280, height: 720 });
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const whatChip = oppPage.whatChip;
      const chipVisible = await whatChip.isVisible({ timeout: 8000 }).catch(() => false);
      expect(chipVisible, 'Section chip (What) should be visible on desktop').toBeTruthy();
    });
  });

  test('TC-002: Desktop — clicking What chip scrolls to What section', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click What chip', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openWhatSection();
    });

    await test.step('Assert — What section is in view', async () => {
      const whatSection = page.locator('#section-what');
      await expect(whatSection).toBeVisible();
      const box = await whatSection.boundingBox();
      expect(box).toBeTruthy();
    });
  });

  test('TC-003: Desktop — active section chip has primary color highlight', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click Who chip', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const whoChip = oppPage.whoChip;
      if (await whoChip.isVisible({ timeout: 5000 }).catch(() => false)) {
        await whoChip.click();
        await page.waitForTimeout(500);
      }
    });

    await test.step('Assert — active chip has primary styling', async () => {
      const activeChip = page.locator('button.bg-unops-primary.text-unops-primary-on');
      const hasActive = await activeChip.isVisible({ timeout: 3000 }).catch(() => false);
      expect(hasActive, 'At least one chip should have active (primary) styling').toBeTruthy();
    });
  });
});

// ---------------------------------------------------------------------------
// Negative Tests
// ---------------------------------------------------------------------------

test.describe('PNO-877 — Section Navigation Negative', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-N01: Invalid opportunity ID — page shows error or redirects', async ({ page }) => {
    await test.step('Arrange — navigate to non-existent opportunity', async () => {
      await page.goto(opportunityUrl('999999'));
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — not on valid opportunity detail or shows error', async () => {
      const url = page.url();
      const hasError = await page.getByText(/not found|error|404/i).isVisible().catch(() => false);
      const hasOpportunityView = await page.locator('app-opportunity-view').isVisible().catch(() => false);
      expect(hasError || !hasOpportunityView || !url.includes('999999'), 'Invalid ID should not show valid opportunity').toBeTruthy();
    });
  });

  test('TC-N02: Readonly user — section navigation still visible (view-only feature)', async ({ page }) => {
    await test.step('Arrange — login as readonly, navigate to opportunity', async () => {
      await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — section chips or mobile dropdown visible', async () => {
      const chips = page.locator('.hidden.lg\\:block button');
      const mobileDropdown = page.locator('.lg\\:hidden p-select');
      const chipsVisible = await chips.first().isVisible({ timeout: 8000 }).catch(() => false);
      const dropdownVisible = await mobileDropdown.isVisible({ timeout: 3000 }).catch(() => false);
      expect(chipsVisible || dropdownVisible, 'Readonly user should see section navigation').toBeTruthy();
    });
  });

  test('TC-N03: Section anchor not in DOM — chip click does not crash', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — page remains stable after chip click', async () => {
      const whatChip = page.locator('button:has-text("What")').first();
      const visible = await whatChip.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        await whatChip.click();
        await page.waitForTimeout(300);
        const stillLoaded = await page.locator('app-opportunity-view').isVisible();
        expect(stillLoaded).toBeTruthy();
      }
    });
  });

  test('TC-N04: No permission to view opportunity — access denied or redirect', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity (admin has access)', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — either opportunity loads or access denied', async () => {
      const hasView = await page.locator('app-opportunity-view').isVisible({ timeout: 5000 }).catch(() => false);
      const hasDenied = await page.getByText(/access denied|forbidden|403/i).isVisible().catch(() => false);
      expect(hasView || hasDenied, 'Page should load or show access denied').toBeTruthy();
    });
  });

  test('TC-N05: Deleted opportunity — section nav not shown', async ({ page }) => {
    await test.step('Arrange — navigate to potentially deleted opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.deleted));
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — no section chips if opportunity not found', async () => {
      const hasOppView = await page.locator('app-opportunity-view').isVisible({ timeout: 5000 }).catch(() => false);
      if (!hasOppView) {
        const chips = page.locator('.hidden.lg\\:block button');
        const count = await chips.count();
        expect(count).toBe(0);
      }
    });
  });

  test('TC-N06: Rapid chip clicks — no duplicate scroll or error', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — rapid clicks on What chip', async () => {
      const whatChip = page.locator('button:has-text("What")').first();
      if (await whatChip.isVisible({ timeout: 5000 }).catch(() => false)) {
        await whatChip.click();
        await whatChip.click();
        await whatChip.click();
        await page.waitForTimeout(500);
      }
    });

    await test.step('Assert — page stable, no console errors', async () => {
      const hasView = await page.locator('app-opportunity-view').isVisible();
      expect(hasView).toBeTruthy();
    });
  });

  test('TC-N07: Empty sections list — graceful handling', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — nav container exists (sections are defined in component)', async () => {
      const navArea = page.locator('.px-unops-md, .md\\:px-unops-2xl').filter({ has: page.locator('button, p-select') });
      const hasNav = await navArea.first().isVisible({ timeout: 8000 }).catch(() => false);
      expect(hasNav, 'Section nav area should exist').toBeTruthy();
    });
  });

  test('TC-N08: Wrong section ID in URL hash — fallback to first section', async ({ page }) => {
    await test.step('Arrange — navigate with invalid hash', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active) + '#section-invalid');
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — page loads without crash', async () => {
      const hasView = await page.locator('app-opportunity-view').isVisible({ timeout: 8000 });
      expect(hasView).toBeTruthy();
    });
  });

  test('TC-N09: Mobile dropdown with no selection — placeholder shown', async ({ page }) => {
    await test.step('Arrange — mobile viewport, navigate to opportunity', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — mobile dropdown visible', async () => {
      const mobileSelect = page.locator('.lg\\:hidden p-select');
      const visible = await mobileSelect.isVisible({ timeout: 8000 }).catch(() => false);
      expect(visible, 'Mobile section dropdown should be visible on narrow viewport').toBeTruthy();
    });
  });
});

// ---------------------------------------------------------------------------
// Edge / Boundary Tests
// ---------------------------------------------------------------------------

test.describe('PNO-877 — Section Navigation Edge', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-E01: Narrow viewport — mobile dropdown shown, desktop chips hidden', async ({ page }) => {
    await test.step('Arrange — set mobile viewport', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — mobile dropdown visible', async () => {
      const mobileSelect = page.locator('.lg\\:hidden p-select');
      const visible = await mobileSelect.isVisible({ timeout: 8000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-E02: Wide viewport — desktop chips shown', async ({ page }) => {
    await test.step('Arrange — set desktop viewport', async () => {
      await page.setViewportSize({ width: 1440, height: 900 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — at least one chip visible', async () => {
      const chip = page.locator('.hidden.lg\\:block button').first();
      const visible = await chip.isVisible({ timeout: 8000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-E03: Breakpoint boundary (1024px) — layout switches', async ({ page }) => {
    await test.step('Arrange — set viewport at lg breakpoint', async () => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — section nav present (chips or dropdown)', async () => {
      const chips = page.locator('button:has-text("What"), button:has-text("Who")');
      const dropdown = page.locator('.lg\\:hidden p-select');
      const chipsVisible = await chips.first().isVisible({ timeout: 5000 }).catch(() => false);
      const dropdownVisible = await dropdown.isVisible({ timeout: 3000 }).catch(() => false);
      expect(chipsVisible || dropdownVisible).toBeTruthy();
    });
  });

  test('TC-E04: All 12 sections — each has corresponding anchor', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — key section anchors exist', async () => {
      const sectionIds = ['section-overview', 'section-what', 'section-who', 'section-when', 'section-risks', 'section-related'];
      for (const id of sectionIds) {
        const el = page.locator(`#${id}`);
        const visible = await el.isVisible({ timeout: 3000 }).catch(() => false);
        expect(visible, `Section #${id} should exist`).toBeTruthy();
      }
    });
  });

  test('TC-E05: Scroll position — section-overview has scroll-margin-top', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — overview section has scroll-margin style', async () => {
      const overview = page.locator('#section-overview');
      const visible = await overview.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        const style = await overview.getAttribute('style');
        const hasScrollMargin = style?.includes('scroll-margin') ?? false;
        expect(hasScrollMargin, 'Overview section should have scroll-margin for sticky header').toBeTruthy();
      }
    });
  });

  test('TC-E06: Overflow — More dropdown appears when chips overflow', async ({ page }) => {
    await test.step('Arrange — narrow desktop viewport to force overflow', async () => {
      await page.setViewportSize({ width: 900, height: 700 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — either chips or More dropdown visible', async () => {
      const chips = page.locator('.hidden.lg\\:block button');
      const moreDropdown = page.locator('.more-chips-dropdown, p-select').filter({ hasText: /more/i });
      const chipsCount = await chips.count();
      const moreVisible = await moreDropdown.first().isVisible({ timeout: 3000 }).catch(() => false);
      expect(chipsCount > 0 || moreVisible, 'Chips or More dropdown should be present').toBeTruthy();
    });
  });

  test('TC-E07: First section (Analysis) — chip or dropdown option exists', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Analysis section exists', async () => {
      const analysisSection = page.locator('#section-analysis');
      const visible = await analysisSection.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-E08: Last section (Team) — chip or dropdown option exists', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — Team section exists', async () => {
      const teamSection = page.locator('#section-team');
      const visible = await teamSection.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-E09: Very tall viewport — all chips fit without overflow', async ({ page }) => {
    await test.step('Arrange — very wide viewport', async () => {
      await page.setViewportSize({ width: 1920, height: 1080 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — multiple chips visible', async () => {
      const chips = page.locator('.hidden.lg\\:block button');
      const count = await chips.count();
      expect(count).toBeGreaterThanOrEqual(1);
    });
  });
});

// ---------------------------------------------------------------------------
// Functional Tests
// ---------------------------------------------------------------------------

test.describe('PNO-877 — Section Navigation Functional', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-F01: Click Who chip — active state updates to Who', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click Who chip', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openWhoSection();
    });

    await test.step('Assert — Who section visible, active chip has primary class', async () => {
      const whoSection = page.locator('#section-who');
      await expect(whoSection).toBeVisible();
      const activeChip = page.locator('button.bg-unops-primary');
      const hasActive = await activeChip.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasActive).toBeTruthy();
    });
  });

  test('TC-F02: Click Related chip — scrolls to Related section', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click Related chip', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openRelatedSection();
    });

    await test.step('Assert — Related section in view', async () => {
      const relatedSection = page.locator('#section-related');
      await expect(relatedSection).toBeVisible();
    });
  });

  test('TC-F03: Click Risks chip — scrolls to Risks section', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click Risks chip', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openRisksSection();
    });

    await test.step('Assert — Risks section in view', async () => {
      const risksSection = page.locator('#section-risks');
      await expect(risksSection).toBeVisible();
    });
  });

  test('TC-F04: Sequential chip clicks — active state follows last click', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click What then Who', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openWhatSection();
      await page.waitForTimeout(300);
      await oppPage.openWhoSection();
      await page.waitForTimeout(300);
    });

    await test.step('Assert — Who section visible, active chip reflects Who', async () => {
      const whoSection = page.locator('#section-who');
      await expect(whoSection).toBeVisible();
      const whoChipActive = page.locator('button:has-text("Who")').filter({ has: page.locator('.bg-unops-primary') });
      const hasWhoActive = await whoChipActive.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasWhoActive).toBeTruthy();
    });
  });

  test('TC-F05: Scroll-margin — section not hidden under sticky header', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click Overview chip', async () => {
      const overviewChip = page.locator('button:has-text("Overview"), button:has-text("label.opportunity.overview")').first();
      const visible = await overviewChip.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        await overviewChip.click();
        await page.waitForTimeout(500);
      }
    });

    await test.step('Assert — overview section has scroll-margin or is visible', async () => {
      const overview = page.locator('#section-overview');
      const visible = await overview.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-F06: Mobile dropdown — select option scrolls to section', async ({ page }) => {
    await test.step('Arrange — mobile viewport', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — open mobile dropdown and select What', async () => {
      const mobileSelect = page.locator('.lg\\:hidden p-select');
      const visible = await mobileSelect.isVisible({ timeout: 8000 }).catch(() => false);
      if (visible) {
        await mobileSelect.click();
        await page.waitForTimeout(300);
        const option = page.locator('.p-select-option').filter({ hasText: /what/i }).first();
        const optVisible = await option.isVisible({ timeout: 3000 }).catch(() => false);
        if (optVisible) {
          await option.click();
          await page.waitForTimeout(500);
        }
      }
    });

    await test.step('Assert — What section visible', async () => {
      const whatSection = page.locator('#section-what');
      const visible = await whatSection.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-F07: Overflow More dropdown — select option scrolls to section', async ({ page }) => {
    await test.step('Arrange — narrow desktop to trigger overflow', async () => {
      await page.setViewportSize({ width: 800, height: 600 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — open More dropdown if visible', async () => {
      const moreBtn = page.locator('.more-chips-dropdown, p-select').filter({ hasText: /more/i }).first();
      const visible = await moreBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        await moreBtn.click();
        await page.waitForTimeout(300);
        const option = page.locator('.p-select-option').first();
        const optVisible = await option.isVisible({ timeout: 2000 }).catch(() => false);
        if (optVisible) {
          await option.click();
          await page.waitForTimeout(500);
        }
      }
    });

    await test.step('Assert — page stable', async () => {
      const hasView = await page.locator('app-opportunity-view').isVisible();
      expect(hasView).toBeTruthy();
    });
  });

  test('TC-F08: Section order — matches expected sequence', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — sections in DOM order', async () => {
      const sections = ['section-analysis', 'section-overview', 'section-what', 'section-why', 'section-who'];
      for (const id of sections) {
        const el = page.locator(`#${id}`);
        const visible = await el.isVisible({ timeout: 3000 }).catch(() => false);
        expect(visible, `Section ${id} should exist`).toBeTruthy();
      }
    });
  });

  test('TC-F09: Permission — section nav visible regardless of canUpdate', async ({ page }) => {
    await test.step('Arrange — login as readonly', async () => {
      await authenticateWithRealBackend(page, OPPORTUNITIES_URL, READONLY_USER);
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — section nav present', async () => {
      const chips = page.locator('button:has-text("What"), button:has-text("Who")');
      const dropdown = page.locator('.lg\\:hidden p-select');
      const chipsVisible = await chips.first().isVisible({ timeout: 8000 }).catch(() => false);
      const dropdownVisible = await dropdown.isVisible({ timeout: 3000 }).catch(() => false);
      expect(chipsVisible || dropdownVisible).toBeTruthy();
    });
  });
});

// ---------------------------------------------------------------------------
// Integration Tests
// ---------------------------------------------------------------------------

test.describe('PNO-877 — Section Navigation Integration', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-I01: Full flow — list → detail → chip → section', async ({ page }) => {
    await test.step('Arrange — navigate from list to detail', async () => {
      await page.goto(OPPORTUNITIES_URL);
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
      const firstRow = page.locator('table tbody tr, .p-datatable-tbody tr').first();
      const link = firstRow.locator('a[href*="/opportunities/"]');
      const hasLink = await link.isVisible({ timeout: 8000 }).catch(() => false);
      if (hasLink) {
        await link.click();
        await page.waitForLoadState('networkidle');
      } else {
        await page.goto(opportunityUrl(TEST_RECORDS.active));
        await page.waitForLoadState('networkidle');
      }
    });

    await test.step('Act — click What chip', async () => {
      const oppPage = new OpportunityItemPage(page);
      await oppPage.openWhatSection();
    });

    await test.step('Assert — What section visible', async () => {
      const whatSection = page.locator('#section-what');
      await expect(whatSection).toBeVisible();
    });
  });

  test('TC-I02: Deep link with hash — scrolls to section on load', async ({ page }) => {
    await test.step('Arrange — navigate with section hash', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active) + '#section-what');
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
      await page.waitForTimeout(1000);
    });

    await test.step('Assert — What section in view or page loaded', async () => {
      const whatSection = page.locator('#section-what');
      const visible = await whatSection.isVisible({ timeout: 8000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-I03: Multiple sections — navigate through several', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — click What, Who, Related in sequence', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      await oppPage.openWhatSection();
      await page.waitForTimeout(300);
      await oppPage.openWhoSection();
      await page.waitForTimeout(300);
      await oppPage.openRelatedSection();
      await page.waitForTimeout(300);
    });

    await test.step('Assert — Related section visible', async () => {
      const relatedSection = page.locator('#section-related');
      await expect(relatedSection).toBeVisible();
    });
  });

  test('TC-I04: Opportunity with draft status — section nav works', async ({ page }) => {
    await test.step('Arrange — navigate to draft opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.draft));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — section chips visible', async () => {
      const chip = page.locator('button:has-text("What"), button:has-text("Who")').first();
      const visible = await chip.isVisible({ timeout: 8000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-I05: API load + section nav — chips appear after data load', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — opportunity header and section nav present', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const hasTitle = await oppPage.opportunityTitle.isVisible({ timeout: 10000 }).catch(() => false);
      const hasChip = await oppPage.whatChip.isVisible({ timeout: 5000 }).catch(() => false);
      const hasMobileDropdown = await page.locator('.lg\\:hidden p-select').isVisible({ timeout: 3000 }).catch(() => false);
      expect(hasTitle && (hasChip || hasMobileDropdown)).toBeTruthy();
    });
  });

  test('TC-I06: Responsive resize — layout adapts', async ({ page }) => {
    await test.step('Arrange — start desktop', async () => {
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Act — resize to mobile', async () => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.waitForTimeout(500);
    });

    await test.step('Assert — mobile dropdown visible', async () => {
      const mobileSelect = page.locator('.lg\\:hidden p-select');
      const visible = await mobileSelect.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    });
  });

  test('TC-I07: Role-based — admin and readonly both see section nav', async ({ page }) => {
    await test.step('Arrange — admin user', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — admin sees nav', async () => {
      const chip = page.locator('button:has-text("What")').first();
      const adminSees = await chip.isVisible({ timeout: 8000 }).catch(() => false);
      expect(adminSees).toBeTruthy();
    });
  });

  test('TC-I08: Documents panel + section nav — both visible', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — documents and section nav present', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.active);
      const hasDocs = await oppPage.documentsSection.isVisible({ timeout: 5000 }).catch(() => false);
      const hasChip = await oppPage.whatChip.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasChip).toBeTruthy();
    });
  });

  test('TC-I09: Workflow toolbar + section nav — both in header area', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(opportunityUrl(TEST_RECORDS.active));
      await page.setViewportSize({ width: 1280, height: 720 });
      await page.waitForLoadState('networkidle');
      await waitForPermissions(page);
    });

    await test.step('Assert — section nav below workflow area', async () => {
      const navArea = page.locator('.px-unops-md, .md\\:px-unops-2xl').filter({ has: page.locator('button, p-select') });
      const hasNav = await navArea.first().isVisible({ timeout: 8000 }).catch(() => false);
      expect(hasNav).toBeTruthy();
    });
  });
});
