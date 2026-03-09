/**
 * @fileoverview Authentication Helper
 * Provides reusable authentication functions for E2E tests
 */

import { Page } from '@playwright/test';
import { getTestCredentials, getTimeout } from './test-config';
import { setupAPIMocks } from './api-mocks.helper';
import { setupAuthOnlyMocks } from './auth-only-mocks.helper';
import { waitForPageReady, waitForAngularReady } from './wait.helper';
import path from 'path';

/**
 * QA-041 FIX: Conditional logging to prevent memory exhaustion in large suite runs.
 * Enable verbose auth logs with PLAYWRIGHT_DEBUG_AUTH=true environment variable.
 */
const DEBUG_AUTH = process.env.PLAYWRIGHT_DEBUG_AUTH === 'true' || process.env.PLAYWRIGHT_DEBUG_MOCKS === 'true';

function authLog(message: string): void {
  if (DEBUG_AUTH) {
    console.log(message);
  }
}

/**
 * ✅ REAL BACKEND AUTHENTICATION (Cookie-Based)
 * Use this for testing with real backend at http://localhost:5159
 * 
 * This matches the proven approach from contacts.spec.ts that successfully
 * authenticates with the development backend using IAP simulation cookies.
 * 
 * Prerequisites:
 * - Backend running at http://localhost:5159
 * - Test user exists: test@playwright.local with Administrator role
 * - Created via setup-test-user.sql
 */

/**
 * Authenticate with real backend using development cookies
 * 
 * UPDATE 2026-02-02: Now includes API mocks to ensure permission checks pass
 * when backend is not running. This fixes DEF-001 (route guard blocking access).
 * See QA Tests/DEF-001_RouteGuard_DeepAnalysis.md for full analysis.
 * 
 * @param page - Playwright page object
 * @param targetUrl - URL to navigate to after authentication
 * @param testUserEmail - Email of test user (default: test@playwright.local)
 */
/**
 * Role configuration for non-admin test users.
 * Any email NOT in this map defaults to Administrator + Internal roles.
 * 
 * QA-039 FIX: Previously all users received Administrator claims regardless
 * of the testUserEmail parameter. Now restricted users get appropriate roles.
 */
const RESTRICTED_TEST_USERS: Record<string, { roles: string[]; isInternal: boolean; name: string }> = {
  'test-readonly@playwright.local': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test Readonly User',
  },
  'test-no-permissions@playwright.local': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test No-Permissions User',
  },
  'viewer@example.com': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test Viewer',
  },
  'doa2@example.com': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test DoA2 Approver',
  },
  'other-user@example.com': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test Other User',
  },
  /** Collaborator: can edit content but cannot perform workflow actions (Submit, Cancel, Reopen) */
  'collaborator@example.com': {
    roles: ['UNOPS_GEN_USER'],
    isInternal: true,
    name: 'Test Collaborator',
  },
};

/**
 * Mock-only authentication: sets up full API mocks and navigates to the target URL.
 * Use when tests do NOT require a real backend — all API responses are mocked.
 * API mocks must be set up via setupAPIMocks() BEFORE calling this function.
 */
