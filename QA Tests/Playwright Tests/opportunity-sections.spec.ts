/**
 * @fileoverview Playwright E2E tests for Opportunity Sections
 * Tests derived from JIRA Zephyr test case gap analysis
 * Covers: Team Section, Workflow Status, WHY Section, WHAT Section
 * @author UNOPS Opportunity+ QA Team
 */

import { test, expect, Page } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';

// Test configuration
const BASE_URL = process.env.TEST_BASE_URL || 'http://127.0.0.1:4200';

/**
 * Navigate to a specific opportunity using hash-based routing
 */
async function navigateToOpportunity(page: Page, opportunityId: string): Promise<void> {
  await page.goto(`${BASE_URL}/#/partnerships/opportunities/${opportunityId}`);
  await page.waitForLoadState('load');
  await page.waitForTimeout(2000);
}

/**
 * Navigate to a specific section within an opportunity
 */
async function navigateToSection(page: Page, sectionName: string): Promise<void> {
  const tabSelector = `[data-testid="${sectionName.toLowerCase()}-tab"], [role="tab"]:has-text("${sectionName}")`;
  await page.locator(tabSelector).first().click();
  await page.waitForTimeout(1000);
}

// ============================================================================
// TEAM SECTION TESTS (PNO-979)
// ============================================================================
// NOTE: These tests require specific test data (opportunities with IDs like 
// 'test-opportunity-1', 'draft-opportunity-1') that must be seeded in the test database.
// They are skipped in mocked environments.

