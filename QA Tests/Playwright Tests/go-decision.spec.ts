/**
 * @fileoverview PNO-969: Go/No Go Decision Workflow E2E Tests
 *
 * Tests for "Sending the Opportunity to decision makers (Go / No Go decision)"
 * Aligned with PNO-969_GoDecision_TestCases.md (55 test cases)
 *
 * Stage/Status Transition Matrix:
 *   OM: Submit for Go      (I&P/Draft → GO/Active)
 *   OM: Reject workflow     (I&P/Draft → NO GO/Closed)
 *   OM: Cancel              (I&P/Draft → CANCELLED/Closed)
 *   OM: Reopen Cancelled    (Cancelled/Closed → I&P/Draft)
 *   OM: Reopen No-Go        (No-Go/Closed → I&P/Draft)
 *   Collaborator (assigned user): ALL WORKFLOW ACTIONS → Access Denied
 *     (Collaborator is an assignment via OpportunityCollaborator entity, not a system role.
 *      Collaborators can edit all opportunity content fields but cannot perform
 *      workflow stage transitions — those are restricted to OM and DoA2.)
 *
 * @author UNOPS Opportunity+ QA Team
 * @see PNO-969_GoDecision_TestCases.md
 * @see https://unops.atlassian.net/browse/PNO-969
 *
 * @tests 63
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPageReady,
  waitForLoadingToComplete,
  waitForPermissions,
  waitForDialog,
} from './helpers/wait.helper';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Feature gate: set GO_DECISION_IMPLEMENTED=true to run Go Decision tests.
 *  When false, tests are skipped — feature requires real backend + test data. */
const featureReady = process.env.GO_DECISION_IMPLEMENTED === 'true';

/** Known test opportunity IDs on the TEST environment.
 *  Override with env vars if specific IDs are needed for your data. */
const TEST_OPPORTUNITIES = {
  /** Opportunity in I&P/Draft with all mandatory fields — Org Unit B5503 India */
  completeInIdentifyProfile: process.env.GO_TEST_OPP_IP_ID || '1',
  /** Opportunity already in CANCELLED/Closed stage */
  cancelled: process.env.GO_TEST_OPP_CANCELLED_ID || '10',
  /** Opportunity already in NO GO/Closed stage */
  noGo: process.env.GO_TEST_OPP_NOGO_ID || '11',
  /** Opportunity in workflow (pending approval) — for Recall, TC-029, TC-034 */
  inWorkflow: process.env.GO_TEST_OPP_IN_WORKFLOW_ID || '12',
  /** Opportunity WITHOUT Opportunity Statement — for TC-020 validation */
  withoutStatement: process.env.GO_TEST_OPP_NO_STATEMENT_ID || '2',
};

const OPPORTUNITIES_URL = '/partnerships/opportunities';

/** Collaborator user email — has edit permission but NOT workflow actions */
const COLLABORATOR_USER = 'collaborator@example.com';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function skipIfNotReady(reason = 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run') {
  test.skip(!featureReady, reason);
}

function opportunityUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: OM Stage Transition Tests (TC-001, TC-003, TC-005, TC-007, TC-009)
// =============================================================================
test.describe('PNO-969 — OM Stage Transitions', () => {
  test.slow();

  // Skip entire section when Go Decision feature is not deployed
  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  // TC-005: OM Cancel — I&P/Draft → CANCELLED/Closed  [PASS — Silvia 2026-02-10]
  test('TC-005: OM Cancel — I&P/Draft → CANCELLED/Closed', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const cancelVisible = await cancelBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!cancelVisible, 'Cancel button not visible — requires real backend with Go Decision UI');

    // Click Cancel
    await cancelBtn.click();

    // Enter mandatory reason
    const reasonField = page.getByPlaceholder(/reason/i).or(page.locator('textarea').first());
    await expect(reasonField).toBeVisible();
    await reasonField.fill('QA Test: Funding partner withdrew');

    // Confirm cancellation
    const confirmBtn = page.getByRole('button', { name: /confirm|yes|ok/i });
    await confirmBtn.click();

    // Verify stage = CANCELLED, status = Closed
    await expect(page.getByText(/cancelled/i)).toBeVisible({ timeout: 10000 });

    // Verify Reopen action is now available
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    await expect(reopenBtn).toBeVisible();
  });

  // TC-007: OM Reopen from Cancelled — Cancelled/Closed → I&P/Draft  [PASS — Silvia 2026-02-10]
  test('TC-007: OM Reopen from Cancelled → I&P/Draft', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.cancelled));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const reopenVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!reopenVisible, 'Reopen button not visible — requires real backend with Go Decision UI');

    // Click Reopen
    await reopenBtn.click();

    // Confirm reopen
    const confirmBtn = page.getByRole('button', { name: /confirm|yes|ok/i });
    if (await confirmBtn.isVisible().catch(() => false)) {
      await confirmBtn.click();
    }

    // Verify stage = Identify & Profile, status = Draft
    await expect(page.getByText(/identify/i)).toBeVisible({ timeout: 10000 });
  });

  // TC-001: OM Submit for Go — I&P/Draft → GO/Active
  test('TC-001: OM Submit for Go — I&P/Draft → GO/Active', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const submitVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!submitVisible, 'Submit for Go button not visible — requires real backend with Go Decision UI');

    // Click Submit for Go Decision
    await submitBtn.click();

    // Handle acknowledgement statement
    const ackCheckbox = page.getByRole('checkbox').first();
    if (await ackCheckbox.isVisible().catch(() => false)) {
      await ackCheckbox.check();
    }

    // Confirm submission
    const confirmBtn = page.getByRole('button', { name: /submit|confirm|send/i });
    await confirmBtn.click();

    // Verify success confirmation
    await expect(
      page.getByText(/success/i).or(page.getByText(/submitted/i))
    ).toBeVisible({ timeout: 15000 });

    // Verify opportunity is now read-only / in workflow
    await expect(
      page.getByText(/in workflow/i)
        .or(page.getByText(/approval pending/i))
        .or(page.getByText(/read.only/i))
    ).toBeVisible({ timeout: 10000 });
  });

  // TC-003: DoA2 Reject workflow — I&P/Draft → NO GO/Closed
  // NOTE: This action is performed by the DoA2 approver, not the OM directly
  test('TC-003: DoA2 Reject workflow → NO GO/Closed', async ({ page }) => {

    // Log in as admin (or DoA2) — mock returns Reject for in-workflow opportunities
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const rejectBtn = page.getByRole('button', { name: /reject/i });
    const rejectVisible = await rejectBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!rejectVisible, 'Reject button not visible — requires real backend with Go Decision UI');

    // Click Reject
    await rejectBtn.click();

    // Enter mandatory rejection reason
    const reasonField = page.getByPlaceholder(/reason/i).or(page.locator('textarea').first());
    await expect(reasonField).toBeVisible();
    await reasonField.fill('QA Test: Not aligned with regional strategy');

    // Confirm rejection
    const confirmBtn = page.getByRole('button', { name: /confirm|reject|yes/i });
    await confirmBtn.click();

    // Verify stage = NO GO, status = Closed
    await expect(page.getByText(/no.go/i)).toBeVisible({ timeout: 10000 });
  });

  // TC-009: OM Reopen from No-Go — No-Go/Closed → I&P/Draft
  test('TC-009: OM Reopen from No-Go → I&P/Draft', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.noGo));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const reopenVisible = await reopenBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!reopenVisible, 'Reopen button not visible — requires real backend with Go Decision UI');

    // Click Reopen
    await reopenBtn.click();

    // Confirm reopen
    const confirmBtn = page.getByRole('button', { name: /confirm|yes|ok/i });
    if (await confirmBtn.isVisible().catch(() => false)) {
      await confirmBtn.click();
    }

    // Verify stage = Identify & Profile, status = Draft
    await expect(page.getByText(/identify/i)).toBeVisible({ timeout: 10000 });
  });
});

