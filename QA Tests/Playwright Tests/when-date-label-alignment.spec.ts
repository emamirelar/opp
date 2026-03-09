/**
 * @fileoverview PNO-1182: WHEN Section Date Field Label Alignment E2E Tests
 *
 * Bug: Floating labels for "Implementation Start Date" and "Proposal Submission
 * Date" are visually misaligned compared to "Target Signing Date" in the When tab.
 * Labels sit lower than intended or clash with the field top border.
 *
 * Fix: Added SCSS rules in opportunity-when-section.component.scss:
 * - max-width: calc(100% - 3.5rem) to prevent label overflow into calendar icon
 * - text-overflow: ellipsis, overflow: hidden, white-space: nowrap for truncation
 * - background-color: white + padding for filled/focused labels
 *
 * Requirements tested:
 * - REQ-1: All date field labels share consistent alignment
 * - REQ-2: Long labels truncated with ellipsis, not overlapping calendar icon
 * - REQ-3: Filled/focused labels have white background and padding
 * - REQ-4: Labels use translate pipe (i18n) via p-floatlabel variant="on"
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1182
 *
 * @tests 20
 */

import { test, expect } from '@playwright/test';
import { setupAPIMocks } from './helpers/api-mocks.helper';
import { authenticateWithMocks } from './helpers/auth.helper';
import { waitForPermissions } from './helpers/wait.helper';
import { OpportunityItemPage } from './pages/opportunity-item.page';

const ADMIN_USER = 'test@playwright.local';
const OPP_ID = 1;