test.describe('Team Section Tests (PNO-979)', () => {
  // Skip - these tests require specific test data that doesn't exist in mocked environment
  test.skip(true, 'Team section tests require specific test data seeding - skipped in mocked environment');
  
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test.describe('Team Section Layout', () => {
    test('POS_001 - Team Section is positioned as last tab', async ({ page }) => {
      await navigateToOpportunity(page, 'test-opportunity-1');
      
      const tabs = page.locator('[role="tab"]');
      const tabCount = await tabs.count();
      const lastTab = tabs.nth(tabCount - 1);
      
      await expect(lastTab).toContainText(/Team/i);
    });

    test('POS_002 - Team Section contains three subsections', async ({ page }) => {
      await navigateToOpportunity(page, 'test-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await expect(page.locator('text=Opportunity Development Team')).toBeVisible();
      await expect(page.locator('text=Other Internal Stakeholders')).toBeVisible();
      await expect(page.locator('text=Opportunity decision making pathway')).toBeVisible();
    });
  });

  test.describe('Opportunity Manager', () => {
    test('NEG_003 - Cannot save without Opportunity Manager', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      // Clear the OM field
      const omField = page.locator('[data-testid="opportunity-manager-field"]');
      await omField.clear();
      
      // Attempt to save
      await page.locator('[data-testid="save-button"]').click();
      
      // Verify validation error
      await expect(page.locator('text=Opportunity Manager is required')).toBeVisible();
    });

    test('POS_004 - OM card displays standardized position title', async ({ page }) => {
      await navigateToOpportunity(page, 'test-opportunity-1');
      await navigateToSection(page, 'Team');
      
      const omCard = page.locator('[data-testid="opportunity-manager-card"]');
      await expect(omCard.locator('.position-title')).toBeVisible();
    });
  });

  test.describe('Collaborators', () => {
    test('POS_005 - Can search and add active personnel as collaborators', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="add-collaborator-button"]').click();
      await page.locator('[data-testid="collaborator-search"]').fill('John');
      
      await expect(page.locator('[data-testid="search-results"]')).toBeVisible();
      
      const firstResult = page.locator('[data-testid="search-result-item"]').first();
      await firstResult.click();
      
      await expect(page.locator('[data-testid="collaborators-list"]')).toContainText(/John/i);
    });

    test('NEG_006 - Expertise field is mandatory when adding collaborator', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="add-collaborator-button"]').click();
      await page.locator('[data-testid="collaborator-search"]').fill('Test User');
      await page.locator('[data-testid="search-result-item"]').first().click();
      
      // Leave expertise empty and try to save
      await page.locator('[data-testid="save-collaborator-button"]').click();
      
      await expect(page.locator('text=Expertise is required')).toBeVisible();
    });

    test('POS_007 - Expertise dropdown contains specific values', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="add-collaborator-button"]').click();
      await page.locator('[data-testid="expertise-dropdown"]').click();
      
      const expectedValues = [
        'Project Management',
        'Technical Expertise',
        'Financial Management',
        'Legal',
        'Procurement',
        'Human Resources',
        'Communications',
        'Risk Management',
        'Monitoring & Evaluation',
        'Other'
      ];
      
      for (const value of expectedValues) {
        await expect(page.locator(`[role="option"]:has-text("${value}")`)).toBeVisible();
      }
    });

    test('POS_008 - Expertise dropdown allows multi-selection', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="add-collaborator-button"]').click();
      await page.locator('[data-testid="expertise-dropdown"]').click();
      
      await page.locator('[role="option"]:has-text("Project Management")').click();
      await page.locator('[role="option"]:has-text("Legal")').click();
      
      // Verify both are selected
      await expect(page.locator('.selected-expertise')).toHaveCount(2);
    });
  });

  test.describe('Responsible Org Unit', () => {
    test('POS_010 - Org Unit search restricted to D&P units', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="org-unit-dropdown"]').click();
      await page.locator('[data-testid="org-unit-search"]').fill('D&P');
      
      const results = page.locator('[data-testid="org-unit-option"]');
      const count = await results.count();
      
      for (let i = 0; i < count; i++) {
        const text = await results.nth(i).textContent();
        expect(text).toMatch(/D&P|Development and Partnerships/i);
      }
    });

    test('POS_012 - Org Unit Type auto-populates upon selection', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Team');
      
      await page.locator('[data-testid="org-unit-dropdown"]').click();
      await page.locator('[data-testid="org-unit-option"]').first().click();
      
      const orgTypeField = page.locator('[data-testid="org-unit-type"]');
      await expect(orgTypeField).not.toBeEmpty();
    });
  });

  test.describe('Country Mismatch Warning', () => {
    test('B&L_018 - Warning popup triggers on country mismatch', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-country');
      await navigateToSection(page, 'Team');
      
      // Select an org unit NOT responsible for the implementation country
      await page.locator('[data-testid="org-unit-dropdown"]').click();
      await page.locator('[data-testid="mismatched-org-unit"]').click();
      
      await expect(page.locator('[data-testid="mismatch-warning-dialog"]')).toBeVisible();
    });

    test('POS_026 - Cancel reverts org unit selection', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-country');
      await navigateToSection(page, 'Team');
      
      const originalValue = await page.locator('[data-testid="org-unit-dropdown"]').inputValue();
      
      await page.locator('[data-testid="org-unit-dropdown"]').click();
      await page.locator('[data-testid="mismatched-org-unit"]').click();
      
      await page.locator('[data-testid="mismatch-cancel-button"]').click();
      
      const currentValue = await page.locator('[data-testid="org-unit-dropdown"]').inputValue();
      expect(currentValue).toBe(originalValue);
    });
  });

  test.describe('Permissions', () => {
    test('NEG_029 - View-only user cannot edit Team section', async ({ page }) => {
      // Login as view-only user using shared auth helper
      await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1', 'viewer@example.com');
      await navigateToSection(page, 'Team');
      
      await expect(page.locator('[data-testid="edit-button"]')).not.toBeVisible();
      await expect(page.locator('[data-testid="add-collaborator-button"]')).not.toBeVisible();
    });
  });
});

// ============================================================================
// WORKFLOW STATUS TESTS (PNO-940)
// ============================================================================