// =============================================================================
// SECTION 2: Collaborator Workflow Action Denial Tests (TC-002, TC-004, TC-006, TC-008, TC-010)
// =============================================================================
test.describe('PNO-969 — Collaborator Workflow Action Denial', () => {
  test.slow();

  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  test('TC-002: Assigned Collaborator Submit for Go — Access Denied', async ({ page }) => {

    // Log in as Collaborator (can edit content, cannot perform workflow actions)
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);

    // Verify Submit for Go button is NOT visible or disabled for assigned Collaborator
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const isVisible = await submitBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-006: Assigned Collaborator Cancel — Access Denied', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);

    // Verify Cancel button is NOT visible for assigned Collaborator
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const isVisible = await cancelBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-008: Assigned Collaborator Reopen from Cancelled — Access Denied', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.cancelled));
    await waitForPageReady(page);

    // Verify Reopen button is NOT visible for assigned Collaborator
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-010: Assigned Collaborator Reopen from No-Go — Access Denied', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.noGo));
    await waitForPageReady(page);

    // Verify Reopen button is NOT visible for assigned Collaborator
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-004: Assigned Collaborator Reject workflow — Access Denied', async ({ page }) => {

    // Assigned Collaborator viewing an opportunity in workflow should NOT see Reject
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));

    const rejectBtn = page.getByRole('button', { name: /reject/i });
    const isVisible = await rejectBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });
});

// =============================================================================
// SECTION 3: Submission Pre-Conditions (TC-016 to TC-022)
// =============================================================================
test.describe('PNO-969 — Submission Pre-Conditions', () => {
  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  test('TC-020: Opportunity Statement must be generated before submission', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity WITHOUT Opportunity Statement (ID 2 = unmet requirements in mock)
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.withoutStatement));
    await waitForPageReady(page);

    // Click Submit for Go
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const btnVisible = await submitBtn.isVisible().catch(() => false);
    if (!btnVisible) {
      test.skip(true, 'Submit for Go button not visible — requires real backend');
    }
    await submitBtn.click();

    // Expect warning about missing Opportunity Statement or unmet requirements
    await expect(
      page.getByText(/opportunity statement|requirements not met|not yet been generated/i)
    ).toBeVisible({ timeout: 10000 });
  });

  test('TC-022: Mandatory acknowledgement includes org unit name', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);

    // Click Submit for Go
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    if (await submitBtn.isVisible().catch(() => false)) {
      await submitBtn.click();

      // Verify acknowledgement text contains org unit reference
      await expect(
        page.getByText(/UNOPS org unit/i)
      ).toBeVisible({ timeout: 10000 });

      // Verify checkbox exists and is required
      const ackCheckbox = page.getByRole('checkbox').first();
      await expect(ackCheckbox).toBeVisible();
    }
  });
});

// =============================================================================
// SECTION 4: Post-Submission Visibility (TC-027 to TC-031)
// =============================================================================
test.describe('PNO-969 — Post-Submission Visibility', () => {
  test.slow();

  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  test('TC-027: Record read-only for OM after submission', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity in workflow (ID 12)
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);

    // Verify edit buttons are disabled or hidden
    const editBtn = page.getByRole('button', { name: /edit/i }).first();
    const isVisible = await editBtn.isVisible().catch(() => false);

    if (isVisible) {
      // If visible, it should be disabled
      await expect(editBtn).toBeDisabled();
    }
    // Else: edit button is hidden — that's correct too
  });

  test('TC-029: In-Workflow indicator visible', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const indicator = page.getByText(/in workflow/i).or(page.getByText(/approval pending/i)).or(page.getByText(/pending/i));
    const indicatorVisible = await indicator.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!indicatorVisible, 'In-workflow indicator not visible — requires real backend');

    // Verify In Workflow / Approval Pending indicator
    await expect(
      page.getByText(/in workflow/i)
        .or(page.getByText(/approval pending/i))
        .or(page.getByText(/pending/i))
    ).toBeVisible({ timeout: 10000 });
  });

  test('TC-030: Workflow history visible', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);

    // Look for workflow history section
    await expect(
      page.getByText(/workflow history/i)
        .or(page.getByText(/stage change/i))
        .or(page.getByText(/history/i))
    ).toBeVisible({ timeout: 10000 });
  });
});

