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

  // ==========================================
  // FORM DATA ENDPOINTS - Required for dialog rendering
  // ==========================================
  
  // Mock /api/values/partners - Partners dropdown
  await page.route(url => url.toString().includes('/api/values/partners'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/partners');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'Test Partner 1', type: 'Government' },
        { id: 2, name: 'Test Partner 2', type: 'NGO' },
        { id: 3, name: 'Test Partner 3', type: 'Private Sector' },
      ]),
    });
  });

  // Mock /api/values/organization-units - Organization units dropdown
  await page.route(url => url.toString().includes('/api/values/organization-units'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/organization-units');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'HQ - Headquarters', code: 'HQ' },
        { id: 2, name: 'RO - Regional Office', code: 'RO' },
        { id: 3, name: 'CO - Country Office', code: 'CO' },
      ]),
    });
  });

  // Mock /api/partner-tree-structure - Hierarchical partner structure
  await page.route(url => url.toString().includes('/api/partner-tree-structure'), async (route) => {
    console.log('[API Mock] Intercepted: /api/partner-tree-structure');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        {
          id: 1,
          name: 'Test Partner 1',
          children: [
            { id: 11, name: 'Test Partner 1 - Division A', children: [] },
            { id: 12, name: 'Test Partner 1 - Division B', children: [] },
          ],
        },
        {
          id: 2,
          name: 'Test Partner 2',
          children: [],
        },
      ]),
    });
  });

  // Mock /api/values/liaison-offices - Liaison offices dropdown
  await page.route(url => url.toString().includes('/api/values/liaison-offices'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/liaison-offices');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'New York Office', location: 'USA' },
        { id: 2, name: 'Geneva Office', location: 'Switzerland' },
        { id: 3, name: 'Copenhagen Office', location: 'Denmark' },
      ]),
    });
  });

  // Mock /api/values/contacts - Contacts dropdown
  await page.route(url => url.toString().includes('/api/values/contacts'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/contacts');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'John Smith', email: 'john.smith@test.com' },
        { id: 2, name: 'Jane Doe', email: 'jane.doe@test.com' },
        { id: 3, name: 'Bob Johnson', email: 'bob.johnson@test.com' },
      ]),
    });
  });

  // Mock /api/values/users/paged - Users paged endpoint (POST)
  await page.route(url => url.toString().includes('/api/values/users/paged'), async (route) => {
    console.log('[API Mock] Intercepted: POST /api/values/users/paged');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          { id: 1, name: 'Test User 1', email: 'user1@unops.org' },
          { id: 2, name: 'Test User 2', email: 'user2@unops.org' },
          { id: 3, name: 'Test User 3', email: 'user3@unops.org' },
        ],
        totalCount: 3,
        pageIndex: 1,
        pageSize: 20,
      }),
    });
  });

  // ==========================================
  // REFERENCE DATA ENDPOINTS - Required for CachedDataService
  // These are loaded globally on app init for dropdown options
  // ==========================================
  
  // Mock /api/values/salutations - Salutation dropdown (Mr., Ms., Dr., etc.)
  await page.route(url => url.toString().includes('/api/values/salutations'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/salutations');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'Mr.' },
        { id: 2, name: 'Ms.' },
        { id: 3, name: 'Mrs.' },
        { id: 4, name: 'Dr.' },
        { id: 5, name: 'Prof.' },
      ]),
    });
  });

  // Mock /api/values/status - Status dropdown (Active, Inactive, etc.)
  await page.route(url => url.toString().includes('/api/values/status'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/status');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'Active' },
        { id: 2, name: 'Inactive' },
      ]),
    });
  });

  // Mock /api/values/pronouns - Pronouns dropdown
  await page.route(url => url.toString().includes('/api/values/pronouns'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/pronouns');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'He/Him' },
        { id: 2, name: 'She/Her' },
        { id: 3, name: 'They/Them' },
        { id: 4, name: 'Other' },
      ]),
    });
  });

  // Mock /api/values/countries - Countries dropdown
  await page.route(url => url.toString().includes('/api/values/countries'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/countries');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 'US', name: 'United States', code: 'US' },
        { id: 'GB', name: 'United Kingdom', code: 'GB' },
        { id: 'FR', name: 'France', code: 'FR' },
        { id: 'DE', name: 'Germany', code: 'DE' },
        { id: 'CH', name: 'Switzerland', code: 'CH' },
        { id: 'DK', name: 'Denmark', code: 'DK' },
      ]),
    });
  });

  // Mock /api/values/states - States/Provinces dropdown
  await page.route(url => url.toString().includes('/api/values/states'), async (route) => {
    console.log('[API Mock] Intercepted: /api/values/states');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 'NY', name: 'New York', countryCode: 'US' },
        { id: 'CA', name: 'California', countryCode: 'US' },
        { id: 'TX', name: 'Texas', countryCode: 'US' },
      ]),
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
           !urlString.includes('/api/global/preferred-language') &&
           !urlString.includes('/api/values/partners') &&
           !urlString.includes('/api/values/organization-units') &&
           !urlString.includes('/api/partner-tree-structure') &&
           !urlString.includes('/api/values/liaison-offices') &&
           !urlString.includes('/api/values/contacts') &&
           !urlString.includes('/api/values/users/paged') &&
           !urlString.includes('/api/values/salutations') &&
           !urlString.includes('/api/values/status') &&
           !urlString.includes('/api/values/pronouns') &&
           !urlString.includes('/api/values/countries') &&
           !urlString.includes('/api/values/states');
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
 * Setup camera/MediaDevices API mocks for business card scanner
 * @param page - Playwright page object
 */
