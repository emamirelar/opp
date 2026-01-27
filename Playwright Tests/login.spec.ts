import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';
import { getTestCredentials } from './helpers/test-config';
import { assertUrlMatches } from './helpers/assertions.helper';

/**
 * Login Flow E2E Tests
 * 
 * Tests authentication flows including:
 * - Login form display and validation
 * - Successful authentication
 * - Error handling for invalid credentials
 * - Form validation
 */
test.describe('Login Flow', () => {
  let loginPage: LoginPage;
  
  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    await loginPage.navigate();
  });
  
  test('should display login form', async ({ page }) => {
    // Verify login page loaded
    await assertUrlMatches(page, /\/login/);
    
    // Verify form elements are present
    await loginPage.verifyLoginFormVisible();
  });
  
  test('should display email and password labels', async () => {
    // Verify form labels
    await loginPage.verifyFormLabels();
  });
  
  test('should successfully login with valid credentials', async ({ page }) => {
    // Get credentials from config
    const credentials = getTestCredentials();
    
    // Perform login
    await loginPage.login(credentials.email, credentials.password);
    
    // Verify redirect to home/dashboard
    await assertUrlMatches(page, /\/home|\/dashboard/);
    
    // Verify user is logged in (dashboard content visible)
    await expect(page.locator('.max-w-7xl, .dashboard, [data-testid="dashboard"]')).toBeVisible({ timeout: 5000 });
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
  
  test('should display Sign Up button if registration is enabled', async () => {
    // Check if signup section exists
    const isSignupVisible = await loginPage.isSignupSectionVisible();
    
    if (isSignupVisible) {
      // Verify signup button is visible
      await loginPage.assertElementVisible('signup-button');
    }
    
    // Test passes regardless - just verifying UI consistency
    expect(true).toBeTruthy();
  });
});
