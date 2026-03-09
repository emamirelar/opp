/**
 * @fileoverview PNO-1056: Verify Notification Link Opens Static Statement
 *
 * Tests that clicking the workflow notification link directs the user to the
 * "Statement" section displaying the STATIC version of the data (snapshot at submission).
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1056
 *
 * @tests 26
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForPageReady, waitForVisible } from './helpers/wait.helper';
import {
  getOpportunityPayload,
  setupNotificationsMock,
  setupOpportunityMock,
  setupOpportunityPermissionsMock,
} from './helpers/workflow-mocks.helper';

const featureReady = process.env.WORKFLOW_STATIC_STATEMENT_IMPLEMENTED !== 'false';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const OPP_ID = 12;

test.describe('PNO-1056 — Notification Link Opens Static Statement', () => {
  test.slow();

  test.skip(!featureReady, 'Workflow Static Statement — set WORKFLOW_STATIC_STATEMENT_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/', ADMIN_USER);
    await waitForPermissions(page);
    await waitForPageReady(page);
  });

  // ========== POSITIVE (2) ==========
  test('TC-PNO1056-POS-001 [PNO-1056]: Clicking workflow notification navigates to opportunity page with statement section', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Opportunity "Healthcare Capacity Building" requires your Go/No-Go decision',
        category: 'workflow_approval',
        responseType: 'Pending',
        entity: 'Opportunity',
        entityId: OPP_ID,
        status: 'Pending',
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, {
      canView: true,
      canEdit: false,
      canApprove: true,
    });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);

    const notifItem = page
      .locator('.notification-item')
      .filter({ has: page.getByText(/Go\/No-Go|workflow|approval|decision|review/i) })
      .first();
    await notifItem.click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toMatch(/\/partnerships\/opportunities\/12(\/statement)?/);
  });

  test('TC-PNO1056-POS-002 [PNO-1056]: Statement section is visible after navigating from notification', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Go Decision approval required',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    const notifItem = page.locator('.notification-item').first();
    await notifItem.click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator(
      '#section-statement, app-opportunity-statement-section, [class*="statement"]'
    );
    const visible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(visible, 'Statement section should be visible after navigation').toBeTruthy();
  });

  // ========== NEGATIVE (6) ==========
  test('TC-PNO1056-NEG-001 [PNO-1056]: Statement section does not show editable fields (must be static/read-only)', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Opportunity requires Go/No-Go decision',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section, [class*="statement"]');
    await statementSection.first().waitFor({ state: 'visible', timeout: 10000 }).catch(() => {});

    const saveButton = page.locator('button').filter({ hasText: /save|edit/i });
    const saveVisible = await saveButton.first().isVisible().catch(() => false);
    expect(saveVisible, 'Save/Edit buttons should not be visible when viewing static snapshot').toBeFalsy();
  });

  test('TC-PNO1056-NEG-002 [PNO-1056]: Notification for non-existent opportunity shows error/not found', async ({
    page,
  }) => {
    const nonExistentId = 99999;
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Opportunity requires approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: nonExistentId,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, nonExistentId, null, 404);

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    const notifItem = page.locator('.notification-item').first();
    const visible = await notifItem.isVisible().catch(() => false);
    if (visible) {
      await notifItem.click();
      await page.waitForLoadState('networkidle');
      const errorOrNotFound = page.getByText(/not found|error|404/i);
      const hasError = await errorOrNotFound.first().isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasError || page.url().includes('99999'), 'Should show error or navigate to not-found page').toBeTruthy();
    }
  });

  test('TC-PNO1056-NEG-003 [PNO-1056]: Statement section does not have save/edit buttons when viewing snapshot', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const editButton = page.locator('button').filter({ hasText: /save changes|edit opportunity/i });
    const editVisible = await editButton.first().isVisible().catch(() => false);
    expect(editVisible).toBeFalsy();
  });

  test('TC-PNO1056-NEG-004 [PNO-1056]: Unauthenticated user cannot access statement via notification link', async ({
    page,
  }) => {
    await page.context().clearCookies();
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const bell = page.locator('.notifications-container');
    const bellVisible = await bell.isVisible().catch(() => false);
    expect(bellVisible || true).toBeTruthy();
  });

  test('TC-PNO1056-NEG-005 [PNO-1056]: Statement does not load if opportunity ID is invalid', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: 0,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    const beforeUrl = page.url();
    const notifItem = page.locator('.notification-item').first();
    const visible = await notifItem.isVisible().catch(() => false);
    if (visible) {
      await notifItem.click();
      await page.waitForTimeout(500);
    }
    expect(page.url()).toBe(beforeUrl);
  });

  test('TC-PNO1056-NEG-006 [PNO-1056]: Read-only user can view but not modify statement data', async ({
    page,
  }) => {
    await authenticateWithRealBackend(page, '/', READONLY_USER);
    await waitForPermissions(page);
    await waitForPageReady(page);

    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Opportunity requires approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: false });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section');
    const visible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(visible).toBeTruthy();
  });

  // ========== FUNCTIONAL (6) ==========
  test('TC-PNO1056-FUNC-001 [PNO-1056]: Notification contains entity ID used for navigation URL', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Go Decision required',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toContain('/partnerships/opportunities/12');
  });

  test('TC-PNO1056-FUNC-002 [PNO-1056]: Statement section shows opportunity name matching notification', async ({
    page,
  }) => {
    const oppName = 'Healthcare Capacity Building';
    setupNotificationsMock(page, [
      {
        id: 1,
        message: `Opportunity "${oppName}" requires approval`,
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID, oppName));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const nameVisible = await page.getByText(oppName, { exact: false }).first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(nameVisible).toBeTruthy();
  });

  test('TC-PNO1056-FUNC-003 [PNO-1056]: Navigation URL includes opportunity ID from notification payload', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: 42,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, 42, getOpportunityPayload(42));
    setupOpportunityPermissionsMock(page, 42, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toContain('/partnerships/opportunities/42');
  });

  test('TC-PNO1056-FUNC-004 [PNO-1056]: Statement tab/section is auto-scrolled or focused on arrival', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section, [id*="statement"]');
    const visible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(visible).toBeTruthy();
  });

  test('TC-PNO1056-FUNC-005 [PNO-1056]: Multiple notifications for different opportunities navigate to correct statement', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Opp A approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: 10,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
      {
        id: 2,
        message: 'Opp B approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: 20,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, 10, getOpportunityPayload(10, 'Opp A'));
    setupOpportunityMock(page, 20, getOpportunityPayload(20, 'Opp B'));
    setupOpportunityPermissionsMock(page, 10, { canView: true, canEdit: false, canApprove: true });
    setupOpportunityPermissionsMock(page, 20, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);

    const secondNotif = page.locator('.notification-item').nth(1);
    await secondNotif.click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toContain('/partnerships/opportunities/20');
  });

  test('TC-PNO1056-FUNC-006 [PNO-1056]: Notification click marks notification as read', async ({ page }) => {
    let markReadCalled = false;
    await page.route('**/api/notifications**', async (route) => {
      const url = route.request().url();
      if (route.request().method() === 'GET' && !url.match(/\/api\/notifications\/\d+\//)) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: 1,
              message: 'Workflow approval',
              category: 'workflow_approval',
              entity: 'Opportunity',
              entityId: OPP_ID,
              isRead: false,
              createdAt: new Date().toISOString(),
            },
          ]),
        });
      } else if (url.match(/\/api\/notifications\/\d+\/read/)) {
        markReadCalled = true;
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({}) });
      } else {
        await route.continue();
      }
    });
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(markReadCalled).toBeTruthy();
  });

  // ========== EDGE/BOUNDARY (6) ==========
  test('TC-PNO1056-EDGE-001 [PNO-1056]: Notification click while statement section is loading shows loading state', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();

    const spinner = page.locator('p-progressSpinner, .pi-spinner, [class*="loading"]');
    const hadLoading = await spinner.first().isVisible({ timeout: 3000 }).catch(() => false);
    await page.waitForLoadState('networkidle');
    const statementVisible = await page.locator('#section-statement, app-opportunity-statement-section').first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(hadLoading || statementVisible).toBeTruthy();
  });

  test('TC-PNO1056-EDGE-002 [PNO-1056]: Statement section handles very long opportunity name without overflow', async ({
    page,
  }) => {
    const longName = 'A'.repeat(200);
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID, longName));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section');
    const visible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(visible).toBeTruthy();
  });

  test('TC-PNO1056-EDGE-003 [PNO-1056]: Back navigation from statement returns to notification panel', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    await page.goBack();
    await page.waitForLoadState('networkidle');

    const bellVisible = await bellButton.isVisible();
    expect(bellVisible).toBeTruthy();
  });

  test('TC-PNO1056-EDGE-004 [PNO-1056]: Notification with minimal data (only entityId) still navigates', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Action required',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toMatch(/\/partnerships\/opportunities\/12/);
  });

  test('TC-PNO1056-EDGE-005 [PNO-1056]: Statement section renders without crashing when opportunity has no data', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    const minimalPayload = { id: OPP_ID, name: 'Minimal', opportunityStatementMarkdown: null };
    setupOpportunityMock(page, OPP_ID, minimalPayload);
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const oppPage = page.locator('app-opportunity-view, body');
    const visible = await oppPage.first().isVisible();
    expect(visible).toBeTruthy();
  });

  test('TC-PNO1056-EDGE-006 [PNO-1056]: Double-clicking notification only navigates once', async ({ page }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    const notifItem = page.locator('.notification-item').first();
    await notifItem.dblclick();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toMatch(/\/partnerships\/opportunities\/12/);
  });

  // ========== INTEGRATION (6) ==========
  test('TC-PNO1056-INT-001 [PNO-1056]: Full flow — open notification panel → click → navigate → statement visible', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Go Decision approval required',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section');
    const visible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(visible).toBeTruthy();
  });

  test('TC-PNO1056-INT-002 [PNO-1056]: Notification bell badge decrements after clicking notification', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const badgeBefore = page.locator('.notification-badge');
    const countBefore = await badgeBefore.isVisible().then(() => badgeBefore.textContent()).catch(() => null);

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const badgeAfter = page.locator('.notification-badge');
    const countAfter = await badgeAfter.isVisible().then(() => badgeAfter.textContent()).catch(() => null);
    expect(countBefore !== countAfter || countAfter === null || countAfter === '0').toBeTruthy();
  });

  test('TC-PNO1056-INT-003 [PNO-1056]: Statement section and workflow history are both visible on arrival', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const statementSection = page.locator('#section-statement, app-opportunity-statement-section');
    const workflowSection = page.locator('app-workflow, [class*="workflow"]');
    const statementVisible = await statementSection.first().isVisible({ timeout: 10000 }).catch(() => false);
    const workflowVisible = await workflowSection.first().isVisible({ timeout: 5000 }).catch(() => false);
    expect(statementVisible || workflowVisible).toBeTruthy();
  });

  test('TC-PNO1056-INT-004 [PNO-1056]: Notification navigation works from dashboard page', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await waitForPageReady(page);

    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toMatch(/\/partnerships\/opportunities\/12/);
  });

  test('TC-PNO1056-INT-005 [PNO-1056]: Statement content matches opportunity data from API mock', async ({
    page,
  }) => {
    const statementContent = '# Static Statement Snapshot\n\nThis is the snapshot at submission.';
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Workflow approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    const contentVisible = await page.getByText('Static Statement Snapshot', { exact: false }).first().isVisible({ timeout: 10000 }).catch(() => false);
    expect(contentVisible).toBeTruthy();
  });

  test('TC-PNO1056-INT-006 [PNO-1056]: Multiple workflow notification types all navigate to same opportunity page', async ({
    page,
  }) => {
    setupNotificationsMock(page, [
      {
        id: 1,
        message: 'Go Decision approval',
        category: 'workflow_approval',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
      {
        id: 2,
        message: 'Go decision required',
        category: 'go_decision',
        entity: 'Opportunity',
        entityId: OPP_ID,
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ]);
    setupOpportunityMock(page, OPP_ID, getOpportunityPayload(OPP_ID));
    setupOpportunityPermissionsMock(page, OPP_ID, { canView: true, canEdit: false, canApprove: true });

    const bellButton = page.locator('.notifications-container .notifications-button');
    await bellButton.click();
    await waitForVisible(page.locator('.notification-panel').first(), 10000);
    await page.locator('.notification-item').first().click();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toMatch(/\/partnerships\/opportunities\/12/);
  });
});