export async function setupCameraMocks(page: Page): Promise<void> {
  console.log('[API Mock] Setting up camera/MediaDevices mocks...');
  
  await page.addInitScript(() => {
    // Mock getUserMedia for camera access
    if (navigator.mediaDevices) {
      navigator.mediaDevices.getUserMedia = async (constraints: MediaStreamConstraints) => {
        console.log('[Camera Mock] getUserMedia called with constraints:', constraints);
        
        // ✅ Create a real MediaStream using canvas captureStream for browser compatibility
        // This creates an actual MediaStream that can be assigned to video.srcObject
        const canvas = document.createElement('canvas');
        canvas.width = 1280;
        canvas.height = 720;
        
        // Draw a test pattern so the video element has content
        const ctx = canvas.getContext('2d');
        if (ctx) {
          ctx.fillStyle = '#1a1a1a';
          ctx.fillRect(0, 0, canvas.width, canvas.height);
          ctx.fillStyle = '#00ff00';
          ctx.font = '48px Arial';
          ctx.textAlign = 'center';
          ctx.fillText('MOCK CAMERA', canvas.width / 2, canvas.height / 2);
          ctx.fillText('Test Environment', canvas.width / 2, canvas.height / 2 + 60);
        }
        
        // ✅ captureStream() returns a REAL MediaStream that the browser accepts
        const stream = canvas.captureStream(30); // 30 FPS
        
        // Add required methods to the stream
        const originalGetTracks = stream.getTracks.bind(stream);
        stream.getTracks = () => {
          const tracks = originalGetTracks();
          // Enhance tracks with required methods if not present
          tracks.forEach(track => {
            if (!track.getSettings) {
              (track as any).getSettings = () => ({
                width: 1280,
                height: 720,
                aspectRatio: 16/9,
                frameRate: 30,
                facingMode: 'environment',
              });
            }
          });
          return tracks;
        };
        
        console.log('[Camera Mock] Created real MediaStream from canvas');
        return Promise.resolve(stream);
      };
      
      // Mock enumerateDevices
      navigator.mediaDevices.enumerateDevices = async () => {
        console.log('[Camera Mock] enumerateDevices called');
        return [
          {
            kind: 'videoinput',
            deviceId: 'mock-camera-1',
            label: 'Mock Camera (front)',
            groupId: 'mock-group-1',
            toJSON: () => ({}),
          },
          {
            kind: 'videoinput',
            deviceId: 'mock-camera-2',
            label: 'Mock Camera (back)',
            groupId: 'mock-group-1',
            toJSON: () => ({}),
          },
        ] as MediaDeviceInfo[];
      };
      
      // Mock getSupportedConstraints
      navigator.mediaDevices.getSupportedConstraints = () => ({
        aspectRatio: true,
        facingMode: true,
        frameRate: true,
        height: true,
        width: true,
        deviceId: true,
      });
    }
  });
  
  console.log('[API Mock] Camera/MediaDevices mocks configured');
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
