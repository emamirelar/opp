/**
 * @fileoverview axe-core Accessibility E2E Tests
 * Automated WCAG compliance testing using @axe-core/playwright.
 *
 * Prerequisites:
 *   npm install -D @axe-core/playwright
 *
 * Tests validate WCAG 2.1 Level A/AA compliance across key pages.
 * Run with: npx playwright test accessibility-axe.spec.ts
 *
 * @tests 15
 */

import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import { waitForPageReady, waitForLoadingToComplete, waitForPermissions, waitForDialog } from './helpers/wait.helper';
import { PartnersPage } from './pages/partners.page';

test.describe('Accessibility - axe-core WCAG Compliance', () => {
  test.slow();

  test('AXE-001: Login page passes axe-core scan', async ({ page }) => {
    await page.goto('/login');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-002: Home/Dashboard page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-003: Partners list page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-004: Partner detail page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners/1');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-005: Contacts list page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/contacts');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-006: Opportunities list page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-007: Opportunity detail page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/opportunities/1');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-008: Interactions list page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/interactions');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-009: New Partner dialog passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const partnersPage = new PartnersPage(page);
    const newButton = partnersPage.newButton.first();
    await newButton.click();
    await waitForDialog(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-010: Search results page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/search');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-011: Admin pages pass axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/entity-manager');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-012: AI Assistant page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/ai');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-013: User profile page passes axe-core scan', async ({ page }) => {
    await authenticateWithRealBackend(page, '/admin/user-management');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('AXE-014: No critical WCAG violations across all main pages', async ({ page }) => {
    const mainPages = [
      '/',
      '/partnerships/partners',
      '/partnerships/contacts',
      '/partnerships/opportunities',
      '/partnerships/interactions',
      '/search',
      '/ai',
    ];

    const allViolations: Array<{ page: string; violations: typeof import('axe-core').Result['violations'] }> = [];

    for (const route of mainPages) {
      await authenticateWithRealBackend(page, route);
      await waitForPageReady(page);
      await waitForLoadingToComplete(page);
      await waitForPermissions(page);

      const results = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
        .analyze();

      if (results.violations.length > 0) {
        allViolations.push({ page: route, violations: results.violations });
      }
    }

    const criticalViolations = allViolations.filter(
      (v) => v.violations.some((viol) => viol.impact === 'critical' || viol.impact === 'serious')
    );

    expect(
      criticalViolations,
      `Critical WCAG violations found: ${JSON.stringify(criticalViolations, null, 2)}`
    ).toEqual([]);
  });

  test('AXE-015: Color contrast meets WCAG AA requirements', async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners');
    await waitForPageReady(page);
    await waitForLoadingToComplete(page);
    await waitForPermissions(page);

    const results = await new AxeBuilder({ page })
      .withRules(['color-contrast'])
      .analyze();

    expect(results.violations).toEqual([]);
  });
});
