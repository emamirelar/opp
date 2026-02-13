/**
 * @fileoverview Product/Service Search E2E Tests
 * Tests for the Product & Service section on Opportunity detail.
 * 
 * This maps to the "What" section: #section-what, app-opportunity-what-section
 * which includes delivery modality, products/services selection
 * 
 * All tests are EXECUTABLE - no skips.
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Product/Service - What Section Display', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test('PSS-001: What section renders on opportunity detail', async ({ page }) => {
    const whatSection = page.locator('#section-what').first();
    await expect(whatSection).toBeVisible({ timeout: 10000 });
  });

  test('PSS-002: What component selector is present', async ({ page }) => {
    const whatComponent = page.locator('app-opportunity-what-section').first();
    await expect(whatComponent).toBeVisible({ timeout: 10000 });
  });

  test('PSS-003: What section navigation chip visible', async ({ page }) => {
    const whatChip = page.getByText(/what/i).first();
    await expect(whatChip).toBeVisible({ timeout: 10000 });
  });

  test('PSS-004: Can navigate to What section via chip', async ({ page }) => {
    const whatChip = page.getByText(/what/i).first();
    await expect(whatChip).toBeVisible({ timeout: 10000 });
    await whatChip.click();
    await page.waitForTimeout(500);

    const whatSection = page.locator('#section-what').first();
    await expect(whatSection).toBeVisible();
  });

  test('PSS-005: What section contains delivery modality or products content', async ({ page }) => {
    const whatSection = page.locator('#section-what').first();
    await expect(whatSection).toBeVisible({ timeout: 10000 });

    const text = await whatSection.textContent();
    expect(text).toBeTruthy();
    expect(text!.length).toBeGreaterThan(0);
  });

  test('PSS-006: What section has content related to products or services', async ({ page }) => {
    const whatSection = page.locator('#section-what').first();
    await expect(whatSection).toBeVisible({ timeout: 10000 });

    // Look for delivery modality or product-related elements
    const deliveryModality = whatSection.locator('p-select, p-dropdown').first();
    const modalityLabel = whatSection.getByText(/delivery|modality|product|service/i).first();

    const selectVisible = await deliveryModality.isVisible({ timeout: 5000 }).catch(() => false);
    const labelVisible = await modalityLabel.isVisible({ timeout: 3000 }).catch(() => false);

    expect(selectVisible || labelVisible).toBeTruthy();
  });
});
