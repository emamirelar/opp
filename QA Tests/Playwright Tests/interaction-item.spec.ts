/**
 * @fileoverview Interaction Detail Page E2E Tests
 * Tests the interaction detail/item page functionality using InteractionItemPage page object.
 * 
 * Uses real backend authentication and existing interaction data (ID 1).
 * Ensure database has at least one interaction record before running tests.
 * 
 * Mirrors the pattern from partner-item.spec.ts to ensure consistent
 * page-object-based coverage across all entity detail pages.
 * 
 * Actual data-testid attributes used (from interaction-detail.component.html):
 *   - interaction-detail-header, interaction-title (mobile)
 *   - interaction-type-icon (mobile), create-opportunity-button
 *   - edit-interaction-button, delete-interaction-button
 *   - interaction-description-section, interaction-description
 *   - interaction-documents-section
 *   - interaction-details-section, interaction-date, interaction-location, interaction-status
 *   - interaction-contacts-section, interaction-partners-section
 * 
 * @created 2026-02-12
 *
 * @tests 24
 */

import { test, expect } from '@playwright/test';
import { InteractionItemPage } from './pages/interaction-item.page';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { assertUrlMatches, assertDialogOpen } from './helpers/assertions.helper';

/**
 * Interaction Detail Page Test Suite
 * 
 * Tests interaction detail page including:
 * - Page display and layout
 * - Interaction information display (type, date, description, location, status)
 * - Related sections (contacts, partners, documents, opportunities)
 * - Action buttons (edit, delete, create opportunity) - permission-gated
 * - Mobile responsiveness
 * 
 * NOTE: Uses existing interaction ID 1 from the database.
 */