export async function authenticateWithMocks(
  page: Page,
  targetUrl: string,
  testUserEmail: string = 'test@playwright.local'
): Promise<void> {
  await page.context().clearCookies();

  const restrictedUser = RESTRICTED_TEST_USERS[testUserEmail];
  const claims = restrictedUser
    ? [
        { type: 'email', value: testUserEmail },
        { type: 'name', value: restrictedUser.name },
        ...restrictedUser.roles.map(role => ({ type: 'role', value: role })),
        { type: 'IsInternal', value: String(restrictedUser.isInternal) },
        { type: 'IAPAuthenticated', value: 'true' },
        { type: 'sub', value: '99999' },
        { type: 'userId', value: '99999' },
      ]
    : [
        { type: 'email', value: testUserEmail },
        { type: 'name', value: 'Test User' },
        { type: 'role', value: 'Administrator' },
        { type: 'role', value: 'Internal' },
        { type: 'IsInternal', value: 'true' },
        { type: 'IAPAuthenticated', value: 'true' },
        { type: 'sub', value: '12345' },
        { type: 'userId', value: '12345' },
      ];

  await page.unroute(url => url.toString().includes('/user/claims'));
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(claims),
    });
  });

  await page.context().addCookies([{
    name: 'dev-user-email',
    value: testUserEmail,
    domain: '127.0.0.1',
    path: '/',
  }]);

  const baseUrl = process.env.BASE_URL || 'http://localhost:4200';
  const url = targetUrl.startsWith('http') ? targetUrl : `${baseUrl}${targetUrl}`;
  authLog(`[Auth-Mocks] Navigating to ${url}`);
  await page.goto(url, { waitUntil: 'domcontentloaded', timeout: getTimeout('navigation') });
  await page.waitForLoadState('networkidle').catch(() => {});
}

