/**
 * @fileoverview oUP Integration Helper — Type Definitions and Mock Utilities
 *
 * Provides type definitions and helper functions for oUP integration testing.
 * All spec files now use Playwright route interception with mock data instead of
 * connecting to the real oUP environment. The real-connection functions below are
 * retained for reference but are NOT called by any active spec file.
 *
 * @author QA Team
 * @since 2026-02-02
 */

import { Page, BrowserContext, expect } from '@playwright/test';

/**
 * oUP Integration Configuration
 * All values sourced from environment variables for security
 */
export const OUP_CONFIG = {
  baseUrl: process.env.OUP_BASE_URL || 'https://projects-test.unops.org',
  apiUrl: process.env.OUP_API_URL || 'https://projects-test.unops.org/api',
  username: process.env.OUP_USERNAME || '',
  password: process.env.OUP_PASSWORD || '',
  
  // Sync timing
  syncLatencyMinMs: 60000,  // 1 minute minimum
  syncLatencyMaxMs: 300000, // 5 minutes maximum
  pollIntervalMs: 15000,    // Poll every 15 seconds
  
  // Email configuration
  emailHost: process.env.EMAIL_HOST || '',
  emailUsername: process.env.EMAIL_USERNAME || '',
  emailPassword: process.env.EMAIL_PASSWORD || '',
};

/**
 * Check if oUP credentials are configured
 */
export function hasOupCredentials(): boolean {
  return !!(
    process.env.OUP_BASE_URL &&
    process.env.OUP_USERNAME &&
    process.env.OUP_PASSWORD
  );
}

/**
 * Check if email credentials are configured
 */
export function hasEmailCredentials(): boolean {
  return !!(
    process.env.EMAIL_HOST &&
    process.env.EMAIL_USERNAME &&
    process.env.EMAIL_PASSWORD
  );
}

/**
 * Authenticate with oUP test environment
 * @param page - Playwright page
 * @param targetUrl - URL to navigate after auth (default: dashboard)
 */
export async function authenticateWithOup(
  page: Page,
  targetUrl: string = '/dashboard'
): Promise<void> {
  if (!hasOupCredentials()) {
    throw new Error('oUP credentials not configured. Set OUP_BASE_URL, OUP_USERNAME, OUP_PASSWORD environment variables.');
  }
  
  console.log('[oUP Auth] Navigating to oUP login...');
  await page.goto(`${OUP_CONFIG.baseUrl}/login`);
  
  // Wait for login form
  await page.waitForSelector('input[name="username"], input[type="email"]', { timeout: 30000 });
  
  // Fill credentials
  const usernameInput = page.locator('input[name="username"], input[type="email"]').first();
  const passwordInput = page.locator('input[name="password"], input[type="password"]').first();
  
  await usernameInput.fill(OUP_CONFIG.username);
  await passwordInput.fill(OUP_CONFIG.password);
  
  // Submit login
  const loginButton = page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Sign In")').first();
  await loginButton.click();
  
  // Wait for navigation after login
  await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 30000 });
  console.log('[oUP Auth] Login successful');
  
  // Navigate to target URL if not dashboard
  if (targetUrl && targetUrl !== '/dashboard') {
    await page.goto(`${OUP_CONFIG.baseUrl}${targetUrl}`);
  }
}

/**
 * Navigate to engagement in oUP
 * @param page - Playwright page
 * @param engagementNumber - Base engagement number (e.g., "UENB-12345")
 */
export async function navigateToEngagement(
  page: Page,
  engagementNumber: string
): Promise<void> {
  const engagementUrl = `${OUP_CONFIG.baseUrl}/?route=uenb/${engagementNumber}/engagement/overview`;
  console.log(`[oUP] Navigating to engagement: ${engagementNumber}`);
  await page.goto(engagementUrl);
  await page.waitForLoadState('networkidle', { timeout: 30000 });
}

/**
 * Check if engagement exists in oUP via API
 * @param opportunityId - Opportunity+ ID
 * @returns Engagement details if found, null if not
 */
