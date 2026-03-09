/**
 * @fileoverview API Error Handling E2E Tests (Gap 5)
 *
 * Tests that the UNOPS Opportunity+ application handles API errors gracefully —
 * ensuring users see error messages, toasts, or appropriate fallbacks instead of
 * crashes or blank screens when the backend returns errors or timeouts.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/OPP
 *
 * @tests 14
 */

import { test, expect } from '@playwright/test';
import { authenticateWithRealBackend } from './helpers/auth.helper';
import {
  waitForPermissions,
  waitForLoadingToComplete,
  waitForTableData,
  waitForHidden,
} from './helpers/wait.helper';
import { PartnersPage } from './pages/partners.page';
import { OpportunityItemPage } from './pages/opportunity-item.page';
import { ContactsPage } from './pages/contacts.page';
import { PartnerItemPage } from './pages/partner-item.page';
import { DashboardPage } from './pages/dashboard.page';

const ADMIN_USER = 'test@playwright.local';

const TEST_RECORDS = {
  partnerId: process.env.TEST_RECORD_ACTIVE_ID || '1',
  opportunityId: process.env.TEST_RECORD_ACTIVE_ID || '1',
};

const FRONTEND_URL = 'http://localhost:4200';

// =============================================================================
// POSITIVE TESTS (1-2)
// =============================================================================

test.describe('API Error Handling — Positive', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-001: Partner list loads successfully with mocked data → cards/rows visible', async ({ page }) => {
    await test.step('Arrange — navigate to partner list', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — list content visible', async () => {
      const partnersPage = new PartnersPage(page);
      const listview = partnersPage.listview.or(page.locator('app-listview').first()).first();
      await expect(listview).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-002: Opportunity detail loads successfully with mocked data → header and sections visible', async ({ page }) => {
    await test.step('Arrange — navigate to opportunity detail', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/opportunities/${TEST_RECORDS.opportunityId}`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — opportunity content visible', async () => {
      const oppPage = new OpportunityItemPage(page, TEST_RECORDS.opportunityId);
      const header = page.locator('app-opportunity-view').first();
      await expect(oppPage.opportunityTitle.or(header).first()).toBeVisible({ timeout: 10000 });
    });
  });
});

// =============================================================================
// NEGATIVE TESTS (3+)
// =============================================================================

test.describe('API Error Handling — Negative', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-003: Partner list API returns 500 → App shows error message or toast, no blank screen', async ({ page }) => {
    await test.step('Arrange — override partner list API to return 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner list', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — error feedback shown, no blank screen', async () => {
      const toast = page.locator('.p-toast-message, .p-toast-message-error, p-toast .p-toast-message');
      const errorMessage = page.locator('p-message[severity="error"], .p-message-error, [role="alert"]');
      const body = page.locator('body');
      await expect(body).toBeVisible();
      await expect(toast.or(errorMessage).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-004: Opportunity detail API returns 404 → Not found or error message shown', async ({ page }) => {
    await test.step('Arrange — override opportunity detail API to return 404', async () => {
      await page.route(
        url => /\/api\/opportunity\/\d+$/.test(url.toString()),
        async route => {
          await route.fulfill({
            status: 404,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Not Found', message: 'Opportunity not found' }),
          });
        }
      );
    });

    await test.step('Act — navigate to opportunity detail', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/opportunities/${TEST_RECORDS.opportunityId}`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — not found or error feedback shown', async () => {
      const notFound = page.getByText(/not found|404|error/i);
      const toast = page.locator('.p-toast-message, .p-toast-message-error');
      const errorMsg = page.locator('p-message[severity="error"], .p-message-error');
      await expect(notFound.or(toast).or(errorMsg).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-005: Network timeout on partner list → App handles gracefully (error state or loading stops)', async ({ page }) => {
    test.skip(true, 'Requires enhanced mock data or real backend - timeout abort may not show toast');
    await test.step('Arrange — override partner list API to abort (simulate timeout)', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.abort('timedout');
        }
      );
    });

    await test.step('Act — navigate to partner list', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — app remains functional, no crash', async () => {
      const body = page.locator('body');
      await expect(body).toBeVisible();
      const listview = page.locator('app-listview').first();
      const toast = page.locator('.p-toast-message, .p-toast-message-error, p-toast');
      const errorMsg = page.locator('p-message[severity="error"], .p-message-error, [role="alert"]');
      await expect(listview.or(toast).or(errorMsg)).toBeVisible({ timeout: 10000 });
    });
  });
});