export async function authenticateWithRealBackend(
  page: Page,
  targetUrl: string,
  testUserEmail: string = process.env.USE_REAL_API === 'true'
    ? (process.env.TEST_USER_EMAIL || 'test@playwright.local')
    : 'test@playwright.local'
): Promise<void> {
  // Step 1: Clear all cookies
  await page.context().clearCookies();
  
  // Step 2: Setup API mocks BEFORE navigation (FIX for DEF-001)
  // When USE_REAL_API=true, only mock auth/permissions — let data flow to real backend.
  // Otherwise, mock everything (original behavior for environments without backend).
  const useRealApi = process.env.USE_REAL_API === 'true';
  if (useRealApi) {
    authLog('[Auth] USE_REAL_API=true — setting up auth-only mocks (data flows to real backend)...');
    await setupAuthOnlyMocks(page, testUserEmail);
  } else {
    authLog('[Auth] Setting up full API mocks (no real backend)...');
    await setupAPIMocks(page, testUserEmail);
  }
  
  // Step 3: Setup authenticated user claims mock
  // QA-039 FIX: Differentiate claims based on user email
  const restrictedUser = RESTRICTED_TEST_USERS[testUserEmail];
  const claims = restrictedUser
    ? [
        { type: 'email', value: testUserEmail },
        { type: 'name', value: restrictedUser.name },
        ...restrictedUser.roles.map(role => ({ type: 'role', value: role })),
        { type: 'IsInternal', value: String(restrictedUser.isInternal) },
        { type: 'IAPAuthenticated', value: 'true' },
        { type: 'sub', value: '99999' },
        { type: 'userId', value: '99999' }, // Required for topbar notification polling
      ]
    : [
        // Default: Administrator (backwards-compatible for all existing tests)
        { type: 'email', value: testUserEmail },
        { type: 'name', value: 'Test User' },
        { type: 'role', value: 'Administrator' },
        { type: 'role', value: 'Internal' },
        { type: 'IsInternal', value: 'true' },
        { type: 'IAPAuthenticated', value: 'true' },
        { type: 'sub', value: '12345' },
        { type: 'userId', value: '12345' }, // Required for topbar notification polling
      ];
  
  // Override the default empty claims with authenticated user.
  // Always mock /user/claims — the real backend requires IAP auth for this endpoint.
  await page.unroute(url => url.toString().includes('/user/claims'));
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    const roleDesc = restrictedUser ? restrictedUser.roles.join(',') : 'Administrator';
    authLog(`[API Mock] Intercepted: /user/claims (user=${testUserEmail}, roles=${roleDesc})`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(claims),
    });
  });
  
  // Step 3b: QA-039 FIX - Override permission mocks for restricted users
  // setupAPIMocks() returns full admin permissions for all endpoints.
  // For restricted users, override the catch-all permission check to deny create/edit/delete.
  if (restrictedUser) {
    authLog(`[Auth] Overriding permission mocks for restricted user: ${testUserEmail}`);
    
    // Override /api/permissions/check/* — route-level permission checks
    // Admin routes (entity-manager, user-management, translations) must return hasAccess: false for restricted users
    const adminBlockedPaths = [
      'admin/entity-manager',
      'admin/user-management',
      'admin/translations',
      'admin/ai-prompt-management',
      'admin/entity-artifacts',
      'admin/bulk-entity-artifacts',
    ];
    await page.route(url => url.toString().includes('/api/permissions/check/'), async (route) => {
      const requestUrl = route.request().url();
      const isAdminRoute = adminBlockedPaths.some(p => requestUrl.includes(p));
      const hasAccess = !isAdminRoute;
      authLog(`[API Mock] Intercepted: /api/permissions/check/ (RESTRICTED for ${testUserEmail}, adminRoute=${isAdminRoute}, hasAccess=${hasAccess})`);
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          hasAccess,
          route: requestUrl,
          entity: 'Contact',
          permissions: {
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
          }
        }),
      });
    });

    // Override /api/{entity}/{id}/permissions — entity-level permission checks
    // Collaborator: can edit content but NOT workflow actions (Submit, Cancel, Reopen)
    const isCollaborator = testUserEmail === 'collaborator@example.com';
    await page.route(url => /\/api\/\w+\/\d+\/permissions/.test(url.toString()), async (route) => {
      authLog(`[API Mock] Intercepted: entity permissions (RESTRICTED for ${testUserEmail})`);
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          canView: true,
          canEdit: isCollaborator, // Collaborator can edit content; other restricted users cannot
          canDelete: false,
          canSubmit: false,
          canApprove: false,
          canActivate: false,
          canCancel: false,
        }),
      });
    });
  }

  // Step 4: Set authentication cookies for both localhost and 127.0.0.1
  await page.context().addCookies([
    {
      name: 'dev-user-email',
      value: testUserEmail,
      domain: 'localhost',
      path: '/',
      httpOnly: false,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'DevIAPAuth',
      value: testUserEmail,
      domain: 'localhost',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'dev-user-email',
      value: testUserEmail,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: false,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'DevIAPAuth',
      value: testUserEmail,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    }
  ]);
  
  // Step 4b: Prevent Driver.js welcome tour from blocking clicks (set before navigation)
  await page.addInitScript(() => {
    const state = {
      hasSeenWelcome: true,
      hasCompletedHomepageTour: true,
      completedTours: ['homepage-tour'],
      firstVisitDate: new Date().toISOString(),
    };
    try {
      localStorage.setItem('unops-welcome-tour-state', JSON.stringify(state));
    } catch (_) {}
  });

  // Step 5: Navigate to target page with cookies and mocks already set
  const baseURL = 'http://localhost:4200';
  // Strip legacy hash prefix if present — app uses PathLocationStrategy (path-based routing)
  let cleanTargetUrl = targetUrl;
  if (cleanTargetUrl.startsWith('/#/')) {
    cleanTargetUrl = cleanTargetUrl.substring(2);
  } else if (cleanTargetUrl.startsWith('#/')) {
    cleanTargetUrl = `/${cleanTargetUrl.substring(2)}`;
  }
  const fullUrl = cleanTargetUrl.startsWith('http') ? cleanTargetUrl : `${baseURL}${cleanTargetUrl}`;
  authLog(`[Auth] Navigating to ${fullUrl} with mocks and cookies set...`);
  await page.goto(fullUrl);
  
  // Step 6: Wait for page to load (use 'load' not 'networkidle' for faster tests)
  await page.waitForLoadState('load', { timeout: 15000 });
  
  // Step 7: Give Angular time to initialize routing
  await page.waitForTimeout(2000);

  // Step 8: Dismiss overlays that block clicks (Driver.js tour, toast messages)
  authLog('[Auth] Checking for overlays that block clicks...');
  for (let attempt = 1; attempt <= 5; attempt++) {
    await page.waitForTimeout(500);
    const driverOverlay = page.locator('.driver-overlay');
    const driverVisible = await driverOverlay.isVisible({ timeout: 300 }).catch(() => false);
    if (driverVisible) {
      authLog(`[Auth] Dismissing Driver.js overlay (attempt ${attempt})...`);
      await page.locator('.driver-popover-close-btn').click({ timeout: 2000 }).catch(() => {});
      await page.waitForTimeout(800);
    } else {
      // Also dismiss toast if it's blocking (e.g. from previous operations)
      const toast = page.locator('.p-toast-message');
      if (await toast.isVisible({ timeout: 200 }).catch(() => false)) {
        await page.locator('.p-toast-message-close-icon').click({ timeout: 1000 }).catch(() => {});
        await page.waitForTimeout(300);
      }
      if (!driverVisible) break;
    }
  }

  authLog('[Auth] authenticateWithRealBackend complete');
}

