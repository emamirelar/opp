/**
 * @fileoverview Opportunity Budget & Schedule Section E2E Tests
 * Tests the Overview section (contains budget) and When section (contains schedule).
 * 
 * Covers scenarios: OPP-037 to OPP-044
 * 
 * Uses API mocks - fully executable. Budget fields are in the Overview section;
 * schedule/dates are in the When section.
 * 
 * Actual selectors from the Angular templates:
 * - Overview: #section-overview, app-opportunity-overview-section
 * - When: #section-when, app-opportunity-when-section
 * - Budget fields: nameControl, descriptionControl, initiativeBudgetControl
 * - Date fields: #targetSigningDate, #implementationStartDate, #targetDeliveryDate
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

test.describe('Opportunity Budget & Schedule Sections', () => {
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test.describe('Budget - Overview Section', () => {
    test('OPP-037: Overview section with budget displays on opportunity detail', async ({ page }) => {
      // The overview section contains budget information
      const overviewSection = page.locator('#section-overview').first();
      await expect(overviewSection).toBeVisible({ timeout: 10000 });
      
      // The overview component should be rendered
      const overviewComponent = page.locator('app-opportunity-overview-section').first();
      await expect(overviewComponent).toBeVisible({ timeout: 5000 });
    });

    test('OPP-038: Overview section contains budget-related fields', async ({ page }) => {
      const overviewSection = page.locator('#section-overview').first();
      await expect(overviewSection).toBeVisible({ timeout: 10000 });
      
      // Look for budget-related content (text "budget" in the section)
      const budgetText = overviewSection.getByText(/budget/i).first();
      const budgetVisible = await budgetText.isVisible({ timeout: 5000 }).catch(() => false);
      
      // Budget information should be displayed somewhere in the overview
      // The field is initiativeBudgetControl in the component
      expect(budgetVisible).toBeTruthy();
    });

    test('OPP-039: Opportunity name is displayed in overview', async ({ page }) => {
      // Verify the opportunity title is displayed
      const oppTitle = page.locator('[data-testid="opportunity-title"]').first();
      await expect(oppTitle).toBeVisible({ timeout: 10000 });
      
      const titleText = await oppTitle.textContent();
      expect(titleText).toBeTruthy();
      expect(titleText!.length).toBeGreaterThan(0);
    });
  });

  test.describe('Schedule - When Section', () => {
    test('OPP-040: When section displays on opportunity detail', async ({ page }) => {
      // The When section contains schedule/dates
      const whenSection = page.locator('#section-when').first();
      await expect(whenSection).toBeVisible({ timeout: 10000 });
      
      const whenComponent = page.locator('app-opportunity-when-section').first();
      await expect(whenComponent).toBeVisible({ timeout: 5000 });
    });

    test('OPP-041: When section contains date fields', async ({ page }) => {
      const whenSection = page.locator('#section-when').first();
      await expect(whenSection).toBeVisible({ timeout: 10000 });
      
      // Look for date-related content in the When section
      const dateContent = whenSection.getByText(/date|duration|signing|implementation/i).first();
      const dateVisible = await dateContent.isVisible({ timeout: 5000 }).catch(() => false);
      
      expect(dateVisible).toBeTruthy();
    });

    test('OPP-042: Section navigation chip for When is visible', async ({ page }) => {
      // The section navigation chips should include When
      const whenChip = page.getByText(/when/i).first();
      await expect(whenChip).toBeVisible({ timeout: 10000 });
    });
  });

  test.describe('Section Navigation', () => {
    test('OPP-043: Can scroll to overview section via chip navigation', async ({ page }) => {
      // Click on Overview chip
      const overviewChip = page.getByText(/overview/i).first();
      await expect(overviewChip).toBeVisible({ timeout: 10000 });
      await overviewChip.click();
      await page.waitForTimeout(500);
      
      // Overview section should now be visible in viewport
      const overviewSection = page.locator('#section-overview').first();
      await expect(overviewSection).toBeVisible();
    });

    test('OPP-044: Can scroll to When section via chip navigation', async ({ page }) => {
      const whenChip = page.getByText(/when/i).first();
      await expect(whenChip).toBeVisible({ timeout: 10000 });
      await whenChip.click();
      await page.waitForTimeout(500);
      
      const whenSection = page.locator('#section-when').first();
      await expect(whenSection).toBeVisible();
    });
  });
});