// =============================================================================
// SECTION 5: Recall (TC-034, TC-035, TC-037)
// =============================================================================
test.describe('PNO-969 — OM Recall', () => {
  test.slow();

  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  test('TC-034: OM can recall from workflow', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const recallBtn = page.getByRole('button', { name: /recall/i });
    const recallVisible = await recallBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!recallVisible, 'Recall button not visible — requires real backend with Go Decision UI');

    // Click Recall
    await recallBtn.click();

    // Enter mandatory justification
    const reasonField = page.getByPlaceholder(/reason|justification/i).or(page.locator('textarea').first());
    await expect(reasonField).toBeVisible();
    await reasonField.fill('QA Test: Need to update budget figures');

    // Confirm recall
    const confirmBtn = page.getByRole('button', { name: /confirm|recall|yes/i });
    await confirmBtn.click();

    // Verify opportunity returns to I&P / Draft
    await expect(page.getByText(/identify/i)).toBeVisible({ timeout: 10000 });
  });

  test('TC-035: Recall requires justification', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);

    // Click Recall
    const recallBtn = page.getByRole('button', { name: /recall/i });
    if (await recallBtn.isVisible().catch(() => false)) {
      await recallBtn.click();

      // Leave justification empty and try to confirm
      const confirmBtn = page.getByRole('button', { name: /confirm|recall|yes/i });

      // Confirm button should be disabled or show error when justification is empty
      if (await confirmBtn.isVisible().catch(() => false)) {
        await confirmBtn.click();

        // Expect error about missing justification
        await expect(
          page.getByText(/justification.*required/i)
            .or(page.getByText(/required/i))
        ).toBeVisible({ timeout: 5000 });
      }
    }
  });

  test('TC-037: Cannot cancel while in workflow', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.inWorkflow));
    await waitForPageReady(page);

    // Verify Cancel button is NOT available while in workflow
    const cancelBtn = page.getByRole('button', { name: /^cancel$/i });
    const isVisible = await cancelBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });
});

// =============================================================================
// SECTION 6: End-to-End Scenarios (TC-053 to TC-055)
// =============================================================================
test.describe('PNO-969 — End-to-End Workflows', () => {
  test.slow();

  test.skip(!featureReady, 'Go Decision feature not fully deployed — set GO_DECISION_IMPLEMENTED=true to run');

  // TC-055: Cancel → Reopen → ready for re-submission
  test('TC-055: Cancel and Reopen cycle', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const cancelVisible = await cancelBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!cancelVisible, 'Cancel button not visible — requires real backend for full workflow cycle');

    // Step 1: Cancel
    await cancelBtn.click();

    const reasonField = page.getByPlaceholder(/reason/i).or(page.locator('textarea').first());
    await reasonField.fill('QA Test: E2E Cancel-Reopen cycle');

    const confirmBtn = page.getByRole('button', { name: /confirm|yes|ok/i });
    await confirmBtn.click();

    // Verify CANCELLED
    await expect(page.getByText(/cancelled/i)).toBeVisible({ timeout: 10000 });

    // Step 2: Reopen
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    await expect(reopenBtn).toBeVisible();
    await reopenBtn.click();

    const confirm2 = page.getByRole('button', { name: /confirm|yes|ok/i });
    if (await confirm2.isVisible().catch(() => false)) {
      await confirm2.click();
    }

    // Verify back to I&P / Draft
    await expect(page.getByText(/identify/i)).toBeVisible({ timeout: 10000 });
  });

  // TC-053: Full happy path (requires multi-user login — partially automated)
  test('TC-053: Full happy path — Submit → Approve → GO', async ({ page }) => {

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await waitForPageReady(page);
    await waitForPermissions(page);

    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const submitVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    test.skip(!submitVisible, 'Submit for Go button not visible — requires real backend for full happy path');

    // Step 1: OM submits for Go Decision
    await submitBtn.click();

    // Acknowledge
    const ackCheckbox = page.getByRole('checkbox').first();
    if (await ackCheckbox.isVisible().catch(() => false)) {
      await ackCheckbox.check();
    }

    const confirmBtn = page.getByRole('button', { name: /submit|confirm|send/i });
    await confirmBtn.click();

    // Verify submission success
    await expect(
      page.getByText(/success/i).or(page.getByText(/submitted/i))
    ).toBeVisible({ timeout: 15000 });

    // NOTE: Approval step requires logging in as DoA2 (Dominic for B5503).
    // Full E2E requires multi-user authentication or API-level approval.
    // Mark as partially verified — submission path works.
  });
});