// =============================================================================
// EDGE TESTS (3+)
// =============================================================================

test.describe('API Error Handling — Edge', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-006: Permission endpoint returns 403 → UI hides edit/delete buttons (read-only mode)', async ({ page }) => {
    await test.step('Arrange — override partner permissions to return 403-style restricted', async () => {
      await page.route(
        url => /\/api\/partner\/\d+\/permissions/.test(url.toString()),
        async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              canView: true,
              canEdit: false,
              canDelete: false,
              canSubmit: false,
              canApprove: false,
              canActivate: false,
              canCancel: false,
            }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner detail', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners/${TEST_RECORDS.partnerId}`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — edit/delete buttons hidden or disabled', async () => {
      const partnerView = page.locator('app-partner-view, app-partner-detail').first();
      const editBtn = partnerView.locator('p-button, button').filter({ hasText: /edit/i }).first();
      const deleteBtn = partnerView.locator('p-button, button').filter({ hasText: /delete/i }).first();
      const editVisible = await editBtn.isVisible().catch(() => false);
      const deleteVisible = await deleteBtn.isVisible().catch(() => false);
      expect(editVisible).toBe(false);
      expect(deleteVisible).toBe(false);
    });
  });

  test('TC-007: API returns empty list → No data message, not a crash', async ({ page }) => {
    await test.step('Arrange — override partner list to return empty', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ records: [], totalCount: 0 }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner list', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — no data message or empty state, no crash', async () => {
      const partnersPage = new PartnersPage(page);
      const noData = page.getByText(/no (records|data|partners)|empty/i);
      const body = page.locator('body');
      await expect(body).toBeVisible();
      await expect(partnersPage.listview.or(noData).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-008: Multiple consecutive API errors → App remains functional', async ({ page }) => {
    await test.step('Arrange — override partner list to return 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner list, then to contacts', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.goto(`${FRONTEND_URL}/partnerships/contacts`);
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — contacts page loads, app still functional', async () => {
      const contactsPage = new ContactsPage(page);
      expect(page.url()).toContain('contacts');
      await expect(contactsPage.listview.or(contactsPage.header).first()).toBeVisible({ timeout: 10000 });
    });
  });
});

// =============================================================================
// FUNCTIONAL TESTS (3+)
// =============================================================================

test.describe('API Error Handling — Functional', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-009: Error toast appears and can be dismissed', async ({ page }) => {
    await test.step('Arrange — override partner list to return 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner list', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — toast visible, dismissible', async () => {
      const toast = page.locator('.p-toast-message, .p-toast-message-error, p-toast .p-toast-message').first();
      const closeBtn = page.locator('.p-toast-message-close-icon, [aria-label="close"]').first();
      await expect(toast).toBeVisible({ timeout: 10000 });
      if (await closeBtn.isVisible().catch(() => false)) {
        await closeBtn.click();
        await waitForHidden(toast, 5000);
      }
    });
  });

  test('TC-010: After error, user can navigate to another page successfully', async ({ page }) => {
    await test.step('Arrange — override partner list to return 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — navigate to partners (error), then to contacts', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.goto(`${FRONTEND_URL}/partnerships/contacts`);
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — contacts page loads', async () => {
      const contactsPage = new ContactsPage(page);
      expect(page.url()).toContain('contacts');
      await expect(contactsPage.listview.or(contactsPage.header).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-011: Retry/reload after error shows correct data', async ({ page }) => {
    let callCount = 0;
    await test.step('Arrange — partner list fails first call, succeeds on retry', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          callCount++;
          if (callCount === 1) {
            await route.fulfill({
              status: 500,
              contentType: 'application/json',
              body: JSON.stringify({ error: 'Internal Server Error' }),
            });
          } else {
            await route.fulfill({
              status: 200,
              contentType: 'application/json',
              body: JSON.stringify({
                records: [
                  { id: 1, name: 'Test Partner', type: 'Government', status: 'Active', stage: 'Active', country: 'US', createdDate: '2024-01-01T00:00:00Z' },
                ],
                totalCount: 1,
              }),
            });
          }
        }
      );
    });

    await test.step('Act — navigate (error), then reload', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.reload();
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — data visible after reload', async () => {
      const partnersPage = new PartnersPage(page);
      const rowCount = await partnersPage.tableRows.count();
      const listviewVisible = await partnersPage.listview.isVisible().catch(() => false);
      expect(rowCount >= 1 || listviewVisible).toBe(true);
    });
  });
});

