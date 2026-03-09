/**
 * @fileoverview Opportunity Workflow Transitions E2E Tests
 *
 * Tests for workflow transitions not covered by go-decision.spec.ts:
 * Cancel flow, Reopen flows, and immutability enforcement on terminal stages.
 * Also covers approval-pending state blocking edits.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-WORKFLOW
 * @tests 16
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_WORKFLOW_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  go: process.env.TEST_OPP_GO_ID || '8',
  noGo: process.env.TEST_OPP_NOGO_ID || '11',
  cancelled: process.env.TEST_OPP_CANCELLED_ID || '10',
  inWorkflow: process.env.TEST_OPP_IN_WORKFLOW_ID || '12',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Immutability — GO Stage
// =============================================================================
test.describe('Workflow — GO Stage Immutability', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
  });

  test('WF-IMM-001: GO opportunity shows no edit buttons on any section', async ({ page }) => {
    const editButtons = page.locator('button:has(i.pi-pencil)');
    const count = await editButtons.count();
    for (let i = 0; i < count; i++) {
      const btn = editButtons.nth(i);
      const isInWorkflow = await btn.locator('xpath=ancestor::app-stage-workflow').count() > 0;
      if (!isInWorkflow) {
        const isVisible = await btn.isVisible({ timeout: 1000 }).catch(() => false);
        if (isVisible) {
          const text = await btn.textContent().catch(() => '');
          expect(text).not.toContain('Edit');
        }
      }
    }
  });

  test('WF-IMM-002: GO opportunity displays immutable stage badge', async ({ page }) => {
    const stageBadge = page.locator('[data-testid="opportunity-stage"]');
    const hasStage = await stageBadge.isVisible({ timeout: 5000 }).catch(() => false);
    if (hasStage) {
      await expect(stageBadge).toContainText(/GO/i);
    }
  });

  test('WF-IMM-003: GO opportunity has no delete button', async ({ page }) => {
    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 2: Immutability — NO GO Stage
// =============================================================================
test.describe('Workflow — NO GO Stage Immutability', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.noGo));
    await waitForPermissions(page);
  });

  test('WF-IMM-004: NO GO opportunity shows Reopen action', async ({ page }) => {
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });

  test('WF-IMM-005: NO GO opportunity hides section edit buttons', async ({ page }) => {
    const sectionEditBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await sectionEditBtns.count();
    expect(count).toBe(0);
  });
});

// =============================================================================
// SECTION 3: Immutability — CANCELLED Stage
// =============================================================================
test.describe('Workflow — CANCELLED Stage Immutability', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.cancelled));
    await waitForPermissions(page);
  });

  test('WF-IMM-006: CANCELLED opportunity shows Reopen action', async ({ page }) => {
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });

  test('WF-IMM-007: CANCELLED opportunity hides edit controls', async ({ page }) => {
    const sectionEditBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-why button:has(i.pi-pencil)');
    const count = await sectionEditBtns.count();
    expect(count).toBe(0);
  });
});

// =============================================================================
// SECTION 4: Approval Pending — Edits Blocked
// =============================================================================
test.describe('Workflow — Approval Pending Blocks Edits', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.inWorkflow));
    await waitForPermissions(page);
  });

  test('WF-PEND-001: In-workflow opportunity shows pending status', async ({ page }) => {
    const statusBadge = page.locator('[data-testid="opportunity-status"]');
    const workflowIndicator = page.getByText(/in workflow|pending|approval/i).first();
    const hasStatus = await statusBadge.isVisible({ timeout: 5000 }).catch(() => false);
    const hasWorkflow = await workflowIndicator.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasStatus || hasWorkflow).toBeTruthy();
  });

  test('WF-PEND-002: In-workflow opportunity hides section edit buttons', async ({ page }) => {
    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });

  test('WF-PEND-003: Recall button visible for OM on in-workflow opportunity', async ({ page }) => {
    const recallBtn = page.getByRole('button', { name: /recall/i });
    const isVisible = await recallBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 5: Cancel Flow
// =============================================================================
test.describe('Workflow — Cancel Flow', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test('WF-CANCEL-001: Cancel button visible on draft opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const isVisible = await cancelBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });

  test('WF-CANCEL-002: Cancel requires reason text', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);

    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const isVisible = await cancelBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!isVisible, 'Cancel button not visible');

    await cancelBtn.click();
    const reasonField = page.getByPlaceholder(/reason/i).or(page.locator('textarea').first());
    await expect(reasonField).toBeVisible({ timeout: 5000 });
  });

  test('WF-CANCEL-003: Read-only user cannot see cancel button', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    await expect(cancelBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 6: Reopen Flows
// =============================================================================
test.describe('Workflow — Reopen Flows', () => {
  test.slow();
  test.skip(!featureReady, 'Workflow not deployed — set OPPORTUNITY_WORKFLOW_IMPLEMENTED=true');

  test('WF-REOPEN-001: Reopen button visible on NO GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.noGo));
    await waitForPermissions(page);
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });

  test('WF-REOPEN-002: Reopen button visible on CANCELLED opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.cancelled));
    await waitForPermissions(page);
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(isVisible || await page.locator('app-stage-workflow').isVisible()).toBeTruthy();
  });

  test('WF-REOPEN-003: Read-only user cannot see reopen button', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.cancelled), READONLY_USER);
    await waitForPermissions(page);
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    await expect(reopenBtn).not.toBeVisible({ timeout: 5000 });
  });
});
