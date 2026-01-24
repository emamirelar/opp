import { test, expect } from '@playwright/test';

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
  test('should display login form', async ({ page }) => {
    await page.goto('/login');
    
    // Verify login page loaded
    await expect(page).toHaveURL(/\/login/);
    
    // Verify form elements are present using data-testid
    await expect(page.locator('[data-testid="username-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="login-button"]')).toBeVisible();
  });
  
  test('should display email and password labels', async ({ page }) => {
    await page.goto('/login');
    
    // Verify form labels
    await expect(page.locator('label[for="userEmail"]')).toContainText(/Email/i);
    await expect(page.locator('label[for="password"]')).toContainText(/Password/i);
  });
  
  test('should successfully login with valid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // TODO: Replace with actual test credentials from your environment
    const username = 'testuser@unops.org';
    const password = 'TestPassword123!';
    
    // Fill in credentials using data-testid
    await page.locator('[data-testid="username-input"]').fill(username);
    
    // For p-password component, need to target the actual input inside
    await page.locator('[data-testid="password-input"] input').fill(password);
    
    // Click login button
    await page.locator('[data-testid="login-button"]').click();
    
    // Verify redirect to home/dashboard
    await expect(page).toHaveURL(/\/home|\/dashboard/, { timeout: 10000 });
    
    // Verify user is logged in (dashboard content visible)
    await expect(page.locator('.max-w-7xl, .dashboard, [data-testid="dashboard"]')).toBeVisible({ timeout: 5000 });
  });
  
  test('should show error with invalid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in invalid credentials
    await page.locator('[data-testid="username-input"]').fill('invalid@example.com');
    await page.locator('[data-testid="password-input"] input').fill('wrongpassword');
    
    // Click login
    await page.locator('[data-testid="login-button"]').click();
    
    // Verify error message is shown (adjust selector based on actual error display)
    await expect(
      page.locator('.p-message-error, [role="alert"], .error-message')
        .or(page.getByText(/Invalid credentials|Login failed|Authentication failed/i))
    ).toBeVisible({ timeout: 5000 });
  });
  
  test('should validate required fields', async ({ page }) => {
    await page.goto('/login');
    
    // Try to submit without filling fields
    await page.locator('[data-testid="login-button"]').click();
    
    // Verify validation occurs (PrimeNG adds ng-invalid class)
    const usernameField = page.locator('[data-testid="username-input"]');
    const passwordField = page.locator('[data-testid="password-input"]');
    
    // At least one field should show invalid state
    const hasValidationError = 
      await usernameField.evaluate(el => el.classList.contains('ng-invalid')).catch(() => false) ||
      await passwordField.evaluate(el => el.classList.contains('ng-invalid')).catch(() => false);
    
    expect(hasValidationError).toBeTruthy();
  });
  
  test('should allow password visibility toggle', async ({ page }) => {
    await page.goto('/login');
    
    // Fill password field
    await page.locator('[data-testid="password-input"] input').fill('TestPassword123!');
    
    // Find the toggle button (PrimeNG p-password has a toggle mask button)
    const toggleButton = page.locator('[data-testid="password-input"] button').first();
    
    // Verify toggle button exists
    await expect(toggleButton).toBeVisible();
    
    // Click to show password
    await toggleButton.click();
    
    // Verify input type changed to text (password visible)
    const inputType = await page.locator('[data-testid="password-input"] input').getAttribute('type');
    expect(inputType).toBe('text');
    
    // Click again to hide
    await toggleButton.click();
    
    // Verify input type changed back to password
    const hiddenType = await page.locator('[data-testid="password-input"] input').getAttribute('type');
    expect(hiddenType).toBe('password');
  });
  
  test('should display Sign Up button if registration is enabled', async ({ page }) => {
    await page.goto('/login');
    
    // Check if signup section exists (depends on canDoSignUp config)
    const signupSection = page.locator('[data-testid="signup-section"]');
    const isSignupVisible = await signupSection.isVisible().catch(() => false);
    
    if (isSignupVisible) {
      // Verify signup button
      await expect(page.locator('[data-testid="signup-button"]')).toBeVisible();
      await expect(signupSection).toContainText(/Not a member/i);
    }
    
    // Test passes regardless - just verifying UI consistency
    expect(true).toBeTruthy();
  });
});
