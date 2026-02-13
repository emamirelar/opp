/**
 * @fileoverview Multi-Role Workflow E2E Tests
 * Tests for workflows involving different user roles.
 * 
 * Uses the existing mock auth system with different test user emails:
 * - Default (admin): test@playwright.local → Administrator role
 * - Restricted: test-readonly@playwright.local → UNOPS_GEN_USER (read-only)
 * - Viewer: viewer@example.com → UNOPS_GEN_USER (view-only)
 * 
 * All tests are EXECUTABLE with API mocks - no env gate needed.
 * The mock permission system returns different permissions per user role.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Multi-Role: Administrator Access', () => {
  test('Admin should see partner list with New Partner button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');
    
    // Admin should see the header
    const header = page.locator('[data-testid="partners-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Admin should see the New Partner button
    const newPartnerBtn = page.locator('[data-testid="new-partner-button"]').first();
    await expect(newPartnerBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see export and import buttons on partners page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners');
    
    const exportBtn = page.locator('[data-testid="export-button"]').first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
    
    const importBtn = page.locator('[data-testid="import-button"]').first();
    await expect(importBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see edit and delete buttons on partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners/1');
    
    const editBtn = page.locator('[data-testid="edit-partner-button"]').first();
    await expect(editBtn).toBeVisible({ timeout: 10000 });
    
    const deleteBtn = page.locator('[data-testid="delete-partner-button"]').first();
    await expect(deleteBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see all opportunity sections', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
    
    const header = page.locator('[data-testid="opportunity-detail-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Admin should see all sections
    const sectionIds = ['section-overview', 'section-what', 'section-why', 'section-who'];
    for (const sectionId of sectionIds) {
      const section = page.locator(`#${sectionId}`).first();
      await expect(section).toBeVisible({ timeout: 5000 });
    }
  });

  test('Admin should see contact list with New Contact button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts');
    
    const header = page.locator('[data-testid="contacts-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newContactBtn = page.locator('[data-testid="new-contact-button"]').first();
    await expect(newContactBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see interaction list with create buttons', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/interactions');
    
    const header = page.locator('[data-testid="interactions-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newIntBtn = page.locator('[data-testid="new-interaction-button"]').first();
    await expect(newIntBtn).toBeVisible({ timeout: 5000 });
    
    const createOppBtn = page.locator('[data-testid="create-opportunity-button"]').first();
    await expect(createOppBtn).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Multi-Role: Restricted User (View Only)', () => {
  test('Restricted user should NOT see New Partner button', async ({ page }) => {
    // Authenticate as restricted user - QA-039 fix ensures restricted permissions
    await authenticateWithRealBackend(page, '/#/partnerships/partners', 'test-readonly@playwright.local');
    
    // Page should load
    const header = page.locator('[data-testid="partners-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // New Partner button should NOT be visible for restricted user
    const newPartnerBtn = page.locator('[data-testid="new-partner-button"]').first();
    const btnVisible = await newPartnerBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });

  test('Restricted user should NOT see import button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners', 'test-readonly@playwright.local');
    
    const header = page.locator('[data-testid="partners-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const importBtn = page.locator('[data-testid="import-button"]').first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('Restricted user should NOT see edit/delete on partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/partners/1', 'test-readonly@playwright.local');
    
    // Partner detail should still load
    const header = page.locator('[data-testid="partner-detail-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Edit and delete buttons should NOT be visible
    const editBtn = page.locator('[data-testid="edit-partner-button"]').first();
    const deleteBtn = page.locator('[data-testid="delete-partner-button"]').first();
    
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    const deleteVisible = await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false);
    
    expect(editVisible).toBe(false);
    expect(deleteVisible).toBe(false);
  });

  test('Restricted user should NOT see New Contact button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts', 'test-readonly@playwright.local');
    
    const header = page.locator('[data-testid="contacts-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newContactBtn = page.locator('[data-testid="new-contact-button"]').first();
    const btnVisible = await newContactBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });

  test('Restricted user should NOT see edit/delete on contact detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/contacts/1', 'test-readonly@playwright.local');
    
    const header = page.locator('[data-testid="contact-detail-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const editBtn = page.locator('[data-testid="edit-contact-button"]').first();
    const deleteBtn = page.locator('[data-testid="delete-contact-button"]').first();
    
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    const deleteVisible = await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false);
    
    expect(editVisible).toBe(false);
    expect(deleteVisible).toBe(false);
  });
});

test.describe('Multi-Role: Admin vs Restricted Comparison', () => {
  test('Admin sees more buttons than restricted user on interactions page', async ({ page }) => {
    // First check admin
    await authenticateWithRealBackend(page, '/#/partnerships/interactions');
    
    const newIntBtnAdmin = page.locator('[data-testid="new-interaction-button"]').first();
    const adminBtnVisible = await newIntBtnAdmin.isVisible({ timeout: 10000 }).catch(() => false);
    expect(adminBtnVisible).toBeTruthy();
  });

  test('Restricted user has fewer buttons on interactions page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/interactions', 'test-readonly@playwright.local');
    
    const header = page.locator('[data-testid="interactions-header"]').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newIntBtn = page.locator('[data-testid="new-interaction-button"]').first();
    const btnVisible = await newIntBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });
});