export async function findEngagementByOpportunityId(
  page: Page,
  opportunityId: string | number
): Promise<EngagementInfo | null> {
  if (!hasOupCredentials()) {
    console.warn('[oUP API] Cannot check engagement - credentials not configured');
    return null;
  }
  
  try {
    // Query oUP API for engagement linked to opportunity
    const response = await page.request.get(
      `${OUP_CONFIG.apiUrl}/engagements?opportunityPlusId=${opportunityId}`,
      {
        headers: {
          'Accept': 'application/json',
        },
      }
    );
    
    if (response.ok()) {
      const data = await response.json();
      if (data && data.length > 0) {
        return data[0] as EngagementInfo;
      }
    }
    
    return null;
  } catch (error) {
    console.error('[oUP API] Error checking engagement:', error);
    return null;
  }
}

/**
 * Wait for sync from Opportunity+ to oUP
 * Polls oUP API until engagement is created/updated
 * 
 * @param page - Playwright page
 * @param opportunityId - Opportunity+ ID
 * @param timeoutMs - Maximum wait time (default: 5 minutes)
 * @returns Engagement info if found, null if timeout
 */
export async function waitForEngagementSync(
  page: Page,
  opportunityId: string | number,
  timeoutMs: number = OUP_CONFIG.syncLatencyMaxMs
): Promise<EngagementInfo | null> {
  console.log(`[oUP Sync] Waiting for engagement sync (opportunity: ${opportunityId})...`);
  
  const startTime = Date.now();
  let lastCheck = 0;
  
  while (Date.now() - startTime < timeoutMs) {
    const elapsedSec = Math.round((Date.now() - startTime) / 1000);
    
    // Log progress every 30 seconds
    if (elapsedSec - lastCheck >= 30) {
      console.log(`[oUP Sync] Still waiting... (${elapsedSec}s elapsed)`);
      lastCheck = elapsedSec;
    }
    
    const engagement = await findEngagementByOpportunityId(page, opportunityId);
    
    if (engagement) {
      console.log(`[oUP Sync] Engagement found: ${engagement.engagementNumber} (took ${elapsedSec}s)`);
      return engagement;
    }
    
    // Wait before next poll
    await new Promise(resolve => setTimeout(resolve, OUP_CONFIG.pollIntervalMs));
  }
  
  console.warn(`[oUP Sync] Timeout after ${timeoutMs / 1000}s - engagement not found`);
  return null;
}

/**
 * Verify engagement field mapping in oUP
 * @param page - Playwright page
 * @param engagementNumber - Base engagement number
 * @param expectedFields - Expected field values
 */
export async function verifyEngagementFields(
  page: Page,
  engagementNumber: string,
  expectedFields: Partial<EngagementFieldMapping>
): Promise<FieldValidationResult> {
  await navigateToEngagement(page, engagementNumber);
  await page.waitForTimeout(2000);
  
  const results: FieldValidationResult = {
    passed: true,
    fields: {},
  };
  
  // Check each expected field
  for (const [fieldName, expectedValue] of Object.entries(expectedFields)) {
    if (expectedValue === undefined) continue;
    
    const selector = getFieldSelector(fieldName);
    const actualValue = await page.locator(selector).textContent().catch(() => null);
    
    const matches = actualValue?.includes(String(expectedValue)) || false;
    results.fields[fieldName] = {
      expected: String(expectedValue),
      actual: actualValue || 'NOT FOUND',
      passed: matches,
    };
    
    if (!matches) {
      results.passed = false;
      console.error(`[Field Validation] ${fieldName}: Expected "${expectedValue}", got "${actualValue}"`);
    }
  }
  
  return results;
}

/**
 * Get field selector for oUP engagement page
 */
function getFieldSelector(fieldName: string): string {
  const selectors: Record<string, string> = {
    engagementName: '[data-field="engagement-name"], .engagement-name',
    engagementDescription: '[data-field="engagement-description"], .engagement-description',
    estimatedSigningDate: '[data-field="estimated-signing-date"]',
    implementationStartDate: '[data-field="implementation-start-date"]',
    implementationEndDate: '[data-field="implementation-end-date"]',
    organisationalUnit: '[data-field="organisational-unit"]',
    businessDeveloper: '[data-field="business-developer"]',
    projectExecutive: '[data-field="project-executive"]',
    doA2: '[data-field="doa2"]',
    doA3: '[data-field="doa3"]',
    estimatedAmount: '[data-field="estimated-amount"]',
    currency: '[data-field="currency"]',
    stage: '[data-field="stage"], .engagement-stage',
    countries: '[data-field="countries"]',
    fundingPartners: '[data-field="funding-partners"]',
    clientPartners: '[data-field="client-partners"]',
    sdgContributions: '[data-field="sdg-contributions"]',
    projectCategory: '[data-field="project-category"]',
  };
  
  return selectors[fieldName] || `[data-field="${fieldName}"]`;
}

