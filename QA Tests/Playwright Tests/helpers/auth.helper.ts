/**
 * @fileoverview Authentication Helper
 * Provides reusable authentication functions for E2E tests
 */

import { Page } from '@playwright/test';
import { getTestCredentials, getTimeout } from './test-config';
import { setupAPIMocks } from './api-mocks.helper';
import { waitForPageReady, waitForAngularReady } from './wait.helper';

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
 * @param page - Playwright page object
 * @param targetUrl - URL to navigate to after authentication
 * @param testUserEmail - Email of test user (default: test@playwright.local)
 */
export async function authenticateWithRealBackend(
  page: Page,
  targetUrl: string,
  testUserEmail: string = 'test@playwright.local'
): Promise<void> {
  // Step 1: Clear all cookies
  await page.context().clearCookies();
  
  // Step 2: Set authentication cookies BEFORE first navigation
  await page.context().addCookies([
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
  
  // Step 3: Navigate to target page with cookies already set
  const baseURL = 'http://127.0.0.1:4200';
  const fullUrl = targetUrl.startsWith('http') ? targetUrl : `${baseURL}${targetUrl}`;
  await page.goto(fullUrl);
  
  // Step 4: Wait for page to load (use 'load' not 'networkidle' for faster tests)
  await page.waitForLoadState('load', { timeout: 15000 });
  
  // Step 5: Give Angular time to initialize routing
  await page.waitForTimeout(2000);
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
  console.log('[Auth] Setting up API mocks...');
  await setupAPIMocks(page);
  
  // Webkit: Give extra time for route handlers to be registered
  const webkit = isWebkitBrowser(page);
  if (webkit) {
    await page.waitForTimeout(500);
    console.log('[Auth] API mocks ready (webkit extra wait applied)');
  }
  
  // Listen for console errors and page crashes
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

  // Log all network requests for debugging
  page.on('request', request => {
    const url = request.url();
    const method = request.method();
    if (url.includes('/api/') || url.includes('/user/')) {
      console.log(`[Request] ${method} ${url}`);
    }
  });
  
  // Navigate to login page
  // Use baseURL from Playwright config or construct absolute URL
  const baseURL = (page.context() as any)._options?.baseURL || 'http://127.0.0.1:4200';
  const loginUrl = `${baseURL}/login`;
  
  console.log(`Navigating to ${loginUrl}...`);
  if (webkit) {
    console.log('[Auth] Webkit browser detected - using optimized navigation strategy');
  }
  
  try {
    await page.goto(loginUrl, { 
      // Webkit: wait for DOM instead of full load (faster, more reliable)
      // Other browsers: wait for full load event
      waitUntil: webkit ? 'domcontentloaded' : 'load',
      // Webkit: use 2-minute timeout; Others: use 1-minute timeout
      timeout: webkit ? 120000 : 60000
    });
    console.log('Navigation to /login complete');
    
    // Additional stabilization for webkit
    if (webkit) {
      console.log('[Auth] Webkit - adding stabilization waits');
      // Give webkit extra time to stabilize after DOM load
      await page.waitForTimeout(2000);
      // Wait for network to be idle
      await page.waitForLoadState('networkidle', { timeout: 30000 }).catch(() => {
        console.log('[Auth] Network idle timeout (non-critical for webkit)');
      });
      console.log('[Auth] Webkit stabilization complete');
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
      await page.screenshot({ path: 'test-results/login-failed.png' }).catch(() => {});
    } catch (logError) {
      console.error('Failed to log debug info (page may be closed)');
    }
    throw new Error('Angular login component failed to render within 30 seconds. The page may have crashed or navigation failed.');
  }
  
  // Now wait for auth check to complete if it's showing
  const isCheckingAuth = await page.locator('[data-testid="auth-checking-container"]').isVisible().catch(() => false);
  if (isCheckingAuth) {
    console.log('Waiting for authentication check to complete...');
    await page.waitForSelector('[data-testid="auth-checking-container"]', {
      state: 'hidden',
      timeout: getTimeout('long')
    });
  }
  
  // Check if already authenticated with IAP
  const isIapAuth = await page.locator('[data-testid="iap-authenticated-container"]').isVisible().catch(() => false);
  if (isIapAuth) {
    console.log('Already authenticated with IAP, waiting for redirect...');
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
  console.log('[Auth] Updating /user/claims mock to authenticated state...');
  await page.unroute(url => url.toString().includes('/user/claims')); // Remove unauthenticated mock
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    console.log('[API Mock] Intercepted: /user/claims (authenticated after login)');
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
  console.log('Clicking login button...');
  await Promise.all([
    page.waitForURL(url => {
      const path = new URL(url).pathname;
      // Accept root, home, or dashboard as valid redirect targets
      const validPaths = ['/', '/home', '/dashboard'];
      const isValid = validPaths.some(validPath => path === validPath || path.startsWith(validPath + '/'));
      console.log(`[Auth] Navigation check: ${path} - Valid: ${isValid}`);
      return isValid;
    }, { timeout: getTimeout('long') }),
    page.locator('[data-testid="login-button"]').click(),
  ]);
  
  console.log('Login successful! Redirected to:', page.url());
  
  // Wait for any loading overlays to disappear after login
  console.log('[Auth] Waiting for page to be ready after login...');
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
  console.log('[Auth] Checking for welcome tour dialog...');
  
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
      console.log(`[Auth] Dismissing welcome tour dialog (attempt ${attempt})...`);
      try {
        // Click the Close button (X) to dismiss the dialog
        const closeButton = page.locator('[role="dialog"] button').first();
        await closeButton.click({ timeout: 2000 });
        // Wait for dialog close animation to complete
        await page.waitForTimeout(500);
        
        // Verify dialog is actually gone
        const stillVisible = await welcomeDialog.isVisible({ timeout: 500 }).catch(() => false);
        if (!stillVisible) {
          console.log('[Auth] Welcome dialog dismissed successfully');
          dialogDismissed = true;
          break;
        }
      } catch (error) {
        console.log(`[Auth] Failed to dismiss dialog on attempt ${attempt}`);
      }
    } else if (attempt === 1) {
      console.log('[Auth] No welcome dialog found, continuing...');
      break;
    }
    
    // Wait before retry
    if (attempt < 3 && !dialogDismissed) {
      await page.waitForTimeout(500);
    }
  }
  
  // Construct absolute URL if needed
  const baseURL = (page.context() as any)._options?.baseURL || 'http://127.0.0.1:4200';
  const fullUrl = targetUrl.startsWith('http') ? targetUrl : `${baseURL}${targetUrl}`;
  
  console.log(`[Auth] Navigating to ${fullUrl}...`);
  await page.goto(fullUrl);
  await waitForPageReady(page);
  console.log(`[Auth] Navigation to ${fullUrl} complete`);
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
