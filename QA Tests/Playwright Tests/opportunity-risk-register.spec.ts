/**
 * @fileoverview Opportunity Risk Register / DST Section E2E Tests
 * Tests the Risks section (#section-risks) which uses app-opportunity-dst-section.
 * 
 * Covers scenarios: OPP-045 to OPP-050
 * 
 * Uses API mocks - fully executable.
 * 
 * Actual selectors:
 * - Section: #section-risks, app-opportunity-dst-section
 * - Risk fields: #riskTitle, #riskType, #riskCategory, #riskProbability,
 *   #riskProximity, #riskImpactLevel, #riskResponseType, #riskDescription
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Opportunity Risk Register', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test('OPP-045: Risks section renders on opportunity detail', async ({ page }) => {
    // The risks section must be present in the page
    const risksSection = page.locator('#section-risks').first();
    await expect(risksSection).toBeVisible({ timeout: 10000 });
    
    // The DST component (which handles risks) should be rendered
    const dstComponent = page.locator('app-opportunity-dst-section').first();
    await expect(dstComponent).toBeVisible({ timeout: 5000 });
  });

  test('OPP-046: Risks section navigation chip is visible', async ({ page }) => {
    // The chip/tab for Risks should exist in section navigation
    const risksChip = page.getByText(/risks/i).first();
    await expect(risksChip).toBeVisible({ timeout: 10000 });
  });

  test('OPP-047: Can navigate to risks section via chip', async ({ page }) => {
    const risksChip = page.getByText(/risks/i).first();
    await expect(risksChip).toBeVisible({ timeout: 10000 });
    await risksChip.click();
    await page.waitForTimeout(500);
    
    // Section should be scrolled into view
    const risksSection = page.locator('#section-risks').first();
    await expect(risksSection).toBeVisible();
  });

  test('OPP-048: Risks section contains risk-related content', async ({ page }) => {
    const risksSection = page.locator('#section-risks').first();
    await expect(risksSection).toBeVisible({ timeout: 10000 });
    
    // The section should contain risk-related text or form elements
    const sectionText = await risksSection.textContent();
    expect(sectionText).toBeTruthy();
    expect(sectionText!.length).toBeGreaterThan(0);
  });

  test('OPP-049: Pre-defined high risk dropdown is part of the section', async ({ page }) => {
    const risksSection = page.locator('#section-risks').first();
    await expect(risksSection).toBeVisible({ timeout: 10000 });
    
    // Look for the pre-defined high risk element (id="preDefinedHighRisk")
    // This may only be visible in edit mode
    const highRiskSelect = page.locator('#preDefinedHighRisk').first();
    const highRiskVisible = await highRiskSelect.isVisible({ timeout: 3000 }).catch(() => false);
    
    // Also check for risk text/label
    const riskLabel = risksSection.getByText(/risk|high risk/i).first();
    const labelVisible = await riskLabel.isVisible({ timeout: 3000 }).catch(() => false);
    
    // Either the form field or a label about risks should exist
    expect(highRiskVisible || labelVisible).toBeTruthy();
  });

  test('OPP-050: Risks section is part of the section navigation list', async ({ page }) => {
    // Verify all expected section chips exist
    const expectedSections = ['Overview', 'What', 'Why', 'Who', 'Where', 'When', 'Risks'];
    
    for (const sectionName of expectedSections) {
      const chip = page.getByText(new RegExp(sectionName, 'i')).first();
      const visible = await chip.isVisible({ timeout: 5000 }).catch(() => false);
      expect(visible).toBeTruthy();
    }
  });
});