/**
 * @description Check if current browser is webkit (Safari)
 * Webkit has different timing characteristics and needs special handling
 * @param page - Playwright page object
 * @returns True if browser is webkit, false otherwise
 */
function isWebkitBrowser(page: Page): boolean {
  try {
    return page.context().browser()?.browserType().name() === 'webkit';
  } catch {
    return false;
  }
}

/**
 * Login with test credentials
 * @param page - Playwright page object
 * @param email - Optional email override
 * @param password - Optional password override
 */
export async function login(
  page: Page,
  email?: string,
  password?: string
): Promise<void> {
  const credentials = getTestCredentials();
  const userEmail = email || credentials.email;
  const userPassword = password || credentials.password;
  
  // Setup API mocks BEFORE navigation
  authLog('[Auth] Setting up API mocks...');
  await setupAPIMocks(page);
  
  // Webkit: Give extra time for route handlers to be registered
  const webkit = isWebkitBrowser(page);
  if (webkit) {
    await page.waitForTimeout(500);
    authLog('[Auth] API mocks ready (webkit extra wait applied)');
  }
  
  // QA-041 FIX: Only attach event listeners when debugging to prevent memory accumulation.
  // Previously these were attached for every login() call, generating thousands of log entries
  // and accumulating event handlers across tests within the same worker process.
  if (DEBUG_AUTH) {
    page.on('console', msg => {
      if (msg.type() === 'error') {
        console.error('Browser console error:', msg.text());
      }
    });
    
    page.on('pageerror', error => {
      console.error('Page error:', error.message);
    });
    
    page.on('crash', () => {
      console.error('Page crashed!');
    });

    page.on('request', request => {
      const url = request.url();
      const method = request.method();
      if (url.includes('/api/') || url.includes('/user/')) {
        console.log(`[Request] ${method} ${url}`);
      }
    });
  }
  
  // Navigate to login page
  // Use baseURL from Playwright config or construct absolute URL
  const baseURL = (page.context() as any)._options?.baseURL || 'http://localhost:4200';
  const loginUrl = `${baseURL}/login`;
  
  authLog(`Navigating to ${loginUrl}...`);
  if (webkit) {
    authLog('[Auth] Webkit browser detected - using optimized navigation strategy');
  }
  
  try {
    await page.goto(loginUrl, { 
      // Webkit: wait for DOM instead of full load (faster, more reliable)
      // Other browsers: wait for full load event
      waitUntil: webkit ? 'domcontentloaded' : 'load',
      // Webkit: use 2-minute timeout; Others: use 1-minute timeout
      timeout: webkit ? 120000 : 60000
    });
    authLog('Navigation to /login complete');
    
    // Additional stabilization for webkit
    if (webkit) {
      authLog('[Auth] Webkit - adding stabilization waits');
      // Give webkit extra time to stabilize after DOM load
      await page.waitForTimeout(2000);
      // Wait for network to be idle
      await page.waitForLoadState('networkidle', { timeout: 30000 }).catch(() => {
        authLog('[Auth] Network idle timeout (non-critical for webkit)');
      });
      authLog('[Auth] Webkit stabilization complete');
    }
  } catch (navError: any) {
    console.error('Navigation failed:', navError.message);
    throw new Error(`Failed to navigate to login page: ${navError.message}`);
  }
  
  // Give Angular a moment to bootstrap
  await page.waitForTimeout(2000);
  
  // Webkit: Check Angular is fully ready before proceeding
  await waitForAngularReady(page);
  
  // Wait for Angular to bootstrap - look for any Angular-rendered content
  // The login component should render one of these elements
  try {
    await page.waitForSelector(
      '[data-testid="auth-checking-container"], [data-testid="username-input"], [data-testid="iap-authenticated-container"], .login-container',
      { timeout: 30000 }
    );
  } catch (error) {
    // Defensive error logging - page might be closed
    try {
      console.error('Angular components not rendering.');
      console.error('Current URL:', await page.url().catch(() => 'Page closed'));
      console.error('Page title:', await page.title().catch(() => 'Page closed'));
      await page.screenshot({ path: path.resolve(__dirname, '..', 'test-results', 'login-failed.png') }).catch(() => {});
    } catch (logError) {
      console.error('Failed to log debug info (page may be closed)');
    }
    throw new Error('Angular login component failed to render within 30 seconds. The page may have crashed or navigation failed.');
  }
  
  // Now wait for auth check to complete if it's showing
  const isCheckingAuth = await page.locator('[data-testid="auth-checking-container"]').isVisible().catch(() => false);
  if (isCheckingAuth) {
    authLog('Waiting for authentication check to complete...');
    await page.waitForSelector('[data-testid="auth-checking-container"]', {
      state: 'hidden',
      timeout: getTimeout('long')
    });
  }
  
  // Check if already authenticated with IAP
  const isIapAuth = await page.locator('[data-testid="iap-authenticated-container"]').isVisible().catch(() => false);
  if (isIapAuth) {
    authLog('Already authenticated with IAP, waiting for redirect...');
    await page.waitForURL(/\/home|\/dashboard/, { timeout: getTimeout('default') });
    return;
  }
  
  // Wait for login form to be visible
  await page.waitForSelector('[data-testid="username-input"]', {
    state: 'visible',
    timeout: getTimeout('default')
  });
  
  // Fill credentials
  const usernameInput = page.locator('[data-testid="username-input"]');
  await usernameInput.fill(userEmail);
  await page.locator('[data-testid="password-input"] input').fill(userPassword);
  
  // Before clicking login, update the /user/claims mock to return authenticated user
  // This simulates successful authentication
  authLog('[Auth] Updating /user/claims mock to authenticated state...');
  await page.unroute(url => url.toString().includes('/user/claims')); // Remove unauthenticated mock
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    authLog('[API Mock] Intercepted: /user/claims (authenticated after login)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { type: 'email', value: userEmail },
        { type: 'name', value: 'Test User' },
        { type: 'role', value: 'Administrator' },
        { type: 'sub', value: '12345' },
      ]),
    });
  });
  
  // Click login button and wait for navigation
  authLog('Clicking login button...');
  await Promise.all([
    page.waitForURL(url => {
      const path = new URL(url).pathname;
      // Accept root, home, or dashboard as valid redirect targets
      const validPaths = ['/', '/home', '/dashboard'];
      const isValid = validPaths.some(validPath => path === validPath || path.startsWith(validPath + '/'));
      authLog(`[Auth] Navigation check: ${path} - Valid: ${isValid}`);
      return isValid;
    }, { timeout: getTimeout('long') }),
    page.locator('[data-testid="login-button"]').click(),
  ]);
  
  authLog('Login successful! Redirected to: ' + page.url());
  
  // Wait for any loading overlays to disappear after login
  authLog('[Auth] Waiting for page to be ready after login...');
  await waitForPageReady(page);
}

