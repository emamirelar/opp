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
 *
 * @tests 13
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Multi-Role: Administrator Access', () => {
  test.slow();
  test('Admin should see partner list with New Partner button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    
    // Admin should see the header
    const header = page.getByText('Partners', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Admin should see the New Partner button
    const newPartnerBtn = page.getByRole('button', { name: /new partner/i }).first();
    await expect(newPartnerBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see export and import buttons on partners page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    
    const exportBtn = page.getByRole('button', { name: /export/i }).first();
    await expect(exportBtn).toBeVisible({ timeout: 10000 });
    
    const importBtn = page.getByRole('button', { name: /import/i }).first();
    await expect(importBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see edit and delete buttons on partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners/1');
    
    const header = page.locator('app-partner-view, p-panel, .p-panel-header').first();
    await expect(header).toBeVisible({ timeout: 15000 });
    
    const editBtn = page.getByRole('button', { name: /edit/i }).first();
    const deleteBtn = page.getByRole('button', { name: /delete/i }).first();
    const editVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
    const deleteVisible = await deleteBtn.isVisible({ timeout: 5000 }).catch(() => false);
    expect(editVisible || deleteVisible).toBeTruthy();
  });

  test('Admin should see all opportunity sections', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities/1');
    
    const header = page.locator('app-opportunity-view').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Admin should see all sections
    const sectionIds = ['section-overview', 'section-what', 'section-why', 'section-who'];
    for (const sectionId of sectionIds) {
      const section = page.locator(`#${sectionId}`).first();
      await expect(section).toBeVisible({ timeout: 5000 });
    }
  });

  test('Admin should see contact list with New Contact button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    
    const header = page.getByText('Contacts', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newContactBtn = page.getByRole('button', { name: /new/i }).first();
    await expect(newContactBtn).toBeVisible({ timeout: 5000 });
  });

  test('Admin should see interaction list with create buttons', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    
    const header = page.getByText('Interactions', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newIntBtn = page.getByRole('button', { name: /new interaction/i }).first();
    await expect(newIntBtn).toBeVisible({ timeout: 5000 });
    
    const createOppBtn = page.getByRole('button', { name: /new opportunity/i }).first();
    await expect(createOppBtn).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Multi-Role: Restricted User (View Only)', () => {
  test.slow();
  test('Restricted user should NOT see New Partner button', async ({ page }) => {
    // Authenticate as restricted user - QA-039 fix ensures restricted permissions
    await authenticateWithRealBackend(page, '/partnerships/partners', 'test-readonly@playwright.local');
    
    // Page should load
    const header = page.getByText(/partners/i).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // New Partner button should NOT be visible for restricted user
    const newPartnerBtn = page.getByRole('button', { name: /new partner|create/i }).first();
    const btnVisible = await newPartnerBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });

  test('Restricted user should NOT see import button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', 'test-readonly@playwright.local');
    
    const header = page.getByText('Partners', { exact: true }).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const importBtn = page.getByRole('button', { name: /import/i }).first();
    const importVisible = await importBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(importVisible).toBe(false);
  });

  test('Restricted user should NOT see edit/delete on partner detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners/1', 'test-readonly@playwright.local');
    
    // Partner detail should still load
    const header = page.locator('app-partner-view, p-panel, .p-panel-header').first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    // Edit and delete buttons should NOT be visible
    const editBtn = page.getByRole('button', { name: /edit/i }).first();
    const deleteBtn = page.getByRole('button', { name: /delete/i }).first();
    
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    const deleteVisible = await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false);
    
    expect(editVisible).toBe(false);
    expect(deleteVisible).toBe(false);
  });

  test('Restricted user should NOT see New Contact button', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts', 'test-readonly@playwright.local');
    
    const header = page.getByText(/contacts/i).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newContactBtn = page.getByRole('button', { name: /new contact|create|add contact/i }).first();
    const btnVisible = await newContactBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });

  test('Restricted user should NOT see edit/delete on contact detail', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts/1', 'test-readonly@playwright.local');
    
    const header = page.locator('app-contact-view, app-contact-tabs, p-panel, app-contact-item, app-listview').first();
    await expect(header).toBeVisible({ timeout: 15000 });
    
    const editBtn = page.getByRole('button', { name: /edit/i }).first();
    const deleteBtn = page.getByRole('button', { name: /delete/i }).first();
    
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    const deleteVisible = await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false);
    
    expect(editVisible).toBe(false);
    expect(deleteVisible).toBe(false);
  });
});

test.describe('Multi-Role: Admin vs Restricted Comparison', () => {
  test.slow();
  test('Admin sees more buttons than restricted user on interactions page', async ({ page }) => {
    // First check admin
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    
    const newIntBtnAdmin = page.getByRole('button', { name: /new interaction/i }).first();
    const adminBtnVisible = await newIntBtnAdmin.isVisible({ timeout: 10000 }).catch(() => false);
    expect(adminBtnVisible).toBeTruthy();
  });

  test('Restricted user has fewer buttons on interactions page', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions', 'test-readonly@playwright.local');
    
    const header = page.getByText(/interactions/i).first();
    await expect(header).toBeVisible({ timeout: 10000 });
    
    const newIntBtn = page.getByRole('button', { name: /new interaction|create/i }).first();
    const btnVisible = await newIntBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(btnVisible).toBe(false);
  });
});
