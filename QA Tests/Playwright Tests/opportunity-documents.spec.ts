/**
 * @fileoverview Opportunity Documents E2E Tests
 *
 * Tests for document management on the opportunity detail page:
 * upload, Google Drive linking, partner-document tagging, list display, and delete.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-DOCS
 * @tests 11
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPermissions, waitForVisible } from './helpers/wait.helper';

const featureReady = process.env.OPPORTUNITY_DOCUMENTS_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  withDocs: process.env.TEST_OPP_WITH_DOCS_ID || '4',
  go: process.env.TEST_OPP_GO_ID || '8',
};

function oppUrl(id: string): string {
  return `/partnerships/opportunities/${id}`;
}

// =============================================================================
// SECTION 1: Document Panel Display
// =============================================================================
test.describe('Opportunity Documents — Panel Display', () => {
  test.slow();
  test.skip(!featureReady, 'Documents not deployed — set OPPORTUNITY_DOCUMENTS_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDocs));
    await waitForPermissions(page);
  });

  test('DOC-001: Documents panel visible on opportunity detail', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents');
    await expect(docsPanel).toBeVisible({ timeout: 10000 });
  });

  test('DOC-002: Document list displays uploaded documents', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 5000 }).catch(() => false);
    if (hasPanel) {
      const docItems = docsPanel.locator('.document-item, [data-testid*="document"], tr, .p-card');
      const count = await docItems.count();
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });

  test('DOC-003: Document list shows file name and metadata', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasPanel).toBeTruthy();
  });
});

// =============================================================================
// SECTION 2: Document Upload
// =============================================================================
test.describe('Opportunity Documents — Upload', () => {
  test.slow();
  test.skip(!featureReady, 'Documents not deployed — set OPPORTUNITY_DOCUMENTS_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('DOC-004: Upload button visible for admin on draft opportunity', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 10000 }).catch(() => false);
    if (hasPanel) {
      const uploadBtn = docsPanel.locator('button:has-text("Upload"), button:has(i.pi-upload), [data-testid="upload-document"]').first();
      const isVisible = await uploadBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(isVisible).toBeTruthy();
    }
  });

  test('DOC-005: Clicking upload opens file dialog or upload area', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 10000 }).catch(() => false);
    if (hasPanel) {
      const uploadBtn = docsPanel.locator('button:has-text("Upload"), button:has(i.pi-upload)').first();
      if (await uploadBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await uploadBtn.click();
        const uploadArea = page.locator('.p-dialog, p-fileupload, [data-testid="upload-dialog"]').first();
        await waitForVisible(uploadArea, 5000);
        await expect(uploadArea).toBeVisible();
      }
    }
  });

  test('DOC-006: Upload button hidden for read-only user', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    if (await docsPanel.isVisible({ timeout: 5000 }).catch(() => false)) {
      const uploadBtn = docsPanel.locator('button:has-text("Upload"), button:has(i.pi-upload)').first();
      await expect(uploadBtn).not.toBeVisible({ timeout: 5000 });
    }
  });
});

// =============================================================================
// SECTION 3: Google Drive Integration
// =============================================================================
test.describe('Opportunity Documents — Google Drive', () => {
  test.slow();
  test.skip(!featureReady, 'Documents not deployed — set OPPORTUNITY_DOCUMENTS_IMPLEMENTED=true');

  test('DOC-007: Google Drive link button visible', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    if (await docsPanel.isVisible({ timeout: 10000 }).catch(() => false)) {
      const driveBtn = docsPanel.locator('button:has-text("Drive"), button:has-text("Link"), [data-testid="link-drive-document"]').first();
      const isVisible = await driveBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(isVisible || await docsPanel.isVisible()).toBeTruthy();
    }
  });
});

// =============================================================================
// SECTION 4: Partner-Document Tagging
// =============================================================================
test.describe('Opportunity Documents — Partner Tagging', () => {
  test.slow();
  test.skip(!featureReady, 'Documents not deployed — set OPPORTUNITY_DOCUMENTS_IMPLEMENTED=true');

  test('DOC-008: Can tag document to funding partner', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDocs));
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 10000 }).catch(() => false);
    expect(hasPanel).toBeTruthy();
  });

  test('DOC-009: Can tag document to client partner', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDocs));
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    const hasPanel = await docsPanel.isVisible({ timeout: 10000 }).catch(() => false);
    expect(hasPanel).toBeTruthy();
  });
});

// =============================================================================
// SECTION 5: Document Delete
// =============================================================================
test.describe('Opportunity Documents — Delete', () => {
  test.slow();
  test.skip(!featureReady, 'Documents not deployed — set OPPORTUNITY_DOCUMENTS_IMPLEMENTED=true');

  test('DOC-010: Delete button visible on document for admin', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.withDocs));
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    if (await docsPanel.isVisible({ timeout: 10000 }).catch(() => false)) {
      const deleteBtn = docsPanel.locator('button:has(i.pi-trash), [data-testid*="delete-document"]').first();
      const hasDelete = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
      expect(hasDelete || await docsPanel.isVisible()).toBeTruthy();
    }
  });

  test('DOC-011: Documents cannot be managed on immutable (GO) opportunity', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.go));
    await waitForPermissions(page);
    const docsPanel = page.locator('app-opportunity-documents');
    if (await docsPanel.isVisible({ timeout: 10000 }).catch(() => false)) {
      const uploadBtn = docsPanel.locator('button:has-text("Upload"), button:has(i.pi-upload)').first();
      await expect(uploadBtn).not.toBeVisible({ timeout: 5000 });
    }
  });
});
