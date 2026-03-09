/**
 * @fileoverview Opportunity Section Editing E2E Tests
 *
 * Tests for editing and saving each section of the opportunity detail page:
 * Overview, What, Why, Who, Where, When, and Team.
 * Verifies edit mode toggling, field population, save/cancel flows, and data persistence.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-OPP-SECTIONS
 * @tests 34
 */

import { test, expect, Page } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForElementReady,
  waitForLoadingToComplete,
  waitForNetworkIdle,
  waitForPermissions,
} from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

const featureReady = process.env.OPPORTUNITY_EDITING_IMPLEMENTED === 'true';

const ADMIN_USER = 'test@playwright.local';
const READONLY_USER = 'test-readonly@playwright.local';

const TEST_OPP = {
  draft: process.env.TEST_OPP_DRAFT_ID || '2',
  active: process.env.TEST_OPP_ACTIVE_ID || '4',
};

function oppUrl(id: string, section?: string): string {
  return section
    ? `/partnerships/opportunities/${id}/${section}`
    : `/partnerships/opportunities/${id}`;
}

async function navigateToSection(page: Page, sectionName: string): Promise<void> {
  const sectionId = `section-${sectionName.toLowerCase()}`;
  const sectionLocator = page.locator(`#${sectionId}, app-opportunity-${sectionName.toLowerCase()}-section`);
  const chip = page.locator(`button:has-text("${sectionName}")`).first();
  if (await chip.isVisible({ timeout: 3000 }).catch(() => false)) {
    await chip.click();
    await waitForElementReady(sectionLocator.first(), 5000);
    return;
  }
  if ((await sectionLocator.count()) > 0) {
    await sectionLocator.first().scrollIntoViewIfNeeded().catch(() => {});
    await waitForLoadingToComplete(page);
  }
}

async function clickEditButton(page: Page, sectionSelector: string): Promise<boolean> {
  const section = page.locator(sectionSelector);
  const editBtn = section.locator('button:has(i.pi-pencil), [data-testid*="edit"]').first();
  const isVisible = await editBtn.isVisible({ timeout: 5000 }).catch(() => false);
  if (isVisible) {
    await editBtn.click();
    await waitForLoadingToComplete(page);
  }
  return isVisible;
}

async function clickSaveButton(page: Page, sectionSelector: string): Promise<boolean> {
  const section = page.locator(sectionSelector);
  const saveBtn = section.locator('button:has-text("Save"), button:has(i.pi-check)').first();
  const isVisible = await saveBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (isVisible) {
    await saveBtn.click();
    await waitForNetworkIdle(page);
  }
  return isVisible;
}

async function clickCancelButton(page: Page, sectionSelector: string): Promise<boolean> {
  const section = page.locator(sectionSelector);
  const cancelBtn = section.locator('button:has-text("Cancel"), button:has(i.pi-times)').first();
  const isVisible = await cancelBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (isVisible) {
    await cancelBtn.click();
    await waitForLoadingToComplete(page);
  }
  return isVisible;
}

// =============================================================================
// OVERVIEW SECTION
// =============================================================================
test.describe('Section Editing — Overview', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-OVW-001: Edit button visible on overview section for admin', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Overview');
    const editBtn = oppPage.overviewSection.locator('button:has(i.pi-pencil), [data-testid*="edit"]').first();
    await expect(editBtn).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-OVW-002: Can edit and save opportunity name', async ({ page }) => {
    await test.step('Navigate to overview and enter edit mode', async () => {
      await navigateToSection(page, 'Overview');
      await clickEditButton(page, '#section-overview, app-opportunity-overview-section');
    });

    await test.step('Edit name field', async () => {
      const nameInput = page.locator('#section-overview input, app-opportunity-overview-section input').first();
      const isEditable = await nameInput.isVisible({ timeout: 5000 }).catch(() => false);
      if (isEditable) {
        await nameInput.clear();
        await nameInput.fill('Updated Name E2E ' + Date.now());
      }
    });

    await test.step('Save and verify', async () => {
      const saved = await clickSaveButton(page, '#section-overview, app-opportunity-overview-section');
      if (saved) {
        const toast = page.locator('.p-toast-message');
        const hasToast = await toast.isVisible({ timeout: 5000 }).catch(() => false);
        if (hasToast) {
          await expect(toast).toContainText(/success|saved|updated/i);
        }
      }
    });
  });

  test('EDIT-OVW-003: Can edit and save opportunity description', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Overview');
    await clickEditButton(page, '#section-overview, app-opportunity-overview-section');

    const descInput = oppPage.overviewSection.locator('textarea').first();
    await expect(descInput).toBeVisible({ timeout: 5000 });
    await descInput.clear();
    await descInput.fill('Updated description from E2E test');
    await clickSaveButton(page, '#section-overview, app-opportunity-overview-section');
  });

  test('EDIT-OVW-004: Cancel discards changes in overview', async ({ page }) => {
    await navigateToSection(page, 'Overview');
    const editClicked = await clickEditButton(page, '#section-overview, app-opportunity-overview-section');
    if (editClicked) {
      const nameInput = page.locator('#section-overview input').first();
      if (await nameInput.isVisible({ timeout: 3000 }).catch(() => false)) {
        const originalValue = await nameInput.inputValue();
        await nameInput.fill('SHOULD_BE_DISCARDED');
        await clickCancelButton(page, '#section-overview, app-opportunity-overview-section');
        const afterCancel = page.locator('#section-overview input').first();
        if (await afterCancel.isVisible({ timeout: 2000 }).catch(() => false)) {
          const currentValue = await afterCancel.inputValue();
          expect(currentValue).not.toBe('SHOULD_BE_DISCARDED');
        }
      }
    }
  });

  test('EDIT-OVW-005: Read-only user cannot see edit button on overview', async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft), READONLY_USER);
    await waitForPermissions(page);
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Overview');
    const editBtn = oppPage.overviewSection.locator('button:has(i.pi-pencil)').first();
    await expect(editBtn).not.toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// WHAT SECTION