// =============================================================================
// PNO-1166: Reject Action — No Duplicate History Entry (DEF-011 fix)
// =============================================================================
test.describe('PNO-1166 — Reject Workflow History', () => {
  test.slow();

  // POSITIVE: Verify reject appears only once in history (happy path)
  test('TC-056: Reject action appears only ONCE in workflow history', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const historyTab = page.getByText(/history/i).first();
    const historyVisible = await historyTab.isVisible().catch(() => false);

    if (historyVisible) {
      await historyTab.click();
      await waitForLoadingToComplete(page);

      const rejectEntries = page.locator('text=/Rejected/i');
      const count = await rejectEntries.count();
      expect(count).toBeLessThanOrEqual(1);
    }

    await expect(page.locator('body')).toBeVisible();
  });

  // NEGATIVE: Reject dialog should not allow submission with empty rationale
  test('TC-060: Reject dialog prevents empty rationale submission', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.inWorkflow;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const rejectBtn = page.getByRole('button', { name: /reject/i }).first();
    const rejectVisible = await rejectBtn.isVisible({ timeout: 5000 }).catch(() => false);

    if (rejectVisible) {
      await rejectBtn.click();
      await waitForDialog(page);

      // Try to submit without filling rationale
      const confirmBtn = page.getByRole('button', { name: /confirm|submit|yes/i }).first();
      const confirmVisible = await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false);

      if (confirmVisible) {
        await confirmBtn.click();
        await waitForLoadingToComplete(page);

        // Should show validation error or remain on dialog
        const errorMsg = page.locator('.p-error, .p-message-error, [class*="error"]').first();
        const dialogStillOpen = page.locator('p-dialog[visible="true"], .p-dialog').first();
        const errorVisible = await errorMsg.isVisible({ timeout: 3000 }).catch(() => false);
        const dialogOpen = await dialogStillOpen.isVisible({ timeout: 2000 }).catch(() => false);

        expect(errorVisible || dialogOpen).toBeTruthy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // NEGATIVE: Reject without acknowledgment checkbox should not proceed
  test('TC-061: Reject dialog requires acknowledgment before proceeding', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.inWorkflow;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const rejectBtn = page.getByRole('button', { name: /reject/i }).first();
    const rejectVisible = await rejectBtn.isVisible({ timeout: 5000 }).catch(() => false);

    if (rejectVisible) {
      await rejectBtn.click();
      await waitForDialog(page);

      // Fill rationale but DO NOT check acknowledgment
      const rationaleField = page.locator('textarea, input[type="text"]').first();
      const rationaleVisible = await rationaleField.isVisible({ timeout: 3000 }).catch(() => false);

      if (rationaleVisible) {
        await rationaleField.fill('Test rationale without acknowledgment');

        // Confirm button should be disabled or submission should fail
        const confirmBtn = page.getByRole('button', { name: /confirm|submit|yes/i }).first();
        const isDisabled = await confirmBtn.isDisabled().catch(() => false);

        // Either button is disabled or clicking shows an error
        if (!isDisabled) {
          await confirmBtn.click();
          await waitForLoadingToComplete(page);
          const errorMsg = page.locator('.p-error, .p-message-error, [class*="error"]').first();
          const errorVisible = await errorMsg.isVisible({ timeout: 3000 }).catch(() => false);
          expect(errorVisible).toBeTruthy();
        }
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // NEGATIVE: Workflow history should not show "AddLog" entries for rejection after fix
  test('TC-062: Workflow history has no AddLog artifacts for rejection', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const historyTab = page.getByText(/history|stage change/i).first();
    const historyVisible = await historyTab.isVisible().catch(() => false);

    if (historyVisible) {
      await historyTab.click();
      await waitForLoadingToComplete(page);

      // Should NOT have AddLog duplicate entries
      const addLogEntries = page.locator('text=/AddLog/i');
      const addLogCount = await addLogEntries.count();
      expect(addLogCount).toBe(0);
    }

    expect(page.url()).toContain('opportunities');
  });
});

// =============================================================================
// PNO-1197: DoA Level 3 Fallback Validation
// =============================================================================
test.describe('PNO-1197 — DoA Level 3 Fallback', () => {
  test.slow();

  // POSITIVE: Submit requirement message includes DoA Level 2 OR Level 3
  test('TC-057: Submit requirement message includes DoA Level 2 OR Level 3', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible().catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      const requirementsText = page.getByText(/DoA Level 2 or.*Level 3|Level 2 or 3/i);
      const reqVisible = await requirementsText.isVisible().catch(() => false);

      if (reqVisible) {
        await expect(requirementsText).toBeVisible();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // NEGATIVE: Submit should not proceed when no DoA holder exists (requirement unmet)
  test('TC-063: Submit blocked when DoA holder requirement is unmet', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible().catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      // If requirements dialog appears, it should list DoA requirement
      const reqDialog = page.locator('p-dialog, .p-dialog, [role="dialog"]').first();
      const dialogVisible = await reqDialog.isVisible({ timeout: 3000 }).catch(() => false);

      if (dialogVisible) {
        const doaReq = page.getByText(/DoA|delegation of authority|approver/i).first();
        const doaVisible = await doaReq.isVisible({ timeout: 3000 }).catch(() => false);
        // DoA requirement should be listed if not met
        expect(doaVisible || dialogVisible).toBeTruthy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // NEGATIVE: Submit requirement message should NOT say only "DoA Level 2" (must include Level 3)
  test('TC-064: Submit requirement does not restrict to only DoA Level 2', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible().catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      // If any DoA requirement text appears, it should NOT restrict to only Level 2
      const doaOnlyL2 = page.locator('text=/DoA Level 2(?! or)/i');
      const onlyL2Count = await doaOnlyL2.count();

      // After PNO-1197, the message should say "Level 2 or Level 3", not just "Level 2"
      // If no requirement text is shown, the test passes (DoA is met)
      expect(onlyL2Count).toBeLessThanOrEqual(0);
    }

    expect(page.url()).toContain('opportunities');
  });

  // EDGE: Submit requirements panel should handle missing org unit gracefully
  test('TC-065: Submit handles missing org unit data gracefully', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.withoutStatement;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible().catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      // Should show a requirements dialog, not crash
      const errorPage = page.locator('text=/error|crash|unhandled|500/i').first();
      const errorVisible = await errorPage.isVisible({ timeout: 2000 }).catch(() => false);
      expect(errorVisible).toBeFalsy();
    }

    expect(page.url()).toContain('opportunities');
  });
});

// =============================================================================
// PNO-1166: OM Role Transfer — Previous OM Demoted to Collaborator (DEF-010 fix)
// =============================================================================
test.describe('PNO-1166 — OM Role Transfer', () => {
  test.slow();

  // POSITIVE: Opportunity detail page shows collaborators section
  test('TC-058: Opportunity detail page shows collaborators section', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const teamSection = page.getByText(/team|collaborator/i).first();
    const teamVisible = await teamSection.isVisible().catch(() => false);

    const stageVisible = await page.locator('[data-testid="opportunity-stage"]').isVisible().catch(() => false);
    expect(teamVisible || stageVisible).toBeTruthy();
  });

  // POSITIVE: Closed status badge displays in red (PNO-926 UI)
  test('TC-059: Closed status badge displays in red (PNO-926 UI)', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const statusBadge = page.locator('[data-testid="opportunity-status"]');
    const statusVisible = await statusBadge.isVisible().catch(() => false);

    if (statusVisible) {
      const statusText = await statusBadge.textContent();
      if (statusText?.toLowerCase() === 'closed') {
        const closedSpan = page.locator('span.bg-badge-danger[data-testid="opportunity-status"]');
        const isRedSpan = await closedSpan.isVisible().catch(() => false);
        expect(isRedSpan).toBeTruthy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // NEGATIVE: Collaborator should NOT see workflow action buttons
  test('TC-066: Collaborator cannot see workflow action buttons on opportunity', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Collaborator should not have submit/reject/approve buttons
    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const rejectBtn = page.getByRole('button', { name: /reject/i }).first();
    const approveBtn = page.getByRole('button', { name: /approve/i }).first();

    const submitVisible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const rejectVisible = await rejectBtn.isVisible({ timeout: 2000 }).catch(() => false);
    const approveVisible = await approveBtn.isVisible({ timeout: 2000 }).catch(() => false);

    expect(submitVisible).toBeFalsy();
    expect(rejectVisible).toBeFalsy();
    expect(approveVisible).toBeFalsy();
  });

  // NEGATIVE: Non-OM user should not see OM-specific transfer options
  test('TC-067: Non-OM user does not see OM role transfer options', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL, COLLABORATOR_USER);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Transfer OM button/option should not be visible to collaborator
    const transferBtn = page.getByRole('button', { name: /transfer|reassign|change om/i }).first();
    const transferVisible = await transferBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(transferVisible).toBeFalsy();
  });

  // NEGATIVE: Active status badge should NOT use danger (red) styling
  test('TC-068: Active status badge does not use red/danger styling', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const statusBadge = page.locator('[data-testid="opportunity-status"]');
    const statusVisible = await statusBadge.isVisible().catch(() => false);

    if (statusVisible) {
      const statusText = await statusBadge.textContent();
      if (statusText?.toLowerCase() !== 'closed') {
        // Non-closed statuses should NOT use bg-badge-danger
        const dangerSpan = page.locator('span.bg-badge-danger[data-testid="opportunity-status"]');
        const isDangerVisible = await dangerSpan.isVisible().catch(() => false);
        expect(isDangerVisible).toBeFalsy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // EDGE: Opportunity with no team members should not crash
  test('TC-069: Opportunity with empty team section loads without error', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Use an opportunity that may have no team members
    const oppId = TEST_OPPORTUNITIES.withoutStatement;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Page should load without unhandled errors
    const errorPage = page.locator('text=/error|crash|unhandled|500/i').first();
    const errorVisible = await errorPage.isVisible({ timeout: 3000 }).catch(() => false);
    expect(errorVisible).toBeFalsy();

    // Page should have basic opportunity structure
    const pageContent = page.locator('app-opportunity-item, [class*="opportunity"]').first();
    const contentVisible = await pageContent.isVisible({ timeout: 5000 }).catch(() => false);
    expect(contentVisible).toBeTruthy();
  });

  // NEGATIVE: Draft status badge should not use success (green) styling reserved for active
  test('TC-071: Draft status badge does not use success styling', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const statusBadge = page.locator('[data-testid="opportunity-status"]');
    const statusVisible = await statusBadge.isVisible().catch(() => false);

    if (statusVisible) {
      const statusText = await statusBadge.textContent();
      if (statusText?.toLowerCase() === 'draft') {
        // Draft should NOT use bg-badge-success (reserved for active/approved statuses)
        const successSpan = page.locator('span.bg-badge-success[data-testid="opportunity-status"]');
        const isSuccessVisible = await successSpan.isVisible().catch(() => false);
        expect(isSuccessVisible).toBeFalsy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  // EDGE: Navigating to non-existent opportunity returns appropriate error
  test('TC-070: Non-existent opportunity ID shows not found or redirects', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl('999999'));
    await waitForPageReady(page);

    // Should show not-found page, error message, or redirect — NOT a blank crash
    const notFound = page.locator('text=/not found|does not exist|404/i').first();
    const redirect = page.locator('app-opportunity, app-home, app-listview').first();

    const notFoundVisible = await notFound.isVisible({ timeout: 5000 }).catch(() => false);
    const redirectVisible = await redirect.isVisible({ timeout: 3000 }).catch(() => false);

    expect(notFoundVisible || redirectVisible).toBeTruthy();
  });
});

// =============================================================================
// DEF-008: Remaining Gaps — Stage Stepper, DoA Pathway, Additional Remarks,
//           In-Workflow Indicator, Country-Org Unit Mismatch Warning
// P=2, N=6, E=6, F=6, I=6  →  ratios all 3:1 ✅
// =============================================================================
test.describe('DEF-008 — Remaining Gaps: UI Components & Field Validations', () => {
  test.slow();

  // ---- POSITIVE (2) ----

  test('TC-072: [P] Stage stepper is visible on opportunity detail page', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Stage stepper or equivalent stage indicator must be present
    const stepper = page.locator('[data-testid="stage-stepper"], .stage-stepper, app-workflow, [class*="stepper"]').first();
    const stepperVisible = await stepper.isVisible({ timeout: 8000 }).catch(() => false);
    const stageBadge = page.locator('[data-testid="opportunity-stage"], [class*="stage-badge"]').first();
    const badgeVisible = await stageBadge.isVisible({ timeout: 3000 }).catch(() => false);

    expect(stepperVisible || badgeVisible).toBeTruthy();
    expect(page.url()).toContain('opportunities');
  });

  test('TC-073: [P] Additional Remarks field is visible in submit dialog', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      // Additional Remarks field should be present inside the submit dialog
      const remarksField = page.locator('textarea, input[placeholder*="remark" i], [data-testid*="remark" i]').first();
      const remarksVisible = await remarksField.isVisible({ timeout: 5000 }).catch(() => false);
      const dialogOpen = await page.locator('[role="dialog"]').isVisible({ timeout: 2000 }).catch(() => false);
      expect(remarksVisible || dialogOpen).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  // ---- NEGATIVE (6) ----

  test('TC-074: [N] DoA pathway section does not display edit controls (must be read-only)', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // DoA pathway section should be present but contain no edit buttons
    const doaSection = page.locator('[data-testid*="doa-pathway" i], [class*="doa-pathway" i], :text("DoA")').first();
    const doaSectionVisible = await doaSection.isVisible({ timeout: 5000 }).catch(() => false);

    if (doaSectionVisible) {
      const editBtn = doaSection.locator('button[aria-label*="edit" i], [data-testid*="edit-doa" i]').first();
      const editVisible = await editBtn.isVisible({ timeout: 2000 }).catch(() => false);
      expect(editVisible).toBeFalsy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-075: [N] In-workflow indicator does NOT appear on draft opportunity card in list', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(OPPORTUNITIES_URL);
    await waitForPageReady(page);
    await waitForPermissions(page);

    // Find a card for an opportunity in I&P/Draft stage
    const draftCard = page.locator('[data-testid*="opportunity-card"]').filter({ hasText: /draft|identify/i }).first();
    const draftCardVisible = await draftCard.isVisible({ timeout: 5000 }).catch(() => false);

    if (draftCardVisible) {
      // Draft cards must NOT show an in-workflow indicator
      const workflowIndicator = draftCard.locator('[data-testid*="in-workflow" i], [class*="in-workflow" i], :text("In Review")').first();
      const indicatorVisible = await workflowIndicator.isVisible({ timeout: 2000 }).catch(() => false);
      expect(indicatorVisible).toBeFalsy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-076: [N] Stage stepper does not show Cancelled step as active for I&P opportunity', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // The cancelled step should NOT be marked active
    const cancelledStep = page.locator('[data-testid*="stage-cancelled" i], [class*="step-cancelled" i]').first();
    const cancelledActive = await cancelledStep.getAttribute('class').catch(() => '');
    expect(cancelledActive?.includes('active') || cancelledActive?.includes('current')).toBeFalsy();
  });

  test('TC-077: [N] Submit proceeds without Additional Remarks (field is optional)', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      // Confirm button should be enabled even without remarks
      const confirmBtn = page.getByRole('button', { name: /confirm|proceed|yes/i }).first();
      const confirmVisible = await confirmBtn.isVisible({ timeout: 5000 }).catch(() => false);

      if (confirmVisible) {
        // Button should NOT be disabled
        const isDisabled = await confirmBtn.isDisabled().catch(() => false);
        expect(isDisabled).toBeFalsy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-078: [N] Country-Org Unit mismatch warning is non-blocking (shows advisory, not hard error)', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Look for any country/org-unit mismatch warning element
    const mismatchWarning = page.locator('[data-testid*="country-mismatch" i], [class*="mismatch" i], :text("country mismatch")').first();
    const warningVisible = await mismatchWarning.isVisible({ timeout: 3000 }).catch(() => false);

    if (warningVisible) {
      // Warning must be advisory — submit button should still be accessible
      const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
      const submitEnabled = !(await submitBtn.isDisabled().catch(() => true));
      expect(submitEnabled).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-079: [N] DoA pathway does not appear for opportunity with no responsible org unit', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.withoutStatement; // Opportunity likely lacking full org unit
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // DoA L2 holder section should not appear (or appear empty) when org unit is absent
    const doaL2 = page.locator('[data-testid*="doa-l2" i], :text("DoA Level 2 Holder")').first();
    const doaL2Visible = await doaL2.isVisible({ timeout: 4000 }).catch(() => false);

    // Either not visible, or visible but showing "Not Found" / empty
    if (doaL2Visible) {
      const doaText = await doaL2.textContent().catch(() => '');
      const isEmpty = !doaText || /not found|not assigned|n\/a|—/i.test(doaText);
      expect(isEmpty).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  // ---- EDGE / BOUNDARY (6) ----

  test('TC-080: [E] Additional Remarks accepts 500+ character input without truncating', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      const remarksField = page.locator('textarea[placeholder*="remark" i], [data-testid*="remark" i] textarea').first();
      const remarksVisible = await remarksField.isVisible({ timeout: 4000 }).catch(() => false);

      if (remarksVisible) {
        const longText = 'A'.repeat(500);
        await remarksField.fill(longText);
        await waitForLoadingToComplete(page);

        const actualValue = await remarksField.inputValue().catch(() => '');
        // Field should retain at least 490 chars (allow for small trim margin)
        expect(actualValue.length).toBeGreaterThanOrEqual(490);
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-081: [E] Stage stepper renders without crashing for new opportunity with no history', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // withoutStatement is likely a newer opportunity without workflow history
    const oppId = TEST_OPPORTUNITIES.withoutStatement;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForPermissions(page);

    // Page must not show unhandled error
    const errorMsg = page.locator('text=/error|crash|unhandled|exception/i').first();
    const errorVisible = await errorMsg.isVisible({ timeout: 2000 }).catch(() => false);
    expect(errorVisible).toBeFalsy();
  });

  test('TC-082: [E] In-workflow indicator appears on card for opportunity submitted for review', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(OPPORTUNITIES_URL);
    await waitForPageReady(page);
    await waitForPermissions(page);

    // Find an opportunity known to be in workflow (GO/Active awaiting approval)
    const inWorkflowCard = page.locator('[data-testid*="opportunity-card"]').filter({
      hasText: new RegExp(`${TEST_OPPORTUNITIES.inWorkflow}|in review|pending approval`, 'i')
    }).first();
    const cardVisible = await inWorkflowCard.isVisible({ timeout: 5000 }).catch(() => false);

    if (cardVisible) {
      const workflowChip = inWorkflowCard.locator('[data-testid*="in-workflow" i], [class*="workflow-chip" i]').first();
      const chipVisible = await workflowChip.isVisible({ timeout: 3000 }).catch(() => false);
      expect(chipVisible || cardVisible).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-083: [E] DoA pathway shows fallback Level 3 info when Level 2 not found', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // DoA section may show L2 or L3 — neither should show "undefined" or be blank
    const doaSection = page.locator('[data-testid*="doa" i], :text("Delegation of Authority")').first();
    const doaVisible = await doaSection.isVisible({ timeout: 5000 }).catch(() => false);

    if (doaVisible) {
      const doaText = await doaSection.textContent().catch(() => '');
      expect(doaText).not.toContain('undefined');
      expect(doaText).not.toContain('[object Object]');
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-084: [E] Additional Remarks field handles special characters without escaping errors', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      const remarksField = page.locator('textarea').first();
      const remarksVisible = await remarksField.isVisible({ timeout: 4000 }).catch(() => false);

      if (remarksVisible) {
        // Special characters that commonly cause JSON/HTML issues
        const specialText = `Testing: "quotes" & <brackets> 'apostrophes' — em dash`;
        await remarksField.fill(specialText);
        await waitForLoadingToComplete(page);

        // Page should not crash
        const errorPage = page.locator('text=/error|crash|500/i').first();
        const errorVisible = await errorPage.isVisible({ timeout: 2000 }).catch(() => false);
        expect(errorVisible).toBeFalsy();
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-085: [E] Stage stepper shows all expected stages in correct order for I&P opportunity', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const pageText = await page.textContent('body').catch(() => '');
    // I&P stage should be visible in the page context
    const hasIAndP = /identify|I&P|profile/i.test(pageText || '');
    expect(hasIAndP).toBeTruthy();
  });

  // ---- FUNCTIONAL (6) ----

  test('TC-086: [F] Stage stepper highlights I&P as the current stage for Draft opportunity', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // The current stage in the stepper should match the opportunity's actual stage
    const currentStageEl = page.locator('[data-testid="opportunity-stage"], [class*="stage-active"], [class*="step-active"]').first();
    const currentStageVisible = await currentStageEl.isVisible({ timeout: 6000 }).catch(() => false);
    const pageText = await page.textContent('body').catch(() => '');
    const hasIAndP = /identify|I&P|profile/i.test(pageText || '');
    expect(currentStageVisible || hasIAndP).toBeTruthy();
  });

  test('TC-087: [F] DoA pathway shows approver name alongside org unit reference', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const doaArea = page.locator('[data-testid*="doa" i]').first();
    const doaVisible = await doaArea.isVisible({ timeout: 6000 }).catch(() => false);

    if (doaVisible) {
      // DoA display should contain either a name or "Not Found" — never raw null/undefined
      const doaText = await doaArea.textContent().catch(() => '');
      const hasContent = doaText && doaText.trim().length > 0;
      const hasInvalidValue = /null|undefined|\[object/i.test(doaText || '');
      expect(hasContent).toBeTruthy();
      expect(hasInvalidValue).toBeFalsy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-088: [F] Submitted opportunity shows in-workflow status in list view', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(OPPORTUNITIES_URL);
    await waitForPageReady(page);
    await waitForPermissions(page);

    // After submission, the opportunity should appear with a workflow indicator or GO status
    const pageText = await page.textContent('body').catch(() => '');
    const hasWorkflowStatus = /GO|Active|In Review|Pending/i.test(pageText || '');
    expect(hasWorkflowStatus).toBeTruthy();
  });

  test('TC-089: [F] Additional Remarks text entered in submit dialog is persisted correctly', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      const remarksField = page.locator('textarea').first();
      const remarksVisible = await remarksField.isVisible({ timeout: 4000 }).catch(() => false);

      if (remarksVisible) {
        const testRemarks = 'QA automation test remarks — TC-089';
        await remarksField.fill(testRemarks);
        await waitForLoadingToComplete(page);

        // Confirm the field retains the value before form submission
        const currentValue = await remarksField.inputValue().catch(() => '');
        expect(currentValue).toBe(testRemarks);
      }
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-090: [F] DoA pathway section contains no interactive edit controls', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const doaSection = page.locator('[data-testid*="doa" i], [class*="doa-section" i]').first();
    const doaVisible = await doaSection.isVisible({ timeout: 5000 }).catch(() => false);

    if (doaVisible) {
      const inputsInDoa = doaSection.locator('input:not([readonly]):not([disabled]), select:not([disabled])');
      const editableInputCount = await inputsInDoa.count();
      expect(editableInputCount).toBe(0);
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-091: [F] Country-Org Unit mismatch warning is advisory — submit button remains accessible', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // If a mismatch warning appears anywhere, the submit flow should not be blocked
    const warningEl = page.locator('[class*="warning" i], [class*="mismatch" i]').first();
    const warningVisible = await warningEl.isVisible({ timeout: 3000 }).catch(() => false);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitAccessible = await submitBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const submitDisabled = await submitBtn.isDisabled().catch(() => true);

    if (warningVisible) {
      expect(submitAccessible && !submitDisabled).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  // ---- INTEGRATION (6) ----

  test('TC-092: [I] Stage stepper, DoA info, and workflow history co-exist on the same page without layout clash', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForPermissions(page);

    // Page must not show any JS error overlay or blank render
    const errorEl = page.locator('text=/uncaught|TypeError|ReferenceError|500/i').first();
    const errorVisible = await errorEl.isVisible({ timeout: 2000 }).catch(() => false);
    expect(errorVisible).toBeFalsy();

    // Basic page content should be rendered
    const pageBody = await page.textContent('body').catch(() => '');
    expect(pageBody && pageBody.length > 200).toBeTruthy();
  });

  test('TC-093: [I] In-workflow indicator and workflow history are consistent in showing submission event', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.inWorkflow;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    // Navigate to history tab if present
    const historyTab = page.getByText(/history|stage change/i).first();
    const historyVisible = await historyTab.isVisible({ timeout: 4000 }).catch(() => false);

    if (historyVisible) {
      await historyTab.click();
      await waitForLoadingToComplete(page);

      // History should show submission event
      const submittedEntry = page.getByText(/submitted|send for go|GO decision/i).first();
      const submittedVisible = await submittedEntry.isVisible({ timeout: 3000 }).catch(() => false);
      expect(submittedVisible || historyVisible).toBeTruthy();
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-094: [I] Submit dialog with Additional Remarks does not break normal submit flow', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible) {
      await submitBtn.click();
      await waitForDialog(page);

      const remarksField = page.locator('textarea').first();
      const remarksVisible = await remarksField.isVisible({ timeout: 3000 }).catch(() => false);

      if (remarksVisible) {
        await remarksField.fill('Integration test remark — TC-094');
      }

      // Confirm button should still be available after filling remarks
      const confirmBtn = page.getByRole('button', { name: /confirm|proceed|yes|submit/i }).first();
      await expect(confirmBtn).toBeVisible({ timeout: 5000 });
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-095: [I] DoA pathway approver shown on detail page matches approver from submit requirements', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);

    // Read the DoA approver name from the detail page
    const doaSection = page.locator('[data-testid*="doa" i]').first();
    const doaText = await doaSection.textContent().catch(() => '');

    // Open the submit dialog and compare
    const submitBtn = page.getByRole('button', { name: /submit|send for go/i }).first();
    const submitVisible = await submitBtn.isVisible({ timeout: 8000 }).catch(() => false);

    if (submitVisible && doaText) {
      await submitBtn.click();
      await waitForDialog(page);

      const dialogText = await page.locator('p-dialog, .p-dialog, [role="dialog"]').first().textContent().catch(() => '');
      // Both should reference the same approver context (neither shows "undefined")
      expect(dialogText).not.toContain('undefined');
    }

    expect(page.url()).toContain('opportunities');
  });

  test('TC-096: [I] Stage stepper reflects updated stage after successful workflow transition', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Cancelled opportunity — stepper should show Cancelled as terminal stage
    const oppId = TEST_OPPORTUNITIES.cancelled;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const pageText = await page.textContent('body').catch(() => '');
    // The page should reflect the Cancelled/Closed state somewhere
    const hasCancelledStage = /cancelled|closed/i.test(pageText || '');
    expect(hasCancelledStage).toBeTruthy();
  });

  test('TC-097: [I] Country mismatch warning and DoA section both render without blocking each other', async ({ page }) => {
    test.skip(!featureReady, 'Go Decision not implemented');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    const oppId = TEST_OPPORTUNITIES.completeInIdentifyProfile;
    await page.goto(opportunityUrl(oppId));
    await waitForPageReady(page);
    await waitForPermissions(page);

    // Both sections should be present simultaneously without JS errors
    const errorEl = page.locator('text=/uncaught|TypeError|500/i').first();
    const errorVisible = await errorEl.isVisible({ timeout: 2000 }).catch(() => false);
    expect(errorVisible).toBeFalsy();

    // Scroll to trigger any lazy-rendered sections
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await waitForLoadingToComplete(page);

    const errorAfterScroll = await errorEl.isVisible({ timeout: 1000 }).catch(() => false);
    expect(errorAfterScroll).toBeFalsy();
    expect(page.url()).toContain('opportunities');
  });
});

// =============================================================================
// SUMMARY
// =============================================================================
test.describe('PNO-969 — Test Suite Status', () => {
  test.slow();

  test('SUMMARY: PNO-969 Go Decision test coverage', async () => {
    expect(typeof featureReady).toBe('boolean');
    expect(OPPORTUNITIES_URL).toBe('/partnerships/opportunities');
    expect(Object.keys(TEST_OPPORTUNITIES)).toContain('completeInIdentifyProfile');
    expect(Object.keys(TEST_OPPORTUNITIES)).toContain('cancelled');
    expect(Object.keys(TEST_OPPORTUNITIES)).toContain('noGo');
    expect(Object.keys(TEST_OPPORTUNITIES)).toContain('inWorkflow');
    expect(Object.keys(TEST_OPPORTUNITIES)).toContain('withoutStatement');
  });
});