test.describe('Opportunity Workflow Status Tests (PNO-940)', () => {
  // Skip - these tests require specific test data that doesn't exist in mocked environment
  test.skip(true, 'Workflow status tests require specific test data seeding - skipped in mocked environment');
  
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test.describe('Positive Status Transitions', () => {
    test('POS_001 - Draft to Active transition', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      
      await page.locator('[data-testid="activate-button"]').click();
      await page.locator('[data-testid="confirm-dialog-yes"]').click();
      
      await expect(page.locator('[data-testid="status-badge"]')).toContainText(/Active/i);
    });

    test('POS_002 - Active to Pending Decision transition', async ({ page }) => {
      await navigateToOpportunity(page, 'active-opportunity-1');
      
      await page.locator('[data-testid="send-for-decision-button"]').click();
      await page.locator('[data-testid="confirm-submission"]').click();
      
      await expect(page.locator('[data-testid="status-badge"]')).toContainText(/Pending Decision/i);
    });

    test('POS_009 - Status filter in opportunity list', async ({ page }) => {
      await page.goto(`${BASE_URL}/opportunities`);
      
      await page.locator('[data-testid="status-filter"]').click();
      await page.locator('[data-testid="filter-option-draft"]').click();
      
      const rows = page.locator('[data-testid="opportunity-row"]');
      const count = await rows.count();
      
      for (let i = 0; i < count; i++) {
        await expect(rows.nth(i).locator('.status-badge')).toContainText(/Draft/i);
      }
    });

    test('POS_011 - OM recall during pending decision', async ({ page }) => {
      await navigateToOpportunity(page, 'pending-opportunity-1');
      
      await page.locator('[data-testid="recall-button"]').click();
      await page.locator('[data-testid="confirm-recall"]').click();
      
      await expect(page.locator('[data-testid="status-badge"]')).toContainText(/Active/i);
    });
  });

  test.describe('Negative Status Validations', () => {
    test('NEG_001 - Cannot activate with missing mandatory fields', async ({ page }) => {
      await navigateToOpportunity(page, 'incomplete-draft-opportunity');
      
      await page.locator('[data-testid="activate-button"]').click();
      
      await expect(page.locator('[data-testid="validation-error"]')).toBeVisible();
      await expect(page.locator('[data-testid="status-badge"]')).toContainText(/Draft/i);
    });

    test('NEG_004 - Cannot edit during pending decision', async ({ page }) => {
      await navigateToOpportunity(page, 'pending-opportunity-1');
      
      await expect(page.locator('[data-testid="edit-button"]')).toBeDisabled();
      await expect(page.locator('text=Record locked pending decision')).toBeVisible();
    });

    test('NEG_006 - Rejection requires reason', async ({ page }) => {
      // Login as decision maker using shared auth helper
      await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1', 'doa2@example.com');
      
      await page.locator('[data-testid="reject-button"]').click();
      // Leave reason empty
      await page.locator('[data-testid="submit-rejection"]').click();
      
      await expect(page.locator('text=Rejection reason is required')).toBeVisible();
    });
  });

  test.describe('Security Tests', () => {
    test('SEC_002 - Cross-user status change prevention', async ({ page }) => {
      // Login as user who doesn't own the opportunity, using shared auth helper
      await authenticateWithRealBackend(page, '/#/partnerships/opportunities/other-user-opportunity', 'other-user@example.com');
      
      await expect(page.locator('text=Access Denied')).toBeVisible();
    });

    test('SEC_004 - Role-based status actions', async ({ page }) => {
      // Login as viewer using shared auth helper
      await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1', 'viewer@example.com');
      
      // Viewer should NOT see action buttons
      await expect(page.locator('[data-testid="activate-button"]')).not.toBeVisible();
      await expect(page.locator('[data-testid="cancel-button"]')).not.toBeVisible();
    });
  });

  test.describe('Concurrency Tests', () => {
    test('CONC_001 - Duplicate submit prevention', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      
      const activateBtn = page.locator('[data-testid="activate-button"]');
      
      // Double click rapidly
      await activateBtn.dblclick();
      
      // Should show processing indicator and prevent double submission
      await expect(page.locator('[data-testid="loading-spinner"]')).toBeVisible();
      
      // Wait for operation to complete
      await page.waitForSelector('[data-testid="loading-spinner"]', { state: 'hidden' });
      
      // Verify status changed only once
      await expect(page.locator('[data-testid="status-badge"]')).toContainText(/Active/i);
    });
  });
});

// ============================================================================
// WHY SECTION TESTS (PNO-692/938)
// ============================================================================