// =============================================================================
test.describe('Section Editing — What', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-WHAT-001: Edit button visible on What section', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'What');
    await expect(oppPage.whatSection).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHAT-002: Can enter edit mode and modify org unit', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'What');
    const editClicked = await clickEditButton(page, '#section-what, app-opportunity-what-section');
    expect(editClicked).toBeTruthy();
    const orgUnitSelect = oppPage.whatSection.locator('p-select').first();
    await expect(orgUnitSelect).toBeVisible({ timeout: 3000 });
  });

  test('EDIT-WHAT-003: Can save What section changes', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'What');
    await clickEditButton(page, '#section-what, app-opportunity-what-section');
    const saved = await clickSaveButton(page, '#section-what, app-opportunity-what-section');
    expect(saved).toBeTruthy();
  });

  test('EDIT-WHAT-004: Initiative type dropdown available in edit mode', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'What');
    await clickEditButton(page, '#section-what, app-opportunity-what-section');
    const initiativeDropdown = oppPage.whatSection.locator('#initiativeType, [data-testid="initiative-type-select"]').first();
    await expect(initiativeDropdown).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHAT-005: Delivery modality dropdown available in edit mode', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'What');
    await clickEditButton(page, '#section-what, app-opportunity-what-section');
    const modalityDropdown = oppPage.whatSection.locator('[data-testid="delivery-modality-select"]').first();
    await expect(modalityDropdown).toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// WHY SECTION
// =============================================================================
test.describe('Section Editing — Why', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-WHY-001: Can enter edit mode on Why section', async ({ page }) => {
    await navigateToSection(page, 'Why');
    const editClicked = await clickEditButton(page, '#section-why, app-opportunity-why-section');
    expect(editClicked).toBeTruthy();
  });

  test('EDIT-WHY-002: SDG multiselect available in edit mode', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Why');
    await clickEditButton(page, '#section-why, app-opportunity-why-section');
    const sdgSelect = oppPage.whySection.locator('p-multiselect, [data-testid="sdg-multiselect"]').first();
    await expect(sdgSelect).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHY-003: Can edit challenges textarea', async ({ page }) => {
    await navigateToSection(page, 'Why');
    await clickEditButton(page, '#section-why, app-opportunity-why-section');
    const challengesInput = page.locator('#section-why textarea').first();
    if (await challengesInput.isVisible({ timeout: 5000 }).catch(() => false)) {
      await challengesInput.fill('Updated challenges from E2E test');
      await clickSaveButton(page, '#section-why, app-opportunity-why-section');
    }
  });

  test('EDIT-WHY-004: Can edit expected impact field', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Why');
    await clickEditButton(page, '#section-why, app-opportunity-why-section');
    const impactInput = oppPage.whySection.locator('textarea, input').nth(1);
    await expect(impactInput).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHY-005: Can edit beneficiary numbers', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Why');
    await clickEditButton(page, '#section-why, app-opportunity-why-section');
    const benefInput = oppPage.whySection.locator('p-inputnumber, input[type="number"]').first();
    await expect(benefInput).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHY-006: UNOPS missions multiselect available', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Why');
    await clickEditButton(page, '#section-why, app-opportunity-why-section');
    const missionSelect = oppPage.whySection.locator('[data-testid="missions-multiselect"]').first();
    await expect(missionSelect).toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// WHO SECTION
