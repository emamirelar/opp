/**
 * @fileoverview Auth-Only API Mocking Helper
 * Mocks ONLY authentication and permission endpoints.
 * All data endpoints (partners, contacts, opportunities, etc.) pass through
 * to the real .NET backend via the Angular dev server proxy.
 *
 * Use with USE_REAL_API=true environment variable.
 */

import { Page } from '@playwright/test';

const DEBUG_MOCKS = process.env.PLAYWRIGHT_DEBUG_MOCKS === 'true';

function mockLog(message: string): void {
  if (DEBUG_MOCKS) {
    console.log(message);
  }
}

const RESTRICTED_MOCK_USERS = [
  'test-readonly@playwright.local',
  'test-no-permissions@playwright.local',
  'viewer@example.com',
  'doa2@example.com',
  'collaborator@example.com',
  'other-user@example.com',
  'partner.user@test.local',
  'general.user@test.local',
];

/**
 * Setup auth-only mocks — everything else flows to the real backend.
 * Only intercepts:
 *   - /user/claims (user identity)
 *   - /api/permissions/check/ (route guard permission checks)
 *   - /api/configuration (to inject test-safe config)
 *   - /api/dev/check-iap-simulation (IAP dev check)
 */
export async function setupAuthOnlyMocks(page: Page, userEmail?: string): Promise<void> {
  const isRestrictedUser = userEmail ? RESTRICTED_MOCK_USERS.includes(userEmail) : false;
  mockLog('[Auth-Only Mock] Setting up minimal auth route interceptions...');

  // Only mock /api/dev/check-iap-simulation — it may not exist on real backend
  await page.route(url => url.toString().includes('/api/dev/check-iap-simulation'), async (route) => {
    mockLog('[Auth-Only Mock] Intercepted: /api/dev/check-iap-simulation');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ isIapSimulation: true }),
    });
  });

  // Mock /api/permissions/check/ for ALL users — the real backend's permission
  // check endpoint requires full IAP authentication which we can't provide in tests.
  await page.route(url => url.toString().includes('/api/permissions/check/'), async (route) => {
    const requestUrl = route.request().url();
    const adminBlockedPaths = [
      'admin/entity-manager',
      'admin/user-management',
      'admin/translations',
      'admin/ai-prompt-management',
      'admin/entity-artifacts',
      'admin/bulk-entity-artifacts',
    ];
    const isAdminRoute = adminBlockedPaths.some(p => requestUrl.includes(p));
    const hasAccess = isRestrictedUser ? !isAdminRoute : true;

    mockLog(`[Auth-Only Mock] Intercepted: /api/permissions/check/ (restricted=${isRestrictedUser}, hasAccess=${hasAccess})`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        hasAccess,
        route: requestUrl,
        entity: 'Contact',
        permissions: isRestrictedUser ? {
          canRead: !isAdminRoute,
          canCreate: false,
          canUpdate: false,
          canDelete: false,
          canExport: false,
          canImport: false,
          canApprove: false,
          canActivate: false,
          canClose: false,
          canArchive: false,
        } : {
          canRead: true,
          canCreate: true,
          canUpdate: true,
          canDelete: true,
          canExport: true,
          canImport: true,
          canApprove: false,
          canActivate: false,
          canClose: false,
          canArchive: false,
        },
      }),
    });
  });

  mockLog('[Auth-Only Mock] Auth-only routes configured — data endpoints will hit real backend');
}
