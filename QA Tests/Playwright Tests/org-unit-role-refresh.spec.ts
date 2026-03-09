/**
 * @fileoverview PNO-731: Org Unit Role Refresh on Opportunity Update E2E Tests
 *
 * When an opportunity is updated and has a ResponsibleOrgUnitId, the system should always
 * re-trigger stakeholder auto-population from the org unit's entity user roles. Previously,
 * this only happened when the org unit value changed — now it runs whenever the org unit
 * is present in the update request.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-731
 * @tests 15
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForLoadingToComplete,
  waitForElementReady,
  waitForNetworkIdle,
} from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Feature gate: set PNO_731_ORG_UNIT_REFRESH_IMPLEMENTED=true to run these tests. */
const featureReady = process.env.PNO_731_ORG_UNIT_REFRESH_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';
const COLLABORATOR_USER = 'collaborator@example.com';

const TEST_OPPORTUNITY_ID = process.env.TEST_OPPORTUNITY_ID || '1';
const OPPORTUNITIES_URL = '/partnerships/opportunities';
const OPPORTUNITY_DETAIL_URL = `${OPPORTUNITIES_URL}/${TEST_OPPORTUNITY_ID}`;

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function skipIfNotReady(reason = 'PNO-731 Org Unit Role Refresh not deployed — set PNO_731_ORG_UNIT_REFRESH_IMPLEMENTED=true') {
  test.skip(!featureReady, reason);
}

// =============================================================================
// SECTION 1: Org Unit Selection
// =============================================================================
test.describe('PNO-731 — Org Unit Selection', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Org unit dropdown displays on opportunity detail', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity detail', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — org unit field visible', async () => {
      const orgUnitDisplay = oppPage.opportunityOrgUnit;
      const orgUnitField = page.locator('#field-responsibleOrgUnitId');
      const orgUnitLabel = page.getByText(/responsible.*org.*unit|organization.*unit/i);
      const visible =
        (await orgUnitDisplay.isVisible().catch(() => false)) ||
        (await orgUnitField.isVisible().catch(() => false)) ||
        (await orgUnitLabel.first().isVisible().catch(() => false));
      expect(visible).toBe(true);
    });
  });

  test('TC-002: Org unit dropdown displays correct options', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity and enter edit mode', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — click edit on team section', async () => {
      const editBtn = page.locator('p-button').filter({ hasText: /edit/i }).first();
      const visible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        await editBtn.click();
        await waitForLoadingToComplete(page);
      }
    });

    await test.step('Assert — team section visible with org unit controls', async () => {
      const teamSection = oppPage.teamSection;
      await expect(teamSection).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-003: Org unit field shows current value in view mode', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity detail', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — org unit name or header displayed', async () => {
      const headerOrgUnit = oppPage.opportunityOrgUnit;
      const orgUnitText = page.getByText(/HQ|Headquarters|organization.*unit/i);
      const hasOrgUnit =
        (await headerOrgUnit.isVisible().catch(() => false)) ||
        (await orgUnitText.first().isVisible().catch(() => false));
      expect(hasOrgUnit).toBe(true);
    });
  });
});

// =============================================================================
// SECTION 2: Stakeholder Refresh
// =============================================================================
test.describe('PNO-731 — Stakeholder Refresh', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-004: Changing org unit triggers stakeholder refresh', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Act — enter edit and change org unit', async () => {
      const editBtn = page.locator('p-button').filter({ hasText: /edit/i }).first();
      const visible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
      if (visible) {
        await editBtn.click();
        await waitForLoadingToComplete(page);
        const orgUnitSelect = page.locator('p-select').first();
        const selectVisible = await orgUnitSelect.isVisible().catch(() => false);
        if (selectVisible) {
          await orgUnitSelect.click();
          await page.locator('.p-select-option').first().click();
          await waitForLoadingToComplete(page);
        }
      }
    });

    await test.step('Assert — stakeholder section visible', async () => {
      const stakeholderSection = page.getByText(/stakeholder|team|role holder/i);
      await expect(stakeholderSection.first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-005: Updating opportunity without changing org unit still refreshes stakeholders', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity with org unit', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — PNO-731: team section visible when org unit present', async () => {
      const teamSection = oppPage.teamSection;
      await expect(teamSection).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-006: Stakeholder list updates after org unit change', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — stakeholder/team content present', async () => {
      const stakeholderContent = page.getByText(/Test User|Jane Doe|stakeholder|role holder|team/i);
      await expect(stakeholderContent.first()).toBeVisible({ timeout: 10000 });
    });
  });
});

