/**
 * @fileoverview Opportunity Statement Section E2E Tests
 * Tests the Statement section on the Opportunity detail page.
 * 
 * Covers scenarios: OPP-058 to OPP-063
 * 
 * Uses API mocks - fully executable.
 * 
 * Actual selectors:
 * - Section: #section-statement, app-opportunity-statement-section
 * - Section chip: text "Statement"
 *
 * @tests 6
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForVisible } from './helpers/wait.helper';

test.describe('Opportunity Statement Section', () => {
  test.slow();
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities/1');
  });

  test('OPP-058: Statement section renders on opportunity detail', async ({ page }) => {
    const statementSection = page.locator('#section-statement').first();
    await expect(statementSection).toBeVisible({ timeout: 10000 });
    
    const statementComponent = page.locator('app-opportunity-statement-section').first();
    await expect(statementComponent).toBeVisible({ timeout: 5000 });
  });

  test('OPP-059: Statement section navigation chip is visible', async ({ page }) => {
    const statementChip = page.getByText(/statement/i).first();
    await expect(statementChip).toBeVisible({ timeout: 10000 });
  });

  test('OPP-060: Can navigate to statement section via chip', async ({ page }) => {
    const statementChip = page.getByText(/statement/i).first();
    await expect(statementChip).toBeVisible({ timeout: 10000 });
    await statementChip.click();

    const statementSection = page.locator('#section-statement').first();
    await waitForVisible(statementSection, 5000);
    await expect(statementSection).toBeVisible();
  });

  test('OPP-061: Statement section contains content', async ({ page }) => {
    const statementSection = page.locator('#section-statement').first();
    await expect(statementSection).toBeVisible({ timeout: 10000 });
    
    const sectionText = await statementSection.textContent();
    expect(sectionText).toBeTruthy();
    expect(sectionText!.length).toBeGreaterThan(0);
  });

  test('OPP-062: Statement section is positioned after collaboration', async ({ page }) => {
    // Verify the sections appear in the expected order
    const collaborationSection = page.locator('#section-collaboration').first();
    const statementSection = page.locator('#section-statement').first();
    
    await expect(collaborationSection).toBeVisible({ timeout: 10000 });
    await expect(statementSection).toBeVisible({ timeout: 5000 });
    
    // Both exist and are visible - verify by checking both are in the DOM
    const collabBox = await collaborationSection.boundingBox();
    const stmtBox = await statementSection.boundingBox();
    
    // Statement should be below collaboration (higher Y coordinate)
    if (collabBox && stmtBox) {
      expect(stmtBox.y).toBeGreaterThan(collabBox.y);
    }
  });

  test('OPP-063: Collaboration section (comments) renders with app-opportunity-collaboration', async ({ page }) => {
    const collaborationSection = page.locator('#section-collaboration').first();
    await expect(collaborationSection).toBeVisible({ timeout: 10000 });
    
    const collabComponent = page.locator('app-opportunity-collaboration').first();
    await expect(collabComponent).toBeVisible({ timeout: 5000 });
  });
});
