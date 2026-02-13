/**
 * @fileoverview Document Management E2E Tests
 * Tests for document upload, list, and management on entity detail pages.
 * 
 * Uses data-testid selectors from partner/contact/opportunity views:
 * - data-testid="partner-documents-section"
 * - data-testid="contact-documents-section"  
 * - data-testid="upload-document-button"
 * - app-document, app-upload-document, app-opportunity-documents
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Document Management - Partner Documents', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners/1');
  });

  test('DOC-001: Documents section visible on partner detail', async ({ page }) => {
    const docsSection = page.locator('[data-testid="partner-documents-section"]').first();
    await expect(docsSection).toBeVisible({ timeout: 10000 });
  });

  test('DOC-002: Upload document button is visible', async ({ page }) => {
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    await expect(uploadBtn).toBeVisible({ timeout: 10000 });
  });

  test('DOC-003: Upload button opens upload dialog', async ({ page }) => {
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    await expect(uploadBtn).toBeVisible({ timeout: 10000 });
    await uploadBtn.click();
    await page.waitForTimeout(1000);

    // Upload dialog should open
    const dialog = page.locator('[role="dialog"], app-upload-document p-dialog').first();
    await expect(dialog).toBeVisible({ timeout: 5000 });
  });

  test('DOC-004: Upload dialog has document type selector', async ({ page }) => {
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    await expect(uploadBtn).toBeVisible({ timeout: 10000 });
    await uploadBtn.click();
    await page.waitForTimeout(1000);

    const dialog = page.locator('[role="dialog"]').first();
    await expect(dialog).toBeVisible({ timeout: 5000 });

    // Dialog should have a document type selector
    const typeSelector = dialog.locator('p-select, p-dropdown, select').first();
    const typeSelectorVisible = await typeSelector.isVisible({ timeout: 3000 }).catch(() => false);
    expect(typeSelectorVisible).toBeTruthy();
  });

  test('DOC-005: Upload dialog can be closed', async ({ page }) => {
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    await expect(uploadBtn).toBeVisible({ timeout: 10000 });
    await uploadBtn.click();
    await page.waitForTimeout(1000);

    const dialog = page.locator('[role="dialog"]').first();
    await expect(dialog).toBeVisible({ timeout: 5000 });

    // Close with Escape
    await page.keyboard.press('Escape');
    await page.waitForTimeout(500);

    const dialogAfter = page.locator('[role="dialog"]').first();
    const stillVisible = await dialogAfter.isVisible({ timeout: 1000 }).catch(() => false);
    expect(stillVisible).toBe(false);
  });

  test('DOC-006: Documents section has content', async ({ page }) => {
    const docsSection = page.locator('[data-testid="partner-documents-section"]').first();
    await expect(docsSection).toBeVisible({ timeout: 10000 });

    const text = await docsSection.textContent();
    expect(text).toBeTruthy();
    expect(text!.length).toBeGreaterThan(0);
  });
});

test.describe('Document Management - Contact Documents', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts/1');
  });

  test('DOC-007: Documents section visible on contact detail', async ({ page }) => {
    const docsSection = page.locator('[data-testid="contact-documents-section"]').first();
    await expect(docsSection).toBeVisible({ timeout: 10000 });
  });

  test('DOC-008: Upload document button visible on contact', async ({ page }) => {
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    await expect(uploadBtn).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Document Management - Opportunity Documents', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test('DOC-009: Opportunity has documents panel', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents').first();
    await expect(docsPanel).toBeVisible({ timeout: 10000 });
  });

  test('DOC-010: Opportunity documents panel has upload capability', async ({ page }) => {
    const docsPanel = page.locator('app-opportunity-documents').first();
    await expect(docsPanel).toBeVisible({ timeout: 10000 });

    // Look for upload button or link within the documents panel
    const uploadBtn = docsPanel.locator('button, a').filter({ hasText: /upload|add|browse/i }).first();
    const uploadVisible = await uploadBtn.isVisible({ timeout: 5000 }).catch(() => false);

    // Upload functionality should exist
    expect(uploadVisible).toBeTruthy();
  });
});

test.describe('Document Management - Restricted User', () => {
  test('DOC-011: Restricted user cannot upload documents on partner', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners/1', 'test-readonly@playwright.local');

    const header = page.locator('[data-testid="partner-detail-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });

    // Upload button should NOT be visible for restricted user
    const uploadBtn = page.locator('[data-testid="upload-document-button"]').first();
    const uploadVisible = await uploadBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(uploadVisible).toBe(false);
  });
});