test.describe('WHY Section Tests (PNO-692/938)', () => {
  // Skip - these tests require specific test data that doesn't exist in mocked environment
  test.skip(true, 'WHY section tests require specific test data seeding - skipped in mocked environment');
  
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test.describe('SDG Alignment', () => {
    test('POS_001 - SDG selection displays all 17 goals', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="sdg-selector"]').click();
      
      const sdgOptions = page.locator('[data-testid="sdg-option"]');
      await expect(sdgOptions).toHaveCount(17);
    });

    test('POS_002 - Multiple SDG selection', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="sdg-selector"]').click();
      
      await page.locator('[data-testid="sdg-option-1"]').click();
      await page.locator('[data-testid="sdg-option-4"]').click();
      await page.locator('[data-testid="sdg-option-13"]').click();
      
      await page.locator('[data-testid="save-button"]').click();
      
      await expect(page.locator('[data-testid="selected-sdgs"]')).toHaveCount(3);
    });

    test('POS_003 - Primary SDG designation', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-sdgs');
      await navigateToSection(page, 'Why');
      
      const sdgCard = page.locator('[data-testid="sdg-card"]').first();
      await sdgCard.locator('[data-testid="set-primary-button"]').click();
      
      await expect(sdgCard.locator('.primary-badge')).toBeVisible();
    });

    test('NEG_005 - Minimum SDG selection required', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-no-sdg');
      await navigateToSection(page, 'Why');
      
      // Clear any selected SDGs
      const removeButtons = page.locator('[data-testid="remove-sdg-button"]');
      const count = await removeButtons.count();
      for (let i = 0; i < count; i++) {
        await removeButtons.first().click();
      }
      
      await page.locator('[data-testid="submit-for-decision-button"]').click();
      
      await expect(page.locator('text=At least one SDG is required')).toBeVisible();
    });
  });

  test.describe('Beneficiaries', () => {
    test('POS_006 - Beneficiary count entry', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="beneficiary-count"]').fill('50000');
      await page.locator('[data-testid="save-button"]').click();
      
      await page.reload();
      await navigateToSection(page, 'Why');
      
      await expect(page.locator('[data-testid="beneficiary-count"]')).toHaveValue('50000');
    });

    test('NEG_009 - Beneficiary breakdown validation', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="beneficiary-count"]').fill('1000');
      await page.locator('[data-testid="women-count"]').fill('600');
      await page.locator('[data-testid="men-count"]').fill('600');
      
      await expect(page.locator('text=Gender breakdown exceeds total')).toBeVisible();
    });

    test('NEG_010 - Negative beneficiary count rejected', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="beneficiary-count"]').fill('-500');
      await page.locator('[data-testid="save-button"]').click();
      
      await expect(page.locator('text=Beneficiary count must be positive')).toBeVisible();
    });
  });

  test.describe('UN Cooperation Framework', () => {
    test('POS_015 - UN Framework selection', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="un-framework-dropdown"]').click();
      await page.locator('[data-testid="framework-option"]').first().click();
      
      await page.locator('[data-testid="save-button"]').click();
      
      await expect(page.locator('[data-testid="un-framework-dropdown"]')).not.toBeEmpty();
    });

    test('NEG_017 - Framework required for submission', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-no-framework');
      await navigateToSection(page, 'Why');
      
      // Clear framework
      await page.locator('[data-testid="un-framework-dropdown"]').click();
      await page.locator('[data-testid="clear-framework"]').click();
      
      await page.locator('[data-testid="submit-for-decision-button"]').click();
      
      await expect(page.locator('text=UN Cooperation Framework required')).toBeVisible();
    });
  });

  test.describe('High-Risk Checklist', () => {
    test('POS_021 - High-risk checklist display', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      const highRiskSection = page.locator('[data-testid="high-risk-section"]');
      await expect(highRiskSection).toBeVisible();
      
      const questions = highRiskSection.locator('[data-testid="risk-question"]');
      await expect(questions).not.toHaveCount(0);
    });

    test('POS_022 - High-risk flag triggers on Yes answer', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'Why');
      
      // Answer "No" to all questions first
      const noButtons = page.locator('[data-testid="risk-answer-no"]');
      const count = await noButtons.count();
      for (let i = 0; i < count; i++) {
        await noButtons.nth(i).click();
      }
      
      await expect(page.locator('[data-testid="high-risk-flag"]')).not.toBeVisible();
      
      // Now answer "Yes" to one question
      await page.locator('[data-testid="risk-answer-yes"]').first().click();
      
      await expect(page.locator('[data-testid="high-risk-flag"]')).toBeVisible();
    });

    test('NEG_024 - High-risk checklist required for submission', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-incomplete-risk');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="submit-for-decision-button"]').click();
      
      await expect(page.locator('text=High-risk checklist required')).toBeVisible();
    });
  });

  test.describe('AI Features', () => {
    test('POS_012 - AI-assisted context generation', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-context');
      await navigateToSection(page, 'Why');
      
      await page.locator('[data-testid="generate-ai-context"]').click();
      
      await expect(page.locator('[data-testid="ai-loading"]')).toBeVisible();
      await page.waitForSelector('[data-testid="ai-loading"]', { state: 'hidden', timeout: 30000 });
      
      await expect(page.locator('[data-testid="ai-suggestion"]')).toBeVisible();
    });
  });
});

// ============================================================================
// WHAT SECTION TESTS (PNO-700)
// ============================================================================

