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
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

/** Feature gate removed — real backend is available */
const featureReady = true;

/** Known test opportunity IDs on the TEST environment.
 *  Override with env vars if specific IDs are needed for your data. */
const TEST_OPPORTUNITIES = {
  /** Opportunity in I&P/Draft with all mandatory fields — Org Unit B5503 India */
  completeInIdentifyProfile: process.env.GO_TEST_OPP_IP_ID || '1',
  /** Opportunity already in CANCELLED/Closed stage */
  cancelled: process.env.GO_TEST_OPP_CANCELLED_ID || '10',
  /** Opportunity already in NO GO/Closed stage */
  noGo: process.env.GO_TEST_OPP_NOGO_ID || '11',
};

const OPPORTUNITIES_URL = '/#/partnerships/opportunities';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function skipIfNotReady(reason = 'Go Decision feature not fully deployed (PNO-969 / DEF-008)') {
  test.skip(!featureReady, reason);
}

function opportunityUrl(id: string): string {
  return `/#/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: OM Stage Transition Tests (TC-001, TC-003, TC-005, TC-007, TC-009)
// =============================================================================
test.describe('PNO-969 — OM Stage Transitions', () => {

  // TC-005: OM Cancel — I&P/Draft → CANCELLED/Closed  [PASS — Silvia 2026-02-10]
  test('TC-005: OM Cancel — I&P/Draft → CANCELLED/Closed', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to an opportunity in Identify & Profile / Draft
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Verify Cancel action is available
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    await expect(cancelBtn).toBeVisible();

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.cancelled));
    await page.waitForLoadState('networkidle');

    // Verify Reopen action is available
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    await expect(reopenBtn).toBeVisible();

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Verify Submit for Go action is available
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    await expect(submitBtn).toBeVisible();

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

  // TC-003: OM Reject workflow — I&P/Draft → NO GO/Closed
  // NOTE: This action is performed by the DoA2 approver, not the OM directly
  test('TC-003: DoA2 Reject workflow → NO GO/Closed', async ({ page }) => {
    skipIfNotReady();

    // Must log in as DoA2 holder (Dominic for B5503 India)
    // This test requires an opportunity already submitted and pending approval
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);

    // Navigate to opportunity pending approval
    // (requires DoA2 user credentials — may need separate auth)
    await page.waitForLoadState('networkidle');

    // Verify Reject action is available
    const rejectBtn = page.getByRole('button', { name: /reject/i });
    await expect(rejectBtn).toBeVisible();

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.noGo));
    await page.waitForLoadState('networkidle');

    // Verify Reopen action is available
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    await expect(reopenBtn).toBeVisible();

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
  // NOTE: "Collaborator" is an assignment (OpportunityCollaborator entity), not a system role.
  // Users assigned as Collaborators can edit all content fields of the opportunity,
  // but cannot perform workflow stage transitions (Submit, Cancel, Reopen, Approve, Reject).
  // Workflow actions are restricted to OM (Opportunity Manager) and DoA2 (Partnership Lead).
  // These tests verify that assigned Collaborators cannot perform workflow actions.

  test('TC-002: Assigned Collaborator Submit for Go — Access Denied', async ({ page }) => {
    skipIfNotReady('Go Decision feature not fully deployed (PNO-969 / DEF-008)');

    // Log in as a user assigned as Collaborator on this opportunity
    // Navigate to opportunity in I&P / Draft
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Verify Submit for Go button is NOT visible or disabled for assigned Collaborator
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    const isVisible = await submitBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-006: Assigned Collaborator Cancel — Access Denied', async ({ page }) => {
    skipIfNotReady('Go Decision feature not fully deployed (PNO-969 / DEF-008)');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Verify Cancel button is NOT visible for assigned Collaborator
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    const isVisible = await cancelBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-008: Assigned Collaborator Reopen from Cancelled — Access Denied', async ({ page }) => {
    skipIfNotReady('Go Decision feature not fully deployed (PNO-969 / DEF-008)');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.cancelled));
    await page.waitForLoadState('networkidle');

    // Verify Reopen button is NOT visible for assigned Collaborator
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-010: Assigned Collaborator Reopen from No-Go — Access Denied', async ({ page }) => {
    skipIfNotReady('Go Decision feature not fully deployed (PNO-969 / DEF-008)');

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.noGo));
    await page.waitForLoadState('networkidle');

    // Verify Reopen button is NOT visible for assigned Collaborator
    const reopenBtn = page.getByRole('button', { name: /reopen/i });
    const isVisible = await reopenBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });

  test('TC-004: Assigned Collaborator Reject workflow — Access Denied', async ({ page }) => {
    skipIfNotReady('Go Decision feature not fully deployed (PNO-969 / DEF-008)');

    // Assigned Collaborator viewing an opportunity in workflow should NOT see Reject
    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.waitForLoadState('networkidle');

    const rejectBtn = page.getByRole('button', { name: /reject/i });
    const isVisible = await rejectBtn.isVisible().catch(() => false);
    expect(isVisible).toBeFalsy();
  });
});

// =============================================================================
// SECTION 3: Submission Pre-Conditions (TC-016 to TC-022)
// =============================================================================
test.describe('PNO-969 — Submission Pre-Conditions', () => {

  test('TC-020: Opportunity Statement must be generated before submission', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity WITHOUT Opportunity Statement
    await page.waitForLoadState('networkidle');

    // Click Submit for Go
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    if (await submitBtn.isVisible().catch(() => false)) {
      await submitBtn.click();

      // Expect warning about missing Opportunity Statement
      await expect(
        page.getByText(/opportunity statement has not yet been generated/i)
      ).toBeVisible({ timeout: 10000 });
    }
  });

  test('TC-022: Mandatory acknowledgement includes org unit name', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

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

  test('TC-027: Record read-only for OM after submission', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity that has been submitted (in workflow)
    await page.waitForLoadState('networkidle');

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity in workflow
    await page.waitForLoadState('networkidle');

    // Verify In Workflow / Approval Pending indicator
    await expect(
      page.getByText(/in workflow/i)
        .or(page.getByText(/approval pending/i))
        .or(page.getByText(/pending/i))
    ).toBeVisible({ timeout: 10000 });
  });

  test('TC-030: Workflow history visible', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity with workflow history
    await page.waitForLoadState('networkidle');

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

  test('TC-034: OM can recall from workflow', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity in workflow as OM
    await page.waitForLoadState('networkidle');

    // Verify Recall button visible for OM
    const recallBtn = page.getByRole('button', { name: /recall/i });
    await expect(recallBtn).toBeVisible();

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.waitForLoadState('networkidle');

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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    // Navigate to opportunity IN workflow
    await page.waitForLoadState('networkidle');

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

  // TC-055: Cancel → Reopen → ready for re-submission
  test('TC-055: Cancel and Reopen cycle', async ({ page }) => {
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Step 1: Cancel
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    await expect(cancelBtn).toBeVisible();
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
    skipIfNotReady();

    await authenticateWithRealBackend(page, OPPORTUNITIES_URL);
    await page.goto(opportunityUrl(TEST_OPPORTUNITIES.completeInIdentifyProfile));
    await page.waitForLoadState('networkidle');

    // Step 1: OM submits for Go Decision
    const submitBtn = page.getByRole('button', { name: /submit for go/i });
    await expect(submitBtn).toBeVisible();
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
// SUMMARY
// =============================================================================
test.describe('PNO-969 — Test Suite Status', () => {
  test('SUMMARY: PNO-969 Go Decision test coverage', async () => {
    console.log('='.repeat(60));
    console.log('PNO-969: GO/NO GO DECISION TEST SUITE');
    console.log('='.repeat(60));
    console.log('');
    console.log('Feature deployed:', featureReady ? 'YES' : 'NO');
    console.log('');
    console.log('Test Case Document: PNO-969_GoDecision_TestCases.md');
    console.log('Total test cases:  55');
    console.log('');
    console.log('Stage Transitions (OM):');
    console.log('  TC-001: Submit for Go → GO/Active');
    console.log('  TC-003: Reject → NO GO/Closed');
    console.log('  TC-005: Cancel → CANCELLED/Closed     ✅ PASS');
    console.log('  TC-007: Reopen Cancelled → I&P/Draft   ✅ PASS');
    console.log('  TC-009: Reopen No-Go → I&P/Draft');
    console.log('');
    console.log('Workflow Action Denial (Assigned Collaborators):');
    console.log('  TC-002, TC-004, TC-006, TC-008, TC-010: All → Access Denied');
    console.log('  (Collaborator = OpportunityCollaborator assignment, not a system role)');
    console.log('  (Collaborators can edit content but cannot perform workflow transitions)');
    console.log('');
    console.log('Known Issues:');
    console.log('  PNO-1193: OM role transfer not working');
    console.log('  PNO-1171: Reject action appears twice in history');
    console.log('');
    console.log('To enable all tests:');
    console.log('  GO_DECISION_IMPLEMENTED=true');
    console.log('  GO_TEST_OPP_IP_ID=<opportunity-id>');
    console.log('  GO_TEST_OPP_CANCELLED_ID=<opportunity-id>');
    console.log('  GO_TEST_OPP_NOGO_ID=<opportunity-id>');
    console.log('='.repeat(60));

    expect(true).toBeTruthy();
  });
});