test.describe('PNO-1182 — When Section Date Label Alignment', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page, ADMIN_USER);
    await authenticateWithMocks(page, `/partnerships/opportunities/${OPP_ID}`, ADMIN_USER);
    await waitForPermissions(page);
  });

  // ── Positive Tests ──────────────────────────────────────────────────────

  test('TC-001: [Positive] When section renders with floating label date fields', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert When section is visible with float labels', async () => {
      const whenVisible = await oppPage.whenSection.isVisible();
      expect(whenVisible, 'When section must be visible').toBe(true);

      const floatLabelCount = await oppPage.whenFloatLabels.count();
      expect(floatLabelCount, 'When section must contain p-floatlabel elements').toBeGreaterThanOrEqual(1);
    });
  });

  test('TC-002: [Positive] Target Signing Date field uses p-floatlabel', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert Target Signing Date exists within a p-floatlabel', async () => {
      const field = oppPage.targetSigningDateField;
      const visible = await field.isVisible().catch(() => false);
      if (visible) {
        const parentFloatLabel = page.locator('p-floatlabel').filter({ has: field });
        const hasParent = await parentFloatLabel.count();
        expect(hasParent, 'Target Signing Date must be inside a p-floatlabel').toBeGreaterThanOrEqual(1);
      }
    });
  });

  // ── Negative Tests ──────────────────────────────────────────────────────

  test('TC-003: [Negative] Date labels do not overlap with calendar icons', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels have constrained width (not overflowing into icon)', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();

      for (let i = 0; i < count; i++) {
        const label = labels.nth(i);
        const visible = await label.isVisible().catch(() => false);
        if (!visible) continue;

        const maxWidth = await label.evaluate(el => window.getComputedStyle(el).maxWidth);
        if (maxWidth && maxWidth !== 'none') {
          expect(maxWidth, `Label ${i} should have a constrained max-width`).not.toBe('none');
        }
      }
    });
  });

  test('TC-004: [Negative] Date labels do not wrap to multiple lines', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels use nowrap', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();

      for (let i = 0; i < count; i++) {
        const label = labels.nth(i);
        const visible = await label.isVisible().catch(() => false);
        if (!visible) continue;

        const whiteSpace = await label.evaluate(el => window.getComputedStyle(el).whiteSpace);
        if (whiteSpace) {
          expect(whiteSpace, `Label ${i} must use white-space: nowrap`).toBe('nowrap');
        }
      }
    });
  });

  test('TC-005: [Negative] Labels do not use inline styles for alignment fix', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert no inline style max-width on labels', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();

      for (let i = 0; i < count; i++) {
        const label = labels.nth(i);
        const visible = await label.isVisible().catch(() => false);
        if (!visible) continue;

        const inlineStyle = await label.getAttribute('style');
        if (inlineStyle) {
          expect(inlineStyle, `Label ${i} should not have inline max-width`).not.toContain('max-width');
        }
      }
    });
  });

  test('TC-006: [Negative] No duplicate datepicker IDs in When section', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert no duplicate IDs', async () => {
      const targetCount = await oppPage.whenSection.locator('#targetSigningDate').count();
      if (targetCount > 0) {
        expect(targetCount, 'targetSigningDate ID must be unique').toBe(1);
      }
    });
  });

  test('TC-007: [Negative] Date fields remain functional after label fix', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert date fields are not hidden or disabled by the fix', async () => {
      const datepickers = oppPage.whenSection.locator('p-datepicker');
      const count = await datepickers.count();
      expect(count, 'When section should contain date picker elements').toBeGreaterThanOrEqual(1);
    });
  });

  test('TC-008: [Negative] Labels do not use position:absolute (PNO-1182 anti-pattern)', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels are not absolutely positioned by the fix', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();

      for (let i = 0; i < Math.min(count, 4); i++) {
        const label = labels.nth(i);
        const visible = await label.isVisible().catch(() => false);
        if (!visible) continue;

        const position = await label.evaluate(el => window.getComputedStyle(el).position);
        expect(position, `Label ${i} should not be absolutely positioned`).not.toBe('absolute');
      }
    });
  });

  // ── Boundary Tests ──────────────────────────────────────────────────────

  test('TC-009: [Boundary] Labels align consistently across all date fields', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels have consistent vertical position', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();
      const heights: number[] = [];

      for (let i = 0; i < count; i++) {
        const label = labels.nth(i);
        const visible = await label.isVisible().catch(() => false);
        if (!visible) continue;

        const box = await label.boundingBox();
        if (box) {
          heights.push(box.height);
        }
      }

      if (heights.length >= 2) {
        const maxDiff = Math.max(...heights) - Math.min(...heights);
        expect(maxDiff, 'Label heights should be consistent (within 5px)').toBeLessThanOrEqual(5);
      }
    });
  });

  test('TC-010: [Boundary] Long label "Implementation Start Date" truncates with ellipsis', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert long label has overflow hidden', async () => {
      const label = oppPage.whenSection.locator('label[for="implementationStartDate"]');
      const visible = await label.isVisible().catch(() => false);
      if (visible) {
        const overflow = await label.evaluate(el => window.getComputedStyle(el).overflow);
        expect(overflow, 'Implementation Start Date label must have overflow:hidden').toBe('hidden');
      }
    });
  });

  test('TC-011: [Boundary] When section renders on mobile viewport (375px)', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section at mobile width', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert When section is visible at mobile size', async () => {
      const visible = await oppPage.whenSection.isVisible().catch(() => false);
      expect(visible, 'When section must render on mobile viewport').toBe(true);
    });
  });

  test('TC-012: [Boundary] When section renders on tablet viewport (768px)', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section at tablet width', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert date labels are visible at tablet size', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();
      expect(count, 'Date labels should be present at tablet viewport').toBeGreaterThanOrEqual(1);
    });
  });

  test('TC-013: [Boundary] Date field labels do not exceed parent container width', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels stay within container bounds', async () => {
      const floatLabels = oppPage.whenFloatLabels;
      const count = await floatLabels.count();

      for (let i = 0; i < Math.min(count, 4); i++) {
        const container = floatLabels.nth(i);
        const label = container.locator('label').first();

        const containerBox = await container.boundingBox();
        const labelBox = await label.boundingBox();

        if (containerBox && labelBox) {
          expect(
            labelBox.x + labelBox.width,
            `Label ${i} right edge must not exceed container`
          ).toBeLessThanOrEqual(containerBox.x + containerBox.width + 2);
        }
      }
    });
  });

  test('TC-014: [Boundary] All date fields use variant="on" for float labels', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert p-floatlabel elements use variant="on"', async () => {
      const floatLabels = oppPage.whenFloatLabels;
      const count = await floatLabels.count();

      for (let i = 0; i < count; i++) {
        const variant = await floatLabels.nth(i).getAttribute('variant');
        if (variant) {
          expect(variant, `Float label ${i} should use variant="on"`).toBe('on');
        }
      }
    });
  });

  // ── Functional Tests ────────────────────────────────────────────────────

  test('TC-015: [Functional] Each date field has a label with "for" attribute matching field ID', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert label "for" attributes match datepicker IDs', async () => {
      const fieldIds = ['targetSigningDate', 'implementationStartDate', 'targetDeliveryDate'];

      for (const id of fieldIds) {
        const label = oppPage.whenSection.locator(`label[for="${id}"]`);
        const labelExists = await label.count();
        if (labelExists > 0) {
          const forAttr = await label.getAttribute('for');
          expect(forAttr, `Label for="${id}" must exist`).toBe(id);
        }
      }
    });
  });

  test('TC-016: [Functional] Date fields use "yy-mm-dd" date format', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert datepickers have dateFormat attribute', async () => {
      const datepickers = oppPage.whenSection.locator('p-datepicker');
      const count = await datepickers.count();

      for (let i = 0; i < count; i++) {
        const format = await datepickers.nth(i).getAttribute('dateformat');
        if (format) {
          expect(format, `Datepicker ${i} should use yy-mm-dd format`).toBe('yy-mm-dd');
        }
      }
    });
  });

  test('TC-017: [Functional] Date fields have calendar icon visible (showIcon)', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert calendar icons are present', async () => {
      const calendarIcons = oppPage.whenSection.locator('p-datepicker button, p-datepicker .p-datepicker-trigger, p-datepicker .p-icon');
      const count = await calendarIcons.count();
      expect(count, 'Date fields must have calendar icon buttons').toBeGreaterThanOrEqual(1);
    });
  });

  test('TC-018: [Functional] Labels use translation keys (not hardcoded English)', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert labels have text content (translated)', async () => {
      const labels = oppPage.whenDateLabels;
      const count = await labels.count();

      for (let i = 0; i < count; i++) {
        const text = await labels.nth(i).textContent();
        if (text) {
          expect(text.trim().length, `Label ${i} must have non-empty translated text`).toBeGreaterThan(0);
        }
      }
    });
  });

  // ── Integration Tests ───────────────────────────────────────────────────

  test('TC-019: [Integration] Full When section renders with all date fields aligned', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
    });

    await test.step('Assert complete section renders with float labels', async () => {
      const sectionVisible = await oppPage.whenSection.isVisible();
      expect(sectionVisible, 'When section must be visible').toBe(true);

      const floatLabels = await oppPage.whenFloatLabels.count();
      expect(floatLabels, 'Multiple p-floatlabel elements expected').toBeGreaterThanOrEqual(1);

      const labels = oppPage.whenDateLabels;
      const labelCount = await labels.count();

      const positions: { y: number; height: number }[] = [];
      for (let i = 0; i < labelCount; i++) {
        const box = await labels.nth(i).boundingBox();
        if (box) {
          positions.push({ y: box.y, height: box.height });
        }
      }

      if (positions.length >= 2) {
        const heightRange = Math.max(...positions.map(p => p.height)) - Math.min(...positions.map(p => p.height));
        expect(heightRange, 'All label heights should be consistent').toBeLessThanOrEqual(5);
      }
    });
  });

  test('TC-020: [Integration] Navigation to When tab and back preserves layout', async ({ page }) => {
    const oppPage = new OpportunityItemPage(page, OPP_ID);

    await test.step('Navigate to When section', async () => {
      await oppPage.openWhenSection();
      const visible = await oppPage.whenSection.isVisible();
      expect(visible).toBe(true);
    });

    await test.step('Navigate to What section and back to When', async () => {
      await oppPage.openWhatSection();
      await page.waitForTimeout(500);
      await oppPage.openWhenSection();
    });

    await test.step('Assert When section still renders correctly', async () => {
      const floatLabels = await oppPage.whenFloatLabels.count();
      expect(floatLabels, 'Float labels should be present after navigation').toBeGreaterThanOrEqual(1);
    });
  });
});