test.describe('WHAT Section Tests (PNO-700)', () => {
  // Skip - these tests require specific test data that doesn't exist in mocked environment
  test.skip(true, 'WHAT section tests require specific test data seeding - skipped in mocked environment');
  
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test.describe('Scope Definition', () => {
    test('POS_001 - Project scope narrative entry', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'What');
      
      const scopeField = page.locator('[data-testid="scope-narrative"]');
      await scopeField.fill('This is a comprehensive project scope covering all key objectives and deliverables for the implementation phase.');
      
      await page.locator('[data-testid="save-button"]').click();
      
      await page.reload();
      await navigateToSection(page, 'What');
      
      await expect(scopeField).toContainText('comprehensive project scope');
    });

    test('NEG_003 - Scope required for submission', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-no-scope');
      await navigateToSection(page, 'What');
      
      // Clear scope
      await page.locator('[data-testid="scope-narrative"]').fill('');
      
      await page.locator('[data-testid="submit-for-decision-button"]').click();
      
      await expect(page.locator('text=Project scope is required')).toBeVisible();
    });
  });

  test.describe('Deliverables', () => {
    test('POS_004 - Add deliverable', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="add-deliverable-button"]').click();
      
      await page.locator('[data-testid="deliverable-name"]').fill('Training Program');
      await page.locator('[data-testid="deliverable-description"]').fill('Comprehensive training for 100 personnel');
      await page.locator('[data-testid="deliverable-date"]').fill('2026-06-30');
      
      await page.locator('[data-testid="save-deliverable-button"]').click();
      
      await expect(page.locator('[data-testid="deliverables-list"]')).toContainText('Training Program');
    });

    test('POS_005 - Multiple deliverables', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-deliverables');
      await navigateToSection(page, 'What');
      
      // Add additional deliverables
      for (let i = 0; i < 3; i++) {
        await page.locator('[data-testid="add-deliverable-button"]').click();
        await page.locator('[data-testid="deliverable-name"]').fill(`Deliverable ${i + 1}`);
        await page.locator('[data-testid="save-deliverable-button"]').click();
      }
      
      const deliverables = page.locator('[data-testid="deliverable-item"]');
      await expect(deliverables).toHaveCount(3);
    });

    test('POS_007 - Delete deliverable', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-deliverables');
      await navigateToSection(page, 'What');
      
      const initialCount = await page.locator('[data-testid="deliverable-item"]').count();
      
      await page.locator('[data-testid="delete-deliverable-button"]').first().click();
      await page.locator('[data-testid="confirm-delete"]').click();
      
      const finalCount = await page.locator('[data-testid="deliverable-item"]').count();
      expect(finalCount).toBe(initialCount - 1);
    });

    test('NEG_009 - Deliverable name required', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="add-deliverable-button"]').click();
      // Leave name empty
      await page.locator('[data-testid="save-deliverable-button"]').click();
      
      await expect(page.locator('text=Deliverable name is required')).toBeVisible();
    });
  });

  test.describe('Initiative Type', () => {
    test('POS_013 - Initiative type selection', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="initiative-type-dropdown"]').click();
      await page.locator('[data-testid="initiative-option"]').first().click();
      
      await page.locator('[data-testid="save-button"]').click();
      
      await expect(page.locator('[data-testid="initiative-type-dropdown"]')).not.toBeEmpty();
    });

    test('POS_014 - Initiative type hierarchy display', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-1');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="initiative-type-dropdown"]').click();
      
      // Verify hierarchical structure
      const parentItems = page.locator('[data-testid="parent-initiative"]');
      await expect(parentItems).not.toHaveCount(0);
      
      // Expand a parent
      await parentItems.first().click();
      
      const childItems = page.locator('[data-testid="child-initiative"]');
      await expect(childItems).toBeVisible();
    });

    test('NEG_015 - Initiative type required', async ({ page }) => {
      await navigateToOpportunity(page, 'draft-opportunity-no-initiative');
      await navigateToSection(page, 'What');
      
      // Clear initiative type
      await page.locator('[data-testid="clear-initiative-type"]').click();
      
      await page.locator('[data-testid="submit-for-decision-button"]').click();
      
      await expect(page.locator('text=Initiative type is required')).toBeVisible();
    });
  });

  test.describe('AI Matching', () => {
    test('AI_016 - AI matching service options', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-scope');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="match-services-ai"]').click();
      
      await expect(page.locator('[data-testid="ai-loading"]')).toBeVisible();
      await page.waitForSelector('[data-testid="ai-loading"]', { state: 'hidden', timeout: 30000 });
      
      await expect(page.locator('[data-testid="service-suggestions"]')).toBeVisible();
      await expect(page.locator('[data-testid="match-confidence"]')).toBeVisible();
    });

    test('AI_019 - AI content character limits', async ({ page }) => {
      await navigateToOpportunity(page, 'opportunity-with-context');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="generate-scope-ai"]').click();
      await page.waitForSelector('[data-testid="ai-loading"]', { state: 'hidden', timeout: 30000 });
      
      const generatedContent = await page.locator('[data-testid="ai-suggestion"]').textContent();
      expect(generatedContent?.length).toBeLessThanOrEqual(5000); // Assuming 5000 char limit
    });

    test('NEG_020 - AI matching without context shows warning', async ({ page }) => {
      await navigateToOpportunity(page, 'minimal-opportunity');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="match-services-ai"]').click();
      
      await expect(page.locator('text=Add more details for better matching')).toBeVisible();
    });
  });

  test.describe('Grant Support', () => {
    test('POS_026 - Grant support fields display', async ({ page }) => {
      await navigateToOpportunity(page, 'grant-opportunity');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="initiative-type-dropdown"]').click();
      await page.locator('[data-testid="initiative-option"]:has-text("Grant Support")').click();
      
      await expect(page.locator('[data-testid="grant-value-field"]')).toBeVisible();
      await expect(page.locator('[data-testid="grant-recipient-field"]')).toBeVisible();
    });

    test('POS_027 - Grant recipient search', async ({ page }) => {
      await navigateToOpportunity(page, 'grant-opportunity');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="grant-recipient-field"]').click();
      await page.locator('[data-testid="recipient-search"]').fill('Partner');
      
      await expect(page.locator('[data-testid="partner-search-results"]')).toBeVisible();
      
      await page.locator('[data-testid="partner-result"]').first().click();
      
      await expect(page.locator('[data-testid="grant-recipient-field"]')).not.toBeEmpty();
    });

    test('NEG_028 - Grant value validation', async ({ page }) => {
      await navigateToOpportunity(page, 'grant-opportunity');
      await navigateToSection(page, 'What');
      
      await page.locator('[data-testid="grant-value-field"]').fill('-1000');
      await page.locator('[data-testid="save-button"]').click();
      
      await expect(page.locator('text=Grant value must be positive')).toBeVisible();
    });
  });

  test.describe('Integration', () => {
    test('INT_034 - WHAT data in opportunity statement', async ({ page }) => {
      await navigateToOpportunity(page, 'complete-opportunity');
      await navigateToSection(page, 'What');
      
      // Enter scope
      await page.locator('[data-testid="scope-narrative"]').fill('Test scope for statement generation');
      await page.locator('[data-testid="save-button"]').click();
      
      // Generate statement
      await page.locator('[data-testid="generate-statement-button"]').click();
      await page.waitForSelector('[data-testid="statement-loading"]', { state: 'hidden' });
      
      const statement = await page.locator('[data-testid="opportunity-statement"]').textContent();
      expect(statement).toContain('Test scope');
    });
  });
});