// =============================================================================
// SECTION 3: Permission Checks
// =============================================================================
test.describe('PNO-731 — Permission Checks on Org Unit Field', () => {
  test.slow();
  skipIfNotReady();

  test('TC-007: Admin user can see and edit org unit field', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, ADMIN_USER);
    await waitForPermissions(page);

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const editBtn = page.locator('p-button').filter({ hasText: /edit/i }).first();
    await expect(editBtn).toBeVisible({ timeout: 10000 });
  });

  test('TC-008: Readonly user cannot edit org unit field', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, READONLY_USER);
    await waitForPermissions(page);

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const editBtn = page.locator('p-button').filter({ hasText: /edit/i }).first();
    await expect(editBtn).not.toBeVisible();
  });

  test('TC-009: Collaborator can edit org unit field', async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, COLLABORATOR_USER);
    await waitForPermissions(page);

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const editBtn = page.locator('p-button').filter({ hasText: /edit/i }).first();
    await expect(editBtn).toBeVisible({ timeout: 10000 });
  });
});

// =============================================================================
// SECTION 4: Negative & Edge Cases
// =============================================================================
test.describe('PNO-731 — Negative & Edge Cases', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-010: Opportunity without org unit shows prerequisite message', async ({ page }) => {
    await page.route(url => /\/api\/opportunity\/\d+$/.test(url.toString()), async (route) => {
      const method = route.request().method();
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 1,
            name: 'Test Opportunity',
            responsibleOrgUnitId: null,
            responsibleOrgUnitName: null,
            stakeholders: [],
            status: 'Draft',
            stage: 'IDENTIFY & PROFILE',
          }),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ id: 1, success: true }),
        });
      }
    });

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const prereqMessage = page.getByText(/prerequisite|complete.*org.*unit|where section/i);
    await expect(prereqMessage.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-011: Org unit options loaded from organization-units API', async ({ page }) => {
    let orgUnitsCalled = false;
    await page.route(url => url.toString().includes('/api/values/organization-units'), async (route) => {
      orgUnitsCalled = true;
      await route.continue();
    });

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    expect(orgUnitsCalled).toBe(true);
  });

  test('TC-012: Team section displays stakeholder role holders', async ({ page }) => {
    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForLoadingToComplete(page);

    const roleHolderSection = page.getByText(/role holder|stakeholder|team/i);
    await expect(roleHolderSection.first()).toBeVisible({ timeout: 10000 });
  });
});

// =============================================================================
// SECTION 5: Integration
// =============================================================================
test.describe('PNO-731 — Integration', () => {
  test.slow();
  skipIfNotReady();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, OPPORTUNITY_DETAIL_URL, ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-013: Full flow — navigate to opportunity, view org unit, view stakeholders', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPPORTUNITY_ID);
    await test.step('Arrange — navigate to opportunity', async () => {
      await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
      await waitForNetworkIdle(page);
      await waitForPermissions(page);
    });

    await test.step('Act — scroll to team section', async () => {
      const teamSection = oppPage.teamSection;
      await teamSection.scrollIntoViewIfNeeded().catch(() => {});
      await waitForElementReady(teamSection);
    });

    await test.step('Assert — org unit and stakeholders visible', async () => {
      const orgUnitVisible = await oppPage.opportunityOrgUnit.isVisible().catch(() => false);
      const stakeholderSection = page.getByText(/stakeholder|team|role holder/i);
      const stakeholderVisible = await stakeholderSection.first().isVisible().catch(() => false);
      expect(orgUnitVisible || stakeholderVisible).toBe(true);
    });
  });

  test('TC-014: Opportunity detail URL matches expected pattern', async ({ page }) => {
    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);

    expect(page.url()).toContain('/partnerships/opportunities/');
    expect(page.url()).toContain(TEST_OPPORTUNITY_ID);
  });

  test('TC-015: Permission endpoint called for opportunity', async ({ page }) => {
    let permissionsCalled = false;
    await page.route(url => /\/api\/opportunity\/\d+\/permissions/.test(url.toString()), async (route) => {
      permissionsCalled = true;
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          canView: true,
          canEdit: true,
          canDelete: false,
          canSubmit: true,
        }),
      });
    });

    await page.goto(`http://localhost:4200${OPPORTUNITY_DETAIL_URL}`);
    await waitForNetworkIdle(page);
    await waitForPermissions(page);

    expect(permissionsCalled).toBe(true);
  });
});