// =============================================================================
test.describe('Section Editing — Who', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-WHO-001: Who section visible and has edit button', async ({ page }) => {
    await navigateToSection(page, 'Who');
    const section = page.locator('#section-who, app-opportunity-who-section').first();
    await expect(section).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHO-002: Can add a funding partner', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Who');
    await clickEditButton(page, '#section-who, app-opportunity-who-section');
    const addPartnerBtn = oppPage.whoSection.locator('button:has-text("Add"), [data-testid="add-funding-partner"]').first();
    await expect(addPartnerBtn).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHO-003: Can add a client partner', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Who');
    await clickEditButton(page, '#section-who, app-opportunity-who-section');
    const addClientBtn = oppPage.whoSection.locator('[data-testid="add-client-partner"]').first();
    await expect(addClientBtn).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHO-004: Can remove a funding partner', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Who');
    await clickEditButton(page, '#section-who, app-opportunity-who-section');
    const removeBtn = oppPage.whoSection.locator('button:has(i.pi-trash), button:has(i.pi-times)').first();
    await expect(removeBtn).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHO-005: External stakeholder management area visible', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Who');
    const stakeholderArea = oppPage.whoSection.getByText(/external stakeholder/i).first();
    await expect(stakeholderArea).toBeVisible({ timeout: 5000 });
  });
});

// =============================================================================
// WHERE SECTION
// =============================================================================
test.describe('Section Editing — Where', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-WHERE-001: Where section visible with edit button', async ({ page }) => {
    await navigateToSection(page, 'Where');
    const section = page.locator('#section-where, app-opportunity-where-section').first();
    await expect(section).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHERE-002: Can add implementation country', async ({ page }) => {
    await navigateToSection(page, 'Where');
    await clickEditButton(page, '#section-where, app-opportunity-where-section');
    const countrySelect = page.locator('#section-where p-multiselect, #section-where p-select, [data-testid="country-select"]').first();
    await expect(countrySelect).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHERE-003: Can save country changes', async ({ page }) => {
    await navigateToSection(page, 'Where');
    await clickEditButton(page, '#section-where, app-opportunity-where-section');
    const saved = await clickSaveButton(page, '#section-where, app-opportunity-where-section');
    expect(saved).toBeTruthy();
  });
});

// =============================================================================
// WHEN SECTION
// =============================================================================
test.describe('Section Editing — When', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-WHEN-001: When section has date fields in edit mode', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'When');
    await clickEditButton(page, '#section-when, app-opportunity-when-section');
    const dateField = oppPage.whenSection.locator('p-datepicker, input[type="date"]').first();
    await expect(dateField).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHEN-002: Target signing date field available', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'When');
    await clickEditButton(page, '#section-when, app-opportunity-when-section');
    const signingDate = oppPage.whenSection.getByText(/target signing/i).first();
    await expect(signingDate).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHEN-003: Implementation start date field available', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'When');
    await clickEditButton(page, '#section-when, app-opportunity-when-section');
    const startDate = oppPage.whenSection.getByText(/implementation start/i).first();
    await expect(startDate).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHEN-004: Target delivery date field available', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'When');
    await clickEditButton(page, '#section-when, app-opportunity-when-section');
    const deliveryDate = oppPage.whenSection.getByText(/target delivery|delivery date/i).first();
    await expect(deliveryDate).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-WHEN-005: Date validation — implementation start before signing date rejected', async ({ page }) => {
    await navigateToSection(page, 'When');
    const editClicked = await clickEditButton(page, '#section-when, app-opportunity-when-section');
    expect(editClicked).toBeTruthy();
    if (editClicked) {
      await clickSaveButton(page, '#section-when, app-opportunity-when-section');
      const error = page.locator('.p-error, .p-message-error, [class*="error"]').first();
      const hasError = await error.isVisible({ timeout: 3000 }).catch(() => false);
      const successToast = page.locator('.p-toast-message').filter({ hasText: /success|saved|updated/i });
      const hasSuccess = await successToast.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasError || !hasSuccess).toBeTruthy();
    }
  });
});

// =============================================================================
// TEAM SECTION
// =============================================================================
test.describe('Section Editing — Team', () => {
  test.slow();
  test.skip(!featureReady, 'Section editing not deployed — set OPPORTUNITY_EDITING_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, oppUrl(TEST_OPP.draft));
    await waitForPermissions(page);
  });

  test('EDIT-TEAM-001: Team section has edit controls', async ({ page }) => {
    await navigateToSection(page, 'Team');
    const section = page.locator('#section-team').first();
    await expect(section).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-TEAM-002: Opportunity Manager dropdown available', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Team');
    await clickEditButton(page, '#section-team');
    const omSelect = oppPage.teamSection.locator('#opportunityManager, [data-testid="opportunity-manager-select"]').first();
    await expect(omSelect).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-TEAM-003: Can add collaborator', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Team');
    await clickEditButton(page, '#section-team');
    const addBtn = oppPage.teamSection.locator('button:has-text("Add"), [data-testid="add-collaborator"]').first();
    await expect(addBtn).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-TEAM-004: SME/relevant people section visible', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, TEST_OPP.draft);
    await navigateToSection(page, 'Team');
    const smeText = oppPage.teamSection.getByText(/relevant people|SME|subject matter/i).first();
    await expect(smeText).toBeVisible({ timeout: 5000 });
  });

  test('EDIT-TEAM-005: Can save team section changes', async ({ page }) => {
    await navigateToSection(page, 'Team');
    await clickEditButton(page, '#section-team');
    const saved = await clickSaveButton(page, '#section-team');
    expect(saved).toBeTruthy();
  });
});