// ============================================================================
// UTILITY TESTS
// ============================================================================

test.describe('Cross-Section Integration', () => {
  // Skip - these tests require specific test data that doesn't exist in mocked environment
  test.skip(true, 'Cross-section tests require specific test data seeding - skipped in mocked environment');
  
  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/#/partnerships/opportunities/1');
  });

  test('Section completion indicators update correctly', async ({ page }) => {
    await navigateToOpportunity(page, 'draft-opportunity-1');
    
    // Check initial completion indicators
    const whyTab = page.locator('[data-testid="why-tab"]');
    const whatTab = page.locator('[data-testid="what-tab"]');
    const teamTab = page.locator('[data-testid="team-tab"]');
    
    // Navigate to each section and fill required fields
    await navigateToSection(page, 'Why');
    // Fill WHY section...
    
    await navigateToSection(page, 'What');
    // Fill WHAT section...
    
    await navigateToSection(page, 'Team');
    // Fill Team section...
    
    // Verify completion indicators update
    await expect(whyTab.locator('.completion-indicator')).toContainText(/%/);
  });

  test('Data persists across section navigation', async ({ page }) => {
    await navigateToOpportunity(page, 'draft-opportunity-1');
    
    // Enter data in WHY section
    await navigateToSection(page, 'Why');
    await page.locator('[data-testid="beneficiary-count"]').fill('12345');
    
    // Navigate away
    await navigateToSection(page, 'What');
    
    // Navigate back
    await navigateToSection(page, 'Why');
    
    // Verify data persisted
    await expect(page.locator('[data-testid="beneficiary-count"]')).toHaveValue('12345');
  });
});
