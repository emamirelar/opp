/**
 * @fileoverview API Mocking Helper
 * Provides route mocking for backend APIs during E2E tests
 */

import { Page } from '@playwright/test';

/**
 * Setup API mocks for authentication and configuration
 * @param page - Playwright page object
 */
export async function setupAPIMocks(page: Page): Promise<void> {
  console.log('[API Mock] Setting up route interceptions...');
  
  // Mock /api/configuration endpoint
  await page.route(url => url.toString().includes('/api/configuration'), async (route) => {
    console.log('[API Mock] Intercepted: /api/configuration');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        // Mock configuration data
        appName: 'Opportunity+',
        version: '1.0.0',
        environment: 'test',
        // Mock Google API credentials to suppress console errors
        googleClientId: 'mock-google-client-id-for-testing',
        googleApiKey: 'mock-google-api-key-for-testing',
      }),
    });
  });

  // Mock /user/claims endpoint - Return empty array (not authenticated)
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    console.log('[API Mock] Intercepted: /user/claims');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]), // Empty array = not authenticated
    });
  });

  // Mock /api/global/preferred-language endpoint
  await page.route(url => url.toString().includes('/api/global/preferred-language'), async (route) => {
    console.log('[API Mock] Intercepted: /api/global/preferred-language');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ language: 'en' }), // Default to English
    });
  });

  // Mock /user/login endpoint - Authentication endpoint (must be before catch-all)
  await page.route(url => url.toString().includes('/user/login'), async (route) => {
    console.log('[API Mock] Intercepted: /user/login (authentication)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
      // Set authentication cookie
      headers: {
        'Set-Cookie': 'dev-user-email=test@unops.org; Path=/; HttpOnly'
      }
    });
  });

  // Mock /user/register endpoint
  await page.route(url => url.toString().includes('/user/register'), async (route) => {
    console.log('[API Mock] Intercepted: /user/register');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock /user/googleSignIn endpoint
  await page.route(url => url.toString().includes('/user/googleSignIn'), async (route) => {
    console.log('[API Mock] Intercepted: /user/googleSignIn');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Catch-all for any other /api/ and /user/ calls - return smart defaults based on URL pattern
  await page.route(url => {
    const urlString = url.toString();
    return (urlString.includes('/api/') || urlString.includes('/user/')) && 
           !urlString.includes('/api/configuration') &&
           !urlString.includes('/user/claims') &&
           !urlString.includes('/user/login') &&
           !urlString.includes('/user/register') &&
           !urlString.includes('/user/googleSignIn') &&
           !urlString.includes('/api/global/preferred-language');
  }, async (route) => {
    const url = route.request().url();
    const method = route.request().method();
    console.log(`[API Mock] Catch-all intercepted: ${method} ${url}`);
    
    // Smart responses based on URL patterns
    if (method === 'GET') {
      // Permission check endpoints - return correct structure matching Angular PermissionService expectations
      if (url.includes('/api/permissions/check/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            hasAccess: true, // ✅ Required field
            route: url, // ✅ Required field
            entity: 'Contact', // ✅ Required field
            permissions: {
              canRead: true, // ✅ Note: canRead, not canView
              canCreate: true,
              canUpdate: true, // ✅ Note: canUpdate, not canEdit
              canDelete: true,
              canExport: true,
              canImport: true,
              canApprove: false,
              canActivate: false,
              canClose: false,
              canArchive: false,
            }
          }),
        });
      }
      // Entity configuration endpoints - return array of columns directly
      else if (url.includes('/api/entity-configuration/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]), // Returns empty array of columns
        });
      }
      // Role endpoints
      else if (url.includes('/api/role/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // Organization hierarchy - expects array, not object
      else if (url.includes('/api/organization-hierarchy')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]), // Returns array directly
        });
      }
      // User preferences
      else if (url.includes('/api/user-preferences/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({}),
        });
      }
      // Dashboard content
      else if (url.includes('/api/dashboard/content')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            recentUpdates: [],
            quickStats: {},
          }),
        });
      }
      // User info
      else if (url.includes('/api/user-info/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: '12345',
            email: 'test@unops.org',
            name: 'Test User',
          }),
        });
      }
      // AI assistant
      else if (url.includes('/ai-assistant/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // Document types
      else if (url.includes('/api/document-type/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // Default for other GET requests
      else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
    } else {
      // For POST/PUT/DELETE, return success
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      });
    }
  });

  console.log('[API Mock] All API routes configured (including catch-all)');
}

/**
 * Setup authenticated user claims
 * @param page - Playwright page object
 * @param email - User email
 */
export async function setupAuthenticatedUserMock(page: Page, email: string): Promise<void> {
  // Mock /user/claims endpoint - Return authenticated user claims
  await page.unroute(url => url.toString().includes('/user/claims')); // Remove existing mock
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    console.log('[API Mock] Intercepted: /user/claims (authenticated)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { type: 'email', value: email },
        { type: 'name', value: 'Test User' },
        { type: 'role', value: 'Administrator' },
      ]),
    });
  });
}

/**
 * Clear all API mocks
 * @param page - Playwright page object
 */
export async function clearAPIMocks(page: Page): Promise<void> {
  // Unroute all routes - Playwright allows unrouting all at once
  await page.unrouteAll({ behavior: 'ignoreErrors' });
  console.log('[API Mock] All API routes cleared');
}
