/**
 * @fileoverview Simple test to verify login mocking works
 */

import { test, expect } from '@playwright/test';
import { login } from './helpers/auth.helper';
import { waitForLoadingToComplete } from './helpers/wait.helper';

test.describe('Login Mock Verification', () => {
  test('should successfully mock login flow', async ({ page }) => {
    // Enable console logging
    page.on('console', msg => console.log('Browser:', msg.text()));
    
    // Attempt login
    await login(page);
    
    // Verify we're not on login page anymore
    const url = page.url();
    console.log('Final URL:', url);
    expect(url).not.toContain('/login');
    
    // Should be on home, dashboard, or root
    expect(
      url.endsWith('/') || 
      url.includes('/home') || 
      url.includes('/dashboard')
    ).toBeTruthy();
    
    // Verify no loading overlays are blocking the page
    await waitForLoadingToComplete(page);
    console.log('Login test passed! ✅');
  });
});