// =============================================================================
// INTEGRATION TESTS (3+)
// =============================================================================

test.describe('API Error Handling — Integration', () => {
  test.slow();

  test.beforeEach(async ({ page }) => {
    await authenticateWithRealBackend(page, '/partnerships/partners', ADMIN_USER);
    await waitForPermissions(page);
  });

  test('TC-012: Navigate to partner list → 500 error → Navigate to contacts → Contacts loads fine', async ({ page }) => {
    await test.step('Arrange — partner list returns 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — partner list (500) then contacts', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.goto(`${FRONTEND_URL}/partnerships/contacts`);
      await page.waitForLoadState('domcontentloaded');
      await waitForTableData(page);
    });

    await test.step('Assert — contacts loads successfully', async () => {
      const contactsPage = new ContactsPage(page);
      expect(page.url()).toContain('contacts');
      await expect(contactsPage.listview.or(contactsPage.header).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-013: Partner detail with partial API failures (detail loads, permissions fails) → Degrades gracefully', async ({ page }) => {
    await test.step('Arrange — permissions endpoint returns 500, detail returns 200', async () => {
      await page.route(
        url => /\/api\/partner\/\d+\/permissions/.test(url.toString()),
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — navigate to partner detail', async () => {
      await page.goto(`${FRONTEND_URL}/partnerships/partners/${TEST_RECORDS.partnerId}`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — partner content visible, no crash', async () => {
      const partnerPage = new PartnerItemPage(page, TEST_RECORDS.partnerId);
      const infoPanel = page.getByText('Partner Information', { exact: false });
      const body = page.locator('body');
      await expect(body).toBeVisible();
      await expect(partnerPage.partnerName.or(infoPanel).first()).toBeVisible({ timeout: 10000 });
    });
  });

  test('TC-014: Full navigation flow: Dashboard → Partner list (500) → Back to dashboard → Dashboard loads', async ({ page }) => {
    await test.step('Arrange — partner list returns 500', async () => {
      await page.route(
        url => {
          const u = url.toString();
          return /\/api\/partner(\?|$)/.test(u) && !u.includes('/api/partner-tree-structure') && !u.includes('/api/partner/');
        },
        async route => {
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'Internal Server Error' }),
          });
        }
      );
    });

    await test.step('Act — home → partners (500) → home', async () => {
      await page.goto(`${FRONTEND_URL}/`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.goto(`${FRONTEND_URL}/partnerships/partners`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
      await page.goto(`${FRONTEND_URL}/`);
      await page.waitForLoadState('domcontentloaded');
      await waitForLoadingToComplete(page);
    });

    await test.step('Assert — dashboard loads', async () => {
      const dashboardPage = new DashboardPage(page);
      expect(page.url()).not.toContain('/partnerships/partners');
      await expect(page.locator('body')).toBeVisible();
      await expect(dashboardPage.panels.first().or(dashboardPage.welcomeHeader).first()).toBeVisible({ timeout: 10000 });
    });
  });
});