/**
 * Verify high-risk survey responses in oUP
 * @param page - Playwright page
 * @param engagementNumber - Base engagement number
 * @param expectedRisks - Expected high-risk survey question IDs answered "Yes"
 */
export async function verifyHighRiskSurvey(
  page: Page,
  engagementNumber: string,
  expectedRisks: string[]
): Promise<RiskValidationResult> {
  // Navigate to EAC Survey section
  await navigateToEngagement(page, engagementNumber);
  await page.click('[data-section="eac-survey"], a:has-text("High Risk Checklist")');
  await page.waitForTimeout(2000);
  
  const results: RiskValidationResult = {
    passed: true,
    risks: {},
  };
  
  for (const riskId of expectedRisks) {
    const questionSelector = `[data-question-id="${riskId}"]`;
    const answerSelector = `${questionSelector} [data-answer="Yes"], ${questionSelector}:has-text("Yes")`;
    
    const isYes = await page.locator(answerSelector).isVisible().catch(() => false);
    
    results.risks[riskId] = {
      expectedAnswer: 'Yes',
      actualAnswer: isYes ? 'Yes' : 'No',
      passed: isYes,
    };
    
    if (!isYes) {
      results.passed = false;
      console.error(`[Risk Validation] ${riskId}: Expected "Yes", got "No"`);
    }
  }
  
  return results;
}

/**
 * Verify risk register entries in oUP
 * @param page - Playwright page
 * @param engagementNumber - Base engagement number
 * @param expectedRiskCount - Expected number of risks in register
 */
export async function verifyRiskRegister(
  page: Page,
  engagementNumber: string,
  expectedRiskCount: number
): Promise<boolean> {
  await navigateToEngagement(page, engagementNumber);
  await page.click('[data-section="risk-register"], a:has-text("Risk Register")');
  await page.waitForTimeout(2000);
  
  const riskItems = page.locator('[data-testid="risk-item"], .risk-register-row');
  const actualCount = await riskItems.count();
  
  console.log(`[Risk Register] Expected: ${expectedRiskCount}, Actual: ${actualCount}`);
  
  return actualCount >= expectedRiskCount;
}

// ============================================================================
// TYPE DEFINITIONS
// ============================================================================

export interface EngagementInfo {
  engagementNumber: string;
  opportunityPlusId: string | number;
  stage: string;
  name: string;
  createdDate: string;
  lastModifiedDate: string;
}

export interface EngagementFieldMapping {
  engagementName: string;
  engagementDescription: string;
  estimatedSigningDate: string;
  implementationStartDate: string;
  implementationEndDate: string;
  organisationalUnit: string;
  businessDeveloper: string;
  projectExecutive: string;
  doA2: string;
  doA3: string;
  estimatedAmount: number | string;
  currency: string;
  stage: string;
  countries: string[];
  fundingPartners: string[];
  clientPartners: string[];
  sdgContributions: string[];
  projectCategory: string;
}

export interface FieldValidationResult {
  passed: boolean;
  fields: Record<string, {
    expected: string;
    actual: string;
    passed: boolean;
  }>;
}

export interface RiskValidationResult {
  passed: boolean;
  risks: Record<string, {
    expectedAnswer: string;
    actualAnswer: string;
    passed: boolean;
  }>;
}

// ============================================================================
// EMAIL NOTIFICATION HELPERS (Placeholder - Requires email library)
// ============================================================================

/**
 * Check email inbox for oUP notification
 * @requires Email credentials configured
 */
export async function checkEmailNotification(
  subject: string,
  recipientEmail: string,
  withinMinutes: number = 10
): Promise<EmailNotification | null> {
  if (!hasEmailCredentials()) {
    console.warn('[Email] Cannot check emails - credentials not configured');
    return null;
  }
  
  // TODO: Implement email checking with IMAP/API
  // This would require an email client library like imap-simple or nodemailer
  console.log(`[Email] Would check for email with subject "${subject}" to ${recipientEmail}`);
  
  return null;
}

export interface EmailNotification {
  subject: string;
  from: string;
  to: string[];
  body: string;
  receivedDate: Date;
  links: {
    oupLink?: string;
    opportunityPlusLink?: string;
  };
}
