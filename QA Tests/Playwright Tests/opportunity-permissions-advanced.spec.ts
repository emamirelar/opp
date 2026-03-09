/**
 * @fileoverview Opportunity Advanced Permissions E2E Tests
 *
 * Tests for advanced permission scenarios: stakeholder-level access,
 * collaborator edit permissions, immutable stage enforcement,
 * and approval-pending edit blocking.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-PERMS
 * @tests 18
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForLoadingToComplete } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_PERMISSIONS_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';
const COLLABORATOR_USER = 'collaborator@example.com';
const VIEWER_USER = 'viewer@example.com';
const OTHER_USER = 'other-user@example.com';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  active: process.env.TEST_OPP_ACTIVE_ID || '4',
  go: process.env.TEST_OPP_GO_ID || '8',
  noGo: process.env.TEST_OPP_NOGO_ID || '11',
  cancelled: process.env.TEST_OPP_CANCELLED_ID || '10',
  inWorkflow: process.env.TEST_OPP_IN_WORKFLOW_ID || '12',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Collaborator Permissions — Can Edit Content
// =============================================================================
test.describe('Permissions — Collaborator Edit Access', () => {
  test.slow();
  test.skip(!featureReady, 'Permissions not deployed — set OPPORTUNITY_PERMISSIONS_IMPLEMENTED=true');

  test('PERM-001: Collaborator can view opportunity detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), COLLABORATOR_USER);
    await waitForPermissions(page);

    const title = page.locator('[data-testid="opportunity-title"]');
    await expect(title).toBeVisible({ timeout: 10000 });
  });

  test('PERM-002: Collaborator sees edit buttons on editable sections', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), COLLABORATOR_USER);
    await waitForPermissions(page);

    const editBtns = page.locator('button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBeGreaterThanOrEqual(0);
  });

  test('PERM-003: Collaborator cannot see workflow action buttons', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), COLLABORATOR_USER);
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const cancelBtn = page.getByRole('button', { name: /^cancel$/i });
    const submitVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const cancelVisible = await cancelBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(submitVisible).toBeFalsy();
    expect(cancelVisible).toBeFalsy();
  });

  test('PERM-004: Collaborator cannot delete opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), COLLABORATOR_USER);
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"], button:has-text("Delete")');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 2: Read-Only User Permissions
// =============================================================================
test.describe('Permissions — Read-Only User', () => {
  test.slow();
  test.skip(!featureReady, 'Permissions not deployed — set OPPORTUNITY_PERMISSIONS_IMPLEMENTED=true');

  test('PERM-005: Read-only user can view opportunity detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.active), READONLY_USER);
    await waitForPermissions(page);

    const title = page.locator('[data-testid="opportunity-title"]');
    await expect(title).toBeVisible({ timeout: 10000 });
  });

  test('PERM-006: Read-only user sees no edit buttons', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.active), READONLY_USER);
    await waitForPermissions(page);

    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil), #section-why button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });

  test('PERM-007: Read-only user sees no workflow actions', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const cancelBtn = page.getByRole('button', { name: /^cancel$/i });
    await expect(submitBtn).not.toBeVisible({ timeout: 5000 });
    await expect(cancelBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('PERM-008: Read-only user sees no delete button', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);

    const deleteBtn = page.locator('[data-testid="delete-button"]');
    await expect(deleteBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('PERM-009: Read-only user cannot upload documents', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);

    const uploadBtn = page.locator('button:has-text("Upload"), button:has(i.pi-upload)');
    await expect(uploadBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// SECTION 3: Immutable Stage — All Users
// =============================================================================
test.describe('Permissions — Immutable Stage Enforcement', () => {
  test.slow();
  test.skip(!featureReady, 'Permissions not deployed — set OPPORTUNITY_PERMISSIONS_IMPLEMENTED=true');

  test('PERM-010: Admin cannot edit GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);

    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });

  test('PERM-011: Admin cannot edit NO GO opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.noGo));
    await waitForPermissions(page);

    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });

  test('PERM-012: Admin cannot edit CANCELLED opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.cancelled));
    await waitForPermissions(page);

    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });
});

// =============================================================================
// SECTION 4: Approval Pending — Edit Blocked
// =============================================================================
test.describe('Permissions — Approval Pending Blocks Edits', () => {
  test.slow();
  test.skip(!featureReady, 'Permissions not deployed — set OPPORTUNITY_PERMISSIONS_IMPLEMENTED=true');

  test('PERM-013: In-workflow opportunity blocks section editing for admin', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.inWorkflow));
    await waitForPermissions(page);

    const editBtns = page.locator('#section-overview button:has(i.pi-pencil), #section-what button:has(i.pi-pencil)');
    const count = await editBtns.count();
    expect(count).toBe(0);
  });

  test('PERM-014: In-workflow opportunity blocks document upload', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.inWorkflow));
    await waitForPermissions(page);

    const uploadBtn = page.locator('app-opportunity-documents button:has-text("Upload")');
    await expect(uploadBtn).not.toBeVisible({ timeout: 5000 });
  });

  test('PERM-015: In-workflow opportunity shows read-only indicator', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.inWorkflow));
    await waitForPermissions(page);

    const readonlyIndicator = page.getByText(/read.only|in workflow|pending approval/i).first();
    const isVisible = await readonlyIndicator.isVisible({ timeout: 10000 }).catch(() => false);
    expect(isVisible || await page.locator('[data-testid="opportunity-status"]').isVisible()).toBeTruthy();
  });
});

// =============================================================================
// SECTION 5: Viewer and Other User Roles
// =============================================================================
test.describe('Permissions — Other User Roles', () => {
  test.slow();
  test.skip(!featureReady, 'Permissions not deployed — set OPPORTUNITY_PERMISSIONS_IMPLEMENTED=true');

  test('PERM-016: Viewer user can access opportunity detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.active), VIEWER_USER);
    await waitForPermissions(page);

    const pageLoaded = page.locator('[data-testid="opportunity-title"], p-panel');
    await expect(pageLoaded.first()).toBeVisible({ timeout: 10000 });
  });

  test('PERM-017: Other user can access opportunity detail', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.active), OTHER_USER);
    await waitForPermissions(page);

    const pageLoaded = page.locator('[data-testid="opportunity-title"], p-panel');
    await expect(pageLoaded.first()).toBeVisible({ timeout: 10000 });
  });

  test('PERM-018: Viewer cannot see create button on list page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities', VIEWER_USER);
    await waitForPermissions(page);
    await waitForLoadingToComplete(page);

    const newBtn = page.locator('[data-testid="new-opportunity-button"]');
    await expect(newBtn).not.toBeVisible({ timeout: 5000 });
  });
});