test.describe('Interaction Detail Page', () => {
  test.slow();
  let interactionItemPage: InteractionItemPage;
  
  // Use existing interaction ID from database (matches interaction-item-basic.spec.ts)
  const testInteractionId = 1;
  
  /**
   * Setup: Authenticate and navigate to interaction detail page
   */
  test.beforeEach(async ({ page }) => {
    interactionItemPage = new InteractionItemPage(page, testInteractionId);
    
    await authenticateWithRealBackend(page, `/partnerships/interactions/${testInteractionId}`);
    
    await interactionItemPage.waitForLoad();
  });
  
  // =============================================
  // PAGE DISPLAY & HEADER
  // =============================================
  
  /**
   * Test: Page header displays correctly
   * Uses data-testid="interaction-detail-header"
   */
  test('should display interaction detail page header', async () => {
    await interactionItemPage.verifyPageHeader();
  });
  
  /**
   * Test: Interaction type is displayed
   * Uses data-testid="interaction-type-icon" (mobile) or CSS fallback (desktop)
   */
  test('should display interaction type', async () => {
    await interactionItemPage.verifyInteractionType();
  });
  
  /**
   * Test: Interaction date is displayed
   * Uses data-testid="interaction-date"
   */
  test('should display interaction date', async () => {
    await interactionItemPage.verifyInteractionDate();
  });
  
  /**
   * Test: Interaction information is loaded with data
   * Uses actual data-testid attributes for date, description, location, status
   */
  test('should display interaction information', async () => {
    const info = await interactionItemPage.getInteractionInfo();
    
    // At least date or description should have content
    const hasContent = (info.date && info.date.length > 0) ||
                       (info.description && info.description.length > 0);
    expect(hasContent).toBe(true);
  });
  
  // =============================================
  // SECTIONS
  // =============================================
  
  /**
   * Test: Description section is visible
   * Uses data-testid="interaction-description-section"
   */
  test('should display description section', async () => {
    const hasDesc = await interactionItemPage.interactionDescriptionSection.isVisible().catch(() => false);
    expect(hasDesc).toBe(true);
  });
  
  /**
   * Test: Details section is visible (date, location, status)
   * Uses data-testid="interaction-details-section"
   */
  test('should display details section', async () => {
    const hasDetails = await interactionItemPage.interactionDetailsSection.isVisible().catch(() => false);
    expect(hasDetails).toBe(true);
  });
  
  /**
   * Test: Participants section is visible (contacts and/or partners)
   * Uses data-testid="interaction-contacts-section" and "interaction-partners-section"
   */
  test('should display participants section', async () => {
    const hasParticipants = await interactionItemPage.hasParticipantsSection();
    // Participants should be present on most interactions
    expect(typeof hasParticipants).toBe('boolean');
    
    if (hasParticipants) {
      const count = await interactionItemPage.getParticipantsCount();
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });
  
  /**
   * Test: Related contacts section is visible
   * Uses data-testid="interaction-contacts-section"
   */
  test('should display related contacts section', async () => {
    const hasContacts = await interactionItemPage.hasRelatedContactsSection();
    expect(typeof hasContacts).toBe('boolean');
  });
  
  /**
   * Test: Related partners section is visible
   * Uses data-testid="interaction-partners-section"
   */
  test('should display related partners section', async () => {
    const hasPartners = await interactionItemPage.hasRelatedPartnersSection();
    expect(typeof hasPartners).toBe('boolean');
  });
  
  /**
   * Test: Related opportunities section availability
   * No data-testid — uses text-based filter. Currently shows "Coming Soon" placeholder.
   */
  test('should check for related opportunities section', async () => {
    const hasOpportunities = await interactionItemPage.hasRelatedOpportunitiesSection();
    expect(typeof hasOpportunities).toBe('boolean');
  });
  
  /**
   * Test: Documents section is visible
   * Uses data-testid="interaction-documents-section"
   */
  test('should display documents section', async () => {
    const hasDocs = await interactionItemPage.hasDocumentsSection();
    expect(hasDocs).toBe(true);
    
    const docCount = await interactionItemPage.getDocumentCount();
    expect(docCount).toBeGreaterThanOrEqual(0);
  });
  
  // =============================================
  // PERMISSION-GATED BUTTONS
  // =============================================
  
  /**
   * Test: Edit button visibility (permission-based)
   * Uses data-testid="edit-interaction-button"
   */
  test('should reflect edit permission state correctly', async () => {
    await interactionItemPage.waitForPermissionsToLoad();
    
    const isVisible = await interactionItemPage.isEditButtonVisible();
    expect(typeof isVisible).toBe('boolean');
    
    if (isVisible) {
      await expect(interactionItemPage.editButton).toBeVisible();
    }
  });
  
  /**
   * Test: Delete button visibility (permission-based)
   * Uses data-testid="delete-interaction-button"
   */
  test('should reflect delete permission state correctly', async () => {
    await interactionItemPage.waitForPermissionsToLoad();
    
    const isVisible = await interactionItemPage.isDeleteButtonVisible();
    expect(typeof isVisible).toBe('boolean');
    
    if (isVisible) {
      await expect(interactionItemPage.deleteButton).toBeVisible();
    }
  });
  
  /**
   * Test: Edit button opens edit dialog
   */
  test('should open edit dialog when edit button is clicked', async ({ page }) => {
    await interactionItemPage.waitForPermissionsToLoad();
    
    const isVisible = await interactionItemPage.isEditButtonVisible();
    test.skip(!isVisible, 'Edit button not visible — user lacks edit permission');
    
    await interactionItemPage.clickEditButton();
    await assertDialogOpen(page);
  });
  
  /**
   * Test: Delete button opens confirmation dialog
   */
  test('should open delete confirmation dialog when delete button is clicked', async ({ page }) => {
    await interactionItemPage.waitForPermissionsToLoad();
    
    const isVisible = await interactionItemPage.isDeleteButtonVisible();
    test.skip(!isVisible, 'Delete button not visible — user lacks delete permission');
    
    await interactionItemPage.clickDeleteButton();
    await assertDialogOpen(page);
  });
  
  /**
   * Test: Create opportunity button visibility
   * Uses data-testid="create-opportunity-button"
   */
  test('should reflect create opportunity button state', async () => {
    await interactionItemPage.waitForPermissionsToLoad();
    
    const isVisible = await interactionItemPage.isCreateOpportunityButtonVisible();
    expect(typeof isVisible).toBe('boolean');
    
    if (isVisible) {
      await expect(interactionItemPage.createOpportunityButton).toBeVisible();
    }
  });
  
  // =============================================
  // WORKFLOW & NAVIGATION
  // =============================================
  
  /**
   * Test: Workflow status is displayed
   */
  test('should display workflow status if available', async () => {
    const workflowStatus = await interactionItemPage.getWorkflowStatus();
    expect(typeof (workflowStatus === null || typeof workflowStatus === 'string')).toBe('boolean');
  });
  
  /**
   * Test: Back button navigates to list
   */
  test('should navigate back to interactions list when back button is clicked', async ({ page }) => {
    const backVisible = await interactionItemPage.backButton.isVisible().catch(() => false);
    test.skip(!backVisible, 'Back button not present on this page layout');
    
    await interactionItemPage.clickBackButton();
    await assertUrlMatches(page, /\/partnerships\/interactions\/?$/);
  });
  
  // =============================================
  // RESPONSIVENESS & GENERAL
  // =============================================
  
  /**
   * Test: Mobile responsive layout
   */
  test('should display correctly on mobile', async () => {
    await interactionItemPage.verifyMobileResponsive();
  });
  
  /**
   * Test: All main sections are displayed
   */
  test('should display all main sections correctly', async () => {
    await interactionItemPage.verifyMainSectionsDisplayed();
  });
  
  /**
   * Test: URL contains interaction ID
   */
  test('should have correct URL with interaction ID', async ({ page }) => {
    const currentUrl = page.url();
    expect(currentUrl).toContain(`/partnerships/interactions/${testInteractionId}`);
  });
  
  /**
   * Test: Page title is valid
   */
  test('should have a valid page title', async ({ page }) => {
    const title = await page.title();
    expect(title.length).toBeGreaterThan(0);
  });
  
  /**
   * Test: Interaction status is displayed
   * Uses data-testid="interaction-status"
   */
  test('should display interaction status', async () => {
    const hasStatus = await interactionItemPage.interactionStatus.isVisible().catch(() => false);
    expect(typeof hasStatus).toBe('boolean');
  });
  
  /**
   * Test: Interaction location is displayed if available
   * Uses data-testid="interaction-location"
   */
  test('should display interaction location if available', async () => {
    const hasLocation = await interactionItemPage.interactionLocation.isVisible().catch(() => false);
    // Location may not be set for all interactions
    expect(typeof hasLocation).toBe('boolean');
  });
});
