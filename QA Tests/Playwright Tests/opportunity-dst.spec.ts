/**
 * @fileoverview Opportunity DST (Decision Support Tool) Section E2E Tests
 * Tests the DST/Analysis section on the Opportunity detail page.
 * 
 * Covers scenarios: OPP-051 to OPP-057
 * 
 * Uses API mocks - fully executable.
 * 
 * Actual selectors:
 * - Analysis: #section-analysis, app-opportunity-analysis-section
 * - DST/Risks: #section-risks, app-opportunity-dst-section
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Opportunity DST / Analysis Section', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test('OPP-051: Analysis section renders on opportunity detail', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis').first();
    await expect(analysisSection).toBeVisible({ timeout: 10000 });
    
    const analysisComponent = page.locator('app-opportunity-analysis-section').first();
    await expect(analysisComponent).toBeVisible({ timeout: 5000 });
  });

  test('OPP-052: Analysis section navigation chip is visible', async ({ page }) => {
    const analysisChip = page.getByText(/analysis/i).first();
    await expect(analysisChip).toBeVisible({ timeout: 10000 });
  });

  test('OPP-053: Can navigate to analysis section via chip', async ({ page }) => {
    const analysisChip = page.getByText(/analysis/i).first();
    await expect(analysisChip).toBeVisible({ timeout: 10000 });
    await analysisChip.click();
    await page.waitForTimeout(500);
    
    const analysisSection = page.locator('#section-analysis').first();
    await expect(analysisSection).toBeVisible();
  });

  test('OPP-054: Analysis section contains content', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis').first();
    await expect(analysisSection).toBeVisible({ timeout: 10000 });
    
    const sectionText = await analysisSection.textContent();
    expect(sectionText).toBeTruthy();
    expect(sectionText!.length).toBeGreaterThan(0);
  });

  test('OPP-055: DST section (risks) also renders', async ({ page }) => {
    // DST is handled within the risks section
    const risksSection = page.locator('#section-risks').first();
    await expect(risksSection).toBeVisible({ timeout: 10000 });
    
    const dstComponent = page.locator('app-opportunity-dst-section').first();
    await expect(dstComponent).toBeVisible({ timeout: 5000 });
  });

  test('OPP-056: Both Analysis and Risks sections coexist on the page', async ({ page }) => {
    const analysisSection = page.locator('#section-analysis').first();
    const risksSection = page.locator('#section-risks').first();
    
    await expect(analysisSection).toBeVisible({ timeout: 10000 });
    await expect(risksSection).toBeVisible({ timeout: 10000 });
  });

  test('OPP-057: All opportunity sections are rendered', async ({ page }) => {
    // Verify all section IDs exist in the DOM
    const sectionIds = [
      'section-analysis', 'section-overview', 'section-what', 'section-why',
      'section-who', 'section-where', 'section-when', 'section-risks',
      'section-related', 'section-collaboration', 'section-statement', 'section-team'
    ];
    
    for (const sectionId of sectionIds) {
      const section = page.locator(`#${sectionId}`).first();
      const visible = await section.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    }
  });
});