/**
 * Login and navigate to specific page
 * @param page - Playwright page object
 * @param targetUrl - URL to navigate to after login
 * @param email - Optional email override
 * @param password - Optional password override
 */
export async function loginAndNavigate(
  page: Page,
  targetUrl: string,
  email?: string,
  password?: string
): Promise<void> {
  await login(page, email, password);
  
  // Dismiss welcome tour dialog if present (appears after login on dashboard)
  // The dialog may take a moment to render, so we give it extra time
  // This dialog is modal and blocks navigation to other pages
  authLog('[Auth] Checking for welcome tour dialog...');
  
  // Wait a bit for the dialog to appear (it renders after dashboard loads)
  await page.waitForTimeout(1000);
  
  const welcomeDialog = page.locator('[role="dialog"]').filter({ 
    hasText: 'Welcome to UNOPS Opportunity+' 
  });
  
  // Check multiple times in case dialog is slow to render
  let dialogDismissed = false;
  for (let attempt = 1; attempt <= 3; attempt++) {
    const isDialogVisible = await welcomeDialog
      .isVisible({ timeout: 1000 })
      .catch(() => false);
    
    if (isDialogVisible) {
      authLog(`[Auth] Dismissing welcome tour dialog (attempt ${attempt})...`);
      try {
        // Click the Close button (X) to dismiss the dialog
        const closeButton = page.locator('[role="dialog"] button').first();
        await closeButton.click({ timeout: 2000 });
        // Wait for dialog close animation to complete
        await page.waitForTimeout(500);
        
        // Verify dialog is actually gone
        const stillVisible = await welcomeDialog.isVisible({ timeout: 500 }).catch(() => false);
        if (!stillVisible) {
          authLog('[Auth] Welcome dialog dismissed successfully');
          dialogDismissed = true;
          break;
        }
      } catch (error) {
        authLog(`[Auth] Failed to dismiss dialog on attempt ${attempt}`);
      }
    } else if (attempt === 1) {
      authLog('[Auth] No welcome dialog found, continuing...');
      break;
    }
    
    // Wait before retry
    if (attempt < 3 && !dialogDismissed) {
      await page.waitForTimeout(500);
    }
  }
  
  // Construct absolute URL if needed
  const baseURL = (page.context() as any)._options?.baseURL || 'http://localhost:4200';
  // Strip legacy hash prefix if present — app uses PathLocationStrategy (path-based routing)
  let cleanUrl = targetUrl;
  if (cleanUrl.startsWith('/#/')) {
    cleanUrl = cleanUrl.substring(2);
  } else if (cleanUrl.startsWith('#/')) {
    cleanUrl = `/${cleanUrl.substring(2)}`;
  }
  const fullUrl = cleanUrl.startsWith('http') ? cleanUrl : `${baseURL}${cleanUrl}`;
  
  authLog(`[Auth] Navigating to ${fullUrl}...`);
  await page.goto(fullUrl);
  await waitForPageReady(page);
  authLog(`[Auth] Navigation to ${fullUrl} complete`);
}

