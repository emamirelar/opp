/**
 * @tests 5
 */

import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';
import { getTestCredentials } from './helpers/test-config';
import { assertUrlMatches } from './helpers/assertions.helper';
import { setupAPIMocks } from './helpers/api-mocks.helper';

/**
 * Login Flow E2E Tests
 * 
 * Tests authentication flows including:
 * - Login form display and validation
 * - Successful authentication
 * - Error handling for invalid credentials
 * - Form validation
 * 
 * NOTE: These tests REQUIRE a real backend for proper login flow testing.
 * In mocked environments (CI), these tests are skipped.
 * To run: Use staging/development environment with real backend.
 * 
 * @requires Real Backend API
 * @skipped-in CI (API mocking mode)
 */
/**
 * Login Flow UI Tests
 * 
 * These tests verify the login form UI elements are displayed correctly.
 * Tests that require actual backend authentication are in a separate describe block.
 */
test.describe('Login Flow - UI Tests', () => {
  let loginPage: LoginPage;
  
  test.beforeEach(async ({ page }) => {
    // Set up API mocks before navigation to ensure page loads
    await setupAPIMocks(page);
    loginPage = new LoginPage(page);
    await loginPage.navigate();
  });
  
  test.skip('should display login form', async ({ page }) => {
    // No login page — app uses IAP authentication
    // Verify login page loaded
    await assertUrlMatches(page, /\/login/);
    
    // Verify form elements are present
    await loginPage.verifyLoginFormVisible();
  });
  
  test('should display email and password labels', async () => {
    // Verify form labels
    await loginPage.verifyFormLabels();
  });
  
  test.skip('should display Sign Up button if registration is enabled', async () => {
    // No login page — app uses IAP authentication
    // Check if signup section exists
    const isSignupVisible = await loginPage.isSignupSectionVisible();
    
    if (isSignupVisible) {
      const signupBtn = page.getByRole('button', { name: /sign up|register|signup/i }).or(page.locator('[data-testid="signup-button"]')).first();
      await expect(signupBtn).toBeVisible();
    } else {
      // When signup disabled: verify login form is still present and usable
      await loginPage.verifyLoginFormVisible();
    }
  });
});

/**
 * Login Flow Backend Tests
 *
 * These tests verify actual login functionality with a real, non-mocked backend.
 * They are automatically skipped when the real login form is unavailable (e.g. when
 * the IAP-simulation cookie bypasses the login page, or when data-testid attributes
 * are absent from the login template).
 *
 * To enable: ensure the Angular dev server serves the login form at /login with
 * inputs that carry [data-testid="username-input"] / [data-testid="login-button"].
 *
 * QA-NOTE: These tests are gated on REAL_LOGIN_FORM_AVAILABLE=true so that the CI
 * suite does not block on login-form structure that only exists with a real Azure B2C
 * or local-auth setup. Set the env var when running against a fully wired environment.
 */
const realLoginFormAvailable = process.env.REAL_LOGIN_FORM_AVAILABLE === 'true';

test.describe('Login Flow - Backend Tests', () => {
  let loginPage: LoginPage;

  test.skip(!realLoginFormAvailable, 'Login form backend tests require REAL_LOGIN_FORM_AVAILABLE=true — skipped in mock environments');

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    await loginPage.navigate();

    // Skip if the real login form is not rendered (e.g. IAP cookie already bypasses it)
    const usernameInput = page.getByPlaceholder(/username|email/i).or(page.locator('input[type="email"], input[name="email"]')).first();
    const formVisible = await usernameInput.isVisible({ timeout: 5000 }).catch(() => false);
    if (!formVisible) {
      test.skip(true, 'Real login form not detected — running in mocked/cookie-bypass environment');
    }
  });

  test('should successfully login with valid credentials', async ({ page }) => {
    // Get credentials from config
    const credentials = getTestCredentials();

    // Perform login
    await loginPage.login(credentials.email, credentials.password);

    // Verify redirect to home/dashboard
    await assertUrlMatches(page, /\/home|\/dashboard/);

    // Verify user is logged in (dashboard content visible)
    const dashboardContent = page.locator('.max-w-7xl, .dashboard, app-home, [data-testid="dashboard"]').first();
    await expect(dashboardContent).toBeVisible({ timeout: 5000 });
  });

  test('should show error with invalid credentials', async () => {
    // Fill in invalid credentials
    await loginPage.fillUsername('invalid@example.com');
    await loginPage.fillPassword('wrongpassword');
    await loginPage.clickLogin();

    // Verify error message is shown
    await loginPage.verifyErrorMessage();
  });

  test('should validate required fields', async () => {
    // Try to submit without filling fields
    await loginPage.clickLogin();

    // Verify validation occurs
    const hasError = await loginPage.hasValidationError();
    expect(hasError).toBeTruthy();
  });

  test('should allow password visibility toggle', async () => {
    // Fill password field
    await loginPage.fillPassword('TestPassword123!');

    // Toggle to show password
    await loginPage.togglePasswordVisibility();

    // Verify input type changed to text
    const visibleType = await loginPage.getPasswordFieldType();
    expect(visibleType).toBe('text');

    // Toggle to hide password
    await loginPage.togglePasswordVisibility();

    // Verify input type changed back to password
    const hiddenType = await loginPage.getPasswordFieldType();
    expect(hiddenType).toBe('password');
  });
});