/**
 * Check if user is logged in
 * @param page - Playwright page object
 * @returns True if user is logged in
 */
export async function isLoggedIn(page: Page): Promise<boolean> {
  const currentUrl = page.url();
  return !currentUrl.includes('/login');
}

/**
 * Logout from application
 * @param page - Playwright page object
 */
export async function logout(page: Page): Promise<void> {
  // Look for user menu or logout button
  const userMenuButton = page.locator('[data-testid="user-menu-button"]');
  
  if (await userMenuButton.isVisible()) {
    await userMenuButton.click();
    await page.locator('[data-testid="logout-button"]').click();
    await page.waitForURL(/\/login/, { timeout: getTimeout('short') });
  }
}

/**
 * Verify login page elements are visible
 * @param page - Playwright page object
 */
export async function verifyLoginPageElements(page: Page): Promise<void> {
  const usernameInput = page.locator('[data-testid="username-input"]');
  const passwordInput = page.locator('[data-testid="password-input"]');
  const loginButton = page.locator('[data-testid="login-button"]');
  
  await usernameInput.waitFor({ state: 'visible', timeout: getTimeout('short') });
  await passwordInput.waitFor({ state: 'visible', timeout: getTimeout('short') });
  await loginButton.waitFor({ state: 'visible', timeout: getTimeout('short') });
}
